# 商人娘化模型跑到左上角 / 连续进商店错位

- 状态：**未解决**（1.0.26.53 测试补丁已加入修复，待实机验证）
- 来源：留言 命 2026-09-15（“另一个娘化总会跑到左上角”）；讨论区 风色 2026-09-06「连续进入商店房间会导致商人模型错位，出现在左上角」（主题已被删除，只剩墓碑）
- 收录：2026-11-03；根因定位与补丁：2026-11-06

## 现象

商人娘化皮肤（至少“阿塔”系列与另一个娘化包）在连续进入商店房间后，模型位置错到屏幕左上角；有时只是位置不对，有时是换肤后没恢复。
2026-11-06 玩家补充（Merchant2CuteII + 0.111，`godot2026-11-06T11.24.47.log`）：**模型同时明显变大**，然后整体跑到屏幕左上角。玩家明确是“整个商人身体”，不是手部或腿部局部。

## 根因（已定位到代码路径）

### 1. 娘化商人的位置/缩放只是“被重放的前缀”，并非常驻状态

`Merchant2CuteII` 的 `NMerchantButton._Ready` 前缀做两件事：

```csharp
visual.Position += ModConfig.Merchant.VisualPositionOffset; // (+1260, +620)
visual.Scale     = ModConfig.Merchant.VisualScale;          // (0.18, 0.18)
```

原版 `merchant_button.tscn` 给 `%MerchantVisual` 的值是 `Position=(-1122.7, -396.68)`、`Scale=(0.470095, 0.470095)`。
也就是说：**只要这个前缀没跑，画面上的娘化骨骼就会用原版变换渲染——放大约 2.6 倍，并且落在左上方。** 这与玩家描述的“变大 + 跑到左上角”精确吻合（不是“只是没对齐”）。

皮肤切换器不会让这类 `NMerchantButton._Ready` 补丁常驻 Harmony：它由 `IsNodeReadyPresentationTarget` 归类为受管节点初始化，激活时被 `UnpatchProviderCallbacks` 卸下，改由 `MerchantRuntimeAppearance.BeginSelectedMerchantNodeReady`（`MerchantButtonProviderReadyBridgePatch` 前缀）用反射重放；重放产生的改动会被记成一条 `NodeReadyMutation`（`CaptureNodeReadyState` 覆盖整棵按钮子树，含 `%MerchantVisual` 的 `Position/Scale`）。

于是这个变换有两个致命特性：

- 它**只存在于一次重放里**，不作为常驻补丁存在；
- 它**会被通用还原逻辑整段撤回**。

### 2. 触发条件 A：`RestoreNodeReadyBehaviors` 在商店开着时还原了变换

`ManagedSkinModLoader.DeactivateProvider` 的清理第一步就是「恢复场景节点」→ `RestoreNodeReadyBehaviors(runtime)`，把该提供者记录的所有节点改动还原。只要提供者在商店仍在画面上时被停用，`%MerchantVisual` 立刻被写回 `(-1122.7, -396.68)` / `0.470095`，而屏幕上的骨骼仍是娘化骨骼——立刻“变大 + 左上角”，并且**只要这个商店房间实例还在就一直错**（重开商店界面不会重建按钮，所以“重新进入商店不恢复”）。

商店可见期间会改变提供者可见范围（`_runtimeProviderBehaviorScope`）的入口不止一个，包括局内其它皮肤/图鉴操作与房间作用域收尾；`RuntimeProviderScopePolicy.SelectActiveProviders` 只按“可见外观组”判断，而**商店不在对局的角色集合里**，所以任何一次不含 `merchant` 的范围收窄都会停用商人提供者并回写原版变换。

玩家日志 `godot2026-11-06T11.24.47.log`（0.111，Merchant2CuteII 1.6.0，SkinChanger 1.0.26.52）验证了前半段：读档续玩后立刻出现
`已停用未选中皮肤提供者 Merchant2CuteII 的 9 个行为补丁` 与 `已按对局角色将皮肤代码行为收窄到 2 个可见外观组`——也就是商人提供者在与商店无关的对局收窄里被整体停用。这份日志到退出为止没有进入商店（没有 `商店房间已绕过启动预加载缓存…` 那行），说明缺陷只会在之后某次商店/事件里显形，这也解释了“无法稳定复现”。

### 3. 触发条件 B：按钮重建时提供者已不活跃，前缀重放是空操作

`ReplaySelectedNodeReadyPrefixes` 在 `ActiveProviderRuntimes` 里找不到该提供者时直接返回空列表（无日志）。`BeginSelectedMerchantNodeReady` 拿到 `providerId` 后没有检查提供者是否活跃，所以“范围内的旧状态 + 之后重建按钮”的组合会得到一个**从未套用过皮肤变换**的按钮，同样呈现“原版缩放大小的娘化模型在左上角”。
`NMerchantRoom.Create` 前缀里的 `FocusRuntimeProviderBehaviorsOnRunContext([merchant])` 正常情况下会先激活提供者，因此该路径只在范围被别处收窄、或激活失败（`启用 X 的完整视觉会话失败`）时出现——两者都符合“偶发”。

### 4. 假商人：`_Ready` 顺序让它的提供者在按钮初始化时一定不在场

真商人靠 `NMerchantRoom.Create` 前缀先聚焦 `merchant` 分组、再实例化房间，所以按钮 `_Ready` 时提供者已经活跃。假商人只有 `NFakeMerchant._Ready` 前缀做同样的聚焦，而 Godot 的 `_Ready` 是**子节点先于父节点**：`MerchantButton._Ready` 运行时 `NFakeMerchant._Ready` 还没跑，`fake_merchant_monster` 的提供者此时不活跃 → 前缀重放是空操作 → 假商人模型同样停在原版变换上，位置和大小一起偏。这条路径是稳定复现，不是偶发；真商人则要等某个入口把 `merchant` 移出可见范围才会命中。
另外 `TryRefreshFakeMerchant` 热切换后缺少真商人路径里那句 `PartTakeoverVisualScope.Reconcile(...)`：作者自己的商人画面 / 按钮排版还原没有对新按钮再跑一次，画面留在了旧状态。

## 修复（1.0.26.53 测试补丁）

1. **不再回写仍然生效的商人变换**（`Core/MerchantFigureRestorePolicy.cs` + `ManagedSkinModLoader`）：
   `RestoreNodeReadyBehaviors(providerId, runtime)` 现在把正在停用的提供者 id 带进逐条还原；节点上由 `MarkMerchantFigureProvider` 记下的 `%MerchantVisual` 归属，如果该商人分组**仍然选中同一个提供者**（`MerchantFigureRestorePolicy.KeepsTransform`），就跳过它的 `Position/Scale` 还原。真正换皮肤、换提供者、重建按钮时照旧还原；纯策略函数已在 `LogicTests` 覆盖。
2. **重放前先保证提供者在场**（`MerchantRuntimeAppearance.EnsureSelectedMerchantProviderActive`）：
   按钮 `_Ready` 前若发现选中的商人提供者不活跃，先按当前商人分组重新聚焦一次范围，再重放前缀。这一条同时修掉假商人那条稳定路径（它的父节点 `_Ready` 永远晚于按钮）和真商人被别处收窄范围后的路径；重放本身仍走“先还原旧 mutation 再应用”，因此幂等，不会二次累加偏移。
3. **记录并输出实际变换**（`RememberMerchantFigureProvider`）：
   每次商人按钮初始化后写一行 `商人骨骼变换已应用：<provider>（<group>）位置=(x, y)，缩放=(s, s)`。以后玩家回报时，只要这行位置是 `(-1122.7, -396.7)` 或缩放是 `0.4701`，就说明皮肤变换没跟上；日志里同时能看到是否出现 `商人外观提供者 … 不在当前场景范围内，先重新启用`。
4. **假商人热切换补跑 `Reconcile`**：与真商人保持一致，重放作者画面与按钮排版。

## 待实机验证

- 连续进 3 家商店；在商店里切换任意其它皮肤、开/关图鉴、再切商人皮肤；确认商人不再变大、不再跑到左上角。
- 出问题时抓 `godot.log`，搜索 `商人骨骼变换已应用` / `商人外观提供者` / `恢复场景节点`。
- 假商人事件里同样切一次假商人皮肤，确认作者画面与按钮位置正确。


## 2026-11-06 实测补充：同 ID 差分包的第二种成因

作者同时安装工坊 `Merchant2CuteII v1.6.0` 和本地 `mods\商人n版`（v1.4.1，同 ID 差分包）后实测：只有 n 版错位，工坊版与 `商人“阿塔”` 都正常。
`godot.log` 显示 n 版的 DLL 因“与已加载程序集同名但内容不同”被有意排除执行（只供资源），而它把位置/缩放写在 `NMerchantButton._Ready` 前缀里，
于是按钮保留原版变换 `(-1122.7, -396.7) / 0.4701`，画面正好放大约 2.6 倍并落到左上角。该路径与本文前半部分“还原撤掉变换”无关，
已在 1.0.26.54 用“行为别名”修复，详见 `ISSUE_2026-11-06_duplicate_source_dll_behavior.md`。
## 关联

- 与 F21（商人皮肤换不了/每次重启还原）不是同一问题：F21 是持久化/接管路径，这里是运行时位置错位。