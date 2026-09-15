using AtomUI.Localization;
using AtomUI.Toolkits.GalleryBase;
using AtomUIGallery;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(AtomUI.Desktop.Controls.Rendering.Tests.RenderingTestApp))]
[assembly: AvaloniaTestFramework]
[assembly: AvaloniaTestIsolation(AvaloniaTestIsolationLevel.PerTest)]

namespace AtomUI.Desktop.Controls.Rendering.Tests;

public sealed class RenderingTestApp : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<RenderingTestApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });

    public override void Initialize() => this.UseAtomUI(builder =>
    {
        builder.UseDesktopControls();
        builder.UseDesktopExtras();
        builder.UseDesktopColorPicker();
        builder.UseDesktopDataGrid();
        builder.UseGalleryBase(AtomUIGalleryModule.Configure);
        builder.UseGalleryControls();
        builder.UseLanguages(LanguageTags.EnUS, [LanguageTags.EnUS]);
    });
}
