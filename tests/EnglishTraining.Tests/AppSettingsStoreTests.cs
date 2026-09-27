using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class AppSettingsStoreTests
{
    [Fact]
    public void Constructor_MissingFile_HasNoLastSelectedTopic()
    {
        var path = Path.Combine(Path.GetTempPath(), $"settings-{Guid.NewGuid()}.json");
        try
        {
            var store = new AppSettingsStore(path);

            Assert.Null(store.LastSelectedTopicFile);
            Assert.Null(store.LastSelectedTopicTitle);
            Assert.False(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SetLastSelectedTopic_PersistsAcrossInstances()
    {
        var path = Path.Combine(Path.GetTempPath(), $"settings-{Guid.NewGuid()}.json");
        try
        {
            var store = new AppSettingsStore(path);
            store.SetLastSelectedTopic("lesson1.md", "Working from Home");

            var reloaded = new AppSettingsStore(path);

            Assert.Equal("lesson1.md", reloaded.LastSelectedTopicFile);
            Assert.Equal("Working from Home", reloaded.LastSelectedTopicTitle);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SetLastSelectedTopic_Null_ClearsPersistedValue()
    {
        var path = Path.Combine(Path.GetTempPath(), $"settings-{Guid.NewGuid()}.json");
        try
        {
            var store = new AppSettingsStore(path);
            store.SetLastSelectedTopic("lesson1.md", "Working from Home");
            store.SetLastSelectedTopic(null, null);

            var reloaded = new AppSettingsStore(path);

            Assert.Null(reloaded.LastSelectedTopicFile);
            Assert.Null(reloaded.LastSelectedTopicTitle);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SetDataFolder_PersistsAcrossInstances()
    {
        var path = Path.Combine(Path.GetTempPath(), $"settings-{Guid.NewGuid()}.json");
        try
        {
            var store = new AppSettingsStore(path);
            store.SetDataFolder(@"E:\code\english-data\training");

            var reloaded = new AppSettingsStore(path);

            Assert.Equal(@"E:\code\english-data\training", reloaded.DataFolder);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Constructor_MissingFile_HasNoWindowBounds()
    {
        var path = Path.Combine(Path.GetTempPath(), $"settings-{Guid.NewGuid()}.json");
        try
        {
            var store = new AppSettingsStore(path);

            Assert.Null(store.WindowWidth);
            Assert.Null(store.WindowHeight);
            Assert.Null(store.WindowLeft);
            Assert.Null(store.WindowTop);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SetWindowBounds_PersistsAcrossInstances()
    {
        var path = Path.Combine(Path.GetTempPath(), $"settings-{Guid.NewGuid()}.json");
        try
        {
            var store = new AppSettingsStore(path);
            store.SetWindowBounds(50, 60, 1024, 768);

            var reloaded = new AppSettingsStore(path);

            Assert.Equal(50, reloaded.WindowLeft);
            Assert.Equal(60, reloaded.WindowTop);
            Assert.Equal(1024, reloaded.WindowWidth);
            Assert.Equal(768, reloaded.WindowHeight);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SetWindowBounds_DoesNotClearLastSelectedTopic()
    {
        var path = Path.Combine(Path.GetTempPath(), $"settings-{Guid.NewGuid()}.json");
        try
        {
            var store = new AppSettingsStore(path);
            store.SetLastSelectedTopic("lesson1.md", "Working from Home");
            store.SetWindowBounds(50, 60, 1024, 768);

            var reloaded = new AppSettingsStore(path);

            Assert.Equal("lesson1.md", reloaded.LastSelectedTopicFile);
            Assert.Equal(1024, reloaded.WindowWidth);
        }
        finally
        {
            File.Delete(path);
        }
    }

}
