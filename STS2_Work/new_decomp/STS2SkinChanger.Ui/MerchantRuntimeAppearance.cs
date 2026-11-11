using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class MerchantRuntimeAppearance
{
	[CompilerGenerated]
	private sealed class _003CEnumerateNodeTree_003Ed__73 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private IEnumerator<Node> _003C_003E7__wrap2;

		Node IEnumerator<Node>.Current
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
		public _003CEnumerateNodeTree_003Ed__73(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
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
			_003C_003E7__wrap2 = null;
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
					_003C_003E2__current = root;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = root.GetChildren(false).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00d0;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00b6;
					}
					IL_00d0:
					if (_003C_003E7__wrap1.MoveNext())
					{
						Node current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = EnumerateNodeTree(current).GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00b6;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_00b6:
					if (_003C_003E7__wrap2.MoveNext())
					{
						Node current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00d0;
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
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			_003CEnumerateNodeTree_003Ed__73 _003CEnumerateNodeTree_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateNodeTree_003Ed__ = this;
			}
			else
			{
				_003CEnumerateNodeTree_003Ed__ = new _003CEnumerateNodeTree_003Ed__73(0);
			}
			_003CEnumerateNodeTree_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateNodeTree_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	internal const string GroupId = "merchant";

	internal const string FakeMerchantGroupId = "fake_merchant_monster";

	internal const string MerchantRoomScenePath = "res://scenes/rooms/merchant_room.tscn";

	private const string FakeMerchantScenePath = "res://scenes/events/custom/fake_merchant.tscn";

	private const string MerchantInventoryScenePath = "res://scenes/merchant/merchant_inventory.tscn";

	private const string MerchantPreviewRootMeta = "skin_changer_merchant_preview_root";

	private const string MerchantPreviewInputProxyName = "SkinChangerMerchantInputProxy";

	private const string MerchantPreviewFocusedMeta = "skin_changer_merchant_preview_focused";

	private const string PlayerBasePositionMeta = "skin_changer_shop_player_base_position";

	private const string PlayerBaseScaleMeta = "skin_changer_shop_player_base_scale";

	private const string ProviderRootBasePositionMeta = "skin_changer_shop_provider_root_position";

	private const string ProviderRootBaseScaleMeta = "skin_changer_shop_provider_root_scale";

	private static readonly FieldInfo? MerchantButtonField = AccessTools.Field(typeof(NMerchantRoom), "<MerchantButton>k__BackingField");

	private static readonly FieldInfo? FakeMerchantButtonField = AccessTools.Field(typeof(NFakeMerchant), "<MerchantButton>k__BackingField");

	private static readonly MethodInfo? FakeMerchantOpenedMethod = AccessTools.Method(typeof(NFakeMerchant), "OnMerchantOpened", (Type[])null, (Type[])null);

	private static readonly FieldInfo? MerchantHandField = AccessTools.Field(typeof(NMerchantInventory), "<MerchantHand>k__BackingField");

	private static readonly FieldInfo? MerchantRoomInventoryField = AccessTools.Field(typeof(NMerchantRoom), "<Inventory>k__BackingField");

	private static readonly FieldInfo? MerchantRoomProceedButtonField = AccessTools.Field(typeof(NMerchantRoom), "_proceedButton");

	private static readonly FieldInfo? FakeMerchantInventoryField = AccessTools.Field(typeof(NFakeMerchant), "<Inventory>k__BackingField");

	private static readonly FieldInfo? FakeMerchantProceedButtonField = AccessTools.Field(typeof(NFakeMerchant), "_proceedButton");

	private static readonly MethodInfo? FakeMerchantOpenInventoryMethod = AccessTools.Method(typeof(NFakeMerchant), "OpenInventory", (Type[])null, (Type[])null);

	private static readonly FieldInfo? MerchantRoomModelField = AccessTools.Field(typeof(NMerchantRoom), "<Room>k__BackingField");

	private static readonly FieldInfo? MerchantRoomPlayersField = AccessTools.Field(typeof(NMerchantRoom), "_players");

	private static readonly FieldInfo? MerchantRoomDialogueField = AccessTools.Field(typeof(NMerchantRoom), "_dialogue");

	private static readonly FieldInfo? MerchantSkeletonField = AccessTools.Field(typeof(NMerchantButton), "_merchantSkeleton");

	private static readonly MethodInfo? MegaSkeletonSetSkinByNameMethod = AccessTools.Method(typeof(MegaSkeleton), "SetSkinByName", (Type[])null, (Type[])null);

	private static readonly List<WeakReference<Node>> ReplayedInventoryAdditions = new List<WeakReference<Node>>();

	private static readonly Dictionary<ulong, List<WeakReference<Node2D>>> ShopProviderRoots = new Dictionary<ulong, List<WeakReference<Node2D>>>();

	internal static NMerchantCharacter? GetLocalPlayerVisual()
	{
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance != null)
		{
			IReadOnlyList<NMerchantCharacter> playerVisuals = instance.PlayerVisuals;
			if (playerVisuals != null && playerVisuals.Count > 0)
			{
				return instance.PlayerVisuals[0];
			}
		}
		return null;
	}

	internal static string? GetSelectedLocalCharacterProvider()
	{
		Player localPlayer = CharacterAppearanceRuntime.GetLocalPlayer();
		if (localPlayer == null)
		{
			return null;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)localPlayer.Character).Id.Entry, ((object)localPlayer.Character).GetType().Name);
		if (skinGroup != null)
		{
			return SkinService.GetSelectedFullRuntimeProvider(skinGroup.Id);
		}
		return null;
	}

	internal static string? BeginSelectedMerchantNodeReady(Node node)
	{
		string text = ResolveMerchantGroupId(node);
		string text2 = ((text == null) ? null : SkinService.GetSelectedFullRuntimeProvider(text));
		if (text2 == null)
		{
			return null;
		}
		TrackInventoryProviderAdditions(node, ManagedSkinModLoader.ReplaySelectedNodeReadyPrefixes(text2, node));
		return text2;
	}

	internal static void CompleteSelectedMerchantNodeReady(Node node, string? providerId)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (providerId == null || !GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return;
		}
		MerchantProviderReadyTarget target = ((!(node is NMerchantButton)) ? ((node is NMerchantHand) ? MerchantProviderReadyTarget.Hand : MerchantProviderReadyTarget.Inventory) : MerchantProviderReadyTarget.Button);
		if (MerchantProviderReadyPolicy.ResolvePostfixTiming(target) == MerchantProviderPostfixTiming.NextFrameThenSpineReady)
		{
			Callable val = Callable.From((Action)delegate
			{
				CompleteSelectedMerchantSpineNodeReady(node, providerId);
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		else
		{
			CompleteSelectedMerchantNodeReadyCore(node, providerId);
		}
	}

	private static void CompleteSelectedMerchantSpineNodeReady(Node node, string providerId)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Expected O, but got Unknown
		if (!IsSelectedProviderForNode(node, providerId))
		{
			return;
		}
		try
		{
			Node obj = node;
			NMerchantButton val = (NMerchantButton)(object)((obj is NMerchantButton) ? obj : null);
			Node val3;
			if (val == null)
			{
				Node obj2 = node;
				NMerchantHand val2 = (NMerchantHand)(object)((obj2 is NMerchantHand) ? obj2 : null);
				val3 = (Node)(object)((val2 == null) ? null : ((Node)val2).GetParent<Node2D>());
			}
			else
			{
				val3 = ((Node)val).GetNodeOrNull<Node>(NodePath.op_Implicit("%MerchantVisual"));
			}
			Node val4 = val3;
			if (val4 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val4))
			{
				CompleteSelectedMerchantNodeReadyCore(node, providerId);
				return;
			}
			MegaSprite val5 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val4));
			SpineNodeExtensions.RunWhenSpineReady(node, val5, (Action<MegaAnimationState>)delegate
			{
				if (IsSelectedProviderForNode(node, providerId))
				{
					CompleteSelectedMerchantNodeReadyCore(node, providerId);
				}
			});
		}
		catch (Exception ex)
		{
			ModLog.Warn("等待商人外观骨骼就绪失败，改为完成原作者初始化：" + ex.GetBaseException().Message);
			if (IsSelectedProviderForNode(node, providerId))
			{
				CompleteSelectedMerchantNodeReadyCore(node, providerId);
			}
		}
	}

	private static bool IsSelectedProviderForNode(Node node, string providerId)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return false;
		}
		string text = ResolveMerchantGroupId(node);
		if (text != null)
		{
			return string.Equals(SkinService.GetSelectedFullRuntimeProvider(text), providerId, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static void CompleteSelectedMerchantNodeReadyCore(Node node, string providerId)
	{
		TrackInventoryProviderAdditions(node, ManagedSkinModLoader.ReplaySelectedNodeReadyPostfixes(providerId, node));
		string text = ResolveMerchantGroupId(node);
		if (text != null)
		{
			ManagedMerchantSettingsBridge.Bind(node, text, providerId);
		}
		if (node is NMerchantInventory)
		{
			MakeProviderInventoryVisualsPassThrough();
		}
	}

	internal static string? ResolveMerchantGroupId(Node node)
	{
		for (Node val = node; val != null; val = val.GetParent())
		{
			if ((val is NFakeMerchant || val is NFakeMerchantInventory) ? true : false)
			{
				return "fake_merchant_monster";
			}
			if (val is NMerchantRoom)
			{
				return "merchant";
			}
		}
		return null;
	}

	private static void TrackInventoryProviderAdditions(Node owner, IEnumerable<Node> addedRoots)
	{
		if (!(owner is NMerchantInventory))
		{
			return;
		}
		Node target;
		HashSet<ulong> hashSet = ReplayedInventoryAdditions.Select((WeakReference<Node> reference) => (!reference.TryGetTarget(out target) || !GodotObject.IsInstanceValid((GodotObject)(object)target)) ? 0 : ((GodotObject)target).GetInstanceId()).ToHashSet();
		foreach (Node item in addedRoots.Where((Func<Node, bool>)GodotObject.IsInstanceValid))
		{
			if (hashSet.Add(((GodotObject)item).GetInstanceId()))
			{
				ReplayedInventoryAdditions.Add(new WeakReference<Node>(item));
			}
		}
	}

	internal static CharacterCombatTransform GetLocalPlayerTransform(string groupId)
	{
		return SkinService.GetCharacterCombatTransform(GetLocalPlayerTransformKey(groupId), SkinService.Config.GetSelection(groupId));
	}

	internal static CharacterCombatTransform SetLocalPlayerTransform(string groupId, CharacterCombatTransform value, bool save)
	{
		string selection = SkinService.Config.GetSelection(groupId);
		CharacterCombatTransform result = SkinService.SetCharacterCombatTransform(GetLocalPlayerTransformKey(groupId), selection, value, save);
		MultiplayerSkinSync.OnLocalTransformChanged(groupId);
		return result;
	}

	internal static void ApplyLocalPlayerTransform(NMerchantCharacter visual, string groupId, CharacterCombatTransform? value = null)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		CaptureLocalPlayerBaseline(visual);
		Variant meta = ((GodotObject)visual).GetMeta(StringName.op_Implicit("skin_changer_shop_player_base_position"), Variant.op_Implicit(((Node2D)visual).Position));
		Vector2 val = ((Variant)(ref meta)).AsVector2();
		meta = ((GodotObject)visual).GetMeta(StringName.op_Implicit("skin_changer_shop_player_base_scale"), Variant.op_Implicit(((Node2D)visual).Scale));
		Vector2 val2 = ((Variant)(ref meta)).AsVector2();
		CharacterCombatTransform characterCombatTransform = value ?? GetLocalPlayerTransform(groupId);
		((Node2D)visual).Position = val + new Vector2(characterCombatTransform.OffsetX, characterCombatTransform.OffsetY);
		((Node2D)visual).Scale = val2 * characterCombatTransform.Scale;
		ApplyProviderRootsTransform(visual, characterCombatTransform);
		ManagedSlotVisibilityBridge.BindPlayerScene((Node)(object)visual, CharacterAppearanceRuntime.GetLocalPlayer());
	}

	internal static void TrackProviderRoots(NMerchantCharacter visual, IEnumerable<Node> addedRoots)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)visual))
		{
			return;
		}
		Node2D[] array = (from node in addedRoots.OfType<Node2D>()
			where GodotObject.IsInstanceValid((GodotObject)(object)node) && node != visual
			select node).ToArray();
		Node2D[] array2 = array.Where((Node2D node) => ((Node)node).GetParent() == ((Node)visual).GetParent()).ToArray();
		Node2D[] candidatePool = ((array2.Length != 0) ? array2 : array);
		float nearestDistance = ((candidatePool.Length == 0) ? float.PositiveInfinity : candidatePool.Min(delegate(Node2D node)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Vector2 globalPosition = node.GlobalPosition;
			return ((Vector2)(ref globalPosition)).DistanceSquaredTo(((Node2D)visual).GlobalPosition);
		}));
		List<WeakReference<Node2D>> list = (from node in candidatePool.Where(delegate(Node2D node)
			{
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				if (nearestDistance < float.PositiveInfinity)
				{
					if (candidatePool.Length != 1)
					{
						Vector2 globalPosition2 = node.GlobalPosition;
						return ((Vector2)(ref globalPosition2)).DistanceSquaredTo(((Node2D)visual).GlobalPosition) <= nearestDistance + 16f;
					}
					return true;
				}
				return false;
			})
			select new WeakReference<Node2D>(node)).ToList();
		ulong instanceId = ((GodotObject)visual).GetInstanceId();
		if (list.Count == 0)
		{
			ShopProviderRoots.Remove(instanceId);
		}
		else
		{
			ShopProviderRoots[instanceId] = list;
		}
	}

	internal static void ScopePartTakeover(Node room)
	{
		PartTakeoverVisualScope.Capture(room, "merchant");
	}

	internal static void PrepareMerchantSelectionChange()
	{
		NMerchantRoom instance = NMerchantRoom.Instance;
		string selectedFullRuntimeProvider = SkinService.GetSelectedFullRuntimeProvider("merchant");
		if (((instance != null) ? instance.Inventory : null) != null && selectedFullRuntimeProvider != null)
		{
			ManagedSkinModLoader.RestoreSelectedNodeReadyBehavior(selectedFullRuntimeProvider, (Node)(object)instance.Inventory);
		}
		WeakReference<Node>[] array = ReplayedInventoryAdditions.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].TryGetTarget(out var target) && GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				Node parent = target.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(target);
				}
				target.QueueFree();
			}
		}
		ReplayedInventoryAdditions.Clear();
	}

	internal static bool TryCreateCurrentMerchantRoom(NMerchantRoom preloadCreatedRoom, out NMerchantRoom? currentRoom, out string? error)
	{
		currentRoom = null;
		error = null;
		if (MerchantRoomModelField == null || MerchantRoomPlayersField == null || MerchantRoomDialogueField == null)
		{
			error = "merchant room state fields unavailable";
			return false;
		}
		try
		{
			bool flag = false;
			NMerchantRoom val;
			try
			{
				val = SkinService.InstantiateRuntimeScene<NMerchantRoom>("merchant", "res://scenes/rooms/merchant_room.tscn", (Action?)null);
			}
			catch (Exception ex)
			{
				ModLog.Warn("当前商人没有独立房间场景，使用游戏场景兜底：" + ex.GetBaseException().Message);
				val = LoadRuntimeOrBaseScene("res://scenes/rooms/merchant_room.tscn").Instantiate<NMerchantRoom>((GenEditState)0);
			}
			try
			{
				MerchantRoomModelField.SetValue(val, MerchantRoomModelField.GetValue(preloadCreatedRoom));
				MerchantRoomDialogueField.SetValue(val, MerchantRoomDialogueField.GetValue(preloadCreatedRoom));
				IEnumerable<Player> collection = (MerchantRoomPlayersField.GetValue(preloadCreatedRoom) as IEnumerable<Player>) ?? throw new InvalidOperationException("预加载商店房间缺少玩家列表");
				((MerchantRoomPlayersField.GetValue(val) as List<Player>) ?? throw new InvalidOperationException("当前商店房间缺少玩家列表")).AddRange(collection);
				currentRoom = val;
				flag = true;
				return true;
			}
			finally
			{
				if (!flag && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					((GodotObject)val).Free();
				}
			}
		}
		catch (Exception ex2)
		{
			error = ex2.GetBaseException().Message;
			ModLog.Error("按当前皮肤创建商店房间失败：" + ex2);
			return false;
		}
	}

	internal static bool IsMerchantPreviewRoot(Node node)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)node) && ((GodotObject)node).HasMeta(StringName.op_Implicit("skin_changer_merchant_preview_root")))
		{
			StringName obj = StringName.op_Implicit("skin_changer_merchant_preview_root");
			Variant val = default(Variant);
			val = ((GodotObject)node).GetMeta(obj, val);
			return ((Variant)(ref val)).AsBool();
		}
		return false;
	}

	private static void InstallMerchantPreviewInputProxy(NMerchantButton button)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		if (((Node)button).GetNodeOrNull<Control>(NodePath.op_Implicit("SkinChangerMerchantInputProxy")) != null)
		{
			return;
		}
		((Control)button).MouseFilter = (MouseFilterEnum)2;
		((Control)button).FocusMode = (FocusModeEnum)0;
		((GodotObject)button).SetMeta(StringName.op_Implicit("skin_changer_merchant_preview_focused"), Variant.op_Implicit(false));
		Control proxy = new Control
		{
			Name = StringName.op_Implicit("SkinChangerMerchantInputProxy"),
			MouseFilter = (MouseFilterEnum)0,
			FocusMode = (FocusModeEnum)2,
			MouseDefaultCursorShape = (CursorShape)2,
			ZIndex = 100
		};
		((Node)button).AddChild((Node)(object)proxy, false, (InternalMode)0);
		proxy.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((CanvasItem)proxy).MoveToFront();
		MerchantPreviewFocusState focusState = MerchantPreviewFocusState.None;
		bool pressed = false;
		proxy.MouseEntered += delegate
		{
			Transition(MerchantPreviewFocusEvent.MouseEntered);
		};
		proxy.MouseExited += delegate
		{
			pressed = false;
			Transition(MerchantPreviewFocusEvent.MouseExited);
		};
		proxy.FocusEntered += delegate
		{
			Transition(MerchantPreviewFocusEvent.ControllerFocused);
		};
		proxy.FocusExited += delegate
		{
			Transition(MerchantPreviewFocusEvent.ControllerUnfocused);
		};
		proxy.GuiInput += (GuiInputEventHandler)delegate(InputEvent inputEvent)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			if (val != null && (long)val.ButtonIndex == 1)
			{
				if (val.Pressed)
				{
					pressed = true;
				}
				else if (pressed)
				{
					pressed = false;
					((NClickableControl)button).ForceClick();
				}
				proxy.AcceptEvent();
			}
			else if (inputEvent.IsActionPressed(MegaInput.select, false, false))
			{
				pressed = true;
				proxy.AcceptEvent();
			}
			else if (inputEvent.IsActionReleased(MegaInput.select, false) && pressed)
			{
				pressed = false;
				((NClickableControl)button).ForceClick();
				proxy.AcceptEvent();
			}
		};
		void Transition(MerchantPreviewFocusEvent focusEvent)
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			bool isFocused = focusState.IsFocused;
			focusState = MerchantPreviewFocusPolicy.Resolve(focusState, focusEvent);
			if (isFocused != focusState.IsFocused)
			{
				((GodotObject)button).SetMeta(StringName.op_Implicit("skin_changer_merchant_preview_focused"), Variant.op_Implicit(focusState.IsFocused));
				ApplyMerchantPreviewFocus(button);
			}
		}
	}

	private static void ApplyMerchantPreviewFocus(NMerchantButton button)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if (!GodotObject.IsInstanceValid((GodotObject)(object)button))
		{
			return;
		}
		object? obj = MerchantSkeletonField?.GetValue(button);
		MegaSkeleton val = (MegaSkeleton)((obj is MegaSkeleton) ? obj : null);
		if (val == null)
		{
			Node nodeOrNull = ((Node)button).GetNodeOrNull<Node>(NodePath.op_Implicit("%MerchantVisual"));
			if (nodeOrNull != null)
			{
				MegaSprite val2 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)nodeOrNull));
				SpineNodeExtensions.RunWhenSpineReady((Node)(object)button, val2, (Action<MegaAnimationState>)delegate
				{
					ApplyMerchantPreviewFocus(button);
				});
			}
			return;
		}
		int num;
		if (((GodotObject)button).HasMeta(StringName.op_Implicit("skin_changer_merchant_preview_focused")))
		{
			NMerchantButton obj2 = button;
			StringName obj3 = StringName.op_Implicit("skin_changer_merchant_preview_focused");
			Variant val3 = default(Variant);
			val3 = ((GodotObject)obj2).GetMeta(obj3, val3);
			num = (((Variant)(ref val3)).AsBool() ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag = (byte)num != 0;
		try
		{
			if (MegaSkeletonSetSkinByNameMethod == null)
			{
				throw new MissingMethodException(typeof(MegaSkeleton).FullName, "SetSkinByName");
			}
			MegaSkeletonSetSkinByNameMethod.Invoke(val, new object[1] { flag ? "outline" : "default" });
			val.SetSlotsToSetupPose();
		}
		catch (Exception ex)
		{
			ModLog.Warn("应用图鉴商人悬浮外观失败：" + ex.GetBaseException().Message);
		}
	}

	internal static NBackButton? PrepareMerchantPreviewInteraction(NMerchantRoom preview, Action<bool>? inventoryVisibilityChanged = null)
	{
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (MerchantButtonField == null || MerchantRoomInventoryField == null || MerchantRoomProceedButtonField == null)
			{
				ModLog.Warn("商人预览缺少原生交互字段，保留按钮视觉但不连接打开逻辑。");
				return null;
			}
			NMerchantButton val = (NMerchantButton)(((object)((Node)preview).GetNodeOrNull<NMerchantButton>(NodePath.op_Implicit("%MerchantButton"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
			NMerchantInventory inventory = (NMerchantInventory)(((object)((Node)preview).GetNodeOrNull<NMerchantInventory>(NodePath.op_Implicit("%Inventory"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
			NProceedButton val2 = (NProceedButton)(((object)((Node)preview).GetNodeOrNull<NProceedButton>(NodePath.op_Implicit("%ProceedButton"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
			if (val == null || inventory == null || val2 == null)
			{
				ModLog.Warn("商人预览场景缺少 MerchantButton/Inventory/ProceedButton，跳过原生交互连接。");
				return null;
			}
			Player obj = CreatePreviewPlayer("SkinChangerMerchantPreview");
			obj.Gold = 9999;
			MerchantInventory val3 = MerchantInventory.CreateForNormalMerchant(obj);
			MerchantRoomInventoryField.SetValue(preview, inventory);
			MerchantRoomProceedButtonField.SetValue(preview, val2);
			MerchantButtonField.SetValue(preview, val);
			val.IsLocalPlayerDead = false;
			val.PlayerDeadLines = MerchantRoom.Dialogue.PlayerDeadLines;
			InstallMerchantPreviewInputProxy(val);
			((Control)inventory).MouseFilter = (MouseFilterEnum)2;
			inventory.Initialize(val3, MerchantRoom.Dialogue);
			((GodotObject)val).Connect(SignalName.MerchantOpened, Callable.From<NMerchantButton>((Action<NMerchantButton>)delegate
			{
				preview.OpenInventory();
				inventoryVisibilityChanged?.Invoke(inventory.IsOpen);
			}), 0u);
			((GodotObject)inventory).Connect(SignalName.InventoryClosed, Callable.From((Action)delegate
			{
				inventoryVisibilityChanged?.Invoke(obj: false);
			}), 0u);
			((CanvasItem)val2).Visible = false;
			ModLog.Info("商人预览已连接游戏原生 MerchantOpened → OpenInventory 交互路径。");
			return ((Node)inventory).GetNodeOrNull<NBackButton>(NodePath.op_Implicit("%BackButton"));
		}
		catch (Exception ex)
		{
			ModLog.Warn("连接商人预览原生交互失败：" + ex.GetBaseException().Message);
			return null;
		}
	}

	internal static NBackButton? PrepareFakeMerchantPreviewInteraction(NFakeMerchant preview, Action<bool>? inventoryVisibilityChanged = null)
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (FakeMerchantInventoryField == null || FakeMerchantProceedButtonField == null || FakeMerchantOpenInventoryMethod == null)
			{
				ModLog.Warn("假商人预览缺少原生交互字段，保留按钮视觉但不连接打开逻辑。");
				return null;
			}
			NMerchantButton val = (NMerchantButton)(((object)((Node)preview).GetNodeOrNull<NMerchantButton>(NodePath.op_Implicit("%MerchantButton"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
			NMerchantInventory inventory = (NMerchantInventory)(((object)((Node)preview).GetNodeOrNull<NMerchantInventory>(NodePath.op_Implicit("%Inventory"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
			NProceedButton val2 = (NProceedButton)(((object)((Node)preview).GetNodeOrNull<NProceedButton>(NodePath.op_Implicit("%ProceedButton"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
			if (val == null || inventory == null || val2 == null)
			{
				ModLog.Warn("假商人预览场景缺少 MerchantButton/Inventory/ProceedButton，跳过原生交互连接。");
				return null;
			}
			Player val3 = CreatePreviewPlayer("SkinChangerFakeMerchantPreview");
			val3.Gold = 9999;
			MerchantInventory val4 = new MerchantInventory(val3);
			foreach (RelicModel item in ModelDb.AllRelics.OrderBy<RelicModel, string>((RelicModel relic) => ((AbstractModel)relic).Id.Entry, StringComparer.OrdinalIgnoreCase).Take(6))
			{
				val4.AddRelicEntry(new MerchantRelicEntry(item.ToMutable(), val3));
			}
			FakeMerchantInventoryField.SetValue(preview, inventory);
			FakeMerchantProceedButtonField.SetValue(preview, val2);
			AccessTools.Field(typeof(NFakeMerchant), "<MerchantButton>k__BackingField")?.SetValue(preview, val);
			InstallMerchantPreviewInputProxy(val);
			((Control)inventory).MouseFilter = (MouseFilterEnum)2;
			inventory.Initialize(val4, FakeMerchant.Dialogue);
			((GodotObject)val).Connect(SignalName.MerchantOpened, Callable.From<NMerchantButton>((Action<NMerchantButton>)delegate
			{
				FakeMerchantOpenInventoryMethod.Invoke(preview, null);
				inventoryVisibilityChanged?.Invoke(inventory.IsOpen);
			}), 0u);
			((GodotObject)inventory).Connect(SignalName.InventoryClosed, Callable.From((Action)delegate
			{
				inventoryVisibilityChanged?.Invoke(obj: false);
			}), 0u);
			((CanvasItem)val2).Visible = false;
			ModLog.Info("假商人预览已连接游戏原生 MerchantOpened → OpenInventory 交互路径。");
			return ((Node)inventory).GetNodeOrNull<NBackButton>(NodePath.op_Implicit("%BackButton"));
		}
		catch (Exception ex)
		{
			ModLog.Warn("连接假商人预览原生交互失败：" + ex.GetBaseException().Message);
			return null;
		}
	}

	private static Player CreatePreviewPlayer(string seed)
	{
		ulong num = LocalContext.NetId ?? 1;
		Player obj = Player.CreateForNewRun((CharacterModel)(object)ModelDb.Character<Ironclad>(), UnlockState.all, num);
		RunState.CreateForTest((IReadOnlyList<Player>)new _003C_003Ez__ReadOnlySingleElementList<Player>(obj), (IReadOnlyList<ActModel>)null, (IReadOnlyList<ModifierModel>)null, (GameMode)1, 0, seed);
		return obj;
	}

	internal static bool TryRefreshMerchant(out string? error)
	{
		error = null;
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			error = "merchant room unavailable";
			return false;
		}
		if (MerchantButtonField == null || MerchantHandField == null)
		{
			error = "merchant visual fields unavailable";
			return false;
		}
		NMerchantButton val = null;
		NMerchantInventory val2 = null;
		Node2D val3 = null;
		NMerchantButton previous = null;
		Node2D previousContainer = null;
		NMerchantHand previousHand = null;
		bool flag = false;
		bool flag2 = false;
		try
		{
			val = InstantiateMerchantButton();
			try
			{
				val2 = SkinService.InstantiateRuntimeScene<NMerchantInventory>("merchant", "res://scenes/merchant/merchant_inventory.tscn", (Action?)null);
			}
			catch (Exception ex)
			{
				ModLog.Warn("当前商人没有独立库存场景，使用游戏场景兜底：" + ex.GetBaseException().Message);
				val2 = LoadRuntimeOrBaseScene("res://scenes/merchant/merchant_inventory.tscn").Instantiate<NMerchantInventory>((GenEditState)0);
			}
			object obj = ((Node)val2).GetNodeOrNull<Node2D>(NodePath.op_Implicit("MerchantHandContainer"));
			if (obj == null)
			{
				NMerchantHand? obj2 = FindMerchantHand((Node)(object)val2);
				Node obj3 = ((obj2 != null) ? ((Node)obj2).GetParent() : null);
				obj = ((obj3 is Node2D) ? obj3 : null) ?? throw new InvalidOperationException("商店库存场景缺少 MerchantHandContainer 节点");
			}
			val3 = (Node2D)obj;
			((Node)val2).RemoveChild((Node)(object)val3);
			ReplaceMerchantButton(instance, val, out previous);
			flag = true;
			ReplaceMerchantHand(instance.Inventory, val3, out previousContainer, out previousHand);
			flag2 = true;
			string selectedFullRuntimeProvider = SkinService.GetSelectedFullRuntimeProvider("merchant");
			if (selectedFullRuntimeProvider != null)
			{
				foreach (Node item in ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(selectedFullRuntimeProvider, (Node)(object)instance.Inventory, includePrefixes: true))
				{
					ReplayedInventoryAdditions.Add(new WeakReference<Node>(item));
				}
				ManagedMerchantSettingsBridge.Bind((Node)(object)instance.Inventory, "merchant", selectedFullRuntimeProvider);
				MakeProviderInventoryVisualsPassThrough();
			}
			Node parent = ((Node)previous).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)previous);
			}
			((Node)previous).QueueFree();
			Node parent2 = ((Node)previousContainer).GetParent();
			if (parent2 != null)
			{
				parent2.RemoveChild((Node)(object)previousContainer);
			}
			((Node)previousContainer).QueueFree();
			val = null;
			val3 = null;
			flag = false;
			flag2 = false;
			PartTakeoverVisualScope.Reconcile((Node)(object)instance, "merchant");
			return true;
		}
		catch (Exception ex3)
		{
			try
			{
				if (flag2 && val3 != null && GodotObject.IsInstanceValid((GodotObject)(object)val3))
				{
					Node parent3 = ((Node)val3).GetParent();
					if (parent3 != null)
					{
						parent3.RemoveChild((Node)(object)val3);
					}
					((GodotObject)val3).Free();
					val3 = null;
				}
				if (flag && val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					Node parent4 = ((Node)val).GetParent();
					if (parent4 != null)
					{
						parent4.RemoveChild((Node)(object)val);
					}
					((GodotObject)val).Free();
					val = null;
				}
				if (flag && previous != null && GodotObject.IsInstanceValid((GodotObject)(object)previous))
				{
					MerchantButtonField.SetValue(instance, previous);
					((Node)previous).Name = StringName.op_Implicit(((object)((Node)previous).Name).ToString().Replace("__SkinChangerPrevious", string.Empty, StringComparison.Ordinal));
				}
				if (flag2 && previousHand != null && GodotObject.IsInstanceValid((GodotObject)(object)previousHand))
				{
					MerchantHandField.SetValue(instance.Inventory, previousHand);
					if (previousContainer != null && GodotObject.IsInstanceValid((GodotObject)(object)previousContainer))
					{
						((Node)previousContainer).Name = StringName.op_Implicit(((object)((Node)previousContainer).Name).ToString().Replace("__SkinChangerPrevious", string.Empty, StringComparison.Ordinal));
					}
				}
			}
			catch (Exception ex2)
			{
				ModLog.Error("回滚商人外观失败：" + ex2);
			}
			error = ex3.GetBaseException().Message;
			ModLog.Error("刷新商人外观失败：" + ex3);
			return false;
		}
		finally
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
			if (val3 != null && GodotObject.IsInstanceValid((GodotObject)(object)val3))
			{
				((GodotObject)val3).Free();
			}
			if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				((GodotObject)val2).Free();
			}
		}
	}

	internal static bool TryRefreshFakeMerchant(NFakeMerchant fakeMerchant, out string? error)
	{
		error = null;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)fakeMerchant) || fakeMerchant.MerchantButton == null || FakeMerchantButtonField == null || FakeMerchantOpenedMethod == null)
		{
			error = "fake merchant visual fields unavailable";
			return false;
		}
		NMerchantButton val = null;
		NMerchantButton val2 = null;
		bool flag = false;
		string text = string.Empty;
		try
		{
			val2 = fakeMerchant.MerchantButton;
			text = StringName.op_Implicit(((Node)val2).Name);
			val = InstantiateMerchantButton("fake_merchant_monster");
			ReplaceMerchantButtonCore(val2, val, fakeMerchant, FakeMerchantButtonField, delegate(NMerchantButton button)
			{
				FakeMerchantOpenedMethod.Invoke(fakeMerchant, new object[1] { button });
			});
			flag = true;
			string selectedFullRuntimeProvider = SkinService.GetSelectedFullRuntimeProvider("fake_merchant_monster");
			Node parent = ((Node)val2).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)val2);
			}
			((Node)val2).QueueFree();
			NFakeMerchant node = fakeMerchant;
			IEnumerable<string> selectedProviderIds;
			if (selectedFullRuntimeProvider != null)
			{
				IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(selectedFullRuntimeProvider);
				selectedProviderIds = enumerable;
			}
			else
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				selectedProviderIds = enumerable;
			}
			ManagedSkinModLoader.RestoreUnselectedNodeReadyBehaviors((Node)(object)node, selectedProviderIds);
			NMerchantInventory inventory = fakeMerchant.Inventory;
			IEnumerable<string> selectedProviderIds2;
			if (selectedFullRuntimeProvider != null)
			{
				IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(selectedFullRuntimeProvider);
				selectedProviderIds2 = enumerable;
			}
			else
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				selectedProviderIds2 = enumerable;
			}
			ManagedSkinModLoader.RestoreUnselectedNodeReadyBehaviors((Node)(object)inventory, selectedProviderIds2);
			NMerchantHand merchantHand = fakeMerchant.Inventory.MerchantHand;
			IEnumerable<string> selectedProviderIds3;
			if (selectedFullRuntimeProvider != null)
			{
				IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(selectedFullRuntimeProvider);
				selectedProviderIds3 = enumerable;
			}
			else
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				selectedProviderIds3 = enumerable;
			}
			ManagedSkinModLoader.RestoreUnselectedNodeReadyBehaviors((Node)(object)merchantHand, selectedProviderIds3);
			if (selectedFullRuntimeProvider != null)
			{
				ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors((Node)(object)fakeMerchant, selectedFullRuntimeProvider);
				ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(selectedFullRuntimeProvider, (Node)(object)fakeMerchant.Inventory, includePrefixes: true);
				ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(selectedFullRuntimeProvider, (Node)(object)fakeMerchant.Inventory.MerchantHand);
				ManagedMerchantSettingsBridge.Bind((Node)(object)fakeMerchant.Inventory, "fake_merchant_monster", selectedFullRuntimeProvider);
				ManagedMerchantSettingsBridge.Bind((Node)(object)fakeMerchant.Inventory.MerchantHand, "fake_merchant_monster", selectedFullRuntimeProvider);
			}
			val = null;
			return true;
		}
		catch (Exception ex)
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				Node parent2 = ((Node)val).GetParent();
				if (parent2 != null)
				{
					parent2.RemoveChild((Node)(object)val);
				}
			}
			if (flag && val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				FakeMerchantButtonField.SetValue(fakeMerchant, val2);
				((Node)val2).Name = StringName.op_Implicit(text);
			}
			error = ex.GetBaseException().Message;
			ModLog.Error("刷新假商人外观失败：" + ex);
			return false;
		}
		finally
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				Node parent3 = ((Node)val).GetParent();
				if (parent3 != null)
				{
					parent3.RemoveChild((Node)(object)val);
				}
				((GodotObject)val).Free();
			}
		}
	}

	internal static bool TryRefreshLocalPlayer(Player player, string groupId, out NMerchantCharacter? refreshedVisual, out string? error)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		refreshedVisual = null;
		error = null;
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			error = "merchant room unavailable";
			return false;
		}
		if (!(instance.PlayerVisuals is List<NMerchantCharacter> { Count: not 0 } list))
		{
			error = "merchant player visual unavailable";
			return false;
		}
		NMerchantCharacter val = null;
		NMerchantCharacter val2 = null;
		Node val3 = null;
		string text = string.Empty;
		int num = -1;
		bool flag = false;
		bool flag2 = false;
		List<Node> list2 = new List<Node>();
		try
		{
			val2 = list[0];
			val3 = ((Node)val2).GetParent() ?? throw new InvalidOperationException("merchant player visual has no parent");
			num = ((Node)val2).GetIndex(false);
			text = ((object)((Node)val2).Name).ToString();
			val = SkinService.GetOrLoadRuntimeScene(groupId, player.Character.MerchantAnimPath).Instantiate<NMerchantCharacter>((GenEditState)0);
			((Node2D)val).Position = Vector2.Zero;
			((CanvasItem)val).Visible = ((CanvasItem)val2).Visible;
			((CanvasItem)val).Modulate = ((CanvasItem)val2).Modulate;
			((CanvasItem)val).SelfModulate = ((CanvasItem)val2).SelfModulate;
			((CanvasItem)val).ZIndex = ((CanvasItem)val2).ZIndex;
			((CanvasItem)val).ZAsRelative = ((CanvasItem)val2).ZAsRelative;
			((Node)val2).Name = StringName.op_Implicit(text + "__SkinChangerPrevious");
			((Node)val).Name = StringName.op_Implicit(text);
			list[0] = val;
			flag = true;
			val3.AddChild((Node)(object)val, false, (InternalMode)0);
			flag2 = true;
			val3.MoveChild((Node)(object)val, num);
			string selectedFullRuntimeProvider = SkinService.GetSelectedFullRuntimeProvider(groupId);
			if (selectedFullRuntimeProvider != null)
			{
				foreach (var item in ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors((Node)(object)instance, selectedFullRuntimeProvider))
				{
					list2.AddRange(item.AddedRoots);
					TrackProviderRoots(val, item.AddedRoots);
				}
			}
			CaptureLocalPlayerBaseline(val);
			ApplyLocalPlayerTransform(val, groupId);
			Node parent = ((Node)val2).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)val2);
			}
			ShopProviderRoots.Remove(((GodotObject)val2).GetInstanceId());
			((Node)val2).QueueFree();
			refreshedVisual = val;
			val = null;
			flag = false;
			flag2 = false;
			return true;
		}
		catch (Exception ex)
		{
			Node[] array = list2.Distinct().ToArray();
			foreach (Node val4 in array)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)val4))
				{
					Node parent2 = val4.GetParent();
					if (parent2 != null)
					{
						parent2.RemoveChild(val4);
					}
					((GodotObject)val4).Free();
				}
			}
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				ShopProviderRoots.Remove(((GodotObject)val).GetInstanceId());
				if (flag2)
				{
					Node parent3 = ((Node)val).GetParent();
					if (parent3 != null)
					{
						parent3.RemoveChild((Node)(object)val);
					}
				}
				((GodotObject)val).Free();
				val = null;
			}
			if (flag && val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				list[0] = val2;
				((Node)val2).Name = StringName.op_Implicit(text);
				if (val3 != null && ((Node)val2).GetParent() == val3 && num >= 0)
				{
					val3.MoveChild((Node)(object)val2, num);
				}
			}
			error = ex.GetBaseException().Message;
			ModLog.Error("刷新商店角色外观失败：" + ex);
			return false;
		}
		finally
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
		}
	}

	private static string GetLocalPlayerTransformKey(string groupId)
	{
		return groupId + "::merchant_pose";
	}

	private static void CaptureLocalPlayerBaseline(NMerchantCharacter visual)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)visual).HasMeta(StringName.op_Implicit("skin_changer_shop_player_base_position")))
		{
			((GodotObject)visual).SetMeta(StringName.op_Implicit("skin_changer_shop_player_base_position"), Variant.op_Implicit(((Node2D)visual).Position));
		}
		if (!((GodotObject)visual).HasMeta(StringName.op_Implicit("skin_changer_shop_player_base_scale")))
		{
			((GodotObject)visual).SetMeta(StringName.op_Implicit("skin_changer_shop_player_base_scale"), Variant.op_Implicit(((Node2D)visual).Scale));
		}
	}

	private static void ApplyProviderRootsTransform(NMerchantCharacter visual, CharacterCombatTransform transform)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (!ShopProviderRoots.TryGetValue(((GodotObject)visual).GetInstanceId(), out List<WeakReference<Node2D>> value))
		{
			return;
		}
		WeakReference<Node2D>[] array = value.ToArray();
		foreach (WeakReference<Node2D> weakReference in array)
		{
			if (!weakReference.TryGetTarget(out var target) || !GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				value.Remove(weakReference);
				continue;
			}
			if (!((GodotObject)target).HasMeta(StringName.op_Implicit("skin_changer_shop_provider_root_position")))
			{
				((GodotObject)target).SetMeta(StringName.op_Implicit("skin_changer_shop_provider_root_position"), Variant.op_Implicit(target.Position));
			}
			if (!((GodotObject)target).HasMeta(StringName.op_Implicit("skin_changer_shop_provider_root_scale")))
			{
				((GodotObject)target).SetMeta(StringName.op_Implicit("skin_changer_shop_provider_root_scale"), Variant.op_Implicit(target.Scale));
			}
			Variant meta = ((GodotObject)target).GetMeta(StringName.op_Implicit("skin_changer_shop_provider_root_position"), Variant.op_Implicit(target.Position));
			Vector2 val = ((Variant)(ref meta)).AsVector2();
			meta = ((GodotObject)target).GetMeta(StringName.op_Implicit("skin_changer_shop_provider_root_scale"), Variant.op_Implicit(target.Scale));
			Vector2 val2 = ((Variant)(ref meta)).AsVector2();
			target.Position = val + new Vector2(transform.OffsetX, transform.OffsetY);
			target.Scale = val2 * transform.Scale;
		}
		if (value.Count == 0)
		{
			ShopProviderRoots.Remove(((GodotObject)visual).GetInstanceId());
		}
	}

	private static void ReplaceMerchantButton(NMerchantRoom room, NMerchantButton replacement, out NMerchantButton previous)
	{
		previous = room.MerchantButton ?? throw new InvalidOperationException("merchant button is unavailable");
		ReplaceMerchantButtonCore(previous, replacement, room, MerchantButtonField, delegate
		{
			room.OpenInventory();
		});
	}

	private static void ReplaceMerchantButtonCore(NMerchantButton previous, NMerchantButton replacement, object targetOwner, FieldInfo targetField, Action<NMerchantButton> openedCallback)
	{
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		Node obj = ((Node)previous).GetParent() ?? throw new InvalidOperationException("merchant button has no parent");
		int index = ((Node)previous).GetIndex(false);
		StringName name = ((Node)previous).Name;
		bool flag = ((Control)previous).HasFocus();
		CopyControlRuntimeState((Control)(object)previous, (Control)(object)replacement);
		CopyRequiredUniqueNode((Node)(object)previous, (Node)(object)replacement, "MerchantSelectionReticle");
		SetOwnerRecursive((Node)(object)replacement, (Node)(object)replacement);
		if (((Node)replacement).GetNodeOrNull<Node>(NodePath.op_Implicit("%MerchantVisual")) == null || ((Node)replacement).GetNodeOrNull<Node>(NodePath.op_Implicit("%MerchantSelectionReticle")) == null)
		{
			throw new InvalidOperationException("商人场景缺少 MerchantVisual 或 MerchantSelectionReticle");
		}
		((Node)previous).Name = StringName.op_Implicit(StringName.op_Implicit(name) + "__SkinChangerPrevious");
		((Node)replacement).Name = name;
		replacement.IsLocalPlayerDead = previous.IsLocalPlayerDead;
		replacement.PlayerDeadLines = previous.PlayerDeadLines;
		obj.AddChild((Node)(object)replacement, false, (InternalMode)0);
		obj.MoveChild((Node)(object)replacement, index);
		CopyControlRuntimeState((Control)(object)previous, (Control)(object)replacement);
		if (((Node)replacement).GetNodeOrNull<Node>(NodePath.op_Implicit("%MerchantVisual")) == null || ((Node)replacement).GetNodeOrNull<Node>(NodePath.op_Implicit("%MerchantSelectionReticle")) == null)
		{
			throw new InvalidOperationException("商人场景初始化后丢失必要的视觉节点");
		}
		((NClickableControl)replacement).SetEnabled(((NClickableControl)previous).IsEnabled);
		((GodotObject)replacement).Connect(SignalName.MerchantOpened, Callable.From<NMerchantButton>(openedCallback), 0u);
		targetField.SetValue(targetOwner, replacement);
		if (flag)
		{
			((Control)replacement).GrabFocus();
		}
	}

	internal static NMerchantButton InstantiateMerchantButton(string groupId = "merchant")
	{
		string merchantScenePath = GetMerchantScenePath(groupId);
		Node val = SkinService.InstantiateRuntimeScene<Node>(groupId, merchantScenePath, (Action?)null);
		try
		{
			NMerchantButton val2 = (NMerchantButton)(((object)val.GetNodeOrNull<NMerchantButton>(NodePath.op_Implicit("SceneContainer/MerchantButton"))) ?? ((object)(/*isinst with value type is only supported in some contexts*/ ?? throw new InvalidOperationException("原生商人场景缺少 SceneContainer/MerchantButton：" + merchantScenePath))));
			Node parent = ((Node)val2).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)val2);
			}
			SetOwnerRecursive((Node)(object)val2, (Node)(object)val2);
			return val2;
		}
		finally
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
		}
	}

	internal static (Node Scene, string ScenePath) InstantiateMerchantPreviewScene(string groupId = "merchant")
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		string merchantScenePath = GetMerchantScenePath(groupId);
		object obj = (groupId.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase) ? ((object)SkinService.InstantiateRuntimeScene<NFakeMerchant>(groupId, merchantScenePath, (Action?)null)) : ((object)SkinService.InstantiateRuntimeScene<NMerchantRoom>(groupId, merchantScenePath, (Action?)null)));
		((GodotObject)obj).SetMeta(StringName.op_Implicit("skin_changer_merchant_preview_root"), Variant.op_Implicit(true));
		PreparePreviewNativeReferences((Node)obj);
		PartTakeoverVisualScope.Capture((Node)obj, groupId);
		return (Scene: (Node)obj, ScenePath: merchantScenePath);
	}

	private static void PreparePreviewNativeReferences(Node scene)
	{
		try
		{
			NMerchantRoom val = (NMerchantRoom)(object)((scene is NMerchantRoom) ? scene : null);
			if (val == null)
			{
				NFakeMerchant val2 = (NFakeMerchant)(object)((scene is NFakeMerchant) ? scene : null);
				if (val2 != null)
				{
					SetPreviewReference(FakeMerchantButtonField, val2, (Node?)(object)ResolveUniqueNode<NMerchantButton>((Node)(object)val2, "MerchantButton"));
					SetPreviewReference(FakeMerchantInventoryField, val2, (Node?)(object)ResolveUniqueNode<NMerchantInventory>((Node)(object)val2, "Inventory"));
				}
			}
			else
			{
				SetPreviewReference(MerchantButtonField, val, (Node?)(object)ResolveUniqueNode<NMerchantButton>((Node)(object)val, "MerchantButton"));
				SetPreviewReference(MerchantRoomInventoryField, val, (Node?)(object)ResolveUniqueNode<NMerchantInventory>((Node)(object)val, "Inventory"));
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("准备商人预览的原生引用失败：" + ex.GetBaseException().Message);
		}
	}

	private static T? ResolveUniqueNode<T>(Node root, string uniqueName) where T : Node
	{
		return root.GetNodeOrNull<T>(NodePath.op_Implicit("%" + uniqueName)) ?? ((T)(object)/*isinst with value type is only supported in some contexts*/);
	}

	private static void SetPreviewReference(FieldInfo? field, object target, Node? node)
	{
		if (!(field == null) && node != null)
		{
			field.SetValue(target, node);
		}
	}

	internal static string GetMerchantScenePath(string groupId = "merchant")
	{
		if (!groupId.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase))
		{
			return "res://scenes/rooms/merchant_room.tscn";
		}
		return "res://scenes/events/custom/fake_merchant.tscn";
	}

	private static PackedScene LoadRuntimeOrBaseScene(string scenePath, string groupId = "merchant")
	{
		return TryLoadRuntimeOrBaseScene(scenePath, groupId) ?? throw new InvalidOperationException("无法加载商店场景：" + scenePath);
	}

	private static PackedScene? TryLoadRuntimeOrBaseScene(string scenePath, string groupId = "merchant")
	{
		try
		{
			PackedScene orLoadRuntimeScene = SkinService.GetOrLoadRuntimeScene(groupId, scenePath);
			if (orLoadRuntimeScene != null)
			{
				return orLoadRuntimeScene;
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("加载商人场景 " + scenePath + " 失败，使用游戏场景兜底：" + ex.GetBaseException().Message);
		}
		return ResourceLoader.Load<PackedScene>(scenePath, (string)null, (CacheMode)1);
	}

	private static void MakeProviderInventoryVisualsPassThrough()
	{
		WeakReference<Node>[] array = ReplayedInventoryAdditions.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].TryGetTarget(out var target) || !GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				continue;
			}
			foreach (Node item in EnumerateNodeTree(target))
			{
				Control val = (Control)(object)((item is Control) ? item : null);
				if (val != null)
				{
					val.MouseFilter = (MouseFilterEnum)2;
				}
			}
		}
	}

	private static void ReplaceMerchantHand(NMerchantInventory inventory, Node2D replacementContainer, out Node2D previousContainer, out NMerchantHand previousHand)
	{
		previousHand = inventory.MerchantHand;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)previousHand))
		{
			previousHand = FindMerchantHand((Node)(object)inventory) ?? throw new InvalidOperationException("当前商店库存缺少 MerchantHand 节点");
			MerchantHandField.SetValue(inventory, previousHand);
		}
		Node parent = ((Node)previousHand).GetParent();
		previousContainer = (Node2D)(object)(((parent is Node2D) ? parent : null) ?? throw new InvalidOperationException("当前 MerchantHand 没有容器节点"));
		Node obj = ((Node)previousContainer).GetParent() ?? throw new InvalidOperationException("merchant hand has no parent");
		int index = ((Node)previousContainer).GetIndex(false);
		StringName name = ((Node)previousContainer).Name;
		CopyCanvasItemVisualState((CanvasItem)(object)previousContainer, (CanvasItem)(object)replacementContainer);
		SetOwnerRecursive((Node)(object)replacementContainer, (Node)(object)replacementContainer);
		NMerchantHand val = FindMerchantHand((Node)(object)replacementContainer) ?? throw new InvalidOperationException("替换后的商人手部缺少 MerchantHand 节点");
		((Node)previousContainer).Name = StringName.op_Implicit(StringName.op_Implicit(name) + "__SkinChangerPrevious");
		((Node)replacementContainer).Name = name;
		obj.AddChild((Node)(object)replacementContainer, false, (InternalMode)0);
		obj.MoveChild((Node)(object)replacementContainer, index);
		if (!GodotObject.IsInstanceValid((GodotObject)(object)val) || !((Node)val).IsInsideTree())
		{
			throw new InvalidOperationException("MerchantHand 在场景初始化后失效");
		}
		MerchantHandField.SetValue(inventory, val);
	}

	private static NMerchantHand? FindMerchantHand(Node root)
	{
		return EnumerateNodeTree(root).OfType<NMerchantHand>().FirstOrDefault();
	}

	private static void CopyCanvasItemVisualState(CanvasItem source, CanvasItem target)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		target.Visible = source.Visible;
		target.Modulate = source.Modulate;
		target.SelfModulate = source.SelfModulate;
		target.ZIndex = source.ZIndex;
		target.ZAsRelative = source.ZAsRelative;
	}

	private static void CopyControlRuntimeState(Control source, Control target)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		CopyCanvasItemVisualState((CanvasItem)(object)source, (CanvasItem)(object)target);
		target.MouseFilter = source.MouseFilter;
		target.FocusMode = source.FocusMode;
	}

	private static void CopyRequiredUniqueNode(Node baseline, Node replacement, string uniqueName)
	{
		if (replacement.GetNodeOrNull<Node>(NodePath.op_Implicit("%" + uniqueName)) == null)
		{
			Node nodeOrNull = baseline.GetNodeOrNull<Node>(NodePath.op_Implicit("%" + uniqueName));
			Node val = ((nodeOrNull != null) ? nodeOrNull.Duplicate(15) : null);
			if (val != null)
			{
				val.Name = StringName.op_Implicit(uniqueName);
				val.UniqueNameInOwner = true;
				replacement.AddChild(val, false, (InternalMode)0);
			}
		}
	}

	private static void SetOwnerRecursive(Node node, Node owner)
	{
		if (node != owner)
		{
			node.Owner = owner;
		}
		foreach (Node child in node.GetChildren(false))
		{
			SetOwnerRecursive(child, owner);
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateNodeTree_003Ed__73))]
	private static IEnumerable<Node> EnumerateNodeTree(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateNodeTree_003Ed__73(-2)
		{
			_003C_003E3__root = root
		};
	}
}
