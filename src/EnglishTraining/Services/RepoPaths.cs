using System.IO;

namespace EnglishTraining.Services;

/// <summary>
/// Resolves paths relative to the repository root (identified by
/// EnglishTraining.sln), for the "data/" staging folder that the user
/// curates/consumes directly in the working copy — as opposed to app
/// runtime state, which lives under %LOCALAPPDATA% (see AppDataPaths).
/// </summary>
public static class RepoPaths
{
    public static string FindDataDirectory()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "EnglishTraining.sln")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        if (dir is null)
        {
            throw new InvalidOperationException("Repo root (EnglishTraining.sln) not found.");
        }

        return Path.Combine(dir, "data");
    }
}
