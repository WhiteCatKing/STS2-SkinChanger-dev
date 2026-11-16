# 狐猎背景点击和同名项目

当前本地仅一份 KaguyaSilentMSGKSkin（皮肤/猎宝/狐猎nsfw）。配置另有 composition:20c0cd192c7d4ace83351431d72f9682，名字相同，SourceOptionIds 只有原皮肤，HideSources=false，因此原皮肤和合并项目同时显示。未修改用户配置。

狐猎血量换装 1.0.98 的 SilentCharSelectToggle.Postfix(NCharacterSelectScreen, CharacterModel characterModel) 挂在新版 SelectCharacter(NCharacterSelectButton) 上，Harmony 无法绑定 characterModel；实际日志记录 patch failed。参数为空的 TryReattachFromScreen 实际为空实现，既有重挂桥只接受屏幕参数，也无法补偿。它使用 battlewear/silent_kaguya 下 cs_stage 前缀、7 档，并不符合贞德 s1/s2/s3 资源合同。

在既有 AfterRebuild 调用处，只对当前激活且确认为 silentKaguyaHpWear 的提供者，取得当前 silent_bg，调用实际 BattleWear.CharSelectCore.Attach(Control,CsProfile) 和其 Silent profile。保留作者菜单选择检查、七档状态、头部交互，不扩展或修改贞德资源合同。身份判断使用激活程序集，支持带来源后缀的提供者身份；不加载未选中提供者。

--test-fox-char-select 使用真实 DLL 解析并拦截实际 Attach 调用，验证当前背景与原生 profile 的传递、七档与无关程序集排除。check-protected-characters.ps1 全部通过。Release 构建通过。没有运行游戏，最终点击画面未实机验证。
