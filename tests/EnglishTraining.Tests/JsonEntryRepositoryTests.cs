using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class JsonEntryRepositoryTests
{
    [Fact]
    public void Constructor_MissingFile_SeedsFromSeedListAndPersists()
    {
        var path = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var seed = new List<InterpretationEntry> { new() { Text = "look", Ko = "보다" } };
            var repo = new JsonEntryRepository<InterpretationEntry>(path, seed);

            Assert.Single(repo.Entries);
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void AddAndSave_ThenReload_RoundTripsEntries()
    {
        var path = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonEntryRepository<InterpretationEntry>(path, []);
            repo.Add(new InterpretationEntry { Text = "wondering if", Ko = "~인지 궁금하다" });
            repo.Save();

            var reloaded = new JsonEntryRepository<InterpretationEntry>(path, []);

            Assert.Single(reloaded.Entries);
            Assert.Contains(reloaded.Entries, e => e.Text == "wondering if" && e.Ko == "~인지 궁금하다");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Remove_DeletesMatchingEntriesById()
    {
        var path = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonEntryRepository<InterpretationEntry>(path, []);
            var toDelete = new InterpretationEntry { Text = "삭제 대상", Ko = "text" };
            repo.Add(toDelete);
            repo.Save();

            repo.Remove([toDelete.Id]);
            repo.Save();

            Assert.DoesNotContain(repo.Entries, e => e.Id == toDelete.Id);

            var reloaded = new JsonEntryRepository<InterpretationEntry>(path, []);
            Assert.DoesNotContain(reloaded.Entries, e => e.Id == toDelete.Id);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveAs_SwitchesFilePathAndWritesToNewLocation()
    {
        var originalPath = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        var newPath = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonEntryRepository<InterpretationEntry>(originalPath, []);

            repo.SaveAs(newPath);

            Assert.Equal(newPath, repo.FilePath);
            Assert.True(File.Exists(newPath));
        }
        finally
        {
            File.Delete(originalPath);
            File.Delete(newPath);
        }
    }

    [Fact]
    public void Open_ReplacesInMemoryEntriesWithFileContentsAndSwitchesFilePath()
    {
        var pathA = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        var pathB = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var repoA = new JsonEntryRepository<InterpretationEntry>(pathA, []);

            var repoB = new JsonEntryRepository<InterpretationEntry>(pathB, []);
            repoB.Add(new InterpretationEntry { Text = "다른 파일 항목", Ko = "text" });
            repoB.Save();

            repoA.Open(pathB);

            Assert.Equal(pathB, repoA.FilePath);
            Assert.Single(repoA.Entries);
            Assert.Contains(repoA.Entries, e => e.Text == "다른 파일 항목");
        }
        finally
        {
            File.Delete(pathA);
            File.Delete(pathB);
        }
    }
}
