# DateViewer Token 设计

DateViewerToken 为独立单值与范围面板提供默认资源。关联 [架构](overview.md)、[实现](implementation.md)、
[共享设计](shared-panel-design.md)、[Changelog](changelog.md)；全局规则见
[控件 Token 规范](../../../../engineering/development/control-token-guidelines.md)。

## 1. 定位

[DateViewerToken.cs](../../../../../src/AtomUI.Desktop.Controls/DateViewer/DateViewerToken.cs)通过 ControlDesignToken
注册并生成 DateViewerTokenResource/TokenKind。Token 类是 internal，应用使用生成资源入口；
公共面板不依赖 DatePicker Token。DateViewer 与 RangeDateViewer 共享同一组资源。

## 2. Token 分类

| 分类 | Token | 默认计算 / 语义 |
| --- | --- | --- |
| 表面 | PanelBg | ColorBgContainer，面板背景。 |
| 交互 | CellHoverBg | ControlItemBgHover，Hover 背景。 |
| 连续范围 | CellRangeBg | ControlItemBgActive，范围/预览中段。 |
| Compact 尺寸 | PanelWidth | 260，单面板自然宽度。 |
| 值框尺寸 | CellHeight / CellWidth | ControlHeightSM，日期值框最小高度与宽度。 |
| 周期值框 | PeriodCellWidth | ControlHeightLG × 1.5，月、季度、年使用固定宽度，避免背景随文本长度变化。 |
| 周期内边距 | PeriodCellPadding | 左右 UniformlyPaddingXS，上下 0；周期文本在固定值框内居中。 |
| Content 密度 | ContentCellMinHeight | ControlHeightLG × 2，内容单元格最小高度。 |
| 布局 | PanelPadding | PaddingSM，独立面板内边距。 |
| 范围布局 | PanelSpacing | UniformlyMargin，双面板及双 Header 间距。 |

主色、文字、禁用透明度、圆角与边框直接消费 SharedToken。日期值、PanelKind、活动端点、Hover 值与业务禁用
结果不进入 Token；本组没有独立 Motion Token。

## 3. 控件专项模型中的 Token 使用

DateViewerTheme/RangeDateViewerTheme 消费表面、padding 和双面板间距；DatePanelTheme 消费 PanelWidth。
Compact 使用自然宽度，Content 解除固定面板宽度并由外部测量宽度展开列。
DateViewerCellTheme 消费值框尺寸、ContentCellMinHeight 与状态色；范围背景和预览使用同一范围资源。

Header 默认使用 SharedToken 导航按钮；范围每个 Header 按单面板宽度与间距对齐。
内容模板只接管内容，不能以 Token 替代模型状态。

## 4. 控件家族影响

独立公共 owner 使用本组资源。Calendar 与 Picker 可通过内部主题配置表达产品完整视觉，
其业务资源和范围条 lane 不并入 DateViewerToken。默认日期面板不能反向依赖农历、TimeView 或 Picker Footer。

## 5. 兼容性要求

十一个生成资源名、类型与含义构成定制契约。局部资源覆盖、资源移除、主题切换和 Token 动态变化
须保持可用；不能复制静态颜色规避资源生命周期。宿主产品输出仍需按完整视觉基线验证，
公共面板独立尺寸不能充当 Picker 尺寸等价证据。

## 6. 验证策略

验证单/双面板默认尺寸、Compact/Content、状态色、局部覆盖和主题变化，
同时保持原裁剪、布局与滚动条件。Token-only 与 Style-only 消费通过真实 TypeMap/裁剪专项验证。
宿主和 NativeAOT 的验收范围独立记录，见 [共享验收](shared-panel-design.md#10-重建边界与等价验收)。
