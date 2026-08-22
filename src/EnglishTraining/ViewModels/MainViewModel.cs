using System.Collections.ObjectModel;
using System.Windows.Input;
using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    public const double MinFontSize = 12;
    public const double MaxFontSize = 36;
    public const double DefaultFontSize = 18;
    private const double FontSizeStep = 2;

    private IExpressionRepository _repository;
    private string _currentText;

    private double _fontSize = DefaultFontSize;
    private bool _showInterpretation = true;
    private bool _showWriting;
    private bool _showExpression;

    public MainViewModel(string sourceText, IExpressionRepository repository)
    {
        _repository = repository;
        _currentText = sourceText;
        Segments = new ObservableCollection<TextSegment>(TextSegmenter.Segment(sourceText, repository));

        IncreaseFontSizeCommand = new RelayCommand(
            () => FontSize = Math.Min(MaxFontSize, FontSize + FontSizeStep),
            () => FontSize < MaxFontSize);

        DecreaseFontSizeCommand = new RelayCommand(
            () => FontSize = Math.Max(MinFontSize, FontSize - FontSizeStep),
            () => FontSize > MinFontSize);
    }

    public ObservableCollection<TextSegment> Segments { get; }

    public void LoadText(string sourceText)
    {
        _currentText = sourceText;
        Segments.Clear();
        foreach (var segment in TextSegmenter.Segment(sourceText, _repository))
        {
            Segments.Add(segment);
        }
    }

    public void ReloadWithSameText(IExpressionRepository repository)
    {
        _repository = repository;
        LoadText(_currentText);
    }

    public double FontSize
    {
        get => _fontSize;
        set => SetField(ref _fontSize, value);
    }

    public bool ShowInterpretation
    {
        get => _showInterpretation;
        set => SetField(ref _showInterpretation, value);
    }

    public bool ShowWriting
    {
        get => _showWriting;
        set => SetField(ref _showWriting, value);
    }

    public bool ShowExpression
    {
        get => _showExpression;
        set => SetField(ref _showExpression, value);
    }

    public ICommand IncreaseFontSizeCommand { get; }
    public ICommand DecreaseFontSizeCommand { get; }
}
