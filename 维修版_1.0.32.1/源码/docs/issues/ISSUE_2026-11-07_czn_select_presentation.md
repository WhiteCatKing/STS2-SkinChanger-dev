# CZN 系皮肤（Mei Lin / Watcher）选人界面背景与大图头像仍是原皮

- 状态：**已定位，1.0.27.82 已做修复，待实机验证**
- 来源：作者本人实测；皮肤 `MeirinWatcherSkin`（工坊 3789216478，Chaos Zero Nightmare - Mei Lin）

## 现象

选人界面里，观者选 Mei Lin 后：**战斗小人已经变了**，但
选人背景、右侧大图头像（以及选人栏图标等）仍是原版观者。

## 日志证据

```
[CznCore] Character select background loaded: source=character_select
          scene=res://scenes/screens/char_select/meirin_live_bg.tscn variant=LiveLobby
[CznCore] Character select bg loop forced: found=4 started=4 missing=0 failed=0 prepared=6
[SkinChanger] 开始重建选角预览：watcher。
[SkinChanger] 已在恢复游戏基线后重放 MeirinWatcherSkin 的 1 个选角呈现步骤。
[SkinChanger] 已从隔离依赖路径重建完整 DLL 皮肤 WATCHER 的选角展示。
```

即：**先**由 CznCore 把背景/头像铺好（2455 行），**后**由皮肤切换器重建整个选人展示
（2459–2464 行）——重建时"恢复游戏基线 + 清理提供者残留"，把刚铺好的背景和大图一起清掉了。
战斗不受影响，因为战斗模型走的是另一条按玩家/按选择的场景接管链。

## 根因

Mei Lin 这类包不替换规范贴图/场景，而是**运行时构造**选人表现：

- `MeirinCharacterSelectPresentationProfile` 声明的是
  `res://scenes/screens/char_select/meirin_live_bg.tscn`、`meirin_static_portrait_bg.tscn`
  这类**由 CznCore 运行时搭建**的场景，以及 `MeirinWatcherSkin/images/...` 的自有贴图；
- 真正的铺屏动作是 `CharacterSelectPresentationRuntime.ApplyScreenSelection(screen, button, character)`，
  由皮肤包在游戏的 `CharacterSelectedEvent` 里调用（Meirin `Entry.cs:5900`）；
- 皮肤切换器随后重建选人界面（`已从隔离依赖路径重建完整 DLL 皮肤 …`），基线恢复把背景/头像清回原版，
  而 `ApplyScreenSelection` 只在"选中角色"那一刻跑过一次，不会再触发。

## 修复（1.0.27.82）

新增 `Ui/FrameworkSelectPresentationBridge.cs`：扫描已加载程序集里
类型名以 `CharacterSelectPresentationRuntime` 结尾的静态字段（每个 CZN 系皮肤包各持有一个实例），
在皮肤切换器重建完选人展示之后，对这些实例调用 `ApplyScreenSelection(screen, button, character)`，
让框架把背景、大图头像、图标重新铺回来。

- 不做包名硬编码：任何使用同一套共享运行时的皮肤包都会自动生效；
- 调用是幂等的（框架自己会先恢复原生宿主再添加，不会叠两层）；
- 任何一个包抛异常都被吞掉，不影响其它包与原版界面；
- 没有这类框架时扫描为空，什么都不做。

## 复现 / 验证

复现（1.0.27.81 及更早）：

1. 订阅 Mei Lin（3789216478）+ CznCore（3826011578），在选人界面给观者选 Mei Lin；
2. 观察：预览小人变成 Mei Lin，但背景、右侧大图头像还是原版观者。

验证（1.0.27.82）：

1. 同样操作：选人背景、大图头像、图标应一起变成 Mei Lin；
2. 切换其它角色再切回观者，仍然正常（不会残留上一个皮肤的背景）；
3. 其它非 CZN 皮肤（贞德、鲸鱼、商人等）表现不变。

## 已知边界

- 只对"通过共享框架运行时铺选人表现"的包有效；纯资源型皮肤本来就由资源接管，不受影响；
- 如果某个 CZN 系包把运行时实例放在实例字段而不是静态字段里，扫描不到，需要按它的结构补一条。
