// Ignore Spelling: app

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
            selectParams.LoadParams();
            query = LoadQuery(selectParams, query);


            ValidateQuery(selectParams, query);
            selectParams.ValidateParams();

            try
            {
                selectParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(selectParams.Endpoint, selectParams.Key);
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
                    MaxItemCount = selectParams.MaxItems,
                    //PartitionKey
                    PopulateIndexMetrics = selectParams.PopulateIndexMetrics,
                    //PriorityLevel
                    //Properties
                    ResponseContinuationTokenLimitInKb = selectParams.ResponseContinuationTokenLimitInKb,
                    SessionToken = selectParams.SessionToken,
                };
                selectParams.VerboseWriteLine(Utilities.SerializeObject(queryRequestOptions));


                var feedCnt = 0;
                var responseStats = new JArray();
                var metrics = new JArray();
                JArray jsonArray = new JArray();

                var continuationToken = await ReadContinuationTokenAsync(selectParams.ContinuationTokenFile);
                if (continuationToken == null || !string.IsNullOrWhiteSpace(continuationToken))
                {
                    using FeedIterator<dynamic> feed = container.GetItemQueryIterator<dynamic>(
                        queryDefinition: new(query),
                        requestOptions: queryRequestOptions,
                        continuationToken: continuationToken);

                    selectParams.VerboseWriteLine("Processing Results");
                    selectParams.VerboseWriteLine(Utilities.SerializeObject(selectParams));

                    while (feed.HasMoreResults)
                    {
                        if (feedCnt >= selectParams.MaxEnumerations)
                            break;

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

                        continuationToken = response.ContinuationToken;

                        if (selectParams.ShowStats)
                            responseStats.Add(Utilities.FeedResponseJson(feedCnt, response));

                        if (selectParams.PopulateIndexMetrics)
                            metrics.Add(JObject.Parse(response.IndexMetrics));
                    }
                }

                if (!string.IsNullOrWhiteSpace(selectParams.ContinuationTokenFile))
                    SaveContinuationToken(selectParams.ContinuationTokenFile, continuationToken);

                if (selectParams.PopulateIndexMetrics)
                    Utilities.WriteLine(metrics.ToString());
                else if (selectParams.ShowStats)
                    Utilities.WriteLine(responseStats.ToString());
                else
                    Utilities.WriteLine(jsonArray.ToString(selectParams.CompressJson ? Formatting.None : Formatting.Indented));

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


    private static async Task<string?> ReadContinuationTokenAsync(string continuationTokenFile)
    {
        if (string.IsNullOrWhiteSpace(continuationTokenFile))
            return null;

        if (!File.Exists(continuationTokenFile))
            return null;

        using var readFile = new StreamReader(continuationTokenFile);
        return await readFile.ReadToEndAsync();
    }

    private static void SaveContinuationToken(string continuationTokenFile, string? continuationToken)
    {
        using var outputFile = new StreamWriter(continuationTokenFile, false);
        outputFile.Write(continuationToken ?? "");
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
}