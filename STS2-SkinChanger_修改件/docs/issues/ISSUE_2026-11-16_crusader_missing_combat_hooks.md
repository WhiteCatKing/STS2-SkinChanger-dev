# 1.0.32.10：CrusaderSkin 局内动作只有前半段（2026-11-16）

## 证据与原因

作者确认 1.0.32.9 新商店问题解决，反馈工坊 3801530111 动作只播放一半。本机对应 CrusaderSkin 0.1.6（Jerry），日志显示在已有对局中切换为 CrusaderSkin::variant:banneret，作者的 14 个动画/场景 Harmony 行为已启用；选牌音效 smite_antic 出现，但模型注册日志明确表示当前游戏没有 SingletonModel，跳过单例补注册。

反编译本机原 DLL：选牌走 HoveredModelTracker 等 Harmony 补丁，触发 CardSelect/Smite；正式出招由 CrusaderCombatSingleton.BeforeCardPlayed 触发 CardPlay/Smite，受伤低血状态也由该模型监听。进入战斗后的初始化由 CrusaderRunstateSingleton.AfterRoomEntered 负责。这两类已经改为继承自带的 CustomCrusaderSingletonModel -> AbstractModel，构造函数通过游戏 ModHelper.SubscribeForCombatStateHooks / SubscribeForRunStateHooks 注册，不再继承游戏已删除的 SingletonModel。

切换器虽把晚加载程序集发布给模型发现，但 ModelDb.Init 已结束，而且补注册仅查旧 SingletonModel，因此两类监听模型未被构造。选牌前摇正常、正式出招钩子缺席，与反馈吻合。

## 修改

- 明确识别该版本 CustomCrusaderSingletonModel 的具体、可默认构造模型，不按名称模糊实例化第三方任意类型。
- 在模型数据库已初始化时通过 ModelDb.Inject 补注册两个现代订阅模型；先检查 Contains，重复激活不重建实例，也不重复订阅。
- 构造订阅前接入 CrusaderHelper.IsIronclad 的 Player/Creature 原有判断，保留作者返回 false 的条件，再按实际玩家的当前皮肤归属收窄。常驻订阅在切走皮肤后不会继续驱动其他皮肤，同角色队友也按自己的选择判断。
- 作者自己的卡牌映射、正式动画状态机、音效、特效、低血量与房间钩子继续运行，不硬编码一套替代动作时长，不更改原 Mod 文件。

## 验证与限制

- Release 构建 0 警告 0 错误；LogicTests 通过。
- 新增 --test-crusader-hooks <实际 DLL>：未运行提供者初始化器，在真实游戏模型数据库晚加载/补注册两类实际模型；连续补注册三次，ModHelper 仍各返回一份战斗/房间监听。
- 通过真实 CrusaderCombatSingleton.BeforeCardPlayed 核验 CardPlay/Smite 被触发；选中/切走/再次选中/再次切走均正确；同角色其他玩家和原判断不匹配角色拒绝通过。测试隔离 Godot 资源读取、实际原生动画播放与日志，映射使用 Smite 的受控桩，不声称渲染完整动作。
- CZN 实包/角色归属、商人生命周期/新店入口回归通过，保留先前修复。
- 未启动/控制游戏，实际各套变体、所有卡牌/低血量动作的画面未实机覆盖。工坊网页本次不可访问，以本机实际已安装 DLL、清单、配置和日志为证据。

## 交付

版本 1.0.32.10；三个运行文件同步四处并核验 SHA-256，更新 CHANGELOG、START_HERE，提交 main 与标签 v1.0.32.10 并推 GitHub。不上传 Steam 工坊，保留任务前两份公告文档的删除状态。
