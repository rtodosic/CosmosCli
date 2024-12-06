using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json;

namespace CosmosCli.Commands;

public static class ContainerDeleteCommand
{
    public static async Task<int> Command(ContainerDeleteParameters containerDeleteParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            containerDeleteParams = LoadParams(containerDeleteParams);
            containerDeleteParams.ValidateParams();

            try
            {
                containerDeleteParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(containerDeleteParams.Endpoint, containerDeleteParams.Key);

                Database db = await client.CreateDatabaseIfNotExistsAsync(containerDeleteParams.Database);

                Container container = containerDeleteParams.PartitionKey is null ?
                    await db.CreateContainerIfNotExistsAsync(containerDeleteParams.Container, containerDeleteParams.PartitionKey) :
                    db.GetContainer(containerDeleteParams.Container);
                await container.DeleteContainerAsync();

                Utilities.WriteLine($"Container {container.Id} deleted");
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

    private static ContainerDeleteParameters LoadParams(ContainerDeleteParameters containerDeleteParams)
    {
        containerDeleteParams.VerboseWriteLine("Reading params from environment variables:");
        var envParams = new ContainerDeleteParameters();
        envParams.ReadParamsFromEnvironment();
        containerDeleteParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        containerDeleteParams.VerboseWriteLine("Argument base params:");
        containerDeleteParams.VerboseWriteLine(Utilities.SerializeObject(containerDeleteParams));
        envParams.Apply(containerDeleteParams);

        containerDeleteParams.VerboseWriteLine("Resolved to the following:");
        containerDeleteParams.VerboseWriteLine(Utilities.SerializeObject(envParams));
        return envParams;
    }
}