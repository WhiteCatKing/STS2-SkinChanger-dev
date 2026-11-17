# 1.0.34.2：三种模式的选角背景与头像一致性

## 反馈与证据

用户截图显示极速/恐惧选择愚者单图、Fool — Waifu、希尔德等皮肤时小模型改变但大背景仍为原版，部分头像也保留原图。标准模式正常。日志反复出现「批量刷新角色列表头像失败：所选皮肤的选人头像未就绪：Sequence contains no matching element」。用户要求同时检查其他角色，并保留模式特色。

## 修复

1. ManagedSkinModLoader 原来仅将标准 NCharacterSelectScreen.SelectCharacter 识别为受管理的选角呈现。Fool_Waifu 的真实 DLL 已同时声明标准/极速/恐惧回调，但新模式回调未纳入重放，切换器重建背景后作者展示未恢复。现在三种原生类型由同一 IsScreenType 判定，重放时传实际 screen，仍校验目标实例与回调参数；不调用完整 SelectCharacter，不伪造标准界面或重放大厅/音效逻辑。
2. CznCore 的 ApplyScreenSelection 参数仅接受标准界面，但实际框架已有 ApplyModeBackground(ModeSelectionBackground) 和原生模式设置菜单。桥接真实 Screen/Host/Character/ShowMenu 事件对象，仅对当前选中提供者且目标角色匹配的 profile 调用，恢复希尔德及其他 CZN 皮肤所选背景与菜单；不用广播全部同角色 profile。
3. FoolSkinStatic 的选角 postfix 仅支持标准模式。新模式复用其 EnsurePortrait/BuildPortrait，在实际背景父节点创建作者的大图及眨眼等行为，成功后隐藏原 Spine；不自行生成或重画美术。
4. Framework/单图绘制放入同一次可逆呈现快照内，切走皮肤时能清理新增节点、恢复可追踪属性；afterReplay 即便异常也在 finally 记录变更。
5. GetCharacterSelectIcon 不再对未收录皮肤组的内容角色强行查询覆盖层；代码皮肤缺少规范图像时交回 CharacterModel getter，避免用原图覆盖作者内嵌头像。
6. 所有角色头像逐项刷新，单个失败仅影响该项，且普通 Icon 与高亮 IconAdd 同步设置。保留变体头像与锁定状态的已有逻辑。

原生模式的真实背景 host/子视口、恐惧灰度与各自大厅/开局规则保持。不修改第三方文件，不改变标准模式作者的界面类型。特殊代码包仍应以实际合同和画面核对，不能因共用入口就宣称所有未来皮肤完全兼容。

## 验证

- check-dual-branch.ps1：同一个正式版编译 DLL，在正式版 v0.107.1 与测试版 v0.113.1 通过 ABI、三模式入口、局内商人虚派发、运行安全、保护边界、预览绑定、商店与旧鸡煲合同。
- 新 --test-mode-presentation-parity：实际 Fool_Waifu Postfix 参数在各原生 screen 上正确绑定；实际 CznCore 模式事件接收真实 screen/host/character/menu，beta 的 6 个内置角色 × 2 个新模式共 12 次派发通过；实际 FoolSkinStatic 大背景构建入口、无皮肤组角色回退及两层头像写入检查通过。
- 额外读取并验证希尔德、Heidemarie、Tressa、Chizuru、Meirin 五款真实角色皮肤声明；beta 上实际 CznCore Harmony 所有权、商店/休息/战斗事件边界与 200 次嵌套替换回归通过。
- 正式版没有这两个新模式，模式事件检查按能力跳过；正式版通用绑定、头像与单 DLL 检查通过。当前安装的 CznCore 实际加载钩子只在对应 beta 上验证，不把新版框架的正式版可运行性作为已验证结论。
- --test-independent-fool-skins 三种真实打包方式与互斥选择通过；--test-crossbow-select 真实弩手派发通过；check-protected-characters.ps1 包括十字军与贞德 13 实际变体及调色/资源/退出保护通过。
- 旧 --test-czn-compatibility 聚合入口因本机未订阅其硬编码 Tiphera 游戏角色夹（3799519206）而未完整执行。本次改为上述已安装的五皮肤声明、实际 CznCore 及模式专项检查，不声称验证缺失的 Tiphera。
- Release 最终 0 警告 0 错误。没有启动或控制游戏实机验证，原生 Godot 最终布局、动画与所有联机组合不在离线覆盖范围。

内部版本 1.0.34.2，公开工坊仍为 1.0.34。本次部署四处并提交推送 Git，不上传工坊。
