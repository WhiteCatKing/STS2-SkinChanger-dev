# 1.0.32.4：点开大卡后卡图与选择不一致（2026-11-16）

- 症状：总览显示 A，点开大卡却显示 B，上方皮肤下拉仍是 A；手动重新选择皮肤后正常。
- 根因：核对本机游戏 sts2.dll，NInspectCardScreen.UpdateCardDisplay 在 NCard.UpdateVisuals 之后调用 SetPortraitTexture(cardModel2.BigPortrait)，覆盖已应用的皮肤卡图；原有 inspect 后置补丁只同步下拉名称。
- 修法：在现有 CardInspectSkinDisplayPatch 后置补丁中重新应用所选卡图，再同步外部卡牌编辑器的最终呈现，最后同步选择框。复用卡牌正常呈现及外部所有权判断，不更改选择配置，不重载卡牌，不改变升级预览或玩法。
- 覆盖入口：打开大卡、左右翻卡、切换升级预览均经过 UpdateCardDisplay。
- 未改动：下拉焦点/悬停预览逻辑。本次问题发生在大卡显示的最终贴图覆盖，不是历史 IdFocused 问题。
- 验证：Release 构建 0 警告 0 错误；LogicTests 通过；--test-card-inspect-portrait 通过（核对真实游戏 IL 的覆盖顺序与最终补丁调用边界）；--test-card-art-packs 通过（实包维度检查因测试默认工坊目录未找到而跳过）。
- 验证限制：额外 --test-card-layout-reuse 在未初始化 Godot 的离线测试进程中调用 NodePath 原生接口时以 0xC0000005 退出，未通过；未修改与本修复无关的测试基础设施。未启动游戏，实际画面尚未真机验证。

## 后续交付流程（作者本次明确要求）

在工作2内修改；每次修改记录症状、根因、改法及验证结果，提升四段内测版本；构建及离线验证后，将 Gurio.SkinChanger.dll、thunninoiSkinManager.dll、SkinChanger.json 同步到本地安装包与游戏 mods/STS2皮肤切换器_mod测试版安装包，核对哈希；提交本地仓库并推送 origin 当前主线。Steam 工坊上传仍须单独明确要求。保留任务开始前已有的无关改动。