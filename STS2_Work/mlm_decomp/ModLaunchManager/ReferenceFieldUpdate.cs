using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace ModLaunchManager;

internal static class ReferenceFieldUpdate
{
	internal static bool TryReplace(FieldInfo field, object? owner, object expected, object? replacement)
	{
		if (!field.IsInitOnly && !field.IsLiteral && !field.FieldType.IsValueType)
		{
			Type? declaringType = field.DeclaringType;
			if ((object)declaringType != null && !declaringType.IsValueType)
			{
				try
				{
					DynamicMethod dynamicMethod = new DynamicMethod("ReplaceRegistrationSnapshot", typeof(bool), new Type[3]
					{
						typeof(object),
						typeof(object),
						typeof(object)
					}, typeof(ReferenceFieldUpdate).Module, skipVisibility: true);
					ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
					if (field.IsStatic)
					{
						iLGenerator.Emit(OpCodes.Ldsflda, field);
					}
					else
					{
						iLGenerator.Emit(OpCodes.Ldarg_0);
						iLGenerator.Emit(OpCodes.Castclass, field.DeclaringType);
						iLGenerator.Emit(OpCodes.Ldflda, field);
					}
					iLGenerator.Emit(OpCodes.Ldarg_2);
					iLGenerator.Emit(OpCodes.Castclass, field.FieldType);
					iLGenerator.Emit(OpCodes.Ldarg_1);
					iLGenerator.Emit(OpCodes.Castclass, field.FieldType);
					MethodInfo methodInfo = typeof(Interlocked).GetMethods(BindingFlags.Static | BindingFlags.Public).Single((MethodInfo candidate) => candidate.Name == "CompareExchange" && candidate.IsGenericMethodDefinition && candidate.GetParameters().Length == 3);
					iLGenerator.Emit(OpCodes.Call, methodInfo.MakeGenericMethod(field.FieldType));
					iLGenerator.Emit(OpCodes.Ldarg_1);
					iLGenerator.Emit(OpCodes.Ceq);
					iLGenerator.Emit(OpCodes.Ret);
					return dynamicMethod.CreateDelegate<Func<object, object, object, bool>>()(owner, expected, replacement);
				}
				catch
				{
					return false;
				}
			}
		}
		return false;
	}
}
