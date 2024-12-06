using Cocona;

namespace CosmosCli.Parameters;

public class ContainerUpsertParameters : BaseParameters
{
    // Common Params

    [Option('d', Description = "The name of the database that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Database { get; set; }

    [Option('c', Description = "The name of the container that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Container { get; set; }

    // Upsert Commands
    [Option('j', Description = "Compress the json output by removing whitespace and carriage returns.")]
    public bool CompressJson { get; set; }

    [Option('s', Description = "Show response statistics (including RUs, StatusCode, ContinuationToken).")]
    public bool ShowResponseStats { get; set; }

    [Option('p', Description = "Specify the name of the partition key which is provided in the input json.")]
    [HasDefaultValue]
    public string PartitionKey { get; set; } = "";

    [Option('a', Description = "Specify the partition key value which will be used during the upsert.")]
    [HasDefaultValue]
    public string PartitionKeyValue { get; set; } = "";

    [Option('i', Description = "After upsert don't display the saved items.")]
    [HasDefaultValue]
    public bool HideResults { get; set; }

    // ItemRequest
    [Option('l', Description = "Consistency Level (Eventual, ConsistentPrefix, Session, BoundedStaleness, Strong)")]
    [HasDefaultValue]
    public string? ConsistencyLevel { get; set; }

    [Option('r', Description = "Exclude region(s) while querying")]
    [HasDefaultValue]
    public string[]? ExcludeRegion { get; set; }

    [Option("PostTriggers", Description = "Set the names of the post trigger to be invoked after the upsert.")]
    [HasDefaultValue]
    public string[]? PostTriggers { get; set; }


    [Option("PreTriggers", Description = "Set the names of the pre trigger to be invoked after the upsert.")]
    [HasDefaultValue]
    public string[]? PreTriggers { get; set; }

    [Option('t', Description = "Set a session token in the query request.")]
    [HasDefaultValue]
    public string? SessionToken { get; set; }

    public override void ReadParamsFromEnvironment()
    {
        base.ReadParamsFromEnvironment();
        Database = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE");
        Container = Environment.GetEnvironmentVariable("COSMOSDB_CONTAINER");
    }

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(PartitionKey) && string.IsNullOrWhiteSpace(PartitionKeyValue))
        {
            throw new CommandExitedException("PartitionKey or PartitionKeyValue is required", -15);
        }
    }

    public override void Apply(BaseParameters applyParams)
    {
        base.Apply(applyParams);
        if (applyParams is ContainerUpsertParameters)
        {
            var upsertParams = (ContainerUpsertParameters)applyParams;
            if (!string.IsNullOrWhiteSpace(upsertParams.Database))
                this.Database = upsertParams.Database;
            if (!string.IsNullOrWhiteSpace(upsertParams.Container))
                this.Container = upsertParams.Container;
            this.CompressJson = upsertParams.CompressJson;
            this.ShowResponseStats = upsertParams.ShowResponseStats;
            if (!string.IsNullOrWhiteSpace(upsertParams.PartitionKey))
                this.PartitionKey = upsertParams.PartitionKey;
            if (!string.IsNullOrWhiteSpace(upsertParams.PartitionKeyValue))
                this.PartitionKeyValue = upsertParams.PartitionKeyValue;
            this.HideResults = upsertParams.HideResults;
            if (upsertParams.ConsistencyLevel is not null)
                this.ConsistencyLevel = upsertParams.ConsistencyLevel;
            if (upsertParams.ExcludeRegion is not null)
                this.ExcludeRegion = upsertParams.ExcludeRegion;
            if (upsertParams.PostTriggers is not null)
                this.PostTriggers = upsertParams.PostTriggers;
            if (upsertParams.PreTriggers is not null)
                this.PreTriggers = upsertParams.PreTriggers;
            if (upsertParams.SessionToken is not null)
                this.SessionToken = upsertParams.SessionToken;
        }
    }
}