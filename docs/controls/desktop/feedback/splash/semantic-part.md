# Splash Semantic Part 契约

本文档定义 `Splash` 对应用公开的 Semantic Part、选择器、类型约束、数量语义和定制边界。Splash 的整体设计见
[Splash 桌面版架构设计](overview.md)，真实模板与生命周期见 [Splash 桌面版实现原理](implementation.md)，Token 语义见
[Splash Token 设计](token.md)，系统级规则见
[AtomUI Semantic Part 系统设计](../../../../architecture/systems/theming/semantic-parts.md)。

## 1. Semantic Parts

`Splash` 公开 `root`、`logo`、`title`、`subtitle`、`content`、`spin`、`progressBar`、`message`、`detail`、`footer`
十个职责区域：

| Part | Selector | ContractType | Cardinality | Customization | AtomUI 节点 |
| --- | --- | --- | --- | --- | --- |
| `root` | 控件本身 | `Splash` | `Single` | `Root` | `Splash` owner |
| `logo` | `.semantic-logo` | `ContentPresenter` | `Single` | `Selector` | `ContentPresenter#PART_LogoPresenter` |
| `title` | `.semantic-title` | `TextBlock` | `Single` | `Selector` | `TextBlock#PART_TitleBlock` |
| `subtitle` | `.semantic-subtitle` | `TextBlock` | `Single` | `Selector` | `TextBlock#PART_SubtitleBlock` |
| `content` | `.semantic-content` | `ContentPresenter` | `Single` | `Selector` | `ContentPresenter#PART_ContentPresenter` |
| `spin` | `.semantic-spin` | `Spin` | `Single` | `Selector` | `Spin#PART_Spin` |
| `progressBar` | `.semantic-progress-bar` | `ProgressBar` | `Single` | `Selector` | `ProgressBar#PART_ProgressBar` |
| `message` | `.semantic-message` | `TextBlock` | `Single` | `Selector` | `TextBlock#PART_MessageBlock` |
| `detail` | `.semantic-detail` | `TextBlock` | `Single` | `Selector` | `TextBlock#PART_DetailBlock` |
| `footer` | `.semantic-footer` | `ContentPresenter` | `Single` | `Selector` | `ContentPresenter#PART_FooterPresenter` |

表中 `TextBlock` 均指 `Avalonia.Controls.TextBlock`，`Spin` / `ProgressBar` 指 `AtomUI.Desktop.Controls` 中的对应类型。
`Splash` 是 `AtomUI.Desktop.Controls.Extras` 中**首个**采用 Semantic Part 的控件；`root` 是本包第一个隐式 descriptor。

十个 Part 随本次 Semantic Part 改造同时公开，descriptor 的 `Since` 统一为 `6.2.0`，与其余已改造控件一致。Splash 是独立
控件，不存在派生控件家族、public 子 Control owner 或 item container，十个 Part 全部属于 `Splash` 自身。窗口宿主
`SplashWindow` 不是 Semantic owner，理由见 [§6.1](#61-splashwindow-不发布语义部件)。

### 1.1 准入依据（2026-09-16 用户指令）

Splash 的纳入与既有三个追认项（`Expander`、`TabStrip`、`Menu`）依据不同，必须单独说明：

- 上游设计体系当前稳定发布源码中**不存在**与 Splash 职责对应的公开 Semantic DOM owner：启动页是桌面应用启动反馈，
  Web 组件体系没有承载“启动中但应用尚不可交互”这一职责的公开组件。原排除判定见
  [全量改造设计 §2.4](../../../../superpowers/specs/2026-08-12-semantic-part-control-rollout-design.md)，其正向触发条件是
  “新稳定版出现职责直接对应的公开 owner”。该条件**并未发生**；纳入由用户直接指令撤销，判据是 AtomUI 需要让 Splash
  支持 Semantic Part，而不是上游新增 owner。
- 支持该决定的既有先例与 `GroupBox` 一致：`SplitButton` 的触发侧按键说明，上游没有对应键时，AtomUI 可以按自身模板
  结构**显式能力补充**发布 Part，而不是因为“上游没有”就拒绝定制。Splash 的十个 Part 全部是这类按自身模板职责设计的
  显式能力补充。
- Part 命名参照 AtomUI 已改造的同类反馈控件：`logo` / `message` / `detail` 按 Splash 自身 API（`Logo`、`Message`、
  `Detail`）命名，`title` / `subtitle` / `content` / `footer` 与 `Result`、`Empty`、`Alert`、`GroupBox` 的同职责键一致，
  `spin` / `progressBar` 按真实承载控件命名。

Splash 的 Part 名称、`ContractType` 与 cardinality 自本文件发布起构成公共主题契约。

### 1.2 `Splash`

#### `root`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `root` |
| Selector | Splash 本身 |
| SelectorRoute | 不适用 |
| Style Type | 不适用（root 不生成 Style） |
| ContractType | `Splash` |
| Cardinality | `Single` |
| Customization | `Root` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Splash` owner（可见表面由模板内的 `PART_RootLayout` / `PART_SurfaceLayout` 承载，见 §2.1） |
| 职责 | Splash root 是启动页背景、圆角、内容内边距与窗口尺寸基线的统一 owner。 |
| 相关 API | `Background`、`CornerRadius`、`Padding`、`Width`、`MinHeight`、`IsMotionEnabled`、`Status`、`Progress`、`IsIndeterminate` |
| 相关 Token | SplashToken（`SurfaceBackground`、`SurfaceCornerRadius`、`ContentPadding`、`WindowWidth`、`WindowMinHeight`）、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `logo`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `logo` |
| Selector | `.semantic-logo` |
| SelectorRoute | `/template/ .semantic-logo` |
| Style Type | `SplashLogoStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#PART_LogoPresenter` |
| 职责 | 统一表示品牌标识区域（`Logo` / `LogoTemplate`）的尺寸、对齐与外边距。 |
| 相关 API | `Logo`、`LogoTemplate` |
| 相关 Token | `LogoSize`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `title`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `title` |
| Selector | `.semantic-title` |
| SelectorRoute | `/template/ .semantic-title` |
| Style Type | `SplashTitleStyle` |
| ContractType | `TextBlock`（`Avalonia.Controls.TextBlock`） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#PART_TitleBlock` |
| 职责 | 统一表示主标题文字的颜色、字号、字重、行高与对齐。 |
| 相关 API | `Title` |
| 相关 Token | SharedToken（`ColorTextHeading`）、`TitleFontSize`、`TitleLineHeight` |
| 稳定性 | stable since 6.2.0 |

#### `subtitle`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `subtitle` |
| Selector | `.semantic-subtitle` |
| SelectorRoute | `/template/ .semantic-subtitle` |
| Style Type | `SplashSubtitleStyle` |
| ContractType | `TextBlock`（`Avalonia.Controls.TextBlock`） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#PART_SubtitleBlock` |
| 职责 | 统一表示副标题文字的颜色、字号与对齐。 |
| 相关 API | `Subtitle` |
| 相关 Token | `SubtleForeground`、`SubtitleFontSize` |
| 稳定性 | stable since 6.2.0 |

#### `content`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `content` |
| Selector | `.semantic-content` |
| SelectorRoute | `/template/ .semantic-content` |
| Style Type | `SplashContentStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#PART_ContentPresenter` |
| 职责 | 统一表示扩展内容区域（`Content` / `ContentTemplate`）的内边距、背景、对齐与尺寸约束。 |
| 相关 API | `Content`、`ContentTemplate` |
| 相关 Token | `ContentGap`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `spin`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `spin` |
| Selector | `.semantic-spin` |
| SelectorRoute | `/template/ .semantic-spin` |
| Style Type | `SplashSpinStyle` |
| ContractType | `Spin` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `Spin#PART_Spin` |
| 职责 | 统一表示不确定加载指示器的尺寸、颜色与对齐。 |
| 相关 API | `IsIndeterminate`（经 `IsSpinVisible` 投影） |
| 相关 Token | `IndicatorSize`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `progressBar`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `progressBar` |
| Selector | `.semantic-progress-bar` |
| SelectorRoute | `/template/ .semantic-progress-bar` |
| Style Type | `SplashProgressBarStyle` |
| ContractType | `ProgressBar`（`AtomUI.Desktop.Controls.ProgressBar`） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ProgressBar#PART_ProgressBar` |
| 职责 | 统一表示确定进度条的高度、颜色与宽度。 |
| 相关 API | `Progress`、`IsIndeterminate`（经 `IsProgressBarVisible` 投影） |
| 相关 Token | `ProgressBarHeight`、SharedToken |
| 稳定性 | stable since 6.2.0 |

#### `message`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `message` |
| Selector | `.semantic-message` |
| SelectorRoute | `/template/ .semantic-message` |
| Style Type | `SplashMessageStyle` |
| ContractType | `TextBlock`（`Avalonia.Controls.TextBlock`） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#PART_MessageBlock` |
| 职责 | 统一表示状态消息文字的颜色、字号与对齐；`:success` / `:error` 状态色经该节点呈现。 |
| 相关 API | `Message`、`Detail`、`Status`、`SetMessage`、`SetStatus`、`SetError` |
| 相关 Token | SharedToken（`ColorText`）、`MessageFontSize`、`SuccessColor`、`ErrorColor` |
| 稳定性 | stable since 6.2.0 |

#### `detail`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `detail` |
| Selector | `.semantic-detail` |
| SelectorRoute | `/template/ .semantic-detail` |
| Style Type | `SplashDetailStyle` |
| ContractType | `TextBlock`（`Avalonia.Controls.TextBlock`） |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `TextBlock#PART_DetailBlock` |
| 职责 | 统一表示详细信息 / 错误详情文字的颜色、字号、换行与对齐。 |
| 相关 API | `Detail`、`SetMessage`、`SetStatus`、`SetError` |
| 相关 Token | `SubtleForeground`、`DetailFontSize` |
| 稳定性 | stable since 6.2.0 |

#### `footer`

| 字段 | 值 |
| --- | --- |
| Owner | `Splash` |
| Part | `footer` |
| Selector | `.semantic-footer` |
| SelectorRoute | `/template/ .semantic-footer` |
| Style Type | `SplashFooterStyle` |
| ContractType | `ContentPresenter` |
| Cardinality | `Single` |
| Customization | `Selector` |
| CrossVisualRoot | `false` |
| RuntimeCreated | `false` |
| AtomUI 节点 | `ContentPresenter#PART_FooterPresenter` |
| 职责 | 统一表示底部区域（`Footer` / `FooterTemplate`，版本信息、版权信息或启动失败重试入口）的内边距、背景与对齐。 |
| 相关 API | `Footer`、`FooterTemplate` |
| 相关 Token | `FooterMarginTop`、SharedToken |
| 稳定性 | stable since 6.2.0 |

`root` 是隐式 Part，不添加 `.semantic-root`。`ContractType` 只定义 Setter 可以稳定依赖的最低 public 类型，并通过
`x:SetterTargetType` 提供 AXAML 编译期类型上下文；它不参与 `.semantic-*` 的身份匹配。

四个文本位的 `ContractType` 取 `Avalonia.Controls.TextBlock` 基类，而不是模板节点的派生类型
`AtomUI.Desktop.Controls.TextBlock`。派生类型同样满足该契约，取基类可以让后续把节点替换为普通 `TextBlock` 保持兼容；
收窄到派生类型属于破坏性变更。同理，`spin` / `progressBar` 的 `ContractType` 取各自公开控件类型：即使后续以
`CustomIndicator` 等方式提供更多节点，契约仍成立。

九个非 root Part 全部是 `SplashTheme.axaml` 单一 `ControlTemplate` 内的静态节点，`TemplatedParent` 为 `Splash` owner
本身，因此全部声明 `RuntimeCreated=false` 且不携带显式 `SelectorRoute`；生成器按
[Semantic Part Generator §2.3](../../../../modules/generator/semantic-part-generator.md)把静态根模板 Part 的 route
规范化为 `/template/ .<SelectorClass>`。`message` 与 `detail` 位于 progress 区之后、`footer` 之前，但都与其余节点处于
同一 `StackPanel#PART_ContentLayout` 内，不跨越第二层模板边界；`spin` / `progressBar` 虽然自身是 `TemplatedControl`，
marker 仍标注在 Splash 模板给出的这两个节点上（未进入它们各自的模板），因此单一 `/template/` 路由已经足够，不需要
`.semantic-scope-*` 中间锚点，也不需要 `CrossNestedOwners`。Splash 没有任何由 C# 创建并注入 marker 的 Part，也不存在
跨视觉根 Part。

Splash 只有一个叶子主题资产（`src/AtomUI.Desktop.Controls.Extras/Splash/Themes/SplashTheme.axaml`），没有第二个
`ControlTemplate` 变体、没有 Browser 变体、没有派生模板，也不存在参与校验的 `*Themes.axaml`；因此不存在 marker 覆盖
缺口。`SplashWindowTheme.axaml` 是另一个 owner（`SplashWindow`）的主题，不参与 `Splash` 的 Part 校验。

## 2. Part 说明

### 2.1 root

`root` 是 `Splash` owner 本身，在实例整个生命周期内始终存在，每个实例恰好一个。

它负责：

- 承载 `Background`、`CornerRadius`、`Padding` 等启动页表面状态，这三个属性由主题经 `TemplateBinding` 投影到
  `PART_SurfaceLayout`。
- 承载 `Width` 与 `MinHeight` 尺寸基线（默认来自 `WindowWidth` / `WindowMinHeight` Token，`SplashService` 会用
  `SplashOptions.Width` / `MinHeight` 覆盖）。
- 承载 `Status`、`Progress`、`IsIndeterminate` 的运行时状态归一与伪类同步入口。
- 作为全部 Part 的 owner-scoped Selector 作用域边界。

**root 是启动页表面的唯一定制入口。** 可见背景绘制在模板节点 `PART_SurfaceLayout` 上，但它的 `Background`、
`CornerRadius`、`Padding` 都是 `TemplateBinding` 投影自 owner 的三个同名属性，因此改变表面外观应设置 owner 属性
（或使用 owner 作用域的普通 Setter），而不是给模板节点加 Semantic Setter。`PART_RootLayout` 只提供裁剪边界与圆角，
不承载背景 —— 这一分工是 Splash 的既有渲染契约，见 [§6 定制边界](#6-定制边界)。

适合通过 root 定制背景、圆角、内容内边距与窗口尺寸基线；需要按状态分支改变 root 时，在 owner Selector 上组合公开属性
或伪类（例如 `atom|Splash:error`、`atom|Splash[Status=Success]`）。

root 不表示模板中的 `PART_RootLayout`、`PART_SurfaceLayout` 或 `PART_ContentLayout`；这些节点的名称、数量和层级不属于
root 契约。

### 2.2 logo

`logo` 表示品牌标识区域，即 `ContentPresenter#PART_LogoPresenter`，cardinality 为 `Single`。`Logo` 为 `null` 时该节点
仍然存在（内容为空），节点与 marker 不缺席，Part 身份与数量不变。

它负责：

- 承载 `Logo` / `LogoTemplate` 传入内容的呈现。
- 承载标识尺寸（`LogoSize`）与居中对齐；主题把 `LogoSize` 同时映射到该节点的 `Width` 与 `Height`，因此标识呈现为正方形
  区域。

适合定制 `Width` / `Height`、`Margin`、`Opacity`、`HorizontalAlignment` 与 `VerticalAlignment`。这三类在 owner API 上
**没有**对应属性 —— `Logo` 只接受内容实例本身，标识尺寸此前只能通过替换整个 `ControlTheme` 或改 Token 调整。

`logo` 的 Semantic Setter 只作用于标识承载节点本身；`LogoTemplate` 生成的子树由用户拥有，其内部控件以自己的 owner
作用域独立匹配，不属于 `logo` 契约。

**尺寸协调边界（实测）。** `logo` 的 `Width` / `Height` 作用在承载节点（presenter）上，**不是**作用在 `LogoTemplate`
生成的徽标图形上。因此若模板内容自带固定尺寸且大于 presenter，它会撑破 presenter 并向四周溢出，Part Setter 对可见外观
失效。实测反例：presenter 被设为 36×36，而模板内容是固定 48×48 的 `Border`，则该 `Border` 的 Bounds 为 `-6,-6,48,48`
—— 每边溢出 6 px，用户看到的是 48 的徽标，而不是设定的 36。

正确写法是让徽标内容跟随 presenter 尺寸（例如 `HorizontalAlignment="Stretch"` +
`VerticalAlignment="Stretch"`，或让模板内容不设置自身宽高），此时可见尺寸与 Part Setter 一致：

```xml
<atom:Splash.Logo>
    <Border HorizontalAlignment="Stretch"
            VerticalAlignment="Stretch"
            CornerRadius="12"
            Background="{atom:SharedTokenResource ColorPrimary}">
        <atom:TextBlock Text="A" Foreground="White" HorizontalAlignment="Center" VerticalAlignment="Center" />
    </Border>
</atom:Splash.Logo>
```

这与系统设计 §7.1 的布局协调关系同类：Part 的 Setter 已经生效，但最终可见尺寸由父子两级共同决定，排查时应先证明
Part Setter 命中，再核对模板内容是否携带与 presenter 竞争的固定尺寸。

### 2.3 title

`title` 表示主标题文字区域，即 `TextBlock#PART_TitleBlock`，cardinality 为 `Single`。

它负责：

- 承载 `Title` 文本的呈现与布局。
- 默认前景、字号与行高分别来自 SharedToken `ColorTextHeading` 与 `TitleFontSize` / `TitleLineHeight`；字重固定为
  `SemiBold`。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`FontStyle`、`LineHeight`、`TextDecorations`、`Margin`、`Opacity` 与
`LetterSpacing`。owner API 只有 `Title` 字符串，因此标题排版此前只能通过 Token 或整体替换主题调整。

### 2.4 subtitle

`subtitle` 表示副标题文字区域，即 `TextBlock#PART_SubtitleBlock`，cardinality 为 `Single`。

它负责：

- 承载 `Subtitle` 文本的呈现与布局。
- 默认前景与字号来自 Splash Own Token `SubtleForeground` 与 `SubtitleFontSize`。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`FontStyle`、`Margin`、`Opacity` 与对齐。owner API 只有 `Subtitle`
字符串。

`subtitle` 与 `detail` 默认共享 `SubtleForeground` Token，但它们是两个独立 Part：开发者可以只改其中之一的颜色
（例如把错误详情调成更醒目），而不影响另一处弱文本。

### 2.5 content

`content` 表示扩展内容区域，即 `ContentPresenter#PART_ContentPresenter`，cardinality 为 `Single`。该节点始终存在，
`Content` 为空时只是没有内容。

它负责：

- 承载 `Content` / `ContentTemplate` 生成的内容。
- 承载内容区域的水平居中（`HorizontalAlignment="Center"`）。

适合定制 `Padding`（`ContentPresenter` 自身的内边距）、`Background`、`HorizontalContentAlignment` /
`VerticalContentAlignment`、`MinHeight` / `MaxHeight`、`Margin` 与 `Opacity`。

内容区域背景与 root 背景的关系：owner `Background` 绘制在 `PART_SurfaceLayout` 的整个表面矩形上，`content` 的
`Background` 绘制在内容承载节点自己的矩形内，两者可以叠加。需要整块启动页底色时使用 root `Background`；需要内容区域
独立底色时使用 `content` 的 `Background`。

`content` 不包含表面圆角与裁剪；启动页圆角与裁剪属于 root 与 `PART_RootLayout`。

### 2.6 spin 与 progressBar

`spin`（`Spin#PART_Spin`）与 `progressBar`（`ProgressBar#PART_ProgressBar`）共同构成进度区，二者都是 cardinality
`Single` 的模板常驻节点；可见性由 owner 的状态归一互斥切换，节点本身不从模板缺席，marker 数量恒定：

- `spin`：`IsIndeterminate=true` 时可见（`IsSpinVisible`），承载不确定加载指示。适合定制 `Width` / `Height`、
  `Foreground` / `Color`、`Margin` 与对齐；默认尺寸来自 `IndicatorSize`。
- `progressBar`：`Progress` 有值且 `IsIndeterminate=false` 时可见（`IsProgressBarVisible`），承载确定进度。适合定制
  `Height`、`Width`、`BarColor` / `Foreground` 与对齐；默认高度来自 `ProgressBarHeight`。

**为什么拆成两个 Part 而不是合成一个。** 二者是不同的控件类型、不同的 Token 来源（`IndicatorSize` 与
`ProgressBarHeight`）、不同的可见状态，且开发者通常需要分别调整（例如把进度条调细、同时单独更换指示器颜色）。系统设计
§3.6 允许“多个替代实现共享一个 Part”，但那条路径要求 `Cardinality=Multiple` 并把 `ContractType` 放宽到共同基类
（这里将退化为 `TemplatedControl`），从而丢失 `x:SetterTargetType` 的类型上下文，并迫使两种不同视觉共用同一份 Setter。
Splash 选择按真实承载控件拆分。

进度区的对外语义由 `Progress` 与 `IsIndeterminate` 拥有，不由 Part 拥有：分拆只影响定制入口，不改变“`IsIndeterminate`
优先于 `Progress` 的视觉展示”这一既有状态规则。内容 `Margin`（`ProgressMarginTop`）由父级 `Panel#PART_ProgressLayout`
承载，不是 Part。

### 2.7 message

`message` 表示状态消息文字区域，即 `TextBlock#PART_MessageBlock`，cardinality 为 `Single`。

它负责：

- 承载 `Message` 文本的呈现与布局。
- 普通态前景来自 SharedToken `ColorText`，字号来自 `MessageFontSize`。
- 承载状态色：`:success` 与 `:error` 状态 selector 分别把前景覆盖为 `SuccessColor` 与 `ErrorColor`。

适合定制普通态的 `Foreground`、`FontSize`、`FontWeight`、`TextDecorations`、`Margin` 与 `Opacity`。

需要明确的优先级关系：

- 状态色 selector（`^:success` / `^:error`）比普通态 selector 更具体，因此在 `Success` / `Error` 状态下，主题的状态色
  优先于主题的普通消息前景。
- 而生成的 `SplashMessageStyle` 通过 class selector 生效，Setter 按 Avalonia 原生 `BindingPriority.StyleTrigger` 应用，
  优先级高于主题普通 Style Setter。因此 **Part Setter 会盖过状态色**（两者都是状态 selector 的一个 Setter，由声明顺序与
  宿主顺序决定）。若希望保留状态语义，应把状态条件写进 owner 一侧的 selector，例如只对 `:loading` 定制：

  ```xml
  <Style Selector="atom|Splash.my-splash:loading">
      <atom:SplashMessageStyle x:SetterTargetType="TextBlock">
          <Setter Property="Foreground" Value="#F5F8FF" />
      </atom:SplashMessageStyle>
  </Style>
  ```

  这是 Gallery 专用启动页采用的写法（见 [§4](#4-状态与数量语义)），也是推荐做法：把状态留在 owner 选择器上，而不是在
  Part Setter 里复制状态色。

`message` 不表示异常对象或业务状态；`Status` 是 owner API，改变状态应设置该属性，而不是用 Setter 覆盖 `Foreground` 来绕过
状态语义。

### 2.8 detail

`detail` 表示详细信息 / 错误详情文字区域，即 `TextBlock#PART_DetailBlock`，cardinality 为 `Single`。

它负责：

- 承载 `Detail` 文本的呈现，模板固定 `TextWrapping="Wrap"`。
- 默认前景与字号来自 Splash Own Token `SubtleForeground` 与 `DetailFontSize`。

适合定制 `Foreground`、`FontSize`、`FontWeight`、`TextWrapping`、`MaxWidth`、`Margin` 与 `Opacity`。错误详情通常较长，
限制宽度或调整对齐时使用该 Part。

### 2.9 footer

`footer` 表示底部区域，即 `ContentPresenter#PART_FooterPresenter`，cardinality 为 `Single`。`Footer` 为 `null` 时节点仍然
存在（内容为空）。

它负责：

- 承载 `Footer` / `FooterTemplate` 传入内容的呈现。
- 承载底部区域的居中与上方向外边距（`FooterMarginTop`）。

适合定制 `Padding`、`Background`、`Margin`、`MinHeight`、对齐与 `Opacity`。`Footer` 常用于版本/版权信息，或在
`Status=Error` 时承载重试、退出、查看日志等操作入口 —— 这些按钮的命令属于业务层，`footer` 只提供视觉定制。

## 3. Selector 用法

应用级样式先限定 Splash owner，再通过生成的 Semantic Style 进入 Part。生成类型已经封装 owner 类型保护与
`SelectorRoute`，用户不需要复制模板路径：

```xml
<Application.Styles>
    <Style Selector="atom|Splash">
        <atom:SplashLogoStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Width" Value="48" />
            <Setter Property="Height" Value="48" />
        </atom:SplashLogoStyle>

        <atom:SplashTitleStyle x:SetterTargetType="TextBlock">
            <Setter Property="Foreground" Value="#141414" />
            <Setter Property="FontSize" Value="20" />
        </atom:SplashTitleStyle>

        <atom:SplashSubtitleStyle x:SetterTargetType="TextBlock">
            <Setter Property="Foreground" Value="#8C8C8C" />
        </atom:SplashSubtitleStyle>

        <atom:SplashProgressBarStyle x:SetterTargetType="atom:ProgressBar">
            <Setter Property="Height" Value="6" />
        </atom:SplashProgressBarStyle>

        <atom:SplashMessageStyle x:SetterTargetType="TextBlock">
            <Setter Property="FontWeight" Value="Medium" />
        </atom:SplashMessageStyle>

        <atom:SplashDetailStyle x:SetterTargetType="TextBlock">
            <Setter Property="Foreground" Value="#595959" />
        </atom:SplashDetailStyle>

        <atom:SplashFooterStyle x:SetterTargetType="ContentPresenter">
            <Setter Property="Padding" Value="0,8,0,0" />
        </atom:SplashFooterStyle>
    </Style>
</Application.Styles>
```

对特定 Splash class、状态或属性定制时，把 class、伪类或属性选择器放在 owner 一侧：

```xml
<Style Selector="atom|Splash.branded">
    <atom:SplashTitleStyle x:SetterTargetType="TextBlock">
        <Setter Property="Foreground" Value="White" />
    </atom:SplashTitleStyle>
</Style>

<Style Selector="atom|Splash.branded:loading">
    <atom:SplashMessageStyle x:SetterTargetType="TextBlock">
        <Setter Property="Foreground" Value="#F5F8FF" />
    </atom:SplashMessageStyle>
</Style>
```

`spin` 与 `progressBar` 是两个独立入口，各自定制：

```xml
<Style Selector="atom|Splash:indeterminate">
    <atom:SplashSpinStyle x:SetterTargetType="atom:Spin">
        <Setter Property="Width" Value="32" />
        <Setter Property="Height" Value="32" />
    </atom:SplashSpinStyle>
</Style>

<Style Selector="atom|Splash:determinate">
    <atom:SplashProgressBarStyle x:SetterTargetType="atom:ProgressBar">
        <Setter Property="Height" Value="8" />
    </atom:SplashProgressBarStyle>
</Style>
```

九个 Part 都是 owner 自身模板内的静态 Part，生成 Style 的路由是单一的 `/template/` 边界，不需要 `.semantic-scope-*` 中间
锚点，也不存在跨视觉根或容器回收路径。生成 Style 已封装完整路由，用户样式不得复制这些 route，也不得依赖 `PART_*` 名称或
内部节点层级。

不得把 `ContractType` 写入 Part Selector。以下写法不属于公共契约：

- `TextBlock.semantic-title` 或 `:is(TextBlock).semantic-title`。
- `ContentPresenter.semantic-logo`、`Spin.semantic-spin` 或 `ProgressBar.semantic-progress-bar`。
- 直接复制 `/template/ .semantic-*` route 作为用户主路径；route 只属于 descriptor 与生成 Style 的实现元数据。
- 依赖 `PART_*`、internal 类型、Name 或视觉祖先顺序，例如用 `PART_SurfaceLayout` 或 `PART_ContentLayout` 层级收窄选择器。

root 的定制不通过 `.semantic-root`，而是在 owner 一侧直接写属性或属性 Selector。**启动页表面只能走这条路径** ——
`Background` / `CornerRadius` / `Padding` 由 owner 属性经 `TemplateBinding` 投影到 `PART_SurfaceLayout`：

```xml
<Style Selector="atom|Splash.compact">
    <Setter Property="Width" Value="360" />
    <Setter Property="MinHeight" Value="240" />
    <Setter Property="Padding" Value="20" />
    <Setter Property="CornerRadius" Value="16" />
    <Setter Property="Background" Value="#F9F0FF" />
</Style>
```

该路径依赖 Avalonia 原生优先级：owner 作用域普通 Setter 以 `BindingPriority.Style` 应用，与 ControlTheme 的默认
`<Setter Property="Background" ...>` 同级，由样式宿主顺序与声明顺序决定；用户 `Styles` 晚于 ControlTheme 生效，因此可以
正常覆盖默认值。

## 4. 状态与数量语义

Splash 的伪类为 `:loading`、`:success`、`:error`（由 `Status` 驱动）与 `:indeterminate`、`:determinate`（由
`IsIndeterminate` 与 `Progress` 归一驱动）。它们只改变选择器匹配与可见性，不增删 marker：

| 状态 | root | logo | title | subtitle | content | spin | progressBar | message | detail | footer | 说明 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 默认（Loading + Indeterminate） | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 模板常驻节点，九个 marker 各一。 |
| 确定进度 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | `spin` 转 `IsVisible=false`、`progressBar` 转可见，节点与 marker 不变。 |
| `Status=Success` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 只改伪类与 `:success` 前景。 |
| `Status=Error` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 只改伪类与 `:error` 前景。 |
| `Logo=null` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | `logo` 节点仍存在，内容为空。 |
| `Footer=null` | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | `footer` 节点仍存在，内容为空。 |
| 空内容 / 无 Logo / 无 Footer | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 不增删节点。 |
| 模板重应用 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 静态 marker 随模板重建，数量不变。 |

Splash 没有 `Optional` 或 `Multiple` Part，也没有由 C# 创建 marker 的 Part：不存在条件分支模板、容器 prepare/clear/recycle
路径、`RuntimeCreated=true` 注入点，也不存在跨视觉根 Part。所有 Part 在任何状态组合下恰好命中一个节点。

**Semantic Preview 的两个实例约定。** `spin` 与 `progressBar` 互斥可见，而 Semantic Preview 会跳过不可见与尺寸为零的
目标。因此 Gallery Semantic Parts 预览在舞台上放置两个 `Splash` 实例，分别固定确定态（`IsIndeterminate=False` +
`Progress`）与不确定态（`IsIndeterminate=True`），使两个部件都能被定位并高亮。该约定只属于预览层，不改变 descriptor 的
`Single` 定义，也不要求控件提供任何附加 marker。

## 5. 尺寸基线

Splash 的尺寸链与 Semantic Setter 的关系：

| 项目 | 内容 |
| --- | --- |
| 完整尺寸分支 | Splash 没有 `SizeType`、`ISizeTypeAware` 或 `ICustomizableSizeTypeAware`；不存在 `Large` / `Middle` / `Small` / `Custom` 分支。尺寸由 owner 属性、Token 与自然测量确定。 |
| 尺寸属性归属 | `Width` 与 `MinHeight` 由 owner 拥有，主题默认取 `WindowWidth`（420）/ `WindowMinHeight`（280）；`SplashService.ConfigureWindow` 用 `SplashOptions.Width` / `MinHeight` 覆盖这两个值。`Padding` 由 owner 拥有，默认 `ContentPadding`。 |
| 内部尺寸 | `logo` 的 `Width` / `Height` 取 `LogoSize`；`progressBar` 的 `Height` 取 `ProgressBarHeight`；`spin` 的尺寸来自 `Spin` 自身主题与 `IndicatorSize`；文本尺寸来自各自字号 Token。元素间距由 `StackPanel#PART_ContentLayout` 的 `Spacing`（`ContentGap`）决定。 |
| Token 映射 | 每个预设值映射到单个属性而非完整布局规格：`WindowWidth` / `WindowMinHeight` → owner 尺寸；`ContentPadding` → owner `Padding`；`LogoSize` → `logo` 的宽高；`ProgressBarHeight` → `progressBar` 高度；`ContentGap` / `ProgressMarginTop` / `FooterMarginTop` → 布局包装的间距与外边距。 |
| 自然测量 | 高度方向没有固定 `Height`：owner 只用 `MinHeight` 建立基线，`SizeToContent=WidthAndHeight` 的 `SplashWindow` 让窗口随内容增长。宽度方向 `Width` 是公共契约的一部分（`SplashOptions.Width` 直接暴露），属于不可扩展的固定宽度。 |
| 外部映射 | 上游不存在与 Splash 对应的公开 owner，因此不存在上游尺寸档名称到 AtomUI 分支的映射。Splash 作为 AtomUI 原生控件，尺寸由上述属性与 Token 定义。 |
| 失败回归 | 给 `content` 设置固定 `Height` 会绕过自然测量，使 `MinHeight` 基线之外的高度增长失效；给 `logo` 设置超出内容区宽度的固定 `Width` 会挤压同列文本的换行宽度；`LogoTemplate` 内容自带大于 presenter 的固定尺寸时，可见徽标会溢出 presenter，`logo` 的尺寸 Setter 失去可见效果（见 §2.2）。 |

语义 Setter 与尺寸链的优先级关系：`logo` 的宽高、`progressBar` 的 `Height`、`content` 的 `Padding` 等 Setter 通过 class
selector 以 `BindingPriority.StyleTrigger` 应用，高于主题 Style Setter，因此可以有意覆盖 `LogoSize` /
`ProgressBarHeight` / `ContentPadding` 档基线并参与自然测量。固定 `Height` / `MinHeight` 类布局 Setter 会与内容驱动的
高度增长竞争，不作为推荐定制路径；改变留白应通过 `content` 的 `Padding`、owner `Padding` 或 Token 实现。

## 6. 定制边界

### 6.1 SplashWindow 不发布语义部件

`SplashWindow` 不是 Semantic owner，不声明任何 marker，`IThemeManager.SemanticParts` 中不存在它的 descriptor。理由：

- 窗口壳层（`Topmost`、`ShowInTaskbar`、`CanResize`、`SizeToContent`、`WindowStartupLocation`、透明与无装饰）本身已是
  public API，也是 `SplashWindowTheme.axaml` 的默认 Setter，不需要 Part 才能定制。
- 窗口可见表面只有阴影（`SurfaceBoxShadow`）与宿主圆角（`SurfaceCornerRadius`），二者是 Token 语义；既有的窗口级覆盖
  入口是 `SplashWindow.Resources` 或专用 `SplashWindow` 子类，见 [Splash Token 设计](token.md)。
- `SurfaceCornerRadius` 同时被窗口阴影遮罩（`PART_SurfaceHost`）与 Splash 内容表面（`PART_SurfaceLayout` 经
  `CornerRadius` TemplateBinding）消费。再开一个窗口侧 Part 会产生两个互不同步的圆角入口。
- 承载节点 `ShadowsAwareContainer` 是 internal 类型，公开 `ContractType` 只能退化为 `Decorator`，契约价值低。

窗口模板结构（`SplashWindow` → `ShadowsAwareContainer#PART_SurfaceHost` → `Splash`）不属于 Splash 公共语义契约；
`PART_SurfaceHost` 的 `ClipToBounds=False` 保证阴影不被裁剪，这一不变量由窗口主题拥有。

### 6.2 明确排除的节点与区域

以下区域明确不属于 Splash Semantic Part：

- `PART_RootLayout`（`Border`）：模板裁剪根，只承载 `CornerRadius` 投影与 `ClipToBounds=True`。**它不设置
  `Background`、`Padding` 或 `BoxShadow`**，因此不是表面定制入口；设置它的 `Background` 会在圆角内再叠一层矩形，不改变
  启动页表面观感。
- `PART_SurfaceLayout`（`Border`）：表面承载节点，`Background` / `CornerRadius` / `Padding` 全部是 `TemplateBinding`
  投影自 owner 属性。它是 root 的实现节点，不是独立 Part —— 否则会出现两条互相竞争的定制路径。
- `PART_ContentLayout`（`StackPanel`）：内容列的纯布局脊柱，唯一职责是施加 `ContentGap` 间距。它不承载视觉语义，因此不
  发布为 Part；调整元素间距应通过 Token 或主题实现，而不是给该节点加 Semantic 标记。参照 `GroupBox` 排除
  `PART_HeaderContainer` 的先例。
- `PART_ProgressLayout`（`Panel`）：进度区布局包装，唯一职责是施加 `ProgressMarginTop` 上外边距，同理不发布。
- `PART_*` 名称、internal 类型与视觉祖先顺序。
- 用户内容子树：`LogoTemplate`、`ContentTemplate`、`FooterTemplate` 生成的子树由用户拥有；内层控件以自己的 owner
  作用域独立匹配，不继承 Splash 的 Part。
- 业务语义：`Status`、`Progress`、`IsIndeterminate`、启动步骤、异常对象、`SplashService` 编排结果都不由 Part 承载。
- 窗口与独立 TopLevel：Splash 不创建弹层或独立宿主，`CrossVisualRoot=false`；`SplashWindow` 见 §6.1。

### 6.3 与 Token / Theme 的职责边界

| 能力 | Owner |
| --- | --- |
| 启动状态、进度归一与伪类同步 | `Splash` 属性与 `UpdateVisualState` |
| 启动页默认设计值 | SplashToken 与 SharedToken |
| 稳定视觉区域的局部覆盖 | 生成的 Semantic Style（本文件 §1、§3） |
| 整个 Splash 的结构替换 | owner `ControlTheme` |
| 窗口壳层与窗口级表面覆盖 | `SplashWindowTheme.axaml` 与 `SplashWindow.Resources` |

Semantic Style 不拥有行为：`Status` / `Progress` / `IsIndeterminate` 的状态流不依赖用户是否为某个 Part 设置了 Setter。
Semantic Setter 命中只证明目标属性已生效；如果最终布局仍被 owner 测量、`TemplateBinding` 或窗口 `SizeToContent` 约束，
应按跨节点布局约束排查，不能把它解释为 Semantic Style 优先级失效。

## 7. 兼容性与验证

删除或重命名 Part、修改 selector class、收窄 `ContractType`、改变 cardinality、把 `Single` 改为 `Optional` / `Multiple`、
或者让内置模板缺少 marker，均属于公共主题契约变更。把四个文本位的 `ContractType` 从 `Avalonia.Controls.TextBlock` 收窄到
`AtomUI.Desktop.Controls.TextBlock` 属于破坏性变更。把 `spin` 与 `progressBar` 合并为单个 `Multiple` Part 同时属于破坏性
变更（删除两个 Part、放宽一个 `ContractType`）。

验证至少覆盖：

- descriptor 中恰好是 `root`、`content`、`detail`、`footer`、`logo`、`message`、`progressBar`、`spin`、`subtitle`、
  `title` 十个 Part，字段值与 §1 表格一致：九个非 root Part 均为 `Customization=Selector`、`CrossVisualRoot=false`、
  `RuntimeCreated=false`、`CrossNestedOwners=false`、`Cardinality=Single`，不携带显式 `SelectorRoute`（由生成器规范化为
  `/template/ .<class>`），`ContractType` 分别为 `ContentPresenter` / `TextBlock` ×4 / `ContentPresenter` / `Spin` /
  `ProgressBar` / `ContentPresenter`；`root` 的 `ContractType` 为 `Splash` 且 `StyleType` 为 `null`。
- `SplashTheme.axaml` 的九个静态 marker 各一个，且全部使用 `Classes.semantic-*="True"`（不接受字面量 `Classes="semantic-*"`）；
  模板只有一个 `ControlTemplate`，因此不存在 marker 覆盖缺口；模板中不出现 `.semantic-root` 或任何 `.semantic-scope-*`。
- `SplashWindowTheme.axaml` 不声明任何 `Classes.semantic-*`，且 registry 中不存在 `SplashWindow` 的 descriptor。
- 九个非 root Part 在任意状态组合下恰好命中一个 owner-scoped 节点：默认、确定进度、`Status=Success` / `Error`、
  `Logo=null`、`Footer=null`、模板重应用都不改变命中数量。
- `spin` / `progressBar` 的互斥可见性正确：默认 `spin` 可见且 `progressBar` 不可见；确定进度时相反；两者都保留 marker。
- 生成的 `SplashLogoStyle` / `SplashTitleStyle` / `SplashSubtitleStyle` / `SplashContentStyle` / `SplashSpinStyle` /
  `SplashProgressBarStyle` / `SplashMessageStyle` / `SplashDetailStyle` / `SplashFooterStyle` 各自精确命中一个节点
  （不是 0 个、也不是多个）；生成类型名与既有生成 Style 名无冲突。
- 优先级关系成立：`SplashLogoStyle` 的宽高覆盖 `LogoSize` 并且**可见徽标尺寸与 Setter 一致**（以 `Bounds` 断言，避免模板内容自带固定尺寸造成溢出假通过），`SplashProgressBarStyle` 的 `Height` 覆盖 `ProgressBarHeight`，
  `SplashTitleStyle` 的 `Foreground` / `FontSize` 覆盖 Token 投影值，且不改变自然测量之外的布局契约。
- 尺寸基线契约成立：固定 `Width` + `MinHeight` 下内容仍可自然增长；给 `content` 设置固定 `Height` 会破坏该基线（失败回归）。
- 文档一致性：本文件、`overview.md` 的 Part 摘要表与生成的 LLMS 语义文档保持一致，descriptor 与实际模板 marker 无差集。
- Generator 静态输出与 NativeAOT 路径不依赖反射或运行时扫描：九个 marker 通过静态 AXAML class 在既有模板路径一次性添加，
  不引入 VisualTree 搜索、动态 marker 绑定或运行时 AXAML 解析。Extras 是首个采用 Semantic Part 的包，需按
  [Gallery NativeAOT 发布流程](../../../../engineering/workflows/gallery-aot-release-workflow.md)验证发布链路。
