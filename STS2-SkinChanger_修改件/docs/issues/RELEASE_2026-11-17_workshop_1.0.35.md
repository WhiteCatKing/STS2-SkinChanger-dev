# 创意工坊发布记录：1.0.35

- 日期：2026-11-17。
- 公开版本：**1.0.35**；内部/程序集版本：**1.0.35.0**。
- 条目：[皮肤切换器 / Skin Changer](https://steamcommunity.com/sharedfiles/filedetails/?id=3787302680)，App ID `2868840`。
- 发布工具：修改件 `tools/WorkshopPublisher`，配置 `workshop/workshop.json`，沿用原条目。
- 发布结果：工具返回成功，`PUBLISHED_FILE_ID=3787302680`，`LEGAL_AGREEMENT_REQUIRED=False`。
- 已同步 12 张附加预览图；英语及简中、繁中、日语、韩语、俄语、德语、法语、意大利语、波兰语、巴西葡语、泰语、土耳其语、拉美西语、西班牙西语共 15 种语言全部 `Verified localization`。

## 内容

汇总已经提交的 `1.0.34.1`–`1.0.34.4`：局内切换与极速弩手点击切换、三模式选人背景/头像一致性、鸡煲娘化新模式背景、三种商人场景与原版回退、新增极速商人图鉴，以及未点击角色的保存皮肤头像刷新。最后一项修复后用户已反馈「没问题了」并授权本次发布。

## 验证及产物

正式版 API 的 Release AnyCPU 构建零警告零错误，同一生产 DLL 在正式版 v0.107.1 / 测试版 v0.113.1 通过双分支检查；头像实际 DLL 专项和三模式/CznCore 专项再次通过。此前共享入口变更后的完整十字军/贞德保护回归保持通过。没有控制游戏做实机测试，用户反馈与离线测试范围分别记录。

部署前确认游戏未运行。两个 DLL 与 SkinChanger.json 同步至仓库正式/测试安装包、工坊 staging、游戏本地测试安装目录和 Steam 订阅目录，逐文件哈希一致。

生产 DLL SHA-256：`F45A37EE8247ECCBBB0B4A887A9AFA4B7B1A983E2D7FF86FE285D2AF502EE741`。

发布标签：`v1.0.35`、`v1.0.35.0`。先完成 Steam 上传与语言回读，再提交并推送 Git。

累计变更记录见仓库根 [CHANGELOG.md](../../../CHANGELOG.md)。
