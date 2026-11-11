using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Core;

internal sealed record SlotToggleContract(FieldInfo State, FieldInfo Slots, MethodInfo Input, MethodInfo Apply)
{
	public string Id => State.DeclaringType.FullName + ":" + State.Name;

	private const BindingFlags Instance = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	public static SlotToggleContract? TryCreate(Type type)
	{
		MethodInfo method2 = type.GetMethod("_Input", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, new Type[1] { typeof(InputEvent) });
		FieldInfo[] array = (from field in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where field.FieldType == typeof(string[])
			select field).ToArray();
		if (method2?.GetMethodBody() == null || array.Length != 1)
		{
			return null;
		}
		List<CodeInstruction> originalInstructions = PatchProcessor.GetOriginalInstructions((MethodBase)method2, (ILGenerator)null);
		List<FieldInfo> list = new List<FieldInfo>();
		for (int i = 0; i + 3 < originalInstructions.Count; i++)
		{
			if (originalInstructions[i].opcode == OpCodes.Ldfld && originalInstructions[i].operand is FieldInfo fieldInfo && fieldInfo.FieldType == typeof(bool) && originalInstructions[i + 1].opcode == OpCodes.Ldc_I4_0 && originalInstructions[i + 2].opcode == OpCodes.Ceq && originalInstructions[i + 3].opcode == OpCodes.Stfld && object.Equals(originalInstructions[i + 3].operand, fieldInfo))
			{
				list.Add(fieldInfo);
			}
		}
		if (list.Count != 1)
		{
			return null;
		}
		FieldInfo fieldInfo2 = list[0];
		foreach (MethodInfo item in (from method in originalInstructions.Select((CodeInstruction instruction) => instruction.operand).OfType<MethodInfo>()
			where method.DeclaringType == type && method.ReturnType == typeof(void) && method.GetParameters().Length == 0 && method.GetMethodBody() != null
			select method).Distinct())
		{
			List<CodeInstruction> originalInstructions2 = PatchProcessor.GetOriginalInstructions((MethodBase)item, (ILGenerator)null);
			HashSet<string> hashSet = (from instruction in originalInstructions2
				where instruction.opcode == OpCodes.Ldstr
				select instruction.operand as string).ToHashSet();
			if (!hashSet.Contains("get_color") || !hashSet.Contains("set_color"))
			{
				continue;
			}
			for (int j = 0; j + 1 < originalInstructions2.Count; j++)
			{
				if (originalInstructions2[j].opcode != OpCodes.Ldfld || !object.Equals(originalInstructions2[j].operand, fieldInfo2))
				{
					continue;
				}
				int num = j + 1;
				if (num + 2 < originalInstructions2.Count)
				{
					string? name = originalInstructions2[num].opcode.Name;
					if (name != null && name.StartsWith("stloc", StringComparison.Ordinal))
					{
						string? name2 = originalInstructions2[num + 1].opcode.Name;
						if (name2 != null && name2.StartsWith("ldloc", StringComparison.Ordinal))
						{
							int? num2 = LocalIndex(originalInstructions2[num]);
							if (num2.HasValue && num2.GetValueOrDefault() == LocalIndex(originalInstructions2[num + 1]))
							{
								num += 2;
							}
						}
					}
				}
				if (originalInstructions2[num].opcode != OpCodes.Brfalse && originalInstructions2[num].opcode != OpCodes.Brfalse_S)
				{
					continue;
				}
				object operand = originalInstructions2[num].operand;
				if (!(operand is Label))
				{
					continue;
				}
				Label target = (Label)operand;
				int num3 = originalInstructions2.FindIndex(num + 1, (CodeInstruction instruction) => instruction.labels.Contains(target));
				for (int k = num + 1; k + 1 < num3; k++)
				{
					if (originalInstructions2[k].opcode == OpCodes.Ldc_R4 && object.Equals(originalInstructions2[k].operand, 0f) && originalInstructions2[k + 1].opcode == OpCodes.Stfld && originalInstructions2[k + 1].operand is FieldInfo fieldInfo3 && fieldInfo3.DeclaringType == typeof(Color) && fieldInfo3.Name == "A")
					{
						return new SlotToggleContract(fieldInfo2, array[0], method2, item);
					}
				}
			}
		}
		return null;
	}

	private static int? LocalIndex(CodeInstruction instruction)
	{
		if (instruction.operand is LocalVariableInfo localVariableInfo)
		{
			return localVariableInfo.LocalIndex;
		}
		string name = instruction.opcode.Name;
		if (name != null && name.Length == 7)
		{
			if (char.IsDigit(name[name.Length - 1]))
			{
				return name[name.Length - 1] - 48;
			}
		}
		return null;
	}

	[CompilerGenerated]
	private SlotToggleContract(SlotToggleContract original)
	{
		State = original.State;
		Slots = original.Slots;
		Input = original.Input;
		Apply = original.Apply;
	}
}
