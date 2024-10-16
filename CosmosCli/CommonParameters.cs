using Cocona;

using Newtonsoft.Json;

namespace CosmosCli;

public class CommonParameters : ICommandParameterSet
{
    [Option('e', Description = "The endpointUri used to connect to the Cosmos DB service")]
    [HasDefaultValue]
    public string? Endpoint { get; set; }
    [Option('k', Description = "The primary or secondary key used to connect to the Cosmos DB service.")]
    [HasDefaultValue]
    public string? Key { get; set; }

    [Option('d', Description = "The name of the database that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Database { get; set; }

    [Option('c', Description = "The name of the container that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Container { get; set; }

    [JsonIgnore]
    [Option('v', Description = "Display more details of what is happening.")]
    public bool Verbose { get; set; }

    public void ValidateAccountParams()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new CommandExitedException("endpoint is required", -10);
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            throw new CommandExitedException(" key is required", -11);
        }
    }

    public void ValidateParams()
    {
        ValidateAccountParams();
        if (string.IsNullOrWhiteSpace(Database))
        {
            throw new CommandExitedException("Please specify a database", -12);
        }

        if (string.IsNullOrWhiteSpace(Container))
        {
            throw new CommandExitedException("Please specify a container", -13);
        }
    }

    public static CommonParameters ReadParamsFromEnvironment()
    {
        return new CommonParameters()
        {
            Endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT"),
            Key = Environment.GetEnvironmentVariable("COSMOSDB_KEY"),
            Database = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE"),
            Container = Environment.GetEnvironmentVariable("COSMOSDB_CONTAINER"),
        };
    }

    public static CommonParameters ReadParamsFromConfigFile()
    {
        string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cosmos");
        fileName = Path.Combine(fileName, "config.json");
        if (File.Exists(fileName))
        {
            try
            {
                string jsonString = File.ReadAllText(fileName);
                return JsonConvert.DeserializeObject<CommonParameters>(jsonString) ?? new CommonParameters();
            }
            catch (Exception ex)
            {
                Utilities.WarnWriteLine("Config.json file found but unable to read: " + ex.Message);
            }
        }
        return new CommonParameters();
    }
}

public static class CommonParametersExtension
{
    public static CommonParameters Apply(this CommonParameters commonParams, CommonParameters applyParams)
    {
        if (!string.IsNullOrWhiteSpace(applyParams.Endpoint))
            commonParams.Endpoint = applyParams.Endpoint;
        if (!string.IsNullOrWhiteSpace(applyParams.Key))
            commonParams.Key = applyParams.Key;
        if (!string.IsNullOrWhiteSpace(applyParams.Database))
            commonParams.Database = applyParams.Database;
        if (!string.IsNullOrWhiteSpace(applyParams.Container))
            commonParams.Container = applyParams.Container;
        commonParams.Verbose = applyParams.Verbose;
        return commonParams;
    }
}