using AtomUI.Generated.AtomUIToolkitsGalleryBase;
using AtomUI.Toolkits.GalleryBase.Configuration;
using AtomUI.Toolkits.GalleryBase.Controls;

namespace AtomUI.Toolkits.GalleryBase;

public static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Toolkits.GalleryBase";

    public static IAtomUIBuilder UseGalleryBase(this IAtomUIBuilder builder,
                                                Action<GalleryBaseOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Register(
            builder,
            static () => new GalleryControlThemesProvider(),
            prepare: _ =>
            {
                if (configure is not null)
                {
                    var options = new GalleryBaseOptions();
                    configure(options);
                    GalleryBaseConfigurationProvider.SetCurrent(options.BuildConfiguration());
                }
            },
            complete: static builder => GeneratedLanguageModuleRegistration.Register(builder.Localization));
    }
}
