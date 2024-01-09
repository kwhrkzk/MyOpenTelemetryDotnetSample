using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Exporter;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using System.Data;

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

    services.AddDbContext<MyDbContext>((DbContextOptionsBuilder op) => op.UseSqlite("Data Source=mysqlite.db"));

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
