using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace ModLaunchManager;

internal static class DeferredModLoader
{
	internal sealed record StepResult(Mod Mod, int Completed, int Total, TimeSpan Elapsed, Exception? InvocationError);

	internal sealed class ProjectionSnapshot
	{
		internal required List<Mod> LiveOrder { get; init; }

		internal required Dictionary<Mod, ModLoadState> States { get; init; }

		internal required ModSettings? PreviousSettings { get; init; }

		internal required List<SettingsSaveMod>? PreviousModList { get; init; }
	}

	internal readonly record struct DisableResult(bool Success, string? Error);

	private static readonly List<Mod> Deferred = new List<Mod>();

	private static MethodInfo? _tryLoadMod;

	private static MethodInfo? _stateSetter;

	private static FieldInfo? _modsField;

	private static FieldInfo? _settingsSaveManagerField;

	private static MethodInfo? _rawSaveSettings;

	private static readonly Dictionary<Mod, SettingsSaveMod> ProjectedRows = new Dictionary<Mod, SettingsSaveMod>();

	private static Mod[] _ordered = Array.Empty<Mod>();

	private static Mod[] _projectedOrder = Array.Empty<Mod>();

	private static int _nextIndex;

	private static int _loadsCompleted;

	private static bool _deferring;

	private static bool _resuming;

	private static int _resumeStarted;

	private static int _resumeFinished;

	internal static int DeferredCount => Deferred.Count;

	internal static bool CanEnterVanilla
	{
		get
		{
			if (_deferring)
			{
				return Volatile.Read(in _resumeStarted) == 0;
			}
			return false;
		}
	}

	internal static Mod? NextMod
	{
		get
		{
			if (_nextIndex >= _ordered.Length)
			{
				return null;
			}
			return _ordered[_nextIndex];
		}
	}

	internal static int CompletedCount => _nextIndex;

	internal static int TotalCount => _ordered.Length;

	internal static int LoadsCompleted => _loadsCompleted;

	internal static int LoadsTotal
	{
		get
		{
			int num = _loadsCompleted;
			for (int i = _nextIndex; i < _ordered.Length; i++)
			{
				if (WillLoad(_ordered[i]))
				{
					num++;
				}
			}
			return num;
		}
	}

	internal static int LoadedCount => ModManager.Mods.Count((Mod mod) => (int)mod.state == 1);

	internal static bool Install(Harmony harmony)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		try
		{
			_tryLoadMod = AccessTools.Method(typeof(ModManager), "TryLoadMod", new Type[1] { typeof(Mod) }, (Type[])null);
			_stateSetter = AccessTools.PropertySetter(typeof(ModManager), "State");
			_modsField = AccessTools.Field(typeof(ModManager), "_mods");
			_settingsSaveManagerField = AccessTools.Field(typeof(SaveManager), "_settingsSaveManager");
			_rawSaveSettings = AccessTools.Method(typeof(SettingsSaveManager), "SaveSettings", (Type[])null, (Type[])null);
			if (_tryLoadMod == null || _stateSetter == null || _modsField == null || _settingsSaveManagerField == null || _rawSaveSettings == null)
			{
				throw new MissingMemberException("ModManager loader handles or the raw settings saver were not found");
			}
			_deferring = true;
			harmony.Patch((MethodBase)_tryLoadMod, new HarmonyMethod(AccessTools.Method(typeof(DeferredModLoader), "BeforeTryLoadMod", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			return true;
		}
		catch (Exception ex)
		{
			_deferring = false;
			GD.PushWarning($"{"[ModLaunchManager]"} load interception failed: {ex.GetType().Name} {ex.Message}");
			return false;
		}
	}

	public static bool BeforeTryLoadMod(Mod mod)
	{
		if (!_deferring || _resuming || string.Equals(mod.manifest?.id, "ModLaunchManager", StringComparison.Ordinal))
		{
			return true;
		}
		if (!Deferred.Contains(mod))
		{
			Deferred.Add(mod);
		}
		return false;
	}

	internal static IReadOnlyList<Mod> Snapshot()
	{
		return ModManager.Mods.ToArray();
	}

	internal static IReadOnlyList<Mod> RegisterDiscoveredLocalMods(IEnumerable<Mod> discovered)
	{
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (!(_modsField?.GetValue(null) is List<Mod> list))
		{
			throw new InvalidOperationException("ModManager backing list is unavailable");
		}
		ModSettings modSettings = SaveManager.Instance.SettingsSave.ModSettings;
		List<Mod> list2 = new List<Mod>();
		foreach (Mod mod in discovered)
		{
			if (mod.manifest?.id == null || list.Any((Mod existing) => PhysicalModIdentity.Same(existing, mod)))
			{
				continue;
			}
			mod.state = (ModLoadState)((modSettings != null && modSettings.IsModDisabled(mod.manifest.id, mod.modSource)) ? 3 : 0);
			foreach (Mod item in list.Where((Mod existing) => (int)mod.state == 0 && (int)existing.modSource == 2 && (int)existing.state == 0 && string.Equals(existing.manifest?.id, mod.manifest.id, StringComparison.Ordinal)))
			{
				item.state = (ModLoadState)4;
				item.errors?.Clear();
			}
			list.Add(mod);
			if (!Deferred.Contains(mod))
			{
				Deferred.Add(mod);
			}
			list2.Add(mod);
		}
		return list2;
	}

	internal static void DisableDeferral()
	{
		_deferring = false;
	}

	internal static void CancelForVanilla()
	{
		_deferring = false;
		_resuming = false;
		Interlocked.Exchange(ref _resumeStarted, 1);
		Interlocked.Exchange(ref _resumeFinished, 1);
		Deferred.Clear();
		ProjectedRows.Clear();
		_ordered = Array.Empty<Mod>();
		_projectedOrder = Array.Empty<Mod>();
		_nextIndex = 0;
		_loadsCompleted = 0;
	}

	internal static bool RemoveFromLiveList(Mod mod)
	{
		if (!(_modsField?.GetValue(null) is List<Mod> list))
		{
			return false;
		}
		Deferred.Remove(mod);
		return list.Remove(mod);
	}

	internal static ProjectionSnapshot ApplyPlanProjection(LaunchPlan plan)
	{
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		PlanResult planResult = plan.Validate();
		if (!planResult.Success)
		{
			throw new InvalidOperationException(planResult.Message);
		}
		object obj = _modsField?.GetValue(null);
		List<Mod> liveMods = obj as List<Mod>;
		if (liveMods == null)
		{
			throw new InvalidOperationException("ModManager backing list is unavailable");
		}
		Mod[] array = plan.Entries.Select((LaunchEntry entry) => entry.Mod).ToArray();
		if (array.Length != liveMods.Count || array.Any((Mod mod) => !liveMods.Contains(mod)))
		{
			throw new InvalidOperationException("the discovered Mod set changed while the launch manager was open");
		}
		ModSettings modSettings = SaveManager.Instance.SettingsSave.ModSettings;
		ProjectionSnapshot result = new ProjectionSnapshot
		{
			LiveOrder = new List<Mod>(liveMods),
			States = liveMods.ToDictionary((Mod mod) => mod, (Mod mod) => mod.state),
			PreviousSettings = modSettings,
			PreviousModList = ((modSettings != null) ? modSettings.ModList : null)
		};
		liveMods.Clear();
		liveMods.AddRange(array);
		ModSettings val = (ModSettings)(((object)modSettings) ?? ((object)new ModSettings()));
		SaveManager.Instance.SettingsSave.ModSettings = val;
		ProjectedRows.Clear();
		val.ModList = plan.Entries.Select(delegate(LaunchEntry entry)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			SettingsSaveMod val2 = new SettingsSaveMod(entry.Mod)
			{
				IsEnabled = (entry.IsManager || entry.Enabled)
			};
			ProjectedRows[entry.Mod] = val2;
			return val2;
		}).ToList();
		foreach (LaunchEntry entry in plan.Entries)
		{
			if (!entry.IsManager)
			{
				entry.Mod.state = (ModLoadState)((!entry.Enabled) ? 3 : 0);
			}
		}
		return result;
	}

	internal static void RestorePlanProjection(ProjectionSnapshot snapshot)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		List<Mod> obj = (_modsField?.GetValue(null) as List<Mod>) ?? throw new InvalidOperationException("ModManager backing list is unavailable during rollback");
		obj.Clear();
		obj.AddRange(snapshot.LiveOrder);
		foreach (KeyValuePair<Mod, ModLoadState> state2 in snapshot.States)
		{
			state2.Deconstruct(out var key, out var value);
			Mod obj2 = key;
			ModLoadState state = value;
			obj2.state = state;
		}
		SaveManager.Instance.SettingsSave.ModSettings = snapshot.PreviousSettings;
		if (snapshot.PreviousSettings != null)
		{
			snapshot.PreviousSettings.ModList = snapshot.PreviousModList ?? new List<SettingsSaveMod>();
		}
		ProjectedRows.Clear();
	}

	internal static void SaveSettingsRawOrThrow()
	{
		object obj = _settingsSaveManagerField?.GetValue(SaveManager.Instance) ?? throw new InvalidOperationException("the raw settings saver is unavailable");
		try
		{
			_rawSaveSettings?.Invoke(obj, null);
		}
		catch (TargetInvocationException ex)
		{
			throw ex.InnerException ?? ex;
		}
	}

	internal static void LogSavedPlan(LaunchPlan plan)
	{
		GD.Print($"{"[ModLaunchManager]"} saved launch plan: {plan.EnabledCount} enabled, {plan.DisabledCount} disabled.");
	}

	internal static int BeginResume()
	{
		if (Interlocked.Exchange(ref _resumeStarted, 1) != 0)
		{
			return _ordered.Length;
		}
		if (_tryLoadMod == null || _stateSetter == null)
		{
			throw new InvalidOperationException("the vanilla loader handles were not captured");
		}
		_deferring = false;
		_resuming = true;
		_projectedOrder = ModManager.Mods.ToArray();
		_ordered = _projectedOrder.Where((Mod mod) => !string.Equals(mod.manifest?.id, "ModLaunchManager", StringComparison.Ordinal)).ToArray();
		CollectibleModAssemblyLoader.PrepareLoadPlan(_projectedOrder, MlmLocManager.ModIsolation);
		_nextIndex = 0;
		_loadsCompleted = 0;
		try
		{
			SetManagerState((ModManagerState)0);
			GD.Print($"{"[ModLaunchManager]"} resuming {_ordered.Length} deferred mod(s), {LoadsTotal} of them to load, one that runs code per frame.");
		}
		catch
		{
			_resuming = false;
			throw;
		}
		if (_ordered.Length == 0)
		{
			FinishResume();
		}
		return _ordered.Length;
	}

	internal static bool WillLoad(Mod mod)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		return (int)mod.state == 0;
	}

	internal static StepResult? ResumeNext()
	{
		if (Volatile.Read(in _resumeStarted) == 0)
		{
			throw new InvalidOperationException("deferred loading has not been started");
		}
		if (_nextIndex >= _ordered.Length)
		{
			FinishResume();
			return null;
		}
		Mod val = _ordered[_nextIndex];
		bool flag = WillLoad(val);
		Exception ex = null;
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			_tryLoadMod.Invoke(null, new object[1] { val });
		}
		catch (TargetInvocationException ex2)
		{
			ex = ex2.InnerException ?? ex2;
			GD.PushWarning($"{"[ModLaunchManager]"} loader invocation for {val.manifest?.id ?? "<unknown>"} escaped: {ex}");
		}
		catch (Exception ex3)
		{
			ex = ex3;
			GD.PushWarning($"{"[ModLaunchManager]"} loader invocation for {val.manifest?.id ?? "<unknown>"} failed: {ex3}");
		}
		finally
		{
			stopwatch.Stop();
			RestoreProjectedOrderAfterInitializer(val);
			_nextIndex++;
			if (flag)
			{
				_loadsCompleted++;
			}
		}
		StepResult result = new StepResult(val, _nextIndex, _ordered.Length, stopwatch.Elapsed, ex);
		if (_nextIndex >= _ordered.Length)
		{
			FinishResume();
		}
		return result;
	}

	internal static int ResumeAll()
	{
		BeginResume();
		while (ResumeNext() != null)
		{
		}
		return LoadedCount;
	}

	internal static DisableResult DisableAndSkip(Mod mod)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		int num = Array.IndexOf(_ordered, mod);
		if (Volatile.Read(in _resumeStarted) == 0 || num < 0 || num >= _nextIndex)
		{
			return new DisableResult(Success: false, "the failed mod is no longer at the paused queue position");
		}
		if (!ProjectedRows.TryGetValue(mod, out SettingsSaveMod value))
		{
			return new DisableResult(Success: false, "the concrete settings row for this mod is unavailable");
		}
		ModLoadState state = mod.state;
		bool isEnabled = value.IsEnabled;
		mod.state = (ModLoadState)3;
		value.IsEnabled = false;
		try
		{
			SaveSettingsRawOrThrow();
			return new DisableResult(Success: true, null);
		}
		catch (Exception ex)
		{
			mod.state = state;
			value.IsEnabled = isEnabled;
			GD.PushWarning($"{"[ModLaunchManager]"} could not persist disabled state for {mod.manifest?.id ?? "<unknown>"}: {ex}");
			return new DisableResult(Success: false, ex.Message);
		}
	}

	private static void FinishResume()
	{
		if (Interlocked.Exchange(ref _resumeFinished, 1) == 0)
		{
			try
			{
				SetManagerState((ModManagerState)1);
			}
			finally
			{
				_resuming = false;
			}
			GD.Print($"{"[ModLaunchManager]"} deferred loading finished: {LoadedCount} loaded.");
		}
	}

	private static void RestoreProjectedOrderAfterInitializer(Mod initializedMod)
	{
		if (_projectedOrder.Length == 0)
		{
			return;
		}
		object obj = _modsField?.GetValue(null);
		List<Mod> liveMods = obj as List<Mod>;
		if (liveMods != null && !liveMods.SequenceEqual(_projectedOrder))
		{
			if (liveMods.Count != _projectedOrder.Length || !_projectedOrder.All((Mod expected) => liveMods.Any((Mod actual) => actual == expected)))
			{
				GD.PushWarning("[ModLaunchManager] " + (initializedMod.manifest?.id ?? "<unknown>") + " changed the live mod set; the launch order could not be restored safely.");
				return;
			}
			liveMods.Clear();
			liveMods.AddRange(_projectedOrder);
			GD.PushWarning("[ModLaunchManager] " + (initializedMod.manifest?.id ?? "<unknown>") + " changed the live load order; restored the confirmed launch plan.");
		}
	}

	private static void SetManagerState(ModManagerState state)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_stateSetter.Invoke(null, new object[1] { state });
		if (ModManager.State != state)
		{
			throw new InvalidOperationException($"ModManager.State did not change to {state}");
		}
	}
}
