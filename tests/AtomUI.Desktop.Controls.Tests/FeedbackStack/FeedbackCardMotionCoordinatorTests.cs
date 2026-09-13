using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using AtomUI.MotionScene;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackCardMotionCoordinatorTests
{
    static FeedbackCardMotionCoordinatorTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Dispose_Releases_The_Current_Actor_Transition_State_Immediately()
    {
        var actor = new MotionActor
        {
            Content = new Border
            {
                Width = 320,
                Height = 96
            }
        };
        var window = new AvaloniaWindow
        {
            Width = 600,
            Height = 400,
            Content = actor
        };
        var coordinator = new FeedbackCardMotionCoordinator(() => { });

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            coordinator.ApplyActor(
                actor,
                isClosing: false,
                isClosed: false,
                isMotionEnabled: true,
                NotificationPosition.TopCenter,
                TimeSpan.FromMinutes(1));
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            actor.Transitions.ShouldNotBeNull();

            coordinator.Dispose();

            actor.Transitions.ShouldBeNull();
            actor.MotionTransform.ShouldBeNull();
        }
        finally
        {
            coordinator.Dispose();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Attached_Entry_Keeps_The_Ant_Prepare_State_Until_The_Next_Animation_Frame()
    {
        var actor = new MotionActor
        {
            Content = new Border
            {
                Width = 320,
                Height = 96
            }
        };
        var window = new AvaloniaWindow
        {
            Width = 600,
            Height = 400,
            Content = actor
        };
        using var coordinator = new FeedbackCardMotionCoordinator(() => { });

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            coordinator.ApplyActor(
                actor,
                isClosing: false,
                isClosed: false,
                isMotionEnabled: true,
                NotificationPosition.TopRight,
                TimeSpan.FromSeconds(1));
            Dispatcher.UIThread.RunJobs();

            actor.Opacity.ShouldBe(0, 0.001);
            actor.MotionTransform.ShouldNotBeNull();
            actor.MotionTransform!.Value.M31.ShouldBe(FeedbackCardMotion.Offset, 0.001);
            actor.Transitions.ShouldBeNull();

            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            actor.Transitions.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [InlineData(NotificationPosition.TopCenter, 0, -64)]
    [InlineData(NotificationPosition.BottomCenter, 0, 64)]
    [InlineData(NotificationPosition.TopLeft, -64, 0)]
    [InlineData(NotificationPosition.BottomLeft, -64, 0)]
    [InlineData(NotificationPosition.TopRight, 64, 0)]
    [InlineData(NotificationPosition.BottomRight, 64, 0)]
    public void Closing_During_Entry_Preserves_The_Exit_Target_Until_Completion(
        NotificationPosition position, double expectedX, double expectedY)
    {
        var clockType = typeof(Animation).Assembly.GetType("Avalonia.Animation.ClockBase", true)!;
        var pulse = clockType.GetMethod("Pulse", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var clock = Activator.CreateInstance(clockType, nonPublic: true)!;
        var actor = new MotionActor { Content = new Border { Width = 320, Height = 96 } };
        var window = new AvaloniaWindow { Width = 600, Height = 400, Content = actor };
        typeof(Animatable).GetProperty("Clock", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(window, clock);
        var starts = 0;
        var closeCount = 0;
        actor.PreStart += (_, _) => starts++;
        var coordinator = new FeedbackCardMotionCoordinator(() => closeCount++);
        var duration = TimeSpan.FromSeconds(10);
        INotifyTransitionCompleted[] entryTransitions = [];

        try
        {
            window.Show();
            Pump();
            coordinator.ApplyActor(actor, false, false, true, position, duration);
            Pump();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Pump();
            starts.ShouldBe(1);
            actor.Transitions.ShouldNotBeNull();
            entryTransitions = actor.Transitions!.OfType<INotifyTransitionCompleted>().ToArray();
            entryTransitions.Length.ShouldBe(2);
            Step(0);
            Step(2500);
            actor.Opacity.ShouldBeGreaterThan(0d);
            actor.Opacity.ShouldBeLessThan(1d);

            coordinator.StartClose(false, true, position, duration);

            // Drain normal dispatcher work until the old entry's finally has run.
            PumpUntil(() => starts == 2 && entryTransitions.All(IsReleased));
            AssertExitTarget();
            foreach (var milliseconds in new[] { 2500, 5000, 7500, 10000, 12400 })
            {
                Step(milliseconds);
                closeCount.ShouldBe(0);
                AssertExitTarget();
            }
            Step(12600);
            PumpUntil(() => closeCount == 1);
            starts.ShouldBe(2);
            actor.Opacity.ShouldBe(0);
            actor.MotionTransform.ShouldBeNull();
            actor.Transitions.ShouldBeNull();
            Step(15000);
            closeCount.ShouldBe(1);
        }
        finally
        {
            coordinator.Dispose();
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
        }

        void Step(int milliseconds)
        {
            pulse.Invoke(clock, [TimeSpan.FromMilliseconds(milliseconds)]);
            Pump();
        }

        void PumpUntil(Func<bool> condition)
        {
            SpinWait.SpinUntil(() =>
            {
                Pump();
                return condition();
            }, TimeSpan.FromSeconds(2)).ShouldBeTrue();
        }

        void AssertExitTarget()
        {
            var target = actor.GetBaseValue(BaseMotionActor.MotionTransformProperty);
            target.HasValue.ShouldBeTrue();
            target.Value.ShouldNotBeNull();
            target.Value!.Value.M11.ShouldBe(1, 0.001);
            target.Value.Value.M22.ShouldBe(1, 0.001);
            target.Value.Value.M31.ShouldBe(expectedX, 0.001);
            target.Value.Value.M32.ShouldBe(expectedY, 0.001);
        }

        static bool IsReleased(INotifyTransitionCompleted transition)
        {
            try
            {
                _ = transition.CompletedObservable;
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }
    }

    [Fact]
    public void DetachActor_Preserves_The_Hidden_Terminal_State_For_A_Closed_Card()
    {
        var actor = new MotionActor();
        using var coordinator = new FeedbackCardMotionCoordinator(() => { });

        coordinator.ApplyActor(
            actor,
            isClosing: false,
            isClosed: false,
            isMotionEnabled: false,
            NotificationPosition.TopCenter,
            TimeSpan.Zero);

        coordinator.DetachActor(isClosing: false, isClosed: true);

        actor.Opacity.ShouldBe(0);
        actor.Transitions.ShouldBeNull();
        actor.MotionTransform.ShouldBeNull();
    }

    [Fact]
    public void Detach_Before_Entry_Completes_Does_Not_Suppress_The_Next_Attached_Entry()
    {
        var actor = new MotionActor
        {
            Content = new Border
            {
                Width = 320,
                Height = 96
            }
        };
        var window = new AvaloniaWindow
        {
            Width = 600,
            Height = 400,
            Content = actor
        };
        using var coordinator = new FeedbackCardMotionCoordinator(() => { });

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            coordinator.ApplyActor(
                actor,
                isClosing: false,
                isClosed: false,
                isMotionEnabled: true,
                NotificationPosition.TopRight,
                TimeSpan.FromMinutes(1));

            coordinator.DetachActor(isClosing: false, isClosed: false);
            coordinator.ApplyActor(
                actor,
                isClosing: false,
                isClosed: false,
                isMotionEnabled: true,
                NotificationPosition.TopRight,
                TimeSpan.FromMinutes(1));
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            actor.Transitions.ShouldNotBeNull();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Dispose_During_Entry_Releases_The_Coordinator_And_Actor_Graph()
    {
        var references = CreateDisposedMotionGraph();

        Dispatcher.UIThread.RunJobs();
        ForceFullCollection();

        references.Coordinator.IsAlive.ShouldBeFalse();
        references.Actor.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Dispose_While_Waiting_For_The_Animation_Frame_Releases_The_Owner_Graph()
    {
        var references = CreateDisposedAttachedMotionGraph();

        Dispatcher.UIThread.RunJobs();
        ForceFullCollection();

        references.Coordinator.IsAlive.ShouldBeFalse();
        references.Actor.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Configuration_Change_During_Entry_Does_Not_Restart_The_Current_Actor()
    {
        var actor = new MotionActor
        {
            Content = new Border
            {
                Width = 320,
                Height = 96
            }
        };
        var window = new AvaloniaWindow
        {
            Width = 600,
            Height = 400,
            Content = actor
        };
        using var coordinator = new FeedbackCardMotionCoordinator(() => { });
        var starts = 0;
        actor.PreStart += (_, _) => starts++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            coordinator.ApplyActor(
                actor,
                isClosing: false,
                isClosed: false,
                isMotionEnabled: true,
                NotificationPosition.TopCenter,
                TimeSpan.FromMinutes(1));
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            coordinator.UpdateConfiguration(
                isClosing: false,
                isClosed: false,
                isMotionEnabled: true,
                NotificationPosition.BottomCenter,
                TimeSpan.FromSeconds(2));
            Dispatcher.UIThread.RunJobs();

            starts.ShouldBe(1);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Configuration_Change_During_Exit_Does_Not_Restart_The_Current_Actor()
    {
        var actor = new MotionActor();
        using var coordinator = new FeedbackCardMotionCoordinator(() => { });
        var starts = 0;
        actor.PreStart += (_, _) => starts++;
        coordinator.ApplyActor(
            actor,
            isClosing: true,
            isClosed: false,
            isMotionEnabled: true,
            NotificationPosition.TopCenter,
            TimeSpan.FromMinutes(1));
        Dispatcher.UIThread.RunJobs();

        coordinator.UpdateConfiguration(
            isClosing: true,
            isClosed: false,
            isMotionEnabled: true,
            NotificationPosition.BottomCenter,
            TimeSpan.FromSeconds(2));
        Dispatcher.UIThread.RunJobs();

        starts.ShouldBe(1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Coordinator, WeakReference Actor) CreateDisposedMotionGraph()
    {
        var actor = new MotionActor();
        var coordinator = new FeedbackCardMotionCoordinator(() => { });
        coordinator.ApplyActor(
            actor,
            isClosing: false,
            isClosed: false,
            isMotionEnabled: true,
            NotificationPosition.TopCenter,
            TimeSpan.FromMinutes(1));
        Dispatcher.UIThread.RunJobs();

        var references = (new WeakReference(coordinator), new WeakReference(actor));
        coordinator.Dispose();
        return references;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Coordinator, WeakReference Actor) CreateDisposedAttachedMotionGraph()
    {
        var actor = new MotionActor
        {
            Content = new Border
            {
                Width = 320,
                Height = 96
            }
        };
        var window = new AvaloniaWindow
        {
            Width = 600,
            Height = 400,
            Content = actor
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var coordinator = new FeedbackCardMotionCoordinator(() => { });
        coordinator.ApplyActor(
            actor,
            isClosing: false,
            isClosed: false,
            isMotionEnabled: true,
            NotificationPosition.TopRight,
            TimeSpan.FromMinutes(1));
        Dispatcher.UIThread.RunJobs();

        var references = (new WeakReference(coordinator), new WeakReference(actor));
        coordinator.Dispose();
        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return references;
    }

    private static void ForceFullCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Yield();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
