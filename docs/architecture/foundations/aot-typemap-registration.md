# TypeMap 注册架构

本文定义 AtomUI 条件映射、生成代理、包引导和注册执行边界。采用状态与源码迁移状态由
[AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)统一说明。
类型与资源事实由 [Control 注册契约](control-registration-contracts.md)定义；隐藏 ABI 的精确格式见
[TypeMap ABI 契约](../../reference/aot/typemap-contract.md)。

## 1. 单一生成事实源

普通 `AtomUI.Generator` 从 Control、Token、Semantic Part 与包内 AXAML 产生：

- 逐 Control 的单项 descriptor factory 与注册片段。
- 独立主题资产、资源 factory 和必要的真实类型引用。
- 各保留条件对应的 TypeMap 声明。
- 纯字符串候选 key 表和包内具体 Group accessor。
- Package marker 与非裁剪全量调用入口。

生成器不分析应用方法体、消费 AXAML usage 或预编译 DLL 的调用图，不输出应用注册计划。
包内资源归属与字符串资源检查只依据普通主题契约，不把旧跨应用依赖推导移到新的命名下。

普通与裁剪执行复用相同的单项 factory。全量集合、全量 URI dispatch 和全控件静态数组只能由非裁剪分支访问；
不能与 map accessor、Group、候选 key 或共享 helper 放进会共同初始化的全局状态。

## 2. 逐条件映射

一个 Control 片段可以有多个独立保留条件，例如 Control、Token extension、Token enum 和专用 Semantic Style。
每个条件分配唯一 key，多个 key 可以指向同一代理：

```csharp
// 隐藏生成代码，key 格式由生成器定义。
[assembly: TypeMap<DesktopMap>(
    "Button/control", typeof(ButtonRegistration), typeof(Button))]
[assembly: TypeMap<DesktopMap>(
    "Button/token", typeof(ButtonRegistration), typeof(ButtonTokenResourceExtension))]
[assembly: TypeMap<DesktopMap>(
    "Button/style-content", typeof(ButtonRegistration), typeof(ButtonContentStyle))]
```

不得为同一个 key 声明不同 trimTarget 来模拟 OR；不同条件必须使用不同 key。Key 的唯一性包含具体 Group 和稳定的
程序集/类型身份，不能以简单类型名替代完整身份。

所有类型、Token、主题和代码引用先交给官方链接器计算闭包。片段不递归调用其他片段；类型之间的保留循环由官方闭包收敛，
不需要 AtomUI 的运行时 visited graph 或应用 SCC planner。

有用内容为空的派生 Control 无需空片段。internal Control 可以只有资源片段；资源字典的不可分割边界不等于包或目录粒度。

## 3. 代理与激活

每个代理是带自身 Attribute 的 sealed 类型，继承隐藏 ABI `ControlRegistrationFragmentAttribute`。
其 `Add(ControlPackageRegistrationBuilder)` 只添加本 Control 的可选 Token/语义 descriptor 与资产记录。
接口以 [ABI 契约](../../reference/aot/typemap-contract.md)为准，不是用户手写扩展点。

运行时对映射已经返回的目标调用已知的 `GetCustomAttribute<ControlRegistrationFragmentAttribute>()`，然后执行 `Add`。
这允许有限的已知 Attribute 读取，不是零反射设计；禁止据此扩展为程序集扫描、动态方法查找或未知构造函数激活。

无 Attribute、代理类型非法、FragmentId 冲突或重复定义不一致时明确失败。代理与 builder 状态只在本次注册上下文存在，
不得建立持有应用 Provider/ThemeManager 的静态缓存。

条件 TypeMap 声明导致的预期 IL2026 仅在对应生成代码处局部处理并说明原因；不能全局关闭 IL2026、IL3050 或其他诊断。

## 4. 候选 key 与具体 accessor

TypeMap 不提供可用的枚举契约。生成器产生纯字符串候选 key 表，运行时逐项 `TryGetValue`，先按返回的 proxy Type 去重，
再读取代理 Attribute。不能使用 `Keys`、`Values`、`Count` 或 `GetEnumerator` 来推导所选条目。

候选表禁止包含 Control Type、代理实例、descriptor、资源实例或工厂 delegate。候选 key 数量决定查询次数，实际保留的
proxy 数量决定激活次数；不能为省去有限查询而引入全量类型根。

每个包必须生成具体、闭合、非泛型 accessor：

```csharp
[GeneratedTypeMapAccessor(typeof(DesktopMap))]
internal static IReadOnlyDictionary<string, Type> GetMap()
    => TypeMapping.GetOrCreateExternalTypeMapping<DesktopMap>();
```

不能将调用移入 `ReadMap<TGroup>()` 等共享泛型 helper。共享 runtime 接收已经取得的 dictionary；具体 Group 查询保留在
包的生成方法体中，以满足官方 TypeMap intrinsic 的静态可识别要求。
Browser accessor 的构建期转换见 [浏览器链接架构](aot-browser-linking.md)。

## 5. Package marker 与应用引导

包生成隐藏的公共 Group 类型及 assembly marker：

```text
ControlPackageMarkerAttribute(packageId, mapGroup, abiVersion)
```

Group 公共可见仅为跨程序集生成代码引用，不允许用户通过继承或实例化扩展注册机制。Group 不得含有引用全包 Control、
descriptor 或工厂的静态状态。marker 表达包身份与 ABI，不表达应用控件清单。

应用 ordinary generator 只读取已解析程序集引用中的 marker，按完整程序集身份、Package ID、Group 和 ABI 校验并去重，
输出具体的 `TypeMapAssemblyTarget<PackageMap>`。完整类型身份必须包含定义程序集，不能只比较 FullName 或 scope 简称。

普通 adapter DLL 不需要重新以特殊 publish 属性构建，也不需要 usage companion。只要其所需的新格式控件包引用能够解析，
就按同一 marker 规则参与引导。marker 冲突、缺失所需引用或 ABI 不兼容时报告明确错误，不恢复旧消费者 IL 分析。

marker 和 assembly target 不是启用动作。没有调用可选包入口时，不查询该包 map、不创建其 Provider、不运行语言和 initializer。
应用引用一个 NuGet 并不代表要启用它。

## 6. 包入口与执行模式

普通无状态第三方入口只需正常注册 Provider：

```csharp
public static IAtomUIBuilder UseAcmeControls(this IAtomUIBuilder builder)
    => GeneratedControlPackageRegistration.Register(
        builder, static () => new AcmeControlThemesProvider());
```

有生命周期逻辑的入口向生成 helper 提供明确的 provider factory、prepare 和 complete 回调。
生成器不读取入口方法体猜调用顺序；作者不写 TypeMap、入口身份 Attribute、AOT 模式分支或依赖名单。
应用仍通过真实 `UseXxxControls()` 启用包。

一个官方 linker feature switch 决定全量片段调用与 map 查询。非裁剪执行直接添加全部片段；裁剪执行只添加 map 中的片段。
两条执行分支复用单项 factory 和同一个 `ControlPackageRegistrationBuilder`，不保留旧应用 PlanRegistry。
feature switch 必须在发布期消除不可达全量分支；仅运行时 `if` 后过滤全量数组不满足裁剪要求。

包入口只暂存记录与 factory，后续统一跨包提交、失败处理与冻结遵循
[注册生命周期](aot-and-trimming.md#6-注册生命周期)。不能为了简化 map 查询提前创建资源或冻结当前包依赖。

## 7. 平台分支

ControlContract 的 `PlatformAvailability` 是普通产品平台能力，生成器将其写进每个片段的真实平台分支。
先判断平台，再引用或构造该片段的 Token、Semantic、asset factory。即使代理因为静态类型证据可达，不可用平台分支内的
工厂仍必须能被链接器删除。

公共 builder 在构造完 descriptor 后才执行 predicate，不能替代片段内平台 guard。
本包类型优先使用正常 .NET 平台可用性声明；包对外部类型的主题覆盖使用正常平台资源声明。跨平台控件不需新增 AOT 配置。
同一资产导出不兼容平台目标、无法形成一致 factory 时，要求正常拆分资源或诊断，不能静默删掉其中一个可用主题。

可用域按平台与版本半开区间规范化，组合 type、containing type、assembly 的普通声明及资产显式平台元数据。
每个导出先与显式资源域相交；所有导出必须得到同一可用域，RequiredTokenOwner 的类型域必须覆盖该域。
共同域的 guard 在每条入口到共享 AddAsset 的路径上、descriptor 与 factory 引用之前执行。
仅取不同导出的隐式交集可能丢失仍可用的主题，因此不作为修复方式。

当前支持 browser、windows、linux、macos/osx、ios、maccatalyst、android、tvos、freebsd；browser/linux 不接受版本，
其他平台接受对应 OperatingSystem API 的版本分量（Windows 至四段，其余至三段）。子声明只收窄父域；
重启已排除区间、多区间重新启用、无法表示的 OS/版本以及空域报告 ATOMUIREG006。
IsIOS/IsIOSVersionAtLeast 包含 MacCatalyst，域比较与生成 guard 均保留这个重叠关系，不能把两者当成互斥谓词。
这些边界遵循 [.NET 平台兼容性分析说明](https://learn.microsoft.com/en-us/dotnet/standard/analyzers/platform-compat-analyzer)
与 [CA1416](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1416) 的可用性语义；
这里声明的是生成器支持的有界语法，并不承诺实现分析器的全部属性组合。


## 8. 验证边界

普通生成器验证输出确定性、key 唯一性、完整类型身份、具体 Group 调用与片段局部平台分支。
发布验证还必须证明：

- Control/Token/Identity/Style 单独使用均能保留正确契约；相同代理多个条件只激活一次。
- 全量数组、全量资源 dispatch 和生成 helper 不把未用 Control 变成根。
- 多 Group、跨程序集及传递引用可用；未启用可选包没有生命周期副作用。
- 资源字典多目标闭包正确，新增无关同目录控件不扩大选择范围。
- 原型中的已选 map 结果不能替代完整产品的 UI、NuGet、平台与体积矩阵。

产品验收与实现状态由 [AOT 与裁剪架构](aot-and-trimming.md#8-验证与交付门槛)统一拥有。
