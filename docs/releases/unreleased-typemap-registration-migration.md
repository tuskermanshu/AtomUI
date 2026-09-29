# 未发布：TypeMap 注册迁移

本文记录当前源码分支的破坏性迁移，不代表一个已发布版本，也不改变仓库版本号。
发布前仍须完成正式平台、体积、性能及 API 兼容性审查。

## 应用与控件包

产品库统一为 `net10.0`，Browser 应用使用 `net10.0-browser`；不再提供 `lib/net8.0`。构建需要支持官方 TypeMap 的 .NET 10 SDK。普通运行与裁剪发布使用相同的生成片段与包入口；应用保留正常的 `UseAtomUI` / `UseDesktopControls` 配置，不维护控件名单。

第三方控件包必须用当前 Generator 重新构建并重新分发。移除旧 `AotTrimRegistration`、`AotTrimRegistrationPlanRegistry`、`AotTrimControlPackageRegistrationBuilder`、`AotTrimUnitAttribute` / `AotTrimGeneralUnits` 及 `ControlPackageRegistrationEntryAttribute`。旧 Unit、应用 usage、Sidecar、Plan、全包回退及其生成 ABI 不再被读取或执行，旧二进制注册协议不能直接复用。

旧主题发现标记 `ControlThemeProviderAttribute` 与单主题 `IControlThemeProvider` 接口一并删除，不保留无消费者的兼容空壳。
当前包使用 `IControlThemesProvider` / `ControlThemesProvider` 和生成的注册入口；`BaseControlTheme` 的独立强类型模板绑定能力保持不变。

作者使用 [第三方控件包接入](../guides/theming/third-party-control-packages.md) 定义普通包入口。资产导出来自实际 AXAML 目标和资源键，局部资源通过词法作用域及常量 include 解析；删除目录推导所有权和 `AtomUIPackageSharedTheme` / `AtomUIPackageSharedThemePaths` 开关。真正公共的包资源由显式 Package Core/provider 提供，辅助资源使用正常 include。

`ControlThemeAssetDescriptor` 使用 AssetId、实际 Type 导出、带 owner Type 的 Token identity 及语义绑定；删除旧 owner/reference/string 资产构造器与属性。`ControlThemeSemanticPartDescriptor` 的目标必须是 `Type`；删除字符串目标构造器。`ControlPackageRegistration` 使用包含语义描述和资源注册的完整构造器。共同提交先验证 schema，再冻结注册；失败后的 builder 不能继续使用。

## 构建与包布局

这些相对于 6.1.8 的变更在 `scripts/verification/package-layout-allowlist.json` 按包和具体路径确认：

- 保留 6.1.9 已记录的 worker 从 `tools/netstandard2.0` 移至 `tools/net10.0`，以及 SDK 编译的进程适配器。
- 删除旧 `AtomUI.Generator.LinkedPublish` analyzer、三个 `AtomUI.LinkedRegistration*.props/targets` 与 `.atomui-link.json` 资产。
- 新增 `buildTransitive/AtomUI.Registration.targets`，自动解析当前 Generator/worker/Browser backend；普通包消费者无需手动添加分析器。
- 当前 Generator 及其依赖位于明确的 `tools/netstandard2.0` / `analyzers/dotnet/cs` 位置，worker 位于 `tools/net10.0`，Browser backend 位于 `tools/typemap`。构建工具不属于产品 `lib` 或应用发布输出。
- 源码 worker 单独输出到 `.artifacts/bin/<Configuration>/build-tasks/net10.0`，防止消费者的增量清理删除工具；NuGet worker 路径保持 `tools/net10.0`。

语言模板/语言包、资源 wrapper、进程隔离与取消、动态数据 accessor、字体图标及原生平台能力继续保留。构建和发布细节见 [构建与打包](../architecture/foundations/build-and-packaging.md)，TypeMap 契约见 [注册架构](../architecture/foundations/aot-typemap-registration.md)。

## 验证入口

当前可复现消费矩阵位于 `tests/AtomUI.Registration.Fixtures`。`run_package_consumers.py` 从当前可交付源文件建立独立源码快照、空 NuGet cache 和本地 feed，验证独立作者、直接/传递引用、普通预编译 adapter 及 Browser 消费。旧三个 `verify-aot-*-registration.sh` 仅验证已删除的协议，随旧 fixtures 退役。

包布局 gate 仍拒绝未确认的新增/删除路径和未使用的允许项。发布包必须提供完整包清单再对比历史基线；部分 feed 不能证明完整布局门禁通过。`verify-build-task-isolation.ps1` 支持源码、已打包工具和空工具输出目录打包模式，并在活 MSBuild 进程中验证重复执行、强制覆盖、目录删除、整数转换与取消后的句柄释放。

### 资源来源与平台声明

普通编译现在拒绝无法证明来源的跨资产 `StaticResource`，包括同包另一个文件导出的命名主题。
字符串依赖改为普通显式 `ResourceInclude`，不要添加 AOT owner 清单。
真实 type-key 对应相同 TargetType 的已注册默认主题且平台域兼容时，由实际类型条件保留，继续允许应用级主题替换。Semantic Theme metadata 仅来自真实 typed
属性赋值；同名文件不会自动关联主题。未赋值的语义属性保留声明的 ContractType。

多目标资源字典必须在显式资源域内具有一致的平台可用性，且所需 Token owner 覆盖该域。
通过资源拆分或正常平台声明修正 `ATOMUIREG006`；不应使用 warning suppression 隐藏缺失 schema。
直接 `AddControlPackage` 失败后，该 builder 与生成入口一样不可继续 Build；请创建独立 builder 重试。

## 资源平台声明收敛

当前源码已采用[类型与资源平台契约](../architecture/foundations/aot-typemap-registration.md#7-平台分支)，
这次收敛与上文旧注册管线退役分别记录。`AtomUISupportedOSPlatforms` / `AtomUIUnsupportedOSPlatforms`
及 `.csproj` 主题平台路径列表已删除，由标准 .NET 类型平台声明和正常资源 CLR 声明替代，不提供旧 metadata 的兼容读取分支。

包作者与维护者的迁移顺序及验收条件：

1. 保存现有逐控件/资产平台域及 Browser 工厂保留基线，区分控件本身受限与本包主题受限；逐项核对现有路径列表覆盖的资源。
2. 生成器接入 ResourceDictionary/ControlTheme 的资源 CLR 身份与标准平台声明，复用现有域模型和 guard；
   增补无资源类的程序集域、非法 `x:Class`、多目标冲突、Token owner 覆盖及增量失效测试。
3. 普通主题复用 TargetType 的已有声明；真实原生辅助类型补齐自身声明；仅主题受限的资源使用现有或新增正常资源类。
   外部控件不要求上游修改，可复用控件不因其当前主题受限而被整体禁用。
4. 证明逐资产有效域与原行为等价，再同时删除项目列表、`AtomUI.ThemeAssets.targets` 的平台 metadata 暴露及
   `ThemeAssetInput`、`ThemeAssetInfo`、`TokenResourceKeyGenerator` 中的旧字段和读取。普通资源输入、Link 和 wrapper 保留。
5. 从源码与干净 NuGet 消费执行验证，确认普通全量与桌面裁剪的模板、资源顺序和应用级主题替换不变；Browser trimmed
   interpreter 与 `RunAOTCompilation=true` 产物均实际运行，并证明不可用的 AtomUI 资产工厂被删除。
6. 验证资源移动/重命名后平台域不变，同目录新增无关控件/主题不继承限制；源码迁移与发布验收分别记录，以总体状态页的新证据为准。

第三方包需删除旧元数据、采用对应声明并用配套 Generator 普通重建；应用仍保留原包入口，预编译 adapter 不新增使用清单。
本次不改变 TypeMap ABI v1、资源 schema/fingerprint 或 Browser 转换协议，不拆分原生窗口包，也不扩大平台支持范围。
既有验证记录不证明本项迁移完成；本项验证关注行为、裁剪、增量正确性和分发，既有体积门槛与未达标记录继续保留。

## Token 描述符生成形态与桌面工具链门禁

普通 Generator 生成的 Token 描述符改用隐藏 ABI `TokenDescriptor.CreateGenerated`：按值类型共享
`TokenValueCodec<T>.Instance`，按 Token 类生成一个 `TokenValueAccessor`，不再为每个 Token 生成 parse/format/get/set/project
五个委托。公开委托构造函数与 `Parse`/`Format`/`GetValue`/`SetValue`/`ProjectResourceValue` 行为不变；用旧 Generator 构建的
第三方包无需修改即可继续运行，用当前 Generator 重建后才获得体积与启动分配收益。新生成代码需要配套的 AtomUI.Core。

桌面 trimmed CoreCLR 与 NativeAOT 发布不再要求 SDK 恰好为 `10.0.300`，也不再要求 ILCompiler 与 ILLink 的某个构建完全一致，
只要求实际官方 ILLink 与 NativeAOT 编译器不低于已验证的 `10.0.8`；运行时补丁前滚不再触发 `ATOMUIREG006`。
Browser trimmed/AOT 发布依赖 ILLink 内部管线，继续固定 SDK `10.0.300`、ILLink `10.0.8` 与 WASM assets `10.0.10`。
