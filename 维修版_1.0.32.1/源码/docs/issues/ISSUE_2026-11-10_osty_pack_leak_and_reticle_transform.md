# 随从皮肤包盖住主角皮肤 + 局内"外观"选择框错位 / 重置无效

- 状态：**已修（1.0.30.158）**；随从皮肤这条已用离线资源快照 + 实机启动日志验证，
  选人界面→进游戏 的整链仍需作者复测一次（没有新开一局，避免动到现有存档）。
- 来源：作者 2026-11-10 反馈（日志 `%APPDATA%\SlayTheSpire2\logs\godot.log`）

## 一、随从（奥斯提）选了 Orchis，骨妹无论选什么都变 Orchis

### 根因

`OrchisNecrobinderSkinMod` 是一个**同时覆盖 necrobinder 与 osty 两个分组**的完整运行时提供者
（`runtime-provider:OrchisNecrobinderSkinMod 2 linked groups`）。当它只被 `osty` 分组选中时：

1. `SkinService.MountSelectedLargeRuntimeProviderPacks` 仍然把它的**整包 PCK**用
   `replaceFiles: true` 推到 Godot 资源包栈顶。整包里除了 osty 私有资源，还有
   `res://animations/characters/necrobinder/*` 相关的 `.import`、`.godot/imported` 载荷和
   `char_select_necrobinder.png`、`top_panel/character_icon_necrobinder.png`、
   `map_marker_necrobinder.png`、遗物图标等**角色规范路径** → 这些路径会盖住角色当前皮肤。
2. 同一次只挂 `osty` 的重建里，`SkinCatalog.BuildOverlay` 会把该提供者
   `CollectFullRuntimeProviderBaselineOverlay` 的**全部**规范路径写回基线；于是点一下奥斯提皮肤，
   骨妹的头像/地图标记/遗物图标先被打回原版，而模型/骨骼仍可能取到栈顶那份包。
3. 实机日志里的旁证：`[necrobinderHpWear] necrobinder: live combat atlas page is 2017x634,
   expected 1883x1883 — fallback apply (SkinChanger switch)` 每帧刷屏（HpWear 的 atlas 和
   原版都是 1883x1883，2017x634 来自被推到栈顶的 Orchis 页），
   `[STS2-OrchisNecrobinderSkinFix] Disabled orphaned vanilla Necrobinder VFX because Orchis
   removed HeadBoneNode` 也说明骨妹身上被套了 Orchis 的骨骼。

### 修法（通用，不写死包名）

- `SkinCatalog.IsCompanionOnlyProviderSelection(providerId, selections)`：
  一个提供者只要有随从分组、且它**所有非随从分组都选了别的提供者**，就算"只给随从用"。
- `MountSelectedLargeRuntimeProviderPacks`：这类包改用 `replaceFiles: false` **低优先级**挂载
  （私有命名空间照样可见，随从皮肤本身通过校正包正常映射），永不进 `PromotedPackPaths`；
  同一个包若同时被真实分组选中（例如骨妹也选 Orchis），仍按原来的方式置顶。
- `SkinCatalog.BuildOverlay`：只挂随从分组时，跳过这类提供者的基线回写，不再改动角色规范路径。

离线验证：`CatalogInspect --visual-overlay osty OrchisNecrobinderSkinMod` 的输出从 83 个文件
（含 `char_select_necrobinder*.png.import`、`top_panel/character_icon_necrobinder.png.import`、
`map_marker_necrobinder.png.import`、遗物/手型图标）降到 60 个文件（只剩 Osty 自己的资源）。
实机日志改为 `已低优先级挂载随从皮肤包 OrchisNecrobinderSkinMod.pck：该包只用于随从自己的私有资源`
（不再出现 `已直接复用完整外观资源包 OrchisNecrobinderSkinMod.pck`）。

## 二、ESC →「外观」里的选择框：缩放后跑偏、重置像没生效、面板被手牌盖住

### 根因

1. **缩放不是绕中心**：`ApplySelectionReticleAndHitbox` 用 `position = base * scale`、
   `size = baseSize * reticleScale * scale` 直接把比例乘进矩形，锚点仍在左上角。
   缩放 ≠100% 时框只往右下长，只有左上角还贴在目标旁边（实机截图：385% 时只看到目标左上角一个小角标）。
2. **尺寸用错了基准**：传进去的是 `baseSize`（已除过 `resourceScale`），而不是上面算好的
   `size`（`baseSize * resourceScale * wrapperScale`）；怪物皮肤自带缩放时框会整体缩小。
   位置同样用的是 wrapper 局部坐标，而游戏自己写的是 `GlobalPosition`。
3. **数值被静默丢弃**：`OnTransformChanged` 在"该分组有排队中的换肤请求"时直接 `return`，
   但滑块/标签已经改成新值 → 界面显示 100%、模型和选择框还是旧值，看起来就是"重置按钮失效"。
4. **面板在手牌下面**：设置面板和手牌不在同一个 CanvasLayer，手牌画在面板之上，最后一行
   （选择框的 X/Y）被卡牌盖住、点击也被吞掉；面板本身也正好顶到屏幕底边。

### 修法

- `CharacterAppearanceRuntime.ApplySelectionReticleAndHitbox` 重写：
  先按"跟随模型缩放/移动"两个开关把模型变换剥掉再套回去，然后**绕矩形中心**应用玩家自己的
  `SelectionReticleScale`，最后加 `SelectionReticleOffsetX/Y`；位置和尺寸同时写回
  `SelectionReticle` 与 `Hitbox`，用 `GlobalPosition`（与 `NCreature.UpdateBounds` 一致）。
  尺寸基准改为 `size`（含 resource/wrapper 缩放），不再用 `baseSize`。
- `CharacterAppearanceScreen.OnTransformChanged`：数值一律先保存；只有"实时应用"这一句在有排队换肤时
  推迟，等 `OnQueuedSelectionFinished`（成功或失败都算）补一次 `ApplyTransformToKey`。
- `SetTransformControlValues` 的 `_updating` 改成 `try/finally`，属性 setter 抛异常也不会把闸门卡死。
- 面板与返回按钮移入独立的 `CanvasLayer(Layer=105)`（可见性跟随本界面），永远画在手牌之上；
  行高/间距压缩（44→40、7→5、分区标题 40→34），整套设置（含选择框的 X/Y）现在一屏放得下。

实机验证（2562×1440）：选择框 212% → 四角框完整、居中套住怪物；重置 → 回到 100% 的小框；
存盘再读档（暂停菜单 S&L）→ 面板仍显示 212%、框仍按 212% 套在怪物上。

## 三、改动文件

| 文件 | 改动 |
| --- | --- |
| `Catalog/SkinCatalog.cs` | 新增 `IsCompanionOnlyProviderSelection`；`BuildOverlay` 跳过"只给随从用"的提供者的基线回写 |
| `Core/SkinService.cs` | `MountSelectedLargeRuntimeProviderPacks`：随从专用包低优先级挂载、不置顶 |
| `Ui/CharacterAppearanceRuntime.cs` | 选择框矩形/缩放重写（中心锚点、GlobalPosition、正确的尺寸基准） |
| `Ui/CharacterAppearanceScreen.cs` | 变换编辑不再被排队换肤吞掉；`_updating` try/finally；面板独立 CanvasLayer + 紧凑排版 |

版本 1.0.30.158（`Entry.cs`、`STS2SkinChanger.csproj`、`SkinChanger.json`、`workshop.json` 四处）。

---

## 四、2026-11-10 傍晚追加（1.0.30.159）——作者复测仍然复现，查到真正的一层

### 4.1 作者日志（`godot2026-11-10T17.45.04.log`）里的关键序列

| 行 | 内容 | 含义 |
| --- | --- | --- |
| 1247 | `已直接复用完整外观资源包 OrchisNecrobinderSkinMod.pck` | 该会话**启动时**骨妹选的就是 Orchis，整包被置顶 |
| 2282 | `开始应用皮肤选择：necrobinder → necrobinderHpWear` | 之后换成 HpWear（HpWear 包也被置顶） |
| 2647 | `开始应用皮肤选择：necrobinder → necrobinderSkin_NSFW.Ver.2.0` | 再换成 NSFW 2.0 |
| 4130 | `开始应用皮肤选择：cultists → 汐の雨い 半血爆衣 csxjt&ghxjt` | 邪教徒换爆衣版 |

**Godot 的 `ProjectSettings.LoadResourcePack` 只能把包推上去，没有卸载 API。**
所以「曾经因为角色本体选中而被置顶」的包会一直压在栈顶；此后每次进新房间重建外观时，
只要校正包没有把它的规范路径写回基线，骨妹本体就又变回那份包（Orchis / HpWear）。

上一版（1.0.30.158）我为「只给随从用」的包加了 `IsCompanionOnlyProviderSelection` 过滤，
顺手把 `BuildOverlay` 里这类提供者的**基线回写**也跳过了——前提是「它没被置顶」。
但作者这个会话是**先选 Orchis 再换掉**，包早就置顶了，于是校正包不再回写 → 每场战斗/商店/火堆
都回到 Orchis。这是上一轮引入的回归。

### 4.2 修法

1. `SkinCatalog.BuildOverlay`：**恢复无条件基线回写**（含只给随从用的提供者）。
   已置顶的包无法卸载，校正包是唯一能让规范路径回到当前选择的机制。
2. `SkinService.RequestFullOverlayReassert` + `MountOverlay`：当「多分组完整提供者」在某个分组上
   被替换（例：Orchis 同时管 necrobinder+osty，骨妹改选别的），或某个已置顶的包降级为随从专用时，
   下一次重建**自动扩成全部分组**，把所有规范路径重新压一遍。日志会打
   `外观一致性保护：下一次重建将覆盖全部分组（原因：…）`。
3. 面板绑定：换肤会重建生物节点，面板若还握着旧节点，就会出现「滑条动了、模型不动」——
   新增 `CharacterAppearanceScreen.RefreshTargetCreatureReference()`，在读写变换前按实体重新绑定活节点
   （日志：`[外观诊断] 目标节点已重建，面板已重新绑定到 …`）。

### 4.3 邪教徒「半血爆衣」纹理错乱

包自带 DLL `HalfHealthSkin`：`Creature.set_CurrentHp` 的 Postfix 在血量 ≤ 一半时，
把骨骼换成 `res://animations/monsters/cultists/cultists_hurt.tres`。它把结果缓存在
**静态字典 `_hurtSkelCache`** 和 **`ConditionalWeakTable _appliedMap`** 里（按怪物 entry + 规范路径）。
换肤后这些缓存仍指向上一套 atlas 页 → 正常骨骼 + 旧 hurt 图页 = 纹理错乱（且换战斗也不恢复）。

修法：新增 `Core/ProviderRuntimeCacheReset.cs`，在每次选择变化时，对「切换前后涉及的提供者程序集」
里所有 `Dictionary<>` / `ConditionalWeakTable<>` / `HashSet<>` 静态字段（名字含 cache/applied/hurt）
调用 `Clear()`——**清内容而不是换字段**，因为它们是 `static readonly`。日志：
`[外观一致性] 已清空 <提供者> 的 N 个运行时资源缓存：…`。

### 4.4 新增诊断（按作者要求「多加些判断语句」）

`[外观诊断]` 前缀，按 scope 限流：

- `外观重建: 分组=N 生效覆盖=M 生效=配置 / 与配置不同: …`
- `场景实例化: 分组=necrobinder 生效选择=… 提供者=… 资源数=… 场景=…`
- `外观替换: 模型=… 分组=… 取资源分组=… 新场景=… 原场景=…`
- `选中目标: player:奥契丝(NECROBINDER) 分组=… 键=… 面板显示=… 存储=…`
- `下拉同步: 分组=… 配置=… 生效=… 用于显示=… 选中行=…`
- `面板改动: … 实时=… 读取=… 归一后=… wrapper=pos/scale … 存储=…`
- `应用变换: player:… wrapper=pos/scale visuals=scale 父节点=… visuals父=…`

只要作者再复现一次，`grep 外观诊断` 就能看出「哪一层给的皮肤/数值不对」。

---

## 五、2026-11-10 晚（1.0.30.160/161）——实机逐步复现后找到「重置没反应」的真凶

作者要求「自己测通再交付，每个环节截图」，本轮照做（截图见
`C:\Users\Administrator\Desktop\工作2\_实机验证_2026-11-10_1.0.30.161`）。

### 5.1 「重置」为什么像没反应

`SetCharacterCombatTransform` 写入非默认值时，会同时写两份：

```
options[optionId]        = value;   // 这个皮肤自己的
options["__group_default__"] = value;  // 整组记忆（换皮肤/读档也沿用）
```

而**重置**走的是 `IsDefaultCharacterCombatTransform` 分支，只删了 `options[optionId]`，
`__group_default__` 还在 → `GetCharacterCombatTransform` 读回的还是记忆里的旧值 →
面板显示 100%/0，模型保持 255%/(269,163)。实机日志（1.0.30.160）：

```
面板改动: … 读取=scale=1.00 off=(0,0) … 存储=scale=2.55 off=(269,163)   ← 存储没变
应用变换: … scale=2.55 off=(269,163) | wrapper=pos(269,163)/scale(2.55,2.55)
```

修法（1.0.30.161）：默认值分支里连同 `__group_default__` 一起删。

```
existing.Remove(optionId);
existing.Remove(GroupTransformMemoryKey);   // 新增
```

修好后同一步骤的日志变成 `存储=scale=1.00 off=(0,0)`、`wrapper=pos(0,0)/scale(1,1)`，
模型当场回到原位（截图 7）。

### 5.2 随从（奥斯提）在场内不能换皮肤

`CreateCreatureAppearanceBinding` 一直把同伴写死成 `CanSelectSkin:false`（旧设计：同伴跟随主人整套皮肤）。
但 `osty` 分组早已可以独立选择（选人界面的「奥斯提」下拉），于是出现"选人界面能选、局内不能选"的割裂。
新增 `SkinService.CanSelectCompanionSkin(groupId)`：只要玩家已解耦（`CompanionSelectionDecoupled`）
且该组是随从组，就允许面板显示皮肤下拉；选择走原有 `ApplySelection("osty", …)` 事务（不动主人分组）。
实机截图 3：奥斯提面板出现「皮肤」下拉，值为 `Orchis Necrobinder Skin Mod`。

### 5.3 本轮实机验证（1.0.30.161，2562×1440）

| # | 场景 | 结果 |
| --- | --- | --- |
| 1 | 选人界面：奥斯提=Orchis，骨妹=骨妹四档血量换装 | 两个下拉各自独立，`Orchis Necro` 在右上、角色皮肤在右上另一处 |
| 2 | 进入游戏第一场战斗 | 骨妹=血量换装的银发少女，奥斯提=Orchis 的白发魔女（**没有**被换成同款） |
| 3 | ESC→外观→点奥斯提 | 出现「皮肤」下拉（Orchis），模型/血条两个分区正常 |
| 4 | 奥斯提放大+拖动 | 缩放 170%、X 83/Y -28，模型跟着变大移动，主人不动 |
| 5 | 奥斯提「重置」 | 回到 100% / (0,0)，模型回原位 |
| 6 | 角色放大+拖动 | 缩放 255%、X 269/Y 163，模型跟着变大移动，奥斯提不动 |
| 7 | 角色「重置」 | 回到 100% / (0,0)，模型回原位（修好后） |

### 5.4 现场

为做「选人界面→第一场战斗」的整链验证，放弃过一次当前局；存档已从备份同时还原到
`%APPDATA%\...\modded\profile1\saves\current_run.save` 与 Steam 云目录
`D:\steam\userdata\791051706\2868840\remote\modded\profile1\saves\current_run.save`（26759 字节，18:28 那局），
并已在菜单确认「继续游戏 · 密林 楼层 2 · 进阶 10 · 52/66」。

---

## 六、2026-11-10 深夜（1.0.30.167）——奥斯提的两个收尾问题

### 6.1 奥斯提的 Orchis 皮肤带着原版火焰特效

`ApplySelectedVisualPostfix` 里判断"要不要回放该提供者的视觉后处理"时，对**完整运行时提供者**
（Orchis）要求"它在所有分组上都被选中"才回放。奥斯提组选了 Orchis、但角色组是别的皮肤时，
这个条件为假 → 提供者绑在 `Creature.CreateVisuals` 上的后处理（`RemoveChildNodes(visuals, "Flame")`、
`position += OstyCombatPositionOffset`）从不执行 → 模型是 Orchis、脚下还是原版蓝火。

修法：随从分组只看它自己的选择（`catalog.IsCompanionGroup(groupId)` 时不再要求"全组选中"）。
后处理只作用在该召唤物身上（`__instance` 是这只生物），不会碰到主人。

### 6.2 局内选「撑在地上的鸡煲」没有效果

`ApplyCompanionSelection` 内部先 `OstyImpostorBridge.ApplyQuiet(皮肤id)` 换精灵，随后
`RefreshCompanionGroup` 重建召唤物的画面节点，把刚换好的精灵整个替换掉。
修法：重建之后调用新增的 `SkinService.ReapplyCompanionImpostorSkin()` 再套用一次。

### 6.3 实机验证（1.0.30.167，同一场战斗）

| 步骤 | 结果 |
| --- | --- |
| 骨妹=血量换装、奥斯提=Orchis | 魔女脚下**没有**原版蓝火（修前有，见 0_ 截图） |
| 局内下拉→撑在地上的鸡煲 | 召唤物立刻变成那只趴地的黄色机体（与选人界面预览一致） |
| 再切回 Orchis | 变回魔女、依旧无蓝火 |

---

## 七、2026-11-11（1.0.30.170/172）——独立奥斯提 Mod 的皮肤为什么显示不出来

### 7.1 真因

`OstyDefectImpostor` 是在 `NCreature._Ready` 里把整条身体分支换成它自己的 SpineSprite
（日志：`Replaced full body branch with genuine unique SpineSprite inside an identity body group`），
并且"资产不完整就保留原版"。而本 Mod 的两条路径会在它之后**换掉召唤物的画面节点**：

- `ContextualSkinControls.ReplaceCreatedVisuals`
- `CharacterAppearanceRuntime.TryRebuildCreatureVisuals`

节点一换，它装好的精灵就没了，它的完整性检查随即失败 → 保留原版身体。
开着切换器时它的那句 `Replaced full body branch …` 根本不出现，就是这个原因。

### 7.2 修法

新增 `OstyImpostorCompat.OwnsSummonBody(groupId)`：它已安装、且（玩家选了它自己的皮肤 或 osty 组在"游戏默认"）
时返回 true。`ReplaceCreatedVisuals` 与 `TryRebuildCreatureVisuals` 都在这时候直接 return，不碰它的节点。

同时 `SkinService.CanSelectCompanionSkin` 不再依赖 `CompanionAutoFollow`/`CompanionGroupIds` 是否非空：
只要是目录里已知的随从组（或 id 为 `osty`），面板就必须显示皮肤行，否则会出现"换成鸡煲后无法切回去"。

### 7.3 实机验证（1.0.30.172）

| 步骤 | 结果 |
| --- | --- |
| 选人界面奥斯提=撑在地上的鸡煲 → 进战斗 | 战斗里显示那只趴地黄色机体（开着切换器） |
| 外观→奥斯提 | 皮肤行在，显示"撑在地上的鸡煲"，5 项齐全 |
| 局内切 Orchis | 立即变成 Orchis 魔女（无原版火焰） |
| 局内再切回鸡煲 | 面板已切换，但身体仍停在 Orchis：该 Mod 的 `Session.Request` 只是排队，落地要等它自己的"安全时机" |

### 7.4 仍未解决

`Orchis → 鸡煲` 的**局内即时**切换仍受该 Mod 排队机制限制（`已排队，当前动作与反馈结束后切换`）。
要即时只能等一次动作结束/重新召唤，或进一步驱动它的 `Session.Tick` 门控。

---

## 八、2026-11-11（1.0.30.174）——局内即时切换独立奥斯提 Mod 皮肤（专属补丁）

上一版遗留：局内把奥斯提切成"撑在地上的鸡煲"不生效（面板已保存，身体不变）。根因：该 Mod 的 `Session`
只在 `NCreature._Ready` 时绑定"当时那只身体"，我们替换过节点后它再也构造不出自己的 Twelve-name 身体。

修法（只针对该 Mod，不做通用）：
`CharacterAppearanceRuntime.RebuildSummonBodyForImpostor(groupId)` —— 仅当选中它的皮肤时调用，
把这只召唤物的身体**重建成原版骨骼**（`TryRebuildCreatureVisuals(..., ignoreSummonOwnership: true)`），
随后 `OstyImpostorBridge.RefreshLiveSummons()`（Unregister+Register）让它在认得的新节点上重建外观。
日志会连出两条它的 `Replaced full body branch with genuine unique SpineSprite …`。

实机验证（1.0.30.174，同一场战斗，面板不关）：
`Orchis → 撑在地上的鸡煲` 立即变机体；`撑在地上的鸡煲 → Orchis` 立即变回魔女（无原版火焰）。
