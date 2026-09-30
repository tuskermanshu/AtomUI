using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Theme;

public class DesktopControlsDataEntryThemeScopeMigrationTests
{
    private static readonly string[] ControlDirectories =
    [
        "src/AtomUI.Desktop.Controls/AutoComplete",
        "src/AtomUI.Desktop.Controls/ButtonSpinner",
        "src/AtomUI.Desktop.Controls/Cascader",
        "src/AtomUI.Desktop.Controls/CheckBox",
        "src/AtomUI.Desktop.Controls/ComboBox",
        "src/AtomUI.Desktop.Controls/DatePicker",
        "src/AtomUI.Desktop.Controls/Form",
        "src/AtomUI.Desktop.Controls/Input",
        "src/AtomUI.Desktop.Controls/Mentions",
        "src/AtomUI.Desktop.Controls/NumericUpDown",
        "src/AtomUI.Desktop.Controls/OtpLineEdit",
        "src/AtomUI.Desktop.Controls/OptionButtonGroup",
        "src/AtomUI.Desktop.Controls/RadioButton",
        "src/AtomUI.Desktop.Controls/Rate",
        "src/AtomUI.Desktop.Controls/Select",
        "src/AtomUI.Desktop.Controls/Slider",
        "src/AtomUI.Desktop.Controls/Switch",
        "src/AtomUI.Desktop.Controls/TimePicker",
        "src/AtomUI.Desktop.Controls/Transfer",
        "src/AtomUI.Desktop.Controls/TreeSelect",
        "src/AtomUI.Desktop.Controls/Upload",
        "src/AtomUI.Desktop.Controls/Primitives/AddOnDecoratedBox",
        "src/AtomUI.Desktop.Controls/Primitives/InfoPickerInput"
    ];

    private static readonly string[] ThemeDirectories =
        ControlDirectories.Select(static directory => $"{directory}/Themes").ToArray();

    [Fact]
    public void DataEntry_Control_Themes_Use_Explicit_Token_Resources()
    {
        foreach (var relativeDirectory in ThemeDirectories)
        {
            ThemeAssetScopeAssertions.AssertDirectoryUsesExplicitTokenResources(relativeDirectory);
        }
    }
}
