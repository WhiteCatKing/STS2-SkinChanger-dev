using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch;

public static class HextechWarningPatch
{
	[HarmonyPatch(typeof(NCharacterSelectScreen), "AfterInitialized")]
	public static class CharacterSelect_AfterInitialized_Patch
	{
		private static void Postfix()
		{
			bool flag = IsHextechLoaded();
			bool hextechWarningDismissed = ConfigFloatingWindowConfig.HextechWarningDismissed;
			bool flag2 = flag && IsResolvedHextechVersionLoaded();
			bool flag3 = flag2 && ConfigFloatingWindowConfig.HasAcknowledgedHextechWarning && !ConfigFloatingWindowConfig.HextechResolutionDismissed;
			bool flag4 = flag && !flag2 && !hextechWarningDismissed;
			GD.Print($"[PengoTarot] HextechWarning: AfterInitialized postfix. showing={_showing}, hextechLoaded={flag}, resolvedVersion={flag2}, dismissed={hextechWarningDismissed}, showResolution={flag3}");
			if (!_showing && (flag4 || flag3))
			{
				_showing = true;
				if (!flag3)
				{
					ShowWarningAsync();
				}
				else
				{
					ShowResolutionAsync();
				}
			}
		}
	}

	private static readonly string[] HextechModIds = new string[2] { "HextechRunes", "HextechRunesSponsorPack" };

	private static readonly Version HextechResolutionVersion = new Version(0, 9, 2);

	private static bool _showing;

	public static bool IsHextechLoaded()
	{
		try
		{
			return ModManager.Mods.Any((Mod mod) => (int)mod.state == 1 && mod.manifest?.id != null && HextechModIds.Contains<string>(mod.manifest.id, StringComparer.OrdinalIgnoreCase));
		}
		catch
		{
			return false;
		}
	}

	private static bool IsResolvedHextechVersionLoaded()
	{
		try
		{
			List<Mod> list = ModManager.Mods.Where((Mod mod) => (int)mod.state == 1 && mod.manifest?.id != null && HextechModIds.Contains<string>(mod.manifest.id, StringComparer.OrdinalIgnoreCase)).ToList();
			if (list.Count == 0)
			{
				return false;
			}
			return list.All(delegate(Mod mod)
			{
				string text = mod.manifest.version?.Trim() ?? string.Empty;
				if (text.StartsWith("v", StringComparison.OrdinalIgnoreCase))
				{
					string text2 = text;
					text = text2.Substring(1, text2.Length - 1);
				}
				int num = text.IndexOfAny(new char[2] { '-', '+' });
				if (num >= 0)
				{
					text = text.Substring(0, num);
				}
				Version result;
				return Version.TryParse(text, out result) && result >= HextechResolutionVersion;
			});
		}
		catch
		{
			return false;
		}
	}

	private static async Task ShowResolutionAsync()
	{
		try
		{
			if (NModalContainer.Instance != null)
			{
				await ShowPageAsync("HEXTECH_WARNING_TITLE", "HEXTECH_WARNING_RESOLUTION", null, "HEXTECH_WARNING_ACK");
				ConfigFloatingWindowConfig.SetHextechResolutionDismissed();
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] Hextech resolution popup failed: " + ex.Message);
		}
		finally
		{
			_showing = false;
		}
	}

	private static async Task ShowWarningAsync()
	{
		_ = 6;
		try
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (tree == null || NModalContainer.Instance == null || !(await ShowPageAsync("HEXTECH_WARNING_TITLE", "HEXTECH_WARNING_PAGE1", null, "HEXTECH_WARNING_NEXT")))
			{
				return;
			}
			await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
			if (await ShowPageAsync("HEXTECH_WARNING_TITLE", "HEXTECH_WARNING_PAGE2", null, "HEXTECH_WARNING_NEXT"))
			{
				await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				if (await ShowPageAsync("HEXTECH_WARNING_TITLE", "HEXTECH_WARNING_PAGE3", null, "HEXTECH_WARNING_NEXT"))
				{
					await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
					await ShowPageAsync("HEXTECH_WARNING_TITLE", "HEXTECH_WARNING_PAGE4", null, "HEXTECH_WARNING_ACK");
					ConfigFloatingWindowConfig.SetHextechWarningDismissed(value: true);
				}
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] Hextech warning popup failed: " + ex.Message);
		}
		finally
		{
			_showing = false;
		}
	}

	private static async Task<bool> ShowPageAsync(string titleKey, string bodyKey, string? noButtonKey, string yesButtonKey)
	{
		PackedScene val = GD.Load<PackedScene>("res://scenes/ui/generic_popup.tscn");
		if (val == null)
		{
			return false;
		}
		NGenericPopup val2 = val.Instantiate<NGenericPopup>((GenEditState)0);
		if (val2 == null)
		{
			return false;
		}
		NModalContainer instance = NModalContainer.Instance;
		if (instance == null)
		{
			return false;
		}
		instance.Add((Node)(object)val2, true);
		return await val2.WaitForConfirmation(new LocString("main_menu_ui", bodyKey), new LocString("main_menu_ui", titleKey), (noButtonKey == null) ? ((LocString)null) : new LocString("main_menu_ui", noButtonKey), new LocString("main_menu_ui", yesButtonKey));
	}
}
