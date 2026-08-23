using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class TopicMarkdownParserTests
{
    [Fact]
    public void ParseContent_ExtractsTitleAndBody()
    {
        const string markdown = """
            # 일상 대화

            I was wondering if you could help me with this problem. I look forward to seeing you soon.

            You are supposed to be here by nine, but as far as I know the schedule has changed.
            """;

        var topic = TopicMarkdownParser.ParseContent(markdown);

        Assert.Equal("일상 대화", topic.Title);
        Assert.StartsWith("I was wondering if you could help me", topic.Text);
        Assert.Contains("as far as I know the schedule has changed.", topic.Text);
        Assert.NotEqual(Guid.Empty, topic.Id);
    }

    [Fact]
    public void ParseContent_TrimsBlankLinesAroundBody()
    {
        const string markdown = "# Title\n\n\nBody text.\n\n";

        var topic = TopicMarkdownParser.ParseContent(markdown);

        Assert.Equal("Title", topic.Title);
        Assert.Equal("Body text.", topic.Text);
    }

    [Fact]
    public void ParseContent_NoHeading_TreatsWholeContentAsBody()
    {
        const string markdown = "Just plain text, no heading.";

        var topic = TopicMarkdownParser.ParseContent(markdown);

        Assert.Equal(string.Empty, topic.Title);
        Assert.Equal("Just plain text, no heading.", topic.Text);
    }

    [Fact]
    public void Parse_SampleTopicFile_MatchesDocSample()
    {
        var repoRoot = FindRepoRoot();
        var samplePath = Path.Combine(repoRoot, "doc", "sample-topic.md");

        var topic = TopicMarkdownParser.Parse(samplePath);

        Assert.Equal("일상 대화", topic.Title);
        Assert.Contains("look forward to seeing you soon.", topic.Text);
    }

    [Fact]
    public void FormatMultiple_ThenParseMultipleContent_RoundTrips()
    {
        var topics = new List<Topic>
        {
            new() { Id = Guid.NewGuid(), Title = "Daily Chat", Text = "I look forward to seeing you." },
            new() { Id = Guid.NewGuid(), Title = "Business English", Text = "As far as I know, the meeting is tomorrow." },
        };

        var formatted = TopicMarkdownParser.FormatMultiple(topics);
        var reparsed = TopicMarkdownParser.ParseMultipleContent(formatted);

        Assert.Equal(2, reparsed.Count);
        Assert.Equal("Daily Chat", reparsed[0].Title);
        Assert.Equal("I look forward to seeing you.", reparsed[0].Text);
        Assert.Equal("Business English", reparsed[1].Title);
        Assert.Equal("As far as I know, the meeting is tomorrow.", reparsed[1].Text);
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "EnglishTraining.sln")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        return dir ?? throw new InvalidOperationException("Repo root not found.");
    }
}
