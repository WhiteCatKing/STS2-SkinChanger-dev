using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznCore.Presentation;

internal static class NativeCombatBodyRecovery
{
	public static void Bind(MegaSprite sprite, Creature creature, Func<bool> canRestore, Func<float> restore, Action complete, Action<string> warn)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		GodotObject boundObject = ((MegaSpineBinding)sprite).BoundObject;
		Node node = (Node)(object)((boundObject is Node) ? boundObject : null);
		if (node == null || ((GodotObject)node).HasMeta(StringName.op_Implicit("czn_body_recovery")))
		{
			return;
		}
		int generation = 0;
		int exitGeneration = -1;
		string previous = null;
		Callable val2 = Callable.From<GodotObject, GodotObject, GodotObject>((Action<GodotObject, GodotObject, GodotObject>)delegate(GodotObject _, GodotObject _, GodotObject nativeTrack)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			try
			{
				MegaTrackEntry val = new MegaTrackEntry(Variant.op_Implicit(nativeTrack));
				int token;
				try
				{
					string animationName = val.GetAnimationName();
					token = ++generation;
					string text = previous;
					previous = animationName;
					bool num = exitGeneration == token - 1 && text == "b_idle_to_idle" && animationName == "idle";
					exitGeneration = -1;
					if (num && creature.IsAlive && canRestore())
					{
						complete();
					}
					if (!(animationName != "b_idle") && !(text == "idle_to_b_idle"))
					{
						CombatBodyRecoveryRuntime.ScheduleExit(1f, Current, delegate
						{
							float result = restore();
							token = generation;
							exitGeneration = token;
							return result;
						}, delegate
						{
							exitGeneration = -1;
							complete();
						}, warn);
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				bool Current()
				{
					if (token == generation && GodotObject.IsInstanceValid((GodotObject)(object)node) && node.IsInsideTree() && creature.IsAlive)
					{
						return canRestore();
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				warn("Native body recovery failed: " + ex.Message);
			}
		});
		if ((int)sprite.ConnectAnimationStarted(val2) != 0)
		{
			warn("Cannot bind native body recovery");
			return;
		}
		((GodotObject)node).SetMeta(StringName.op_Implicit("czn_body_recovery"), Variant.op_Implicit(true));
		node.TreeExiting += delegate
		{
			int num2 = generation + 1;
			generation = num2;
		};
	}
}
