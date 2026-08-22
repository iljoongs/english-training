using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class InterpretationMarkdownParser
{
    public static InterpretationEntry Parse(string filePath)
    {
        return ParseContent(File.ReadAllText(filePath));
    }

    public static InterpretationEntry ParseContent(string content)
    {
        var (title, fields) = LabeledTextParser.Parse(content, "해석");
        return new InterpretationEntry
        {
            Text = title,
            Ko = fields.GetValueOrDefault("해석", string.Empty),
        };
    }

    public static string Format(InterpretationEntry entry)
    {
        return $"# {entry.Text}\n\n해석: {entry.Ko}\n";
    }

    public static void Export(InterpretationEntry entry, string filePath)
    {
        File.WriteAllText(filePath, Format(entry));
    }
}
