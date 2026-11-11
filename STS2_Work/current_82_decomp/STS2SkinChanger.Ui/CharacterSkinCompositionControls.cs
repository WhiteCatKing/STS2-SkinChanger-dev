using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CharacterSkinCompositionControls
{
	private sealed class EditorState(NCharacterSelectScreen screen, Control entryControl, Button entryButton, Control overlay)
	{
		public NCharacterSelectScreen Screen { get; } = screen;

		public Control EntryControl { get; } = entryControl;

		public Button EntryButton { get; } = entryButton;

		public Control Overlay { get; } = overlay;

		public SkinGroup? Group { get; set; }

		public Action? Refresh { get; set; }

		public string? EditingCompositionId { get; set; }

		public string DraftName { get; set; } = string.Empty;

		public List<string> DraftSources { get; } = new List<string>();

		public bool HideSources { get; set; }

		public string? PendingDeleteCompositionId { get; set; }

		public bool DeleteConfirmationPending
		{
			get
			{
				if (!string.IsNullOrWhiteSpace(EditingCompositionId))
				{
					return string.Equals(PendingDeleteCompositionId, EditingCompositionId, StringComparison.OrdinalIgnoreCase);
				}
				return false;
			}
		}

		public bool Rebuilding { get; set; }

		public string StatusText { get; set; } = string.Empty;

		public bool TryConfirmDelete()
		{
			bool deleteConfirmationPending = DeleteConfirmationPending;
			PendingDeleteCompositionId = (deleteConfirmationPending ? null : EditingCompositionId);
			return deleteConfirmationPending;
		}
	}

	private const string EntryButtonName = "STS2CharacterSkinMergeButton";

	private const string OverlayName = "STS2CharacterSkinMergeOverlay";

	private const string PanelName = "MergePanel";

	private const string MarginName = "MergeMargin";

	private const string ContentName = "MergeContent";

	private static readonly ConditionalWeakTable<NCharacterSelectScreen, EditorState> States = new ConditionalWeakTable<NCharacterSelectScreen, EditorState>();

	public static void Show(NCharacterSelectScreen screen, SkinGroup? group, Action refresh)
	{
		EditorState value = States.GetValue(screen, CreateState);
		bool num = !string.Equals(value.Group?.Id, group?.Id, StringComparison.OrdinalIgnoreCase);
		value.Group = group;
		value.Refresh = refresh;
		((CanvasItem)value.EntryControl).Visible = group != null && SkinService.GetRawCharacterSkinOptions(group.Id).Count > 0;
		if (num && ((CanvasItem)value.Overlay).Visible)
		{
			((CanvasItem)value.Overlay).Visible = false;
			ResetDraft(value);
		}
	}

	public static void Hide(NCharacterSelectScreen screen)
	{
		if (States.TryGetValue(screen, out EditorState value))
		{
			((CanvasItem)value.EntryControl).Visible = false;
			((CanvasItem)value.Overlay).Visible = false;
		}
	}

	private static EditorState CreateState(NCharacterSelectScreen screen)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		HBoxContainer entryControl = new HBoxContainer
		{
			Name = StringName.op_Implicit("STS2CharacterSkinMergeControl"),
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 0f,
			AnchorBottom = 0f,
			OffsetLeft = 38f,
			OffsetTop = 86f,
			OffsetRight = 246f,
			OffsetBottom = 132f,
			MouseFilter = (MouseFilterEnum)0,
			Visible = false,
			ZIndex = 100
		};
		Button entryButton = new Button
		{
			Name = StringName.op_Implicit("STS2CharacterSkinMergeButton"),
			CustomMinimumSize = new Vector2(180f, 46f),
			SizeFlagsHorizontal = (SizeFlags)3,
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2
		};
		ContextualSkinControls.ApplyGameTheme(entryButton);
		((Control)entryButton).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
		((Node)entryControl).AddChild((Node)(object)entryButton, false, (InternalMode)0);
		((Node)screen).AddChild((Node)(object)entryControl, false, (InternalMode)0);
		DraggableSkinControl.Attach((Control)(object)screen, entryControl, mergeButton: true, delegate
		{
			DraggableSkinControl.ApplyDefaultPosition((Control)(object)screen, entryControl, DraggableControlPlacementPolicy.CharacterMergeDefault);
		});
		Control overlay = CreateOverlay();
		((Node)screen).AddChild((Node)(object)overlay, false, (InternalMode)0);
		overlay.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		EditorState state = new EditorState(screen, (Control)(object)entryControl, entryButton, overlay);
		((BaseButton)entryButton).Pressed += delegate
		{
			Open(state);
		};
		((Control)((Node)overlay).GetNode<ColorRect>(NodePath.op_Implicit("Mask"))).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && val.Pressed && (long)val.ButtonIndex == 1)
			{
				state.PendingDeleteCompositionId = null;
				((CanvasItem)overlay).Visible = false;
				((Control)((Node)overlay).GetNode<ColorRect>(NodePath.op_Implicit("Mask"))).AcceptEvent();
			}
		};
		ModLocalization.Bind((Node)(object)entryButton, delegate
		{
			entryButton.Text = ModLocalization.Get(ModText.CharacterSkinMerge);
			if (((CanvasItem)overlay).Visible)
			{
				BuildEditor(state);
			}
		});
		return state;
	}

	private static Control CreateOverlay()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01cd: Expected O, but got Unknown
		Control val = new Control
		{
			Name = StringName.op_Implicit("STS2CharacterSkinMergeOverlay"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 2400
		};
		ColorRect val2 = new ColorRect
		{
			Name = StringName.op_Implicit("Mask"),
			Color = new Color(0f, 0f, 0f, 0.72f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Control)val2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		PanelContainer val3 = new PanelContainer
		{
			Name = StringName.op_Implicit("MergePanel"),
			MouseFilter = (MouseFilterEnum)0,
			AnchorLeft = 0.5f,
			AnchorTop = 0.5f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -430f,
			OffsetTop = -330f,
			OffsetRight = 430f,
			OffsetBottom = 330f
		};
		((Control)val3).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("241a30"), new Color("79547e"), 2));
		ModThemeRuntime.Panel(val3);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		MarginContainer val4 = new MarginContainer
		{
			Name = StringName.op_Implicit("MergeMargin")
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 22);
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 18);
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 22);
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 18);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		VBoxContainer val5 = new VBoxContainer
		{
			Name = StringName.op_Implicit("MergeContent")
		};
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		return val;
	}

	private static void Open(EditorState state)
	{
		if (state.Group != null)
		{
			string selected = SkinService.Config.GetSelection(state.Group.Id);
			CharacterSkinComposition composition2 = SkinService.GetCharacterSkinCompositions(state.Group.Id).FirstOrDefault((CharacterSkinComposition composition) => composition.Id.Equals(selected, StringComparison.OrdinalIgnoreCase));
			LoadDraft(state, composition2);
			BuildEditor(state);
			((CanvasItem)state.Overlay).Visible = true;
			((CanvasItem)state.Overlay).MoveToFront();
		}
	}

	private static void ResetDraft(EditorState state)
	{
		state.EditingCompositionId = null;
		state.DraftName = string.Empty;
		state.DraftSources.Clear();
		state.HideSources = false;
		state.PendingDeleteCompositionId = null;
		state.StatusText = string.Empty;
	}

	private static void LoadDraft(EditorState state, CharacterSkinComposition? composition)
	{
		ResetDraft(state);
		if (composition != null)
		{
			state.EditingCompositionId = composition.Id;
			state.DraftName = composition.Name;
			state.DraftSources.AddRange(composition.SourceOptionIds);
			state.HideSources = composition.HideSources;
		}
	}

	private static void BuildEditor(EditorState state)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Expected O, but got Unknown
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Expected O, but got Unknown
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Expected O, but got Unknown
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected O, but got Unknown
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Expected O, but got Unknown
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Expected O, but got Unknown
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Expected O, but got Unknown
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Expected O, but got Unknown
		SkinGroup group = state.Group;
		if (group == null || !GodotObject.IsInstanceValid((GodotObject)(object)state.Overlay))
		{
			((CanvasItem)state.Overlay).Visible = false;
			return;
		}
		VBoxContainer node = ((Node)state.Overlay).GetNode<VBoxContainer>(NodePath.op_Implicit("MergePanel/MergeMargin/MergeContent"));
		ScrollContainer val = ScrollListRebuild.Begin(node, group.Id);
		state.Rebuilding = true;
		Label val2 = CreateLabel(group.DisplayName + " · " + ModLocalization.Get(ModText.CharacterSkinMerge), 27, (Color?)new Color("efc850"));
		val2.HorizontalAlignment = (HorizontalAlignment)1;
		((Node)node).AddChild((Node)(object)val2, false, (InternalMode)0);
		IReadOnlyList<CharacterSkinComposition> compositions = SkinService.GetCharacterSkinCompositions(group.Id);
		HBoxContainer val3 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(808f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)node).AddChild((Node)(object)val3, false, (InternalMode)0);
		OptionButton profiles = new OptionButton
		{
			CustomMinimumSize = new Vector2(650f, 42f),
			SizeFlagsHorizontal = (SizeFlags)3,
			FitToLongestItem = false,
			ClipText = true,
			Alignment = (HorizontalAlignment)1
		};
		ContextualSkinControls.ApplyGameTheme(profiles);
		((Control)profiles).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
		profiles.AddItem(ModLocalization.Get(ModText.NewCharacterSkinMerge), -1);
		profiles.SetItemMetadata(0, Variant.op_Implicit(string.Empty));
		foreach (CharacterSkinComposition item in compositions)
		{
			int itemCount = profiles.ItemCount;
			profiles.AddItem(item.Name, -1);
			profiles.SetItemMetadata(itemCount, Variant.op_Implicit(item.Id));
		}
		int num = Enumerable.Range(0, profiles.ItemCount).FirstOrDefault(delegate(int index)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = profiles.GetItemMetadata(index);
			return ((Variant)(ref itemMetadata)).AsString().Equals(state.EditingCompositionId ?? string.Empty, StringComparison.OrdinalIgnoreCase);
		});
		profiles.Select(num);
		profiles.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (!state.Rebuilding)
			{
				Variant itemMetadata2 = profiles.GetItemMetadata((int)index);
				string id = ((Variant)(ref itemMetadata2)).AsString();
				LoadDraft(state, compositions.FirstOrDefault((CharacterSkinComposition composition) => composition.Id.Equals(id, StringComparison.OrdinalIgnoreCase)));
				BuildEditor(state);
			}
		};
		((Node)val3).AddChild((Node)(object)profiles, false, (InternalMode)0);
		Button val4 = CreateButton(ModLocalization.Get(ModText.SaveCharacterSkinMerge), 138f);
		((BaseButton)val4).Pressed += delegate
		{
			Save(state);
		};
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		LineEdit val5 = new LineEdit
		{
			Text = state.DraftName,
			PlaceholderText = ModLocalization.Get(ModText.CharacterSkinMergeName),
			MaxLength = 40,
			CustomMinimumSize = new Vector2(808f, 42f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ApplyLineEditTheme(val5);
		val5.TextChanged += (TextChangedEventHandler)delegate(string text)
		{
			state.DraftName = text;
		};
		((Node)node).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Control)val).CustomMinimumSize = new Vector2(808f, 300f);
		((Control)val).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val).SizeFlagsVertical = (SizeFlags)3;
		ScrollListRebuild.PlaceAfterHeader(val);
		VBoxContainer val6 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		BuildSourceRows(state, val6);
		CheckBox val7 = new CheckBox
		{
			Text = ModLocalization.Get(ModText.HideMergedSkinSources),
			ButtonPressed = state.HideSources,
			CustomMinimumSize = new Vector2(808f, 38f)
		};
		ContextualSkinControls.ApplyGameTheme((Button)(object)val7);
		((Control)val7).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
		((BaseButton)val7).Toggled += (ToggledEventHandler)delegate(bool value)
		{
			state.HideSources = value;
		};
		((Node)node).AddChild((Node)(object)val7, false, (InternalMode)0);
		Label val8 = CreateLabel(state.StatusText, 17, (Color?)new Color("efc850"));
		val8.HorizontalAlignment = (HorizontalAlignment)1;
		((Control)val8).CustomMinimumSize = new Vector2(808f, 25f);
		((Node)node).AddChild((Node)(object)val8, false, (InternalMode)0);
		HBoxContainer val9 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(808f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val9).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)node).AddChild((Node)(object)val9, false, (InternalMode)0);
		Button delete = CreateButton(ModLocalization.Get(ModText.DeleteCharacterSkinMerge), 190f);
		((CanvasItem)delete).Visible = !string.IsNullOrWhiteSpace(state.EditingCompositionId);
		if (state.DeleteConfirmationPending)
		{
			ApplyDeleteConfirmationTheme(delete);
		}
		((BaseButton)delete).Pressed += delegate
		{
			Delete(state, delete);
		};
		((Node)val9).AddChild((Node)(object)delete, false, (InternalMode)0);
		((Node)val9).AddChild((Node)(object)SkinWorkshopEntry.CreatePriorityButton((Node)(object)state.Screen, "character", state.Group.Id, delegate
		{
			BuildEditor(state);
		}), false, (InternalMode)0);
		((Node)val9).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		Button val10 = CreateButton(ModLocalization.Get(ModText.Close), 190f);
		((BaseButton)val10).Pressed += delegate
		{
			state.PendingDeleteCompositionId = null;
			((CanvasItem)state.Overlay).Visible = false;
		};
		((Node)val9).AddChild((Node)(object)val10, false, (InternalMode)0);
		state.Rebuilding = false;
	}

	private static void BuildSourceRows(EditorState state, VBoxContainer rows)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Expected O, but got Unknown
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		IReadOnlyList<SkinOption> rawCharacterSkinOptions = SkinService.GetRawCharacterSkinOptions(state.Group.Id);
		Dictionary<string, SkinOption> dictionary = rawCharacterSkinOptions.ToDictionary<SkinOption, string>((SkinOption option) => option.Id, StringComparer.OrdinalIgnoreCase);
		string[] selectedIds = state.DraftSources.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		string[] array = selectedIds.Concat(from option in rawCharacterSkinOptions
			select option.Id into optionId
			where !selectedIds.Contains<string>(optionId, StringComparer.OrdinalIgnoreCase)
			select optionId).ToArray();
		foreach (string optionId2 in array)
		{
			int num = state.DraftSources.FindIndex((string sourceId) => sourceId.Equals(optionId2, StringComparison.OrdinalIgnoreCase));
			bool flag = num >= 0;
			SkinOption value2;
			bool flag2 = dictionary.TryGetValue(optionId2, out value2);
			string text = (flag2 ? ModLocalization.DisplayOptionName(value2.Name) : (optionId2 + " · " + ModLocalization.Get(ModText.CharacterSkinSourceUnavailable)));
			HBoxContainer val = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(784f, 43f),
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			((Node)rows).AddChild((Node)(object)val, false, (InternalMode)0);
			CheckBox val2 = new CheckBox
			{
				ButtonPressed = flag,
				CustomMinimumSize = new Vector2(42f, 34f),
				TooltipText = text
			};
			ContextualSkinControls.ApplyGameTheme((Button)(object)val2);
			((BaseButton)val2).Toggled += (ToggledEventHandler)delegate(bool value)
			{
				if (!state.Rebuilding)
				{
					if (value)
					{
						if (!state.DraftSources.Contains<string>(optionId2, StringComparer.OrdinalIgnoreCase))
						{
							state.DraftSources.Add(optionId2);
						}
					}
					else
					{
						state.DraftSources.RemoveAll((string sourceId) => sourceId.Equals(optionId2, StringComparison.OrdinalIgnoreCase));
					}
					BuildEditor(state);
				}
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			Label val3 = CreateLabel(flag ? (num + 1).ToString() : string.Empty, 18, null);
			((Control)val3).CustomMinimumSize = new Vector2(34f, 36f);
			val3.HorizontalAlignment = (HorizontalAlignment)1;
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			Label val4 = CreateLabel(text, 18, (Color?)(flag2 ? new Color("fff6e2") : new Color("b9adbd")));
			val4.ClipText = true;
			((Control)val4).TooltipText = text;
			((Control)val4).CustomMinimumSize = new Vector2(520f, 36f);
			((Control)val4).SizeFlagsHorizontal = (SizeFlags)3;
			((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
			Button val5 = CreateButton("↑", 52f);
			((BaseButton)val5).Disabled = !flag || num == 0;
			((BaseButton)val5).Pressed += delegate
			{
				MoveSource(state, optionId2, -1);
			};
			((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
			Button val6 = CreateButton("↓", 52f);
			((BaseButton)val6).Disabled = !flag || num == state.DraftSources.Count - 1;
			((BaseButton)val6).Pressed += delegate
			{
				MoveSource(state, optionId2, 1);
			};
			((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		}
	}

	private static void MoveSource(EditorState state, string optionId, int direction)
	{
		int num = state.DraftSources.FindIndex((string sourceId) => sourceId.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		int num2 = num + direction;
		if (num >= 0 && num2 >= 0 && num2 < state.DraftSources.Count)
		{
			List<string> draftSources = state.DraftSources;
			int index = num;
			List<string> draftSources2 = state.DraftSources;
			int index2 = num2;
			string value = state.DraftSources[num2];
			string value2 = state.DraftSources[num];
			draftSources[index] = value;
			draftSources2[index2] = value2;
			BuildEditor(state);
		}
	}

	private static void Save(EditorState state)
	{
		state.PendingDeleteCompositionId = null;
		if (state.Group == null)
		{
			return;
		}
		if (state.DraftSources.Count == 0)
		{
			state.StatusText = ModLocalization.Get(ModText.CharacterSkinMergeNeedsSource);
			BuildEditor(state);
			return;
		}
		if (!SkinService.SaveCharacterSkinComposition(state.Group.Id, state.EditingCompositionId, state.DraftName, state.DraftSources, state.HideSources, out string savedId))
		{
			state.StatusText = SkinService.LastError ?? string.Empty;
			BuildEditor(state);
			return;
		}
		state.EditingCompositionId = savedId;
		CharacterSkinComposition composition2 = SkinService.GetCharacterSkinCompositions(state.Group.Id).FirstOrDefault((CharacterSkinComposition composition) => composition.Id.Equals(savedId, StringComparison.OrdinalIgnoreCase));
		LoadDraft(state, composition2);
		state.Refresh?.Invoke();
		BuildEditor(state);
	}

	private static void Delete(EditorState state, Button button)
	{
		if (state.Group != null && !string.IsNullOrWhiteSpace(state.EditingCompositionId))
		{
			if (!state.TryConfirmDelete())
			{
				ApplyDeleteConfirmationTheme(button);
			}
			else if (!SkinService.DeleteCharacterSkinComposition(state.Group.Id, state.EditingCompositionId))
			{
				state.StatusText = SkinService.LastError ?? string.Empty;
				BuildEditor(state);
			}
			else
			{
				ResetDraft(state);
				state.Refresh?.Invoke();
				BuildEditor(state);
			}
		}
	}

	private static void ApplyDeleteConfirmationTheme(Button button)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		button.Text = ModLocalization.Get(ModText.ConfirmDeleteCharacterSkinMerge);
		(string, string)[] array = new(string, string)[3]
		{
			("normal", "7a1f2b"),
			("hover", "9a2937"),
			("pressed", "5e1720")
		};
		for (int i = 0; i < array.Length; i++)
		{
			var (text, text2) = array[i];
			((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color(text2), new Color("ff7a86"), 2));
		}
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff4f4"));
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), new Color("fff4f4"));
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), new Color("fff4f4"));
	}

	private static Label CreateLabel(string text, int fontSize, Color? color = null)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		Label val = new Label
		{
			Text = text,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), fontSize);
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), (Color)(((_003F?)color) ?? new Color("fff6e2")));
		Color? val2 = color;
		Color val3 = default(Color);
		((Color)(ref val3))._002Ector("efc850");
		bool accent = val2.HasValue && val2.GetValueOrDefault() == val3;
		int preserveTextColor;
		if (color.HasValue)
		{
			val2 = color;
			((Color)(ref val3))._002Ector("efc850");
			if (!val2.HasValue || val2.GetValueOrDefault() != val3)
			{
				val2 = color;
				((Color)(ref val3))._002Ector("fff6e2");
				preserveTextColor = ((!val2.HasValue || val2.GetValueOrDefault() != val3) ? 1 : 0);
				goto IL_00e1;
			}
		}
		preserveTextColor = 0;
		goto IL_00e1;
		IL_00e1:
		ModThemeRuntime.TextControl((Control)(object)val, fontSize, accent, (byte)preserveTextColor != 0);
		if (ContextualSkinControls.GameFont != null)
		{
			((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), ContextualSkinControls.GameFont);
		}
		return val;
	}

	private static Button CreateButton(string text, float width)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_003e: Expected O, but got Unknown
		Button val = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(width, 38f),
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2
		};
		ContextualSkinControls.ApplyGameTheme(val);
		ModThemeRuntime.TextControl((Control)val, 18);
		return val;
	}

	internal static void ApplyLineEditTheme(LineEdit lineEdit)
	{
		ModThemeRuntime.Input(lineEdit, 19);
	}
}
