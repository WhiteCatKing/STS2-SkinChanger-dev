using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal readonly record struct CombatCameraShakeKey(float TimeSeconds, Vector2 Offset)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		builder.Append("TimeSeconds = ");
		builder.Append(TimeSeconds.ToString());
		builder.Append(", Offset = ");
		Vector2 offset = Offset;
		builder.Append(((object)(Vector2)(ref offset)/*cast due to .constrained prefix*/).ToString());
		return true;
	}
}
