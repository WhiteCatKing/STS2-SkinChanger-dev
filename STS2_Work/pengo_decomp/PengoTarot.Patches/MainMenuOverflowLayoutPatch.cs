using System;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace PengoTarot.Patches;

[HarmonyPatch]
internal static class MainMenuOverflowLayoutPatch
{
	private sealed class OriginalLayout
	{
		private readonly float _columnOffsetLeft;

		private readonly float _columnOffsetTop;

		private readonly float _columnOffsetRight;

		private readonly float _columnOffsetBottom;

		private readonly Vector2 _columnScale;

		private readonly Vector2 _logoPosition;

		private readonly Vector2 _logoScale;

		internal float LogoButtonGap { get; }

		internal OriginalLayout(Control column, Node2D logo, float logoButtonGap = 20f)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			_columnOffsetLeft = column.OffsetLeft;
			_columnOffsetTop = column.OffsetTop;
			_columnOffsetRight = column.OffsetRight;
			_columnOffsetBottom = column.OffsetBottom;
			_columnScale = column.Scale;
			_logoPosition = logo.Position;
			_logoScale = logo.Scale;
			LogoButtonGap = logoButtonGap;
		}

		internal void Restore(Control column, Node2D logo)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			column.Scale = _columnScale;
			column.OffsetLeft = _columnOffsetLeft;
			column.OffsetTop = _columnOffsetTop;
			column.OffsetRight = _columnOffsetRight;
			column.OffsetBottom = _columnOffsetBottom;
			logo.Scale = _logoScale;
			logo.Position = _logoPosition;
		}
	}

	private const float EdgeMargin = 10f;

	private const float MinimumLogoButtonGap = 20f;

	private const float MinimumLogoScaleFactor = 0.35f;

	private const bool AddLocalTestButtons = true;

	private const int LocalTestButtonCount = 20;

	private static readonly ConditionalWeakTable<NMainMenu, OriginalLayout> OriginalLayouts = new ConditionalWeakTable<NMainMenu, OriginalLayout>();

	[HarmonyPatch(typeof(NMainMenu), "_Ready")]
	[HarmonyPostfix]
	private static void ReadyPostfix(NMainMenu __instance)
	{
		CaptureOriginalLayout(__instance);
		ScheduleAdjustment(__instance);
	}

	private static void CaptureOriginalLayout(NMainMenu mainMenu)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		VBoxContainer nodeOrNull = ((Node)mainMenu).GetNodeOrNull<VBoxContainer>(NodePath.op_Implicit("MainMenuTextButtons"));
		Node2D nodeOrNull2 = ((Node)mainMenu).GetNodeOrNull<Node2D>(NodePath.op_Implicit("MainMenuBg/BgContainer/Control/Logo"));
		if (nodeOrNull != null && nodeOrNull2 != null && !OriginalLayouts.TryGetValue(mainMenu, out OriginalLayout _))
		{
			float logoButtonGap = 20f;
			if (TryGetLogoBounds(nodeOrNull2, out var bounds))
			{
				Rect2 globalRect = ((Control)nodeOrNull).GetGlobalRect();
				logoButtonGap = Mathf.Max(20f, ((Rect2)(ref globalRect)).Position.Y - ((Rect2)(ref bounds)).End.Y);
			}
			OriginalLayouts.Add(mainMenu, new OriginalLayout((Control)(object)nodeOrNull, nodeOrNull2, logoButtonGap));
		}
	}

	[HarmonyPatch(typeof(NMainMenu), "RefreshButtons")]
	[HarmonyPostfix]
	private static void RefreshButtonsPostfix(NMainMenu __instance)
	{
		ScheduleAdjustment(__instance);
	}

	private static void ScheduleAdjustment(NMainMenu mainMenu)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)mainMenu))
		{
			return;
		}
		Callable val2 = Callable.From((Action)delegate
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)mainMenu))
			{
				Callable val = Callable.From((Action)delegate
				{
					TryAdjustLayout(mainMenu);
				});
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
		});
		((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
	}

	private static void AddTestButtons(NMainMenu mainMenu)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		VBoxContainer nodeOrNull = ((Node)mainMenu).GetNodeOrNull<VBoxContainer>(NodePath.op_Implicit("MainMenuTextButtons"));
		NMainMenuTextButton val = ((nodeOrNull != null) ? ((Node)nodeOrNull).GetNodeOrNull<NMainMenuTextButton>(NodePath.op_Implicit("QuitButton")) : null);
		if (nodeOrNull == null || val == null || ((Node)val).GetChildCount(false) == 0)
		{
			Log.Warn("[PengoTarot] 首页自适应布局：未匹配到测试按钮模板，已跳过测试按钮。", 2);
			return;
		}
		for (int i = 1; i <= 20; i++)
		{
			NMainMenuTextButton val2 = new NMainMenuTextButton
			{
				Name = StringName.op_Implicit($"PengoTarotTestButton{i}"),
				CustomMinimumSize = ((Control)val).CustomMinimumSize,
				SizeFlagsHorizontal = ((Control)val).SizeFlagsHorizontal,
				FocusMode = ((Control)val).FocusMode
			};
			Node val3 = ((Node)val).GetChild(0, false).Duplicate(15);
			((Node)val2).AddChild(val3, false, (InternalMode)0);
			((Node)nodeOrNull).AddChild((Node)(object)val2, false, (InternalMode)0);
			Label val4 = (Label)(object)((val3 is Label) ? val3 : null);
			if (val4 != null)
			{
				val4.Text = "测试";
			}
		}
	}

	private static void TryAdjustLayout(NMainMenu mainMenu)
	{
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0303: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)mainMenu) || !((Node)mainMenu).IsInsideTree())
		{
			return;
		}
		VBoxContainer buttonColumn = ((Node)mainMenu).GetNodeOrNull<VBoxContainer>(NodePath.op_Implicit("MainMenuTextButtons"));
		Node2D logo = ((Node)mainMenu).GetNodeOrNull<Node2D>(NodePath.op_Implicit("MainMenuBg/BgContainer/Control/Logo"));
		Node2D obj = logo;
		Node val = ((obj != null) ? ((Node)obj).GetNodeOrNull(NodePath.op_Implicit("NSpineAutoPlayer")) : null);
		Node2D obj2 = logo;
		Node val2 = ((obj2 != null) ? ((Node)obj2).GetNodeOrNull(NodePath.op_Implicit("light_large")) : null);
		if (buttonColumn == null || logo == null || val == null || val2 == null)
		{
			Log.Warn("[PengoTarot] 首页自适应布局：未匹配到按钮列、标题或标题鬼火，已跳过布局调整。", 2);
			return;
		}
		foreach (Node child in ((Node)buttonColumn).GetChildren(false))
		{
			if (!(child is Control))
			{
				Log.Warn($"[PengoTarot] 首页自适应布局：按钮列中存在未匹配控件 {child.Name}，已跳过布局调整。", 2);
				return;
			}
		}
		OriginalLayout value = OriginalLayouts.GetValue(mainMenu, (NMainMenu _) => new OriginalLayout((Control)(object)buttonColumn, logo));
		value.Restore((Control)(object)buttonColumn, logo);
		if (!TryGetVisibleButtonBounds(buttonColumn, out var bounds))
		{
			Log.Warn("[PengoTarot] 首页自适应布局：无法取得可见按钮边界，已跳过布局调整。", 2);
			return;
		}
		if (!TryGetLogoBounds(logo, out var bounds2))
		{
			Log.Warn("[PengoTarot] 首页自适应布局：无法取得标题边界，已跳过布局调整。", 2);
			return;
		}
		Rect2 val3 = ((Rect2)(ref bounds)).Merge(bounds2);
		Rect2 viewportRect = ((CanvasItem)mainMenu).GetViewportRect();
		float num = ((Rect2)(ref viewportRect)).Position.Y + 10f;
		float num2 = ((Rect2)(ref viewportRect)).End.Y - 10f;
		if (((Rect2)(ref val3)).Position.Y >= num && ((Rect2)(ref val3)).End.Y <= num2)
		{
			return;
		}
		float num3 = num2 - num;
		if (num3 <= 0f || ((Rect2)(ref val3)).Size.Y <= 0f)
		{
			Log.Warn("[PengoTarot] 首页自适应布局：可用区域尺寸无效，已跳过布局调整。", 2);
			return;
		}
		float logoButtonGap = value.LogoButtonGap;
		float num4 = Mathf.Clamp((num3 - ((Rect2)(ref bounds)).Size.Y - logoButtonGap) / ((Rect2)(ref bounds2)).Size.Y, 0.35f, 1f);
		float num5 = 1f;
		float num6 = ((Rect2)(ref bounds2)).Size.Y * num4 + logoButtonGap + ((Rect2)(ref bounds)).Size.Y;
		float num8;
		float targetTop;
		if (num6 > num3)
		{
			num4 = 0.35f;
			num5 = Mathf.Min(1f, num3 / ((Rect2)(ref bounds)).Size.Y);
			float num7 = ((Rect2)(ref bounds)).Size.Y * num5;
			num8 = num;
			targetTop = num + (num3 - num7) * 0.5f;
		}
		else
		{
			num8 = num + (num3 - num6) * 0.5f;
			targetTop = num8 + ((Rect2)(ref bounds2)).Size.Y * num4 + logoButtonGap;
		}
		ApplySharedVerticalTransform(logo, ((Rect2)(ref bounds2)).Position.Y, num8, num4);
		ApplySharedVerticalTransform((Control)(object)buttonColumn, ((Rect2)(ref bounds)).Position.Y, targetTop, num5);
	}

	private static bool TryGetVisibleButtonBounds(VBoxContainer buttonColumn, out Rect2 bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		bounds = default(Rect2);
		bool flag = false;
		foreach (Node child in ((Node)buttonColumn).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((CanvasItem)val).Visible && !(val.Size.Y <= 0f))
			{
				Rect2 globalRect = val.GetGlobalRect();
				bounds = (flag ? ((Rect2)(ref bounds)).Merge(globalRect) : globalRect);
				flag = true;
			}
		}
		return flag;
	}

	private static bool TryGetLogoBounds(Node2D logo, out Rect2 bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			MegaSkeleton skeleton = new MegaSprite(Variant.op_Implicit((GodotObject)(object)logo)).GetSkeleton();
			if (skeleton == null)
			{
				bounds = default(Rect2);
				return false;
			}
			Rect2 bounds2 = skeleton.GetBounds();
			Transform2D globalTransform = ((CanvasItem)logo).GetGlobalTransform();
			Vector2 val = globalTransform * ((Rect2)(ref bounds2)).Position;
			Vector2 val2 = globalTransform * new Vector2(((Rect2)(ref bounds2)).End.X, ((Rect2)(ref bounds2)).Position.Y);
			Vector2 val3 = globalTransform * ((Rect2)(ref bounds2)).End;
			Vector2 val4 = globalTransform * new Vector2(((Rect2)(ref bounds2)).Position.X, ((Rect2)(ref bounds2)).End.Y);
			Vector2 val5 = ((Vector2)(ref val)).Min(val2);
			val5 = ((Vector2)(ref val5)).Min(val3);
			Vector2 val6 = ((Vector2)(ref val5)).Min(val4);
			val5 = ((Vector2)(ref val)).Max(val2);
			val5 = ((Vector2)(ref val5)).Max(val3);
			Vector2 val7 = ((Vector2)(ref val5)).Max(val4);
			bounds = new Rect2(val6, val7 - val6);
			return ((Rect2)(ref bounds)).Size.X > 0f && ((Rect2)(ref bounds)).Size.Y > 0f;
		}
		catch (Exception)
		{
			bounds = default(Rect2);
			return false;
		}
	}

	private static void ApplySharedVerticalTransform(Control item, float sourceTop, float targetTop, float scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 globalPosition = item.GlobalPosition;
		item.Scale *= scale;
		globalPosition.Y = targetTop + (globalPosition.Y - sourceTop) * scale;
		item.GlobalPosition = globalPosition;
	}

	private static void ApplySharedVerticalTransform(Node2D item, float sourceTop, float targetTop, float scale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 globalPosition = item.GlobalPosition;
		item.Scale *= scale;
		globalPosition.Y = targetTop + (globalPosition.Y - sourceTop) * scale;
		item.GlobalPosition = globalPosition;
	}
}
