# Feedback 堆叠基础设施

本文定义 Message 与 Notification 共用的堆叠、生命周期计时和集合呈现契约。共享实现位于
`src/AtomUI.Desktop.Controls/Primitives/FeedbackStack`，控件管理器继续拥有各自的 public API、内容模型、位置和关闭事件。目录入口见
[Control 基础设施](overview.md)，控件入口见 [Message](../../../controls/desktop/feedback/message/overview.md) 与
[Notification](../../../controls/desktop/feedback/notification/overview.md)。

本文是两个反馈控件堆叠行为的唯一设计 owner。控件文档只描述各自差异和接入点，不复制共享算法。

## 1. 设计定位与不变量

反馈堆叠把按创建顺序保存的卡片集合投影为展开或折叠布局。它不改变 Message 和 Notification 的内容、类型图标、
关闭事件及进入/退出动效语义。

- 最新卡片始终靠近宿主边缘；Top 位置向下展开，Bottom 位置向上展开。
- `StackThreshold` 只决定何时折叠。只有当前活动卡片数严格大于阈值时才进入折叠态。
- 指针进入整个堆叠命中区域后展开全部活动卡片，离开后恢复折叠；卡片之间的 gap 也属于该命中区域。
- Message 折叠时只显示最新一张真实卡片，并用两层静态模板背板表达深度。
- Notification 折叠时显示最新三张真实卡片；第三张之后的旧卡片保留在集合中但不参与命中和绘制。
- 折叠仅改变视觉投影，不销毁旧项、不重置剩余时长、不改变 `MaxItems` 语义。
- 计时使用单调时间和剩余时长；暂停后从剩余时长继续，不能重新开始完整时长。
- 一个管理器最多拥有一个惰性调度器；空集合、全部永久展示、detach 或 dispose 时不得保持活动 timer。
- 布局热路径不得通过 LINQ、临时数组或逐帧重建视觉树产生分配。
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
| `IsStackEnabled` | `false` | `true` | 是否允许超过阈值后折叠 |
| `StackThreshold` | `3` | `3` | 活动项数量严格大于该值时折叠；有效值最小为 `1` |
| `IsPauseOnHover` | `true` | `true` | 是否在指针悬停时暂停自动关闭 |
| `MaxItems` | `0` | `0` | `<= 0` 表示不限制；正数表示达到上限时关闭最旧活动项 |
| `Position` | `TopCenter` | `TopRight` | 堆叠的宿主边和横向对齐 |
| `IsMotionEnabled` | SharedToken `EnableMotion` | SharedToken `EnableMotion` | 是否播放卡片和堆叠状态动效 |

`IMessageManager` 与 `INotificationManager` 均公开 `DestroyAll()`。它为所有活动卡片发起正常关闭并释放计时登记；
已经关闭的卡片不会重复回调。Message 的默认展示时长为 3 秒，Notification 为 4.5 秒；时长为零表示永久展示。

本次契约允许修正旧默认值和移除基于轮询实现暴露的计时间隔属性。计时机制是实现细节，不属于控件可定制 API。

## 4. 状态模型

设活动项数为 `n`，有效阈值为 `t = Max(1, StackThreshold)`：

| 条件 | 状态 | Message 投影 | Notification 投影 |
| --- | --- | --- | --- |
| `IsStackEnabled == false` | 普通 | 全部按 16 DIP gap 展开 | 全部按 16 DIP gap 展开 |
| `n <= t` | 普通 | 全部展开 | 全部展开 |
| `n > t && !IsPointerOver` | 折叠 | 最新 1 张真实卡片 + 2 层背板 | 最新 3 张真实卡片 |
| `n > t && IsPointerOver` | 展开堆叠 | 全部展开 | 全部展开 |

正在退出的卡片在退出动效完成前仍由集合持有，但不计入新的 `MaxItems` 淘汰选择。布局层必须给退出 actor 保留确定的
绘制窗口，完成后由 manager 一次性移除。运行时切换 `IsStackEnabled` 或 `StackThreshold` 立即重新投影当前集合，
不重建卡片、不改变剩余时长。

Stack hover 属于 presenter 的整体状态，不由单个卡片竞争写入。非 Stack 普通状态下，`IsPauseOnHover=true` 只暂停实际
悬停的卡片；处于折叠或由折叠展开的 Stack 状态时，悬停暂停该管理器全部活动卡片。`IsPauseOnHover=false` 时 hover
仍可展开堆叠，但不影响计时。

## 5. 展开与折叠几何

布局单位均为 DIP。通用展开 gap 为 16，折叠 offset 为 8。最新项索引 `k=0`，更旧项依次为 `k=1,2...`。

### 5.1 展开态

Top 位置从顶部边距向下累计 `cardHeight + 16`；Bottom 位置从底部边距向上镜像累计。横向位置由
Left / Center / Right 决定。Message 的顶部边距为 8；Notification 对宿主各边的距离为 24。

Items presenter 的 Bounds 覆盖所有可见卡片及卡片间 gap，使从一张卡片移动到另一张时不会产生 hover 抖动。
隐藏旧卡片不参与 hit test，也不能遮挡展开项。

### 5.2 Message 折叠态

最新真实卡片保持 scale 1 和完整 opacity。其后两层为 AXAML 静态背板，不能为每条 Message 动态创建：

| 层 | 顶部偏移 | 宽度变化 | 可见性 |
| --- | --- | --- | --- |
| 第一背板 | 最新卡片高度减 8 | 左右各缩进 8，总宽度减 16 | 折叠且至少 2 个活动项 |
| 第二背板 | 最新卡片高度 | 左右各缩进 16，总宽度减 32 | 折叠且至少 3 个活动项 |

背板高度为 16，使用 tertiary shadow，`IsHitTestVisible=false`。背板只表达堆叠深度，不承载内容、计时或关闭事件。
宽度变化时使用 300ms 过渡，opacity 与位置使用 100ms 过渡；关闭 motion 和全局 motion 禁用仍拥有更高优先级。

### 5.3 Notification 折叠态

折叠时最多呈现最新三张真实卡片：

```text
offset(k) = k * 8
scale(k)  = 1 - k * 0.06,  k = 0, 1, 2
```

因此三层 scale 分别为 `1`、`0.94`、`0.88`。Top 位置的 transform origin 位于 center/top，Bottom 位置位于
center/bottom；展开方向与宿主边相反。卡片转换使用 transform、opacity、clip 和 z-order，不通过 width、height 或
margin 动画触发布局风暴。展开/折叠布局过渡为 200ms。

## 6. 生命周期调度

`FeedbackLifetimeScheduler` 为每个 manager 惰性创建，保存每个活动项的 deadline、remaining 和 pause state。它使用
单调时钟，不直接递减 public `Expiration`。调度顺序：

1. 新增有限时长项时记录 `deadline = now + duration`；永久项不登记 deadline。
2. 没有进度条刷新需求时，只安排距离最近 deadline 的单次唤醒，不执行固定频率全表扫描。
3. Notification 需要可见进度时，以受控刷新节奏只更新可见且活动的进度项；没有这类项时回到最近 deadline 模式。
4. 进入暂停状态时一次性计算并保存 remaining，取消当前唤醒；继续时从 remaining 建立新 deadline。
5. deadline 到达后为所有已到期项发起一次关闭，并立即移除调度登记。
6. 项关闭或 `DestroyAll()` 时移除对应登记；manager detach 时暂停唤醒，manager dispose 时清空登记、停止 timer 并解除 tick。

调度器不拥有用户 delegate。manager 在关闭完成后调用一次 `OnClose`，并在 `finally` 语义下清空 delegate、卡片 owner
引用和事件订阅。回调异常不能阻止集合与调度资源清理。

## 7. Template、资源与动效

`PART_Items` 保持稳定名称和 `ItemsControl` 级模板协作语义。Manager 持有稳定 collection 并绑定为 ItemsSource；
重新应用模板只替换 presenter，不会把卡片从旧 `Panel.Children` 搬运或丢失。

共享 presenter theme 提供布局 panel 和状态 selector。Message 的两层背板在 manager AXAML 中静态声明；
Notification 折叠层由真实 item container 表达。状态从 manager 属性单向投影到 presenter，不能由模板节点反写 manager。

堆叠状态过渡只创建当前变换所需的 compositor 友好动画。`IsMotionEnabled=false`、首次稳定投影、有效时长不大于零、
template detach 和 dispose 时直接收敛到终态并取消未完成动画。旧模板的 completion 无权修改新 presenter。

## 8. 生命周期与资源释放矩阵

| 获取点 | 资源 | 释放点 |
| --- | --- | --- |
| manager 安装到 TopLevel | host layer、safe-area / TopLevel 订阅 | rehost、detach、dispose |
| `OnApplyTemplate` | 当前 presenter、pointer 与状态协作 | 下次模板应用、detach、dispose |
| Show 有限时长项 | scheduler entry、最近 deadline 唤醒 | close、destroy、dispose；detach 只暂停唤醒并保留 remaining，供 reattach 继续 |
| card 关闭接入 | closed 事件、owner 引用、用户回调 | 关闭完成；异常路径也清理 |
| Notification 可见进度 | 进度刷新登记 | 隐藏、关闭、禁用进度、重套模板、detach、dispose |
| 堆叠状态动效 | transform / opacity animation | 完成、状态替换、禁用 motion、detach、dispose |

`Dispose()` 可重复调用。detach 后 manager 可以重新 attach；旧 host、旧 presenter、旧 timer tick 和旧 card 不得被静态
事件、Dispatcher queue、动画 continuation 或 collection 间接保留。共享设施不创建全局 cache 或永久订阅。

## 9. 性能边界

- manager collection 与 item container 在普通/折叠切换间保持稳定；不得通过 Clear/Add 或 reparent 重建。
- `MeasureOverride` / `ArrangeOverride` 使用索引循环和复用状态；禁止 LINQ、闭包、临时列表与每帧 transform 创建。
- 只有实际几何变化使布局失效；纯 opacity / scale 过渡不重复测量所有不可见旧项。
- 折叠时不可见旧 Notification 仍保留逻辑状态，但跳过绘制、命中和进度刷新。
- 性能验收同时比较 Message 与 Notification 的 show、close、stack toggle 和 Gallery 首帧。以不少于 10 次冷样本及
  多轮 mean / median / P95 判断；主要指标不得出现可测量回退。
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
| Notification 折叠 | 最新三层的 offset/scale、z-order、clip 与 Top/Bottom transform origin 正确 |
| Hover | gap 内不塌缩；进入展开全部、离开折叠；普通模式只暂停悬停项，Stack 模式暂停全部 |
| 时钟 | 默认时长、零时长、暂停/继续、最近 deadline、多项同时到期和进度值正确 |
| 集合 | MaxItems 淘汰最旧活动项；DestroyAll 一次关闭全部；关闭回调只触发一次 |
| 模板 | retemplate 不丢项，旧 presenter 不再接收状态；禁用 motion 直接收敛 |
| 生命周期 | detach/reattach、rehost、dispose、回调异常及延迟 Dispatcher 操作均无保留链 |
| 性能 | 基线与优化样本同策略，show/close/toggle/首帧主要指标无可测量回退，热路径无新分配 |
| 平台 | Desktop 与 Browser 主题语义一致；链接注册或模板类型变化通过 NativeAOT 检查 |

自动化测试使用可控时钟验证剩余时间和 deadline，不依赖真实 sleep。内存测试以 `WeakReference` 和强制 GC 验证 owner
链释放以及移出 panel 的 card 不会被 transform cache 保留；视觉测试同时检查 Bounds、transform、opacity、IsVisible 与
hit-test，不能只断言单个伪类或最终集合数量。
