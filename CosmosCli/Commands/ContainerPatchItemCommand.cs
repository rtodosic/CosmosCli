using System.Runtime.Serialization;
using Cocona;
using CosmosCli.Parameters;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerPatchItemCommand
{
    public static async Task<int> Command(ContainerPatchItemParameters patchParams, [Argument] string? jsonItems = null)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            patchParams.LoadParams();
            jsonItems = LoadJson(patchParams, jsonItems);


            ValidateJson(patchParams, jsonItems);
            patchParams.ValidateParams();

            try
            {
                patchParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                CosmosClient client = new(patchParams.Endpoint, patchParams.Key);
                patchParams.VerboseWriteLine($"Get Database ({patchParams.Database})...");
                Database database = await client.CreateDatabaseIfNotExistsAsync(patchParams.Database);
                patchParams.VerboseWriteLine($"Get Container ({patchParams.Container})...");
                Container container = string.IsNullOrWhiteSpace(patchParams.PartitionKey)
                    ? database.GetContainer(patchParams.Container)
                    : (Container)await database.CreateContainerIfNotExistsAsync(
                        patchParams.Container,
                        $"/{patchParams.PartitionKey}");

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
                        patchParams.VerboseWriteLine($"Patch {count}...");
                        var patchItem = ConvertToPatch(jobj.ToString());
                        patchParams.VerboseWriteLine($"Patch...");
                        var patchedItem = await PatchItemAsync(container, patchItem, patchParams);
                        jsonArray.Add(patchedItem.Resource);
                        if (patchParams.ShowStats)
                            responseStats.Add(Utilities.ItemResponseJson(count, patchedItem));
                    }
                    if (!patchParams.ShowStats)
                        Utilities.WriteLine(jsonArray.ToString(patchParams.CompressJson ? Formatting.None : Formatting.Indented));
                }
                else if (jsonItems.Trim().StartsWith("{"))
                {
                    patchParams.VerboseWriteLine("Patch json...");
                    var patchItem = ConvertToPatch(jsonItems);
                    patchParams.VerboseWriteLine($"Patch...");
                    var patchedItem = await PatchItemAsync(container, patchItem, patchParams);
                    if (patchParams.ShowStats)
                        responseStats.Add(Utilities.ItemResponseJson(1, patchedItem));
                    else
                        Utilities.WriteLine(patchedItem.Resource.ToString(patchParams.CompressJson ? Formatting.None : Formatting.Indented));
                }
                else
                {
                    throw new CommandExitedException("Invalid JSON", -14);
                }

                if (patchParams.ShowStats)
                {
                    Utilities.WriteLine(responseStats.ToString(patchParams.CompressJson ? Formatting.None : Formatting.Indented));
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

    private static void ValidateJson(ContainerPatchItemParameters patchParams, string? jsonItems)
    {
        if (string.IsNullOrWhiteSpace(jsonItems))
            throw new CommandExitedException("Please specify the JSON items to be patched", -14);
        // TODO: Validate that the JSON contains the id and partitionKey properties
    }

    private static string? LoadJson(ContainerPatchItemParameters patchParams, string? jsonItems)
    {
        return Utilities.ReadFromPipeIfNull(jsonItems);
    }

    private static async Task<ItemResponse<dynamic>> PatchItemAsync(Container container, PatchItem patch, ContainerPatchItemParameters patchParams)
    {
        var partitionKey = patch.PartitionKey ?? patch.Id;
        var pk = partitionKey switch
        {
            string s => new PartitionKey(s),
            int i => new PartitionKey(i),
            long l => new PartitionKey(l),
            double d => new PartitionKey(d),
            bool b => new PartitionKey(b),

            Guid g => new PartitionKey(g.ToString()),
            DateTime dt => new PartitionKey(dt.ToString("o")), // ISO‑8601

            _ => throw new ArgumentException($"Unsupported partition key type: {partitionKey?.GetType().Name}")
        };

        var itemRequestOptions = new PatchItemRequestOptions
        {
            //AddRequestHeaders
            ConsistencyLevel = Utilities.ConvertToConsistencyLevel(patchParams.ConsistencyLevel),
            //CosmosThresholdOptions
            //DedicatedGatewayRequestOptions
            //EnableContentResponseOnWrite = true,
            ExcludeRegions = patchParams.ExcludeRegion?.ToList(),
            //IfMatchEtag
            //IfNoneMatchEtag
            //IndexingDirective
            PostTriggers = patchParams.PostTriggers?.ToList(),
            PreTriggers = patchParams.PreTriggers?.ToList(),
            //Properties
            SessionToken = patchParams.SessionToken,
            FilterPredicate = patchParams.FilterPredicate
        };
        patchParams.VerboseWriteLine("ItemRequestOptions:");
        patchParams.VerboseWriteLine(Utilities.SerializeObject(itemRequestOptions));
        patchParams.VerboseWriteLine("Patch...");
        patchParams.VerboseWriteLine(patch.ToString());

        var patchedItem = await container.PatchItemAsync<dynamic>(
            id: patch.Id,
            partitionKey: pk,
            patchOperations: ConvertToPatchOperations(patch.PatchOperations),
            requestOptions: itemRequestOptions
        );
        return patchedItem;
    }

    private static IReadOnlyList<PatchOperation> ConvertToPatchOperations(List<PatchOp> operations)
    {
        var patchOperations = new List<PatchOperation>();
        foreach (var op in operations)
        {
            PatchOperation patchOp = op.OperationType switch
            {
                PatchOperationType.Add => PatchOperation.Add(op.Path, op.Value),
                PatchOperationType.Remove => PatchOperation.Remove(op.Path),
                PatchOperationType.Replace => PatchOperation.Replace(op.Path, op.Value),
                PatchOperationType.Set => PatchOperation.Set(op.Path, op.Value),
                PatchOperationType.Increment => PatchOperation.Increment(op.Path, Convert.ToDouble(op.Value ?? 0)),
                PatchOperationType.Move => PatchOperation.Move(op.From!, op.Path),
                _ => throw new CommandExitedException($"Unsupported Patch Operation type: {op.OperationType}", -14)
            };
            patchOperations.Add(patchOp);
        }
        return patchOperations;
    }

    private static PatchItem ConvertToPatch(string? patchItem)
    {
        if (string.IsNullOrWhiteSpace(patchItem))
            throw new CommandExitedException("Please specify the JSON patch item", -14);

        PatchItem? patch = JsonConvert.DeserializeObject<PatchItem>(patchItem);
        if (patch == null)
        {
            throw new CommandExitedException("Invalid JSON patch item", -14);
        }

        return patch;
    }
}

public class PatchItem
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = "";

    [JsonProperty(PropertyName = "partitionKey")]
    public object? PartitionKey { get; set; }

    [JsonProperty(PropertyName = "patchOperations")]
    public List<PatchOp> PatchOperations { get; set; } = new();

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}

public enum PatchOpType
{
    [EnumMember(Value = "add")]
    Add,
    [EnumMember(Value = "remove")]
    Remove,
    [EnumMember(Value = "replace")]
    Replace,
    [EnumMember(Value = "set")]
    Set,
    [EnumMember(Value = "incr")]
    Increment,
    [EnumMember(Value = "move")]
    Move
}

public class PatchOp
{
    [JsonProperty(PropertyName = "op")]
    public PatchOperationType OperationType { get; set; }

    [JsonProperty(PropertyName = "path")]
    public string Path { get; set; } = "";

    [JsonProperty(PropertyName = "from")]
    public string? From { get; set; } = "";

    [JsonProperty(PropertyName = "value")]
    public object? Value { get; set; }
}