using System;

namespace ModLaunchManager;

internal struct GlassSpring
{
	internal float Value;

	internal float Velocity;

	internal float Target;

	internal readonly bool Settled
	{
		get
		{
			if (Math.Abs(Value - Target) < 0.001f)
			{
				return Math.Abs(Velocity) < 0.01f;
			}
			return false;
		}
	}

	public GlassSpring(float value)
	{
		Velocity = 0f;
		Value = value;
		Target = value;
	}

	internal void Reset(float value)
	{
		Value = (Target = value);
		Velocity = 0f;
	}

	internal void Step(float seconds, float frequency = 4.5f, float damping = 0.78f)
	{
		if (!(seconds <= 0f))
		{
			float num = (float)Math.PI * 2f * frequency;
			float num2 = damping * num;
			float num3 = num * MathF.Sqrt(1f - damping * damping);
			float num4 = Value - Target;
			float num5 = (Velocity + num2 * num4) / num3;
			float x = num3 * seconds;
			float num6 = MathF.Cos(x);
			float num7 = MathF.Sin(x);
			float num8 = MathF.Exp((0f - num2) * seconds);
			Value = Target + num8 * (num4 * num6 + num5 * num7);
			Velocity = num8 * ((num5 * num3 - num2 * num4) * num6 - (num4 * num3 + num2 * num5) * num7);
			if (Settled)
			{
				Reset(Target);
			}
		}
	}
}
