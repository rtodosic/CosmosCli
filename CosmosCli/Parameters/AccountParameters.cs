using Cocona;
using CosmosCli.Validations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CosmosCli.Parameters;

public class AccountParameters : ICommandParameterSet
{
    [Option('e', Description = "The endpointUri used to connect to the Cosmos DB service")]
    [ValidateEndpoint]
    [HasDefaultValue]
    public string? Endpoint { get; set; }

    [Option('k', Description = "The primary or secondary key used to connect to the Cosmos DB service.")]
    [HasDefaultValue]
    [ValidateKey]
    [JsonIgnore]
    public string? Key { get; set; }

    [JsonIgnore]
    [Option('v', Description = "Display more details of what is happening.")]
    public bool Verbose { get; set; }

    [JsonIgnore]
    internal JObject? ConfigFileJson { get; set; }

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

    public virtual void ValidateParams()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new CommandExitedException($"{nameof(Endpoint)} is required", -10);
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            throw new CommandExitedException($"{nameof(Key)} is required", -10);
        }

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out Uri? endpointUri) ||
            (endpointUri.Scheme != Uri.UriSchemeHttp && endpointUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new CommandExitedException($"{nameof(Endpoint)} is not a valid HTTP or HTTPS URI: '{Endpoint}'", -10);
        }

        if (Key.Any(char.IsWhiteSpace))
        {
            throw new CommandExitedException($"{nameof(Key)} is not valid Base64", -10);
        }

        try
        {
            _ = Convert.FromBase64String(Key);
        }
        catch (FormatException)
        {
            throw new CommandExitedException($"{nameof(Key)} is not valid Base64", -10);
        }
    }

    public virtual void LoadParams()
    {
        // Load from Environment
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            Endpoint = Environment.GetEnvironmentVariable("COSMOSDB_ENDPOINT");
            if (!string.IsNullOrWhiteSpace(Endpoint))
                VerboseWriteLine($"Endpoint - loaded from environment COSMOSDB_ENDPOINT: {Endpoint}");
        }

        if (string.IsNullOrWhiteSpace(Key))
        {
            Key = Environment.GetEnvironmentVariable("COSMOSDB_KEY");
            if (!string.IsNullOrWhiteSpace(Key))
                VerboseWriteLine($"Key - loaded from environment COSMOSDB_KEY");
        }

        ReadConfigFile();

        // Load from config file
        if (ConfigFileJson is not null)
        {
            if (string.IsNullOrWhiteSpace(Endpoint))
            {
                if (ConfigFileJson.TryGetValue(nameof(Endpoint), out JToken? endpointToken))
                {
                    Endpoint = endpointToken.ToString();
                    if (!string.IsNullOrWhiteSpace(Endpoint))
                        VerboseWriteLine($"Endpoint - loaded from config file: {Endpoint}");
                }
            }

            if (string.IsNullOrWhiteSpace(Key))
            {
                if (ConfigFileJson.TryGetValue(nameof(Key), out JToken? keyToken))
                {
                    Key = keyToken.ToString();
                    if (!string.IsNullOrWhiteSpace(Key))
                        VerboseWriteLine($"Key - loaded from config file: {Key}");
                }
            }
        }
    }


    public void ReadConfigFile()
    {
        string fileName = "config.json";

        // Try from the current fold first.
        // If we don't find the file, try the global config file.
        if (!File.Exists(fileName))
        {
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cosmos");
            fileName = Path.Combine(filePath, "config.json");
        }

        if (File.Exists(fileName))
        {
            try
            {
                string jsonString = File.ReadAllText(fileName);
                ConfigFileJson = JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                Utilities.WarnWriteLine($"Unable to parse file {fileName}: {ex.Message}");
            }
        }
    }
}