using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class ModalShowCasePageTests
{
    [Fact]
    public void Modal_ShowCase_Uses_Document_Layout_With_Examples()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Modal/Views/ModalShowCase.axaml");

        source.ShouldContain("ModalShowCaseLangResource PageSubtitle");
        source.ShouldContain("ModalShowCaseLangResource PageDescription");
        source.ShouldNotContain("ModalShowCaseLangResource InfoNamespaceLabel");
        source.ShouldNotContain("ModalShowCaseLangResource InfoPackageLabel");
        source.ShouldNotContain("ModalShowCaseLangResource InfoBaseClassLabel");
        source.ShouldContain("ModalShowCaseLangResource ComponentCategory");
        source.ShouldContain("ModalShowCaseLangResource ComponentStatusStable");
        source.ShouldNotContain("ModalShowCaseLangResource ScenarioExamples");
        source.ShouldNotContain("ModalShowCaseLangResource ScenarioApi");
        source.ShouldNotContain("ModalShowCaseLangResource ScenarioDesignToken");
        source.ShouldNotContain("Tag=\"Examples\"");
        source.ShouldNotContain("Tag=\"Api\"");
        source.ShouldNotContain("Tag=\"DesignToken\"");
        source.ShouldContain("<gallery:GalleryShowCaseHost");
        source.ShouldNotContain("<gallery:GalleryStickyTabsHost");
        source.ShouldContain("StickyContentPadding=\"28,0,28,0\"");
        source.ShouldNotContain("<atom:TabStrip Name=\"ScenarioTabs\"");
        source.ShouldNotContain("<ContentControl Name=\"ScenarioContentHost\">");
        source.ShouldContain("Name=\"ExamplesContent\"");
        source.ShouldContain("IsScrollEnabled=\"False\"");
        source.ShouldContain("IsDeferredLoadingEnabled=\"True\"");
        source.ShouldContain("InitialDeferredLoadItemCount=\"4\"");
        source.ShouldContain("DeferredLoadBatchSize=\"2\"");
        source.ShouldContain("ContentMargin=\"28,10,28,28\"");
        CountShowCaseItemElements(source).ShouldBe(10);
        CountOccurrences(source, "IsDeferredContentEnabled=\"True\"").ShouldBe(10);
        CountOccurrences(source, "<gallery:ShowCaseItem.DeferredContentTemplate>").ShouldBe(10);
        // 10 个示例 DataTemplate + 1 个语义预览 DataTemplate。
        CountOccurrences(source, "DataTemplate x:DataType=\"viewModels:ModalViewModel\"").ShouldBe(11);
        source.ShouldContain("ModalShowCaseLangResource BasicTitle");
        source.ShouldContain("ModalShowCaseLangResource MessageBoxStyleTitle");
        source.ShouldContain("ModalShowCaseLangResource StaticDialogApiTitle");
        source.ShouldContain("ModalShowCaseLangResource P2ContentOpenBeforeCloseDialog");
        // 所有示例由 Button 触发，不使用开关切换。
        source.ShouldNotContain("ToggleSwitch");
        source.ShouldContain("MessageBoxOverlayHostButton");
        source.ShouldContain("MessageBoxWindowHostButton");
        source.ShouldContain("SemanticStyleDialogOpenButton");
        CountOccurrences(source, "HostMaxWidth=\"").ShouldBeGreaterThanOrEqualTo(2);
        CountOccurrences(source, "HostMaxHeight=\"").ShouldBeGreaterThanOrEqualTo(2);
        // 示例区不使用显式 PlacementTarget：只有语义预览把 stage 作为 placement target。
        ExtractModalExampleItems(source).ShouldNotContain("PlacementTarget=\"");
        source.ShouldNotContain("<atom:TabControl");
        source.ShouldNotContain("<atom:TabItem");
        source.ShouldNotContain("<atom:DataGrid");
        source.ShouldNotContain(">Gallery<");
    }

    [Fact]
    public void Modal_ShowCase_Semantic_Preview_Follows_The_Pinned_Overlay_Pattern()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Modal/Views/ModalShowCase.axaml");

        source.ShouldContain("GalleryShowCaseHost.SemanticPartsContentTemplate");

        // 语义页签并列两个预览：Dialog 与 MessageBox。
        var dialogPreview     = ExtractSemanticPreview(source, "ModalSemanticPreview");
        var messageBoxPreview = ExtractSemanticPreview(source, "MessageBoxSemanticPreview");

        AssertPinnedOverlayPreview(dialogPreview, "atom:Dialog", "ModalSemanticStage",
            "ModalSemanticStageLayer", "ModalSemanticOwner");
        AssertPinnedOverlayPreview(messageBoxPreview, "atom:MessageBox", "MessageBoxSemanticStage",
            "MessageBoxSemanticStageLayer", "MessageBoxSemanticOwner");

        // 两个预览的 PartDescriptions 顺序都严格对齐 antd Modal _semantic.tsx。
        foreach (var preview in new[] { dialogPreview, messageBoxPreview })
        {
            var paths = Regex.Matches(preview, "SemanticPartDescription Path=\"([^\"]+)\"")
                             .Select(static m => m.Groups[1].Value).ToArray();
            paths.ShouldBe(["root", "mask", "container", "wrapper", "header", "title", "body", "footer", "close"]);
        }

        // 跨根经 ISemanticPartCrossRootProvider 上报，不允许出现 code-behind 根注册。
        source.ShouldNotContain("HandleSemanticPreviewLoaded");
        source.ShouldNotContain("HandleSemanticPreviewUnloaded");

        // SemanticStyles 示例必须用生成 Style 类定制（专用 Style 唯一定制入口）。
        var semanticStylesItem = ExtractShowCaseItem(source, "SemanticStylesTitle");
        semanticStylesItem.ShouldContain("atom:DialogMaskStyle");
        semanticStylesItem.ShouldContain("atom:DialogContainerStyle");
        semanticStylesItem.ShouldContain("atom:DialogHeaderStyle");
        semanticStylesItem.ShouldContain("atom:DialogTitleStyle");
        semanticStylesItem.ShouldContain("atom:DialogBodyStyle");
        semanticStylesItem.ShouldContain("atom:DialogFooterStyle");
        semanticStylesItem.ShouldContain("atom:DialogCloseStyle");
        // title 是 Avalonia TextBlock、close 是 Avalonia Button，x:SetterTargetType 不得带 atom: 前缀。
        semanticStylesItem.ShouldContain("x:SetterTargetType=\"TextBlock\"");
        semanticStylesItem.ShouldContain("x:SetterTargetType=\"Button\"");
        // 禁止以事件处理器为特征的代码回退。
        semanticStylesItem.ShouldNotContain(".Loaded=");
        semanticStylesItem.ShouldNotContain(".Unloaded=");
    }

    [Fact]
    public void Modal_ShowCase_MessageBox_Semantic_Styling_Is_Merged_Into_The_Styling_Example()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Modal/Views/ModalShowCase.axaml");

        // MessageBox 语义样式示例必须合并进「自定义语义结构的样式」条目，而不是独立成卡。
        source.ShouldNotContain("MessageBoxSemanticStylesTitle");
        var item = ExtractShowCaseItem(source, "SemanticStylesTitle");

        // 同一个条目同时提供 Dialog 与 MessageBox 两个触发按钮。
        item.ShouldContain("Name=\"SemanticStyleDialogOpenButton\"");
        item.ShouldContain("Name=\"MessageBoxSemanticStyleOpenButton\"");
        CountOccurrences(item, "<atom:Button").ShouldBe(2);
        item.ShouldNotContain("ToggleSwitch");

        // Dialog 部分用 Dialog<Part>Style。
        item.ShouldContain("atom:DialogMaskStyle");
        item.ShouldContain("atom:DialogCloseStyle");

        // MessageBox 部分必须用 MessageBox<Part>Style 与独立的 owner 选择器。
        item.ShouldContain("Classes=\"messagebox-semantic-styles-demo\"");
        item.ShouldContain("Style Selector=\"atom|MessageBox.messagebox-semantic-styles-demo\"");
        item.ShouldContain("atom:MessageBoxMaskStyle");
        item.ShouldContain("atom:MessageBoxContainerStyle");
        item.ShouldContain("atom:MessageBoxHeaderStyle");
        item.ShouldContain("atom:MessageBoxTitleStyle");
        item.ShouldContain("atom:MessageBoxBodyStyle");
        item.ShouldContain("atom:MessageBoxFooterStyle");
        item.ShouldContain("atom:MessageBoxCloseStyle");

        // 禁止以事件处理器为特征的代码回退。
        item.ShouldNotContain(".Loaded=");
        item.ShouldNotContain(".Unloaded=");
    }

    [Fact]
    public void Modal_ShowCase_Examples_Match_Approved_Control_Demo_Content()
    {
        var source   = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Modal/Views/ModalShowCase.axaml");
        var approved = ReadRepoFile("tests/AtomUIGallery.Tests/ShowCases/ModalShowCaseExamples.snapshot");

        var normalized = NormalizeMarkup(ExtractModalExampleItems(source));
        CountShowCaseItemElements(normalized).ShouldBe(ReadSnapshotCount(approved));
        ComputeSha256(normalized).ShouldBe(ReadSnapshotHash(approved));
    }

    private static string ExtractModalExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    // 对齐上游 getContainer={false} 的内联模态：舞台自带 ScopeAwareOverlayLayerPanel，
    // owner 经 OverlayScope 把 overlay 宿主限定在舞台作用域内，mask 随舞台而不是铺满窗口；
    // 常开由 IsPinnedOpen 钉住，StandardButtons 提供真实按钮序列以呈现 footer。
    private static void AssertPinnedOverlayPreview(
        string preview,
        string ownerType,
        string stageName,
        string stageLayerName,
        string ownerName)
    {
        preview.ShouldContain($"SemanticOwnerType=\"{{x:Type {ownerType}}}\"");
        preview.ShouldContain($"SemanticOwner=\"{{Binding #{ownerName}}}\"");
        preview.ShouldContain($"Name=\"{stageName}\"");
        preview.ShouldContain($"Name=\"{stageLayerName}\"");
        preview.ShouldContain($"<atom:ScopeAwareOverlayLayerPanel Name=\"{stageLayerName}\"");
        preview.ShouldContain($"OverlayScope=\"{{Binding #{stageLayerName}}}\"");
        preview.ShouldContain($"PlacementTarget=\"{{Binding #{stageLayerName}}}\"");
        preview.ShouldContain("IsOpen=\"True\"");
        preview.ShouldContain("IsPinnedOpen=\"True\"");
        preview.ShouldContain("IsMotionEnabled=\"False\"");
        preview.ShouldContain("IsModal=\"True\"");
        preview.ShouldContain("DialogHostType=\"Overlay\"");
        preview.ShouldContain("StandardButtons=\"Cancel,Ok\"");
    }

    private static string ExtractSemanticPreview(string source, string previewName)
    {
        const string closingMarker = "</gallery:SemanticPartPreview>";

        var startMarker = $"<gallery:SemanticPartPreview Name=\"{previewName}\"";
        var start       = source.IndexOf(startMarker, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0, $"missing SemanticPartPreview '{previewName}'");

        var end = source.IndexOf(closingMarker, start, StringComparison.Ordinal);
        end.ShouldBeGreaterThan(start);

        return source[start..(end + closingMarker.Length)];
    }

    private static string ExtractShowCaseItem(string source, string titleResourceName)
    {
        var titleMarker = $"ModalShowCaseLangResource {titleResourceName}";
        var titleIndex  = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf("</gallery:ShowCaseItem>", titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemEnd + "</gallery:ShowCaseItem>".Length)];
    }

    private static string NormalizeMarkup(string source)
    {
        return ShowCaseSnapshotMarkup.Normalize(source);
    }

    private static string ComputeSha256(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string ReadSnapshotHash(string source)
    {
        return source
            .Split('\n')
            .First(line => line.StartsWith("sha256:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim();
    }

    private static int ReadSnapshotCount(string source)
    {
        return int.Parse(source
            .Split('\n')
            .First(line => line.StartsWith("count:", StringComparison.Ordinal))
            .Split(':', 2)[1]
            .Trim());
    }

    private static int CountShowCaseItemElements(string source)
    {
        return Regex.Matches(source, @"<gallery:ShowCaseItem(\s|>)", RegexOptions.CultureInvariant).Count;
    }

    private static int CountOccurrences(string source, string value)
    {
        var count      = 0;
        var startIndex = 0;
        while (true)
        {
            var matchIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (matchIndex < 0)
            {
                return count;
            }

            count++;
            startIndex = matchIndex + value.Length;
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
