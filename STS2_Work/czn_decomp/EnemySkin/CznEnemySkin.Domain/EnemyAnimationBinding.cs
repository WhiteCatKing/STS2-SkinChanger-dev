using System.Runtime.CompilerServices;
using System.Text;

namespace CznEnemySkin.Domain;

internal sealed record EnemyAnimationBinding(EnemyAction Action, string Sts2AnimationName, string CznAnimationName, bool Loop, EnemyPresentationStance Stance = EnemyPresentationStance.Normal)
{
	internal string TriggerAliasKey { get; } = Sts2AnimationName.Replace("_", string.Empty);

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Action = ");
		builder.Append(Action.ToString());
		builder.Append(", Sts2AnimationName = ");
		builder.Append((object?)Sts2AnimationName);
		builder.Append(", CznAnimationName = ");
		builder.Append((object?)CznAnimationName);
		builder.Append(", Loop = ");
		builder.Append(Loop.ToString());
		builder.Append(", Stance = ");
		builder.Append(Stance.ToString());
		return true;
	}
}
