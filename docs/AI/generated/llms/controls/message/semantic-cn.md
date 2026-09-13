# Message 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

| Part | AtomUI 节点 | 职责 | 相关 API | 相关 Token | 稳定性 |
| --- | --- | --- | --- | --- | --- |
| `root` | `Message` | 反馈控件根语义区域，承载 public API、反馈状态和主题入口。 | 见 API 与契约模型 | 见视觉与主题模型 | stable |
| `host` | `宿主或弹层区域` | 承载 overlay、popup、portal、message host、drawer 或 modal 容器。 | 见 API 与契约模型 | 见视觉与主题模型 | stable |
| `surface` | `反馈表面` | 承载背景、边框、阴影、尺寸、placement 和视觉状态。 | 见 API 与契约模型 | 见视觉与主题模型 | stable |
| `content` | `内容区域` | 承载标题、正文、图标、进度、结果、操作或关闭入口。 | 见 API 与契约模型 | 见视觉与主题模型 | stable |
| `motion` | `动效区域` | 表达进入退出、loading、progress、skeleton 或水印刷新反馈。 | 见 API 与契约模型 | 见视觉与主题模型 | stable |

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
| `MessageCard` | control theme | `MessageCardTheme.axaml` | 用户代码 / 控件宿主 | `Icon`, `Message` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `{x:Static atom:BaseMotionActor.MotionActorPart}` | template node (MotionActor) | `MessageCardTheme.axaml` | MessageCard | `Icon`, `Message` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_Frame` | template node (Border) | `MessageCardTheme.axaml` | MessageCard | `Icon`, `Message` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
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
