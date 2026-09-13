using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AtomUI.Desktop.Controls;
using AtomUI.Toolkits.GalleryBase.Controls;
using AtomUIGallery.ShowCases.Notification;
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

public class NotificationShowCasePageTests
{
    [Fact]
    public void Notification_ShowCase_Uses_Document_Layout_With_Examples()
    {
        var source = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml");

        source.ShouldContain("NotificationShowCaseLangResource PageSubtitle");
        source.ShouldContain("NotificationShowCaseLangResource PageDescription");
        source.ShouldNotContain("NotificationShowCaseLangResource InfoNamespaceLabel");
        source.ShouldNotContain("NotificationShowCaseLangResource InfoPackageLabel");
        source.ShouldNotContain("NotificationShowCaseLangResource InfoBaseClassLabel");
        source.ShouldContain("NotificationShowCaseLangResource ComponentCategory");
        source.ShouldContain("NotificationShowCaseLangResource ComponentStatusStable");
        source.ShouldNotContain("NotificationShowCaseLangResource ScenarioExamples");
        source.ShouldNotContain("NotificationShowCaseLangResource ScenarioApi");
        source.ShouldNotContain("NotificationShowCaseLangResource ScenarioDesignToken");
        source.ShouldNotContain("Tag=\"Examples\"");
        source.ShouldNotContain("Tag=\"Api\"");
        source.ShouldNotContain("Tag=\"DesignToken\"");
        source.ShouldContain("<gallery:GalleryStickyTabsHost");
        source.ShouldContain("StickyContentPadding=\"28,0,28,0\"");
        source.ShouldNotContain("<atom:TabStrip Name=\"ScenarioTabs\"");
        source.ShouldNotContain("<ContentControl Name=\"ScenarioContentHost\">");
        source.ShouldContain("Name=\"ExamplesContent\"");
        source.ShouldContain("IsScrollEnabled=\"False\"");
        source.ShouldContain("IsDeferredLoadingEnabled=\"True\"");
        source.ShouldContain("InitialDeferredLoadItemCount=\"4\"");
        source.ShouldContain("DeferredLoadBatchSize=\"2\"");
        source.ShouldContain("ContentMargin=\"28,10,28,28\"");
        CountShowCaseItemElements(source).ShouldBe(7);
        CountOccurrences(source, "IsDeferredContentEnabled=\"True\"").ShouldBe(7);
        CountOccurrences(source, "<gallery:ShowCaseItem.DeferredContentTemplate>").ShouldBe(7);
        CountOccurrences(source, "DataTemplate x:DataType=\"viewModels:NotificationViewModel\"").ShouldBe(7);
        source.ShouldContain("NotificationShowCaseLangResource BasicTitle");
        source.ShouldContain("NotificationShowCaseLangResource PlacementTitle");
        source.ShouldContain("NotificationShowCaseLangResource ProgressTitle");
        source.ShouldContain("NotificationShowCaseLangResource StackTitle");
        source.ShouldContain("SourceKey=\"notification-stack\"");
        source.ShouldNotContain("<atom:TabControl");
        source.ShouldNotContain("<atom:TabItem");
        source.ShouldNotContain("<atom:DataGrid");
        source.ShouldNotContain(">Gallery<");
    }

    [Fact]
    public void Notification_ShowCase_Examples_Match_Approved_Control_Demo_Content()
    {
        var source   = ReadRepoFile("controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml");
        var approved = ReadRepoFile("tests/AtomUIGallery.Tests/ShowCases/NotificationShowCaseExamples.snapshot");

        var normalized = NormalizeMarkup(ExtractNotificationExampleItems(source));
        CountShowCaseItemElements(normalized).ShouldBe(ReadSnapshotCount(approved));
        ComputeSha256(normalized).ShouldBe(ReadSnapshotHash(approved));
    }

    [Fact]
    public void Notification_Stack_ShowCase_Isolates_Configuration_Lifetime_And_Destroy_Scope()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new NotificationShowCase();

        ShowInWindow(page, window =>
        {
            var items = page.GetVisualDescendants().OfType<ShowCaseItem>().ToArray();
            var stackItem = items.Single(item => item.SourceKey == "notification-stack");
            stackItem.MaterializeDeferredContent();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var enabledSwitch = stackItem.GetVisualDescendants()
                                         .OfType<DesktopToggleSwitch>()
                                         .Single(control => control.Name == "NotificationStackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "NotificationStackThresholdInput");
            var openButton = stackItem.GetVisualDescendants()
                                      .OfType<DesktopButton>()
                                      .Single(control => control.Name == "OpenStackNotificationButton");
            var destroyButton = stackItem.GetVisualDescendants()
                                         .OfType<DesktopButton>()
                                         .Single(control => control.Name == "DestroyStackNotificationsButton");

            enabledSwitch.IsChecked.ShouldBe(true);
            thresholdInput.Value.ShouldBe(3m);
            thresholdInput.Minimum.ShouldBe(1m);
            thresholdInput.Maximum.ShouldBe(10m);
            thresholdInput.IsEnabled.ShouldBeTrue();

            RaiseClick(destroyButton);
            window.GetVisualDescendants().OfType<WindowNotificationManager>().ShouldBeEmpty();

            RaiseClick(openButton);
            RaiseClick(openButton);
            window.UpdateLayout();

            var stackManager = window.GetVisualDescendants().OfType<WindowNotificationManager>().Single();
            stackManager.IsStackEnabled.ShouldBeTrue();
            stackManager.StackThreshold.ShouldBe(3);
            stackManager.GetVisualDescendants()
                        .OfType<NotificationCard>()
                        .Select(card => card.Content)
                        .ShouldBe([
                            "Notification 1: This is a stacked notification.",
                            "Notification 2: This is a deliberately longer stacked notification used to verify variable-height cards."
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

            var managers = window.GetVisualDescendants().OfType<WindowNotificationManager>().ToArray();
            managers.Length.ShouldBe(2);
            var defaultManager = managers.Single(manager => !ReferenceEquals(manager, stackManager));
            defaultManager.GetVisualDescendants().OfType<NotificationCard>().Count().ShouldBe(1);

            stackManager.IsMotionEnabled = false;
            RaiseClick(destroyButton);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            stackManager.GetVisualDescendants().OfType<NotificationCard>().ShouldBeEmpty();
            defaultManager.GetVisualDescendants().OfType<NotificationCard>().Count().ShouldBe(1);

            RaiseClick(openButton);
            window.UpdateLayout();
            stackManager.GetVisualDescendants()
                        .OfType<NotificationCard>()
                        .Single()
                        .Content.ShouldBe("Notification 3: This is a stacked notification.");

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            foreach (var manager in managers)
            {
                manager.GetVisualParent().ShouldBeNull();
            }
        });
    }

    [Fact]
    public void Notification_Stack_ShowCase_Localization_Is_Complete()
    {
        foreach (var language in new[] { "en-US", "zh-CN", "zh-TW", "pt-BR" })
        {
            var localization = XliffTestDocument.Read(
                $"controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Localization/{language}.xlf");

            foreach (var key in new[]
                     {
                         "StackTitle",
                         "StackDescription",
                         "StackEnabledLabel",
                         "StackThresholdLabel",
                         "P2ContentDestroyAll",
                         "P2NotificationStackedTitleFormat",
                         "P2NotificationStackedFormat",
                         "P2NotificationLongStackedFormat"
                     })
            {
                localization.ContainsKey(key).ShouldBeTrue($"Missing {key} in {language}.");
            }
        }
    }

    [Fact]
    public void Notification_Stack_ShowCase_Configuration_Row_Uses_One_Vertical_Center_Line()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new NotificationShowCase();

        ShowInWindow(page, window =>
        {
            var stackItem = page.GetVisualDescendants()
                                .OfType<ShowCaseItem>()
                                .Single(item => item.SourceKey == "notification-stack");
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
                                         .Single(control => control.Name == "NotificationStackEnabledSwitch");
            var thresholdInput = stackItem.GetVisualDescendants()
                                          .OfType<DesktopNumericUpDown>()
                                          .Single(control => control.Name == "NotificationStackThresholdInput");

            var expectedCenterY = GetCenterY(thresholdInput, stackItem);
            GetCenterY(enabledLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(enabledSwitch, stackItem).ShouldBe(expectedCenterY, 0.5);
            GetCenterY(thresholdLabel, stackItem).ShouldBe(expectedCenterY, 0.5);
        });
    }

    [Fact]
    public void Notification_Stack_ShowCase_Displays_The_V619_Version_Badge()
    {
        AvaloniaTestApp.EnsureInitialized();
        var page = new NotificationShowCase();

        ShowInWindow(page, window =>
        {
            var stackItem = page.GetVisualDescendants()
                                .OfType<ShowCaseItem>()
                                .Single(item => item.SourceKey == "notification-stack");
            window.UpdateLayout();

            stackItem.BadgeText.ShouldBe("v6.1.9");
            stackItem.GetVisualDescendants()
                     .OfType<DesktopRibbonBadge>()
                     .Single()
                     .Text.ShouldBe("v6.1.9");
        });
    }

    private static string ExtractNotificationExampleItems(string source)
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
            Child = content
        };
        var window = new AvaloniaWindow
        {
            Width = 1000,
            Height = 900,
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
