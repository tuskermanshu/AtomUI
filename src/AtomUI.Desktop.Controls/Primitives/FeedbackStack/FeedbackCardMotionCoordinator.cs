using AtomUI.MotionScene;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Owns the current template actor's finite entry/exit execution. It prevents stale template
/// continuations from completing a newer motion and releases every cancellation source.
/// </summary>
internal sealed class FeedbackCardMotionCoordinator : IDisposable
{
    private Action? _completeClose;
    private BaseMotionActor? _actor;
    private CancellationTokenSource? _entryCancellation;
    private CancellationTokenSource? _exitCancellation;
    private (BaseMotionActor Actor, Task Completion)? _runningMotion;
    private MotionExecutionState _entryState;
    private MotionExecutionState _exitState;
    private NotificationPosition _position;
    private TimeSpan _duration;
    private bool _isMotionEnabled;
    private bool _isClosing;
    private bool _isClosed;
    private bool _entryCompleted;
    private bool _closeCompleted;
    private bool _isDisposed;

    internal FeedbackCardMotionCoordinator(Action completeClose)
    {
        _completeClose = completeClose;
    }

    internal void ApplyActor(
        BaseMotionActor? actor,
        bool isClosing,
        bool isClosed,
        bool isMotionEnabled,
        NotificationPosition position,
        TimeSpan duration)
    {
        if (_isDisposed)
        {
            return;
        }

        UpdateState(isClosing, isClosed, isMotionEnabled, position, duration);
        CancelEntry();
        CancelExit();
        _actor = actor;

        if (_isClosed)
        {
            ConvergeHidden();
        }
        else if (_isClosing)
        {
            ScheduleExit(DispatcherPriority.Loaded);
        }
        else if (_actor is null)
        {
            // Template/property initialization can run before an actor exists. It must not
            // consume the one entry motion that belongs to the first realized actor.
            return;
        }
        else if (!_isMotionEnabled || _entryCompleted)
        {
            _entryCompleted = true;
            ConvergeVisible();
        }
        else
        {
            FeedbackCardMotion.PrepareEntry(_actor, _position);
            ScheduleEntry();
        }
    }

    internal void StartClose(
        bool isClosed,
        bool isMotionEnabled,
        NotificationPosition position,
        TimeSpan duration)
    {
        if (_isDisposed || isClosed || _closeCompleted)
        {
            return;
        }

        UpdateState(true, isClosed, isMotionEnabled, position, duration);
        CancelEntry();
        if (!_isMotionEnabled)
        {
            CancelExit();
            CompleteExit();
            return;
        }

        ScheduleExit();
    }

    internal void UpdateConfiguration(
        bool isClosing,
        bool isClosed,
        bool isMotionEnabled,
        NotificationPosition position,
        TimeSpan duration)
    {
        if (_isDisposed)
        {
            return;
        }

        UpdateState(isClosing, isClosed, isMotionEnabled, position, duration);
        if (_isClosed)
        {
            CancelEntry();
            CancelExit();
            ConvergeHidden();
            return;
        }

        if (!_isMotionEnabled)
        {
            CancelEntry();
            if (_isClosing)
            {
                CancelExit();
                CompleteExit();
            }
            else
            {
                if (_actor is not null)
                {
                    _entryCompleted = true;
                    ConvergeVisible();
                }
            }
            return;
        }
    }

    internal void DetachActor(bool isClosing, bool isClosed)
    {
        if (_isDisposed)
        {
            return;
        }

        _isClosing = isClosing;
        _isClosed = isClosed;
        CancelEntry();
        CancelExit();
        if (_isClosed)
        {
            ConvergeHidden();
        }
        else if (_isClosing)
        {
            CompleteExit();
        }
        else
        {
            ConvergeVisible();
        }
        _actor = null;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        CancelEntry();
        CancelExit();
        if (_isClosed || _isClosing)
        {
            ConvergeHidden();
        }
        else
        {
            ConvergeVisible();
        }
        _actor = null;
        _completeClose = null;
    }

    private void UpdateState(
        bool isClosing,
        bool isClosed,
        bool isMotionEnabled,
        NotificationPosition position,
        TimeSpan duration)
    {
        _isClosing = isClosing;
        _isClosed = isClosed;
        _isMotionEnabled = isMotionEnabled;
        _position = position;
        _duration = duration;
    }

    private void ScheduleEntry()
    {
        if (_actor is null || _isClosing || _isClosed || _entryCompleted ||
            _entryState != MotionExecutionState.Idle)
        {
            return;
        }

        _entryState = MotionExecutionState.Pending;
        Dispatcher.UIThread.InvokeAsync(RunEntryAsync, DispatcherPriority.Loaded);
    }

    private async Task RunEntryAsync()
    {
        if (_isDisposed || _entryState != MotionExecutionState.Pending || _isClosing || _isClosed)
        {
            return;
        }

        var actor = _actor;
        if (actor is null || !_isMotionEnabled)
        {
            _entryState = MotionExecutionState.Idle;
            _entryCompleted = true;
            ConvergeVisible();
            return;
        }

        var topLevel = TopLevel.GetTopLevel(actor);
        if (topLevel is null)
        {
            // A template can be applied before its card enters a visual tree. Keep the
            // prepared state intact; the card re-applies the actor when it is attached.
            _entryState = MotionExecutionState.Idle;
            return;
        }

        _entryState = MotionExecutionState.Playing;
        var cancellation = new CancellationTokenSource();
        _entryCancellation = cancellation;
        try
        {
            // Match Ant Design's prepare -> next frame -> active motion lifecycle.
            // A dispatcher priority is not a render boundary, so starting the transition
            // earlier allows the compositor to skip the transparent translated frame.
            await WaitForAnimationFrameAsync(topLevel, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(_actor, actor) || _entryState != MotionExecutionState.Playing ||
                _isClosing || _isClosed || !_isMotionEnabled)
            {
                return;
            }

            await RunMotionAsync(actor, true, cancellation.Token);
            if (!cancellation.IsCancellationRequested && ReferenceEquals(_actor, actor) &&
                _entryState == MotionExecutionState.Playing && !_isClosing && !_isClosed)
            {
                _entryCompleted = true;
                ConvergeVisible();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            cancellation.Dispose();
            if (ReferenceEquals(_entryCancellation, cancellation))
            {
                _entryCancellation = null;
                _entryState = MotionExecutionState.Idle;
            }
        }
    }

    private static async Task WaitForAnimationFrameAsync(
        TopLevel topLevel,
        CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellationRegistration = cancellationToken.Register(
            static state =>
            {
                var (source, token) = ((TaskCompletionSource, CancellationToken))state!;
                source.TrySetCanceled(token);
            },
            (completion, cancellationToken));
        topLevel.RequestAnimationFrame(_ => completion.TrySetResult());
        await completion.Task;
    }

    private void ScheduleExit(DispatcherPriority? priority = null)
    {
        if (_isDisposed || _isClosed || _closeCompleted || _exitState != MotionExecutionState.Idle)
        {
            return;
        }

        _exitState = MotionExecutionState.Pending;
        if (priority is { } dispatcherPriority)
        {
            Dispatcher.UIThread.InvokeAsync(RunExitAsync, dispatcherPriority);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(RunExitAsync);
        }
    }

    private async Task RunExitAsync()
    {
        if (_isDisposed || _exitState != MotionExecutionState.Pending || _isClosed || _closeCompleted)
        {
            return;
        }

        var actor = _actor;
        if (actor is null || !_isMotionEnabled)
        {
            CompleteExit();
            return;
        }

        _exitState = MotionExecutionState.Playing;
        var cancellation = new CancellationTokenSource();
        _exitCancellation = cancellation;
        try
        {
            await RunMotionAsync(actor, false, cancellation.Token);
            if (!cancellation.IsCancellationRequested && ReferenceEquals(_actor, actor) &&
                _exitState == MotionExecutionState.Playing)
            {
                CompleteExit();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            cancellation.Dispose();
            if (ReferenceEquals(_exitCancellation, cancellation))
            {
                _exitCancellation = null;
                if (_exitState == MotionExecutionState.Playing)
                {
                    _exitState = MotionExecutionState.Idle;
                }
            }
        }
    }

    private async Task RunMotionAsync(BaseMotionActor actor, bool isEntering, CancellationToken cancellationToken)
    {
        // Cancellation requests do not finish AbstractMotion's asynchronous cleanup.
        // Its old finally must release the actor before another motion writes to it.
        if (_runningMotion is { } previousMotion && ReferenceEquals(previousMotion.Actor, actor))
        {
            try
            {
                await previousMotion.Completion.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }
        cancellationToken.ThrowIfCancellationRequested();

        var motion = new FeedbackCardMotion(isEntering, _position, _duration);
        var motionTask = motion.RunAsync(actor, cancellationToken: cancellationToken);
        _runningMotion = (actor, motionTask);
        try
        {
            await motionTask;
        }
        finally
        {
            if (_runningMotion is { } currentMotion && ReferenceEquals(currentMotion.Completion, motionTask))
            {
                _runningMotion = null;
            }
        }
    }

    private void CompleteExit()
    {
        if (_isDisposed || _isClosed || _closeCompleted)
        {
            return;
        }

        _closeCompleted = true;
        _exitState = MotionExecutionState.Completing;
        ConvergeHidden();
        var completeClose = _completeClose;
        _completeClose = null;
        try
        {
            completeClose?.Invoke();
        }
        finally
        {
            _exitState = MotionExecutionState.Idle;
        }
    }

    private void CancelEntry()
    {
        var cancellation = _entryCancellation;
        _entryCancellation = null;
        cancellation?.Cancel();
        _entryState = MotionExecutionState.Idle;
    }

    private void CancelExit()
    {
        var cancellation = _exitCancellation;
        _exitCancellation = null;
        cancellation?.Cancel();
        _exitState = MotionExecutionState.Idle;
    }

    private void ConvergeVisible()
    {
        if (_actor is null)
        {
            return;
        }

        _actor.Transitions = null;
        _actor.MotionTransform = null;
        _actor.Opacity = 1;
    }

    private void ConvergeHidden()
    {
        if (_actor is null)
        {
            return;
        }

        _actor.Transitions = null;
        _actor.MotionTransform = null;
        _actor.Opacity = 0;
    }
}
