using Cocona;

namespace CosmosCli.Parameters;

public class ContainerDeleteParameters : ContainerParameters
{
    [Option('p', Description = "Specify the name of the partition key.")]
    [HasDefaultValue]
    public string PartitionKey { get; set; } = "";

    public override void ValidateParams()
    {
        base.ValidateParams();

        if (!PartitionKey.StartsWith('/'))
            PartitionKey = $"/{PartitionKey}";
    }
}