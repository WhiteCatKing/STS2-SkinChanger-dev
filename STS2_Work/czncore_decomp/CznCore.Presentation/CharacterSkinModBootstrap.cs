using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CznCore.Standalone;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal static class CharacterSkinModBootstrap
{
	private enum InitializationState
	{
		NotStarted,
		Initializing,
		Initialized,
		Failed
	}

	private static readonly object InitializationSync = new object();

	private static readonly object CorePatchSync = new object();

	private static readonly Dictionary<string, InitializationState> States = new Dictionary<string, InitializationState>(StringComparer.Ordinal);

	private static readonly Dictionary<string, CharacterSkinProfile> Profiles = new Dictionary<string, CharacterSkinProfile>(StringComparer.Ordinal);

	private static bool _coreCompatibilityPatchesApplied;

	[ThreadStatic]
	private static List<Action>? _initializationRollback;

	public static CharacterSkinProfile? RegisteredProfile => null;

	public static CharacterSkinProfile? FindProfile(CharacterModel? character)
	{
		lock (InitializationSync)
		{
			foreach (CharacterSkinProfile value in Profiles.Values)
			{
				if (value.AppliesTo(character))
				{
					return value.For(null);
				}
			}
			return null;
		}
	}

	internal static void RegisterProfile(CharacterSkinProfile profile)
	{
		lock (InitializationSync)
		{
			CharacterAppearanceCatalog.Register(profile);
			Profiles.Add(profile.ProfileId, profile);
		}
		TrackInitializationRollback(delegate
		{
			lock (InitializationSync)
			{
				Profiles.Remove(profile.ProfileId);
			}
			CharacterAppearanceCatalog.Unregister(profile.ProfileId);
		});
	}

	internal static void TrackInitializationRollback(Action undo)
	{
		_initializationRollback?.Add(undo);
	}

	public static void Init(Type entryType, CharacterSkinProfile profile, string harmonyId, string initializedMessage, int logPriority = 2, Func<bool>? isDebugEnabled = null, Func<CardModel?, bool>? isPresentationTargetCard = null, Action? configureBeforePatching = null, Action? onInitializationFailed = null)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Expected O, but got Unknown
		lock (InitializationSync)
		{
			if (States.TryGetValue(harmonyId, out var value) && value != 0)
			{
				SkinLog.Warn("Ignored character skin initialization for " + harmonyId + " because state=" + value.ToString() + ". Restart the game before loading a replaced DLL.", logPriority);
				return;
			}
			States[harmonyId] = InitializationState.Initializing;
		}
		SkinLogger skinLogger = new SkinLogger(profile.LogPrefix, isDebugEnabled);
		List<Action> list = (_initializationRollback = new List<Action>());
		try
		{
			StandaloneEntry.RequireApi(1);
			CharacterAppearanceCatalog.ConfigureStorage(ProjectSettings.GlobalizePath("user://CznModConfig/shared/appearances.json"), ProjectSettings.GlobalizePath("user://mod_configs/CznCore/appearances.json"));
			RegisterProfile(profile);
			CombatTriggeredCardComboRuntime.Configure(profile.ProfileId, isPresentationTargetCard);
			configureBeforePatching?.Invoke();
			Harmony harmony = new Harmony(harmonyId);
			ApplyCoreCompatibilityPatchesSafely(harmony, logPriority, skinLogger);
			ApplyHarmonyPatchesSafely(entryType, harmony, logPriority, skinLogger.Info, skinLogger.Warn);
			lock (InitializationSync)
			{
				States[harmonyId] = InitializationState.Initialized;
			}
			skinLogger.Info(initializedMessage, logPriority);
		}
		catch (Exception ex3)
		{
			lock (InitializationSync)
			{
				States[harmonyId] = InitializationState.Failed;
			}
			for (int num = list.Count - 1; num >= 0; num--)
			{
				try
				{
					list[num]();
				}
				catch (Exception ex)
				{
					skinLogger.Warn("Cleanup failed: " + ex, logPriority);
				}
			}
			try
			{
				onInitializationFailed?.Invoke();
			}
			catch (Exception ex2)
			{
				skinLogger.Warn("Cleanup failed: " + ex2, logPriority);
			}
			skinLogger.Error("Failed to initialize: " + ex3, logPriority);
		}
		finally
		{
			_initializationRollback = null;
		}
	}

	public static void ApplyHarmonyPatchesSafely(Type entryType, Harmony harmony, int logPriority = 2)
	{
		ApplyHarmonyPatchesSafely(entryType, harmony, logPriority, SkinLog.Info, SkinLog.Warn);
	}

	private static void ApplyHarmonyPatchesSafely(Type entryType, Harmony harmony, int logPriority, Action<string, int> info, Action<string, int> warn)
	{
		int num = 0;
		List<string> list = null;
		Type[] nestedTypes = entryType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
		foreach (Type type in nestedTypes)
		{
			if (HasHarmonyPatchAttribute(type))
			{
				try
				{
					harmony.CreateClassProcessor(type).Patch();
					num++;
				}
				catch (Exception ex)
				{
					(list ?? (list = new List<string>())).Add(type.Name);
					warn("Skipped incompatible patch class " + type.Name + " (target " + DescribePatchTarget(type) + "): " + FormatExceptionForLog(ex), logPriority);
				}
			}
		}
		info("Harmony patch summary: patched=" + num + " skipped=" + (list?.Count ?? 0) + ((list != null) ? (" [" + string.Join(", ", list) + "]") : string.Empty), logPriority);
	}

	private static void ApplyCoreCompatibilityPatchesSafely(Harmony harmony, int logPriority, SkinLogger log)
	{
		List<string> skipped;
		lock (CorePatchSync)
		{
			if (!_coreCompatibilityPatchesApplied)
			{
				List<Type> list = new List<Type>();
				list.AddRange(RunStartPreloadHarmonyPatches.PatchTypes);
				list.AddRange(SkinTransportPatches.PatchTypes);
				list.AddRange(CombatBlockGainSfxReplacementPatches.PatchTypes);
				list.AddRange(CombatFormVfxSurfacePatches.PatchTypes);
				list.AddRange(SkinCharacterSelectPatches.PatchTypes);
				list.AddRange(SkinCardTransportPatches.PatchTypes);
				list.AddRange(CombatCutInToggleHudPatches.PatchTypes);
				list.AddRange(CombatTriggeredCardComboRuntime.PatchTypes);
				Type[] array = list.ToArray();
				skipped = new List<string>();
				PresentationTransports.TryEnsureTypes(array, Skip);
				_coreCompatibilityPatchesApplied = true;
				log.Info("Core compatibility patch summary: patched=" + (array.Distinct().Count() - skipped.Count) + " skipped=" + skipped.Count + ((skipped.Count != 0) ? (" [" + string.Join(", ", skipped) + "]") : string.Empty), logPriority);
			}
		}
		void Skip(Type patchType, Exception ex)
		{
			skipped.Add(patchType.Name);
			log.Warn("Skipped core compatibility patch " + patchType.Name + " (target " + DescribePatchTarget(patchType) + "): " + FormatExceptionForLog(ex), logPriority);
		}
	}

	private static string DescribePatchTarget(Type patchType)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Invalid comparison between Unknown and I4
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected I4, but got Unknown
		try
		{
			Type type = null;
			string text = null;
			MethodType? val = null;
			Type[] array = null;
			object[] customAttributes = patchType.GetCustomAttributes(inherit: false);
			foreach (object obj in customAttributes)
			{
				HarmonyAttribute val2 = (HarmonyAttribute)((obj is HarmonyAttribute) ? obj : null);
				if (val2 != null)
				{
					HarmonyMethod info = val2.info;
					if ((object)type == null)
					{
						type = info.declaringType;
					}
					if (text == null)
					{
						text = info.methodName;
					}
					MethodType? val3 = val;
					if (!val3.HasValue)
					{
						val = info.methodType;
					}
					if (array == null)
					{
						array = info.argumentTypes;
					}
				}
			}
			if (type == null)
			{
				return "unresolved";
			}
			string text2 = type.Name + "." + (text ?? "<method-level>");
			if (HasTargetMethodResolver(patchType))
			{
				return text2 + " (TargetMethod)";
			}
			MethodType valueOrDefault = val.GetValueOrDefault();
			if (text == null && (int)valueOrDefault != 3)
			{
				return text2;
			}
			return ((int)valueOrDefault switch
			{
				3 => AccessTools.Constructor(type, array, false), 
				1 => AccessTools.PropertyGetter(type, text), 
				2 => AccessTools.PropertySetter(type, text), 
				0 => AccessTools.Method(type, text, array, (Type[])null), 
				_ => null, 
			} == null) ? (text2 + " [missing]") : text2;
		}
		catch
		{
			return "unresolved";
		}
	}

	private static bool HasTargetMethodResolver(Type patchType)
	{
		if (!(patchType.GetMethod("TargetMethod", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null))
		{
			return patchType.GetMethod("TargetMethods", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;
		}
		return true;
	}

	public static void ApplyCharacterSelectTitle(CharacterSkinProfile profile, CharacterModel characterModel, ref string result)
	{
		if (TryGetCharacterSelectIdentity(profile, characterModel, out CharacterSelectIdentityProfile identity))
		{
			result = identity.TitleLocKey;
		}
	}

	public static void ApplyCharacterSelectDescription(CharacterSkinProfile profile, CharacterModel characterModel, ref string result)
	{
		if (TryGetCharacterSelectIdentity(profile, characterModel, out CharacterSelectIdentityProfile identity))
		{
			string descriptionLocKey = profile.For(null).CharacterSelectPresentation.DescriptionLocKey;
			result = (string.IsNullOrEmpty(descriptionLocKey) ? identity.DescriptionLocKey : descriptionLocKey);
		}
	}

	public static void ApplyCharacterMapMarker(CharacterSkinProfile profile, CharacterModel characterModel, ref CompressedTexture2D result)
	{
		if (!profile.AppliesTo(characterModel))
		{
			return;
		}
		string effectiveMapMarkerIconPath = profile.For(null).CharacterSelectPresentation.EffectiveMapMarkerIconPath;
		if (!string.IsNullOrWhiteSpace(effectiveMapMarkerIconPath))
		{
			CompressedTexture2D val = ResourceLoader.Load<CompressedTexture2D>(effectiveMapMarkerIconPath, (string)null, (CacheMode)1);
			if (val != null)
			{
				result = val;
			}
		}
	}

	public static void ApplyCharacterTitle(CharacterSkinProfile profile, CharacterModel characterModel, ref LocString result)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (TryGetCharacterSelectIdentity(profile, characterModel, out CharacterSelectIdentityProfile identity))
		{
			result = new LocString("characters", identity.TitleLocKey);
		}
	}

	public static void ApplyCharacterTitleObject(CharacterSkinProfile profile, CharacterModel characterModel, ref LocString result)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (TryGetCharacterSelectIdentity(profile, characterModel, out CharacterSelectIdentityProfile identity))
		{
			result = new LocString("characters", GetCharacterTitleObjectLocKey(identity));
		}
	}

	public static bool TryGetCharacterSelectIdentity(CharacterSkinProfile profile, CharacterModel characterModel, out CharacterSelectIdentityProfile identity)
	{
		identity = profile.CharacterSelect;
		return profile.AppliesTo(characterModel);
	}

	private static bool HasHarmonyPatchAttribute(Type type)
	{
		try
		{
			object[] customAttributes = type.GetCustomAttributes(inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				if (customAttributes[i].GetType().FullName == "HarmonyLib.HarmonyPatch")
				{
					return true;
				}
			}
		}
		catch
		{
		}
		return false;
	}

	private static string GetCharacterTitleObjectLocKey(CharacterSelectIdentityProfile identity)
	{
		if (!identity.TitleLocKey.EndsWith(".title", StringComparison.Ordinal))
		{
			return identity.TitleLocKey;
		}
		string titleLocKey = identity.TitleLocKey;
		int length = ".title".Length;
		return titleLocKey.Substring(0, titleLocKey.Length - length) + ".titleObject";
	}

	private static string FormatExceptionForLog(Exception ex)
	{
		Exception ex2 = ex;
		while (ex2.InnerException != null)
		{
			ex2 = ex2.InnerException;
		}
		return ex2.GetType().Name + ": " + ex2.Message;
	}
}
