using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznCore.Presentation;

internal static class CombatEffectRuntime
{
	private const string SourceRootModulateMeta = "czn_effect_settings_source_modulate";

	public const string ParticleStartDelayMeta = "czn_particle_start_delay_seconds";

	public const string ParticleEmissionDurationMeta = "czn_particle_emission_duration_seconds";

	private const string ParticleStartTokenMeta = "czn_particle_start_token";

	private static readonly ConditionalWeakTable<Node, MegaSprite> MegaSpriteCache = new ConditionalWeakTable<Node, MegaSprite>();

	private static readonly ConditionalWeakTable<Node, CombatEffectNodeSet> EffectNodeCaches = new ConditionalWeakTable<Node, CombatEffectNodeSet>();

	private static readonly string[] SpineMaterialProperties = new string[4] { "normal_material", "additive_material", "multiply_material", "screen_material" };

	public static bool HasRootVisualSettings(CanvasItem root)
	{
		return ((GodotObject)root).HasMeta(StringName.op_Implicit("czn_effect_settings_source_modulate"));
	}

	public static void ApplyRootVisualSettings(CanvasItem root, float brightness, float opacity)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)root).HasMeta(StringName.op_Implicit("czn_effect_settings_source_modulate")))
		{
			((GodotObject)root).SetMeta(StringName.op_Implicit("czn_effect_settings_source_modulate"), Variant.op_Implicit(root.Modulate));
		}
		StringName obj = StringName.op_Implicit("czn_effect_settings_source_modulate");
		Variant val = default(Variant);
		val = ((GodotObject)root).GetMeta(obj, val);
		Color val2 = ((Variant)(ref val)).AsColor();
		root.Modulate = new Color(val2.R * brightness, val2.G * brightness, val2.B * brightness, val2.A * opacity);
	}

	public static void PrimeNodeCaches(Node root)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)root))
		{
			EffectNodeCaches.Remove(root);
			EffectNodeCaches.Add(root, BuildEffectNodeCache(root));
		}
	}

	public static Node2D? DetachLayer(Node sourceRoot, string childName, Color modulate, string behindParentLayerName)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		Node nodeOrNull = sourceRoot.GetNodeOrNull(NodePath.op_Implicit(childName));
		Node2D val = (Node2D)(object)((nodeOrNull is Node2D) ? nodeOrNull : null);
		if (val == null)
		{
			return null;
		}
		sourceRoot.RemoveChild((Node)(object)val);
		ResetLayerTransform(val, IsBehindParentLayer(childName, behindParentLayerName), modulate);
		return val;
	}

	public static int AttachLayersByOriginalZ(Node2D effectRoot, Node sourceRoot, Color modulate, string behindParentLayerName)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		List<(Node2D, int)> list = new List<(Node2D, int)>();
		foreach (Node child in sourceRoot.GetChildren(false))
		{
			Node2D val = (Node2D)(object)((child is Node2D) ? child : null);
			if (val != null)
			{
				sourceRoot.RemoveChild((Node)(object)val);
				int zIndex = ((CanvasItem)val).ZIndex;
				ResetLayerTransform(val, IsBehindParentLayer(((object)((Node)val).Name).ToString(), behindParentLayerName), modulate);
				list.Add((val, zIndex));
			}
		}
		list.Sort(((Node2D Layer, int OriginalZIndex) a, (Node2D Layer, int OriginalZIndex) b) => a.OriginalZIndex.CompareTo(b.OriginalZIndex));
		foreach (var item2 in list)
		{
			Node2D item = item2.Item1;
			((Node)effectRoot).AddChild((Node)(object)item, false, (InternalMode)0);
			num++;
		}
		return num;
	}

	public static int StartSpineAnimation(Node effectRoot, string animationName, bool loop, float timeScale = 1f, Action<string>? warn = null)
	{
		int num = 0;
		foreach (Node spineSprite in GetSpineSprites(effectRoot))
		{
			num += StartSpineAnimationNode(spineSprite, animationName, loop, timeScale, warn);
		}
		return num;
	}

	public static int StartSpineAnimationsFromMetadata(Node effectRoot, string metadataKey = "czn_spine_animation_name", string fallbackAnimationName = "animation", bool loop = false, float timeScale = 1f, Action<string>? warn = null)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (Node spineSprite in GetSpineSprites(effectRoot))
		{
			string text;
			if (!((GodotObject)spineSprite).HasMeta(StringName.op_Implicit(metadataKey)))
			{
				text = fallbackAnimationName;
			}
			else
			{
				StringName obj = StringName.op_Implicit(metadataKey);
				Variant val = default(Variant);
				val = ((GodotObject)spineSprite).GetMeta(obj, val);
				text = ((Variant)(ref val)).AsString();
			}
			string text2 = text;
			if (string.IsNullOrWhiteSpace(text2))
			{
				text2 = fallbackAnimationName;
			}
			num += StartSpineAnimationNode(spineSprite, text2, loop, timeScale, warn);
		}
		return num;
	}

	public static void ApplyAuthoredRootTransform(Node2D root, Vector2 positionOffset, Vector2 scaleMultiplier, float rotationOffsetRadians = 0f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		root.Position += positionOffset;
		root.Scale = new Vector2(root.Scale.X * scaleMultiplier.X, root.Scale.Y * scaleMultiplier.Y);
		root.Rotation += rotationOffsetRadians;
	}

	private static int StartSpineAnimationNode(Node effectSpine, string animationName, bool loop, float timeScale, Action<string>? warn)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)effectSpine))
		{
			return 0;
		}
		try
		{
			MegaAnimationState animationState = GetMegaSprite(effectSpine).GetAnimationState();
			if (animationState == null)
			{
				return 0;
			}
			MegaTrackEntry val = SpineAnimationCompat.SetAnimationTracked(animationState, animationName, loop);
			try
			{
				if (val != null && Math.Abs(timeScale - 1f) > 0.001f)
				{
					val.SetTimeScale(timeScale);
				}
			}
			finally
			{
				SpineAnimationCompat.ReleaseTracked(val);
			}
			return 1;
		}
		catch (Exception ex)
		{
			warn?.Invoke(StringName.op_Implicit(effectSpine.Name) + " animation=" + animationName + " " + SkinLog.FormatException(ex));
			return 0;
		}
	}

	public static int StartParticles(Node effectRoot, Action<string>? warn = null)
	{
		int num = 0;
		foreach (Node particleNode in GetParticleNodes(effectRoot))
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)particleNode))
			{
				continue;
			}
			try
			{
				int token = NextParticleStartToken(particleNode);
				float particleStartDelaySeconds = GetParticleStartDelaySeconds(particleNode);
				if (particleStartDelaySeconds <= 0.001f)
				{
					StartParticleNode(particleNode, token, warn);
				}
				else
				{
					GodotScheduler.Delay(particleStartDelaySeconds, delegate
					{
						if (GodotObject.IsInstanceValid((GodotObject)(object)particleNode) && IsParticleStartTokenCurrent(particleNode, token))
						{
							StartParticleNode(particleNode, token, warn);
						}
					});
				}
				num++;
			}
			catch (Exception ex)
			{
				warn?.Invoke(StringName.op_Implicit(particleNode.Name) + " " + SkinLog.FormatException(ex));
			}
		}
		return num;
	}

	public static int StopParticles(Node effectRoot)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (Node particleNode in GetParticleNodes(effectRoot))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)particleNode))
			{
				try
				{
					NextParticleStartToken(particleNode);
					StringName obj = StringName.op_Implicit("emitting");
					bool flag = false;
					((GodotObject)particleNode).Set(obj, Variant.From<bool>(ref flag));
					num++;
				}
				catch
				{
				}
			}
		}
		return num;
	}

	private static MegaSprite GetMegaSprite(Node spineNode)
	{
		return MegaSpriteCache.GetValue(spineNode, (Node node) => new MegaSprite(Variant.op_Implicit((GodotObject)(object)node)));
	}

	public static int ApplyCanvasMaterialRecursive(Node root, Material material)
	{
		int num = 0;
		foreach (CanvasItem canvasItem in GetCanvasItems(root))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)canvasItem))
			{
				canvasItem.Material = material;
				canvasItem.UseParentMaterial = false;
				num++;
			}
		}
		return num;
	}

	public static int ApplyCanvasAndSpineMaterials(Node root, Material material)
	{
		int result = ApplyCanvasMaterialRecursive(root, material);
		foreach (Node spineSprite in GetSpineSprites(root))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)spineSprite))
			{
				SetAllSpineMaterials(spineSprite, material);
			}
		}
		return result;
	}

	public static void SetSpineMaterial(Node spineNode, string property, Material material)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		string text = "set_" + property;
		if (((GodotObject)spineNode).HasMethod(StringName.op_Implicit(text)))
		{
			((GodotObject)spineNode).Call(StringName.op_Implicit(text), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)material) });
			return;
		}
		try
		{
			((GodotObject)spineNode).Set(StringName.op_Implicit(property), Variant.From<Material>(ref material));
		}
		catch
		{
		}
	}

	public static int ApplySpineMaterialAndResetCanvas(Node root, Material material, Func<Node, Color>? modulateForNode, string? skipRootMeta = null)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(skipRootMeta) && ((GodotObject)root).HasMeta(StringName.op_Implicit(skipRootMeta)))
		{
			return 0;
		}
		int num = 0;
		foreach (Node node in GetNodes(root))
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)node))
			{
				continue;
			}
			string @class = ((GodotObject)node).GetClass();
			bool num2 = IsParticleClass(@class);
			if (@class == "SpineSprite")
			{
				SetAllSpineMaterials(node, material);
				num++;
			}
			if (num2)
			{
				continue;
			}
			CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
			if (val != null)
			{
				ResetSpineCanvasOverride(val, @class);
				if (modulateForNode != null)
				{
					val.Modulate = modulateForNode(node);
				}
			}
		}
		return num;
	}

	public static int ApplySpineMaterialsAndResetCanvas(Node root, CombatSpineMaterialSet materials, Func<Node, Color>? modulateForNode, string? skipRootMeta = null)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(skipRootMeta) && ((GodotObject)root).HasMeta(StringName.op_Implicit(skipRootMeta)))
		{
			return 0;
		}
		int num = 0;
		foreach (Node node in GetNodes(root))
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)node))
			{
				continue;
			}
			string @class = ((GodotObject)node).GetClass();
			bool num2 = IsParticleClass(@class);
			if (@class == "SpineSprite")
			{
				SetAllSpineMaterials(node, materials);
				num++;
			}
			if (num2)
			{
				continue;
			}
			CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
			if (val != null)
			{
				ResetSpineCanvasOverride(val, @class);
				if (modulateForNode != null)
				{
					val.Modulate = modulateForNode(node);
				}
			}
		}
		return num;
	}

	public static int ApplySpineMaterialOnly(Node root, Material material, string? skipRootMeta = null)
	{
		if (!string.IsNullOrEmpty(skipRootMeta) && ((GodotObject)root).HasMeta(StringName.op_Implicit(skipRootMeta)))
		{
			return 0;
		}
		int num = 0;
		foreach (Node spineSprite in GetSpineSprites(root))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)spineSprite))
			{
				SetAllSpineMaterials(spineSprite, material);
				num++;
			}
		}
		return num;
	}

	private static void ResetSpineCanvasOverride(CanvasItem item, string className)
	{
		if (!(className != "SpineSprite"))
		{
			item.Material = null;
			item.UseParentMaterial = false;
		}
	}

	public static int ApplySpineMaterialsOnly(Node root, CombatSpineMaterialSet materials, string? skipRootMeta = null)
	{
		if (!string.IsNullOrEmpty(skipRootMeta) && ((GodotObject)root).HasMeta(StringName.op_Implicit(skipRootMeta)))
		{
			return 0;
		}
		int num = 0;
		foreach (Node spineSprite in GetSpineSprites(root))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)spineSprite))
			{
				SetAllSpineMaterials(spineSprite, materials);
				num++;
			}
		}
		return num;
	}

	public static void ApplyCanvasMaterialAndModulate(Node root, Material material, Color modulate)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		foreach (CanvasItem canvasItem in GetCanvasItems(root))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)canvasItem))
			{
				canvasItem.Material = material;
				canvasItem.UseParentMaterial = false;
				canvasItem.Modulate = modulate;
			}
		}
	}

	private static void SetAllSpineMaterials(Node spineNode, Material material)
	{
		string[] spineMaterialProperties = SpineMaterialProperties;
		foreach (string property in spineMaterialProperties)
		{
			SetSpineMaterial(spineNode, property, material);
		}
	}

	private static void SetAllSpineMaterials(Node spineNode, CombatSpineMaterialSet materials)
	{
		SetSpineMaterial(spineNode, "normal_material", materials.Normal);
		SetSpineMaterial(spineNode, "additive_material", materials.Additive);
		SetSpineMaterial(spineNode, "multiply_material", materials.Multiply);
		SetSpineMaterial(spineNode, "screen_material", materials.Screen);
	}

	private static void ResetLayerTransform(Node2D layer, bool showBehindParent, Color modulate)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		layer.Position = Vector2.Zero;
		layer.Scale = Vector2.One;
		layer.Rotation = 0f;
		((CanvasItem)layer).TopLevel = false;
		((CanvasItem)layer).ZAsRelative = true;
		((CanvasItem)layer).ZIndex = ((!showBehindParent) ? 1 : 0);
		((CanvasItem)layer).ShowBehindParent = showBehindParent;
		if (layer != null)
		{
			((CanvasItem)layer).Material = null;
			((CanvasItem)layer).UseParentMaterial = false;
			((CanvasItem)layer).Modulate = modulate;
		}
	}

	private static bool IsBehindParentLayer(string layerName, string behindParentLayerName)
	{
		if (!string.IsNullOrEmpty(behindParentLayerName))
		{
			return string.Equals(layerName, behindParentLayerName, StringComparison.Ordinal);
		}
		return false;
	}

	private static IEnumerable<Node> TraverseDepthFirst(Node root)
	{
		return GodotNodeUtil.TraverseDepthFirst(root);
	}

	private static IReadOnlyList<Node> GetNodes(Node root)
	{
		if (EffectNodeCaches.TryGetValue(root, out CombatEffectNodeSet value))
		{
			return value.Nodes;
		}
		return FindNodesLive(root);
	}

	private static IReadOnlyList<Node> GetSpineSprites(Node root)
	{
		if (EffectNodeCaches.TryGetValue(root, out CombatEffectNodeSet value))
		{
			return value.SpineSprites;
		}
		return FindSpineSpritesLive(root);
	}

	private static IReadOnlyList<CanvasItem> GetCanvasItems(Node root)
	{
		if (EffectNodeCaches.TryGetValue(root, out CombatEffectNodeSet value))
		{
			return value.CanvasItems;
		}
		return FindCanvasItemsLive(root);
	}

	private static IReadOnlyList<Node> GetParticleNodes(Node root)
	{
		if (EffectNodeCaches.TryGetValue(root, out CombatEffectNodeSet value))
		{
			return value.Particles;
		}
		return FindParticleNodesLive(root);
	}

	private static CombatEffectNodeSet BuildEffectNodeCache(Node root)
	{
		return CombatEffectNodeSet.Collect(root, reverseSiblings: true, includeCanvas: true);
	}

	private static IReadOnlyList<Node> FindNodesLive(Node root)
	{
		List<Node> list = new List<Node>();
		foreach (Node item in TraverseDepthFirst(root))
		{
			list.Add(item);
		}
		return list;
	}

	private static IReadOnlyList<Node> FindSpineSpritesLive(Node root)
	{
		return GodotNodeUtil.FindNodesByClass(root, "SpineSprite");
	}

	private static IReadOnlyList<CanvasItem> FindCanvasItemsLive(Node root)
	{
		List<CanvasItem> list = new List<CanvasItem>();
		foreach (Node item in TraverseDepthFirst(root))
		{
			CanvasItem val = (CanvasItem)(object)((item is CanvasItem) ? item : null);
			if (val != null)
			{
				list.Add(val);
			}
		}
		return list;
	}

	private static IReadOnlyList<Node> FindParticleNodesLive(Node root)
	{
		List<Node> list = new List<Node>();
		foreach (Node item in TraverseDepthFirst(root))
		{
			if (IsParticleClass(((GodotObject)item).GetClass()))
			{
				list.Add(item);
			}
		}
		return list;
	}

	private static bool IsParticleClass(string className)
	{
		if (!(className == "GPUParticles2D"))
		{
			return className == "CPUParticles2D";
		}
		return true;
	}

	private static void StartParticleNode(Node particleNode, int token, Action<string>? warn)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		StringName obj = StringName.op_Implicit("emitting");
		bool flag = false;
		((GodotObject)particleNode).Set(obj, Variant.From<bool>(ref flag));
		if (((GodotObject)particleNode).HasMethod(StringName.op_Implicit("restart")))
		{
			((GodotObject)particleNode).Call(StringName.op_Implicit("restart"), Array.Empty<Variant>());
		}
		StringName obj2 = StringName.op_Implicit("emitting");
		flag = true;
		((GodotObject)particleNode).Set(obj2, Variant.From<bool>(ref flag));
		ScheduleParticleStop(particleNode, token, warn);
	}

	private static void ScheduleParticleStop(Node particleNode, int token, Action<string>? warn)
	{
		float particleEmissionDurationSeconds = GetParticleEmissionDurationSeconds(particleNode);
		if (particleEmissionDurationSeconds <= 0.001f)
		{
			return;
		}
		try
		{
			GodotScheduler.Delay(particleEmissionDurationSeconds, delegate
			{
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				try
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)particleNode) && IsParticleStartTokenCurrent(particleNode, token))
					{
						Node obj = particleNode;
						StringName obj2 = StringName.op_Implicit("emitting");
						bool flag = false;
						((GodotObject)obj).Set(obj2, Variant.From<bool>(ref flag));
					}
				}
				catch (Exception ex)
				{
					warn?.Invoke(StringName.op_Implicit(particleNode.Name) + " particle stop failed: " + SkinLog.FormatException(ex));
				}
			});
		}
		catch (Exception ex2)
		{
			warn?.Invoke(StringName.op_Implicit(particleNode.Name) + " particle stop scheduling failed: " + SkinLog.FormatException(ex2));
		}
	}

	private static int NextParticleStartToken(Node particleNode)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		int result = GetParticleStartToken(particleNode) + 1;
		((GodotObject)particleNode).SetMeta(StringName.op_Implicit("czn_particle_start_token"), Variant.From<int>(ref result));
		return result;
	}

	private static int GetParticleStartToken(Node particleNode)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)particleNode).HasMeta(StringName.op_Implicit("czn_particle_start_token")))
			{
				StringName obj = StringName.op_Implicit("czn_particle_start_token");
				Variant val = default(Variant);
				val = ((GodotObject)particleNode).GetMeta(obj, val);
				return ((Variant)(ref val)).AsInt32();
			}
		}
		catch
		{
		}
		return 0;
	}

	private static bool IsParticleStartTokenCurrent(Node particleNode, int token)
	{
		return GetParticleStartToken(particleNode) == token;
	}

	private static float GetParticleStartDelaySeconds(Node particleNode)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)particleNode).HasMeta(StringName.op_Implicit("czn_particle_start_delay_seconds")))
			{
				StringName obj = StringName.op_Implicit("czn_particle_start_delay_seconds");
				Variant val = default(Variant);
				val = ((GodotObject)particleNode).GetMeta(obj, val);
				return Math.Max(0f, ((Variant)(ref val)).AsSingle());
			}
		}
		catch
		{
		}
		return 0f;
	}

	private static float GetParticleEmissionDurationSeconds(Node particleNode)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (((GodotObject)particleNode).HasMeta(StringName.op_Implicit("czn_particle_emission_duration_seconds")))
			{
				StringName obj = StringName.op_Implicit("czn_particle_emission_duration_seconds");
				Variant val = default(Variant);
				val = ((GodotObject)particleNode).GetMeta(obj, val);
				float num = ((Variant)(ref val)).AsSingle();
				return float.IsFinite(num) ? Math.Max(0f, num) : 0f;
			}
		}
		catch
		{
		}
		return 0f;
	}
}
