using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Debug;
using MegaCrit.Sts2.addons.mega_text;

namespace ModLaunchManager;

internal static class MainMenuModWarningPatch
{
	private sealed record BaselineColor(Color Modulate)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Modulate = ");
			Color modulate = Modulate;
			builder.Append(((object)(Color)(ref modulate)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	private static readonly ConditionalWeakTable<NDebugInfoLabelManager, BaselineColor> BaselineColors = new ConditionalWeakTable<NDebugInfoLabelManager, BaselineColor>();

	private static readonly FieldInfo? ModdedWarningField = AccessTools.Field(typeof(NDebugInfoLabelManager), "_moddedWarning");

	private static readonly FieldInfo? ModWarningLabelField = AccessTools.Field(typeof(NDebugInfoLabelManager), "_modWarningLabel");

	internal static bool Install(Harmony harmony)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		//IL_00ad: Expected O, but got Unknown
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(NDebugInfoLabelManager), "UpdateText", new Type[1] { typeof(string) }, (Type[])null) ?? throw new MissingMethodException("NDebugInfoLabelManager", "UpdateText");
			if (ModdedWarningField == null || ModWarningLabelField == null)
			{
				throw new MissingFieldException("NDebugInfoLabelManager", "_moddedWarning or _modWarningLabel");
			}
			harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(AccessTools.Method(typeof(MainMenuModWarningPatch), "RememberBaselineColor", (Type[])null, (Type[])null)), new HarmonyMethod(AccessTools.Method(typeof(MainMenuModWarningPatch), "RewriteMainMenuWarning", (Type[])null, (Type[])null))
			{
				priority = 0
			}, (HarmonyMethod)null, (HarmonyMethod)null);
			return true;
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not patch the main-menu mod warning: {value}");
			return false;
		}
	}

	private static void RememberBaselineColor(NDebugInfoLabelManager __instance)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		object? obj = ModdedWarningField?.GetValue(__instance);
		MegaLabel val = (MegaLabel)((obj is MegaLabel) ? obj : null);
		if (val != null && !BaselineColors.TryGetValue(__instance, out BaselineColor _))
		{
			BaselineColors.Add(__instance, new BaselineColor(((CanvasItem)val).Modulate));
		}
	}

	private static void RewriteMainMenuWarning(NDebugInfoLabelManager __instance)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		if (!__instance.isMainMenu)
		{
			return;
		}
		object? obj = ModdedWarningField?.GetValue(__instance);
		MegaLabel val = (MegaLabel)((obj is MegaLabel) ? obj : null);
		if (val == null)
		{
			return;
		}
		object? obj2 = ModWarningLabelField?.GetValue(__instance);
		MegaRichTextLabel val2 = (MegaRichTextLabel)((obj2 is MegaRichTextLabel) ? obj2 : null);
		if (val2 == null)
		{
			return;
		}
		List<Mod> list = ModManager.GetLoadedMods().ToList();
		List<Mod> source = ModManager.Mods.Where(ShouldDisplayErrors).ToList();
		bool flag = source.Any(delegate(Mod mod)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)mod.state != 2)
			{
				List<LocString> errors = mod.errors;
				if (errors != null)
				{
					return errors.Count > 0;
				}
				return false;
			}
			return true;
		});
		LocString val3 = new LocString("main_menu_ui", "MODDED_WARNING");
		val3.Add("count", (decimal)list.Count);
		val3.Add("hasError", flag);
		val.SetTextAutoSize(val3.GetFormattedText());
		LocString[] array = source.SelectMany((Mod mod) => mod.errors ?? new List<LocString>()).ToArray();
		if (array.Length != 0)
		{
			val2.Text = string.Join('\n', array.Select((LocString error) => error.GetFormattedText()));
		}
		else
		{
			LocString val4 = new LocString("main_menu_ui", "MOD_ERROR.NONE");
			val4.Add("mods", string.Join(", ", list.Select(delegate(Mod mod)
			{
				object obj3 = mod.manifest?.name;
				if (obj3 == null)
				{
					ModManifest manifest = mod.manifest;
					if (manifest == null)
					{
						return (string)null;
					}
					obj3 = manifest.id;
				}
				return (string)obj3;
			})));
			val2.Text = val4.GetFormattedText();
		}
		((CanvasItem)val).Modulate = (flag ? StsColors.redGlow : BaselineColors.GetValue(__instance, (NDebugInfoLabelManager _) => new BaselineColor(Colors.White)).Modulate);
	}

	internal static bool ShouldDisplayErrors(Mod mod)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		ModLoadState state = mod.state;
		if (state - 1 <= 1)
		{
			return true;
		}
		return false;
	}
}
