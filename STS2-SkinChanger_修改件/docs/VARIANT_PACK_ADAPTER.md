# 变体包适配指南（给以后遇到"同名/同源皮肤包"时用）

本文说明皮肤切换器是怎样**自动**适配"皮肤包的变体/形态由包自己驱动"这一类包的，
以及遇到新包时只需要检查什么。对应代码：

| 文件 | 职责 |
| --- | --- |
| `Catalog/ProviderSettingsVariantOptions.cs` | 从包的程序集元数据里识别"设置型外观变体"（外观通道 + 标签 + 默认值 + 写入口 + 重绘钩子），并把每个变体注册成独立下拉选项 |
| `Core/VariantHotkeyPoller.cs` | 接管包自己的外观快捷键（轮询物理按键），只在当前包自己的变体家族里前进一格 |
| `Core/ProviderMenuGateCompat.cs` | 放行/收紧包自己的"门控"：`MenuAllowed`/`SceneAllowed`/`PlayerAllowed` 与点击、调色板入口 |
| `Core/VariantPackCapabilities.cs` | **能力探测**：从包里找出上述入口点（按签名 + 名称线索），不写死类型名 |
| `Ui/CharSelectStageCycle.cs` | 选人界面"点角色切战损"：按包自己的 `s<n>_<group>_data.tres` 资源换骨骼 |
| `Core/VariantPortraits.cs` | 按变体/形态取包自己的头像图（`portrait_palettes/<变体>/s<档>_<kind>_icon.png`） |

## 一、为什么需要这一层

两个包可以**同程序集身份**（`JoanofArcIroncladHpwear, 0.3.5.0`）但内容不同。.NET 的默认加载域
只能留一份，另一份的代码永远不执行：它的门控、调色板、头像、点角色切形态全都不会发生。
本 Mod 因此把自己当成"那一份的替身"：

1. 变体下拉照常注册（两份各自独立，见 `ContractKey` 里的程序集绝对路径）；
2. 选中"没加载的那份"时，由同契约的已加载那份**代执行**调色板写入；
3. 头像、选人立绘、点角色切形态由本 Mod 按**被选中那份自己的资源**渲染；
4. 包自己的入口点被"只在它自己被选中时动作"的前缀保护，避免互相串。

## 二、新包接入：只需要这些约定

### 1. 设置型变体（必需，否则本层完全不介入）

包把自己的一套外观放在一个 int/enum 设置上，并且：

- 有一个静态成员返回设置对象（`Current`/`Instance`/`Settings`/…）；
- 属性名读起来像外观通道（`Appearance`/`Palette`/`Variant`/`Style`/…）；
- 标签来自静态 `string[]`/`List<string>`（名字像通道）或 enum 成员；
- 可选：`Persist<Channel>` 写入口、`Refresh*(SceneTree)`/`Apply*(Node)` 重绘钩子。

识别逻辑全部基于名字线索与 IL 结构，**不依赖具体类型名**。检查方式：日志里应出现
`已把 <包名> 的 N 个外观变体注册为独立皮肤选项`。

### 2. 能力探测（可选，缺哪项就少哪项功能）

| 能力 | 探测规则 | 缺失后果 |
| --- | --- | --- |
| 门控 | 静态 `bool` 方法，名字含 `Menu`/`Scene`/`Allow`/`Permit`，参数 ≤1 个，且**同一类型上至少 2 个** | 无法放行该包自己的头像/快捷键，只能靠本 Mod 代做 |
| 点角色切形态 | 静态方法，参数正好一个 `Control`，名字含 `Attach`/`Click`/`Input`/`Cycle` | 由本 Mod 的资源级切换负责（见 3） |
| 调色板重绘 | 静态方法，参数为 `SceneTree` 或 `Node`，名字以 `Refresh`/`Apply` 开头 | 变体只能靠代执行写入，重绘要等包自己的下一次 tick |
| 快捷键匹配 | 静态 `bool Matches(string, InputEventKey)` | 读不到包自己的按键绑定，只能用默认 `=` |

日志里会打印一行 `已锁定 <dll> 的“只在它自己被选中时动作”…（门控=… 点击=… 调色板=… 快捷键匹配=…）`，
新包接入先看这行确认探测到了什么。

### 3. 资源命名约定（本 Mod 自己读包内文件时用）

- 战损档：`res://<包根>/s<1..N>_<group>_data.tres`（`group` 从当前 spine 的 `s1_<group>.spskel` 反推）；
- 变体头像：`res://<包根>/portrait_palettes/<变体下标>/s<档>_run_icon.png`、
  `…/s<档>_select_icon.png`、`…/s<档>_run_outline.png`；
- 变体下标 = 下拉家族里的 0 基下标（原色 0、黑贞德 1、Saber 2、霓虹 3），与包自己的
  `appearance - 1` 对应。

**没有这些文件的包只是少这个功能，不会报错**（`VariantPortraits`/`CharSelectStageCycle` 都返回 null）。

### 4. 包根怎么来

`SkinCatalog.GetAssetRootDirectory(group, option)`：取该选项任意一个 `s1_*` 资产的目录部分，
例如 `res://images/...` 的源路径 `res://joanofarcironcladhpwear/s1_ironclad.spskel` → `res://joanofarcironcladhpwear`。
读文件走 `SkinCatalog.ReadProviderFile(group, option, 相对路径)`，**直接从包的 PCK 读字节**，
不依赖挂载后的 `res://` 命名空间（裸 PNG 在那里常常不存在）。

## 三、新包排查清单

1. 启动日志搜 `独立皮肤选项` → 变体是否识别；
2. 搜 `已锁定 …（门控=… 点击=… 调色板=… 快捷键匹配=…）` → 能力探测结果；
3. 搜 `已放行 … 门控` → 门控是否被接管；
4. 选人界面：按包自己的快捷键能否切变体（`外观变体快捷键：A → B`）、点角色能否切档
   （`选人界面点角色切战损：… 第 1 档 → 第 2 档`）；
5. 局内：切变体后看模型、左上角头像、卡面、预览是否同步；
6. 如果某一步没有日志，就是那一步的"约定"没对上——照着第二节补资源/入口即可，不需要改本层的逻辑。

## 四、禁止事项

- 不要再打开 `AllowIsolatedDuplicateProviderLoad`（把第二份 DLL 载进独立 `AssemblyLoadContext`）：
  实测第二份一注册 Godot 脚本类型/模型类型就崩游戏，已默认关闭且保留注释。
- 不要把 `ProviderSettingsVariantOptions` 的契约键改回"程序集简单名"：两份同源包会互相串写。
## 五、1.0.30.134 的清理与通用化（现状）

### 已删除的代码（都是死路或一次性试验）

- `IsolatedDuplicateProviderLoader` 与 `LoadIsolatedDuplicate` / `AllowIsolatedDuplicateProviderLoad` /
  `ProviderAssembly.RequiresIsolatedContext` / `RunIsolatedPaletteBootstrap` / 隔离初始化回退：
  这条路实测会让第二份代码在注册 Godot 脚本类型时崩游戏，已整体删除（历史原因见 `docs/issues` 里那份问题记录）。
- 选人界面的"屏幕级输入钩子"（`CharSelectStageScreenClickPatch`）：`NCharacterSelectScreen` 根本没有重写
  `_UnhandledInput/_Input/_GuiInput`，注册后什么都不会挂上，纯死代码。
- `SkinService.EnableSiblingVariantActivation`（恒为 false 的分支）与其帮手 `SiblingVariantProviderIds`。
- `IsSameAppearanceChannel`、`ProviderCharSelectCyclingPolicy` 等一次性工具。

### 现在这套机制的通用规则

1. **最全的那份执行**：同一契约形状（同程序集名 + 同设置类型 + 同属性 + 同标签）的多个包，
   按 PE 元数据类型数比较，永远让代码最全的一份跑；被选中但代码少的那份只提供自己的资源。
2. **一个行为只有一个主人**：门控方法名含 `Player` 的（局内爆衣/形态）只对"自己就是被选中的那份"答 true；
   `Menu`/`Scene` 仍允许同源代执行（选人头像、外观快捷键需要）。
3. **代执行**：被选中那份没加载时，同契约的已加载包替它写设置（`ProviderSettingsVariantOptions.Apply`）。
4. **资源按家族缓存**：外观变体与"原色"共享同一批文件，运行时 overlay 以 `FamilyBaseId` 作 key，
   连续切变体从 ~600–1300 ms 降到 ~1 ms。
5. **约定式取文件**（找不到就跳过，不会报错）：
   - 战损档：`s<n>_<group>_data.tres`、`s<n>_<group>.tres`、`s<n>_data.tres`、`<group>_s<n>_data.tres`
   - 变体头像：`portrait_palettes/<i>/…`、`portraits/<i>/…`、`variants/<i>/…`、`appearance/<i>/…`
     （先按 0 基再按 1 基各试一次，文件名 `s<stage>_{run,select,run_outline}_icon.png`）

新包接入时不需要改代码：满足第一节的"设置型变体"约定即可；有额外的入口点会由
`VariantPackCapabilities` 自动探测（日志里打印 `门控=… 点击=… 调色板=… 快捷键匹配=…`）。
## 六、"选人头像跟战损"这条路（已回退，结论留档）

1.0.30.139–156 试过 9 种写法：反射写 `_icon/_iconAdd`、扫子节点 `Icon*`、`CallDeferred` 补写、
接管按钮 `RefreshState/RefreshOutline/RefreshPlayerIcons`、冻结游戏自身刷新、让运行的包自己处理、
反射 `PortraitSync.Texture(stage, selection)`、反射 `Palette.PortraitTexture(stage, select)`、给合成图加缓存。

结论（实测）：
- 可见节点是子节点 `TextureRect:Icon` / `IconAdd`，私有字段 `_icon` 在这一版是空的。
- 包内**没有** `portrait_palettes/`；只有 `palette/s<stage>_<kind>/{index,1,2,3}.png` 与
  `s<stage>_<kind>.ctex`，后者各档是同一张图。
- 每次轮询重新取合成图会得到新实例 → 视觉上"疯狂闪烁"；缓存后不再闪，但与第 1 档差异仍不明显。

因此这条功能在 1.0.30.157 中**回退**。若将来要继续，正确的切入点是查清
`Palette.PortraitTexture(stage, true)` 在各档实际返回什么（以及它依赖的 palette 页/着色器是否在
选人界面已就绪），而不是继续往按钮节点上写贴图。