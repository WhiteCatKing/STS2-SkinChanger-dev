using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace PengoTarot.Patches;

[HarmonyPatch]
public static class HandCardHolder_EnchantmentIconPatch
{
	private const string IconNodeName = "PlanetEnchantmentIcon";

	private const float IconTop = -304f;

	private const float IconBottom = -208f;

	private const float IconLeft = -48f;

	private const float IconRight = 48f;

	[HarmonyPatch(typeof(NHandCardHolder), "_Ready")]
	[HarmonyPostfix]
	private static void OnReady(NHandCardHolder __instance)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if (((Node)__instance).FindChild("PlanetEnchantmentIcon", false, false) == null)
		{
			TextureRect val = new TextureRect
			{
				Name = StringName.op_Implicit("PlanetEnchantmentIcon"),
				MouseFilter = (MouseFilterEnum)2,
				AnchorLeft = 0.5f,
				AnchorTop = 0.5f,
				AnchorRight = 0.5f,
				AnchorBottom = 0.5f,
				OffsetLeft = -48f,
				OffsetTop = -304f,
				OffsetRight = 48f,
				OffsetBottom = -208f,
				GrowHorizontal = (GrowDirection)2,
				GrowVertical = (GrowDirection)2,
				StretchMode = (StretchModeEnum)5,
				Visible = false
			};
			((Node)__instance).AddChild((Node)(object)val, false, (InternalMode)0);
		}
	}

	[HarmonyPatch(typeof(NHandCardHolder), "SetIndexLabel")]
	[HarmonyPostfix]
	private static void SetIndexLabel_Postfix(NHandCardHolder __instance)
	{
		Node obj = ((Node)__instance).FindChild("PlanetEnchantmentIcon", false, false);
		TextureRect val = (TextureRect)(object)((obj is TextureRect) ? obj : null);
		if (val != null)
		{
			CardModel cardModel = ((NCardHolder)__instance).CardModel;
			EnchantmentModel val2 = ((cardModel != null) ? cardModel.Enchantment : null);
			if (val2 != null && ((object)val2).GetType().Name.StartsWith("Planet"))
			{
				val.Texture = (Texture2D)(object)val2.Icon;
				((CanvasItem)val).Visible = true;
			}
			else
			{
				((CanvasItem)val).Visible = false;
			}
		}
	}
}
