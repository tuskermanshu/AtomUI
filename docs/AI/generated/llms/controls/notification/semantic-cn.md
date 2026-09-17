# Notification 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

Notification 由两个 public owner 组成，因此公开两个独立 descriptor：

| Owner | 职责 |
| --- | --- |
| `NotificationCard` | 单条通知卡片（上游 notice），承载 `root`、`wrapper`、`icon`、`section`、`title`、`description`、`actions`、`close`、`progress` 九个 Part。 |
| `WindowNotificationManager` | 服务型通知宿主（上游 notification list），承载 `root`（= 上游 `list`）与 `listContent` 两个 Part。 |

上游 `NotificationSemanticType` 的 `classNames` / `styles` 为
`{ list?, listContent?, wrapper?, root?, title?, description?, actions?, icon?, section?, close?, progress? }` 十一个键
（上游 6.6.3 稳定发布源码 `notification/interface.ts`，语义 demo
`notification/demo/_semantic.tsx`）。上游 DOM 嵌套为
`list > listContent > root > [ wrapper > (icon, section > (title, description)), actions, close, progress ]`；
`root`、`icon`、`title`、`description`、`actions` 自 6.0.0 起公开，`wrapper`、`section`、`close`、`progress`、
`list`、`listContent` 自 6.4.0 起公开。

AtomUI 把十一个上游 Part 一一映射到两个 owner 的真实节点：

```text
WindowNotificationManager (root，对应上游 list)
  └─ FeedbackStackPresenter#PART_Items (listContent，公共契约 ItemsControl，Margin 消费 manager.Padding)
       └─ ItemsPresenter / FeedbackStackPanel
            └─ NotificationCard (root，对应上游 notice root)
                 └─ MotionActor
                      └─ Border#Frame (表面投影，Padding=0)
                           └─ FeedbackStackTransitionSnapshotHost#PART_StackTransitionSnapshotHost
                                └─ Grid#PART_Layout
                                     ├─ Border#ContentBox (消费 card.Padding)
                                     │    └─ StackPanel
                                     │         ├─ DockPanel#Wrapper (wrapper)
                                     │         │    ├─ IconPresenter#IconPresenter (icon)
                                     │         │    └─ StackPanel#Section (section)
                                     │         │         ├─ SelectableTextBlock#HeaderTitle (title)
                                     │         │         └─ ContentPresenter#Content (description)
                                     │         └─ ContentPresenter#ActionsContainer (actions)
                                     ├─ IconButton#PART_CloseButton (close，右上角覆盖)
                                     └─ NotificationProgressBar#ProgressBar (progress，运行时创建、跨列贴底)
```

上游 `root` 就是 notification notice 本身，因此映射到 `NotificationCard` owner；上游 `list` 是承载全部 notice 的
定位容器，映射到 `WindowNotificationManager` owner。两个 owner 各自拥有隐式 `root`，不额外声明 `.semantic-root` marker。

结构与布局边界：

- manager 铺满宿主反馈层，安全区外边距由 `TopLevelMarginBinder` 提供；列表内边距由 owner `Padding` 提供，
  投影到 presenter 的 `Margin`。`Position` 决定六种对齐，内联使用时定位作用于调用方分配的布局范围。
- `listContent` 公共类型为 `ItemsControl`。实际 presenter 和 panel 管理可变高度卡片的堆叠、展开与项间距。
- `Frame` 是 `MotionActor` 的直接内容，绘制背景、边框、阴影。card `Padding` 由内部 `ContentBox` 消费；
  `Frame` 自身零内边距，使关闭按钮偏移从边框内侧计算，进度条能够贴底。
- `close` 与内容在 Grid 第一行重叠，右上对齐；`progress` 运行时加入第二行并跨两列，以有限的卡片内容宽度测量。
  两者均不嵌入 `wrapper` / `section` / `actions` 的正文流。快照 host 只拥有折叠过渡期间的内容快照，不公开 Part。
- `wrapper` 使用 DockPanel，icon 的右外边距取 `NotificationIconMargin`，并与标题首行顶部对齐。
  `section` 使用 StackPanel，其 `Spacing` 取 `NotificationSectionSpacing`。

两个 owner 的 descriptor `Since` 统一为 `6.2.0`（AtomUI Semantic Part 首版约定，不逐 Part 记录上游小版本）。

以下类型不持有独立 Semantic descriptor：

- `Notification`（`INotification` 的实现）是服务调用的数据对象，不是 Control。
- `INotification` / `INotificationManager` 是服务契约接口。
- `NotificationPseudoClass`、`NotificationPosition`、`NotificationType` 是状态与枚举类型。
- `NotificationCardToken`、`NotificationProgressBar`、`NotificationProgressBarVisibleConverter` 与各 Motion 类型是
  Token scope、内部进度绘制与动效实现；Semantic Part 不产生 Token identity。

### 1.1 `NotificationCard`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `root` |
| Selector | NotificationCard 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `NotificationCard` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard owner（表面投影到 `Border#Frame`，动效由 `MotionActor` 承载） |
| 职责 | 通知项根元素：承载 `Title`、`Content`、`Icon`、`Actions`、`NotificationType`、`IsClosing`、`IsClosed`、`IsMotionEnabled` 与进入/退出动效；根表面（背景、边框、圆角、阴影、内边距）投影到模板中的 `Border#Frame`。对应上游 notice root。 |
| 相关 API | `Title`、`Content`、`Icon`、`Actions`、`ActionsTemplate`、`NotificationType`、`IsClosing`、`IsClosed`、`IsMotionEnabled`、`Close()`、`NotificationClosed` |
| 相关 Token | `NotificationBg`、`NotificationPadding`、SharedToken（`BoxShadows`、`BorderRadiusLG`） |
| 稳定性 | stable since 6.2.0 |

#### `wrapper`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `wrapper` |
| Selector | `.semantic-wrapper` |
| SelectorRoute | `/template/ .semantic-wrapper` |
| Style Type | `NotificationCardWrapperStyle` |
| ContractType | `DockPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `DockPanel#Wrapper` |
| 职责 | 图标与内容区域的包裹元素：决定 icon 与 section 的排列方向、对齐与图标间距。对应上游 notice wrapper（flex 行，`align-items: flex-start`）。 |
| 相关 API | `Icon`、`Title`、`Content`（决定子节点可见性） |
| 相关 Token | `NotificationIconMargin` |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `NotificationCardIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `IconPresenter#IconPresenter` |
| 职责 | 状态图标元素：尺寸、画刷与行高；`NotificationType` 决定默认图标与状态色（Information = `ColorPrimary`，Success = `ColorSuccess`，Warning = `ColorWarning`，Error = `ColorError`）。对应上游 notice icon。 |
| 相关 API | `Icon`、`NotificationType` |
| 相关 Token | `NotificationIconSize`、`NotificationIconMargin`、SharedToken（`ColorPrimary`、`ColorSuccess`、`ColorWarning`、`ColorError`） |
| 稳定性 | stable since 6.2.0 |

#### `section`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `section` |
| Selector | `.semantic-section` |
| SelectorRoute | `/template/ .semantic-section` |
| Style Type | `NotificationCardSectionStyle` |
| ContractType | `StackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `StackPanel#Section` |
| 职责 | 包含标题与描述的内容区域元素：决定两者的纵向间距与对齐。对应上游 notice section（flex column，`gap: marginXS`）。 |
| 相关 API | `Title`、`Content` |
| 相关 Token | `NotificationSectionSpacing` |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `NotificationCardTitleStyle` |
| ContractType | `AtomUI.Desktop.Controls.SelectableTextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `atom:SelectableTextBlock#HeaderTitle`（AtomUI 类型，主题中带 `atom:` 前缀） |
| 职责 | 标题元素：文本颜色、字号、行高与右侧留白（为关闭按钮预留）。对应上游 notice title。 |
| 相关 API | `Title` |
| 相关 Token | SharedToken（`FontSizeLG`、`FontHeightLG`、`ColorTextHeading`）、`NotificationTitlePadding` |
| 稳定性 | stable since 6.2.0 |

#### `description`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `description` |
| Selector | `.semantic-description` |
| SelectorRoute | `/template/ .semantic-description` |
| Style Type | `NotificationCardDescriptionStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `ContentPresenter#Content` |
| 职责 | 描述元素：正文颜色、字号、行高与换行。对应上游 notice description（在标题下方，不与关闭按钮同排，因此不预留右侧空间）。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | SharedToken（`FontSize`、`FontHeight`、`ColorText`） |
| 稳定性 | stable since 6.2.0 |

#### `actions`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `actions` |
| Selector | `.semantic-actions` |
| SelectorRoute | `/template/ .semantic-actions` |
| Style Type | `NotificationCardActionsStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `ContentPresenter#ActionsContainer` |
| 职责 | 操作组元素：位于 notice 右下角，承载调用方传入的操作内容；`Actions` 为空时节点隐藏。对应上游 notice actions。 |
| 相关 API | `Actions`、`ActionsTemplate` |
| 相关 Token | `NotificationActionsMargin` |
| 稳定性 | stable since 6.2.0 |

#### `close`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `close` |
| Selector | `.semantic-close` |
| SelectorRoute | `/template/ .semantic-close` |
| Style Type | `NotificationCardCloseStyle` |
| ContractType | `IconButton` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | NotificationCard 模板中的 `IconButton#PART_CloseButton` |
| 职责 | 关闭按钮覆盖元素：位置（右上角）、尺寸、圆角、默认/悬停/按下颜色与聚焦样式。对应上游 notice close（absolute top-right）。 |
| 相关 API | `Close()`、`NotificationClosed` |
| 相关 Token | `NotificationCloseButtonSize`、`NotificationCloseButtonPadding`、`NotificationCloseButtonMargin`、SharedToken（`BorderRadiusSM`、`IconSizeSM`、`ColorIcon`、`ColorIconHover`、`ColorBgTextHover`、`ColorBgTextActive`） |
| 稳定性 | stable since 6.2.0 |

#### `progress`

| 字段 | 值 |
| --- | --- |
| Owner | `NotificationCard` |
| Part | `progress` |
| Selector | `.semantic-progress` |
| SelectorRoute | `/template/ .semantic-progress` |
| Style Type | `NotificationCardProgressStyle` |
| ContractType | `Control` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `true` |
| AtomUI 节点 | `NotificationCard` 运行时创建的 `NotificationProgressBar#ProgressBar`（贴卡片底边，左右内缩 `BorderRadiusLG`） |
| 职责 | 进度覆盖元素：展示自动关闭的剩余时间。对应上游 notice progress（absolute bottom）。 |
| 相关 API | `IsShowProgress`、`Expiration` |
| 相关 Token | `NotificationProgressHeight`、`NotificationProgressBg`、`NotificationProgressMargin` |
| 稳定性 | stable since 6.2.0 |

### 1.2 `WindowNotificationManager`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `WindowNotificationManager` |
| Part | `root` |
| Selector | WindowNotificationManager 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `WindowNotificationManager` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | WindowNotificationManager owner（宿主层/full-screen 覆盖层；无宿主构造时为内联可放置实例） |
| 职责 | 通知列表根元素：承载 `Position`、`MaxItems`、`IsMotionEnabled`、`IsPauseOnHover`，管理宿主层安装、通知队列、超时关闭与宿主 detach；对应上游 `list` 的定位/层级/宽度与边缘内边距语义。 |
| 相关 API | `Position`、`MaxItems`、`IsMotionEnabled`、`IsPauseOnHover`、`Padding`、`IsStackEnabled`、`StackThreshold`、`Show(INotification)`、`DestroyAll()`、`Dispose()` |
| 相关 Token | `NotificationTopMargin`、`NotificationBottomMargin`、SharedToken（`EnableMotion`） |
| 稳定性 | stable since 6.2.0 |

#### `listContent`

| 字段 | 值 |
| --- | --- |
| Owner | `WindowNotificationManager` |
| Part | `listContent` |
| Selector | `.semantic-list-content` |
| SelectorRoute | `/template/ .semantic-list-content` |
| Style Type | `WindowNotificationManagerListContentStyle` |
| ContractType | `ItemsControl` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | WindowNotificationManager 模板中的 `FeedbackStackPresenter#PART_Items` |
| 职责 | 通知列表内容元素：notice 的排列方向、顺序、对齐与项间距；对应上游 `listContent` 的 notice 排列/间距语义。 |
| 相关 API | `Position`、`MaxItems`、`IsStackEnabled`、`StackThreshold` |
| 相关 Token | SharedToken（`EnableMotion`、`UniformlyMargin`） |
| 稳定性 | stable since 6.2.0 |

隐式 `root` 不声明 `.semantic-root` marker。`NotificationCard` 的 `wrapper`、`icon`、`section`、`title`、
`description`、`actions`、`close` marker 静态声明在 `NotificationCardTheme.axaml` 模板内；`progress` 由控件运行时
创建并在创建路径注入 semantic class；`WindowNotificationManager` 的 `listContent` marker 静态声明在
`WindowNotificationManagerTheme.axaml` 模板内。

## Abstract AXAML Structure

未定位到可生成抽象 AXAML 结构的 ControlTheme 模板。生成器不会根据 semantic parts 发明 AXAML 节点；请以 Template Parts、主题文件和源码索引为准。

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
Notification
  -> NotificationCard (control theme, NotificationCardTheme.axaml)
     -> MotionActor#{x:Static atom:BaseMotionActor.MotionActorPart} (internal-observable)
        -> Border#Frame (template-stable)
           -> FeedbackStackTransitionSnapshotHost#PART_StackTransitionSnapshotHost (template-stable)
              -> Grid#PART_Layout (template-stable)
                 -> Border#ContentBox (template-stable)
                    -> StackPanel (template-stable)
                       -> DockPanel#Wrapper (template-stable)
                          -> IconPresenter#IconPresenter (internal-observable)
                          -> StackPanel#Section (template-stable)
                             -> SelectableTextBlock#HeaderTitle (template-stable)
                             -> ContentPresenter#Content (internal-observable)
                       -> ContentPresenter#ActionsContainer (internal-observable)
                 -> IconButton#PART_CloseButton (template-stable)
  -> NotificationProgressBar (control theme, NotificationProgressBarTheme.axaml)
  -> FeedbackStackPresenter (presenter control theme, FeedbackStackPresenterTheme.axaml)
     -> Border (template-stable)
        -> ItemsPresenter#PART_ItemsPresenter (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `Notification` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `NotificationCard` | control theme | `NotificationCardTheme.axaml` | 用户代码 / 控件宿主 | `Actions`, `ActionsTemplate`, `Background`, `BorderBrush`, `BorderThickness`, `BoxShadow` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `{x:Static atom:BaseMotionActor.MotionActorPart}` | template node (MotionActor) | `NotificationCardTheme.axaml` | NotificationCard | `Actions`, `ActionsTemplate`, `Background`, `BorderBrush`, `BorderThickness`, `BoxShadow` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `Frame` | template node (Border) | `NotificationCardTheme.axaml` | NotificationCard | `Actions`, `ActionsTemplate`, `Background`, `BorderBrush`, `BorderThickness`, `BoxShadow` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_StackTransitionSnapshotHost` | template node (FeedbackStackTransitionSnapshotHost) | `NotificationCardTheme.axaml` | NotificationCard | `Actions`, `ActionsTemplate`, `Content`, `ContentTemplate`, `Icon`, `Padding` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Layout` | template node (Grid) | `NotificationCardTheme.axaml` | NotificationCard | `Actions`, `ActionsTemplate`, `Content`, `ContentTemplate`, `Icon`, `Padding` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ContentBox` | template node (Border) | `NotificationCardTheme.axaml` | NotificationCard | `Actions`, `ActionsTemplate`, `Content`, `ContentTemplate`, `Icon`, `Padding` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `StackPanel` | template node (StackPanel) | `NotificationCardTheme.axaml` | NotificationCard | `Actions`, `ActionsTemplate`, `Content`, `ContentTemplate`, `Icon`, `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `Wrapper` | template node (DockPanel) | `NotificationCardTheme.axaml` | NotificationCard | `Content`, `ContentTemplate`, `Icon`, `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `IconPresenter` | template node (IconPresenter) | `NotificationCardTheme.axaml` | NotificationCard | `Icon` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `Section` | template node (StackPanel) | `NotificationCardTheme.axaml` | NotificationCard | `Content`, `ContentTemplate`, `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `HeaderTitle` | template node (SelectableTextBlock) | `NotificationCardTheme.axaml` | NotificationCard | `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `Content` | template node (ContentPresenter) | `NotificationCardTheme.axaml` | NotificationCard | `Content`, `ContentTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `ActionsContainer` | template node (ContentPresenter) | `NotificationCardTheme.axaml` | NotificationCard | `Actions`, `ActionsTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_CloseButton` | template node (IconButton) | `NotificationCardTheme.axaml` | NotificationCard | 主题状态 / visual state | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `NotificationProgressBar` | control theme | `NotificationProgressBarTheme.axaml` | Notification | 主题状态 / visual state | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `FeedbackStackPresenter` | presenter control theme | `FeedbackStackPresenterTheme.axaml` | Notification | `ItemsPanel` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_ItemsPresenter` | template node (ItemsPresenter) | `FeedbackStackPresenterTheme.axaml` | FeedbackStackPresenter | `ItemsPanel` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Show(INotification, string[]?)`、`MaxItems` | 创建通知并约束活动项上限；`MaxItems <= 0` 表示不限制。 |
| Stack | `IsStackEnabled`、`StackThreshold`、`IsPauseOnHover` | 控制阈值折叠、整体 hover 展开和生命周期暂停。 |
| 交互与状态 | `DestroyAll()`、`IsClosed`、`IsClosing`、`IsMotionEnabled`、`IsShowProgress` | 清空活动通知，并表达卡片关闭、进度和动效状态。 |
| 视觉与布局 | `Position`、`ProgressIndicatorBrush`、`ProgressIndicatorThickness` | 默认 `TopRight`；控制宿主位置和进度视觉。 |
| 内容对象 | `Expiration`、`NotificationType`、`Title`、`Content`、`Icon` | 表达正文、类型、自动关闭时长与一次性关闭回调。 |

## Pseudo Classes

| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 自动关闭、进度、hover 暂停、Stack 展开/折叠和关闭 motion。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Notification Token + ControlTheme。 |

## State Flow

Notification 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、loading/async、collection/filter、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

## Theme and Token Boundaries

Notification 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `NotificationCardTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `NotificationProgressBarTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `WindowNotificationManagerTheme.axaml` | 定义弹层、窗口或 overlay 宿主视觉。 |

Notification 使用 internal `NotificationCardToken` 作为控件 Token scope。Token 只表达卡片背景、尺寸、padding、图标、进度和外部间距等视觉语义，不承载 Stack、剩余时长或关闭状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

列表的公开样式入口是 `WindowNotificationManager.Padding` 与生成的 `WindowNotificationManagerListContentStyle`。
前者控制队列边缘间隔；后者以 `ItemsControl` 为公共目标，支持宽度、最小宽度与对齐等属性。
项间距和卡片顺序由内部反馈栈管理，不能以 `Spacing` / `ReverseOrder` 作为 listContent 的公开 Setter。

Token 边界：

Notification Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- internal `NotificationCardToken`，scope id 为 `NotificationCard`，源码位于 `src/AtomUI.Desktop.Controls/Notifications/NotificationCardToken.cs`。

## Customization Boundaries

维护 Notification 时必须保持以下不变量：

- Stack API、默认值与共享基础设施文档构成当前契约；后续不得仅修改 Notification 一侧而造成两个管理器同名 API 语义分叉。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

维护不变量：

维护 Notification 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- `IsClosing` / `IsClosed` public 状态不得与 internal `MotionExecutionState` 合并；重复调度不得创建并行退出动效。
- `MaxItems <= 0` 必须保持无限语义；Stack 不能通过提前关闭旧项模拟折叠。
- template detach、rehost、DestroyAll、用户回调异常与 dispose 均必须释放 scheduler entry、presenter、host 订阅、card owner 与 delegate。
