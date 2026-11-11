using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal readonly struct CombatTargetAnchorSnapshot
{
	public NCreature Node { get; }

	public Node2D? Spine { get; }

	public Vector2 VfxSpawnGlobal { get; }

	public Vector2 HitboxGlobalPosition { get; }

	public Vector2 HitboxSize { get; }

	public Vector2 HitboxTopGlobal { get; }

	public Vector2 HitboxBottomGlobal { get; }

	public Vector2 BoundsSize { get; }

	public Vector2 HitboxCenterGlobal => HitboxGlobalPosition + HitboxSize * 0.5f;

	public CombatTargetAnchorSnapshot(NCreature node, Node2D? spine, Vector2 vfxSpawnGlobal, Vector2 hitboxGlobalPosition, Vector2 hitboxSize, Vector2 hitboxTopGlobal, Vector2 hitboxBottomGlobal, Vector2 boundsSize)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Node = node;
		Spine = spine;
		VfxSpawnGlobal = vfxSpawnGlobal;
		HitboxGlobalPosition = hitboxGlobalPosition;
		HitboxSize = hitboxSize;
		HitboxTopGlobal = hitboxTopGlobal;
		HitboxBottomGlobal = hitboxBottomGlobal;
		BoundsSize = boundsSize;
	}
}
