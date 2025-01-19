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

                if (await DatabaseExistsAsync(client, databaseNewParams))
                {
                    Utilities.WriteLine($"Database {databaseNewParams.Database} already exists");
                }
                else
                {
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
                        Utilities.WriteLine($"Database {db.Id} created with throughput of {throughput}");
                    else
                        Utilities.WriteLine($"Database {db.Id} created without a specified throughput");
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

    private static async Task<bool> DatabaseExistsAsync(CosmosClient client, DatabaseNewParameters databaseNewParams)
    {
        var queryDef = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", databaseNewParams.Database);

        var databaseIterator = client.GetDatabaseQueryIterator<DatabaseProperties>(queryDef);
        while (databaseIterator.HasMoreResults)
        {
            foreach (var databaseInfo in await databaseIterator.ReadNextAsync())
            {
                if (databaseInfo.Id == databaseNewParams.Database)
                    return true;
            }
        }
        return false;
    }
}