using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

/// <summary>
/// Parses a lesson file (§30) — a "## 기사 제목" article followed by its
/// "### Text"/"### Words"/"### Writing"/"### Translation" sections (Korean aliases "본문"/"단어"/
/// "영작" also recognized) — into the Topic/InterpretationEntry/WritingEntry
/// lists that feed the reading window and its learning popup (§31).
/// </summary>
public static class LessonMarkdownParser
{
    public static (List<Topic> Topics, List<InterpretationEntry> Words, List<WritingEntry> Writings) Parse(string filePath)
    {
        return ParseContent(File.ReadAllText(filePath), Path.GetFileName(filePath));
    }

    public static (List<Topic> Topics, List<InterpretationEntry> Words, List<WritingEntry> Writings) ParseContent(
        string content, string sourceFileName)
    {
        var topics = new List<Topic>();
        var words = new List<InterpretationEntry>();
        var writings = new List<WritingEntry>();

        foreach (var (title, body) in SplitArticles(content))
        {
            var sections = MarkdownSectionSplitter.Split(body);
            if (sections is null)
            {
                continue;
            }

            Topic? articleTopic = null;
            var translations = new List<TranslationPair>();

            foreach (var (sectionTitle, sectionBody) in sections)
            {
                if (IsSectionNamed(sectionTitle, "Text", "본문"))
                {
                    if (sectionBody.Length > 0)
                    {
                        articleTopic = new Topic
                        {
                            Id = Guid.NewGuid(),
                            Title = title,
                            Text = sectionBody,
                            SourceFileName = sourceFileName,
                        };
                        topics.Add(articleTopic);
                    }
                }
                else if (IsSectionNamed(sectionTitle, "Translation", "해석"))
                {
                    translations.AddRange(ParseTranslationLines(sectionBody));
                }
                else if (IsSectionNamed(sectionTitle, "Words", "단어"))
                {
                    words.AddRange(ParseWordLines(sectionBody));
                }
                else if (IsSectionNamed(sectionTitle, "Writing", "영작"))
                {
                    writings.AddRange(ParseWritingLines(sectionBody));
                }
            }

            if (articleTopic is not null)
            {
                articleTopic.Translations = translations;
            }
        }

        return (topics, words, writings);
    }

    private static IEnumerable<(string Title, string Body)> SplitArticles(string content)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');

        var indices = lines
            .Select((line, index) => (line, index))
            .Where(t => t.line.TrimStart().StartsWith("## ") && !t.line.TrimStart().StartsWith("### "))
            .Select(t => t.index)
            .ToList();

        for (var i = 0; i < indices.Count; i++)
        {
            var start = indices[i];
            var end = i + 1 < indices.Count ? indices[i + 1] : lines.Length;

            var title = lines[start].TrimStart()[3..].Trim();
            var body = string.Join("\n", lines[(start + 1)..end]);

            yield return (title, body);
        }
    }

    private static bool IsSectionNamed(string sectionTitle, string english, string korean)
    {
        return sectionTitle.Equals(english, StringComparison.OrdinalIgnoreCase)
            || sectionTitle.Equals(korean, StringComparison.Ordinal);
    }

    private static IEnumerable<InterpretationEntry> ParseWordLines(string sectionBody)
    {
        foreach (var rawLine in sectionBody.Split('\n'))
        {
            var line = StripBullet(rawLine);
            if (line.Length == 0)
            {
                continue;
            }

            if (TodayEnglishParser.TryParseWordLine(line, out var entry))
            {
                yield return entry;
            }
        }
    }

    private static IEnumerable<WritingEntry> ParseWritingLines(string sectionBody)
    {
        foreach (var rawLine in sectionBody.Split('\n'))
        {
            var line = StripBullet(rawLine);
            if (line.Length == 0)
            {
                continue;
            }

            var parts = line.Split('|').Select(p => p.Trim()).ToArray();
            switch (parts.Length)
            {
                case 2:
                    yield return new WritingEntry { Text = parts[0], Description = string.Empty, Example = parts[1] };
                    break;
                case 3:
                    yield return new WritingEntry { Text = parts[0], Description = parts[1], Example = parts[2] };
                    break;
            }
        }
    }

    private static IEnumerable<TranslationPair> ParseTranslationLines(string sectionBody)
    {
        foreach (var rawLine in sectionBody.Split('\n'))
        {
            var line = StripBullet(rawLine);
            if (line.Length == 0)
            {
                continue;
            }

            // English sentences may contain no '|', so split on the first one.
            var separator = line.IndexOf('|');
            if (separator <= 0)
            {
                continue;
            }

            var english = line[..separator].Trim();
            var korean = line[(separator + 1)..].Trim();
            if (english.Length > 0 && korean.Length > 0)
            {
                yield return new TranslationPair(english, korean);
            }
        }
    }

    private static string StripBullet(string rawLine)
    {
        var line = rawLine.Trim();
        return line.StartsWith("- ") ? line[2..].Trim() : line;
    }
}
