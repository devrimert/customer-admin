namespace CustomerAdmin.Api.Common;

public static class SearchPatterns
{
    /// <summary>
    /// Pass this to EF.Functions.ILike. Without it, Npgsql uses ESCAPE '' and the escaping below has no effect.
    /// </summary>
    public const string EscapeCharacter = @"\";

    /// <summary>
    /// Builds an ILIKE pattern that finds the term anywhere in the text.
    /// LIKE wildcards in the term are escaped, so "50%" is searched literally.
    /// </summary>
    public static string Contains(string term) =>
        $"%{term.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%";
}