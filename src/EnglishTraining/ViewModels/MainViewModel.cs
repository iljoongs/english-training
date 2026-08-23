using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EnglishTraining.Models;
using EnglishTraining.Services;

namespace EnglishTraining.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    public const double DefaultFontSize = 18;

    private IExpressionRepository _repository;
    private string _currentText;

    private double _fontSize = DefaultFontSize;
    private bool _showInterpretation = true;
    private bool _showWriting;
    private string _fontFamilyName = FontCatalog.DefaultFontFamilyName;
    private double _lineSpacingMultiplier = 1.0;
    private MarginPreset _marginPreset = MarginPreset.Normal;
    private ReadingTheme _theme = ReadingTheme.Paper;
    private double _dimmingOpacity;

    public MainViewModel(string sourceText, IExpressionRepository repository)
    {
        _repository = repository;
        _currentText = sourceText;
        Segments = new ObservableCollection<TextSegment>(TextSegmenter.Segment(sourceText, repository));
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

    public string FontFamilyName
    {
        get => _fontFamilyName;
        set => SetField(ref _fontFamilyName, value);
    }

    public double LineSpacingMultiplier
    {
        get => _lineSpacingMultiplier;
        set => SetField(ref _lineSpacingMultiplier, value);
    }

    public MarginPreset MarginPreset
    {
        get => _marginPreset;
        set => SetField(ref _marginPreset, value);
    }

    public ReadingTheme Theme
    {
        get => _theme;
        set
        {
            if (SetField(ref _theme, value))
            {
                OnPropertyChanged(nameof(PageBackgroundBrush));
                OnPropertyChanged(nameof(ForegroundBrush));
            }
        }
    }

    public double DimmingOpacity
    {
        get => _dimmingOpacity;
        set => SetField(ref _dimmingOpacity, value);
    }

    public IReadOnlyList<string> AvailableFontFamilyNames => FontCatalog.AvailableFontFamilyNames;

    public IReadOnlyList<MarginPreset> MarginPresetOptions { get; } = Enum.GetValues<MarginPreset>();

    public IReadOnlyList<ReadingTheme> ThemeOptions { get; } = Enum.GetValues<ReadingTheme>();

    public Brush PageBackgroundBrush => Theme switch
    {
        ReadingTheme.Dark => new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E)),
        ReadingTheme.Sepia => new SolidColorBrush(Color.FromRgb(0xF4, 0xEC, 0xD8)),
        _ => CreatePaperTextureBrush(),
    };

    public Brush ForegroundBrush => Theme switch
    {
        ReadingTheme.Dark => new SolidColorBrush(Color.FromRgb(0xD4, 0xD4, 0xD4)),
        ReadingTheme.Sepia => new SolidColorBrush(Color.FromRgb(0x5B, 0x46, 0x36)),
        _ => new SolidColorBrush(Color.FromRgb(0x2B, 0x26, 0x20)),
    };

    private static Brush CreatePaperTextureBrush()
    {
        var image = new BitmapImage(new Uri("pack://application:,,,/Assets/Textures/paper.png"));
        return new ImageBrush(image)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, image.PixelWidth, image.PixelHeight),
            ViewportUnits = BrushMappingMode.Absolute,
        };
    }
}
