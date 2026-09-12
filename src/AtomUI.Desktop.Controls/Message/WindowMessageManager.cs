using System.Collections.ObjectModel;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[TemplatePart("PART_Items", typeof(FeedbackStackPresenter))]
[PseudoClasses(NotificationPseudoClass.TopLeft,
    NotificationPseudoClass.TopRight,
    NotificationPseudoClass.BottomLeft,
    NotificationPseudoClass.BottomRight,
    NotificationPseudoClass.TopCenter,
    NotificationPseudoClass.BottomCenter)]
public class WindowMessageManager : TemplatedControl, IMessageManager, IMotionAwareControl, IDisposable
{
    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<WindowMessageManager, NotificationPosition>(
            nameof(Position), NotificationPosition.TopCenter);

    public static readonly StyledProperty<int> MaxItemsProperty =
        AvaloniaProperty.Register<WindowMessageManager, int>(nameof(MaxItems));

    public static readonly StyledProperty<bool> IsStackEnabledProperty =
        AvaloniaProperty.Register<WindowMessageManager, bool>(nameof(IsStackEnabled));

    public static readonly StyledProperty<int> StackThresholdProperty =
        AvaloniaProperty.Register<WindowMessageManager, int>(nameof(StackThreshold), 3);

    public static readonly StyledProperty<bool> IsPauseOnHoverProperty =
        AvaloniaProperty.Register<WindowMessageManager, bool>(nameof(IsPauseOnHover), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<WindowMessageManager>();

    private readonly ObservableCollection<MessageCard> _cards = new();
    private TopLevel? _topLevel;
    private bool _isDisposed;
    private Panel? _hostLayer;
    private bool _hostLayerUsesNativeAdorner;
    private IDisposable? _safeAreaMarginSubscription;
    private FeedbackStackPresenter? _presenter;
    private FeedbackLifetimeScheduler? _lifetimeScheduler;
    private bool _isLifecyclePaused = true;
    private bool _isStackPaused;
    private const int MaxHostLayerRetryCount = 30;
    private bool _hostLayerRetryScheduled;
    private int _hostLayerRetryCount;

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public int MaxItems
    {
        get => GetValue(MaxItemsProperty);
        set => SetValue(MaxItemsProperty, value);
    }

    public bool IsStackEnabled
    {
        get => GetValue(IsStackEnabledProperty);
        set => SetValue(IsStackEnabledProperty, value);
    }

    public int StackThreshold
    {
        get => GetValue(StackThresholdProperty);
        set => SetValue(StackThresholdProperty, value);
    }

    public bool IsPauseOnHover
    {
        get => GetValue(IsPauseOnHoverProperty);
        set => SetValue(IsPauseOnHoverProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal IReadOnlyList<MessageCard> Cards => _cards;

    internal bool HasLifetimeScheduler => _lifetimeScheduler is not null;

    internal int LifetimeEntryCount => _lifetimeScheduler?.Count ?? 0;

    internal bool IsLifetimePaused => _lifetimeScheduler?.IsAllPaused ?? _isLifecyclePaused;

    public WindowMessageManager(TopLevel? host)
    {
        if (host is not null)
        {
            _topLevel = host;
            InstallFromTopLevel(host);
        }
    }

    static WindowMessageManager()
    {
        HorizontalAlignmentProperty.OverrideDefaultValue<WindowMessageManager>(HorizontalAlignment.Stretch);
        VerticalAlignmentProperty.OverrideDefaultValue<WindowMessageManager>(VerticalAlignment.Stretch);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_presenter is not null)
        {
            _presenter.StackHoverChanged -= OnStackHoverChanged;
            _presenter.ItemsSource = null;
        }

        base.OnApplyTemplate(e);
        _presenter = e.NameScope.Find<FeedbackStackPresenter>("PART_Items");
        if (_presenter is not null)
        {
            _presenter[!FeedbackStackPresenter.PositionProperty] = this[!PositionProperty];
            _presenter[!FeedbackStackPresenter.IsStackEnabledProperty] = this[!IsStackEnabledProperty];
            _presenter[!FeedbackStackPresenter.StackThresholdProperty] = this[!StackThresholdProperty];
            _presenter.StackMode = FeedbackStackMode.Message;
            _presenter.ItemsSource = _cards;
            _presenter.StackHoverChanged += OnStackHoverChanged;
        }
        UpdatePseudoClasses(Position);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isLifecyclePaused = false;
        UpdateSchedulerPauseState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isLifecyclePaused = true;
        UpdateSchedulerPauseState();
        base.OnDetachedFromVisualTree(e);
    }

    public void Show(IMessage message, string[]? classes = null)
    {
        Dispatcher.VerifyAccess();
        if (_isDisposed)
        {
            return;
        }

        var card = new MessageCard
        {
            Icon = message.Icon,
            Message = message.Content,
            MessageType = message.Type,
            OnClose = message.OnClose,
            HoverChanged = OnCardHoverChanged
        };
        card[!MessageCard.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        if (classes is not null)
        {
            for (var i = 0; i < classes.Length; i++)
            {
                card.Classes.Add(classes[i]);
            }
        }

        card.MessageClosed += OnMessageClosed;
        _cards.Add(card);
        if (message.Expiration > TimeSpan.Zero)
        {
            var scheduler = _lifetimeScheduler ??= new FeedbackLifetimeScheduler();
            scheduler.Register(card, message.Expiration);
            UpdateSchedulerPauseState();
        }
        RemoveExcessMessages();
    }

    public void DestroyAll()
    {
        Dispatcher.VerifyAccess();
        for (var i = _cards.Count - 1; i >= 0; i--)
        {
            var card = _cards[i];
            _lifetimeScheduler?.Remove(card);
            card.Close();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PositionProperty)
        {
            UpdatePseudoClasses(change.GetNewValue<NotificationPosition>());
        }
        else if (change.Property == IsPauseOnHoverProperty && !change.GetNewValue<bool>())
        {
            _isStackPaused = false;
            UpdateSchedulerPauseState();
            for (var i = 0; i < _cards.Count; i++)
            {
                _lifetimeScheduler?.SetPaused(_cards[i], false);
            }
        }
        else if (change.Property == IsStackEnabledProperty && !change.GetNewValue<bool>())
        {
            _isStackPaused = false;
            UpdateSchedulerPauseState();
        }
    }

    private void OnCardHoverChanged(IFeedbackStackItem item, bool isPointerOver)
    {
        if (IsPauseOnHover)
        {
            _lifetimeScheduler?.SetPaused(item, isPointerOver);
        }
    }

    private void OnStackHoverChanged(object? sender, FeedbackStackHoverChangedEventArgs e)
    {
        _isStackPaused = e.IsStackInteraction && e.IsPointerOver && IsPauseOnHover;
        UpdateSchedulerPauseState();
        _lifetimeScheduler?.Refresh();
    }

    private void UpdateSchedulerPauseState()
    {
        _lifetimeScheduler?.SetAllPaused(_isLifecyclePaused || _isStackPaused);
    }

    private void OnMessageClosed(object? sender, RoutedEventArgs e)
    {
        if (sender is not MessageCard card)
        {
            return;
        }

        _lifetimeScheduler?.Remove(card);
        card.MessageClosed -= OnMessageClosed;
        card.HoverChanged = null;
        _cards.Remove(card);
        var callback = card.OnClose;
        card.OnClose = null;
        try
        {
            callback?.Invoke();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"Message close callback failed: {exception.Message}");
        }
    }

    private void RemoveExcessMessages()
    {
        if (MaxItems <= 0)
        {
            return;
        }

        var activeCount = 0;
        for (var i = 0; i < _cards.Count; i++)
        {
            if (!_cards[i].IsClosing)
            {
                activeCount++;
            }
        }

        var excessCount = activeCount - MaxItems;
        for (var i = 0; i < _cards.Count && excessCount > 0; i++)
        {
            var card = _cards[i];
            if (card.IsClosing)
            {
                continue;
            }
            _lifetimeScheduler?.Remove(card);
            card.Close();
            excessCount--;
        }
    }

    private void InstallFromTopLevel(TopLevel topLevel)
    {
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        topLevel.TemplateApplied += TopLevelOnTemplateApplied;
        TryInstallHostLayer(topLevel);
        _safeAreaMarginSubscription?.Dispose();
        _safeAreaMarginSubscription = TopLevelMarginBinder.BindHostMargin(topLevel, margin => Margin = margin);
    }

    private void TryInstallHostLayer(TopLevel topLevel)
    {
        _hostLayer = WindowFeedbackLayer.GetLayer(topLevel);
        _hostLayerUsesNativeAdorner = false;
        if (_hostLayer is null)
        {
            _hostLayer = AdornerLayer.GetAdornerLayer(topLevel);
            _hostLayerUsesNativeAdorner = _hostLayer is not null;
        }
        if (_hostLayer is null)
        {
            ScheduleHostLayerRetry(topLevel);
            return;
        }

        _hostLayerRetryCount = 0;
        _hostLayerRetryScheduled = false;
        if (!_hostLayer.Children.Contains(this))
        {
            _hostLayer.Children.Add(this);
        }
        AdornerLayer.SetAdornedElement(this, _hostLayerUsesNativeAdorner ? _hostLayer : null);
    }

    private void ScheduleHostLayerRetry(TopLevel topLevel)
    {
        if (_hostLayerRetryScheduled || _isDisposed || _hostLayerRetryCount >= MaxHostLayerRetryCount)
        {
            return;
        }
        _hostLayerRetryScheduled = true;
        if (_hostLayerRetryCount == 0)
        {
            Dispatcher.UIThread.Post(() => RetryInstallFromTopLevel(topLevel), DispatcherPriority.Loaded);
        }
        else
        {
            DispatcherTimer.RunOnce(() => RetryInstallFromTopLevel(topLevel), TimeSpan.FromMilliseconds(16));
        }
    }

    private void RetryInstallFromTopLevel(TopLevel topLevel)
    {
        if (!_hostLayerRetryScheduled)
        {
            return;
        }
        _hostLayerRetryScheduled = false;
        if (_isDisposed || _hostLayer is not null || !ReferenceEquals(_topLevel, topLevel))
        {
            return;
        }
        _hostLayerRetryCount++;
        TryInstallHostLayer(topLevel);
    }

    private void TopLevelOnTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        RemoveFromHostLayer();
        _hostLayerRetryScheduled = false;
        _hostLayerRetryCount = 0;
        var topLevel = (TopLevel)sender!;
        topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        InstallFromTopLevel(topLevel);
    }

    private void RemoveFromHostLayer()
    {
        _safeAreaMarginSubscription?.Dispose();
        _safeAreaMarginSubscription = null;
        if (_hostLayer is null)
        {
            return;
        }
        _hostLayer.Children.Remove(this);
        if (_hostLayerUsesNativeAdorner)
        {
            AdornerLayer.SetAdornedElement(this, null);
        }
        _hostLayer = null;
        _hostLayerUsesNativeAdorner = false;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        if (_topLevel is not null)
        {
            _topLevel.TemplateApplied -= TopLevelOnTemplateApplied;
        }
        if (_presenter is not null)
        {
            _presenter.StackHoverChanged -= OnStackHoverChanged;
            _presenter.ItemsSource = null;
            _presenter = null;
        }
        _lifetimeScheduler?.Dispose();
        _lifetimeScheduler = null;
        for (var i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];
            card.MessageClosed -= OnMessageClosed;
            card.HoverChanged = null;
            card.OnClose = null;
        }
        _cards.Clear();
        _isLifecyclePaused = true;
        _isStackPaused = false;
        _hostLayerRetryScheduled = false;
        _hostLayerRetryCount = 0;
        RemoveFromHostLayer();
        _topLevel = null;
    }

    private void UpdatePseudoClasses(NotificationPosition position)
    {
        PseudoClasses.Set(NotificationPseudoClass.TopLeft, position == NotificationPosition.TopLeft);
        PseudoClasses.Set(NotificationPseudoClass.TopRight, position == NotificationPosition.TopRight);
        PseudoClasses.Set(NotificationPseudoClass.BottomLeft, position == NotificationPosition.BottomLeft);
        PseudoClasses.Set(NotificationPseudoClass.BottomRight, position == NotificationPosition.BottomRight);
        PseudoClasses.Set(NotificationPseudoClass.TopCenter, position == NotificationPosition.TopCenter);
        PseudoClasses.Set(NotificationPseudoClass.BottomCenter, position == NotificationPosition.BottomCenter);
    }
}
