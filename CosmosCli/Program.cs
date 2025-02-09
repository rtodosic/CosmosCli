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
    a.AddCommand("show", DatabaseShowCommand.Command);
    a.AddCommand("new", DatabaseNewCommand.Command);
    a.AddCommand("update", DatabaseUpdateCommand.Command);
    a.AddCommand("delete", DatabaseDeleteCommand.Command);
});

app.AddSubCommand("container", a =>
{
    a.AddCommand("show", ContainerShowCommand.Command);
    a.AddCommand("new", ContainerNewCommand.Command);
    a.AddCommand("delete", ContainerDeleteCommand.Command);
    a.AddCommand("index", ContainerIndexCommand.Command);
    a.AddCommand("select-item", ContainerSelectItemCommand.Command);
    a.AddCommand("upsert-item", ContainerUpsertItemCommand.Command);
    a.AddCommand("delete-item", ContainerDeleteItemCommand.Command);
});

app.Run();