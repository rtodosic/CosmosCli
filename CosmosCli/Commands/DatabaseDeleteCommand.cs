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
            databaseDeleteParams = LoadParams(databaseDeleteParams);
            databaseDeleteParams.ValidateParams();

            try
            {
                databaseDeleteParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(databaseDeleteParams.Endpoint, databaseDeleteParams.Key);

                Database db = await client.CreateDatabaseIfNotExistsAsync(databaseDeleteParams.Database);
                await db.DeleteAsync();

                Utilities.WriteLine($"Database {db.Id} deleted");
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

    private static DatabaseDeleteParameters LoadParams(DatabaseDeleteParameters databaseDeleteParams)
    {
        databaseDeleteParams.VerboseWriteLine("Reading params from environment variables:");
        var envParams = new DatabaseDeleteParameters();
        envParams.ReadParamsFromEnvironment();
        databaseDeleteParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        databaseDeleteParams.VerboseWriteLine("Argument base params:");
        databaseDeleteParams.VerboseWriteLine(Utilities.SerializeObject(databaseDeleteParams));
        envParams.Apply(databaseDeleteParams);

        databaseDeleteParams.VerboseWriteLine("Resolved to the following:");
        databaseDeleteParams.VerboseWriteLine(Utilities.SerializeObject(envParams));
        return envParams;
    }
}