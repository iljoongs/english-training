using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class DataFolderPathsTests
{
    [Fact]
    public void TryGetDefaultDataFolder_ReturnsSiblingEnglishDataTrainingFolder()
    {
        var repoRoot = FindRepoRoot();
        var expected = Path.Combine(Path.GetDirectoryName(repoRoot)!, "english-data", "training");

        var found = DataFolderPaths.TryGetDefaultDataFolder(out var dataFolder);

        Assert.True(found);
        Assert.Equal(expected, dataFolder);
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "EnglishTraining.sln")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        return dir ?? throw new InvalidOperationException("Repo root not found.");
    }
}
