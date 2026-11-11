# 联机里远端玩家在火堆显示原皮（战斗正常）

- 状态：**已定位，1.0.27.79 已做修复 + 诊断，待实机验证**
- 来源：作者本人双开实测；涉及皮肤 `DeepseekWhaleDefect`（工坊 3810578000）

## 现象

A 机（Defect，皮肤 `DeepseekWhaleDefect`）与 B 机联机：
B 机在**战斗**里看 A 是正常的鲸鱼皮肤，但进**火堆**时 A 显示成原版 Defect；
A 机自己看自己正常。

## 日志证据

A 机日志（`godot.log`，16:51 会话）：

```
[INFO] [SkinChanger] 已提交本机联机皮肤同步（进入对局）：玩家=1；角色=DEFECT；皮肤=DeepseekWhaleDefect
[INFO] [SkinChanger] 已接收联机玩家 1000 的角色 IRONCLAD：皮肤=JoanofArcIroncladHpwear::variant:1 …
[DEBUG] [RestSiteSynchronizer] Beginning rest site
[INFO] Preloading 'RestSite Room' assets... count=4 vfx=2
[INFO] [SkinChanger] 已为联机玩家临时挂载 DeepseekWhaleDefect 的完整运行时资源
```

对皮肤包做静态审计（`RuntimeTests --audit-provider-details`）确认它**带**火堆资源：

```
served   res://scenes/rest_site/characters/defect_rest_site.tscn
served   res://animations/rest_site/defect/restsite_defect.atlas / .png / .skel
served   res://scenes/creature_visuals/defect.tscn
```

所以不是皮肤包缺资源。

## 根因

游戏的房间预加载在**任何玩家作用域之外**读取角色模型的
`CharacterModel.RestSiteAnimPath`（原版路径 `res://scenes/rest_site/characters/defect_rest_site.tscn`），
把**原版场景**放进 `PreloadManager.Cache`（`PreloadManager.Cache` 就是 `AssetCache`）。

之后 `NRestSiteCharacter.Create(player, index)` 才会进入我们为"这名玩家"开的作用域，但
它取场景的方式是 `PreloadManager.Cache.GetScene(player.Character.RestSiteAnimPath)` ——
**同一路径、已经缓存的是原版场景**，于是拿到的仍是原版。
战斗没有这个问题，因为战斗模型是 `NCreatureVisuals` 在作用域内即时创建的，不经过房间级预加载缓存。

顺带解释了"主机自己正常"：A 机是本地玩家，本地选择直接生效（并且本地路径会被我们的接管改写）。

## 修复（1.0.27.79）

新增 `Core/RemoteRoomSceneLease.cs`，在远端玩家的房间节点实例化瞬间：

1. 在该玩家的运行时作用域内，用 `SkinService.GetOrLoadRuntimeScene(group, canonicalPath)`
   取到"他的皮肤"的场景；
2. 若与缓存里的不同，临时 `PreloadManager.Cache.SetAsset(path, 皮肤场景)`；
3. `Create` 结束（作用域释放后）把缓存条目**恢复**回原来的场景——节点自己已经实例化好了，
   之后的本地加载仍然看到原版资源。

接在两处：`MultiplayerRestSiteCreateScopePatch`（火堆）和联机商人循环
（`MultiplayerMerchantPlayerVisualIsolationPatch`，商人房）。

同时打一行诊断：

```
[INFO] [SkinChanger] 联机远端火堆场景已切换：“defect”的玩家使用 DeepseekWhaleDefect，
       房间场景取皮肤资源而不是预加载的原版（res://scenes/rest_site/characters/defect_rest_site.tscn）。
```

## 复现 / 验证

复现（1.0.27.78 及更早）：

1. A 机 Defect 选 `DeepseekWhaleDefect`（工坊 3810578000），B 机随便；
2. 双开/联机开一局，一起进火堆；
3. B 机看 A：战斗里是鲸鱼、火堆里是原版 Defect。

验证（1.0.27.79）：

1. 同样局面进火堆：B 机（观察方）看到的 A 应为鲸鱼皮肤的火堆姿势；
2. B 机日志出现 `联机远端火堆场景已切换：…`；
3. 商人房同理（`联机远端商人场景已切换`）；
4. A 机本地、单人模式行为不变。

## 注意：双开时日志只有一个

双开时两个实例共用 `%APPDATA%\SlayTheSpire2\logs`，后启动的实例会覆盖 `godot.log`。
本次拿到的 `godot.log` 是 **Defect 那一侧**的，看不到"观察方"的解析过程。下次复现请：

1. 先让观察方（B）进火堆，然后在 B 的窗口里等 1 秒；
2. 立刻把 `%APPDATA%\SlayTheSpire2\logs\godot.log` 复制成 `godot_B.log`；
3. 再到 A 侧复制成 `godot_A.log` 一起发我。

## 已知边界

- 只处理"房间级预加载缓存把原版场景钉死"这一种；如果某皮肤包的火堆场景**不存在**，
  仍然按原版显示（这是正确回退）；
- 恢复缓存条目是尽力而为：若期间另有 Mod 覆盖同路径，以它为准。
