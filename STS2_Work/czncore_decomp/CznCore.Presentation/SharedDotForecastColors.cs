using System;
using System.Runtime.CompilerServices;
using Godot;

namespace CznCore.Presentation;

internal static class SharedDotForecastColors
{
	private sealed class SurfaceState
	{
		internal Material? Original;

		internal readonly ShaderMaterial Material = new ShaderMaterial
		{
			Shader = PaletteShader
		};
	}

	internal static readonly Color Rift = new Color("FFB347");

	internal static readonly Color Pain = new Color("9678F0");

	private static readonly ConditionalWeakTable<Control, SurfaceState> States = new ConditionalWeakTable<Control, SurfaceState>();

	private static readonly Shader PaletteShader = new Shader
	{
		Code = "shader_type canvas_item;\r\nuniform float width_px = 1.0;\r\nuniform float rift_end = 0.0;\r\nuniform float pain_end = 1.0;\r\nuniform vec4 rift_color : source_color;\r\nuniform vec4 pain_color : source_color;\r\nvarying float local_x;\r\nvarying vec4 vertex_color;\r\nvoid vertex() { local_x = VERTEX.x; vertex_color = COLOR; }\r\nvoid fragment() {\r\n    vec4 tex = texture(TEXTURE, UV);\r\n    float x = clamp(local_x / max(width_px, 1.0), 0.0, 1.0);\r\n    float light = max(tex.r, max(tex.g, tex.b));\r\n    vec3 rgb = tex.rgb * vertex_color.rgb;\r\n    if (x < rift_end) rgb = rift_color.rgb * light;\r\n    else if (x < pain_end) rgb = pain_color.rgb * light;\r\n    COLOR = vec4(rgb, tex.a * vertex_color.a);\r\n}"
	};

	internal static void Apply(Control foreground, int poison, int rift, int pain)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		if (rift <= 0 && pain <= 0)
		{
			if (States.TryGetValue(foreground, out SurfaceState value))
			{
				if (((CanvasItem)foreground).Material == value.Material)
				{
					((CanvasItem)foreground).Material = value.Original;
				}
				States.Remove(foreground);
			}
			return;
		}
		SurfaceState value2 = States.GetValue(foreground, (Control _) => new SurfaceState());
		if (((CanvasItem)foreground).Material != value2.Material)
		{
			value2.Original = ((CanvasItem)foreground).Material;
		}
		double num = Math.Max(1.0, (double)poison + (double)rift + (double)pain);
		value2.Material.SetShaderParameter(StringName.op_Implicit("width_px"), Variant.op_Implicit(foreground.Size.X));
		value2.Material.SetShaderParameter(StringName.op_Implicit("rift_end"), Variant.op_Implicit((float)((double)rift / num)));
		value2.Material.SetShaderParameter(StringName.op_Implicit("pain_end"), Variant.op_Implicit((float)(((double)rift + (double)pain) / num)));
		value2.Material.SetShaderParameter(StringName.op_Implicit("rift_color"), Variant.op_Implicit(Rift));
		value2.Material.SetShaderParameter(StringName.op_Implicit("pain_color"), Variant.op_Implicit(Pain));
		((CanvasItem)foreground).Material = (Material)(object)value2.Material;
	}
}
