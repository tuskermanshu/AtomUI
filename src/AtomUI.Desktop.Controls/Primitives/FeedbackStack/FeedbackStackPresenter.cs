using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal sealed class FeedbackStackHoverChangedEventArgs(bool isPointerOver, bool isStackInteraction) : EventArgs
{
    internal bool IsPointerOver { get; } = isPointerOver;
    internal bool IsStackInteraction { get; } = isStackInteraction;
}

/// <summary>
/// Stable feedback items host that owns whole-stack hover state while delegating hot-path geometry to
/// <see cref="FeedbackStackPanel"/>.
/// </summary>
internal sealed class FeedbackStackPresenter : ItemsControl
{
    private static readonly FuncTemplate<Panel?> DefaultPanel = new(() => new FeedbackStackPanel());

    public static readonly StyledProperty<FeedbackStackMode> StackModeProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, FeedbackStackMode>(nameof(StackMode));

    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, NotificationPosition>(
            nameof(Position), NotificationPosition.TopCenter);

    public static readonly StyledProperty<bool> IsStackEnabledProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, bool>(nameof(IsStackEnabled));

    public static readonly StyledProperty<int> StackThresholdProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, int>(nameof(StackThreshold), 3);

    public static readonly StyledProperty<double> ExpandedGapProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, double>(nameof(ExpandedGap), 16);

    public static readonly StyledProperty<double> CollapsedOffsetProperty =
        AvaloniaProperty.Register<FeedbackStackPresenter, double>(nameof(CollapsedOffset), 8);

    public static readonly DirectProperty<FeedbackStackPresenter, bool> IsCollapsedProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, bool>(nameof(IsCollapsed), owner => owner.IsCollapsed);

    public static readonly DirectProperty<FeedbackStackPresenter, int> ActiveItemCountProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, int>(
            nameof(ActiveItemCount), owner => owner.ActiveItemCount);

    public static readonly DirectProperty<FeedbackStackPresenter, double> FirstBackplateWidthProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, double>(
            nameof(FirstBackplateWidth), owner => owner.FirstBackplateWidth);

    public static readonly DirectProperty<FeedbackStackPresenter, double> SecondBackplateWidthProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, double>(
            nameof(SecondBackplateWidth), owner => owner.SecondBackplateWidth);

    public static readonly DirectProperty<FeedbackStackPresenter, Thickness> FirstBackplateMarginProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, Thickness>(
            nameof(FirstBackplateMargin), owner => owner.FirstBackplateMargin);

    public static readonly DirectProperty<FeedbackStackPresenter, Thickness> SecondBackplateMarginProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, Thickness>(
            nameof(SecondBackplateMargin), owner => owner.SecondBackplateMargin);

    // These read-only styled aliases exist so AXAML can bind the two static message backplates.
    public static readonly DirectProperty<FeedbackStackPresenter, bool> IsFirstBackplateVisibleProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, bool>(
            nameof(IsFirstBackplateVisible), owner => owner.IsFirstBackplateVisible);

    public static readonly DirectProperty<FeedbackStackPresenter, bool> IsSecondBackplateVisibleProperty =
        AvaloniaProperty.RegisterDirect<FeedbackStackPresenter, bool>(
            nameof(IsSecondBackplateVisible), owner => owner.IsSecondBackplateVisible);

    private FeedbackStackPanel? _panel;
    private bool _isCollapsed;
    private int _activeItemCount;
    private double _firstBackplateWidth;
    private double _secondBackplateWidth;
    private Thickness _firstBackplateMargin;
    private Thickness _secondBackplateMargin;
    private bool _isFirstBackplateVisible;
    private bool _isSecondBackplateVisible;
    private bool _enteredStack;

    internal event EventHandler<FeedbackStackHoverChangedEventArgs>? StackHoverChanged;

    public FeedbackStackMode StackMode
    {
        get => GetValue(StackModeProperty);
        set => SetValue(StackModeProperty, value);
    }

    public NotificationPosition Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
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

    public double ExpandedGap
    {
        get => GetValue(ExpandedGapProperty);
        set => SetValue(ExpandedGapProperty, value);
    }

    public double CollapsedOffset
    {
        get => GetValue(CollapsedOffsetProperty);
        set => SetValue(CollapsedOffsetProperty, value);
    }

    public bool IsCollapsed => _isCollapsed;

    public int ActiveItemCount => _activeItemCount;

    public double FirstBackplateWidth => _firstBackplateWidth;

    public double SecondBackplateWidth => _secondBackplateWidth;

    public Thickness FirstBackplateMargin => _firstBackplateMargin;

    public Thickness SecondBackplateMargin => _secondBackplateMargin;

    public bool IsFirstBackplateVisible => _isFirstBackplateVisible;

    public bool IsSecondBackplateVisible => _isSecondBackplateVisible;

    static FeedbackStackPresenter()
    {
        ItemsPanelProperty.OverrideDefaultValue<FeedbackStackPresenter>(DefaultPanel);
    }

    internal void AttachPanel(FeedbackStackPanel panel)
    {
        _panel = panel;
        ConfigurePanel();
    }

    internal void DetachPanel(FeedbackStackPanel panel)
    {
        if (ReferenceEquals(_panel, panel))
        {
            _panel = null;
        }
    }

    internal void ReportLayoutState(bool isCollapsed, Size collapsedCardSize, int activeItemCount)
    {
        SetAndRaise(IsCollapsedProperty, ref _isCollapsed, isCollapsed);
        SetAndRaise(ActiveItemCountProperty, ref _activeItemCount, activeItemCount);
        SetAndRaise(
            IsFirstBackplateVisibleProperty,
            ref _isFirstBackplateVisible,
            isCollapsed && activeItemCount >= 2);
        SetAndRaise(
            IsSecondBackplateVisibleProperty,
            ref _isSecondBackplateVisible,
            isCollapsed && activeItemCount >= 3);

        var firstWidth = Math.Max(0, collapsedCardSize.Width - 16);
        var secondWidth = Math.Max(0, collapsedCardSize.Width - 32);
        SetAndRaise(FirstBackplateWidthProperty, ref _firstBackplateWidth, firstWidth);
        SetAndRaise(SecondBackplateWidthProperty, ref _secondBackplateWidth, secondWidth);

        var isBottom = Position is NotificationPosition.BottomLeft or
            NotificationPosition.BottomCenter or
            NotificationPosition.BottomRight;
        var firstMargin = new Thickness(0, isBottom ? 8 : Math.Max(0, collapsedCardSize.Height - 8), 0, 0);
        var secondMargin = new Thickness(0, isBottom ? 0 : collapsedCardSize.Height, 0, 0);
        SetAndRaise(FirstBackplateMarginProperty, ref _firstBackplateMargin, firstMargin);
        SetAndRaise(SecondBackplateMarginProperty, ref _secondBackplateMargin, secondMargin);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StackModeProperty ||
            change.Property == PositionProperty ||
            change.Property == IsStackEnabledProperty ||
            change.Property == StackThresholdProperty ||
            change.Property == ExpandedGapProperty ||
            change.Property == CollapsedOffsetProperty)
        {
            ConfigurePanel();
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        SetPointerOverState(true);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetPointerOverState(false);
    }

    internal void SetPointerOverState(bool isPointerOver)
    {
        if (isPointerOver)
        {
            _enteredStack = IsStackEnabled && ActiveItemCount > Math.Max(1, StackThreshold);
            if (_enteredStack && _panel is not null)
            {
                _panel.IsStackExpanded = true;
            }
            StackHoverChanged?.Invoke(this, new FeedbackStackHoverChangedEventArgs(true, _enteredStack));
            return;
        }

        if (_panel is not null)
        {
            _panel.IsStackExpanded = false;
        }
        StackHoverChanged?.Invoke(this, new FeedbackStackHoverChangedEventArgs(false, _enteredStack));
        _enteredStack = false;
    }

    private void ConfigurePanel()
    {
        if (_panel is null)
        {
            return;
        }

        _panel.StackMode = StackMode;
        _panel.Position = Position;
        _panel.IsStackEnabled = IsStackEnabled;
        _panel.StackThreshold = StackThreshold;
        _panel.ExpandedGap = ExpandedGap;
        _panel.CollapsedOffset = CollapsedOffset;
    }

}
