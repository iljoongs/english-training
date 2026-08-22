using System.Text.RegularExpressions;

namespace EnglishTraining.Services;

public static partial class TextNormalizer
{
    public static string Normalize(string text)
    {
        var lower = text.ToLowerInvariant();
        var noPunctuation = NonWordOrSpaceRegex().Replace(lower, string.Empty);
        var collapsed = MultiSpaceRegex().Replace(noPunctuation, " ");
        return collapsed.Trim();
    }

    [GeneratedRegex(@"[^\w\s]")]
    private static partial Regex NonWordOrSpaceRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultiSpaceRegex();
}
