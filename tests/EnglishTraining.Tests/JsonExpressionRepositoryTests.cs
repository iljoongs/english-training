using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class JsonExpressionRepositoryTests
{
    private static List<InterpretationEntry> Interpretations() =>
    [
        new() { Text = "look", Ko = "보다" },
        new() { Text = "look forward to", Ko = "~을 기대하다" },
    ];

    private static List<WritingEntry> Writings() =>
    [
        new() { Text = "look forward to", Description = "d", Example = "e" },
    ];

    private static List<ExpressionEntry> Expressions() =>
    [
        new() { Text = "look forward to", Meaning = "m", Usage = "u", Example = "e" },
        new() { Text = "as far as I know", Meaning = "m2", Usage = "u2", Example = "e2" },
    ];

    [Fact]
    public void LoadFromEntries_MergesByNormalizedTextAndComputesMaxWordCount()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings(), Expressions());

        // union of texts: look, look forward to, as far as I know
        Assert.Equal(3, repo.All.Count);
        Assert.Equal(5, repo.MaxWordCount); // "as far as I know"
    }

    [Fact]
    public void LoadFromEntries_EntryPresentInAllThreeCategories_MergesAllSections()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings(), Expressions());

        repo.TryGetByNormalizedText("look forward to", out var expression);

        Assert.NotNull(expression);
        Assert.Equal("~을 기대하다", expression!.Interpretation?.Ko);
        Assert.Equal("d", expression.Writing?.Description);
        Assert.Equal("m", expression.Expression?.Meaning);
    }

    [Fact]
    public void LoadFromEntries_EntryPresentInOnlyOneCategory_LeavesOtherSectionsNull()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings(), Expressions());

        repo.TryGetByNormalizedText("look", out var expression);

        Assert.NotNull(expression);
        Assert.NotNull(expression!.Interpretation);
        Assert.Null(expression.Writing);
        Assert.Null(expression.Expression);
    }

    [Fact]
    public void TryGetByNormalizedText_UnknownPhrase_ReturnsFalse()
    {
        var repo = JsonExpressionRepository.LoadFromEntries(Interpretations(), Writings(), Expressions());

        var found = repo.TryGetByNormalizedText("not registered", out var expression);

        Assert.False(found);
        Assert.Null(expression);
    }
}
