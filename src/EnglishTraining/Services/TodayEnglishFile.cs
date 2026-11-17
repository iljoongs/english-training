using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace EnglishTraining.Services;

/// <summary>
/// Per-topic "Today's English" memo files (§27): one markdown file per topic
/// under "data/today/", filled by the reading window's Add Word / Add Sentence
/// right-click actions and editable in the Today's English window. The file
/// layout is designed to be pasted back into Claude (see
/// <see cref="BuildClaudePrompt"/>) to turn the memo into a lesson file (§30).
/// </summary>
public static partial class TodayEnglishFile
{
    public const string WordsHeading = "Words";
    public const string SentencesHeading = "Sentences";

    public static string ResolveDirectory()
    {
        return Path.Combine(RepoPaths.FindDataDirectory(), "today");
    }

    /// <summary>File path for a topic's memo: "{directory}/{slug-of-title}.md".</summary>
    public static string ResolvePath(string topicTitle, string? directory = null)
    {
        return Path.Combine(directory ?? ResolveDirectory(), Slugify(topicTitle) + ".md");
    }

    public static string Slugify(string title)
    {
        var slug = NonSlugCharRegex().Replace(title.Trim().ToLowerInvariant(), "-").Trim('-');
        return slug.Length == 0 ? "untitled" : slug;
    }

    /// <summary>The topic's memo content, or a fresh template when no file exists yet.</summary>
    public static string ReadOrTemplate(string topicTitle, string topicText, string? directory = null)
    {
        var path = ResolvePath(topicTitle, directory);
        return File.Exists(path) ? File.ReadAllText(path) : BuildTemplate(topicTitle, topicText);
    }

    public static void Write(string topicTitle, string content, string? directory = null)
    {
        var path = ResolvePath(topicTitle, directory);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    /// <summary>Adds a word under "### Words". Returns false if it is already listed.</summary>
    public static bool AddWord(string topicTitle, string topicText, string word, string? directory = null)
    {
        return AddItem(topicTitle, topicText, WordsHeading, word, directory);
    }

    /// <summary>Adds a sentence under "### Sentences". Returns false if it is already listed.</summary>
    public static bool AddSentence(string topicTitle, string topicText, string sentence, string? directory = null)
    {
        return AddItem(topicTitle, topicText, SentencesHeading, sentence, directory);
    }

    public static string BuildTemplate(string topicTitle, string topicText)
    {
        return $"# Today's English\n\n## {topicTitle}\n\n### Text\n{topicText.Trim()}\n\n### {WordsHeading}\n\n### {SentencesHeading}\n";
    }

    /// <summary>
    /// Inserts "- item" at the end of the "### heading" section, keeping everything
    /// else in the content untouched. Appends the section if it is missing.
    /// </summary>
    public static string InsertItem(string content, string heading, string item)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n').ToList();
        var bullet = "- " + item;

        var headingIndex = lines.FindIndex(l => l.Trim().Equals("### " + heading, StringComparison.OrdinalIgnoreCase));
        if (headingIndex < 0)
        {
            while (lines.Count > 0 && lines[^1].Trim().Length == 0)
            {
                lines.RemoveAt(lines.Count - 1);
            }

            lines.Add(string.Empty);
            lines.Add("### " + heading);
            lines.Add(bullet);
            lines.Add(string.Empty);
            return string.Join("\n", lines);
        }

        var end = lines.FindIndex(headingIndex + 1, l => HeadingLineRegex().IsMatch(l));
        if (end < 0)
        {
            end = lines.Count;
        }

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

    /// <summary>True if "- item" is already listed under "### heading".</summary>
    public static bool ContainsItem(string content, string heading, string item)
    {
        var lines = content.Replace("\r\n", "\n").Split('\n');
        var inSection = false;
        foreach (var line in lines)
        {
            if (HeadingLineRegex().IsMatch(line))
            {
                inSection = line.Trim().Equals("### " + heading, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (inSection && line.Trim().Equals("- " + item, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Wraps a memo with instructions so it can be pasted into Claude as-is and
    /// turned into a lesson file in the format of §30 (english-data/training).
    /// </summary>
    public static string BuildClaudePrompt(string content)
    {
        var sb = new StringBuilder();
        sb.AppendLine("아래는 영어 기사를 읽다가 모아 둔 단어와 문장 메모입니다.");
        sb.AppendLine("이 메모와 본문(### Text)을 바탕으로 영어 학습용 레슨 파일(.md)을 만들어 주세요.");
        sb.AppendLine();
        sb.AppendLine("## 출력 형식 (코드 블록 하나로, 이 구조 그대로)");
        sb.AppendLine();
        sb.AppendLine("```markdown");
        sb.AppendLine("## 기사 제목");
        sb.AppendLine();
        sb.AppendLine("### Text");
        sb.AppendLine("(본문 그대로)");
        sb.AppendLine();
        sb.AppendLine("### Translation");
        sb.AppendLine("영어 문장 | 한글 번역        ← 본문의 모든 문장을 순서대로, 한 줄에 한 문장");
        sb.AppendLine();
        sb.AppendLine("### Words");
        sb.AppendLine("단어 (품사) (해석) (표현)    ← 한 줄에 하나");
        sb.AppendLine();
        sb.AppendLine("### Writing");
        sb.AppendLine("표현 | 쓰임 설명 | 예문       ← 한 줄에 하나");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("## 규칙");
        sb.AppendLine("- Words는 메모의 ### Words 단어를 모두 넣고, 본문에서 배울 만한 단어·숙어도 보충해 주세요.");
        sb.AppendLine("- Words의 괄호 세 쌍은 모두 있어야 하고, 해석과 표현 안에는 괄호를 쓰지 마세요. 해석이 여러 개면 쉼표로 이으세요.");
        sb.AppendLine("- 품사는 n, v, adj, adv 중 하나이고, 숙어·구동사는 phrase를 쓰세요. 표현은 would rather A than B 같은 틀이나 짧은 연어입니다.");
        sb.AppendLine("- Writing은 직접 써 보기 좋은 표현만 골라 주세요. 설명과 예문 안에는 | 를 쓰지 마세요.");
        sb.AppendLine("- Translation의 영어 문장은 Text와 한 글자도 다르지 않게 복사하세요. 번역 안에는 | 를 쓰지 마세요.");
        sb.AppendLine("- 메모의 ### Sentences 문장은 Translation에 반드시 포함되어야 하고, 그 문장의 핵심 표현은 Words나 Writing에 넣어 주세요.");
        sb.AppendLine();
        sb.AppendLine("## 메모");
        sb.AppendLine();
        sb.AppendLine("~~~markdown");
        sb.AppendLine(content.TrimEnd());
        sb.AppendLine("~~~");
        return sb.ToString();
    }

    private static bool AddItem(string topicTitle, string topicText, string heading, string item, string? directory)
    {
        item = WhitespaceRegex().Replace(item.Trim(), " ");
        if (item.Length == 0)
        {
            return false;
        }

        var content = ReadOrTemplate(topicTitle, topicText, directory);
        if (ContainsItem(content, heading, item))
        {
            return false;
        }

        Write(topicTitle, InsertItem(content, heading, item), directory);
        return true;
    }

    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex NonSlugCharRegex();

    [GeneratedRegex(@"^#{1,3} ")]
    private static partial Regex HeadingLineRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
