using System;
using System.Collections.Generic;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class SharedDefensePresentationRuntime : IDisposable
{
	private const string ScriptPath = "res://CznCore/scripts/defense_presentation.gd";

	private const float ShieldBreakAudibilityGain = 1.4125375f;

	private readonly Func<Creature, bool> _accepts;

	private readonly Action<Node2D, Creature> _settings;

	private readonly Action<string> _warn;

	private readonly string _settingsGroup;

	private readonly CombatCreatureSpineResolver _spines = new CombatCreatureSpineResolver();

	private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

	private bool _disposed;

	private Func<Creature, float>? _breakVolume;

	private AudioStreamPlayerRuntime? _breakAudio;

	public bool PlayPartialBlockFeedback { get; set; } = true;

	public void ConfigureBreakAudio(Func<Creature, float> volume)
	{
		_breakVolume = volume;
	}

	private bool TryBreakAudio(Creature target)
	{
		if (_disposed || !_accepts(target) || _breakVolume == null)
		{
			return false;
		}
		float num = _breakVolume(target);
		if (num <= 0f)
		{
			return false;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(target) : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val) || !((Node)val).IsInsideTree() || !((CanvasItem)val).IsVisibleInTree())
		{
			return false;
		}
		if (_breakAudio == null)
		{
			_breakAudio = new AudioStreamPlayerRuntime("CznShieldBreak_" + _settingsGroup, "Missing shield break audio: ", "Shield break audio load failed: ", (CacheMode)1, delegate(string message, int _)
			{
				_warn(message);
			}, 12);
		}
		return _breakAudio.Play("res://CznCore/audio/sfx/defense/shield_broke.ogg", num * 1.4125375f);
	}

	public SharedDefensePresentationRuntime(string owner, Func<Creature, bool> accepts, Action<Node2D> settings, string settingsGroup, Action<string> warn)
		: this(owner, accepts, delegate(Node2D effect, Creature _)
		{
			settings(effect);
		}, settingsGroup, warn)
	{
	}

	public SharedDefensePresentationRuntime(string owner, Func<Creature, bool> accepts, Action<Node2D, Creature> settings, string settingsGroup, Action<string> warn)
	{
		_subscriptions.Add(SkinEventHub.Subscribe(owner + ".defense.audio", delegate(NativeShieldBreakSfxRequest e)
		{
			e.TryReplace(() => TryBreakAudio(e.Target));
		}));
		_accepts = accepts;
		_settings = settings;
		_settingsGroup = settingsGroup;
		_warn = warn;
		_subscriptions.Add(SkinEventHub.Subscribe(owner + ".defense.gain", delegate(BlockActuallyIncreasedEvent e)
		{
			if (e.Receiver.IsMonster && e.CurrentBlock > e.PreviousBlock)
			{
				Play(e.Receiver, "system_buff_enemy_shield");
			}
		}));
		_subscriptions.Add(SkinEventHub.Subscribe(owner + ".defense.hit", delegate(DefenseBlockDamaged e)
		{
			if (PlayPartialBlockFeedback && SharedDefenseRules.PlayHit(e.Before, e.After))
			{
				Play(e.Target, e.Target.IsMonster ? "system_hit_shield_enemy" : "system_hit_shield_player");
			}
		}));
		_subscriptions.Add(SkinEventHub.Subscribe(owner + ".defense.break", delegate(DefenseBlockBroken e)
		{
			Play(e.Target, e.Target.IsMonster ? "system_break_shield_enemy" : "system_break_shield_player");
		}));
	}

	private void Play(Creature target, string route)
	{
		if (_disposed || !_accepts(target))
		{
			return;
		}
		NCombatRoom room = NCombatRoom.Instance;
		GodotScheduler.Deferred(delegate
		{
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Expected O, but got Unknown
			//IL_013c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			if (!_disposed && room != null && GodotObject.IsInstanceValid((GodotObject)(object)room) && room == NCombatRoom.Instance && _accepts(target))
			{
				NCreature creatureNode = room.GetCreatureNode(target);
				if (creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && ((Node)creatureNode).IsInsideTree() && ((CanvasItem)creatureNode).IsVisibleInTree())
				{
					Node2D val = null;
					try
					{
						Node2D val2 = _spines.Find(creatureNode);
						Node2D anchor = null;
						Vector2 scale = Vector2.One;
						bool flag = target.IsMonster && MonsterPresentationSurface.TryResolveDefense(val2, route, out anchor, out scale);
						Node2D val3 = (Node2D)(flag ? anchor : (target.IsMonster ? ((object)CombatTargetSurfaceAnchorRuntime.Resolve(creatureNode, (route == "system_hit_shield_enemy") ? "CznShieldHit" : "CznShieldRoot", "Shield anchor: ", null, null, (route == "system_hit_shield_enemy") ? CombatTargetSurfaceAnchorMode.VfxSpawnPosition : CombatTargetSurfaceAnchorMode.FloorLockedVfxX)) : ((object)val2)));
						if (val3 != null)
						{
							val = new Node2D
							{
								Name = StringName.op_Implicit("CznDefenseFeedback")
							};
							((GodotObject)val).SetScript(Variant.op_Implicit((GodotObject)(object)ResourceLoader.Load<Script>("res://CznCore/scripts/defense_presentation.gd", (string)null, (CacheMode)1)));
							((GodotObject)val).Set(StringName.op_Implicit("route"), Variant.op_Implicit(route));
							((GodotObject)val).Set(StringName.op_Implicit("settings_group"), Variant.op_Implicit(_settingsGroup));
							Vector2 globalScale;
							Vector2 val4;
							if (val2 == null)
							{
								globalScale = val3.GlobalScale;
								val4 = ((Vector2)(ref globalScale)).Abs();
							}
							else
							{
								globalScale = val2.GlobalScale;
								val4 = ((Vector2)(ref globalScale)).Abs();
							}
							Vector2 val5 = val4;
							globalScale = val3.GlobalScale;
							Vector2 val6 = ((Vector2)(ref globalScale)).Abs();
							val.Scale = (flag ? scale : (new Vector2(val5.X / Math.Max(0.0001f, val6.X), val5.Y / Math.Max(0.0001f, val6.Y)) * (target.IsMonster ? 1f : 0.62f)));
							_settings(val, target);
							((Node)val3).AddChild((Node)(object)val, false, (InternalMode)0);
						}
					}
					catch (Exception ex)
					{
						if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
						{
							((Node)val).QueueFree();
						}
						_warn("Defense presentation failed; native feedback retained: " + ex.Message);
					}
				}
			}
		});
	}

	public void Dispose()
	{
		_disposed = true;
		foreach (IDisposable subscription in _subscriptions)
		{
			subscription.Dispose();
		}
		_subscriptions.Clear();
		_spines.Clear();
		_breakAudio?.TrimCache(stopPlayback: true);
	}
}
