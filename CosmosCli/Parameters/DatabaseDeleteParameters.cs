using Cocona;

namespace CosmosCli.Parameters;

public class DatabaseDeleteParameters : BaseParameters
{
    [Option('d', Description = "The name of the database that you are deleting.")]
    [HasDefaultValue]
    public string? Database { get; set; }

    public override void Apply(BaseParameters applyParams)
    {
        base.Apply(applyParams);
        if (applyParams is DatabaseDeleteParameters)
        {
            var databaseParams = (DatabaseDeleteParameters)applyParams;
            if (!string.IsNullOrWhiteSpace(databaseParams.Database))
                this.Database = databaseParams.Database;
        }
    }

    public override void ValidateParams()
    {
        base.ValidateParams();
        if (string.IsNullOrWhiteSpace(Database))
        {
            throw new CommandExitedException("Database must be specified", -15);
        }
    }
}