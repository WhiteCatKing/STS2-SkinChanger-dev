using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

namespace ModLaunchManager;

internal static class LiquidGlass
{
	internal enum GlassText
	{
		Body,
		All,
		OnAccent
	}

	private sealed class Capture
	{
		private static readonly List<Capture> Placed = new List<Capture>();

		internal readonly BackBufferCopy Node = new BackBufferCopy
		{
			Name = StringName.op_Implicit("BackdropCapture"),
			CopyMode = (CopyModeEnum)0
		};

		private readonly Control _area;

		private readonly float _bleed;

		private readonly int _deepestLevel;

		private Rect2 _rect;

		private bool _enabled;

		internal Capture(Control area, float bleed, int deepestLevel)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			_area = area;
			_bleed = bleed;
			_deepestLevel = deepestLevel;
		}

		internal void Enable(bool enabled)
		{
			if (enabled == _enabled)
			{
				return;
			}
			_enabled = enabled;
			if (!BoundedCopies)
			{
				Node.CopyMode = (CopyModeEnum)(enabled ? 2 : 0);
			}
			else if (enabled)
			{
				if (Placed.Count == 0)
				{
					RenderingServer.FramePreDraw += PlaceAll;
				}
				Placed.Add(this);
				Place();
				Node.CopyMode = (CopyModeEnum)1;
			}
			else
			{
				Node.CopyMode = (CopyModeEnum)0;
				Placed.Remove(this);
				if (Placed.Count == 0)
				{
					RenderingServer.FramePreDraw -= PlaceAll;
				}
			}
		}

		private static void PlaceAll()
		{
			for (int num = Placed.Count - 1; num >= 0; num--)
			{
				Placed[num].Place();
			}
		}

		private void Place()
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			if (!GodotObject.IsInstanceValid((GodotObject)(object)_area) || !((Node)_area).IsInsideTree())
			{
				return;
			}
			Viewport viewport = ((Node)_area).GetViewport();
			Rect2 val = viewport.GetStretchTransform() * viewport.GlobalCanvasTransform * ((CanvasItem)_area).GetGlobalTransformWithCanvas() * new Rect2(-Vector2.One * _bleed, _area.Size + Vector2.One * 2f * _bleed);
			Rect2 val2 = ((Rect2)(ref val)).Grow(128f);
			Vector2 val3 = ((Rect2)(ref val2)).Position / 32f;
			Vector2 val4 = ((Vector2)(ref val3)).Floor() * 32f;
			val3 = ((Rect2)(ref val2)).End / 32f;
			Rect2 val5 = default(Rect2);
			((Rect2)(ref val5))._002Ector(val4, ((Vector2)(ref val3)).Ceil() * 32f - val4);
			if (LevelsFollowBounds)
			{
				val = viewport.GetStretchTransform() * viewport.GetVisibleRect();
				val3 = ((Rect2)(ref val)).Size;
				Vector2I val6 = (Vector2I)((Vector2)(ref val3)).Round();
				int num = 1 << _deepestLevel;
				if (val6.X % num != 0)
				{
					((Rect2)(ref val5))._002Ector(0f, ((Rect2)(ref val5)).Position.Y, (float)val6.X, ((Rect2)(ref val5)).Size.Y);
				}
				if (val6.Y % num != 0)
				{
					((Rect2)(ref val5))._002Ector(((Rect2)(ref val5)).Position.X, 0f, ((Rect2)(ref val5)).Size.X, (float)val6.Y);
				}
			}
			if (!(val5 == _rect))
			{
				_rect = val5;
				Node.Rect = val5;
			}
		}
	}

	private sealed class AddedInk
	{
		private static readonly CanvasItemMaterial Plus = new CanvasItemMaterial
		{
			BlendMode = (BlendModeEnum)1
		};

		private readonly Control _host;

		private SceneTree? _tree;

		private bool _active;

		internal AddedInk(Control host)
		{
			_host = host;
			((Node)host).TreeEntered += Hook;
			((Node)host).TreeExiting += delegate
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Expected O, but got Unknown
				if (_tree != null)
				{
					_tree.NodeAdded -= new NodeAddedEventHandler(Added);
				}
				_tree = null;
			};
			if (((Node)host).IsInsideTree())
			{
				Hook();
			}
		}

		private void Hook()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			if (_tree == null)
			{
				_tree = ((Node)_host).GetTree();
				_tree.NodeAdded += new NodeAddedEventHandler(Added);
			}
		}

		internal void Apply(bool active)
		{
			_active = active;
			Walk((Node)(object)_host, onGlass: true);
		}

		private void Walk(Node node, bool onGlass)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Expected O, but got Unknown
			foreach (Node child in node.GetChildren(false))
			{
				int num;
				if (onGlass)
				{
					Control val = (Control)(object)((child is Control) ? child : null);
					num = ((val == null || !HasOwnFill(val)) ? 1 : 0);
				}
				else
				{
					num = 0;
				}
				bool onGlass2 = (byte)num != 0;
				if ((child is Label || child is RichTextLabel) ? true : false)
				{
					Set((CanvasItem)child, onGlass2);
				}
				Walk(child, onGlass2);
			}
		}

		private void Added(Node node)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			bool flag = ((node is Label || node is RichTextLabel) ? true : false);
			if (!flag || !((Node)_host).IsAncestorOf(node))
			{
				return;
			}
			bool onGlass = true;
			Node parent = node.GetParent();
			while (parent != null && parent != _host)
			{
				Control val = (Control)(object)((parent is Control) ? parent : null);
				if (val != null && HasOwnFill(val))
				{
					onGlass = false;
					break;
				}
				parent = parent.GetParent();
			}
			Set((CanvasItem)node, onGlass);
		}

		private void Set(CanvasItem item, bool onGlass)
		{
			if (_active && onGlass)
			{
				if (item.Material == null)
				{
					Material val = (item.Material = (Material)(object)Plus);
				}
			}
			else if (item.Material == Plus)
			{
				item.Material = null;
			}
		}

		private static bool HasOwnFill(Control control)
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (!(control is BaseButton) && !(control is LineEdit) && !(control is TextEdit))
			{
				if (control is PanelContainer || control is Panel)
				{
					StyleBox themeStylebox = control.GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null);
					StyleBoxFlat val = (StyleBoxFlat)(object)((themeStylebox is StyleBoxFlat) ? themeStylebox : null);
					return val != null && val.BgColor.A >= 0.9f;
				}
				return false;
			}
			return true;
		}
	}

	private sealed class SoftFills
	{
		internal const float Opacity = 0.75f;

		private static readonly string[] Boxes = new string[6] { "normal", "hover", "pressed", "hover_pressed", "disabled", "read_only" };

		private readonly Control _host;

		private HashSet<StyleBox> _ours = new HashSet<StyleBox>();

		private HashSet<StyleBox> _previous = new HashSet<StyleBox>();

		private SceneTree? _tree;

		private bool _active;

		internal SoftFills(Control host)
		{
			_host = host;
			((Node)host).TreeEntered += Hook;
			((Node)host).TreeExiting += delegate
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Expected O, but got Unknown
				if (_tree != null)
				{
					_tree.NodeAdded -= new NodeAddedEventHandler(Added);
				}
				_tree = null;
			};
			if (((Node)host).IsInsideTree())
			{
				Hook();
			}
		}

		private void Hook()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Expected O, but got Unknown
			if (_tree == null)
			{
				_tree = ((Node)_host).GetTree();
				_tree.NodeAdded += new NodeAddedEventHandler(Added);
			}
		}

		internal void Apply(bool active)
		{
			_active = active;
			HashSet<StyleBox> ours = _ours;
			HashSet<StyleBox> previous = _previous;
			_previous = ours;
			_ours = previous;
			_ours.Clear();
			Walk((Node)(object)_host);
			_previous.Clear();
		}

		private void Walk(Node node)
		{
			foreach (Node child in node.GetChildren(false))
			{
				Control val = (Control)(object)((child is Control) ? child : null);
				if (val != null && Filled(val))
				{
					Soften(val);
				}
				Walk(child);
			}
		}

		private void Added(Node node)
		{
			if (_active)
			{
				Control val = (Control)(object)((node is Control) ? node : null);
				if (val != null && Filled(val) && ((Node)_host).IsAncestorOf(node))
				{
					Soften(val);
				}
			}
		}

		private StyleBox? ThemeStyle(Control control, string box)
		{
			Theme val = null;
			Theme val4;
			for (Node val2 = (Node)(object)_host; val2 != null && val == null; val = val4, val2 = val2.GetParent())
			{
				Control val3 = (Control)(object)((val2 is Control) ? val2 : null);
				if (val3 != null)
				{
					Theme theme = val3.Theme;
					if (theme != null)
					{
						val4 = theme;
						continue;
					}
				}
				else
				{
					Window val5 = (Window)(object)((val2 is Window) ? val2 : null);
					if (val5 != null)
					{
						Theme theme2 = val5.Theme;
						if (theme2 != null)
						{
							val4 = theme2;
							continue;
						}
					}
				}
				val4 = null;
			}
			if (val == null)
			{
				return null;
			}
			StringName val6 = (control.ThemeTypeVariation.IsEmpty ? StringName.op_Implicit(((GodotObject)control).GetClass()) : control.ThemeTypeVariation);
			while (!val6.IsEmpty)
			{
				if (val.HasStylebox(StringName.op_Implicit(box), val6))
				{
					return val.GetStylebox(StringName.op_Implicit(box), val6);
				}
				val6 = val.GetTypeVariationBase(val6);
			}
			return null;
		}

		private static bool Filled(Control control)
		{
			if ((!(control is LineEdit) && !(control is TextEdit)) || 1 == 0)
			{
				if (control is BaseButton)
				{
					return ((Node)control).GetNodeOrNull(NodePath.op_Implicit("GlassMaterial")) == null;
				}
				return false;
			}
			return true;
		}

		private void Soften(Control control)
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Expected O, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			string[] boxes = Boxes;
			foreach (string text in boxes)
			{
				if (control.HasThemeStyleboxOverride(StringName.op_Implicit(text)))
				{
					StyleBox themeStylebox = control.GetThemeStylebox(StringName.op_Implicit(text), (StringName)null);
					if (themeStylebox != null && (_previous.Contains(themeStylebox) || _ours.Contains(themeStylebox)))
					{
						control.RemoveThemeStyleboxOverride(StringName.op_Implicit(text));
					}
				}
				if (_active && !control.HasThemeStyleboxOverride(StringName.op_Implicit(text)))
				{
					StyleBox? obj = ThemeStyle(control, text);
					StyleBoxFlat val = (StyleBoxFlat)(object)((obj is StyleBoxFlat) ? obj : null);
					if (val != null && val.BgColor.A >= 0.9f)
					{
						StyleBoxFlat val2 = (StyleBoxFlat)((Resource)val).Duplicate(false);
						val2.BgColor = new Color(val.BgColor, val.BgColor.A * 0.75f);
						val2.ShadowSize = 0;
						_ours.Add((StyleBox)(object)val2);
						control.AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)(object)val2);
					}
				}
			}
		}
	}

	internal sealed class Lens
	{
		private readonly Control _host;

		private readonly bool _primary;

		private readonly bool _surface;

		private readonly GlassText _text;

		private readonly ColorRect _plate;

		private readonly Capture _capture;

		private readonly Timer _clock;

		private ShaderMaterial? _material;

		private GlassSpring _hover = new GlassSpring(0f);

		private GlassSpring _press = new GlassSpring(0f);

		private GlassSpring _x = new GlassSpring(0f);

		private GlassSpring _y = new GlassSpring(0f);

		private ulong _lastTick;

		private bool _hovered;

		private bool _pressed;

		private BaseButton? _pressedSource;

		private Vector2 _pointer;

		private readonly AddedInk? _ink;

		private readonly SoftFills? _fills;

		internal Lens(Control host, bool primary, bool surface, GlassText text)
		{
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Expected O, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Expected O, but got Unknown
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Expected O, but got Unknown
			_host = host;
			_primary = primary;
			_surface = surface;
			_text = text;
			if (surface)
			{
				_ink = new AddedInk(host);
				_fills = new SoftFills(host);
			}
			Node2D val = new Node2D
			{
				Name = StringName.op_Implicit("GlassMaterial"),
				ShowBehindParent = true
			};
			_capture = new Capture(host, 16f, surface ? 4 : 2);
			_plate = new ColorRect
			{
				Name = StringName.op_Implicit("Lens"),
				MouseFilter = (MouseFilterEnum)2,
				Visible = false
			};
			_clock = new Timer
			{
				Name = StringName.op_Implicit("InteractionClock"),
				WaitTime = 1.0 / 120.0,
				ProcessMode = (ProcessModeEnum)3
			};
			((Node)val).AddChild((Node)(object)_capture.Node, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)_plate, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)_clock, false, (InternalMode)0);
			((Node)host).AddChild((Node)(object)val, false, (InternalMode)0);
			_clock.Timeout += Tick;
			host.Resized += Geometry;
			host.ThemeChanged += Refresh;
			((Node)host).TreeEntered += Refresh;
			((CanvasItem)host).VisibilityChanged += Refresh;
			((Node)host).TreeExiting += delegate
			{
				_capture.Enable(enabled: false);
				Stop();
			};
			BaseButton val2 = (BaseButton)(object)((host is BaseButton) ? host : null);
			if (val2 != null)
			{
				Bind(val2);
				((CanvasItem)val2).Draw += SyncVisibility;
			}
		}

		private void Geometry()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			((Control)_plate).Position = new Vector2(-16f, -16f);
			((Control)_plate).Size = _host.Size + Vector2.One * 16f * 2f;
			ShaderMaterial? material = _material;
			if (material != null)
			{
				material.SetShaderParameter(StringName.op_Implicit("plate_size"), Variant.op_Implicit(_host.Size));
			}
			if (!_hovered && !_pressed)
			{
				_pointer = _host.Size * 0.5f;
			}
			Write();
		}

		private void SyncVisibility()
		{
			int num;
			if (UiKit.Palette.IsLiquidGlass && ((CanvasItem)_host).IsVisibleInTree())
			{
				Control host = _host;
				BaseButton val = (BaseButton)(object)((host is BaseButton) ? host : null);
				if (val == null || !val.Disabled)
				{
					num = ((!_primary || ((object)_host.ThemeTypeVariation).ToString() == "PrimaryButton") ? 1 : 0);
					goto IL_0058;
				}
			}
			num = 0;
			goto IL_0058;
			IL_0058:
			bool flag = (byte)num != 0;
			((CanvasItem)_plate).Visible = flag;
			_capture.Enable(flag);
			_ink?.Apply(flag && UiKit.Palette.IsDark);
			_fills?.Apply(flag && UiKit.Palette.IsDark);
			if (flag && _material == null)
			{
				Configure();
			}
			if (!flag)
			{
				Stop();
			}
		}

		private void Refresh()
		{
			if (UiKit.Palette.IsLiquidGlass)
			{
				Configure();
			}
			SyncVisibility();
			Geometry();
		}

		private void Configure()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Expected O, but got Unknown
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			if (_material == null)
			{
				ShaderMaterial val = new ShaderMaterial();
				object obj = _shader;
				if (obj == null)
				{
					Shader val2 = new Shader
					{
						Code = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap, repeat_disable;\r\nuniform vec2 plate_size = vec2(300.0, 40.0);\r\nuniform float radius = 20.0;\r\nuniform float dark_mode = 0.0;\r\nuniform float primary = 0.0;\r\nuniform float surface = 0.0;\r\nuniform float bleed = 16.0;\r\nuniform float reveal = 1.0;\r\nuniform vec2 reveal_origin = vec2(0.0);\r\nuniform vec2 reveal_size = vec2(80.0, 30.0);\r\nuniform vec4 tint : source_color = vec4(0.98, 0.98, 1.0, 1.0);\r\nuniform float hover_amount = 0.0;\r\nuniform float press_amount = 0.0;\r\nuniform vec2 lens_offset = vec2(0.0);\r\nuniform vec2 pointer = vec2(100.0, 20.0);\r\n// Floor for light glass, ceiling for dark and colored glass; see ReadingLimit.\r\nuniform float reading_limit = 0.65;\r\n// The outer band, in plate points, that never carries text and keeps its full optics.\r\nuniform float clear_rim = 3.5;\r\n// 1 where the text on this glass is added to it rather than painted over it; see AddedInk.\r\nuniform float added_ink = 0.0;\r\n// Further blurred levels across the face, for a surface that carries text throughout.\r\nuniform float face_blur = 0.0;\r\n\r\n// Rounded corners turn continuous (a superellipse, as in Liquid Glass Studio) wherever the\r\n// radius is shorter than the half extent; capsule ends stay circular.\r\nfloat shape(vec2 p, vec2 half_size, float r) {\r\n    vec2 q = abs(p) - half_size + r;\r\n    vec2 c = max(q, vec2(0.0));\r\n    float n = r < min(half_size.x, half_size.y) - 0.5 ? 2.6 : 2.0;\r\n    float corner = pow(pow(c.x, n) + pow(c.y, n), 1.0 / n);\r\n    return corner + min(max(q.x, q.y), 0.0) - r;\r\n}\r\n\r\n// Canvas colors are sRGB with the launcher's non-HDR 2D viewport. Apply the text\r\n// budget in linear light, after refraction, press highlights and rim illumination.\r\n// A deeply blurred level, magnified by the hardware, shows its texel grid as soft facets.\r\n// Where the face is frosted further, four taps half a texel apart on a rotated grid fold\r\n// those facets into one smooth wash.\r\nvec3 frosted(vec2 uv, float lod, vec2 point_uv) {\r\n    if (face_blur <= 0.0) return textureLod(screen_texture, uv, lod).rgb;\r\n    vec2 reach = point_uv * exp2(lod) * 0.5;\r\n    vec3 sum = textureLod(screen_texture, clamp(uv + reach * vec2(0.5, 1.0), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(-1.0, 0.5), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(-0.5, -1.0), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(1.0, -0.5), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    return sum * 0.25;\r\n}\r\n\r\nvec3 readable_body(vec3 srgb) {\r\n    srgb = clamp(srgb, vec3(0.0), vec3(1.0));\r\n    vec3 linear = mix(srgb / 12.92, pow((srgb + 0.055) / 1.055, vec3(2.4)),\r\n                      step(vec3(0.04045), srgb));\r\n    float luminance = dot(linear, vec3(0.2126, 0.7152, 0.0722));\r\n    if (dark_mode < 0.5 && primary < 0.5) {\r\n        linear = mix(linear, vec3(1.0), max(0.0, (reading_limit - luminance) / max(1.0 - luminance, 0.001)));\r\n    } else if (added_ink > 0.5) {\r\n        // Deep shadows pass as they are; above them the backdrop is compressed toward the\r\n        // ceiling without ever reaching it, so bright artwork keeps its shading instead of\r\n        // being cut to one flat tone. The shoulder meets the toe at the same slope, so\r\n        // there is no seam between glass that transmits and glass that holds back.\r\n        float toe = reading_limit * 0.1;\r\n        float shoulder = reading_limit - toe;\r\n        float rolled = luminance <= toe ? luminance\r\n            : toe + shoulder * (luminance - toe) / (luminance - toe + shoulder);\r\n        linear *= rolled / max(luminance, 0.0001);\r\n        // Bright glass keeps part of its colour: an added ink brightens every channel of\r\n        // greyer glass, where a saturated channel would leave it nothing to add to.\r\n        linear = mix(vec3(rolled), linear, mix(1.0, 0.6, smoothstep(toe, reading_limit, rolled)));\r\n        float top = max(max(linear.r, linear.g), linear.b);\r\n        if (top > 1.0) linear = rolled + (linear - rolled) * (1.0 - rolled) / (top - rolled);\r\n    } else {\r\n        linear *= min(1.0, reading_limit / max(luminance, 0.001));\r\n    }\r\n    return mix(linear * 12.92, 1.055 * pow(linear, vec3(1.0 / 2.4)) - 0.055,\r\n               step(vec3(0.0031308), linear));\r\n}\r\n\r\nvoid fragment() {\r\n    vec2 extent = plate_size + vec2(bleed * 2.0);\r\n    float expansion = clamp(reveal, 0.0, 1.0);\r\n    float press = clamp(press_amount, -0.05, 1.1);\r\n    vec2 stretch = vec2(1.0) + (vec2(0.015, 0.028) * press + abs(lens_offset) * 0.004) * (1.0 - surface);\r\n    vec2 center = mix(reveal_origin - plate_size * 0.5, vec2(0.0), expansion);\r\n    vec2 p = (UV * extent - extent * 0.5 - lens_offset - center) / stretch;\r\n    vec2 half_size = max(mix(reveal_size, plate_size, expansion) * 0.5, vec2(1.0));\r\n    float r = min(radius, min(half_size.x, half_size.y));\r\n    // A continuous corner reaches further along each side than a circular one of the same\r\n    // radius; this keeps its visible rounding close to the native panel's.\r\n    if (r < min(half_size.x, half_size.y) - 0.5) r = min(r * 1.25, min(half_size.x, half_size.y));\r\n    float d = shape(p, half_size, r);\r\n    float aa = max(fwidth(d), 0.55);\r\n    float mask = 1.0 - smoothstep(-aa * 0.5, aa * 0.5, d);\r\n    vec2 grad = vec2(shape(p + vec2(0.25, 0.0), half_size, r) - shape(p - vec2(0.25, 0.0), half_size, r),\r\n                     shape(p + vec2(0.0, 0.25), half_size, r) - shape(p - vec2(0.0, 0.25), half_size, r));\r\n    vec2 outward = grad / max(length(grad), 0.0001);\r\n    // The bezel rises over its band along a convex squircle, h(x) = (1 - (1 - x)^4)^(1/4)\r\n    // (kube.io): steep at the rim, flat where the face begins, with no crease between.\r\n    float bevel = min(mix(9.0, 22.0, surface) + press * 0.7, r * 0.8);\r\n    float u = 1.0 - clamp(-min(d, 0.0) / max(bevel, 0.5), 0.0, 1.0);\r\n    float u4 = u * u * u * u;\r\n    float slope = min(u * u * u / pow(max(1.0 - u4, 0.0001), 0.75), 6.0);\r\n    // Rises a little taller than it is wide, so the rim visibly gathers the backdrop.\r\n    vec3 normal = normalize(vec3(outward * slope * 1.5, 1.0));\r\n    // Snell's law through crown glass, then across the bezel's height to the backdrop.\r\n    vec3 ray = refract(vec3(0.0, 0.0, -1.0), normal, 1.0 / 1.5);\r\n    float height = bevel * pow(max(1.0 - u4, 0.0), 0.25);\r\n    vec2 displacement = ray.xy * (height + bevel * 0.5) / max(-ray.z, 0.25);\r\n    displacement = clamp(displacement, vec2(-24.0), vec2(24.0));\r\n    vec2 pixel_uv = fwidth(SCREEN_UV) / max(fwidth(UV) * extent, vec2(0.00001));\r\n    // Clear through the bezel, where the bent backdrop is the material's signature, and\r\n    // frosted over the face, where it only has to carry color and movement.\r\n    float lod = mix(mix(1.6, 2.6, surface) + face_blur * (1.0 - u), 0.35, u * u);\r\n    vec2 uv = clamp(SCREEN_UV + displacement * pixel_uv, vec2(0.001), vec2(0.999));\r\n    vec3 bg = frosted(uv, lod, pixel_uv);\r\n    // About one pixel of dispersion at the strongest edge.\r\n    vec2 split = displacement * pixel_uv * 0.035;\r\n    bg.r = frosted(clamp(uv + split, vec2(0.001), vec2(0.999)), lod, pixel_uv).r;\r\n    bg.b = frosted(clamp(uv - split, vec2(0.001), vec2(0.999)), lod, pixel_uv).b;\r\n    float luma = dot(bg, vec3(0.2126, 0.7152, 0.0722));\r\n    // Vibrancy (Backdrop): transmitted color is lifted rather than washed out.\r\n    bg = mix(vec3(luma), bg, 1.4);\r\n    // A thin frost: readable_body below, not the tint, is what holds text contrast,\r\n    // so the fill only has to separate the glass from what it transmits.\r\n    float density = mix(0.20 + (1.0 - luma) * 0.14, 0.14 + luma * 0.30, dark_mode);\r\n    density = mix(density, mix(0.28, 0.24, dark_mode), surface);\r\n    vec3 body = mix(bg, tint.rgb, density);\r\n    // Colored glass maps transmitted luminance into the accent's tonal range, staying\r\n    // near the accent itself over dark content rather than sinking toward navy.\r\n    vec3 colored = tint.rgb * (0.86 + 0.20 * luma) + bg * 0.035;\r\n    body = mix(body, colored, primary);\r\n    vec2 to_pointer = p + half_size - pointer;\r\n    float touch = exp(-dot(to_pointer, to_pointer) / max(plate_size.y * plate_size.y * 1.6, 1.0));\r\n    body = mix(body, vec3(1.0), touch * (hover_amount * 0.018 + press * 0.07) * (1.0 - surface));\r\n    vec2 pointer_direction = clamp((pointer - half_size) / max(plate_size, vec2(1.0)), vec2(-0.5), vec2(0.5));\r\n    vec2 light_dir = normalize(vec2(-0.6, -0.8) + pointer_direction * hover_amount * 0.35);\r\n    float key = pow(max(dot(outward, light_dir), 0.0), 3.0);\r\n    float bounce = pow(max(dot(outward, -light_dir), 0.0), 4.0);\r\n    float fresnel = pow(1.0 - normal.z, 3.0);\r\n    float rim = exp(-pow((d + 0.65) / 0.85, 2.0));\r\n    // A hairline specular on the lit and opposite edges, and the light the bezel itself\r\n    // catches, strongest where it faces the key light and fading toward the face.\r\n    float glint = u * u * (0.03 + key * 0.07 + bounce * 0.03);\r\n    float shine = (rim * (0.035 + key * 0.30 + bounce * 0.13) + fresnel * (0.02 + key * 0.08) + glint)\r\n                  * mix(1.0, 0.7, surface);\r\n    body = mix(body, vec3(1.0), shine);\r\n    // A soft sky reflection across the upper face.\r\n    float from_top = (p.y + half_size.y) / max(min(half_size.y * 1.1, 48.0), 1.0);\r\n    body = mix(body, vec3(1.0), (1.0 - smoothstep(0.0, 1.0, from_top)) * mix(0.06, 0.035, dark_mode));\r\n    body *= 1.0 - exp(-pow((d + 1.8) / 1.35, 2.0)) * (1.0 - key) * 0.07;\r\n    // The outer band contains no text; retain its optical reflection, easing into the\r\n    // budget across the whole band. The entire content area, including the pointer\r\n    // highlight, stays within the contrast budget.\r\n    body = mix(body, readable_body(body), smoothstep(1.5, max(clear_rim, 3.5), -d));\r\n    float sd = max(shape(p - vec2(0.0, mix(2.0, 5.0, surface)), half_size, r), 0.0);\r\n    float shadow = exp(-sd * sd / mix(24.0, 60.0, surface)) * mix(0.11, 0.25, dark_mode);\r\n    float alpha = mask + shadow * (1.0 - mask);\r\n    COLOR = vec4(clamp(body, vec3(0.0), vec3(1.0)) * mask / max(alpha, 0.001), alpha);\r\n}"
					};
					_shader = val2;
					obj = (object)val2;
				}
				val.Shader = (Shader)obj;
				_material = val;
			}
			((CanvasItem)_plate).Material = (Material)(object)_material;
			_material.SetShaderParameter(StringName.op_Implicit("dark_mode"), Variant.op_Implicit(UiKit.Palette.IsDark ? 1f : 0f));
			_material.SetShaderParameter(StringName.op_Implicit("tint"), Variant.op_Implicit(_primary ? UiKit.AccentStrong : UiKit.Palette.SurfaceRaised));
			_material.SetShaderParameter(StringName.op_Implicit("primary"), Variant.op_Implicit(_primary ? 1f : 0f));
			_material.SetShaderParameter(StringName.op_Implicit("surface"), Variant.op_Implicit(_surface ? 1f : 0f));
			_material.SetShaderParameter(StringName.op_Implicit("radius"), Variant.op_Implicit(_surface ? 22f : 999f));
			_material.SetShaderParameter(StringName.op_Implicit("clear_rim"), Variant.op_Implicit(_surface ? 12f : 3.5f));
			_material.SetShaderParameter(StringName.op_Implicit("face_blur"), Variant.op_Implicit(_surface ? 1f : 0f));
			bool flag = _surface && UiKit.Palette.IsDark;
			_material.SetShaderParameter(StringName.op_Implicit("added_ink"), Variant.op_Implicit(flag ? 1f : 0f));
			_material.SetShaderParameter(StringName.op_Implicit("reading_limit"), Variant.op_Implicit(flag ? 0.155f : ReadingLimit(_text)));
		}

		internal void Bind(BaseButton button)
		{
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Expected O, but got Unknown
			((Control)button).MouseEntered += delegate
			{
				_hovered = true;
				Aim();
			};
			((Control)button).MouseExited += delegate
			{
				_hovered = false;
				Aim();
			};
			button.ButtonDown += delegate
			{
				if (!button.Disabled)
				{
					_pressedSource = button;
					_pressed = true;
					Aim();
				}
			};
			button.ButtonUp += delegate
			{
				if (_pressedSource == button)
				{
					_pressedSource = null;
					_pressed = false;
					Aim();
				}
			};
			((CanvasItem)button).Draw += delegate
			{
				if (button.Disabled && _pressedSource == button)
				{
					_pressedSource = null;
					_pressed = false;
					Aim();
				}
			};
			BaseButton obj = button;
			OptionButton val = (OptionButton)(object)((obj is OptionButton) ? obj : null);
			if (val != null)
			{
				((Popup)val.GetPopup()).PopupHide += delegate
				{
					_pressed = (_hovered = false);
					Aim();
				};
			}
			((Control)button).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
			{
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_0047: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0081: Unknown result type (might be due to invalid IL or missing references)
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				if (!button.Disabled)
				{
					InputEventMouse val2 = (InputEventMouse)(object)((input is InputEventMouse) ? input : null);
					if (val2 != null)
					{
						Lens lens = this;
						Transform2D globalTransformWithCanvas = ((CanvasItem)_host).GetGlobalTransformWithCanvas();
						lens.Point(((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * (((CanvasItem)button).GetGlobalTransformWithCanvas() * val2.Position));
					}
					else if ((input is InputEventKey || input is InputEventJoypadButton) ? true : false)
					{
						Point(_host.Size * 0.5f);
					}
				}
			};
		}

		internal void Point(Vector2 point)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_pointer = point;
			Aim();
		}

		internal void Hover(bool value)
		{
			_hovered = value;
			Aim();
		}

		internal void Press(bool value)
		{
			_pressed = value;
			Aim();
		}

		private void Aim()
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			if (((CanvasItem)_plate).Visible && _material != null)
			{
				_hover.Target = ((_hovered || _pressed) ? 1 : 0);
				_press.Target = (_pressed ? 1 : 0);
				Vector2 val = (_pointer - _host.Size * 0.5f) * 0.035f;
				_x.Target = (_pressed ? (2.2f * MathF.Tanh(val.X / 2.2f)) : 0f);
				_y.Target = (_pressed ? (1.6f * MathF.Tanh(val.Y / 1.6f)) : 0f);
				Write();
				if (_clock.IsStopped())
				{
					_lastTick = Time.GetTicksUsec();
					_clock.Start(-1.0);
				}
			}
		}

		private void Tick()
		{
			ulong ticksUsec = Time.GetTicksUsec();
			float seconds = Math.Min((float)(ticksUsec - _lastTick) / 1000000f, 0.1f);
			_lastTick = ticksUsec;
			_hover.Step(seconds, 5f, 0.9f);
			_press.Step(seconds);
			_x.Step(seconds, 4f, 0.75f);
			_y.Step(seconds, 4f, 0.75f);
			Write();
			if (_hover.Settled && _press.Settled && _x.Settled && _y.Settled)
			{
				_clock.Stop();
			}
		}

		private void Write()
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			if (_material != null)
			{
				_material.SetShaderParameter(StringName.op_Implicit("hover_amount"), Variant.op_Implicit(_hover.Value));
				_material.SetShaderParameter(StringName.op_Implicit("press_amount"), Variant.op_Implicit(_press.Value));
				_material.SetShaderParameter(StringName.op_Implicit("lens_offset"), Variant.op_Implicit(new Vector2(_x.Value, _y.Value)));
				_material.SetShaderParameter(StringName.op_Implicit("pointer"), Variant.op_Implicit(_pointer));
			}
		}

		private void Stop()
		{
			_clock.Stop();
			_hovered = (_pressed = false);
			_pressedSource = null;
			_hover.Reset(0f);
			_press.Reset(0f);
			_x.Reset(0f);
			_y.Reset(0f);
			Write();
		}
	}

	internal const float Bleed = 16f;

	private static Shader? _shader;

	private static Shader? _backdropShader;

	private static Shader? _edgeShader;

	private const double ReadingTarget = 4.65;

	private const float CaptureMargin = 128f;

	private const float CaptureAlign = 32f;

	private static readonly bool BoundedCopies;

	private static readonly bool LevelsFollowBounds;

	internal const float AddedInkReadingLimit = 0.155f;

	private const float EdgeTopBand = 280f;

	private const float EdgeBottomBand = 220f;

	internal const string ShaderCode = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap, repeat_disable;\r\nuniform vec2 plate_size = vec2(300.0, 40.0);\r\nuniform float radius = 20.0;\r\nuniform float dark_mode = 0.0;\r\nuniform float primary = 0.0;\r\nuniform float surface = 0.0;\r\nuniform float bleed = 16.0;\r\nuniform float reveal = 1.0;\r\nuniform vec2 reveal_origin = vec2(0.0);\r\nuniform vec2 reveal_size = vec2(80.0, 30.0);\r\nuniform vec4 tint : source_color = vec4(0.98, 0.98, 1.0, 1.0);\r\nuniform float hover_amount = 0.0;\r\nuniform float press_amount = 0.0;\r\nuniform vec2 lens_offset = vec2(0.0);\r\nuniform vec2 pointer = vec2(100.0, 20.0);\r\n// Floor for light glass, ceiling for dark and colored glass; see ReadingLimit.\r\nuniform float reading_limit = 0.65;\r\n// The outer band, in plate points, that never carries text and keeps its full optics.\r\nuniform float clear_rim = 3.5;\r\n// 1 where the text on this glass is added to it rather than painted over it; see AddedInk.\r\nuniform float added_ink = 0.0;\r\n// Further blurred levels across the face, for a surface that carries text throughout.\r\nuniform float face_blur = 0.0;\r\n\r\n// Rounded corners turn continuous (a superellipse, as in Liquid Glass Studio) wherever the\r\n// radius is shorter than the half extent; capsule ends stay circular.\r\nfloat shape(vec2 p, vec2 half_size, float r) {\r\n    vec2 q = abs(p) - half_size + r;\r\n    vec2 c = max(q, vec2(0.0));\r\n    float n = r < min(half_size.x, half_size.y) - 0.5 ? 2.6 : 2.0;\r\n    float corner = pow(pow(c.x, n) + pow(c.y, n), 1.0 / n);\r\n    return corner + min(max(q.x, q.y), 0.0) - r;\r\n}\r\n\r\n// Canvas colors are sRGB with the launcher's non-HDR 2D viewport. Apply the text\r\n// budget in linear light, after refraction, press highlights and rim illumination.\r\n// A deeply blurred level, magnified by the hardware, shows its texel grid as soft facets.\r\n// Where the face is frosted further, four taps half a texel apart on a rotated grid fold\r\n// those facets into one smooth wash.\r\nvec3 frosted(vec2 uv, float lod, vec2 point_uv) {\r\n    if (face_blur <= 0.0) return textureLod(screen_texture, uv, lod).rgb;\r\n    vec2 reach = point_uv * exp2(lod) * 0.5;\r\n    vec3 sum = textureLod(screen_texture, clamp(uv + reach * vec2(0.5, 1.0), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(-1.0, 0.5), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(-0.5, -1.0), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(1.0, -0.5), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    return sum * 0.25;\r\n}\r\n\r\nvec3 readable_body(vec3 srgb) {\r\n    srgb = clamp(srgb, vec3(0.0), vec3(1.0));\r\n    vec3 linear = mix(srgb / 12.92, pow((srgb + 0.055) / 1.055, vec3(2.4)),\r\n                      step(vec3(0.04045), srgb));\r\n    float luminance = dot(linear, vec3(0.2126, 0.7152, 0.0722));\r\n    if (dark_mode < 0.5 && primary < 0.5) {\r\n        linear = mix(linear, vec3(1.0), max(0.0, (reading_limit - luminance) / max(1.0 - luminance, 0.001)));\r\n    } else if (added_ink > 0.5) {\r\n        // Deep shadows pass as they are; above them the backdrop is compressed toward the\r\n        // ceiling without ever reaching it, so bright artwork keeps its shading instead of\r\n        // being cut to one flat tone. The shoulder meets the toe at the same slope, so\r\n        // there is no seam between glass that transmits and glass that holds back.\r\n        float toe = reading_limit * 0.1;\r\n        float shoulder = reading_limit - toe;\r\n        float rolled = luminance <= toe ? luminance\r\n            : toe + shoulder * (luminance - toe) / (luminance - toe + shoulder);\r\n        linear *= rolled / max(luminance, 0.0001);\r\n        // Bright glass keeps part of its colour: an added ink brightens every channel of\r\n        // greyer glass, where a saturated channel would leave it nothing to add to.\r\n        linear = mix(vec3(rolled), linear, mix(1.0, 0.6, smoothstep(toe, reading_limit, rolled)));\r\n        float top = max(max(linear.r, linear.g), linear.b);\r\n        if (top > 1.0) linear = rolled + (linear - rolled) * (1.0 - rolled) / (top - rolled);\r\n    } else {\r\n        linear *= min(1.0, reading_limit / max(luminance, 0.001));\r\n    }\r\n    return mix(linear * 12.92, 1.055 * pow(linear, vec3(1.0 / 2.4)) - 0.055,\r\n               step(vec3(0.0031308), linear));\r\n}\r\n\r\nvoid fragment() {\r\n    vec2 extent = plate_size + vec2(bleed * 2.0);\r\n    float expansion = clamp(reveal, 0.0, 1.0);\r\n    float press = clamp(press_amount, -0.05, 1.1);\r\n    vec2 stretch = vec2(1.0) + (vec2(0.015, 0.028) * press + abs(lens_offset) * 0.004) * (1.0 - surface);\r\n    vec2 center = mix(reveal_origin - plate_size * 0.5, vec2(0.0), expansion);\r\n    vec2 p = (UV * extent - extent * 0.5 - lens_offset - center) / stretch;\r\n    vec2 half_size = max(mix(reveal_size, plate_size, expansion) * 0.5, vec2(1.0));\r\n    float r = min(radius, min(half_size.x, half_size.y));\r\n    // A continuous corner reaches further along each side than a circular one of the same\r\n    // radius; this keeps its visible rounding close to the native panel's.\r\n    if (r < min(half_size.x, half_size.y) - 0.5) r = min(r * 1.25, min(half_size.x, half_size.y));\r\n    float d = shape(p, half_size, r);\r\n    float aa = max(fwidth(d), 0.55);\r\n    float mask = 1.0 - smoothstep(-aa * 0.5, aa * 0.5, d);\r\n    vec2 grad = vec2(shape(p + vec2(0.25, 0.0), half_size, r) - shape(p - vec2(0.25, 0.0), half_size, r),\r\n                     shape(p + vec2(0.0, 0.25), half_size, r) - shape(p - vec2(0.0, 0.25), half_size, r));\r\n    vec2 outward = grad / max(length(grad), 0.0001);\r\n    // The bezel rises over its band along a convex squircle, h(x) = (1 - (1 - x)^4)^(1/4)\r\n    // (kube.io): steep at the rim, flat where the face begins, with no crease between.\r\n    float bevel = min(mix(9.0, 22.0, surface) + press * 0.7, r * 0.8);\r\n    float u = 1.0 - clamp(-min(d, 0.0) / max(bevel, 0.5), 0.0, 1.0);\r\n    float u4 = u * u * u * u;\r\n    float slope = min(u * u * u / pow(max(1.0 - u4, 0.0001), 0.75), 6.0);\r\n    // Rises a little taller than it is wide, so the rim visibly gathers the backdrop.\r\n    vec3 normal = normalize(vec3(outward * slope * 1.5, 1.0));\r\n    // Snell's law through crown glass, then across the bezel's height to the backdrop.\r\n    vec3 ray = refract(vec3(0.0, 0.0, -1.0), normal, 1.0 / 1.5);\r\n    float height = bevel * pow(max(1.0 - u4, 0.0), 0.25);\r\n    vec2 displacement = ray.xy * (height + bevel * 0.5) / max(-ray.z, 0.25);\r\n    displacement = clamp(displacement, vec2(-24.0), vec2(24.0));\r\n    vec2 pixel_uv = fwidth(SCREEN_UV) / max(fwidth(UV) * extent, vec2(0.00001));\r\n    // Clear through the bezel, where the bent backdrop is the material's signature, and\r\n    // frosted over the face, where it only has to carry color and movement.\r\n    float lod = mix(mix(1.6, 2.6, surface) + face_blur * (1.0 - u), 0.35, u * u);\r\n    vec2 uv = clamp(SCREEN_UV + displacement * pixel_uv, vec2(0.001), vec2(0.999));\r\n    vec3 bg = frosted(uv, lod, pixel_uv);\r\n    // About one pixel of dispersion at the strongest edge.\r\n    vec2 split = displacement * pixel_uv * 0.035;\r\n    bg.r = frosted(clamp(uv + split, vec2(0.001), vec2(0.999)), lod, pixel_uv).r;\r\n    bg.b = frosted(clamp(uv - split, vec2(0.001), vec2(0.999)), lod, pixel_uv).b;\r\n    float luma = dot(bg, vec3(0.2126, 0.7152, 0.0722));\r\n    // Vibrancy (Backdrop): transmitted color is lifted rather than washed out.\r\n    bg = mix(vec3(luma), bg, 1.4);\r\n    // A thin frost: readable_body below, not the tint, is what holds text contrast,\r\n    // so the fill only has to separate the glass from what it transmits.\r\n    float density = mix(0.20 + (1.0 - luma) * 0.14, 0.14 + luma * 0.30, dark_mode);\r\n    density = mix(density, mix(0.28, 0.24, dark_mode), surface);\r\n    vec3 body = mix(bg, tint.rgb, density);\r\n    // Colored glass maps transmitted luminance into the accent's tonal range, staying\r\n    // near the accent itself over dark content rather than sinking toward navy.\r\n    vec3 colored = tint.rgb * (0.86 + 0.20 * luma) + bg * 0.035;\r\n    body = mix(body, colored, primary);\r\n    vec2 to_pointer = p + half_size - pointer;\r\n    float touch = exp(-dot(to_pointer, to_pointer) / max(plate_size.y * plate_size.y * 1.6, 1.0));\r\n    body = mix(body, vec3(1.0), touch * (hover_amount * 0.018 + press * 0.07) * (1.0 - surface));\r\n    vec2 pointer_direction = clamp((pointer - half_size) / max(plate_size, vec2(1.0)), vec2(-0.5), vec2(0.5));\r\n    vec2 light_dir = normalize(vec2(-0.6, -0.8) + pointer_direction * hover_amount * 0.35);\r\n    float key = pow(max(dot(outward, light_dir), 0.0), 3.0);\r\n    float bounce = pow(max(dot(outward, -light_dir), 0.0), 4.0);\r\n    float fresnel = pow(1.0 - normal.z, 3.0);\r\n    float rim = exp(-pow((d + 0.65) / 0.85, 2.0));\r\n    // A hairline specular on the lit and opposite edges, and the light the bezel itself\r\n    // catches, strongest where it faces the key light and fading toward the face.\r\n    float glint = u * u * (0.03 + key * 0.07 + bounce * 0.03);\r\n    float shine = (rim * (0.035 + key * 0.30 + bounce * 0.13) + fresnel * (0.02 + key * 0.08) + glint)\r\n                  * mix(1.0, 0.7, surface);\r\n    body = mix(body, vec3(1.0), shine);\r\n    // A soft sky reflection across the upper face.\r\n    float from_top = (p.y + half_size.y) / max(min(half_size.y * 1.1, 48.0), 1.0);\r\n    body = mix(body, vec3(1.0), (1.0 - smoothstep(0.0, 1.0, from_top)) * mix(0.06, 0.035, dark_mode));\r\n    body *= 1.0 - exp(-pow((d + 1.8) / 1.35, 2.0)) * (1.0 - key) * 0.07;\r\n    // The outer band contains no text; retain its optical reflection, easing into the\r\n    // budget across the whole band. The entire content area, including the pointer\r\n    // highlight, stays within the contrast budget.\r\n    body = mix(body, readable_body(body), smoothstep(1.5, max(clear_rim, 3.5), -d));\r\n    float sd = max(shape(p - vec2(0.0, mix(2.0, 5.0, surface)), half_size, r), 0.0);\r\n    float shadow = exp(-sd * sd / mix(24.0, 60.0, surface)) * mix(0.11, 0.25, dark_mode);\r\n    float alpha = mask + shadow * (1.0 - mask);\r\n    COLOR = vec4(clamp(body, vec3(0.0), vec3(1.0)) * mask / max(alpha, 0.001), alpha);\r\n}";

	internal const string EdgeCode = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap, repeat_disable;\r\nuniform float dark_mode = 0.0;\r\nuniform vec2 logical_size = vec2(1600.0, 1000.0);\r\nuniform float reading_limit = 0.65;\r\nvoid fragment() {\r\n    // Header, presets and two-line feedback sit on the backdrop itself. Logical\r\n    // coordinates keep the bands on that text at any DPI or content scale.\r\n    float top = 1.0 - smoothstep(160.0, 280.0, UV.y * logical_size.y);\r\n    float bottom = 1.0 - smoothstep(100.0, 220.0, (1.0 - UV.y) * logical_size.y);\r\n    float band = max(top, bottom);\r\n    if (band < 0.002) discard;\r\n    // Frost progressively toward the window edge, then settle the budget in linear light.\r\n    vec3 srgb = textureLod(screen_texture, SCREEN_UV, 2.6 * band).rgb;\r\n    vec3 linear = mix(srgb / 12.92, pow((srgb + 0.055) / 1.055, vec3(2.4)), step(vec3(0.04045), srgb));\r\n    float luminance = dot(linear, vec3(0.2126, 0.7152, 0.0722));\r\n    if (dark_mode < 0.5)\r\n        linear = mix(linear, vec3(1.0), max(0.0, (reading_limit - luminance) / max(1.0 - luminance, 0.001)));\r\n    else\r\n        linear *= min(1.0, reading_limit / max(luminance, 0.0001));\r\n    vec3 settled = mix(linear * 12.92, 1.055 * pow(linear, vec3(1.0 / 2.4)) - 0.055,\r\n                       step(vec3(0.0031308), linear));\r\n    COLOR = vec4(clamp(settled, vec3(0.0), vec3(1.0)), band);\r\n}";

	internal const string BackdropCode = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform float dark_mode = 0.0;\r\nuniform vec2 logical_size = vec2(1600.0, 1000.0);\r\n\r\nfloat glow(vec2 p, vec2 center, vec2 spread) {\r\n    vec2 q = (p - center) / spread;\r\n    return exp(-dot(q, q));\r\n}\r\n\r\nvoid fragment() {\r\n    // Aspect-correct: x runs 0..aspect so the ribbon keeps its curvature on any window.\r\n    float aspect = logical_size.x / max(logical_size.y, 1.0);\r\n    vec2 p = vec2(UV.x * aspect, UV.y);\r\n    float u = UV.x;\r\n    vec3 top = mix(vec3(0.905, 0.937, 0.988), vec3(0.036, 0.058, 0.118), dark_mode);\r\n    vec3 bottom = mix(vec3(0.949, 0.929, 0.973), vec3(0.052, 0.040, 0.100), dark_mode);\r\n    vec3 base = mix(top, bottom, smoothstep(0.0, 1.0, UV.y));\r\n    base = mix(base, mix(vec3(0.855, 0.835, 0.980), vec3(0.118, 0.098, 0.300), dark_mode),\r\n               glow(p, vec2(aspect * 0.86, 0.10), vec2(aspect * 0.34, 0.42)) * 0.75);\r\n    base = mix(base, mix(vec3(0.992, 0.878, 0.827), vec3(0.170, 0.066, 0.180), dark_mode),\r\n               glow(p, vec2(aspect * 0.08, 0.92), vec2(aspect * 0.30, 0.36)) * 0.70);\r\n    // Light pooling under the action bar gives its glass something to transmit.\r\n    base = mix(base, mix(vec3(0.690, 0.840, 0.985), vec3(0.036, 0.170, 0.330), dark_mode),\r\n               glow(p, vec2(aspect * 0.84, 1.02), vec2(aspect * 0.24, 0.20)) * 0.65);\r\n    // A broad glass ribbon rising from the bottom edge to the right, lit along its upper\r\n    // crest. It passes behind the inspector and never meets the left edge, where only a\r\n    // cut-off sliver of it would show.\r\n    float crest = 1.22 - 0.98 * u + 0.05 * sin(u * 5.2 + 0.7);\r\n    float d = UV.y - crest;\r\n    float body = exp(-pow(d / 0.12, 2.0)) * smoothstep(-0.02, 0.0, d + 0.10);\r\n    base = mix(base, mix(vec3(0.545, 0.765, 0.965), vec3(0.047, 0.212, 0.420), dark_mode), body * 0.72);\r\n    base = mix(base, mix(vec3(0.690, 0.890, 0.930), vec3(0.030, 0.230, 0.300), dark_mode),\r\n               exp(-pow((d - 0.12) / 0.07, 2.0)) * 0.45);\r\n    // The crest reads as the ribbon's lit edge, not a hairline, where it meets the window edge.\r\n    float line = exp(-pow((d + 0.085) / 0.024, 2.0));\r\n    base = mix(base, mix(vec3(1.0), vec3(0.330, 0.540, 0.860), dark_mode), line * mix(0.55, 0.40, dark_mode));\r\n    // Sub-LSB dither: long 8-bit gradients band visibly on large displays.\r\n    float noise = fract(sin(dot(FRAGCOORD.xy, vec2(12.9898, 78.233))) * 43758.5453) - 0.5;\r\n    COLOR = vec4(base + noise / 255.0, 1.0);\r\n}";

	private static Shader? _menuShader;

	private const float MenuBleed = 10f;

	private static readonly StringName GlassMenuPending;

	private const float SnapshotRefraction = 24f;

	private const float SnapshotBlur = 32f;

	private const int SnapshotAlign = 32;

	private const int SnapshotDeepestBlock = 8;

	internal static float ReadingLimit(GlassText text)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		object obj = text switch
		{
			GlassText.Body => new Color[1] { UiKit.Text }, 
			GlassText.OnAccent => new Color[1] { UiKit.OnAccent }, 
			_ => new Color[7]
			{
				UiKit.Text,
				UiKit.TextMuted,
				UiKit.TextDim,
				UiKit.Success,
				UiKit.Warning,
				UiKit.Danger,
				UiKit.Accent
			}, 
		};
		bool flag = UiKit.Palette.IsDark || text == GlassText.OnAccent;
		double num = (flag ? 1 : 0);
		Color[] array = (Color[])obj;
		for (int i = 0; i < array.Length; i++)
		{
			double num2 = Luminance(array[i]);
			num = (flag ? Math.Min(num, (num2 + 0.05) / 4.65 - 0.05) : Math.Max(num, 4.65 * (num2 + 0.05) - 0.05));
		}
		return (float)Math.Clamp(num, 0.0, 1.0);
	}

	private static double Luminance(Color c)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		return 0.2126 * Linear(c.R) + 0.7152 * Linear(c.G) + 0.0722 * Linear(c.B);
		static double Linear(double v)
		{
			if (!(v <= 0.04045))
			{
				return Math.Pow((v + 0.055) / 1.055, 2.4);
			}
			return v / 12.92;
		}
	}

	internal static PanelContainer Group(Control content)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0046: Expected O, but got Unknown
		PanelContainer val = new PanelContainer
		{
			ThemeTypeVariation = StringName.op_Implicit("GlassGroupPanel"),
			SizeFlagsHorizontal = (SizeFlags)4,
			SizeFlagsVertical = (SizeFlags)4
		};
		((Node)val).AddChild((Node)(object)content, false, (InternalMode)0);
		Lens lens = Decorate((Control)val, primary: false, surface: false, GlassText.All);
		BindChildren((Node)(object)content, lens);
		return val;
	}

	private static void BindChildren(Node node, Lens lens)
	{
		BaseButton val = (BaseButton)(object)((node is BaseButton) ? node : null);
		if (val != null)
		{
			lens.Bind(val);
		}
		foreach (Node child in node.GetChildren(false))
		{
			BindChildren(child, lens);
		}
	}

	internal static Lens Decorate(Control host, bool primary = false, bool surface = false, GlassText? text = null)
	{
		Lens result = new Lens(host, primary, surface, (GlassText)(((int?)text) ?? (primary ? 2 : (surface ? 1 : 0))));
		OptionButton val = (OptionButton)(object)((host is OptionButton) ? host : null);
		if (val != null)
		{
			AttachMenu(val);
		}
		return result;
	}

	internal static void DecorateSlider(HSlider slider)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected O, but got Unknown
		((Control)slider).ThemeTypeVariation = StringName.op_Implicit("GlassSlider");
		Control thumb = new Control
		{
			Name = StringName.op_Implicit("GlassThumb"),
			MouseFilter = (MouseFilterEnum)2,
			Size = new Vector2(24f, 24f)
		};
		((Node)slider).AddChild((Node)(object)thumb, false, (InternalMode)0);
		Lens lens = Decorate(thumb, primary: false, surface: false, null);
		((Range)slider).ValueChanged += (ValueChangedEventHandler)delegate
		{
			Geometry();
		};
		((Control)slider).Resized += Geometry;
		((Control)slider).ThemeChanged += Geometry;
		((Node)slider).TreeEntered += Geometry;
		((CanvasItem)slider).VisibilityChanged += Geometry;
		((CanvasItem)slider).Draw += Geometry;
		((Control)slider).MouseEntered += delegate
		{
			lens.Hover(value: true);
		};
		((Control)slider).MouseExited += delegate
		{
			lens.Hover(value: false);
		};
		((Slider)slider).DragStarted += delegate
		{
			lens.Press(value: true);
		};
		((Slider)slider).DragEnded += (DragEndedEventHandler)delegate
		{
			lens.Press(value: false);
		};
		((Control)slider).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			InputEventMouse val = (InputEventMouse)(object)((input is InputEventMouse) ? input : null);
			if (val != null)
			{
				lens.Point(val.Position - thumb.Position);
			}
		};
		void Geometry()
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			float num = ((((Range)slider).MaxValue > ((Range)slider).MinValue) ? ((float)((((Range)slider).Value - ((Range)slider).MinValue) / (((Range)slider).MaxValue - ((Range)slider).MinValue))) : 0f);
			if (((Control)slider).IsLayoutRtl())
			{
				num = 1f - num;
			}
			thumb.Position = new Vector2(Math.Max(((Control)slider).Size.X - 24f, 0f) * num, (((Control)slider).Size.Y - 24f) * 0.5f);
			((CanvasItem)thumb).Visible = UiKit.Palette.IsLiquidGlass && ((Slider)slider).Editable && ((CanvasItem)slider).IsVisibleInTree();
		}
	}

	internal static ColorRect Backdrop()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		ColorRect rect = new ColorRect
		{
			MouseFilter = (MouseFilterEnum)2
		};
		ShaderMaterial val = new ShaderMaterial();
		object obj = _backdropShader;
		if (obj == null)
		{
			Shader val2 = new Shader
			{
				Code = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform float dark_mode = 0.0;\r\nuniform vec2 logical_size = vec2(1600.0, 1000.0);\r\n\r\nfloat glow(vec2 p, vec2 center, vec2 spread) {\r\n    vec2 q = (p - center) / spread;\r\n    return exp(-dot(q, q));\r\n}\r\n\r\nvoid fragment() {\r\n    // Aspect-correct: x runs 0..aspect so the ribbon keeps its curvature on any window.\r\n    float aspect = logical_size.x / max(logical_size.y, 1.0);\r\n    vec2 p = vec2(UV.x * aspect, UV.y);\r\n    float u = UV.x;\r\n    vec3 top = mix(vec3(0.905, 0.937, 0.988), vec3(0.036, 0.058, 0.118), dark_mode);\r\n    vec3 bottom = mix(vec3(0.949, 0.929, 0.973), vec3(0.052, 0.040, 0.100), dark_mode);\r\n    vec3 base = mix(top, bottom, smoothstep(0.0, 1.0, UV.y));\r\n    base = mix(base, mix(vec3(0.855, 0.835, 0.980), vec3(0.118, 0.098, 0.300), dark_mode),\r\n               glow(p, vec2(aspect * 0.86, 0.10), vec2(aspect * 0.34, 0.42)) * 0.75);\r\n    base = mix(base, mix(vec3(0.992, 0.878, 0.827), vec3(0.170, 0.066, 0.180), dark_mode),\r\n               glow(p, vec2(aspect * 0.08, 0.92), vec2(aspect * 0.30, 0.36)) * 0.70);\r\n    // Light pooling under the action bar gives its glass something to transmit.\r\n    base = mix(base, mix(vec3(0.690, 0.840, 0.985), vec3(0.036, 0.170, 0.330), dark_mode),\r\n               glow(p, vec2(aspect * 0.84, 1.02), vec2(aspect * 0.24, 0.20)) * 0.65);\r\n    // A broad glass ribbon rising from the bottom edge to the right, lit along its upper\r\n    // crest. It passes behind the inspector and never meets the left edge, where only a\r\n    // cut-off sliver of it would show.\r\n    float crest = 1.22 - 0.98 * u + 0.05 * sin(u * 5.2 + 0.7);\r\n    float d = UV.y - crest;\r\n    float body = exp(-pow(d / 0.12, 2.0)) * smoothstep(-0.02, 0.0, d + 0.10);\r\n    base = mix(base, mix(vec3(0.545, 0.765, 0.965), vec3(0.047, 0.212, 0.420), dark_mode), body * 0.72);\r\n    base = mix(base, mix(vec3(0.690, 0.890, 0.930), vec3(0.030, 0.230, 0.300), dark_mode),\r\n               exp(-pow((d - 0.12) / 0.07, 2.0)) * 0.45);\r\n    // The crest reads as the ribbon's lit edge, not a hairline, where it meets the window edge.\r\n    float line = exp(-pow((d + 0.085) / 0.024, 2.0));\r\n    base = mix(base, mix(vec3(1.0), vec3(0.330, 0.540, 0.860), dark_mode), line * mix(0.55, 0.40, dark_mode));\r\n    // Sub-LSB dither: long 8-bit gradients band visibly on large displays.\r\n    float noise = fract(sin(dot(FRAGCOORD.xy, vec2(12.9898, 78.233))) * 43758.5453) - 0.5;\r\n    COLOR = vec4(base + noise / 255.0, 1.0);\r\n}"
			};
			_backdropShader = val2;
			obj = (object)val2;
		}
		val.Shader = (Shader)obj;
		ShaderMaterial material = val;
		((CanvasItem)rect).Material = (Material)(object)material;
		((Node)rect).TreeEntered += Refresh;
		((Control)rect).ThemeChanged += Refresh;
		((Control)rect).Resized += Refresh;
		return UiKit.FullRect<ColorRect>(rect);
		void Refresh()
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			((CanvasItem)rect).Visible = UiKit.Palette.IsLiquidGlass && !UiKit.HasCustomBackdrop;
			material.SetShaderParameter(StringName.op_Implicit("dark_mode"), Variant.op_Implicit(UiKit.Palette.IsDark ? 1f : 0f));
			material.SetShaderParameter(StringName.op_Implicit("logical_size"), Variant.op_Implicit(((Control)rect).Size));
		}
	}

	internal static Control ScrollEdge()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		Control edge = UiKit.FullRect<Control>(new Control
		{
			Name = StringName.op_Implicit("ScrollEdge"),
			MouseFilter = (MouseFilterEnum)2
		});
		ShaderMaterial val = new ShaderMaterial();
		object obj = _edgeShader;
		if (obj == null)
		{
			Shader val2 = new Shader
			{
				Code = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap, repeat_disable;\r\nuniform float dark_mode = 0.0;\r\nuniform vec2 logical_size = vec2(1600.0, 1000.0);\r\nuniform float reading_limit = 0.65;\r\nvoid fragment() {\r\n    // Header, presets and two-line feedback sit on the backdrop itself. Logical\r\n    // coordinates keep the bands on that text at any DPI or content scale.\r\n    float top = 1.0 - smoothstep(160.0, 280.0, UV.y * logical_size.y);\r\n    float bottom = 1.0 - smoothstep(100.0, 220.0, (1.0 - UV.y) * logical_size.y);\r\n    float band = max(top, bottom);\r\n    if (band < 0.002) discard;\r\n    // Frost progressively toward the window edge, then settle the budget in linear light.\r\n    vec3 srgb = textureLod(screen_texture, SCREEN_UV, 2.6 * band).rgb;\r\n    vec3 linear = mix(srgb / 12.92, pow((srgb + 0.055) / 1.055, vec3(2.4)), step(vec3(0.04045), srgb));\r\n    float luminance = dot(linear, vec3(0.2126, 0.7152, 0.0722));\r\n    if (dark_mode < 0.5)\r\n        linear = mix(linear, vec3(1.0), max(0.0, (reading_limit - luminance) / max(1.0 - luminance, 0.001)));\r\n    else\r\n        linear *= min(1.0, reading_limit / max(luminance, 0.0001));\r\n    vec3 settled = mix(linear * 12.92, 1.055 * pow(linear, vec3(1.0 / 2.4)) - 0.055,\r\n                       step(vec3(0.0031308), linear));\r\n    COLOR = vec4(clamp(settled, vec3(0.0), vec3(1.0)), band);\r\n}"
			};
			_edgeShader = val2;
			obj = (object)val2;
		}
		val.Shader = (Shader)obj;
		ShaderMaterial material = val;
		(Control Strip, ColorRect Plate, Capture Copy)[] bands = new(Control, ColorRect, Capture)[2]
		{
			Band("Top", top: true),
			Band("Bottom", top: false)
		};
		((Node)edge).TreeEntered += Refresh;
		edge.ThemeChanged += Refresh;
		edge.Resized += Refresh;
		((CanvasItem)edge).VisibilityChanged += FollowVisibility;
		((Node)edge).TreeExiting += delegate
		{
			(Control, ColorRect, Capture)[] array = bands;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Item3.Enable(enabled: false);
			}
		};
		return edge;
		(Control, ColorRect, Capture) Band(string name, bool top)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			Control val3 = new Control
			{
				Name = StringName.op_Implicit(name),
				MouseFilter = (MouseFilterEnum)2,
				ClipContents = true
			};
			val3.AnchorRight = 1f;
			float anchorTop = (val3.AnchorBottom = ((!top) ? 1 : 0));
			val3.AnchorTop = anchorTop;
			if (top)
			{
				val3.OffsetBottom = 280f;
			}
			else
			{
				val3.OffsetTop = -220f;
			}
			Capture capture = new Capture(val3, 0f, 3);
			ColorRect val4 = new ColorRect
			{
				Name = StringName.op_Implicit("Plate"),
				MouseFilter = (MouseFilterEnum)2,
				Material = (Material)(object)material
			};
			((Node)val3).AddChild((Node)(object)capture.Node, false, (InternalMode)0);
			((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)edge).AddChild((Node)(object)val3, false, (InternalMode)0);
			return (val3, val4, capture);
		}
		void FollowVisibility()
		{
			(Control, ColorRect, Capture)[] array3 = bands;
			for (int k = 0; k < array3.Length; k++)
			{
				array3[k].Item3.Enable(((CanvasItem)edge).IsVisibleInTree());
			}
		}
		void Refresh()
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			((CanvasItem)edge).Visible = UiKit.Palette.IsLiquidGlass;
			material.SetShaderParameter(StringName.op_Implicit("dark_mode"), Variant.op_Implicit(UiKit.Palette.IsDark ? 1f : 0f));
			material.SetShaderParameter(StringName.op_Implicit("reading_limit"), Variant.op_Implicit(ReadingLimit(GlassText.All)));
			material.SetShaderParameter(StringName.op_Implicit("logical_size"), Variant.op_Implicit(edge.Size));
			(Control, ColorRect, Capture)[] array2 = bands;
			for (int j = 0; j < array2.Length; j++)
			{
				var (val5, val6, _) = array2[j];
				((Control)val6).Position = (Vector2)((val5 == bands[0].Strip) ? Vector2.Zero : new Vector2(0f, 220f - edge.Size.Y));
				((Control)val6).Size = edge.Size;
			}
			FollowVisibility();
		}
	}

	internal static float? MenuPlateInset(PopupMenu popup)
	{
		if (!UiKit.Palette.IsLiquidGlass || !((GodotObject)popup).HasMeta(GlassMenuPending))
		{
			return null;
		}
		return 10f;
	}

	private static void AttachMenu(OptionButton source)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		PopupMenu popup = source.GetPopup();
		SubViewport view = new SubViewport
		{
			Name = StringName.op_Implicit("GlassMenuRender"),
			TransparentBg = true,
			Disable3D = true,
			RenderTargetUpdateMode = (UpdateMode)0
		};
		ColorRect plate = new ColorRect
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)view).AddChild((Node)(object)plate, false, (InternalMode)0);
		((Node)source).AddChild((Node)(object)view, false, (InternalMode)0);
		Timer clock = new Timer
		{
			Name = StringName.op_Implicit("MenuSpringClock"),
			WaitTime = 1.0 / 120.0,
			ProcessMode = (ProcessModeEnum)3
		};
		((Node)source).AddChild((Node)(object)clock, false, (InternalMode)0);
		ShaderMaterial material = null;
		ImageTexture snapshot = null;
		Rect2I snapshotPart = default(Rect2I);
		Vector2I windowPixels = default(Vector2I);
		StyleBoxTexture panel = null;
		GlassSpring reveal = new GlassSpring(0f);
		ulong lastTick = 0uL;
		bool active = false;
		bool readbackFailed = false;
		string[] fontColors = new string[5] { "font_color", "font_hover_color", "font_disabled_color", "font_separator_color", "font_accelerator_color" };
		Color[] originalColors = (Color[])(object)new Color[fontColors.Length];
		((Window)popup).AboutToPopup += delegate
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			Clear();
			if (UiKit.Palette.IsLiquidGlass && !readbackFailed)
			{
				((GodotObject)popup).SetMeta(GlassMenuPending, Variant.op_Implicit(true));
			}
		};
		((Window)popup).VisibilityChanged += delegate
		{
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Expected O, but got Unknown
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Expected O, but got Unknown
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Expected O, but got Unknown
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Unknown result type (might be due to invalid IL or missing references)
			if (!((Window)popup).Visible)
			{
				Clear();
			}
			else if (((GodotObject)popup).HasMeta(GlassMenuPending) && UiKit.Palette.IsLiquidGlass)
			{
				try
				{
					if (!Capture())
					{
						Clear();
						return;
					}
				}
				catch (Exception ex)
				{
					readbackFailed = true;
					Clear();
					GD.PushWarning("[ModLaunchManager] glass menu capture unavailable; using standard menu: " + ex.Message);
					return;
				}
				if (material == null)
				{
					ShaderMaterial val = new ShaderMaterial();
					object obj = _menuShader;
					if (obj == null)
					{
						Shader val2 = new Shader
						{
							Code = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap, repeat_disable;\r\nuniform vec2 plate_size = vec2(300.0, 40.0);\r\nuniform float radius = 20.0;\r\nuniform float dark_mode = 0.0;\r\nuniform float primary = 0.0;\r\nuniform float surface = 0.0;\r\nuniform float bleed = 16.0;\r\nuniform float reveal = 1.0;\r\nuniform vec2 reveal_origin = vec2(0.0);\r\nuniform vec2 reveal_size = vec2(80.0, 30.0);\r\nuniform vec4 tint : source_color = vec4(0.98, 0.98, 1.0, 1.0);\r\nuniform float hover_amount = 0.0;\r\nuniform float press_amount = 0.0;\r\nuniform vec2 lens_offset = vec2(0.0);\r\nuniform vec2 pointer = vec2(100.0, 20.0);\r\n// Floor for light glass, ceiling for dark and colored glass; see ReadingLimit.\r\nuniform float reading_limit = 0.65;\r\n// The outer band, in plate points, that never carries text and keeps its full optics.\r\nuniform float clear_rim = 3.5;\r\n// 1 where the text on this glass is added to it rather than painted over it; see AddedInk.\r\nuniform float added_ink = 0.0;\r\n// Further blurred levels across the face, for a surface that carries text throughout.\r\nuniform float face_blur = 0.0;\r\n\r\n// Rounded corners turn continuous (a superellipse, as in Liquid Glass Studio) wherever the\r\n// radius is shorter than the half extent; capsule ends stay circular.\r\nfloat shape(vec2 p, vec2 half_size, float r) {\r\n    vec2 q = abs(p) - half_size + r;\r\n    vec2 c = max(q, vec2(0.0));\r\n    float n = r < min(half_size.x, half_size.y) - 0.5 ? 2.6 : 2.0;\r\n    float corner = pow(pow(c.x, n) + pow(c.y, n), 1.0 / n);\r\n    return corner + min(max(q.x, q.y), 0.0) - r;\r\n}\r\n\r\n// Canvas colors are sRGB with the launcher's non-HDR 2D viewport. Apply the text\r\n// budget in linear light, after refraction, press highlights and rim illumination.\r\n// A deeply blurred level, magnified by the hardware, shows its texel grid as soft facets.\r\n// Where the face is frosted further, four taps half a texel apart on a rotated grid fold\r\n// those facets into one smooth wash.\r\nvec3 frosted(vec2 uv, float lod, vec2 point_uv) {\r\n    if (face_blur <= 0.0) return textureLod(screen_texture, uv, lod).rgb;\r\n    vec2 reach = point_uv * exp2(lod) * 0.5;\r\n    vec3 sum = textureLod(screen_texture, clamp(uv + reach * vec2(0.5, 1.0), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(-1.0, 0.5), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(-0.5, -1.0), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    sum += textureLod(screen_texture, clamp(uv + reach * vec2(1.0, -0.5), vec2(0.001), vec2(0.999)), lod).rgb;\r\n    return sum * 0.25;\r\n}\r\n\r\nvec3 readable_body(vec3 srgb) {\r\n    srgb = clamp(srgb, vec3(0.0), vec3(1.0));\r\n    vec3 linear = mix(srgb / 12.92, pow((srgb + 0.055) / 1.055, vec3(2.4)),\r\n                      step(vec3(0.04045), srgb));\r\n    float luminance = dot(linear, vec3(0.2126, 0.7152, 0.0722));\r\n    if (dark_mode < 0.5 && primary < 0.5) {\r\n        linear = mix(linear, vec3(1.0), max(0.0, (reading_limit - luminance) / max(1.0 - luminance, 0.001)));\r\n    } else if (added_ink > 0.5) {\r\n        // Deep shadows pass as they are; above them the backdrop is compressed toward the\r\n        // ceiling without ever reaching it, so bright artwork keeps its shading instead of\r\n        // being cut to one flat tone. The shoulder meets the toe at the same slope, so\r\n        // there is no seam between glass that transmits and glass that holds back.\r\n        float toe = reading_limit * 0.1;\r\n        float shoulder = reading_limit - toe;\r\n        float rolled = luminance <= toe ? luminance\r\n            : toe + shoulder * (luminance - toe) / (luminance - toe + shoulder);\r\n        linear *= rolled / max(luminance, 0.0001);\r\n        // Bright glass keeps part of its colour: an added ink brightens every channel of\r\n        // greyer glass, where a saturated channel would leave it nothing to add to.\r\n        linear = mix(vec3(rolled), linear, mix(1.0, 0.6, smoothstep(toe, reading_limit, rolled)));\r\n        float top = max(max(linear.r, linear.g), linear.b);\r\n        if (top > 1.0) linear = rolled + (linear - rolled) * (1.0 - rolled) / (top - rolled);\r\n    } else {\r\n        linear *= min(1.0, reading_limit / max(luminance, 0.001));\r\n    }\r\n    return mix(linear * 12.92, 1.055 * pow(linear, vec3(1.0 / 2.4)) - 0.055,\r\n               step(vec3(0.0031308), linear));\r\n}\r\n\r\nvoid fragment() {\r\n    vec2 extent = plate_size + vec2(bleed * 2.0);\r\n    float expansion = clamp(reveal, 0.0, 1.0);\r\n    float press = clamp(press_amount, -0.05, 1.1);\r\n    vec2 stretch = vec2(1.0) + (vec2(0.015, 0.028) * press + abs(lens_offset) * 0.004) * (1.0 - surface);\r\n    vec2 center = mix(reveal_origin - plate_size * 0.5, vec2(0.0), expansion);\r\n    vec2 p = (UV * extent - extent * 0.5 - lens_offset - center) / stretch;\r\n    vec2 half_size = max(mix(reveal_size, plate_size, expansion) * 0.5, vec2(1.0));\r\n    float r = min(radius, min(half_size.x, half_size.y));\r\n    // A continuous corner reaches further along each side than a circular one of the same\r\n    // radius; this keeps its visible rounding close to the native panel's.\r\n    if (r < min(half_size.x, half_size.y) - 0.5) r = min(r * 1.25, min(half_size.x, half_size.y));\r\n    float d = shape(p, half_size, r);\r\n    float aa = max(fwidth(d), 0.55);\r\n    float mask = 1.0 - smoothstep(-aa * 0.5, aa * 0.5, d);\r\n    vec2 grad = vec2(shape(p + vec2(0.25, 0.0), half_size, r) - shape(p - vec2(0.25, 0.0), half_size, r),\r\n                     shape(p + vec2(0.0, 0.25), half_size, r) - shape(p - vec2(0.0, 0.25), half_size, r));\r\n    vec2 outward = grad / max(length(grad), 0.0001);\r\n    // The bezel rises over its band along a convex squircle, h(x) = (1 - (1 - x)^4)^(1/4)\r\n    // (kube.io): steep at the rim, flat where the face begins, with no crease between.\r\n    float bevel = min(mix(9.0, 22.0, surface) + press * 0.7, r * 0.8);\r\n    float u = 1.0 - clamp(-min(d, 0.0) / max(bevel, 0.5), 0.0, 1.0);\r\n    float u4 = u * u * u * u;\r\n    float slope = min(u * u * u / pow(max(1.0 - u4, 0.0001), 0.75), 6.0);\r\n    // Rises a little taller than it is wide, so the rim visibly gathers the backdrop.\r\n    vec3 normal = normalize(vec3(outward * slope * 1.5, 1.0));\r\n    // Snell's law through crown glass, then across the bezel's height to the backdrop.\r\n    vec3 ray = refract(vec3(0.0, 0.0, -1.0), normal, 1.0 / 1.5);\r\n    float height = bevel * pow(max(1.0 - u4, 0.0), 0.25);\r\n    vec2 displacement = ray.xy * (height + bevel * 0.5) / max(-ray.z, 0.25);\r\n    displacement = clamp(displacement, vec2(-24.0), vec2(24.0));\r\n    vec2 pixel_uv = fwidth(SCREEN_UV) / max(fwidth(UV) * extent, vec2(0.00001));\r\n    // Clear through the bezel, where the bent backdrop is the material's signature, and\r\n    // frosted over the face, where it only has to carry color and movement.\r\n    float lod = mix(mix(1.6, 2.6, surface) + face_blur * (1.0 - u), 0.35, u * u);\r\n    vec2 uv = clamp(SCREEN_UV + displacement * pixel_uv, vec2(0.001), vec2(0.999));\r\n    vec3 bg = frosted(uv, lod, pixel_uv);\r\n    // About one pixel of dispersion at the strongest edge.\r\n    vec2 split = displacement * pixel_uv * 0.035;\r\n    bg.r = frosted(clamp(uv + split, vec2(0.001), vec2(0.999)), lod, pixel_uv).r;\r\n    bg.b = frosted(clamp(uv - split, vec2(0.001), vec2(0.999)), lod, pixel_uv).b;\r\n    float luma = dot(bg, vec3(0.2126, 0.7152, 0.0722));\r\n    // Vibrancy (Backdrop): transmitted color is lifted rather than washed out.\r\n    bg = mix(vec3(luma), bg, 1.4);\r\n    // A thin frost: readable_body below, not the tint, is what holds text contrast,\r\n    // so the fill only has to separate the glass from what it transmits.\r\n    float density = mix(0.20 + (1.0 - luma) * 0.14, 0.14 + luma * 0.30, dark_mode);\r\n    density = mix(density, mix(0.28, 0.24, dark_mode), surface);\r\n    vec3 body = mix(bg, tint.rgb, density);\r\n    // Colored glass maps transmitted luminance into the accent's tonal range, staying\r\n    // near the accent itself over dark content rather than sinking toward navy.\r\n    vec3 colored = tint.rgb * (0.86 + 0.20 * luma) + bg * 0.035;\r\n    body = mix(body, colored, primary);\r\n    vec2 to_pointer = p + half_size - pointer;\r\n    float touch = exp(-dot(to_pointer, to_pointer) / max(plate_size.y * plate_size.y * 1.6, 1.0));\r\n    body = mix(body, vec3(1.0), touch * (hover_amount * 0.018 + press * 0.07) * (1.0 - surface));\r\n    vec2 pointer_direction = clamp((pointer - half_size) / max(plate_size, vec2(1.0)), vec2(-0.5), vec2(0.5));\r\n    vec2 light_dir = normalize(vec2(-0.6, -0.8) + pointer_direction * hover_amount * 0.35);\r\n    float key = pow(max(dot(outward, light_dir), 0.0), 3.0);\r\n    float bounce = pow(max(dot(outward, -light_dir), 0.0), 4.0);\r\n    float fresnel = pow(1.0 - normal.z, 3.0);\r\n    float rim = exp(-pow((d + 0.65) / 0.85, 2.0));\r\n    // A hairline specular on the lit and opposite edges, and the light the bezel itself\r\n    // catches, strongest where it faces the key light and fading toward the face.\r\n    float glint = u * u * (0.03 + key * 0.07 + bounce * 0.03);\r\n    float shine = (rim * (0.035 + key * 0.30 + bounce * 0.13) + fresnel * (0.02 + key * 0.08) + glint)\r\n                  * mix(1.0, 0.7, surface);\r\n    body = mix(body, vec3(1.0), shine);\r\n    // A soft sky reflection across the upper face.\r\n    float from_top = (p.y + half_size.y) / max(min(half_size.y * 1.1, 48.0), 1.0);\r\n    body = mix(body, vec3(1.0), (1.0 - smoothstep(0.0, 1.0, from_top)) * mix(0.06, 0.035, dark_mode));\r\n    body *= 1.0 - exp(-pow((d + 1.8) / 1.35, 2.0)) * (1.0 - key) * 0.07;\r\n    // The outer band contains no text; retain its optical reflection, easing into the\r\n    // budget across the whole band. The entire content area, including the pointer\r\n    // highlight, stays within the contrast budget.\r\n    body = mix(body, readable_body(body), smoothstep(1.5, max(clear_rim, 3.5), -d));\r\n    float sd = max(shape(p - vec2(0.0, mix(2.0, 5.0, surface)), half_size, r), 0.0);\r\n    float shadow = exp(-sd * sd / mix(24.0, 60.0, surface)) * mix(0.11, 0.25, dark_mode);\r\n    float alpha = mask + shadow * (1.0 - mask);\r\n    COLOR = vec4(clamp(body, vec3(0.0), vec3(1.0)) * mask / max(alpha, 0.001), alpha);\r\n}".Replace("hint_screen_texture, ", "").Replace("uniform vec2 plate_size", "uniform vec2 capture_origin;\nuniform vec2 capture_scale;\nuniform vec2 plate_size").Replace("SCREEN_UV", "(capture_origin + UV * capture_scale)")
						};
						_menuShader = val2;
						obj = (object)val2;
					}
					val.Shader = (Shader)obj;
					material = val;
				}
				material.SetShaderParameter(StringName.op_Implicit("screen_texture"), Variant.op_Implicit((GodotObject)(object)snapshot));
				material.SetShaderParameter(StringName.op_Implicit("dark_mode"), Variant.op_Implicit(UiKit.Palette.IsDark ? 1f : 0f));
				material.SetShaderParameter(StringName.op_Implicit("tint"), Variant.op_Implicit(UiKit.Palette.SurfaceRaised));
				material.SetShaderParameter(StringName.op_Implicit("surface"), Variant.op_Implicit(1f));
				material.SetShaderParameter(StringName.op_Implicit("radius"), Variant.op_Implicit(16f));
				material.SetShaderParameter(StringName.op_Implicit("bleed"), Variant.op_Implicit(10f));
				material.SetShaderParameter(StringName.op_Implicit("reading_limit"), Variant.op_Implicit(ReadingLimit(GlassText.All)));
				((CanvasItem)plate).Material = (Material)(object)material;
				panel = new StyleBoxTexture
				{
					Texture = (Texture2D)(object)((Viewport)view).GetTexture()
				};
				StyleBox themeStylebox = ((Window)popup).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null);
				Side[] array = new Side[4];
				RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
				Side[] array2 = (Side[])(object)array;
				foreach (Side val3 in array2)
				{
					((StyleBox)panel).SetContentMargin(val3, themeStylebox.GetContentMargin(val3) + 10f);
				}
				for (int j = 0; j < fontColors.Length; j++)
				{
					originalColors[j] = ((Window)popup).GetThemeColor(StringName.op_Implicit(fontColors[j]), (StringName)null);
				}
				active = true;
				reveal.Reset(0f);
				reveal.Target = 1f;
				Geometry();
				Write();
				view.RenderTargetUpdateMode = (UpdateMode)4;
				((Window)popup).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)panel);
				lastTick = Time.GetTicksUsec();
				clock.Start(-1.0);
			}
		};
		((Viewport)popup).SizeChanged += Geometry;
		clock.Timeout += delegate
		{
			ulong ticksUsec = Time.GetTicksUsec();
			reveal.Step(Math.Min((float)(ticksUsec - lastTick) / 1000000f, 0.1f), 5f, 0.9f);
			lastTick = ticksUsec;
			Write();
			if (reveal.Settled)
			{
				clock.Stop();
				view.RenderTargetUpdateMode = (UpdateMode)1;
			}
		};
		((Control)source).ThemeChanged += delegate
		{
			if (active)
			{
				Clear();
			}
		};
		((Node)source).TreeExiting += Clear;
		bool Capture()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			(Vector2, Vector2, Vector2I) tuple = Placement();
			Vector2 origin = tuple.Item1;
			Vector2 span = tuple.Item2;
			Vector2I size = tuple.Item3;
			Vector2I windowPixels2;
			Image val4 = ReadWindow(((Node)source).GetViewport(), Part, out windowPixels2, out snapshotPart);
			try
			{
				if (val4 == null || val4.IsEmpty())
				{
					return false;
				}
				windowPixels = windowPixels2;
				val4.GenerateMipmaps(false);
				snapshot = ImageTexture.CreateFromImage(val4);
				return true;
			}
			finally
			{
				((IDisposable)val4)?.Dispose();
			}
			Rect2I Part(Vector2I pixels)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_005e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				//IL_006d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_007b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0080: Unknown result type (might be due to invalid IL or missing references)
				//IL_0084: Unknown result type (might be due to invalid IL or missing references)
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_008e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_0096: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
				//IL_011f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0120: Unknown result type (might be due to invalid IL or missing references)
				//IL_0121: Unknown result type (might be due to invalid IL or missing references)
				//IL_0122: Unknown result type (might be due to invalid IL or missing references)
				//IL_0127: Unknown result type (might be due to invalid IL or missing references)
				//IL_012b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0130: Unknown result type (might be due to invalid IL or missing references)
				//IL_0135: Unknown result type (might be due to invalid IL or missing references)
				//IL_0110: Unknown result type (might be due to invalid IL or missing references)
				Rect2 val5 = new Rect2(origin * Vector2I.op_Implicit(pixels), span * Vector2I.op_Implicit(pixels));
				Rect2 val6 = ((Rect2)(ref val5)).Grow(24f * span.X * (float)pixels.X / (float)size.X + 32f);
				Vector2 val7 = ((Rect2)(ref val6)).Position / 32f;
				val7 = ((Vector2)(ref val7)).Floor() * 32f;
				Vector2I val8 = (Vector2I)((Vector2)(ref val7)).Max(Vector2.Zero);
				val7 = ((Rect2)(ref val6)).End / 32f;
				val7 = ((Vector2)(ref val7)).Ceil() * 32f;
				Vector2I val9 = (Vector2I)((Vector2)(ref val7)).Min(Vector2I.op_Implicit(pixels));
				if (pixels.X % 8 != 0)
				{
					ref int x = ref val8.X;
					ref int x2 = ref val9.X;
					int x3 = pixels.X;
					x = 0;
					x2 = x3;
				}
				if (pixels.Y % 8 != 0)
				{
					ref int x = ref val8.Y;
					ref int y = ref val9.Y;
					int x3 = pixels.Y;
					x = 0;
					y = x3;
				}
				Vector2I val10 = val8;
				Vector2I val11 = val9 - val8;
				return new Rect2I(val10, ((Vector2I)(ref val11)).Max(Vector2I.Zero));
			}
		}
		void Clear()
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			clock.Stop();
			active = false;
			view.RenderTargetUpdateMode = (UpdateMode)0;
			if (panel != null)
			{
				((Window)popup).RemoveThemeStyleboxOverride(StringName.op_Implicit("panel"));
				string[] array3 = fontColors;
				foreach (string text in array3)
				{
					((Window)popup).RemoveThemeColorOverride(StringName.op_Implicit(text));
				}
			}
			panel = null;
			if (material != null)
			{
				material.SetShaderParameter(StringName.op_Implicit("screen_texture"), default(Variant));
			}
			ImageTexture obj2 = snapshot;
			if (obj2 != null)
			{
				((GodotObject)obj2).Dispose();
			}
			snapshot = null;
			if (((GodotObject)popup).HasMeta(GlassMenuPending))
			{
				((GodotObject)popup).RemoveMeta(GlassMenuPending);
			}
		}
		void Geometry()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			if (active && material != null && snapshot != null)
			{
				var (val16, val17, val18) = Placement();
				view.Size = val18;
				((Control)plate).Size = Vector2I.op_Implicit(val18);
				material.SetShaderParameter(StringName.op_Implicit("capture_origin"), Variant.op_Implicit((val16 * Vector2I.op_Implicit(windowPixels) - Vector2I.op_Implicit(((Rect2I)(ref snapshotPart)).Position)) / Vector2I.op_Implicit(((Rect2I)(ref snapshotPart)).Size)));
				material.SetShaderParameter(StringName.op_Implicit("capture_scale"), Variant.op_Implicit(val17 * Vector2I.op_Implicit(windowPixels) / Vector2I.op_Implicit(((Rect2I)(ref snapshotPart)).Size)));
				material.SetShaderParameter(StringName.op_Implicit("plate_size"), Variant.op_Implicit(Vector2I.op_Implicit(val18) - Vector2.One * 2f * 10f));
				material.SetShaderParameter(StringName.op_Implicit("reveal_origin"), Variant.op_Implicit(new Vector2(((float)val18.X - 20f) * 0.5f, 0f)));
				material.SetShaderParameter(StringName.op_Implicit("reveal_size"), Variant.op_Implicit(new Vector2(Math.Min(((Control)source).Size.X, val18.X), 28f)));
			}
		}
		(Vector2 Origin, Vector2 Span, Vector2I Size) Placement()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			Vector2I val12 = default(Vector2I);
			((Vector2I)(ref val12))._002Ector(Math.Max(((Window)popup).Size.X, 1), Math.Max(((Window)popup).Size.Y, 1));
			Window window = ((Node)source).GetWindow();
			Vector2 val13 = Vector2I.op_Implicit(((Window)popup).IsEmbedded() ? ((Window)popup).Position : (((Window)popup).Position - window.Position));
			Vector2 val14;
			if (!((Window)popup).IsEmbedded())
			{
				val14 = Vector2I.op_Implicit(window.Size);
			}
			else
			{
				Rect2 visibleRect = ((Node)source).GetViewport().GetVisibleRect();
				val14 = ((Rect2)(ref visibleRect)).Size;
			}
			Vector2 val15 = val14;
			return (Origin: val13 / val15, Span: Vector2I.op_Implicit(val12) / val15, Size: val12);
		}
		void Write()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			ShaderMaterial obj3 = material;
			if (obj3 != null)
			{
				obj3.SetShaderParameter(StringName.op_Implicit("reveal"), Variant.op_Implicit(reveal.Value));
			}
			float num = Mathf.SmoothStep(0.35f, 0.9f, reveal.Value);
			for (int l = 0; l < fontColors.Length; l++)
			{
				((Window)popup).AddThemeColorOverride(StringName.op_Implicit(fontColors[l]), UiKit.Alpha(originalColors[l], originalColors[l].A * num));
			}
		}
	}

	private static Image? ReadWindow(Viewport viewport, Func<Vector2I, Rect2I> partOf, out Vector2I windowPixels, out Rect2I part)
	{
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I8
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00c4: Expected O, but got Unknown
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		Texture2D texture = (Texture2D)(object)viewport.GetTexture();
		RenderingDevice renderingDevice = RenderingServer.GetRenderingDevice();
		if (renderingDevice != null)
		{
			Rid val = RenderingServer.TextureGetRdTexture(((Resource)texture).GetRid(), false);
			if (((Rid)(ref val)).IsValid)
			{
				RDTextureFormat val2 = renderingDevice.TextureGetFormat(val);
				if (val2 != null && (long)val2.Format == 36)
				{
					windowPixels = new Vector2I((int)val2.Width, (int)val2.Height);
					part = partOf(windowPixels);
					if (((Rect2I)(ref part)).Area <= 0)
					{
						return null;
					}
					Rid val3 = renderingDevice.TextureCreate(new RDTextureFormat
					{
						Format = (DataFormat)36,
						Width = (uint)((Rect2I)(ref part)).Size.X,
						Height = (uint)((Rect2I)(ref part)).Size.Y,
						UsageBits = (TextureUsageBits)385
					}, new RDTextureView(), (Array<byte[]>)null);
					if (((Rid)(ref val3)).IsValid)
					{
						try
						{
							if ((int)renderingDevice.TextureCopy(val, val3, new Vector3((float)((Rect2I)(ref part)).Position.X, (float)((Rect2I)(ref part)).Position.Y, 0f), Vector3.Zero, new Vector3((float)((Rect2I)(ref part)).Size.X, (float)((Rect2I)(ref part)).Size.Y, 1f), 0u, 0u, 0u, 0u) == 0)
							{
								byte[] array = renderingDevice.TextureGetData(val3, 0u);
								if (array != null && array.Length >= ((Rect2I)(ref part)).Area * 4)
								{
									Image val4 = Image.CreateFromData(((Rect2I)(ref part)).Size.X, ((Rect2I)(ref part)).Size.Y, false, (Format)5, array);
									if (!viewport.TransparentBg)
									{
										val4.Convert((Format)4);
									}
									return val4;
								}
							}
						}
						finally
						{
							renderingDevice.FreeRid(val3);
						}
					}
				}
			}
		}
		Image image = texture.GetImage();
		try
		{
			windowPixels = image.GetSize();
			part = partOf(windowPixels);
			return (image.IsEmpty() || ((Rect2I)(ref part)).Area <= 0) ? null : image.GetRegion(part);
		}
		finally
		{
			((IDisposable)image)?.Dispose();
		}
	}

	static LiquidGlass()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Dictionary versionInfo = Engine.GetVersionInfo();
		int boundedCopies;
		if ((int)versionInfo[Variant.op_Implicit("major")] == 4)
		{
			int num = (int)versionInfo[Variant.op_Implicit("minor")];
			boundedCopies = ((num >= 3 && num <= 6) ? 1 : 0);
		}
		else
		{
			boundedCopies = 0;
		}
		BoundedCopies = (byte)boundedCopies != 0;
		LevelsFollowBounds = RenderingServer.GetRenderingDevice() == null;
		GlassMenuPending = StringName.op_Implicit("mlm_glass_menu");
	}
}
