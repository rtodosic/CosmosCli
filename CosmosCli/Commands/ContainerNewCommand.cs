﻿using Cocona;

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
            containerNewParams.LoadParams();
            containerNewParams.ValidateParams();

            try
            {
                containerNewParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(containerNewParams.Endpoint, containerNewParams.Key);

                Database db = await client.CreateDatabaseIfNotExistsAsync(containerNewParams.Database);
                if (await ContainerExistsAsync(db, containerNewParams.Container, containerNewParams))
                {
                    Utilities.WriteLine($"Container {containerNewParams.Container} already exists in database {containerNewParams.Database}");
                }
                else
                {
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
                          })
                    };

                    Utilities.WriteLine($"Container {container.Id} created in database {db.Id}");
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

    private static async Task<bool> ContainerExistsAsync(Database db, string? container, ContainerNewParameters param)
    {
        QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c where c.id = @name")
            .WithParameter("@name", param.Container);

        FeedIterator<ContainerProperties> containerIterator = db.GetContainerQueryIterator<ContainerProperties>(queryDefinition);
        while (containerIterator.HasMoreResults)
        {
            foreach (var containerInfo in await containerIterator.ReadNextAsync())
            {
                if (containerInfo.Id == container)
                    return true;
            }
        }

        return false;
    }
}