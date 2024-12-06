using Cocona;

namespace CosmosCli.Parameters;

public class ContainerNewParameters : BaseParameters
{
    [Option('d', Description = "The name of the database.")]
    [HasDefaultValue]
    public string? Database { get; set; }

    [Option('c', Description = "The name of the container that you are creating to in Cosmos DB.")]
    [HasDefaultValue]
    public string? Container { get; set; }

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

    public override void Apply(BaseParameters applyParams)
    {
        base.Apply(applyParams);
        if (applyParams is ContainerNewParameters)
        {
            var containerParams = (ContainerNewParameters)applyParams;
            if (!string.IsNullOrWhiteSpace(containerParams.Database))
                this.Database = containerParams.Database;
            if (!string.IsNullOrWhiteSpace(containerParams.Container))
                this.Container = containerParams.Container;
            if (!string.IsNullOrEmpty(containerParams.PartitionKey))
                this.PartitionKey = containerParams.PartitionKey;
            if (containerParams.AutoscaleThroughput is not null)
                this.AutoscaleThroughput = containerParams.AutoscaleThroughput;
            if (containerParams.ManualThroughput is not null)
                this.ManualThroughput = containerParams.ManualThroughput;
            if (containerParams.IndexFilename is not null)
                this.IndexFilename = containerParams.IndexFilename;
            if (containerParams.DefaultTimeToLive is not null)
                this.DefaultTimeToLive = containerParams.DefaultTimeToLive;
        }
    }

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(Database))
        {
            throw new CommandExitedException("Database must be specified", -15);
        }
        if (string.IsNullOrWhiteSpace(Container))
        {
            throw new CommandExitedException("Container must be specified", -15);
        }
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