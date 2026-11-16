using System.Reflection;
using Godot;

namespace STS2SkinChanger.Core;

// This legacy AnimatedSprite pack calls PatchAll before Preload. A removed game argument
// aborts PatchAll, leaving its otherwise valid scene fields null for every room callback.
internal static class LegacyDefectPresentationCompat
{
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    internal static MethodInfo? BackgroundCallback(Assembly assembly) =>
        assembly.GetName().Name == "DefectSkinMod"
            ? assembly.GetType("DefectSkinMod.Scripts.NCharacterSelectScreen_SelectCharacter_Patch")?.GetMethod("Postfix", Static)
            : null;

    internal static void EnsureScenes(Assembly assembly)
    {
        if (BackgroundCallback(assembly) == null) return;
        var globals = assembly.GetType("DefectSkinMod.Scripts.DefectSkinGlobals");
        if (globals?.GetField("CharacterScene", Static)?.GetValue(null) is PackedScene &&
            globals.GetField("SelectBgScene", Static)?.GetValue(null) is PackedScene) return;
        globals?.GetMethod("Preload", Static, null, Type.EmptyTypes, null)?.Invoke(null, null);
    }
}
