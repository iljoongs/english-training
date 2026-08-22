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
            repository, WritingMarkdownParser.Parse, WritingMarkdownParser.Export);
        DataContext = _viewModel;

        Closing += (_, _) => _viewModel.SaveCurrentEntry();
    }

    private void OnAddNewClick(object sender, RoutedEventArgs e)
    {
        var dialog = new NewTopicDialog("새 영작 추가", "표현") { Owner = this };
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
            $"선택한 항목 {selected.Count}개를 삭제하시겠습니까?",
            "삭제 확인",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteEntries(selected);
        }
    }

    private void OnImportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Markdown 파일 (*.md)|*.md", CheckFileExists = true };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.AddEntryFromFile(dialog.FileName);
        }
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        if (EntriesListBox.SelectedItem is not WritingEntry entry)
        {
            MessageBox.Show(this, "내보낼 항목을 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog { Filter = "Markdown 파일 (*.md)|*.md", FileName = $"{entry.Text}.md" };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ExportEntry(entry, dialog.FileName);
        }
    }

    private void OnFileOpenClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "영작 관리 파일 (*.json)|*.json", CheckFileExists = true };
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
        var dialog = new SaveFileDialog { Filter = "영작 관리 파일 (*.json)|*.json", FileName = "writings.json" };
        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.SaveFileAs(dialog.FileName);
        }
    }
}
