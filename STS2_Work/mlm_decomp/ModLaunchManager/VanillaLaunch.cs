using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;

namespace ModLaunchManager;

internal static class VanillaLaunch
{
	internal const string HarmonyOwner = "ModLaunchManager.Bootstrap";

	private static Harmony? _harmony;

	private static FieldInfo? _mods;

	private static FieldInfo? _state;

	private static FieldInfo? _modTypes;

	private static FieldInfo? _patchCache;

	private static FieldInfo? _earlyState;

	private static FieldInfo? _steamCallback;

	private static PropertyInfo? _assemblyMap;

	private static ResolveEventHandler? _assemblyResolver;

	private static MethodInfo? _copySaves;

	private static object? _pendingSaveCopy;

	private static bool _deferSaveCopy;

	private static ModSettings? _originalModSettings;

	private static List<SettingsSaveMod>? _originalModList;

	internal static bool Available { get; private set; }

	internal static bool IsActive { get; private set; }

	internal static bool RequiresRestart { get; private set; }

	internal static void Install(Harmony harmony)
	{
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Expected O, but got Unknown
		try
		{
			_harmony = harmony;
			_mods = RequiredField(typeof(ModManager), "_mods", typeof(List<Mod>));
			_state = RequiredField(typeof(ModManager), "<State>k__BackingField", typeof(ModManagerState));
			_patchCache = RequiredField(typeof(ModManager), "_hasHarmonyPatches", typeof(bool?));
			_modTypes = RequiredField(typeof(ReflectionHelper), "_modTypes", typeof(Type[]));
			_earlyState = AccessTools.Field(typeof(OneTimeInitialization), "_state") ?? throw new MissingFieldException("OneTimeInitialization._state");
			_steamCallback = AccessTools.Field(typeof(ModManager), "_steamItemInstalledCallback") ?? throw new MissingFieldException("ModManager._steamItemInstalledCallback");
			if (!_steamCallback.IsStatic || _steamCallback.IsInitOnly || !typeof(IDisposable).IsAssignableFrom(_steamCallback.FieldType))
			{
				throw new MissingFieldException("Unsupported Workshop discovery callback.");
			}
			Type? type = typeof(ModManager).Assembly.GetType("MegaCrit.Sts2.Core.Modding.AssemblyInfo");
			_assemblyMap = type?.GetProperty("ModMap", BindingFlags.Static | BindingFlags.Public);
			if (type != null && _assemblyMap == null)
			{
				throw new MissingMemberException("AssemblyInfo.ModMap");
			}
			_assemblyResolver = (AccessTools.Method(typeof(ModManager), "HandleAssemblyResolveFailure", (Type[])null, (Type[])null) ?? throw new MissingMethodException("ModManager.HandleAssemblyResolveFailure")).CreateDelegate<ResolveEventHandler>();
			_originalModSettings = SaveManager.Instance.SettingsSave.ModSettings;
			ModSettings? originalModSettings = _originalModSettings;
			_originalModList = ((originalModSettings != null) ? originalModSettings.ModList.ToList() : null);
			_copySaves = AccessTools.Method(typeof(ModManager), "CopyUnmoddedSaveFilesIfNeeded", (Type[])null, (Type[])null);
			if (_copySaves != null)
			{
				harmony.Patch((MethodBase)_copySaves, new HarmonyMethod(AccessTools.Method(typeof(VanillaLaunch), "BeforeSaveCopy", (Type[])null, (Type[])null)), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			_deferSaveCopy = true;
			Available = true;
		}
		catch (Exception value)
		{
			Available = false;
			_deferSaveCopy = false;
			GD.PushWarning($"{"[ModLaunchManager]"} same-process vanilla launch unavailable: {value}");
		}
	}

	private static FieldInfo RequiredField(Type type, string name, Type fieldType)
	{
		FieldInfo fieldInfo = AccessTools.Field(type, name);
		if ((object)fieldInfo == null || !fieldInfo.IsStatic || fieldInfo.IsInitOnly || fieldInfo.FieldType != fieldType)
		{
			throw new MissingFieldException(type.FullName, name);
		}
		return fieldInfo;
	}

	private static bool BeforeSaveCopy(object __0)
	{
		if (!Available || !_deferSaveCopy || !StartupGate.ShouldSuspend())
		{
			return true;
		}
		_pendingSaveCopy = __0;
		return false;
	}

	internal static void CompleteModdedLaunch()
	{
		_deferSaveCopy = false;
		object pendingSaveCopy = _pendingSaveCopy;
		_pendingSaveCopy = null;
		if (pendingSaveCopy == null || IsActive)
		{
			return;
		}
		try
		{
			_copySaves.Invoke(null, new object[1] { pendingSaveCopy });
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} deferred first-time save copy failed: {value}");
		}
	}

	internal static bool TryEnter(out string errorKey)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Invalid comparison between Unknown and I4
		errorKey = "vanilla.unavailable";
		List<(MethodBase, Patches)> list = new List<(MethodBase, Patches)>();
		bool flag = false;
		try
		{
			if (!Available || IsActive || RequiresRestart || !StartupGate.ShouldSuspend() || !DeferredModLoader.CanEnterVanilla || _earlyState?.GetValue(null)?.ToString() != "VeryEarly" || (int)ModManager.State != 1 || SaveManager.Instance.IsProfileInitialized || _assemblyMap?.GetValue(null) != null)
			{
				return false;
			}
			List<Mod> source = (List<Mod>)_mods.GetValue(null);
			Mod[] managers = source.Where((Mod mod) => mod.manifest?.id == "ModLaunchManager" && (int)mod.state == 1).ToArray();
			Assembly managerAssembly = typeof(ModLaunchManagerMod).Assembly;
			if (managers.Length != 1 || managers[0].manifest.hasPck || !CollectibleModAssemblyLoader.GetRecordedAssemblies(managers[0]).Contains(managerAssembly) || source.Any(delegate(Mod mod)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Invalid comparison between Unknown and I4
				bool flag2 = mod != managers[0];
				if (flag2)
				{
					ModLoadState state = mod.state;
					bool flag3 = state - 1 <= 1;
					flag2 = flag3 || CollectibleModAssemblyLoader.GetRecordedAssemblies(mod).Length != 0;
				}
				return flag2;
			}) || CollectibleModAssemblyLoader.GetRecordedAssemblies(managers[0]).Any((Assembly assembly) => assembly != managerAssembly))
			{
				errorKey = "vanilla.alreadyLoaded";
				return false;
			}
			MethodBase[] array = Harmony.GetAllPatchedMethods().ToArray();
			foreach (MethodBase methodBase in array)
			{
				Patches patchInfo = Harmony.GetPatchInfo(methodBase);
				if (patchInfo != null && patchInfo.Owners.Count != 0)
				{
					if (patchInfo.Owners.Any((string owner) => owner != "ModLaunchManager.Bootstrap"))
					{
						errorKey = "vanilla.alreadyLoaded";
						return false;
					}
					list.Add((methodBase, patchInfo));
				}
			}
			foreach (var item in list)
			{
				_harmony.Unpatch(item.Item1, (HarmonyPatchType)0, "ModLaunchManager.Bootstrap");
			}
			if (Harmony.GetAllPatchedMethods().Any(delegate(MethodBase method)
			{
				Patches patchInfo2 = Harmony.GetPatchInfo(method);
				return patchInfo2 != null && patchInfo2.Owners.Count > 0;
			}))
			{
				throw new InvalidOperationException("Active patches remain after removing launcher hooks.");
			}
			object value = _steamCallback.GetValue(null);
			if (value != null && !(value is IDisposable))
			{
				throw new InvalidOperationException("Unrecognized Workshop discovery callback.");
			}
			flag = true;
			(value as IDisposable)?.Dispose();
			_steamCallback.SetValue(null, null);
			AppDomain.CurrentDomain.AssemblyResolve -= _assemblyResolver;
			_mods.SetValue(null, new List<Mod>());
			_state.SetValue(null, (object)(ModManagerState)2);
			_modTypes.SetValue(null, null);
			_patchCache.SetValue(null, false);
			UserDataPathProvider.IsRunningModded = false;
			SaveManager.Instance.SettingsSave.ModSettings = _originalModSettings;
			if (_originalModSettings != null)
			{
				_originalModSettings.ModList = _originalModList;
			}
			DeferredModLoader.CancelForVanilla();
			SaveProfileSwitcher.CancelRequest();
			AutoUnlockAll.CancelRequest();
			_pendingSaveCopy = null;
			_deferSaveCopy = false;
			IsActive = true;
			GD.Print("[ModLaunchManager] continuing this process as vanilla: no loaded mods or active patches; original saves selected.");
			return true;
		}
		catch (Exception value2)
		{
			RequiresRestart |= flag;
			GD.PushWarning($"{"[ModLaunchManager]"} vanilla handoff failed: {value2}");
			try
			{
				foreach (var item2 in list)
				{
					_harmony.Unpatch(item2.Item1, (HarmonyPatchType)0, "ModLaunchManager.Bootstrap");
					Restore(item2.Item1, item2.Item2.Prefixes, (HarmonyPatchType)1);
					Restore(item2.Item1, item2.Item2.Postfixes, (HarmonyPatchType)2);
					Restore(item2.Item1, item2.Item2.Transpilers, (HarmonyPatchType)3);
					Restore(item2.Item1, item2.Item2.Finalizers, (HarmonyPatchType)4);
				}
			}
			catch (Exception value3)
			{
				RequiresRestart = true;
				GD.PushWarning($"{"[ModLaunchManager]"} could not restore launcher hooks: {value3}");
			}
			errorKey = (RequiresRestart ? "vanilla.failedRestart" : "vanilla.failed");
			return false;
		}
	}

	private static void Restore(MethodBase method, IEnumerable<Patch> patches, HarmonyPatchType kind)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Invalid comparison between Unknown and I4
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		foreach (Patch item in patches.OrderBy((Patch patch) => patch.index))
		{
			HarmonyMethod val = new HarmonyMethod(item.PatchMethod)
			{
				priority = item.priority,
				before = item.before,
				after = item.after
			};
			_harmony.Patch(method, ((int)kind == 1) ? val : null, ((int)kind == 2) ? val : null, ((int)kind == 3) ? val : null, ((int)kind == 4) ? val : null);
		}
	}
}
