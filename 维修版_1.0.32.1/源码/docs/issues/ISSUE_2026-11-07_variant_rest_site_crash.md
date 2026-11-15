# 火堆/商人被变体后缀打断：`ironclad_3_rest_site.tscn` 加载失败导致火堆不能用

- 状态：**已修复**（1.0.27.78，待实机验证）
- 来源：玩家反馈（联机火堆"什么事也干不了"，单人正常），日志 `Downloads\godot.log`

## 现象

联机进火堆时，人物贴图缺失、火堆选项（回血/锻造/修补）全都不显示，只看到空场景；
单人正常。日志：

```
ERROR: Cannot open file 'res://scenes/rest_site/characters/ironclad_3_rest_site.tscn'
ERROR: Cannot open file 'res://scenes/merchant/characters/ironclad_3_merchant.tscn'
[ERROR] Failed to load resource synchronously: res://scenes/rest_site/characters/ironclad_3_rest_site.tscn
  at NRestSiteCharacter.Create(Player, Int32 characterIndex)
  at NRestSiteRoom._Ready(...)
```

## 根因（是我们自己的问题）

玩家把铁甲战士选成了 `JoanofArcIroncladHpwear::variant:3`（"霓虹审判官"）。
`ContextualSkinControls.ResolveVariantScenePath` 对**所有**以 `::variant:<后缀>` 结尾的选项都拼接

```
res://scenes/merchant/characters/ironclad_<后缀>_merchant.tscn
res://scenes/rest_site/characters/ironclad_<后缀>_rest_site.tscn
```

但两种变体的语义不同：

| 变体来源 | 语义 | 是否有独立场景 |
| --- | --- | --- |
| `ProviderVariantOptions`（同目录兄弟场景，如 CrusaderSkin） | 每个变体一个 `<group>_<suffix>.tscn` | 有，但**未必**带 merchant/rest 场景 |
| `ProviderSettingsVariantOptions`（贞德这种设置型调色板） | **同一个场景**，只是皮肤包自己的设置值不同 | 完全没有 |

贞德的变体是设置型的，后缀 `3` 只是调色板索引，被当成场景名去拼路径 → 文件不存在。
游戏在**预加载**阶段（`NAssetLoader`）就尝试加载它并失败，进而在
`NRestSiteCharacter.Create` 实例化时抛出 `AssetLoadException`；异常发生在 `NRestSiteRoom._Ready`
内部，房间初始化中断，所以火堆逻辑虽然算出来了但一个选项都没画出来。

单人没复现，是因为单人时那个玩家自己不是铁甲（或没选这个变体），联机里只要有一个铁甲玩家
挂着这个变体，**所有人**进火堆都会触发同一批预加载。

## 修复（1.0.27.78）

`Ui/ContextualSkinControls.cs`：

1. **设置型变体不再拼场景名**：新增 `ProviderSettingsVariantOptions.IsSettingsVariant(optionId)`，
   命中的选项（调色板变体）直接返回 `null`，走游戏/皮肤包自己的默认路径。
2. **场景型变体先验存在再使用**：`VariantSceneAvailable` 用 `ResourceLoader.Exists(path)` 检查
   merchant / rest / creature_visuals 的变体场景；不存在就返回 `null` 回退默认，
   并写一条日志：
   `变体场景不存在，已回退到默认外观：res://…/ironclad_3_rest_site.tscn（分组=ironclad，变体=3）。`
3. `ResolveVariantCombatPath` 同样加了这两道检查。

这样：皮肤包有变体火堆/商人场景时照旧生效；没有时自动回退，不再让整个房间初始化失败。

## 复现 / 验证

复现（1.0.27.77 及更早）：

1. 装 `JoanofArcIroncladHpwear`（战士贞德），铁甲战士皮肤选第 4 个变体「霓虹审判官」；
2. 联机开一局，让使用该变体的玩家进火堆（或商人房）；
3. 火堆场景没有人、没有选项，日志出现 `ironclad_3_rest_site.tscn` 加载失败。

验证（1.0.27.78）：

1. 同样的联机局面进火堆：火堆正常显示人物与三个选项（铁甲显示默认/皮肤本体的火堆外观）；
2. 日志出现一行 `变体场景不存在，已回退到默认外观：…`（说明回退生效）；
3. 换成 CrusaderSkin 这类**真有**变体场景的皮肤包并选变体：火堆/商人仍是该变体的外观，
   日志里**没有**上面那条回退提示。

## 顺手的说明（可直接贴给玩家）

> 已定位：不是游戏坏了，是我们变体注册时把"调色板变体"错当成"独立场景变体"，
> 去加载 `ironclad_3_rest_site.tscn` 这种不存在的文件，火堆房间初始化被打断。
> 更新到 1.0.27.78 即可；临时绕过：把铁甲战士皮肤切回原版（或选"原色/默认"变体），
> 只影响选了该变体的那名铁甲玩家，联机里其他人不受影响。

## 已知边界

- 变体包如果**只**给战斗场景、没给火堆/商人场景：现在会回退到默认外观（不是变体外观），
  这是皮肤包资源缺失，属于作者侧问题；
- `ResourceLoader.Exists` 在皮肤包尚未挂载时可能返回 false，此时同样回退默认，
  不会报错，只是该变体的火堆/商人外观不生效。
