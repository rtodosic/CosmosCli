using Cocona;

namespace CosmosCli.Parameters;

public class DatabaseNewParameters : BaseParameters
{
    [Option('d', Description = "The name of the database that you are creating.")]
    [HasDefaultValue]
    public string? Database { get; set; }

    [Option("autoscale", Description = "(optional) set the number of autoscaled RU/s throughput.")]
    [HasDefaultValue]
    public int? AutoscaleThroughput { get; set; }

    [Option("manual", Description = "(optional) set the number of manual RU/s throughput.")]
    [HasDefaultValue]
    public int? ManualThroughput { get; set; }

    public override void Apply(BaseParameters applyParams)
    {
        base.Apply(applyParams);
        if (applyParams is DatabaseNewParameters)
        {
            var databaseParams = (DatabaseNewParameters)applyParams;
            if (!string.IsNullOrWhiteSpace(databaseParams.Database))
                this.Database = databaseParams.Database;
            if (databaseParams.AutoscaleThroughput is not null)
                this.AutoscaleThroughput = databaseParams.AutoscaleThroughput;
            if (databaseParams.ManualThroughput is not null)
                this.ManualThroughput = databaseParams.ManualThroughput;
        }
    }

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(Database))
        {
            throw new CommandExitedException("Database must be specified", -15);
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
    }
}