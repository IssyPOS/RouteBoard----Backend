using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using POSShopTicketing.Application.Common.Exceptions;
using POSShopTicketing.Domain.Exceptions;
using POSShopTicketing.Shared.Wrappers;
using ValidationException = POSShopTicketing.Application.Common.Exceptions.ValidationException;

namespace POSShopTicketing.Api.Middleware;

/// <summary>Turns exceptions raised anywhere in the pipeline into the
/// same ApiResponse envelope every other endpoint returns.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
        catch (ValidationException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.BadRequest,
                ApiResponse<object>.Failure(ex.Errors.SelectMany(e => e.Value).ToList(), "Validation failed."));
        }
        catch (NotFoundException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.NotFound,
                ApiResponse<object>.Failure(ex.Message, "Resource not found."));
        }
        catch (UnauthorizedException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.Unauthorized,
                ApiResponse<object>.Failure(ex.Message, "Authentication failed."));
        }
        catch (ForbiddenException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.Forbidden,
                ApiResponse<object>.Failure(ex.Message, "Not allowed."));
        }
        catch (DomainException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.Conflict,
                ApiResponse<object>.Failure(ex.Message, "Business rule violated."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError,
                ApiResponse<object>.Failure("An unexpected error occurred.", "Internal server error."));
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, ApiResponse<object> response)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
