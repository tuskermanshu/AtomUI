# PopupConfirm 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

PopupConfirm 的唯一 Semantic owner 是 `PopupConfirm`。`PopupConfirm` 继承 `FlyoutHost`，弹层由
`PopupConfirmFlyout.CreatePresenter()` 在运行时创建、跨视觉根；internal 的 `PopupConfirmContainer`
不作为 public Semantic owner，它只承载确认体节点。公开 9 个 Semantic Part：`root`（隐式）+
8 个 `popup.*`。

语义对齐上游 Popconfirm 的语义 DOM（槽位：`root` / `container` / `icon` / `title` / `content` / `arrow`）。映射关系中有一处刻意差异：上游 `content` 槽位（描述正文）在 AtomUI
映射为 `popup.description`，因为 FlyoutHost 家族的 `popup.content` 已固定表示弹层框体的内容呈现面
（`ContentPresenter#ContentPresenter`），不能再用作描述节点。上游并未把按钮行
发布为语义 key，AtomUI 以 `popup.actions` 发布操作区，对齐 Alert `actions` 的稳定先例。

| PopupConfirm Part | 上游 Popconfirm 槽位 | AtomUI 节点 |
| --- | --- | --- |
| `root`（隐式） | 不适用（AtomUI 控件自身） | `PopupConfirm` owner |
| `popup.root` | `root` | `FlyoutPresenter`（`ArrowDecoratedBox`） |
| `popup.container` | `container` | `Border#PART_ContentDecorator` |
| `popup.content` | 不适用（Popover 家族框体内容面） | `ContentPresenter#ContentPresenter` |
| `popup.arrow` | `arrow` | `ArrowIndicator#PART_ArrowIndicator` |
| `popup.icon` | `icon` | `IconPresenter#PART_IconPresenter` |
| `popup.title` | `title` | `TextBlock#PART_Title` |
| `popup.description` | `content` | `ContentPresenter#PART_Content` |
| `popup.actions` | 不适用（上游按钮行未发布为语义 key） | `StackPanel#PART_ButtonLayout` |

声明位于 `PopupConfirm.SemanticParts.cs` partial 文件；`root` 为生成器隐式注册，不在该文件中显式声明。全部
8 个 `popup.*` 部件声明 `CrossVisualRoot=true` + `RuntimeCreated=true`：承载节点由
`PopupConfirmFlyout.CreatePresenter()` 在运行时创建，不在 `PopupConfirm` 自身 `PopupConfirmTheme.axaml`
模板内，`SelectorRoute` 因此使用从 owner 逻辑祖先链经 `popup.root` 下钻的后代路由。marker 注入策略分两类：

- 四个弹层框体件 `popup.root` / `popup.container` / `popup.content` / `popup.arrow` 复用 `FlyoutHost`
  家族共享的代码注入路径：`Flyout.CreatePresenter()` 注入 `popup.root`，`FlyoutPresenter.OnApplyTemplate`
  注入 `popup.container` / `popup.content` / `popup.arrow`。这三类 marker 不能静态声明在共享
  `ArrowDecoratedBoxTheme.axaml` 上，否则会污染 DatePicker / TimePicker / Menu / TreeView 等其它
  `ArrowDecoratedBox` 弹层。
- 四个确认体件 `popup.icon` / `popup.title` / `popup.description` / `popup.actions` 的 marker 静态声明在
  `PopupConfirmContainerTheme.axaml` 上；该主题只服务 PopupConfirm 内部容器，静态标记不会污染其它控件。

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `root` |
| Selector | PopupConfirm 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `PopupConfirm` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | PopupConfirm owner |
| 职责 | 触发宿主与确认状态的组织边界，承载 public API、确认/取消事件与主题入口。 |
| 相关 API | 全部 PopupConfirm public API |
| 相关 Token | PopupConfirmToken、FlyoutHostToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.root`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.root` |
| Selector | `.semantic-popup-root` |
| SelectorRoute | `>> .semantic-popup-root` |
| Style Type | `PopupConfirmPopupRootStyle` |
| ContractType | `FlyoutPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `Flyout.CreatePresenter()` 创建的 `FlyoutPresenter`（`ArrowDecoratedBox`） |
| 职责 | 弹层根节点，承载弹层背景、边框、内边距与箭头，对应上游 Popconfirm `root` 槽位。 |
| 相关 API | `Flyout`、`ShouldUseOverlayPopup`、`IsArrowVisible` |
| 相关 Token | FlyoutHostToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.container`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.container` |
| Selector | `.semantic-popup-container` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-container` |
| Style Type | `PopupConfirmPopupContainerStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `Border#PART_ContentDecorator`（背景/边框/圆角/内边距，模板绑定自 presenter） |
| 职责 | 弹层内层容器，承载背景、边框、圆角与内边距，对应上游 Popconfirm `container` 槽位。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.content`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.content` |
| Selector | `.semantic-popup-content` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-content` |
| Style Type | `PopupConfirmPopupContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `ContentPresenter#ContentPresenter` |
| 职责 | 弹层框体的内容呈现面，承载确认体容器；同为 FlyoutHost 家族的内容槽位，不对应上游 Popconfirm 单独语义 key。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.arrow`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.arrow` |
| Selector | `.semantic-popup-arrow` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-arrow` |
| Style Type | `PopupConfirmPopupArrowStyle` |
| ContractType | `ArrowIndicator` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `ArrowDecoratedBoxTheme.axaml` 的 `ArrowIndicator#PART_ArrowIndicator` |
| 职责 | 指向锚点的浮动箭头，对应上游 Popconfirm `arrow` 槽位。 |
| 相关 API | `IsArrowVisible`、`Placement` |
| 相关 Token | ArrowDecoratedBoxToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.icon`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.icon` |
| Selector | `.semantic-popup-icon` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-icon` |
| Style Type | `PopupConfirmPopupIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `IconPresenter#PART_IconPresenter` |
| 职责 | 确认状态图标，`ConfirmStatus` 通过 `IconBrush` 切换主题/警告/错误色，对应上游 Popconfirm `icon` 槽位。 |
| 相关 API | `Icon`、`ConfirmStatus` |
| 相关 Token | PopupConfirmToken、SharedToken（`IconSizeLG`、`ColorPrimary/ColorWarning/ColorError`） |
| 稳定性 | stable since 6.2.0 |

#### `popup.title`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.title` |
| Selector | `.semantic-popup-title` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-title` |
| Style Type | `PopupConfirmPopupTitleStyle` |
| ContractType | `Avalonia.Controls.TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `TextBlock#PART_Title` |
| 职责 | 确认框标题，对应上游 Popconfirm `title` 槽位；标题文字由 `SharedToken ColorTextHeading` 与 `FontWeight=SemiBold` 承载。 |
| 相关 API | `Title` |
| 相关 Token | PopupConfirmToken（`TitleMargin`）、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.description`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.description` |
| Selector | `.semantic-popup-description` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-description` |
| Style Type | `PopupConfirmPopupDescriptionStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `ContentPresenter#PART_Content` |
| 职责 | 确认描述正文，对应上游 Popconfirm `content` 槽位（AtomUI 因 `popup.content` 已占用而改名为 `popup.description`）。 |
| 相关 API | `ConfirmContent`、`ConfirmContentTemplate` |
| 相关 Token | PopupConfirmToken（`ContentContainerMargin`）、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `popup.actions`

| 字段 | 值 |
| --- | --- |
| Owner | `PopupConfirm` |
| Part | `popup.actions` |
| Selector | `.semantic-popup-actions` |
| SelectorRoute | `>> .semantic-popup-root >> .semantic-popup-actions` |
| Style Type | `PopupConfirmPopupActionsStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `true` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `PopupConfirmContainerTheme.axaml` 的 `StackPanel#PART_ButtonLayout` |
| 职责 | 确认/取消操作区，承载 `PART_CancelButton` 与 `PART_OkButton`；对齐 Alert `actions` 先例，上游未把按钮行发布为语义 key。 |
| 相关 API | `OkText`、`CancelText`、`OkButtonType`、`IsShowCancelButton` |
| 相关 Token | PopupConfirmToken（`ButtonSpacing`、`ButtonContainerMargin`）、SharedToken |
| 稳定性 | stable since 6.2.0 |

## Abstract AXAML Structure

来源：`src/AtomUI.Desktop.Controls/PopupConfirm/Themes/PopupConfirmTheme.axaml`

```xml
<ContentPresenter Name="PART_ContentPresenter" />
```

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
PopupConfirm
  -> PopupConfirmContainer (internal container control theme, PopupConfirmContainerTheme.axaml)
     -> DockPanel#PART_MainLayout (template-stable)
        -> StackPanel#PART_ButtonLayout (template-stable)
           -> Button#PART_CancelButton (template-stable)
           -> Button#PART_OkButton (template-stable)
        -> DockPanel (template-stable)
           -> IconPresenter#PART_IconPresenter (template-stable)
           -> StackPanel (template-stable)
              -> TextBlock#PART_Title (template-stable)
              -> ContentPresenter#PART_Content (template-stable)
  -> PopupConfirm (control theme, PopupConfirmTheme.axaml)
     -> ContentPresenter#PART_ContentPresenter (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `PopupConfirm` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PopupConfirmContainer` | internal container control theme | `PopupConfirmContainerTheme.axaml` | PopupConfirm | `CancelText`, `ClipToBounds`, `ConfirmContent`, `ConfirmContentTemplate`, `Icon`, `IsShowCancelButton` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_MainLayout` | template node (DockPanel) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `CancelText`, `ClipToBounds`, `ConfirmContent`, `ConfirmContentTemplate`, `Icon`, `IsShowCancelButton` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ButtonLayout` | template node (StackPanel) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `CancelText`, `IsShowCancelButton`, `OkButtonType`, `OkText` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_CancelButton` | template node (Button) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `CancelText`, `IsShowCancelButton` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_OkButton` | template node (Button) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `OkButtonType`, `OkText` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `DockPanel` | template node (DockPanel) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `ConfirmContent`, `ConfirmContentTemplate`, `Icon`, `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_IconPresenter` | template node (IconPresenter) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `Icon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `StackPanel` | template node (StackPanel) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `ConfirmContent`, `ConfirmContentTemplate`, `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Title` | template node (TextBlock) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Content` | template node (ContentPresenter) | `PopupConfirmContainerTheme.axaml` | PopupConfirmContainer | `ConfirmContent`, `ConfirmContentTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PopupConfirm` | control theme | `PopupConfirmTheme.axaml` | 用户代码 / 控件宿主 | `ClipToBounds`, `Content`, `ContentTemplate` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PART_ContentPresenter` | template node (ContentPresenter) | `PopupConfirmTheme.axaml` | PopupConfirm | `ClipToBounds`, `Content`, `ContentTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `CancelText`、`ConfirmContent`、`ConfirmContentTemplate`、`Icon`、`OkText`、`Title` | 定义控件展示内容、输入数据、模板或业务对象入口。 |
| 交互与状态 | `ConfirmStatus`、`IsShowCancelButton` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 其他稳定入口 | `OkButtonType` | 保留为 public surface，变更前需确认 Gallery 和用户 XAML 依赖。 |

## Pseudo Classes

| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | input/value。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | PopupConfirm Token + ControlTheme。 |

## State Flow

PopupConfirm 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- input/value 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## Theme and Token Boundaries

PopupConfirm 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `PopupConfirmContainerTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |
| `PopupConfirmTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |

PopupConfirm 使用 `PopupConfirmToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 input/value 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

Token 边界：

PopupConfirm Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `PopupConfirmToken`，scope id 为 `PopupConfirm`，源码位于 `src/AtomUI.Desktop.Controls/PopupConfirm/PopupConfirmToken.cs`。

## Customization Boundaries

维护 PopupConfirm 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 不破坏已发布的 Semantic Part 名称、selector class / route、ContractType 与数量语义（见 [Semantic Part 契约](semantic-part.md)）。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

维护不变量：

维护 PopupConfirm 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- 已发布的 Semantic Part 名称、selector class / route、ContractType 和数量语义（见 [Semantic Part 契约](semantic-part.md)）。
