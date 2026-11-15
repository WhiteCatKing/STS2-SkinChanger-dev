// 受保护兼容模块：十字军/贞德及同包变体。修改前阅读本目录 AGENTS.md 和 README.md。
using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>
/// Skin packs may refuse to touch the character-select presentation when the Skin Changer is present
/// but the selected provider is not literally their own id. The Joan packs do exactly that: their
/// <c>SkinSelectionGate</c> asks the Skin Changer for <c>GetSelectedRuntimeProvider("ironclad")</c> and
/// only proceeds when it reads back <c>JoanofArcIroncladHpwear</c>.
/// <para>
/// The 0.3.5-S re-export is registered under <c>JoanofArcIroncladHpwear111</c>, so that comparison
/// fails and the pack switches off its own menu-portrait sync, its in-run portrait sync and its
/// appearance hotkey — visible as “the avatar never changes” and “the quick-switch key does nothing”,
/// even though the same code works in the 0.3.6 package.
/// </para>
/// <para>
/// A re-export that shares the whole variant contract is the same skin, so the gate is allowed to
/// answer “yes” while one of that contract's variants is selected. Every other selection keeps the
/// pack's own answer, so a genuinely different Ironclad skin still leaves the pack disabled.
/// </para>
/// </summary>
internal static class ProviderMenuGateCompat
{

    private static readonly HashSet<string> Patched = new(StringComparer.OrdinalIgnoreCase);
    private static double _nextScan;

    /// <summary>
    /// 战士（Joan 系）专属的选人门控兼容。作者要求先把所有「只针对战士」的逻辑停用，让铁甲战士
    /// 和其他角色走完全同一条路径；代码保留，确认背景问题根源后再决定是否恢复。
    /// 刻意用 static readonly 而不是 const：运行期同样恒为 false，但编译器不会把保留下来的分支
    /// 判成不可达代码（CS0162），恢复时只改这一行的值即可。
    /// </summary>
    private static readonly bool IroncladCompatEnabled = false;

    /// <summary>Installs the gate patch for every loaded package that exposes a variant contract.</summary>
    internal static void Tick(double delta)
    {
        if (!IroncladCompatEnabled)
        {
            return;
        }

        _nextScan -= delta;
        if (_nextScan > 0)
        {
            return;
        }

        _nextScan = 1d;

        try
        {
            foreach (var contract in VariantContracts())
            {
                if (!ManagedSkinModLoader.TryGetLoadedProviderAssembly(contract.AssemblyPath, out var assembly) ||
                    assembly == null ||
                    !Patched.Add(contract.AssemblyPath))
                {
                    continue;
                }

                PatchGate(contract, assembly);
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("检查皮肤包选人界面接管门控失败：" + exception.GetBaseException().Message);
        }
    }

    private static IEnumerable<ProviderSettingsVariantOptions.SettingsVariantContract> VariantContracts() =>
        ProviderSettingsVariantOptions.Slots()
            .Select(slot => slot.Contract)
            .DistinctBy(contract => contract.AssemblyPath, StringComparer.OrdinalIgnoreCase);

    private static void PatchGate(
        ProviderSettingsVariantOptions.SettingsVariantContract pack,
        Assembly assembly)
    {
        // Everything is discovered from the package itself (see VariantPackCapabilitiesScanner):
        // other packs that expose the same kinds of entry points are handled without touching this
        // file, and a package that exposes none is simply left alone.
        var capabilities = VariantPackCapabilitiesScanner.ForAssembly(assembly);
        if (capabilities == null)
        {
            return;
        }

        var gateHarmony = new Harmony(Entry.ModId + ".variant-gate");
        var gatePatched = 0;
        foreach (var method in capabilities.GateMethods)
        {
            try
            {
                gateHarmony.Patch(
                    method,
                    postfix: new HarmonyMethod(
                        typeof(ProviderMenuGateCompat),
                        nameof(AllowWhenSiblingVariantSelected)));
                gatePatched++;
            }
            catch (Exception exception)
            {
                ModLog.Warn(
                    $"放行皮肤包 {Path.GetFileName(pack.AssemblyPath)} 的门控 {method.Name} 失败：" +
                    exception.GetBaseException().Message);
            }
        }

        if (gatePatched > 0)
        {
            ModLog.Info(
                $"已放行 {Path.GetFileName(pack.AssemblyPath)} 的选人界面/头像门控：选中同源包的变体时，" +
                "它自己的选人头像、局内头像与外观快捷键照常工作。");
        }

        PatchOwnSelectionEntryPoints(pack, capabilities);
    }

    /// <summary>
    /// Keeps the pack's own "act on the character" entry points to the package that actually owns the
    /// selection. With both packs now loaded side by side, the pack that is <b>not</b> selected must not
    /// swap its damage stages onto the other model, and its palette refresh must not repaint it either.
    /// </summary>
    /// <summary>
    /// Keeps the pack's own "act on the character" entry points to the package that owns the selection.
    /// Click cycling stays exclusive to that package; the palette refresh is also allowed for a
    /// same-contract executor, because a package that lost the assembly identity cannot paint its own
    /// variant and the winner has to do it on its behalf.
    /// </summary>
    private static void PatchOwnSelectionEntryPoints(
        ProviderSettingsVariantOptions.SettingsVariantContract pack,
        VariantPackCapabilities capabilities)
    {
        var perTarget = new Harmony(Entry.ModId + ".variant-own-selection");
        var patched = 0;

        foreach (var method in capabilities.ClickMethods)
        {
            try
            {
                perTarget.Patch(
                    method,
                    prefix: new HarmonyMethod(typeof(ProviderMenuGateCompat), nameof(AllowOnlyForOwnSelection)));
                patched++;
            }
            catch
            {
                // Optional guard: the pack's behaviour simply stays as it is.
            }
        }

        foreach (var method in capabilities.PaletteMethods)
        {
            try
            {
                perTarget.Patch(
                    method,
                    prefix: new HarmonyMethod(typeof(ProviderMenuGateCompat), nameof(AllowForOwnerOrExecutor)));
                patched++;
            }
            catch
            {
                // Optional guard: the pack's behaviour simply stays as it is.
            }
        }

        if (patched > 0)
        {
            ModLog.Info(
                $"已锁定 {Path.GetFileName(pack.AssemblyPath)} 的“只在它自己被选中时动作”：" +
                "点角色切形态与调色板刷新不会作用到另一个同源皮肤包上。" +
                $"（{capabilities.Describe()}）");
        }
    }

    /// <summary>
    /// Palette refresh guard: the pack owns the selection, or it is the executor of a same-contract
    /// package that cannot run (its palette is the only one that can paint that variant).
    /// </summary>
    private static bool AllowForOwnerOrExecutor(MethodBase __originalMethod)
    {
        try
        {
            var assembly = __originalMethod?.DeclaringType?.Assembly;
            if (assembly == null ||
                !ManagedSkinModLoader.TryGetLoadedProviderPath(assembly, out var assemblyPath) ||
                ProviderSettingsVariantOptions.ContractForAssembly(assemblyPath) is not { } pack)
            {
                return true;
            }

            var selected = ActiveVariantContract();
            if (selected == null)
            {
                return true;
            }

            if (selected.AssemblyPath.Equals(pack.AssemblyPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return !ProviderSettingsVariantOptions.IsPackageLoaded(selected) &&
                   ProviderSettingsVariantOptions.SharesContractShape(selected, pack);
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Runs before the pack's click handler. Returns false when another package owns the current
    /// Ironclad selection, so the pack cannot swap its own stages onto that package's model.
    /// </summary>
    private static bool AllowOnlyForOwnSelection(MethodBase __originalMethod)
    {
        try
        {
            var assembly = __originalMethod?.DeclaringType?.Assembly;
            if (assembly == null ||
                !ManagedSkinModLoader.TryGetLoadedProviderPath(assembly, out var assemblyPath) ||
                ProviderSettingsVariantOptions.ContractForAssembly(assemblyPath) is not { } pack)
            {
                return true;
            }

            var selected = ActiveVariantContract();
            if (selected == null)
            {
                return true;
            }

            return selected.AssemblyPath.Equals(pack.AssemblyPath, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return true;
        }
    }

    private static void AllowWhenSiblingVariantSelected(
        ref bool __result,
        MethodBase __originalMethod)
    {
        if (__result)
        {
            return;
        }

        try
        {
            var assembly = __originalMethod?.DeclaringType?.Assembly;
            if (assembly == null ||
                !ManagedSkinModLoader.TryGetLoadedProviderPath(assembly, out var assemblyPath) ||
                ProviderSettingsVariantOptions.ContractForAssembly(assemblyPath) is not { } pack)
            {
                return;
            }

            foreach (var pair in SkinService.Config.Selections)
            {
                var contract = ProviderSettingsVariantOptions.ContractOf(pair.Value);
                if (contract == null)
                {
                    continue;
                }

                // This package owns the current selection: its portraits, its hotkey and its click
                // cycling are all correct, so let it through.
                if (contract.AssemblyPath.Equals(pack.AssemblyPath, StringComparison.OrdinalIgnoreCase))
                {
                    __result = true;
                    return;
                }

                // The in-run player gate drives the damage-stage (clothing) behaviour. That must stay
                // strictly with the package that owns the selection: letting a same-contract sibling
                // answer it is what put one pack's clothing damage onto the other pack's model.
                if (__originalMethod?.Name.Contains("Player", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return;
                }

                // Fallback for the case the package could not be loaded at all (identity conflict
                // without an isolated context): a same-contract package acts on its behalf for the
                // menu / character-select presentation.
                if (ProviderSettingsVariantOptions.SharesContractShape(contract, pack) &&
                    !ProviderSettingsVariantOptions.IsPackageLoaded(contract))
                {
                    __result = true;
                    return;
                }
            }
        }
        catch
        {
            // A compatibility patch must never change the pack's answer when it cannot decide.
        }
    }

    private static IEnumerable<Type?> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types;
        }
        catch
        {
            return [];
        }
    }
    /// <summary>
    /// Whether the player shows this package's appearance anywhere right now. Scans every group, not just
    /// the first variant option it finds: with several skin packs that have their own appearance channel,
    /// taking the first one blocked a pack's own click/attach for no reason.
    /// </summary>
    private static bool IsPackageSelected(string assemblyPath)
    {
        foreach (var pair in SkinService.Config.Selections)
        {
            if (ProviderSettingsVariantOptions.ContractOf(pair.Value) is { } contract &&
                contract.AssemblyPath.Equals(assemblyPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The settings-variant contract the player is currently showing, whatever character group it
    /// belongs to. Keeps this compatibility layer free of hard-coded character ids.
    /// </summary>
    private static ProviderSettingsVariantOptions.SettingsVariantContract? ActiveVariantContract()
    {
        foreach (var pair in SkinService.Config.Selections)
        {
            if (ProviderSettingsVariantOptions.ContractOf(pair.Value) is { } contract)
            {
                return contract;
            }
        }

        return null;
    }
}
