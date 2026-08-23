using System.Windows;
using EnglishTraining.Models;
using EnglishTraining.Services;
using EnglishTraining.Views;

namespace EnglishTraining;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var topicRepository = JsonTopicRepository.CreateDefault();

        var interpretationRepository = new JsonEntryRepository<InterpretationEntry>(
            AppDataPaths.Resolve("interpretations.json"), DefaultLearningData.Interpretations());
        var writingRepository = new JsonEntryRepository<WritingEntry>(
            AppDataPaths.Resolve("writings.json"), DefaultLearningData.Writings());

        var expressionRepository = JsonExpressionRepository.LoadFromEntries(
            interpretationRepository.Entries, writingRepository.Entries);

        var settingsStore = AppSettingsStore.CreateDefault();
        var initialTopic = topicRepository.Topics.FirstOrDefault(t => t.Id == settingsStore.LastSelectedTopicId)
            ?? topicRepository.Topics.FirstOrDefault();

        var window = new ReadingWindow(
            initialTopic,
            expressionRepository,
            topicRepository,
            interpretationRepository,
            writingRepository,
            settingsStore);

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
