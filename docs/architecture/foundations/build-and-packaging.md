# 构建与打包

> 本文对应本地已实现的构建资产与打包布局；本地验证、未完成门槛与发布状态由
> [AOT 与裁剪架构](aot-and-trimming.md#1-状态与事实边界)统一维护。

AtomUI 使用集中化 MSBuild 配置。顶层 Directory.Build.props/targets 导入 Repository 聚合入口；构建资产显式列入白名单，
按普通生成、资源编译、本地化、注册后端和发布验证划分职责。

## 基础设施边界

```text
build/
├── AtomUI.Build.Tasks.Process.cs
├── AtomUI.Generator.props
├── AtomUI.Generator.targets
├── AtomUI.GeneratorConsumer.targets
├── AtomUI.Registration.targets
├── AtomUI.Localization.props
├── AtomUI.Localization.targets
├── AtomUI.Repository.props
├── AtomUI.Repository.targets
├── AtomUI.ThemeAssets.targets
├── MacOSHomebrewNativeAot.targets
├── OutputPaths.props
├── PackageMetadata.props
├── ProjectDefaults.props
└── Versions.props
```

该树为当前资产面。Registration target 只选择后端、设置 linker feature switch、配置 Browser 扩展及检查产物；
不收集应用 C#/AXAML usage、不传播 ProjectReference 发布上下文，也不生成应用注册计划。

## Repository 与 NuGet 入口

Repository.props 管理版本、项目默认值、输出路径和唯一的 NuGet build/tool asset 白名单；Repository.targets 执行一致的
注入与打包规则。包项目不复制另一套工具清单。

普通 AtomUI.Generator 仍以 netstandard2.0 Analyzer 交付；构建任务使用独立 .NET 构建宿主；Browser 映射转换器位于独立
`AtomUI.TypeMap.Linker` 构建项目。目标项目与状态见[TypeMap Linker 模块](../../modules/typemap-linker/overview.md)。

产品包提供 package-specific `buildTransitive/<PackageId>.props/.targets` 并幂等注入工具。多个产品包同时引用时只允许一份
兼容的普通生成器与一份 Browser 转换器；编译工具不得进入 lib/、runtime 依赖图或应用 publish 输出。
本地化 props 继续按已验证的 Catalog/Bundle 资产生成；不能把语言包 metadata 与已移除的注册协议混淆。

第三方包在普通编译中生成 TypeMap、Package marker、单项 factory 与资源契约。应用只读解析后的引用 metadata 输出
TypeMapAssemblyTarget。包引用本身不执行 provider 或 initializer，普通预编译 consumer DLL 不需要附带使用清单。

## 构建任务与文件生命周期

所有需要 AtomUI.Build.Tasks 的 feature target 使用唯一的 AtomUIBuildTasksAssembly。
任务通过 RoslynCodeTaskFactory 的薄适配器启动独立 `dotnet AtomUI.Build.Tasks.dll` 进程，等待退出后返回结果；不加载到常驻
MSBuild 节点。取消时终止本次 worker，清理请求目录，不留下后台 worker。

worker 的 DLL、deps.json、runtimeconfig、必要依赖和适配器源码一起交付；协议只传值，不跨进程传自定义任务对象。
它不发起嵌套项目构建。新任务或参数同时更新适配器、分发与隔离测试，语言/资源功能继续使用该宿主。

Browser linker 扩展必须加载到其对应 ILLink 阶段，不能混入上述 worker 的常驻加载策略，也不进入应用 Runtime。
其专用 ABI、增量输入和清理边界由[Browser TypeMap 链接](aot-browser-linking.md)定义。

MacOSHomebrewNativeAot.targets 只服务本仓库 macOS NativeAOT 链接，不随 NuGet 分发；其他平台使用自身工具链。

## 主题资产的平台输入

边界由[类型与资源平台契约](aot-typemap-registration.md#7-平台分支)定义：MSBuild 只提供普通 AXAML 输入和资源编译接线，
生成器通过 Compilation 读取控件/资源类型的标准平台声明。项目、props 和 targets 均不维护主题平台路径清单。

`AtomUI.Desktop.Controls.csproj` 的平台排除列表及 `AtomUI.ThemeAssets.targets` 的自定义平台 metadata 暴露已删除。
普通资源输入不再承载第二份平台清单，也不保留旧 metadata 的兼容读取路径。第三方包须使用配套生成器完成普通重建，具体见[迁移说明](../../releases/unreleased-typemap-registration-migration.md#资源平台声明收敛)。

`AvaloniaXaml`、`AdditionalFiles`、`Link`、项目目录、package/catalog 身份和普通资源编译继续使用。
`GenerateThemeAssetWrappersTask` 及其 `BeforeTargets="GenerateAvaloniaResources"` 接线保留；资源字典和默认 typed
ControlTheme 的 deferred wrapper、URI、顺序与构造时机保持原契约。新 `x:Class` 资源仍走 Avalonia 正常编译和加载。

不新增平台扫描任务、第二次项目编译或运行时反射发现。平台声明属于现有 C# 编译输入，AXAML 身份属于已有资源输入；
对两者的增量失效在普通 Generator 测试中验证。源码/冷 NuGet 消费需共同证明项目无需平台路径配置，工具仍只注入一次，
Browser 后端继续消费同一份已生成 TypeMap 和官方标记结果，无须新增转换协议。

## Target Framework 与工具链

新控件注册体系的产品目标统一到 net10.0；Browser 使用 net10.0-browser，不携带旧 TFM 的注册兼容分支。
产品项目已使用上述 TFM；Generator 保留 netstandard2.0 工具目标。历史发布版本的 TFM 记录仍按对应版本保留。

TFM 不能代表全部工具兼容性。实际 SDK、ILLink、NativeAOT 与 WebAssembly workload 组合必须经过发布验证；普通构建资产
自动检查支持范围。不支持的 Browser linker ABI、缺工具或未转换 accessor 应阻止发布，不能改成保留全包。

## 注册后端与构建模式

| 模式 | 注册与构建 |
| --- | --- |
| Debug / 非裁剪 Release | 同一工厂事实源的完整片段集合，不调用 Mono 未实现的 TypeMapping |
| trimmed CoreCLR | 官方 TypeMap 保留结果 |
| Desktop NativeAOT | ILC 原生条件映射 |
| Browser trimmed / Browser AOT | ILLink 完成标记后转换自有 accessor；AOT 和 bundling 消费转换后的 IL |

一个 linker feature switch 区分完整与选中路径；不能用包自身 DEBUG 条件替代消费应用的发布模式。
裁剪路径不得通过共享静态字段或 full manifest getter 保留所有工厂。

Browser 扩展 DLL、依赖、配置和 ABI 都是增量链接输入。扩展变化必须使链接失效；内容不变应正常复用。
完整契约见[TypeMap 注册管线](aot-typemap-registration.md)和[注册 ABI](../../reference/aot/typemap-contract.md)。

## 包版本管理

`Directory.Packages.props` 启用 Central Package Management：

```xml
<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
```

Avalonia、ReactiveUI、Roslyn、测试依赖等版本在此统一管理。Release 条件下还会配置 AtomUI 各 NuGet 包版本。

AtomUI 自身版本由 `build/Versions.props` 中的 `AtomUIVersion` 管理。

## 正确性验证

先使用统一 affected verification 计划检查变更与专项义务：

```bash
python3 scripts/verification/test.py plan
python3 scripts/verification/test.py run
```

注册迁移要求同时验证 source、传递 ProjectReference、预编译 consumer DLL、干净 NuGet cache 和第三方包。
构建任务隔离测试继续验证同一 MSBuild 会话中重复执行、覆盖/删除工具输出；不以单次新进程构建代替文件生命周期证据。

必须实际启动 Desktop NativeAOT、trimmed CoreCLR、Browser trimmed 和 Browser AOT 产物，检查模板、Token、Semantic Part、
主题切换和未使用类型缺失。仅 publish 成功或旧注册脚本通过不能证明新架构完成；迁移同时更新脚本、断言和验证策略。
包检查读取 nupkg 条目，确认仅包含目标构建资产且无旧注册工具和协议残留。

## 破坏性变更门禁

当前源码的 net10 与注册工具布局迁移见 [未发布 TypeMap 迁移说明](../../releases/unreleased-typemap-registration-migration.md)。

发布准备对上一版做两道机械化比对，避免只靠人工审计漏掉破坏性变更。两道门禁观测产物事实，不依赖提交的 `!` 标记。

- **包 API 校验**：`build/PackageValidation.props` 在传入 `AtomUIPackageValidationBaselineVersion` 时启用
  `EnablePackageValidation` 与 `PackageValidationBaselineVersion`，以 ApiCompat 做 `lib/` 公共 API 的 IL 级比对，
  并覆盖跨 TFM 一致性。普通开发构建不传该属性，因此不拉取基线包、也不变慢。AtomUI 开启 Avalonia private API
  后，Avalonia 只在 `CoreCompile` 前加入真实拆分程序集；ApiCompat 不执行到该阶段，因此校验 target 会在 SDK 收集完
  每个 TFM 的引用后，把同一组 Avalonia implementation DLL 追加到现有 `PackageValidationReferencePath`。发布日志中若
  出现 `Could not resolve reference`，表示 API 校验引用图不完整，不能按普通 warning 忽略。
- **包布局校验**：`scripts/verification/verify-package-layout.ps1` 比对 `lib/` 的 TFM 集合、`tools/`、`build/`、
  `buildTransitive/`。ApiCompat 看不到这些路径，而 6.1.9 的 `tools/netstandard2.0 → tools/net10.0` 正属于此类。

`scripts/BuildNuGetPackages.ps1` 的 `-PackageValidationBaselineVersion` 同时驱动两者：它先对发布包项目 restore 以拉取
基线包（pack 使用 `--no-build`，不会自行 restore），随后 pack，最后运行布局校验。前置工具项目不是发布包，不施加校验
属性。有意变更的豁免分别是 `build/PackageValidationSuppressions/<ProjectName>.xml` 与
`scripts/verification/package-layout-allowlist.json`（按基线版本与包 ID 记录，未命中的条目会导致失败）。

本地验证：

```bash
pwsh -NoProfile -File scripts/verification/verify-package-layout.Tests.ps1
pwsh -NoProfile -File scripts/verification/verify-package-layout.ps1 \
    -PackageDirectory <release-dir> -BaselineVersion <previous-version>
dotnet test tests/AtomUI.Generator.Tests/AtomUI.Generator.Tests.csproj --framework net10.0 --no-restore
```

判定口径与发布流程见 [版本发布准备规范](../../engineering/workflows/release-preparation.md)。

## 源生成输出

需要把源生成结果写到仓库目录的项目只声明标准 SDK 属性：

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>GeneratedFiles</CompilerGeneratedFilesOutputPath>
```

`GeneratedFiles/` 是本地编译产物，默认由 `.gitignore` 忽略；只有 GalleryBase 中被结构测试直接读取的少量快照保留跟踪。

`AtomUI.Repository.targets` 根据 `CompilerGeneratedFilesOutputPath` 统一从 `Compile` 移除这些快照，避免第二次构建把上次
生成结果作为普通源码再次编译。项目文件不得重复声明同一条 `Compile Remove`。源生成器通过 Analyzer 方式参与当前编译。

## 打包边界

当前源码打包边界为：

- `AtomUI.Native`
- `AtomUI.Core`
- `AtomUI.Fonts.AlibabaSans`
- `AtomUI.Icons.Shared`
- `AtomUI.Icons.AntDesign`
- `AtomUI.Controls.Shared`
- `AtomUI.Controls`
- `AtomUI.Desktop.Controls`
- `AtomUI.Toolkits.GalleryBase`
- `AtomUI.Desktop.Controls.DataGrid`
- `AtomUI.Desktop.Controls.ColorPicker`
- `AtomUI.Desktop.Controls.Extras`
- `AtomUI.Generator`

DataGrid、ColorPicker 和 Extras 是独立按需包，但源码上依赖 `AtomUI.Desktop.Controls` 并访问其内部成员。
GalleryBase 是产品中立的 Gallery 应用底座包，跟随主库版本发布，供 AtomUI 生态内的产品 Gallery、Demo 和文档应用复用。

正式 NuGet 项目、Package ID 和发布分组统一声明在 `scripts/NuGetPackageProjects.ps1`。GitHub Actions 发布 workflow、
本地 NuGet 发布脚本和产物完整性校验必须共同消费该清单，不得分别维护项目列表。新增、拆分或移除正式包时，先更新该清单，
并让缺包或多包校验在上传与推送前失败。

`scripts/BuildNuGetPackages.ps1` 是正式包的唯一构建编排入口。它先构建清单中的 Build Tasks、普通 Generator 和 Browser TypeMap linker 工具
等非包前置项目，再完成全部包项目的 build，之后才允许执行任何 pack。发布构建必须关闭 MSBuild 节点复用、串行访问共享
工具输出，并在完整包集合校验通过后才进入本地 feed、artifact upload 或 nuget.org push；任一 `dotnet` 命令失败都必须立即
终止流程。pack 不为注册信息发起第二套应用分析或嵌套构建；它封装同一次普通编译生成的 metadata 和工具。

注册型产品包必须从同一次、同版本 Release 构建中封装 Generator 和 Build Tasks。不得在修改 `AtomUIVersion` 后使用
`dotnet pack --no-build` 复用另一个版本留下的工具输出；多个同版本产品包中的编译资产必须具有一致内容。
