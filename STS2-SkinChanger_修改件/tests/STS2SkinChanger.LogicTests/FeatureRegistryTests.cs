using System;
using System.Collections.Generic;
using STS2SkinChanger.Core.Features;

/// <summary>
/// 模块化安装的核心约定：一个模块装失败只回滚它自己，别的模块必须原样保持已安装。
/// 这条规则如果破了，恰好会在「某个第三方框架的补丁目标在某游戏版本缺失」时
/// 把一个模块的失败扩散成整片皮肤功能消失。
/// </summary>
internal static class FeatureRegistryTests
{
    public static void Run()
    {
        var installer = new RecordingInstaller();
        var registry = new FeatureRegistry(installer);

        var characterResult = registry.InstallGroup(
            FeatureIds.Character,
            [typeof(FakeCharacterPatchA), typeof(FakeCharacterPatchB)]);
        Require(characterResult.Installed, "正常模块必须安装成功。");
        Require(installer.Installed.Count == 2, "成功模块的两个补丁类都必须被安装。");

        // 第二个模块的第二个类抛出：该模块已装的那一个必须被撤回，第一个模块一项都不能动。
        installer.FailOn.Add(typeof(FakeMerchantPatchB));
        var merchantResult = registry.InstallGroup(
            FeatureIds.Merchant,
            [typeof(FakeMerchantPatchA), typeof(FakeMerchantPatchB)]);

        Require(!merchantResult.Installed, "模块内有补丁类抛异常时，该模块必须整体判为未启用。");
        Require(
            installer.Uninstalled.Count == 1 && installer.Uninstalled[0] == typeof(FakeMerchantPatchA),
            "失败模块只能回滚自己已经装上的补丁类，且必须正好撤回那一类。");
        Require(
            !installer.Uninstalled.Contains(typeof(FakeCharacterPatchA)) &&
            !installer.Uninstalled.Contains(typeof(FakeCharacterPatchB)),
            "另一个模块的补丁不能被连带卸载——模块之间不共享失败。");
        Require(registry.IsInstalled(FeatureIds.Character), "已经装好的模块仍要报告为已安装。");
        Require(!registry.IsInstalled(FeatureIds.Merchant), "失败的模块不能报告为已安装。");
        Require(
            registry.Results[FeatureIds.Merchant].Error is { Length: > 0 },
            "失败的模块必须带上失败原因，便于日志定位。");

        var repeated = false;
        try
        {
            registry.InstallGroup(FeatureIds.Character, [typeof(FakeCharacterPatchA)]);
        }
        catch (InvalidOperationException)
        {
            repeated = true;
        }

        Require(repeated, "同一个模块重复安装必须被拒绝，避免补丁被叠加两次。");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition)
        {
            throw new InvalidOperationException(reason);
        }
    }

    private sealed class RecordingInstaller : IPatchInstaller
    {
        internal List<Type> Installed { get; } = [];

        internal List<Type> Uninstalled { get; } = [];

        internal HashSet<Type> FailOn { get; } = [];

        public void Install(Type patchClass)
        {
            if (FailOn.Contains(patchClass))
            {
                throw new MissingMethodException("模拟第三方补丁目标方法在本次游戏版本中缺失。");
            }

            Installed.Add(patchClass);
        }

        public void Uninstall(Type patchClass)
        {
            Uninstalled.Add(patchClass);
            Installed.Remove(patchClass);
        }
    }

    private sealed class FakeCharacterPatchA;

    private sealed class FakeCharacterPatchB;

    private sealed class FakeMerchantPatchA;

    private sealed class FakeMerchantPatchB;
}
