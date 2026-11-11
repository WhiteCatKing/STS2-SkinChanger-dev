using System;
using System.Linq;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCreature), "_Ready")]
internal static class EnemyEntranceAnimationPatch
{
	private const string EntrancePlayedMeta = "czn_enemy_skin_entrance_played";

	private static void Postfix(NCreature __instance)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Creature entity = __instance.Entity;
			if (((entity != null) ? entity.Monster : null) == null || __instance.Entity.IsDead || ((GodotObject)__instance).HasMeta(StringName.op_Implicit("czn_enemy_skin_entrance_played")) || !EnemyVisualHostAdapter.TryGetAppliedProfileId(__instance.Visuals, out string profileId) || !Entry.Replacements.TryGetProfile(profileId, out EnemyReplacementProfile profile))
			{
				return;
			}
			string entranceAnimation = profile.Monster.EntranceAnimation;
			if (entranceAnimation == null)
			{
				return;
			}
			bool flag;
			if (__instance.Entity.Monster is TestSubject && TestSubjectAnimatorAdapter.TryPlayEntrance(__instance, profile))
			{
				StringName obj = StringName.op_Implicit("czn_enemy_skin_entrance_played");
				flag = true;
				((GodotObject)__instance).SetMeta(obj, Variant.From<bool>(ref flag));
				return;
			}
			MegaSprite spineBody = __instance.Visuals.SpineBody;
			MegaAnimationState val = ((spineBody != null) ? spineBody.TryGetAnimationState() : null);
			if (val == null)
			{
				Entry.Log.Warn("Skipped enemy entrance because the Spine state was not ready [profile=" + profile.Id + "].", 1);
				return;
			}
			val.SetAnimation(entranceAnimation, false, 0);
			val.AddAnimation(profile.Monster.AnimationBindings.FirstOrDefault((EnemyAnimationBinding candidate) => candidate.Stance == EnemyPresentationStance.Normal && candidate.Action == EnemyAction.Idle)?.CznAnimationName ?? "idle_loop", 0f, true, 0);
			Entry.ActionPresentation.Play(__instance, profile, EnemyAction.Entrance, DecimillipedePresentationAdapter.ShouldPlayEntranceAudio(__instance.Entity.Monster));
			StringName obj2 = StringName.op_Implicit("czn_enemy_skin_entrance_played");
			flag = true;
			((GodotObject)__instance).SetMeta(obj2, Variant.From<bool>(ref flag));
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Enemy entrance animation adapter failed: " + ex.Message, 1);
		}
	}
}
