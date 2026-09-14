# Menu 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

`Menu` descriptor 的 Part 集合（`root` 隐式，其余 11 个按路径排序）：

| Part | SelectorClass | SelectorRoute | ContractType | Cardinality | CrossVisualRoot | CrossNestedOwners | RuntimeCreated |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `root` | 不适用 | 不适用 | `Menu` | `Single` | `false` | `false` | `false` |
| `item` | `.semantic-item` | `>> .semantic-item` | `MenuItem` | `Multiple` | `false` | `true` | `true` |
| `itemContent` | `.semantic-item-content` | `>> .semantic-item /template/ .semantic-item-content` | `ContentPresenter` | `Multiple` | `false` | `true` | `true` |
| `itemIcon` | `.semantic-item-icon` | `>> .semantic-item /template/ .semantic-item-icon` | `IconPresenter` | `Multiple` | `false` | `true` | `true` |
| `itemTitle` | `.semantic-item-title` | `>> .semantic-scope-group /template/ .semantic-item-title` | `ContentPresenter` | `Multiple` | `false` | `true` | `true` |
| `list` | `.semantic-list` | `>> .semantic-scope-group /template/ .semantic-list` | `ItemsPresenter` | `Multiple` | `false` | `true` | `true` |
| `popup.root` | `.semantic-popup-root` | `>> .semantic-popup-root` | `Border` | `Multiple` | `true` | `true` | `true` |
| `subMenu.item` | `.semantic-sub-menu-item` | `>> .semantic-sub-menu-item` | `MenuItem` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemContent` | `.semantic-sub-menu-item-content` | `>> .semantic-sub-menu-item /template/ .semantic-sub-menu-item-content` | `ContentPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemIcon` | `.semantic-sub-menu-item-icon` | `>> .semantic-sub-menu-item /template/ .semantic-sub-menu-item-icon` | `IconPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.itemTitle` | `.semantic-sub-menu-item-title` | `>> .semantic-sub-menu-group /template/ .semantic-sub-menu-item-title` | `ContentPresenter` | `Multiple` | `true` | `true` | `true` |
| `subMenu.list` | `.semantic-sub-menu-list` | `>> .semantic-sub-menu-group /template/ .semantic-sub-menu-list` | `ItemsPresenter` | `Multiple` | `true` | `true` | `true` |

生成 Style 类型（`AtomUI.Theme.Styling`）：

```text
item                  -> MenuItemStyle
itemIcon              -> MenuItemIconStyle
itemContent           -> MenuItemContentStyle
itemTitle             -> MenuItemTitleStyle
list                  -> MenuListStyle
popup.root            -> MenuPopupRootStyle
subMenu.item          -> MenuSubMenuItemStyle
subMenu.itemIcon      -> MenuSubMenuItemIconStyle
subMenu.itemContent   -> MenuSubMenuItemContentStyle
subMenu.itemTitle     -> MenuSubMenuItemTitleStyle
subMenu.list          -> MenuSubMenuListStyle
```

`root` 是隐式 Part：不声明 `.semantic-root` marker，不生成 Style，通过 owner 属性、owner-scoped Style 或替换
ControlTheme 定制。

### 2.1 Part 说明

- `item` / `subMenu.item`：菜单项容器。一级与子菜单两级共用同一个 public 容器类型，靠互斥 marker 区分。一级项位于
  菜单栏第一层与一级分组内部；子菜单项位于任意深度子菜单与其内部分组。
- `itemIcon` / `subMenu.itemIcon`：菜单项图标区域。一级节点是 `TopLevelMenuItemTheme` 的
  `IconPresenter#ItemIconPresenter`；子菜单节点是 `MenuItemTheme` 的 `IconPresenter#ItemIconPresenter`。两级模板都常驻该
  节点，`Icon` 为 null 时只是隐藏，marker 不增删。
- `itemContent` / `subMenu.itemContent`：菜单项文字内容区域。一级节点是 `TopLevelMenuItemTheme` 的
  `ContentPresenter#HeaderPresenter`；子菜单节点是 `MenuItemTheme` 的 `ContentPresenter#ItemTextPresenter`。
- `itemTitle` / `subMenu.itemTitle`：分组标题区域，节点是 `MenuItemGroupTheme` 的
  `ContentPresenter#GroupTitlePresenter`。菜单栏一级项不渲染一级分组标题（上游 horizontal 语义），因此 `itemTitle` 在
  顶层解析为 0；子菜单内分组提供 `subMenu.itemTitle`。
- `list` / `subMenu.list`：分组列表区域，节点是 `MenuItemGroupTheme` 的 `ItemsPresenter#PART_ItemsPresenter`。与
  `itemTitle` 同理，`list` 在顶层解析为 0。
- `popup.root`：子菜单弹层框体。一级子菜单的弹层框体在 `TopLevelMenuItemTheme` 内，嵌套子菜单的弹层框体在
  `MenuItemTheme` 内；两个模板的 `Border#PopupFrame` 都带该 marker，因此菜单栏子菜单与嵌套子菜单都覆盖。

## Abstract AXAML Structure

来源：`src/AtomUI.Desktop.Controls/Menu/Themes/MenuTheme.axaml`

```xml
<PixelAlignedBorder>
    <ItemsPresenter Name="PART_ItemsPresenter" />
</PixelAlignedBorder>
```

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
Menu
  -> FlyoutHost (control theme, FlyoutHostTheme.axaml)
     -> ContentPresenter#PART_ContentPresenter (template-stable)
  -> MenuFlyoutPresenter (presenter control theme, MenuFlyoutPresenterTheme.axaml)
     -> ArrowDecoratedBox#{x:Static atom:AbstractArrowDecoratedBox.ArrowDecoratorPart} (template-stable)
        -> MenuPopupScrollHost (internal-observable)
           -> ItemsPresenter#PART_ItemsPresenter (template-stable)
  -> TreeViewFlyoutPresenter (presenter control theme, TreeViewFlyoutPresenterTheme.axaml)
     -> ArrowDecoratedBox#{x:Static atom:AbstractArrowDecoratedBox.ArrowDecoratorPart} (template-stable)
        -> ItemsPresenter#ItemsPresenter (internal-observable)
  -> MenuItemGroup (control theme, MenuItemGroupTheme.axaml)
     -> StackPanel (template-stable)
        -> ContentPresenter#GroupTitlePresenter (internal-observable)
        -> ItemsPresenter#PART_ItemsPresenter (template-stable)
  -> MenuItem (item container control theme, MenuItemTheme.axaml)
     -> Panel (template-stable)
        -> Border#Frame (template-stable)
           -> Grid (template-stable)
              -> Panel#ToggleItemsLayout (template-stable)
                 -> CheckBox#PART_ToggleCheckbox (template-stable)
                 -> RadioButton#PART_ToggleRadio (template-stable)
              -> IconPresenter#ItemIconPresenter (internal-observable)
              -> ContentPresenter#ItemTextPresenter (internal-observable)
              -> TextBlock#InputGestureText (template-stable)
              -> RightOutlined#MenuIndicatorIcon (template-stable)
        -> Popup#PART_Popup (template-stable)
           -> Border#PopupFrame (template-stable)
              -> MenuPopupScrollHost (internal-observable)
                 -> ItemsPresenter#PART_ItemsPresenter (template-stable)
  -> MenuPopupScrollHost (control theme, MenuPopupScrollHostTheme.axaml)
     -> ScrollViewer (template-stable)
        -> ContentPresenter#PART_ContentPresenter (template-stable)
     -> ContentPresenter#PART_ContentPresenter (template-stable)
  -> MenuSeparator (control theme, MenuSeparatorTheme.axaml)
  -> Menu (control theme, MenuTheme.axaml)
     -> PixelAlignedBorder (template-stable)
        -> ItemsPresenter#PART_ItemsPresenter (template-stable)
  -> MenuItem (item container control theme, TopLevelMenuItemTheme.axaml)
     -> Panel (template-stable)
        -> Border#Frame (template-stable)
           -> Grid (template-stable)
              -> IconPresenter#ItemIconPresenter (internal-observable)
              -> ContentPresenter#HeaderPresenter (internal-observable)
        -> Popup#PART_Popup (template-stable)
           -> Border#PopupFrame (template-stable)
              -> MenuPopupScrollHost (internal-observable)
                 -> ItemsPresenter#PART_ItemsPresenter (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `Menu` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `FlyoutHost` | control theme | `FlyoutHostTheme.axaml` | Menu | `ClipToBounds`, `Content`, `ContentTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_ContentPresenter` | template node (ContentPresenter) | `FlyoutHostTheme.axaml` | FlyoutHost | `ClipToBounds`, `Content`, `ContentTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuFlyoutPresenter` | presenter control theme | `MenuFlyoutPresenterTheme.axaml` | Menu | `ArrowPosition`, `IsArrowVisible`, `IsMotionEnabled`, `IsScrollEnabled`, `ItemsPanel`, `MaxPopupHeight` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `{x:Static atom:AbstractArrowDecoratedBox.ArrowDecoratorPart}` | template node (ArrowDecoratedBox) | `MenuFlyoutPresenterTheme.axaml` | MenuFlyoutPresenter | `ArrowPosition`, `IsArrowVisible`, `IsMotionEnabled`, `IsScrollEnabled`, `ItemsPanel`, `MaxPopupHeight` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuPopupScrollHost` | template node (MenuPopupScrollHost) | `MenuFlyoutPresenterTheme.axaml` | MenuFlyoutPresenter | `IsMotionEnabled`, `IsScrollEnabled`, `ItemsPanel`, `atom` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_ItemsPresenter` | template node (ItemsPresenter) | `MenuFlyoutPresenterTheme.axaml` | MenuFlyoutPresenter | `ItemsPanel` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `TreeViewFlyoutPresenter` | presenter control theme | `TreeViewFlyoutPresenterTheme.axaml` | Menu | `ArrowPosition`, `Background`, `BackgroundSizing`, `CornerRadius`, `IsArrowVisible`, `ItemsPanel` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `{x:Static atom:AbstractArrowDecoratedBox.ArrowDecoratorPart}` | template node (ArrowDecoratedBox) | `TreeViewFlyoutPresenterTheme.axaml` | TreeViewFlyoutPresenter | `ArrowPosition`, `Background`, `BackgroundSizing`, `CornerRadius`, `IsArrowVisible`, `ItemsPanel` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ItemsPresenter` | template node (ItemsPresenter) | `TreeViewFlyoutPresenterTheme.axaml` | TreeViewFlyoutPresenter | `ItemsPanel` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `MenuItemGroup` | control theme | `MenuItemGroupTheme.axaml` | 用户代码 / 控件宿主 | `Header`, `HeaderTemplate` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `StackPanel` | template node (StackPanel) | `MenuItemGroupTheme.axaml` | MenuItemGroup | `Header`, `HeaderTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `GroupTitlePresenter` | template node (ContentPresenter) | `MenuItemGroupTheme.axaml` | MenuItemGroup | `Header`, `HeaderTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_ItemsPresenter` | template node (ItemsPresenter) | `MenuItemGroupTheme.axaml` | MenuItemGroup | 主题状态 / visual state | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuItem` | item container control theme | `MenuItemTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `CornerRadius`, `Foreground`, `GroupName`, `Header`, `HeaderTemplate` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `Panel` | template node (Panel) | `MenuItemTheme.axaml` | MenuItem | `Background`, `CornerRadius`, `Foreground`, `GroupName`, `Header`, `HeaderTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `Frame` | template node (Border) | `MenuItemTheme.axaml` | MenuItem | `Background`, `CornerRadius`, `Foreground`, `GroupName`, `Header`, `HeaderTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ToggleItemsLayout` | template node (Panel) | `MenuItemTheme.axaml` | MenuItem | `GroupName`, `IsChecked`, `IsEnabled` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ToggleCheckbox` | template node (CheckBox) | `MenuItemTheme.axaml` | MenuItem | `IsChecked` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ToggleRadio` | template node (RadioButton) | `MenuItemTheme.axaml` | MenuItem | `GroupName`, `IsChecked` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `ItemIconPresenter` | template node (IconPresenter) | `MenuItemTheme.axaml` | MenuItem | `Foreground`, `Icon` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `ItemTextPresenter` | template node (ContentPresenter) | `MenuItemTheme.axaml` | MenuItem | `Header`, `HeaderTemplate` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `InputGestureText` | template node (TextBlock) | `MenuItemTheme.axaml` | MenuItem | `InputGesture` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuIndicatorIcon` | template node (RightOutlined) | `MenuItemTheme.axaml` | MenuItem | `Foreground` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Popup` | template node (Popup) | `MenuItemTheme.axaml` | MenuItem | `IsMotionEnabled`, `IsScrollEnabled`, `ItemsPanel`, `MaxPopupHeight`, `PopupPadding`, `ShouldUseOverlayPopup` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PopupFrame` | template node (Border) | `MenuItemTheme.axaml` | MenuItem | `IsMotionEnabled`, `IsScrollEnabled`, `ItemsPanel`, `MaxPopupHeight`, `PopupPadding`, `atom` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuPopupScrollHost` | template node (MenuPopupScrollHost) | `MenuItemTheme.axaml` | MenuItem | `IsMotionEnabled`, `IsScrollEnabled`, `ItemsPanel`, `atom` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_ItemsPresenter` | template node (ItemsPresenter) | `MenuItemTheme.axaml` | MenuItem | `ItemsPanel` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuPopupScrollHost` | control theme | `MenuPopupScrollHostTheme.axaml` | Menu | `AllowAutoHide`, `Content`, `ContentTemplate`, `HorizontalContentAlignment`, `IsMotionEnabled`, `VerticalContentAlignment` | internal-observable | 用于理解结构和状态流，不应指导用户代码直接依赖。 |
| `PART_ContentPresenter` | template node (ContentPresenter) | `MenuPopupScrollHostTheme.axaml` | MenuPopupScrollHost | `Content`, `ContentTemplate`, `HorizontalContentAlignment`, `VerticalContentAlignment` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuSeparator` | control theme | `MenuSeparatorTheme.axaml` | 用户代码 / 控件宿主 | 主题状态 / visual state | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `Menu` | control theme | `MenuTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `BackgroundSizing`, `BorderBrush`, `BorderThickness`, `CornerRadius`, `Padding` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PART_ItemsPresenter` | template node (ItemsPresenter) | `MenuTheme.axaml` | Menu | 主题状态 / visual state | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `MenuItem` | item container control theme | `TopLevelMenuItemTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `CornerRadius`, `Foreground`, `Header`, `HeaderTemplate`, `Icon` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `Panel` | template node (Panel) | `TopLevelMenuItemTheme.axaml` | MenuItem | `Background`, `CornerRadius`, `Foreground`, `Header`, `HeaderTemplate`, `Icon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `Frame` | template node (Border) | `TopLevelMenuItemTheme.axaml` | MenuItem | `Background`, `CornerRadius`, `Foreground`, `Header`, `HeaderTemplate`, `Icon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| 契约组 | 代表成员 | 维护含义 |
| --- | --- | --- |
| 内容与数据 | `Items`、`MenuItem`、`MenuItemData`、`MenuSeparatorData` | 定义菜单项集合、数据驱动菜单项和分割项入口。 |
| 选择与集合 | `DisplayPageSize`、`IsScrollEnabled` | 维护弹层显示页数上限、滚动开关、选择、展开和集合状态。 |
| 交互与状态 | `IsMotionEnabled`、`ShouldUseOverlayPopup` | 表达用户可观察状态、可用性、清除、加载或反馈语义。 |
| 视觉与布局 | `LineWidth`、`Orientation`、`OverlayHostShadow`、`PopupRootShadow`、`SizeType` | 影响尺寸、位置、颜色、形状、密度和模板视觉变量。 |
| 动效与异步 | `CloseMotion`、`MotionDuration`、`OpenMotion` | 约束动效开关、异步加载、播放速度、超时和任务边界。 |

## Pseudo Classes

| 状态反馈 | public API、内部状态和伪类如何形成用户可感知反馈。 | open/close、collection/filter、motion、visual option。 |
| 主题语义 | ControlTheme、SharedToken、控件 Token 和模板绑定如何表达视觉。 | Menu Token + ControlTheme。 |

## State Flow

Menu 的状态流按以下路径收敛：

```text
Public API / inherited command / item source / user input
  -> 控件实例状态
  -> effective state / pseudo-class / template property
  -> ControlTheme selector / presenter / renderer
  -> Gallery 可观察行为
```

状态维护规则：

- Disabled 或不可交互状态优先屏蔽 pointer、keyboard、motion 和提交类反馈。
- open/close、collection/filter、motion、visual option 状态由控件实例或明确的数据 owner 推导，不能在 template part 之间双向竞争。
- `IsScrollEnabled` 控制弹层内容是否创建 `ScrollViewer`。滚动开启时 `DisplayPageSize` 参与最大高度计算；滚动禁用时弹层直接显示全部菜单项，不使用 `DisplayPageSize` 限高。
- 模板重套用时必须把 public API 对应状态回放到新的 part、伪类和主题变量。
- 集合、弹层、异步、动效或窗口相关状态必须能处理 reset、close、cancel、detach 和 owner 释放。

子菜单的 pointer 交互采用独立的 hover intent 模型：

- `SelectedItem` 表达菜单导航和当前项状态，`IsSubMenuOpen` 表达已提交的弹层状态；二者都不能作为延迟任务是否仍然有效的唯一依据。
- pointer 进入带子菜单的非顶层项时，只为当前目标建立延迟打开意图。pointer 在延迟完成前离开该项时，打开意图立即失效，子菜单不得在离开后继续弹出。
- 已打开子菜单的关闭延迟只用于允许 pointer 从父项移动到其弹层。pointer 重新进入父项、子菜单弹层或其后代项时，待执行的关闭意图必须失效。
- 同一目标不能同时持有互相矛盾的打开和关闭意图。不同兄弟项切换时可以同时存在“关闭旧项”和“打开新项”，但每类意图最多只有一个当前目标。
- keyboard、access key 和 pointer press 触发的显式打开不经过 hover 延迟，不得被旧 hover callback 覆盖或回滚。
- 菜单关闭、窗口失活、宿主解除连接或交互处理器 detach 时，所有未完成 hover intent 必须统一失效。

## Theme and Token Boundaries

Menu 的视觉模型由控件模板、ControlTheme、SharedToken 和必要的控件 Token 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `ContextMenuTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `MenuItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `MenuSeparatorTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `MenuTheme.axaml` | 提供控件模板、selector、资源绑定和状态视觉。 |
| `TopLevelMenuItemTheme.axaml` | 定义集合项、容器项或局部单元的状态视觉。 |
| `src/AtomUI.Desktop.Controls/Flyouts/Themes/MenuFlyoutPresenterTheme.axaml` | 定义 `MenuFlyout` 菜单项 presenter 的弹层内容模板和滚动承载结构。 |

Menu 使用 `MenuToken` 作为控件 Token scope。Token 只表达组件视觉语义，不承载 open/close、collection/filter、motion、visual option 运行时状态。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 不把可由 AXAML 表达的模板状态迁移为 C# 动态创建视觉。
- 弹层滚动开关通过内部 `MenuPopupScrollHost` 复用模板分支；禁用滚动时不能保留隐藏或禁用状态的 `ScrollViewer`。
- 不把 hover、pressed、selected、expanded、loading、filter、popup open 等运行时状态写入 Token。
- Browser 或平台特化主题必须保持同一 API 的语义一致。

Token 边界：

Menu Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `MenuToken`，scope id 为 `Menu`，源码位于 `src/AtomUI.Desktop.Controls/Menu/MenuToken.cs`。

## Customization Boundaries

维护 Menu 时必须保持以下不变量：

- 不擅自新增、删除、重命名或改变 public/protected API、Avalonia 属性、事件和默认值。
- 不破坏 template part、伪类、ControlTheme key、Token 名称和资源 key。
- 不改变 Gallery 已展示的 XAML 用法、默认外观、交互顺序和状态优先级。
- `IsScrollEnabled` 默认值必须保持为 `true`；滚动禁用时视觉树中不得创建 `ScrollViewer`，也不得继续按 `DisplayPageSize` 限制弹层高度。
- Template part 重新应用、集合替换、弹层关闭、窗口失活和控件 detach 时必须释放旧订阅和资源宿主。
- 不通过隐藏延迟、强制刷新或吞异常掩盖状态同步问题。
- 不把 `SelectedItem`、`IsSubMenuOpen` 或一次 callback 内的 pointer 判断当作 hover intent 的替代状态；延迟任务必须有明确 owner、目标身份和失效边界。
- 修复 hover 行为不得新增 public/protected API，也不得改变 `DefaultMenuInteractionHandler` 已公开类型和构造函数契约。
- 不引入运行时反射扫描作为 API、Token 或数据路径发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

维护不变量：

维护 Menu 时不得破坏：

- Public API、默认值、事件顺序和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- 旧 template part、事件订阅、Popup/Flyout/Window host 和 collection view 的释放路径。
- `DefaultMenuInteractionHandler` 的公开类型、构造函数和外部注入能力。
- `IsScrollEnabled` 默认值、继承传播、本地覆盖和 `MenuFlyout` 到 presenter 中继语义。
- 滚动禁用时不创建 `ScrollViewer`，滚动开启时 `DisplayPageSize` 继续限制弹层最大高度。
- 选择状态、Popup 状态与 hover intent 的职责分离。
- `MenuItem.SyncSubMenuPopupOpenState` 的非重入性：`Popup.IsOpen` 的打开 / 关闭结果会回写 `IsSubMenuOpen`，写入
  又触发同一同步方法。弹层无法保持打开时（放置目标跑出 `TopLevel` 可视矩形）该回写链曾无限递归直至栈溢出，因此同步
  必须由 `_isSyncingSubMenuPopupState` 守卫；移除守卫会让可视区外的子菜单展开直接崩溃。
- Light/Dark、Browser/Desktop 和不同 SizeType 下的主题一致性。
- plain Menu 的语义层级只在其自身子树内下发；共享容器在 ContextMenu / MenuFlyout / DropdownButton 弹层中的 marker 行为不变。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
