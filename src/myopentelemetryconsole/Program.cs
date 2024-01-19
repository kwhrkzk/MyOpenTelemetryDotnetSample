using domain;
using infra;
using infra.context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using myopentelemetryconsole;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Data;
using System.Diagnostics;
using usecase;

ConsoleAppBuilder builder = ConsoleApp.CreateBuilder(args);

builder.ConfigureLogging((HostBuilderContext ctxt, ILoggingBuilder builder) =>
{
    builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
    builder.AddOpenTelemetry((OpenTelemetryLoggerOptions logging) => logging.AddConsoleExporter());
})
.ConfigureServices((HostBuilderContext ctxt, IServiceCollection services) =>
{
    string environmentName =
        ctxt.Configuration.GetSection("name").Value
            ?? throw new ArgumentNullException("There is no 'name' in the appsettings.json.");

    string zipkinUrl = ctxt.Configuration.GetSection("zipkinurl").Value
            ?? throw new ArgumentNullException("There is no 'zipkinurl' in the appsettings.json.");

    services.AddSingleton<ActivitySource>(new ActivitySource(environmentName));

    services.AddDbContext<MyDbContext>();
    services.AddTransient<IOperateDB, OperateDB>();

    services.AddUsecaseDecorator();

    services.AddOpenTelemetry()
    .WithTracing((TracerProviderBuilder tracing) => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(environmentName))
        .AddSource(environmentName)
        .AddEntityFrameworkCoreInstrumentation((EntityFrameworkInstrumentationOptions op) =>
        {
            op.EnrichWithIDbCommand = (Activity activity, IDbCommand command) =>
            {
                activity.DisplayName = $"{command.CommandText[..10]}";
                activity.AddTag("db.CommandText", command.CommandText);
            };
        })
        .AddConsoleExporter()
        .AddZipkinExporter((ZipkinExporterOptions op) => op.Endpoint = new Uri(zipkinUrl))
    );
});

var app = builder.Build();

app.AddCommands<MyCommand>();
app.Run();
