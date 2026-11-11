using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CharacterSkinBundleControls
{
	private sealed class EditorState(NCharacterSelectScreen screen, HBoxContainer entry, Control overlay, VBoxContainer content)
	{
		public string GroupId = string.Empty;

		public string DisplayName = string.Empty;

		public Action? Refresh;

		public CharacterSkinBundle Draft = new CharacterSkinBundle();

		public string? EditingName;

		public string Status = string.Empty;

		public bool Dirty;

		public bool PendingDelete;

		public Button DeleteButton;

		public Label StatusLabel;

		public IReadOnlyList<SkinPresetCategory> CardCategories = Array.Empty<SkinPresetCategory>();

		public IReadOnlyList<SkinPresetCategory> MonsterCategories = Array.Empty<SkinPresetCategory>();

		public List<Action> RefreshPresetNames = new List<Action>();

		public NCharacterSelectScreen Screen { get; } = screen;

		public HBoxContainer Entry { get; } = entry;

		public Control Overlay { get; } = overlay;

		public VBoxContainer Content { get; } = content;
	}

	private static readonly ConditionalWeakTable<NCharacterSelectScreen, EditorState> States = new ConditionalWeakTable<NCharacterSelectScreen, EditorState>();

	private static void AddContentSection(EditorState state, VBoxContainer fields, bool monsters)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		IReadOnlyList<SkinPresetCategory> readOnlyList = (monsters ? state.MonsterCategories : state.CardCategories);
		if (readOnlyList.Count == 0)
		{
			return;
		}
		((Node)fields).AddChild((Node)new Control
		{
			CustomMinimumSize = new Vector2(0f, 8f),
			MouseFilter = (MouseFilterEnum)2
		}, false, (InternalMode)0);
		HBoxContainer val = new HBoxContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)fields).AddChild((Node)(object)val, false, (InternalMode)0);
		Label val2 = CreateLabel(monsters ? ModLocalization.BundleMonsters : ModLocalization.BundleCards, 21);
		ModThemeRuntime.AccentText((Control)(object)val2);
		((Control)val2).CustomMinimumSize = new Vector2(250f, 42f);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		OptionButton mode = CreateOptions();
		((Node)mode).Name = StringName.op_Implicit(monsters ? "BundleMonsterMode" : "BundleCardMode");
		mode.AddItem(ModLocalization.BundleMultiplePresets, 0);
		mode.AddItem(ModLocalization.BundleModPriority, 1);
		BundleContentMode bundleContentMode = (monsters ? state.Draft.MonsterMode : state.Draft.CardMode);
		mode.Select(mode.GetItemIndex((int)bundleContentMode));
		mode.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			BundleContentMode itemId = (BundleContentMode)mode.GetItemId((int)index);
			if (monsters)
			{
				state.Draft.MonsterMode = itemId;
			}
			else
			{
				state.Draft.CardMode = itemId;
			}
			MarkDirty(state);
			BuildEditor(state);
		};
		((Node)val).AddChild((Node)(object)mode, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)SkinWorkshopEntry.CreatePriorityButton((Node)(object)state.Screen, monsters ? "monster" : "cards", "", delegate
		{
			BuildEditor(state);
		}), false, (InternalMode)0);
		if (bundleContentMode == BundleContentMode.ModPriority)
		{
			AddModPriorityRows(state, fields, monsters);
		}
		else
		{
			AddPresetRows(state, fields, readOnlyList, monsters ? state.Draft.MonsterPresetNames : state.Draft.CardPresetNames);
		}
	}

	private static void AddModPriorityRows(EditorState state, VBoxContainer fields, bool monsters)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Expected O, but got Unknown
		Dictionary<string, BundleModSource> sources = SkinService.GetBundleModSources(monsters).ToDictionary<BundleModSource, string>((BundleModSource source) => source.OptionId, StringComparer.OrdinalIgnoreCase);
		List<BundleModPriorityEntry> entries = SkinService.GetBundleModPriority(state.Draft, monsters);
		if (monsters)
		{
			state.Draft.MonsterModPriority = entries;
		}
		else
		{
			state.Draft.CardModPriority = entries;
		}
		BundleModPriorityEntry[] visible = entries.Where((BundleModPriorityEntry entry) => sources.ContainsKey(entry.OptionId)).ToArray();
		for (int i = 0; i < visible.Length; i++)
		{
			int visibleIndex = i;
			string id = visible[i].OptionId;
			HBoxContainer val = new HBoxContainer
			{
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			((Node)fields).AddChild((Node)(object)val, false, (InternalMode)0);
			CheckBox val2 = new CheckBox
			{
				Text = ModLocalization.Get(ModText.EnabledForCategory),
				ButtonPressed = visible[i].Enabled,
				CustomMinimumSize = new Vector2(88f, 38f)
			};
			ContextualSkinControls.ApplyGameTheme((Button)(object)val2);
			((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 17);
			((BaseButton)val2).Toggled += (ToggledEventHandler)delegate(bool value)
			{
				int index = entries.FindIndex((BundleModPriorityEntry entry) => entry.OptionId.Equals(id, StringComparison.OrdinalIgnoreCase));
				entries[index] = entries[index]with
				{
					Enabled = value
				};
				MarkDirty(state);
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			Label val3 = CreateLabel(ModLocalization.DisplayOptionName(sources[id].Name), 19);
			val3.ClipText = true;
			((Control)val3).SizeFlagsHorizontal = (SizeFlags)3;
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			int[] array = new int[2] { -1, 1 };
			foreach (int direction2 in array)
			{
				Button val4 = new Button
				{
					Text = ((direction2 < 0) ? "↑" : "↓"),
					CustomMinimumSize = new Vector2(42f, 38f),
					Disabled = ((direction2 < 0) ? (visibleIndex == 0) : (visibleIndex == visible.Length - 1)),
					FocusMode = (FocusModeEnum)0
				};
				ContextualSkinControls.ApplyGameTheme(val4);
				((Control)val4).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
				((BaseButton)val4).Pressed += delegate
				{
					Move(direction2);
				};
				((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
			}
			void Move(int direction)
			{
				int num = entries.FindIndex((BundleModPriorityEntry entry) => entry.OptionId.Equals(id, StringComparison.OrdinalIgnoreCase));
				int num2 = entries.FindIndex((BundleModPriorityEntry entry) => entry.OptionId.Equals(visible[visibleIndex + direction].OptionId, StringComparison.OrdinalIgnoreCase));
				List<BundleModPriorityEntry> list = entries;
				int index2 = num;
				List<BundleModPriorityEntry> list2 = entries;
				int index3 = num2;
				BundleModPriorityEntry value2 = entries[num2];
				BundleModPriorityEntry value3 = entries[num];
				list[index2] = value2;
				list2[index3] = value3;
				MarkDirty(state);
				BuildEditor(state);
			}
		}
	}

	internal static void ShowForCharacter(NCharacterSelectScreen screen, string groupId, string displayName, Action refresh)
	{
		EditorState value = States.GetValue(screen, CreateState);
		if (!value.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
		{
			((CanvasItem)value.Overlay).Hide();
		}
		value.GroupId = groupId;
		value.DisplayName = displayName;
		value.Refresh = refresh;
		((CanvasItem)value.Entry).Show();
	}

	internal static void Hide(NCharacterSelectScreen screen)
	{
		if (States.TryGetValue(screen, out EditorState value))
		{
			((CanvasItem)value.Entry).Hide();
			((CanvasItem)value.Overlay).Hide();
		}
	}

	private static EditorState CreateState(NCharacterSelectScreen screen)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Expected O, but got Unknown
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Expected O, but got Unknown
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Expected O, but got Unknown
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Expected O, but got Unknown
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Expected O, but got Unknown
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Expected O, but got Unknown
		HBoxContainer entry = new HBoxContainer
		{
			Name = StringName.op_Implicit("STS2CharacterSkinBundleControl"),
			ZIndex = 100,
			MouseFilter = (MouseFilterEnum)0,
			Visible = false
		};
		Button button = CreateButton(ModText.CharacterSkinBundle);
		((Node)button).Name = StringName.op_Implicit("STS2CharacterSkinBundleButton");
		((Control)button).CustomMinimumSize = new Vector2(180f, 46f);
		((Node)entry).AddChild((Node)(object)button, false, (InternalMode)0);
		((Node)screen).AddChild((Node)(object)entry, false, (InternalMode)0);
		DraggableSkinControl.Attach((Control)(object)screen, entry, SkinService.GetCharacterSkinBundlePosition, SkinService.SetCharacterSkinBundlePosition, SkinService.ResetCharacterSkinBundlePosition, delegate
		{
			DraggableSkinControl.ApplyDefaultPosition((Control)(object)screen, entry, DraggableControlPlacementPolicy.CharacterBundleDefault);
		});
		Control overlay = new Control
		{
			Name = StringName.op_Implicit("STS2CharacterSkinBundleOverlay"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 2400
		};
		((Node)screen).AddChild((Node)(object)overlay, false, (InternalMode)0);
		overlay.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		ColorRect mask = new ColorRect
		{
			Color = new Color(0f, 0f, 0f, 0.72f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)overlay).AddChild((Node)(object)mask, false, (InternalMode)0);
		((Control)mask).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		PanelContainer val = new PanelContainer
		{
			Name = StringName.op_Implicit("BundlePanel"),
			MouseFilter = (MouseFilterEnum)0,
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			AnchorTop = 0.1f,
			AnchorBottom = 0.9f,
			OffsetLeft = -430f,
			OffsetRight = 430f,
			OffsetTop = 0f,
			OffsetBottom = 0f,
			ClipContents = true
		};
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("241a30"), new Color("79547e"), 2));
		ModThemeRuntime.Panel(val);
		((Node)overlay).AddChild((Node)(object)val, false, (InternalMode)0);
		MarginContainer val2 = new MarginContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 22);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 22);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 18);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 18);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		VBoxContainer val3 = new VBoxContainer
		{
			Name = StringName.op_Implicit("BundleContent"),
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		EditorState state = new EditorState(screen, entry, overlay, val3);
		((BaseButton)button).Pressed += delegate
		{
			Open(state);
		};
		((Control)mask).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I8
			InputEventMouseButton val4 = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val4 != null && val4.Pressed && (long)val4.ButtonIndex == 1)
			{
				((CanvasItem)overlay).Hide();
				((Control)mask).AcceptEvent();
			}
		};
		ModLocalization.Bind((Node)(object)button, delegate
		{
			button.Text = ModLocalization.Get(ModText.CharacterSkinBundle);
			if (((CanvasItem)overlay).Visible)
			{
				BuildEditor(state);
			}
		});
		return state;
	}

	private static void Open(EditorState state)
	{
		SkinService.InitializeCardGroupsAfterModels();
		state.CardCategories = SkinService.GetCardPresetCategories();
		state.MonsterCategories = SkinService.GetMonsterPresetCategories();
		string active = SkinService.Config.ActiveCharacterSkinBundles.GetValueOrDefault(state.GroupId);
		IReadOnlyList<CharacterSkinBundle> characterSkinBundles = SkinService.GetCharacterSkinBundles(state.GroupId);
		LoadDraft(state, characterSkinBundles.FirstOrDefault((CharacterSkinBundle bundle) => bundle.Name.Equals(active, StringComparison.OrdinalIgnoreCase)) ?? characterSkinBundles.FirstOrDefault());
		BuildEditor(state);
		((CanvasItem)state.Overlay).Show();
		((CanvasItem)state.Overlay).MoveToFront();
	}

	private static void LoadDraft(EditorState state, CharacterSkinBundle? bundle)
	{
		state.EditingName = bundle?.Name;
		state.Draft = ((bundle == null) ? new CharacterSkinBundle
		{
			CharacterGroupId = state.GroupId,
			CharacterOptionId = SkinService.Config.GetSelection(state.GroupId)
		} : CharacterSkinBundlePolicy.Clone(bundle));
		string optionId = SkinService.Catalog?.ResolveStoredVisualSelectionId(state.GroupId, state.Draft.CharacterOptionId) ?? state.Draft.CharacterOptionId;
		state.Draft.CharacterOptionId = SkinService.GetCharacterSkinBundleSourceOptions(state.GroupId).FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))?.Id ?? "__base__";
		if (bundle == null)
		{
			BundlePresetPolicy.InitializeDraft(state.Draft, state.CardCategories.Select((SkinPresetCategory category) => category.Id), state.MonsterCategories.Select((SkinPresetCategory category) => category.Id));
		}
		state.Dirty = bundle == null;
		state.PendingDelete = false;
		state.Status = string.Empty;
	}

	private static void BuildEditor(EditorState state)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected O, but got Unknown
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Expected O, but got Unknown
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Expected O, but got Unknown
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Expected O, but got Unknown
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Expected O, but got Unknown
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Expected O, but got Unknown
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0536: Expected O, but got Unknown
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Expected O, but got Unknown
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Unknown result type (might be due to invalid IL or missing references)
		//IL_068d: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Expected O, but got Unknown
		//IL_06b2: Unknown result type (might be due to invalid IL or missing references)
		state.CardCategories = SkinService.GetCardPresetCategories();
		state.MonsterCategories = SkinService.GetMonsterPresetCategories();
		state.RefreshPresetNames.Clear();
		ScrollContainer val = ScrollListRebuild.Begin(state.Content, state.GroupId + ":" + state.Draft.Id);
		Label val2 = CreateLabel(state.DisplayName + " · " + ModLocalization.Get(ModText.CharacterSkinBundle), 27);
		val2.HorizontalAlignment = (HorizontalAlignment)1;
		ModThemeRuntime.AccentText((Control)(object)val2);
		((Node)state.Content).AddChild((Node)(object)val2, false, (InternalMode)0);
		HBoxContainer val3 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)state.Content).AddChild((Node)(object)val3, false, (InternalMode)0);
		OptionButton val4 = CreateOptions();
		((Node)val4).Name = StringName.op_Implicit("BundleProfiles");
		val4.AddItem(ModLocalization.Get(ModText.NewBundle), -1);
		IReadOnlyList<CharacterSkinBundle> bundles = SkinService.GetCharacterSkinBundles(state.GroupId);
		for (int i = 0; i < bundles.Count; i++)
		{
			val4.AddItem(bundles[i].Name, -1);
			if (bundles[i].Name.Equals(state.EditingName, StringComparison.OrdinalIgnoreCase))
			{
				val4.Select(i + 1);
			}
		}
		val4.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			LoadDraft(state, (index == 0L) ? null : bundles[(int)index - 1]);
			BuildEditor(state);
		};
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		Button val5 = CreateButton(ModText.SaveCharacterSkinMerge);
		((Node)val5).Name = StringName.op_Implicit("SaveBundle");
		((Control)val5).CustomMinimumSize = new Vector2(138f, 42f);
		((BaseButton)val5).Pressed += delegate
		{
			Save(state);
		};
		((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
		LineEdit val6 = new LineEdit
		{
			Name = StringName.op_Implicit("BundleName"),
			Text = state.Draft.Name,
			MaxLength = 40,
			PlaceholderText = ModLocalization.Get(ModText.BundleName),
			Editable = true,
			CustomMinimumSize = new Vector2(0f, 42f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		CharacterSkinCompositionControls.ApplyLineEditTheme(val6);
		((Control)val6).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 20);
		val6.TextChanged += (TextChangedEventHandler)delegate(string value)
		{
			state.Draft.Name = value;
			MarkDirty(state);
			foreach (Action refreshPresetName in state.RefreshPresetNames)
			{
				refreshPresetName();
			}
		};
		((Node)state.Content).AddChild((Node)(object)val6, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)state.Content).AddChild((Node)(object)val7, false, (InternalMode)0);
		Label val8 = CreateLabel(ModLocalization.Get(ModText.BundleCharacterSkin), 19);
		((Control)val8).CustomMinimumSize = new Vector2(250f, 42f);
		((Node)val7).AddChild((Node)(object)val8, false, (InternalMode)0);
		OptionButton val9 = CreateOptions();
		((Node)val9).Name = StringName.op_Implicit("BundleCharacterSkin");
		List<(string Id, string Name)> options = (from option in SkinService.GetCharacterSkinBundleSourceOptions(state.GroupId)
			select (Id: option.Id, Name: ModLocalization.DisplayOptionName(option.Name))).ToList();
		options.Insert(0, ("__base__", ModLocalization.Get(ModText.GameDefault)));
		int val10 = options.FindIndex(((string Id, string Name) option) => option.Id.Equals(state.Draft.CharacterOptionId, StringComparison.OrdinalIgnoreCase));
		foreach (var item in options)
		{
			int itemCount = val9.ItemCount;
			val9.AddItem(item.Name, -1);
			val9.SetItemMetadata(itemCount, Variant.op_Implicit(item.Id));
		}
		val9.Select(Math.Max(0, val10));
		PresetChoiceColoring.Attach(val9, (string id) => id == "__base__");
		val9.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			state.Draft.CharacterOptionId = options[(int)index].Id;
			MarkDirty(state);
		};
		((Node)val7).AddChild((Node)(object)val9, false, (InternalMode)0);
		((Control)val).CustomMinimumSize = Vector2.Zero;
		((Control)val).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val).SizeFlagsVertical = (SizeFlags)3;
		val.HorizontalScrollMode = (ScrollMode)0;
		val.VerticalScrollMode = (ScrollMode)1;
		ScrollListRebuild.PlaceAfterHeader(val);
		VBoxContainer val11 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)0
		};
		((Control)val11).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val).AddChild((Node)(object)val11, false, (InternalMode)0);
		AddContentSection(state, val11, monsters: false);
		AddContentSection(state, val11, monsters: true);
		CheckBox val12 = new CheckBox
		{
			Text = ModLocalization.BundleHideSource,
			ButtonPressed = state.Draft.HideSources,
			CustomMinimumSize = new Vector2(0f, 38f)
		};
		ContextualSkinControls.ApplyGameTheme((Button)(object)val12);
		((Control)val12).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
		((BaseButton)val12).Toggled += (ToggledEventHandler)delegate(bool value)
		{
			state.Draft.HideSources = value;
			MarkDirty(state);
		};
		((Node)state.Content).AddChild((Node)(object)val12, false, (InternalMode)0);
		Label val13 = CreateLabel(state.Status, 17);
		((Node)val13).Name = StringName.op_Implicit("BundleStatus");
		val13.AutowrapMode = (AutowrapMode)3;
		val13.HorizontalAlignment = (HorizontalAlignment)1;
		((Control)val13).CustomMinimumSize = new Vector2(0f, 25f);
		ModThemeRuntime.AccentText((Control)(object)val13);
		((Node)state.Content).AddChild((Node)(object)val13, false, (InternalMode)0);
		state.StatusLabel = val13;
		HBoxContainer val14 = new HBoxContainer();
		((Control)val14).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)state.Content).AddChild((Node)(object)val14, false, (InternalMode)0);
		Button val15 = CreateButton(state.PendingDelete ? ModText.ConfirmDeleteCharacterSkinMerge : ModText.DeleteCharacterSkinMerge);
		((Node)val15).Name = StringName.op_Implicit("DeleteBundle");
		((CanvasItem)val15).Visible = state.EditingName != null;
		((Control)val15).CustomMinimumSize = new Vector2(190f, 42f);
		if (state.PendingDelete)
		{
			ApplyDeleteWarning(val15);
		}
		((BaseButton)val15).Pressed += delegate
		{
			Delete(state);
		};
		((Node)val14).AddChild((Node)(object)val15, false, (InternalMode)0);
		state.DeleteButton = val15;
		((Node)val14).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		Button val16 = CreateButton(ModText.Close);
		((Control)val16).CustomMinimumSize = new Vector2(190f, 42f);
		((BaseButton)val16).Pressed += delegate
		{
			((CanvasItem)state.Overlay).Hide();
		};
		((Node)val14).AddChild((Node)(object)val16, false, (InternalMode)0);
	}

	private static void AddPresetRows(EditorState state, VBoxContainer fields, IReadOnlyList<SkinPresetCategory> categories, Dictionary<string, string> references)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Expected O, but got Unknown
		foreach (SkinPresetCategory category in categories)
		{
			HBoxContainer val = new HBoxContainer();
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val2 = CreateLabel(category.DisplayName, 19);
			((Control)val2).CustomMinimumSize = new Vector2(250f, 40f);
			val2.ClipText = true;
			((Control)val2).TooltipText = category.DisplayName;
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			OptionButton picker = CreateOptions();
			((Node)picker).Name = StringName.op_Implicit("BundlePreset");
			((Control)picker).SizeFlagsHorizontal = (SizeFlags)3;
			string ownKey = BundlePresetPolicy.PresetKey(state.Draft);
			List<string> names = new string[1] { ownKey }.Concat(category.PresetNames.Where(BundlePresetPolicy.IsOwned)).Concat(category.PresetNames.Where((string key) => !BundlePresetPolicy.IsOwned(key))).Distinct<string>(StringComparer.OrdinalIgnoreCase)
				.ToList();
			string current = BundlePresetPolicy.ResolveReference(state.Draft, references.GetValueOrDefault(category.Id), names);
			references[category.Id] = current;
			int num = names.FindIndex((string value) => value.Equals(current, StringComparison.OrdinalIgnoreCase));
			for (int i = 0; i < names.Count; i++)
			{
				picker.AddItem(Display(names[i]), -1);
				picker.SetItemMetadata(i, Variant.op_Implicit(names[i]));
			}
			picker.Select(num);
			PresetChoiceColoring.Attach(picker);
			state.RefreshPresetNames.Add(delegate
			{
				picker.SetItemText(0, Display(ownKey));
			});
			picker.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
			{
				references[category.Id] = names[(int)index];
				MarkDirty(state);
			};
			((Node)val).AddChild((Node)(object)picker, false, (InternalMode)0);
			((Node)fields).AddChild((Node)(object)val, false, (InternalMode)0);
			string Display(string key)
			{
				if (!(key == ownKey))
				{
					return SkinService.GetPresetDisplayName(key);
				}
				return SkinService.GetBundlePresetDisplayName(state.Draft);
			}
		}
	}

	private static void MarkDirty(EditorState state)
	{
		state.Dirty = true;
		state.PendingDelete = false;
		state.StatusLabel.Text = string.Empty;
		state.Status = string.Empty;
		state.DeleteButton.Text = ModLocalization.Get(ModText.DeleteCharacterSkinMerge);
		ContextualSkinControls.ApplyGameTheme(state.DeleteButton);
	}

	private static void Save(EditorState state)
	{
		bool flag = ((state.EditingName == null) ? SkinService.CreateCharacterSkinBundle(state.Draft) : SkinService.OverwriteCharacterSkinBundle(state.EditingName, state.Draft));
		if (flag)
		{
			LoadDraft(state, SkinService.GetCharacterSkinBundles(state.GroupId).First((CharacterSkinBundle bundle) => bundle.Name.Equals(state.Draft.Name.Trim(), StringComparison.OrdinalIgnoreCase)));
		}
		state.Status = (flag ? ModLocalization.Get(ModText.BundleSaved) : (SkinService.LastError ?? string.Empty));
		state.PendingDelete = false;
		BuildEditor(state);
		if (flag)
		{
			state.Refresh?.Invoke();
		}
	}

	private static void Delete(EditorState state)
	{
		if (state.EditingName == null)
		{
			return;
		}
		if (!state.PendingDelete)
		{
			state.PendingDelete = true;
			state.DeleteButton.Text = ModLocalization.Get(ModText.ConfirmDeleteCharacterSkinMerge);
			ApplyDeleteWarning(state.DeleteButton);
			return;
		}
		if (SkinService.DeleteCharacterSkinBundle(state.GroupId, state.EditingName))
		{
			LoadDraft(state, null);
		}
		else
		{
			state.Status = SkinService.LastError ?? string.Empty;
		}
		state.PendingDelete = false;
		BuildEditor(state);
		if (state.EditingName == null)
		{
			state.Refresh?.Invoke();
		}
	}

	private static Label CreateLabel(string text, int size)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Label val = new Label
		{
			Text = text,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), size);
		ModThemeRuntime.TextControl((Control)(object)val, size);
		return val;
	}

	private static Button CreateButton(ModText text)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		Button val = new Button
		{
			Text = ModLocalization.Get(text),
			CustomMinimumSize = new Vector2(100f, 42f),
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2
		};
		ContextualSkinControls.ApplyGameTheme(val);
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
		return val;
	}

	private static OptionButton CreateOptions()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		OptionButton val = new OptionButton
		{
			CustomMinimumSize = new Vector2(0f, 42f),
			FitToLongestItem = false,
			ClipText = true,
			SizeFlagsHorizontal = (SizeFlags)3,
			Alignment = (HorizontalAlignment)1
		};
		ContextualSkinControls.ApplyGameTheme(val);
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
		return val;
	}

	private static void ApplyDeleteWarning(Button button)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[4] { "normal", "hover", "pressed", "focus" };
		foreach (string text in array)
		{
			((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("852f3b"), new Color("f28b83"), 2));
		}
	}
}
