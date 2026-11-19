using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

// Late discovery keeps older game versions loadable: the Blitz/Dread types are optional.
internal static class CharacterSelectModePatches
{
    internal static void Install(Harmony harmony)
    {
        foreach (var name in CharacterSelectScreenAccess.ModeTypes)
        {
            var type = AccessTools.TypeByName(name);
            if (type == null) continue;
            Bind("OnSubmenuOpened", typeof(RandomCharacterLobbySkinPatch), "Prefix", null);
            Bind("SelectCharacter", typeof(CharacterSelectionSkinPatch), "Prefix", "Postfix");
            Bind("AfterInitialized", typeof(MultiplayerSkinLobbyAttachPatch), null, "Postfix");
            Bind("_Process", typeof(MultiplayerSkinLobbyTickPatch), null, "Postfix");
            Bind("StartNewSingleplayerRun", typeof(SingleplayerEmbarkSkinSelectorPatch), "Prefix", null);
            Bind("StartNewMultiplayerRun", typeof(MultiplayerEmbarkSkinSelectorPatch), "Prefix", null);
            // These mode screens also rebuild their background from helpers that run outside
            // SelectCharacter (Dread's win-streak animation calls SetSelectedCharacterVisuals
            // directly). Re-apply the selected skin's background after that helper, with Last
            // priority so no later postfix of ours can undo it.
            BindBackgroundRefresh(type, "SetSelectedCharacterVisuals");

            void BindBackgroundRefresh(Type screenType, string methodName)
            {
                var method = screenType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (method == null || method.GetParameters().Length != 1) return;
                var parameters = method.GetParameters();
                if (parameters[0].ParameterType != typeof(MegaCrit.Sts2.Core.Models.CharacterModel)) return;
                harmony.Patch(method, postfix: new HarmonyMethod(
                    typeof(CharacterSelectModeBackgroundPatch), nameof(CharacterSelectModeBackgroundPatch.Postfix))
                {
                    priority = Priority.Last
                });
            }

            void Bind(string methodName, Type patch, string? prefix, string? postfix)
            {
                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (method == null) return;
                harmony.Patch(method, prefix == null ? null : new HarmonyMethod(patch, prefix) { priority = Priority.First },
                    postfix == null ? null : new HarmonyMethod(patch, postfix) { priority = Priority.First });
            }
        }
    }
}
