using Serilog;
using TaskFlow.API.Extensions;
using TaskFlow.API.Middleware;
using TaskFlow.Application.Extensions;
using TaskFlow.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TaskFlow.API")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

    if (context.HostingEnvironment.IsDevelopment())
    {
        loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {TraceId} {Message:lj}{NewLine}{Exception}");
    }
    else
    {
        loggerConfiguration.WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter());
    }
});

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
    };
});

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.MapHealthChecks("/health");

Log.Information("TaskFlow.API starting up");

await app.RunAsync();
