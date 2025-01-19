// Ignore Spelling: app
// Ignore Spelling: upsert
// Ignore Spelling: upserted

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerUpsertItemCommand
{
    public static async Task<int> Command(ContainerUpsertItemParameters upsertParams, [Argument] string? jsonItems = null)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            upsertParams.LoadParams();
            jsonItems = LoadJson(upsertParams, jsonItems);


            ValidateJson(upsertParams, jsonItems);
            upsertParams.ValidateParams();

            try
            {
                upsertParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                CosmosClient client = new(upsertParams.Endpoint, upsertParams.Key);
                upsertParams.VerboseWriteLine($"Get Database ({upsertParams.Database})...");
                Database database = await client.CreateDatabaseIfNotExistsAsync(upsertParams.Database);
                upsertParams.VerboseWriteLine($"Get Container ({upsertParams.Container})...");
                Container container = await database.CreateContainerIfNotExistsAsync(
                    upsertParams.Container,
                    $"/{upsertParams.PartitionKey}");

                dynamic json;
                var responseStats = new JArray();
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

                        if (upsertParams.ShowStats)
                            responseStats.Add(Utilities.ItemResponseJson(count, upsertedItem));
                    }
                    if (!upsertParams.ShowStats)
                        Utilities.WriteLine(jsonArray.ToString(upsertParams.CompressJson ? Formatting.None : Formatting.Indented));
                }
                else if (jsonItems.Trim().StartsWith("{"))
                {
                    json = JObject.Parse(jsonItems);
                    upsertParams.VerboseWriteLine("Upsert json...");
                    var upsertedItem = await UpsertItemAsync(container, json, upsertParams);

                    if (upsertParams.ShowStats)
                        responseStats.Add(Utilities.ItemResponseJson(1, upsertedItem));
                    else
                        Utilities.WriteLine(upsertedItem.Resource.ToString(upsertParams.CompressJson ? Formatting.None : Formatting.Indented));
                }
                else
                {
                    throw new CommandExitedException("Invalid JSON", -14);
                }

                if (upsertParams.ShowStats)
                {
                    Utilities.WriteLine(responseStats.ToString());
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

    private static void ValidateJson(ContainerUpsertItemParameters upsertParams, string? jsonItems)
    {
        if (string.IsNullOrWhiteSpace(jsonItems))
            throw new CommandExitedException("Please specify the JSON items to be upserted", -14);
        // TODO: Validate that the JSON contains the id and partitionKey properties
    }

    private static string? LoadJson(ContainerUpsertItemParameters upsertParams, string? jsonItems)
    {
        return Utilities.ReadFromPipeIfNull(jsonItems);
    }

    private static async Task<ItemResponse<dynamic>> UpsertItemAsync(Container container, JObject jobj, ContainerUpsertItemParameters upsertParams)
    {
        var partitionKey = jobj[upsertParams.PartitionKey].ToString();
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
            partitionKey: new PartitionKey(partitionKey),
            requestOptions: itemRequestOptions
        );
        return upsertedItem;
    }
}