// Ignore Spelling: app

using Cocona;
using Cocona.Builder;

using CosmosCli;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmos.Command;

public static class AccountCommand
{
    public static CommandConventionBuilder AddAccountCommand(this CoconaApp app)
    {
        return app.AddCommand("account", async(
            ILogger < Program > logger,
            CommonParameters commandParams,
            SelectParameters selectParams,
            QueryRequestParameters queryParams,
            [Argument] string ? query = null) =>
        {
            var defaultConsoleColor = Console.ForegroundColor;
            try
            {
                // Get common parameters
                Utilities.VerboseWriteLine(commandParams.Verbose, "Reading params from config file:");
                CommonParameters configParams = CommonParameters.ReadParamsFromConfigFile();
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(configParams));

                Utilities.VerboseWriteLine(commandParams.Verbose, "Reading params from environment variables:");
                CommonParameters envParams = CommonParameters.ReadParamsFromEnvironment();
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(envParams));

                Utilities.VerboseWriteLine(commandParams.Verbose, "Argument base params:");
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(commandParams));

                commandParams = configParams.Apply(envParams).Apply(commandParams);
                Utilities.VerboseWriteLine(commandParams.Verbose, "Resolved to the following:");
                Utilities.VerboseWriteLine(commandParams.Verbose, Utilities.SerializeObject(commandParams));

                // Validate the account parameters
                commandParams.ValidateAccountParams();

                try
                {
                    Utilities.VerboseWriteLine(commandParams.Verbose, "Connecting to the Cosmos DB...");
                    var client = new CosmosClient(commandParams.Endpoint, commandParams.Key);

                    var accountInfo = await client.ReadAccountAsync();
                    Utilities.WriteLine("");
                    Utilities.WriteLine($"Account Id: {accountInfo.Id}");
                    Utilities.WriteLine($"Consistency: {accountInfo.Consistency}");
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
        });
    }

}