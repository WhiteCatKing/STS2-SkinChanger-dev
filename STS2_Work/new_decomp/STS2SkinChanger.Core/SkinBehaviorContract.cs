using System;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

internal static class SkinBehaviorContract
{
	public static MethodInfo[] Find(Type type)
	{
		if (!type.IsClass || type.IsAbstract || type.ContainsGenericParameters || !HasProperty(type, "ProfileId", typeof(string)) || !HasProperty(type, "TargetCharacterType", typeof(Type)) || !HasProperty(type, "BodyTexturePath", typeof(string)) || !HasProperty(type, "BodySkeletonDataPath", typeof(string)))
		{
			return Array.Empty<MethodInfo>();
		}
		return type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Where(delegate(MethodInfo method)
		{
			if (method.Name == "AppliesTo" && method.ReturnType == typeof(bool) && !method.IsGenericMethod && method.GetMethodBody() != null)
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == 1)
				{
					ParameterInfo parameterInfo = parameters[0];
					if (parameterInfo != null)
					{
						Type parameterType = parameterInfo.ParameterType;
						if (!(parameterType == typeof(CharacterModel)))
						{
							return parameterType == typeof(Player);
						}
						return true;
					}
				}
			}
			return false;
		}).ToArray();
	}

	private static bool HasProperty(Type type, string name, Type propertyType)
	{
		PropertyInfo property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
		if ((object)property != null && property.PropertyType == propertyType && property.GetIndexParameters().Length == 0)
		{
			MethodInfo getMethod = property.GetMethod;
			if ((object)getMethod != null)
			{
				return !getMethod.IsStatic;
			}
			return false;
		}
		return false;
	}
}
