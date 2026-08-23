using System.IO;
using System.Windows;
using EnglishTraining.Services;

namespace EnglishTraining.Views;

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

    private void UpdateStatus()
    {
        var size = File.Exists(_filePath) ? new FileInfo(_filePath).Length : 0;
        StatusText.Text = $"{_filePath} ({size:N0} bytes)";
    }
}
