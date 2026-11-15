# 假商人事件「背景」：战斗背景跟随事件图 + 下拉可用

- 状态：**未解决**（1.0.26.66 已实现，待实机验证）
- 记录日期：2026-11-07
- 需求来源：作者本人；上游交接见桌面 `假商人背景功能_交接说明_2026-11-07.md`

## 1.0.26.66：事件构图补丁并入主 mod（独立小功能）

作者确认「战斗里的那张图位置/大小更好」，要求**事件里的动画帐篷也按战斗的构图显示**，
并把这个调整**并入皮肤切换器**、标好位置方便以后删掉。

- 文件：**`STS2SkinChanger/Core/FakeMerchantEventFrame.cs`（唯一文件，全部实现）**
  - 补丁点只有 1 个：`NFakeMerchant._Ready` 的 `Postfix`（类 `FakeMerchantEventFramePatch`）。
  - 逻辑：等 Spine 骨架就绪 → 取 `MegaSkeleton.GetBounds()` → 按**事件画面自身矩形**做等比铺满
    （居中、超出裁剪）→ 整体**右移 `HorizontalShift`**（当前 **41 px**，画布 1920×1080 像素）。
  - `SceneContainer/BgContainer`（帐篷 + 火焰/星点/闪光）整体变换，装饰跟着走；
    `SceneContainer/SpineSprite2`（帐篷前景 cutter 层）用同一骨架空间映射，前后不错位。
  - 也作用于 SkinChanger 里的假商人**预览**（铺满预览框，不会爆屏）。
  - 无存档、无配置、无外部文件；事件外观包本身不受影响。
- **删除方式**：删掉上面那一个文件即可（`Entry` 按 `[HarmonyPatch]` 类逐个安装，文件没了补丁就没了），
  再删掉本节说明。事件立即恢复游戏原构图。
- 早期的独立补丁 mod（`STS2_Work\FakeMerchantEventFramePatch\`，带调参热键和 `horizontal_shift.txt`
  配置文件）**已整份删除**，其中的调参代码没有并入主 mod，避免留双份实现；游戏 `mods` 目录下的
  同名独立 mod 也已移除，不会重复生效。
- 实机测量数据：骨架范围 `(-1151.28,-358.47)+(4866,2880)`、骨架 scale 0.5、铺满缩放 0.789、
  居中原点 `(454.27,113.26)`；原画右侧白边约 40 px，作者实机最终右移 41 px 刚好出屏。

## 1.0.26.65 补充（作者反馈后）

作者确认下拉里的「假商人事件背景 / Fake Merchant Event」就是想要的那张图，要求：
**「跟随分类」「模组默认」在假商人战斗里也显示这张图**（仅限假商人战斗）。

- 1.0.26.64 走的是「不写规范重定向 → 让游戏用原场景」的路子，实机日志显示
  `假商人战斗背景来源=遭遇自带（事件图）`，但画面仍是原版帐篷：
  场景的 canoncial 请求会被 `ContextualSkinControls.ReplaceCachedScene`（`PreloadManager` 缓存补丁）
  换回我们运行包里的别名副本，所以只去掉 `.remap` 不够。
- 因此改为**直接复用下拉里那条已经验证可用的图片选项**：
  `BackgroundSkinService.TryUseFakeMerchantEventImageBackground` 在 `EncounterModel.CreateBackground`
  的补丁里为假商人遭遇（跟随分类 / 模组默认）创建同一张 `images/events/fake_merchant.png` 背景。
  不再依赖场景解析链路，和作者已确认的选项走完全相同的创建路径。
- `游戏默认` 仍优先用运行包里隔离的原版帐篷副本；拿不到时退回这张事件图（不会黑屏）。
- 图片改为 `LoadFakeMerchantEventImage`：先 `EnsureFakeMerchantEventArtMounted`，再用
  `ResourceLoader.Load(CacheMode.Replace)` 直接读**当前挂载**的版本，若为空才回退 `PreloadManager`。
  原因：`PreloadManager.Cache.GetTexture2D` 有我们的缓存补丁，会把事件贴图路由到所属皮肤分组，
  而 `fake_merchant_monster` 组并不拥有这张图，于是每次读取都刷一条
  `[ERROR] 最终接管贴图 res://images/events/fake_merchant.png 失败：KeyNotFoundException`，
  并且可能命中先前缓存的**原版**贴图。直接读并替换缓存可同时解决这两点。
- 结果按会话缓存（`_fakeMerchantEventImage`），预览与战斗复用同一张贴图。

## 目标

1. 假商人 **事件** 阶段背景由事件外观包（如 `NSFW化原版事件`，id `3765802910`）接管。
2. 假商人 **战斗** 阶段：跟随分类 / 模组默认 → 与事件同一张图；游戏默认 → 原版；其它 → 玩家所选。
3. 假商人的事件图进入「背景」下拉，可自由选择，且不会变黑屏。
4. 其它层与普通战斗行为不变；不改其它 mod 的文件。

## 根因（1.0.26.63 遗留）

- 假商人怪物皮肤是「运行时档案」外观包：我们为它建私有运行包（`res://sts2_skin_runtime/<会话>/<序号>/…`），
  游戏基线场景（含 `scenes/backgrounds/fake_merchant_event_encounter/**` 与
  `animations/backgrounds/fake_merchant_room/bottom/**`）都被复制成别名，**同时写出原路径 `.remap`/`.import` 桥**。
- 该运行包在事件图挂载**之后**才挂载，于是这些桥把帐篷的规范路径又抢回我们副本里的**原版**贴图：
  事件场景走游戏原场景路径所以显示新图，战斗背景场景被桥到别名副本所以显示原版帐篷。
- 也就是说：不是挂载顺序本身，而是「运行包替这些路径写了原路径重定向」。

## 1.0.26.64 改动

### 战斗背景不再用覆盖包里的副本（第 1 步）

- 新增 `Core/FakeMerchantEventArtPolicy.cs`：把「事件图重绘的路径」定义为
  - `res://scenes/backgrounds/fake_merchant_event_encounter/`（遭遇背景场景 + 各层）
  - `res://animations/backgrounds/fake_merchant_room/bottom/`（帐篷底层 Spine 图集）
  并规定这些路径**不写规范路径重定向**。
- `Catalog/SkinCatalog.cs`（`BuildAliasedResourceOverlay`）：直接场景 `.tscn.remap` 与依赖 `.import/.remap`
  两处规范桥都按上述规则跳过。私有别名仍照常生成，只是不再指向规范路径。
- 效果：游戏 `EncounterModel.CreateBackground` 在建战斗背景时加载的是**游戏原场景**，其贴图按规范路径解析，
  而规范路径上最上层正是事件图资源包 → 战斗与事件同一张图。
- 商人本体（`top/`、`hand/`）不在此列，继续走私有隔离，行为不变。

### 下拉里的「假商人事件背景」真正可用（第 2 步）

- `BackgroundSkinService.AddFakeMerchantEventFromPack` 重写：
  - 来源仍取 `FindFakeMerchantEventArtProvider()`（`Source` 事件包 id，不是 `game`）。
  - 图改用事件包的**满屏立绘** `res://images/events/fake_merchant.png`（3440×1616）。
    旧版本用的是帐篷底层 Spine 图集页（5964×3061，画面集中在左下角），当壁纸必然不满屏、偏左下，
    选中后看起来就是空/黑背景——这是上一次黑屏的真正原因，不是排版问题。
  - 注册前用 `TryProbeBackground` 真的创建一次预览；**失败就不注册**（宁可没有，也不给会黑屏的选项）。
  - 创建/预览前会 `SkinService.MountFakeMerchantEventArt()`，不依赖事件界面是否先跑过。
  - 原来那条「指向游戏自带帐篷场景、只是换了个名字」的重复条目已删除（游戏自带的帐篷条目仍在列表里）。
- 保留旧选项 id `bg::<事件包>::fake_merchant_event`，旧存档的该选择仍能解析。

### 游戏默认 → 原版帐篷

- `BackgroundSkinService.TryUseFakeMerchantVanillaBackground`：`游戏默认` 时用运行包已经隔离好的
  **基线副本身景**（`SkinService.TryGetIsolatedRuntimeResourcePath("fake_merchant_monster", 遭遇背景场景)`）
  重建战斗背景；拿不到副本则保留遭遇自带背景（= 事件图），不会黑屏。
- `SkinService` 为此记录每个分组最近一次的运行包（`LastPreparedRuntimeOverlays`）并提供只读查询。

### 诊断日志清理（第 3 步）

删除三处 `[诊断]`（`MountFakeMerchantEventArt`、`IsFakeMerchantEncounter`、`ApplyToRoom`）。
保留三条正式日志：

- `假商人战斗背景来源=遭遇自带（事件图）。`
- `假商人战斗背景来源=游戏默认（原版帐篷副本）。`
- `已把假商人事件背景图加入可切换列表：…`（注册成功时）

### 版本

`.csproj`、`Entry.InternalTestVersion`、`SkinChanger.json`、`workshop/workshop.json` 同步 1.0.26.64。

## 验证

- 离线：`LogicTests` 全通过（新增 `FakeMerchantEventArtPolicyTests`：事件图路径跳过规范桥、商人本体不跳过、
  其它背景不受影响、空值与反斜杠路径安全）。
- `RuntimeTests` 仍有**改动前就存在**的失败：`OtherCompendiumUiTests` 要求 `RefreshEventRegions` 调用
  `ModelDb.get_Acts`，而源码已改用 `ActCatalog.All`，与本改动无关，未处理。
- 实机（待作者）：见下。

## 实机验证步骤

1. 选中商人皮肤，背景选择保持「跟随分类」。
2. 进假商人事件：帐篷是新图（与 1.0.26.64 相同）。
3. 打完事件进战斗：背景应为**下拉里那张「假商人事件背景」**；
   日志应出现 `假商人战斗背景来源=事件图（假商人事件背景 / Fake Merchant Event）。`
4. 「模组默认」重复第 3 步，结果应相同。
5. 背景选择「游戏默认」再打假商人战斗：应为原版帐篷
   （日志 `假商人战斗背景来源=游戏默认（原版帐篷副本）`；拿不到副本则退回事件图）。
6. 背景下拉里仍只有**一条**「假商人事件背景 / Fake Merchant Event」，选中后普通战斗不黑屏。
7. 日志里不应再出现 `[诊断]`，也不应再出现
   `最终接管贴图 res://images/events/fake_merchant.png 失败`。

## 已知风险 / 下一步

- 假商人战斗背景现在是**静态立绘**（与下拉里那张一致），不是事件阶段的 Spine 动画帐篷。
  这是作者 2026-11-07 明确要求的取舍：两个默认项显示这张图。
- 立绘 3440×1616 与 16:9 不完全一致，`KeepAspectCovered` 会裁掉少量上下边。
- 事件包若只重绘帐篷 Spine、不含 `images/events/fake_merchant.png`，则不注册该选项，
  假商人战斗也会退回遭遇自带背景（游戏自带帐篷条目仍在列表里）。
- 「游戏默认」依赖商人皮肤运行包已隔离出的基线场景副本；未选商人皮肤时退回事件图。
