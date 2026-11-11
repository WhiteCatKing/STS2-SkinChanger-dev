using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CznCore.Godot;
using Godot;

namespace CznCore.Presentation;

public static class UiSharpnessAudit
{
	public sealed record Finding(string Kind, string Resource, string NodePath, Vector2 SourcePixels, Vector2 ScreenPixels, float Density)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Kind = ");
			builder.Append((object?)Kind);
			builder.Append(", Resource = ");
			builder.Append((object?)Resource);
			builder.Append(", NodePath = ");
			builder.Append((object?)NodePath);
			builder.Append(", SourcePixels = ");
			Vector2 val = SourcePixels;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", ScreenPixels = ");
			val = ScreenPixels;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Density = ");
			builder.Append(Density.ToString());
			return true;
		}
	}

	public const string EnvironmentFlag = "CZN_UI_SHARPNESS_AUDIT";

	private const float Tolerance = 0.95f;

	private const string OutputDirectory = "user://czn_ui_sharpness";

	private static bool _hotkeyInstalled;

	private static bool _hotkeyDown;

	public static IReadOnlyList<Finding> Scan(Node root)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		List<Finding> list = new List<Finding>();
		foreach (Node item in GodotNodeUtil.TraverseDepthFirst(root))
		{
			CanvasItem val = (CanvasItem)(object)((item is CanvasItem) ? item : null);
			if (val == null || !val.IsVisibleInTree())
			{
				continue;
			}
			float scale = ScreenScale(val);
			CanvasItem val2 = val;
			NinePatchRect val3 = (NinePatchRect)(object)((val2 is NinePatchRect) ? val2 : null);
			if (val3 == null)
			{
				TextureRect val4 = (TextureRect)(object)((val2 is TextureRect) ? val2 : null);
				if (val4 == null)
				{
					TextureButton val5 = (TextureButton)(object)((val2 is TextureButton) ? val2 : null);
					if (val5 == null)
					{
						Sprite2D val6 = (Sprite2D)(object)((val2 is Sprite2D) ? val2 : null);
						if (val6 != null && val6.Texture != null)
						{
							Vector2 size;
							if (!val6.RegionEnabled)
							{
								size = val6.Texture.GetSize();
							}
							else
							{
								Rect2 regionRect = val6.RegionRect;
								size = ((Rect2)(ref regionRect)).Size;
							}
							Vector2 val7 = size;
							AddStretched(list, "sprite", val6.Texture, (CanvasItem)(object)val6, val7, val7, scale);
						}
					}
					else if (val5.TextureNormal != null)
					{
						AddStretched(list, "texture", val5.TextureNormal, (CanvasItem)(object)val5, val5.TextureNormal.GetSize(), ((Control)val5).Size, scale);
					}
				}
				else if (val4.Texture != null)
				{
					AddStretched(list, "texture", val4.Texture, (CanvasItem)(object)val4, val4.Texture.GetSize(), ((Control)val4).Size, scale);
				}
			}
			else if (val3.Texture != null)
			{
				AddCorners(list, val3.Texture, (CanvasItem)(object)val3, scale);
			}
			Control val8 = (Control)(object)((val is Control) ? val : null);
			if (val8 == null)
			{
				continue;
			}
			string[] array = new string[4] { "panel", "normal", "hover", "pressed" };
			foreach (string text in array)
			{
				if (val8.HasThemeStyleboxOverride(StringName.op_Implicit(text)))
				{
					StyleBox themeStylebox = val8.GetThemeStylebox(StringName.op_Implicit(text), (StringName)null);
					StyleBoxTexture val9 = (StyleBoxTexture)(object)((themeStylebox is StyleBoxTexture) ? themeStylebox : null);
					if (val9 != null && val9.Texture != null)
					{
						AddCorners(list, val9.Texture, (CanvasItem)(object)val8, scale);
					}
				}
			}
			if ((val8 is Label || val8 is RichTextLabel || val8 is Button || val8 is LineEdit) ? true : false)
			{
				AddText(list, val8, scale);
			}
		}
		return list;
	}

	public static string ScanAndReport(string label)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return "";
		}
		IReadOnlyList<Finding> readOnlyList = Scan((Node)(object)val.Root);
		DirAccess.MakeDirRecursiveAbsolute(ProjectSettings.GlobalizePath("user://czn_ui_sharpness"));
		string text = $"{"user://czn_ui_sharpness"}/{label}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
		StringBuilder stringBuilder = new StringBuilder("kind,resource,density,source_px,screen_px,node\n");
		foreach (Finding item in readOnlyList.OrderBy((Finding f) => f.Density))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(12, 8, stringBuilder2, invariantCulture);
			handler.AppendFormatted(item.Kind);
			handler.AppendLiteral(",\"");
			handler.AppendFormatted(item.Resource);
			handler.AppendLiteral("\",");
			handler.AppendFormatted(item.Density, "0.###");
			handler.AppendLiteral(",");
			handler.AppendFormatted(item.SourcePixels.X, "0");
			handler.AppendLiteral("x");
			handler.AppendFormatted(item.SourcePixels.Y, "0");
			handler.AppendLiteral(",");
			handler.AppendFormatted(item.ScreenPixels.X, "0");
			handler.AppendLiteral("x");
			handler.AppendFormatted(item.ScreenPixels.Y, "0");
			handler.AppendLiteral(",\"");
			handler.AppendFormatted(item.NodePath);
			handler.AppendLiteral("\"\n");
			stringBuilder2.Append(invariantCulture, ref handler);
		}
		FileAccess val2 = FileAccess.Open(text, (ModeFlags)2);
		try
		{
			if (val2 != null)
			{
				val2.StoreString(stringBuilder.ToString());
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
		IEnumerable<((string Kind, string Resource) Key, float Density, int Count)> enumerable = (from f in readOnlyList
			group f by (Kind: f.Kind, Resource: f.Resource) into g
			select (Key: g.Key, Density: g.Min((Finding f) => f.Density), Count: g.Count()) into g
			orderby g.Density
			select g).Take(15);
		SkinLog.Info($"UI sharpness scan '{label}': {readOnlyList.Count} findings at window {val.Root.Size}; report {ProjectSettings.GlobalizePath(text)}");
		foreach (var item2 in enumerable)
		{
			SkinLog.Info($"  {item2.Key.Kind} density {item2.Density:0.00} x{item2.Count}: {item2.Key.Resource}");
		}
		return text;
	}

	public static void InstallHotkeyIfRequested()
	{
		bool flag = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CZN_UI_SHARPNESS_AUDIT")) || FileAccess.FileExists("user://czn_ui_sharpness/ENABLED");
		if (_hotkeyInstalled || !flag)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		_hotkeyInstalled = true;
		val.ProcessFrame += delegate
		{
			int num;
			if (Input.IsKeyPressed((Key)4194341) && Input.IsKeyPressed((Key)4194326))
			{
				num = (Input.IsKeyPressed((Key)4194325) ? 1 : 0);
				if (num != 0 && !_hotkeyDown)
				{
					ScanAndReport("hotkey");
				}
			}
			else
			{
				num = 0;
			}
			_hotkeyDown = (byte)num != 0;
		};
		SkinLog.Info("UI sharpness audit armed: Ctrl+Shift+F10 scans the current screen.");
	}

	public static float ScreenScale(CanvasItem item)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Transform2D val = item.GetGlobalTransformWithCanvas();
		float num = ((Vector2)(ref val.X)).Length();
		Viewport viewport = ((Node)item).GetViewport();
		while (true)
		{
			SubViewport val2 = (SubViewport)(object)((viewport is SubViewport) ? viewport : null);
			if (val2 == null)
			{
				break;
			}
			Node parent = ((Node)val2).GetParent();
			SubViewportContainer val3 = (SubViewportContainer)(object)((parent is SubViewportContainer) ? parent : null);
			if (val3 == null)
			{
				break;
			}
			float num2 = (val3.Stretch ? ((float)val3.StretchShrink) : 1f);
			float num3 = num;
			val = ((CanvasItem)val3).GetGlobalTransformWithCanvas();
			num = num3 * (num2 * ((Vector2)(ref val.X)).Length());
			viewport = ((Node)val3).GetViewport();
		}
		float num4 = num;
		val = viewport.GetFinalTransform();
		return num4 * ((Vector2)(ref val.X)).Length();
	}

	private static void AddStretched(List<Finding> findings, string kind, Texture2D texture, CanvasItem item, Vector2 source, Vector2 local, float scale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = local * scale;
		if (!(val.X < 1f) && !(val.Y < 1f))
		{
			source *= Texels(texture);
			float num = Mathf.Min(source.X / val.X, source.Y / val.Y);
			if (num < 0.95f)
			{
				findings.Add(new Finding(kind, ((Resource)texture).ResourcePath, NodePath.op_Implicit(((Node)item).GetPath()), source, val, num));
			}
		}
	}

	private static void AddCorners(List<Finding> findings, Texture2D texture, CanvasItem item, float scale)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		float num = Texels(texture);
		if (num / scale < 0.95f)
		{
			findings.Add(new Finding("nine-patch", ((Resource)texture).ResourcePath, NodePath.op_Implicit(((Node)item).GetPath()), texture.GetSize() * num, texture.GetSize() * scale, num / scale));
		}
	}

	private static float Texels(Texture2D texture)
	{
		ImageTexture val = (ImageTexture)(object)((texture is ImageTexture) ? texture : null);
		if (val == null || texture.GetWidth() == 0)
		{
			return 1f;
		}
		return (float)((Texture2D)val).GetImage().GetWidth() / (float)texture.GetWidth();
	}

	private static void AddText(List<Finding> findings, Control control, float scale)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Font themeFont = control.GetThemeFont(StringName.op_Implicit("font"), (StringName)null);
		object obj = ((themeFont is FontFile) ? themeFont : null);
		if (obj == null)
		{
			Font obj2 = ((themeFont is FontVariation) ? themeFont : null);
			Font obj3 = ((obj2 != null) ? ((FontVariation)obj2).BaseFont : null);
			obj = ((obj3 is FontFile) ? obj3 : null);
		}
		FontFile val = (FontFile)obj;
		if (val != null && !val.MultichannelSignedDistanceField)
		{
			float num = ((val.Oversampling > 0f) ? val.Oversampling : 1f);
			if (num / scale < 0.95f)
			{
				findings.Add(new Finding("font", ((Resource)val).ResourcePath, NodePath.op_Implicit(((Node)control).GetPath()), Vector2.One * num, Vector2.One * scale, num / scale));
			}
		}
	}
}
