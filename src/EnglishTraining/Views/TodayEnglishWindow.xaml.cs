using System.IO;
using System.Windows;
using EnglishTraining.Services;

namespace EnglishTraining.Views;

/// <summary>
/// Free-text editor for "data/english-today.md" (§27.2). Save overwrites the file;
/// Copy for Claude puts the memo plus processing instructions on the clipboard.
/// </summary>
public partial class TodayEnglishWindow : Window
{
    private readonly string _filePath;

    public TodayEnglishWindow()
    {
        InitializeComponent();

        _filePath = TodayEnglishFile.ResolvePath();
        ContentTextBox.Text = TodayEnglishFile.ReadOrDefault();
        UpdateStatus();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        TodayEnglishFile.Write(ContentTextBox.Text);
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
