using Cocona;

namespace CosmosCli.Parameters;

public class ContainerIndexParameters : BaseParameters
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

    public override void Apply(BaseParameters applyParams)
    {
        base.Apply(applyParams);
        if (applyParams is ContainerIndexParameters)
        {
            var containerParams = (ContainerIndexParameters)applyParams;
            if (!string.IsNullOrWhiteSpace(containerParams.Database))
                this.Database = containerParams.Database;
            if (!string.IsNullOrWhiteSpace(containerParams.Container))
                this.Container = containerParams.Container;
            if (!string.IsNullOrEmpty(containerParams.PartitionKey))
                this.PartitionKey = containerParams.PartitionKey;
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

        if (!PartitionKey.StartsWith('/'))
            PartitionKey = $"/{PartitionKey}";
    }
}