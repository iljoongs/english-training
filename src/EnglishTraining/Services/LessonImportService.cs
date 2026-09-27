using EnglishTraining.Models;

namespace EnglishTraining.Services;

/// <summary>
/// Executes "Files &gt; Import All" (§30): parses a lesson file via
/// LessonMarkdownParser and adds its topics, words and writing entries to the
/// three repositories at once. Duplicates are skipped so re-importing the same
/// file is safe: a topic with the same title (case-insensitive), or a
/// word/writing entry with the same normalized text (TextNormalizer, §14),
/// that already exists or was added earlier in this import.
/// </summary>
public static class LessonImportService
{
    public static LessonImportResult ImportContent(
        string content,
        ITopicRepository topicRepository,
        IEntryRepository<InterpretationEntry> interpretationRepository,
        IEntryRepository<WritingEntry> writingRepository)
    {
        var (topics, words, writings) = LessonMarkdownParser.ParseContent(content);
        var duplicatesSkipped = 0;

        var topicTitles = topicRepository.Topics
            .Select(t => t.Title.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var topicsAdded = 0;
        foreach (var topic in topics)
        {
            if (!topicTitles.Add(topic.Title.Trim()))
            {
                duplicatesSkipped++;
                continue;
            }

            topicRepository.Add(topic);
            topicsAdded++;
        }

        var wordsAdded = AddNewEntries(words, interpretationRepository, ref duplicatesSkipped);
        var writingsAdded = AddNewEntries(writings, writingRepository, ref duplicatesSkipped);

        if (topicsAdded > 0)
        {
            topicRepository.Save();
        }

        return new LessonImportResult
        {
            TopicsAdded = topicsAdded,
            WordsAdded = wordsAdded,
            WritingsAdded = writingsAdded,
            DuplicatesSkipped = duplicatesSkipped,
        };
    }

    private static int AddNewEntries<T>(List<T> entries, IEntryRepository<T> repository, ref int duplicatesSkipped)
        where T : IEntry
    {
        var existingTexts = repository.Entries
            .Select(e => TextNormalizer.Normalize(e.Text))
            .ToHashSet();

        var added = 0;
        foreach (var entry in entries)
        {
            if (!existingTexts.Add(TextNormalizer.Normalize(entry.Text)))
            {
                duplicatesSkipped++;
                continue;
            }

            repository.Add(entry);
            added++;
        }

        if (added > 0)
        {
            repository.Save();
        }

        return added;
    }
}
