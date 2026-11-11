using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace ModLaunchManager;

internal static class UiKit
{
	internal const int TreeCellPadding = 8;

	internal const int TreeColumnGap = 4;

	internal const int TreeCheckSize = 18;

	private static UiPalette _palette = UiPalette.FluentDark;

	private static float _fontScale = 1f;

	private static LauncherBackground.Stats? _backdrop;

	internal static readonly Color Clear = new Color(0f, 0f, 0f, 0f);

	private static SystemFont? _uiFont;

	private static SystemFont? _fluentUiFont;

	private static SystemFont? _fluentHeadingFont;

	private static SystemFont? _monoFont;

	private static SystemFont? _glassFont;

	private static SystemFont? _glassHeadingFont;

	private static SystemFont? _boldUiFont;

	private static readonly Dictionary<SystemFont, string> CjkFallbackLocale = new Dictionary<SystemFont, string>();

	private const float MenuGap = 4f;

	internal const int ScrollBarWidth = 14;

	private const int ScrollPillOuter = 3;

	private const int ScrollPillRest = 4;

	private const int ScrollPillActive = 7;

	private const int ScrollTrackEnd = 4;

	private const int ScrollGrabberMinimum = 28;

	internal static UiPalette Palette => _palette;

	internal static bool HasCustomBackdrop => _backdrop.HasValue;

	internal static Color Canvas => _palette.Canvas;

	internal static Color Surface => BackdropPlane(_palette.Surface, 0.56f, 0.5f);

	internal static Color SurfaceRaised => BackdropPlane(_palette.SurfaceRaised, 0.62f, 0.56f);

	internal static Color SurfaceInset => BackdropPlane(_palette.SurfaceInset, 0.58f, 0.52f);

	internal static Color Border => BackdropPlane(_palette.Border, 0.86f, 0.78f);

	internal static Color BorderStrong => BackdropPlane(_palette.BorderStrong, 0.92f, 0.86f);

	internal static Color ControlFill => _palette.SurfaceInset;

	internal static Color ControlHover => _palette.SurfaceHover;

	internal static Color Text => _palette.Text;

	internal static Color TextMuted => _palette.TextMuted;

	internal static Color TextDim => _palette.TextDim;

	internal static Color Accent => _palette.Accent;

	internal static Color AccentStrong => _palette.AccentStrong;

	internal static Color AccentSoft => BackdropPlane(_palette.AccentSoft, 0.46f, 0.34f);

	internal static Color OnAccent => _palette.OnAccent;

	internal static Color Success => _palette.Success;

	internal static Color Warning => _palette.Warning;

	internal static Color Danger => _palette.Danger;

	internal static SystemFont UiFont
	{
		get
		{
			if (!_palette.IsLiquidGlass)
			{
				if (!_palette.IsFluent)
				{
					return DefaultUiFont;
				}
				return FluentUiFont;
			}
			return GlassUiFont;
		}
	}

	internal static SystemFont BoldUiFont
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			if (_boldUiFont == null)
			{
				_boldUiFont = new SystemFont
				{
					FontWeight = 700,
					AllowSystemFallback = true
				};
			}
			_boldUiFont.FontNames = UiFont.FontNames;
			return WithCjkFallback(_boldUiFont);
		}
	}

	private static SystemFont GlassUiFont
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			SystemFont? obj = _glassFont;
			if (obj == null)
			{
				SystemFont val = new SystemFont();
				val.FontNames = new string[5] { "SF Pro Text", "Helvetica Neue", "Segoe UI Variable Text", "Segoe UI", "Microsoft YaHei UI" };
				val.AllowSystemFallback = true;
				val.FontWeight = 400;
				obj = (_glassFont = val);
			}
			return WithCjkFallback(obj);
		}
	}

	private static SystemFont GlassHeadingFont
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			SystemFont? obj = _glassHeadingFont;
			if (obj == null)
			{
				SystemFont val = new SystemFont();
				val.FontNames = new string[5] { "SF Pro Display", "Helvetica Neue", "Segoe UI Variable Display", "Segoe UI", "Microsoft YaHei UI" };
				val.AllowSystemFallback = true;
				val.FontWeight = 600;
				obj = (_glassHeadingFont = val);
			}
			return WithCjkFallback(obj);
		}
	}

	private static SystemFont DefaultUiFont
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			SystemFont? obj = _uiFont;
			if (obj == null)
			{
				SystemFont val = new SystemFont();
				val.FontNames = new string[4] { "Microsoft YaHei UI", "Noto Sans CJK SC", "Segoe UI", "Arial" };
				val.AllowSystemFallback = true;
				val.FontWeight = 500;
				obj = (_uiFont = val);
			}
			return WithCjkFallback(obj);
		}
	}

	private static SystemFont FluentUiFont
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			SystemFont? obj = _fluentUiFont;
			if (obj == null)
			{
				SystemFont val = new SystemFont();
				val.FontNames = new string[5] { "Segoe UI Variable Text", "Segoe UI", "Microsoft YaHei UI", "Noto Sans CJK SC", "Arial" };
				val.AllowSystemFallback = true;
				val.FontWeight = 400;
				obj = (_fluentUiFont = val);
			}
			return WithCjkFallback(obj);
		}
	}

	private static SystemFont FluentHeadingFont
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			SystemFont? obj = _fluentHeadingFont;
			if (obj == null)
			{
				SystemFont val = new SystemFont();
				val.FontNames = new string[4] { "Segoe UI Variable Display", "Segoe UI Semibold", "Microsoft YaHei UI", "Noto Sans CJK SC" };
				val.AllowSystemFallback = true;
				val.FontWeight = 600;
				obj = (_fluentHeadingFont = val);
			}
			return WithCjkFallback(obj);
		}
	}

	internal static SystemFont MonoFont
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			SystemFont? obj = _monoFont;
			if (obj == null)
			{
				SystemFont val = new SystemFont();
				val.FontNames = new string[4] { "Cascadia Mono", "Consolas", "Noto Sans Mono CJK SC", "Microsoft YaHei UI" };
				val.AllowSystemFallback = true;
				val.FontWeight = 400;
				obj = (_monoFont = val);
			}
			return WithCjkFallback(obj);
		}
	}

	private static Color TreeSurface
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!_palette.IsLiquidGlass && !HasCustomBackdrop)
			{
				return _palette.SurfaceInset;
			}
			return Clear;
		}
	}

	private static int BodyFontSize => ScaleFontSize((_palette.IsFluent || _palette.IsLiquidGlass) ? 14 : 15);

	private static Color WashTone
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (!_palette.IsFluent && !_palette.IsLiquidGlass)
			{
				return Accent;
			}
			return Text;
		}
	}

	private static StyleBoxFlat TreeTitleBox(Color background, Color border, int radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		StyleBoxFlat obj = Box(background, border, radius, 0, 8, 8);
		((StyleBox)obj).ContentMarginLeft = 12f;
		return obj;
	}

	internal static void UsePalette(UiPalette palette)
	{
		_palette = palette;
	}

	internal static void UseFontScale(float scale)
	{
		_fontScale = Math.Clamp(scale, 0.85f, 1.25f);
	}

	internal static int ScaleFontSize(int size)
	{
		return Math.Max(10, Mathf.RoundToInt((float)size * _fontScale));
	}

	internal static void UseCustomBackdrop(LauncherBackground.Stats? backdrop)
	{
		_backdrop = backdrop;
	}

	private static SystemFont WithCjkFallback(SystemFont font)
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		string text;
		try
		{
			text = MlmLocManager.EffectiveLocale;
		}
		catch
		{
			text = string.Empty;
		}
		if (CjkFallbackLocale.TryGetValue(font, out string value) && value == text)
		{
			return font;
		}
		string[] array = (text.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase) ? new string[4] { "Microsoft JhengHei UI", "Microsoft JhengHei", "PingFang TC", "Noto Sans CJK TC" } : (text.StartsWith("ja", StringComparison.OrdinalIgnoreCase) ? new string[4] { "Yu Gothic UI", "Meiryo UI", "Hiragino Sans", "Noto Sans CJK JP" } : ((!text.StartsWith("ko", StringComparison.OrdinalIgnoreCase)) ? new string[5] { "Microsoft YaHei UI", "Microsoft YaHei", "PingFang SC", "Noto Sans CJK SC", "Source Han Sans SC" } : new string[3] { "Malgun Gothic", "Apple SD Gothic Neo", "Noto Sans CJK KR" })));
		string[] fontNames = array;
		Array<Font> obj2 = new Array<Font>();
		obj2.Add((Font)new SystemFont
		{
			FontNames = fontNames,
			AllowSystemFallback = true,
			FontWeight = font.FontWeight
		});
		((Font)font).Fallbacks = obj2;
		CjkFallbackLocale[font] = text;
		return font;
	}

	internal static Color Alpha(Color color, float alpha)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return new Color(color.R, color.G, color.B, alpha);
	}

	private static Color BackdropPlane(Color color, float dark, float light)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (_palette.IsLiquidGlass)
		{
			return color;
		}
		LauncherBackground.Stats? backdrop = _backdrop;
		if (backdrop.HasValue)
		{
			LauncherBackground.Stats valueOrDefault = backdrop.GetValueOrDefault();
			float num = (_palette.IsDark ? dark : light);
			float num2 = num + (1f - num) * 0.55f;
			return Alpha(color, Mathf.Lerp(num, num2, valueOrDefault.LegibilityDemand(_palette)));
		}
		return color;
	}

	internal static void PinTreeTitles(Tree tree, bool pinned)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[3] { "title_button_normal", "title_button_hover", "title_button_pressed" };
		Color surface;
		float reach;
		if (!pinned)
		{
			string[] array2 = array;
			foreach (string text in array2)
			{
				((Control)tree).RemoveThemeStyleboxOverride(StringName.op_Implicit(text));
			}
		}
		else
		{
			surface = PinnedRowSurface(Clear, 0f);
			reach = Math.Max(0f, ((Control)tree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null).GetMargin((Side)1) - 1f);
			((Control)tree).AddThemeStyleboxOverride(StringName.op_Implicit("title_button_normal"), (StyleBox)(object)Title(Clear));
			((Control)tree).AddThemeStyleboxOverride(StringName.op_Implicit("title_button_hover"), (StyleBox)(object)Title(Alpha(WashTone, 0.06f)));
			((Control)tree).AddThemeStyleboxOverride(StringName.op_Implicit("title_button_pressed"), (StyleBox)(object)Title(AccentSoft));
		}
		StyleBoxFlat Title(Color over)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			StyleBoxFlat obj = TreeTitleBox(((Color)(ref surface)).Blend(over), Border, 0);
			obj.BorderWidthBottom = 1;
			obj.ExpandMarginTop = reach;
			return obj;
		}
	}

	internal static Color PinnedRowSurface(Color tint, float share)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Color val = ((TreeSurface.A >= 0.95f) ? TreeSurface : _palette.SurfaceRaised);
		Color result = default(Color);
		((Color)(ref result))._002Ector(val.R, val.G, val.B, 1f);
		if (!(share <= 0f))
		{
			return ((Color)(ref result)).Blend(RowTint(tint, share));
		}
		return result;
	}

	internal static Color RowTint(Color tint, float share)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (_palette.IsLiquidGlass)
		{
			return Alpha(tint, Math.Min(share, 0.05f));
		}
		LauncherBackground.Stats? backdrop = _backdrop;
		if (!backdrop.HasValue)
		{
			return Alpha(tint, share);
		}
		float alpha = 0.34f + backdrop.GetValueOrDefault().LegibilityDemand(_palette) * 0.14f;
		Color canvas = _palette.Canvas;
		return Alpha(((Color)(ref canvas)).Lerp(tint, Math.Clamp(share * 2.2f, 0f, 1f)), alpha);
	}

	internal static Theme CreateTheme()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_045d: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_0484: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05df: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0608: Unknown result type (might be due to invalid IL or missing references)
		//IL_062a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_0656: Unknown result type (might be due to invalid IL or missing references)
		//IL_0667: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Unknown result type (might be due to invalid IL or missing references)
		//IL_0717: Unknown result type (might be due to invalid IL or missing references)
		//IL_0721: Unknown result type (might be due to invalid IL or missing references)
		//IL_0726: Unknown result type (might be due to invalid IL or missing references)
		//IL_0735: Unknown result type (might be due to invalid IL or missing references)
		//IL_073f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0749: Unknown result type (might be due to invalid IL or missing references)
		//IL_0753: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0769: Unknown result type (might be due to invalid IL or missing references)
		//IL_0794: Unknown result type (might be due to invalid IL or missing references)
		//IL_0799: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0806: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0810: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_0825: Unknown result type (might be due to invalid IL or missing references)
		//IL_082e: Unknown result type (might be due to invalid IL or missing references)
		//IL_083a: Unknown result type (might be due to invalid IL or missing references)
		//IL_084b: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_088d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0892: Unknown result type (might be due to invalid IL or missing references)
		//IL_089c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0908: Unknown result type (might be due to invalid IL or missing references)
		//IL_0901: Unknown result type (might be due to invalid IL or missing references)
		//IL_0912: Unknown result type (might be due to invalid IL or missing references)
		//IL_091c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0921: Unknown result type (might be due to invalid IL or missing references)
		//IL_0931: Unknown result type (might be due to invalid IL or missing references)
		//IL_093f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0950: Unknown result type (might be due to invalid IL or missing references)
		//IL_0961: Unknown result type (might be due to invalid IL or missing references)
		//IL_0972: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a17: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a34: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a40: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a68: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ab8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ace: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b04: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b09: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b29: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b30: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b94: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b99: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c92: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d16: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d9a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0db9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0de2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e53: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e85: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eaa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ecf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ed9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ede: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f10: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f3c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fd3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ffb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1000: Unknown result type (might be due to invalid IL or missing references)
		//IL_1025: Unknown result type (might be due to invalid IL or missing references)
		//IL_102f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1034: Unknown result type (might be due to invalid IL or missing references)
		//IL_1059: Unknown result type (might be due to invalid IL or missing references)
		//IL_105e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1083: Unknown result type (might be due to invalid IL or missing references)
		//IL_10a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_1112: Unknown result type (might be due to invalid IL or missing references)
		//IL_111c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1234: Unknown result type (might be due to invalid IL or missing references)
		//IL_1239: Unknown result type (might be due to invalid IL or missing references)
		//IL_1266: Unknown result type (might be due to invalid IL or missing references)
		//IL_126b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1293: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1315: Unknown result type (might be due to invalid IL or missing references)
		//IL_131f: Unknown result type (might be due to invalid IL or missing references)
		//IL_132f: Unknown result type (might be due to invalid IL or missing references)
		//IL_134e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1358: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_13c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_13cf: Expected O, but got Unknown
		//IL_13e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_13e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_13fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1407: Expected O, but got Unknown
		//IL_1452: Unknown result type (might be due to invalid IL or missing references)
		//IL_145c: Expected O, but got Unknown
		//IL_148c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1496: Unknown result type (might be due to invalid IL or missing references)
		//IL_14cf: Unknown result type (might be due to invalid IL or missing references)
		int bodyFontSize = BodyFontSize;
		Theme val = new Theme
		{
			DefaultFont = (Font)(object)UiFont,
			DefaultFontSize = bodyFontSize
		};
		val.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("Label"), Text);
		val.SetColor(StringName.op_Implicit("font_shadow_color"), StringName.op_Implicit("Label"), _palette.TextShadow);
		val.SetConstant(StringName.op_Implicit("shadow_offset_x"), StringName.op_Implicit("Label"), 0);
		val.SetConstant(StringName.op_Implicit("shadow_offset_y"), StringName.op_Implicit("Label"), 1);
		AddLabelVariation(val, "TitleLabel", ScaleFontSize(_palette.IsFluent ? 28 : 26), Text);
		AddLabelVariation(val, "SectionLabel", ScaleFontSize(_palette.IsFluent ? 20 : 19), Text);
		AddLabelVariation(val, "SubtitleLabel", ScaleFontSize(14), TextMuted);
		AddLabelVariation(val, "MutedLabel", ScaleFontSize(14), TextMuted);
		AddLabelVariation(val, "DimLabel", ScaleFontSize(13), TextDim);
		AddLabelVariation(val, "CaptionLabel", ScaleFontSize(12), TextDim);
		AddLabelVariation(val, "GroupLabel", ScaleFontSize(_palette.IsLiquidGlass ? 13 : 14), Text);
		AddLabelVariation(val, "MetaLabel", ScaleFontSize(13), TextMuted);
		AddLabelVariation(val, "SuccessLabel", ScaleFontSize(14), Success);
		AddLabelVariation(val, "WarningLabel", ScaleFontSize(14), Warning);
		AddLabelVariation(val, "DangerLabel", ScaleFontSize(14), Danger);
		AddLabelVariation(val, "MetricLabel", ScaleFontSize(_palette.IsFluent ? 20 : 18), Text);
		if (_palette.IsFluent)
		{
			val.SetFont(StringName.op_Implicit("font"), StringName.op_Implicit("TitleLabel"), (Font)(object)FluentHeadingFont);
			val.SetFont(StringName.op_Implicit("font"), StringName.op_Implicit("SectionLabel"), (Font)(object)FluentHeadingFont);
			val.SetFont(StringName.op_Implicit("font"), StringName.op_Implicit("MetricLabel"), (Font)(object)FluentHeadingFont);
			val.SetFont(StringName.op_Implicit("font"), StringName.op_Implicit("GroupLabel"), (Font)(object)FluentHeadingFont);
		}
		val.SetTypeVariation(StringName.op_Implicit("CardPanel"), StringName.op_Implicit("PanelContainer"));
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("CardPanel"), (StyleBox)(object)PanelBox(SurfaceRaised, Border, 8, 1, 18, 16, elevated: true));
		val.SetTypeVariation(StringName.op_Implicit("InsetPanel"), StringName.op_Implicit("PanelContainer"));
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("InsetPanel"), (StyleBox)(object)PanelBox(SurfaceInset, Border, 6, 1, 14, 12));
		val.SetTypeVariation(StringName.op_Implicit("BarPanel"), StringName.op_Implicit("PanelContainer"));
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("BarPanel"), (StyleBox)(object)PanelBox(Surface, Border, 6, 1, 12, 8));
		val.SetTypeVariation(StringName.op_Implicit("StatusPanel"), StringName.op_Implicit("PanelContainer"));
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("StatusPanel"), (StyleBox)(object)PanelBox(Surface, Border, 6, 1, 13, 8));
		val.SetTypeVariation(StringName.op_Implicit("DialogPanel"), StringName.op_Implicit("PanelContainer"));
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("DialogPanel"), (StyleBox)(object)DialogBox());
		int num = (_palette.IsFluent ? 4 : 5);
		int radius = (_palette.IsFluent ? 4 : 999);
		Color val2 = (_palette.IsFluent ? BorderStrong : Border);
		Color accentStrong = AccentStrong;
		Color primaryHover = _palette.PrimaryHover;
		Color primaryPressed = _palette.PrimaryPressed;
		Color onAccent = OnAccent;
		Color? border = (_palette.IsFluent ? AccentStrong : Alpha(Accent, 0.45f));
		Color? hoverBorder = (_palette.IsFluent ? new Color?(_palette.PrimaryHover) : ((Color?)null));
		Color? pressedBorder = (_palette.IsFluent ? new Color?(_palette.PrimaryPressed) : ((Color?)null));
		Color? hoverText = _palette.PrimaryHoverText;
		AddButtonVariation(val, "PrimaryButton", accentStrong, primaryHover, primaryPressed, onAccent, num, 14, 9, bodyFontSize, border, hoverBorder, pressedBorder, null, hoverText, null, null);
		AddButtonVariation(val, "SecondaryButton", _palette.SecondaryFill, ControlHover, _palette.SecondaryPressed, Text, num, 14, 9, bodyFontSize, val2, null, null, null, null, null, null);
		AddButtonVariation(val, "ToolbarButton", ControlFill, ControlHover, _palette.ToolbarPressed, TextMuted, num, 14, 9, bodyFontSize, val2, null, null, null, null, null, null);
		Color dangerFill = _palette.DangerFill;
		Color dangerHover = _palette.DangerHover;
		Color dangerPressed = _palette.DangerPressed;
		Color dangerText = _palette.DangerText;
		Color? border2 = Alpha(Danger, 0.4f);
		Color? hoverBorder2 = (_palette.IsFluent ? new Color?(Alpha(Danger, 0.7f)) : ((Color?)null));
		Color? pressedBorder2 = (_palette.IsFluent ? new Color?(Danger) : ((Color?)null));
		hoverText = _palette.DangerHoverText;
		AddButtonVariation(val, "DangerButton", dangerFill, dangerHover, dangerPressed, dangerText, num, 14, 9, bodyFontSize, border2, hoverBorder2, pressedBorder2, null, hoverText, null, null);
		AddButtonVariation(val, "IconButton", ControlFill, ControlHover, _palette.ToolbarPressed, TextMuted, num, 6, 5, ScaleFontSize(15), val2, null, null, null, null, null, null);
		AddButtonVariation(val, "AccentIconButton", _palette.SecondaryFill, ControlHover, _palette.SecondaryPressed, Accent, num, 6, 5, ScaleFontSize(15), Alpha(Accent, 0.38f), Accent, Accent, hoverText: Accent, pressedText: Accent, hoverPressed: null, hoverPressedBorder: null);
		AddButtonVariation(val, "MiniButton", ControlFill, ControlHover, _palette.ToolbarPressed, TextMuted, num, 11, 5, ScaleFontSize(13), val2, null, null, null, null, null, null);
		Color controlFill = ControlFill;
		Color controlHover2 = ControlHover;
		Color accentStrong2 = AccentStrong;
		Color textMuted = TextMuted;
		int fontSize2 = ScaleFontSize(13);
		Color? border4 = val2;
		hoverText = AccentStrong;
		Color? pressedText = OnAccent;
		Color? hoverPressed = _palette.PrimaryHover;
		Color? hoverPressedBorder = _palette.PrimaryHover;
		AddButtonVariation(val, "SwitchButton", controlFill, controlHover2, accentStrong2, textMuted, num, 11, 5, fontSize2, border4, null, hoverText, pressedText, null, hoverPressed, hoverPressedBorder);
		AddButtonVariation(val, "FieldDropdown", ControlFill, ControlHover, _palette.ToolbarPressed, Text, num, 12, 9, bodyFontSize, val2, null, null, null, null, null, null);
		Color normal = (HasCustomBackdrop ? ControlFill : Clear);
		Color chipHover = _palette.ChipHover;
		Color chipPressed = _palette.ChipPressed;
		Color textMuted2 = TextMuted;
		int fontSize3 = ScaleFontSize(13);
		Color? border5 = val2;
		hoverPressedBorder = _palette.ChipPressedBorder;
		hoverPressed = _palette.ChipPressedText;
		pressedText = _palette.ChipPressedHover;
		hoverText = _palette.ChipPressedBorder;
		AddButtonVariation(val, "ChipButton", normal, chipHover, chipPressed, textMuted2, radius, 13, 6, fontSize3, border5, null, hoverPressedBorder, hoverPressed, null, pressedText, hoverText);
		int num2;
		UiPalette palette;
		if (!HasCustomBackdrop)
		{
			palette = _palette;
			num2 = ((palette != null && palette.IsLiquidGlass && palette.IsDark) ? 1 : 0);
		}
		else
		{
			num2 = 1;
		}
		bool flag = (byte)num2 != 0;
		Color normal2 = (flag ? ControlFill : Alpha(Text, _palette.IsDark ? 0.075f : 0.055f));
		palette = _palette;
		AddButtonVariation(val, "TagButton", normal2, (palette != null && palette.IsLiquidGlass && palette.IsDark) ? ControlHover : Alpha(Text, _palette.IsDark ? 0.13f : 0.1f), AccentSoft, TextMuted, _palette.IsFluent ? 4 : 999, 9, 3, ScaleFontSize(12), Clear, Clear, Clear, null, null, null, null);
		Color accentSoft = AccentSoft;
		Color value = BackdropPlane(_palette.TreeHoverSelected, 0.6f, 0.48f);
		Color value2 = (_palette.IsFluent ? Accent : Clear);
		Color clear = Clear;
		Color hover = Alpha(WashTone, 0.06f);
		Color textMuted3 = TextMuted;
		Color? border6 = Clear;
		Color? hoverBorder4 = Clear;
		Color? pressedBorder4 = value2;
		Color? pressedText2 = Text;
		hoverText = value;
		pressedText = value2;
		AddButtonVariation(val, "RowButton", clear, hover, accentSoft, textMuted3, num, 12, 8, bodyFontSize, border6, hoverBorder4, pressedBorder4, pressedText2, null, hoverText, pressedText);
		LeadingRule(val, "RowButton", "pressed");
		LeadingRule(val, "RowButton", "hover_pressed");
		val.SetStylebox(StringName.op_Implicit("normal"), StringName.op_Implicit("LineEdit"), (StyleBox)(object)FieldBox(ControlFill, val2, focused: false));
		val.SetStylebox(StringName.op_Implicit("focus"), StringName.op_Implicit("LineEdit"), (StyleBox)(object)FieldBox(ControlFill, _palette.FieldFocusBorder, focused: true));
		val.SetStylebox(StringName.op_Implicit("read_only"), StringName.op_Implicit("LineEdit"), (StyleBox)(object)FieldBox(_palette.Surface, val2, focused: false));
		val.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("LineEdit"), Text);
		val.SetColor(StringName.op_Implicit("font_uneditable_color"), StringName.op_Implicit("LineEdit"), TextDim);
		val.SetColor(StringName.op_Implicit("font_placeholder_color"), StringName.op_Implicit("LineEdit"), TextDim);
		val.SetColor(StringName.op_Implicit("caret_color"), StringName.op_Implicit("LineEdit"), Text);
		val.SetColor(StringName.op_Implicit("selection_color"), StringName.op_Implicit("LineEdit"), Alpha(Accent, 0.22f));
		val.SetStylebox(StringName.op_Implicit("normal"), StringName.op_Implicit("TextEdit"), (StyleBox)(object)FieldBox(ControlFill, val2, focused: false));
		val.SetStylebox(StringName.op_Implicit("focus"), StringName.op_Implicit("TextEdit"), (StyleBox)(object)FieldBox(ControlFill, _palette.FieldFocusBorder, focused: true));
		val.SetStylebox(StringName.op_Implicit("read_only"), StringName.op_Implicit("TextEdit"), (StyleBox)(object)FieldBox(_palette.Surface, val2, focused: false));
		val.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("TextEdit"), Text);
		val.SetColor(StringName.op_Implicit("font_readonly_color"), StringName.op_Implicit("TextEdit"), TextDim);
		val.SetColor(StringName.op_Implicit("font_placeholder_color"), StringName.op_Implicit("TextEdit"), TextDim);
		val.SetColor(StringName.op_Implicit("caret_color"), StringName.op_Implicit("TextEdit"), Text);
		val.SetColor(StringName.op_Implicit("selection_color"), StringName.op_Implicit("TextEdit"), Alpha(Accent, 0.22f));
		val.SetColor(StringName.op_Implicit("current_line_color"), StringName.op_Implicit("TextEdit"), Alpha(WashTone, 0.05f));
		val.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("TextEdit"), bodyFontSize);
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(TreeSurface, Border, 6, 1, 5, 5));
		val.SetStylebox(StringName.op_Implicit("focus"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(Clear, _palette.TreeFocusBorder, 5, 1, 0, 0));
		val.SetStylebox(StringName.op_Implicit("selected"), StringName.op_Implicit("Tree"), (StyleBox)(object)TreeSelectionBox(AccentSoft, focused: false));
		val.SetStylebox(StringName.op_Implicit("selected_focus"), StringName.op_Implicit("Tree"), (StyleBox)(object)TreeSelectionBox(AccentSoft, focused: true));
		val.SetStylebox(StringName.op_Implicit("hovered"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(Alpha(WashTone, 0.05f), Clear, 4, 0, 4, 2));
		val.SetStylebox(StringName.op_Implicit("hovered_dimmed"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(Alpha(WashTone, 0.03f), Clear, 4, 0, 4, 2));
		Color background = BackdropPlane(_palette.TreeHoverSelected, 0.6f, 0.48f);
		val.SetStylebox(StringName.op_Implicit("hovered_selected"), StringName.op_Implicit("Tree"), (StyleBox)(object)TreeSelectionBox(background, focused: false));
		val.SetStylebox(StringName.op_Implicit("hovered_selected_focus"), StringName.op_Implicit("Tree"), (StyleBox)(object)TreeSelectionBox(background, focused: true));
		val.SetStylebox(StringName.op_Implicit("cursor"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(Clear, _palette.TreeCursor, 4, 1, 0, 0));
		val.SetStylebox(StringName.op_Implicit("cursor_unfocused"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(Clear, Border, 4, 1, 0, 0));
		val.SetStylebox(StringName.op_Implicit("title_button_normal"), StringName.op_Implicit("Tree"), (StyleBox)(object)TreeTitleBox(Clear, Clear, 0));
		val.SetStylebox(StringName.op_Implicit("title_button_hover"), StringName.op_Implicit("Tree"), (StyleBox)(object)TreeTitleBox(Alpha(WashTone, 0.06f), Clear, 4));
		val.SetStylebox(StringName.op_Implicit("title_button_pressed"), StringName.op_Implicit("Tree"), (StyleBox)(object)TreeTitleBox(AccentSoft, Clear, 4));
		val.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("Tree"), TextMuted);
		val.SetColor(StringName.op_Implicit("font_selected_color"), StringName.op_Implicit("Tree"), Text);
		val.SetColor(StringName.op_Implicit("title_button_color"), StringName.op_Implicit("Tree"), TextDim);
		val.SetColor(StringName.op_Implicit("drop_position_color"), StringName.op_Implicit("Tree"), _palette.IsFluent ? Accent : Text);
		val.SetColor(StringName.op_Implicit("guide_color"), StringName.op_Implicit("Tree"), Alpha(Border, 0.5f));
		val.SetConstant(StringName.op_Implicit("item_margin"), StringName.op_Implicit("Tree"), 8);
		val.SetConstant(StringName.op_Implicit("h_separation"), StringName.op_Implicit("Tree"), 4);
		val.SetConstant(StringName.op_Implicit("inner_item_margin_left"), StringName.op_Implicit("Tree"), 8);
		val.SetConstant(StringName.op_Implicit("inner_item_margin_right"), StringName.op_Implicit("Tree"), 8);
		val.SetConstant(StringName.op_Implicit("button_margin"), StringName.op_Implicit("Tree"), 6);
		val.SetConstant(StringName.op_Implicit("v_separation"), StringName.op_Implicit("Tree"), 5);
		val.SetConstant(StringName.op_Implicit("scroll_border"), StringName.op_Implicit("Tree"), 6);
		val.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("Tree"), bodyFontSize);
		val.SetFontSize(StringName.op_Implicit("title_button_font_size"), StringName.op_Implicit("Tree"), ScaleFontSize(12));
		val.SetStylebox(StringName.op_Implicit("background"), StringName.op_Implicit("ProgressBar"), (StyleBox)(object)Box(ControlFill, Border, 4, 1, 0, 0));
		val.SetStylebox(StringName.op_Implicit("fill"), StringName.op_Implicit("ProgressBar"), (StyleBox)(object)Box(_palette.ProgressFill, Clear, 4, 0, 0, 0));
		val.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("ProgressBar"), Text);
		val.SetColor(StringName.op_Implicit("font_outline_color"), StringName.op_Implicit("ProgressBar"), _palette.ProgressOutline);
		val.SetConstant(StringName.op_Implicit("outline_size"), StringName.op_Implicit("ProgressBar"), 3);
		val.SetColor(StringName.op_Implicit("default_color"), StringName.op_Implicit("RichTextLabel"), TextMuted);
		val.SetColor(StringName.op_Implicit("font_shadow_color"), StringName.op_Implicit("RichTextLabel"), Alpha(_palette.TextShadow, _palette.TextShadow.A * 0.85f));
		val.SetColor(StringName.op_Implicit("selection_color"), StringName.op_Implicit("RichTextLabel"), Alpha(Accent, 0.2f));
		val.SetFontSize(StringName.op_Implicit("normal_font_size"), StringName.op_Implicit("RichTextLabel"), ScaleFontSize(14));
		val.SetConstant(StringName.op_Implicit("line_separation"), StringName.op_Implicit("RichTextLabel"), 3);
		val.SetStylebox(StringName.op_Implicit("separator"), StringName.op_Implicit("HSeparator"), (StyleBox)new StyleBoxLine
		{
			Color = Border,
			Thickness = 1
		});
		val.SetStylebox(StringName.op_Implicit("separator"), StringName.op_Implicit("VSeparator"), (StyleBox)new StyleBoxLine
		{
			Color = Border,
			Thickness = 1,
			Vertical = true
		});
		val.SetConstant(StringName.op_Implicit("separation"), StringName.op_Implicit("HSeparator"), 6);
		val.SetConstant(StringName.op_Implicit("separation"), StringName.op_Implicit("VSeparator"), 6);
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("ScrollContainer"), (StyleBox)new StyleBoxEmpty());
		AddScrollBarStyles(val, "VScrollBar");
		AddScrollBarStyles(val, "HScrollBar");
		val.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("TooltipPanel"), (StyleBox)(object)PanelBox(_palette.TooltipFill, _palette.BorderStrong, _palette.IsFluent ? 6 : 5, 1, 11, 7, elevated: true));
		val.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("TooltipLabel"), Text);
		val.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("TooltipLabel"), ScaleFontSize(13));
		AddPopupMenuStyles(val);
		AddFluentGlyphs(val);
		AddLiquidGlassStyles(val);
		return val;
	}

	internal static PanelContainer Card(string variation = "CardPanel")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		PanelContainer val = new PanelContainer
		{
			ThemeTypeVariation = StringName.op_Implicit(variation),
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		if (variation == "GlassInspectorPanel")
		{
			LiquidGlass.Decorate((Control)(object)val, primary: false, surface: true, null);
		}
		return val;
	}

	internal static Label Label(string text, string variation = "")
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		return new Label
		{
			Text = text,
			ThemeTypeVariation = StringName.op_Implicit(variation)
		};
	}

	internal static Button Button(string text, string variation = "SecondaryButton", float minimumWidth = 0f, float height = 40f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		Button val = new Button
		{
			Text = text,
			ThemeTypeVariation = StringName.op_Implicit(variation),
			CustomMinimumSize = new Vector2(minimumWidth, height),
			FocusMode = (FocusModeEnum)2
		};
		if (variation == "PrimaryButton")
		{
			LiquidGlass.Decorate((Control)(object)val, primary: true, surface: false, null);
		}
		else if (variation == "GlassIconButton")
		{
			LiquidGlass.Decorate((Control)(object)val, primary: false, surface: false, null);
		}
		return val;
	}

	internal static OptionButton Dropdown(string variation = "FieldDropdown", float minimumWidth = 0f, float height = 0f, bool fitToLongestItem = false)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		OptionButton dropdown = new OptionButton
		{
			ThemeTypeVariation = StringName.op_Implicit(variation),
			CustomMinimumSize = new Vector2(minimumWidth, height),
			FocusMode = (FocusModeEnum)2,
			FitToLongestItem = fitToLongestItem
		};
		PopupMenu menu = dropdown.GetPopup();
		((Window)menu).AboutToPopup += delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Window)menu).MaxSize = Vector2I.Zero;
		};
		((Window)menu).VisibilityChanged += delegate
		{
			if (((Window)menu).Visible)
			{
				PlaceMenu((Control)(object)dropdown, menu, dropdown.Selected);
			}
		};
		if (variation == "GlassMenuButton")
		{
			LiquidGlass.Decorate((Control)(object)dropdown, primary: false, surface: false, null);
		}
		return dropdown;
	}

	internal static void PopupUnder(Control anchor, PopupMenu menu)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		((Window)menu).MaxSize = Vector2I.Zero;
		((Window)menu).ResetSize();
		Transform2D screenTransform = ((CanvasItem)anchor).GetScreenTransform();
		((Window)menu).Position = new Vector2I(Mathf.RoundToInt(screenTransform.Origin.X), Mathf.RoundToInt(screenTransform.Origin.Y + anchor.Size.Y * ((Transform2D)(ref screenTransform)).Scale.Y));
		Callable val = Callable.From((Action)delegate
		{
			if (((Window)menu).Visible)
			{
				PlaceMenu(anchor, menu, -1);
			}
		});
		((GodotObject)menu).Connect(SignalName.VisibilityChanged, val, 0u);
		((Window)menu).Popup((Rect2I?)null);
		((GodotObject)menu).Disconnect(SignalName.VisibilityChanged, val);
		if (((Window)menu).Visible)
		{
			((GodotObject)menu).Connect(SignalName.PopupHide, Callable.From((Action)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				((Window)menu).MaxSize = Vector2I.Zero;
			}), 4u);
		}
	}

	private static void PlaceMenu(Control anchor, PopupMenu menu, int selected)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		if (!((Node)anchor).IsInsideTree())
		{
			return;
		}
		Rect2 val = (((Window)menu).IsEmbedded() ? ((Node)anchor).GetViewport().GetVisibleRect() : Rect2I.op_Implicit(DisplayServer.ScreenGetUsableRect(DisplayServer.WindowGetCurrentScreen(((Window)menu).GetWindowId()))));
		Transform2D screenTransform = ((CanvasItem)anchor).GetScreenTransform();
		float y = screenTransform.Origin.Y;
		float num = y + anchor.Size.Y * ((Transform2D)(ref screenTransform)).Scale.Y;
		float contentScaleFactor = ((Window)menu).ContentScaleFactor;
		float num2 = 4f * contentScaleFactor;
		float? num3 = LiquidGlass.MenuPlateInset(menu);
		float num6;
		float num7;
		if (num3.HasValue)
		{
			float valueOrDefault = num3.GetValueOrDefault();
			float num4 = valueOrDefault;
			float num5 = valueOrDefault;
			num6 = num5;
			num7 = num4;
		}
		else
		{
			PanelContainer val2 = ((IEnumerable)((Node)menu).GetChildren(true)).OfType<PanelContainer>().FirstOrDefault();
			if (val2 == null)
			{
				num7 = 0f;
				num6 = 0f;
			}
			else
			{
				float num8 = ((Control)val2).OffsetTop * contentScaleFactor;
				float num4 = (0f - ((Control)val2).OffsetBottom) * contentScaleFactor;
				num6 = num4;
				num7 = num8;
			}
		}
		float num9 = ((Window)menu).Size.Y;
		float num10 = ((Rect2)(ref val)).End.Y - (num + num2 - num7);
		float num11 = y - num2 + num6 - ((Rect2)(ref val)).Position.Y;
		bool flag = num9 > num10 && num11 > num10;
		float num12 = Mathf.Floor(flag ? num11 : num10);
		if (num12 < 96f * contentScaleFactor)
		{
			return;
		}
		if (num9 > num12)
		{
			((Window)menu).MinSize = new Vector2I(((Window)menu).MinSize.X, Math.Min(((Window)menu).MinSize.Y, (int)num12));
			((Window)menu).MaxSize = new Vector2I(Math.Max(((Window)menu).Size.X, ((Window)menu).MinSize.X), (int)num12);
			int item = ((menu.GetFocusedItem() >= 0) ? menu.GetFocusedItem() : selected);
			if (item >= 0)
			{
				Callable val3 = Callable.From((Action)delegate
				{
					if (((Window)menu).Visible && item < menu.ItemCount)
					{
						menu.ScrollToItem(item);
					}
				});
				((Callable)(ref val3)).CallDeferred(Array.Empty<Variant>());
			}
		}
		float num13 = (flag ? (y - num2 + num6 - (float)((Window)menu).Size.Y) : (num + num2 - num7));
		((Window)menu).Position = new Vector2I(((Window)menu).Position.X, Mathf.RoundToInt(num13));
	}

	internal static void FollowFocus(ScrollContainer scroll, float margin = 8f)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		scroll.FollowFocus = false;
		Viewport viewport = null;
		GuiFocusChangedEventHandler handler = new GuiFocusChangedEventHandler(Reveal);
		((Node)scroll).TreeEntered += Attach;
		((Node)scroll).TreeExiting += Detach;
		if (((Node)scroll).IsInsideTree())
		{
			Attach();
		}
		void Attach()
		{
			Detach();
			viewport = ((Node)scroll).GetViewport();
			viewport.GuiFocusChanged += handler;
		}
		void Detach()
		{
			if (viewport != null && GodotObject.IsInstanceValid((GodotObject)(object)viewport))
			{
				viewport.GuiFocusChanged -= handler;
			}
			viewport = null;
		}
		void Reveal(Control focused)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)scroll) && GodotObject.IsInstanceValid((GodotObject)(object)focused) && ((CanvasItem)scroll).IsVisibleInTree() && ((Node)scroll).IsAncestorOf((Node)(object)focused))
			{
				Transform2D globalTransform = ((CanvasItem)scroll).GetGlobalTransform();
				Rect2 val = ((Transform2D)(ref globalTransform)).AffineInverse() * ((CanvasItem)focused).GetGlobalTransform() * new Rect2(Vector2.Zero, focused.Size);
				Rect2 val2 = ((Rect2)(ref val)).Grow(margin);
				ScrollBar vScrollBar = (ScrollBar)(object)scroll.GetVScrollBar();
				ScrollBar hScrollBar = (ScrollBar)(object)scroll.GetHScrollBar();
				Vector2 val3 = ((Control)scroll).Size - new Vector2(((CanvasItem)vScrollBar).Visible ? ((Control)vScrollBar).Size.X : 0f, ((CanvasItem)hScrollBar).Visible ? ((Control)hScrollBar).Size.Y : 0f);
				float num = Shift(((Rect2)(ref val2)).Position.Y, ((Rect2)(ref val2)).End.Y, val3.Y);
				float num2 = (((int)scroll.HorizontalScrollMode == 0) ? 0f : Shift(((Rect2)(ref val2)).Position.X, ((Rect2)(ref val2)).End.X, val3.X));
				if (num != 0f)
				{
					((Range)vScrollBar).Value = ((Range)vScrollBar).Value + (double)num;
				}
				if (num2 != 0f)
				{
					((Range)hScrollBar).Value = ((Range)hScrollBar).Value + (double)num2;
				}
			}
		}
		static float Shift(float start, float end, float length)
		{
			if (!(start < 0f))
			{
				if (!(end > length))
				{
					return 0f;
				}
				return Math.Min(end - length, start);
			}
			return start;
		}
	}

	internal static void FitDialogHeight(Control card, ScrollContainer body, float maxShare)
	{
		bool queued = false;
		foreach (Control item in ((IEnumerable)((Node)body).GetChildren(false)).OfType<Control>())
		{
			item.MinimumSizeChanged += Queue;
		}
		card.Resized += Queue;
		Queue();
		void Fit()
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			queued = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)card) && GodotObject.IsInstanceValid((GodotObject)(object)body) && ((Node)card).IsInsideTree())
			{
				float num = (from child in ((IEnumerable)((Node)body).GetChildren(false)).OfType<Control>()
					select child.GetCombinedMinimumSize().Y).DefaultIfEmpty(0f).Max();
				float num2 = Mathf.Ceil(Math.Min(card.GetCombinedMinimumSize().Y + num, card.GetParentAreaSize().Y * maxShare));
				card.AnchorTop = 0.5f;
				card.AnchorBottom = 0.5f;
				card.OffsetTop = 0f - Mathf.Floor(num2 / 2f);
				card.OffsetBottom = Mathf.Ceil(num2 / 2f);
			}
		}
		void Queue()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			if (!queued)
			{
				queued = true;
				Callable val = Callable.From((Action)Fit);
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
		}
	}

	internal static Button Chip(string text, ButtonGroup group, bool pressed)
	{
		Button obj = Button(text, "ChipButton", 0f, 30f);
		((BaseButton)obj).ToggleMode = true;
		((BaseButton)obj).ButtonGroup = group;
		((BaseButton)obj).ButtonPressed = pressed;
		((Control)obj).MouseDefaultCursorShape = (CursorShape)2;
		return obj;
	}

	internal static Color BadgeInk(BadgeTone tone)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(tone switch
		{
			BadgeTone.Quiet => TextDim, 
			BadgeTone.Positive => Success, 
			BadgeTone.Caution => Warning, 
			BadgeTone.Critical => Danger, 
			_ => TextMuted, 
		});
	}

	private static bool IsStatusTone(BadgeTone tone)
	{
		if ((uint)(tone - 2) <= 2u)
		{
			return true;
		}
		return false;
	}

	private static StyleBoxFlat BadgeBox(BadgeTone tone, bool onReadingBand = false)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		Color val = BadgeInk(tone);
		bool flag = IsStatusTone(tone);
		Color surfaceRaised;
		if (_palette.IsLiquidGlass)
		{
			if (!onReadingBand)
			{
				surfaceRaised = _palette.SurfaceRaised;
				return Box(((Color)(ref surfaceRaised)).Lerp(val, flag ? 0.09f : 0.045f), Clear, 7, 0, 8, 4);
			}
			return Box(Clear, Clear, 0, 0, 3, 4);
		}
		if (!HasCustomBackdrop)
		{
			return Box(Alpha(val, flag ? 0.16f : 0.09f), Clear, 999, 0, 11, 5);
		}
		surfaceRaised = _palette.SurfaceRaised;
		return Box(((Color)(ref surfaceRaised)).Lerp(val, flag ? 0.14f : 0.09f), Alpha(val, flag ? 0.55f : 0.34f), 999, 1, 11, 5);
	}

	internal static PanelContainer Badge(string text, BadgeTone tone)
	{
		Label label;
		return Badge(text, tone, out label);
	}

	internal static PanelContainer Badge(string text, BadgeTone tone, out Label label)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		PanelContainer val = new PanelContainer
		{
			SizeFlagsHorizontal = (SizeFlags)0,
			SizeFlagsVertical = (SizeFlags)4
		};
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)BadgeBox(tone));
		HBoxContainer val2 = new HBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		Color val3 = BadgeInk(tone);
		Panel val4 = new Panel
		{
			CustomMinimumSize = new Vector2(6f, 6f),
			SizeFlagsVertical = (SizeFlags)4,
			MouseFilter = (MouseFilterEnum)2,
			Visible = IsStatusTone(tone)
		};
		((Control)val4).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)Box(val3, Clear, 999, 0, 0, 0));
		label = Label(text, "DimLabel");
		((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_color"), val3);
		label.VerticalAlignment = (VerticalAlignment)1;
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)label, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		return val;
	}

	internal static void RestyleBadge(PanelContainer badge, BadgeTone tone, bool onReadingBand = false)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		((Control)badge).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)BadgeBox(tone, onReadingBand));
		if (((Node)badge).GetChildCount(false) == 0)
		{
			return;
		}
		Node child = ((Node)badge).GetChild(0, false);
		HBoxContainer val = (HBoxContainer)(object)((child is HBoxContainer) ? child : null);
		if (val == null)
		{
			return;
		}
		Color val2 = BadgeInk(tone);
		if (((Node)val).GetChildCount(false) > 0)
		{
			Node child2 = ((Node)val).GetChild(0, false);
			Panel val3 = (Panel)(object)((child2 is Panel) ? child2 : null);
			if (val3 != null)
			{
				((Control)val3).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)Box(val2, Clear, 999, 0, 0, 0));
				((CanvasItem)val3).Visible = IsStatusTone(tone);
			}
		}
		if (((Node)val).GetChildCount(false) > 1)
		{
			Node child3 = ((Node)val).GetChild(1, false);
			Label val4 = (Label)(object)((child3 is Label) ? child3 : null);
			if (val4 != null)
			{
				((Control)val4).AddThemeColorOverride(StringName.op_Implicit("font_color"), val2);
			}
		}
	}

	internal static void RestyleGradient(TextureRect sheet, Color[] colors)
	{
		Texture2D texture = sheet.Texture;
		GradientTexture2D val = (GradientTexture2D)(object)((texture is GradientTexture2D) ? texture : null);
		if (val != null)
		{
			Gradient gradient = val.Gradient;
			if (gradient != null)
			{
				gradient.Colors = colors;
			}
		}
	}

	internal static HSeparator Divider()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		return new HSeparator
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
	}

	internal static Label Caption(string text)
	{
		return Label(text, "CaptionLabel");
	}

	internal static TextureRect GradientSheet(Color[] colors, float[] offsets, bool horizontal)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		Gradient gradient = new Gradient
		{
			Offsets = offsets,
			Colors = colors
		};
		return new TextureRect
		{
			Texture = (Texture2D)new GradientTexture2D
			{
				Gradient = gradient,
				Width = (horizontal ? 256 : 8),
				Height = (horizontal ? 8 : 256),
				FillFrom = Vector2.Zero,
				FillTo = (horizontal ? Vector2.Right : Vector2.Down)
			},
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)0,
			MouseFilter = (MouseFilterEnum)2
		};
	}

	internal static Shortcut Keys(Key keycode, bool ctrl = false, bool alt = false, bool shift = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		InputEventKey val = new InputEventKey
		{
			Keycode = keycode,
			CtrlPressed = ctrl,
			AltPressed = alt,
			ShiftPressed = shift
		};
		Array val2 = new Array();
		val2.Add(Variant.op_Implicit((GodotObject)(object)val));
		return new Shortcut
		{
			Events = val2
		};
	}

	internal static MarginContainer Margin(Control child, int horizontal = 16, int vertical = 14)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		MarginContainer val = new MarginContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), horizontal);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), horizontal);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), vertical);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), vertical);
		((Node)val).AddChild((Node)(object)child, false, (InternalMode)0);
		return val;
	}

	internal static T FullRect<T>(T control) where T : Control
	{
		((Control)control).AnchorLeft = 0f;
		((Control)control).AnchorTop = 0f;
		((Control)control).AnchorRight = 1f;
		((Control)control).AnchorBottom = 1f;
		((Control)control).OffsetLeft = 0f;
		((Control)control).OffsetTop = 0f;
		((Control)control).OffsetRight = 0f;
		((Control)control).OffsetBottom = 0f;
		return control;
	}

	private static StyleBoxFlat PanelBox(Color background, Color border, int radius, int borderWidth, int horizontalPadding, int verticalPadding, bool elevated = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		StyleBoxFlat val = Box(background, border, radius, borderWidth, horizontalPadding, verticalPadding);
		if (_palette.IsFluent && elevated)
		{
			val.ShadowColor = _palette.SurfaceShadow;
			val.ShadowSize = 4;
			val.ShadowOffset = new Vector2(0f, 2f);
			val.ExpandMarginLeft = 1f;
			val.ExpandMarginTop = 1f;
			val.ExpandMarginRight = 1f;
			val.ExpandMarginBottom = 2f;
		}
		return val;
	}

	internal static StyleBoxFlat DialogBox(int horizontalPadding = 22, int verticalPadding = 18)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if (_palette.IsLiquidGlass)
		{
			StyleBoxFlat obj = Box(_palette.SurfaceRaised, Alpha(_palette.Text, _palette.IsDark ? 0.14f : 0.08f), 22, 1, horizontalPadding, verticalPadding);
			obj.ShadowColor = new Color(0f, 0f, 0f, _palette.IsDark ? 0.48f : 0.2f);
			obj.ShadowSize = 28;
			obj.ShadowOffset = new Vector2(0f, 12f);
			return obj;
		}
		StyleBoxFlat obj2 = Box(_palette.SurfaceRaised, _palette.BorderStrong, 8, 1, horizontalPadding, verticalPadding);
		obj2.ShadowColor = _palette.SurfaceShadow;
		obj2.ShadowSize = 16;
		obj2.ShadowOffset = new Vector2(0f, 8f);
		return obj2;
	}

	internal static StyleBoxFlat InsetBox(int horizontalPadding, int verticalPadding)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!_palette.IsLiquidGlass)
		{
			return PanelBox(SurfaceInset, Border, 6, 1, horizontalPadding, verticalPadding);
		}
		return Box(SurfaceInset, Clear, 12, 0, horizontalPadding, verticalPadding);
	}

	internal static StyleBoxFlat NoticeBox(Color tone, int horizontalPadding = 12, int verticalPadding = 9)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (!_palette.IsLiquidGlass)
		{
			return Box(Alpha(tone, 0.12f), Alpha(tone, 0.55f), 8, 1, horizontalPadding, verticalPadding);
		}
		return Box(Alpha(tone, _palette.IsDark ? 0.16f : 0.1f), Clear, 12, 0, horizontalPadding + 2, verticalPadding + 2);
	}

	private static StyleBoxFlat FieldBox(Color background, Color border, bool focused)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!_palette.IsFluent)
		{
			if (!focused)
			{
				return Box(background, border, 5, 1, 12, 9);
			}
			return Box(background, border, 5, 2, 11, 8);
		}
		StyleBoxFlat val = Box(background, border, 4, 1, 12, 9);
		if (focused)
		{
			val.BorderWidthBottom = 2;
			((StyleBox)val).ContentMarginBottom = 8f;
		}
		return val;
	}

	private static void LeadingRule(Theme theme, string variation, string state)
	{
		StyleBox stylebox = theme.GetStylebox(StringName.op_Implicit(state), StringName.op_Implicit(variation));
		StyleBoxFlat val = (StyleBoxFlat)(object)((stylebox is StyleBoxFlat) ? stylebox : null);
		if (val != null)
		{
			val.BorderWidthLeft = 3;
			val.BorderWidthTop = 0;
			val.BorderWidthRight = 0;
			val.BorderWidthBottom = 0;
			theme.SetStylebox(StringName.op_Implicit(state), StringName.op_Implicit(variation), (StyleBox)(object)val);
		}
	}

	private static StyleBoxFlat TreeSelectionBox(Color background, bool focused)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!_palette.IsFluent)
		{
			if (!focused)
			{
				return Box(background, Clear, 4, 0, 4, 2);
			}
			return Box(background, _palette.TreeFocusBorder, 4, 1, 3, 1);
		}
		StyleBoxFlat val = Box(background, _palette.TreeFocusBorder, 4, 0, 5, 1);
		val.BorderWidthLeft = 3;
		if (focused)
		{
			val.BorderWidthTop = 1;
			val.BorderWidthRight = 1;
			val.BorderWidthBottom = 1;
		}
		return val;
	}

	internal static StyleBoxFlat Box(Color background, Color border, int radius, int borderWidth, int horizontalPadding, int verticalPadding)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		if (border.A <= 0f)
		{
			borderWidth = 0;
		}
		return new StyleBoxFlat
		{
			BgColor = background,
			BorderColor = border,
			BorderWidthLeft = borderWidth,
			BorderWidthTop = borderWidth,
			BorderWidthRight = borderWidth,
			BorderWidthBottom = borderWidth,
			CornerRadiusTopLeft = radius,
			CornerRadiusTopRight = radius,
			CornerRadiusBottomLeft = radius,
			CornerRadiusBottomRight = radius,
			ContentMarginLeft = horizontalPadding,
			ContentMarginRight = horizontalPadding,
			ContentMarginTop = verticalPadding,
			ContentMarginBottom = verticalPadding,
			AntiAliasing = true
		};
	}

	private static void AddPopupMenuStyles(Theme theme)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("PopupMenu"), (StyleBox)(object)PanelBox(_palette.SurfaceRaised, _palette.Border, 8, 1, 6, 6, elevated: true));
		theme.SetStylebox(StringName.op_Implicit("hover"), StringName.op_Implicit("PopupMenu"), (StyleBox)(object)Box(ControlHover, Clear, 4, 0, 8, 5));
		theme.SetStylebox(StringName.op_Implicit("separator"), StringName.op_Implicit("PopupMenu"), (StyleBox)new StyleBoxLine
		{
			Color = _palette.Border,
			Thickness = 1
		});
		theme.SetStylebox(StringName.op_Implicit("labeled_separator_left"), StringName.op_Implicit("PopupMenu"), (StyleBox)new StyleBoxLine
		{
			Color = _palette.Border,
			Thickness = 1
		});
		theme.SetStylebox(StringName.op_Implicit("labeled_separator_right"), StringName.op_Implicit("PopupMenu"), (StyleBox)new StyleBoxLine
		{
			Color = _palette.Border,
			Thickness = 1
		});
		theme.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("PopupMenu"), Text);
		theme.SetColor(StringName.op_Implicit("font_hover_color"), StringName.op_Implicit("PopupMenu"), Text);
		theme.SetColor(StringName.op_Implicit("font_disabled_color"), StringName.op_Implicit("PopupMenu"), Alpha(TextDim, 0.75f));
		theme.SetColor(StringName.op_Implicit("font_accelerator_color"), StringName.op_Implicit("PopupMenu"), TextDim);
		theme.SetColor(StringName.op_Implicit("font_separator_color"), StringName.op_Implicit("PopupMenu"), TextDim);
		theme.SetFont(StringName.op_Implicit("font"), StringName.op_Implicit("PopupMenu"), (Font)(object)UiFont);
		theme.SetFont(StringName.op_Implicit("font_separator"), StringName.op_Implicit("PopupMenu"), (Font)(object)UiFont);
		theme.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("PopupMenu"), ScaleFontSize(14));
		theme.SetFontSize(StringName.op_Implicit("font_separator_size"), StringName.op_Implicit("PopupMenu"), ScaleFontSize(12));
		theme.SetConstant(StringName.op_Implicit("item_start_padding"), StringName.op_Implicit("PopupMenu"), 10);
		theme.SetConstant(StringName.op_Implicit("item_end_padding"), StringName.op_Implicit("PopupMenu"), 10);
		theme.SetConstant(StringName.op_Implicit("v_separation"), StringName.op_Implicit("PopupMenu"), 3);
	}

	private static void AddFluentGlyphs(Theme theme)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0452: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		if (_palette.IsFluent || _palette.IsLiquidGlass)
		{
			ImageTexture val = UiGlyphs.CheckBox(18, _palette.SurfaceRaised, TextDim, 1f, Clear, UiGlyphs.Mark.None);
			ImageTexture val2 = UiGlyphs.CheckBox(18, AccentStrong, AccentStrong, 1f, OnAccent, UiGlyphs.Mark.Check);
			ImageTexture val3 = UiGlyphs.CheckBox(18, AccentStrong, AccentStrong, 1f, OnAccent, UiGlyphs.Mark.Dash);
			theme.SetIcon(StringName.op_Implicit("unchecked"), StringName.op_Implicit("Tree"), (Texture2D)(object)val);
			theme.SetIcon(StringName.op_Implicit("checked"), StringName.op_Implicit("Tree"), (Texture2D)(object)val2);
			theme.SetIcon(StringName.op_Implicit("indeterminate"), StringName.op_Implicit("Tree"), (Texture2D)(object)val3);
			theme.SetIcon(StringName.op_Implicit("unchecked_disabled"), StringName.op_Implicit("Tree"), (Texture2D)(object)UiGlyphs.CheckBox(18, _palette.DisabledFill, _palette.DisabledBorder, 1f, Clear, UiGlyphs.Mark.None));
			theme.SetIcon(StringName.op_Implicit("checked_disabled"), StringName.op_Implicit("Tree"), (Texture2D)(object)UiGlyphs.CheckBox(18, _palette.DisabledFill, _palette.DisabledBorder, 1f, Alpha(TextDim, 0.5f), UiGlyphs.Mark.Check));
			theme.SetIcon(StringName.op_Implicit("indeterminate_disabled"), StringName.op_Implicit("Tree"), (Texture2D)(object)UiGlyphs.CheckBox(18, _palette.DisabledFill, _palette.DisabledBorder, 1f, Alpha(TextDim, 0.5f), UiGlyphs.Mark.Dash));
			ImageTexture val4 = UiGlyphs.Radio(18, _palette.SurfaceRaised, TextDim, 1f, AccentStrong, selected: false);
			ImageTexture val5 = UiGlyphs.Radio(18, _palette.SurfaceRaised, AccentStrong, 1.5f, AccentStrong, selected: true);
			ImageTexture val6 = UiGlyphs.Radio(18, _palette.DisabledFill, _palette.DisabledBorder, 1f, TextDim, selected: false);
			ImageTexture val7 = UiGlyphs.Radio(18, _palette.DisabledFill, _palette.DisabledBorder, 1f, Alpha(TextDim, 0.5f), selected: true);
			theme.SetIcon(StringName.op_Implicit("radio_unchecked"), StringName.op_Implicit("PopupMenu"), (Texture2D)(object)val4);
			theme.SetIcon(StringName.op_Implicit("radio_checked"), StringName.op_Implicit("PopupMenu"), (Texture2D)(object)val5);
			theme.SetIcon(StringName.op_Implicit("radio_unchecked_disabled"), StringName.op_Implicit("PopupMenu"), (Texture2D)(object)val6);
			theme.SetIcon(StringName.op_Implicit("radio_checked_disabled"), StringName.op_Implicit("PopupMenu"), (Texture2D)(object)val7);
			theme.SetIcon(StringName.op_Implicit("checked"), StringName.op_Implicit("PopupMenu"), (Texture2D)(object)val2);
			theme.SetIcon(StringName.op_Implicit("checked_disabled"), StringName.op_Implicit("PopupMenu"), (Texture2D)(object)UiGlyphs.CheckBox(18, _palette.DisabledFill, _palette.DisabledBorder, 1f, Alpha(TextDim, 0.5f), UiGlyphs.Mark.Check));
			theme.SetIcon(StringName.op_Implicit("arrow"), StringName.op_Implicit("OptionButton"), (Texture2D)(object)UiGlyphs.Chevron(12, new Color(1f, 1f, 1f, 1f), 2f));
			theme.SetConstant(StringName.op_Implicit("arrow_margin"), StringName.op_Implicit("OptionButton"), 10);
			theme.SetConstant(StringName.op_Implicit("modulate_arrow"), StringName.op_Implicit("OptionButton"), 1);
			theme.SetStylebox(StringName.op_Implicit("slider"), StringName.op_Implicit("HSlider"), (StyleBox)(object)Box(Alpha(Text, 0.16f), Clear, 999, 0, 0, 2));
			theme.SetStylebox(StringName.op_Implicit("grabber_area"), StringName.op_Implicit("HSlider"), (StyleBox)(object)Box(AccentStrong, Clear, 999, 0, 0, 2));
			theme.SetStylebox(StringName.op_Implicit("grabber_area_highlight"), StringName.op_Implicit("HSlider"), (StyleBox)(object)Box(_palette.PrimaryHover, Clear, 999, 0, 0, 2));
			theme.SetIcon(StringName.op_Implicit("grabber"), StringName.op_Implicit("HSlider"), (Texture2D)(object)UiGlyphs.Radio(20, _palette.SurfaceRaised, BorderStrong, 1f, AccentStrong, selected: true));
			theme.SetIcon(StringName.op_Implicit("grabber_highlight"), StringName.op_Implicit("HSlider"), (Texture2D)(object)UiGlyphs.Radio(20, _palette.SurfaceRaised, AccentStrong, 1f, _palette.PrimaryHover, selected: true));
			theme.SetIcon(StringName.op_Implicit("grabber_disabled"), StringName.op_Implicit("HSlider"), (Texture2D)(object)UiGlyphs.Radio(20, _palette.DisabledFill, _palette.DisabledBorder, 1f, TextDim, selected: true));
		}
	}

	private static void AddScrollBarStyles(Theme theme, string type)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		bool flag = type == "VScrollBar";
		StyleBoxEmpty val = new StyleBoxEmpty();
		int num = 7;
		if (flag)
		{
			float contentMarginLeft = (((StyleBox)val).ContentMarginRight = num);
			((StyleBox)val).ContentMarginLeft = contentMarginLeft;
			contentMarginLeft = (((StyleBox)val).ContentMarginBottom = 4f);
			((StyleBox)val).ContentMarginTop = contentMarginLeft;
		}
		else
		{
			float contentMarginLeft = (((StyleBox)val).ContentMarginBottom = num);
			((StyleBox)val).ContentMarginTop = contentMarginLeft;
			contentMarginLeft = (((StyleBox)val).ContentMarginRight = 4f);
			((StyleBox)val).ContentMarginLeft = contentMarginLeft;
		}
		theme.SetStylebox(StringName.op_Implicit("scroll"), StringName.op_Implicit(type), (StyleBox)(object)val);
		theme.SetStylebox(StringName.op_Implicit("scroll_focus"), StringName.op_Implicit(type), (StyleBox)(object)val);
		theme.SetStylebox(StringName.op_Implicit("grabber"), StringName.op_Implicit(type), (StyleBox)(object)ScrollPill(_palette.ScrollGrabber, flag, 4));
		theme.SetStylebox(StringName.op_Implicit("grabber_highlight"), StringName.op_Implicit(type), (StyleBox)(object)ScrollPill(_palette.ScrollGrabberHover, flag, 7));
		theme.SetStylebox(StringName.op_Implicit("grabber_pressed"), StringName.op_Implicit(type), (StyleBox)(object)ScrollPill(_palette.ScrollGrabberPressed, flag, 7));
	}

	private static StyleBoxFlat ScrollPill(Color color, bool vertical, int thickness)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		int num = 11 - thickness;
		StyleBoxFlat val = new StyleBoxFlat
		{
			BgColor = color,
			AntiAliasing = true,
			CornerRadiusTopLeft = 999,
			CornerRadiusTopRight = 999,
			CornerRadiusBottomLeft = 999,
			CornerRadiusBottomRight = 999
		};
		if (vertical)
		{
			val.ExpandMarginLeft = -num;
			val.ExpandMarginRight = -3f;
			float contentMarginTop = (((StyleBox)val).ContentMarginBottom = 14f);
			((StyleBox)val).ContentMarginTop = contentMarginTop;
		}
		else
		{
			val.ExpandMarginTop = -num;
			val.ExpandMarginBottom = -3f;
			float contentMarginTop = (((StyleBox)val).ContentMarginRight = 14f);
			((StyleBox)val).ContentMarginLeft = contentMarginTop;
		}
		return val;
	}

	private static void AddLabelVariation(Theme theme, string name, int size, Color color)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		theme.SetTypeVariation(StringName.op_Implicit(name), StringName.op_Implicit("Label"));
		theme.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit(name), size);
		theme.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit(name), color);
	}

	private static void AddButtonVariation(Theme theme, string name, Color normal, Color hover, Color pressed, Color text, int radius = 5, int horizontalPadding = 14, int verticalPadding = 9, int fontSize = 15, Color? border = null, Color? hoverBorder = null, Color? pressedBorder = null, Color? pressedText = null, Color? hoverText = null, Color? hoverPressed = null, Color? hoverPressedBorder = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		Color border2 = (Color)(((_003F?)border) ?? Border);
		Color val = (Color)(((_003F?)hoverBorder) ?? _palette.HoverBorder);
		Color val2 = pressedBorder ?? val;
		Color background = hoverPressed ?? pressed;
		Color border3 = hoverPressedBorder ?? val2;
		theme.SetTypeVariation(StringName.op_Implicit(name), StringName.op_Implicit("Button"));
		theme.SetStylebox(StringName.op_Implicit("normal"), StringName.op_Implicit(name), (StyleBox)(object)Box(normal, border2, radius, 1, horizontalPadding, verticalPadding));
		theme.SetStylebox(StringName.op_Implicit("hover"), StringName.op_Implicit(name), (StyleBox)(object)Box(hover, val, radius, 1, horizontalPadding, verticalPadding));
		theme.SetStylebox(StringName.op_Implicit("pressed"), StringName.op_Implicit(name), (StyleBox)(object)Box(pressed, val2, radius, 1, horizontalPadding, verticalPadding));
		theme.SetStylebox(StringName.op_Implicit("hover_pressed"), StringName.op_Implicit(name), (StyleBox)(object)Box(background, border3, radius, 1, horizontalPadding, verticalPadding));
		theme.SetStylebox(StringName.op_Implicit("focus"), StringName.op_Implicit(name), (StyleBox)(object)Box(Clear, _palette.FocusRing, radius, 2, horizontalPadding - 1, verticalPadding - 1));
		theme.SetStylebox(StringName.op_Implicit("disabled"), StringName.op_Implicit(name), (StyleBox)(object)Box(_palette.DisabledFill, _palette.DisabledBorder, radius, 1, horizontalPadding, verticalPadding));
		theme.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit(name), text);
		theme.SetColor(StringName.op_Implicit("font_hover_color"), StringName.op_Implicit(name), (Color)(((_003F?)hoverText) ?? Text));
		theme.SetColor(StringName.op_Implicit("font_pressed_color"), StringName.op_Implicit(name), (Color)(((_003F?)pressedText) ?? ((_003F?)hoverText) ?? Text));
		theme.SetColor(StringName.op_Implicit("font_hover_pressed_color"), StringName.op_Implicit(name), (Color)(((_003F?)pressedText) ?? ((_003F?)hoverText) ?? Text));
		theme.SetColor(StringName.op_Implicit("font_focus_color"), StringName.op_Implicit(name), (Color)(((_003F?)hoverText) ?? Text));
		theme.SetColor(StringName.op_Implicit("font_disabled_color"), StringName.op_Implicit(name), Alpha(TextDim, _palette.IsDark ? 0.7f : 0.85f));
		theme.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit(name), fontSize);
	}

	internal static Color Html(string value)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Color.FromHtml(value.AsSpan());
	}

	private static void AddLiquidGlassStyles(Theme theme)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0745: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_083d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0859: Unknown result type (might be due to invalid IL or missing references)
		//IL_087a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a24: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0921: Unknown result type (might be due to invalid IL or missing references)
		//IL_0928: Expected O, but got Unknown
		//IL_0aa2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b4d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b6c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bfc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c53: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c58: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c80: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cbe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d11: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0962: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0969: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d69: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dbf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dfa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e30: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e63: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e94: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e99: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ea3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eb5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f25: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f34: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0988: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		theme.SetTypeVariation(StringName.op_Implicit("GlassGroupPanel"), StringName.op_Implicit("PanelContainer"));
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("GlassGroupPanel"), (StyleBox)(object)Box(Clear, Clear, 0, 0, 0, 0));
		theme.SetTypeVariation(StringName.op_Implicit("GlassMenuButton"), StringName.op_Implicit("MiniButton"));
		theme.SetTypeVariation(StringName.op_Implicit("GlassIconButton"), StringName.op_Implicit("AccentIconButton"));
		theme.SetTypeVariation(StringName.op_Implicit("GlassGroupButton"), StringName.op_Implicit("SecondaryButton"));
		theme.SetTypeVariation(StringName.op_Implicit("GlassInspectorPanel"), StringName.op_Implicit("CardPanel"));
		theme.SetTypeVariation(StringName.op_Implicit("GlassSlider"), StringName.op_Implicit("HSlider"));
		if (!_palette.IsLiquidGlass)
		{
			return;
		}
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("GlassGroupPanel"), (StyleBox)(object)Box(Clear, Clear, 999, 0, 4, 3));
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("BarPanel"), (StyleBox)(object)Box(Clear, Clear, 0, 0, 0, 7));
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("StatusPanel"), (StyleBox)(object)Box(Clear, Clear, 0, 0, 2, 0));
		StyleBoxFlat val = Box(_palette.SurfaceRaised, Clear, 22, 0, 20, 18);
		val.ShadowColor = _palette.SurfaceShadow;
		val.ShadowSize = 8;
		val.ShadowOffset = new Vector2(0f, 3f);
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("CardPanel"), (StyleBox)(object)val);
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("GlassInspectorPanel"), (StyleBox)(object)Box(Clear, Clear, 22, 0, 20, 18));
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("InsetPanel"), (StyleBox)(object)Box(_palette.SurfaceInset, Clear, 12, 0, 14, 12));
		string[] array = new string[4] { "TitleLabel", "SectionLabel", "MetricLabel", "GroupLabel" };
		foreach (string text in array)
		{
			theme.SetFont(StringName.op_Implicit("font"), StringName.op_Implicit(text), (Font)(object)GlassHeadingFont);
		}
		theme.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("TitleLabel"), ScaleFontSize(21));
		theme.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("SectionLabel"), ScaleFontSize(18));
		theme.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("MetricLabel"), ScaleFontSize(18));
		theme.SetFontSize(StringName.op_Implicit("font_size"), StringName.op_Implicit("CaptionLabel"), ScaleFontSize(12));
		theme.SetConstant(StringName.op_Implicit("shadow_offset_y"), StringName.op_Implicit("Label"), 0);
		string[] array2 = new string[11]
		{
			"PrimaryButton", "SecondaryButton", "ToolbarButton", "DangerButton", "IconButton", "AccentIconButton", "MiniButton", "SwitchButton", "FieldDropdown", "ChipButton",
			"RowButton"
		};
		string[] array3 = new string[6] { "normal", "hover", "pressed", "hover_pressed", "disabled", "focus" };
		array = array2;
		foreach (string text2 in array)
		{
			string[] array4 = array3;
			foreach (string text3 in array4)
			{
				StyleBox stylebox = theme.GetStylebox(StringName.op_Implicit(text3), StringName.op_Implicit(text2));
				StyleBoxFlat val2 = (StyleBoxFlat)(object)((stylebox is StyleBoxFlat) ? stylebox : null);
				if (val2 == null)
				{
					continue;
				}
				int cornerRadiusAll = ((text2 == "RowButton") ? 8 : ((text2 == "FieldDropdown") ? 9 : 999));
				val2.SetCornerRadiusAll(cornerRadiusAll);
				val2.SetBorderWidthAll((text3 == "focus") ? 2 : 0);
				if (text3 == "focus")
				{
					val2.BorderColor = _palette.FocusRing;
				}
				else if (text2 == "PrimaryButton" && text3 != "disabled")
				{
					val2.BgColor = Clear;
				}
				else if (text2 == "ChipButton" && text3 == "normal")
				{
					val2.BgColor = Clear;
				}
				else if (text2 == "ChipButton" && text3 == "hover")
				{
					val2.BgColor = Alpha(Text, 0.07f);
				}
				else
				{
					bool flag;
					switch (text2)
					{
					case "ToolbarButton":
					case "MiniButton":
					case "SwitchButton":
					case "FieldDropdown":
					case "IconButton":
					case "AccentIconButton":
						flag = true;
						break;
					default:
						flag = false;
						break;
					}
					if (flag && text3 == "normal")
					{
						val2.BgColor = _palette.SecondaryFill;
					}
				}
				if (text2 == "SecondaryButton" && text3 == "normal")
				{
					val2.ShadowColor = new Color(0f, 0f, 0f, _palette.IsDark ? 0.16f : 0.09f);
					val2.ShadowSize = 2;
					val2.ShadowOffset = new Vector2(0f, 1f);
				}
				else if (text2 == "ChipButton")
				{
					val2.ShadowSize = 0;
				}
				theme.SetStylebox(StringName.op_Implicit(text3), StringName.op_Implicit(text2), (StyleBox)(object)val2);
			}
			array4 = new string[5] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color" };
			foreach (string text4 in array4)
			{
				StringName obj = StringName.op_Implicit(text4);
				StringName obj2 = StringName.op_Implicit(text2);
				_003F val3;
				switch (text2)
				{
				case "ChipButton":
					if (text4 == "font_color")
					{
						val3 = TextMuted;
						break;
					}
					goto default;
				default:
					val3 = Text;
					break;
				case "DangerButton":
					val3 = ((text4 == "font_color") ? _palette.DangerText : _palette.DangerHoverText);
					break;
				case "PrimaryButton":
					val3 = OnAccent;
					break;
				}
				theme.SetColor(obj, obj2, (Color)val3);
			}
			theme.SetColor(StringName.op_Implicit("font_disabled_color"), StringName.op_Implicit(text2), TextDim);
		}
		theme.SetColor(StringName.op_Implicit("font_pressed_color"), StringName.op_Implicit("SwitchButton"), OnAccent);
		theme.SetColor(StringName.op_Implicit("font_hover_pressed_color"), StringName.op_Implicit("SwitchButton"), OnAccent);
		array = new string[2] { "LineEdit", "TextEdit" };
		foreach (string text5 in array)
		{
			theme.SetStylebox(StringName.op_Implicit("normal"), StringName.op_Implicit(text5), (StyleBox)(object)Box(_palette.SurfaceInset, Clear, 9, 0, 12, 9));
			theme.SetStylebox(StringName.op_Implicit("read_only"), StringName.op_Implicit(text5), (StyleBox)(object)Box(_palette.Surface, Clear, 9, 0, 12, 9));
			theme.SetStylebox(StringName.op_Implicit("focus"), StringName.op_Implicit(text5), (StyleBox)(object)Box(Clear, _palette.FocusRing, 9, 2, 12, 9));
			theme.SetColor(StringName.op_Implicit("caret_color"), StringName.op_Implicit(text5), Accent);
			theme.SetColor(StringName.op_Implicit("font_selected_color"), StringName.op_Implicit(text5), Text);
			theme.SetColor(StringName.op_Implicit("selection_color"), StringName.op_Implicit(text5), _palette.AccentSoft);
		}
		array = new string[3] { "GlassMenuButton", "GlassIconButton", "GlassGroupButton" };
		foreach (string text6 in array)
		{
			string text7 = ((text6 == "GlassMenuButton") ? "MiniButton" : ((text6 == "GlassIconButton") ? "AccentIconButton" : "SecondaryButton"));
			string[] array4 = array3;
			foreach (string text8 in array4)
			{
				StyleBoxFlat val4 = (StyleBoxFlat)((Resource)theme.GetStylebox(StringName.op_Implicit(text8), StringName.op_Implicit(text7))).Duplicate(false);
				if (!(text8 == "focus") && !(text8 == "disabled"))
				{
					val4.BgColor = ((text6 == "GlassGroupButton" && text8 != "normal") ? Alpha(Text, (text8 == "hover") ? 0.06f : 0.1f) : Clear);
					val4.ShadowSize = 0;
				}
				theme.SetStylebox(StringName.op_Implicit(text8), StringName.op_Implicit(text6), (StyleBox)(object)val4);
			}
		}
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(Clear, Clear, 10, 0, 5, 5));
		theme.SetStylebox(StringName.op_Implicit("focus"), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(Clear, Alpha(Accent, 0.3f), 10, 1, 0, 0));
		array = new string[4] { "selected", "selected_focus", "hovered_selected", "hovered_selected_focus" };
		foreach (string text9 in array)
		{
			theme.SetStylebox(StringName.op_Implicit(text9), StringName.op_Implicit("Tree"), (StyleBox)(object)Box(text9.StartsWith("hovered") ? _palette.TreeHoverSelected : _palette.AccentSoft, Clear, 7, 0, 4, 2));
		}
		theme.SetColor(StringName.op_Implicit("font_color"), StringName.op_Implicit("Tree"), Text);
		array = new string[3] { "font_selected_color", "font_hovered_selected_color", "font_hovered_color" };
		foreach (string text10 in array)
		{
			theme.SetColor(StringName.op_Implicit(text10), StringName.op_Implicit("Tree"), Text);
		}
		theme.SetColor(StringName.op_Implicit("font_hovered_dimmed_color"), StringName.op_Implicit("Tree"), TextMuted);
		theme.SetColor(StringName.op_Implicit("font_disabled_color"), StringName.op_Implicit("Tree"), TextDim);
		theme.SetColor(StringName.op_Implicit("guide_color"), StringName.op_Implicit("Tree"), Clear);
		theme.SetConstant(StringName.op_Implicit("v_separation"), StringName.op_Implicit("Tree"), 7);
		StyleBoxFlat val5 = Box(_palette.SurfaceRaised, Alpha(Text, 0.1f), 12, 1, 6, 6);
		val5.ShadowColor = new Color(0f, 0f, 0f, _palette.IsDark ? 0.3f : 0.16f);
		val5.ShadowSize = 12;
		val5.ShadowOffset = new Vector2(0f, 4f);
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("PopupMenu"), (StyleBox)(object)val5);
		theme.SetStylebox(StringName.op_Implicit("hover"), StringName.op_Implicit("PopupMenu"), (StyleBox)(object)Box(AccentStrong, Clear, 6, 0, 8, 5));
		theme.SetColor(StringName.op_Implicit("font_hover_color"), StringName.op_Implicit("PopupMenu"), Colors.White);
		theme.SetColor(StringName.op_Implicit("font_disabled_color"), StringName.op_Implicit("PopupMenu"), TextDim);
		theme.SetColor(StringName.op_Implicit("font_selected_color"), StringName.op_Implicit("RichTextLabel"), Text);
		theme.SetColor(StringName.op_Implicit("selection_color"), StringName.op_Implicit("RichTextLabel"), _palette.AccentSoft);
		theme.SetConstant(StringName.op_Implicit("v_separation"), StringName.op_Implicit("PopupMenu"), 5);
		StyleBoxFlat val6 = Box(_palette.TooltipFill, Border, 8, 1, 11, 7);
		val6.ShadowColor = new Color(0f, 0f, 0f, _palette.IsDark ? 0.3f : 0.14f);
		val6.ShadowSize = 6;
		val6.ShadowOffset = new Vector2(0f, 2f);
		theme.SetStylebox(StringName.op_Implicit("panel"), StringName.op_Implicit("TooltipPanel"), (StyleBox)(object)val6);
		theme.SetConstant(StringName.op_Implicit("outline_size"), StringName.op_Implicit("ProgressBar"), 0);
		theme.SetStylebox(StringName.op_Implicit("slider"), StringName.op_Implicit("HSlider"), (StyleBox)(object)Box(Alpha(Text, 0.12f), Clear, 999, 0, 0, 3));
		theme.SetStylebox(StringName.op_Implicit("grabber_area"), StringName.op_Implicit("HSlider"), (StyleBox)(object)Box(Accent, Clear, 999, 0, 0, 3));
		theme.SetStylebox(StringName.op_Implicit("grabber_area_highlight"), StringName.op_Implicit("HSlider"), (StyleBox)(object)Box(Accent, Clear, 999, 0, 0, 3));
		theme.SetIcon(StringName.op_Implicit("grabber"), StringName.op_Implicit("HSlider"), (Texture2D)(object)UiGlyphs.Radio(22, Colors.White, Border, 1f, Clear, selected: false));
		theme.SetIcon(StringName.op_Implicit("grabber_highlight"), StringName.op_Implicit("HSlider"), (Texture2D)(object)UiGlyphs.Radio(22, Colors.White, Accent, 1f, Clear, selected: false));
		Texture2D val7 = (Texture2D)(object)UiGlyphs.Radio(24, Clear, Clear, 0f, Clear, selected: false);
		theme.SetIcon(StringName.op_Implicit("grabber"), StringName.op_Implicit("GlassSlider"), val7);
		theme.SetIcon(StringName.op_Implicit("grabber_highlight"), StringName.op_Implicit("GlassSlider"), val7);
		theme.SetIcon(StringName.op_Implicit("grabber_disabled"), StringName.op_Implicit("GlassSlider"), (Texture2D)(object)UiGlyphs.Radio(24, _palette.DisabledFill, Border, 1f, Clear, selected: false));
		theme.SetConstant(StringName.op_Implicit("center_grabber"), StringName.op_Implicit("GlassSlider"), 0);
		theme.SetConstant(StringName.op_Implicit("grabber_offset"), StringName.op_Implicit("GlassSlider"), 0);
	}
}
