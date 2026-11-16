# 奶蛙纹理错配与单图预览叠影

用户反馈 1.0.33.14 奶蛙菜单和战斗均呈现变形原皮；单图预览有原版身体透出。

根因一：奶蛙替换 fool.skel 和 fool.png，但继续复用游戏的 fool.atlas。IncludeAliasedDependencyChain 的 GetSiblingAtlasTextureAssets 用图集所属的游戏索引寻找页面，抢先把原版 fool.png 标为已发现；后续队列无法再选奶蛙页面。实际资源图包含奶蛙骨架，却只含原版纹理。修复在加入图集兄弟页面前，按相同逻辑页面路径先解析当前选择，再回退图集所属索引，保持 requiresAliasedLocation=true 的隔离约束。

新增真实包断言在修复前失败（Real Naiwa body texture must reach the isolated scene graph）。修复后战斗模型图必须含原包 CTEX 的逐字节内容，并用奶蛙私有纹理源别名而非原版纹理别名；菜单背景图也验证对应真实 CTEX，默认模型图不得包含奶蛙纹理。

根因二：IndependentFoolSkinCompat.ApplyPreview 在 CreateVisuals 后、场景进入树前调用。NCreatureVisuals.Body 在 Ready 才赋值，所以隐藏时可能为空。将 ApplyPreview 移到 AddChild、ApplyRuntimeSpine、作者呈现收尾、slot binding 之后，捕捉预览之前。保留仅所选静态皮肤执行的门槛与预览节点所有权，不重放战斗 Ready。

回归增加共享预览 IL 调用时序断言，确保模型就绪收尾先于单图替换、单图替换先于 BeginCapture。--test-independent-fool-skins 与 check-protected-characters.ps1 全通过；Release 零警告零错误。没有控制游戏，最终渲染仍未实机验证。
