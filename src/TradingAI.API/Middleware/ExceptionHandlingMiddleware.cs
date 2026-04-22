using System.Net;
using System.Text.Json;
using TradingAI.Application.Common.Exceptions;

namespace TradingAI.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception err)
        {
            logger.LogError(err, "An unhandled exception occurred during the request.");
            await HandleExceptionAsync(context, err);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = exception switch
        {
            ValidationException     => HttpStatusCode.BadRequest,    // 400
            UnauthorizedException   => HttpStatusCode.Unauthorized,  // 401
            NotFoundException       => HttpStatusCode.NotFound,      // 404
            ConflictException       => HttpStatusCode.Conflict,      // 409
            _                       => HttpStatusCode.InternalServerError // 500
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        object payload = exception switch
        {
            ValidationException ve => new
            {
                status = (int)code,
                message = ve.Message,
                errors = ve.Errors
            },
            ConflictException or NotFoundException or UnauthorizedException => new
            {
                status = (int)code,
                message = exception.Message
            },
            _ => new
            {
                status = (int)code,
                message = "An unexpected error occurred."
            }
        };

        var result = JsonSerializer.Serialize(payload);
        return context.Response.WriteAsync(result);
    }
}
