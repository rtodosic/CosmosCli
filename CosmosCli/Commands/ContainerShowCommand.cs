// Ignore Spelling: app

using Cocona;

using CosmosCli.Parameters;

using Microsoft.Azure.Cosmos;

using Newtonsoft.Json.Linq;

namespace CosmosCli.Commands;

public static class ContainerShowCommand
{
    public static async Task<int> Command(ContainerParameters containerParams)
    {
        var defaultConsoleColor = Console.ForegroundColor;
        try
        {
            containerParams.LoadParams();
            containerParams.ValidateParams();

            try
            {
                containerParams.VerboseWriteLine("Connecting to the Cosmos DB...");
                var client = new CosmosClient(containerParams.Endpoint, containerParams.Key);

                Database db = client.GetDatabase(containerParams.Database);
                Container container = db.GetContainer(containerParams.Container);

                var containerInfo = await container.ReadContainerAsync();
                JObject jsonObject = JObject.FromObject(containerInfo);

                Utilities.WriteLine(jsonObject.ToString());
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