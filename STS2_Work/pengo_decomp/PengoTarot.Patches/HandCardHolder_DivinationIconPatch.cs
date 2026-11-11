using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using PengoTarot.Data.Divination;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

[HarmonyPatch]
public static class HandCardHolder_DivinationIconPatch
{
	private const string IconNodeName = "DivinationAfflictionIcon";

	private static Texture2D? _justiceTexture;

	private static Texture2D? _hangedManTexture;

	private static Texture2D? _deathTexture;

	private const float IconLeft = 88f;

	private const float IconTop = -288f;

	private const float IconRight = 152f;

	private const float IconBottom = -224f;

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
		if (((Node)__instance).FindChild("DivinationAfflictionIcon", false, false) == null)
		{
			TextureRect val = new TextureRect
			{
				Name = StringName.op_Implicit("DivinationAfflictionIcon"),
				MouseFilter = (MouseFilterEnum)2,
				AnchorLeft = 0.5f,
				AnchorTop = 0.5f,
				AnchorRight = 0.5f,
				AnchorBottom = 0.5f,
				OffsetLeft = 88f,
				OffsetTop = -288f,
				OffsetRight = 152f,
				OffsetBottom = -224f,
				GrowHorizontal = (GrowDirection)2,
				GrowVertical = (GrowDirection)2,
				StretchMode = (StretchModeEnum)5,
				Visible = false
			};
			((Node)__instance).AddChild((Node)(object)val, false, (InternalMode)0);
			UpdateIcon(__instance);
		}
	}

	private static void UpdateIcon(NHandCardHolder holder)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Invalid comparison between Unknown and I4
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Invalid comparison between Unknown and I4
		if (!GodotObject.IsInstanceValid((GodotObject)(object)holder) || !((Node)holder).IsInsideTree())
		{
			return;
		}
		Node obj = ((Node)holder).FindChild("DivinationAfflictionIcon", false, false);
		TextureRect val = (TextureRect)(object)((obj is TextureRect) ? obj : null);
		if (val != null)
		{
			string text = null;
			CardModel cardModel = ((NCardHolder)holder).CardModel;
			Creature val2 = ((cardModel != null) ? cardModel.Owner.Creature : null);
			if (cardModel != null && (int)cardModel.Type == 1 && ((val2 != null) ? val2.GetPower<TarJusticeReversedPower>() : null) != null && !NormalDivinationTurnState.HasPlayedCardThisTurn(val2, (CardType)1))
			{
				text = "res://images/enchantments/tar_justice_reversed_enchantment.png";
			}
			else if (cardModel != null && (int)cardModel.Type == 2 && ((val2 != null) ? val2.GetPower<TarHangedManReversedPower>() : null) != null && !NormalDivinationTurnState.HasPlayedCardThisTurn(val2, (CardType)2))
			{
				text = "res://images/enchantments/tar_hanged_man_reversed_enchantment.png";
			}
			else if (cardModel != null && (int)cardModel.Type == 3 && ((val2 != null) ? val2.GetPower<TarDeathReversedPower>() : null) != null)
			{
				text = "res://images/enchantments/tar_death_reversed_enchantment.png";
			}
			if (text != null)
			{
				TextureRect val3 = val;
				Texture2D texture = ((text == "res://images/enchantments/tar_justice_reversed_enchantment.png") ? GetValidTexture(ref _justiceTexture, text) : ((!(text == "res://images/enchantments/tar_hanged_man_reversed_enchantment.png")) ? GetValidTexture(ref _deathTexture, text) : GetValidTexture(ref _hangedManTexture, text)));
				val3.Texture = texture;
				((CanvasItem)val).Visible = true;
			}
			else
			{
				((CanvasItem)val).Visible = false;
			}
		}
	}

	private static Texture2D GetValidTexture(ref Texture2D? texture, string path)
	{
		if (texture == null || !GodotObject.IsInstanceValid((GodotObject)(object)texture))
		{
			texture = GD.Load<Texture2D>(path);
		}
		return texture;
	}

	[HarmonyPatch(typeof(NHandCardHolder), "SetIndexLabel")]
	[HarmonyPostfix]
	private static void SetIndexLabel_Postfix(NHandCardHolder __instance)
	{
		UpdateIcon(__instance);
	}

	[HarmonyPatch(typeof(NHandCardHolder), "UpdateCard")]
	[HarmonyPostfix]
	private static void UpdateCard_Postfix(NHandCardHolder __instance)
	{
		UpdateIcon(__instance);
	}

	[HarmonyPatch(typeof(NHandCardHolder), "Flash")]
	[HarmonyPostfix]
	private static void Flash_Postfix(NHandCardHolder __instance)
	{
		UpdateIcon(__instance);
	}
}
