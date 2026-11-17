# 1.0.34.1：局内切换与极速弩手选角点击

## 反馈与证据

用户 godot.log 在 CharacterAppearanceScreen.EnumerateSelectableTargets().MoveNext 中反复报 BadImageFormatException: Bad IL format，导致局内切换界面的提示、点击和下一目标流程中断。另有新房间商人初始化错误：NMerchantButton 不能写入 NBlitzMerchantButton 字段。用户同时反馈极速模式 CrossbowFool（皮肤/愚者/a大愚者弩手可战损版）点击切换失效。

## 根因与修改

- 正式版 NMerchantRoom.MerchantButton 是具体属性；测试版变成抽象属性。C# 对 null-conditional 的已判非空接收者生成 `call` 而非 `callvirt`，故同一 DLL 在测试版实际走到调用时试图编译抽象方法，报 Bad IL。先前 ABI 检查只识别缺失签名，未识别这一语义差异，单纯 PrepareMethod 也没有检出运行期编译抽象方法的问题。
- 旧 1.0.34 DLL 在新增检查中明确检出三处：枚举目标一处，RequestSkinSelection 成功/回滚刷新两处。改由 GameBranchApi.MerchantButton 显式虚派发，保留极速房间的覆盖方法。
- 商人重建此前始终实例化普通商店场景。极速房间私有字段要求 NBlitzMerchantButton；现在按实际房间类型使用 blitz_merchant_room.tscn，假商人与标准场景继续各用自己的场景。保留库存、回滚和初始化时序。
- CrossbowFool.Selection.Bind 的参数限定标准 NCharacterSelectScreen，其 Harmony 入口也只挂标准 SelectCharacter。新增独立 CrossbowFoolSelectCompat，仅在极速/恐惧选角并且当前选择的活动提供者真实匹配 CrossbowFool 合同时工作；不修改第三方 DLL/PCK，不创建伪标准选角界面。
- 重用作者 SkinStages.Find/Get、SelectionParts.Create、Selection.FitArt、WearSettings.Current.Enabled 与原 GDScript sync_enabled；保持作者分部位点击、过渡和设置。不自行推导战损档位。背景重建或模型更换时重绑，重复检查复用同一 catcher；切换其他皮肤/隐藏背景时取消旧 catcher 输入，异常后停止观察避免刷日志。
- 通过既有两个选角背景重建完成入口挂接；标准选角继续由作者处理。

## 验证

- 使用当前 beta 对应快照与正式版 v0.107.1 快照，同一正式版引用编译 DLL 通过 check-dual-branch.ps1：ABI、选角模式、运行安全、保护边界、预览绑定、商店生命周期、真实旧鸡煲包。
- GameBranchCompatibilityTests 增加实际发布 IL 审计，拒绝对原生抽象方法的非虚 call；旧 1.0.34 发布 DLL 负对照失败，修复后两分支通过。
- 额外验证适配方法保留 callvirt；通过 Harmony 截取真实具体房间 getter，验证虚派发返回其结果及空房间处理；核对极速房间实际字段类型与场景路径。
- --test-crossbow-select 使用真实 CrossbowFool.dll，两分支各验证精确合同、200 次作者 catcher 工厂/布局调用的对象与顺序、极速/恐惧接入、标准/无关界面排除，以及真实嵌入点击脚本合同。
- check-protected-characters.ps1 完整通过，包含实际十字军、贞德 13 变体、调色页/战损与资源预算等回归。
- 最终构建 0 警告 0 错误。没有启动、控制游戏或实机验证界面；GDScript 的最终命中区域与完整联机组合未离线覆盖。

内部版本 1.0.34.1，公开仍 1.0.34；部署本地四处并推 Git，本次不上传工坊。
