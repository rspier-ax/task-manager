using System.Net;
using System.Text.Json;
using TaskManager.Application.Exceptions;

namespace TaskManager.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (status, message) = exception switch
        {
            ValidationException ve => (HttpStatusCode.BadRequest, ve.Message),
            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message),
            ConflictException ce => (HttpStatusCode.Conflict, ce.Message),
            UnauthorizedAccessException uae => (HttpStatusCode.Unauthorized, uae.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new { error = message }, JsonOptions));
    }
}
