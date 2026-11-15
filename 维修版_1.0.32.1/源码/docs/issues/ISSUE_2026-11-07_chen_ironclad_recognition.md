# Chen Ironclad 皮肤"部分运行时识别不到"

- 状态：**已定位主要竞态，1.0.27.77 已做兼容 + 诊断，待实机验证**
- 反馈来源：未解决问题清单第 10 条（"Chen Ironclad"皮肤在部分运行时识别不到）
- 实包复核：2026-11-07，工坊 **3749575378**，`ChenIronclad` v1.1.0（作者 thunninoi）

## 这个 Mod 是什么

`Chen Ironclad`（内部 id `ChenIronclad`，工坊 3749575378 / Nexus 431 `Chen As The Ironclad`，作者 **thunninoi**）：
把铁甲战士整套换成《明日方舟》的陈（Chen The Dawnstreak）。

- 12.8 MB 的 `ChenIronclad.pck`：战斗模型、商人模型、火堆模型、选人背景、图标、卡框材质、
  卡牌飞行动画（card trail）、能量图标与 5 层球、石头剪刀布手部图；
- 17 KB 的 `ChenIronclad.dll`：继承框架的 `Chen_Skin : CharacterSkin<Ironclad>`，
  用 `[HarmonyPatch(typeof(SkinRegistry), "SkinDbSetup")]` 的 **postfix** 把皮肤注册进框架；
- 依赖：`BaseLib`（≥3.3.1）与 **`thunninoiSkinManager`**（皮肤框架）。

关键点：**它自己不直接生成外观**，而是把"皮肤 id → 资源路径"注册到皮肤框架里，
由框架在 `SkinDbSetup()` 时统一收集。皮肤切换器自带一个**行为隔离的 `thunninoiSkinManager.dll`
兼容层**（`FrameworkCompatibilityLayer`），就是为了在没有原框架时满足这个依赖。

## 为什么"有时识别不到"

注册链路是：

```
ManagedSkinModLoader 激活提供者（装好它的 Harmony 补丁）
  → FrameworkCompatibilityLayer.NotifyProviderActivated(assembly)
    → SkinRegistry.SkinDbSetup()   （内置兼容层里是空方法，只当钩子）
      → ChenIronclad 的 SkinDbSetup postfix 执行 → SkinRegistry.Register(SkinData)
```

`SkinDbSetup` 是**一次性钩子**：谁在它被调用的那一刻挂上了 postfix，谁就被注册。
三种情况会让陈的 postfix 没赶上：

1. 提供者在"激活"路径上补丁还没装好（另一个 Mod 抢先调用、或激活顺序不同）；
2. 原版 `thunninoiSkinManager` 在场时走"协作模式"，`HasRegistrationCallbacks` 一开始为 false，
   旧代码直接 **return 放弃**，之后不再重试；
3. 注册发生在模型库就绪之前（旧代码靠 `ProviderRegistrations` 延后，但只补一次）。

于是同一个人、同一套 Mod，某次启动能看到"陈"，某次看不到——即"部分运行时识别不到"。

另外还有一条**静默丢弃**路径：`SkinCatalog` 只接受"资源闭合"的框架契约
（`FrameworkContractResourceClosureComplete`），任何一项资源路径在包内和基线里都找不到时，
整个契约被丢掉且**没有日志**，表现也是下拉里没有这个皮肤。

## 修复（1.0.27.77）

1. **注册补做**（`FrameworkCompatibilityLayer`）：激活后在 +0.5s、+2.0s 各重跑一次
   `SkinDbSetup`（`Register` 按 skin id 去重，重复调用安全），并在协作模式下等
   `HasRegistrationCallbacks` 变真再补做；不再一开始就放弃。
2. **注册结果诊断**：补做时打印一行
   `框架皮肤注册统计（ChenIronclad.dll）：角色=1，皮肤=1：[chen]`
   —— 下次反馈只要带这行，就能一眼看出"框架到底收没收到"。
3. **契约丢弃诊断**（`SkinCatalog`）：框架契约因资源缺失被丢弃时打印
   `未接受 X 的框架皮肤 <skinId>：这些资源在包内与基线中都不存在：…`
   —— 把原来完全静默的"识别不到"变成可查的日志。

## 你要怎么复现 / 验证

复现（旧版 1.0.27.76 及更早）：

1. 装 `ChenIronclad`（已放入 `mods\ChenIronclad`），确保 `BaseLib` 在；
2. 启动游戏，进选人界面看铁甲战士的皮肤下拉：**有时**有 `Chen Ironclad`，**有时没有**；
   （也可以反复重启几次、或用 MLM 调整 Mod 顺序后再试——顺序变了命中率就变。）
3. 若没有：日志里既没有"已隔离皮肤提供者 ChenIronclad"，也没有任何解释。

验证（1.0.27.77）：

1. 重启游戏，选人界面铁甲战士皮肤下拉里应稳定出现 `Chen Ironclad`；
2. 日志应出现（每个会话一次）：
   - `已隔离皮肤提供者 ChenIronclad：视觉组=1, 卡图=13 …`
   - `框架皮肤注册统计（ChenIronclad.dll）：角色=1，皮肤≥1：[chen]`
3. 选中它进游戏：战斗/商人/火堆/选人背景/卡框/能量球/卡牌飞行都应是陈的外观；
4. 如果下拉里仍然没有：把日志给我——
   - 有 `未接受 ChenIronclad 的框架皮肤 …：这些资源…不存在` → 是资源闭合被拒，我按缺的路径补规则；
   - 有 `框架皮肤注册统计…皮肤=0` → 是框架注册没跑（补丁未生效），我改成强制加载它的初始化器；
   - 两行都没有 → 说明提供者根本没被探测到，我再查包识别。

## 已知边界

- 陈的外观完全由 `thunninoiSkinManager` 框架驱动，本 Mod 只保证"框架在、注册上"，
  不替换它自己的场景/动画逻辑；
- 玩家如果同时装了**原版** `thuninoi's Skinloader`（3749563676），会走协作模式，
  下拉里可能出现两套（框架自带的选择器 + 皮肤切换器）；本修复只保证注册不丢，
  不合并两套 UI。
