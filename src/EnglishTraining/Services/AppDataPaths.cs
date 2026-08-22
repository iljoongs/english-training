using System.IO;

namespace EnglishTraining.Services;

public static class AppDataPaths
{
    public static string Resolve(string fileName)
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EnglishTraining");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }
}
