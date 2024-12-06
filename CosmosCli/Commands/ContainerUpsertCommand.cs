// Ignore Spelling: app
// Ignore Spelling: upsert
// Ignore Spelling: upserted

using System.Text;

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerUpsertCommand
{
    public static async Task<int> Command(ContainerUpsertParameters upsertParams, [Argument] string? jsonItems = null)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            upsertParams = LoadParams(upsertParams);
            jsonItems = LoadJson(upsertParams, jsonItems);


            ValidateJson(upsertParams, jsonItems);
            ValidateParams(upsertParams);

            try
            {
                upsertParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                CosmosClient client = new(upsertParams.Endpoint, upsertParams.Key);
                upsertParams.VerboseWriteLine($"Get Database ({upsertParams.Database})...");
                Database database = await client.CreateDatabaseIfNotExistsAsync(upsertParams.Database);
                upsertParams.VerboseWriteLine($"Get Container ({upsertParams.Container})...");
                Container container = database.GetContainer(upsertParams.Container);

                dynamic json;
                var responseStatsList = new List<StringBuilder>();
                if (jsonItems!.Trim().StartsWith("["))
                {
                    json = JArray.Parse(jsonItems);
                    var count = 0;
                    JArray jsonArray = new JArray();
                    foreach (JObject jobj in json)
                    {
                        count++;
                        upsertParams.VerboseWriteLine($"Upsert {count}...");
                        var upsertedItem = await UpsertItemAsync(container, jobj, upsertParams);
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
                    upsertParams.VerboseWriteLine("Upsert json...");
                    var upsertedItem = await UpsertItemAsync(container, json, upsertParams);

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
        return 0;
    }

    private static void ValidateParams(ContainerUpsertParameters upsertParams)
    {
        if (string.IsNullOrWhiteSpace(upsertParams.Endpoint))
        {
            throw new CommandExitedException("endpoint is required", -10);
        }

        if (string.IsNullOrWhiteSpace(upsertParams.Key))
        {
            throw new CommandExitedException(" key is required", -11);
        }
        if (string.IsNullOrWhiteSpace(upsertParams.Database))
        {
            throw new CommandExitedException("Please specify a database", -12);
        }
        if (string.IsNullOrWhiteSpace(upsertParams.Container))
        {
            throw new CommandExitedException("Please specify a container", -13);
        }
    }

    private static void ValidateJson(ContainerUpsertParameters upsertParams, string? jsonItems)
    {
        if (string.IsNullOrWhiteSpace(jsonItems))
            throw new CommandExitedException("Please specify the JSON items to be upserted", -14);
    }

    private static string? LoadJson(ContainerUpsertParameters upsertParams, string? jsonItems)
    {
        return Utilities.ReadFromPipeIfNull(jsonItems);
    }

    private static ContainerUpsertParameters LoadParams(ContainerUpsertParameters upsertParams)
    {
        upsertParams.VerboseWriteLine("Reading params from environment variables:");
        var envParams = new ContainerUpsertParameters();
        envParams.ReadParamsFromEnvironment();
        upsertParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        upsertParams.VerboseWriteLine("Argument base params:");
        upsertParams.VerboseWriteLine(Utilities.SerializeObject(upsertParams));

        envParams.Apply(upsertParams);
        upsertParams.VerboseWriteLine("Resolved to the following:");
        upsertParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        return envParams;
    }

    private static async Task<ItemResponse<dynamic>> UpsertItemAsync(Container container, JObject jobj, ContainerUpsertParameters upsertParams)
    {
        var partitionKey = GetPartitionKey(upsertParams, jobj);
        upsertParams.VerboseWriteLine($"ParitionKey: {partitionKey}");

        var itemRequestOptions = new ItemRequestOptions
        {
            //AddRequestHeaders
            ConsistencyLevel = Utilities.ConvertToConsistencyLevel(upsertParams.ConsistencyLevel),
            //CosmosThresholdOptions
            //DedicatedGatewayRequestOptions
            //EnableContentResponseOnWrite = true,
            ExcludeRegions = upsertParams.ExcludeRegion?.ToList(),
            //IfMatchEtag
            //IfNoneMatchEtag
            //IndexingDirective
            PostTriggers = upsertParams.PostTriggers?.ToList(),
            PreTriggers = upsertParams.PreTriggers?.ToList(),
            //Properties
            SessionToken = upsertParams.SessionToken
        };
        upsertParams.VerboseWriteLine("ItemRequestOptions:");
        upsertParams.VerboseWriteLine(Utilities.SerializeObject(itemRequestOptions));
        upsertParams.VerboseWriteLine("Upsert...");
        upsertParams.VerboseWriteLine(jobj.ToString());

        var upsertedItem = await container.UpsertItemAsync<dynamic>(
            item: jobj,
            partitionKey: new PartitionKey(partitionKey)
        //requestOptions: itemRequestOptions
        );
        return upsertedItem;
    }

    private static string GetPartitionKey(ContainerUpsertParameters upsertParams, JObject json)
    {
        if (!String.IsNullOrWhiteSpace(upsertParams.PartitionKey))
            return json[upsertParams.PartitionKey].ToString();

        // Assume validation verified that we have a PartitionKey or PartitionKeyValue
        return upsertParams.PartitionKeyValue;
    }
}