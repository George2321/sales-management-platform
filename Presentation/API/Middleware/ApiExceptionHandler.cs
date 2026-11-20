using Microsoft.AspNetCore.Diagnostics;
using SalesManagementPlatform.Application.Exceptions;
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
            ApplicationFailureException { Kind: ApplicationFailureKind.NotFound } failure =>
                (StatusCodes.Status404NotFound, failure.Title),
            ApplicationFailureException { Kind: ApplicationFailureKind.Conflict } failure =>
                (StatusCodes.Status409Conflict, failure.Title),
            ApplicationFailureException { Kind: ApplicationFailureKind.Validation } failure =>
                (StatusCodes.Status422UnprocessableEntity, failure.Title),
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
