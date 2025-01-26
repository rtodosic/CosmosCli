using CosmosCli.Parameters;
using Microsoft.Azure.Cosmos;

namespace CosmosCli.Extensions;

public static class CosmosClientExtension
{
    public static async Task<bool> DatabaseExistsAsync(this CosmosClient client, DatabaseParameters databaseParams)
    {
        var queryDef = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", databaseParams.Database);

        var databaseIterator = client.GetDatabaseQueryIterator<DatabaseProperties>(queryDef);
        while (databaseIterator.HasMoreResults)
        {
            foreach (var databaseInfo in await databaseIterator.ReadNextAsync())
            {
                if (databaseInfo.Id == databaseParams.Database)
                    return true;
            }
        }
        return false;
    }
}