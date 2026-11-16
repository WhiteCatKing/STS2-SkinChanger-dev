# 2026-11-17：1.0.33.7 MLM 加载顺序前置报错与一键整理

## 证据与边界
- 当前 godot.log 与用户粘贴日志均显示切换器 1.0.33.6 在多个皮肤/框架之后才初始化，随后 ReflectionTypeLoadException 指向已经处于 collectible 上下文的 RitsuLib.Runtime。
- CLR 程序集不能在本次运行中事后迁移；上次的上下文策略兼容必须在框架加载前生效。排序过晚时不能继续假装修复成功。
- 自己的 DLL 尚未加载时无法执行自己的检查。因此用 MLM 在执行 DLL 之前读取的清单声明配合初始化器最前面的错误检查，而不是假称能在未加载时安装补丁。

## 实现
- SkinChanger.json 增加 MLM 原生 loadPriority=first。MLM 能在执行切换器 DLL 前读取，自己的优先级提示与一键整理会遵循此声明；MLM 仍保持第一个。
- Entry.Initialize 第一个调用执行 MlmEarlyLoadOrderGuard。仅 MLM 已加载时生效；用同一套元数据皮肤检测器识别已先行加载的皮肤，并检测已经隔离装入的共享框架。
- 错误顺序直接从初始化器抛出说明性异常，由 MLM 的 ResumeNext/ContinueAfterStep/HasLoadError/ShowLoadError 原有流程停止加载并展示错误，避免等游戏主菜单或类型扫描才提示。
- 错误文本明确要求将切换器放在所有皮肤之前、MLM 正下方第 2 位，提示返回一键整理、保存后重启，不要忽略错误继续。
- 如果玩家曾手动忽略本模组声明的优先级，在顺序拒绝时只恢复切换器自己的声明，由 MLM 当前计划/组织 API 处理；不改变其他模组的忽略偏好，不代替玩家写启动配置。
- 未装 MLM 时保留旧排序提示；未开启皮肤、不相关内容 Mod 不因本检查被阻止；不修改第三方 DLL/PCK，不执行作者初始化器，不挂载皮肤资源。

## 验证
- 新 --test-mlm-order-guard 使用真实 MLM DLL 和实际十字军 PCK：有/无游戏基线均识别前置皮肤，调用 Entry 时先抛错而不执行初始化；真实 MLM HasLoadError 识别此错误。
- 实际读取本清单确认 First；真实 LaunchPlan.AutoSortDependencies 一键整理后 MLM 第 1、切换器第 2；恢复自己的 IgnorePriority 后依旧如此，其他条目的偏好保留。
- 覆盖错误顺序/正确顺序、collectible 共享框架、不相关内容、关闭皮肤、无 MLM 等边界。
- 原 MLM 上下文兼容专项、十字军/贞德、缓存/退出及预览保护回归通过，Release 0 警告 0 错误。
- 未控制游戏实机验证弹窗/按钮画面。若本次已加载皮肤，整理后必须通过新进程应用正确顺序。

内测 1.0.33.7，公开版保持 1.0.33；同步四处安装包，提交推送 Git，不上传工坊。
