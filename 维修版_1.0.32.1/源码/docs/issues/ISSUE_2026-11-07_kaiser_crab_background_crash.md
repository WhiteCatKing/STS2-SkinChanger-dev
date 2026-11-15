# 进帝皇蟹 Boss 房：背景功能把遭遇自带背景当风景替换，本体无纹理、无卡牌、整局卡死

- 状态：**已修复**（1.0.27.85 保住本体 / 1.0.27.87 恢复换背景 / 1.0.27.88 修缩放，待实机验证）
- 涉及：`BackgroundSkinService`（新的「背景」功能）+ `Ui/EmperorCrabCompat`（白猫帝皇蟹兼容层，随本 Mod 打包）
- 对应清单：新增第 27 条

## 现象

在 HIVE（第三幕）走到 Boss 房、进入 `KAISER_CRAB_BOSS` 战斗时：

- 帝皇蟹**本体完全没有纹理**；
- **手牌不出现**，战斗不开始；
- 整个游戏**卡死**，只能强退。

## 日志证据（`godot.log` 17964–17987）

```text
17964: Creating NCombatRoom with mode=ActiveCombat encounter=KAISER_CRAB_BOSS.
17973: [SkinChanger] 已加载 kaiser_crab_boss_setup 的 24 个独立资源…
17974: [EmperorCrabSkinFix] 已用皮肤版场景创建局内帝皇蟹背景，group=kaiser_crab_boss_setup
17975: [EmperorCrabSkinFix] 已恢复局内帝皇蟹背景可见性
17978: [SkinChanger] 已按背景选择替换 HIVE 的房间背景：CZN Enemy Skin · Spaitz Outdoor2 2。
17980: [EmperorCrabSkinFix] 已更新 Crusher 的背景引用
17981: [EmperorCrabSkinFix] 已更新 Rocket 的背景引用
17982/17983: （同一批引用又被更新一次）
17986: [ERROR] System.ObjectDisposedException: Cannot access a disposed object.
       Object name: '…Vfx.Backgrounds.NKaiserCrabBossBackground'.
          at Godot.CanvasItem.SetVisible(Boolean visible)
          at MegaCrit.Sts2.Core.Models.Monsters.Crusher.AfterAddedToRoom()
          at MegaCrit.Sts2.Core.Combat.CombatManager.AfterCreatureAdded(…)
          at MegaCrit.Sts2.Core.Combat.CombatManager.StartCombatInternal(…)
```

## 原因

帝皇蟹是「背景即本体」的 Boss：`KaiserCrabBoss.HasCustomBackground == true`，它的
`CreateBackground` 走 `CreateBackgroundAssetsForCustom`，背景场景
`kaiser_crab_boss_background.tscn` 里挂着 `%KaiserCrab`（类型 `NKaiserCrabBossBackground`，
是 `Node2D`，**不是** `NCombatBackground` 的子类，只是它下面的一个子节点）——**这就是战斗里
可见的帝皇蟹本体**；两只钳子 `Crusher` / `Rocket` 还把这个子节点缓存在自己的 `_background`
私有字段里。

新的「背景」功能有两条替换路径：

| 路径 | 位置 | 行为 |
| --- | --- | --- |
| 创建时 | `ApplyToResult`（`EncounterModel.CreateBackground` 后置补丁） | 直接 `Free()` 掉结果换成地区背景 |
| 房间挂上后 | `ApplyToRoom`（`NCombatRoom.SetUpBackground` 后置补丁） | `RemoveChild` + `QueueFree()` 旧背景再挂地区背景 |

这次走的是第二条：17978 把选中的 CZN「Spaitz Outdoor2 2」换成了房间背景，
`QueueFree()` 掉整棵皮肤版帝皇蟹背景 —— 本体被删，钳子缓存的 `%KaiserCrab` 也随之失效。

`QueueFree()` 延迟到帧末释放，所以 17980–17983 写引用时完全不报错；
帧末真正释放后，`Crusher.AfterAddedToRoom()` 的 `SetVisible(...)` 撞上已释放对象，
异常从 `StartCombatInternal` 抛出，战斗初始化半途中断 —— 于是没卡牌、没纹理、卡死。

`ApplyToResult` 这次没打印日志，说明它当时提前 return 了，真正动手的是 `ApplyToRoom`。

## 修复（1.0.27.85）

**核心原则：遭遇自己声明的战斗背景（`EncounterModel.HasCustomBackground`）不是风景，一律不替换。**

> 一开始试过「只替换类型正好是 `NCombatBackground` 的节点」，但帝皇蟹背景的根节点
> **就是** `NCombatBackground`，特殊类只是它下面的子节点 —— 这条判据不会触发，已弃用。

1. 新增 `EncounterOwnsBackground(encounter)`：读 `EncounterModel.HasCustomBackground`
   （反射，`protected virtual`；本项目 `ResolveGameDefault` 已在用同一个属性）。
2. 新增 `CurrentEncounter(room)`：从 `NCombatRoom._visuals.Encounter` 取当前遭遇
   （只看 `room.Background` 分不清这背景是遭遇的还是地区的）；保留一条兜底：
   若背景类不是正好 `NCombatBackground`，也视为遭遇自带。
3. 三处入口统一用 `CanApplyActBackground(background, encounter)`：
   - `ApplyToResult`（创建时）
   - `ApplyToRoom`（挂上房间后，**本次事故入口**）
   - `RefreshCurrentActIfNeeded`（局内中途改背景，否则玩家一改设置就会重现卡死）
   假商人（`fake_merchant_event_encounter`）保留它原有的专门处理，不走这条跳过。
4. 第二层保险 `CombatCrabSkinPatch.UpdateMonsterBackgroundReferences`：给钳子写引用前，
   必须确认 `room.Background` 有效、确实是该节点的祖先、节点仍在场景树里；
   否则不动字段，让游戏自带的懒加载路径兜底（塞一个将死节点才是致命的）。

## 验证

1. HIVE 走到帝皇蟹 Boss 房：应看到皮肤版帝皇蟹本体（有纹理）、正常抽手牌、战斗可正常进行；
2. 日志里应出现 `HIVE 的战斗背景由遭遇自带（HasCustomBackground），已跳过地区背景替换。`
   且**不再**出现 `已按背景选择替换 HIVE 的房间背景`；
3. 帝皇蟹半血爆衣换装仍要正常（`已应用帝皇蟹半血爆衣皮肤`）；
4. 普通战斗（`HasCustomBackground == false`）背景替换照旧，日志仍是 `已按背景选择替换 … 的房间背景`；
5. 帝皇蟹战斗中打开背景设置改一次背景：不得卡死（应记录「已忽略中途换背景」）。

## 影响面说明

这条规则让所有 `HasCustomBackground == true` 的遭遇（女王、无面者、实验体、水瀑巨人、
知识恶魔、拉加维林女族长、帝皇蟹等 12 个）在战斗中**保留自己的战场**，不再套用地区背景。
这与 CZN Enemy Skin 自己的做法一致（它对 `HasCustomBackground` 的遭遇不做地区背景替换）。
代价：这些 Boss 战里选中的 CZN 战场不会出现；若作者希望这些 Boss 也能换场地，
需要另做「把战场叠在遭遇背景之下」的方案，而不是替换节点。

---

## 补充（1.0.27.86）：不换节点，只换图层

1.0.27.85 把帝皇蟹背景整个保住了，副作用是**这类 Boss 房再也换不了背景**（.85 实测：只显示原版）。

原因：`NCombatBackground` 的场景结构是「根节点 + `Layer_00..` 图层位 + 场景自有节点（帝皇蟹的
`%KaiserCrab` 就在这一层）」。战场 art 全部挂在 `Layer_XX` 里，所以只要**替换图层位里的内容**，
既能换战场、又不动遭遇自带的节点。

做法（`BackgroundSkinService`）：

1. `ResolveActBattlefieldOption(actId, selection)`：解析玩家选的战场（跟随分类/模组默认 → 提供者默认；
   明确选择 → 该选项；"游戏默认" → **返回 null，即保留遭遇自己的图**，等于给了一个"关掉"的开关）。
2. `LayerPathsFor(option, rng)`：给图层路径 —— 游戏自带背景用 `new BackgroundAssets(title, rng)`，
   皮肤包用 `BackgroundPackShell.LayerPaths`。
3. `GraftOptionLayers(...)`：**先在离屏 staging 节点里把图层实例化好**（`FillLayers` 会调用
   `configure_for_sts2` 契约，可能失败），成功后才清空真实 `Layer_XX` 并把新图层搬进去；
   失败则原样不动（避免"清空了却填不上"导致全黑）。
4. `TryGraftEncounterLayers(...)`：包一层幂等（背景上打 `sc_encounter_layers_option` 元数据，
   同一选择不重复叠加），三处入口（创建时 / 挂上房间后 / 局内中途改背景）都走它。

附带效果：
- 其它 `HasCustomBackground` 的 Boss（女王、无面者…）同样**可以换战场了**，只要它们的背景场景
  有 `Layer_XX` 图层位；没有图层位的场景会保留原样并记一条诊断；
- 帝皇蟹战斗中途改背景也能实时生效（只换图层，不动本体）。

### 这一版的验证

1. HIVE 帝皇蟹 Boss 房：本体有纹理、有手牌、不卡死；
2. 背景应变成所选的 CZN 战场（日志：`HIVE 战斗背景由遭遇自带，已把所选战场叠加进它的图层：…`）；
3. 把该幕背景切成「游戏默认」→ 帝皇蟹房应保持它自己的原版战场（不再叠加）；
4. 帝皇蟹半血爆衣仍正常；
5. 战斗中改一次背景：战场即时变化、不卡死、本体不消失；
6. 其它 Boss / 普通战斗：背景切换与此前一致。
---

## 补充（1.0.27.87）：为什么「只换图层」不行，改成整块垫在本体之后

.86 把选中战场的**图层场景**抽出来塞进帝皇蟹背景的 `Layer_00..` 槽位。实测结果：
**帝皇蟹上半身被背景盖住，只有下半一小部分露出来。**

查了场景和皮肤包才明白（`pckpeek` 读 `SlayTheSpire2.pck` 与 `CznEnemySkin.pck`）：

帝皇蟹背景 `kaiser_crab_boss_background.tscn` 的节点顺序是

```
KaiserCrabBossBackground (Control, NCombatBackground)
├─ Layer_00 .. Layer_03   ← 战场美术（由 SetLayers 填）
├─ Layer_04               ← 小装饰（beads）
├─ KaiserCrab             ← 本体（NKaiserCrabBossBackground），画在 00..04 之上
└─ Foreground
```

而 CZN 战场 `b_spaitz_outdoor2_2_background.tscn` 的结构是

```
SpaitzOutdoor22BackgroundShell (Control + battlefield_aggregate_runtime.gd)
└─ BattlefieldCanvas (CanvasLayer, layer = -1)     ← 关键
   ├─ Layer_00        ← 里面是 Node2D "Stage" + 一堆 SpineSprite
   └─ Foreground
```

它内部用**很大的 z_index** 排序（天空 2400~2600、地面/背景 -2100~-3100），
并且刻意放在 **CanvasLayer -1**，脚本注释写得很清楚：
`official ground/prop depth remains local to this canvas and can never compete with STS2 combat actors`。

把它的图层场景抽进 canvas 0 的 `Layer_00` 后，CanvasLayer 这层保护没了，
天空的 `z_index = 2400+` 直接压过 `z_index = 0` 的蟹本体 —— 于是"上半身被背景盖住"。

### 正确做法：整块节点垫在本体之后，只隐藏遭遇自己的布景

`TryApplyEncounterBackdrop`：

1. 用 `CreateOptionBackground` 生成**完整**的选中战场节点（保留它自己的 CanvasLayer -1 与 z 契约）；
2. 作为子节点插到帝皇蟹背景的**第 0 位**（对没有 CanvasLayer 的普通背景，这保证它画在本体之下）；
3. **隐藏**帝皇蟹背景自己的 `Layer_XX` 布景（保留 `KaiserCrab` 本体与 `Foreground`），
   这样新战场才露得出来；
4. 背景上打 `sc_encounter_backdrop_option` 记录当前垫的是哪一项，幂等。

于是：CZN 战场在自己的 -1 层、蟹本体在 0 层 → 本体永远在上，布景在下；不再有 z_index 冲突。

### 还原

把该幕背景切成「游戏默认」→ `ResolveActBattlefieldOption` 返回 null →
`ClearEncounterBackdrop` 删掉垫层、恢复 `Layer_XX` 可见 → 回到遭遇自己的原生竞技场。
战斗中改背景同样是"换垫层"，不碰本体。

### 这一版验证

1. HIVE 帝皇蟹房：本体完整可见（不再被盖住）、有手牌、不卡死；
2. 背景是所选 CZN 战场，日志：`HIVE 战斗背景由遭遇自带，已把所选战场垫在本体之后：…`；
3. 切成「游戏默认」→ 背景回到帝皇蟹自己的原生竞技场，日志：`… 战斗背景已恢复为遭遇自带。`；
4. 战斗中改一次背景 → 战场即时换、本体不消失、不卡死；
5. 半血爆衣、其它 Boss、普通战斗不受影响。
---

## 补充（1.0.27.88）：事件图在帝皇蟹房「缩小」——相机缩放补偿

实测：其它背景（CZN 战场）在帝皇蟹房一切正常，只有**不是场景、而是一张图的选项**
（`假商人事件背景 / Fake Merchant Event`，走 `CreateImageBackground`）会**缩小一圈**。

原因：`KaiserCrabBoss.GetCameraScaling() => 0.75f`，战斗里 `SceneContainer.Scale = 0.75`，
canvas 0 上的东西都按 0.75 画 → 1920×1080 的图只覆盖 75% 屏幕。
CZN 战场之所以没事，是因为它整块在 **CanvasLayer -1**，**不受 Camera2D 影响**，永远铺满。

修复（`AttachEncounterBackdrop`）：垫层如果是 canvas 0 的（节点树里**没有** `CanvasLayer`），
按 `1 / GetCameraScaling()` 反向缩放补偿；带 `CanvasLayer` 的包一律不动。

各遭遇的相机缩放（`GetCameraScaling`）：

| 遭遇 | 缩放 |
| --- | --- |
| KaiserCrabBoss | 0.75 |
| KnowledgeDemonBoss / TheKinBoss | 0.85 |
| CeremonialBeastBoss / QueenBoss / TheInsatiableBoss / VantomBoss / WaterfallGiantBoss / LagavulinMatriarchBoss | 0.9 |
| TestSubjectBoss / SoulFyshBoss / 两个事件遭遇 / 普通战斗 | 1.0 |

所以这张图在别的自带背景 Boss 房也会偏小（5%~15%），这次一并修掉了。

## 顺带：13 个自带背景的遭遇场景结构核对（离线，用 pckpeek 读 pck）

结论：**`.87` 的「只隐藏 Layer_*」对所有自带背景的 Boss 都安全** —— 每个 Boss 的本体
都是「非 Layer 子节点」，不会被隐藏。

| 场景 | Layer 位 | 保留的非 Layer 子节点 |
| --- | --- | --- |
| kaiser_crab_boss | 00–04 | KaiserCrab（本体）、Foreground |
| the_insatiable_boss | 00–02 | TheInsatiable（本体）、沙尘/瀑布粒子、Foreground |
| queen_boss | 00–03 | SpineSprite（本体）、Repy、雾/星光粒子、Foreground |
| ceremonial_beast_boss | 00–04 | SpineSprite/SpineSprite2（本体）、cutter、beads×22、雾/萤火、Foreground |
| vantom_boss | 00–04 | 油膜 shader（z_index −11）、油/雾粒子、Foreground |
| knowledge_demon_boss | 00–03 | 虫群粒子、Foreground |
| test_subject_boss | 00–03 | stars、Clouds1、Foreground |
| soul_fysh / the_kin / lagavulin / waterfall_giant / the_architect | 00–04 / 00 | 仅 Foreground |
| fake_merchant_event | 00–03 | 仅 Foreground（帐篷美术在图层里） |

已知小瑕疵（不影响功能）：Vantom 的油膜层 `z_index = −11` 会被垫层盖住；
帝皇蟹相机还有 `GetCameraOffset() = Down*35`，垫层未做 35px 位移补偿。