using System.IO;
using System.Windows;
using EnglishTraining.Services;

namespace EnglishTraining.Views;

/// <summary>
/// Free-text editor for one topic's "Today's English" memo (§27.1). Save writes
/// "data/today/{topic}.md"; Copy for Claude puts the memo plus lesson-file
/// instructions on the clipboard so it can be pasted into Claude.
/// </summary>
public partial class TodayEnglishWindow : Window
{
    private readonly string _topicTitle;
    private readonly string _filePath;

    public TodayEnglishWindow(string topicTitle, string topicText)
    {
        InitializeComponent();

        _topicTitle = topicTitle;
        _filePath = TodayEnglishFile.ResolvePath(topicTitle);
        Title = $"Today's English - {topicTitle}";
        ContentTextBox.Text = TodayEnglishFile.ReadOrTemplate(topicTitle, topicText);
        UpdateStatus();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        TodayEnglishFile.Write(_topicTitle, ContentTextBox.Text);
        UpdateStatus();
    }

    private void OnCopyForClaudeClick(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(TodayEnglishFile.BuildClaudePrompt(ContentTextBox.Text));
        StatusText.Text = "Copied to clipboard - paste it into Claude.";
    }

    private void UpdateStatus()
    {
        var size = File.Exists(_filePath) ? new FileInfo(_filePath).Length : 0;
        StatusText.Text = $"{_filePath} ({size:N0} bytes)";
    }
}
