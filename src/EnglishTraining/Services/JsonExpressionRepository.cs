using EnglishTraining.Models;

namespace EnglishTraining.Services;

public sealed class JsonExpressionRepository : IExpressionRepository
{
    private readonly Dictionary<string, LearningExpression> _byNormalizedText;

    public IReadOnlyList<LearningExpression> All { get; }
    public int MaxWordCount { get; }

    public JsonExpressionRepository(IReadOnlyList<LearningExpression> expressions)
    {
        All = expressions;
        MaxWordCount = expressions.Count == 0 ? 0 : expressions.Max(e => e.WordCount);

        _byNormalizedText = new Dictionary<string, LearningExpression>();
        foreach (var expression in expressions)
        {
            _byNormalizedText[TextNormalizer.Normalize(expression.Text)] = expression;
        }
    }

    public static JsonExpressionRepository LoadFromEntries(
        IReadOnlyList<InterpretationEntry> interpretations,
        IReadOnlyList<WritingEntry> writings)
    {
        var interpretationsByKey = interpretations
            .GroupBy(e => TextNormalizer.Normalize(e.Text))
            .ToDictionary(g => g.Key, g => new InterpretationInfo
            {
                PartOfSpeech = g.Last().PartOfSpeech,
                Ko = g.Last().Ko,
                Expression = g.Last().Expression,
            });

        var writingsByKey = writings
            .GroupBy(e => TextNormalizer.Normalize(e.Text))
            .ToDictionary(g => g.Key, g => new WritingInfo { Description = g.Last().Description, Example = g.Last().Example });

        var textByKey = new Dictionary<string, string>();
        foreach (var entry in interpretations)
        {
            textByKey.TryAdd(TextNormalizer.Normalize(entry.Text), entry.Text);
        }

        foreach (var entry in writings)
        {
            textByKey.TryAdd(TextNormalizer.Normalize(entry.Text), entry.Text);
        }

        var merged = textByKey.Select(kvp => new LearningExpression
        {
            Text = kvp.Value,
            Interpretation = interpretationsByKey.GetValueOrDefault(kvp.Key),
            Writing = writingsByKey.GetValueOrDefault(kvp.Key),
        }).ToList();

        return new JsonExpressionRepository(merged);
    }

    public bool TryGetByNormalizedText(string normalizedText, out LearningExpression? expression)
    {
        return _byNormalizedText.TryGetValue(normalizedText, out expression);
    }
}
