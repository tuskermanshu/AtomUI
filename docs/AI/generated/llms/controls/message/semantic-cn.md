# Message 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

Message 由两个 public owner 组成，因此公开两个独立 descriptor：

| Owner | 职责 |
| --- | --- |
| `MessageCard` | 单条消息卡片（上游 notice），承载 `root`、`wrapper`、`icon`、`title` 四个 Part。 |
| `WindowMessageManager` | 服务型消息宿主（上游 message list），承载 `root`（= 上游 `list`）与 `listContent` 两个 Part。 |

上游 `MessageSemanticType` 的 `classNames` / `styles` 均为
`{ root?, wrapper?, icon?, title?, list?, listContent? }` 六个键（上游 6.6.3 稳定发布源码
`message/interface.ts`，语义 demo `message/demo/_semantic.tsx`）。上游 DOM 嵌套为
`list > listContent > root > wrapper > [icon, title]`；`root` 自 6.0.0 起公开，`wrapper`、`title`、`list`、
`listContent` 自 6.4.0 起公开。

AtomUI 把六个上游 Part 一一映射到两个 owner 的真实节点：

```text
WindowMessageManager (root，对应上游 list)
  └─ Grid#PART_StackHost (按 Position 锚定，Margin 消费 manager.Padding)
       ├─ Panel (零尺寸、不可命中的两层静态背板)
       └─ FeedbackStackPresenter#PART_Items (listContent，公共契约 ItemsControl)
            └─ ItemsPresenter / FeedbackStackPanel
                 └─ MessageCard (root，对应上游 notice root)
                      └─ MotionActor
                           └─ Border#PART_Frame (root 表面投影)
                                └─ DockPanel#PART_HeaderContainer (wrapper)
                                     ├─ IconPresenter#PART_IconContent (icon)
                                     └─ SelectableTextBlock#PART_Message (title)
```

上游 `root` 就是 message notice 本身，因此映射到 `MessageCard` owner；上游 `list` 是承载全部 notice 的定位容器，
映射到 `WindowMessageManager` owner。两个 owner 各自拥有隐式 `root`，不额外声明 `.semantic-root` marker。

结构与布局边界：

- manager 的 `Position` 决定六种边缘对齐，`Padding` 决定队列与 manager 边缘的间隔。
  宿主模式中 manager 铺满反馈层并通过 `TopLevelMarginBinder` 接收安全区外边距；内联模式中同样的定位作用于
  调用方分配的布局范围。模板中的 `PART_StackHost` 按位置锚定并紧贴队列，`listContent` 填满该 host。
- `listContent` 的公共类型为 `ItemsControl`；实际 presenter 和其内部 panel 管理堆叠、顺序与项间距。
  这两个内部类型及其专属属性不是应用的样式契约。
- 上游 `wrapper` 用 flex `gap: marginXS` + `align-items: center` 排列 icon 与 title；AtomUI `DockPanel` 无 `Spacing`，
  等价的图标间距由 `IconPresenter` 的 `MessageIconMargin`（右外边距 `UniformlyMarginXS`）表达，视觉结果一致。

两个 owner 的 descriptor `Since` 统一为 `6.0`（AtomUI Semantic Part 首版约定，不逐 Part 记录上游小版本）。

以下类型不持有独立 Semantic descriptor：

- `Message`（`IMessage` 的实现）是服务调用的数据对象，不是 Control。
- `IMessage` / `IMessageManager` 是服务契约接口。
- `MessageCardPseudoClass`、`MessageType` 是状态与枚举类型。
- `MessageCardToken` 是 Token scope；Semantic Part 不产生 Token identity。

### 1.1 `MessageCard`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `MessageCard` |
| Part | `root` |
| Selector | MessageCard 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `MessageCard` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | MessageCard owner（表面投影到 `Border#PART_Frame`，动效由 `MotionActor` 承载） |
| 职责 | 单条消息项根元素：承载 `Message`、`MessageType`、`Icon`、`IsClosing`、`IsClosed`、`IsMotionEnabled` 与进入/退出动效；根表面（背景、圆角、阴影、内边距）投影到模板中的 `Border#PART_Frame`。对应上游 notice root。 |
| 相关 API | `Message`、`MessageType`、`Icon`、`IsClosing`、`IsClosed`、`IsMotionEnabled`、`Close()`、`MessageClosed` |
| 相关 Token | `ContentBg`、`ContentPadding`、SharedToken（`BoxShadows`、`BorderRadiusLG`） |
| 稳定性 | stable since 6.0 |

#### `wrapper`

| 字段 | 值 |
| --- | --- |
| Owner | `MessageCard` |
| Part | `wrapper` |
| Selector | `.semantic-wrapper` |
| SelectorRoute | `/template/ .semantic-wrapper` |
| Style Type | `MessageCardWrapperStyle` |
| ContractType | `DockPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | MessageCard 模板中的 `DockPanel#PART_HeaderContainer` |
| 职责 | 图标与标题的包裹元素：决定 icon/title 的排列方向、对齐与图标间距。对应上游 notice wrapper。 |
| 相关 API | `Icon`、`Message`（决定子节点可见性） |
| 相关 Token | `MessageIconMargin` |
| 稳定性 | stable since 6.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `MessageCard` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `MessageCardIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | MessageCard 模板中的 `IconPresenter#PART_IconContent` |
| 职责 | 状态图标元素：尺寸、画刷与行高；`MessageType` 决定默认图标与状态色（Information/Loading = `ColorPrimary`，Success = `ColorSuccess`，Warning = `ColorWarning`，Error = `ColorError`）。对应上游 notice icon。 |
| 相关 API | `Icon`、`MessageType` |
| 相关 Token | `MessageIconSize`、SharedToken（`ColorPrimary`、`ColorSuccess`、`ColorWarning`、`ColorError`） |
| 稳定性 | stable since 6.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `MessageCard` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `MessageCardTitleStyle` |
| ContractType | `Avalonia.Controls.SelectableTextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | MessageCard 模板中的 `SelectableTextBlock#PART_Message`（Avalonia 类型，非 `atom:` 前缀） |
| 职责 | 消息文本元素：文本颜色、字号、行高、换行与文本选择样式。对应上游 notice title（上游把 `content` 作为 notice title 渲染）。 |
| 相关 API | `Message` |
| 相关 Token | SharedToken（`FontSize`、`FontHeight`、`ColorText`、`SelectionBackground`、`SelectionForeground`） |
| 稳定性 | stable since 6.0 |

### 1.2 `WindowMessageManager`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `WindowMessageManager` |
| Part | `root` |
| Selector | WindowMessageManager 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `WindowMessageManager` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | WindowMessageManager owner（宿主层/full-screen 覆盖层；无宿主构造时为内联可放置实例） |
| 职责 | 消息列表根元素：承载 `Position`、`MaxItems`、`IsMotionEnabled`，管理宿主层安装、消息队列、超时关闭与宿主 detach；对应上游 `list` 的定位/层级/宽度语义。 |
| 相关 API | `Position`、`MaxItems`、`IsMotionEnabled`、`Padding`、`IsStackEnabled`、`StackThreshold`、`IsPauseOnHover`、`Show(IMessage)`、`DestroyAll()`、`Dispose()` |
| 相关 Token | `MessageTopMargin`、SharedToken（`EnableMotion`） |
| 稳定性 | stable since 6.0 |

#### `listContent`

| 字段 | 值 |
| --- | --- |
| Owner | `WindowMessageManager` |
| Part | `listContent` |
| Selector | `.semantic-list-content` |
| SelectorRoute | `/template/ .semantic-list-content` |
| Style Type | `WindowMessageManagerListContentStyle` |
| ContractType | `ItemsControl` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | WindowMessageManager 模板中的 `FeedbackStackPresenter#PART_Items` |
| 职责 | 消息列表内容元素：notice 的排列方向、顺序与对齐；对应上游 `listContent` 的 notice 排列/间距语义。 |
| 相关 API | `Position`、`MaxItems`、`IsStackEnabled`、`StackThreshold` |
| 相关 Token | SharedToken（`EnableMotion`） |
| 稳定性 | stable since 6.0 |

隐式 `root` 不声明 `.semantic-root` marker。`MessageCard` 的 `wrapper`、`icon`、`title` marker 静态声明在
`MessageCardTheme.axaml` 模板内；`WindowMessageManager` 的 `listContent` marker 静态声明在
`WindowMessageManagerTheme.axaml` 模板内。四个 marker 都在 owner 自身的 `ControlTheme` 资产中，因此两个 descriptor
都不需要 `RuntimeCreated=true`，生成器按 owner 主题资产做静态校验。

## Abstract AXAML Structure

未定位到可生成抽象 AXAML 结构的 ControlTheme 模板。生成器不会根据 semantic parts 发明 AXAML 节点；请以 Template Parts、主题文件和源码索引为准。

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
Message
  -> MessageCard (control theme, MessageCardTheme.axaml)
     -> MotionActor#{x:Static atom:BaseMotionActor.MotionActorPart} (internal-observable)
        -> Border#PART_Frame (template-stable)
           -> DockPanel#PART_HeaderContainer (template-stable)
              -> IconPresenter#PART_IconContent (template-stable)
              -> SelectableTextBlock#PART_Message (template-stable)
  -> FeedbackStackPresenter (presenter control theme, FeedbackStackPresenterTheme.axaml)
     -> Border (template-stable)
        -> ItemsPresenter#PART_ItemsPresenter (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `Message` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `MessageCard` | control theme | `MessageCardTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `BorderBrush`, `BorderThickness`, `BoxShadow`, `CornerRadius`, `Icon` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `{x:Static atom:BaseMotionActor.MotionActorPart}` | template node (MotionActor) | `MessageCardTheme.axaml` | MessageCard | `Background`, `BorderBrush`, `BorderThickness`, `BoxShadow`, `CornerRadius`, `Icon` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_Frame` | template node (Border) | `MessageCardTheme.axaml` | MessageCard | `Background`, `BorderBrush`, `BorderThickness`, `BoxShadow`, `CornerRadius`, `Icon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_HeaderContainer` | template node (DockPanel) | `MessageCardTheme.axaml` | MessageCard | `Icon`, `Message` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_IconContent` | template node (IconPresenter) | `MessageCardTheme.axaml` | MessageCard | `Icon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Message` | template node (SelectableTextBlock) | `MessageCardTheme.axaml` | MessageCard | `Message` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `FeedbackStackPresenter` | presenter control theme | `FeedbackStackPresenterTheme.axaml` | Message | `ItemsPanel` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_ItemsPresenter` | template node (ItemsPresenter) | `FeedbackStackPresenterTheme.axaml` | FeedbackStackPresenter | `ItemsPanel` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Show(IMessage, string[]?)`、`MaxItems` | 创建消息并约束活动项上限；`MaxItems <= 0` 表示不限制。 |
| Stack | `IsStackEnabled`、`StackThreshold`、`IsPauseOnHover` | 默认关闭；控制阈值折叠、整体 hover 展开和实际 hover 期间的生命周期暂停，不改变消息时长。 |
| 交互与状态 | `DestroyAll()`、`IsClosed`、`IsClosing`、`IsMotionEnabled` | 清空活动消息，并表达卡片关闭与动效状态。 |
| 视觉与布局 | `Position` | 默认为 `TopCenter`，决定宿主边和横向对齐。 |
| 内容对象 | `Message`、`MessageType`、`IMessage.Expiration` | 表达正文、类型、图标、自动关闭时长与一次性关闭回调。 |

## Pseudo Classes

| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 自动关闭、hover 暂停、Stack 展开/折叠和关闭 motion。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Message Token + ControlTheme。 |

## State Flow

Message 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- collection/filter、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## Theme and Token Boundaries

Message 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `MessageCardTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `WindowMessageManagerTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |

Message 使用 internal `MessageCardToken` 作为控件 Token scope。Token 只表达卡片背景、padding、图标与外部间距等组件视觉语义，不承载 Stack、剩余时长或关闭状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

列表的公开样式入口是 `WindowMessageManager.Padding` 与生成的 `WindowMessageManagerListContentStyle`。
前者控制队列边缘间隔；后者以 `ItemsControl` 为公共目标，支持宽度、最小宽度与对齐等属性。
项间距和卡片顺序由内部反馈栈管理，不能以 `Spacing` / `ReverseOrder` 作为 listContent 的公开 Setter。

Token 边界：

Message Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- internal `MessageCardToken`，scope id 为 `MessageCard`，源码位于 `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`。

## Customization Boundaries

维护 Message 时必须保持以下不变量：

- Stack API、默认值与共享基础设施文档构成当前契约；后续不得仅修改 Message 一侧而造成两个管理器同名 API 语义分叉。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

维护不变量：

维护 Message 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- `IsClosing` / `IsClosed` public 状态不得与 internal `MotionExecutionState` 合并；重复调度不得创建并行退出动效。
- `MaxItems <= 0` 必须保持无限语义；Stack 不能通过提前关闭旧项模拟折叠。
- Stack 不能隐式把有限时长改为永久展示，也不能在未 hover 时暂停 scheduler；Gallery 的永久 Stack 示例必须通过
  `Expiration=TimeSpan.Zero` 显式表达。
- template detach、rehost、DestroyAll、用户回调异常与 dispose 均必须释放 scheduler entry、presenter、host 订阅、card owner 与 delegate。
