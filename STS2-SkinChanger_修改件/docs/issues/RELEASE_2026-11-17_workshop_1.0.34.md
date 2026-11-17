# 2026-11-17 创意工坊发布 1.0.34

用户明确授权更新工坊。公开版本 1.0.34，内部 1.0.34.0，汇总此前 1.0.33.1–17 的兼容与诊断修复；此次仅调整发布版本和说明。

- 皮肤检测、选人预览、战损背景、商店生命周期和 MLM 顺序提示等修复随包发布。
- 正式版 v0.107.1 与测试版 v0.113.1 同一 DLL 离线 ABI、选人模式、运行安全、保护边界、预览、商店、真实旧鸡煲包检查全部通过；Release 构建 0 警告 0 错误。未启动或控制游戏。
- Windows 自动崩溃诊断无需手动开启外部工具；本 Mod 加载前的崩溃不在监视范围。
- 15 种语言同步发布版本、功能更新和双分支离线兼容说明；保留主图与 12 张附加预览图。
- 通过既有 WorkshopPublisher 与已登录 Steam 客户端更新既有条目 3787302680；15 种语言逐一 Verified localization，PUBLISHED_FILE_ID=3787302680，LEGAL_AGREEMENT_REQUIRED=False，退出码 0。
- 发布 DLL SHA-256：F6CD9D937CE7B9F569550735605A90AA790A7F1A5364B05C7251A9F4DEB08213。
- 三个文件同步仓库两套安装包、工坊 staging、游戏测试 Mod 目录及 Steam 工坊内容目录，逐文件哈希一致。

页面：https://steamcommunity.com/sharedfiles/filedetails/?id=3787302680

工坊成功后提交 Git main，标签 v1.0.34 / v1.0.34.0。
