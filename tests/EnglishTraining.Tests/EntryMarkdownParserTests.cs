using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class EntryMarkdownParserTests
{
    [Fact]
    public void InterpretationMarkdownParser_ParseContent_ExtractsTextAndKo()
    {
        const string markdown = "# would like to\n\n해석: ~하고 싶다\n";

        var entry = InterpretationMarkdownParser.ParseContent(markdown);

        Assert.Equal("would like to", entry.Text);
        Assert.Equal("~하고 싶다", entry.Ko);
    }

    [Fact]
    public void InterpretationMarkdownParser_FormatThenParse_RoundTrips()
    {
        var original = new InterpretationEntry { Text = "look forward to", Ko = "~을 기대하다" };

        var reparsed = InterpretationMarkdownParser.ParseContent(InterpretationMarkdownParser.Format(original));

        Assert.Equal(original.Text, reparsed.Text);
        Assert.Equal(original.Ko, reparsed.Ko);
    }

    [Fact]
    public void InterpretationMarkdownParser_Parse_SampleFile_MatchesDocSample()
    {
        var samplePath = Path.Combine(FindRepoRoot(), "doc", "sample-interpretation.md");

        var entry = InterpretationMarkdownParser.Parse(samplePath);

        Assert.Equal("would like to", entry.Text);
        Assert.Equal("~하고 싶다", entry.Ko);
    }

    [Fact]
    public void WritingMarkdownParser_ParseContent_ExtractsAllFields()
    {
        const string markdown = """
            # would like to

            설명: 정중하게 원하는 것을 표현할 때 사용
            예문: I would like to know more about this.
            """;

        var entry = WritingMarkdownParser.ParseContent(markdown);

        Assert.Equal("would like to", entry.Text);
        Assert.Equal("정중하게 원하는 것을 표현할 때 사용", entry.Description);
        Assert.Equal("I would like to know more about this.", entry.Example);
    }

    [Fact]
    public void WritingMarkdownParser_FormatThenParse_RoundTrips()
    {
        var original = new WritingEntry { Text = "wondering if", Description = "d", Example = "e" };

        var reparsed = WritingMarkdownParser.ParseContent(WritingMarkdownParser.Format(original));

        Assert.Equal(original.Text, reparsed.Text);
        Assert.Equal(original.Description, reparsed.Description);
        Assert.Equal(original.Example, reparsed.Example);
    }

    [Fact]
    public void WritingMarkdownParser_Parse_SampleFile_MatchesDocSample()
    {
        var samplePath = Path.Combine(FindRepoRoot(), "doc", "sample-writing.md");

        var entry = WritingMarkdownParser.Parse(samplePath);

        Assert.Equal("would like to", entry.Text);
        Assert.Contains("정중하게", entry.Description);
        Assert.Equal("I would like to know more about this.", entry.Example);
    }

    [Fact]
    public void ExpressionMarkdownParser_ParseContent_ExtractsAllFields()
    {
        const string markdown = """
            # would like to

            의미: ~하고 싶다 (want to의 정중한 표현)
            사용법: would like to + 동사원형
            예문: I would like to know more about this.
            """;

        var entry = ExpressionMarkdownParser.ParseContent(markdown);

        Assert.Equal("would like to", entry.Text);
        Assert.Equal("~하고 싶다 (want to의 정중한 표현)", entry.Meaning);
        Assert.Equal("would like to + 동사원형", entry.Usage);
        Assert.Equal("I would like to know more about this.", entry.Example);
    }

    [Fact]
    public void ExpressionMarkdownParser_FormatThenParse_RoundTrips()
    {
        var original = new ExpressionEntry { Text = "be supposed to", Meaning = "m", Usage = "u", Example = "e" };

        var reparsed = ExpressionMarkdownParser.ParseContent(ExpressionMarkdownParser.Format(original));

        Assert.Equal(original.Text, reparsed.Text);
        Assert.Equal(original.Meaning, reparsed.Meaning);
        Assert.Equal(original.Usage, reparsed.Usage);
        Assert.Equal(original.Example, reparsed.Example);
    }

    [Fact]
    public void ExpressionMarkdownParser_Parse_SampleFile_MatchesDocSample()
    {
        var samplePath = Path.Combine(FindRepoRoot(), "doc", "sample-expression.md");

        var entry = ExpressionMarkdownParser.Parse(samplePath);

        Assert.Equal("would like to", entry.Text);
        Assert.Contains("정중한 표현", entry.Meaning);
        Assert.Equal("would like to + 동사원형", entry.Usage);
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
