# 1.0.32.8：局内角色切换被其他 CZN 皮肤覆盖（2026-11-16）

## 证据与原因

作者确认上一版商人 NPC 已正常；本次截图显示商店玩家从 Hilde 切为 foolSkin 后仍是 Hilde。godot.log 中选择已保存，Hilde 行为补丁已停用，foolSkin 商店资源也已生成，但仍出现 Tiphera 与 Heidemarie 的 merchant presentation，后者提示 without player mapping。

反编译本机 CznCore 1.1.4：MerchantPresentationRuntime.ResolveMerchantCharacterPlayer 用节点 GetIndex 查房间玩家列表。热替换中旧/新节点共存、联机节点重排时，此索引不可靠。共享 Apply 对空玩家跳过适用性检查；SkinEventOwnership.Accepts 未对 MerchantCharacterReadyEvent 过滤，而 Hilde 自有事件回调在调用共享 Apply 之前就已绑定自己的骨架。因此仅停用皮肤 Harmony 补丁或提高优先级不能修复。

## 修改

- 商店玩家热替换与联机建模的入树阶段携带明确的 Player + NMerchantCharacter 配对作用域；嵌套、异常退出均恢复先前归属，不把本地归属套给其他节点。
- 共享玩家解析改用作用域配对、房间 PlayerVisuals 引用和单玩家兜底，不再按节点兄弟序号猜测。
- 共享 MerchantPresentationRuntime.Apply 在任何模型写入前检查实际玩家的适用谓词；MerchantCharacterReadyEvent 在调用作者回调前检查 profile.AppliesTo(owner)，阻止错误皮肤先行绑定骨架。无法确定归属的受管皮肤事件关闭，非受管原生目录上下文保留原流程。
- 不修改第三方 DLL、商人库存/购买状态或上一版商人 NPC 流程。

## 战斗与火堆核对

本机 CznCore 的火堆 Ready/翻转事件通过 Node.Player，回合/受伤等战斗事件通过实际 Player 调用 profile.AppliesTo；BodyAppearanceResources.Apply 也在写入前检查 owner 与 profile。切换器既有共享选择边界和火堆 Create/Ready 玩家作用域覆盖这些路径，战斗刷新重建受影响的玩家模型。未发现商店的按兄弟序号猜测归属漏洞；保留这些流程，新增真实火堆 Ready、战斗回合事件的外来 profile 拒绝回归。火堆缺失玩家的拒绝与战斗不匹配角色的拒绝为离线检查，不等于逐个动作实机覆盖。

## 验证与交付

- Release 构建 0 警告 0 错误；LogicTests 通过。
- CZN 实包扫描/目录、真实 CznCore Harmony 安装、共享选择/卡图边界通过。
- 新增真实商店玩家解析、事件前置拒绝、共享 Apply 谓词，以及 200 次嵌套配对作用域恢复；火堆与战斗事件拒绝测试通过。
- 商人生命周期、连续切换资源生命周期、大卡卡图专项回归通过。
- 未启动或控制游戏，未声称验证实际战斗/火堆画面或消除所有原生崩溃。
- 三个运行文件同步既有四处目标并核验 SHA-256；版本 1.0.32.8，提交 main 与标签 v1.0.32.8 并推 GitHub，不上传工坊。保留原有两份公告文档删除状态。
