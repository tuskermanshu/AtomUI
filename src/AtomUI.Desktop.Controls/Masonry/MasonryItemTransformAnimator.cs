using Avalonia.Animation;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace AtomUI.Desktop.Controls.MasonryInternal;

/// <summary>
/// RenderTransform（ITransform?）的关键帧插值器。
/// Avalonia 动画注册表没有 ITransform 条目，直接对 RenderTransform 做关键帧动画会抛
/// "No animator registered"；AtomUI 已为 TransformOperations 注册过同类插值器
/// （MotionTransformOptionsAnimator），这里对 ITransform 做同样的降级插值
/// （非 TransformOperations 的值按 Identity 处理）。
/// 在 MasonryPanel 静态构造中注册，影响面仅限"以关键帧动画 ITransform 属性"这一
/// 此前必然抛异常的场景，不改变任何现有动画行为。
/// </summary>
internal sealed class MasonryItemTransformAnimator : InterpolatingAnimator<ITransform?>
{
    public override ITransform? Interpolate(double progress, ITransform? oldValue, ITransform? newValue)
    {
        var oldOperations = EnsureOperations(oldValue);
        var newOperations = EnsureOperations(newValue);
        return TransformOperations.Interpolate(oldOperations, newOperations, progress);
    }

    private static TransformOperations EnsureOperations(ITransform? value)
    {
        return value as TransformOperations ?? TransformOperations.Identity;
    }
}
