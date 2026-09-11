using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Message;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;
using DesktopButton = AtomUI.Desktop.Controls.Button;
using DesktopNumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using DesktopRibbonBadge = AtomUI.Desktop.Controls.RibbonBadge;
using DesktopToggleSwitch = AtomUI.Desktop.Controls.ToggleSwitch;

namespace AtomUIGallery.Tests.ShowCases;

public class MessageShowCasePageTests
{
    [Fact]
    public void Message_ShowCase_Uses_Document_Layout_With_Examples()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml");

        source.ShouldContain("MessageShowCaseLangResource PageSubtitle");
        source.ShouldContain("MessageShowCaseLangResource PageDescription");
        source.ShouldNotContain("MessageShowCaseLangResource InfoNamespaceLabel");
        source.ShouldNotContain("MessageShowCaseLangResource InfoPackageLabel");
        source.ShouldNotContain("MessageShowCaseLangResource InfoBaseClassLabel");
        source.ShouldContain("MessageShowCaseLangResource ComponentCategory");
        source.ShouldContain("MessageShowCaseLangResource ComponentStatusStable");
        source.ShouldNotContain("MessageShowCaseLangResource ScenarioExamples");
        source.ShouldNotContain("MessageShowCaseLangResource ScenarioApi");
        source.ShouldNotContain("MessageShowCaseLangResource ScenarioDesignToken");
        source.ShouldNotContain("Tag=\"Examples\"");
        source.ShouldNotContain("Tag=\"Api\"");
        source.ShouldNotContain("Tag=\"DesignToken\"");
        source.ShouldContain("<gallery:GalleryShowCaseHost");
        source.ShouldContain("StickyContentPadding=\"28,0,28,0\"");
        source.ShouldNotContain("<atom:TabStrip Name=\"ScenarioTabs\"");
        source.ShouldNotContain("<ContentControl Name=\"ScenarioContentHost\">");
        source.ShouldContain("Name=\"ExamplesContent\"");
        source.ShouldContain("IsScrollEnabled=\"False\"");
        source.ShouldContain("IsDeferredLoadingEnabled=\"True\"");
        source.ShouldContain("InitialDeferredLoadItemCount=\"4\"");
        source.ShouldContain("DeferredLoadBatchSize=\"2\"");
        source.ShouldContain("ContentMargin=\"28,10,28,28\"");
        CountShowCaseItemElements(source).ShouldBe(6);
        CountOccurrences(source, "IsDeferredContentEnabled=\"True\"").ShouldBe(6);
        CountOccurrences(source, "<gallery:ShowCaseItem.DeferredContentTemplate>").ShouldBe(6);
        CountOccurrences(source, "DataTemplate x:DataType=\"vm:MessageViewModel\"").ShouldBe(6);
        source.ShouldContain("MessageShowCaseLangResource BasicTitle");
        source.ShouldContain("MessageShowCaseLangResource OtherTypesTitle");
        source.ShouldContain("MessageShowCaseLangResource LoadingIndicatorTitle");
        source.ShouldContain("MessageShowCaseLangResource CallbackTitle");
        source.ShouldContain("MessageShowCaseLangResource StackTitle");
        source.ShouldContain("SourceKey=\"message-stack\"");
        source.ShouldContain("MessageShowCaseLangResource SemanticPartStyleTitle");
        source.ShouldNotContain("<atom:TabControl");
        source.ShouldNotContain("<atom:TabItem");
        source.ShouldNotContain("<atom:DataGrid");
        source.ShouldNotContain(">Gallery<");
    }

    [Fact]
    public void Message_ShowCase_Examples_Match_Approved_Control_Demo_Content()
    {
        var source   = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml");
        var approved = ReadRepoFile("tests/AtomUIGallery.Tests/ShowCases/MessageShowCaseExamples.snapshot");

        var normalized = NormalizeMarkup(ExtractMessageExampleItems(source));
        CountShowCaseItemElements(normalized).ShouldBe(ReadSnapshotCount(approved));
        ComputeSha256(normalized).ShouldBe(ReadSnapshotHash(approved));
    }

    [Fact]
    public void Message_Stack_ShowCase_Isolates_Configuration_Lifetime_And_Destroy_Scope()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new MessageShowCase();

        ShowInWindow(page, window =>
        {
            var items = page.GetVisualDescendants().OfType<ShowCaseItem>().ToArray();
            var stackItem = items.Single(item => item.SourceKey == "message-stack");
            stackItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var enabledSwitch = stackItem.GetVisualDescendants()
                                         .OfType<DesktopToggleSwitch>()
                                         .Single(control => control.Name == "StackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "StackThresholdInput");
            var openButton = stackItem.GetVisualDescendants()
                                      .OfType<DesktopButton>()
                                      .Single(control => control.Name == "OpenStackMessageButton");
            var destroyButton = stackItem.GetVisualDescendants()
                                         .OfType<DesktopButton>()
                                         .Single(control => control.Name == "DestroyStackMessagesButton");

            enabledSwitch.IsChecked.ShouldBe(true);
            thresholdInput.Value.ShouldBe(3m);
            thresholdInput.Minimum.ShouldBe(1m);
            thresholdInput.Maximum.ShouldBe(10m);
            thresholdInput.IsEnabled.ShouldBeTrue();

            RaiseClick(destroyButton);
            window.GetVisualDescendants().OfType<WindowMessageManager>().ShouldBeEmpty();

            RaiseClick(openButton);
            RaiseClick(openButton);
            window.UpdateLayout();

            var stackManager = window.GetVisualDescendants().OfType<WindowMessageManager>().Single();
            stackManager.IsStackEnabled.ShouldBeTrue();
            stackManager.StackThreshold.ShouldBe(3);
            stackManager.GetVisualDescendants()
                        .OfType<MessageCard>()
                        .Select(card => card.Message)
                        .ShouldBe([
                            "Message 1: This is a stacked message.",
                            "Message 2: This is a slightly longer stacked message."
                        ]);

            thresholdInput.Value = 5m;
            stackManager.StackThreshold.ShouldBe(5);

            enabledSwitch.IsChecked = false;
            stackManager.IsStackEnabled.ShouldBeFalse();
            thresholdInput.IsEnabled.ShouldBeFalse();

            enabledSwitch.IsChecked = true;
            stackManager.IsStackEnabled.ShouldBeTrue();
            thresholdInput.IsEnabled.ShouldBeTrue();

            var basicItem = items[0];
            var normalButton = basicItem.GetVisualDescendants().OfType<DesktopButton>().Single();
            RaiseClick(normalButton);
            window.UpdateLayout();

            var managers = window.GetVisualDescendants().OfType<WindowMessageManager>().ToArray();
            managers.Length.ShouldBe(2);
            var defaultManager = managers.Single(manager => !ReferenceEquals(manager, stackManager));
            defaultManager.IsStackEnabled.ShouldBeFalse();
            defaultManager.GetVisualDescendants().OfType<MessageCard>().Count().ShouldBe(1);

            stackManager.IsMotionEnabled = false;
            RaiseClick(destroyButton);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            stackManager.GetVisualDescendants().OfType<MessageCard>().ShouldBeEmpty();
            defaultManager.GetVisualDescendants().OfType<MessageCard>().Count().ShouldBe(1);

            RaiseClick(openButton);
            window.UpdateLayout();
            stackManager.GetVisualDescendants()
                        .OfType<MessageCard>()
                        .Single()
                        .Message.ShouldBe("Message 3: This is a stacked message.");

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            foreach (var manager in managers)
            {
                manager.GetVisualParent().ShouldBeNull();
            }
        });
    }

    [Fact]
    public void Message_Stack_ShowCase_Localization_Is_Complete()
    {
        foreach (var language in new[] { "en-US", "zh-CN", "zh-TW", "pt-BR" })
        {
            var localization = XliffTestDocument.Read(
                $"controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Localization/{language}.xlf");

            foreach (var key in new[]
                     {
                         "StackTitle",
                         "StackDescription",
                         "StackEnabledLabel",
                         "StackThresholdLabel",
                         "P2ContentOpenMessageBox",
                         "P2ContentDestroyAll",
                         "P2MessageStackedFormat",
                         "P2MessageLongStackedFormat"
                     })
            {
                localization.ContainsKey(key).ShouldBeTrue($"Missing {key} in {language}.");
            }
        }
    }

    [Fact]
    public void Message_Stack_ShowCase_Configuration_Row_Uses_One_Vertical_Center_Line()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new MessageShowCase();

        ShowInWindow(page, window =>
        {
            var stackItem = page.GetVisualDescendants()
                                .OfType<ShowCaseItem>()
                                .Single(item => item.SourceKey == "message-stack");
            stackItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var enabledLabel = stackItem.GetVisualDescendants()
                                        .OfType<AvaloniaTextBlock>()
                                        .Single(control => control.Text == "Enabled:");
            var thresholdLabel = stackItem.GetVisualDescendants()
                                          .OfType<AvaloniaTextBlock>()
                                          .Single(control => control.Text == "Threshold:");
            var enabledSwitch = stackItem.GetVisualDescendants()
                                         .OfType<DesktopToggleSwitch>()
                                         .Single(control => control.Name == "StackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "StackThresholdInput");

            var expectedCenterY = GetCenterY(thresholdInput, stackItem);
            GetCenterY(enabledLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(enabledSwitch, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(thresholdLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
        });
    }

    [Fact]
    public void Message_Stack_ShowCase_Displays_The_V619_Version_Badge()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new MessageShowCase();

        ShowInWindow(page, window =>
        {
            var stackItem = page.GetVisualDescendants()
                                .OfType<ShowCaseItem>()
                                .Single(item => item.SourceKey == "message-stack");
            window.UpdateLayout();

            stackItem.BadgeText.ShouldBe("v6.1.9");
            stackItem.GetVisualDescendants()
                     .OfType<DesktopRibbonBadge>()
                     .Single()
                     .Text.ShouldBe("v6.1.9");
        });
    }

    [Fact]
    public void Message_ShowCase_Semantic_Parts_Follow_The_Two_Owner_Pattern()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml");
        var codeBehind = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml.cs");

        source.ShouldContain("GalleryShowCaseHost.SemanticPartsContentTemplate");
        CountOccurrences(source, "<gallery:SemanticPartPreview ").ShouldBe(2);
        source.ShouldContain("<gallery:SemanticPartPreview Name=\"MessageManagerSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:WindowMessageManager}\"");
        source.ShouldContain("<gallery:SemanticPartPreview Name=\"MessageCardSemanticPreview\"");
        source.ShouldContain("SemanticOwnerType=\"{x:Type atom:MessageCard}\"");

        // manager 有两个 public 构造（含无参），因此预览可声明式实例化；manager 没有声明式消息入口，
        // 示例数据由 owner 的 Loaded 事件调用 Show() 播种。
        source.ShouldContain("<atom:WindowMessageManager Name=\"MessageManagerSemanticOwner\"");
        source.ShouldContain("Loaded=\"HandleSemanticPreviewOwnerLoaded\"");
        codeBehind.ShouldContain("HandleSemanticPreviewOwnerLoaded");
        // 舞台根是 Border 而非 owner 类型，必须显式绑定 SemanticOwner，否则 owner 兼容性校验会拒绝。
        source.ShouldContain("SemanticOwner=\"{Binding #MessageManagerSemanticOwner}\"");
        source.ShouldContain("SemanticOwner=\"{Binding #MessageCardSemanticOwner}\"");

        // 两个 owner 各自独立 descriptor，PartDescriptions 顺序与 semantic-part.md / descriptor 一致。
        var descriptions = Regex.Matches(source, "SemanticPartDescription Path=\"([^\"]+)\"")
                                .Select(static match => match.Groups[1].Value)
                                .ToArray();
        descriptions.ShouldBe(["root", "listContent", "root", "wrapper", "icon", "title"]);

        // SemanticStyles 示例必须用生成的专用 Style 类定制（专用 Style 唯一定制入口）。
        var semanticStylesItem = ExtractShowCaseItem(source, "SemanticPartStyleTitle");
        semanticStylesItem.ShouldContain("atom:MessageCardWrapperStyle");
        semanticStylesItem.ShouldContain("atom:MessageCardIconStyle");
        semanticStylesItem.ShouldContain("atom:MessageCardTitleStyle");
        semanticStylesItem.ShouldContain("x:SetterTargetType=\"DockPanel\"");
        semanticStylesItem.ShouldContain("x:SetterTargetType=\"atom:IconPresenter\"");
        semanticStylesItem.ShouldContain("x:SetterTargetType=\"SelectableTextBlock\"");
        // 部件样式必须是声明式专用 Style，不得以事件处理器做代码回退。
        semanticStylesItem.ShouldNotContain(".Loaded=");
        semanticStylesItem.ShouldNotContain(".Unloaded=");
        semanticStylesItem.ShouldNotContain("/template/ .semantic-");

        // 语义预览不得用代码回退定制部件：不得按 Name 查找模板节点后设置部件属性，也不得注册
        // AdditionalRoots（manager 内联在舞台视觉树内，无需跨根注册）。
        source.ShouldNotContain("/template/ .semantic-");
        codeBehind.ShouldNotContain("AdditionalRoots");
        codeBehind.ShouldNotContain("semantic-wrapper");
        codeBehind.ShouldNotContain("semantic-icon");
        codeBehind.ShouldNotContain("semantic-title");
        codeBehind.ShouldNotContain("semantic-list-content");
    }

    private static string ExtractShowCaseItem(string source, string titleResourceName)
    {
        var titleMarker = $"MessageShowCaseLangResource {titleResourceName}";
        var titleIndex  = source.IndexOf(titleMarker, StringComparison.Ordinal);
        titleIndex.ShouldBeGreaterThanOrEqualTo(0);

        var itemStart = source.LastIndexOf("<gallery:ShowCaseItem", titleIndex, StringComparison.Ordinal);
        itemStart.ShouldBeGreaterThanOrEqualTo(0);

        var itemEnd = source.IndexOf("</gallery:ShowCaseItem>", titleIndex, StringComparison.Ordinal);
        itemEnd.ShouldBeGreaterThan(titleIndex);

        return source[itemStart..(itemEnd + "</gallery:ShowCaseItem>".Length)];
    }

    private static string ExtractMessageExampleItems(string source)
    {
        const string firstItemMarker  = "<gallery:ShowCaseItem";
        const string panelCloseMarker = "</gallery:ShowCasePanel>";

        var firstItemStart = source.IndexOf(firstItemMarker, StringComparison.Ordinal);
        firstItemStart.ShouldBeGreaterThanOrEqualTo(0);

        var panelCloseStart = source.IndexOf(panelCloseMarker, firstItemStart, StringComparison.Ordinal);
        panelCloseStart.ShouldBeGreaterThan(firstItemStart);

        return source[firstItemStart..panelCloseStart];
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

    private static double GetCenterY(Control control, Visual relativeTo)
    {
        var center = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            relativeTo);
        center.ShouldNotBeNull();
        return center.Value.Y;
    }

    private static void RaiseClick(DesktopButton button)
    {
        button.RaiseEvent(new RoutedEventArgs(DesktopButton.ClickEvent, button));
        Dispatcher.UIThread.RunJobs();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var visualLayerManager = new VisualLayerManager
        {
            EnableAdornerLayer = true,
            Child              = content
        };
        var window = new AvaloniaWindow
        {
            Width   = 1000,
            Height  = 900,
            Content = visualLayerManager
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        try
        {
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
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
