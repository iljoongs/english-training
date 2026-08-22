using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using EnglishTraining.Controls;
using EnglishTraining.Models;
using EnglishTraining.Services;
using EnglishTraining.ViewModels;

namespace EnglishTraining.Views;

public partial class ReadingWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ITopicRepository _topicRepository;
    private readonly IEntryRepository<InterpretationEntry> _interpretationRepository;
    private readonly IEntryRepository<WritingEntry> _writingRepository;
    private readonly IEntryRepository<ExpressionEntry> _expressionEntryRepository;
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
        IEntryRepository<ExpressionEntry> expressionEntryRepository)
    {
        InitializeComponent();

        Title = initialTopic?.Title ?? "영어 학습";
        _topicRepository = topicRepository;
        _interpretationRepository = interpretationRepository;
        _writingRepository = writingRepository;
        _expressionEntryRepository = expressionEntryRepository;
        _currentTopicId = initialTopic?.Id;
        _viewModel = new MainViewModel(initialTopic?.Text ?? string.Empty, expressionRepository);
        DataContext = _viewModel;

        TopicsListBox.ItemsSource = _topics;
        RefreshTopics();

        BindingOperations.SetBinding(
            Document,
            FlowDocument.FontSizeProperty,
            new Binding(nameof(MainViewModel.FontSize)) { Source = _viewModel });

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

        TopicsListBox.SelectedItem = _topics.FirstOrDefault(t => t.Id == _currentTopicId);
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
        var paragraph = new Paragraph { LineHeight = 28, Margin = new Thickness(0, 0, 0, 16) };

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
    }

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
            _viewModel.ShowExpression,
            out var interpretation,
            out var writing,
            out var expression);

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
            Expression = expression,
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
            _writingRepository.Entries,
            _expressionEntryRepository.Entries);

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

    private void OnManageExpressionsClick(object sender, RoutedEventArgs e)
    {
        var managementWindow = new ExpressionManagementWindow(_expressionEntryRepository);
        managementWindow.Closed += (_, _) => RefreshExpressionData();
        managementWindow.Show();
    }
}
