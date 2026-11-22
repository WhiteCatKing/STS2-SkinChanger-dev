# A/B 构建对照记录模板

> 用于复现并二分定位回归问题（「本版新回归」/「既有老问题」/「第三方包自身行为」）。

## 一、复制用模板

```markdown
### 基本信息
- 日期：YYYY-MM-DD HH:MM
- 游戏分支与版本：正式版 vX.Y.Z / 测试版 vX.Y.Z
- 对应单号 / 房间消息：#<编号>
- 问题描述：

### 构建标识对比
| 项 | 构建 A（基准版） | 构建 B（对照版） |
|---|---|---|
| 内测版本号 |  |  |
| `Gurio.SkinChanger.dll` SHA-256 前 12 位 |  |  |
| 对应日志文件 |  |  |
| 日志特征行 1（启动版本行） |  |  |
| 日志特征行 2（补丁接管/保留计数） |  |  |

> 取 DLL 哈希（PowerShell）：
> `(Get-FileHash "<Mod目录>\Gurio.SkinChanger.dll" -Algorithm SHA256).Hash.Substring(0, 12)`

### 启用的 Mod 清单与加载顺序
> 从当前存档槽位 `settings.save` 导出（注意：游戏按同 ID **最后一行** 生效）：
> ```powershell
> (Get-Content "$env:APPDATA\SlayTheSpire2\steam\<SteamId>\settings.save" -Raw | ConvertFrom-Json).mod_settings.mod_list |
>   Select-Object id, source, enabled | Format-Table -AutoSize
> ```

1. `Gurio.SkinChanger`（期望顺序：首位）
2. ...

### 复现操作步骤（A / B 严格一致）
1. 
2. 
3. 

### A / B 观测结果对比
| 观测点 | 构建 A 表现与日志依据 | 构建 B 表现与日志依据 | 判定归属 |
|---|---|---|---|
| 1.  |  |  | 本版回归 / 老问题 / 第三方 |
| 2.  |  |  |  |

### 结论
- **本版引入回归**：
- **既有老问题 / 第三方行为（划出本单）**：
- **根因 / 后续去向**：
```

---

## 二、填写示例（#259 卡牌总览工具栏消失与日奈图鉴头像回退）

### 基本信息
- 日期：2026-11-21 01:56
- 游戏分支与版本：测试版 v0.113.1
- 对应单号 / 房间消息：`#259` → `#260` / `#262` / `#264`
- 问题描述：`1.0.36.105` 构建打开卡牌总览时皮肤工具栏消失，且选中空崎日奈后愚者图鉴头像回退为游戏默认；同时界面出现日奈局外台词与遗物修改器按钮。

### 构建标识对比
| 项 | 构建 A（#257 基准版） | 构建 B（#259 对照版） |
|---|---|---|
| 内测版本号 | `1.0.36.104` | `1.0.36.105` |
| `Gurio.SkinChanger.dll` SHA-256 前 12 位 | `（保留上一版备份对比）` | `（测试部署版）` |
| 对应日志文件 | `logs/godot*.log`（01:31 归档 / 01:56 运行） | `logs/godot2026-11-21T01.53.12.log` |
| 日志特征行 1（启动版本行） | `[SkinChanger] 内测版本 1.0.36.104...` | `[SkinChanger] 内测版本 1.0.36.105...` |
| 日志特征行 2（日奈补丁计数） | 01:31 L3232 / 01:56 L2750：「4 个资源/选角呈现入口已交由本 Mod 接管，保留 56 个」 | L2774：「5 个资源/选角呈现入口已交由本 Mod 接管，保留 55 个」 |

### 启用的 Mod 清单与加载顺序
1. `Gurio.SkinChanger`（首位）
2. `RitsuLib`（负责在 `NCardLibrary._Ready` 后置同步卡池角色头像，读取 `CharacterModel.IconTexture`）
3. `SorasakiMana`（工坊 `3839527876`，空崎日奈）
4. 遗物修改器 Mod

### 复现操作步骤（A / B 严格一致）
1. 启动游戏并在选角界面为愚者选中空崎日奈角色皮肤，将愚者卡图保持「游戏默认」。
2. 从主菜单进入「图鉴 → 卡牌总览」，切至愚者卡池并观察左下/左侧栏、顶部卡池头像与愚者打击/防御卡面。
3. 退出卡牌总览切回主菜单，观察是否有额外台词与遗物修改器按钮，并保存对应时间点 `godot*.log`。

### A / B 观测结果对比
| 观测点 | 构建 A（`1.0.36.104`）表现与日志 | 构建 B（`1.0.36.105`）表现与日志 | 判定归属 |
|---|---|---|---|
| 1. 卡牌总览皮肤按钮行 +「有皮肤卡牌」筛选框 | 位于左下角（被返回箭头遮挡一部分）；无异常日志 | 两条控件全部消失；`godot2026-11-21T01.53.12.log` L2991/L3235/L4668 抛 `NullReferenceException`（`NLibraryStatTickbox.SetLabel` ← `CardSkinControls.Attach` ← `CardLibrarySkinReadyPatch.Postfix`） | **本版引入回归** |
| 2. 愚者图鉴卡池小头像 | 正常显示日奈光环图标 | 回退为原版愚者图标（同一 `NCardLibrary._Ready` 后置链被上面 `SetLabel` 异常提前截断，下游 RitsuLib 回调未执行；原怀疑多拦 1 个日奈补丁经核查仅为 `CardModel.PortraitPath`） | **本版引入回归**（同链截断并发问题） |
| 3. 日奈局外台词与图鉴遗物修改器按钮 | 主界面/图鉴均有出现 | 与 A 一致，完全相同 | **既有老问题 / 第三方自身行为**（划出本单） |

### 结论
- **本版引入回归**：`CardSkinControls.Attach` 将 `NLibraryStatTickbox` 挂载到尚未入树的 `STS2CardSkinToolbar` 即调用 `SetLabel`，引发 `NullReferenceException` 且外层补丁未做异常隔离，同时吞掉自身工具栏并截断同一 Harmony `_Ready` 后置链上的 RitsuLib 头像同步。
- **既有老问题 / 第三方行为（划出本单）**：日奈局外台词与遗物修改器文字按钮在 A/B 上表现一致，与切换器本次改动无关。
- **根因与修复去向**：`#261` / `#263` 改为先入树再调 `SetLabel`（归位至 `Sidebar/MarginContainer/TopVBox` 顶部），并收窄 `CardModel.PortraitPath` 隔离范围；同时在阶段 1 第二轮通过 `UiInjectionGuard` 统一隔离切换器 UI 注入异常，防止再次截断同链其他 Mod 后置。
