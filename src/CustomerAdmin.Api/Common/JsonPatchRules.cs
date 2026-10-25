using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson.Operations;
using Microsoft.AspNetCore.Mvc;

namespace CustomerAdmin.Api.Common;

/// <summary>
/// Rules every PATCH endpoint in this API follows (RFC 6902, status codes per RFC 5789):
/// the first operation tests the version, after that only "replace" is allowed, the version is read-only.
/// </summary>
public static class JsonPatchRules
{
    public const string VersionPath = "/version";

    /// <summary>
    /// Checks the operations before anything is loaded. Returns null if the patch is fine.
    /// </summary>
    public static ActionResult? CheckOperations<T>(this ControllerBase controller, JsonPatchDocument<T> patch)
        where T : class
    {
        var operations = patch.Operations;

        // First get test. Paths are case-sensitive (JSON Pointer), "/Version" doesn't count.
        if (operations.Count == 0 ||
            operations[0] is not { OperationType: OperationType.Test, path: VersionPath })
        {
            return controller.Problem(
                statusCode: StatusCodes.Status428PreconditionRequired,
                title: "Precondition required",
                detail: $"The first operation must be a 'test' on '{VersionPath}'.");
        }

        // Only 'replace' is allowed. The version itself can't be changed by the client.
        var hasInvalidOperation = operations
            .Skip(1)
            .Any(o => o.OperationType != OperationType.Replace || o.path == VersionPath);

        return hasInvalidOperation
            ? controller.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Unsupported operation",
                detail: $"Only 'replace' is allowed after the version test, and '{VersionPath}' can't be replaced.")
            : null;
    }

    /// <summary>
    /// Applies the patch to a copy of the current values and validates it with the same rules as PUT.
    /// Returns an error response, or null if the copy can be saved.
    /// </summary>
    public static ActionResult? ApplyAndValidate<T>(this ControllerBase controller, JsonPatchDocument<T> patch, T target)
        where T : class
    {
        // ApplyTo reports errors through the callback. On any error the copy is thrown away.
        var errors = new List<JsonPatchError>();
        patch.ApplyTo(target, errors.Add);

        if (errors.Any(e => e.Operation?.OperationType == OperationType.Test))
        {
            return controller.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ProblemMessages.ConcurrencyConflict);
        }

        if (errors.Count > 0)
        {
            return controller.Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Invalid patch",
                detail: string.Join(" ", errors.Select(e => e.ErrorMessage)));
        }

        if (!controller.TryValidateModel(target))
        {
            return controller.ValidationProblem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                modelStateDictionary: controller.ModelState);
        }

        return null;
    }
}