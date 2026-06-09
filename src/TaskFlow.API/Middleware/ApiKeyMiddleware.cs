using Microsoft.Extensions.Options;
using TaskFlow.API.Settings;

namespace TaskFlow.API.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly ApiSettings _settings;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiSettings> settings, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsExemptPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey))
        {
            _logger.LogWarning("API Key missing for request to {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API Key is required.");
            return;
        }

        if (!string.Equals(providedKey, _settings.ApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API Key provided for request to {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Invalid API Key.");
            return;
        }

        await _next(context);
    }

    private static bool IsExemptPath(PathString path)
    {
        return path.StartsWithSegments("/health")
            || path.StartsWithSegments("/swagger");
    }
}
