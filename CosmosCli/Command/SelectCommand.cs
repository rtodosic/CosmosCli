// Ignore Spelling: app

using System.Text;

using Cocona;
using Cocona.Builder;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Command;

public static class SelectCommand
{
    public static CommandConventionBuilder AddSelectCommand(this CoconaApp app)
    {
        return app.AddCommand("select", async(
            ILogger < Program > logger,
            CommonParameters commandParams,
            SelectParameters selectParams,
            QueryRequestParameters queryParams,
            [Argument] string ? query = null) =>
            {
            var defaultConsoleColor = Console.ForegroundColor;
            try
            {
                // Get common parameters
                Utilities.VerboseWriteLine(commandParams.Verbose, "Reading params from config file:");
                CommonParameters configParams = CommonParameters.ReadParamsFromConfigFile();
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(configParams));

                Utilities.VerboseWriteLine(commandParams.Verbose, "Reading params from environment variables:");
                CommonParameters envParams = CommonParameters.ReadParamsFromEnvironment();
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(envParams));

                Utilities.VerboseWriteLine(commandParams.Verbose, "Argument base params:");
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(commandParams));

                commandParams = configParams.Apply(envParams).Apply(commandParams);
                Utilities.VerboseWriteLine(commandParams.Verbose, "Resolved to the following:");
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(commandParams));

                // Get the query
                if (query != null)
                {
                    Utilities.VerboseWriteLine(commandParams.Verbose, "Query from args:");
                    Utilities.VerboseWriteLine(commandParams.Verbose, query);
                }
                else
                {
                    query = Utilities.ReadFromPipeIfNull(query);
                    if (query != null)
                    {

                        Utilities.VerboseWriteLine(commandParams.Verbose, "Query from pipeline:");
                        Utilities.VerboseWriteLine(commandParams.Verbose, query ?? "");
                    }
                }

                // Validate the query
                if (string.IsNullOrWhiteSpace(query))
                    throw new CommandExitedException("Please specify a query", -14);

                // Validate the common parameters
                commandParams.ValidateParams();

                try
                {
                    Utilities.VerboseWriteLine(commandParams.Verbose, "Connecting to the Cosmos DB...");
                    var client = new CosmosClient(commandParams.Endpoint, commandParams.Key);
                    Utilities.VerboseWriteLine(commandParams.Verbose, $"Get Database ({commandParams.Database})...");
                    Database database = await client.CreateDatabaseIfNotExistsAsync(commandParams.Database);
                    Utilities.VerboseWriteLine(commandParams.Verbose, $"Get Container ({commandParams.Container})...");
                    Container container = database.GetContainer(commandParams.Container);

                    Utilities.VerboseWriteLine(commandParams.Verbose, "Executing query...");
                    var queryRequestOptions = new QueryRequestOptions()
                    {
                        //AddRequestHeaders
                        ConsistencyLevel = Utilities.ConvertToConsistencyLevel(queryParams.ConsistencyLevel),
                        //CosmosThresholdOptions = null, NonPointOperationLatencyThreshold
                        //DedicatedGatewayRequestOptions
                        EnableLowPrecisionOrderBy = queryParams.EnableLowPrecisionOrderBy,
                        EnableOptimisticDirectExecution = !(queryParams.DisableOptimisticDirectExecution ?? false),
                        EnableScanInQuery = queryParams.EnableScanInQuery,
                        ExcludeRegions = queryParams.ExcludeRegion?.ToList(),
                        //IfMatchEtag
                        //IfNoneMatchEtag
                        //MaxBufferedItemCount
                        MaxItemCount = queryParams.MaxItemCount,
                        //PartitionKey
                        PopulateIndexMetrics = queryParams.PopulateIndexMetrics,
                        //PriorityLevel
                        //Properties
                        ResponseContinuationTokenLimitInKb = queryParams.ResponseContinuationTokenLimitInKb,
                        SessionToken = queryParams.SessionToken,
                    };
                    Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(queryRequestOptions));

                    using FeedIterator<dynamic> feed = container.GetItemQueryIterator<dynamic>(
                        queryDefinition: new(query),
                        requestOptions: queryRequestOptions);

                    Utilities.VerboseWriteLine(commandParams.Verbose, "Processing Results");
                    Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(selectParams));

                    var feedCnt = 0;
                    var responseStatsList = new List<StringBuilder>();
                    JArray jsonArray = new JArray();
                    while (feed.HasMoreResults)
                    {
                        if (feedCnt > selectParams.MaxEnumerations)
                            throw new CommandExitedException(-20);
                        Utilities.VerboseWriteLine(commandParams.Verbose, $"Enumeration {feedCnt + 1}...");
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

                        if (queryParams.PopulateIndexMetrics)
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"{response.IndexMetrics}");
                            responseStatsList.Add(sb);
                        }
                    }

                    Utilities.WriteLine(jsonArray.ToString(selectParams.CompressJson ? Formatting.None : Formatting.Indented));

                    if (queryParams.PopulateIndexMetrics || selectParams.ShowResponseStats)
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
        });
    }
}