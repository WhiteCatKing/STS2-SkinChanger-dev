using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record BattleReadyOverlayProfile(string RuntimeNodeNamePrefix, string ScenePath, string SkeletonDataPath, BattleReadyOverlayAnimations Animations, float OutDelaySeconds, float CancelOutDelaySeconds, float SlideOffsetX, float SlideDurationSeconds, BattleReadyOverlayIdleEffectPlan? IdleEffect, IReadOnlyDictionary<CombatPresentationCardRoute, string> CardAnimations, float CardUseOutDelaySeconds = -1f, float? CardUseSlideDurationSeconds = null)
{
	public Vector2 LayoutOffset { get; init; } = Vector2.Zero;

	public string RuntimeNodeName(string suffix)
	{
		return RuntimeNodeNamePrefix + suffix;
	}

	public bool TryGetCardAnimation(CombatPresentationCardRoute route, out string animationName)
	{
		return LeftSideCardAnimationResolver.TryResolve(CardAnimations, route, out animationName);
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("RuntimeNodeNamePrefix = ");
		builder.Append((object?)RuntimeNodeNamePrefix);
		builder.Append(", ScenePath = ");
		builder.Append((object?)ScenePath);
		builder.Append(", SkeletonDataPath = ");
		builder.Append((object?)SkeletonDataPath);
		builder.Append(", Animations = ");
		builder.Append(Animations);
		builder.Append(", OutDelaySeconds = ");
		builder.Append(OutDelaySeconds.ToString());
		builder.Append(", CancelOutDelaySeconds = ");
		builder.Append(CancelOutDelaySeconds.ToString());
		builder.Append(", SlideOffsetX = ");
		builder.Append(SlideOffsetX.ToString());
		builder.Append(", SlideDurationSeconds = ");
		builder.Append(SlideDurationSeconds.ToString());
		builder.Append(", IdleEffect = ");
		builder.Append(IdleEffect);
		builder.Append(", CardAnimations = ");
		builder.Append(CardAnimations);
		builder.Append(", CardUseOutDelaySeconds = ");
		builder.Append(CardUseOutDelaySeconds.ToString());
		builder.Append(", CardUseSlideDurationSeconds = ");
		builder.Append(CardUseSlideDurationSeconds.ToString());
		builder.Append(", LayoutOffset = ");
		Vector2 layoutOffset = LayoutOffset;
		builder.Append(((object)(Vector2)(ref layoutOffset)/*cast due to .constrained prefix*/).ToString());
		return true;
	}
}
