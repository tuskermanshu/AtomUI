using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class DesktopControlsWindowOverlayThemeScopeMigrationTests
{
    private static readonly string[] ControlDirectories =
    [
        "src/AtomUI.Desktop.Controls/AdornerLayer",
        "src/AtomUI.Desktop.Controls/Dialog",
        "src/AtomUI.Desktop.Controls/Drawer",
        "src/AtomUI.Desktop.Controls/Flyouts",
        "src/AtomUI.Desktop.Controls/Popup",
        "src/AtomUI.Desktop.Controls/Window",
        "src/AtomUI.Desktop.Controls/WindowTitleBar"
    ];

    private static readonly string[] ThemeDirectories =
        ControlDirectories.Select(static directory => $"{directory}/Themes").ToArray();

    [Fact]
    public void WindowOverlay_Control_Themes_Use_Explicit_Token_Resources()
    {
        foreach (var relativeDirectory in ThemeDirectories)
        {
            ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(relativeDirectory);
        }
    }
}
