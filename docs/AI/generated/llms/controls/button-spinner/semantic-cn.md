# ButtonSpinner 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

`ButtonSpinner` 公开 `root`、`content`、`innerLeftContent`、`innerRightContent`、`actions`、`increaseButton`、
`decreaseButton` 七个职责区域（§1.1–1.7），全部为 `Single`。其中 `content`、`innerLeftContent`、`innerRightContent`
位于帧主题、两个按钮位于手柄主题，因此这四个部件声明 `CrossNestedOwners`；`actions` 的 marker 仍在 ButtonSpinner
自己的模板内，按宿主模板本地校验。

ButtonSpinner 是 AtomUI 的带步进按钮输入基座，其公开语义区域对应上游 `InputNumber` 已公开并实际消费的
`root` / `prefix` / `suffix` / `input` / `actions` 键：帧内主内容区映射 `input` 职责，帧内左、右内容槽映射
`prefix` / `suffix` 职责，步进按钮区映射 `actions`。范围依据与准入记录见
[改造设计](../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md)。

### 1.1 `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `root` |
| Selector | ButtonSpinner 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ButtonSpinner` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | ButtonSpinner owner |
| 职责 | 承载尺寸档、variant、状态、步进开关和 owner-scoped Semantic Style 入口。 |
| 相关 API | `SizeType`、`StyleVariant`、`Status`、`IsSpinEnabled`、`IsButtonSpinnerVisible`、`IsButtonSpinnerFloatable`、`ButtonSpinnerLocation`、`SpinnerHandleWidth`、`IsEnabled` |
| 相关 Token | SharedToken、`ButtonSpinnerToken` |
| 稳定性 | stable since 6.2.0 |

`root` 是控件自身，不声明 `.semantic-root` marker。它适合定制 ButtonSpinner 整体 `Opacity`、对齐和尺寸约束。

**根边框定制：** 可见外框由共享输入帧绘制，其状态机拥有 `BorderBrush`。owner 的 `BorderBrush` 会以
LocalValue 中继到帧节点生效（对齐上游 `styles.root.borderColor` 语义与共享 `AbstractTextInput` 行为），
因此在 owner-scoped Style 里直接写根 `Setter Property="BorderBrush"` 即可定制外框颜色：

```xml
<Style Selector="atom|ButtonSpinner.semantic-object">
    <Setter Property="BorderBrush" Value="#1677FF" />
    <atom:ButtonSpinnerActionsStyle x:SetterTargetType="TemplatedControl">
        <Setter Property="Background" Value="#F0F5FF" />
    </atom:ButtonSpinnerActionsStyle>
</Style>
```

定制期间该属性槽的 hover / focus 变色冻结（LocalValue 优先于帧状态机），focus 的 `BoxShadow` 光晕不受影响；
置空（或不设置）后恢复帧状态机。中继带接管标记：仅当本控件实际写入过该槽位时才在置空时清除，因此
`NumericUpDown` 这种「自身持有中继、内部 spinner 是其派生实现」的嵌套场景不会被外层清除。

### 1.2 `content`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-scope-frame /template/ .semantic-content` |
| Style Type | `ButtonSpinnerContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（锚点 `.semantic-scope-frame` 解析到 `ButtonSpinnerDecoratedBox` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧模板内的主内容 presenter（`PART_ContentPresenter`） |
| 职责 | 承载 `Content` 与 `ContentTemplate` 的最终呈现，即 ButtonSpinner 的内容表面。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | 输入尺寸 padding、`FontSize` 继承 |
| 稳定性 | stable since 6.2.0 |

`content` 是 ButtonSpinner 自己的内容表面。ButtonSpinner 不拥有文本编辑面，因此不发布 `input` 部件；派生控件
（如 `NumericUpDown`）自行在自己的 owner 上发布 `input`，其契约不通过 ButtonSpinner 继承。`Content=null` 时
presenter 仍属于静态模板结构。适合定制 `Opacity`、`Margin`、`Padding`、`Foreground` 和对齐；内容子树由用户或
派生控件拥有，不由 `content` 契约继续展开。

### 1.3 `innerLeftContent`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `innerLeftContent` |
| Selector | `.semantic-inner-left-content` |
| SelectorRoute | `/template/ .semantic-scope-frame /template/ .semantic-inner-left-content` |
| Style Type | `ButtonSpinnerInnerLeftContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（锚点 `.semantic-scope-frame` 解析到 `ButtonSpinnerDecoratedBox` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧模板内的内容左槽 presenter（`PART_ContentLeftAddOn`，internal `AddOnContentPresenter`） |
| 职责 | 承载 `InnerLeftContent` 与 `InnerLeftContentTemplate` 的最终呈现。 |
| 相关 API | `InnerLeftContent`、`InnerLeftContentTemplate` |
| 相关 Token | `SpacingXXS`、输入尺寸 padding |
| 稳定性 | stable since 6.2.0 |

`innerLeftContent` 对应上游 `prefix` 职责，但**不使用 `semantic-prefix` class**：`NumericUpDown` 为其自有
`prefix` 发布的 route 是宽松后代（`/template/ .semantic-scope-spinner >> .semantic-prefix`），而 ButtonSpinner 的
帧节点位于 `NumericUpDownSpinner` 的子树内。若 ButtonSpinner 额外标记 `semantic-prefix`，该 route 会命中两个节点，
破坏 `NumericUpDown` 已发布的 `Single` 契约与其现有测试（该测试用 `.Single()` 解析目标）。改用与 ButtonSpinner
公开 API 同名的 `innerLeftContent` 可在不修改任何已发布契约的前提下保持唯一目标，且与仓库已有的
`itemContent` / `listContent` 命名法一致。帧槽位在空内容时自折叠，因此该部件呈现的是「有内容时才可见的左槽」。

### 1.4 `innerRightContent`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `innerRightContent` |
| Selector | `.semantic-inner-right-content` |
| SelectorRoute | `/template/ .semantic-scope-frame /template/ .semantic-inner-right-content` |
| Style Type | `ButtonSpinnerInnerRightContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（锚点 `.semantic-scope-frame` 解析到 `ButtonSpinnerDecoratedBox` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧模板内的内容右槽 presenter（`PART_ContentRightAddOn`，internal `AddOnContentPresenter`） |
| 职责 | 承载 `InnerRightContent` 与 `InnerRightContentTemplate` 的最终呈现。 |
| 相关 API | `InnerRightContent`、`InnerRightContentTemplate` |
| 相关 Token | `SpacingXXS`、输入尺寸 padding |
| 稳定性 | stable since 6.2.0 |

`innerRightContent` 对应上游 `suffix` 职责，命名理由同 §1.3。它与浮动手柄在 hover 时的 `ContentRightShift`
位移共享同一布局链路：定制其 `Margin` / `Padding` 时要按 §7.1 审计与手柄占位的协调结果。

### 1.5 `actions`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `actions` |
| Selector | `.semantic-actions` |
| SelectorRoute | `/template/ .semantic-scope-frame >> .semantic-actions` |
| Style Type | `ButtonSpinnerActionsStyle` |
| ContractType | `TemplatedControl` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `false`（marker 在 owner 模板内） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 帧内 `SpinnerContent` 承载的步进手柄（internal `ButtonSpinnerHandle`，最低 public 类型 `TemplatedControl`） |
| 职责 | 步进按钮区整体表面：手柄背景、分隔线与外框描边、圆角、内边距和悬浮/浮动呈现。 |
| 相关 API | `IsButtonSpinnerVisible`、`IsButtonSpinnerFloatable`、`ButtonSpinnerLocation`、`SpinnerHandleWidth`、`IsSpinEnabled` |
| 相关 Token | `ButtonSpinnerToken.HandleWidth`、`HandleBg`、`FilledHandleBg`、`HandleBorderColor`、`HandleActiveBg` |
| 稳定性 | stable since 6.2.0 |

`actions` 直接对应上游 `InputNumber` 的 `actions` 键——上游把上、下步进按钮包在同一个 `-actions` 容器内，AtomUI
对应的容器就是 `ButtonSpinnerHandle`。手柄的背景与描边由该节点自身的 `Render` 绘制并在状态变化时重绘，因此它是
步进区唯一稳定的对外表面。

适合定制的属性只有该节点实际参与绘制的成员：`Background`（填充）、`BorderBrush`（分隔竖线、中线以及
hover / pressed 颜色来源）、`CornerRadius`（填充外角）和 `Opacity`。**不适合定制：** 手柄描边宽度来自 internal
`SpinnerBorderThickness`（由 `SharedToken.BorderThickness` 供给），`BorderThickness` 与 `Padding` 在该节点的渲染与
模板中都没有参与，设置它们不会产生视觉变化，不属于本 Part 的稳定定制面。

步进行为、`ValidSpinDirection`、键盘与滚轮路径仍由 `ButtonSpinner` 拥有，不通过 Semantic Style 改写。

手柄在 `IsButtonSpinnerVisible=false`、或浮动模式下未悬浮、或控件 disabled 时保持存在但 `Opacity` 为 0；节点
身份与数量不随这些状态变化，因此是 `Single` 而不是 `Optional`。

### 1.6 `increaseButton`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `increaseButton` |
| Selector | `.semantic-increase-button` |
| SelectorRoute | `>> .semantic-actions /template/ .semantic-increase-button` |
| Style Type | `ButtonSpinnerIncreaseButtonStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（以 `.semantic-actions` 为锚点解析到 `ButtonSpinnerHandle` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 手柄主题内的增加按钮（`IconButton#PART_IncreaseButton`） |
| 职责 | 提供增加步进的交互入口表面。 |
| 相关 API | `IsSpinEnabled`、`ValidSpinDirection`、`IsButtonSpinnerVisible` |
| 相关 Token | `HandleIconSize`、`HandleHoverColor`、`ColorPrimaryActive`、`HandleActiveBg` |
| 稳定性 | stable since 6.2.0 |

`increaseButton` 的最低 public `ContractType` 是 `IconButton`。marker 位于 `ButtonSpinnerHandle` 自己的主题资产
（嵌套控件的模板），因此按 [Semantic Part 系统设计 §3.3.1](../../../../architecture/systems/theming/semantic-parts.md)
以 `.semantic-actions` 为锚点校验与解析。它适合定制 `IconBrush`、`IconWidth` / `IconHeight`、`Background`、
`CornerRadius` 和 `Opacity`。增加按钮的启用状态由 `ValidSpinDirection` 与 `IsSpinEnabled` 推导（`SetButtonUsage`），
不通过 Semantic Style 改写。

### 1.7 `decreaseButton`

| 字段 | 值 |
| --- | --- |
| Owner | `ButtonSpinner` |
| Part | `decreaseButton` |
| Selector | `.semantic-decrease-button` |
| SelectorRoute | `>> .semantic-actions /template/ .semantic-decrease-button` |
| Style Type | `ButtonSpinnerDecreaseButtonStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| CrossNestedOwners | `true`（以 `.semantic-actions` 为锚点解析到 `ButtonSpinnerHandle` 主题） |
| RuntimeCreated | `false` |
| AtomUI 节点 | 手柄主题内的减少按钮（`IconButton#PART_DecreaseButton`） |
| 职责 | 提供减少步进的交互入口表面。 |
| 相关 API | `IsSpinEnabled`、`ValidSpinDirection`、`IsButtonSpinnerVisible` |
| 相关 Token | `HandleIconSize`、`HandleHoverColor`、`ColorPrimaryActive`、`HandleActiveBg` |
| 稳定性 | stable since 6.2.0 |

与 `increaseButton` 对称；两个按钮是独立职责，因此分别发布为两个 `Single` 部件，允许差异化定制而无需
`Multiple` 语义。`ButtonSpinnerLocation` 只改变手柄相对内容的位置，不交换这两个按钮的身份。

上游 `InputNumber` 只为整个 `actions` 容器公开一个键，没有为单个上、下按钮公开语义键；本控件额外发布这两个
部件属于显式能力补充，理由与 `SplitButton` 的 `primary` / `secondary` 一致：它们的宿主是模板内部件，不发布则
完全不可定制。

## Abstract AXAML Structure

来源：`src/AtomUI.Desktop.Controls/ButtonSpinner/Themes/ButtonSpinnerTheme.axaml`

```xml
<ButtonSpinnerDecoratedBox Name="PART_DecoratedBox" />
```

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
ButtonSpinner
  -> ButtonSpinnerDecoratedBox (control theme, ButtonSpinnerDecoratedBoxTheme.axaml)
     -> DockPanel#RootLayout (template-stable)
        -> PixelAlignedBorder#{x:Static atom:AddOnDecoratedBoxThemeConstants.LeftAddOnPart} (template-stable)
           -> AddOnContentPresenter#PART_LeftAddOnPresenter (template-stable)
        -> PixelAlignedBorder#{x:Static atom:AddOnDecoratedBoxThemeConstants.RightAddOnPart} (template-stable)
           -> AddOnContentPresenter#PART_RightAddOnPresenter (template-stable)
        -> Panel (template-stable)
           -> AddOnDecoratedBoxContentFrame#{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentFramePart} (template-stable)
              -> ButtonSpinnerContentPanel#ContentLayout (internal-observable)
                 -> AddOnContentPresenter#{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart} (internal-observable)
                 -> AddOnContentPresenter#{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart} (internal-observable)
                 -> ContentPresenter#{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentPresenterPart} (internal-observable)
           -> ContentPresenter#PART_SpinnerHandle (template-stable)
  -> ButtonSpinnerHandle (control theme, ButtonSpinnerHandleTheme.axaml)
     -> UniformGrid (template-stable)
        -> IconButton#PART_IncreaseButton (template-stable)
        -> IconButton#PART_DecreaseButton (template-stable)
  -> ButtonSpinner (control theme, ButtonSpinnerTheme.axaml)
     -> ButtonSpinnerDecoratedBox#PART_DecoratedBox (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `ButtonSpinner` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `ButtonSpinnerDecoratedBox` | control theme | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinner | `Background`, `BorderBrush`, `ButtonSpinnerLocation`, `Content`, `ContentLeftAddOn`, `ContentLeftAddOnTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `RootLayout` | template node (DockPanel) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `Background`, `BorderBrush`, `ButtonSpinnerLocation`, `Content`, `ContentLeftAddOn`, `ContentLeftAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `{x:Static atom:AddOnDecoratedBoxThemeConstants.LeftAddOnPart}` | template node (PixelAlignedBorder) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `LeftAddOn`, `LeftAddOnBorderThickness`, `LeftAddOnCornerRadius`, `LeftAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_LeftAddOnPresenter` | template node (AddOnContentPresenter) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `LeftAddOn`, `LeftAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `{x:Static atom:AddOnDecoratedBoxThemeConstants.RightAddOnPart}` | template node (PixelAlignedBorder) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `RightAddOn`, `RightAddOnBorderThickness`, `RightAddOnCornerRadius`, `RightAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_RightAddOnPresenter` | template node (AddOnContentPresenter) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `RightAddOn`, `RightAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `Panel` | template node (Panel) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `Background`, `BorderBrush`, `ButtonSpinnerLocation`, `Content`, `ContentLeftAddOn`, `ContentLeftAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentFramePart}` | template node (AddOnDecoratedBoxContentFrame) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `Background`, `BorderBrush`, `ButtonSpinnerLocation`, `Content`, `ContentLeftAddOn`, `ContentLeftAddOnTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ContentLayout` | template node (ButtonSpinnerContentPanel) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `ButtonSpinnerLocation`, `Content`, `ContentLeftAddOn`, `ContentLeftAddOnTemplate`, `ContentRightAddOn`, `ContentRightAddOnTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart}` | template node (AddOnContentPresenter) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `ContentLeftAddOn`, `ContentLeftAddOnTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart}` | template node (AddOnContentPresenter) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `ContentRightAddOn`, `ContentRightAddOnTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `{x:Static atom:AddOnDecoratedBoxThemeConstants.ContentPresenterPart}` | template node (ContentPresenter) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `Content`, `ContentTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_SpinnerHandle` | template node (ContentPresenter) | `ButtonSpinnerDecoratedBoxTheme.axaml` | ButtonSpinnerDecoratedBox | `HandleOpacity`, `SpinnerContent` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ButtonSpinnerHandle` | control theme | `ButtonSpinnerHandleTheme.axaml` | ButtonSpinner | 主题状态 / visual state | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_IncreaseButton` | template node (IconButton) | `ButtonSpinnerHandleTheme.axaml` | ButtonSpinnerHandle | 主题状态 / visual state | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_DecreaseButton` | template node (IconButton) | `ButtonSpinnerHandleTheme.axaml` | ButtonSpinnerHandle | 主题状态 / visual state | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ButtonSpinner` | control theme | `ButtonSpinnerTheme.axaml` | 用户代码 / 控件宿主 | `ButtonSpinnerLocation`, `CompactSpaceItemPosition`, `CompactSpaceOrientation`, `Content`, `ContentTemplate`, `DataValidationErrors` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PART_DecoratedBox` | template node (ButtonSpinnerDecoratedBox) | `ButtonSpinnerTheme.axaml` | ButtonSpinner | `ButtonSpinnerLocation`, `CompactSpaceItemPosition`, `CompactSpaceOrientation`, `Content`, `ContentTemplate`, `DataValidationErrors` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ContentLeftShift`、`ContentPadding`、`ContentRightShift`、`InnerLeftContent`、`InnerLeftContentTemplate`、`InnerRightContent`、`InnerRightContentTemplate`、`LeftAddOnTemplate`、`RightAddOnTemplate`、`SpinnerContent` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 交互与状态 | `IsButtonSpinnerFloatable`、`IsButtonSpinnerVisible`、`IsHandleFloatable`、`IsMotionEnabled`、`IsShowHandle`、`IsSpinEnabled`、`Status` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `HandleOffset`、`SizeType`、`SpinnerBorderThickness`、`SpinnerHandleWidth`、`StyleVariant` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `ButtonSpinnerLocation`、`HandleOpacity`、`LeftAddOn`、`RightAddOn` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

## Pseudo Classes

| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | ButtonSpinner Token + ControlTheme。 |

## State Flow

ButtonSpinner 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## Theme and Token Boundaries

ButtonSpinner 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `ButtonSpinnerTheme.axaml` | owner 模板：帧宿主（`semantic-scope-frame` 锚点）与 `SpinnerContent` 内的步进手柄；持有 `SizeType` 圆角映射与 `SpinnerHandleWidth` 默认值。 |
| `ButtonSpinnerDecoratedBoxTheme.axaml` | 帧模板（`BasedOn` 共享 `AddOnDecoratedBoxTheme`）：左右 addon 区、内容框、内容左/右槽与浮动手柄 presenter；`SizeType` padding 与 `MinHeight` 基线。 |
| `ButtonSpinnerHandleTheme.axaml` | 步进手柄模板：增加/减少按钮、手柄自身的背景与分隔线描边绘制、variant 与 disabled 状态视觉。 |

ButtonSpinner 使用 `ButtonSpinnerToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

Token 边界：

ButtonSpinner Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ButtonSpinnerToken`，scope id 为 `ButtonSpinner`，源码位于 `src/AtomUI.Desktop.Controls/ButtonSpinner/ButtonSpinnerToken.cs`。

## Customization Boundaries

维护 ButtonSpinner 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

维护不变量：

维护 ButtonSpinner 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
