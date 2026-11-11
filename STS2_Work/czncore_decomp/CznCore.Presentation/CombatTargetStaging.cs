using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatTargetStaging
{
	private readonly struct CombatTargetProxySource
	{
		public Node2D Anchor { get; }

		public Vector2 ScaleMagnitude { get; }

		public Transform2D ViewportBasis { get; }

		public CombatTargetProxySource(Node2D anchor, Vector2 scaleMagnitude, Transform2D viewportBasis)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			Anchor = anchor;
			ScaleMagnitude = scaleMagnitude;
			ViewportBasis = viewportBasis;
		}
	}

	private const int SpineVisualStateMaxTracks = 8;

	public const string SpineVisualStateMeta = "czn_spine_state";

	private const float ProxyBasisEpsilon = 0.0001f;

	public static NCreature? FirstValidCreatureNode(IReadOnlyList<NCreature>? creatureNodes)
	{
		if (creatureNodes == null)
		{
			return null;
		}
		foreach (NCreature creatureNode in creatureNodes)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				return creatureNode;
			}
		}
		return null;
	}

	public static Node2D? FindFirstSpineAnchor(CombatCreatureSpineResolver spines, IReadOnlyList<NCreature>? creatureNodes)
	{
		if (creatureNodes == null)
		{
			return null;
		}
		foreach (NCreature creatureNode in creatureNodes)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				Node2D val = spines.Find(creatureNode);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					return val;
				}
			}
		}
		return null;
	}

	private static Node2D? TryCreateTargetProxy(Node2D? sourceAnchor, string proxyName, int duplicateFlags, string failureContext, Action<string>? warn = null)
	{
		if (sourceAnchor == null || !GodotObject.IsInstanceValid((GodotObject)(object)sourceAnchor))
		{
			return null;
		}
		try
		{
			Node val = ((Node)sourceAnchor).Duplicate(duplicateFlags);
			Node2D val2 = (Node2D)(object)((val is Node2D) ? val : null);
			if (val2 == null)
			{
				if (val != null)
				{
					val.QueueFree();
				}
				return null;
			}
			((Node)val2).Name = StringName.op_Implicit(proxyName);
			return val2;
		}
		catch (Exception ex)
		{
			warn?.Invoke(failureContext + ": " + SkinLog.FormatException(ex));
			return null;
		}
	}

	public static Node2D? TryCreateMirroredTargetProxy(Node2D? sourceAnchor, string proxyName, int duplicateFlags, string failureContext, Action<string>? warn = null)
	{
		Vector2 baseScale;
		return TryCreateMirroredTargetProxy(sourceAnchor, proxyName, duplicateFlags, failureContext, out baseScale, warn);
	}

	public static Node2D? TryCreateMirroredTargetProxy(Node2D? sourceAnchor, string proxyName, int duplicateFlags, string failureContext, out Vector2 baseScale, Action<string>? warn = null)
	{
		return TryCreateMirroredTargetProxy(sourceAnchor, proxyName, duplicateFlags, failureContext, out baseScale, reflectHorizontally: false, warn);
	}

	public static Node2D? TryCreateMirroredTargetProxy(Node2D? sourceAnchor, string proxyName, int duplicateFlags, string failureContext, out Vector2 baseScale, bool reflectHorizontally, Action<string>? warn = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		baseScale = Vector2.One;
		if (!TryCaptureProxySource(sourceAnchor, out var source))
		{
			return null;
		}
		CombatTargetProxyRuntime combatTargetProxyRuntime = null;
		try
		{
			combatTargetProxyRuntime = CreateProxyRuntime(proxyName);
			baseScale = source.ScaleMagnitude;
			((Node2D)combatTargetProxyRuntime).Scale = baseScale;
			if (!TryAttachProxyVisual(combatTargetProxyRuntime, source, proxyName + "Visual", duplicateFlags, source.ScaleMagnitude, Vector2.Zero, 0, reflectHorizontally, failureContext, warn))
			{
				((GodotObject)combatTargetProxyRuntime).Free();
				return null;
			}
			return (Node2D?)(object)combatTargetProxyRuntime;
		}
		catch (Exception ex)
		{
			if (combatTargetProxyRuntime != null)
			{
				((GodotObject)combatTargetProxyRuntime).Free();
			}
			warn?.Invoke(failureContext + ": " + SkinLog.FormatException(ex));
			return null;
		}
	}

	public static Node2D? TryCreateMirroredTargetProxyGroup(IReadOnlyList<Node2D>? sourceAnchors, string groupName, string childNamePrefix, int duplicateFlags, int maxCopies, Vector2 groupScale, Func<Node2D, int, int, Vector2, Vector2> resolveChildPosition, string failureContext, out Vector2 baseScale, Action<string>? warn = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		baseScale = Vector2.One;
		if (sourceAnchors == null || maxCopies <= 0)
		{
			return null;
		}
		List<CombatTargetProxySource> list = new List<CombatTargetProxySource>(Math.Min(maxCopies, sourceAnchors.Count));
		foreach (Node2D sourceAnchor in sourceAnchors)
		{
			if (list.Count >= maxCopies)
			{
				break;
			}
			if (TryCaptureProxySource(sourceAnchor, out var source))
			{
				list.Add(source);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		Vector2 val = ResolveAverageScale(list);
		baseScale = new Vector2(val.X * MathF.Abs(groupScale.X), val.Y * MathF.Abs(groupScale.Y));
		CombatTargetProxyRuntime combatTargetProxyRuntime = null;
		try
		{
			combatTargetProxyRuntime = CreateProxyRuntime(groupName);
			((Node2D)combatTargetProxyRuntime).Scale = baseScale;
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				CombatTargetProxySource source2 = list[i];
				Vector2 position = resolveChildPosition(source2.Anchor, i, list.Count, baseScale);
				if (TryAttachProxyVisual(combatTargetProxyRuntime, source2, childNamePrefix + (i + 1), duplicateFlags, val, position, i, reflectHorizontally: false, failureContext, warn))
				{
					num++;
				}
			}
			if (num == 0)
			{
				((GodotObject)combatTargetProxyRuntime).Free();
				return null;
			}
			return (Node2D?)(object)combatTargetProxyRuntime;
		}
		catch (Exception ex)
		{
			if (combatTargetProxyRuntime != null)
			{
				((GodotObject)combatTargetProxyRuntime).Free();
			}
			warn?.Invoke(failureContext + ": " + SkinLog.FormatException(ex));
			return null;
		}
	}

	private static CombatTargetProxyRuntime CreateProxyRuntime(string name)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		CombatTargetProxyRuntime combatTargetProxyRuntime = new CombatTargetProxyRuntime();
		((Node)combatTargetProxyRuntime).Name = StringName.op_Implicit(name);
		((Node2D)combatTargetProxyRuntime).Position = Vector2.Zero;
		((Node2D)combatTargetProxyRuntime).Rotation = 0f;
		((Node2D)combatTargetProxyRuntime).Scale = Vector2.One;
		((CanvasItem)combatTargetProxyRuntime).TopLevel = false;
		((CanvasItem)combatTargetProxyRuntime).ZAsRelative = true;
		((CanvasItem)combatTargetProxyRuntime).ZIndex = 0;
		((CanvasItem)combatTargetProxyRuntime).Visible = true;
		combatTargetProxyRuntime.BindRouteCommandSignals();
		return combatTargetProxyRuntime;
	}

	private static bool TryAttachProxyVisual(CombatTargetProxyRuntime runtime, CombatTargetProxySource source, string visualName, int duplicateFlags, Vector2 basisNormalizationScale, Vector2 position, int zIndex, bool reflectHorizontally, string failureContext, Action<string>? warn)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = TryCreateTargetProxy(source.Anchor, visualName, duplicateFlags, failureContext, warn);
		if (val == null)
		{
			return false;
		}
		Vector2 val2 = DivideComponents(source.ViewportBasis.X, basisNormalizationScale);
		Vector2 val3 = DivideComponents(source.ViewportBasis.Y, basisNormalizationScale);
		if (reflectHorizontally)
		{
			val2 = ReflectViewportX(val2);
			val3 = ReflectViewportX(val3);
		}
		val.Transform = new Transform2D(val2, val3, position);
		((CanvasItem)val).TopLevel = false;
		((CanvasItem)val).ZAsRelative = true;
		((CanvasItem)val).ZIndex = zIndex;
		((CanvasItem)val).Visible = true;
		StampMirroredSpineVisualStates((Node)(object)source.Anchor, (Node)(object)val);
		PruneLivePresentationChildren((Node)(object)source.Anchor, (Node)(object)val);
		CombatCinematicFeedback.RegisterMirror(source.Anchor, val);
		((Node)runtime).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static void PruneLivePresentationChildren(Node source, Node copy)
	{
		for (int num = Math.Min(source.GetChildCount(false), copy.GetChildCount(false)) - 1; num >= 0; num--)
		{
			Node child = source.GetChild(num, false);
			Node child2 = copy.GetChild(num, false);
			if (IsLivePresentationChild(child))
			{
				copy.RemoveChild(child2);
				((GodotObject)child2).Free();
			}
			else
			{
				PruneLivePresentationChildren(child, child2);
			}
		}
	}

	private static bool IsLivePresentationChild(Node node)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (((GodotObject)node).HasMeta(StringName.op_Implicit("support_body")) || ((GodotObject)node).HasMeta(StringName.op_Implicit("czn_inner_owner")))
		{
			return true;
		}
		foreach (StringName meta in ((GodotObject)node).GetMetaList())
		{
			if (((object)meta).ToString().EndsWith("_combat_transient_effects_root", StringComparison.Ordinal))
			{
				return true;
			}
		}
		Variant script = ((GodotObject)node).GetScript();
		GodotObject obj = ((Variant)(ref script)).AsGodotObject();
		for (Script val = (Script)(object)((obj is Script) ? obj : null); val != null; val = val.GetBaseScript())
		{
			if (((Resource)val).ResourcePath == "res://CznCore/scripts/inner_effect_composite.gd")
			{
				return true;
			}
		}
		if (!(node is SubViewport))
		{
			Sprite2D val2 = (Sprite2D)(object)((node is Sprite2D) ? node : null);
			if (val2 == null || !(val2.Texture is ViewportTexture))
			{
				TextureRect val3 = (TextureRect)(object)((node is TextureRect) ? node : null);
				if (val3 != null)
				{
					return val3.Texture is ViewportTexture;
				}
				return false;
			}
		}
		return true;
	}

	private static void StampMirroredSpineVisualStates(Node source, Node duplicate)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = (Node2D)(object)((source is Node2D) ? source : null);
		if (val != null)
		{
			Node2D val2 = (Node2D)(object)((duplicate is Node2D) ? duplicate : null);
			if (val2 != null && ((GodotObject)val).HasMethod(StringName.op_Implicit("get_animation_state")) && ((GodotObject)val).HasMethod(StringName.op_Implicit("get_skeleton")))
			{
				Dictionary val3 = CaptureSpineVisualState(val);
				((GodotObject)val2).SetMeta(StringName.op_Implicit("czn_spine_state"), Variant.op_Implicit(val3));
			}
		}
		int num = Math.Min(source.GetChildCount(false), duplicate.GetChildCount(false));
		for (int i = 0; i < num; i++)
		{
			StampMirroredSpineVisualStates(source.GetChild(i, false), duplicate.GetChild(i, false));
		}
	}

	private static bool TryCaptureProxySource(Node2D? anchor, out CombatTargetProxySource source)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		source = default(CombatTargetProxySource);
		if (anchor == null || !GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			return false;
		}
		if (!CombatCinematicFormation.TryGetViewportTransform(anchor, out var viewportTransform))
		{
			return false;
		}
		float val = ((Vector2)(ref viewportTransform.X)).Length();
		float val2 = ((Vector2)(ref viewportTransform.Y)).Length();
		source = new CombatTargetProxySource(anchor, new Vector2(Math.Max(val, 0.0001f), Math.Max(val2, 0.0001f)), new Transform2D(viewportTransform.X, viewportTransform.Y, Vector2.Zero));
		return true;
	}

	private static Vector2 ResolveAverageScale(IReadOnlyList<CombatTargetProxySource> sources)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.Zero;
		foreach (CombatTargetProxySource source in sources)
		{
			val += source.ScaleMagnitude;
		}
		return val / (float)sources.Count;
	}

	private static Vector2 DivideComponents(Vector2 value, Vector2 divisor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(value.X / divisor.X, value.Y / divisor.Y);
	}

	private static Vector2 ReflectViewportX(Vector2 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(0f - value.X, value.Y);
	}

	public static Dictionary CaptureSpineVisualState(Node2D? sourceAnchor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Expected O, but got Unknown
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		Dictionary val = new Dictionary();
		if (sourceAnchor == null || !GodotObject.IsInstanceValid((GodotObject)(object)sourceAnchor))
		{
			return val;
		}
		Variant val2;
		try
		{
			if (((GodotObject)sourceAnchor).HasMethod(StringName.op_Implicit("get_skeleton")))
			{
				val2 = ((GodotObject)sourceAnchor).Call(StringName.op_Implicit("get_skeleton"), Array.Empty<Variant>());
				GodotObject val3 = ((Variant)(ref val2)).AsGodotObject();
				if (val3 != null && val3.HasMethod(StringName.op_Implicit("get_skin")))
				{
					val2 = val3.Call(StringName.op_Implicit("get_skin"), Array.Empty<Variant>());
					GodotObject val4 = ((Variant)(ref val2)).AsGodotObject();
					if (val4 != null && val4.HasMethod(StringName.op_Implicit("get_name")))
					{
						val2 = val4.Call(StringName.op_Implicit("get_name"), Array.Empty<Variant>());
						string text = ((Variant)(ref val2)).AsString();
						if (!string.IsNullOrEmpty(text))
						{
							val[Variant.op_Implicit("skin")] = Variant.op_Implicit(text);
						}
					}
				}
			}
		}
		catch
		{
		}
		try
		{
			if (((GodotObject)sourceAnchor).HasMethod(StringName.op_Implicit("get_animation_state")))
			{
				val2 = ((GodotObject)sourceAnchor).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
				GodotObject val5 = ((Variant)(ref val2)).AsGodotObject();
				if (val5 != null && val5.HasMethod(StringName.op_Implicit("get_current")))
				{
					Array val6 = new Array();
					for (int i = 0; i < 8; i++)
					{
						val2 = val5.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(i) });
						GodotObject val7 = ((Variant)(ref val2)).AsGodotObject();
						if (val7 == null || !val7.HasMethod(StringName.op_Implicit("get_animation")))
						{
							continue;
						}
						val2 = val7.Call(StringName.op_Implicit("get_animation"), Array.Empty<Variant>());
						GodotObject val8 = ((Variant)(ref val2)).AsGodotObject();
						if (val8 == null || !val8.HasMethod(StringName.op_Implicit("get_name")))
						{
							continue;
						}
						val2 = val8.Call(StringName.op_Implicit("get_name"), Array.Empty<Variant>());
						string text2 = ((Variant)(ref val2)).AsString();
						if (!string.IsNullOrEmpty(text2) && !text2.StartsWith("<"))
						{
							Dictionary val9 = new Dictionary();
							val2 = Variant.op_Implicit("track");
							val9[val2] = Variant.op_Implicit(i);
							Variant val10 = Variant.op_Implicit("anim");
							val9[val10] = Variant.op_Implicit(text2);
							Dictionary val11 = val9;
							if (val7.HasMethod(StringName.op_Implicit("get_track_time")))
							{
								Variant val12 = Variant.op_Implicit("time");
								val10 = val7.Call(StringName.op_Implicit("get_track_time"), Array.Empty<Variant>());
								val11[val12] = Variant.op_Implicit(((Variant)(ref val10)).AsSingle());
							}
							if (val7.HasMethod(StringName.op_Implicit("get_loop")))
							{
								Variant val13 = Variant.op_Implicit("loop");
								val10 = val7.Call(StringName.op_Implicit("get_loop"), Array.Empty<Variant>());
								val11[val13] = Variant.op_Implicit(((Variant)(ref val10)).AsBool());
							}
							val6.Add(Variant.op_Implicit(val11));
							if (i == 0)
							{
								val[Variant.op_Implicit("anim")] = Variant.op_Implicit(text2);
							}
						}
					}
					if (val6.Count > 0)
					{
						val[Variant.op_Implicit("tracks")] = Variant.op_Implicit(val6);
					}
				}
			}
		}
		catch
		{
		}
		return val;
	}

	public static bool TryGetAnchorSnapshot(CombatCreatureSpineResolver? spines, NCreature? creatureNode, out CombatTargetAnchorSnapshot snapshot)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		snapshot = default(CombatTargetAnchorSnapshot);
		if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return false;
		}
		Node2D spine = spines?.Find(creatureNode);
		Vector2 vfxSpawnGlobal = Vector2.Zero;
		try
		{
			vfxSpawnGlobal = creatureNode.VfxSpawnPosition;
		}
		catch
		{
		}
		Vector2 hitboxGlobalPosition = Vector2.Zero;
		Vector2 hitboxSize = Vector2.Zero;
		Vector2 hitboxTopGlobal = Vector2.Zero;
		Vector2 hitboxBottomGlobal = Vector2.Zero;
		try
		{
			Control hitbox = creatureNode.Hitbox;
			if (hitbox != null && GodotObject.IsInstanceValid((GodotObject)(object)hitbox))
			{
				hitboxGlobalPosition = hitbox.GlobalPosition;
				hitboxSize = hitbox.Size;
				hitboxTopGlobal = creatureNode.GetTopOfHitbox();
				hitboxBottomGlobal = creatureNode.GetBottomOfHitbox();
			}
		}
		catch
		{
		}
		Vector2 boundsSize = Vector2.Zero;
		try
		{
			NCreatureVisuals visuals = creatureNode.Visuals;
			Vector2? obj3;
			if (visuals == null)
			{
				obj3 = null;
			}
			else
			{
				Control bounds = visuals.Bounds;
				obj3 = ((bounds != null) ? new Vector2?(bounds.Size) : ((Vector2?)null));
			}
			boundsSize = (Vector2)(((_003F?)obj3) ?? Vector2.Zero);
		}
		catch
		{
		}
		snapshot = new CombatTargetAnchorSnapshot(creatureNode, spine, vfxSpawnGlobal, hitboxGlobalPosition, hitboxSize, hitboxTopGlobal, hitboxBottomGlobal, boundsSize);
		return true;
	}
}
