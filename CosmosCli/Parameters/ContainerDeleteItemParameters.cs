using Cocona;

namespace CosmosCli.Parameters;

public class ContainerDeleteItemParameters : BaseParameters
{
    // Common Params
    [Option('d', Description = "The name of the database that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Database { get; set; }

    [Option('c', Description = "The name of the container that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Container { get; set; }

    [Option('s', Description = "Show response statistics (including RUs, StatusCode, ContinuationToken).")]
    public bool ShowResponseStats { get; set; }

    [Option('i', Description = "The name of the id property in the input JSON (if not specified 'id' will be used).")]
    [HasDefaultValue]
    public string Id { get; set; } = "id";


    [Option('p', Description = "The name of the partition key property in the input JSON.")]
    [HasDefaultValue]
    public string PartitionKey { get; set; } = "";

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
        if (string.IsNullOrWhiteSpace(Database))
        {
            throw new CommandExitedException("Please specify a database", -12);
        }
        if (string.IsNullOrWhiteSpace(Container))
        {
            throw new CommandExitedException("Please specify a container", -13);
        }
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new CommandExitedException("Id is required", -15);
        }
        if (string.IsNullOrWhiteSpace(PartitionKey))
        {
            throw new CommandExitedException("PartitionKey is required", -15);
        }
    }

    public override void Apply(BaseParameters applyParams)
    {
        base.Apply(applyParams);
        if (applyParams is ContainerDeleteItemParameters)
        {
            var deleleteParams = (ContainerDeleteItemParameters)applyParams;
            if (!string.IsNullOrWhiteSpace(deleleteParams.Database))
                this.Database = deleleteParams.Database;
            if (!string.IsNullOrWhiteSpace(deleleteParams.Container))
                this.Container = deleleteParams.Container;
            this.ShowResponseStats = deleleteParams.ShowResponseStats;
            if (!string.IsNullOrWhiteSpace(deleleteParams.Id))
                this.Id = deleleteParams.Id;
            if (!string.IsNullOrWhiteSpace(deleleteParams.PartitionKey))
                this.PartitionKey = deleleteParams.PartitionKey;
            if (deleleteParams.ConsistencyLevel is not null)
                this.ConsistencyLevel = deleleteParams.ConsistencyLevel;
            if (deleleteParams.ExcludeRegion is not null)
                this.ExcludeRegion = deleleteParams.ExcludeRegion;
            if (deleleteParams.PostTriggers is not null)
                this.PostTriggers = deleleteParams.PostTriggers;
            if (deleleteParams.PreTriggers is not null)
                this.PreTriggers = deleleteParams.PreTriggers;
            if (deleleteParams.SessionToken is not null)
                this.SessionToken = deleleteParams.SessionToken;
        }
    }
}