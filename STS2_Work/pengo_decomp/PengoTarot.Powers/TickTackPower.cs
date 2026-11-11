using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.GameActions;

namespace PengoTarot.Powers;

public sealed class TickTackPower : PowerModel
{
	private static readonly HashSet<TickTackPower> ActiveEffects = new HashSet<TickTackPower>();

	private const string FlashSfx = "event:/sfx/ui/clicks/ui_checkbox_on";

	private const float MinFlashInterval = 0.2f;

	private const float MaxFlashInterval = 1f;

	private const float TickInterval = 1f;

	private static readonly Color RedTint = new Color(1f, 0.2f, 0.2f, 1f);

	private Timer? _redFlashTimer;

	private Timer? _tickTimer;

	private Timer? _vfxTimer;

	private Tween? _flashTween;

	private NCreature? _creatureNode;

	private readonly List<float> _flashIntervals = new List<float>();

	private int _flashIndex;

	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	public override PowerInstanceType InstanceType => (PowerInstanceType)1;

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		BuildFlashSchedule();
		Callable val3 = Callable.From((Action)delegate
		{
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Invalid comparison between Unknown and I4
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Invalid comparison between Unknown and I4
			if (((AbstractModel)this).IsMutable)
			{
				NCombatRoom instance = NCombatRoom.Instance;
				NCreature val = ((instance != null) ? instance.GetCreatureNode(((PowerModel)this).Owner) : null);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					_creatureNode = val;
					ActiveEffects.Add(this);
					StartRedFlash(val);
					StartVfxTimer(val);
					INetGameService netService = RunManager.Instance.NetService;
					NetGameType? val2 = ((netService != null) ? new NetGameType?(netService.Type) : ((NetGameType?)null));
					if ((int)val2.GetValueOrDefault() == 2 || (int)val2.GetValueOrDefault() == 1)
					{
						StartTickTimer(val);
					}
				}
			}
		});
		((Callable)(ref val3)).CallDeferred(Array.Empty<Variant>());
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		ActiveEffects.Remove(this);
		StopRedFlash();
		StopTickTimer();
		StopVfxTimer();
		return Task.CompletedTask;
	}

	internal static void StopAllEffectsForRoomTransition()
	{
		foreach (TickTackPower item in new List<TickTackPower>(ActiveEffects))
		{
			item.StopRedFlash();
			item.StopTickTimer();
			item.StopVfxTimer();
		}
		ActiveEffects.Clear();
	}

	public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)side == 1 && ((AbstractModel)this).IsMutable && ((PowerModel)this).Amount > 0)
		{
			RemoveSilently();
		}
		return Task.CompletedTask;
	}

	private async Task RemoveSilently()
	{
		if (((AbstractModel)this).IsMutable && ((PowerModel)this).Amount > 0)
		{
			StopTickTimer();
			await PowerCmd.Remove((PowerModel)(object)this);
		}
	}

	private void StartRedFlash(NCreature nCreature)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		_redFlashTimer = new Timer
		{
			OneShot = true
		};
		_redFlashTimer.Timeout += OnFlashTick;
		GodotTreeExtensions.AddChildSafely((Node)(object)nCreature, (Node)(object)_redFlashTimer);
		ScheduleNextFlash();
	}

	private void StopRedFlash()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		ActiveEffects.Remove(this);
		Timer? redFlashTimer = _redFlashTimer;
		if (redFlashTimer != null)
		{
			redFlashTimer.Stop();
		}
		Timer? redFlashTimer2 = _redFlashTimer;
		if (redFlashTimer2 != null)
		{
			((Node)redFlashTimer2).QueueFree();
		}
		_redFlashTimer = null;
		Tween? flashTween = _flashTween;
		if (flashTween != null)
		{
			flashTween.Kill();
		}
		_flashTween = null;
		_flashIntervals.Clear();
		_flashIndex = 0;
		if (_creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_creatureNode))
		{
			NCreatureVisuals visuals = _creatureNode.Visuals;
			if (visuals != null && GodotObject.IsInstanceValid((GodotObject)(object)visuals))
			{
				((CanvasItem)visuals).Modulate = Colors.White;
			}
		}
		_creatureNode = null;
	}

	private void OnFlashTick()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (((AbstractModel)this).IsMutable && ((PowerModel)this).Amount > 0)
		{
			CombatManager instance = CombatManager.Instance;
			if (instance != null && instance.IsInProgress && _creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_creatureNode))
			{
				NCreatureVisuals visuals = _creatureNode.Visuals;
				if (visuals != null && GodotObject.IsInstanceValid((GodotObject)(object)visuals))
				{
					Color val = ((((CanvasItem)visuals).Modulate != Colors.White) ? Colors.White : RedTint);
					Tween val2 = ((Node)visuals).CreateTween();
					val2.TweenProperty((GodotObject)(object)visuals, NodePath.op_Implicit("modulate"), Variant.op_Implicit(val), 0.11999999731779099).SetEase((EaseType)2);
					_flashTween = val2;
					if (((PowerModel)this).Owner != null && LocalContext.IsMe(((PowerModel)this).Owner))
					{
						SfxCmd.Play("event:/sfx/ui/clicks/ui_checkbox_on", 1f);
					}
					ScheduleNextFlash();
				}
				return;
			}
		}
		StopRedFlash();
	}

	private void ScheduleNextFlash()
	{
		if (_redFlashTimer != null)
		{
			float num;
			if (_flashIndex < _flashIntervals.Count)
			{
				num = _flashIntervals[_flashIndex];
				_flashIndex++;
			}
			else
			{
				num = 0.2f;
			}
			_redFlashTimer.WaitTime = num;
			_redFlashTimer.Start(-1.0);
		}
	}

	private void BuildFlashSchedule()
	{
		_flashIntervals.Clear();
		_flashIndex = 0;
		int amount = ((PowerModel)this).Amount;
		if (amount <= 0)
		{
			return;
		}
		float num = (float)amount * 1f;
		int num2 = Mathf.FloorToInt(num * 0.5f / 1f);
		for (int i = 0; i < num2; i++)
		{
			_flashIntervals.Add(1f);
		}
		float num3 = num - (float)num2 * 1f;
		if (!(num3 <= 0f))
		{
			float num4 = (num3 - 1f) / (num3 - 0.2f);
			int num5 = Mathf.RoundToInt(Mathf.Log(0.2f) / Mathf.Log(num4)) + 1;
			num5 = Mathf.Max(num5, 2);
			float num6 = Mathf.Pow(0.2f, 1f / (float)(num5 - 1));
			float num7 = 1f;
			for (int j = 0; j < num5; j++)
			{
				_flashIntervals.Add(num7);
				num7 = Mathf.Max(num7 * num6, 0.2f);
			}
		}
	}

	private void StartTickTimer(NCreature nCreature)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		_tickTimer = new Timer
		{
			OneShot = false,
			WaitTime = 1.0
		};
		_tickTimer.Timeout += OnTickTimeout;
		GodotTreeExtensions.AddChildSafely((Node)(object)nCreature, (Node)(object)_tickTimer);
		_tickTimer.Start(-1.0);
	}

	private void StopTickTimer()
	{
		if (_tickTimer != null)
		{
			_tickTimer.Stop();
			((Node)_tickTimer).QueueFree();
			_tickTimer = null;
		}
	}

	private void OnTickTimeout()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I4
		if (!((AbstractModel)this).IsMutable || ((PowerModel)this).Amount <= 0)
		{
			StopTickTimer();
			return;
		}
		ActionQueueSynchronizer actionQueueSynchronizer = RunManager.Instance.ActionQueueSynchronizer;
		if ((int)actionQueueSynchronizer.CombatState == 2)
		{
			Creature owner = ((PowerModel)this).Owner;
			Player val = ((owner != null) ? owner.Player : null);
			if (val != null)
			{
				actionQueueSynchronizer.RequestEnqueue((GameAction)(object)new TickTackGameAction(val));
			}
		}
	}

	private void StartVfxTimer(NCreature nCreature)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		_vfxTimer = new Timer
		{
			OneShot = false,
			WaitTime = 1.0
		};
		_vfxTimer.Timeout += OnVfxTick;
		GodotTreeExtensions.AddChildSafely((Node)(object)nCreature, (Node)(object)_vfxTimer);
		_vfxTimer.Start(-1.0);
	}

	private void StopVfxTimer()
	{
		if (_vfxTimer != null)
		{
			_vfxTimer.Stop();
			((Node)_vfxTimer).QueueFree();
			_vfxTimer = null;
		}
	}

	private void OnVfxTick()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (((AbstractModel)this).IsMutable && ((PowerModel)this).Amount > 1 && _creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_creatureNode) && ((PowerModel)this).Owner != null && LocalContext.IsMe(((PowerModel)this).Owner))
		{
			ShakeCreature(_creatureNode, ((PowerModel)this).Amount);
			NPowerAppliedVfx val = NPowerAppliedVfx.Create((PowerModel)(object)this, ((PowerModel)this).Amount, false);
			if (val != null)
			{
				GodotTreeExtensions.AddChildSafely((Node)(object)_creatureNode, (Node)(object)val);
				((Control)val).GlobalPosition = ((Control)NCombatRoom.Instance).Size / 2f;
			}
		}
	}

	private static void ShakeCreature(NCreature nCreature, int remaining)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		float intensity = Mathf.Clamp(30f / (float)remaining, 5f, 30f);
		float num = 0.3f;
		Vector2 originalPos = ((Control)nCreature).Position;
		Tween obj = ((Node)nCreature).CreateTween();
		obj.TweenMethod(Callable.From<float>((Action<float>)delegate(float t)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			((Control)nCreature).Position = originalPos + Vector2.Right * intensity * Mathf.Sin(t * 12f) * Mathf.Sin(t * 1.5f);
		}), Variant.op_Implicit(0f), Variant.op_Implicit((float)Math.PI * 2f), (double)num).SetEase((EaseType)1).SetTrans((TransitionType)7);
		obj.TweenCallback(Callable.From<Vector2>((Func<Vector2>)(() => ((Control)nCreature).Position = originalPos)));
	}
}
