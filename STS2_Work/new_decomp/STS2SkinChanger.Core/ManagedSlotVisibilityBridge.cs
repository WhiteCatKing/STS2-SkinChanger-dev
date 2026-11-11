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
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class ManagedSlotVisibilityBridge
{
	private sealed class PreviewBinding(string groupId, string providerId, SlotToggleContract contract, string[] sourceSlots, bool hidden)
	{
		public readonly string GroupId = groupId;

		public readonly string ProviderId = providerId;

		public readonly SlotToggleContract Contract = contract;

		public readonly string[] SourceSlots = sourceSlots;

		public bool Hidden = hidden;
	}

	private sealed class LiveBinding(Node spineNode, string groupId, string providerId, SlotVisibilitySelection[] selections) : IDisposable
	{
		private readonly List<(GodotObject Slot, SlotAlphaMask Mask)> _targets = new List<(GodotObject, SlotAlphaMask)>();

		private Callable _callback;

		private bool _connected;

		private bool _disposed;

		public bool Matches(string group, string? provider, SlotVisibilitySelection[] current)
		{
			if (!_disposed && groupId.Equals(group, StringComparison.OrdinalIgnoreCase) && providerId.Equals(provider, StringComparison.OrdinalIgnoreCase))
			{
				return selections.SequenceEqual(current);
			}
			return false;
		}

		public void Start()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			spineNode.TreeExiting += Dispose;
			MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spineNode));
			SpineNodeExtensions.RunWhenSpineReady(spineNode, sprite, (Action<MegaAnimationState>)delegate
			{
				Initialize(sprite);
			});
		}

		private void Initialize(MegaSprite sprite)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			if (_disposed || !GodotObject.IsInstanceValid((GodotObject)(object)spineNode) || !spineNode.IsInsideTree())
			{
				return;
			}
			try
			{
				Dictionary<string, GodotObject> slots = new Dictionary<string, GodotObject>(StringComparer.Ordinal);
				MegaSkeleton skeleton = sprite.GetSkeleton();
				GodotObject val = ((skeleton != null) ? ((MegaSpineBinding)skeleton).BoundObject : null);
				if (val == null)
				{
					Dispose();
					return;
				}
				Variant val2 = val.Call(StringName.op_Implicit("get_slots"), Array.Empty<Variant>());
				foreach (Variant item in ((Variant)(ref val2)).AsGodotArray())
				{
					Variant current = item;
					GodotObject val3 = ((Variant)(ref current)).AsGodotObject();
					val2 = val3.Call(StringName.op_Implicit("get_data"), Array.Empty<Variant>());
					val2 = ((Variant)(ref val2)).AsGodotObject().Call(StringName.op_Implicit("get_name"), Array.Empty<Variant>());
					string key = ((Variant)(ref val2)).AsString();
					slots.TryAdd(key, val3);
				}
				string[] array = selections.SelectMany((SlotVisibilitySelection state) => SlotVisibilityPolicy.ResolveSlots(state.SourceSlots, slots.Keys)).Distinct<string>(StringComparer.Ordinal).ToArray();
				if (array.Length == 0)
				{
					ModLog.Warn($"部件显隐未找到已验证的骨骼映射 group={groupId} provider={providerId} scene={spineNode.SceneFilePath}");
					Dispose();
					return;
				}
				string[] array2 = array;
				foreach (string key2 in array2)
				{
					_targets.Add((slots[key2], new SlotAlphaMask()));
				}
				if (((GodotObject)spineNode).HasSignal(StringName.op_Implicit("world_transforms_changed")))
				{
					_callback = Callable.From<Variant>((Action<Variant>)delegate
					{
						Apply();
					});
					((GodotObject)spineNode).Connect(StringName.op_Implicit("world_transforms_changed"), _callback, 0u);
					_connected = true;
				}
				Apply();
				ModLog.Info($"部件显隐已恢复 group={groupId} provider={providerId} slots={string.Join(",", array)}");
			}
			catch (Exception ex)
			{
				Dispose();
				ModLog.Warn("初始化骨骼部件显隐失败：" + ex.GetBaseException().Message);
			}
		}

		private void Apply()
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			if (_disposed)
			{
				return;
			}
			try
			{
				foreach (var (val, slotAlphaMask) in _targets)
				{
					if (GodotObject.IsInstanceValid(val))
					{
						Variant val2 = val.Call(StringName.op_Implicit("get_color"), Array.Empty<Variant>());
						Color val3 = ((Variant)(ref val2)).AsColor();
						val3.A = slotAlphaMask.Hide(val3.A);
						val.Call(StringName.op_Implicit("set_color"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(val3) });
					}
				}
			}
			catch (Exception ex)
			{
				Dispose();
				ModLog.Warn("骨骼部件已失效，停止显隐刷新：" + ex.GetBaseException().Message);
			}
		}

		public void Dispose()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)spineNode))
			{
				spineNode.TreeExiting -= Dispose;
				if (_connected && ((GodotObject)spineNode).IsConnected(StringName.op_Implicit("world_transforms_changed"), _callback))
				{
					((GodotObject)spineNode).Disconnect(StringName.op_Implicit("world_transforms_changed"), _callback);
				}
			}
			foreach (var (val, slotAlphaMask) in _targets)
			{
				if (GodotObject.IsInstanceValid(val))
				{
					try
					{
						Variant val2 = val.Call(StringName.op_Implicit("get_color"), Array.Empty<Variant>());
						Color val3 = ((Variant)(ref val2)).AsColor();
						val3.A = slotAlphaMask.Restore(val3.A);
						val.Call(StringName.op_Implicit("set_color"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(val3) });
					}
					catch
					{
					}
				}
			}
			_targets.Clear();
		}
	}

	[CompilerGenerated]
	private sealed class _003CWalk_003Ed__10 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
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
		public _003CWalk_003Ed__10(int _003C_003E1__state)
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
						_003C_003E7__wrap2 = Walk(current).GetEnumerator();
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
			_003CWalk_003Ed__10 _003CWalk_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CWalk_003Ed__ = this;
			}
			else
			{
				_003CWalk_003Ed__ = new _003CWalk_003Ed__10(0);
			}
			_003CWalk_003Ed__.root = _003C_003E3__root;
			return _003CWalk_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private static readonly Dictionary<Type, SlotToggleContract?> Contracts = new Dictionary<Type, SlotToggleContract>();

	private static readonly HashSet<MethodInfo> PatchedInputs = new HashSet<MethodInfo>();

	private static readonly ConditionalWeakTable<object, PreviewBinding> Previews = new ConditionalWeakTable<object, PreviewBinding>();

	private static readonly ConditionalWeakTable<Node, LiveBinding> LiveBindings = new ConditionalWeakTable<Node, LiveBinding>();

	private static readonly ConditionalWeakTable<Node, object> BoundRoots = new ConditionalWeakTable<Node, object>();

	private static readonly Harmony Observer = new Harmony("Gurio.SkinChanger.slot_visibility");

	internal static void BindPreview(Node root, string groupId, string providerId)
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		string[] slots = default(string[]);
		foreach (Control item in Walk(root).OfType<Control>())
		{
			try
			{
				Type type = ((object)item).GetType();
				if (!ManagedSkinModLoader.IsProviderAssemblyFor(providerId, type.Assembly))
				{
					continue;
				}
				if (!Contracts.TryGetValue(type, out SlotToggleContract contract))
				{
					contract = SlotToggleContract.TryCreate(type);
					Contracts[type] = contract;
				}
				int num;
				if (!(contract == null))
				{
					object value = contract.Slots.GetValue(item);
					slots = value as string[];
					num = ((slots == null) ? 1 : 0);
				}
				else
				{
					num = 1;
				}
				bool flag = (byte)num != 0;
				if (!flag)
				{
					int num2 = slots.Length;
					bool flag2 = ((num2 > 256 || num2 == 0) ? true : false);
					flag = flag2;
				}
				if (!flag && !slots.Any(string.IsNullOrWhiteSpace))
				{
					if (!PatchedInputs.Contains(contract.Input))
					{
						Observer.Patch((MethodBase)contract.Input, (HarmonyMethod)null, new HarmonyMethod(typeof(ManagedSlotVisibilityBridge), "AfterProviderInput", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
						PatchedInputs.Add(contract.Input);
					}
					SlotVisibilitySelection slotVisibilitySelection = SkinService.GetSlotVisibilitySelections(groupId, providerId).FirstOrDefault((SlotVisibilitySelection state) => state.ToggleId == contract.Id && state.SourceSlots.ToHashSet<string>(StringComparer.Ordinal).SetEquals(slots));
					if (slotVisibilitySelection != null)
					{
						contract.State.SetValue(item, slotVisibilitySelection.Hidden);
						contract.Apply.Invoke(item, null);
					}
					Previews.Remove(item);
					Previews.Add(item, new PreviewBinding(groupId, providerId, contract, slots.ToArray(), (bool)contract.State.GetValue(item)));
					ModLog.Info($"部件显隐开关已接入 group={groupId} provider={providerId} toggle={contract.Id} hidden={slotVisibilitySelection?.Hidden ?? false}");
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn("接入皮肤部件开关失败，保留原交互：" + ex.GetBaseException().Message);
			}
		}
	}

	private static void AfterProviderInput(object __instance)
	{
		try
		{
			if (!Previews.TryGetValue(__instance, out PreviewBinding value))
			{
				return;
			}
			Control val = (Control)((__instance is Control) ? __instance : null);
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val) || !((Node)val).IsInsideTree() || !((CanvasItem)val).IsVisibleInTree())
			{
				return;
			}
			bool flag = (bool)value.contract.State.GetValue(__instance);
			if (flag != value.hidden)
			{
				value.hidden = flag;
				if (SkinService.GetSelectedManualCharacterVariant(value.groupId) == null)
				{
					SkinService.SaveSlotVisibilitySelection(new SlotVisibilitySelection(value.groupId, value.providerId, value.contract.Id, flag, value.sourceSlots));
				}
				ManualCharacterVariantBridge.AfterSlotToggle(val, value.groupId, value.providerId, value.contract.Id, flag);
				ModLog.Info($"部件显隐选择已保存 group={value.groupId} provider={value.providerId} hidden={flag} slots={value.sourceSlots.Length}");
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("保存皮肤部件开关失败：" + ex.GetBaseException().Message);
		}
	}

	internal static void BindPlayerScene(Node root, Player? player)
	{
		try
		{
			if (player == null)
			{
				return;
			}
			SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)player.Character).Id.Entry, ((object)player.Character).GetType().Name);
			if (skinGroup == null)
			{
				return;
			}
			Player? localPlayer = CharacterAppearanceRuntime.GetLocalPlayer();
			if (((localPlayer != null) ? new ulong?(localPlayer.NetId) : ((ulong?)null)) != player.NetId)
			{
				ManualCharacterVariant selectedManualCharacterVariant = SkinService.GetSelectedManualCharacterVariant(skinGroup.Id);
				if ((object)selectedManualCharacterVariant == null)
				{
					return;
				}
				ManualCharacterState state = selectedManualCharacterVariant.State;
				if ((object)state == null || state.IsStatic)
				{
					return;
				}
			}
			string selectedCreatureRuntimeProvider = SkinService.GetSelectedCreatureRuntimeProvider(skinGroup.Id);
			BindModel(root, skinGroup.Id, selectedCreatureRuntimeProvider);
		}
		catch (Exception ex)
		{
			ModLog.Warn("恢复皮肤部件显隐失败，保留模型：" + ex.GetBaseException().Message);
		}
	}

	internal static void BindModel(Node root, string groupId, string? provider)
	{
		try
		{
			SlotVisibilitySelection[] array = ((provider == null) ? ((IEnumerable<SlotVisibilitySelection>)Array.Empty<SlotVisibilitySelection>()) : ((IEnumerable<SlotVisibilitySelection>)SkinService.GetSlotVisibilitySelections(groupId, provider))).Where((SlotVisibilitySelection state) => state.Hidden).ToArray();
			if (array.Length == 0 && !BoundRoots.TryGetValue(root, out object _))
			{
				return;
			}
			if (array.Length != 0)
			{
				BoundRoots.GetOrCreateValue(root);
			}
			foreach (Node item in from node in Walk(root)
				where ((GodotObject)node).GetClass().ToString() == "SpineSprite"
				select node)
			{
				if (LiveBindings.TryGetValue(item, out LiveBinding value2))
				{
					if (value2.Matches(groupId, provider, array))
					{
						continue;
					}
					value2.Dispose();
				}
				LiveBindings.Remove(item);
				if (array.Length != 0)
				{
					LiveBinding liveBinding = new LiveBinding(item, groupId, provider, array);
					LiveBindings.Add(item, liveBinding);
					liveBinding.Start();
				}
			}
			if (array.Length == 0)
			{
				BoundRoots.Remove(root);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("恢复皮肤部件显隐失败，保留模型：" + ex.GetBaseException().Message);
		}
	}

	[IteratorStateMachine(typeof(_003CWalk_003Ed__10))]
	private static IEnumerable<Node> Walk(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWalk_003Ed__10(-2)
		{
			_003C_003E3__root = root
		};
	}
}
