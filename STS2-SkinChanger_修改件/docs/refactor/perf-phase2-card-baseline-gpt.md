# 优化阶段 2 · 卡图性能基线

工作树 `perf/phase2-card-baseline-20261122`，基于 `9bec0e9`。本阶段给现有卡图流程加计数，实际游戏数据由云按下文操作后采集。

## 计数点与口径

| 计数点 | 记录 |
| --- | --- |
| `SkinService.GetOrLoadCardPortrait` | 进入切换器卡图请求路径、有效的 `CardPortraitCache` 命中、进入隔离加载，以及失败回退（含已记录失败的请求）。原生路径上由游戏直接处理的请求无需经过此入口。 |
| `LoadIsolatedCardPortrait` 内实际执行的 `ResourceLoader.Load` | 根资源加载调用数。只有会话缓存要求实际执行加载时才计数，分别记同一游戏进程内该资源路径的首次和重访耗时。 |
| `LoadRasterImageFromBuffer` 的 PNG/JPEG/WebP 分支 | 实际调用解码器的次数与耗时。格式签名不匹配时没有调用解码器。原始图的计时单位是资源读取路径，后续上传不计入这项。 |
| `CardPortraitSessionCache.GetOrLoad` | 复用原来预算检查中的底图查找和实例 ID。每个原生底图对象只计一次。图集底图路径有效时，按实际资源路径计新增实例。 |
| `CardPortraitPerformanceView` | 图鉴首批网格加载完成、跨一个可见窗口高度、卡图选择保存并刷新、关闭图鉴时输出。每条日志同时采样 Godot 的纹理内存监视器和 `GC.GetTotalMemory(false)`。 |

`atlas_by_path`／`atlas_new` 统计真实新增的图集底图实例，重复引用同一实例只算一次。Godot 的 C# API 不提供原生解码器的调用次数，因此用新增底图实例作为图集重载指标。`native_load` 是根资源加载调用，包括只有切片根资源变化、底图仍复用的情况。`raster_decode` 是直接解码调用。三项可并排对照。底图路径拿不到或监测表达到上限时记录 `backing_untracked`。上游加载器在忽略缓存时仍设置了资源路径缓存，游戏所带 MegaDot 实机上的底图路径覆盖率以日志为准。[Godot 4.5.1 ResourceLoader 源码](https://github.com/godotengine/godot/blob/4.5.1-stable/core/io/resource_loader.cpp)

每次打开图鉴时开始新统计窗口。首条日志只覆盖本次图鉴打开后的请求，后续每条日志覆盖上一次采样后的增量。冷／热判定、底图实例去重和内存采样峰值跨图鉴重开保留。纹理峰值是历次快照的观察峰值，受采样间隔影响。页面是连续滚动中的一个可视区高度，滚动位置至多每 100ms 读一次。资源路径与实例记录有上限，达到后单列未归因计数。`atlas_by_path` 最多列八条，按新增实例数排序。

## 日志样例

以下数字为格式示例，实机基线待采集。

```text
[卡图性能] scene=library_page detail="2" requests=96 hits=71 isolated=25 fallback=0 native_load=18 raster_decode=3 native_texture_new=16 atlas_new=12 atlas_repeat_pages=2 atlas_max_loads=9 atlas_path="res://alias/card_atlas_0.png" atlas_paths=2 atlas_by_path="res://alias/card_atlas_0.png=9|res://alias/card_atlas_1.png=3" load_path_max_calls=2 load_path="res://alias/strike.tres" load_paths_untracked=0 backing_untracked=0 cold_n=17 cold_avg_ms=4.10 cold_max_ms=28.50 hot_n=4 hot_avg_ms=2.40 hot_max_ms=7.90 timing_untracked=0 texture_bytes=753000000 texture_peak_bytes=796000000 managed_bytes=92000000 managed_peak_bytes=108000000
```

`scene=library_open` 的 `detail` 另含从打开到首批网格完成的 `elapsed_ms`。`scene=card_selection` 的 `detail` 是卡池与包或预设名。`scene=library_close` 保留最后一段的统计，即使此段没有加载。

## 行为核对

核对调用前后的代码，缓存命中、隔离包生成、`Ignore` 与 `IgnoreDeep` 的选择、预算、LRU、释放、回退与选择持久化均沿用既有逻辑。没有新增资源加载。图鉴 hook 只观察原任务完成和滚动位置，不替换原任务。命中时只做计数，时间戳和资源归因留在实际加载路径。只增加日志，没有新增界面或配置。

`CardPortraitPerformanceTests` 覆盖窗口增量、原生图集实例去重、分路径计数、首次／重访计时、采样峰值、上限、连续滚动分页，以及双引用下四个实际 Harmony 目标。它已进入完整无参 `RuntimeTests`，也可用 `--test-card-portrait-performance` 独立运行。门禁清单保持 33 项。

## 离线验证结果

在临时副本直接执行 `tools/run-gates.ps1`。正式版 v0.111.1（Release）及测试版 v0.113.1（ReleaseBeta）均 0 警告、0 错误。LogicTests 和完整无参 RuntimeTests 通过，末行均为 `Workshop policy tests passed.`。双端门禁结论为 `PASSED`，共 33 项，30 PASS、3 PASS+EXT。另跑两端 `check-protected-characters.ps1` 通过。日志在 `%TEMP%/sts2-gpt-292/gates4.log` 与 `protected-stable.log`、`protected-beta.log`。

未提交、未推送、未部署。首开、翻页事件归因和 MegaDot 实际可识别的图集页数仍待实机确认。

## 给云的实机操作步骤

1. 使用测试版，确认皮肤切换器排在两包之前。复用本机 `_test_新增_愚者卡图` 里已确认同属愚者卡池的 **A＝CuteFool-Cards，B＝愚者卡面美化**。固定窗口大小和愚者筛选，关闭「有皮肤卡牌」，避免不同包改变卡牌列表。若需建立 A、B 和「游戏默认」三个优先级预设，先完成设置再启动本轮新游戏进程。默认预设请关闭这一池的所有皮肤优先项。
2. 从主菜单打开卡牌图鉴，切到愚者卡池，滚到顶部并停一秒。运行日志应出现 `scene=library_open`。
3. 选 A。向下滚约一个屏幕的卡牌高度并停一秒，共做三次。每跨一屏，预期有一条 `library_page`。准备切换时回到列表顶部并停一秒。
4. 选 B，重复向下翻三屏，每屏停一秒。再回顶部。
5. 选「游戏默认」，重复向下翻三屏，每屏停一秒。核对画面回到原版。原版加载由游戏直接处理时，切换器的请求和图集计数为 0 属于正常。再回顶部。
6. 切回 A，同样翻三屏，确认图案与第一次选 A 一致。命中隔离卡图的会话缓存时，请求记在 `isolated`，`native_load` 和 `raster_decode` 无增量。停一秒后关闭图鉴，确认出现 `library_close`。交给 Claude 本次 `godot.log` 中从 `library_open` 到 `library_close` 的全部 `[卡图性能]` 行。向上回顶部产生的行保留，比较向下翻页时可按页号区分。

## 复核后修改（#300）

- 图鉴打开和首批网格观察入口统一经 `UiInjectionGuard.Run` 隔离异常。滚动仍在节流命中后进入护栏。
- 打开图鉴时在 `SkinService.Sync` 锁内清理窗口计数和路径计数。`CardPortraitPerformanceTests` 验证清理后的请求计数和路径统计，并验证会话路径、底图去重和峰值仍可跨窗口复用。
- 正式版与测试版重新通过 `run-gates.ps1 -Out $env:TEMP\sts2-gates-gpt`。33 项中 30 PASS、3 PASS+EXT，零失败。两端的卡图性能用例和完整 RuntimeTests 均通过。日志在 `%TEMP%\sts2-gates-gpt-300.log`。第 5、6 步的画面和日志仍待实机确认。

## 实机基线（2026-11-22 云实测，测试版 v0.113.1，DLL BB350BDD6107，约 200 个 Mod）

愚者卡池，按步骤：CuteFool-Cards → 愚者卡面美化 → 游戏默认 → CuteFool-Cards，每次翻 4–5 屏再回顶部（开头愚者卡面美化是进图鉴时的已选项）。

| 场景 | 新建图集底图 `atlas_new` | 原生加载 / 直解 PNG | 冷加载 平均 / 最大 | 纹理内存（全局） |
|---|---|---|---|---|
| 打开图鉴（`elapsed_ms=976`） | 25 | 25 / 0 | 4.6 / 22.5 ms | 4264 MB |
| 第一次选 CuteFool + 翻页 | 18+5+7+10+6+3 = **49** | 84 / 0 | 7.6–11.9 / 25.7 ms | 4619 → 4893 MB |
| 愚者卡面美化（散图 PNG） | 0 | 1 / 79 | 4.5–6.5 / 8.7 ms | → 5098 MB |
| 游戏默认（会话里的原版页） | 1 | 约 70 / 0 | 1.2–1.9 / 6.5 ms | → 5144 MB |
| **第二次**选 CuteFool + 翻页 | 18+5+7+10+6+3 = **49（与第一次完全相同）** | 68 / 0 | 计为热路径 | 5152 → **5354 MB** |
| 另：摄政王全卡美化 / CNRRegentCards | 17 / 9 | 26 / 9 | 15.7 / **85.9 ms** | 5417 MB |

读法：
- 同一窗口内 `atlas_repeat_pages=0`、`atlas_max_loads=1`：**一屏之内没有重复解码**。
- **浪费在跨切换**：切走再切回同一个包，它的 49 张图集页全部重新建了一份底图；愚者卡面美化再次选中也重新直解 25 张 PNG。纹理内存一路上涨（第二轮 CuteFool 又涨约 200 MB），旧底图没有被复用。
- 冷加载单张 5–12 ms，最大 26 ms；摄政王全卡美化出现 86 ms 的单张卡顿。
- 阶段 3b 的验收目标据此定为：切回已加载过的包时 `atlas_new` ≈ 0、直解 PNG ≈ 0，纹理内存不再随来回切换上涨；单张冷加载最大值不劣化。
- 本次日志没有 `scene=library_close` 行：最后一条 `[卡图性能]` 之后紧接着就是 `NGame.Quit`，是在图鉴里直接关了游戏，没有走关闭图鉴，属正常，不是钩子问题。
- 同次 `[完整性自检]`：检查 70 项，嫌疑 3（OrchisNecrobinderSkinMod 带了亡灵契约师选人立绘 `char_select_necrobinder*.png` 的同名导入文件），确认 0，耗时 353 ms，主线程 4 ms（2 帧）。`[界面注入]` 失败 0 条。
