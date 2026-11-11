using System;
using System.Collections;
using System.Linq;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class ModThemeListHover
{
	private const float HoverScale = 1.04f;

	internal static void AddScrollList(ScrollContainer scroll, VBoxContainer list)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		MarginContainer padding = new MarginContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Node)scroll).AddChild((Node)(object)padding, false, (InternalMode)0);
		((Node)padding).AddChild((Node)(object)list, false, (InternalMode)0);
		((Control)scroll).Resized += Refresh;
		((Control)list).MinimumSizeChanged += Refresh;
		ModThemeRuntime.Bind((Node)(object)padding, "hover_padding", delegate
		{
			Refresh();
		});
		void Refresh()
		{
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			ModThemeSettings current = ModThemeRuntime.Current;
			float num = (from b in ((IEnumerable)((Node)list).GetChildren(false)).OfType<Button>()
				select ((Control)b).GetCombinedMinimumSize().Y).DefaultIfEmpty(58f).Max();
			int num2 = current.TextOutline + (current.TextShadowEnabled ? (Math.Max(Math.Abs(current.TextShadowOffsetX), Math.Abs(current.TextShadowOffsetY)) + current.TextShadowSize) : 0) + 2;
			Vector2 val = ScrollPadding(new Vector2(((Control)scroll).Size.X, num), (float)num2 * 1.04f);
			(string, float)[] array = new(string, float)[4]
			{
				("left", val.X),
				("right", val.X),
				("top", val.Y),
				("bottom", val.Y)
			};
			for (int i = 0; i < array.Length; i++)
			{
				var (text, num3) = array[i];
				if (((Control)padding).GetThemeConstant(StringName.op_Implicit("margin_" + text), (StringName)null) != (int)num3)
				{
					((Control)padding).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), (int)num3);
				}
			}
		}
	}

	internal static Vector2 ScrollPadding(Vector2 size, float decoration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(MathF.Ceiling((size.X * 0.03999996f + decoration * 2f) / 2.08f), MathF.Ceiling(size.Y * 0.03999996f / 2f + decoration));
	}

	internal static void Attach(Button button)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (((GodotObject)button).HasMeta(StringName.op_Implicit("sc_theme_list_hover")))
		{
			return;
		}
		((GodotObject)button).SetMeta(StringName.op_Implicit("sc_theme_list_hover"), Variant.op_Implicit(true));
		Tween tween = null;
		((Control)button).MouseEntered += delegate
		{
			Change(hovered: true);
		};
		((Control)button).MouseExited += delegate
		{
			Change(hovered: false);
		};
		((CanvasItem)button).VisibilityChanged += delegate
		{
			if (!((CanvasItem)button).IsVisibleInTree())
			{
				Reset();
			}
		};
		((Node)button).TreeExiting += Reset;
		((Control)button).Resized += Resize;
		Resize();
		void Change(bool hovered)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			Tween obj = tween;
			if (obj != null)
			{
				obj.Kill();
			}
			Resize();
			if (((Node)button).IsInsideTree())
			{
				tween = ((Node)button).CreateTween().SetEase((EaseType)1).SetTrans((TransitionType)7);
				tween.TweenProperty((GodotObject)(object)button, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * (hovered ? 1.04f : 1f)), 0.14);
			}
		}
		void Reset()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			Tween obj2 = tween;
			if (obj2 != null)
			{
				obj2.Kill();
			}
			tween = null;
			((Control)button).Scale = Vector2.One;
		}
		void Resize()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			((Control)button).PivotOffset = ((Control)button).Size * 0.5f;
		}
	}
}
