using System.Net;
using MotChecker.Api.Exceptions;

namespace MotChecker.Api.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An error occurred processing request {Path}",
                context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            DvsaApiException dvsaEx => (
                dvsaEx.StatusCode,
                new { error = dvsaEx.Message }
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                new { error = argEx.Message }
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                new { error = "An unexpected error occurred" }
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(response);
    }
}