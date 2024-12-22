// Ignore Spelling: app

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class DatabaseShowCommand
{
    public static async Task<int> Command(DatabaseParameters databaseParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            databaseParams.LoadParams();
            databaseParams.ValidateParams();

            try
            {
                databaseParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(databaseParams.Endpoint, databaseParams.Key);

                Database db = client.GetDatabase(databaseParams.Database);
                DatabaseProperties databaseInfo = await db.ReadAsync();

                JObject jsonObject = JObject.FromObject(databaseInfo);
                jsonObject["containers"] = new JArray();

                FeedIterator<ContainerProperties> containerIterator = db.GetContainerQueryIterator<ContainerProperties>();
                while (containerIterator.HasMoreResults)
                {
                    foreach (var containerInfo in await containerIterator.ReadNextAsync())
                    {
                        JObject containerJsonObject = JObject.FromObject(containerInfo);
                        ((JArray)jsonObject["containers"]).Add(containerJsonObject);

                    }
                }

                Utilities.WriteLine(jsonObject.ToString());
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
}