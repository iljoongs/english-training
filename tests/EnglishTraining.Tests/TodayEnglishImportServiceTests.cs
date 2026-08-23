using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class TodayEnglishImportServiceTests
{
    private const string SampleContent = """
        # 오늘의 영어

        **I was wondering if you could help me with this.**
        (이것 좀 도와주실 수 있는지 궁금했어요.)

        **I was wondering if you could help me with this.**
        (이것 좀 도와주실 수 있는지 궁금했어요.)

        **Let's touch base again next week.**
        (다음 주에 다시 연락합시다.)
        """;

    [Fact]
    public void ImportContent_AddsNewEntriesAndSkipsWithinFileDuplicate()
    {
        var path = TempPath();
        try
        {
            var interpretationRepository = new JsonEntryRepository<InterpretationEntry>(path, []);

            var result = TodayEnglishImportService.ImportContent(SampleContent, interpretationRepository);

            Assert.Equal(2, result.InterpretationsAdded);
            Assert.Equal(1, result.DuplicatesSkipped);
            Assert.Equal(2, interpretationRepository.Entries.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ImportContent_PersistsAddedEntriesToDisk()
    {
        var path = TempPath();
        try
        {
            var interpretationRepository = new JsonEntryRepository<InterpretationEntry>(path, []);

            TodayEnglishImportService.ImportContent(SampleContent, interpretationRepository);

            var reloaded = new JsonEntryRepository<InterpretationEntry>(path, []);
            Assert.Equal(2, reloaded.Entries.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ImportContent_SkipsEntriesAlreadyPresentInRepository()
    {
        var path = TempPath();
        try
        {
            var interpretationRepository = new JsonEntryRepository<InterpretationEntry>(
                path,
                [new InterpretationEntry { Text = "I was wondering if you could help me with this.", Ko = "기존 번역" }]);

            var result = TodayEnglishImportService.ImportContent(SampleContent, interpretationRepository);

            Assert.Equal(1, result.InterpretationsAdded);
            Assert.Equal(2, result.DuplicatesSkipped);
            Assert.Equal("기존 번역", interpretationRepository.Entries
                .Single(e => e.Text == "I was wondering if you could help me with this.").Ko);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ImportContent_RunTwice_IsIdempotent()
    {
        var path = TempPath();
        try
        {
            var interpretationRepository = new JsonEntryRepository<InterpretationEntry>(path, []);

            TodayEnglishImportService.ImportContent(SampleContent, interpretationRepository);
            var second = TodayEnglishImportService.ImportContent(SampleContent, interpretationRepository);

            Assert.Equal(0, second.InterpretationsAdded);
            Assert.Equal(3, second.DuplicatesSkipped);
            Assert.Equal(2, interpretationRepository.Entries.Count);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string TempPath() =>
        Path.Combine(Path.GetTempPath(), $"interpretations-{Guid.NewGuid()}.json");
}
