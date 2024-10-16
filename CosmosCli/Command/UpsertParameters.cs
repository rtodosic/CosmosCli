using Cocona;

namespace CosmosCli;

public class UpsertParameters : ICommandParameterSet
{
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


    public void ValidateParams()
    {
        if (string.IsNullOrWhiteSpace(PartitionKey) && string.IsNullOrWhiteSpace(PartitionKeyValue))
        {
            throw new CommandExitedException("PartitionKey or PartitionKeyValue is required", -15);
        }
    }
}