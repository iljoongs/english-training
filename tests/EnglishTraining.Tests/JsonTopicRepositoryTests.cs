using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class JsonTopicRepositoryTests
{
    [Fact]
    public void Constructor_MissingFile_SeedsDefaultTopicAndPersists()
    {
        var path = Path.Combine(Path.GetTempPath(), $"topics-{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonTopicRepository(path);

            Assert.Single(repo.Topics);
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void AddAndSave_ThenReload_RoundTripsTopics()
    {
        var path = Path.Combine(Path.GetTempPath(), $"topics-{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonTopicRepository(path);
            repo.Add(new Topic { Id = Guid.NewGuid(), Title = "비즈니스 영어", Text = "Let's schedule a meeting." });
            repo.Save();

            var reloaded = new JsonTopicRepository(path);

            Assert.Equal(2, reloaded.Topics.Count);
            Assert.Contains(reloaded.Topics, t => t.Title == "비즈니스 영어" && t.Text == "Let's schedule a meeting.");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Remove_DeletesMatchingTopicsById()
    {
        var path = Path.Combine(Path.GetTempPath(), $"topics-{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonTopicRepository(path);
            var toDelete = new Topic { Id = Guid.NewGuid(), Title = "삭제 대상", Text = "text" };
            repo.Add(toDelete);
            repo.Save();

            repo.Remove([toDelete.Id]);
            repo.Save();

            Assert.DoesNotContain(repo.Topics, t => t.Id == toDelete.Id);

            var reloaded = new JsonTopicRepository(path);
            Assert.DoesNotContain(reloaded.Topics, t => t.Id == toDelete.Id);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveAs_SwitchesFilePathAndWritesToNewLocation()
    {
        var originalPath = Path.Combine(Path.GetTempPath(), $"topics-{Guid.NewGuid()}.json");
        var newPath = Path.Combine(Path.GetTempPath(), $"topics-{Guid.NewGuid()}.json");
        try
        {
            var repo = new JsonTopicRepository(originalPath);

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
    public void Open_ReplacesInMemoryTopicsWithFileContentsAndSwitchesFilePath()
    {
        var pathA = Path.Combine(Path.GetTempPath(), $"topics-{Guid.NewGuid()}.json");
        var pathB = Path.Combine(Path.GetTempPath(), $"topics-{Guid.NewGuid()}.json");
        try
        {
            var repoA = new JsonTopicRepository(pathA); // seeds "Sample" at pathA

            var repoB = new JsonTopicRepository(pathB); // seeds "Sample" at pathB
            repoB.Add(new Topic { Id = Guid.NewGuid(), Title = "다른 파일 주제", Text = "text" });
            repoB.Save();

            repoA.Open(pathB);

            Assert.Equal(pathB, repoA.FilePath);
            Assert.Equal(2, repoA.Topics.Count);
            Assert.Contains(repoA.Topics, t => t.Title == "다른 파일 주제");
        }
        finally
        {
            File.Delete(pathA);
            File.Delete(pathB);
        }
    }
}
