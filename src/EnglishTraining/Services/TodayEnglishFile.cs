using System.IO;

namespace EnglishTraining.Services;

/// <summary>
/// Reads/writes the single "data/today.md" scratch file: the reading
/// window's 단어 등록/문장 등록 right-click actions append to it, and the
/// "오늘의 영어" window lets the user open/edit it directly. Words are
/// appended as plain lines; sentences are appended in bold (**text**) so the
/// two are visually distinguishable in the same file.
/// </summary>
public static class TodayEnglishFile
{
    private const string DefaultContent = "# 오늘의 영어\n\n";

    public static string ResolvePath()
    {
        return Path.Combine(RepoPaths.FindDataDirectory(), "today.md");
    }

    public static string ReadOrDefault()
    {
        var path = ResolvePath();
        return File.Exists(path) ? File.ReadAllText(path) : DefaultContent;
    }

    public static void Write(string content)
    {
        var path = ResolvePath();
        EnsureDirectory(path);
        File.WriteAllText(path, content);
    }

    public static void AppendWord(string word)
    {
        Append(word);
    }

    public static void AppendSentence(string sentence)
    {
        Append($"**{sentence}**");
    }

    private static void Append(string line)
    {
        var path = ResolvePath();
        EnsureDirectory(path);

        if (!File.Exists(path))
        {
            File.WriteAllText(path, DefaultContent);
        }

        File.AppendAllText(path, $"{line}\n\n");
    }

    private static void EnsureDirectory(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
