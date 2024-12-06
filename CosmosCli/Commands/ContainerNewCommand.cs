using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;

namespace CosmosCli.Commands;

public static class ContainerNewCommand
{
    public static async Task<int> Command(ContainerNewParameters containerNewParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            containerNewParams = LoadParams(containerNewParams);
            containerNewParams.ValidateParams();

            try
            {
                containerNewParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(containerNewParams.Endpoint, containerNewParams.Key);

                Database db = await client.CreateDatabaseIfNotExistsAsync(containerNewParams.Database);

                Container container = (containerNewParams.AutoscaleThroughput, containerNewParams.ManualThroughput) switch
                {
                    (not null, null) => await db.CreateContainerIfNotExistsAsync(
                        new ContainerProperties(containerNewParams.Container, containerNewParams.PartitionKey)
                        {
                            //AnalyticalStoreTimeToLiveInSeconds
                            //ClientEncryptionPolicy
                            //ConflictResolutionPolicy
                            DefaultTimeToLive = containerNewParams.DefaultTimeToLive,
                            //GeospatialConfig
                            IndexingPolicy = ReadIndexFile(containerNewParams.IndexFilename)
                            //PartitionKeyDefinitionVersion
                            //PartitionKeyPaths
                            //SelfLink
                            //TimeToLivePropertyPath
                            //UniqueKeyPolicy
                            //VectorEmbeddingPolicy
                        },
                        ThroughputProperties.CreateAutoscaleThroughput(containerNewParams.AutoscaleThroughput ?? 0)),
                    (null, not null) => await db.CreateContainerIfNotExistsAsync(
                       new ContainerProperties(containerNewParams.Container, containerNewParams.PartitionKey)
                       {
                           //AnalyticalStoreTimeToLiveInSeconds
                           //ClientEncryptionPolicy
                           //ConflictResolutionPolicy
                           DefaultTimeToLive = containerNewParams.DefaultTimeToLive,
                           //GeospatialConfig
                           IndexingPolicy = ReadIndexFile(containerNewParams.IndexFilename)
                           //PartitionKeyDefinitionVersion
                           //PartitionKeyPaths
                           //SelfLink
                           //TimeToLivePropertyPath
                           //UniqueKeyPolicy
                           //VectorEmbeddingPolicy
                       },
                       ThroughputProperties.CreateManualThroughput(containerNewParams.ManualThroughput ?? 0)),
                    _ => await db.CreateContainerIfNotExistsAsync(
                      new ContainerProperties(containerNewParams.Container, containerNewParams.PartitionKey)
                      {
                          //AnalyticalStoreTimeToLiveInSeconds
                          //ClientEncryptionPolicy
                          //ConflictResolutionPolicy
                          DefaultTimeToLive = containerNewParams.DefaultTimeToLive,
                          //GeospatialConfig
                          IndexingPolicy = ReadIndexFile(containerNewParams.IndexFilename)
                          //PartitionKeyDefinitionVersion
                          //PartitionKeyPaths
                          //SelfLink
                          //TimeToLivePropertyPath
                          //UniqueKeyPolicy
                          //VectorEmbeddingPolicy
                      },
                      ThroughputProperties.CreateManualThroughput(containerNewParams.ManualThroughput ?? 0))
                };

                Utilities.WriteLine($"Container {container.Id} exists");
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

    private static IndexingPolicy ReadIndexFile(string? indexFilename)
    {
        if (indexFilename is not null && File.Exists(indexFilename))
        {
            string json = File.ReadAllText(indexFilename);
            IndexingPolicy indexingPolicy = JsonConvert.DeserializeObject<IndexingPolicy>(json);

            return indexingPolicy;
        }
        return new IndexingPolicy();
    }

    private static ContainerNewParameters LoadParams(ContainerNewParameters containerNewParams)
    {
        containerNewParams.VerboseWriteLine("Reading params from environment variables:");
        var envParams = new ContainerNewParameters();
        envParams.ReadParamsFromEnvironment();
        containerNewParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        containerNewParams.VerboseWriteLine("Argument base params:");
        containerNewParams.VerboseWriteLine(Utilities.SerializeObject(containerNewParams));
        envParams.Apply(containerNewParams);

        containerNewParams.VerboseWriteLine("Resolved to the following:");
        containerNewParams.VerboseWriteLine(Utilities.SerializeObject(envParams));
        return envParams;
    }
}