﻿using Cocona;
using CosmosCli.Extensions;
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
                if (await db.ContainerExistsAsync(containerNewParams.Container, containerNewParams))
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
                                IndexingPolicy = ReadIndexFile(containerNewParams.IndexFilename),
                                //PartitionKeyDefinitionVersion
                                //PartitionKeyPaths
                                //SelfLink
                                //TimeToLivePropertyPath
                                //UniqueKeyPolicy
                                VectorEmbeddingPolicy = ReadVectorFile(containerNewParams.VectorFilename)
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
                               IndexingPolicy = ReadIndexFile(containerNewParams.IndexFilename),
                               //PartitionKeyDefinitionVersion
                               //PartitionKeyPaths
                               //SelfLink
                               //TimeToLivePropertyPath
                               //UniqueKeyPolicy
                               VectorEmbeddingPolicy = ReadVectorFile(containerNewParams.VectorFilename)
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
                              IndexingPolicy = ReadIndexFile(containerNewParams.IndexFilename),
                              //PartitionKeyDefinitionVersion
                              //PartitionKeyPaths
                              //SelfLink
                              //TimeToLivePropertyPath
                              //UniqueKeyPolicy
                              VectorEmbeddingPolicy = ReadVectorFile(containerNewParams.VectorFilename)
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
            return JsonConvert.DeserializeObject<IndexingPolicy>(json) ?? new IndexingPolicy();
        }
        return new IndexingPolicy();
    }

    private static VectorEmbeddingPolicy? ReadVectorFile(string? vectorFilename)
    {
        if (vectorFilename is not null && File.Exists(vectorFilename))
        {
            string json = File.ReadAllText(vectorFilename);
            return JsonConvert.DeserializeObject<VectorEmbeddingPolicy>(json);
        }
        return null;
    }
}