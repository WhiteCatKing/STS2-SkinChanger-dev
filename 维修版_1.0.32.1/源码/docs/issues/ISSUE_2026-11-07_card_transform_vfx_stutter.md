# 事件里“变化”卡牌时特效生硬 / 像没有特效

- 状态：**部分定位**（1.0.27.75 已做预加载 + 诊断日志，待实机确认）
- 反馈来源：作者本人 2026-11-07（“在‘事件’中‘变化’卡牌的时候，特效好像没了，变得很生硬”）

## 现象

在事件里选择“变化”卡牌（把一张牌随机变成另一张）时，原本的“卡牌放大 → 白光扫过 → 揭开新卡面”动画看起来
被跳过或很生硬，像是直接换了一张牌。

## 日志证据

`godot2026-11-07T13.36.log` 里那次变化（symbiote 事件，13:24 左右）确实播放了光效：

```
[INFO] Player 1 chose cards [STRIKE_IRONCLAD]
WARNING: 'res://sts2_skin_runtime/<session>/027_card_provider/images/atlases/card_atlas.sprites/ironclad/crimson_mantle.tres': ...
    at: open (core/io/resource_format_binary.cpp:1109)
    C# backtrace (most recent call first):
        [0] Godot.Resource Godot.ResourceLoader.Load(...)
        [2] Godot.Texture2D STS2SkinChanger.Core.SkinService.LoadIsolatedCardPortrait(...)
        [4] void STS2SkinChanger.Core.SkinService.ReplaceCardPortrait(...)
        [6] void <unknown>.STS2SkinChanger.Ui.CardSkinControls.ApplySelectedPortraitToNode_Patch1(...)
        [8] void <unknown>.MegaCrit.Sts2.Core.Nodes.Cards.NCard.Reload_Patch50(...)
        [10] void MegaCrit.Sts2.Core.Nodes.Vfx.Cards.NCardTransformShineVfx.UpdateCard(NCard, CardModel)
        [11] void NCardTransformShineVfx+<PlayUntilCardUpdate>d__28.MoveNext()
```

说明光效 `NCardTransformShineVfx` 是**真的在播**，但在它 `UpdateCard` 换模型的瞬间，我们的卡图接管在
**动画线程里同步读盘**（`LoadIsolatedCardPortrait` → `ResourceLoader.Load(..., CacheMode.Ignore)`，
每张新卡第一次出现都要真读一次，值几百 KB 的图集切片），主线程被卡住几十毫秒；动画的
`AwaitProcessFrame` / Tween 在恢复时一次跨过好几帧 → 白光还没扫完卡面已经换了，观感就是“生硬/没特效”。

同一份日志里没有 `单张卡图首次加载较慢`（阈值 30ms）的行，说明单张不算“极慢”，但一次变化会连着换
2~N 张卡（预览循环还会把候选结果全画一遍），累加起来就把动画挤掉了。

## 处理（1.0.27.75）

新增 `Ui/CardTransformPreload.cs`，在动画开始**之前**用现成的 `SkinService.PreloadCardPortraits`
把卡图读进缓存（命中 `CardPortraitCache` 后，动画期间只剩字典查询）：

| 补丁 | 时机 | 预加载内容 |
| --- | --- | --- |
| `NTransformPreview.Initialize` | 打开“变化”预览（候选结果循环展示） | 每个 `CardTransformation` 的原文卡、替换卡、全部候选替换 |
| `NCardTransformVfx.Create` | 居中变化特效创建前 | `startCard` + `endCard` |
| `NCardTransformShineVfx.Create` | 光效开始前 | 卡面当前模型 + `endCard` |

同时加了一行诊断（每次变化打印一次）：

```
[INFO] [SkinChanger] 变化特效已创建：<起始卡 id> → <结果卡 id>
```

## 怎么复现 / 验证

复现（1.0.27.74 及更早）：

1. 带皮肤（尤其选了“卡图”类皮肤提供者）进一局；
2. 进入会“变化”卡牌的事件，或直接调出“变化”卡牌的选择界面；
3. 选卡确认：卡牌会瞬间换面，白光扫过/放大动画要么很短要么看不到。

验证（1.0.27.75）：

1. 同一流程再走一次：应能看到完整的“放大 → 白光 → 揭开新卡面”；
2. 日志里出现 `变化特效已创建：… → …`；
3. 把日志发我：**如果这行没有出现**，说明游戏根本没创建这个特效（预览风格/容器问题，另有原因）；
   **如果出现但动画仍然硬**，说明是容器被遮挡或另有补丁干扰，需要按这行前后的日志继续查。

## 已知边界

- 预加载只消除“读盘卡动画”，不改变游戏本身的动画时长/曲线；
- 卡图缓存上限仍是 64 张（`MaxCardPortraitCacheEntries`），候选结果特别多的局面可能仍会淘汰重载；
- 如果玩家没有选任何“卡图”皮肤，本补丁不产生任何可见影响。

## 1.0.27.76：真正的原因是我们自己的卡面盖住了光效

1.0.27.75 的预加载只是把“读盘卡动画”消掉（日志里 `变化特效已创建：CARD.STRIKE_IRONCLAD → CARD.DEMON_FORM`
后面紧跟的是 `PreloadCardPortraits` 在 `Create_Patch1` 里执行，动画期间不再读盘）。但作者反馈“卡面盖在特效上面”，
说明剩下的问题是**绘制顺序**。

`ApplyManagedFullFrameArt` 给整图卡面新建了一个 `TextureRect`（`FullFrameOverlayName`）挂到
`%AncientBorder` 的父节点，且是**最后一个子节点**；而 `NCardTransformShineVfx` 是挂到
`%CardVfxContainer` 分支里的。后加的子节点绘制在上，于是我们的卡面把白光/粒子整块盖住。

修复两处：

1. `CardSkinControls.TrackAddedPresentationNode`：新增节点登记后调用
   `SendPresentationBehindCardVfx`，把节点移动到 `%CardVfxContainer` 所在分支**之前**（分支可能在更深的层级，
   会向上爬到同一个父节点再 `MoveChild`）。
2. `CardTransformShineVfx` 的 `Create` 后置：把光效节点 `ZIndex` 提到 10（卡面内部一般是 0，
   本 Mod 的全屏编辑器用 2000），即使某个提供者用了正的 z-index 也不会被盖住。

这样卡牌本身的贴图/边框仍由我们接管，但“光效层”永远在卡面之上。
