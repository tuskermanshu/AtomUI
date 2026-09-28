using AtomUI.Generated.AtomUIDesktopControlsColorPicker;
namespace AtomUI.Desktop.Controls;

public static class ColorPickerThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.ColorPicker";

    public static IAtomUIBuilder UseDesktopColorPicker(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Register(
            builder,
            static () => new AtomUIColorPickerThemesProvider(),
            complete: static builder => GeneratedLanguageModuleRegistration.Register(builder.Localization));
    }
}
