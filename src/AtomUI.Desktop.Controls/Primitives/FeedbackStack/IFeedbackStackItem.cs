namespace AtomUI.Desktop.Controls;

/// <summary>Internal lifecycle contract shared by transient feedback surfaces.</summary>
internal interface IFeedbackStackItem
{
    bool IsClosing { get; }
    bool IsClosed { get; }
    bool IsProgressVisible { get; }
    bool IsStackVisible { get; set; }

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
