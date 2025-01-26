using Cocona;
using CosmosCli.Parameters;
using Microsoft.Azure.Cosmos;

namespace CosmosCli.Commands;

public static class DatabaseUpdateCommand
{
    public static async Task<int> Command(DatabaseUpdateParameters databaseUpdateParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            databaseUpdateParams.LoadParams();
            databaseUpdateParams.ValidateParams();

            try
            {
                databaseUpdateParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(databaseUpdateParams.Endpoint, databaseUpdateParams.Key);

                Database db;

                if (databaseUpdateParams.AutoscaleThroughput is not null)
                {
                    db = await client.CreateDatabaseIfNotExistsAsync(
                        databaseUpdateParams.Database,
                        ThroughputProperties.CreateAutoscaleThroughput(databaseUpdateParams.AutoscaleThroughput ?? 0));
                    await db.ReplaceThroughputAsync(ThroughputProperties.CreateAutoscaleThroughput(databaseUpdateParams.AutoscaleThroughput ?? 0));
                }
                else if (databaseUpdateParams.ManualThroughput is not null)
                {
                    db = await client.CreateDatabaseIfNotExistsAsync(
                       databaseUpdateParams.Database,
                       ThroughputProperties.CreateManualThroughput(databaseUpdateParams.ManualThroughput ?? 0));
                    await db.ReplaceThroughputAsync(ThroughputProperties.CreateManualThroughput(databaseUpdateParams.ManualThroughput ?? 0));
                }
                else
                {
                    db = await client.CreateDatabaseIfNotExistsAsync(
                        databaseUpdateParams.Database);
                    await db.ReplaceThroughputAsync(null);
                }

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