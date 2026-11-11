# 联机里远端 Defect 的充能球皮肤不生效（本地正常）

- 状态：**已修复**（1.0.27.80，待实机验证）
- 来源：作者本人双开实测；皮肤 `DeepseekWhaleDefect`（工坊 3810578000）

## 现象

A 机 Defect 用 `DeepseekWhaleDefect`：A 自己看到的充能球是皮肤版（鲸鱼球），
B 机（观察方）看到的**战斗、角色都正常，只有充能球还是原版**。

## 根因

充能球的节点由 `OrbModel.CreateSprite()` 生成：

```csharp
private string SpritePath => SceneHelper.GetScenePath(
    "orbs/orb_visuals/" + base.Id.Entry.ToLowerInvariant());

public Node2D CreateSprite() =>
    PreloadManager.Cache.GetScene(SpritePath).Instantiate<Node2D>(...);
```

`SpritePath` 是**按球种**的规范路径（`res://scenes/orbs/orb_visuals/<球>.tscn`），
皮肤包正是替换这些规范场景。问题有三层：

1. `CreateSprite` 是在**任何玩家作用域之外**被调用的（球是"某玩家的球"，但创建时不带玩家上下文），
   所以远端玩家的皮肤资源没被挂载/选中；
2. 这些规范场景**按路径**缓存，房间预加载或第一次本地创建时原版场景已经进缓存；
3. `scenes/orbs/orb_visuals/*` 不在角色外观选项的资源清单里（静态审计里显示为 `unserved`），
   所以运行时只剩"整包挂载"这一条路能提供它们——而整包挂载只在玩家作用域内生效。

日志佐证：`Asset not cached: res://scenes/orbs/orb_visuals/lightning_orb.tscn`（走 AssetCache 规范路径）。

## 修复（1.0.27.80）

新增 `MultiplayerOrbSpriteScopePatch`（`OrbModel.CreateSprite` 前缀/终结器）：

1. 从 `OrbModel.Owner` 取这颗球的所属玩家；
2. 在该玩家的**运行时作用域**内创建（`MultiplayerSkinSync.BeginPlayerRuntimeScope`），
   皮肤提供者的整包资源因此可见；
3. 用与火堆同款的 `RemoteRoomSceneLease` 把缓存里的原版球场景临时替换成皮肤场景，
   创建完成后**恢复**原缓存（节点已实例化，后续本地加载不受影响）；
4. 诊断日志：
   `联机远端充能球场景已切换：“defect”的玩家使用 DeepseekWhaleDefect，房间场景取皮肤资源…`

所属玩家为空（可变的球模型、预览实例）时直接跳过，不影响单人/本地表现。

## 复现 / 验证

复现（1.0.27.79 及更早）：

1. A 机 Defect 选 `DeepseekWhaleDefect`，联机开局；
2. A 把球充起来（闪电/冰霜等）；
3. B 机看 A 的球：角色是鲸鱼、球还是原版。

验证（1.0.27.80）：

1. 同样操作：B 机看到的球应变成皮肤的球（形状/颜色/光效随皮肤）；
2. B 机日志出现 `联机远端充能球场景已切换：…`；
3. A 机本地看到的球不变；单人模式不变；
4. 两名 Defect 玩家使用**不同球皮肤**时各自显示各自（作用域按玩家隔离，缓存按路径临时替换后恢复）。

## 已知边界

- 只对"把整包资源挂上来替换规范球场景"的皮肤包有效；如果某包只提供球图标（`images/orbs/*.png`）
  而不提供 `scenes/orbs/orb_visuals/*.tscn`，则球体外观变化仍需包作者补场景；
- 缓存替换是临时的，作用域外不会把远端皮肤泄漏给本地玩家。

## 1.0.27.81：从"修好这一例"改成通用规则

上一版之所以生效，靠的是这个皮肤包属于"全量运行时提供者"（整包挂载）。为了让以后任何同类包都自动生效，
补了两条通用规则：

### 1. 皮肤包对"自己没声明的规范路径"也生效

`SkinCatalog.BuildRuntimeResourceOverlay` 之前只认外观选项里声明过的资源
（战斗/选人/火堆/商人等），球场景（`scenes/orbs/orb_visuals/*`）、UI 场景之类
在静态审计里一直是 `unserved`，只有整包挂载才能救。现在解析顺序变成：

```
provider relic 资源 → 在该选择的提供者 PCK 里查这个规范路径 → 选项声明的资源 → 游戏基线
```

也就是说：**请求某个规范路径时，先看这位玩家所用皮肤包的 PCK 里有没有同名文件**。
不需要包作者把球场景登记进清单，也不需要维护球种白名单。

### 2. 同一条规则覆盖充能球图标

球图标走 `OrbModel.Icon`（`res://images/orbs/<球>.png`，`GetCompressedTexture2D`）。
新增 `MultiplayerOrbIconScopePatch` + `RemoteRoomSceneLease.BeginTexture`：
在球的所属玩家作用域内替换成皮肤包的图标，用完恢复缓存。

### 现在的覆盖范围（按"所属玩家"解析的规范路径）

| 场景 | 钩子 |
| --- | --- |
| 火堆人物 | `NRestSiteCharacter.Create` |
| 商人房人物 | 联机商人循环（我们接管的那段） |
| 充能球本体 | `OrbModel.CreateSprite` |
| 充能球图标 | `OrbModel.Icon` |

以后遇到"某个皮肤包改了某个规范场景/贴图，但队友看不到"，只要它属于玩家拥有的物件，
把对应创建入口按同样方式挂上"所属玩家作用域 + 临时换缓存"即可，规则本身不用再改。
