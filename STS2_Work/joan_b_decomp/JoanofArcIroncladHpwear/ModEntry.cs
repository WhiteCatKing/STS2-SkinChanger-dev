using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesktopDeathSafety;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace JoanofArcIroncladHpwear;

[ModInitializer("Initialize")]
public static class ModEntry
{
	internal const string HarmonyId = "JoanofArcIroncladHpwear.Patches";

	private static Logger? _log;

	private static readonly List<string> Pending = new List<string>();

	internal static Logger Log
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected O, but got Unknown
			object obj = _log;
			if (obj == null)
			{
				Logger val = new Logger("JoanofArcIroncladHpwear", (LogType)0);
				_log = val;
				obj = (object)val;
			}
			return (Logger)obj;
		}
	}

	internal static void Defer(string message)
	{
		lock (Pending)
		{
			Pending.Add(message);
		}
	}

	internal static void FlushDeferred()
	{
		if (Pending.Count != 0)
		{
			string[] array;
			lock (Pending)
			{
				array = Pending.ToArray();
				Pending.Clear();
			}
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TryInfo(array2[i]);
			}
		}
	}

	internal static void TryInfo(string message)
	{
		try
		{
			Log.Info(message, 1);
		}
		catch
		{
		}
	}

	internal static void TryError(string message)
	{
		try
		{
			Log.Error(message, 1);
		}
		catch
		{
		}
	}

	public static void Initialize()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		Harmony val = new Harmony("JoanofArcIroncladHpwear.Patches");
		int num = 0;
		int num2 = 0;
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types)
		{
			if (DeclaresPatches(type))
			{
				try
				{
					val.CreateClassProcessor(type).Patch();
					num++;
				}
				catch (Exception ex)
				{
					num2++;
					Log.Error($"{type.Name} patch failed (continuing): {ex.GetType().Name}: {ex.Message}", 1);
				}
			}
		}
		RitsuSettingsLoader.TryRegister();
		Palette.StartDeferred();
		if (num == 0)
		{
			Log.Error("JoanofArcIroncladHpwear: no patch class applied — mod inactive.", 1);
			Runtime.Bootstrap();
			PaletteSync.Bootstrap();
			return;
		}
		Log.Info($"{"JoanofArcIroncladHpwear"} v{typeof(ModEntry).Assembly.GetName().Version}: HP wear armed ({num} patch classes, {num2} failed).", 1);
		Runtime.Bootstrap();
		PaletteSync.Bootstrap();
	}

	private static bool DeclaresPatches(Type type)
	{
		if (type.GetCustomAttributes(typeof(HarmonyPatch), inherit: true).Length == 0 && type.GetCustomAttributes(typeof(HarmonyPatchAll), inherit: true).Length == 0)
		{
			return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Any((MethodInfo m) => m.GetCustomAttributes(typeof(HarmonyPrefix), inherit: true).Length != 0 || m.GetCustomAttributes(typeof(HarmonyPostfix), inherit: true).Length != 0 || m.GetCustomAttributes(typeof(HarmonyTranspiler), inherit: true).Length != 0);
		}
		return true;
	}
}
