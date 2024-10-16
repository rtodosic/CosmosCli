using Cocona;

using Cosmos.Command;

using CosmosCli.Command;

var builder = CoconaApp.CreateBuilder();

var app = builder.Build();
app.AddAccountCommand();
app.AddSelectCommand();
app.AddUpsertCommand();
app.Run();