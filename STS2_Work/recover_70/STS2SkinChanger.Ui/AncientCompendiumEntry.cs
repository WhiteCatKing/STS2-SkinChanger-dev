using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.addons.mega_text;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class AncientCompendiumEntry
{
	private sealed class AttachmentState
	{
		public NCompendiumBottomButton? Button { get; set; }

		public int DeferredAttempts { get; set; }

		public bool DeferredScheduled { get; set; }
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateDescendants_003Ed__18 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private Node _003Cchild_003E5__3;

		private IEnumerator<Node> _003C_003E7__wrap3;

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
		public _003CEnumerateDescendants_003Ed__18(int _003C_003E1__state)
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
					break;
				case 1:
					_003C_003E1__state = -3;
					_003C_003E7__wrap3 = EnumerateDescendants(_003Cchild_003E5__3).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00c4;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00c4;
					}
					IL_00c4:
					if (_003C_003E7__wrap3.MoveNext())
					{
						Node current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					_003Cchild_003E5__3 = null;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_003Cchild_003E5__3 = _003C_003E7__wrap1.Current;
					_003C_003E2__current = _003Cchild_003E5__3;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
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
		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			_003CEnumerateDescendants_003Ed__18 _003CEnumerateDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateDescendants_003Ed__ = this;
			}
			else
			{
				_003CEnumerateDescendants_003Ed__ = new _003CEnumerateDescendants_003Ed__18(0);
			}
			_003CEnumerateDescendants_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const string ButtonName = "STS2AncientCompendiumButton";

	private const string ScreenName = "STS2AncientCompendium";

	private const string OverlayName = "STS2AncientCompendiumEntryOverlay";

	private static readonly FieldInfo StackField = AccessTools.Field(typeof(NSubmenu), "_stack");

	private static readonly ConditionalWeakTable<NCompendiumSubmenu, AttachmentState> AttachmentStates = new ConditionalWeakTable<NCompendiumSubmenu, AttachmentState>();

	private static NCompendiumBottomButton? _entryButton;

	public static void Attach(NCompendiumSubmenu compendium)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)compendium))
		{
			return;
		}
		AttachmentState orCreateValue = AttachmentStates.GetOrCreateValue(compendium);
		NCompendiumBottomButton val = FindOwnButton((Node)(object)compendium) ?? orCreateValue.Button;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			val = null;
		}
		HBoxContainer nodeOrNull = ((Node)compendium).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("MarginContainer/VBoxContainer/BottomRow"));
		Node val2 = FindVisibleArchiveButton((Node)(object)compendium);
		Node val3 = ((val2 == null) ? null : FindInsertionHost(val2, (Node)(object)compendium));
		if (val == null)
		{
			if (nodeOrNull == null && val3 == null)
			{
				ScheduleDeferredAttach(compendium, orCreateValue);
				return;
			}
			val = (orCreateValue.Button = CreateButton(compendium));
			if (nodeOrNull != null)
			{
				((Node)nodeOrNull).AddChild((Node)(object)val, false, (InternalMode)0);
			}
			else
			{
				val3.AddChild((Node)(object)val, false, (InternalMode)0);
			}
		}
		orCreateValue.Button = val;
		_entryButton = val;
		if (!TryAttachToVisibleHost(compendium, val, val2, val3))
		{
			ScheduleDeferredAttach(compendium, orCreateValue);
		}
	}

	private static NCompendiumBottomButton CreateButton(NCompendiumSubmenu compendium)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		string scenePath = SceneHelper.GetScenePath("screens/main_menu/compendium_bottom_button");
		NCompendiumBottomButton val = PreloadManager.Cache.GetScene(scenePath).Instantiate<NCompendiumBottomButton>((GenEditState)0);
		((Node)val).Name = StringName.op_Implicit("STS2AncientCompendiumButton");
		((Control)val).FocusMode = (FocusModeEnum)2;
		MegaLabel buttonLabel = ((Node)val).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("Label"));
		MegaLabel obj = buttonLabel;
		if (obj != null)
		{
			obj.SetTextAutoSize(ModLocalization.Get(ModText.OtherCompendium));
		}
		if (buttonLabel != null)
		{
			ModLocalization.Bind((Node)(object)val, delegate
			{
				buttonLabel.SetTextAutoSize(ModLocalization.Get(ModText.OtherCompendium));
			});
		}
		TextureRect nodeOrNull = ((Node)val).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Icon"));
		if (nodeOrNull != null)
		{
			((Control)nodeOrNull).OffsetLeft = 70f;
			((Control)nodeOrNull).OffsetTop = 22f;
			((Control)nodeOrNull).OffsetRight = -70f;
			((Control)nodeOrNull).OffsetBottom = -62f;
			nodeOrNull.StretchMode = (StretchModeEnum)5;
			AncientEventModel val2 = GetAncients().FirstOrDefault();
			if (val2 != null)
			{
				nodeOrNull.Texture = val2.MapIcon;
			}
		}
		((GodotObject)val).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			Open(compendium);
		}), 0u);
		return val;
	}

	private static bool TryAttachToVisibleHost(NCompendiumSubmenu compendium, NCompendiumBottomButton button, Node? visibleAnchor, Node? visibleHost)
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)button))
		{
			return false;
		}
		if (visibleAnchor == null)
		{
			visibleAnchor = FindVisibleArchiveButton((Node)(object)compendium);
		}
		if (visibleHost == null)
		{
			visibleHost = ((visibleAnchor == null) ? null : FindInsertionHost(visibleAnchor, (Node)(object)compendium));
		}
		if (visibleHost == null || !IsVisibleInTree(visibleHost))
		{
			HBoxContainer nodeOrNull = ((Node)compendium).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("MarginContainer/VBoxContainer/BottomRow"));
			if (nodeOrNull == null || !IsVisibleInTree((Node)(object)nodeOrNull))
			{
				return false;
			}
			((CanvasItem)button).Show();
			ConfigureFocus(compendium, button, (Node)(object)nodeOrNull);
			return true;
		}
		HBoxContainer nodeOrNull2 = ((Node)compendium).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("MarginContainer/VBoxContainer/BottomRow"));
		if (visibleHost != nodeOrNull2)
		{
			return TryAttachToOverlay(compendium, button, visibleAnchor);
		}
		Node parent = ((Node)button).GetParent();
		if (parent != visibleHost)
		{
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)button);
			}
			visibleHost.AddChild((Node)(object)button, false, (InternalMode)0);
		}
		((CanvasItem)button).Show();
		if (((visibleAnchor != null) ? visibleAnchor.GetParent() : null) == visibleHost)
		{
			visibleHost.MoveChild((Node)(object)button, Math.Min(visibleAnchor.GetIndex(false) + 1, Math.Max(visibleHost.GetChildCount(false) - 1, 0)));
			if (!(visibleHost is Container))
			{
				Control val = (Control)(object)((visibleAnchor is Control) ? visibleAnchor : null);
				if (val != null)
				{
					if (button != null)
					{
						((Control)button).Size = val.Size;
						((Control)button).Position = val.Position + new Vector2(val.Size.X + 24f, 0f);
					}
				}
			}
		}
		ConfigureFocus(compendium, button, visibleHost);
		return true;
	}

	private static bool TryAttachToOverlay(NCompendiumSubmenu compendium, NCompendiumBottomButton button, Node? visibleAnchor)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		if (compendium == null || !((CanvasItem)compendium).IsVisibleInTree())
		{
			return false;
		}
		Control val = ((Node)compendium).GetNodeOrNull<Control>(NodePath.op_Implicit("STS2AncientCompendiumEntryOverlay"));
		if (val == null)
		{
			val = new Control
			{
				Name = StringName.op_Implicit("STS2AncientCompendiumEntryOverlay"),
				MouseFilter = (MouseFilterEnum)2,
				ZIndex = 20
			};
			((Node)compendium).AddChild((Node)(object)val, false, (InternalMode)0);
			val.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		}
		Node parent = ((Node)button).GetParent();
		if (parent != val)
		{
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)button);
			}
			((Node)val).AddChild((Node)(object)button, false, (InternalMode)0);
		}
		((Control)button).SetAnchorsAndOffsetsPreset((LayoutPreset)0, (LayoutPresetMode)0, 0);
		((CanvasItem)button).Show();
		((Control)button).Scale = new Vector2(0.68f, 0.68f);
		Vector2 val2 = ((Control)button).Size;
		if (val2.X <= 1f || val2.Y <= 1f)
		{
			Control val3 = (Control)(object)((visibleAnchor is Control) ? visibleAnchor : null);
			val2 = (((Control)button).Size = (Vector2)((val3 != null && val3.Size.X > 1f) ? val3.Size : new Vector2(260f, 110f)));
		}
		Rect2? visibleArchiveRect = GetVisibleArchiveRect((Node)(object)compendium);
		Rect2 visibleRect = ((Node)compendium).GetViewport().GetVisibleRect();
		Vector2 val5 = val2 * ((Control)button).Scale;
		Vector2 val6 = default(Vector2);
		((Vector2)(ref val6))._002Ector(((Rect2)(ref visibleRect)).End.X - val5.X - 24f, ((Rect2)(ref visibleRect)).End.Y - val5.Y - 24f);
		if (visibleArchiveRect.HasValue)
		{
			Rect2 value = visibleArchiveRect.Value;
			float num = ((Rect2)(ref value)).End.X + 24f;
			value = visibleArchiveRect.Value;
			Vector2 val7 = default(Vector2);
			((Vector2)(ref val7))._002Ector(num, ((Rect2)(ref value)).Position.Y);
			value = visibleArchiveRect.Value;
			float x = ((Rect2)(ref value)).Position.X;
			value = visibleArchiveRect.Value;
			Vector2 val8 = default(Vector2);
			((Vector2)(ref val8))._002Ector(x, ((Rect2)(ref value)).End.Y + 24f);
			if (val7.X + val5.X <= ((Rect2)(ref visibleRect)).End.X - 24f)
			{
				val6 = val7;
			}
			else if (val8.Y + val5.Y <= ((Rect2)(ref visibleRect)).End.Y - 24f)
			{
				val6 = val8;
			}
		}
		Transform2D globalTransformWithCanvas = ((CanvasItem)val).GetGlobalTransformWithCanvas();
		((Control)button).Position = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * val6;
		ConfigureOverlayFocus(button, visibleAnchor);
		return true;
	}

	private static Rect2? GetVisibleArchiveRect(Node root)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Control[] array = (from node in EnumerateDescendants(root)
			where (node is NCompendiumBottomButton || node is NShortSubmenuButton) ? true : false
			select node).Where(delegate(Node node)
		{
			if (!((object)node.Name).ToString().Equals("STS2AncientCompendiumButton", StringComparison.Ordinal))
			{
				Control val = (Control)(object)((node is Control) ? node : null);
				if (val != null)
				{
					return ((CanvasItem)val).IsVisibleInTree();
				}
			}
			return false;
		}).OfType<Control>().ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		Rect2 value = array[0].GetGlobalRect();
		foreach (Control item in array.Skip(1))
		{
			value = ((Rect2)(ref value)).Merge(item.GetGlobalRect());
		}
		return value;
	}

	private static void ConfigureOverlayFocus(NCompendiumBottomButton button, Node? visibleAnchor)
	{
		Control val = (Control)(object)((visibleAnchor is Control) ? visibleAnchor : null);
		if (val != null)
		{
			((Control)button).FocusNeighborLeft = ((Node)val).GetPath();
			val.FocusNeighborRight = ((Node)button).GetPath();
		}
	}

	private static void ConfigureFocus(NCompendiumSubmenu compendium, NCompendiumBottomButton button, Node host)
	{
		Control[] source = (from control in ((IEnumerable)host.GetChildren(false)).OfType<Control>()
			where control != button
			select control).ToArray();
		int index = ((Node)button).GetIndex(false);
		Control val = ((IEnumerable<Control>)source).LastOrDefault((Func<Control, bool>)((Control control) => ((Node)control).GetIndex(false) < index));
		Control val2 = ((IEnumerable<Control>)source).FirstOrDefault((Func<Control, bool>)((Control control) => ((Node)control).GetIndex(false) > index));
		if (val != null)
		{
			((Control)button).FocusNeighborLeft = ((Node)val).GetPath();
		}
		if (val2 != null)
		{
			((Control)button).FocusNeighborRight = ((Node)val2).GetPath();
		}
		NCompendiumBottomButton nodeOrNull = ((Node)compendium).GetNodeOrNull<NCompendiumBottomButton>(NodePath.op_Implicit("%StatisticsButton"));
		NCompendiumBottomButton nodeOrNull2 = ((Node)compendium).GetNodeOrNull<NCompendiumBottomButton>(NodePath.op_Implicit("%RunHistoryButton"));
		if (nodeOrNull != null && nodeOrNull2 != null)
		{
			NShortSubmenuButton nodeOrNull3 = ((Node)compendium).GetNodeOrNull<NShortSubmenuButton>(NodePath.op_Implicit("%BestiaryButton"));
			((Control)nodeOrNull).FocusNeighborRight = ((Node)button).GetPath();
			((Control)button).FocusNeighborLeft = ((Node)nodeOrNull).GetPath();
			((Control)button).FocusNeighborRight = ((Node)nodeOrNull2).GetPath();
			((Control)button).FocusNeighborTop = ((nodeOrNull3 != null) ? ((Node)nodeOrNull3).GetPath() : null) ?? ((Node)button).GetPath();
			((Control)button).FocusNeighborBottom = ((Node)button).GetPath();
			((Control)nodeOrNull2).FocusNeighborLeft = ((Node)button).GetPath();
			if (nodeOrNull3 != null)
			{
				((Control)nodeOrNull3).FocusNeighborBottom = ((Node)button).GetPath();
			}
		}
	}

	private static void ScheduleDeferredAttach(NCompendiumSubmenu compendium, AttachmentState state)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (state.DeferredScheduled || state.DeferredAttempts >= 12)
		{
			return;
		}
		state.DeferredScheduled = true;
		state.DeferredAttempts++;
		Callable val = Callable.From((Action)delegate
		{
			state.DeferredScheduled = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)compendium))
			{
				Attach(compendium);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static NCompendiumBottomButton? FindOwnButton(Node root)
	{
		return EnumerateDescendants(root).OfType<NCompendiumBottomButton>().FirstOrDefault((Func<NCompendiumBottomButton, bool>)((NCompendiumBottomButton button) => ((object)((Node)button).Name).ToString().Equals("STS2AncientCompendiumButton", StringComparison.Ordinal)));
	}

	private static Node? FindVisibleArchiveButton(Node root)
	{
		return (from node in EnumerateDescendants(root)
			where (node is NCompendiumBottomButton || node is NShortSubmenuButton) ? true : false
			where !((object)node.Name).ToString().Equals("STS2AncientCompendiumButton", StringComparison.Ordinal) && IsVisibleInTree(node)
			orderby ((object)node.GetPath()).ToString().Length
			select node).FirstOrDefault();
	}

	private static Node? FindInsertionHost(Node anchor, Node root)
	{
		Node parent = anchor.GetParent();
		while (parent != null && parent != root)
		{
			if (parent is Container && IsVisibleInTree(parent))
			{
				return parent;
			}
			parent = parent.GetParent();
		}
		Node parent2 = anchor.GetParent();
		if (parent2 == null || !IsVisibleInTree(parent2))
		{
			return null;
		}
		return parent2;
	}

	private static bool IsVisibleInTree(Node node)
	{
		CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val != null)
		{
			return val.IsVisibleInTree();
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CEnumerateDescendants_003Ed__18))]
	private static IEnumerable<Node> EnumerateDescendants(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateDescendants_003Ed__18(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static void Open(NCompendiumSubmenu compendium)
	{
		object? value = StackField.GetValue(compendium);
		NSubmenuStack val = (NSubmenuStack)((value is NSubmenuStack) ? value : null);
		if (val == null)
		{
			ModLog.Error("无法取得图鉴菜单栈，其它图鉴未打开。");
		}
		else
		{
			OpenFromStack(val);
		}
	}

	internal static void OpenFromStack(NSubmenuStack stack)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)stack))
		{
			ModLog.Error("无法取得图鉴菜单栈，其它图鉴未打开。");
			return;
		}
		AncientCompendiumScreen ancientCompendiumScreen = ((Node)stack).GetNodeOrNull<AncientCompendiumScreen>(NodePath.op_Implicit("STS2AncientCompendium"));
		if (ancientCompendiumScreen == null)
		{
			AncientCompendiumScreen ancientCompendiumScreen2 = new AncientCompendiumScreen();
			((Node)ancientCompendiumScreen2).Name = StringName.op_Implicit("STS2AncientCompendium");
			((CanvasItem)ancientCompendiumScreen2).Visible = false;
			((Control)ancientCompendiumScreen2).MouseFilter = (MouseFilterEnum)2;
			ancientCompendiumScreen = ancientCompendiumScreen2;
			((Node)stack).AddChild((Node)(object)ancientCompendiumScreen, false, (InternalMode)0);
		}
		stack.Push((NSubmenu)(object)ancientCompendiumScreen);
	}

	internal static AncientEventModel[] GetAncients()
	{
		return ModelDb.AllAncients.Where((AncientEventModel ancient) => ResourceLoader.Exists(GetScenePath(ancient), "")).DistinctBy((AncientEventModel ancient) => ((AbstractModel)ancient).Id).OrderBy<AncientEventModel, string>((AncientEventModel ancient) => GetTitle(ancient), StringComparer.CurrentCultureIgnoreCase)
			.ToArray();
	}

	internal static string GetScenePath(AncientEventModel ancient)
	{
		return "res://scenes/events/background_scenes/" + ((AbstractModel)ancient).Id.Entry.ToLowerInvariant() + ".tscn";
	}

	internal static string GetTitle(AncientEventModel ancient)
	{
		try
		{
			return ((EventModel)ancient).Title.GetFormattedText();
		}
		catch
		{
			return ((AbstractModel)ancient).Id.Entry.Replace('_', ' ').CapitalizeWords();
		}
	}

	internal static SkinGroup? FindGroup(string modelId)
	{
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog == null)
		{
			return null;
		}
		string token = NormalizeToken(modelId);
		return catalog.Groups.FirstOrDefault((SkinGroup candidate) => NormalizeToken(candidate.Id) == token);
	}

	internal static void ReplaceAncientIcon(AncientEventModel ancient, string resourcePath, ref Texture2D result)
	{
		SkinGroup skinGroup = FindGroup(((AbstractModel)ancient).Id.Entry);
		if (skinGroup == null)
		{
			return;
		}
		try
		{
			Resource orLoadRuntimeResource = SkinService.GetOrLoadRuntimeResource(skinGroup.Id, resourcePath);
			result = (Texture2D)(object)(((orLoadRuntimeResource is Texture2D) ? orLoadRuntimeResource : null) ?? throw new InvalidOperationException("独立先古皮肤资源不是贴图：" + resourcePath));
		}
		catch (Exception value)
		{
			ModLog.Error($"最终接管先古头像 {resourcePath} 失败：{value}");
		}
	}

	internal static void RefreshCompendiumEntryIcon(Node context)
	{
		NCompendiumBottomButton val = _entryButton;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			Node obj = ((Node)context.GetTree().Root).FindChild("STS2AncientCompendiumButton", true, false);
			val = (_entryButton = (NCompendiumBottomButton?)(object)((obj is NCompendiumBottomButton) ? obj : null));
		}
		AncientEventModel val2 = GetAncients().FirstOrDefault();
		if (val != null && val2 != null)
		{
			((Node)val).GetNode<TextureRect>(NodePath.op_Implicit("Icon")).Texture = val2.MapIcon;
		}
	}

	private static string NormalizeToken(string value)
	{
		return Regex.Replace(value, "[^a-zA-Z0-9]", string.Empty).ToLowerInvariant();
	}
}
