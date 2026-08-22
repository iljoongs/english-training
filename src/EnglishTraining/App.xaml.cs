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
        var expressionEntryRepository = new JsonEntryRepository<ExpressionEntry>(
            AppDataPaths.Resolve("expressions.json"), DefaultLearningData.Expressions());

        var expressionRepository = JsonExpressionRepository.LoadFromEntries(
            interpretationRepository.Entries, writingRepository.Entries, expressionEntryRepository.Entries);

        var initialTopic = topicRepository.Topics.FirstOrDefault();

        var window = new ReadingWindow(
            initialTopic,
            expressionRepository,
            topicRepository,
            interpretationRepository,
            writingRepository,
            expressionEntryRepository);
        MainWindow = window;
        window.Show();
    }
}
