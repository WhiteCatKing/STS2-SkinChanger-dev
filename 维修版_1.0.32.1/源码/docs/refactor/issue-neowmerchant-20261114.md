# 问题记录：涅奥商人包不在列表里 + 首次切换后商人卡死（1.0.31.97 实测，2026-11-14）

## 现象（云实测）
- 同时装了 `商人nsfw裸`（Merchant2CuteII）和 `雪霁改 一代涅奥替换商人`（neowmerchant）。进游戏后，按加载顺序生效的是涅奥。
- 皮肤切换器的商人下拉里只有 Merchant2CuteII，没有涅奥。
- 第一次从涅奥切到 Merchant2CuteII 后，商人和猎人（玩家）的动画都卡住，商人也点不动；回主菜单再进一次就恢复，但下拉里依旧没有涅奥。

## 问题 1：为什么识别不到涅奥（已查清）
- neowmerchant.pck 一共 12 个文件，全部放在私有路径 `res://npc_feminization2/{merchant,hand,ui}/…`，**没有一个规范路径**。
- 它靠代码换皮：`[HarmonyPatch(NMerchantRoom, "_Ready")]` 的 Postfix → `MerchantRuntime.Attach` → 找到 skeleton data 为 `res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres`（以及手部 `…/hand/merchant_hand_skel_data.tres`）的节点，异步在旁边建一个 `NpcFeminization2Visual` 覆盖层，并把原节点藏起来。开关（NSFW）走 RitsuLib 设置。
- 皮肤切换器的目录只按"包里有没有规范路径资源"来给分组；DLL 证据规则（`SkinProviderPatchEvidencePolicy`）只覆盖角色，商人没有。所以它既不算商人皮肤，也不会被隔离，由游戏原样加载，补丁全局常驻。
- 离线复现：`CatalogInspect` 对这两个包只列出了 Merchant2CuteII。

## 问题 2：首次切换后卡死（原因未定）
日志里没有熔断，也没有我们自己的异常。切换时我们按 Merchant2CuteII 重建了商人节点，在主线程同步加载了 130 + 55 个资源（约 1.8 秒）。涅奥的全局补丁和它的异步覆盖层还挂在旧节点上。推测是两个商人包同时生效、互相抢同一组节点，但还没有证据。

**要先区分是不是这次重构引入的**：用作者原版 1.0.31.95 按同样的步骤再测一次。
- .95 也卡住 → 原来就有，是两个商人包冲突。
- .95 不卡 → 问题出在 .96/.97 的改动里，要回头查。

## .95 对照结论
云用作者原版 1.0.31.95 按同样步骤复测：**一样卡住**。所以问题 2 原来就有，不是 .96/.97 的重构引入的。

## 1.0.31.98 已实现（问题 1）
- `SkinCatalog.IsDirectMerchantSkinAssembly`：有 HarmonyPatch 目标 NMerchantRoom/Button/Hand，并且程序集里引用了 `backgrounds/merchant_room/` → 判为代码型商人皮肤（只读元数据，带缓存）。
- `LooksLikeDllSkinProvider` 先认这一类 → 进入提供者探测 → 交给隔离加载。
- `AddDirectMerchantRuntimeProviderOptions`：在商人分组里加一个零资源的运行时提供者选项（和代码型角色皮肤用同一套标记）。
- 离线 `CatalogInspect`：商人下拉现在是 neowmerchant + Merchant2CuteII。全机扫描所有 Mod DLL，命中的只有这两个商人包和切换器自己（自己不参与目录）。
- 待实机：被隔离后涅奥只在选中时生效；选中涅奥时商人能正常换成涅奥；在两者之间来回切换时不再卡死。

## 1.0.31.98 实测（云）与 1.0.31.99 修正
- 涅奥已进下拉、已被隔离（日志「已隔离皮肤提供者 商人娘化涅奥版」）；这次没报卡死。
- 但选中涅奥后显示的是原版商人：它唯一的补丁 `NMerchantRoom._Ready` 后置被加载器归为"受管的节点就绪回放"摘掉了（日志「保留 0 个原作者行为补丁」），而真商人房间这条回放只给角色提供者跑，商人分组没跑。
  → .99：新增 `MerchantRuntimeAppearance.ReplayDirectMerchantRoomReady`，只对代码型商人提供者生效，挂在具体房间 `_Ready` 后置（进商店）和 `TryRefreshMerchant`（局内热切）两处。
- 假商人（怪物图鉴）下拉里也出现了涅奥：是 `MergeOrphanMonsterOptions` 把商人分组的选项复制给了 `fake_merchant_monster`。→ .99：代码型商人提供者不复制过去（它不处理假商人）。离线已确认。
- 记录、未处理：
  - 第一次进商店，Merchant2CuteII 放大飞到左上角，只看得见脚；打开外观面板选一次后回到原位。属于报告第三节"商人位姿"那一族问题，和涅奥无关，后续单独查。
  - 假商人下拉里出现战士包「奥卡」：单独检查 `小银龙奥卡-常服板甲` 时只进 ironclad，不进商人，所以是和别的包组合后被并进来的。和这次改动无关，后续单独查。

## 1.0.31.99 实测（云）与 1.0.31.100 修正
- 商人选的是游戏默认，进店却是 Merchant2CuteII 的样子，而且飞在角落。根因：`ManagedSkinModLoader.IsMerchantSkinProvider` 按名字豁免，id 里含 "Merchant" 的包一律不卸。云之前在怪物图鉴里给假商人选过一次 Merchant2CuteII，它就被激活并常驻了，进店时补丁照样替换商人，但没有位姿。
  → .100：只豁免 `merchant` / `fake_merchant_monster` **当前选中**的提供者，两侧去掉后缀后全等比较。这正是 GPT 勘察里点名的"按名字常驻豁免"。
- 涅奥选中后仍是原版。日志显示回放走的提供者是 Merchant2CuteII（`[商人] 已为代码型商人皮肤 Merchant2CuteII 重放…：0 个提供者`），涅奥一次都没回放。两个原因：
  1. 登记代码型提供者时，Merchant2CuteII 的 DLL 也命中规则，被误登记了（它其实早已靠规范资源进了下拉）。→ 改为只登记真正以代码型身份加进下拉的包。
  2. 回放取的是"完整运行时提供者"，而涅奥不算完整运行时提供者，所以取到 null。热切换时，这次回放还放在只有完整提供者才会进的分支里。→ 改为按普通运行时选择取；回放前先确保它已激活；热切换的回放挪到旧节点释放之后，不再放在那个分支里。

## 1.0.31.100 实测（云）与 1.0.31.101
- 选中涅奥后**完全退出游戏再进**：正常显示涅奥。说明识别、隔离、进店回放这一路是通的。
- 局内热切到涅奥、或者只回主菜单再进：显示原版。日志 `重放房间就绪步骤：0 个提供者`，紧接着才是 `延后初始化完成…neowmerchant`。原因是涅奥刚被选中、或刚被场景收窄卸掉，它的激活排进了延后队列，回放发生在激活之前。
  → .101：回放时它还没激活，就把房间和提供者记下来，由每帧 tick 等它激活后补跑一次。最多等 300 帧（约 5 秒）；期间选择被换掉就放弃。
- 进店显示 Merchant2CuteII：这一轮云在怪物图鉴里给**假商人**选过 Merchant2CuteII（两个分组共用一个 DLL / PCK），真商店于是被它接管。原因未定，已请 GPT 只读分析（`task-gpt-fake-merchant-coupling-20261114.md`）。

## 1.0.31.101 实测（干净环境）与 1.0.31.102
- 日志显示等待重放生效了：`激活后已重放房间就绪步骤：1 个提供者（等了 2 帧）`。涅奥的代码确实跑了，但商人还是原版。
- 根因（读涅奥反编译的 `FindTargets`）：它**只认 skeleton data 的 `ResourcePath` 恰好等于** `res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres`（手部同理）的节点。我们热切换时，是在"按选择挂的运行时覆盖层"里实例化新商人按钮的，资源路径变成了 `res://sts2_skin_runtime/...` 这样的别名，它就找不到目标。冷启动时房间走的是规范场景，所以重启就正常。
  → .102：`SkinService` 的 `GetOrLoadRuntimeScene` / `LoadRuntimeScene` / `InstantiateRuntimeScene` 在当前选择是代码型商人皮肤时，直接用游戏规范路径加载场景（这类选择本身不带任何规范资源，没有需要覆盖的东西）。

## 1.0.31.102 实测（云）与 1.0.31.103
- 云决定：真商人、假商人**保持作者的联动设计**，不改成独立选择（GPT 的分析 `analysis-fake-merchant-coupling-gpt.md` 留作记录）。
- 第一次进店，热切到涅奥：**成功**。
- 从涅奥切回原版 / Merchant2CuteII 后，商人**点不动**。读反编译：涅奥的 `MerchantPresentation` 把商人按钮的 `MouseFilter` 设成 Ignore，自己挂一个 `SilhouetteHit` 命中区来转发点击。它没有对外的"卸下"入口，切走后覆盖层还在，按钮仍是 Ignore。
  → .103：新增兼容枝条 `Ui/CodeDrivenMerchantCompat.cs`。`PrepareMerchantSelectionChange` 在从代码型商人皮肤切走前，把带 `NpcFeminization2Visual` 标记的节点移出场景树（这会触发它自己的 Dispose，把 MouseFilter 还原），然后兜底把商人按钮恢复为 Stop。
- 停在涅奥、回主菜单再进店：变成原版，再切回来也是原版。日志显示回放确实执行了（`重放房间就绪步骤：1 个提供者`），但涅奥没认出目标。推测是会话里换过皮肤以后，我们刷新缓存时用 `CacheMode.Ignore` 读回的骨骼资源 `ResourcePath` 为空（或者是别名），而涅奥只按全等路径匹配。
  → .103：回放前 `EnsureCanonicalTargets` 把商人按钮 `%MerchantVisual` 和手部 spine 节点的骨骼数据 `TakeOverPath` 到规范路径，并记录原路径（`[商人] 代码型皮肤目标骨骼路径已接管：… → …`），下次实测时用来确认这个推测。

## 1.0.31.103 实测（云）与 1.0.31.104
- 热切到涅奥：生效。切走后能点：生效。
- 停在涅奥、回主菜单再进店：涅奥出现了（说明路径接管有效），但飞在左上角。日志证实，进店时商人骨骼来自覆盖层 019 的别名，GPT 对照日志确认 019 是 **Merchant2CuteII** 的房间运行包（`analysis-neow-reentry-gpt.md`）。原节点其实是 Merchant2CuteII 的骨骼，却没有它的位姿，涅奥照着它的范围摆放，就跟着飞了。
  → .104：`SkinService` 的三个场景入口在代码型商人选择下，按**「游戏默认」选择**构建商人场景（基线内容，走现有的 `LoadRuntimeResourcesForSelection` / `WithRuntimeResourcesForSelection`，选择固定为 `__base__`），取代 .102 的规范路径 Reuse。这样不会沿用旧的别名层或缓存的 PackedScene。规范路径仍由回放前的 `EnsureCanonicalTargets` 接管。GPT 提出的"挂载时强制基线覆盖 + 清 MountedOverlayOwners"暂不做，实测不够再上。

## 1.0.31.104 实测（云，21:00）：通过
- 热切、切走后点击、停在涅奥回主菜单再进店：**全部正常**，涅奥在原位，可以点击进店（截图见云 21:00 的消息）。
- 日志 `_test_backup_20261114\godot-104-godot.log`：回放、路径接管、切走清理都按预期出现。错误类型（`Script inherits from native type 'Node2D'`、`Handle is not initialized`、`fake_merchant_top.tres.tres`、`character_select/defect`）从 .95 起就有，不是新问题。
- 外观面板固定在左上角，这是面板本身的布局，不影响使用。
- 结论：GPT 的"挂载时强制基线"方案不需要做了。环境已恢复（工坊目录改回，测试包移到 `deployed_mods_folder_104`）；NpcFeminization2 两个包仍在 `removed_mods`（和涅奥同名程序集，不能同时装）。

## Merchant2CuteII 第一次进店飞左上角（1.0.31.105）
- 对照 .98 和 .99 的日志：先在图鉴里给假商人选了 Merchant2CuteII（真商人联动跟着变），它被激活后，自己的 `ResourcePreloader` 用 `CacheMode.Reuse` 读了商人规范骨骼，并存进自己的 `_cache` 一直持有（反编译 `merchant_ws_decomp` 第 1207 行起）。之后切回游戏默认，进店时 `merchant` 分组没有加载任何运行时资源，房间按规范路径从 Godot 缓存里取到的还是它的骨骼，没有位姿，于是放大飞到左上角。
- 修法：新增 `Core/MerchantCanonicalBaseline.cs`。`ApplySelection` 中只要真商人或假商人分组受影响，就在下一帧执行：新选择不提供规范骨骼，并且缓存里那份不是我们还原过的，就用 `SkinService.LoadBaselineResources`（基线别名）读一份，`TakeOverPath` 回规范路径，`PreloadManager` 里有这两条的话也一并替换。进店时记一行 `[商人] 进店商人骨骼：…`。
- 步骤：`ingame-test-steps-1.0.31.105.md`。
- **实测通过（云，21:15）**：日志 `_test_backup_20261114\godot-105-godot.log`。切回游戏默认后出现 `已按游戏默认还原商人规范骨骼缓存`；第一次进店时为 `进店商人骨骼：已还原的基线；选择=__base__，缩放=0.4701`；店里切到 Merchant2CuteII，位姿正常应用；切回游戏默认、切到涅奥都正常。环境已恢复。

## NpcFeminization2 两个包（云 19:59 装入，正好赶上 .100 的测试）
- 两个都是商人皮肤（`art/merchant/*.spjson|atlas|png` 是散文件，**没有 PCK**），由 DLL 按 `config.json` 从自己目录读资源，换掉 `shop_merchant_top.tres` 那只商人。sfw 和 nsfw 两个变体的 DLL 完全相同（`E2B478E3D350`），区别只在 art 和 config。
- 安装状态：`mods\NpcFeminization2\` 里还套了一层 `NpcFeminization2\`（解压多套了一层）。再加上 `NpcFeminization2nsfw\NpcFeminization2\`，同一个 id 一共装了 **3 份**。切换器把它们当作同 ID 的三个来源隔离了，但它们没有 PCK，没进商人下拉，所以实际上被关掉了。
- **关键**：`neowmerchant.dll` 和 `NpcFeminization2.dll` 的程序集名相同（都叫 `NpcFeminization2`，版本 1.3.2 和 1.0.0）。加载器把同名程序集当成"同一个包的再发布"，只运行其中一份。日志里有 `已跳过 商人娘化涅奥版 的重复初始化器 NpcFeminization.ModEntry.Init`。也就是说，从 19:59 起，涅奥的代码可能被 1.0.0 那份顶替了，.100 那一轮的实测结果被污染。
- 结论：要验证涅奥，得先把这两个包移出 `mods`。要不要另外支持 NpcFeminization2（无 PCK、散文件、与涅奥同名程序集）是单独一件事：同名程序集在一个进程里本来只能跑一份，两者无论如何不能同时用。

## 修法草案（问题 1，原稿）
新增"代码型商人皮肤"的识别：DLL 对 `NMerchantRoom` / `NMerchantButton` 打了补丁，并且 DLL 的字符串常量里引用了商人规范骨架路径（`shop_merchant_top.tres` / `merchant_hand_skel_data.tres`），并且包里带有 spine 资源 → 识别为 `merchant` 分组的运行时提供者。识别出来以后，交给现有的提供者隔离：只有选中它时才 PatchAll，没选中时不挂补丁。这样两个商人包就不会同时生效，问题 2 很可能也会一起消失。
涉及 `SkinCatalog` / `ManagedSkinModLoader`（主干，Claude 来做）。
