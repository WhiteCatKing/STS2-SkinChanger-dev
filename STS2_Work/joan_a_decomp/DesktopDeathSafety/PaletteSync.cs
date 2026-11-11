using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace DesktopDeathSafety;

public static class PaletteSync
{
	private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string Provider = "JoanofArcIroncladHpwear";

	private static Assembly manager;

	public static void Bootstrap()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			MethodInfo methodInfo = assembly.GetType("STS2SkinChanger.Core.VariantHotkeyBridge")?.GetMethod("Adopt", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(string),
				typeof(string)
			}, null);
			if (methodInfo == null)
			{
				continue;
			}
			manager = assembly;
			MethodInfo method = typeof(PaletteSync).GetMethod("BeforeAdopt", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			Harmony val = new Harmony("JoanofArcIroncladHpwear.PaletteNoRebuild");
			try
			{
				Patches patchInfo = Harmony.GetPatchInfo((MethodBase)methodInfo);
				if (patchInfo == null || !patchInfo.Owners.Contains(val.Id))
				{
					val.Patch((MethodBase)methodInfo, new HarmonyMethod(method), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				break;
			}
			catch (Exception ex)
			{
				GD.PushWarning("Jeanne palette bridge unavailable: " + ex.GetBaseException().Message);
				break;
			}
		}
	}

	private static bool IsJeanneColor(string id)
	{
		switch (id)
		{
		default:
			return id == "JoanofArcIroncladHpwear::variant:3";
		case "JoanofArcIroncladHpwear":
		case "JoanofArcIroncladHpwear::variant:1":
		case "JoanofArcIroncladHpwear::variant:2":
			return true;
		}
	}

	public static bool CanAdopt(string groupId, string current, string target, string activeProvider)
	{
		if (groupId == "ironclad" && IsJeanneColor(current) && IsJeanneColor(target) && current != target)
		{
			return activeProvider == "JoanofArcIroncladHpwear";
		}
		return false;
	}

	private static Type ManagerType(string name)
	{
		return manager.GetType("STS2SkinChanger." + name, throwOnError: true);
	}

	public static bool BeforeAdopt(string groupId, string optionId)
	{
		if (groupId != "ironclad" || !IsJeanneColor(optionId) || manager == null)
		{
			return true;
		}
		Type type2;
		Type type3;
		try
		{
			Type type = ManagerType("Core.SkinService");
			type2 = ManagerType("Core.MultiplayerSkinSync");
			type3 = ManagerType("Ui.CharacterAppearanceScreen");
			object obj = type3.GetProperty("HotkeyTargetCreature", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
			string text = type3.GetProperty("HotkeyTargetGroupId", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as string;
			if (obj != null && text == groupId)
			{
				object obj2 = type2.GetMethod("UsesLocalFallbackControls", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { obj });
				if (obj2 is bool && (bool)obj2)
				{
					return true;
				}
			}
			object value = type.GetProperty("Config", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
			IDictionary<string, string> dictionary = (IDictionary<string, string>)value.GetType().GetProperty("Selections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(value);
			lock (type.GetField("Sync", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null))
			{
				if (!dictionary.TryGetValue(groupId, out var value2))
				{
					return true;
				}
				if (type.GetMethod("GetDisplayedSelection", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, new object[1] { groupId }) as string != value2)
				{
					return true;
				}
				string activeProvider = (string)type.GetMethod("GetSelectedRuntimeProvider", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, new object[1] { groupId });
				if (!CanAdopt(groupId, value2, optionId, activeProvider))
				{
					return true;
				}
				string text2 = (string)type.GetProperty("ConfigPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null);
				MethodInfo method = value.GetType().GetMethod("Save", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null);
				dictionary[groupId] = optionId;
				try
				{
					method.Invoke(value, new object[1] { text2 });
				}
				catch
				{
					dictionary[groupId] = value2;
					throw;
				}
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("Jeanne palette sync fallback: " + ex.GetBaseException().Message);
			return true;
		}
		try
		{
			ManagerType("Ui.ContextualSkinControls").GetMethod("RefreshCharacterSelectDropdown", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { groupId });
			type3.GetMethod("RefreshLiveSkinDropdowns", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { groupId });
			object[] array = new object[2] { 0uL, null };
			object obj2 = type2.GetMethod("TryGetLocalCharacter", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, array);
			if (obj2 is bool && (bool)obj2 && array[1]?.GetType().Name == "Ironclad")
			{
				obj2 = type2.GetMethod("RememberLocalAdvertisement", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { false });
				if (obj2 is bool && (bool)obj2)
				{
					type2.GetMethod("SendLocalAdvertisement", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, new object[1] { false });
				}
			}
		}
		catch (Exception ex2)
		{
			GD.PushWarning("Jeanne palette sync UI: " + ex2.GetBaseException().Message);
		}
		return false;
	}
}
