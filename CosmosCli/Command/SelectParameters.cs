using Cocona;

namespace CosmosCli;

public class SelectParameters : ICommandParameterSet
{
    [Option('_', Description = "Drop system generated properties (_rid, _attachments, _etag, _self, _ts).")]
    public bool DropSystemProperties { get; set; }

    [Option('j', Description = "Compress the json output by removing whitespace and carriage returns.")]
    public bool CompressJson { get; set; }

    [Option('s', Description = "Show response statistics (including RUs, StatusCode, ContinuationToken).")]
    public bool ShowResponseStats { get; set; }

    [Option('p', Description = "The maximum number of enumerations (Default is 1).")]
    [HasDefaultValue]
    public int MaxEnumerations { get; set; } = 1;
}