using System.Diagnostics;
using System.Text.Json;

namespace QuoteQuiz.API.Middleware;

public class LoggingMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "passwordHash", "token", "secret", "key", "authorization", "confirmPassword"
    };

    private const int MaxBodySizeBytes = 10 * 1024; // skip bodies larger than 10KB

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;

        await LogRequestAsync(context, method, path);

        await _next(context);

        stopwatch.Stop();

        var statusCode = context.Response.StatusCode;
        var level = statusCode >= 500 ? LogLevel.Error
            : statusCode >= 400 ? LogLevel.Warning
            : LogLevel.Information;

        _logger.Log(level, "{Method} {Path} → {StatusCode} in {ElapsedMs}ms",
            method, path, statusCode, stopwatch.ElapsedMilliseconds);
    }

    private async Task LogRequestAsync(HttpContext context, string method, string path)
    {
        var isJson = context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
        var hasBody = context.Request.ContentLength is > 0;

        if (!hasBody || !isJson)
        {
            _logger.LogInformation("→ {Method} {Path}", method, path);
            return;
        }

        if (context.Request.ContentLength > MaxBodySizeBytes)
        {
            _logger.LogInformation("→ {Method} {Path} [body omitted: {Size} bytes]",
                method, path, context.Request.ContentLength);
            return;
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var sanitized = Sanitize(rawBody);
        _logger.LogInformation("→ {Method} {Path} {Body}", method, path, sanitized);
    }

    private static string Sanitize(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var sanitized = SanitizeElement(doc.RootElement);
            return JsonSerializer.Serialize(sanitized);
        }
        catch
        {
            return "[unparseable body]";
        }
    }

    private static object? SanitizeElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                p => p.Name,
                p => SensitiveFields.Contains(p.Name) ? "****" : SanitizeElement(p.Value)!
            ),
            JsonValueKind.Array => element.EnumerateArray().Select(SanitizeElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.GetDecimal(),
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            _ => null
        };
    }
}
