using AtomUI.MotionScene;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Placement-aware translate/fade motion shared by Message and Notification cards.
/// Queue positioning and stack scale remain on the outer card transform.
/// </summary>
internal sealed class FeedbackCardMotion : AbstractMotion
{
    internal const double Offset = 64d;

    private static readonly ITransform RestTransform = BuildTranslateTransform(0, 0);
    private static readonly ITransform TopTransform = BuildTranslateTransform(0, -Offset);
    private static readonly ITransform BottomTransform = BuildTranslateTransform(0, Offset);
    private static readonly ITransform LeftTransform = BuildTranslateTransform(-Offset, 0);
    private static readonly ITransform RightTransform = BuildTranslateTransform(Offset, 0);

    private readonly bool _isEntering;
    private readonly ITransform _offsetTransform;

    internal FeedbackCardMotion(
        bool isEntering,
        NotificationPosition position,
        TimeSpan duration)
        : base(duration, CreateEasing())
    {
        _isEntering = isEntering;
        _offsetTransform = CreateOffsetTransform(position);
        RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
    }

    internal static void PrepareEntry(BaseMotionActor actor, NotificationPosition position)
    {
        actor.Transitions = null;
        actor.RenderTransformOrigin = new RelativePoint(0, 0, RelativeUnit.Absolute);
        actor.Opacity = 0;
        actor.MotionTransform = CreateOffsetTransform(position);
    }

    protected override void ConfigureMotionStartValue(BaseMotionActor actor)
    {
        actor.Opacity = _isEntering ? 0 : 1;
        actor.MotionTransform = _isEntering ? _offsetTransform : RestTransform;
    }

    protected override void ConfigureMotionEndValue(BaseMotionActor actor)
    {
        actor.Opacity = _isEntering ? 1 : 0;
        actor.MotionTransform = _isEntering ? RestTransform : _offsetTransform;
    }

    protected override void NotifyCompleted(BaseMotionActor actor)
    {
        actor.Opacity = _isEntering ? 1 : 0;
    }

    private static Easing CreateEasing()
    {
        return new SplineEasing
        {
            X1 = 0.645,
            Y1 = 0.045,
            X2 = 0.355,
            Y2 = 1
        };
    }

    private static ITransform CreateOffsetTransform(NotificationPosition position)
    {
        return position switch
        {
            NotificationPosition.TopCenter => TopTransform,
            NotificationPosition.BottomCenter => BottomTransform,
            NotificationPosition.TopLeft or NotificationPosition.BottomLeft =>
                LeftTransform,
            _ => RightTransform
        };
    }
}
