# 同名程序集姊妹包行为委托修复交付记录（`#338`）

- **工作树 / 分支**：`D:\mod\工作2\上游修复_姊妹包行为委托\STS2-SkinChanger_修改件`（分支 `fix/twin-assembly-behavior-20261123`，基线 `7fd616c`）
- **边界确认**：仅改动 `Core/ManagedSkinModLoader.cs` 及对应测试/清单/维护文档；未触碰 `ContextualSkinControls.ApplyProviderVariantLabels`、E 项运行包缓存文件及机制 1 的 `DeferredDispatch`；未执行 `git commit`、部署或启动游戏。

---

## 1. 根因复盘

两款不同清单 ID 的姊妹包（例如 `JoanofArcIroncladHpwear111` 与 `JoanofArcIroncladHpwear`）共享相同的程序集简单名（`JoanofArcIroncladHpwear`）、culture 与公钥令牌，但 DLL 内容不同：

1. 先选包 A 加载并激活其行为程序集后，切回原版时 `DeactivateProvidersExcept` 将 A 的行为补丁全部卸载（蓝图与 `InvokedProviderInitializers` 仍保留）。
2. 再选包 B 时，`GetOrLoadProviderAssembly` 命中已占用的同名程序集，但原 `TryReuseSiblingBehavior` 只认 `NormalizeProviderIdentity` 相同的同 ID 包（`::source:*`），对不同 ID 的姊妹包直接返回 `false` 并落入 `ReportProviderAssemblyIdentityConflict`，导致 B 只接管资源、A 的行为补丁未重新挂回（例如贞德战损无 `wear timer attached`）。
3. 此外，源码核查发现 3 处配套缺口，不一起补齐即便建了别名也会在实际游戏里失效：
   - **`DeactivateProvidersExcept` 未解析委托目标**：直接切 A ↔ B 或 B 选中期间再次刷新时，若只把 `NormalizeProviderIdentity(selectedId)` 放进保留集，会把代理执行者 A 误当成未选中卸掉。
   - **`IsProviderAssemblyFor` 与重放入口未解析别名**：`SkinService.IsCharacterBehaviorSelected`（供 `ManagedSkinBehaviorBridge` / 战损计时器归属门控、`CrusaderHookBridge`、`ManagedSlotVisibilityBridge`、`ManualCharacterVariantBridge`、`ManagedMerchantSettingsBridge`、`ProviderModeSelectOverlayBridge` 使用）以及 `ReplaySelectedCreatureReady` / `ReplaySelectedNodeReady*` / `RestoreUnselectedNodeReadyBehaviors` 直接查原 `providerId`，在 B 选中而由 A 的程序集执行时会误判为未选中或找不到运行时。
   - **注册期同 `FullName` 冲突且先选第二包**：当两包连 `Version` 都相同但字节不同时，后注册的包在登记期会被标 `CanActivateBehavior = false`；若玩家先选后注册的包，原 `GetOrLoadProviderAssembly` 在 `occupied == null` 时直接返回 `null`。

---

## 2. 修复内容（`STS2SkinChanger/Core/ManagedSkinModLoader.cs`）

全部按通用规则实现，无任何包名或 `"111"` 后缀特判：

1. **扩大姊妹包判定（`TryReuseSiblingBehavior` + `OwnsLoadedAssembly`）**：
   - 条件：程序集简单名一致（不区分大小写）、`CultureName` 一致、`PublicKeyToken` 逐字节一致，且已加载程序集确实归属某个已登记在 `ProviderAssemblies` 中的不同提供者（按 `LoadedProviderAssemblies` 引用匹配或非空 `Location` 匹配）。
   - 优先级：同 `NormalizeProviderIdentity` 逻辑 ID 的姊妹包优先（保持原有同 ID 路径零回退），其次匹配任意已登记的同名程序集提供者；对目标递归取 `ResolveBehaviorProviderId` 避免链式别名。
   - 写入 `BehaviorProviderAliases[requested.ProviderId] = targetId`；资源仍来自所选包，不把冲突 DLL 记入 `LoadedProviderAssemblies`。
   - 日志：同 ID 保留原句；不同 ID 同名姊妹包首次建委托时打一行 `ModLog.Info`：`同名程序集姊妹包 <B> 的行为改由 <A> 代为执行（程序集 <Name>），资源继续来自所选包。`；若已加载程序集不属于任何已登记提供者或 culture/公钥令牌不匹配，保留原有 `ReportProviderAssemblyIdentityConflict` 警告并维持只接管资源。
   - 同步补齐 `GetOrLoadProviderAssembly` 内 `LoadedProviderAssembliesByIdentity` 与 `AppDomain` 冲突分支，在报冲突前均先尝试 `TryReuseSiblingBehavior`；当 `!provider.CanActivateBehavior` 且 `occupied == null` 时，若 `ProviderAssemblySourcesByIdentity` 已登记同标识主提供者，先加载主提供者并建立委托。
2. **停用保留委托目标（`DeactivateProvidersExcept`）**：
   - 计算 `selected` 保留集时，对每个选中项同时加入 `NormalizeProviderIdentity(providerId)` 与 `NormalizeProviderIdentity(ResolveBehaviorProviderId(providerId))`，保证已选 B 时不误卸代理执行者 A。
3. **重选经蓝图重激活且不重复初始化（`ActivateSelectedProviders` + `ActivateProvider`）**：
   - 当 A 此前已被停用、`ActiveProviderRuntimes` 不含 A 时，`IsBehaviorActiveFor(B)` 为 `false`，不会跳过；`ActivateProvider(B)` 经 `GetOrLoadProviderAssembly` 建立或复用别名后命中 `behaviorId != providerId`，若 `!ActiveProviderRuntimes.ContainsKey(behaviorId)` 则调用 `ActivateProvider(behaviorId, behaviorProvider)`，命中 B 项蓝图重复激活路径把补丁重新挂上，且 `InvokedProviderInitializers` 阻止初始化器二次执行。
   - 在 `ActivateSelectedProviders` 末尾（`FindRicherSiblingAssemblyPath` 之后）加 `if (!IsBehaviorActiveFor(providerId))` 防护，避免已通过更富姊妹包激活后重复激活一遍。
4. **归属门控与场景重放全面感知别名**：
   - `IsProviderAssemblyFor(providerId, assembly)` 同时匹配 `providerId` 与 `ResolveBehaviorProviderId(providerId)`，确保 `SkinService.IsCharacterBehaviorSelected` 与 `ManagedSkinBehaviorBridge` 在选中委托包 B 时放行 A 程序集的行为回调（如血量换装计时器）。
   - `GetSelectedCharacterSkinPathMethods`、`ReplaySelectedCreatureReady`、`ReplaySelectedNodeReadyBehavior`、`ReplaySelectedNodeReadyPrefixes`、`ReplaySelectedNodeReadyPostfixes`、`RestoreSelectedNodeReadyBehavior`、`ReplaySelectedCompanionReveal`、`BeginSelectedNodeReadyTracking`、`EndSelectedNodeReadyTracking` 与 `RestoreUnselectedNodeReadyBehaviors` 均经 `ResolveBehaviorProviderId` 解析运行时。

---

## 3. 改动文件清单与既有测试变更说明

| 文件路径 | 变更类型 | 说明 |
| --- | --- | --- |
| `STS2SkinChanger/Core/ManagedSkinModLoader.cs` | 修改 | 上述 4 点修复（同名姊妹包委托、停用保留、重选蓝图重激活、归属与重放别名解析） |
| `tests/STS2SkinChanger.RuntimeTests/TwinAssemblyBehaviorDelegationTests.cs` | 新增 | 使用 `.NET 9` `PersistedAssemblyBuilder` 生成真实同简单名、不同字节、带 `[ModInitializer]` 与 Harmony Postfix 的磁盘 DLL，端到端实测 `A -> 原版 -> B`、`B -> 原版 -> A`（含不同版本与同 `FullName` 注册冲突两种情况）、`A <-> B` 直接互切、`A -> 别的角色皮肤 C -> B`、同 ID 姊妹包非回归、`IsProviderAssemblyFor` 门控放行、初始化器严格只跑一次、以及未登记宿主冲突保持警告与只接管资源 |
| `tests/STS2SkinChanger.RuntimeTests/ProviderAssemblyReuseTests.cs` | 修改 | **既有断言更新说明**：原 `1.0.36.101` 测试中 `unrelated` 用例构造了与已登记 `original` 同简单名/同 culture/同公钥令牌的不同 ID 包，并断言其不能委托（对应旧的「仅同逻辑 ID 复用」窄规则）。按 `#338` 新规格，该用例已更新为验证不同 ID 姊妹包（`twinSibling`）委托行为且不独立加载 DLL，同时**新增 2 条更严格的负例断言**：① 同简单名但 `CultureName` 不同（`en-US`）必须保持只接管资源；② 与未登记任何提供者的已加载程序集同名冲突必须保持只接管资源 |
| `tests/STS2SkinChanger.RuntimeTests/Program.cs` | 修改 | 注册 `--test-twin-assembly-behavior` 与全量 `Gate.Run("TwinAssemblyBehaviorDelegationTests", ...)` |
| `tests/required-fixes.json` | 修改 | 新增 `twin-assembly-behavior-delegation`（清单总数 37 -> 38） |
| `docs/agents/maintenance-rules.md` | 修改 | 「不许丢的修复」补充同名程序集姊妹包行为委托条目 |
| `docs/refactor/twin-assembly-behavior-gemini.md` | 新增 | 本交接文档 |

---

## 4. 双分支构建与门禁结果（产物隔离至 `%TEMP%\sts2-gates-gemini-twin`）

| 检查项 | Stable (`Release`, v0.111.1) | Beta (`ReleaseBeta`, v0.113.1) | 备注 |
| --- | --- | --- | --- |
| `required-fixes.json` Logic 类自检 | PASS（38 项） | PASS（38 项） | — |
| 4 工程构建（Compat / Mod / LogicTests / RuntimeTests） | PASS（0 警告 / 0 错误） | PASS（0 警告 / 0 错误） | — |
| `LogicTests` | PASS | PASS | — |
| `RuntimeTests --test-provider-assembly-reuse` | PASS | PASS | 同 ID + 姊妹包委托 + 文化/未登记负例全过 |
| `RuntimeTests --test-twin-assembly-behavior` | PASS | PASS | 全部正反方向与负例端到端通过 |
| `RuntimeTests` 无参全量 | PASS | PASS | `total=38, PASS=36, FAIL=0, NOT RUN=0, SEPARATE=2, GAP=0`，尾行 `Workshop policy tests passed.` |
| 全部 5 个无参 `SEPARATE` 项 + `--test-runtime-safety` / `--test-preview-binding` | PASS | PASS | 逐项通过 |
| `run-gates.ps1` / `check-protected-characters.ps1`（贞德实包） | 交由 Claude 复跑 | 交由 Claude 复跑 | 按本机红线不绕过 PS 策略且不加载第三方 Mod DLL |

---

## 5. 给云的实机验证步骤

部署后在同时装有 `JoanofArcIroncladHpwear111`（sfw）与 `JoanofArcIroncladHpwear`（nsfw）的环境下验证以下 4 条链路：

1. **原复现路径（111 → 原版 → nsfw）**：
   - 开局铁甲选 `111`（sfw）→ 换回原版铁甲 → 换 `nsfw` 贞德 → 进战斗掉血跨档。
   - **预期**：日志出现 `同名程序集姊妹包 JoanofArcIroncladHpwear 的行为改由 JoanofArcIroncladHpwear111 代为执行` 与蓝图重激活日志，不重复执行初始化器；进战斗后出现 `wear timer attached`，掉血正常切换战损档位，且画面资源仍来自 nsfw 包。
2. **反方向（nsfw → 原版 → 111）**：
   - 重启游戏后直接选 `nsfw` → 换原版 → 换 `111` → 进战斗/选人界面确认行为与调色板变体正常。
3. **直接互切（111 ↔ nsfw）**：
   - 在选人界面或局外直接 `111 -> nsfw -> 111` 来回切换，确认日志中代理执行者保持在位不反复卸载、不重复初始化，战损与变体均正常。
4. **跨皮肤切回（111 → 其他铁甲皮肤 → nsfw）**：
   - 切到其他铁甲皮肤（如十字军）确认 `111` 正常停用，再切到 `nsfw` 确认行为自动恢复且进战斗掉血换装正常。

---

## 6. 返工补强（`#344`：不同逻辑 ID 同名程序集的 PE 元数据类型集合重合度防线）

### 6.1 为什么必须加类型重合判定

工坊里不少皮肤 Mod 由同一份模板项目改出，可能共享完全相同的程序集简单名（例如 `SilentSkinMod`）、`CultureName` 与 `PublicKeyToken`，但内部代码与逻辑完全无关。若不同逻辑 ID 仅凭简单名 + culture + 公钥令牌就建立委托，选中 B 时会错误执行 A 的行为补丁，比「只接管资源」风险更高。

### 6.2 判定规则、阈值与缓存（`ManagedSkinModLoader.cs`）

- **作用范围**：**仅对不同逻辑 ID（`NormalizeProviderIdentity` 不同）分支生效**；同逻辑 ID（`::source:*`）原有复用路径不加此限制，保持完全不变。
- **只读 PE 元数据提取（`ReadAuthoredTypeFullNames`）**：使用 `PEReader` + `MetadataReader` 读取两份 DLL 的 `TypeDefinitions` 全名集合（含嵌套类型 `Outer+Inner`），**不加载、不执行任何程序集**；自动排除 `<Module>`、`<PrivateImplementationDetails>`、`<>c*` 等 `<` 开头编译器生成类型，以及 `Microsoft.CodeAnalysis.*` / `System.Runtime.CompilerServices.*` 编译器注入属性，避免小型无关 Mod 因共用 Roslyn 样板类型被虚高拉升重合度，也避免真实姊妹包因匿名闭包编号漂移被压低分数。
- **阈值常量**：`TwinAssemblyTypeOverlapThreshold = 0.8`（Jaccard 系数 `|A ∩ B| / |A ∪ B| >= 0.80`；任一侧无法读元数据或有效类型集合为空时返回 `0.0`，失败闭环拒绝委托）。
  - **实测依据（只读静态 `PEReader` 对照真实贞德两包）**：`JoanofArcIroncladHpwear111.dll` vs `JoanofArcIroncladHpwear.dll` 去除编译器噪声后有效作者类型为 `51` vs `56`，交集 `51` / 并集 `56`，**Jaccard = `0.9107`**（全量含编译器类型也为 `79 / 89 = 0.8876`），留有充分安全裕度；而只共享 `ModEntry` 等 1–2 个模板类型的无关 Mod 重合度通常 `< 0.25`。
- **双文件戳缓存**：`TwinAssemblyTypeOverlapCache` 以规范排序后的两份 DLL `(FullPath, Length, LastWriteTimeUtc.Ticks)` 为键缓存计算结果（上限 256 条），切皮肤时不重复读盘或解析 PE 元数据；在 `!CanActivateBehavior && occupied == null` 注册冲突路径也先做纯元数据校验（`IsEligibleSiblingByMetadata`），不达标时甚至不会加载主包 DLL。
- **日志**：委托成立时输出 `同名程序集姊妹包 <B> 的行为改由 <A> 代为执行（程序集 <Name>，类型重合 0.xx），资源继续来自所选包。`；未达标时拒绝委托并沿用原有 `ReportProviderAssemblyIdentityConflict` 冲突警告。

### 6.3 恢复并加强的负例测试

- `ProviderAssemblyReuseTests.cs`：**恢复** `unrelated` 反例断言（同简单名、同 culture、同公钥令牌，只共享 `SharedSkin.ModEntry`、其余类型完全不同，重合度 `1/9 = 0.111 < 0.80`，断言保持只接管资源、`ResolveBehaviorProviderId(unrelated) == unrelated`），同时保留共享 `5/6` 类型的 `twinSibling` 正例以及 `differentCulture` / `unregisteredCollision` 负例。
- `TwinAssemblyBehaviorDelegationTests.cs`：所有姊妹包测试 DLL 均构造真实共享类型（`5/6 = 0.83 >= 0.80`）并断言委托日志包含 `类型重合 0.83`；新增端到端用例 `VerifyTemplateCloneDifferentTypesDoesNotDelegate`（同简单名模板克隆、仅共用 `Entry`、`1/9 = 0.11 < 0.80`，断言不委托、不执行 A 补丁、保留原冲突警告）。

---

## 7. 返工补强（`#348`：统一在加载入口优先加载更全姊妹包 + 禁止完整版反向委托给精简版）

### 7.1 实机根因（云 11-24 17:48 场日志）

先选 `JoanofArcIroncladHpwear111`（v0.3.5.0，51 个作者类型，精简版）时，`SkinService` 在进入 `ActivateSelectedProviders` 之前会先调用 `EnsureProviderGodotScripts("JoanofArcIroncladHpwear111")`（以及检视/设置/预热等路径），这些入口统一直接走 `GetOrLoadProviderAssembly`，导致精简版 `111` 抢先占住了程序集名 `JoanofArcIroncladHpwear`。随后 `ActivateSelectedProviders` 按原有规则尝试改由更全的 `JoanofArcIroncladHpwear`（v0.3.16，56 个作者类型）执行时，因程序集名已被占，反而触发了「完整版 → 精简版」的反向委托，实际一直跑 0.3.5 精简版代码，战斗掉血无法触发 `combat: stage 1->2`。

### 7.2 修复方案（`ManagedSkinModLoader.cs`）

已全仓审计所有托管皮肤提供者加载入口（`EnsureProviderGodotScripts`、`TryGetProviderAssemblyForInspection`、`GetStatefulCardArtAssembly`、`EnsureProviderSettings`、`DiscoverVisualPostfixes`、预热及 `ActivateProvider`），它们**100% 汇流至单一入口 `GetOrLoadProviderAssembly`**。本轮将「更全优先 + 单向委托」直接收敛在该统一入口内：

1. **统一「更全优先」加载决策（`TryFindRicherTwinSibling`）**：
   - 在 `GetOrLoadProviderAssembly` 中，当目标简单名尚未被加载（`occupied == null`）时，扫描已登记提供者中**不同逻辑 ID**、简单名/culture/公钥令牌一致、类型重合度 `>= 0.80` 且**作者类型数严格更多**（`GetAuthoredTypeCount(candidate) > GetAuthoredTypeCount(requested)`，口径与类型重合判定完全一致，且经 `ProviderAssemblyMetadataCache` 按文件戳缓存）的姊妹包，取作者类型数最多者优先加载（即使该更全包在注册期因后扫描同 `FullName` 被标过 `CanActivateBehavior = false`，因程序集名尚空闲也允许直接加载该更全包）。
   - 加载完成后，把当前请求的精简包写入 `BehaviorProviderAliases` 委托给更全包，输出明确日志（含双方类型数与重合度），所选包资源保持不变。
2. **严格单向委托（禁止「完整版 → 精简版」反向委托）**：
   - 在 `TryReuseSiblingBehavior` 的不同逻辑 ID 分支中，除重合度 `>= 0.80` 外，额外要求 `GetAuthoredTypeCount(requested) <= GetAuthoredTypeCount(source)`（只允许精简版或同完整度包委托给已加载包）。
   - 若某条遗漏/外部路径已抢先加载了精简版而当前请求的是更全版（`requestedTypeCount > sourceTypeCount`），**拒绝反向委托**、不写入 `BehaviorProviderAliases`、返回 `null` 保持仅接管资源，并去重输出一条明确 `ModLog.Warn` 说明程序集已被精简版提前占用、行为可能不完整。
3. **同逻辑 ID 与类型重合判定不变**：同逻辑 ID（`::source:*`）原有路径仍不施加类型重合与方向限制；模板克隆（重合度 `< 0.80`）仍拒绝委托并保留原冲突警告。

### 7.3 测试覆盖更新

- `TwinAssemblyBehaviorDelegationTests.cs`：
  - `VerifySlimmerFirstAlwaysLoadsFullerAndDelegates`：A（5 类型精简版 v0.3.5.0）与 B（6 类型完整版 v0.3.16.0，重合 0.83）同名，先经提前加载入口 `TryGetProviderAssemblyForInspection(A)` 再走 `ActivateSelectedProviders([A])`，断言实际加载的是 B 的字节、A 未被加载且委托给 B、仅 B 初始化一次且执行 B 的补丁（200）；继续验证 `A -> 原版 -> B`、`B -> 原版 -> A`、`A <-> B`、`A -> 别的角色皮肤 C -> B` 全程只有 B 在跑且初始化器只跑一次。
  - `VerifyFullerFirstDirectionPreserved`：先选 B → 原版 → A 保持原样。
  - `VerifyRegistrationConflictStillPrefersFullerBuild`：同 `FullName` 下精简版先登记、完整版后登记（`CanActivateBehavior = false`），先选精简版 A 仍优先加载完整版 B。
  - `VerifyReverseDelegationFullerToSlimmerForbidden`：强制把精简版 A 先装进加载上下文后再选完整版 B，断言拒绝反向委托、`ResolveBehaviorProviderId(B) == B`、不运行 A 的补丁并打出 `拒绝反向委托` + `行为可能不完整` 警告。
- `ProviderAssemblyReuseTests.cs`：补充 `fullerTwin`（7 类型 > 已加载 6 类型）反向委托被拒断言，保留 `slimmerTwin`（5 类型 <= 6 类型）正向委托、同 ID 与 `unrelated` / `differentCulture` / `unregisteredCollision` 负例。

### 7.4 验证结果（`#348` 返工）

- 双分支构建（Stable `Release` / Beta `ReleaseBeta`，输出隔离到 `%TEMP%\sts2-gates-gemini-twin`）：4 工程 0 警告 0 错误。
- 双分支测试全绿：`LogicTests`、`--test-provider-assembly-reuse`、`--test-twin-assembly-behavior`、无参 `RuntimeTests`（`total=38, PASS=36, FAIL=0, NOT RUN=0, SEPARATE=2, GAP=0`，尾行 `Workshop policy tests passed.`）、5 项无参 `SEPARATE`、`--test-runtime-safety`、`--test-preview-binding` 全部通过。
- 涉及 7 个修改/新增文件均已校验为纯 LF、无 BOM；`run-gates.ps1` 与真包受保护角色门禁留交 Claude 合并审查时统一复跑。
