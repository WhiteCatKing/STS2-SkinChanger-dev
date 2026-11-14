# 皮肤切换器 · 问题与修复对照表

用途：以后遇到相似症状，先在这张表里按「症状」找根因，避免重复试错。
记录范围：本轮会话（2026-11-12 ~ 11-14）遇到的全部缺陷，含已修复、已回退、暂停观察。
配套文档：`CHANGELOG.md`（逐版改动）、`更新日志_1.0.31.80.md`（对外发布说明）、`STS2-SkinChanger_修改件\docs\merchant-isolation-design.md`（商人模块设计）。

## 一、启动与加载

| 症状 | 根因 | 修法 / 结论 | 版本·提交 |
|---|---|---|---|
| 启动就崩 `0xC0000005`，栈 `Godot.OS..cctor` ← `BaseLib.Logger..cctor` | Steam 正在替换工坊文件时启动，非代码不兼容（同一构建三次可启动两次） | 等 Steam 下载完再启动；崩了就重启 | 观察结论，非缺陷 |
| 游戏更新 v0.113.0 后「致命错误」 | v0.113.0 删除 `MegaCrit.Sts2.Core.Models.SingletonModel`，`ProviderModelDiscovery` 直接 `typeof` 引用 → `TypeLoadException` 打断 `ExecuteEssential_Patch3` | 反射按名取类型，缺失即跳过 | 1.0.31.0 · `d1dfaaa` |
| 十把有四把启动卡死、黑屏、无响应（进程在、CPU 空转） | 冷启动预热极重（`外观预热与选角预载` 约 20.7 s，整体 28.5 s），真卡死时日志停在 `背景资源扫描完成…` 或 `Preloading 'characters=X'` | ①加 `StartupWatchdog`（阶段停滞 45 s 写 `startup_hang.txt`）②把预热拆成子阶段便于定位 | 1.0.31.81 `a72821d`、1.0.31.82 `fd1361f` |
| 旧版 v0.107.1 上「所有皮肤啥也检测不到」 | DLL 用 v0.113 编译，硬引用只存在于 v0.113 的类型（`NStandardMerchantRoom`/`NBlitzMerchantRoom`），程序集加载失败 | 4 处硬引用改 `AccessTools.TypeByName` 运行时探测；补丁枚举整段 try/catch | 1.0.31.83 `5844858` |
| 日志单份涨到 39 MB | 诊断日志逐条 flush 且不轮转 | 日志 24 MB 上限 + 关键诊断去重 | 1.0.31.x（性能批次） |

## 二、选人界面

| 症状 | 根因 | 修法 / 结论 | 版本·提交 |
|---|---|---|---|
| 随从专用皮肤（奥斯提/亡灵手掌）出现在其他角色下拉里，选中后整局卡死 | 下拉未过滤随从专用提供者；`IsCompanionOnlyProviderSelection` 要求“选在随从分组”，主人页恒 false → 排除表恒空 | 新增 `IsCompanionOnlyProvider`（只看声明），在选项循环里过滤 | 1.0.31.33 `f944ea2` |
| 非骨妹角色也出现奥斯提预览框，且选过骨妹后到处都有 | 预览面板挂在 `InfoPanel/STS2OstyModelPreview`，`Refresh` 每次无条件 `ShowOsty()` | `CompanionHiddenMeta` 标记 + `OstyModelPreviewControls` 门控，只对骨妹显示 | 1.0.31.35–.37、1.0.31.46 `09c5f40` |
| 整包重建时崩 `NSpineAutoPlayer's parent's skeleton data must have exactly 1 animation. This has 6.` | 随从分组走了“完整运行时重建” | 随从分组跳过整包重建 + try/catch 回退普通资源挂载 | 1.0.31.32 `a5dc77d` |
| 十字军变体：选不同变体预览小人不变 | 变体选项沿用 `ProviderId`，走提供者默认场景 | 变体场景写回 `ResolveVariantCombatPath` | 1.0.31.88 `8eadb98` → 定版 1.0.31.92 |
| 十字军变体：换上了但动画少一半 | 变体 id 带 `::variant:` 后缀，被收窄逻辑判为“未选中”→ 卸掉该提供者 14 个补丁 | 比较前归一化 id（`NormalizeProviderIdentity`） | 1.0.31.94 `675f345` |
| 十字军变体把动画彻底关掉 | `ProviderId = null` 会让提供者被判未选中、11 个动画补丁停用 | 回退该赋值 | 1.0.31.85 `b22890e` → 1.0.31.86 `b7a6e65` |
| 快速连点变体偶发崩溃 | 重建 `Callable.From` 排队竞态（未复现于单击） | 暂不加守卫，出现再加“上一次重建未完成则丢弃” | 观察中 |
| 贞德四变体背景/头像全一样 | `CharSelectStageCycle` 的 `directory` 被改成包根，`portrait_palettes/<变体>/…` 全部拼到同一目录 | 拆 `DataDirectory` 与 `Directory` | 1.0.31.8 `f1e2bb6`（仍待作者复验） |
| 贞德变体背景被改坏（整片原皮） | 1.0.31.4 的「包自命名背景兜底」选到 `res://custom/…` 别名路径，挂载撤销后实例化失败 | 回退（1.0.31.5），后续兜底排除别名前缀 | 1.0.31.5 `1f69f3e` |
| 鸡煲娘化（DefectSkinMod）背景与局内小人不变、只有头像变 | 该包背景由自己的 DLL 补丁塞进 `AnimatedBg`，不走规范路径；其初始化器在 v0.113 上抛 `TargetInvocationException` 被整包回滚 | 作者要求暂停（他确认该包脱离切换器也无法工作） | 暂停 |

## 三、局内 / 商人

| 症状 | 根因 | 修法 / 结论 | 版本·提交 |
|---|---|---|---|
| 商人皮肤无法切换 | v0.113 把 `NMerchantRoom` 改成 abstract，`MerchantButton` 变抽象属性，backing field 移到子类 | `ResolveMerchantButtonField(instance)` 按实例类型沿基类链解析 + 具体房间 `_Ready` 补丁 | 1.0.31.2 `d42dfc7` |
| 商人刷新报「独立皮肤资源不是场景：merchant_room.tscn」 | v0.113 拆成 `standard_merchant_room.tscn` / `blitz_merchant_room.tscn` | 按房间类型选路径 | 1.0.31.3 `0a3e5c2` |
| SL 后商人变回原版 | 读档时房间先于资源就绪，`_Ready` 只跑一次且失败不重试 | 房间就绪看门狗（每帧 tick 驱动，240 帧内重试） | 1.0.31.29 `21be726` |
| 局内切换商人：a/b 飞左上角、c 变原版 | ①为修 SL 改成重建节点，重建用游戏本体场景（c 的 pck 无该路径）②重放与包自身前缀双份执行 | 禁止重建节点、禁止双份重放；改为“补丁在位 + 进树前重放一次” | 1.0.31.57–.72 `7e4b96e` |
| 选 b 却显示 a | 运行时 key 前缀匹配（`Merchant2CuteII` 是 `Merchant2CuteIIB` 的前缀） | 运行时 key 全等匹配 | 1.0.31.72 `7e4b96e` |
| 商人包补丁被场景收窄卸掉 | `DeactivateProvidersExcept` 把商人提供者当“未选中”卸载 | `IsMerchantSkinProvider` 豁免 | 1.0.31.66 `bd65015` |
| 商人位姿重复套用（变大/偏移） | 包前缀与我们的重放各加一次 | `CorrectDoubleAppliedPose`：`0 < scale < 0.1` 视为重复并还原 | 1.0.31.74 `54963a9` |
| 商人皮肤被误摘补丁（玩家反馈） | `VisualPatchGuard` 摘补丁判据对“游戏原生提前加载、我方未接管”的包恒真 | 新增 `IsManagedProviderAssembly` 前置条件 | 1.0.31.31 `7bd709d` |
| 假商人帐篷坐标校正影响未装 NSFW-Event 的玩家 | 判据用“目录存在”，Steam 退订不删目录 | 改查 `settings.save` 启用列表 | 1.0.31.80 `2734437` |
| 假商人控制台 `event fake_merchant_event` 不存在 | 指令名与 encounter id 不同 | 记录备用：`event fake_merchant` / `fight fake_merchant_event_encounter` | 备忘 |

## 四、战斗内 / 皮肤包协作

| 症状 | 根因 | 修法 / 结论 | 版本·提交 |
|---|---|---|---|
| 局内点贞德切换 → 屏幕黑矩形 + 整机卡死 | 战斗重建去加载选人专用资源 `…/s3_charselect.ctex`，该文件不在当前覆盖层 → Spine 拿到半加载纹理，原生层 `Parameter "mem" is null.` 崩进程 | 缺失资源只跳过不抛异常（不再发请求） | 1.0.31.93 `092ec98` |
| 战损爆衣全部失效 | 收窄比较只归一了左侧，带 `::source:` 后缀的运行 key 被判未选中 → 卸掉战损补丁 | 两侧都归一 | 1.0.31.95 `fa2590c` |
| 骨妹（necrobinder）低血抽搐 | 皮肤包自身活锁：`ReconcileLowHealthIdle` 在 hp≤25% 且动画为 `idle_loop` 时把 Stage 重置 -1，每 0.12 s 重置骨骼 | 包自身问题，作者决定不修 | 关闭 |
| 魅魔/骨妹「点击背景切战损」不生效 | 包靠 `SkinSelectionGate.MenuAllowed()` 反射问我们“选中的是不是我”；且它要求选人骨架 atlas 页尺寸等于预设值 | 已定位闸门链路；作者决定暂停 | 暂停 |
| 奥斯提皮肤失效 | 包按 `NCreature.OstyScaleToSize` 打补丁，v0.113 改名 `ScaleAndPositionOsty`，`PatchAll` 失败后自我 `UnpatchAll` | `OstyDefectImpostorCompat` 拦截 `AccessTools.Method` 重定向 | 1.0.31.1 `2f43d54` |

## 五、外部/环境类（不是代码缺陷）

| 症状 | 根因 | 处理 |
|---|---|---|
| 每次启动都提示「需要重启生效」 | 同一 id 双来源：工坊 `3787302680`（1.0.29）+ 本地 `mods` 目录（1.0.31.x），游戏每次判定模组列表变更 | 取消工坊订阅其一；同时修 `ShouldShowLoadOrderWarning` 为 AND 判定（后回退） |
| C 盘被吃 63 GB | `cache\` 会话目录只在正常退出清理，卡死/强退整份留下（130 个会话） | `TrimCacheBudget(8 GB)` 启动时清理 |
| 每次改皮肤重写 588 MB 覆盖包 | 覆盖包签名含物理路径（每会话目录名不同） | `StableArchiveToken`（文件名骨架 + 字节数） | 1.0.31.30 `46449f8` |
| 工坊上传返回 `k_EResultFail` | Steam 不接受空更新说明 | `ChangeNote` 必须非空 |
| 工坊上传无回调 | `SubmitItemUpdateResult_t` 是 CallResult，不是 Callback | 用 `CallResult<T>.Create(...)` + `Set(call)`；仓库 `STS2_Work\workshop_update` 已有正确实现 |

## 六、经验教训（避免重复踩）

1. **`EmptyNCreatureVisuals` 不是“空模型”**，这套包正常呈现就用它承载；看到它不要判定“模型没建起来”。
2. **不要质疑作者的实机结论**；他对比不同皮肤会改配置，但“某个皮肤没变”这个结论成立。
3. **id 比较永远先归一化**（`::variant:` / `::source:` 后缀），否则会出现“修好 A 就坏 B”。
4. **不要让包的补丁与我们重放的同一套前缀同时生效**（双份执行会把节点搞坏）。
5. **单帧 `CallDeferred` 不可靠**：依赖 `Instance` 的时机要改成“多帧重试”或“每帧 tick 驱动”。
6. **任何资源加载失败都不许抛进渲染循环**，原生层会直接把进程带走。
7. 回退前先确认作者意图（1.0.31.87/88 我曾擅自回退，被明确纠正）。
