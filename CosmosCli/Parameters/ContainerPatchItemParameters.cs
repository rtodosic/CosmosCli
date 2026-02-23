using Cocona;

namespace CosmosCli.Parameters;

public class ContainerPatchItemParameters : ContainerParameters
{
    // Patch Commands
    [Option('j', Description = "Compress the json output by removing whitespace and carriage returns.")]
    public bool CompressJson { get; set; }

    [Option('s', Description = "Show response statistics (including RUs, StatusCode, ContinuationToken).")]
    public bool ShowStats { get; set; }

    [Option('p', Description = "Specify the name of the partition key which is provided in the input json.")]
    [HasDefaultValue]
    public string PartitionKey { get; set; } = "";

    // ItemRequest
    [Option('l', Description = "Consistency Level (Eventual, ConsistentPrefix, Session, BoundedStaleness, Strong)")]
    [HasDefaultValue]
    public string? ConsistencyLevel { get; set; }

    [Option('r', Description = "Exclude region(s) while querying")]
    [HasDefaultValue]
    public string[]? ExcludeRegion { get; set; }

    [Option("PostTriggers", Description = "Set the names of the post trigger to be invoked after the patch.")]
    [HasDefaultValue]
    public string[]? PostTriggers { get; set; }


    [Option("PreTriggers", Description = "Set the names of the pre trigger to be invoked after the patch.")]
    [HasDefaultValue]
    public string[]? PreTriggers { get; set; }

    [Option("SessionToken", Description = "Set a session token in the query request.")]
    [HasDefaultValue]
    public string? SessionToken { get; set; }

    [Option("FilterPredicate", Description = "Set a filter predicate to be applied for patch operations.")]
    [HasDefaultValue]
    public string? FilterPredicate { get; set; }

    public override void ValidateParams()
    {
        base.ValidateParams();

        // PartitionKey is only used if we create the container. 
        // For the patch we expect the partition key to be specified in the input document.
        if (!string.IsNullOrWhiteSpace(PartitionKey) && PartitionKey.StartsWith('/'))
        {
            PartitionKey = PartitionKey[1..];
        }
    }
}