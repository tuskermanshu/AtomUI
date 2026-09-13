using AtomUI.Desktop.Controls;
using AtomUIGallery.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AtomUINumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;

namespace AtomUIGallery.ShowCases.Message;

public partial class MessageShowCase : GalleryReactiveUserControl<MessageViewModel>
{
    public const string LanguageId = nameof(MessageShowCase);

    private WindowMessageManager? _defaultMessageManager;
    private WindowMessageManager? _stackMessageManager;
    private bool _isStackEnabled = true;
    private int _stackThreshold = 3;
    private int _stackMessageIndex;

    public MessageShowCase()
    {
        InitializeComponent();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _defaultMessageManager?.Dispose();
        _defaultMessageManager = null;
        _stackMessageManager?.Dispose();
        _stackMessageManager = null;
    }

    private void ShowSimpleMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            Lang(MessageShowCaseLangResourceKind.P2MessageHelloAtomUIAvalonia, "Hello, AtomUI/Avalonia!")
        ));
    }

    private void ShowInfoMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Information,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageInformation, "This is an information message.")
        ));
    }

    private void ShowSuccessMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Success,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageSuccess, "This is a success message.")
        ));
    }

    private void ShowWarningMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Warning,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageWarning, "This is a warning message.")
        ));
    }

    private void ShowErrorMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Error,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageError, "This is an error message.")
        ));
    }

    private void ShowLoadingMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Loading,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageActionInProgress, "Action in progress...")
        ));
    }

    private void ShowSequentialMessage(object? sender, RoutedEventArgs e)
    {
        ShowMessage(new AtomUIMessage(
            type: MessageType.Loading,
            content: Lang(MessageShowCaseLangResourceKind.P2MessageActionInProgress, "Action in progress..."),
            expiration: TimeSpan.FromSeconds(2.5),
            onClose: () =>
            {
                ShowMessage(new AtomUIMessage(
                    type: MessageType.Success,
                    expiration: TimeSpan.FromSeconds(2.5),
                    content: Lang(MessageShowCaseLangResourceKind.P2MessageLoadingFinished, "Loading finished"),
                    onClose: () =>
                    {
                        ShowMessage(new AtomUIMessage(
                            type: MessageType.Information,
                            expiration: TimeSpan.FromSeconds(2.5),
                            content: Lang(MessageShowCaseLangResourceKind.P2MessageLoadingFinished, "Loading finished")
                        ));
                    }
                ));
            }
        ));
    }

    private void HandleStackEnabledChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not AtomUIToggleSwitch toggleSwitch)
        {
            return;
        }

        _isStackEnabled = toggleSwitch.IsChecked == true;
        if (_stackMessageManager is not null)
        {
            _stackMessageManager.IsStackEnabled = _isStackEnabled;
        }
    }

    private void HandleStackThresholdChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (sender is not AtomUINumericUpDown { Value: { } value })
        {
            return;
        }

        _stackThreshold = (int)value;
        if (_stackMessageManager is not null)
        {
            _stackMessageManager.StackThreshold = _stackThreshold;
        }
    }

    private void ShowStackMessage(object? sender, RoutedEventArgs e)
    {
        var index = ++_stackMessageIndex;
        var isLongMessage = index % 2 == 0;
        var content = isLongMessage
            ? Format(
                MessageShowCaseLangResourceKind.P2MessageLongStackedFormat,
                "Message {0}: This is a slightly longer stacked message.",
                index)
            : Format(
                MessageShowCaseLangResourceKind.P2MessageStackedFormat,
                "Message {0}: This is a stacked message.",
                index);

        GetStackMessageManager()?.Show(new AtomUIMessage(
            content,
            type: MessageType.Information,
            expiration: TimeSpan.Zero));
    }

    private void DestroyStackMessages(object? sender, RoutedEventArgs e)
    {
        _stackMessageManager?.DestroyAll();
    }

    private void ShowMessage(AtomUIMessage message)
    {
        GetDefaultMessageManager()?.Show(message);
    }

    private static string Lang(MessageShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    private static string Format(
        MessageShowCaseLangResourceKind resourceKind,
        string fallback,
        params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }

    private WindowMessageManager? GetDefaultMessageManager()
    {
        if (_defaultMessageManager is not null)
        {
            return _defaultMessageManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _defaultMessageManager = new WindowMessageManager(topLevel)
        {
            MaxItems = 0
        };
        return _defaultMessageManager;
    }

    private WindowMessageManager? GetStackMessageManager()
    {
        if (_stackMessageManager is not null)
        {
            return _stackMessageManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _stackMessageManager = new WindowMessageManager(topLevel)
        {
            MaxItems       = 0,
            IsStackEnabled = _isStackEnabled,
            StackThreshold = _stackThreshold
        };
        return _stackMessageManager;
    }
}
