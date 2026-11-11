using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CznCore.Godot;
using CznCore.Presentation;
using CznCore.Settings;
using CznCore.Standalone;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using MeirinWatcherSkin.Characters.Meirin;
using WatcherMod;

namespace MeirinWatcherSkin;

[ModInitializer("Init")]
public static class Entry
{
	[HarmonyPatch(typeof(Hook), "AfterAttack")]
	private static class MeirinAttackTimingCleanupPatch
	{
		private static bool Prepare()
		{
			return SkinProfile.Features.CombatEntryAnimation;
		}

		private static void Postfix(AttackCommand command)
		{
			Creature attacker = command.Attacker;
			Player val = ((attacker != null) ? attacker.Player : null);
			if (val != null && MeirinAttackTimings.TryGetValue(val, out MeirinAttackTimingState value) && value.Command == command)
			{
				MeirinAttackTimings.Remove(val);
			}
			if (val != null && IsTargetPlayer(val))
			{
				CompleteMeirinBodyReadyAction(val);
			}
		}
	}

	private sealed class MeirinAttackTimingState
	{
		private CombatAttackTimingSession? _timing;

		public AttackCommand? Command { get; private set; }

		public string TriggerName { get; private set; } = string.Empty;

		public string CueName { get; private set; } = string.Empty;

		public int HitCount { get; private set; }

		public float HitMarkerSeconds { get; private set; }

		public bool HasPlayedVoice { get; set; }

		public bool HasConsumedRouteReservation { get; set; }

		public bool IsActive
		{
			get
			{
				if (Command != null)
				{
					return _timing != null;
				}
				return false;
			}
		}

		public void Begin(AttackCommand command, string triggerName, string cueName, int hitCount, int token, ulong startMsec, float hitMarkerSeconds)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Expected O, but got Unknown
			Command = command;
			TriggerName = triggerName;
			CueName = cueName;
			HitCount = Math.Max(1, hitCount);
			HitMarkerSeconds = Math.Max(0f, hitMarkerSeconds);
			HasPlayedVoice = false;
			HasConsumedRouteReservation = false;
			_timing = new CombatAttackTimingSession(token, startMsec, (IReadOnlyList<float>)new _003C_003Ez__ReadOnlySingleElementList<float>(HitMarkerSeconds));
		}

		public int NextNativeAttackTriggerIndex()
		{
			CombatAttackTimingSession? timing = _timing;
			if (timing == null)
			{
				return 0;
			}
			return timing.NextNativeAttackTriggerIndex();
		}

		public bool TryResolveRemainingHitDelaySeconds(int triggerIndex, ulong nowMsec, out float targetElapsedSeconds, out float remainingSeconds)
		{
			targetElapsedSeconds = HitMarkerSeconds;
			remainingSeconds = 0f;
			if (_timing == null || !_timing.TryResolveDamageReleaseElapsedSeconds(triggerIndex, HitCount, ref targetElapsedSeconds))
			{
				return false;
			}
			remainingSeconds = _timing.ResolveRemainingDelaySeconds(targetElapsedSeconds, nowMsec);
			return true;
		}
	}

	private sealed class EntryBattleReadyOverlayRuntimeContext : IBattleReadyOverlayRuntimeContext<CardModel>
	{
		public LeftSideCardPresentationPolicy<CardModel> PresentationPolicy { get; } = new LeftSideCardPresentationPolicy<CardModel>((Func<LeftSideCardPresentationMode>)(() => (!SkinProfile.Features.BattleReadyOverlay) ? ((LeftSideCardPresentationMode)0) : SkinRuntimeSettings.BattleReadyOverlayMode), (Func<CardModel, bool>)IsLocalTargetCard, (Func<CardModel, bool>)HasCombatCutInPresentation);

		public float BattleReadyScale => SkinRuntimeSettings.BattleReadyScale;

		public float BattleReadyOpacity => SkinRuntimeSettings.BattleReadyOpacity;

		public float BattleReadyOffsetX => SkinRuntimeSettings.BattleReadyOffsetX;

		public float BattleReadyOffsetY => SkinRuntimeSettings.BattleReadyOffsetY;

		public ISkinTimingProbe TimingProbe => (ISkinTimingProbe)(object)Diagnostics;

		public bool IsTargetCard(CardModel? card)
		{
			return IsLocalTargetCard(card);
		}

		public CombatPresentationCardRoute ResolveRoute(CardModel card)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return CombatCardContexts.GetRoute(card);
		}

		public Node? GetOverlayParent()
		{
			return GetBattleReadyScreen();
		}

		public Node2D? DetachIdleEffectLayer(Node sourceRoot, string childName)
		{
			return null;
		}

		public void ApplyIdleEffectMaterial(Node root)
		{
		}

		public void ApplyIdleEffectMaterialDeferred(Node root, int frames)
		{
		}

		public void StartIdleEffects(Node root)
		{
		}

		public void LogInfo(string message)
		{
			SkinLog.Debug("Meirin battle ready: " + message);
		}

		public void LogWarn(string message)
		{
			SkinLog.Warn("Meirin battle ready: " + message);
		}
	}

	private static class BundledCardArtHolder
	{
		internal static readonly BundledSkinCardArt Instance = new BundledSkinCardArt("MeirinWatcherSkin", SkinProfile, (Action<string, int>)SkinLog.Warn);
	}

	[HarmonyPatch(typeof(WatcherCard), "OnEnqueuePlayVfx")]
	private static class WatcherCardEnqueuePresentationPatch
	{
		[HarmonyPriority(800)]
		private static void Prefix(WatcherCard __instance, Creature? target, out Task __state)
		{
			__state = TryBeginMeirinSupportCardPresentation((CardModel)(object)__instance, target);
		}

		private static void Postfix(Task __state, ref Task __result)
		{
			if (!__state.IsCompletedSuccessfully)
			{
				global::_003C_003Ey__InlineArray2<Task> buffer = default(global::_003C_003Ey__InlineArray2<Task>);
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 0) = __result;
				global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 1) = __state;
				__result = Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<Task>, Task>(in buffer, 2));
			}
		}
	}

	private sealed class SupportNativeTriggerGate
	{
		private string? _pendingRoute;

		private ulong _expiresAtMs;

		public void Register(string route, ulong nowMs)
		{
			_pendingRoute = route;
			_expiresAtMs = nowMs + 750;
		}

		public bool TryConsume(ulong nowMs, out string route)
		{
			if (!TryPeek(nowMs, out route))
			{
				return false;
			}
			_pendingRoute = null;
			return true;
		}

		public bool TryPeek(ulong nowMs, out string route)
		{
			route = string.Empty;
			if (string.IsNullOrEmpty(_pendingRoute) || nowMs > _expiresAtMs)
			{
				_pendingRoute = null;
				return false;
			}
			route = _pendingRoute;
			return true;
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectIcon")]
	[HarmonyPriority(0)]
	private static class CharacterSelectIconPatch
	{
		private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result)
		{
			ApplyCharacterSelectIcon(__instance, "res://MeirinWatcherSkin/images/character_select/char_select_meirin.png", ref __result);
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_CharacterSelectLockedIcon")]
	[HarmonyPriority(0)]
	private static class CharacterSelectLockedIconPatch
	{
		private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result)
		{
			ApplyCharacterSelectIcon(__instance, "res://MeirinWatcherSkin/images/character_select/char_select_meirin_locked.png", ref __result);
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectButton), "Init")]
	[HarmonyPriority(0)]
	private static class CharacterSelectButtonIconPatch
	{
		private static void Postfix(NCharacterSelectButton __instance, CharacterModel character)
		{
			SkinPatchGuard.Run("Meirin character-select button icon patch", (Action)delegate
			{
				if (SkinProfile.AppliesTo(character))
				{
					TextureRect nodeOrNull = ((Node)__instance).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon"));
					CompressedTexture2D val = ResourceLoader.Load<CompressedTexture2D>(__instance.IsLocked ? "res://MeirinWatcherSkin/images/character_select/char_select_meirin_locked.png" : "res://MeirinWatcherSkin/images/character_select/char_select_meirin.png", (string)null, (CacheMode)1);
					if (nodeOrNull != null && val != null)
					{
						nodeOrNull.Texture = (Texture2D)(object)val;
					}
				}
			}, 2);
		}
	}

	private sealed class MeirinBodyReadyHost : ICombatBodyReadyHost<Player, CardModel>
	{
		public bool IsTargetCard(CardModel? card)
		{
			return IsPresentationTargetCard(card);
		}

		public Player? ResolveActor(CardModel card)
		{
			return card.Owner;
		}

		public bool CanShow(Player actor)
		{
			return CanShowMeirinBodyReady(actor);
		}

		public void Warn(string message, int priority)
		{
			SkinLog.Warn(message, priority);
		}
	}

	private sealed class MeirinBodyReadyHitResponseGuardHost : ICombatHitResponseGuardHost<Player>
	{
		public void BeginHitResponseGuard(Player? actor, int token, CombatPresentationHitResponsePolicy hitResponsePolicy, string? reason)
		{
		}

		public void ScheduleHitResponseGuardClear(Player? actor, int token, float delaySeconds)
		{
		}
	}

	private sealed class MeirinBodyReadyPresentationRuntime : ICombatPresentationRuntime<Player>
	{
		public int NextToken(Player? actor)
		{
			if (actor != null)
			{
				return MeirinBodyReadyPlaybackTokens.GetOrCreateValue(actor).Next();
			}
			return 0;
		}

		public void ClearManualAttackChain(Player? actor)
		{
		}

		public void PlayBodyAnimation(Player? actor, CombatPresentationBodyStep step, string? loopAnimation, int token)
		{
			PlayMeirinBodyReadyState(actor, new _003C_003Ez__ReadOnlySingleElementList<CombatPresentationBodyStep>(step), loopAnimation, token);
		}

		public void PlayBodySequence(Player? actor, IReadOnlyList<CombatPresentationBodyStep> steps, string? loopAnimation, int token)
		{
			PlayMeirinBodyReadyState(actor, steps, loopAnimation, token);
		}
	}

	private sealed class MeirinCinematicSupportTimingState
	{
		private CombatAttackTimingSession? _timing;

		private bool _pending;

		public CardModel? Card { get; private set; }

		public void Begin(CardModel card, ulong routeStartMsec)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Expected O, but got Unknown
			Card = card;
			_timing = new CombatAttackTimingSession(1, routeStartMsec, (IReadOnlyList<float>)null);
			_pending = true;
		}

		public bool TryConsume(string triggerName, ulong nowMsec, out float remainingSeconds)
		{
			if (!_pending || _timing == null || (!string.Equals(triggerName, "Cast", StringComparison.OrdinalIgnoreCase) && !string.Equals(triggerName, "PowerUp", StringComparison.OrdinalIgnoreCase)))
			{
				remainingSeconds = 0f;
				return false;
			}
			_pending = false;
			remainingSeconds = _timing.ResolveRemainingDelaySeconds(2.7f, nowMsec);
			return true;
		}
	}

	private sealed class MeirinCinematicPhaseState
	{
		public int Generation { get; }

		public MeirinCombatRoute Route { get; }

		public CardModel Card { get; }

		public Player Player { get; }

		public NCreature OwnerNode { get; }

		public IReadOnlyList<NCreature> TargetNodes { get; }

		public Node Overlay { get; }

		public ulong LoadInstantiateMs { get; }

		public ulong StageMountMs { get; set; }

		public ulong PreplayStartMsec { get; set; }

		public ulong PreplayMs { get; set; }

		public ulong RouteStartMsec { get; set; }

		public AttackCommand? AttackCommand { get; set; }

		public int AttackHitCount { get; set; }

		public int AttackToken { get; set; }

		public bool HasStarted { get; set; }

		public bool HasPlayedAudio { get; set; }

		public bool HasReleasedBattlefieldActors { get; set; }

		public bool HasRestoredBody { get; set; }

		public List<MeirinCinematicHiddenActor> HiddenActors { get; } = new List<MeirinCinematicHiddenActor>();

		public HashSet<ulong> HiddenVisualIds { get; } = new HashSet<ulong>();

		private TaskCompletionSource<ulong> VisualEpochCompletion { get; } = new TaskCompletionSource<ulong>();

		public MeirinCinematicPhaseState(int generation, MeirinCombatRoute route, CardModel card, Player player, NCreature ownerNode, IReadOnlyList<NCreature> targetNodes, Node overlay, ulong loadInstantiateMs)
		{
			Generation = generation;
			Route = route;
			Card = card;
			Player = player;
			OwnerNode = ownerNode;
			TargetNodes = targetNodes;
			Overlay = overlay;
			LoadInstantiateMs = loadInstantiateMs;
		}

		public Task<ulong> WaitForVisualEpochAsync()
		{
			return VisualEpochCompletion.Task;
		}

		public void TryCompleteVisualEpoch(ulong routeStartMsec)
		{
			VisualEpochCompletion.TrySetResult(routeStartMsec);
		}
	}

	private sealed record MeirinCinematicHiddenActor(Node2D Visual, bool WasVisible);

	[HarmonyPatch(typeof(Hook), "ModifyAttackHitCount")]
	private static class NormalAttackEffectPatch
	{
		private static bool Prepare()
		{
			return SkinProfile.Features.CombatEntryAnimation;
		}

		private static void Postfix(object? combatState, AttackCommand attackCommand, decimal __result)
		{
			SkinPatchGuard.Run("Meirin normal attack effects", (Action)delegate
			{
				if (!(__result <= 0m))
				{
					AbstractModel modelSource = attackCommand.ModelSource;
					CardModel val = (CardModel)(object)((modelSource is CardModel) ? modelSource : null);
					if (val != null && IsPresentationTargetCard(val))
					{
						bool flag = PresentationGates.CombatPresentationEffectsEnabled && CombatAudience.Allows(val, (CombatPresentationChannel)1);
						bool flag2 = PresentationGates.CombatPresentationSfxEnabled && CombatAudience.Allows(val, (CombatPresentationChannel)2);
						if (val.Owner != null)
						{
							NCreature val2 = CombatCreatureNodes.ResolvePlayer(NCombatRoom.Instance, val.Owner, "Meirin combat effect actor lookup failed", (Action<string>)delegate(string message)
							{
								SkinLog.Warn(message);
							});
							if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
							{
								IReadOnlyList<Creature> readOnlyList = CombatEffectAttackTargets.ResolveTargets(val, attackCommand, combatState, (Action<string>)delegate(string message)
								{
									SkinLog.Warn(message);
								});
								IReadOnlyList<NCreature> readOnlyList2 = CombatCreatureNodes.ResolveMany(NCombatRoom.Instance, readOnlyList, "Meirin combat effect target lookup failed", (Action<string>)delegate(string message)
								{
									SkinLog.Warn(message);
								});
								string text = ResolveAttackPresentationTrigger(attackCommand);
								if (text == null)
								{
									SkinLog.InfoOnce("meirin_unmapped_attack_presentation_" + ((object)val).GetType().FullName, "Retained native attack presentation because no Meirin trigger mapping was available: card=" + ((object)val).GetType().Name + ".");
								}
								else
								{
									int num = CombatEffectTokens.GetOrCreateValue(val2).Next();
									string cueName;
									string text2;
									if (string.Equals(text, "Attack", StringComparison.OrdinalIgnoreCase))
									{
										MeirinCombatRoute meirinCombatRoute = ResolveMeirinAttackRoute(val, attackCommand, __result);
										if (meirinCombatRoute == MeirinCombatRoute.MultiHitAttack && TryBeginMeirinMultiHitPresentation(attackCommand, val, val2, (int)Math.Max(1m, __result), num))
										{
											SkinLog.Debug("Queued Meirin resolved-target multi-hit presentation");
											return;
										}
										switch (meirinCombatRoute)
										{
										case MeirinCombatRoute.MultiHitAttack:
											SkinLog.Warn("Meirin multi-hit presentation was not accepted; retained native attack presentation.");
											return;
										case MeirinCombatRoute.UgAttack:
											if (TryBeginMeirinUgPresentation(attackCommand, val, val2, readOnlyList2, combatState, (int)Math.Max(1m, __result), num))
											{
												SuppressNativeAttackPresentation(attackCommand);
												SkinLog.Debug("Queued Meirin official attack effects=ug_attack");
												return;
											}
											break;
										}
										if ((uint)(meirinCombatRoute - 2) <= 1u)
										{
											RegisterAttackRouteOverride(val.Owner, "u2_attack");
											cueName = "u2_attack_play";
											text2 = "u2_attack_sequence";
										}
										else
										{
											int num2 = NormalAttackVariants.GetOrCreateValue(val.Owner).Reserve();
											ScheduleNormalAttackVariantExpiry(val.Owner, num2);
											cueName = ((num2 == 1) ? "attack_play1" : "attack_play2");
											text2 = cueName;
										}
									}
									else
									{
										cueName = "f_strong_attack_play";
										text2 = cueName;
									}
									bool triggeredComboContinuation;
									bool flag3 = TryBeginMeirinAttackTimingPresentation(attackCommand, val, val2, text, cueName, (int)Math.Max(1m, __result), num, out triggeredComboContinuation);
									if (!flag3 && !SkinRuntimeSettings.OfficialAttackTimingEnabled)
									{
										flag3 = CanPlayMeirinAttackBodyRoute(val2, text, cueName);
									}
									if (!flag3)
									{
										SkinLog.Warn("Meirin attack presentation was not accepted; retained native attack presentation card=" + ((object)val).GetType().Name + " route=" + cueName + ".");
									}
									else
									{
										SuppressNativeAttackPresentation(attackCommand);
										BeginMeirinBodyReadyAction(val);
										TryBeginMeirinMeleeApproach(val2, readOnlyList2, cueName);
										string text3 = (string.Equals(cueName, "attack_play2", StringComparison.Ordinal) ? "attack_play2_combo" : "attack_play1_combo");
										if (flag)
										{
											CombatEffectRoutes.PlayForTargets(val2, readOnlyList2, triggeredComboContinuation ? text3 : text2, num);
											PlayAttackCameraShake(val2, readOnlyList2, cueName, triggeredComboContinuation ? new float?(0.066f) : ((float?)null));
										}
										if (flag2)
										{
											PlayCombatSfxCues(val2, triggeredComboContinuation ? text3 : cueName, num);
										}
										SkinLog.Debug(() => "Queued Meirin official attack effects=" + cueName);
									}
								}
							}
						}
					}
				}
			}, 2);
		}
	}

	private sealed class EntryCombatEffectCueHost : ICombatTransientEffectCueHost<NCreature>
	{
		public bool IsEnabled()
		{
			return PresentationGates.CombatPresentationEffectsEnabled;
		}

		public bool IsEnabledFor(NCreature? actor)
		{
			CombatPresentationAudiencePolicy combatAudience = CombatAudience;
			object obj;
			if (actor == null)
			{
				obj = null;
			}
			else
			{
				Creature entity = actor.Entity;
				obj = ((entity != null) ? entity.Player : null);
			}
			return combatAudience.Allows((Player)obj, (CombatPresentationChannel)1);
		}

		public bool IsTokenCurrent(NCreature? actor, int token)
		{
			if (actor != null && GodotObject.IsInstanceValid((GodotObject)(object)actor))
			{
				if (token != -1)
				{
					return CombatEffectTokens.GetOrCreateValue(actor).IsCurrent(token);
				}
				return true;
			}
			return false;
		}

		public bool IsActorValid(NCreature? actor)
		{
			if (actor != null)
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)actor);
			}
			return false;
		}

		public void ClearExisting(Node2D spine)
		{
			ClearCombatEffects(spine);
		}

		public PackedScene? LoadScene(string scenePath, bool allowSynchronousSceneLoad)
		{
			return CombatEffects.LoadScene(scenePath, allowSynchronousSceneLoad);
		}

		public Node2D? AcquireRoot(CombatPresentationEffectCue cue, PackedScene scene)
		{
			return CombatEffects.AcquireRoot(cue, scene, true);
		}

		public int PrepareRoot(Node2D effectRoot, Node2D spine, CombatPresentationEffectCue cue)
		{
			return PrepareCombatEffectRoot(effectRoot, spine, cue);
		}

		public bool IsEffectActive(Node2D effectRoot, int generation)
		{
			return CombatEffects.IsActive(effectRoot, generation);
		}

		public bool MarkEffectVisible(Node2D effectRoot, int generation)
		{
			return CombatEffects.MarkPlaybackVisible(effectRoot, generation);
		}

		public void ReleaseRoot(Node2D effectRoot, int? generation)
		{
			CombatEffects.Release(effectRoot, generation);
		}

		public void StartEffect(Node2D effectRoot, CombatPresentationEffectCue cue)
		{
			StartCombatEffect(effectRoot, cue);
		}
	}

	private sealed class EntryCombatEffectRouterHost : ICombatTransientEffectCueRouterHost<NCreature, NCreature>
	{
		public bool IsEnabled()
		{
			return PresentationGates.CombatPresentationEffectsEnabled;
		}

		public bool IsEnabledFor(NCreature? actor)
		{
			CombatPresentationAudiencePolicy combatAudience = CombatAudience;
			object obj;
			if (actor == null)
			{
				obj = null;
			}
			else
			{
				Creature entity = actor.Entity;
				obj = ((entity != null) ? entity.Player : null);
			}
			return combatAudience.Allows((Player)obj, (CombatPresentationChannel)1);
		}

		public Node2D? FindOwnerAnchor(NCreature actor)
		{
			return CombatSpines.Find(actor);
		}

		public Node2D? FindTargetAnchor(NCreature? target)
		{
			return ResolveCombatEffectTargetVisualAnchor(target);
		}

		public Node2D? FindTargetSurfaceAnchor(NCreature? target)
		{
			return ResolveCombatEffectTargetSurfaceAnchor(target);
		}

		public Node2D? FindScreenAnchor()
		{
			return ResolveCombatScreenEffectAnchor((CombatScreenEffectLayer)1);
		}

		public Node2D? FindScreenBackAnchor()
		{
			return ResolveCombatScreenEffectAnchor((CombatScreenEffectLayer)0);
		}

		public void Queue(NCreature actor, Node2D anchor, CombatPresentationEffectCue cue, int token)
		{
			CombatEffectCues.Queue(actor, anchor, cue, token);
		}
	}

	private sealed class EntryCombatSfxCueHost : ICombatSfxCueHost<NCreature>
	{
		public bool IsEnabled()
		{
			return PresentationGates.CombatPresentationSfxEnabled;
		}

		public bool IsEnabledFor(NCreature? actor)
		{
			CombatPresentationAudiencePolicy combatAudience = CombatAudience;
			object obj;
			if (actor == null)
			{
				obj = null;
			}
			else
			{
				Creature entity = actor.Entity;
				obj = ((entity != null) ? entity.Player : null);
			}
			return combatAudience.Allows((Player)obj, (CombatPresentationChannel)2);
		}

		public bool IsTokenCurrent(NCreature? actor, int token)
		{
			if (actor != null && GodotObject.IsInstanceValid((GodotObject)(object)actor))
			{
				if (token != -1)
				{
					return CombatEffectTokens.GetOrCreateValue(actor).IsCurrent(token);
				}
				return true;
			}
			return false;
		}

		public bool IsActorValid(NCreature? actor)
		{
			if (actor != null)
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)actor);
			}
			return false;
		}

		public void PlaySfx(string path, float volume)
		{
			SfxPlayer.Play(path, volume);
		}

		public void Warn(string message, int priority)
		{
			SkinLog.Warn(message, priority);
		}
	}

	private sealed class MeirinPresentationTokenState
	{
		private int _token;

		public int Next()
		{
			_token++;
			if (_token <= 0)
			{
				_token = 1;
			}
			return _token;
		}

		public bool IsCurrent(int token)
		{
			if (token > 0)
			{
				return token == _token;
			}
			return false;
		}
	}

	private sealed class MeirinNormalAttackVariantState
	{
		private int _next;

		private int _pending;

		public int Reserve()
		{
			if (_pending == 0)
			{
				_pending = NextVariant();
			}
			return _pending;
		}

		public int Consume()
		{
			int result = ((_pending != 0) ? _pending : NextVariant());
			_pending = 0;
			return result;
		}

		public void ExpireIfPending(int variant)
		{
			if (_pending == variant)
			{
				_pending = 0;
			}
		}

		private int NextVariant()
		{
			_next = ((_next != 1) ? 1 : 2);
			return _next;
		}
	}

	[HarmonyPatch(typeof(Hook), "BeforeCombatStart")]
	private static class CombatEntryPresentationPatch
	{
		private static bool Prepare()
		{
			return SkinProfile.Features.CombatEntryAnimation;
		}

		private static void Postfix(IRunState runState, object? combatState)
		{
			SkinPatchGuard.Run("Meirin combat entry presentation", (Action)delegate
			{
				object? obj = combatState;
				object? obj2 = ((obj is ICombatState) ? obj : null);
				ScheduleCombatEntries((obj2 != null) ? ((ICombatState)obj2).Players : null);
			}, 2);
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterCombatVictory")]
	private static class CombatVictoryPresentationPatch
	{
		private static bool Prepare()
		{
			return SkinProfile.Features.CombatEntryAnimation;
		}

		private static void Postfix(IRunState runState, object? combatState, CombatRoom room)
		{
			SkinPatchGuard.Run("Meirin combat victory presentation", (Action)delegate
			{
				object? obj = combatState;
				object? obj2 = ((obj is ICombatState) ? obj : null);
				PlayCombatVictoryForPlayers((obj2 != null) ? ((ICombatState)obj2).Players : null);
			}, 2);
		}
	}

	private enum MeirinMeleeStepEffectKind
	{
		MoveStart,
		MoveArrive,
		ReturnStart,
		ReturnArrive
	}

	private sealed class MeirinMeleeMotionState
	{
		public Vector2 OriginalPosition;

		public bool Active;

		public bool Returning;

		public ulong TargetCreatureInstanceId;

		public int StepEffectZIndex;

		public bool PlayStepEffects;

		public float ApproachSeconds = 0.2f;

		public float ReturnDelaySeconds = 0.8667f;

		public float FailsafeSeconds = 1.4f;

		public int Token { get; private set; }

		public int NextToken()
		{
			Token++;
			if (Token <= 0)
			{
				Token = 1;
			}
			return Token;
		}
	}

	private sealed class EntryCharacterContextCutInHost : ICharacterContextCutInHost
	{
		public bool IsEnabled()
		{
			return PresentationGates.CombatPresentationEffectsEnabled;
		}

		public bool IsPlayerCurrent(Player? player)
		{
			return CombatAudience.AllowsUnforecastedCutIn(player);
		}

		public string RuntimeNodeName(string suffix)
		{
			return SkinProfile.RuntimeNodeName(suffix);
		}

		public void Warn(string message, int priority)
		{
			SkinLog.Warn(message, priority);
		}

		public void Info(string message, int priority)
		{
			SkinLog.Info(message, priority);
		}
	}

	[HarmonyPatch(typeof(NCreature), "_Ready")]
	[HarmonyPriority(0)]
	private static class CombatBodyReadyPatch
	{
		private static bool Prepare()
		{
			return SkinProfile.Features.CombatBodyScalePatch;
		}

		private static void Postfix(NCreature? __instance)
		{
			SkinPatchGuard.Run("Meirin combat body ready patch", (Action)delegate
			{
				//IL_0072: Unknown result type (might be due to invalid IL or missing references)
				if (__instance != null)
				{
					CharacterSkinProfile skinProfile = SkinProfile;
					Creature entity = __instance.Entity;
					if (skinProfile.AppliesTo((entity != null) ? entity.Player : null) && CombatBodySurface.Ensure(__instance, "ready"))
					{
						string idleAnimation = SkinProfile.CombatBody.IdleAnimation;
						if (CombatBodySurface.HasAnimation(__instance, idleAnimation, "Meirin combat idle"))
						{
							SpineAnimationCompat.SetAnimation(__instance.SpineAnimation, idleAnimation, true, 0);
							SkinLog.InfoOnce("meirin_combat_idle", "Applied Meirin combat body and idle during creature initialization.");
						}
						TryPlayPendingCombatEntry(__instance);
						RemoteNativeBodyTriggers.TryPlayPendingCombatEntry(__instance);
					}
				}
			}, 2);
		}
	}

	[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
	private static class CombatNativeBodyTriggerPatch
	{
		private static bool Prefix(Creature creature, string triggerName, float waitTime, ref Task __result)
		{
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
			if (creature.IsDead)
			{
				return true;
			}
			try
			{
				Player player = creature.Player;
				if (player == null || !SkinProfile.AppliesTo(player) || !CombatAudience.Allows(player, (CombatPresentationChannel)0))
				{
					return true;
				}
				if (TryHandleMeirinCinematicSupportTrigger(player, triggerName, out Task handledTask))
				{
					__result = handledTask;
					return false;
				}
				if (TryConsumeSupportNativeTrigger(player, triggerName, out string supportRoute))
				{
					__result = WaitForSuppressedSupportTrigger(supportRoute, waitTime);
					SkinLog.Debug(() => "Suppressed duplicate Meirin support body trigger route=" + supportRoute + " native=" + triggerName);
					return false;
				}
				if (TryHandleMeirinMultiHitTrigger(creature, player, triggerName, waitTime, out Task handledTask2))
				{
					__result = handledTask2;
					return false;
				}
				if (TryHandleMeirinAttackTimingTrigger(creature, player, triggerName, waitTime, out Task handledTask3))
				{
					__result = handledTask3;
					return false;
				}
				bool flag = string.Equals(triggerName, "Attack", StringComparison.OrdinalIgnoreCase) || string.Equals(triggerName, "heavyAttack", StringComparison.OrdinalIgnoreCase);
				Task task = default(Task);
				if (!flag && RemoteNativeBodyTriggers.TryPlay(creature, triggerName, waitTime, ref task))
				{
					__result = task;
					return false;
				}
				if (!IsLocalPlayer(player) && !flag)
				{
					return true;
				}
				TryPlayNativeBodyVoice(player, triggerName);
				CharacterSkinRemoteNativeBodyAnimation mapping = default(CharacterSkinRemoteNativeBodyAnimation);
				if (!SkinProfile.CombatBody.TryGetRemoteNativeBodyAnimation(triggerName, ref mapping))
				{
					return true;
				}
				NCreature val = CombatCreatureNodes.ResolvePlayer(NCombatRoom.Instance, player, "Meirin combat body player node lookup failed", (Action<string>)delegate(string message)
				{
					SkinLog.Warn(message);
				});
				if (val == null || !CombatBodySurface.Ensure(val, "native_body_trigger_" + triggerName))
				{
					return true;
				}
				IReadOnlyList<string> sequence = ResolveAnimationSequence(player, mapping);
				if (!ValidateAnimations(val, sequence))
				{
					return true;
				}
				PlayAnimationSequence(val, sequence, mapping);
				__result = WaitForHandledTrigger(triggerName, sequence, waitTime);
				return false;
			}
			catch (Exception ex)
			{
				SkinLog.WarnException("Meirin native body trigger failed: ", ex);
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterDeath")]
	private static class DeathPresentationPatch
	{
		private static bool Prepare()
		{
			if (!SkinProfile.Features.CombatEntryAnimation)
			{
				return SkinProfile.Features.DeathCgOverlay;
			}
			return true;
		}

		private static void Postfix(IRunState runState, object? combatState, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
		{
			SkinPatchGuard.Run("Meirin death presentation", (Action)delegate
			{
				Player player = creature.Player;
				if (!(player == null || !IsTargetPlayer(player) || wasRemovalPrevented))
				{
					ClearMeirinMultiHitPresentation(player, restoreBody: false);
					ClearMeirinBodyReady(player);
					MeirinAttackTimings.Remove(player);
					CancelMeirinCinematicPhase(player, restoreBody: false);
					BattleReadyOverlay.Cleanup(false);
					if (CombatAudience.Allows(player, (CombatPresentationChannel)0))
					{
						CombatDeathSequence.Schedule(creature, (Action<NCreature, string, bool>)null, (Action<string, int>)SkinLog.Info, (Action<string, int>)SkinLog.Warn, CombatBodySurface);
					}
				}
			}, 2);
		}
	}

	[HarmonyPatch(typeof(SfxCmd), "PlayDeath", new Type[] { typeof(Player) })]
	private static class NativePlayerDeathSfxPatch
	{
		private static bool Prefix(Player player)
		{
			if (IsTargetPlayer(player) && IsLocalPlayer(player))
			{
				SkinLog.Info("Suppressed native Watcher death SFX for Meirin.");
				return false;
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(RunManager), "AbandonInternal")]
	private static class AbandonRunDefeatOverlayPatch
	{
		private static bool Prepare()
		{
			return SkinProfile.Features.DeathCgOverlay;
		}

		private static void Prefix()
		{
			SkinPatchGuard.Run("Meirin abandon defeat overlay", (Action)delegate
			{
				if (PresentationGates.DefeatCgOverlayDisplayEnabled)
				{
					RunManager instance = RunManager.Instance;
					RunState val = ((instance != null) ? instance.DebugOnlyGetState() : null);
					if (IsTargetPlayer((val != null) ? LocalContext.GetMe((IPlayerCollection)(object)val) : null))
					{
						BattleReadyOverlay.Cleanup(false);
					}
				}
			}, 2);
		}
	}

	private sealed class EntryCombatEffectMaterialHost : ICombatEffectMaterialHost
	{
		public float GetEffectBrightness()
		{
			return SkinRuntimeSettings.EffectBrightness;
		}

		public float GetEffectOpacity()
		{
			return SkinRuntimeSettings.EffectOpacity;
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_Icon")]
	[HarmonyPriority(0)]
	private static class CharacterIconScenePatch
	{
		private static void Postfix(CharacterModel __instance, ref Control __result)
		{
			if (SkinProfile.AppliesTo(__instance))
			{
				Texture2D val = LoadIdentityTexture("res://MeirinWatcherSkin/images/ui/character_icon_meirin.png");
				if (val != null && __result != null)
				{
					ApplyIdentityTexture((Node)(object)__result, val);
					SkinLog.InfoOnce("meirin_top_bar_icon", "Applied Meirin top-bar character icon.");
				}
			}
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_IconTexture")]
	[HarmonyPriority(0)]
	private static class CharacterIconTexturePatch
	{
		private static void Postfix(CharacterModel __instance, ref Texture2D __result)
		{
			ApplyIdentityTextureResult(__instance, "res://MeirinWatcherSkin/images/ui/character_icon_meirin.png", ref __result);
		}
	}

	[HarmonyPatch(typeof(CharacterModel), "get_IconOutlineTexture")]
	[HarmonyPriority(0)]
	private static class CharacterIconOutlineTexturePatch
	{
		private static void Postfix(CharacterModel __instance, ref Texture2D __result)
		{
			ApplyIdentityTextureResult(__instance, "res://MeirinWatcherSkin/images/ui/character_icon_meirin_outline.png", ref __result);
		}
	}

	[HarmonyPatch]
	private static class WatcherCardLibraryIconPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.TypeByName("WatcherMod.WatcherCardLibraryInjector") ?? throw new TypeLoadException("WatcherMod.WatcherCardLibraryInjector");
			return AccessTools.Method(type, "Inject", new Type[1] { typeof(NCardLibrary) }, (Type[])null) ?? throw new MissingMethodException(type.FullName, "Inject");
		}

		[HarmonyPriority(0)]
		private static void Postfix(NCardLibrary instance)
		{
			Node obj = ((Node)instance).FindChild("WatcherPool", true, false);
			Control val = ((obj != null) ? obj.GetNodeOrNull<Control>(NodePath.op_Implicit("Image")) : null);
			Texture2D val2 = LoadIdentityTexture("res://MeirinWatcherSkin/images/ui/character_icon_meirin.png");
			if (val != null && val2 != null)
			{
				ApplyIdentityTexture((Node)(object)val, val2);
				SkinLog.InfoOnce("meirin_card_library_pool_icon", "Applied Meirin Watcher card-library pool icon.");
			}
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterAttack")]
	private static class MeirinMultiHitCleanupPatch
	{
		private static bool Prepare()
		{
			return SkinProfile.Features.CombatEntryAnimation;
		}

		private static void Postfix(AttackCommand command)
		{
			Creature attacker = command.Attacker;
			Player val = ((attacker != null) ? attacker.Player : null);
			if (val != null && MultiHitStates.TryGetValue(val, out MultiHitState value) && value.Matches(command))
			{
				if (!value.EndQueued && IsMeirinMultiHitStateCurrent(value))
				{
					QueueMeirinMultiHitEnd(value.Actor, "attack_end");
				}
				MultiHitStates.Remove(val);
			}
		}
	}

	private sealed record MultiHitCueCache(MultiHitEffectCueSet Play1Effects, MultiHitEffectCueSet Play2Effects, MultiHitEffectCueSet StrongEffects, MultiHitEffectCueSet U2Effects, IReadOnlyList<CombatPresentationSfxCue> ReadySfx, IReadOnlyList<CombatPresentationSfxCue> Play1Sfx, IReadOnlyList<CombatPresentationSfxCue> Play2Sfx, IReadOnlyList<CombatPresentationSfxCue> StrongSfx, IReadOnlyList<CombatPresentationSfxCue> U2Sfx)
	{
		public MultiHitEffectCueSet ResolveEffects(string cueName)
		{
			return cueName switch
			{
				"attack_play2" => Play2Effects, 
				"f_strong_attack_play" => StrongEffects, 
				"u2_attack_play" => U2Effects, 
				_ => Play1Effects, 
			};
		}

		public IReadOnlyList<CombatPresentationSfxCue> ResolvePlaySfx(string cueName)
		{
			return cueName switch
			{
				"attack_play2" => Play2Sfx, 
				"f_strong_attack_play" => StrongSfx, 
				"u2_attack_play" => U2Sfx, 
				_ => Play1Sfx, 
			};
		}
	}

	private sealed record MultiHitEffectCueSet(IReadOnlyList<CombatPresentationEffectCue> All, IReadOnlyList<CombatPresentationEffectCue> OwnerOnly)
	{
		public static MultiHitEffectCueSet Empty { get; } = new MultiHitEffectCueSet(Array.Empty<CombatPresentationEffectCue>(), Array.Empty<CombatPresentationEffectCue>());

		[CompilerGenerated]
		private MultiHitEffectCueSet(MultiHitEffectCueSet original)
		{
			All = original.All;
			OwnerOnly = original.OwnerOnly;
		}
	}

	private readonly record struct MultiHitBeat(int HitIndex, string CueName, bool IsFinal)
	{
		public bool UsesU2Finisher
		{
			get
			{
				if (IsFinal)
				{
					return string.Equals(CueName, "u2_attack_play", StringComparison.Ordinal);
				}
				return false;
			}
		}

		public string FinalReadyAnimation
		{
			get
			{
				if (!UsesU2Finisher)
				{
					return "f_strong_attack_ready";
				}
				return "u2_attack_ready";
			}
		}

		public string FinalEndAnimation
		{
			get
			{
				if (!UsesU2Finisher)
				{
					return "f_strong_attack_end";
				}
				return "u2_attack_end";
			}
		}

		public float DamageDelaySeconds
		{
			get
			{
				string cueName = CueName;
				if (!(cueName == "u2_attack_play"))
				{
					if (cueName == "f_strong_attack_play")
					{
						return 0.299f;
					}
					return 0.066f;
				}
				return 0.46600002f;
			}
		}
	}

	private sealed class MultiHitState
	{
		private MultiHitBeat? _pendingHit;

		private int _nextHitIndex;

		private readonly ulong _routeStartMsec;

		private readonly bool _useOfficialTiming;

		private ulong _lastImpactStartMsec;

		private ulong? _pendingDamageStartMsec;

		private float _pendingDamageDelaySeconds;

		public AttackCommand Command { get; }

		public CardModel Card { get; }

		public NCreature Actor { get; }

		public int HitCount { get; }

		public int Token { get; }

		public bool UsesU2Finisher { get; }

		public string FinisherCueName
		{
			get
			{
				if (!UsesU2Finisher)
				{
					return "f_strong_attack_play";
				}
				return "u2_attack_play";
			}
		}

		public bool HasPlayedVoice { get; private set; }

		public bool EndQueued { get; private set; }

		public MultiHitState(AttackCommand command, CardModel card, NCreature actor, int hitCount, int token, ulong routeStartMsec, bool useOfficialTiming, bool usesU2Finisher)
		{
			Command = command;
			Card = card;
			Actor = actor;
			HitCount = hitCount;
			Token = token;
			UsesU2Finisher = usesU2Finisher;
			_routeStartMsec = routeStartMsec;
			_useOfficialTiming = useOfficialTiming;
			_lastImpactStartMsec = 0uL;
		}

		public bool Matches(Player player, Creature creature)
		{
			if (Card.Owner == player)
			{
				return Command.Attacker == creature;
			}
			return false;
		}

		public bool Matches(CardModel card)
		{
			return Card == card;
		}

		public bool Matches(AttackCommand command)
		{
			return Command == command;
		}

		public bool TryQueueNextHit(ulong nowMsec, out MultiHitBeat beat, out float remainingSeconds)
		{
			if (_nextHitIndex >= HitCount)
			{
				beat = default(MultiHitBeat);
				remainingSeconds = 0f;
				return false;
			}
			int num = _nextHitIndex++;
			bool flag = num == HitCount - 1;
			beat = new MultiHitBeat(num, flag ? FinisherCueName : (((num & 1) == 0) ? "attack_play1" : "attack_play2"), flag);
			_pendingHit = beat;
			ulong num2 = ((num == 0) ? _routeStartMsec : _lastImpactStartMsec);
			float num3 = ((num == 0) ? 0.2f : 0.2f);
			remainingSeconds = (_useOfficialTiming ? CombatAttackTimingSession.ResolveRemainingDelaySeconds(num2, num3, nowMsec) : 0f);
			return true;
		}

		public bool TryResolveNextHit(out MultiHitBeat beat)
		{
			MultiHitBeat? pendingHit = _pendingHit;
			if (pendingHit.HasValue)
			{
				MultiHitBeat valueOrDefault = pendingHit.GetValueOrDefault();
				beat = valueOrDefault;
				_pendingHit = null;
				return true;
			}
			beat = default(MultiHitBeat);
			return false;
		}

		public void MarkImpactStarted(ulong nowMsec, float damageDelaySeconds)
		{
			_lastImpactStartMsec = nowMsec;
			_pendingDamageStartMsec = nowMsec;
			_pendingDamageDelaySeconds = Math.Max(0f, damageDelaySeconds);
		}

		public float ConsumeDamageWait(ulong nowMsec)
		{
			ulong? pendingDamageStartMsec = _pendingDamageStartMsec;
			if (pendingDamageStartMsec.HasValue)
			{
				ulong valueOrDefault = pendingDamageStartMsec.GetValueOrDefault();
				if (_useOfficialTiming)
				{
					_pendingDamageStartMsec = null;
					return CombatAttackTimingSession.ResolveRemainingDelaySeconds(valueOrDefault, _pendingDamageDelaySeconds, nowMsec);
				}
			}
			_pendingDamageStartMsec = null;
			return 0f;
		}

		public void MarkVoicePlayed()
		{
			HasPlayedVoice = true;
		}

		public void MarkEndQueued()
		{
			EndQueued = true;
		}
	}

	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	private static class WatcherRestSiteScenePathPatch
	{
		[HarmonyPriority(0)]
		private static void Postfix(CharacterModel __instance, ref string __result)
		{
			if (((object)__instance).GetType() == typeof(Watcher))
			{
				__result = "res://MeirinWatcherSkin/scenes/rest_site/meirin_rest_site.tscn";
			}
		}
	}

	[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
	[HarmonyPriority(0)]
	private static class RestSiteBodySurfacePatch
	{
		private static bool Prefix(NRestSiteCharacter __instance)
		{
			if (!IsTargetPlayer(__instance.Player))
			{
				return true;
			}
			return !TryInitializeRestSiteInteraction(__instance);
		}

		private static void Postfix(NRestSiteCharacter __instance)
		{
			SkinPatchGuard.Run("Meirin rest-site body surface", (Action)delegate
			{
				if (IsTargetPlayer(__instance.Player))
				{
					RestSitePresentation.Apply(__instance);
				}
			}, 2);
		}
	}

	[HarmonyPatch(typeof(NMerchantCharacter), "_Ready")]
	[HarmonyPriority(0)]
	private static class MerchantBodySurfacePatch
	{
		private static bool Prefix(NMerchantCharacter __instance)
		{
			return !IsMeirinMerchantSurface(__instance);
		}

		private static void Postfix(NMerchantCharacter __instance)
		{
			SkinPatchGuard.Run("Meirin merchant body surface", (Action)delegate
			{
				if (IsTargetPlayer(ResolveMerchantCharacterPlayer(__instance)))
				{
					MerchantPresentation.Apply(__instance);
				}
			}, 2);
		}
	}

	[HarmonyPatch(typeof(NFakeMerchant), "_Ready")]
	[HarmonyPriority(0)]
	private static class FakeMerchantBodySurfacePatch
	{
		private static void Postfix(NFakeMerchant __instance)
		{
			SkinPatchGuard.Run("Meirin fake merchant body surface", (Action)delegate
			{
				MerchantPresentation.Apply(__instance);
			}, 2);
		}
	}

	private sealed class EntryRestSitePresentationRuntimeContext : IRestSitePresentationRuntimeContext
	{
		public IReadOnlyList<Node> FindSpineSprites(Node root)
		{
			return Entry.FindSpineSprites(root);
		}

		public void ApplyLayerMaterial(Node root)
		{
		}

		public void ApplyLayerMaterialDeferred(Node root, int maxFrames)
		{
		}

		public void StartLayerEffects(Node root, bool visible)
		{
		}
	}

	[HarmonyPatch(typeof(NGameOverScreen), "MoveCreaturesToDifferentLayerAndDisableUi")]
	private static class GameOverVisualsScalePatch
	{
		private static readonly string GameOverScaleMeta = SkinProfile.RuntimeMetaKey("game_over_scaled");

		private static readonly string GameOverDeathSequenceMeta = SkinProfile.RuntimeMetaKey("game_over_death_sequence");

		private const float MaxStandaloneUnscaledSpineScale = 0.75f;

		private static void Postfix(NGameOverScreen __instance)
		{
			SkinPatchGuard.Run("Meirin game-over body presentation", (Action)delegate
			{
				if (NCombatRoom.Instance == null)
				{
					RunManager instance = RunManager.Instance;
					RunState val = ((instance != null) ? instance.DebugOnlyGetState() : null);
					if (val != null)
					{
						Player me = LocalContext.GetMe((IPlayerCollection)(object)val);
						if (IsTargetPlayer(me))
						{
							Control value = Traverse.Create((object)__instance).Field("_creatureContainer").GetValue<Control>();
							if (value != null)
							{
								foreach (Node child in ((Node)value).GetChildren(false))
								{
									GameOverVisualScaleRuntime.ScaleStandaloneSpineSprites(child, GameOverScaleMeta, SkinProfile.BodySpineScaleMultiplier, 0.75f);
								}
								if (me != null && me.Creature.IsDead)
								{
									GameOverVisualScaleRuntime.ScheduleStandaloneDeathSequence((Node)(object)value, GameOverDeathSequenceMeta, (IReadOnlyList<string>)SkinProfile.CombatBody.DeathSequence, (Action<string, int>)SkinLog.Info, (Action<string, int>)SkinLog.Warn);
								}
							}
						}
					}
				}
			}, 2);
		}
	}

	[HarmonyPatch(typeof(Hook), "BeforeCombatStart")]
	private static class CombatStartVoicePatch
	{
		private static void Postfix(IRunState runState, object? combatState)
		{
			Player me = LocalContext.GetMe((IPlayerCollection)(object)runState);
			if (IsTargetPlayer(me))
			{
				VoicePlayer.PlayFor(me, SkinProfile.Voice.CombatStartPath, 1f);
			}
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterCombatVictory")]
	private static class CombatVictoryVoicePatch
	{
		private static void Postfix(IRunState runState, object? combatState, CombatRoom room)
		{
			Player me = LocalContext.GetMe((IPlayerCollection)(object)runState);
			if (IsTargetPlayer(me))
			{
				VoicePlayer.PlayFor(me, SkinProfile.Voice.VictoryPath, 1f);
			}
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterDamageReceived")]
	private static class DamageVoicePatch
	{
		private static void Postfix(PlayerChoiceContext choiceContext, IRunState runState, object? combatState, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			CombatDamageVoice.TryPlayHitVoice(target, result, 1f);
		}
	}

	private sealed class WatcherBodyVfxFollowScope
	{
		private readonly Dictionary<Node, WatcherBodyVfxFollowAnchor> _anchors = new Dictionary<Node, WatcherBodyVfxFollowAnchor>();

		internal Creature Owner { get; }

		public WatcherBodyVfxFollowScope(Creature owner, Node2D spine)
		{
			Owner = owner;
			base._002Ector();
		}

		internal void Attach(Node2D vfx)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			Node parent = ((Node)vfx).GetParent();
			if (parent != null)
			{
				if (!_anchors.TryGetValue(parent, out WatcherBodyVfxFollowAnchor value))
				{
					value = new WatcherBodyVfxFollowAnchor();
					parent.AddChild((Node)(object)value, false, (InternalMode)0);
					value.Initialize(spine, ResolveCurrentMeirinBodyDisplacement(spine));
					_anchors.Add(parent, value);
				}
				((Node)vfx).Reparent((Node)(object)value, false);
			}
		}
	}

	[HarmonyPatch]
	private static class BoninallWatcherAudioPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.TypeByName("WatcherMod.WatcherAudioHelper") ?? throw new TypeLoadException("WatcherMod.WatcherAudioHelper");
			return AccessTools.Method(type, "PlayOneShot", new Type[2]
			{
				typeof(string),
				typeof(float)
			}, (Type[])null) ?? throw new MissingMethodException(type.FullName, "PlayOneShot");
		}

		private static bool Prefix(string resPath)
		{
			if (resPath.Contains("/audio/watcher/", StringComparison.Ordinal))
			{
				return true;
			}
			SkinLog.InfoOnce("meirin_suppressed_boninall_attack_audio", "Suppressed Boninall Watcher attack audio replaced by Meirin.");
			return false;
		}
	}

	[HarmonyPatch]
	private static class BoninallStanceVfxAttachmentPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.TypeByName("WatcherMod.StanceVfxHelper") ?? throw new TypeLoadException("WatcherMod.StanceVfxHelper");
			return AccessTools.Method(type, "SpawnOnCreature", new Type[2]
			{
				typeof(Creature),
				typeof(Node2D)
			}, (Type[])null) ?? throw new MissingMethodException(type.FullName, "SpawnOnCreature");
		}

		private static void Postfix(Creature owner, ref Node2D? __result)
		{
			Node2D stanceVfx = __result;
			SkinPatchGuard.Run("Watcher stance VFX attachment", (Action)delegate
			{
				//IL_0072: Unknown result type (might be due to invalid IL or missing references)
				//IL_0078: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				if (stanceVfx != null && GodotObject.IsInstanceValid((GodotObject)(object)stanceVfx))
				{
					Creature obj = owner;
					if (IsTargetPlayer((obj != null) ? obj.Player : null))
					{
						CombatCreatureSpineResolver combatSpines = CombatSpines;
						NCombatRoom instance = NCombatRoom.Instance;
						Node2D val = combatSpines.Find((instance != null) ? instance.GetCreatureNode(owner) : null);
						if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && ((Node)stanceVfx).GetParent() != val)
						{
							Node2D obj2 = stanceVfx;
							obj2.GlobalPosition += ResolveCurrentMeirinBodyDisplacement(val);
							((Node)stanceVfx).Reparent((Node)(object)val, true);
						}
					}
				}
			}, 2);
		}
	}

	[HarmonyPatch]
	private static class BoninallBodyCenteredVfxPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.TypeByName("WatcherMod.WatcherAttackVfxHelper") ?? throw new TypeLoadException("WatcherMod.WatcherAttackVfxHelper");
			return AccessTools.Method(type, "CreatureCenter", new Type[1] { typeof(Creature) }, (Type[])null) ?? throw new MissingMethodException(type.FullName, "CreatureCenter");
		}

		private static void Postfix(Creature creature, ref Vector2 __result)
		{
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			if (IsTargetPlayer((creature != null) ? creature.Player : null) && ActiveWatcherBodyVfxFollowScope?.Owner != creature)
			{
				CombatCreatureSpineResolver combatSpines = CombatSpines;
				NCombatRoom instance = NCombatRoom.Instance;
				Node2D val = combatSpines.Find((instance != null) ? instance.GetCreatureNode(creature) : null);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					__result += ResolveCurrentMeirinBodyDisplacement(val);
				}
			}
		}
	}

	[HarmonyPatch]
	private static class BoninallDivinityStanceChangeFollowPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.TypeByName("WatcherMod.WatcherAttackVfxHelper") ?? throw new TypeLoadException("WatcherMod.WatcherAttackVfxHelper");
			return AccessTools.Method(type, "PlayDivinityStanceChange", new Type[1] { typeof(Creature) }, (Type[])null) ?? throw new MissingMethodException(type.FullName, "PlayDivinityStanceChange");
		}

		private static void Prefix(Creature owner, out WatcherBodyVfxFollowScope? __state)
		{
			__state = BeginWatcherBodyVfxFollowScope(owner);
		}

		private static void Postfix(WatcherBodyVfxFollowScope? __state)
		{
			ActiveWatcherBodyVfxFollowScope = __state;
		}
	}

	[HarmonyPatch]
	private static class BoninallDivinityEyeFollowPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = (AccessTools.TypeByName("WatcherMod.WatcherAttackVfxHelper") ?? throw new TypeLoadException("WatcherMod.WatcherAttackVfxHelper")).GetNestedType("DivinityEyeInstance", BindingFlags.NonPublic) ?? throw new TypeLoadException("WatcherMod.WatcherAttackVfxHelper+DivinityEyeInstance");
			return AccessTools.Method(type, "Tick", new Type[1] { typeof(float) }, (Type[])null) ?? throw new MissingMethodException(type.FullName, "Tick");
		}

		private static void Prefix(Creature ___Owner, out WatcherBodyVfxFollowScope? __state)
		{
			__state = BeginWatcherBodyVfxFollowScope(___Owner);
		}

		private static void Postfix(WatcherBodyVfxFollowScope? __state)
		{
			ActiveWatcherBodyVfxFollowScope = __state;
		}
	}

	[HarmonyPatch]
	private static class BoninallGlobalBodyVfxAttachmentPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.TypeByName("WatcherMod.WatcherAttackVfxHelper") ?? throw new TypeLoadException("WatcherMod.WatcherAttackVfxHelper");
			return AccessTools.Method(type, "TryAddToCombatVfx", new Type[2]
			{
				typeof(Node),
				typeof(bool)
			}, (Type[])null) ?? throw new MissingMethodException(type.FullName, "TryAddToCombatVfx");
		}

		private static void Postfix(Node node, bool __result)
		{
			if (__result)
			{
				Node2D val = (Node2D)(object)((node is Node2D) ? node : null);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					ActiveWatcherBodyVfxFollowScope?.Attach(val);
				}
			}
		}
	}

	[HarmonyPatch]
	private static class BoninallDirectCombatVfxPatch
	{
		[CompilerGenerated]
		private sealed class _003CTargetMethods_003Ed__1 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
		{
			private int _003C_003E1__state;

			private MethodBase _003C_003E2__current;

			private int _003C_003El__initialThreadId;

			private List<MethodInfo>.Enumerator _003C_003E7__wrap1;

			MethodBase IEnumerator<MethodBase>.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			object IEnumerator.Current
			{
				[DebuggerHidden]
				get
				{
					return _003C_003E2__current;
				}
			}

			[DebuggerHidden]
			public _003CTargetMethods_003Ed__1(int _003C_003E1__state)
			{
				this._003C_003E1__state = _003C_003E1__state;
				_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
			}

			[DebuggerHidden]
			void IDisposable.Dispose()
			{
				int num = _003C_003E1__state;
				if (num == -3 || num == 1)
				{
					try
					{
					}
					finally
					{
						_003C_003Em__Finally1();
					}
				}
				_003C_003E7__wrap1 = default(List<MethodInfo>.Enumerator);
				_003C_003E1__state = -2;
			}

			private bool MoveNext()
			{
				try
				{
					Type type3;
					switch (_003C_003E1__state)
					{
					default:
						return false;
					case 0:
					{
						_003C_003E1__state = -1;
						Type type2 = AccessTools.TypeByName("WatcherMod.WatcherAttackVfxHelper") ?? throw new TypeLoadException("WatcherMod.WatcherAttackVfxHelper");
						_003C_003E7__wrap1 = AccessTools.GetDeclaredMethods(type2).GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_00b6;
					}
					case 1:
						_003C_003E1__state = -3;
						goto IL_00b6;
					case 2:
					{
						_003C_003E1__state = -1;
						Type type = AccessTools.TypeByName("WatcherMod.WatcherCombatHelper") ?? throw new TypeLoadException("WatcherMod.WatcherCombatHelper");
						_003C_003E2__current = AccessTools.Method(type, "UpdateEyeAnimation", new Type[2]
						{
							typeof(Creature),
							typeof(Type)
						}, (Type[])null) ?? throw new MissingMethodException(type.FullName, "UpdateEyeAnimation");
						_003C_003E1__state = 3;
						return true;
					}
					case 3:
						{
							_003C_003E1__state = -1;
							return false;
						}
						IL_00b6:
						while (_003C_003E7__wrap1.MoveNext())
						{
							MethodInfo current = _003C_003E7__wrap1.Current;
							if (current.ReturnType == typeof(void) && AttackVfxMethodNames.Contains(current.Name))
							{
								_003C_003E2__current = current;
								_003C_003E1__state = 1;
								return true;
							}
						}
						_003C_003Em__Finally1();
						_003C_003E7__wrap1 = default(List<MethodInfo>.Enumerator);
						type3 = AccessTools.TypeByName("WatcherMod.WatcherCinematicHelper") ?? throw new TypeLoadException("WatcherMod.WatcherCinematicHelper");
						_003C_003E2__current = AccessTools.Method(type3, "PlaySignatureCut", new Type[1] { typeof(Creature) }, (Type[])null) ?? throw new MissingMethodException(type3.FullName, "PlaySignatureCut");
						_003C_003E1__state = 2;
						return true;
					}
				}
				catch
				{
					//try-fault
					((IDisposable)this).Dispose();
					throw;
				}
			}

			bool IEnumerator.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				return this.MoveNext();
			}

			private void _003C_003Em__Finally1()
			{
				_003C_003E1__state = -1;
				((IDisposable)_003C_003E7__wrap1/*cast due to .constrained prefix*/).Dispose();
			}

			[DebuggerHidden]
			void IEnumerator.Reset()
			{
				throw new NotSupportedException();
			}

			[DebuggerHidden]
			IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
			{
				if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
				{
					_003C_003E1__state = 0;
					return this;
				}
				return new _003CTargetMethods_003Ed__1(0);
			}

			[DebuggerHidden]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return ((IEnumerable<MethodBase>)this).GetEnumerator();
			}
		}

		private static readonly HashSet<string> AttackVfxMethodNames = new HashSet<string>(StringComparer.Ordinal)
		{
			"PlayCleave", "PlayFlyingSleevesSlash", "PlayFlickCoin", "PlayClash", "PlayWallop", "PlayViolentAttack", "PlayExpunge", "TriggerExpungeSlash", "PlayEnergyBlade", "PlayBladeImpact",
			"TriggerSignatureSwing", "SetStaffEyeAnimation", "PlayEyeRadiance", "PlaySignatureTrail", "PlaySignatureDischarge", "PlaySignatureImpact"
		};

		[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__1))]
		private static IEnumerable<MethodBase> TargetMethods()
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003CTargetMethods_003Ed__1(-2);
		}

		private static bool Prefix(MethodBase __originalMethod)
		{
			SkinLog.InfoOnce("meirin_suppressed_boninall_direct_combat_vfx", "Suppressed Boninall direct combat VFX/body helper: " + __originalMethod.DeclaringType?.Name + "." + __originalMethod.Name + ".");
			return false;
		}
	}

	[HarmonyPatch]
	private static class BoninallSignatureCutDecisionPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.TypeByName("WatcherMod.WatcherCinematicHelper") ?? throw new TypeLoadException("WatcherMod.WatcherCinematicHelper");
			return AccessTools.Method(type, "ShouldPlayCutFor", new Type[1] { typeof(Creature) }, (Type[])null) ?? throw new MissingMethodException(type.FullName, "ShouldPlayCutFor");
		}

		private static bool Prefix(Creature caster, ref bool __result)
		{
			if (!IsTargetPlayer((caster != null) ? caster.Player : null))
			{
				return true;
			}
			__result = false;
			SkinLog.InfoOnce("meirin_suppressed_boninall_signature_cut", "Suppressed Boninall SignatureMove cinematic for Meirin.");
			return false;
		}
	}

	[HarmonyPatch(typeof(Watcher), "GenerateAnimator")]
	[HarmonyPriority(800)]
	private static class WatcherAnimatorPatch
	{
		private static bool Prefix(Watcher __instance, MegaSprite controller, ref CreatureAnimator __result)
		{
			if (((object)__instance).GetType() != typeof(Watcher))
			{
				return true;
			}
			__result = MeirinWatcherAnimator.Create(controller);
			SkinLog.InfoOnce("meirin_native_watcher_animator", "Applied Meirin animator while preserving canonical WatcherMod.Watcher.");
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateCalledMethods_003Ed__47 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private MethodBase method;

		public MethodBase _003C_003E3__method;

		private byte[] _003Cil_003E5__2;

		private Module _003Cmodule_003E5__3;

		private Type[] _003CtypeArguments_003E5__4;

		private Type[] _003CmethodArguments_003E5__5;

		private int _003Coffset_003E5__6;

		MethodBase IEnumerator<MethodBase>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CEnumerateCalledMethods_003Ed__47(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cil_003E5__2 = null;
			_003Cmodule_003E5__3 = null;
			_003CtypeArguments_003E5__4 = null;
			_003CmethodArguments_003E5__5 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				try
				{
					_003Cil_003E5__2 = method.GetMethodBody()?.GetILAsByteArray();
				}
				catch (Exception)
				{
					_003Cil_003E5__2 = null;
				}
				if (_003Cil_003E5__2 == null)
				{
					return false;
				}
				_003Cmodule_003E5__3 = method.Module;
				Type declaringType = method.DeclaringType;
				_003CtypeArguments_003E5__4 = (((object)declaringType != null && declaringType.IsGenericType) ? declaringType.GetGenericArguments() : null);
				_003CmethodArguments_003E5__5 = (method.IsGenericMethod ? method.GetGenericArguments() : null);
				_003Coffset_003E5__6 = 0;
				break;
			}
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			while (_003Coffset_003E5__6 < _003Cil_003E5__2.Length)
			{
				if (!TryReadIlOpCode(_003Cil_003E5__2, ref _003Coffset_003E5__6, out var opCode))
				{
					return false;
				}
				int num = _003Coffset_003E5__6;
				if (!TryAdvanceIlOperand(_003Cil_003E5__2, opCode, ref _003Coffset_003E5__6))
				{
					return false;
				}
				OperandType operandType = opCode.OperandType;
				bool flag = ((operandType == OperandType.InlineMethod || operandType == OperandType.InlineTok) ? true : false);
				if (flag && num + 4 <= _003Cil_003E5__2.Length)
				{
					int metadataToken = BitConverter.ToInt32(_003Cil_003E5__2, num);
					MethodBase methodBase = null;
					try
					{
						methodBase = _003Cmodule_003E5__3.ResolveMethod(metadataToken, _003CtypeArguments_003E5__4, _003CmethodArguments_003E5__5);
					}
					catch (Exception)
					{
					}
					if (methodBase != null)
					{
						_003C_003E2__current = methodBase;
						_003C_003E1__state = 1;
						return true;
					}
				}
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			_003CEnumerateCalledMethods_003Ed__47 _003CEnumerateCalledMethods_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateCalledMethods_003Ed__ = this;
			}
			else
			{
				_003CEnumerateCalledMethods_003Ed__ = new _003CEnumerateCalledMethods_003Ed__47(0);
			}
			_003CEnumerateCalledMethods_003Ed__.method = _003C_003E3__method;
			return _003CEnumerateCalledMethods_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateScannableMethods_003Ed__46 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Type cardType;

		public Type _003C_003E3__cardType;

		private MethodInfo[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		private Type[] _003C_003E7__wrap3;

		private int _003C_003E7__wrap4;

		MethodBase IEnumerator<MethodBase>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CEnumerateScannableMethods_003Ed__46(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = cardType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				_003C_003E7__wrap2 = 0;
				goto IL_0071;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E7__wrap2++;
				goto IL_0071;
			case 2:
				{
					_003C_003E1__state = -1;
					_003C_003E7__wrap4++;
					goto IL_00fc;
				}
				IL_0071:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
				{
					MethodInfo methodInfo = _003C_003E7__wrap1[_003C_003E7__wrap2];
					_003C_003E2__current = methodInfo;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E7__wrap1 = null;
				_003C_003E7__wrap3 = cardType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
				_003C_003E7__wrap2 = 0;
				goto IL_0121;
				IL_0121:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap3.Length)
				{
					Type type = _003C_003E7__wrap3[_003C_003E7__wrap2];
					_003C_003E7__wrap1 = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					_003C_003E7__wrap4 = 0;
					goto IL_00fc;
				}
				_003C_003E7__wrap3 = null;
				return false;
				IL_00fc:
				if (_003C_003E7__wrap4 < _003C_003E7__wrap1.Length)
				{
					MethodInfo methodInfo2 = _003C_003E7__wrap1[_003C_003E7__wrap4];
					_003C_003E2__current = methodInfo2;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003E7__wrap1 = null;
				_003C_003E7__wrap2++;
				goto IL_0121;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			_003CEnumerateScannableMethods_003Ed__46 _003CEnumerateScannableMethods_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateScannableMethods_003Ed__ = this;
			}
			else
			{
				_003CEnumerateScannableMethods_003Ed__ = new _003CEnumerateScannableMethods_003Ed__46(0);
			}
			_003CEnumerateScannableMethods_003Ed__.cardType = _003C_003E3__cardType;
			return _003CEnumerateScannableMethods_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	private static readonly ConditionalWeakTable<Player, MeirinAttackTimingState> MeirinAttackTimings = new ConditionalWeakTable<Player, MeirinAttackTimingState>();

	private static readonly CombatBlockGainFeedbackRuntime BlockGainFeedback;

	private const ulong SupportNativeTriggerSuppressionMs = 750uL;

	private static readonly ConditionalWeakTable<Player, SupportNativeTriggerGate> SupportNativeTriggerGates;

	private static readonly Dictionary<Type, MeirinCardCapabilities> CardCapabilityCache;

	private static readonly Dictionary<short, OpCode> IlOpCodesByValue;

	private const string WatcherCombatHelperTypeName = "WatcherCombatHelper";

	private const string AttackCommandTypeName = "AttackCommand";

	private const string WatcherAttackCommandCompatTypeName = "WatcherAttackCommandCompat";

	private const string RandomOpponentTargetingMethodName = "TargetingRandomOpponents";

	private const string RandomOpponentTargetingCompatMethodName = "TargetingRandomOpponentsCompat";

	private const string MantraPowerTypeName = "Mantra";

	private static readonly string[] PowerApplyDeclaringTypeNames;

	private static readonly string[] StanceEntryMethodNames;

	private const string CharacterSelectIconPath = "res://MeirinWatcherSkin/images/character_select/char_select_meirin.png";

	private const string CharacterSelectLockedIconPath = "res://MeirinWatcherSkin/images/character_select/char_select_meirin_locked.png";

	private static readonly HashSet<string> CharacterSelectPmaSpineNodeNames;

	private static Material? _characterSelectPmaMaterial;

	private static readonly ConditionalWeakTable<Player, MeirinPresentationTokenState> MeirinBodyReadyPlaybackTokens;

	private static readonly ConditionalWeakTable<Player, MeirinPresentationTokenState> MeirinBodyReadyActionTokens;

	private static readonly MeirinBodyReadyPresentationRuntime MeirinBodyReadyPresentation;

	private static readonly Lazy<CombatPresentationController<Player>> MeirinBodyReadyControllerLazy;

	private static readonly Lazy<CombatPresentationStatePlaybackRuntime<Player>> MeirinBodyReadyStatesLazy;

	private static readonly Lazy<CombatBodyReadyPresentationRuntime<Player, CardModel>> MeirinBodyReadyLazy;

	private static readonly Lazy<CombatReadyIdleVoiceRuntime<Player>> MeirinReadyIdleVoicesLazy;

	private static readonly Lazy<RemoteNativeBodyTriggerRuntime> RemoteNativeBodyTriggersLazy;

	private const string MeirinUgOverlayScenePath = "res://MeirinWatcherSkin/scenes/combat_ug_overlay.tscn";

	private const string MeirinUxOverlayScenePath = "res://MeirinWatcherSkin/scenes/combat_ux_overlay.tscn";

	private const string MeirinUgAttackRouteName = "ug_attack";

	private const float MeirinUgHitSeconds = 2.2f;

	private const float MeirinUxHitSeconds = 2.7f;

	private const float MeirinUgDurationSeconds = 4.0666f;

	private const float MeirinUxDurationSeconds = 4.2667f;

	private const float MeirinCinematicReleasePaddingSeconds = 1f;

	private const int MeirinCinematicTargetProxyDuplicateFlags = 0;

	private const int MeirinCinematicMaximumTargetProxies = 5;

	private static PackedScene? _meirinUgOverlayScene;

	private static PackedScene? _meirinUxOverlayScene;

	private static MeirinCinematicPhaseState? _meirinCinematicPhase;

	private static int _meirinCinematicGeneration;

	private static readonly ConditionalWeakTable<Player, MeirinCinematicSupportTimingState> MeirinCinematicSupportTimings;

	private const int CombatEffectPoolCap = 2;

	private const int CombatEffectMaximumPoolCap = 4;

	private const float MeirinSts2EffectComfortBrightness = 0.55f;

	private const float MeirinSts2EffectComfortOpacity = 0.85f;

	private static readonly Vector2 CombatScreenEffectReferenceSize;

	private static readonly CombatAttackEffectTargetResolver CombatEffectAttackTargets;

	private static readonly ConditionalWeakTable<NCreature, MeirinPresentationTokenState> CombatEffectTokens;

	private static readonly ConditionalWeakTable<Player, MeirinNormalAttackVariantState> NormalAttackVariants;

	private const string U2AttackRouteName = "u2_attack";

	private static readonly ConditionalWeakTable<Player, SupportNativeTriggerGate> AttackRouteGates;

	private static readonly FieldInfo? AttackCommandAttackerVfxField;

	private static readonly FieldInfo? AttackCommandAttackerSfxField;

	private static readonly FieldInfo? AttackCommandTmpAttackerSfxField;

	private static readonly FieldInfo? AttackCommandHitVfxField;

	private static readonly FieldInfo? AttackCommandHitSfxField;

	private static readonly FieldInfo? AttackCommandTmpHitSfxField;

	private static readonly FieldInfo? AttackCommandCustomAttackerVfxNodesField;

	private static readonly FieldInfo? AttackCommandCustomHitVfxNodesField;

	private static readonly FieldInfo? AttackCommandAttackerAnimNameField;

	private static readonly FieldInfo? AttackCommandShouldPlayAnimationField;

	private static readonly CombatWarmupPolicyProfile CombatWarmupPolicy;

	private static readonly Lazy<CharacterEffectEngine> CombatEffectEngineLazy;

	private static readonly Lazy<CombatTransientEffectCueRuntime<NCreature>> CombatEffectCuesLazy;

	private static readonly Lazy<CombatTransientEffectCueRouter<NCreature, NCreature>> CombatEffectRoutesLazy;

	private static readonly Lazy<CombatSfxCueRuntime<NCreature>> CombatSfxCuesLazy;

	private static readonly HashSet<Player> PendingCombatEntryPlayers;

	private static readonly Lazy<CombatBodySequenceRuntime> CombatEntryBodySequencesLazy;

	private static int _combatEntryToken;

	private const string MeirinMeleePlayerMoveScenePath = "res://MeirinWatcherSkin/scenes/combat_melee_player_move.tscn";

	private const string MeirinMeleePlayerArriveScenePath = "res://MeirinWatcherSkin/scenes/combat_melee_player_arrive.tscn";

	private const float MeirinMeleePlayerMoveLifetimeSeconds = 0.9f;

	private const float MeirinMeleePlayerArriveLifetimeSeconds = 0.8f;

	private const float MeirinMeleeStepEffectScale = 1f;

	private const float MeirinMeleeReturnStepEffectScale = 0.7f;

	private static readonly Vector2 MeirinMeleeApproachStepOfficialOffset;

	private static readonly Vector2 MeirinMeleeReturnStepSts2Offset;

	private const float MeirinMeleeApproachSeconds = 0.2f;

	private const float MeirinMeleeReturnDelaySeconds = 0.8667f;

	private const float MeirinMeleeFailsafeSeconds = 1.4f;

	private const float MeirinMeleeApproachSnapDelaySeconds = 0.04f;

	private const float MeirinMeleeReturnSnapDelaySeconds = 0.06f;

	private const float MeirinMeleeMinimumTravelX = 40f;

	private const float MeirinMeleeMinimumMaximumTravelX = 760f;

	private const float MeirinMeleeMaximumTravelViewportFraction = 0.85f;

	private const float MeirinU2MeleeApproachSeconds = 0.4f;

	private const float MeirinU2MeleeReturnDelaySeconds = 1.0667f;

	private const float MeirinU2MeleeFailsafeSeconds = 1.6f;

	private const float MeirinAttackPlay1CloseCombatOffsetX = -100f;

	private const float MeirinAttackPlay2CloseCombatOffsetX = -156f;

	private const float MeirinU2AttackCloseCombatOffsetX = -156f;

	private const float MeirinStrongFinisherSts2OffsetX = -156f;

	private static readonly ConditionalWeakTable<Node2D, MeirinMeleeMotionState> MeirinMeleeMotions;

	private static readonly HashSet<Node2D> ActiveMeirinMeleeSpines;

	private static readonly HashSet<Node2D> ActiveMeirinMeleeStepEffects;

	private const string HarmonyId = "local.meirinwatcherskin";

	private const float NormalAttackReadySeconds = 0.2f;

	private const float NormalAttackPlayHitDelaySeconds = 0.066f;

	private const float NormalAttackHitDelaySeconds = 0.266f;

	private const float NormalAttackMotionSkipFastSeconds = 0.2f;

	private const float HeavyAttackReadySeconds = 0.2f;

	private const float HeavyAttackPlayHitDelaySeconds = 0.099f;

	private const float HeavyAttackHitDelaySeconds = 0.299f;

	private const float DefenseHitDelaySeconds = 0.4667f;

	private const float DebuffHitDelaySeconds = 0.4667f;

	private const float U1BuffHitDelaySeconds = 0.4667f;

	private const float U3BuffHitDelaySeconds = 0.8827f;

	private const float U4BuffHitDelaySeconds = 0.6827f;

	private const float U2AttackReadySeconds = 0.4f;

	private const float U2AttackPlayHitDelaySeconds = 0.066f;

	private const float U2AttackHitDelaySeconds = 0.46600002f;

	private static readonly CharacterSkinProfile SkinProfile;

	private static readonly CharacterSkinSettingsStore SkinSettingsStore;

	private static readonly SkinCharacterSettings SkinRuntimeSettings;

	private static readonly CharacterSkinPresentationGates PresentationGates;

	private static readonly CombatPresentationAudiencePolicy CombatAudience;

	private static readonly SkinDiagnosticsRuntime Diagnostics;

	private static readonly CharacterSkinVoicePlayerRuntime VoicePlayer;

	private static readonly CharacterSkinSfxPlayerRuntime SfxPlayer;

	private static readonly CharacterGrowthVoiceRuntime GrowthVoice;

	private static readonly CharacterRestSiteHealVoiceRuntime RestSiteHealVoice;

	private static readonly CharacterContextCutInRuntime ContextCutIns;

	private static readonly CharacterContextVoiceRuntime ContextVoice;

	private static readonly CombatDamageVoiceRuntime CombatDamageVoice;

	private static readonly CombatDeathSequenceRuntime CombatDeathSequence;

	private static readonly CharacterSelectPresentationRuntime CharacterSelectRuntime;

	private static readonly List<IDisposable> TransportEventSubscriptions;

	private static readonly RestSitePresentationProfile RestSiteProfile;

	private static readonly CombatCreatureNodeResolver CombatCreatureNodes;

	private static readonly CombatCreatureSpineResolver CombatSpines;

	private static readonly CombatCreatureVisualAnchorResolver CombatVisualAnchors;

	private static readonly CombatEntryBodyVisibilityRuntime CombatEntryBodyVisibility;

	private static readonly CombatBodySurfaceCoordinator CombatBodySurface;

	private static readonly RestSitePresentationRuntime RestSitePresentation;

	private static readonly MerchantPresentationRuntime MerchantPresentation;

	private static readonly CombatCardContextBuilder CombatCardContexts;

	private static readonly NativePlayedCardVisibilityRuntime<CardModel> NativePlayedCardVisibility;

	private static readonly CombatQueuedCardPresentationRuntime<CardModel> QueuedCardPresentation;

	private static readonly Lazy<BattleReadyOverlayRuntime<CardModel>> BattleReadyOverlayLazy;

	private static readonly TimedDuplicateSuppressor DefeatOverlayGate;

	private static readonly CombatEffectMaterialRuntime EffectMaterials;

	private static CharacterDefeatLifecycle? DefeatLifecycle;

	private const string CharacterIdentityIconPath = "res://MeirinWatcherSkin/images/ui/character_icon_meirin.png";

	private const string CharacterIdentityOutlineIconPath = "res://MeirinWatcherSkin/images/ui/character_icon_meirin_outline.png";

	private static readonly FieldInfo? MultiHitBeforeDamageField;

	private static readonly ConditionalWeakTable<Player, MultiHitState> MultiHitStates;

	private static readonly Lazy<MultiHitCueCache> MultiHitCues;

	private static readonly IReadOnlyList<string> MultiHitStrongAnimations;

	private static readonly IReadOnlyList<string> MultiHitU2Animations;

	private const string MeirinRestSiteScenePath = "res://MeirinWatcherSkin/scenes/rest_site/meirin_rest_site.tscn";

	private static readonly Lazy<SkinSettingsGlobalUiRuntime> SettingsGlobalRuntimeLazy;

	private static readonly Lazy<SkinSettingsUiRuntime> SettingsUiRuntimeLazy;

	private static long _nextRestSiteVoiceTick;

	private static readonly PresentationEventScope TransportScope;

	private const string BoninallHarmonyId = "boninall.watcher";

	private static WatcherBodyVfxFollowScope? ActiveWatcherBodyVfxFollowScope;

	private static readonly HashSet<string> BoninallPresentationPatchTypes;

	private static BundledSkinCardArt BundledCardArt => BundledCardArtHolder.Instance;

	private static CombatBodyReadyPresentationRuntime<Player, CardModel> MeirinBodyReady => MeirinBodyReadyLazy.Value;

	private static CombatReadyIdleVoiceRuntime<Player> MeirinReadyIdleVoices => MeirinReadyIdleVoicesLazy.Value;

	private static RemoteNativeBodyTriggerRuntime RemoteNativeBodyTriggers => RemoteNativeBodyTriggersLazy.Value;

	private static CharacterEffectEngine CombatEffectEngine => CombatEffectEngineLazy.Value;

	private static CombatTransientEffectPoolRuntime CombatEffects => CombatEffectEngine.Pool;

	private static CombatTransientEffectCueRuntime<NCreature> CombatEffectCues => CombatEffectCuesLazy.Value;

	private static CombatTransientEffectCueRouter<NCreature, NCreature> CombatEffectRoutes => CombatEffectRoutesLazy.Value;

	private static CombatSfxCueRuntime<NCreature> CombatSfxCues => CombatSfxCuesLazy.Value;

	private static CombatBodySequenceRuntime CombatEntryBodySequences => CombatEntryBodySequencesLazy.Value;

	private static string MeirinMeleeLayerLiftOwnerKey => SkinProfile.RuntimeMetaKey("melee_approach_layer_lift");

	internal static bool IsSkinDebugEnabled => SkinSettingsStore.DebugLogEnabled;

	internal static object RegisteredProfile => SkinProfile;

	private static BattleReadyOverlayRuntime<CardModel> BattleReadyOverlay => BattleReadyOverlayLazy.Value;

	private static SkinSettingsGlobalUiRuntime SettingsGlobalRuntime => SettingsGlobalRuntimeLazy.Value;

	private static SkinSettingsUiRuntime SettingsUiRuntime => SettingsUiRuntimeLazy.Value;

	private static bool TryBeginMeirinAttackTimingPresentation(AttackCommand command, CardModel card, NCreature actor, string triggerName, string cueName, int hitCount, int token, out bool triggeredComboContinuation)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		triggeredComboContinuation = false;
		if (SkinRuntimeSettings.OfficialAttackTimingEnabled)
		{
			Creature attacker = command.Attacker;
			Player val = ((attacker != null) ? attacker.Player : null);
			CharacterSkinRemoteNativeBodyAnimation mapping = default(CharacterSkinRemoteNativeBodyAnimation);
			if (val != null && val == card.Owner && SkinProfile.CombatBody.TryGetRemoteNativeBodyAnimation(triggerName, ref mapping) && TryResolveMeirinAttackTimingSequence(triggerName, cueName, mapping, out IReadOnlyList<string> sequence, out float hitMarkerSeconds) && CombatBodySurface.Ensure(actor, "attack_timing_" + cueName) && ValidateAnimations(actor, sequence))
			{
				CombatTriggeredCardComboStep val2 = default(CombatTriggeredCardComboStep);
				bool flag = hitCount == 1 && (string.Equals(cueName, "attack_play1", StringComparison.Ordinal) || string.Equals(cueName, "attack_play2", StringComparison.Ordinal)) && CombatTriggeredCardComboRuntime.TryEnter(card, ref val2);
				if (flag && ((CombatTriggeredCardComboStep)(ref val2)).IsContinuation)
				{
					sequence = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { cueName, "attack_end" });
					hitMarkerSeconds = 0.066f;
					triggeredComboContinuation = true;
				}
				ulong ticksMsec = Time.GetTicksMsec();
				PlayAnimationSequence(actor, sequence, mapping);
				if (flag)
				{
					CombatTriggeredCardComboRuntime.ReserveNextBeat(card, ticksMsec, (triggeredComboContinuation ? 0f : 0.2f) + 0.2f, cueName);
				}
				MeirinAttackTimings.GetOrCreateValue(val).Begin(command, triggerName, cueName, Math.Max(1, hitCount), token, ticksMsec, hitMarkerSeconds);
				SkinLog.Debug(() => "Started Meirin attack timing route=" + cueName + " marker=" + hitMarkerSeconds.ToString("0.###") + " hits=" + Math.Max(1, hitCount));
				return true;
			}
		}
		return false;
	}

	private static bool TryResolveMeirinAttackTimingSequence(string triggerName, string cueName, CharacterSkinRemoteNativeBodyAnimation mapping, out IReadOnlyList<string> sequence, out float hitMarkerSeconds)
	{
		sequence = Array.Empty<string>();
		hitMarkerSeconds = 0f;
		if (string.Equals(triggerName, "Attack", StringComparison.OrdinalIgnoreCase))
		{
			if (string.Equals(cueName, "u2_attack_play", StringComparison.Ordinal))
			{
				sequence = new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "u2_attack_ready", "u2_attack_play", "u2_attack_end" });
				hitMarkerSeconds = 0.46600002f;
				return true;
			}
			if (string.Equals(cueName, "attack_play1", StringComparison.Ordinal) || string.Equals(cueName, "attack_play2", StringComparison.Ordinal))
			{
				sequence = new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "attack_ready", cueName, "attack_end" });
				hitMarkerSeconds = 0.266f;
				return true;
			}
		}
		else if (string.Equals(triggerName, "heavyAttack", StringComparison.OrdinalIgnoreCase) && string.Equals(cueName, "f_strong_attack_play", StringComparison.Ordinal))
		{
			IReadOnlyList<string> animationSequence = ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationSequence;
			if (animationSequence != null && animationSequence.Count > 0)
			{
				sequence = ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationSequence;
				hitMarkerSeconds = 0.299f;
				return true;
			}
		}
		return false;
	}

	private static bool TryHandleMeirinAttackTimingTrigger(Creature creature, Player player, string triggerName, float nativeWaitTime, out Task handledTask)
	{
		handledTask = Task.CompletedTask;
		if (!SkinRuntimeSettings.OfficialAttackTimingEnabled || creature.Player != player)
		{
			return false;
		}
		if (TryHandlePendingMeirinUgAttackTrigger(creature, player, triggerName, nativeWaitTime, out handledTask))
		{
			return true;
		}
		if (!MeirinAttackTimings.TryGetValue(player, out MeirinAttackTimingState state) || !state.IsActive || !string.Equals(triggerName, state.TriggerName, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (!state.HasPlayedVoice)
		{
			state.HasPlayedVoice = true;
			TryPlayMeirinAttackTimingVoice(player, state.CueName);
		}
		ConsumeMeirinAttackTimingReservation(player, state);
		int triggerIndex = state.NextNativeAttackTriggerIndex();
		float targetElapsedSeconds = state.HitMarkerSeconds;
		float remainingSeconds = 0f;
		if (!state.TryResolveRemainingHitDelaySeconds(triggerIndex, Time.GetTicksMsec(), out targetElapsedSeconds, out remainingSeconds))
		{
			remainingSeconds = 0f;
			SkinLog.Warn("Meirin attack timing received an extra native trigger route=" + state.CueName + " index=" + triggerIndex + " hits=" + state.HitCount);
		}
		handledTask = ((remainingSeconds <= 0.001f) ? Task.CompletedTask : Cmd.CustomScaledWait(remainingSeconds, remainingSeconds, false, default(CancellationToken)));
		SkinLog.Debug(() => "Meirin attack damage release route=" + state.CueName + " trigger=" + triggerIndex + " targetElapsed=" + targetElapsedSeconds.ToString("0.###") + " nativeWait=" + nativeWaitTime.ToString("0.###") + " remaining=" + remainingSeconds.ToString("0.###"));
		return true;
	}

	private static void TryPlayMeirinAttackTimingVoice(Player player, string cueName)
	{
		if (SkinProfile.Features.CombatVoiceHooks)
		{
			IReadOnlyList<string> readOnlyList = ((cueName == "ug_attack") ? MeirinCombatPresentationAssets.UgAttackVoiceCandidates : ((!(cueName == "u2_attack_play")) ? MeirinCombatPresentationAssets.AttackVoiceCandidates : MeirinCombatPresentationAssets.U2AttackVoiceCandidates));
			IReadOnlyList<string> readOnlyList2 = readOnlyList;
			VoicePlayer.PlayRandomFor(player, readOnlyList2, 1f);
		}
	}

	private static void ConsumeMeirinAttackTimingReservation(Player player, MeirinAttackTimingState state)
	{
		if (!state.HasConsumedRouteReservation)
		{
			state.HasConsumedRouteReservation = true;
			if (string.Equals(state.CueName, "u2_attack_play", StringComparison.Ordinal) || string.Equals(state.CueName, "ug_attack", StringComparison.Ordinal))
			{
				TryConsumeAttackRouteOverride(player, out string _);
			}
			else if (string.Equals(state.CueName, "attack_play1", StringComparison.Ordinal))
			{
				NormalAttackVariants.GetOrCreateValue(player).ExpireIfPending(1);
			}
			else if (string.Equals(state.CueName, "attack_play2", StringComparison.Ordinal))
			{
				NormalAttackVariants.GetOrCreateValue(player).ExpireIfPending(2);
			}
		}
	}

	private static bool IsLocalTargetCard(CardModel? card)
	{
		if (((card != null) ? card.Owner : null) != null && IsTargetPlayer(card.Owner))
		{
			return IsLocalPlayer(card.Owner);
		}
		return false;
	}

	private static Node? GetBattleReadyScreen()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			return null;
		}
		return BattleReadyOverlayScreen.Resolve((Node)(object)instance.CombatVfxContainer);
	}

	private static bool ShouldShowBattleReadyForCard(CardModel? card)
	{
		return BattleReadyOverlay.CanShow(card);
	}

	private static bool HasCombatCutInPresentation(CardModel card)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		CombatPresentationCardContext val = CombatCardContexts.BuildPreview(card);
		MeirinCardCapabilities capabilities = ResolveCardCapabilities(card);
		if ((int)val.Route == 1)
		{
			if (capabilities.UsesRandomOpponentTargeting)
			{
				CombatPresentationCardContext obj = val._003CClone_003E_0024();
				obj.set_TargetScope((CombatPresentationTargetScope)5);
				val = obj;
			}
			return MeirinCombatRouteRules.ResolveAttackRoute(val) == MeirinCombatRoute.UgAttack;
		}
		return MeirinCombatRouteRules.ResolveSupportRoute(val, card.GainsBlock, capabilities, ResolveMeirinCardOrigin(card), allowUx: true) == MeirinCombatRoute.UxBuff;
	}

	private static void ShowCardHoverPresentation(CardModel? card)
	{
		if (ShouldTriggerCardSelectionPresentationOnHover())
		{
			ShowCardReadyPresentation(card);
		}
		if (ShouldShowBattleReadyForCard(card))
		{
			BattleReadyOverlay.ShowIdleForCard(card);
		}
	}

	private static void EndCardHoverPresentation(CardModel? card)
	{
		if (IsLocalTargetCard(card))
		{
			BattleReadyOverlay.HideForCard(card);
			if (ShouldTriggerCardSelectionPresentationOnHover())
			{
				MeirinBodyReady.HideForCard(card);
			}
		}
	}

	private static void CancelCardHoverPresentation(CardModel? card)
	{
		if (IsLocalTargetCard(card))
		{
			MeirinBodyReady.CancelForCard(card);
			BattleReadyOverlay.CancelForCard(card);
		}
	}

	private static void HideCardHoverPresentationForCinematic(CardModel? card)
	{
		if (IsLocalTargetCard(card))
		{
			BattleReadyOverlay.Cleanup(false);
		}
	}

	private static void PlayBattleReadyForCommittedCard(CardModel? card)
	{
		if (card != null && ShouldShowBattleReadyForCard(card))
		{
			BattleReadyOverlay.PlayForCard(card);
		}
	}

	private static Task TryBeginMeirinSupportCardPresentation(CardModel card, Creature? target)
	{
		CardModel obj = card;
		Player player = ((obj != null) ? obj.Owner : null);
		if (player == null || !IsPresentationTargetCard(card))
		{
			return Task.CompletedTask;
		}
		Task presentationTask = Task.CompletedTask;
		SkinPatchGuard.Run("Meirin card enqueue presentation routing", (Action)delegate
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Invalid comparison between Unknown and I4
			SkinLog.InfoOnce("meirin_card_enqueue_presentation_routing", "Meirin owns supported card body actions while retaining native gameplay VFX.");
			if ((int)CombatCardContexts.GetRoute(card) != 1)
			{
				NCreature val = CombatCreatureNodes.ResolvePlayer(NCombatRoom.Instance, player, "Meirin support-card body lookup failed", (Action<string>)delegate(string message)
				{
					SkinLog.Warn(message);
				});
				if (val != null)
				{
					RestoreMeirinMeleeMotionForActor(val);
					if (CombatBodySurface.Ensure(val, "support_card"))
					{
						CombatPresentationCardContext context = CombatCardContexts.Build(card, 1, 0f, 0m);
						var (sequence, readOnlyList, text) = ResolveSupportCardPresentation(card, context, CombatSupportFullScreenPresentationPolicy.Allows(CombatAudience, card));
						if (text == "ux_buff")
						{
							if (TryBeginMeirinUxPresentation(card, val, out Task releaseTask))
							{
								presentationTask = releaseTask;
								return;
							}
							(sequence, readOnlyList, text) = ResolveSupportCardPresentation(card, context, allowUx: false);
						}
						BindMeirinBodyRecovery(val);
						if (TryPlayCombatBodySequence(val, sequence, SkinProfile.CombatBody.ReadyLoopAnimation, "Meirin " + text))
						{
							BeginMeirinBodyReadyAction(card);
							int token = CombatEffectTokens.GetOrCreateValue(val).Next();
							RegisterSupportNativeTrigger(player, text);
							SkinLog.InfoOnce("meirin_support_route_" + text, "Accepted Meirin support presentation route=" + text + " card=" + ((object)card).GetType().Name + " local=" + IsLocalPlayer(player));
							if (readOnlyList.Count > 0)
							{
								VoicePlayer.PlayRandomFor(player, readOnlyList, 1f);
							}
							QueueSupportCardEffects(target, val, text, token);
						}
					}
				}
			}
		}, 2);
		return presentationTask;
	}

	private static void QueueSupportCardEffects(Creature? targetModel, NCreature actor, string route, int token)
	{
		Creature entity = actor.Entity;
		Player val = ((entity != null) ? entity.Player : null);
		bool flag;
		switch (route)
		{
		case "defense":
			if (PresentationGates.CombatPresentationSfxEnabled && CombatAudience.Allows(val, (CombatPresentationChannel)2))
			{
				PlayCombatSfxCues(actor, "defense_ready", token);
			}
			return;
		case "u1_buff":
		case "u3_buff":
		case "u4_buff":
		case "debuff":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			return;
		}
		string text = route + "_sequence";
		if (PresentationGates.CombatPresentationSfxEnabled && CombatAudience.Allows(val, (CombatPresentationChannel)2))
		{
			PlayCombatSfxCues(actor, text, token);
		}
		if (PresentationGates.CombatPresentationEffectsEnabled && CombatAudience.Allows(val, (CombatPresentationChannel)1))
		{
			NCreature val2 = CombatCreatureNodes.Resolve(NCombatRoom.Instance, targetModel, "Meirin support-card effect target lookup failed", (Action<string>)delegate(string message)
			{
				SkinLog.Warn(message);
			});
			object obj;
			if (val2 != null)
			{
				IReadOnlyList<NCreature> readOnlyList = new _003C_003Ez__ReadOnlySingleElementList<NCreature>(val2);
				obj = readOnlyList;
			}
			else
			{
				obj = null;
			}
			IReadOnlyList<NCreature> readOnlyList2 = (IReadOnlyList<NCreature>)obj;
			CombatEffectRoutes.PlayForTargets(actor, readOnlyList2, text, token);
		}
	}

	private static void RegisterSupportNativeTrigger(Player player, string route)
	{
		SupportNativeTriggerGates.GetOrCreateValue(player).Register(route, Time.GetTicksMsec());
	}

	private static bool TryConsumeSupportNativeTrigger(Player player, string triggerName, out string route)
	{
		route = string.Empty;
		if (!string.Equals(triggerName, "Cast", StringComparison.OrdinalIgnoreCase) && !string.Equals(triggerName, "PowerUp", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return SupportNativeTriggerGates.GetOrCreateValue(player).TryConsume(Time.GetTicksMsec(), out route);
	}

	private static (IReadOnlyList<string> Sequence, IReadOnlyList<string> Voices, string Route) ResolveSupportCardPresentation(CardModel card, CombatPresentationCardContext context, bool allowUx)
	{
		return MeirinCombatRouteRules.ResolveSupportRoute(context, card.GainsBlock, ResolveCardCapabilities(card), ResolveMeirinCardOrigin(card), allowUx) switch
		{
			MeirinCombatRoute.Defense => (Sequence: new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "defense_ready", "defense_play" }), Voices: MeirinCombatPresentationAssets.DefenseVoiceCandidates, Route: "defense"), 
			MeirinCombatRoute.Debuff => (Sequence: new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "debuff_ready", "debuff_play" }), Voices: Array.Empty<string>(), Route: "debuff"), 
			MeirinCombatRoute.U3Buff => (Sequence: new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "u3_buff_ready", "u3_buff_play" }), Voices: MeirinCombatPresentationAssets.U3BuffVoiceCandidates, Route: "u3_buff"), 
			MeirinCombatRoute.U4Buff => (Sequence: new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "u4_buff_ready", "u4_buff_play" }), Voices: MeirinCombatPresentationAssets.U4BuffVoiceCandidates, Route: "u4_buff"), 
			MeirinCombatRoute.UxBuff => (Sequence: new _003C_003Ez__ReadOnlySingleElementList<string>("ux_buff"), Voices: MeirinCombatPresentationAssets.UxBuffVoiceCandidates, Route: "ux_buff"), 
			_ => (Sequence: new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "u1_buff_ready", "u1_buff_play" }), Voices: MeirinCombatPresentationAssets.U1BuffVoiceCandidates, Route: "u1_buff"), 
		};
	}

	private static MeirinCardOrigin ResolveMeirinCardOrigin(CardModel card)
	{
		if (!(card is WatcherCard))
		{
			return MeirinCardOrigin.NonWatcher;
		}
		return MeirinCardOrigin.Watcher;
	}

	private static MeirinCardCapabilities ResolveCardCapabilities(CardModel card)
	{
		Type cardType = ((object)card).GetType();
		lock (CardCapabilityCache)
		{
			if (CardCapabilityCache.TryGetValue(cardType, out var value))
			{
				return value;
			}
		}
		MeirinCardCapabilities capabilities = MeirinCardCapabilities.None;
		try
		{
			capabilities = ScanCardCapabilities(cardType);
		}
		catch (Exception ex)
		{
			SkinLog.WarnException("Meirin card capability scan failed: ", ex);
		}
		lock (CardCapabilityCache)
		{
			CardCapabilityCache[cardType] = capabilities;
		}
		SkinLog.Debug(() => "Meirin card capabilities " + cardType.Name + " stance=" + capabilities.ChangesStance + " divinity=" + capabilities.EntersDivinity + " mantra=" + capabilities.MantraRelated + " power=" + capabilities.AppliesPower + " randomOpponents=" + capabilities.UsesRandomOpponentTargeting);
		return capabilities;
	}

	private static MeirinCardCapabilities ScanCardCapabilities(Type cardType)
	{
		bool changesStance = false;
		bool entersDivinity = false;
		bool flag = false;
		bool appliesPower = false;
		bool usesRandomOpponentTargeting = false;
		foreach (MethodBase item in EnumerateScannableMethods(cardType))
		{
			foreach (MethodBase item2 in EnumerateCalledMethods(item))
			{
				string name = item2.Name;
				string text = item2.DeclaringType?.Name;
				if (string.Equals(text, "WatcherCombatHelper", StringComparison.Ordinal))
				{
					if (string.Equals(name, "EnterDivinity", StringComparison.Ordinal))
					{
						changesStance = true;
						entersDivinity = true;
					}
					else if (Array.IndexOf(StanceEntryMethodNames, name) >= 0)
					{
						changesStance = true;
					}
					else if (name.Contains("Mantra", StringComparison.Ordinal))
					{
						flag = true;
					}
				}
				if (string.Equals(name, "Apply", StringComparison.Ordinal) && text != null && Array.IndexOf(PowerApplyDeclaringTypeNames, text) >= 0)
				{
					appliesPower = true;
				}
				if (IsRandomOpponentTargetingCall(text, name))
				{
					usesRandomOpponentTargeting = true;
				}
				if (!flag && ReferencesMantraTypeArgument(item2))
				{
					flag = true;
				}
			}
		}
		return new MeirinCardCapabilities(changesStance, entersDivinity, flag, appliesPower, usesRandomOpponentTargeting);
	}

	private static bool IsRandomOpponentTargetingCall(string? declaringTypeName, string methodName)
	{
		if (!string.Equals(declaringTypeName, "AttackCommand", StringComparison.Ordinal) || !string.Equals(methodName, "TargetingRandomOpponents", StringComparison.Ordinal))
		{
			if (string.Equals(declaringTypeName, "WatcherAttackCommandCompat", StringComparison.Ordinal))
			{
				return string.Equals(methodName, "TargetingRandomOpponentsCompat", StringComparison.Ordinal);
			}
			return false;
		}
		return true;
	}

	private static bool ReferencesMantraTypeArgument(MethodBase method)
	{
		if (!method.IsGenericMethod)
		{
			return false;
		}
		Type[] genericArguments = method.GetGenericArguments();
		for (int i = 0; i < genericArguments.Length; i++)
		{
			if (string.Equals(genericArguments[i].Name, "Mantra", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CEnumerateScannableMethods_003Ed__46))]
	private static IEnumerable<MethodBase> EnumerateScannableMethods(Type cardType)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateScannableMethods_003Ed__46(-2)
		{
			_003C_003E3__cardType = cardType
		};
	}

	[IteratorStateMachine(typeof(_003CEnumerateCalledMethods_003Ed__47))]
	private static IEnumerable<MethodBase> EnumerateCalledMethods(MethodBase method)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateCalledMethods_003Ed__47(-2)
		{
			_003C_003E3__method = method
		};
	}

	private static bool TryReadIlOpCode(byte[] il, ref int offset, out OpCode opCode)
	{
		short num = il[offset++];
		if (num == 254)
		{
			if (offset >= il.Length)
			{
				opCode = default(OpCode);
				return false;
			}
			num = (short)(0xFE00 | il[offset++]);
		}
		return IlOpCodesByValue.TryGetValue(num, out opCode);
	}

	private static bool TryAdvanceIlOperand(byte[] il, OpCode opCode, ref int offset)
	{
		int num;
		switch (opCode.OperandType)
		{
		case OperandType.InlineNone:
			num = 0;
			break;
		case OperandType.ShortInlineBrTarget:
		case OperandType.ShortInlineI:
		case OperandType.ShortInlineVar:
			num = 1;
			break;
		case OperandType.InlineVar:
			num = 2;
			break;
		case OperandType.InlineI8:
		case OperandType.InlineR:
			num = 8;
			break;
		case OperandType.InlineSwitch:
			num = ResolveIlSwitchOperandSize(il, offset);
			break;
		default:
			num = 4;
			break;
		}
		int num2 = num;
		if (num2 < 0 || offset + num2 > il.Length)
		{
			return false;
		}
		offset += num2;
		return true;
	}

	private static int ResolveIlSwitchOperandSize(byte[] il, int offset)
	{
		if (offset + 4 > il.Length)
		{
			return -1;
		}
		int num = BitConverter.ToInt32(il, offset);
		if (num >= 0)
		{
			return 4 + num * 4;
		}
		return -1;
	}

	private static Dictionary<short, OpCode> BuildIlOpCodeTable()
	{
		Dictionary<short, OpCode> dictionary = new Dictionary<short, OpCode>();
		FieldInfo[] fields = typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public);
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].GetValue(null) is OpCode value)
			{
				dictionary[value.Value] = value;
			}
		}
		return dictionary;
	}

	private static bool TryPrepareCharacterSelectSpine(Node spineNode, string source)
	{
		string text = StringName.op_Implicit(spineNode.Name);
		if (!CharacterSelectPmaSpineNodeNames.Contains(text))
		{
			return false;
		}
		Material characterSelectPmaMaterial = GetCharacterSelectPmaMaterial();
		if (characterSelectPmaMaterial == null)
		{
			SkinLog.Warn("Character select PMA material missing: node=" + text + " source=" + source);
			return false;
		}
		CombatEffectRuntime.ApplyCanvasAndSpineMaterials(spineNode, characterSelectPmaMaterial);
		return true;
	}

	private static Material? GetCharacterSelectPmaMaterial()
	{
		return _characterSelectPmaMaterial ?? (_characterSelectPmaMaterial = ResourceLoader.Load<Material>(SkinProfile.PremultipliedAlphaMaterialPath, (string)null, (CacheMode)1));
	}

	private static void ApplyCharacterSelectIcon(CharacterModel character, string path, ref CompressedTexture2D result)
	{
		if (SkinProfile.AppliesTo(character))
		{
			CompressedTexture2D val = ResourceLoader.Load<CompressedTexture2D>(path, (string)null, (CacheMode)1);
			if (val != null)
			{
				result = val;
				SkinLog.InfoOnce("meirin_character_select_icon_" + path, "Applied Meirin character-select icon: " + path);
			}
			else
			{
				SkinLog.InfoOnce("meirin_character_select_icon_missing_" + path, "Meirin character-select icon could not be loaded: " + path);
			}
		}
	}

	private static bool ShouldTriggerCardSelectionPresentationOnHover()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		return (int)SkinRuntimeSettings.CardSelectionPresentationTrigger == 0;
	}

	private static bool ShouldTriggerCardSelectionPresentationOnSelection()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		return (int)SkinRuntimeSettings.CardSelectionPresentationTrigger == 1;
	}

	private static void ShowCardReadyPresentation(CardModel? card)
	{
		if (IsLocalTargetCard(card) && card != null && MeirinBodyReady.ShowForCard(card))
		{
			MeirinReadyIdleVoices.TryPlayForReadyTransition(card.Owner);
		}
	}

	private static void ShowCardSelectionPresentation(CardModel? card)
	{
		if (ShouldTriggerCardSelectionPresentationOnSelection())
		{
			ShowCardReadyPresentation(card);
		}
	}

	private static void HoldMeirinBodyReadyForCardPlay(CardModel? card)
	{
		if (IsLocalTargetCard(card))
		{
			MeirinBodyReady.HoldForCardPlay(card);
		}
	}

	private static int BeginMeirinBodyReadyAction(CardModel? card)
	{
		if (!IsPresentationTargetCard(card) || ((card != null) ? card.Owner : null) == null)
		{
			return 0;
		}
		int result = MeirinBodyReadyActionTokens.GetOrCreateValue(card.Owner).Next();
		MeirinBodyReady.BeginAction(card);
		return result;
	}

	private static void CompleteMeirinBodyReadyAction(Player? player)
	{
		if (player != null)
		{
			MeirinBodyReadyActionTokens.GetOrCreateValue(player).Next();
			MeirinBodyReady.CompleteAction(player);
		}
	}

	private static void BindMeirinBodyRecovery(NCreature actor)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		Player player = actor.Entity.Player;
		if (player == null)
		{
			return;
		}
		Node2D val = CombatSpines.Find(actor);
		if (val != null)
		{
			NativeCombatBodyRecovery.Bind(new MegaSprite(Variant.op_Implicit((GodotObject)(object)val)), actor.Entity, (Func<bool>)(() => !MeirinBodyReady.IsReady(player) && (_meirinCinematicPhase == null || _meirinCinematicPhase.Player != player || _meirinCinematicPhase.HasReleasedBattlefieldActors)), (Func<float>)(() => (!TryPlayCombatBodySequence(actor, new _003C_003Ez__ReadOnlySingleElementList<string>(SkinProfile.CombatBody.ReadyExitAnimation), SkinProfile.CombatBody.IdleAnimation, "Meirin action restore")) ? 0f : CombatEntryBodySequences.GetAnimationDelaySeconds(SkinProfile.CombatBody.ReadyExitAnimation)), (Action)delegate
			{
				CompleteMeirinBodyReadyAction(player);
			}, (Action<string>)delegate(string message)
			{
				SkinLog.Warn(message);
			});
		}
	}

	private static void ClearMeirinBodyReady(Player? player)
	{
		if (player != null && MeirinBodyReadyLazy.IsValueCreated)
		{
			MeirinBodyReadyActionTokens.GetOrCreateValue(player).Next();
			RemoteNativeBodyTriggers.InvalidatePlayerPresentation(player);
		}
	}

	private static void SetCardPlayPresentationEnabledForRuntime(bool enabled, bool persist)
	{
		SkinRuntimeSettings.SetShowCardPlayPresentation(enabled, persist);
	}

	private static void ApplyCardPlayPresentationEnabledForRuntime(bool enabled)
	{
		if (!enabled)
		{
			NativePlayedCardVisibility.RestorePlayContainer();
			CancelMeirinCinematicPhase(null, restoreBody: true);
		}
	}

	private static bool CanShowMeirinBodyReady(Player player)
	{
		if (!PresentationGates.CombatBodyPresentationEnabled || player.Creature.IsDead || !CombatAudience.Allows(player, (CombatPresentationChannel)0) || (_meirinCinematicPhase != null && _meirinCinematicPhase.Player == player) || HasActiveMeirinMultiHitPresentation(player))
		{
			return false;
		}
		if (MeirinAttackTimings.TryGetValue(player, out MeirinAttackTimingState value))
		{
			return !value.IsActive;
		}
		return true;
	}

	private static bool CanPlayMeirinReadyIdleVoice(Player player)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Invalid comparison between Unknown and I4
		if (SkinProfile.Features.CombatVoiceHooks && PresentationGates.CombatBodyPresentationEnabled && IsTargetPlayer(player) && IsLocalPlayer(player))
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			if (playerCombatState == null)
			{
				return false;
			}
			return (int)playerCombatState.Phase == 3;
		}
		return false;
	}

	private static void PlayMeirinBodyReadyState(Player? player, IReadOnlyList<CombatPresentationBodyStep> steps, string? loopAnimation, int token)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (player == null || steps.Count == 0 || !IsMeirinBodyReadyPlaybackTokenCurrent(player, token))
		{
			return;
		}
		NCreature val = CombatCreatureNodes.ResolvePlayer(NCombatRoom.Instance, player, "Meirin card-ready body lookup failed", (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		});
		if (val == null || !CombatBodySurface.Ensure(val, "card_ready"))
		{
			return;
		}
		foreach (CombatPresentationBodyStep step in steps)
		{
			if (!CombatBodySurface.HasAnimation(val, step.BodyAnimation, "Meirin card-ready"))
			{
				return;
			}
		}
		if (string.IsNullOrWhiteSpace(loopAnimation) || CombatBodySurface.HasAnimation(val, loopAnimation, "Meirin card-ready loop"))
		{
			SpineAnimationCompat.SetAnimation(val.SpineAnimation, steps[0].BodyAnimation, false, 0);
			for (int i = 1; i < steps.Count; i++)
			{
				SpineAnimationCompat.AddAnimation(val.SpineAnimation, steps[i].BodyAnimation, 0f, false, 0);
			}
			if (!string.IsNullOrWhiteSpace(loopAnimation))
			{
				SpineAnimationCompat.AddAnimation(val.SpineAnimation, loopAnimation, 0f, true, 0);
			}
		}
	}

	private static bool IsMeirinBodyReadyPlaybackTokenCurrent(Player player, int token)
	{
		if (MeirinBodyReadyPlaybackTokens.TryGetValue(player, out MeirinPresentationTokenState value))
		{
			return value.IsCurrent(token);
		}
		return false;
	}

	private static bool TryBeginMeirinUgPresentation(AttackCommand command, CardModel card, NCreature ownerNode, IReadOnlyList<NCreature>? targetNodes, object? combatState, int hitCount, int token)
	{
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		Player owner = card.Owner;
		CombatAttackLethalForecastResult val = default(CombatAttackLethalForecastResult);
		if (owner != null && PresentationGates.CombatPresentationEffectsEnabled && PresentationGates.CutInPresentationRuntimeEnabled && SkinRuntimeSettings.OfficialAttackTimingEnabled && CombatAttackFullScreenPresentationPolicy.Allows(PresentationGates, CombatAudience, card, command, combatState, Math.Max(1, hitCount), CombatEffectAttackTargets, ref val, (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		}, (Action<string>)delegate(string message)
		{
			SkinLog.Debug(message);
		}) && targetNodes != null && targetNodes.Count != 0)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val2 = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val2 != null && val2.Root != null)
			{
				List<NCreature> list = ResolveMeirinCinematicTargets(targetNodes);
				List<Node2D> list2 = ResolveMeirinCinematicTargetAnchors(list);
				List<Node2D> list3 = ResolveMeirinCinematicTargetVisualAnchors(list);
				Node2D val3 = ResolveMeirinCinematicActorAnchor(ownerNode);
				CombatCinematicFormationSnapshot val4 = CombatCinematicFormation.Capture(val3, (IReadOnlyList<Node2D>)list2);
				if (!((CombatCinematicFormationSnapshot)(ref val4)).HasTargets)
				{
					return false;
				}
				bool reflectSingleTarget = default(bool);
				Vector2 val5 = ((CombatCinematicFormationSnapshot)(ref val4)).ResolveSingleTargetOnActorRight(ref reflectSingleTarget);
				CombatCinematicFormationSnapshot targetVisualFormation = CombatCinematicFormation.Capture((Node2D)null, (IReadOnlyList<Node2D>)list3);
				Vector2 val6 = ResolveMeirinCinematicTargetVisualPosition(val4, targetVisualFormation, val5);
				Node2D val7 = TryCreateMeirinCinematicTargetProxy(list2, reflectSingleTarget);
				if (list.Count == 0 || val7 == null)
				{
					return false;
				}
				if (!TryCreateMeirinCinematicOverlay(MeirinCombatRoute.UgAttack, out Node overlay, out ulong loadInstantiateMs) || overlay == null)
				{
					((Node)val7).QueueFree();
					return false;
				}
				CancelMeirinCinematicPhase(null, restoreBody: true);
				MeirinCinematicPhaseState meirinCinematicPhaseState = (_meirinCinematicPhase = new MeirinCinematicPhaseState(NextMeirinCinematicGeneration(), MeirinCombatRoute.UgAttack, card, owner, ownerNode, list, overlay, loadInstantiateMs)
				{
					AttackCommand = command,
					AttackHitCount = Math.Max(1, hitCount),
					AttackToken = token
				});
				try
				{
					BattleReadyOverlay.SuppressNextPlayForCard(card);
					HideCardHoverPresentationForCinematic(card);
					ConnectMeirinCinematicOverlay(meirinCinematicPhaseState);
					ulong num = (meirinCinematicPhaseState.PreplayStartMsec = Time.GetTicksMsec());
					((Node)val2.Root).AddChild(overlay, false, (InternalMode)0);
					meirinCinematicPhaseState.StageMountMs = Time.GetTicksMsec() - num;
					ApplyMeirinCinematicRuntimeActorAnchor(overlay, val3, val4);
					((GodotObject)overlay).Call(StringName.op_Implicit("set_target_proxy"), (Variant[])(object)new Variant[3]
					{
						Variant.op_Implicit((GodotObject)(object)val7),
						Variant.op_Implicit(val5),
						Variant.op_Implicit(val6)
					});
					Variant val8 = ((GodotObject)overlay).Call(StringName.op_Implicit("is_ready_for_playback"), Array.Empty<Variant>());
					if (!((Variant)(ref val8)).AsBool())
					{
						throw new InvalidOperationException("UG preplay barrier was not ready.");
					}
					HideMeirinCinematicActor(meirinCinematicPhaseState, ownerNode);
					foreach (NCreature item in list)
					{
						HideMeirinCinematicActor(meirinCinematicPhaseState, item);
					}
					meirinCinematicPhaseState.PreplayMs = Time.GetTicksMsec() - num;
					val8 = ((GodotObject)overlay).Call(StringName.op_Implicit("start_stage"), Array.Empty<Variant>());
					if (!((Variant)(ref val8)).AsBool())
					{
						throw new InvalidOperationException("UG stage rejected its render preplay.");
					}
					BeginMeirinBodyReadyAction(card);
					SkinLog.Info("Accepted Meirin UG presentation card=" + ((object)card).GetType().Name + " hits=" + meirinCinematicPhaseState.AttackHitCount + " targets=" + list.Count + " targetSideStaged=" + reflectSingleTarget);
					return true;
				}
				catch (Exception ex)
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)val7) && ((Node)val7).GetParent() == null)
					{
						((Node)val7).QueueFree();
					}
					SkinLog.Warn("Meirin UG overlay mount failed: " + SkinLog.FormatException(ex));
					CancelMeirinCinematicPhase(owner, restoreBody: true);
					return false;
				}
			}
		}
		return false;
	}

	private static bool TryBeginMeirinUxPresentation(CardModel card, NCreature ownerNode, out Task releaseTask)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		releaseTask = Task.CompletedTask;
		Player owner = card.Owner;
		if (owner != null && PresentationGates.CombatPresentationEffectsEnabled && PresentationGates.CutInPresentationRuntimeEnabled)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null && val.Root != null && TryCreateMeirinCinematicOverlay(MeirinCombatRoute.UxBuff, out Node overlay, out ulong loadInstantiateMs) && overlay != null)
			{
				Node2D val2 = ResolveMeirinCinematicActorAnchor(ownerNode);
				CombatCinematicFormationSnapshot formation = CombatCinematicFormation.Capture(val2, (IReadOnlyList<Node2D>)null);
				CancelMeirinCinematicPhase(null, restoreBody: true);
				MeirinCinematicPhaseState meirinCinematicPhaseState = (_meirinCinematicPhase = new MeirinCinematicPhaseState(NextMeirinCinematicGeneration(), MeirinCombatRoute.UxBuff, card, owner, ownerNode, Array.Empty<NCreature>(), overlay, loadInstantiateMs));
				try
				{
					BattleReadyOverlay.SuppressNextPlayForCard(card);
					HideCardHoverPresentationForCinematic(card);
					ConnectMeirinCinematicOverlay(meirinCinematicPhaseState);
					ulong num = (meirinCinematicPhaseState.PreplayStartMsec = Time.GetTicksMsec());
					((Node)val.Root).AddChild(overlay, false, (InternalMode)0);
					meirinCinematicPhaseState.StageMountMs = Time.GetTicksMsec() - num;
					ApplyMeirinCinematicRuntimeActorAnchor(overlay, val2, formation);
					Variant val3 = ((GodotObject)overlay).Call(StringName.op_Implicit("is_ready_for_playback"), Array.Empty<Variant>());
					if (!((Variant)(ref val3)).AsBool())
					{
						throw new InvalidOperationException("UX preplay barrier was not ready.");
					}
					HideMeirinCinematicActor(meirinCinematicPhaseState, ownerNode);
					val3 = ((GodotObject)overlay).Call(StringName.op_Implicit("start_stage"), Array.Empty<Variant>());
					if (!((Variant)(ref val3)).AsBool())
					{
						throw new InvalidOperationException("UX stage rejected its render preplay.");
					}
					BeginMeirinBodyReadyAction(card);
					releaseTask = WaitForMeirinUxReleaseAsync(meirinCinematicPhaseState);
					SkinLog.Info("Accepted Meirin UX presentation card=" + ((object)card).GetType().Name);
					return true;
				}
				catch (Exception ex)
				{
					SkinLog.Warn("Meirin UX overlay mount failed: " + SkinLog.FormatException(ex));
					CancelMeirinCinematicPhase(owner, restoreBody: true);
					return false;
				}
			}
		}
		return false;
	}

	private static bool TryCreateMeirinCinematicOverlay(MeirinCombatRoute route, out Node? overlay, out ulong loadInstantiateMs)
	{
		overlay = null;
		loadInstantiateMs = 0uL;
		string text = ((route == MeirinCombatRoute.UgAttack) ? "res://MeirinWatcherSkin/scenes/combat_ug_overlay.tscn" : "res://MeirinWatcherSkin/scenes/combat_ux_overlay.tscn");
		try
		{
			ulong ticksMsec = Time.GetTicksMsec();
			ref PackedScene reference = ref route == MeirinCombatRoute.UgAttack ? ref _meirinUgOverlayScene : ref _meirinUxOverlayScene;
			if (reference == null)
			{
				reference = ResourceLoader.Load<PackedScene>(text, (string)null, (CacheMode)1);
			}
			if (reference != null)
			{
				Node val = reference.Instantiate((GenEditState)0);
				if (val != null)
				{
					loadInstantiateMs = Time.GetTicksMsec() - ticksMsec;
					if (!((GodotObject)val).HasSignal(StringName.op_Implicit("presentation_started")) || !((GodotObject)val).HasSignal(StringName.op_Implicit("stage_started")) || !((GodotObject)val).HasSignal(StringName.op_Implicit("battlefield_actors_released")) || !((GodotObject)val).HasSignal(StringName.op_Implicit("stage_finished")) || !((GodotObject)val).HasMethod(StringName.op_Implicit("is_ready_for_playback")) || !((GodotObject)val).HasMethod(StringName.op_Implicit("start_stage")) || !((GodotObject)val).HasMethod(StringName.op_Implicit("cancel_stage")) || !((GodotObject)val).HasMethod(StringName.op_Implicit("set_runtime_actor_anchor")) || (route == MeirinCombatRoute.UgAttack && !((GodotObject)val).HasMethod(StringName.op_Implicit("set_target_proxy"))))
					{
						val.QueueFree();
						SkinLog.Warn("Meirin cinematic overlay contract is incomplete: " + text);
						return false;
					}
					overlay = val;
					return true;
				}
			}
			SkinLog.Warn("Meirin cinematic overlay missing/invalid: " + text);
			return false;
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Meirin cinematic overlay creation failed: " + SkinLog.FormatException(ex));
			return false;
		}
	}

	private static void ConnectMeirinCinematicOverlay(MeirinCinematicPhaseState phase)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		phase.Overlay.Name = StringName.op_Implicit(SkinProfile.RuntimeNodeName((phase.Route == MeirinCombatRoute.UgAttack) ? "UgOverlay" : "UxOverlay"));
		((GodotObject)phase.Overlay).Connect(StringName.op_Implicit("presentation_started"), Callable.From((Action)delegate
		{
			PlayMeirinCinematicAudio(phase);
		}), 4u);
		((GodotObject)phase.Overlay).Connect(StringName.op_Implicit("stage_started"), Callable.From((Action)delegate
		{
			OnMeirinCinematicStageStarted(phase);
		}), 4u);
		((GodotObject)phase.Overlay).Connect(StringName.op_Implicit("battlefield_actors_released"), Callable.From((Action)delegate
		{
			OnMeirinCinematicBattlefieldActorsReleased(phase);
		}), 4u);
		((GodotObject)phase.Overlay).Connect(StringName.op_Implicit("stage_finished"), Callable.From((Action)delegate
		{
			FinishMeirinCinematicPhase(phase.Overlay, phase.Generation);
		}), 4u);
	}

	private static void OnMeirinCinematicStageStarted(MeirinCinematicPhaseState phase)
	{
		if (_meirinCinematicPhase == phase && phase.Generation == _meirinCinematicGeneration && !phase.HasStarted)
		{
			phase.HasStarted = true;
			phase.RouteStartMsec = Time.GetTicksMsec();
			if (phase.PreplayStartMsec != 0)
			{
				phase.PreplayMs = phase.RouteStartMsec - phase.PreplayStartMsec;
			}
			if (phase.Route == MeirinCombatRoute.UgAttack && phase.AttackCommand != null)
			{
				RegisterAttackRouteOverride(phase.Player, "ug_attack");
				MeirinAttackTimingState orCreateValue = MeirinAttackTimings.GetOrCreateValue(phase.Player);
				orCreateValue.Begin(phase.AttackCommand, "Attack", "ug_attack", phase.AttackHitCount, phase.AttackToken, phase.RouteStartMsec, 2.2f);
				orCreateValue.HasPlayedVoice = true;
			}
			else if (phase.Route == MeirinCombatRoute.UxBuff)
			{
				MeirinCinematicSupportTimings.GetOrCreateValue(phase.Player).Begin(phase.Card, phase.RouteStartMsec);
			}
			ScheduleMeirinCinematicFallback(phase, (phase.Route == MeirinCombatRoute.UgAttack) ? 4.0666f : 4.2667f);
			SkinLog.Info("Meirin cinematic preplay complete route=" + phase.Route.ToString() + " loadInstantiateMs=" + phase.LoadInstantiateMs + " stageMountMs=" + phase.StageMountMs + " preplayMs=" + phase.PreplayMs + " visualEpoch=" + phase.RouteStartMsec);
			phase.TryCompleteVisualEpoch(phase.RouteStartMsec);
		}
	}

	private static void OnMeirinCinematicBattlefieldActorsReleased(MeirinCinematicPhaseState phase)
	{
		if (_meirinCinematicPhase == phase && phase.Generation == _meirinCinematicGeneration && !phase.HasReleasedBattlefieldActors)
		{
			phase.HasReleasedBattlefieldActors = true;
			RestoreMeirinCinematicBody(phase, completed: true);
			RestoreMeirinCinematicActors(phase);
			SkinLog.Info("Meirin cinematic battlefield actors restored route=" + phase.Route);
		}
	}

	private static bool TryHandlePendingMeirinUgAttackTrigger(Creature creature, Player player, string triggerName, float nativeWaitTime, out Task handledTask)
	{
		handledTask = Task.CompletedTask;
		MeirinCinematicPhaseState meirinCinematicPhase = _meirinCinematicPhase;
		if (meirinCinematicPhase == null || meirinCinematicPhase.Route != MeirinCombatRoute.UgAttack || meirinCinematicPhase.HasStarted || meirinCinematicPhase.AttackCommand == null || meirinCinematicPhase.Player != player || creature.Player != player || !string.Equals(triggerName, "Attack", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		handledTask = WaitForMeirinUgPreplayTriggerAsync(meirinCinematicPhase, creature, player, triggerName, nativeWaitTime);
		return true;
	}

	private static async Task WaitForMeirinUgPreplayTriggerAsync(MeirinCinematicPhaseState phase, Creature creature, Player player, string triggerName, float nativeWaitTime)
	{
		ulong num = await phase.WaitForVisualEpochAsync();
		if (num == 0L || _meirinCinematicPhase != phase || phase.Generation != _meirinCinematicGeneration)
		{
			return;
		}
		if (TryHandleMeirinAttackTimingTrigger(creature, player, triggerName, nativeWaitTime, out Task handledTask))
		{
			await handledTask;
			return;
		}
		float num2 = CombatAttackTimingSession.ResolveRemainingDelaySeconds(num, 2.2f, Time.GetTicksMsec());
		SkinLog.Warn("Meirin UG preplay completed without its attack timing state; using epoch fallback.");
		if (num2 > 0.001f)
		{
			await Cmd.CustomScaledWait(num2, num2, false, default(CancellationToken));
		}
	}

	private static async Task WaitForMeirinUxReleaseAsync(MeirinCinematicPhaseState phase)
	{
		ulong num = await phase.WaitForVisualEpochAsync();
		if (num != 0L && _meirinCinematicPhase == phase && phase.Generation == _meirinCinematicGeneration)
		{
			float num2 = CombatAttackTimingSession.ResolveRemainingDelaySeconds(num, 2.7f, Time.GetTicksMsec());
			if (num2 > 0.001f)
			{
				await Cmd.CustomScaledWait(num2, num2, false, default(CancellationToken));
			}
		}
	}

	private static void PlayMeirinCinematicAudio(MeirinCinematicPhaseState phase)
	{
		if (!phase.HasPlayedAudio)
		{
			phase.HasPlayedAudio = true;
			if (PresentationGates.CombatPresentationSfxEnabled && CombatAudience.Allows(phase.Player, (CombatPresentationChannel)2))
			{
				SfxPlayer.Play((phase.Route == MeirinCombatRoute.UgAttack) ? "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_ug_attack.ogg" : "res://MeirinWatcherSkin/audio/sfx/se_1027_meirin_ux_buff.ogg", 1f);
			}
			if (SkinProfile.Features.CombatVoiceHooks)
			{
				VoicePlayer.PlayRandomFor(phase.Player, (phase.Route == MeirinCombatRoute.UgAttack) ? MeirinCombatPresentationAssets.UgAttackVoiceCandidates : MeirinCombatPresentationAssets.UxBuffVoiceCandidates, 1f);
			}
		}
	}

	private static List<NCreature> ResolveMeirinCinematicTargets(IReadOnlyList<NCreature> targetNodes)
	{
		List<NCreature> list = new List<NCreature>();
		foreach (NCreature targetNode in targetNodes)
		{
			if (list.Count >= 5)
			{
				break;
			}
			if (!GodotObject.IsInstanceValid((GodotObject)(object)targetNode))
			{
				continue;
			}
			Creature entity = targetNode.Entity;
			if (entity != null && entity.IsAlive)
			{
				Node2D val = CombatSpines.Find(targetNode);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					list.Add(targetNode);
				}
			}
		}
		return list;
	}

	private static Node2D? ResolveMeirinCinematicActorAnchor(NCreature ownerNode)
	{
		Node2D obj = CombatSpines.Find(ownerNode);
		if (obj == null)
		{
			obj = ownerNode.Body;
			if (obj == null)
			{
				NCreatureVisuals visuals = ownerNode.Visuals;
				if (visuals == null)
				{
					return null;
				}
				obj = ((Node)visuals).GetNodeOrNull<Node2D>(NodePath.op_Implicit("%Visuals"));
			}
		}
		return obj;
	}

	private static List<Node2D> ResolveMeirinCinematicTargetAnchors(IReadOnlyList<NCreature> targetNodes)
	{
		List<Node2D> list = new List<Node2D>(targetNodes.Count);
		foreach (NCreature targetNode in targetNodes)
		{
			Node2D val = CombatSpines.Find(targetNode);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				list.Add(val);
			}
		}
		return list;
	}

	private static List<Node2D> ResolveMeirinCinematicTargetVisualAnchors(IReadOnlyList<NCreature> targetNodes)
	{
		List<Node2D> list = new List<Node2D>(targetNodes.Count);
		foreach (NCreature targetNode in targetNodes)
		{
			Node2D val = ResolveCombatEffectTargetVisualAnchor(targetNode);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				list.Add(val);
			}
		}
		return list;
	}

	private static Vector2 ResolveMeirinCinematicTargetVisualPosition(CombatCinematicFormationSnapshot targetBodyFormation, CombatCinematicFormationSnapshot targetVisualFormation, Vector2 stagedTargetBodyPosition)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!((CombatCinematicFormationSnapshot)(ref targetVisualFormation)).HasTargets)
		{
			return stagedTargetBodyPosition;
		}
		if (((CombatCinematicFormationSnapshot)(ref targetBodyFormation)).TargetCount == 1 && ((CombatCinematicFormationSnapshot)(ref targetVisualFormation)).TargetCount == 1)
		{
			return ((CombatCinematicFormationSnapshot)(ref targetVisualFormation)).TargetViewportCenter + stagedTargetBodyPosition - ((CombatCinematicFormationSnapshot)(ref targetBodyFormation)).TargetViewportCenter;
		}
		return ((CombatCinematicFormationSnapshot)(ref targetVisualFormation)).TargetViewportCenter;
	}

	private static void ApplyMeirinCinematicRuntimeActorAnchor(Node overlay, Node2D? ownerAnchor, CombatCinematicFormationSnapshot formation)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (((CombatCinematicFormationSnapshot)(ref formation)).HasActorAnchor && ownerAnchor != null && GodotObject.IsInstanceValid((GodotObject)(object)ownerAnchor))
		{
			((GodotObject)overlay).Call(StringName.op_Implicit("set_runtime_actor_anchor"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit((GodotObject)(object)ownerAnchor),
				Variant.op_Implicit(((CombatCinematicFormationSnapshot)(ref formation)).ActorViewportPosition),
				Variant.op_Implicit(true)
			});
		}
	}

	private static Node2D? TryCreateMeirinCinematicTargetProxy(IReadOnlyList<Node2D> anchors, bool reflectSingleTarget)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (anchors.Count == 0)
		{
			return null;
		}
		Vector2 val = default(Vector2);
		if (anchors.Count == 1)
		{
			Node2D val2 = CombatTargetStaging.TryCreateMirroredTargetProxy(anchors[0], "MeirinUgTargetProxy", 0, "Meirin UG target proxy creation failed", ref val, reflectSingleTarget, (Action<string>)delegate(string message)
			{
				SkinLog.Warn(message);
			});
			if (val2 != null)
			{
				val2.Position = Vector2.Zero;
				val2.Rotation = 0f;
				((CanvasItem)val2).TopLevel = false;
				((CanvasItem)val2).ZAsRelative = true;
				((CanvasItem)val2).ZIndex = 0;
				((CanvasItem)val2).Visible = true;
			}
			return val2;
		}
		CombatCinematicFormationSnapshot val3 = CombatCinematicFormation.Capture((Node2D)null, anchors);
		Vector2 formationCenter = ((CombatCinematicFormationSnapshot)(ref val3)).TargetViewportCenter;
		Vector2 val4 = default(Vector2);
		return CombatTargetStaging.TryCreateMirroredTargetProxyGroup(anchors, "MeirinUgTargetProxyGroup", "MeirinUgTargetProxy", 0, 5, Vector2.One, (Func<Node2D, int, int, Vector2, Vector2>)((Node2D anchor, int _, int _, Vector2 baseScale) => (!CombatCinematicFormation.TryGetViewportPosition(anchor, ref val4)) ? Vector2.Zero : ResolveMeirinCinematicProxyOffset(val4 - formationCenter, baseScale)), "Meirin UG AOE target proxy creation failed", ref val, (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		});
	}

	private static Vector2 ResolveMeirinCinematicProxyOffset(Vector2 screenOffset, Vector2 baseScale)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((MathF.Abs(baseScale.X) > 0.001f) ? (screenOffset.X / baseScale.X) : screenOffset.X, (MathF.Abs(baseScale.Y) > 0.001f) ? (screenOffset.Y / baseScale.Y) : screenOffset.Y);
	}

	private static void HideMeirinCinematicActor(MeirinCinematicPhaseState phase, NCreature creatureNode)
	{
		object obj = CombatSpines.Find(creatureNode);
		if (obj == null)
		{
			NCreatureVisuals visuals = creatureNode.Visuals;
			obj = ((visuals != null) ? ((Node)visuals).GetNodeOrNull<Node2D>(NodePath.op_Implicit("%Visuals")) : null);
		}
		Node2D val = (Node2D)obj;
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && !phase.HiddenVisualIds.Contains(((GodotObject)val).GetInstanceId()))
		{
			phase.HiddenVisualIds.Add(((GodotObject)val).GetInstanceId());
			phase.HiddenActors.Add(new MeirinCinematicHiddenActor(val, ((CanvasItem)val).Visible));
			((CanvasItem)val).Hide();
		}
	}

	private static void RestoreMeirinCinematicActors(MeirinCinematicPhaseState phase)
	{
		foreach (MeirinCinematicHiddenActor hiddenActor in phase.HiddenActors)
		{
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)hiddenActor.Visual))
				{
					((CanvasItem)hiddenActor.Visual).Visible = hiddenActor.WasVisible;
				}
			}
			catch (Exception ex)
			{
				SkinLog.Warn("Meirin cinematic actor restore failed: " + SkinLog.FormatException(ex));
			}
		}
		phase.HiddenActors.Clear();
		phase.HiddenVisualIds.Clear();
	}

	private static void RestoreMeirinCinematicBody(MeirinCinematicPhaseState phase, bool completed = false)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (phase.HasRestoredBody || !GodotObject.IsInstanceValid((GodotObject)(object)phase.OwnerNode))
		{
			return;
		}
		Creature entity = phase.OwnerNode.Entity;
		if (entity == null || !entity.IsAlive)
		{
			return;
		}
		phase.HasRestoredBody = true;
		string text = (completed ? SkinProfile.CombatBody.ReadyLoopAnimation : SkinProfile.CombatBody.IdleAnimation);
		if (CombatBodySurface.Ensure(phase.OwnerNode, "cinematic_restore") && CombatBodySurface.HasAnimation(phase.OwnerNode, text, "Meirin cinematic idle restore"))
		{
			if (completed)
			{
				BindMeirinBodyRecovery(phase.OwnerNode);
			}
			SpineAnimationCompat.SetAnimation(phase.OwnerNode.SpineAnimation, text, true, 0);
		}
	}

	private static bool TryHandleMeirinCinematicSupportTrigger(Player player, string triggerName, out Task handledTask)
	{
		handledTask = Task.CompletedTask;
		if (!MeirinCinematicSupportTimings.TryGetValue(player, out MeirinCinematicSupportTimingState value) || !value.TryConsume(triggerName, Time.GetTicksMsec(), out var remainingSeconds))
		{
			return false;
		}
		MeirinCinematicSupportTimings.Remove(player);
		handledTask = ((remainingSeconds <= 0.001f) ? Task.CompletedTask : Cmd.CustomScaledWait(remainingSeconds, remainingSeconds, false, default(CancellationToken)));
		return true;
	}

	private static void FinishMeirinCinematicPhase(Node overlay, int generation)
	{
		MeirinCinematicPhaseState meirinCinematicPhase = _meirinCinematicPhase;
		if (meirinCinematicPhase != null && meirinCinematicPhase.Generation == generation && generation == _meirinCinematicGeneration && meirinCinematicPhase.Overlay == overlay)
		{
			_meirinCinematicPhase = null;
			ReleaseMeirinCinematicPhase(meirinCinematicPhase, restoreBody: true);
		}
	}

	private static void CancelMeirinCinematicPhase(Player? player, bool restoreBody)
	{
		MeirinCinematicPhaseState meirinCinematicPhase = _meirinCinematicPhase;
		if (meirinCinematicPhase != null && (player == null || meirinCinematicPhase.Player == player))
		{
			_meirinCinematicPhase = null;
			NextMeirinCinematicGeneration();
			ReleaseMeirinCinematicPhase(meirinCinematicPhase, restoreBody);
		}
	}

	private static void ReleaseMeirinCinematicPhase(MeirinCinematicPhaseState phase, bool restoreBody)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		phase.TryCompleteVisualEpoch(0uL);
		CompleteMeirinBodyReadyAction(phase.Player);
		MeirinCinematicSupportTimings.Remove(phase.Player);
		if (phase.AttackCommand != null && MeirinAttackTimings.TryGetValue(phase.Player, out MeirinAttackTimingState value) && value.Command == phase.AttackCommand)
		{
			MeirinAttackTimings.Remove(phase.Player);
		}
		try
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)phase.Overlay) && ((GodotObject)phase.Overlay).HasMethod(StringName.op_Implicit("cancel_stage")))
			{
				((GodotObject)phase.Overlay).Call(StringName.op_Implicit("cancel_stage"), Array.Empty<Variant>());
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Meirin cinematic synchronous cancel failed: " + SkinLog.FormatException(ex));
		}
		if (restoreBody)
		{
			RestoreMeirinCinematicBody(phase);
		}
		RestoreMeirinCinematicActors(phase);
		try
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)phase.Overlay))
			{
				phase.Overlay.QueueFree();
			}
		}
		catch (Exception ex2)
		{
			SkinLog.Warn("Meirin cinematic overlay release failed: " + SkinLog.FormatException(ex2));
		}
	}

	private static void ScheduleMeirinCinematicFallback(MeirinCinematicPhaseState phase, float durationSeconds)
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			val.CreateTimer((double)(durationSeconds + 1f), true, false, true).Timeout += delegate
			{
				FinishMeirinCinematicPhase(phase.Overlay, phase.Generation);
			};
		}
	}

	private static int NextMeirinCinematicGeneration()
	{
		_meirinCinematicGeneration++;
		if (_meirinCinematicGeneration <= 0)
		{
			_meirinCinematicGeneration = 1;
		}
		return _meirinCinematicGeneration;
	}

	private static void ConfigureCombatCutInToggleHud()
	{
		CombatCutInToggleHudService.RegisterCharacter(SkinRuntimeSettings, (Func<Player, bool>)IsTargetPlayer, (Func<bool>)(() => SkinProfile.Features.CombatEntryAnimation || SkinProfile.Features.BattleReadyOverlay), (Func<string, string>)SkinProfile.RuntimeNodeName, (Action<string, int>)SkinLog.Warn, (Action<string, int>)delegate(string message, int priority)
		{
			SkinLog.Debug(() => message, priority);
		});
	}

	private static bool CanPlayMeirinAttackBodyRoute(NCreature actor, string triggerName, string cueName)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		CharacterSkinRemoteNativeBodyAnimation mapping = default(CharacterSkinRemoteNativeBodyAnimation);
		if (SkinProfile.CombatBody.TryGetRemoteNativeBodyAnimation(triggerName, ref mapping) && TryResolveMeirinAttackTimingSequence(triggerName, cueName, mapping, out IReadOnlyList<string> sequence, out float _) && CombatBodySurface.Ensure(actor, "attack_route_" + cueName))
		{
			return ValidateAnimations(actor, sequence);
		}
		return false;
	}

	private static string? ResolveAttackPresentationTrigger(AttackCommand attackCommand)
	{
		object obj = AttackCommandShouldPlayAnimationField?.GetValue(attackCommand);
		if (!(obj is bool) || !(bool)obj || !(AttackCommandAttackerAnimNameField?.GetValue(attackCommand) is string text))
		{
			return null;
		}
		if (string.Equals(text, "Attack", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "heavyAttack", StringComparison.OrdinalIgnoreCase))
		{
			return text;
		}
		return null;
	}

	private static void SuppressNativeAttackPresentation(AttackCommand attackCommand)
	{
		if (SkinProfile.Features.SuppressNativeAttackCommandPresentation)
		{
			AttackCommandAttackerVfxField?.SetValue(attackCommand, null);
			AttackCommandHitVfxField?.SetValue(attackCommand, null);
			ClearAttackCommandList(attackCommand, AttackCommandCustomAttackerVfxNodesField);
			ClearAttackCommandList(attackCommand, AttackCommandCustomHitVfxNodesField);
			AttackCommandAttackerSfxField?.SetValue(attackCommand, null);
			AttackCommandTmpAttackerSfxField?.SetValue(attackCommand, null);
			AttackCommandHitSfxField?.SetValue(attackCommand, null);
			AttackCommandTmpHitSfxField?.SetValue(attackCommand, null);
		}
	}

	private static void PlayCombatSfxCues(NCreature actor, string cueName, int token)
	{
		IReadOnlyList<CombatPresentationSfxCue> readOnlyList = default(IReadOnlyList<CombatPresentationSfxCue>);
		if (SkinProfile.CombatPresentation.TryGetSfxCues(cueName, ref readOnlyList))
		{
			CombatSfxCues.Play(actor, readOnlyList, token);
		}
	}

	private static void ClearAttackCommandList(AttackCommand command, FieldInfo? field)
	{
		if (field?.GetValue(command) is IList list)
		{
			list.Clear();
		}
	}

	private static int ConsumeNormalAttackVariant(Player player)
	{
		return NormalAttackVariants.GetOrCreateValue(player).Consume();
	}

	private static void PlayAttackCameraShake(NCreature actor, IReadOnlyList<NCreature>? targetNodes, string cueName, float? delaySecondsOverride = null)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null || val.Root == null)
		{
			return;
		}
		List<Node2D?> anchors = new List<Node2D> { CombatSpines.Find(actor) };
		if (targetNodes != null)
		{
			foreach (NCreature targetNode in targetNodes)
			{
				anchors.Add(CombatSpines.Find(targetNode));
				anchors.Add(ResolveCombatEffectTargetSurfaceAnchor(targetNode));
			}
		}
		anchors.Add(ResolveCombatScreenEffectAnchor((CombatScreenEffectLayer)1));
		anchors.Add(ResolveCombatScreenEffectAnchor((CombatScreenEffectLayer)0));
		float num = delaySecondsOverride ?? (string.Equals(cueName, "u2_attack_play", StringComparison.Ordinal) ? 0.46600002f : (0.2f + (string.Equals(cueName, "f_strong_attack_play", StringComparison.Ordinal) ? 0.099f : 0.066f)));
		Rect2 visibleRect = ((Viewport)val.Root).GetVisibleRect();
		Vector2 viewportSize = ((Rect2)(ref visibleRect)).Size;
		GodotScheduler.Delay(num, (Action)delegate
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			CombatCameraShakeRuntime.PlayAdditivePath((IReadOnlyList<Node2D>)anchors, (IReadOnlyList<CombatCameraShakeTrack>)new _003C_003Ez__ReadOnlyArray<CombatCameraShakeTrack>((CombatCameraShakeTrack[])(object)new CombatCameraShakeTrack[2]
			{
				new CombatCameraShakeTrack(0f, CombatOfficialCameraShakeProfiles.VibrationDown),
				new CombatCameraShakeTrack(0f, CombatOfficialCameraShakeProfiles.VibrationLeft)
			}), viewportSize, 1080f, (TransitionType)7, (EaseType)2);
		});
	}

	private static MeirinCombatRoute ResolveMeirinAttackRoute(CardModel card, AttackCommand attackCommand, decimal hitCount)
	{
		CombatPresentationCardContext context = CombatCardContexts.Build(card, (int)Math.Max(1m, hitCount), 0f, CombatEffectAttackTargets.GetDamagePerHit(attackCommand, (Creature)null, (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		}));
		if (attackCommand.IsRandomlyTargeted)
		{
			CombatPresentationCardContext obj = context._003CClone_003E_0024();
			obj.set_TargetScope((CombatPresentationTargetScope)5);
			context = obj;
		}
		MeirinCombatRoute route = MeirinCombatRouteRules.ResolveAttackRoute(context);
		SkinLog.Debug(() => "Meirin attack route=" + route.ToString() + " damage=" + context.DamagePerHit + " hits=" + context.HitCount + " rarity=" + context.Rarity + " cost=" + context.EnergyCost + " random=" + attackCommand.IsRandomlyTargeted + " multi=" + attackCommand.IsMultiTargeted);
		return route;
	}

	private static void RegisterAttackRouteOverride(Player player, string route)
	{
		AttackRouteGates.GetOrCreateValue(player).Register(route, Time.GetTicksMsec());
	}

	private static bool TryConsumeAttackRouteOverride(Player player, out string route)
	{
		return AttackRouteGates.GetOrCreateValue(player).TryConsume(Time.GetTicksMsec(), out route);
	}

	private static bool IsU2AttackRouteReserved(Player player)
	{
		if (AttackRouteGates.GetOrCreateValue(player).TryPeek(Time.GetTicksMsec(), out string route))
		{
			return string.Equals(route, "u2_attack", StringComparison.Ordinal);
		}
		return false;
	}

	private static void ScheduleNormalAttackVariantExpiry(Player player, int variant)
	{
		GodotScheduler.Delay(1.3f, (Action)delegate
		{
			NormalAttackVariants.GetOrCreateValue(player).ExpireIfPending(variant);
		});
	}

	private static int AttachCombatEffectLayers(Node2D effectRoot, Node sourceRoot)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return CombatEffectRuntime.AttachLayersByOriginalZ(effectRoot, sourceRoot, SkinProfile.CombatEffects.IdleModulate, SkinProfile.CombatEffects.BehindParentLayerName);
	}

	private static void ApplyCombatEffectMaterial(Node root, CombatPresentationEffectCue cue)
	{
		if (!ShouldPreserveMeirinSceneMaterial(cue))
		{
			EffectMaterials.ApplyEffectCueMaterial(root, cue, (Func<Node, Color>)null, (string)null);
			GodotScheduler.DeferredFrames(root, 4, (Action<Node>)delegate(Node node)
			{
				EffectMaterials.ApplySpineOnlyEffectCueMaterial(node, cue, (string)null);
			});
		}
	}

	private static bool ShouldPreserveMeirinSceneMaterial(CombatPresentationEffectCue cue)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)cue.Source.Kind == 0)
		{
			return cue.Source.ScenePath?.StartsWith("res://MeirinWatcherSkin/scenes/combat_effects/", StringComparison.Ordinal) ?? false;
		}
		return false;
	}

	private static int PrepareCombatEffectRoot(Node2D root, Node2D anchor, CombatPresentationEffectCue cue)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		int result = CombatEffects.BeginPlayback(root, cue);
		((Node)root).Name = StringName.op_Implicit(SkinProfile.RuntimeNodeName(cue.GroupName ?? "CombatEffect"));
		root.Position = Vector2.Zero;
		root.Scale = Vector2.One;
		root.Rotation = 0f;
		((CanvasItem)root).TopLevel = false;
		((CanvasItem)root).ZAsRelative = true;
		((CanvasItem)root).ZIndex = CombatPresentationGodotLayeringRuntime.ResolveCueZIndex(cue);
		((CanvasItem)root).ShowBehindParent = (int)cue.Anchor == 0 && cue.ZIndex < 0;
		((CanvasItem)root).VisibilityLayer = ((CanvasItem)anchor).VisibilityLayer;
		((CanvasItem)root).LightMask = ((CanvasItem)anchor).LightMask;
		((CanvasItem)root).Modulate = (Color)(ShouldPreserveMeirinSceneMaterial(cue) ? ResolveMeirinSceneEffectModulate(cue.Alpha, cue.BrightnessMultiplier) : new Color(1f, 1f, 1f, cue.Alpha));
		Vector2 val = (cue.NeutralizeAnchorScale ? ResolveCombatEffectAnchorScale(anchor) : Vector2.One);
		if (MathF.Abs(cue.OffsetX) > 0.001f || MathF.Abs(cue.OffsetY) > 0.001f)
		{
			root.Position += new Vector2(cue.OffsetX / val.X, cue.OffsetY / val.Y);
		}
		if (cue.NeutralizeAnchorScale)
		{
			root.Scale *= new Vector2(1f / val.X, 1f / val.Y);
		}
		root.Scale *= cue.Scale;
		root.RotationDegrees += cue.RotationDegrees;
		return result;
	}

	private static Color ResolveMeirinSceneEffectModulate(float sourceAlpha = 1f, float brightnessMultiplier = 1f)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp(0.55f * SkinRuntimeSettings.EffectBrightness * MathF.Max(0f, brightnessMultiplier), 0f, 2f);
		float num2 = Mathf.Clamp(0.85f * MathF.Max(0f, sourceAlpha) * SkinRuntimeSettings.EffectOpacity, 0f, 1f);
		return new Color(num, num, num, num2);
	}

	private static Vector2 ResolveCombatEffectAnchorScale(Node2D anchor)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector2 globalScale = anchor.GlobalScale;
		return new Vector2((MathF.Abs(globalScale.X) > 0.001f) ? globalScale.X : 1f, (MathF.Abs(globalScale.Y) > 0.001f) ? globalScale.Y : 1f);
	}

	private static Node2D? ResolveCombatEffectTargetVisualAnchor(NCreature? target)
	{
		if (target == null || !GodotObject.IsInstanceValid((GodotObject)(object)target))
		{
			return null;
		}
		return CombatTargetSurfaceAnchorRuntime.Resolve(target, SkinProfile.RuntimeNodeName("CombatEffectTargetVisualAnchor"), "Meirin target visual effect anchor failed: ", (Action<string, int>)SkinLog.Warn, (Action<NCreature, Node2D, Vector2, string>)null, (CombatTargetSurfaceAnchorMode)1) ?? CombatSpines.Find(target);
	}

	private static Node2D? ResolveCombatEffectTargetSurfaceAnchor(NCreature? target)
	{
		if (target == null || !GodotObject.IsInstanceValid((GodotObject)(object)target))
		{
			return null;
		}
		return CombatTargetSurfaceAnchorRuntime.Resolve(target, SkinProfile.RuntimeNodeName("CombatEffectTargetSurfaceAnchor"), "Meirin target surface effect anchor failed: ", (Action<string, int>)SkinLog.Warn, (Action<NCreature, Node2D, Vector2, string>)null, (CombatTargetSurfaceAnchorMode)0) ?? CombatSpines.Find(target);
	}

	private static Node2D? ResolveCombatScreenEffectAnchor(CombatScreenEffectLayer layer)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return CombatScreenEffectAnchorRuntime.Resolve(SkinProfile.RuntimeNodeName(((int)layer == 1) ? "CombatScreenEffectAnchor" : "CombatScreenBackEffectAnchor"), layer, CombatScreenEffectReferenceSize, (CombatScreenEffectScaleMode)0, (Action<string>)delegate(string message)
		{
			SkinLog.Warn("Meirin combat screen effect anchor failed: " + message);
		});
	}

	private static void StartCombatEffect(Node2D root, CombatPresentationEffectCue cue)
	{
		CombatEffectRuntime.StartSpineAnimation((Node)(object)root, string.IsNullOrWhiteSpace(cue.SpineAnimationName) ? "animation" : cue.SpineAnimationName, false, cue.TimeScale, (Action<string>)delegate(string message)
		{
			SkinLog.Warn("Meirin combat effect animation failed: " + message);
		});
		if (PresentationGates.CombatParticlesEnabled)
		{
			CombatEffectRuntime.StartParticles((Node)(object)root, (Action<string>)delegate(string message)
			{
				SkinLog.Warn("Meirin combat effect particles failed: " + message);
			});
		}
	}

	private static void ClearCombatEffects(Node2D anchor)
	{
		foreach (Node child in ((Node)anchor).GetChildren(false))
		{
			Node2D val = (Node2D)(object)((child is Node2D) ? child : null);
			if (val != null && CombatEffects.IsTransientRoot((Node)(object)val))
			{
				CombatEffects.Release(val, (int?)null);
			}
		}
	}

	private static void CleanupCombatEffects(bool clearSceneCache)
	{
		CancelMeirinCinematicPhase(null, restoreBody: true);
		CleanupMeirinMeleeApproaches();
		if (CombatEffectEngineLazy.IsValueCreated)
		{
			CombatEffectEngine.Reset(clearSceneCache);
		}
	}

	private static void ScheduleCombatEntries(IEnumerable<Player>? players)
	{
		CancelMeirinCinematicPhase(null, restoreBody: true);
		PendingCombatEntryPlayers.Clear();
		ClearMeirinMultiHitPresentations(players, restoreBody: true);
		RemoteNativeBodyTriggers.ScheduleCombatEntryForRemotePlayers(players);
		if (players == null)
		{
			return;
		}
		foreach (Player player in players)
		{
			ClearMeirinBodyReady(player);
			MeirinAttackTimings.Remove(player);
			if (IsTargetPlayer(player) && IsLocalPlayer(player))
			{
				PendingCombatEntryPlayers.Add(player);
				if (TryPlayCombatEntry(player))
				{
					PendingCombatEntryPlayers.Remove(player);
				}
			}
		}
	}

	private static void TryPlayPendingCombatEntry(NCreature? creatureNode)
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
		if (val != null && PendingCombatEntryPlayers.Contains(val) && TryPlayCombatEntry(val))
		{
			PendingCombatEntryPlayers.Remove(val);
		}
	}

	private static bool TryPlayCombatEntry(Player player)
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		NCreature val = CombatCreatureNodes.ResolvePlayer(NCombatRoom.Instance, player, "Meirin combat entry player node lookup failed", (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		});
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return false;
		}
		CombatBodySurface.Ensure(val, "combat_entry");
		IReadOnlyList<CombatPresentationBodyStep> readOnlyList = CombatBodyEntryPlaybackRuntime.BuildEntrySteps(SkinProfile.CombatBody);
		if (readOnlyList.Count == 0)
		{
			CombatEntryBodyVisibility.Reveal(val, "empty_entry");
			return true;
		}
		foreach (CombatPresentationBodyStep item in readOnlyList)
		{
			if (!CombatBodySurface.HasAnimation(val, item.BodyAnimation, "Meirin combat entry"))
			{
				CombatEntryBodyVisibility.Reveal(val, "missing_entry_animation");
				return true;
			}
		}
		string idleAnimation = SkinProfile.CombatBody.IdleAnimation;
		if (!CombatBodySurface.HasAnimation(val, idleAnimation, "Meirin combat entry idle"))
		{
			CombatEntryBodyVisibility.Reveal(val, "missing_entry_idle");
			return true;
		}
		int token = NextCombatEntryToken();
		CombatBodyEntryRunInStart val2 = CombatBodyEntryPlaybackRuntime.TryBeginRunIn(SkinProfile.CombatBody, readOnlyList, val, (Action<string>)delegate(string message)
		{
			SkinLog.Debug("Meirin " + message);
		}, (Action<string>)delegate(string message)
		{
			SkinLog.Warn("Meirin " + message);
		});
		IReadOnlyList<CombatBodySequenceStepSchedule> readOnlyList2 = CombatEntryBodySequences.BuildStepSchedule(readOnlyList);
		SpineAnimationCompat.SetAnimation(val.SpineAnimation, readOnlyList[0].BodyAnimation, false, 0);
		CombatEntryBodyVisibility.Reveal(val, "entry_start");
		Vector2 offset = default(Vector2);
		for (int i = 1; i < readOnlyList2.Count; i++)
		{
			CombatBodySequenceStepSchedule val3 = readOnlyList2[i];
			if (((CombatBodyEntryRunInStart)(ref val2)).UsesSegmentOffsets && CombatBodyEntryPlaybackRuntime.TryGetSegmentOffset(SkinProfile.CombatBody, i, ref offset))
			{
				QueueCombatEntrySegmentOffset(val, token, ((CombatBodyEntryRunInStart)(ref val2)).TargetPosition, offset, ((CombatBodySequenceStepSchedule)(ref val3)).DelaySeconds);
			}
			SpineAnimationCompat.AddAnimation(val.SpineAnimation, ((CombatBodySequenceStepSchedule)(ref val3)).Step.BodyAnimation, 0f, false, 0);
		}
		SpineAnimationCompat.AddAnimation(val.SpineAnimation, idleAnimation, 0f, true, 0);
		CombatBodyEntryPlaybackRuntime.ScheduleCompletion(val, val2, CombatEntryBodySequences.GetSequenceDelaySeconds(readOnlyList), (Action<string>)delegate(string message)
		{
			SkinLog.Debug("Meirin " + message);
		}, (Action<string>)delegate(string message)
		{
			SkinLog.Warn("Meirin " + message);
		});
		SkinLog.Info("Queued Meirin combat entry sequence=" + CombatEntryBodySequences.FormatSequence(readOnlyList));
		return true;
	}

	private static int NextCombatEntryToken()
	{
		_combatEntryToken++;
		if (_combatEntryToken <= 0)
		{
			_combatEntryToken = 1;
		}
		return _combatEntryToken;
	}

	private static void QueueCombatEntrySegmentOffset(NCreature creatureNode, int token, Vector2 targetPosition, Vector2 offset, float delaySeconds)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		GodotScheduler.Delay(Math.Max(0.05f, delaySeconds), (Action)delegate
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (token == _combatEntryToken && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode))
			{
				CombatBodyEntryPlaybackRuntime.ApplySegmentOffset(creatureNode, targetPosition, offset);
			}
		});
	}

	private static void OnCombatBodySurfaceApplied(NCreature creatureNode)
	{
		CleanupMeirinMeleeApproaches();
		CombatEntryBodyVisibility.HideUntilEntryStart(creatureNode);
	}

	private static void PlayCombatVictoryForPlayers(IEnumerable<Player>? players)
	{
		NextCombatEntryToken();
		CancelMeirinCinematicPhase(null, restoreBody: true);
		CleanupMeirinMeleeApproaches();
		PendingCombatEntryPlayers.Clear();
		BattleReadyOverlay.Cleanup(false);
		ClearMeirinMultiHitPresentations(players, restoreBody: false);
		RemoteNativeBodyTriggers.ScheduleCombatEntryForRemotePlayers((IEnumerable<Player>)Array.Empty<Player>());
		if (players == null)
		{
			return;
		}
		foreach (Player player in players)
		{
			ClearMeirinBodyReady(player);
			MeirinAttackTimings.Remove(player);
			if (!IsTargetPlayer(player) || !CombatAudience.Allows(player, (CombatPresentationChannel)0))
			{
				continue;
			}
			NCreature val = CombatCreatureNodes.ResolvePlayer(NCombatRoom.Instance, player, "Meirin victory player node lookup failed", (Action<string>)delegate(string message)
			{
				SkinLog.Warn(message);
			});
			if (val != null)
			{
				CombatBodyEntryPlaybackRuntime.CompleteActiveRunIn(val, "victory", (Action<string>)delegate(string message)
				{
					SkinLog.Debug("Meirin " + message);
				}, (Action<string>)delegate(string message)
				{
					SkinLog.Warn("Meirin " + message);
				});
				TryPlayCombatBodySequence(val, SkinProfile.CombatBody.VictoryStartAnimations, SkinProfile.CombatBody.VictoryLoopAnimation, "Meirin combat victory");
			}
		}
	}

	private static bool TryPlayCombatBodySequence(NCreature creatureNode, IReadOnlyList<string> sequence, string? loopAnimation, string warningPrefix)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (sequence.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < sequence.Count; i++)
		{
			string text = sequence[i];
			if (!CombatBodySurface.HasAnimation(creatureNode, text, warningPrefix))
			{
				continue;
			}
			bool flag = i == sequence.Count - 1 && string.Equals(text, loopAnimation, StringComparison.Ordinal);
			SpineAnimationCompat.SetAnimation(creatureNode.SpineAnimation, text, flag, 0);
			for (int j = i + 1; j < sequence.Count; j++)
			{
				string text2 = sequence[j];
				if (CombatBodySurface.HasAnimation(creatureNode, text2, warningPrefix))
				{
					bool flag2 = j == sequence.Count - 1 && string.Equals(text2, loopAnimation, StringComparison.Ordinal);
					SpineAnimationCompat.AddAnimation(creatureNode.SpineAnimation, text2, 0f, flag2, 0);
				}
			}
			if (!string.IsNullOrWhiteSpace(loopAnimation) && !string.Equals(sequence[sequence.Count - 1], loopAnimation, StringComparison.Ordinal) && CombatBodySurface.HasAnimation(creatureNode, loopAnimation, warningPrefix + " loop"))
			{
				SpineAnimationCompat.AddAnimation(creatureNode.SpineAnimation, loopAnimation, 0f, true, 0);
			}
			return true;
		}
		return false;
	}

	private static void TryBeginMeirinMeleeApproach(NCreature ownerCreature, IReadOnlyList<NCreature>? targetCreatures, string cueName)
	{
		TryBeginMeirinMeleeApproach(ownerCreature, targetCreatures, null, cueName, targetAlreadyResolved: false);
	}

	private static void TryBeginMeirinResolvedTargetMeleeApproach(NCreature ownerCreature, NCreature resolvedTarget, string cueName)
	{
		TryBeginMeirinMeleeApproach(ownerCreature, null, resolvedTarget, cueName, targetAlreadyResolved: true);
	}

	private static void TryBeginMeirinMeleeApproach(NCreature ownerCreature, IReadOnlyList<NCreature>? targetCreatures, NCreature? resolvedTarget, string cueName, bool targetAlreadyResolved)
	{
		SkinPatchGuard.Run("Meirin melee approach", (Action)delegate
		{
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0211: Unknown result type (might be due to invalid IL or missing references)
			//IL_0213: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0229: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
			if (SkinRuntimeSettings.MeleeApproachMovementEnabled && (resolvedTarget != null || (targetCreatures != null && targetCreatures.Count != 0)))
			{
				Node2D val = CombatSpines.Find(ownerCreature);
				Node obj = ((val != null) ? ((Node)val).GetParent() : null);
				Node2D val2 = (Node2D)(object)((obj is Node2D) ? obj : null);
				if (val2 != null)
				{
					NCreature targetCreature;
					if (resolvedTarget != null)
					{
						targetCreature = resolvedTarget;
					}
					else if (!TryResolveNearestMeleeTarget(ownerCreature, val, targetCreatures, out targetCreature))
					{
						return;
					}
					if (targetCreature != null && ownerCreature != targetCreature)
					{
						Node2D val3 = CombatVisualAnchors.Find(targetCreature);
						if (val3 != null)
						{
							MeirinMeleeMotionState orCreateValue = MeirinMeleeMotions.GetOrCreateValue(val);
							ApplyMeirinMeleeRouteTiming(orCreateValue, cueName);
							int playStepEffects;
							if (PresentationGates.CombatPresentationEffectsEnabled)
							{
								CombatPresentationAudiencePolicy combatAudience = CombatAudience;
								Creature entity = ownerCreature.Entity;
								playStepEffects = (combatAudience.Allows((entity != null) ? entity.Player : null, (CombatPresentationChannel)1) ? 1 : 0);
							}
							else
							{
								playStepEffects = 0;
							}
							orCreateValue.PlayStepEffects = (byte)playStepEffects != 0;
							if (TryResolveMeirinMeleeOffsetX(cueName, out var offsetX))
							{
								bool flag = CanContinueMeirinMeleeApproach(orCreateValue, targetCreature);
								if (!flag)
								{
									RestoreMeirinMeleeMotion(val, removeState: false);
								}
								Vector2 val4 = (flag ? orCreateValue.OriginalPosition : val.Position);
								Vector2 val5 = val2.ToGlobal(val4);
								float num = MathF.Sign(val3.GlobalPosition.X - val5.X);
								if (!(MathF.Abs(num) < 0.5f))
								{
									float num2 = ((num > 0f) ? offsetX : (0f - offsetX));
									float num3 = val3.GlobalPosition.X + num2 - val5.X;
									if (!(num3 * num <= 1f) && !(MathF.Abs(num3) < 40f))
									{
										float num4 = 760f;
										MainLoop mainLoop = Engine.GetMainLoop();
										SceneTree val6 = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
										if (val6 != null)
										{
											Window root = val6.Root;
											float num5;
											if (root == null)
											{
												num5 = 0f;
											}
											else
											{
												Rect2 visibleRect = ((Viewport)root).GetVisibleRect();
												num5 = ((Rect2)(ref visibleRect)).Size.X;
											}
											float num6 = num5;
											if (num6 > 1f)
											{
												num4 = MathF.Max(num4, num6 * 0.85f);
											}
										}
										num3 = Math.Clamp(num3, 0f - num4, num4);
										Vector2 val7 = CombatMeleeFormation.ResolveApproachDelta(ownerCreature, targetCreature, num3);
										Vector2 val8 = val2.ToLocal(val5 + val7);
										if (flag && TryContinueMeirinMeleeApproach(val, orCreateValue, targetCreature, val8, cueName, offsetX, targetAlreadyResolved, out var token))
										{
											ScheduleMeirinMeleeReturn(val, orCreateValue, token);
										}
										else
										{
											int token2 = orCreateValue.NextToken();
											orCreateValue.OriginalPosition = val.Position;
											orCreateValue.Active = true;
											orCreateValue.Returning = false;
											orCreateValue.TargetCreatureInstanceId = ((targetCreature != null) ? ((GodotObject)targetCreature).GetInstanceId() : 0);
											ActiveMeirinMeleeSpines.Add(val);
											int zIndex = (orCreateValue.StepEffectZIndex = -3);
											CombatLayerLiftRuntime.AcquireDrawOrder(val, MeirinMeleeLayerLiftOwnerKey);
											if (orCreateValue.PlayStepEffects)
											{
												PrewarmMeirinMeleeStepEffects();
												TryPlayMeirinMeleeStepEffect(val2, orCreateValue.OriginalPosition, MeirinMeleeStepEffectKind.MoveStart, token2, zIndex);
											}
											ScheduleMeirinMeleePositionSnap(val, orCreateValue, val8, token2, targetAlreadyResolved ? 0f : 0.04f, returning: false);
											if (orCreateValue.PlayStepEffects)
											{
												ScheduleMeirinMeleeStepEffect(val, orCreateValue, val2, val8, MeirinMeleeStepEffectKind.MoveArrive, token2, zIndex, targetAlreadyResolved ? 0f : orCreateValue.ApproachSeconds);
											}
											ScheduleMeirinMeleeReturn(val, orCreateValue, token2);
										}
									}
								}
							}
						}
					}
				}
			}
		}, 2);
	}

	private static bool CanContinueMeirinMeleeApproach(MeirinMeleeMotionState state, NCreature? targetCreature)
	{
		if (state.Active && !state.Returning && targetCreature != null && GodotObject.IsInstanceValid((GodotObject)(object)targetCreature) && state.TargetCreatureInstanceId != 0L)
		{
			return state.TargetCreatureInstanceId == ((GodotObject)targetCreature).GetInstanceId();
		}
		return false;
	}

	private static bool TryContinueMeirinMeleeApproach(Node2D ownerSpine, MeirinMeleeMotionState state, NCreature? targetCreature, Vector2 targetPosition, string cueName, float offsetX, bool targetAlreadyResolved, out int token)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		token = 0;
		if (!CanContinueMeirinMeleeApproach(state, targetCreature))
		{
			return false;
		}
		token = state.NextToken();
		state.Returning = false;
		CombatLayerLiftRuntime.AcquireDrawOrder(ownerSpine, MeirinMeleeLayerLiftOwnerKey);
		Vector2 previousPosition = ownerSpine.Position;
		Vector2 val = targetPosition - previousPosition;
		if (((Vector2)(ref val)).LengthSquared() > 1f)
		{
			ScheduleMeirinMeleePositionSnap(ownerSpine, state, targetPosition, token, targetAlreadyResolved ? 0f : 0.04f, returning: false);
			SkinLog.Debug(() => "Realigned Meirin consecutive melee position cue=" + cueName + " offsetX=" + offsetX + " from=" + ((object)(Vector2)(ref previousPosition)/*cast due to .constrained prefix*/).ToString() + " to=" + ((object)(Vector2)(ref targetPosition)/*cast due to .constrained prefix*/).ToString() + " token=" + state.Token);
		}
		else
		{
			SkinLog.Debug(() => "Preserved aligned Meirin melee position cue=" + cueName + " offsetX=" + offsetX + " token=" + state.Token);
		}
		return true;
	}

	private static void ScheduleMeirinMeleeReturn(Node2D ownerSpine, MeirinMeleeMotionState state, int token)
	{
		GodotScheduler.Delay(state.ReturnDelaySeconds, (Action)delegate
		{
			BeginMeirinMeleeReturn(ownerSpine, state, token);
		});
		GodotScheduler.Delay(state.FailsafeSeconds, (Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)ownerSpine) && MeirinMeleeMotions.TryGetValue(ownerSpine, out MeirinMeleeMotionState value) && value == state && value.Active && value.Token == token)
			{
				RestoreMeirinMeleeMotion(ownerSpine, removeState: false);
			}
		});
	}

	private static void ScheduleMeirinMeleePositionSnap(Node2D ownerSpine, MeirinMeleeMotionState state, Vector2 targetPosition, int token, float delaySeconds, bool returning)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (delaySeconds <= 0.001f)
		{
			Apply();
		}
		else
		{
			GodotScheduler.Delay(delaySeconds, (Action)Apply);
		}
		void Apply()
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)ownerSpine) && MeirinMeleeMotions.TryGetValue(ownerSpine, out MeirinMeleeMotionState value) && value == state && value.Active && value.Token == token && value.Returning == returning)
			{
				CombatCameraShakeRuntime.CancelAdditivePath(ownerSpine);
				ownerSpine.Position = targetPosition;
			}
		}
	}

	private static void ScheduleMeirinMeleeStepEffect(Node2D ownerSpine, MeirinMeleeMotionState state, Node2D ownerParent, Vector2 localPosition, MeirinMeleeStepEffectKind kind, int token, int zIndex, float delaySeconds)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (delaySeconds <= 0.001f)
		{
			Play();
		}
		else
		{
			GodotScheduler.Delay(delaySeconds, (Action)Play);
		}
		void Play()
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)ownerSpine) && GodotObject.IsInstanceValid((GodotObject)(object)ownerParent) && MeirinMeleeMotions.TryGetValue(ownerSpine, out MeirinMeleeMotionState value) && value == state && value.Active && value.Token == token)
			{
				TryPlayMeirinMeleeStepEffect(ownerParent, localPosition, kind, token, zIndex);
			}
		}
	}

	private static void ApplyMeirinMeleeRouteTiming(MeirinMeleeMotionState state, string cueName)
	{
		bool flag = string.Equals(cueName, "u2_attack_play", StringComparison.Ordinal);
		state.ApproachSeconds = (flag ? 0.4f : 0.2f);
		state.ReturnDelaySeconds = (flag ? 1.0667f : 0.8667f);
		state.FailsafeSeconds = (flag ? 1.6f : 1.4f);
	}

	private static bool TryResolveMeirinMeleeOffsetX(string cueName, out float offsetX)
	{
		if (string.Equals(cueName, "attack_play1", StringComparison.Ordinal))
		{
			offsetX = -100f;
			return true;
		}
		if (string.Equals(cueName, "attack_play2", StringComparison.Ordinal))
		{
			offsetX = -156f;
			return true;
		}
		if (string.Equals(cueName, "u2_attack_play", StringComparison.Ordinal))
		{
			offsetX = -156f;
			return true;
		}
		if (string.Equals(cueName, "f_strong_attack_play", StringComparison.Ordinal))
		{
			offsetX = -156f;
			return true;
		}
		offsetX = 0f;
		return false;
	}

	private static bool TryResolveNearestMeleeTarget(NCreature ownerCreature, Node2D ownerSpine, IReadOnlyList<NCreature> candidates, out NCreature? targetCreature)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		targetCreature = null;
		float num = float.PositiveInfinity;
		foreach (NCreature candidate in candidates)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)candidate) || ownerCreature == candidate || candidate.Entity == null || !candidate.Entity.IsAlive)
			{
				continue;
			}
			Node2D val = CombatVisualAnchors.Find(candidate);
			if (val != null)
			{
				float num2 = MathF.Abs(val.GlobalPosition.X - ownerSpine.GlobalPosition.X);
				if (!(num2 >= num))
				{
					num = num2;
					targetCreature = candidate;
				}
			}
		}
		return targetCreature != null;
	}

	private static void BeginMeirinMeleeReturn(Node2D ownerSpine, MeirinMeleeMotionState expectedState, int token)
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)ownerSpine) || !MeirinMeleeMotions.TryGetValue(ownerSpine, out MeirinMeleeMotionState state) || state != expectedState || !state.Active || state.Token != token)
		{
			return;
		}
		state.Returning = true;
		if (state.PlayStepEffects)
		{
			Node parent = ((Node)ownerSpine).GetParent();
			Node2D val = (Node2D)(object)((parent is Node2D) ? parent : null);
			if (val != null)
			{
				TryPlayMeirinMeleeStepEffect(val, ownerSpine.Position, MeirinMeleeStepEffectKind.ReturnStart, token, state.StepEffectZIndex);
			}
		}
		Vector2 originalPosition = state.OriginalPosition;
		GodotScheduler.Delay(0.06f, (Action)delegate
		{
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)ownerSpine) && MeirinMeleeMotions.TryGetValue(ownerSpine, out MeirinMeleeMotionState value) && value == state && value.Active && value.Returning && value.Token == token)
			{
				CombatCameraShakeRuntime.CancelAdditivePath(ownerSpine);
				ownerSpine.Position = originalPosition;
				if (value.PlayStepEffects)
				{
					Node parent2 = ((Node)ownerSpine).GetParent();
					Node2D val2 = (Node2D)(object)((parent2 is Node2D) ? parent2 : null);
					if (val2 != null)
					{
						TryPlayMeirinMeleeStepEffect(val2, originalPosition, MeirinMeleeStepEffectKind.ReturnArrive, token, value.StepEffectZIndex);
					}
				}
				CompleteMeirinMeleeMotion(ownerSpine, value, token);
			}
		});
	}

	private static void PrewarmMeirinMeleeStepEffects()
	{
		if (PresentationGates.CombatPresentationEffectsEnabled)
		{
			CombatEffects.LoadScene("res://MeirinWatcherSkin/scenes/combat_melee_player_move.tscn", true);
			CombatEffects.LoadScene("res://MeirinWatcherSkin/scenes/combat_melee_player_arrive.tscn", true);
		}
	}

	private static void TryPlayMeirinMeleeStepEffect(Node2D ownerParent, Vector2 localPosition, MeirinMeleeStepEffectKind kind, int token, int zIndex)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		if (!PresentationGates.CombatPresentationEffectsEnabled || !GodotObject.IsInstanceValid((GodotObject)(object)ownerParent))
		{
			return;
		}
		float num = 1f;
		Vector2 val = Vector2.Zero;
		bool flag = false;
		string text;
		string text2;
		float num2;
		switch (kind)
		{
		default:
			return;
		case MeirinMeleeStepEffectKind.MoveStart:
			text = "res://MeirinWatcherSkin/scenes/combat_melee_player_arrive.tscn";
			text2 = "meirin_melee_move_start";
			num2 = 0.8f;
			break;
		case MeirinMeleeStepEffectKind.MoveArrive:
			text = "res://MeirinWatcherSkin/scenes/combat_melee_player_move.tscn";
			text2 = "meirin_melee_move_arrive";
			num2 = 0.9f;
			val = MeirinMeleeApproachStepOfficialOffset;
			break;
		case MeirinMeleeStepEffectKind.ReturnStart:
			text = "res://MeirinWatcherSkin/scenes/combat_melee_player_move.tscn";
			text2 = "meirin_melee_return_start";
			num2 = 0.9f;
			num = 0.7f;
			val = MeirinMeleeReturnStepSts2Offset;
			flag = true;
			break;
		case MeirinMeleeStepEffectKind.ReturnArrive:
			text = "res://MeirinWatcherSkin/scenes/combat_melee_player_arrive.tscn";
			text2 = "meirin_melee_return_arrive";
			num2 = 0.8f;
			break;
		}
		CombatPresentationEffectCue val2 = new CombatPresentationEffectCue(text, 0f, num2, false, (CombatPresentationEffectPalette)0, (CombatPresentationEffectAnchor)0, true, false, text2, num, 1f, 0f, 0f, 0f, 1f, 0, 0f, 0f, true, 2, true, (CombatPresentationEffectImportance)0, 1f, (int?)null, false, (CombatPresentationEffectWarmupTier)0, (string)null, 0f, false);
		PackedScene val3 = CombatEffects.LoadScene(text, true);
		Node2D effectRoot = ((val3 == null) ? null : CombatEffects.AcquireRoot(val2, val3, true));
		if (effectRoot == null)
		{
			return;
		}
		int generation = CombatEffects.BeginPlayback(effectRoot, val2);
		((Node)effectRoot).Name = StringName.op_Implicit(SkinProfile.RuntimeNodeName(text2));
		effectRoot.Position = localPosition + val;
		effectRoot.Scale = new Vector2(flag ? (0f - num) : num, num);
		effectRoot.Rotation = 0f;
		((CanvasItem)effectRoot).TopLevel = false;
		((CanvasItem)effectRoot).ZAsRelative = zIndex == 0;
		((CanvasItem)effectRoot).ZIndex = ((zIndex != 0) ? (-3) : 0);
		((CanvasItem)effectRoot).Visible = true;
		((CanvasItem)effectRoot).Modulate = ResolveMeirinSceneEffectModulate();
		((CanvasItem)effectRoot).VisibilityLayer = ((CanvasItem)ownerParent).VisibilityLayer;
		((CanvasItem)effectRoot).LightMask = ((CanvasItem)ownerParent).LightMask;
		((Node)ownerParent).AddChild((Node)(object)effectRoot, false, (InternalMode)0);
		CombatPresentationGodotLayeringRuntime.PlaceNativeForeground(effectRoot, ownerParent);
		if (!CombatEffects.MarkPlaybackVisible(effectRoot, generation))
		{
			CombatEffects.Release(effectRoot, (int?)generation);
			return;
		}
		ActiveMeirinMeleeStepEffects.Add(effectRoot);
		StartMeirinMeleeStepAnimation((Node)(object)effectRoot, "Back", "b");
		StartMeirinMeleeStepAnimation((Node)(object)effectRoot, "Front", "f");
		GodotScheduler.Delay(num2, (Action)delegate
		{
			ActiveMeirinMeleeStepEffects.Remove(effectRoot);
			if (GodotObject.IsInstanceValid((GodotObject)(object)effectRoot))
			{
				CombatEffects.Release(effectRoot, (int?)generation);
			}
		});
	}

	private static void StartMeirinMeleeStepAnimation(Node root, string childName, string animationName)
	{
		Node nodeOrNull = root.GetNodeOrNull<Node>(NodePath.op_Implicit(childName));
		if (nodeOrNull != null)
		{
			CombatEffectRuntime.StartSpineAnimation(nodeOrNull, animationName, false, 1f, (Action<string>)delegate(string message)
			{
				SkinLog.Warn("Meirin melee step animation failed: " + message);
			});
		}
	}

	private static void CompleteMeirinMeleeMotion(Node2D ownerSpine, MeirinMeleeMotionState expectedState, int token)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)ownerSpine) && MeirinMeleeMotions.TryGetValue(ownerSpine, out MeirinMeleeMotionState value) && value == expectedState && value.Token == token)
		{
			CombatCameraShakeRuntime.CancelAdditivePath(ownerSpine);
			ownerSpine.Position = value.OriginalPosition;
			value.Active = false;
			value.Returning = false;
			value.TargetCreatureInstanceId = 0uL;
			CombatLayerLiftRuntime.Release(ownerSpine, MeirinMeleeLayerLiftOwnerKey);
			ActiveMeirinMeleeSpines.Remove(ownerSpine);
		}
	}

	private static void RestoreMeirinMeleeMotion(Node2D ownerSpine, bool removeState)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (MeirinMeleeMotions.TryGetValue(ownerSpine, out MeirinMeleeMotionState value))
		{
			if (value.Active && GodotObject.IsInstanceValid((GodotObject)(object)ownerSpine))
			{
				CombatCameraShakeRuntime.CancelAdditivePath(ownerSpine);
				ownerSpine.Position = value.OriginalPosition;
			}
			value.Active = false;
			value.Returning = false;
			value.TargetCreatureInstanceId = 0uL;
		}
		CombatLayerLiftRuntime.Release(ownerSpine, MeirinMeleeLayerLiftOwnerKey);
		ActiveMeirinMeleeSpines.Remove(ownerSpine);
		if (removeState)
		{
			MeirinMeleeMotions.Remove(ownerSpine);
		}
	}

	private static void RestoreMeirinMeleeMotionForActor(NCreature actor)
	{
		Node2D val = CombatSpines.Find(actor);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && MeirinMeleeMotions.TryGetValue(val, out MeirinMeleeMotionState value) && value.Active)
		{
			RestoreMeirinMeleeMotion(val, removeState: false);
			SkinLog.Debug(() => "Restored Meirin melee home before support-card presentation.");
		}
	}

	private static void CleanupMeirinMeleeApproaches()
	{
		Node2D[] array = (Node2D[])(object)new Node2D[ActiveMeirinMeleeStepEffects.Count];
		ActiveMeirinMeleeStepEffects.CopyTo(array);
		Node2D[] array2 = array;
		foreach (Node2D val in array2)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				CombatEffects.Release(val, (int?)null);
			}
		}
		ActiveMeirinMeleeStepEffects.Clear();
		Node2D[] array3 = (Node2D[])(object)new Node2D[ActiveMeirinMeleeSpines.Count];
		ActiveMeirinMeleeSpines.CopyTo(array3);
		array2 = array3;
		foreach (Node2D val2 in array2)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				RestoreMeirinMeleeMotion(val2, removeState: true);
			}
		}
		ActiveMeirinMeleeSpines.Clear();
		CombatLayerLiftRuntime.ReleaseAll(MeirinMeleeLayerLiftOwnerKey);
	}

	public static void Init()
	{
		_ = SettingsUiRuntime;
		SkinLog.Configure(SkinProfile.LogPrefix);
		if (SkinProfile.ResolveTargetCharacterType() == null)
		{
			SkinLog.InfoOnce("meirin_watcher_target_missing", "WatcherMod.Watcher was not found. Meirin presentation is disabled for this Watcher port.");
			return;
		}
		CharacterSkinModBootstrap.Init(typeof(Entry), SkinProfile, "local.meirinwatcherskin", "Initialized Meirin Watcher body, character-select, voice, and settings runtime.", 2, (Func<bool>)(() => SkinSettingsStore.DebugLogEnabled), (Func<CardModel, bool>)IsPresentationTargetCard, (Action)delegate
		{
			CharacterSettingsUiRegistry.Register((ISkinSettingsUiRuntime)(object)SettingsUiRuntime, (ISkinSettingsGlobalUiRuntime)(object)SettingsGlobalRuntime);
			SharedSupportPresentation.Register(SkinProfile.ProfileId, (Func<Creature, bool>)((Creature c) => SkinProfile.AppliesTo(c.Player)), (Func<ICharacterSkinAudioSettings>)(() => (ICharacterSkinAudioSettings)(object)SkinRuntimeSettings));
			BundledCardArt.Register();
			DisableBoninallPresentationPatches();
			ConfigureTransportEventSubscriptions();
			InitializeDefeatLifecycle();
			ConfigureRunStartPreload();
			ConfigureCombatCutInToggleHud();
		}, (Action)delegate
		{
			BundledCardArt.Unregister();
			TransportScope.Dispose();
			foreach (IDisposable transportEventSubscription in TransportEventSubscriptions)
			{
				transportEventSubscription.Dispose();
			}
			TransportEventSubscriptions.Clear();
		});
	}

	private static bool IsTargetPlayer(Player? player)
	{
		return SkinProfile.AppliesTo(player);
	}

	private static bool IsPresentationTargetCard(CardModel? card)
	{
		if (((card != null) ? card.Owner : null) != null && IsTargetPlayer(card.Owner))
		{
			return CombatAudience.Allows(card.Owner, (CombatPresentationChannel)0);
		}
		return false;
	}

	private static bool IsLocalPlayer(Player? player)
	{
		if (player != null)
		{
			return LocalContext.IsMe(player);
		}
		return false;
	}

	private static void TryPlayNativeBodyVoice(Player player, string triggerName)
	{
		if (SkinProfile.Features.CombatVoiceHooks)
		{
			IReadOnlyList<string> readOnlyList = (string.Equals(triggerName, "Attack", StringComparison.OrdinalIgnoreCase) ? (IsU2AttackRouteReserved(player) ? MeirinCombatPresentationAssets.U2AttackVoiceCandidates : MeirinCombatPresentationAssets.AttackVoiceCandidates) : (string.Equals(triggerName, "heavyAttack", StringComparison.OrdinalIgnoreCase) ? MeirinCombatPresentationAssets.AttackVoiceCandidates : (string.Equals(triggerName, "Cast", StringComparison.OrdinalIgnoreCase) ? MeirinCombatPresentationAssets.DefenseVoiceCandidates : ((!string.Equals(triggerName, "PowerUp", StringComparison.OrdinalIgnoreCase)) ? null : MeirinCombatPresentationAssets.BuffVoiceCandidates))));
			IReadOnlyList<string> readOnlyList2 = readOnlyList;
			if (readOnlyList2 != null)
			{
				VoicePlayer.PlayRandomFor(player, readOnlyList2, 1f);
			}
		}
	}

	private static IReadOnlyList<string> ResolveAnimationSequence(Player player, CharacterSkinRemoteNativeBodyAnimation mapping)
	{
		IReadOnlyList<string> animationSequence;
		if (string.Equals(((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationName, "attack_ready", StringComparison.Ordinal))
		{
			animationSequence = ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationSequence;
			if (animationSequence != null && animationSequence.Count > 0)
			{
				if (TryConsumeAttackRouteOverride(player, out string route) && string.Equals(route, "u2_attack", StringComparison.Ordinal))
				{
					return new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "u2_attack_ready", "u2_attack_play", "u2_attack_end" });
				}
				int num = ConsumeNormalAttackVariant(player);
				return new _003C_003Ez__ReadOnlyArray<string>(new string[3]
				{
					"attack_ready",
					(num == 1) ? "attack_play1" : "attack_play2",
					"attack_end"
				});
			}
		}
		animationSequence = ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationSequence;
		if (animationSequence != null && animationSequence.Count > 0)
		{
			return ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationSequence;
		}
		if (!string.IsNullOrWhiteSpace(((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationName))
		{
			return new string[1] { ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).AnimationName };
		}
		return Array.Empty<string>();
	}

	private static bool ValidateAnimations(NCreature creatureNode, IReadOnlyList<string> sequence)
	{
		foreach (string item in sequence)
		{
			if (!CombatBodySurface.HasAnimation(creatureNode, item, "Meirin native body trigger"))
			{
				return false;
			}
		}
		return sequence.Count > 0;
	}

	private static void PlayAnimationSequence(NCreature creatureNode, IReadOnlyList<string> sequence, CharacterSkinRemoteNativeBodyAnimation mapping)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = sequence.Count == 1 && ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).Loop;
		SpineAnimationCompat.SetAnimation(creatureNode.SpineAnimation, sequence[0], flag, 0);
		for (int i = 1; i < sequence.Count; i++)
		{
			bool flag2 = i == sequence.Count - 1 && ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).Loop;
			SpineAnimationCompat.AddAnimation(creatureNode.SpineAnimation, sequence[i], 0f, flag2, 0);
		}
		if (!((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).Loop && ((CharacterSkinRemoteNativeBodyAnimation)(ref mapping)).RestoreIdle)
		{
			SpineAnimationCompat.AddAnimation(creatureNode.SpineAnimation, SkinProfile.CombatBody.ReadyExitAnimation, 0f, false, 0);
			SpineAnimationCompat.AddAnimation(creatureNode.SpineAnimation, SkinProfile.CombatBody.IdleAnimation, 0f, true, 0);
		}
	}

	private static Task WaitForHandledTrigger(string triggerName, IReadOnlyList<string> sequence, float nativeWaitTime)
	{
		if (!SkinRuntimeSettings.OfficialAttackTimingEnabled)
		{
			float num = MathF.Max(0f, nativeWaitTime);
			return Cmd.CustomScaledWait(MathF.Min(num * 0.5f, 0.25f), num, false, default(CancellationToken));
		}
		if (sequence.Count > 1 && string.Equals(sequence[1], "u2_attack_play", StringComparison.Ordinal))
		{
			return Cmd.CustomScaledWait(0.46600002f, 0.46600002f, false, default(CancellationToken));
		}
		float num2 = (string.Equals(triggerName, "Attack", StringComparison.OrdinalIgnoreCase) ? 0.266f : ((!string.Equals(triggerName, "heavyAttack", StringComparison.OrdinalIgnoreCase)) ? MathF.Max(0f, nativeWaitTime) : 0.299f));
		return Cmd.CustomScaledWait(num2, num2, false, default(CancellationToken));
	}

	private static Task WaitForSuppressedSupportTrigger(string route, float nativeWaitTime)
	{
		float num = route switch
		{
			"defense" => 0.4667f, 
			"debuff" => 0.4667f, 
			"u1_buff" => 0.4667f, 
			"u3_buff" => 0.8827f, 
			"u4_buff" => 0.6827f, 
			_ => MathF.Max(0f, nativeWaitTime), 
		};
		return Cmd.CustomScaledWait(MathF.Min(num * 0.5f, 0.25f), num, false, default(CancellationToken));
	}

	private static void InitializeDefeatLifecycle()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		if (DefeatLifecycle == null)
		{
			DefeatLifecycle = new CharacterDefeatLifecycle("MeirinWatcherSkin", (Func<Player, bool>)((Player player) => SkinProfile.AppliesTo(player)), (Func<SkinCharacterSettings>)(() => SkinRuntimeSettings), "res://MeirinWatcherSkin/scenes/defeat_vacant_overlay.tscn", 1.1f, "res://MeirinWatcherSkin/audio/voice/vo_1027_lose_01.ogg", "res://MeirinWatcherSkin/audio/voice/vo_1027_stage_fail_01.ogg", (Action<string>)delegate(string message)
			{
				SkinLog.Warn(message);
			});
		}
	}

	private static void ApplyIdentityTextureResult(CharacterModel character, string path, ref Texture2D result)
	{
		if (SkinProfile.AppliesTo(character))
		{
			Texture2D val = LoadIdentityTexture(path);
			if (val != null)
			{
				result = val;
			}
		}
	}

	private static Texture2D? LoadIdentityTexture(string path)
	{
		Texture2D obj = ResourceLoader.Load<Texture2D>(path, (string)null, (CacheMode)1);
		if (obj == null)
		{
			SkinLog.InfoOnce("meirin_identity_icon_missing_" + path, "Meirin character identity icon could not be loaded: " + path);
		}
		return obj;
	}

	private static void ApplyIdentityTexture(Node node, Texture2D texture)
	{
		TextureRect val = (TextureRect)(object)((node is TextureRect) ? node : null);
		if (val != null)
		{
			val.Texture = texture;
		}
		foreach (Node child in node.GetChildren(false))
		{
			ApplyIdentityTexture(child, texture);
		}
	}

	private static bool TryBeginMeirinMultiHitPresentation(AttackCommand command, CardModel card, NCreature actor, int hitCount, int token)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		Player owner = card.Owner;
		Creature attacker = command.Attacker;
		bool flag = (attacker != null && attacker.HasPower<Wrath>()) || (attacker != null && attacker.HasPower<Divinity>());
		IReadOnlyList<string> sequence = (flag ? MultiHitU2Animations : MultiHitStrongAnimations);
		if (owner != null && hitCount >= 2)
		{
			Creature attacker2 = command.Attacker;
			if (((attacker2 != null) ? attacker2.Player : null) == owner && !(MultiHitBeforeDamageField == null) && CombatBodySurface.Ensure(actor, "multi_hit") && ValidateAnimations(actor, sequence))
			{
				Func<Task> originalBeforeDamage = MultiHitBeforeDamageField.GetValue(command) as Func<Task>;
				SpineAnimationCompat.SetAnimation(actor.SpineAnimation, "attack_ready", false, 0);
				SuppressNativeAttackPresentation(command);
				InstallMeirinMultiHitHooks(command, card, originalBeforeDamage);
				MultiHitState state = new MultiHitState(command, card, actor, hitCount, token, Time.GetTicksMsec(), SkinRuntimeSettings.OfficialAttackTimingEnabled, flag);
				MultiHitStates.Remove(owner);
				MultiHitStates.Add(owner, state);
				BeginMeirinBodyReadyAction(card);
				PlayMeirinMultiHitReadySfx(actor, token);
				SkinLog.Debug(() => "Started Meirin multi-hit hits=" + hitCount + " finisher=" + state.FinisherCueName);
				return true;
			}
		}
		return false;
	}

	private static void InstallMeirinMultiHitHooks(AttackCommand command, CardModel card, Func<Task>? originalBeforeDamage)
	{
		command.WithHitVfxNode((Func<Creature, Node2D>)delegate(Creature target)
		{
			PlayMeirinMultiHitResolvedHit(card, target);
			return (Node2D?)null;
		});
		command.BeforeDamage((Func<Task>)async delegate
		{
			if (originalBeforeDamage != null)
			{
				await originalBeforeDamage();
			}
			await WaitForMeirinMultiHitDamage(card);
		});
	}

	private static bool TryHandleMeirinMultiHitTrigger(Creature creature, Player player, string triggerName, float nativeWaitTime, out Task handledTask)
	{
		handledTask = Task.CompletedTask;
		if (!string.Equals(triggerName, "Attack", StringComparison.OrdinalIgnoreCase) || !MultiHitStates.TryGetValue(player, out MultiHitState state) || !state.Matches(player, creature) || !IsMeirinMultiHitStateCurrent(state) || !state.TryQueueNextHit(Time.GetTicksMsec(), out var beat, out var remainingSeconds))
		{
			return false;
		}
		if (beat.HitIndex == 0 && !state.UsesU2Finisher && SkinProfile.Features.CombatVoiceHooks)
		{
			VoicePlayer.PlayRandomFor(player, MeirinCombatPresentationAssets.AttackVoiceCandidates, 1f);
			state.MarkVoicePlayed();
		}
		handledTask = ((remainingSeconds <= 0.001f) ? Task.CompletedTask : Cmd.CustomScaledWait(remainingSeconds, remainingSeconds, false, default(CancellationToken)));
		SkinLog.Debug(() => "Meirin multi-hit target-resolution gate hit=" + (beat.HitIndex + 1) + "/" + state.HitCount + " animation=" + beat.CueName + " nativeWait=" + nativeWaitTime.ToString("0.###") + " remaining=" + remainingSeconds.ToString("0.###"));
		return true;
	}

	private static void PlayMeirinMultiHitResolvedHit(CardModel card, Creature target)
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetMeirinMultiHitState(card, out MultiHitState state) || !state.TryResolveNextHit(out var beat))
		{
			return;
		}
		NCreature actor = state.Actor;
		NCreature val = CombatCreatureNodes.Resolve(NCombatRoom.Instance, target, "Meirin multi-hit target lookup failed", (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		});
		if (val != null)
		{
			TryBeginMeirinResolvedTargetMeleeApproach(actor, val, beat.CueName);
		}
		if (beat.IsFinal)
		{
			SpineAnimationCompat.SetAnimation(actor.SpineAnimation, beat.FinalReadyAnimation, false, 0);
			SpineAnimationCompat.AddAnimation(actor.SpineAnimation, beat.CueName, 0f, false, 0);
			QueueMeirinMultiHitEnd(actor, beat.FinalEndAnimation);
			state.MarkEndQueued();
			if (beat.UsesU2Finisher && !state.HasPlayedVoice && SkinProfile.Features.CombatVoiceHooks)
			{
				VoicePlayer.PlayRandomFor(state.Card.Owner, MeirinCombatPresentationAssets.U2AttackVoiceCandidates, 1f);
				state.MarkVoicePlayed();
			}
		}
		else
		{
			SpineAnimationCompat.SetAnimation(actor.SpineAnimation, beat.CueName, false, 0);
		}
		state.MarkImpactStarted(Time.GetTicksMsec(), beat.DamageDelaySeconds);
		Creature entity = actor.Entity;
		Player val2 = ((entity != null) ? entity.Player : null);
		int num;
		if (PresentationGates.CombatPresentationEffectsEnabled)
		{
			num = (CombatAudience.Allows(val2, (CombatPresentationChannel)1) ? 1 : 0);
			if (num != 0)
			{
				PlayMeirinMultiHitCues(actor, val, beat.CueName, state.Token);
			}
		}
		else
		{
			num = 0;
		}
		if (PresentationGates.CombatPresentationSfxEnabled && CombatAudience.Allows(val2, (CombatPresentationChannel)2))
		{
			PlayMeirinMultiHitPlaySfx(actor, beat.CueName, state.Token);
		}
		if (num != 0)
		{
			object targetNodes;
			if (val != null)
			{
				IReadOnlyList<NCreature> readOnlyList = new _003C_003Ez__ReadOnlySingleElementList<NCreature>(val);
				targetNodes = readOnlyList;
			}
			else
			{
				targetNodes = null;
			}
			PlayAttackCameraShake(actor, (IReadOnlyList<NCreature>?)targetNodes, beat.CueName, beat.DamageDelaySeconds);
		}
	}

	private static async Task WaitForMeirinMultiHitDamage(CardModel card)
	{
		if (TryGetMeirinMultiHitState(card, out MultiHitState state))
		{
			float num = state.ConsumeDamageWait(Time.GetTicksMsec());
			if (num > 0.001f)
			{
				await Cmd.CustomScaledWait(num, num, false, default(CancellationToken));
			}
		}
	}

	private static bool TryGetMeirinMultiHitState(CardModel card, out MultiHitState state)
	{
		Player owner = card.Owner;
		if (owner != null && MultiHitStates.TryGetValue(owner, out MultiHitState value) && value.Matches(card) && IsMeirinMultiHitStateCurrent(value))
		{
			state = value;
			return true;
		}
		state = null;
		return false;
	}

	private static bool IsMeirinMultiHitStateCurrent(MultiHitState state)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)state.Actor) && CombatEffectTokens.TryGetValue(state.Actor, out MeirinPresentationTokenState value))
		{
			return value.IsCurrent(state.Token);
		}
		return false;
	}

	private static bool HasActiveMeirinMultiHitPresentation(Player player)
	{
		if (MultiHitStates.TryGetValue(player, out MultiHitState value))
		{
			return IsMeirinMultiHitStateCurrent(value);
		}
		return false;
	}

	private static void QueueMeirinMultiHitEnd(NCreature actor, string endAnimation)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		BindMeirinBodyRecovery(actor);
		SpineAnimationCompat.AddAnimation(actor.SpineAnimation, endAnimation, 0f, false, 0);
		SpineAnimationCompat.AddAnimation(actor.SpineAnimation, SkinProfile.CombatBody.ReadyLoopAnimation, 0f, true, 0);
	}

	private static void PlayMeirinMultiHitCues(NCreature actor, NCreature? target, string cueName, int token)
	{
		MultiHitEffectCueSet multiHitEffectCueSet = MultiHitCues.Value.ResolveEffects(cueName);
		IReadOnlyList<CombatPresentationEffectCue> readOnlyList = ((target == null) ? multiHitEffectCueSet.OwnerOnly : multiHitEffectCueSet.All);
		CombatEffectRoutes.PlayCues(actor, target, "multi_hit:" + cueName, readOnlyList, token);
	}

	private static void PlayMeirinMultiHitReadySfx(NCreature actor, int token)
	{
		CombatSfxCues.Play(actor, MultiHitCues.Value.ReadySfx, token);
	}

	private static void PlayMeirinMultiHitPlaySfx(NCreature actor, string cueName, int token)
	{
		CombatSfxCues.Play(actor, MultiHitCues.Value.ResolvePlaySfx(cueName), token);
	}

	private static MultiHitCueCache BuildMultiHitCueCache()
	{
		return new MultiHitCueCache(BuildMultiHitEffectCueSet("attack_play1_combo", 0f), BuildMultiHitEffectCueSet("attack_play2_combo", 0f), BuildMultiHitEffectCueSet("f_strong_attack_play", 0f), BuildMultiHitEffectCueSet("u2_attack_sequence", 0f), BuildMultiHitReadySfxCues("attack_play1"), GetMultiHitSfxCues("attack_play1_combo"), GetMultiHitSfxCues("attack_play2_combo"), GetMultiHitSfxCues("f_strong_attack_play"), GetMultiHitSfxCues("u2_attack_play"));
	}

	private static MultiHitEffectCueSet BuildMultiHitEffectCueSet(string routeName, float startDelayOffsetSeconds)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<CombatPresentationEffectCue> readOnlyList = default(IReadOnlyList<CombatPresentationEffectCue>);
		if (!SkinProfile.CombatPresentation.TryGetTransientEffects(routeName, ref readOnlyList))
		{
			return MultiHitEffectCueSet.Empty;
		}
		List<CombatPresentationEffectCue> list = new List<CombatPresentationEffectCue>(readOnlyList.Count);
		List<CombatPresentationEffectCue> list2 = new List<CombatPresentationEffectCue>(readOnlyList.Count);
		foreach (CombatPresentationEffectCue item in readOnlyList)
		{
			CombatPresentationEffectCue obj = item._003CClone_003E_0024();
			obj.set_StartDelaySeconds(Math.Max(0f, item.StartDelaySeconds - startDelayOffsetSeconds));
			CombatPresentationEffectCue val = obj;
			list.Add(val);
			if ((int)val.Anchor == 0)
			{
				list2.Add(val);
			}
		}
		return new MultiHitEffectCueSet(list.ToArray(), list2.ToArray());
	}

	private static IReadOnlyList<CombatPresentationSfxCue> BuildMultiHitReadySfxCues(string cueName)
	{
		IReadOnlyList<CombatPresentationSfxCue> readOnlyList = default(IReadOnlyList<CombatPresentationSfxCue>);
		if (!SkinProfile.CombatPresentation.TryGetSfxCues(cueName, ref readOnlyList))
		{
			return Array.Empty<CombatPresentationSfxCue>();
		}
		List<CombatPresentationSfxCue> list = new List<CombatPresentationSfxCue>(readOnlyList.Count);
		foreach (CombatPresentationSfxCue item in readOnlyList)
		{
			if (!(item.DelaySeconds >= 0.199f))
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	private static IReadOnlyList<CombatPresentationSfxCue> GetMultiHitSfxCues(string cueName)
	{
		IReadOnlyList<CombatPresentationSfxCue> result = default(IReadOnlyList<CombatPresentationSfxCue>);
		if (!SkinProfile.CombatPresentation.TryGetSfxCues(cueName, ref result))
		{
			return Array.Empty<CombatPresentationSfxCue>();
		}
		return result;
	}

	private static void ClearMeirinMultiHitPresentations(IEnumerable<Player>? players, bool restoreBody)
	{
		if (players == null)
		{
			return;
		}
		foreach (Player player in players)
		{
			ClearMeirinMultiHitPresentation(player, restoreBody);
		}
	}

	private static void ClearMeirinMultiHitPresentation(Player player, bool restoreBody)
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (!MultiHitStates.TryGetValue(player, out MultiHitState value))
		{
			return;
		}
		MultiHitStates.Remove(player);
		if (!GodotObject.IsInstanceValid((GodotObject)(object)value.Actor))
		{
			return;
		}
		RestoreMeirinMeleeMotionForActor(value.Actor);
		if (restoreBody)
		{
			Creature entity = value.Actor.Entity;
			if (entity != null && entity.IsAlive && CombatBodySurface.Ensure(value.Actor, "multi_hit_cleanup") && CombatBodySurface.HasAnimation(value.Actor, SkinProfile.CombatBody.IdleAnimation, "Meirin multi-hit cleanup idle"))
			{
				SpineAnimationCompat.SetAnimation(value.Actor.SpineAnimation, SkinProfile.CombatBody.IdleAnimation, true, 0);
			}
		}
	}

	private static void ConfigureRunStartPreload()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		CombatEffectEngine.Register(SkinProfile, "Meirin", (Func<IReadOnlyList<RunStartPreloadItem>>)BuildMeirinRunStartPreloadItems, (RunStartPreloadStep[])(object)new RunStartPreloadStep[4]
		{
			RunStartPreloadStep.Action(0.945f, "Preparing melee step effects", (Action)PrewarmMeirinMeleeStepEffects, (Func<bool>)(() => PresentationGates.CombatPresentationEffectsEnabled), true),
			new RunStartPreloadStep(0.975f, "Preparing entry animation", (Func<Node, Task>)PrewarmMeirinCombatEntryForRunStart, (Func<bool>)(() => PresentationGates.CombatBodyPresentationEnabled), true),
			RunStartPreloadStep.Action(0.985f, "Preparing card-play overlay", (Action)BattleReadyOverlay.Preload, (Func<bool>)(() => PresentationGates.BattleReadyOverlayDisplayEnabled), true),
			RunStartPreloadStep.Action(0.99f, "Preparing voice", (Action)delegate
			{
				VoicePlayer.Prewarm(SkinProfile.Voice.PrewarmPaths);
			}, (Func<bool>)(() => SkinRuntimeSettings.EffectiveVoiceEnabled), true)
		});
	}

	private static RunStartPreloadItem[] BuildMeirinRunStartPreloadItems()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_003f: Expected O, but got Unknown
		RunStartPreloadItemBuilder val = new RunStartPreloadItemBuilder();
		RunStartPreloadCommonItemSources val2 = new RunStartPreloadCommonItemSources(SkinProfile, (Func<bool>)(() => PresentationGates.CombatPresentationEffectsEnabled));
		val2.set_Policy(RunStartPreloadCommonItemPolicy.BodyOnly);
		RunStartPreloadCommonItems.Add(val, val2);
		return val.ToArray();
	}

	private static Task PrewarmMeirinCombatEntryForRunStart(Node host)
	{
		return RunStartCombatPrewarm.WarmEntryBodyAsync(host, "Meirin", SkinProfile, (Func<bool>)(() => PresentationGates.CombatBodyPresentationEnabled));
	}

	private static bool TryInitializeRestSiteInteraction(NRestSiteCharacter restSiteCharacter)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		Control nodeOrNull = ((Node)restSiteCharacter).GetNodeOrNull<Control>(NodePath.op_Implicit("ControlRoot"));
		Control nodeOrNull2 = ((Node)restSiteCharacter).GetNodeOrNull<Control>(NodePath.op_Implicit("%Hitbox"));
		NSelectionReticle nodeOrNull3 = ((Node)restSiteCharacter).GetNodeOrNull<NSelectionReticle>(NodePath.op_Implicit("%SelectionReticle"));
		Control nodeOrNull4 = ((Node)restSiteCharacter).GetNodeOrNull<Control>(NodePath.op_Implicit("%ThoughtBubbleLeft"));
		Control nodeOrNull5 = ((Node)restSiteCharacter).GetNodeOrNull<Control>(NodePath.op_Implicit("%ThoughtBubbleRight"));
		if (nodeOrNull == null || nodeOrNull2 == null || nodeOrNull3 == null || nodeOrNull4 == null || nodeOrNull5 == null)
		{
			SkinLog.Warn("Meirin rest-site interaction nodes are incomplete; using native initialization.");
			return false;
		}
		NRestSiteCharacter obj = restSiteCharacter;
		StringName controlRoot = PropertyName._controlRoot;
		GodotObject val = (GodotObject)(object)nodeOrNull;
		((GodotObject)obj).Set(controlRoot, Variant.From<GodotObject>(ref val));
		NRestSiteCharacter obj2 = restSiteCharacter;
		StringName hitbox = PropertyName.Hitbox;
		val = (GodotObject)(object)nodeOrNull2;
		((GodotObject)obj2).Set(hitbox, Variant.From<GodotObject>(ref val));
		NRestSiteCharacter obj3 = restSiteCharacter;
		StringName selectionReticle = PropertyName._selectionReticle;
		val = (GodotObject)(object)nodeOrNull3;
		((GodotObject)obj3).Set(selectionReticle, Variant.From<GodotObject>(ref val));
		NRestSiteCharacter obj4 = restSiteCharacter;
		StringName leftThoughtAnchor = PropertyName._leftThoughtAnchor;
		val = (GodotObject)(object)nodeOrNull4;
		((GodotObject)obj4).Set(leftThoughtAnchor, Variant.From<GodotObject>(ref val));
		NRestSiteCharacter obj5 = restSiteCharacter;
		StringName rightThoughtAnchor = PropertyName._rightThoughtAnchor;
		val = (GodotObject)(object)nodeOrNull5;
		((GodotObject)obj5).Set(rightThoughtAnchor, Variant.From<GodotObject>(ref val));
		Callable val2 = Callable.From<Variant>((Func<Variant>)(() => ((GodotObject)restSiteCharacter).Call(MethodName.OnFocus, Array.Empty<Variant>())));
		Callable val3 = Callable.From<Variant>((Func<Variant>)(() => ((GodotObject)restSiteCharacter).Call(MethodName.OnUnfocus, Array.Empty<Variant>())));
		((GodotObject)nodeOrNull2).Connect(SignalName.FocusEntered, val2, 0u);
		((GodotObject)nodeOrNull2).Connect(SignalName.FocusExited, val3, 0u);
		((GodotObject)nodeOrNull2).Connect(SignalName.MouseEntered, val2, 0u);
		((GodotObject)nodeOrNull2).Connect(SignalName.MouseExited, val3, 0u);
		return true;
	}

	private static bool IsMeirinMerchantSurface(NMerchantCharacter merchantCharacter)
	{
		if (IsTargetPlayer(ResolveMerchantCharacterPlayer(merchantCharacter)))
		{
			return true;
		}
		if (((Node)merchantCharacter).Name == StringName.op_Implicit("WatcherMerchant"))
		{
			return true;
		}
		return ((Node)merchantCharacter).SceneFilePath.Contains("watcher_merchant", StringComparison.OrdinalIgnoreCase);
	}

	private static Player? ResolveMerchantCharacterPlayer(NMerchantCharacter merchantCharacter)
	{
		NMerchantRoom instance = NMerchantRoom.Instance;
		List<Player> list = ((instance == null) ? null : Traverse.Create((object)instance).Field("_players").GetValue<List<Player>>());
		if (list == null)
		{
			return null;
		}
		int index = ((Node)merchantCharacter).GetIndex(false);
		if (index < 0 || index >= list.Count)
		{
			return null;
		}
		return list[index];
	}

	private static List<Node> FindSpineSprites(Node root)
	{
		return GodotNodeUtil.FindNodesByClass(root, "SpineSprite");
	}

	private static void ConfigureTransportEventSubscriptions()
	{
		BattleReadyCardPlayEvents.AddSubscriptions((ICollection<IDisposable>)TransportEventSubscriptions, "MeirinWatcherSkin", (Func<BattleReadyOverlayRuntime<CardModel>>)(() => BattleReadyOverlay), (Action<CardModel>)null);
		RemoteNativeBodyTriggers.AddCardReadyEventSubscriptions((ICollection<IDisposable>)TransportEventSubscriptions, "Meirin");
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<CardPlayEnqueuingEvent>(SkinProfile, "Meirin non-Watcher card enqueue presentation", (Action<CardPlayEnqueuingEvent>)delegate(CardPlayEnqueuingEvent gameEvent)
		{
			if (!(((CardPlayEnqueuingEvent)(ref gameEvent)).Card is WatcherCard))
			{
				TaskHelper.RunSafely(TryBeginMeirinSupportCardPresentation(((CardPlayEnqueuingEvent)(ref gameEvent)).Card, ((CardPlayEnqueuingEvent)(ref gameEvent)).Target));
			}
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<LocalCardPlayedEvent>(SkinProfile, "Meirin native played card hide", (Action<LocalCardPlayedEvent>)delegate(LocalCardPlayedEvent gameEvent)
		{
			QueuedCardPresentation.Queue(((LocalCardPlayedEvent)(ref gameEvent)).Card);
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<CardPlayingEvent>(SkinProfile, "Meirin teammate card playing", (Action<CardPlayingEvent>)delegate(CardPlayingEvent gameEvent)
		{
			Player player = ((CardPlayingEvent)(ref gameEvent)).CardPlay.Player;
			if (CombatAudience.IsTeammate(player) && CombatAudience.Allows(player, (CombatPresentationChannel)0))
			{
				QueuedCardPresentation.Queue(((CardPlayingEvent)(ref gameEvent)).CardPlay.Card);
				TryBeginMeirinSupportCardPresentation(((CardPlayingEvent)(ref gameEvent)).CardPlay.Card, ((CardPlayingEvent)(ref gameEvent)).CardPlay.Target);
			}
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<NativeBlockGainSfxReplacementRequest>(SkinProfile, "Meirin native block gain SFX replacement", (Action<NativeBlockGainSfxReplacementRequest>)BlockGainFeedback.RegisterNativeSfxReplacement));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<BlockActuallyIncreasedEvent>(SkinProfile, "Meirin block gain effect-only feedback", (Action<BlockActuallyIncreasedEvent>)BlockGainFeedback.Schedule));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<MultiplayerLoadScreenStartedEvent>(SkinProfile, "Meirin multiplayer load character-select background", (Action<MultiplayerLoadScreenStartedEvent>)delegate(MultiplayerLoadScreenStartedEvent gameEvent)
		{
			CharacterSelectRuntime.ApplyMultiplayerLoadBackground(((MultiplayerLoadScreenStartedEvent)(ref gameEvent)).Node);
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<CharacterSelectedEvent>(SkinProfile, "Meirin character-select presentation", (Action<CharacterSelectedEvent>)delegate(CharacterSelectedEvent gameEvent)
		{
			CharacterSelectRuntime.ApplyScreenSelection(((CharacterSelectedEvent)(ref gameEvent)).Screen, ((CharacterSelectedEvent)(ref gameEvent)).Button, ((CharacterSelectedEvent)(ref gameEvent)).Character);
			CharacterSelectRuntime.ApplyCombatPresentationModeDropdown(((CharacterSelectedEvent)(ref gameEvent)).Screen, ((CharacterSelectedEvent)(ref gameEvent)).Button, ((CharacterSelectedEvent)(ref gameEvent)).Character);
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<RandomCharacterResolvedEvent>(SkinProfile, "Meirin random character-select background", (Action<RandomCharacterResolvedEvent>)delegate(RandomCharacterResolvedEvent gameEvent)
		{
			CharacterSelectRuntime.ApplyRandomCharacterResolved(((RandomCharacterResolvedEvent)(ref gameEvent)).Screen, ((RandomCharacterResolvedEvent)(ref gameEvent)).Character);
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<CharacterSelectButtonSelectedEvent>(SkinProfile, "Meirin character-select voice", (Action<CharacterSelectButtonSelectedEvent>)delegate(CharacterSelectButtonSelectedEvent gameEvent)
		{
			CharacterSelectRuntime.PlaySelectedVoice(((CharacterSelectButtonSelectedEvent)(ref gameEvent)).Button);
		}));
		if (SkinProfile.Features.BattleReadyOverlay)
		{
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<HandCardFocusedEvent>(SkinProfile, "Meirin battle-ready hand focus", (Action<HandCardFocusedEvent>)delegate(HandCardFocusedEvent e)
			{
				ShowCardHoverPresentation(((NCardHolder)((HandCardFocusedEvent)(ref e)).Holder).CardModel);
			}));
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<HandCardUnfocusedEvent>(SkinProfile, "Meirin battle-ready hand unfocus", (Action<HandCardUnfocusedEvent>)delegate(HandCardUnfocusedEvent e)
			{
				EndCardHoverPresentation(((NCardHolder)((HandCardUnfocusedEvent)(ref e)).Holder).CardModel);
			}));
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<HandCardHoverEffectsEvent>(SkinProfile, "Meirin battle-ready hand hover", (Action<HandCardHoverEffectsEvent>)delegate(HandCardHoverEffectsEvent e)
			{
				CardModel cardModel = ((NCardHolder)((HandCardHoverEffectsEvent)(ref e)).Holder).CardModel;
				if (((HandCardHoverEffectsEvent)(ref e)).IsHovered)
				{
					ShowCardHoverPresentation(cardModel);
				}
				else
				{
					EndCardHoverPresentation(cardModel);
				}
			}));
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<HandCardMousePressedEvent>(SkinProfile, "Meirin battle-ready hand mouse press", (Action<HandCardMousePressedEvent>)delegate(HandCardMousePressedEvent e)
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Invalid comparison between Unknown and I8
				InputEvent input = ((HandCardMousePressedEvent)(ref e)).Input;
				InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
				if (val != null && (long)val.ButtonIndex == 1)
				{
					ShowCardHoverPresentation(((NCardHolder)((HandCardMousePressedEvent)(ref e)).Holder).CardModel);
				}
			}));
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<MouseCardPlayStartedEvent>(SkinProfile, "Meirin battle-ready mouse card play start", (Action<MouseCardPlayStartedEvent>)delegate(MouseCardPlayStartedEvent e)
			{
				NHandCardHolder holder = ((NCardPlay)((MouseCardPlayStartedEvent)(ref e)).Node).Holder;
				ShowCardHoverPresentation((holder != null) ? ((NCardHolder)holder).CardModel : null);
			}));
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<CardSelectionPresentationStartedEvent>(SkinProfile, "Meirin battle-ready card selection", (Action<CardSelectionPresentationStartedEvent>)delegate(CardSelectionPresentationStartedEvent e)
			{
				ShowCardSelectionPresentation(((CardSelectionPresentationStartedEvent)(ref e)).Card);
			}));
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<CardPlayCommittedEvent>(SkinProfile, "Meirin battle-ready card committed", (Action<CardPlayCommittedEvent>)delegate(CardPlayCommittedEvent e)
			{
				HoldMeirinBodyReadyForCardPlay(((CardPlayCommittedEvent)(ref e)).Card);
			}));
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<CardPlayCancelledEvent>(SkinProfile, "Meirin battle-ready cancel", (Action<CardPlayCancelledEvent>)delegate(CardPlayCancelledEvent e)
			{
				NHandCardHolder holder2 = ((CardPlayCancelledEvent)(ref e)).Node.Holder;
				CancelCardHoverPresentation((holder2 != null) ? ((NCardHolder)holder2).CardModel : null);
			}));
		}
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<RestSiteCharacterReadyEvent>(SkinProfile, "Meirin rest-site camp voice", (Action<RestSiteCharacterReadyEvent>)delegate(RestSiteCharacterReadyEvent gameEvent)
		{
			TryPlayRestSiteVoice(((RestSiteCharacterReadyEvent)(ref gameEvent)).Node.Player);
		}));
		if (!string.IsNullOrWhiteSpace(SkinProfile.Voice.RestSiteHealPath))
		{
			TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<RestSiteHealedEvent>(SkinProfile, "Meirin rest-site heal voice", (Action<RestSiteHealedEvent>)delegate(RestSiteHealedEvent gameEvent)
			{
				if (RestSiteHealVoice.TryPlay(((RestSiteHealedEvent)(ref gameEvent)).Player, ((RestSiteHealedEvent)(ref gameEvent)).IsMimicked, 1f))
				{
					SkinLog.Info("Played Meirin rest-site heal voice.");
				}
			}));
		}
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<RestSiteSmithedEvent>(SkinProfile, "Meirin rest-site smith growth voice", (Action<RestSiteSmithedEvent>)delegate(RestSiteSmithedEvent gameEvent)
		{
			GrowthVoice.TryPlay(((RestSiteSmithedEvent)(ref gameEvent)).Player, 1f);
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<RoomEnteringEvent>(SkinProfile, "Meirin merchant room growth voice", (Action<RoomEnteringEvent>)delegate(RoomEnteringEvent gameEvent)
		{
			if (SkinRunContext.IsMerchantRoomOrFakeMerchant(((RoomEnteringEvent)(ref gameEvent)).Room))
			{
				GrowthVoice.TryPlay(SkinRunContext.TryGetLocalPlayer(((RoomEnteringEvent)(ref gameEvent)).RunState), 1f);
			}
		}));
		TransportEventSubscriptions.Add(TransportScope.SubscribeOwned<PotionUsedEvent>(SkinProfile, "Meirin teammate potion voice", (Action<PotionUsedEvent>)delegate(PotionUsedEvent gameEvent)
		{
			TryPlayTeammatePotionVoice(((PotionUsedEvent)(ref gameEvent)).Potion, ((PotionUsedEvent)(ref gameEvent)).Target);
		}));
		CharacterContextVoiceSubscriptions.Add((ICollection<IDisposable>)TransportEventSubscriptions, SkinProfile.Voice, ContextVoice, "Meirin", (Action<string>)delegate(string message)
		{
			SkinLog.Info("Played Meirin " + message + ".");
		});
		ContextVoice.PrewarmContextCues();
		CombatDamageVoiceSubscriptions.Add((ICollection<IDisposable>)TransportEventSubscriptions, SkinProfile.Voice, CombatDamageVoice, "Meirin", (Action<string>)delegate(string message)
		{
			SkinLog.Info("Played Meirin " + message + ".");
		});
	}

	private static void TryPlayRestSiteVoice(Player? player)
	{
		if (SkinProfile.AppliesTo(player))
		{
			long tickCount = Environment.TickCount64;
			long num = Interlocked.Read(in _nextRestSiteVoiceTick);
			if (tickCount >= num && VoicePlayer.PlayLocalFor(player, RestSiteProfile.VoicePath, 1f))
			{
				Interlocked.Exchange(ref _nextRestSiteVoiceTick, tickCount + RestSiteProfile.VoiceCooldownMs);
			}
		}
	}

	private static void TryPlayTeammatePotionVoice(PotionModel potion, Creature? target)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Player owner = potion.Owner;
		Creature creature = owner.Creature;
		if (IsTargetPlayer(owner) && target != null && target.IsPlayer && target != creature && target.Side == creature.Side)
		{
			VoicePlayer.PlayRandomFor(owner, MeirinCombatPresentationAssets.PotionUseTeammateVoiceCandidates, 1f);
		}
	}

	private static Vector2 ResolveCurrentMeirinBodyDisplacement(Node2D spine)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (MeirinMeleeMotions.TryGetValue(spine, out MeirinMeleeMotionState value) && value.Active)
		{
			Node parent = ((Node)spine).GetParent();
			Node2D val = (Node2D)(object)((parent is Node2D) ? parent : null);
			if (val != null)
			{
				return spine.GlobalPosition - val.ToGlobal(value.OriginalPosition);
			}
		}
		return Vector2.Zero;
	}

	private static WatcherBodyVfxFollowScope? BeginWatcherBodyVfxFollowScope(Creature owner)
	{
		WatcherBodyVfxFollowScope activeWatcherBodyVfxFollowScope = ActiveWatcherBodyVfxFollowScope;
		if (!IsTargetPlayer(owner.Player))
		{
			return activeWatcherBodyVfxFollowScope;
		}
		CombatCreatureSpineResolver combatSpines = CombatSpines;
		NCombatRoom instance = NCombatRoom.Instance;
		Node2D val = combatSpines.Find((instance != null) ? instance.GetCreatureNode(owner) : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			ActiveWatcherBodyVfxFollowScope = new WatcherBodyVfxFollowScope(owner, val);
		}
		return activeWatcherBodyVfxFollowScope;
	}

	private static void DisableBoninallPresentationPatches()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		List<(MethodBase, MethodInfo)> list = new List<(MethodBase, MethodInfo)>();
		MethodBase[] array = Harmony.GetAllPatchedMethods().ToArray();
		foreach (MethodBase methodBase in array)
		{
			Patches patchInfo = Harmony.GetPatchInfo(methodBase);
			if (patchInfo == null)
			{
				continue;
			}
			foreach (Patch item in patchInfo.Prefixes.Concat(patchInfo.Postfixes).Concat(patchInfo.Transpilers).Concat(patchInfo.Finalizers))
			{
				string text = item.PatchMethod.DeclaringType?.FullName;
				if (string.Equals(item.owner, "boninall.watcher", StringComparison.Ordinal) && text != null && BoninallPresentationPatchTypes.Contains(text))
				{
					list.Add((methodBase, item.PatchMethod));
				}
			}
		}
		Harmony val = new Harmony("local.meirinwatcherskin");
		foreach (var (methodBase2, methodInfo) in list.Distinct())
		{
			val.Unpatch(methodBase2, methodInfo);
		}
		SkinLog.Info("Meirin presentation takeover disabled Boninall presentation patches=" + list.Count);
	}

	static Entry()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Expected O, but got Unknown
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Expected O, but got Unknown
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0463: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Expected O, but got Unknown
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Expected O, but got Unknown
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Expected O, but got Unknown
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Expected O, but got Unknown
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e0: Expected O, but got Unknown
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Expected O, but got Unknown
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Expected O, but got Unknown
		//IL_0517: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Expected O, but got Unknown
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Expected O, but got Unknown
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Expected O, but got Unknown
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Expected O, but got Unknown
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Expected O, but got Unknown
		//IL_05a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Expected O, but got Unknown
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Expected O, but got Unknown
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Expected O, but got Unknown
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Expected O, but got Unknown
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Expected O, but got Unknown
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Expected O, but got Unknown
		//IL_068c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0696: Expected O, but got Unknown
		//IL_06ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Expected O, but got Unknown
		//IL_06d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e2: Expected O, but got Unknown
		//IL_0767: Unknown result type (might be due to invalid IL or missing references)
		//IL_0771: Expected O, but got Unknown
		//IL_0795: Unknown result type (might be due to invalid IL or missing references)
		//IL_079f: Expected O, but got Unknown
		//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bb: Expected O, but got Unknown
		IReadOnlyList<CombatPresentationSfxCue> readOnlyList = default(IReadOnlyList<CombatPresentationSfxCue>);
		BlockGainFeedback = new CombatBlockGainFeedbackRuntime((Func<Player, bool>)((Player player) => CombatAudience.Allows(player, (CombatPresentationChannel)0)), (Func<Player, NCreature>)((Player player) => CombatCreatureNodes.ResolvePlayer(NCombatRoom.Instance, player, "Meirin block gain player node lookup failed", (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		})), (Action<NCreature, int>)delegate(NCreature actor, int token)
		{
			CombatEffectRoutes.PlayOwnerCues(actor, "block_gain_feedback", token);
		}, (Func<bool>)(() => PresentationGates.CombatPresentationSfxEnabled), (Func<IReadOnlyList<CombatPresentationSfxCue>>)(() => (!SkinProfile.CombatPresentation.TryGetSfxCues("block_gain_feedback", ref readOnlyList)) ? null : readOnlyList), (Func<string, float, bool>)((string path, float volume) => SfxPlayer.TryPlay(path, volume)), "Meirin block gain effect-only feedback", (Func<Player, bool>)((Player player) => CombatAudience.Allows(player, (CombatPresentationChannel)2)));
		SupportNativeTriggerGates = new ConditionalWeakTable<Player, SupportNativeTriggerGate>();
		CardCapabilityCache = new Dictionary<Type, MeirinCardCapabilities>();
		IlOpCodesByValue = BuildIlOpCodeTable();
		PowerApplyDeclaringTypeNames = new string[2] { "PowerCmd", "WatcherPowerCmdCompat" };
		StanceEntryMethodNames = new string[4] { "EnterWrath", "EnterCalm", "EnterForeseen", "ExitStance" };
		CharacterSelectPmaSpineNodeNames = new HashSet<string>(StringComparer.Ordinal) { "MeirinBgBack01", "MeirinBgBack02", "MeirinBgFront01" };
		MeirinBodyReadyPlaybackTokens = new ConditionalWeakTable<Player, MeirinPresentationTokenState>();
		MeirinBodyReadyActionTokens = new ConditionalWeakTable<Player, MeirinPresentationTokenState>();
		MeirinBodyReadyPresentation = new MeirinBodyReadyPresentationRuntime();
		MeirinBodyReadyControllerLazy = new Lazy<CombatPresentationController<Player>>((Func<CombatPresentationController<Player>>)(() => new CombatPresentationController<Player>(SkinProfile.CombatPresentation, (ICombatPresentationRuntime<Player>)(object)MeirinBodyReadyPresentation)));
		MeirinBodyReadyStatesLazy = new Lazy<CombatPresentationStatePlaybackRuntime<Player>>((Func<CombatPresentationStatePlaybackRuntime<Player>>)(() => new CombatPresentationStatePlaybackRuntime<Player>(SkinProfile.CombatPresentation, (ICombatPresentationRuntime<Player>)(object)MeirinBodyReadyPresentation, MeirinBodyReadyControllerLazy.Value, CombatEntryBodySequences, (ICombatHitResponseGuardHost<Player>)(object)new MeirinBodyReadyHitResponseGuardHost())));
		MeirinBodyReadyLazy = new Lazy<CombatBodyReadyPresentationRuntime<Player, CardModel>>((Func<CombatBodyReadyPresentationRuntime<Player, CardModel>>)(() => new CombatBodyReadyPresentationRuntime<Player, CardModel>(SkinProfile.CombatBody.ReadyExitGraceSeconds, (ICombatBodyReadyHost<Player, CardModel>)(object)new MeirinBodyReadyHost(), SkinProfile.CombatPresentation, MeirinBodyReadyStatesLazy.Value, CombatEntryBodySequences)));
		MeirinReadyIdleVoicesLazy = new Lazy<CombatReadyIdleVoiceRuntime<Player>>((Func<CombatReadyIdleVoiceRuntime<Player>>)(() => new CombatReadyIdleVoiceRuntime<Player>(SkinProfile.Voice.ReadyIdlePaths, (Func<Player, int?>)delegate(Player player)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			return (playerCombatState == null) ? ((int?)null) : new int?(playerCombatState.TurnNumber);
		}, (Func<Player, bool>)CanPlayMeirinReadyIdleVoice, (Func<Player, string, float, bool>)VoicePlayer.PlayFor, (Func<Player, object>)((Player player) => player.PlayerCombatState), 1f, 1f, 5f)));
		RemoteNativeBodyTriggersLazy = new Lazy<RemoteNativeBodyTriggerRuntime>((Func<RemoteNativeBodyTriggerRuntime>)(() => new RemoteNativeBodyTriggerRuntime(SkinProfile, CombatCreatureNodes, CombatBodySurface, (Func<Player, bool>)IsLocalPlayer, (Func<Player, bool>)((Player? player) => CombatAudience.Allows(player, (CombatPresentationChannel)0)), (Func<CombatBodyReadyPresentationRuntime<Player, CardModel>>)(() => MeirinBodyReady), (Action<NCreature, string, bool>)null, (Action<NCreature, string>)delegate(NCreature creatureNode, string _)
		{
			CombatEntryBodyVisibility.Reveal(creatureNode, "remote_entry_start");
		})));
		MeirinCinematicSupportTimings = new ConditionalWeakTable<Player, MeirinCinematicSupportTimingState>();
		CombatScreenEffectReferenceSize = new Vector2(1800f, 1080f);
		CombatEffectAttackTargets = new CombatAttackEffectTargetResolver();
		CombatEffectTokens = new ConditionalWeakTable<NCreature, MeirinPresentationTokenState>();
		NormalAttackVariants = new ConditionalWeakTable<Player, MeirinNormalAttackVariantState>();
		AttackRouteGates = new ConditionalWeakTable<Player, SupportNativeTriggerGate>();
		AttackCommandAttackerVfxField = AccessTools.Field(typeof(AttackCommand), "_attackerVfx");
		AttackCommandAttackerSfxField = AccessTools.Field(typeof(AttackCommand), "_attackerSfx");
		AttackCommandTmpAttackerSfxField = AccessTools.Field(typeof(AttackCommand), "_tmpAttackerSfx");
		AttackCommandHitVfxField = AccessTools.Field(typeof(AttackCommand), "<HitVfx>k__BackingField");
		AttackCommandHitSfxField = AccessTools.Field(typeof(AttackCommand), "<HitSfx>k__BackingField");
		AttackCommandTmpHitSfxField = AccessTools.Field(typeof(AttackCommand), "<TmpHitSfx>k__BackingField");
		AttackCommandCustomAttackerVfxNodesField = AccessTools.Field(typeof(AttackCommand), "_customAttackerVfxNodes");
		AttackCommandCustomHitVfxNodesField = AccessTools.Field(typeof(AttackCommand), "_customHitVfxNodes");
		AttackCommandAttackerAnimNameField = AccessTools.Field(typeof(AttackCommand), "_attackerAnimName");
		AttackCommandShouldPlayAnimationField = AccessTools.Field(typeof(AttackCommand), "_shouldPlayAnimation");
		CombatWarmupPolicyProfile obj = CombatWarmupPolicyProfile.LowPressureSpineWarmup((IReadOnlyList<string>)new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "attack_play1", "attack_play2" }), (IReadOnlyList<string>)Array.Empty<string>(), (IReadOnlyList<string>)new _003C_003Ez__ReadOnlyArray<string>(new string[3] { "meirin_", "common_hit_eff", "system_buff_shield" }), (LowPressureSpineWarmupTiming)null)._003CClone_003E_0024();
		obj.set_PrewarmAllCombatSfx(true);
		CombatWarmupPolicy = obj;
		CombatEffectEngineLazy = new Lazy<CharacterEffectEngine>((Func<CharacterEffectEngine>)delegate
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Expected O, but got Unknown
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Expected O, but got Unknown
			CharacterEffectEngineOptions val = new CharacterEffectEngineOptions(SkinProfile.ProfileId, (ICombatEffectCatalog)new PresentationEffectCatalog(SkinProfile.CombatPresentation), CombatWarmupPolicy, (ISkinTimingProbe)(object)Diagnostics, (Func<bool>)(() => PresentationGates.CombatPresentationEffectsEnabled), (Action<Node2D, CombatPresentationEffectCue>)StartCombatEffect);
			val.set_MetaKeyPrefix(SkinProfile.RuntimeMetaKeyPrefix);
			val.set_AttachLayers((Func<Node2D, Node, int>)AttachCombatEffectLayers);
			val.set_ApplyMaterial((Action<Node, CombatPresentationEffectCue>)ApplyCombatEffectMaterial);
			val.set_PoolCap((Func<CombatPresentationEffectCue, int>)((CombatPresentationEffectCue cue) => Math.Clamp((cue.MaxActiveInstances > 0) ? cue.MaxActiveInstances : 2, 1, 4)));
			val.set_SfxEnabled((Func<bool>)(() => PresentationGates.CombatPresentationSfxEnabled));
			val.set_PrewarmSfx((Func<string, bool>)((string path) => SfxPlayer.Prewarm(path)));
			val.set_ShouldDeferWarmup((Func<bool>)(() => Diagnostics.ShouldDeferWarmup()));
			return new CharacterEffectEngine(val);
		});
		CombatEffectCuesLazy = new Lazy<CombatTransientEffectCueRuntime<NCreature>>((Func<CombatTransientEffectCueRuntime<NCreature>>)(() => new CombatTransientEffectCueRuntime<NCreature>((ICombatTransientEffectCueHost<NCreature>)(object)new EntryCombatEffectCueHost(), (ISkinTimingProbe)(object)Diagnostics, (Action<string, int>)SkinLog.Warn)));
		CombatEffectRoutesLazy = new Lazy<CombatTransientEffectCueRouter<NCreature, NCreature>>((Func<CombatTransientEffectCueRouter<NCreature, NCreature>>)(() => new CombatTransientEffectCueRouter<NCreature, NCreature>(SkinProfile.CombatPresentation, (ICombatTransientEffectCueRouterHost<NCreature, NCreature>)(object)new EntryCombatEffectRouterHost(), (Action<string, int>)SkinLog.Warn)));
		CombatSfxCuesLazy = new Lazy<CombatSfxCueRuntime<NCreature>>((Func<CombatSfxCueRuntime<NCreature>>)(() => new CombatSfxCueRuntime<NCreature>((ICombatSfxCueHost<NCreature>)(object)new EntryCombatSfxCueHost(), "Meirin combat SFX scheduling failed: ")));
		PendingCombatEntryPlayers = new HashSet<Player>();
		CombatEntryBodySequencesLazy = new Lazy<CombatBodySequenceRuntime>((Func<CombatBodySequenceRuntime>)(() => new CombatBodySequenceRuntime(SkinProfile.CombatPresentation, SkinProfile.CombatBody.ReadyExitAnimation, 0f)));
		MeirinMeleeApproachStepOfficialOffset = new Vector2(-50f, 0f);
		MeirinMeleeReturnStepSts2Offset = new Vector2(-330f, 0f);
		MeirinMeleeMotions = new ConditionalWeakTable<Node2D, MeirinMeleeMotionState>();
		ActiveMeirinMeleeSpines = new HashSet<Node2D>();
		ActiveMeirinMeleeStepEffects = new HashSet<Node2D>();
		SkinProfile = MeirinSkinProfile.Profile;
		SkinSettingsStore = SkinSettings.Store;
		SkinRuntimeSettings = SkinSettings.ForCharacter(SkinProfile.ProfileId);
		PresentationGates = new CharacterSkinPresentationGates(SkinProfile.Features, (ICharacterSkinPresentationSettings)(object)SkinRuntimeSettings);
		CombatAudience = new CombatPresentationAudiencePolicy((ICombatPresentationAudienceSettings)(object)SkinRuntimeSettings, (Func<Player, bool>)IsTargetPlayer);
		Diagnostics = new SkinDiagnosticsRuntime((Func<bool>)(() => false), CombatWarmupPolicyProfile.Default.TimingProbeThresholdMs, 8);
		VoicePlayer = new CharacterSkinVoicePlayerRuntime((ICharacterSkinAudioSettings)(object)SkinRuntimeSettings, "MeirinWatcherVoicePlayer", 8f);
		SfxPlayer = new CharacterSkinSfxPlayerRuntime((ICharacterSkinAudioSettings)(object)SkinRuntimeSettings, "MeirinWatcherSfxPlayer", 1f, 2);
		GrowthVoice = new CharacterGrowthVoiceRuntime(SkinProfile, VoicePlayer);
		RestSiteHealVoice = new CharacterRestSiteHealVoiceRuntime(SkinProfile, VoicePlayer);
		ContextCutIns = new CharacterContextCutInRuntime(MeirinContextCutInProfile.Profiles, (ICharacterContextCutInHost)(object)new EntryCharacterContextCutInHost(), "Meirin context cut-in");
		ContextVoice = new CharacterContextVoiceRuntime(SkinProfile, VoicePlayer, SfxPlayer, ContextCutIns);
		CombatDamageVoice = new CombatDamageVoiceRuntime(SkinProfile, VoicePlayer, (Func<Player, bool>)IsTargetPlayer);
		CombatDeathSequence = new CombatDeathSequenceRuntime(SkinProfile);
		CharacterSelectRuntime = new CharacterSelectPresentationRuntime(SkinProfile, SkinRuntimeSettings, VoicePlayer, new CharacterSelectPresentationRuntimeOptions((ILocalizedText)(object)UiText.SwitchCharacterBackground, UiText.SettingsUi, (Func<Node, string, bool>)TryPrepareCharacterSelectSpine, (Func<Player>)CharacterSkinVoicePlayerRuntime.GetLocalRunPlayer, (Func<IReadOnlyList<CharacterSelectMenuSectionDescriptor>>)null));
		TransportEventSubscriptions = new List<IDisposable>();
		RestSiteProfile = SkinProfile.RestSite;
		CombatCreatureNodes = new CombatCreatureNodeResolver();
		CombatSpines = new CombatCreatureSpineResolver();
		CombatVisualAnchors = new CombatCreatureVisualAnchorResolver(CombatSpines);
		CombatEntryBodyVisibility = new CombatEntryBodyVisibilityRuntime(SkinProfile, CombatSpines, (Func<Player, bool>)IsTargetPlayer, (Func<bool>)(() => PresentationGates.CombatBodyPresentationEnabled), (Action<string>)delegate(string message)
		{
			SkinLog.Warn("Meirin " + message);
		}, (Action<string>)delegate(string message)
		{
			SkinLog.Debug(message);
		}, 2f);
		CombatBodySurface = new CombatBodySurfaceCoordinator(SkinProfile, CombatSpines, (Action<NCreature>)OnCombatBodySurfaceApplied, (Action<NCreature>)null, (Func<IEnumerable<string>>)null, (Action<string>)delegate(string message)
		{
			SkinLog.Warn(message);
		}, (Action<string>)delegate(string message)
		{
			SkinLog.Debug(message);
		});
		RestSitePresentation = new RestSitePresentationRuntime(RestSiteProfile, (IRestSitePresentationRuntimeContext)(object)new EntryRestSitePresentationRuntimeContext());
		MerchantPresentation = new MerchantPresentationRuntime(SkinProfile, CharacterSkinSurfaceDefaults.CznMerchantSurface, (Func<Node, IReadOnlyList<Node>>)FindSpineSprites);
		CombatCardContexts = new CombatCardContextBuilder(SkinProfile.CardPresentation, (Func<Player, bool>)IsTargetPlayer, (Func<Player, bool>)IsLocalPlayer);
		NativePlayedCardVisibility = CombatPlayedCardPresentationRuntimes.CreateNativeCardVisibility(SkinProfile.RuntimeMetaKeyPrefix, (Func<CardModel, bool>)IsPresentationTargetCard, (Action<string, int>)delegate(string message, int priority)
		{
			SkinLog.Warn(message, priority);
		});
		QueuedCardPresentation = CombatPlayedCardPresentationRuntimes.CreateQueuedNativeCardHideOnly(NativePlayedCardVisibility, (Func<CardModel, bool>)IsPresentationTargetCard, (Func<CardModel, bool>)((CardModel? card) => PresentationGates.CardPlayPresentationEnabled && CombatAudience.Allows(card, (CombatPresentationChannel)0)), (Func<CardModel, bool>)((CardModel? card) => CombatAudience.Allows(card, (CombatPresentationChannel)4)));
		BattleReadyOverlayLazy = new Lazy<BattleReadyOverlayRuntime<CardModel>>((Func<BattleReadyOverlayRuntime<CardModel>>)(() => new BattleReadyOverlayRuntime<CardModel>(SkinProfile.BattleReady, (IBattleReadyOverlayRuntimeContext<CardModel>)(object)new EntryBattleReadyOverlayRuntimeContext())));
		DefeatOverlayGate = new TimedDuplicateSuppressor();
		EffectMaterials = new CombatEffectMaterialRuntime(SkinProfile.PremultipliedAlphaMaterialPath, SkinProfile.CombatEffects.MaterialPolicy, 1f, (ICombatEffectMaterialHost)(object)new EntryCombatEffectMaterialHost(), (string)null);
		MultiHitBeforeDamageField = AccessTools.Field(typeof(AttackCommand), "_beforeDamage");
		MultiHitStates = new ConditionalWeakTable<Player, MultiHitState>();
		MultiHitCues = new Lazy<MultiHitCueCache>(BuildMultiHitCueCache);
		MultiHitStrongAnimations = new _003C_003Ez__ReadOnlyArray<string>(new string[7] { "attack_ready", "attack_play1", "attack_play2", "attack_end", "f_strong_attack_ready", "f_strong_attack_play", "f_strong_attack_end" });
		MultiHitU2Animations = new _003C_003Ez__ReadOnlyArray<string>(new string[7] { "attack_ready", "attack_play1", "attack_play2", "attack_end", "u2_attack_ready", "u2_attack_play", "u2_attack_end" });
		SettingsGlobalRuntimeLazy = new Lazy<SkinSettingsGlobalUiRuntime>((Func<SkinSettingsGlobalUiRuntime>)(() => new SkinSettingsGlobalUiRuntime(SkinSettingsStore)));
		SettingsUiRuntimeLazy = new Lazy<SkinSettingsUiRuntime>((Func<SkinSettingsUiRuntime>)(() => new SkinSettingsUiRuntime(SkinProfile, SkinRuntimeSettings, new SkinSettingsUiRuntimeHooks((Action)delegate
		{
			EffectMaterials.ApplyBrightnessSettings();
		}, (Action)delegate
		{
			BattleReadyOverlay.ApplyTransformFromSettings();
		}, (Action)delegate
		{
			BattleReadyOverlay.Cleanup(true);
		}, (Action)delegate
		{
			CleanupCombatEffects(clearSceneCache: true);
		}, (Action)delegate
		{
			CleanupCombatEffects(clearSceneCache: true);
		}, (Action)delegate
		{
			SfxPlayer.TrimCache(true);
		}, (Action)delegate
		{
			VoicePlayer.Stop();
		}, (Action<bool>)ApplyCardPlayPresentationEnabledForRuntime, (Action<string, int>)delegate(string message, int priority)
		{
			SkinLog.Info(message, priority);
		}), BundledCardArt.CreateCharacterExtensions())));
		TransportScope = new PresentationEventScope("MeirinWatcherSkin", (Action<string, Exception>)delegate(string context, Exception ex)
		{
			SkinLog.WarnException(context + ": ", ex);
		});
		BoninallPresentationPatchTypes = new HashSet<string>(StringComparer.Ordinal)
		{
			"WatcherMod.WatcherCreatureVisualsPatch", "WatcherMod.WatcherMerchantCharacterPatch", "WatcherMod.WatcherRestSiteCharacterPatch", "WatcherMod.WatcherCharSelectBgPatch", "WatcherMod.WatcherCharSelectRandomBgPatch", "WatcherMod.WatcherMultiplayerLoadHostBgPatch", "WatcherMod.WatcherMultiplayerLoadClientBgPatch", "WatcherMod.WatcherCharSelectIconPatch", "WatcherMod.WatcherCharSelectLockedIconPatch", "WatcherMod.WatcherIconTexturePatch",
			"WatcherMod.WatcherIconOutlineTexturePatch", "WatcherMod.WatcherIconScenePatch", "WatcherMod.NMapMarkerWatcherInitPatch", "WatcherMod.WatcherProphetMerchantSkinPatch", "WatcherMod.WatcherAttackSfxPatch", "WatcherMod.WatcherCastSfxPatch", "WatcherMod.WatcherDeathSfxPatch", "WatcherMod.AttackCommandWithHitFxPatch"
		};
	}
}
