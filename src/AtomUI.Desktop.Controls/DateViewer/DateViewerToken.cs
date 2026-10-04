using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class DateViewerToken : AbstractControlDesignToken
{
    public Color PanelBg { get; set; }
    public Color CellHoverBg { get; set; }
    public Color CellRangeBg { get; set; }
    public double PanelWidth { get; set; }
    public double CellHeight { get; set; }
    public double CellWidth { get; set; }
    public double PeriodCellWidth { get; set; }
    public Thickness PeriodCellPadding { get; set; }
    public double ContentCellMinHeight { get; set; }
    public Thickness PanelPadding { get; set; }
    public double PanelSpacing { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        PanelBg = EffectiveGlobalToken.ColorBgContainer;
        CellHoverBg = EffectiveGlobalToken.ControlItemBgHover;
        CellRangeBg = EffectiveGlobalToken.ControlItemBgActive;
        PanelWidth = 260;
        CellHeight = EffectiveGlobalToken.ControlHeightSM;
        CellWidth = EffectiveGlobalToken.ControlHeightSM;
        PeriodCellWidth = EffectiveGlobalToken.ControlHeightLG * 1.5;
        PeriodCellPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXS, 0);
        ContentCellMinHeight = EffectiveGlobalToken.ControlHeightLG * 2;
        PanelPadding = EffectiveGlobalToken.PaddingSM;
        PanelSpacing = EffectiveGlobalToken.UniformlyMargin;
    }
}
