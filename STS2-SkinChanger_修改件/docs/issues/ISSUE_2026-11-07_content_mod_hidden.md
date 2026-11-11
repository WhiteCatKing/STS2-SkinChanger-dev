# 内容 Mod（如「杀戮尖塔2晓夫九」）被当成外观包隔离，整包像被隐藏

- 状态：**未解决**（1.0.27.71 已修复，待实机验证）
- 记录日期：2026-11-07
- 反馈来源：作者本人（工坊条目 3796187683 `kyxiaofujiu`）

## 现象

订阅并安装 `杀戮尖塔2晓夫九`（新增卡池/角色的内容 Mod，manifest 里 `affects_gameplay: false`）后，
装了皮肤切换器就"像被隐藏了一样"：新增内容的美术/场景资源不生效。
日志（`godot.log` 977 行）：

```
[SkinChanger] 已隔离皮肤提供者 杀戮尖塔2晓夫九：视觉组=0, 卡图=88, 卡牌呈现=0, 独立图片=0,
场景脚本=175, 交互场景=否；原 PCK 未全局挂载；…
[SkinChanger] 已移除 187 个提供者皮肤呈现补丁…kyxiaofujiu=4…
```

## 根因

皮肤切换器把「**非 gameplay 声明 + 有可扫描的美术资源**」的包当成皮肤提供者，接管后**不全局挂载它的 PCK**。
而 `kyxiaofujiu.dll` 实际定义了新的内容模型（`XiaofujiuCardPool : CardPoolModel`、卡牌/角色补丁），
它的 88 张卡图是**给自己新增卡牌用的**，不是给原版卡牌换皮用的。于是：

1. PCK 不挂载 → 新卡池的卡图、场景、脚本资源全部缺失；
2. 它还被列入"移除提供者皮肤呈现补丁"，作者自己的 4 个补丁被卸掉；
3. manifest 写的是 `affects_gameplay: false`（作者只当它是附加内容），所以没走"内容 Mod 原样加载"的分支。

## 修复（1.0.27.71）

新增 `Core/ProviderGameplayContentPolicy.cs`：**只读元数据**扫描提供者 DLL，若其中定义了
`CardPoolModel` / `CardModel` / `CharacterModel` / `MonsterModel` / `RelicModel` / `RelicPoolModel` /
`PotionModel` / `PotionPoolModel` / `EventModel` / `ActModel` / `EncounterModel` / `OrbModel` /
`EnchantmentModel` / `AfflictionModel` / `ModifierModel` 的子类，就判定它是**内容 Mod**，不是外观包。

`Catalog/SkinCatalog.cs` `ProbeSkinProviders` 里，非 gameplay 的包在进入外观探测前先做这个判断：
命中就**不建立提供者**（`continue`），于是皮肤切换器不拦截它，游戏的加载器按作者原意全局挂载 DLL+PCK，
日志给出一条：

```
未按皮肤接管 杀戮尖塔2晓夫九：其 DLL 定义了新的游戏内容模型（卡池/角色/怪物等），
属于内容 Mod 而非外观包；已交回游戏原加载器，DLL 与 PCK 按作者原意全局加载。
```

框架宿主（BaseLib / RitsuLib 等）排除在外，仍走各自的兼容路径；`affects_gameplay: true` 的包本来就不进
外观探测，不受影响。

## 离线验证

`RuntimeTests --audit-provider-content <mod根目录> <id>`：

```
definesNewContent=True   kyxiaofujiu
definesNewContent=False  CrusaderSkin
definesNewContent=False  JoanofArcIroncladHpwear
definesNewContent=False  Merchant2CuteII
```

`--audit-provider-content-all <mods;工坊目录>` 扫全部已装 Mod：命中的 13 个里只有 `kyxiaofujiu`
（和 BaseLib）声明 `affects_gameplay: false`，即本次改动只影响它，其它内容 Mod 行为不变。

## 实机验证

1. 启动游戏，日志中 `杀戮尖塔2晓夫九` 不再出现 `已隔离皮肤提供者`，改为上面那条"未按皮肤接管"。
2. 该 Mod 的新卡池/卡图、场景资源正常（与不装皮肤切换器时一致）。
3. 皮肤切换器的下拉里不再出现它的"卡图"条目（它本来就不该被当成皮肤）。
4. 其它皮肤包（CrusaderSkin / 贞德 / 商人等）的下拉与之前一致。

## 已知边界

- 只看 DLL 是否**定义**内容模型子类；如果某内容 Mod 把新内容全部放在另一个依赖 Mod 里、
  自己只带资源，本判断识别不到（此时仍会按外观包处理）。
- 反过来，如果某个纯外观包也定义了模型子类（极少见），它会改为全局加载，不再作为皮肤出现在下拉里。
