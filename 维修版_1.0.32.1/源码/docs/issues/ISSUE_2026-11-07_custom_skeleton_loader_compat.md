# 与 Custom Skeleton Loader 同开时模型加载不正常

- 状态：**未解决**（1.0.27.72 已做首版兼容，待实机验证）
- 记录日期：2026-11-07
- 反馈来源：作者本人；问题清单第 7 条 / `docs/WORKSHOP_FEEDBACK_2026-08-30.md` F36（留言 TVsJohn 09-17）

## 那个 Mod 是干什么的

`Custom Skeleton Loader`（内部 id `CustomSkeletonLoader`，作者 **Khiral**，v0.0.1，依赖 `ModConfig`）：

- 不是工坊 Mod，发布在 **Nexus Mods 505**（https://www.nexusmods.com/slaythespire2/mods/505），
  国内有 3DM / 游侠等搬运。
- 把外部 Spine 素材（`.atlas` / `.png` / `.skel`）放进
  `mods\CustomSkeletonLoader\ReplaceResources\`，运行时按**骨骼名**替换游戏里的角色/怪物/UI 骨骼。
  它自带从 PCK 导出官方骨骼的工具（ModConfig 面板里），方便玩家做出替换包。
- 也就是说：它和换肤一样，**要独占同一个角色的整套外观**（骨架 + 图集 + 贴图）。

## 为什么和皮肤切换器一起就坏

它的 4 个替换入口全部收敛到同一个方法
`CustomSkeletonLoader.SkeletonLoader.TryBuildMegaSkeletonDataResource(name)`：

| 补丁 | 作用 |
| --- | --- |
| `AssetCache.LoadAsset` prefix | 所有 `.tres` 按文件名推出骨骼名，命中替换包就**直接返回自己的** `SpineSkeletonDataResource`（跳过原加载） |
| `AssetLoadingSession.AddToCache` prefix | 同上，替换已加载的骨架资源 |
| `NCreatureVisuals._Ready` postfix | 角色创建完再把 `SpineBody` 的骨架换成自己的，并重播当前动画 |
| `NSpineAutoPlayer._Ready` prefix | 命中时**跳过原 `_Ready`**，延迟换骨架 |
| 商人按钮/商人角色/商人手/Regent 选人背景 | 同规则 |

皮肤切换器选中某个皮肤时，皮肤包自带 `..._skel_data.tres`（骨架名与原版相同，如 `ironclad`），
于是两边同时想拥有这个角色：加载器把皮肤刚绑好的骨架（或皮肤包自己的 `.tres` 加载）换掉，
皮肤图集与骨架不匹配 → 模型错乱/绑定失败。

## 首版兼容（1.0.27.72）

新增 `Core/CustomSkeletonLoaderCompat.cs`：皮肤切换器启动时（或加载器稍后才被加载时）
用 Harmony 给**加载器自己的** `TryBuildMegaSkeletonDataResource` 加一个 prefix（不改对方任何文件）：

- 当前这个骨骼名对应的**外观组**如果本机正在显示皮肤（含联机“共享”模式下队友的皮肤），
  prefix 返回 `null` → 加载器所有入口都会走"没有替换"的兜底，皮肤资源原样保留；
- 该组用的是**原皮/游戏默认**时照常返回替换 → 加载器功能不变；
- 只对能映射到外观组的名字生效；映射不到的名字（它自己的其它骨骼）完全不受影响；
- 日志每个组只提示一次：
  `Custom Skeleton Loader 让位给皮肤：ironclad 当前使用皮肤选择，不再用 ReplaceResources 里的骨骼替换该角色。`

## 优先规则（明确写死）

**同一个角色：选了皮肤 → 皮肤赢；选原皮 → 加载器赢。**
不支持"某个皮肤的骨架 + 加载器的替换骨架"这种组合（两者都是整套外观，必须二选一）。
需要让加载器接管某个角色时，把该角色的皮肤选回“原皮/游戏默认”即可。

## 实机验证（注意：空 `ReplaceResources` 时加载器本身什么都不做）

1. **先准备一个替换包**：把 `正常.atlas / .png / .skel` 按加载器的目录约定放进
   `mods\CustomSkeletonLoader\ReplaceResources\`（它面板里能导出官方骨架做模板），重启游戏。
2. 铁甲战士选**原皮** → 模型应变成替换包的骨架（加载器生效），日志可见它的
   `[CustomSkeletonLoader] … replaced`。
3. 铁甲战士选**某个皮肤**（贞德 / 十字军等）→ 模型应是**皮肤自己的样子**，不能错位/丢贴图；
   日志出现 `Custom Skeleton Loader 让位给皮肤：ironclad …`。
4. 来回切换皮肤 ↔ 原皮数次、再重启一次：两个方向都要正常（皮肤不破、原皮能被替换）。
5. 联机：队友选了皮肤（共享模式），本机选原皮 → 加载器不应把队友换成替换骨架。

## 已知边界

- 如果加载器将来改了方法名/签名（例如 `TryBuild…` 改名），兼容会打印
  `Custom Skeleton Loader 兼容：未找到 …` 并跳过，不会报错但也不再生效。
- 只做“让位”，不做混合：不能同时显示“加载器的骨架 + 皮肤的颜色/贴图”。
- 名字映射靠外观组 id（`ironclad`、`merchant`、怪物 id 等）+ 去后缀/前缀；
  若某个名字对不上组，加载器仍会替换（可能再次出现错位），需要按实际名字补映射。

## 1.0.27.73 追加：之前为什么完全不生效

首版兼容只处理了"两边都加载之后谁赢"，但日志显示加载器**根本没被游戏加载**：

```
[SkinChanger] 已隔离皮肤提供者 CustomSkeletonLoader：视觉组=1 … 原 PCK 未全局挂载；
```

它被当成皮肤提供者接管了（DLL 由本 Mod 载入、PCK 不挂载、初始化器不执行），所以它的
`[CustomSkeletonLoader] Loaded successfully! N methods patched.` 从未出现——ReplaceResources 里放什么都没用。
触发条件很微妙：它的 pck 里只有 11 个空的本地化表；把 `ReplaceResources` 放进去素材后
`HasLooseAppearanceResource` 变真 → `BringsOwnAppearance` 为真 → 探测把它提升成"视觉组=1"。

修复：`Catalog/SkinCatalog.cs` 新增 `LooksLikeAssetPipelineTool(mod)`——元数据里若补丁目标是
`AssetLoadingSession`（或 `AssetCache` + `LoadAsset`），说明它接管的是**资源加载管线**（资源替换工具），
不是外观包；在"看起来像 DLL 皮肤但没有任何可呈现资源"的分支里直接交回游戏原加载器，并打印：

```
未按皮肤接管 CustomSkeletonLoader：它的补丁接管的是资源加载管线（AssetCache/AssetLoadingSession），
属于资源替换工具而不是外观包；已交回游戏原加载器，其 DLL 与 PCK 按作者原意完整加载。
```

离线核对（`RuntimeTests --audit-provider-probe`）：

```
CustomSkeletonLoader     => NO_PROVIDER（交回游戏）
JoanofArcIroncladHpwear  => PROVIDER visual=1 …（仍是皮肤）
CrusaderSkin             => PROVIDER visual=1 …
Merchant2CuteII          => PROVIDER visual=2 …
```

注意：作者的 `ironclad` 当前选择是 `__base__`（原皮），所以修复后加载器的铁甲战士替换应当生效；
一旦把铁甲战士选成某个皮肤，首版兼容会让加载器让位给皮肤（日志"让位给皮肤"）。
