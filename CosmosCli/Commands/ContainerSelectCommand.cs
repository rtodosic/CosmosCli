// Ignore Spelling: app

using System.Text;

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerSelectCommand
{
    public static async Task<int> Command(ContainerSelectParameters selectParams, [Argument] string? query = null)
    {

        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            selectParams = LoadParams(selectParams);
            query = LoadQuery(selectParams, query);


            ValidateQuery(selectParams, query);
            ValidateParams(selectParams);

            try
            {
                selectParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(selectParams.Key);
                selectParams.VerboseWriteLine($"Get Database ({selectParams.Database})...");
                Database database = await client.CreateDatabaseIfNotExistsAsync(selectParams.Database);

                selectParams.VerboseWriteLine($"Get Container ({selectParams.Container})...");
                Container container = database.GetContainer(selectParams.Container);

                selectParams.VerboseWriteLine("Executing query...");
                var queryRequestOptions = new QueryRequestOptions()
                {
                    //AddRequestHeaders
                    ConsistencyLevel = Utilities.ConvertToConsistencyLevel(selectParams.ConsistencyLevel),
                    //CosmosThresholdOptions = null, NonPointOperationLatencyThreshold
                    //DedicatedGatewayRequestOptions
                    EnableLowPrecisionOrderBy = selectParams.EnableLowPrecisionOrderBy,
                    EnableOptimisticDirectExecution = !(selectParams.DisableOptimisticDirectExecution ?? false),
                    EnableScanInQuery = selectParams.EnableScanInQuery,
                    ExcludeRegions = selectParams.ExcludeRegion?.ToList(),
                    //IfMatchEtag
                    //IfNoneMatchEtag
                    //MaxBufferedItemCount
                    MaxItemCount = selectParams.MaxItemCount,
                    //PartitionKey
                    PopulateIndexMetrics = selectParams.PopulateIndexMetrics,
                    //PriorityLevel
                    //Properties
                    ResponseContinuationTokenLimitInKb = selectParams.ResponseContinuationTokenLimitInKb,
                    SessionToken = selectParams.SessionToken,
                };
                selectParams.VerboseWriteLine(Utilities.SerializeObject(queryRequestOptions));

                using FeedIterator<dynamic> feed = container.GetItemQueryIterator<dynamic>(
                    queryDefinition: new(query),
                    requestOptions: queryRequestOptions);

                selectParams.VerboseWriteLine("Processing Results");
                selectParams.VerboseWriteLine(Utilities.SerializeObject(selectParams));

                var feedCnt = 0;
                var responseStatsList = new List<StringBuilder>();
                JArray jsonArray = new JArray();
                while (feed.HasMoreResults)
                {
                    if (feedCnt > selectParams.MaxEnumerations)
                        throw new CommandExitedException(-20);
                    selectParams.VerboseWriteLine($"Enumeration {feedCnt + 1}...");
                    feedCnt++;

                    FeedResponse<dynamic> response = await feed.ReadNextAsync();
                    foreach (dynamic item in response)
                    {
                        var obj = item as JObject;
                        if (obj != null)
                        {
                            if (selectParams.DropSystemProperties)
                            {
                                obj.Remove("_rid");
                                obj.Remove("_attachments");
                                obj.Remove("_etag");
                                obj.Remove("_self");
                                obj.Remove("_ts");
                            }
                            jsonArray.Add(obj);
                        }
                    }

                    if (selectParams.ShowResponseStats)
                        responseStatsList.Add(Utilities.FeedResponseOutput(feedCnt, response));

                    if (selectParams.PopulateIndexMetrics)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"{response.IndexMetrics}");
                        responseStatsList.Add(sb);
                    }
                }

                Utilities.WriteLine(jsonArray.ToString(selectParams.CompressJson ? Formatting.None : Formatting.Indented));

                if (selectParams.PopulateIndexMetrics || selectParams.ShowResponseStats)
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
            Console.Out.Flush();
            Console.ForegroundColor = defaultConsoleColor;
        }
        return 0;
    }

    private static ContainerSelectParameters LoadParams(ContainerSelectParameters selectParams)
    {
        selectParams.VerboseWriteLine("Reading params from environment variables:");
        var envParams = new ContainerSelectParameters();
        envParams.ReadParamsFromEnvironment();
        selectParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        selectParams.VerboseWriteLine("Argument base params:");
        selectParams.VerboseWriteLine(Utilities.SerializeObject(selectParams));

        envParams.Apply(selectParams);
        selectParams.VerboseWriteLine("Resolved to the following:");
        selectParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        return envParams;
    }

    private static string? LoadQuery(ContainerSelectParameters selectParams, string? query)
    {
        if (query != null)
        {
            selectParams.VerboseWriteLine("Query from args:");
            selectParams.VerboseWriteLine(query);
        }
        else
        {
            query = Utilities.ReadFromPipeIfNull(query);
            if (query != null)
            {

                selectParams.VerboseWriteLine("Query from pipeline:");
                selectParams.VerboseWriteLine(query ?? "");
            }
        }
        return query;
    }

    private static void ValidateQuery(ContainerSelectParameters selectParams, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new CommandExitedException("Please specify a query", -14);
    }

    private static void ValidateParams(ContainerSelectParameters selectParams)
    {
        if (string.IsNullOrWhiteSpace(selectParams.Endpoint))
        {
            throw new CommandExitedException("endpoint is required", -10);
        }

        if (string.IsNullOrWhiteSpace(selectParams.Key))
        {
            throw new CommandExitedException(" key is required", -11);
        }
        if (string.IsNullOrWhiteSpace(selectParams.Database))
        {
            throw new CommandExitedException("Please specify a database", -12);
        }
        if (string.IsNullOrWhiteSpace(selectParams.Container))
        {
            throw new CommandExitedException("Please specify a container", -13);
        }
    }
}