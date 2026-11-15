# 上游整合 1.0.32.1（完成，待 Claude 审核/实测）

GPT，2026-11-15；基于 `f60ce33`，工作区分支 `merge/upstream-1.0.32.0`。分支由 Claude 建立；将上游 `34ace48..52fe010` 的五个功能文件净补丁应用到本地基线副本，再落回指定分支，保留本地模块、战损、骨手、缩小与 .113 贴图检查。四处内测版本统一为 **1.0.32.1**，三段公开版本仍为上游的 **1.0.31**；未 commit、未部署。

- **保存后看门狗**：从清缓存前移到 `NGame.Quit` 的 Postfix；只有 `__runOriginal && Coordinator.AllowNativeQuit` 才启动。首次被 Prefix 拦截、启动未放行、缓存清理与原版 Quit 的保存调用期间均不计时；原版正常返回后才开始 12 秒引擎收尾监视。日志不再断言“存档已写完”。
- **复用与所有权分离**：保留完整 `CaptureProviderPatches`，呈现/Ready 元数据及战损补跑不再为空；统一卸载入口跳过复用程序集，切回也不重复安装仍在位的回调，泄漏检查仅约束本 Mod 有卸载权的回调。候选须核对 Location 或完整 identity，匹配失败重抛。`InvokedProviderInitializers` 逻辑未改：切换器自己执行过的初始化器会跳过；游戏先执行、切换器未记录的初始化器，在首次接管时仍可能再执行，复用标记本身不保证去重。
- **其余接入**：保留单请求/无子线程、Common 完成或 60 秒兜底、退出取消待派发、图标存在性检查、manifest 空判断；准确注明 Paused 不等待在途请求。新增 Common 常驻标签及清单（45 处）。未改 Features/Safety。
- **验证**：Release **0 错误、9 个既有警告**；LogicTests 全绿。新增 `RuntimeTests --test-provider-ownership` 直接测生产实现：候选匹配、复用回调可见且不被卸载、切回不重复安装、普通回调可卸载、真实 Quit 补丁绑定和计时门槛，全部通过。判断保留在 Loader 同文件，未让无引擎 LogicTests 引入整个 Loader。`git diff --check` 通过。曾误跑默认 Runtime 全套，其既有卡牌“直接调用 IL”断言失败（`AppearanceResourceBoundaryTests:156`）；对应补丁早已使用 `Safe.Run`，本轮未改该处，未扩修此无关项，不将全套 Runtime 记为通过。

产物/日志：`C:\Users\yuinn\AppData\Local\Temp\sts2-gpt-1.0.32.1-8992ccccf73c4791afb09216ae2e24ff`（`build.log`、`logic.log`、`runtime-ownership.log`）；DLL SHA256 `06C120C74BA0EDC90467C7D51B38D713C5C95164A450B5AB84DC222658D30F9D`。

给云的实测（Claude 审核接入后，最多六步）：
1. 保留皮肤选择退出重开，核对选人/开局正常、Common 完成放行日志；未收到完成信号时应有 60 秒兜底。
2. 预加载仍有在途请求时连续切皮肤，确认不卡；打开退出确认后取消，继续换肤/游玩正常。
3. 局内首次切入战损包→受伤跨档→切走→切回，确认换档恢复且无重复 Timer/轮询。
4. 原版切魅魔，检查选人、小模型和开局；退出重进恢复选择，保留 .113 缺页回原版对照。
5. 骨手热换肤后过回合、存读档，确认位置/用户偏移及缩小效果正常。
6. 有正常缓存积累时修改设置、推进进度后退出；核对缓存清理→原版退出的日志顺序，重开检查进度/设置。12 秒监视只从原版 Quit 返回后算起。

**请维护者自测**：云没有十字军，且云环境从未触发程序集复用分支。请维护者检查十字军换皮、切走再切回后动作是否正常，以及日志是否出现“复用游戏已加载的…程序集”；云侧通过不能替代这条路径的实测。

## #94 基线对照补充

已从 `f60ce33` 的 git archive 在临时目录完整构建 RuntimeTests，用捕获异常的 .NET 宿主分别运行基线和当前版本默认全套。两者均以退出码 1 停在同一 `InvalidOperationException`：`AppearanceResourceBoundaryTests.CheckCardReentryIdentity` 的“树重入必须使用相同卡牌所有权和最终资源流程”断言（第 156 行）。这是原有直调 IL 断言不识别 `Safe.Run` 包裹的测试问题，不是缺 Godot 引擎导致，也不是本次合并引入；不在本单修复旧卡牌断言。

原系统窗口来自我首次误跑默认全套，参数未按预期传入。后续对照均设置 `DOTNET_DbgEnableMiniDump=0`，并由宿主 catch 托管异常、记日志、返回 1，未让异常逃出进程。后续显式运行 `dotnet <RuntimeTests.dll> --test-provider-ownership`；需要全套时通过捕获宿主运行。基线日志为产物目录的 `baseline-f60ce33/runtime-caught.log`，当前为 `runtime-current-caught.log`，宿主为 `caught-runtime-host/output/Host.dll`。
