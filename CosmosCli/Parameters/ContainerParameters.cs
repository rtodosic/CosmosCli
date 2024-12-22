﻿using Cocona;

namespace CosmosCli.Parameters;

public class ContainerParameters : DatabaseParameters
{
    [Option('c', Description = "The name of the container that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Container { get; set; }


    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(Container))
        {
            throw new CommandExitedException("Container must be specified", -16);
        }
    }

    public override void LoadParams()
    {
        base.LoadParams();

        // Load from Environment variables
        if (string.IsNullOrWhiteSpace(Container))
        {
            Container = Environment.GetEnvironmentVariable("COSMOSDB_CONTAINER");
            if (!string.IsNullOrWhiteSpace(Container))
                VerboseWriteLine($"Container - loaded from environment COSMOSDB_CONTAINER: {Container}");
        }

        // Load from config file
        if (ConfigFileJson is not null)
        {
            if (string.IsNullOrWhiteSpace(Container))
            {
                Container = ConfigFileJson["Container"].ToString();
                if (!string.IsNullOrWhiteSpace(Container))
                    VerboseWriteLine($"Container - loaded from config file: {Container}");
            }
        }
    }
}