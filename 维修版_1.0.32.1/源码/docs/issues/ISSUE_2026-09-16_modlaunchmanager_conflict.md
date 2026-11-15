# 与 ModLaunchManager 启动冲突

- 状态：**已解决**
- 来源：留言 ZiRAN_ice 2026-09-16（“今天运行的时候显示和模组管理器（ModLaunchManager）冲突了”）
- 收录：2026-11-03

## 现象

同时启用 SkinChanger 与 ModLaunchManager 时启动报冲突；具体报错文本/日志未提供。

## 初步判断

两者都可能改 Mod 加载顺序、清单或 Harmony 补丁：

- SkinChanger 的加载顺序询问/F35 逻辑会移动自身顺序；
- ModLaunchManager 若也重排/重写 mod 清单，可能互相覆盖；
- 也可能是程序集/类型发现阶段的冲突。

## 下一步

1. 向反馈者要完整 `godot.log` 与 Mod 列表（客观需样本）。
2. 本地同时装两个 Mod 复现，确认是加载顺序、清单写入还是补丁冲突。
3. 若只是顺序问题，考虑在检测到 ModLaunchManager 时跳过自动排序。

## 解决

- 版本：1.0.24（2026-11-03）
- 检测到 MLM 时，把 SkinChanger 排在 MLM 之后，顺序为 `MLM → SkinChanger → 其他`；
- 点击“重启”时，按游戏刚保存的顺序同步 MLM 自己的 `mod_launch_last_plan.json` 启动计划数据；
- 未修改 MLM 的 DLL、代码或清单；未检测到 MLM 时该逻辑完全休眠；
- 实机验证：白猫 2026-11-03 确认排序与重启流程正常。

## 关联

- 与 F17（baselib/RitsuLib 框架启动冲突）不是同一组，F17 已定案并加固。
