using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using AtomUIGallery.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;

namespace AtomUIGallery.ShowCases.Notification;

public partial class NotificationShowCase : GalleryReactiveUserControl<NotificationViewModel>
{
    public const string LanguageId = nameof(NotificationShowCase);

    private WindowNotificationManager? _basicManager;
    private WindowNotificationManager? _topLeftManager;
    private WindowNotificationManager? _topManager;
    private WindowNotificationManager? _topRightManager;
    private WindowNotificationManager? _bottomLeftManager;
    private WindowNotificationManager? _bottomManager;
    private WindowNotificationManager? _bottomRightManager;
    private WindowNotificationManager? _stackManager;
    private bool _isPauseOnHover = true;
    private bool _isStackEnabled = true;
    private int _stackThreshold = 3;
    private int _stackNotificationIndex;

    public NotificationShowCase()
    {
        InitializeComponent();
        AddHandler(AbstractOptionButtonGroup.OptionCheckedChangedEvent, HandleHoverOptionGroupCheckedChanged);
    }

    private void HandleHoverOptionGroupCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        _isPauseOnHover = args.Index == 0;
        if (_basicManager is not null)
        {
            _basicManager.IsPauseOnHover = _isPauseOnHover;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        DisposeManager(ref _basicManager);
        DisposeManager(ref _topLeftManager);
        DisposeManager(ref _topManager);
        DisposeManager(ref _topRightManager);
        DisposeManager(ref _bottomLeftManager);
        DisposeManager(ref _bottomManager);
        DisposeManager(ref _bottomRightManager);
        DisposeManager(ref _stackManager);
    }

    private WindowNotificationManager? GetBasicManager()
    {
        var manager = GetManager(ref _basicManager, NotificationPosition.TopRight);
        if (manager is not null)
        {
            manager.IsPauseOnHover = _isPauseOnHover;
        }
        return manager;
    }

    private WindowNotificationManager? GetManager(ref WindowNotificationManager? manager, NotificationPosition position)
    {
        if (manager is not null)
        {
            return manager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        manager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 0,
            Position = position
        };
        return manager;
    }

    private static void DisposeManager(ref WindowNotificationManager? manager)
    {
        manager?.Dispose();
        manager = null;
    }

    private void ShowSimpleNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowNeverCloseNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            expiration: TimeSpan.Zero,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationNeverCloseContent,
                "I will never close automatically. This is a purposely very very long description that has many many characters and words.")
        ));
    }

    private void ShowSuccessNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Success,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowInfoNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Information,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowWarningNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Warning,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowErrorNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Error,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification.")
        ));
    }

    private void ShowTopNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _topManager, NotificationPosition.TopCenter)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTopTitle, "Notification Top"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowBottomNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _bottomManager, NotificationPosition.BottomCenter)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationBottomTitle, "Notification Bottom"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowTopLeftNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _topLeftManager, NotificationPosition.TopLeft)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTopLeftTitle, "Notification TopLeft"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowTopRightNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _topRightManager, NotificationPosition.TopRight)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTopRightTitle, "Notification TopRight"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowBottomLeftNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _bottomLeftManager, NotificationPosition.BottomLeft)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationBottomLeftTitle, "Notification BottomLeft"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowBottomRightNotification(object? sender, RoutedEventArgs e)
    {
        GetManager(ref _bottomRightManager, NotificationPosition.BottomRight)?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationBottomRightTitle, "Notification BottomRight"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationHello, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowCustomIconNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification."),
            icon: new SettingOutlined()
        ));
    }

    private void ShowProgressNotification(object? sender, RoutedEventArgs e)
    {
        GetBasicManager()?.Show(new AtomUINotification(
            type: NotificationType.Information,
            title: Lang(NotificationShowCaseLangResourceKind.P2NotificationTitle, "Notification Title"),
            content: Lang(NotificationShowCaseLangResourceKind.P2NotificationContent,
                "This is the content of the notification. This is the content of the notification. This is the content of the notification."),
            showProgress: true
        ));
    }

    private void HandleStackEnabledChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUIToggleSwitch toggleSwitch)
        {
            return;
        }

        _isStackEnabled = toggleSwitch.IsChecked == true;
        if (_stackManager is not null)
        {
            _stackManager.IsStackEnabled = _isStackEnabled;
        }
    }

    private void HandleStackThresholdChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (sender is not AtomUINumericUpDown { Value: { } value })
        {
            return;
        }

        _stackThreshold = (int)value;
        if (_stackManager is not null)
        {
            _stackManager.StackThreshold = _stackThreshold;
        }
    }

    private void ShowStackNotification(object? sender, RoutedEventArgs e)
    {
        var index = ++_stackNotificationIndex;
        var title = Format(
            NotificationShowCaseLangResourceKind.P2NotificationStackedTitleFormat,
            "Notification {0}",
            index);
        var content = index % 2 == 0
            ? Format(
                NotificationShowCaseLangResourceKind.P2NotificationLongStackedFormat,
                "Notification {0}: This is a deliberately longer stacked notification used to verify variable-height cards.",
                index)
            : Format(
                NotificationShowCaseLangResourceKind.P2NotificationStackedFormat,
                "Notification {0}: This is a stacked notification.",
                index);

        GetStackManager()?.Show(new AtomUINotification(
            title: title,
            content: content,
            expiration: TimeSpan.Zero));
    }

    private void DestroyStackNotifications(object? sender, RoutedEventArgs e)
    {
        _stackManager?.DestroyAll();
    }

    private WindowNotificationManager? GetStackManager()
    {
        if (_stackManager is not null)
        {
            return _stackManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _stackManager = new WindowNotificationManager(topLevel)
        {
            MaxItems = 0,
            Position = NotificationPosition.TopRight,
            IsStackEnabled = _isStackEnabled,
            StackThreshold = _stackThreshold
        };
        return _stackManager;
    }

    private static string Lang(NotificationShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    private static string Format(
        NotificationShowCaseLangResourceKind resourceKind,
        string fallback,
        params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}
