using System;
using System.Collections.Generic;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznCore.Presentation;

internal static class GameOverVisualScaleRuntime
{
	public static int ScaleStandaloneSpineSprites(Node root, string scaleMeta, float scaleMultiplier, float maxStandaloneUnscaledSpineScale)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Stack<Node> stack = new Stack<Node>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node val = stack.Pop();
			foreach (Node child in val.GetChildren(false))
			{
				stack.Push(child);
			}
			Node2D val2 = (Node2D)(object)((val is Node2D) ? val : null);
			if (val2 != null && !(((GodotObject)val).GetClass() != "SpineSprite") && !((GodotObject)val).HasMeta(StringName.op_Implicit(scaleMeta)))
			{
				Vector2 scale = val2.Scale;
				if (!(Math.Max(Math.Abs(scale.X), Math.Abs(scale.Y)) > maxStandaloneUnscaledSpineScale))
				{
					val2.Scale = CharacterSkinSurfaceDefaults.ResolveCznCombatBodySpineScale(scale, scaleMultiplier);
					StringName obj = StringName.op_Implicit(scaleMeta);
					bool flag = true;
					((GodotObject)val).SetMeta(obj, Variant.From<bool>(ref flag));
					num++;
				}
			}
		}
		return num;
	}

	public static void ScheduleStandaloneDeathSequence(Node root, string deathSequenceMeta, IReadOnlyList<string> deathSequence, Action<string, int> info, Action<string, int> warn)
	{
		ArgumentNullException.ThrowIfNull(root, "root");
		ArgumentNullException.ThrowIfNull(deathSequence, "deathSequence");
		ArgumentNullException.ThrowIfNull(info, "info");
		ArgumentNullException.ThrowIfNull(warn, "warn");
		GodotScheduler.Deferred(delegate
		{
			PlayStandaloneDeathSequence(root, deathSequenceMeta, deathSequence, info, warn);
		});
	}

	private static void PlayStandaloneDeathSequence(Node root, string deathSequenceMeta, IReadOnlyList<string> deathSequence, Action<string, int> info, Action<string, int> warn)
	{
		try
		{
			if (TryPlayStandaloneDeathSequence(root, deathSequenceMeta, deathSequence, warn, out var pendingSpine) > 0)
			{
				info("Player game-over death animation sequence started: " + string.Join(" -> ", deathSequence), 2);
			}
			else if (pendingSpine)
			{
				warn("Player game-over death animation skipped: standalone SpineSprite animation state not ready.", 2);
			}
		}
		catch (Exception ex)
		{
			warn("Player game-over death animation failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	private static int TryPlayStandaloneDeathSequence(Node root, string deathSequenceMeta, IReadOnlyList<string> deathSequence, Action<string, int> warn, out bool pendingSpine)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		pendingSpine = false;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)root) || deathSequence.Count == 0)
		{
			return 0;
		}
		int num = 0;
		Stack<Node> stack = new Stack<Node>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node val = stack.Pop();
			foreach (Node child in val.GetChildren(false))
			{
				stack.Push(child);
			}
			Node2D val2 = (Node2D)(object)((val is Node2D) ? val : null);
			if (val2 == null || ((GodotObject)val).GetClass() != "SpineSprite" || ((GodotObject)val).HasMeta(StringName.op_Implicit(deathSequenceMeta)))
			{
				continue;
			}
			MegaSprite val3 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val2));
			MegaAnimationState val4 = val3.TryGetAnimationState();
			if (val4 == null)
			{
				pendingSpine = true;
				continue;
			}
			bool flag;
			if (!HasStandaloneDeathSequence(val3, deathSequence, warn))
			{
				StringName obj = StringName.op_Implicit(deathSequenceMeta);
				flag = true;
				((GodotObject)val).SetMeta(obj, Variant.From<bool>(ref flag));
				continue;
			}
			SpineAnimationCompat.SetAnimation(val4, deathSequence[0], loop: false);
			for (int i = 1; i < deathSequence.Count; i++)
			{
				SpineAnimationCompat.AddAnimation(val4, deathSequence[i], 0f, loop: false);
			}
			StringName obj2 = StringName.op_Implicit(deathSequenceMeta);
			flag = true;
			((GodotObject)val).SetMeta(obj2, Variant.From<bool>(ref flag));
			num++;
		}
		return num;
	}

	private static bool HasStandaloneDeathSequence(MegaSprite sprite, IReadOnlyList<string> deathSequence, Action<string, int> warn)
	{
		foreach (string item in deathSequence)
		{
			if (!sprite.HasAnimation(item))
			{
				warn("Player game-over death animation missing body animation: " + item, 2);
				return false;
			}
		}
		return true;
	}
}
