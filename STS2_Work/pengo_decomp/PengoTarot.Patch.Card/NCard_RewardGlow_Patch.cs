using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using PengoTarot.Cards;

namespace PengoTarot.Patch.Card;

[HarmonyPatch(typeof(NCard), "ActivateRewardScreenGlow")]
internal static class NCard_RewardGlow_Patch
{
	private static readonly HashSet<Type> GoldenGlowTypes = new HashSet<Type>
	{
		typeof(TarLoversUpright),
		typeof(TarLoversReversed),
		typeof(TarDevilUprightSub),
		typeof(TarDevilReversedSub),
		typeof(TarStarUprightSub),
		typeof(TarStarReversedSub),
		typeof(TarMoonUprightSub),
		typeof(TarMoonReversedSub),
		typeof(TarSunUprightSub),
		typeof(TarSunReversedSub),
		typeof(TarWorldUprightSub),
		typeof(TarWorldReversedSub),
		typeof(TarWheelOfFortuneUpright),
		typeof(TarWheelOfFortuneReversed),
		typeof(TarTowerUpright),
		typeof(TarTowerReversed)
	};

	private static readonly HashSet<Type> BlueGlowTypes = new HashSet<Type>
	{
		typeof(TarFoolReversed),
		typeof(TarMagicianReversed),
		typeof(TarHighPriestessReversed),
		typeof(TarHermitUpright),
		typeof(TarDevilUpright),
		typeof(TarDevilReversed),
		typeof(TarStarUpright),
		typeof(TarStarReversed),
		typeof(TarMoonUpright),
		typeof(TarMoonReversed),
		typeof(TarSunUpright),
		typeof(TarSunReversed),
		typeof(TarWorldUpright),
		typeof(TarWorldReversed)
	};

	private static readonly FieldRef<NCard, GpuParticles2D> SparklesRef = AccessTools.FieldRefAccess<NCard, GpuParticles2D>("_sparkles");

	private static readonly FieldRef<NCard, NCardRareGlow?> RareGlowRef = AccessTools.FieldRefAccess<NCard, NCardRareGlow>("_rareGlow");

	private static readonly FieldRef<NCard, NCardUncommonGlow?> UncommonGlowRef = AccessTools.FieldRefAccess<NCard, NCardUncommonGlow>("_uncommonGlow");

	private static void Postfix(NCard __instance)
	{
		if (RareGlowRef.Invoke(__instance) != null || UncommonGlowRef.Invoke(__instance) != null)
		{
			return;
		}
		CardModel model = __instance.Model;
		if (model != null)
		{
			Type type = ((object)model).GetType();
			if (GoldenGlowTypes.Contains(type))
			{
				ApplyRareGlow(__instance);
			}
			else if (BlueGlowTypes.Contains(type))
			{
				ApplyUncommonGlow(__instance);
			}
		}
	}

	private static void ApplyRareGlow(NCard instance)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		((CanvasItem)SparklesRef.Invoke(instance)).Visible = true;
		NCardRareGlow val = NCardRareGlow.Create();
		if (val != null)
		{
			((Node2D)val).Scale = new Vector2(1.1f, 1.2f);
			GodotTreeExtensions.AddChildSafely((Node)(object)instance.Body, (Node)(object)val);
			GodotTreeExtensions.MoveChildSafely((Node)(object)instance.Body, (Node)(object)val, 1);
		}
		RareGlowRef.Invoke(instance) = val;
		((CanvasItem)instance.CardHighlight).Modulate = NCardHighlight.gold;
	}

	private static void ApplyUncommonGlow(NCard instance)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		NCardUncommonGlow val = NCardUncommonGlow.Create();
		if (val != null)
		{
			((Node2D)val).Scale = new Vector2(1.1f, 1.2f);
			GodotTreeExtensions.AddChildSafely((Node)(object)instance.Body, (Node)(object)val);
			GodotTreeExtensions.MoveChildSafely((Node)(object)instance.Body, (Node)(object)val, 1);
		}
		UncommonGlowRef.Invoke(instance) = val;
		((CanvasItem)instance.CardHighlight).Modulate = NCardHighlight.playableColor;
	}
}
