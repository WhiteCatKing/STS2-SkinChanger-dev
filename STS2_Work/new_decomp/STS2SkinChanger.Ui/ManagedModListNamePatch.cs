using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NModMenuRow), "_Ready")]
internal static class ManagedModListNamePatch
{
	[HarmonyPostfix]
	[HarmonyPriority(800)]
	private static void Postfix(NModMenuRow __instance)
	{
		if (!ManagedSkinModLoader.IsManagedProviderForDisplay(__instance.Mod))
		{
			return;
		}
		Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("Title"));
		RichTextLabel val = (RichTextLabel)(object)((nodeOrNull is RichTextLabel) ? nodeOrNull : null);
		if (val == null)
		{
			Label val2 = (Label)(object)((nodeOrNull is Label) ? nodeOrNull : null);
			if (val2 != null)
			{
				val2.Text = ManagedModListNamePolicy.Format(val2.Text, isManagedProvider: true);
			}
		}
		else
		{
			val.Text = ManagedModListNamePolicy.Format(val.Text, isManagedProvider: true);
		}
	}
}
