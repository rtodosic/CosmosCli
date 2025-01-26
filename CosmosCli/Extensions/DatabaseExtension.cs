using CosmosCli.Parameters;
using Microsoft.Azure.Cosmos;

namespace CosmosCli.Extensions;

public static class DatabaseExtension
{
    public static async Task<bool> ContainerExistsAsync(this Database db, string? container, ContainerParameters param)
    {
        QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c where c.id = @name")
            .WithParameter("@name", param.Container);

        FeedIterator<ContainerProperties> containerIterator = db.GetContainerQueryIterator<ContainerProperties>(queryDefinition);
        while (containerIterator.HasMoreResults)
        {
            foreach (var containerInfo in await containerIterator.ReadNextAsync())
            {
                if (containerInfo.Id == container)
                    return true;
            }
        }
        return false;
    }
}