using System;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class CeremonialBeastPlowMotionAdapter
{
	private sealed class MotionState(Node2D body, Vector2 homePosition)
	{
		public Node2D Body { get; } = body;

		public Vector2 HomePosition { get; } = homePosition;

		public Tween? Tween { get; set; }
	}

	private const double StrongAttackReadySeconds = 0.4667;

	private const double StrongAttackPlaySeconds = 0.6667;

	private const double StrongAttackEndSeconds = 0.6667;

	private const float OfficialReadyBodyPeakY = 113.85f;

	private readonly ConditionalWeakTable<NCreature, MotionState> _motions = new ConditionalWeakTable<NCreature, MotionState>();

	public bool Handle(NCreature creature, EnemyReplacementProfile profile, string trigger)
	{
		if (!(profile.Id != "mountain_god_for_ceremonial_beast"))
		{
			Creature entity = creature.Entity;
			MonsterModel obj = ((entity != null) ? entity.Monster : null);
			CeremonialBeast val = (CeremonialBeast)(object)((obj is CeremonialBeast) ? obj : null);
			if (val != null)
			{
				try
				{
					switch (trigger)
					{
					case "Plow":
						BeginSequence(creature, val, profile);
						return false;
					case "EndPlow":
						return HasActiveSequence(creature);
					case "PlowHit":
					case "Stun":
					case "Dead":
						RestoreImmediately(creature);
						return false;
					}
				}
				catch (Exception ex)
				{
					RestoreImmediately(creature);
					Entry.Log.Warn("Ceremonial Beast Plow motion adapter failed " + $"[profile={profile.Id}, trigger={trigger}, " + "error=" + ex.Message + "].", 1);
				}
				return false;
			}
		}
		return false;
	}

	private void BeginSequence(NCreature creature, CeremonialBeast beast, EnemyReplacementProfile profile)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		if (!TryResolveBody(creature, out Node2D body))
		{
			RestoreImmediately(creature);
			return;
		}
		MotionState state;
		if (_motions.TryGetValue(creature, out MotionState value) && GodotObject.IsInstanceValid((GodotObject)(object)value.Body) && value.Body == body)
		{
			state = value;
			KillTween(state);
			body.Position = state.HomePosition;
		}
		else
		{
			_motions.Remove(creature);
			state = new MotionState(body, body.Position);
			_motions.Add(creature, state);
		}
		Vector2 destinationLocal;
		bool num = TryResolveDestination(beast, body, state.HomePosition, out destinationLocal);
		Tween val = ((Node)body).CreateTween();
		state.Tween = val;
		if (num)
		{
			val.TweenProperty((GodotObject)(object)body, new NodePath("position"), Variant.op_Implicit(destinationLocal), 0.4667).SetTrans((TransitionType)0);
		}
		else
		{
			val.TweenInterval(0.4667);
		}
		val.TweenCallback(Callable.From((Action)delegate
		{
			PlayStrongAttackPresentation(creature, state, profile);
		}));
		val.TweenInterval(0.6667);
		float returnArcHeight = 113.85f * MathF.Abs(profile.Monster.VisualScale);
		if (num)
		{
			val.TweenMethod(Callable.From<float>((Action<float>)delegate(float progress)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				ApplyReturnFrame(creature, state, destinationLocal, returnArcHeight, progress);
			}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), 0.6667).SetTrans((TransitionType)0);
		}
		else
		{
			val.TweenInterval(0.6667);
		}
		val.TweenCallback(Callable.From((Action)delegate
		{
			CompleteSequence(creature, state);
		}));
	}

	private static bool TryResolveDestination(CeremonialBeast beast, Node2D body, Vector2 homePosition, out Vector2 destinationLocal)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		destinationLocal = homePosition;
		if (((MonsterModel)beast).CombatState.IsLiveCombat() && TryResolveTargetRightEdge(beast, out var globalX))
		{
			Node parent = ((Node)body).GetParent();
			Node2D val = (Node2D)(object)((parent is Node2D) ? parent : null);
			if (val != null)
			{
				Vector2 globalPosition = body.GlobalPosition;
				if (globalX >= globalPosition.X || Mathf.IsEqualApprox(globalX, globalPosition.X))
				{
					return false;
				}
				Vector2 val2 = default(Vector2);
				((Vector2)(ref val2))._002Ector(globalX, globalPosition.Y);
				Transform2D globalTransform = ((CanvasItem)val).GetGlobalTransform();
				destinationLocal = ((Transform2D)(ref globalTransform)).AffineInverse() * val2;
				destinationLocal.Y = homePosition.Y;
				return true;
			}
		}
		return false;
	}

	private void PlayStrongAttackPresentation(NCreature creature, MotionState expected, EnemyReplacementProfile profile)
	{
		if (!IsCurrentSequence(creature, expected))
		{
			return;
		}
		try
		{
			Entry.ActionPresentation.Play(creature, profile, EnemyAction.ChargeEnd);
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Mountain God strong-attack presentation failed " + $"[profile={profile.Id}, error={ex.Message}].", 1);
		}
	}

	private void ApplyReturnFrame(NCreature creature, MotionState expected, Vector2 start, float arcHeight, float progress)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (IsCurrentSequence(creature, expected))
		{
			expected.Body.Position = EnemyBodyMotionPath.ParabolicArc(start, expected.HomePosition, arcHeight, progress);
		}
	}

	private void CompleteSequence(NCreature creature, MotionState expected)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (IsCurrentSequence(creature, expected))
		{
			expected.Tween = null;
			expected.Body.Position = expected.HomePosition;
			_motions.Remove(creature);
		}
	}

	private bool HasActiveSequence(NCreature creature)
	{
		if (!_motions.TryGetValue(creature, out MotionState value))
		{
			return false;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)value.Body))
		{
			return true;
		}
		_motions.Remove(creature);
		return false;
	}

	private bool IsCurrentSequence(NCreature creature, MotionState expected)
	{
		if (_motions.TryGetValue(creature, out MotionState value) && value == expected)
		{
			return GodotObject.IsInstanceValid((GodotObject)(object)expected.Body);
		}
		return false;
	}

	private void RestoreImmediately(NCreature creature)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_motions.TryGetValue(creature, out MotionState value))
		{
			KillTween(value);
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Body))
			{
				value.Body.Position = value.HomePosition;
			}
			_motions.Remove(creature);
		}
	}

	private static bool TryResolveBody(NCreature creature, out Node2D body)
	{
		body = null;
		if (creature.Visuals == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals))
		{
			return false;
		}
		Node2D currentBody = creature.Visuals.GetCurrentBody();
		if (!GodotObject.IsInstanceValid((GodotObject)(object)currentBody) || !((Node)currentBody).IsInsideTree())
		{
			return false;
		}
		body = currentBody;
		return true;
	}

	private static bool TryResolveTargetRightEdge(CeremonialBeast beast, out float globalX)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		globalX = 0f;
		float num = 0f;
		int num2 = 0;
		foreach (Creature playerCreature in ((MonsterModel)beast).CombatState.PlayerCreatures)
		{
			if (playerCreature.IsAlive)
			{
				NCreature creatureNode = playerCreature.GetCreatureNode();
				if (creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && ((Node)creatureNode).IsInsideTree())
				{
					num += creatureNode.GetBottomOfHitbox().X + creatureNode.Hitbox.Size.X * 0.5f;
					num2++;
				}
			}
		}
		if (num2 == 0)
		{
			return false;
		}
		globalX = num / (float)num2;
		return float.IsFinite(globalX);
	}

	private static void KillTween(MotionState state)
	{
		try
		{
			Tween? tween = state.Tween;
			if (tween != null)
			{
				tween.Kill();
			}
		}
		catch
		{
		}
		state.Tween = null;
	}
}
