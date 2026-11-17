# 1.0.34.3：鸡煲新模式背景与商人三场景

## 反馈及证据

1. 杂鱼鸡煲娘化在标准模式大背景正常，新模式仍为原版；小模型已正常。
2. Merchant2CuteII（工坊 3748331824）已有作者极速版本，但切换器仍显示原版；本地 Merchant2CuteIIB 旧改版出现商人/桌子消失。用户要求普通商人、假商人、极速商人三种场景对应同一包，缺少对应资源时回退原版，并新增极速图鉴入口。
3. 日志还记录图鉴 NProceedButton._Ready 访问没有初始化的局内同步器而抛 NullReferenceException。

## 鸡煲背景

作者 Postfix 的实例参数硬编码 NCharacterSelectScreen，无法直接传入新模式。LegacyDefectPresentationCompat 现在在选中此包的新模式里读取已预载的 SelectBgScene，实例化成功后再清除旧背景、加入 DefectAnimatedBg，并使用作者相同的全尺寸布局。沿用作者场景与动画，不伪造标准模式界面，不改变局内角色逻辑；标准模式原回调保留。

## 商人包和资源归属

- 商人资源规则补充原生极速背景/房间/按钮，以及 Merchant2CuteII 私有 blitz_merchant 资源。更新包包含的动画 animation/done/done_loop、皮肤 default/outline 和 TOKEN_1–6 已从实际 PCK 核对。
- 原作者 MerchantSceneSkinner.ApplyBeforeEnteringTree(Node) 会拒绝已进入树的节点；原处理只重放 _Ready，不会调用这个场景工厂阶段。新 MerchantVariantScenes 在所选资源作用域内创建场景，并定向调用作者入树前方法。
- 检查活动程序集；未活动时只通过既有 inspection 加载入口取得所选包的程序集，调用限定单棵树的 helper，不执行全局初始化器或 PatchAll。使用当前家族的 runtime 选择，避免必须同时选中假商人才允许普通/极速显示更新资源。
- 旧 Merchant2CuteIIB 实际不具备 BlitzMerchantCompatibility 合同，不套用其普通商人资源/变换；使用基础资源隔离重建原版极速场景。未知或不完整场景也尝试原生回退。
- merchant 与 fake_merchant_monster 的真实包选项补齐对方家族的缺失项。缺失项标记 IsMerchantVariantFallback，资源为空、不作为 runtime 提供者、不重放私有房间回调，并从完整包所有权分组计算中排除，避免人为增加激活条件。

## 三种图鉴与局内安全

- 其他图鉴新增「商人极速版」，翻译覆盖 15 语言，仅实际具备极速类型的游戏分支显示。
- 普通与极速行共用 merchant 皮肤选择；假商人仍用 fake_merchant_monster 选择。所有对应包都列出，作者没有制作的场景呈现原版。
- 使用实际 NBlitzMerchantRoom 与 NBlitzMerchantButton，不将普通按钮写入极速字段。入树前检查房间、按钮、商品、前进及六个 Token 等必要节点，失败使用基础场景。
- 极速热切换仅替换正确的原生按钮，不重建库存/商品模型。通过原生 Setup 绑定原库存，保留选择次数；次数用尽时调用原生 CompleteShop。旧按钮退出树时原生 Teardown 解除自己的购买事件。
- 不把普通商人的手部重建、普通位姿/透明状态、私有商店回调强加给极速场景。新场景保留原生布局。
- 替换前验证字段可赋值性；AddChild/初始化失败时撤回新节点、恢复旧名字和字段。新按钮绑定失败时仍保留旧商店，不先删除旧树。
- 图鉴初始化使用原生 CreateForBlitzMerchant；前进按钮仅在商人预览子树跳过 _Ready/_ExitTree 的局内同步器订阅/取消订阅，实际局内不受影响。
- 场景创建继续绑定当前外观身份，过期请求不提交。

## 验证与边界

- Release 0 警告 0 错误。同一个正式版引用编译的 DLL 通过正式版 v0.107.1 / 测试版 v0.113.1 的 check-dual-branch.ps1；旧鸡煲真实 DLL 的两场景预载与选角合同通过。
- --test-merchant-variants 两分支通过：两份实际商人 DLL/PCK、更新包极速骨架动画与槽位、私有极速资源归属、两家族选项、不激活代码的缺失家族回退、按分支显示第三行、200 次作者 helper 派发；beta 额外验证 200 次同库存 Setup、剩余次数保持、耗尽 CompleteShop、原生 Teardown 存在和错误按钮类型在碰树前被拒。
- 原版 PCK 的 blitz_merchant_room / blitz_merchant_button 已核对真实节点名字和关系，包含六个 Token 与 MerchantVisual/Reticle。
- beta 三模式呈现专项及实际 CznCore 所有权/多角色合同通过；CznStyleUI 仍由原生加载器拥有，不误归为皮肤。
- check-protected-characters.ps1 全部通过，包括十字军、贞德 13 实际变体、24 次战损调色与资源/退出保护。
- 没有启动或控制游戏实机验证；未声称 Godot 的最终画面、每个第三方资源包或全部联机组合已验证。首次新增离线测试缺少日志隔离曾触发无原生 Godot 的测试进程退出，补上 AuditLogSilencer 后专项通过，不是游戏崩溃实验。

内部版本 1.0.34.3，公开工坊仍 1.0.34。同步本地四处安装产物、提交推送 Git，本次不上传工坊。

最终生产 DLL SHA-256：820030BE2940F98D9A1BE978B599234A0E0CE92FFFE28F064CB6E01BB28B8C28。
