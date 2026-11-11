using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class IntrinsicCharacterPreview
{
	private static readonly ConcurrentDictionary<MethodInfo, bool> VisualCallbacks = new ConcurrentDictionary<MethodInfo, bool>();

	internal static bool Replay(NCreature owner, CharacterModel character, string? groupId)
	{
		Assembly assembly = ((object)character).GetType().Assembly;
		if (assembly == typeof(CharacterModel).Assembly || (groupId != null && SkinService.GetSelectedCreatureRuntimeProvider(groupId) != null))
		{
			return false;
		}
		ReadOnlyCollection<Patch> readOnlyCollection = Harmony.GetPatchInfo((MethodBase)AccessTools.Method(typeof(NCreature), "_Ready", (Type[])null, (Type[])null))?.Postfixes;
		if (readOnlyCollection == null)
		{
			return false;
		}
		int num = 0;
		foreach (MethodInfo item in (from patch in readOnlyCollection
			orderby patch.priority descending, patch.index
			select patch.PatchMethod).Distinct())
		{
			if (CanReplay(item, assembly))
			{
				try
				{
					item.Invoke(null, new object[1] { owner });
					num++;
				}
				catch (Exception ex)
				{
					ModLog.Warn($"角色原生模型预览初始化失败 {((AbstractModel)character).Id.Entry}/{item.DeclaringType?.FullName}：" + ex.GetBaseException().Message);
				}
			}
		}
		if (num > 0)
		{
			ModLog.Info($"选角小模型已衔接角色原生外观初始化：{((AbstractModel)character).Id.Entry}；步骤={num}。");
		}
		return num > 0;
	}

	internal static bool CanReplay(MethodInfo callback, Assembly characterAssembly)
	{
		if (!(callback.DeclaringType?.Assembly != characterAssembly) && callback.IsStatic && !(callback.ReturnType != typeof(void)))
		{
			ParameterInfo[] parameters = callback.GetParameters();
			if (parameters != null && parameters.Length == 1)
			{
				ParameterInfo parameterInfo = parameters[0];
				if (parameterInfo != null)
				{
					Type parameterType = parameterInfo.ParameterType;
					if (!(parameterType != typeof(NCreature)))
					{
						return VisualCallbacks.GetOrAdd(callback, Analyze);
					}
				}
			}
		}
		return false;
	}

	private static bool Analyze(MethodInfo callback)
	{
		Assembly assembly = callback.DeclaringType.Assembly;
		Stack<MethodBase> stack = new Stack<MethodBase>();
		HashSet<MethodBase> hashSet = new HashSet<MethodBase>();
		bool flag = false;
		bool flag2 = false;
		int num = 0;
		stack.Push(callback);
		try
		{
			MethodBase result;
			while (stack.TryPop(out result))
			{
				if (!hashSet.Add(result))
				{
					continue;
				}
				if (hashSet.Count > 128 || result.GetMethodBody() == null)
				{
					return false;
				}
				foreach (CodeInstruction originalInstruction in PatchProcessor.GetOriginalInstructions(result, (ILGenerator)null))
				{
					if (++num > 16000 || originalInstruction.opcode == OpCodes.Calli)
					{
						return false;
					}
					if (originalInstruction.operand is FieldInfo fieldInfo && fieldInfo.DeclaringType?.Assembly == typeof(CharacterModel).Assembly)
					{
						return false;
					}
					if (originalInstruction.operand is MethodBase methodBase)
					{
						Type declaringType = methodBase.DeclaringType;
						if (declaringType == null)
						{
							return false;
						}
						if (declaringType == typeof(NCreature))
						{
							flag |= methodBase.Name == "get_Body";
							flag2 |= methodBase.Name == "get_Visuals";
						}
						if (declaringType.Assembly == assembly)
						{
							stack.Push(methodBase);
						}
						else if (!AllowsExternalCall(methodBase, result))
						{
							return false;
						}
					}
				}
			}
			return flag && flag2;
		}
		catch
		{
			return false;
		}
	}

	private static bool AllowsExternalCall(MethodBase called, MethodBase caller)
	{
		Type declaringType = called.DeclaringType;
		string text = declaringType.FullName ?? "";
		if (declaringType.Assembly == typeof(CharacterModel).Assembly)
		{
			if (text.StartsWith("MegaCrit.Sts2.Core.Bindings.MegaSpine.", StringComparison.Ordinal) || text.StartsWith("MegaCrit.Sts2.Core.Logging.", StringComparison.Ordinal))
			{
				return true;
			}
			if (declaringType == typeof(NCreature))
			{
				switch (called.Name)
				{
				case "get_Body":
				case "get_Visuals":
				case "get_Entity":
					return true;
				default:
					return false;
				}
			}
			if (declaringType == typeof(Creature))
			{
				string name = called.Name;
				if (name == "get_IsPlayer" || name == "get_ModelId")
				{
					return true;
				}
				return false;
			}
			if (declaringType == typeof(AbstractModel))
			{
				return called.Name == "get_Id";
			}
			if (declaringType == typeof(ModelDb))
			{
				return called.Name == "Character";
			}
			if (declaringType == typeof(ModelId))
			{
				string name = called.Name;
				if (name == "op_Equality" || name == "op_Inequality")
				{
					return true;
				}
				return false;
			}
			return false;
		}
		if (declaringType.Assembly == typeof(Node).Assembly)
		{
			bool flag = text.StartsWith("Godot.", StringComparison.Ordinal) && declaringType != typeof(SceneTree) && declaringType != typeof(OS) && declaringType != typeof(FileAccess) && declaringType != typeof(DirAccess);
			if (flag)
			{
				bool flag2;
				switch (called.Name)
				{
				case "GetTree":
				case "GetTreeRoot":
				case "GetMainLoop":
					flag2 = true;
					break;
				default:
					flag2 = false;
					break;
				}
				flag = !flag2;
			}
			return flag;
		}
		if (!text.StartsWith("System.", StringComparison.Ordinal))
		{
			return false;
		}
		if (text.StartsWith("System.Reflection.", StringComparison.Ordinal) && called.Name == "Invoke")
		{
			return caller.GetParameters().Any((ParameterInfo parameter) => parameter.ParameterType.FullName?.StartsWith("MegaCrit.Sts2.Core.Bindings.MegaSpine.", StringComparison.Ordinal) ?? false);
		}
		if (!text.StartsWith("System.IO.", StringComparison.Ordinal) && !text.StartsWith("System.Net.", StringComparison.Ordinal) && !text.StartsWith("System.Threading.", StringComparison.Ordinal) && !text.StartsWith("System.Reflection.Emit.", StringComparison.Ordinal) && !text.StartsWith("System.Diagnostics.Process", StringComparison.Ordinal) && !text.StartsWith("System.Runtime.InteropServices.", StringComparison.Ordinal) && declaringType != typeof(Environment))
		{
			return called.Name != "DynamicInvoke";
		}
		return false;
	}
}
