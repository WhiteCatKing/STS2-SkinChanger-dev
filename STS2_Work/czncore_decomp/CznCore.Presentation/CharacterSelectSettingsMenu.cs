using System;
using System.Collections.Generic;
using CznCore.Settings;
using Godot;

namespace CznCore.Presentation;

internal sealed class CharacterSelectSettingsMenu : Control
{
	internal const string SharedNodeName = "CznCharacterSelectMenu";

	internal const string OwnerMeta = "czn_character_select_menu_owner";

	internal const string OpenStateMeta = "czn_character_select_menu_open";

	internal const string NavigationOwnerMeta = "czn_character_select_menu_navigation_owner";

	internal const string NativeLeftNeighborMeta = "czn_character_select_menu_native_left_neighbor";

	private const string CharacterListPath = "CharSelectButtons/ButtonContainer";

	private const int Layer = 120;

	private const float HandleSize = 64f;

	private const float DrawerWidth = 480f;

	private const float DrawerHeight = 424f;

	private readonly List<CharacterSelectMenuItemView> _views = new List<CharacterSelectMenuItemView>();

	private readonly List<CharacterSelectExternalPanel> _panels = new List<CharacterSelectExternalPanel>();

	private readonly PanelContainer _drawer = new PanelContainer
	{
		Name = StringName.op_Implicit("Drawer"),
		Visible = false
	};

	private readonly VBoxContainer _sections = new VBoxContainer
	{
		SizeFlagsHorizontal = (SizeFlags)3
	};

	private readonly Button _handle = new Button
	{
		Name = StringName.op_Implicit("Handle"),
		FocusMode = (FocusModeEnum)2
	};

	private readonly Label _handleLabel = CharacterSelectMenuStyle.Label("≡", 28, CharacterSelectMenuStyle.Text);

	private readonly Control _screen;

	private readonly string _ownerId;

	private readonly Action _unbindModal;

	private CharacterSelectDescriptionLayout? _description;

	private Control? _nativeButton;

	private NodePath _nativeLeftNeighbor = new NodePath(string.Empty);

	private bool _open;

	internal bool IsCurrent
	{
		get
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)_screen) && ((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion())
			{
				return ((Node)_screen).GetNodeOrNull(NodePath.op_Implicit("CznCharacterSelectMenu")) == this;
			}
			return false;
		}
	}

	private CharacterSelectSettingsMenu(Control screen, string ownerId)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		_screen = screen;
		_ownerId = ownerId;
		((Node)this).Name = StringName.op_Implicit("CznCharacterSelectMenu");
		((Control)this).MouseFilter = (MouseFilterEnum)2;
		((CanvasItem)this).ZIndex = 120;
		((GodotObject)this).SetMeta(StringName.op_Implicit("czn_character_select_menu_owner"), Variant.op_Implicit(ownerId));
		BuildFrame();
		((Node)screen).AddChild((Node)(object)this, false, (InternalMode)0);
		((Control)this).SetAnchorsPreset((LayoutPreset)4, false);
		((Control)this).OffsetLeft = 0f;
		((Control)this).OffsetRight = 544f;
		((Control)this).OffsetTop = -212f;
		((Control)this).OffsetBottom = 212f;
		_unbindModal = CharacterSelectMenuStyle.BindModalLayer((Control)(object)this, 120);
		CaptureNativeNavigation();
		((Node)this).TreeExiting += Release;
	}

	internal static CharacterSelectSettingsMenu Attach(Control screen, string ownerId, string legacyTitle, IReadOnlyList<CharacterSelectMenuSectionDescriptor> sections)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)screen).GetNodeOrNull(NodePath.op_Implicit("CznCharacterSelectMenu")) is CharacterSelectSettingsMenu characterSelectSettingsMenu)
		{
			if (characterSelectSettingsMenu._ownerId == ownerId)
			{
				characterSelectSettingsMenu.SetContent(sections);
				return characterSelectSettingsMenu;
			}
			Remove(characterSelectSettingsMenu);
		}
		CharacterSelectSettingsMenu characterSelectSettingsMenu2 = new CharacterSelectSettingsMenu(screen, ownerId);
		characterSelectSettingsMenu2.SetContent(sections);
		int open;
		if (((GodotObject)screen).HasMeta(StringName.op_Implicit("czn_character_select_menu_open")))
		{
			StringName obj = StringName.op_Implicit("czn_character_select_menu_open");
			Variant val = default(Variant);
			val = ((GodotObject)screen).GetMeta(obj, val);
			open = (((Variant)(ref val)).AsBool() ? 1 : 0);
		}
		else
		{
			open = 0;
		}
		characterSelectSettingsMenu2.SetOpen((byte)open != 0, focus: false);
		SkinLog.Info("Character-select options menu attached: owner=" + ownerId);
		return characterSelectSettingsMenu2;
	}

	internal static void RemoveOwned(Control screen, string ownerId)
	{
		if (((Node)screen).GetNodeOrNull(NodePath.op_Implicit("CznCharacterSelectMenu")) is CharacterSelectSettingsMenu characterSelectSettingsMenu && characterSelectSettingsMenu._ownerId == ownerId)
		{
			Remove(characterSelectSettingsMenu);
		}
	}

	internal static void ReleaseBeforeSelection(Control screen)
	{
		if (((Node)screen).GetNodeOrNull(NodePath.op_Implicit("CznCharacterSelectMenu")) is CharacterSelectSettingsMenu menu)
		{
			Remove(menu);
		}
	}

	private static void Remove(CharacterSelectSettingsMenu menu)
	{
		((Node)menu).GetParent().RemoveChild((Node)(object)menu);
		((Node)menu).QueueFree();
	}

	private void BuildFrame()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		((Control)_drawer).OffsetLeft = 64f;
		((Control)_drawer).OffsetRight = 544f;
		((Control)_drawer).OffsetBottom = 424f;
		((Control)_drawer).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CharacterSelectMenuStyle.Panel);
		MarginContainer val = new MarginContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		string[] array = new string[4] { "left", "top", "right", "bottom" };
		foreach (string text in array)
		{
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 18);
		}
		VBoxContainer val2 = new VBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)val2).AddChild((Node)(object)CreateHeader(), false, (InternalMode)0);
		ScrollContainer val3 = new ScrollContainer
		{
			Name = StringName.op_Implicit("SectionsScroll"),
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		((Control)_sections).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)val3).AddChild((Node)(object)_sections, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)_drawer).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)this).AddChild((Node)(object)_drawer, false, (InternalMode)0);
		((Control)_handle).OffsetRight = 64f;
		((Control)_handle).OffsetTop = 180f;
		((Control)_handle).OffsetBottom = 244f;
		CharacterSelectMenuStyle.ApplyAction((BaseButton)(object)_handle);
		_handleLabel.HorizontalAlignment = (HorizontalAlignment)1;
		_handleLabel.VerticalAlignment = (VerticalAlignment)1;
		((Control)_handleLabel).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)_handle).AddChild((Node)(object)_handleLabel, false, (InternalMode)0);
		((BaseButton)_handle).Pressed += delegate
		{
			SetOpen(!_open, focus: true);
		};
		((Node)this).AddChild((Node)(object)_handle, false, (InternalMode)0);
		CharacterSelectMenuStyle.CloseOnCancel((Node)(object)this, CloseDrawer);
	}

	private bool CloseDrawer()
	{
		if (!_open || CharacterSelectMenuStyle.NativeModalOpen)
		{
			return false;
		}
		SetOpen(open: false, focus: true);
		return true;
	}

	private HBoxContainer CreateHeader()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer();
		Label val2 = CharacterSelectMenuStyle.Label(SharedSettingsText.CharacterSettings.Current, 22, CharacterSelectMenuStyle.Text);
		((Control)val2).SizeFlagsHorizontal = (SizeFlags)3;
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Button val3 = CharacterSelectMenuStyle.TextButton("Close", "×", new Vector2(32f, 32f));
		((BaseButton)val3).Pressed += delegate
		{
			CloseDrawer();
		};
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		return val;
	}

	private void SetContent(IReadOnlyList<CharacterSelectMenuSectionDescriptor> sections)
	{
		ClearContent();
		_description = CharacterSelectDescriptionLayout.Apply(_screen);
		foreach (CharacterSelectMenuSectionDescriptor section in sections)
		{
			if (section.ExternalIconPath == null)
			{
				((Node)_sections).AddChild((Node)(object)CreateSection(section.Items), false, (InternalMode)0);
			}
			else
			{
				AddExternalSection(section);
			}
		}
		RefreshViews();
		ConfigureNavigation();
	}

	private VBoxContainer CreateSection(IEnumerable<CharacterSelectMenuItemDescriptor> items)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		foreach (CharacterSelectMenuItemDescriptor item in items)
		{
			((Node)val).AddChild((Node)(object)AddView(item.CreateView(RefreshViews)), false, (InternalMode)0);
		}
		return val;
	}

	private void AddExternalSection(CharacterSelectMenuSectionDescriptor section)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		CharacterSelectExternalPanel panel = null;
		CharacterSelectMenuActionDescriptor characterSelectMenuActionDescriptor = new CharacterSelectMenuActionDescriptor(section.Id, section.ExternalIconPath, section.Title, () => SharedSettingsText.OpenSection.Current, section.Title, delegate
		{
			OpenPanel(panel);
		})
		{
			Accent = section.ExternalAccent
		};
		Button button = AddView(characterSelectMenuActionDescriptor.CreateView(RefreshViews));
		((Node)_sections).AddChild((Node)(object)button, false, (InternalMode)0);
		panel = new CharacterSelectExternalPanel(_screen, (Control)(object)_drawer, section, section.CreateExtraContent(ConfigureNavigation), delegate
		{
			SetOpen(open: true, focus: false);
			((Control)button).GrabFocus();
		});
		((Node)_screen).AddChild((Node)(object)panel, false, (InternalMode)0);
		_panels.Add(panel);
	}

	private Button AddView(CharacterSelectMenuItemView view)
	{
		_views.Add(view);
		CharacterSelectMenuStyle.CloseOnCancel((Node)(object)view.Button, CloseDrawer);
		return view.Button;
	}

	private void OpenPanel(CharacterSelectExternalPanel panel)
	{
		if (!CharacterSelectMenuStyle.NativeModalOpen)
		{
			SetOpen(open: false, focus: false);
			panel.Toggle();
		}
	}

	private void RefreshViews()
	{
		foreach (CharacterSelectMenuItemView view in _views)
		{
			view.Refresh();
		}
	}

	private void SetOpen(bool open, bool focus)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		_open = open;
		((CanvasItem)_drawer).Visible = open;
		if (open)
		{
			foreach (CharacterSelectExternalPanel panel in _panels)
			{
				((CanvasItem)panel).Hide();
			}
		}
		((Control)_handle).TooltipText = (open ? SharedSettingsText.CloseOptions : SharedSettingsText.OpenOptions).Current;
		_handleLabel.Text = (open ? "×" : "≡");
		((GodotObject)_screen).SetMeta(StringName.op_Implicit("czn_character_select_menu_open"), Variant.op_Implicit(open));
		ConfigureNavigation();
		if (focus && ((CanvasItem)this).IsVisibleInTree())
		{
			((Control)((open && _views.Count > 0) ? _views[0].Button : _handle)).GrabFocus();
		}
	}

	private void ClearContent()
	{
		_description?.Restore();
		_description = null;
		foreach (Node child in ((Node)_sections).GetChildren(false))
		{
			((Node)_sections).RemoveChild(child);
			child.QueueFree();
		}
		foreach (CharacterSelectExternalPanel panel in _panels)
		{
			((CanvasItem)panel).Hide();
			((Node)panel).Name = StringName.op_Implicit("Retired" + ((GodotObject)panel).GetInstanceId());
			((Node)panel).QueueFree();
		}
		_panels.Clear();
		_views.Clear();
	}

	private void Release()
	{
		ClearContent();
		_unbindModal();
		RestoreNativeNavigation();
	}

	private void CaptureNativeNavigation()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		_nativeButton = FindFirstFocusable(((Node)_screen).GetNodeOrNull(NodePath.op_Implicit("CharSelectButtons/ButtonContainer")));
		if (_nativeButton != null)
		{
			if (!((GodotObject)_nativeButton).HasMeta(StringName.op_Implicit("czn_character_select_menu_native_left_neighbor")))
			{
				((GodotObject)_nativeButton).SetMeta(StringName.op_Implicit("czn_character_select_menu_native_left_neighbor"), Variant.op_Implicit(_nativeButton.FocusNeighborLeft));
			}
			Control? nativeButton = _nativeButton;
			StringName obj = StringName.op_Implicit("czn_character_select_menu_native_left_neighbor");
			Variant val = default(Variant);
			val = ((GodotObject)nativeButton).GetMeta(obj, val);
			_nativeLeftNeighbor = ((Variant)(ref val)).AsNodePath();
			((GodotObject)_nativeButton).SetMeta(StringName.op_Implicit("czn_character_select_menu_navigation_owner"), Variant.op_Implicit(_ownerId));
		}
	}

	private void RestoreNativeNavigation()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (_nativeButton != null)
		{
			Variant meta = ((GodotObject)_nativeButton).GetMeta(StringName.op_Implicit("czn_character_select_menu_navigation_owner"), Variant.op_Implicit(string.Empty));
			if (!(((Variant)(ref meta)).AsString() != _ownerId))
			{
				_nativeButton.FocusNeighborLeft = _nativeLeftNeighbor;
				((GodotObject)_nativeButton).RemoveMeta(StringName.op_Implicit("czn_character_select_menu_navigation_owner"));
				((GodotObject)_nativeButton).RemoveMeta(StringName.op_Implicit("czn_character_select_menu_native_left_neighbor"));
			}
		}
	}

	private void ConfigureNavigation()
	{
		NodePath path = ((Node)_handle).GetPath();
		Control? nativeButton = _nativeButton;
		NodePath val = ((nativeButton != null) ? ((Node)nativeButton).GetPath() : null) ?? path;
		if (_nativeButton != null)
		{
			_nativeButton.FocusNeighborLeft = path;
		}
		((Control)_handle).FocusNeighborLeft = path;
		((Control)_handle).FocusNeighborTop = path;
		((Control)_handle).FocusNeighborBottom = path;
		NodePath focusNeighborRight;
		if (!_open || _views.Count == 0)
		{
			Button handle = _handle;
			Button handle2 = _handle;
			NodePath val3 = (((Control)_handle).FocusNext = val);
			focusNeighborRight = (((Control)handle2).FocusPrevious = val3);
			((Control)handle).FocusNeighborRight = focusNeighborRight;
			return;
		}
		for (int i = 0; i < _views.Count; i++)
		{
			Button button = _views[i].Button;
			NodePath path2 = ((Node)_views[(i - 1 + _views.Count) % _views.Count].Button).GetPath();
			NodePath path3 = ((Node)_views[(i + 1) % _views.Count].Button).GetPath();
			((Control)button).FocusNeighborLeft = path;
			((Control)button).FocusNeighborRight = val;
			focusNeighborRight = (((Control)button).FocusPrevious = path2);
			((Control)button).FocusNeighborTop = focusNeighborRight;
			focusNeighborRight = (((Control)button).FocusNext = path3);
			((Control)button).FocusNeighborBottom = focusNeighborRight;
		}
		Button handle3 = _handle;
		focusNeighborRight = (((Control)_handle).FocusNext = ((Node)_views[0].Button).GetPath());
		((Control)handle3).FocusNeighborRight = focusNeighborRight;
		Button handle4 = _handle;
		List<CharacterSelectMenuItemView> views = _views;
		((Control)handle4).FocusPrevious = ((Node)views[views.Count - 1].Button).GetPath();
	}

	private static Control? FindFirstFocusable(Node? root)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (root == null)
		{
			return null;
		}
		foreach (Node child in root.GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((CanvasItem)val).Visible && ((int)val.FocusMode != 0 || val is BaseButton))
			{
				return val;
			}
			Control val2 = FindFirstFocusable(child);
			if (val2 != null)
			{
				return val2;
			}
		}
		return null;
	}
}
