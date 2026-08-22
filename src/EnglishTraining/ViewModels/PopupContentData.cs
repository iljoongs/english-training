using EnglishTraining.Models;

namespace EnglishTraining.ViewModels;

public sealed class PopupContentData
{
    public required string Title { get; init; }
    public InterpretationInfo? Interpretation { get; init; }
    public WritingInfo? Writing { get; init; }
    public ExpressionInfo? Expression { get; init; }
}
