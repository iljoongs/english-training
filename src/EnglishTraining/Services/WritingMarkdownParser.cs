using System.IO;
using EnglishTraining.Models;

namespace EnglishTraining.Services;

public static class WritingMarkdownParser
{
    public static WritingEntry Parse(string filePath)
    {
        return ParseContent(File.ReadAllText(filePath));
    }

    public static WritingEntry ParseContent(string content)
    {
        var (title, fields) = LabeledTextParser.Parse(content, "설명", "예문");
        return new WritingEntry
        {
            Text = title,
            Description = fields.GetValueOrDefault("설명", string.Empty),
            Example = fields.GetValueOrDefault("예문", string.Empty),
        };
    }

    public static string Format(WritingEntry entry)
    {
        return $"# {entry.Text}\n\n설명: {entry.Description}\n예문: {entry.Example}\n";
    }

    public static void Export(WritingEntry entry, string filePath)
    {
        File.WriteAllText(filePath, Format(entry));
    }
}
