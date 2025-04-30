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

    private static JObject GetKeyAndPartition(JObject jObj, ContainerDeleteItemParameters deleteParams)
    {
        var id = jObj["id"]?.ToString();
        if (id == null)
            throw new CommandExitedException("JSON does not contain an 'id' property", -14);
        var partitionKey = jObj[deleteParams.PartitionKey]?.ToString();
        if (id == null)
            throw new CommandExitedException($"JSON does not contain an '{deleteParams.PartitionKey}' property", -14);

        deleteParams.VerboseWriteLine($"Id: {id}, PartitionKey: {partitionKey}");
        return new JObject
        {
            ["id"] = id,
            [deleteParams.PartitionKey] = partitionKey
        };
    }

    private static async Task<ItemResponse<dynamic>> DeleteItemAsync(Container container, JObject jObj, ContainerDeleteItemParameters deleteParams)
    {
        var id = (jObj["id"]?.ToString()) ?? throw new CommandExitedException("JSON does not contain an 'id' property", -14);
        var partitionKeyToken = jObj[deleteParams.PartitionKey] ?? throw new CommandExitedException($"PartitionKey {deleteParams.PartitionKey} not found in JSON", -14);
        PartitionKey partitionKey = partitionKeyToken.Type switch
        {
            JTokenType.Integer => new PartitionKey(partitionKeyToken.Value<int>()),
            JTokenType.Float => new PartitionKey(partitionKeyToken.Value<double>()),
            JTokenType.Boolean => new PartitionKey(partitionKeyToken.Value<bool>()),
            JTokenType.String => new PartitionKey(partitionKeyToken.Value<string>()),
            JTokenType.Guid => new PartitionKey(partitionKeyToken.Value<Guid>().ToString()),
            JTokenType.Date => new PartitionKey(partitionKeyToken.Value<DateTime>().ToString("o")),
            _ => throw new CommandExitedException($"Unsupported PartitionKey type: {partitionKeyToken.Type}", -14)
        };
        deleteParams.VerboseWriteLine($"PartitionKey: {partitionKeyToken} (Type: {partitionKeyToken.Type})");

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
        deleteParams.VerboseWriteLine(jObj.ToString());

        var deletedItem = await container.DeleteItemAsync<dynamic>(
            id,
            partitionKey: partitionKey,
            requestOptions: itemRequestOptions
        );

        return deletedItem;
    }
}