using System;
using System.Globalization;
using Godot;

namespace CznCore.Presentation;

internal static class CombatRadialRgbBlurRuntime
{
	private static readonly float[] GlowProfile = new float[17]
	{
		0.9922f, 0.9608f, 0.9117f, 0.8547f, 0.7887f, 0.7179f, 0.6417f, 0.5631f, 0.4827f, 0.4033f,
		0.3255f, 0.2511f, 0.1819f, 0.1195f, 0.0652f, 0.0208f, 0f
	};

	private static readonly Shader BlurShader = new Shader
	{
		Code = "shader_type canvas_item;\r\nrender_mode unshaded;\r\nuniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear_mipmap;\r\nuniform float force = 0.3;\r\nuniform float force2 = -1.0;\r\nuniform float strength = 1.0;\r\nuniform float sampling = 10.0;\r\nuniform bool glow_mask = false;\r\nuniform float glow[17];\r\nfloat mask(vec2 uv) {\r\n\tif (!glow_mask) return 1.0;\r\n\tfloat r = clamp(length(uv - vec2(0.5)) * 2.0, 0.0, 1.0) * 16.0;\r\n\tint i = int(floor(r));\r\n\treturn i >= 16 ? glow[16] : mix(glow[i], glow[i + 1], r - float(i));\r\n}\r\nvoid fragment() {\r\n\tvec2 radial = UV - vec2(0.5);\r\n\tvec4 source = texture(screen_texture, SCREEN_UV);\r\n\tvec3 accumulated = vec3(0.0);\r\n\tfloat count = max(1.0, sampling);\r\n\tfor (int index = 0; index < 10; index++) {\r\n\t\tfloat enabled = step(float(index), count - 1.0);\r\n\t\tfloat progress = count <= 1.0 ? 0.0 : float(index) / (count - 1.0);\r\n\t\tvec2 offset = radial * force * 0.02 * progress;\r\n\t\taccumulated.r += texture(screen_texture, SCREEN_UV + offset).r * enabled;\r\n\t\taccumulated.g += texture(screen_texture, SCREEN_UV).g * enabled;\r\n\t\taccumulated.b += texture(screen_texture, SCREEN_UV + offset * force2).b * enabled;\r\n\t}\r\n\tvec3 blurred = accumulated / count;\r\n\tCOLOR = vec4(mix(source.rgb, blurred, clamp(strength, 0.0, 1.0) * mask(UV)), source.a);\r\n}"
	};

	public static Node2D? Play(Node2D parent, Vector2 localPosition, Vector2 scale, CombatRadialRgbBlurSpec spec, int zIndex = 1)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Expected O, but got Unknown
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)parent) || !((Node)parent).IsInsideTree())
		{
			return null;
		}
		Vector2 val = spec.MaskSize * 0.5f;
		Gradient val2 = new Gradient();
		val2.Colors = (Color[])(object)new Color[2]
		{
			Colors.White,
			Colors.White
		};
		Gradient gradient = val2;
		ShaderMaterial material = new ShaderMaterial
		{
			Shader = BlurShader
		};
		material.SetShaderParameter(StringName.op_Implicit("force"), Variant.op_Implicit(spec.Force));
		material.SetShaderParameter(StringName.op_Implicit("force2"), Variant.op_Implicit(spec.Force2));
		material.SetShaderParameter(StringName.op_Implicit("sampling"), Variant.op_Implicit(spec.Sampling));
		material.SetShaderParameter(StringName.op_Implicit("glow_mask"), Variant.op_Implicit(spec.GlowMask));
		material.SetShaderParameter(StringName.op_Implicit("glow"), Variant.op_Implicit(GlowProfile));
		material.SetShaderParameter(StringName.op_Implicit("strength"), Variant.op_Implicit(0f));
		Polygon2D val3 = new Polygon2D();
		((Node)val3).Name = StringName.op_Implicit("CznRadialRgbBlur");
		val3.Polygon = (Vector2[])(object)new Vector2[4]
		{
			-val,
			new Vector2(val.X, 0f - val.Y),
			val,
			new Vector2(0f - val.X, val.Y)
		};
		val3.UV = (Vector2[])(object)new Vector2[4]
		{
			Vector2.Zero,
			Vector2.Right,
			Vector2.One,
			Vector2.Down
		};
		val3.Texture = (Texture2D)new GradientTexture1D
		{
			Gradient = gradient,
			Width = 2
		};
		((Node2D)val3).Position = localPosition;
		((Node2D)val3).Scale = scale;
		((CanvasItem)val3).ZIndex = zIndex;
		((CanvasItem)val3).Material = (Material)(object)material;
		Polygon2D val4 = val3;
		((Node)parent).AddChild((Node)(object)val4, false, (InternalMode)0);
		Tween val5 = ((Node)val4).CreateTween();
		val5.TweenMethod(Callable.From<float>((Action<float>)delegate(float p)
		{
			SetStrength(material, spec.RiseCurve, 0f, 1f, p);
		}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)MathF.Max(0.001f, spec.RiseSeconds));
		if (spec.HoldSeconds > 0f)
		{
			val5.TweenInterval((double)spec.HoldSeconds);
		}
		val5.TweenMethod(Callable.From<float>((Action<float>)delegate(float p)
		{
			SetStrength(material, spec.FallCurve, 1f, spec.EndValue, p);
		}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), (double)MathF.Max(0.001f, spec.FallSeconds));
		val5.TweenCallback(Callable.From((Action)((Node)val4).QueueFree));
		return (Node2D?)(object)val4;
	}

	private static void SetStrength(ShaderMaterial material, string curve, float from, float to, float progress)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		material.SetShaderParameter(StringName.op_Implicit("strength"), Variant.op_Implicit(Mathf.Lerp(from, to, Ease(curve, progress))));
	}

	internal static float Ease(string curve, float progress)
	{
		string[] array = curve.Split(',');
		if (array.Length != 4)
		{
			return progress;
		}
		float a = float.Parse(array[0], CultureInfo.InvariantCulture);
		float a2 = float.Parse(array[1], CultureInfo.InvariantCulture);
		float b = float.Parse(array[2], CultureInfo.InvariantCulture);
		float b2 = float.Parse(array[3], CultureInfo.InvariantCulture);
		float num = 0f;
		float num2 = 1f;
		float num3 = progress;
		for (int i = 0; i < 20; i++)
		{
			num3 = (num + num2) * 0.5f;
			if (Bezier(num3, a, b) < progress)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		return Bezier(num3, a2, b2);
	}

	private static float Bezier(float s, float a, float b)
	{
		return 3f * (1f - s) * (1f - s) * s * a + 3f * (1f - s) * s * s * b + s * s * s;
	}
}
