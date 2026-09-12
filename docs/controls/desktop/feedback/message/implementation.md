# Message 桌面版实现原理

本文档描述 Message 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Message 桌面版架构设计](overview.md)，共用堆叠与计时算法见 [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)，变化记录见 [Message Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Message Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Message 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Message/IMessage.cs`
- `src/AtomUI.Desktop.Controls/Message/IMessageManager.cs`
- `src/AtomUI.Desktop.Controls/Message/Message.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCard.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCardPseudoClass.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageCardToken.cs`
- `src/AtomUI.Desktop.Controls/Message/MessageType.cs`
- `src/AtomUI.Desktop.Controls/Message/Themes/MessageCardTheme.axaml`
- `src/AtomUI.Desktop.Controls/Message/Themes/WindowMessageManagerTheme.axaml`
- `src/AtomUI.Desktop.Controls/Message/WindowMessageManager.cs`
- `src/AtomUI.Desktop.Controls/FeedbackStack/FeedbackStackPresenter.cs`
- `src/AtomUI.Desktop.Controls/FeedbackStack/FeedbackStackPanel.cs`
- `src/AtomUI.Desktop.Controls/FeedbackStack/FeedbackLifetimeScheduler.cs`
- `src/AtomUI.Desktop.Controls/FeedbackStack/IFeedbackStackItem.cs`
- `src/AtomUI.Core/MotionScene/MotionExecutionState.cs`

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。

## 3. 核心类职责

- `Message`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MessageCard`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `MessageCardToken`：internal 控件 Token scope，负责从全局 token 派生控件语义变量。
- `WindowMessageManager`：拥有稳定卡片集合、public Stack 配置、TopLevel host、生命周期调度与用户回调清理。
- `FeedbackStackPresenter`：消费稳定 ItemsSource，管理整体 hover 和 Message 静态背板状态。
- `FeedbackStackPanel`：按共享几何契约测量和排列，不拥有内容或生命周期。
- `FeedbackLifetimeScheduler`：按单调 deadline 调度有限时长项；空闲时没有活动 timer。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Message 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Show`、`MaxItems`、`Message`、`MessageType`、`Icon`。
- Stack 与生命周期：`IsStackEnabled`、`StackThreshold`、`IsPauseOnHover`、`DestroyAll()`。
- 交互与状态：`IsClosed`、`IsClosing`、`IsMotionEnabled`。
- 视觉与布局：`Position`。

`IsClosing` 和 `IsClosed` 是 MessageCard 的 public 业务状态。关闭动效执行由 MessageCard 实例单独持有
`MotionExecutionState`，按 `Idle -> Pending -> Playing -> Completing -> Idle` 推进；Core 的共享 enum 只统一阶段语义，
不拥有 Dispatcher 任务、MotionActor 或 public 属性。属性变化与模板重套用都进入同一个 Pending 调度入口，不能并行启动
两次退出动效；Completing 只负责提交一次 `IsClosed=true`。

维护要求：

- 外部设置的 Avalonia 属性必须在模板应用前后保持一致。
- 集合、选择、展开、过滤、分页、上传任务或异步 loader 必须能处理 reset、replace 和 clear。
- 伪类和 internal state 必须从单一 owner 推导，避免双向同步导致循环更新。
- overview.md 的 API 契约说明应与源码实际状态流一致。

## 5. 生命周期与模板接入

生命周期规则：

- 构造阶段只注册必要状态，不依赖 template part。
- 模板应用时获取 part、建立事件订阅和绑定，并先释放旧 part 订阅。
- manager 的卡片 collection 在模板之外创建并保持稳定；新 `PART_Items` 只重新绑定该 collection，旧 presenter 立即解绑。
- 控件卸载、弹层关闭、窗口关闭、集合替换或 container recycle 时释放事件订阅和资源宿主。
- DynamicResource、TokenResourceBinder 或 C# binding 必须有明确 owner 和释放点。
- Browser 和 Desktop 宿主下的主题加载顺序不得影响 public API 语义。

稳定 template part 接入点：

- `PART_Frame`：承载根视觉、边框、背景或尺寸基线。
- `PART_HeaderContainer`：稳定模板协作入口，重命名前必须同步主题和实现。
- `PART_IconContent`：展示图标、状态图标或操作图标。
- `PART_Items`：`ItemsControl` 级稳定入口，承载共享 presenter 和 panel；不能再由 manager 直接修改 `Panel.Children`。
- `PART_Message`：稳定模板协作入口，重命名前必须同步主题和实现。

## 6. 交互与事件处理

Message 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 值提交或命令触发必须保持继承控件的事件顺序。

稳定事件路径包括 `MessageClosed`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- MessageCard 的关闭请求、模板状态回放和最终 `IsClosed` 提交必须经过同一个关闭动效执行状态流。
- `Show` 在 UI thread 同步创建并登记卡片，加入稳定 collection 后更新 MaxItems 和 Stack 投影；不使用延迟队列重新取得旧模板的 children。
- Stack 判定使用活动项数量严格大于有效阈值；Message 折叠只保留最新真实卡片可见，并由 AXAML 中两个静态背板表达深度。
- 生命周期调度只保存 deadline / remaining 与 card 协作引用。hover 暂停、继续和 deadline 到期由一个 manager 级 scheduler 完成，不为每项创建 timer。
- `DestroyAll()` 复制的是关闭请求顺序而非视觉集合；正在关闭项不会重复关闭，清理仍由 card 的 `MessageClosed` 单一路径提交。

实现文档不逐行解释私有方法。若某个私有算法成为稳定维护入口，应在本节补充算法不变量，而不是把代码复述为说明书。

## 8. 资源、性能与 AOT 边界

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- manager、ItemsControl 与 card collection 在 Stack 切换和重套模板之间保持稳定。
- `MeasureOverride` / `ArrangeOverride` 不允许 LINQ、临时数组、闭包或逐帧 transform 创建。
- 没有有限时长活动项时 scheduler 不持有 timer；没有 Notification 进度需求时只安排最近 deadline。
- 折叠背板由 AXAML 静态创建，不随 show 次数增加视觉对象。
- 性能修改必须使用同一 Message 场景比较基线与优化后的 mean、median、P95，并证明主要指标无可测量回退。

## 9. 维护不变量

维护 Message 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
- `IsClosing` / `IsClosed` public 状态不得与 internal `MotionExecutionState` 合并；重复调度不得创建并行退出动效。
- `MaxItems <= 0` 必须保持无限语义；Stack 不能通过提前关闭旧项模拟折叠。
- template detach、rehost、DestroyAll、用户回调异常与 dispose 均必须释放 scheduler entry、presenter、host 订阅、card owner 与 delegate。

## 10. 测试与验证

推荐验证：

- `CloseMotionExecutionTests` 验证关闭属性变化与模板重套用同时请求退出动效时只启动一次 motion，并只提交一次
  `IsClosed=true`。
- `FeedbackStackLayoutTests` 覆盖阈值边界、最新项顺序、Message 背板、hover gap、运行时配置和 Top/Bottom 几何。
- `FeedbackLifetimeSchedulerTests` 以可控时钟覆盖最近 deadline、剩余时长、普通/Stack 暂停和空闲停表。
- manager 生命周期测试以 `WeakReference` 覆盖 show/destroy/retemplate/detach/dispose 和用户回调异常。
- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
