using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Ui;

internal static class FrameworkSelectPresentationBridge
{
	private static readonly List<(object Instance, MethodInfo Apply)> Runtimes = new List<(object, MethodInfo)>();

	private static int _scannedAssemblyCount = -1;

	public static void Reapply(NCharacterSelectScreen screen, CharacterModel character, NCharacterSelectButton? button)
	{
		try
		{
			Scan();
			if (Runtimes.Count == 0 || button == null)
			{
				return;
			}
			foreach (var (obj, methodInfo) in Runtimes)
			{
				try
				{
					methodInfo.Invoke(obj, new object[3] { screen, button, character });
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private static void Scan()
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		if (assemblies.Length == _scannedAssemblyCount)
		{
			return;
		}
		_scannedAssemblyCount = assemblies.Length;
		Assembly[] array = assemblies;
		foreach (Assembly assembly in array)
		{
			Type[] array2;
			try
			{
				array2 = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				array2 = ex.Types.Where((Type type) => type != null).Cast<Type>().ToArray();
			}
			catch
			{
				continue;
			}
			Type[] array3 = array2;
			for (int j = 0; j < array3.Length; j++)
			{
				FieldInfo[] fields = array3[j].GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!fieldInfo.FieldType.Name.Equals("CharacterSelectPresentationRuntime", StringComparison.Ordinal))
					{
						continue;
					}
					object instance;
					try
					{
						instance = fieldInfo.GetValue(null);
					}
					catch
					{
						continue;
					}
					if (instance != null && !Runtimes.Any<(object, MethodInfo)>(((object Instance, MethodInfo Apply) entry) => entry.Instance == instance))
					{
						MethodInfo method = instance.GetType().GetMethod("ApplyScreenSelection", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
						if (method != null)
						{
							Runtimes.Add((instance, method));
						}
					}
				}
			}
		}
	}
}
