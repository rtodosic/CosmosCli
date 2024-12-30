using Cocona;

using Newtonsoft.Json.Linq;

namespace CosmosCli.Parameters;

public class DatabaseParameters : AccountParameters
{
    [Option('d', Description = "The name of the database that you are creating.")]
    [HasDefaultValue]
    public string? Database { get; set; }


    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(Database))
        {
            throw new CommandExitedException("Database must be specified", -15);
        }
    }

    public override void LoadParams()
    {
        base.LoadParams();

        // Load from environment variables
        if (string.IsNullOrWhiteSpace(Database))
        {
            Database = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE");
            if (!string.IsNullOrWhiteSpace(Database))
                VerboseWriteLine($"Database - loaded from environment COSMOSDB_DATABASE: {Database}");
        }

        // Load from config file
        if (ConfigFileJson is not null && string.IsNullOrWhiteSpace(Database))
        {
            if (ConfigFileJson.TryGetValue("Database", out JToken databaseToken))
            {
                Database = databaseToken.ToString();
                if (!string.IsNullOrWhiteSpace(Database))
                    VerboseWriteLine($"Database - loaded from config file: {Database}");
            }
        }
    }
}