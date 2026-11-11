using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.sts2.Core.Nodes.TopBar;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CharacterAppearanceRuntime
{
	private sealed record PendingSelection(string GroupId, string OptionId);

	private sealed record PendingMonsterRefresh(HashSet<string> GroupIds, string Reason);

	[CompilerGenerated]
	private sealed class _003CEnumerateDescendants_003Ed__144<TNode> : IEnumerable<TNode>, IEnumerable, IEnumerator<TNode>, IEnumerator, IDisposable where TNode : notnull, Node
	{
		private int _003C_003E1__state;

		private TNode _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private Node _003Cchild_003E5__3;

		private IEnumerator<TNode> _003C_003E7__wrap3;

		TNode IEnumerator<TNode>.Current
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
		public _003CEnumerateDescendants_003Ed__144(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003Cchild_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = root.GetChildren(false).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00f9;
				case 1:
					_003C_003E1__state = -3;
					goto IL_0092;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00d8;
					}
					IL_00f9:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_003Cchild_003E5__3 = _003C_003E7__wrap1.Current;
						Node obj = _003Cchild_003E5__3;
						TNode val = (TNode)(object)((obj is TNode) ? obj : null);
						if (val != null)
						{
							_003C_003E2__current = val;
							_003C_003E1__state = 1;
							return true;
						}
						goto IL_0092;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_0092:
					_003C_003E7__wrap3 = EnumerateDescendants<TNode>(_003Cchild_003E5__3).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00d8;
					IL_00d8:
					if (_003C_003E7__wrap3.MoveNext())
					{
						TNode current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					_003Cchild_003E5__3 = null;
					goto IL_00f9;
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
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
		{
			_003CEnumerateDescendants_003Ed__144<TNode> _003CEnumerateDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateDescendants_003Ed__ = this;
			}
			else
			{
				_003CEnumerateDescendants_003Ed__ = new _003CEnumerateDescendants_003Ed__144<TNode>(0);
			}
			_003CEnumerateDescendants_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TNode>)this).GetEnumerator();
		}
	}

	internal const string TransformWrapperName = "SkinChangerCharacterTransform";

	internal const string HealthDisplayWrapperName = "SkinChangerHealthDisplayTransform";

	internal const string IntentDisplayWrapperName = "SkinChangerIntentDisplayTransform";

	private const string HealthBoundsProxyName = "SkinChangerHealthBoundsProxy";

	private const string BaseVisualScaleMeta = "skin_changer_character_base_visual_scale";

	private const string BaseDefaultScaleMeta = "skin_changer_character_base_default_scale";

	private const string CurrentScaleMeta = "skin_changer_character_current_scale";

	private const string CurrentOffsetXMeta = "skin_changer_character_current_offset_x";

	private const string CurrentOffsetYMeta = "skin_changer_character_current_offset_y";

	private const string CurrentHealthScaleMeta = "skin_changer_health_current_scale";

	private const string CurrentHealthOffsetXMeta = "skin_changer_health_current_offset_x";

	private const string CurrentHealthOffsetYMeta = "skin_changer_health_current_offset_y";

	private const string CurrentHealthFollowScaleMeta = "skin_changer_health_follow_scale";

	private const string CurrentHealthFollowMovementMeta = "skin_changer_health_follow_movement";

	private const string CurrentIntentScaleMeta = "skin_changer_intent_current_scale";

	private const string CurrentIntentOffsetXMeta = "skin_changer_intent_current_offset_x";

	private const string CurrentIntentOffsetYMeta = "skin_changer_intent_current_offset_y";

	private const string CurrentIntentFollowScaleMeta = "skin_changer_intent_follow_scale";

	private const string CurrentIntentFollowMovementMeta = "skin_changer_intent_follow_movement";

	private const string CurrentReticleScaleMeta = "skin_changer_reticle_current_scale";

	private const string CurrentReticleOffsetXMeta = "skin_changer_reticle_current_offset_x";

	private const string CurrentReticleOffsetYMeta = "skin_changer_reticle_current_offset_y";

	private const string CurrentReticleFollowScaleMeta = "skin_changer_reticle_follow_scale";

	private const string CurrentReticleFollowMovementMeta = "skin_changer_reticle_follow_movement";

	private const string BossMapRefreshGenerationMeta = "skin_changer_boss_map_refresh_generation";

	private static readonly FieldInfo? VisualsField = AccessTools.Field(typeof(NCreature), "<Visuals>k__BackingField");

	private static readonly FieldInfo? CombatBackgroundField = AccessTools.Field(typeof(NCombatRoom), "<Background>k__BackingField");

	private static readonly FieldInfo? SpineAnimatorField = AccessTools.Field(typeof(NCreature), "_spineAnimator");

	private static readonly FieldInfo? SelectionReticleField = AccessTools.Field(typeof(NCreature), "_selectionReticle");

	private static readonly FieldInfo? SelectionReticleTweenField = AccessTools.Field(typeof(NSelectionReticle), "_currentTween");

	private static readonly FieldInfo? StateDisplayField = AccessTools.Field(typeof(NCreature), "_stateDisplay");

	private static readonly FieldInfo? TempScaleField = AccessTools.Field(typeof(NCreature), "_tempScale");

	private static readonly FieldInfo? RunStateField = AccessTools.Field(typeof(NRun), "_state");

	private static readonly FieldInfo? RunRoomContainerField = AccessTools.Field(typeof(NRun), "_roomContainer");

	private static readonly FieldInfo? MapMarkerField = AccessTools.Field(typeof(NMapScreen), "_marker");

	private static readonly FieldInfo? BossMapUsesSpineField = AccessTools.Field(typeof(NBossMapPoint), "_usesSpine");

	private static readonly FieldInfo? BossMapAnimatorField = AccessTools.Field(typeof(NBossMapPoint), "_animController");

	private static readonly FieldInfo? BossMapMaterialField = AccessTools.Field(typeof(NBossMapPoint), "_material");

	private static readonly FieldInfo? BossMapPlaceholderImageField = AccessTools.Field(typeof(NBossMapPoint), "_placeholderImage");

	private static readonly FieldInfo? BossMapPlaceholderOutlineField = AccessTools.Field(typeof(NBossMapPoint), "_placeholderOutline");

	private static readonly MethodInfo? BossMapRefreshColorMethod = AccessTools.Method(typeof(NBossMapPoint), "RefreshColorInstantly", (Type[])null, (Type[])null);

	private static readonly MethodInfo? TopBarRoomIconRefreshMethod = AccessTools.Method(typeof(NTopBarRoomIcon), "UpdateIcon", (Type[])null, (Type[])null);

	private static readonly FieldInfo? OrbNodesField = AccessTools.Field(typeof(NOrbManager), "_orbs");

	private static readonly FieldInfo? EnergyCounterField = AccessTools.Field(typeof(NCombatUi), "_energyCounter");

	private static readonly FieldInfo? StarCounterField = AccessTools.Field(typeof(NCombatUi), "_starCounter");

	private static readonly MethodInfo? ConnectSpineAnimatorSignalsMethod = AccessTools.Method(typeof(NCreature), "ConnectSpineAnimatorSignals", (Type[])null, (Type[])null);

	private static readonly MethodInfo? UpdateBoundsMethod = AccessTools.Method(typeof(NCreature), "UpdateBounds", new Type[1] { typeof(Node) }, (Type[])null);

	private static readonly MethodInfo? UpdatePhobiaModeMethod = AccessTools.Method(typeof(NCreature), "UpdatePhobiaMode", (Type[])null, (Type[])null);

	private static readonly MethodInfo? SetOrbManagerPositionMethod = AccessTools.Method(typeof(NCreature), "SetOrbManagerPosition", (Type[])null, (Type[])null);

	private static PendingSelection? _pendingSelection;

	private static PendingMonsterRefresh? _pendingMonsterRefresh;

	private static WeakReference<NCombatRoom>? _playerLayoutRoom;

	private static readonly HashSet<string> CombatRuntimeGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> CombatRuntimeMonsterGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> PersistentCombatRuntimeGroupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static long _combatRuntimeScopeLease;

	private static bool _combatRuntimeScopeActive;

	private static float _playerLayoutScaling = 1f;

	private static bool _fullyCenterPlayers;

	internal static event Action<string, bool, string?>? QueuedSelectionFinished;

	private static bool TryGetRunState(NRun? run, out IRunState runState)
	{
		runState = null;
		if (run == null || !GodotObject.IsInstanceValid((GodotObject)(object)run) || (object)RunStateField == null)
		{
			return false;
		}
		try
		{
			object? value = RunStateField.GetValue(run);
			IRunState val = (IRunState)((value is IRunState) ? value : null);
			if (val == null)
			{
				return false;
			}
			runState = val;
			return true;
		}
		catch (TargetException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	internal static AppearanceSelectionRequestResult RequestSelection(string groupId, string optionId)
	{
		if (!CanApplySelectionNow())
		{
			_pendingSelection = new PendingSelection(groupId, optionId);
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				((Node)instance).GetNodeOrNull<CharacterAppearanceRuntimeNode>(NodePath.op_Implicit("SkinChangerAppearanceRuntime"))?.Wake();
			}
			return new AppearanceSelectionRequestResult(AppearanceSelectionRequestState.Queued);
		}
		return ApplySelectionNow(groupId, optionId);
	}

	internal static string? GetRequestedOption(string groupId)
	{
		PendingSelection pendingSelection = _pendingSelection;
		if ((object)pendingSelection == null || !pendingSelection.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		return pendingSelection.OptionId;
	}

	internal static Player? GetLocalPlayer()
	{
		try
		{
			IRunState runState;
			return TryGetRunState(NRun.Instance, out runState) ? LocalContext.GetMe((IPlayerCollection)(object)runState) : null;
		}
		catch (Exception ex)
		{
			ModLog.Warn("读取当前玩家失败：" + ex.GetBaseException().Message);
			return null;
		}
	}

	internal static bool TryGetPlayerCharacter(ulong playerNetId, out CharacterModel character)
	{
		try
		{
			if (TryGetRunState(NRun.Instance, out IRunState runState))
			{
				Player val = ((IEnumerable<Player>)((IPlayerCollection)runState).Players).FirstOrDefault((Func<Player, bool>)((Player player) => player.NetId == playerNetId));
				if (val != null)
				{
					character = val.Character;
					return true;
				}
			}
		}
		catch
		{
		}
		character = null;
		return false;
	}

	internal static bool PlayerMatchesCharacterSelection(ulong playerNetId, string characterId, string groupId)
	{
		try
		{
			if (!TryGetRunState(NRun.Instance, out IRunState runState))
			{
				return false;
			}
			Player val = ((IEnumerable<Player>)((IPlayerCollection)runState).Players).FirstOrDefault((Func<Player, bool>)((Player candidate) => candidate.NetId == playerNetId));
			SkinGroup skinGroup = ((val == null) ? null : ContextualSkinControls.FindGroup(((AbstractModel)val.Character).Id.Entry, ((object)val.Character).GetType().Name));
			return val != null && ((AbstractModel)val.Character).Id.Entry.Equals(characterId, StringComparison.OrdinalIgnoreCase) && (skinGroup?.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase) ?? false);
		}
		catch
		{
			return false;
		}
	}

	internal static void FocusRuntimeProviderBehaviorsOnRunCharacters(NRun? run = null)
	{
		FocusRuntimeProviderBehaviorsOnRunContext(null, "对局角色", null, run);
	}

	internal static long FocusRuntimeProviderBehaviorsOnRunContext(IEnumerable<string>? additionalGroupIds = null, string reason = "对局", long? expectedScopeLease = null, NRun? run = null, bool refreshCurrentRoom = false)
	{
		try
		{
			HashSet<string> hashSet = RuntimeProviderScopePolicy.MergeVisibleGroups(additionalGroupIds, CombatRuntimeGroupIds).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			NRun run2 = run ?? NRun.Instance;
			if (TryGetRunState(run2, out IRunState runState))
			{
				hashSet.UnionWith((from player in ((IPlayerCollection)runState).Players
					select ContextualSkinControls.FindGroup(((AbstractModel)player.Character).Id.Entry, ((object)player.Character).GetType().Name)?.Id into groupId
					where groupId != null
					select groupId).Cast<string>());
				HashSet<string> groupIds = ResolveMonsterGroupIds(runState.Act.AllMonsters);
				hashSet.UnionWith(ResolveCurrentBossGroupIds(runState));
				HashSet<string> hashSet2 = ResolveActiveCombatMonsterGroupIds();
				IReadOnlySet<string> monsterRunEnvironmentProviders = SkinService.GetMonsterRunEnvironmentProviders("act:" + ((AbstractModel)runState.Act).Id.Entry.ToLowerInvariant(), groupIds, _combatRuntimeScopeActive ? hashSet2 : null);
				long scopeLease;
				if (expectedScopeLease.HasValue)
				{
					return TrackCurrentRoomScopeLease(SkinService.TryFocusRuntimeProviderBehaviorsOnGroups(expectedScopeLease.Value, hashSet, monsterRunEnvironmentProviders, reason, out scopeLease) ? scopeLease : 0, refreshCurrentRoom, run2);
				}
				return TrackCurrentRoomScopeLease(SkinService.FocusRuntimeProviderBehaviorsOnGroups(hashSet, monsterRunEnvironmentProviders, reason), refreshCurrentRoom, run2);
			}
			long scopeLease2;
			if (expectedScopeLease.HasValue)
			{
				return TrackCurrentRoomScopeLease(SkinService.TryFocusRuntimeProviderBehaviorsOnGroups(expectedScopeLease.Value, hashSet, (IReadOnlyCollection<string>)(object)Array.Empty<string>(), reason, out scopeLease2) ? scopeLease2 : 0, refreshCurrentRoom, run2);
			}
			return TrackCurrentRoomScopeLease(SkinService.FocusRuntimeProviderBehaviorsOnGroups(hashSet, (IReadOnlyCollection<string>)(object)Array.Empty<string>(), reason), refreshCurrentRoom, run2);
		}
		catch (Exception ex)
		{
			ModLog.Warn("收窄当前场景皮肤行为失败：" + ex);
			return 0L;
		}
	}

	private static long TrackCurrentRoomScopeLease(long scopeLease, bool refreshCurrentRoom, NRun? run)
	{
		if (scopeLease != 0 && refreshCurrentRoom)
		{
			if (_combatRuntimeScopeActive)
			{
				_combatRuntimeScopeLease = scopeLease;
			}
			if (run != null)
			{
				object? obj = RunRoomContainerField?.GetValue(run);
				NSceneContainer val = (NSceneContainer)((obj is NSceneContainer) ? obj : null);
				if (val != null)
				{
					RoomRuntimeScopeOwnership.RefreshTree((Node?)(object)val.CurrentScene, scopeLease);
				}
			}
		}
		return scopeLease;
	}

	private static HashSet<string> ResolveMonsterGroupIds(IEnumerable<MonsterModel> monsters)
	{
		return (from monster in monsters
			select ContextualSkinControls.FindGroup(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name)?.Id into groupId
			where groupId != null
			select groupId).Cast<string>().ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static HashSet<string> ResolveCurrentBossGroupIds(IRunState runState)
	{
		return ResolveMonsterGroupIds(((IEnumerable<EncounterModel>)(object)new EncounterModel[2]
		{
			runState.Act.BossEncounter,
			runState.Act.SecondBossEncounter
		}).Where((EncounterModel encounter) => encounter != null).SelectMany((EncounterModel encounter) => encounter.AllPossibleMonsters));
	}

	private static HashSet<string> ResolveActiveCombatMonsterGroupIds()
	{
		NCombatRoom val = TryGetActiveCombatRoom(NRun.Instance);
		if (val == null || !((Node)val).IsInsideTree())
		{
			return CombatRuntimeMonsterGroupIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		return (from creature in val.CreatureNodes
			where creature.Entity.Monster != null
			select ContextualSkinControls.FindGroup(creature.Entity.ModelId.Entry, ((object)creature.Entity.Monster).GetType().Name)?.Id into groupId
			where groupId != null
			select groupId).Cast<string>().ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static NCombatRoom? TryGetActiveCombatRoom(NRun? run)
	{
		if (run == null || RunRoomContainerField?.GetValue(run) == null)
		{
			return null;
		}
		return run.CombatRoom;
	}

	internal static bool TryGetBossMapAppearance(NBossMapPoint bossPoint, out SkinGroup group, out string title)
	{
		group = null;
		title = string.Empty;
		try
		{
			if (!TryGetRunState(NRun.Instance, out IRunState runState))
			{
				return false;
			}
			EncounterModel val = ((((NMapPoint)bossPoint).Point == runState.Map.SecondBossMapPoint) ? runState.Act.SecondBossEncounter : runState.Act.BossEncounter);
			if (val == null)
			{
				return false;
			}
			group = val.AllPossibleMonsters.Select((MonsterModel monster) => ContextualSkinControls.FindGroup(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name)).FirstOrDefault(delegate(SkinGroup candidate)
			{
				if (candidate != null)
				{
					List<SkinOption> options = candidate.Options;
					if (options != null)
					{
						return options.Count > 0;
					}
				}
				return false;
			});
			if (group == null)
			{
				return false;
			}
			title = val.Title.GetFormattedText();
			return true;
		}
		catch
		{
			group = null;
			title = string.Empty;
			return false;
		}
	}

	internal static void FocusRuntimeProviderBehaviorsOnCombatRoom(ICombatRoomVisuals visuals)
	{
		_combatRuntimeScopeActive = true;
		CombatRuntimeGroupIds.Clear();
		CombatRuntimeMonsterGroupIds.Clear();
		PersistentCombatRuntimeGroupIds.Clear();
		foreach (Creature item in visuals.Allies.Concat(visuals.Enemies))
		{
			object obj = ((object)item.Monster)?.GetType().Name;
			if (obj == null)
			{
				Player player = item.Player;
				obj = ((player != null) ? ((object)player.Character).GetType().Name : null);
			}
			string modelTypeName = (string)obj;
			SkinGroup skinGroup = ContextualSkinControls.FindGroup(item.ModelId.Entry, modelTypeName);
			if (skinGroup != null)
			{
				CombatRuntimeGroupIds.Add(skinGroup.Id);
				if (item.Monster != null)
				{
					CombatRuntimeMonsterGroupIds.Add(skinGroup.Id);
				}
				if (item.Player != null)
				{
					PersistentCombatRuntimeGroupIds.Add(skinGroup.Id);
				}
			}
		}
		_combatRuntimeScopeLease = FocusRuntimeProviderBehaviorsOnRunContext(CombatRuntimeGroupIds, "战斗场景", null);
	}

	internal static void AddVisibleCombatRuntimeGroup(string groupId)
	{
		if (NRun.Instance != null)
		{
			bool num = CombatRuntimeGroupIds.Add(groupId);
			CombatRuntimeMonsterGroupIds.Add(groupId);
			if (num)
			{
				_combatRuntimeScopeLease = FocusRuntimeProviderBehaviorsOnRunContext(CombatRuntimeGroupIds, "战斗召唤", null);
			}
		}
	}

	internal static void ClearCombatRuntimeGroups()
	{
		long combatRuntimeScopeLease = _combatRuntimeScopeLease;
		_combatRuntimeScopeLease = 0L;
		_combatRuntimeScopeActive = false;
		IReadOnlySet<string> groupIds = RuntimeResourceRetentionPolicy.SelectTransientCombatGroups(CombatRuntimeGroupIds, PersistentCombatRuntimeGroupIds);
		CombatRuntimeGroupIds.Clear();
		CombatRuntimeMonsterGroupIds.Clear();
		PersistentCombatRuntimeGroupIds.Clear();
		SkinService.ReleaseTransientRuntimeResources(groupIds);
		if (combatRuntimeScopeLease != 0L)
		{
			FocusRuntimeProviderBehaviorsOnRunContext(null, "对局角色", combatRuntimeScopeLease);
		}
	}

	internal static void RefreshRunMonsterSelection(IEnumerable<string> affectedGroupIds, string reason)
	{
		if (NRun.Instance == null)
		{
			return;
		}
		HashSet<string> hashSet = affectedGroupIds.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (hashSet.Count == 0)
		{
			return;
		}
		if (!CanApplySelectionNow())
		{
			if (_pendingMonsterRefresh == null)
			{
				_pendingMonsterRefresh = new PendingMonsterRefresh(hashSet, reason);
			}
			else
			{
				_pendingMonsterRefresh.GroupIds.UnionWith(hashSet);
				_pendingMonsterRefresh = _pendingMonsterRefresh with
				{
					Reason = reason
				};
			}
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				((Node)instance).GetNodeOrNull<CharacterAppearanceRuntimeNode>(NodePath.op_Implicit("SkinChangerAppearanceRuntime"))?.Wake();
			}
		}
		else
		{
			ApplyRunAppearanceRefresh(hashSet, reason);
		}
	}

	internal static NCreature? GetCurrentCreature(Player? player)
	{
		if (player == null)
		{
			return null;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return null;
		}
		return instance.GetCreatureNode(player.Creature);
	}

	internal static bool ProcessPendingSelection()
	{
		if (_pendingSelection == null && _pendingMonsterRefresh == null)
		{
			return false;
		}
		if (!CanApplySelectionNow())
		{
			return true;
		}
		PendingSelection pendingSelection = _pendingSelection;
		if ((object)pendingSelection != null)
		{
			_pendingSelection = null;
			AppearanceSelectionRequestResult appearanceSelectionRequestResult = ApplySelectionNow(pendingSelection.GroupId, pendingSelection.OptionId);
			CharacterAppearanceRuntime.QueuedSelectionFinished?.Invoke(pendingSelection.GroupId, appearanceSelectionRequestResult.State == AppearanceSelectionRequestState.Applied, appearanceSelectionRequestResult.Error);
		}
		PendingMonsterRefresh pendingMonsterRefresh = _pendingMonsterRefresh;
		if ((object)pendingMonsterRefresh != null)
		{
			_pendingMonsterRefresh = null;
			ApplyRunAppearanceRefresh(pendingMonsterRefresh.GroupIds, pendingMonsterRefresh.Reason);
		}
		if (!(_pendingSelection != null))
		{
			return _pendingMonsterRefresh != null;
		}
		return true;
	}

	internal static void ClearPendingSelection()
	{
		_pendingSelection = null;
		_pendingMonsterRefresh = null;
	}

	internal static bool CanApplySelectionImmediately()
	{
		return CanApplySelectionNow();
	}

	internal static void OnCreatureReady(NCreature creature)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals))
		{
			return;
		}
		CaptureVisualBaseline(creature.Visuals);
		if (TryGetCreatureAppearance(creature, out CreatureAppearanceBinding _))
		{
			EnsureTransformWrapper(creature);
			EnsureHealthDisplayWrapper(creature);
			if (SupportsIntentAppearance(creature))
			{
				EnsureIntentDisplayWrapper(creature);
			}
			ApplyStoredTransform(creature);
		}
	}

	internal static void ApplyStoredTransform(NCreature creature)
	{
		if (TryGetCreatureAppearance(creature, out CreatureAppearanceBinding binding))
		{
			ApplyPreviewTransform(creature, GetCreatureCombatTransform(creature, binding));
		}
	}

	internal static CharacterCombatTransform GetCreatureCombatTransform(NCreature creature)
	{
		if (!TryGetCreatureAppearance(creature, out CreatureAppearanceBinding binding))
		{
			return new CharacterCombatTransform();
		}
		return GetCreatureCombatTransform(creature, binding);
	}

	internal static CharacterCombatTransform SetCreatureCombatTransform(NCreature creature, CharacterCombatTransform value, bool save = true)
	{
		if (!TryGetCreatureAppearance(creature, out CreatureAppearanceBinding binding))
		{
			return new CharacterCombatTransform();
		}
		if (!MultiplayerSkinSync.CanEditTransformForCreature(creature.Entity))
		{
			return GetCreatureCombatTransform(creature, binding);
		}
		string selectionForCreature = MultiplayerSkinSync.GetSelectionForCreature(creature.Entity, binding.Group.Id);
		if (MultiplayerSkinSync.UsesLocalFallbackControls(creature.Entity))
		{
			return MultiplayerSkinSync.SetLocalFallbackTransform(creature.Entity, binding.TransformKey, value);
		}
		if (!binding.UsesMonsterScale)
		{
			CharacterCombatTransform result = SkinService.SetCharacterCombatTransform(binding.TransformKey, selectionForCreature, value, save);
			MultiplayerSkinSync.OnLocalTransformChanged(creature.Entity, binding.Group.Id);
			return result;
		}
		float scale = SkinService.SetSelectedMonsterScale(binding.Group.Id, value.Scale, save: false);
		CharacterCombatTransform characterCombatTransform = SkinService.SetCharacterCombatTransform(binding.TransformKey, selectionForCreature, value with
		{
			Scale = 1f
		}, save);
		MultiplayerSkinSync.OnLocalTransformChanged(creature.Entity, binding.Group.Id);
		return characterCombatTransform with
		{
			Scale = scale
		};
	}

	internal static void ApplyTransformToKey(string transformKey)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return;
		}
		NCreature[] array = instance.CreatureNodes.ToArray();
		foreach (NCreature creature in array)
		{
			if (TryGetCreatureAppearance(creature, out CreatureAppearanceBinding binding) && binding.TransformKey.Equals(transformKey, StringComparison.OrdinalIgnoreCase))
			{
				ApplyStoredTransform(creature);
			}
		}
	}

	internal static void ApplyPreviewTransform(NCreature creature, CharacterCombatTransform value)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = EnsureTransformWrapper(creature);
		if (val != null)
		{
			SetCurrentTransform(val, value);
			val.Position = new Vector2(value.OffsetX, value.OffsetY);
			if (TryGetCreatureAppearance(creature, out CreatureAppearanceBinding binding) && binding.UsesMonsterScale)
			{
				ContextualSkinControls.ApplyMonsterScalePreview(creature.Visuals, value.Scale);
				val.Scale = Vector2.One;
			}
			else
			{
				val.Scale = Vector2.One * value.Scale;
			}
			RefreshCreatureAnchors(creature);
		}
	}

	internal static Node2D? GetTransformWrapper(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return null;
		}
		return ((Node)creature).GetNodeOrNull<Node2D>(NodePath.op_Implicit("SkinChangerCharacterTransform"));
	}

	internal static Node2D? GetHealthDisplayWrapper(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return null;
		}
		return ((Node)creature).GetNodeOrNull<Node2D>(NodePath.op_Implicit("SkinChangerHealthDisplayTransform"));
	}

	internal static Node2D? GetIntentDisplayWrapper(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return null;
		}
		return ((Node)creature).GetNodeOrNull<Node2D>(NodePath.op_Implicit("SkinChangerIntentDisplayTransform"));
	}

	internal static NSelectionReticle? GetSelectionReticle(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return null;
		}
		object? obj = SelectionReticleField?.GetValue(creature);
		return (NSelectionReticle?)((obj is NSelectionReticle) ? obj : null);
	}

	internal static void StopSelectionReticleAnimation(NSelectionReticle? reticle)
	{
		if (reticle != null && GodotObject.IsInstanceValid((GodotObject)(object)reticle))
		{
			object? obj = SelectionReticleTweenField?.GetValue(reticle);
			Tween val = (Tween)((obj is Tween) ? obj : null);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				val.Kill();
			}
		}
	}

	internal static bool SupportsIntentAppearance(NCreature? creature)
	{
		if (creature != null && GodotObject.IsInstanceValid((GodotObject)(object)creature) && TryGetCreatureAppearance(creature, out CreatureAppearanceBinding binding))
		{
			return binding.SupportsIntent;
		}
		return false;
	}

	internal static NCreatureStateDisplay? GetStateDisplay(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature))
		{
			return null;
		}
		object? obj = StateDisplayField?.GetValue(creature);
		return (NCreatureStateDisplay?)((obj is NCreatureStateDisplay) ? obj : null);
	}

	internal static Control? GetHealthBarBounds(NCreature? creature)
	{
		NCreatureStateDisplay stateDisplay = GetStateDisplay(creature);
		if (stateDisplay == null || !GodotObject.IsInstanceValid((GodotObject)(object)stateDisplay))
		{
			return null;
		}
		NHealthBar nodeOrNull = ((Node)stateDisplay).GetNodeOrNull<NHealthBar>(NodePath.op_Implicit("HealthBar"));
		if (nodeOrNull == null || !GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull.HpBarContainer))
		{
			return null;
		}
		return nodeOrNull.HpBarContainer;
	}

	internal static Control? GetModelBounds(NCreature? creature)
	{
		if (creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)creature) || !GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals))
		{
			return null;
		}
		return ((Node)creature.Visuals).GetNodeOrNull<Control>(NodePath.op_Implicit("%Bounds"));
	}

	internal static bool TryGetCreatureAppearance(NCreature creature, out CreatureAppearanceBinding binding)
	{
		binding = null;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creature) || creature.Entity == null)
		{
			return false;
		}
		string entry = creature.Entity.ModelId.Entry;
		Player player = creature.Entity.Player;
		string modelTypeName = ((player != null) ? ((object)player.Character).GetType().Name : null) ?? ((object)creature.Entity.Monster)?.GetType().Name;
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(entry, modelTypeName);
		Player petOwner = creature.Entity.PetOwner;
		if (petOwner != null && skinGroup == null)
		{
			CharacterModel character = petOwner.Character;
			SkinGroup skinGroup2 = ContextualSkinControls.FindGroup(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name);
			if (skinGroup2 != null)
			{
				binding = new CreatureAppearanceBinding(skinGroup2, skinGroup2.Id + "::companion::" + entry, UsesMonsterScale: false, CanSelectSkin: false, SupportsIntent: false, SupportsCombatControls: true);
				return true;
			}
		}
		if (skinGroup != null)
		{
			binding = CreateCreatureAppearanceBinding(skinGroup, creature.Entity.IsMonster, creature.Entity.PetOwner != null);
			return true;
		}
		return false;
	}

	private static CreatureAppearanceBinding CreateCreatureAppearanceBinding(SkinGroup group, bool isMonster, bool isCompanion)
	{
		return new CreatureAppearanceBinding(group, group.Id, isMonster && !isCompanion, !isCompanion, isMonster && !isCompanion, SupportsCombatControls: true);
	}

	private static CharacterCombatTransform GetCreatureCombatTransform(NCreature creature, CreatureAppearanceBinding binding)
	{
		if (MultiplayerSkinSync.TryGetSyncedTransform(creature.Entity, binding.TransformKey, out CharacterCombatTransform transform))
		{
			return transform;
		}
		string selectionForCreature = MultiplayerSkinSync.GetSelectionForCreature(creature.Entity, binding.Group.Id);
		CharacterCombatTransform characterCombatTransform = SkinService.GetCharacterCombatTransform(binding.TransformKey, selectionForCreature);
		if (!binding.UsesMonsterScale)
		{
			return characterCombatTransform;
		}
		return characterCombatTransform with
		{
			Scale = SkinService.GetSelectedMonsterScale(binding.Group.Id)
		};
	}

	internal static void CorrectBoundsForVisualTransforms(NCreature creature, Node boundsContainer)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		Node2D transformWrapper = GetTransformWrapper(creature);
		if (transformWrapper == null)
		{
			CorrectMonsterBoundsForModelOnlyScale(creature);
			return;
		}
		try
		{
			Control node = boundsContainer.GetNode<Control>(NodePath.op_Implicit("%Bounds"));
			float num2 = ((TempScaleField?.GetValue(creature) is float num && !Mathf.IsZeroApprox(num)) ? num : 1f);
			float num3 = (creature.Entity.IsMonster ? ContextualSkinControls.GetAppliedMonsterScaleFactor(creature.Visuals) : 1f);
			if (Mathf.IsZeroApprox(num3))
			{
				num3 = 1f;
			}
			float x = transformWrapper.Scale.X;
			Vector2 val = node.Size * ((Node2D)creature.Visuals).Scale / num2 / num3;
			Transform2D globalTransform = ((CanvasItem)transformWrapper).GetGlobalTransform();
			Vector2 val2 = ((Transform2D)(ref globalTransform)).AffineInverse() * node.GlobalPosition / num2 / num3;
			Vector2 val3 = val * num3 * x;
			Vector2 globalPosition = ((CanvasItem)transformWrapper).GetGlobalTransform() * (val2 * num3);
			CharacterCombatTransform currentTransform = GetCurrentTransform(transformWrapper);
			creature.Hitbox.Size = val3;
			creature.Hitbox.GlobalPosition = globalPosition;
			if (SupportsIntentAppearance(creature))
			{
				NSelectionReticle selectionReticle = GetSelectionReticle(creature);
				if (selectionReticle != null)
				{
					ApplySelectionReticleAndHitbox(selectionReticle, creature.Hitbox, currentTransform, val2, val);
					goto IL_019a;
				}
			}
			object? obj = SelectionReticleField?.GetValue(creature);
			Control val4 = (Control)((obj is Control) ? obj : null);
			if (val4 != null)
			{
				val4.Size = val3;
				val4.GlobalPosition = globalPosition;
				val4.PivotOffset = val3 * 0.5f;
			}
			goto IL_019a;
			IL_019a:
			Marker2D node2 = boundsContainer.GetNode<Marker2D>(NodePath.op_Implicit("IntentPos"));
			globalTransform = ((CanvasItem)transformWrapper).GetGlobalTransform();
			Vector2 val5 = ((Transform2D)(ref globalTransform)).AffineInverse() * ((Node2D)node2).GlobalPosition / num2 / num3;
			Node2D val6 = (SupportsIntentAppearance(creature) ? EnsureIntentDisplayWrapper(creature) : null);
			if (val6 != null)
			{
				creature.IntentContainer.Position = -creature.IntentContainer.Size * 0.5f;
				ApplyIntentDisplayTransform(val6, currentTransform, val5);
			}
			else
			{
				Vector2 val7 = ((CanvasItem)transformWrapper).GetGlobalTransform() * (val5 * num3);
				globalTransform = ((CanvasItem)creature).GetGlobalTransform();
				Vector2 val8 = ((Transform2D)(ref globalTransform)).AffineInverse() * val7;
				creature.IntentContainer.Position = val8 - creature.IntentContainer.Size * 0.5f;
			}
			NCreatureStateDisplay stateDisplay = GetStateDisplay(creature);
			Node2D val9 = EnsureHealthDisplayWrapper(creature);
			Control val10 = EnsureHealthBoundsProxy(creature);
			if (stateDisplay != null && val9 != null && val10 != null)
			{
				val9.Position = Vector2.Zero;
				val9.Scale = Vector2.One;
				val10.Size = val;
				val10.Scale = Vector2.One;
				val10.GlobalPosition = ((CanvasItem)creature).GetGlobalTransform() * val2;
				stateDisplay.SetCreatureBounds(val10);
				ApplyHealthDisplayTransform(val9, currentTransform);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("校正实战外观定位点失败：" + ex.GetBaseException().Message);
		}
	}

	private static void CorrectMonsterBoundsForModelOnlyScale(NCreature creature)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		if (creature.Entity.IsPlayer || !GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals))
		{
			return;
		}
		float appliedMonsterScaleFactor = ContextualSkinControls.GetAppliedMonsterScaleFactor(creature.Visuals);
		if (Mathf.IsZeroApprox(appliedMonsterScaleFactor) || Mathf.IsEqualApprox(appliedMonsterScaleFactor, 1f))
		{
			return;
		}
		try
		{
			Vector2 val = creature.Hitbox.Size / appliedMonsterScaleFactor;
			Vector2 globalPosition = ((Control)creature).GlobalPosition + (creature.Hitbox.GlobalPosition - ((Control)creature).GlobalPosition) / appliedMonsterScaleFactor;
			creature.Hitbox.Size = val;
			creature.Hitbox.GlobalPosition = globalPosition;
			object? obj = SelectionReticleField?.GetValue(creature);
			Control val2 = (Control)((obj is Control) ? obj : null);
			if (val2 != null)
			{
				val2.Size = val;
				val2.GlobalPosition = globalPosition;
				val2.PivotOffset = val * 0.5f;
			}
			creature.IntentContainer.Position = new Vector2(creature.IntentContainer.Position.X, creature.IntentContainer.Position.Y / appliedMonsterScaleFactor);
			NCreatureStateDisplay? stateDisplay = GetStateDisplay(creature);
			if (stateDisplay != null)
			{
				stateDisplay.SetCreatureBounds(creature.Hitbox);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("校正怪物模型缩放后的战斗 UI 失败：" + ex.GetBaseException().Message);
		}
	}

	internal static void CorrectOrbPositionForCharacterTransform(NCreature creature)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (GetTransformWrapper(creature) == null || !creature.Entity.IsPlayer || creature.OrbManager == null)
		{
			return;
		}
		try
		{
			Transform2D globalTransform = ((CanvasItem)creature).GetGlobalTransform();
			Vector2 position = ((Transform2D)(ref globalTransform)).AffineInverse() * ((Node2D)creature.Visuals.OrbPosition).GlobalPosition;
			((Control)creature.OrbManager).Position = position;
			if (!creature.OrbManager.IsLocal)
			{
				NOrbManager orbManager = creature.OrbManager;
				((Control)orbManager).Position = ((Control)orbManager).Position + Vector2.Up * 50f;
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("校正角色充能球定位点失败：" + ex.GetBaseException().Message);
		}
	}

	internal static bool HasActiveDeathAnimation(NCreature creature)
	{
		Task deathAnimationTask = creature.DeathAnimationTask;
		if (deathAnimationTask != null)
		{
			return !deathAnimationTask.IsCompleted;
		}
		return false;
	}

	private static bool CanApplySelectionNow()
	{
		try
		{
			if (!RunManager.Instance.IsInProgress)
			{
				return true;
			}
			ActionExecutor actionExecutor = RunManager.Instance.ActionExecutor;
			if (actionExecutor != null && (actionExecutor.IsRunning || actionExecutor.CurrentlyRunningAction != null))
			{
				return false;
			}
			NCombatRoom instance = NCombatRoom.Instance;
			return instance == null || instance.CreatureNodes.All((NCreature creature) => !HasActiveDeathAnimation(creature));
		}
		catch
		{
			return false;
		}
	}

	private static AppearanceSelectionRequestResult ApplySelectionNow(string groupId, string optionId)
	{
		try
		{
			Dictionary<string, string> previous = new Dictionary<string, string>(SkinService.Config.Selections, StringComparer.OrdinalIgnoreCase);
			if (!SkinService.ApplySelection(groupId, optionId))
			{
				return new AppearanceSelectionRequestResult(AppearanceSelectionRequestState.Failed, SkinService.LastError);
			}
			HashSet<string> hashSet = (from key in previous.Keys.Concat(SkinService.Config.Selections.Keys)
				where previous.GetValueOrDefault(key) != SkinService.Config.Selections.GetValueOrDefault(key)
				select key).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			hashSet.Add(groupId);
			List<string> list = ApplyRunAppearanceRefresh(hashSet, "局内外观切换");
			RefreshCurrentCharacterUi();
			MultiplayerSkinSync.OnLocalCharacterSelectionChanged(groupId);
			if (list.Count > 0)
			{
				string text = string.Join("; ", list);
				ModLog.Warn("皮肤选择已保存，但部分实战外观将在下次创建时生效：" + text);
				return new AppearanceSelectionRequestResult(AppearanceSelectionRequestState.Applied, text);
			}
			return new AppearanceSelectionRequestResult(AppearanceSelectionRequestState.Applied);
		}
		catch (Exception ex)
		{
			ModLog.Error("实战切换角色外观失败：" + ex);
			return new AppearanceSelectionRequestResult(AppearanceSelectionRequestState.Failed, ex.GetBaseException().Message);
		}
	}

	private static List<string> RefreshLiveCreatures(IReadOnlySet<string> affectedGroups)
	{
		List<string> list = new List<string>();
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return list;
		}
		NCreature[] array = instance.CreatureNodes.ToArray();
		bool flag = false;
		HashSet<Player> hashSet = (from creature in array.Where((NCreature creature) => creature.Entity.Player != null).Where(delegate(NCreature creature)
			{
				CharacterModel character = creature.Entity.Player.Character;
				SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name);
				return skinGroup != null && affectedGroups.Contains(skinGroup.Id);
			})
			select creature.Entity.Player).ToHashSet();
		NCreature[] array2 = array;
		foreach (NCreature val in array2)
		{
			string entry = val.Entity.ModelId.Entry;
			Player player = val.Entity.Player;
			string modelTypeName = ((player != null) ? ((object)player.Character).GetType().Name : null) ?? ((object)val.Entity.Monster)?.GetType().Name;
			SkinGroup skinGroup2 = ContextualSkinControls.FindGroup(entry, modelTypeName);
			bool num = skinGroup2 != null && affectedGroups.Contains(skinGroup2.Id);
			Player petOwner = val.Entity.PetOwner;
			bool flag2 = petOwner != null && hashSet.Contains(petOwner);
			if (num || flag2)
			{
				string error;
				if (HasActiveDeathAnimation(val))
				{
					list.Add(entry + ": death animation");
				}
				else if (!TryRebuildCreatureVisuals(val, out error))
				{
					list.Add(entry + ": " + error);
				}
				else if (val.Entity.IsPlayer || val.Entity.PetOwner != null)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			RefreshPlayerAndPetLayout(instance);
		}
		return list;
	}

	private static List<string> ApplyRunAppearanceRefresh(IReadOnlySet<string> affectedGroups, string reason)
	{
		FocusRuntimeProviderBehaviorsOnRunContext(affectedGroups, reason, null, null, refreshCurrentRoom: true);
		List<string> list = RefreshLiveCreatures(affectedGroups);
		if (ResolveActiveCombatMonsterGroupIds().Overlaps(affectedGroups) && CombatManager.Instance.IsInProgress && NCombatRoom.Instance != null)
		{
			if (!RefreshCurrentCombatBackground())
			{
				list.Add("current combat background");
			}
			ManagedSkinModLoader.RefreshRunEnvironmentPresentation();
		}
		RefreshCurrentBossPresentation(affectedGroups);
		return list;
	}

	internal static bool RefreshCurrentCombatBackground()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		CombatState val = CombatManager.Instance.DebugOnlyGetState();
		EncounterModel val2 = ((val != null) ? val.Encounter : null);
		if (!CombatManager.Instance.IsInProgress || instance == null || !((Node)instance).IsInsideTree() || val == null || val2 == null || CombatBackgroundField == null)
		{
			return false;
		}
		NCombatBackground background = instance.Background;
		Node val3 = (Node)(((object)((background != null) ? ((Node)background).GetParent() : null)) ?? ((object)((Node)instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%BgContainer"))));
		if (val3 == null)
		{
			return false;
		}
		NCombatBackground val4 = null;
		try
		{
			val4 = val2.CreateBackground(val.RunState.Act, NCombatRoom.GenerateBackgroundRngForCurrentPoint(val.RunState));
			if (val4 == null)
			{
				throw new InvalidOperationException("CreateBackground returned null");
			}
			StringName name = ((background != null) ? ((Node)background).Name : null) ?? ((Node)val4).Name;
			((Node)val4).Name = StringName.op_Implicit("SkinChangerPendingCombatBackground");
			val3.AddChild((Node)(object)val4, false, (InternalMode)0);
			if (background != null && GodotObject.IsInstanceValid((GodotObject)(object)background))
			{
				val3.MoveChild((Node)(object)val4, ((Node)background).GetIndex(false));
			}
			CombatBackgroundField.SetValue(instance, val4);
			if (background != null && GodotObject.IsInstanceValid((GodotObject)(object)background))
			{
				Node parent = ((Node)background).GetParent();
				if (parent != null)
				{
					parent.RemoveChild((Node)(object)background);
				}
				((Node)background).QueueFree();
			}
			((Node)val4).Name = name;
			ModLog.Info("已按当前遭遇重新加载战斗背景：" + ((AbstractModel)val2).Id.Entry + "。");
			return true;
		}
		catch (Exception ex)
		{
			if (val4 != null && GodotObject.IsInstanceValid((GodotObject)(object)val4))
			{
				Node parent2 = ((Node)val4).GetParent();
				if (parent2 != null)
				{
					parent2.RemoveChild((Node)(object)val4);
				}
				((Node)val4).QueueFree();
			}
			CombatBackgroundField.SetValue(instance, background);
			ModLog.Warn("重新加载当前战斗背景失败，已保留原背景：" + ex.GetBaseException().Message);
			return false;
		}
	}

	internal static bool RefreshPlayerAppearance(ulong playerNetId)
	{
		bool result = MultiplayerTreasureHandAppearance.RefreshPlayer(playerNetId);
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return result;
		}
		bool flag = false;
		bool flag2 = false;
		NCreature[] array = instance.CreatureNodes.Where(delegate(NCreature creature)
		{
			Player player = creature.Entity.Player;
			if (player == null || player.NetId != playerNetId)
			{
				Player petOwner = creature.Entity.PetOwner;
				if (petOwner == null)
				{
					return false;
				}
				return petOwner.NetId == playerNetId;
			}
			return true;
		}).ToArray();
		foreach (NCreature creature2 in array)
		{
			if (!HasActiveDeathAnimation(creature2))
			{
				if (TryRebuildCreatureVisuals(creature2, out string error))
				{
					flag = true;
					continue;
				}
				flag2 = true;
				ModLog.Warn($"刷新联机玩家 {playerNetId} 的外观失败：{error}");
			}
		}
		if (flag)
		{
			RefreshPlayerAndPetLayout(instance);
		}
		if (flag)
		{
			return !flag2;
		}
		return false;
	}

	internal static bool RefreshPlayerTransforms(ulong playerNetId)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance == null)
		{
			return false;
		}
		int num = 0;
		NCreature[] array = instance.CreatureNodes.Where(delegate(NCreature creature)
		{
			Player player = creature.Entity.Player;
			if (player == null || player.NetId != playerNetId)
			{
				Player petOwner = creature.Entity.PetOwner;
				if (petOwner == null)
				{
					return false;
				}
				return petOwner.NetId == playerNetId;
			}
			return true;
		}).ToArray();
		foreach (NCreature creature2 in array)
		{
			if (!HasActiveDeathAnimation(creature2))
			{
				ApplyStoredTransform(creature2);
				num++;
			}
		}
		if (num > 0)
		{
			ContextualSkinControls.RefreshMultiplayerPlayerIcons(playerNetId);
		}
		return num > 0;
	}

	internal static void CapturePlayerAndPetLayout(float scaling, bool fullyCenterPlayers)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		if (instance != null)
		{
			_playerLayoutRoom = new WeakReference<NCombatRoom>(instance);
			_playerLayoutScaling = scaling;
			_fullyCenterPlayers = fullyCenterPlayers;
		}
	}

	private static void RefreshPlayerAndPetLayout(NCombatRoom room)
	{
		try
		{
			if (_playerLayoutRoom == null || !_playerLayoutRoom.TryGetTarget(out NCombatRoom target) || target != room)
			{
				return;
			}
			List<NCreature> list = room.CreatureNodes.Where((NCreature creature) => creature.Entity.IsPlayer || creature.Entity.PetOwner != null).ToList();
			if (list.Count == 0)
			{
				return;
			}
			NCombatRoom.PositionPlayersAndPets(list, _playerLayoutScaling, _fullyCenterPlayers);
			foreach (NCreature item in list)
			{
				RefreshCreatureAnchors(item);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新实战玩家与宠物位置失败：" + ex.GetBaseException().Message);
		}
	}

	private static bool TryRebuildCreatureVisuals(NCreature creature, out string? error)
	{
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		error = null;
		if (VisualsField == null || SpineAnimatorField == null)
		{
			error = "game visual fields unavailable";
			return false;
		}
		using (MultiplayerSkinSync.BeginCreatureRuntimeScope(creature.Entity))
		{
			NCreatureVisuals visuals2 = creature.Visuals;
			StringName name = ((Node)visuals2).Name;
			object value = SpineAnimatorField.GetValue(creature);
			Node parent = ((Node)visuals2).GetParent();
			int index = ((Node)visuals2).GetIndex(false);
			NCreatureVisuals visuals3 = null;
			List<Node> list = new List<Node>();
			try
			{
				Player player = creature.Entity.Player;
				CharacterModel playerCharacter = ((player != null) ? player.Character : null);
				SkinGroup skinGroup = ((playerCharacter == null) ? null : ContextualSkinControls.FindGroup(((AbstractModel)playerCharacter).Id.Entry, ((object)playerCharacter).GetType().Name));
				string text = ((playerCharacter == null) ? null : ContextualSkinControls.CanonicalScenePath("creature_visuals/" + ((AbstractModel)playerCharacter).Id.Entry.ToLowerInvariant()));
				if (playerCharacter == null || skinGroup == null || text == null || !SkinService.TryInstantiateSelectedCharacterCreatureVisuals(skinGroup.Id, text, delegate(NCreatureVisuals visuals)
				{
					ContextualSkinControls.ApplySelectedProviderVisualPostfix(((AbstractModel)playerCharacter).Id.Entry, ((object)playerCharacter).GetType().Name, playerCharacter, ref visuals);
					NCreatureVisuals visuals4 = visuals;
					ContextualSkinControls.ApplySelectedCreatureVisualPostfix(creature.Entity, ref visuals4);
					visuals = visuals4 ?? visuals;
					return visuals;
				}, out visuals3))
				{
					visuals3 = creature.Entity.CreateVisuals() ?? throw new InvalidOperationException("CreateVisuals returned null");
				}
				if (visuals3 == null)
				{
					throw new InvalidOperationException("selected character visual factory returned null");
				}
				((Node)visuals3).Name = ((Node)visuals2).Name;
				((Node2D)visuals3).Position = Vector2.Zero;
				((CanvasItem)visuals3).Visible = ((CanvasItem)visuals2).Visible;
				((CanvasItem)visuals3).Modulate = ((CanvasItem)visuals2).Modulate;
				((CanvasItem)visuals3).SelfModulate = ((CanvasItem)visuals2).SelfModulate;
				((CanvasItem)visuals3).ZIndex = ((CanvasItem)visuals2).ZIndex;
				((CanvasItem)visuals3).ZAsRelative = ((CanvasItem)visuals2).ZAsRelative;
				Variant meta = ((GodotObject)visuals2).GetMeta(StringName.op_Implicit("skin_changer_character_base_visual_scale"), Variant.op_Implicit(((Node2D)visuals2).Scale));
				Vector2 divisor = ((Variant)(ref meta)).AsVector2();
				meta = ((GodotObject)visuals2).GetMeta(StringName.op_Implicit("skin_changer_character_base_default_scale"), Variant.op_Implicit(visuals2.DefaultScale));
				float num = ((Variant)(ref meta)).AsSingle();
				bool num2 = !ContextualSkinControls.IsMonsterScaleManaged(visuals2);
				Vector2 val = (num2 ? SafeDivide(((Node2D)visuals2).Scale, divisor) : Vector2.One);
				float num3 = ((num2 && !Mathf.IsZeroApprox(num)) ? (visuals2.DefaultScale / num) : 1f);
				AttachReplacementCreatureVisuals(creature, parent, visuals2, visuals3);
				((Node)creature).MoveChild((Node)(object)visuals3, (parent == creature) ? index : parent.GetIndex(false));
				CreatureAnimator val2 = null;
				if (visuals3.HasSpineAnimation)
				{
					val2 = (CreatureAnimator)((creature.Entity.Player != null) ? ((object)GenerateCharacterAnimator(creature.Entity.Player.Character, visuals3.SpineBody, creature.Entity)) : ((object)creature.Entity.Monster.GenerateAnimator(visuals3.SpineBody)));
					if (creature.Entity.Monster != null)
					{
						visuals3.SetUpSkin(creature.Entity.Monster);
					}
				}
				VisualsField.SetValue(creature, visuals3);
				SpineAnimatorField.SetValue(creature, val2);
				ConnectSpineAnimatorSignalsMethod?.Invoke(creature, null);
				UpdatePhobiaModeMethod?.Invoke(creature, null);
				ReplaySelectedCreatureReady(creature);
				RestoreDeadCreaturePose(creature, val2);
				CaptureVisualBaseline(visuals3);
				meta = ((GodotObject)visuals3).GetMeta(StringName.op_Implicit("skin_changer_character_base_visual_scale"), Variant.op_Implicit(((Node2D)visuals3).Scale));
				Vector2 val3 = ((Variant)(ref meta)).AsVector2();
				meta = ((GodotObject)visuals3).GetMeta(StringName.op_Implicit("skin_changer_character_base_default_scale"), Variant.op_Implicit(visuals3.DefaultScale));
				float num4 = ((Variant)(ref meta)).AsSingle();
				((Node2D)visuals3).Scale = val3 * val;
				visuals3.DefaultScale = num4 * num3;
				MigrateFormVfx(visuals2, visuals3, list);
				ApplyStoredTransform(creature);
				if (creature.Entity.IsPlayer)
				{
					RebuildCurrentOrbVisuals(creature);
				}
				Node parent2 = ((Node)visuals2).GetParent();
				if (parent2 != null)
				{
					parent2.RemoveChild((Node)(object)visuals2);
				}
				((Node)visuals3).Name = name;
				((Node)visuals2).QueueFree();
				CreatureIntentDepth.Apply(creature);
				return true;
			}
			catch (Exception ex2)
			{
				try
				{
					VisualsField.SetValue(creature, visuals2);
					SpineAnimatorField.SetValue(creature, value);
					foreach (Node item in ((IEnumerable<Node>)list).Where((Func<Node, bool>)GodotObject.IsInstanceValid))
					{
						Control nodeOrNull = ((Node)visuals2).GetNodeOrNull<Control>(NodePath.op_Implicit("%FormVfx"));
						if (nodeOrNull != null)
						{
							item.Reparent((Node)(object)nodeOrNull, true);
						}
					}
					if (visuals3 != null && GodotObject.IsInstanceValid((GodotObject)(object)visuals3))
					{
						Node parent3 = ((Node)visuals3).GetParent();
						if (parent3 != null)
						{
							parent3.RemoveChild((Node)(object)visuals3);
						}
						((Node)visuals3).QueueFree();
					}
					if (((Node)visuals2).GetParent() == null)
					{
						parent.AddChild((Node)(object)visuals2, false, (InternalMode)0);
						parent.MoveChild((Node)(object)visuals2, index);
					}
					ConnectSpineAnimatorSignalsMethod?.Invoke(creature, null);
					RefreshCreatureAnchors(creature);
				}
				catch (Exception ex)
				{
					ModLog.Error("回滚实战角色外观失败：" + ex);
				}
				error = ex2.GetBaseException().Message;
				ModLog.Error($"重建 {creature.Entity.ModelId.Entry} 的实战外观失败：{ex2}");
				return false;
			}
		}
	}

	private static void RestoreDeadCreaturePose(NCreature creature, CreatureAnimator? animator)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!creature.Entity.IsDead || animator == null || !animator.HasTrigger("Dead"))
		{
			return;
		}
		creature.SetAnimationTrigger("Dead");
		SpineAnimationAccess spineAnimation = creature.SpineAnimation;
		MegaTrackEntry currentTrack = ((SpineAnimationAccess)(ref spineAnimation)).GetCurrentTrack(0);
		try
		{
			if (currentTrack != null)
			{
				currentTrack.SetTrackTime(currentTrack.GetAnimationEnd());
			}
		}
		finally
		{
			(currentTrack as IDisposable)?.Dispose();
		}
	}

	private static void AttachReplacementCreatureVisuals(NCreature creature, Node previousParent, NCreatureVisuals previousVisuals, NCreatureVisuals replacement)
	{
		previousParent.RemoveChild((Node)(object)previousVisuals);
		VisualsField.SetValue(creature, replacement);
		((Node)creature).AddChild((Node)(object)replacement, false, (InternalMode)0);
	}

	private static Node2D? EnsureTransformWrapper(NCreature creature)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals))
		{
			return null;
		}
		Node2D val = GetTransformWrapper(creature);
		if (val == null)
		{
			val = new Node2D
			{
				Name = StringName.op_Implicit("SkinChangerCharacterTransform")
			};
			int index = ((Node)creature.Visuals).GetIndex(false);
			((Node)creature).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)creature).MoveChild((Node)(object)val, Math.Max(0, index));
		}
		if (((Node)creature.Visuals).GetParent() != val)
		{
			CreatureVisualParentBridge.Register(creature, val);
			((Node)creature.Visuals).Reparent((Node)(object)val, false);
		}
		return val;
	}

	private static Node2D? EnsureHealthDisplayWrapper(NCreature creature)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		NCreatureStateDisplay stateDisplay = GetStateDisplay(creature);
		if (stateDisplay == null || !GodotObject.IsInstanceValid((GodotObject)(object)stateDisplay))
		{
			return null;
		}
		Node2D val = GetHealthDisplayWrapper(creature);
		if (val == null)
		{
			val = new Node2D
			{
				Name = StringName.op_Implicit("SkinChangerHealthDisplayTransform")
			};
			int index = ((Node)stateDisplay).GetIndex(false);
			((Node)creature).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)creature).MoveChild((Node)(object)val, Math.Max(0, index));
		}
		if (((Node)stateDisplay).GetParent() != val)
		{
			((Node)stateDisplay).Reparent((Node)(object)val, false);
		}
		return val;
	}

	private static Node2D? EnsureIntentDisplayWrapper(NCreature creature)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		Control intentContainer = creature.IntentContainer;
		if (intentContainer == null || !GodotObject.IsInstanceValid((GodotObject)(object)intentContainer))
		{
			return null;
		}
		Node2D val = GetIntentDisplayWrapper(creature);
		if (val == null)
		{
			val = new Node2D
			{
				Name = StringName.op_Implicit("SkinChangerIntentDisplayTransform")
			};
			int index = ((Node)intentContainer).GetIndex(false);
			((Node)creature).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)creature).MoveChild((Node)(object)val, Math.Max(0, index));
		}
		if (((Node)intentContainer).GetParent() != val)
		{
			((Node)intentContainer).Reparent((Node)(object)val, false);
		}
		return val;
	}

	private static Control? EnsureHealthBoundsProxy(NCreature creature)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		Control nodeOrNull = ((Node)creature).GetNodeOrNull<Control>(NodePath.op_Implicit("SkinChangerHealthBoundsProxy"));
		if (nodeOrNull != null)
		{
			return nodeOrNull;
		}
		nodeOrNull = new Control
		{
			Name = StringName.op_Implicit("SkinChangerHealthBoundsProxy"),
			MouseFilter = (MouseFilterEnum)2,
			Visible = false
		};
		((Node)creature).AddChild((Node)(object)nodeOrNull, false, (InternalMode)0);
		return nodeOrNull;
	}

	private static void SetCurrentTransform(Node2D wrapper, CharacterCombatTransform value)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_character_current_scale"), Variant.op_Implicit(value.Scale));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_character_current_offset_x"), Variant.op_Implicit(value.OffsetX));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_character_current_offset_y"), Variant.op_Implicit(value.OffsetY));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_health_current_scale"), Variant.op_Implicit(value.HealthBarScale));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_health_current_offset_x"), Variant.op_Implicit(value.HealthBarOffsetX));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_health_current_offset_y"), Variant.op_Implicit(value.HealthBarOffsetY));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_health_follow_scale"), Variant.op_Implicit(value.HealthBarFollowsModelScale));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_health_follow_movement"), Variant.op_Implicit(value.HealthBarFollowsModelMovement));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_intent_current_scale"), Variant.op_Implicit(value.IntentScale));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_intent_current_offset_x"), Variant.op_Implicit(value.IntentOffsetX));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_intent_current_offset_y"), Variant.op_Implicit(value.IntentOffsetY));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_intent_follow_scale"), Variant.op_Implicit(value.IntentFollowsModelScale));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_intent_follow_movement"), Variant.op_Implicit(value.IntentFollowsModelMovement));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_reticle_current_scale"), Variant.op_Implicit(value.SelectionReticleScale));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_reticle_current_offset_x"), Variant.op_Implicit(value.SelectionReticleOffsetX));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_reticle_current_offset_y"), Variant.op_Implicit(value.SelectionReticleOffsetY));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_reticle_follow_scale"), Variant.op_Implicit(value.SelectionReticleFollowsModelScale));
		((GodotObject)wrapper).SetMeta(StringName.op_Implicit("skin_changer_reticle_follow_movement"), Variant.op_Implicit(value.SelectionReticleFollowsModelMovement));
	}

	private static CharacterCombatTransform GetCurrentTransform(Node2D wrapper)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		Variant meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_character_current_scale"), Variant.op_Implicit(1f));
		float scale = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_character_current_offset_x"), Variant.op_Implicit(0f));
		float offsetX = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_character_current_offset_y"), Variant.op_Implicit(0f));
		CharacterCombatTransform characterCombatTransform = new CharacterCombatTransform(scale, offsetX, ((Variant)(ref meta)).AsSingle());
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_health_current_scale"), Variant.op_Implicit(1f));
		characterCombatTransform.HealthBarScale = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_health_current_offset_x"), Variant.op_Implicit(0f));
		characterCombatTransform.HealthBarOffsetX = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_health_current_offset_y"), Variant.op_Implicit(0f));
		characterCombatTransform.HealthBarOffsetY = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_health_follow_scale"), Variant.op_Implicit(false));
		characterCombatTransform.HealthBarFollowsModelScale = ((Variant)(ref meta)).AsBool();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_health_follow_movement"), Variant.op_Implicit(true));
		characterCombatTransform.HealthBarFollowsModelMovement = ((Variant)(ref meta)).AsBool();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_intent_current_scale"), Variant.op_Implicit(1f));
		characterCombatTransform.IntentScale = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_intent_current_offset_x"), Variant.op_Implicit(0f));
		characterCombatTransform.IntentOffsetX = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_intent_current_offset_y"), Variant.op_Implicit(0f));
		characterCombatTransform.IntentOffsetY = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_intent_follow_scale"), Variant.op_Implicit(false));
		characterCombatTransform.IntentFollowsModelScale = ((Variant)(ref meta)).AsBool();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_intent_follow_movement"), Variant.op_Implicit(true));
		characterCombatTransform.IntentFollowsModelMovement = ((Variant)(ref meta)).AsBool();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_reticle_current_scale"), Variant.op_Implicit(1f));
		characterCombatTransform.SelectionReticleScale = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_reticle_current_offset_x"), Variant.op_Implicit(0f));
		characterCombatTransform.SelectionReticleOffsetX = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_reticle_current_offset_y"), Variant.op_Implicit(0f));
		characterCombatTransform.SelectionReticleOffsetY = ((Variant)(ref meta)).AsSingle();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_reticle_follow_scale"), Variant.op_Implicit(true));
		characterCombatTransform.SelectionReticleFollowsModelScale = ((Variant)(ref meta)).AsBool();
		meta = ((GodotObject)wrapper).GetMeta(StringName.op_Implicit("skin_changer_reticle_follow_movement"), Variant.op_Implicit(true));
		characterCombatTransform.SelectionReticleFollowsModelMovement = ((Variant)(ref meta)).AsBool();
		return characterCombatTransform;
	}

	private static void ApplyHealthDisplayTransform(Node2D wrapper, CharacterCombatTransform value)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = (Vector2)(value.HealthBarFollowsModelMovement ? new Vector2(value.OffsetX, value.OffsetY) : Vector2.Zero);
		wrapper.Position = val + new Vector2(value.HealthBarOffsetX, value.HealthBarOffsetY);
		float num = value.HealthBarScale * (value.HealthBarFollowsModelScale ? value.Scale : 1f);
		wrapper.Scale = Vector2.One * num;
	}

	private static void ApplyIntentDisplayTransform(Node2D wrapper, CharacterCombatTransform value, Vector2 baseMarkerPosition)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		float num = (value.IntentFollowsModelScale ? value.Scale : 1f);
		Vector2 val = (Vector2)(value.IntentFollowsModelMovement ? new Vector2(value.OffsetX, value.OffsetY) : Vector2.Zero);
		wrapper.Position = baseMarkerPosition * num + val + new Vector2(value.IntentOffsetX, value.IntentOffsetY);
		wrapper.Scale = Vector2.One * value.IntentScale * num;
	}

	private static void ApplySelectionReticleAndHitbox(NSelectionReticle reticle, Control hitbox, CharacterCombatTransform value, Vector2 basePosition, Vector2 baseSize)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		float num = (value.SelectionReticleFollowsModelScale ? value.Scale : 1f);
		Vector2 val = (Vector2)(value.SelectionReticleFollowsModelMovement ? new Vector2(value.OffsetX, value.OffsetY) : Vector2.Zero);
		Vector2 position = basePosition * num + val + new Vector2(value.SelectionReticleOffsetX, value.SelectionReticleOffsetY);
		Vector2 val2 = baseSize * value.SelectionReticleScale * num;
		((Control)reticle).Position = position;
		((Control)reticle).Size = val2;
		((Control)reticle).PivotOffset = val2 * 0.5f;
		hitbox.Position = position;
		hitbox.Size = val2;
		hitbox.PivotOffset = val2 * 0.5f;
	}

	private static void CaptureVisualBaseline(NCreatureVisuals visuals)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)visuals).HasMeta(StringName.op_Implicit("skin_changer_character_base_visual_scale")))
		{
			((GodotObject)visuals).SetMeta(StringName.op_Implicit("skin_changer_character_base_visual_scale"), Variant.op_Implicit(((Node2D)visuals).Scale));
		}
		if (!((GodotObject)visuals).HasMeta(StringName.op_Implicit("skin_changer_character_base_default_scale")))
		{
			((GodotObject)visuals).SetMeta(StringName.op_Implicit("skin_changer_character_base_default_scale"), Variant.op_Implicit(visuals.DefaultScale));
		}
	}

	private static void RefreshCreatureAnchors(NCreature creature)
	{
		UpdateBoundsMethod?.Invoke(creature, new object[1] { creature.Visuals });
		SetOrbManagerPositionMethod?.Invoke(creature, null);
	}

	private static void ReplaySelectedCreatureReady(NCreature creature)
	{
		IReadOnlyList<string> selectedCreatureProviderIds = GetSelectedCreatureProviderIds(creature);
		foreach (string item in selectedCreatureProviderIds)
		{
			ManagedSkinModLoader.ReplaySelectedCreatureReady(item, creature);
		}
		ReplaySelectedCreatureNodeReady(creature, selectedCreatureProviderIds);
		foreach (string item2 in selectedCreatureProviderIds)
		{
			ManagedSkinModLoader.ReplaySelectedCompanionReveal(item2, creature);
		}
	}

	internal static void ReplaySelectedCreatureNodeReady(NCreature creature)
	{
		ReplaySelectedCreatureNodeReady(creature, GetSelectedCreatureProviderIds(creature));
	}

	private static void ReplaySelectedCreatureNodeReady(NCreature creature, IReadOnlyList<string> providerIds)
	{
		Player player = creature.Entity.Player;
		object obj = ((player != null) ? player.Character : null);
		if (obj == null)
		{
			Player petOwner = creature.Entity.PetOwner;
			obj = ((petOwner != null) ? petOwner.Character : null);
		}
		CharacterModel val = (CharacterModel)obj;
		string text = ((val == null) ? null : ContextualSkinControls.FindGroup(((AbstractModel)val).Id.Entry, ((object)val).GetType().Name)?.Id);
		using ((text == null) ? null : ManualCharacterVariantBridge.BeginScope(text))
		{
			ManagedSkinModLoader.RestoreUnselectedNodeReadyBehaviors((Node)(object)creature, providerIds);
			foreach (string providerId in providerIds)
			{
				ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(providerId, (Node)(object)creature);
			}
			ManagedSlotVisibilityBridge.BindPlayerScene((Node)(object)creature.Visuals, creature.Entity.Player);
		}
	}

	private static IReadOnlyList<string> GetSelectedCreatureProviderIds(NCreature creature)
	{
		List<string> list = new List<string>();
		string entry = creature.Entity.ModelId.Entry;
		Player player = creature.Entity.Player;
		string modelTypeName = ((player != null) ? ((object)player.Character).GetType().Name : null) ?? ((object)creature.Entity.Monster)?.GetType().Name;
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(entry, modelTypeName);
		if (skinGroup != null)
		{
			AddSelectedCreatureRuntimeProvider(list, skinGroup.Id);
		}
		Player petOwner = creature.Entity.PetOwner;
		if (petOwner != null)
		{
			CharacterModel character = petOwner.Character;
			SkinGroup skinGroup2 = ContextualSkinControls.FindGroup(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name);
			if (skinGroup2 != null)
			{
				AddSelectedCreatureRuntimeProvider(list, skinGroup2.Id);
			}
		}
		return list;
	}

	private static void AddSelectedCreatureRuntimeProvider(ICollection<string> providerIds, string groupId)
	{
		string selectedCreatureRuntimeProvider = SkinService.GetSelectedCreatureRuntimeProvider(groupId);
		if (selectedCreatureRuntimeProvider != null && !providerIds.Contains<string>(selectedCreatureRuntimeProvider, StringComparer.OrdinalIgnoreCase))
		{
			providerIds.Add(selectedCreatureRuntimeProvider);
		}
	}

	private static void RebuildCurrentOrbVisuals(NCreature creature)
	{
		try
		{
			if (creature.OrbManager == null || !(OrbNodesField?.GetValue(creature.OrbManager) is IEnumerable<NOrb> source))
			{
				return;
			}
			NOrb[] array = source.ToArray();
			foreach (NOrb val in array)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)val) && val.Model != null)
				{
					val.ReplaceOrb(val.Model);
				}
			}
			creature.OrbManager.UpdateVisuals((OrbEvokeType)0);
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新角色充能球外观失败：" + ex.GetBaseException().Message);
		}
	}

	private static void MigrateFormVfx(NCreatureVisuals oldVisuals, NCreatureVisuals newVisuals, ICollection<Node> movedChildren)
	{
		Control nodeOrNull = ((Node)oldVisuals).GetNodeOrNull<Control>(NodePath.op_Implicit("%FormVfx"));
		Control nodeOrNull2 = ((Node)newVisuals).GetNodeOrNull<Control>(NodePath.op_Implicit("%FormVfx"));
		if (nodeOrNull == null || nodeOrNull2 == null)
		{
			return;
		}
		foreach (Node child in ((Node)nodeOrNull).GetChildren(false))
		{
			movedChildren.Add(child);
			child.Reparent((Node)(object)nodeOrNull2, true);
		}
	}

	private static Vector2 SafeDivide(Vector2 value, Vector2 divisor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(Mathf.IsZeroApprox(divisor.X) ? 1f : (value.X / divisor.X), Mathf.IsZeroApprox(divisor.Y) ? 1f : (value.Y / divisor.Y));
	}

	private static CreatureAnimator GenerateCharacterAnimator(object character, object spineBody, object creature)
	{
		MethodInfo[] array = (from method in character.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
			where method.Name == "GenerateAnimator"
			orderby method.GetParameters().Length descending
			select method).ToArray();
		foreach (MethodInfo methodInfo in array)
		{
			object[] array2 = methodInfo.GetParameters().Length switch
			{
				2 => new object[2] { spineBody, creature }, 
				1 => new object[1] { spineBody }, 
				_ => Array.Empty<object>(), 
			};
			if (array2.Length != 0)
			{
				object? obj = methodInfo.Invoke(character, array2);
				CreatureAnimator val = (CreatureAnimator)((obj is CreatureAnimator) ? obj : null);
				if (val != null)
				{
					return val;
				}
			}
		}
		throw new MissingMethodException(character.GetType().FullName, "GenerateAnimator");
	}

	private static void RefreshCurrentCharacterUi()
	{
		Player localPlayer = GetLocalPlayer();
		NRun instance = NRun.Instance;
		if (localPlayer == null || instance == null)
		{
			return;
		}
		try
		{
			NTopBarPortrait portrait = instance.GlobalUi.TopBar.Portrait;
			foreach (Node child in ((Node)portrait).GetChildren(false))
			{
				((Node)portrait).RemoveChild(child);
				child.QueueFree();
			}
			((Node)portrait).AddChild((Node)(object)localPlayer.Character.Icon, false, (InternalMode)0);
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新实战左上角角色头像失败：" + ex.GetBaseException().Message);
		}
		try
		{
			object? obj = MapMarkerField?.GetValue(NMapScreen.Instance);
			TextureRect val = (TextureRect)((obj is TextureRect) ? obj : null);
			if (val != null)
			{
				val.Texture = (Texture2D)(object)localPlayer.Character.MapMarker;
			}
		}
		catch (Exception ex2)
		{
			ModLog.Warn("刷新地图角色标记失败：" + ex2.GetBaseException().Message);
		}
		RefreshVisibleRelicIcons(instance);
		RefreshCurrentEnergyCounter(localPlayer);
	}

	internal static void RefreshCurrentBossPresentation(IReadOnlySet<string> affectedGroups)
	{
		NRun instance = NRun.Instance;
		if (instance == null || !TryGetRunState(instance, out IRunState runState))
		{
			return;
		}
		HashSet<string> hashSet = ResolveCurrentBossGroupIds(runState);
		if (!hashSet.Overlaps(affectedGroups))
		{
			return;
		}
		try
		{
			string[] selectedProviders = (from providerId in hashSet.Select(SkinService.GetSelectedRuntimeProvider)
				where providerId != null
				select providerId).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
			NMapScreen instance2 = NMapScreen.Instance;
			if (instance2 != null)
			{
				foreach (NBossMapPoint item in EnumerateDescendants<NBossMapPoint>((Node)(object)instance2))
				{
					RefreshBossMapPointPresentation(item, selectedProviders, refreshNativePresentation: true);
				}
			}
			NTopBarBossIcon bossIcon = instance.GlobalUi.TopBar.BossIcon;
			bossIcon.RefreshBossIcon();
			TopBarRoomIconRefreshMethod?.Invoke(instance.GlobalUi.TopBar.RoomIcon, null);
			NHoverTipSet.Remove((Control)(object)bossIcon);
			ModLog.Info("已刷新当前阶段 Boss 的地图图标、顶部图标、房间图标和名称；受影响外观组=" + string.Join(",", hashSet.Intersect(affectedGroups)) + "。");
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新当前阶段 Boss 地图/顶部外观失败：" + ex.GetBaseException().Message);
		}
	}

	internal static void RefreshBossMapPointPresentation(NBossMapPoint bossPoint)
	{
		try
		{
			if (TryGetRunState(NRun.Instance, out IRunState runState))
			{
				string[] selectedProviders = (from providerId in ResolveCurrentBossGroupIds(runState).Select(SkinService.GetSelectedRuntimeProvider)
					where providerId != null
					select providerId).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
				RefreshBossMapPointPresentation(bossPoint, selectedProviders, refreshNativePresentation: false);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("初始化 Boss 地图皮肤失败，保留游戏当前图标：" + ex.GetBaseException().Message);
		}
	}

	private static void RefreshBossMapPointPresentation(NBossMapPoint bossPoint, IEnumerable<string> selectedProviders, bool refreshNativePresentation)
	{
		ManagedSkinModLoader.RestoreAllNodeReadyBehaviors((Node)(object)bossPoint);
		if (refreshNativePresentation && TryGetRunState(NRun.Instance, out IRunState runState))
		{
			RefreshNativeBossMapPoint(bossPoint, runState);
		}
		foreach (string selectedProvider in selectedProviders)
		{
			ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(selectedProvider, (Node)(object)bossPoint);
		}
	}

	internal static void RefreshNativeBossMapPoint(NBossMapPoint bossPoint, IRunState runState)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		EncounterModel val = ((((NMapPoint)bossPoint).Point == runState.Map.SecondBossMapPoint) ? runState.Act.SecondBossEncounter : runState.Act.BossEncounter);
		if (val == null)
		{
			return;
		}
		Node2D node = ((Node)bossPoint).GetNode<Node2D>(NodePath.op_Implicit("%SpineSprite"));
		TextureRect node2 = ((Node)bossPoint).GetNode<TextureRect>(NodePath.op_Implicit("%PlaceholderImage"));
		TextureRect node3 = ((Node)bossPoint).GetNode<TextureRect>(NodePath.op_Implicit("%PlaceholderOutline"));
		Variant meta = ((GodotObject)bossPoint).GetMeta(StringName.op_Implicit("skin_changer_boss_map_refresh_generation"), Variant.op_Implicit(0L));
		long generation = ((Variant)(ref meta)).AsInt64() + 1;
		((GodotObject)bossPoint).SetMeta(StringName.op_Implicit("skin_changer_boss_map_refresh_generation"), Variant.op_Implicit(generation));
		MegaSkeletonDataResource bossNodeSpineResource = val.BossNodeSpineResource;
		if (bossNodeSpineResource != null)
		{
			((CanvasItem)node2).Visible = false;
			((CanvasItem)node3).Visible = false;
			((CanvasItem)node).Visible = true;
			MegaSprite animator = default(MegaSprite);
			ref MegaSprite reference = ref animator;
			object? obj = BossMapAnimatorField?.GetValue(bossPoint);
			reference = (MegaSprite)(((obj is MegaSprite) ? obj : null) ?? ((object)new MegaSprite(Variant.op_Implicit((GodotObject)(object)node))));
			animator.SetSkeletonDataRes(bossNodeSpineResource);
			BossMapAnimatorField?.SetValue(bossPoint, animator);
			BossMapUsesSpineField?.SetValue(bossPoint, true);
			Material normalMaterial = animator.GetNormalMaterial();
			ShaderMaterial val2 = (ShaderMaterial)(object)((normalMaterial is ShaderMaterial) ? normalMaterial : null);
			if (val2 != null)
			{
				BossMapMaterialField?.SetValue(bossPoint, val2);
			}
			SpineNodeExtensions.RunWhenSpineReady((Node)(object)bossPoint, animator, (Action<MegaAnimationState>)delegate(MegaAnimationState animationState)
			{
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				if (GodotObject.IsInstanceValid((GodotObject)(object)bossPoint))
				{
					Variant meta2 = ((GodotObject)bossPoint).GetMeta(StringName.op_Implicit("skin_changer_boss_map_refresh_generation"), Variant.op_Implicit(0L));
					if (((Variant)(ref meta2)).AsInt64() == generation)
					{
						Material normalMaterial2 = animator.GetNormalMaterial();
						ShaderMaterial val3 = (ShaderMaterial)(object)((normalMaterial2 is ShaderMaterial) ? normalMaterial2 : null);
						if (val3 != null)
						{
							BossMapMaterialField?.SetValue(bossPoint, val3);
						}
						animationState.SetAnimation("animation", true, 0);
						BossMapRefreshColorMethod?.Invoke(bossPoint, null);
					}
				}
			});
		}
		else
		{
			((CanvasItem)node).Visible = false;
			node2.Texture = PreloadManager.Cache.GetAsset<Texture2D>(val.BossNodePath + ".png");
			node3.Texture = PreloadManager.Cache.GetAsset<Texture2D>(val.BossNodePath + "_outline.png");
			((CanvasItem)node2).Visible = true;
			((CanvasItem)node3).Visible = true;
			BossMapPlaceholderImageField?.SetValue(bossPoint, node2);
			BossMapPlaceholderOutlineField?.SetValue(bossPoint, node3);
			BossMapUsesSpineField?.SetValue(bossPoint, false);
			BossMapRefreshColorMethod?.Invoke(bossPoint, null);
		}
	}

	private static void RefreshVisibleRelicIcons(NRun run)
	{
		foreach (NRelic item in EnumerateDescendants<NRelic>((Node)(object)run))
		{
			try
			{
				item.Model = item.Model;
			}
			catch (Exception ex)
			{
				ModLog.Warn("刷新框架遗物图标失败：" + ex.GetBaseException().Message);
			}
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateDescendants_003Ed__144<>))]
	private static IEnumerable<TNode> EnumerateDescendants<TNode>(Node root) where TNode : Node
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateDescendants_003Ed__144<TNode>(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static void RefreshCurrentEnergyCounter(Player player)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		NCombatUi val = ((instance != null) ? instance.Ui : null);
		if (val != null)
		{
			RefreshEnergyCounter(val, player);
		}
	}

	internal static void RefreshEnergyCounter(NCombatUi combatUi, Player player)
	{
		try
		{
			using (CombatHudResourceLifecycle.BeforeEnergyCounterReplacement(combatUi))
			{
				ReplaceEnergyCounterCore(combatUi, player);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新实战能量界面失败，未释放无法安全交接的原控件：" + ex.GetBaseException().Message);
		}
	}

	private static void ReplaceEnergyCounterCore(NCombatUi combatUi, Player player)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		object? obj = EnergyCounterField?.GetValue(combatUi);
		NEnergyCounter val = (NEnergyCounter)((obj is NEnergyCounter) ? obj : null);
		if (val == null)
		{
			return;
		}
		NEnergyCounter val2 = null;
		NStarCounter val3 = null;
		int index = ((Node)val).GetIndex(false);
		bool flag = false;
		try
		{
			val2 = NEnergyCounter.Create(player);
			if (val2 != null)
			{
				StringName name2 = (((Node)val2).Name = ((Node)val).Name);
				((CanvasItem)val2).Visible = ((CanvasItem)val).Visible;
				((CanvasItem)val2).Modulate = ((CanvasItem)val).Modulate;
				((CanvasItem)val2).SelfModulate = ((CanvasItem)val).SelfModulate;
				((Node)combatUi.EnergyCounterContainer).AddChild((Node)(object)val2, false, (InternalMode)0);
				((Node)combatUi.EnergyCounterContainer).MoveChild((Node)(object)val2, index);
				object? obj2 = StarCounterField?.GetValue(combatUi);
				NStarCounter val4 = (NStarCounter)((obj2 is NStarCounter) ? obj2 : null);
				if (val4 != null)
				{
					val3 = val4;
					((Node)val3).Reparent((Node)(object)val2, true);
				}
				EnergyCounterField.SetValue(combatUi, val2);
				Node parent = ((Node)val).GetParent();
				if (parent != null)
				{
					parent.RemoveChild((Node)(object)val);
				}
				flag = true;
				((Node)val2).Name = name2;
				((Node)val).QueueFree();
			}
		}
		catch (Exception ex)
		{
			EnergyCounterField.SetValue(combatUi, val);
			if (val3 != null && GodotObject.IsInstanceValid((GodotObject)(object)val3) && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((Node)val3).Reparent((Node)(object)val, true);
			}
			if (flag && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((Node)combatUi.EnergyCounterContainer).AddChild((Node)(object)val, false, (InternalMode)0);
				((Node)combatUi.EnergyCounterContainer).MoveChild((Node)(object)val, index);
			}
			if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				Node parent2 = ((Node)val2).GetParent();
				if (parent2 != null)
				{
					parent2.RemoveChild((Node)(object)val2);
				}
				((Node)val2).QueueFree();
			}
			ModLog.Warn("刷新实战能量计数器失败：" + ex.GetBaseException().Message);
		}
	}
}
