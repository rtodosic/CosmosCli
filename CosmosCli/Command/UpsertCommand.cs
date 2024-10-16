// Ignore Spelling: app
// Ignore Spelling: upsert
// Ignore Spelling: upserted

using System.Text;

using Cocona;
using Cocona.Builder;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Command;

public static class UpsertCommand
{
    public static CommandConventionBuilder AddUpsertCommand(this CoconaApp app)
    {
        return app.AddCommand("upsert", async(
            ILogger < Program > logger,
            CommonParameters commandParams,
            UpsertParameters upsertParams,
            ItemRequestParameters itemRequestParams,
            [Argument] string ? jsonItems = null) =>
            {
            var defaultConsoleColor = Console.ForegroundColor;
            try
            {
                Utilities.VerboseWriteLine(commandParams.Verbose, "Reading common params from config file:");
                CommonParameters configParams = CommonParameters.ReadParamsFromConfigFile();
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(configParams));

                Utilities.VerboseWriteLine(commandParams.Verbose, "Reading common params from environment variables:");
                CommonParameters envParams = CommonParameters.ReadParamsFromEnvironment();
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(envParams));

                Utilities.VerboseWriteLine(commandParams.Verbose, "Argument common params:");
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(commandParams));

                commandParams = configParams.Apply(envParams).Apply(commandParams);
                Utilities.VerboseWriteLine(commandParams.Verbose, "Resolved common params:");
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(commandParams));

                jsonItems = Utilities.ReadFromPipeIfNull(jsonItems);

                configParams.ValidateParams();
                upsertParams.ValidateParams();
                if (string.IsNullOrWhiteSpace(jsonItems))
                    throw new CommandExitedException("Please specify the JSON items to be upserted", -14);

                try
                {
                    Utilities.VerboseWriteLine(commandParams.Verbose, "Connecting to the Cosmos DB...");
                    CosmosClient client = new(commandParams.Endpoint, commandParams.Key);
                    Utilities.VerboseWriteLine(commandParams.Verbose, $"Get Database ({commandParams.Database})...");
                    Database database = await client.CreateDatabaseIfNotExistsAsync(commandParams.Database);
                    Utilities.VerboseWriteLine(commandParams.Verbose, $"Get Container ({commandParams.Container})...");
                    Container container = database.GetContainer(commandParams.Container);

                    dynamic json;
                    var responseStatsList = new List<StringBuilder>();
                    if (jsonItems.Trim().StartsWith("["))
                    {
                        json = JArray.Parse(jsonItems);
                        var count = 0;
                        JArray jsonArray = new JArray();
                        foreach (JObject jobj in json)
                        {
                            count++;
                            Utilities.VerboseWriteLine(commandParams.Verbose, $"Upsert {count}...");
                            var upsertedItem = await UpsertItemAsync(container, jobj, commandParams, upsertParams, itemRequestParams);
                            jsonArray.Add(upsertedItem.Resource);

                            if (upsertParams.ShowResponseStats)
                                responseStatsList.Add(Utilities.ItemResponseOutput(count, upsertedItem));
                        }
                        if (!upsertParams.HideResults)
                            Utilities.WriteLine(jsonArray.ToString(upsertParams.CompressJson ? Formatting.None : Formatting.Indented));
                    }
                    else if (jsonItems.Trim().StartsWith("{"))
                    {
                        json = JObject.Parse(jsonItems);
                        Utilities.VerboseWriteLine(commandParams.Verbose, "Upsert json...");
                        var upsertedItem = await UpsertItemAsync(container, json, commandParams, upsertParams, itemRequestParams);

                        if (upsertParams.ShowResponseStats)
                            responseStatsList.Add(Utilities.ItemResponseOutput(1, upsertedItem));

                        if (!upsertParams.HideResults)
                            Utilities.WriteLine(upsertedItem.Resource.ToString(upsertParams.CompressJson ? Formatting.None : Formatting.Indented));
                    }
                    else
                    {
                        throw new CommandExitedException("Invalid JSON", -14);
                    }

                    if (upsertParams.ShowResponseStats)
                    {
                        Utilities.WriteLine("");
                        foreach (var sb in responseStatsList)
                            Utilities.WriteLine(sb.ToString());
                    }
                }
                catch (Exception ex)
                {
                    throw new CommandExitedException(ex.Message, -10);
                }
            }
            catch (CommandExitedException cee)
            {
                Utilities.ErrorWriteLine("ERROR: " + cee.Message);
                throw;
            }
            finally
            {
                Console.ForegroundColor = defaultConsoleColor;
            }
        });
    }

    private static async Task<ItemResponse<dynamic>> UpsertItemAsync(Container container, JObject jobj, CommonParameters commandParams, UpsertParameters upsertParams, ItemRequestParameters itemRequestParams)
    {
        var partitionKey = GetPartitionKey(upsertParams, jobj);
        Utilities.VerboseWriteLine(commandParams.Verbose, $"ParitionKey: {partitionKey}");

        var itemRequestOptions = new ItemRequestOptions
        {
            //AddRequestHeaders
            ConsistencyLevel = Utilities.ConvertToConsistencyLevel(itemRequestParams.ConsistencyLevel),
            //CosmosThresholdOptions
            //DedicatedGatewayRequestOptions
            //EnableContentResponseOnWrite = true,
            ExcludeRegions = itemRequestParams.ExcludeRegion?.ToList(),
            //IfMatchEtag
            //IfNoneMatchEtag
            //IndexingDirective
            PostTriggers = itemRequestParams.PostTriggers?.ToList(),
            PreTriggers = itemRequestParams.PreTriggers?.ToList(),
            //Properties
            SessionToken = itemRequestParams.SessionToken
        };
        Utilities.VerboseWriteLine(commandParams.Verbose, "ItemRequestOptions:");
        Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(itemRequestOptions));
        Utilities.VerboseWriteLine(commandParams.Verbose, "Upsert...");
        Utilities.VerboseWriteLine(commandParams.Verbose, jobj.ToString());

        var upsertedItem = await container.UpsertItemAsync<dynamic>(
            item: jobj,
            partitionKey: new PartitionKey(partitionKey)
        //requestOptions: itemRequestOptions
        );
        return upsertedItem;
    }

    private static string GetPartitionKey(UpsertParameters upsertParams, JObject json)
    {
        if (!String.IsNullOrWhiteSpace(upsertParams.PartitionKey))
            return json[upsertParams.PartitionKey].ToString();

        // Assume validation verified that we have a PartitionKey or PartitionKeyValue
        return upsertParams.PartitionKeyValue;
    }
}