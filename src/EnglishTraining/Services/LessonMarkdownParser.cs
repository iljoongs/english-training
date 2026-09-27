using EnglishTraining.Models;

namespace EnglishTraining.Services;

/// <summary>
/// Parses a "lesson" file (§30) that bundles sentences, words and writing
/// entries in one markdown file, grouped by article:
/// <code>
/// ## 기사 제목
/// ### Text      → Topic (Title = 기사 제목)
/// ### Words     → "단어(품사) (해석) (표현)" lines (same as today.md, §29.1)
/// ### Writing   → "표현 | 설명 | 예문" lines ("표현 | 예문" also accepted)
/// </code>
/// A leading "- " on Words/Writing lines is ignored. Content before the first
/// "## " heading (e.g. a "# 날짜" title) is ignored.
/// </summary>
public static class LessonMarkdownParser
{
    private enum Block { None, Text, Words, Writing }

    public static (List<Topic> Topics, List<InterpretationEntry> Words, List<WritingEntry> Writings) ParseContent(string content)
    {
        var topics = new List<Topic>();
        var words = new List<InterpretationEntry>();
        var writings = new List<WritingEntry>();

        string? title = null;
        var block = Block.None;
        var text = new List<string>();
        var wordLines = new List<string>();
        var writingLines = new List<string>();

        void FlushArticle()
        {
            if (title is not null)
            {
                var body = MarkdownSectionSplitter.CleanBody(string.Join("\n", text));
                if (body.Length > 0)
                {
                    topics.Add(new Topic { Id = Guid.NewGuid(), Title = title, Text = body });
                }

                TodayEnglishParser.Parse(string.Join("\n", wordLines), out var parsedWords);
                words.AddRange(parsedWords);
                writings.AddRange(writingLines.Select(ParseWritingLine).OfType<WritingEntry>());
            }

            text.Clear();
            wordLines.Clear();
            writingLines.Clear();
            block = Block.None;
        }

        foreach (var rawLine in content.Replace("\r\n", "\n").Split('\n'))
        {
            var trimmed = rawLine.Trim();

            if (trimmed.StartsWith("## "))
            {
                FlushArticle();
                title = trimmed[3..].Trim();
                continue;
            }

            if (trimmed.StartsWith("### "))
            {
                block = ToBlock(trimmed[4..].Trim());
                continue;
            }

            switch (block)
            {
                case Block.Text:
                    text.Add(rawLine);
                    break;
                case Block.Words:
                    wordLines.Add(StripBullet(trimmed));
                    break;
                case Block.Writing:
                    writingLines.Add(StripBullet(trimmed));
                    break;
            }
        }

        FlushArticle();
        return (topics, words, writings);
    }

    private static Block ToBlock(string heading) => heading.ToLowerInvariant() switch
    {
        "text" or "본문" => Block.Text,
        "words" or "단어" => Block.Words,
        "writing" or "영작" => Block.Writing,
        _ => Block.None,
    };

    private static string StripBullet(string line) =>
        line.StartsWith("- ") ? line[2..].TrimStart() : line;

    private static WritingEntry? ParseWritingLine(string line)
    {
        var parts = line.Split('|', 3).Select(p => p.Trim()).ToArray();
        return parts.Length switch
        {
            3 when parts[0].Length > 0 => new WritingEntry { Text = parts[0], Description = parts[1], Example = parts[2] },
            2 when parts[0].Length > 0 => new WritingEntry { Text = parts[0], Example = parts[1] },
            _ => null,
        };
    }
}
