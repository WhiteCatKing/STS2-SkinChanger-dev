using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Data.Judgement;

public static class PortableTarotUseRouter
{
	private readonly record struct HiddenScreenState(CanvasItem Screen, bool WasVisible, ProcessModeEnum ProcessMode)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Screen = ");
			builder.Append(Screen);
			builder.Append(", WasVisible = ");
			builder.Append(WasVisible.ToString());
			builder.Append(", ProcessMode = ");
			ProcessModeEnum processMode = ProcessMode;
			builder.Append(((object)(ProcessModeEnum)(ref processMode)/*cast due to .constrained prefix*/).ToString());
			return true;
		}

		[CompilerGenerated]
		public void Deconstruct(out CanvasItem Screen, out bool WasVisible, out ProcessModeEnum ProcessMode)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected I8, but got Unknown
			Screen = this.Screen;
			WasVisible = this.WasVisible;
			ProcessMode = (ProcessModeEnum)(long)this.ProcessMode;
		}
	}

	private readonly record struct AvailabilitySnapshot(bool HasBlockingCardSelection, IReadOnlyList<CardModel>? Cards, bool HasMerchantRelic, bool HasRelicReward, bool HasTreasureRelic);

	[CompilerGenerated]
	private sealed class _003CDescendants_003Ed__20<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull, Node
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private Node _003Cchild_003E5__3;

		private IEnumerator<T> _003C_003E7__wrap3;

		T IEnumerator<T>.Current
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
		public _003CDescendants_003Ed__20(int _003C_003E1__state)
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
						T val = (T)(object)((obj is T) ? obj : null);
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
					_003C_003E7__wrap3 = Descendants<T>(_003Cchild_003E5__3).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00d8;
					IL_00d8:
					if (_003C_003E7__wrap3.MoveNext())
					{
						T current = _003C_003E7__wrap3.Current;
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
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			_003CDescendants_003Ed__20<T> _003CDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendants_003Ed__ = this;
			}
			else
			{
				_003CDescendants_003Ed__ = new _003CDescendants_003Ed__20<T>(0);
			}
			_003CDescendants_003Ed__.root = _003C_003E3__root;
			return _003CDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	private static readonly HashSet<NCardHighlight> ActiveUseHoverHighlights = new HashSet<NCardHighlight>();

	private static readonly HashSet<ulong> PlayersSelecting = new HashSet<ulong>();

	public static bool CanUse(Player player, string tarotId)
	{
		if (!IsSelecting(player.NetId))
		{
			IReadOnlyList<CardModel> cards;
			if (!TryResolveCombat(player, tarotId, out TarotDef def) && !TryResolveWheelOfFortune(player, tarotId, out var context))
			{
				return TryResolve(player, tarotId, out def, out context, out cards);
			}
			return true;
		}
		return false;
	}

	public static int GetUsableMask(Player player, IReadOnlyList<string> tarotIds)
	{
		if (tarotIds.Count == 0 || IsSelecting(player.NetId))
		{
			return 0;
		}
		int num = 0;
		if (CombatManager.Instance.IsInProgress)
		{
			for (int i = 0; i < tarotIds.Count; i++)
			{
				if (TryResolveCombat(player, tarotIds[i], out TarotDef _))
				{
					num |= 1 << i;
				}
			}
			return num;
		}
		AvailabilitySnapshot availabilitySnapshot = CaptureAvailabilitySnapshot(player);
		int j;
		for (j = 0; j < tarotIds.Count; j++)
		{
			TarotDef tarotDef = TarotDeck.All.FirstOrDefault((TarotDef candidate) => candidate.Id == tarotIds[j]);
			if (tarotDef != null)
			{
				bool flag;
				if (TarotAcquisitionResolver.IsWheelOfFortune(tarotDef))
				{
					flag = availabilitySnapshot.HasMerchantRelic || availabilitySnapshot.HasRelicReward || availabilitySnapshot.HasTreasureRelic;
				}
				else if (tarotDef.Enchantment != null && !availabilitySnapshot.HasBlockingCardSelection)
				{
					IReadOnlyList<CardModel> source = availabilitySnapshot.Cards ?? Array.Empty<CardModel>();
					string id = tarotDef.Id;
					bool flag2 = ((id == "LOVERS_UPRIGHT" || id == "LOVERS_REVERSED") ? true : false);
					int num2 = ((!flag2) ? 1 : tarotDef.CardsToEnchant);
					flag = ((IEnumerable<CardModel>)source).Count((Func<CardModel, bool>)tarotDef.Enchantment.CanEnchant) >= num2;
				}
				else
				{
					flag = false;
				}
				if (flag)
				{
					num |= 1 << j;
				}
			}
		}
		return num;
	}

	public static Task<bool> TryUse(Player player, int slotIndex, string tarotId)
	{
		lock (PlayersSelecting)
		{
			if (!PlayersSelecting.Add(player.NetId))
			{
				return Task.FromResult(result: false);
			}
		}
		try
		{
			if (!JudgementTarotInventoryState.TryGetSlot(player.NetId, slotIndex, out string tarotId2, out uint revision) || !string.Equals(tarotId2, tarotId, StringComparison.Ordinal))
			{
				return Task.FromResult(result: false);
			}
			if (TryResolveCombat(player, tarotId, out TarotDef def))
			{
				RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue((GameAction)(object)new UsePortableTarotCombatAction(player, slotIndex, tarotId, revision));
				return Task.FromResult(result: true);
			}
			if (TryResolveWheelOfFortune(player, tarotId, out var context))
			{
				RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue((GameAction)(object)new UseWheelOfFortuneAction(player, slotIndex, tarotId, revision, context));
				return Task.FromResult(result: true);
			}
			if (!TryResolve(player, tarotId, out def, out PortableTarotUseContext context2, out IReadOnlyList<CardModel> _))
			{
				return Task.FromResult(result: false);
			}
			UsePortableTarotAction usePortableTarotAction = new UsePortableTarotAction(player, slotIndex, tarotId, revision, context2);
			RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue((GameAction)(object)usePortableTarotAction);
			return Task.FromResult(result: true);
		}
		finally
		{
			lock (PlayersSelecting)
			{
				PlayersSelecting.Remove(player.NetId);
			}
		}
	}

	internal static async Task<IReadOnlyList<CardModel>> SelectDeckTargetsInAction(Player player, IReadOnlyList<CardModel> cards, EnchantmentModel enchantment, int amount, CardSelectorPrefs prefs, PlayerChoiceContext choiceContext)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (cards.Count <= ((CardSelectorPrefs)(ref prefs)).MinSelect)
		{
			return cards;
		}
		if (player.Creature.IsDead)
		{
			return Array.Empty<CardModel>();
		}
		Dictionary<CardModel, int> deckOrder = PileTypeExtensions.GetPile((PileType)6, player).Cards.Select((CardModel card, int index) => (card: card, index: index)).ToDictionary(((CardModel card, int index) pair) => pair.card, ((CardModel card, int index) pair) => pair.index);
		CardModel[] sortedCards = cards.OrderBy((CardModel card) => deckOrder[card]).ToArray();
		bool reopenDeck = false;
		bool reopenMap = false;
		List<HiddenScreenState> hiddenScreens = new List<HiddenScreenState>();
		try
		{
			if (LocalContext.IsMe(player))
			{
				YieldBlockingScreens(hiddenScreens, out reopenDeck, out reopenMap);
			}
			uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(player);
			await choiceContext.SignalPlayerChoiceBegun(player, (PlayerChoiceOptions)0);
			IReadOnlyList<CardModel> selected;
			if (LocalContext.IsMe(player) && (int)RunManager.Instance.NetService.Type != 4)
			{
				selected = (await ((NCardGridSelectionScreen)NDeckEnchantSelectScreen.ShowScreen((IReadOnlyList<CardModel>)sortedCards, enchantment, amount, prefs)).CardsSelected()).ToArray();
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(player, choiceId, PlayerChoiceResult.FromMutableDeckCards((IEnumerable<CardModel>)selected));
			}
			else
			{
				selected = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(player, choiceId)).AsDeckCards().ToArray();
			}
			await choiceContext.SignalPlayerChoiceEnded();
			return selected;
		}
		finally
		{
			if (LocalContext.IsMe(player))
			{
				RestoreHiddenScreens(hiddenScreens);
				if (reopenMap)
				{
					NMapScreen instance = NMapScreen.Instance;
					if (instance != null && !instance.IsOpen)
					{
						instance.Open(true);
					}
				}
				if (reopenDeck && RunManager.Instance.IsInProgress)
				{
					NCapstoneContainer instance2 = NCapstoneContainer.Instance;
					if (((instance2 != null) ? instance2.CurrentCapstoneScreen : null) == null)
					{
						NDeckViewScreen.ShowScreen(player);
					}
				}
			}
		}
	}

	private static void YieldBlockingScreens(List<HiddenScreenState> hiddenScreens, out bool reopenDeck, out bool reopenMap)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		NCapstoneContainer instance = NCapstoneContainer.Instance;
		reopenDeck = ((instance != null) ? instance.CurrentCapstoneScreen : null) is NDeckViewScreen;
		if (reopenDeck)
		{
			NCapstoneContainer.Instance.Close();
		}
		NMapScreen instance2 = NMapScreen.Instance;
		reopenMap = instance2 != null && instance2.IsOpen;
		if (reopenMap)
		{
			NMapScreen.Instance.Close(false);
		}
		IScreenContext currentScreen = ActiveScreenContext.Instance.GetCurrentScreen();
		if (currentScreen != null && !(currentScreen is IOverlayScreen) && !IsBaseRunContext(currentScreen))
		{
			CanvasItem val = (CanvasItem)(object)((currentScreen is CanvasItem) ? currentScreen : null);
			if (val != null && val.IsVisibleInTree())
			{
				hiddenScreens.Add(new HiddenScreenState(val, val.Visible, ((Node)val).ProcessMode));
				val.Visible = false;
				((Node)val).ProcessMode = (ProcessModeEnum)4;
				ActiveScreenContext.Instance.Update();
				Log.Info($"[Judgement] 临时隐藏阻挡塔罗附魔选择的自定义界面：{((Node)val).GetPath()}", 2);
			}
		}
	}

	private static bool IsBaseRunContext(IScreenContext screen)
	{
		NRun instance = NRun.Instance;
		if (instance != null && screen != instance.EventRoom && screen != instance.CombatRoom && screen != instance.TreasureRoom && screen != instance.RestSiteRoom && screen != instance.MapRoom && screen != instance.MerchantRoom)
		{
			NMerchantRoom merchantRoom = instance.MerchantRoom;
			return screen == ((merchantRoom != null) ? merchantRoom.Inventory : null);
		}
		return true;
	}

	private static void RestoreHiddenScreens(IEnumerable<HiddenScreenState> hiddenScreens)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		foreach (HiddenScreenState hiddenScreen in hiddenScreens)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)hiddenScreen.Screen))
			{
				hiddenScreen.Screen.Visible = hiddenScreen.WasVisible;
				((Node)hiddenScreen.Screen).ProcessMode = hiddenScreen.ProcessMode;
			}
		}
		ActiveScreenContext.Instance.Update();
	}

	private static bool IsSelecting(ulong playerNetId)
	{
		lock (PlayersSelecting)
		{
			return PlayersSelecting.Contains(playerNetId);
		}
	}

	public static void SetUseHoverGlow(Player player, string tarotId, bool active)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		ClearUseHoverGlow();
		if (!active || !TryResolve(player, tarotId, out TarotDef def, out PortableTarotUseContext _, out IReadOnlyList<CardModel> cards))
		{
			return;
		}
		HashSet<CardModel> hashSet = ((IEnumerable<CardModel>)cards).Where((Func<CardModel, bool>)def.Enchantment.CanEnchant).ToHashSet();
		foreach (NCard item in Descendants<NCard>((Node)(object)((SceneTree)Engine.GetMainLoop()).Root))
		{
			if (item.Model != null && hashSet.Contains(item.Model) && ((CanvasItem)item).IsVisibleInTree())
			{
				((CanvasItem)item.CardHighlight).Modulate = NCardHighlight.gold;
				item.CardHighlight.AnimShow();
				ActiveUseHoverHighlights.Add(item.CardHighlight);
			}
		}
	}

	public static void ClearUseHoverGlow()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		foreach (NCardHighlight activeUseHoverHighlight in ActiveUseHoverHighlights)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)activeUseHoverHighlight))
			{
				activeUseHoverHighlight.AnimHide();
				((CanvasItem)activeUseHoverHighlight).Modulate = NCardHighlight.playableColor;
			}
		}
		ActiveUseHoverHighlights.Clear();
	}

	private static bool TryResolve(Player player, string tarotId, out TarotDef def, out PortableTarotUseContext context, out IReadOnlyList<CardModel> cards)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		def = TarotDeck.All.FirstOrDefault((TarotDef candidate) => candidate.Id == tarotId);
		context = PortableTarotUseContext.None;
		cards = Array.Empty<CardModel>();
		if (def?.Enchantment == null || CombatManager.Instance.IsInProgress)
		{
			return false;
		}
		Window root = ((SceneTree)Engine.GetMainLoop()).Root;
		if (FindVisible<NChooseACardSelectionScreen>((Node)(object)root) != null)
		{
			return false;
		}
		NCardRewardSelectionScreen val = FindVisible<NCardRewardSelectionScreen>((Node)(object)root);
		if (val != null)
		{
			IReadOnlyList<CardModel> readOnlyList = PortableTarotRewardContextRegistry.Get((Node)(object)val);
			if (readOnlyList != null)
			{
				context = PortableTarotUseContext.CardReward;
				cards = readOnlyList;
				goto IL_0155;
			}
		}
		NSimpleCardSelectScreen val2 = FindVisible<NSimpleCardSelectScreen>((Node)(object)root);
		if (val2 != null)
		{
			IReadOnlyList<CardModel> readOnlyList2 = PortableTarotRewardContextRegistry.Get((Node)(object)val2);
			if (readOnlyList2 != null)
			{
				context = PortableTarotUseContext.SimpleGridReward;
				cards = readOnlyList2;
				goto IL_0155;
			}
		}
		NMerchantInventory val3 = FindVisible<NMerchantInventory>((Node)(object)root);
		if (val3 != null && val3.IsOpen && val3.Inventory != null)
		{
			context = PortableTarotUseContext.Merchant;
			cards = (from entry in val3.Inventory.CardEntries
				where ((MerchantEntry)entry).IsStocked && entry.CreationResult != null
				select entry.CreationResult.Card).ToArray();
		}
		else if (player.RunState.CurrentRoom is RestSiteRoom)
		{
			context = PortableTarotUseContext.Deck;
			cards = player.Deck.Cards.ToArray();
		}
		goto IL_0155;
		IL_0155:
		string id = def.Id;
		bool flag = ((id == "LOVERS_UPRIGHT" || id == "LOVERS_REVERSED") ? true : false);
		int num = ((!flag) ? 1 : def.CardsToEnchant);
		return ((IEnumerable<CardModel>)cards).Count((Func<CardModel, bool>)def.Enchantment.CanEnchant) >= num;
	}

	private static bool TryResolveCombat(Player player, string tarotId, out TarotDef def)
	{
		def = TarotDeck.All.FirstOrDefault((TarotDef candidate) => candidate.Id == tarotId);
		if (def == null || !TarotAcquisitionResolver.IsPortableCandidate(def) || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsOverOrEnding || !LocalContext.IsMe(player) || player.Creature.IsDead)
		{
			return false;
		}
		if (TarotAcquisitionResolver.IsTower(def))
		{
			return true;
		}
		if (def.Enchantment == null)
		{
			return false;
		}
		return ((IEnumerable<CardModel>)PileTypeExtensions.GetPile((PileType)2, player).Cards).Count((Func<CardModel, bool>)def.Enchantment.CanEnchant) >= def.CardsToEnchant;
	}

	private static bool TryResolveWheelOfFortune(Player player, string tarotId, out PortableTarotUseContext context)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		context = PortableTarotUseContext.None;
		TarotDef tarotDef = TarotDeck.All.FirstOrDefault((TarotDef candidate) => candidate.Id == tarotId);
		if (tarotDef == null || !TarotAcquisitionResolver.IsWheelOfFortune(tarotDef) || CombatManager.Instance.IsInProgress)
		{
			return false;
		}
		Window root = ((SceneTree)Engine.GetMainLoop()).Root;
		NMerchantInventory val = FindVisible<NMerchantInventory>((Node)(object)root);
		if (val != null && val.IsOpen && val.Inventory != null && val.Inventory.RelicEntries.Any((MerchantRelicEntry entry) => ((MerchantEntry)entry).IsStocked && entry.Model != null && TarotImmediateEffects.CanWheelOfFortuneCopy(entry.Model)))
		{
			context = PortableTarotUseContext.MerchantRelic;
			return true;
		}
		if (Descendants<NRewardButton>((Node)(object)root).FirstOrDefault((Func<NRewardButton, bool>)delegate(NRewardButton button)
		{
			if (((CanvasItem)button).IsVisibleInTree() && ((NClickableControl)button).IsEnabled)
			{
				Reward reward = button.Reward;
				RelicReward val2 = (RelicReward)(object)((reward is RelicReward) ? reward : null);
				if (val2 != null && val2.Relic != null)
				{
					return TarotImmediateEffects.CanWheelOfFortuneCopy(val2.Relic);
				}
			}
			return false;
		}) != null)
		{
			context = PortableTarotUseContext.RelicReward;
			return true;
		}
		if (player.RunState.CurrentRoom is TreasureRoom)
		{
			TreasureRoomRelicSynchronizer treasureRoomRelicSynchronizer = RunManager.Instance.TreasureRoomRelicSynchronizer;
			IReadOnlyList<RelicModel> currentRelics = treasureRoomRelicSynchronizer.CurrentRelics;
			if (currentRelics != null && currentRelics.Any(TarotImmediateEffects.CanWheelOfFortuneCopy) && !treasureRoomRelicSynchronizer.GetPlayerVote(player).voteReceived)
			{
				context = PortableTarotUseContext.TreasureRelic;
				return true;
			}
		}
		return false;
	}

	private static AvailabilitySnapshot CaptureAvailabilitySnapshot(Player player)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		NChooseACardSelectionScreen val = null;
		NCardRewardSelectionScreen val2 = null;
		NSimpleCardSelectScreen val3 = null;
		NMerchantInventory val4 = null;
		bool flag = false;
		foreach (Node item in Descendants<Node>((Node)(object)((SceneTree)Engine.GetMainLoop()).Root))
		{
			if (val == null)
			{
				NChooseACardSelectionScreen val5 = (NChooseACardSelectionScreen)(object)((item is NChooseACardSelectionScreen) ? item : null);
				if (val5 != null && ((CanvasItem)val5).IsVisibleInTree())
				{
					val = val5;
					goto IL_00aa;
				}
			}
			if (val2 == null)
			{
				NCardRewardSelectionScreen val6 = (NCardRewardSelectionScreen)(object)((item is NCardRewardSelectionScreen) ? item : null);
				if (val6 != null && ((CanvasItem)val6).IsVisibleInTree())
				{
					val2 = val6;
					goto IL_00aa;
				}
			}
			if (val3 == null)
			{
				NSimpleCardSelectScreen val7 = (NSimpleCardSelectScreen)(object)((item is NSimpleCardSelectScreen) ? item : null);
				if (val7 != null && ((CanvasItem)val7).IsVisibleInTree())
				{
					val3 = val7;
					goto IL_00aa;
				}
			}
			if (val4 == null)
			{
				NMerchantInventory val8 = (NMerchantInventory)(object)((item is NMerchantInventory) ? item : null);
				if (val8 != null && ((CanvasItem)val8).IsVisibleInTree())
				{
					val4 = val8;
				}
			}
			goto IL_00aa;
			IL_00aa:
			if (flag)
			{
				continue;
			}
			NRewardButton val9 = (NRewardButton)(object)((item is NRewardButton) ? item : null);
			if (val9 != null && ((CanvasItem)val9).IsVisibleInTree() && ((NClickableControl)val9).IsEnabled)
			{
				Reward reward = val9.Reward;
				RelicReward val10 = (RelicReward)(object)((reward is RelicReward) ? reward : null);
				if (val10 != null && val10.Relic != null && TarotImmediateEffects.CanWheelOfFortuneCopy(val10.Relic))
				{
					flag = true;
				}
			}
		}
		IReadOnlyList<CardModel> cards = null;
		if (val2 != null)
		{
			IReadOnlyList<CardModel> readOnlyList = PortableTarotRewardContextRegistry.Get((Node)(object)val2);
			if (readOnlyList != null)
			{
				cards = readOnlyList;
				goto IL_01d4;
			}
		}
		if (val3 != null)
		{
			IReadOnlyList<CardModel> readOnlyList2 = PortableTarotRewardContextRegistry.Get((Node)(object)val3);
			if (readOnlyList2 != null)
			{
				cards = readOnlyList2;
				goto IL_01d4;
			}
		}
		if (val4 != null && val4.IsOpen && val4.Inventory != null)
		{
			cards = (from entry in val4.Inventory.CardEntries
				where ((MerchantEntry)entry).IsStocked && entry.CreationResult != null
				select entry.CreationResult.Card).ToArray();
		}
		else if (player.RunState.CurrentRoom is RestSiteRoom)
		{
			cards = player.Deck.Cards;
		}
		goto IL_01d4;
		IL_01d4:
		bool hasMerchantRelic = val4 != null && val4.IsOpen && val4.Inventory != null && val4.Inventory.RelicEntries.Any((MerchantRelicEntry entry) => ((MerchantEntry)entry).IsStocked && entry.Model != null && TarotImmediateEffects.CanWheelOfFortuneCopy(entry.Model));
		bool hasTreasureRelic = false;
		if (player.RunState.CurrentRoom is TreasureRoom)
		{
			TreasureRoomRelicSynchronizer treasureRoomRelicSynchronizer = RunManager.Instance.TreasureRoomRelicSynchronizer;
			IReadOnlyList<RelicModel> currentRelics = treasureRoomRelicSynchronizer.CurrentRelics;
			hasTreasureRelic = currentRelics != null && currentRelics.Any(TarotImmediateEffects.CanWheelOfFortuneCopy) && !treasureRoomRelicSynchronizer.GetPlayerVote(player).voteReceived;
		}
		return new AvailabilitySnapshot(val != null, cards, hasMerchantRelic, flag, hasTreasureRelic);
	}

	public static IReadOnlyList<CardModel>? ResolveActionCards(Player player, PortableTarotUseContext context)
	{
		if (context == PortableTarotUseContext.Deck)
		{
			return player.Deck.Cards;
		}
		return PortableTarotRewardContextRegistry.Get(player, context);
	}

	private static T? FindVisible<T>(Node root) where T : CanvasItem
	{
		return Descendants<T>(root).FirstOrDefault((T node) => ((CanvasItem)node).IsVisibleInTree());
	}

	[IteratorStateMachine(typeof(_003CDescendants_003Ed__20<>))]
	private static IEnumerable<T> Descendants<T>(Node root) where T : Node
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendants_003Ed__20<T>(-2)
		{
			_003C_003E3__root = root
		};
	}

	public static void RefreshVisibleCards(IEnumerable<CardModel> cards)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		HashSet<CardModel> hashSet = cards.ToHashSet();
		foreach (NCard item in Descendants<NCard>((Node)(object)((SceneTree)Engine.GetMainLoop()).Root))
		{
			if (item.Model != null && hashSet.Contains(item.Model))
			{
				item.UpdateVisuals((PileType)0, (CardPreviewMode)1);
			}
		}
	}
}
