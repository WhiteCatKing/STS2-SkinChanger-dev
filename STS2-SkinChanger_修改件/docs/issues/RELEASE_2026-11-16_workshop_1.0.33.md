# 2026-11-16：创意工坊正式发布 1.0.33

用户授权将当前版本用于更新创意工坊，并同步 Git。以已交付 1.0.32.15 为基线，发布版本改为三段 1.0.33、内部标记 1.0.33.0；没有新增业务逻辑修改。

## 发布内容
- 汇总近期角色皮肤/背景/预览/头像一致性、CZN 兼容、十字军动作、贞德变体/战损、商店/火堆切换、大卡卡图与瓦库背景修复、空闲预读与缓存保护。
- 英文和其余 14 种语言同步更新 featureUpdate、页面版本、适用游戏版本说明及 GitHub 仓库/投稿指南链接；修正繁体说明的重复 h2 标签。
- 页面明确本次针对 v0.113.1，未重新验证旧分支；保留特殊代码皮肤兼容及订阅重启提示，不额外承诺完全兼容。
- 保留已有主图与 12 张附加预览图；更新中英文 changenote，致谢提供瓦库补丁与复现信息的玩家。
- 源码 workshop 配置的 contentFolder 指向仓库既有 STS2_Work/workshop_content_1.0.20，避免继续引用不存在的 content 目录。实际本次上传前亦同步 Steam 工坊内容目录。

## 发布验证
- 使用仓库现有 tools/WorkshopPublisher，通过已登录的 Steam 客户端 API 更新条目 3787302680；没有新建条目，没有 SteamCMD 密码操作，没有启动游戏。
- Steam SubmitItemUpdate 成功，英文及 schinese/tchinese/japanese/koreana/russian/german/french/italian/polish/brazilian/thai/turkish/latam/spanish 共 15 种语言逐一回读 Verified localization。
- PUBLISHED_FILE_ID=3787302680；LEGAL_AGREEMENT_REQUIRED=False；工具正常退出 0。
- 工坊安装包 DLL SHA-256：4A8C0F85001697CB583C12B9A032F6AA6FAF8D18CBC6BC8D4498F2F1E0625D28。
- 三文件同步两套仓库安装包、workshop staging、游戏测试 mod 目录和 Steam 工坊内容目录，逐文件 SHA-256 一致。
- Release 0 警告 0 错误；战士保护脚本全部通过（真实 13 变体/正式出招、资源路径 200 次换主、安全/退出边界、预览/加载队列），瓦库背景专项通过。没有实机渲染测试。

页面：https://steamcommunity.com/sharedfiles/filedetails/?id=3787302680 。随后提交 Git main，并推送正式标签 v1.0.33 和内部标签 v1.0.33.0。
