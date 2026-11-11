# 商人皮肤「Merchant2CuteII」选项消失：同 ID 双份来源被游戏整体禁用

- 状态：**已定位并恢复**（2026-11-07）
- 反馈来源：作者本人（`mods\商人n版` v1.4.1 + 工坊 3748331824 v1.6.0 同时安装）

## 现象

皮肤切换器里商人皮肤（Merchant2CuteII）的选项整条消失，日志只剩：

```
Skipping loading mod Merchant2CuteII, it is set to disabled in settings
Skipping loading mod Merchant2CuteII, it is set to disabled in settings
```

## 根因

游戏自己的"同 ID 双来源"处理：

```
[WARN] Mod with ID Merchant2CuteII is loaded both via Steam and local mods directory.
Steam version (v1.6.0) is greater than local version (v1.4.1), so we are disabling the local version.
```

游戏比较两份清单的版本，把**版本低的那份禁用**。但它写回 `settings.save` 的开关是
**按 mod id** 而不是按来源，所以 `mod_list` 里两条 `Merchant2CuteII`（`mods_directory` 与
`steam_workshop`）都变成 `"is_enabled": false`。下一轮启动两份都不加载，
`SkinChanger` 的 `IsProviderLoadCandidate` 看到"settings 禁用"就直接跳过，下拉里自然没有选项。

这不是皮肤切换器写的：我们只**读**这个开关（`ModSettings.IsModDisabled`），
唯一会写 settings 的是"加载顺序提示"里的重排，且只改顺序、保留每条的 enabled。
时间线也吻合：`godot2026-11-07T12.21.54.log` 商人还正常（游戏刚发出上面那条 WARN），
之后 12:33 的日志起两条都被跳过。

## 处理

1. 把本地差分包 `mods\商人n版` 移到备份：
   `STS2_Work\商人n版_停用备份_20261107`（可随时搬回）。
2. 保留工坊版 v1.6.0（更新、且作者 `ISSUE_2026-09-15_merchant_model_top_left.md`
   记录里 n 版有模型错位问题）。
3. 清掉 `settings.save` 里 `mods_directory` 那条残留记录，并把工坊条目改回
   `"is_enabled": true`（备份 `settings.save.bak_20261107_merchant`）。

## 结论 / 以后怎么办

- **同一个 mod 不要同时放"工坊订阅 + 本地 mods 目录"**：这个游戏版本会按版本号禁用低版本那份，
  并按 id 把两条一起关掉，最终两份都不加载。
- 需要测试"同 ID 差分包"时：临时把本地包放回、测完再移走；或者只留差分包并把工坊那份取消订阅。
- 皮肤切换器可以读这个状态做提示（可选改进）：启动时若发现某提供者 `DisabledDuplicate` 且
  settings 全局禁用，就提示"该皮肤被游戏的重复来源规则禁用了，请只保留一个版本"。
