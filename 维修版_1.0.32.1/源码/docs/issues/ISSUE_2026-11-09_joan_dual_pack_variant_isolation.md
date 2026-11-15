# 战士贞德双包（0.3.5-S / 0.3.6）：变体快捷键、点击切形态、双包互不干扰

- 状态：**已修好并实机验证**（1.0.30.125：a 的局内头像恢复随变体；变体查找已缓存）
- 来源：作者实测（2026-11-09，多轮日志 + 截图 + 上号复核）
- 涉及包：
  - a = `mods\战士贞德纯S版_v0.3.5-S`（清单 id `JoanofArcIroncladHpwear111`，只有 `s1_*` 一套骨骼）
  - b = `mods\雪霁&森森改 贞德战士可战损版`（清单 id `JoanofArcIroncladHpwear`，`s1/s2/s3`）

## 一、先厘清两个完全不同的功能（上一轮我搞错了）

| 功能 | 归属 | 表现 |
| --- | --- | --- |
| **外观差分快换** | 皮肤包自己的设置项「切换外观差分快捷键」（默认 `=`，作者改成 `Key1`） | 原色→黑贞德→Saber 王庭蓝→霓虹审判官 循环，**只改配色** |
| **点角色切形态** | 皮肤包自己的 `CharSelectCycle`（b 有、a 没有） | 点选角背景角色：同一变体下切 1/2/3 档（完整/战损/更战损），改的是**骨骼** |

上一轮把"点角色"当成了快换，给 a 装了点击轮换，导致 b 自己的点击被顶掉 —— 已全部删除。

## 二、根因

1. **两个包的程序集身份完全相同**（`JoanofArcIroncladHpwear, 0.3.5.0`），.NET 只能加载一个：
   另一份被当作"差分包"，只接管 PCK/图片，不执行 DLL（日志有明确提示）。
2. **包自己的门控把 0.3.5-S 判成"不是我的皮肤"**
   `SkinSelectionGate` 里写死了 `GetSelectedRuntimeProvider("ironclad") == "JoanofArcIroncladHpwear"`
   （反射调用本 Mod 的 `SkinService`）。a 的 id 是 `…111` → 判定 false，于是包自己关掉了：
   - 选人界面头像/立绘同步（`PortraitSync.Menu`）→ "选人头像不变"
   - 局内左上角头像（`PortraitSync.RunIcon` / `PlayerAllowed`）→ "局内头像不变"
   - **选人界面里的外观快捷键**（`SceneAllowed` 也走同一门控）→ "快捷切换没用"
3. **快捷键输入本身也有坑**：a 的接收器只有 `_input`，游戏 UI 先消费就收不到（b 在 0.3.6 补了
   `window_input` 转发）。so 只靠包自己不可靠。
4. **变体契约按"程序集简单名"识别**：a 的 4 个变体和 b 的 4 个变体被当成同一个契约，
   选 a 的变体会写进 b 的 `WearSettings`（日志实证）。

## 三、改动（1.0.30.107，只改本 Mod）

1. `Catalog/ProviderSettingsVariantOptions.cs`
   - 契约加 `ProviderId`，`ContractKey` = **程序集绝对路径 + 包 id + 设置类型 + 属性**。
   - 找程序集先查 `ManagedSkinModLoader.TryGetLoadedProviderAssembly`（提供者 DLL 多为内存流加载，
     `Location` 为空），再按磁盘路径精确匹配；**不再按简单名兜底**。
   - 新增 `ContractOf / Family / NextFamilyOption / ReadHotkeyBinding / TryGetExecutorContract /
     ContractForAssembly / SharesContractShape / IsPackageLoaded`。
2. `Core/VariantHotkeyPoller.cs`（新）—— **接管皮肤包自己的外观快捷键**
   - 0.05 s **轮询物理按键**（不依赖事件是否被 UI 消费），匹配的绑定从**当前皮肤所属包的设置对象**
     读（`AppearanceHotkey`，支持 `Key1`、`=`、`Ctrl+Shift+F1` 这几种写法）。
   - 只在**当前这个包自己的变体家族**里前进一格；`a`/`b` 各算各的家族。
   - 触发的动作走和下拉完全相同的路径（`ContextualSkinControls.RequestFrameworkSelection`），
     所以下拉、选人头像、预览小人、背景大图一起刷新。
3. `Core/ProviderMenuGateCompat.cs`（新）—— **放行同源包的门控**
   - 当"当前选中的变体"与这个包**同契约**（程序集名/设置类型/属性/标签全同）时，
     让 `MenuAllowed/SceneAllowed/PlayerAllowed` 返回 true：包自己的选人头像、局内头像、
     菜单里的快捷键恢复工作。
   - 同时给它的 `CharSelectCycle.Attach/OnInput` 加前缀：**只有这个包自己拥有当前选择时**才允许
     点角色切形态，避免把 b 的 s2/s3 骨骼套到 a 的模型上。
4. `Core/ManagedSkinModLoader.cs`：新增 `TryGetLoadedProviderAssembly / TryGetLoadedProviderPath`。
5. `Catalog/ProviderCharSelectCyclingPolicy.cs`、`Ui/CharSelectSkinClickCycle.cs`：**删除**（上一轮的错误方案）。

## 四、实机验证（本机 2562×1440，截图对比）

| 场景 | 结果 |
| --- | --- |
| a（ab 同装、a 执行）按 `Key1` | `原色→黑贞德→Saber 王庭蓝→霓虹审判官→原色`，每次**只前进一格**，日志 `外观变体快捷键：… → …` ✅ |
| 同上，看四处是否对应 | 角色皮肤下拉 `text='战士贞德 血量换装111_黑贞德' id='…::variant:1'`；选人头像、预览小人、背景大图同色 ✅ |
| 同上，点选角背景角色 | 无任何反应（a 只有 1 档）✅ |
| b（只装 b）按 `=`（它自己的绑定） | `variant:1→variant:2`，配色变 Saber ✅ |
| b 点选角背景角色 | `char-select: stage 1->2/3`、`2->3/3`，**变体不变**、只换战损形态；预览与头像同步 ✅ |
| ab 同装、选中 a 的变体 | 只动 a 的家族；b 的 4 条变体不被带动 ✅ |

测试用的临时手段：把 a 的目录临时移出 `mods` 得到"只装 b"，测完已原样移回；
`Gurio.SkinChanger/skin_changer.json` 的 `Selections.ironclad` 已恢复为 `JoanofArcIroncladHpwear111::variant:1`。

## 五、边界

- ab 同装时只有一份 DLL 会执行，**共用同一个调色板值**：选 a 的变体时，由已加载的那份（可能是 b）
  代写同一个下标（按标签名映射）。下拉/快捷键的归属仍按包隔离。
- a 没有 `s2/s3` 资源，"按血量战损"在 a 上不可能出现；点背景无反应是正确行为。
- 快捷键被本 Mod 接管后，包自己的那条分支被静音（避免一次按键走两格）；包的其他按键（如 b 的
  `F6` 安全换骨架）不受影响。
## 六、1.0.30.109 追加：ab 同装的真实阻断点

### 已修（本轮根因）

1. **契约的 `AssemblyName` 取自文件名**（`Path.GetFileNameWithoutExtension`），而 a 的文件名是
   `JoanofArcIroncladHpwear111.dll`、真正的程序集名是 `JoanofArcIroncladHpwear` →
   `SharesContractShape` 永远为 false → **同契约代执行从来没生效**。
   这正是"ab 同装时 b 只显示 a 最后一个变体"的根因。现已改读元数据里的程序集名。
2. 两条独立路径的基础设施已就位（但默认关闭）：`IsolatedDuplicateProviderLoader` 能把第二份
   DLL 载进独立 `AssemblyLoadContext`（依赖全部回落到默认域，所以 Harmony/游戏类型共用、
   静态状态各自独立）；`GetActiveRuntimeProviders` 里有 `EnableSiblingVariantActivation`
   开关，打开后同契约兄弟包会被一起激活（实测两份都进了"完整视觉会话"，游戏不崩）。
   `BuildInitializerKey` 也补上了加载域名——否则字节相同的两个模块会撞 key，
   第二份的初始化器会被当成"重复"跳过。

### 仍未完成

- 第二份（隔离域里那份）的**作者初始化器没有真正跑起来**：日志里有它的
  `已按当前选择启用 … 完整视觉会话（27 个补丁）`，但没有它的 `palette ready`/`HP wear armed`。
  已尝试：按 `[ModInitializer]` 特性取初始化器、按 `ModEntry.Initialize` 兜底、跳过
  `FrameworkProviderInitializerPolicy` —— 均未触发（`GetLoadableTypes` 在隔离域里能读到 79 个类型，
  说明类型加载没问题，问题在特性/方法匹配或重复键判断）。
- 在该点修好之前，`EnableSiblingVariantActivation` 保持 false：ab 同装时 b 仍只有**资源 + 代执行
  调色板**（变体会正确换色），但点角色切战损没有。a 单独/b 单独的行为与 1.0.30.107 一致（已验证）。

### 下一步（明确）

1. 找到隔离域里 `ModEntry.Initialize` 未被调用的确切原因（在 `RunIsolatedInitializerFallback`
   里逐条打点：类型数、找到的类型名、取到的方法、是否已存在 key）。
2. 该初始化器跑起来后，打开 `EnableSiblingVariantActivation`，实机验证：
   - ab 同装、选中 b → b 的 4 个变体 + 点角色 1/2/3 档都正常，a 不跟着动；
   - ab 同装、选中 a → 反之；
   - 下拉/选人头像/预览小人/背景大图四者一致。
## 七、1.0.30.110 进展与卡点（继续中）

本轮把"第二份代码为什么跑不起来"逐段打点，结论：

1. ✅ 隔离域加载成功：`已把同源皮肤包 JoanofArcIroncladHpwear111.dll 载入独立程序集域`，
   域内能枚举到 79 个类型，`JoanofArcIroncladHpwear.ModEntry` 存在。
2. ✅ 兄弟包能被解析出来（`SiblingVariantProviderIds` 用 `ResourceNamespaceId`＝清单 id，
   两个包各自独立；`AssemblyName` 已改为读元数据，契约形状比较这才成立）。
3. ⚠️ 但把兄弟包加进 `active` 之后，**它的 `ActivateProvider` 没有走完**：
   既没有 `启用 …111 的完整视觉会话`，也没有我加的 `RunIsolatedPaletteBootstrap` 日志，
   而同一段里紧邻的 `[iso-probe]` 却打出来了 —— 说明激活流程在探针**之后**被跳过/中断，
   目前怀疑点：`RuntimeProviderScopePolicy.SelectActiveProviders` 对同组（ironclad）只留一个，
   或 `ActivateSelectedProviders` 的去重/`CanActivateBehavior` 判定把它挡掉。
4. 代码里保留了完整可用的基础设施（隔离域加载、`RunIsolatedPaletteBootstrap`、
   `EnableSiblingVariantActivation` 开关、两个包的"只在自己被选中时动作"前缀），
   默认关闭，避免半成品状态影响正常使用。

**下一步（就查这一条）**：在 `RuntimeProviderScopePolicy.SelectActiveProviders` 与
`ActivateSelectedProviders` 各加一条一次性日志，确认 a 的 providerId 是在哪一步被丢掉的；
放行后打开开关，再按作者清单实机验四条。
## 八、隔离域路线实测结论：会崩，已回退

实测（1.0.30.110 调试构建）：两个包都被激活（日志 `[act] activate JoanofArcIroncladHpwear111`），
紧接着游戏进程直接消失，`godot.log` 停在 `已把皮肤提供者 JoanofArcIroncladHpwear111 的程序集纳入模型类型发现`。
说明第二份代码（在自己的 `AssemblyLoadContext` 里）真正开始打补丁/跑初始化器之后，
会在引擎层崩掉（同两个包各自注册 Godot 场景脚本、同 ID 的 Harmony 补丁两次作用等）。
因此该路线**不能作为发布方案**，已把 `EnableSiblingVariantActivation` 关掉、调试日志全部删除。
保留的代码只作为下一步尝试的基础，不再默认启用。

## 九、下一步：不加载第二份代码的"资源级战损切换"

不依赖第二份 DLL，由本 Mod 自己完成 b 的"点角色切战损"：

1. 点击时定位选角背景里的 `SpineSprite`，按当前选中包（b）的 PCK 里
   `s{n}_charselect_data.tres` 依次换 1→2→3（`set_skeleton_data_res` + 重放 idle）。
2. 只在该包**有 s2/s3 资源**且**它自己的代码没在跑**时启用，避免与包自己的点击重复。
3. 头像/立绘：包自己的 `PortraitSync` 不会跑，需要本 Mod 按
   `portrait_palettes/<变体序号>/s<stage>_select_icon.png` 取图（a/b 都按这个约定放了图，
   第 4 变体缺图时回退 s1 图）。
4. 验证点：ab 同装、选中 b → 点角色切 1/2/3 档，变体不变；下拉/头像/预览/背景四处一致；
   选中 a → 点角色仍然无反应。
## 十、1.0.30.112：点角色切战损改由本 Mod 自己做（已实机验证）

不再依赖第二份 DLL。新增 `Ui/CharSelectStageCycle.cs`：

1. 钩 `NCharacterSelectScreen.SelectCharacter` + `ShowCharacter`，在选角背景上挂 0.2 s 宿主流；
2. 读当前 spine 的骨骼路径（`skeleton_data_res.skeleton_file_res.ResourcePath`，实测
   `res://joanofarcironcladhpwear/s1_charselect.spskel`），推出同目录的
   `s<n>_charselect_data.tres`，存在才认为"这个包有战损档"（a 只有 s1 → 不挂，点角色仍然无反应）；
3. 点击时 `spine.Set("skeleton_data_res", 资源)` → 重放 idle → 调包自己的 `Palette.ApplyNow(spine)`
   （反射；包在跑时用它保住配色/变体色调）；
4. 判档：s2 不存在就不接管；到顶回 s1。

实测（b 为选中皮肤）：日志
`选人界面点角色切战损：charselect 第 1 档 → 第 2 档`、`第 2 档 → 第 3 档`，
截图确认变体不变、只有战损程度变化；a 选中时不会接管（它没有 s2 资源），点角色依旧无反应。

### 仍未对齐

- **选人头像 / 右侧小预览** 不跟随战损档：包自己的点击会调 `PortraitSync.RefreshMenuNow`
  （按 `portrait_palettes/<变体>/s<档>_select_icon.png` 换按钮头像）与预览重建，本 Mod 目前只换了
  背景骨骼 + 按钮头像的尝试（`RefreshCharacterButtons`，尚未生效，下一步查）。
- 需要在"ab 同装、b 未加载"的真实场景再确认一次（本 Mod 的切换只依赖被选中包的 PCK，与哪份 DLL 在跑无关）。
## 十一、1.0.30.113/114：a 闪退修复 + 现状

### 闪退根因（已修）

选 a 的时候，`GetOrLoadProviderAssembly` 发现 a 与已加载的 b 同名不同内容，命中了我上一版加的
"同契约就载入独立程序集域"分支 → 第二份代码注册 Godot 脚本类型/模型类型时把游戏打崩
（日志最后两行正是 `已把同源皮肤包 …111.dll 载入独立程序集域` → `已按当前选择注册 …111 的 Godot 场景脚本类型`）。
现已用 `AllowIsolatedDuplicateProviderLoad = false` 关掉该分支：同名包继续走"只接管资源 + 由本 Mod 代执行调色板"，
与 1.0.30.107 的行为一致（那一版 a 是正常的）。

### 现状（1.0.30.114，实机）

- a 选中：正常载入（9 pages），`Key1` 切 4 个变体，点角色无反应（它没有 s2 资源）✅ 不再闪退
- b 选中：`Key1` 切 4 个变体；点角色 1→2→3 档（本 Mod 自己换骨骼资源），变体色调保留 ✅
- **未对齐**：底部选人头像、右侧小预览仍停在 s1。已尝试：从包内 `portrait_palettes/<变体>/s<档>_select_icon.png`
  用 `FileAccess + Image.LoadPngFromBuffer` 生成纹理并写回 `NCharacterSelectButton` 的 `%Icon`；
  调整了刷新顺序（先预览、后头像）仍未生效，下一步查按钮节点定位（`Character` 属性比较 / `%Icon` 实际命名）
  以及预览面板重建路径。
## 十二、1.0.30.115

### a 固定显示 b 最后一次的变体（已修）

原因是我自己上一版加的"调色板刷新只归包所有"前缀：a 被选中时它自己没加载，实际画调色板的是 b，
于是 b 的 `Palette.RefreshAppearance` 被我挡住了 —— 代执行写进去的值永远不生效，画面停在 b 上一次的值。
现在调色板守护放宽为**所有者或代执行者**（同契约 + 被选中包未加载），点角色切换的守护仍严格归所有者。

### 奥斯提下拉消失（已修）

目录里没有 `osty` 分组时（当前没有启用带 osty 资源的皮肤包），`PopulateOsty` 直接把下拉藏了，
连独立奥斯提 Mod 自己的皮肤一起消失。现在：只要**独立奥斯提 Mod 提供了皮肤**或**已保存过选择**，
下拉就继续显示（游戏默认 + 独立皮肤 + 随机 + 工坊）；`ApplyCompanionGroupSelection` 在没有分组时
把选择记为游戏默认并返回成功，不再报"未知的随从皮肤选择"。

### 顺手修掉的闪退隐患

`CharSelectStageCycle` 里读 `skeleton_data_res` / `get_animation_state` 的 `Variant` 没释放，
和工作室版日志里的闪退同一类（见下）——已全部 `using var` 释放。

## 十三、创意工坊版 1.0.29.100 的闪退（据日志 + 事件查看器）

日志：`godot2026-11-09T21.57.13.log`（版本 1.0.29.100，**多人联机对局中**），最后一行是
`MuteInBackground: FocusOut received`，之后没有 `Steamworks shutdown` —— 进程直接消失。
Windows 事件查看器 `.NET Runtime` 1026（21:56:43）给出的栈是：

```
The process was terminated due to an unhandled exception.
   at Godot.GodotObject.Dispose(Boolean)
   at Godot.GodotObject.Finalize()
   at System.GC.RunFinalizers()
```

即**GC 终结器线程在释放一个已经被原生侧销毁的 Godot 对象**（同一晚更早的几次是
`ConvertCallableToNative` / `godotsharp_string_new_with_utf16_chars` 的 Finalize 帧，同一类问题）。
这是"托管包装器没在主线程释放/原生对象先没了"的典型形态，和当年骨妹换装的 PRG-6985
`Variant` 泄漏崩溃同源；1.0.29.100 是旧版，SkinChanger 与皮肤包里都还有未释放的
`Variant`/`Callable`。新版（1.0.30.x）已把这类调用改成 `using var` 释放，本次又补了一处。
建议：让玩家更新到当前测试/正式版本；若仍复现，用事件查看器的 1026 栈定位到具体类型。
## 十四、1.0.30.116/117：局内外观同步（左上角头像已实机验证）

### 根因
包自己的 `PortraitSync` 会在每次刷新时把左上角头像换成
`portrait_palettes/<appearance-1>/s<stage>_run_icon.png`；但那份代码只在"占住程序集的那一份"里运行，
被顶掉的那份（a）永远拿的是 `s1_run_icon.ctex`（= 原色立绘），所以变体换了头像不动。
本 Mod 自己的头像接管（`CharacterModel.IconTexture/Icon/CharacterSelectIcon/MapMarker` 的 Postfix）
也只读规范路径 `ui/top_panel/character_icon_ironclad.png`，同样是原色。

### 做法
新增 `Core/VariantPortraits.cs` + `SkinCatalog.GetAssetRootDirectory/ReadProviderFile`：

1. 从当前选择的选项的资产里推出这个包的资源根（例如 `res://joanofarcironcladhpwear`）；
2. 变体序号 = 选项在家族里的下标（原色 0、黑贞德 1、Saber 2、霓虹 3，正好等于包的 appearance-1）；
3. **直接从该包的 PCK 里读** `portrait_palettes/<序号>/s<stage>_{run,select,run_outline}_icon.png`
   （这些是裸 PNG，挂载后的 `res://` 里不一定存在，所以走 `PckArchive.ReadFile` 再 `Image.LoadPngFromBuffer`）；
4. 在 `ReplaceCharacterTexture`（`IconTexture`/`IconOutlineTexture`）、`ReplaceCharacterSelectTexture`、
   `ReplaceCharacterIcon`（`Icon` 场景里的 TextureRect）里优先用它；地图标记路径跳过（包没有按变体的标记图）。

### 实机（1.0.30.117，战斗中按 Key1）
- 黑贞德 → 左上角头像变深色；Saber → 头像变金蓝；原色 → 回到原色立绘（该档包本身没有 `portrait_palettes/0`，与包自己行为一致）。
- 同一时刻战斗中的模型、卡面、预览都跟着变，没有推翻之前已修好的选人界面行为。

### 仍可能不完全一致
- 地图小图标：两个包都没提供"按变体"的标记图，保持它们自己的标记（不随变体）。
- 火堆/商店：靠包自己的调色板（`s1_restsite` / `s1_ironclad_shop` 页）着色；若某档缺页会退回原色，
  这与包单独运行时的行为一致。
## 十五、1.0.30.118/119：把这一套做成可复用的接口

新增 `Core/VariantPackCapabilities.cs`：**按签名 + 名称线索从包里探测入口点**，不再写死
`SkinSelectionGate` / `CharSelectCycle` / `Palette` 这些类型名。

| 能力 | 探测规则 | 用途 |
| --- | --- | --- |
| 门控 | 同一类型上至少 2 个静态 `bool` 方法，名字含 `Menu`/`Scene`/`Allow`/`Permit`，参数 ≤1 | postfix 放行：选中同源变体时让包自己的头像/快捷键照常工作 |
| 点角色切形态 | 静态方法，第 1 个参数是 `Control`（可带第 2 个事件参数），名字含 `Attach`/`Click`/`Input`/`Cycle` | prefix 收紧：只有该包自己的选择才允许切形态 |
| 调色板重绘 | 静态方法，参数 `SceneTree` 或 `Node`，名字以 `Refresh`/`Apply` 开头；优先选类型名含 `Palette`/`Tint`/`Color` 的 | prefix 放行"所有者或代执行者" |
| 快捷键匹配 | 静态 `bool Matches(string, InputEventKey)` | 读包自己的按键绑定（轮询驱动） |

同时去掉了 `ProviderMenuGateCompat` 与 `CharSelectStageCycle` 里写死的 `"ironclad"` 分组 id
（改为从当前选择/当前选人角色取）。

实机日志（1.0.30.119，a 的 DLL）：

```
已锁定 JoanofArcIroncladHpwear111.dll 的“只在它自己被选中时动作”…
（门控=SkinSelectionGate(MenuAllowed/AllowPlayer/PlayerAllowed/SceneAllowed)
 点击=-(-) 调色板=Palette(RefreshAppearance/ApplyNow/Apply) 快捷键匹配=AppearanceHotkey.Matches）
```

`点击=-` 正是 a 的特性（0.3.5-S 没有点角色切形态的代码），说明探测是准的。
接入新包时看这一行就能知道缺哪一块。

适配指南已单独成文：`docs/VARIANT_PACK_ADAPTER.md`（约定、能力表、排查清单、禁止事项）。
## 十六、1.0.30.120–122：运行时切换后点角色切战损（未完成）

### 已确认的机制（用探针实测）

1. 选人界面的节点树：`screen` 下 `0:StaticBg(Pass,隐藏)`、`1:AnimatedBg(Stop,可见, -388/-80/2560x1200)`，
   `AnimatedBg` 里 `0:IRONCLAD_bg(Control, Stop)`；**`NCharacterSelectScreen` 本身没有重写
   `_UnhandledInput/_Input/_GuiInput`**，所以屏幕级 Harmony 钩子挂不上（日志括号为空）。
2. 包自己的拦截层是 `bg.AddChild(Control{MouseFilter=Stop, FullRect}) + MoveChild(last)`（`JoanofArcIroncladHpwearClickCatcher`）。
3. 把拦截层挂在 `AnimatedBg`（背景的父容器）上时，**点击确实能进到我们的 `OnClick`**；
   挂在 `screen` 上则因为 screen 不是有尺寸的 Control（rect=0）收不到点击。
4. 切到"代码没执行的那份包"之后，`ResolvePlan` 能通过（说明判档改成读 PCK 之后是准的），
   但 `LoadStageData` 在当时的命名空间里找不到 `s2_<group>_data.tres` —— 于是静默返回、点击看起来"没反应"。
   已加：`EnsureProviderPackMounted()`（用 `Catalog.GetProviderResourcePackPaths(selection)` + `ProjectSettings.LoadResourcePack`）
   把被选中那份包自己的 PCK 整体挂上，再 `ResourceLoader.Load`；仍失败才走 `WithRuntimeResources`。

### 目前状态

- b 一开始就选中（b 的 DLL 在跑）：点击由包自己处理 ✅
- a 一开始就选中：点击无反应（a 没有 s2 资源）✅ 符合预期
- **a 先加载 → 运行时切到 b**：`OnClick` 能进（AnimatedBg 挂载方式），但 `LoadStageData` 仍返回 null，
  切档没有发生 ❌ —— 下一步：确认 `GetProviderResourcePackPaths(b 的选项)` 在 delta 状态下返回了什么、
  以及 `s2_charselect_data.tres` 在这个 PCK 里的真实路径（`res://joanofarcironcladhpwear/s2_charselect_data.tres`
  是包自己代码用的路径，理论上应存在）。

### 备注

- 调试用的临时键（F7 切皮肤）与所有探针日志已从 1.0.30.122 中移除。
- 不要为了省事把拦截层挂回 `screen`（rect=0，收不到点击），也不要改成 `_UnhandledInput` 屏幕钩子（挂不上）。
## 十七、1.0.30.123/124：局内爆衣归属（实机验证）

### 现象
b 该有爆衣、a 完全没有（a 只有残血换动画）。实际却是"b 不能爆衣、a 能爆衣"，后来又"两个都不爆衣"。

### 根因两个

1. **执行权归错了包**：同程序集身份只能跑一份，谁先被选中谁占住身份。a（0.3.5-S 精简版）先占住时，
   b 的爆衣代码根本没运行；b 先占住时，我上一版的"同源代执行"又把 b 的 `PlayerAllowed` 放行给了 a 的选择，
   于是 a 身上出现了 b 的爆衣。
2. **门控放行过头**：`AllowWhenSiblingVariantSelected` 对 `PlayerAllowed` 也做了同源回退。

### 修法

1. `ProviderMenuGateCompat.AllowWhenSiblingVariantSelected`：门控方法名含 `Player` 时**不再**做同源回退
   （玩家/爆衣只认"自己就是被选中的那份"）；`Menu`/`Scene` 仍保留回退（选人头像、外观快捷键需要）。
2. `ProviderSettingsVariantOptions.FindRicherSiblingAssemblyPath` + `ActivateSelectedProviders`：
   同一契约形状的多个包里，**永远让代码最全的那份执行**（按 PE 元数据的类型数比较；b=129024B/更多类型，a=112640B）。
   日志：`同源皮肤包 JoanofArcIroncladHpwear111.dll 代码更少，改由 JoanofArcIroncladHpwear.dll 执行行为；选中的那份只提供它自己的资源。`

### 实机（1.0.30.124，两个包都装）

- 选 a，打到 16/80：模型**没有爆衣**（和 a 单独时一致）✅
- 选 b，打到 16/80：模型**爆衣**（外套打开、内衣露出），日志 `portrait run: player=1 stage=2 hp=32/80` ✅
- 变体、选人界面、局内头像等此前的功能未受影响。
## 十八、1.0.30.125：a 的局内头像 + 变体切换卡顿

### 头像又回到默认的原因

上一节把 `PlayerAllowed` 收紧后，b 的 `PortraitSync`（负责换左上角头像）对 a 的选择就被门控挡住了——这是对的
（爆衣不该串），但头像不能再依赖它。而本 Mod 自己的头像接管里有一条"运行时提供者自己不画就交给它"的让位
（`ShouldDeferCharacterIconResourceToExternalRuntime`），此时那个提供者恰好被挡住，于是谁都没画，头像停在默认图。

修法：**"设置型变体"的选择一律不让位**（`ContractOf(visualSelection) != null` → 直接由本 Mod 接管），
头像继续走 `VariantPortraits`（从被选中那份包的 PCK 里读 `portrait_palettes/<变体>/s<档>_run_icon.png`）。

实机（1.0.30.125，选 a，战斗中按 Key1 到 Saber）：左上角头像变成金蓝 Saber ✅

### 卡顿

`VariantPortraits.Resolve` 之前每次调用都要：
读选择（加锁）→ `ContractOf` → `Catalog.GetAssetRootDirectory`（LINQ 扫 124 个分组的所有资产）→ 读 PCK。
而 `CharacterModel.IconTexture / Icon / MapMarker` 这三个 getter **每帧**都会调用它，于是每帧都在做整表扫描。

修法：按 `(分组, 类型, 档位)` 备忘，命中时只比较一次"选择字符串是否变了"；选择一变（`ApplySelection` 和快捷键切换）
就 `ClearCache()`。现在每帧只剩一次字典查找。
## 十九、1.0.30.126–129：一轮完整回归（含两个真回归）

### 真回归 1：快捷键「失效」其实是绑定变了

`VariantHotkeyPoller` 读的是**皮肤包设置里的实际按键**。当前 b 的设置为 **F1**（不是 Key1），
我先前用 Key1 测所以一直"没反应"。改按 F1 后日志立刻是
`外观变体快捷键：…::variant:1 → …::variant:2`。结论：轮询器本身没坏。

### 真回归 2：每帧都在做门控扫描（卡顿来源）

删调试代码时把 `_elapsed = 0;` 一起删掉了，于是 `VariantHotkeyPoller.Tick` 每帧都通过间隔判断，
进而每帧调用 `ProviderMenuGateCompat.Tick`（反射扫描全部门控）。已恢复，5 Hz 一次。
另外 `CharacterAppearanceRuntime.ReplaySelectedCreatureNodeReady` 对**预览用的** `FrameworkPreviewCreature`
也会重放包里的 `OnCreatureReady`，在 `NCreature.UpdateBounds` 里 NRE（日志能看到），
每次换外观都抛一次——预览生物已跳过重放。

### 选人界面点击拦截层的定位（定论）

- 挂在 `screen` 上：rect=0，永远收不到点击（screen 不是有尺寸的 Control）。
- 挂在角色背景（`IRONCLAD_bg`）上：SkinChanger 换皮肤会重建这个节点，拦截层随即失效。
- 挂在背景的父容器（`AnimatedBg`）并置于最后一个子节点：**稳定有效**，且不会挡住按钮/下拉。
- 点击坐标也很关键：角色胸口那块被本 Mod 的预览/合并控件盖住，点那里不会触发；点画面偏上/偏右（如 2350,250）可以。

### 回归矩阵（1.0.30.129）

| 项目 | 结果 |
| --- | --- |
| b：选人界面点背景切战损（1→2→3→1） | ✅ 实机通过（日志 + 截图） |
| b：快捷键切变体（F1） | ✅ 实机通过 |
| a：选人界面点背景 | ✅ 无反应（a 没有 s2） |
| a：快捷键切变体 + 局内头像 | ✅ 通过（125 实机） |
| b：局内爆衣（16/80） | ✅ 通过（124 实机） |
| a：局内不爆衣（16/80） | ✅ 通过（124 实机） |
| 变体切换卡顿 | 已修（每帧门控扫描 + 预览 NRE） |
| a：快捷键切变体 | 本轮未复测（可点 F1 直接看） |
## 二十、1.0.30.130/131：局内左上角头像（a 的变体）

### 根因
局内 HUD 的左上角头像是 `player.Character.Icon` 这个 **Control 实例**。刷新时
`RefreshCurrentCharacterUi()` 把它重新 `AddChild` 到 `TopBar.Portrait` 下——但 HUD 复用的还是**同一个实例**，
所以 `CharacterModel.Icon` getter 里即使换成了新贴图，屏幕上那个节点也没被重绘。
本 Mod 的 `ReplaceCharacterIcon` 里的变体贴图分支还排在 `SelectedSkinProvidesResource / ShouldDefer` 两道门之后，
a 的选择被挡时根本走不到。

### 修法
1. `ReplaceCharacterIcon`：**先**用 `VariantPortraits.Resolve(groupId, "run", 1)` 给"现有的" Control 里的
   TextureRect 上色，再走原来的场景替换逻辑（替换出来的新场景同样会上色）。
2. `CharacterAppearanceRuntime.RefreshCurrentCharacterUi`：重新 `AddChild(Icon)` 之后立刻
   `VariantPortraits.Paint(portrait, groupId)`，直接给 HUD 上那个节点上色。
3. 选人界面的三条路径（`ReplaceCharacterSelectTexture` 等）**没有改动**，只新增了函数。

### 验证状态（诚实记录）
- 选人界面：a/b 的变体、b 的点背景切战损、a 点背景无反应 —— 与上一版一致（未改动）。
- 局内：脚本按键在后半段不再被游戏接收（窗口焦点问题），只抓到一次
  `外观变体快捷键：…variant:3 → …variant:1` + `已刷新局内左上角角色头像：…variant:1`，
  画面头像与该变体一致；**Saber 档的局内头像尚未抓到截图**，需要人工按键确认。
## 二十一、复核：b 的爆衣在 1.0.30.131 仍然正常

用你当前这局的存档（继续游戏）实测：

- 48/80：`portrait run: player=1 stage=1 hp=48/80`，模型原色未爆衣
- 39/80：`portrait run: player=1 stage=2 hp=39/80`，进入第 2 档
- 13/80：模型外套打开、内衣露出（截图确认）

也就是说 1.0.30.131 没有破坏爆衣；上一局你停在 39/80 左右，那时刚好是第 1→2 档的临界，
再掉一点血才会明显看到。b 的档位阈值大致是 50% 与 25%~30% 两处，a 则始终只有一套（不爆衣）。
## 二十二、1.0.30.132/133：变体切换的卡顿（按日志量化后修）

### 日志给出的事实（1.0.30.131 那一局）

- 13 次"独立资源"重建，累计 8.18 秒，**单次中位数 625 ms**，每次都伴随一次 21~33 MiB 运行包的重建；
- "开始重建外观覆盖" 出现了 39 次（每次切换触发两次）。

### 修法

1. `GetOrPrepareRuntimeOverlay`：外观变体的资源与"家族基准选项"**完全相同**，所以 overlay 一律按
   `FamilyBaseId` 准备（否则每个变体 id 都算新 key，每次都重建 60+ 文件/20+ MiB 的包）。
2. `ApplySelection`：`stateOnly` 现在也认"同一个包的另一个外观变体"（`ProviderSettingsVariantOptions.IsSamePackageVariant`），
   这类切换只写设置、不重新挂载整包。

### 实测（1.0.30.133，选人界面按 F1 连续切）

- 变体切换从 **625~1313 ms** 降到 **0.3~0.9 ms**（日志里变成 `已复用 … 耗时=0.3 ms`）；
- 仍会慢的是"基准选项 ↔ 某个变体"以及进入对局后的第一次切换（模型本身要重建），单次 200~880 ms。

结论：连续切变体已经不再卡；剩下的是首帧建模型的固有成本。
## 二十三、1.0.30.135/136：选人头像跟战损 + 原色变体的局内头像

### 1. 点角色切战损时选人头像不变

两个原因：
1. `NCharacterSelectButton` 的头像不在名为 `Icon*` 的子节点里，而是私有字段 `_icon` / `_iconAdd`
   （`TextureRect`，场景里序列化为 `%Icon` / `%IconAdd`）。之前只扫描子节点名，永远改不到。
2. `LoadStageIcon` 还在用老的 `FileAccess` + 挂载后 `res://` 路径读 PNG，a/b 的包在这种命名空间下读不到，
   于是返回 null、直接放弃。

修法：头像改成**反射写 `_icon`/`_iconAdd` 字段**（再兜底扫描名字含 Icon/Portrait 的 TextureRect）；
取图统一走 `VariantPortraits.Resolve(groupId, "select", stage)`（从 PCK 直读）。
实机：b 的选人界面点背景 1→2 档时，左下方选人头像**跟着换了**（截图对比）。

### 2. a 的「原色」变体局内头像没了

我上一版把头像候选路径写成了"下标 i，再试 i+1"，结果原色（下标 0）命中了
`portrait_palettes/1/…`（黑贞德的图），而真正的原色没有 `portrait_palettes/0` 目录，
于是看起来"没有头像"或者"头像不对"。

修法：**下标 0（原色）不再查变体目录**，直接走包自己的规范头像（`ui/top_panel/character_icon_*.png`），
只有 1 及以后才查 `portrait_palettes/<i>/…`。这样 4 个变体都有头像：原色用包默认图，其余用各自目录。
## 二十四、1.0.30.137/138：撤销"变体切换不重挂"的优化

### 症状（用户反馈）
切变体（不是切战损）时：预览小人、选人背景、局内战斗小人的颜色都不变。

### 原因
1.0.30.133 为了消灭卡顿，把"同包另一个变体"也当成 `stateOnly`，于是**跳过了 MountOverlay**。
那次重挂虽然要 ~0.9 s，但它同时重建了选人预览/背景并把调色板应用到局内模型；
跳过后只有包自己的 `Palette.RefreshAppearance` 会被调用，而预览小人/背景不在它的刷新范围内。

### 处理
- `stateOnly` 恢复为只认 `IsSameManualVariantSource`（即变体切换**必须**重挂）。
- 保留 overlay key 按 `FamilyBaseId` 归一化的改动（无害，减少不同变体之间的重复准备）。
- 尝试过"重挂但不清运行时资源缓存"来省下 33 MiB 的重新打包：实测仍是 950 ms，说明这笔开销在
  `MountOverlay` 自己（每个分组重新生成 overlay PCK），不在缓存层，所以先按正确性优先处理。

### 现状与代价
- 变体切换：背景、预览、局内模型颜色、选人头像、局内小头像全部跟着变（1.0.30.138 实机确认局内显示 Saber）。
- 代价：每次切变体约 0.95 s（就是那 33 MiB 运行包重建）。下一步若要既快又对，需要给
  `MountOverlay` 的 overlay 生成加缓存（按分组 + 资源集合做 key），而不是再跳过它。
## 二十五、1.0.30.139：选人头像"闪一下又变回去"

### 现象
点角色切战损时，选人头像的表情会变一瞬，随即恢复原样。

### 原因
`CycleWithLog` 里先调 `CharacterModelPreviewControls.Refresh`，它会**延迟**（`CallDeferred`）重建预览，
重建过程会调用按钮自己的 `RefreshPlayerIcons`，把 `_icon` / `_iconAdd` 重新按当前皮肤（第 1 档）设置。
我们的 `RefreshCharacterButtons` 是同步执行的，跑在它前面，于是被覆盖。

### 修法
写完头像后，再用 `Callable.From(...).CallDeferred()` **补写一次**，等延迟重建跑完再落一次；
实机等 3 秒截图确认：第 2 档的表情（外套打开）保持住了，没有再回退。
## 二十六、1.0.30.141：把"补写一次"换成接管按钮刷新

1.0.30.139 只是在自己写完头像后 `CallDeferred` 再写一次，属于"抢时序"，换成别的刷新来源又会回退。
1.0.30.141 改成**接管按钮自己的刷新入口**：扫描 `NCharacterSelectButton` 上名字含 `Refresh`
且参数 ≤1 的方法（实机命中 `RefreshState` / `RefreshOutline` / `RefreshPlayerIcons`），在它们的
postfix 里按"记住的档位"重新上色。谁刷新都一样，不会再被盖回去。

同时把当前档位记在 `StageByEntry` 里（绑定时清空），`ApplyRememberedStageIcon` 用
`VariantPortraits.Resolve(groupId, "select", stage)` 取图。

实机（1.0.30.141）：点背景到第 2 档，**等 6 秒**再截图，头像仍是第 2 档的表情；
日志确认 `选人界面战损头像：已接管按钮的刷新入口（RefreshState/RefreshOutline/RefreshPlayerIcons）`。
## 二十七、1.0.30.152：选人头像跟战损——目前的实测结论（未达成）

### 我自己跑出来的事实（截图对照，不是猜）

- 点背景：背景从第 1 档换到第 2 档，并且**保持**（包自己的日志 `char-select: stage 1->2/3`）。
- 左下的选人头像：**从头到尾没有变化**（连"闪一下"都没在我这边复现）。
- 节点结构：按钮里是 `TextureRect:Icon` / `TextureRect:IconAdd`（可见），而 `_icon` / `_iconAdd`
  私有字段在当前版本里是空的（tex=0），所以之前反射写字段完全没生效。
- 包的资源：`portrait_palettes/` 在 b 的 PCK 里**不存在**；存在的是
  `palette/s<stage>_<kind>/{index,1,2,3}.png`（调色板页）和 `s<stage>_<kind>.ctex`。
  后者各档是**同一张图**，所以直接写 `Icon` 也不会有可见变化；
  各档真正的差异由包自己的 `Palette.PortraitTexture(stage, selection)` 用调色板合成。

### 已尝试（均无效，记录以免后人重走）

1. 反射写 `_icon`/`_iconAdd` 字段（字段为空）。
2. 扫描子节点 `Icon*` 写贴图（贴图相同，看不出变化）。
3. `CallDeferred` 补写（解决时序，但图本身没变）。
4. 接管 `NCharacterSelectButton` 的 `RefreshState/RefreshOutline/RefreshPlayerIcons`（postfix 里重写，仍是同一张图）。
5. 让运行的包自己处理点击（包只换背景，不重画按钮头像）。
6. 反射 `PortraitSync.Texture(stage, selection)`：类型枚举可能因不可加载类型失败。
7. 反射 `Palette.PortraitTexture(stage, selection)`：当前实现已接上，但界面仍未变化。

### 下一步需要的一个判定

**把 STS2皮肤切换器 里"点角色切战损"关掉（或用原版 b 单独运行）**，看选人头像是否会随战损变化：
- 会变 → 说明包自己会画，是我们抢先接管把它的绘制挡掉了，那就改成"只在不重画时才由我们兜底"；
- 不会变 → 说明这个功能从来不存在，之前看到的"闪一下"是别的东西（例如右侧大立绘/预览小人），
  那就不要在这条路上继续投入。
## 二十八、1.0.30.153–156：回到"记得档位 + 每 0.2 秒补写"，并消掉闪烁

用户反馈 1.0.30.155"疯狂来回切换"，说明这个方向确实能让头像动起来，只是有两个写者在抢着画。

- 1.0.30.153：恢复"我们接管点击 + 记录档位 + Tick 里每 0.2 秒补写"（就是用户说的"上上次"那套）。
- 1.0.30.154：加 `CharSelectStageIconHoldPatch`，冻结游戏自身的 `RefreshPlayerIcon*`（staged 时不执行原逻辑）。
- 1.0.30.155：补写改用包自己的 `Palette.PortraitTexture(stage, true)`（各档真正的合成图）。
- 1.0.30.156：**给合成图加缓存**。闪烁根因是 `Palette.PortraitTexture` 每次调用都可能返回新的
  `ImageTexture` 实例，每 0.2 秒换一次实例 → 视觉上疯狂闪；缓存后同一档位复用同一实例。

若 1.0.30.156 仍不显示各档差异，说明 `Palette.PortraitTexture` 在该状态下返回的是同一张图，
下一步要读它的返回路径（`palette/s<stage>_<kind>/` 页 + 着色器）或干脆复用包在右侧大立绘上的做法。
## 二十九、1.0.30.157：按用户要求回退到"选人头像实验"之前

回退范围：撤销 1.0.30.139–156 全部"选人头像跟战损"的实验（`StageByEntry` / `ReassertStageIcon` /
`CharSelectStageIconHoldPatch` / `PackagePalettePortrait` / `CharSelectButtonStageIconPatch` /
`ReplaceCharacterSelectTexture` 里的档位分支）。`Ui/CharSelectStageCycle.cs` 整体恢复为 1.0.30.134 的版本。

保留（都属于"实验之前"已经验证过的内容）：
- 战损点击的拦截层挂在 `AnimatedBg`、判档/取档从 PCK 直读、点角色切战损本身；
- 变体切换（背景/预览/局内模型）与 overlay 的家族缓存；
- 局内左上角头像的变体绘制（`VariantPortraits.Paint`）；
- 爆衣归属、门控收紧、最全包执行等。

顺手做的清理与通用化：
- `ProviderMenuGateCompat` 的两处"是不是我"判断改用 `IsPackageSelected`（按分组扫全部选择，不再取第一个）。
- 删除实验期遗留的类型与调用（`Entry.cs` 里两个 `Install` 调用等）。
- 失败尝试的原因整理见第二十七、二十八节，避免以后重走。

已知未解决：选人头像不随战损档变化（与实验前一致）。若要再战，起点应是"包里 `Palette.PortraitTexture`
各档实际返回什么"，而不是继续在按钮节点上写贴图。