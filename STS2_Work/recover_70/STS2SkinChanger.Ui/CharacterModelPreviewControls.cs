using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CharacterModelPreviewControls
{
	private const string PanelName = "STS2CharacterModelPreview";

	internal const float GripWidth = 12f;

	internal const float Gap = 1f;

	internal const float ModelInset = 13f;

	internal static bool IsEnabled => ShouldShow(ModManager.Mods, FrameworkRegistryCooperation.IsActive);

	internal static bool ShouldShow(IEnumerable<Mod> mods, bool nativeCooperationActive)
	{
		if (!nativeCooperationActive)
		{
			return !mods.Any((Mod mod) => (int)mod.state == 1 && FrameworkCompatibilityLayer.IsKnownFrameworkHost(mod.manifest?.id));
		}
		return false;
	}

	internal static Rect2? ResolveLayout(Rect2 infoFrame, Rect2 screen)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (((Rect2)(ref infoFrame)).HasArea() && ((Rect2)(ref screen)).HasArea())
		{
			Vector2 val = ((Rect2)(ref infoFrame)).Position;
			if (((Vector2)(ref val)).IsFinite())
			{
				val = ((Rect2)(ref infoFrame)).Size;
				if (((Vector2)(ref val)).IsFinite())
				{
					val = ((Rect2)(ref screen)).Position;
					if (((Vector2)(ref val)).IsFinite())
					{
						val = ((Rect2)(ref screen)).Size;
						if (((Vector2)(ref val)).IsFinite())
						{
							float num = ((Rect2)(ref infoFrame)).End.X + 1f;
							float num2 = Math.Min(((Rect2)(ref infoFrame)).Size.Y * (28f / 39f) + 13f, ((Rect2)(ref screen)).End.X - 24f - num);
							if (!(num2 < 77f))
							{
								return new Rect2(num, ((Rect2)(ref infoFrame)).Position.Y, num2, ((Rect2)(ref infoFrame)).Size.Y);
							}
							return null;
						}
					}
				}
			}
		}
		return null;
	}

	internal static Rect2? ResolveDockArea(Rect2 infoFrame)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (((Rect2)(ref infoFrame)).HasArea())
		{
			Vector2 val = ((Rect2)(ref infoFrame)).Position;
			if (((Vector2)(ref val)).IsFinite())
			{
				val = ((Rect2)(ref infoFrame)).Size;
				if (((Vector2)(ref val)).IsFinite())
				{
					float num = 38f;
					return new Rect2(((Rect2)(ref infoFrame)).Position.X - 1f - num, ((Rect2)(ref infoFrame)).Position.Y + ((Rect2)(ref infoFrame)).Size.Y * 0.1f, num, ((Rect2)(ref infoFrame)).Size.Y * 0.8f);
				}
			}
		}
		return null;
	}

	internal static Rect2? ResolveDockLayout(Rect2 infoFrame)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		Rect2? val = ResolveDockArea(infoFrame);
		if (val.HasValue)
		{
			Rect2 valueOrDefault = val.GetValueOrDefault();
			return new Rect2(((Rect2)(ref valueOrDefault)).GetCenter().X - 6f, ((Rect2)(ref infoFrame)).Position.Y, ((Rect2)(ref infoFrame)).Size.Y * (28f / 39f) + 13f, ((Rect2)(ref infoFrame)).Size.Y);
		}
		return null;
	}

	internal static bool IsGripDocked(Rect2 infoFrame, Vector2 gripCenter)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (((Vector2)(ref gripCenter)).IsFinite())
		{
			Rect2? val = ResolveDockArea(infoFrame);
			if (val.HasValue)
			{
				Rect2 valueOrDefault = val.GetValueOrDefault();
				return ((Rect2)(ref valueOrDefault)).HasPoint(gripCenter);
			}
		}
		return false;
	}

	internal static Rect2? ResolveDockHint(Rect2 infoFrame, bool dragging, bool visible)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!(dragging && visible))
		{
			return null;
		}
		return ResolveDockArea(infoFrame);
	}

	internal static bool ShouldLoadModel(bool docked, bool hasCharacter, bool enabled, bool visible)
	{
		return !docked && hasCharacter && enabled && visible;
	}

	public static void Refresh(NCharacterSelectScreen screen, CharacterModel character)
	{
		if (!IsEnabled)
		{
			Hide(screen);
			return;
		}
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("InfoPanel"));
		if (nodeOrNull != null)
		{
			CharacterModelPreviewPanel characterModelPreviewPanel = ((Node)nodeOrNull).GetNodeOrNull<CharacterModelPreviewPanel>(NodePath.op_Implicit("STS2CharacterModelPreview"));
			if (characterModelPreviewPanel == null)
			{
				CharacterModelPreviewPanel characterModelPreviewPanel2 = new CharacterModelPreviewPanel();
				((Node)characterModelPreviewPanel2).Name = StringName.op_Implicit("STS2CharacterModelPreview");
				((CanvasItem)characterModelPreviewPanel2).Visible = false;
				characterModelPreviewPanel = characterModelPreviewPanel2;
				characterModelPreviewPanel.Initialize(screen, nodeOrNull);
				((Node)nodeOrNull).AddChild((Node)(object)characterModelPreviewPanel, false, (InternalMode)0);
			}
			characterModelPreviewPanel.ShowCharacter(character);
		}
	}

	public static void Hide(NCharacterSelectScreen screen)
	{
		((Node)screen).GetNodeOrNull<CharacterModelPreviewPanel>(NodePath.op_Implicit("InfoPanel/STS2CharacterModelPreview"))?.Suspend();
	}
}
