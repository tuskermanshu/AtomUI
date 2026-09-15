using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Popups;

public class PopupShadowTests
{
    static PopupShadowTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void ShadowsAwareContainer_Shadow_Renderer_Does_Not_Clip_BoxShadow()
    {
        var container = new ShadowsAwareContainer
        {
            BoxShadow = BoxShadows.Parse("0 8 24 0 #66000000"),
            Child     = new Border()
        };

        var shadowRenderer = GetFrameRenderer(container);

        shadowRenderer.ShouldNotBeNull();
        shadowRenderer!.ClipToBounds.ShouldBeFalse();
    }

    [Fact]
    public void Popup_Exposes_SurfaceBackground_As_A_Public_Styled_Property()
    {
        Popup.SurfaceBackgroundProperty.OwnerType.ShouldBe(typeof(Popup));
        typeof(Popup).GetProperty(nameof(Popup.SurfaceBackground)).ShouldNotBeNull();
        new Popup().SurfaceBackground.ShouldBeNull();
    }

    [Fact]
    public void ShadowsAwareContainer_Frame_Renderer_Draws_Configured_Surface()
    {
        var container = new ShadowsAwareContainer
        {
            BoxShadow         = BoxShadows.Parse("0 8 24 0 #66000000"),
            SurfaceBackground = Brushes.White,
            Child             = new Border()
        };

        var frameRenderer = GetFrameRenderer(container);

        frameRenderer.ShouldNotBeNull();
        frameRenderer!.GetType()
                      .GetProperty("SurfaceBackground", BindingFlags.Instance | BindingFlags.Public)
                      .ShouldNotBeNull()!
                      .GetValue(frameRenderer)
                      .ShouldBe(Brushes.White);
    }

    [Fact]
    public void ShadowsAwareContainer_Null_Surface_Preserves_Transparent_Frame_Fill()
    {
        var container = new ShadowsAwareContainer
        {
            BoxShadow         = BoxShadows.Parse("0 8 24 0 #66000000"),
            SurfaceBackground = null,
            Child             = new Border()
        };

        var frameRenderer = GetFrameRenderer(container);

        frameRenderer.ShouldNotBeNull();
        frameRenderer!.GetType()
                      .GetProperty("SurfaceBackground", BindingFlags.Instance | BindingFlags.Public)
                      .ShouldNotBeNull()!
                      .GetValue(frameRenderer)
                      .ShouldBeNull();
    }

    [Fact]
    public void ShadowsAwareContainer_Resolves_The_Final_Surface_Through_Nested_ContentPresenters()
    {
        var expected = new CornerRadius(9);
        var container = new ShadowsAwareContainer
        {
            Child = new ContentPresenter
            {
                Content = new ContentPresenter
                {
                    Content = new Border { CornerRadius = expected }
                }
            }
        };
        var window = new AvaloniaWindow { Content = container };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            container.CornerRadius.ShouldBe(expected);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void ShadowsAwareContainer_Releases_A_Replaced_Nested_Surface()
    {
        var result = CreateNestedSurfaceReplacement();

        try
        {
            CollectGarbage();

            result.OldSurface.IsAlive.ShouldBeFalse();
            result.Container.CornerRadius.ShouldBe(new CornerRadius(12));
        }
        finally
        {
            result.Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Control? GetFrameRenderer(ShadowsAwareContainer container)
    {
        var field = typeof(ShadowsAwareContainer).GetField(
            "_frameRenderer",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (Control?)field!.GetValue(container);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static SurfaceReplacementResult CreateNestedSurfaceReplacement()
    {
        var oldSurface = new Border { CornerRadius = new CornerRadius(4) };
        var innerPresenter = new ContentPresenter { Content = oldSurface };
        var container = new ShadowsAwareContainer
        {
            Child = new ContentPresenter { Content = innerPresenter }
        };
        var window = new AvaloniaWindow { Content = container };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        container.CornerRadius.ShouldBe(new CornerRadius(4));

        var weakSurface = new WeakReference(oldSurface);
        innerPresenter.Content = new Border { CornerRadius = new CornerRadius(12) };
        oldSurface = null!;
        Dispatcher.UIThread.RunJobs();

        return new SurfaceReplacementResult(window, container, weakSurface);
    }

    private static void CollectGarbage()
    {
        for (var iteration = 0; iteration < 3; iteration++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }

    private sealed record SurfaceReplacementResult(
        AvaloniaWindow Window,
        ShadowsAwareContainer Container,
        WeakReference OldSurface);
}
