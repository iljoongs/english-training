namespace EnglishTraining.Models;

public sealed class LearningExpression
{
    public string Text { get; init; } = string.Empty;
    public InterpretationInfo? Interpretation { get; init; }
    public WritingInfo? Writing { get; init; }

    public int WordCount => Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
}
