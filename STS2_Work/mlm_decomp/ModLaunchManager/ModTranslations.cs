using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModTranslations
{
	internal static bool Available { get; private set; }

	internal static string Root => LauncherDataPaths.Directory("mod_launch_translations");

	internal static string Id(Mod mod)
	{
		return mod.manifest?.id ?? throw new TranslationFileException("invalidPath");
	}

	internal static string Folder(Mod mod)
	{
		return ModTranslationFiles.ModFolder(Root, Id(mod));
	}

	internal static string PackPath(Mod mod)
	{
		return Path.Combine(mod.path, mod.manifest.id + ".pck");
	}

	internal static bool CanEdit(Mod mod)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		if (Available)
		{
			ModManifest manifest = mod.manifest;
			if (manifest != null && manifest.hasPck && (int)mod.state != 1)
			{
				return CollectibleModAssemblyLoader.GetRecordedAssemblies(mod).Length == 0;
			}
		}
		return false;
	}

	internal static void Install(Harmony harmony)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		MethodInfo methodInfo = null;
		MethodInfo methodInfo2 = AccessTools.Method(typeof(ModTranslations), "AfterResourcePack", (Type[])null, (Type[])null);
		try
		{
			methodInfo = AccessTools.Method(typeof(ModManager), "TryLoadMod", new Type[1] { typeof(Mod) }, (Type[])null) ?? throw new MissingMethodException("ModManager", "TryLoadMod");
			harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(methodInfo2), (HarmonyMethod)null);
			Available = true;
		}
		catch (Exception value2)
		{
			Available = false;
			if (methodInfo != null)
			{
				try
				{
					harmony.Unpatch((MethodBase)methodInfo, methodInfo2);
				}
				catch (Exception value)
				{
					GD.PushWarning($"{"[ModLaunchManager]"} translation hook cleanup failed: {value}");
				}
			}
			GD.PushWarning($"{"[ModLaunchManager]"} custom translation hook unavailable: {value2}");
		}
	}

	internal static IEnumerable<CodeInstruction> AfterResourcePack(IEnumerable<CodeInstruction> instructions)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		List<CodeInstruction> list = instructions.ToList();
		int[] array = (from p in list.Select((CodeInstruction instruction, int index) => (instruction: instruction, index: index))
			where p.instruction.opcode == OpCodes.Call && p.instruction.operand is MethodInfo methodInfo && methodInfo.DeclaringType == typeof(ProjectSettings) && methodInfo.Name == "LoadResourcePack" && methodInfo.ReturnType == typeof(bool)
			select p.index).ToArray();
		if (array.Length != 1)
		{
			throw new InvalidOperationException("Expected exactly one mod resource-pack mount.");
		}
		list.InsertRange(array[0] + 1, new global::_003C_003Ez__ReadOnlyArray<CodeInstruction>((CodeInstruction[])(object)new CodeInstruction[2]
		{
			new CodeInstruction(OpCodes.Ldarg_0, (object)null),
			new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(ModTranslations), "ApplyAfterMount", (Type[])null, (Type[])null))
		}));
		return list;
	}

	internal static bool ApplyAfterMount(bool mounted, Mod mod)
	{
		if (!mounted)
		{
			return false;
		}
		try
		{
			string text = ModTranslationFiles.ModFolder(LauncherDataPaths.ReadDirectory("mod_launch_translations"), Id(mod));
			if (!ModTranslationFiles.HasCustomFiles(text))
			{
				return true;
			}
			Dictionary<string, byte[]> original = PckLocalizationArchive.Read(PackPath(mod), Id(mod));
			Dictionary<string, byte[]> dictionary = ModTranslationFiles.BuildOverlay(text, Id(mod), original, ValidateFormat);
			if (dictionary.Count == 0)
			{
				return true;
			}
			string text2 = Path.Combine(Path.GetTempPath(), "ModLaunchManager", "translations", Path.GetFileName(text) + "-" + Environment.ProcessId + ".zip");
			ModTranslationFiles.WriteOverlay(text2, dictionary);
			if (!ProjectSettings.LoadResourcePack(text2, true, 0))
			{
				throw new IOException("Godot could not mount the translation overlay.");
			}
			GD.Print($"{"[ModLaunchManager]"} applied {dictionary.Count} custom translation file(s) for {Id(mod)}.");
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} translations for {mod.manifest?.id} were not applied: {value}");
			MainMenuNotice.Queue(MlmLocManager.Get("translations.notice.title"), MlmLocManager.Get("translations.notice.failed", ("name", mod.manifest?.name ?? mod.manifest?.id ?? "")));
		}
		return true;
	}

	internal static void ValidateFormat(Dictionary<string, string> table)
	{
		string text5 = default(string);
		foreach (var (text3, text4) in table)
		{
			if (!text3.StartsWith("EXTENSION.", StringComparison.Ordinal) && !LocValidator.ValidateFormatString(text4, ref text5))
			{
				throw new TranslationFileException("invalidFormat", "Invalid format for key '" + text3 + "': " + text5, TranslationFormatIssue.GameFormat);
			}
		}
	}
}
