namespace AtomUI.Desktop.Controls;

internal interface IFeedbackStackItem
{
    bool IsClosing { get; }
    bool IsClosed { get; }
    bool IsProgressVisible { get; }
    bool IsStackVisible { get; }

    void RequestClose();
    void UpdateRemaining(TimeSpan remaining);
}

internal interface IFeedbackClock
{
    TimeSpan Now { get; }
}

internal interface IFeedbackWakeup : IDisposable
{
    void Schedule(TimeSpan due, Action callback);
    void Cancel();
}
