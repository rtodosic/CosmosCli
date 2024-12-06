using Cocona;

namespace CosmosCli.Parameters;

public class ContainerSelectParameters : BaseParameters
{
    // Common Params
    [Option('d', Description = "The name of the database that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Database { get; set; }

    [Option('c', Description = "The name of the container that you are connecting to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Container { get; set; }

    // Select command params
    [Option('_', Description = "Drop system generated properties (_rid, _attachments, _etag, _self, _ts).")]
    public bool DropSystemProperties { get; set; }

    [Option('j', Description = "Compress the json output by removing whitespace and carriage returns.")]
    public bool CompressJson { get; set; }

    [Option('s', Description = "Show response statistics (including RUs, StatusCode, ContinuationToken).")]
    public bool ShowResponseStats { get; set; }

    [Option('p', Description = "The maximum number of enumerations (Default is 1).")]
    [HasDefaultValue]
    public int MaxEnumerations { get; set; } = 1;

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

    public override void Apply(BaseParameters applyParams)
    {
        base.Apply(applyParams);
        if (applyParams is ContainerSelectParameters)
        {
            var selectParams = (ContainerSelectParameters)applyParams;
            if (!string.IsNullOrWhiteSpace(selectParams.Database))
                this.Database = selectParams.Database;
            if (!string.IsNullOrWhiteSpace(selectParams.Container))
                this.Container = selectParams.Container;
            this.DropSystemProperties = selectParams.DropSystemProperties;
            this.CompressJson = selectParams.CompressJson;
            this.ShowResponseStats = selectParams.ShowResponseStats;
            this.MaxEnumerations = selectParams.MaxEnumerations;
            if (selectParams.ConsistencyLevel is not null)
                this.ConsistencyLevel = selectParams.ConsistencyLevel;
            if (selectParams.EnableLowPrecisionOrderBy is not null)
                this.EnableLowPrecisionOrderBy = selectParams.EnableLowPrecisionOrderBy;
            if (selectParams.DisableOptimisticDirectExecution is not null)
                this.DisableOptimisticDirectExecution = selectParams.DisableOptimisticDirectExecution;
            if (selectParams.EnableScanInQuery is not null)
                this.EnableScanInQuery = selectParams.EnableScanInQuery;
            if (selectParams.ExcludeRegion is not null)
                this.ExcludeRegion = selectParams.ExcludeRegion;
            if (selectParams.MaxItemCount is not null)
                this.MaxItemCount = selectParams.MaxItemCount;
            this.PopulateIndexMetrics = selectParams.PopulateIndexMetrics;
            if (selectParams.ResponseContinuationTokenLimitInKb is not null)
                this.ResponseContinuationTokenLimitInKb = selectParams.ResponseContinuationTokenLimitInKb;
            if (selectParams.SessionToken is not null)
                this.SessionToken = selectParams.SessionToken;
        }
    }


    //public static ContainerSelectParameters ReadParamsFromConfigFile()
    //{
    //    string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cosmos");
    //    string fileName = Path.Combine(filePath, "config.json");
    //    if (File.Exists(fileName))
    //    {
    //        try
    //        {
    //            string jsonString = File.ReadAllText(fileName);
    //            return JsonConvert.DeserializeObject<ContainerSelectParameters>(jsonString) ?? new ContainerSelectParameters();
    //        }
    //        catch (Exception ex)
    //        {
    //            Utilities.WarnWriteLine("Config.json file found but unable to read: " + ex.Message);
    //        }
    //    }
    //    return new ContainerSelectParameters();
    //}

    public override void ReadParamsFromEnvironment()
    {
        base.ReadParamsFromEnvironment();
        Database = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE");
        Container = Environment.GetEnvironmentVariable("COSMOSDB_CONTAINER");
    }
}