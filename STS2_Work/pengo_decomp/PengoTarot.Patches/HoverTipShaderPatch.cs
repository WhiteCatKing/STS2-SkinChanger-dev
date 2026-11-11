using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NHoverTipSet), "Init")]
internal static class HoverTipShaderPatch
{
	private static readonly ConditionalWeakTable<NHoverTipSet, List<bool>> GoldTextTipMasks = new ConditionalWeakTable<NHoverTipSet, List<bool>>();

	private static readonly Lazy<ShaderMaterial> ColorizeMaterial = new Lazy<ShaderMaterial>((Func<ShaderMaterial>)CreateColorizeMaterial);

	private static readonly Lazy<ShaderMaterial> StarcloudMaterial = new Lazy<ShaderMaterial>((Func<ShaderMaterial>)delegate
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Shader val = ResourceLoader.Load<Shader>("res://balatroeffect/Shaders/pengo_starcloud.gdshader", (string)null, (CacheMode)1);
		if (val != null)
		{
			ShaderMaterial val2 = new ShaderMaterial
			{
				Shader = val
			};
			val2.SetShaderParameter(StringName.op_Implicit("star_size"), Variant.op_Implicit(3.6f));
			val2.SetShaderParameter(StringName.op_Implicit("star_softness"), Variant.op_Implicit(0.08f));
			val2.SetShaderParameter(StringName.op_Implicit("fbm_mix"), Variant.op_Implicit(0f));
			val2.SetShaderParameter(StringName.op_Implicit("fbm_strength"), Variant.op_Implicit(0f));
			val2.SetShaderParameter(StringName.op_Implicit("cloud_amp"), Variant.op_Implicit(0f));
			return val2;
		}
		Log.Error("[PengoTarot] Failed to load starcloud shader!", 2);
		return new ShaderMaterial();
	});

	internal static void Prefix(NHoverTipSet __instance, ref IEnumerable<IHoverTip> hoverTips)
	{
		List<IHoverTip> list = (List<IHoverTip>)(hoverTips = hoverTips?.Where((IHoverTip tip) => tip != null).ToList() ?? new List<IHoverTip>());
		if (list.Count != 0)
		{
			List<bool> list2 = (from tip in IHoverTip.RemoveDupes((IEnumerable<IHoverTip>)list).OfType<HoverTip>().ToList()
				select IsPengoTarotTip(tip)).ToList();
			if (list2.Any((bool isGold) => isGold))
			{
				GoldTextTipMasks.AddOrUpdate(__instance, list2);
			}
		}
	}

	internal static void Postfix(NHoverTipSet __instance)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Expected O, but got Unknown
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		if (!GoldTextTipMasks.TryGetValue(__instance, out List<bool> value))
		{
			return;
		}
		GoldTextTipMasks.Remove(__instance);
		VFlowContainer value2 = Traverse.Create((object)__instance).Field("_textHoverTipContainer").GetValue<VFlowContainer>();
		if (value2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)value2))
		{
			return;
		}
		int num = 0;
		foreach (var item in ((IEnumerable)((Node)value2).GetChildren(false)).OfType<Control>().Zip(value))
		{
			var (val, _) = item;
			if (!item.Second || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				continue;
			}
			Control node = ((Node)val).GetNode<Control>(NodePath.op_Implicit("%Bg"));
			if (node != null)
			{
				if (StarcloudMaterial.Value.Shader != null && ((Node)val).GetNodeOrNull(NodePath.op_Implicit("StarOverlay")) == null)
				{
					Random random = new Random();
					StarcloudMaterial.Value.SetShaderParameter(StringName.op_Implicit("uv_offset"), Variant.op_Implicit(new Vector2((float)random.NextDouble() * 100f, (float)random.NextDouble() * 100f)));
					StarcloudMaterial.Value.SetShaderParameter(StringName.op_Implicit("density"), Variant.op_Implicit(120f));
					Vector2 size = val.Size;
					float num2 = Mathf.Max(size.X, size.Y);
					StarcloudMaterial.Value.SetShaderParameter(StringName.op_Implicit("uv_scale"), Variant.op_Implicit(new Vector2(size.X / num2, size.Y / num2)));
					SubViewport val2 = new SubViewport
					{
						Name = StringName.op_Implicit("StarViewport"),
						TransparentBg = true,
						Disable3D = true,
						Size = (Vector2I)(size * 2f)
					};
					SubViewportContainer val3 = new SubViewportContainer
					{
						Name = StringName.op_Implicit("StarOverlay"),
						Stretch = true,
						MouseFilter = (MouseFilterEnum)2,
						Material = (Material)(object)StarcloudMaterial.Value
					};
					((Control)val3).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
					((Node)val3).AddChild((Node)(object)val2, false, (InternalMode)0);
					((Node)val).RemoveChild((Node)(object)node);
					((Node)val2).AddChild((Node)(object)node, false, (InternalMode)0);
					node.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
					((CanvasItem)node).Material = (Material)(object)ColorizeMaterial.Value;
					((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
					((Node)val).MoveChild((Node)(object)val3, ((Node)val).GetNode(NodePath.op_Implicit("TextContainer")).GetIndex(false));
				}
				else
				{
					((CanvasItem)node).Material = (Material)(object)ColorizeMaterial.Value;
				}
				num++;
			}
		}
	}

	internal static bool IsPengoTarotTip(HoverTip tip)
	{
		if (((HoverTip)(ref tip)).CanonicalModel != null)
		{
			return ((object)((HoverTip)(ref tip)).CanonicalModel).GetType().Namespace?.StartsWith("PengoTarot") ?? false;
		}
		return false;
	}

	internal static ShaderMaterial CreateColorizeMaterial()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		Shader shader = new Shader
		{
			Code = "\r\nshader_type canvas_item;\r\n\r\nuniform float colorize_hue : hint_range(0.0, 1.0) = 0.611;\r\nuniform float colorize_saturation : hint_range(0.0, 2.0) = 0.4;\r\nuniform float colorize_lightness : hint_range(-0.5, 0.5) = 0.0;\r\n\r\nvec3 rgb_to_hsl(vec3 c) {\r\n    float maxC = max(max(c.r, c.g), c.b);\r\n    float minC = min(min(c.r, c.g), c.b);\r\n    float delta = maxC - minC;\r\n    float l = (maxC + minC) * 0.5;\r\n    float s = 0.0;\r\n    float h = 0.0;\r\n    if (delta > 0.0001) {\r\n        s = (l < 0.5) ? delta / (maxC + minC) : delta / (2.0 - maxC - minC);\r\n        if (c.r == maxC)\r\n            h = (c.g - c.b) / delta + (c.g < c.b ? 6.0 : 0.0);\r\n        else if (c.g == maxC)\r\n            h = (c.b - c.r) / delta + 2.0;\r\n        else\r\n            h = (c.r - c.g) / delta + 4.0;\r\n        h /= 6.0;\r\n    }\r\n    return vec3(h, s, l);\r\n}\r\n\r\nfloat hue_to_rgb(float p, float q, float t) {\r\n    if (t < 0.0) t += 1.0;\r\n    if (t > 1.0) t -= 1.0;\r\n    if (t < 1.0/6.0) return p + (q - p) * 6.0 * t;\r\n    if (t < 0.5) return q;\r\n    if (t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6.0;\r\n    return p;\r\n}\r\n\r\nvec3 hsl_to_rgb(vec3 hsl) {\r\n    if (hsl.y < 0.0001) return vec3(hsl.z);\r\n    float q = hsl.z < 0.5 ? hsl.z * (1.0 + hsl.y) : hsl.z + hsl.y - hsl.z * hsl.y;\r\n    float p = 2.0 * hsl.z - q;\r\n    return vec3(\r\n        hue_to_rgb(p, q, hsl.x + 1.0/3.0),\r\n        hue_to_rgb(p, q, hsl.x),\r\n        hue_to_rgb(p, q, hsl.x - 1.0/3.0)\r\n    );\r\n}\r\n\r\nvoid fragment() {\r\n    COLOR = texture(TEXTURE, UV);\r\n    if (COLOR.a < 0.01) discard;\r\n    vec3 hsl = rgb_to_hsl(COLOR.rgb);\r\n    hsl.x = colorize_hue;\r\n    hsl.y = colorize_saturation;\r\n    hsl.z = clamp(hsl.z + colorize_lightness, 0.0, 1.0);\r\n    COLOR.rgb = hsl_to_rgb(hsl);\r\n}"
		};
		return new ShaderMaterial
		{
			Shader = shader
		};
	}
}
