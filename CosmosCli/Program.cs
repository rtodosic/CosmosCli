using Cocona;

using CosmosCli.Commands;

var builder = CoconaLiteApp.CreateBuilder();

var app = builder.Build();

app.AddSubCommand("account", a =>
{
    a.AddCommand("show", AccountShowCommand.Command);
});

app.AddSubCommand("database", a =>
{
    a.AddCommand("new", DatabaseNewCommand.Command);
    a.AddCommand("update", DatabaseUpdateCommand.Command);
    a.AddCommand("delete", DatabaseDeleteCommand.Command);
});

app.AddSubCommand("container", a =>
{
    a.AddCommand("new", ContainerNewCommand.Command);
    a.AddCommand("delete", ContainerDeleteCommand.Command);
    a.AddCommand("index", ContainerIndexCommand.Command);
    a.AddCommand("select", ContainerSelectCommand.Command);
    a.AddCommand("upsert", ContainerUpsertCommand.Command);
});

app.Run();