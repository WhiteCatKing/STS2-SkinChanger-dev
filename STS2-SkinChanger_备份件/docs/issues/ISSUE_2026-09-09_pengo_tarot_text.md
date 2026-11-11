# Pengo's Tarot Enchantments 文本破损

- 状态：**未解决**
- 来源：留言 오래전부터당신같은남자를기다려왓다우 2026-09-09（英文：“Pengo's Tarot Enchantments Pack ... When I open it, the text appears broken.”）
- 收录：2026-11-03

## 现象

打开 `Pengo's Tarot Enchantments Pack` 时文本显示破损（乱码/缺字/键值未解析），具体截图未提供。

## 初步判断

- SkinChanger 的 15 语言 `ModLocalization` 可能覆盖了该 Mod 的文本表，或注入的 locale 文件与它的键冲突；
- 也可能是该 Mod 自身文本表编码/格式问题，被 SkinChanger 的本地化加载顺序放大。

## 下一步

1. 取该 Mod 包，检查 `localization/` 下的键与编码；与本 Mod 注入的本地化键做差集。
2. 用只开该 Mod、不开 SkinChanger 对照，确认是否本 Mod 引起。
3. 若确认冲突，给 `ModLocalization` 加“只写本 Mod 自己的键”的白名单/命名空间隔离。

## 关联

- 未见于现有清单；与“小丑牌翻译失效”（8 月已当日修复）可能同源，但本次是另一个 Mod。
