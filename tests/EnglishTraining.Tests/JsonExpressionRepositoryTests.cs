using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class JsonExpressionRepositoryTests
{
    private static List<InterpretationEntry> Interpretations() =>
    [
        new() { Text = "look", Ko = "보다" },
        new() { Text = "look forward to", PartOfSpeech = "phrase", Ko = "~을 기대하다", Expression = "look forward to + 동명사(-ing)" },
        new() { Text = "as far as I know", Ko = "내가 아는 한" },
    ];

    private static List<WritingEntry> Writings() =>
    [
        new() { Text = "look forward to", Description = "d", Example = "e" },
    ];

    [Fact]
    public void LoadFromEntries_MergesByNormalizedTextAndComputesMaxWordCount()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings());

        // union of texts: look, look forward to, as far as I know
        Assert.Equal(3, repo.All.Count);
        Assert.Equal(5, repo.MaxWordCount); // "as far as I know"
    }

    [Fact]
    public void LoadFromEntries_EntryPresentInBothCategories_MergesBothSections()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings());

        repo.TryGetByNormalizedText("look forward to", out var expression);

        Assert.NotNull(expression);
        Assert.Equal("phrase", expression!.Interpretation?.PartOfSpeech);
        Assert.Equal("~을 기대하다", expression.Interpretation?.Ko);
        Assert.Equal("look forward to + 동명사(-ing)", expression.Interpretation?.Expression);
        Assert.Equal("d", expression.Writing?.Description);
    }

    [Fact]
    public void LoadFromEntries_EntryPresentInOnlyOneCategory_LeavesOtherSectionNull()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings());

        repo.TryGetByNormalizedText("look", out var expression);

        Assert.NotNull(expression);
        Assert.NotNull(expression!.Interpretation);
        Assert.Null(expression.Writing);
    }

    [Fact]
    public void TryGetByNormalizedText_UnknownPhrase_ReturnsFalse()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings());

        var found = repo.TryGetByNormalizedText("not registered", out var expression);

        Assert.False(found);
        Assert.Null(expression);
    }
}
