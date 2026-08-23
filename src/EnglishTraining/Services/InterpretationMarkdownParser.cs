using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class InterpretationMarkdownParser
{
    public static InterpretationEntry Parse(string filePath)
    {
        return ParseContent(File.ReadAllText(filePath));
    }

    public static InterpretationEntry ParseContent(string content)
    {
        var (title, fields) = LabeledTextParser.Parse(content, "품사", "해석", "표현");
        return new InterpretationEntry
        {
            Text = title,
            PartOfSpeech = fields.GetValueOrDefault("품사", string.Empty),
            Ko = fields.GetValueOrDefault("해석", string.Empty),
            Expression = fields.GetValueOrDefault("표현", string.Empty),
        };
    }

    /// <summary>
    /// Parses a file with multiple "### 단어" sections (e.g. data/2026-08-22-word.md).
    /// Each section's body becomes the Ko value directly unless it uses an explicit
    /// "품사:"/"해석:"/"표현:" label (or "#### 품사"/"#### 해석"/"#### 표현" heading).
    /// Falls back to the single-entry format when no "### " heading is present.
    /// </summary>
    public static List<InterpretationEntry> ParseMultiple(string filePath)
    {
        return ParseMultipleContent(File.ReadAllText(filePath));
    }

    /// <summary>
    /// Import entry point used by the 단어 관리 window's "가져오기" (button/menu/drag-drop):
    /// tries the compact today.md line formats (§29.1 — "단어(품사) (해석) (표현)" or a bold
    /// sentence + translation pair) first, since a false match there is unlikely. Only falls
    /// back to the "### 단어" / labeled single-entry format (ParseMultiple) when no lines
    /// matched, so a today.md-style file doesn't collapse into one near-empty entry.
    /// </summary>
    public static List<InterpretationEntry> ParseAny(string filePath)
    {
        var content = File.ReadAllText(filePath);
        TodayEnglishParser.Parse(content, out var fromCompactFormat);
        return fromCompactFormat.Count > 0 ? fromCompactFormat : ParseMultipleContent(content);
    }

    public static List<InterpretationEntry> ParseMultipleContent(string content)
    {
        var sections = MarkdownSectionSplitter.Split(content);
        if (sections is null)
        {
            return [ParseContent(content)];
        }

        return sections.Select(s =>
        {
            if (LabeledTextParser.HasAnyLabel(s.Body, "품사", "해석", "표현"))
            {
                var fields = LabeledTextParser.Parse(s.Body, "품사", "해석", "표현").Fields;
                return new InterpretationEntry
                {
                    Text = s.Title,
                    PartOfSpeech = fields.GetValueOrDefault("품사", string.Empty),
                    Ko = fields.GetValueOrDefault("해석", string.Empty),
                    Expression = fields.GetValueOrDefault("표현", string.Empty),
                };
            }

            return new InterpretationEntry { Text = s.Title, Ko = s.Body };
        }).ToList();
    }

    public static string Format(InterpretationEntry entry)
    {
        return $"# {entry.Text}\n\n품사: {entry.PartOfSpeech}\n해석: {entry.Ko}\n표현: {entry.Expression}\n";
    }

    public static void Export(InterpretationEntry entry, string filePath)
    {
        File.WriteAllText(filePath, Format(entry));
    }

    /// <summary>
    /// Formats every entry as its own "### 단어" section (with explicit
    /// 품사/해석/표현 labels) in one file, for the reading window's "Words &gt;
    /// Export" (§29.4) — exports the whole list at once, not just one entry.
    /// </summary>
    public static string FormatMultiple(IReadOnlyList<InterpretationEntry> entries, string headerTitle = "Words Export")
    {
        var sections = entries.Select(e => $"### {e.Text}\n품사: {e.PartOfSpeech}\n해석: {e.Ko}\n표현: {e.Expression}\n");
        return $"# {headerTitle}\n\n{string.Join("\n", sections)}";
    }

    public static void ExportMultiple(IReadOnlyList<InterpretationEntry> entries, string filePath, string headerTitle = "Words Export")
    {
        File.WriteAllText(filePath, FormatMultiple(entries, headerTitle));
    }
}
