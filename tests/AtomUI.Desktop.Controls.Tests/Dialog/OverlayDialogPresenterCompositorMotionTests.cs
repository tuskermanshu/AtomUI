using System.Reactive.Linq;
using AtomUI.Controls.Primitives;
using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Dialog;

[Collection(DialogLifecycleTestCollection.Name)]
public class OverlayDialogPresenterCompositorMotionTests
{
    static OverlayDialogPresenterCompositorMotionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Opening_Motion_Does_Not_Write_UI_Thread_Transform_Frames()
    {
        RunOnUIThread(() =>
        {
            var (window, presenter) = CreatePresenter();
            Task? showTask = null;
            try
            {
                presenter.MotionDuration = TimeSpan.FromMilliseconds(500);
                showTask = presenter.ShowAsync(CancellationToken.None).AsTask();
                PumpUntil(
                    () => presenter.GetVisualDescendants().OfType<MotionActor>().Any(),
                    TimeSpan.FromSeconds(2));

                var surfaceActor = FindMotionActor(presenter, "PART_SurfaceMotionActor");
                var wroteTransformFrame = surfaceActor.MotionTransform is not null;
                using var subscription = surfaceActor
                    .GetObservable(BaseMotionActor.MotionTransformProperty)
                    .Subscribe(transform => wroteTransformFrame |= transform is not null);

                PumpFor(TimeSpan.FromMilliseconds(150));

                wroteTransformFrame.ShouldBeFalse(
                    "overlay opening motion should stay on the compositor instead of updating MotionTransform on the UI thread");
                showTask.IsCompleted.ShouldBeFalse();

                WaitWithDispatcherPump(showTask);
                surfaceActor.Opacity.ShouldBe(1.0);
                surfaceActor.MotionTransform.ShouldBeNull();
                FindMotionActor(presenter, "PART_MaskMotionActor").Opacity.ShouldBe(1.0);
            }
            finally
            {
                if (showTask is { IsCompleted: false })
                {
                    WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                }

                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void Closing_Motion_Does_Not_Write_UI_Thread_Transform_Frames()
    {
        RunOnUIThread(() =>
        {
            var (window, presenter) = CreatePresenter();
            Task? closeTask = null;
            try
            {
                presenter.MotionDuration = TimeSpan.FromMilliseconds(80);
                var showTask = presenter.ShowAsync(CancellationToken.None).AsTask();
                PumpUntil(
                    () => presenter.GetVisualDescendants().OfType<MotionActor>().Any(),
                    TimeSpan.FromSeconds(2));
                PumpFor(TimeSpan.FromMilliseconds(150));
                WaitWithDispatcherPump(showTask);

                var surfaceActor = FindMotionActor(presenter, "PART_SurfaceMotionActor");
                var wroteTransformFrame = surfaceActor.MotionTransform is not null;
                using var subscription = surfaceActor
                    .GetObservable(BaseMotionActor.MotionTransformProperty)
                    .Subscribe(transform => wroteTransformFrame |= transform is not null);

                presenter.MotionDuration = TimeSpan.FromMilliseconds(500);
                closeTask = presenter.CloseAsync().AsTask();
                PumpFor(TimeSpan.FromMilliseconds(150));

                wroteTransformFrame.ShouldBeFalse(
                    "overlay closing motion should stay on the compositor instead of updating MotionTransform on the UI thread");
                closeTask.IsCompleted.ShouldBeFalse();

                WaitWithDispatcherPump(closeTask);
                presenter.Parent.ShouldBeNull();
            }
            finally
            {
                if (closeTask is { IsCompleted: false })
                {
                    WaitWithDispatcherPump(closeTask);
                }

                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void Unanchored_Opening_Motion_Does_Not_Write_UI_Thread_Opacity_Frames()
    {
        RunOnUIThread(() =>
        {
            var (window, presenter) = CreatePresenter(anchored: false);
            Task? showTask = null;
            try
            {
                presenter.MotionDuration = TimeSpan.FromMilliseconds(500);
                showTask = presenter.ShowAsync(CancellationToken.None).AsTask();
                PumpUntil(
                    () => presenter.GetVisualDescendants().OfType<MotionActor>().Any(),
                    TimeSpan.FromSeconds(2));

                var surfaceActor = FindMotionActor(presenter, "PART_SurfaceMotionActor");
                var wroteIntermediateOpacity = IsIntermediateOpacity(surfaceActor.Opacity);
                using var subscription = surfaceActor
                    .GetObservable(Visual.OpacityProperty)
                    .Subscribe(opacity => wroteIntermediateOpacity |= IsIntermediateOpacity(opacity));

                PumpFor(TimeSpan.FromMilliseconds(150));

                wroteIntermediateOpacity.ShouldBeFalse(
                    "unanchored opening fade should stay on the compositor instead of updating Opacity on the UI thread");
                showTask.IsCompleted.ShouldBeFalse();

                WaitWithDispatcherPump(showTask);
                surfaceActor.Opacity.ShouldBe(1.0);
            }
            finally
            {
                if (showTask is { IsCompleted: false })
                {
                    presenter.IsMotionEnabled = false;
                    WaitWithDispatcherPump(presenter.CloseAsync().AsTask());
                }

                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void Unanchored_Closing_Motion_Does_Not_Write_UI_Thread_Opacity_Frames()
    {
        RunOnUIThread(() =>
        {
            var (window, presenter) = CreatePresenter(anchored: false);
            Task? closeTask = null;
            try
            {
                presenter.MotionDuration = TimeSpan.FromMilliseconds(80);
                var showTask = presenter.ShowAsync(CancellationToken.None).AsTask();
                PumpUntil(
                    () => presenter.GetVisualDescendants().OfType<MotionActor>().Any(),
                    TimeSpan.FromSeconds(2));
                PumpFor(TimeSpan.FromMilliseconds(150));
                WaitWithDispatcherPump(showTask);

                var surfaceActor = FindMotionActor(presenter, "PART_SurfaceMotionActor");
                var wroteIntermediateOpacity = IsIntermediateOpacity(surfaceActor.Opacity);
                using var subscription = surfaceActor
                    .GetObservable(Visual.OpacityProperty)
                    .Subscribe(opacity => wroteIntermediateOpacity |= IsIntermediateOpacity(opacity));

                presenter.MotionDuration = TimeSpan.FromMilliseconds(500);
                closeTask = presenter.CloseAsync().AsTask();
                PumpFor(TimeSpan.FromMilliseconds(150));

                wroteIntermediateOpacity.ShouldBeFalse(
                    "unanchored closing fade should stay on the compositor instead of updating Opacity on the UI thread");
                closeTask.IsCompleted.ShouldBeFalse();

                WaitWithDispatcherPump(closeTask);
                presenter.Parent.ShouldBeNull();
            }
            finally
            {
                if (closeTask is { IsCompleted: false })
                {
                    WaitWithDispatcherPump(closeTask);
                }

                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    [Fact]
    public void Close_During_Opening_Motion_Restores_Actor_State_Before_Teardown()
    {
        RunOnUIThread(() =>
        {
            var (window, presenter) = CreatePresenter();
            try
            {
                presenter.MotionDuration = TimeSpan.FromSeconds(5);
                var showTask = presenter.ShowAsync(CancellationToken.None).AsTask();
                PumpUntil(
                    () => presenter.GetVisualDescendants().OfType<MotionActor>().Any(),
                    TimeSpan.FromSeconds(2));
                PumpFor(TimeSpan.FromMilliseconds(150));

                var surfaceActor = FindMotionActor(presenter, "PART_SurfaceMotionActor");
                showTask.IsCompleted.ShouldBeFalse();

                presenter.MotionDuration = TimeSpan.FromMilliseconds(80);
                WaitWithDispatcherPump(presenter.CloseAsync().AsTask());

                presenter.Parent.ShouldBeNull();
                surfaceActor.Opacity.ShouldBe(1.0);
                surfaceActor.MotionTransform.ShouldBeNull();
            }
            finally
            {
                WaitWithDispatcherPump(presenter.DisposeAsync().AsTask());
                window.Close();
            }
        });
    }

    private static (AtomUI.Desktop.Controls.Window window, OverlayDialogPresenter presenter) CreatePresenter(
        bool anchored = true)
    {
        var placementTarget = new Border { Width = 80, Height = 32 };
        var root = new ScopeAwareOverlayLayerPanel
        {
            Width    = 480,
            Height   = 360,
            Children = { placementTarget }
        };
        var window = new AtomUI.Desktop.Controls.Window
        {
            Width   = 480,
            Height  = 360,
            Content = root
        };
        var dialog = new AtomUI.Desktop.Controls.Dialog
        {
            Content         = "Dialog",
            StandardButtons = DialogStandardButton.Ok,
            IsModal         = true,
            IsMotionEnabled = true,
            HostWidth       = 240,
            HostHeight      = 160,
            PlacementTarget = placementTarget,
            MotionAnchorMode = anchored
                ? DialogMotionAnchorMode.ExplicitPlacementTarget
                : DialogMotionAnchorMode.FallbackPlacementTarget
        };
        var presenter = new OverlayDialogPresenter(dialog, placementTarget)
        {
            IsMotionEnabled = true
        };
        window.Show();
        return (window, presenter);
    }

    private static MotionActor FindMotionActor(OverlayDialogPresenter presenter, string name)
    {
        return presenter.GetVisualDescendants()
                        .OfType<MotionActor>()
                        .Single(actor => actor.Name == name);
    }

    private static bool IsIntermediateOpacity(double opacity)
    {
        return opacity > 0 && opacity < 1;
    }

    private static void PumpUntil(Func<bool> condition, TimeSpan timeout)
    {
        var timeoutAt = DateTimeOffset.UtcNow + timeout;
        while (!condition() && DateTimeOffset.UtcNow < timeoutAt)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        condition().ShouldBeTrue();
    }

    private static void PumpFor(TimeSpan duration)
    {
        using var cancellation = new CancellationTokenSource(duration);
        Dispatcher.UIThread.MainLoop(cancellation.Token);
    }

    private static void WaitWithDispatcherPump(Task task)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _ = task.ContinueWith(
            _ => cancellation.Cancel(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        Dispatcher.UIThread.MainLoop(cancellation.Token);

        task.IsCompleted.ShouldBeTrue();
        task.GetAwaiter().GetResult();
    }

    private static void RunOnUIThread(Action action)
    {
        Dispatcher.UIThread.Invoke(action);
    }
}
