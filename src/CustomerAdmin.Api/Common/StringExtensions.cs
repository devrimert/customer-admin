namespace CustomerAdmin.Api.Common;

public static class StringExtensions
{
    /// <summary>
    /// Trims the value and turns empty or whitespace-only input into null
    /// </summary>
    public static string? NullIfEmpty(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}