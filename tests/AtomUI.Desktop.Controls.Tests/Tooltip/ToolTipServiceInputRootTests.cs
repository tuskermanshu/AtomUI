using System.Reflection;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Tooltip;

public class ToolTipServiceInputRootTests
{
    static ToolTipServiceInputRootTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void GetRootVisual_Returns_Window_Presentation_Root()
    {
        var content = new Border();
        var window = new AvaloniaWindow { Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            var source = content.GetPresentationSource().ShouldNotBeNull();
            var inputRoot = source.ShouldBeAssignableTo<IInputRoot>();

            ToolTipService.GetRootVisual(inputRoot).ShouldBeSameAs(source.RootVisual);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void GetRootVisual_Returns_Popup_Presentation_Root()
    {
        var anchor = new Border();
        var popupContent = new Border();
        var popup = new Popup
        {
            PlacementTarget = anchor,
            Child = popupContent,
            ShouldUseOverlayLayer = true
        };
        var canvas = new Canvas { Children = { anchor, popup } };
        var layers = new VisualLayerManager { Child = canvas };
        typeof(VisualLayerManager).GetProperty(
                "EnablePopupOverlayLayer",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(layers, true);
        var window = new AvaloniaWindow { Content = layers };
        window.Show();
        popup.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        try
        {
            var source = popupContent.GetPresentationSource().ShouldNotBeNull();
            var anchorSource = anchor.GetPresentationSource().ShouldNotBeNull();
            var inputRoot = source.ShouldBeAssignableTo<IInputRoot>();

            ToolTipService.GetRootVisual(inputRoot).ShouldBeSameAs(source.RootVisual);
            source.ShouldBeSameAs(anchorSource,
                "an overlay popup must remain in the owning window's presentation source");
            source.RootVisual.ShouldNotBeNull().GetType().Name.ShouldBe("TopLevelHost");
        }
        finally
        {
            popup.IsOpen = false;
            window.Close();
        }
    }

    [Fact]
    public void GetRootVisual_Returns_Native_PopupRoot()
    {
        var anchor = new Border();
        var popupContent = new Border();
        var popup = new Popup
        {
            PlacementTarget = anchor,
            Child = popupContent
        };
        var canvas = new Canvas { Children = { anchor, popup } };
        var window = new AvaloniaWindow { Content = canvas };
        window.Show();
        SetHeadlessOverlayPopups(window, false);
        popup.IsOpen = true;
        Dispatcher.UIThread.RunJobs();

        try
        {
            var source = popupContent.GetPresentationSource().ShouldNotBeNull();
            var anchorSource = anchor.GetPresentationSource().ShouldNotBeNull();
            var inputRoot = source.ShouldBeAssignableTo<IInputRoot>();

            source.ShouldNotBeSameAs(anchorSource,
                "a native popup must own a distinct presentation source");
            source.RootVisual.ShouldNotBeNull().GetType().Name.ShouldBe("TopLevelHost");
            ToolTipService.GetRootVisual(inputRoot).ShouldBeSameAs(source.RootVisual);
        }
        finally
        {
            popup.IsOpen = false;
            SetHeadlessOverlayPopups(window, true);
            window.Close();
        }
    }

    [Fact]
    public void Native_ToolTip_LeaveWindow_Preserves_Same_Timestamp_Root_Transition()
    {
        var host = new Border();
        var window = new AvaloniaWindow { Content = host };
        window.Show();
        SetHeadlessOverlayPopups(window, false);
        Dispatcher.UIThread.RunJobs();

        using var inputManager = new TestInputManager();
        using var mouseDevice = new MouseDevice();
        using var service = new ToolTipService(inputManager);
        try
        {
            ToolTip.SetTip(host, "tip");
            ToolTip.SetShowDelay(host, 0);
            service.Update(GetInputRoot(host), host);
            Dispatcher.UIThread.RunJobs();
            ToolTip.GetIsOpen(host).ShouldBeTrue();

            var tip = host.GetValue(ToolTip.ToolTipProperty).ShouldNotBeNull();
            var windowRoot = GetInputRoot(host);
            var tipRoot = GetInputRoot(tip);
            tipRoot.ShouldNotBeSameAs(windowRoot);

            const ulong timestamp = 42;
            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, windowRoot, RawPointerEventType.Move, timestamp, host));
            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, tipRoot, RawPointerEventType.Move, timestamp, tip));
            ToolTip.GetIsOpen(host).ShouldBeTrue(
                "moving from the window root into the native tooltip root must keep the tooltip open");

            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, windowRoot, RawPointerEventType.LeaveWindow, timestamp, null));
            ToolTip.GetIsOpen(host).ShouldBeTrue(
                "paired root events with the same platform timestamp describe one transition");

            inputManager.ProcessInput(CreatePointerEvent(mouseDevice, windowRoot, RawPointerEventType.LeaveWindow, timestamp + 1, null));
            ToolTip.GetIsOpen(host).ShouldBeFalse();
        }
        finally
        {
            ToolTip.SetIsOpen(host, false);
            SetHeadlessOverlayPopups(window, true);
            window.Close();
        }
    }

    private static IInputRoot GetInputRoot(Visual visual) =>
        visual.GetPresentationSource().ShouldNotBeNull().ShouldBeAssignableTo<IInputRoot>();

    private static RawPointerEventArgs CreatePointerEvent(
        IInputDevice device,
        IInputRoot root,
        RawPointerEventType type,
        ulong timestamp,
        IInputElement? hit)
    {
        var args = new RawPointerEventArgs(device, timestamp, root, type, default(Point), default);
        typeof(RawPointerEventArgs).GetProperty(
                "InputHitTestResult",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(args, (hit, hit));
        return args;
    }

    private static void SetHeadlessOverlayPopups(AvaloniaWindow window, bool value)
    {
        var implementation = window.PlatformImpl.ShouldNotBeNull();
        var options = implementation.GetType().GetField(
                "_options",
                BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(implementation)
            .ShouldNotBeNull();
        options.GetType().GetProperty("OverlayPopups")!.SetValue(options, value);
    }

    private sealed class TestInputManager : IInputManager, IDisposable
    {
        private readonly Subject<RawInputEventArgs> _preProcess = new();
        private readonly Subject<RawInputEventArgs> _process = new();
        private readonly Subject<RawInputEventArgs> _postProcess = new();

        public IObservable<RawInputEventArgs> PreProcess => _preProcess;
        public IObservable<RawInputEventArgs> Process => _process;
        public IObservable<RawInputEventArgs> PostProcess => _postProcess;

        public void ProcessInput(RawInputEventArgs e)
        {
            _preProcess.OnNext(e);
            _process.OnNext(e);
            _postProcess.OnNext(e);
        }

        public void Dispose()
        {
            _preProcess.Dispose();
            _process.Dispose();
            _postProcess.Dispose();
        }
    }

}
