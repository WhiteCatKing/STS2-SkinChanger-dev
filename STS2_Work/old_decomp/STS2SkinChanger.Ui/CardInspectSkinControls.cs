using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CardInspectSkinControls
{
	[CompilerGenerated]
	private sealed class _003CDescendants_003Ed__20 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
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
					break;
				case 1:
					_003C_003E1__state = -3;
					_003C_003E7__wrap3 = Descendants(_003Cchild_003E5__3).GetEnumerator();
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
			_003CDescendants_003Ed__20 _003CDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendants_003Ed__ = this;
			}
			else
			{
				_003CDescendants_003Ed__ = new _003CDescendants_003Ed__20(0);
			}
			_003CDescendants_003Ed__.root = _003C_003E3__root;
			return _003CDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const string SelectorName = "STS2IndividualCardSkinSelector";

	private const string DragHandleName = "IndividualCardSkinDragHandle";

	private const string DropdownName = "IndividualCardSkinDropdown";

	private const string UpdatingMeta = "sts2_individual_card_skin_updating";

	private const string OptionsCardMeta = "sts2_individual_card_skin_options_card";

	private const string PreviewCardMeta = "sts2_individual_card_skin_preview_card";

	private const string PreviewOptionMeta = "sts2_individual_card_skin_preview_option";

	public static void Attach(NInspectCardScreen screen)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Expected O, but got Unknown
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Expected O, but got Unknown
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected O, but got Unknown
		if (((Node)screen).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("STS2IndividualCardSkinSelector")) != null)
		{
			return;
		}
		HBoxContainer selector = new HBoxContainer
		{
			Name = StringName.op_Implicit("STS2IndividualCardSkinSelector"),
			AnchorLeft = 0.5f,
			AnchorTop = 0.5f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -168f,
			OffsetTop = -526f,
			OffsetRight = 168f,
			OffsetBottom = -478f,
			GrowHorizontal = (GrowDirection)2,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 20,
			Visible = false
		};
		((Control)selector).AddThemeConstantOverride(StringName.op_Implicit("separation"), 4);
		Button dragHandle = new Button
		{
			Name = StringName.op_Implicit("IndividualCardSkinDragHandle"),
			Text = "⋮",
			CustomMinimumSize = new Vector2(24f, 48f),
			FocusMode = (FocusModeEnum)0,
			MouseFilter = (MouseFilterEnum)0,
			MouseDefaultCursorShape = (CursorShape)13
		};
		ContextualSkinControls.ApplyGameTheme(dragHandle);
		((Control)dragHandle).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 22);
		OptionButton dropdown = new OptionButton
		{
			Name = StringName.op_Implicit("IndividualCardSkinDropdown"),
			CustomMinimumSize = new Vector2(308f, 48f),
			SizeFlagsHorizontal = (SizeFlags)3,
			FitToLongestItem = false,
			ClipText = true,
			Alignment = (HorizontalAlignment)1
		};
		ContextualSkinControls.ApplyGameTheme(dropdown);
		((Control)dropdown).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 20);
		PopupMenu popup = dropdown.GetPopup();
		((Window)popup).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 20);
		((Window)popup).AboutToPopup += delegate
		{
			PopulateOptions(screen, selector, dropdown);
		};
		bool dragging;
		Vector2 dragOffset;
		checked
		{
			popup.IdFocused += (IdFocusedEventHandler)delegate(long id)
			{
				PreviewSelection(screen, dropdown, popup.GetItemIndex((int)id));
			};
			((Popup)popup).PopupHide += delegate
			{
				RestorePreview(screen);
			};
			dropdown.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
			{
				ApplySelection(screen, selector, dropdown, (int)index);
			};
			dragging = false;
			dragOffset = Vector2.Zero;
		}
		((Control)dragHandle).GuiInput += (GuiInputEventHandler)delegate(InputEvent inputEvent)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Invalid comparison between Unknown and I8
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Invalid comparison between Unknown and I8
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			if (val != null)
			{
				if ((long)val.ButtonIndex == 2 && val.Pressed)
				{
					dragging = false;
					SkinService.ResetIndividualCardSkinSelectorPosition();
					ApplyDefaultSelectorPosition(selector);
					((Control)dragHandle).AcceptEvent();
				}
				else if ((long)val.ButtonIndex == 1)
				{
					if (val.Pressed)
					{
						dragging = true;
						dragOffset = ((CanvasItem)screen).GetLocalMousePosition() - (((Control)selector).Position + ((Control)selector).Size / 2f);
						((Control)dragHandle).GrabClickFocus();
					}
					else if (dragging)
					{
						dragging = false;
						SaveSelectorPosition(screen, selector);
					}
					((Control)dragHandle).AcceptEvent();
				}
			}
			else if (inputEvent is InputEventMouseMotion && dragging)
			{
				MoveSelector(screen, selector, ((CanvasItem)screen).GetLocalMousePosition() - dragOffset);
				((Control)dragHandle).AcceptEvent();
			}
		};
		((Node)selector).AddChild((Node)(object)dragHandle, false, (InternalMode)0);
		((Node)selector).AddChild((Node)(object)dropdown, false, (InternalMode)0);
		((Node)screen).AddChild((Node)(object)selector, false, (InternalMode)0);
		DragHandleHoverVisibility.Attach((Control)(object)selector, dragHandle, () => dragging);
		ApplyStoredSelectorPosition(screen, selector);
		ModLocalization.Bind((Node)(object)screen, delegate
		{
			((Control)dragHandle).TooltipText = ModLocalization.Get(ModText.CardSkinSelectorDragHint);
			Sync(screen);
		});
		((Control)dragHandle).TooltipText = ModLocalization.Get(ModText.CardSkinSelectorDragHint);
	}

	private static void ApplyStoredSelectorPosition(NInspectCardScreen screen, HBoxContainer selector)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (!SkinService.TryGetIndividualCardSkinSelectorPosition(out var x, out var y))
		{
			ApplyDefaultSelectorPosition(selector);
			return;
		}
		Vector2 size = ((Control)screen).Size;
		NormalizedControlPosition position = CardSkinSelectorPlacementPolicy.ClampNormalized(x, y, size.X, size.Y);
		ApplyNormalizedSelectorPosition(selector, position);
	}

	private static void ApplyDefaultSelectorPosition(HBoxContainer selector)
	{
		((Control)selector).AnchorLeft = 0.5f;
		((Control)selector).AnchorTop = 0.5f;
		((Control)selector).AnchorRight = 0.5f;
		((Control)selector).AnchorBottom = 0.5f;
		((Control)selector).OffsetLeft = -168f;
		((Control)selector).OffsetTop = -526f;
		((Control)selector).OffsetRight = 168f;
		((Control)selector).OffsetBottom = -478f;
	}

	private static void MoveSelector(NInspectCardScreen screen, HBoxContainer selector, Vector2 requestedCenter)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 size = ((Control)screen).Size;
		NormalizedControlPosition position = CardSkinSelectorPlacementPolicy.ClampNormalized(requestedCenter.X / Math.Max(1f, size.X), requestedCenter.Y / Math.Max(1f, size.Y), size.X, size.Y);
		ApplyNormalizedSelectorPosition(selector, position);
	}

	private static void ApplyNormalizedSelectorPosition(HBoxContainer selector, NormalizedControlPosition position)
	{
		((Control)selector).AnchorLeft = position.X;
		((Control)selector).AnchorTop = position.Y;
		((Control)selector).AnchorRight = position.X;
		((Control)selector).AnchorBottom = position.Y;
		((Control)selector).OffsetLeft = -168f;
		((Control)selector).OffsetTop = -24f;
		((Control)selector).OffsetRight = 168f;
		((Control)selector).OffsetBottom = 24f;
	}

	private static void SaveSelectorPosition(NInspectCardScreen screen, HBoxContainer selector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 size = ((Control)screen).Size;
		if (!(size.X <= 0f) && !(size.Y <= 0f))
		{
			Vector2 val = ((Control)selector).Position + ((Control)selector).Size / 2f;
			NormalizedControlPosition position = CardSkinSelectorPlacementPolicy.ClampNormalized(val.X / size.X, val.Y / size.Y, size.X, size.Y);
			ApplyNormalizedSelectorPosition(selector, position);
			SkinService.SetIndividualCardSkinSelectorPosition(position.X, position.Y);
		}
	}

	public static void Sync(NInspectCardScreen screen)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		HBoxContainer nodeOrNull = ((Node)screen).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("STS2IndividualCardSkinSelector"));
		NCard nodeOrNull2 = ((Node)screen).GetNodeOrNull<NCard>(NodePath.op_Implicit("Card"));
		CardModel val = ((nodeOrNull2 != null) ? nodeOrNull2.Model : null);
		if (nodeOrNull != null && val != null)
		{
			OptionButton node = ((Node)nodeOrNull).GetNode<OptionButton>(NodePath.op_Implicit("IndividualCardSkinDropdown"));
			((GodotObject)nodeOrNull).SetMeta(StringName.op_Implicit("sts2_individual_card_skin_updating"), Variant.op_Implicit(true));
			((GodotObject)nodeOrNull).RemoveMeta(StringName.op_Implicit("sts2_individual_card_skin_options_card"));
			node.Clear();
			((Control)node).TooltipText = string.Empty;
			string cardOverrideSelection = SkinService.GetCardOverrideSelection(val);
			node.AddItem(SelectionDisplayName(cardOverrideSelection, val), -1);
			node.SetItemMetadata(0, Variant.op_Implicit(cardOverrideSelection));
			node.Select(0);
			SkinWorkshopEntry.RefreshSelectionColor(node);
			((GodotObject)nodeOrNull).SetMeta(StringName.op_Implicit("sts2_individual_card_skin_updating"), Variant.op_Implicit(false));
			((CanvasItem)nodeOrNull).Visible = true;
		}
	}

	private static void PopulateOptions(NInspectCardScreen screen, HBoxContainer selector, OptionButton dropdown)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		NCard nodeOrNull = ((Node)screen).GetNodeOrNull<NCard>(NodePath.op_Implicit("Card"));
		CardModel val = ((nodeOrNull != null) ? nodeOrNull.Model : null);
		if (val == null)
		{
			return;
		}
		string text = ((object)((AbstractModel)val).Id).ToString();
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_individual_card_skin_options_card"), Variant.op_Implicit(string.Empty));
		if (((Variant)(ref meta)).AsString().Equals(text, StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		SkinService.InitializeCardGroupsAfterModels();
		(IReadOnlyList<CardSkinOption> Options, IReadOnlyList<SkinPriorityState> States) cardOptionsForDisplay = SkinService.GetCardOptionsForDisplay(val);
		IReadOnlyList<CardSkinOption> item = cardOptionsForDisplay.Options;
		IReadOnlyList<SkinPriorityState> item2 = cardOptionsForDisplay.States;
		string selected = SkinService.GetCardOverrideSelection(val);
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_individual_card_skin_updating"), Variant.op_Implicit(true));
		dropdown.Clear();
		SkinChoiceDimming.Bind(dropdown, item2);
		dropdown.AddItem(CardSkinControls.InheritedSelectionText(val), -1);
		dropdown.SetItemMetadata(0, Variant.op_Implicit("__inherit__"));
		dropdown.AddItem(ModLocalization.Get(ModText.GameOriginal), -1);
		dropdown.SetItemMetadata(1, Variant.op_Implicit("__base__"));
		foreach (CardSkinOption item3 in item)
		{
			int itemCount = dropdown.ItemCount;
			dropdown.AddItem(ModLocalization.DisplayOptionName(item3.Name), -1);
			dropdown.SetItemMetadata(itemCount, Variant.op_Implicit(item3.Id));
		}
		int num = Enumerable.Range(0, dropdown.ItemCount).FirstOrDefault(delegate(int index)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = dropdown.GetItemMetadata(index);
			return ((Variant)(ref itemMetadata)).AsString().Equals(selected, StringComparison.OrdinalIgnoreCase);
		});
		dropdown.Select(num);
		SkinWorkshopEntry.Append(dropdown);
		PresetChoiceColoring.Attach(dropdown, SkinOptionStylePolicy.IsAccented, colorSelection: false);
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_individual_card_skin_options_card"), Variant.op_Implicit(text));
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_individual_card_skin_updating"), Variant.op_Implicit(false));
	}

	private static string SelectionDisplayName(string optionId, CardModel card)
	{
		if (optionId.Equals("__inherit__", StringComparison.OrdinalIgnoreCase))
		{
			return CardSkinControls.InheritedSelectionText(card);
		}
		if (optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return ModLocalization.Get(ModText.GameOriginal);
		}
		string cardOptionName = SkinService.GetCardOptionName(optionId);
		if (!string.IsNullOrWhiteSpace(cardOptionName))
		{
			return ModLocalization.DisplayOptionName(cardOptionName);
		}
		return optionId;
	}

	private static void ApplySelection(NInspectCardScreen screen, HBoxContainer selector, OptionButton dropdown, int index)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_individual_card_skin_updating"), Variant.op_Implicit(false));
		if (((Variant)(ref val)).AsBool())
		{
			return;
		}
		NCard nodeOrNull = ((Node)screen).GetNodeOrNull<NCard>(NodePath.op_Implicit("Card"));
		CardModel val2 = ((nodeOrNull != null) ? nodeOrNull.Model : null);
		if (val2 == null || index < 0 || index >= dropdown.ItemCount)
		{
			return;
		}
		val = dropdown.GetItemMetadata(index);
		string optionId = ((Variant)(ref val)).AsString();
		if (SkinWorkshopEntry.Open(optionId, (Node)(object)screen, SkinService.GetCardInheritedGroupId(val2) ?? "", delegate
		{
			Sync(screen);
		}, cards: true))
		{
			return;
		}
		if (!SkinService.ApplyCardSelection(val2, optionId))
		{
			ModLog.Error("单卡皮肤界面切换失败：" + SkinService.LastError);
			Sync(screen);
			return;
		}
		string cardId = ((object)((AbstractModel)val2).Id).ToString();
		Callable val3 = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)screen))
			{
				SceneTree tree = ((Node)screen).GetTree();
				RefreshMatchingCards((Node?)(object)((tree != null) ? tree.Root : null), cardId);
				Sync(screen);
			}
		});
		((Callable)(ref val3)).CallDeferred(Array.Empty<Variant>());
	}

	private static void PreviewSelection(NInspectCardScreen screen, OptionButton dropdown, int index)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		if (index < 0 || index >= dropdown.ItemCount)
		{
			RestorePreview(screen);
			return;
		}
		NCard cardNode = ((Node)screen).GetNodeOrNull<NCard>(NodePath.op_Implicit("Card"));
		NCard obj = cardNode;
		CardModel val = ((obj != null) ? obj.Model : null);
		if (cardNode == null || val == null)
		{
			return;
		}
		string text = ((object)((AbstractModel)val).Id).ToString();
		Variant val2 = dropdown.GetItemMetadata(index);
		string text2 = ((Variant)(ref val2)).AsString();
		if (!WorkshopCatalogPolicy.IsSkinChoice(text2))
		{
			RestorePreview(screen);
			return;
		}
		val2 = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_card"), Variant.op_Implicit(string.Empty));
		if (((Variant)(ref val2)).AsString().Equals(text, StringComparison.OrdinalIgnoreCase))
		{
			val2 = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_option"), Variant.op_Implicit(string.Empty));
			if (((Variant)(ref val2)).AsString().Equals(text2, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
		}
		((GodotObject)screen).SetMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_card"), Variant.op_Implicit(text));
		((GodotObject)screen).SetMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_option"), Variant.op_Implicit(text2));
		try
		{
			SkinService.WithCardPreviewSelection(val, text2, delegate
			{
				CardSkinControls.RefreshCardSkin(cardNode);
			});
		}
		catch (Exception ex)
		{
			((GodotObject)screen).RemoveMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_card"));
			((GodotObject)screen).RemoveMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_option"));
			ModLog.Error("预览单卡皮肤失败：" + ex);
		}
	}

	private static void RestorePreview(NInspectCardScreen screen)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)screen).HasMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_option")))
		{
			return;
		}
		Variant meta = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_card"), Variant.op_Implicit(string.Empty));
		string value = ((Variant)(ref meta)).AsString();
		((GodotObject)screen).RemoveMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_card"));
		((GodotObject)screen).RemoveMeta(StringName.op_Implicit("sts2_individual_card_skin_preview_option"));
		NCard nodeOrNull = ((Node)screen).GetNodeOrNull<NCard>(NodePath.op_Implicit("Card"));
		if (nodeOrNull == null)
		{
			return;
		}
		CardModel model = nodeOrNull.Model;
		if (((model != null) ? new bool?(((object)((AbstractModel)model).Id).ToString().Equals(value, StringComparison.OrdinalIgnoreCase)) : ((bool?)null)) != true)
		{
			return;
		}
		try
		{
			CardSkinControls.RefreshCardSkin(nodeOrNull);
		}
		catch (Exception ex)
		{
			ModLog.Error("恢复单卡皮肤预览失败：" + ex);
		}
	}

	private static void RefreshMatchingCards(Node? root, string cardId)
	{
		CardSurfaceView.RefreshTinyCards();
		if (root == null)
		{
			return;
		}
		try
		{
			foreach (NCard item in Descendants(root).OfType<NCard>())
			{
				CardModel model = item.Model;
				if (model != null && ((object)((AbstractModel)model).Id).ToString().Equals(cardId, StringComparison.OrdinalIgnoreCase))
				{
					CardSkinControls.RefreshCardSkin(item);
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Error("刷新单卡皮肤失败：" + ex);
		}
	}

	[IteratorStateMachine(typeof(_003CDescendants_003Ed__20))]
	private static IEnumerable<Node> Descendants(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendants_003Ed__20(-2)
		{
			_003C_003E3__root = root
		};
	}
}
