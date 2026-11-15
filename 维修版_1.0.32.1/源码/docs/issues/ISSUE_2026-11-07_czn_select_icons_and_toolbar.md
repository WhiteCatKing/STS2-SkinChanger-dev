# CZN 皮肤：选人栏头像 / 顶栏头像仍是原皮；背景切换按钮消失

- 状态：**已修复**（1.0.27.84 工具条、1.0.27.90 头像，待实机验证）
- 涉及：`MeirinWatcherSkin`（工坊 3789216478，Chaos Zero Nightmare - Mei Lin）+ `CznCore`（3826011578）

> 历史：这两项在 2026-11-07 夜曾与一次写文件事故混在 1.0.27.83 里一起做，
> 事故后按"只恢复到损毁前一步"回退到 1.0.27.82，两项均未保留。
> 现在在干净的 1.0.27.82 树上重做，**一项一次构建**。

## 现象

选人背景已经能跟着皮肤走，但：

1. 选人界面**下方角色按钮上的头像**、**左上角（顶栏）头像**仍是原版观者；
2. 皮肤包本该有**两套/三套背景**（静态立绘 / 动态立绘 / 直播大厅 LiveLobby）可切换，
   但界面上找不到切换入口。

## 原因一：我们的贴图接管把 CznCore 的头像覆盖回原版 → 1.0.27.83

CznCore 用**后置补丁**接管 `CharacterModel.get_CharacterSelectIcon`（选人栏头像）
和 `get_IconTexture`（顶栏头像），返回皮肤 profile 里的 `character_icon_meirin.png`。

皮肤切换器在同一 getter 上也有一个 `Priority.Last` 的后置补丁，原本依赖
`ShouldDeferCharacterIconResourceToExternalRuntime` 让位给外部运行时；但那个判断要求
**选项带有 runtime image**（`GetRuntimeImagePath != null`）。CZN 系是 DLL/框架型皮肤，
没有这个字段 → 判断为 false → 我们用"规范路径的原始贴图"覆盖了 CznCore 的结果，于是原皮。

修复（`Core/SkinService.cs`）：

```csharp
return Catalog.IsRuntimeProviderOption(groupId, visualSelection) &&
       !Catalog.IsResourceBackedOption(groupId, visualSelection);
```

即"非资源型运行时提供者"一律让位给它的运行时头像。资源型皮肤（自带 PCK 资源）行为不变。

## 原因二：我们把皮肤包的选人工具条清掉了 → 1.0.27.84

CznCore 的切换控件是一个挂在选人界面的工具条，节点名以 `CharacterSelectOptionsPanel` 结尾
（`CharacterSelectPresentationRuntime.ToolbarPanelNodeNameSuffix`），日志里对应
`Character-select options menu attached: owner=meirin`。

皮肤切换器重建选人界面时会"清理上一位提供者残留"，
`RemoveStaleProviderCharacterSelectControls` 把所有以该后缀结尾的节点都删掉——
**包括刚挂上的这一份**，于是切换按钮消失，只剩默认背景。

修复（`Ui/ContextualSkinControls.cs`）：方法增加 `character` 参数，当前角色选中的是
运行时提供者时直接跳过清理（它们自己管理工具条生命周期），只对资源型皮肤继续清理。

## 怎么切换背景（修复后）

选人界面选中观者（Mei Lin）后，CznCore 会在选人界面挂出它自己的选项条：
`静态立绘 / 动态立绘 / 直播大厅（LiveLobby）` 三种背景可切换；另外
**CZN 设置页**（ModConfig 里的 CZN 分页）也有对应开关。
修复前这个选项条被我们删掉，所以"找不到切换"。

## 验证

1. 选人界面选 Mei Lin：下方按钮头像、左上角头像应为 Mei Lin 图标；
2. 选人界面应出现 CznCore 的选项条，可在几种背景之间切换，切换后背景立即更换；
3. 切到别的角色再切回来：头像/背景正确，无上一个皮肤的残留；
4. 其它皮肤（贞德/鲸鱼/商人）表现不变。

若头像仍是原版：说明不是"让位"判断的问题，回退到 1.0.27.83 之前的形态再查
CznCore 补丁与我们的补丁的 Harmony 优先级顺序。
若工具条仍不出现：回退到 1.0.27.83 单独验证第三项，确认是清理逻辑还是显示层级问题。

---

## 补充（1.0.27.89）：.83 的判据不对，改成"看它有没有这个文件"

.83 之后实测：**头像/图标仍然全是原版**（选人栏头像、顶栏头像、小头像、地图小图标），
只有预览小人和选人背景图跟着皮肤走。

用离线工具查出了关键事实：

```text
dotnet run ... -- --audit-provider-details <game.pck> <Meirin 的 mod 目录>
MeirinWatcherSkin: pack=626 served=202 groups=[]

dotnet run ... -- --audit-character-catalog <game.pck> MeirinWatcherSkin.json watcher
MeirinWatcherSkin 角色归属错误：实际 []，预期 [watcher]
```

也就是说：**皮肤切换器的目录里，Meirin 这个包一个角色组都没认领、独立图片=0**
（`已隔离皮肤提供者 … 独立图片=0`）。它的头像、地图图标**全部由它自己的 DLL 打 getter 补丁**提供：

- Meirin 自己：`get_CharacterSelectIcon`、`get_IconTexture`、`get_IconOutlineTexture`、`get_Icon`
- CznCore（框架）：`get_MapMarker` → `CharacterSkinModBootstrap.ApplyCharacterMapMarker`

而我们的四个 `Priority.Last` 后置补丁（`CharacterSelectIcon` / `CharacterSelectLockedIcon` /
`IconTexture` / `IconOutlineTexture` / `Icon` / `MapMarker`）跑在它们**之后**，按"规范路径"去取贴图：
我们目录里没有这个文件 → 取到的是**游戏原版贴图** → 把 CznCore/Meirin 刚设好的值覆盖掉。

.83 的判据（`IsRuntimeProviderOption && !IsResourceBackedOption`）之所以没用：Meirin 的选项在目录里
**是有资源的**（206 个左右），`IsResourceBackedOption` 为 true，于是判据不成立、照样覆盖。

**新判据**（`SkinService.ShouldDeferCharacterIconResourceToExternalRuntime`）：

```csharp
if (!Catalog.IsRuntimeProviderOption(groupId, visualSelection)) return false; // 纯资源皮肤照旧
if (!Catalog.IsResourceBackedOption(groupId, visualSelection)) return true;   // 纯代码皮肤全让位
// 运行时提供者自带部分资源时：只对它没带的这个文件让位
return !Catalog.SelectedOptionProvidesResource(groupId, visualSelection, resourcePath);
```

新增 `SkinCatalog.SelectedOptionProvidesResource(groupId, optionId, resourcePath)`：
查该选项的 `Assets` 里有没有这个规范化路径。有 → 我们接管（原有行为）；没有 → **不动结果**，
让提供者自己的补丁生效。

这样：
- Meirin / CZN 这类"代码提供头像"的皮肤 —— 我们让位，头像/顶栏/地图图标由它们给；
- 普通资源皮肤（自带 `char_select_*.png` 等）—— 照旧由我们接管，不受影响。

### 验证

1. 选人界面选 Mei Lin：下方角色按钮头像 = Mei Lin；
2. **进入游戏后左上角顶栏头像** = Mei Lin；
3. 大地图上走动的小人图标 = Mei Lin（CznCore 的 `get_MapMarker`）；
4. 选人界面预览小人、选人背景仍正常；
5. 换一个普通资源皮肤（自带头像的工坊皮肤）：头像仍由皮肤切换器接管，不能退化成原版。
---

## 补充（1.0.27.90）：真相是「我们把提供者的头像补丁摘掉了」

作者提供了「关闭皮肤切换器」那次的日志做对照（第 363 行
`Skipping loading mod Gurio.SkinChanger, it is set to disabled in settings`）。对照结果一针见血：

| 日志 | 关闭本 Mod | 开启本 Mod |
| --- | --- | --- |
| `Applied Meirin character-select icon` | **有**（1 次） | **0 次** |
| `Applied Meirin top-bar character icon` | **有**（1 次） | **0 次** |

也就是说：开着我们的时候，Meirin 的头像补丁**根本没跑**——不是被我们覆盖，是被我们**摘掉**了。

原因在 `ManagedSkinModLoader`：提供者激活后，会把它的补丁分成两类，
`IsCharacterPresentationTarget` 认为"CharacterModel 上任何名字带 Icon/Texture/Visual/MapMarker… 的 get_*"
都归本 Mod 接管，于是 `managedPatches` 被 `UnpatchProviderCallbacks` 摘掉（日志里的那条
`8 个资源/选角呈现入口已交由本 Mod 接管`）。可我们的目录里 Meirin **一个资源都没有**
（`--audit-character-catalog` → 实际 []，`独立图片=0`），摘掉之后没人能供应 → 原版。

这同时解释了三个位置的表现差异：

- **#3 地图小图标**：由 **CznCore.dll** 打补丁，CznCore 是依赖库、不是"提供者程序集"，
  所以从来不在被摘的名单里；1.0.27.89 让我们的 `get_MapMarker` 后置补丁"无资源就让位"后，它就能显示 → 修好了；
- **#1 选人下方头像 / #2 顶栏头像**：由 **MeirinWatcherSkin.dll** 自己打补丁 → 被我们摘掉 → 一直是原版。

### 修法（两处）

1. `ManagedSkinModLoader`：`IsCharacterPresentationTarget` 不再认领这六个身份头像 getter
   （`get_CharacterSelectIcon` / `get_CharacterSelectLockedIcon` / `get_IconTexture` /
   `get_IconOutlineTexture` / `get_Icon` / `get_MapMarker`），让提供者自己的补丁留着；
   它们仍然算"行为补丁"，所以切走时照样会被卸载，不会串到别的角色。
2. `SkinService.SelectedSkinProvidesResource` + 三个接管入口（`ReplaceCharacterSelectTexture` /
   `ReplaceCharacterIcon` / `ReplaceCharacterTexture` / `TryLoadManagedCharacterIconTexture`）：
   **只有当前皮肤真的带了这张图，我们才接管**；没有就静默让位，不再"加载规范路径（=原版）"再覆盖。

### 验证（重启后）

日志里应重新出现 `Applied Meirin character-select icon` / `Applied Meirin top-bar character icon`；
选人下方头像、顶栏头像、地图小人图标、局内小头像都应是 Mei Lin。

### 仍未解决的上游问题（留档）

`框架皮肤注册统计（MeirinWatcherSkin）：角色=0，皮肤=0：[]` —— 本 Mod 的框架扫描没把 Meirin 认领到
`watcher` 组，所以该组的选择一直是 `__base__`（`分组 watcher 的皮肤 MeirinWatcherSkin 被 __base__ 接管，退回原版`）。
现在靠"让位给提供者"绕过了它；若要更彻底，需要让框架注册识别这类 `CharacterSkinProfile` 声明。