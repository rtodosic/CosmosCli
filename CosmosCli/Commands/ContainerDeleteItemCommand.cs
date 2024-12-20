// Ignore Spelling: app

using System.Text;

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerDeleteItemCommand
{
    public static async Task<int> Command(ContainerDeleteItemParameters deleteParams, [Argument] string? jsonItems = null)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            deleteParams = LoadParams(deleteParams);
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
                Container container = database.GetContainer(deleteParams.Container);

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
                        deleteParams.VerboseWriteLine($"Delete {count}...");
                        var deletedItem = await DeleteItemAsync(container, jobj, deleteParams);
                        jsonArray.Add(deletedItem.Resource);

                        if (deleteParams.ShowResponseStats)
                            responseStatsList.Add(Utilities.ItemResponseOutput(count, deletedItem));
                    }
                }
                else if (jsonItems.Trim().StartsWith("{"))
                {
                    json = JObject.Parse(jsonItems);
                    deleteParams.VerboseWriteLine("Delete json...");
                    var deletedItem = await DeleteItemAsync(container, json, deleteParams);

                    if (deleteParams.ShowResponseStats)
                        responseStatsList.Add(Utilities.ItemResponseOutput(1, deletedItem));
                }
                else
                {
                    throw new CommandExitedException("Invalid JSON", -14);
                }

                if (deleteParams.ShowResponseStats)
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

    private static ContainerDeleteItemParameters LoadParams(ContainerDeleteItemParameters deleteParams)
    {
        deleteParams.VerboseWriteLine("Reading params from environment variables:");
        var envParams = new ContainerDeleteItemParameters();
        envParams.ReadParamsFromEnvironment();
        deleteParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        deleteParams.VerboseWriteLine("Argument base params:");
        deleteParams.VerboseWriteLine(Utilities.SerializeObject(deleteParams));

        envParams.Apply(deleteParams);
        deleteParams.VerboseWriteLine("Resolved to the following:");
        deleteParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        return envParams;
    }

    private static async Task<ItemResponse<dynamic>> DeleteItemAsync(Container container, JObject jobj, ContainerDeleteItemParameters deleteParams)
    {
        var id = jobj[deleteParams.Id].ToString();
        // TODO: what if the partition key is not a string?
        var partitionKey = jobj[deleteParams.PartitionKey].ToString();
        deleteParams.VerboseWriteLine($"Id: {id}, ParitionKey: {partitionKey}");

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