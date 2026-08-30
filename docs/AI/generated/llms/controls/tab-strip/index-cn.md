# TabStrip

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## 概述

TabStrip 是 AtomUI 桌面控件体系中的标签条控件，用于展示、选择、关闭和拖动排序页签，不承载内容页。

TabStrip 不负责完整 TabControl 内容容器或主导航菜单。这些职责应由业务层、组合控件或更专用的 AtomUI 控件承担。

主要源码入口：

- `src/AtomUI.Desktop.Controls/TabControl/TabStrip`

## 包与命名空间

| 项 | 值 |
| --- | --- |
| NuGet 包 | `AtomUI.Desktop.Controls` |
| .NET 命名空间 | `AtomUI.Desktop.Controls` |
| AXAML 命名空间 | `https://atomui.net` |
| Gallery 页面 | `controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip` |
| 状态 | Stable |

## 何时使用

TabStrip 的设计语言围绕控件职责、可观察状态和主题契约组织，而不是围绕模板节点组织。

| 维度 | 含义 | TabStrip 中的表达 |
| --- | --- | --- |
| 产品语义 | 控件在界面中承担的稳定职责。 | TabStrip 是 AtomUI 桌面控件体系中的标签条控件，用于展示、选择、关闭和拖动排序页签，不承载内容页。 |
| 内容承载 | 用户数据、展示内容、集合项或操作入口如何进入控件。 | `CloseIcon`、`HeaderEndEdgePadding`、`HeaderEndExtraContent`、`HeaderEndExtraContentTemplate`、`HeaderStartEdgePadding`、`HeaderStartExtraContent`、`HeaderStartExtraContentTemplate`、`Icon`。 |
| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | selection/checked/active、reorder、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | SharedToken / 关联控件 Token + ControlTheme。 |

## 公共 API

TabStrip 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。

核心 public surface 按语义分组维护：

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `CloseIcon`、`HeaderEndEdgePadding`、`HeaderEndExtraContent`、`HeaderEndExtraContentTemplate`、`HeaderStartEdgePadding`、`HeaderStartExtraContent`、`HeaderStartExtraContentTemplate`、`Icon`、`OverflowPopupTemplate` | 定义控件展示内容、输入数据、模板或业务对象入口；`OverflowPopupTemplate` 的 data item 固定为 `TabOverflowPopupContext`。 |
| 选择与集合 | `IsTabReorderEnabled`、`TabActivationTrigger`、`SelectedIndex`、`SelectedItem`、`ItemsSource` | 维护页签选择触发时机、集合顺序和拖动排序状态。 |
| 交互与状态 | `IsAutoHideCloseButton`、`IsClosable`、`IsMotionEnabled`、`IsShowAddTabButton`、`IsTabAutoHideCloseButton`、`IsTabClosable` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `SizeType`、`TabAlignmentCenter`、`TabStripPlacement` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |

稳定事件包括 `AddTabRequest`、`Closed`、`Closing`、`TabReordering`、`TabReordered`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。

主要公开类型与枚举：

- 控件与数据类型：`BaseTabStrip`、`CardTabStrip`、`TabStrip`、`TabStripClosedEventArgs`、`TabStripClosingEventArgs`、`TabStripItem`、`TabOverflowPopupContext`、`TabOverflowItem`。
- 枚举：`TabActivationTrigger`、`TabSharp`。
- `TabScrollViewer`、`ITabOverflowOwner`、默认 overflow menu 及其 item container 均为 internal 实现，不属于用户 API。

稳定 template part：

当前 owner 没有显式 `[TemplatePart]` 契约；共用 `TabScrollViewerTheme` 的 `PART_OverflowPopup`、
`PART_ScrollStartEdgeIndicator`、`PART_ScrollEndEdgeIndicator` 与 `PART_ScrollMenuIndicator` 是家族内部模板协作边界。

控件专属或内部伪类包括 `TabPseudoClass.Bottom`、`TabPseudoClass.Left`、`TabPseudoClass.Right`、`TabPseudoClass.Top`。这些伪类属于主题 selector 可观察契约，不能在未同步主题和 Gallery 的情况下重命名或删除。

## 事件与命令

TabStrip 的公共契约由 public/protected 类型成员、Avalonia 属性、事件、命令、template part、伪类、ControlTheme key 和资源 key 共同组成。维护时应先确认这些契约是否已经被源码、Gallery 示例或文档暴露。
稳定事件包括 `AddTabRequest`、`Closed`、`Closing`、`TabReordering`、`TabReordered`。事件触发顺序属于兼容契约，不能因内部状态重排而改变。
- 控件与数据类型：`BaseTabStrip`、`CardTabStrip`、`TabStrip`、`TabStripClosedEventArgs`、`TabStripClosingEventArgs`、`TabStripItem`、`TabOverflowPopupContext`、`TabOverflowItem`。

## 使用示例

稳定示例来源于 Gallery ShowCase 和源码查看片段。生成器只输出可从 `ShowCaseItem` 追溯的示例，不维护第二套手写示例。

以下示例来自 Gallery 源码查看使用的 `ShowCaseItem` 片段，并已按中文资源规范化。

### 基础用法

来源：`controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip/Views/TabStripShowCase.axaml:122`

Gallery key：`ExamplesContent` / item `0`

```axaml
<StackPanel Orientation="Vertical" Spacing="20">
    <atom:TabStrip>
        <atom:TabStripItem IsClosable="True" Content="标签页 1" />
        <atom:TabStripItem IsClosable="True" Content="标签页 2" />
        <atom:TabStripItem IsClosable="True" Content="标签页 3" />
    </atom:TabStrip>
</StackPanel>
```

### 通过 ItemSource 生成 TabStripItem

来源：`controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip/Views/TabStripShowCase.axaml:139`

Gallery key：`ExamplesContent` / item `1`

```axaml
<StackPanel Orientation="Vertical" Spacing="20">
    <atom:TabStrip ItemsSource="{Binding TabStripItemDataSource}">
        <atom:TabStrip.ItemTemplate>
            <DataTemplate x:DataType="atom:TabItemData">
                <TextBlock Text="{Binding Header}" />
            </DataTemplate>
        </atom:TabStrip.ItemTemplate>
    </atom:TabStrip>
</StackPanel>
```

### 禁用标签

来源：`controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip/Views/TabStripShowCase.axaml:248`

Gallery key：`ExamplesContent` / item `4`

```axaml
<StackPanel Orientation="Vertical" Spacing="20">
    <atom:CardTabStrip>
        <atom:TabStripItem Content="标签页 1" />
        <atom:TabStripItem IsEnabled="False" IsClosable="True" Content="标签页 2" />
        <atom:TabStripItem Content="标签页 3" />
    </atom:CardTabStrip>

    <atom:TabStrip>
        <atom:TabStripItem Content="标签页 1" />
        <atom:TabStripItem IsEnabled="False" Content="标签页 2" />
        <atom:TabStripItem Content="标签页 3" />
    </atom:TabStrip>
</StackPanel>
```

### 居中显示

来源：`controlgallery/AtomUIGallery/ShowCases/Navigation/TabStrip/Views/TabStripShowCase.axaml:271`

Gallery key：`ExamplesContent` / item `5`

```axaml
<StackPanel Orientation="Vertical" Spacing="20">
    <atom:TabStrip TabAlignmentCenter="True">
        <atom:TabStripItem Content="标签页 1" />
        <atom:TabStripItem Content="标签页 2" />
        <atom:TabStripItem Content="标签页 3" />
    </atom:TabStrip>

    <atom:CardTabStrip TabAlignmentCenter="True">
        <atom:TabStripItem Content="标签页 1" />
        <atom:TabStripItem Content="标签页 2" />
        <atom:TabStripItem Content="标签页 3" />
    </atom:CardTabStrip>
</StackPanel>
```

## 状态模型

TabStrip 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- selection/checked/active、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。
- Tab 激活触发由 `TabActivationTrigger` 控制，默认值为 `PointerReleased`；按下时只记录候选 Tab，只有鼠标在同一个 Tab 上松开才激活。
- `TabActivationTrigger=PointerPressed` 表达按下立即激活；该模式仍必须通过统一选择入口更新 `SelectedIndex`、`SelectedItem`、伪类和主题状态。
- `PointerReleased` 模式下，按下 Tab A、移动到 Tab B 或 Tab 外松开不应激活新 Tab；拖动排序进入 active reorder 后，释放事件不得再触发 Tab 激活。
- `IsTabClosable` 是生成 `TabStripItem` 的模板级默认值；overflow 菜单使用容器最终生效的 `IsClosable`，因此控件级默认、单项覆盖和 overflow 呈现必须保持同一语义。
- 拖动排序开启后，排序结果必须提交到 `ItemsSource` 或 `Items` 的逻辑集合顺序；拖动过程采用 Chrome 式轨道内实时让位预览，被拖 Tab 只沿 Tab 轨道主轴移动并覆盖在兄弟 Tab 上方，其他 Tab 通过临时 transform 让出目标位置，不能直接把 `ItemsPresenter.Panel.Children` 当作排序数据源。
- `TabStripPlacement=Top/Bottom` 时主轴为 X 轴，被拖 Tab 的 Y 位移必须保持为 0；`TabStripPlacement=Left/Right` 时主轴为 Y 轴，被拖 Tab 的 X 位移必须保持为 0。目标位置由被拖 Tab 的前进边缘跨过被覆盖兄弟 Tab 主轴中线决定：向后拖动使用 trailing edge，向前拖动使用 leading edge，相当于覆盖兄弟 Tab 约一半宽度或高度即触发让位，而不是由 pointer 的非主轴偏移决定。
- overflow 是当前打开会话的不可变 `TabOverflowItem` 快照，不拥有独立的选择或关闭语义；`TryActivate` 与 `TryClose` 必须经 `BaseTabStrip` 统一提交，旧会话 item 必须被拒绝。
- `OverflowPopupTemplate=null` 使用默认菜单；非空模板只替换弹层内容，不能改变溢出判定、placement、light-dismiss、选择或关闭 owner。
- `OverflowPopupTemplate` 默认值为 `null`，由 `TabStrip` 与 `CardTabStrip` 继承。模板 data item 固定为
  `TabOverflowPopupContext`；模板只能通过 `TryActivate`、`TryClose` 与 `Dismiss` 提交操作。
- pointer 点击 `PART_ScrollMenuIndicator` 打开 overflow Popup 时不得自动聚焦选中项、第一项、Popup 根节点或搜索框；焦点保持在激活器，只有用户后续显式 Tab/方向键导航或点击输入框时才进入弹层内容。
- 水平布局必须先为可见的 `PART_ScrollMenuIndicator` 保留空间；父级宽度缩窄、瀑布流换列或最终 arrange 小于先前 measure 时，激活器仍必须可见且完整落在 owner 边界内。

## 主题与 Design Token

TabStrip 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

TabStrip 的 Line/Card ControlTheme 位于 `TabControl/Themes/TabStrip`，并复用 TabControl 家族统一的
`TabScrollViewerTheme` 与 `TabOverflowMenuTheme`。统一主题承载 edge indicator、更多按钮、静态 Popup shell 和默认菜单；
TabStrip 自身主题只负责 owner 布局与外观映射。

TabStrip 当前没有专属 Token 文档；主题通过 SharedToken、关联控件 Token 或继承主题资源表达视觉语义。运行时状态不得写入 Token 模型。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- 默认 `TabOverflowMenuTheme` 必须根据不可变 projection 的 `IsClosable` 控制关闭入口：不可关闭项不显示也不命中关闭按钮；可关闭项只通过 context `TryClose` 转发。
- 自定义 `OverflowPopupTemplate` 的 surface、搜索和空状态由应用模板负责；Popup host 仍由 AtomUI 负责定位、light-dismiss、pinned 与生命周期释放。
- Popup host 沿嵌套 `ContentPresenter` 解析最终 surface 的圆角；模板根与可见背景必须暴露一致的
  `CornerRadius`，item header/template 必须通过控件自身的 content pipeline 呈现，不能生成空白菜单项。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

Token 来源：

- TabStrip 当前没有专属 `token.md`；LLMS 生成按第 5 节视觉与主题模型、SharedToken、控件家族 Token 或主题资源说明 Token 边界。

## AOT 与裁剪注意事项

资源和 AOT 约束：

- 不通过运行时反射扫描 public API、Token 或 Gallery 示例数据。
- 不把可静态声明的模板结构迁移到 C# 动态创建。
- 异步加载、上传、弹层和窗口生命周期必须能取消或释放。
- 缓存对象必须与控件、窗口、弹层或数据 owner 生命周期一致。
- Source generator 生成文件不手工编辑；需要修改时改输入源或 generator。

性能边界：

- 控件应优先复用 Avalonia 原生虚拟化、模板绑定和资源系统。
- 避免为每次状态变化创建不必要的视觉对象、订阅或动画对象。
- edge indicator 使用模板内固定 `TabOverflowEdgeIndicator` 和 family Token 化 `BoxShadows`；滚动或测量期间只切换可见性，不创建 `LinearGradientBrush`、gradient stop、binding 或额外视觉树。绘制时创建不可变的 `Rect` / `BoxShadows` 快照，由专用 draw operation 消费。
- 大集合控件必须保证 container recycle 后不会泄漏旧 item 状态。
- 拖动 move 帧内只更新轻量 transform、目标 index 和自动滚动请求；主轴约束和目标 index 计算必须是纯几何计算，不得在 pointer move 中反复移动集合、重建 item 容器或重新应用模板。
- 拖动预览 transform、绘制层级、计时器和订阅应按交互会话缓存并在会话结束释放；不得因一次拖动永久保留视觉对象或数据 item。
- 拖动排序不得引入运行时反射、动态类型扫描或 AOT 不友好的事件发现路径。
- 图标槽对齐不得为无图标 Tab 创建额外图标控件、动态占位对象或 C# 运行时模板分支；应复用静态 AXAML 槽位、现有资源绑定和内部布尔状态，避免增加模板实例化和 container recycle 成本。
- 未打开实例只有一个无 Child 的静态 Popup shell，不建立打开态订阅；第一次打开创建一次内容树，重复打开只重建 immutable projection。
- `PART_ScrollMenuIndicator.Click` 与 `PART_OverflowPopup.Closed` 是 template-part 生命周期订阅，可在关闭态存在，但必须在 re-template / detach 时成对解除；“无打开态订阅”只指 collection/selection、per-item、command、dispatcher 与 owner-action 会话订阅。
- 每次打开不得创建 Flyout、`CompositeDisposable`、relay binding、per-item delegate、command 或 dispatcher closure；默认
  item container 关闭时清除旧 item/header/template、DataContext 及标题 presenter 的模板子树。默认菜单拥有仅包含空容器的局部缓存，容量随最近一次非空快照缩小；Context 切换时丢弃缓存，完整 teardown 后随菜单根一起释放。
- 自定义搜索 item 直接把 `Header` / `HeaderTemplate` 交给按钮的原生 content pipeline；模板根圆角与可见 surface
  必须一致。替换嵌套 presenter 的最终 surface 后，旧 surface 必须可以被 GC 回收。
- 重复 open/close 的 allocated bytes/op 与 Gen0 压力必须相对旧基线至少下降 30%；never-open 创建、布局、滚动不得出现超过 5% 的稳定回退。
- context 只弱引用 action target；关闭态 Items 为空且无活动打开会话订阅，完整 teardown 后 cached root、context、template 与 DynamicResource anchor 均不得被旧会话保留。
- Public context、compiled AXAML 和静态 Theme 注册必须保持 AOT 友好，不引入 reflection binding、type scan 或动态注册。

## 源码索引

主要源码文件：

- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/BaseTabStrip.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/CardTabStrip.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/CardTabStrip.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStrip.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStrip.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStripItem.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabStrip/TabStripItem.SemanticParts.cs`
- `src/AtomUI.Desktop.Controls/TabControl/TabScrollViewer.cs`：四控件共用的 internal sealed 滚动、溢出、Popup 与会话 owner。
- `src/AtomUI.Desktop.Controls/TabControl/TabOverflowPopupContext.cs`：public context、不可变 item projection 与内部 weak action bridge。
- `src/AtomUI.Desktop.Controls/TabControl/TabOverflowMenu.cs`：默认菜单及其 internal item container；不承载 owner-specific 分支。
- `src/AtomUI.Desktop.Controls/TabControl/Themes/TabScrollViewerTheme.axaml`：三个 indicator、更多按钮、滚动 presenter 与静态 `PART_OverflowPopup` shell。
- `src/AtomUI.Desktop.Controls/TabControl/Themes/TabOverflowMenuTheme.axaml`：默认 menu-like 内容、selected/disabled/closable 视觉。

职责边界：

- 控件主文件保留 public/protected API、Avalonia 属性注册、事件和主要生命周期入口。
- Theme 文件负责静态视觉结构、template part、selector 和资源绑定。
- Token 文件只提供组件视觉变量，不保存实例状态。
- Gallery 文件只展示用法和示例，不作为运行时逻辑 owner。
- Tab 拖动排序属于 `BaseTabStrip` 的集合与选择协作路径；实现应落在 `BaseTabStrip`、`TabStripItem`、滚动视口和内部拖动协作对象之间，不能把排序状态散落到 Gallery、theme 或业务数据对象中。
- overflow 属于 `TabScrollViewer` 打开会话的不可变 projection；public context 只提供经 owner 验证的 action，`BaseTabStrip` 仍是选择、关闭事件和集合变更的唯一 owner。
- 垂直页签图标对齐属于 `BaseTabStrip` 的 owner 级布局状态；`Left` / `Right` placement 下由 owner 统一判断同组是否存在图标，再把内部保留图标槽状态投射到 `TabStripItem`，不能通过 Gallery 手工补空图标或新增 public API。
- 默认 Line TabStrip 的 `Left` / `Right` placement 应保持紧凑的垂直节奏；相邻间距和 item 自身垂直 padding 都应按 Line 紧凑模型处理。Card TabStrip 使用独立 `CardGutter` 和 Card padding 视觉节奏，本规则不得改变 Card 外观。

## 相关文档

- 源设计文档：`docs/controls/desktop/navigation/tab-strip/overview.md`
- 实现文档：`docs/controls/desktop/navigation/tab-strip/implementation.md`
- Semantic Part 文档：`docs/controls/desktop/navigation/tab-strip/semantic-part.md`
- 变更记录：`docs/controls/desktop/navigation/tab-strip/changelog.md`
- 语义结构：`./semantic-cn.md`
