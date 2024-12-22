using Cocona;

namespace CosmosCli.Parameters;

public class ContainerDeleteItemParameters : ContainerParameters
{
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

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new CommandExitedException("Id is required", -15);
        }
        if (string.IsNullOrWhiteSpace(PartitionKey))
        {
            throw new CommandExitedException("PartitionKey is required", -15);
        }
    }
}