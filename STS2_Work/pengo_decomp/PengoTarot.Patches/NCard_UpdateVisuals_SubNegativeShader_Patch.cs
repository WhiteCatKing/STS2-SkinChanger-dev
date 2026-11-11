using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class NCard_UpdateVisuals_SubNegativeShader_Patch
{
	private static ShaderMaterial? _negativeMaterial;

	private static ShaderMaterial GetOrCreateMaterial()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (_negativeMaterial != null)
		{
			return _negativeMaterial;
		}
		Shader val = new Shader();
		val.Code = "\r\nshader_type canvas_item;\r\nrender_mode blend_mix;\r\nuniform float intensity : hint_range(0, 1) = 1.0;\r\nuniform float hue_shift : hint_range(-0.5, 0.5) = -0.15;\r\nuniform float brightness_boost : hint_range(-1, 1) = 0.2;\r\n// 前置声明 hue2rgb\r\nfloat hue2rgb(float p, float q, float t) {\r\n    if (t < 0.0) t += 1.0;\r\n    if (t > 1.0) t -= 1.0;\r\n    if (t < 1.0/6.0) return p + (q - p) * 6.0 * t;\r\n    if (t < 1.0/2.0) return q;\r\n    if (t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6.0;\r\n    return p;\r\n}\r\nvec3 rgb_to_hsl(vec3 c) {\r\n    float low = min(min(c.r, c.g), c.b);\r\n    float high = max(max(c.r, c.g), c.b);\r\n    float delta = high - low;\r\n    float sum = high + low;\r\n    float l = sum * 0.5;\r\n    float h = 0.0;\r\n    float s = 0.0;\r\n    if (delta > 0.001) {\r\n        s = l < 0.5 ? delta / sum : delta / (2.0 - sum);\r\n        if (high == c.r)\r\n            h = (c.g - c.b) / delta + (c.g < c.b ? 6.0 : 0.0);\r\n        else if (high == c.g)\r\n            h = (c.b - c.r) / delta + 2.0;\r\n        else\r\n            h = (c.r - c.g) / delta + 4.0;\r\n        h /= 6.0;\r\n    }\r\n    return vec3(h, s, l);\r\n}\r\nvec3 hsl_to_rgb(vec3 hsl) {\r\n    float h = hsl.x;\r\n    float s = hsl.y;\r\n    float l = hsl.z;\r\n    if (s < 0.001) return vec3(l);\r\n    float q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;\r\n    float p = 2.0 * l - q;\r\n    return vec3(\r\n        hue2rgb(p, q, h + 1.0/3.0),\r\n        hue2rgb(p, q, h),\r\n        hue2rgb(p, q, h - 1.0/3.0)\r\n    );\r\n}\r\nvoid fragment() {\r\n    vec4 tex = texture(TEXTURE, UV);\r\n    if (tex.a < 0.01) discard;\r\n    vec3 hsl = rgb_to_hsl(tex.rgb);\r\n    hsl.b = 1.0 - hsl.b;\r\n    hsl.r = mod(hsl.r + hue_shift, 1.0);\r\n    hsl.b = clamp(hsl.b + brightness_boost, 0.0, 1.0);\r\n    vec3 neg_color = hsl_to_rgb(hsl);\r\n    float highlight = smoothstep(0.6, 1.0, hsl.b) * 0.15;\r\n    neg_color += vec3(0.2, 0.4, 0.9) * highlight;\r\n    COLOR = vec4(mix(tex.rgb, neg_color, intensity), tex.a);\r\n}\r\n";
		_negativeMaterial = new ShaderMaterial();
		_negativeMaterial.Shader = val;
		return _negativeMaterial;
	}

	private static bool ShouldApplyNegativeShader(CardModel card)
	{
		if (card == null)
		{
			return false;
		}
		if (card.Pool is TarotPool && ((AbstractModel)card).Id.Entry.ToLowerInvariant().Contains("sub"))
		{
			return true;
		}
		if (card.Enchantment != null)
		{
			string? @namespace = ((object)card.Enchantment).GetType().Namespace;
			if (@namespace != null && @namespace.StartsWith("PengoTarot") && ((AbstractModel)card.Enchantment).Id.Entry.ToLowerInvariant().Contains("sub"))
			{
				return true;
			}
		}
		return false;
	}

	private static void Postfix(NCard __instance, PileType pileType, CardPreviewMode previewMode)
	{
		if (__instance.Model == null)
		{
			return;
		}
		bool flag = ShouldApplyNegativeShader(__instance.Model);
		ShaderMaterial orCreateMaterial = GetOrCreateMaterial();
		Node2D val = ((Node)__instance).GetNodeOrNull<Node2D>(NodePath.op_Implicit("Portrait")) ?? ((Node)__instance).GetNodeOrNull<Node2D>(NodePath.op_Implicit("CardImage"));
		if (val == null)
		{
			return;
		}
		if (flag)
		{
			if (((CanvasItem)val).Material != orCreateMaterial)
			{
				((CanvasItem)val).Material = (Material)(object)orCreateMaterial;
			}
		}
		else if (((CanvasItem)val).Material == orCreateMaterial)
		{
			((CanvasItem)val).Material = null;
		}
	}
}
