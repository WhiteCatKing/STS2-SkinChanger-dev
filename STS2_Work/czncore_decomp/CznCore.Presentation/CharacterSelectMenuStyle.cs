using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace CznCore.Presentation;

internal static class CharacterSelectMenuStyle
{
	public static readonly Color Accent = new Color("ff9e00");

	public static readonly Color Text = new Color("2c2c2c");

	public static readonly Color Muted = new Color("666666");

	public static readonly Color Surface = new Color("eaeaea");

	public static readonly Color SurfaceRaised = new Color("f5f5f5");

	public static readonly Color Border = new Color(new Color("b5b5b5"), 0.42f);

	public static readonly StyleBoxFlat Panel = Flat(Surface, Border, 1);

	public static readonly StyleBoxFlat Raised = Flat(SurfaceRaised, Border, 1);

	public static readonly StyleBoxFlat Clear = Flat(Colors.Transparent, Colors.Transparent, 0, 0);

	public static readonly StyleBoxFlat ToggleOn = Flat(Accent, Border, 1, 3);

	public static readonly StyleBoxFlat ToggleOff = Flat(SurfaceRaised, Border, 1, 3);

	private static readonly Color Pressed = new Color(1f, 0.94f, 0.84f, 1f);

	private static readonly string[] EntryStates = new string[5] { "normal", "hover", "pressed", "hover_pressed", "focus" };

	private static readonly StyleBoxFlat[] ActionStyles = (StyleBoxFlat[])(object)new StyleBoxFlat[5]
	{
		Raised,
		Flat(Colors.White, new Color(Accent, 0.82f), 1),
		Flat(Pressed, Accent, 2),
		Flat(Colors.White, new Color(Accent, 0.82f), 1),
		Outline(Flat(Colors.Transparent, Accent, 2))
	};

	private static readonly Dictionary<(Color, bool), StyleBoxFlat[]> EntryStyles = new Dictionary<(Color, bool), StyleBoxFlat[]>();

	private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

	public static bool NativeModalOpen
	{
		get
		{
			NModalContainer instance = NModalContainer.Instance;
			return ((instance != null) ? instance.OpenModal : null) != null;
		}
	}

	public static Action BindModalLayer(Control surface, int layer)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		NModalContainer modal = NModalContainer.Instance;
		((Node)modal).ChildEnteredTree += new ChildEnteredTreeEventHandler(Opened);
		((Node)modal).ChildExitingTree += new ChildExitingTreeEventHandler(Closing);
		Refresh();
		return delegate
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			((Node)modal).ChildEnteredTree -= new ChildEnteredTreeEventHandler(Opened);
			((Node)modal).ChildExitingTree -= new ChildExitingTreeEventHandler(Closing);
		};
		void Closing(Node _)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Callable val = Callable.From((Action)Refresh);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		void Opened(Node _)
		{
			Refresh();
		}
		void Refresh()
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)surface))
			{
				((CanvasItem)surface).ZIndex = ((!NativeModalOpen) ? layer : 0);
			}
		}
	}

	public static void ApplyEntry(BaseButton button, Color accent, bool toggle)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!EntryStyles.TryGetValue((accent, toggle), out StyleBoxFlat[] value))
		{
			Dictionary<(Color, bool), StyleBoxFlat[]> entryStyles = EntryStyles;
			(Color, bool) key = (accent, toggle);
			StyleBoxFlat[] obj = new StyleBoxFlat[5]
			{
				Bar(SurfaceRaised, 1, accent),
				Bar(Colors.White, 1, accent),
				toggle ? Bar(SurfaceRaised, 1, accent) : Bar(Pressed, 2, accent),
				Bar(Colors.White, 1, accent),
				Bar(Colors.Transparent, 2, accent, drawCenter: false)
			};
			value = (StyleBoxFlat[])(object)obj;
			entryStyles[key] = (StyleBoxFlat[])(object)obj;
		}
		ApplyStates(button, value);
	}

	public static void ApplyAction(BaseButton button)
	{
		ApplyStates(button, ActionStyles);
	}

	public static Button TextButton(string name, string text, Vector2 minimumSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Button val = new Button
		{
			Name = StringName.op_Implicit(name),
			Text = text,
			CustomMinimumSize = minimumSize,
			Shortcut = CancelShortcut()
		};
		ApplyStates((BaseButton)(object)val, ActionStyles);
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)Clear);
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
		string[] array = new string[4] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" };
		foreach (string text2 in array)
		{
			((Control)val).AddThemeColorOverride(StringName.op_Implicit(text2), Text);
		}
		return val;
	}

	public static Label Label(string text, int fontSize, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		Label val = new Label
		{
			Text = text,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), fontSize);
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), color);
		return val;
	}

	public static TextureRect Icon(string path, Color modulate)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		return new TextureRect
		{
			Name = StringName.op_Implicit("Icon"),
			Texture = Texture(path),
			Modulate = modulate,
			CustomMinimumSize = new Vector2(30f, 30f),
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)5,
			MouseFilter = (MouseFilterEnum)2,
			SizeFlagsVertical = (SizeFlags)4
		};
	}

	public static Shortcut CancelShortcut()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002d: Expected O, but got Unknown
		Shortcut val = new Shortcut();
		Array val2 = new Array();
		val2.Add(Variant.op_Implicit((GodotObject)new InputEventKey
		{
			Keycode = (Key)4194305
		}));
		val.Events = val2;
		return val;
	}

	public static void CloseOnCancel(Node root, Func<bool> close)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		Control control = (Control)(object)((root is Control) ? root : null);
		if (control != null && (int)control.FocusMode != 0)
		{
			control.GuiInput += (GuiInputEventHandler)delegate(InputEvent inputEvent)
			{
				if (inputEvent.IsActionPressed(MegaInput.cancel, false, false) && close())
				{
					control.AcceptEvent();
				}
			};
		}
		foreach (Node child in root.GetChildren(false))
		{
			CloseOnCancel(child, close);
		}
	}

	private static Texture2D Texture(string path)
	{
		if (!Textures.TryGetValue(path, out Texture2D value))
		{
			value = (Textures[path] = (Texture2D)(FileAccess.FileExists(path) ? ((object)ImageTexture.CreateFromImage(Image.LoadFromFile(path))) : ((object)ResourceLoader.Load<Texture2D>(path, (string)null, (CacheMode)1))));
		}
		return value;
	}

	private static void ApplyStates(BaseButton button, StyleBoxFlat[] styles)
	{
		for (int i = 0; i < EntryStates.Length; i++)
		{
			((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit(EntryStates[i]), (StyleBox)(object)styles[i]);
		}
	}

	private static StyleBoxFlat Bar(Color background, int width, Color accent, bool drawCenter = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		StyleBoxFlat obj = Flat(background, new Color(accent, 0.7f), width);
		obj.BorderWidthLeft = 4;
		obj.DrawCenter = drawCenter;
		return obj;
	}

	private static StyleBoxFlat Outline(StyleBoxFlat style)
	{
		style.DrawCenter = false;
		return style;
	}

	private static StyleBoxFlat Flat(Color background, Color border, int width, int radius = 8)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		StyleBoxFlat val = new StyleBoxFlat
		{
			BgColor = background,
			BorderColor = border,
			AntiAliasing = true,
			AntiAliasingSize = 0.75f
		};
		val.SetBorderWidthAll(width);
		val.SetCornerRadiusAll(radius);
		return val;
	}
}
