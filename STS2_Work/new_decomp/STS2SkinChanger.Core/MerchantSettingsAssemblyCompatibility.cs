using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace STS2SkinChanger.Core;

internal static class MerchantSettingsAssemblyCompatibility
{
	internal const string CommandBase = "MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.AbstractConsoleCmd";

	internal static MemoryStream? Rewrite(Stream input, out int changed)
	{
		changed = 0;
		Type type2 = typeof(Harmony).Assembly.GetType("Mono.Cecil.AssemblyDefinition", throwOnError: true);
		MethodInfo? method = type2.GetMethod("ReadAssembly", new Type[1] { typeof(Stream) });
		long position = input.Position;
		input.Position = 0L;
		object obj = method.Invoke(null, new object[1] { input });
		try
		{
			object obj2 = Get(obj, "MainModule");
			foreach (object item in from type in Items(Get(obj2, "Types")).ToArray()
				where Text(type, "BaseType") == "MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.AbstractConsoleCmd"
				select type)
			{
				object[] methods = Items(Get(item, "Methods")).ToArray();
				object obj3 = Find(methods, "ApplyToExistingHands", "System.Int32");
				object obj4 = Find(methods, "UpdateLegVisibility", "System.Void", "System.Boolean");
				object obj5 = Find(methods, "TryApplyToHand", "System.Boolean", "MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantHand");
				object obj6 = Find(methods, "UpdateLegVisibilityStatic", "System.Void", "Godot.Node", "System.Boolean");
				if (obj3 == null || obj4 == null || obj5 == null || obj6 == null || !Calls(obj3, "Godot.Engine", "GetMainLoop") || !Calls(obj4, "Godot.Engine", "GetMainLoop") || !Calls(obj3, Text(item, "FullName"), "FindAndApplyRecursive") || !Calls(obj4, Text(item, "FullName"), "UpdateLegVisibilityStatic") || !Items(Get(Get(obj6, "Body"), "Instructions")).Any((object instruction) => GetNullable(instruction, "Operand") as string == "MerchantInventoryLeg"))
				{
					continue;
				}
				MethodInfo method2 = obj2.GetType().GetMethod("ImportReference", new Type[1] { typeof(MethodBase) });
				object operand = method2.Invoke(obj2, new object[1] { typeof(Type).GetMethod("GetTypeFromHandle") });
				object operand2 = method2.Invoke(obj2, new object[1] { typeof(ProviderSettingsApi).GetMethod("Refresh") });
				object[] array = new object[2] { obj3, obj4 };
				foreach (object obj7 in array)
				{
					object obj8 = Get(obj7, "Body");
					Get(obj8, "ExceptionHandlers").GetType().GetMethod("Clear").Invoke(Get(obj8, "ExceptionHandlers"), null);
					Get(obj8, "Variables").GetType().GetMethod("Clear").Invoke(Get(obj8, "Variables"), null);
					object obj9 = Get(obj8, "Instructions");
					obj9.GetType().GetMethod("Clear").Invoke(obj9, null);
					object processor = obj8.GetType().GetMethod("GetILProcessor").Invoke(obj8, null);
					Emit(processor, "Ldtoken", item);
					Emit(processor, "Call", operand);
					Emit(processor, "Call", operand2);
					if (obj7 == obj4)
					{
						Emit(processor, "Pop");
					}
					Emit(processor, "Ret");
					changed++;
				}
			}
			if (changed == 0)
			{
				return null;
			}
			MemoryStream memoryStream = new MemoryStream();
			try
			{
				type2.GetMethod("Write", new Type[1] { typeof(Stream) }).Invoke(obj, new object[1] { memoryStream });
				memoryStream.Position = 0L;
				return memoryStream;
			}
			catch
			{
				memoryStream.Dispose();
				throw;
			}
		}
		finally
		{
			(obj as IDisposable)?.Dispose();
			input.Position = position;
		}
	}

	private static object? Find(IEnumerable<object> methods, string name, string result, params string[] parameters)
	{
		return methods.SingleOrDefault((object method) => Text(method, "Name") == name && (bool)Get(method, "IsStatic") && (bool)Get(method, "HasBody") && Text(method, "ReturnType") == result && (from parameter in Items(Get(method, "Parameters"))
			select Text(parameter, "ParameterType")).SequenceEqual(parameters));
	}

	private static bool Calls(object method, string type, string name)
	{
		return (from instruction in Items(Get(Get(method, "Body"), "Instructions"))
			select GetNullable(instruction, "Operand")).Any((object operand) => operand != null && Text(operand, "Name") == name && Text(operand, "DeclaringType") == type);
	}

	private static void Emit(object processor, string opcode, object? operand = null)
	{
		object value = typeof(Harmony).Assembly.GetType("Mono.Cecil.Cil.OpCodes", throwOnError: true).GetField(opcode).GetValue(null);
		object[] args = ((operand != null) ? new object[2] { value, operand } : new object[1] { value });
		processor.GetType().GetMethods().Single((MethodInfo method) => method.Name == "Emit" && method.GetParameters().Length == args.Length && method.GetParameters().Select((ParameterInfo parameter, int index) => parameter.ParameterType.IsInstanceOfType(args[index])).All((bool match) => match))
			.Invoke(processor, args);
	}

	private static IEnumerable<object> Items(object value)
	{
		return ((IEnumerable)value).Cast<object>();
	}

	private static object? GetNullable(object value, string name)
	{
		return value.GetType().GetProperties().FirstOrDefault((PropertyInfo property) => property.Name == name && property.GetIndexParameters().Length == 0)?.GetValue(value);
	}

	private static object Get(object value, string name)
	{
		return GetNullable(value, name) ?? throw new MissingMemberException(value.GetType().Name, name);
	}

	private static string Text(object value, string name)
	{
		return GetNullable(value, name)?.ToString() ?? "";
	}
}
