using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record CombatEffectMaterialPolicyProfile(Color IdleTint, float IdleBrightness, float IdleAlphaBoost, float IdleAlphaPower, Color AttackTint, float AttackBrightness, float AttackAlphaBoost, float AttackAlphaPower, float IdleBackLayerAlpha, CombatEffectViewportMaterialProfile IdleBackViewport, CombatEffectViewportMaterialProfile IdleFrontViewport)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("IdleTint = ");
		Color val = IdleTint;
		builder.Append(((object)(Color)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", IdleBrightness = ");
		builder.Append(IdleBrightness.ToString());
		builder.Append(", IdleAlphaBoost = ");
		builder.Append(IdleAlphaBoost.ToString());
		builder.Append(", IdleAlphaPower = ");
		builder.Append(IdleAlphaPower.ToString());
		builder.Append(", AttackTint = ");
		val = AttackTint;
		builder.Append(((object)(Color)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", AttackBrightness = ");
		builder.Append(AttackBrightness.ToString());
		builder.Append(", AttackAlphaBoost = ");
		builder.Append(AttackAlphaBoost.ToString());
		builder.Append(", AttackAlphaPower = ");
		builder.Append(AttackAlphaPower.ToString());
		builder.Append(", IdleBackLayerAlpha = ");
		builder.Append(IdleBackLayerAlpha.ToString());
		builder.Append(", IdleBackViewport = ");
		builder.Append(IdleBackViewport);
		builder.Append(", IdleFrontViewport = ");
		builder.Append(IdleFrontViewport);
		return true;
	}
}
