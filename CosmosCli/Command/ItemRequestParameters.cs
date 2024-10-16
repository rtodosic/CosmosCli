using Cocona;

namespace CosmosCli;

public class ItemRequestParameters : ICommandParameterSet
{
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
}