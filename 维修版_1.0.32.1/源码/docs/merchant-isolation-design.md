# 商人皮肤模块：隔离重建设计（v1）

## 目标

商人（含假商人）作为一个**完全独立**的模块对待：注册、检测、激活、停用四条路径都只服务「商人这个人」的皮肤，
不参与角色/怪物那套分组继承与场景重建；资源按官方路径组织。

第一条验收：**选中 c（ATA_Merchant）时一切正常**（选人界面下拉能选、进商店显示阿塔、局内切换不炸）。

## 以 c 为蓝本的实测事实（2026-11-14）

`D:\steam\steamapps\workshop\content\2868840\3747990002`：

```
ATA_Merchant.dll    62,976 B
ATA_Merchant.pck    31,196,524 B
mod_manifest.json   {"id":"ATA_Merchant","pck_name":"ATA_Merchant","name":"商人“阿塔”",
                     "version":"0.1.8","has_pck":true,"has_dll":true,"min_game_version":"0.103.2"}
```

pck 里的商人资源**不在官方路径**，是它自己的命名空间：

```
res://ATA_Merchant/spine/merchant/npc.{atlas,skel,tres}     ← 商人本体骨架
res://ATA_Merchant/spine/merchant/hand.{atlas,skel,tres}    ← 商人手部
res://.godot/imported/shop_merchant_bottom.png-….s3tc.ctex  ← 只引用了官方背景图
```

DLL 侧（UTF-16 字符串）：`ATA_Merchant.patches`、`%MerchantVisual`、`_merchantDialogue`、`_merchantRug`、
`ATA_MERCHANT.talk.*`、`event:/sfx/npcs/merchant/ata_merchant/*`。

结论：**c 完全靠自己**——pck 提供自己的骨架，DLL 补丁改 `%MerchantVisual` 的 `skeleton_data_res` 并写位姿。
它不碰官方场景、不需要我们重放任何前缀。

## 三条既有设计错误（本次要彻底拆掉）

1. **重建节点**：为修「SL 后显示旧皮肤」，刷新路径改成新建 `standard_merchant_room.tscn`，
   而 c 的 pck 没有该路径 → 拿到原版场景 → c 变原版商人。
2. **重放节点就绪前缀**：为拿 a/b 的位姿，我们反射重放它们 `_Ready` 前缀；一旦同时让包自己的补丁生效，
   同一节点双份执行 → 商店进不去。
3. **场景收窄会停用商人群组的提供者**：商人群组不在角色可见集合里，刚激活就被 `DeactivateProvidersExcept` 停掉，
   导致包补丁从没真正装上（日志里只有 a 有 `Harmony patches` 行）。

## 新模块结构（`Core\MerchantSkinModule.cs` + `Ui\MerchantSkinPresenter.cs`）

**注册**（学角色/怪物那套：只认包自述 + 资源证据，不猜路径）

- 提供者资格：manifest 有 `has_dll` 或 `has_pck`，且满足任一条
  - pck 内存在 `*merchant*` 命名的骨架/tres（如 `ATA_Merchant/spine/merchant/npc.tres`）；
  - DLL 字符串含 `%MerchantVisual` / `MerchantButton` / `NMerchantRoom` 之一。
- 分组：固定 `merchant`（假商人若声明，归 `fake_merchant` 子组，二者互不继承）。
- 记录 `MerchantProvider { Id, DisplayName, Pck, Dll, Evidence }`，日志一行：`[商人注册] id=… 证据=…`。

**检测**（下拉里怎么出现）

- 只在 `merchant` 分组下列出；不参与其它分组的候选池，也不从其它分组的选项里被“继承”进来。
- 选择值就是 `MerchantProvider.Id`（必要时带 `::source:<workshopId>`），与现有配置格式兼容。

**激活**（选中时）

1. 挂 pck（完整资源包，不生成覆盖包）；
2. `ActivateProvider(id)`：**让它的 DLL 补丁真正生效并一直保持**（钉住，不做场景收窄）；
3. 不重放任何 `_Ready` 前缀、不写位姿、不重建节点；
4. 日志：`[商人激活] id=… 补丁=… pck=…`。

**停用**（切走时）

1. `UnpatchAll` 该程序集 → 恢复节点原生行为；
2. 卸载 pck；
3. 让官方 `NMerchantRoom._Ready` 重新跑一次资源刷新（只换资源、不建节点）。

**刷新（SL / 重进房间）**

- 只做「同一节点重新套一次资源」：`ScopePartTakeover(room)` + `TryRefreshMerchant`；
- **禁止**新建 `NMerchantButton`；若 `%MerchantVisual` 实例无效，只记录日志，交给包自己的补丁处理。

## 迁移步骤

1. 新增 `MerchantSkinModule`（注册/检测/激活/停用），先只接到 `merchant` 分组，开关 `MerchantModuleEnabled = true`；
2. `MerchantRuntimeAppearance` 只保留 `ScopePartTakeover` / `TryRefreshMerchant`（纯资源替换），删除重建、重放、位姿记忆、看门狗；
3. 删除 1.0.31.51+ 的 `IsMerchantFacingPatch` / 钉住逻辑；`VisualPatchGuard` 的摘补丁范围改为「我们接管的程序集」；
4. 用 c 验收：下拉出现「商人“阿塔”」→ 进商店显示阿塔 → 局内切 a/b/c 均正常；
5. 假商人同结构复用到 `fake_merchant` 子组。

## 备份

改动前的商人相关源码已备份到 `D:\Storage\Desktop\工作2\STS2_Work\merchant_backup_20261114\`（保留原相对路径，16 个文件）。
