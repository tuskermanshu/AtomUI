using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.MotionScene;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Input;
using Avalonia.Media.Transformation;

namespace AtomUI.Desktop.Controls;

public static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls";

    public static IAtomUIBuilder UseDesktopControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Register(
            builder,
            static () => RuntimePlatform.Features.SupportsNativeWindow
                ? new DesktopControlThemesProvider()
                : new BrowserDesktopControlThemesProvider(),
            PrepareDesktopPackageCore,
            CompleteDesktopPackageCore);
    }

    private static void PrepareDesktopPackageCore(IAtomUIBuilder builder)
    {
        builder.UseCommonControls();
        DialogInputCaptureTracker.Initialize();
    }

    private static void CompleteDesktopPackageCore(IAtomUIBuilder builder)
    {
        GeneratedLanguageModuleRegistration.Register(builder.Localization);
        builder.Theme.AddInitializer(InitializeDesktopRuntime);
    }

    private static void InitializeDesktopRuntime(IThemeManager manager)
    {
        Animation.RegisterCustomAnimator<TransformOperations, MotionTransformOptionsAnimator>();
        var inputManager = AvaloniaLocator.CurrentMutable.GetService(typeof(IInputManager)) as IInputManager;
        if (inputManager is not null)
        {
            AvaloniaLocator.CurrentMutable.BindToSelf(new ToolTipService(inputManager));
        }

        if (!RuntimePlatform.Features.SupportsNativeWindow)
        {
            return;
        }

        if (manager is ThemeManager themeManager)
        {
            MediaBreakPointThemeBootstrapper.Attach(themeManager);
        }
    }

}
