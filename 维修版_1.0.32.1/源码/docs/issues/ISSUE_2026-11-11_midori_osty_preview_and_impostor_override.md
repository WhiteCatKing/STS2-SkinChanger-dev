# 奥斯提：`midori_black` 选人预览不显示 + 提权后抢占「撑在地上的鸡煲」

- 状态：**已修复（内部版本 1.0.30.193），作者实机复测通过**
  - 1.0.30.189 实机复测：预览已修好；**局内仍是猫** → 第二根因是场景缓存，见第七节。
  - 1.0.30.190 实机复测：缓存刷新已生效但仍是猫 → 真根因是**选中项覆盖在基线还原之后又把提权包的猫壳写回校正包**，见第八节。
1.0.30.191 实机复测：仍是猫 → 第八节的还原循环根本没命中（日志里 [提权] 已按游戏原包还原 为 0 次）。
1.0.30.192 实机复测：仍是猫 → 真根因是**Godot 按路径的资源缓存 + 游戏 PreloadManager.Cache 已被旧皮肤污染**，见第九节。
1.0.30.193 实机复测：**通过**（猫 → 鸡煲正确显示鸡煲，见 9.5）。
- 来源：作者 2026-11-11 反馈（工坊条目 `3747561432` = midori_black、`3820801911` = OstyDefectImpostor）

## 一、结论先说

两个现象不是同一个原因，但都出在我们自己这边：

| 现象 | 根因 | 修复 |
| --- | --- | --- |
| midori 的奥斯提不在选人界面「奥斯提预览框」显示 | 预览框只会画 **Spine 骨架**（`osty_skel_data.tres`）；midori 的奥斯提是**帧动画场景**（`AnimatedSprite2D` + 帧图），包里没有 `*skel_data*.tres`，于是永远退回游戏原版骨架 | 预览框新增「场景型」分支（3.2） |
| 局内换成「撑在地上的鸡煲」仍显示 midori 的猫 | midori 同时被选成「亡灵契约师」皮肤 → 整份 PCK 被**提权到包栈顶**；随后写「资源校正包」时用的是**通用基线索引**，而提权包自己的索引也在里面、又是最后加入的，于是 `ResolveBaseline` 把 **midori 自己的 247 字节 `osty.tscn` 壳**当成「游戏原样」写回校正包 —— 猫一直压住规范路径。**不是** midori 强制挂载 | 校正包改用「只认游戏原包」的基线还原（3.3） |

「这个 mod 是强制挂载的」这个怀疑**方向对、位置错**：
- 启动时**没有**被游戏强制挂载 —— 日志明确 `已隔离皮肤提供者 midori_black … 原 PCK 未全局挂载`；
- 它的 DLL（`sts2_custom_character_sprite_kit_bdd`，`AssetCache.GetScene` / `MonsterModel.CreateVisuals` 等全局 Harmony 补丁）**一次都没运行过**（全部日志 `grep SpriteKit` = 0 行）；
- 真正"强制"的是**我们自己的提权路径**加上**基线索引顺序**。

## 二、证据

### 2.1 预览框为什么画不出猫

`Ui/OstyModelPreviewControls.cs:65 LoadSkeletonForCurrentSelection`（修复前）只有两种来源：

1. 选了独立奥斯提 Mod → `res://osty_defect/display_select.tres`；
2. 否则优先该选项自带的 `*skel_data*.tres`（`:86-93`），没有就 `res://animations/monsters/osty/osty_skel_data.tres`（`:24`）。

midori 的 osty 选项资源里只有场景 + 帧图，没有 skel_data → 走到第 2 条的兜底。日志（`godot.log`，osty=midori）：

```
[INFO] [SkinChanger] 奥斯提预览：加载 res://animations/monsters/osty/osty_skel_data.tres，类型=SpineSkeletonDataResource。
```

所以预览框画的是**原版奥斯提骨架**，与 midori 无关。

### 2.2 midori 的奥斯提到底是什么

`PckInspect` 列出 `midori_black.pck`（1325 项）关键结构：

- `res://scenes/creature_visuals/osty.tscn`（247 B）—— 只是一个壳：

  ```gdscript
  [gd_scene load_steps=2 format=3 uid="uid://nmmy4366ndwf"]
  [ext_resource type="PackedScene" uid="uid://cbb7da26gx5e6" path="res://scenes/creature_visuals/templates/osty.tscn" id="1_template"]
  [node name="Osty" instance=ExtResource("1_template")]
  ```

- `res://scenes/creature_visuals/templates/osty.tscn`（54 622 B）—— 真正的猫，`AnimatedSprite2D` + `catimage/**` 共 239 张帧图；动画名 `attack`、`attack_poke`、`dead_loop`、`die`、`hurt`、`idle_loop`、`revive`。
  ⚠️ **游戏本体没有 `templates/osty.tscn` 这个路径**（读出来 0 字节），所以它没有"游戏基线"可回退。
- `sprite_kit_config.cfg` 里的 `custom_scene_replacements`：`source_scene=osty.tscn → replacement_scene=templates/osty.tscn`，这是给作者自己的 SpriteKit DLL 用的；我们没运行它的 DLL，但**同样的路径映射被写进了覆盖包**，效果等价。

### 2.3 "强制替换"的直接证据：校正包把猫壳当成了游戏基线

`godot2026-11-11T04.47.35.log` 里，`开始应用皮肤选择：necrobinder → midori_black` 之后连续 3 次：

```
[INFO] [SkinChanger] 已直接复用完整外观资源包 midori_black.pck，后续仅生成资源校正包。
```

对应代码 `Core/SkinService.cs:6793-6871`：`directProviderPacks` 按分组收集运行时提供者整包；`IsCompanionOnlyProviderSelection(...)` 为 false（因为 necrobinder 也选了 midori）→ 走提权分支；`ProjectSettings.LoadResourcePack(packPath, replaceFiles: true)`，Godot 只能置顶、**不能卸载**。

但真正把猫写进「校正包」的是下一步。`MountOverlayCore`（`:6304-6433`）顺序：

1. `:6365 promotedPackResourcePaths` = 提权包全部路径；
2. `:6377 staleCanonicalPaths` = 受影响分组规范依赖 ∪ 提权包路径；
3. `:6401 files = catalog.BuildBaselineDependencyOverlay(staleCanonicalPaths)`；
4. 写包并 `LoadResourcePack(overlay, replaceFiles: true)`。

**决定性取证**（读 `%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\overlay-cache` 里的缓存包，`PckInspect --read` 取文件测字节）：

| 覆盖包 | 大小 / 时间 | `res://scenes/creature_visuals/osty.tscn` |
| --- | --- | --- |
| `9eb7eea3…` | 874 MB / 4:48 | 247 B（midori 猫壳） |
| `474e5552…` | 199 MB / 4:41 | 247 B（midori 猫壳） |
| `3407b46b…` | 198 MB / 4:41 | 247 B（midori 猫壳） |
| `2983c05a…` | 1.4 MB / 5:22 | **4 064 B（游戏原版 Spine）** |

根因在 `Catalog/SkinCatalog.cs:838-851` + `ResolveBaseline`（`:2808-2826`）：`Build` 先把游戏 `gameArchive` 作为基线索引 0，随后把 **AffectsGameplay 的 mod 索引追加到后面**；`ResolveBaseline` 从**最后一个索引往前**找，所以 midori 的壳盖住了游戏原版。于是"还原基线"写回的是猫。

结果：任何按规范路径重建的奥斯提都拿到猫，包括选「撑在地上的鸡煲」时 `CharacterAppearanceScreen.cs:987` → `CharacterAppearanceRuntime.RebuildSummonBodyForImpostor`（`:1278`）那次重建 → 独立 Mod 找不到它要求的 12 个骨骼名 → 按它自己的日志 `incomplete assets keep original visuals` 保留猫。

### 2.4 附带 bug：带空格的路径进不了依赖闭包

修复前 `CatalogInspect --visual-overlay osty midori_black` 输出 **600** 项，mod 里的帧图是 **624** 张：

| 目录 | mod 内 | 覆盖包内（修复前） |
| --- | --- | --- |
| 猫待机 | 48 | 47（`猫待机_00031` 原包就没有，不算丢失） |
| 猫攻击 | 48 | 48 |
| 猫咪退场 | 46 | 46 |
| 猫受击 | 26 | 26 |
| 猫死亡待机 | 48 | 48 |
| **猫召唤 2** | **24** | **0** |

原因：`Catalog/SkinCatalog.cs:9086` 的 `EmbeddedResourcePathRegex` 把**空格**当成路径终止符，`res://…/catimage/猫召唤 2/猫召唤 2_00009.png` 匹配不到（`…/猫攻击/猫攻击_00009.png` 能匹配），整个 `猫召唤 2` 目录被丢掉。实机后果：

```
ERROR: No loader found resource: res://scenes/creature_visuals/catimage/猫召唤 2/猫召唤 2_00000.png (expected type: Texture2D)
```

注：624 只是帧图数，配套的 `.s3tc.ctex` 也要进包，所以修复后的正确预期是 **648**。

### 2.5 缓存层（本轮未改）

游戏的 `AssetCache` 按路径缓存 `Resource`（`GetAsset` 命中直接返回、不重判包栈），我们的 `ContextualSkinControls.ReplaceCachedScene`（`:3097`）对 `/creature_visuals/` 路径直接 `return`（`:3105-3110`）。修复后规范路径本身就是游戏原版，新会话首次加载即正确；覆盖包签名由文件表内容决定，旧缓存包（含 247 B 猫壳）不会被复用，无需清缓存。同会话内从"midori 猫"切到别的皮肤仍受既有缓存策略限制（与本问题无关，历史上就如此）。

## 三、已实施的修法（1.0.30.189）

### 3.1 依赖闭包允许空格

`Catalog/SkinCatalog.cs:9086-9093` 只把取路径的字符类里的空格去掉（终止 lookahead 仍保留空格，否则 `res://a.png 中文备注` 这类文本会因惰性量词整条失配）。
验收：`CatalogInspect --visual-overlay osty midori_black` **600 → 648**（+24 png +24 ctex）；逻辑测试全绿。

### 3.2 预览框支持「场景型」奥斯提

`Ui/OstyModelPreviewControls.cs`：

- `TryLoadSceneForCurrentSelection`：独立 Mod 不走（它有自己的 `display_select.tres`）；否则从当前选项资源里优先取规范 `res://scenes/creature_visuals/osty.tscn`，退化时取 `/creature_visuals/` 下最短的 `.tscn`（避免拿到 `templates/osty.tscn`）；
- `LoadSceneForPreview`：先 `SkinService.GetOrLoadRuntimeScene`，失败再 `ResourceLoader.Load<PackedScene>(…, CacheMode.Ignore)`；
- `TryShowScene`：实例化为 `Node2D`、校验有 `AnimatedSprite2D`/`Sprite2D`（原版 Spine 壳没有精灵，直接放弃并落回骨架分支）、播放 `idle`/`待机` 动画；
- `MeasureSceneBounds` / `FitScene`：优先用场景里的 `Bounds` 控件，否则按精灵局部包围盒合并，缩放公式与 `FitModel` 一致（`×0.85`）。

### 3.3 校正包按「游戏原包」还原提权路径

- 新增 `Catalog/SkinCatalog.cs` `BuildGameBaselineOverlay(IEnumerable<string>)`：只用 `_baselineIndexes[0]`（游戏本体）解析路径，提供者私有资源（游戏里没有的路径）不还原。
- `Core/SkinService.cs:6405` 在 `BuildBaselineDependencyOverlay(staleCanonicalPaths)` 之后合并 `BuildGameBaselineOverlay(promotedPackResourcePaths)`；其后的 `FindBaselinePathsShadowingSelectedRemaps` 仍会删掉「选中 provider 自己要用的规范路径」，所以 necrobinder=midori 时猫皮正常，osty 没选 midori 时拿回 4 064 B 原版。
- 离线验收（`CatalogInspect … --force-gameplay midori_black --baseline-probe res://scenes/creature_visuals/osty.tscn`）：

  ```
  legacy: res://scenes/creature_visuals/osty.tscn = 247 B    ← 修复前的错误基线
  game  : res://scenes/creature_visuals/osty.tscn = 4064 B   ← 修复后
  ```

  副作用：覆盖包不再把 `catimage/**`、`.godot/imported/**` 复制进去（这些由已挂载的提权包提供），体积会显著变小。

### 3.4 原计划的「隔离加载原版场景」不再需要

3.3 让规范路径本身就是原版，`RebuildSummonBodyForImpostor` 不必绕开包栈。

（**1.0.30.189 复测后更正**：包栈确实已经是原版，但游戏进程内的场景缓存没有跟着换，见第七节。）

## 四、待实机确认

1. 选人界面：osty 选 midori → 预览框显示黑猫帧动画（日志 `奥斯提预览：用场景型外观 …`）；osty 选「撑在地上的鸡煲」→ 仍用 `res://osty_defect/display_select.tres`。
2. 局内：necrobinder=midori + osty=撑在地上的鸡煲 → 鸡煲不再被替换成猫。
3. 日志：不再出现 `No loader found resource: …/猫召唤 2/…`；覆盖包体积明显下降。

## 五、旧版本的临时规避（历史记录）

- 想用 midori 的奥斯提：**只**在「奥斯提」下拉里选它（不要在角色下拉里也选 midori）。此时 `IsCompanionOnlyProviderSelection` 为真 → `replaceFiles: false` 低优先级挂载。
- 已经把它选成角色皮肤：**重启游戏**再切奥斯提（提权过的包本进程内撤不掉）。

## 六、相关文件

| 文件 | 关注点 |
| --- | --- |
| `Ui/OstyModelPreviewControls.cs` | 预览：Spine 分支 + 新增场景型分支 |
| `Catalog/SkinCatalog.cs:9086` | 依赖闭包正则（空格路径） |
| `Catalog/SkinCatalog.cs` `BuildGameBaselineOverlay` | 只认游戏原包的基线还原 |
| `Core/SkinService.cs:6401-6410` | 校正包组装顺序：通用基线 → 游戏基线合并 |
| `Core/SkinService.cs:6452-6510` | 选中项覆盖之后：提权包外来规范路径再按游戏原包还原 |
| `Core/SkinService.cs:9157` | `LargeProviderMountPlan`（含 `PromotedProviderIds`） |
| `Core/SkinService.cs:6793-6871` | 整包提权 / 随从专用低优先级挂载 |
| `Ui/CharacterAppearanceRuntime.cs:1278`、`:1574` | 为独立 Mod 重建原版身体 |
| `Ui/ContextualSkinControls.cs:3097` | `AssetCache.GetScene` 后置接管（`/creature_visuals/` 跳过） |
| `tools/CatalogInspect/Program.cs` `--baseline-probe` / `--force-gameplay` | 基线还原离线探针 |

## 七、1.0.30.190：局内仍是猫的第二个根因 —— 游戏的场景缓存

### 7.1 复测事实（1.0.30.189）

切换后覆盖包是对的，猫却不是包栈给的：

- `overlay-cache\45c84c93…pck`（1 MB，19:14:35）里 `res://scenes/creature_visuals/osty.tscn` = **4 064 B 游戏原版**（首行 `[gd_scene load_steps=11 format=3 uid="uid://c1o05keyx0hrv"]`）；
- 同一时刻之前的 `de43a6d9…pck`（453 MB，19:14:02）= 247 B 猫壳；
- 配置里 `osty = __base__`、`CompanionImpostorSkinId = OstyDefectImpostor.ground_defect`，midori 没有被任何分组选中。

日志（`godot.log` 19:24:45）在切回原版的那一刻：

```
[WARN] [OstyDefectImpostor] Body replacement rolled back; original visuals retained:
       InvalidOperationException: Expected a direct native %Visuals SpineSprite; another mod or game version changed the layout.
[INFO] [奥斯提] 已为独立 Mod 重建 1 只召唤物的身体（原版骨架），准备让它重建外观。
[INFO] [奥斯提] 重建后重新套用独立 Mod 皮肤：OstyDefectImpostor.ground_defect
```

之后没有任何 OstyDefectImpostor 日志 —— 它拿到的身体里没有原生 `%Visuals` SpineSprite，于是回滚、保留旧外观（也就是之前那只猫）。

### 7.2 为什么重建会拿到猫

游戏的身体场景**全部**经过 `MegaCrit.Sts2.Core.Assets.PreloadManager.Cache`（`MonsterModel.CreateVisuals()` 就是 `PreloadManager.Cache.GetScene(VisualsPath).Instantiate<NCreatureVisuals>(…)`）：

- `AssetCache.GetAsset` 命中 `ConcurrentDictionary<string, Resource> _cache` 就**直接返回**，不会重新判断包栈；
- 未命中时用 `ResourceLoader.Load<Resource>(path, null, CacheMode.Reuse)`，Godot 自己的资源缓存同样返回旧实例；
- 我们的 `Ui/ContextualSkinControls.cs:3097 ReplaceCachedScene` 对 `/creature_visuals/` 路径直接 `return`（`:3105-3110`，`:3129`），所以缓存层从来没人纠正。

于是 midori 的猫一旦在本进程里被实例化过，`res://scenes/creature_visuals/osty.tscn` 的缓存条目就一直指向猫壳（→ `templates/osty.tscn` 帧动画场景），重建出来的"原版身体"其实是猫：`HasSpineAnimation == false`，没有 SpineSprite，独立 Mod 必然回滚。

### 7.3 修法

`Core/SkinService.cs` 新增 `RefreshCachedCreatureVisualScenes(IEnumerable<string> mountedPaths)`：

- 只处理本次覆盖层里 `*.tscn` 且路径含 `/creature_visuals/` 的条目；
- 且只处理 `PreloadManager.Cache.ContainsKey(path)` 为真的（游戏确实缓存过它）；
- 用 `ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Ignore)`（只绕开这个场景自身，不重读骨骼/贴图依赖）重新读一遍，再 `cache.SetAsset(path, scene)` 写回；
- 成功时打一条 `[缓存] 已按当前包栈刷新 N 个外观场景缓存。`。

调用点在 `MountOverlayCore` 的 `Mark("写包+挂载…")` 之后、且 `mountedDeltaFiles > 0` 时——此时新层已经在包栈顶。

另在 `Ui/CharacterAppearanceRuntime.cs` 的独立召唤物重建分支加了一条诊断（`[奥斯提] 重建身体完成：类型=…，Spine=…，子节点=…`），下次实机可以直接看出重建出来的到底是原版 Spine 还是猫。

### 7.4 实机验收

1. `necrobinder = midori_black` + `osty = __base__` + 独立 Mod「撑在地上的鸡煲」，进战斗后应是鸡煲；
2. 日志出现 `[缓存] 已按当前包栈刷新 N 个外观场景缓存。`，且 `[奥斯提] 重建身体完成：… Spine=True …`；
3. 不再出现 `Body replacement rolled back … %Visuals SpineSprite`。


## 八、1.0.30.191：真正的根因 —— 选中项覆盖又把提权包的猫壳写回了校正包

### 8.1 复测现象（1.0.30.190）与作者给出的规律

1.0.30.190 上线后局内仍是猫，但作者给出了一条很干净的复现规律：

1. 进游戏时选的就是「撑在地上的鸡煲」→ 正常显示鸡煲；
2. 局内**先选「猫」再切「撑在地上的鸡煲」**→ 显示猫；
3. 局内先选「撑在地上的鸡煲」以外的任何别的皮肤，再切「撑在地上的鸡煲」→ 正常显示鸡煲。

也就是说，只有「猫 → 鸡煲」这一条路径坏。

### 8.2 日志证据（`godot.log` 19:37:32，1.0.30.190 运行）

```
[性能] 覆盖包[visual] 写盘 0.0 MB = 1.0 ms（2 项）
LoadResourcePack = 0.7 ms（2 项）
[缓存] 已按当前包栈刷新 1 个外观场景缓存。
随从分组 osty 独立应用：__base__（不影响任何其它分组）。
[WARN] [OstyDefectImpostor] Body replacement rolled back; original visuals retained:
InvalidOperationException: Expected a direct native %Visuals SpineSprite; another mod or game version changed the layout.
[INFO] [奥斯提] 重建身体完成：类型=NCreatureVisuals，Spine=False，
子节点=Visuals(AnimatedSprite2D)、Bounds(Control)、CenterPos(Marker2D)、IntentPos(Marker2D)、OrbPos(Marker2D)、TalkPos(Marker2D)
```

注意两点：

- 这次增量层只有 **2 项**，而成功的那几次（`:8082`、`:8332`）重建出来的是 `Spine=True`、子节点为 `OstyDefectReviveFragments(Node2D)、OstyDefectBodyGroup(CanvasGroup)、Bounds(Control)、CenterPos(Marker2D)、IntentPos(Marker2D)`——独立 Mod 自己的鸡煲节点；
- 失败时子节点就是 midori 猫场景的原始树（`Visuals` 是 `AnimatedSprite2D`），说明 `MonsterModel.CreateVisuals()` 当场返回的就是猫壳，**不是** 7.2 里猜的「缓存没刷」——缓存确实刷了，刷到的却还是猫。

### 8.3 真正的根因：校正包的装配顺序

`Core/SkinService.cs` 的 `MountOverlayCore` 里，文件表是这么装配的：

1. `:6402` `files = catalog.BuildBaselineDependencyOverlay(staleCanonicalPaths);`
2. `:6406-6409` 把提权包路径按**游戏原包**还原（`BuildGameBaselineOverlay`）——1.0.30.189 的修法；
3. `:6412` `var selectedOverlay = catalog.BuildOverlay(effectiveSelections, overlayGroups);`
4. `:6431-6451` 把选中项的资源**逐个写回** `files[selectedFile.Key] = selectedFile.Value;`

问题在第 4 步：midori 系皮肤的选项资源表里**本身就带着** `res://scenes/creature_visuals/osty.tscn`。离线可复现（`CatalogInspect`，`necrobinderHpWear` 就是配置里的 `necrobinder` 选择）：

```
visual overlay: 275 files
res://scenes/creature_visuals/osty.tscn
```

于是 1.0.30.189 在第 2 步还原好的原版 `osty.tscn`，又在第 4 步被 midori 包里那份 **247 B 猫壳**覆盖回去；校正包（2 项 = `osty.tscn` + 它的 remap）压到栈顶，缓存刷新读到的自然还是猫，`CreateVisuals()` 返回猫。

`OstyDefectImpostor` 的 postfix 校验 `%Visuals`：

- 必须是 `Parent == root` 的直接子节点、`GetClass().ToString() == "SpineSprite"`；
- 还要求 `!IsInsideTree() && !IsNodeReady()`，否则抛 `"Replacement requires the original unready, off-tree NCreatureVisuals root."`；
- 猫场景这点不满足 → 回滚 → `original visuals retained` → 屏幕上留下猫。

这同时解释了三条规律：开局就选鸡煲（没有提权过 midori，第 2 步的还原没被覆盖）、「猫 → 鸡煲」（midori 被提权过，第 4 步把猫壳写回）、「别的皮肤 → 鸡煲」（别的皮肤选项表里没有 osty 的规范路径，覆盖不到）。

### 8.4 修法（1.0.30.191）

在选中项覆盖**之后**、缓存 `_cachedOverlayPlan` **之前**再判一次归属：

- `LargeProviderMountPlan` 增加 `PromotedProviderIds`（`MountSelectedLargeRuntimeProviderPacks` 里按真正被提权的包反查 provider；record 定义在 `Core/SkinService.cs:9157` 附近）；
- 遍历 `files` 中 `ArchivePath` 属于提权包的条目：必须在游戏原包里存在（`catalog.IsBaseGameResource`）→ `catalog.FindGroupIdForResourcePath(path)` 找归属分组 → 归属分组当前选择解析出的 provider（`catalog.ResolveVisualProviderId`）**不在**提权 provider 集合里 → 按游戏原包还原；归属分组自己仍选着这个提供者时不动（midori 的猫形态不受影响）；
- 一次性 `catalog.BuildGameBaselineOverlay(foreignPaths)` 写回，并打日志 `[提权] 已按游戏原包还原 N 条其它分组的规范路径：…`。

同时把缓存刷新条件从 `mountedDeltaFiles > 0` 放宽为 `overlayPlanRebuilt || mountedDeltaFiles > 0`，并把刷新日志改成带具体路径（`[缓存] 已按当前包栈刷新 N 个外观场景缓存：res://…`），避免「增量 0 项但缓存仍是旧皮肤」时漏刷。

### 8.5 实机验收（1.0.30.191）

1. 局内先选「猫」再切「撑在地上的鸡煲」→ 应显示鸡煲；
2. 日志出现 `[提权] 已按游戏原包还原 1 条其它分组的规范路径：res://scenes/creature_visuals/osty.tscn`；
3. 日志出现 `[缓存] 已按当前包栈刷新 … osty.tscn`，且 `[奥斯提] 重建身体完成：… Spine=True …`；
4. 不再出现 `Body replacement rolled back …`。
## 九、第三个根因：路径级资源缓存被旧皮肤污染（1.0.30.192 → 1.0.30.193 修复）

### 9.1 1.0.30.191 为什么没命中
作者 20:00 那次运行（godot.log 968 295 B）里 [提权] 已按游戏原包还原 出现 **0 次**，说明第八节那套「按归属分组再还原」的判定一次都没进。同一次运行里切到鸡煲时出现的是：

`
[性能] 覆盖包[visual] 命中磁盘缓存 …（2 项）
[缓存] 已按当前包栈刷新 1 个外观场景缓存：res://scenes/creature_visuals/osty.tscn
随从分组 osty 独立应用：__base__
[WARN] [OstyDefectImpostor] Body replacement rolled back … Expected a direct native %Visuals SpineSprite
重建身体完成：Spine=False，子节点=Visuals(AnimatedSprite2D)…
`

即：包栈、覆盖包都已经是对的，但**按路径取出来的场景仍然是猫**。

### 9.2 机制
两条独立的按路径缓存，包栈换了也不会自动失效：
1. Godot 自己的资源缓存：ResourceLoader.Load(path, cacheMode: Reuse) 命中就返回旧资源；
2. 游戏 MegaCrit.Sts2.Core.Assets.AssetCache（PreloadManager.Cache）：_cache 是 ConcurrentDictionary<string, Resource>，按**原样路径字符串**做键，GetAsset 命中且实例有效就直接返回，MonsterModel.CreateVisuals() 正是 PreloadManager.Cache.GetScene(VisualsPath)，而 Osty 没有 override VisualsPath，所以永远是 es://scenes/creature_visuals/osty.tscn。

猫形态一旦先被实例化并写进这两层缓存，后面无论怎么压包栈、怎么写覆盖包，CreateVisuals 拿到的都还是猫。OstyDefectImpostor 的 VisualBranchReplacement.TryReplace 要求 %Visuals 是直接子节点、GetClass()=="SpineSprite"、且节点必须**未进树且未 ready**，猫是 AnimatedSprite2D → 只能回滚保留猫。

### 9.3 修法（Core/SkinService.cs）
- EnsureGameSceneOnTop(string scenePath)：从 catalog.TryReadBaseGameResource 取游戏原包字节（含 .remap），写一个**包名带自增序号**的小包（ase_scene_{hash}_{seq:D3}.pck；同一包名重复 LoadResourcePack 不会重新压到栈顶，必须每次新包）→ LoadResourcePack(..., replaceFiles: true) → 用 ResourceLoader.Load<PackedScene>(path, null, CacheMode.Replace) 强制重读并用 PreloadManager.Cache.SetAsset 写回 → 再刷一次缓存。
- ProbeSceneHasSpine(string scenePath)：按规范路径取一次场景、实例化、读 HasSpineAnimation、Free()。
- InstantiateGameCreatureVisuals(string scenePath, out string? error)：探针仍判「不是 Spine」时的兜底——把同样的原包字节写进**私有别名路径** es://__skin_changer_base/{hash}/{文件名}.tscn（该路径在两层缓存里都没有历史条目），从别名路径实例化。
- Ui/CharacterAppearanceRuntime.cs：TryRebuildCreatureVisuals(..., NCreatureVisuals? prebuiltVisuals = null) 支持传入预制身体；RebuildSummonBodyForImpostor 先 EnsureGameSceneOnTop → 探针为真就按常规 CreateVisuals 通道重建（让独立 Mod 的 CreateVisuals Postfix 自然接管），探针为假才用别名兜底。

### 9.4 为什么别名兜底也能显示正确的鸡煲
别名身体是**原生 Spine** 的完好原版 NCreatureVisuals（Spine=True、Visuals(Node2D)），重建进树后 ReapplyCompanionImpostorSkin() → OstyImpostorBridge.ApplyQuiet + RefreshLiveSummons() 能按它自己的 API 重建外观，于是显示鸡煲而不是幽灵原版。

### 9.5 实机复测（1.0.30.193，作者要求由 AI 自己上号）
用 --rendering-driver 无关的引擎内截图 + 文件驱动切换跑通：
1. 战斗中 ESC → 外观 → 点奥斯提 → 皮肤选 midori_black（猫）；
2. 再把皮肤切到「撑在地上的鸡煲」→ **画面变成从地里伸出的鸡煲（蓝色火焰），不再是猫**；应用变换 从 scale=5.00 off=(-67,-10) 变回 scale=1.45 off=(-93,-71)。
3. 直接用 UI 下拉重复同样两步（不是脚本通道）也通过。
4. 日志：[原版] 已把游戏原版场景压到栈顶 …（4064 B） → [原版] 别名场景实例化成功 … → [奥斯提] 重建身体完成：Spine=True，子节点=Visuals(Node2D)… → 重建后重新套用独立 Mod 皮肤：OstyDefectImpostor.ground_defect。

### 9.6 自动化复测设施（默认关闭）
Core/DebugSnapshot.cs 提供两条**只在环境变量 STS2SC_DEBUG=1 时启用**的通道（正常游玩完全不可见）：
- %TEMP%\sts2_snap.req 存在 → 等 RenderingServer.FramePostDraw 后把视口存成 %TEMP%\sts2ctl\snap.png（Vulkan 下外部截图只能拿到黑屏，引擎内取纹理有效）；
- %TEMP%\sts2_pick.txt 写 <分组>|<选项ID> → 走与下拉完全相同的应用路径（ApplyCompanionSelection → 重建 → ReapplyCompanionImpostorSkin）。
注意：运行时加载的 Mod DLL 里 Node 的虚方法（_Ready/_Process）不会被引擎回调，必须挂 SceneTree.ProcessFrame / RenderingServer.FramePostDraw 这类原生信号。
## 十、1.0.30.194：别名兜底绕过独立 Mod 的 CreateVisuals 补丁（「原皮」根因）

### 10.1 现象（作者反馈）
选「撑在地上的鸡煲」时局内显示**原皮**，只有重新选中该皮肤（或 S/L）后才出现鸡煲；同时所有奥斯提皮肤下单位名都变成「坚果」。

### 10.2 根因（原皮）
1.0.30.193 的别名兜底是**我们自己** Instantiate 一份原版身体再交给 TryRebuildCreatureVisuals(prebuiltVisuals:…)。而 OstyDefectImpostor 的身体替换挂在 MonsterModel.CreateVisuals 的 Harmony **Postfix** 上，自己实例化就把这条链路绕过去了 —— 独立 Mod 永远不会接管，身体停在原版幽灵形态。

### 10.3 修法
- Core\SkinService.cs：拆出 LoadGameSceneViaAlias()（写 ase_alias_{hash}.pck，从私有别名路径 es://__skin_changer_base/{hash}/osty.tscn 取场景），新增 ForceCanonicalSceneFromAlias()：把该别名场景 PreloadManager.Cache.SetAsset(scenePath, scene) **登记到规范路径**上。
- Ui\CharacterAppearanceRuntime.cs：RebuildSummonBodyForImpostor 不再传 prebuilt，而是 EnsureGameSceneOnTop → ProbeSceneHasSpine 为假时 ForceCanonicalSceneFromAlias，随后**始终走常规** TryRebuildCreatureVisuals，让 MonsterModel.CreateVisuals() 自己取场景、独立 Mod 的 Postfix 正常执行。
- 删除 TryRebuildCreatureVisuals 的 prebuiltVisuals 参数。

### 10.4 复测（1.0.30.194，实机）
猫 → 鸡煲后日志：
`
[原版] 别名场景已就绪：res://__skin_changer_base/BB82E24D637D864C/osty.tscn ← res://scenes/creature_visuals/osty.tscn（4064 B）
[原版] 已把别名场景登记为规范路径缓存：res://scenes/creature_visuals/osty.tscn
[OstyDefectImpostor] Replaced full body branch with genuine unique SpineSprite inside an identity body group, local scale 0.62; original root and markers retained.
[奥斯提] 重建身体完成：类型=NCreatureVisuals，Spine=True，子节点=OstyDefectReviveFragments(Node2D)、OstyDefectBodyGroup(CanvasGroup)、Bounds(Control)、CenterPos(Marker2D)
`
画面为独立 Mod 自己的鸡煲身体（黄/蓝外壳、蓝球），不是猫、也不是原版幽灵。

### 10.5 名字变「坚果」不是本 Mod 的问题
D:\steam\steamapps\workshop\content\2868840\3747561432\midori_black_localization.json 是**独立 Mod**「Midori Black - Character Name Localization」（"description":"角色名称替换补丁：奥斯提→坚果，亡灵契约师→明前奶绿"，has_dll:true），游戏把它当第 168 个 Mod 加载并 Harmony.PatchAll 了 LocString.GetFormattedText()：
`
168: Midori Black - Character Name Localization (midori_black_localization)
[LocalizationPatch] ✓ Patched LocString.GetFormattedText()
`
它是**无差别重命名**，与皮肤选择无关；要还原名字只能在「模组配置」里关掉这个名称补丁（属于第三方 Mod，我们不宜去拦别人的 Harmony 补丁）。
## 十一、1.0.30.195：名字只在选猫时改 + 联机重建去重

### 11.1 名字问题的最终形态
第三方 Mod `midori_black_localization`（3747561432 目录下的独立子 Mod，第 168 个加载）会改 `LocString.GetFormattedText`。
探针证明它连 `GetRawText` 都已经是「坚果」，即**本地化表在加载阶段就被替换**，所以「读原表还原」不可行。

### 11.2 修法
新增 `Core/CompanionDisplayNameGuard.cs`：
- `[HarmonyPatch(typeof(LocString), nameof(LocString.GetFormattedText))]` + `[HarmonyAfter("midori_black_localization")]` + `[HarmonyPostfix][HarmonyPriority(Priority.Last)]`；
- 只处理 `LocEntryKey == "OSTY.name"`；
- 奥斯提分组当前选中的提供者包含 `midori` → 保留「坚果」；否则从**游戏原包** `res://localization/<lang>/monsters.json` 读回原名（`SkinService.TryReadBaseGameResource` + `JsonDocument`），缓存一次。
- 实机：皮肤=游戏默认 → 面板标题「奥斯提」；皮肤=midori_black → 「坚果」。

### 11.3 联机卡顿量化（`godot.log` 20.5 MB / 15.9 万行）
- `外观重建合计` **3626 次**，平均 **59.4 ms**，累计 215.5 s；
- 触发源 top：`联机外观定时校准` 553 次、`已刷新联机玩家 … 的头像` 300 次、`应用变换` 472 次；
- 单次成本：`提权完整包` 42–49 ms、`本地化` 259–496 ms（个别大重建 2 s 级）。

### 11.4 已做的两条优化
1. `Core/MultiplayerSkinSync.cs`：`ReconcileRemoteAppearances` 按「选择签名」去重，签名未变不再排队重建（8 秒自愈不再空转）。
2. `Core/SkinService.cs`：整包提权改成「本次会话已提权过就跳过」，避免每轮重复 `LoadResourcePack`。

### 11.5 验收
- 1.0.30.195 构建 0 错误 3 警告（既有 CS8601）、`LogicTests` 全绿、部署 4 处（DLL SHA-256 前缀 `1C71092D424F`）；
- 实机：`猫 → 撑在地上的鸡煲` 显示独立 Mod 身体（`Replaced full body branch …` + `重建身体完成：Spine=True`）；名字按选择切换。
- 联机性能需真实双人局复测（单人局无法触发 `ReconcileRemoteAppearances`）。

## 十二、1.0.30.196：游戏默认皮肤、局内随机项、Orchis 特效排查

### 12.1 奥斯提「游戏默认」显示的不是原版
`Ui/CharacterAppearanceRuntime.cs` 的 `RefreshCompanionGroup(groupId)` 开头新增：当该分组选中的是 `SkinCatalog.BaseOptionId` 且 `RebuildSummonBodyForImpostor(groupId) > 0` 时直接返回，强制按游戏原包还原召唤物身体，避免复用上一套皮肤污染过的规范路径缓存。
实机（`猫 → 游戏默认`）：`[原版] 别名场景已就绪：res://__skin_changer_base/BB82E24D637D864C/osty.tscn` → `[原版] 已把别名场景登记为规范路径缓存` → `[奥斯提] 重建身体完成：…子节点=Visuals(Node2D)…`，画面回到原版奥斯提（蓝焰手）。

### 12.2 局内奥斯提下拉里的「随机皮肤」
`Ui/CharacterAppearanceScreen.cs` 的 `PopulateSkinDropdown` 不再给伙伴分组追加「随机」项（只保留在选人界面）。实机下拉只剩：游戏默认 / midori_black / Orchis Necrobinder Skin Mod / 会画画的Small_day / 撑在地上的鸡煲 / 创意工坊。

### 12.3 骨妹用 Orchis 皮肤出现原版特效
- 回退了 1.0.30.195 引入的「整包已在栈顶就跳过提权」优化（`Core/SkinService.cs`）：校正覆盖层每次都挂在整包之上，跳过重挂会让整包沉到覆盖层下面，VFX/资源被基线覆盖层盖回原版。日志已恢复每轮 `已直接复用完整外观资源包 OrchisNecrobinderSkinMod.pck`。
- 排除的解释：用 `tools/PckInspect` 列 `OrchisNecrobinderSkinMod.pck`（174 文件），它对游戏规范路径的覆盖只有 11 个 `.import`（选角头像、地图标记、三张遗物图、联机猜拳手、角色图标），**没有**规范路径的 VFX；所以不是 `BuildGameBaselineOverlay` 把这些特效还原了。
- 待确认：作者所指「原版特效」具体是哪一个（卡牌/攻击命中/遗物/入场），需要具体复现才能定位是 `ManagedSkinModLoader` 的补丁分类（`IsManagedNodeReadyPresentationPatch`）还是视觉会话的接管范围。

## 十三、1.0.30.197：奥斯提根源位移（独立 Mod 的宠物摆位补丁）

### 13.1 取证
- 反编译 `OrchisNecrobinderSkinMod.dll`（`%TEMP%\orchis_src\OrchisNecrobinderSkinMod.Scripts\Entry.cs`）：它有三个补丁会动奥斯提——
  - `Creature.CreateVisuals` Postfix：给 Osty 换骨架后 `nodeOrNull.Position += OstyCombatPositionOffset`；
  - `NCreature._Ready` Postfix：`ApplyNecrobinderCombatScale` + `ScheduleCombatOstyOrdering`；
  - `NCreature.OstyScaleToSize` **Prefix**：直接 `TweenProperty(__instance, "position", playerNode.Position + GetOstyOffsetFromPlayer(...))`。
- 游戏原版 `NCreature.OstyScaleToSize`（`sts2.dll`，`data_sts2_windows_x86_64\sts2.dll`）也 tween 根节点 position，但只在血量变化时按固定偏移摆一次；原版 `AnimShake` 只动 `Visuals.Position`，不动根节点。
- 实测（新增 `%TEMP%\sts2_watch.txt` 监视钩子，写 `%TEMP%\sts2ctl\watch.log`）：启用 Orchis 时奥斯提**根节点**从 `x=-145.6` 一路滑到 `x=-400.6`；未启用时稳定在 `-374.3` 不动。血条是根节点的子节点，所以整条跟着滑 —— 与作者描述一致。

### 13.2 修法（`Core\ManagedSkinModLoader.cs`）
1. `MonsterVisualCallback` 新增 `OstyPositioning`，`ClassifyMonsterVisualCallback` 把 `NCreature.OstyScaleToSize` 归类为它；
2. 新增 `IsOstyPositioningPatch` / `ProviderOwnsOstyPet(providerId)`：只有「当前 osty 分组选中的提供者就是它」时才保留该补丁，否则并入 `managedPatches` 一并 `Unpatch`；
3. `Ui\CharacterAppearanceRuntime.cs` 的 `GetSelectedCreatureProviderIds`：宠物分组已经自己选了提供者时（`PetGroupOwnsProvider`），**不再**把主人（necrobinder）的提供者追加到宠物上，避免主人的 `NCreature._Ready` Postfix 被 `ReplaySelectedNodeReadyBehavior` 重放到宠物身上。
4. 附带：`STS2SC_DEBUG=1` 时输出每个提供者保留的行为补丁清单（`[调试] … 保留行为补丁：… → …`），便于以后定位「哪个第三方补丁在动我的模型」。

### 13.3 复测
- 1.0.30.197 部署后：静止 18 s，奥斯提根节点 `root=(-400.6,125.0)` **恒定不变**；结束回合 18 s 内仅在 `-399.9 ~ -400.6` 之间变化（< 1 px，属正常动画收尾）。
- 切皮肤后的 20 s 会看到一次「滑入就位」（-145 → -400），属重建后的归位缓动，不是攻击抖动。
- 构建 0 错误 3 警告（既有 CS8601）、`LogicTests` 全绿，DLL SHA-256 前缀 `ADF4844F0F8C`，部署 4 处。

### 13.4 仍未解决
- 「骨妹本体用 Orchis 皮肤出现原版特效」：已排除「规范路径 `.import` 被基线还原」（Orchis 包只覆盖 11 个 `.import`，无 VFX）；Orchis 的 Osty 相关补丁现已隔离，但玩家本体的特效来源仍未定位，等作者提供具体是哪个特效/哪一帧的截图再继续。

## 十四、1.0.30.198：Orchis 本体头顶蓝火（原版特效）真正的根因

### 14.1 现象
骨妹本体选 `Orchis Necrobinder Skin Mod` 后，模型是 Orchis 的白发少女，但**帽子上面仍然挂着一团原版蓝火**。
这团火的节点是 `Visuals/HeadBoneNode/SteppedFireMix_dark`（外加 `LowHealthParticles`、`HurtParticles`），Orchis 的 `Creature.CreateVisuals` 后置处理里 `ApplyNecrobinderPlayerSkeleton` 会 `RemoveChildNodes(body, "HeadBoneNode")` 把它删掉。

### 14.2 根因
`Core/SkinService.cs` 的 `ApplySelectedVisualPostfix` 里，回放提供者视觉后处理的条件是：

```csharp
var allowed = catalog?.IsRuntimeProviderOption(groupId, selection) == true &&
              (catalog.IsCompanionGroup(groupId) ||
               !catalog.ProviderUsesFullRuntime(selection) ||
               catalog.IsFullRuntimeProviderFullySelected(selection, GetVisualSelections()));
```

Orchis 是「完整运行时」提供者，而局内把奥斯提分给了 `撑在地上的鸡煲`（`OstyDefectImpostor.ground_defect`），于是
`IsFullRuntimeProviderFullySelected` 为假 → `providerId = null` → **骨妹分组的视觉后处理一次都没回放**，
`HeadBoneNode` 留了下来。之前 1.0.30.175 修的是「随从分组只看自己的选择」，同一个坑在角色分组上又踩了一次。

### 14.3 修法
把分组**自己的选择**当作唯一授权：只要该分组当前选择的选项能解析出运行时提供者，就回放它的 `CreateVisuals` 后置处理（注释写明了原因）。
另外在 `Ui/CharacterAppearanceRuntime.cs` 的 `TryRebuildCreatureVisuals` 里，`AttachReplacementCreatureVisuals` + `MoveChild` 之后补一次
`ContextualSkinControls.ApplySelectedCreatureVisualPostfix(creature.Entity, ref attachedVisuals)`——热重建出来的实例不在游戏的创建路径上，提供者的收尾处理从没见过这棵树。

### 14.4 验证
- 日志：`[外观诊断] 后置重放前后 HeadBoneNode：False → False`（修复前是 `True → True`）。
- 截图：Orchis 帽子上的蓝火消失，镰刀/裙摆正常；奥斯提仍是「撑在地上的鸡煲」，面板标题仍是「奥斯提」。
- 1.0.30.198：构建 0 错误 3 警告（既有 CS8601）、`LogicTests` 全绿、部署 4 处、DLL SHA-256 前缀 `74E84AAE0273`。

## 十五、1.0.30.199：重启后皮肤被重置 & 选角界面图标刷屏

### 15.1 「退出重进皮肤变回原皮」
- 现象：给骨妹选 `OrchisNecrobinderSkinMod`，退出重进后变回原版；配置里 `necrobinder` 被写回 `__base__`。
- 日志证据：启动时 `已将 OrchisNecrobinderSkinMod 的 1 个不完整联动选择恢复为游戏原版，避免界面显示已选但实际运行时未启用。`，随后 `已移除 5 个提供者皮肤呈现补丁…`。
- 根因：启动清理用严格版 `Catalog.IsFullRuntimeProviderFullySelected(providerId, Config.Selections)` 判定完整性；奥斯提分组此时选的是 `midori_black`，于是 Orchis 被判成「不完整联动选择」→ 整个提供者被回滚成原版。
- 修法：改用 `Catalog.GetFullySelectedFullRuntimeProvidersIgnoringCompanions(Config.Selections)` 的结果集判定——随从分组有自己的挂载与授权，不参与角色提供者的完整性判定。
- 验证：重启后配置保持 `necrobinder=OrchisNecrobinderSkinMod`、`osty=midori_black`，日志不再出现「不完整联动选择」。

### 15.2 选角界面 `char_select_random_character.png` 刷屏与卡死
- 现象：选角界面反复报 `ERROR: Error loading resource: 'res://images/packed/character_select/char_select_random_character.png'`，栈为 `SkinService.LoadCharacterSelectIcon` → `GetCharacterSelectIcon` → `ContextualSkinControls.RefreshAllCharacterButtonIcons` → `RebuildCharacterDisplay`；严重时窗口 `Responding=False`、内存涨到 2 GB 以上。
- 根因：随机角色槽位没有这个命名的图标文件，`ResourceLoader.Load` 每次都报错，而字符按钮刷新会反复触发。
- 修法：`LoadCharacterSelectIcon` 先 `ResourceLoader.Exists(path)`，缺失就记进 `MissingCharacterSelectIcons` 并返回 null（`catch` 也记录），后续刷新直接短路。
- 验证：`char_select_random_character` 报错计数归零。

### 15.3 仍未解决：选角界面预览小人头顶的原版蓝火
- 局内在 1.0.30.198 修好后，选角界面的预览小人仍带 `Visuals/HeadBoneNode`（诊断 `HeadBoneNode：True → True（分组=necrobinder）`）。
- 已知机制：预览由 `Core/FrameworkModelPreview.CreateVisuals` 用私有工厂装配（`TryInstantiateSelectedCharacterCreatureVisuals`），不在游戏 `Creature.CreateVisuals` 的调用路径上；而 Orchis 删火焰的动作写在自己的 `CreateVisuals` 后置补丁里。
- 关键约束：选角界面会把「皮肤代码行为」收窄到当前可见的外观组（`SkinService.ActivateRuntimeProviderBehaviorsForGroups` 的日志 `已按选角预览将皮肤代码行为收窄到 1 个可见外观组`），`ManagedSkinModLoader.GetOrLoadProviderAssembly` 在 `CanActivateBehavior=false` 时直接返回 null，于是预览里既没有真实补丁、也无法回放。
- 下一步方向（未实施）：预览期间临时激活被预览分组的运行时提供者（`ManagedSkinModLoader.ActivateSelectedProviders`），或让回放走一条「只加载程序集、不装补丁」的反射通道。

## 十六、1.0.30.198（回退版）：按作者要求撤销 1.0.30.199 的两处改动

作者反馈「好像已经快改崩了」，要求回退到上一个版本（1.0.30.198）。本次只做回退，没有新增功能。

### 16.1 撤销的改动

1. `Core/SkinService.cs` 启动完整性判定：`Catalog.GetFullySelectedFullRuntimeProvidersIgnoringCompanions(Config.Selections)` → 恢复为 `!Catalog.IsFullRuntimeProviderFullySelected(providerId, Config.Selections)`。
2. `Core/SkinService.cs` 的 `LoadCharacterSelectIcon`：移除 `ResourceLoader.Exists` 预检与 `MissingCharacterSelectIcons` 记忆集合，恢复直接 `ResourceLoader.Load<Texture2D>(path, null, CacheMode.IgnoreDeep)`。

### 16.2 回退后重新出现的行为

- 场景：角色分组用运行时提供者（如 Orchis），随从分组选了别的皮肤（如 osty = midori_black）时，启动会打印 `已将 … 的 1 个不完整联动选择恢复为游戏原版…` 并把角色皮肤回滚成原版 ⇒ 「退出重进皮肤变原皮」复现。
- 选角界面 `char_select_random_character.png` 的资源错误会重新刷屏（可能拖慢/卡住选角界面）。

### 16.3 验证与交付

- 版本 4 处 1.0.30.198：`STS2SkinChanger/Entry.cs:20`、`STS2SkinChanger/STS2SkinChanger.csproj:10-13`、`STS2SkinChanger/SkinChanger.json:7`、`workshop/workshop.json:7`。
- 构建 0 错误 3 警告（既有 CS8601）；`LogicTests` 全绿（`Skin Changer logic policy tests passed.`）。
- 部署 4 处，DLL SHA-256 前缀 `BB844DE5F630`。注意：与首次 1.0.30.198 交付的 `74E84AAE0273` 不同（回退代码的注释/换行与原始提交不完全一致，行为等价）。

### 16.4 未解决（回退不影响）

- 选角界面预览小人仍带原版蓝火；已确认预览路径下 `SkinService.ApplySelectedVisualPostfix` 一次都没被调用（探针计数 0），方案 A 两版实现均无效。
- 选角界面偶发卡死（`Responding=False`、内存约 2.1 GB），与方案 A 无关（只加探针日志的构建同样复现）。

## 十七、1.0.30.200：只修「退出重进皮肤变回原皮」，不动其它已修好的功能

### 17.1 问题

角色分组选运行时提供者（Orchis）、随从分组选别的皮肤（osty = midori_black）时，启动清理把 Orchis 判成「联动选择不完整」，于是把它拥有的 `necrobinder` 分组重置为游戏原版：

```
[INFO] [SkinChanger] 已将 OrchisNecrobinderSkinMod 的 1 个不完整联动选择恢复为游戏原版，避免界面显示已选但实际运行时未启用。
```

### 17.2 本版改法（只改一行判定 + 一个访问修饰符）

- `Catalog/SkinCatalog.cs`：`IsFullRuntimeProviderSelectedIgnoringCompanions(string, IReadOnlyDictionary<string,string>)` 由 `private` 改为 `public`（逻辑不变，仍是「只统计非随从分组」）。
- `Core/SkinService.cs:8234-8240` 启动清理的完整性判定：

  ```csharp
  .Where(providerId =>
      !Catalog.IsFullRuntimeProviderSelectedIgnoringCompanions(providerId, Config.Selections))
  ```

  即「随从分组自己选的皮肤，不能把角色的运行时提供者算成不完整」。

### 17.3 为什么不用 1.0.30.199 的写法

1.0.30.199 是在启动清理里改用集合版 `GetFullySelectedFullRuntimeProvidersIgnoringCompanions(...)` 并对每个 provider 做 `Contains` 判定；语义相同但多建一个集合，而且同批还改了选角图标加载（作者认为「快改崩了」，已整批回退）。本版只保留这一处语义修正，不重新引入图标改动。

### 17.4 回归保护（明确没动的部分）

- `SkinService.ApplySelectedVisualPostfix` 的 `allowed` 判定（Orchis 局内蓝火修复）未改；
- `CharacterAppearanceRuntime.RefreshCompanionGroup` 的「游戏默认 → 强制按原包重建身体」未改；
- `CharacterAppearanceScreen.PopulateSkinDropdown` 去掉伙伴分组「随机」项未改；
- 别名场景 / 规范路径缓存 / 提权顺序等一律未改。

### 17.5 验证

- 构建 0 错误 3 警告（既有 CS8601）；`LogicTests` 全绿。
- 手动把配置写成 `necrobinder=OrchisNecrobinderSkinMod`、`osty=midori_black` 后启动：启动后配置**保持不变**，`不完整联动选择` 日志计数 **0**，日志确认 `内测版本 1.0.30.200`。
- 版本 4 处 1.0.30.200；部署 4 处，DLL SHA-256 前缀 `25D454CC2125`。

## 十八、1.0.30.201：撤销 .200 的完整性放宽（随从选择把角色本体带成 Orchis）

### 18.1 现象

作者反馈（m02551）：「不行，你还是改掉了最重要的一点——之前我着手修复的，如果奥斯提用了orchis皮肤，那么进入游戏后的骨妹本体也会被替换成orchis这个bug」

### 18.2 根因

- 1.0.30.200 把启动清理的完整性判定换成 `IsFullRuntimeProviderSelectedIgnoringCompanions`（忽略随从分组）。
- 完整性判定决定该提供者的运行时行为是否保持激活。Orchis 是「完整运行时提供者」，激活后会安装进程级 Harmony 回调（`Creature.CreateVisuals` / `CharacterModel.CreateVisuals` / `NCreature._Ready` 等），其中玩家本体分支会把骨妹换成 Orchis 骨架。
- 因此 `osty = Orchis`、`necrobinder = 其它皮肤` 时，Orchis 因「随从选了它」被视为完整 → 保持激活 → 全局回调作用到骨妹本体上。放宽判定等于把这道唯一的防线拆掉。

### 18.3 修法

`STS2SkinChanger\Core\SkinService.cs:8229-8242` 恢复严格判定：

```csharp
.Where(providerId =>
    !Catalog.IsFullRuntimeProviderFullySelected(providerId, Config.Selections))
```

`SkinCatalog.IsFullRuntimeProviderSelectedIgnoringCompanions` 保留为 `public`（当前无调用方），便于后续做「按生物作用域」的精细化方案。

### 18.4 代价（须知）

严格判定与「角色皮肤重启后保持」在同一个开关上：`necrobinder = Orchis` 且 `osty = 其它皮肤` 时，Orchis 同样被判不完整，启动时仍会被停用（即 .200 想修的那条会回来）。两者要同时成立，必须让提供者的玩家本体回调「只作用于选择了它的生物」，不能靠放宽完整性判定。

### 18.5 验证

- 配置写成 `necrobinder = necrobinderSkin_NSFW.Ver.2.0`、`osty = OrchisNecrobinderSkinMod`（即泄漏场景）后启动：
  - `已移除 5 个提供者皮肤呈现补丁，保留其余 DLL 功能；目标：CharacterModel.CreateVisuals、NCharacterSelectScreen.SelectCharacter、NCreature._Ready、NCreature.OstyScaleToSize、NRestSiteRoom._Ready；来源：OrchisNecrobinderSkinMod=4、RestSiteFondleCursorMod=1`
  - `已低优先级挂载随从皮肤包 OrchisNecrobinderSkinMod.pck：该包只用于随从自己的私有资源，不会覆盖角色已选的皮肤。`
  - 启动日志 `内测版本 1.0.30.201`
- 构建 0 错误 3 警告（既有 CS8601）；`LogicTests` 全绿；版本 4 处 1.0.30.201；部署 4 处，DLL SHA-256 前缀 `FBB05DC56986`。
