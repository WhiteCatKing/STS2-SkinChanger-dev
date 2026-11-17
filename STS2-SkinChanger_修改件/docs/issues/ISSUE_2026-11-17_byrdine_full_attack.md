# 异鸟成鸟攻击缺少全屏俯冲

内部版本 1.0.35.2，工坊公开版仍 1.0.35，本次不上传。

用户确认前一轮其它生物显示正常，但成鸟的攻击仅有起飞/落地，缺少官方全屏冲刺和红色拖尾。

## 原因及原生合同

实际 GodByrdPower.AfterPlayerTurnStart 流程为 TriggerAnim("Attack")，等待 0.6 秒，播放 event:/sfx/byrdpip/byrd_2_attack，创建 NByrdineAttackVfx，再等待 0.15 秒执行伤害。中间画面使用独立 scenes/vfx/monsters/byrdine_overlay_vfx.tscn，其 SpineSprite 播放 attack_overlay，包含 fx_placeholder 槽位上的拖尾材质。原图鉴只播放身体 attack 并衔接 idle，未创建该特效。

## 修复

- 成鸟攻击按钮转交 ByrdineAttackPreview：立即播放原身体攻击/落地链，按官方 0.6 秒间隔播放独立全屏特效与原生音效。
- 使用实际原生场景，保留 SpineSprite、SpineSlotNode、ShaderMaterial 和作者比例；在预览视口中心展示，不套用小模型缩放。
- 原生特效根节点的 _Ready 依赖真实 Creature，原生 Create 依赖战斗 VFX 容器。预览不调用它们，而在入树前取出完整 SpineSprite 子树并释放未入树根节点；复用成鸟原生羽毛/眼睛组合设置，直接播放 attack_overlay。
- 选中资源的加载、实例化和入树保持在相应资源作用域中。正式版无需引用测试版专属类型。
- 每只预览鸟仅允许一个活动序列。重复点击先取消旧序列，播放完成、切换条目、关闭图鉴、鸟退出树均清理；取消时停止 Tween、解绑 TreeExiting、隐藏并释放特效。额外有最长等待保护，缺失完成信号也不会无限残留。
- 仅预览视觉，不创建真实宠物/敌人、不调用伤害和能力移除命令。

## 验证与交付

- Release 正式引用构建零警告零错误；同一 DLL 通过正式版 v0.107.1 / 测试版 v0.113.1 的双分支 ABI、三模式、预览、运行时安全与商店生命周期检查。
- --test-byrdine-attack 两端通过：读取真实特效 PCK，验证独立骨架、attack_overlay、槽位拖尾材质和原生缩放；beta 额外检查真实 GodByrdPower 异步状态机的 0.6 秒、音效和 VFX 工厂调用，防止凭猜测填写时序；验证按钮入口、原生根入树前抽取、取消与不调用伤害工厂的边界。
- --test-extra-creatures 两端通过，保留正式版三项/测试版五项、奥斯提共用选择和显示路径、动作合同与事件清理。
- 未启动或控制游戏，最终全屏画面与声音尚未实机验证。

生产 DLL SHA-256：5C6919D61E9AA1E6A81E7A98461E7CB00662EDBCD1BE05F2FAABF043B213A804。
