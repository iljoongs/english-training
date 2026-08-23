using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class TopicMarkdownParser
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
        var text = MarkdownSectionSplitter.CleanBody(string.Join("\n", bodyLines));

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
        var sections = MarkdownSectionSplitter.Split(content);
        if (sections is null)
        {
            return [ParseContent(content)];
        }

        return sections
            .Select(s => new Topic { Id = Guid.NewGuid(), Title = s.Title, Text = s.Body })
            .ToList();
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
    /// Formats every topic as its own "### 제목" section in one file (the
    /// inverse of ParseMultiple), for the reading window's "Sentences &gt;
    /// Export" (§29.4) — exports the whole list at once, not just one topic.
    /// </summary>
    public static string FormatMultiple(IReadOnlyList<Topic> topics, string headerTitle = "Sentences Export")
    {
        var sections = topics.Select(t => $"### {t.Title}\n{t.Text}\n");
        return $"# {headerTitle}\n\n{string.Join("\n", sections)}";
    }

    public static void ExportMultiple(IReadOnlyList<Topic> topics, string filePath, string headerTitle = "Sentences Export")
    {
        File.WriteAllText(filePath, FormatMultiple(topics, headerTitle));
    }
}
