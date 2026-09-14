# 按改动影响选择验证

本文定义 AtomUI 日常验证入口、影响选择规则和结果有效性。全量测试是显式基线；常规迭代使用受影响范围，
不能把 `dotnet test AtomUI.slnx` 当作默认或完整测试清单。测试项目由仓库实际 `.csproj` 发现，不依赖解决方案是否收录。

## 日常入口

在仓库根目录使用 Python 3.9+；工具仅使用标准库。测试与构建沿用仓库 .NET SDK 和现有 VSTest/xUnit。

```bash
# 查看本地暂存、未暂存、新增、删除和重命名输入对应的计划
python3 scripts/verification/test.py plan

# 开发过程：控件/功能域的直接验证，不构成最终交付验证
python3 scripts/verification/test.py run --scope iterate

# 收尾：直接影响 + 必要消费路径，一次构建后执行选中的测试
python3 scripts/verification/test.py run

# 已提交分支：显式提供比较基线，同时纳入本地修改
python3 scripts/verification/test.py run --base origin/main

# 需要额外验证时只追加，不替换自动选择的测试
python3 scripts/verification/test.py run --include-test 'tests/AtomUI.Desktop.Controls.Tests/ScrollViewer/**/*.cs'
```

`origin/main` 是基线参数示例，使用实际目标分支。`--base` 使用与 HEAD 的 merge-base；默认 HEAD 只比较工作区，
工作区干净时报告 `no-changes`，不会声称已验证整个分支。所有运行认证**当前工作树**；暂存版本与工作树不同的文件
单独列出，不能用这份报告声称暂存快照已经验证。

`plan --path <path>` 可以反复传入路径试算，输出明确标记为 preview，不能交给 runner 当成当前改动的验证证据。
`--json` 输出全部测试类、源文件、命中理由、编译项目、覆盖缺口和专项义务。

## 选择依据

策略位于 `scripts/verification/test-policy.json`；实现位于 `scripts/verification/affected/`。

1. **所有权**：维护生产源码根与测试源文件模式，按控件家族或基础设施子域分组。基础控件与 Desktop 同一家族
   可以共享所有权，测试项目不必与生产项目同名。
2. **消费关系**：C# 类型、扩展方法与 AXAML 资源声明提供正向影响线索。项目依赖方向限制运行时消费关系，
   向外传播以实际改动文件的新旧声明为起点，保留直接控件家族的测试；消费者仅由实际引用文件的声明选择，
   不连带同目录中无关类型。直接消费者进入验证范围，继承关系继续传播。组合控件的整个公开类型不会无限传播，否则一个 Button 容易把所有
   窗口及窗口内所有控件重新带入全量。
3. **源码读取测试**：测试代码引用的输入文件名建立独立索引，不受 `ProjectReference` 限制。它只增加选择，
   不用于排除测试；同名文件可能保守地多选。文件读取关系优先于普通文档忽略规则。
4. **显式共享规则**：主题扫描、Popup 清单、Catalog/XLIFF、Gallery 结构、共享生成源码、图片加载、资源生命周期、
   没有项目引用的本地化集成测试等，使用明确的附加规则。目录扫描和动态依赖不能仅靠符号匹配推断。
5. **保守退回**：直接改动没有精确测试归属时运行完整所有者测试组并报告原因。没有对应测试的间接消费者纳入编译，
   在报告中明确标记为只有编译证据。无法找到所有者、规则引用不存在的 suite 时，报告 gap 并拒绝运行成功。

符号分析是保守的静态辅助，不是完整 C#/XAML 语义分析器，也不保证发现任意反射、配置拼接、资源 key 计算或
外部程序的依赖。新增这类路径必须增加显式策略，评审时解释其消费边界。发布/显式全量基线发现选测遗漏后，应先把
遗漏关系加入策略和工具回归测试，再修复产品问题，避免相同路径再次漏选。

测试类从真实 C# 声明发现，支持 partial、嵌套类、块命名空间和组合 Fact/Theory 属性，不从文件名猜类名。
测试 helper、全局初始化、AssemblyInfo、snapshot、测试资源等发生变化时，扩大到所属测试项目。

## 测试与构建成本

runner 用临时 solution 汇总选中项目，一次构建共享依赖；构建保留各项目声明的 TFM，不能把 netstandard 生成器
强制构建成 net10。随后测试使用 net10 与 `--no-build --no-restore`，并在 runsettings 中保存过滤器，避免命令行
长度限制。Restore 交给增量构建判断，不通过永久 `--no-restore` 隐藏新依赖。

构建与测试串行持有当前 checkout 的 OS 文件锁。保持原有 UI 程序集的串行测试约束，不通过关闭隔离约束提速。
该锁只协调统一入口；外部直接 `dotnet build` 不遵守锁，所以执行器同时检查 build 后与测试结束时的产物指纹。

无新改动时可以复用成功 receipt，但必须同时匹配：

- HEAD、暂存差异、所有 Git 管理/未忽略输入的当前内容；删除和新文件也参与。
- 完整测试计划、配置、SDK 信息、Python、平台和环境变量指纹。
- 选中构建图与测试运行依赖的产物内容，包括生成器与其他 TFM；产物被删除、替换或重建后重新验证。
  集成测试新建的独立 fixture 产物不属于被锁定的测试依赖，允许正常生成。

环境变量只保存摘要，不写入报告。`--fresh` 禁用 receipt 复用；CI 始终使用 fresh。
规划过程中、规划后、验证过程中输入变化都不能认证成功；缓存复用时也重新检查输入。

## 结果契约

报告位于忽略的 `.artifacts/verification/latest.json`；`receipt.json` 保存最近一份可复用成功证据，
`history.json` 最多保留 100 条耗时记录，CLI 的总耗时包含仓库发现、审计和影响规划。`metrics` 命令显示历史。日志、临时 solution、runsettings、TRX 全部放在
系统临时目录，正常退出、测试失败和可处理的中断后清理；强制杀进程后遵循系统临时目录清理策略。

| 状态 | 含义 | 退出码 |
| --- | --- | --- |
| `passed` | 本次计划内检查完成，不代表整个仓库已验证 | 0 |
| `reused` | 相同输入/配置/产物匹配已有通过证据，保留原验证时间 | 0 |
| `iteration-passed` | 开发阶段直接验证通过，仍需执行 change 范围 | 0 |
| `tests-passed` | 显式 `--tests-only` 的测试阶段通过，专项义务仍单独列出 | 0 |
| `no-changes` | 当前比较范围没有改动，没有执行测试 | 0 |
| `pending` | 测试通过，但仍有专项验证义务 | 2 |
| `blocked` | 影响规则有缺口或指向不存在的 suite | 2 |
| `failed` | 构建、测试、证据或快照一致性失败 | 1 |

测试进程退出 0 不足以成功：必须有 TRX，选中测试数大于零，每个选中类有实际通过的结果，计数与逐项结果一致，
不存在失败、跳过或未执行结果。默认不把跳过的测试折算为通过。新增平台 skip 必须明确调整验证范围或提供对应
平台证据，不能通过忽略返回码解决。

普通文档可以得到不运行 .NET 测试的计划；被测试或生成器消费的文档先按输入关系选择验证。
每次收尾仍执行 `git diff --check`，并保留原始 UX/手动验证要求。

## 专项义务

发布注册、生成器、共享编译源码、产品项目配置、native 能力、打包资产等会产生 `obligations`。这些义务不因为
单元测试通过而消失，runner 返回 pending；单元测试证据可以复用，但不代表专项已完成。执行报告指定的真实 publish、package 或平台
验证，按 [AOT 编程规范](../development/aot-programming-guidelines.md)、
[Gallery 发布流程](gallery-aot-release-workflow.md) 和受影响平台文档汇报独立证据。

`check --id <计划中的义务 ID> -- <验证命令及参数>` 只执行与策略中该义务的 `verifier` 数组**完全相同**的命令，
记录当前输入/环境、完整命令、退出状态、耗时和输出摘要。多步骤义务必须先实现并审查覆盖全部步骤的验证脚本，
再将其完整 argv 注册到策略；不能用 quick 命令、单独 publish、启动进程或说明文字冒充完整验证。
当前发布/平台义务跨越多个现有手动流程，策略没有为它们预注册未经验证的统一 verifier；默认保留 pending，
按现有专项流程提供独立证据。工具拒绝未注册命令，不提供临时绕过。注册 verifier 的策略修改本身也需评审和验证。
随后再次执行同一 `run`，已通过的单测可以复用，当前输入下通过的专项证据附在报告中；失败或输入改变后的旧证据
不能满足义务。CI 需要专项检查时，同样在 `run` 前调用对应 `check`，使用已经验证的发布/平台脚本。

工具不提供 `--ignore-obligations` 或仅凭文字声明生成成功 receipt 的开关。NativeAOT publish 也不等于 UI 行为验证；
hover、滚动、裁剪、资源释放等仍保留原始复现条件和交互/生命周期检查。

依赖与 SDK 变更需要显式全量及目标平台/TFM 验证；常规执行不会为了扩大范围自动运行全量。

## 全量、CI 和维护

```bash
# 用户明确要求或版本发布时使用，发现全部测试项目
python3 scripts/verification/test.py run --scope full --fresh

# 维护影响规则时的快速检查
python3 scripts/verification/test.py audit
python3 scripts/verification/test.py self-test
python3 scripts/verification/test.py metrics
```

`.github/workflows/verify-affected.yml` 在 PR 上按基线选择测试，保存短期紧凑报告；手动 workflow 可以显式选择全量。
CI 没有定期全量任务，不改仓库 branch protection；是否要求该 job 通过由仓库管理员配置。
该 job 明确命名为 `Selected tests and impact report`，使用 `--tests-only`，成功只表示选中的测试阶段通过；
专项义务仍保留在 JSON 中，必须由发布/平台检查单独覆盖。这个 job 不承担发布可用性 gate，不能把它的绿色结果
当成 NativeAOT、package 或平台行为已经验证。普通本地 `run` 不带此参数，专项未满足时仍返回 pending/退出码 2。

新增测试项目必须有生产路径映射；新增生产根必须有所有者或显式规则。audit 校验全仓库映射与测试项目发现，
不会因为当前 PR 没有触碰遗漏路径就放过新缺口。修改选择器时运行真实临时 Git 仓库和进程边界测试；测试必须
证明漏选、旧产物、空结果或错误输入会被识别，不能只断言配置文件包含某行文字。

收尾报告说明实际范围、命令、通过数量、耗时、复用情况、只有编译证据的消费者和未完成专项。若精确选择频繁退回
整个 owner，先完善对应功能域与共享规则，再考虑拆分测试项目；不要削弱回归测试本身来降低耗时。
