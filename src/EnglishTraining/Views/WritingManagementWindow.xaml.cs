using System.Linq;
using System.Windows;
using EnglishTraining.Models;
using EnglishTraining.Services;
using EnglishTraining.ViewModels;
using Microsoft.Win32;

namespace EnglishTraining.Views;

public partial class WritingManagementWindow : Window
{
    private readonly EntryManagementViewModel<WritingEntry> _viewModel;

    public WritingManagementWindow(IEntryRepository<WritingEntry> repository)
    {
        InitializeComponent();

        _viewModel = new EntryManagementViewModel<WritingEntry>(
            repository, WritingMarkdownParser.ParseMultiple, WritingMarkdownParser.Export);
        DataContext = _viewModel;

        Closing += (_, _) => _viewModel.SaveCurrentEntry();
    }

    private void OnAddNewClick(object sender, RoutedEventArgs e)
    {
        var dialog = new NewTopicDialog("Add New Writing", "Expression") { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.AddEntry(dialog.EnteredText);
        }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var selected = EntriesListBox.SelectedItems.Cast<WritingEntry>().ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"Delete the selected {selected.Count} item(s)?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteEntries(selected);
        }
    }

    private void OnImportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Markdown files (*.md)|*.md", CheckFileExists = true };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.AddEntriesFromFile(dialog.FileName);
        }
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        if (EntriesListBox.SelectedItem is not WritingEntry entry)
        {
            MessageBox.Show(this, "Please select an item to export.", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog { Filter = "Markdown files (*.md)|*.md", FileName = $"{entry.Text}.md" };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ExportEntry(entry, dialog.FileName);
        }
    }

    private void OnFileOpenClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Writing data (*.json)|*.json", CheckFileExists = true };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.OpenFile(dialog.FileName);
        }
    }

    private void OnFileSaveClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveCurrentEntry();
    }

    private void OnFileSaveAsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "Writing data (*.json)|*.json", FileName = "writings.json" };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.SaveFileAs(dialog.FileName);
        }
    }

    private void OnSortAscendingClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SortAscending();
    }

    private void OnSortDescendingClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SortDescending();
    }
}
