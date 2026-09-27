using EnglishTraining.Services;

namespace EnglishTraining.Tests;

public class LessonFolderLoaderTests
{
    [Fact]
    public void LoadFolder_MissingFolder_ReturnsEmptyResult()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"lessons-{Guid.NewGuid()}");

        var result = LessonFolderLoader.LoadFolder(folder);

        Assert.Empty(result.Topics);
        Assert.Empty(result.Words);
        Assert.Empty(result.Writings);
        Assert.Equal(0, result.SkippedFileCount);
    }

    [Fact]
    public void LoadFolder_EmptyFolder_ReturnsEmptyResult()
    {
        var folder = CreateTempFolder();
        try
        {
            var result = LessonFolderLoader.LoadFolder(folder);

            Assert.Empty(result.Topics);
            Assert.Empty(result.Words);
            Assert.Empty(result.Writings);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void LoadFolder_MultipleFiles_ReadsInFilenameOrder()
    {
        var folder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(folder, "b-lesson.md"), "## Second\n\n### Text\nBody two.\n");
            File.WriteAllText(Path.Combine(folder, "a-lesson.md"), "## First\n\n### Text\nBody one.\n");

            var result = LessonFolderLoader.LoadFolder(folder);

            Assert.Equal(2, result.Topics.Count);
            Assert.Equal("First", result.Topics[0].Title);
            Assert.Equal("Second", result.Topics[1].Title);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void LoadFolder_DuplicateWord_KeepsFirstFileRead()
    {
        var folder = CreateTempFolder();
        try
        {
            File.WriteAllText(
                Path.Combine(folder, "a-lesson.md"),
                "## First\n\n### Words\ncommute (v) (통근하다) (commute to work)\n");
            File.WriteAllText(
                Path.Combine(folder, "b-lesson.md"),
                "## Second\n\n### Words\ncommute (n) (통근) (a long commute)\n");

            var result = LessonFolderLoader.LoadFolder(folder);

            var word = Assert.Single(result.Words);
            Assert.Equal("v", word.PartOfSpeech);
            Assert.Equal("통근하다", word.Ko);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void LoadFolder_SubfolderFiles_AreIgnored()
    {
        var folder = CreateTempFolder();
        try
        {
            var subFolder = Path.Combine(folder, "sub");
            Directory.CreateDirectory(subFolder);
            File.WriteAllText(Path.Combine(subFolder, "nested.md"), "## Nested\n\n### Text\nShould be ignored.\n");
            File.WriteAllText(Path.Combine(folder, "top.md"), "## Top\n\n### Text\nTop level.\n");

            var result = LessonFolderLoader.LoadFolder(folder);

            var topic = Assert.Single(result.Topics);
            Assert.Equal("Top", topic.Title);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void LoadFolder_LockedFile_IsSkippedAndCounted()
    {
        var folder = CreateTempFolder();
        try
        {
            var lockedPath = Path.Combine(folder, "a-locked.md");
            File.WriteAllText(lockedPath, "## Locked\n\n### Text\nUnreadable.\n");
            File.WriteAllText(Path.Combine(folder, "b-ok.md"), "## Ok\n\n### Text\nReadable.\n");

            using (new FileStream(lockedPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var result = LessonFolderLoader.LoadFolder(folder);

                Assert.Equal(1, result.SkippedFileCount);
                var topic = Assert.Single(result.Topics);
                Assert.Equal("Ok", topic.Title);
            }
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"lessons-{Guid.NewGuid()}");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
