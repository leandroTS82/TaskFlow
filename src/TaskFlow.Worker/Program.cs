using Serilog;
using TaskFlow.Worker.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWorkerServices(builder.Configuration);

builder.Logging.ClearProviders();
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TaskFlow.Worker");

    if (builder.Environment.IsDevelopment())
        loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
    else
        loggerConfiguration.WriteTo.Console(
            new Serilog.Formatting.Json.JsonFormatter());
});

var host = builder.Build();

Log.Information("TaskFlow.Worker starting");

await host.RunAsync();