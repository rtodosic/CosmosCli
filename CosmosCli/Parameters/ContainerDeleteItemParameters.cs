using Cocona;

namespace CosmosCli.Parameters;

public class ContainerDeleteItemParameters : ContainerParameters
{
    [Option('j', Description = "Compress the json output by removing whitespace and carriage returns.")]
    public bool CompressJson { get; set; }

    [Option('s', Description = "Show response statistics (including RUs, StatusCode, ContinuationToken).")]
    public bool ShowStats { get; set; }

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

    [Option("SessionToken", Description = "Set a session token in the query request.")]
    [HasDefaultValue]
    public string? SessionToken { get; set; }

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(PartitionKey))
        {
            throw new CommandExitedException("PartitionKey is required", -11);
        }

        if (PartitionKey.StartsWith('/'))
        {
            PartitionKey = PartitionKey[1..];
        }
    }
}