using System.Net;
using System.Text.Json;
using ZenoBank.BuildingBlocks.Shared.Common.Exceptions;
using ZenoBank.BuildingBlocks.Shared.Common.Responses;

namespace ZenoBank.Services.Notification.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "An unexpected error occurred."
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                response.Message = validationException.Message;
                response.Errors = validationException.Errors;
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                response.Message = notFoundException.Message;
                break;

            case UnauthorizedException unauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                response.Message = unauthorizedException.Message;
                break;

            case BusinessException businessException:
                statusCode = HttpStatusCode.BadRequest;
                response.Message = businessException.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
