using EnglishTraining.Models;

namespace EnglishTraining.Services;

public interface IExpressionRepository
{
    IReadOnlyList<LearningExpression> All { get; }
    int MaxWordCount { get; }
    bool TryGetByNormalizedText(string normalizedText, out LearningExpression? expression);
}
