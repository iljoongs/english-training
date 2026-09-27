using System.IO;
using System.Windows;
using EnglishTraining.Services;
using Microsoft.Win32;

namespace EnglishTraining.Views;

/// <summary>
/// Lets the user pick the data folder (§31.4) the reading window reads
/// lesson files from. Modal; OK validates the folder exists before closing.
/// </summary>
public partial class SettingsWindow : Window
{
    public string? SelectedFolder { get; private set; }

    public SettingsWindow(string? currentDataFolder)
    {
        InitializeComponent();

        DataFolderTextBox.Text = currentDataFolder ?? string.Empty;

        if (DataFolderPaths.TryGetDefaultDataFolder(out var defaultFolder) && Directory.Exists(defaultFolder))
        {
            DefaultFolderButton.IsEnabled = true;
            DefaultFolderButton.ToolTip = defaultFolder;
        }
        else
        {
            DefaultFolderButton.IsEnabled = false;
            DefaultFolderButton.ToolTip = DataFolderPaths.TryGetDefaultDataFolder(out var attempted)
                ? $"{attempted} (not found)"
                : "Repository root not found";
        }
    }

    private void OnBrowseClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            InitialDirectory = Directory.Exists(DataFolderTextBox.Text) ? DataFolderTextBox.Text : string.Empty,
        };

        if (dialog.ShowDialog(this) == true)
        {
            DataFolderTextBox.Text = dialog.FolderName;
        }
    }

    private void OnDefaultFolderClick(object sender, RoutedEventArgs e)
    {
        if (DataFolderPaths.TryGetDefaultDataFolder(out var defaultFolder))
        {
            DataFolderTextBox.Text = defaultFolder;
        }
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        var folder = DataFolderTextBox.Text.Trim();
        if (!Directory.Exists(folder))
        {
            MessageBox.Show(this, $"Folder not found:\n{folder}", "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SelectedFolder = folder;
        DialogResult = true;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
