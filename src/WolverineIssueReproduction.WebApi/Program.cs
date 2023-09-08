using JasperFx.Core;
using Marten;
using Marten.NodaTimePlugin;
using Marten.Exceptions;
using Marten.Services.Json;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Npgsql;
using Oakton;
using Weasel.Core;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Marten;
using WolverineIssueReproduction.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ApplyOaktonExtensions();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.OnException<ConcurrencyException>().RetryTimes(3);
    opts.Policies
        .OnException<NpgsqlException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
    
    opts.Policies.AutoApplyTransactions();
    
    // UNCOMMENT THIS LINE TO SEE THE EXPECTED LOG MESSAGE
    //opts.Policies.UseDurableLocalQueues(); 
    
/*
info: Wolverine.Runtime.WolverineRuntime[0]
      Successfully started agent wolverinedb://default/ on node 10a3ab23-67ce-462c-9347-30146a22470b
info: WolverineIssueReproduction.Application.LogCommandHandler[0]
      HOWDY!
info: WolverineIssueReproduction.Application.LogCommand[104]
      Successfully processed message WolverineIssueReproduction.Application.LogCommand#018a755f-0274-47fd-8e7e-d77bb33cb6d5 from local://wolverineissuereproduction.application.logcommand/
*/
    
    
    opts.ApplicationAssembly = typeof(IApplicationRoot).Assembly;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMarten(opts =>
    {
        var connString = builder
            .Configuration
            .GetConnectionString("marten");

        opts.Connection(connString);
        opts.UseNodaTime();
        opts.UseDefaultSerialization(
            serializerType: SerializerType.SystemTextJson,
            enumStorage: EnumStorage.AsString,
            casing: Casing.Default
        );
    })
    .UseLightweightSessions()
    .IntegrateWithWolverine()
    .EventForwardingToWolverine();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapWolverineEndpoints();

await app.RunOaktonCommands(args);