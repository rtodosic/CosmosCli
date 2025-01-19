using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

namespace CosmosCli.Commands;

public static class DatabaseDeleteCommand
{
    public static async Task<int> Command(DatabaseDeleteParameters databaseDeleteParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            databaseDeleteParams.LoadParams();
            databaseDeleteParams.ValidateParams();

            try
            {
                databaseDeleteParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(databaseDeleteParams.Endpoint, databaseDeleteParams.Key);

                if (!await DatabaseExistsAsync(client, databaseDeleteParams))
                {
                    Utilities.ErrorWriteLine($"Database {databaseDeleteParams.Database} does not exist");
                    return -1;
                }
                else
                {
                    Database db = client.GetDatabase(databaseDeleteParams.Database);
                    await db.DeleteAsync();

                    Utilities.WriteLine($"Database {db.Id} deleted");
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

    private static async Task<bool> DatabaseExistsAsync(CosmosClient client, DatabaseDeleteParameters databaseDeleteParams)
    {
        var queryDef = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", databaseDeleteParams.Database);

        var databaseIterator = client.GetDatabaseQueryIterator<DatabaseProperties>(queryDef);
        while (databaseIterator.HasMoreResults)
        {
            foreach (var databaseInfo in await databaseIterator.ReadNextAsync())
            {
                if (databaseInfo.Id == databaseDeleteParams.Database)
                    return true;
            }
        }
        return false;
    }
}