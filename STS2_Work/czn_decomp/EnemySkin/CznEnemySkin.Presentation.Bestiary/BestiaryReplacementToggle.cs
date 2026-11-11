using System;
using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Application;
using CznEnemySkin.Domain;
using CznEnemySkin.Localization;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Presentation.Bestiary;

internal sealed class BestiaryReplacementToggle : Button
{
	internal const string NodeName = "CznEnemySkinReplacementToggle";

	private const string TickboxScenePath = "res://scenes/ui/tickbox.tscn";

	private EnemyReplacementService _replacements;

	private EnemyReplacementProfile? _profile;

	private IReadOnlyList<EnemyReplacementProfile> _linkedProfiles = Array.Empty<EnemyReplacementProfile>();

	private NBestiaryEntry? _selectedEntry;

	private Action<NBestiaryEntry>? _refreshSelection;

	private Label _titleLabel;

	private Label _statusLabel;

	private Control? _tickboxVisual;

	private Label? _fallbackTickbox;

	private bool _subscribedToLocaleChange;

	public override void _EnterTree()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		((Node)this)._EnterTree();
		try
		{
			LocString.SubscribeToLocaleChange(new LocaleChangeCallback(OnLocaleChanged));
			_subscribedToLocaleChange = true;
		}
		catch
		{
		}
	}

	public override void _ExitTree()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (_subscribedToLocaleChange)
		{
			try
			{
				LocString.UnsubscribeToLocaleChange(new LocaleChangeCallback(OnLocaleChanged));
			}
			catch
			{
			}
			_subscribedToLocaleChange = false;
		}
		((Node)this)._ExitTree();
	}

	public void Initialize(EnemyReplacementService replacements, Action<NBestiaryEntry> refreshSelection)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		_replacements = replacements;
		_refreshSelection = refreshSelection;
		((Node)this).Name = StringName.op_Implicit("CznEnemySkinReplacementToggle");
		((CanvasItem)this).Visible = false;
		((Control)this).FocusMode = (FocusModeEnum)2;
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		((Control)this).MouseDefaultCursorShape = (CursorShape)2;
		((BaseButton)this).ToggleMode = true;
		((Button)this).Text = string.Empty;
		((Button)this).Flat = true;
		((Control)this).CustomMinimumSize = new Vector2(470f, 72f);
		((Control)this).AnchorLeft = 0f;
		((Control)this).AnchorRight = 0f;
		((Control)this).AnchorTop = 1f;
		((Control)this).AnchorBottom = 1f;
		((Control)this).OffsetLeft = 340f;
		((Control)this).OffsetRight = 810f;
		((Control)this).OffsetTop = -138f;
		((Control)this).OffsetBottom = -66f;
		((Control)this).TooltipText = EnemyLocalization.GetToggleTooltip();
		AddTransparentStyles((Button)(object)this);
		BuildCheckboxContent();
		((BaseButton)this).Pressed += OnPressed;
	}

	public void ShowProfile(EnemyReplacementProfile profile, NBestiaryEntry selectedEntry, IReadOnlyList<EnemyReplacementProfile>? linkedProfiles = null)
	{
		_profile = profile;
		_linkedProfiles = linkedProfiles ?? new _003C_003Ez__ReadOnlySingleElementList<EnemyReplacementProfile>(profile);
		_selectedEntry = selectedEntry;
		((CanvasItem)this).Visible = true;
		RefreshState();
	}

	public void HideProfile()
	{
		if (((Control)this).HasFocus())
		{
			((Control)this).ReleaseFocus();
		}
		_profile = null;
		_linkedProfiles = Array.Empty<EnemyReplacementProfile>();
		_selectedEntry = null;
		((CanvasItem)this).Visible = false;
	}

	private void OnPressed()
	{
		if (_profile == null || _selectedEntry == null)
		{
			return;
		}
		bool flag = !_replacements.IsEnabled(_profile);
		foreach (EnemyReplacementProfile linkedProfile in _linkedProfiles)
		{
			_replacements.SetEnabled(linkedProfile, flag);
		}
		((BaseButton)this).SetPressedNoSignal(flag);
		UpdateVisualState(flag);
		_refreshSelection?.Invoke(_selectedEntry);
	}

	private void RefreshState()
	{
		if (_profile != null)
		{
			bool flag = _linkedProfiles.Count > 0 && _linkedProfiles.All(_replacements.IsEnabled);
			((BaseButton)this).SetPressedNoSignal(flag);
			UpdateVisualState(flag);
		}
	}

	private void OnLocaleChanged()
	{
		((Control)this).TooltipText = EnemyLocalization.GetToggleTooltip();
		if (_profile != null)
		{
			RefreshState();
			if (_selectedEntry != null)
			{
				_refreshSelection?.Invoke(_selectedEntry);
			}
		}
	}

	private void BuildCheckboxContent()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Expected O, but got Unknown
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		HBoxContainer val = new HBoxContainer
		{
			Name = StringName.op_Implicit("CheckboxRow"),
			MouseFilter = (MouseFilterEnum)2,
			CustomMinimumSize = new Vector2(470f, 72f)
		};
		((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)this).AddChild((Node)(object)val, false, (InternalMode)0);
		_titleLabel = new Label
		{
			Name = StringName.op_Implicit("ReplacementLabel"),
			Text = string.Empty,
			MouseFilter = (MouseFilterEnum)2,
			CustomMinimumSize = new Vector2(280f, 72f),
			VerticalAlignment = (VerticalAlignment)1
		};
		((Control)_titleLabel).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 24);
		((Node)val).AddChild((Node)(object)_titleLabel, false, (InternalMode)0);
		_statusLabel = new Label
		{
			Name = StringName.op_Implicit("ReplacementStatus"),
			MouseFilter = (MouseFilterEnum)2,
			CustomMinimumSize = new Vector2(86f, 72f),
			HorizontalAlignment = (HorizontalAlignment)2,
			VerticalAlignment = (VerticalAlignment)1
		};
		((Control)_statusLabel).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
		((Node)val).AddChild((Node)(object)_statusLabel, false, (InternalMode)0);
		CenterContainer val2 = new CenterContainer
		{
			Name = StringName.op_Implicit("CheckboxCenter"),
			MouseFilter = (MouseFilterEnum)2,
			CustomMinimumSize = new Vector2(64f, 72f)
		};
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		try
		{
			PackedScene val3 = ResourceLoader.Load<PackedScene>("res://scenes/ui/tickbox.tscn", (string)null, (CacheMode)1);
			if (val3 != null)
			{
				_tickboxVisual = val3.Instantiate<Control>((GenEditState)0);
				((Node)_tickboxVisual).Name = StringName.op_Implicit("TickboxVisual");
				_tickboxVisual.MouseFilter = (MouseFilterEnum)2;
				_tickboxVisual.CustomMinimumSize = new Vector2(64f, 64f);
				((Node)val2).AddChild((Node)(object)_tickboxVisual, false, (InternalMode)0);
				return;
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Bestiary tickbox visual unavailable; using text fallback [error=" + ex.Message + "].", 1);
		}
		_fallbackTickbox = new Label
		{
			Name = StringName.op_Implicit("TickboxFallback"),
			MouseFilter = (MouseFilterEnum)2,
			CustomMinimumSize = new Vector2(64f, 64f),
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1
		};
		((Control)_fallbackTickbox).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 36);
		((Node)val2).AddChild((Node)(object)_fallbackTickbox, false, (InternalMode)0);
	}

	private void UpdateVisualState(bool enabled)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (_profile == null)
		{
			return;
		}
		_titleLabel.Text = EnemyLocalization.GetToggleLabel(_profile);
		_statusLabel.Text = EnemyLocalization.GetStatus(enabled);
		((CanvasItem)_statusLabel).Modulate = (enabled ? new Color("E6D6A4") : new Color("928C82"));
		if (_tickboxVisual != null && GodotObject.IsInstanceValid((GodotObject)(object)_tickboxVisual))
		{
			CanvasItem nodeOrNull = ((Node)_tickboxVisual).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Ticked"));
			CanvasItem nodeOrNull2 = ((Node)_tickboxVisual).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("NotTicked"));
			if (nodeOrNull != null)
			{
				nodeOrNull.Visible = enabled;
			}
			if (nodeOrNull2 != null)
			{
				nodeOrNull2.Visible = !enabled;
			}
		}
		if (_fallbackTickbox != null)
		{
			_fallbackTickbox.Text = (enabled ? "☑" : "☐");
		}
	}

	private static void AddTransparentStyles(Button button)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		string[] array = new string[5] { "normal", "hover", "pressed", "focus", "disabled" };
		foreach (string text in array)
		{
			((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)new StyleBoxEmpty());
		}
	}
}
