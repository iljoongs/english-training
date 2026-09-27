using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class LessonMarkdownParserTests
{
    [Fact]
    public void Parse_SampleLessonFile_ExtractsBothArticles()
    {
        var repoRoot = FindRepoRoot();
        var samplePath = Path.Combine(repoRoot, "doc", "sample-lesson.md");

        var (topics, words, writings) = LessonMarkdownParser.Parse(samplePath);

        Assert.Equal(2, topics.Count);
        Assert.Equal("Working from Home", topics[0].Title);
        Assert.Contains("commute", topics[0].Text);
        Assert.Equal("Sleep and Memory", topics[1].Title);
        Assert.Contains("fall asleep", topics[1].Text);
        Assert.All(topics, t => Assert.Equal("sample-lesson.md", t.SourceFileName));

        Assert.Equal(5, words.Count);
        Assert.Contains(words, w => w.Text == "commute" && w.PartOfSpeech == "v" && w.Ko == "통근하다");
        Assert.Contains(words, w => w.Text == "fall asleep" && w.Expression == "fall asleep on the sofa");

        Assert.Equal(3, writings.Count);
        Assert.Contains(writings, w => w.Text == "commute" && w.Description.Length > 0);
        Assert.Contains(writings, w => w.Text == "fall asleep" && w.Description == "" && w.Example == "I fell asleep during the class.");
    }

    [Fact]
    public void ParseContent_KoreanBlockNames_AreRecognized()
    {
        const string markdown = """
            ## 재택 근무

            ### 본문
            More companies allow remote work.

            ### 단어
            commute (v) (통근하다) (commute to work)

            ### 영작
            commute | I commute by bus.
            """;

        var (topics, words, writings) = LessonMarkdownParser.ParseContent(markdown, "lesson.md");

        var topic = Assert.Single(topics);
        Assert.Equal("재택 근무", topic.Title);
        var word = Assert.Single(words);
        Assert.Equal("commute", word.Text);
        var writing = Assert.Single(writings);
        Assert.Equal("commute", writing.Text);
    }

    [Fact]
    public void ParseContent_BulletPrefix_IsStripped()
    {
        const string markdown = """
            ## Article

            ### Words
            - commute (v) (통근하다) (commute to work)

            ### Writing
            - commute | I commute by bus.
            """;

        var (_, words, writings) = LessonMarkdownParser.ParseContent(markdown, "lesson.md");

        Assert.Equal("commute", Assert.Single(words).Text);
        Assert.Equal("commute", Assert.Single(writings).Text);
    }

    [Fact]
    public void ParseContent_MalformedLines_AreSkipped()
    {
        const string markdown = """
            ## Article

            ### Words
            commute (v) (통근하다)
            survey (n) (설문 조사) (a recent survey found that ~)

            ### Writing
            only-one-field
            commute | I commute by bus.
            """;

        var (_, words, writings) = LessonMarkdownParser.ParseContent(markdown, "lesson.md");

        Assert.Equal("survey", Assert.Single(words).Text);
        Assert.Equal("commute", Assert.Single(writings).Text);
    }

    [Fact]
    public void ParseContent_EmptyTextSection_DoesNotCreateTopic()
    {
        const string markdown = """
            ## Article

            ### Words
            commute (v) (통근하다) (commute to work)
            """;

        var (topics, words, _) = LessonMarkdownParser.ParseContent(markdown, "lesson.md");

        Assert.Empty(topics);
        Assert.Single(words);
    }

    [Fact]
    public void ParseContent_NoArticleHeading_ReturnsEmptyLists()
    {
        const string markdown = "Just plain text, no ## heading.";

        var (topics, words, writings) = LessonMarkdownParser.ParseContent(markdown, "lesson.md");

        Assert.Empty(topics);
        Assert.Empty(words);
        Assert.Empty(writings);
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
