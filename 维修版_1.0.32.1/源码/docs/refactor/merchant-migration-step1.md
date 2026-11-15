# 商人模块迁移第 1 步

GPT，2026-11-14。基于 1.0.31.96 工作区，HEAD 为 `01eca75`。已完成模块注册、选项判定、在用检测、四个补丁类接入、三个延迟执行点的代次核对，以及 GameApi 第一批搬移。

## 交付与验证

主工程 Release 构建通过，0 错误、8 个既有警告。LogicTests 构建 0 错误、0 警告，完整运行退出码 0，末行 `Skin Changer logic policy tests passed.`，新增用例也输出 `Merchant feature policy tests passed.`。

构建输出位于 `C:\Users\yuinn\AppData\Local\Temp\sts2-merchant-step1-9c8aba8f69444061b476eb391ee036e5`，主工程在 `mod/`，测试在 `logic/`，测试运行记录为 `logic-results.txt`。两次构建均显式传入任务书要求的 GameAssemblyDir 并用 `-o` 指向临时目录。

本步的实现选择是先迁移预热与 NPC 子节点 Ready 桥。预览玩法保护、房间 scope 和玩家外观混合的类继续由主干安装。具体房间 Ready 仍走已有 Entry 手动安装路径，接线理由见下表，交 Claude 复核。

## 文件

| 文件 | 改动 |
| --- | --- |
| `STS2SkinChanger/Core/Features/Merchant/MerchantFeature.cs` | 实现 ISkinFeature，Id 为 merchant |
| `STS2SkinChanger/Core/Features/Merchant/MerchantFeaturePolicy.cs` | 从目录分组/选项 ID 快照判断是否需要模块的纯函数 |
| `STS2SkinChanger/Core/Features/FeatureBootstrap.cs:30` | CreateFeatures 仅增加 `yield return new Merchant.MerchantFeature();` 一行 |
| `STS2SkinChanger/Core/GameApi/MerchantRoomApi.cs` | 搬入字段缓存、实例字段解析、房型场景解析和具体房间 Ready 探测 |
| `STS2SkinChanger/Ui/MerchantRuntimeAppearance.cs` | 原 API 调用转接、四个 FeaturePatch 标记、Safe 包裹及延迟代次核对 |
| `tests/STS2SkinChanger.LogicTests/MerchantFeaturePolicyTests.cs` | 正常目录与选项判定用例 |
| LogicTests 的 `.csproj`、`Program.cs` | 各追加一处 policy 引用/用例入口，保留现有测试接线 |

## 激活与在用判定

`MerchantFeature.ShouldBeActive` 将 Catalog.Groups 转为组 ID 和选项 ID 的枚举，交给纯函数。目录未就绪返回 null。merchant 或 fake_merchant_monster 中有非空且不等于 `__base__` 的选项 ID 即返回 true，比较采用 OrdinalIgnoreCase。选项来源是现有目录的 Options，包含纯 PCK 和 UI/风格部件选项。普通角色组的商店外观不会打开 NPC 模块。

`IsInUse` 先检查活的 NMerchantRoom.Instance，再遍历当前 SceneTree 根下的活节点，寻找 NMerchantRoom、NFakeMerchant 或带现有 merchant-preview-root 标记的节点。缺少可用 SceneTree 时返回 true，查询抛错时由注册表的 SafeInUse 按 true 处理。节点判断复用 NodeGuard。该查询随注册表的两秒评估执行。

`OnActivated` 在已有真实商店时调用一次 TryRefreshMerchant，失败只报告现有错误信息。`OnDeactivating` 复用 PrepareMerchantSelectionChange，清理重放新增库存节点并走原有库存呈现恢复路径。

## 补丁类归属

以下四类增加 `[FeaturePatch("merchant")]`，由注册表按需装卸。

| 类 | 职责 |
| --- | --- |
| MerchantRoomPrewarmPatch | 地图打开时预热商人场景 |
| MerchantButtonProviderReadyBridgePatch | 商人按钮 Ready 前后接入 |
| MerchantInventoryProviderReadyBridgePatch | 商人库存 Ready 前后接入 |
| MerchantHandProviderReadyBridgePatch | 商人手部 Ready 前后接入 |

以下类保留主干安装，依据任务书“拿不准的不加，写进交件”的约定记录。

| 类或路径 | 理由 |
| --- | --- |
| MerchantRoomCreateAppearancePatch | 同时建立全局行为范围并登记实例 scope，退出释放由主干持有 |
| MerchantRoomPlayerAppearancePatch | 同时承载玩家模型、角色回放、预览玩法阻断及商人部件对齐 |
| MerchantRoomPreviewEnterTreePatch、FakeMerchantPreviewEnterTreePatch | 原版图鉴预览也依赖这层玩法生命周期阻断，保护按主干持有 |
| MerchantRoomPreviewExitTreePatch、FakeMerchantPreviewExitTreePatch | 包含 ExitTree 保护及实例 scope 收尾 |
| FakeMerchantAppearancePatch | 同时处理预览玩法阻断、事件帐篷资源和全局 scope，保留混合类的当前入口 |
| MerchantPreviewPurchaseBlockPatch | 原版预览也需要购买阻断，且 Prefix 含 ref Task 参数 |
| MerchantRoomConcreteReadyPatch | Entry 显式调用 Install，当前类没有 HarmonyPatch 特性，FeatureBootstrap 的类扫描不会收集该手动回调；本步仅搬探测并包其 Postfix |
| RestSitePlayerAppearancePatch | 休息点的玩家呈现 |
| MultiplayerMerchantPlayerVisualIsolationPatch、ProviderMerchantVariantPathPatch | 联机按玩家隔离和角色变体，属于主干角色路径 |
| FakeMerchantEventFramePatch、FakeMerchantCombatArtPatch、ProviderSettingsModInfoPatch | 事件构图/战斗背景/共享设置入口，沿用各自模块边界 |

后续若要把 concrete Ready 也交给 registry，需要 Claude 将 Entry 的手动安装与注册表的安装/摘除合同对接。若拆混合类，可以分别迁移 NPC 回调和保留玩家、预览基本保护；这些选择已在本步交件中明确，当前四个模块类已可由注册表独立管理。

## Safe 包裹

五个补丁方法使用 `Safe.Run("patch:merchant", …)`，分别为预热 Postfix、手动 concrete Ready Postfix、Button/Inventory/Hand 三个桥的 Postfix。三个 NPC 延迟回调的执行动作也使用同一 key。

三条 Ready Prefix 的 out __state 接口按任务书跳过 lambda 包裹。ExitTree、scope 释放、PrepareMerchantSelectionChange 等收尾沿用原路径。预览生命周期及含玩家逻辑的混合类按上表归属保留。本步没有新增 bool Prefix 包裹。

## 延迟回调代次

| 位置，均在 MerchantRuntimeAppearance.cs | 排队与执行 |
| --- | --- |
| `:241–250` | Ready Postfix 的 CallDeferred，排队时保存 Generation，执行前 IsCurrent |
| `:279–285` | 商人 Spine Ready 回调，保存 Generation，执行前同时检查代次和现有 provider/node 归属 |
| `:670–679` | 预览焦点等待 Spine 的回调，保存 Generation，执行前 IsCurrent |

公共代次检查在 `:300–301`，注册表尚未存在时沿用初始化阶段行为。玩家商店模型与休息点路径保留原调用。

## GameApi 搬移

`MerchantRoomApi.ResolveMerchantButtonField` 保留原实现的实际 Type 缓存、沿基类链依次查 `<MerchantButton>k__BackingField` / `_merchantButton` 的顺序，原位置改为转调。

`ResolveMerchantRoomScenePath` 保留按实际类型名 NBlitzMerchantRoom 选择 blitz，其余选择 standard 的规则。原来的两个场景路径常量转为 API 常量别名。

`ConcreteReadyMethods` 保留 standard/blitz 两个完整类型名的运行时探测及 `_Ready` 方法查找顺序，缺类型或方法继续跳过。UI 中的 Install 仍对返回方法挂原 Postfix。

## 测试覆盖

MerchantFeaturePolicyTests 覆盖目录未就绪、空目录、只有原版/空白选项、真实商人纯资源选项、假商人部件、无关角色组、分组大小写和原版 ID 大小写，共八项断言。注册表既有测试随完整 LogicTests 运行通过。

8 个构建警告来自 CardTransformPreload、VariantHotkeyBridge、关闭增强刷新后的不可达分支、ContextualSkinControls 和 ProviderMenuGateCompat，与既有构建记录相同。定向 git diff --check 通过。

实机步骤由 Claude 编写并交云操作，当前交件依据构建和离线测试。

## 可选、不阻塞

- 无新增项。

## 房间交件

商人模块迁移第 1 步已交付，MerchantFeature/policy、四个 NPC 补丁类、三个延迟回调代次和 MerchantRoomApi 已接入。
Release 构建 0 错误，完整 LogicTests 通过。预览基本保护、混合职责类和 Entry 手动 concrete Ready 的保留理由已列入交件，请 Claude 复核。
交件位于 D:\mod\工作2\STS2-SkinChanger_修改件\docs\refactor\merchant-migration-step1.md
