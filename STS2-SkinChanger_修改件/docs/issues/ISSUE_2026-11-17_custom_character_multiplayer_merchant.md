# 第三方角色联机商店类型边界

玩家转述 InvalidCastException: Godot.Node2D -> NMerchantCharacter，堆栈指向 MultiplayerMerchantPlayerVisualIsolationPatch.Prefix。没有原始日志和玩家模组文件；“房主重开”“ManagedNetActionCarrier commands”等片段不能认定与此异常同源。

核对本地代码确实在替代 AfterRoomIsLoaded 整段循环时调用 Instantiate<NMerchantCharacter>。官方循环本身也使用该类型，因此不能仅改成吞掉异常或伪造 NMerchantCharacter 包装；这样可能再次出错或破坏作者自定义角色初始化。

修复：在任何顺序调整、资源作用域和实例创建前检查队伍 CharacterModel 的程序集来源，含第三方角色的队伍返回 true，保留游戏原方法及其他 Mod 在该方法上的兼容补丁。全原版角色的联机队伍继续使用原外观隔离。TryRefreshLocalPlayer 对第三方角色同步返回受控不支持，避免后续再次强转。此策略意味着混合队伍不再由本替代循环保证逐玩家外观隔离，这是保留自定义角色创建合同的边界。

--test-custom-merchant-boundary 使用真实 Player、原版角色和第三方 CharacterModel 测试替身，验证两种玩家次序与三人混合队伍、无场景依赖时原 Prefix 直接放行且不改顺序、原版保留隔离、热切换避让。--test-merchant-lifecycle 通过；check-protected-characters.ps1 全通过；Release 零警告零错误。未运行游戏；没有证明天音本体和全部第三方补丁组合的原生流程已无其他错误。
