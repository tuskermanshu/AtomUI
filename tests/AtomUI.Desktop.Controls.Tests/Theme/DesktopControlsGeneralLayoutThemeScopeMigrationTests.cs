using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class DesktopControlsGeneralLayoutThemeScopeMigrationTests
{
    [Fact]
    public void General_And_Layout_Control_Themes_Use_Explicit_Token_Resources()
    {
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Buttons/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/FloatButton/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Space/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Splitter/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/SplitView/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Separator/Themes");
    }
}
