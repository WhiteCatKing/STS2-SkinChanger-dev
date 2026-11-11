using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyVisualHostAdapter
{
	private const string SecondaryBodyNamePrefix = "CznEnemySecondaryBody_";

	private readonly EnemyEffectMaterialRuntime _effectMaterials;

	internal static readonly StringName AppliedProfileMeta = new StringName("czn_enemy_skin_applied_profile");

	internal static readonly StringName OfficialDeathBodyHiddenMeta = new StringName("czn_enemy_skin_official_death_body_hidden");

	private static readonly NodePath VisualsPath = new NodePath("%Visuals");

	private static readonly ConditionalWeakTable<NCreatureVisuals, string> AppliedProfileIds = new ConditionalWeakTable<NCreatureVisuals, string>();

	public EnemyVisualHostAdapter(EnemyEffectMaterialRuntime effectMaterials)
	{
		_effectMaterials = effectMaterials;
	}

	public bool TryCreateReplacement(EnemyReplacementProfile profile, Vector2 combatVisualOffset, out NCreatureVisuals replacement)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		replacement = null;
		NCreatureVisuals val = null;
		try
		{
			Resource val2 = ResourceLoader.Load<Resource>(profile.Monster.SkeletonDataPath, (string)null, (CacheMode)1);
			if (val2 == null)
			{
				Entry.Log.Warn("Enemy replacement resource is unavailable; using STS2 original [profile=" + profile.Id + ", path=" + profile.Monster.SkeletonDataPath + "].", 1);
				return false;
			}
			GodotObject val3 = (GodotObject)(object)val2;
			MegaSkeletonDataResource val4 = new MegaSkeletonDataResource(Variant.From<GodotObject>(ref val3));
			ValidateAnimationContract(profile, val4);
			val = PreloadManager.Cache.GetScene(profile.Monster.HostVisualsPath).Instantiate<NCreatureVisuals>((GenEditState)0);
			Node2D val5 = EnsureSpineSpriteHost(val);
			Vector2 scale = val5.Scale;
			((CanvasItem)val5).ZAsRelative = true;
			((CanvasItem)val5).ZIndex = 0;
			new MegaSprite(Variant.op_Implicit((GodotObject)(object)val5)).SetSkeletonDataRes(val4);
			EnemyDefenseModelScales.Apply(val5, profile.Monster.SkeletonDataPath);
			_effectMaterials.ApplyToBody((Node)(object)val5, profile.Monster.ApplyNeutralMultiplyToBody);
			val5.Scale = new Vector2(profile.Monster.VisualScale, profile.Monster.VisualScale);
			ApplyHostTransformPolicy(profile, val5);
			ApplyBodyLayoutPolicy(profile, val5);
			val5.Position += combatVisualOffset;
			CreateSecondaryBodies(profile, val, val5);
			ThievingHopperCardAnchorAdapter.Prepare(profile, val5, scale);
			ApplySelectionBounds(profile, val, val5);
			ApplyCenterAnchorPolicy(profile, val);
			RemoveIncompatibleNativeChildren(profile, val5);
			RemoveIncompatibleNativeHostChildren(profile, val);
			DisableNativeParticles(profile, val);
			EnemyPresentationDepthPolicy.NormalizeVisualSubtree((Node)(object)val);
			NCreatureVisuals obj = val;
			StringName appliedProfileMeta = AppliedProfileMeta;
			string id = profile.Id;
			((GodotObject)obj).SetMeta(appliedProfileMeta, Variant.From<string>(ref id));
			Control node = ((Node)val).GetNode<Control>(NodePath.op_Implicit("%Bounds"));
			replacement = val;
			val = null;
			Entry.Log.Info("Prepared enemy replacement visual " + $"[profile={profile.Id}, target={profile.Target.MonsterId}, " + $"visualScale={profile.Monster.VisualScale:F6}, " + $"combatVisualOffset={combatVisualOffset}, " + $"selectionBounds={node.Position} " + $"{node.Size}].", 1);
			return true;
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Enemy replacement visual failed; using STS2 original " + $"[profile={profile.Id}, error={ex.Message}].", 1);
			return false;
		}
		finally
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
		}
	}

	private void CreateSecondaryBodies(EnemyReplacementProfile profile, NCreatureVisuals visuals, Node2D primaryBody)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		Node val = ((Node)primaryBody).GetParent() ?? throw new InvalidOperationException("Enemy primary body has no parent.");
		foreach (EnemySecondaryBodyProfile secondary2 in profile.Monster.SecondaryBodies.OrderBy((EnemySecondaryBodyProfile item) => item.ZIndexOffset))
		{
			GodotObject val2 = (GodotObject)(object)(ResourceLoader.Load<Resource>(secondary2.SkeletonDataPath, (string)null, (CacheMode)1) ?? throw new InvalidOperationException("Secondary body resource is unavailable: " + secondary2.SkeletonDataPath));
			MegaSkeletonDataResource val3 = new MegaSkeletonDataResource(Variant.From<GodotObject>(ref val2));
			ValidateSecondaryBody(secondary2, val3);
			Variant val4 = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			GodotObject obj = ((Variant)(ref val4)).AsGodotObject();
			Node2D val5 = (Node2D)(object)((obj is Node2D) ? obj : null);
			if (val5 == null)
			{
				throw new InvalidOperationException("MegaSpine could not create a secondary SpineSprite.");
			}
			((Node)val5).Name = StringName.op_Implicit("CznEnemySecondaryBody_" + secondary2.Role);
			val5.Position = primaryBody.Position;
			val5.Rotation = primaryBody.Rotation;
			val5.Skew = primaryBody.Skew;
			val5.Scale = new Vector2(secondary2.VisualScale, secondary2.VisualScale);
			((CanvasItem)val5).ZAsRelative = true;
			((CanvasItem)val5).ZIndex = secondary2.ZIndexOffset;
			((CanvasItem)val5).YSortEnabled = ((CanvasItem)primaryBody).YSortEnabled;
			((CanvasItem)val5).ShowBehindParent = ((CanvasItem)primaryBody).ShowBehindParent;
			((CanvasItem)val5).Visible = secondary2.InitiallyVisible;
			val.AddChild((Node)(object)val5, false, (InternalMode)0);
			((Node)val5).Owner = ((Node)primaryBody).Owner;
			MegaSprite val6 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val5));
			val6.SetSkeletonDataRes(val3);
			EnemyDefenseModelScales.Apply(val5, secondary2.SkeletonDataPath);
			_effectMaterials.ApplyToBody((Node)(object)val5);
			SpineNodeExtensions.RunWhenSpineReady((Node)(object)visuals, val6, (Action<MegaAnimationState>)delegate(MegaAnimationState state)
			{
				PlayInitialSecondaryAnimation(state, secondary2);
			});
		}
		if (profile.Monster.SecondaryBodies.Any((EnemySecondaryBodyProfile secondary) => secondary.HidePrimaryInitially))
		{
			((CanvasItem)primaryBody).Visible = false;
		}
	}

	private static void ValidateSecondaryBody(EnemySecondaryBodyProfile profile, MegaSkeletonDataResource skeletonData)
	{
		if (!skeletonData.HasAnimation(profile.IdleAnimation))
		{
			throw new InvalidOperationException($"Secondary body '{profile.Role}' is missing idle animation '{profile.IdleAnimation}'.");
		}
		string entranceAnimation = profile.EntranceAnimation;
		if (entranceAnimation != null && !skeletonData.HasAnimation(entranceAnimation))
		{
			throw new InvalidOperationException($"Secondary body '{profile.Role}' is missing entrance animation '{entranceAnimation}'.");
		}
	}

	private static void PlayInitialSecondaryAnimation(MegaAnimationState state, EnemySecondaryBodyProfile profile)
	{
		string entranceAnimation = profile.EntranceAnimation;
		if (entranceAnimation != null)
		{
			state.SetAnimation(entranceAnimation, false, 0);
			state.AddAnimation(profile.IdleAnimation, 0f, true, 0);
		}
		else
		{
			state.SetAnimation(profile.IdleAnimation, true, 0);
		}
	}

	private static void ApplyHostTransformPolicy(EnemyReplacementProfile profile, Node2D spineNode)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (profile.Monster.HostTransformPolicy == EnemyHostTransformPolicy.ResetParentTransform)
		{
			Node parent = ((Node)spineNode).GetParent();
			Node obj = ((parent is Node2D) ? parent : null) ?? throw new InvalidOperationException("Profile '" + profile.Id + "' asks to reset the host parent transform, but the replacement visuals no longer have a Node2D parent.");
			((Node2D)obj).Position = Vector2.Zero;
			((Node2D)obj).Rotation = 0f;
			((Node2D)obj).Skew = 0f;
			((Node2D)obj).Scale = Vector2.One;
		}
	}

	private static void ApplyBodyLayoutPolicy(EnemyReplacementProfile profile, Node2D spineNode)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		spineNode.Position = (Vector2)(profile.Monster.BodyOriginPolicy switch
		{
			EnemyBodyOriginPolicy.ResetToSkeletonOrigin => Vector2.Zero, 
			EnemyBodyOriginPolicy.ResetVerticalOffset => new Vector2(spineNode.Position.X, 0f), 
			_ => spineNode.Position, 
		});
		if (profile.Monster.MirrorBodyHorizontally)
		{
			spineNode.Scale = new Vector2(0f - Mathf.Abs(spineNode.Scale.X), spineNode.Scale.Y);
		}
	}

	private static void ApplyCenterAnchorPolicy(EnemyReplacementProfile profile, NCreatureVisuals candidate)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (profile.Monster.CenterAnchorOnSelectionBounds)
		{
			Control node = ((Node)candidate).GetNode<Control>(NodePath.op_Implicit("%Bounds"));
			Vector2 position = node.Position + node.Size * 0.5f;
			((Node2D)((Node)candidate).GetNode<Marker2D>(NodePath.op_Implicit("%CenterPos"))).Position = position;
		}
	}

	public static bool TryGetAppliedProfileId(NCreatureVisuals? visuals, out string profileId)
	{
		profileId = string.Empty;
		if (visuals == null || !GodotObject.IsInstanceValid((GodotObject)(object)visuals))
		{
			return false;
		}
		profileId = AppliedProfileIds.GetValue(visuals, (NCreatureVisuals candidate) => ReadAppliedProfileId(candidate));
		return profileId.Length != 0;
	}

	private static string ReadAppliedProfileId(NCreatureVisuals visuals)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I8
		if (!((GodotObject)visuals).HasMeta(AppliedProfileMeta))
		{
			return string.Empty;
		}
		Variant meta = ((GodotObject)visuals).GetMeta(AppliedProfileMeta, default(Variant));
		if ((long)((Variant)(ref meta)).VariantType != 4)
		{
			return string.Empty;
		}
		string text = ((Variant)(ref meta)).AsString();
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return string.Empty;
	}

	public static bool TryResolveAppliedProfile(NCreature? creature, out EnemyReplacementProfile profile)
	{
		profile = null;
		object obj;
		if (creature == null)
		{
			obj = null;
		}
		else
		{
			Creature entity = creature.Entity;
			obj = ((entity != null) ? entity.Monster : null);
		}
		MonsterModel val = (MonsterModel)obj;
		if (val == null)
		{
			return false;
		}
		if (TryGetAppliedProfileId(creature.Visuals, out string profileId) && Entry.Replacements.TryGetProfile(profileId, out profile) && Entry.Replacements.IsEnabled(profile))
		{
			return string.Equals(profile.Target.MonsterId, ((AbstractModel)val).Id.Entry, StringComparison.Ordinal);
		}
		return false;
	}

	public static bool TrySetPrimaryBodyVisible(NCreature creature, bool visible)
	{
		NCreatureVisuals visuals = creature.Visuals;
		Node2D val = ((visuals != null) ? ((Node)visuals).GetNodeOrNull<Node2D>(VisualsPath) : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return false;
		}
		((CanvasItem)val).Visible = visible;
		return true;
	}

	public static bool TrySetSecondaryBodyVisible(NCreature creature, string role, bool visible)
	{
		if (!TryResolveSecondaryBody(creature, role, out Node2D body))
		{
			return false;
		}
		((CanvasItem)body).Visible = visible;
		return true;
	}

	public static bool IsSecondaryBodyVisible(NCreature creature, string role)
	{
		if (TryResolveSecondaryBody(creature, role, out Node2D body))
		{
			return ((CanvasItem)body).Visible;
		}
		return false;
	}

	public static bool TryPlaySecondaryBody(NCreature creature, string role, string animation, string? nextLoopAnimation = null)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!TryResolveSecondaryBody(creature, role, out Node2D body))
		{
			return false;
		}
		MegaAnimationState val = new MegaSprite(Variant.op_Implicit((GodotObject)(object)body)).TryGetAnimationState();
		if (val == null)
		{
			return false;
		}
		val.SetAnimation(animation, nextLoopAnimation == animation, 0);
		if (nextLoopAnimation != null && nextLoopAnimation != animation)
		{
			val.AddAnimation(nextLoopAnimation, 0f, true, 0);
		}
		return true;
	}

	internal static bool TryResolveSecondaryBody(NCreature creature, string role, out Node2D body)
	{
		body = null;
		NCreatureVisuals visuals = creature.Visuals;
		Node2D obj = ((visuals != null) ? ((Node)visuals).GetNodeOrNull<Node2D>(VisualsPath) : null);
		Node obj2 = ((obj != null) ? ((Node)obj).GetParent() : null);
		Node2D val = ((obj2 != null) ? obj2.GetNodeOrNull<Node2D>(NodePath.op_Implicit("CznEnemySecondaryBody_" + role)) : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return false;
		}
		body = val;
		return true;
	}

	public static bool TryHideBodyForOfficialDeathEffect(NCreature creature, EnemyReplacementProfile profile)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (profile.Monster.DeathBodyPolicy == EnemyDeathBodyPolicy.HideForOfficialEffect && creature.Visuals != null && GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals))
		{
			CanvasItem nodeOrNull = ((Node)creature.Visuals).GetNodeOrNull<CanvasItem>(VisualsPath);
			if (nodeOrNull != null)
			{
				Color selfModulate = nodeOrNull.SelfModulate;
				nodeOrNull.SelfModulate = new Color(selfModulate.R, selfModulate.G, selfModulate.B, 0f);
				NCreatureVisuals visuals = creature.Visuals;
				StringName officialDeathBodyHiddenMeta = OfficialDeathBodyHiddenMeta;
				bool flag = true;
				((GodotObject)visuals).SetMeta(officialDeathBodyHiddenMeta, Variant.From<bool>(ref flag));
				return true;
			}
		}
		return false;
	}

	public static bool WasBodyHiddenForOfficialDeathEffect(NCreature? creature)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (((creature != null) ? creature.Visuals : null) != null && GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals) && ((GodotObject)creature.Visuals).HasMeta(OfficialDeathBodyHiddenMeta))
		{
			NCreatureVisuals visuals = creature.Visuals;
			StringName officialDeathBodyHiddenMeta = OfficialDeathBodyHiddenMeta;
			Variant val = default(Variant);
			val = ((GodotObject)visuals).GetMeta(officialDeathBodyHiddenMeta, val);
			return ((Variant)(ref val)).AsBool();
		}
		return false;
	}

	private static void ValidateAnimationContract(EnemyReplacementProfile profile, MegaSkeletonDataResource skeletonData)
	{
		foreach (EnemyAnimationBinding animationBinding in profile.Monster.AnimationBindings)
		{
			if (!skeletonData.HasAnimation(animationBinding.Sts2AnimationName))
			{
				throw new InvalidOperationException("Replacement skeleton is missing STS2 animation alias '" + animationBinding.Sts2AnimationName + "'.");
			}
			if (!skeletonData.HasAnimation(animationBinding.CznAnimationName))
			{
				throw new InvalidOperationException("Replacement skeleton is missing CZN phase animation '" + animationBinding.CznAnimationName + "'.");
			}
		}
		string entranceAnimation = profile.Monster.EntranceAnimation;
		if (entranceAnimation != null && !skeletonData.HasAnimation(entranceAnimation))
		{
			throw new InvalidOperationException("Replacement skeleton is missing entrance animation '" + entranceAnimation + "'.");
		}
	}

	private static void RemoveIncompatibleNativeChildren(EnemyReplacementProfile profile, Node2D spineNode)
	{
		foreach (string incompatibleNativeVisualChildPath in profile.Monster.IncompatibleNativeVisualChildPaths)
		{
			Node nodeOrNull = ((Node)spineNode).GetNodeOrNull<Node>(NodePath.op_Implicit(incompatibleNativeVisualChildPath));
			if (nodeOrNull != null && ((Node)spineNode).IsAncestorOf(nodeOrNull))
			{
				Node parent = nodeOrNull.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(nodeOrNull);
				}
				((GodotObject)nodeOrNull).Free();
			}
		}
	}

	private static void RemoveIncompatibleNativeHostChildren(EnemyReplacementProfile profile, NCreatureVisuals candidate)
	{
		foreach (string incompatibleNativeHostChildPath in profile.Monster.IncompatibleNativeHostChildPaths)
		{
			Node nodeOrNull = ((Node)candidate).GetNodeOrNull<Node>(NodePath.op_Implicit(incompatibleNativeHostChildPath));
			if (nodeOrNull != null && ((Node)candidate).IsAncestorOf(nodeOrNull))
			{
				Node parent = nodeOrNull.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(nodeOrNull);
				}
				((GodotObject)nodeOrNull).Free();
			}
		}
	}

	private static Node2D EnsureSpineSpriteHost(NCreatureVisuals candidate)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Node2D node = ((Node)candidate).GetNode<Node2D>(VisualsPath);
		if (((GodotObject)node).GetClass() == "SpineSprite")
		{
			return node;
		}
		Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		Node2D val2 = (Node2D)(object)((obj is Node2D) ? obj : null);
		if (val2 == null)
		{
			throw new InvalidOperationException("MegaSpine could not create a SpineSprite host.");
		}
		Node obj2 = ((Node)node).GetParent() ?? throw new InvalidOperationException("Enemy Visuals placeholder has no parent.");
		int index = ((Node)node).GetIndex(false);
		Node owner = ((Node)node).Owner;
		val2.Position = node.Position;
		val2.Rotation = node.Rotation;
		val2.Skew = node.Skew;
		((CanvasItem)val2).ZIndex = ((CanvasItem)node).ZIndex;
		((CanvasItem)val2).ZAsRelative = ((CanvasItem)node).ZAsRelative;
		((CanvasItem)val2).YSortEnabled = ((CanvasItem)node).YSortEnabled;
		((CanvasItem)val2).ShowBehindParent = ((CanvasItem)node).ShowBehindParent;
		((Node)val2).Name = StringName.op_Implicit("Visuals");
		((Node)val2).UniqueNameInOwner = true;
		((CanvasItem)val2).Visible = true;
		((Node)node).UniqueNameInOwner = false;
		obj2.RemoveChild((Node)(object)node);
		obj2.AddChild((Node)(object)val2, false, (InternalMode)0);
		obj2.MoveChild((Node)(object)val2, index);
		((Node)val2).Owner = owner;
		((GodotObject)node).Free();
		return val2;
	}

	private static void ApplySelectionBounds(EnemyReplacementProfile profile, NCreatureVisuals candidate, Node2D spineNode)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (!profile.Monster.UseNativeHostBounds)
		{
			EnemySelectionBounds selectionBounds = profile.Monster.SelectionBounds;
			Rect2 bounds = TransformRect(new Rect2(new Vector2(selectionBounds.Left, selectionBounds.Top), new Vector2(selectionBounds.Width, selectionBounds.Height)), spineNode.Transform);
			bounds = ApplyInteractionWidthCap(bounds, profile.Monster.InteractionBounds);
			Control node = ((Node)candidate).GetNode<Control>(NodePath.op_Implicit("%Bounds"));
			Marker2D node2 = ((Node)candidate).GetNode<Marker2D>(NodePath.op_Implicit("%IntentPos"));
			Vector2 val = node.Position + node.Size * 0.5f;
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(((Node2D)node2).Position.X - val.X, ((Node2D)node2).Position.Y - node.Position.Y);
			node.Position = ((Rect2)(ref bounds)).Position;
			node.Size = ((Rect2)(ref bounds)).Size;
			Vector2 val3 = ((Rect2)(ref bounds)).Position + ((Rect2)(ref bounds)).Size * 0.5f;
			((Node2D)node2).Position = new Vector2(val3.X + val2.X, ((Rect2)(ref bounds)).Position.Y + val2.Y);
		}
	}

	private static Rect2 ApplyInteractionWidthCap(Rect2 bounds, EnemyInteractionBoundsAdapter? adapter)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (adapter == null || ((Rect2)(ref bounds)).Size.X <= adapter.MaximumWidth)
		{
			return bounds;
		}
		float num = (((Rect2)(ref bounds)).Size.X - adapter.MaximumWidth) * 0.5f;
		return new Rect2(new Vector2(((Rect2)(ref bounds)).Position.X + num, ((Rect2)(ref bounds)).Position.Y), new Vector2(adapter.MaximumWidth, ((Rect2)(ref bounds)).Size.Y));
	}

	private static Rect2 TransformRect(Rect2 source, Transform2D transform)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = transform * ((Rect2)(ref source)).Position;
		Vector2 val2 = transform * new Vector2(((Rect2)(ref source)).End.X, ((Rect2)(ref source)).Position.Y);
		Vector2 val3 = transform * new Vector2(((Rect2)(ref source)).Position.X, ((Rect2)(ref source)).End.Y);
		Vector2 val4 = transform * ((Rect2)(ref source)).End;
		float num = Mathf.Min(Mathf.Min(val.X, val2.X), Mathf.Min(val3.X, val4.X));
		float num2 = Mathf.Min(Mathf.Min(val.Y, val2.Y), Mathf.Min(val3.Y, val4.Y));
		float num3 = Mathf.Max(Mathf.Max(val.X, val2.X), Mathf.Max(val3.X, val4.X));
		float num4 = Mathf.Max(Mathf.Max(val.Y, val2.Y), Mathf.Max(val3.Y, val4.Y));
		return new Rect2(new Vector2(num, num2), new Vector2(num3 - num, num4 - num2));
	}

	private static void DisableNativeParticles(EnemyReplacementProfile profile, NCreatureVisuals candidate)
	{
		foreach (string item in profile.Monster.NativeParticlePathsToDisable)
		{
			Node nodeOrNull = ((Node)candidate).GetNodeOrNull<Node>(NodePath.op_Implicit(item));
			GpuParticles2D val = (GpuParticles2D)(object)((nodeOrNull is GpuParticles2D) ? nodeOrNull : null);
			if (val == null)
			{
				CpuParticles2D val2 = (CpuParticles2D)(object)((nodeOrNull is CpuParticles2D) ? nodeOrNull : null);
				if (val2 != null)
				{
					val2.Emitting = false;
				}
			}
			else
			{
				val.Emitting = false;
			}
		}
	}
}
