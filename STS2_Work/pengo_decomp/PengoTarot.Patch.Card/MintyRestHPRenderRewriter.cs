using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace PengoTarot.Patch.Card;

public static class MintyRestHPRenderRewriter
{
	private const string MintyRestHPRenderTypeName = "MintySpire2.MintySpire2Code.RestHPRender";

	private const string HealLabelNodeName = "ModHealPreviewLabel";

	private static MethodInfo? _applyRelicHealModifiers;

	private static Harmony? _harmony;

	private static bool _hasPatched;

	private static int _deferAttempts;

	public static void ScheduleAfterModsLoaded()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Callable val = Callable.From((Action)RunDeferred);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] Could not schedule Minty rest HP compat patch: " + ex.Message);
		}
	}

	private static void RunDeferred()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (_hasPatched)
		{
			return;
		}
		if ((int)ModManager.State != 1)
		{
			if (++_deferAttempts < 300)
			{
				Callable val = Callable.From((Action)RunDeferred);
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
			else
			{
				GD.PrintErr("[PengoTarot] ModManager never Initialized; Minty rest HP compat patch skipped.");
			}
			return;
		}
		_hasPatched = true;
		try
		{
			TryPatch();
		}
		catch (Exception value)
		{
			GD.PrintErr($"[PengoTarot] Minty rest HP compat patch failed: {value}");
		}
	}

	private static void TryPatch()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		Type type = AccessTools.TypeByName("MintySpire2.MintySpire2Code.RestHPRender");
		if (type == null)
		{
			return;
		}
		MethodInfo methodInfo = AccessTools.Method(type, "UpdateExtraLabel", (Type[])null, (Type[])null);
		if (methodInfo == null)
		{
			GD.PrintErr("[PengoTarot] Minty rewriter FAILED: RestHPRender.UpdateExtraLabel not found.");
			return;
		}
		_applyRelicHealModifiers = AccessTools.Method(type, "ApplyRelicHealModifiers", (Type[])null, (Type[])null);
		if (_applyRelicHealModifiers == null)
		{
			GD.PrintErr("[PengoTarot] Minty rewriter FAILED: RestHPRender.ApplyRelicHealModifiers not found.");
			return;
		}
		MethodInfo method = typeof(MintyRestHPRenderRewriter).GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public);
		_harmony = new Harmony("com.pengotarot.mintyresthp");
		_harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, new HarmonyMethod(method), (HarmonyMethod)null, (HarmonyMethod)null);
		GD.Print("[PengoTarot] Minty rewriter OK: patched RestHPRender.UpdateExtraLabel for Devil overflow HP.");
	}

	public static void Postfix(NRestSiteButton button)
	{
		if (!TarDevilDivinationPatch.ShouldApply() || !(button.Option is HealRestSiteOption))
		{
			return;
		}
		Player value = Traverse.Create((object)button.Option).Property("Owner", (object[])null).GetValue<Player>();
		if (value == null || !LocalContext.IsMe(value))
		{
			return;
		}
		Node obj = ((Node)button).FindChild("ModHealPreviewLabel", true, false);
		Label val = (Label)(object)((obj is Label) ? obj : null);
		if (val != null)
		{
			Creature creature = value.Creature;
			int currentHp = creature.CurrentHp;
			decimal num = HealRestSiteOption.GetBaseHealAmount(creature);
			if (_applyRelicHealModifiers != null)
			{
				num = (decimal)_applyRelicHealModifiers.Invoke(null, new object[2] { value, num });
			}
			int value2 = currentHp + Math.Max(0, (int)Math.Floor(num));
			val.Text = $"HP: {currentHp} → {value2}";
		}
	}
}
