# Feedback 堆叠基础设施

本文定义 Message 与 Notification 共用的堆叠、生命周期计时和集合呈现契约。共享实现位于
`src/AtomUI.Desktop.Controls/Primitives/FeedbackStack`，控件管理器继续拥有各自的 public API、内容模型、位置和关闭事件。目录入口见
[Control 基础设施](overview.md)，控件入口见 [Message](../../../controls/desktop/feedback/message/overview.md) 与
[Notification](../../../controls/desktop/feedback/notification/overview.md)。

本文是两个反馈控件堆叠行为的唯一设计 owner。控件文档只描述各自差异和接入点，不复制共享算法。

## 1. 设计定位与不变量

反馈堆叠把按创建顺序保存的卡片集合投影为展开或折叠布局，并统一 Message 与 Notification 的卡片进入、退出和队列
重排动效。它不改变两个控件的内容、类型图标、关闭事件及 public 状态语义。

- 最新卡片始终靠近宿主边缘；Top 位置向下展开，Bottom 位置向上展开。
- `StackThreshold` 只决定何时折叠。只有当前活动卡片数严格大于阈值时才进入折叠态。
- 指针进入整个堆叠命中区域后展开全部活动卡片，离开后恢复折叠；卡片之间的 gap 也属于该命中区域。
- Message 折叠时只显示最新一张真实卡片，并用两层静态模板背板表达深度。
- Notification 折叠稳态最多显示最新三张真实卡片，实际可见数为 `Min(3, StackThreshold)`；更旧卡片保留在集合中，目标 opacity 为 `0` 且不参与命中和进度刷新。展开到折叠的过渡期内，这些旧卡片仍必须取得完整折叠投影，不能以未裁剪原尺寸穿过前层后再消失。
- 折叠仅改变视觉投影，不销毁旧项、不重置剩余时长、不改变 `MaxItems` 语义。
- Stack 配置与展示时长彼此独立：开启或进入折叠态不会把有限时长项改成永久项，也不会自行暂停 deadline。
- 计时使用单调时间和剩余时长；暂停后从剩余时长继续，不能重新开始完整时长。
- 一个管理器最多拥有一个惰性调度器；空集合、全部永久展示、detach 或 dispose 时不得保持活动 timer。
- 布局热路径不得通过 LINQ、临时数组或逐帧重建视觉树产生分配。
- 卡片出现与队列让位必须是同一次连续反馈：新卡片从宿主边方向进入，已有卡片同步过渡到新的稳定位置，不允许先跳变
  Bounds 再单独播放新卡片动画。
- 进入和退出不得通过压缩卡片宽高制造弹出感；卡片 scale 在整个进入/退出阶段保持 `1`。
- 静态可声明的背板必须位于 AXAML；运行时只更新可见性与布局状态。

## 2. 角色、所有权与源码边界

| 角色 | 稳定职责 | 所有者 |
| --- | --- | --- |
| `FeedbackStackPresenter` | 保存稳定 `ItemsSource` 接入、堆叠 hover 状态和模板背板 | 共享内部呈现层 |
| `FeedbackStackPanel` | 测量、展开/折叠排列、命中几何和可见项裁剪 | 共享内部布局层 |
| `FeedbackLifetimeScheduler` | 单调截止时间、暂停/继续、最近截止唤醒和可选进度刷新 | 每个管理器一个内部实例 |
| `IFeedbackStackItem` | 卡片关闭、剩余时长、进度和布局投影所需的强类型内部协作 | `MessageCard`、`NotificationCard` |
| 活动集合 | 创建顺序稳定的卡片；包含进入、展示和正在退出的项 | 对应 Window manager |
| public 内容对象 | 文本、类型、图标、时长和用户回调 | `IMessage` / `INotification` |

共享类型均为 internal，不新增 public 基类。MessageCard 与 NotificationCard 通过显式内部接口协作，避免把两个控件的
public surface、模板结构或专属状态强行合并。共享层不引用 Gallery，不使用运行时反射或字符串成员发现。

```text
Show(public content)
  -> manager 创建 card 并加入稳定集合
  -> presenter / panel 根据 stack state 投影布局
  -> lifetime scheduler 登记单调截止时间
  -> close request -> card exit motion -> closed event
  -> manager 清理登记、回调、订阅和集合项
```

## 3. Public 配置与默认值

两个管理器公开同名配置，使 XAML 与代码调用保持一致：

| 配置 | Message 默认值 | Notification 默认值 | 语义 |
| --- | --- | --- | --- |
| `IsStackEnabled` | `false` | `false` | 是否允许超过阈值后折叠；两个控件均需显式开启 Stack |
| `StackThreshold` | `3` | `3` | 活动项数量严格大于该值时折叠；有效值最小为 `1` |
| `IsPauseOnHover` | `true` | `true` | 是否在指针悬停时暂停自动关闭 |
| `MaxItems` | `0` | `0` | `<= 0` 表示不限制；正数表示达到上限时关闭最旧活动项 |
| `Position` | `TopCenter` | `TopRight` | 堆叠的宿主边和横向对齐 |
| `IsMotionEnabled` | SharedToken `EnableMotion` | SharedToken `EnableMotion` | 是否播放卡片和堆叠状态动效 |

`IMessageManager` 与 `INotificationManager` 均公开 `DestroyAll()`。它为所有活动卡片发起正常关闭并释放计时登记；
已经关闭的卡片不会重复回调。Message 的默认展示时长为 3 秒，Notification 为 4.5 秒；时长为零表示永久展示。
Stack 开启、关闭、折叠或展开均不改写内容对象的时长：有限时长项继续自动关闭，零时长项不进入生命周期调度器。

本次契约允许修正旧默认值和移除基于轮询实现暴露的计时间隔属性。计时机制是实现细节，不属于控件可定制 API。

## 4. 状态模型

设活动项数为 `n`，有效阈值为 `t = Max(1, StackThreshold)`：

| 条件 | 状态 | Message 投影 | Notification 投影 |
| --- | --- | --- | --- |
| `IsStackEnabled == false` | 普通 | 全部按 16 DIP gap 展开 | 全部按 16 DIP gap 展开 |
| `n <= t` | 普通 | 全部展开 | 全部展开 |
| `n > t && !IsPointerOver` | 折叠 | 最新 1 张真实卡片 + 2 层背板 | 最新 `Min(3, t)` 张真实卡片 |
| `n > t && IsPointerOver` | 展开堆叠 | 全部展开 | 全部展开 |

正在退出的卡片在退出动效完成前仍由集合持有，但从 `IsClosing=true` 起不再计入活动数量、阈值和后续队列位置。
布局层必须保留它最后一次有效 offset、scale、clip、层级和宿主边锚点，使退出 actor 独立离场；其他活动卡片在关闭开始时
同步重排，不能等待 `IsClosed=true` 后才让位。完成后由 manager 一次性移除。运行时切换 `IsStackEnabled` 或
`StackThreshold` 立即重新投影当前集合，不重建卡片、不改变剩余时长。

Stack hover 属于 presenter 的独立指针状态，不由单个卡片竞争写入，也不缓存“进入时是否已折叠”。数量、Stack 开关或
阈值变化时都以当前 `IsPointerOver` 重新派生展开态。Stack 开启且 `IsPauseOnHover=true` 时，指针位于列表内会暂停该
管理器全部活动卡片，即使数量尚未超过阈值；Stack 关闭时只暂停实际悬停的卡片。`IsPauseOnHover=false` 时 hover 仍可
展开堆叠，但不影响计时。没有实际指针停留时不得因 Stack 开启或折叠状态暂停 deadline。

## 5. 展开与折叠几何

布局单位均为 DIP。通用展开 gap 为 16，折叠 offset 为 8。最新项索引 `k=0`，更旧项依次为 `k=1,2...`。

### 5.1 展开态

Top 位置从顶部边距向下累计 `cardHeight + 16`；Bottom 位置从底部边距向上镜像累计。横向位置由
Left / Center / Right 决定。Message 的顶部边距为 8；Notification 对宿主各边的距离为 24。

Panel 使用稳定宿主边锚点排列卡片，并以 `RenderTransform` 表达每张卡片相对锚点的展开位置。新增、移除或高度变化时，
已有卡片的目标 transform 在同一布局周期更新，由 compositor 友好的 transform transition 完成让位；不得通过动画
`Bounds`、`Width`、`Height`、`Margin` 或在 Dispatcher 中补做第二次布局来模拟移动。新卡片的内容 actor 负责进入/退出，
外层卡片 transform 只负责队列位置和 Stack scale，两层 transform 必须保持职责正交。

`IsMotionEnabled=false` 时不需要为不可观察的中间位置维持动画锚点：Panel 直接把可见卡片排列到终态 Bounds，并让隐藏
卡片保持其展开态 Bounds/transform。每个子项仍完成一次有效 Arrange，不能通过跳过 Arrange 留下 layout-invalid 状态。
该快路径只减少属性写入与后续布局，不改变最终可见位置、层级、命中或 Stack 投影。

Items presenter 的 Bounds 覆盖所有可见卡片及卡片间 gap，使从一张卡片移动到另一张时不会产生 hover 抖动。
隐藏旧卡片不参与 hit test，也不能遮挡展开项。

### 5.2 Message 折叠态

最新真实卡片保持 scale 1 和完整 opacity。其后两层为 AXAML 静态背板，不能为每条 Message 动态创建：

| 层 | 顶部偏移 | 宽度变化 | 可见性 |
| --- | --- | --- | --- |
| 第一背板 | 最新卡片高度减 8 | 左右各缩进 8，总宽度减 16 | 折叠且至少 2 个活动项 |
| 第二背板 | 最新卡片高度 | 左右各缩进 16，总宽度减 32 | 折叠且至少 3 个活动项 |

背板高度为 16，使用 tertiary shadow，`IsHitTestVisible=false`。背板只表达堆叠深度，不承载内容、计时或关闭事件。
背板尺寸始终由最新活动卡片决定，展开时仍持续测量和排列，不能把宽度清零。模板用零尺寸 Panel 叠放背板，避免透明
背板扩大空队列、单项队列或展开队列的布局及 hover 范围。顶部模式以宿主顶边为锚点，Top margin 为 `h-8` / `h`；
底部模式以宿主底边为锚点，Top margin 为 `-h-8` / `-h-16`，其中 `h` 为最新卡片高度。

显隐由 opacity 和 RenderTransform 共同表达，不使用 `IsVisible` 切断过渡。展开目标为 opacity 0 和向远离宿主边方向
平移 16 DIP，折叠目标为 opacity 1 和零平移。二者使用 `MotionDurationFast`（默认 100ms）与 Ant ease-in-out，在卡片
收拢期间完成。只有最新卡片宽度变化才使用 `MotionDurationSlow`（默认 300ms）过渡；hover 切换不重启宽度动画。
关闭 motion 和全局 motion 禁用仍拥有更高优先级，并直接提交当前目标状态。

### 5.3 Notification 折叠态

Notification 必须使用每张真实卡片的测量高度生成投影，不能假设卡片等高。设 `h(k)` 为第 `k` 张卡片高度，`p(k)` 为
对应 placement 轴上的 inset（Top 使用 top inset，Bottom 使用 bottom inset），`e(k)` 为该层远离宿主边的一侧：

```text
p(0) = 0
e(0) = h(0)
p(k) = e(k - 1) + 8 - h(k),  k > 0
e(k) = p(k) + h(k)
```

该算法使不同高度卡片的相邻远端边始终相差 8 DIP。折叠触发条件仍为 `n > t`；参与折叠 extent 计算的层数为 `t`，但
实际绘制和命中的真实卡片最多为 `Min(3, t)`。所有活动卡片的折叠目标 scale 均按
`1 - Min(k, 2) * 0.06` 计算，因此前三层为 `1`、`0.94`、`0.88`，其余旧项也收敛到最深层 `0.88`，同时以 opacity `0`
退出可见投影。Top 位置的 transform origin 位于 center/bottom，Bottom 位置位于 center/top；展开方向与宿主边相反。
最新项保持完整裁剪；所有背层（包括折叠稳态不可见的旧项）都取得 placement-aware 半裁剪：Top 只保留靠下半部，Bottom
只保留靠上半部。

卡片转换使用 transform、opacity、clip 和 z-order，不通过逐帧 width、height 或 margin 动画触发布局风暴。展开时宿主
extent 先扩展再移动卡片；折叠和关闭时必须让卡片先取得连续目标并保留退出投影，宿主 extent 的收敛不得造成屏幕坐标
跳变。展开/折叠布局过渡为 200ms。

Notification 从展开态恢复折叠态时，最多为折叠稳态仍可见的三张真实卡片冻结一次内容像素。快照只包含卡片内部正文布局，
不包含由外层卡片负责的背景、阴影、clip、opacity 与 transform；真实正文继续参与 Measure / Arrange，但在过渡期间不绘制、
不命中，由一次性 `RenderTargetBitmap` 代替。位图固定为折叠开始前已经呈现的内容，随外层卡片完成原有投影动画。展开方向
不创建快照，中途重新展开会立即恢复真实正文。位图源矩形使用物理 `PixelSize`，目标矩形使用控件逻辑 Bounds，保证
RenderScaling 大于 1 时不会把局部像素错误放大。

快照的捕获范围必须使用内容已经排列完成的 `Bounds.Size`，再按当前 RenderScaling 转为物理像素尺寸；不能使用
内容的固有 `DesiredSize`。模板中 Stretch 的正文可能比固有尺寸更宽，按固有尺寸捕获再铺满实际 Bounds 会在切换
快照的第一帧改变文字比例。捕获与替代绘制必须对应同一块已排列区域。

裁剪目标由 Panel 独占，必须作为稳定的属性基值保存；过渡完成不能恢复旧默认值并触发反向裁剪。几何更新读取
属性的当前有效值，因为基值通知内部可能嵌套发布动画值，外层通知携带的目标不能覆盖已插值的当前帧。
禁用 motion 时几何直接收敛，同时在目标变化时同步裁剪基值；重新启用 motion 不得把静止卡片重新投影到旧状态。

## 6. 生命周期调度

`FeedbackLifetimeScheduler` 为每个 manager 惰性创建，保存每个活动项的 deadline、remaining 和 pause state。它使用
单调时钟，不直接递减 public `Expiration`。调度顺序：

1. 新增有限时长项时记录 `deadline = now + duration`；永久项不登记 deadline。
2. 没有进度条刷新需求时，只安排距离最近 deadline 的单次唤醒，不执行固定频率全表扫描。
3. Notification 需要可见进度时，以受控刷新节奏只更新可见且活动的进度项；没有这类项时回到最近 deadline 模式。
4. 进入暂停状态时一次性计算并保存 remaining，取消当前唤醒；继续时从 remaining 建立新 deadline。
5. deadline 到达后先移除本批已到期项的调度登记，再逐项请求关闭；本次唤醒结束时清空临时列表，不把卡片引用留到下次唤醒。
6. 同步关闭回调结束后读取最新单调时刻计算下次唤醒，避免把回调耗时额外叠加到后续 deadline。
7. 项关闭或 `DestroyAll()` 时移除对应登记；manager detach 时暂停唤醒，manager dispose 时清空登记、停止 timer 并解除 tick。

是否登记 deadline 只由内容对象的展示时长决定，不读取 Stack 开关或折叠状态。manager 只把生命周期 detach 和实际 hover
暂停投影为 scheduler 的全局暂停；Stack 开启、超过阈值或从 hover 展开恢复折叠都不能单独暂停有限时长项。

调度器不拥有用户 delegate。manager 在调用 `OnClose` 前完成集合移除，并清空 delegate、卡片 owner 引用和事件订阅，
使回调重入不会重复提交关闭。回调异常不能阻止集合与调度资源清理。

`MaxItems` 按创建顺序固定本批最旧活动项，`DestroyAll()` 固定调用时的卡片批次并倒序发起关闭。批次只在显式关闭操作
发生时分配，不替换稳定的 ItemsSource。同步关闭与回调内嵌套关闭不得跳项或越界；回调新增项不纳入外层批次，manager
被回调 dispose 后停止继续发出关闭请求。

## 7. Template、资源与动效

`PART_Items` 保持稳定名称和 `ItemsControl` 级模板协作语义。Manager 持有稳定 collection 并绑定为 ItemsSource；
重新应用模板只替换 presenter，不会把卡片从旧 `Panel.Children` 搬运或丢失。

共享 presenter theme 提供布局 panel 和状态 selector。Message 的两层背板在 manager AXAML 中静态声明；
Notification 折叠层由真实 item container 表达。状态从 manager 属性单向投影到 presenter，不能由模板节点反写 manager。

卡片进入/退出使用同一个 internal Feedback motion 工厂和普通 render-only motion actor，并按 `Position` 选择位移轴与方向：TopCenter 从 `Y=-64 DIP`、
BottomCenter 从 `Y=+64 DIP`、Left 从 `X=-64 DIP`、Right 从 `X=+64 DIP` 进入，退出沿同一方向离场。起点/终点只改变
opacity 与 translate，scale 始终为 `1`。默认时长使用 SharedToken `MotionDurationMid`（默认 `200ms`），opacity 与
translate 在整个时长内连续变化，统一使用 `cubic-bezier(0.645, 0.045, 0.355, 1)`；不得增加使绝大部分透明度集中在
最后若干帧的中间关键帧。

进入动效遵守 `prepare -> frame -> active` 三阶段时序：模板 actor 首先在无 transition 状态下写入透明度 `0` 与方向偏移，
由所属 `TopLevel` 请求一次动画帧，确认准备态进入渲染管线后才安装 transition 并写入可见终态。Dispatcher priority 不构成
渲染帧边界，不能替代该帧屏障。模板在附着 TopLevel 前应用时只保留准备态，卡片附着后重新提交当前 actor 并请求帧；
初始化过程中的临时 detach 不得把尚未完成的进入动效错误标记为完成。

新增或移除卡片时，外层队列位置与内层卡片进入/退出并行过渡。展开/折叠和普通队列重排同样使用
`MotionDurationMid` 与上述 easing；中途再次变化时从当前呈现值连续转向新目标，不排队播放过时状态。

presenter 记录最后一次来自真实 pointer 事件的屏幕坐标。窗口 resize 或 placement 布局移动改变 presenter bounds 时，必须
用新的窗口坐标系重新验证该点是否仍位于 presenter 内；若已移出，则同步清除 Stack 展开状态与全局生命周期暂停。重复的
PointerEntered / PointerExited 不能被缓存布尔值直接丢弃，而应把 panel 和 manager 状态重新收敛到当前 pointer 事实。该机制
不订阅全局输入，不创建 timer，detach / retemplate 后不得保留 TopLevel 引用。

堆叠状态过渡只创建当前变换所需的 compositor 友好动画。`IsMotionEnabled=false`、首次稳定投影、有效时长不大于零、
template detach 和 dispose 时直接收敛到终态并取消未完成动画。每个 card 只允许一个进入任务和一个受
`MotionExecutionState` 约束的退出任务；retemplate/detach 必须取消旧 actor 的执行，旧模板的 completion 无权修改新
presenter 或提交关闭状态。Position/Duration 在当前 motion 播放中变化时不重启同一 actor，新的配置从下一次 motion 生效，
避免回拉和 continuation 竞争。完成、取消和异常路径都必须释放 transition completion 订阅及 cancellation source。

进入与退出保存当前实际 motion task 及其 actor，作为同一 actor 写入的交接边界。取消 token 只表示取消请求；复用
actor 时，新 motion 必须等待旧 motion 的异步清理完成后才写入，避免旧 finally 清除新退出目标。新模板的不同 actor
无需等待旧 actor 清理。等待可以由新执行自己的 token 取消；记录的任务只有在引用仍匹配时清空，保证重套模板、detach
和 dispose 不会让旧执行覆盖新执行。

## 8. 生命周期与资源释放矩阵

| 获取点 | 资源 | 释放点 |
| --- | --- | --- |
| manager 安装到 TopLevel | host layer、safe-area / TopLevel 订阅 | rehost、detach、dispose |
| `OnApplyTemplate` | 当前 presenter、pointer 与状态协作 | 下次模板应用、detach、dispose |
| Show 有限时长项 | scheduler entry、最近 deadline 唤醒 | close、destroy、dispose；detach 只暂停唤醒并保留 remaining，供 reattach 继续 |
| card 关闭接入 | closed 事件、owner 引用、用户回调 | 关闭完成；异常路径也清理 |
| Notification 可见进度 | 进度刷新登记 | 隐藏、关闭、禁用进度、重套模板、detach、dispose |
| 堆叠状态动效 | transform / card opacity / clip animation、弱引用投影快照 | 完成、卡片移除、状态替换、禁用 motion、detach、dispose |
| Notification 折叠内容快照 | 最多三张卡片各一个 `RenderTargetBitmap`、正文临时绘制/命中状态 | transform 到达本次折叠目标、中途展开、关闭、重套模板、禁用 motion、detach、dispose 或捕获失败 |
| 卡片进入动效 | 当前 actor、animation task、一次性 animation-frame callback、cancellation source / registration | 完成、开始关闭、retemplate、detach、dispose；frame callback 不捕获 card 或 coordinator |

`Dispose()` 可重复调用。detach 后 manager 可以重新 attach；旧 host、旧 presenter、旧 timer tick 和旧 card 不得被静态
事件、Dispatcher queue、动画 continuation 或 collection 间接保留。共享设施不创建全局 cache 或永久订阅。

## 9. 性能边界

- manager collection 与 item container 在普通/折叠切换间保持稳定；不得通过 Clear/Add 或 reparent 重建。
- `MeasureOverride` / `ArrangeOverride` 使用索引循环和复用状态；禁止 LINQ、闭包、临时列表与每帧 transform 创建。
- Notification 的 translate/fade actor 不得使用会在每个动画帧使 Measure 失效的 layout-aware 模式；进出场只更新内部
  render transform 和 opacity。
- 默认关闭 Stack 的普通 Notification 路径不创建折叠裁剪状态；进入准备只请求一次动画帧，不注册持续逐帧 managed 回调。
- 四个方向偏移 transform 为不可变共享值；每次 show 不得重新分配相同的 `64 DIP` 方向 transform。
- 卡片目标位置未变化时复用缓存 transform；每张卡片保留两个非 identity 目标槽位，以覆盖展开/折叠往返而不重复创建。
  只有稳定目标落在缓存外时才创建新 transform，由 Avalonia transition/compositor 插值，不注册逐帧 Dispatcher 回调。
- 折叠内容快照数量严格限制为折叠稳态可见层数，最多三个；最新卡片没有位移、缩放或裁剪变化时不捕获。快照完成条件由
  已有 `RenderTransform` 属性变化到本次目标矩阵驱动，不创建 timer、全局输入订阅或独立逐帧 managed callback。
- 显式快照是短生命周期资源，不是永久 `CacheMode`。每次折叠最多捕获一次；完成、打断及所有生命周期出口都必须先恢复
  真实正文，再对位图执行一次 `Dispose()` 并清空引用。
- motion-disabled 路径直接排列可见终态，并维持隐藏子项的有效展开布局，避免无效 Arrange 重试和无意义 transform 写入。
- 只有实际几何变化使布局失效；纯 opacity / scale 过渡不重复测量所有不可见旧项。
- 折叠稳态的不可见旧 Notification 仍保留逻辑状态，但跳过绘制、命中和进度刷新；折叠过渡的 transform、scale、clip 与
  card opacity 目标仍须同步提交。
- 性能验收同时比较 Message 与 Notification 的 show、close、stack toggle 和 Gallery 首帧。以不少于 10 次冷样本及
  多轮 mean / median / P95 判断；主要指标不得出现显著或无归因回退。可观察效果新增的可测量成本必须给出绝对值、
  缩放边界和对照样本，并继续消除与该效果无关的属性通知、布局或资源分配。
- 优化不能通过减少功能、缩短内容、取消动效或改变样本达到。内存验收必须证明重复 show/destroy/toggle/retemplate/
  detach 后 manager、presenter、card、timer 与旧 Gallery 均可回收。

## 10. AOT 与兼容边界

共享类型采用强类型属性、静态 AXAML 和已有注册机制，不使用运行时反射、动态程序集扫描、字符串属性访问或
运行时生成模板。若新内部 ControlTheme 或 Panel 进入链接注册范围，必须同步相应生成输入并执行 Gallery NativeAOT 验证。

本设计有意统一 Message 与 Notification 的 stack API，并调整 Message 的 Position、默认时长、MaxItems 默认值以及
Notification 的 MaxItems 默认值。除此之外，内容对象、类型枚举、关闭事件、卡片 public 状态和既有主题 key 保持稳定。

## 11. 验证契约

| 范围 | 必须证明的不变量 |
| --- | --- |
| 阈值 | `n == threshold` 不折叠，`n == threshold + 1` 折叠；运行时切换立即生效 |
| Message 折叠 | 仅最新真实卡片可见，两层背板尺寸、偏移、阴影、命中和过渡正确 |
| Notification 折叠 | 变高卡片位置、`Min(3, threshold)` 可见层、折叠 extent、scale、z-order、clip 与 Top/Bottom transform origin 正确；阈值外旧项同步取得最深层 scale、半裁剪和 card opacity 目标；平铺到折叠只冻结实际变化的可见正文，反向展开与打断立即恢复真实正文 |
| 进入/退出 | 四类宿主方向均从/向 `64 DIP` 偏移；全程 scale=1；opacity 和 translate 使用完整 200ms ease-in-out |
| 队列重排 | 新增、关闭、移除、展开和折叠只改变有效投影目标；退出项保留最后投影，其他卡片从关闭开始同步让位 |
| Hover | gap 内不塌缩；数量和阈值跨界时按当前指针状态重算；Stack 开启时仅在实际 hover 期间暂停全部 |
| 时钟 | 默认时长、零时长、Stack 开启但未 hover 时仍到期、暂停/继续、最近 deadline、多项同时到期和进度值正确 |
| 集合 | MaxItems 淘汰最旧活动项；DestroyAll 一次关闭全部；关闭回调只触发一次 |
| 模板 | retemplate 不丢项，旧 presenter 不再接收状态；禁用 motion 直接收敛 |
| 生命周期 | detach/reattach、rehost、dispose、回调异常及延迟 Dispatcher 操作均无保留链；折叠完成、打断、关闭和重套模板均释放内容位图 |
| 性能 | 基线与优化样本同策略，show/close/toggle/首帧主要指标无可测量回退，热路径无新分配 |
| 平台 | Desktop 与 Browser 主题语义一致；链接注册或模板类型变化通过 NativeAOT 检查 |

自动化测试使用可控时钟验证剩余时间和 deadline，不依赖真实 sleep。内存测试以 `WeakReference` 和强制 GC 验证 owner
链释放以及移出 panel 的 card 不会被 transform cache、transition completion 或 cancellation source 保留；视觉测试同时
检查锚点 Bounds、目标 transform、opacity、IsVisible 与 hit-test，不能只断言单个伪类或最终集合数量。录像走查必须在
60fps 下覆盖连续快速新增、自动关闭、DestroyAll、阈值折叠和 hover 展开，并与参考动效比较进入方向、显现节奏和已有项
让位的连续性。
