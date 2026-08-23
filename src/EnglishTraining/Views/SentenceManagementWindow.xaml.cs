using System.Linq;
using System.Windows;
using System.Windows.Input;
using EnglishTraining.Services;
using EnglishTraining.ViewModels;
using Microsoft.Win32;

namespace EnglishTraining.Views;

public partial class SentenceManagementWindow : Window
{
    private readonly SentenceManagementViewModel _viewModel;
    private readonly ReadingWindow _ownerReadingWindow;

    public SentenceManagementWindow(ITopicRepository topicRepository, ReadingWindow ownerReadingWindow)
    {
        InitializeComponent();

        _ownerReadingWindow = ownerReadingWindow;
        _viewModel = new SentenceManagementViewModel(topicRepository);
        DataContext = _viewModel;

        Closing += (_, _) => _viewModel.SaveCurrentText();
    }

    private void OnAddNewClick(object sender, RoutedEventArgs e)
    {
        var dialog = new NewTopicDialog { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.AddTopic(dialog.EnteredText);
        }
    }

    private void OnAddFromFileClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Markdown files (*.md)|*.md",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.AddTopicsFromFile(dialog.FileName);
        }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        var selected = TopicsListBox.SelectedItems.Cast<TopicViewModel>().ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"Delete the selected {selected.Count} topic(s)?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            _viewModel.DeleteTopics(selected);
        }
    }

    private void OnTopicDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (TopicsListBox.SelectedItem is not TopicViewModel topic)
        {
            return;
        }

        _ownerReadingWindow.LoadTopic(topic.Topic);
    }

    private void OnFileOpenClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Sentence data (*.json)|*.json",
            CheckFileExists = true,
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.OpenFile(dialog.FileName);
        }
    }

    private void OnFileSaveClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SaveCurrentText();
    }

    private void OnFileSaveAsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Sentence data (*.json)|*.json",
            FileName = "topics.json",
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.SaveFileAs(dialog.FileName);
        }
    }

    private void OnExportClick(object sender, RoutedEventArgs e)
    {
        if (TopicsListBox.SelectedItem is not TopicViewModel topic)
        {
            MessageBox.Show(this, "Please select a topic to export.", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Markdown files (*.md)|*.md",
            FileName = $"{topic.Title}.md",
        };

        if (dialog.ShowDialog(this) == true)
        {
            _viewModel.ExportTopic(topic, dialog.FileName);
        }
    }
}
