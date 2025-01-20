// Ignore Spelling: app

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerDeleteItemCommand
{
    public static async Task<int> Command(ContainerDeleteItemParameters deleteParams, [Argument] string? jsonItems = null)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            deleteParams.LoadParams();
            jsonItems = LoadJson(deleteParams, jsonItems);

            ValidateJson(deleteParams, jsonItems);
            deleteParams.ValidateParams();

            try
            {
                deleteParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                CosmosClient client = new(deleteParams.Endpoint, deleteParams.Key);
                deleteParams.VerboseWriteLine($"Get Database ({deleteParams.Database})...");
                Database database = await client.CreateDatabaseIfNotExistsAsync(deleteParams.Database);
                deleteParams.VerboseWriteLine($"Get Container ({deleteParams.Container})...");
                Container container = await database.CreateContainerIfNotExistsAsync(
                    deleteParams.Container,
                    $"/{deleteParams.PartitionKey}");

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
                        deleteParams.VerboseWriteLine($"Delete {count}...");
                        var resource = GetKeyAndPartition(jobj, deleteParams);
                        var deletedItem = await DeleteItemAsync(container, resource, deleteParams);
                        jsonArray.Add(resource);

                        if (deleteParams.ShowStats)
                            responseStats.Add(Utilities.ItemResponseJson(count, deletedItem));
                    }

                    if (!deleteParams.ShowStats)
                        Utilities.WriteLine(jsonArray.ToString(deleteParams.CompressJson ? Formatting.None : Formatting.Indented));
                }
                else if (jsonItems.Trim().StartsWith("{"))
                {
                    json = JObject.Parse(jsonItems);
                    deleteParams.VerboseWriteLine("Delete json...");
                    var resource = GetKeyAndPartition(json, deleteParams);
                    var deletedItem = await DeleteItemAsync(container, resource, deleteParams);

                    if (deleteParams.ShowStats)
                        responseStats.Add(Utilities.ItemResponseJson(1, deletedItem));
                    else
                        Utilities.WriteLine(resource.ToString(deleteParams.CompressJson ? Formatting.None : Formatting.Indented));
                }
                else
                {
                    throw new CommandExitedException("Invalid JSON", -14);
                }

                if (deleteParams.ShowStats)
                    Utilities.WriteLine(responseStats.ToString());
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

    private static void ValidateJson(ContainerDeleteItemParameters deleteParams, string? jsonItems)
    {
        if (string.IsNullOrWhiteSpace(jsonItems))
            throw new CommandExitedException("Please specify the JSON items to be deleted", -14);
        // TODO: Validate that the JSON contains the id and partitionKey properties
    }

    private static string? LoadJson(ContainerDeleteItemParameters deleteParams, string? jsonItems)
    {
        return Utilities.ReadFromPipeIfNull(jsonItems);
    }

    private static JObject GetKeyAndPartition(JObject jobj, ContainerDeleteItemParameters deleteParams)
    {
        var id = jobj["id"].ToString();
        // TODO: what if the partition key is not a string?
        var partitionKey = jobj[deleteParams.PartitionKey].ToString();
        deleteParams.VerboseWriteLine($"Id: {id}, ParitionKey: {partitionKey}");
        return new JObject
        {
            ["id"] = id,
            [deleteParams.PartitionKey] = partitionKey
        };
    }

    private static async Task<ItemResponse<dynamic>> DeleteItemAsync(Container container, JObject jobj, ContainerDeleteItemParameters deleteParams)
    {
        var id = jobj["id"].ToString();
        var partitionKey = jobj[deleteParams.PartitionKey].ToString();

        var itemRequestOptions = new ItemRequestOptions
        {
            //AddRequestHeaders
            ConsistencyLevel = Utilities.ConvertToConsistencyLevel(deleteParams.ConsistencyLevel),
            //CosmosThresholdOptions
            //DedicatedGatewayRequestOptions
            //EnableContentResponseOnWrite = true,
            ExcludeRegions = deleteParams.ExcludeRegion?.ToList(),
            //IfMatchEtag
            //IfNoneMatchEtag
            //IndexingDirective
            PostTriggers = deleteParams.PostTriggers?.ToList(),
            PreTriggers = deleteParams.PreTriggers?.ToList(),
            //Properties
            SessionToken = deleteParams.SessionToken
        };
        deleteParams.VerboseWriteLine("ItemRequestOptions:");
        deleteParams.VerboseWriteLine(Utilities.SerializeObject(itemRequestOptions));
        deleteParams.VerboseWriteLine("Delete...");
        deleteParams.VerboseWriteLine(jobj.ToString());

        var deletedItem = await container.DeleteItemAsync<dynamic>(
            id,
            partitionKey: new PartitionKey(partitionKey),
            requestOptions: itemRequestOptions
        );

        return deletedItem;
    }
}