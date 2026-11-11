using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch]
internal static class HeavyAttackGate
{
	private static readonly FieldRef<AttackCommand, string?> AttackerAnimName = AccessTools.FieldRefAccess<AttackCommand, string>("_attackerAnimName");

	private static readonly FieldRef<AttackCommand, float> AttackerAnimDelay = AccessTools.FieldRefAccess<AttackCommand, float>("_attackerAnimDelay");

	private static readonly FieldRef<AttackCommand, bool> ShouldPlayAnimation = AccessTools.FieldRefAccess<AttackCommand, bool>("_shouldPlayAnimation");

	private static readonly FieldRef<AttackCommand, int> HitCount = AccessTools.FieldRefAccess<AttackCommand, int>("_hitCount");

	private static readonly FieldRef<AttackCommand, Creature?> SingleTargetRef = AccessTools.FieldRefAccess<AttackCommand, Creature>("_singleTarget");

	private static readonly FieldRef<AttackCommand, Creature?> VisualAttacker = AccessTools.FieldRefAccess<AttackCommand, Creature>("_visualAttacker");

	private static readonly FieldRef<AttackCommand, decimal> DamagePerHit = AccessTools.FieldRefAccess<AttackCommand, decimal>("_damagePerHit");

	private static readonly FieldRef<AttackCommand, CalculatedDamageVar?> CalculatedVarRef = AccessTools.FieldRefAccess<AttackCommand, CalculatedDamageVar>("_calculatedDamageVar");

	private static readonly MethodInfo? PossibleTargets = AccessTools.Method(typeof(AttackCommand), "GetPossibleTargets", (Type[])null, (Type[])null);

	private const string DefaultAttackAnim = "Attack";

	private static bool _warned;

	[HarmonyPrefix]
	[HarmonyPriority(600)]
	[HarmonyPatch(typeof(AttackCommand), "Execute")]
	private static void ExecutePrefix(AttackCommand __instance)
	{
		try
		{
			TryUpgradeToHeavy(__instance);
		}
		catch (Exception ex)
		{
			WarnOnce("重击判定异常: " + ex.Message);
		}
	}

	private static void TryUpgradeToHeavy(AttackCommand cmd)
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		WearSettings current = WearSettingsRuntime.Current;
		if (!current.HeavyAttackEnabled || !string.Equals(AttackerAnimName.Invoke(cmd), "Attack", StringComparison.Ordinal) || !ShouldPlayAnimation.Invoke(cmd) || HitCount.Invoke(cmd) > 1)
		{
			return;
		}
		Creature attacker = cmd.Attacker;
		if (attacker == null)
		{
			return;
		}
		Creature val = VisualAttacker.Invoke(cmd) ?? attacker;
		Player player = val.Player;
		if (!(((player != null) ? player.Character : null) is Ironclad) || !HeavyAttackFx.IsUsingThisSkin(val))
		{
			return;
		}
		AbstractModel modelSource = cmd.ModelSource;
		if ((modelSource is DramaticEntrance || modelSource is DyingStar || modelSource is FlickFlack) ? true : false)
		{
			return;
		}
		Creature val2 = ResolveJudgeTarget(cmd);
		if (val2 != null)
		{
			CalculatedDamageVar obj = CalculatedVarRef.Invoke(cmd);
			decimal num = ((obj != null) ? ((CalculatedVar)obj).Calculate(val2) : DamagePerHit.Invoke(cmd));
			ICombatState combatState = val2.CombatState;
			IRunState from = IRunState.GetFrom(new Creature[2] { val2, attacker }.OfType<Creature>());
			ValueProp damageProps = cmd.DamageProps;
			AbstractModel modelSource2 = cmd.ModelSource;
			IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
			if (Hook.ModifyDamage(from, combatState, val2, attacker, num, damageProps, (CardModel)(object)((modelSource2 is CardModel) ? modelSource2 : null), cmd.CardPlay, (ModifyDamageHookType)14, (CardPreviewMode)0, ref enumerable) >= (decimal)current.HeavyAttackThreshold)
			{
				AttackerAnimName.Invoke(cmd) = Ironclad.GetHeavyAnimIfApplicable(val.Player.Character);
				AttackerAnimDelay.Invoke(cmd) = Ironclad.GetHeavyAttackDelayIfApplicable(val.Player.Character);
			}
		}
	}

	private static Creature? ResolveJudgeTarget(AttackCommand cmd)
	{
		Creature val = SingleTargetRef.Invoke(cmd);
		if (val != null)
		{
			return val;
		}
		try
		{
			if (PossibleTargets?.Invoke(cmd, null) is IReadOnlyList<Creature> { Count: >0 } readOnlyList)
			{
				return readOnlyList[0];
			}
		}
		catch
		{
		}
		return null;
	}

	private static void WarnOnce(string message)
	{
		if (!_warned)
		{
			_warned = true;
			ModEntry.TryInfo("[heavy-attack] " + message);
		}
	}
}
