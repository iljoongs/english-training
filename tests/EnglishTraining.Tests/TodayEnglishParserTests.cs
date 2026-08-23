using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class TodayEnglishParserTests
{
    [Fact]
    public void Parse_BoldSentenceFollowedByTranslation_ProducesInterpretationEntry()
    {
        const string content = """
            # 오늘의 영어

            **I was wondering if you could help me with this.**
            (이것 좀 도와주실 수 있는지 궁금했어요.)
            """;

        TodayEnglishParser.Parse(content, out var interpretations);

        var entry = Assert.Single(interpretations);
        Assert.Equal("I was wondering if you could help me with this.", entry.Text);
        Assert.Equal("이것 좀 도와주실 수 있는지 궁금했어요.", entry.Ko);
    }

    [Fact]
    public void Parse_BareWordLine_IsSkipped()
    {
        const string content = """
            # 오늘의 영어

            postpone
            """;

        TodayEnglishParser.Parse(content, out var interpretations);

        Assert.Empty(interpretations);
    }

    [Fact]
    public void Parse_BoldSentenceWithNoTranslationLine_IsSkipped()
    {
        const string content = """
            # 오늘의 영어

            **Let's touch base again next week.**

            **This one has a translation.**
            (이건 번역이 있음.)
            """;

        TodayEnglishParser.Parse(content, out var interpretations);

        var entry = Assert.Single(interpretations);
        Assert.Equal("This one has a translation.", entry.Text);
    }

    [Fact]
    public void Parse_MultipleSentences_ProducesEntryForEach()
    {
        const string content = """
            # 오늘의 영어

            **First sentence.**
            (첫 번째 문장.)

            **Second sentence.**
            (두 번째 문장.)
            """;

        TodayEnglishParser.Parse(content, out var interpretations);

        Assert.Equal(2, interpretations.Count);
    }

    [Fact]
    public void Parse_WordLineWithThreeGroups_ProducesEntryWithPartOfSpeech()
    {
        const string content = "Astronomers(n) (천문학자들) (astronomers say/discovered ~)";

        TodayEnglishParser.Parse(content, out var interpretations);

        var entry = Assert.Single(interpretations);
        Assert.Equal("Astronomers", entry.Text);
        Assert.Equal("n", entry.PartOfSpeech);
        Assert.Equal("천문학자들", entry.Ko);
        Assert.Equal("astronomers say/discovered ~", entry.Expression);
    }

    [Fact]
    public void Parse_WordLineWithTwoGroups_IsSkipped()
    {
        // Older "단어(뜻) (사용법)" two-group format is no longer recognized.
        const string content = "deadline(마감일, 기한) (meet the deadline)";

        TodayEnglishParser.Parse(content, out var interpretations);

        Assert.Empty(interpretations);
    }

    [Fact]
    public void Parse_DuplicateWordLines_ProducesBothRawEntries()
    {
        const string content = """
            unique(adj) (독특한, 유일무이한) (unique to ~)

            unique(adj) (독특한, 유일무이한) (unique to ~)
            """;

        TodayEnglishParser.Parse(content, out var interpretations);

        Assert.Equal(2, interpretations.Count);
    }

    [Fact]
    public void Parse_MixedWordAndSentenceLines_ProducesEntryForEach()
    {
        const string content = """
            deadline(n) (마감일, 기한) (meet the deadline)

            **I was wondering if you could help me with this.**
            (이것 좀 도와주실 수 있는지 궁금했어요.)
            """;

        TodayEnglishParser.Parse(content, out var interpretations);

        Assert.Equal(2, interpretations.Count);
        Assert.Contains(interpretations, e => e.Text == "deadline" && e.PartOfSpeech == "n");
        Assert.Contains(interpretations, e => e.Text == "I was wondering if you could help me with this.");
    }
}
