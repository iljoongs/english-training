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
    public void InterpretationMarkdownParser_ParseContent_ExtractsExpressionLabel()
    {
        const string markdown = "# look forward to\n\n해석: ~을 기대하다\n표현: look forward to + 동명사(-ing)\n";

        var entry = InterpretationMarkdownParser.ParseContent(markdown);

        Assert.Equal("look forward to", entry.Text);
        Assert.Equal("~을 기대하다", entry.Ko);
        Assert.Equal("look forward to + 동명사(-ing)", entry.Expression);
    }

    [Fact]
    public void InterpretationMarkdownParser_ParseContent_ExtractsPartOfSpeechLabel()
    {
        const string markdown = "# deadline\n\n품사: n\n해석: 마감일, 기한\n표현: meet the deadline\n";

        var entry = InterpretationMarkdownParser.ParseContent(markdown);

        Assert.Equal("deadline", entry.Text);
        Assert.Equal("n", entry.PartOfSpeech);
        Assert.Equal("마감일, 기한", entry.Ko);
        Assert.Equal("meet the deadline", entry.Expression);
    }

    [Fact]
    public void InterpretationMarkdownParser_FormatThenParse_RoundTrips()
    {
        var original = new InterpretationEntry
        {
            Text = "look forward to",
            PartOfSpeech = "phrase",
            Ko = "~을 기대하다",
            Expression = "look forward to + 동명사(-ing)",
        };

        var reparsed = InterpretationMarkdownParser.ParseContent(InterpretationMarkdownParser.Format(original));

        Assert.Equal(original.Text, reparsed.Text);
        Assert.Equal(original.PartOfSpeech, reparsed.PartOfSpeech);
        Assert.Equal(original.Ko, reparsed.Ko);
        Assert.Equal(original.Expression, reparsed.Expression);
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
    public void InterpretationMarkdownParser_ParseMultipleContent_PlainBodyBecomesKo()
    {
        const string markdown = """
            # 2026-08-22 단어/숙어

            ### deadline
            마감일, 기한

            ### protection
            보호
            """;

        var entries = InterpretationMarkdownParser.ParseMultipleContent(markdown);

        Assert.Equal(2, entries.Count);
        Assert.Equal("마감일, 기한", entries[0].Ko);
        Assert.Equal("deadline", entries[0].Text);
        Assert.Equal("보호", entries[1].Ko);
    }

    [Fact]
    public void InterpretationMarkdownParser_ParseMultipleContent_LabeledSectionsExtractExpression()
    {
        const string markdown = """
            # 2026-08-22 단어/숙어

            ### deadline
            해석: 마감일, 기한
            표현: meet the deadline
            """;

        var entries = InterpretationMarkdownParser.ParseMultipleContent(markdown);

        var entry = Assert.Single(entries);
        Assert.Equal("deadline", entry.Text);
        Assert.Equal("마감일, 기한", entry.Ko);
        Assert.Equal("meet the deadline", entry.Expression);
    }

    [Fact]
    public void InterpretationMarkdownParser_ParseMultipleContent_LabeledSectionsExtractPartOfSpeech()
    {
        const string markdown = """
            # 2026-08-22 단어/숙어

            ### deadline
            품사: n
            해석: 마감일, 기한
            표현: meet the deadline
            """;

        var entries = InterpretationMarkdownParser.ParseMultipleContent(markdown);

        var entry = Assert.Single(entries);
        Assert.Equal("deadline", entry.Text);
        Assert.Equal("n", entry.PartOfSpeech);
        Assert.Equal("마감일, 기한", entry.Ko);
        Assert.Equal("meet the deadline", entry.Expression);
    }

    [Fact]
    public void InterpretationMarkdownParser_FormatMultiple_ThenParseMultipleContent_RoundTrips()
    {
        var entries = new List<InterpretationEntry>
        {
            new() { Text = "deadline", PartOfSpeech = "n", Ko = "마감일, 기한", Expression = "meet the deadline" },
            new() { Text = "unique", PartOfSpeech = "adj", Ko = "독특한", Expression = "unique to" },
        };

        var formatted = InterpretationMarkdownParser.FormatMultiple(entries);
        var reparsed = InterpretationMarkdownParser.ParseMultipleContent(formatted);

        Assert.Equal(2, reparsed.Count);
        Assert.Equal("deadline", reparsed[0].Text);
        Assert.Equal("n", reparsed[0].PartOfSpeech);
        Assert.Equal("마감일, 기한", reparsed[0].Ko);
        Assert.Equal("meet the deadline", reparsed[0].Expression);
        Assert.Equal("unique", reparsed[1].Text);
        Assert.Equal("adj", reparsed[1].PartOfSpeech);
    }

    [Fact]
    public void InterpretationMarkdownParser_ParseMultiple_DataFile_ParsesEveryEntry()
    {
        var path = Path.Combine(FindRepoRoot(), "data", "2026-08-22-word.md");

        var entries = InterpretationMarkdownParser.ParseMultiple(path);

        Assert.Equal(18, entries.Count);
        Assert.Contains(entries, e => e.Text == "deadline" && e.Ko == "마감일, 기한");
        Assert.Contains(entries, e => e.Text == "figure out" && e.Ko == "알아내다, 이해하다");
    }

    [Fact]
    public void InterpretationMarkdownParser_ParseAny_TodayMdStyleFile_UsesCompactWordFormat()
    {
        var path = Path.Combine(Path.GetTempPath(), $"today-{Guid.NewGuid()}.md");
        try
        {
            File.WriteAllText(path, "deadline(n) (마감일, 기한) (meet the deadline)\n\nflexible(adj) (유연한) (a flexible schedule)\n");

            var entries = InterpretationMarkdownParser.ParseAny(path);

            Assert.Equal(2, entries.Count);
            Assert.Contains(entries, e => e.Text == "deadline" && e.PartOfSpeech == "n" && e.Ko == "마감일, 기한" && e.Expression == "meet the deadline");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void InterpretationMarkdownParser_ParseAny_StructuredFile_FallsBackToParseMultiple()
    {
        var path = Path.Combine(Path.GetTempPath(), $"structured-{Guid.NewGuid()}.md");
        try
        {
            File.WriteAllText(path, "# 2026-08-22 단어/숙어\n\n### deadline\n마감일, 기한\n\n### protection\n보호\n");

            var entries = InterpretationMarkdownParser.ParseAny(path);

            Assert.Equal(2, entries.Count);
            Assert.Contains(entries, e => e.Text == "deadline" && e.Ko == "마감일, 기한");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void WritingMarkdownParser_ParseMultipleContent_PlainBodyBecomesExample()
    {
        const string markdown = """
            # 2026-08-22 영작

            ### deadline
            Harry missed a deadline to tell the palace of his plans.

            ### protection
            The U.K. would not pay for police protection for them.
            """;

        var entries = WritingMarkdownParser.ParseMultipleContent(markdown);

        Assert.Equal(2, entries.Count);
        Assert.Equal(string.Empty, entries[0].Description);
        Assert.Equal("Harry missed a deadline to tell the palace of his plans.", entries[0].Example);
    }

    [Fact]
    public void WritingMarkdownParser_ParseMultiple_DataFile_ParsesEveryEntry()
    {
        var path = Path.Combine(FindRepoRoot(), "data", "2026-08-22-writings.md");

        var entries = WritingMarkdownParser.ParseMultiple(path);

        Assert.Equal(18, entries.Count);
        Assert.Contains(entries, e => e.Text == "deadline" && e.Example.Contains("missed a deadline"));
    }

    [Fact]
    public void WritingMarkdownParser_FormatMultiple_ThenParseMultipleContent_RoundTrips()
    {
        var entries = new List<WritingEntry>
        {
            new() { Text = "deadline", Description = "d1", Example = "Harry missed a deadline." },
            new() { Text = "protection", Description = "d2", Example = "They needed police protection." },
        };

        var formatted = WritingMarkdownParser.FormatMultiple(entries);
        var reparsed = WritingMarkdownParser.ParseMultipleContent(formatted);

        Assert.Equal(2, reparsed.Count);
        Assert.Equal("deadline", reparsed[0].Text);
        Assert.Equal("d1", reparsed[0].Description);
        Assert.Equal("Harry missed a deadline.", reparsed[0].Example);
        Assert.Equal("protection", reparsed[1].Text);
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
