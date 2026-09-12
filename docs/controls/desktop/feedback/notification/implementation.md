# Notification 桌面版实现原理

本文档描述 Notification 桌面版的内部实现范围、源码职责、状态流、生命周期、资源边界和维护规则。公共设计与 API 契约见 [Notification 桌面版架构设计](overview.md)，共用堆叠与计时算法见 [Feedback 堆叠基础设施](../../../../architecture/systems/control-infrastructure/feedback-stack.md)，变化记录见 [Notification Changelog](changelog.md)。涉及控件 Token 的实现应同时阅读 [Notification Token 设计](token.md)。

## 1. 实现定位

本文档覆盖 Notification 的控件实现、主题接入、状态同步和 Gallery 可见维护边界。具体属性注册、默认值、绘制细节和 AXAML selector 仍应直接阅读源码；本文只记录维护者必须理解的稳定结构和不变量。

## 2. 源码文件结构

主要源码文件：

- `src/AtomUI.Desktop.Controls/Notifications/INotification.cs`
- `src/AtomUI.Desktop.Controls/Notifications/INotificationManager.cs`
- `src/AtomUI.Desktop.Controls/Notifications/Notification.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationCard.cs`
- `src/AtomUI.Desktop.Controls/Notifications/NotificationMotions.cs`
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

- `Notification`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationCard`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveDownInMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveDownOutMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveLeftInMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveLeftOutMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveRightInMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveRightOutMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveUpInMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationMoveUpOutMotion`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationProgressBar`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationProgressBarVisibleConverter`：控件核心或内部协作类型，维护 public surface 与主题可观察行为。
- `NotificationCardToken`：internal 控件 Token scope，负责从全局 token 派生控件语义变量。
- `WindowNotificationManager`：拥有稳定卡片集合、public Stack 配置、TopLevel host、生命周期调度与用户回调清理。
- `FeedbackStackPresenter`：消费稳定 ItemsSource，管理整体 hover 和可见项投影。
- `FeedbackStackPanel`：按共享几何契约测量和排列 Notification 的三层折叠与 Top/Bottom 方向。
- `FeedbackLifetimeScheduler`：按单调 deadline 调度有限时长项，并只为可见进度项安排刷新。

核心协作规则：

- 控件实例是 public API 和运行时状态 owner。
- Template part 是视觉协作对象，生命周期必须受 `OnApplyTemplate` 或模板加载流程管理。
- 数据对象、选项对象、任务对象或节点对象只保存业务数据，不应反向持有不可释放的视觉对象。
- 弹层、窗口、计时器、异步 loader 和全局管理器必须有明确关闭、解绑或释放路径。

## 4. 状态与数据流

Notification 的状态流遵循下面路径：

```text
Public API / ItemsSource / Command / Event
  -> 控件实例状态
  -> internal state / effective state / pseudo-class
  -> template part property / AXAML selector
  -> renderer / popup / adorner / Gallery observable behavior
```

源码中的状态入口按以下语义维护：

- 内容与数据：`Show`、`MaxItems`、`Title`、`Content`、`Icon`、`NotificationType`。
- Stack 与生命周期：`IsStackEnabled`、`StackThreshold`、`IsPauseOnHover`、`DestroyAll()`、`Expiration`、`CurrentExpiration`。
- 交互与状态：`IsClosed`、`IsClosing`、`IsMotionEnabled`、`IsShowProgress`。
- 视觉与布局：`Position`、`ProgressIndicatorBrush`、`ProgressIndicatorThickness`。

`NotificationType.Default` 是普通通知入口，不生成类型图标；带类型通知由 `NotificationType` 映射到 success/info/warning/error 伪类和默认状态图标。自定义 `Icon` 始终优先于类型图标。

`IsClosing` 和 `IsClosed` 是 NotificationCard 的 public 业务状态。关闭动效执行由 NotificationCard 实例单独持有
`MotionExecutionState`，按 `Idle -> Pending -> Playing -> Completing -> Idle` 推进；Core 的共享 enum 只统一阶段语义，
不拥有 Dispatcher 任务、MotionActor、Position 或 public 属性。属性变化与模板重套用都进入同一个 Pending 调度入口，
不能并行启动两次退出动效；Completing 只负责提交一次 `IsClosed=true`。

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

- `PART_CloseButton`：承载用户触发入口、导航或关闭动作。
- `PART_Items`：`ItemsControl` 级稳定入口，承载共享 presenter 和 panel；不能再由 manager 直接修改 `Panel.Children`。
- `PART_Layout`：稳定模板协作入口，重命名前必须同步主题和实现。

## 6. 交互与事件处理

Notification 的交互事件应从输入源收敛到控件级语义事件：

- Pointer、keyboard、focus 和 command 事件不应绕过 Avalonia 基础控件语义。
- 没有弹层职责的路径不应引入额外 popup 或全局输入捕获。
- 集合类路径必须稳定处理 container prepare、clear、过滤、分组和虚拟化回收。
- 值提交或命令触发必须保持继承控件的事件顺序。

稳定事件路径包括 `NotificationClosed`。事件参数和触发时机属于兼容边界。

## 7. 内部算法与关键流程

维护者需要重点关注以下流程：

- API 默认值到 effective state 的归一。
- Template part 重新应用时的状态回放。
- 主题资源、Token 和 SharedToken 计算后的视觉更新。
- ItemsSource、selection、checked、expanded、filter、paging 或 upload task 的集合同步。
- 动效启停、初始加载阶段 transition 抑制和卸载取消。
- NotificationCard 的关闭请求、模板状态回放和最终 `IsClosed` 提交必须经过同一个关闭动效执行状态流。
- `Show` 在 UI thread 同步创建并登记卡片，加入稳定 collection 后更新 MaxItems 和 Stack 投影；不使用 cleanup queue 或轮询寻找关闭项。
- Stack 判定使用活动项数量严格大于有效阈值；Notification 折叠呈现最新三张真实卡片，scale 为 `1`、`0.94`、`0.88`，其余旧项保留但不绘制或命中。
- 生命周期调度不递减 public `Expiration`，只把剩余时间单向投影到可见进度；没有进度刷新需求时按最近 deadline 单次唤醒。
- `DestroyAll()` 对所有未关闭项发起一次关闭；集合移除、用户回调与资源释放仍由 `NotificationClosed` 单一路径提交。

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
- 一个 manager 最多一个惰性 scheduler timer；隐藏旧项不进行进度刷新、绘制或 hit test。
- scale、offset 与 opacity 通过 compositor 友好的 transform 更新，不动画 width、height 或 margin。
- 性能修改必须使用同一 Notification 场景比较基线与优化后的 mean、median、P95，并证明主要指标无可测量回退。

## 9. 维护不变量

维护 Notification 时不得破坏：

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
- `FeedbackStackLayoutTests` 覆盖阈值边界、最新三项顺序、scale/offset、hover gap、运行时配置和六种 Position 几何。
- `FeedbackLifetimeSchedulerTests` 以可控时钟覆盖最近 deadline、进度刷新、剩余时长、普通/Stack 暂停和空闲停表。
- manager 生命周期测试以 `WeakReference` 覆盖 show/destroy/retemplate/detach/dispose、进度刷新和用户回调异常。
- 纯文档改动运行 `git diff --check` 并检查相对链接。
- 控件 API 或行为变更运行对应 `tests/AtomUI.Desktop.Controls.Tests` 或专用包测试。
- DataGrid 相关变更运行 `tests/AtomUI.Desktop.Controls.DataGrid.Tests`。
- Gallery 示例或源码片段变更运行 `tests/AtomUIGallery.Tests`。
- AOT、生成器或动态数据路径变更按 Gallery NativeAOT 发布流程验证。
