# GroupBox 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

`GroupBox` 公开 `root`、`header`、`icon`、`title`、`content` 五个职责区域：

| Part | Selector | ContractType | Cardinality | Customization | AtomUI 节点 |
| --- | --- | --- | --- | --- | --- |
| `root` | 控件本身 | `GroupBox` | `Single` | `Root` | `GroupBox` owner |
| `header` | `.semantic-header` | `Border` | `Single` | `Selector` | `Border#PART_HeaderContent` |
| `icon` | `.semantic-icon` | `IconPresenter` | `Single` | `Selector` | `IconPresenter#PART_HeaderIconPresenter` |
| `title` | `.semantic-title` | `TextBlock` | `Single` | `Selector` | `TextBlock#PART_HeaderPresenter` |
| `content` | `.semantic-content` | `ContentPresenter` | `Single` | `Selector` | `ContentPresenter#PART_ContentPresenter` |

五个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since` 统一为 `6.2.0`，与其余已改造控件一致。
GroupBox 是独立控件，不存在派生控件家族、public 子 Control owner 或 item container，五个 Part 全部属于 `GroupBox` 自身。

### 1.1 准入依据（2026-09-16 用户指令）

GroupBox 的纳入与既有三个追认项（`Expander`、`TabStrip`、`Menu`）依据不同，必须单独说明：

- 上游设计体系当前稳定发布源码中**不存在**与 GroupBox 职责对应的公开 Semantic DOM owner：其组件目录下没有 fieldset、
  group 或 group-box 类组件，`GroupBox` 标识在组件源码中零命中。因此本次纳入不存在可映射的上游 owner。
- 原排除判定见[全量改造设计 §2.4](../../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md)，其正向
  触发条件是“新稳定版出现职责直接对应的公开 owner”。该条件**并未发生**；纳入由用户直接指令撤销，判据是 AtomUI 需要
  让 GroupBox 支持 Semantic Part，而不是上游新增 owner。因此沿用 §2.4 的三次撤销记录方式（日期化范围变更），而不是
  声明一次新的上游 Gate 通过。
- 支持该决定的既有先例是 `SplitButton` 的触发侧按键：上游没有对应键时，AtomUI 可以按自身模板结构**显式能力补充**发布
  Part，而不是因为“上游没有”就拒绝定制。
- 产品职责的最近参照是上游 `Card`：它公开 `root` / `header` / `title` / `body` / `extra` / `cover` / `actions` 分区键，是
  “带标题的边框容器”这一职责的分区式参照。GroupBox 的 `header` / `title` 与之一一对应，`icon` / `content` 按 GroupBox
  自身的 Header API 与 `ContentControl.Content` 命名。`Card` 的 owner 资格已由 AtomUI `Card` 持有，GroupBox **不借用**
  Card 的准入资格。

GroupBox 的 Part 名称、`ContractType` 与 cardinality 自本文件发布起构成公共主题契约。

### 1.2 `GroupBox`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `root` |
| Selector | GroupBox 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `GroupBox` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `GroupBox` owner（可见边框与背景由 owner `Render` 自绘，无模板节点承载） |
| 职责 | GroupBox root 是分组边框、背景、圆角、内容内边距与标题位置的统一 owner。 |
| 相关 API | `HeaderTitle`、`HeaderTitleColor`、`HeaderIcon`、`HeaderTitlePosition`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight`、`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius`、`Padding`、`Content`、`ContentTemplate` |
| 相关 Token | GroupBoxToken、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `header`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `header` |
| Selector | `.semantic-header` |
| SelectorRoute | `/template/ .semantic-header` |
| Style Type | `GroupBoxHeaderStyle` |
| ContractType | `Border` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Border#PART_HeaderContent` |
| 职责 | 统一表示 Header 内容区域（图标与标题的承载区域）的背景、内边距与水平对齐；该区域的 bounds 同时是边框缺口的几何来源。 |
| 相关 API | `HeaderTitle`、`HeaderIcon`、`HeaderTitlePosition`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight` |
| 相关 Token | `HeaderContentPadding`、`HeaderContainerMargin`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `icon`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `icon` |
| Selector | `.semantic-icon` |
| SelectorRoute | `/template/ .semantic-icon` |
| Style Type | `GroupBoxIconStyle` |
| ContractType | `IconPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `IconPresenter#PART_HeaderIconPresenter` |
| 职责 | 统一表示 Header 图标的尺寸、颜色与间距。 |
| 相关 API | `HeaderIcon` |
| 相关 Token | `HeaderIconMargin`、`IconSizeLG`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `GroupBoxTitleStyle` |
| ContractType | `TextBlock` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#PART_HeaderPresenter` |
| 职责 | 统一表示标题文字的布局、颜色、字体与对齐。 |
| 相关 API | `HeaderTitle`、`HeaderTitleColor`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight` |
| 相关 Token | SharedToken（`ColorText`、`FontSize`） |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `GroupBox` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `GroupBoxContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#PART_ContentPresenter` |
| 职责 | 统一表示分组内容区域的内边距、背景与内容呈现。 |
| 相关 API | `Content`、`ContentTemplate`、`Padding`、`Background` |
| 相关 Token | `ContentPadding`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。`header` 的承载节点是公开的
`Border`，`icon` 是公开的 `IconPresenter`，`content` 是公开的 `ContentPresenter`；`title` 的 `ContractType` 取
`Avalonia.Controls.TextBlock` 而不是节点自身的派生类型 `AtomUI.Desktop.Controls.TextBlock`——派生类型同样满足该契约，
取基类可以让后续把节点替换为普通 `TextBlock` 保持兼容；收窄到派生类型属于破坏性变更。

`header`、`icon`、`title`、`content` 全部是 `GroupBoxTheme.axaml` 单一 `ControlTemplate` 内的静态节点，`TemplatedParent`
为 GroupBox owner 本身，因此四个 Part 声明 `RuntimeCreated=false` 且不携带显式 `SelectorRoute`；生成器按
[Semantic Part Generator §2.3](../../../../modules/generator/semantic-part-generator.md)把静态根模板 Part 的 route 规范化为
`/template/ .<SelectorClass>`。`icon` 与 `title` 在模板中位于 `header` 节点内部（`Decorator`/`Border` → `StackPanel`），
但都不跨越第二层模板边界，`TemplatedParent` 仍是 GroupBox owner，因此单一 `/template/` 路由已经足够，不需要为它们声明
更宽的 logical descendant 路由。GroupBox 没有任何由 C# 创建并注入 marker 的 Part，也不存在跨视觉根 Part。

GroupBox 只有一个叶子主题资产（`src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml`），没有第二个
`ControlTemplate` 变体、没有 Browser 变体、没有派生模板，也不存在用于聚合的 `*Themes.axaml` 参与校验；因此不存在
marker 覆盖缺口。

## Abstract AXAML Structure

来源：`src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml`

```xml
<Border Name="PART_Frame">
    <DockPanel>
        <Panel Name="PART_HeaderContainer">
            <Border Name="PART_HeaderContent">
                <StackPanel>
                    <IconPresenter Name="PART_HeaderIconPresenter" />
                    <TextBlock Name="PART_HeaderPresenter" />
                </StackPanel>
            </Border>
        </Panel>
        <ContentPresenter Name="PART_ContentPresenter" />
    </DockPanel>
</Border>
```

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
GroupBox
  -> GroupBox (control theme, GroupBoxTheme.axaml)
     -> Border#PART_Frame (template-stable)
        -> DockPanel (template-stable)
           -> Panel#PART_HeaderContainer (template-stable)
              -> Border#PART_HeaderContent (template-stable)
                 -> StackPanel (template-stable)
                    -> IconPresenter#PART_HeaderIconPresenter (template-stable)
                    -> TextBlock#PART_HeaderPresenter (template-stable)
           -> ContentPresenter#PART_ContentPresenter (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `GroupBox` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `GroupBox` | control theme | `GroupBoxTheme.axaml` | 用户代码 / 控件宿主 | `Content`, `ContentTemplate`, `CornerRadius`, `HeaderFontSize`, `HeaderFontStyle`, `HeaderFontWeight` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PART_Frame` | template node (Border) | `GroupBoxTheme.axaml` | GroupBox | `Content`, `ContentTemplate`, `CornerRadius`, `HeaderFontSize`, `HeaderFontStyle`, `HeaderFontWeight` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `DockPanel` | template node (DockPanel) | `GroupBoxTheme.axaml` | GroupBox | `Content`, `ContentTemplate`, `HeaderFontSize`, `HeaderFontStyle`, `HeaderFontWeight`, `HeaderIcon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_HeaderContainer` | template node (Panel) | `GroupBoxTheme.axaml` | GroupBox | `HeaderFontSize`, `HeaderFontStyle`, `HeaderFontWeight`, `HeaderIcon`, `HeaderTitle`, `HeaderTitleColor` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_HeaderContent` | template node (Border) | `GroupBoxTheme.axaml` | GroupBox | `HeaderFontSize`, `HeaderFontStyle`, `HeaderFontWeight`, `HeaderIcon`, `HeaderTitle`, `HeaderTitleColor` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `StackPanel` | template node (StackPanel) | `GroupBoxTheme.axaml` | GroupBox | `HeaderFontSize`, `HeaderFontStyle`, `HeaderFontWeight`, `HeaderIcon`, `HeaderTitle`, `HeaderTitleColor` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_HeaderIconPresenter` | template node (IconPresenter) | `GroupBoxTheme.axaml` | GroupBox | `HeaderIcon` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_HeaderPresenter` | template node (TextBlock) | `GroupBoxTheme.axaml` | GroupBox | `HeaderFontSize`, `HeaderFontStyle`, `HeaderFontWeight`, `HeaderTitle`, `HeaderTitleColor` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ContentPresenter` | template node (ContentPresenter) | `GroupBoxTheme.axaml` | GroupBox | `Content`, `ContentTemplate`, `Padding` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_Frame` | `Border` | 承载整体布局根节点，并提供边框 bounds 参考。 |
| `PART_HeaderContainer` | `Panel` | Header 行容器。 |
| `PART_HeaderContent` | `Decorator` | Header 内容实际 bounds，用于标题缺口和对齐计算。 |
| `PART_HeaderIconPresenter` | `IconPresenter` | Header 图标展示。 |
| `PART_HeaderPresenter` | `TextBlock` | Header 标题展示。 |
| `PART_ContentPresenter` | `ContentPresenter` | 内容承载。 |

## Pseudo Classes

源文档未声明控件专属伪类。控件仍可能消费 Avalonia 标准状态，例如 `:pointerover`、`:pressed`、`:disabled` 和 focus 相关状态。

## State Flow

GroupBox 自身没有 hover、pressed、loading、selected、expanded 或 checked 状态。它不拦截输入事件，也不为 Header 提供默认点击行为。

GroupBox 的有效状态来自公共属性、模板测量结果和主题资源：

```text
HeaderTitle / HeaderIcon / HeaderTitlePosition
      + Header font properties
      + template measured bounds
        ↓
Header content bounds
        ↓
Frame border bounds + Header gap bounds
        ↓
Background / BorderBrush / BorderThickness / CornerRadius render state
```

`HeaderIcon`、Header 字体、标题位置和 Header 内容变化会影响缺口尺寸或位置。内容尺寸变化会通过模板根 `PART_Frame` 参与 GroupBox 的 measure pass，使自动高度随内容 `DesiredSize` 增长，同时仍尊重父容器可用空间和显式高度约束。`Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 改变会影响自绘边框和背景。

## Theme and Token Boundaries

GroupBox 的默认 Theme 位于 `src/AtomUI.Desktop.Controls/GroupBox/Themes/GroupBoxTheme.axaml`。Theme 提供 Header、Frame 和 Content 的可测量结构，并设置默认背景、边框、圆角、标题颜色、标题字号和间距。

标题缺口是 GroupBox 的视觉契约：Header 内容区域必须从上边框中形成缺口。缺口不能依赖用 `Background` 覆盖边框线，因为 `Background` 允许为透明或半透明。

Theme 职责：

- 设置 `Background`、`BorderBrush`、`BorderThickness`、`CornerRadius` 默认值。
- 设置 `HeaderTitleColor`、`HeaderFontSize`。
- 通过 GroupBox Token 设置 Header 容器外边距、Header 内容内边距、Header 图标间距和内容内边距。
- 根据 `HeaderTitlePosition` 设置 `PART_HeaderContent` 的水平对齐。

Theme 不负责动态构建 Header 或绘制边框。Header 缺口属于控件渲染模型。

Token 边界：

GroupBox Token 将全局 SharedToken 转换为 GroupBox 可消费的组件级结构值，主要覆盖 Header 和内容区域的间距。颜色、边框厚度、圆角和字体基础值直接来自 SharedToken，不在 GroupBox Token 中重复定义。

GroupBox Token 不表达实例状态，也不负责 Header 缺口的运行时 bounds。Header 缺口由模板测量结果和控件渲染模型共同决定。

## Customization Boundaries

维护 GroupBox 时必须保持以下不变量：

- `HeaderTitle`、`HeaderTitleColor`、`HeaderIcon`、`HeaderTitlePosition`、`HeaderFontSize`、`HeaderFontStyle`、`HeaderFontWeight` 的 API 名称、类型和默认语义不变。
- `GroupBoxTitlePosition.Left`、`Right`、`Center` 的名称和含义不变。
- `PART_Frame`、`PART_HeaderContainer`、`PART_HeaderContent`、`PART_HeaderIconPresenter`、`PART_HeaderPresenter`、`PART_ContentPresenter` 的 template part 名称不变。
- `PART_HeaderContent` 保持为 `Decorator` 的子类（当前为 `Border`，用于承载 Semantic Part `header` 的背景能力），缺口几何继续以它的实际 `Bounds` 为准。
- `Background="Transparent"` 时内容区保持透明，同时 Header 标题下方不应出现边框短线；该契约在 Header 设置不透明背景时同样成立，缺口始终是几何排除而非背景遮挡。
- 未设置显式高度时，GroupBox 的 `DesiredSize.Height` 必须包含 Header 通道、内容内边距和内容自身期望高度，避免内容多时被 Header 或边框区域挤压。
- Header 图标为 `null` 时图标节点不可见，不保留额外图标占位宽度。
- Header 内容位置改变只影响 Header 水平对齐，不改变内容区域布局语义。
- Token 名称和语义不擅自重命名或删除。

维护不变量：

内部重构必须保持以下不变量：

- Header 缺口计算基于 `PART_HeaderContent` 的实际 bounds。
- `PART_Frame` 保持边框绘制的布局参考。
- `PART_Frame` 必须参与 GroupBox 测量，自动高度不能退化为只测量裸 Content。
- 不用 Header 背景遮挡边框线来模拟缺口。
- 透明背景、半透明背景和普通背景走同一渲染模型。
- Header 图标隐藏时不保留额外图标占位。
- GroupBox 不新增点击、折叠或选择行为。
- Token 只表达布局和视觉默认值，不承载实例 bounds 或渲染缓存。
- 四个 Semantic marker 只作为静态 `Classes.semantic-*="True"` 存在于 `GroupBoxTheme.axaml` 单一模板内，不由代码在运行时
  增删，也不参与 `Render`、测量或失效条件。
- `PART_HeaderContent` 保持 `Decorator` 兼容类型（当前 `Border`）与 `PART_HeaderContent` 名称，缺口继续以它的 `Bounds` 为准。
- 可见分组边框与背景只由 `GroupBox.Render` 绘制；不得为 Semantic Part 在模板中新增承载 `Background` / `BorderBrush` 的
  边框层来替代自绘几何。
