using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class ExpressionMarkdownParser
{
    public static ExpressionEntry Parse(string filePath)
    {
        return ParseContent(File.ReadAllText(filePath));
    }

    public static ExpressionEntry ParseContent(string content)
    {
        var (title, fields) = LabeledTextParser.Parse(content, "의미", "사용법", "예문");
        return new ExpressionEntry
        {
            Text = title,
            Meaning = fields.GetValueOrDefault("의미", string.Empty),
            Usage = fields.GetValueOrDefault("사용법", string.Empty),
            Example = fields.GetValueOrDefault("예문", string.Empty),
        };
    }

    public static string Format(ExpressionEntry entry)
    {
        return $"# {entry.Text}\n\n의미: {entry.Meaning}\n사용법: {entry.Usage}\n예문: {entry.Example}\n";
    }

    public static void Export(ExpressionEntry entry, string filePath)
    {
        File.WriteAllText(filePath, Format(entry));
    }
}
