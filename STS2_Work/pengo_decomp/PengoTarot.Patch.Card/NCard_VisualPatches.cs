using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.addons.mega_text;
using PengoTarot.Cards;
using PengoTarot.Patch.VisualVanilla;

namespace PengoTarot.Patch.Card;

[HarmonyPatch]
public static class NCard_VisualPatches
{
	private const float BannerTop = -207f;

	private const float BannerBottom = -124f;

	private const float BannerLeft = -163f;

	private const float BannerRight = 164f;

	private const float TitleTop = -204f;

	private const float TitleBottom = -150f;

	private const float TitleLeft = -105f;

	private const float TitleRight = 105f;

	private const float DescTop = 37f;

	private const float DescBottom = 173f;

	private const float TextBgTop = -22f;

	private const float TextBgBottom = 181f;

	private const float TextBgLeft = -133f;

	private const float TextBgRight = 131f;

	private const float PortraitLeft = -153f;

	private const float PortraitRight = 445f;

	private const float BorderLeft = -154f;

	private const float BorderRight = 152f;

	private const float BorderGlassLeft = -148.465f;

	private const float BorderGlassRight = 442.08f;

	[HarmonyPatch(typeof(NCard), "Reload")]
	[HarmonyPostfix]
	private static void Reload_Postfix(NCard __instance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			ApplyVisuals(__instance);
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
	[HarmonyPostfix]
	private static void UpdateVisuals_Postfix(NCard __instance)
	{
		ApplyVisuals(__instance);
	}

	[HarmonyPatch(typeof(NCard), "OnFreedToPool")]
	[HarmonyPostfix]
	private static void OnFreedToPool_Postfix(NCard __instance)
	{
		ResetPengoTarotVisuals(__instance);
	}

	private static void ApplyVisuals(NCard __instance)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)__instance))
		{
			return;
		}
		CardModel model = __instance.Model;
		if (model != null)
		{
			if (model is TarCard)
			{
				ResetPengoTarotVisuals(__instance);
				ApplyTarCard(__instance, model);
			}
			else if (model is PlanetCard)
			{
				ResetPengoTarotVisuals(__instance);
				ApplyPlanetCard(__instance);
			}
		}
	}

	public static void ResetPengoTarotVisuals(NCard __instance)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)__instance))
		{
			Node obj = ((Node)__instance).FindChild("TypePlaque", true, false);
			NinePatchRect val = (NinePatchRect)(object)((obj is NinePatchRect) ? obj : null);
			if (val != null)
			{
				((CanvasItem)val).Visible = true;
			}
			Node obj2 = ((Node)__instance).FindChild("AncientBanner", true, false);
			Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
			if (val2 != null)
			{
				((CanvasItem)val2).Visible = true;
				((CanvasItem)val2).Modulate = Colors.White;
				val2.OffsetTop = -207f;
				val2.OffsetBottom = -124f;
				val2.OffsetLeft = -163f;
				val2.OffsetRight = 164f;
			}
			Node obj3 = ((Node)__instance).FindChild("TitleLabel", true, false);
			Label val3 = (Label)(object)((obj3 is Label) ? obj3 : null);
			if (val3 != null)
			{
				((CanvasItem)val3).Visible = true;
				((CanvasItem)val3).Modulate = Colors.White;
				((CanvasItem)val3).ZIndex = 0;
				((Control)val3).OffsetTop = -204f;
				((Control)val3).OffsetBottom = -150f;
			}
			Node obj4 = ((Node)__instance).FindChild("AncientTextBg", true, false);
			TextureRect val4 = (TextureRect)(object)((obj4 is TextureRect) ? obj4 : null);
			if (val4 != null)
			{
				((Control)val4).PivotOffset = Vector2.Zero;
				((Control)val4).Rotation = 0f;
				((Control)val4).OffsetTop = -22f;
				((Control)val4).OffsetBottom = 181f;
				((Control)val4).OffsetLeft = -133f;
				((Control)val4).OffsetRight = 131f;
			}
			Node obj5 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
			RichTextLabel val5 = (RichTextLabel)(object)((obj5 is RichTextLabel) ? obj5 : null);
			if (val5 != null)
			{
				((Control)val5).OffsetTop = 37f;
				((Control)val5).OffsetBottom = 173f;
				val5.VerticalAlignment = (VerticalAlignment)1;
				((CanvasItem)val5).ZIndex = 0;
			}
			Node obj6 = ((Node)__instance).FindChild("AncientPortrait", true, false);
			TextureRect val6 = (TextureRect)(object)((obj6 is TextureRect) ? obj6 : null);
			if (val6 != null)
			{
				((Control)val6).OffsetLeft = -153f;
				((Control)val6).OffsetRight = 445f;
			}
			Node obj7 = ((Node)__instance).FindChild("AncientBorder", true, false);
			TextureRect val7 = (TextureRect)(object)((obj7 is TextureRect) ? obj7 : null);
			if (val7 != null)
			{
				((Control)val7).OffsetLeft = -154f;
				((Control)val7).OffsetRight = 152f;
			}
			Node obj8 = ((Node)__instance).FindChild("AncientBorderGlassOverlay", true, false);
			TextureRect val8 = (TextureRect)(object)((obj8 is TextureRect) ? obj8 : null);
			if (val8 != null)
			{
				((Control)val8).OffsetLeft = -148.465f;
				((Control)val8).OffsetRight = 442.08f;
			}
		}
	}

	private static void ResetTextZIndex(NCard __instance)
	{
		Node obj = ((Node)__instance).FindChild("TitleLabel", true, false);
		Control val = (Control)(object)((obj is Control) ? obj : null);
		if (val != null)
		{
			((CanvasItem)val).ZIndex = 0;
		}
		Node obj2 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
		Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
		if (val2 != null)
		{
			((CanvasItem)val2).ZIndex = 0;
		}
	}

	private static void ApplyTarCard(NCard __instance, CardModel model)
	{
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		bool tarotVanilla = VanillaStyleConfig.TarotVanilla;
		bool flag = ((object)model).GetType().Name.Contains("Reversed");
		Node obj = ((Node)__instance).FindChild("TypePlaque", true, false);
		NinePatchRect val = (NinePatchRect)(object)((obj is NinePatchRect) ? obj : null);
		if (val != null)
		{
			((CanvasItem)val).Visible = false;
		}
		if (tarotVanilla)
		{
			Node obj2 = ((Node)__instance).FindChild("AncientBanner", true, false);
			Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
			if (val2 != null)
			{
				((CanvasItem)val2).Modulate = Colors.White;
				val2.OffsetTop = -215f;
				val2.OffsetBottom = -132f;
			}
			Node obj3 = ((Node)__instance).FindChild("TitleLabel", true, false);
			Label val3 = (Label)(object)((obj3 is Label) ? obj3 : null);
			if (val3 != null)
			{
				((CanvasItem)val3).Modulate = Colors.White;
				((Control)val3).OffsetTop = -209f;
				((Control)val3).OffsetBottom = -155f;
			}
			if (flag)
			{
				Node obj4 = ((Node)__instance).FindChild("AncientTextBg", true, false);
				TextureRect val4 = (TextureRect)(object)((obj4 is TextureRect) ? obj4 : null);
				if (val4 != null)
				{
					((Control)val4).PivotOffset = Vector2.Zero;
					((Control)val4).Rotation = 0f;
					((CanvasItem)val4).Visible = true;
					((Control)val4).OffsetTop = -22f;
					((Control)val4).OffsetBottom = 205f;
				}
				Node obj5 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
				RichTextLabel val5 = (RichTextLabel)(object)((obj5 is RichTextLabel) ? obj5 : null);
				if (val5 != null)
				{
					((Control)val5).OffsetTop = 37f;
					((Control)val5).OffsetBottom = 173f;
					val5.VerticalAlignment = (VerticalAlignment)1;
				}
			}
			else
			{
				Node obj6 = ((Node)__instance).FindChild("AncientTextBg", true, false);
				TextureRect val6 = (TextureRect)(object)((obj6 is TextureRect) ? obj6 : null);
				if (val6 != null)
				{
					((CanvasItem)val6).Visible = true;
					((Control)val6).PivotOffset = Vector2.Zero;
					((Control)val6).Rotation = 0f;
					((Control)val6).OffsetTop = -22f;
					((Control)val6).OffsetBottom = 205f;
				}
				Node obj7 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
				RichTextLabel val7 = (RichTextLabel)(object)((obj7 is RichTextLabel) ? obj7 : null);
				if (val7 != null)
				{
					((Control)val7).OffsetTop = 37f;
					((Control)val7).OffsetBottom = 173f;
					val7.VerticalAlignment = (VerticalAlignment)1;
				}
			}
			return;
		}
		Node obj8 = ((Node)__instance).FindChild("AncientBanner", true, false);
		Control val8 = (Control)(object)((obj8 is Control) ? obj8 : null);
		if (val8 != null)
		{
			((CanvasItem)val8).Modulate = Colors.Transparent;
		}
		Node obj9 = ((Node)__instance).FindChild("TitleLabel", true, false);
		Label val9 = (Label)(object)((obj9 is Label) ? obj9 : null);
		if (val9 != null)
		{
			((CanvasItem)val9).Modulate = Colors.Transparent;
		}
		if (flag)
		{
			Node obj10 = ((Node)__instance).FindChild("AncientTextBg", true, false);
			TextureRect val10 = (TextureRect)(object)((obj10 is TextureRect) ? obj10 : null);
			if (val10 != null)
			{
				float num = 264f;
				float num2 = 203f;
				((Control)val10).PivotOffset = new Vector2(num / 2f, num2 / 2f);
				((Control)val10).Rotation = (float)Math.PI;
				((CanvasItem)val10).Visible = true;
				((Control)val10).OffsetTop = -211f;
				((Control)val10).OffsetBottom = -5f;
			}
			Node obj11 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
			RichTextLabel val11 = (RichTextLabel)(object)((obj11 is RichTextLabel) ? obj11 : null);
			if (val11 != null)
			{
				((Control)val11).OffsetTop = -211f;
				((Control)val11).OffsetBottom = -40f;
			}
		}
		else
		{
			Node obj12 = ((Node)__instance).FindChild("AncientTextBg", true, false);
			TextureRect val12 = (TextureRect)(object)((obj12 is TextureRect) ? obj12 : null);
			if (val12 != null)
			{
				((CanvasItem)val12).Visible = true;
				((Control)val12).PivotOffset = Vector2.Zero;
				((Control)val12).Rotation = 0f;
				((Control)val12).OffsetTop = -22f;
				((Control)val12).OffsetBottom = 205f;
			}
			Node obj13 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
			RichTextLabel val13 = (RichTextLabel)(object)((obj13 is RichTextLabel) ? obj13 : null);
			if (val13 != null)
			{
				((Control)val13).OffsetTop = 37f;
				((Control)val13).OffsetBottom = 173f;
				val13.VerticalAlignment = (VerticalAlignment)1;
			}
		}
	}

	private static void ApplyPlanetCard(NCard __instance)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		bool planetVanilla = VanillaStyleConfig.PlanetVanilla;
		Node obj = ((Node)__instance).FindChild("TypePlaque", true, false);
		NinePatchRect val = (NinePatchRect)(object)((obj is NinePatchRect) ? obj : null);
		if (val != null)
		{
			((CanvasItem)val).Visible = false;
		}
		Node obj2 = ((Node)__instance).FindChild("AncientBanner", true, false);
		Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
		if (val2 != null)
		{
			((CanvasItem)val2).Modulate = (planetVanilla ? Colors.White : Colors.Transparent);
		}
		Node obj3 = ((Node)__instance).FindChild("TitleLabel", true, false);
		Label val3 = (Label)(object)((obj3 is Label) ? obj3 : null);
		if (val3 != null)
		{
			((CanvasItem)val3).Modulate = (planetVanilla ? Colors.White : Colors.Transparent);
		}
		Node obj4 = ((Node)__instance).FindChild("AncientPortrait", true, false);
		TextureRect val4 = (TextureRect)(object)((obj4 is TextureRect) ? obj4 : null);
		if (val4 != null)
		{
			((Control)val4).OffsetLeft = -152.5f;
			((Control)val4).OffsetRight = 445.5f;
		}
		Node obj5 = ((Node)__instance).FindChild("AncientBorder", true, false);
		TextureRect val5 = (TextureRect)(object)((obj5 is TextureRect) ? obj5 : null);
		if (val5 != null)
		{
			((Control)val5).OffsetLeft = -165f;
			((Control)val5).OffsetRight = 164f;
		}
		Node obj6 = ((Node)__instance).FindChild("AncientBorderGlassOverlay", true, false);
		TextureRect val6 = (TextureRect)(object)((obj6 is TextureRect) ? obj6 : null);
		if (val6 != null)
		{
			((Control)val6).OffsetLeft = -165f;
			((Control)val6).OffsetRight = 493f;
		}
		Node obj7 = ((Node)__instance).FindChild("AncientTextBg", true, false);
		TextureRect val7 = (TextureRect)(object)((obj7 is TextureRect) ? obj7 : null);
		if (val7 != null)
		{
			((CanvasItem)val7).Visible = true;
			((Control)val7).PivotOffset = Vector2.Zero;
			((Control)val7).Rotation = 0f;
			((CanvasItem)val7).Modulate = new Color(0f, 0f, 0f, 0.5f);
			((Control)val7).OffsetLeft = -165f;
			((Control)val7).OffsetRight = 164f;
			((Control)val7).OffsetTop = 5f;
			((Control)val7).OffsetBottom = 211f;
		}
		Node obj8 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
		RichTextLabel val8 = (RichTextLabel)(object)((obj8 is RichTextLabel) ? obj8 : null);
		if (val8 != null)
		{
			((Control)val8).OffsetTop = 52f;
			((Control)val8).OffsetBottom = 188f;
		}
	}

	private static void SetTextAboveOverlay(NCard __instance)
	{
		int zIndex = 1;
		Node obj = ((Node)__instance).FindChild("OverlayContainer", true, false);
		Control val = (Control)(object)((obj is Control) ? obj : null);
		if (val != null)
		{
			zIndex = ((CanvasItem)val).ZIndex + 1;
		}
		Node obj2 = ((Node)__instance).FindChild("TitleLabel", true, false);
		Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
		if (val2 != null)
		{
			((CanvasItem)val2).ZIndex = zIndex;
		}
		Node obj3 = ((Node)__instance).FindChild("DescriptionLabel", true, false);
		Control val3 = (Control)(object)((obj3 is Control) ? obj3 : null);
		if (val3 != null)
		{
			((CanvasItem)val3).ZIndex = zIndex;
		}
	}

	private static void ApplyTextShadow(NCard __instance)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Node obj = ((Node)__instance).FindChild("DescriptionLabel", true, false);
		RichTextLabel val = (RichTextLabel)(object)((obj is RichTextLabel) ? obj : null);
		if (val != null)
		{
			((Control)val).AddThemeColorOverride(RichTextLabel.FontShadowColor, new Color(0f, 0f, 0f, 0.85f));
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("shadow_outline_size"), 8);
		}
	}

	[HarmonyPatch(typeof(NInspectCardScreen), "SetCard")]
	[HarmonyPrefix]
	private static void SetCard_Prefix(NInspectCardScreen __instance, int index)
	{
		object? obj = AccessTools.Field(typeof(NInspectCardScreen), "_card")?.GetValue(__instance);
		NCard val = (NCard)((obj is NCard) ? obj : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && val.Model != null)
		{
			CardModel model = val.Model;
			if (model is TarCard || model is PlanetCard)
			{
				ResetPengoTarotVisuals(val);
			}
		}
	}
}
