using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Utils;

public static class MultiHitDetector
{
	private static readonly ConcurrentDictionary<Type, bool> _cache;

	private static readonly Dictionary<short, OpCode> _opCodes;

	static MultiHitDetector()
	{
		_cache = new ConcurrentDictionary<Type, bool>();
		_opCodes = new Dictionary<short, OpCode>();
		FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType == typeof(OpCode))
			{
				OpCode value = (OpCode)fieldInfo.GetValue(null);
				_opCodes[value.Value] = value;
			}
		}
	}

	public static bool IsMultiHitCard(CardModel card)
	{
		Type type = ((object)card).GetType();
		if (_cache.TryGetValue(type, out var value))
		{
			return value;
		}
		try
		{
			bool flag = ScanType(type);
			_cache[type] = flag;
			return flag;
		}
		catch (Exception ex)
		{
			Log.Error($"[PengoTarot.MultiHitDetector] Failed to scan {type.FullName}; assuming not multi-hit. {ex.GetType().Name}: {ex.Message}", 2);
			_cache[type] = false;
			return false;
		}
	}

	private static bool ScanType(Type cardType)
	{
		MethodInfo method = cardType.GetMethod("OnPlay", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
		{
			typeof(PlayerChoiceContext),
			typeof(CardPlay)
		}, null);
		if (method == null || method.DeclaringType == typeof(CardModel))
		{
			return false;
		}
		MethodInfo actualLogicMethod = GetActualLogicMethod(method);
		if (actualLogicMethod == null)
		{
			return false;
		}
		HashSet<MethodInfo> visited = new HashSet<MethodInfo>();
		return ScanMethod(actualLogicMethod, cardType, visited);
	}

	private static MethodInfo? GetActualLogicMethod(MethodInfo method)
	{
		AsyncStateMachineAttribute customAttribute = method.GetCustomAttribute<AsyncStateMachineAttribute>();
		if (customAttribute == null)
		{
			return method;
		}
		Type stateMachineType = customAttribute.StateMachineType;
		MethodInfo method2 = stateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
		if (method2 != null)
		{
			return method2;
		}
		MethodInfo[] methods = stateMachineType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			if (methodInfo.Name.Contains("MoveNext") && methodInfo.GetParameters().Length == 0)
			{
				return methodInfo;
			}
		}
		return null;
	}

	private static bool ScanMethod(MethodInfo method, Type owningType, HashSet<MethodInfo> visited)
	{
		if (!visited.Add(method))
		{
			return false;
		}
		if (method.DeclaringType != owningType && !owningType.IsAssignableFrom(method.DeclaringType) && method.DeclaringType?.ReflectedType != owningType)
		{
			return false;
		}
		MethodBody methodBody = method.GetMethodBody();
		if (methodBody == null)
		{
			return false;
		}
		byte[] iLAsByteArray = methodBody.GetILAsByteArray();
		if (iLAsByteArray == null || iLAsByteArray.Length == 0)
		{
			return false;
		}
		int num = iLAsByteArray.Length * 2;
		int num2 = 0;
		int num3 = 0;
		while (num3 < iLAsByteArray.Length)
		{
			short num4 = iLAsByteArray[num3];
			int num5 = 1;
			if (num4 == 254)
			{
				if (num3 + 1 >= iLAsByteArray.Length)
				{
					break;
				}
				num4 = (short)(0xFE00 | iLAsByteArray[num3 + 1]);
				num5 = 2;
			}
			if (!_opCodes.TryGetValue(num4, out var value))
			{
				num3 += num5;
				continue;
			}
			if (num2++ > num)
			{
				return false;
			}
			if (value == OpCodes.Switch)
			{
				int num6 = num3 + num5;
				if (num6 + 4 <= iLAsByteArray.Length)
				{
					uint num7 = BitConverter.ToUInt32(iLAsByteArray, num6);
					num3 = num6 + 4 + (int)(num7 * 4);
				}
				else
				{
					num3 += num5;
				}
				continue;
			}
			int num8 = num5 + GetOperandSize(value);
			if (num8 < num5)
			{
				num3 += num5;
				continue;
			}
			if (value == OpCodes.Call || value == OpCodes.Callvirt)
			{
				int num9 = num3 + num5;
				if (num9 + 4 <= iLAsByteArray.Length)
				{
					int metadataToken = BitConverter.ToInt32(iLAsByteArray, num9);
					MethodInfo methodInfo = null;
					try
					{
						methodInfo = method.Module.ResolveMethod(metadataToken) as MethodInfo;
					}
					catch
					{
					}
					if (methodInfo != null && methodInfo.Name == "WithHitCount" && methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(int))
					{
						return true;
					}
					if (methodInfo?.DeclaringType != null && (methodInfo.DeclaringType == owningType || methodInfo.DeclaringType.DeclaringType == owningType))
					{
						MethodInfo actualLogicMethod = GetActualLogicMethod(methodInfo);
						if (actualLogicMethod != null && ScanMethod(actualLogicMethod, owningType, visited))
						{
							return true;
						}
					}
				}
			}
			num3 += num8;
		}
		return false;
	}

	private static int GetOperandSize(OpCode op)
	{
		return op.OperandType switch
		{
			OperandType.InlineBrTarget => 4, 
			OperandType.InlineField => 4, 
			OperandType.InlineI => 4, 
			OperandType.InlineMethod => 4, 
			OperandType.InlineSig => 4, 
			OperandType.InlineString => 4, 
			OperandType.InlineTok => 4, 
			OperandType.InlineType => 4, 
			OperandType.InlineVar => 2, 
			OperandType.InlineI8 => 8, 
			OperandType.InlineR => 8, 
			OperandType.ShortInlineBrTarget => 1, 
			OperandType.ShortInlineI => 1, 
			OperandType.ShortInlineR => 4, 
			OperandType.ShortInlineVar => 1, 
			_ => 0, 
		};
	}
}
