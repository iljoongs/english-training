using System.IO;

namespace EnglishTraining.Services;

/// <summary>
/// Resolves the default data folder (§31.4) — the sibling repository
/// english-data's "training" folder, found the same way RepoPaths finds
/// this repo's own root (walk up from the build output to
/// EnglishTraining.sln), then stepping one directory further up to the
/// repo root's parent.
/// </summary>
public static class DataFolderPaths
{
    public static bool TryGetDefaultDataFolder(out string dataFolder)
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "EnglishTraining.sln")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        if (dir is null)
        {
            dataFolder = string.Empty;
            return false;
        }

        var repoParent = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (repoParent is null)
        {
            dataFolder = string.Empty;
            return false;
        }

        dataFolder = Path.Combine(repoParent, "english-data", "training");
        return true;
    }
}
