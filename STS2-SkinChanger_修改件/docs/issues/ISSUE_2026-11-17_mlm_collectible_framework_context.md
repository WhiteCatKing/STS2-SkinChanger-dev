# 2026-11-17：1.0.33.6 新版 ModLaunchManager 隔离上下文兼容

## 根因
- 本地 godot.log 在启动 GameStartupWrapper 中出现 ReflectionTypeLoadException，内部重复 FileLoadException 指向 STS2-RitsuLib.Runtime 0.6.12，根异常为 A non-collectible assembly may not reference a collectible assembly。
- 玩家开启/关闭对照日志此前也显示同一错误导致多个皮肤的 RitsuLib 设置跳过；本地进一步放大为游戏枚举模组类型时中断启动。
- MLM 新 CollectibleModAssemblyLoader 默认使用可卸载上下文；切换器因解析器/加载操作被预检判定使用游戏上下文，托管皮肤随切换器加载，与处于可卸载上下文的共享框架不兼容。
- 核对实际 RitsuLib Bootstrap，其 runtime 模块沿用根 Loader 所在上下文，因此应修复框架根加载决策，不能在事后重复加载另一套 Runtime 或吞掉启动异常。
- 旧 MlmLaunchPlanSync 只同步排序，仍保留；不是本次隔离上下文冲突的修复。

## 实现
- 在 Entry 初始化早期安装反射兼容，不硬引用 MLM DLL。
- 在 MLM 真正读取程序集前，将 RitsuLib/BaseLib/JmcModLib/原皮肤管理框架及其声明依赖，调整为 MLM 原生游戏上下文加载决策。
- 兼容已缓存的启动计划；冷缓存仍调用 MLM 自己的 Build，再修正当前共享框架条目。循环依赖用 visited 集合终止。
- 继续由 MLM 原函数执行文件加载及 GameContextLoads 记录，保留需要重启才能移除的语义。不改启用状态、排序，不执行未选中皮肤初始化器，不改变无关模组的隔离策略。
- 不修改第三方 DLL/PCK/manifest，不强行注册所有未选中皮肤设置；本修复解决已执行设置初始化时的上下文异常。

## 验证
- 新 --test-mlm-context：以真实 CLR 动态程序集重现不可卸载引用可卸载接口失败，并验证统一共享上下文后可创建消费类型。
- 加载当前真实 MLM DLL，实际安装 loader Harmony prefix；验证私有决策结构、预缓存 false 决策修正、冷计划、循环框架依赖、无关内容/皮肤条目不接管及策略计算不执行模块初始化。
- 十字军/贞德完整保护及缓存/退出保护通过。Release 0 警告 0 错误。
- 未启动/控制游戏做实机验证；该修复必须在新进程、框架尚未装入可卸载上下文时生效，不尝试迁移当前已加载 CLR 程序集。

内测版本 1.0.33.6，公开版本仍 1.0.33。同步本地四处安装产物，提交推送 Git，不上传工坊。
