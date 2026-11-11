using System;
using System.Linq;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class OwlMagistrateAnimatorAdapter
{
	private sealed class FlightState
	{
		public bool IsFlying { get; set; }

		public Node2D? Body { get; set; }

		public Vector2 HomePosition { get; set; }

		public Tween? Motion { get; set; }

		public MegaSprite? Controller { get; set; }
	}

	internal const string ProfileId = "winged_humanface_for_owl_magistrate";

	private const float FlightOffsetY = -94f;

	private const float TakeOffSeconds = 1.25f;

	private const float VerdictReturnSeconds = 0.5f;

	private static readonly ConditionalWeakTable<OwlMagistrate, FlightState> States = new ConditionalWeakTable<OwlMagistrate, FlightState>();

	public static CreatureAnimator Create(OwlMagistrate magistrate, MegaSprite controller, EnemyReplacementProfile profile)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Expected O, but got Unknown
		EnemyAnimationGraphBuilder enemyAnimationGraphBuilder = new EnemyAnimationGraphBuilder(profile, controller);
		AnimState val = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Idle);
		AnimState nextState = StateWithBounds(profile, EnemyPresentationStance.Transformed, EnemyAction.Idle, "FlyingBounds");
		AnimState val2 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Special);
		AnimState val3 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.BasicAttack);
		AnimState val4 = StateWithBounds(profile, EnemyPresentationStance.Normal, EnemyAction.Buff, "FlyingBounds");
		AnimState val5 = StateWithBounds(profile, EnemyPresentationStance.Transformed, EnemyAction.HeavyAttack, "IdleBounds");
		AnimState val6 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Hurt);
		AnimState val7 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Hurt);
		AnimState val8 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Normal, EnemyAction.Death);
		AnimState val9 = enemyAnimationGraphBuilder.State(EnemyPresentationStance.Transformed, EnemyAction.Death);
		RequireAnimations(controller);
		PrepareFlightState(magistrate, controller);
		val2.NextState = val;
		val3.NextState = val;
		val5.NextState = val;
		val6.NextState = val;
		val7.NextState = nextState;
		CreatureAnimator val10 = new CreatureAnimator(val, controller);
		val10.AddAnyState("Attack", val2, (Func<bool>)(() => IsMove(magistrate, EnemyAction.Special)));
		val10.AddAnyState("Attack", val5, (Func<bool>)(() => IsMove(magistrate, EnemyAction.HeavyAttack)));
		val10.AddAnyState("Attack", val3, (Func<bool>)(() => IsMove(magistrate, EnemyAction.BasicAttack)));
		val10.AddAnyState("Attack", val3, (Func<bool>)null);
		val10.AddAnyState("TakeOff", val4, (Func<bool>)null);
		val10.AddAnyState("Hit", val7, (Func<bool>)(() => IsFlying(magistrate)));
		val10.AddAnyState("Hit", val6, (Func<bool>)null);
		val10.AddAnyState("Dead", val9, (Func<bool>)(() => IsFlying(magistrate)));
		val10.AddAnyState("Dead", val8, (Func<bool>)null);
		return val10;
	}

	public static void ObserveTrigger(OwlMagistrate magistrate, string trigger)
	{
		FlightState orCreateValue = States.GetOrCreateValue(magistrate);
		if (trigger == "TakeOff")
		{
			orCreateValue.IsFlying = true;
			MoveBody(orCreateValue, -94f, 1.25f);
			ResetTakeOffWingbeat(orCreateValue);
		}
		else if (trigger == "Attack" && IsMove(magistrate, EnemyAction.HeavyAttack))
		{
			orCreateValue.IsFlying = false;
			MoveBody(orCreateValue, 0f, 0.5f);
		}
		else if (trigger == "Attack" && !orCreateValue.IsFlying)
		{
			MoveBody(orCreateValue, 0f, 0f);
		}
		else if (trigger == "Dead" && !orCreateValue.IsFlying)
		{
			MoveBody(orCreateValue, 0f, 0f);
		}
	}

	private static AnimState StateWithBounds(EnemyReplacementProfile profile, EnemyPresentationStance stance, EnemyAction action, string boundsContainer)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		EnemyAnimationBinding enemyAnimationBinding = profile.Monster.AnimationBindings.Single((EnemyAnimationBinding candidate) => candidate.Stance == stance && candidate.Action == action);
		AnimState val = new AnimState(enemyAnimationBinding.CznAnimationName, enemyAnimationBinding.Loop);
		val.set_BoundsContainer(boundsContainer);
		return val;
	}

	private static void PrepareFlightState(OwlMagistrate magistrate, MegaSprite controller)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		GodotObject boundObject = ((MegaSpineBinding)controller).BoundObject;
		Node2D val = (Node2D)(object)((boundObject is Node2D) ? boundObject : null);
		if (val == null)
		{
			throw new InvalidOperationException("Winged Humanoid Beast requires a Node2D Spine host.");
		}
		FlightState orCreateValue = States.GetOrCreateValue(magistrate);
		KillMotion(orCreateValue);
		orCreateValue.IsFlying = false;
		orCreateValue.Body = val;
		orCreateValue.HomePosition = val.Position;
		orCreateValue.Controller = controller;
	}

	private static void ResetTakeOffWingbeat(FlightState state)
	{
		MegaSprite? controller = state.Controller;
		MegaAnimationState obj = ((controller != null) ? controller.TryGetAnimationState() : null);
		MegaTrackEntry val = ((obj != null) ? obj.GetCurrent(0) : null);
		try
		{
			if (!(((val != null) ? val.GetAnimationName() : null) != "fly_loop"))
			{
				val.SetTrackTime(0f);
				val.SetTimeScale(1f);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void MoveBody(FlightState state, float offsetY, float seconds)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		KillMotion(state);
		Node2D body = state.Body;
		if (body == null || !GodotObject.IsInstanceValid((GodotObject)(object)body))
		{
			return;
		}
		Vector2 val = state.HomePosition + new Vector2(0f, offsetY);
		if (seconds <= 0f)
		{
			body.Position = val;
			return;
		}
		Tween motion = ((Node)body).CreateTween();
		state.Motion = motion;
		motion.TweenProperty((GodotObject)(object)body, new NodePath("position"), Variant.op_Implicit(val), (double)seconds).SetTrans((TransitionType)0);
		motion.TweenCallback(Callable.From((Action)delegate
		{
			if (state.Motion == motion)
			{
				state.Motion = null;
			}
		}));
	}

	private static void KillMotion(FlightState state)
	{
		try
		{
			Tween? motion = state.Motion;
			if (motion != null)
			{
				motion.Kill();
			}
		}
		catch
		{
		}
		state.Motion = null;
	}

	private static bool IsMove(OwlMagistrate magistrate, EnemyAction expected)
	{
		if (Entry.MoveSemantics.TryResolve((MonsterModel)(object)magistrate, "Attack", out var action))
		{
			return action == expected;
		}
		return false;
	}

	private static bool IsFlying(OwlMagistrate magistrate)
	{
		if (States.TryGetValue(magistrate, out FlightState value))
		{
			return value.IsFlying;
		}
		return false;
	}

	private static void RequireAnimations(MegaSprite controller)
	{
		string[] array = new string[4] { "normal_skill1", "normal_skill2", "normal_skill3", "fly_loop" };
		foreach (string text in array)
		{
			if (!controller.HasAnimation(text))
			{
				throw new InvalidOperationException("Winged Humanoid Beast is missing official animation '" + text + "'.");
			}
		}
	}
}
