using AtomUI.Controls.Primitives;
using AtomUI.Toolkits.GalleryBase.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using ModalShowCasePage = AtomUIGallery.ShowCases.Modal.ModalShowCase;
using ModalViewModelPage = AtomUIGallery.ShowCases.Modal.ModalViewModel;

namespace AtomUIGallery.Tests.ShowCases;

public class ModalSemanticPartHighlightTests
{
    [Fact]
    public void Modal_Semantic_Previews_Highlight_All_Parts_Inside_Their_Stages()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new ModalShowCasePage { DataContext = new ModalViewModelPage(new ModalTestScreen()) };
        // 两个预览纵向排列，窗口高度必须容纳完整语义内容，HoverCard 的 MouseMove 坐标才能落在窗口内。
        var window = new AtomUIWindow { Width = 1280, Height = 1600, Content = page };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            AssertPreviewHighlightsAllParts(
                window, "ModalSemanticPreview", "ModalSemanticOwner",
                "进入语义页签后，舞台内 Dialog 必须保持常开");
            AssertPreviewHighlightsAllParts(
                window, "MessageBoxSemanticPreview", "MessageBoxSemanticOwner",
                "进入语义页签后，舞台内 MessageBox 必须保持常开");

            // 钉住常开：语义页签内点击遮罩不关闭任何一个预览宿主。OverlayDialogMask 是 internal 类型，
            // 经其模板 Border#Frame（遮罩可视后代，命中判定要求 source 在 mask 子树内）发起按压。
            var maskFrame = window.GetVisualDescendants().OfType<MotionActor>()
                                  .First(m => m.Name == "PART_MaskMotionActor")
                                  .GetVisualDescendants().OfType<Border>()
                                  .Single(b => b.Name == "Frame");
            RaisePointerPressed(maskFrame);
            Dispatcher.UIThread.RunJobs();
            FindNamedOwner(window, "ModalSemanticOwner").IsOpen
                .ShouldBeTrue("钉住的 Dialog 预览必须忽略遮罩点击");
            FindNamedOwner(window, "MessageBoxSemanticOwner").IsOpen
                .ShouldBeTrue("钉住的 MessageBox 预览不得被无关遮罩点击关闭");

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            GetHighlightCount(window).ShouldBe(0, "切回示例页签后必须清理全部高亮");
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void AssertPreviewHighlightsAllParts(
        AtomUIWindow window,
        string previewName,
        string ownerName,
        string because)
    {
        var preview = window.GetVisualDescendants().OfType<SemanticPartPreview>()
                            .Single(p => p.Name == previewName);
        var owner = FindNamedOwner(window, ownerName);
        owner.IsOpen.ShouldBeTrue(because);
        owner.GetCrossRoots().ShouldHaveSingleItem($"{ownerName} 必须把存活 presenter 上报为跨根");

        var partsPane = preview.GetVisualDescendants().OfType<Border>()
                               .First(b => b.Name == "PART_PartsPane");
        var cards = partsPane.GetVisualDescendants().OfType<UserControl>()
                             .Where(c => c.GetType().Name.Contains("SemanticPartPreviewItem"))
                             .ToArray();
        cards.Length.ShouldBe(9);

        // 顺序与上游 Modal _semantic.tsx 一致；root 依赖 owner 铺满舞台（Dialog 主题默认零尺寸，
        // resolver 对零尺寸目标不建 Adorner），舞台 owner 已显式 420x320。
        foreach (var path in new[]
                 {
                     "root", "mask", "container", "wrapper",
                     "header", "title", "body", "footer", "close"
                 })
        {
            HoverCard(cards, window, path);
            GetHighlightCount(window).ShouldBe(1, $"部件 '{path}' 在 '{previewName}' 内必须恰好命中一个高亮");
        }
    }

    private static AtomUI.Desktop.Controls.Dialog FindNamedOwner(Visual root, string ownerName)
    {
        return root.GetVisualDescendants()
                   .OfType<AtomUI.Desktop.Controls.Dialog>()
                   .Single(d => d.Name == ownerName);
    }

    private static int GetHighlightCount(AtomUIWindow window)
    {
        return window.GetVisualDescendants()
                     .Count(v => v.GetType().Name == "SemanticPartAdorner");
    }

    private static void HoverCard(UserControl[] cards, AtomUIWindow window, string path)
    {
        var card = cards.Single(c =>
            (string?)c.DataContext?.GetType().GetProperty("Path")!.GetValue(c.DataContext) == path);
        card.BringIntoView();
        Dispatcher.UIThread.RunJobs();
        var center = card.TransformToVisual(window)!.Value
                         .Transform(new Point(card.Bounds.Width / 2, card.Bounds.Height / 2));
        window.MouseMove(new Point(center.X, center.Y));
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();
    }

    private static void RaisePointerPressed(InputElement source)
    {
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            new Pointer(Pointer.GetNextFreeId(), PointerType.Mouse, true),
            source,
            default,
            0,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
    }
}

internal sealed class ModalTestScreen : IScreen
{
    public RoutingState Router { get; } = new();
}
