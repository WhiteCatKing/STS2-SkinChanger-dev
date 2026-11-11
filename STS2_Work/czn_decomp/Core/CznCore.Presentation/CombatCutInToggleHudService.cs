using System;
using System.Collections.Generic;
using System.Linq;
using CznCore.Settings;
using CznCore.Standalone;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatCutInToggleHudService
{
	private static readonly object Sync = new object();

	private static readonly Dictionary<string, CombatCutInToggleHudController> Controllers = new Dictionary<string, CombatCutInToggleHudController>(StringComparer.Ordinal);

	private static readonly CombatCutInToggleHudProfile SharedProfile = new CombatCutInToggleHudProfile("res://CznCore/images/ui/cut_in_toggle_on.png", "res://CznCore/images/ui/cut_in_toggle_off.png", 64f, -22f, -116f, "CombatCutInToggleHud", "CombatCutInToggleHudIcon", "static_hover_tips", "CZN_CUT_IN_TOGGLE_ENABLED.title", "CZN_CUT_IN_TOGGLE_ENABLED.description", "CZN_CUT_IN_TOGGLE_DISABLED.title", "CZN_CUT_IN_TOGGLE_DISABLED.description");

	public static bool IsRegistered
	{
		get
		{
			lock (Sync)
			{
				return Controllers.Count != 0;
			}
		}
	}

	public static void Register(CombatCutInToggleHudProfile profile, ICombatCutInToggleHudHost host)
	{
		string owner = host.RuntimeNodeName("Owner");
		lock (Sync)
		{
			Controllers.Add(owner, new CombatCutInToggleHudController(profile, host));
		}
		CharacterSkinModBootstrap.TrackInitializationRollback(delegate
		{
			Unregister(owner);
		});
	}

	public static void Register(CombatCutInToggleHudProfile profile, Func<Player?, bool> isTargetPlayer, Func<bool> isAvailable, Func<bool> isCutInEnabled, Action<bool, bool> setCutInEnabled, Func<string, string> runtimeNodeName, Action<string, int> warn, Action<string, int>? debug = null)
	{
		Register(profile, new DelegateCombatCutInToggleHudHost(isTargetPlayer, isAvailable, isCutInEnabled, setCutInEnabled, runtimeNodeName, warn, debug));
	}

	public static void RegisterCharacter(SkinCharacterSettings settings, Func<Player?, bool> isTargetPlayer, Func<bool> isAvailable, Func<string, string> runtimeNodeName, Action<string, int> warn, Action<string, int>? debug = null)
	{
		Register(SharedProfile, isTargetPlayer, isAvailable, () => settings.ShowCardPlayPresentation, delegate(bool enabled, bool persist)
		{
			settings.SetCombatPresentationMode(CombatPresentationMode.Custom, persist);
			settings.SetShowCardPlayPresentation(enabled, persist);
		}, runtimeNodeName, warn, debug);
		PresentationTransports.EnsureTypes(CombatCutInToggleHudPatches.PatchTypes);
	}

	public static void Activate(NCombatUi ui, CombatState state)
	{
		ForEach(delegate(CombatCutInToggleHudController controller)
		{
			controller.Activate(ui, state);
		});
	}

	public static void Enable(NCombatUi ui)
	{
		ForEach(delegate(CombatCutInToggleHudController controller)
		{
			controller.SetCombatUiEnabled(enabled: true, ui);
		});
	}

	public static void Disable()
	{
		ForEach(delegate(CombatCutInToggleHudController controller)
		{
			controller.SetCombatUiEnabled(enabled: false);
		});
	}

	public static void Deactivate()
	{
		ForEach(delegate(CombatCutInToggleHudController controller)
		{
			controller.Cleanup();
		});
	}

	internal static void Unregister(string owner)
	{
		CombatCutInToggleHudController value;
		lock (Sync)
		{
			if (!Controllers.Remove(owner, out value))
			{
				return;
			}
		}
		value.Cleanup();
	}

	private static void ForEach(Action<CombatCutInToggleHudController> action)
	{
		KeyValuePair<string, CombatCutInToggleHudController>[] array;
		lock (Sync)
		{
			array = Controllers.ToArray();
		}
		KeyValuePair<string, CombatCutInToggleHudController>[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			KeyValuePair<string, CombatCutInToggleHudController> entry = array2[i];
			SkinPatchGuard.Run(entry.Key + " HUD", delegate
			{
				action(entry.Value);
			});
		}
	}
}
