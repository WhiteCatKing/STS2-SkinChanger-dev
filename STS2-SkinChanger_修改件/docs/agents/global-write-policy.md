# 全局资源写入规则（优化阶段 3a）

> 给维护本仓库的 AI 和人看。切换器往 Godot 全局资源系统写东西（挂资源包、`TakeOverPath`、改 `PreloadManager` 缓存）会影响整局游戏，而且挂上的包**卸不下来**。这份文件定义：哪些路径永远不许写、哪些路径在什么条件下可以写、写了怎么恢复。所有写入都要经过同一个入口并记账。

## 1. 禁止向全局发布非原版内容的路径

F1–F3 只包含当前运行分支的游戏本体 PCK 索引中实际存在的条目。路径类型用于从本体索引挑选根条目，其导入描述、重定向和递归依赖也必须在本体索引里存在。Mod 角色、私有子目录和仅名字相似的路径不会因此进入保护集。例如 `status/beta/wither.png.import` 只有在该版本原包确实索引它时才算 F2。

| 编号 | 路径范围 | 判定函数 | 为什么 | 出过的事 |
|---|---|---|---|---|
| F1 | 游戏共享图集页（`card_atlas_N`、`relic_atlas`、`epoch_atlas`、`compressed_atlas`、`ui_atlas` 等）及其 `.import` / `.remap` / `.godot/imported` 产物 | `GlobalWriteProtection.Decide` | 一页图集同时画几十上百张无关的卡 / 图标，覆盖一页，选"游戏默认"也会显示别的包的图 | 1.0.36.105 愚者卡包 |
| F2 | 原版卡图规范路径 `res://images/packed/card_portraits/**` 及其导入产物 | `GlobalWriteProtection.Decide` | 卡图一律走私有别名（`CardBinaryResource` 改写依赖），不再写规范路径 | 1.0.36.105 卡图隔离 |
| F3 | 游戏选人头像规范入口及其依赖 | `GlobalWriteProtection.Decide` | 选人头像按当前选择私有提供；全局写入会让未选中的包占住头像 | 1.0.36.105 选人头像、响木天音 |

F4 及以后：盘点时发现的其他"多处共用、写了就污染无关画面"的路径，由 Claude 审定后补进来。

## 2. 允许写入的场景（例外表）

规范路径默认不写。下表每一行都要写清楚：哪个入口、写哪些路径、什么时候写、取消选择后原版怎么回来、哪个真实包用例守着。**没登记的写入就是违规。**

| 编号 | 入口（函数） | 写入方式 | 路径范围 | 触发时机 | 恢复办法 | 守护用例 |
|---|---|---|---|---|---|---|
| A01 | `SkinService.EnsureIsolatedCardOverlay` | PCK true | 不涉及规范路径。会话卡图别名及其私有载荷 | 初次请求或增补缺图 | 切换选择后改读对应会话别名，原版走原路径 | FoolCardPackIsolationTests |
| A02 | `SkinService.BeginRuntimeResourceScope` | PCK true | 场景及其私有别名，部分包有规范运行依赖 | 联机场景或独立运行资源作用域打开 | 作用域关闭后恢复本机选择，再按组回填基线 | AppearanceQueueTests（缓存模拟），真实写入用例缺 |
| A03 | `SkinService.GetEpochPortraitOverride` | PCK true | 不涉及规范路径。时间线插图私有路径 | 读取替换时间线插图 | 默认选择走原版入口 | 缺 |
| A04 | `SkinService.GetBaselineRelicIcon` | PCK true | 不涉及规范路径。原版遗物图集私有路径 | 读取原版遗物切片 | 默认图读同一原版私有别名 | 缺 |
| A05 | `SkinService.LoadSelectedRelicBundle` | PCK true | 不涉及规范路径。遗物大图、切片与图集的私有别名 | 选择遗物包时 | 默认重新解析原版切片 | 缺 |
| A06 | `SkinService.WithPreparedRuntimeResources` | PCK true | 指定场景和资产的别名，部分包有规范运行依赖 | 加载所选角色、怪物或房间资源 | 按组恢复原版资源与规范缓存归属 | ProtectedCharacterTests |
| A07 | `SkinService.MountContextualFullRuntimeProviderPacks` | PCK true | 过滤后的完整提供者包，含所选场景、骨架、效果规范路径及原作者私有路径 | 联机临时取不同的完整包 | 作用域关闭时重挂本机选择并校正基线 | 缺 |
| A08 | `SkinService.EnsureScopedRuntimeProviderResourcesMounted` | PCK false | 过滤后的完整依赖包，包含规范场景、效果或音效，以及提供者私有路径 | 激活受限怪物或角色包 | 只填缺失条目，游戏/当前覆盖保持优先权 | 缺 |
| A09 | `SkinService.MountSelectedLargeRuntimeProviderPacks（仅随从）` | PCK false | 过滤后的提供者完整包，用于私有依赖。规范项仅可填缺 | 选择仅依赖随从的完整包 | 已有原版条目优先，随从改读原版 | 缺 |
| A10 | `SkinService.MountSelectedLargeRuntimeProviderPacks（提权）` | PCK true | 过滤后的整包，含规范场景、骨架、特效及其他原版同名项 | 选中有整包依赖的外观 | 校正层回填未选择的路径和原版资源 | LargeProviderOverlayTests，Hibiki 实包审计 |
| A11 | `SkinService.MountArchiveOverlay` | PCK true | 增量层的场景、图像、骨架、文本或原版恢复资源；途径有私有别名也有规范路径 | 启动、选择变化、修正或退出联机作用域 | 重挂当前选择或逐路径回填本体，已有层留在栈中 | ProtectedCharacterTests、FoolCardPackIsolationTests（生成内容） |
| A12 | `SkinService.EnsureGameSceneOnTop（挂包）` | PCK true | 目标原版场景与存在的 .remap | 重建原版外观时 | 写入本身是恢复，随后刷新两层缓存 | 缺 |
| A13 | `SkinService.LoadGameSceneViaAlias` | PCK true | 不涉及规范路径。__skin_changer_base 下的原版场景别名 | 绕开被替换的规范场景 | 继续用原版私有别名或按选择读回规范入口 | 缺 |
| A14 | `SkinService.InstantiateGameCreatureVisuals` | PCK true | 不涉及规范路径。__skin_changer_base 下的原版生物场景别名 | 直接实例化原版生物外观 | 读原版场景别名，原版规范入口维持现状 | 缺 |
| A15 | `BackgroundSkinService.CreatePackBackground` | PCK false | 未另过滤的完整背景提供者包，可含规范路径。已有路径维持优先权 | 创建包提供的战斗背景 | 默认走原版路径，之前补入的条目留在栈中 | 缺 |
| A16 | `SkinService.TakeOverRuntimeCanonicalPath` | Godot TakeOverPath | 当前运行场景及运行依赖的规范路径 | 绑定外观资源至标准查找入口 | 重绑本体或本机选择， displaced 资源恢复别名 | AppearanceQueueTests（缓存模拟），真实写入用例缺 |
| A17 | `SkinService.RestoreRuntimeResourceAlias` | Godot TakeOverPath | 不涉及规范路径。原私有别名或 sts2_skin_runtime/retained | 规范路径让给另一实例或恢复原版时 | 保留仍被持有的实例在原别名或新别名下 | AppearanceQueueTests（缓存模拟），真实写入用例缺 |
| A18 | `ContextualSkinControls.TakeOverCharacterSelectSceneCache（Godot）` | Godot TakeOverPath | 选人动态场景的规范入口，和选人静态头像入口分开 | 换选人外观或重建显示时 | 重建后同一入口登记当前选择或原版 | 缺 |
| A19 | `SkinService.RefreshCachedCreatureVisualScenes` | Preload SetAsset | 刚挂载的规范生物场景 | 增量层变更或恢复原版后 | 规范缓存重新读当前包栈顶 | 缺 |
| A20 | `SkinService.EnsureGameSceneOnTop（预载缓存）` | Preload SetAsset | 目标规范场景 | 原版场景被写回并重新加载后 | 缓存写回的是原版 | 缺 |
| A21 | `SkinService.ForceCanonicalSceneFromAlias` | Preload SetAsset | 目标规范场景 | 原版回退时把原版别名结果登记到规范入口 | 写入本身是恢复 | 缺 |
| A22 | `ContextualSkinControls.RebuildRuntimeProviderCharacterDisplay` | Preload SetAsset | 选人动态场景规范入口 | 重建所选角色时，紧跟 A18/A23 | 下次重建登记当前外观或原版 | 缺 |
| A23 | `ContextualSkinControls.TakeOverCharacterSelectSceneCache（预载缓存）` | Preload SetAsset | 选人动态场景规范入口 | 与 A18 同步给游戏预载缓存登记 | 下次重建登记当前外观或原版 | 缺 |
| A24 | `ContextualSkinControls.RebuildMonsterDisplay` | Preload SetAsset | 怪物图鉴生物场景规范入口 | 图鉴选中怪物或更换外观 | 重建时重新读取当前外观或原版 | 缺 |
| A25 | `RemoteRoomSceneLease.Begin` | Preload SetAsset | 联机同伴在商人或火堆使用的场景路径 | 创建远端玩家房间节点前 | 释放时 A26 写回先前对象 | 缺 |
| A26 | `RemoteRoomSceneLease.Dispose` | Preload SetAsset | A25 的同一场景路径 | 离开临时显示作用域 | 写入本身是恢复，原缓存对象有效时执行 | 缺 |
| A27 | `RemoteRoomSceneLease.BeginTexture` | Preload SetAsset | 联机同伴在火堆等房间使用的贴图路径 | 创建远端玩家房间贴图前 | 释放时 A28 写回先前对象 | 缺 |
| A28 | `RemoteRoomSceneLease.TextureLease.Dispose` | Preload SetAsset | A27 的同一贴图路径 | 离开临时贴图作用域 | 写入本身是恢复，原缓存对象有效时执行 | 缺 |
| A29 | `SkinService.EnsureGameSceneOnTop（Godot 缓存）` | ResourceLoader Replace | 原版场景的规范根路径 | A12 成功后强制重读场景 | 写入本身是恢复。外部依赖按 Reuse | 缺 |
| A30 | `AncientCompendium.InstantiateOtherPreviewScene` | ResourceLoader ReplaceDeep | 其它图鉴的商人、怪物等场景及 Godot 可枚举的外部依赖，可能含共享载荷 | 无已识别外观组而预览时 | 后续外观组走独立别名。此入口没有逐条恢复递归缓存 | 缺 |
| A31 | `ExternalCardVisualBridge.PrepareManagedProviderIdentity` | ResourcePath setter | 不涉及规范路径。编辑器辨认卡片的合成路径，每个活视图带实例编号 | 卡片运行中首次把当前提供者贴图传给外部编辑器 | 恢复原节点贴图，视图缓存条目随视图生命周期结束 | 缺 |
| A32 | `PoroVariantBridge.GetFactory` | PCK false | 不涉及规范路径。小红帽骨妹包里随从（魄罗）文件夹、对应导入载荷和工厂脚本（`res://RedHoodNecrobinder/...`），重写成会话缓存 `poro_<hash>.pck` 再挂 | 首次需要魄罗随从外观时 | 只填缺；原版条目和当前覆盖保持优先。D4：只记账 | 缺（1.0.38.13 白猫新增，3a 合入时补登记） |

全局写入点共 32 处。表内 PCK true / false 是传给 Godot 的替换标志。只有明确改变全局映射或强制写缓存的调用计入本表。普通 Reuse 读取、Ignore 私有读取、PCK 文件本身的磁盘生成和仅改变返回值的补丁不计入。

### 可能触及 F1–F3 的入口

- A02 和 A06 共用准备好的运行包。共享路径仅保留与本体逐字节一致的项目。商人场景和贴图保留其私有别名。A16 的缓存写入另按同一个本体索引核验。
- A07 和 A10 的高优先级完整包先剔除非原版保护项。A08 和 A09 的低优先级挂载仍使用原选人安全包过滤。
- A11 从多类来源汇总增量。双分支已用实际贞德包确认铁甲原版校正的候选命中 F3（规范头像导入描述和载荷）。最终是否需要重挂取决于当前栈顶。详见 [3a 交接](../refactor/perf-phase3a-global-writes-gpt.md)。
- A15 直接以低优先级挂完整背景包。包内出现同名保护路径即可记录，已有条目的实际优先权仍由 Godot 决定。
- A30 深度替换可更新场景的外部依赖，即使根路径是场景也需检查其依赖。候选路径不证明每项都被引擎替换。

## 2.5 审定结论（11-22 初定，11-23 第四轮更新）

- **D1 原版恢复不算违规**：写到 F1–F3 路径的内容如果字节来自游戏本体 PCK（例如 A11 给铁甲选人头像的原版校正），属于恢复原版，允许写入。台账标"原版恢复"。
- **D2 统一判定**保留三道旧过滤各自的职责，输入先限于当前原版 PCK 的真实条目。高优先级发布使用完整 F 集，包含本体中的间接载荷。
- **D3 剔除并记账**
  - A07/A10 的高优先级完整包和 A11 的增量覆盖先移除非原版 F 项，再发布剩余资源。保留原版字节、私有资源和安全的删除条目。台账逐包保存被剔除路径，挂载层只登记实际挂上的资源。
  - A06 允许运行资源通过私有别名使用。共享图集的直接内容或导入描述与本体字节一致时可以保留在共享路径，并记为恢复。
  - 如果索引读不出，或过滤后的包仍请求写入非原版保护项，带拒绝原因进入第三轮的回退逻辑。Godot 返回普通 `false` 时仍是一次引擎挂载失败。
  - `TakeOverPath` 和 `PreloadManager.Cache.SetAsset` 对单个非原版保护路径跳过此次写入并记账。
- **D4 低优先级挂包**：`replaceFiles: false`（A08、A09、A15）不会覆盖游戏里已有的条目，F 类路径只要游戏本体里存在就不算违规，只记台账。
- **D5 A30 深度重载**：`AncientCompendium` 的 `ReplaceDeep` 是重新读取、不发布新内容，不在 F 类范围内，这一阶段不动；改成私有别名放进后续计划。

## 3. 统一入口与台账

- 本表的显式全局写入由 `GlobalResourceWrites` 统一记账，含挂包、路径接管、预载缓存写入、强制替换加载和合成路径登记。
- 每次写入记一条台账：来源（例外表编号 + 调用点）、方式、涉及的规范路径（资源包读它自己的索引）、结果（尝试 / 成功 / 失败 / 恢复）。
- 台账分别记剔除、原版恢复、拒绝和引擎失败。剔除记录包含来源包和被剔除的实际路径。
- 门禁 `global-write-ledger` 的实包审计使用 `--audit-global-write-packs`，逐包输出剔除、原版恢复和整包拒绝数量，并断言整包拒绝为 0。运行方法和结果见 [第四轮交接](../refactor/perf-phase3a-global-writes-gpt.md#第四轮311)。
