using System.Collections.Generic;
using Godot;

namespace PengoTarot.BalatroEffect;

public static class EffectRegistry
{
	public record EffectDef(int Mode, Shader? Shader, string LocKey, bool Aligns = false);

	private static readonly List<EffectDef> _allEffects = new List<EffectDef>();

	private static readonly Dictionary<int, EffectDef> _byMode = new Dictionary<int, EffectDef>();

	public static IReadOnlyList<EffectDef> AllEffects => _allEffects;

	public static IReadOnlyDictionary<int, EffectDef> ByMode => _byMode;

	public static Shader? GetShader(int mode)
	{
		if (!_byMode.TryGetValue(mode, out EffectDef value))
		{
			return null;
		}
		return value.Shader;
	}

	public static void Initialize()
	{
		if (_allEffects.Count <= 0)
		{
			Register(0, null, "OPTION_NONE");
			Register(1, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_FOIL");
			Register(2, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_FOIL_ALT");
			Register(3, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_POLYCHROME");
			Register(4, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_HOLOGRAPHIC");
			Register(5, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_NEGATIVE");
			Register(6, LoadShader("res://balatroeffect/Shaders/balatro_effects_parts.gdshader"), "OPTION_NEGATIVE_BLUE");
			Register(7, LoadShader("res://balatroeffect/Shaders/pengo_aniso_rainbow.gdshader"), "OPTION_ANISO_FIXED", aligns: true);
			Register(8, LoadShader("res://balatroeffect/Shaders/pengo_aniso_rainbow.gdshader"), "OPTION_ANISO_STRIPE", aligns: true);
			Register(9, LoadShader("res://balatroeffect/Shaders/pengo_aniso_rainbow.gdshader"), "OPTION_ANISO_DUAL", aligns: true);
			Register(10, LoadShader("res://balatroeffect/Shaders/pengo_vhs.gdshader"), "OPTION_VHS", aligns: true);
			Register(11, LoadShader("res://balatroeffect/Shaders/pengo_crt.gdshader"), "OPTION_CRT");
			Register(12, LoadShader("res://balatroeffect/Shaders/pengo_vhs2.gdshader"), "OPTION_VHS2");
			Register(13, LoadShader("res://balatroeffect/Shaders/pengo_sweep.gdshader"), "OPTION_SWEEP");
			Register(15, LoadShader("res://balatroeffect/Shaders/pengo_hover_glow.gdshader"), "OPTION_HOVER_GLOW", aligns: true);
			Register(16, LoadShader("res://balatroeffect/Shaders/pengo_glitter.gdshader"), "OPTION_GLITTER", aligns: true);
			Register(17, LoadShader("res://balatroeffect/Shaders/pengo_aurora_overlay.gdshader"), "OPTION_AURORA", aligns: true);
			Register(18, LoadShader("res://balatroeffect/Shaders/pengo_pixelate.gdshader"), "OPTION_PIXELATE");
			Register(19, LoadShader("res://balatroeffect/Shaders/pengo_outline.gdshader"), "OPTION_OUTLINE");
			Register(20, LoadShader("res://balatroeffect/Shaders/pengo_starcloud.gdshader"), "OPTION_STARCLOUD", aligns: true);
			Register(21, LoadShader("res://balatroeffect/Shaders/pengo_randomstars.gdshader"), "OPTION_RANDOMSTARS", aligns: true);
		}
	}

	private static void Register(int mode, Shader? shader, string locKey, bool aligns = false)
	{
		EffectDef effectDef = new EffectDef(mode, shader, locKey, aligns);
		_allEffects.Add(effectDef);
		_byMode[mode] = effectDef;
	}

	private static Shader? LoadShader(string path)
	{
		if (FileAccess.FileExists(path))
		{
			return GD.Load<Shader>(path);
		}
		GD.PrintErr("[EffectRegistry] Shader not found: " + path);
		return null;
	}
}
