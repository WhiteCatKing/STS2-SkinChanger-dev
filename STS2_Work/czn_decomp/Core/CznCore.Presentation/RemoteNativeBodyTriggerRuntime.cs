using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class RemoteNativeBodyTriggerRuntime
{
	private readonly CharacterSkinProfile _profile;

	private readonly CombatCreatureNodeResolver _creatures;

	private readonly CombatBodySurfaceCoordinator _bodySurface;

	private readonly Func<Player?, bool> _isLocalPlayer;

	private readonly Func<Player?, bool> _canPresentBody;

	private readonly Action<NCreature, string, bool>? _beforeSetAnimation;

	private readonly Action<NCreature, string>? _onCombatEntryStart;

	private readonly CombatBodySequenceRuntime _entryBodySequences;

	private readonly RemoteCardReadyBodyRuntime _cardReady;

	private readonly string _combatEntryPlayedMeta;

	private readonly string _combatEntryOffsetTokenMeta;

	private readonly HashSet<Player> _pendingCombatEntryPlayers = new HashSet<Player>();

	private int _combatEntryOffsetToken;

	public RemoteNativeBodyTriggerRuntime(CharacterSkinProfile profile, CombatCreatureNodeResolver creatures, CombatBodySurfaceCoordinator bodySurface, Func<Player?, bool> isLocalPlayer, Func<Player?, bool> canPresentBody, Func<CombatBodyReadyPresentationRuntime<Player, CardModel>> getReadyPresentation, Action<NCreature, string, bool>? beforeSetAnimation = null, Action<NCreature, string>? onCombatEntryStart = null)
	{
		RemoteNativeBodyTriggerRuntime remoteNativeBodyTriggerRuntime = this;
		_profile = profile;
		_creatures = creatures;
		_bodySurface = bodySurface;
		_isLocalPlayer = isLocalPlayer;
		_canPresentBody = (Player? player) => player != null && player.Creature.IsAlive && canPresentBody(player);
		_beforeSetAnimation = beforeSetAnimation;
		_onCombatEntryStart = onCombatEntryStart;
		_entryBodySequences = new CombatBodySequenceRuntime(profile.CombatPresentation, profile.CombatBody.ReadyExitAnimation, 0f);
		_cardReady = new RemoteCardReadyBodyRuntime((Player? player) => player != null && profile.AppliesTo(player) && !isLocalPlayer(player), _canPresentBody, getReadyPresentation, ResolveCardReadyTransitionSeconds(profile, _entryBodySequences), delegate(string message, int priority)
		{
			SkinLog.Warn("Remote " + remoteNativeBodyTriggerRuntime._profile.ProfileId + " " + message, priority);
		});
		_combatEntryPlayedMeta = profile.RuntimeMetaKey("remote_combat_entry_played");
		_combatEntryOffsetTokenMeta = profile.RuntimeMetaKey("remote_combat_entry_offset_token");
	}

	public void AddCardReadyEventSubscriptions(ICollection<IDisposable> subscriptions, string contextPrefix)
	{
		_cardReady.AddEventSubscriptions(subscriptions, contextPrefix);
	}

	public bool TryConsumeCardReadyActionDelay(CardModel? card, out float delaySeconds)
	{
		return _cardReady.TryConsumeActionHandoffDelay(card, out delaySeconds);
	}

	public void InvalidatePlayerPresentation(Player player)
	{
		_pendingCombatEntryPlayers.Remove(player);
		_cardReady.ClearPlayer(player);
	}

	public void ScheduleCombatEntryForRemotePlayers(IEnumerable<Player>? players)
	{
		_pendingCombatEntryPlayers.Clear();
		if (players == null)
		{
			_cardReady.TrackPlayers(Array.Empty<Player>());
			return;
		}
		List<Player> list = new List<Player>();
		foreach (Player player in players)
		{
			if (player != null && _profile.AppliesTo(player) && !_isLocalPlayer(player))
			{
				list.Add(player);
			}
		}
		_cardReady.TrackPlayers(list);
		if (_profile.CombatBody.EntrySequence.Length == 0)
		{
			return;
		}
		foreach (Player item in list)
		{
			if (_canPresentBody(item))
			{
				_pendingCombatEntryPlayers.Add(item);
				TryPlayPendingCombatEntry(item);
			}
		}
	}

	public void TryPlayPendingCombatEntry(NCreature? creatureNode)
	{
		object obj;
		if (creatureNode == null)
		{
			obj = null;
		}
		else
		{
			Creature entity = creatureNode.Entity;
			obj = ((entity != null) ? entity.Player : null);
		}
		Player val = (Player)obj;
		if (val != null && _pendingCombatEntryPlayers.Contains(val))
		{
			TryPlayPendingCombatEntry(val);
		}
	}

	public bool TryPlay(Creature? creature, string triggerName, float waitTime, out Task handledTask)
	{
		handledTask = Task.CompletedTask;
		if (((creature != null) ? creature.Player : null) == null || !_profile.AppliesTo(creature.Player) || _isLocalPlayer(creature.Player) || !_canPresentBody(creature.Player) || !_profile.CombatBody.TryGetRemoteNativeBodyAnimation(triggerName, out var animation))
		{
			return false;
		}
		IReadOnlyList<string> readOnlyList = ResolveAnimationSequence(triggerName, animation);
		if (readOnlyList.Count == 0)
		{
			return false;
		}
		Player player = creature.Player;
		NCreature val = _creatures.ResolvePlayer(NCombatRoom.Instance, player, "Remote " + _profile.ProfileId + " body trigger player node lookup failed", delegate(string message)
		{
			SkinLog.Warn(message);
		});
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return false;
		}
		ClearCombatEntryOffsetToken(val, null);
		_bodySurface.Ensure(val, "remote_native_body_trigger");
		if (IsActiveIdleTrigger(triggerName))
		{
			if (!TrySetRemoteIdleState(val, player))
			{
				return false;
			}
			handledTask = WaitForHandledNativeBodyTrigger(waitTime);
			SkinLog.Info("Mapped remote " + _profile.ProfileId + " native body trigger=" + triggerName + " using synced card-ready state");
			return true;
		}
		foreach (string animationName in readOnlyList)
		{
			if (!_bodySurface.HasAnimation(val, animationName, _profile.ProfileId + " remote combat"))
			{
				SkinLog.Debug(() => "Skipped unavailable remote " + _profile.ProfileId + " body animation: " + animationName);
				return false;
			}
		}
		bool loop = animation.Loop;
		if (!TrySetBodyAnimationSequence(val, readOnlyList, loop))
		{
			return false;
		}
		if (!animation.Loop && animation.RestoreIdle)
		{
			AppendPostActionIdleSequence(val, player);
		}
		handledTask = WaitForHandledNativeBodyTrigger(waitTime);
		SkinLog.Info("Mapped remote " + _profile.ProfileId + " native body trigger=" + triggerName + " animation=" + FormatAnimationSequence(readOnlyList));
		return true;
	}

	private void TryPlayPendingCombatEntry(Player player)
	{
		if (TryPlayCombatEntry(player))
		{
			_pendingCombatEntryPlayers.Remove(player);
		}
	}

	private bool TryPlayCombatEntry(Player player)
	{
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!_profile.AppliesTo(player) || _isLocalPlayer(player) || !_canPresentBody(player))
			{
				return true;
			}
			NCreature val = _creatures.ResolvePlayer(NCombatRoom.Instance, player, "Remote " + _profile.ProfileId + " combat entry player node lookup failed", delegate(string message)
			{
				SkinLog.Warn(message);
			});
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				return false;
			}
			if (((GodotObject)val).HasMeta(StringName.op_Implicit(_combatEntryPlayedMeta)))
			{
				return true;
			}
			if (!_bodySurface.Ensure(val, "remote_combat_entry"))
			{
				return false;
			}
			if (!HasCombatEntryAnimations(val))
			{
				return true;
			}
			IReadOnlyList<CombatPresentationBodyStep> readOnlyList = CombatBodyEntryPlaybackRuntime.BuildEntrySteps(_profile.CombatBody);
			CombatBodyEntryRunInStart entryRunIn = CombatBodyEntryPlaybackRuntime.TryBeginRunIn(_profile.CombatBody, readOnlyList, val, delegate(string message)
			{
				SkinLog.Debug(() => "Remote " + _profile.ProfileId + " " + message);
			}, delegate(string message)
			{
				SkinLog.Warn("Remote " + _profile.ProfileId + " " + message);
			});
			int num = BeginCombatEntryOffsetToken(val);
			if (!TrySetCombatEntrySequence(val, readOnlyList))
			{
				ClearCombatEntryOffsetToken(val, num);
				return true;
			}
			_onCombatEntryStart?.Invoke(val, "remote_entry_start");
			QueueCombatEntrySegmentOffsets(val, entryRunIn, readOnlyList, num);
			StringName obj = StringName.op_Implicit(_combatEntryPlayedMeta);
			bool flag = true;
			((GodotObject)val).SetMeta(obj, Variant.From<bool>(ref flag));
			SkinLog.Info("Queued remote " + _profile.ProfileId + " combat entry sequence=" + FormatAnimationSequence(_profile.CombatBody.EntrySequence) + " -> " + _profile.CombatBody.ActiveCombatIdleAnimation + " -> " + _profile.CombatBody.ReadyExitAnimation + " -> " + _profile.CombatBody.IdleAnimation);
			return true;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Remote " + _profile.ProfileId + " combat entry playback failed: " + SkinLog.FormatException(ex));
			return true;
		}
	}

	private IReadOnlyList<string> ResolveAnimationSequence(string triggerName, CharacterSkinRemoteNativeBodyAnimation mapping)
	{
		IReadOnlyList<string> animationSequence = mapping.AnimationSequence;
		if (animationSequence != null && animationSequence.Count > 0)
		{
			return mapping.AnimationSequence;
		}
		if (string.Equals(triggerName, "Dead", StringComparison.OrdinalIgnoreCase) && _profile.CombatBody.DeathSequence.Length != 0)
		{
			return _profile.CombatBody.DeathSequence;
		}
		if (!string.IsNullOrWhiteSpace(mapping.AnimationName))
		{
			return new string[1] { mapping.AnimationName };
		}
		return Array.Empty<string>();
	}

	private static float ResolveCardReadyTransitionSeconds(CharacterSkinProfile profile, CombatBodySequenceRuntime bodySequences)
	{
		string cardReady = profile.CombatPresentation.CommonStateIds.CardReady;
		if (!profile.CombatPresentation.TryGetState(cardReady, out CombatPresentationState state) || state.BodySteps.Count <= 0)
		{
			return 0f;
		}
		return bodySequences.GetSequenceDelaySeconds(state.BodySteps);
	}

	private bool TrySetBodyAnimation(NCreature creatureNode, string animationName, bool loop)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			_beforeSetAnimation?.Invoke(creatureNode, animationName, loop);
			SpineAnimationCompat.SetAnimation(creatureNode.SpineAnimation, animationName, loop);
			return true;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Remote " + _profile.ProfileId + " body animation failed: " + animationName + " " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private bool TrySetBodyAnimationSequence(NCreature creatureNode, IReadOnlyList<string> animationSequence, bool loopLast)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (animationSequence.Count == 0)
		{
			return false;
		}
		try
		{
			bool flag = animationSequence.Count == 1 && loopLast;
			_beforeSetAnimation?.Invoke(creatureNode, animationSequence[0], flag);
			SpineAnimationCompat.SetAnimation(creatureNode.SpineAnimation, animationSequence[0], flag);
			for (int i = 1; i < animationSequence.Count; i++)
			{
				bool loop = i == animationSequence.Count - 1 && loopLast;
				SpineAnimationCompat.AddAnimation(creatureNode.SpineAnimation, animationSequence[i], 0f, loop);
			}
			return true;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Remote " + _profile.ProfileId + " body animation sequence failed: " + FormatAnimationSequence(animationSequence) + " " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private bool TrySetCombatEntrySequence(NCreature creatureNode, IReadOnlyList<CombatPresentationBodyStep> entrySteps)
	{
		if (entrySteps.Count == 0)
		{
			return false;
		}
		string[] array = new string[entrySteps.Count];
		for (int i = 0; i < entrySteps.Count; i++)
		{
			array[i] = entrySteps[i].BodyAnimation;
		}
		if (!TrySetBodyAnimationSequence(creatureNode, array, loopLast: false))
		{
			return false;
		}
		AppendReadyExitToIdleSequence(creatureNode);
		return true;
	}

	private int BeginCombatEntryOffsetToken(NCreature creatureNode)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_combatEntryOffsetToken++;
		if (_combatEntryOffsetToken == 0)
		{
			_combatEntryOffsetToken = 1;
		}
		((GodotObject)creatureNode).SetMeta(StringName.op_Implicit(_combatEntryOffsetTokenMeta), Variant.From<int>(ref _combatEntryOffsetToken));
		return _combatEntryOffsetToken;
	}

	private void ClearCombatEntryOffsetToken(NCreature creatureNode, int? expectedToken = null, bool completeRunIn = true)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
		{
			return;
		}
		if (!((GodotObject)creatureNode).HasMeta(StringName.op_Implicit(_combatEntryOffsetTokenMeta)))
		{
			return;
		}
		if (expectedToken.HasValue)
		{
			StringName obj = StringName.op_Implicit(_combatEntryOffsetTokenMeta);
			Variant val = default(Variant);
			val = ((GodotObject)creatureNode).GetMeta(obj, val);
			if (((Variant)(ref val)).AsInt32() != expectedToken.Value)
			{
				return;
			}
		}
		if (completeRunIn)
		{
			CombatBodyEntryPlaybackRuntime.CompleteActiveRunIn(creatureNode, "remote_entry_token_clear", delegate(string message)
			{
				SkinLog.Debug(() => "Remote " + _profile.ProfileId + " " + message);
			}, delegate(string message)
			{
				SkinLog.Warn("Remote " + _profile.ProfileId + " " + message);
			});
		}
		((GodotObject)creatureNode).RemoveMeta(StringName.op_Implicit(_combatEntryOffsetTokenMeta));
	}

	private bool IsCombatEntryOffsetTokenCurrent(NCreature creatureNode, int expectedToken)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && ((GodotObject)creatureNode).HasMeta(StringName.op_Implicit(_combatEntryOffsetTokenMeta)))
		{
			StringName obj = StringName.op_Implicit(_combatEntryOffsetTokenMeta);
			Variant val = default(Variant);
			val = ((GodotObject)creatureNode).GetMeta(obj, val);
			return ((Variant)(ref val)).AsInt32() == expectedToken;
		}
		return false;
	}

	private void QueueCombatEntrySegmentOffsets(NCreature creatureNode, CombatBodyEntryRunInStart entryRunIn, IReadOnlyList<CombatPresentationBodyStep> entrySteps, int entryOffsetToken)
	{
		CombatBodyEntryPlaybackRuntime.ScheduleCompletion(creatureNode, entryRunIn, _entryBodySequences.GetSequenceDelaySeconds(entrySteps), delegate(string message)
		{
			SkinLog.Debug(() => "Remote " + _profile.ProfileId + " " + message);
		}, delegate(string message)
		{
			SkinLog.Warn("Remote " + _profile.ProfileId + " " + message);
		});
		if (!entryRunIn.UsesSegmentOffsets || entrySteps.Count <= 1)
		{
			ClearCombatEntryOffsetToken(creatureNode, entryOffsetToken, completeRunIn: false);
			return;
		}
		IReadOnlyList<CombatBodySequenceStepSchedule> readOnlyList = _entryBodySequences.BuildStepSchedule(entrySteps);
		int num = -1;
		for (int i = 1; i < readOnlyList.Count; i++)
		{
			if (CombatBodyEntryPlaybackRuntime.TryGetSegmentOffset(_profile.CombatBody, i, out var _))
			{
				num = i;
			}
		}
		if (num < 0)
		{
			ClearCombatEntryOffsetToken(creatureNode, entryOffsetToken, completeRunIn: false);
			return;
		}
		for (int j = 1; j < readOnlyList.Count; j++)
		{
			if (!CombatBodyEntryPlaybackRuntime.TryGetSegmentOffset(_profile.CombatBody, j, out var offset2))
			{
				continue;
			}
			bool isLastOffset = j == num;
			float delaySeconds = readOnlyList[j].DelaySeconds;
			GodotScheduler.Delay(Math.Max(0.05f, delaySeconds), delegate
			{
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				if (IsCombatEntryOffsetTokenCurrent(creatureNode, entryOffsetToken))
				{
					CombatBodyEntryPlaybackRuntime.ApplySegmentOffset(creatureNode, entryRunIn.TargetPosition, offset2);
					if (isLastOffset)
					{
						CombatBodyEntryPlaybackRuntime.CompleteRunInIfCurrent(creatureNode, entryRunIn, "remote_entry_last_segment", delegate(string message)
						{
							SkinLog.Debug(() => "Remote " + _profile.ProfileId + " " + message);
						}, delegate(string message)
						{
							SkinLog.Warn("Remote " + _profile.ProfileId + " " + message);
						});
						ClearCombatEntryOffsetToken(creatureNode, entryOffsetToken, completeRunIn: false);
					}
				}
			});
		}
	}

	private void AppendReadyExitToIdleSequence(NCreature creatureNode)
	{
		string activeCombatIdleAnimation = _profile.CombatBody.ActiveCombatIdleAnimation;
		TryAddBodyAnimation(creatureNode, activeCombatIdleAnimation, loop: false, "combat entry ready");
		string readyExitAnimation = _profile.CombatBody.ReadyExitAnimation;
		TryAddBodyAnimation(creatureNode, readyExitAnimation, loop: false, "combat entry ready exit");
		string idleAnimation = _profile.CombatBody.IdleAnimation;
		TryAddBodyAnimation(creatureNode, idleAnimation, loop: true, "combat entry idle");
	}

	private bool TrySetRemoteIdleState(NCreature creatureNode, Player player)
	{
		if (_cardReady.IsPresentationActive(player))
		{
			return true;
		}
		string idleAnimation = _profile.CombatBody.IdleAnimation;
		if (!string.IsNullOrWhiteSpace(idleAnimation) && _bodySurface.HasAnimation(creatureNode, idleAnimation, _profile.ProfileId + " remote synced idle"))
		{
			return TrySetBodyAnimation(creatureNode, idleAnimation, loop: true);
		}
		return false;
	}

	private void AppendPostActionIdleSequence(NCreature creatureNode, Player player)
	{
		if (!_cardReady.IsPresentationActive(player))
		{
			TryAddBodyAnimation(creatureNode, _profile.CombatBody.ReadyExitAnimation, loop: false, "post-action ready exit");
			TryAddBodyAnimation(creatureNode, _profile.CombatBody.IdleAnimation, loop: true, "post-action idle");
		}
	}

	private bool TryAddBodyAnimation(NCreature creatureNode, string? animationName, bool loop, string reason)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(animationName) || !_bodySurface.HasAnimation(creatureNode, animationName, _profile.ProfileId + " remote " + reason))
		{
			return false;
		}
		try
		{
			SpineAnimationCompat.AddAnimation(creatureNode.SpineAnimation, animationName, 0f, loop);
			return true;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Remote " + _profile.ProfileId + " body animation queue failed: " + animationName + " " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private bool HasCombatEntryAnimations(NCreature creatureNode)
	{
		string[] entrySequence = _profile.CombatBody.EntrySequence;
		foreach (string animationName in entrySequence)
		{
			if (!_bodySurface.HasAnimation(creatureNode, animationName, _profile.ProfileId + " remote combat entry"))
			{
				return false;
			}
		}
		string activeCombatIdleAnimation = _profile.CombatBody.ActiveCombatIdleAnimation;
		if (!string.IsNullOrWhiteSpace(activeCombatIdleAnimation) && !_bodySurface.HasAnimation(creatureNode, activeCombatIdleAnimation, _profile.ProfileId + " remote combat entry"))
		{
			return false;
		}
		return HasReadyExitOrIdleAnimation(creatureNode);
	}

	private bool HasReadyExitOrIdleAnimation(NCreature creatureNode)
	{
		string readyExitAnimation = _profile.CombatBody.ReadyExitAnimation;
		if (!string.IsNullOrWhiteSpace(readyExitAnimation) && !_bodySurface.HasAnimation(creatureNode, readyExitAnimation, _profile.ProfileId + " remote combat idle restore"))
		{
			return false;
		}
		string idleAnimation = _profile.CombatBody.IdleAnimation;
		if (!string.IsNullOrWhiteSpace(idleAnimation))
		{
			return _bodySurface.HasAnimation(creatureNode, idleAnimation, _profile.ProfileId + " remote combat idle restore");
		}
		return true;
	}

	private static string FormatAnimationSequence(IReadOnlyList<string> animationSequence)
	{
		return string.Join(" -> ", animationSequence);
	}

	private static bool IsActiveIdleTrigger(string triggerName)
	{
		return string.Equals(triggerName, "Idle", StringComparison.OrdinalIgnoreCase);
	}

	private static Task WaitForHandledNativeBodyTrigger(float waitTime)
	{
		float num = MathF.Max(0f, waitTime);
		return Cmd.CustomScaledWait(MathF.Min(num * 0.5f, 0.25f), num, false, default(CancellationToken));
	}
}
