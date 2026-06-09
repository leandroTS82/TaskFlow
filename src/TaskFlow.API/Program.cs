using Serilog;
using TaskFlow.API.Extensions;
using TaskFlow.Application.Extensions;

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Log.Information("TaskFlow.API starting up");

app.Run();
