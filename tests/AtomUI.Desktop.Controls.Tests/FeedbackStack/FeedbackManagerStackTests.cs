using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackManagerStackTests
{
    static FeedbackManagerStackTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Managers_Expose_Ant_Aligned_Stack_Defaults()
    {
        using var message = new WindowMessageManager(null);
        using var notification = new WindowNotificationManager();

        message.Position.ShouldBe(NotificationPosition.TopCenter);
        message.MaxItems.ShouldBe(0);
        message.IsStackEnabled.ShouldBeFalse();
        message.StackThreshold.ShouldBe(3);
        message.IsPauseOnHover.ShouldBeTrue();

        notification.Position.ShouldBe(NotificationPosition.TopRight);
        notification.MaxItems.ShouldBe(0);
        notification.IsStackEnabled.ShouldBeTrue();
        notification.StackThreshold.ShouldBe(3);
        notification.IsPauseOnHover.ShouldBeTrue();
        new Message("message").Expiration.ShouldBe(TimeSpan.FromSeconds(3));
        new Notification("title", "content").Expiration.ShouldBe(TimeSpan.FromSeconds(4.5));
    }

    [Fact]
    public void Permanent_Items_Keep_Managers_At_Zero_Idle_Timers()
    {
        using var message = new WindowMessageManager(null);
        using var notification = new WindowNotificationManager();

        message.Show(new Message("message", expiration: TimeSpan.Zero));
        notification.Show(new Notification("title", "content", expiration: TimeSpan.Zero));

        message.HasLifetimeScheduler.ShouldBeFalse();
        message.LifetimeEntryCount.ShouldBe(0);
        notification.HasLifetimeScheduler.ShouldBeFalse();
        notification.LifetimeEntryCount.ShouldBe(0);
    }

    [Fact]
    public void Timed_Items_Share_One_Lazy_Scheduler_Per_Manager_And_DestroyAll_Drains_It()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 8; i++)
            {
                manager.Show(new Notification($"Notification {i}", "content", expiration: TimeSpan.FromMinutes(1)));
            }

            manager.HasLifetimeScheduler.ShouldBeTrue();
            manager.LifetimeEntryCount.ShouldBe(8);

            manager.DestroyAll();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            manager.LifetimeEntryCount.ShouldBe(0);
        });
    }

    [Fact]
    public void Detached_Lifecycle_Pause_Cannot_Be_Overridden_By_Disabling_Hover_Pause()
    {
        using var manager = new WindowNotificationManager();
        manager.Show(new Notification("Notification", "content", expiration: TimeSpan.FromMinutes(1)));

        manager.IsLifetimePaused.ShouldBeTrue();

        manager.IsPauseOnHover = false;

        manager.IsLifetimePaused.ShouldBeTrue();

        ShowInWindow(manager, _ => manager.IsLifetimePaused.ShouldBeFalse());
    }

    [Fact]
    public void Message_Stack_Uses_One_Real_Card_And_Two_Static_Backplates()
    {
        using var manager = new WindowMessageManager(null)
        {
            IsMotionEnabled = false,
            IsStackEnabled = true
        };

        ShowInWindow(manager, window =>
        {
            ShowMessages(manager, 4);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            presenter.IsCollapsed.ShouldBeTrue();
            manager.Cards.Count.ShouldBe(4);
            manager.Cards.Count(card => ((IFeedbackStackItem)card).IsStackVisible).ShouldBe(1);

            var backplates = manager.GetVisualDescendants()
                                    .OfType<Border>()
                                    .Where(border => border.Name is "PART_FirstBackplate" or "PART_SecondBackplate")
                                    .ToArray();
            backplates.Length.ShouldBe(2);
            backplates.ShouldAllBe(border => border.IsVisible && !border.IsHitTestVisible);
        });
    }

    [Fact]
    public void Notification_Stack_Keeps_Three_Real_Cards_And_Retains_Older_Items()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification($"Notification {i}", "content", expiration: TimeSpan.Zero));
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
            presenter.IsCollapsed.ShouldBeTrue();
            manager.Cards.Count.ShouldBe(4);
            manager.Cards.Count(card => ((IFeedbackStackItem)card).IsStackVisible).ShouldBe(3);
            ((IFeedbackStackItem)manager.Cards[0]).IsStackVisible.ShouldBeFalse();
        });
    }

    [Fact]
    public void DestroyAll_Closes_Each_Card_Once_And_Empties_The_Stable_Collection()
    {
        using var manager = new WindowNotificationManager
        {
            IsMotionEnabled = false
        };
        var closeCount = 0;

        ShowInWindow(manager, window =>
        {
            for (var i = 0; i < 4; i++)
            {
                manager.Show(new Notification(
                    $"Notification {i}",
                    "content",
                    expiration: TimeSpan.Zero,
                    onClose: () => closeCount++));
            }
            window.UpdateLayout();

            manager.DestroyAll();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            manager.DestroyAll();

            manager.Cards.Count.ShouldBe(0);
            closeCount.ShouldBe(4);
            manager.GetVisualDescendants().OfType<NotificationCard>().ShouldBeEmpty();
        });
    }

    [Fact]
    public void Dispose_Releases_Manager_Card_And_User_Callback_Owner()
    {
        var references = CreateDisposedNotificationGraph();

        ForceFullCollection();

        references.Manager.IsAlive.ShouldBeFalse();
        references.Card.IsAlive.ShouldBeFalse();
        references.CallbackOwner.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Dispose_After_Attachment_Releases_Window_Presenter_Card_Scheduler_And_Callback_Graph()
    {
        var references = CreateDisposedAttachedMessageGraph();

        ForceFullCollection();

        references.Manager.IsAlive.ShouldBeFalse();
        references.Presenter.IsAlive.ShouldBeFalse();
        references.Card.IsAlive.ShouldBeFalse();
        references.CallbackOwner.IsAlive.ShouldBeFalse();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Manager, WeakReference Card, WeakReference CallbackOwner)
        CreateDisposedNotificationGraph()
    {
        var callbackOwner = new object();
        var manager = new WindowNotificationManager();
        manager.Show(new Notification(
            "Notification",
            "content",
            expiration: TimeSpan.FromSeconds(10),
            onClose: () => GC.KeepAlive(callbackOwner)));
        var managerReference = new WeakReference(manager);
        var cardReference = new WeakReference(manager.Cards[0]);
        var callbackReference = new WeakReference(callbackOwner);
        manager.Dispose();
        return (managerReference, cardReference, callbackReference);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference Manager,
                    WeakReference Presenter,
                    WeakReference Card,
                    WeakReference CallbackOwner) CreateDisposedAttachedMessageGraph()
    {
        var callbackOwner = new object();
        var manager = new WindowMessageManager(null)
        {
            IsMotionEnabled = false,
            IsStackEnabled = true
        };
        var window = new AvaloniaWindow
        {
            Width = 700,
            Height = 500,
            Content = manager
        };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        manager.ApplyTemplate();
        manager.Show(new Message(
            "Message",
            expiration: TimeSpan.FromMinutes(1),
            onClose: () => GC.KeepAlive(callbackOwner)));
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();

        var presenter = manager.GetVisualDescendants().OfType<FeedbackStackPresenter>().Single();
        var managerReference = new WeakReference(manager);
        var presenterReference = new WeakReference(presenter);
        var cardReference = new WeakReference(manager.Cards[0]);
        var callbackReference = new WeakReference(callbackOwner);

        manager.Dispose();
        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return (managerReference, presenterReference, cardReference, callbackReference);
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

    private static void ShowMessages(WindowMessageManager manager, int count)
    {
        for (var i = 0; i < count; i++)
        {
            manager.Show(new Message($"Message {i}", expiration: TimeSpan.Zero));
        }
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width = 700,
            Height = 500,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            content.ApplyTemplate();
            window.UpdateLayout();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
