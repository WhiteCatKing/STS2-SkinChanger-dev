# 涅奥回店旧别名分析 · GPT · 2026-11-14

依据 `.103` 源码与 `godot-103-godot2026-11-14T20.40.58.log`。源码路径相对 `STS2SkinChanger/`，仅分析任务指定流程。

1. **019 确认来自 Merchant2CuteII。** 日志 L35670–35671 记录其房间运行包为 `/019`，回店 L35769–35770 的身体、手部资源都来自同层。进店由游戏 `NStandardMerchantRoom.Create → PreloadManager.Cache.GetScene → Instantiate` 创建整房间，按钮随场景创建。`AssetCache.GetScene` 后置再经 `ReplaceCachedScene → GetOrLoadRuntimeScene`，涅奥走 `.102` 新增的规范路径 `CacheMode.Reuse` 分支，可继续拿到持有旧骨骼的场景。`MerchantRoomCreateAppearancePatch` 只切范围、记租约，具体房间 Ready 后置才回放涅奥。依据 `Ui/ContextualSkinControls.cs` L3258–3277/L4047–4052、`Core/SkinService.cs` L4482–4527、`Ui/MerchantRuntimeAppearance.cs` L1915–1936/L1976–1978。游戏反编译证据在 `%TEMP%/sts2-gpt-neow-reentry-NStandardMerchantRoom.cs` L67–83。

2. **关键差异发生在场景加载与缓存，BuildOverlay 本身已对两种选择回退基线。** 涅奥选项 Assets 为空，所以与 base 一样走 `ResolveBaseline`。base 热切仍会重新构建基线的隔离场景及依赖，涅奥却提前返回规范路径 Reuse 场景。此前别名构建会写入规范 `.remap/.import` 桥接，挂载后只登记 `RuntimeCanonicalDependencyPaths`，没有使 `MountedOverlayOwners` 对应记录失效，校正增量可因“来源未变”跳过恢复。涅奥 PCK 只有私有路径，其提权清归属也触及不到商人规范路径。挂载后的 `RefreshCachedCreatureVisualScenes` 仅刷新 `/creature_visuals/*.tscn` 且用 Ignore，商店父场景及骨骼依赖缓存继续存活。依据 `Catalog/SkinCatalog.cs` L3153–3196/L5664–5671/L5716–5729/L6279–6284、`Core/SkinService.cs` L4543–4572/L5962–5976/L6452–6457/L6598–6608/L7045–7050/L7368–7372/L8416–8438。日志 L35532–35535 的 base 新别名与 L35769–35770 的旧 019 对应这一差异。可确认旧资源来源及还原缺口，日志尚不能区分回店时旧 remap 与已缓存 PackedScene 各自的贡献。`CodeDrivenMerchantCompat.EnsureCanonicalTargets` L33–45 只给当前数据 TakeOverPath，骨骼内容和位姿仍来自原对象。

3. **最小修法建议采用一个代码型商人基线准备入口，供挂载和场景加载共用。** 判据为真商人组且有效提供者经行为来源解析后满足 `IsDirectMerchantProvider`。

   - 在 `MountOverlayCore` 合并选中项之后，为该分支用 `BuildGameBaselineOverlay` 强制覆盖商店房间、库存场景、身体与手部骨骼及其游戏原包依赖、remap/import。清除这些规范路径的 `MountedOverlayOwners` 再交给增量挂载，保证旧记录不会跳过恢复。覆盖计划复用分支也须执行此步骤。现有入口见 `Core/SkinService.cs` L6470–6488/L6524–6534/L6587–6590，游戏原包接口见 `Catalog/SkinCatalog.cs` L4806–4848。
   - `TryLoadCanonicalSceneForCodeDrivenSkin` 在首次取该房间场景之前调用同一准备入口，再用 `CacheMode.ReplaceDeep` 重读游戏基线场景与外部依赖，并把重读的商店房间、库存场景写回 `PreloadManager.Cache.SetAsset`。热切的 `InstantiateRuntimeScene` 与原生进店的 `GetOrLoadRuntimeScene` 都已汇入这里，修复需早于 Instantiate，Ready 时改路径已经太晚。现有汇入口见 `Core/SkinService.cs` L4499–4546，已有 Replace + SetAsset 用法见 L8503–8512。

涅奥私有 PCK 继续使用现有挂载链，行为激活及 Ready 回放继续按 neowmerchant 选择执行。上述补充只决定真商人的规范基底资源，真、假商人按云已确认的联动设计处理。本文为静态分析，修法待 Claude 实现及同流程实测。
