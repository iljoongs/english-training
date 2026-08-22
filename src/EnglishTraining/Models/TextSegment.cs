namespace EnglishTraining.Models;

public sealed class TextSegment
{
    public required string DisplayText { get; init; }
    public bool IsMatch { get; init; }
    public LearningExpression? Expression { get; init; }
}
