using Microsoft.AspNetCore.Diagnostics;
using SalesManagementPlatform.Application.Orders.Exceptions;
using SalesManagementPlatform.Domain.Exceptions;

namespace SalesManagementPlatform.Api.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            OrderNotFoundException =>
                (StatusCodes.Status404NotFound, "Order not found"),
            DuplicateOrderReferenceException =>
                (StatusCodes.Status409Conflict, "Order reference already exists"),
            OrderStatusTransitionException or OrderStatusConflictException =>
                (StatusCodes.Status409Conflict, "Order status could not be saved"),
            DomainRuleException =>
                (StatusCodes.Status422UnprocessableEntity, "Validation failed"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Request could not be completed")
        };

        string detail;
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unexpected API error for {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
            detail = "Something went wrong. Please try again.";
        }
        else
        {
            detail = exception.Message;
        }

        await Results
            .Problem(
                statusCode: statusCode,
                title: title,
                detail: detail)
            .ExecuteAsync(httpContext);

        return true;
    }
}
