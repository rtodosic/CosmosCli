using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

namespace CosmosCli.Commands;

public static class ContainerDeleteCommand
{
    public static async Task<int> Command(ContainerDeleteParameters containerDeleteParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            containerDeleteParams.LoadParams();
            containerDeleteParams.ValidateParams();

            try
            {
                containerDeleteParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(containerDeleteParams.Endpoint, containerDeleteParams.Key);
                Database db = await client.CreateDatabaseIfNotExistsAsync(containerDeleteParams.Database);

                if (await ContainerExistsAsync(db, containerDeleteParams.Container, containerDeleteParams))
                {
                    Container container = db.GetContainer(containerDeleteParams.Container);
                    await container.DeleteContainerAsync();

                    Utilities.WriteLine($"Container {container.Id} deleted");
                }
                else
                {
                    Utilities.ErrorWriteLine($"Container {containerDeleteParams.Container} not found in database {containerDeleteParams.Database}");
                    return -1;
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

    private static async Task<bool> ContainerExistsAsync(Database db, string? container, ContainerDeleteParameters param)
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