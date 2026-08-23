using System.Text.RegularExpressions;

namespace EnglishTraining.Services;

/// <summary>
/// Splits a "daily batch" markdown file (a "# 날짜 ..." heading followed by
/// one or more "### 항목" sections) into (Title, Body) pairs, one per "### "
/// section. Used by every *MarkdownParser that supports importing several
/// entries from a single file.
/// </summary>
public static partial class MarkdownSectionSplitter
{
    public static List<(string Title, string Body)>? Split(string content)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');

        var indices = lines
            .Select((line, index) => (line, index))
            .Where(t => t.line.TrimStart().StartsWith("### "))
            .Select(t => t.index)
            .ToList();

        if (indices.Count == 0)
        {
            return null;
        }

        var sections = new List<(string, string)>();
        for (var i = 0; i < indices.Count; i++)
        {
            var start = indices[i];
            var end = i + 1 < indices.Count ? indices[i + 1] : lines.Length;

            var title = lines[start].TrimStart()[4..].Trim();
            var bodyLines = lines[(start + 1)..end];
            var body = CleanBody(string.Join("\n", bodyLines));

            sections.Add((title, body));
        }

        return sections;
    }

    /// <summary>
    /// Strips markdown link syntax (keeping only the link text) and trailing
    /// hard-break spaces, then collapses runs of blank lines to a single one.
    /// </summary>
    public static string CleanBody(string text)
    {
        var noLinks = MarkdownLinkRegex().Replace(text, "$1");
        var trimmedLines = noLinks.Split('\n').Select(l => l.TrimEnd());
        var collapsed = BlankLineRunRegex().Replace(string.Join("\n", trimmedLines), "\n\n");
        return collapsed.Trim();
    }

    [GeneratedRegex(@"\[([^\]]+)\]\([^)]*\)")]
    private static partial Regex MarkdownLinkRegex();

    [GeneratedRegex(@"\n{3,}")]
    private static partial Regex BlankLineRunRegex();
}
