using AtomUI.Generated.AtomUIDesktopControlsExtras;
namespace AtomUI.Desktop.Controls;

public static class ExtrasThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.Extras";

    public static IAtomUIBuilder UseDesktopExtras(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Register(
            builder,
            static () => new AtomUIExtrasThemesProvider());
    }
}
