using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Runs;

namespace CznCore.Presentation;

internal sealed class RemoteCardReadyBodyRuntime
{
	private sealed class RemotePlayerState(Player player)
	{
		public Player Player { get; } = player;

		public CardModel? HoveredCard { get; set; }

		public CardModel? PlayingCard { get; set; }

		public CardModel? SynthesizedReadyCard { get; set; }

		public ulong SynthesizedReadyStartedMsec { get; set; }

		public void Clear()
		{
			HoveredCard = null;
			PlayingCard = null;
			SynthesizedReadyCard = null;
			SynthesizedReadyStartedMsec = 0uL;
		}
	}

	private readonly Func<Player?, bool> _isTargetRemotePlayer;

	private readonly Func<Player?, bool> _canPresentBody;

	private readonly Func<CombatBodyReadyPresentationRuntime<Player, CardModel>> _getReadyPresentation;

	private readonly Action<string, int> _warn;

	private readonly float _readyTransitionSeconds;

	private readonly Dictionary<ulong, RemotePlayerState> _players = new Dictionary<ulong, RemotePlayerState>();

	private HoveredModelTracker? _hoveredModelTracker;

	private bool _eventSubscriptionsAdded;

	public RemoteCardReadyBodyRuntime(Func<Player?, bool> isTargetRemotePlayer, Func<Player?, bool> canPresentBody, Func<CombatBodyReadyPresentationRuntime<Player, CardModel>> getReadyPresentation, float readyTransitionSeconds, Action<string, int> warn)
	{
		_isTargetRemotePlayer = isTargetRemotePlayer ?? throw new ArgumentNullException("isTargetRemotePlayer");
		_canPresentBody = canPresentBody ?? throw new ArgumentNullException("canPresentBody");
		_getReadyPresentation = getReadyPresentation ?? throw new ArgumentNullException("getReadyPresentation");
		_readyTransitionSeconds = Math.Max(0f, readyTransitionSeconds);
		_warn = warn ?? throw new ArgumentNullException("warn");
	}

	public void AddEventSubscriptions(ICollection<IDisposable> subscriptions, string contextPrefix)
	{
		ArgumentNullException.ThrowIfNull(subscriptions, "subscriptions");
		if (!_eventSubscriptionsAdded)
		{
			_eventSubscriptionsAdded = true;
			subscriptions.Add(SkinEventHub.Subscribe(contextPrefix + " teammate card ready begin", delegate(CardPlayingEvent gameEvent)
			{
				BeginCardPlay(gameEvent.CardPlay.Card);
			}));
			subscriptions.Add(SkinEventHub.Subscribe(contextPrefix + " teammate card ready complete", delegate(CardPlayedEvent gameEvent)
			{
				CompleteCardPlay(gameEvent.CardPlay.Card);
			}));
		}
	}

	public void TrackPlayers(IEnumerable<Player> players)
	{
		ArgumentNullException.ThrowIfNull(players, "players");
		CombatBodyReadyPresentationRuntime<Player, CardModel> combatBodyReadyPresentationRuntime = _getReadyPresentation();
		foreach (RemotePlayerState value in _players.Values)
		{
			combatBodyReadyPresentationRuntime.Clear(value.Player);
		}
		_players.Clear();
		foreach (Player player in players)
		{
			if (_isTargetRemotePlayer(player))
			{
				_players[player.NetId] = new RemotePlayerState(player);
			}
		}
		RefreshHoveredModelSubscription();
	}

	public void ClearPlayer(Player player)
	{
		if (_players.TryGetValue(player.NetId, out RemotePlayerState value))
		{
			value.Clear();
		}
		_getReadyPresentation().Clear(player);
	}

	public bool IsPresentationActive(Player player)
	{
		return _getReadyPresentation().IsPresentationActive(player);
	}

	public bool TryConsumeActionHandoffDelay(CardModel? card, out float delaySeconds)
	{
		delaySeconds = 0f;
		Player val = ((card != null) ? card.Owner : null);
		if (val == null || !_players.TryGetValue(val.NetId, out RemotePlayerState value) || !_canPresentBody(val))
		{
			return false;
		}
		if (value.SynthesizedReadyCard == card && value.SynthesizedReadyStartedMsec != 0 && _readyTransitionSeconds > 0f)
		{
			ulong ticksMsec = Time.GetTicksMsec();
			float num = ((ticksMsec >= value.SynthesizedReadyStartedMsec) ? ((float)(ticksMsec - value.SynthesizedReadyStartedMsec) / 1000f) : 0f);
			delaySeconds = Math.Max(0f, _readyTransitionSeconds - num);
		}
		value.SynthesizedReadyCard = null;
		value.SynthesizedReadyStartedMsec = 0uL;
		return true;
	}

	private void RefreshHoveredModelSubscription()
	{
		HoveredModelTracker val = null;
		if (_players.Count > 0)
		{
			try
			{
				val = RunManager.Instance.HoveredModelTracker;
			}
			catch (Exception ex)
			{
				_warn("Card selection tracker lookup failed: " + SkinLog.FormatException(ex), 2);
			}
		}
		if (_hoveredModelTracker != val)
		{
			if (_hoveredModelTracker != null)
			{
				_hoveredModelTracker.HoverChanged -= OnHoveredModelChanged;
			}
			_hoveredModelTracker = val;
			if (_hoveredModelTracker != null)
			{
				_hoveredModelTracker.HoverChanged += OnHoveredModelChanged;
			}
		}
	}

	private void OnHoveredModelChanged(ulong playerId)
	{
		try
		{
			if (!_players.TryGetValue(playerId, out RemotePlayerState value))
			{
				return;
			}
			if (!_canPresentBody(value.Player))
			{
				value.Clear();
				_getReadyPresentation().Clear(value.Player);
				return;
			}
			CardModel val = ResolveHoveredCard(value.Player);
			if (value.PlayingCard != null)
			{
				value.HoveredCard = ((val != null && val != value.PlayingCard) ? val : null);
				return;
			}
			if (val != null)
			{
				value.HoveredCard = val;
				_getReadyPresentation().ShowForCard(val);
				return;
			}
			CardModel hoveredCard = value.HoveredCard;
			value.HoveredCard = null;
			if (hoveredCard != null)
			{
				_getReadyPresentation().HideForCard(hoveredCard);
			}
		}
		catch (Exception ex)
		{
			_warn("Card selection sync failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	private CardModel? ResolveHoveredCard(Player player)
	{
		HoveredModelTracker? hoveredModelTracker = _hoveredModelTracker;
		AbstractModel obj = ((hoveredModelTracker != null) ? hoveredModelTracker.GetHoveredModel(player.NetId) : null);
		CardModel val = (CardModel)(object)((obj is CardModel) ? obj : null);
		if (val == null || val.Owner != player)
		{
			return null;
		}
		return val;
	}

	private void BeginCardPlay(CardModel? card)
	{
		Player val = ((card != null) ? card.Owner : null);
		if (val != null && _players.TryGetValue(val.NetId, out RemotePlayerState value) && _canPresentBody(val))
		{
			CombatBodyReadyPresentationRuntime<Player, CardModel> combatBodyReadyPresentationRuntime = _getReadyPresentation();
			bool flag = combatBodyReadyPresentationRuntime.ShowForCard(card);
			combatBodyReadyPresentationRuntime.HoldForCardPlay(card);
			flag = flag && combatBodyReadyPresentationRuntime.IsReadyForCard(card);
			value.SynthesizedReadyCard = (flag ? card : null);
			value.SynthesizedReadyStartedMsec = (flag ? Time.GetTicksMsec() : 0);
			value.PlayingCard = card;
			if (value.HoveredCard == card)
			{
				value.HoveredCard = null;
			}
		}
	}

	private void CompleteCardPlay(CardModel? card)
	{
		Player val = ((card != null) ? card.Owner : null);
		if (val != null && _players.TryGetValue(val.NetId, out RemotePlayerState value) && value.PlayingCard == card)
		{
			value.PlayingCard = null;
			if (value.SynthesizedReadyCard == card)
			{
				value.SynthesizedReadyCard = null;
				value.SynthesizedReadyStartedMsec = 0uL;
			}
			if (_canPresentBody(val) && value.HoveredCard != null && value.HoveredCard != card)
			{
				_getReadyPresentation().ShowForCard(value.HoveredCard);
			}
		}
	}
}
