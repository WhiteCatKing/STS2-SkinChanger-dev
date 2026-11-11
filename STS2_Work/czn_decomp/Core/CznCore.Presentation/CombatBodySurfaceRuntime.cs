using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal sealed class CombatBodySurfaceRuntime(CharacterSkinProfile profile, CombatCreatureSpineResolver spineResolver, Action<string>? warn = null, Action<string>? debug = null)
{
	private readonly string _scaleMeta = profile.RuntimeMetaKey("combat_body_surface_scaled");

	private readonly string _positionMeta = profile.RuntimeMetaKey("combat_body_surface_position_offset");

	public CombatBodySurfaceState GetState(NCreature? creature)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (!profile.Features.CombatBodyScalePatch)
		{
			return new CombatBodySurfaceState("feature_disabled", FeatureEnabled: false, creature != null && GodotObject.IsInstanceValid((GodotObject)(object)creature), SpineFound: false, Applied: false, null, null);
		}
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return new CombatBodySurfaceState("creature_invalid", FeatureEnabled: true, CreatureValid: false, SpineFound: false, Applied: false, null, null);
		}
		Node2D val = spineResolver.Find(creature);
		if (val == null)
		{
			return new CombatBodySurfaceState("spine_not_found", FeatureEnabled: true, CreatureValid: true, SpineFound: false, Applied: false, null, null);
		}
		GodotObject val2 = (GodotObject)(object)val;
		bool flag = val2.HasMeta(StringName.op_Implicit(_scaleMeta));
		return new CombatBodySurfaceState(flag ? "already_applied" : "pending", FeatureEnabled: true, CreatureValid: true, SpineFound: true, flag, val.Scale, val2.GetClass());
	}

	public bool Apply(NCreature creature)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if (!profile.Features.CombatBodyScalePatch)
		{
			return false;
		}
		Node2D val = spineResolver.Find(creature);
		if (val == null)
		{
			return false;
		}
		GodotObject val2 = (GodotObject)(object)val;
		if (val2.HasMeta(StringName.op_Implicit(_scaleMeta)))
		{
			return ApplyPositionOffset(val, val2);
		}
		if (!string.IsNullOrWhiteSpace(profile.BodySkeletonDataPath))
		{
			RebindSkeleton(creature, val);
		}
		Vector2 scale = val.Scale;
		val.Scale = CharacterSkinSurfaceDefaults.ResolveCznCombatBodySpineScale(scale, profile.BodySpineScaleMultiplier);
		ApplyPositionOffset(val, val2);
		StringName obj = StringName.op_Implicit(_scaleMeta);
		bool flag = true;
		val2.SetMeta(obj, Variant.From<bool>(ref flag));
		Action<string>? action = debug;
		if (action != null)
		{
			string? text = ((object)(Vector2)(ref scale)/*cast due to .constrained prefix*/).ToString();
			Vector2 scale2 = val.Scale;
			action("Applied combat body surface scale: " + text + " -> " + ((object)(Vector2)(ref scale2)/*cast due to .constrained prefix*/).ToString());
		}
		return true;
	}

	private bool ApplyPositionOffset(Node2D spine, GodotObject spineObject)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!profile.Features.CombatPositionOffsetPatch || profile.CombatPositionOffset == Vector2.Zero || spineObject.HasMeta(StringName.op_Implicit(_positionMeta)))
		{
			return false;
		}
		Vector2 position = spine.Position;
		spine.Position += profile.CombatPositionOffset;
		StringName obj = StringName.op_Implicit(_positionMeta);
		bool flag = true;
		spineObject.SetMeta(obj, Variant.From<bool>(ref flag));
		Action<string>? action = debug;
		if (action != null)
		{
			string? text = ((object)(Vector2)(ref position)/*cast due to .constrained prefix*/).ToString();
			Vector2 position2 = spine.Position;
			action("Applied combat body surface position offset: " + text + " -> " + ((object)(Vector2)(ref position2)/*cast due to .constrained prefix*/).ToString());
		}
		return true;
	}

	private void RebindSkeleton(NCreature creature, Node2D spine)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		CharacterSkinProfile characterSkinProfile = profile;
		Creature entity = creature.Entity;
		CharacterSkinProfile characterSkinProfile2 = characterSkinProfile.For((entity != null) ? entity.Player : null);
		Resource val = ResourceLoader.Load<Resource>(characterSkinProfile2.BodySkeletonDataPath, (string)null, (CacheMode)1);
		if (val == null)
		{
			throw new InvalidOperationException("Missing combat body skeleton data: " + characterSkinProfile2.BodySkeletonDataPath);
		}
		RemoveIncompatibleNativeVisualChildren(spine);
		if (characterSkinProfile2.CombatBody.AtomicPoseTransitions)
		{
			val = CombatBodyPoseTransitionRuntime.CreateResource(val);
		}
		MegaSprite val2 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spine));
		GodotObject val3 = (GodotObject)(object)val;
		val2.SetSkeletonDataRes(new MegaSkeletonDataResource(Variant.From<GodotObject>(ref val3)));
		if (characterSkinProfile2.CombatBody.AtomicPoseTransitions)
		{
			CombatBodyPoseTransitionRuntime.Bind(val2);
		}
		string idleAnimation = characterSkinProfile2.CombatBody.IdleAnimation;
		if (!string.IsNullOrWhiteSpace(idleAnimation))
		{
			if (!val2.HasAnimation(idleAnimation))
			{
				throw new InvalidOperationException("Combat body skeleton missing idle animation: " + idleAnimation);
			}
			SpineAnimationCompat.SetAnimation(val2.GetAnimationState(), idleAnimation);
		}
		if (spine != null)
		{
			((CanvasItem)spine).Visible = true;
		}
		spineResolver.TrySetVisualsSpineBody(creature.Visuals, spine, warn);
		debug?.Invoke("Rebound combat body skeleton: " + characterSkinProfile2.BodySkeletonDataPath);
	}

	private void RemoveIncompatibleNativeVisualChildren(Node2D spine)
	{
		IReadOnlyList<string> incompatibleNativeVisualChildPaths = profile.CombatBody.IncompatibleNativeVisualChildPaths;
		if (incompatibleNativeVisualChildPaths == null || incompatibleNativeVisualChildPaths.Count == 0)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (string item in incompatibleNativeVisualChildPaths)
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				continue;
			}
			Node nodeOrNull = ((Node)spine).GetNodeOrNull<Node>(NodePath.op_Implicit(item));
			if (nodeOrNull != null && ((Node)spine).IsAncestorOf(nodeOrNull))
			{
				Node parent = nodeOrNull.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(nodeOrNull);
				}
				((GodotObject)nodeOrNull).Free();
				list.Add(item);
			}
		}
		if (list.Count > 0)
		{
			debug?.Invoke("Removed incompatible native combat visual children: " + string.Join(", ", list));
		}
	}
}
