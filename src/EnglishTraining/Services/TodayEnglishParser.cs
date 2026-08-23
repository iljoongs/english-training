using System.Text.RegularExpressions;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

/// <summary>
/// Parses "data/today.md" (§27/§29) into InterpretationEntry records:
/// * a word line "단어(품사) (해석) (표현)" (three parenthesized groups)
/// * a bold sentence line "**문장**" followed by a "(번역)" line
/// A bold sentence with no translation line yet, or a line that doesn't match
/// either shape, is skipped — it's treated as not-yet-annotated.
/// </summary>
public static partial class TodayEnglishParser
{
    public static void Parse(string content, out List<InterpretationEntry> interpretations)
    {
        interpretations = [];

        var lines = content.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var sentenceMatch = BoldSentenceLine().Match(line);
            if (sentenceMatch.Success)
            {
                var translationIndex = NextNonBlankLineIndex(lines, i + 1);
                if (translationIndex is { } index)
                {
                    var translationMatch = TranslationLine().Match(lines[index].Trim());
                    if (translationMatch.Success)
                    {
                        interpretations.Add(new InterpretationEntry
                        {
                            Text = sentenceMatch.Groups["sentence"].Value.Trim(),
                            Ko = translationMatch.Groups["ko"].Value.Trim(),
                        });
                        i = index;
                    }
                }

                continue;
            }

            var wordMatch = WordLine().Match(line);
            if (wordMatch.Success)
            {
                interpretations.Add(new InterpretationEntry
                {
                    Text = wordMatch.Groups["word"].Value.Trim(),
                    PartOfSpeech = wordMatch.Groups["pos"].Value.Trim(),
                    Ko = wordMatch.Groups["ko"].Value.Trim(),
                    Expression = wordMatch.Groups["expression"].Value.Trim(),
                });
            }
        }
    }

    private static int? NextNonBlankLineIndex(string[] lines, int start)
    {
        for (var i = start; i < lines.Length; i++)
        {
            if (lines[i].Trim().Length > 0)
            {
                return i;
            }
        }

        return null;
    }

    [GeneratedRegex(@"^\*\*(?<sentence>.+?)\*\*$")]
    private static partial Regex BoldSentenceLine();

    [GeneratedRegex(@"^\((?<ko>.+)\)$")]
    private static partial Regex TranslationLine();

    [GeneratedRegex(@"^(?<word>[^()]+?)\((?<pos>[^()]*)\)\s*\((?<ko>[^()]*)\)\s*\((?<expression>[^()]*)\)$")]
    private static partial Regex WordLine();
}
