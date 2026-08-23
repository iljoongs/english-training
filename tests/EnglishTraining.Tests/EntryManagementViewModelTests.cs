using EnglishTraining.Models;
using EnglishTraining.Services;
using EnglishTraining.ViewModels;

namespace EnglishTraining.Tests;

public class EntryManagementViewModelTests
{
    private static EntryManagementViewModel<InterpretationEntry> CreateViewModel(IEntryRepository<InterpretationEntry> repository)
    {
        return new EntryManagementViewModel<InterpretationEntry>(
            repository,
            InterpretationMarkdownParser.ParseMultiple,
            InterpretationMarkdownParser.Export);
    }

    [Fact]
    public void Constructor_SortsEntriesAscendingByDefault()
    {
        var path = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var repository = new JsonEntryRepository<InterpretationEntry>(path,
            [
                new() { Text = "wondering if" },
                new() { Text = "as far as I know" },
                new() { Text = "look" },
            ]);

            var viewModel = CreateViewModel(repository);

            Assert.Equal(["as far as I know", "look", "wondering if"], viewModel.Entries.Select(e => e.Text));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SortAscending_OrdersEntriesByTextAndKeepsSelection()
    {
        var path = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var repository = new JsonEntryRepository<InterpretationEntry>(path,
            [
                new() { Text = "wondering if" },
                new() { Text = "as far as I know" },
                new() { Text = "look" },
            ]);

            var viewModel = CreateViewModel(repository);
            var selected = viewModel.Entries.Single(e => e.Text == "look");
            viewModel.SelectedEntry = selected;

            viewModel.SortAscending();

            Assert.Equal(["as far as I know", "look", "wondering if"], viewModel.Entries.Select(e => e.Text));
            Assert.Same(selected, viewModel.SelectedEntry);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void SortDescending_OrdersEntriesByTextDescending()
    {
        var path = Path.Combine(Path.GetTempPath(), $"entries-{Guid.NewGuid()}.json");
        try
        {
            var repository = new JsonEntryRepository<InterpretationEntry>(path,
            [
                new() { Text = "wondering if" },
                new() { Text = "as far as I know" },
                new() { Text = "look" },
            ]);

            var viewModel = CreateViewModel(repository);

            viewModel.SortDescending();

            Assert.Equal(["wondering if", "look", "as far as I know"], viewModel.Entries.Select(e => e.Text));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
