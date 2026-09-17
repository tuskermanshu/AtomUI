using System.Reflection;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIMenu = AtomUI.Desktop.Controls.Menu;
using AtomUIMenuItem = AtomUI.Desktop.Controls.MenuItem;
using AtomUIMenuItemGroup = AtomUI.Desktop.Controls.MenuItemGroup;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Menu;

public class MenuSemanticLevelTests
{
    static MenuSemanticLevelTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Top_Level_And_Submenu_Containers_Carry_Mutually_Exclusive_Level_Markers()
    {
        var (menu, root, subMenuItem) = CreateMenuWithGroupAndSubmenu();

        ShowInWindow(menu, root, host =>
        {
            var topLevelItem = menu.Items[0].ShouldBeOfType<AtomUIMenuItem>();
            var group        = menu.Items[1].ShouldBeOfType<AtomUIMenuItemGroup>();
            var groupItem    = group.Items[0].ShouldBeOfType<AtomUIMenuItem>();

            // 第一层：菜单栏项与顶层分组内的项都只带一级 marker。
            topLevelItem.Classes.Contains("semantic-item").ShouldBeTrue();
            topLevelItem.Classes.Contains("semantic-sub-menu-item").ShouldBeFalse();
            groupItem.Classes.Contains("semantic-item").ShouldBeTrue();
            groupItem.Classes.Contains("semantic-sub-menu-item").ShouldBeFalse();
            group.Classes.Contains("semantic-scope-group").ShouldBeTrue();
            group.Classes.Contains("semantic-sub-menu-group").ShouldBeFalse();

            // 子菜单层：展开后实例子项只带子菜单 marker，且不得残留一级 marker。
            var subMenuContainer = menu.Items[2].ShouldBeOfType<AtomUIMenuItem>();
            subMenuContainer.SetCurrentValue(Avalonia.Controls.MenuItem.IsSubMenuOpenProperty, true);
            for (var i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs();
                host.UpdateLayout();
            }

            var childItem = subMenuContainer.Items[0].ShouldBeOfType<AtomUIMenuItem>();
            childItem.Classes.Contains("semantic-sub-menu-item").ShouldBeTrue();
            childItem.Classes.Contains("semantic-item").ShouldBeFalse();

            MarkedWithin(host, "semantic-sub-menu-item").ShouldContain(childItem);
            MarkedWithin(host, "semantic-item").ShouldNotContain(childItem);
        });
    }

    [Fact]
    public void Menu_Outside_Plain_Owner_Keeps_Legacy_Item_Marker()
    {
        // MenuItemGroup 被 MenuFlyout / ContextMenu / DropdownButton 弹层复用时不属于 plain Menu 语义作用域，
        // 必须保留既有的 .semantic-item marker，不能因为 plain Menu 的层级隔离而丢失。
        var group = new AtomUIMenuItemGroup { Header = "Group" };
        group.Items.Add(new object());

        var root   = CreateRoot(group);
        var window = new AtomUIWindow { Width = 320, Height = 200, Content = root };
        try
        {
            window.Show();
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            var child = group.ContainerFromIndex(0).ShouldBeOfType<AtomUIMenuItem>();
            child.Classes.Contains("semantic-item").ShouldBeTrue();
            child.Classes.Contains("semantic-sub-menu-item").ShouldBeFalse();
            group.Classes.Contains("semantic-scope-group").ShouldBeFalse();
            group.Classes.Contains("semantic-sub-menu-group").ShouldBeFalse();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Pinned_Open_Declared_Before_Containers_Exist_Still_Opens_Submenu()
    {
        // 复刻 Gallery 语义预览的用法：AXAML 在模板应用、容器生成之前写入 IsPopupPinnedOpen="True"。
        // 请求必须独立于容器生命周期存活，容器实现后再补齐，而不是在没有任何容器时被丢弃。
        var subMenuItem = new AtomUIMenuItem { Header = "Submenu" };
        subMenuItem.Items.Add(new AtomUIMenuItem { Header = "Submenu Leaf" });

        var menu = new AtomUIMenu
        {
            IsMotionEnabled    = false,
            IsPopupPinnedOpen  = true
        };
        menu.Items.Add(subMenuItem);

        var root   = CreateRoot(menu);
        var window = new AtomUIWindow { Width = 520, Height = 320, Content = root };
        try
        {
            window.Show();
            for (var i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            subMenuItem.IsPopupPinnedOpen.ShouldBeTrue(
                "a declarative pin must reach containers created after the property was set");
            subMenuItem.IsSubMenuOpen.ShouldBeTrue();
            subMenuItem.GetVisualDescendants()
                       .OfType<Popup>()
                       .ShouldContain(static popup => popup.IsOpen);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void IsPopupPinnedOpen_Is_Public_Api_For_Cross_Assembly_Previews()
    {
        // Gallery 的语义预览在另一个程序集里通过 AXAML 声明钉住打开，因此该属性必须公开。
        // 先例：NavMenu / Tour / DropdownButton / AbstractSelect 等已经公开同一属性。
        var property = typeof(AtomUIMenu).GetProperty(nameof(AtomUIMenu.IsPopupPinnedOpen));
        property.ShouldNotBeNull();
        property.GetGetMethod().ShouldNotBeNull().IsPublic.ShouldBeTrue();
        property.GetSetMethod().ShouldNotBeNull().IsPublic.ShouldBeTrue();
    }

    [Fact]
    public void Item_Icon_And_Content_Are_Level_Neutral_Static_Template_Markers()
    {
        // icon / content 的层级 marker 由两个菜单项模板静态声明（同 NavMenu）：同一节点同时带两级 marker，
        // 由 route 的容器 anchor 决定命中哪一层。分组模板同理提供标题与列表的两级 marker。
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml",
            "ItemIconPresenter",
            "semantic-item-icon");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml",
            "ItemIconPresenter",
            "semantic-sub-menu-item-icon");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml",
            "ItemTextPresenter",
            "semantic-item-content");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemTheme.axaml",
            "ItemTextPresenter",
            "semantic-sub-menu-item-content");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml",
            "PopupFrame",
            "semantic-popup-root");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml",
            "ItemIconPresenter",
            "semantic-item-icon");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/TopLevelMenuItemTheme.axaml",
            "HeaderPresenter",
            "semantic-item-content");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemGroupTheme.axaml",
            "GroupTitlePresenter",
            "semantic-item-title");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemGroupTheme.axaml",
            "PART_ItemsPresenter",
            "semantic-list");
        AssertTemplateHasClass(
            "src/AtomUI.Desktop.Controls/Menu/Themes/MenuItemGroupTheme.axaml",
            "PART_ItemsPresenter",
            "semantic-sub-menu-list");
    }

    private static void AssertTemplateHasClass(
        string relativePath,
        string nodeName,
        string semanticClass)
    {
        var document = System.Xml.Linq.XDocument.Load(GetRepoFile(relativePath));
        var node = document.Descendants()
                           .Where(element => element.Attribute("Name")?.Value == nodeName)
                           .ToArray();
        node.ShouldNotBeEmpty($"{relativePath} must declare a node named {nodeName}");
        node.ShouldContain(element =>
            element.Attributes().Any(attribute =>
                attribute.Name.LocalName == $"Classes.{semanticClass}"));
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

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }

    private static (AtomUIMenu Menu, VisualLayerManager Root, object SubMenuItem) CreateMenuWithGroupAndSubmenu()
    {
        var group = new AtomUIMenuItemGroup { Header = "Group" };
        group.Items.Add(new AtomUIMenuItem { Header = "Group Leaf" });

        var subMenuItem = new AtomUIMenuItem { Header = "Submenu" };
        subMenuItem.Items.Add(new AtomUIMenuItem { Header = "Submenu Leaf" });

        var menu = new AtomUIMenu { IsMotionEnabled = false };
        menu.Items.Add(new AtomUIMenuItem { Header = "File" });
        menu.Items.Add(group);
        menu.Items.Add(subMenuItem);

        return (menu, CreateRoot(menu), subMenuItem);
    }

    private static VisualLayerManager CreateRoot(Control content)
    {
        var visualLayerManager = new VisualLayerManager { Child = content };
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
        return visualLayerManager;
    }

    private static Control[] MarkedWithin(Visual root, string semanticClass)
    {
        return root.GetVisualDescendants()
                   .OfType<Control>()
                   .Where(control => control.Classes.Contains(semanticClass))
                   .ToArray();
    }

    private static void ShowInWindow(Control content, Control root, Action<VisualLayerManager> assertion)
    {
        var window = new AtomUIWindow { Width = 520, Height = 320, Content = root };
        try
        {
            window.Show();
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            assertion((VisualLayerManager)root);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
