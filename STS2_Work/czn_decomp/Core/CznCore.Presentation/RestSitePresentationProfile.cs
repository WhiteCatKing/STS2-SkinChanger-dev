using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record RestSitePresentationProfile(string PrimaryAnimationName, IReadOnlyList<string> FallbackAnimationNames, string VoicePath, int VoiceCooldownMs, Vector2 BodyOffset, string BodyBasePositionMeta, IReadOnlyList<RestSiteCampingLayerProfile> CampingLayers, RestSiteSurfaceProfile? Surface = null, IReadOnlyList<string>? HiddenSlotNames = null)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("PrimaryAnimationName = ");
		builder.Append((object?)PrimaryAnimationName);
		builder.Append(", FallbackAnimationNames = ");
		builder.Append(FallbackAnimationNames);
		builder.Append(", VoicePath = ");
		builder.Append((object?)VoicePath);
		builder.Append(", VoiceCooldownMs = ");
		builder.Append(VoiceCooldownMs.ToString());
		builder.Append(", BodyOffset = ");
		Vector2 bodyOffset = BodyOffset;
		builder.Append(((object)(Vector2)(ref bodyOffset)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", BodyBasePositionMeta = ");
		builder.Append((object?)BodyBasePositionMeta);
		builder.Append(", CampingLayers = ");
		builder.Append(CampingLayers);
		builder.Append(", Surface = ");
		builder.Append(Surface);
		builder.Append(", HiddenSlotNames = ");
		builder.Append(HiddenSlotNames);
		return true;
	}
}
