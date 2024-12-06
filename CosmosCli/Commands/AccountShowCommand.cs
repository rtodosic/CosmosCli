// Ignore Spelling: app

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

namespace CosmosCli.Commands;

public static class AccountShowCommand
{
    public static async Task<int> Command(AccountShowParameters accountParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            accountParams = LoadParams(accountParams);
            accountParams.ValidateParams();

            try
            {
                accountParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(accountParams.Endpoint, accountParams.Key);

                var accountInfo = await client.ReadAccountAsync();
                Utilities.WriteLine("");
                Utilities.WriteLine($"Account Id: {accountInfo.Id}");
                Utilities.WriteLine($"Consistency.DefaultConsistencyLevel: {accountInfo.Consistency.DefaultConsistencyLevel}");
                Utilities.WriteLine($"Consistency.MaxStalenessIntervalInSeconds: {accountInfo.Consistency.MaxStalenessIntervalInSeconds}");
                Utilities.WriteLine($"Consistency.MaxStalenessPrefix: {accountInfo.Consistency.MaxStalenessPrefix}");
                Utilities.WriteLine($"ETag: {accountInfo.ETag}");
                if (accountInfo.ReadableRegions.Any())
                {
                    Utilities.WriteLine("ReadableRegions:");
                    foreach (var region in accountInfo.ReadableRegions)
                    {
                        Utilities.WriteLine($"   - {region.Name}");
                        Utilities.WriteLine($"     {region.Endpoint}");
                    }
                }
                if (accountInfo.WritableRegions.Any())
                {
                    Utilities.WriteLine("WritableRegions:");
                    foreach (var region in accountInfo.WritableRegions)
                    {
                        Utilities.WriteLine($"   - {region.Name}");
                        Utilities.WriteLine($"     {region.Endpoint}");
                    }
                }

                var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
                while (iterator.HasMoreResults)
                {
                    Utilities.WriteLine("");
                    Utilities.WriteLine("Database:");
                    foreach (var database in await iterator.ReadNextAsync())
                    {
                        Utilities.WriteLine($"   - Database Id: {database.Id}");
                        Utilities.WriteLine($"     SelfLink: {database.SelfLink}");
                        Utilities.WriteLine($"     ETag: {database.ETag}");
                        Utilities.WriteLine($"     LastModified: {database.LastModified}");
                    }
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

    private static AccountShowParameters LoadParams(AccountShowParameters accountParams)
    {
        accountParams.VerboseWriteLine("Reading params from environment variables:");
        var envParams = new AccountShowParameters();
        envParams.ReadParamsFromEnvironment();
        accountParams.VerboseWriteLine(Utilities.SerializeObject(envParams));

        accountParams.VerboseWriteLine("Argument base params:");
        accountParams.VerboseWriteLine(Utilities.SerializeObject(accountParams));
        envParams.Apply(accountParams);

        accountParams.VerboseWriteLine("Resolved to the following:");
        accountParams.VerboseWriteLine(Utilities.SerializeObject(envParams));
        return envParams;
    }
}