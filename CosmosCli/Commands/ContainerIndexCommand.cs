using Cocona;
using CosmosCli.Parameters;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace CosmosCli.Commands;

public static class ContainerIndexCommand
{
    public static async Task<int> Command(ContainerIndexParameters containerIndexParams, [Argument] string? index = null)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            containerIndexParams.LoadParams();
            index = LoadIndex(containerIndexParams, index);

            containerIndexParams.ValidateParams();

            try
            {
                containerIndexParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(containerIndexParams.Endpoint, containerIndexParams.Key);

                Database db = await client.CreateDatabaseIfNotExistsAsync(containerIndexParams.Database);

                Container container = containerIndexParams.PartitionKey is null ?
                    await db.CreateContainerIfNotExistsAsync(containerIndexParams.Container, containerIndexParams.PartitionKey) :
                    db.GetContainer(containerIndexParams.Container);

                ContainerProperties containerProperties = await container.ReadContainerAsync();

                if (index is not null)
                {
                    containerProperties.IndexingPolicy = ConvertToIndexPolicy(index);
                    await container.ReplaceContainerAsync(containerProperties);
                }

                Utilities.WriteLine(Utilities.SerializeObject(containerProperties.IndexingPolicy));
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

    private static string? LoadIndex(ContainerIndexParameters containerIndexParams, string? index)
    {
        if (index != null)
        {
            containerIndexParams.VerboseWriteLine("Index from args:");
            containerIndexParams.VerboseWriteLine(index);
        }
        else
        {
            index = Utilities.ReadFromPipeIfNull(index);
            if (index != null)
            {

                containerIndexParams.VerboseWriteLine("Index from pipeline:");
                containerIndexParams.VerboseWriteLine(index ?? "");
            }
        }

        // if the index is a filename, load it
        if (index != null && File.Exists(index))
        {
            index = File.ReadAllText(index);
        }

        return index;
    }

    private static IndexingPolicy ConvertToIndexPolicy(string? index)
    {
        if (index is not null)
        {
            return JsonConvert.DeserializeObject<IndexingPolicy>(index) ?? new IndexingPolicy();
        }
        return new IndexingPolicy();
    }
}