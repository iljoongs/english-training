using System.IO;
using System.Linq;
using System.Windows;
using EnglishTraining.Models;
using EnglishTraining.Services;
using EnglishTraining.ViewModels;
using Microsoft.Win32;

namespace EnglishTraining.Views;

public partial class InterpretationManagementWindow : Window
{
    private readonly EntryManagementViewModel<InterpretationEntry> _viewModel;

    public InterpretationManagementWindow(IEntryRepository<InterpretationEntry> repository)
    {
        InitializeComponent();

        _viewModel = new EntryManagementViewModel<InterpretationEntry>(
            repository, InterpretationMarkdownParser.ParseAny, InterpretationMarkdownParser.Export);
        DataContext = _viewModel;

        Closing += (_, _) => _viewModel.SaveCurrentEntry();
    }

    private void OnAddNewClick(object sender, RoutedEventArgs e)
    {
        var dialog = new NewTopicDialog("Add New Word", "Word/Idiom") { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.AddEntry(dialog.EnteredText);
        }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var selected = EntriesListBox.SelectedItems.Cast<InterpretationEntry>().ToList();
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

    private void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }

    private void OnPreviewFileDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
        foreach (var path in paths.Where(p => Path.GetExtension(p).Equals(".md", StringComparison.OrdinalIgnoreCase)))
        {
            _viewModel.AddEntriesFromFile(path);
        }

        e.Handled = true;
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        if (EntriesListBox.SelectedItem is not InterpretationEntry entry)
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
        var dialog = new OpenFileDialog { Filter = "Word data (*.json)|*.json", CheckFileExists = true };
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
        var dialog = new SaveFileDialog { Filter = "Word data (*.json)|*.json", FileName = "interpretations.json" };
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
