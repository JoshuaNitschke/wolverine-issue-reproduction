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
    opts.UseSystemTextJsonForSerialization(opt =>
    {
        opt.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    });
    opts.Policies.OnException<ConcurrencyException>().RetryTimes(3);
    opts.Policies
        .OnException<NpgsqlException>()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());
    
    opts.Policies.AutoApplyTransactions();
    
    // UNCOMMENT THIS LINE TO SEE THE EXPECTED LOG MESSAGE
    // opts.Policies.UseDurableLocalQueues();

    opts.ApplicationAssembly = typeof(IApplicationRoot).Assembly;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(opt => opt.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IClock>(SystemClock.Instance);


builder.Services.AddMarten(opts =>
    {
        var connString = builder
            .Configuration
            .GetConnectionString("marten");

        opts.Connection(connString);
        opts.UseDefaultSerialization(
            serializerType: SerializerType.SystemTextJson,
            enumStorage: EnumStorage.AsString,
            casing: Casing.Default
        );
        opts.UseNodaTime();
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