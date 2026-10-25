using CustomerAdmin.Api.Data;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CustomerAdmin.Api.Common;

/// <summary>
/// Turns expected database conflicts into 409 responses.
/// All other exceptions fall through to the default handler and 
/// become a generic 500 without internal details.
/// </summary>
public sealed class ConflictExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var detail = exception switch
        {
            // DbUpdateConcurrencyException derives from DbUpdateException. Check first.
            DbUpdateConcurrencyException => ProblemMessages.ConcurrencyConflict,

            DbUpdateException
            {
                InnerException: PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: DatabaseNames.OnePrimaryContactPerCustomer
                }
            } => "This customer already has a primary contact.",

            _ => null
        };

        if (detail is null)
        {
            return false;  // undetailed 500 response
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = detail
            }
        });
    }
}