using EnglishTraining.Models;

namespace EnglishTraining.ViewModels;

public sealed class TopicViewModel : ViewModelBase
{
    private string _title;
    private string _text;

    public TopicViewModel(Topic topic)
    {
        Topic = topic;
        _title = topic.Title;
        _text = topic.Text;
    }

    public Topic Topic { get; }

    public Guid Id => Topic.Id;

    public string Title
    {
        get => _title;
        set
        {
            if (SetField(ref _title, value))
            {
                Topic.Title = value;
            }
        }
    }

    public string Text
    {
        get => _text;
        set
        {
            if (SetField(ref _text, value))
            {
                Topic.Text = value;
            }
        }
    }
}
