# Splash 语义结构

> 生成产物：由源文档生成，不要手工编辑。修改内容请回到控件文档、源码 public surface、Token 类型或生成数据、Gallery ShowCase 或源码结构。

## Semantic Parts

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

## Abstract AXAML Structure

来源：`src/AtomUI.Desktop.Controls.Extras/Splash/Themes/SplashTheme.axaml`

```xml
<Border Name="PART_RootLayout">
    <Border Name="PART_SurfaceLayout">
        <StackPanel Name="PART_ContentLayout">
            <ContentPresenter Name="PART_LogoPresenter" />
            <TextBlock Name="PART_TitleBlock" />
            <TextBlock Name="PART_SubtitleBlock" />
            <ContentPresenter Name="PART_ContentPresenter" />
            <Panel Name="PART_ProgressLayout">
                <Spin Name="PART_Spin" />
                <ProgressBar Name="PART_ProgressBar" />
            </Panel>
            <TextBlock Name="PART_MessageBlock" />
            <TextBlock Name="PART_DetailBlock" />
            <ContentPresenter Name="PART_FooterPresenter" />
        </StackPanel>
    </Border>
</Border>
```

## Composition Model

该章节由控件 `Themes/` 文件夹中的真实主题文件生成，用于说明 public 控件与内部协作对象之间的运行时结构。内部节点只用于理解和维护，不应指导用户代码直接依赖。

### 控件角色图

```text
Splash
  -> Splash (control theme, SplashTheme.axaml)
     -> Border#PART_RootLayout (template-stable)
        -> Border#PART_SurfaceLayout (template-stable)
           -> StackPanel#PART_ContentLayout (template-stable)
              -> ContentPresenter#PART_LogoPresenter (template-stable)
              -> TextBlock#PART_TitleBlock (template-stable)
              -> TextBlock#PART_SubtitleBlock (template-stable)
              -> ContentPresenter#PART_ContentPresenter (template-stable)
              -> Panel#PART_ProgressLayout (template-stable)
                 -> Spin#PART_Spin (template-stable)
                 -> ProgressBar#PART_ProgressBar (template-stable)
              -> TextBlock#PART_MessageBlock (template-stable)
              -> TextBlock#PART_DetailBlock (template-stable)
              -> ContentPresenter#PART_FooterPresenter (template-stable)
  -> SplashWindow (control theme, SplashWindowTheme.axaml)
     -> ShadowsAwareContainer#PART_SurfaceHost (template-stable)
```

### 协作节点

| 节点 | 类型 | 来源 | 生命周期 owner | 影响的 public API | 稳定性 | Agent 使用边界 |
| --- | --- | --- | --- | --- | --- | --- |
| `Splash` | public control | `源文档 + public API` | 用户代码 / 控件宿主 | public API | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `Splash` | control theme | `SplashTheme.axaml` | 用户代码 / 控件宿主 | `Background`, `Content`, `ContentTemplate`, `CornerRadius`, `Detail`, `Footer` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PART_RootLayout` | template node (Border) | `SplashTheme.axaml` | Splash | `Background`, `Content`, `ContentTemplate`, `CornerRadius`, `Detail`, `Footer` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_SurfaceLayout` | template node (Border) | `SplashTheme.axaml` | Splash | `Background`, `Content`, `ContentTemplate`, `CornerRadius`, `Detail`, `Footer` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ContentLayout` | template node (StackPanel) | `SplashTheme.axaml` | Splash | `Content`, `ContentTemplate`, `Detail`, `Footer`, `FooterTemplate`, `IsProgressBarVisible` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_LogoPresenter` | template node (ContentPresenter) | `SplashTheme.axaml` | Splash | `Logo`, `LogoTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_TitleBlock` | template node (TextBlock) | `SplashTheme.axaml` | Splash | `Title` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_SubtitleBlock` | template node (TextBlock) | `SplashTheme.axaml` | Splash | `Subtitle` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ContentPresenter` | template node (ContentPresenter) | `SplashTheme.axaml` | Splash | `Content`, `ContentTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ProgressLayout` | template node (Panel) | `SplashTheme.axaml` | Splash | `IsProgressBarVisible`, `IsSpinVisible`, `ProgressValue` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_Spin` | template node (Spin) | `SplashTheme.axaml` | Splash | `IsSpinVisible` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_ProgressBar` | template node (ProgressBar) | `SplashTheme.axaml` | Splash | `IsProgressBarVisible`, `ProgressValue` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_MessageBlock` | template node (TextBlock) | `SplashTheme.axaml` | Splash | `Message` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_DetailBlock` | template node (TextBlock) | `SplashTheme.axaml` | Splash | `Detail` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `PART_FooterPresenter` | template node (ContentPresenter) | `SplashTheme.axaml` | Splash | `Footer`, `FooterTemplate` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |
| `SplashWindow` | control theme | `SplashWindowTheme.axaml` | 用户代码 / 控件宿主 | `Splash` | public | 用户可直接使用 public 控件；可作为示例和 API 入口。 |
| `PART_SurfaceHost` | template node (ShadowsAwareContainer) | `SplashWindowTheme.axaml` | SplashWindow | `Splash` | template-stable | 用于主题维护；变更需同步主题、实现和 LLMS。 |

## Template Parts

| Template Part | 类型 | 职责 |
| --- | --- | --- |
| `PART_RootLayout` | `Panel` | 承载启动页根布局、背景、圆角和阴影边界。 |
| `PART_LogoPresenter` | `ContentPresenter` | 展示 `Logo` 和 `LogoTemplate`。 |
| `PART_TitleBlock` | `TextBlock` | 展示主标题。 |
| `PART_SubtitleBlock` | `TextBlock` | 展示副标题。 |
| `PART_MessageBlock` | `TextBlock` | 展示当前状态消息。 |
| `PART_DetailBlock` | `TextBlock` | 展示详细信息或错误详情。 |
| `PART_ProgressBar` | `ProgressBar` | 展示确定进度。 |
| `PART_Spin` | `Spin` | 展示不确定加载状态。 |
| `PART_ContentPresenter` | `ContentPresenter` | 展示扩展内容。 |
| `PART_FooterPresenter` | `ContentPresenter` | 展示底部内容或启动失败操作入口。 |

## Pseudo Classes

控件专属伪类：

| 伪类 | 含义 |
| --- | --- |
| `:loading` | `Status` 为 `Loading`。 |
| `:success` | `Status` 为 `Success`。 |
| `:error` | `Status` 为 `Error`。 |
| `:indeterminate` | `IsIndeterminate` 为 `true`。 |
| `:determinate` | `Progress` 有有效值且 `IsIndeterminate` 为 `false`。 |

## State Flow

Splash 的状态流按以下路径收敛：

```text
Splash visual API / SplashService API / Splash static API
  -> Splash instance properties
  -> pseudo-class / template binding
  -> ControlTheme selector / ProgressBar / Spin / TextBlock
  -> SplashWindow visible behavior
```

状态维护规则：

- `Splash` 视觉控件只持有可展示状态，不创建主窗口、不关闭应用、不吞异常。
- `Splash` 本体提供 `SetMessage`、`SetProgress`、`SetStatus` 和 `SetError` 状态写入方法。
- `SplashWindow` 只持有窗口级状态和关闭动效，不解释业务启动步骤。
- `SplashService` 是实例 API 的状态 owner，同一个服务实例一次只管理一个 `CurrentWindow`。
- `Splash` 静态 API 只委托给 `Splash.DefaultService`，不直接持有窗口或视觉节点。
- `Progress` 为 `null` 或 `IsIndeterminate=true` 时显示不确定加载；`Progress` 有效且 `IsIndeterminate=false` 时显示确定进度。
- `Status=Error` 时保留窗口，等待调用方决定重试、退出或显示补充内容。
- `CloseAsync()` 必须幂等；重复调用、取消或窗口已关闭都不能留下不可释放窗口引用。
- 所有服务 API 写入 UI 状态时必须回到 UI thread。

## Theme and Token Boundaries

Splash 的视觉模型由 `Splash` 控件模板、`SplashWindow` 宿主主题、SharedToken 和 `SplashToken` 共同构成。

| 主题文件 | 职责 |
| --- | --- |
| `SplashTheme.axaml` | 定义启动页视觉控件模板、状态 selector、ProgressBar/Spin 组合和内容区域。 |
| `SplashWindowTheme.axaml` | 定义桌面启动窗口宿主、透明无装饰窗口模板、阴影宿主和内容承载边界。 |

Splash 使用独立的 Control identity 和 `SplashToken` Own Token scope。Token 只表达组件视觉语义，不承载 `Status`、`Progress`、`IsIndeterminate`、启动步骤或异常对象。`SplashTheme.axaml` 通过 `SplashTokenResource` 统一读取 Splash 的 Effective Global Token 和 Own Token：标题读取 Global Token `ColorTextHeading`，普通消息读取 Global Token `ColorText`，副标题和详情读取 Own Token `SubtleForeground`。默认没有 Splash Control 级覆盖时，Effective Global Token 回退到当前主题的全局结果，因此默认 Light/Dark 视觉不变。
`SplashWindow` 使用 `{x:Type atom:SplashWindow}` 作为隐式 `ControlTheme` key；窗口模板必须保持透明内容宿主，避免默认 Window 背景破坏 Splash 表面圆角。
`SplashWindowTheme.axaml` 直接使用 `ShadowsAwareContainer#PART_SurfaceHost` 承载 `Splash`，由 `SurfaceBoxShadow` 控制窗口表面阴影，由 `SurfaceCornerRadius` 控制阴影遮罩圆角。`SplashTheme.axaml` 内部的 `PART_RootLayout` 和 `PART_SurfaceLayout` 继续负责背景、内容圆角和裁剪。

资源覆盖边界：

- 同时影响窗口阴影宿主和 Splash 内容表面的视觉资源，应写入 `SplashWindow.Resources`。
- 只影响 `Splash` 内部模板的资源，可以写入 `Splash.Resources`。
- 应用需要完整的专用启动窗口视觉时，定义自己的 `SplashWindow` 和内部 Splash 子控件；子控件通过 `StyleKeyOverride` 复用标准 Splash Theme，并由自己的 AXAML `Styles` 维护专用模板视觉。窗口或页面不得进入 Splash 模板修改内部节点。
- 不通过 C# `TokenResourceBinder` 在窗口宿主和 Splash 之间桥接 `SurfaceBoxShadow`、`SurfaceCornerRadius` 等模板可表达关系。

主题维护规则：

- 不删除或重命名已经稳定的 ControlTheme key、template part、伪类和资源 key。
- 视觉结构优先使用 AXAML、`TemplateBinding`、selector、`Spin`、`ProgressBar` 和 `ContentPresenter` 表达。
- 不把状态显示逻辑改成 C# 动态创建视觉，除非 AXAML 无法表达且生命周期 owner 明确。
- Gallery 和业务代码不得从页面、父控件 Style 或窗口 ControlTheme 通过 `/template/`、C# `.Template()` 或 `PART_*` 名称进入 Splash 模板。专用 Splash 子控件可以通过 `StyleKeyOverride` 复用标准 Splash Theme，并在自己的 AXAML `Styles` 中进入自己的一层模板；该子控件承担模板契约所有权。
- Light/Dark 主题应保持品牌区域、进度区域、错误状态和窗口表面的对比度。

Token 边界：

Splash Token 只表达组件级视觉变量，例如窗口尺寸、内容间距、品牌尺寸、文字规格、进度区域间距、圆角、阴影和状态色。Token 不承载启动步骤、`Status`、`Progress`、`IsIndeterminate`、异常对象、主窗口引用或服务状态。

当前 Token scope：

- `SplashToken`，scope id 为 `Splash`，源码位于 `src/AtomUI.Desktop.Controls.Extras/Splash/SplashToken.cs`。

## Customization Boundaries

维护 Splash 时必须保持以下不变量：

- `Splash` 视觉控件不接管应用生命周期，不创建主窗口，不吞业务异常。
- `SplashService` 不替调用方决定错误后退出、重试或继续。
- 静态 API 不绕过 `ISplashService`，不直接持有窗口或模板节点。
- `CloseAsync()` 保持幂等，最短展示时间和关闭延迟不会导致窗口引用泄漏。
- `Status`、`Progress`、`IsIndeterminate` 的优先级稳定，Gallery 和用户 XAML 可依赖。
- Template part、伪类、ControlTheme key、Token 名称和资源 key 不擅自变更。
- 不引入运行时反射扫描作为 API、Token、服务或窗口发现机制。
- 文档只描述当前稳定设计；历史变化记录在 `changelog.md`。

维护不变量：

维护 Splash 时不得破坏：

- 视觉控件、窗口宿主、实例服务和静态 API 的职责边界。
- Public API、默认值、服务委托路径和 Gallery 可观察行为。
- Template part 名称、ControlTheme key、伪类和资源 key。
- `SplashTokenResource` 对 Effective Global Token 与 Own Token 的统一消费边界；`SharedTokenResource` 只用于明确要求永远跟随真正 Global Token snapshot 的值。
- `CloseAsync()` 幂等、最短展示时间、关闭延迟和引用释放路径。
- Light/Dark、不同 DPI、不同平台窗口系统下的主题一致性。
- 控件文档、源码 public surface、Token 类型或生成数据与源码契约的一致性。
