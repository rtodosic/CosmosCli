using Cocona;

namespace CosmosCli;

public class QueryRequestParameters : ICommandParameterSet
{
    // QueryRequestOptions
    [Option('l', Description = "Consistency Level (Eventual, ConsistentPrefix, Session, BoundedStaleness, Strong)")]
    [HasDefaultValue]
    public string? ConsistencyLevel { get; set; }

    [Option("EnableLowPrecisionOrderBy", Description = "Set low precision order by in your Azure Cosmos DB query.")]
    [HasDefaultValue]
    public bool? EnableLowPrecisionOrderBy { get; set; }

    [Option("EnableOptimisticDirectExecution", Description = "Disable direct (optimistic) execution of the query.")]
    [HasDefaultValue]
    public bool? DisableOptimisticDirectExecution { get; set; }

    [Option("EnableScanInQuery", Description = "Enable scans on the queries.")]
    [HasDefaultValue]
    public bool? EnableScanInQuery { get; set; }

    [Option('r', Description = "Exclude region(s) while querying")]
    [HasDefaultValue]
    public string[]? ExcludeRegion { get; set; }

    [Option('i', Description = "The maximum of items in an enumeration")]
    [HasDefaultValue]
    public int? MaxItemCount { get; set; }

    [Option('m', Description = "Obtain which existing indexes where used and potential new indexes")]
    [HasDefaultValue]
    public bool PopulateIndexMetrics { get; set; }

    [Option("ResponseContinuationTokenLimitInKb", Description = "The response continuation token limit in KB request option for document query requests.")]
    [HasDefaultValue]
    public int? ResponseContinuationTokenLimitInKb { get; set; }

    [Option('t', Description = "Set a session token in the query request.")]
    [HasDefaultValue]
    public string? SessionToken { get; set; }
}