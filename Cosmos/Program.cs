using Cocona;
using CosmosCli.Command;

var builder = CoconaApp.CreateBuilder();
//builder.Services.AddSingleton<IdGenerator>();

var app = builder.Build();
app.AddSelectCommand();
app.AddUpsertCommand();
app.Run();
