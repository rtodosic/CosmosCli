using Cocona;
using CosmosCli.Parameters;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class AccountShowCommand
{
    public static async Task<int> Command(AccountParameters accountParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            accountParams.LoadParams();
            accountParams.ValidateParams();

            try
            {
                accountParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(accountParams.Endpoint, accountParams.Key);

                var accountInfo = await client.ReadAccountAsync();
                JObject jsonObject = JObject.FromObject(accountInfo);
                jsonObject["databases"] = new JArray();


                var databaseIterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
                while (databaseIterator.HasMoreResults)
                {
                    foreach (var databaseInfo in await databaseIterator.ReadNextAsync())
                    {
                        JObject databaseJsonObject = JObject.FromObject(databaseInfo);
                        ((JArray)jsonObject["databases"]).Add(databaseJsonObject);
                        databaseJsonObject["containers"] = new JArray();

                        Database db = client.GetDatabase(databaseInfo.Id);
                        FeedIterator<ContainerProperties> containerIterator = db.GetContainerQueryIterator<ContainerProperties>();
                        while (containerIterator.HasMoreResults)
                        {
                            foreach (var containerInfo in await containerIterator.ReadNextAsync())
                            {
                                JObject containerJsonObject = JObject.FromObject(containerInfo);
                                ((JArray)databaseJsonObject["containers"]).Add(containerJsonObject);

                            }
                        }
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