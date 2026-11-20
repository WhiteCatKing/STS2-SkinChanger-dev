using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Core;

internal static class HibikiSelectPortrait
{
    private static readonly Harmony PortraitHarmony = new(Entry.ModId + ".HibikiSelectPortrait");
    private static readonly HashSet<MethodBase> Patched = [];

    internal static void InstallFor(Type characterType)
    {
        if (PrivatePath(characterType, false) == null) return;

        // RitsuLib overrides the virtual path getter even though the native assembly
        // also declares it. Patch the resolved implementation once, scoped by model type.
        foreach (var name in new[] { "CharacterSelectIconPath", "CharacterSelectLockedIconPath",
                     "CharacterSelectIcon", "CharacterSelectLockedIcon" })
        {
            if (DeclaredGetter(characterType, name) is not { } getter || Patched.Contains(getter))
                continue;
            try
            {
                var pathGetter = getter.ReturnType == typeof(string);
                var prefix = pathGetter ? nameof(PathPrefix) : nameof(TexturePrefix);
                PortraitHarmony.Patch(getter, prefix: new HarmonyMethod(typeof(HibikiSelectPortrait), prefix)
                    { priority = Priority.First },
                    postfix: pathGetter ? new HarmonyMethod(typeof(HibikiSelectPortrait), nameof(PathPostfix))
                        { priority = Priority.Last } : null);
                Patched.Add(getter);
            }
            catch (Exception error) { CombatAppearanceBoundary.Warn("天音选角头像", error); }
        }
    }

    internal static MethodInfo? DeclaredGetter(Type characterType, string name) =>
        AccessTools.PropertyGetter(characterType, name)?.DeclaringType is { } owner
            ? AccessTools.DeclaredPropertyGetter(owner, name)
            : null;

    private static bool PathPrefix(CharacterModel __instance, MethodBase __originalMethod, ref string __result)
    {
        if (AvailablePath(__instance, __originalMethod.Name.Contains("Locked", StringComparison.Ordinal)) is not { } path)
            return true;
        __result = path;
        return false;
    }

    private static void PathPostfix(CharacterModel __instance, MethodBase __originalMethod, ref string __result)
    {
        var locked = __originalMethod.Name.Contains("Locked", StringComparison.Ordinal);
        var path = PrivatePath(__instance.GetType(), locked);
        if (path != null && __result != path && AvailablePath(__instance, locked) is { } available)
            __result = available;
    }

    private static bool TexturePrefix(CharacterModel __instance, MethodBase __originalMethod, ref CompressedTexture2D __result)
    {
        if (Load(__instance, __originalMethod.Name.Contains("Locked", StringComparison.Ordinal)) is not { } icon)
            return true;
        __result = icon;
        return false;
    }

    internal static string? PrivatePath(Type characterType, bool locked)
    {
        if (characterType.FullName != "MaidenSuccubus.Characters.MaidenSuccubusCharacter" ||
            characterType.Assembly.GetName().Name != "MaidenSuccubus") return null;

        var state = locked ? "locked" : "normal";
        return $"res://MaidenSuccubus/images/ui/character_select/hibiki_amane_select_{state}_v04_132x195.png";
    }

    internal static string? AvailablePath(CharacterModel character, bool locked)
    {
        var path = PrivatePath(character.GetType(), locked);
        if (path == null) return null;
        try { return ResourceLoader.Exists(path) ? path : null; }
        catch (Exception error)
        {
            CombatAppearanceBoundary.Warn("天音选角头像", error);
            return null;
        }
    }

    internal static CompressedTexture2D? Load(CharacterModel character, bool locked)
    {
        var path = AvailablePath(character, locked);
        if (path == null) return null;
        try { return ResourceLoader.Load<CompressedTexture2D>(path); }
        catch (Exception error)
        {
            CombatAppearanceBoundary.Warn("天音选角头像", error);
            return null;
        }
    }
}

[HarmonyPatch(typeof(NCharacterSelectButton), nameof(NCharacterSelectButton.Init))]
internal static class HibikiSelectPortraitInitPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(CharacterModel character) =>
        HibikiSelectPortrait.InstallFor(character.GetType());
}
