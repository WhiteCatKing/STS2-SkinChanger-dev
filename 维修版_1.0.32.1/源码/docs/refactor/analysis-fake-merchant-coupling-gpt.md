# 假商人选择联动分析 · GPT · 2026-11-14

依据 `.100-run2.log` 与当前 `.101` 源码，只读追踪本次正常流程。下文源码路径相对 `STS2SkinChanger/`，日志指 `_test_backup_20261114/godot-1.0.31.100-run2.log`。

1. **直接根因是全量提供者选择事务。** 日志 L764–765 明确记录选择 `fake_merchant_monster → Merchant2CuteII` 时同步写入 `merchant`，进店后 L32320 又确认 `merchant 配置=Merchant2CuteII`。本日志只记录一次手动假商人选择，末次切涅奥时事务把假商人退回 `__base__`（L32324–32326）。`BuildVisualSelectionTransaction` 先把整包所有分组作为目标，再展开同步、连带退回，并由 `ApplySelection` 写入配置。商店入口传入 `merchant`，再合并当前战斗组、玩家、Boss，日志中的“2 个组”与本局 SILENT + merchant 相符，范围计算没有主动添加假商人。`.100` 的双组选中豁免还会让假商人提供者跨场景保留，修复独立选择时也要一起收紧。依据 `Catalog/SkinCatalog.cs` L2401–2472、`Core/SkinService.cs` L1951–1954/L2002–2005、`Ui/MerchantRuntimeAppearance.cs` L1910–1913、`Ui/CharacterAppearanceRuntime.cs` L274–289、`Core/ManagedSkinModLoader.cs` L1101–1117，日志 L858。

2. **骨骼日志由切换器的按钮 Ready 桥接触发。** Prefix 按节点祖先解析出 `merchant`，取得该组已联动成 Merchant2CuteII 的完整提供者，回放其 Ready Prefix，Postfix 再调用 `RememberMerchantFigureProvider` 记录位姿。日志 L32266–32268 与该链一致，用户热切涅奥发生在之后的 L32324。依据 `Ui/MerchantRuntimeAppearance.cs` L118–135/L181–213/L424–435/L2143–2167、`Core/SkinService.cs` L5080–5099。

3. **已有资源还原流程，但这次进店的有效选择要求保留 Merchant2CuteII。** `BuildOverlay` 先还原整包规范资源，再覆盖选中项。`MountOverlayCore` 随后按路径归属组检查，归属组仍选提权提供者时保留资源，因此此时商人身体、手部按 Merchant2CuteII 生效。热切涅奥后日志 L32330–32340 记录卸掉 6 个行为补丁、挂载校正层并显示 `merchant=neowmerchant`。日志没有逐路径内容，无法确认当时 `shop_merchant_top.tres` 的实际缓存对象。依据 `Catalog/SkinCatalog.cs` L3105–3123/L3153–3196、`Core/SkinService.cs` L6440–6450/L6491–6527。

4. **建议最小修法，覆盖选择、激活和回放三个入口。**

   - `SkinCatalog.BuildVisualSelectionTransaction` 对真、假商人只更新请求组，同时在目标展开、旧提供者整包退回、新提供者同步三个位置排除另一商人组。两组各自保存选择。现有三处联动见 L2409–2412/L2423–2442/L2448–2471。
   - `SkinService.GetActiveRuntimeProviders` 给商人使用“当前可见组且该组实际选中此提供者”的判据，按选中组与可见组的交集筛选。当前全量门槛要求所有关联组同时选择，随后候选又使用提供者全部所属组，独立选择后必须补此分支。`DeactivateProvidersExcept` 同步取消跨场景双组豁免，保留本次计算出的活动集合。依据 `Core/SkinService.cs` L7043–7058/L7100–7106、`Core/RuntimeProviderScopePolicy.cs` L75–81、`Catalog/SkinCatalog.cs` L2352–2364、`Core/ManagedSkinModLoader.cs` L1106–1117。
   - 商人 Ready 桥接、`IsSelectedProviderForNode` 与 `ManagedMerchantSettingsBridge.Bind/Refresh/RequestRefresh` 统一改用“节点所属商人组当前选中的提供者”，为这两个组增加局部分组查询，接入现有回放与设置应用。现有 `GetSelectedFullRuntimeProvider` 的整包全选门槛会挡住独立选择后的位姿和设置，需同步替换这些商人调用。资源继续走既有归属还原，并确保两组规范路径都参加校正。依据 `Core/SkinService.cs` L5095–5099、`Ui/MerchantRuntimeAppearance.cs` L118–135/L397–404/L407–416、`Core/ManagedMerchantSettingsBridge.cs` L56–79/L121–122、`Core/SkinService.cs` L6406–6411/L6491–6527。

交付仅含分析文档，结论来自日志与静态源码，修法由 Claude 实现并验证。
