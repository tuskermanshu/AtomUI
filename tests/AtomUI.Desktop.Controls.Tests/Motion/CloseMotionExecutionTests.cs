using AtomUI.Controls.Primitives;
using AtomUI.MotionScene;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Motion;

public class CloseMotionExecutionTests
{
    static CloseMotionExecutionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void MessageCard_Retemplate_During_Pending_Close_Starts_One_Exit_Motion()
    {
        var card = new TestMessageCard
        {
            IsMotionEnabled         = true,
            OpenCloseMotionDuration = TimeSpan.Zero
        };
        var actor = new MotionActor();
        var preStartCount = 0;
        actor.PreStart += (_, _) => preStartCount++;

        card.Close();
        card.ApplyMotionActor(actor);
        Dispatcher.UIThread.RunJobs();

        preStartCount.ShouldBe(1);
        card.IsClosed.ShouldBeTrue();
    }

    [Fact]
    public void NotificationCard_Retemplate_During_Pending_Close_Starts_One_Exit_Motion()
    {
        using var manager = new WindowNotificationManager();
        var card = new TestNotificationCard(manager)
        {
            IsMotionEnabled         = true,
            OpenCloseMotionDuration = TimeSpan.Zero
        };
        var actor = new MotionActor();
        var preStartCount = 0;
        actor.PreStart += (_, _) => preStartCount++;

        card.Close();
        card.ApplyMotionActor(actor);
        Dispatcher.UIThread.RunJobs();

        preStartCount.ShouldBe(1);
        card.IsClosed.ShouldBeTrue();
    }

    [Fact]
    public void MessageCard_Retemplate_During_Playing_Close_Restarts_On_The_Current_Actor_And_Closes_Once()
    {
        var card = new TestMessageCard
        {
            IsMotionEnabled = true,
            OpenCloseMotionDuration = TimeSpan.FromSeconds(1)
        };
        var firstActor = new MotionActor();
        var currentActor = new MotionActor();
        var firstStarts = 0;
        var currentStarts = 0;
        var closeCount = 0;
        firstActor.PreStart += (_, _) => firstStarts++;
        currentActor.PreStart += (_, _) => currentStarts++;
        card.MessageClosed += (_, _) => closeCount++;

        card.Close();
        card.ApplyMotionActor(firstActor);
        Dispatcher.UIThread.RunJobs();
        firstStarts.ShouldBe(1);

        card.OpenCloseMotionDuration = TimeSpan.Zero;
        card.ApplyMotionActor(currentActor);
        Dispatcher.UIThread.RunJobs();

        currentStarts.ShouldBe(1);
        card.IsClosed.ShouldBeTrue();
        closeCount.ShouldBe(1);
    }

    [Fact]
    public void NotificationCard_Retemplate_During_Playing_Close_Restarts_On_The_Current_Actor_And_Closes_Once()
    {
        using var manager = new WindowNotificationManager();
        var card = new TestNotificationCard(manager)
        {
            IsMotionEnabled = true,
            OpenCloseMotionDuration = TimeSpan.FromSeconds(1)
        };
        var firstActor = new MotionActor();
        var currentActor = new MotionActor();
        var firstStarts = 0;
        var currentStarts = 0;
        var closeCount = 0;
        firstActor.PreStart += (_, _) => firstStarts++;
        currentActor.PreStart += (_, _) => currentStarts++;
        card.NotificationClosed += (_, _) => closeCount++;

        card.Close();
        card.ApplyMotionActor(firstActor);
        Dispatcher.UIThread.RunJobs();
        firstStarts.ShouldBe(1);

        card.OpenCloseMotionDuration = TimeSpan.Zero;
        card.ApplyMotionActor(currentActor);
        Dispatcher.UIThread.RunJobs();

        currentStarts.ShouldBe(1);
        card.IsClosed.ShouldBeTrue();
        closeCount.ShouldBe(1);
    }

    [Fact]
    public void MessageCard_Disabling_Motion_During_Close_Converges_And_Closes_Once()
    {
        var card = new TestMessageCard
        {
            IsMotionEnabled = true,
            OpenCloseMotionDuration = TimeSpan.FromSeconds(1)
        };
        var actor = new MotionActor();
        var closeCount = 0;
        card.MessageClosed += (_, _) => closeCount++;

        card.Close();
        card.ApplyMotionActor(actor);
        Dispatcher.UIThread.RunJobs();

        card.IsMotionEnabled = false;
        Dispatcher.UIThread.RunJobs();

        card.IsClosed.ShouldBeTrue();
        closeCount.ShouldBe(1);
    }

    [Fact]
    public void NotificationCard_Disabling_Motion_During_Close_Converges_And_Closes_Once()
    {
        using var manager = new WindowNotificationManager();
        var card = new TestNotificationCard(manager)
        {
            IsMotionEnabled = true,
            OpenCloseMotionDuration = TimeSpan.FromSeconds(1)
        };
        var actor = new MotionActor();
        var closeCount = 0;
        card.NotificationClosed += (_, _) => closeCount++;

        card.Close();
        card.ApplyMotionActor(actor);
        Dispatcher.UIThread.RunJobs();

        card.IsMotionEnabled = false;
        Dispatcher.UIThread.RunJobs();

        card.IsClosed.ShouldBeTrue();
        closeCount.ShouldBe(1);
    }

    [Fact]
    public void MessageCard_External_Closed_State_Cancels_Entry_And_Converges_Hidden()
    {
        var card = new TestMessageCard
        {
            IsMotionEnabled = true,
            OpenCloseMotionDuration = TimeSpan.FromMinutes(1)
        };
        var actor = new MotionActor();
        var window = new AvaloniaWindow
        {
            Width = 600,
            Height = 400,
            Content = actor
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            card.ApplyMotionActor(actor);
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            actor.Transitions.ShouldNotBeNull();

            card.IsClosed = true;

            actor.Transitions.ShouldBeNull();
            actor.MotionTransform.ShouldBeNull();
            actor.Opacity.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void NotificationCard_External_Closed_State_Cancels_Entry_And_Converges_Hidden()
    {
        using var manager = new WindowNotificationManager();
        var card = new TestNotificationCard(manager)
        {
            IsMotionEnabled = true,
            OpenCloseMotionDuration = TimeSpan.FromMinutes(1)
        };
        var actor = new MotionActor();
        var window = new AvaloniaWindow
        {
            Width = 600,
            Height = 400,
            Content = actor
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            card.ApplyMotionActor(actor);
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            actor.Transitions.ShouldNotBeNull();

            card.IsClosed = true;

            actor.Transitions.ShouldBeNull();
            actor.MotionTransform.ShouldBeNull();
            actor.Opacity.ShouldBe(0);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TestMessageCard : MessageCard
    {
        internal void ApplyMotionActor(BaseMotionActor actor)
        {
            var nameScope = new NameScope();
            nameScope.Register(BaseMotionActor.MotionActorPart, actor);
            OnApplyTemplate(new TemplateAppliedEventArgs(nameScope));
        }
    }

    private sealed class TestNotificationCard : NotificationCard
    {
        internal TestNotificationCard(WindowNotificationManager manager)
            : base(manager)
        {
        }

        internal void ApplyMotionActor(BaseMotionActor actor)
        {
            var nameScope = new NameScope();
            nameScope.Register(BaseMotionActor.MotionActorPart, actor);
            OnApplyTemplate(new TemplateAppliedEventArgs(nameScope));
        }
    }
}
