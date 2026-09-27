using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using EnglishTraining.Controls;
using EnglishTraining.Models;
using EnglishTraining.Services;
using EnglishTraining.ViewModels;

namespace EnglishTraining.Views;

public partial class ReadingWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly AppSettingsStore _settingsStore;
    private readonly ObservableCollection<TopicViewModel> _topics = [];
    private readonly Popup _popup;
    private readonly PopupContentView _popupContent;
    private ExpressionSpan? _activeSpan;
    private Guid? _currentTopicId;

    public ReadingWindow(AppSettingsStore settingsStore)
    {
        InitializeComponent();

        _settingsStore = settingsStore;
        _viewModel = new MainViewModel(string.Empty, new JsonExpressionRepository([]));
        DataContext = _viewModel;
        ApplySavedDisplaySettings();

        TopicsListBox.ItemsSource = _topics;

        Closing += (_, _) => _settingsStore.SetWindowBounds(Left, Top, Width, Height);

        BindingOperations.SetBinding(
            Document,
            FlowDocument.FontSizeProperty,
            new Binding(nameof(MainViewModel.FontSize)) { Source = _viewModel });

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        _popupContent = new PopupContentView();
        _popup = new Popup
        {
            Child = _popupContent,
            Placement = PlacementMode.Custom,
            CustomPopupPlacementCallback = PlacePopup,
            AllowsTransparency = true,
            StaysOpen = true,
            IsOpen = false,
        };

        ReloadData();
    }

    /// <summary>
    /// Reads the configured (or default) data folder and rebuilds the topic
    /// list / learning popup data from it (§31.2). Called on startup, after
    /// Settings is saved, and from Menu &gt; Reload (F5).
    /// </summary>
    private void ReloadData()
    {
        _popup.IsOpen = false;
        _activeSpan = null;

        var folder = ResolveDataFolder();
        var result = folder is not null
            ? LessonFolderLoader.LoadFolder(folder)
            : new LessonFolderResult { Topics = [], Words = [], Writings = [], FileCount = 0, SkippedFileCount = 0 };

        _topics.Clear();
        foreach (var topic in result.Topics)
        {
            _topics.Add(new TopicViewModel(topic));
        }

        var expressionRepository = JsonExpressionRepository.LoadFromEntries(result.Words, result.Writings);
        _viewModel.ReloadWithSameText(expressionRepository);

        var selected = _topics.FirstOrDefault(t =>
                t.Topic.SourceFileName == _settingsStore.LastSelectedTopicFile
                && t.Title == _settingsStore.LastSelectedTopicTitle)
            ?? _topics.FirstOrDefault();

        if (selected is not null)
        {
            Title = selected.Title;
            _currentTopicId = selected.Id;
            _viewModel.LoadText(selected.Text);
        }
        else
        {
            Title = "English Training";
            _currentTopicId = null;
            _viewModel.LoadText(string.Empty);
        }

        TopicsListBox.SelectedItem = selected;
        if (selected is not null)
        {
            TopicsListBox.ScrollIntoView(selected);
        }

        BuildDocument();
        UpdateStatusBar(folder, result);
        GuidanceTextBlock.Visibility = result.FileCount == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private string? ResolveDataFolder()
    {
        if (_settingsStore.DataFolder is { Length: > 0 } configured)
        {
            return configured;
        }

        if (DataFolderPaths.TryGetDefaultDataFolder(out var defaultFolder) && Directory.Exists(defaultFolder))
        {
            _settingsStore.SetDataFolder(defaultFolder);
            return defaultFolder;
        }

        return null;
    }

    private void UpdateStatusBar(string? folder, LessonFolderResult result)
    {
        StatusTextBlock.Text = folder is null
            ? "No data folder set."
            : $"{folder} · {result.FileCount} files · {result.Topics.Count} topics · " +
              $"{result.Words.Count} words · {result.Writings.Count} writing" +
              (result.SkippedFileCount > 0 ? $" · {result.SkippedFileCount} file(s) skipped" : string.Empty);
    }

    private void LoadTopic(Topic topic)
    {
        _popup.IsOpen = false;
        _activeSpan = null;

        Title = topic.Title;
        _currentTopicId = topic.Id;
        _settingsStore.SetLastSelectedTopic(topic.SourceFileName, topic.Title);
        _viewModel.LoadText(topic.Text);
        BuildDocument();

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    private void OnTopicSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TopicsListBox.SelectedItem is TopicViewModel selected && selected.Id != _currentTopicId)
        {
            LoadTopic(selected.Topic);
        }
    }

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            ReloadData();
        }
    }

    private void OnReloadClick(object sender, RoutedEventArgs e)
    {
        ReloadData();
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(_settingsStore.DataFolder) { Owner = this };
        if (settingsWindow.ShowDialog() == true && settingsWindow.SelectedFolder is { } folder)
        {
            _settingsStore.SetDataFolder(folder);
            ReloadData();
        }
    }

    private void BuildDocument()
    {
        var paragraph = new Paragraph { Margin = new Thickness(0, 0, 0, 16) };

        foreach (var segment in _viewModel.Segments)
        {
            if (segment.IsMatch && segment.Expression is not null)
            {
                var span = new ExpressionSpan(segment.DisplayText, segment.Expression);
                span.HoverStarted += OnSpanHoverStarted;
                span.HoverEnded += OnSpanHoverEnded;
                paragraph.Inlines.Add(new InlineUIContainer(span));
            }
            else
            {
                paragraph.Inlines.Add(new Run(segment.DisplayText));
            }
        }

        Document.Blocks.Clear();
        Document.Blocks.Add(paragraph);
        ApplyDocumentFormatting();
    }

    private void ApplySavedDisplaySettings()
    {
        if (_settingsStore.FontSize is { } fontSize)
        {
            _viewModel.FontSize = fontSize;
        }

        if (_settingsStore.FontFamilyName is { } fontFamilyName)
        {
            _viewModel.FontFamilyName = fontFamilyName;
        }

        if (_settingsStore.LineSpacingMultiplier is { } lineSpacingMultiplier)
        {
            _viewModel.LineSpacingMultiplier = lineSpacingMultiplier;
        }

        if (_settingsStore.MarginPreset is { } marginPreset)
        {
            _viewModel.MarginPreset = marginPreset;
        }

        if (_settingsStore.Theme is { } theme)
        {
            _viewModel.Theme = theme;
        }

        if (_settingsStore.DimmingOpacity is { } dimmingOpacity)
        {
            _viewModel.DimmingOpacity = dimmingOpacity;
        }
    }

    private static readonly HashSet<string> DocumentFormattingProperties =
    [
        nameof(MainViewModel.FontSize),
        nameof(MainViewModel.FontFamilyName),
        nameof(MainViewModel.LineSpacingMultiplier),
        nameof(MainViewModel.MarginPreset),
        nameof(MainViewModel.Theme),
    ];

    private static readonly HashSet<string> DisplaySettingsProperties =
    [
        nameof(MainViewModel.FontSize),
        nameof(MainViewModel.FontFamilyName),
        nameof(MainViewModel.LineSpacingMultiplier),
        nameof(MainViewModel.MarginPreset),
        nameof(MainViewModel.Theme),
        nameof(MainViewModel.DimmingOpacity),
    ];

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null)
        {
            return;
        }

        if (DocumentFormattingProperties.Contains(e.PropertyName))
        {
            ApplyDocumentFormatting();
        }

        if (DisplaySettingsProperties.Contains(e.PropertyName))
        {
            _settingsStore.SetDisplaySettings(
                _viewModel.FontFamilyName,
                _viewModel.FontSize,
                _viewModel.LineSpacingMultiplier,
                _viewModel.MarginPreset,
                _viewModel.Theme,
                _viewModel.DimmingOpacity);
        }
    }

    private void ApplyDocumentFormatting()
    {
        Document.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), $"./Assets/Fonts/#{_viewModel.FontFamilyName}");
        Document.PagePadding = new Thickness(GetMarginSize(_viewModel.MarginPreset));
        Document.Background = _viewModel.PageBackgroundBrush;
        Document.Foreground = _viewModel.ForegroundBrush;

        var lineHeight = _viewModel.FontSize * 1.3 * _viewModel.LineSpacingMultiplier;
        foreach (var paragraph in Document.Blocks.OfType<Paragraph>())
        {
            paragraph.LineHeight = lineHeight;
        }
    }

    private static double GetMarginSize(MarginPreset preset) => preset switch
    {
        MarginPreset.Narrow => 24,
        MarginPreset.Wide => 80,
        _ => 48,
    };

    private void OnSpanHoverStarted(object? sender, EventArgs e)
    {
        if (sender is not ExpressionSpan span)
        {
            return;
        }

        var built = PopupContentAssembler.TryBuildSections(
            span.Expression,
            _viewModel.ShowInterpretation,
            _viewModel.ShowWriting,
            out var interpretation,
            out var writing);

        if (!built)
        {
            return;
        }

        _activeSpan = span;
        _popupContent.DataContext = new PopupContentData
        {
            Title = span.Expression.Text,
            Interpretation = interpretation,
            Writing = writing,
        };

        // PlacementTarget can only change while the popup is closed.
        _popup.IsOpen = false;
        _popup.PlacementTarget = span;
        _popup.IsOpen = true;
    }

    private void OnSpanHoverEnded(object? sender, EventArgs e)
    {
        if (sender != _activeSpan)
        {
            return;
        }

        _popup.IsOpen = false;
        _activeSpan = null;
    }

    private CustomPopupPlacement[] PlacePopup(Size popupSize, Size targetSize, Point offset)
    {
        if (_popup.PlacementTarget is not FrameworkElement target)
        {
            return new[] { new CustomPopupPlacement(new Point(0, targetSize.Height + 4), PopupPrimaryAxis.None) };
        }

        var screen = SystemParameters.WorkArea;
        var targetTopLeft = target.PointToScreen(new Point(0, 0));

        var x = 0.0;
        var y = targetSize.Height + 4;

        if (targetTopLeft.X + x + popupSize.Width > screen.Right)
        {
            x -= targetTopLeft.X + x + popupSize.Width - screen.Right;
        }

        if (targetTopLeft.X + x < screen.Left)
        {
            x = screen.Left - targetTopLeft.X;
        }

        if (targetTopLeft.Y + y + popupSize.Height > screen.Bottom)
        {
            y = -popupSize.Height - 4;
        }

        return new[] { new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.None) };
    }

    private void OnDocumentContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (DocumentViewer.Selection.IsEmpty)
        {
            e.Handled = true;
        }
    }

    private void OnRegisterWordClick(object sender, RoutedEventArgs e)
    {
        var word = DocumentViewer.Selection.Text.Trim();
        if (word.Length == 0)
        {
            return;
        }

        TodayEnglishFile.AppendWord(word);
    }

    private void OnRegisterSentenceClick(object sender, RoutedEventArgs e)
    {
        var sentence = DocumentViewer.Selection.Text.Trim();
        if (sentence.Length == 0)
        {
            return;
        }

        TodayEnglishFile.AppendSentence(sentence);
    }

    private void OnTodayEnglishClick(object sender, RoutedEventArgs e)
    {
        new TodayEnglishWindow().Show();
    }
}
