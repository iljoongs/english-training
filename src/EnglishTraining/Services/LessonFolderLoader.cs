using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

/// <summary>
/// Result of reading every "*.md" lesson file directly under a data folder
/// (§31.2) — the reading window's sole source of topics/words/writings once
/// the app becomes a read-only lesson reader.
/// </summary>
public sealed class LessonFolderResult
{
    public required IReadOnlyList<Topic> Topics { get; init; }
    public required IReadOnlyList<InterpretationEntry> Words { get; init; }
    public required IReadOnlyList<WritingEntry> Writings { get; init; }
    public required int FileCount { get; init; }
    public required int SkippedFileCount { get; init; }
}

/// <summary>
/// Reads all "*.md" lesson files directly under a data folder (no
/// subfolders) and merges them into one in-memory dataset (§31.2). Files are
/// read in filename order (case-insensitive); a file that fails to read is
/// skipped and counted, the rest still loads. Duplicate words/writings
/// (same normalized text) keep the first-read (lowest filename) entry.
/// </summary>
public static class LessonFolderLoader
{
    public static LessonFolderResult LoadFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            return new LessonFolderResult { Topics = [], Words = [], Writings = [], FileCount = 0, SkippedFileCount = 0 };
        }

        var files = Directory.GetFiles(folderPath, "*.md", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var topics = new List<Topic>();
        var seenWordKeys = new HashSet<string>();
        var words = new List<InterpretationEntry>();
        var seenWritingKeys = new HashSet<string>();
        var writings = new List<WritingEntry>();
        var skippedFileCount = 0;

        foreach (var file in files)
        {
            (List<Topic> Topics, List<InterpretationEntry> Words, List<WritingEntry> Writings) parsed;
            try
            {
                parsed = LessonMarkdownParser.Parse(file);
            }
            catch (IOException)
            {
                skippedFileCount++;
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                skippedFileCount++;
                continue;
            }

            topics.AddRange(parsed.Topics);

            foreach (var word in parsed.Words)
            {
                if (seenWordKeys.Add(TextNormalizer.Normalize(word.Text)))
                {
                    words.Add(word);
                }
            }

            foreach (var writing in parsed.Writings)
            {
                if (seenWritingKeys.Add(TextNormalizer.Normalize(writing.Text)))
                {
                    writings.Add(writing);
                }
            }
        }

        return new LessonFolderResult
        {
            Topics = topics,
            Words = words,
            Writings = writings,
            FileCount = files.Count,
            SkippedFileCount = skippedFileCount,
        };
    }
}
