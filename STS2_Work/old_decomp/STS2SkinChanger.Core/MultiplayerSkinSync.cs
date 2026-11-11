using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class MultiplayerSkinSync
{
	private sealed class SelectionScope : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				Stack<IReadOnlyDictionary<string, string>> selectionScopes = _selectionScopes;
				if (selectionScopes != null && selectionScopes.Count > 0)
				{
					selectionScopes.Pop();
				}
			}
		}
	}

	private sealed class CombinedScope(IDisposable selectionScope, IDisposable resourceScope) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				resourceScope.Dispose();
				selectionScope.Dispose();
			}
		}
	}

	internal const byte ProtocolVersion = 9;

	internal const int ReservedMessageId = 254;

	private const int MaximumSourceOptionCount = 64;

	private const int MaximumSourceOptionManifestLength = 32768;

	private static readonly byte[] CapabilityMagic = new byte[8] { 71, 83, 67, 65, 80, 48, 57, 33 };

	private static readonly HashSet<ulong> CapablePeers = new HashSet<ulong>();

	private static readonly Dictionary<ulong, SkinChangerNetMessage> AdvertisedSelections = new Dictionary<ulong, SkinChangerNetMessage>();

	private static readonly Dictionary<ulong, SessionCharacterSelection> AvailableSelections = new Dictionary<ulong, SessionCharacterSelection>();

	private static readonly Dictionary<(ulong PlayerId, string GroupId), string> LocalFallbackSelections = new Dictionary<(ulong, string), string>();

	private static readonly Dictionary<(ulong PlayerId, string TransformKey), CharacterCombatTransform> LocalFallbackTransforms = new Dictionary<(ulong, string), CharacterCombatTransform>();

	private static readonly HashSet<ulong> PendingRefreshes = new HashSet<ulong>();

	private static readonly HashSet<ulong> PendingTransformRefreshes = new HashSet<ulong>();

	private static readonly HashSet<ulong> PendingIconRefreshes = new HashSet<ulong>();

	private static readonly Dictionary<ulong, string> LastReceivedTransformSignatures = new Dictionary<ulong, string>();

	private static readonly Dictionary<ulong, string> LastAppliedTransformSignatures = new Dictionary<ulong, string>();

	private static readonly HashSet<string> MissingInstalledSkinWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly object Sync = new object();

	[ThreadStatic]
	private static Stack<IReadOnlyDictionary<string, string>>? _selectionScopes;

	private static INetGameService? _netService;

	private static StartRunLobby? _lobby;

	private static MessageHandlerDelegate<SkinChangerNetMessage>? _messageHandler;

	private static double _snapshotElapsed;

	private static int _snapshotStage;

	private static bool _runtimeProvidersDirty;

	private static double _appearanceRetryCooldown;

	private static bool _localTransformAdvertisementDirty;

	private static double _localTransformBroadcastCooldown;

	private static string? _lastSentTransformSignature;

	private static bool _inRun;

	private static bool _hasLobbySession;

	private static StartRunLobby? _suspendedLobby;

	private static bool _resumeRunWhenEnabled;

	private static bool _needsLobbyRoundReset;

	private static bool ShouldApplyRemoteAppearance()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		INetGameService? netService = _netService;
		if (netService == null || !NetGameTypeExtensions.IsMultiplayer(netService.Type))
		{
			return false;
		}
		return MultiplayerSkinSyncParticipationPolicy.Resolve(SkinService.ShouldSendMultiplayerSkinChanges(), SkinService.ShouldReceiveMultiplayerSkinChanges(), isMultiplayer: true).ApplyRemoteAppearance;
	}

	private static bool ShouldParticipate()
	{
		if (!SkinService.ShouldSendMultiplayerSkinChanges())
		{
			return SkinService.ShouldReceiveMultiplayerSkinChanges();
		}
		return true;
	}

	internal static string? GetScopedSelection(string groupId)
	{
		Stack<IReadOnlyDictionary<string, string>> selectionScopes = _selectionScopes;
		if (selectionScopes == null || selectionScopes.Count == 0)
		{
			return null;
		}
		return selectionScopes.Peek().GetValueOrDefault(groupId);
	}

	internal static IReadOnlyDictionary<string, string>? GetScopedSelections()
	{
		Stack<IReadOnlyDictionary<string, string>> selectionScopes = _selectionScopes;
		if (selectionScopes == null || selectionScopes.Count <= 0)
		{
			return null;
		}
		return selectionScopes.Peek();
	}

	internal static IReadOnlyList<IReadOnlyDictionary<string, string>> GetAvailableSelectionMaps()
	{
		if (!ShouldApplyRemoteAppearance())
		{
			return Array.Empty<IReadOnlyDictionary<string, string>>();
		}
		lock (Sync)
		{
			return AvailableSelections.Values.Select((SessionCharacterSelection selection) => selection.SelectionOverrides).ToArray();
		}
	}

	internal static string GetSelectionForCreature(Creature creature, string groupId)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldApplyRemoteAppearance())
		{
			return SkinService.Config.GetSelection(groupId);
		}
		Player val = creature.Player ?? creature.PetOwner;
		if (val == null || FrameworkModelPreview.IsPreviewPlayer(val))
		{
			return SkinService.Config.GetSelection(groupId);
		}
		if (TryGetPlayerSelectionMap(val.NetId, val.Character, out IReadOnlyDictionary<string, string> selections) && selections.TryGetValue(groupId, out var value))
		{
			return value;
		}
		INetGameService netService = _netService;
		if (netService != null && NetGameTypeExtensions.IsMultiplayer(netService.Type) && val.NetId != netService.NetId)
		{
			return "__base__";
		}
		return SkinService.Config.GetSelection(groupId);
	}

	internal static bool CanEditSkinForCreature(Creature creature)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldApplyRemoteAppearance())
		{
			return true;
		}
		Player val = creature.Player ?? creature.PetOwner;
		INetGameService netService = _netService;
		if (val == null || netService == null || !NetGameTypeExtensions.IsMultiplayer(netService.Type))
		{
			return true;
		}
		if (val.NetId == netService.NetId)
		{
			return true;
		}
		lock (Sync)
		{
			SessionCharacterSelection value;
			return !AvailableSelections.TryGetValue(val.NetId, out value) || !value.OwnerAppearanceLoaded;
		}
	}

	internal static bool CanEditLocalPlayerSkinInRun()
	{
		return true;
	}

	internal static void RequestIconRefresh(ulong playerNetId)
	{
		lock (Sync)
		{
			PendingIconRefreshes.Add(playerNetId);
		}
	}

	internal static bool CanEditTransformForCreature(Creature creature)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldApplyRemoteAppearance())
		{
			return true;
		}
		Player val = creature.Player ?? creature.PetOwner;
		INetGameService netService = _netService;
		if (val == null || netService == null || !NetGameTypeExtensions.IsMultiplayer(netService.Type) || val.NetId == netService.NetId)
		{
			return true;
		}
		lock (Sync)
		{
			SessionCharacterSelection value;
			return !AvailableSelections.TryGetValue(val.NetId, out value) || !value.OwnerAppearanceLoaded;
		}
	}

	internal static bool UsesLocalFallbackControls(Creature creature)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldApplyRemoteAppearance())
		{
			return false;
		}
		Player val = creature.Player ?? creature.PetOwner;
		INetGameService netService = _netService;
		if (val != null && netService != null && NetGameTypeExtensions.IsMultiplayer(netService.Type) && val.NetId != netService.NetId)
		{
			return CanEditTransformForCreature(creature);
		}
		return false;
	}

	internal static bool TryGetSyncedTransform(Creature creature, string transformKey, out CharacterCombatTransform transform)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldApplyRemoteAppearance())
		{
			transform = null;
			return false;
		}
		Player val = creature.Player ?? creature.PetOwner;
		INetGameService netService = _netService;
		if (val == null || netService == null || !NetGameTypeExtensions.IsMultiplayer(netService.Type) || val.NetId == netService.NetId)
		{
			transform = null;
			return false;
		}
		lock (Sync)
		{
			if (AvailableSelections.TryGetValue(val.NetId, out SessionCharacterSelection value) && value.OwnerAppearanceLoaded)
			{
				transform = value.Transforms.GetValueOrDefault(transformKey) ?? new CharacterCombatTransform();
				return true;
			}
			if (LocalFallbackTransforms.TryGetValue((val.NetId, transformKey), out transform))
			{
				return true;
			}
			if (AvailableSelections.TryGetValue(val.NetId, out value) && value.Transforms.TryGetValue(transformKey, out transform))
			{
				return true;
			}
		}
		transform = null;
		return false;
	}

	internal static CharacterCombatTransform SetLocalFallbackTransform(Creature creature, string transformKey, CharacterCombatTransform value)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Player val = creature.Player ?? creature.PetOwner;
		if (val == null || !CanEditTransformForCreature(creature))
		{
			if (!TryGetSyncedTransform(creature, transformKey, out CharacterCombatTransform transform))
			{
				return new CharacterCombatTransform();
			}
			return transform;
		}
		INetGameService netService = _netService;
		if (netService == null || !NetGameTypeExtensions.IsMultiplayer(netService.Type) || val.NetId == netService.NetId)
		{
			return SkinService.NormalizeCharacterCombatTransform(value);
		}
		CharacterCombatTransform characterCombatTransform = SkinService.NormalizeCharacterCombatTransform(value);
		lock (Sync)
		{
			LocalFallbackTransforms[(val.NetId, transformKey)] = characterCombatTransform;
			return characterCombatTransform;
		}
	}

	internal static bool TrySetLocalFallbackSkin(Creature creature, string groupId, string optionId, out string? error)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		error = null;
		Player owner = creature.Player ?? creature.PetOwner;
		INetGameService netService = _netService;
		if (owner == null || netService == null || owner.NetId == netService.NetId || !NetGameTypeExtensions.IsMultiplayer(netService.Type) || !CanEditSkinForCreature(creature))
		{
			error = "当前联机外观由角色所属玩家控制。";
			return false;
		}
		if (!SkinService.TryBuildSessionCharacterSelection(groupId, optionId, out IReadOnlyDictionary<string, string> selectionOverrides))
		{
			error = "找不到所选角色外观。";
			return false;
		}
		lock (Sync)
		{
			LocalFallbackSelections[(owner.NetId, groupId)] = optionId;
			(ulong, string)[] array = LocalFallbackTransforms.Keys.Where<(ulong, string)>(((ulong PlayerId, string TransformKey) key) => key.PlayerId == owner.NetId).ToArray();
			foreach ((ulong, string) key2 in array)
			{
				LocalFallbackTransforms.Remove(key2);
			}
			AvailableSelections[owner.NetId] = new SessionCharacterSelection(((AbstractModel)owner.Character).Id.Entry, groupId, optionId, selectionOverrides, new Dictionary<string, CharacterCombatTransform>(StringComparer.OrdinalIgnoreCase), OwnerAppearanceLoaded: false);
			PendingRefreshes.Add(owner.NetId);
			PendingTransformRefreshes.Remove(owner.NetId);
			PendingIconRefreshes.Add(owner.NetId);
			_runtimeProvidersDirty = true;
			_appearanceRetryCooldown = 0.0;
		}
		ModLog.Info($"本机手动为联机玩家 {owner.NetId} 设置替代外观：{groupId}/{optionId}；不是该玩家发送的选择。");
		return true;
	}

	internal static IDisposable? BeginCreatureSelectionScope(Creature creature)
	{
		Player val = creature.Player ?? creature.PetOwner;
		if (val == null || FrameworkModelPreview.IsPreviewPlayer(val))
		{
			return null;
		}
		if (!TryGetPlayerSelectionMap(val.NetId, val.Character, out IReadOnlyDictionary<string, string> selections))
		{
			return null;
		}
		if (_selectionScopes == null)
		{
			_selectionScopes = new Stack<IReadOnlyDictionary<string, string>>();
		}
		_selectionScopes.Push(selections);
		return new SelectionScope();
	}

	internal static IDisposable? BeginCreatureRuntimeScope(Creature creature)
	{
		IDisposable disposable = BeginCreatureSelectionScope(creature);
		if (disposable == null)
		{
			return null;
		}
		try
		{
			Player player = creature.Player;
			object obj = ((player != null) ? ((AbstractModel)player.Character).Id.Entry : null);
			if (obj == null)
			{
				MonsterModel monster = creature.Monster;
				obj = ((monster != null) ? ((AbstractModel)monster).Id.Entry : null);
			}
			string text = (string)obj;
			Player player2 = creature.Player;
			string modelTypeName = ((player2 != null) ? ((object)player2.Character).GetType().Name : null) ?? ((object)creature.Monster)?.GetType().Name;
			SkinGroup skinGroup = ((text == null) ? null : ContextualSkinControls.FindGroup(text, modelTypeName));
			if (skinGroup == null)
			{
				return disposable;
			}
			string scenePath = ((creature.Player != null) ? ContextualSkinControls.CanonicalScenePath("creature_visuals/" + text.ToLowerInvariant()) : ContextualSkinControls.GetMonsterVisualsPath(creature.Monster));
			IDisposable resourceScope = SkinService.BeginRuntimeResourceScope(skinGroup.Id, scenePath);
			return new CombinedScope(disposable, resourceScope);
		}
		catch
		{
			disposable.Dispose();
			throw;
		}
	}

	internal static IDisposable? BeginPlayerSelectionScope(ulong playerNetId)
	{
		if (!TryGetPlayerSelectionMap(playerNetId, null, out IReadOnlyDictionary<string, string> selections))
		{
			return null;
		}
		if (_selectionScopes == null)
		{
			_selectionScopes = new Stack<IReadOnlyDictionary<string, string>>();
		}
		_selectionScopes.Push(selections);
		return new SelectionScope();
	}

	internal static IDisposable? BeginPlayerRuntimeScope(Player player, string scenePath)
	{
		IDisposable disposable = BeginPlayerSelectionScope(player.NetId);
		if (disposable == null)
		{
			return null;
		}
		try
		{
			SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)player.Character).Id.Entry, ((object)player.Character).GetType().Name);
			if (skinGroup == null)
			{
				return disposable;
			}
			IDisposable resourceScope = SkinService.BeginRuntimeResourceScope(skinGroup.Id, scenePath);
			return new CombinedScope(disposable, resourceScope);
		}
		catch
		{
			disposable.Dispose();
			throw;
		}
	}

	private static bool TryGetPlayerSelectionMap(ulong playerNetId, CharacterModel? character, out IReadOnlyDictionary<string, string> selections)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (!ShouldApplyRemoteAppearance())
		{
			selections = null;
			return false;
		}
		CharacterModel val = character;
		if (val == null && TryGetPlayerCharacter(playerNetId, out CharacterModel character2))
		{
			val = character2;
		}
		lock (Sync)
		{
			if (AvailableSelections.TryGetValue(playerNetId, out SessionCharacterSelection value) && (val == null || (value.CharacterId.Equals(((AbstractModel)val).Id.Entry, StringComparison.OrdinalIgnoreCase) && ContextualSkinControls.MatchesGroupIdentity(value.GroupId, ((AbstractModel)val).Id.Entry, ((object)val).GetType().Name))))
			{
				selections = value.SelectionOverrides;
				return true;
			}
		}
		INetGameService netService = _netService;
		if (netService == null || !NetGameTypeExtensions.IsMultiplayer(netService.Type) || playerNetId == netService.NetId || val == null)
		{
			selections = null;
			return false;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)val).Id.Entry, ((object)val).GetType().Name);
		if (skinGroup != null && SkinService.TryBuildSessionCharacterSelection(skinGroup.Id, "__base__", out selections))
		{
			return true;
		}
		selections = null;
		return false;
	}

	internal static void AttachToRun()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		INetGameService netService = RunManager.Instance.NetService;
		bool flag = !_inRun || _netService != netService;
		_lobby = null;
		_needsLobbyRoundReset = false;
		if (!MultiplayerSkinSyncParticipationPolicy.Resolve(SkinService.ShouldSendMultiplayerSkinChanges(), SkinService.ShouldReceiveMultiplayerSkinChanges(), NetGameTypeExtensions.IsMultiplayer(netService.Type)).AttachTransport)
		{
			_suspendedLobby = null;
			_resumeRunWhenEnabled = NetGameTypeExtensions.IsMultiplayer(netService.Type);
			return;
		}
		_suspendedLobby = null;
		_resumeRunWhenEnabled = false;
		AttachToService(netService, "对局");
		_inRun = true;
		if (flag)
		{
			_snapshotElapsed = 0.0;
			_snapshotStage = 0;
			PublishLocalCharacterSelection("进入对局");
		}
		lock (Sync)
		{
			foreach (ulong key in AvailableSelections.Keys)
			{
				PendingIconRefreshes.Add(key);
			}
		}
	}

	internal static void AttachToLobby(StartRunLobby lobby)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		INetGameService netService = lobby.NetService;
		if (!MultiplayerSkinSyncParticipationPolicy.Resolve(SkinService.ShouldSendMultiplayerSkinChanges(), SkinService.ShouldReceiveMultiplayerSkinChanges(), NetGameTypeExtensions.IsMultiplayer(netService.Type)).AttachTransport)
		{
			_suspendedLobby = (NetGameTypeExtensions.IsMultiplayer(netService.Type) ? lobby : null);
			_resumeRunWhenEnabled = false;
			return;
		}
		_suspendedLobby = null;
		_resumeRunWhenEnabled = false;
		bool flag = _lobby != lobby;
		if (_needsLobbyRoundReset || (flag && _hasLobbySession))
		{
			ResetRoundStateForLobby();
		}
		_needsLobbyRoundReset = false;
		AttachToService(netService, "联机选角");
		_lobby = lobby;
		_hasLobbySession = true;
		_inRun = false;
		RememberLocalAdvertisement();
	}

	private static void ResetRoundStateForLobby()
	{
		bool flag;
		lock (Sync)
		{
			flag = AvailableSelections.Count > 0;
			AdvertisedSelections.Clear();
			AvailableSelections.Clear();
			LocalFallbackSelections.Clear();
			LocalFallbackTransforms.Clear();
			PendingRefreshes.Clear();
			PendingTransformRefreshes.Clear();
			PendingIconRefreshes.Clear();
			LastReceivedTransformSignatures.Clear();
			LastAppliedTransformSignatures.Clear();
			MissingInstalledSkinWarnings.Clear();
			_runtimeProvidersDirty = false;
			_appearanceRetryCooldown = 0.0;
			_localTransformAdvertisementDirty = false;
			_localTransformBroadcastCooldown = 0.0;
			_lastSentTransformSignature = null;
		}
		if (flag)
		{
			try
			{
				SkinService.RefreshSessionRuntimeProviders();
			}
			catch (Exception ex)
			{
				ModLog.Warn("清理上一局联机皮肤运行时失败：" + ex.GetBaseException().Message);
			}
		}
		SkinService.ClearSessionCharacterCompositions();
		_snapshotElapsed = 0.0;
		_snapshotStage = 0;
		ModLog.Info("已进入新的联机选角回合；上一回合的玩家皮肤映射已清理。");
	}

	private static void AttachToService(INetGameService service, string stage)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (MultiplayerSkinSyncParticipationPolicy.Resolve(SkinService.ShouldSendMultiplayerSkinChanges(), SkinService.ShouldReceiveMultiplayerSkinChanges(), NetGameTypeExtensions.IsMultiplayer(service.Type)).AttachTransport && service != _netService)
		{
			DetachFromRun(clearCapabilities: false);
			_netService = service;
			_messageHandler = HandleMessage;
			service.RegisterMessageHandler<SkinChangerNetMessage>(_messageHandler);
			service.Disconnected += OnDisconnected;
			INetHostGameService val = (INetHostGameService)(object)((service is INetHostGameService) ? service : null);
			if (val != null)
			{
				val.ClientDisconnected += OnClientDisconnected;
			}
			_snapshotElapsed = 0.0;
			_snapshotStage = 0;
			ModLog.Info("多人角色皮肤同步已在" + stage + "阶段启用；仅向确认安装 Skin Changer 的玩家发送选择。");
		}
	}

	internal static void DetachFromRun(bool clearCapabilities = true, bool refreshRuntimeProviders = false, bool clearOnlineCache = false)
	{
		INetGameService netService = _netService;
		if (netService != null)
		{
			try
			{
				if (_messageHandler != null)
				{
					netService.UnregisterMessageHandler<SkinChangerNetMessage>(_messageHandler);
				}
				netService.Disconnected -= OnDisconnected;
				INetHostGameService val = (INetHostGameService)(object)((netService is INetHostGameService) ? netService : null);
				if (val != null)
				{
					val.ClientDisconnected -= OnClientDisconnected;
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn("清理多人皮肤同步监听失败：" + ex.GetBaseException().Message);
			}
		}
		if (netService != null && !clearOnlineCache)
		{
			_needsLobbyRoundReset = true;
		}
		else if (clearOnlineCache)
		{
			_needsLobbyRoundReset = false;
		}
		_netService = null;
		_lobby = null;
		_messageHandler = null;
		_inRun = false;
		bool flag;
		lock (Sync)
		{
			flag = AvailableSelections.Count > 0;
			AdvertisedSelections.Clear();
			AvailableSelections.Clear();
			LocalFallbackSelections.Clear();
			LocalFallbackTransforms.Clear();
			PendingRefreshes.Clear();
			PendingTransformRefreshes.Clear();
			PendingIconRefreshes.Clear();
			LastReceivedTransformSignatures.Clear();
			LastAppliedTransformSignatures.Clear();
			MissingInstalledSkinWarnings.Clear();
			_runtimeProvidersDirty = false;
			_appearanceRetryCooldown = 0.0;
			_localTransformAdvertisementDirty = false;
			_localTransformBroadcastCooldown = 0.0;
			_lastSentTransformSignature = null;
			if (clearCapabilities)
			{
				_hasLobbySession = false;
				CapablePeers.Clear();
			}
		}
		if (flag && refreshRuntimeProviders)
		{
			try
			{
				SkinService.RefreshSessionRuntimeProviders();
			}
			catch (Exception ex2)
			{
				ModLog.Warn("清理联机皮肤运行时失败：" + ex2.GetBaseException().Message);
			}
		}
		SkinService.ClearSessionCharacterCompositions();
	}

	internal static void Tick(double delta)
	{
		if (!ShouldParticipate())
		{
			return;
		}
		INetGameService netService = _netService;
		if (netService == null || !netService.IsConnected)
		{
			return;
		}
		FlushLocalTransformAdvertisement(delta);
		_snapshotElapsed += delta;
		if ((_snapshotStage == 0 && _snapshotElapsed >= 0.75) || (_snapshotStage == 1 && _snapshotElapsed >= 3.0))
		{
			_snapshotStage++;
			BroadcastKnownSelections();
		}
		RetryAdvertisementsWaitingForPlayerIdentity();
		_appearanceRetryCooldown = Math.Max(0.0, _appearanceRetryCooldown - delta);
		ulong[] array;
		ulong[] array2;
		ulong[] array3;
		bool runtimeProvidersDirty;
		Dictionary<ulong, SessionCharacterSelection> selectionSnapshot;
		lock (Sync)
		{
			if (!_runtimeProvidersDirty && PendingRefreshes.Count == 0 && PendingTransformRefreshes.Count == 0 && PendingIconRefreshes.Count == 0)
			{
				return;
			}
			array = PendingRefreshes.ToArray();
			array2 = PendingTransformRefreshes.Where((ulong playerId) => !PendingRefreshes.Contains(playerId)).ToArray();
			array3 = PendingIconRefreshes.ToArray();
			runtimeProvidersDirty = _runtimeProvidersDirty;
			selectionSnapshot = new Dictionary<ulong, SessionCharacterSelection>(AvailableSelections);
			bool IsCurrentSelection(ulong playerId)
			{
				return (object)selectionSnapshot.GetValueOrDefault(playerId) == AvailableSelections.GetValueOrDefault(playerId);
			}
		}
		ulong[] array4 = array2;
		foreach (ulong num in array4)
		{
			bool flag = false;
			try
			{
				flag = CharacterAppearanceRuntime.RefreshPlayerTransforms(num);
				if (!flag)
				{
					lock (Sync)
					{
						PendingTransformRefreshes.Add(num);
					}
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn($"刷新联机玩家 {num} 的外观参数失败：" + ex.GetBaseException().Message);
			}
			if (flag)
			{
				lock (Sync)
				{
					PendingTransformRefreshes.Remove(num);
				}
			}
		}
		if (_appearanceRetryCooldown > 0.0 || (!runtimeProvidersDirty && array.Length == 0 && array3.Length == 0))
		{
			return;
		}
		bool flag2 = CharacterAppearanceRuntime.CanApplySelectionImmediately();
		if (runtimeProvidersDirty)
		{
			if (!flag2)
			{
				return;
			}
			lock (Sync)
			{
				_runtimeProvidersDirty = false;
			}
			try
			{
				SkinService.RefreshSessionRuntimeProviders();
			}
			catch (Exception ex2)
			{
				lock (Sync)
				{
					_runtimeProvidersDirty = true;
				}
				_appearanceRetryCooldown = 1.0;
				ModLog.Warn("刷新联机皮肤运行时失败，保留请求稍后重试：" + ex2.GetBaseException().Message);
				return;
			}
			lock (Sync)
			{
				if (_runtimeProvidersDirty)
				{
					return;
				}
			}
		}
		_appearanceRetryCooldown = 1.0;
		array4 = array3;
		foreach (ulong num2 in array4)
		{
			try
			{
				if (!ContextualSkinControls.RefreshMultiplayerPlayerIcons(num2))
				{
					continue;
				}
				lock (Sync)
				{
					if (IsCurrentSelection(num2))
					{
						PendingIconRefreshes.Remove(num2);
					}
				}
			}
			catch (Exception ex3)
			{
				ModLog.Warn($"刷新联机玩家 {num2} 的头像失败，保留重试：" + ex3.GetBaseException().Message);
			}
		}
		if (!flag2)
		{
			return;
		}
		array4 = array;
		foreach (ulong num3 in array4)
		{
			try
			{
				if (!CharacterAppearanceRuntime.RefreshPlayerAppearance(num3))
				{
					continue;
				}
				lock (Sync)
				{
					if (IsCurrentSelection(num3))
					{
						PendingRefreshes.Remove(num3);
						PendingTransformRefreshes.Remove(num3);
						ModLog.Info($"联机玩家 {num3} 的场景外观刷新完成；皮肤={selectionSnapshot.GetValueOrDefault(num3)?.OptionId ?? "本机选择"}。");
					}
				}
			}
			catch (Exception ex4)
			{
				ModLog.Warn($"刷新联机玩家 {num3} 的外观失败，保留重试：" + ex4.GetBaseException().Message);
			}
		}
	}

	internal static void OnLocalTransformChanged(string groupId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		INetGameService netService = _netService;
		if (netService != null && NetGameTypeExtensions.IsMultiplayer(netService.Type) && TryGetLocalCharacter(out ulong _, out CharacterModel character) && ContextualSkinControls.MatchesGroupIdentity(groupId, ((AbstractModel)character).Id.Entry, ((object)character).GetType().Name) && MarkLocalTransformAdvertisementDirty())
		{
			ModLog.Info("已检测到本机 " + groupId + " 的外观参数变更，等待广播给其他玩家。");
			FlushLocalTransformAdvertisement(0.05);
		}
	}

	internal static void OnLocalTransformChanged(Creature creature, string groupId)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		INetGameService netService = _netService;
		if (netService != null && NetGameTypeExtensions.IsMultiplayer(netService.Type))
		{
			Player val = creature.Player ?? creature.PetOwner;
			if (val == null)
			{
				OnLocalTransformChanged(groupId);
			}
			else if (val.NetId == netService.NetId && ContextualSkinControls.MatchesGroupIdentity(groupId, ((AbstractModel)val.Character).Id.Entry, ((object)val.Character).GetType().Name) && MarkLocalTransformAdvertisementDirty())
			{
				ModLog.Info("已检测到本机 " + groupId + " 的外观参数变更，等待广播给其他玩家。");
				FlushLocalTransformAdvertisement(0.05);
			}
		}
	}

	private static bool MarkLocalTransformAdvertisementDirty()
	{
		if (!SkinService.ShouldSendMultiplayerSkinChanges())
		{
			return false;
		}
		lock (Sync)
		{
			if (_localTransformAdvertisementDirty)
			{
				return false;
			}
			_localTransformAdvertisementDirty = true;
			return true;
		}
	}

	private static void FlushLocalTransformAdvertisement(double delta)
	{
		bool flag = false;
		ulong netId;
		SkinChangerNetMessage value;
		lock (Sync)
		{
			_localTransformBroadcastCooldown = Math.Max(0.0, _localTransformBroadcastCooldown - delta);
			if (!_localTransformAdvertisementDirty || _localTransformBroadcastCooldown > 0.0 || _netService == null)
			{
				return;
			}
			netId = _netService.NetId;
			_localTransformAdvertisementDirty = false;
			_localTransformBroadcastCooldown = 0.05;
			if (!AdvertisedSelections.TryGetValue(netId, out value))
			{
				flag = true;
				value = default(SkinChangerNetMessage);
			}
		}
		if (flag)
		{
			RememberLocalAdvertisement();
			lock (Sync)
			{
				if (!AdvertisedSelections.TryGetValue(netId, out value))
				{
					return;
				}
			}
		}
		if (string.IsNullOrWhiteSpace(value.GroupId) || string.IsNullOrWhiteSpace(value.OptionId))
		{
			return;
		}
		value.TransformManifest = SerializeTransformManifest(SkinService.GetSessionCharacterCombatTransforms(value.GroupId, value.OptionId));
		string text = value.GroupId + "\n" + value.OptionId + "\n" + value.SourceOptionManifest + "\n" + value.TransformManifest;
		bool flag2 = false;
		lock (Sync)
		{
			AdvertisedSelections[netId] = value;
			if (!string.Equals(_lastSentTransformSignature, text, StringComparison.Ordinal))
			{
				_lastSentTransformSignature = text;
				flag2 = true;
			}
		}
		if (flag2)
		{
			ModLog.Info($"已广播本机 {value.GroupId} 的联机外观参数：{value.TransformManifest.Length} 字符，状态={text.GetHashCode():X8}。");
		}
		SendLocalAdvertisement();
	}

	internal static void OnLocalCharacterSelectionChanged(string groupId)
	{
		if (!TryGetLocalCharacter(out ulong playerNetId, out CharacterModel character))
		{
			return;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name);
		if (skinGroup == null || !skinGroup.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		RememberLocalAdvertisement();
		SendLocalAdvertisement();
		ContextualSkinControls.RefreshMultiplayerPlayerIcons(playerNetId);
		try
		{
			MultiplayerTreasureHandAppearance.RefreshPlayer(playerNetId);
		}
		catch (Exception ex)
		{
			lock (Sync)
			{
				PendingRefreshes.Add(playerNetId);
			}
			ModLog.Warn("刷新本机宝箱手部失败，保留重试：" + ex.GetBaseException().Message);
		}
	}

	internal static void OnLobbyCharacterChanged(StartRunLobby lobby, ulong playerId, bool randomResolution)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (_lobby != lobby)
		{
			return;
		}
		INetGameService? netService = _netService;
		if (netService == null || !NetGameTypeExtensions.IsMultiplayer(netService.Type) || playerId != _netService.NetId)
		{
			return;
		}
		PublishLocalCharacterSelection(randomResolution ? "随机角色已确定" : "大厅角色改变");
		lock (Sync)
		{
			PendingIconRefreshes.Add(playerId);
		}
	}

	private static void PublishLocalCharacterSelection(string reason)
	{
		try
		{
			if (!RememberLocalAdvertisement())
			{
				return;
			}
			SendLocalAdvertisement();
			if (!SkinService.ShouldSendMultiplayerSkinChanges())
			{
				return;
			}
			lock (Sync)
			{
				if (_netService != null && AdvertisedSelections.TryGetValue(_netService.NetId, out var value))
				{
					ModLog.Info($"已提交本机联机皮肤同步（{reason}）：玩家={value.PlayerNetId}；角色={value.CharacterId}；皮肤={value.OptionId}；来源={value.SourceOptionManifest}。");
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("联机皮肤同步失败（" + reason + "），保留阶段快照重试：" + ex.GetBaseException().Message);
		}
	}

	internal static void OnReceivePreferenceChanged(bool enabled)
	{
		ulong[] array;
		SkinChangerNetMessage[] array2;
		ulong[] array3;
		lock (Sync)
		{
			array = AvailableSelections.Keys.Where(delegate(ulong playerId)
			{
				INetGameService? netService = _netService;
				return playerId != ((netService != null) ? new ulong?(netService.NetId) : ((ulong?)null));
			}).ToArray();
			array2 = AdvertisedSelections.Values.Where(delegate(SkinChangerNetMessage message)
			{
				ulong playerNetId = message.PlayerNetId;
				INetGameService? netService2 = _netService;
				return playerNetId != ((netService2 != null) ? new ulong?(netService2.NetId) : ((ulong?)null));
			}).ToArray();
			if (!enabled)
			{
				array3 = array;
				foreach (ulong num in array3)
				{
					AvailableSelections.Remove(num);
					PendingRefreshes.Remove(num);
					PendingTransformRefreshes.Remove(num);
					PendingIconRefreshes.Remove(num);
				}
				_runtimeProvidersDirty = true;
			}
		}
		if (enabled)
		{
			SkinChangerNetMessage[] array4 = array2;
			for (int i = 0; i < array4.Length; i++)
			{
				SkinChangerNetMessage message2 = array4[i];
				try
				{
					TryMakeSelectionAvailable(message2);
				}
				catch (Exception ex)
				{
					ModLog.Warn($"重新接收联机玩家 {message2.PlayerNetId} 的外观失败：" + ex.GetBaseException().Message);
				}
			}
			return;
		}
		try
		{
			SkinService.ClearSessionCharacterCompositions();
			SkinService.RefreshSessionRuntimeProviders();
		}
		catch (Exception ex2)
		{
			ModLog.Warn("关闭接收皮肤改变时恢复本机资源失败：" + ex2.GetBaseException().Message);
		}
		array3 = array;
		foreach (ulong num2 in array3)
		{
			try
			{
				ContextualSkinControls.RefreshMultiplayerPlayerIcons(num2);
				if (_inRun && CharacterAppearanceRuntime.CanApplySelectionImmediately())
				{
					CharacterAppearanceRuntime.RefreshPlayerAppearance(num2);
				}
			}
			catch (Exception ex3)
			{
				ModLog.Warn($"关闭接收皮肤改变后恢复玩家 {num2} 的本机外观失败：" + ex3.GetBaseException().Message);
			}
		}
	}

	internal static void OnDirectionPreferenceChanged()
	{
		if (ShouldParticipate())
		{
			StartRunLobby suspendedLobby = _suspendedLobby;
			bool resumeRunWhenEnabled = _resumeRunWhenEnabled;
			_suspendedLobby = null;
			_resumeRunWhenEnabled = false;
			if (resumeRunWhenEnabled && NRun.Instance != null)
			{
				AttachToRun();
				ModLog.Info("已重新启用本局的联机皮肤同步。");
				return;
			}
			if (suspendedLobby != null)
			{
				AttachToLobby(suspendedLobby);
				ModLog.Info("已重新启用当前房间的联机皮肤同步。");
				return;
			}
			if (_netService != null && SkinService.ShouldSendMultiplayerSkinChanges())
			{
				RememberLocalAdvertisement();
				SendLocalAdvertisement();
			}
			ModLog.Info("联机皮肤发送或接收已启用；将在当前或下次联机房间生效。");
			return;
		}
		StartRunLobby lobby = _lobby;
		bool inRun = _inRun;
		ulong[] array;
		lock (Sync)
		{
			array = AvailableSelections.Keys.ToArray();
		}
		_suspendedLobby = lobby;
		_resumeRunWhenEnabled = inRun;
		DetachFromRun(clearCapabilities: true, refreshRuntimeProviders: true);
		OnlineSkinCache.OnRemoteSkinLoadingPreferenceChanged(enabled: false);
		ulong[] array2 = array;
		foreach (ulong num in array2)
		{
			try
			{
				ContextualSkinControls.RefreshMultiplayerPlayerIcons(num);
				if (inRun && CharacterAppearanceRuntime.CanApplySelectionImmediately())
				{
					CharacterAppearanceRuntime.RefreshPlayerAppearance(num);
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn($"关闭联机同步后恢复玩家 {num} 的本机外观失败：" + ex.GetBaseException().Message);
			}
		}
		ModLog.Info("皮肤改变的发送与接收均已关闭；不会再修改握手包或交换玩家外观。");
	}

	internal static void BroadcastLocalFallbackBeforeDisablingSend()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		INetGameService? netService = _netService;
		if (netService != null && NetGameTypeExtensions.IsMultiplayer(netService.Type))
		{
			RememberLocalAdvertisement(baseOnly: true);
			SendLocalAdvertisement(force: true);
		}
	}

	internal static void ResetConnectionState(bool clearOnlineCache = false)
	{
		_suspendedLobby = null;
		_resumeRunWhenEnabled = false;
		DetachFromRun(clearCapabilities: true, clearOnlineCache, clearOnlineCache);
	}

	internal static void EndMultiplayerRunSession()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		_suspendedLobby = null;
		_resumeRunWhenEnabled = false;
		INetGameService? netService = _netService;
		if ((netService != null && NetGameTypeExtensions.IsMultiplayer(netService.Type)) || _inRun || _hasLobbySession)
		{
			DetachFromRun(clearCapabilities: true, refreshRuntimeProviders: true, clearOnlineCache: true);
		}
	}

	internal static void MarkPeerCapable(ulong peerId, byte protocolVersion)
	{
		if (!ShouldParticipate())
		{
			return;
		}
		if (protocolVersion != 9)
		{
			ModLog.Warn($"玩家 {peerId} 使用不兼容的 Skin Changer 联机协议 {protocolVersion}。");
			return;
		}
		lock (Sync)
		{
			if (CapablePeers.Add(peerId))
			{
				_snapshotElapsed = 0.0;
				_snapshotStage = 0;
				ModLog.Info($"已确认联机玩家 {peerId} 支持 Skin Changer 皮肤同步。");
			}
		}
	}

	internal static bool TryReadCapabilityTrailer(byte[] packetBytes, out byte protocolVersion)
	{
		protocolVersion = 0;
		if (!MultiplayerSkinSyncParticipationPolicy.Resolve(SkinService.ShouldSendMultiplayerSkinChanges(), SkinService.ShouldReceiveMultiplayerSkinChanges(), isMultiplayer: true).ReadCapabilityTrailer)
		{
			return false;
		}
		int num = CapabilityMagic.Length + 1;
		if (packetBytes.Length < num)
		{
			return false;
		}
		for (int num2 = packetBytes.Length - num; num2 >= 0; num2--)
		{
			if (packetBytes.AsSpan(num2, CapabilityMagic.Length).SequenceEqual(CapabilityMagic))
			{
				protocolVersion = packetBytes[num2 + CapabilityMagic.Length];
				return true;
			}
		}
		return false;
	}

	internal static void AppendCapabilityTrailer(PacketWriter writer)
	{
		if (MultiplayerSkinSyncParticipationPolicy.Resolve(SkinService.ShouldSendMultiplayerSkinChanges(), SkinService.ShouldReceiveMultiplayerSkinChanges(), isMultiplayer: true).WriteCapabilityTrailer)
		{
			int num = writer.BitPosition % 8;
			if (num != 0)
			{
				writer.WriteByte((byte)0, 8 - num);
			}
			writer.WriteBytes(CapabilityMagic, CapabilityMagic.Length);
			writer.WriteByte((byte)9, 8);
		}
	}

	private static void HandleMessage(SkinChangerNetMessage message, ulong senderId)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Invalid comparison between Unknown and I4
		if (!ShouldParticipate())
		{
			return;
		}
		INetGameService netService = _netService;
		if (netService == null || message.ProtocolVersion != 9 || message.Kind != SkinSyncMessageKind.CharacterSelection)
		{
			return;
		}
		bool flag = !IsCapable(senderId);
		MarkPeerCapable(senderId, message.ProtocolVersion);
		if ((int)netService.Type == 2)
		{
			if (!IsCapable(senderId) || message.PlayerNetId != senderId)
			{
				return;
			}
		}
		else
		{
			INetGameService obj = ((netService is INetClientGameService) ? netService : null);
			ulong? obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				NetClient netClient = ((INetClientGameService)obj).NetClient;
				obj2 = ((netClient != null) ? new ulong?(netClient.HostNetId) : ((ulong?)null));
			}
			if (obj2 != senderId || !IsCapable(senderId))
			{
				return;
			}
			if (flag)
			{
				RememberLocalAdvertisement();
				SendLocalAdvertisement();
			}
		}
		if (message.Kind != SkinSyncMessageKind.CharacterSelection || !ValidateText(message.CharacterId) || !ValidateText(message.GroupId) || !ValidateText(message.OptionId) || !TryParseSourceOptionManifest(message.SourceOptionManifest, out IReadOnlyList<string> _))
		{
			return;
		}
		string transformManifest = message.TransformManifest;
		if ((transformManifest != null && transformManifest.Length > 65536) || !TryParseTransformManifest(message.TransformManifest, message.GroupId, out IReadOnlyDictionary<string, CharacterCombatTransform> _))
		{
			return;
		}
		RememberAdvertisement(message);
		string text = message.GroupId + "\n" + message.OptionId + "\n" + message.SourceOptionManifest + "\n" + message.TransformManifest;
		bool flag2 = false;
		lock (Sync)
		{
			if (!LastReceivedTransformSignatures.TryGetValue(message.PlayerNetId, out string value) || !value.Equals(text, StringComparison.Ordinal))
			{
				LastReceivedTransformSignatures[message.PlayerNetId] = text;
				flag2 = !string.IsNullOrWhiteSpace(message.TransformManifest);
			}
		}
		if (flag2)
		{
			ModLog.Info($"已收到联机玩家 {message.PlayerNetId} 的外观参数：{message.TransformManifest.Length} 字符，状态={text.GetHashCode():X8}。");
		}
		if ((int)netService.Type == 2)
		{
			RelayFromHost(message, senderId);
		}
		TryMakeSelectionAvailable(message);
	}

	private static bool ValidateText(string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value.Length <= 512;
		}
		return false;
	}

	private static string SerializeTransformManifest(IReadOnlyDictionary<string, CharacterCombatTransform> transforms)
	{
		return JsonSerializer.Serialize(transforms);
	}

	internal static string SerializeSourceOptionManifest(IReadOnlyList<string> sourceOptionIds)
	{
		string[] array = sourceOptionIds.Where(ValidateText).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		if (array.Length != sourceOptionIds.Count || array.Length > 64)
		{
			ModLog.Warn("联机合并皮肤来源数量或格式无效，已对远端玩家回退为原皮。");
			return "[]";
		}
		string text = JsonSerializer.Serialize(array);
		if (text.Length > 32768)
		{
			ModLog.Warn("联机合并皮肤来源清单过大，已对远端玩家回退为原皮。");
			return "[]";
		}
		return text;
	}

	internal static bool TryParseSourceOptionManifest(string? manifest, out IReadOnlyList<string> sourceOptionIds)
	{
		sourceOptionIds = Array.Empty<string>();
		if (string.IsNullOrWhiteSpace(manifest) || manifest.Length > 32768)
		{
			return false;
		}
		try
		{
			string[] array = JsonSerializer.Deserialize<string[]>(manifest);
			if (array == null || array.Length > 64 || array.Any((string sourceId) => !ValidateText(sourceId)) || array.Distinct<string>(StringComparer.OrdinalIgnoreCase).Count() != array.Length)
			{
				return false;
			}
			sourceOptionIds = array;
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	private static bool TryParseTransformManifest(string? manifest, string groupId, out IReadOnlyDictionary<string, CharacterCombatTransform> transforms)
	{
		transforms = new Dictionary<string, CharacterCombatTransform>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrWhiteSpace(manifest))
		{
			return true;
		}
		if (manifest.Length > 65536)
		{
			return false;
		}
		try
		{
			Dictionary<string, CharacterCombatTransform> dictionary = JsonSerializer.Deserialize<Dictionary<string, CharacterCombatTransform>>(manifest);
			if (dictionary == null || dictionary.Count > 64)
			{
				return false;
			}
			string value2 = groupId + "::companion::";
			Dictionary<string, CharacterCombatTransform> dictionary2 = new Dictionary<string, CharacterCombatTransform>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, CharacterCombatTransform> item in dictionary)
			{
				bool flag = item.Value == null;
				if (!flag)
				{
					int length = item.Key.Length;
					bool flag2 = ((length > 512 || length == 0) ? true : false);
					flag = flag2;
				}
				if (flag || (!item.Key.Equals(groupId, StringComparison.OrdinalIgnoreCase) && !item.Key.StartsWith(value2, StringComparison.OrdinalIgnoreCase)) || !IsFinite(item.Value))
				{
					return false;
				}
				dictionary2[item.Key] = SkinService.NormalizeCharacterCombatTransform(item.Value);
			}
			transforms = dictionary2;
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
		static bool IsFinite(CharacterCombatTransform value)
		{
			if (float.IsFinite(value.Scale) && float.IsFinite(value.OffsetX) && float.IsFinite(value.OffsetY) && float.IsFinite(value.HealthBarScale) && float.IsFinite(value.HealthBarOffsetX) && float.IsFinite(value.HealthBarOffsetY) && float.IsFinite(value.IntentScale) && float.IsFinite(value.IntentOffsetX) && float.IsFinite(value.IntentOffsetY) && float.IsFinite(value.SelectionReticleScale) && float.IsFinite(value.SelectionReticleOffsetX))
			{
				return float.IsFinite(value.SelectionReticleOffsetY);
			}
			return false;
		}
	}

	private static bool IsCapable(ulong peerId)
	{
		lock (Sync)
		{
			return CapablePeers.Contains(peerId);
		}
	}

	private static void TryMakeSelectionAvailable(SkinChangerNetMessage message)
	{
		if (!ShouldApplyRemoteAppearance())
		{
			return;
		}
		INetGameService? netService = _netService;
		if (((netService != null) ? new ulong?(netService.NetId) : ((ulong?)null)) == message.PlayerNetId)
		{
			lock (Sync)
			{
				AvailableSelections.Remove(message.PlayerNetId);
				PendingRefreshes.Remove(message.PlayerNetId);
				PendingTransformRefreshes.Remove(message.PlayerNetId);
				PendingIconRefreshes.Remove(message.PlayerNetId);
				return;
			}
		}
		if (!PlayerMatchesCharacterSelection(message) || !TryParseSourceOptionManifest(message.SourceOptionManifest, out IReadOnlyList<string> sourceOptionIds) || !TryParseTransformManifest(message.TransformManifest, message.GroupId, out IReadOnlyDictionary<string, CharacterCombatTransform> transforms))
		{
			return;
		}
		bool flag = SkinService.ShouldReceiveMultiplayerSkinChanges();
		string resolvedOptionId = "__base__";
		IReadOnlyList<string> readOnlyList2;
		if (!flag)
		{
			IReadOnlyList<string> readOnlyList = Array.Empty<string>();
			readOnlyList2 = readOnlyList;
		}
		else
		{
			readOnlyList2 = SkinService.GetAvailableCharacterSelectionSourceIds(message.GroupId, sourceOptionIds);
		}
		IReadOnlyList<string> readOnlyList3 = readOnlyList2;
		bool flag2 = false;
		bool flag3;
		IReadOnlyDictionary<string, string> selectionOverrides;
		if (!flag || sourceOptionIds.Count == 0)
		{
			flag3 = SkinService.TryBuildSessionCharacterSelection(message.GroupId, "__base__", out selectionOverrides);
		}
		else if (readOnlyList3.Count > 0)
		{
			flag3 = SkinService.TryBuildSessionCharacterComposition(message.GroupId, readOnlyList3, out selectionOverrides, out resolvedOptionId);
			flag2 = flag3;
		}
		else
		{
			selectionOverrides = null;
			flag3 = false;
		}
		(ulong, string) key2 = (message.PlayerNetId, message.GroupId);
		if (!flag3)
		{
			if (flag && sourceOptionIds.Count > 0)
			{
				string item = $"{message.PlayerNetId}\n{message.GroupId}\n" + message.SourceOptionManifest;
				lock (Sync)
				{
					if (MissingInstalledSkinWarnings.Add(item))
					{
						ModLog.Info($"联机玩家 {message.PlayerNetId} 选择了本机未安装的皮肤 {string.Join(", ", sourceOptionIds)}；已使用原皮，不会下载远端资源。");
					}
				}
			}
			string value;
			lock (Sync)
			{
				LocalFallbackSelections.TryGetValue(key2, out value);
			}
			if (!string.IsNullOrWhiteSpace(value) && SkinService.TryBuildSessionCharacterSelection(message.GroupId, value, out selectionOverrides))
			{
				resolvedOptionId = value;
				flag3 = true;
				flag2 = false;
			}
			if (!flag3 && !SkinService.TryBuildSessionCharacterSelection(message.GroupId, "__base__", out selectionOverrides))
			{
				lock (Sync)
				{
					AvailableSelections.Remove(message.PlayerNetId);
					PendingRefreshes.Add(message.PlayerNetId);
					PendingTransformRefreshes.Remove(message.PlayerNetId);
					PendingIconRefreshes.Add(message.PlayerNetId);
					_runtimeProvidersDirty = true;
					return;
				}
			}
			if (!flag3)
			{
				resolvedOptionId = "__base__";
				flag2 = false;
			}
		}
		bool flag4 = false;
		lock (Sync)
		{
			if (AvailableSelections.TryGetValue(message.PlayerNetId, out SessionCharacterSelection value2) && (!value2.CharacterId.Equals(message.CharacterId, StringComparison.OrdinalIgnoreCase) || !value2.GroupId.Equals(message.GroupId, StringComparison.OrdinalIgnoreCase)))
			{
				(ulong, string)[] array = LocalFallbackTransforms.Keys.Where<(ulong, string)>(((ulong PlayerId, string TransformKey) key) => key.PlayerId == message.PlayerNetId).ToArray();
				foreach ((ulong, string) key3 in array)
				{
					LocalFallbackTransforms.Remove(key3);
				}
			}
			if (flag2)
			{
				LocalFallbackSelections.Remove(key2);
				(ulong, string)[] array = LocalFallbackTransforms.Keys.Where<(ulong, string)>(((ulong PlayerId, string TransformKey) key) => key.PlayerId == message.PlayerNetId).ToArray();
				foreach ((ulong, string) key4 in array)
				{
					LocalFallbackTransforms.Remove(key4);
				}
			}
			SessionCharacterSelection sessionCharacterSelection = new SessionCharacterSelection(message.CharacterId, message.GroupId, resolvedOptionId, selectionOverrides, transforms, flag2);
			flag4 = !AvailableSelections.TryGetValue(message.PlayerNetId, out SessionCharacterSelection value3) || !value3.CharacterId.Equals(sessionCharacterSelection.CharacterId, StringComparison.OrdinalIgnoreCase) || !value3.GroupId.Equals(sessionCharacterSelection.GroupId, StringComparison.OrdinalIgnoreCase) || !value3.OptionId.Equals(sessionCharacterSelection.OptionId, StringComparison.OrdinalIgnoreCase) || value3.OwnerAppearanceLoaded != sessionCharacterSelection.OwnerAppearanceLoaded || !DictionaryEquals(value3.SelectionOverrides, sessionCharacterSelection.SelectionOverrides);
			AvailableSelections[message.PlayerNetId] = sessionCharacterSelection;
			if (flag4)
			{
				_appearanceRetryCooldown = 0.0;
				PendingRefreshes.Add(message.PlayerNetId);
				PendingTransformRefreshes.Remove(message.PlayerNetId);
				PendingIconRefreshes.Add(message.PlayerNetId);
				_runtimeProvidersDirty = true;
			}
			else
			{
				PendingTransformRefreshes.Add(message.PlayerNetId);
			}
		}
		string value4 = message.GroupId + "\n" + resolvedOptionId + "\n" + message.SourceOptionManifest + "\n" + message.TransformManifest;
		bool flag5 = false;
		lock (Sync)
		{
			if (!LastAppliedTransformSignatures.TryGetValue(message.PlayerNetId, out string value5) || !value5.Equals(value4, StringComparison.Ordinal))
			{
				LastAppliedTransformSignatures[message.PlayerNetId] = value4;
				flag5 = !string.IsNullOrWhiteSpace(message.TransformManifest);
			}
		}
		if (flag4 || flag5)
		{
			ModLog.Info($"已接收联机玩家 {message.PlayerNetId} 的角色 {message.CharacterId}：皮肤={resolvedOptionId}，参数项={transforms.Count}，对方声明={message.OptionId}，已匹配本机来源={flag2}，头像刷新={(flag4 ? "等待节点刷新" : "仅参数刷新")}。 ");
		}
		static bool DictionaryEquals(IReadOnlyDictionary<string, string> left, IReadOnlyDictionary<string, string> right)
		{
			string value6;
			if (left.Count == right.Count)
			{
				return left.All<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => right.TryGetValue(pair.Key, out value6) && value6.Equals(pair.Value, StringComparison.OrdinalIgnoreCase));
			}
			return false;
		}
	}

	private static void RetryAdvertisementsWaitingForPlayerIdentity()
	{
		SkinChangerNetMessage[] array;
		lock (Sync)
		{
			array = AdvertisedSelections.Values.Where(delegate(SkinChangerNetMessage message)
			{
				ulong playerNetId = message.PlayerNetId;
				INetGameService? netService = _netService;
				SessionCharacterSelection value;
				return playerNetId != ((netService != null) ? new ulong?(netService.NetId) : ((ulong?)null)) && (!AvailableSelections.TryGetValue(message.PlayerNetId, out value) || !value.CharacterId.Equals(message.CharacterId, StringComparison.OrdinalIgnoreCase) || !value.GroupId.Equals(message.GroupId, StringComparison.OrdinalIgnoreCase));
			}).ToArray();
		}
		SkinChangerNetMessage[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			TryMakeSelectionAvailable(array2[i]);
		}
	}

	internal static bool IsReadyGateActiveForOnlineCache()
	{
		return false;
	}

	private static bool RememberLocalAdvertisement(bool baseOnly = false)
	{
		if (!TryGetLocalCharacter(out ulong playerNetId, out CharacterModel character))
		{
			return false;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name);
		if (skinGroup == null)
		{
			lock (Sync)
			{
				AdvertisedSelections.Remove(playerNetId);
			}
			return false;
		}
		SkinChangerNetMessage skinChangerNetMessage = default(SkinChangerNetMessage);
		skinChangerNetMessage.ProtocolVersion = 9;
		skinChangerNetMessage.Kind = SkinSyncMessageKind.CharacterSelection;
		skinChangerNetMessage.PlayerNetId = playerNetId;
		skinChangerNetMessage.CharacterId = ((AbstractModel)character).Id.Entry;
		skinChangerNetMessage.GroupId = skinGroup.Id;
		skinChangerNetMessage.OptionId = (baseOnly ? "__base__" : SkinService.Config.GetSelection(skinGroup.Id));
		SkinChangerNetMessage message = skinChangerNetMessage;
		message.SourceOptionManifest = (baseOnly ? "[]" : SerializeSourceOptionManifest(SkinService.GetCharacterSelectionSourceIds(message.GroupId, message.OptionId)));
		message.TransformManifest = SerializeTransformManifest(SkinService.GetSessionCharacterCombatTransforms(message.GroupId, message.OptionId));
		RememberAdvertisement(message);
		return true;
	}

	internal static void RetryCachedSelection(SkinChangerNetMessage message, string sessionOptionId)
	{
	}

	internal static void OnLocalOnlineMetadataReady(string groupId, string optionId)
	{
	}

	private static bool TryGetLocalCharacter(out ulong playerNetId, out CharacterModel character)
	{
		INetGameService netService = _netService;
		if (netService != null && TryGetLobbyCharacter(netService.NetId, out character))
		{
			playerNetId = netService.NetId;
			return true;
		}
		Player localPlayer = CharacterAppearanceRuntime.GetLocalPlayer();
		if (localPlayer != null)
		{
			playerNetId = localPlayer.NetId;
			character = localPlayer.Character;
			return true;
		}
		playerNetId = 0uL;
		character = null;
		return false;
	}

	internal static bool TryGetPlayerCharacter(ulong playerNetId, out CharacterModel character)
	{
		if (TryGetLobbyCharacter(playerNetId, out character))
		{
			return true;
		}
		return CharacterAppearanceRuntime.TryGetPlayerCharacter(playerNetId, out character);
	}

	private static bool PlayerMatchesCharacterSelection(SkinChangerNetMessage message)
	{
		if (TryGetLobbyCharacter(message.PlayerNetId, out CharacterModel character))
		{
			if (((AbstractModel)character).Id.Entry.Equals(message.CharacterId, StringComparison.OrdinalIgnoreCase))
			{
				return ContextualSkinControls.MatchesGroupIdentity(message.GroupId, ((AbstractModel)character).Id.Entry, ((object)character).GetType().Name);
			}
			return false;
		}
		return CharacterAppearanceRuntime.PlayerMatchesCharacterSelection(message.PlayerNetId, message.CharacterId, message.GroupId);
	}

	private static bool TryGetLobbyCharacter(ulong playerNetId, out CharacterModel character)
	{
		character = null;
		StartRunLobby lobby = _lobby;
		if (lobby == null)
		{
			return false;
		}
		try
		{
			if (!(AccessTools.Property(typeof(StartRunLobby), "Players")?.GetValue(lobby) is IEnumerable enumerable))
			{
				return false;
			}
			foreach (object item in enumerable)
			{
				if (item == null)
				{
					continue;
				}
				Type type = item.GetType();
				if (AccessTools.Field(type, "id")?.GetValue(item) is ulong num && num == playerNetId)
				{
					object? obj = AccessTools.Field(type, "character")?.GetValue(item);
					CharacterModel val = (CharacterModel)((obj is CharacterModel) ? obj : null);
					if (val != null)
					{
						character = val;
						return true;
					}
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("读取联机选角玩家失败：" + ex.GetBaseException().Message);
		}
		return false;
	}

	private static void RememberAdvertisement(SkinChangerNetMessage message)
	{
		lock (Sync)
		{
			AdvertisedSelections[message.PlayerNetId] = message;
		}
	}

	private static bool SameAdvertisedSelection(SkinChangerNetMessage left, SkinChangerNetMessage right)
	{
		if (left.PlayerNetId == right.PlayerNetId && string.Equals(left.CharacterId, right.CharacterId, StringComparison.OrdinalIgnoreCase) && string.Equals(left.GroupId, right.GroupId, StringComparison.OrdinalIgnoreCase) && string.Equals(left.OptionId, right.OptionId, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(left.SourceOptionManifest, right.SourceOptionManifest, StringComparison.Ordinal);
		}
		return false;
	}

	private static void SendLocalAdvertisement(bool force = false)
	{
		if (!force && !SkinService.ShouldSendMultiplayerSkinChanges())
		{
			return;
		}
		INetGameService netService = _netService;
		if (netService == null)
		{
			return;
		}
		SkinChangerNetMessage value;
		lock (Sync)
		{
			if (!AdvertisedSelections.TryGetValue(netService.NetId, out value))
			{
				return;
			}
		}
		if (netService is INetHostGameService)
		{
			SkinChangerNetMessage message = value;
			bool force2 = force;
			RelayFromHost(message, null, force2);
			return;
		}
		INetGameService obj = ((netService is INetClientGameService) ? netService : null);
		ulong? obj2;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			NetClient netClient = ((INetClientGameService)obj).NetClient;
			obj2 = ((netClient != null) ? new ulong?(netClient.HostNetId) : ((ulong?)null));
		}
		ulong? num = obj2;
		if (num.HasValue)
		{
			netService.SendMessage<SkinChangerNetMessage>(value);
		}
	}

	private static void BroadcastKnownSelections()
	{
		RememberLocalAdvertisement();
		if (_netService is INetHostGameService)
		{
			SkinChangerNetMessage[] array;
			lock (Sync)
			{
				array = AdvertisedSelections.Values.ToArray();
			}
			SkinChangerNetMessage[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RelayFromHost(array2[i], null);
			}
		}
		else
		{
			SendLocalAdvertisement();
		}
	}

	private static void RelayFromHost(SkinChangerNetMessage message, ulong? exceptPeerId = null, bool force = false)
	{
		if (!ShouldParticipate())
		{
			return;
		}
		if (!force)
		{
			ulong playerNetId = message.PlayerNetId;
			INetGameService? netService = _netService;
			if (playerNetId == ((netService != null) ? new ulong?(netService.NetId) : ((ulong?)null)) && !SkinService.ShouldSendMultiplayerSkinChanges())
			{
				return;
			}
		}
		INetGameService? netService2 = _netService;
		INetHostGameService val = (INetHostGameService)(object)((netService2 is INetHostGameService) ? netService2 : null);
		if (val == null)
		{
			return;
		}
		ulong[] array;
		lock (Sync)
		{
			array = CapablePeers.ToArray();
		}
		ulong[] array2 = array;
		foreach (ulong num in array2)
		{
			if (num != exceptPeerId)
			{
				((INetGameService)val).SendMessage<SkinChangerNetMessage>(message, num);
			}
		}
	}

	private static void OnClientDisconnected(ulong peerId, NetErrorInfo _)
	{
		lock (Sync)
		{
			CapablePeers.Remove(peerId);
			AdvertisedSelections.Remove(peerId);
			if (AvailableSelections.Remove(peerId))
			{
				_runtimeProvidersDirty = true;
			}
			(ulong, string)[] array = LocalFallbackSelections.Keys.Where<(ulong, string)>(((ulong PlayerId, string GroupId) key) => key.PlayerId == peerId).ToArray();
			foreach ((ulong, string) key2 in array)
			{
				LocalFallbackSelections.Remove(key2);
			}
			(ulong, string)[] array2 = LocalFallbackTransforms.Keys.Where<(ulong, string)>(((ulong PlayerId, string TransformKey) key) => key.PlayerId == peerId).ToArray();
			foreach ((ulong, string) key3 in array2)
			{
				LocalFallbackTransforms.Remove(key3);
			}
			PendingRefreshes.Remove(peerId);
			PendingTransformRefreshes.Remove(peerId);
			PendingIconRefreshes.Remove(peerId);
		}
	}

	private static void OnDisconnected(NetErrorInfo _)
	{
		DetachFromRun();
	}
}
