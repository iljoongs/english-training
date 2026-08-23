using EnglishTraining.Models;

namespace EnglishTraining.Services;

/// <summary>
/// Executes "학습 &gt; 가져오기" (§29): parses today.md content via
/// TodayEnglishParser and adds any new interpretation entries to the given
/// repository. An entry is skipped as a duplicate when an entry with the
/// same normalized text (TextNormalizer, same rule as §14 matching) already
/// exists in the repository or was already added earlier in this same
/// import.
/// </summary>
public static class TodayEnglishImportService
{
    public static TodayEnglishImportResult ImportContent(
        string content,
        IEntryRepository<InterpretationEntry> interpretationRepository)
    {
        TodayEnglishParser.Parse(content, out var parsedInterpretations);

        var existingTexts = interpretationRepository.Entries
            .Select(e => TextNormalizer.Normalize(e.Text))
            .ToHashSet();

        var interpretationsAdded = 0;
        var duplicatesSkipped = 0;
        foreach (var entry in parsedInterpretations)
        {
            if (!existingTexts.Add(TextNormalizer.Normalize(entry.Text)))
            {
                duplicatesSkipped++;
                continue;
            }

            interpretationRepository.Add(entry);
            interpretationsAdded++;
        }

        if (interpretationsAdded > 0)
        {
            interpretationRepository.Save();
        }

        return new TodayEnglishImportResult
        {
            InterpretationsAdded = interpretationsAdded,
            DuplicatesSkipped = duplicatesSkipped,
        };
    }
}
