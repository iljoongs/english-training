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
            Full, showInterpretation: true, showWriting: true,
            out var interpretation, out var writing);

        Assert.True(result);
        Assert.NotNull(interpretation);
        Assert.NotNull(writing);
    }

    [Fact]
    public void TryBuildSections_NothingSelected_ReturnsFalse()
    {
        var result = PopupContentAssembler.TryBuildSections(
            Full, showInterpretation: false, showWriting: false,
            out var interpretation, out var writing);

        Assert.False(result);
        Assert.Null(interpretation);
        Assert.Null(writing);
    }

    [Fact]
    public void TryBuildSections_SelectedButNoData_HidesThatSectionOnly()
    {
        var result = PopupContentAssembler.TryBuildSections(
            InterpretationOnly, showInterpretation: true, showWriting: true,
            out var interpretation, out var writing);

        Assert.True(result);
        Assert.NotNull(interpretation);
        Assert.Null(writing);
    }

    [Fact]
    public void TryBuildSections_SelectedSectionHasNoDataAndNothingElseSelected_ReturnsFalse()
    {
        var result = PopupContentAssembler.TryBuildSections(
            InterpretationOnly, showInterpretation: false, showWriting: true,
            out var interpretation, out var writing);

        Assert.False(result);
        Assert.Null(interpretation);
        Assert.Null(writing);
    }
}
