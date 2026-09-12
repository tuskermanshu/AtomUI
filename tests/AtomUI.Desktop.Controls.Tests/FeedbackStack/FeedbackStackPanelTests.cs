using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackStackPanelTests
{
    [Fact]
    public void Message_Collapses_Only_When_Count_Is_Greater_Than_Threshold()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        var first = AddItem(panel, 100, 20);
        var second = AddItem(panel, 100, 20);
        var third = AddItem(panel, 100, 20);

        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeFalse();
        panel.DesiredSize.Height.ShouldBe(92, 0.01);
        first.IsStackVisible.ShouldBeTrue();
        second.IsStackVisible.ShouldBeTrue();
        third.IsStackVisible.ShouldBeTrue();

        var latest = AddItem(panel, 120, 20);
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeTrue();
        latest.IsStackVisible.ShouldBeTrue();
        third.IsStackVisible.ShouldBeFalse();
        second.IsStackVisible.ShouldBeFalse();
        first.IsStackVisible.ShouldBeFalse();
        panel.CollapsedCardSize.ShouldBe(new Size(120, 20));
        panel.DesiredSize.Height.ShouldBe(36, 0.01);
    }

    [Fact]
    public void Hover_Expanded_Message_Puts_Newest_Item_At_The_Top_And_Preserves_Gaps()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        var oldest = AddItem(panel, 80, 20);
        var middle = AddItem(panel, 90, 20);
        var newer = AddItem(panel, 100, 20);
        var latest = AddItem(panel, 110, 20);
        panel.IsStackExpanded = true;

        Layout(panel, 200);

        latest.Bounds.Y.ShouldBe(0, 0.01);
        newer.Bounds.Y.ShouldBe(36, 0.01);
        middle.Bounds.Y.ShouldBe(72, 0.01);
        oldest.Bounds.Y.ShouldBe(108, 0.01);
        panel.DesiredSize.Height.ShouldBe(128, 0.01);
    }

    [Fact]
    public void Notification_Collapsed_Stack_Uses_Three_Real_Cards_With_Ant_Scale_And_Offset()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        var hidden = AddItem(panel, 100, 40);
        var third = AddItem(panel, 100, 40);
        var second = AddItem(panel, 100, 40);
        var latest = AddItem(panel, 100, 40);

        Layout(panel, 200);

        hidden.IsStackVisible.ShouldBeFalse();
        third.IsStackVisible.ShouldBeTrue();
        second.IsStackVisible.ShouldBeTrue();
        latest.IsStackVisible.ShouldBeTrue();
        latest.Bounds.ShouldBe(new Rect(100, 0, 100, 40));
        second.Bounds.ShouldBe(new Rect(100, 56, 100, 40));
        third.Bounds.ShouldBe(new Rect(100, 112, 100, 40));
        GetScale(latest).ShouldBe(1, 0.001);
        GetScale(second).ShouldBe(0.94, 0.001);
        GetScale(third).ShouldBe(0.88, 0.001);
        GetTranslateY(second).ShouldBe(-48, 0.001);
        GetTranslateY(third).ShouldBe(-96, 0.001);
        panel.DesiredSize.Height.ShouldBe(56, 0.01);
    }

    [Fact]
    public void Bottom_Position_Mirrors_Collapsed_Offsets_And_Keeps_Latest_Nearest_The_Edge()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.BottomLeft);
        var third = AddItem(panel, 100, 40);
        var second = AddItem(panel, 100, 40);
        var latest = AddItem(panel, 100, 40);
        AddItem(panel, 100, 40);

        Layout(panel, 200);

        var actualLatest = (TestFeedbackControl)panel.Children[^1];
        actualLatest.Bounds.Y.ShouldBe(16, 0.01);
        latest.Bounds.Y.ShouldBe(-40, 0.01);
        second.Bounds.Y.ShouldBe(-96, 0.01);
        GetTranslateY(latest).ShouldBe(48, 0.001);
        GetTranslateY(second).ShouldBe(96, 0.001);
        third.IsStackVisible.ShouldBeFalse();
    }

    [Fact]
    public void Runtime_Stack_Disable_Expands_All_Items_And_Resets_Transforms()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        var items = new TestFeedbackControl[4];
        for (var i = 0; i < items.Length; i++)
        {
            items[i] = AddItem(panel, 100, 20);
        }
        Layout(panel, 200);
        GetScale(items[1]).ShouldBe(0.88, 0.001);

        panel.IsStackEnabled = false;
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeFalse();
        panel.DesiredSize.Height.ShouldBe(128, 0.01);
        for (var i = 0; i < items.Length; i++)
        {
            items[i].IsStackVisible.ShouldBeTrue();
            items[i].IsHitTestVisible.ShouldBeTrue();
            GetScale(items[i]).ShouldBe(1, 0.001);
        }
    }

    [Fact]
    public void Repeated_Layout_Reuses_Cached_Layer_Transforms()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }
        Layout(panel, 200);
        var secondLayer = panel.Children[^2].RenderTransform;
        var thirdLayer = panel.Children[^3].RenderTransform;

        for (var i = 0; i < 10; i++)
        {
            panel.InvalidateArrange();
            Layout(panel, 200);
            panel.Children[^2].RenderTransform.ShouldBeSameAs(secondLayer);
            panel.Children[^3].RenderTransform.ShouldBeSameAs(thirdLayer);
        }
    }

    [Fact]
    public void Cached_Transform_Does_Not_Retain_A_Removed_Card()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }
        Layout(panel, 200);

        var removedCard = RemoveCachedCard(panel);
        ForceFullCollection();

        removedCard.IsAlive.ShouldBeFalse();
        GC.KeepAlive(panel);
    }

    [Fact]
    public void Presenter_Hover_Expands_And_Collapses_The_Whole_Stack()
    {
        var presenter = new FeedbackStackPresenter
        {
            StackMode = FeedbackStackMode.Message,
            IsStackEnabled = true,
            StackThreshold = 3
        };
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }
        Layout(panel, 200);
        presenter.AttachPanel(panel);
        presenter.ReportLayoutState(panel.IsCollapsed, panel.CollapsedCardSize, 4);
        var hoverEvents = 0;
        presenter.StackHoverChanged += (_, args) =>
        {
            args.IsStackInteraction.ShouldBeTrue();
            hoverEvents++;
        };

        presenter.SetPointerOverState(true);
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeFalse();
        panel.Children.Cast<TestFeedbackControl>().ShouldAllBe(item => item.IsStackVisible);

        presenter.SetPointerOverState(false);
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeTrue();
        hoverEvents.ShouldBe(2);
    }

    private static FeedbackStackPanel CreatePanel(FeedbackStackMode mode, NotificationPosition position)
    {
        return new FeedbackStackPanel
        {
            StackMode = mode,
            Position = position,
            IsStackEnabled = true,
            StackThreshold = 3,
            ExpandedGap = 16,
            CollapsedOffset = 8
        };
    }

    private static TestFeedbackControl AddItem(Panel panel, double width, double height)
    {
        var item = new TestFeedbackControl(new Size(width, height));
        panel.Children.Add(item);
        return item;
    }

    private static void Layout(Control control, double width)
    {
        control.Measure(new Size(width, double.PositiveInfinity));
        control.Arrange(new Rect(0, 0, width, control.DesiredSize.Height));
    }

    private static double GetScale(Control control)
    {
        control.RenderTransform.ShouldNotBeNull();
        return control.RenderTransform.Value.M11;
    }

    private static double GetTranslateY(Control control)
    {
        control.RenderTransform.ShouldNotBeNull();
        return control.RenderTransform.Value.M32;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RemoveCachedCard(FeedbackStackPanel panel)
    {
        var card = panel.Children[^2];
        var reference = new WeakReference(card);
        panel.Children.Remove(card);
        return reference;
    }

    private static void ForceFullCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed class TestFeedbackControl : Control, IFeedbackStackItem
    {
        private readonly Size _size;

        public TestFeedbackControl(Size size)
        {
            _size = size;
        }

        public bool IsClosing { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsProgressVisible => false;
        public bool IsStackVisible { get; set; } = true;

        public void RequestClose()
        {
            IsClosing = true;
        }

        public void UpdateRemaining(TimeSpan remaining)
        {
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return _size;
        }
    }
}
