using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// Splash 的九个语义部件全部位于 SplashTheme.axaml 单一 ControlTemplate 内的常驻静态节点，
// 因此 Cardinality 统一为 Single、不携带显式 SelectorRoute（生成器规范化为 "/template/ .<class>"），
// 且不需要 CrossVisualRoot / CrossNestedOwners / RuntimeCreated。
//
// 不发布为 Part 的节点及理由：
// - PART_RootLayout / PART_SurfaceLayout：可见表面属性已有 owner 真源（Background / CornerRadius /
//   Padding 均为 TemplateBinding），再发布 Part 会形成第二条互相竞争的定制路径。
// - PART_ContentLayout：纯布局脊柱，唯一职责是施加 ContentGap，间距走 Token 而不是 Part。
// - SplashWindow / PART_SurfaceHost：窗口壳层本身已是 public API，表面阴影与宿主圆角是 Token 语义，
//   窗口级覆盖入口是 SplashWindow.Resources 或专用子类；ShadowsAwareContainer 还是 internal，
//   ContractType 只能退化为 Decorator。
//
// 进度区按两个不同控件分别发布（spin / progressBar），而不是合成一个 Multiple Part：
// 二者是不同控件类型、不同 Token（IndicatorSize / ProgressBarHeight）、不同状态的替代实现，
// 开发者需要分别调整（例如把进度条调细、单独换指示器颜色）；合成后会丢失 x:SetterTargetType
// 的类型上下文，并迫使两种视觉共用一份 Setter。
//
// 四个文本位的 ContractType 取 Avalonia.Controls.TextBlock 基类而不是节点派生类型
// （AtomUI.Desktop.Controls.TextBlock）：派生类型同样满足契约，取基类让后续把节点替换为
// 普通 TextBlock 保持兼容；收窄到派生类型会成为破坏性变更。必须完全限定，否则在本命名空间内
// 非限定的 TextBlock 会解析到 AtomUI 的派生类型。
[SemanticPart(
    "logo",
    SelectorClass = "semantic-logo",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "subtitle",
    SelectorClass = "semantic-subtitle",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "spin",
    SelectorClass = "semantic-spin",
    ContractType = typeof(Spin),
    Since = "6.2.0")]
[SemanticPart(
    "progressBar",
    SelectorClass = "semantic-progress-bar",
    ContractType = typeof(ProgressBar),
    Since = "6.2.0")]
[SemanticPart(
    "message",
    SelectorClass = "semantic-message",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "detail",
    SelectorClass = "semantic-detail",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "footer",
    SelectorClass = "semantic-footer",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class Splash
{
}
