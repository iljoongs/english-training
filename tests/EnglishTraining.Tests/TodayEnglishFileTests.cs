using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class TodayEnglishFileTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "today-english-tests-" + Guid.NewGuid().ToString("N"));

    private string TestPath => Path.Combine(_dir, "english-today.md");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, recursive: true);
        }
    }

    [Fact]
    public void ResolvePath_PointsToDataEnglishTodayMd()
    {
        Assert.EndsWith(Path.Combine("data", "english-today.md"), TodayEnglishFile.ResolvePath());
    }

    [Fact]
    public void AddItem_CreatesFileWithTopicSection()
    {
        var added = TodayEnglishFile.AddItem("Working from Home", "deadline", TestPath);

        Assert.True(added);
        var content = File.ReadAllText(TestPath);
        Assert.StartsWith("# English Today", content);
        Assert.Contains("## Working from Home\n- deadline", content);
    }

    [Fact]
    public void AddItem_WordsAndSentencesAreStoredAlike_GroupedByTopic()
    {
        TodayEnglishFile.AddItem("A", "deadline", TestPath);
        TodayEnglishFile.AddItem("B", "would rather", TestPath);
        TodayEnglishFile.AddItem("A", "I look forward to seeing you again.", TestPath);

        var content = TodayEnglishFile.ReadOrDefault(TestPath);
        var a = content.IndexOf("## A", StringComparison.Ordinal);
        var b = content.IndexOf("## B", StringComparison.Ordinal);
        Assert.True(a < content.IndexOf("- deadline", StringComparison.Ordinal));
        Assert.True(content.IndexOf("- I look forward", StringComparison.Ordinal) < b);
        Assert.True(b < content.IndexOf("- would rather", StringComparison.Ordinal));
        Assert.DoesNotContain("**", content);
        Assert.DoesNotContain("###", content);
    }

    [Fact]
    public void AddItem_DuplicateInSameTopic_IsSkipped_ButAllowedInOtherTopic()
    {
        Assert.True(TodayEnglishFile.AddItem("A", "deadline", TestPath));
        Assert.False(TodayEnglishFile.AddItem("A", "Deadline", TestPath));
        Assert.False(TodayEnglishFile.AddItem("A", "   ", TestPath));
        Assert.True(TodayEnglishFile.AddItem("B", "deadline", TestPath));

        var lines = TodayEnglishFile.ReadOrDefault(TestPath).Split('\n');
        Assert.Equal(2, lines.Count(l => l.Trim().Equals("- deadline", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void AddItem_CollapsesWhitespaceInSelection()
    {
        TodayEnglishFile.AddItem("A", "line one\r\n  line two", TestPath);

        Assert.Contains("- line one line two", TodayEnglishFile.ReadOrDefault(TestPath));
    }

    [Fact]
    public void InsertItem_KeepsHandEditedContent()
    {
        var edited = "# English Today\n\n## A\n- deadline (마감일)\n\nmy own note\n\n## B\n- flexible\n";

        var result = TodayEnglishFile.InsertItem(edited, "A", "commute");

        Assert.Contains("my own note\n- commute\n\n## B", result);
        Assert.Contains("- deadline (마감일)", result);
        Assert.Contains("## B\n- flexible", result);
    }

    [Fact]
    public void Write_ThenReadOrDefault_RoundTrips()
    {
        TodayEnglishFile.Write("custom content\n", TestPath);

        Assert.Equal("custom content\n", TodayEnglishFile.ReadOrDefault(TestPath));
    }

    [Fact]
    public void ReadOrDefault_MissingFile_ReturnsHeader()
    {
        Assert.Equal(TodayEnglishFile.DefaultContent, TodayEnglishFile.ReadOrDefault(TestPath));
    }

    [Fact]
    public void BuildClaudePrompt_ContainsInstructionsAndMemo()
    {
        var prompt = TodayEnglishFile.BuildClaudePrompt("# English Today\n\n## A\n- deadline\n");

        Assert.Contains("### Translation", prompt);
        Assert.Contains("단어 (품사) (해석) (표현)", prompt);
        Assert.Contains("표현 | 쓰임 설명 | 예문", prompt);
        Assert.Contains("## A\n- deadline", prompt);
    }
}
