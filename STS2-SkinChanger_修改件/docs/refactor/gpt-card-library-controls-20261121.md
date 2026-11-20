# #260 卡牌总览皮肤控件恢复

## 根因

#259 的归档日志 `godot2026-11-21T01.53.12.log` 第 2991、3235、4668 行均指向 `NLibraryStatTickbox.SetLabel`，前一行是空引用错误。#257 的 01:31 和 01:56 对照日志均无此异常。卡牌总览的 `_Ready` 后置调用 `CardSkinControls.Attach`，在工具栏入场景树之前实例化并加入 `NLibraryStatTickbox`，接着就调用 `SetLabel`。筛选框尚未执行 `_Ready`，所以 `SetLabel` 访问标签时抛出 `NullReferenceException`，整个工具栏还没挂上屏幕。这个异常发生在执行 `CardGrid.OffsetTop` 赋值之前。云的 #257 / #259 实机对照确认，两版均有遗物修改器文字按钮，本次处理图鉴控件和日奈头像。

## 修复

`CardSkinControls.Attach` 将皮肤优先级、预设和「有皮肤卡牌」放在左侧栏 `Sidebar/MarginContainer/TopVBox` 的最上方，搜索框之前。第一行是两个按钮，第二行是筛选框。先把工具栏挂入原生侧栏，再加入筛选框触发 `_Ready`，最后设置标签和信号。卡池联动、来源标记、筛选重置走同一处节点查找。恢复卡牌网格由原生场景布局。

直接检查正式版 v0.111.1 和测试版 v0.113.1 的 `card_library.tscn`。两版都有同一个原生侧栏顶部容器、相同的侧栏尺寸和左下返回箭头布局。回归断言验证工具栏作为顶部第一个子项、挂载先于标签读取、代码不访问卡牌网格，以及两档视口下控件避开返回箭头（含阴影）。另两项日奈卡图补丁和奥斯提提示继续通过原测试。

## 验证

- 正式版 Release 与测试版 ReleaseBeta 均 0 警告、0 错误。两端的 LogicTests、完整无参 RuntimeTests 和 `check-protected-characters.ps1` 五项全部通过。
- `git diff --check` 通过。构建和测试日志位于 `%TEMP%\sts2-gpt-260\official.log`、`beta.log`。未提交、未部署。

## 实机复测

1. 正式版和测试版各打开一次卡牌总览。左栏搜索框正上方应显示工具栏。选择已安装卡图包的卡池，确认第一行显示皮肤优先级与预设，第二行显示「有皮肤卡牌」。
2. 点开并关闭两个弹窗，勾选和取消筛选框，切换卡池后退出并重新进入。确认计数、筛选重置、左下返回箭头和第一排卡牌显示正常，并检查日志中是否仍出现 `NLibraryStatTickbox.SetLabel` 异常。

## #262 日奈的图鉴卡池头像

实测对照中，#257 有日奈头像，#259 回落到原版。检查实装的 `SorasakiMana.dll`，`FoolPortraitPatch` 只给 `CardModel.PortraitPath` 安装后置。角色光环由两个独立后置提供。日志所说的 `IconScene` 来自 `CharacterIconScenePatch`（`CharacterModel.Icon`）。图鉴卡池头像的实际路径是 RitsuLib 的 `CardLibraryCompendiumPatch.Postfix → SyncExistingFilterIcons → TryGetCharacterIconTexture`，读取由 `CharacterIconTexturePatch` 提供的 `CharacterModel.IconTexture`。#257 实测日志第 3175–3179 行有这条调用链。#259 在同一个 `NCardLibrary._Ready` 的切换器后置里抛出 `SetLabel` 空引用，后续图标同步失去执行机会。两条光环后置在资源接管判断中一直放行。#259 日志第 2774 行显示 5 个入口接管、保留 55 个补丁。#257 的 01:31 归档第 3232 行及 01:56 运行日志第 2750 行均为 4 个接管、保留 56 个。结合分类器的改动和实装 DLL 的目标，这 1 个差额对应 `FoolPortraitPatch` 的 `CardModel.PortraitPath` 后置。修复后的头像画面仍待实机复测。

沿用 #260 的入树顺序修复，并将卡图隔离收窄到以 `CardModel.PortraitPath` 为基定义的 getter。测试继续用真实 Harmony 后置复现并撤销卡图劫持，同时检查 `CharacterModel.Icon`、`IconTexture`、`CardPoolModel.DeckEntryCardColor` 及 `CardModel.Portrait` 保留原处理。另用实装日奈 DLL 的三个目标与回调对新版分类器逐一核对，只有 `FoolPortraitPatch` 被隔离，并核对了实装 RitsuLib 的图鉴读取入口。

正式版 v0.111.1 和测试版 v0.113.1 均 0 警告、0 错误，LogicTests、完整 RuntimeTests 和受保护角色实包五项通过。复测日志与 IL 核对在 `%TEMP%\sts2-gpt-262`。未提交、未部署。

实机复测时选日奈为愚者角色皮肤，把愚者卡图设为「游戏默认」。退出并重进卡牌总览，比较愚者卡池头像（应为日奈光环）与打击／防御卡（应为原版）。再选日奈卡图并切回默认，确认只有卡图随卡图选项变化。最后切走日奈角色外观并重进，确认卡池头像随角色外观切走。台词和遗物修改器按钮是 A/B 两版均有的行为，未做变更。
