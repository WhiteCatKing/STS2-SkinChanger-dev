# 火堆场景观者皮肤回退原版

- 状态：**未解决**
- 来源：Steam 工坊讨论区，2026-09-17，抗拒从宽
- 链接：https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292648589/
- 收录：2026-11-03

## 现象

使用 `Chaos Zero Nightmare - Mei Lin (Watcher Skin)` 时，火堆（休息处）显示不对，变成原版观者。

## 初步判断

- 该皮肤可能只提供了战斗场景的 Spine 资源，火堆场景仍走原版 `rest_site` 路径；
- 也可能是 SkinChanger 的场景组映射/覆盖包没有包含火堆场景，或 provider 清单没有声明 `rest_site` 组；
- 与 1.0.20 的共享骨骼/场景重建逻辑可能有关，但报告时间早于 1.0.20。

## 下一步

1. 取该皮肤包，确认 `rest_site` 相关 `.tres/.skel/.atlas` 是否存在、路径是否与游戏一致。
2. 看 provider 清单/目录扫描是否把火堆资源归组；必要时补场景映射。
3. 用该皮肤实机进火堆验证。

## 关联

- 同一讨论串里的欧洛巴斯问题已定案为 RitsuLib 补丁（见 `COMPLETED_FIXED.md`），本条是另一个问题。
