using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class FrameworkEntryAnimationPatch
{
	private static readonly FieldInfo? CurrentStateField = AccessTools.Field(typeof(CreatureAnimator), "_currentState");

	private static readonly MethodInfo? SetAnimationMethod = ResolveAnimationMethod("SetAnimation", 3);

	private static readonly MethodInfo? AddAnimationMethod = ResolveAnimationMethod("AddAnimation", 4);

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return (from method in (from type in AccessTools.AllTypes()
				where !type.IsAbstract && !type.ContainsGenericParameters && typeof(CharacterModel).IsAssignableFrom(type)
				select type).SelectMany((Type type) => type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).Where(delegate(MethodInfo method)
			{
				if (method.Name == "GenerateAnimator" && !method.IsAbstract && !method.ContainsGenericParameters && typeof(CreatureAnimator).IsAssignableFrom(method.ReturnType))
				{
					ParameterInfo[] parameters = method.GetParameters();
					if (parameters.Length == 1 || (parameters.Length == 2 && parameters[1].ParameterType == typeof(Creature)))
					{
						return parameters[0].ParameterType == typeof(MegaSprite);
					}
				}
				return false;
			})
			select (MethodInfo)MethodBase.GetMethodFromHandle(method.MethodHandle, method.DeclaringType.TypeHandle)).Distinct();
	}

	[HarmonyPostfix]
	[HarmonyPriority(600)]
	private static void Postfix(CharacterModel __instance, MegaSprite __0, CreatureAnimator __result)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		try
		{
			object? obj = CurrentStateField?.GetValue(__result);
			AnimState val = (AnimState)((obj is AnimState) ? obj : null);
			if (val != null)
			{
				FrameworkEntryAnimationPlan frameworkEntryAnimationPlan = FrameworkEntryAnimationPolicy.Resolve(FrameworkSkinRuntime.HasSelectedCharacterContract(__instance), __0.HasAnimation("entry"), val.Id, val.IsLooping);
				if (!(frameworkEntryAnimationPlan == null) && !(SetAnimationMethod == null) && !(AddAnimationMethod == null))
				{
					MegaAnimationState animationState = __0.GetAnimationState();
					SetAnimationMethod.Invoke(animationState, new object[3] { frameworkEntryAnimationPlan.EntryAnimationId, false, 0 });
					AddAnimationMethod.Invoke(animationState, new object[4] { frameworkEntryAnimationPlan.QueuedAnimationId, 0f, frameworkEntryAnimationPlan.QueuedAnimationLoops, 0 });
					AnimState val2 = new AnimState(frameworkEntryAnimationPlan.EntryAnimationId, false)
					{
						NextState = val
					};
					__result.AddAnyState("Entry", val2, (Func<bool>)null);
					CurrentStateField.SetValue(__result, val2);
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("应用框架皮肤登场动画失败：" + ex.GetBaseException().Message);
		}
	}

	private static MethodInfo? ResolveAnimationMethod(string name, int parameterCount)
	{
		return typeof(MegaAnimationState).GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(delegate(MethodInfo method)
		{
			if (method.Name.Equals(name, StringComparison.Ordinal))
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == parameterCount)
				{
					Type[] second = ((!name.Equals("SetAnimation", StringComparison.Ordinal)) ? new Type[4]
					{
						typeof(string),
						typeof(float),
						typeof(bool),
						typeof(int)
					} : new Type[3]
					{
						typeof(string),
						typeof(bool),
						typeof(int)
					});
					return parameters.Select((ParameterInfo parameter) => parameter.ParameterType).SequenceEqual(second);
				}
			}
			return false;
		});
	}
}
