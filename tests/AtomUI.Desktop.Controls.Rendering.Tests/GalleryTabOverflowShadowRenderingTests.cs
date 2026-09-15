using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.TabControl;
using AtomUIGallery.ShowCases.TabStrip;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public class GalleryTabOverflowShadowRenderingTests
{
    [Theory]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public void Production_Search_Example_Keeps_Visible_Shadows_Inside_The_Scrolled_Page(bool strip, double scaling)
    {
        Control page = strip
            ? new TabStripShowCase { DataContext = new TabStripViewModel(new TestScreen()) }
            : new TabControlShowCase { DataContext = new TabControlViewModel(new TestScreen()) };
        var window = new AvaloniaWindow
        {
            // Main content width of the user's 1300-DIP window, after its 280-DIP sidebar.
            Width = 1020,
            Height = 900,
            Background = Brushes.White,
            Content = page
        };
        window.Show();
        try
        {
            window.SetRenderScaling(scaling);
            var examples = page.FindControl<ShowCasePanel>("ExamplesContent").ShouldNotBeNull();
            for (var i = 0; i <= 8; i++)
                ((ShowCaseItem)examples.Children[i]).MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            var example = (ShowCaseItem)examples.Children[8];
            example.BringIntoView();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var viewers = example.GetVisualDescendants().OfType<Avalonia.Controls.ScrollViewer>()
                .Where(control => control.GetType().Name == "TabScrollViewer").ToArray();
            viewers.Length.ShouldBe(2);
            foreach (var viewer in viewers)
            {
                viewer.Extent.Width.ShouldBeGreaterThan(viewer.Viewport.Width);
                var viewport = Find<Panel>(viewer, "ScrollContentViewport");
                var start = Find<Control>(viewer, "PART_ScrollStartEdgeIndicator");
                var end = Find<Control>(viewer, "PART_ScrollEndEdgeIndicator");
                start.IsVisible.ShouldBeFalse();
                end.IsVisible.ShouldBeTrue();
                var origin = viewport.TranslatePoint(default, window).ShouldNotBeNull();
                origin.Y.ShouldBeGreaterThanOrEqualTo(0);
                (origin.Y + viewport.Bounds.Height).ShouldBeLessThanOrEqualTo(window.ClientSize.Height);
                TabOverflowShadowRenderingTests.AssertPixels(window, viewport, start, end, horizontal: true);
            }
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static T Find<T>(Control root, string name) where T : Control =>
        root.GetVisualDescendants().OfType<T>().Single(control => control.Name == name);

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }
}
