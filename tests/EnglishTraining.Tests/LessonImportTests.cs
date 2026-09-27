using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class LessonImportTests
{
    private const string SampleContent = """
        # 2026-09-27

        ## Working from Home

        ### Text
        Workers do not have to commute. They would rather stay home.

        See the [survey](https://example.com) for details.

        ### Words
        commute (v) (통근하다) (commute to work)
        - would rather (phrase) (차라리 ~하겠다) (would rather A than B)

        ### Writing
        would rather | 두 가지 중 하나를 더 원할 때 | I would rather walk than wait.
        - commute | I commute by bus.

        ## Sleep and Memory

        ### Text
        Try not to stay up all night.

        ### Words
        stay up (phrase) (깨어 있다) (stay up late)
        Commute (v) (통근하다) (duplicate within file)
        """;

    [Fact]
    public void ParseContent_SplitsArticlesIntoTopicsWordsAndWritings()
    {
        var (topics, words, writings) = LessonMarkdownParser.ParseContent(SampleContent);

        Assert.Equal(["Working from Home", "Sleep and Memory"], topics.Select(t => t.Title));
        Assert.Equal(
            "Workers do not have to commute. They would rather stay home.\n\nSee the survey for details.",
            topics[0].Text);

        Assert.Equal(["commute", "would rather", "stay up", "Commute"], words.Select(w => w.Text));
        Assert.Equal("v", words[0].PartOfSpeech);
        Assert.Equal("통근하다", words[0].Ko);
        Assert.Equal("commute to work", words[0].Expression);

        Assert.Equal(2, writings.Count);
        Assert.Equal("would rather", writings[0].Text);
        Assert.Equal("두 가지 중 하나를 더 원할 때", writings[0].Description);
        Assert.Equal("I would rather walk than wait.", writings[0].Example);
        Assert.Equal("commute", writings[1].Text);
        Assert.Equal(string.Empty, writings[1].Description);
        Assert.Equal("I commute by bus.", writings[1].Example);
    }

    [Fact]
    public void ParseContent_WithoutArticleHeadings_ReturnsNothing()
    {
        var (topics, words, writings) = LessonMarkdownParser.ParseContent("### deadline\n마감일");

        Assert.Empty(topics);
        Assert.Empty(words);
        Assert.Empty(writings);
    }

    [Fact]
    public void ImportContent_AddsToAllRepositoriesAndSkipsDuplicates()
    {
        var paths = TempPaths();
        try
        {
            var (topicRepository, interpretationRepository, writingRepository) = CreateRepositories(paths);

            var result = LessonImportService.ImportContent(
                SampleContent, topicRepository, interpretationRepository, writingRepository);

            Assert.Equal(2, result.TopicsAdded);
            Assert.Equal(3, result.WordsAdded);
            Assert.Equal(2, result.WritingsAdded);
            Assert.Equal(1, result.DuplicatesSkipped);

            var (reloadedTopics, reloadedWords, reloadedWritings) = CreateRepositories(paths);
            Assert.Equal(3, reloadedTopics.Topics.Count); // seeded "Sample" topic + 2 imported
            Assert.Equal(3, reloadedWords.Entries.Count);
            Assert.Equal(2, reloadedWritings.Entries.Count);
        }
        finally
        {
            DeleteAll(paths);
        }
    }

    [Fact]
    public void ImportContent_RunTwice_AddsNothingTheSecondTime()
    {
        var paths = TempPaths();
        try
        {
            var (topicRepository, interpretationRepository, writingRepository) = CreateRepositories(paths);
            LessonImportService.ImportContent(SampleContent, topicRepository, interpretationRepository, writingRepository);

            var second = LessonImportService.ImportContent(
                SampleContent, topicRepository, interpretationRepository, writingRepository);

            Assert.Equal(0, second.TopicsAdded);
            Assert.Equal(0, second.WordsAdded);
            Assert.Equal(0, second.WritingsAdded);
            Assert.Equal(8, second.DuplicatesSkipped);
        }
        finally
        {
            DeleteAll(paths);
        }
    }

    private static (JsonTopicRepository, JsonEntryRepository<InterpretationEntry>, JsonEntryRepository<WritingEntry>) CreateRepositories(string[] paths) =>
        (new JsonTopicRepository(paths[0]),
         new JsonEntryRepository<InterpretationEntry>(paths[1], []),
         new JsonEntryRepository<WritingEntry>(paths[2], []));

    private static string[] TempPaths() =>
        new[] { "topics", "interpretations", "writings" }
            .Select(name => Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid()}.json"))
            .ToArray();

    private static void DeleteAll(string[] paths)
    {
        foreach (var path in paths)
        {
            File.Delete(path);
        }
    }
}
