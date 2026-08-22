using System.Collections.ObjectModel;
using System.IO;
using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.ViewModels;

public sealed class SentenceManagementViewModel : ViewModelBase
{
    private readonly ITopicRepository _repository;
    private TopicViewModel? _selectedTopic;

    public SentenceManagementViewModel(ITopicRepository repository)
    {
        _repository = repository;
        Topics = new ObservableCollection<TopicViewModel>(repository.Topics.Select(t => new TopicViewModel(t)));
        _selectedTopic = Topics.FirstOrDefault();
    }

    public ObservableCollection<TopicViewModel> Topics { get; }

    public TopicViewModel? SelectedTopic
    {
        get => _selectedTopic;
        set
        {
            if (SetField(ref _selectedTopic, value))
            {
                _repository.Save();
                NotifyFileStatusChanged();
            }
        }
    }

    public string FileStatusText
    {
        get
        {
            var path = _repository.FilePath;
            var size = File.Exists(path) ? new FileInfo(path).Length : 0;
            return $"{path} ({FormatSize(size)})";
        }
    }

    public TopicViewModel AddTopic(string title)
    {
        var topic = new Topic { Id = Guid.NewGuid(), Title = title, Text = string.Empty };
        _repository.Add(topic);
        _repository.Save();
        NotifyFileStatusChanged();

        var viewModel = new TopicViewModel(topic);
        Topics.Add(viewModel);
        SelectedTopic = viewModel;
        return viewModel;
    }

    public List<TopicViewModel> AddTopicsFromFile(string filePath)
    {
        var topics = TopicMarkdownParser.ParseMultiple(filePath);

        var viewModels = new List<TopicViewModel>();
        foreach (var topic in topics)
        {
            _repository.Add(topic);
            var viewModel = new TopicViewModel(topic);
            Topics.Add(viewModel);
            viewModels.Add(viewModel);
        }

        _repository.Save();
        NotifyFileStatusChanged();

        SelectedTopic = viewModels.FirstOrDefault();
        return viewModels;
    }

    public void ExportTopic(TopicViewModel topic, string filePath)
    {
        TopicMarkdownParser.Export(topic.Topic, filePath);
    }

    public void DeleteTopics(IEnumerable<TopicViewModel> topics)
    {
        var toRemove = topics.ToList();
        if (toRemove.Count == 0)
        {
            return;
        }

        _repository.Remove(toRemove.Select(t => t.Id));
        _repository.Save();
        NotifyFileStatusChanged();

        foreach (var topic in toRemove)
        {
            Topics.Remove(topic);
        }

        if (SelectedTopic is not null && toRemove.Contains(SelectedTopic))
        {
            SelectedTopic = Topics.FirstOrDefault();
        }
    }

    public void SaveCurrentText()
    {
        _repository.Save();
        NotifyFileStatusChanged();
    }

    public void SaveFileAs(string filePath)
    {
        _repository.SaveAs(filePath);
        NotifyFileStatusChanged();
    }

    public void OpenFile(string filePath)
    {
        _repository.Open(filePath);

        Topics.Clear();
        foreach (var topic in _repository.Topics)
        {
            Topics.Add(new TopicViewModel(topic));
        }

        SelectedTopic = Topics.FirstOrDefault();
        NotifyFileStatusChanged();
    }

    private void NotifyFileStatusChanged()
    {
        OnPropertyChanged(nameof(FileStatusText));
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024.0:0.#} KB";
        }

        return $"{bytes / (1024.0 * 1024.0):0.#} MB";
    }
}
