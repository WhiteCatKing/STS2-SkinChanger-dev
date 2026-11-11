using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal readonly record struct CombatBodyEntryRunInStart(bool Started, bool UsesSegmentOffsets, Vector2 TargetPosition, int Generation)
{
	public static CombatBodyEntryRunInStart None { get; } = new CombatBodyEntryRunInStart(Started: false, UsesSegmentOffsets: false, Vector2.Zero, 0);

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		builder.Append("Started = ");
		builder.Append(Started.ToString());
		builder.Append(", UsesSegmentOffsets = ");
		builder.Append(UsesSegmentOffsets.ToString());
		builder.Append(", TargetPosition = ");
		Vector2 targetPosition = TargetPosition;
		builder.Append(((object)(Vector2)(ref targetPosition)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", Generation = ");
		builder.Append(Generation.ToString());
		return true;
	}
}
