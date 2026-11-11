using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record CharacterSkinCombatEffectsProfile(Vector2 IdleScaleMultiplier, Vector2 IdleSourceScreenOffset, Color IdleModulate, Color BuffModulate, string IdleBackLayerName, string IdleFrontLayerName, string BehindParentLayerName, Vector2I IdleViewportSize, Vector2 IdleViewportOrigin, CombatEffectMaterialPolicyProfile MaterialPolicy, float TargetEffectAlphaMultiplier = 1f)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("IdleScaleMultiplier = ");
		Vector2 val = IdleScaleMultiplier;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", IdleSourceScreenOffset = ");
		val = IdleSourceScreenOffset;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", IdleModulate = ");
		Color val2 = IdleModulate;
		builder.Append(((object)(Color)(ref val2)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", BuffModulate = ");
		val2 = BuffModulate;
		builder.Append(((object)(Color)(ref val2)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", IdleBackLayerName = ");
		builder.Append((object?)IdleBackLayerName);
		builder.Append(", IdleFrontLayerName = ");
		builder.Append((object?)IdleFrontLayerName);
		builder.Append(", BehindParentLayerName = ");
		builder.Append((object?)BehindParentLayerName);
		builder.Append(", IdleViewportSize = ");
		Vector2I idleViewportSize = IdleViewportSize;
		builder.Append(((object)(Vector2I)(ref idleViewportSize)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", IdleViewportOrigin = ");
		val = IdleViewportOrigin;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", MaterialPolicy = ");
		builder.Append(MaterialPolicy);
		builder.Append(", TargetEffectAlphaMultiplier = ");
		builder.Append(TargetEffectAlphaMultiplier.ToString());
		return true;
	}
}
