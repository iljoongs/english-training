using System.Text.RegularExpressions;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static partial class TextSegmenter
{
    public static List<TextSegment> Segment(string rawText, IExpressionRepository repository)
    {
        var segments = new List<TextSegment>();
        if (string.IsNullOrEmpty(rawText))
        {
            return segments;
        }

        var tokens = WordTokenRegex().Matches(rawText)
            .Select(m => (Start: m.Index, End: m.Index + m.Length, Normalized: TextNormalizer.Normalize(m.Value)))
            .ToList();

        var plainStart = 0;
        var i = 0;
        while (i < tokens.Count)
        {
            LearningExpression? matched = null;
            var matchedLen = 0;

            var maxLen = Math.Min(repository.MaxWordCount, tokens.Count - i);
            for (var len = maxLen; len >= 1; len--)
            {
                var phrase = string.Join(" ", tokens.Skip(i).Take(len).Select(t => t.Normalized));
                if (repository.TryGetByNormalizedText(phrase, out var expression))
                {
                    matched = expression;
                    matchedLen = len;
                    break;
                }
            }

            if (matched != null)
            {
                var matchStart = tokens[i].Start;
                var matchEnd = tokens[i + matchedLen - 1].End;

                if (matchStart > plainStart)
                {
                    segments.Add(new TextSegment { DisplayText = rawText[plainStart..matchStart], IsMatch = false });
                }

                segments.Add(new TextSegment
                {
                    DisplayText = rawText[matchStart..matchEnd],
                    IsMatch = true,
                    Expression = matched,
                });

                plainStart = matchEnd;
                i += matchedLen;
            }
            else
            {
                i += 1;
            }
        }

        if (plainStart < rawText.Length)
        {
            segments.Add(new TextSegment { DisplayText = rawText[plainStart..], IsMatch = false });
        }

        return segments;
    }

    [GeneratedRegex(@"[A-Za-z']+")]
    private static partial Regex WordTokenRegex();
}
