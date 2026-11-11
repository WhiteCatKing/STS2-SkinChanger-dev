using System;
using System.Globalization;
using Godot;

namespace CznCore.Presentation;

internal sealed class CombatEffectMaterialRuntime
{
	private readonly string _premultipliedAlphaMaterialPath;

	private readonly string? _premultipliedAlphaMultiplyMaterialPath;

	private readonly CombatEffectMaterialPolicyProfile _policy;

	private readonly float _referenceEffectBrightness;

	private readonly ICombatEffectMaterialHost _host;

	private Material? _premultipliedAlphaMaterial;

	private Material? _premultipliedAlphaAdditiveMaterial;

	private Material? _premultipliedAlphaMultiplyMaterial;

	private Material? _premultipliedAlphaScreenMaterial;

	private CombatSpineMaterialSet? _premultipliedAlphaSpineMaterials;

	private CombatSpineMaterialSet? _attackEffectSpineMaterials;

	private Material? _idleEffectAdditiveMaterial;

	private Material? _attackEffectAdditiveMaterial;

	private Material? _idleEffectSubviewportBackMaterial;

	private Material? _idleEffectSubviewportFrontMaterial;

	public CombatEffectMaterialRuntime(string premultipliedAlphaMaterialPath, CombatEffectMaterialPolicyProfile policy, float referenceEffectBrightness, ICombatEffectMaterialHost host, string? premultipliedAlphaMultiplyMaterialPath = null)
	{
		_premultipliedAlphaMaterialPath = premultipliedAlphaMaterialPath;
		_premultipliedAlphaMultiplyMaterialPath = premultipliedAlphaMultiplyMaterialPath;
		_policy = policy;
		_referenceEffectBrightness = Math.Max(0.001f, referenceEffectBrightness);
		_host = host ?? throw new ArgumentNullException("host");
	}

	public Material GetPremultipliedAlphaMaterial()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (_premultipliedAlphaMaterial != null)
		{
			return _premultipliedAlphaMaterial;
		}
		if (!string.IsNullOrWhiteSpace(_premultipliedAlphaMaterialPath))
		{
			Material val = ResourceLoader.Load<Material>(_premultipliedAlphaMaterialPath, (string)null, (CacheMode)1);
			if (val != null)
			{
				_premultipliedAlphaMaterial = val;
				return val;
			}
		}
		CanvasItemMaterial val2 = new CanvasItemMaterial();
		StringName obj = StringName.op_Implicit("blend_mode");
		int num = 4;
		((GodotObject)val2).Set(obj, Variant.From<int>(ref num));
		StringName obj2 = StringName.op_Implicit("light_mode");
		num = 0;
		((GodotObject)val2).Set(obj2, Variant.From<int>(ref num));
		_premultipliedAlphaMaterial = (Material?)(object)val2;
		return (Material)(object)val2;
	}

	public int ApplyEffectMaterial(Node root, Func<Node, Color>? modulateForNode = null, string? skipRootMeta = null)
	{
		return CombatEffectRuntime.ApplySpineMaterialsAndResetCanvas(root, GetPremultipliedAlphaSpineMaterials(), CreateEffectBrightnessModulate(modulateForNode), skipRootMeta);
	}

	public int ApplySpineOnlyEffectMaterial(Node root, string? skipRootMeta = null)
	{
		return CombatEffectRuntime.ApplySpineMaterialsOnly(root, GetPremultipliedAlphaSpineMaterials(), skipRootMeta);
	}

	public int ApplyAttackEffectMaterial(Node root, float alpha = 1f, float brightnessMultiplier = 1f)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Max(0f, brightnessMultiplier);
		Color modulate = new Color(num, num, num, Mathf.Clamp(alpha, 0f, 1f));
		return CombatEffectRuntime.ApplySpineMaterialsAndResetCanvas(root, GetAttackSpineMaterials(), CreateEffectBrightnessModulate((Node _) => modulate));
	}

	public int ApplyEffectCueMaterial(Node root, CombatPresentationEffectCue cue, Func<Node, Color>? defaultModulateForNode = null, string? defaultSkipRootMeta = null)
	{
		if (cue.UseAttackBoost)
		{
			return ApplyAttackEffectMaterial(root, cue.Alpha, cue.BrightnessMultiplier);
		}
		if (cue.UseNeutralPmaMaterial)
		{
			return ApplyEffectMaterial(root, CreateCueBrightnessModulate(cue, null));
		}
		return ApplyEffectMaterial(root, CreateCueBrightnessModulate(cue, defaultModulateForNode), defaultSkipRootMeta);
	}

	public int ApplySpineOnlyEffectCueMaterial(Node root, CombatPresentationEffectCue cue, string? defaultSkipRootMeta = null)
	{
		if (cue.UseAttackBoost)
		{
			return CombatEffectRuntime.ApplySpineMaterialsOnly(root, GetAttackSpineMaterials());
		}
		if (cue.UseNeutralPmaMaterial)
		{
			return ApplySpineOnlyEffectMaterial(root);
		}
		return ApplySpineOnlyEffectMaterial(root, defaultSkipRootMeta);
	}

	private CombatSpineMaterialSet GetPremultipliedAlphaSpineMaterials()
	{
		if (_premultipliedAlphaSpineMaterials != null)
		{
			return _premultipliedAlphaSpineMaterials;
		}
		_premultipliedAlphaSpineMaterials = new CombatSpineMaterialSet(GetPremultipliedAlphaMaterial(), GetPremultipliedAlphaAdditiveMaterial(), GetPremultipliedAlphaMultiplyMaterial(), GetPremultipliedAlphaScreenMaterial());
		return _premultipliedAlphaSpineMaterials;
	}

	private CombatSpineMaterialSet GetAttackSpineMaterials()
	{
		if (_attackEffectSpineMaterials != null)
		{
			return _attackEffectSpineMaterials;
		}
		_attackEffectSpineMaterials = new CombatSpineMaterialSet(GetPremultipliedAlphaMaterial(), GetAttackAdditiveMaterial(), GetPremultipliedAlphaMultiplyMaterial(), GetPremultipliedAlphaScreenMaterial());
		return _attackEffectSpineMaterials;
	}

	private Material GetPremultipliedAlphaMultiplyMaterial()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0058: Expected O, but got Unknown
		if (_premultipliedAlphaMultiplyMaterial != null)
		{
			return _premultipliedAlphaMultiplyMaterial;
		}
		if (!string.IsNullOrWhiteSpace(_premultipliedAlphaMultiplyMaterialPath))
		{
			Material val = ResourceLoader.Load<Material>(_premultipliedAlphaMultiplyMaterialPath, (string)null, (CacheMode)1);
			if (val != null)
			{
				_premultipliedAlphaMultiplyMaterial = val;
				return val;
			}
		}
		_premultipliedAlphaMultiplyMaterial = (Material?)new ShaderMaterial
		{
			Shader = new Shader
			{
				Code = "shader_type canvas_item;\r\nrender_mode blend_mul, unshaded;\r\nvoid fragment() {\r\n\tvec4 tex = texture(TEXTURE, UV);\r\n\tvec3 straight_rgb = vec3(1.0);\r\n\tif (tex.a > 0.0001) {\r\n\t\tstraight_rgb = clamp(COLOR.rgb / tex.a, 0.0, 1.0);\r\n\t}\r\n\tfloat coverage = clamp(COLOR.a, 0.0, 1.0);\r\n\tCOLOR = vec4(mix(vec3(1.0), straight_rgb, coverage), 1.0);\r\n}"
			}
		};
		return _premultipliedAlphaMultiplyMaterial;
	}

	public Material GetIdleAdditiveMaterial()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (_idleEffectAdditiveMaterial != null)
		{
			return _idleEffectAdditiveMaterial;
		}
		ShaderMaterial val = CreateAdditiveMaterial(new Color(1.18f, 0.95f, 1.38f, 1f), 1.35f, 1.8f, 1.85f);
		val.SetShaderParameter(StringName.op_Implicit("tint"), Variant.op_Implicit(_policy.IdleTint));
		val.SetShaderParameter(StringName.op_Implicit("brightness"), Variant.op_Implicit(ScaleBrightness(_policy.IdleBrightness)));
		val.SetShaderParameter(StringName.op_Implicit("alpha_boost"), Variant.op_Implicit(ScaleOpacity(_policy.IdleAlphaBoost)));
		val.SetShaderParameter(StringName.op_Implicit("alpha_power"), Variant.op_Implicit(_policy.IdleAlphaPower));
		_idleEffectAdditiveMaterial = (Material?)(object)val;
		return (Material)(object)val;
	}

	public Material GetAttackAdditiveMaterial()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (_attackEffectAdditiveMaterial != null)
		{
			return _attackEffectAdditiveMaterial;
		}
		ShaderMaterial val = CreateAdditiveMaterial(Colors.White, 1.4f, 1.18f, 1.15f);
		val.SetShaderParameter(StringName.op_Implicit("tint"), Variant.op_Implicit(_policy.AttackTint));
		val.SetShaderParameter(StringName.op_Implicit("brightness"), Variant.op_Implicit(ScaleBrightness(_policy.AttackBrightness)));
		val.SetShaderParameter(StringName.op_Implicit("alpha_boost"), Variant.op_Implicit(ScaleOpacity(_policy.AttackAlphaBoost)));
		val.SetShaderParameter(StringName.op_Implicit("alpha_power"), Variant.op_Implicit(_policy.AttackAlphaPower));
		_attackEffectAdditiveMaterial = (Material?)(object)val;
		return (Material)(object)val;
	}

	private Material GetPremultipliedAlphaAdditiveMaterial()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_premultipliedAlphaAdditiveMaterial != null)
		{
			return _premultipliedAlphaAdditiveMaterial;
		}
		ShaderMaterial val = CreateAdditiveMaterial(Colors.White, 1f, 1f, 1f);
		val.SetShaderParameter(StringName.op_Implicit("brightness"), Variant.op_Implicit(ScaleBrightness(1f)));
		_premultipliedAlphaAdditiveMaterial = (Material?)(object)val;
		return (Material)(object)val;
	}

	private Material GetPremultipliedAlphaScreenMaterial()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_premultipliedAlphaScreenMaterial != null)
		{
			return _premultipliedAlphaScreenMaterial;
		}
		ShaderMaterial val = CreateAdditiveMaterial(Colors.White, 1f, 1f, 1f);
		val.SetShaderParameter(StringName.op_Implicit("brightness"), Variant.op_Implicit(ScaleBrightness(1f)));
		_premultipliedAlphaScreenMaterial = (Material?)(object)val;
		return (Material)(object)val;
	}

	public Material GetIdleSubviewportMaterial(bool backLayer)
	{
		if (backLayer && _idleEffectSubviewportBackMaterial != null)
		{
			return _idleEffectSubviewportBackMaterial;
		}
		if (!backLayer && _idleEffectSubviewportFrontMaterial != null)
		{
			return _idleEffectSubviewportFrontMaterial;
		}
		ShaderMaterial val = CreateSubviewportMaterial();
		if (backLayer)
		{
			ApplySubviewportMaterialParameters(val, _policy.IdleBackViewport);
			_idleEffectSubviewportBackMaterial = (Material?)(object)val;
		}
		else
		{
			ApplySubviewportMaterialParameters(val, _policy.IdleFrontViewport);
			_idleEffectSubviewportFrontMaterial = (Material?)(object)val;
		}
		return (Material)(object)val;
	}

	public void ApplyBrightnessSettings()
	{
		SetShaderBrightness(_premultipliedAlphaAdditiveMaterial, 1f);
		SetShaderBrightness(_premultipliedAlphaScreenMaterial, 1f);
		SetShaderBrightness(_idleEffectAdditiveMaterial, _policy.IdleBrightness);
		SetShaderBrightness(_attackEffectAdditiveMaterial, _policy.AttackBrightness);
		SetShaderOpacity(_idleEffectAdditiveMaterial, _policy.IdleAlphaBoost);
		SetShaderOpacity(_attackEffectAdditiveMaterial, _policy.AttackAlphaBoost);
		SetShaderBrightness(_idleEffectSubviewportBackMaterial, _policy.IdleBackViewport.Brightness);
		SetShaderBrightness(_idleEffectSubviewportFrontMaterial, _policy.IdleFrontViewport.Brightness);
		SetSubviewportOpacity(_idleEffectSubviewportBackMaterial, _policy.IdleBackViewport.AlphaMultiplier);
		SetSubviewportOpacity(_idleEffectSubviewportFrontMaterial, _policy.IdleFrontViewport.AlphaMultiplier);
	}

	private static ShaderMaterial CreateAdditiveMaterial(Color defaultTint, float defaultBrightness, float defaultAlphaBoost, float defaultAlphaPower)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		Shader val = new Shader();
		val.Code = "shader_type canvas_item;\nrender_mode blend_add, unshaded;\nuniform vec4 tint : source_color = " + ColorDefault(defaultTint) + ";\nuniform float brightness = " + FloatDefault(defaultBrightness) + ";\nuniform float alpha_boost = " + FloatDefault(defaultAlphaBoost) + ";\nuniform float alpha_power = " + FloatDefault(defaultAlphaPower) + ";\nvoid fragment() {\n\tvec4 tex = texture(TEXTURE, UV) * COLOR;\n\tfloat alpha = max(tex.a, 0.0001);\n\tvec3 straight_rgb = clamp(tex.rgb / alpha, vec3(0.0), vec3(1.0));\n\tfloat shaped_alpha = pow(clamp(tex.a, 0.0, 1.0), alpha_power) * tint.a * alpha_boost;\n\tfloat output_alpha = clamp(shaped_alpha, 0.0, 1.0);\n\tCOLOR = vec4(straight_rgb * tint.rgb * brightness * output_alpha, output_alpha);\n}\n";
		Shader shader = val;
		return new ShaderMaterial
		{
			Shader = shader
		};
	}

	private static ShaderMaterial CreateSubviewportMaterial()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		Shader shader = new Shader
		{
			Code = "shader_type canvas_item;\nrender_mode blend_add, unshaded;\nuniform float brightness = 1.0;\nuniform float contrast = 1.0;\nuniform float saturation = 1.0;\nuniform float alpha_mul = 1.0;\nvec3 apply_saturation(vec3 color, float amount) {\n\tfloat luma = dot(color, vec3(0.2126, 0.7152, 0.0722));\n\treturn mix(vec3(luma), color, amount);\n}\nvoid fragment() {\n\tvec4 tex = texture(TEXTURE, UV);\n\tfloat alpha = clamp(tex.a * alpha_mul, 0.0, 1.0);\n\tvec3 rgb = tex.rgb;\n\trgb = (rgb - vec3(0.5)) * contrast + vec3(0.5);\n\trgb = apply_saturation(rgb, saturation) * brightness;\n\trgb = max(rgb, vec3(0.0));\n\tCOLOR = vec4(rgb, alpha);\n}\n"
		};
		return new ShaderMaterial
		{
			Shader = shader
		};
	}

	private void ApplySubviewportMaterialParameters(ShaderMaterial material, CombatEffectViewportMaterialProfile profile)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		material.SetShaderParameter(StringName.op_Implicit("brightness"), Variant.op_Implicit(ScaleBrightness(profile.Brightness)));
		material.SetShaderParameter(StringName.op_Implicit("contrast"), Variant.op_Implicit(profile.Contrast));
		material.SetShaderParameter(StringName.op_Implicit("saturation"), Variant.op_Implicit(profile.Saturation));
		material.SetShaderParameter(StringName.op_Implicit("alpha_mul"), Variant.op_Implicit(ScaleOpacity(profile.AlphaMultiplier)));
	}

	private float ScaleBrightness(float tunedValue)
	{
		return tunedValue * GetEffectBrightnessScale();
	}

	private float ScaleOpacity(float tunedValue)
	{
		return tunedValue * GetEffectOpacityScale();
	}

	private Func<Node, Color> CreateEffectBrightnessModulate(Func<Node, Color>? modulateForNode)
	{
		float brightnessScale = GetEffectBrightnessScale();
		float opacityScale = GetEffectOpacityScale();
		return (Node node) => ApplyEffectBrightnessScale((modulateForNode == null) ? Colors.White : modulateForNode(node), brightnessScale, opacityScale);
	}

	private static Func<Node, Color>? CreateCueBrightnessModulate(CombatPresentationEffectCue cue, Func<Node, Color>? modulateForNode)
	{
		if (Math.Abs(cue.BrightnessMultiplier - 1f) <= 0.001f)
		{
			return modulateForNode;
		}
		float brightness = Math.Max(0f, cue.BrightnessMultiplier);
		return delegate(Node node)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			Color val = ((modulateForNode == null) ? Colors.White : modulateForNode(node));
			return new Color(val.R * brightness, val.G * brightness, val.B * brightness, val.A);
		};
	}

	private static Color ApplyEffectBrightnessScale(Color source, float brightnessScale, float opacityScale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return new Color(source.R * brightnessScale, source.G * brightnessScale, source.B * brightnessScale, source.A * opacityScale);
	}

	private float GetEffectBrightnessScale()
	{
		return Mathf.Clamp(_host.GetEffectBrightness() / _referenceEffectBrightness, 0f, 2f);
	}

	private float GetEffectOpacityScale()
	{
		return Mathf.Clamp(_host.GetEffectOpacity(), 0f, 1f);
	}

	private void SetShaderBrightness(Material? material, float tunedValue)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			val.SetShaderParameter(StringName.op_Implicit("brightness"), Variant.op_Implicit(ScaleBrightness(tunedValue)));
		}
	}

	private void SetShaderOpacity(Material? material, float tunedValue)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			val.SetShaderParameter(StringName.op_Implicit("alpha_boost"), Variant.op_Implicit(ScaleOpacity(tunedValue)));
		}
	}

	private void SetSubviewportOpacity(Material? material, float tunedValue)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			val.SetShaderParameter(StringName.op_Implicit("alpha_mul"), Variant.op_Implicit(ScaleOpacity(tunedValue)));
		}
	}

	private static string ColorDefault(Color color)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		return "vec4(" + FloatDefault(color.R) + ", " + FloatDefault(color.G) + ", " + FloatDefault(color.B) + ", " + FloatDefault(color.A) + ")";
	}

	private static string FloatDefault(float value)
	{
		string text = value.ToString("0.###", CultureInfo.InvariantCulture);
		if (!text.Contains('.'))
		{
			return text + ".0";
		}
		return text;
	}
}
