using System.Xml.Linq;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISplash = AtomUI.Desktop.Controls.Splash;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Splash;

/// <summary>
/// Splash 的九个语义部件全部位于 <c>SplashTheme.axaml</c> 单一 ControlTemplate 内
/// （<c>RuntimeCreated=false</c>，无 <c>.semantic-scope-*</c> 锚点，无 C# marker 注入）。
/// 这些测试锁定 descriptor 契约、模板 marker 契约、生成 Style 的精确命中与优先级，
/// 以及启动页固定 <c>Width</c> + <c>MinHeight</c> 自然测量基线的协调结果。
/// </summary>
public class SplashSemanticPartTests
{
    private const string LogoClass        = "semantic-logo";
    private const string TitleClass       = "semantic-title";
    private const string SubtitleClass    = "semantic-subtitle";
    private const string ContentClass     = "semantic-content";
    private const string SpinClass        = "semantic-spin";
    private const string ProgressBarClass = "semantic-progress-bar";
    private const string MessageClass     = "semantic-message";
    private const string DetailClass      = "semantic-detail";
    private const string FooterClass      = "semantic-footer";
    private const string OwnerClass       = "semantic-owner";

    private static readonly string[] AllPartClasses =
    [
        ContentClass,
        DetailClass,
        FooterClass,
        LogoClass,
        MessageClass,
        ProgressBarClass,
        SpinClass,
        SubtitleClass,
        TitleClass
    ];

    static SplashSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Approved_Parts()
    {
        var descriptor = GetDescriptor();

        // Parts 按 root 优先、其余按 Path ordinal 排序，因此不是声明顺序。
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe([
                      "root",
                      "content",
                      "detail",
                      "footer",
                      "logo",
                      "message",
                      "progressBar",
                      "spin",
                      "subtitle",
                      "title"
                  ]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "logo"),
            LogoClass,
            typeof(ContentPresenter),
            "SplashLogoStyle");
        // 四个文本位的 ContractType 取 Avalonia.Controls.TextBlock 基类而不是节点派生类型
        // （AtomUI.Desktop.Controls.TextBlock），以便后续把节点换成普通 TextBlock 不构成破坏性变更。
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "title"),
            TitleClass,
            typeof(Avalonia.Controls.TextBlock),
            "SplashTitleStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "subtitle"),
            SubtitleClass,
            typeof(Avalonia.Controls.TextBlock),
            "SplashSubtitleStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "content"),
            ContentClass,
            typeof(ContentPresenter),
            "SplashContentStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "spin"),
            SpinClass,
            typeof(AtomUI.Desktop.Controls.Spin),
            "SplashSpinStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "progressBar"),
            ProgressBarClass,
            typeof(AtomUI.Desktop.Controls.ProgressBar),
            "SplashProgressBarStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "message"),
            MessageClass,
            typeof(Avalonia.Controls.TextBlock),
            "SplashMessageStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "detail"),
            DetailClass,
            typeof(Avalonia.Controls.TextBlock),
            "SplashDetailStyle");
        AssertPart(
            descriptor.Parts.Single(static part => part.Name == "footer"),
            FooterClass,
            typeof(ContentPresenter),
            "SplashFooterStyle");
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        var document = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls.Extras/Splash/Themes/SplashTheme.axaml"),
            LoadOptions.SetLineInfo);

        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        // 字面量 Classes="semantic-*" 不是合法静态 marker；必须使用 Classes.semantic-*="True"。
        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe([
                                "semantic-content:ContentPresenter",
                                "semantic-detail:TextBlock",
                                "semantic-footer:ContentPresenter",
                                "semantic-logo:ContentPresenter",
                                "semantic-message:TextBlock",
                                "semantic-progress-bar:ProgressBar",
                                "semantic-spin:Spin",
                                "semantic-subtitle:TextBlock",
                                "semantic-title:TextBlock"
                            ]);

        // root 由生成器隐式产生，模板不得声明 .semantic-root。
        classPropertyMarkers.ShouldNotContain(static marker =>
            marker.Attribute.Name.LocalName == "Classes.semantic-root");

        // Splash 没有容器生命周期与跨视觉根部件，因此模板里不应出现任何 scope 锚点。
        classPropertyMarkers.ShouldNotContain(static marker =>
            marker.Attribute.Name.LocalName.StartsWith("Classes.semantic-scope-", StringComparison.Ordinal));

        // 只有一个 ControlTemplate，没有 Browser 或派生变体，因此不存在 marker 覆盖缺口。
        document.Descendants()
                .Count(static element => element.Name.LocalName == "ControlTemplate")
                .ShouldBe(1);
    }

    /// <summary>
    /// 窗口宿主 <c>SplashWindow</c> 不发布 Semantic Part。窗口壳层（Topmost、ShowInTaskbar、
    /// 透明、尺寸、居中）本身已是 public API，表面阴影与宿主圆角是 Token 语义，
    /// 其窗口级覆盖入口是 <c>SplashWindow.Resources</c> 或专用子类；再开一个窗口侧 Part
    /// 会与 <c>Splash</c> 自身的表面属性形成两条不同步的定制路径。
    /// </summary>
    [Fact]
    public void SplashWindow_Host_Is_Not_A_Semantic_Owner_And_Declares_No_Markers()
    {
        var registry = GetRegistry();
        registry.TryGetControl(typeof(AtomUI.Desktop.Controls.SplashWindow), out _).ShouldBeFalse();

        var document = XDocument.Load(
            GetRepoFile("src/AtomUI.Desktop.Controls.Extras/Splash/Themes/SplashWindowTheme.axaml"));

        document.Descendants()
                .SelectMany(static element => element.Attributes())
                .ShouldNotContain(static attribute =>
                    attribute.Name.LocalName.StartsWith("Classes.semantic-", StringComparison.Ordinal));
    }

    [Fact]
    public void Owner_Scoped_Markers_Stay_Exactly_One_Per_Declared_Part()
    {
        var splash = CreateSplash();

        ShowInWindow(splash, () =>
        {
            GetOwnerMarkers(splash)
                .SelectMany(static control => control.Classes)
                .Where(static className => className.StartsWith("semantic-", StringComparison.Ordinal))
                .OrderBy(static className => className, StringComparer.Ordinal)
                .ShouldBe(AllPartClasses);

            // 九个部件都在 owner 的模板作用域内：单一 /template/ 路由成立，不需要显式 SelectorRoute。
            foreach (var marker in AllPartClasses)
            {
                ReferenceEquals(FindOwnerMarker(splash, marker).TemplatedParent, splash)
                    .ShouldBeTrue($"{marker} must stay inside the Splash template scope");
            }

            AssertMarkerCounts(splash);
        });
    }

    [Fact]
    public void Generated_Semantic_Styles_Hit_Exactly_One_Node_Per_Part()
    {
        var splash = CreateSplash();
        splash.Classes.Add(OwnerClass);

        var descriptor = GetDescriptor();
        var ownerStyle = new Style(selector => selector!.OfType<AtomUISplash>().Class(OwnerClass));
        foreach (var part in descriptor.Parts.Where(static part => part.StyleType != null))
        {
            var partStyle = (Style)Activator.CreateInstance(part.StyleType.ShouldNotBeNull())
                                        .ShouldNotBeNull();
            partStyle.Setters.Add(new Setter(Control.TagProperty, part.Name));
            ownerStyle.Children.Add(partStyle);
        }

        splash.Styles.Add(ownerStyle);

        ShowInWindow(splash, () =>
        {
            var tagged = splash.GetVisualDescendants()
                               .OfType<Control>()
                               .Where(static control => control.Tag is string)
                               .ToArray();

            foreach (var name in AllPartClasses.Select(MarkerToPartName))
            {
                tagged.Count(control => Equals(control.Tag, name)).ShouldBe(
                    1,
                    $"Splash part '{name}' must hit exactly one node; hits: " +
                    string.Join(
                        " | ",
                        tagged.Where(control => Equals(control.Tag, name))
                              .Select(static control => $"{control.GetType().Name}#{control.Name}")));
            }
        });
    }

    [Fact]
    public void Semantic_Selectors_Match_Their_Declared_Owner_Routes()
    {
        var splash = CreateSplash();
        splash.Classes.Add(OwnerClass);
        foreach (var marker in AllPartClasses)
        {
            splash.Styles.Add(CreateRouteStyle(marker, marker));
        }

        ShowInWindow(splash, () =>
        {
            foreach (var marker in AllPartClasses)
            {
                FindOwnerMarker(splash, marker).Tag.ShouldBe(marker);
            }
        });
    }

    /// <summary>
    /// 生成的 Semantic Style 通过 class selector 生效，Setter 以 <c>StyleTrigger</c> 优先级应用，
    /// 高于主题普通 Style Setter 与 <c>TemplateBinding</c>。因此 Part Setter 可以有意覆盖
    /// <c>LogoSize</c> / <c>ProgressBarHeight</c> 这类 Token 基线与文本前景的投影值。
    /// </summary>
    [Fact]
    public void Part_Setters_Override_Token_Baselines()
    {
        var titleBrush = new SolidColorBrush(Color.FromRgb(0x8A, 0x2B, 0xE2));
        var splash = CreateSplash();
        splash.IsIndeterminate = false;
        splash.Progress        = 0.5d;
        splash.Classes.Add(OwnerClass);

        splash.Styles.Add(new Style(selector => selector!.OfType<AtomUISplash>().Class(OwnerClass))
        {
            Children =
            {
                new Style(selector => selector!.Nesting().Template().Class(LogoClass))
                {
                    Setters =
                    {
                        new Setter(Layoutable.WidthProperty, 37d),
                        new Setter(Layoutable.HeightProperty, 37d)
                    }
                },
                new Style(selector => selector!.Nesting().Template().Class(TitleClass))
                {
                    Setters =
                    {
                        new Setter(Avalonia.Controls.TextBlock.ForegroundProperty, titleBrush),
                        new Setter(Avalonia.Controls.TextBlock.FontSizeProperty, 29d)
                    }
                },
                new Style(selector => selector!.Nesting().Template().Class(ProgressBarClass))
                {
                    Setters = { new Setter(Layoutable.HeightProperty, 11d) }
                },
                new Style(selector => selector!.Nesting().Template().Class(ContentClass))
                {
                    Setters = { new Setter(ContentPresenter.PaddingProperty, new Thickness(17)) }
                }
            }
        });

        ShowInWindow(splash, () =>
        {
            var logo = FindOwnerMarker<ContentPresenter>(splash, LogoClass);
            logo.Width.ShouldBe(37d);
            logo.Height.ShouldBe(37d);

            var title = FindOwnerMarker<Avalonia.Controls.TextBlock>(splash, TitleClass);
            title.Foreground.ShouldBe(titleBrush);
            title.FontSize.ShouldBe(29d);

            // 主题把 ProgressBarHeight 映射到进度条 Height；Semantic Setter 必须整体覆盖它。
            // 注意 AtomUI.Desktop.Controls.ProgressBar 派生自 RangeBase，不是 Avalonia 的 ProgressBar，
            // 因此这里的 ContractType 与节点类型都取 AtomUI 自己的类型。
            var progressBar = FindOwnerMarker<AtomUI.Desktop.Controls.ProgressBar>(splash, ProgressBarClass);
            progressBar.Height.ShouldBe(11d);

            var content = FindOwnerMarker<ContentPresenter>(splash, ContentClass);
            content.Padding.ShouldBe(new Thickness(17));
        });
    }

    [Fact]
    public void Marker_Counts_Stay_Stable_Across_State_Switches_And_Template_Reapplication()
    {
        var splash = CreateSplash();
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 480
        };

        try
        {
            window.Show();
            window.Content = splash;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splash);

            // 确定进度：ProgressBar 转为可见、Spin 转隐藏，但两者都不从模板缺席。
            splash.IsIndeterminate = false;
            splash.Progress        = 0.25d;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splash);
            FindOwnerMarker(splash, ProgressBarClass).IsVisible.ShouldBeTrue();
            FindOwnerMarker(splash, SpinClass).IsVisible.ShouldBeFalse();

            // 状态切换只改伪类与可见性，不增删 marker。
            splash.Status          = SplashStatus.Error;
            splash.Message         = "failed";
            splash.Detail          = "detail";
            splash.IsIndeterminate = true;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splash);
            FindOwnerMarker(splash, SpinClass).IsVisible.ShouldBeTrue();
            FindOwnerMarker(splash, ProgressBarClass).IsVisible.ShouldBeFalse();

            splash.Logo   = null;
            splash.Footer = null;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splash);

            // 模板重应用：静态 marker 随模板重建，数量按重建时的状态恢复。
            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            window.Content = splash;
            Dispatcher.UIThread.RunJobs();
            AssertMarkerCounts(splash);
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>
    /// 尺寸基线：Splash 没有 <c>SizeType</c>，<c>Width</c> 由 <c>WindowWidth</c> Token 固定
    /// （<c>SplashOptions.Width</c> 默认 420），高度用 <c>MinHeight</c> 建立基线并允许内容自然增长。
    /// 语义 marker 不得改变这套协调方式。
    /// </summary>
    [Fact]
    public void Fixed_Width_And_MinHeight_Baseline_Allow_Natural_Content_Growth()
    {
        var splash = CreateSplash();
        splash.MinHeight = 280d;
        splash.Content   = new Border { Height = 400 };

        ShowInWindow(splash, () =>
        {
            splash.Bounds.Width.ShouldBe(splash.Width, 0.001);
            splash.Bounds.Width.ShouldBe(420d, 0.001);
            splash.Bounds.Height.ShouldBeGreaterThan(400d);
            splash.Bounds.Height.ShouldBeGreaterThanOrEqualTo(280d);
        });
    }

    /// <summary>
    /// 尺寸基线失败回归：给 <c>content</c> 设置固定 <c>Height</c> 会绕过自然测量，
    /// 使 MinHeight 基线之外的自动高度契约失效——这是固定布局类 Setter 不作为
    /// <c>content</c> 推荐定制路径的原因，见 semantic-part.md 的尺寸基线章节。
    /// </summary>
    [Fact]
    public void Fixed_Height_On_The_Content_Part_Suppresses_The_Natural_Auto_Height_Baseline()
    {
        var baseline = CreateSplash();
        baseline.Content = new Border { Height = 400 };

        var constrained = CreateSplash();
        constrained.Content = new Border { Height = 400 };
        constrained.Styles.Add(new Style(selector =>
            selector!.OfType<AtomUISplash>().Template().Class(ContentClass))
        {
            Setters = { new Setter(Layoutable.HeightProperty, 1d) }
        });

        var root = new StackPanel
        {
            Children = { baseline, constrained }
        };

        ShowInWindow(root, () =>
        {
            FindOwnerMarker(constrained, ContentClass).Height.ShouldBe(1d);
            baseline.Bounds.Height.ShouldBeGreaterThan(400d);
            constrained.Bounds.Height.ShouldBeLessThan(baseline.Bounds.Height);
        });
    }

    private static string MarkerToPartName(string marker)
    {
        return marker switch
        {
            ProgressBarClass => "progressBar",
            _ => marker["semantic-".Length..]
        };
    }

    private static SemanticPartRegistry GetRegistry()
    {
        return Application.Current.ShouldNotBeNull()
                          .GetThemeManager().ShouldNotBeNull()
                          .SemanticParts;
    }

    private static ControlSemanticDescriptor GetDescriptor()
    {
        GetRegistry().TryGetControl(typeof(AtomUISplash), out var descriptor).ShouldBeTrue();
        return descriptor.ShouldNotBeNull();
    }

    private static AtomUISplash CreateSplash()
    {
        return new AtomUISplash
        {
            Logo     = new Border { Width = 40, Height = 40 },
            Title    = "AtomUI",
            Subtitle = "starting",
            Message  = "loading",
            Detail   = "modules",
            Footer   = "v1.0.0"
        };
    }

    private static Style CreateRouteStyle(string partClass, string tag)
    {
        return new Style(selector =>
            selector!.OfType<AtomUISplash>().Class(OwnerClass).Template().Class(partClass))
        {
            Setters = { new Setter(Control.TagProperty, tag) }
        };
    }

    private static void AssertMarkerCounts(AtomUISplash splash)
    {
        foreach (var marker in AllPartClasses)
        {
            GetOwnerMarkers(splash, marker).Length.ShouldBe(
                1,
                $"Splash part '{marker}' must resolve to exactly one owner-scoped marker");
        }
    }

    /// <summary>
    /// 只统计 <c>TemplatedParent</c> 为 owner 自身的节点，确保 marker 不会来自嵌套模板边界
    /// （例如 <c>Spin</c> / <c>ProgressBar</c> 内部模板）。
    /// </summary>
    private static Control[] GetOwnerMarkers(TemplatedControl owner, string? marker = null)
    {
        return owner.GetVisualDescendants()
                    .OfType<Control>()
                    .Where(control => ReferenceEquals(control.TemplatedParent, owner))
                    .Where(static control => control.Classes.Any(
                        className => className.StartsWith("semantic-", StringComparison.Ordinal)))
                    .Where(control => marker is null || control.Classes.Contains(marker))
                    .ToArray();
    }

    private static Control FindOwnerMarker(TemplatedControl owner, string marker)
    {
        return GetOwnerMarkers(owner, marker).Single();
    }

    /// <summary>
    /// 按 <c>ContractType</c> 的可赋值关系取出 marker 节点：ContractType 是 Setter 的最低稳定类型，
    /// 实际节点可以是其派生类型（例如文本位的 ContractType 是 <c>Avalonia.Controls.TextBlock</c>，
    /// 而模板节点是派生类型 <c>AtomUI.Desktop.Controls.TextBlock</c>），因此断言可赋值而不是精确类型。
    /// </summary>
    private static T FindOwnerMarker<T>(TemplatedControl owner, string marker)
        where T : Control
    {
        var control = FindOwnerMarker(owner, marker);
        control.ShouldBeAssignableTo<T>();
        return (T)control;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 640,
            Height  = 480,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.SelectorRoute.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUISplash));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.StyleType.ShouldBeNull();
    }

    private static void AssertPart(
        SemanticPartDescriptor part,
        string selectorClass,
        Type contractType,
        string expectedStyleTypeName)
    {
        part.Path.ShouldBe(part.Name);
        part.SelectorClass.ShouldBe(selectorClass);
        // 静态根模板 Part 未声明 SelectorRoute，由生成器规范化为 "/template/ .<class>"。
        part.SelectorRoute.ShouldBe($"/template/ .{selectorClass}");
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
        part.CrossNestedOwners.ShouldBeFalse();
        part.Since.ShouldBe("6.2.0");
        part.Theme.ShouldBeNull();
        part.StyleType.ShouldNotBeNull();
        part.StyleType!.Name.ShouldBe(expectedStyleTypeName);
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
}
