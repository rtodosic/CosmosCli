using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

namespace CosmosCli.Commands;

public static class DatabaseNewCommand
{
    public static async Task<int> Command(DatabaseNewParameters databaseNewParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            databaseNewParams.LoadParams();
            databaseNewParams.ValidateParams();

            try
            {
                databaseNewParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(databaseNewParams.Endpoint, databaseNewParams.Key);

                Database db = (databaseNewParams.AutoscaleThroughput, databaseNewParams.ManualThroughput) switch
                {
                    (not null, null) => await client.CreateDatabaseIfNotExistsAsync(
                        databaseNewParams.Database,
                        ThroughputProperties.CreateAutoscaleThroughput(databaseNewParams.AutoscaleThroughput ?? 0)),
                    (null, not null) => await client.CreateDatabaseIfNotExistsAsync(
                       databaseNewParams.Database,
                       ThroughputProperties.CreateManualThroughput(databaseNewParams.ManualThroughput ?? 0)),
                    _ => await client.CreateDatabaseIfNotExistsAsync(
                       databaseNewParams.Database)
                };

                var throughput = await db.ReadThroughputAsync();
                if (throughput is not null)
                    Utilities.WriteLine($"Database {db.Id} exists with throughput of {throughput}");
                else
                    Utilities.WriteLine($"Database {db.Id} exists without a specified throughput");

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