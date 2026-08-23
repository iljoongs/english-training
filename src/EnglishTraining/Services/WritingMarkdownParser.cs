using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class WritingMarkdownParser
{
    public static WritingEntry Parse(string filePath)
    {
        return ParseContent(File.ReadAllText(filePath));
    }

    public static WritingEntry ParseContent(string content)
    {
        var (title, fields) = LabeledTextParser.Parse(content, "설명", "예문");
        return new WritingEntry
        {
            Text = title,
            Description = fields.GetValueOrDefault("설명", string.Empty),
            Example = fields.GetValueOrDefault("예문", string.Empty),
        };
    }

    /// <summary>
    /// Parses a file with multiple "### 단어" sections (e.g. doc/sample-writings-multi.md).
    /// Each section's body becomes the Example value directly unless it uses explicit
    /// "설명:"/"예문:" labels. Falls back to the single-entry format when no "### "
    /// heading is present.
    /// </summary>
    public static List<WritingEntry> ParseMultiple(string filePath)
    {
        return ParseMultipleContent(File.ReadAllText(filePath));
    }

    public static List<WritingEntry> ParseMultipleContent(string content)
    {
        var sections = MarkdownSectionSplitter.Split(content);
        if (sections is null)
        {
            return [ParseContent(content)];
        }

        return sections.Select(s =>
        {
            if (LabeledTextParser.HasAnyLabel(s.Body, "설명", "예문"))
            {
                var fields = LabeledTextParser.Parse(s.Body, "설명", "예문").Fields;
                return new WritingEntry
                {
                    Text = s.Title,
                    Description = fields.GetValueOrDefault("설명", string.Empty),
                    Example = fields.GetValueOrDefault("예문", string.Empty),
                };
            }

            return new WritingEntry { Text = s.Title, Description = string.Empty, Example = s.Body };
        }).ToList();
    }

    public static string Format(WritingEntry entry)
    {
        return $"# {entry.Text}\n\n설명: {entry.Description}\n예문: {entry.Example}\n";
    }

    public static void Export(WritingEntry entry, string filePath)
    {
        File.WriteAllText(filePath, Format(entry));
    }

    /// <summary>
    /// Formats every entry as its own "### 단어" section (with explicit
    /// 설명/예문 labels) in one file, for the reading window's "Writing &gt;
    /// Export" (§29.4) — exports the whole list at once, not just one entry.
    /// </summary>
    public static string FormatMultiple(IReadOnlyList<WritingEntry> entries, string headerTitle = "Writing Export")
    {
        var sections = entries.Select(e => $"### {e.Text}\n설명: {e.Description}\n예문: {e.Example}\n");
        return $"# {headerTitle}\n\n{string.Join("\n", sections)}";
    }

    public static void ExportMultiple(IReadOnlyList<WritingEntry> entries, string filePath, string headerTitle = "Writing Export")
    {
        File.WriteAllText(filePath, FormatMultiple(entries, headerTitle));
    }
}
