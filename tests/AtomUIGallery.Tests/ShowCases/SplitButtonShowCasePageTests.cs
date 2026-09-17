using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AtomUIGallery.Tests.ShowCases;

public class SplitButtonShowCasePageTests
{
    [Fact]
    public void SplitButton_ShowCase_Uses_Document_Layout_With_Examples()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/SplitButton/Views/SplitButtonShowCase.axaml");

        source.ShouldContain("SplitButtonShowCaseLangResource PageSubtitle");
        source.ShouldContain("SplitButtonShowCaseLangResource PageDescription");
        source.ShouldNotContain("SplitButtonShowCaseLangResource InfoNamespaceLabel");
        source.ShouldNotContain("SplitButtonShowCaseLangResource InfoPackageLabel");
        source.ShouldNotContain("SplitButtonShowCaseLangResource InfoBaseClassLabel");
        source.ShouldContain("SplitButtonShowCaseLangResource ComponentCategory");
        source.ShouldContain("SplitButtonShowCaseLangResource ComponentStatusStable");
        source.ShouldNotContain("SplitButtonShowCaseLangResource ScenarioExamples");
        source.ShouldNotContain("SplitButtonShowCaseLangResource ScenarioApi");
        source.ShouldNotContain("SplitButtonShowCaseLangResource ScenarioDesignToken");
        source.ShouldNotContain("Tag=\"Examples\"");
        source.ShouldNotContain("Tag=\"Api\"");
        source.ShouldNotContain("Tag=\"DesignToken\"");
        source.ShouldContain("<gallery:GalleryShowCaseHost");
        source.ShouldContain("gallery:GalleryShowCaseHost.SemanticPartsContentTemplate");
        source.ShouldContain("StickyContentPadding=\"28,0,28,0\"");
        source.ShouldNotContain("<atom:TabStrip Name=\"ScenarioTabs\"");
        source.ShouldNotContain("<ContentControl Name=\"ScenarioContentHost\">");
        source.ShouldContain("Name=\"ExamplesContent\"");
        source.ShouldContain("IsScrollEnabled=\"False\"");
        source.ShouldContain("IsDeferredLoadingEnabled=\"True\"");
        source.ShouldContain("InitialDeferredLoadItemCount=\"4\"");
        source.ShouldContain("DeferredLoadBatchSize=\"2\"");
        source.ShouldContain("ContentMargin=\"28,10,28,28\"");
        source.ShouldNotContain("Selector=\"atom|TextBlock.info-label\"");
        source.ShouldNotContain("Selector=\"atom|TextBlock.info-value\"");
        CountOccurrences(source, "Classes=\"info-label\"").ShouldBe(0);
        CountOccurrences(source, "Classes=\"info-value\"").ShouldBe(0);
        source.ShouldNotContain("LineHeight=\"22\"");
        source.ShouldContain("Description=\"{gallery:SplitButtonShowCaseLangResource PageDescription}\"");
        CountShowCaseItemElements(source).ShouldBe(6);
        CountOccurrences(source, "IsDeferredContentEnabled=\"True\"").ShouldBe(6);
        CountOccurrences(source, "<gallery:ShowCaseItem.DeferredContentTemplate>").ShouldBe(6);
        // 6 个示例模板 + 1 个语义部件预览模板
        CountOccurrences(source, "DataTemplate x:DataType=\"vm:SplitButtonViewModel\"").ShouldBe(7);
        CountOccurrences(source, "<gallery:ShowCaseItem.Styles>").ShouldBe(4);
        source.ShouldContain("SplitButtonShowCaseLangResource BasicTitle");
        source.ShouldContain("SplitButtonShowCaseLangResource SizeTitle");
        source.ShouldContain("SplitButtonShowCaseLangResource FlyoutTriggerTypeTitle");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticStylesTitle");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticStylesDescription");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticStylesObjectButton");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticStylesObjectPrimary");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticStylesItemProfile");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticStylesItemSettings");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticStylesItemLogout");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:SplitButton}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #SplitButtonSemanticOwner}\"");
        source.ShouldContain("IsPopupPinnedOpen=\"True\"");
        // 弹层（含级联子菜单）必须完整落在预览舞台内：锚点靠上并给足舞台高度，
        // 否则菜单会溢出舞台、在紧凑布局下压到部件卡。
        source.ShouldContain("PreviewContentAlignment=\"Top\"");
        source.ShouldContain("PreviewStageMinHeight=\"420\"");
        // 预览锚点水平居中（同 MenuShowCase 的弹出型预览）：贴左会让按钮与级联菜单整体偏向舞台左侧。
        source.ShouldContain("HorizontalAlignment=\"Center\"");
        source.ShouldContain("IsSubMenuOpen=\"True\"");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPreviewGroupTitle");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPreviewFirstMenuItem");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPreviewSecondMenuItem");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPreviewSubMenu");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPreviewOption1");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPreviewOption2");
        source.ShouldContain("Kind=SaveOutlined");
        source.ShouldContain("Kind=EditOutlined");
        source.ShouldContain("Kind=DeleteOutlined");
        // 语义部件定制必须使用生成的专用 Style 类，禁止代码回退
        source.ShouldContain("<atom:SplitButtonPopupRootStyle x:SetterTargetType=\"atom:ArrowDecoratedBox\">");
        source.ShouldContain("<atom:SplitButtonItemStyle x:SetterTargetType=\"atom:MenuItem\">");
        source.ShouldContain("<atom:SplitButtonItemIconStyle x:SetterTargetType=\"atom:IconPresenter\">");
        source.ShouldContain("<atom:SplitButtonItemContentStyle x:SetterTargetType=\"ContentPresenter\">");
        source.ShouldContain("<atom:SplitButtonSecondaryStyle x:SetterTargetType=\"atom:Button\">");
        source.ShouldContain("Value=\"#d9d9d9\"");
        source.ShouldContain("Value=\"#1890ff\"");
        source.ShouldContain("Foreground=\"{atom:SharedTokenResource ColorError}\"");
        source.ShouldContain("<atom:MenuSeparator />");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPrimaryDescription");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticSecondaryDescription");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticPopupRootDescription");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticItemTitleDescription");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticItemDescription");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticItemContentDescription");
        source.ShouldContain("SplitButtonShowCaseLangResource SemanticItemIconDescription");
        source.ShouldContain("Path=\"primary\"");
        source.ShouldContain("Path=\"secondary\"");
        source.IndexOf("Path=\"popup.root\"", StringComparison.Ordinal)
            .ShouldBeLessThan(source.IndexOf("Path=\"itemTitle\"", StringComparison.Ordinal));
        source.IndexOf("Path=\"itemTitle\"", StringComparison.Ordinal)
            .ShouldBeLessThan(source.IndexOf("Path=\"item\"", StringComparison.Ordinal));
        source.IndexOf("Path=\"item\"", StringComparison.Ordinal)
            .ShouldBeLessThan(source.IndexOf("Path=\"itemContent\"", StringComparison.Ordinal));
        source.IndexOf("Path=\"itemContent\"", StringComparison.Ordinal)
            .ShouldBeLessThan(source.IndexOf("Path=\"itemIcon\"", StringComparison.Ordinal));
        // 弹层根注册处理器（AdditionalRoots）是跨视觉根弹层语义预览的仓库既定模式
        // （同 InfoFlyout）：仅允许挂在 SemanticPartPreview 上，其余元素禁止 Loaded/Unloaded。
        source.ShouldContain("Loaded=\"HandleSemanticPreviewLoaded\"");
        source.ShouldContain("Unloaded=\"HandleSemanticPreviewUnloaded\"");
        CountOccurrences(source, "Loaded=\"").ShouldBe(1);
        CountOccurrences(source, "Unloaded=\"").ShouldBe(1);
        source.ShouldNotContain("<atom:TabControl");
        source.ShouldNotContain("<atom:TabItem");
        source.ShouldNotContain("<atom:DataGrid");
        source.ShouldNotContain(">Gallery<");
    }

    [Fact]
    public void SplitButton_Size_Example_Includes_Custom_Size_Demo()
    {
        var source   = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/SplitButton/Views/SplitButtonShowCase.axaml");
        var examples = ExtractSplitButtonExampleItems(source);

        var sizeItem = ExtractShowCaseItemByTitle(examples, "SplitButtonShowCaseLangResource SizeTitle");
        sizeItem.ShouldContain("SizeType=\"Custom\"");
        sizeItem.ShouldContain("Height=\"44\"");
        sizeItem.ShouldContain("Padding=\"18,0\"");
        sizeItem.ShouldContain("FontSize=\"15\"");
        sizeItem.ShouldContain("SplitButtonShowCaseLangResource P2ContentCustom");
    }

    [Fact]
    public void SplitButton_ShowCase_Examples_Match_Approved_Control_Demo_Content()
    {
        var source   = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/General/SplitButton/Views/SplitButtonShowCase.axaml");
        var approved = ReadRepoFile("tests/AtomUIGallery.Tests/ShowCases/SplitButtonShowCaseExamples.snapshot");

        var normalized = NormalizeMarkup(ExtractSplitButtonExampleItems(source));
        CountShowCaseItemElements(normalized).ShouldBe(ReadSnapshotCount(approved));
        ComputeSha256(normalized).ShouldBe(ReadSnapshotHash(approved));
    }

    private static string ExtractSplitButtonExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
    }

    private static string ExtractShowCaseItemByTitle(string source, string titleResource)
    {
        const string itemStartMarker = "<gallery:ShowCaseItem";
        const string itemCloseMarker = "</gallery:ShowCaseItem>";

        var titleIndex = source.IndexOf(titleResource, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf(itemStartMarker, titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemClose = source.IndexOf(itemCloseMarker, titleIndex, StringComparison.Ordinal);
        itemClose.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemClose + itemCloseMarker.Length)];
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
