namespace EnglishTraining.Services;

public static class LabeledTextParser
{
    /// <summary>
    /// True if any of the given labels appears as a "라벨: 값" line or a
    /// "#### 라벨" heading line anywhere in the content.
    /// </summary>
    public static bool HasAnyLabel(string content, params string[] labels)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');
        return lines.Any(line => labels.Any(label => IsColonLabel(line, label) || IsHeadingLabel(line, label)));
    }

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
            var colonLabel = labels.FirstOrDefault(label => IsColonLabel(line, label));
            if (colonLabel is not null)
            {
                Flush();
                currentLabel = colonLabel;
                buffer.Add(line[(colonLabel.Length + 1)..].Trim());
                continue;
            }

            var headingLabel = labels.FirstOrDefault(label => IsHeadingLabel(line, label));
            if (headingLabel is not null)
            {
                Flush();
                currentLabel = headingLabel;
                // The "#### 라벨" line itself carries no value; the value starts on the next line(s).
                continue;
            }

            buffer.Add(line);
        }

        Flush();

        return (title, fields);
    }

    private static bool IsColonLabel(string line, string label) => line.StartsWith(label + ":");

    private static bool IsHeadingLabel(string line, string label) => line.TrimStart() == "#### " + label;
}
