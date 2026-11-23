# 阶段 3b · 卡图跨切换复用

工作树 `上游修复_优化阶段3b/STS2-SkinChanger_修改件`，分支 `perf/phase3b-card-cache-20261123`，基线为白猫 1.0.38.13 的 `40d90f6`。本轮交付工作树改动，待复核和云实机验收。

## 取图与失效

- 图鉴预载、奖励预载走 `PreloadCardPortraits`，模型的 `CardModel.Portrait` 后置补丁走 `ReplaceCardPortrait`。两者汇入 `GetOrLoadCardPortrait`，覆盖使用模型卡图的图鉴、手牌、大图和牌组。直接解码的 PNG 无需单独挂载根资源。第三方完全绕过模型 getter 自行绘制的画面仍由第三方负责。
- 图集键包含实际源 PCK 的绝对路径、文件句柄长度和 UTC 修改时间，以及私有根、页和载荷路径、每份改写后导入描述的 SHA-256。各切片保留自己的区域。私有命名空间沿用当前游戏会话中该选项的挂载记录。
- 仅将 A01 实际挂载的文件加入依赖图。递归检查文本或二进制纹理、导入重定向和页载荷，可引用先前批次的私有页。依赖闭合、全部来自同一物理快照且属于当前命名空间时，允许 Godot 复用外部页。真实皮肤包中的混源、未知或不能闭合的图集走深度加载，切换分组清理这类缓存。游戏默认继续回原版。沿用 A01，无新增全局写入点。
- 散图键包含实际文件身份、会话、提供者、分组、资源路径和解码方式。内联 PNG 记录 cardartpack JSON 的身份，松散图片记录图片本身，PCK 文件记录 PCK 身份。图像由内存创建。
- 快表和私有缓存命中都要核对物理文件身份。包更新或删除后，旧键退出缓存，请求保留游戏回退。重新建立目录或重启游戏后读取新快照。若人为同时保留文件长度和修改时间，元数据无法识别内容替换。

## 预算与释放

已验证的皮肤图使用 **1 GiB** 独立字节 LRU。沿用宽 × 高 × 8 的保守估算，共享底图实例只计一次。本机 CuteFool PCK 的 74 个 CTEX 按头部尺寸合计 541,192,072 B，愚者美化 JSON 的 91 个 PNG 合计 351,932,800 B。两者约 852 MiB，超过原来共享的 512 MiB。1 GiB 为这两个包留出余量。原版及保守路径保留原来的 512 MiB；两个缓存合计最多持有 1.5 GiB 计费纹理，各限 1,024 项，单张估算达到 512 MiB 仍会拒绝。

64 项快表只保留纹理弱引用。预算淘汰、会话清理或身份失效时，缓存放掉自己持有的引用，外部视图和 AI_silent 可以继续使用纹理。最后一个外部持有者放手后，既有退役队列在游戏线程每帧最多显式 Dispose 32 个包装对象。视图、GC 和释放队列均会影响全局纹理内存。

`[卡图性能]` 新增 `reuse_hits`、`reuse_evictions`（本次卡图会话累计，淘汰含身份失效）、`reuse_bytes`、`reuse_budget`、`other_card_bytes`（采样时估算）。阶段 2 的 `atlas_new`、`native_load`、`raster_decode` 仍按日志窗口计数。

## 离线结果

用生产 LRU 和阶段 2 计数器比较相同的第二轮请求。按真实包的常见尺寸模拟 49 张 1109×832 图集页、79 张 800×608 PNG，另加一张 1024×1024 默认图。替身记录进入加载回调的次数，未分配 GPU 显存。

| 第二轮 | 新建页 atlas_new | PNG raster_decode | 驻留估算 |
| --- | ---: | ---: | ---: |
| 旧 512 MiB 共享预算对照 | 49 | 79 | 529,857,024 B，含默认图 |
| 新 1 GiB 私有预算 | 0 | 0 | 669,098,496 B 皮肤图；默认图另计 8,388,608 B |

通过生产取图入口另测 A、B、原版。首次返回三张独立纹理，计数为 1 页、1 次 PNG；再切三轮计数为 0、0，6 次私有命中，驻留保持 16,384 B。另一切片共用同一页且保留独立根。用例另查修改时间、大小变更、混源、开放依赖、LRU 淘汰、外部持有，以及放手后的主线程 Dispose。阶段 2 的游戏实测是重选 A 新建 49 页，另一次重选 B 直解 25 张。表内 79 是离线工作集，冷加载时间和全局显存仍待实机。

## 变更文件

- `STS2SkinChanger/Catalog/` 中的 `CardPrivateGraph.cs`、`CardBinaryResource.cs`、`SkinCatalog.cs` 负责挂载后的依赖和实际来源，`CardArtPack.cs`、`CardArtPackScanner.cs`、`LooseCardPortraitPackScanner.cs` 保留散图来源。
- `STS2SkinChanger/Pck/PckArchive.cs` 记录打开文件的快照。`STS2SkinChanger/Core/` 下的 `SkinService.cs`、`CardPortraitSessionCache.cs` 接入选图、缓存、失效、统计及释放。
- `tests/STS2SkinChanger.RuntimeTests/` 中的 `CardCacheReuseTests.cs`、`CardPortraitSessionTests.cs`、`FoolCardPackIsolationTests.cs`、`Program.cs` 提供替身和真实包测试。`tests/required-fixes.json` 登记独立门禁 `card-cache-reuse`，`docs/agents/maintenance-rules.md` 登记维护约束。

## 门禁与真实包

- 正式版 v0.111.1 与测试版游戏程序集均运行 `tools/run-gates.ps1`。共 36 项，32 PASS、4 PASS+EXT，FAIL、NOT RUN、GAP 均为 0。两个无参 RuntimeTests 日志的末行均为 `Workshop policy tests passed.`，`card-cache-reuse` 独立用例两端通过。
- 两端另跑 `tools/check-protected-characters.ps1`，使用各自的游戏 PCK 和本机维护集里的十字军、贞德 SFW、贞德 HPwear，均退出 0。另用测试版运行真实愚者双包用例，检查 CuteFool 卡图的私有依赖来源、愚者美化散图的文件身份、默认和非愚者隔离，退出 0。
- 门禁汇总在 `%TEMP%/sts2-3b-gates.log`，两端缓存计数在 `%TEMP%/sts2-gates-gpt-3b/{stable,beta}/separate_--test-card-cache-reuse.log`。受保护角色日志在 `%TEMP%/sts2-3b-protected-{stable,beta}.log`，真实卡包日志在 `%TEMP%/sts2-3b-fool-final.log`。本轮未部署，未提交。

## 云的实机验收

1. 沿用阶段 2 的测试版、Mod 和窗口尺寸，切换器排在 A、B 前。打开图鉴，固定愚者卡池，关闭“有皮肤卡牌”，默认预设关闭该池所有皮肤优先项。
2. 从顶部选择 A（CuteFool-Cards），向下翻三到四屏，每屏停一秒，记住一张卡的图案并回顶部。换 B（愚者卡面美化）重复，再换游戏默认并核对原版。回顶部切 A，核对相同卡与首轮一致。
3. 同一游戏进程连续跑三轮 A → B → 默认 → A。保留从 `library_open` 到 `library_close` 的全部性能行。相同页面在未淘汰时，重选 A 的 `atlas_new` 和重选 B 的 `raster_decode` 应接近零。对照 `reuse_hits` 增量、`reuse_evictions`、`reuse_bytes` 和全局 `texture_bytes`。每轮结束稍停，看第二、三轮显存是否趋稳，是否仍各涨约 200 MB。同时对比首次单张加载峰值。
4. 在手牌、牌组、大图中核对同一卡的图案和裁切，切回默认后再看一次。装有 AI_silent 时，保留外部视图跨切换核对显示。遇到异常时记录提供者、卡名、轮次及对应日志窗口。

## 复核后修改

- 卡图快表与私有图集共用两秒的文件状态检查。按物理来源缓存结果，连续命中不会延长有效期。打开图鉴、切换卡图、扫描卡图目录或重新登记工坊资源会让旧检查过期，下次用到该来源时立即重查。确认包已变更时，旧底图退出复用缓存，当前取图回退原版。
- 失效前缀与对应字典使用同一套比较规则。私有缓存区分大小写，原版及保守缓存忽略大小写。正常条目和失败条目使用同一规则。
- `card-cache-reuse` 通过生产取图入口验证，连续 24 次读取同一卡图只查询一次文件。另测切换后重查、开图鉴后识别已变更的包、两秒到期、失败结果的短期缓存，以及两种大小写规则。
- 复跑双引用门禁，36 项中 32 PASS、4 PASS+EXT，零失败。两端受保护角色检查和真实愚者双包检查通过。日志在 %TEMP%/sts2-gates-gpt-3b2.log、%TEMP%/sts2-3b2-protected-{stable,beta}.log 和 %TEMP%/sts2-3b2-fool.log。游戏内耗时和显存仍按上面的三轮步骤验收。

## 实机结果（2026-11-23 19:15 云实测，测试版 v0.113.1，DLL F46AF14C3D31，约 210 个 Mod）

愚者卡池，A＝CuteFool-Cards、B＝愚者卡面美化，A → B → 游戏默认 → A 连做三轮，正常关闭图鉴。云：没有串图，默认回到原版，后续翻页明显流畅。

| 场景 | 阶段 2 基线（BB350BDD6107） | 3b |
| --- | --- | --- |
| 第一次选 A + 翻页 `atlas_new` | 49 | 约 58（首次冷加载，正常） |
| **再次选 A** + 翻页 `atlas_new` | **49（与第一次相同）** | **0**（三轮都是 0） |
| 第一次选 B `raster_decode` | 79 | 约 86（首次） |
| **再次选 B** `raster_decode` | 25 | **0** |
| 纹理内存走势 | 4264 → 5354 MB，每轮再选 A 约 +200 MB | 第一轮后稳定在 5.10–5.18 GB，第二、三轮不再上涨 |
| 冷加载最大单张 | 25.7 ms | 24.8 ms（不劣化） |
| 复用缓存 | — | `reuse_bytes` 停在约 783 MiB（预算 1 GiB），`reuse_evictions=0`，`reuse_hits` 累计约 5000 |

同次：全局写入拦截 0、失败 0；无崩溃。
