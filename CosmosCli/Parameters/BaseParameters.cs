using Cocona;

using Newtonsoft.Json;

namespace CosmosCli.Parameters;

public class BaseParameters : ICommandParameterSet
{
    // Common Params
    [Option('e', Description = "The endpointUri used to connect to the Cosmos DB service")]
    [HasDefaultValue]
    public string? Endpoint { get; set; }
    [Option('k', Description = "The primary or secondary key used to connect to the Cosmos DB service.")]
    [HasDefaultValue]
    public string? Key { get; set; }

    [JsonIgnore]
    [Option('v', Description = "Display more details of what is happening.")]
    public bool Verbose { get; set; }


    public void VerboseWriteLine(string value)
    {
        if (Verbose)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(value);
            Console.ForegroundColor = currentColor;
        }
    }

    public virtual void Apply(BaseParameters applyParams)
    {
        if (!string.IsNullOrWhiteSpace(applyParams.Endpoint))
            this.Endpoint = applyParams.Endpoint;
        if (!string.IsNullOrWhiteSpace(applyParams.Key))
            this.Key = applyParams.Key;
        this.Verbose = applyParams.Verbose;
    }

    public virtual void ReadParamsFromEnvironment()
    {
        Endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT");
        Key = Environment.GetEnvironmentVariable("COSMOSDB_KEY");
    }

    public virtual void ValidateParams()
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
}