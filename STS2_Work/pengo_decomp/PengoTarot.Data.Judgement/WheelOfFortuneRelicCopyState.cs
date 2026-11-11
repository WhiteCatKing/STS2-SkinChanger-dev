using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rewards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;
using MegaCrit.Sts2.Core.Rewards;

namespace PengoTarot.Data.Judgement;

public static class WheelOfFortuneRelicCopyState
{
	[CompilerGenerated]
	private sealed class _003CDescendants_003Ed__15<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull, Node
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node node;

		public Node _003C_003E3__node;

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
		public _003CDescendants_003Ed__15(int _003C_003E1__state)
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
					_003C_003E7__wrap1 = node.GetChildren(false).GetEnumerator();
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
			_003CDescendants_003Ed__15<T> _003CDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendants_003Ed__ = this;
			}
			else
			{
				_003CDescendants_003Ed__ = new _003CDescendants_003Ed__15<T>(0);
			}
			_003CDescendants_003Ed__.node = _003C_003E3__node;
			return _003CDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	private static readonly Dictionary<MerchantRelicEntry, int> MerchantCopies = new Dictionary<MerchantRelicEntry, int>();

	private static readonly Dictionary<ulong, RewardsSet> ActiveRewards = new Dictionary<ulong, RewardsSet>();

	private static int _treasureMultiplier = 1;

	public static int TreasureCopies => Math.Max(0, _treasureMultiplier - 1);

	public static void RegisterRewards(RewardsSet set)
	{
		ActiveRewards[set.Player.NetId] = set;
	}

	public static void UnregisterRewards(RewardsSet set)
	{
		if (ActiveRewards.GetValueOrDefault(set.Player.NetId) == set)
		{
			ActiveRewards.Remove(set.Player.NetId);
		}
	}

	public static bool Apply(Player player, PortableTarotUseContext context, int factor)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		switch (context)
		{
		case PortableTarotUseContext.MerchantRelic:
		{
			MerchantInventory merchant = PortableTarotRewardContextRegistry.GetMerchant(player);
			if (merchant == null)
			{
				return false;
			}
			foreach (MerchantRelicEntry item in merchant.RelicEntries.Where((MerchantRelicEntry e) => ((MerchantEntry)e).IsStocked && e.Model != null && TarotImmediateEffects.CanWheelOfFortuneCopy(e.Model)))
			{
				int num = MerchantCopies.GetValueOrDefault(item) + 1;
				MerchantCopies[item] = checked(num * factor - 1);
			}
			RefreshVisuals();
			return true;
		}
		case PortableTarotUseContext.RelicReward:
		{
			if (!ActiveRewards.TryGetValue(player.NetId, out RewardsSet value))
			{
				return false;
			}
			RelicReward[] array = (from r in value.Rewards.OfType<RelicReward>()
				where r.Relic != null && TarotImmediateEffects.CanWheelOfFortuneCopy(r.Relic)
				select r).ToArray();
			if (array.Length == 0)
			{
				return false;
			}
			RelicReward[] array2 = array;
			foreach (RelicReward val in array2)
			{
				for (int j = 1; j < factor; j++)
				{
					value.Rewards.Add((Reward)new RelicReward(ModelDb.GetById<RelicModel>(((AbstractModel)val.Relic).Id).ToMutable(), player));
				}
			}
			RefreshRewardScreen(value);
			return true;
		}
		case PortableTarotUseContext.TreasureRelic:
			checked
			{
				_treasureMultiplier *= factor;
				RefreshVisuals();
				return true;
			}
		default:
			return false;
		}
	}

	public static int TakeMerchantCopies(MerchantRelicEntry entry)
	{
		int valueOrDefault = MerchantCopies.GetValueOrDefault(entry);
		MerchantCopies.Remove(entry);
		return valueOrDefault;
	}

	public static void ResetTreasure()
	{
		_treasureMultiplier = 1;
		RefreshVisuals();
	}

	private static NMerchantInventory? FindVisibleMerchant()
	{
		return Descendants<NMerchantInventory>(Root()).FirstOrDefault((Func<NMerchantInventory, bool>)((NMerchantInventory node) => ((CanvasItem)node).IsVisibleInTree() && node.IsOpen));
	}

	public static void RefreshVisuals()
	{
		foreach (NMerchantRelic item in Descendants<NMerchantRelic>(Root()))
		{
			MerchantRelicEntry value = Traverse.Create((object)item).Field("_relicEntry").GetValue<MerchantRelicEntry>();
			NRelic value2 = Traverse.Create((object)item).Field("_relicNode").GetValue<NRelic>();
			if (value != null && value2 != null)
			{
				RebuildShadows(value2, MerchantCopies.GetValueOrDefault(value));
			}
		}
		foreach (NTreasureRoomRelicHolder item2 in Descendants<NTreasureRoomRelicHolder>(Root()))
		{
			NRelic relic = item2.Relic;
			if (relic != null && Traverse.Create((object)relic).Field("_model").GetValue<RelicModel>() != null)
			{
				RebuildShadows(relic, TreasureCopies);
			}
		}
	}

	private static void RebuildShadows(NRelic original, int count)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		RelicModel value = Traverse.Create((object)original).Field("_model").GetValue<RelicModel>();
		if (value == null || !TarotImmediateEffects.CanWheelOfFortuneCopy(value))
		{
			return;
		}
		Node parent = ((Node)original).GetParent();
		if (parent == null)
		{
			return;
		}
		foreach (Node child in parent.GetChildren(false))
		{
			if (((object)child.Name).ToString().StartsWith("PengoWheelRelicShadow", StringComparison.Ordinal))
			{
				child.QueueFree();
			}
		}
		for (int num = count - 1; num >= 0; num--)
		{
			NRelic val = NRelic.Create(value, (IconSize)1);
			((Node)val).Name = StringName.op_Implicit($"PengoWheelRelicShadow{num}");
			((Control)val).MouseFilter = (MouseFilterEnum)2;
			((CanvasItem)val).Modulate = new Color(0.8f, 0.8f, 0.8f, 0.8f);
			((Control)val).Position = ((Control)original).Position + new Vector2(18f + (float)num * 13f, -18f - (float)num * 5f);
			((Control)val).Scale = ((Control)original).Scale;
			parent.AddChild((Node)(object)val, false, (InternalMode)0);
			parent.MoveChild((Node)(object)val, ((Node)original).GetIndex(false));
		}
	}

	private static void RefreshRewardScreen(RewardsSet set)
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		NRewardsScreen val = Descendants<NRewardsScreen>(Root()).FirstOrDefault((Func<NRewardsScreen, bool>)((NRewardsScreen s) => ((CanvasItem)s).IsVisibleInTree() && Traverse.Create((object)s).Field("_rewardsSet").GetValue<RewardsSet>() == set));
		if (val == null)
		{
			return;
		}
		Control value = Traverse.Create((object)val).Field("_rewardsContainer").GetValue<Control>();
		List<Control> value2 = Traverse.Create((object)val).Field("_rewardButtons").GetValue<List<Control>>();
		HashSet<Reward> represented = (from b in value2.OfType<NRewardButton>()
			select b.Reward).ToHashSet();
		foreach (Reward item in set.Rewards.Where((Reward r) => !represented.Contains(r)))
		{
			NRewardButton val2 = NRewardButton.Create(item, val);
			((GodotObject)val2).Connect(SignalName.RewardClaimed, Callable.From<NRewardButton>((Action<NRewardButton>)val.RewardCollectedFrom), 0u);
			value2.Add((Control)(object)val2);
			((Node)value).AddChild((Node)(object)val2, false, (InternalMode)0);
			item.MarkContentAsSeen();
		}
	}

	private static Node Root()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (Node)(object)((SceneTree)Engine.GetMainLoop()).Root;
	}

	[IteratorStateMachine(typeof(_003CDescendants_003Ed__15<>))]
	private static IEnumerable<T> Descendants<T>(Node node) where T : Node
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendants_003Ed__15<T>(-2)
		{
			_003C_003E3__node = node
		};
	}
}
