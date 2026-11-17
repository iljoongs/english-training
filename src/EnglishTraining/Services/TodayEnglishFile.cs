using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EnglishTraining.Services;

/// <summary>
/// The single "data/english-today.md" memo (§27): everything the user adds via
/// the reading window's "Add to Today's English" action, grouped under one
/// "## 주제" heading per topic. Words, idioms, expressions and sentences are
/// deliberately NOT distinguished — each is just a "- item" line. The items are
/// processed later (e.g. with Claude, see <see cref="BuildClaudePrompt"/>) into
/// the lesson data kept in english-data.md (§27.4).
/// </summary>
public static partial class TodayEnglishFile
{
    public const string FileName = "english-today.md";
    public const string DefaultContent = "# English Today\n";

    public static string ResolvePath()
    {
        return Path.Combine(RepoPaths.FindDataDirectory(), FileName);
    }

    public static string ReadOrDefault(string? path = null)
    {
        path ??= ResolvePath();
        return File.Exists(path) ? File.ReadAllText(path) : DefaultContent;
    }

    public static void Write(string content, string? path = null)
    {
        path ??= ResolvePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    /// <summary>
    /// Adds "- item" under the topic's "## 제목" section (created at the end of the
    /// file if missing). Returns false if the item is empty or already listed there.
    /// </summary>
    public static bool AddItem(string topicTitle, string item, string? path = null)
    {
        item = WhitespaceRegex().Replace(item.Trim(), " ");
        if (item.Length == 0)
        {
            return false;
        }

        var content = ReadOrDefault(path);
        if (ContainsItem(content, topicTitle, item))
        {
            return false;
        }

        Write(InsertItem(content, topicTitle, item), path);
        return true;
    }

    /// <summary>
    /// Inserts "- item" at the end of the topic's section, keeping everything else
    /// untouched. Appends a new "## topic" section if the topic is not there yet.
    /// </summary>
    public static string InsertItem(string content, string topicTitle, string item)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n').ToList();
        var bullet = "- " + item;

        var headingIndex = FindTopicHeading(lines, topicTitle);
        if (headingIndex < 0)
        {
            while (lines.Count > 0 && lines[^1].Trim().Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            lines.Add(string.Empty);
            lines.Add("## " + topicTitle.Trim());
            lines.Add(bullet);
            lines.Add(string.Empty);
            return string.Join("\n", lines);
        }

        var end = FindSectionEnd(lines, headingIndex);
        var insertAt = end;
        while (insertAt > headingIndex + 1 && lines[insertAt - 1].Trim().Length == 0)
        {
            insertAt--;
        }

        lines.Insert(insertAt, bullet);
        if (insertAt + 1 < lines.Count && HeadingLineRegex().IsMatch(lines[insertAt + 1]))
        {
            lines.Insert(insertAt + 1, string.Empty);
        }

        return string.Join("\n", lines);
    }

    /// <summary>True if "- item" is already listed (case-insensitive) in the topic's section.</summary>
    public static bool ContainsItem(string content, string topicTitle, string item)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n').ToList();
        var headingIndex = FindTopicHeading(lines, topicTitle);
        if (headingIndex < 0)
        {
            return false;
        }

        var end = FindSectionEnd(lines, headingIndex);
        for (var i = headingIndex + 1; i < end; i++)
        {
            if (lines[i].Trim().Equals("- " + item, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Wraps the memo with instructions so it can be pasted into Claude as-is and
    /// turned into lesson data in the format of §30 (kept in english-data).
    /// </summary>
    public static string BuildClaudePrompt(string content)
    {
        var sb = new StringBuilder();
        sb.AppendLine("아래는 영어 기사를 읽다가 모아 둔 메모입니다. `## 주제`별로 `- 항목`이 있고,");
        sb.AppendLine("항목은 단어·숙어·표현·문장이 구분 없이 섞여 있습니다.");
        sb.AppendLine("각 항목이 무엇인지 스스로 판단해서 주제별로 영어 학습 데이터로 정리해 주세요.");
        sb.AppendLine();
        sb.AppendLine("## 출력 형식 (주제마다 코드 블록 하나)");
        sb.AppendLine();
        sb.AppendLine("```markdown");
        sb.AppendLine("## 주제 제목");
        sb.AppendLine();
        sb.AppendLine("### Translation");
        sb.AppendLine("영어 문장 | 한글 번역        ← 문장 항목만, 한 줄에 한 문장");
        sb.AppendLine();
        sb.AppendLine("### Words");
        sb.AppendLine("단어 (품사) (해석) (표현)    ← 단어·숙어·표현 항목, 한 줄에 하나");
        sb.AppendLine();
        sb.AppendLine("### Writing");
        sb.AppendLine("표현 | 쓰임 설명 | 예문       ← 직접 써 볼 만한 표현, 한 줄에 하나");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## 규칙");
        sb.AppendLine("- 문장 항목은 Translation에 넣고, 그 문장의 핵심 단어·표현은 Words나 Writing에도 넣어 주세요.");
        sb.AppendLine("- Words의 괄호 세 쌍은 모두 있어야 하고, 해석과 표현 안에는 괄호를 쓰지 마세요. 해석이 여러 개면 쉼표로 이으세요.");
        sb.AppendLine("- 품사는 n, v, adj, adv 중 하나이고, 숙어·구동사는 phrase를 쓰세요. 표현은 would rather A than B 같은 틀이나 짧은 연어입니다.");
        sb.AppendLine("- Translation의 번역, Writing의 설명·예문 안에는 | 를 쓰지 마세요.");
        sb.AppendLine("- 본문(### Text)은 메모에 없으므로 만들지 마세요.");
        sb.AppendLine();
        sb.AppendLine("## 메모");
        sb.AppendLine();
        sb.AppendLine("~~~markdown");
        sb.AppendLine(content.TrimEnd());
        sb.AppendLine("~~~");
        return sb.ToString();
    }

    private static int FindTopicHeading(List<string> lines, string topicTitle)
    {
        var wanted = "## " + topicTitle.Trim();
        return lines.FindIndex(l => l.Trim().Equals(wanted, StringComparison.OrdinalIgnoreCase));
    }

    private static int FindSectionEnd(List<string> lines, int headingIndex)
    {
        var end = lines.FindIndex(headingIndex + 1, l => HeadingLineRegex().IsMatch(l));
        return end < 0 ? lines.Count : end;
    }

    [GeneratedRegex(@"^#{1,2} ")]
    private static partial Regex HeadingLineRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
