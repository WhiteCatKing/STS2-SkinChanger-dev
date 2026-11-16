# 随 Mod 自动运行的崩溃诊断

用户提供 桌面/新建文件夹 (3)/Sts2CrashWatch.exe。核对说明与同仓库 tools/Sts2CrashWatch 源码，其主功能是异常退出/启动失败、窗口不响应且日志停滞、缺少关停记录、WER 与卡死转储及报告。原程序与用户目录均未改动；不把其启动游戏和常驻等待下一次游戏的行为移入 Mod。

实现：Entry 在路径初始化完成后调用 CrashDiagnostics.Start。托管未处理异常及未观测任务异常写入有上限的本次会话事件文件，不 SetObserved、不吞掉原异常。Windows 自动从当前 DLL 提取内嵌 UTF-8 BOM PowerShell 脚本，用结构化 ArgumentList、NoProfile/NonInteractive、CreateNoWindow/Hidden 启动，只监视当前 PID + 进程启动时间 + 进程名。依赖 Windows 自带 PowerShell，不需要安装额外 .NET 或手动打开监视器。脚本启动失败会在游戏日志记录，托管异常可留下下次启动补记。

每秒检查窗口响应和日志增长；同时连续达到 30 秒才记疑似卡死，抓带线程信息的小型转储（没有 FullMemory 标志），游戏仍在时由 Mod 记录提示。无窗口、仍有日志增长、短暂无响应不触发。恢复响应单独记录，不把可恢复停顿声称为确认崩溃。监视器不终止或重启游戏。

异常退出后记录有符号退出码与十六进制、监视期间峰值内存、会话与 Mod 版本、同 PID 的 Windows Application Error 事件、托管异常及可用转储路径。普通 ERROR、资源泄漏文字不单独判为崩溃；完成 Quit/Steam 关停且退出码 0 算正常。退出码非 0 或终止性异常不会因出现 Quit 字样被掩盖。原有 QuitCacheCleanup 12 秒兜底行为保留，仅新增 forced-exit 标记，退出码 0 的收尾超时也生成诊断；QuitTimingDiagnostics 仅在原生 Quit 实际运行后写完成标记。

只在进程结束后向游戏日志追加结构化诊断。始终持有原日志句柄并允许删除/重命名，因此立即重开游戏不会把上一局报告误写进新日志。若 Godot 运行时不允许写入，退出后通过 Windows GetFinalPathNameByHandle 和文件 ID 核验安全重新取得原文件写权限，支持已轮换文件。仍被锁时保留 pending-recovery，在下次启动用 Mod 自身日志补记一次。很早结束、尚未取得监视句柄时只写退出码未知/未能确认的证据。原生异常后的转储只能关联已存在的匹配 PID WER 文件，不改注册表以强制系统生成转储；没有转储会明确注明。

位置：用户数据目录/Gurio.SkinChanger/diagnostics/<session-id>/，包含 diagnostic.txt、godot-tail.log（最多约 1 MiB 日志尾部及摘要）、managed-events.log 和可用 hang.dmp。正常退出不生成崩溃摘要。只清理本目录下符合 GUID 且已完成的自有会话：保留最近 3 次正常、8 次异常记录；待补记和未完成记录不自动删除。没有上传、遥测或持久系统设置修改。

验证：--test-crash-diagnostics 使用自建隐藏 PowerShell 测试进程，真实验证正常退出、普通 ERROR 不误报、有 Quit 标记仍非零退出、既有看门狗零退出超时、早退出未知码、只读后升级原文件写权限、轮换后旧文件追加且新日志不变、仍被锁时下次补记、历史边界、实际 MiniDumpWriteDump 生成 MDMP 文件头。直接从嵌入脚本 AST 提取真实挂起判定函数覆盖无窗口/响应正常/日志增长/短暂停顿/双阈值。没有启动、控制或故意崩溃游戏。

同一正式版编译 DLL 在正式版/测试版通过 check-dual-branch.ps1（零转换/零未解析、预览、商店、保护边界、鸡煲），完整 check-protected-characters.ps1 通过。正式引用固定为 stable-api-1071 快照（MVID 97f10687-c306-4798-ab75-8b9f23f34dfb），当前安装目录已切回 beta；测试引用为 beta 快照 MVID 9d417b39-1d40-4fc7-b993-28acdc29e151。

限制：Mod 加载前无法监视；系统断电、同时结束观察器、系统策略阻止脚本、不可写存储等情形不能保证即时写入。后台观察器完整监视只在 Windows 启用，其他系统保留终止性托管异常的下次启动补记。诊断是证据而不是自动归责；不是所有原生崩溃都有可用转储。未做游戏内真实崩溃实验。
