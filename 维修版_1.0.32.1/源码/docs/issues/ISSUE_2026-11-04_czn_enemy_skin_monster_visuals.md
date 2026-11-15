# CZN 怪物美化包（CznEnemySkin）的怪物模型不生效

- 状态：**已解决**（1.0.26.3 实机确认，日志出现 `Prepared enemy replacement visual`）
- 记录日期：2026-11-04
- 反馈者：卡洛蒂（皮肤切换器原作者）；当前接手作者复现并排查
- 相关 Mod：`CZN Enemy Skin [CznEnemySkin]` 工坊 3783041069（依赖 `CznCore` 工坊 3826011578）

## 现象

CZN 系列里角色、地区背景、BGM、Boss 图标等都由皮肤切换器正常接管，唯独怪物模型始终显示原版：

- 日志有 `已按当前选择启用 CZN Enemy Skin 的完整视觉会话`、`已接管 CZN Enemy Skin 的逐怪物启用判断`、`已低优先级挂载 CznEnemySkin 的完整运行时资源`；
- 但整份 `godot.log` 没有任何 `[CznEnemySkin] Prepared enemy replacement visual`，也没有 `Enemy replacement visual failed`；
- 即作者自己的 `MonsterModel.CreateVisuals` 前缀从未执行。

## 原因

CznEnemySkin 不是资源替换型怪物皮肤，而是逐怪物运行时外观包：

1. 它按怪物各带一份 `EnemyReplacementProfile`（宿主场景＝游戏原版 `res://scenes/creature_visuals/*.tscn`，骨骼数据＝自己的私有 `res://CznEnemySkin/animations/monsters/...`）；
2. 真正的替换发生在它给 `MonsterModel.CreateVisuals` 打的 Harmony **前缀**里：实例化原版宿主场景后把自己的 Spine 骨骼塞进去，并直接返回该节点；
3. 它的 PCK 里没有任何 `scenes/creature_visuals/*` 规范路径，因此皮肤切换器按“规范资源”接管时没有任何东西可替换。

皮肤切换器在隔离提供者时，把 `MonsterModel.CreateVisuals` 判成“资源所有权补丁”并卸载（日志中的 `3 个资源/选角呈现入口已交由本 Mod 接管` 即 `MonsterModel.CreateVisuals`、`NCreature._Ready`、`NBossMapPoint._Ready`）。作者的前缀被卸载后：

- 逐怪物 `IsEnabled` 路由仍在，但没有任何代码再去创建替换模型；
- 皮肤切换器自己的 `MonsterVisualResultPatch` 后置补丁再把结果实例化成规范宿主场景（原版模型），于是每个怪物都回到原版。

## 修复（1.0.26.3 测试补丁）

1. 对“逐怪物运行时外观提供者”（`ProviderUsesScopedMonsterRuntime`）保留其 `MonsterModel.CreateVisuals`、`NCreature._Ready`、`NBossMapPoint._Ready` 三个回调，不再按资源所有权/节点就绪补丁卸载。作者的补丁自身会核对 `IsEnabled(profile)` 与已应用的 profile，不会污染未选中的怪物。
2. 当前选择是该提供者的逐怪物运行时配置（`SkinOption.IsManagedMonsterRuntimeProfile`）时，皮肤切换器不再用规范宿主场景覆盖作者创建结果。
3. 新增离线策略 `MonsterRuntimePresentationPolicy` 与逻辑测试，锁定以上两条规则。
4. 未修改 CznCore / CznEnemySkin 的任何文件、DLL、PCK 或清单。

## 验证

- 离线：LogicTests、RuntimeTests 通过。
- 实机：等测试补丁在游戏内验证后更新本文件状态。
