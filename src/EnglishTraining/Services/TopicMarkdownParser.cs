using System.IO;
using System.Text.RegularExpressions;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static partial class TopicMarkdownParser
{
    public static Topic Parse(string filePath)
    {
        return ParseContent(File.ReadAllText(filePath));
    }

    public static Topic ParseContent(string content)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');

        var titleLineIndex = Array.FindIndex(lines, l => l.TrimStart().StartsWith("# "));
        var title = titleLineIndex >= 0
            ? lines[titleLineIndex].TrimStart()[2..].Trim()
            : string.Empty;

        var bodyLines = titleLineIndex >= 0 ? lines.Skip(titleLineIndex + 1) : lines;
        var text = CleanBody(string.Join("\n", bodyLines));

        return new Topic { Id = Guid.NewGuid(), Title = title, Text = text };
    }

    /// <summary>
    /// Parses a file that may contain multiple articles under "### " subheadings
    /// (e.g. a day's worth of news articles under a "# yyyy-MM-dd" heading, as
    /// produced by data/2026-08-22.md). Each "### " section becomes its own topic.
    /// Falls back to the single-topic format (Parse/ParseContent) when no "### "
    /// heading is present.
    /// </summary>
    public static List<Topic> ParseMultiple(string filePath)
    {
        return ParseMultipleContent(File.ReadAllText(filePath));
    }

    public static List<Topic> ParseMultipleContent(string content)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');

        var sectionIndices = lines
            .Select((line, index) => (line, index))
            .Where(t => t.line.TrimStart().StartsWith("### "))
            .Select(t => t.index)
            .ToList();

        if (sectionIndices.Count == 0)
        {
            return [ParseContent(content)];
        }

        var topics = new List<Topic>();
        for (var i = 0; i < sectionIndices.Count; i++)
        {
            var start = sectionIndices[i];
            var end = i + 1 < sectionIndices.Count ? sectionIndices[i + 1] : lines.Length;

            var title = lines[start].TrimStart()[4..].Trim();
            var bodyLines = lines[(start + 1)..end];
            var text = CleanBody(string.Join("\n", bodyLines));

            topics.Add(new Topic { Id = Guid.NewGuid(), Title = title, Text = text });
        }

        return topics;
    }

    public static string Format(Topic topic)
    {
        return $"# {topic.Title}\n\n{topic.Text}\n";
    }

    public static void Export(Topic topic, string filePath)
    {
        File.WriteAllText(filePath, Format(topic));
    }

    /// <summary>
    /// Strips markdown link syntax (keeping only the link text) and trailing
    /// hard-break spaces, then collapses runs of blank lines to a single one.
    /// </summary>
    private static string CleanBody(string text)
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
