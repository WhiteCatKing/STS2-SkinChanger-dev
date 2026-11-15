# 1.0.32.7：商店角色过小、商人首次切换及提供者隔离（2026-11-16）

## 证据与定位

作者截图：Hilde 商店玩家模型很小；选择工坊 3747990002（本机 ATA_Merchant）第一次仍原版，第二次正常。提供的 godot.log：第一次 ATA 切换在节点创建/骨骼变换记录后才执行 ATA 初始化（4031 附近重放 0、4035 基线缩放 0.4701，4038–4046 才初始化完成）；第二次已重放作者步骤，位姿为 (125,280)、缩放 0.15。

本机 Hilde DLL 对商店节点 SceneFilePath 精确比较 res://scenes/merchant/characters/fool_merchant.tscn，命中后才绑定身体并调用作者的 MerchantPresentationRuntime.Apply。切换器返回的私有 PackedScene 可以把节点 SceneFilePath 变成隔离路径，使这条作者的尺寸流程被跳过。作者的 CznCore 商店表面尺寸为其自身声明的 2.021，不应由切换器随意指定放大倍数。火堆也纳入场景身份边界检查，未宣称已实机看到同样问题。

## 修改

- 记录非战斗场景实际加载别名 -> 请求原始路径的对应关系（只限商店角色/火堆角色场景）。在 NMerchantCharacter/NRestSiteCharacter._Ready 最前恢复节点的 SceneFilePath，保留底层 PCK/资源的隔离路径；未知路径不按文件名猜测。这样作者自己的场景识别与尺寸/姿态流程可以正常执行。
- 激活计划增加 requiredProviderId，所选商人同步初始化，其余不相关后台任务继续延后。TryRefreshMerchant 创建第一颗节点前再次确保所选提供者完成激活。
- 商人缺少行为时直接完成既有提供者激活，不再仅重新聚焦场景并留下延后队列。
- 切走的 Merchant 提供者不再因为名称含 Merchant 而永久豁免停用；仅当前商人/行为别名获得保留，其他仍按正常选择集合管理。
- 删除旧的「临时 PatchAll 补装」通路及固定 ATA/a/b 坐标缩放纠偏函数，避免重复安装与写死参数互相覆盖。保留现有外观节点的事务式替换和失败回滚，不重建商店库存、商品、购买记录或玩法房间；没有实际执行 SL，也不调用房间玩法初始化模拟 SL。

## 验证与范围

- Release 0 警告 0 错误，LogicTests 全绿；增加所选商人同步、无关提供者仍延后的计划测试。
- --test-merchant-lifecycle：200 次商店/火堆场景身份恢复（拦截 Godot 属性隔离原生依赖）、初始化先于建节点的真实调用边界、旧商人隔离通过。
- --test-czn-compatibility：五个真实皮肤包声明/探测/目录、真实 CznCore 接口、两个非战斗 Ready 前缀安装与共享选择边界通过。
- --test-switch-resource-lifetime 和 --test-card-inspect-portrait 通过，保留此前修复。
- 未启动游戏，角色实际大小、火堆画面以及连续商人切换效果仍需实机确认。上述循环次数均为离线检查，不能当作游戏里切换次数。

## 交付

版本 1.0.32.7，三个运行文件同步四处并核对哈希；记录 CHANGELOG，提交并推送 origin/main，标签 v1.0.32.7。不上传工坊，不改第三方 Mod。保留任务开始前已有两份公告文档的删除状态。