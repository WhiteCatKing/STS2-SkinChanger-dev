using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace STS2SkinChanger.Core;

internal static class MultiplayerTreasureHandAppearance
{
	private sealed class Hand
	{
		internal readonly WeakReference<NHandImage> Node = new WeakReference<NHandImage>(node);

		internal RelicPickingFightMove? Move = move;

		public Hand(NHandImage node, RelicPickingFightMove? move)
		{
		}
	}

	private static readonly List<Hand> Hands = new List<Hand>();

	private static readonly FieldInfo TextureField = AccessTools.Field(typeof(NHandImage), "_textureRect");

	internal static void Remember(NHandImage node, RelicPickingFightMove? move)
	{
		NHandImage target;
		Hand hand2 = Hands.FirstOrDefault((Hand hand) => hand.Node.TryGetTarget(out target) && node == target);
		if (hand2 == null)
		{
			Hands.Add(new Hand(node, move));
		}
		else
		{
			hand2.move = move;
		}
	}

	internal static void Forget(NHandImage node)
	{
		Hands.RemoveAll((Hand hand) => !hand.Node.TryGetTarget(out NHandImage target) || node == target);
	}

	internal static bool RefreshPlayer(ulong playerId)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected I4, but got Unknown
		bool result = false;
		Hand[] array = Hands.ToArray();
		foreach (Hand hand in array)
		{
			if (!hand.Node.TryGetTarget(out NHandImage target) || !GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				Hands.Remove(hand);
				continue;
			}
			Player player = target.Player;
			if (player == null || player.NetId != playerId || !((Node)target).IsInsideTree())
			{
				continue;
			}
			object? value = TextureField.GetValue(target);
			TextureRect val = (TextureRect)((value is TextureRect) ? value : null);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				using (MultiplayerSkinSync.BeginPlayerSelectionScope(playerId))
				{
					CharacterModel character = target.Player.Character;
					TextureRect val2 = val;
					val2.Texture = (Texture2D)(hand.move switch
					{
						0L => character.ArmRockTexture, 
						1L => character.ArmPaperTexture, 
						2L => character.ArmScissorsTexture, 
						_ => character.ArmPointingTexture, 
					});
					result = true;
				}
			}
		}
		return result;
	}
}
