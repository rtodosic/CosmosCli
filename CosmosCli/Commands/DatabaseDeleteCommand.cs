﻿using Cocona;

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
}