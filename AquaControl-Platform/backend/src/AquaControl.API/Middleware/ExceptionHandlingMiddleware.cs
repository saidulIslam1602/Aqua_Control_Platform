using System.Net;
using System.Text.Json;
using AquaControl.Application.Common.Models;

namespace AquaControl.API.Middleware;

public class ExceptionHandlingMiddleware
{
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, error) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, 
                Error.Validation("Validation.Failed", exception.Message)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, 
                Error.Unauthorized("Auth.Unauthorized", "Unauthorized access")),
            KeyNotFoundException => (HttpStatusCode.NotFound, 
                Error.NotFound("Resource.NotFound", "The requested resource was not found")),
            InvalidOperationException => (HttpStatusCode.Conflict, 
                Error.Conflict("Operation.Invalid", exception.Message)),
            _ => (HttpStatusCode.InternalServerError, 
                Error.Failure("Server.InternalError", "An internal server error occurred"))
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            error.Code,
            error.Description,
            error.Type,
            Timestamp = DateTime.UtcNow,
            Path = context.Request.Path.Value
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

