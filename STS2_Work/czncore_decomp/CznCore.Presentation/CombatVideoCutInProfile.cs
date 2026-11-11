using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record CombatVideoCutInProfile(string StateId, string VideoPath, Vector2 SourceSize, float LifetimeSeconds, float DimAlpha = 0f, float FadeInSeconds = 0f, float FadeOutDelaySeconds = 0f, float FadeOutSeconds = 0f, float StartDelaySeconds = 0f, int CanvasLayer = 980)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("StateId = ");
		builder.Append((object?)StateId);
		builder.Append(", VideoPath = ");
		builder.Append((object?)VideoPath);
		builder.Append(", SourceSize = ");
		Vector2 sourceSize = SourceSize;
		builder.Append(((object)(Vector2)(ref sourceSize)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", LifetimeSeconds = ");
		builder.Append(LifetimeSeconds.ToString());
		builder.Append(", DimAlpha = ");
		builder.Append(DimAlpha.ToString());
		builder.Append(", FadeInSeconds = ");
		builder.Append(FadeInSeconds.ToString());
		builder.Append(", FadeOutDelaySeconds = ");
		builder.Append(FadeOutDelaySeconds.ToString());
		builder.Append(", FadeOutSeconds = ");
		builder.Append(FadeOutSeconds.ToString());
		builder.Append(", StartDelaySeconds = ");
		builder.Append(StartDelaySeconds.ToString());
		builder.Append(", CanvasLayer = ");
		builder.Append(CanvasLayer.ToString());
		return true;
	}
}
