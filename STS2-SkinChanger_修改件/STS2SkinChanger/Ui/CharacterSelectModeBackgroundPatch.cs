using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Blitz/Dread character-select screens build their own background inside
/// <c>SetSelectedCharacterVisuals</c>-style helpers, and some of those helpers run *after* the
/// patched <c>SelectCharacter</c> postfix (the Dread win-streak animation calls the helper
/// directly). Without this postfix the selected skin's big background is replaced by the game's
/// own scene again. It only re-applies the background, never the rest of the selection UI.
/// </summary>
internal static class CharacterSelectModeBackgroundPatch
{
    // Parametrised by position: the Dread helper's parameter is named characterModel, and this
    // patch is also bound to future variants of the same helper by CharacterSelectModePatches.
    internal static void Postfix(Control __instance, CharacterModel __0)
    {
        if (!GodotObject.IsInstanceValid(__instance) || __0 == null)
        {
            return;
        }

        try
        {
            ContextualSkinControls.ReapplySelectedBackground(__instance, __0);
            CharSelectStageCycle.Attach(__instance, __0);
            ProviderModeSelectOverlayBridge.Attach(__instance, __0);
        }
        catch (Exception exception)
        {
            ModLog.Warn("极速/恐惧选人背景重放失败：" + exception.GetBaseException().Message);
        }
    }
}
