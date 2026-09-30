using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class DesktopControlsMiscThemeScopeMigrationTests
{
    private static readonly string[] ThemeDirectories =
    [
        "src/AtomUI.Desktop.Controls/BorderBeam/Themes",
        "src/AtomUI.Desktop.Controls/GroupBox/Themes",
        "src/AtomUI.Desktop.Controls/ScrollViewer/Themes",
        "src/AtomUI.Desktop.Controls/Primitives/ArrowDecoratedBox/Themes",
        "src/AtomUI.Desktop.Controls/Primitives/IndicatorScrollViewer/Themes"
    ];

    [Fact]
    public void Misc_Control_Themes_Use_Explicit_Token_Resources()
    {
        foreach (var relativeDirectory in ThemeDirectories)
        {
            ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(relativeDirectory);
        }
    }
}
