using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class DesktopControlsFeedbackThemeScopeMigrationTests
{
    private static readonly string[] ControlDirectories =
    [
        "src/AtomUI.Desktop.Controls/Alert",
        "src/AtomUI.Desktop.Controls/Collapse",
        "src/AtomUI.Desktop.Controls/Message",
        "src/AtomUI.Desktop.Controls/MessageBox",
        "src/AtomUI.Desktop.Controls/Notifications",
        "src/AtomUI.Desktop.Controls/PopupConfirm",
        "src/AtomUI.Desktop.Controls/ProgressBar",
        "src/AtomUI.Desktop.Controls/Result",
        "src/AtomUI.Desktop.Controls/Skeleton",
        "src/AtomUI.Desktop.Controls/Spin",
        "src/AtomUI.Desktop.Controls/Tooltip",
        "src/AtomUI.Desktop.Controls/Tour"
    ];

    private static readonly string[] ThemeDirectories =
        ControlDirectories.Select(static directory => $"{directory}/Themes").ToArray();

    [Fact]
    public void Feedback_Control_Themes_Use_Explicit_Token_Resources()
    {
        foreach (var relativeDirectory in ThemeDirectories)
        {
            ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(relativeDirectory);
        }
    }
}
