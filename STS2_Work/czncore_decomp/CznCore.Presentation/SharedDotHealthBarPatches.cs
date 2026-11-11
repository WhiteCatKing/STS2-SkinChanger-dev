using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CznCore.Gameplay.Dots;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class SharedDotHealthBarPatches
{
	[HarmonyPatch(typeof(NHealthBar), "RefreshText")]
	private static class LethalColorPatch
	{
		private static void Postfix(Creature ____creature, Control ____hpLabel)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			if (____hpLabel != null && ____creature.CurrentHp > 0 && !HpDisplayExtensions.IsInfinite(____creature.HpDisplay))
			{
				SharedDotForecast.Amounts amounts = SharedDotForecast.CalculateAmounts(____creature);
				if (amounts.Total >= ____creature.CurrentHp && (long)amounts.Rift + (long)amounts.Pain > 0 && amounts.Poison < Math.Max(amounts.Rift, amounts.Pain))
				{
					bool flag = amounts.Rift >= amounts.Pain;
					____hpLabel.AddThemeColorOverride(StringName.op_Implicit("font_color"), flag ? SharedDotForecastColors.Rift : SharedDotForecastColors.Pain);
					____hpLabel.AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color(flag ? "4A2808" : "211747"));
				}
			}
		}
	}

	[HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
	private static class ColorPatch
	{
		private static void Postfix(Creature ____creature, Control ____poisonForeground)
		{
			if (____poisonForeground != null)
			{
				SharedDotForecast.Amounts amounts = SharedDotForecast.CalculateAmounts(____creature);
				SharedDotForecastColors.Apply(____poisonForeground, amounts.Poison, amounts.Rift, amounts.Pain);
			}
		}
	}

	[HarmonyPatch]
	private static class ForecastPatch
	{
		private static IEnumerable<MethodBase> TargetMethods()
		{
			return new MethodInfo[3]
			{
				AccessTools.Method(typeof(NHealthBar), "RefreshForeground", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(NHealthBar), "RefreshText", (Type[])null, (Type[])null),
				AccessTools.Method(typeof(NHealthBar), "IsPoisonLethal", (Type[])null, (Type[])null)
			};
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
		{
			//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Expected O, but got Unknown
			List<CodeInstruction> list = instructions.Select((Func<CodeInstruction, CodeInstruction>)((CodeInstruction i) => new CodeInstruction(i))).ToList();
			int num = 0;
			int num2 = 0;
			for (int j = 0; j < list.Count; j++)
			{
				if (!(list[j].operand is MethodInfo methodInfo) || methodInfo.DeclaringType != typeof(Creature) || !methodInfo.IsGenericMethod || methodInfo.GetGenericArguments().SingleOrDefault() != typeof(PoisonPower))
				{
					continue;
				}
				if (methodInfo.Name == "HasPower")
				{
					list[j].opcode = OpCodes.Call;
					list[j].operand = AccessTools.Method(typeof(SharedDotForecast), "HasDamageOverTime", (Type[])null, (Type[])null);
					num2++;
				}
				else if (methodInfo.Name == "GetPower")
				{
					int k;
					for (k = j + 1; k < list.Count && k < j + 14 && !CodeInstructionExtensions.IsStloc(list[k], (LocalBuilder)null); k++)
					{
					}
					if (k >= list.Count || k >= j + 14 || !list.Skip(j).Take(k - j).Any((CodeInstruction c) => c.operand is MethodInfo methodInfo2 && methodInfo2.DeclaringType == typeof(PoisonPower) && methodInfo2.Name == "CalculateTotalDamageNextTurn"))
					{
						throw new InvalidOperationException("Unrecognized native poison forecast IL: " + __originalMethod.Name);
					}
					CodeInstruction val = new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(SharedDotForecast), "Calculate", (Type[])null, (Type[])null));
					val.labels.AddRange(list[j].labels);
					val.blocks.AddRange(list[j].blocks);
					list.RemoveRange(j, k - j);
					list.Insert(j, val);
					num++;
				}
			}
			int num3 = ((!(__originalMethod.Name == "IsPoisonLethal")) ? 1 : 0);
			int num4 = ((!(__originalMethod.Name == "RefreshText")) ? 1 : 0);
			if (num != num3 || num2 != num4)
			{
				throw new InvalidOperationException("Native DOT forecast patch count mismatch: " + __originalMethod.Name);
			}
			return list;
		}
	}

	public static void Initialize(Harmony harmony)
	{
		harmony.CreateClassProcessor(typeof(ForecastPatch)).Patch();
		harmony.CreateClassProcessor(typeof(ColorPatch)).Patch();
		harmony.CreateClassProcessor(typeof(LethalColorPatch)).Patch();
	}
}
