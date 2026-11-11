using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal readonly record struct CombatRadialRgbBlurSpec(float Force, float Force2, float Sampling, Vector2 MaskSize, bool GlowMask, float RiseSeconds, string RiseCurve, float HoldSeconds, float FallSeconds, float EndValue, string FallCurve)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		builder.Append("Force = ");
		builder.Append(Force.ToString());
		builder.Append(", Force2 = ");
		builder.Append(Force2.ToString());
		builder.Append(", Sampling = ");
		builder.Append(Sampling.ToString());
		builder.Append(", MaskSize = ");
		Vector2 maskSize = MaskSize;
		builder.Append(((object)(Vector2)(ref maskSize)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", GlowMask = ");
		builder.Append(GlowMask.ToString());
		builder.Append(", RiseSeconds = ");
		builder.Append(RiseSeconds.ToString());
		builder.Append(", RiseCurve = ");
		builder.Append((object?)RiseCurve);
		builder.Append(", HoldSeconds = ");
		builder.Append(HoldSeconds.ToString());
		builder.Append(", FallSeconds = ");
		builder.Append(FallSeconds.ToString());
		builder.Append(", EndValue = ");
		builder.Append(EndValue.ToString());
		builder.Append(", FallCurve = ");
		builder.Append((object?)FallCurve);
		return true;
	}
}
