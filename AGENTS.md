# 工作区约定

《杀戮尖塔 2》皮肤切换器（Skin Changer）的本地工作区。唯一可改的源码树是 `STS2-SkinChanger_修改件/`；`STS2-SkinChanger_源文件（坚决不能动）/` 是上游原始快照，只作对照。

## 先读这几份

- [START_HERE.md](START_HERE.md) — 接手入口：路径、日常四步、代码结构、已知坑、当前状态。
- [STS2-SkinChanger_修改件/docs/agents/maintenance-rules.md](STS2-SkinChanger_修改件/docs/agents/maintenance-rules.md) — 维护规矩（不许丢的修复、改完必须做、要停下来问的事）。
- [STS2-SkinChanger_修改件/docs/agents/fix-and-test-workflow.md](STS2-SkinChanger_修改件/docs/agents/fix-and-test-workflow.md) — 修复与测试流程。
- [STS2-SkinChanger_修改件/docs/agents/pitfalls-checklist.md](STS2-SkinChanger_修改件/docs/agents/pitfalls-checklist.md) — 踩坑复盘与按症状排查清单。
- [docs/owner-checklist.md](docs/owner-checklist.md) — 给作者本人的合作注意事项（含「什么时候该叫 AI 停下来」）。
- [CHANGELOG.md](CHANGELOG.md) — 版本变更记录。

## 约束

- 只改与任务相关的文件；不批量改编码与换行（统一 LF）。
- 构建、测试、部署步骤只在源码树里执行，产物部署到四处：两份安装包 / `STS2_Work\workshop_content_1.0.20` / 游戏 `mods\STS2皮肤切换器_mod测试版安装包`。
- 工坊发布直接用「每次修好并打包供本地使用」的同一版本、同一产物，不另起一套发布构建；发布前核对版本号与 `Gurio.SkinChanger.dll`、`thunninoiSkinManager.dll`、`SkinChanger.json` 的 SHA256。
- 工坊改动说明以同一条目上一次实际发布包为起点，核对并写齐到本次发布包之间实际入包的新增、调整和修复。中英双语写在「改动说明」，不放简介。细则见 [maintenance-rules.md](STS2-SkinChanger_修改件/docs/agents/maintenance-rules.md) 的「工坊发布」。
- 不启动游戏做实机测试；游戏内验证由作者本人完成。
- 提交信息用中文。
