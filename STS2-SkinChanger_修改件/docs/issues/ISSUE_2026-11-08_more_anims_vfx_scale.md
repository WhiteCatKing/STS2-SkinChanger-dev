# 装了角色皮肤后「More * Animations」系列的道具/特效被放大数倍

- 状态：**修复中**（补丁 Mod `MoreAnimVfxScaleFix` 0.4.0，待实机验证）
- 涉及：`More Ironclad Animations (ShieldOnly)` 3798368044、`More Silent Animations` 3821353824、
  `More Necrobinder Animations` 3816316338、`More Defect Animations` 3810696310
- 与皮肤切换器**无关**：这些 mod 全开、皮肤切换器关闭时同样复现

## 现象

装上角色皮肤后，这四个 mod 的持盾/道具/特效在战斗里被放大数倍（最直观是帝皇蟹之外那次截图里的巨盾：
原版约 150px，换皮后约 700px），角色本体大小却正常。不同皮肤、不同特效都这样。

## 根因

这四个 mod 把道具/特效做成**普通 `Sprite2D`/`Node2D`（固定像素）挂到角色 Spine 节点下面**：

- 铁甲：`ShieldOnly` 在角色 body（`SpineSprite`）下新建 `SpineSlotNode` 名为 `ShieldOnlyWeapons`，
  把盾/锤的 `Sprite2D` 作为它的子节点；
- 另外三个同作者 mod 同样用 `SpineSlotNode`（`MoreSilent*` / `Necro*` / `MoreDefect*` 前缀的节点）。

固定像素节点只继承 `SpineSprite.Scale`，**不继承骨骼单位**。而皮肤为了让自己的美术（骨骼单位较小）
撑出同样的剪影，会把 `Visuals` 的缩放调大：

| 场景 | `scenes/creature_visuals/<角色>.tscn` 里 `Visuals` 的 scale |
| --- | --- |
| 原版 ironclad | **0.28** |
| 原版 silent / regent | 0.29 |
| 原版 defect | 0.125 |
| 原版 necrobinder | 0.27 |
| 贞德皮肤（JoanofArcIroncladHpwear） | **0.780** |

于是道具被同比放大了 `0.78 / 0.28 = 2.79` 倍，而角色本体因为骨骼单位不同仍然一样大 —— 巨盾就是这么来的。

## 修复：新增独立补丁 Mod `MoreAnimVfxScaleFix`

源码：`STS2-SkinChanger_修改件\standalone\MoreAnimVfxScaleFix\`
产物：`mods\MoreAnimVfxScaleFix\{MoreAnimVfxScaleFix.dll, MoreAnimVfxScaleFix.json}`，
桌面另存一份 `工作2\MoreAnimVfxScaleFix_安装包\`。

做法（无 Harmony，只读扫描，不会和别的 mod 抢补丁）：

1. 每 0.5 秒遍历一次场景树，找出名字以 `ShieldOnly` / `MoreSilent` / `MoreDefect` / `Necro`
   开头、且挂在角色 `SpineSprite` 下的 `Node2D` 道具根；
2. 若该根是 `SpineSlotNode`（它每帧由骨骼槽驱动自己的变换，写 Scale 会被覆盖），
   就修正它**子节点**的 Scale；
3. 修正系数 `k = 原版Spine缩放 / 当前Spine缩放`，乘在节点自身的基准 Scale 上（基准记在 meta 里，
   所以反复扫描不会累积，切皮肤后自动重算）；
4. 系数为 1（没装皮肤，或皮肤没改缩放）时把 Scale 还原成基准，不做任何事。

覆盖的原版缩放表：ironclad 0.28 / silent 0.29 / defect 0.125 / necrobinder 0.27 / regent 0.29，
读取自游戏包里的 `scenes/creature_visuals/<角色>.tscn`（v0.111.0）。其它角色先不动。

## 验证

1. 装上并**启用** `MoreAnimVfxScaleFix`，重启游戏；
2. 日志应出现 `[MoreAnimVfxScaleFix] 已安装：…`；
3. 换贞德皮肤进战斗打「壁垒」：日志应出现
   `[MoreAnimVfxScaleFix] ironclad：ShieldOnlyWeapons 按 0.28/0.78 = 0.359x 修正（…）`，
   盾牌应回到原版大小；
4. 关掉所有皮肤（原版角色）再看：不应有任何修正（系数 1），道具仍是原样；
5. 另外三个角色（观者/骨妹/机器人）各换皮肤打一张带道具的牌，尺寸应正常。

## 备注

- 如果日志里只有「已安装」没有「修正」，说明道具节点名/父节点结构与假设不符，需要把该 mod 的实际
  节点名报过来，我再放宽匹配；
- 这四个 mod 的作者以后若自己修好了缩放，直接停用本补丁即可，不会互相干扰。

## 0.1.0 实测未生效（已升级到 0.2.0）

0.1.0 的日志只有 `已安装`，没有一条 `修正` —— 说明匹配条件没命中。日志确认补丁本身是加载了的
（`Calling initializer method of type MoreAnimVfxScaleFix.ModEntry`），所以问题在假设：

- 原来的假设是"道具挂在 `SpineSprite` **下面**"（用祖先查找 SpineSprite）；
  实际很可能是道具挂在 `NCreatureVisuals` 下面，而 SpineSprite 是它的**子节点**，祖先查不到 → 全部跳过。

0.2.0 的改动：

1. 改为**先找角色（NCreature），再在它的子树里找 `SpineSprite`**（不再依赖祖先关系）；
2. 道具识别放宽为：名字命中四个前缀 **或** 直接挂在 `SpineSlotNode` 里的 Node2D；
3. 新增一次性诊断日志：`[MoreAnimVfxScaleFix] 诊断：角色=… spine=… spineScale=… 已修正道具=…`
   后面跟最多 40 行 `节点 父/子 class=… scale=…`，即使没修好也能一眼看出真实节点名和类名。
## 0.2.0 仍然零触发 —— 真正的坑：mod 程序集没有注册给 Godot

0.2.0 的日志里补丁加载了、节点也在树里（`Node:MoreAnimVfxScaleFix×1`，见 P3 健康报告），
但 `诊断` / `修正` 一行都没有 —— 说明**节点的 `_Process` 根本没被调用**。

原因在游戏代码 `Main.cs:22`：

```csharp
ScriptManagerBridge.LookupScriptsInAssembly(typeof(GodotPlugins.Game.Main).Assembly);
```

游戏只注册了**它自己**的程序集。mod 的 DLL 是启动后才动态加载的，所以 mod 里的 C# 节点
（任何 `Node` 子类）收不到 `_Process` 等虚回调 —— 除非 mod 自己注册。主 Mod 早就对
提供者程序集做过这一步（`RuntimeMonsterVisualModeBridge.RegisterGodotScripts`），照抄即可。

0.3.1 的改动：

1. `ModEntry.Init` 里先反射调用 `ScriptManagerBridge.LookupScriptsInAssembly(自己的程序集)`，
   再创建运行节点；
2. 加启动自检：`运行时节点已收到第一帧（_Process 正常）`；
3. 每个道具用它**自己上方的 Spine 节点**算系数（皮肤可能留下旧 body 节点），
   取不到才退回角色子树里的第一个 Spine。
## 0.3.1 依然零回调 —— 换成 Harmony 驱动（0.4.0）

0.3.1 日志里 `已注册本程序集的 C# 脚本类型` 打印了，但**仍然没有** `已收到第一帧`，
也没有诊断。查我们自己的补丁 DLL：里面**没有 `ScriptPath` / `AssemblyHasScripts` 属性** ——
普通 classlib 不会跑 Godot 的 C# 源生成器，所以这些类型对 Godot 来说根本不是"脚本"，
`LookupScriptsInAssembly` 注册了个空，`_Process` 永远不会被调用。

0.4.0 不再依赖 Godot 节点回调，改用 Harmony（主 Mod 已验证可行）：

```csharp
[ModInitializer(nameof(Init))]  // Init 里 new Harmony(id).PatchAll(自己的程序集)
[HarmonyPatch(typeof(NRun), "_Process")]                      // 跑图期间
[HarmonyPatch(typeof(NEnergyCounter), "_Process")]            // 战斗中（能量球）
[HarmonyPatch(typeof(NEndTurnLongPressBar), "_Process")]      // 战斗中（结束回合按钮）
```

三个驱动都调同一个 `VfxFixer.Tick`，内部按 500ms 节流，所以重复调用只是一次时间戳比较。
这样即使某个节点在某场景不存在，另外两个也会把修正跑起来。