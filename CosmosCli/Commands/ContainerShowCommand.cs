using Cocona;
using CosmosCli.Parameters;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerShowCommand
{
    public static async Task<int> Command(ContainerParameters containerParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            containerParams.LoadParams();
            containerParams.ValidateParams();

            try
            {
                containerParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(containerParams.Endpoint, containerParams.Key);

                Database db = client.GetDatabase(containerParams.Database);

                QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c where c.id = @name")
                    .WithParameter("@name", containerParams.Container);

                FeedIterator<ContainerProperties> containerIterator = db.GetContainerQueryIterator<ContainerProperties>(queryDefinition);

                while (containerIterator.HasMoreResults)
                {
                    foreach (var containerInfo in await containerIterator.ReadNextAsync())
                    {
                        JObject jsonObject = JObject.FromObject(containerInfo);
                        Utilities.WriteLine(jsonObject.ToString());
                        return 0;
                    }
                }

                Utilities.ErrorWriteLine($"Container {containerParams.Container} not found in database {containerParams.Database}");
                return -1;
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
    }
}