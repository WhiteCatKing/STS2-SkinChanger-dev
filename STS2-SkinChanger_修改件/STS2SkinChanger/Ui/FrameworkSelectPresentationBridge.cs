using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class FrameworkSelectPresentationBridge
{
    public static void Reapply(Control screen, CharacterModel character, NCharacterSelectButton? button)
    {
        if (button != null) CznPresentationBridge.ReapplySelection(screen, character, button);
    }
}