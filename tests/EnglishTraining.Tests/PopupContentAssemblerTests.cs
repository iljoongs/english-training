using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class PopupContentAssemblerTests
{
    private static readonly LearningExpression Full = new()
    {
        Text = "look forward to",
        Interpretation = new InterpretationInfo { Ko = "~을 기대하다" },
        Writing = new WritingInfo { Description = "d", Example = "e" },
        Expression = new ExpressionInfo { Meaning = "m", Usage = "u", Example = "e" },
    };

    private static readonly LearningExpression InterpretationOnly = new()
    {
        Text = "look",
        Interpretation = new InterpretationInfo { Ko = "보다" },
    };

    [Fact]
    public void TryBuildSections_AllSelectedWithFullData_ReturnsAllSections()
    {
        var result = PopupContentAssembler.TryBuildSections(
            Full, showInterpretation: true, showWriting: true, showExpression: true,
            out var interpretation, out var writing, out var expression);

        Assert.True(result);
        Assert.NotNull(interpretation);
        Assert.NotNull(writing);
        Assert.NotNull(expression);
    }

    [Fact]
    public void TryBuildSections_NothingSelected_ReturnsFalse()
    {
        var result = PopupContentAssembler.TryBuildSections(
            Full, showInterpretation: false, showWriting: false, showExpression: false,
            out var interpretation, out var writing, out var expression);

        Assert.False(result);
        Assert.Null(interpretation);
        Assert.Null(writing);
        Assert.Null(expression);
    }

    [Fact]
    public void TryBuildSections_SelectedButNoData_HidesThatSectionOnly()
    {
        var result = PopupContentAssembler.TryBuildSections(
            InterpretationOnly, showInterpretation: true, showWriting: true, showExpression: true,
            out var interpretation, out var writing, out var expression);

        Assert.True(result);
        Assert.NotNull(interpretation);
        Assert.Null(writing);
        Assert.Null(expression);
    }

    [Fact]
    public void TryBuildSections_SelectedSectionHasNoDataAndNothingElseSelected_ReturnsFalse()
    {
        var result = PopupContentAssembler.TryBuildSections(
            InterpretationOnly, showInterpretation: false, showWriting: true, showExpression: false,
            out var interpretation, out var writing, out var expression);

        Assert.False(result);
        Assert.Null(interpretation);
        Assert.Null(writing);
        Assert.Null(expression);
    }
}
