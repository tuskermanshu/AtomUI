# ComboBox 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

ComboBox 是唯一 Semantic owner，公开 12 个 Semantic Part：隐式 `root` 加 11 个非 root 部件（`prefix`、`frame`、
`content`、`placeholder`、`input`、`suffix`、`indicator`、`popup.root`、`popup.list`、`popup.listItem`、`popup.empty`）。
11 个非 root 部件各生成一个 `ComboBox*Style` 类型，`root` 不生成。声明位于 `ComboBox.SemanticParts.cs` partial 文件。

触发区部件的 marker 位于 `ComboBoxTheme.axaml` 宿主模板内：`prefix` / `suffix` 借用共享
`AddOnDecoratedBoxTheme` 的 `.semantic-scope-prefix` / `.semantic-scope-suffix` scope 锚点路由到宿主模板
投影给 decorated box 的内容节点（与 `Select` / `TreeSelect` / `Cascader` 同构）；`content` / `placeholder` /
`input` 直接标注宿主模板节点。`indicator` 声明 `CrossNestedOwners=true`：其物理节点在 ComboBox 自有的
internal `ComboBoxHandle` 模板内，经宿主模板的 `.semantic-scope-handle` 锚点跨入。弹层四部件位于 owner 自有的
模板内 Popup：`popup.root` 标注在宿主模板 `PopupFrame` 静态节点上，`popup.list` / `popup.empty` 是
`PopupFrame` 内的静态节点，`popup.listItem` 的 marker 在运行时容器创建路径注入。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `root` |
| Selector | ComboBox 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `ComboBox` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | ComboBox owner |
| 职责 | ComboBox root 是数据源、选择、过滤、弹层与输入框状态的组织边界。 |
| 相关 API | 全部 ComboBox public API |
| 相关 Token | ComboBoxToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `prefix`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `prefix` |
| Selector | `.semantic-prefix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix` |
| Style Type | `ComboBoxPrefixStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中投影给 `AddOnDecoratedBox.ContentLeftAddOn` 的 `AddOnContentPresenter`（经 `$parent[atom:ComboBox]` 编译绑定呈现公共 API 值） |
| 职责 | 内容框内联前缀区域，承载 `ContentLeftAddOn` 用户内容。 |
| 相关 API | `ContentLeftAddOn`、`ContentLeftAddOnTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `frame`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `frame` |
| Selector | `.semantic-frame` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-frame` |
| Style Type | `ComboBoxFrameStyle` |
| ContractType | `PixelAlignedBorder` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `true` |
| AtomUI 节点 | 共享 `AddOnDecoratedBoxTheme.axaml` 中 `AddOnDecoratedBoxContentFrame`（`Name="PART_ContentFrame"`）；锚点 `.semantic-scope-input` 位于 ComboBox 宿主模板的 `AddOnDecoratedBox` 节点 |
| 职责 | **输入框边框盒**——outlined / filled / underlined 变体的最内层边框盒，决定输入框的边框颜色、边框宽度、圆角与背景。这是应用最常需要的定制点（如把默认灰边框改成品牌色）。 |
| 相关 API | `StyleVariant`、`SizeType`、`Status`、`FormStatus`；内部 `EffectiveStatus` 驱动状态色 |
| 相关 Token | SharedToken（`ColorBorder`、`ColorPrimary`、`ColorError`、`ColorWarning`、`BorderRadius*`） |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `ComboBoxContentStyle` |
| ContractType | `Panel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` decorated box 内容面板（`PlaceholderText`、`SelectedContentPresenter`、`PART_EditableTextBox` 的公共父节点） |
| 职责 | 输入内容面板，承载占位符、非编辑态选中内容与编辑态输入框三个互斥/叠加节点。 |
| 相关 API | `PlaceholderText`、`SelectionBoxItem`、`SelectionBoxItemTemplate`、`IsEditable` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `placeholder`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `placeholder` |
| Selector | `.semantic-placeholder` |
| SelectorRoute | `/template/ .semantic-content > .semantic-placeholder` |
| Style Type | `ComboBoxPlaceholderStyle` |
| ContractType | `Avalonia.Controls.TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PlaceholderText`（`atom:TextBlock`，基类为 `Avalonia.Controls.TextBlock`） |
| 职责 | 未选择任何项且非编辑态时显示的占位符文本。 |
| 相关 API | `PlaceholderText` |
| 相关 Token | SharedToken（节点以 `Opacity=0.3` 弱化，见定制边界） |
| 稳定性 | stable since 6.2.0 |

#### `input`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `input` |
| Selector | `.semantic-input` |
| SelectorRoute | `/template/ .semantic-content > .semantic-input` |
| Style Type | `ComboBoxInputStyle` |
| ContractType | `Avalonia.Controls.TextBox` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PART_EditableTextBox`（internal `ComboBoxTextBox : Avalonia.Controls.TextBox`） |
| 职责 | `IsEditable=true` 时渲染的可编辑 / 过滤输入框；非编辑态节点仍存在但 `IsVisible=false`。 |
| 相关 API | `IsEditable`、`Text`、`IsFilterEnabled`、`FilterValue` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `suffix`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `suffix` |
| Selector | `.semantic-suffix` |
| SelectorRoute | `/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix` |
| Style Type | `ComboBoxSuffixStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `ContentRightAddOn` 的 `StackPanel`（承载 `PART_ContentRightAddOnPresenter`、`PART_FormFeedBack`、`PART_ComboBoxHandle`） |
| 职责 | 内容框右侧后缀区，承载用户后缀内容、Form 校验反馈与下拉指示器。 |
| 相关 API | `ContentRightAddOn`、`ContentRightAddOnTemplate`、`FormFeedback` |
| 相关 Token | SharedToken；`TextElement.Foreground` 默认取 `ColorTextQuaternary`（共享 `AddOnDecoratedBoxTheme` 提供，为本 Part 的下界） |
| 稳定性 | stable since 6.2.0 |

#### `indicator`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `indicator` |
| Selector | `.semantic-indicator` |
| SelectorRoute | `>> .semantic-scope-handle /template/ .semantic-indicator` |
| Style Type | `ComboBoxIndicatorStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `true` |
| AtomUI 节点 | `ComboBoxHandleTheme.axaml` 中 `PART_OpenIndicatorButton`（`atom:IconButton`，图标 `DownOutlined`）；锚点 `.semantic-scope-handle` 位于宿主模板的 `PART_ComboBoxHandle` |
| 职责 | 下拉展开指示器（箭头按钮）；同时是鼠标点击展开/收起的触发入口。 |
| 相关 API | `IsDropDownOpen`、`IsEnabled`、`IsMotionEnabled` |
| 相关 Token | ComboBoxToken（`HandleHoverColor` 驱动 hover 态箭头色） |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `/template/ .semantic-popup-root` |
| Style Type | `ComboBoxPopupRootStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PART_Popup` 的直接子节点 `PopupFrame` |
| 职责 | 弹层框体，承载候选列表与空态，决定弹层圆角、背景与内边距边界。 |
| 相关 API | `MaxDropDownHeight`、`PopupContentPadding`（internal）、`EffectivePopupWidth`（internal） |
| 相关 Token | SharedToken（`ColorBgElevated`）、PopupToken（`PopupCornerRadius`） |
| 稳定性 | stable since 6.2.0 |

#### `popup.list`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.list` |
| Selector | `.semantic-popup-list` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-list` |
| Style Type | `ComboBoxPopupListStyle` |
| ContractType | `Avalonia.Controls.ScrollViewer` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PopupFrame` 内的 `atom:ScrollViewer`（内部为 `PART_ItemsPresenter`） |
| 职责 | 候选列表滚动区，`IsVisible` 与空态互斥，是候选项的排布与滚动边界。 |
| 相关 API | `MaxDropDownHeight`、`ItemsPanel`、`ScrollViewer.IsLiteMode`、`ScrollViewer.AllowAutoHide` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.listItem`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.listItem` |
| Selector | `.semantic-popup-list-item` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-list-item` |
| Style Type | `ComboBoxPopupListItemStyle` |
| ContractType | `ComboBoxItem` |
| Cardinality | `Multiple` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | 运行时容器：`CreateContainerForItemOverride` / `PrepareContainerForItemOverride` 创建的 `ComboBoxItem` 实例本身（marker 不写在 `ComboBoxItemTheme.axaml` 模板内部节点上，理由见 §2） |
| 职责 | 单个候选项容器，承载项内容、选中视觉、hover/active 候选视觉、禁用态与过滤隐藏。 |
| 相关 API | `ItemsSource`、`ItemTemplate`、`SelectedItem`、`SelectedIndex`、`ItemHeight`（internal）、`OptionFontSize` |
| 相关 Token | ComboBoxToken（`ItemColor`、`ItemBgColor`、`ItemHoverColor`、`ItemHoverBgColor`、`ItemSelectedColor`、`ItemSelectedBgColor`、`ItemPadding`、`ItemMargin`） |
| 稳定性 | stable since 6.2.0 |

#### `popup.empty`

| 字段 | 值 |
| --- | --- |
| Owner | `ComboBox` |
| Part | `popup.empty` |
| Selector | `.semantic-popup-empty` |
| SelectorRoute | `/template/ .semantic-popup-root >> .semantic-popup-empty` |
| Style Type | `ComboBoxPopupEmptyStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `false` |
| CrossNestedOwners | `false` |
| AtomUI 节点 | `ComboBoxTheme.axaml` 中 `PART_EmptyIndicator`（内含 `atom:Empty`） |
| 职责 | 生效过滤模式下无匹配项时显示的弹层空态区；`IsVisible` 由 `IsEffectiveEmptyVisible` 驱动，与 `popup.list` 互斥。**仅在 `IsEditable=true` + `IsFilterEnabled=true` + 存在过滤值 + 无匹配项时可见**：未开启过滤时 `IsEffectiveEmptyVisible` 被强制为 `false`，此时弹层显示的是空的列表区而非空态区，空态节点仍存在但隐藏。 |
| 相关 API | `IsEditable`、`IsFilterEnabled`、`Text` / `FilterValue`；`IsEffectiveEmptyVisible`（internal，由上述状态推导） |
| 相关 Token | SharedToken（`Padding`） |
| 稳定性 | stable since 6.2.0 |

## Abstract AXAML Structure

来源：`src/AtomUI.Desktop.Controls/ComboBox/Themes/ComboBoxTheme.axaml`

```xml
<Panel>
    <AddOnDecoratedBox Name="{x:Static atom:AddOnDecoratedBox.AddOnDecoratedBoxPart}">
        <Panel>
            <TextBlock Name="PlaceholderText" />
            <ContentPresenter Name="SelectedContentPresenter" />
            <ComboBoxTextBox Name="PART_EditableTextBox" />
        </Panel>
    </AddOnDecoratedBox>
    <Popup Name="PART_Popup">
        <Border Name="PopupFrame">
            <Panel>
                <ScrollViewer>
                    <ItemsPresenter Name="PART_ItemsPresenter" />
                </ScrollViewer>
                <Border Name="PART_EmptyIndicator">
                    <Empty />
                </Border>
            </Panel>
        </Border>
    </Popup>
</Panel>
```

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
ComboBox
  -> ComboBoxHandle (control theme, ComboBoxHandleTheme.axaml)
     -> IconButton#PART_OpenIndicatorButton (template-stable)
  -> ComboBoxItem (item container control theme, ComboBoxItemTheme.axaml)
     -> ContentPresenter#ContentPresenter (internal-observable)
  -> ComboBoxTextBox (control theme, ComboBoxTextBoxTheme.axaml)
     -> ScrollViewer#ScrollViewer (template-stable)
        -> Panel (template-stable)
           -> TextBlock#Placeholder (template-stable)
           -> InputTextPresenter#PART_TextPresenter (template-stable)
  -> ComboBox (control theme, ComboBoxTheme.axaml)
     -> Panel (template-stable)
        -> AddOnDecoratedBox#{x:Static atom:AddOnDecoratedBox.AddOnDecoratedBoxPart} (template-stable)
           -> Panel (template-stable)
              -> TextBlock#PlaceholderText (template-stable)
              -> ContentPresenter#SelectedContentPresenter (internal-observable)
              -> ComboBoxTextBox#PART_EditableTextBox (template-stable)
        -> Popup#PART_Popup (template-stable)
           -> Border#PopupFrame (template-stable)
              -> Panel (template-stable)
                 -> ScrollViewer (template-stable)
                    -> ItemsPresenter#PART_ItemsPresenter (template-stable)
                 -> Border#PART_EmptyIndicator (template-stable)
                    -> Empty (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `ComboBox` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `ComboBoxHandle` | control theme | `ComboBoxHandleTheme.axaml` | ComboBox | `IsEnabled`, `IsMotionEnabled` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_OpenIndicatorButton` | template node (IconButton) | `ComboBoxHandleTheme.axaml` | ComboBoxHandle | `IsEnabled`, `IsMotionEnabled` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ComboBoxItem` | item container control theme | `ComboBoxItemTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `Content`, `ContentTemplate`, `CornerRadius`, `HorizontalContentAlignment`, `Padding` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `ContentPresenter` | template node (ContentPresenter) | `ComboBoxItemTheme.axaml` | ComboBoxItem | `Background`, `Content`, `ContentTemplate`, `CornerRadius`, `HorizontalContentAlignment`, `Padding` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `ComboBoxTextBox` | control theme | `ComboBoxTextBoxTheme.axaml` | ComboBox | `CaretBlinkInterval`, `CaretBrush`, `CaretIndex`, `HorizontalContentAlignment`, `LineHeight`, `PasswordChar` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `ScrollViewer` | template node (ScrollViewer) | `ComboBoxTextBoxTheme.axaml` | ComboBoxTextBox | `CaretBlinkInterval`, `CaretBrush`, `CaretIndex`, `HorizontalContentAlignment`, `LineHeight`, `PasswordChar` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `Panel` | template node (Panel) | `ComboBoxTextBoxTheme.axaml` | ComboBoxTextBox | `CaretBlinkInterval`, `CaretBrush`, `CaretIndex`, `HorizontalContentAlignment`, `LineHeight`, `PasswordChar` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `Placeholder` | template node (TextBlock) | `ComboBoxTextBoxTheme.axaml` | ComboBoxTextBox | `HorizontalContentAlignment`, `LineHeight`, `PlaceholderForeground`, `PlaceholderText`, `TextAlignment`, `TextWrapping` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_TextPresenter` | template node (InputTextPresenter) | `ComboBoxTextBoxTheme.axaml` | ComboBoxTextBox | `CaretBlinkInterval`, `CaretBrush`, `CaretIndex`, `HorizontalContentAlignment`, `LineHeight`, `PasswordChar` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ComboBox` | control theme | `ComboBoxTheme.axaml` | 用户代码 / 控件宿主 | `DataValidationErrors`, `EffectivePopupWidth`, `FormStatus`, `IsEditable`, `IsEffectiveEmptyVisible`, `IsEnabled` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `Panel` | template node (Panel) | `ComboBoxTheme.axaml` | ComboBox | `DataValidationErrors`, `EffectivePopupWidth`, `FormStatus`, `IsEditable`, `IsEffectiveEmptyVisible`, `IsEnabled` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `{x:Static atom:AddOnDecoratedBox.AddOnDecoratedBoxPart}` | template node (AddOnDecoratedBox) | `ComboBoxTheme.axaml` | ComboBox | `DataValidationErrors`, `FormStatus`, `IsEditable`, `IsEnabled`, `IsKeyboardFocusWithin`, `IsMotionEnabled` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PlaceholderText` | template node (TextBlock) | `ComboBoxTheme.axaml` | ComboBox | `PlaceholderText`, `SelectingItemsControl` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `SelectedContentPresenter` | template node (ContentPresenter) | `ComboBoxTheme.axaml` | ComboBox | `IsShowOverflowTip`, `OverflowTipDelay`, `OverflowTipPlacement`, `SelectionBoxItem`, `SelectionBoxItemTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_EditableTextBox` | template node (ComboBoxTextBox) | `ComboBoxTheme.axaml` | ComboBox | `IsEditable`, `PlaceholderForeground`, `PlaceholderText`, `Text` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Popup` | template node (Popup) | `ComboBoxTheme.axaml` | ComboBox | `EffectivePopupWidth`, `IsEffectiveEmptyVisible`, `IsMotionEnabled`, `ItemsPanel`, `MaxDropDownHeight`, `PopupContentPadding` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PopupFrame` | template node (Border) | `ComboBoxTheme.axaml` | ComboBox | `EffectivePopupWidth`, `IsEffectiveEmptyVisible`, `IsMotionEnabled`, `ItemsPanel`, `MaxDropDownHeight`, `PopupContentPadding` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ItemsPresenter` | template node (ItemsPresenter) | `ComboBoxTheme.axaml` | ComboBox | `ItemsPanel` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_EmptyIndicator` | template node (Border) | `ComboBoxTheme.axaml` | ComboBox | `IsEffectiveEmptyVisible` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `ContentLeftAddOn`、`ContentLeftAddOnTemplate`、`ContentRightAddOn`、`ContentRightAddOnTemplate`、`FilterValue`、`FilterValueSelector`、`LeftAddOnTemplate`、`OptionFontSize`、`RightAddOnTemplate` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 选择与集合 | `SelectedItem`、`SelectedIndex`、`DropDownDisplayPageSize`、`Filter`、`IsFilterEnabled` | 维护选择、展开、过滤、分页、分组或集合状态。 |
| 交互与状态 | `IsAllowClear`、`IsMotionEnabled`、`ShouldUseOverlayPopup`、`IsPopupPinnedOpen`、`Status`、`IsShowOverflowTip`、`OverflowTipDelay`、`OverflowTipPlacement` | 表达用户可观察状态、可用性、清除、加载、反馈和非编辑态选中内容溢出提示语义。Form 校验扩展状态进入内部 `FormStatus`，不覆盖显式 `Status`。 |
| 视觉与布局 | `SizeType`、`StyleVariant` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 其他稳定入口 | `LeftAddOn`、`RightAddOn` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

## Pseudo Classes

| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、collection/filter、input/value、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | ComboBox Token + ControlTheme。 |

## State Flow

ComboBox 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- `DataValidationErrors` 是 native error 唯一真源；`FormStatus` 承载 Form 的 warning、success、validating 和 error 投影，`Status` 只表示用户显式请求。输入表面通过 `InputControlState.ResolveEffectiveStatus` 计算有效状态，native/Form error 优先于 Form warning，再优先于显式 warning/error。
- open/close、collection/filter、input/value、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 单选结果以继承的 `SelectedItem` / `SelectedIndex` 为准；AtomUI Form 集成使用 `SelectedItem` 作为 ComboBox 的默认表单值，`SetFormValue`、`GetFormValue` 和 `ClearFormValue` 不应转换为字符串或读取展示文本。
- 下拉项的 active candidate 与 `SelectedItem` / `SelectedIndex` 分离。鼠标移动和键盘 `Up` / `Down` 共享同一个 active candidate；迁移只改变候选视觉，`Enter` 才提交 `SelectedItem`。`:pointerover` 不得形成第二个候选高亮，selected 视觉优先于 active 视觉。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## Theme and Token Boundaries

ComboBox 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `ComboBoxHandleTheme.axaml` | 定义局部操作入口、按钮或 handle 的状态视觉。 |
| `ComboBoxItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `ComboBoxTheme.axaml` | 定义布局、内容承载或框架节点视觉。 |

非编辑态选中内容的完整文本提示复用共享 `OverflowTip` attached behavior。模板只在 `SelectedContentPresenter` 上接入 `IsShowOverflowTip`、`OverflowTipDelay`、`OverflowTipPlacement` 和 `SelectionBoxItem`；`IsEditable=true` 时编辑输入框不默认启用该提示。

ComboBox 使用 `ComboBoxToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close、collection/filter、input/value、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

Token 边界：

ComboBox Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `ComboBoxToken`，scope id 为 `ComboBox`，源码位于 `src/AtomUI.Desktop.Controls/ComboBox/ComboBoxToken.cs`。

## Customization Boundaries

维护 ComboBox 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

维护不变量：

维护 ComboBox 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- Semantic Part 的 selector class、route、`ContractType`、cardinality 与 marker 归属；静态 marker 与运行时 marker 的
  注入点必须与 [ComboBox Semantic Part 契约](semantic-part.md) 一致（含 `popup.listItem` 打在容器实例上、在两条容器路径
  幂等注入的约束）。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
