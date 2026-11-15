# Pengo's Tarot Enchantments 文本破损

- 状态：**已定位，1.0.27.74 已做兼容修复，待实机验证**
- 来源：留言 오래전부터당신같은남자를기다려왓다우 2026-09-09（英文：“… the text appears broken.”）
- 收录：2026-11-03；实包复核：2026-11-07（工坊 3747679239，`PengoTarot` v1.4.16-b100417）

## 这个 Mod 是干什么的

`Pengo's Tarot Enchantments Pack`（内部 id `PengoTarot`，工坊 3747679239，作者 Pengo）：

- 44 张 Balatro 风格塔罗牌 + 12 张星球牌 + 40 种附魔；
- 自带一整套自己的 UI：主菜单 Balatro 入口、塔罗/星球卡检视界面（`NBalatroInspectScreen`、
  `NBalatroInspectEnchantScreen`）、便携塔罗弹窗、商人塔罗位；
- 文本在 `PengoTarot/localization/<lang>/{cards,enchantments,gameplay_ui,...}.json`，
  中文（zhs）齐全（44 张牌 + 附魔都有 `TAR_*` 键，值如 `0-愚者-正`），编码 UTF-8 正常。

## 根因（不是本地化表，是字体）

它的 Balatro 风格界面把**自己打包的像素字体** `kreon_bold.ttf` 硬编码到大部分 Label 上，
而这个字体**只有拉丁字形**。反编译片段：

```csharp
Label node4 = ...GetNode<Label>("EffectHeader");
node4.AddThemeFontOverride("font", BoldFont);   // BoldFont = Kreon
node4.Text = Tr("LABEL_EFFECT");                // 中文 = “效果”
Label node5 = ...("IntensityHeader");
node5.AddThemeFontOverride("font", BoldFont);   // 同上
...
node2.AddThemeFontOverride("font", LocaleFontUtil.GetLocaleFont(FontType.Regular) ?? BoldFont);
```

只有个别 Label 走了 `LocaleFontUtil` 拿语言字体，其余（效果/强度/部件标题、部件行、卡面文本等）
全部用 Kreon。于是中文/日文/韩文玩家看到的就是**一排排空方框**——即“文本显示破损”。
英文玩家完全正常，所以这个缺陷容易被忽略。

（游戏侧 `FontManager.NeedsFontSubstitution("zhs") = true`，`ZhsFontPathSet` 里
Regular/Bold/Italic 三个字体都齐全，所以只要把字体换过来就能显示。）

## 兼容修复（1.0.27.74）

新增 `Core/PengoTarotFontCompat.cs`：皮肤切换器启动时（或该 Mod 稍后加载时）给它的 4 个界面类
`_Ready` 加 Harmony postfix，节点就绪后递归遍历子树，凡是**文本里含 CJK 字符**的
`Label`/`RichTextLabel`/`Button`，就用游戏的 `FontManager.GetSubstituteFont(lang, Bold)`
覆盖字体；纯 ASCII 的（数字、`0-`、排名）保留它的像素字体，外观不变。

- `_Ready` 后立即跑一次，并在 0.15 秒后再跑一次（它的文本是 `_Ready` 里逐节点填的）；
- 只在需要字体替换的语言（jpn/kor/zhs/zht/tha/rus/pol）下生效，英文下完全不介入；
- 不改对方任何文件；该 Mod 没装时自动跳过；安装失败只打一条 Warn。

## 你要怎么复现 / 验证

复现（旧版本）：

1. 安装 `PengoTarot`（工坊 3747679239 或本地 `mods\PengoTarot`），游戏语言设为**简体中文**。
2. 进一局（或主菜单 Balatro 入口），打开塔罗/附魔的 Balatro 检视界面。
3. 旧版本（1.0.27.73 及以前）：界面里“效果 / 强度 / 部件”等中文字样显示为空方框。

验证（1.0.27.74）：

1. 同上进入该界面：中文正常显示，数字/英文仍是像素风字体。
2. 日志出现一次：
   `[SkinChanger] Pengo's Tarot Enchantments 字体兼容已启用：…`
3. 把游戏切成英文再进同一界面：与之前一致（桥接不介入）。

## 已知边界

- 只修“含 CJK 的文本节点”；如果它的某个界面在**运行中**再新建 Label（不是 `_Ready` 里建的），
  那一批可能仍用像素字体，需要在日志/截图里定位后补 hook。
- 这是**对方 Mod 自身的缺陷**（硬编码拉丁字体），本兼容只是替它在中文环境下兜底；
  更彻底的做法仍是让 Pengo 在 `Tr(...)` 的 Label 上统一走 `LocaleFontUtil`。
