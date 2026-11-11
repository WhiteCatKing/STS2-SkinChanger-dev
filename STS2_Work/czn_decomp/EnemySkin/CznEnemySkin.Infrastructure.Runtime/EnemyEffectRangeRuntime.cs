using System;
using System.Linq;
using Godot;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyEffectRangeRuntime
{
	private readonly Random _random = new Random();

	private static readonly string[] RangeKeys = new string[6] { "czn_cfx_x_range", "czn_cfx_y_range", "czn_cfx_rotation_range", "czn_cfx_uniform_scale_range", "czn_cfx_scale_x_range", "czn_cfx_scale_y_range" };

	internal static float Sample(float minimum, float maximum, Random random)
	{
		if (!float.IsFinite(minimum) || !float.IsFinite(maximum) || maximum < minimum)
		{
			throw new ArgumentException("Invalid official effect range.");
		}
		if (minimum != maximum)
		{
			return minimum + (maximum - minimum) * random.NextSingle();
		}
		return minimum;
	}

	public void Apply(Node2D root)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		Variant meta = ((GodotObject)root).GetMeta(StringName.op_Implicit("czn_cfx_sample_ranges"), Variant.op_Implicit(false));
		if (!((Variant)(ref meta)).AsBool())
		{
			return;
		}
		foreach (Node node in ((Node)root).FindChildren("*", "Node2D", true, false))
		{
			Node2D layer;
			if (RangeKeys.Any((string key) => ((GodotObject)node).HasMeta(StringName.op_Implicit(key))))
			{
				layer = (Node2D)node;
				layer.Position = new Vector2(Read("czn_cfx_x_range", layer.Position.X), Read("czn_cfx_y_range", layer.Position.Y, reverse: true));
				layer.RotationDegrees = Read("czn_cfx_rotation_range", layer.RotationDegrees, reverse: true);
				if (((GodotObject)layer).HasMeta(StringName.op_Implicit("czn_cfx_uniform_scale_range")))
				{
					float num = Read("czn_cfx_uniform_scale_range", 1f);
					layer.Scale = new Vector2(num, num);
				}
				else
				{
					layer.Scale = new Vector2(Read("czn_cfx_scale_x_range", layer.Scale.X), Read("czn_cfx_scale_y_range", layer.Scale.Y));
				}
			}
			float Read(string key, float fallback, bool reverse = false)
			{
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_003a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0040: Unknown result type (might be due to invalid IL or missing references)
				if (!((GodotObject)layer).HasMeta(StringName.op_Implicit(key)))
				{
					return fallback;
				}
				Node2D obj = layer;
				StringName obj2 = StringName.op_Implicit(key);
				Variant val = default(Variant);
				val = ((GodotObject)obj).GetMeta(obj2, val);
				Vector2 val2 = ((Variant)(ref val)).AsVector2();
				float num2 = Sample(val2.X, val2.Y, _random);
				if (!reverse)
				{
					return num2;
				}
				return 0f - num2;
			}
		}
	}
}
