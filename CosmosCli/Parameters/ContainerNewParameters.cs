using Cocona;

namespace CosmosCli.Parameters;

public class ContainerNewParameters : ContainerParameters
{
    [Option('p', Description = "Specify the name of the partition key.")]
    [HasDefaultValue]
    public string PartitionKey { get; set; } = "";


    [Option("autoscale", Description = "(optional) set the number of autoscaled RU/s throughput.")]
    [HasDefaultValue]
    public int? AutoscaleThroughput { get; set; }

    [Option("manual", Description = "(optional) set the number of manual RU/s throughput.")]
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
        if (AutoscaleThroughput is not null && AutoscaleThroughput < 1000)
        {
            throw new CommandExitedException($"Autoscale throughput values must be between 1000 and 1000000 inclusive in increments of 1000. {AutoscaleThroughput} is less than required minimum throughput of 1000.", -15);
        }
        if (AutoscaleThroughput is not null && AutoscaleThroughput > 1000000)
        {
            throw new CommandExitedException($"Autoscale throughput values must be between 1000 and 1000000 inclusive in increments of 1000. {AutoscaleThroughput} is greater than required maximum throughput of 1000000.", -15);
        }
        if (AutoscaleThroughput is not null && AutoscaleThroughput % 1000 != 0)
        {
            throw new CommandExitedException($"Autoscale throughput values must be between 1000 and 1000000 inclusive in increments of 1000. {AutoscaleThroughput} is not an increment of 1000.", -15);
        }
        if (ManualThroughput is not null && ManualThroughput < 400)
        {
            throw new CommandExitedException($"Manual throughput values must be between 400 and 1000000 inclusive in increments of 100. {ManualThroughput} is less than required minimum throughput of 400.", -15);
        }
        if (ManualThroughput is not null && ManualThroughput > 1000000)
        {
            throw new CommandExitedException($"Manual throughput values must be between 400 and 1000000 inclusive in increments of 100. {ManualThroughput} is greater than required maximum throughput of 1000000.", -15);
        }
        if (ManualThroughput is not null && ManualThroughput % 100 != 0)
        {
            throw new CommandExitedException($"Manual throughput values must be between 400 and 1000000 inclusive in increments of 100. {ManualThroughput} is not an increment of 100.", -15);
        }

        if (!PartitionKey.StartsWith('/'))
            PartitionKey = $"/{PartitionKey}";
    }
}