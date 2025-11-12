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
                AquaControl.Application.Common.Models.Error.Validation("Validation.Failed", exception.Message)),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, 
                AquaControl.Application.Common.Models.Error.Unauthorized("Auth.Unauthorized", "Unauthorized access")),
            KeyNotFoundException => (HttpStatusCode.NotFound, 
                AquaControl.Application.Common.Models.Error.NotFound("Resource.NotFound", "The requested resource was not found")),
            InvalidOperationException => (HttpStatusCode.Conflict, 
                AquaControl.Application.Common.Models.Error.Conflict("Operation.Invalid", exception.Message)),
            _ => (HttpStatusCode.InternalServerError, 
                AquaControl.Application.Common.Models.Error.Failure("Server.InternalError", "An internal server error occurred"))
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            Code = error.Code,
            Description = error.Description,
            Type = error.Type,
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

