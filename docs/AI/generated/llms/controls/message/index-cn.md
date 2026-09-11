# Message

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## 概述

Message 是 AtomUI 桌面控件体系中的全局消息控件，用于展示轻量级、自动关闭的操作反馈。

Message 不负责持久通知、模态对话框或页面内 Alert。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/Message`

## 包与命名空间

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Feedback/Message` |
| 状态 | Stable |

## 何时使用

Message 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | Message 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | Message 是 AtomUI 桌面控件体系中的全局消息控件，用于展示轻量级、自动关闭的操作反馈。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `Message` 内容对象、`Icon`、`MessageType`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | 自动关闭、hover 暂停、Stack 展开/折叠和关闭 motion。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Message Token + ControlTheme。 |

## 公共 API

Message 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Show(IMessage, string[]?)`、`MaxItems` | 创建消息并约束活动项上限；`MaxItems <= 0` 表示不限制。 |
| Stack | `IsStackEnabled`、`StackThreshold`、`IsPauseOnHover` | 默认关闭；控制阈值折叠、整体 hover 展开和实际 hover 期间的生命周期暂停，不改变消息时长。 |
| 交互与状态 | `DestroyAll()`、`IsClosed`、`IsClosing`、`IsMotionEnabled` | 清空活动消息，并表达卡片关闭与动效状态。 |
| 视觉与布局 | `Position` | 默认为 `TopCenter`，决定宿主边和横向对齐。 |
| 内容对象 | `Message`、`MessageType`、`IMessage.Expiration` | 表达正文、类型、图标、自动关闭时长与一次性关闭回调。 |

稳定事件包括 `MessageClosed`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 类型：`Message`、`MessageCard`、`WindowMessageManager`、`IMessageManager`。
- 枚举：`MessageType`。

Semantic Part owner 为两个 public Control：`MessageCard`（单条消息卡片）与 `WindowMessageManager`（服务型消息宿主）。
两个 owner 各自公开独立 descriptor，完整 Part 表、selector、`ContractType`、cardinality、节点映射与定制边界见
[Message Semantic Part 契约](semantic-part.md)。

稳定 template part：

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Frame` | `?` | 承载根视觉、边框、背景或尺寸基线。 |
| `PART_HeaderContainer` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |
| `PART_IconContent` | `?` | 展示图标、状态图标或操作图标。 |
| `PART_Items` | `ItemsControl` | 承载 manager 的稳定卡片集合和共享 Stack panel。 |
| `PART_Message` | `?` | 稳定模板协作入口，重命名前必须同步主题和实现。 |

控件专属或内部伪类包括 `Error=:error`、`Information=:information`、`Loading=:loading`、`MessageCardPseudoClass.Error`、`MessageCardPseudoClass.Information`、`MessageCardPseudoClass.Loading`、`MessageCardPseudoClass.Success`、`MessageCardPseudoClass.Warning`、`Success=:success`、`Warning=:warning`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 事件与命令

Message 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。
稳定事件包括 `MessageClosed`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

## 使用示例

稳定示例来源于 Gallery ShowCase 和源码查看片段。生成器只输出可从 `ShowCaseItem` 追溯的示例，不维护第二套手写示例。

以下示例来自 Gallery 源码查看使用的 `ShowCaseItem` 片段，并已按中文资源规范化。

### 基础用法

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml:145`

Gallery key：`ExamplesContent` / item `0`

```axaml
<atom:Button ButtonType="Primary" Click="ShowSimpleMessage" Content="显示普通消息" />
```

### 其他消息类型

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml:156`

Gallery key：`ExamplesContent` / item `1`

```axaml
<StackPanel Orientation="Horizontal" Spacing="10">
    <atom:Button ButtonType="Default" Click="ShowSuccessMessage" Content="成功" />
    <atom:Button ButtonType="Default" Click="ShowInfoMessage" Content="信息" />
    <atom:Button ButtonType="Default" Click="ShowWarningMessage" Content="警告" />
    <atom:Button ButtonType="Default" Click="ShowErrorMessage" Content="错误" />
</StackPanel>
```

### 带加载指示器的消息

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml:171`

Gallery key：`ExamplesContent` / item `2`

```axaml
<atom:Button ButtonType="Default" Click="ShowLoadingMessage" Content="显示加载指示器" />
```

### 回调

来源：`controlgallery/AtomUIGallery/ShowCases/Feedback/Message/Views/MessageShowCase.axaml:182`

Gallery key：`ExamplesContent` / item `3`

```axaml
<atom:Button ButtonType="Default" Click="ShowSequentialMessage" Content="显示加载指示器" />
```

## 状态模型

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

## 主题与 Design Token

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

Token 来源：

Message Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- internal `MessageCardToken`，scope id 为 `MessageCard`，源码位于 `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`。

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
- 没有有限时长活动项时 scheduler 不保持活动 timer；没有 Notification 进度需求时只安排最近 deadline。
- Gallery Stack 示例的零时长消息不创建 scheduler entry、异步循环、取消令牌或逐项 timer；两个示例 manager 都只在
  第一次实际操作时创建，并在页面 detach 时释放。
- 折叠背板由 AXAML 静态创建，不随 show 次数增加视觉对象。
- 性能修改必须使用同一 Message 场景比较基线与优化后的 mean、median、P95，并证明主要指标无可测量回退。

## 源码索引

主要源码文件：

- `src/AtomUI.Desktop.Controls/Message/IMessage.cs`
- `src/AtomUI.Desktop.Controls/Message/IMessageManager.cs`
- `src/AtomUI.Desktop.Controls/Message/Message.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCard.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCard.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCardPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageType.cs`
- `src/AtomUI.Desktop.Controls/Message/Themes/MessageCardTheme.axaml`
- `src/AtomUI.Desktop.Controls/Message/Themes/WindowMessageManagerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Message/WindowMessageManager.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPresenter.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackStackPanel.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackLifetimeScheduler.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotion.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/FeedbackCardMotionCoordinator.cs`
- `src/AtomUI.Desktop.Controls/Primitives/FeedbackStack/IFeedbackStackItem.cs`
- `src/AtomUI.Desktop.Controls/Message/WindowMessageManager.SemanticParts.cs`
- `src/AtomUI.Core/MotionScene/MotionExecutionState.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 相关文档

- 源设计文档：`docs/controls/desktop/feedback/message/overview.md`
- 实现文档：`docs/controls/desktop/feedback/message/implementation.md`
- Semantic Part 文档：`docs/controls/desktop/feedback/message/semantic-part.md`
- Token 文档：`docs/controls/desktop/feedback/message/token.md`
- 变更记录：`docs/controls/desktop/feedback/message/changelog.md`
- 语义结构：`./semantic-cn.md`
