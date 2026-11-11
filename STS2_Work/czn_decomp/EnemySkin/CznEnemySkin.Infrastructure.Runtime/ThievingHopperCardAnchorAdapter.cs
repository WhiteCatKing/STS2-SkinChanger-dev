using CznEnemySkin.Domain;
using Godot;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class ThievingHopperCardAnchorAdapter
{
	private const string TargetMonsterId = "THIEVING_HOPPER";

	private const string SourceAnchorPath = "CardBoneNode";

	private const string ReplacementAnchorName = "CznEnemySkinStolenCardAnchor";

	public static void Prepare(EnemyReplacementProfile profile, Node2D spineNode, Vector2 nativeHostScale)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		if (profile.Target.MonsterId != "THIEVING_HOPPER")
		{
			return;
		}
		Node2D nodeOrNull = ((Node)spineNode).GetNodeOrNull<Node2D>(NodePath.op_Implicit("CardBoneNode"));
		if (nodeOrNull == null)
		{
			return;
		}
		Node parent = ((Node)nodeOrNull).GetParent();
		if (parent == null)
		{
			return;
		}
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(nativeHostScale.X / spineNode.Scale.X, nativeHostScale.Y / spineNode.Scale.Y);
		Node2D val2 = new Node2D
		{
			Name = StringName.op_Implicit("CznEnemySkinStolenCardAnchor"),
			Position = nodeOrNull.Position * val,
			Rotation = nodeOrNull.Rotation,
			Skew = nodeOrNull.Skew,
			Scale = nodeOrNull.Scale * val,
			ZIndex = ((CanvasItem)nodeOrNull).ZIndex,
			ZAsRelative = ((CanvasItem)nodeOrNull).ZAsRelative,
			ShowBehindParent = ((CanvasItem)nodeOrNull).ShowBehindParent
		};
		int index = ((Node)nodeOrNull).GetIndex(false);
		Node owner = ((Node)nodeOrNull).Owner;
		parent.AddChild((Node)(object)val2, false, (InternalMode)0);
		parent.MoveChild((Node)(object)val2, index);
		((Node)val2).Owner = owner;
		foreach (Node child in ((Node)nodeOrNull).GetChildren(false))
		{
			((Node)nodeOrNull).RemoveChild(child);
			((Node)val2).AddChild(child, false, (InternalMode)0);
			child.Owner = owner;
		}
		parent.RemoveChild((Node)(object)nodeOrNull);
		((GodotObject)nodeOrNull).Free();
	}
}
