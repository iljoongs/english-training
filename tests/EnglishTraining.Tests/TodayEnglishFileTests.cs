using EnglishTraining.Services;

namespace EnglishTraining.Tests;

// TodayEnglishFile always resolves to the real repo "data/today.md" (no
// per-test path injection point), so every test backs up whatever is there
// before mutating it and restores it afterward to avoid corrupting real data.
public class TodayEnglishFileTests
{
    [Fact]
    public void ResolvePath_PointsToDataTodayMd()
    {
        var path = TodayEnglishFile.ResolvePath();

        Assert.EndsWith(Path.Combine("data", "today.md"), path);
    }

    [Fact]
    public void AppendWord_AppendsPlainLine()
    {
        WithBackup(() =>
        {
            TodayEnglishFile.AppendWord("integration-test-word");

            var content = TodayEnglishFile.ReadOrDefault();
            Assert.Contains("integration-test-word", content);
            Assert.DoesNotContain("**integration-test-word**", content);
        });
    }

    [Fact]
    public void AppendSentence_AppendsBoldLine()
    {
        WithBackup(() =>
        {
            TodayEnglishFile.AppendSentence("This is a test sentence.");

            var content = TodayEnglishFile.ReadOrDefault();
            Assert.Contains("**This is a test sentence.**", content);
        });
    }

    [Fact]
    public void AppendWordAndSentence_ShareTheSameFile()
    {
        WithBackup(() =>
        {
            TodayEnglishFile.AppendWord("integration-test-word");
            TodayEnglishFile.AppendSentence("This is a test sentence.");

            var content = TodayEnglishFile.ReadOrDefault();
            Assert.Contains("integration-test-word", content);
            Assert.Contains("**This is a test sentence.**", content);
        });
    }

    [Fact]
    public void Write_ThenReadOrDefault_RoundTrips()
    {
        WithBackup(() =>
        {
            TodayEnglishFile.Write("# 오늘의 영어\n\nintegration-test-content\n");

            Assert.Equal("# 오늘의 영어\n\nintegration-test-content\n", TodayEnglishFile.ReadOrDefault());
        });
    }

    private static void WithBackup(Action test)
    {
        var path = TodayEnglishFile.ResolvePath();
        var existed = File.Exists(path);
        var original = existed ? File.ReadAllText(path) : null;

        try
        {
            test();
        }
        finally
        {
            if (existed)
            {
                File.WriteAllText(path, original);
            }
            else
            {
                File.Delete(path);
            }
        }
    }
}
