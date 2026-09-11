# Notification Semantic Part 契约

本文档定义 Notification 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Notification 的整体设计见
[Notification 桌面版架构设计](overview.md)，内部实现原理见 [Notification 桌面版实现原理](implementation.md)，Token 设计见
[Notification Token 设计](token.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

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
  └─ Border（消费 owner Padding）
       └─ ReversibleStackPanel#PART_Items (listContent, .semantic-list-content)
            └─ NotificationCard (root，对应上游 notice root)
                 └─ LayoutAwareMotionActor
                      └─ Border#Frame (root 表面投影：背景/边框/圆角/阴影；MotionActor 直接内容，Padding=0)
                           └─ Panel#PART_Layout (等于 padding box：Frame 无内边距)
                                ├─ Border#ContentBox (消费 owner Padding，即上/下 paddingMD、左/右 paddingLG)
                                │    └─ StackPanel
                                │         ├─ DockPanel#Wrapper (wrapper, .semantic-wrapper)
                                │         │    ├─ IconPresenter#IconPresenter (icon, .semantic-icon)
                                │         │    └─ StackPanel#Section (section, .semantic-section)
                                │         │         ├─ SelectableTextBlock#HeaderTitle (title, .semantic-title)
                                │         │         └─ ContentPresenter#Content (description, .semantic-description)
                                │         └─ ContentPresenter#ActionsContainer (actions, .semantic-actions)
                                ├─ IconButton#PART_CloseButton (close, .semantic-close，覆盖层)
                                └─ NotificationProgressBar#ProgressBar (progress, .semantic-progress，运行时创建覆盖层)
```

上游 `root` 就是 notification notice 本身，因此映射到 `NotificationCard` owner；上游 `list` 是承载全部 notice 的
定位容器，映射到 `WindowNotificationManager` owner。两个 owner 各自拥有隐式 `root`，不额外声明 `.semantic-root` marker。

与上游 DOM 的结构差异（Part 名称、数量与样式语义不变）：

- 上游 `list` 自身承担 placement（`topRight` 等）；AtomUI 的 `WindowNotificationManager` 铺满 TopLevel 并只负责安全区
  外边距与列表内边距，具体对齐由 `ReversibleStackPanel#PART_Items`（`listContent`）实际承担。因此"放置"语义在
  AtomUI 由 `root`（`Position` 属性与宿主层范围的作用域）与 `listContent`（实际排列容器）共同表达。
- 上游 notice 的 `close` 是 `position: absolute` 覆盖层，`progress` 是 `position: absolute; bottom: 0` 覆盖层。
  AtomUI 同样把两者表达为 `Panel#PART_Layout` 上的覆盖元素（`close` 右上、`progress` 贴底并左右内缩 `BorderRadiusLG`），
  不参与 `wrapper` / `section` / `actions` 的流式排版。
- 上游 `position: absolute` 的子元素相对**padding box** 定位，而非 border box 或内容盒。因此 AtomUI 把
  `Padding` 从 `Border#Frame` 下移到 Frame 内部的 `Border#ContentBox`，让 `Panel#PART_Layout` 恰好等于
  padding box：`close` 的右侧偏移量到边框内侧、`progress` 贴在边框底边（而不是被 20px 内边距抬高）。
  `Frame` 自身保持 `Padding=0`，同时负责画 `BoxShadow`。
- `Border#Frame` 必须是 `LayoutAwareMotionActor` 的直接内容（中间只允许 `ContentControl` 自带的
  `PART_ContentPresenter`）。曾经的模板在两者之间夹了一层 `Panel#PART_Layout`，`BoxShadow` 被裁到右/下各只剩
  1 个逻辑像素（实测设备像素 2，上游为 8），卡片右下角看起来「少了一块」。`Message` 的 `PART_Frame` 正是
  MotionActor 的直接内容，所以它的硬阴影一直正常；本控件的回归测试
  `Frame_Sits_Directly_In_The_Motion_Actor_So_BoxShadow_Is_Not_Clipped` 锁定该约束。
- 上游 `wrapper` 用 flex `gap: marginSM` + `align-items: flex-start` 排列 icon 与 section；AtomUI `DockPanel` 无
  `Spacing`，等价的图标间距由 `IconPresenter` 的 `NotificationIconMargin`（右外边距）表达，视觉结果一致。
- 上游 `section` 用 flex column `gap: marginXS`；AtomUI 由 `StackPanel#Section.Spacing` 表达。

两个 owner 的 descriptor `Since` 统一为 `6.0`（AtomUI Semantic Part 首版约定，不逐 Part 记录上游小版本）。

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
| AtomUI 节点 | NotificationCard owner（表面投影到 `Border#Frame`，动效由 `LayoutAwareMotionActor` 承载） |
| 职责 | 通知项根元素：承载 `Title`、`Content`、`Icon`、`Actions`、`NotificationType`、`IsClosing`、`IsClosed`、`IsMotionEnabled` 与进入/退出动效；根表面（背景、边框、圆角、阴影、内边距）投影到模板中的 `Border#Frame`。对应上游 notice root。 |
| 相关 API | `Title`、`Content`、`Icon`、`Actions`、`ActionsTemplate`、`NotificationType`、`IsClosing`、`IsClosed`、`IsMotionEnabled`、`Close()`、`NotificationClosed` |
| 相关 Token | `NotificationBg`、`NotificationPadding`、SharedToken（`BoxShadows`、`BorderRadiusLG`） |
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 稳定性 | stable since 6.0 |

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
| 相关 API | `Position`、`MaxItems`、`IsMotionEnabled`、`IsPauseOnHover`、`Show(INotification)`、`Dispose()` |
| 相关 Token | `NotificationPadding`（列表内边距）、SharedToken（`EnableMotion`、`MarginLG`） |
| 稳定性 | stable since 6.0 |

#### `listContent`

| 字段 | 值 |
| --- | --- |
| Owner | `WindowNotificationManager` |
| Part | `listContent` |
| Selector | `.semantic-list-content` |
| SelectorRoute | `/template/ .semantic-list-content` |
| Style Type | `WindowNotificationManagerListContentStyle` |
| ContractType | `ReversibleStackPanel` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | WindowNotificationManager 模板中的 `ReversibleStackPanel#PART_Items` |
| 职责 | 通知列表内容元素：notice 的排列方向、顺序、对齐与项间距；对应上游 `listContent` 的 notice 排列/间距语义。 |
| 相关 API | `Position`、`MaxItems`（决定内容区可见项数量） |
| 相关 Token | SharedToken（`EnableMotion`、`UniformlyMargin`） |
| 稳定性 | stable since 6.0 |

隐式 `root` 不声明 `.semantic-root` marker。`NotificationCard` 的 `wrapper`、`icon`、`section`、`title`、
`description`、`actions`、`close` marker 静态声明在 `NotificationCardTheme.axaml` 模板内；`progress` 由控件运行时
创建并在创建路径注入 semantic class；`WindowNotificationManager` 的 `listContent` marker 静态声明在
`WindowNotificationManagerTheme.axaml` 模板内。

## 2. Part 说明

### 2.1 NotificationCard root

`root` 是 `NotificationCard` owner 本身，每条可见通知恰好一个。它承载 `Title`、`Content`、`Icon`、`Actions`、
`NotificationType` 内容/状态 API，`IsClosing` / `IsClosed` 关闭状态，以及 `IsMotionEnabled` 与进出场动效。

它负责：

- 单条通知的根语义与状态机：`Close()` 置 `IsClosing`，关闭动效完成后提交一次 `IsClosed=true` 并抛出
  `NotificationClosed`。
- 根视觉表面：背景（`NotificationBg`）、圆角（`BorderRadiusLG`）、阴影（`BoxShadows`）投影到 `Border#Frame`；
  内边距（`NotificationPadding`）投影到 `Frame` 内部的 `Border#ContentBox`（原因见 §1 的 padding box 说明）。
  卡片**不承担**通知间距：`root` 的几何等于可见卡片本身（四边零外边距），
  通知间距由 `listContent` 的 `Spacing` 表达（见 §5）。
- 作为 `wrapper` / `icon` / `section` / `title` / `description` / `actions` / `close` / `progress`
  owner-scoped Selector 的作用域边界。

适合通过 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`BoxShadow`、`Padding` 等 owner 侧 Setter
定制整卡视觉——背景/圆角/阴影/边框经模板内 `Border#Frame` 的 `TemplateBinding` 投影到卡片外框，`Padding` 经
`Border#ContentBox` 投影到内容内边距，是上游 `root` styles
（背景色、圆角、阴影、内边距、边框）的 AtomUI 等价表达。`root` 不表示模板中的 `LayoutAwareMotionActor`、
`Border#Frame` 或 `Panel#PART_Layout` 节点本身。

### 2.2 wrapper / icon / section / title / description / actions

六个 Part 在 `NotificationCard` 模板内各对应一个常驻节点，cardinality 均为 `Single`：

- `wrapper` 承载 `DockPanel#Wrapper`：icon 停靠左侧、section 填充剩余宽度；`NotificationIconMargin` 提供图标与
  文本之间的间距。适合定制 `Padding`、`Margin`、`HorizontalAlignment` 等布局属性。
- `icon` 承载 `IconPresenter#IconPresenter`：`Icon` 为空且 `NotificationType` 为默认时隐藏；设置类型或自定义
  `Icon` 时显示。适合定制 `Width`、`Height`、`Margin`、`IconBrush`。
- `section` 承载 `StackPanel#Section`：标题与描述的共同父级，`NotificationSectionSpacing` 提供两者纵向间距。
  适合定制 `Spacing`、`Orientation` 等布局属性。
- `title` 承载 `atom:SelectableTextBlock#HeaderTitle`：`Title` 文本呈现，支持文本选择。适合定制 `Foreground`、
  `FontSize`、`LineHeight`、`Padding`。
- `description` 承载 `ContentPresenter#Content`：`Content` 呈现，支持 `ContentTemplate` 与文本换行。适合定制
  `Foreground`、`FontSize`、`LineHeight`、`Padding`。
- `actions` 承载 `ContentPresenter#ActionsContainer`：`Actions` 为空时整体隐藏；非空时按默认右上角对齐显示。
  适合定制 `Margin`、`HorizontalAlignment` 与内容模板。

`icon` 的可见性由 `Icon` / `NotificationType` 数据状态驱动；Semantic Style 覆盖 `IsVisible` 会绕过数据状态机，属于
不推荐用法。

### 2.3 close / progress

两个 Part 是覆盖层，cardinality 均为 `Single`：

- `close` 承载 `IconButton#PART_CloseButton`，放在 `Panel#PART_Layout` 上并以右上角对齐，因此不占用 `wrapper` /
  `actions` 的排版空间，对齐上游 absolute top-right。适合定制位置（`Margin`）、尺寸、圆角与交互色。
- `progress` 由 `ConfigureProgressBar` 运行时创建 `NotificationProgressBar#ProgressBar` 并注入
  `semantic-progress` class，只在 `IsShowProgress` 为真且 `Expiration` 非空时存在；节点贴卡片底边、左右内缩
  `BorderRadiusLG`，对齐上游 absolute bottom。`IsShowProgress` 关闭或 `Expiration` 清空时节点与 marker 一起释放。
  适合定制 `Margin`、`Height`（通过 `NotificationProgressHeight`）与进度画刷。

### 2.4 WindowNotificationManager root / listContent

两个 Part 在 `WindowNotificationManager` 内各对应一个稳定节点，cardinality 均为 `Single`：

- `root` 承载 manager owner 本身：安装到 TopLevel 宿主层（`WindowFeedbackLayer` 优先，回退 `AdornerLayer`）时它是
  full-screen 覆盖层，负责安全区外边距、列表内边距（`NotificationPadding`）、通知队列（`_pendingNotifications`）、
  过期计时器（`_cardExpiredTimer`）、`MaxItems` 清理与 `Dispose` 卸载。
- `listContent` 承载 `ReversibleStackPanel#PART_Items`：所有 `NotificationCard` 的逻辑父级，决定 notice 的排列方向、
  `ReverseOrder`、对齐与 `Spacing`。

适合通过 `root` 定制 manager 的内边距与宿主层相关视觉，通过 `listContent` 定制 notice 排列间距与对齐。

## 3. Selector 用法

应用级样式先限定 owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护和
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|NotificationCard">
        <atom:NotificationCardWrapperStyle x:SetterTargetType="DockPanel">
            <Setter Property="Margin" Value="0" />
        </atom:NotificationCardWrapperStyle>
        <atom:NotificationCardIconStyle x:SetterTargetType="atom:IconPresenter">
            <Setter Property="IconBrush" Value="#1677FF" />
        </atom:NotificationCardIconStyle>
        <atom:NotificationCardTitleStyle x:SetterTargetType="atom:SelectableTextBlock">
            <Setter Property="FontWeight" Value="SemiBold" />
        </atom:NotificationCardTitleStyle>
        <atom:NotificationCardDescriptionStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Foreground" Value="#3F6600" />
        </atom:NotificationCardDescriptionStyle>
    </Style>

    <Style Selector="atom|WindowNotificationManager">
        <atom:WindowNotificationManagerListContentStyle x:SetterTargetType="atom:ReversibleStackPanel">
            <Setter Property="Spacing" Value="12" />
        </atom:WindowNotificationManagerListContentStyle>
    </Style>
</Application.Styles>
```

对特定 class 或状态定制时，把 class、属性或伪类放在 owner 一侧：

```xml
<Style Selector="atom|NotificationCard:error">
    <atom:NotificationCardIconStyle x:SetterTargetType="atom:IconPresenter">
        <Setter Property="IconBrush" Value="#CF1322" />
    </atom:NotificationCardIconStyle>
</Style>
```

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `atom|IconPresenter.semantic-icon` 或 `:is(atom|IconPresenter).semantic-icon`。
- 直接复制 `/template/ .semantic-wrapper` 等 route 作为用户主路径；route 只属于 descriptor 与生成 Style 的
  实现元数据。
- 依赖 `PART_*`、`Name` 或视觉祖先顺序。
- 通过 Semantic Style 设置 `icon` 的 `IsVisible` 绕过 `Icon` / `NotificationType` 数据状态机。
- 通过 Semantic Style 设置 `progress` 的存在性；该节点由 `IsShowProgress` 与 `Expiration` 数据状态决定。

## 4. 状态与数量语义

数量契约以已实例化的 AtomUI 内置节点为边界。除 `progress` 外，各 Part 都是 owner 自身 `ControlTheme` 的静态模板
节点（`RuntimeCreated=false`），marker 随 owner 实例存在，不随数据项迁移；`progress` 是 RuntimeCreated Part，
只在进度可见时存在。

| 场景 | NotificationCard 各 Part | WindowNotificationManager root / listContent | 说明 |
| --- | --- | --- | --- |
| 单条通知 | 各 1 | 各 1 | 每条可见通知一个 NotificationCard。 |
| N 条可见通知 | 各 N | 各 1 | manager 只有一个模板实例与一个 `PART_Items`。 |
| `Icon` 为 null 且默认类型 | `icon` marker 保留，节点 `IsVisible=false` | — | 高亮与命中只针对有效可见实例。 |
| 自定义 `Icon` | `icon` marker 保留 | — | 自定义图标不新增节点。 |
| `Actions` 为空 | `actions` marker 保留，节点 `IsVisible=false` | — | 提供操作后节点显示，不新增节点。 |
| `IsShowProgress=false` 或 `Expiration` 为空 | `progress` 节点与 marker 都不存在 | — | `Optional` 语义由运行时创建路径表达。 |
| 显示进度 | `progress` 恰 1 | — | 节点贴底覆盖，不改变其它 Part 数量。 |
| 超时自动关闭 | 关闭后随卡片移除 | 各 1 | `NotificationClosed` 后 manager 从 `PART_Items` 移除卡片。 |
| 手动 `Close()` | 同上 | 各 1 | 与超时走同一关闭动效状态流。 |
| 超过 `MaxItems` | 只保留 `MaxItems` 条可见 | 各 1 | 超出部分被 `Close()`，marker 随卡片移除。 |
| 宿主 detach / `Dispose` | 全部移除 | root 随 manager 卸载 | `OnDetachedFromVisualTree` 清空 `PART_Items`；`Dispose` 卸载事件、计时器与宿主层。 |

## 5. 尺寸基线

Notification 没有 `SizeType` 分档，视觉基线由 `NotificationCardToken` 与全局 Token 常量表达：

- 卡片背景 `NotificationBg`（`ColorBgElevated`），内边距 `NotificationPadding`
  （垂直 `UniformlyPaddingMD`，水平 `UniformlyPaddingLG`，上下左右对称），严格取上游
  `notificationPadding = paddingMD paddingLG` 的同名值，不做比例缩放。
- 圆角 SharedToken `BorderRadiusLG`，阴影 SharedToken `BoxShadows`。
- 宽度 `NotificationWidth`（384），对齐上游 `width: 384`。
- `wrapper` 的图标间距 `NotificationIconMargin`（右 `UniformlyMarginSM`），对齐上游 notice wrapper `gap: marginSM`。
- `section` 的标题与描述间距 `NotificationSectionSpacing`（`UniformlyMarginXS`），对齐上游 `gap: marginXS`。
- 标题右侧留白 `NotificationTitlePadding`（右 `UniformlyPaddingLG`），对齐上游
  `.notice-closable` 的 `padding-inline-end: paddingLG`；描述在标题下方、不与关闭按钮同排，按上游
  `.notice-title + .notice-description { padding-inline-end: 0 }` 不预留右侧空间。
- `close` 尺寸 `NotificationCloseButtonSize`（`ControlHeightLG * 0.55`，对齐上游 `notificationCloseButtonSize`）、
  内间距 `NotificationCloseButtonPadding`（`PaddingXXS`）、覆盖层偏移 `NotificationCloseButtonMargin`
  （上 `UniformlyPaddingMD`、右 `UniformlyPaddingLG`），对齐上游 `top: notificationPaddingVertical` 与
  `inset-inline-end: notificationPaddingHorizontal`。
- `actions` 上边距 `NotificationActionsMargin`（`UniformlyMarginSM`），对齐上游 `margin-top: marginSM`。
- `progress` 高度 `NotificationProgressHeight`（2）、左右内缩 `NotificationProgressMargin`（`BorderRadiusLG`）、
  彩色进度值画刷 `NotificationProgressBg`（`ColorPrimaryBorderHover -> ColorPrimary` 渐变）、底槽
  `NotificationProgressTrackBg`（`ColorFillQuaternary`，对齐上游 `rgba(0, 0, 0, 0.04)`），对齐上游 progress
  覆盖层「底槽 + 进度值」两层绘制；缺底槽时剩余时间不可见。
- 文本字号/行高/颜色：标题 SharedToken `FontSizeLG` / `FontHeightLG` / `ColorTextHeading`，描述 SharedToken
  `FontSize` / `FontHeight` / `ColorText`。上游 notice title 为 `fontSizeLG` / `lineHeightLG`（16 / 1.5，行高 24），
  section 为 `gap: marginXS`（8）。
- 图标垂直对齐：上游 notice wrapper 是 `display: flex; align-items: flex-start`，图标与标题行顶部对齐。AtomUI
  的 `IconPresenter` 在 `DockPanel` 中默认 `Stretch` 且带显式 `Height`，会被垂直居中而低一截，因此主题显式设置
  `VerticalAlignment="Top"`。

`WindowNotificationManager` 没有独立尺寸 Token：其 `root` 铺满 TopLevel 并由 `TopLevelMarginBinder` 投影安全区外边距，
并通过 `NotificationPadding` 提供列表内边距，`listContent` 的对齐与顺序来自 `Position` 伪类、项间距来自 `Spacing`。

### 5.1 list / listContent / root 的几何归属

上游把三个层级的职责分得很清楚，AtomUI 严格对齐：

| 上游 | AtomUI 节点 | 承担 |
| --- | --- | --- |
| `.ant-notification` / `.ant-notification-list`（`--notification-margin-edge`、padding） | `WindowNotificationManager`（`root`）+ 模板内 `Border` 消费 `Padding` | 列表容器内边距与定位 |
| `.ant-notification-list-content`（`gap: notificationMarginBottom`） | `ReversibleStackPanel#PART_Items`（`listContent`） | 通知项排列与间距 |
| `.ant-notification-notice`（绝对定位，自身零外边距） | `NotificationCard`（`root`） | 单条通知，几何等于可见卡片 |

据此，`root`（卡片）的高亮框与可见卡片完全重合（四边间距为 0），`listContent` 高亮框相对 `root` 四边各内缩一个
内边距，形成两个大小不同、间距对称的矩形——与上游悬停 `list` / `listContent` 的表现一致。

### 5.2 单预览合并两个 owner

对齐上游 `notification#semantic-dom` 的单预览形态：一个 `SemanticPartPreview` 通过 `SemanticOwners` 声明
`WindowNotificationManager` 与 `NotificationCard` 两个 owner，Part 列表按 owner 声明顺序合并（11 项）。`root` 等
重名路径在合并列表里靠 `SemanticPartDescription.OwnerType` 消歧，列表行的 owner 标签用于区分归属；高亮按 Part
所属 owner 解析，只会在该 owner 的实例作用域内命中。多 owner 时每一条描述都必须声明 `OwnerType`，否则报错而不是
静默归属。

`NotificationCard` 提供公开无参构造，因此预览里的卡片可以直接声明式实例化（manager 构造路径仍用于运行时队列）。

### 5.3 反馈层样式作用域

宿主构造下 `WindowNotificationManager` 会被挂进 `WindowFeedbackLayer`，该层在页面视觉树之外。样式作用域实测结论与
Message 一致：页面视觉树内的 `Style` / `Styles` 命中不到反馈层卡片，样式必须落在 manager 自身、Window 或
Application 之一。Gallery 的 `Custom Semantic Part styling` 示例把生成的专用 Style 类写在页面 AXAML 的
`UserControl.Resources` 里（声明式、类型安全），再由 code-behind 挂到 manager 的 `Styles`；挂载是唯一的代码步骤，
不涉及任何部件属性改写。

资源查找必须走声明该 `Styles` 的资源宿主。写在 `UserControl.Resources` 的样式只有该页面自己的
`Resources.TryGetResource(...)` 能看到；用 `Application.Current.TryGetResource(...)` 查找会返回 false，样式被静默跳过、
卡片回落默认外观（本页最初即因此失效，已由
`Notification_Semantic_Style_Buttons_Produce_Styled_Cards` 端到端锁定）。若把 `Styles` 放到 `Application.Resources`，
才改用 `Application.Current` 查找，二者不能混用。

状态分支（如 `semantic-error-style-demo:error`）要求卡片的 `NotificationType` 伪类成立。`NotificationCard` 在挂到视觉树
后由 `SetupNotificationTypePseudoClasses` 设置 `:error` / `:success` 等伪类，因此按类型分支的样式只在类型伪类生效后
命中；`Show` 路径会把 `NotificationType` 传给卡片，满足该前提。

## 6. 定制边界

以下区域明确不属于 Notification Semantic Part：

- 关闭动效执行状态（`MotionExecutionState` 与 `LayoutAwareMotionActor`）、`Close()` 调度与 `IsClosed` 提交路径是
  行为状态，不是 Part。
- `Border#Frame`、`Panel#PART_Layout`、`Border#ContentBox`、`StackPanel` 是模板结构节点：`root` 的表面投影到 `Frame`，
内边距投影到 `ContentBox`，但节点名称、
  数量与层级不属于 `root` 契约。
- `_pendingNotifications` 队列、`_cardExpiredTimer` / `_cleanupTimer` 计时器、`MaxHostLayerRetryCount` 重试与宿主层
  安装/卸载是 manager 的行为实现，不是 Part。
- `NotificationCard` 的默认图标选择（`SetupDefaultNotificationIcon`）与 `NotificationType` 伪类是行为状态；
  `icon` Part 只表达图标元素的视觉。
- `NotificationPropertyBar` 的绘制算法（剩余时间比例、`Render` 矩形）是实现细节；`progress` Part 只表达该覆盖
  元素的视觉区域。
- `Notification`、`INotification`、`INotificationManager` 数据/契约类型。

Semantic Style 服从 Avalonia 原生属性优先级。Notification 没有用户可见的颜色状态机入口：卡片状态色由
`NotificationType` 属性 Selector 在主题内设置，用户 Semantic Style setter 覆盖主题 setter，样式移除后主题恢复。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality，或者让任一内置模板缺少 marker，
均属于公共主题契约变更。

验证至少覆盖：

- descriptor 中 `NotificationCard` 只有 `root`、`wrapper`、`icon`、`section`、`title`、`description`、`actions`、
  `close`、`progress`，`WindowNotificationManager` 只有 `root`、`listContent`，字段值与本文表格一致；
  `Notification`、`INotification`、`INotificationManager`、`NotificationCardToken`、`NotificationProgressBar`
  不持有 descriptor。
- 两个 owner 的主题资产各自携带批准 marker：`NotificationCardTheme.axaml` 为 `semantic-wrapper:DockPanel`、
  `semantic-icon:IconPresenter`、`semantic-section:StackPanel`、`semantic-title:SelectableTextBlock`、
  `semantic-description:ContentPresenter`、`semantic-actions:ContentPresenter`、`semantic-close:IconButton`；
  `WindowNotificationManagerTheme.axaml` 为 `semantic-list-content:ReversibleStackPanel`；
  `progress` 不进静态模板 marker，由运行时路径注入。
- 全部 `NotificationType`、自定义 `Icon`、`Actions` 有无、进度开关、多项 queue、关闭与 detach 后 marker 数量符合
  第 4 节表。
- owner-scoped Semantic Style（九个生成 Style 类型）与 `x:SetterTargetType` 可以编译并命中对应最低 public 类型。
- 宿主 attach/detach、`Dispose` 后不保留旧卡片、计时器或宿主层引用。
- 默认主题不消费 `.semantic-*`，未声明用户 Semantic Style 时不增加 selector activator。
- Generator 静态输出和 NativeAOT 路径不依赖反射或运行时扫描。

### 7.1 与改造前的差异（行为与渲染变化）

本次改造把 notice 模板结构对齐到上游 DOM，属于已授权的渲染结果与公共主题契约变更：

- `close` 从原来的流内 `DockPanel` 子节点改为右上角覆盖层；`title` 改为通过 `NotificationTitlePadding` 为关闭按钮
  预留右侧空间（描述按上游规则不预留），而不是依赖 `DockPanel` 的 `LastChildFill`。
- 卡片 `Padding` 由 `(左右, 上, 左右, 0)` 改为四边对称，对齐上游 `padding: paddingMD paddingLG`；该内边距不再由
  `Border#Frame` 承担，而是投影到 Frame 内部的 `Border#ContentBox`（见下一项）。
- `Border#Frame` 从 `Panel#PART_Layout` 的子节点提升为 `LayoutAwareMotionActor` 的直接内容，`Padding` 下移到
  新增的 `Border#ContentBox`。原因是 `position:absolute` 的覆盖层以 padding box 为基准，且 `Frame` 的
  `BoxShadow` 曾被中间 `Panel` 裁到右/下各 1 个逻辑像素（实测设备像素 2，上游为 8）。结构变化见 §1 的模板示意。
- 新增 `section`（`StackPanel`）承载标题与描述，标题与描述间距改由其 `Spacing` 表达；原 `NotificationContentMargin`
  与 `HeaderMargin` 两个 Token 由 `NotificationSectionSpacing` 取代。
- 新增 `actions` 区域与 `NotificationCard.Actions` / `ActionsTemplate`、`INotification.Actions` /
  `ActionsTemplate`、`Notification.Actions` 公共契约。
- `progress` 从 `Grid` 的行内元素改为 `Panel` 上的底部覆盖层，左右内缩 `BorderRadiusLG`。
- manager 新增 `Padding`（`MarginLG`）承担列表内边距；卡片侧移除 `NotificationTopMargin` / `NotificationBottomMargin` /
  `NotificationMarginBottom` 三个边缘外边距 Token。
- `NotificationCard` 新增公开无参构造（与 `MessageCard` 对齐），使卡片可声明式实例化；manager 构造路径行为不变。
- `WindowNotificationManager` 补齐 `OnDetachedFromVisualTree` 清空 `PART_Items`，与 `WindowMessageManager` 的内存
  泄漏修复保持一致。
