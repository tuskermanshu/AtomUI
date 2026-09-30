using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xml.Linq;
using AtomUIGallery.Workspace.Views;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.Workspace;

public class WorkspaceWindowLayoutTests
{
    [Fact]
    public void Sidebar_Does_Not_Show_Search_Box()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml"));

        source.ShouldContain("Name=\"ShellHost\"");
        source.ShouldNotContain("<atom:SearchEdit");
        source.ShouldNotContain("Search components...");
        source.ShouldNotContain("<workspaceviews:CaseNavigation Grid.Row=\"1\"");
        source.ShouldNotContain("<Border Grid.Row=\"2\"");
    }

    [Fact]
    public void Workspace_Draws_Navigation_Content_Separator()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Toolkits.GalleryBase/Shell/GalleryShellView.cs"));

        source.ShouldContain("Name             = \"WorkspaceNavigationSeparator\"");
        source.ShouldContain("Grid.SetColumn(_navigationSeparator, 1)");
        source.ShouldContain("Width            = 1");
        source.ShouldContain("HorizontalAlignment = HorizontalAlignment.Left");
        source.ShouldContain("SharedTokenKind.ColorBorderSecondary");
        source.ShouldContain("IsHitTestVisible = false");
        source.ShouldNotContain("BorderThickness=\"0,0,1,0\"");
    }

    [Fact]
    public void Source_Code_Drawer_Masks_Entire_Shell_Instead_Of_Content_Column()
    {
        var source = File.ReadAllText(GetRepoFile("src/AtomUI.Toolkits.GalleryBase/Shell/GalleryShellView.cs"));

        source.ShouldContain("Child = RoutedViewHost");
        source.ShouldContain("PageContent = rootLayout");
        source.ShouldContain("Content = codeDrawerHost");
        source.ShouldNotContain("PageContent = RoutedViewHost");
        source.ShouldNotContain("Child = codeDrawerHost");
    }

    [Fact]
    public void Sidebar_Navigation_Does_Not_Override_Selected_Background()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/CaseNavigation.axaml"));

        source.ShouldNotContain("<UserControl.Styles>");
        source.ShouldNotContain("BaseNavMenuItemHeader[IsSelected=True]");
        source.ShouldNotContain("IsDarkStyle=True][IsSelected=True]");
        source.ShouldNotContain("Value=\"#");
    }

    [Fact]
    public void Sidebar_Navigation_Does_Not_Couple_Dark_Menu_Style_To_Global_Dark_Mode()
    {
        var viewSource = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/CaseNavigation.axaml"));
        var codeBehindSource = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/CaseNavigation.axaml.cs"));

        viewSource.ShouldNotContain("IsDarkStyle=\"True\"");
        codeBehindSource.ShouldNotContain("IThemeManager.IsDarkThemeModeProperty");
        codeBehindSource.ShouldNotContain("NavMenu.IsDarkStyleProperty");
        codeBehindSource.ShouldContain("ShowCaseNavMenu");
    }

    [Fact]
    public void Workspace_Window_Has_Minimum_Width_To_Protect_Main_Content()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml"));

        source.ShouldContain("MinWidth=\"520\"");
        source.ShouldNotContain("MinWidth=\"1040\"");
        source.ShouldNotContain("MinWidth=\"1200\"");
    }

    [Fact]
    public void Title_Bar_Separator_Does_Not_Intercept_Pointer_Input()
    {
        var document = XDocument.Load(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml"));
        var separator = document.Descendants().Single(element =>
            (string?)element.Attribute("Name") == "TitleBarBottomSeparator");

        separator.Name.LocalName.ShouldBe("Border");
        ((string?)separator.Attribute("IsHitTestVisible")).ShouldBe("False");
    }

    [Fact]
    public void Workspace_Window_Menu_Keeps_Motion_And_WaveSpirit_Checks_In_Sync()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml.cs"));

        source.ShouldContain("FindSiblingMenuItem(menuItem, WindowMenuItemKind.WaveSpirit)");
        source.ShouldContain("waveSpiritMenuItem.IsChecked = false");
        source.ShouldContain("FindSiblingMenuItem(menuItem, WindowMenuItemKind.Motion)");
        source.ShouldContain("motionMenuItem.IsChecked = true");
        source.ShouldNotContain("TitleBarWaveSpiritToggleButton");
    }

    [Fact]
    public void Workspace_Window_Caption_Menu_Controls_Button_Visibility_Without_Changing_Capability()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml.cs"));

        source.ShouldContain("IsMinimizeCaptionButtonVisible = menuItem.IsChecked");
        source.ShouldContain("IsMaximizeCaptionButtonVisible = menuItem.IsChecked");
        source.ShouldNotContain("CanMinimize = menuItem.IsChecked");
        source.ShouldNotContain("CanMaximize = menuItem.IsChecked");
    }

    [Fact]
    public void Workspace_Window_Theme_Choices_Use_Available_Theme_Ids()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml.cs"));

        source.ShouldContain("ViewModel.AvailableThemes");
        source.ShouldContain("new StableCommand(ViewModel.SwitchThemeCommand)");
        Regex.IsMatch(source, @"\bToggleType\s*=\s*MenuItemToggleType\.Radio\b").ShouldBeTrue();
        Regex.IsMatch(source, @"\bGroupName\s*=\s*ThemeColorGroupName\b").ShouldBeTrue();
        source.ShouldContain("string.Equals(theme.Id, ViewModel.CurrentThemeId");
        Regex.IsMatch(source, @"\bCommand\s*=\s*switchThemeCommand\b").ShouldBeTrue();
        source.ShouldContain("CommandParameter = theme.Id");
    }

    [Fact]
    public void Workspace_Window_Theme_Command_Does_Not_Forward_CanExecuteChanged_To_Menu_Items()
    {
        var innerCommand  = new RecordingCommand();
        var stableCommand = CreateStableThemeCommand(innerCommand);
        var raiseCount    = 0;
        stableCommand.CanExecuteChanged += (_, _) => raiseCount++;

        innerCommand.RaiseCanExecuteChanged();

        raiseCount.ShouldBe(0);
        stableCommand.CanExecute("PolarGreen").ShouldBeTrue();
        stableCommand.Execute("PolarGreen");
        innerCommand.ExecuteParameters.ShouldBe(["PolarGreen"]);
    }

    [Fact]
    public void Workspace_Window_Nests_Theme_Choices_Under_A_Localized_Settings_Submenu()
    {
        var source = File.ReadAllText(GetRepoFile("controlgallery/AtomUIGallery/Workspace/Views/WorkspaceWindow.axaml"));
        var document = XDocument.Parse(source);
        XNamespace atom = "https://atomui.net";
        var topLevelItems = document.Descendants(atom + "Menu")
                                    .Single()
                                    .Elements(atom + "MenuItem")
                                    .ToArray();
        var themeMenuItem = topLevelItems.Single(static item =>
            item.Attribute("Header")?.Value.Contains("MenuItemTheme}", StringComparison.Ordinal) == true);

        themeMenuItem.Attribute("Tag").ShouldBeNull();
        var children = themeMenuItem.Elements().ToArray();
        children.Length.ShouldBe(6);
        children[0].Name.ShouldBe(atom + "MenuItem");
        children[0].Attribute("Header")!.Value.ShouldContain("MenuItemThemeSettings");
        children[0].Attribute("Tag")!.Value.ShouldContain("WindowMenuItemKind.ThemeCatalog");
        children[1].Name.ShouldBe(atom + "MenuSeparator");
        children[2].Attribute("Header")!.Value.ShouldContain("MenuItemAppearance");
        children[2].Attribute("Tag").ShouldBeNull();
        var appearanceItems = children[2].Elements(atom + "MenuItem").ToArray();
        appearanceItems.Length.ShouldBe(3);
        appearanceItems[0].Attribute("Header")!.Value.ShouldContain("MenuItemLightMode");
        appearanceItems[0].Attribute("ToggleType")!.Value.ShouldBe("Radio");
        appearanceItems[0].Attribute("Tag")!.Value.ShouldContain("WindowMenuItemKind.LightMode");
        appearanceItems[1].Attribute("Header")!.Value.ShouldContain("MenuItemDarkMode");
        appearanceItems[1].Attribute("ToggleType")!.Value.ShouldBe("Radio");
        appearanceItems[1].Attribute("Tag")!.Value.ShouldContain("WindowMenuItemKind.DarkMode");
        appearanceItems[2].Attribute("Header")!.Value.ShouldContain("MenuItemFollowSystem");
        appearanceItems[2].Attribute("ToggleType")!.Value.ShouldBe("Radio");
        appearanceItems[2].Attribute("Tag")!.Value.ShouldContain("WindowMenuItemKind.FollowSystem");
        children[3].Attribute("Tag")!.Value.ShouldContain("WindowMenuItemKind.Compact");
        children[4].Attribute("Tag")!.Value.ShouldContain("WindowMenuItemKind.Motion");
        children[5].Attribute("Tag")!.Value.ShouldContain("WindowMenuItemKind.WaveSpirit");
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not find repository file: {relativePath}");
    }

    private static ICommand CreateStableThemeCommand(ICommand innerCommand)
    {
        var commandType = typeof(WorkspaceWindow).GetNestedType("StableCommand", BindingFlags.NonPublic);
        commandType.ShouldNotBeNull();
        var constructor = commandType!.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(ICommand)],
            modifiers: null);
        constructor.ShouldNotBeNull();
        return (ICommand)constructor.Invoke([innerCommand]);
    }

    private sealed class RecordingCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public List<object?> ExecuteParameters { get; } = [];

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            ExecuteParameters.Add(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
