using Cocona;
using CosmosCli.Validations;

namespace CosmosCli.Parameters;

public class DatabaseNewParameters : DatabaseParameters
{
    [Option("autoscale", Description = "(optional) set the number of autoscaled RU/s throughput.")]
    [HasDefaultValue]
    [ValidateAutoscaleThroughput]
    public int? AutoscaleThroughput { get; set; }

    [Option("manual", Description = "(optional) set the number of manual RU/s throughput.")]
    [HasDefaultValue]
    [ValidateManualThroughput]
    public int? ManualThroughput { get; set; }

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (AutoscaleThroughput is not null && ManualThroughput is not null)
        {
            throw new CommandExitedException("Autoscale and Manual throughput can not be both specified at the same time", -15);
        }
    }
}