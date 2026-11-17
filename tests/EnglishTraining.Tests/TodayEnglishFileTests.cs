using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class TodayEnglishFileTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "today-english-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void ResolveDirectory_PointsToDataToday()
    {
        Assert.EndsWith(Path.Combine("data", "today"), TodayEnglishFile.ResolveDirectory());
    }

    [Theory]
    [InlineData("Working from Home", "working-from-home")]
    [InlineData("  Sleep & Memory! ", "sleep-memory")]
    [InlineData("재택 근무", "재택-근무")]
    [InlineData("???", "untitled")]
    public void Slugify_MakesSafeFileNames(string title, string expected)
    {
        Assert.Equal(expected, TodayEnglishFile.Slugify(title));
    }

    [Fact]
    public void AddWord_CreatesTopicFileFromTemplate()
    {
        var added = TodayEnglishFile.AddWord("Working from Home", "Body text.", "deadline", _dir);

        Assert.True(added);
        var content = File.ReadAllText(Path.Combine(_dir, "working-from-home.md"));
        Assert.Contains("## Working from Home", content);
        Assert.Contains("### Text\nBody text.", content);
        Assert.Contains("### Words\n- deadline", content);
        Assert.Contains("### Sentences", content);
    }

    [Fact]
    public void AddWordAndSentence_GoToTheirOwnSections()
    {
        TodayEnglishFile.AddWord("T", "Body.", "deadline", _dir);
        TodayEnglishFile.AddSentence("T", "Body.", "I look forward to seeing you again.", _dir);
        TodayEnglishFile.AddWord("T", "Body.", "flexible", _dir);

        var content = TodayEnglishFile.ReadOrTemplate("T", "Body.", _dir);
        var wordsAt = content.IndexOf("### Words", StringComparison.Ordinal);
        var sentencesAt = content.IndexOf("### Sentences", StringComparison.Ordinal);
        Assert.True(content.IndexOf("- deadline", StringComparison.Ordinal) > wordsAt);
        Assert.True(content.IndexOf("- flexible", StringComparison.Ordinal) < sentencesAt);
        Assert.True(content.IndexOf("- I look forward", StringComparison.Ordinal) > sentencesAt);
    }

    [Fact]
    public void AddWord_Duplicate_IsSkipped()
    {
        Assert.True(TodayEnglishFile.AddWord("T", "Body.", "deadline", _dir));
        Assert.False(TodayEnglishFile.AddWord("T", "Body.", "Deadline", _dir));
        Assert.False(TodayEnglishFile.AddWord("T", "Body.", "   ", _dir));

        var content = TodayEnglishFile.ReadOrTemplate("T", "Body.", _dir);
        Assert.Single(content.Split('\n'), l => l.Trim().Equals("- deadline", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddItem_CollapsesWhitespaceInSelection()
    {
        TodayEnglishFile.AddSentence("T", "Body.", "line one\r\n  line two", _dir);

        Assert.Contains("- line one line two", TodayEnglishFile.ReadOrTemplate("T", "Body.", _dir));
    }

    [Fact]
    public void InsertItem_KeepsHandEditedContent_AndAddsMissingSection()
    {
        var edited = "# Today's English\n\n## T\n\n### Words\n- deadline (n) (마감일)\n\nmy own note\n";

        var result = TodayEnglishFile.InsertItem(edited, "Words", "flexible");
        result = TodayEnglishFile.InsertItem(result, "Sentences", "Hello there.");

        Assert.Contains("- deadline (n) (마감일)\n\nmy own note\n- flexible", result);
        Assert.Contains("my own note", result);
        Assert.Contains("### Sentences\n- Hello there.", result);
    }

    [Fact]
    public void Write_ThenReadOrTemplate_RoundTrips()
    {
        TodayEnglishFile.Write("T", "custom content\n", _dir);

        Assert.Equal("custom content\n", TodayEnglishFile.ReadOrTemplate("T", "Body.", _dir));
    }

    [Fact]
    public void BuildClaudePrompt_ContainsInstructionsAndMemo()
    {
        var memo = TodayEnglishFile.BuildTemplate("Working from Home", "Body text.");

        var prompt = TodayEnglishFile.BuildClaudePrompt(memo);

        Assert.Contains("### Translation", prompt);
        Assert.Contains("단어 (품사) (해석) (표현)", prompt);
        Assert.Contains("표현 | 쓰임 설명 | 예문", prompt);
        Assert.EndsWith("~~~\n", prompt.Replace("\r\n", "\n"));
        Assert.Contains("## Working from Home", prompt);
    }
}
