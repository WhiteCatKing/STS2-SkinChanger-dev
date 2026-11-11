# 战斗背景切换（按地区）

- 状态：**未解决**（已实现，待实机验证）
- 记录日期：2026-11-04
- 需求来源：作者本人；卡洛蒂（CZN 怪物美化包反馈）

## 需求

- 暂停菜单「外观」下方新增「背景」入口：修改当前所在地区的全部战斗背景。
- 「其它图鉴」在「先古 / 商人 / 生物 / 事件」之后新增「背景」分类：按地区（密林 / 暗港 / 巢穴 / 荣耀）分别选择。
- 只处理战斗背景；火堆、商店、事件、先古等场景背景不动。
- 切换立即生效，包括当前正在进行的战斗。

## 游戏侧事实

- 战斗背景由 `EncounterModel.CreateBackground(parentAct, rng)` 创建：普通战斗取
  `ActModel.GenerateBackgroundAssets(rng)`（`res://scenes/backgrounds/<act>/` 下的
  `<act>_background.tscn` 外壳 + `layers/` 随机层），自定义背景的遭遇取遭遇自己的同名目录；
  最后由 `NCombatBackground.Create(BackgroundAssets)` 实例化并挂进 `NCombatRoom.BgContainer`。
- 地区 id 即 `ActModel.Id.Entry` 的小写：`overgrowth`、`underdocks`、`hive`、`glory`。
- CZN 不对规范路径做替换，而是在自己的 DLL 里声明“地区/遭遇 → 战场 profile”，并用
  `LayeredBackgroundRuntime.TryCreate(profile, rng, out NCombatBackground)` 生成分层背景。

## 实现（1.0.26.8 测试补丁）

- 选项顺序固定为：`跟随分类` → `◈游戏默认` → `◈模组默认` → 所有检测到的背景 → `创意工坊`。
  - `跟随分类` / `模组默认`：不接管，完全交给各 mod 自己的地区/遭遇/Boss 分配（两者效果相同，
    模组默认用与游戏默认同款金色 + ◈ 显示）。
  - `游戏默认`：游戏官方背景；普通战斗取地区目录，自定义遭遇取遭遇目录。若当前地区在新地图/
    改名的地图里找不到官方背景，则等同“跟随分类”，不接管也不报错。
  - 其余条目：全部可用背景，**所有地区共用一份全量列表**，可以在密林用暗港、巢穴甚至官方 Boss
    场景的背景。选中后强制该层所有战斗使用它。
- `BackgroundSkinService`：
  - 游戏背景：运行时枚举 `res://scenes/backgrounds/*` 中带 `<名称>_background.tscn` 的目录，
    包含官方的 4 个地区与所有自定义遭遇（Boss/事件）背景；新地图/新层级只要符合该结构也会自动出现。
  - Mod 背景：扫描所有已识别外观包的 PCK，收集 `**/battlefields/<id>/<id>_background.tscn`
    以及规范 `res://scenes/backgrounds/<名称>/...` 外壳，连同其 `layers/` 一起记录；
    当前为 CZN 的 22 个战场（不区分小怪/标准/Boss，全部可选）。
  - 通用分层构建：实例化外壳 → 按 `_bg_XX_*` 分组随机选层填入 `BattlefieldCanvas/Layer_XX`
    （或规范外壳的 `Layer_XX`）→ `_fg_*` 填入 `Foreground` → 有 `configure_for_sts2` 时调用；
    来源包未挂载时按低优先级临时挂载其 PCK 再构建。
- `EncounterModel.CreateBackground` 后置补丁（Priority.Last）最终决定背景：
  - 上述两类“默认”不接管；`游戏默认` 缺资源时不接管；显式背景按选择构建并替换旧节点；
  - 被替换节点尚未进场景树，直接释放；当前战斗会立即重建背景。
- 暂停菜单「背景」按钮 + `BackgroundAppearanceScreen` 完全套用「外观」面板：顶部提示
  “选择当层地图的战斗背景”，标题为当前地区名，分区“背景”，下接上述选项；无其它控件；
  打开时关闭全屏遮罩，保持画面明亮。
- 「其它图鉴」新增「背景」分类，条目为各地区，右侧下拉为同一套选项；选中地区后把当前背景
  直接铺满预览区（与「事件」一样是整屏替换），切换选项即时重画。
  - 「跟随分类 / 模组默认 / 游戏默认」的预览取该地区的默认图：官方地区用官方地区背景，
    Mod 新地图用该 Mod 声明的 standard combat 战场（打小怪同款）；找不到就不画，不报错。
- Mod 新增地区（例如“雪原”）自动兼容：地区列表来自 `ModelDb.Acts`，当前对局地区若不在列表里
  会临时补一条；局内标题、其它图鉴条目、默认背景都按该 Mod 的声明解析，不写死地区 id。
## 已知边界

- 背景来源必须是“按地区/遭遇声明战场 profile”的完整外观包（当前为 CZN）；纯规范路径背景包
  以后再按同样方式接入。
- 若所选来源的包未被当前配置激活（例如没有任何怪物选用它），其战场资源可能未挂载，
  此时会记录一次警告并保留原背景。
- 预览为实时场景（分层背景/Spine），不生成静态缩略图；下拉名称取地区名或“Mod 名 · 背景名”。

## 验证

- 离线：LogicTests、RuntimeTests 通过（含 `BackgroundSkinPolicy`）。
- 实机：待验证。


