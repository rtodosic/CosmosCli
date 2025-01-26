using Cocona;
using CosmosCli.Validations;

namespace CosmosCli.Parameters;

public class ContainerNewParameters : ContainerParameters
{
    [Option('p', Description = "Specify the name of the partition key.")]
    [HasDefaultValue]
    public string PartitionKey { get; set; } = "";

    [Option("autoscale", Description = "(optional) set the number of autoscaled RU/s throughput.")]
    [HasDefaultValue]
    [ValidateAutoscaleThroughput]
    public int? AutoscaleThroughput { get; set; }

    [Option("manual", Description = "(optional) set the number of manual RU/s throughput.")]
    [ValidateManualThroughput]
    [HasDefaultValue]
    public int? ManualThroughput { get; set; }


    [Option("index", Description = "(optional) file that holds the index policy.")]
    [HasDefaultValue]
    public string? IndexFilename { get; set; }

    [Option("defaultTimeToLive", Description = "The time to live on the container.")]
    [HasDefaultValue]
    public int? DefaultTimeToLive { get; set; }

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(PartitionKey))
        {
            throw new CommandExitedException("PartitionKey must be specified", -15);
        }

        if (AutoscaleThroughput is not null && ManualThroughput is not null)
        {
            throw new CommandExitedException("Autoscale and Manual throughput can not be both specified at the same time", -15);
        }

        if (!PartitionKey.StartsWith('/'))
            PartitionKey = $"/{PartitionKey}";
    }
}