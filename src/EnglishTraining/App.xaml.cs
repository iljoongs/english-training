using System.Windows;
using EnglishTraining.Services;
using EnglishTraining.Views;

namespace EnglishTraining;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settingsStore = AppSettingsStore.CreateDefault();
        var window = new ReadingWindow(settingsStore);

        if (settingsStore.WindowWidth is { } width && settingsStore.WindowHeight is { } height)
        {
            window.Width = width;
            window.Height = height;
        }

        if (settingsStore.WindowLeft is { } left && settingsStore.WindowTop is { } top)
        {
            window.Left = left;
            window.Top = top;
        }

        MainWindow = window;
        window.Show();
    }
}
