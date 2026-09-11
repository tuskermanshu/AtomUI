# Notification

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## 概述

Notification 是 AtomUI 桌面控件体系中的通知控件，用于在窗口角落展示可关闭的较重反馈和进度信息。

Notification 不负责即时消息气泡、页面内 Alert 或模态确认。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Notifications`

## 包与命名空间

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Notification` |
| 状态 | Stable |

## 何时使用

Notification 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Notification 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Notification 是 AtomUI 桌面控件体系中的通知控件，用于在窗口角落展示可关闭的较重反馈和进度信息。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Notification` 内容对象、`Title`、`Content`、`Icon`、`NotificationType`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 自动关闭、进度、hover 暂停、Stack 展开/折叠和关闭 motion。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Notification Token + ControlTheme。 |

## 公共 API

Notification 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Show(INotification, string[]?)`、`MaxItems` | 创建通知并约束活动项上限；`MaxItems <= 0` 表示不限制。 |
| Stack | `IsStackEnabled`、`StackThreshold`、`IsPauseOnHover` | 控制阈值折叠、整体 hover 展开和生命周期暂停。 |
| 交互与状态 | `DestroyAll()`、`IsClosed`、`IsClosing`、`IsMotionEnabled`、`IsShowProgress` | 清空活动通知，并表达卡片关闭、进度和动效状态。 |
| 视觉与布局 | `Position`、`ProgressIndicatorBrush`、`ProgressIndicatorThickness` | 默认 `TopRight`；控制宿主位置和进度视觉。 |
| 内容对象 | `Expiration`、`NotificationType`、`Title`、`Content`、`Icon` | 表达正文、类型、自动关闭时长与一次性关闭回调。 |

稳定事件包括 `NotificationClosed`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`Notification`、`NotificationCard`、`NotificationProgressBar`、`WindowNotificationManager`、`INotificationManager`。
- 枚举：`NotificationPosition`、`NotificationType`。

`NotificationType.Default` 表达普通通知语义，默认不显示类型图标，也不投射 success/info/warning/error 状态伪类。`Information`、`Success`、`Warning` 和 `Error` 表达带类型通知语义，在未设置自定义 `Icon` 时使用对应状态图标，并参与状态颜色 selector。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_CloseButton` | `?` | 承载用户触发入口、导航或关闭动作。 |
| `PART_Items` | `ItemsControl` | 承载 manager 的稳定卡片集合和共享 Stack panel。 |
| `PART_Layout` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

控件专属或内部伪类包括 `BottomCenter=:bottomcenter`、`BottomLeft=:bottomleft`、`BottomRight=:bottomright`、`NotificationPseudoClass.BottomCenter`、`NotificationPseudoClass.BottomLeft`、`NotificationPseudoClass.BottomRight`、`NotificationPseudoClass.TopCenter`、`NotificationPseudoClass.TopLeft`、`NotificationPseudoClass.TopRight`、`TopCenter=:topcenter`、`TopLeft=:topleft`、`TopRight=:topright`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 事件与命令

Notification 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。
稳定事件包括 `NotificationClosed`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

## 使用示例

稳定示例来源于 Gallery ShowCase 和源码查看片段。生成器只输出可从 `ShowCaseItem` 追溯的示例，不维护第二套手写示例。

以下示例来自 Gallery 源码查看使用的 `ShowCaseItem` 片段，并已按中文资源规范化。

### 基础用法

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml:156`

Gallery key：`ExamplesContent` / item `0`

```axaml
<atom:Button ButtonType="Primary" Click="ShowSimpleNotification" Content="显示通知" />
```

### 自动关闭时长

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml:165`

Gallery key：`ExamplesContent` / item `1`

```axaml
<atom:Button ButtonType="Primary" Click="ShowNeverCloseNotification" Content="打开通知框" />
```

### 带图标通知

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml:174`

Gallery key：`ExamplesContent` / item `2`

```axaml
<StackPanel Orientation="Horizontal" Spacing="10">
    <atom:Button ButtonType="Default" Click="ShowSuccessNotification" Content="成功" />
    <atom:Button ButtonType="Default" Click="ShowInfoNotification" Content="信息" />
    <atom:Button ButtonType="Default" Click="ShowWarningNotification" Content="警告" />
    <atom:Button ButtonType="Default" Click="ShowErrorNotification" Content="错误" />
</StackPanel>
```

### 弹出位置

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Notification/Views/NotificationShowCase.axaml:188`

Gallery key：`ExamplesContent` / item `3`

```axaml
<StackPanel Orientation="Vertical" Spacing="10">
    <StackPanel Orientation="Horizontal" Spacing="10">
        <atom:Button ButtonType="Primary" Click="ShowTopNotification" Content="顶部" />
        <atom:Button ButtonType="Primary" Click="ShowBottomNotification" Content="底部" />
    </StackPanel>
    <atom:Separator />
    <StackPanel Orientation="Horizontal" Spacing="10">
        <atom:Button ButtonType="Primary" Click="ShowTopLeftNotification" Content="左上" />
        <atom:Button ButtonType="Primary" Click="ShowTopRightNotification" Content="右上" />
    </StackPanel>
    <atom:Separator />
    <StackPanel Orientation="Horizontal" Spacing="10">
        <atom:Button ButtonType="Primary" Click="ShowBottomLeftNotification" Content="左下" />
        <atom:Button ButtonType="Primary" Click="ShowBottomRightNotification" Content="右下" />
    </StackPanel>
</StackPanel>
```

## 状态模型

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

## 主题与 Design Token

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

Token 来源：

Notification Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- internal `NotificationCardToken`，scope id 为 `NotificationCard`，源码位于 `src/AtomUI.Desktop.Controls/Notifications/NotificationCardToken.cs`。

## AOT 与裁剪注意事项

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- manager、ItemsControl 与 card collection 在 Stack 切换和重套模板之间保持稳定。
- `MeasureOverride` / `ArrangeOverride` 不允许 LINQ、临时数组、闭包或逐帧 transform 创建。
- 稳态布局必须复用缓存 transform；目标变化最多创建一个 transform 并交给 transition 插值，不增加逐帧 managed 回调。
- Notification 进出场必须使用 render-only actor，不能因 translate/fade 在每帧触发 panel Measure；布局只在集合、测量
  尺寸、位置、Stack 配置或 hover 投影实际变化时失效。
- 默认 Stack 关闭时，普通 Notification 不进入折叠裁剪投影；入场只增加一个一次性帧屏障，四个方向的偏移 transform 静态复用。
- 一个 manager 最多一个惰性 scheduler timer；隐藏旧项在折叠稳态不进行进度刷新、绘制或 hit test，其折叠过渡只复用
  已有 transform 和 clip geometry，不新增 timer、逐帧 managed 回调或强引用缓存。
- 内容快照是 bounded one-shot 资源：一次折叠最多三个，不在普通 Notification、首次折叠稳态或反向展开路径创建；完成
  检测复用卡片已有的 transform 属性通知，不增加 timer、全局事件或独立动画时钟。
- 禁止用永久 `BitmapCache` 代替显式快照；renderer cache 不能保证正文像素在父级 transform / clip 重合成期间保持冻结，
  也不能提供确定的资源释放边界。
- scale、offset 与 opacity 通过 compositor 友好的 transform 更新，不动画 width、height 或 margin。
- 性能修改必须使用同一 Notification 场景比较基线与优化后的 mean、median、P95，并证明主要指标无可测量回退。

## 源码索引

主要源码文件：

- `src/AtomUI.Desktop.Controls/Notifications/INotification.cs`
- `src/AtomUI.Desktop.Controls/Notifications/INotificationManager.cs`
- `src/AtomUI.Desktop.Controls/Notifications/Notification.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationPosition.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationProgressBar.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCardToken.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationType.cs`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/NotificationCardTheme.axaml`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/NotificationProgressBarTheme.axaml`
- `src/AtomUI.Desktop.Controls/Notifications/Themes/WindowNotificationManagerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Notifications/Utils/NotificationProgressBarVisibleConverter.cs`
- `src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPresenter.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPanel.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackLifetimeScheduler.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotion.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotionCoordinator.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/IFeedbackStackItem.cs`
- `src/AtomUI.Desktop.Controls/Notifications/WindowNotificationManager.SemanticParts.cs`
- `src/AtomUI.Core/MotionScene/MotionExecutionState.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 相关文档

- 源设计文档：`docs/controls/desktop/feedback/notification/overview.md`
- 实现文档：`docs/controls/desktop/feedback/notification/implementation.md`
- Semantic Part 文档：`docs/controls/desktop/feedback/notification/semantic-part.md`
- Token 文档：`docs/controls/desktop/feedback/notification/token.md`
- 变更记录：`docs/controls/desktop/feedback/notification/changelog.md`
- 语义结构：`./semantic-cn.md`
