using System.Collections;
using System.Reflection;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUI.Toolkits.GalleryBase.Localization;
using AtomUI.Toolkits.GalleryBase.Navigation;
using AtomUIGallery.ShowCases.Splash;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUIGallery.Tests.ShowCases;

public class SplashShowCasePageTests
{
    [Fact]
    public void Splash_ShowCase_Is_Registered_Under_Other_Category()
    {
        var configuration      = global::AtomUIGallery.AtomUIGalleryModule.CreateConfiguration();
        var galleryProject     = ReadRepoFile("controlgallery/AtomUIGallery/AtomUIGallery.csproj");
        var assemblyInfoSource = ReadRepoFile("controlgallery/AtomUIGallery/Properties/AssemblyInfo.cs");
        var navigationEn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/en-US.xlf");
        var navigationZhCn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-CN.xlf");
        var navigationZhTw = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/Workspace/Localization/CaseNavigationLang/zh-TW.xlf");

        var otherNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == "Other");
        var splashNode = Walk(configuration.NavigationNodes)
            .First(node => node.Key == SplashViewModel.ID);

        otherNode.IsRoute.ShouldBeFalse();
        otherNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        otherNode.Icon.ShouldNotBeNull();
        splashNode.IsRoute.ShouldBeTrue();
        splashNode.Header.ShouldBeAssignableTo<IGalleryLocalizedText>();
        configuration.Routes.ContainsRoute(SplashViewModel.ID).ShouldBeTrue();
        galleryProject.ShouldContain("AtomUI.Desktop.Controls.Extras");
        assemblyInfoSource.ShouldContain("AtomUIGallery.ShowCases.Splash");

        foreach (var localization in new[] { navigationEn, navigationZhCn, navigationZhTw })
        {
            localization.ContainsKey("Other_Splash").ShouldBeTrue();
        }
    }

    [Fact]
    public void Splash_ShowCase_Uses_Document_Layout_With_Examples()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");

        source.ShouldContain("SplashShowCaseLangResource PageSubtitle");
        source.ShouldContain("SplashShowCaseLangResource PageDescription");
        source.ShouldContain("SplashShowCaseLangResource ComponentCategory");
        source.ShouldContain("SplashShowCaseLangResource ComponentStatusPreview");
        source.ShouldContain("SplashShowCaseLangResource ComponentIntroducedVersion");
        source.ShouldNotContain("SplashShowCaseLangResource ScenarioExamples");
        source.ShouldNotContain("SplashShowCaseLangResource ScenarioApi");
        source.ShouldNotContain("SplashShowCaseLangResource ScenarioDesignToken");
        // Splash 通过 GalleryShowCaseHost 提供 Examples / Semantic Parts 两个页签；
        // GalleryShowCaseHost 内部承载 GalleryStickyTabsHost，吸顶行为不变。
        source.ShouldContain("<gallery:GalleryShowCaseHost");
        source.ShouldNotContain("<gallery:GalleryStickyTabsHost");
        source.ShouldContain("<gallery:GalleryShowCaseHost.SemanticPartsContentTemplate>");
        source.ShouldContain("<gallery:GalleryShowCaseHeader");
        source.ShouldContain("Title=\"Splash\"");
        source.ShouldContain("Category=\"{gallery:SplashShowCaseLangResource ComponentCategory}\"");
        source.ShouldContain("Status=\"{gallery:SplashShowCaseLangResource ComponentStatusPreview}\"");
        source.ShouldContain("StatusTagColor=\"processing\"");
        source.ShouldContain("IntroducedVersion=\"{gallery:SplashShowCaseLangResource ComponentIntroducedVersion}\"");
        source.ShouldContain("Subtitle=\"{gallery:SplashShowCaseLangResource PageSubtitle}\"");
        source.ShouldContain("Description=\"{gallery:SplashShowCaseLangResource PageDescription}\"");
        source.ShouldContain("Namespace=\"AtomUI.Desktop.Controls\"");
        source.ShouldContain("Package=\"AtomUI.Desktop.Controls.Extras\"");
        source.ShouldContain("BaseClass=\"ContentControl\"");
        source.ShouldContain("MetadataValueWidth=\"240\"");
        source.ShouldContain("StickyContentPadding=\"28,0,28,0\"");
        source.ShouldNotContain("<atom:TabStrip Name=\"ScenarioTabs\"");
        source.ShouldNotContain("<ContentControl Name=\"ScenarioContentHost\">");
        source.ShouldContain("Name=\"ExamplesContent\"");
        source.ShouldContain("IsScrollEnabled=\"False\"");
        source.ShouldContain("IsDeferredLoadingEnabled=\"True\"");
        source.ShouldNotContain("Tag=\"Examples\"");
        source.ShouldNotContain("Tag=\"Api\"");
        source.ShouldNotContain("Tag=\"DesignToken\"");
        source.ShouldNotContain("<atom:TabControl");
        source.ShouldNotContain("<atom:DataGrid");
    }

    [Fact]
    public void Splash_ShowCase_Semantic_Parts_Preview_Covers_All_Published_Parts()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");
        var en = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/en-US.xlf");

        source.ShouldContain("<gallery:SemanticPartPreview.PartDescriptions>");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:Splash}\"");

        // 右侧说明必须逐个覆盖 descriptor 的十个 Part（含隐式 root）。
        foreach (var path in new[]
                 {
                     "root", "logo", "title", "subtitle", "content",
                     "spin", "progressBar", "message", "detail", "footer"
                 })
        {
            source.ShouldContain($"Path=\"{path}\"");
        }

        // 进度区两个部件互斥可见：两个实例必须分别固定确定态与不确定态，
        // 否则不可见的那个部件无法被定位高亮。
        source.ShouldContain("IsIndeterminate=\"True\"");
        source.ShouldContain("IsIndeterminate=\"False\"");

        foreach (var key in new[]
                 {
                     "SemanticPartStyleTitle",
                     "SemanticPartStyleDescription",
                     "SemanticRootDescription",
                     "SemanticLogoDescription",
                     "SemanticTitleDescription",
                     "SemanticSubtitleDescription",
                     "SemanticContentDescription",
                     "SemanticSpinDescription",
                     "SemanticProgressBarDescription",
                     "SemanticMessageDescription",
                     "SemanticDetailDescription",
                     "SemanticFooterDescription"
                 })
        {
            source.ShouldContain($"SplashShowCaseLangResource {key}");
            en.ContainsKey(key).ShouldBeTrue($"{key} must be localized");
            en[key].ShouldNotBeNullOrWhiteSpace();
        }

        // Semantic Part 说明不得混入 React / DOM 专属术语或上游组件版本号。
        foreach (var key in new[]
                 {
                     "SemanticRootDescription",
                     "SemanticSpinDescription",
                     "SemanticProgressBarDescription"
                 })
        {
            en[key].ShouldNotContain("className");
            en[key].ShouldNotContain("style prop");
            en[key].ShouldNotContain("DOM");
        }
    }

    [Fact]
    public void Splash_ShowCase_Header_Centers_Title_Tags_And_Adds_Blue_Introduced_Version_Tag()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");
        var en = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/en-US.xlf");
        var zhCn = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/zh-CN.xlf");
        var zhTw = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/zh-TW.xlf");

        source.ShouldContain("<gallery:GalleryShowCaseHeader");
        source.ShouldContain("Status=\"{gallery:SplashShowCaseLangResource ComponentStatusPreview}\"");
        source.ShouldContain("StatusTagColor=\"processing\"");
        source.ShouldContain("IntroducedVersion=\"{gallery:SplashShowCaseLangResource ComponentIntroducedVersion}\"");
        source.ShouldNotContain("IntroducedVersionTagColor=");
        source.ShouldNotContain("IsIntroducedVersionTagBordered=");

        foreach (var localization in new[] { en, zhCn, zhTw })
        {
            localization["ComponentIntroducedVersion"].ShouldBe("v6.0.7");
        }
    }

    [Fact]
    public void Splash_ShowCase_Demos_Cover_Visual_States_Without_Showing_Window()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");

        var basicDemo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource BasicTitle");
        basicDemo.ShouldContain("<atom:Splash");
        basicDemo.ShouldContain("Title=\"AtomUI\"");
        basicDemo.ShouldContain("IsIndeterminate=\"True\"");
        basicDemo.ShouldContain("Logo=\"{Binding BasicLogo}\"");
        basicDemo.ShouldNotContain("Splash.ShowAsync");

        var progressDemo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource DeterminateTitle");
        progressDemo.ShouldContain("Progress=\"{Binding ProgressValue}\"");
        progressDemo.ShouldContain("IsIndeterminate=\"False\"");
        progressDemo.ShouldContain("Message=\"{gallery:SplashShowCaseLangResource P2MessageLoadingModules}\"");

        var statusDemo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource StatusTitle");
        statusDemo.ShouldContain("Status=\"Success\"");
        statusDemo.ShouldContain("Status=\"Error\"");
        statusDemo.ShouldContain("Footer=\"{gallery:SplashShowCaseLangResource P2FooterStaticPreview}\"");

        var composedDemo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource ComposedTitle");
        composedDemo.ShouldContain("<atom:Tag");
        composedDemo.ShouldContain("LogoTemplate");
        composedDemo.ShouldContain("FooterTemplate");
    }

    [Fact]
    public void Splash_ShowCase_Window_Service_Demo_Runs_Fake_Loading_And_Closes()
    {
        var pageSource       = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");
        var codeBehindSource = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml.cs");
        var en               = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/en-US.xlf");
        var zhCn             = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/zh-CN.xlf");
        var zhTw             = XliffTestDocument.Read(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/Localization/zh-TW.xlf");

        var serviceDemo = ExtractShowCaseItemMarkup(pageSource, "SplashShowCaseLangResource WindowServiceTitle");
        serviceDemo.ShouldContain("SplashShowCaseLangResource WindowServiceDescription");
        serviceDemo.ShouldContain("P2ContentShowWindowSplash");
        serviceDemo.ShouldContain("Click=\"HandleShowWindowSplashButtonClick\"");

        codeBehindSource.ShouldContain("private bool _isWindowSplashRunning");
        codeBehindSource.ShouldContain("new GallerySplashService()");
        codeBehindSource.ShouldContain("private sealed class GallerySplashService : SplashService");
        codeBehindSource.ShouldContain("return new GallerySplashWindow();");
        codeBehindSource.ShouldNotContain("base.CreateWindow(");
        codeBehindSource.ShouldNotContain("window.Resources[");
        codeBehindSource.ShouldNotContain("ControlTokenResourceKey");
        codeBehindSource.ShouldNotContain("SplashTokens.Identity");
        codeBehindSource.ShouldNotContain("SharedTokenKind");
        codeBehindSource.ShouldNotContain("SplashTokenKind");
        codeBehindSource.ShouldNotContain("WindowSplashTitleForegroundResourceKey");
        codeBehindSource.ShouldNotContain("WindowSplashMessageForegroundResourceKey");
        codeBehindSource.ShouldNotContain("AddTemplateForegroundStyle");
        codeBehindSource.ShouldNotContain(".Template()");
        codeBehindSource.ShouldNotContain(".Name(partName)");
        codeBehindSource.ShouldNotContain("PART_TitleBlock");
        codeBehindSource.ShouldNotContain("PART_MessageBlock");
        codeBehindSource.ShouldNotContain("CreateWindowSplashSurfaceBrush");
        codeBehindSource.ShouldContain("await splashService.ShowAsync(new SplashOptions");
        codeBehindSource.ShouldContain("private const double WindowSplashWidth = 560");
        codeBehindSource.ShouldContain("private const double WindowSplashMinHeight = 360");
        codeBehindSource.ShouldContain("Width               = WindowSplashWidth");
        codeBehindSource.ShouldContain("MinHeight           = WindowSplashMinHeight");
        codeBehindSource.ShouldContain("Logo                = WindowSplashLogo");
        codeBehindSource.ShouldContain("LogoTemplate        = CreateWindowSplashLogoTemplate()");
        codeBehindSource.ShouldNotContain("Content             = WindowSplashStages");
        codeBehindSource.ShouldNotContain("ContentTemplate     = CreateWindowSplashStagesTemplate()");
        codeBehindSource.ShouldNotContain("CreateWindowSplashStagesTemplate");
        codeBehindSource.ShouldNotContain("CreateWindowSplashStage");
        codeBehindSource.ShouldContain("FooterTemplate      = CreateWindowSplashFooterTemplate()");
        codeBehindSource.ShouldContain("LinearGradientBrush");
        codeBehindSource.ShouldContain("TimeSpan.FromSeconds(5)");
        codeBehindSource.ShouldContain("await Task.Delay(TimeSpan.FromSeconds(1)");
        codeBehindSource.ShouldContain("await splashService.SetProgressAsync");
        codeBehindSource.ShouldContain("await splashService.SetStatusAsync(SplashStatus.Success");
        codeBehindSource.ShouldContain("await splashService.CloseAsync()");

        foreach (var localization in new[] { en, zhCn, zhTw })
        {
            localization.ContainsKey("WindowServiceTitle").ShouldBeTrue();
            localization.ContainsKey("WindowServiceDescription").ShouldBeTrue();
            localization.ContainsKey("P2ContentShowWindowSplash").ShouldBeTrue();
            localization.ContainsKey("P2WindowSplashMessageStarting").ShouldBeTrue();
            localization.ContainsKey("P2WindowSplashMessageComplete").ShouldBeTrue();
            localization.ContainsKey("P2WindowSplashFooter").ShouldBeTrue();
        }
    }

    /// <summary>
    /// Semantic Part 演示必须使用生成的专用 Style 类型在 AXAML 声明式定制
    /// （Semantic Part 系统设计 §5.4）：断言专用 Style 类与关键 Setter 值存在，
    /// 并断言不存在以 Name / Loaded / Unloaded 事件处理器为特征的 code-behind 回退。
    /// </summary>
    [Fact]
    public void Splash_ShowCase_Semantic_Part_Demo_Uses_Generated_Styles_Without_CodeBehind_Fallback()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml");
        var codeBehind = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Other/Splash/Views/SplashShowCase.axaml.cs");

        var demo = ExtractShowCaseItemMarkup(source, "SplashShowCaseLangResource SemanticPartStyleTitle");
        demo.ShouldContain("SourceKey=\"splash-semantic-part\"");
        demo.ShouldContain("Classes=\"semantic-parts\"");

        // 专用 Semantic Part Style：每个定制点必须命中生成的 Style 类型并提供 Setter 值。
        demo.ShouldContain("<atom:SplashLogoStyle x:SetterTargetType=\"ContentPresenter\">");
        demo.ShouldContain("<atom:SplashTitleStyle x:SetterTargetType=\"TextBlock\">");
        demo.ShouldContain("<atom:SplashSubtitleStyle x:SetterTargetType=\"TextBlock\">");
        demo.ShouldContain("<atom:SplashMessageStyle x:SetterTargetType=\"TextBlock\">");
        demo.ShouldContain("<atom:SplashDetailStyle x:SetterTargetType=\"TextBlock\">");
        demo.ShouldContain("<atom:SplashProgressBarStyle x:SetterTargetType=\"atom:ProgressBar\">");
        demo.ShouldContain("<Setter Property=\"Height\" Value=\"10\" />");
        demo.ShouldContain("<Setter Property=\"Foreground\" Value=\"#531DAB\" />");

        // root 的定制入口是 owner 作用域的普通 Setter，不是不存在的 root Part Style。
        demo.ShouldContain("<Style Selector=\"atom|Splash.semantic-parts-root\">");
        demo.ShouldContain("<Setter Property=\"CornerRadius\" Value=\"16\" />");
        demo.ShouldNotContain("<atom:SplashRootStyle");

        // 演示不得穿透 Splash 模板或依赖 template part 名称。
        demo.ShouldNotContain("/template/");
        demo.ShouldNotContain("PART_");
        demo.ShouldNotContain("<atom:Splash.Styles>");

        // 不存在以 Name / Loaded / Unloaded 为特征的 code-behind 回退。
        codeBehind.ShouldNotContain("splash-semantic-part");
        codeBehind.ShouldNotContain(".Template()");
        codeBehind.ShouldNotContain("PART_TitleBlock");
        codeBehind.ShouldNotContain("PART_LogoPresenter");
    }

    /// <summary>
    /// Semantic Parts 预览必须在页签真正切换到 SemanticParts 后才物化，并解析全部十个 Part。
    /// 该用例同时是 descriptor 注册的运行时证据：Preview 在缺少 descriptor、说明路径不存在，
    /// 或 owner 类型不兼容时会直接抛出，仅做源码文本断言无法发现这类失败。
    ///
    /// 进度区的两个部件互斥可见（`spin` 只在不确定态可见、`progressBar` 只在确定态可见），
    /// 而 Preview 跳过不可见目标，因此舞台需要确定态与不确定态各一个实例。
    /// </summary>
    [Fact]
    public void Splash_Semantic_Preview_Resolves_All_Ten_Parts_With_Both_Progress_States()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplashShowCase
        {
            DataContext = new SplashViewModel(new TestScreen())
        };

        AssertInWindow(page, () =>
        {
            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldBeEmpty();

            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            page.GetVisualDescendants().OfType<SemanticPartPreview>().ShouldHaveSingleItem();
            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SplashSemanticPreview");

            var semanticOwner = preview.SemanticOwner.ShouldBeOfType<AtomUI.Desktop.Controls.Splash>();
            semanticOwner.Name.ShouldBe("SplashSemanticOwner");

            // 十个 Part 的说明路径必须全部解析为 descriptor 中的真实 Part。
            var previewItems = GetPreviewItems(preview);
            previewItems
                .Select(static item => (string)item.GetType().GetProperty("Path")!.GetValue(item)!)
                .OrderBy(static path => path, StringComparer.Ordinal)
                .ShouldBe([
                    "content", "detail", "footer", "logo", "message",
                    "progressBar", "root", "spin", "subtitle", "title"
                ]);

            // 两个实例分别固定不确定态与确定态，使 spin 与 progressBar 都能被定位。
            var stages = page.GetVisualDescendants()
                             .OfType<AtomUI.Desktop.Controls.Splash>()
                             .Where(static splash => splash.IsEffectivelyVisible)
                             .ToArray();
            stages.Length.ShouldBe(2);
            stages.Count(static splash => splash.IsIndeterminate).ShouldBe(1);
            stages.Count(static splash => !splash.IsIndeterminate).ShouldBe(1);

            var indeterminate = stages.Single(static splash => splash.IsIndeterminate);
            var determinate   = stages.Single(static splash => !splash.IsIndeterminate);
            FindMarker(indeterminate, "semantic-spin").IsEffectivelyVisible.ShouldBeTrue();
            FindMarker(determinate, "semantic-progress-bar").IsEffectivelyVisible.ShouldBeTrue();

            // 每个公开 Part 都必须能在舞台上真正定位到至少一个目标节点。
            // 只断言说明列表有 10 行是不够的：某个 Part 若解析不到可见目标（例如它只存在于被隐藏的
            // 那个进度态实例上），高亮会是空的，而说明行仍然存在。
            foreach (var item in previewItems)
            {
                var path = (string)item.GetType().GetProperty("Path")!.GetValue(item)!;
                InvokeSetHoveredPart(preview, item, true);
                Dispatcher.UIThread.RunJobs();

                var session = ReadHighlightSession(preview);
                session.ShouldNotBeNull($"part '{path}' must produce a highlight session");
                ReadTotalMatchCount(session!).ShouldBeGreaterThanOrEqualTo(
                    1,
                    $"part '{path}' must resolve to at least one target inside the preview stage");

                InvokeSetHoveredPart(preview, item, false);
                Dispatcher.UIThread.RunJobs();
            }

            host.SelectedTab = GalleryShowCaseTab.Examples;
            Dispatcher.UIThread.RunJobs();
            Assert.All(
                page.GetVisualDescendants().OfType<SemanticPartPreview>(),
                static candidate => Assert.False(candidate.IsEffectivelyVisible));
        });
    }

    /// <summary>
    /// 单预览页签的内容高度被 <c>GalleryShowCaseHost</c> 钳制到视口剩余高度（此时部件面板内部滚动），
    /// 因此 Semantic Parts 舞台的合成内容必须能放进画布：任何实例只要超出画布内容区，它的下半部分就会被
    /// 静默裁掉（真机表现为第二个实例只剩一条顶部白边，看起来像渲染异常）。
    ///
    /// 该契约**依赖视口尺寸**，所以不能只测一个窗口大小。实测的失效区间是「非 compact 且画布较窄」：
    /// 此时左右布局可用宽度不足以放下并排的两个实例，内容一旦换行就会把第二个实例推到画布之外；
    /// 而更窄的窗口会切到 compact（上下布局、画布高度变为内容驱动）反而看不出来。因此本用例遍历
    /// 宽/窄/高/矮多种视口，逐尺寸断言包含关系。
    /// </summary>
    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1300, 900)]
    [InlineData(1300, 480)]
    [InlineData(1000, 800)]
    [InlineData(900, 900)]
    [InlineData(820, 700)]
    [InlineData(700, 600)]
    [InlineData(600, 560)]
    [InlineData(560, 480)]
    [InlineData(520, 900)]
    [InlineData(520, 520)]
    [InlineData(520, 380)]
    public void Splash_Semantic_Preview_Keeps_Both_Progress_State_Instances_Inside_The_Stage(
        double width,
        double height)
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplashShowCase
        {
            DataContext = new SplashViewModel(new TestScreen())
        };

        AssertInWindow(page, width, height, () =>
        {
            var host = page.GetVisualDescendants().OfType<GalleryShowCaseHost>().Single();
            host.SelectedTab = GalleryShowCaseTab.SemanticParts;
            Dispatcher.UIThread.RunJobs();

            var preview = page.GetVisualDescendants()
                              .OfType<SemanticPartPreview>()
                              .Single(static candidate => candidate.Name == "SplashSemanticPreview");
            var contentHost = preview.GetVisualDescendants()
                                     .OfType<ContentPresenter>()
                                     .Single(static candidate => candidate.Name == "PART_PreviewContentHost");

            var instances = page.GetVisualDescendants()
                                .OfType<AtomUI.Desktop.Controls.Splash>()
                                .Where(static splash => splash.IsEffectivelyVisible)
                                .ToArray();
            instances.Length.ShouldBe(2);

            var context = $"viewport={width}x{height}";
            const double tolerance = 0.5;
            foreach (var instance in instances)
            {
                var origin = instance.TranslatePoint(default, contentHost);
                origin.ShouldNotBeNull();

                var offset = origin!.Value;
                var state  = instance.IsIndeterminate ? "indeterminate" : "determinate";

                // 顶部与左侧不得越出内容区。
                offset.Y.ShouldBeGreaterThanOrEqualTo(-tolerance, $"Splash '{state}' starts above the stage ({context})");
                offset.X.ShouldBeGreaterThanOrEqualTo(-tolerance, $"Splash '{state}' starts left of the stage ({context})");

                // 底部与右侧不得越出内容区：越出部分会被画布裁掉，用户只会看到半张卡片。
                (offset.Y + instance.Bounds.Height).ShouldBeLessThanOrEqualTo(
                    contentHost.Bounds.Height + tolerance,
                    $"Splash '{state}' is clipped at the stage bottom ({context}) " +
                    $"(origin.Y={offset.Y:0.#}, height={instance.Bounds.Height:0.#}, " +
                    $"canvas={contentHost.Bounds.Width:0.#}x{contentHost.Bounds.Height:0.#})");
                (offset.X + instance.Bounds.Width).ShouldBeLessThanOrEqualTo(
                    contentHost.Bounds.Width + tolerance,
                    $"Splash '{state}' is clipped at the stage right edge ({context}) " +
                    $"(origin.X={offset.X:0.#}, width={instance.Bounds.Width:0.#}, " +
                    $"canvas={contentHost.Bounds.Width:0.#}x{contentHost.Bounds.Height:0.#})");
            }

            // 两个实例并排：纵向位置一致，横向不重叠，避免再次堆叠或压在一起。
            var left  = instances[0].TranslatePoint(default, contentHost)!.Value;
            var right = instances[1].TranslatePoint(default, contentHost)!.Value;
            Math.Abs(left.Y - right.Y).ShouldBeLessThan(
                instances[0].Bounds.Height / 2,
                $"both progress-state instances must sit side by side, not stack vertically ({context})");
            (left.X + instances[0].Bounds.Width).ShouldBeLessThanOrEqualTo(
                right.X + tolerance,
                $"the two progress-state instances must not overlap horizontally ({context})");
        });
    }

    /// <summary>
    /// Semantic Part 演示的专用 Style 必须真正命中并生效，而不只是写在 AXAML 里：
    /// 断言生成 Style 的 Setter 值出现在目标节点上，且 root 表面定制通过 owner 属性生效。
    /// </summary>
    [Fact]
    public void Splash_Semantic_Style_Example_Applies_The_Declared_Values()
    {
        AvaloniaTestApp.EnsureInitialized();

        var page = new SplashShowCase
        {
            DataContext = new SplashViewModel(new TestScreen())
        };

        AssertInWindow(page, () =>
        {
            var panel = page.GetVisualDescendants().OfType<ShowCasePanel>().Single();
            var item = panel.Children
                            .OfType<ShowCaseItem>()
                            .Single(static candidate => candidate.SourceKey == "splash-semantic-part");
            item.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();

            var styled = page.GetVisualDescendants()
                             .OfType<AtomUI.Desktop.Controls.Splash>()
                             .Single(static splash => splash.Classes.Contains("semantic-parts"));

            // 专用 Semantic Part Style 的 Setter 必须落在模板节点上。
            // logo 必须断言**可见尺寸**而不只是 presenter 的 Width/Height：内容若自带固定尺寸，
            // presenter 会被撑破（内层 48 溢出 36 的 presenter），Part Setter 便对可见外观失效。
            var logoPresenter = FindMarker<ContentPresenter>(styled, "semantic-logo");
            logoPresenter.Width.ShouldBe(36d);
            logoPresenter.Height.ShouldBe(36d);
            var logoContent = logoPresenter.GetVisualChildren().OfType<Control>().FirstOrDefault();
            logoContent.ShouldNotBeNull("the logo part must host the logo template content");
            logoContent!.Bounds.Width.ShouldBe(
                36d,
                "the visible logo must match the Part style value; a fixed-size logo template overflows the presenter");
            logoContent.Bounds.Height.ShouldBe(36d);
            logoContent.Bounds.X.ShouldBe(0d, "the logo content must not overflow its presenter box");
            logoContent.Bounds.Y.ShouldBe(0d);
            AssertSolidColor(FindMarker<Avalonia.Controls.TextBlock>(styled, "semantic-title").Foreground, "#531DAB");
            FindMarker<Avalonia.Controls.TextBlock>(styled, "semantic-title").FontSize.ShouldBe(22d);
            AssertSolidColor(FindMarker<Avalonia.Controls.TextBlock>(styled, "semantic-subtitle").Foreground, "#722ED1");
            AssertSolidColor(FindMarker<Avalonia.Controls.TextBlock>(styled, "semantic-message").Foreground, "#1D39C4");
            AssertSolidColor(FindMarker<Avalonia.Controls.TextBlock>(styled, "semantic-detail").Foreground, "#08979C");
            FindMarker<Avalonia.Controls.TextBlock>(styled, "semantic-detail").FontSize.ShouldBe(13d);
            // 主题把 ProgressBarHeight 映射到进度条高度；示例必须覆盖它。
            // AtomUI 的 ProgressBar 派生自 RangeBase，不是 Avalonia.Controls.ProgressBar。
            FindMarker<AtomUI.Desktop.Controls.ProgressBar>(styled, "semantic-progress-bar").Height.ShouldBe(10d);

            // root 的表面定制走 owner 作用域普通 Setter。
            var rootStyled = page.GetVisualDescendants()
                                 .OfType<AtomUI.Desktop.Controls.Splash>()
                                 .Single(static splash => splash.Classes.Contains("semantic-parts-root"));
            AssertSolidColor(rootStyled.Background, "#F9F0FF");
            rootStyled.CornerRadius.ShouldBe(new CornerRadius(16));
            rootStyled.Padding.ShouldBe(new Thickness(28, 24));

            // 状态色语义不受普通消息 Part 定制影响：Success 状态仍走主题的 SuccessColor。
            // 只针对本示例自己的实例断言，避免与其他演示项中的 Success 实例混淆。
            rootStyled.Status.ShouldBe(AtomUI.Desktop.Controls.SplashStatus.Success);
            FindMarker<Avalonia.Controls.TextBlock>(rootStyled, "semantic-message").Foreground.ShouldNotBeNull();
        });
    }

    private static object[] GetPreviewItems(SemanticPartPreview preview)
    {
        var itemsValue = typeof(SemanticPartPreview)
                         .GetProperty("Items", BindingFlags.Instance | BindingFlags.NonPublic)
                         ?.GetValue(preview);
        itemsValue.ShouldNotBeNull();
        return itemsValue.ShouldBeAssignableTo<IEnumerable>().Cast<object>().ToArray();
    }

    private static void InvokeSetHoveredPart(SemanticPartPreview preview, object item, bool isHovered)
    {
        var method = typeof(SemanticPartPreview)
                     .GetMethod("SetHoveredPart", BindingFlags.Instance | BindingFlags.NonPublic)
                     ?? throw new MissingMethodException(typeof(SemanticPartPreview).FullName, "SetHoveredPart");
        method.Invoke(preview, [item, isHovered]);
    }

    private static object? ReadHighlightSession(SemanticPartPreview preview)
    {
        return typeof(SemanticPartPreview)
               .GetProperty("ActiveHighlightSession", BindingFlags.Instance | BindingFlags.NonPublic)
               ?.GetValue(preview);
    }

    private static int ReadTotalMatchCount(object session)
    {
        // SemanticPartHighlightSession 是 internal 类型，而 TotalMatchCount 是它的 public 属性，
        // 因此反射必须同时包含 Public（只用 NonPublic 会取不到）。
        var value = session.GetType()
                           .GetProperty("TotalMatchCount", BindingFlags.Instance | BindingFlags.Public)
                           ?.GetValue(session);
        value.ShouldNotBeNull();
        return value.ShouldBeAssignableTo<int>();
    }

    private static Control FindMarker(AtomUI.Desktop.Controls.Splash splash, string marker)
    {
        return splash.GetVisualDescendants()
                     .OfType<Control>()
                     .Single(control => control.Classes.Contains(marker));
    }

    private static T FindMarker<T>(AtomUI.Desktop.Controls.Splash splash, string marker)
        where T : Control
    {
        var control = FindMarker(splash, marker);
        control.ShouldBeAssignableTo<T>();
        return (T)control;
    }

    private static void AssertSolidColor(IBrush? brush, string expected)
    {
        brush.ShouldBeAssignableTo<ISolidColorBrush>();
        ((ISolidColorBrush)brush!).Color.ShouldBe(Color.Parse(expected));
    }

    private static void AssertInWindow(Control content, Action assertion)
    {
        AssertInWindow(content, 1280, 900, assertion);
    }

    private static void AssertInWindow(Control content, double width, double height, Action assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = content
        };
        var window = new AvaloniaWindow
        {
            Content = visualLayerManager,
            Width   = width,
            Height  = height
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

    private sealed class TestScreen : IScreen
    {
        public RoutingState Router { get; } = new();
    }

    [Fact]
    public void Splash_ShowCase_Dedicated_Child_Owns_Its_Custom_Splash_Styles()
    {
        var windowSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/ShowCaseControls/GallerySplashWindow.axaml");
        var windowControlSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/ShowCaseControls/GallerySplashWindow.axaml.cs");
        var splashSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/ShowCaseControls/GalleryWindowSplash.axaml");
        var splashControlSource = ReadRepoFile(
            "controlgallery/AtomUIGallery/ShowCases/Other/Splash/ShowCaseControls/GalleryWindowSplash.axaml.cs");

        windowControlSource.ShouldContain("partial class GallerySplashWindow : SplashWindow");
        windowControlSource.ShouldNotContain("GalleryWindowSplash :");
        windowSource.ShouldContain("x:Class=\"AtomUIGallery.ShowCases.Splash.GallerySplashWindow\"");
        windowSource.ShouldContain("<local:GalleryWindowSplash />");
        windowSource.ShouldNotContain("ControlTheme");
        windowSource.ShouldNotContain("BasedOn");
        windowSource.ShouldNotContain("StaticResource");
        windowSource.ShouldNotContain("/template/");

        splashControlSource.ShouldContain("sealed partial class GalleryWindowSplash : SplashControl");
        splashControlSource.ShouldContain("StyleKeyOverride { get; } = typeof(SplashControl)");
        splashSource.ShouldContain("x:Class=\"AtomUIGallery.ShowCases.Splash.GalleryWindowSplash\"");
        splashSource.ShouldContain("Classes=\"gallery-window-splash\"");
        splashSource.ShouldContain("<atom:Splash.Background>");
        splashSource.ShouldContain("<atom:Splash.Styles>");
        splashSource.ShouldNotContain("ControlTheme");
        splashSource.ShouldNotContain("BasedOn");
        splashSource.ShouldNotContain("StaticResource");
        splashSource.ShouldNotContain("Border#PART_SurfaceLayout");
        // 专用启动页视觉必须通过生成的 Semantic Part Style 表达（Semantic Part 系统设计 §5.4）：
        // 外层 Style 负责 owner 类型与业务 class，生成 Style 类型封装 /template/ 路由。
        // 因此这里既断言生成 Style 类型与关键 Setter 值存在，也断言不存在模板穿透与 PART_* 依赖。
        splashSource.ShouldContain("<atom:SplashTitleStyle x:SetterTargetType=\"TextBlock\">");
        splashSource.ShouldContain("<atom:SplashSubtitleStyle x:SetterTargetType=\"TextBlock\">");
        splashSource.ShouldContain("<atom:SplashDetailStyle x:SetterTargetType=\"TextBlock\">");
        splashSource.ShouldContain("<atom:SplashMessageStyle x:SetterTargetType=\"TextBlock\">");
        splashSource.ShouldNotContain("/template/");
        splashSource.ShouldNotContain("PART_TitleBlock");
        splashSource.ShouldNotContain("PART_MessageBlock");
        splashSource.ShouldNotContain("PART_SubtitleBlock");
        splashSource.ShouldNotContain("PART_DetailBlock");
        splashSource.ShouldContain("atom|Splash.gallery-window-splash:loading");
        splashSource.ShouldContain("#0B1026");
        splashSource.ShouldContain("#1D39C4");
        splashSource.ShouldContain("#13C2C2");
        splashSource.ShouldContain("#F5F8FF");
        splashSource.ShouldContain("#D6E4FF");
        splashSource.ShouldNotContain("ControlTokenResourceKey");
        splashSource.ShouldNotContain("SplashTokenKind");
        splashSource.ShouldNotContain("gallery-window-splash:success");
        splashSource.ShouldNotContain("gallery-window-splash:error");
    }

    private static string ExtractShowCaseItemMarkup(string source, string titleMarker)
    {
        var titleIndex = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemEndMarker = "</gallery:ShowCaseItem>";

        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf(itemEndMarker, titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(itemStart);

        return source[itemStart..(itemEnd + itemEndMarker.Length)];
    }

    private static string ExtractHeaderTitleMarkup(string source)
    {
        const string headerStartMarker = "Text=\"Splash\"";
        const string headerEndMarker = "SplashShowCaseLangResource PageSubtitle";

        var titleIndex = source.IndexOf(headerStartMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);
        var headerStart = source.LastIndexOf("<Grid", titleIndex, StringComparison.Ordinal);
        headerStart.ShouldBeGreaterThanOrEqualTo(0);
        var headerEnd = source.IndexOf(headerEndMarker, headerStart, StringComparison.Ordinal);
        headerEnd.ShouldBeGreaterThan(headerStart);

        return source[headerStart..headerEnd];
    }

    private static IEnumerable<GalleryNavigationNode> Walk(IEnumerable<GalleryNavigationNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Walk(node.Children))
            {
                yield return child;
            }
        }
    }

    private static string ReadRepoFile(string relativePath)
    {
        var path = GetRepoFile(relativePath);
        File.Exists(path).ShouldBeTrue($"Expected repository file to exist: {relativePath}");
        return File.ReadAllText(path);
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

        return Path.Combine(AppContext.BaseDirectory, relativePath);
    }
}
