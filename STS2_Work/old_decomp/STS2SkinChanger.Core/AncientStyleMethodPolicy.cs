using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace STS2SkinChanger.Core;

internal static class AncientStyleMethodPolicy
{
	public static bool ResolveWithoutProviderMethod(bool isNativeAncient, bool requestsAncientLayout)
	{
		return isNativeAncient || requestsAncientLayout;
	}

	public static MethodInfo? Find(Assembly assembly)
	{
		return Find(GetLoadableTypes(assembly));
	}

	public static MethodInfo? Find(IEnumerable<Type> types)
	{
		return (from candidate in Enumerable.ThenBy(from candidate in types.SelectMany((Type type) => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(delegate(MethodInfo method)
				{
					if (method.Name.Equals("IsAncientStyleEnabled", StringComparison.Ordinal) && method.ReturnType == typeof(bool))
					{
						ParameterInfo[] parameters = method.GetParameters();
						if (parameters != null && parameters.Length == 1)
						{
							ParameterInfo parameterInfo = parameters[0];
							if (parameterInfo != null)
							{
								Type parameterType = parameterInfo.ParameterType;
								return parameterType == typeof(string);
							}
						}
					}
					return false;
				}).Select(delegate(MethodInfo method)
				{
					string? fullName = type.FullName;
					return new
					{
						Method = method,
						Score = ((fullName != null && fullName.Equals("CardPortraitsCore.ConfigHelper", StringComparison.Ordinal)) ? 2 : (type.Name.Equals("ConfigHelper", StringComparison.Ordinal) ? 1 : 0))
					};
				}))
				orderby candidate.Score descending
				select candidate, candidate => candidate.Method.DeclaringType?.FullName, StringComparer.Ordinal)
			select candidate.Method).FirstOrDefault();
	}

	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type type) => type != null).Cast<Type>();
		}
	}
}
