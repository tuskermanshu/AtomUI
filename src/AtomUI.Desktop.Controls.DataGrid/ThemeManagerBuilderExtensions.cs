using AtomUI.Generated.AtomUIDesktopControlsDataGrid;
namespace AtomUI.Desktop.Controls;

public static class DataGridThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Desktop.Controls.DataGrid";

    public static IAtomUIBuilder UseDesktopDataGrid(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return GeneratedControlPackageRegistration.Register(
            builder,
            static () => new AtomUIDataGridThemesProvider(),
            complete: static builder => GeneratedLanguageModuleRegistration.Register(builder.Localization));
    }
}
