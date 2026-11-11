using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;

namespace CznCore.Presentation;

[HarmonyPatch]
internal static class SharedShieldBreakAudio
{
	internal const string NativeEvent = "event:/sfx/block_break";

	internal static IEnumerable<MethodBase> TargetMethods()
	{
		return from m in (from m in typeof(CreatureCmd).GetMethods(BindingFlags.Static | BindingFlags.Public).Where(delegate(MethodInfo m)
				{
					string name = m.Name;
					return (name == "Damage" || name == "LoseBlock") ? true : false;
				})
				select m.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType into t
				where t != null
				select t).Select((Func<Type, MethodBase>)((Type t) => AccessTools.Method(t, "MoveNext", (Type[])null, (Type[])null)))
			where PatchProcessor.GetOriginalInstructions(m, (ILGenerator)null).Any((CodeInstruction i) => i.opcode == OpCodes.Ldstr && object.Equals(i.operand, "event:/sfx/block_break"))
			select m;
	}

	internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
	{
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.Select((Func<CodeInstruction, CodeInstruction>)((CodeInstruction i) => new CodeInstruction(i))).ToList();
		MethodInfo methodInfo = AccessTools.Method(typeof(SfxCmd), "Play", new Type[2]
		{
			typeof(string),
			typeof(float)
		}, (Type[])null);
		string receiverName = (__originalMethod.DeclaringType.Name.StartsWith("<Damage>", StringComparison.Ordinal) ? "<originalTarget>" : "target");
		FieldInfo fieldInfo = __originalMethod.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Single((FieldInfo f) => f.FieldType == typeof(Creature) && ((!(receiverName == "target")) ? f.Name.StartsWith(receiverName, StringComparison.Ordinal) : (f.Name == receiverName)));
		int num = 0;
		for (int j = 0; j + 2 < list.Count; j++)
		{
			if (!(list[j].opcode != OpCodes.Ldstr) && object.Equals(list[j].operand, "event:/sfx/block_break"))
			{
				if (list[j + 1].opcode != OpCodes.Ldc_R4 || !CodeInstructionExtensions.Calls(list[j + 2], methodInfo))
				{
					throw new InvalidOperationException("Shield break native call shape changed.");
				}
				CodeInstruction val = list[j + 2];
				CodeInstruction val2 = new CodeInstruction(OpCodes.Ldarg_0, (object)null);
				val2.labels.AddRange(val.labels);
				val.labels.Clear();
				val2.blocks.AddRange(val.blocks);
				val.blocks.Clear();
				list.InsertRange(j + 2, new _003C_003Ez__ReadOnlyArray<CodeInstruction>((CodeInstruction[])(object)new CodeInstruction[2]
				{
					val2,
					new CodeInstruction(OpCodes.Ldfld, (object)fieldInfo)
				}));
				val.operand = AccessTools.Method(typeof(SharedShieldBreakAudio), "Play", (Type[])null, (Type[])null);
				num++;
				j += 4;
			}
		}
		if (num != 1)
		{
			throw new InvalidOperationException($"Expected one shield break call, found {num}.");
		}
		return list;
	}

	internal static void Play(string sfx, float volume, Creature target)
	{
		if (!TryReplace(target))
		{
			SfxCmd.Play(sfx, volume);
		}
	}

	internal static bool TryReplace(Creature target)
	{
		if (!NonInteractiveMode.IsActive && !CombatManager.Instance.IsEnding)
		{
			NativeShieldBreakSfxRequest request = new NativeShieldBreakSfxRequest(target);
			try
			{
				SkinEventHub.Publish(request);
			}
			catch (Exception ex)
			{
				SkinLog.Warn("Shield audio failed; native retained: " + ex.Message);
			}
			SkinLog.Debug(() => $"CZN_SHIELD_BREAK_AUDIO side={target.Side} replaced={request.Replaced}");
			return request.Replaced;
		}
		return false;
	}
}
