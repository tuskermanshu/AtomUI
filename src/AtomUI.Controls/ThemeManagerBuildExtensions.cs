using AtomUI.Generated.AtomUIControls;

namespace AtomUI.Controls;

internal static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Controls.Common";

    public static IAtomUIBuilder UseCommonControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        GeneratedControlPackageRegistration.Ensure(
            builder,
            static () => RuntimePlatform.Features.SupportsNativeWindow
                ? new CommonControlThemesProvider()
                : new BrowserCommonControlThemesProvider(),
            static builder =>
            {
                builder.UseImageLoading();
                builder.AddImageCodec(static () => new SvgImageCodec());
            },
            static builder => GeneratedLanguageModuleRegistration.Register(builder.Localization));

        return builder;
    }
}
