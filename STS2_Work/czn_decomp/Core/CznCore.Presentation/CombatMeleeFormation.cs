using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatMeleeFormation
{
	public static Vector2 ResolveApproachDelta(NCreature ownerFormationAnchor, NCreature targetFormationAnchor, float horizontalTravel)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Control)targetFormationAnchor).GlobalPosition.Y - ((Control)ownerFormationAnchor).GlobalPosition.Y;
		return new Vector2(horizontalTravel, num);
	}
}
