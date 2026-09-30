using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class DesktopControlsNavigationThemeScopeMigrationTests
{
    [Fact]
    public void Navigation_Control_Themes_Use_Explicit_Token_Resources()
    {
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Breadcrumb/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Menu/Themes");
        ThemeAssetScopeAssertions.AssertFileUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Flyouts/Themes/MenuFlyoutPresenterTheme.axaml");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Pagination/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/Steps/Themes");
        ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(
            "src/AtomUI.Desktop.Controls/TabControl/Themes");
    }
}
