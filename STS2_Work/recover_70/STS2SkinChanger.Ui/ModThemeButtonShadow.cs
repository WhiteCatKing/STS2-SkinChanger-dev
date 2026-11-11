using System;
using System.Runtime.CompilerServices;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal sealed class ModThemeButtonShadow
{
	private static readonly ConditionalWeakTable<Button, ModThemeButtonShadow> Instances = new ConditionalWeakTable<Button, ModThemeButtonShadow>();

	private static readonly string[] States = new string[5] { "normal", "hover", "pressed", "hover_pressed", "disabled" };

	private static readonly string[] IconKeys = new string[13]
	{
		"icon", "checked", "unchecked", "radio_checked", "radio_unchecked", "checked_disabled", "unchecked_disabled", "radio_checked_disabled", "radio_unchecked_disabled", "checked_mirrored",
		"unchecked_mirrored", "checked_disabled_mirrored", "unchecked_disabled_mirrored"
	};

	private readonly Button _owner;

	private Button? _shadow;

	private StyleBoxEmpty? _spacing;

	private Texture2D? _arrow;

	internal static void Attach(Button button)
	{
		Instances.GetValue(button, (Button node) => new ModThemeButtonShadow(node)).Refresh();
	}

	private ModThemeButtonShadow(Button owner)
	{
		_owner = owner;
		((CanvasItem)owner).Draw += Refresh;
		((Control)owner).Resized += Refresh;
	}

	private static Button CreateShadow(Button owner)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (!(owner is OptionButton))
		{
			if (!(owner is CheckBox))
			{
				if (owner is CheckButton)
				{
					return (Button)new CheckButton();
				}
				return new Button();
			}
			return (Button)new CheckBox();
		}
		return (Button)new OptionButton
		{
			FitToLongestItem = false
		};
	}

	private void EnsureShadow()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		if (_shadow == null)
		{
			_shadow = CreateShadow(_owner);
			((Node)_shadow).Name = StringName.op_Implicit("SCThemeTextShadow");
			((CanvasItem)_shadow).ShowBehindParent = true;
			((Control)_shadow).MouseFilter = (MouseFilterEnum)2;
			((Control)_shadow).FocusMode = (FocusModeEnum)0;
			_shadow.Flat = true;
			_spacing = new StyleBoxEmpty();
			string[] states = States;
			foreach (string text in states)
			{
				((Control)_shadow).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)(object)_spacing);
				((Control)_shadow).AddThemeStyleboxOverride(StringName.op_Implicit(text + "_mirrored"), (StyleBox)(object)_spacing);
			}
			states = new string[6] { "normal", "focus", "pressed", "hover", "hover_pressed", "disabled" };
			foreach (string text2 in states)
			{
				((Control)_shadow).AddThemeColorOverride(StringName.op_Implicit("icon_" + text2 + "_color"), Colors.Transparent);
			}
			states = new string[4] { "checkbox_checked_color", "checkbox_unchecked_color", "button_checked_color", "button_unchecked_color" };
			foreach (string text3 in states)
			{
				((Control)_shadow).AddThemeColorOverride(StringName.op_Implicit(text3), Colors.Transparent);
			}
			((Node)_owner).AddChild((Node)(object)_shadow, false, (InternalMode)0);
		}
	}

	private void Refresh()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Expected O, but got Unknown
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Expected O, but got Unknown
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_049b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0556: Unknown result type (might be due to invalid IL or missing references)
		//IL_0540: Unknown result type (might be due to invalid IL or missing references)
		//IL_0545: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_owner) || ((GodotObject)_owner).IsQueuedForDeletion())
		{
			return;
		}
		ModThemeSettings current = ModThemeRuntime.Current;
		if (!current.TextShadowEnabled || !(current.TextShadowOpacity > 0f) || string.IsNullOrEmpty(_owner.Text))
		{
			if (_shadow != null)
			{
				((CanvasItem)_shadow).Visible = false;
			}
			return;
		}
		EnsureShadow();
		Button shadow = _shadow;
		((CanvasItem)shadow).Visible = true;
		if (shadow.Text != _owner.Text)
		{
			shadow.Text = _owner.Text;
		}
		if (shadow.Alignment != _owner.Alignment)
		{
			shadow.Alignment = _owner.Alignment;
		}
		if (((Control)shadow).LayoutDirection != ((Control)_owner).LayoutDirection)
		{
			((Control)shadow).LayoutDirection = ((Control)_owner).LayoutDirection;
		}
		if (shadow.TextDirection != _owner.TextDirection)
		{
			shadow.TextDirection = _owner.TextDirection;
		}
		if (shadow.Language != _owner.Language)
		{
			shadow.Language = _owner.Language;
		}
		if (shadow.ClipText != _owner.ClipText)
		{
			shadow.ClipText = _owner.ClipText;
		}
		if (shadow.TextOverrunBehavior != _owner.TextOverrunBehavior)
		{
			shadow.TextOverrunBehavior = _owner.TextOverrunBehavior;
		}
		if (shadow.AutowrapMode != _owner.AutowrapMode)
		{
			shadow.AutowrapMode = _owner.AutowrapMode;
		}
		if (shadow.AutowrapTrimFlags != _owner.AutowrapTrimFlags)
		{
			shadow.AutowrapTrimFlags = _owner.AutowrapTrimFlags;
		}
		if (shadow.Icon != _owner.Icon)
		{
			shadow.Icon = _owner.Icon;
		}
		if (shadow.IconAlignment != _owner.IconAlignment)
		{
			shadow.IconAlignment = _owner.IconAlignment;
		}
		if (shadow.VerticalIconAlignment != _owner.VerticalIconAlignment)
		{
			shadow.VerticalIconAlignment = _owner.VerticalIconAlignment;
		}
		if (shadow.ExpandIcon != _owner.ExpandIcon)
		{
			shadow.ExpandIcon = _owner.ExpandIcon;
		}
		Font themeFont = ((Control)_owner).GetThemeFont(StringName.op_Implicit("font"), (StringName)null);
		if (((Control)shadow).GetThemeFont(StringName.op_Implicit("font"), (StringName)null) != themeFont || !((Control)shadow).HasThemeFontOverride(StringName.op_Implicit("font")))
		{
			((Control)shadow).AddThemeFontOverride(StringName.op_Implicit("font"), themeFont);
		}
		int themeFontSize = ((Control)_owner).GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null);
		if (((Control)shadow).GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null) != themeFontSize || !((Control)shadow).HasThemeFontSizeOverride(StringName.op_Implicit("font_size")))
		{
			((Control)shadow).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), themeFontSize);
		}
		string[] array = new string[5] { "h_separation", "icon_max_width", "line_spacing", "check_v_offset", "arrow_margin" };
		foreach (string text in array)
		{
			Constant(text, ((Control)_owner).GetThemeConstant(StringName.op_Implicit(text), (StringName)null));
		}
		array = IconKeys;
		foreach (string text2 in array)
		{
			if (((Control)_owner).HasThemeIcon(StringName.op_Implicit(text2), (StringName)null) && (!((Control)shadow).HasThemeIconOverride(StringName.op_Implicit(text2)) || ((Control)shadow).GetThemeIcon(StringName.op_Implicit(text2), (StringName)null) != ((Control)_owner).GetThemeIcon(StringName.op_Implicit(text2), (StringName)null)))
			{
				((Control)shadow).AddThemeIconOverride(StringName.op_Implicit(text2), ((Control)_owner).GetThemeIcon(StringName.op_Implicit(text2), (StringName)null));
			}
		}
		if (_owner is OptionButton && ((Control)_owner).HasThemeIcon(StringName.op_Implicit("arrow"), (StringName)null))
		{
			Vector2 size = ((Control)_owner).GetThemeIcon(StringName.op_Implicit("arrow"), (StringName)null).GetSize();
			if (_arrow == null || _arrow.GetSize() != size)
			{
				GradientTexture2D val = new GradientTexture2D();
				val.Width = Math.Max(1, (int)size.X);
				val.Height = Math.Max(1, (int)size.Y);
				Gradient val2 = new Gradient();
				val2.Colors = (Color[])(object)new Color[2]
				{
					Colors.Transparent,
					Colors.Transparent
				};
				val.Gradient = val2;
				_arrow = (Texture2D?)(object)val;
				((Control)shadow).AddThemeIconOverride(StringName.op_Implicit("arrow"), _arrow);
			}
		}
		Color val3 = ModThemeRuntime.Tint(current.TextShadowColor, current.TextShadowOpacity);
		array = new string[7] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_disabled_color", "font_focus_color", "font_outline_color" };
		foreach (string text3 in array)
		{
			if (!((Control)shadow).HasThemeColorOverride(StringName.op_Implicit(text3)) || ((Control)shadow).GetThemeColor(StringName.op_Implicit(text3), (StringName)null) != val3)
			{
				((Control)shadow).AddThemeColorOverride(StringName.op_Implicit(text3), val3);
			}
		}
		Constant("outline_size", current.TextShadowSize);
		SyncMargins();
		Vector2 val4 = default(Vector2);
		((Vector2)(ref val4))._002Ector((float)current.TextShadowOffsetX, (float)current.TextShadowOffsetY);
		if (((Control)shadow).Position != val4)
		{
			((Control)shadow).Position = val4;
		}
		if (((Control)shadow).Size != ((Control)_owner).Size)
		{
			((Control)shadow).Size = ((Control)_owner).Size;
		}
	}

	private void Constant(string key, int value)
	{
		if (!((Control)_shadow).HasThemeConstantOverride(StringName.op_Implicit(key)) || ((Control)_shadow).GetThemeConstant(StringName.op_Implicit(key), (StringName)null) != value)
		{
			((Control)_shadow).AddThemeConstantOverride(StringName.op_Implicit(key), value);
		}
	}

	private void SyncMargins()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I8
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		DrawMode drawMode = ((BaseButton)_owner).GetDrawMode();
		_003F val = drawMode - 1;
		if ((long)val > 3L)
		{
			goto IL_0050;
		}
		switch ((uint)val)
		{
		case 2u:
			break;
		case 3u:
			goto IL_0038;
		case 0u:
			goto IL_0040;
		case 1u:
			goto IL_0048;
		default:
			goto IL_0050;
		}
		string text = "disabled";
		goto IL_0056;
		IL_0050:
		text = "normal";
		goto IL_0056;
		IL_0040:
		text = "pressed";
		goto IL_0056;
		IL_0048:
		text = "hover";
		goto IL_0056;
		IL_0056:
		string text2 = text;
		bool flag = ((Control)_owner).GetThemeConstant(StringName.op_Implicit("align_to_largest_stylebox"), (StringName)null) != 0;
		Side[] array = new Side[4];
		RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		Side[] array2 = (Side[])(object)array;
		foreach (Side val2 in array2)
		{
			float num = 0f;
			string[] array3 = (flag ? States : new string[1] { text2 });
			foreach (string text3 in array3)
			{
				string text4 = ((((Control)_owner).IsLayoutRtl() && ((Control)_owner).HasThemeStylebox(StringName.op_Implicit(text3 + "_mirrored"), (StringName)null)) ? (text3 + "_mirrored") : text3);
				num = Math.Max(num, ((Control)_owner).GetThemeStylebox(StringName.op_Implicit(text4), (StringName)null).GetMargin(val2));
			}
			if (((StyleBox)_spacing).GetContentMargin(val2) != num)
			{
				((StyleBox)_spacing).SetContentMargin(val2, num);
			}
		}
		return;
		IL_0038:
		text = "hover_pressed";
		goto IL_0056;
	}
}
