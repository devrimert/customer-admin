namespace CustomerAdmin.Api.Common;

/// <summary>
/// Error messages used in more than one place.
/// </summary>
public static class ProblemMessages
{
    public const string ConcurrencyConflict =
        "The record was changed by someone else. Reload it and try again.";
}
