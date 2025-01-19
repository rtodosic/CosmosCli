using Cocona;

namespace CosmosCli.Parameters;

public class ContainerSelectItemParameters : ContainerParameters
{
    // Select command params
    [Option('_', Description = "Drop system generated properties (_rid, _attachments, _etag, _self, _ts, _lsn).")]
    public bool DropSystemProperties { get; set; }

    [Option('j', Description = "Compress the json output by removing whitespace and carriage returns.")]
    public bool CompressJson { get; set; }

    [Option('s', Description = "Show response statistics (including RUs, StatusCode, ContinuationToken).")]
    public bool ShowStats { get; set; }

    [Option('i', Description = "The maximum number of enumerations (Default is 1).")]
    [HasDefaultValue]
    public int MaxEnumerations { get; set; } = 1;

    [Option('t', Description = "File that holds the continuation token.")]
    [HasDefaultValue]
    public string ContinuationTokenFile { get; set; } = "";


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

    [Option('n', Description = "The maximum of items in an enumeration")]
    [HasDefaultValue]
    public int? MaxItems { get; set; }

    [Option('m', Description = "Obtain which existing indexes where used and potential new indexes")]
    [HasDefaultValue]
    public bool PopulateIndexMetrics { get; set; }

    [Option("ResponseContinuationTokenLimitInKb", Description = "The response continuation token limit in KB request option for document query requests.")]
    [HasDefaultValue]
    public int? ResponseContinuationTokenLimitInKb { get; set; }

    [Option("SessionToken", Description = "Set a session token in the query request.")]
    [HasDefaultValue]
    public string? SessionToken { get; set; }

    public override void ValidateParams()
    {
        base.ValidateParams();
    }
}