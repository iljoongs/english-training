namespace EnglishTraining.Services;

public static class LabeledTextParser
{
    public static (string Title, Dictionary<string, string> Fields) Parse(string content, params string[] labels)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');

        var titleLineIndex = Array.FindIndex(lines, l => l.TrimStart().StartsWith("# "));
        var title = titleLineIndex >= 0
            ? lines[titleLineIndex].TrimStart()[2..].Trim()
            : string.Empty;

        var fields = new Dictionary<string, string>();
        string? currentLabel = null;
        var buffer = new List<string>();

        void Flush()
        {
            if (currentLabel is not null)
            {
                fields[currentLabel] = string.Join("\n", buffer).Trim();
            }

            buffer.Clear();
        }

        var bodyLines = titleLineIndex >= 0 ? lines.Skip(titleLineIndex + 1) : lines;
        foreach (var line in bodyLines)
        {
            var matchedLabel = labels.FirstOrDefault(label => line.StartsWith(label + ":"));
            if (matchedLabel is not null)
            {
                Flush();
                currentLabel = matchedLabel;
                buffer.Add(line[(matchedLabel.Length + 1)..].Trim());
            }
            else
            {
                buffer.Add(line);
            }
        }

        Flush();

        return (title, fields);
    }
}
