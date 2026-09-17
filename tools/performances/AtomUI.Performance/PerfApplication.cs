using AtomUI.Desktop.Controls;
using AtomUI.Theme;
using Avalonia;

namespace AtomUI.Performance;

public sealed class PerfApplication : Application
{
    public override void Initialize()
    {
        this.UseAtomUI(builder =>
        {
            builder.Theme.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
            builder.UseDesktopControls();
            builder.UseDesktopColorPicker();
            builder.UseDesktopDataGrid();
        });
    }
}
