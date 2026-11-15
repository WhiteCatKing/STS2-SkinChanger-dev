# 十字军、贞德及其变体兼容边界

此目录由用户要求归拢，保留原有命名空间、类型、FeaturePatch 标记和对外接口。文件移动不改变运行时发现方式，也不新增重复的 Harmony 初始化。

| 能力 | 保护模块 |
| --- | --- |
| 十字军正式出招、低血量/房间钩子与玩家选择限制 | CrusaderHookBridge（ProviderModelDiscovery 负责实际补注册） |
| 十字军 Default/Aggressor/Banneret/Templar/Infernal | ProviderVariantOptions、ProviderVariantPathPatch；原状态机/映射由提供者执行 |
| 贞德两包、四种调色变体、同身份程序集行为代执行 | ProviderSettingsVariantOptions、VariantPackCapabilities、ProviderMenuGateCompat |
| 贞德选人/局内头像、战损阶段、点击形态 | VariantPortraits、CharSelectStageCycle、CharSelectPackRebindCompat、CharSelectTakeoverSuppressor |
| 作者快捷键与下拉选项一致性 | VariantHotkeyBridge、VariantHotkeyPoller（保留已有停用标记状态） |

不能把两个贞德包按程序集简单名混为同一选择：SFW 包只取自身资源，不能继承另一包的玩家战损行为；菜单/头像的同源代执行与玩家作用域有不同规则。十字军不能只验证选牌前摇，必须验证 BeforeCardPlayed -> CardPlay/Smite。

共享资源管线留在原位置，在本目录之外调用这些接口；不得用 Free/Dispose 删除活跃资源，也不得把旧 UI 预览当成战斗 NCreature 重放。

1.0.32.12：完整依赖图可使用等价的逐节点隔离：先按拓扑顺序加载本选择的私有别名资源，再临时绑定规范路径，最后加载引用者。缓存复用必须匹配同一不可变别名，不能仅按规范路径复用。图不完整仍使用 IgnoreDeep 深度隔离；跨组/联机作用域结束恢复原所有权。真实 13 变体检查完整图及原有动作/行为断言，禁止退化为跨皮肤共享可变骨架。

回归入口 tools/check-protected-characters.ps1：真实十字军 DLL 正式出招和切走限制、真实三包目录/变体、手动变体/作用域/动画发现，以及资源预算和失败熔断。该检查验证托管合同和实际包数据，不等价于实机动画渲染。

1.0.32.13：有 s2/s3 的贞德包由 SC 持久点击接收层独占处理战损（消费事件，尊重 WearEnabled），不能把作者接收器可见/Stop 当成能够工作的证明。切换变体后必须重新绑定当前包战损资源并执行调色；三档使用同一个隔离根集合，禁止缓存优先借用另一包资源。无战损包不得新增战损能力。

1.0.32.14 根因补充：作者调色/战损以 ResourcePath 识别包，背景和小预览并存时不得让规范路径抢占清空旧背景 atlas/skeleton 的路径。旧资源先安全迁回私有路径（别名碰撞也要保护），再让新图接管；scope 恢复遵守同一规则。保护脚本新增必跑真实语义的路径清空/200 次替换回归。
