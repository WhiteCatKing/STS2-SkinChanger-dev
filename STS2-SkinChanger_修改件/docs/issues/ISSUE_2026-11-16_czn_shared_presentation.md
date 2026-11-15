# 1.0.32.5：CZN 系列共享呈现兼容与切换安全（2026-11-16）

## 本机包核对

| 工坊 ID | 本机清单 ID | 定位 |
|---|---|---|
| 3840027648 | HildeFoolSkin | fool 角色皮肤 |
| 3826012249 | HeidemarieRegentSkin | regent 角色皮肤 |
| 3826012071 | TressaSilentSkin | silent 角色皮肤 |
| 3826011833 | ChizuruIroncladSkin | ironclad 角色皮肤 |
| 3789216478 | MeirinWatcherSkin | watcher 角色皮肤，依赖 Watcher |
| 3799519206 | CznTipheraCharacter | 新增玩法角色、卡牌、遗物；保留原加载器，不作为皮肤包隔离 |

共用 CznCore（3826011578，本机 1.1.4）。只读核对实际 DLL/PCK/清单，未修改第三方包。

## 已确认的兼容缺口与修改

1. 识别：旧扫描器只理解资源注册 setter/特定资源集构造器；新版 CharacterSkinProfile 通过 typeof(T) 和构造参数声明身体。新增 PE 元数据静态识别，目标及骨架必须唯一才映射；不执行静态初始化器。只有能明确接管身体资源的配置才取消旧的整包交回逻辑，无法解析的仍交回原加载器。
2. 背景：原 FrameworkSelectPresentationBridge 遍历全部程序集并读取静态字段，既找不到仅保存在闭包中的展示实例，又可能初始化未选中的包。改为在 CznCore 展示实例构造完成时登记，重建选角界面后复用作者自己的 ApplyScreenSelection（保留作者背景模式与控制菜单），不再全域读取静态字段。
3. 共享事件：CznCore 的事件订阅和共享补丁不属于皮肤 DLL，卸载皮肤自身 Harmony 补丁后仍然存在。对受管皮肤的 IsSelected/IsActive 按切换器当前可见选择判断，玩家事件使用既有联机选择作用域；切走后背景/火堆/商店等旧包事件不再按框架旧设置继续接管。未接管的玩法内容不受这一判断影响。
4. 小人预览：在新建的、尚未入树的预览模型上绑定所选配置的战斗骨架；只接受唯一 Spine 节点和 SpineSkeletonDataResource 类型，避免拿选角立绘代替战斗身体，也不在运行中的 Spine 上替换骨架。
5. 卡面：受管皮肤的 card_replacements.json 已由切换器读入卡牌目录；禁止 CznCore 的共享 BundledSkinCardArt 再通过进树/检视补丁强制套包内卡图。未受管的内容 Mod 卡面保持作者原行为。

## 切换闪退调查与安全改动

提供的 godot.log 最后停在 defectHpWear -> DeepseekWhaleDefect 切换后的本地化重载，未记录原生崩溃栈；系统应用事件和现有 CrashDumps 未找到对应游戏转储。因此不能把日志最后一个阶段当作已证实的闪退根因，也不能宣称所有闪退已经消除。

确认存在的并发风险：SkinPreloader 从可变的公共 res:// 路径发起后台加载，选角重建中的 Paused 只能停止新派发，不能撤回在途请求；换肤挂载时公共路径的骨架/图集/导入依赖可能已换归属。改为只接受不可变的 res://sts2_skin_runtime/ 私有路径（并拒绝路径逃逸），公共资源由实际使用入口加载。这会减少后台预热，首次进入某些场景可能增加同步加载时间。

本次同时移除全域静态字段扫描这一不必要的第三方初始化入口。上述措施消除已发现的危险路径，但本次崩溃的原生原因仍缺证据；若再次闪退，应继续根据新日志/崩溃转储定位，不能以 try/catch 宣称能捕获原生访问冲突。

## 验证

- Release：0 警告、0 错误。
- LogicTests：Skin Changer logic policy tests passed.
- --test-czn-compatibility <game.pck> <workshop>：五个真实皮肤 DLL 声明与资源目标、真实 PCK 探测及角色下拉目录均通过；Tiphera 新内容排除通过；真实 CznCore Harmony 接口安装通过；共享事件选择边界（原版/选中/切走）及私有路径预加载边界通过。
- --test-card-inspect-portrait：通过，保留上一版大卡修复；作者已确认 1.0.32.4 实机修复成功。
- --test-card-art-packs：契约测试通过，测试默认工坊目录未找到导致的实包维度检查仍跳过；本次 CZN 实包由上面的专门审计覆盖。
- 新专项测试初次加载真实 CznCore 时，游戏日志初始化触发未启动 Godot 的原生调用，测试进程退出；测试已隔离日志依赖后重跑通过（没有启动游戏）。
- 未启动游戏。背景模式、预览姿态、火堆/商店及快速切换的实际画面和本次闪退是否复现仍需真机观察，离线合同测试不能证明这些画面全部正确。

## 交付

版本 1.0.32.5，三个运行文件同步到本地正式包、本地测试包、工坊内容暂存和指定游戏测试目录，按文件核对 SHA-256；本地提交、附注标签 v1.0.32.5、推送 origin/main。未上传 Steam 工坊。保留开始前已有两份公告文档的删除状态。