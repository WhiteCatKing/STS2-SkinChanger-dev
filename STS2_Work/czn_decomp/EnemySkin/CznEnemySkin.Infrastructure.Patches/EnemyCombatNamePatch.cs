using System;
using System.Reflection;
using CznEnemySkin.Domain;
using CznEnemySkin.Localization;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.addons.mega_text;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCreatureStateDisplay), "RefreshValues")]
internal static class EnemyCombatNamePatch
{
	private const string SubtitleNodeName = "CznOriginalEnemyName";

	private const float NativeNameplateHeight = 28f;

	private const float TwoLineNameplateHeight = 48f;

	private static readonly FieldInfo? CreatureField = AccessTools.Field(typeof(NCreatureStateDisplay), "_creature");

	private static readonly FieldInfo? NameplateLabelField = AccessTools.Field(typeof(NCreatureStateDisplay), "_nameplateLabel");

	private static readonly FieldInfo? NameplateContainerField = AccessTools.Field(typeof(NCreatureStateDisplay), "_nameplateContainer");

	private static bool _reportedMissingFields;

	private static void Postfix(NCreatureStateDisplay __instance)
	{
		try
		{
			if (CreatureField == null || NameplateLabelField == null || NameplateContainerField == null)
			{
				ReportMissingFieldsOnce();
				return;
			}
			object? value = NameplateLabelField.GetValue(__instance);
			MegaLabel val = (MegaLabel)((value is MegaLabel) ? value : null);
			if (val == null)
			{
				return;
			}
			object? value2 = NameplateContainerField.GetValue(__instance);
			Control val2 = (Control)((value2 is Control) ? value2 : null);
			if (val2 == null)
			{
				return;
			}
			object? value3 = CreatureField.GetValue(__instance);
			Creature val3 = (Creature)((value3 is Creature) ? value3 : null);
			if (val3 != null)
			{
				MonsterModel monster = val3.Monster;
				if (monster != null && Entry.Replacements.TryResolveMonster(((AbstractModel)monster).Id.Entry, out EnemyReplacementProfile profile))
				{
					if (EnemyLocalization.TryGetEnemyName(profile, out string name))
					{
						val.SetTextAutoSize(name);
						UpdateOriginalNameSubtitle(__instance, val2, val, name);
					}
					else
					{
						RestoreNativeLayoutIfOwned(val2, val);
					}
					return;
				}
			}
			RestoreNativeLayoutIfOwned(val2, val);
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not update enemy combat display name: " + ex.Message, 1);
		}
	}

	private static void UpdateOriginalNameSubtitle(NCreatureStateDisplay display, Control container, MegaLabel replacementLabel, string replacementName)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I8
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		string text = string.Empty;
		if (((GodotObject)display).HasMeta(StringName.op_Implicit("czn_enemy_skin_original_combat_name")))
		{
			Variant meta = ((GodotObject)display).GetMeta(StringName.op_Implicit("czn_enemy_skin_original_combat_name"), default(Variant));
			if ((long)((Variant)(ref meta)).VariantType == 4)
			{
				text = ((Variant)(ref meta)).AsString();
			}
		}
		bool flag = Entry.PresentationSettings.ShowOriginalEnemyName && !string.IsNullOrWhiteSpace(text) && !string.Equals(text, replacementName, StringComparison.CurrentCulture);
		MegaLabel val = EnsureSubtitle(container, replacementLabel);
		((CanvasItem)val).Visible = flag;
		container.Size = new Vector2(container.Size.X, flag ? 48f : 28f);
		if (!flag)
		{
			RestoreNativeLayout(container, replacementLabel);
			return;
		}
		((Control)replacementLabel).AnchorTop = 0f;
		((Control)replacementLabel).AnchorBottom = 0f;
		((Control)replacementLabel).OffsetTop = -5f;
		((Control)replacementLabel).OffsetBottom = 25f;
		val.SetTextAutoSize("(" + text + ")");
	}

	private static void RestoreNativeLayout(Control container, MegaLabel replacementLabel)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		container.Size = new Vector2(container.Size.X, 28f);
		((Control)replacementLabel).AnchorTop = 0f;
		((Control)replacementLabel).AnchorBottom = 1f;
		((Control)replacementLabel).OffsetTop = -12f;
		((Control)replacementLabel).OffsetBottom = 12f;
		MegaLabel nodeOrNull = ((Node)container).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("CznOriginalEnemyName"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Visible = false;
		}
	}

	private static void RestoreNativeLayoutIfOwned(Control container, MegaLabel replacementLabel)
	{
		if (((Node)container).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("CznOriginalEnemyName")) != null)
		{
			RestoreNativeLayout(container, replacementLabel);
		}
	}

	private static MegaLabel EnsureSubtitle(Control container, MegaLabel replacementLabel)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		MegaLabel nodeOrNull = ((Node)container).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("CznOriginalEnemyName"));
		if (nodeOrNull != null)
		{
			return nodeOrNull;
		}
		MegaLabel val = new MegaLabel
		{
			Name = StringName.op_Implicit("CznOriginalEnemyName"),
			MouseFilter = (MouseFilterEnum)2,
			AnchorRight = 1f,
			OffsetLeft = -5f,
			OffsetTop = 24f,
			OffsetRight = 5f,
			OffsetBottom = 48f,
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1,
			MinFontSize = 9,
			MaxFontSize = 14
		};
		((Control)val).AddThemeFontOverride(Label.Font, ((Control)replacementLabel).GetThemeFont(Label.Font, (StringName)null));
		((Control)val).AddThemeFontSizeOverride(Label.FontSize, 14);
		((Control)val).AddThemeColorOverride(Label.FontColor, new Color(0.72f, 0.72f, 0.72f, 0.92f));
		((Control)val).AddThemeColorOverride(Label.FontShadowColor, new Color(0f, 0f, 0f, 0.45f));
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("shadow_offset_x"), 1);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("shadow_offset_y"), 1);
		((Node)container).AddChild((Node)(object)val, false, (InternalMode)0);
		return val;
	}

	private static void ReportMissingFieldsOnce()
	{
		if (!_reportedMissingFields)
		{
			_reportedMissingFields = true;
			Entry.Log.Warn("Enemy combat name adapter is unavailable because the STS2 nameplate fields changed.", 1);
		}
	}
}
