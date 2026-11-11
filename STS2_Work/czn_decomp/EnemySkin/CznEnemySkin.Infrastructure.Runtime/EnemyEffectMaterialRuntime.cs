using System;
using CznEnemySkin.Domain;
using Godot;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyEffectMaterialRuntime
{
	private static readonly StringName EffectComfortAppliedMeta = new StringName("czn_enemy_skin_effect_comfort_applied");

	private static readonly StringName NormalMaterialProperty = new StringName("normal_material");

	private static readonly StringName AdditiveMaterialProperty = new StringName("additive_material");

	private static readonly StringName MultiplyMaterialProperty = new StringName("multiply_material");

	private static readonly StringName ScreenMaterialProperty = new StringName("screen_material");

	internal const string StraightAlphaNeutralMultiplyMaterialPath = "res://CznEnemySkin/materials/straight_alpha_neutral_multiply.tres";

	private Material? _straightAlphaNeutralMultiply;

	private ShaderMaterial? _straightAlphaComfortAdditive;

	private ShaderMaterial? _opaqueBlackAdditive;

	private readonly Func<float> _intensityProvider;

	public EnemyEffectMaterialRuntime(Func<float> intensityProvider)
	{
		_intensityProvider = intensityProvider ?? throw new ArgumentNullException("intensityProvider");
	}

	public void WarmUpMaterials()
	{
		try
		{
			GetStraightAlphaComfortAdditive();
			GetOpaqueBlackAdditive();
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not warm up enemy effect materials: " + ex.Message, 1);
		}
	}

	public void RefreshIntensity()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_straightAlphaComfortAdditive != null && GodotObject.IsInstanceValid((GodotObject)(object)_straightAlphaComfortAdditive))
		{
			_straightAlphaComfortAdditive.SetShaderParameter(StringName.op_Implicit("intensity"), Variant.op_Implicit(GetComfortIntensity()));
		}
	}

	public int ApplyToBody(Node bodyRoot, bool applyNeutralMultiply = false)
	{
		EnemyNodeSet nodes = EnemyNodeTraversal.Collect(bodyRoot);
		int num = ApplyBodyAdditiveComfort(nodes);
		if (applyNeutralMultiply)
		{
			num += ApplyBodyNeutralMultiply(nodes);
		}
		return num;
	}

	public int Apply(Node effectRoot, EnemyNodeSet nodes, EnemyEffectMaterialPolicy policy)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		ApplyEffectComfort(effectRoot);
		Material val;
		switch (policy)
		{
		case EnemyEffectMaterialPolicy.Native:
			return 0;
		case EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply:
			val = GetStraightAlphaNeutralMultiply();
			break;
		case EnemyEffectMaterialPolicy.OpaqueBlackAdditive:
			val = GetOpaqueBlackAdditive();
			break;
		default:
			throw new ArgumentOutOfRangeException("policy", policy, "Unsupported enemy effect material policy.");
		}
		Material val2 = val;
		int num = 0;
		foreach (Node spineSprite in nodes.SpineSprites)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)spineSprite))
			{
				if (policy == EnemyEffectMaterialPolicy.OpaqueBlackAdditive)
				{
					((GodotObject)spineSprite).Set(NormalMaterialProperty, Variant.op_Implicit((GodotObject)(object)val2));
					((GodotObject)spineSprite).Set(AdditiveMaterialProperty, Variant.op_Implicit((GodotObject)(object)val2));
					((GodotObject)spineSprite).Set(MultiplyMaterialProperty, Variant.op_Implicit((GodotObject)(object)val2));
					((GodotObject)spineSprite).Set(ScreenMaterialProperty, Variant.op_Implicit((GodotObject)(object)val2));
				}
				else
				{
					((GodotObject)spineSprite).Set(MultiplyMaterialProperty, Variant.op_Implicit((GodotObject)(object)val2));
				}
				num++;
			}
		}
		if (num == 0)
		{
			throw new InvalidOperationException($"Enemy effect material policy '{policy}' requires " + "at least one SpineSprite.");
		}
		return num;
	}

	private int ApplyEffectComfort(Node effectRoot)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		CanvasItem val = (CanvasItem)(object)((effectRoot is CanvasItem) ? effectRoot : null);
		if (val == null || ((GodotObject)effectRoot).HasMeta(EffectComfortAppliedMeta))
		{
			return 0;
		}
		Color modulate = val.Modulate;
		val.Modulate = new Color(modulate.R, modulate.G, modulate.B, modulate.A * GetComfortIntensity());
		StringName effectComfortAppliedMeta = EffectComfortAppliedMeta;
		bool flag = true;
		((GodotObject)effectRoot).SetMeta(effectComfortAppliedMeta, Variant.From<bool>(ref flag));
		return 1;
	}

	private int ApplyBodyAdditiveComfort(EnemyNodeSet nodes)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Material straightAlphaComfortAdditive = GetStraightAlphaComfortAdditive();
		int num = 0;
		foreach (Node spineSprite in nodes.SpineSprites)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)spineSprite))
			{
				((GodotObject)spineSprite).Set(AdditiveMaterialProperty, Variant.op_Implicit((GodotObject)(object)straightAlphaComfortAdditive));
				num++;
			}
		}
		return num;
	}

	private int ApplyBodyNeutralMultiply(EnemyNodeSet nodes)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Material straightAlphaNeutralMultiply = GetStraightAlphaNeutralMultiply();
		int num = 0;
		foreach (Node spineSprite in nodes.SpineSprites)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)spineSprite))
			{
				((GodotObject)spineSprite).Set(MultiplyMaterialProperty, Variant.op_Implicit((GodotObject)(object)straightAlphaNeutralMultiply));
				num++;
			}
		}
		return num;
	}

	private Material GetStraightAlphaComfortAdditive()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (_straightAlphaComfortAdditive != null && GodotObject.IsInstanceValid((GodotObject)(object)_straightAlphaComfortAdditive))
		{
			RefreshIntensity();
			return (Material)(object)_straightAlphaComfortAdditive;
		}
		Shader shader = new Shader
		{
			Code = "shader_type canvas_item;\nrender_mode blend_add, unshaded;\nuniform float intensity = 1.0;\nvoid fragment() {\n\tvec4 tex = texture(TEXTURE, UV) * COLOR;\n\tCOLOR = vec4(tex.rgb * intensity, tex.a);\n}\n"
		};
		_straightAlphaComfortAdditive = new ShaderMaterial
		{
			Shader = shader
		};
		_straightAlphaComfortAdditive.SetShaderParameter(StringName.op_Implicit("intensity"), Variant.op_Implicit(GetComfortIntensity()));
		return (Material)(object)_straightAlphaComfortAdditive;
	}

	private float GetComfortIntensity()
	{
		return Mathf.Clamp(_intensityProvider(), 0f, 1f);
	}

	private Material GetStraightAlphaNeutralMultiply()
	{
		if (_straightAlphaNeutralMultiply != null && GodotObject.IsInstanceValid((GodotObject)(object)_straightAlphaNeutralMultiply))
		{
			return _straightAlphaNeutralMultiply;
		}
		_straightAlphaNeutralMultiply = ResourceLoader.Load<Material>("res://CznEnemySkin/materials/straight_alpha_neutral_multiply.tres", (string)null, (CacheMode)1);
		if (_straightAlphaNeutralMultiply == null)
		{
			throw new InvalidOperationException("Could not load enemy effect material: res://CznEnemySkin/materials/straight_alpha_neutral_multiply.tres");
		}
		return _straightAlphaNeutralMultiply;
	}

	private Material GetOpaqueBlackAdditive()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		if (_opaqueBlackAdditive != null && GodotObject.IsInstanceValid((GodotObject)(object)_opaqueBlackAdditive))
		{
			return (Material)(object)_opaqueBlackAdditive;
		}
		Shader shader = new Shader
		{
			Code = "shader_type canvas_item;\nrender_mode blend_add, unshaded;\nvoid fragment() {\n\tCOLOR = texture(TEXTURE, UV) * COLOR;\n}\n"
		};
		_opaqueBlackAdditive = new ShaderMaterial
		{
			Shader = shader
		};
		return (Material)(object)_opaqueBlackAdditive;
	}
}
