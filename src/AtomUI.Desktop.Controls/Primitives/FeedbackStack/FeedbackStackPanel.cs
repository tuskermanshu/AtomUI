using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal enum FeedbackStackMode
{
    Message,
    Notification
}

/// <summary>
/// Steady-state allocation-free layout primitive shared by transient feedback hosts.
/// Children are ordered from oldest to newest.
/// </summary>
internal sealed class FeedbackStackPanel : Panel
{
    private static readonly ITransform FullTransform = BuildTransform(1, 0);
    public static readonly StyledProperty<FeedbackStackMode> StackModeProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, FeedbackStackMode>(nameof(StackMode));

    public static readonly StyledProperty<NotificationPosition> PositionProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, NotificationPosition>(
            nameof(Position), NotificationPosition.TopCenter);

    public static readonly StyledProperty<bool> IsStackEnabledProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, bool>(nameof(IsStackEnabled));

    public static readonly StyledProperty<int> StackThresholdProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, int>(nameof(StackThreshold), 3);

    public static readonly StyledProperty<bool> IsStackExpandedProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, bool>(nameof(IsStackExpanded));

    public static readonly StyledProperty<double> ExpandedGapProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, double>(nameof(ExpandedGap), 16);

    public static readonly StyledProperty<double> CollapsedOffsetProperty =
        AvaloniaProperty.Register<FeedbackStackPanel, double>(nameof(CollapsedOffset), 8);

    private bool _isCollapsed;
    private Size _collapsedCardSize;
    private FeedbackStackPresenter? _owner;
    private readonly ConditionalWeakTable<Control, CachedTransform> _collapsedTransforms = new();

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

    public bool IsStackExpanded
    {
        get => GetValue(IsStackExpandedProperty);
        set => SetValue(IsStackExpandedProperty, value);
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

    public Size CollapsedCardSize => _collapsedCardSize;

    static FeedbackStackPanel()
    {
        AffectsMeasure<FeedbackStackPanel>(
            StackModeProperty,
            PositionProperty,
            IsStackEnabledProperty,
            StackThresholdProperty,
            IsStackExpandedProperty,
            ExpandedGapProperty,
            CollapsedOffsetProperty);
        AffectsArrange<FeedbackStackPanel>(
            StackModeProperty,
            PositionProperty,
            IsStackEnabledProperty,
            StackThresholdProperty,
            IsStackExpandedProperty,
            ExpandedGapProperty,
            CollapsedOffsetProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var activeCount = 0;
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            child.Measure(availableSize);
            if (IsActive(child))
            {
                activeCount++;
            }
        }

        _isCollapsed = IsStackEnabled && !IsStackExpanded && activeCount > Math.Max(1, StackThreshold);
        _collapsedCardSize = default;
        if (activeCount == 0)
        {
            return default;
        }

        var desiredSize = _isCollapsed
            ? MeasureCollapsed(activeCount)
            : MeasureExpanded(activeCount);
        _owner?.ReportLayoutState(_isCollapsed, _collapsedCardSize, activeCount);
        return desiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var activeCount = CountActiveChildren();
        _isCollapsed = IsStackEnabled && !IsStackExpanded && activeCount > Math.Max(1, StackThreshold);

        if (_isCollapsed)
        {
            ArrangeCollapsed(finalSize, activeCount);
        }
        else
        {
            ArrangeExpanded(finalSize, activeCount);
        }

        return finalSize;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _owner = this.FindAncestorOfType<FeedbackStackPresenter>();
        _owner?.AttachPanel(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _owner?.DetachPanel(this);
        _owner = null;
        base.OnDetachedFromVisualTree(e);
    }

    private Size MeasureExpanded(int activeCount)
    {
        var width = 0d;
        var height = 0d;
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (!IsActive(child))
            {
                continue;
            }

            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;
        }

        height += Math.Max(0, ExpandedGap) * Math.Max(0, activeCount - 1);
        return new Size(width, height);
    }

    private Size MeasureCollapsed(int activeCount)
    {
        var visibleCount = StackMode == FeedbackStackMode.Message ? 1 : Math.Min(3, activeCount);
        var remaining = visibleCount;
        var width = 0d;
        var height = 0d;

        for (var i = Children.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var child = Children[i];
            if (!IsActive(child))
            {
                continue;
            }

            if (remaining == visibleCount)
            {
                _collapsedCardSize = child.DesiredSize;
            }

            var depth = visibleCount - remaining;
            width = Math.Max(width, child.DesiredSize.Width);
            height = Math.Max(height, child.DesiredSize.Height + depth * Math.Max(0, CollapsedOffset));
            remaining--;
        }

        if (StackMode == FeedbackStackMode.Message)
        {
            height += 2 * Math.Max(0, CollapsedOffset);
        }

        return new Size(width, height);
    }

    private void ArrangeExpanded(Size finalSize, int activeCount)
    {
        var isBottom = IsBottomPosition(Position);
        var gap = Math.Max(0, ExpandedGap);
        var y = 0d;
        var arranged = 0;

        for (var step = 0; step < Children.Count; step++)
        {
            var index = isBottom ? step : Children.Count - 1 - step;
            var child = Children[index];
            if (!IsActive(child))
            {
                Hide(child);
                continue;
            }

            Show(child, 1, isBottom);
            var size = child.DesiredSize;
            child.Arrange(new Rect(ResolveX(finalSize.Width, size.Width), y, size.Width, size.Height));
            child.ZIndex = activeCount - arranged;
            y += size.Height + gap;
            arranged++;
        }
    }

    private void ArrangeCollapsed(Size finalSize, int activeCount)
    {
        var isBottom = IsBottomPosition(Position);
        var visibleCount = StackMode == FeedbackStackMode.Message ? 1 : Math.Min(3, activeCount);
        var gap = Math.Max(0, ExpandedGap);
        var offset = Math.Max(0, CollapsedOffset);
        var depth = 0;
        var expandedEdge = isBottom ? finalSize.Height : 0d;

        for (var i = Children.Count - 1; i >= 0; i--)
        {
            var child = Children[i];
            if (!IsActive(child))
            {
                Hide(child);
                continue;
            }

            var size = child.DesiredSize;
            var expandedY = isBottom ? expandedEdge - size.Height : expandedEdge;
            expandedEdge = isBottom ? expandedY - gap : expandedY + size.Height + gap;

            var layerDepth = StackMode == FeedbackStackMode.Message ? 0 : Math.Min(depth, 2);
            var collapsedY = isBottom
                ? finalSize.Height - size.Height - layerDepth * offset
                : layerDepth * offset;
            var scale = StackMode == FeedbackStackMode.Notification ? 1 - layerDepth * 0.06 : 1;
            var isVisibleLayer = depth < visibleCount;
            SetVisibility(child, isVisibleLayer);
            SetTransform(
                child,
                isVisibleLayer ? scale : 1,
                isVisibleLayer ? collapsedY - expandedY : 0,
                isBottom);
            child.Arrange(new Rect(ResolveX(finalSize.Width, size.Width), expandedY, size.Width, size.Height));
            child.ZIndex = Math.Max(0, visibleCount - depth);
            depth++;
        }
    }

    private int CountActiveChildren()
    {
        var count = 0;
        for (var i = 0; i < Children.Count; i++)
        {
            if (IsActive(Children[i]))
            {
                count++;
            }
        }
        return count;
    }

    private static bool IsActive(Control child)
    {
        return child is not IFeedbackStackItem item || !item.IsClosed;
    }

    private void Hide(Control child)
    {
        SetVisibility(child, false);
        SetTransform(child, 1, 0, false);
    }

    private void Show(Control child, double scale, bool isBottom)
    {
        SetVisibility(child, true);
        SetTransform(child, scale, 0, isBottom);
    }

    private static void SetVisibility(Control child, bool isVisible)
    {
        if (child is IFeedbackStackItem item)
        {
            item.IsStackVisible = isVisible;
        }
        child.Opacity = isVisible ? 1 : 0;
        child.IsHitTestVisible = isVisible;
    }

    private void SetTransform(Control child, double scale, double translateY, bool isBottom)
    {
        ITransform transform;
        if (Math.Abs(scale - 1) < 0.0001 && Math.Abs(translateY) < 0.0001)
        {
            transform = FullTransform;
        }
        else if (_collapsedTransforms.TryGetValue(child, out var cached) && cached.Matches(scale, translateY))
        {
            transform = cached.Transform;
        }
        else
        {
            transform = BuildTransform(scale, translateY);
            if (cached is null)
            {
                _collapsedTransforms.Add(child, new CachedTransform(scale, translateY, transform));
            }
            else
            {
                cached.Update(scale, translateY, transform);
            }
        }

        if (!ReferenceEquals(child.RenderTransform, transform))
        {
            child.RenderTransform = transform;
        }
        child.RenderTransformOrigin = new RelativePoint(0.5, isBottom ? 1 : 0, RelativeUnit.Relative);
    }

    private static ITransform BuildTransform(double scale, double translateY)
    {
        var builder = new TransformOperations.Builder(2);
        builder.AppendScale(scale, scale);
        builder.AppendTranslate(0, translateY);
        return builder.Build();
    }

    private double ResolveX(double finalWidth, double childWidth)
    {
        return Position switch
        {
            NotificationPosition.TopRight or NotificationPosition.BottomRight => finalWidth - childWidth,
            NotificationPosition.TopCenter or NotificationPosition.BottomCenter => (finalWidth - childWidth) / 2,
            _ => 0
        };
    }

    private static bool IsBottomPosition(NotificationPosition position)
    {
        return position is NotificationPosition.BottomLeft or
            NotificationPosition.BottomCenter or
            NotificationPosition.BottomRight;
    }

    private sealed class CachedTransform(double scale, double translateY, ITransform transform)
    {
        internal double Scale { get; private set; } = scale;
        internal double TranslateY { get; private set; } = translateY;
        internal ITransform Transform { get; private set; } = transform;

        internal bool Matches(double scale, double translateY)
        {
            return Math.Abs(Scale - scale) < 0.0001 && Math.Abs(TranslateY - translateY) < 0.0001;
        }

        internal void Update(double scale, double translateY, ITransform transform)
        {
            Scale = scale;
            TranslateY = translateY;
            Transform = transform;
        }
    }
}
