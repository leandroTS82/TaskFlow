using Serilog;
using TaskFlow.Processor.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddProcessorServices(builder.Configuration);

builder.Logging.ClearProviders();
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TaskFlow.Processor");

    if (builder.Environment.IsDevelopment())
        loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    else
        loggerConfiguration.WriteTo.Console(
            new Serilog.Formatting.Json.JsonFormatter());
});

var host = builder.Build();

Log.Information("TaskFlow.Processor starting");

await host.RunAsync();