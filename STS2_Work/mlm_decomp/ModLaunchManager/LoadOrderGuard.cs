using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace ModLaunchManager;

internal static class LoadOrderGuard
{
	private const int RunAfterEveryone = int.MinValue;

	private const int MaxListedIds = 8;

	private static bool _installed;

	private static bool _promotionScheduled;

	private static int? _savedIndexAtStartup;

	private static string? _savedListAtStartup;

	private static readonly HashSet<string> ReportedOverrides = new HashSet<string>(StringComparer.Ordinal);

	internal static void Install(Harmony harmony)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		if (_installed)
		{
			return;
		}
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(SettingsSaveManager), "SaveSettings", (Type[])null, (Type[])null);
			if (methodInfo == null)
			{
				throw new MissingMethodException("SettingsSaveManager", "SaveSettings");
			}
			HarmonyMethod val = new HarmonyMethod(AccessTools.Method(typeof(LoadOrderGuard), "BeforeSettingsSaved", (Type[])null, (Type[])null));
			val.priority = int.MinValue;
			val.after = new string[1] { "CrossVersionCompat.LoadOrderGuard" };
			HarmonyMethod val2 = val;
			HarmonyMethod val3 = new HarmonyMethod(AccessTools.Method(typeof(LoadOrderGuard), "AfterSettingsSaved", (Type[])null, (Type[])null));
			harmony.Patch((MethodBase)methodInfo, val2, val3, (HarmonyMethod)null, (HarmonyMethod)null);
			_installed = true;
			GD.Print("[ModLaunchManager] settings order guard armed; " + DescribeSavedListAtStartup() + ".");
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not install settings order guard: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static void SchedulePromotionForNextRestart(int modsLoadedBeforeUs)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (_promotionScheduled)
		{
			return;
		}
		_promotionScheduled = true;
		try
		{
			if (Engine.GetMainLoop() is SceneTree)
			{
				Callable val = Callable.From((Action)delegate
				{
					EnsureFirstAndSave(modsLoadedBeforeUs);
				});
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
			else
			{
				EnsureFirstAndSave(modsLoadedBeforeUs);
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not schedule saved-order repair: {ex.GetType().Name} {ex.Message}");
			EnsureFirstAndSave(modsLoadedBeforeUs);
		}
	}

	private static void BeforeSettingsSaved()
	{
		try
		{
			ModSettings modSettings = SaveManager.Instance.SettingsSave.ModSettings;
			List<SettingsSaveMod> list = ((modSettings != null) ? modSettings.ModList : null);
			if (list != null)
			{
				int num = list.FindIndex(IsManagerRow);
				bool flag = list.Any((SettingsSaveMod row) => IsManagerRow(row) && !row.IsEnabled);
				string value = ((num > 0) ? Join(from row in list.Take(num)
					select row.Id) : "");
				PromoteEveryManagerRow(list);
				if (num > 0 || flag)
				{
					string text = ((num > 0) ? $"at position {num + 1}, behind {value}" : "at the front");
					GD.Print("[ModLaunchManager] settings order guard: this mod was " + text + (flag ? " and switched off" : "") + "; moved it to the front and switched it on before saving. Save requested by " + DescribeRequester() + ".");
				}
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not normalize saved order: " + ex.Message);
		}
	}

	private static void AfterSettingsSaved()
	{
		try
		{
			ModSettings modSettings = SaveManager.Instance.SettingsSave.ModSettings;
			List<SettingsSaveMod> list = ((modSettings != null) ? modSettings.ModList : null);
			if (list == null || list.Count == 0)
			{
				return;
			}
			int num = list.FindLastIndex(IsManagerRow);
			if (num < 0)
			{
				return;
			}
			bool flag = list.Where(IsManagerRow).All((SettingsSaveMod row) => row.IsEnabled);
			if (!(num == 0 && flag))
			{
				string text = Join(from row in list.Take(num)
					select row.Id);
				string item = $"{num}|{flag}|{text}";
				if (ReportedOverrides.Add(item))
				{
					GD.PushWarning($"{"[ModLaunchManager]"} another mod changed the saved mod list after the order guard ran: this mod was written at position {num + 1}" + ((num > 0) ? (" behind " + text) : "") + (flag ? "" : ", switched off") + ". The launcher will load late on the next start and repair the order then. Save requested by " + DescribeRequester() + ".");
				}
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not verify the saved order: " + ex.Message);
		}
	}

	private static int PromoteEveryManagerRow(List<SettingsSaveMod> list)
	{
		List<SettingsSaveMod> list2 = list.Where(IsManagerRow).ToList();
		if (list2.Count == 0)
		{
			return -1;
		}
		int result = list.IndexOf(list2[0]);
		list.RemoveAll(list2.Contains);
		list.InsertRange(0, list2);
		foreach (SettingsSaveMod item in list2)
		{
			item.IsEnabled = true;
		}
		if (list2.Count > 1)
		{
			GD.Print($"{"[ModLaunchManager]"} the saved mod list holds {list2.Count} " + "rows for this mod (a local copy and a Workshop subscription both count); all of them were moved to the front so the load order cannot be overwritten.");
		}
		return result;
	}

	private static void EnsureFirstAndSave(int modsLoadedBeforeUs)
	{
		try
		{
			SaveManager instance = SaveManager.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				SettingsSave settingsSave = instance.SettingsSave;
				if (settingsSave == null)
				{
					obj = null;
				}
				else
				{
					ModSettings modSettings = settingsSave.ModSettings;
					obj = ((modSettings != null) ? modSettings.ModList : null);
				}
			}
			List<SettingsSaveMod> list = (List<SettingsSaveMod>)obj;
			if (list == null || list.Count == 0)
			{
				GD.PushWarning("[ModLaunchManager] " + MlmLocManager.Get("guard.savedOrderUnavailable"));
				return;
			}
			string text = DescribeModsLoadedAhead();
			List<string> list2 = list.Select((SettingsSaveMod item) => item.Id).ToList();
			bool flag = list.Where(IsManagerRow).All((SettingsSaveMod item) => item.IsEnabled);
			int num = PromoteEveryManagerRow(list);
			if (num < 0)
			{
				GD.PushWarning("[ModLaunchManager] saved mod list has no manager entry; the save guard will retry on the next settings write.");
				return;
			}
			GD.Print($"{"[ModLaunchManager]"} late start: mods loaded before the launcher: {text}; {DescribeSavedListAtStartup()}; list before repair: {Join(list2)}.");
			if (!flag || !list2.SequenceEqual<string>(list.Select((SettingsSaveMod item) => item.Id), StringComparer.Ordinal))
			{
				SaveManager.Instance.SaveSettings();
			}
			if (_savedIndexAtStartup == 0)
			{
				GD.PushWarning("[ModLaunchManager] " + MlmLocManager.Get("guard.orderOverridden", ("loaded", modsLoadedBeforeUs), ("mods", text)));
				MainMenuNotice.Queue(MlmLocManager.Get("guard.overriddenDialog.title"), MlmLocManager.Get("guard.overriddenDialog.body", ("loaded", modsLoadedBeforeUs), ("mods", text)));
			}
			else
			{
				GD.PushWarning("[ModLaunchManager] " + MlmLocManager.Get("guard.savedManagerFirst", ("position", num + 1), ("loaded", modsLoadedBeforeUs)));
				MainMenuNotice.Queue(MlmLocManager.Get("guard.restartDialog.title"), MlmLocManager.Get("guard.restartDialog.body", ("position", num + 1), ("loaded", modsLoadedBeforeUs)));
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] " + MlmLocManager.Get("guard.saveManagerFirstFailed", ("errorType", ex.GetType().Name), ("error", ex.Message)));
		}
	}

	private static bool IsManagerRow(SettingsSaveMod row)
	{
		return string.Equals(row.Id, "ModLaunchManager", StringComparison.Ordinal);
	}

	private static string DescribeSavedListAtStartup()
	{
		try
		{
			if (!_savedIndexAtStartup.HasValue)
			{
				SaveManager instance = SaveManager.Instance;
				object obj;
				if (instance == null)
				{
					obj = null;
				}
				else
				{
					SettingsSave settingsSave = instance.SettingsSave;
					if (settingsSave == null)
					{
						obj = null;
					}
					else
					{
						ModSettings modSettings = settingsSave.ModSettings;
						obj = ((modSettings != null) ? modSettings.ModList : null);
					}
				}
				List<SettingsSaveMod> list = (List<SettingsSaveMod>)obj;
				if (list == null)
				{
					return "the saved mod list is not available yet";
				}
				_savedIndexAtStartup = list.FindLastIndex(IsManagerRow);
				_savedListAtStartup = ((list.Count == 0) ? "the saved mod list was empty (first modded launch)" : ((_savedIndexAtStartup < 0) ? $"the saved mod list ({list.Count} rows) had no row for this mod yet (first launch after installing it)" : ($"this mod was saved at position {_savedIndexAtStartup + 1} of {list.Count}" + ((_savedIndexAtStartup > 0) ? (", behind " + Join(from row in list.Take(_savedIndexAtStartup.Value)
					select row.Id)) : ""))));
			}
			return _savedListAtStartup ?? "the saved mod list is not available yet";
		}
		catch (Exception ex)
		{
			return "the saved mod list could not be read (" + ex.GetType().Name + ")";
		}
	}

	private static string DescribeModsLoadedAhead()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Invalid comparison between Unknown and I4
		try
		{
			List<string> list = new List<string>();
			foreach (Mod mod in ModManager.Mods)
			{
				if (!string.Equals(mod.manifest?.id, "ModLaunchManager", StringComparison.Ordinal))
				{
					if ((int)mod.state == 1)
					{
						list.Add(mod.manifest?.id ?? "<unknown>");
					}
					continue;
				}
				break;
			}
			return (list.Count == 0) ? "<none>" : Join(list);
		}
		catch (Exception ex)
		{
			return "<unavailable: " + ex.GetType().Name + ">";
		}
	}

	private static string DescribeRequester()
	{
		try
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				typeof(LoadOrderGuard).Assembly.GetName().Name ?? "",
				typeof(SettingsSaveManager).Assembly.GetName().Name ?? "",
				typeof(Harmony).Assembly.GetName().Name ?? "",
				typeof(GD).Assembly.GetName().Name ?? "",
				"mscorlib",
				"netstandard"
			};
			List<string> list = new List<string>();
			StackFrame[] frames = new StackTrace(fNeedFileInfo: false).GetFrames();
			for (int i = 0; i < frames.Length; i++)
			{
				string text = frames[i].GetMethod()?.DeclaringType?.Assembly.GetName().Name;
				if (text != null && !hashSet.Contains(text) && !text.StartsWith("System", StringComparison.Ordinal) && !text.StartsWith("Microsoft", StringComparison.Ordinal) && !list.Contains(text))
				{
					list.Add(text);
				}
			}
			return (list.Count == 0) ? "the game itself" : string.Join(", ", list);
		}
		catch
		{
			return "an unknown caller";
		}
	}

	private static string Join(IEnumerable<string> ids)
	{
		List<string> list = ids.ToList();
		if (list.Count == 0)
		{
			return "<none>";
		}
		string text = string.Join(", ", list.Take(8));
		if (list.Count <= 8)
		{
			return text;
		}
		return $"{text} (+{list.Count - 8} more)";
	}
}
