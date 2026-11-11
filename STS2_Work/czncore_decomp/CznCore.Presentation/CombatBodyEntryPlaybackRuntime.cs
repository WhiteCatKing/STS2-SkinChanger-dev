using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatBodyEntryPlaybackRuntime
{
	private const string EntryRunInGenerationMeta = "czn_combat_entry_run_in_generation";

	private const string EntryRunInTargetPositionMeta = "czn_combat_entry_run_in_target_position";

	private const float EntryRunInCompletionPadSeconds = 0.08f;

	private static int _entryRunInGeneration;

	private static readonly ConditionalWeakTable<NCreature, CombatEntrySegmentPlayback> SegmentPlayback = new ConditionalWeakTable<NCreature, CombatEntrySegmentPlayback>();

	private static readonly CombatCreatureSpineResolver Spines = new CombatCreatureSpineResolver();

	public static IReadOnlyList<CombatPresentationBodyStep> BuildEntrySteps(CharacterSkinCombatBodyProfile profile)
	{
		return BuildBodySteps(profile.EntrySequence);
	}

	public static IReadOnlyList<CombatPresentationBodyStep> BuildBodySteps(IReadOnlyList<string> animations)
	{
		if (animations.Count == 0)
		{
			return Array.Empty<CombatPresentationBodyStep>();
		}
		CombatPresentationBodyStep[] array = new CombatPresentationBodyStep[animations.Count];
		for (int i = 0; i < animations.Count; i++)
		{
			array[i] = new CombatPresentationBodyStep(animations[i], null, null);
		}
		return array;
	}

	public static bool IsEntrySequence(CharacterSkinCombatBodyProfile profile, IReadOnlyList<CombatPresentationBodyStep> steps)
	{
		if (profile.EntrySequence.Length == 0 || steps.Count < profile.EntrySequence.Length)
		{
			return false;
		}
		for (int i = 0; i < profile.EntrySequence.Length; i++)
		{
			if (!string.Equals(steps[i].BodyAnimation, profile.EntrySequence[i], StringComparison.Ordinal))
			{
				return false;
			}
		}
		return true;
	}

	public static CombatBodyEntryRunInStart TryBeginRunIn(CharacterSkinCombatBodyProfile profile, IReadOnlyList<CombatPresentationBodyStep> steps, NCreature actorNode, Action<string>? debug = null, Action<string>? warn = null)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!HasRunIn(profile) || actorNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)actorNode))
			{
				return CombatBodyEntryRunInStart.None;
			}
			if (!IsEntrySequence(profile, steps))
			{
				CompleteActiveRunIn(actorNode, "non_entry_sequence", debug, warn);
				return CombatBodyEntryRunInStart.None;
			}
			CompleteActiveRunIn(actorNode, "entry_restart", debug, warn);
			Vector2 position = ((Node2D)actorNode.Visuals).Position;
			int generation = BeginRunIn(actorNode, position);
			if (profile.EntryRunInSegmentOffsets.Length != 0)
			{
				Node2D spine = Spines.Find(actorNode) ?? throw new InvalidOperationException("Entry Spine is unavailable");
				CombatBodyEntryRunInStart runIn = new CombatBodyEntryRunInStart(Started: true, UsesSegmentOffsets: false, position, generation);
				SegmentPlayback.Add(actorNode, new CombatEntrySegmentPlayback(spine, (Node2D)(object)actorNode.Visuals, position, profile.EntrySequence, profile.EntryRunInSegmentOffsets, delegate
				{
					CompleteRunInIfCurrent(actorNode, runIn, "animation_transition", debug, warn);
				}));
				ApplySegmentOffset(actorNode, position, profile.EntryRunInSegmentOffsets[0]);
				debug?.Invoke("Combat entry segment offset started: " + ((object)(Vector2)(ref profile.EntryRunInSegmentOffsets[0])/*cast due to .constrained prefix*/).ToString());
				return runIn;
			}
			((Node2D)actorNode.Visuals).Position = position + new Vector2(profile.EntryRunInOffsetX, 0f);
			Tween obj = ((Node)actorNode.Visuals).CreateTween();
			obj.SetEase((EaseType)1);
			obj.SetTrans((TransitionType)7);
			obj.TweenProperty((GodotObject)(object)actorNode.Visuals, NodePath.op_Implicit("position:x"), Variant.op_Implicit(position.X), (double)profile.EntryRunInDurationSeconds);
			debug?.Invoke("Combat entry run-in started: " + profile.EntryRunInOffsetX + " -> 0");
			return new CombatBodyEntryRunInStart(Started: true, UsesSegmentOffsets: false, position, generation);
		}
		catch (Exception ex)
		{
			CompleteActiveRunIn(actorNode, "entry_failed", debug, warn);
			warn?.Invoke("Combat entry run-in failed: " + SkinLog.FormatException(ex));
			return CombatBodyEntryRunInStart.None;
		}
	}

	public static bool TryGetSegmentOffset(CharacterSkinCombatBodyProfile profile, int stepIndex, out Vector2 offset)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (stepIndex <= 0 || stepIndex >= profile.EntryRunInSegmentOffsets.Length)
		{
			offset = default(Vector2);
			return false;
		}
		offset = profile.EntryRunInSegmentOffsets[stepIndex];
		return true;
	}

	public static void ApplySegmentOffset(NCreature actorNode, Vector2 targetPosition, Vector2 offset)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (actorNode != null && GodotObject.IsInstanceValid((GodotObject)(object)actorNode))
		{
			((Node2D)actorNode.Visuals).Position = targetPosition + offset;
		}
	}

	public static void ScheduleCompletion(NCreature actorNode, CombatBodyEntryRunInStart runIn, float sequenceDelaySeconds, Action<string>? debug = null, Action<string>? warn = null)
	{
		if (!runIn.Started || actorNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)actorNode) || SegmentPlayback.TryGetValue(actorNode, out CombatEntrySegmentPlayback _))
		{
			return;
		}
		float seconds = Math.Max(0.05f, sequenceDelaySeconds + 0.08f);
		try
		{
			GodotScheduler.Delay(seconds, delegate
			{
				CompleteRunInIfCurrent(actorNode, runIn, "scheduled_completion", debug, warn);
			});
		}
		catch (Exception ex)
		{
			warn?.Invoke("Combat entry run-in completion scheduling failed: " + SkinLog.FormatException(ex));
			CompleteRunInIfCurrent(actorNode, runIn, "completion_schedule_failed", debug, warn);
		}
	}

	public static void CompleteRunInIfCurrent(NCreature actorNode, CombatBodyEntryRunInStart runIn, string reason, Action<string>? debug = null, Action<string>? warn = null)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (!runIn.Started || runIn.Generation <= 0)
		{
			return;
		}
		try
		{
			if (actorNode != null && GodotObject.IsInstanceValid((GodotObject)(object)actorNode))
			{
				if (IsRunInGenerationCurrent((GodotObject)(object)actorNode, runIn.Generation))
				{
					((Node2D)actorNode.Visuals).Position = runIn.TargetPosition;
					ClearRunInMeta((GodotObject)(object)actorNode);
					debug?.Invoke("Combat entry run-in completed: " + reason);
				}
			}
		}
		catch (Exception ex)
		{
			warn?.Invoke("Combat entry run-in completion failed: " + SkinLog.FormatException(ex));
		}
	}

	public static void CompleteActiveRunIn(NCreature actorNode, string reason, Action<string>? debug = null, Action<string>? warn = null)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (actorNode != null && GodotObject.IsInstanceValid((GodotObject)(object)actorNode))
			{
				if (((GodotObject)actorNode).HasMeta(StringName.op_Implicit("czn_combat_entry_run_in_target_position")))
				{
					StringName obj = StringName.op_Implicit("czn_combat_entry_run_in_target_position");
					Variant val = default(Variant);
					val = ((GodotObject)actorNode).GetMeta(obj, val);
					Vector2 position = ((Variant)(ref val)).AsVector2();
					((Node2D)actorNode.Visuals).Position = position;
					ClearRunInMeta((GodotObject)(object)actorNode);
					debug?.Invoke("Combat entry run-in completed: " + reason);
				}
			}
		}
		catch (Exception ex)
		{
			warn?.Invoke("Combat entry run-in completion failed: " + SkinLog.FormatException(ex));
		}
	}

	public static bool HasRunIn(CharacterSkinCombatBodyProfile profile)
	{
		if (profile.EntryRunInSegmentOffsets.Length == 0)
		{
			if (profile.EntryRunInDurationSeconds > 0f)
			{
				return profile.EntryRunInOffsetX != 0f;
			}
			return false;
		}
		return true;
	}

	private static int BeginRunIn(NCreature actorNode, Vector2 targetPosition)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		int result = NextRunInGeneration();
		((GodotObject)actorNode).SetMeta(StringName.op_Implicit("czn_combat_entry_run_in_generation"), Variant.From<int>(ref result));
		((GodotObject)actorNode).SetMeta(StringName.op_Implicit("czn_combat_entry_run_in_target_position"), Variant.From<Vector2>(ref targetPosition));
		return result;
	}

	private static int NextRunInGeneration()
	{
		_entryRunInGeneration++;
		if (_entryRunInGeneration <= 0)
		{
			_entryRunInGeneration = 1;
		}
		return _entryRunInGeneration;
	}

	private static bool IsRunInGenerationCurrent(GodotObject actorObject, int generation)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (actorObject.HasMeta(StringName.op_Implicit("czn_combat_entry_run_in_generation")))
		{
			StringName obj = StringName.op_Implicit("czn_combat_entry_run_in_generation");
			Variant val = default(Variant);
			val = actorObject.GetMeta(obj, val);
			return ((Variant)(ref val)).AsInt32() == generation;
		}
		return false;
	}

	private static void ClearRunInMeta(GodotObject actorObject)
	{
		NCreature val = (NCreature)(object)((actorObject is NCreature) ? actorObject : null);
		if (val != null && SegmentPlayback.TryGetValue(val, out CombatEntrySegmentPlayback value))
		{
			value.Dispose();
			SegmentPlayback.Remove(val);
		}
		if (actorObject.HasMeta(StringName.op_Implicit("czn_combat_entry_run_in_generation")))
		{
			actorObject.RemoveMeta(StringName.op_Implicit("czn_combat_entry_run_in_generation"));
		}
		if (actorObject.HasMeta(StringName.op_Implicit("czn_combat_entry_run_in_target_position")))
		{
			actorObject.RemoveMeta(StringName.op_Implicit("czn_combat_entry_run_in_target_position"));
		}
	}
}
