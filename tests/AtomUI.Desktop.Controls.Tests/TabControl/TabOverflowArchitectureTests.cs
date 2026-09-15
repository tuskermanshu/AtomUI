using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabOverflowArchitectureTests
{
    private static readonly string[] OwnerThemePaths =
    [
        "src/AtomUI.Desktop.Controls/TabControl/Themes/TabControlTheme.axaml",
        "src/AtomUI.Desktop.Controls/TabControl/Themes/CardTabControlTheme.axaml",
        "src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/TabStripTheme.axaml",
        "src/AtomUI.Desktop.Controls/TabControl/Themes/TabStrip/CardTabStripTheme.axaml"
    ];

    [Fact]
    public void Four_Owner_Themes_Use_One_Shared_Viewer_And_Template_Boundary()
    {
        foreach (var path in OwnerThemePaths)
        {
            var source = ReadRepositoryFile(path);
            source.ShouldContain("<atom:TabScrollViewer");
            source.ShouldContain("OverflowPopupTemplate=\"{TemplateBinding OverflowPopupTemplate}\"");
            source.ShouldContain("IsPopupPinnedOpen=\"{TemplateBinding IsPopupPinnedOpen}\"");
            source.ShouldNotContain("<atom:TabControlScrollViewer");
            source.ShouldNotContain("<atom:TabStripScrollViewer");
        }
    }

    [Fact]
    public void Shared_Viewer_Has_Static_Lazy_Popup_Without_Per_Open_Flyout_Infrastructure()
    {
        var source = ReadRepositoryFile(
            "src/AtomUI.Desktop.Controls/TabControl/TabScrollViewer.cs");
        var theme = ReadRepositoryFile(
            "src/AtomUI.Desktop.Controls/TabControl/Themes/TabScrollViewerTheme.axaml");

        source.ShouldContain("internal sealed class TabScrollViewer");
        source.ShouldContain("EnsurePopupContent");
        source.ShouldContain("CloseSession");
        source.ShouldNotContain("MenuFlyout");
        source.ShouldNotContain("RelayBind");
        source.ShouldNotContain("Dispatcher.Post");

        theme.ShouldContain("<atom:Popup Name=\"PART_OverflowPopup\"");
        theme.ShouldContain("<DataTemplate x:Key=\"TabOverflowPopupDefaultTemplate\"");
    }

    [Fact]
    public void Obsolete_Duplicated_Overflow_Types_Are_Removed()
    {
        foreach (var path in new[]
                 {
                     "src/AtomUI.Desktop.Controls/TabControl/BaseTabScrollViewer.cs",
                     "src/AtomUI.Desktop.Controls/TabControl/TabControlScrollViewer.cs",
                     "src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStripScrollViewer.cs",
                     "src/AtomUI.Desktop.Controls/TabControl/BaseOverflowMenuItem.cs",
                     "src/AtomUI.Desktop.Controls/TabControl/TabControlOverflowMenuItem.cs",
                     "src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStripOverflowMenuItem.cs"
                 })
        {
            File.Exists(Path.Combine(GetRepositoryRoot(), path)).ShouldBeFalse(path);
        }
    }

    [Theory]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/CardTabControl.cs")]
    [InlineData("src/AtomUI.Desktop.Controls/TabControl/TabStrip/CardTabStrip.cs")]
    public void Card_Owners_Detach_The_Previous_Add_Button_Before_Retemplating(string path)
    {
        var source = ReadRepositoryFile(path);

        source.ShouldContain("_addTabButton.Click -= HandleAddButtonClicked");
        source.IndexOf("_addTabButton.Click -= HandleAddButtonClicked", StringComparison.Ordinal)
            .ShouldBeLessThan(source.IndexOf("e.NameScope.Find<IconButton>", StringComparison.Ordinal));
    }

    private static string ReadRepositoryFile(string relativePath)
    {
        return File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));
    }

    private static string GetRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AtomUI.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
               ?? throw new DirectoryNotFoundException("Unable to locate the AtomUI repository root.");
    }
}
