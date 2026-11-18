# 缓存拒绝写入的界面提示（#130）

工作目录 `D:\mod\工作2\上游修复_1.0.35`，实际分支 `fix/1.0.36-stability`，HEAD `aeefc93`。GPT，2026-11-18。未 commit、未改版本号、未部署或启动游戏。

统一运行时 PCK 写入入口遇到 CacheWriteRejectedException 时，保留原异常类型和拒绝原因，给异常附上本地化展示文字；现有 SkinService.LastError 和切换回滚继续使用它。日志仍记录原始诊断详情（包括磁盘剩余字节对应信息），不是只记录面向玩家的简化文案。

界面表现：

- 外观面板打开时，下一主线程帧通过既有 LiveScreens/SetStatus 显示警告；相同原因再次拒绝仍刷新状态栏。
- 选人/暂停等共用下拉切换失败以及选人呈现刷新回滚时，显示同一个 12 秒提示条，重复手动操作会重新显示/延长提示，避免用户再次操作却没有反馈。
- 不在外观界面时，自动写包拒绝也通过顶部提示条通知。同一原因每局仅自动展示一次；目录重载不重置，NRun 实例变化才重置。纯数据队列可由后台线程写入，Godot 操作只发生在主线程 ProcessFrame。通过原生信号接入，避开运行时 Mod Node 虚方法不回调的问题；Entry 安装，并在主菜单 Ready 补装。
- 提示条复用同一节点，使用游戏字体，MouseFilter.Ignore、FocusMode.None；不弹模态、不暂停游戏、不抢战斗输入，12 秒后隐藏。

原因分预算达到 8 GiB、磁盘空间不足、挂载文件保护三种，使用 ModText 与 ModLocalization 的 partial 翻译表，全部 15 种发布语言均有各自文案。中文分别说明保留当前外观，并建议重启释放缓存或清理磁盘后重试。

主要文件：Core/CacheWriteBudget.cs、CacheWriteNoticeState.cs（新）、SkinChangerPaths.cs、ModLocalization.cs、ModLocalization.CacheWrite.cs（新）、Ui/CacheWriteNotice.cs（新）、CharacterAppearanceScreen.cs、ContextualSkinControls.cs、Entry.cs；RuntimeTests/CacheWriteNoticeTests.cs（新）和 Program.cs。预算计算、挂载文件寿命策略没有新增改动。

验证：RuntimeTests 项目（连主 Mod）构建 0 警告 0 错误；--test-cache-write-notice exit 0；完整无参数 RuntimeTests exit 0；LogicTests 构建 0/0、全套 exit 0。新测试覆盖同原因去重、不同原因分别提示、同局重载保留/新局重置、后台线程排队、队列消费不循环、15 语言真实覆盖和各原因文案区分，以及真实 WritePckOutput 低空间拒绝附带本地化消息而不执行 writer。运行前均 SetErrorMode(3)。本轮 git diff/diff --check 已核对。

临时输出 `%TEMP%\sts2-cache-notice-1.0.36-gpt`；日志 `%TEMP%\sts2-cache-notice-build.log`、`sts2-cache-notice-target.log`、`sts2-cache-notice-full.log`、`sts2-cache-notice-logic-build.log`、`sts2-cache-notice-logic.log`。画面尺寸、布局和战斗中提示条的实际观感未启动游戏验证，请后续集成时确认。
