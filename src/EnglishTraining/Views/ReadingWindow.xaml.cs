using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using EnglishTraining.Controls;
using EnglishTraining.Models;
using EnglishTraining.Services;
using EnglishTraining.ViewModels;
using Microsoft.Win32;

namespace EnglishTraining.Views;

public partial class ReadingWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ITopicRepository _topicRepository;
    private readonly IEntryRepository<InterpretationEntry> _interpretationRepository;
    private readonly IEntryRepository<WritingEntry> _writingRepository;
    private readonly AppSettingsStore _settingsStore;
    private readonly ObservableCollection<TopicViewModel> _topics = [];
    private readonly Popup _popup;
    private readonly PopupContentView _popupContent;
    private ExpressionSpan? _activeSpan;
    private Guid? _currentTopicId;

    public ReadingWindow(
        Topic? initialTopic,
        IExpressionRepository expressionRepository,
        ITopicRepository topicRepository,
        IEntryRepository<InterpretationEntry> interpretationRepository,
        IEntryRepository<WritingEntry> writingRepository,
        AppSettingsStore settingsStore)
    {
        InitializeComponent();

        Title = initialTopic?.Title ?? "English Training";
        _topicRepository = topicRepository;
        _interpretationRepository = interpretationRepository;
        _writingRepository = writingRepository;
        _settingsStore = settingsStore;
        _currentTopicId = initialTopic?.Id;
        _viewModel = new MainViewModel(initialTopic?.Text ?? string.Empty, expressionRepository);
        DataContext = _viewModel;
        ApplySavedDisplaySettings();

        TopicsListBox.ItemsSource = _topics;
        RefreshTopics();

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

        BuildDocument();
    }

    public void LoadTopic(Topic topic)
    {
        _popup.IsOpen = false;
        _activeSpan = null;

        Title = topic.Title;
        _currentTopicId = topic.Id;
        _settingsStore.SetLastSelectedTopic(topic.Id);
        _viewModel.LoadText(topic.Text);
        BuildDocument();
        RefreshTopics();

        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
    }

    public void RefreshTopics()
    {
        _topics.Clear();
        foreach (var topic in _topicRepository.Topics)
        {
            _topics.Add(new TopicViewModel(topic));
        }

        var selected = _topics.FirstOrDefault(t => t.Id == _currentTopicId);
        TopicsListBox.SelectedItem = selected;

        if (selected is not null)
        {
            TopicsListBox.ScrollIntoView(selected);
        }
    }

    private void OnTopicSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TopicsListBox.SelectedItem is TopicViewModel selected && selected.Id != _currentTopicId)
        {
            LoadTopic(selected.Topic);
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

    public void RefreshExpressionData()
    {
        var merged = JsonExpressionRepository.LoadFromEntries(
            _interpretationRepository.Entries,
            _writingRepository.Entries);

        _viewModel.ReloadWithSameText(merged);
        BuildDocument();
    }

    private void OnManageTopicsClick(object sender, RoutedEventArgs e)
    {
        var managementWindow = new SentenceManagementWindow(_topicRepository, this);
        managementWindow.Closed += (_, _) => RefreshTopics();
        managementWindow.Show();
    }

    private void OnManageInterpretationsClick(object sender, RoutedEventArgs e)
    {
        var managementWindow = new InterpretationManagementWindow(_interpretationRepository);
        managementWindow.Closed += (_, _) => RefreshExpressionData();
        managementWindow.Show();
    }

    private void OnManageWritingsClick(object sender, RoutedEventArgs e)
    {
        var managementWindow = new WritingManagementWindow(_writingRepository);
        managementWindow.Closed += (_, _) => RefreshExpressionData();
        managementWindow.Show();
    }

    private void OnFilesLoadClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Sentence data (*.json)|*.json", CheckFileExists = true };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        _topicRepository.Open(dialog.FileName);

        var firstTopic = _topicRepository.Topics.FirstOrDefault();
        if (firstTopic is not null)
        {
            LoadTopic(firstTopic);
        }
        else
        {
            _currentTopicId = null;
            RefreshTopics();
        }
    }

    private void OnFilesSaveClick(object sender, RoutedEventArgs e)
    {
        _topicRepository.Save();
    }

    private void OnFilesSaveAsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "Sentence data (*.json)|*.json", FileName = "topics.json" };
        if (dialog.ShowDialog(this) == true)
        {
            _topicRepository.SaveAs(dialog.FileName);
        }
    }

    private void OnSentencesImportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Markdown files (*.md)|*.md", CheckFileExists = true };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        foreach (var topic in TopicMarkdownParser.ParseMultiple(dialog.FileName))
        {
            _topicRepository.Add(topic);
        }

        _topicRepository.Save();
        RefreshTopics();
    }

    private void OnSentencesExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "Markdown files (*.md)|*.md", FileName = "sentences-export.md" };
        if (dialog.ShowDialog(this) == true)
        {
            TopicMarkdownParser.ExportMultiple(_topicRepository.Topics, dialog.FileName);
        }
    }

    private void OnWordsImportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Markdown files (*.md)|*.md", CheckFileExists = true };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        foreach (var entry in InterpretationMarkdownParser.ParseAny(dialog.FileName))
        {
            _interpretationRepository.Add(entry);
        }

        _interpretationRepository.Save();
        RefreshExpressionData();
    }

    private void OnWordsExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "Markdown files (*.md)|*.md", FileName = "words-export.md" };
        if (dialog.ShowDialog(this) == true)
        {
            InterpretationMarkdownParser.ExportMultiple(_interpretationRepository.Entries, dialog.FileName);
        }
    }

    private void OnWritingImportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Markdown files (*.md)|*.md", CheckFileExists = true };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        foreach (var entry in WritingMarkdownParser.ParseMultiple(dialog.FileName))
        {
            _writingRepository.Add(entry);
        }

        _writingRepository.Save();
        RefreshExpressionData();
    }

    private void OnWritingExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "Markdown files (*.md)|*.md", FileName = "writing-export.md" };
        if (dialog.ShowDialog(this) == true)
        {
            WritingMarkdownParser.ExportMultiple(_writingRepository.Entries, dialog.FileName);
        }
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

    private void OnImportTodayEnglishClick(object sender, RoutedEventArgs e)
    {
        var content = TodayEnglishFile.ReadOrDefault();
        var result = TodayEnglishImportService.ImportContent(content, _interpretationRepository);

        if (result.InterpretationsAdded > 0)
        {
            RefreshExpressionData();
        }

        MessageBox.Show(
            this,
            $"Added {result.InterpretationsAdded} new word(s).\n" +
            $"Skipped as duplicates: {result.DuplicatesSkipped}",
            "Import Complete",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
