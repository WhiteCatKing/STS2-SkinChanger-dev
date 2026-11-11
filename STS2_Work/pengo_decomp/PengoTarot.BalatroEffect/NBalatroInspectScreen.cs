using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;

namespace PengoTarot.BalatroEffect;

[ScriptPath("res://balatroeffect/Scenes/NBalatroInspectScreen.cs")]
public class NBalatroInspectScreen : Control, IScreenContext
{
	public class MethodName : MethodName
	{
		public static readonly StringName Tr = StringName.op_Implicit("Tr");

		public static readonly StringName Create = StringName.op_Implicit("Create");

		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName ApplyLocaleFontsToUi = StringName.op_Implicit("ApplyLocaleFontsToUi");

		public static readonly StringName InitializeFromScene = StringName.op_Implicit("InitializeFromScene");

		public static readonly StringName AddHoverScale = StringName.op_Implicit("AddHoverScale");

		public static readonly StringName HoverScaleTo = StringName.op_Implicit("HoverScaleTo");

		public static readonly StringName UpdateButtonAvailability = StringName.op_Implicit("UpdateButtonAvailability");

		public static readonly StringName UpdateMenuAvailability = StringName.op_Implicit("UpdateMenuAvailability");

		public static readonly StringName Close = StringName.op_Implicit("Close");

		public static readonly StringName OnRightButtonReleased = StringName.op_Implicit("OnRightButtonReleased");

		public static readonly StringName OnLeftButtonReleased = StringName.op_Implicit("OnLeftButtonReleased");

		public static readonly StringName OnEnchantTickClicked = StringName.op_Implicit("OnEnchantTickClicked");

		public static readonly StringName SwitchToEnchant = StringName.op_Implicit("SwitchToEnchant");

		public static readonly StringName SetCard = StringName.op_Implicit("SetCard");

		public static readonly StringName RefreshPanelState = StringName.op_Implicit("RefreshPanelState");

		public static readonly StringName WireModeButton = StringName.op_Implicit("WireModeButton");

		public static readonly StringName ModeLocKey = StringName.op_Implicit("ModeLocKey");

		public static readonly StringName UpdateModeButtonStyles = StringName.op_Implicit("UpdateModeButtonStyles");

		public static readonly StringName SetModeButtonStyle = StringName.op_Implicit("SetModeButtonStyle");

		public static readonly StringName SetTickState = StringName.op_Implicit("SetTickState");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName DefaultFocusedControl = StringName.op_Implicit("DefaultFocusedControl");

		public static readonly StringName _card = StringName.op_Implicit("_card");

		public static readonly StringName _backstop = StringName.op_Implicit("_backstop");

		public static readonly StringName _hoverTipRect = StringName.op_Implicit("_hoverTipRect");

		public static readonly StringName _leftButton = StringName.op_Implicit("_leftButton");

		public static readonly StringName _rightButton = StringName.op_Implicit("_rightButton");

		public static readonly StringName _cardArea = StringName.op_Implicit("_cardArea");

		public static readonly StringName _panelArea = StringName.op_Implicit("_panelArea");

		public static readonly StringName _slider = StringName.op_Implicit("_slider");

		public static readonly StringName _exportBtn = StringName.op_Implicit("_exportBtn");

		public static readonly StringName _importBtn = StringName.op_Implicit("_importBtn");

		public static readonly StringName _menuBtn = StringName.op_Implicit("_menuBtn");

		public static readonly StringName _menuPopup = StringName.op_Implicit("_menuPopup");

		public static readonly StringName _enchantTickbox = StringName.op_Implicit("_enchantTickbox");

		public static readonly StringName _enchantTickVisuals = StringName.op_Implicit("_enchantTickVisuals");

		public static readonly StringName _suppressOriginReturn = StringName.op_Implicit("_suppressOriginReturn");

		public static readonly StringName _index = StringName.op_Implicit("_index");

		public static readonly StringName _viewAllUpgraded = StringName.op_Implicit("_viewAllUpgraded");

		public static readonly StringName _cardPosition = StringName.op_Implicit("_cardPosition");

		public static readonly StringName _leftButtonX = StringName.op_Implicit("_leftButtonX");

		public static readonly StringName _rightButtonX = StringName.op_Implicit("_rightButtonX");

		public static readonly StringName _openTween = StringName.op_Implicit("_openTween");

		public static readonly StringName _cardTween = StringName.op_Implicit("_cardTween");

		public static readonly StringName _originInspect = StringName.op_Implicit("_originInspect");
	}

	public class SignalName : SignalName
	{
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static ValueChangedEventHandler _003C_003E9__44_4;

		public static Action<NButton> _003C_003E9__44_5;

		public static Action<NButton> _003C_003E9__44_6;

		public static Action<int> _003C_003E9__44_7;

		public static Action<NButton> _003C_003E9__44_9;

		public static Action<NButton> _003C_003E9__44_10;

		internal void _003CInitializeFromScene_003Eb__44_4(double v)
		{
			Config.SetIntensity(InspectScreen.CurrentCardId, (double)(float)v / 100.0);
		}

		internal void _003CInitializeFromScene_003Eb__44_5(NButton _)
		{
			InspectScreen.OnCopyCard();
		}

		internal void _003CInitializeFromScene_003Eb__44_6(NButton _)
		{
			InspectScreen.OnPasteCard();
		}

		internal void _003CInitializeFromScene_003Eb__44_7(int id)
		{
			InspectScreen.OnMenuAction(id);
		}

		internal void _003CInitializeFromScene_003Eb__44_9(NButton _)
		{
			InspectScreen.OnPaginatorNavigate(-1);
		}

		internal void _003CInitializeFromScene_003Eb__44_10(NButton _)
		{
			InspectScreen.OnPaginatorNavigate(1);
		}
	}

	private const string SliderName = "BalatroEffectsSlider";

	private const string AuthorPresetPath = "res://balatroeffect/Assets/author_preset.json";

	private static readonly string CardScenePath = "res://scenes/cards/card.tscn";

	private static Font? _boldFont;

	private static Font? _regularFont;

	private NCard _card;

	private ColorRect _backstop;

	private Control _hoverTipRect;

	private NButton _leftButton;

	private NButton _rightButton;

	private Control _cardArea;

	private Control _panelArea;

	private NSlider _slider;

	private NButton _exportBtn;

	private NButton _importBtn;

	private NButton _menuBtn;

	private PopupMenu _menuPopup;

	private Control _enchantTickbox;

	private Control _enchantTickVisuals;

	private bool _suppressOriginReturn;

	private readonly Dictionary<Control, Tween> _hoverTweens = new Dictionary<Control, Tween>();

	private List<CardModel>? _cards;

	private int _index;

	private bool _viewAllUpgraded;

	private Vector2 _cardPosition;

	private float _leftButtonX;

	private float _rightButtonX;

	private Tween? _openTween;

	private Tween? _cardTween;

	private NInspectCardScreen? _originInspect;

	private static Font BoldFont
	{
		get
		{
			object obj = _boldFont;
			if (obj == null)
			{
				obj = LocaleFontUtil.GetLocaleFont((FontType)1) ?? GD.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres");
				_boldFont = (Font?)obj;
			}
			return (Font)obj;
		}
	}

	private static Font RegularFont
	{
		get
		{
			object obj = _regularFont;
			if (obj == null)
			{
				obj = LocaleFontUtil.GetLocaleFont((FontType)0) ?? GD.Load<Font>("res://themes/kreon_regular_shared.tres");
				_regularFont = (Font?)obj;
			}
			return (Font)obj;
		}
	}

	internal List<CardModel>? CurrentCards => _cards;

	public static string[] AssetPaths => new string[1] { CardScenePath };

	public Control? DefaultFocusedControl => null;

	private static string Tr(string key)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;
	}

	public static NBalatroInspectScreen? Create()
	{
		return GD.Load<PackedScene>("res://balatroeffect/Scenes/balatro_inspect_screen.tscn").Instantiate<NBalatroInspectScreen>((GenEditState)0);
	}

	public override void _Ready()
	{
		ApplyLocaleFontsToUi();
		InitializeFromScene();
	}

	private void ApplyLocaleFontsToUi()
	{
		foreach (Node item in ((Node)this).FindChildren("*", "Label", true, false))
		{
			Label val = (Label)(object)((item is Label) ? item : null);
			if (val != null)
			{
				FontControlUtils.ApplyLocaleFontSubstitution((Control)(object)val, (FontType)0, StringName.op_Implicit("font"));
			}
		}
		foreach (Node item2 in ((Node)this).FindChildren("*", "RichTextLabel", true, false))
		{
			RichTextLabel val2 = (RichTextLabel)(object)((item2 is RichTextLabel) ? item2 : null);
			if (val2 != null)
			{
				FontControlUtils.ApplyLocaleFontSubstitution((Control)(object)val2, (FontType)0, StringName.op_Implicit("normal_font"));
			}
		}
	}

	private void InitializeFromScene()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Expected O, but got Unknown
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_065e: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e7: Expected O, but got Unknown
		//IL_0722: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_083d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0843: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d9: Unknown result type (might be due to invalid IL or missing references)
		_backstop = ((Node)this).GetNode<ColorRect>(NodePath.op_Implicit("Backstop"));
		((GodotObject)((Node)this).GetNode<NButton>(NodePath.op_Implicit("BackstopHit"))).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			Close();
		}), 0u);
		_cardArea = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%CardArea"));
		_hoverTipRect = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%HoverTipRect"));
		Control node = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%CardAnchor"));
		PackedScene val = GD.Load<PackedScene>(CardScenePath);
		_card = val.Instantiate<NCard>((GenEditState)0);
		((Node)_card).Name = StringName.op_Implicit("Card");
		((Control)_card).SetAnchorsPreset((LayoutPreset)8, false);
		((Control)_card).Scale = Vector2.One * 2f;
		_cardPosition = ((Control)_card).Position;
		((Node)node).AddChild((Node)(object)_card, false, (InternalMode)0);
		_leftButton = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%LeftArrow"));
		_leftButtonX = ((Control)_leftButton).Position.X;
		((GodotObject)_leftButton).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnLeftButtonReleased();
		}), 0u);
		AddHoverScale((Control)(object)_leftButton, (Control)(object)_leftButton);
		_rightButton = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%RightArrow"));
		_rightButtonX = ((Control)_rightButton).Position.X;
		((GodotObject)_rightButton).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnRightButtonReleased();
		}), 0u);
		AddHoverScale((Control)(object)_rightButton, (Control)(object)_rightButton);
		_enchantTickbox = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%EnchantTickbox"));
		_enchantTickVisuals = ((Node)_enchantTickbox).GetNode<Control>(NodePath.op_Implicit("TickEnchant/TickboxVisuals"));
		_enchantTickbox.MouseFilter = (MouseFilterEnum)0;
		((GodotObject)_enchantTickbox).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent evt)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			InputEventMouseButton val2 = (InputEventMouseButton)(object)((evt is InputEventMouseButton) ? evt : null);
			if (val2 != null && (long)val2.ButtonIndex == 1 && val2.Pressed)
			{
				OnEnchantTickClicked();
			}
		}), 0u);
		Label node2 = ((Node)_enchantTickbox).GetNode<Label>(NodePath.op_Implicit("Label"));
		((Control)node2).AddThemeFontOverride(StringName.op_Implicit("font"), LocaleFontUtil.GetLocaleFont((FontType)0) ?? BoldFont);
		node2.Text = Tr("VIEW_ENCHANTMENTS");
		SetTickState(_enchantTickVisuals, ticked: false);
		_panelArea = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%PanelArea"));
		VBoxContainer node3 = ((Node)this).GetNode<VBoxContainer>(NodePath.op_Implicit("PanelArea/PanelMargin/PanelVBox"));
		Label node4 = ((Node)node3).GetNode<Label>(NodePath.op_Implicit("EffectHeader"));
		((Control)node4).AddThemeFontOverride(StringName.op_Implicit("font"), BoldFont);
		node4.Text = Tr("LABEL_EFFECT");
		Label node5 = ((Node)node3).GetNode<Label>(NodePath.op_Implicit("IntensityHeader"));
		((Control)node5).AddThemeFontOverride(StringName.op_Implicit("font"), BoldFont);
		node5.Text = Tr("LABEL_INTENSITY");
		Label node6 = ((Node)node3).GetNode<Label>(NodePath.op_Implicit("PartsHeader"));
		((Control)node6).AddThemeFontOverride(StringName.op_Implicit("font"), BoldFont);
		node6.Text = Tr("LABEL_PARTS");
		Label node7 = ((Node)node3).GetNode<Label>(NodePath.op_Implicit("PartsHint"));
		((Control)node7).AddThemeFontOverride(StringName.op_Implicit("font"), RegularFont);
		node7.Text = Tr("HINT_CHECK_PARTS");
		foreach (Node child in ((Node)((Node)node3).GetNode<GridContainer>(NodePath.op_Implicit("PartsGrid"))).GetChildren(false))
		{
			HBoxContainer val3 = (HBoxContainer)(object)((child is HBoxContainer) ? child : null);
			if (val3 == null || !((object)((Node)val3).Name).ToString().StartsWith("Row_"))
			{
				continue;
			}
			string text = ((object)((Node)val3).Name).ToString().Substring(4);
			Label nodeOrNull = ((Node)val3).GetNodeOrNull<Label>(NodePath.op_Implicit("Label"));
			if (nodeOrNull != null)
			{
				nodeOrNull.Text = Tr("PART_" + text.ToUpperInvariant());
			}
			Control val4 = null;
			foreach (Node child2 in ((Node)val3).GetChildren(false))
			{
				Control val5 = (Control)(object)((child2 is Control) ? child2 : null);
				if (val5 != null && ((object)((Node)val5).Name).ToString().StartsWith("Tick"))
				{
					val4 = val5;
					break;
				}
			}
			if (val4 == null)
			{
				continue;
			}
			string partName = text;
			Control nodeOrNull2 = ((Node)val4).GetNodeOrNull<Control>(NodePath.op_Implicit("TickboxVisuals"));
			((Control)val3).MouseFilter = (MouseFilterEnum)0;
			((GodotObject)val3).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent evt)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I8
				InputEventMouseButton val6 = (InputEventMouseButton)(object)((evt is InputEventMouseButton) ? evt : null);
				if (val6 != null && (long)val6.ButtonIndex == 1 && val6.Pressed)
				{
					InspectScreen.OnPartClicked(partName);
				}
			}), 0u);
			val4.MouseFilter = (MouseFilterEnum)2;
			if (nodeOrNull != null)
			{
				((Control)nodeOrNull).MouseFilter = (MouseFilterEnum)2;
			}
			if (nodeOrNull2 != null)
			{
				AddHoverScale((Control)(object)val3, nodeOrNull2, 1.15f);
			}
		}
		_slider = ((Node)this).GetNode<NSlider>(NodePath.op_Implicit("%SliderCenter/Slider"));
		((Node)_slider).Name = StringName.op_Implicit("BalatroEffectsSlider");
		NSlider slider = _slider;
		object obj = _003C_003Ec._003C_003E9__44_4;
		if (obj == null)
		{
			ValueChangedEventHandler val7 = delegate(double v)
			{
				Config.SetIntensity(InspectScreen.CurrentCardId, (double)(float)v / 100.0);
			};
			_003C_003Ec._003C_003E9__44_4 = val7;
			obj = (object)val7;
		}
		((Range)slider).ValueChanged += (ValueChangedEventHandler)obj;
		Control nodeOrNull3 = ((Node)_slider).GetNodeOrNull<Control>(NodePath.op_Implicit("%Handle"));
		if (nodeOrNull3 != null)
		{
			AddHoverScale((Control)(object)_slider, nodeOrNull3, 1.15f);
		}
		_exportBtn = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%ExportBtn"));
		((Node)_exportBtn).GetNode<Label>(NodePath.op_Implicit("Label")).Text = Tr("BTN_COPY_CARD");
		((GodotObject)_exportBtn).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			InspectScreen.OnCopyCard();
		}), 0u);
		AddHoverScale((Control)(object)_exportBtn, (Control)(object)_exportBtn);
		_importBtn = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%ImportBtn"));
		((Node)_importBtn).GetNode<Label>(NodePath.op_Implicit("Label")).Text = Tr("BTN_PASTE_CARD");
		((GodotObject)_importBtn).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			InspectScreen.OnPasteCard();
		}), 0u);
		AddHoverScale((Control)(object)_importBtn, (Control)(object)_importBtn);
		_menuBtn = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%MenuBtnHolder"));
		((Node)_menuBtn).GetNode<Label>(NodePath.op_Implicit("Label")).Text = Tr("MENU_TITLE");
		AddHoverScale((Control)(object)_menuBtn, (Control)(object)_menuBtn);
		_menuPopup = new PopupMenu
		{
			Name = StringName.op_Implicit("MenuPopup")
		};
		((Window)_menuPopup).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 28);
		((Window)_menuPopup).AddThemeColorOverride(StringName.op_Implicit("font_disabled_color"), new Color(0.35f, 0.35f, 0.35f, 1f));
		_menuPopup.AddItem(Tr("MENU_CLEAR"), 0, (Key)0);
		_menuPopup.AddItem(Tr("MENU_RESET"), 1, (Key)0);
		_menuPopup.AddItem(Tr("MENU_APPLY_TO_VISIBLE"), 2, (Key)0);
		_menuPopup.AddItem(Tr("MENU_EXPORT_GLOBAL"), 3, (Key)0);
		_menuPopup.AddItem(Tr("MENU_IMPORT_GLOBAL"), 4, (Key)0);
		_menuPopup.AddItem(Tr("MENU_LOAD_AUTHOR_GLOBAL"), 5, (Key)0);
		((GodotObject)_menuPopup).Connect(SignalName.IdPressed, Callable.From<int>((Action<int>)delegate(int id)
		{
			InspectScreen.OnMenuAction(id);
		}), 0u);
		((Node)_menuBtn).AddChild((Node)(object)_menuPopup, false, (InternalMode)0);
		Font localeFont = LocaleFontUtil.GetLocaleFont((FontType)0);
		if (localeFont != null)
		{
			((Window)_menuPopup).AddThemeFontOverride(StringName.op_Implicit("font"), localeFont);
		}
		((GodotObject)_menuBtn).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			UpdateMenuAvailability();
			Vector2 globalPosition = ((Control)_menuBtn).GlobalPosition;
			Vector2 size = ((Control)_menuBtn).Size;
			((Window)_menuPopup).Popup((Rect2I?)new Rect2I((int)globalPosition.X, (int)(globalPosition.Y + size.Y), 0, 0));
		}), 0u);
		NButton node8 = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%PgLeftBtn"));
		NButton node9 = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%PgRightBtn"));
		Label node10 = ((Node)this).GetNode<Label>(NodePath.op_Implicit("%PgLabel"));
		((GodotObject)node8).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			InspectScreen.OnPaginatorNavigate(-1);
		}), 0u);
		((GodotObject)node9).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			InspectScreen.OnPaginatorNavigate(1);
		}), 0u);
		AddHoverScale((Control)(object)node8, (Control)(object)node8);
		AddHoverScale((Control)(object)node9, (Control)(object)node9);
		InspectScreen.InitPaginator(node10);
		WireModeButton("%NormalModeBtn", "normal");
		WireModeButton("%SeparatelyModeBtn", "separately");
		WireModeButton("%FullyModeBtn", "fully");
		ShaderController.ApplyStarcloudBgAspect((Control)(object)((Node)this).GetNodeOrNull<ColorRect>(NodePath.op_Implicit("CardAreaBg")));
		((CanvasItem)this).Visible = false;
		((NClickableControl)_leftButton).Disable();
		((NClickableControl)_rightButton).Disable();
	}

	private void AddHoverScale(Control trigger, Control target, float scale = 1.04f)
	{
		trigger.MouseEntered += delegate
		{
			HoverScaleTo(target, scale);
		};
		trigger.MouseExited += delegate
		{
			HoverScaleTo(target, 1f);
		};
	}

	private void HoverScaleTo(Control target, float scale)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_hoverTweens.TryGetValue(target, out Tween value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			value.Kill();
		}
		target.PivotOffset = target.Size / 2f;
		Tween val = ((Node)target).CreateTween();
		val.TweenProperty((GodotObject)(object)target, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * scale), 0.10000000149011612).SetTrans((TransitionType)4).SetEase((EaseType)1);
		_hoverTweens[target] = val;
	}

	private static void UpdateButtonAvailability(NButton btn, bool valid)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((CanvasItem)btn).Modulate = (valid ? Colors.White : Color.FromHsv(0f, 0.1f, 0.72f, 1f));
	}

	private void UpdateMenuAvailability()
	{
		bool flag = FileAccess.FileExists("res://balatroeffect/Assets/author_preset.json");
		string json = DisplayServer.ClipboardGet() ?? "";
		_menuPopup.SetItemDisabled(0, !Config.HasEffect(InspectScreen.CurrentCardId));
		_menuPopup.SetItemDisabled(1, !flag);
		_menuPopup.SetItemDisabled(2, CurrentCards == null || CurrentCards.Count == 0);
		_menuPopup.SetItemDisabled(3, !Config.HasAnyEffect());
		_menuPopup.SetItemDisabled(4, !Config.IsValidPresetJson(json));
		_menuPopup.SetItemDisabled(5, !flag);
	}

	public void Open(List<CardModel> cards, int index, bool viewAllUpgraded = false, NInspectCardScreen? origin = null)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		_cards = cards;
		_originInspect = origin;
		_viewAllUpgraded = viewAllUpgraded;
		((CanvasItem)this).Visible = true;
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		SetCard(index);
		((Control)_card).Scale = Vector2.One * 1.75f;
		((CanvasItem)_card).Modulate = StsColors.transparentBlack;
		((CanvasItem)_leftButton).Modulate = StsColors.transparentBlack;
		((CanvasItem)_rightButton).Modulate = StsColors.transparentBlack;
		Tween? openTween = _openTween;
		if (openTween != null)
		{
			openTween.Kill();
		}
		_openTween = ((Node)this).CreateTween().SetParallel(true);
		_openTween.TweenProperty((GodotObject)(object)_backstop, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.9f), 0.25);
		_openTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(1f), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)5)
			.From(Variant.op_Implicit(0f));
		_openTween.TweenProperty((GodotObject)(object)_leftButton, NodePath.op_Implicit("position:x"), Variant.op_Implicit(_leftButtonX), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)10)
			.From(Variant.op_Implicit(_leftButtonX + 100f))
			.SetDelay(0.1);
		_openTween.TweenProperty((GodotObject)(object)_leftButton, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.25).SetDelay(0.1);
		_openTween.TweenProperty((GodotObject)(object)_rightButton, NodePath.op_Implicit("position:x"), Variant.op_Implicit(_rightButtonX), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)10)
			.From(Variant.op_Implicit(_rightButtonX - 100f))
			.SetDelay(0.1);
		_openTween.TweenProperty((GodotObject)(object)_rightButton, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.25).SetDelay(0.1);
		Tween? cardTween = _cardTween;
		if (cardTween != null)
		{
			cardTween.Kill();
		}
		_cardTween = ((Node)this).CreateTween().SetParallel(true);
		_cardTween.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.25);
		_cardTween.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * 2f), 0.15).SetEase((EaseType)1).SetTrans((TransitionType)11)
			.SetDelay(0.1);
		ActiveScreenContext.Instance.Update();
		NHotkeyManager.Instance.AddBlockingScreen((Node)(object)this);
		((NClickableControl)_rightButton).Enable();
		((NClickableControl)_leftButton).Enable();
		NHotkeyManager.Instance.PushHotkeyPressedBinding(StringName.op_Implicit(MegaInput.cancel), (Action)Close);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(StringName.op_Implicit(MegaInput.pauseAndBack), (Action)Close);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(StringName.op_Implicit(MegaInput.left), (Action)OnLeftButtonReleased);
		NHotkeyManager.Instance.PushHotkeyPressedBinding(StringName.op_Implicit(MegaInput.right), (Action)OnRightButtonReleased);
	}

	public void Close()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		if (!((CanvasItem)this).Visible)
		{
			return;
		}
		((Control)this).MouseFilter = (MouseFilterEnum)2;
		((Control)_leftButton).MouseFilter = (MouseFilterEnum)2;
		((Control)_rightButton).MouseFilter = (MouseFilterEnum)2;
		((NClickableControl)_rightButton).Disable();
		((NClickableControl)_leftButton).Disable();
		NHoverTipSet.Clear();
		Tween? openTween = _openTween;
		if (openTween != null)
		{
			openTween.Kill();
		}
		_openTween = ((Node)this).CreateTween().SetParallel(true);
		_openTween.TweenProperty((GodotObject)(object)_backstop, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.25);
		_openTween.TweenProperty((GodotObject)(object)_leftButton, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.1);
		_openTween.TweenProperty((GodotObject)(object)_rightButton, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.1);
		_openTween.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("modulate"), Variant.op_Implicit(StsColors.transparentWhite), 0.1);
		_openTween.Chain().TweenCallback(Callable.From((Action)delegate
		{
			((CanvasItem)this).Visible = false;
			ActiveScreenContext.Instance.Update();
			if (!_suppressOriginReturn && _originInspect != null && _cards != null)
			{
				_originInspect.Open(_cards, _index, _viewAllUpgraded);
			}
			((Node)this).QueueFree();
		}));
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.cancel), (Action)Close);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.pauseAndBack), (Action)Close);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.left), (Action)OnLeftButtonReleased);
		NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.right), (Action)OnRightButtonReleased);
		NHotkeyManager.Instance.RemoveBlockingScreen((Node)(object)this);
	}

	private void OnRightButtonReleased()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (((CanvasItem)_rightButton).Visible && _cards != null)
		{
			SetCard(_index + 1);
			((CanvasItem)_card).Modulate = Colors.White;
			Tween? openTween = _openTween;
			if (openTween != null)
			{
				openTween.Kill();
			}
			_openTween = ((Node)this).CreateTween().SetParallel(true);
			_openTween.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("position"), Variant.op_Implicit(_cardPosition), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)5)
				.From(Variant.op_Implicit(_cardPosition + new Vector2(100f, 0f)));
		}
	}

	private void OnLeftButtonReleased()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (((CanvasItem)_leftButton).Visible && _cards != null)
		{
			SetCard(_index - 1);
			((CanvasItem)_card).Modulate = Colors.White;
			Tween? openTween = _openTween;
			if (openTween != null)
			{
				openTween.Kill();
			}
			_openTween = ((Node)this).CreateTween().SetParallel(true);
			_openTween.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("position"), Variant.op_Implicit(_cardPosition), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)5)
				.From(Variant.op_Implicit(_cardPosition + new Vector2(-100f, 0f)));
		}
	}

	private void OnEnchantTickClicked()
	{
		SetTickState(_enchantTickVisuals, ticked: true);
		SwitchToEnchant();
	}

	private void SwitchToEnchant()
	{
		if (_cards == null || _cards.Count == 0)
		{
			SetTickState(_enchantTickVisuals, ticked: false);
			return;
		}
		_suppressOriginReturn = true;
		List<CardModel> cards = _cards;
		int index = _index;
		bool viewAllUpgraded = _viewAllUpgraded;
		NInspectCardScreen originInspect = _originInspect;
		Close();
		NBalatroInspectEnchantScreen nBalatroInspectEnchantScreen = NBalatroInspectEnchantScreen.Create();
		if (nBalatroInspectEnchantScreen != null)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
			if (obj != null)
			{
				((Node)((SceneTree)obj).Root).AddChild((Node)(object)nBalatroInspectEnchantScreen, false, (InternalMode)0);
			}
			nBalatroInspectEnchantScreen.Open(cards, index, viewAllUpgraded, originInspect);
		}
	}

	private void SetCard(int index)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (_cards != null)
		{
			_index = Math.Clamp(index, 0, _cards.Count - 1);
			((CanvasItem)_leftButton).Visible = _index > 0;
			((Control)_leftButton).MouseFilter = (MouseFilterEnum)(((CanvasItem)_leftButton).Visible ? 0 : 2);
			((CanvasItem)_rightButton).Visible = _index < _cards.Count - 1;
			((Control)_rightButton).MouseFilter = (MouseFilterEnum)(((CanvasItem)_rightButton).Visible ? 0 : 2);
			CardModel val = _cards[_index];
			CardModel val2 = (CardModel)((AbstractModel)val).MutableClone();
			if (_viewAllUpgraded && !val.IsUpgraded && val.IsUpgradable)
			{
				val2.UpgradePreviewType = (CardUpgradePreviewType)1;
				val2.UpgradeInternal();
				_card.Model = val2;
				_card.ShowUpgradePreview();
			}
			else
			{
				_card.Model = val2;
				_card.UpdateVisuals((PileType)0, (CardPreviewMode)1);
			}
			NHoverTipSet.Clear();
			NHoverTipSet obj = NHoverTipSet.CreateAndShow((Control)(object)this, val2.HoverTips, (HoverTipAlignment)0);
			if (obj != null)
			{
				obj.SetAlignment(_hoverTipRect, HoverTip.GetHoverTipAlignment((Control)(object)this, 0.75f));
			}
			InspectScreen.CurrentCardId = ((object)((AbstractModel)val).Id).ToString();
			InspectScreen.UpdatePaginatorTarget(InspectScreen.CurrentCardId);
			if (Config.GetEditMode(InspectScreen.CurrentCardId) == "separately")
			{
				InspectScreen.SeparatelyHighlight = ((Config.AllPartNames.Length != 0) ? Config.AllPartNames[0] : "");
			}
			RefreshPanelState();
			ShaderController.ApplyShader(_card);
		}
	}

	public void RefreshPanelState()
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		NSlider slider = _slider;
		if (slider != null)
		{
			((GodotObject)slider).SetBlockSignals(true);
		}
		if (_slider != null)
		{
			((Range)_slider).Value = Config.GetIntensity(InspectScreen.CurrentCardId) * 100.0;
		}
		NSlider slider2 = _slider;
		if (slider2 != null)
		{
			((GodotObject)slider2).SetBlockSignals(false);
		}
		string editMode = Config.GetEditMode(InspectScreen.CurrentCardId);
		UpdateModeButtonStyles(editMode);
		bool flag = editMode == "separately";
		bool flag2 = editMode == "fully";
		string[] allPartNames = Config.AllPartNames;
		foreach (string text in allPartNames)
		{
			int effect = Config.GetEffect(InspectScreen.CurrentCardId, text);
			SetTickState("%Tick" + text, effect > 0);
			Control nodeOrNull = ((Node)this).GetNodeOrNull<Control>(NodePath.op_Implicit("%Tick" + text));
			Control val = ((nodeOrNull != null) ? ((Node)nodeOrNull).GetNodeOrNull<Control>(NodePath.op_Implicit("TickboxVisuals")) : null);
			if (val != null)
			{
				bool flag3 = (flag ? (text != InspectScreen.SeparatelyHighlight) : (flag2 ? true : false));
				((CanvasItem)val).Modulate = (Color)(flag3 ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White);
				Control nodeOrNull2 = ((Node)this).GetNodeOrNull<Control>(NodePath.op_Implicit("%Tick" + text));
				Node obj = ((nodeOrNull2 != null) ? ((Node)nodeOrNull2).GetParent() : null);
				Node obj2 = ((obj is HBoxContainer) ? obj : null);
				Label val2 = ((obj2 != null) ? obj2.GetNodeOrNull<Label>(NodePath.op_Implicit("Label")) : null);
				if (val2 != null)
				{
					((CanvasItem)val2).Modulate = (Color)(flag3 ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White);
				}
			}
		}
		UpdateButtonAvailability(_exportBtn, Config.HasEffect(InspectScreen.CurrentCardId));
		UpdateButtonAvailability(_importBtn, Config.IsValidCardPresetJson(DisplayServer.ClipboardGet() ?? ""));
	}

	private void WireModeButton(string nodePath, string mode)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Label nodeOrNull = ((Node)this).GetNodeOrNull<Label>(NodePath.op_Implicit(nodePath));
		if (nodeOrNull == null)
		{
			return;
		}
		nodeOrNull.Text = Tr(ModeLocKey(mode));
		((Control)nodeOrNull).MouseFilter = (MouseFilterEnum)0;
		((GodotObject)nodeOrNull).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent evt)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((evt is InputEventMouseButton) ? evt : null);
			if (val != null && (long)val.ButtonIndex == 1 && val.Pressed)
			{
				InspectScreen.OnEditModeSelected(mode);
			}
		}), 0u);
		AddHoverScale((Control)(object)nodeOrNull, (Control)(object)nodeOrNull, 1.06f);
	}

	private static string ModeLocKey(string mode)
	{
		if (!(mode == "normal"))
		{
			if (mode == "separately")
			{
				return "MODE_SEPARATELY";
			}
			return "MODE_FULLY";
		}
		return "MODE_NORMAL";
	}

	private void UpdateModeButtonStyles(string editMode)
	{
		SetModeButtonStyle(((Node)this).GetNodeOrNull<Label>(NodePath.op_Implicit("%NormalModeBtn")), editMode == "normal");
		SetModeButtonStyle(((Node)this).GetNodeOrNull<Label>(NodePath.op_Implicit("%SeparatelyModeBtn")), editMode == "separately");
		SetModeButtonStyle(((Node)this).GetNodeOrNull<Label>(NodePath.op_Implicit("%FullyModeBtn")), editMode == "fully");
	}

	private static void SetModeButtonStyle(Label? label, bool active)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		if (label != null)
		{
			((CanvasItem)label).Modulate = (Color)(active ? Colors.White : new Color(0.8f, 0.8f, 0.8f, 0.8f));
			if (active)
			{
				((Control)label).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color(0.144f, 0.331f, 0.36f, 1f));
				((Control)label).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 12);
			}
			else
			{
				((Control)label).RemoveThemeColorOverride(StringName.op_Implicit("font_outline_color"));
				((Control)label).RemoveThemeConstantOverride(StringName.op_Implicit("outline_size"));
			}
		}
	}

	private void SetTickState(string nodePath, bool ticked)
	{
		Control nodeOrNull = ((Node)this).GetNodeOrNull<Control>(NodePath.op_Implicit(nodePath));
		if (nodeOrNull != null)
		{
			Control nodeOrNull2 = ((Node)nodeOrNull).GetNodeOrNull<Control>(NodePath.op_Implicit("TickboxVisuals"));
			if (nodeOrNull2 != null)
			{
				SetTickState(nodeOrNull2, ticked);
			}
		}
	}

	private static void SetTickState(Control? visuals, bool ticked)
	{
		if (visuals != null)
		{
			Control nodeOrNull = ((Node)visuals).GetNodeOrNull<Control>(NodePath.op_Implicit("Ticked"));
			Control nodeOrNull2 = ((Node)visuals).GetNodeOrNull<Control>(NodePath.op_Implicit("NotTicked"));
			if (nodeOrNull != null)
			{
				((CanvasItem)nodeOrNull).Visible = ticked;
			}
			if (nodeOrNull2 != null)
			{
				((CanvasItem)nodeOrNull2).Visible = !ticked;
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Expected O, but got Unknown
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Expected O, but got Unknown
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Expected O, but got Unknown
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0577: Unknown result type (might be due to invalid IL or missing references)
		//IL_0582: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Expected O, but got Unknown
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_0629: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(21)
		{
			new MethodInfo(MethodName.Tr, new PropertyInfo((Type)4, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("key"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Create, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ApplyLocaleFontsToUi, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.InitializeFromScene, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.AddHoverScale, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("trigger"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)24, StringName.op_Implicit("target"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)3, StringName.op_Implicit("scale"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.HoverScaleTo, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("target"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)3, StringName.op_Implicit("scale"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateButtonAvailability, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("btn"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)1, StringName.op_Implicit("valid"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateMenuAvailability, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.Close, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnRightButtonReleased, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnLeftButtonReleased, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnEnchantTickClicked, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.SwitchToEnchant, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.SetCard, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("index"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.RefreshPanelState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.WireModeButton, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("nodePath"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)4, StringName.op_Implicit("mode"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ModeLocKey, new PropertyInfo((Type)4, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("mode"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateModeButtonStyles, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("editMode"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.SetModeButtonStyle, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("label"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Label"), false),
				new PropertyInfo((Type)1, StringName.op_Implicit("active"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.SetTickState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("nodePath"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)1, StringName.op_Implicit("ticked"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.Tr && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text = Tr(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text);
			return true;
		}
		if ((ref method) == MethodName.Create && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			NBalatroInspectScreen nBalatroInspectScreen = Create();
			ret = VariantUtils.CreateFrom<NBalatroInspectScreen>(ref nBalatroInspectScreen);
			return true;
		}
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplyLocaleFontsToUi && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ApplyLocaleFontsToUi();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.InitializeFromScene && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			InitializeFromScene();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.AddHoverScale && ((NativeVariantPtrArgs)(ref args)).Count == 3)
		{
			AddHoverScale(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[1]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[2]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.HoverScaleTo && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			HoverScaleTo(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdateButtonAvailability && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			UpdateButtonAvailability(VariantUtils.ConvertTo<NButton>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdateMenuAvailability && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			UpdateMenuAvailability();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Close && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnRightButtonReleased && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnRightButtonReleased();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnLeftButtonReleased && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnLeftButtonReleased();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnEnchantTickClicked && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnEnchantTickClicked();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SwitchToEnchant && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			SwitchToEnchant();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetCard && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			SetCard(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RefreshPanelState && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RefreshPanelState();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.WireModeButton && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			WireModeButton(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ModeLocKey && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text2 = ModeLocKey(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text2);
			return true;
		}
		if ((ref method) == MethodName.UpdateModeButtonStyles && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			UpdateModeButtonStyles(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetModeButtonStyle && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			SetModeButtonStyle(VariantUtils.ConvertTo<Label>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetTickState && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			SetTickState(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		return ((Control)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.Tr && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text = Tr(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text);
			return true;
		}
		if ((ref method) == MethodName.Create && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			NBalatroInspectScreen nBalatroInspectScreen = Create();
			ret = VariantUtils.CreateFrom<NBalatroInspectScreen>(ref nBalatroInspectScreen);
			return true;
		}
		if ((ref method) == MethodName.UpdateButtonAvailability && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			UpdateButtonAvailability(VariantUtils.ConvertTo<NButton>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ModeLocKey && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text2 = ModeLocKey(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text2);
			return true;
		}
		if ((ref method) == MethodName.SetModeButtonStyle && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			SetModeButtonStyle(VariantUtils.ConvertTo<Label>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName.Tr)
		{
			return true;
		}
		if ((ref method) == MethodName.Create)
		{
			return true;
		}
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplyLocaleFontsToUi)
		{
			return true;
		}
		if ((ref method) == MethodName.InitializeFromScene)
		{
			return true;
		}
		if ((ref method) == MethodName.AddHoverScale)
		{
			return true;
		}
		if ((ref method) == MethodName.HoverScaleTo)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateButtonAvailability)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateMenuAvailability)
		{
			return true;
		}
		if ((ref method) == MethodName.Close)
		{
			return true;
		}
		if ((ref method) == MethodName.OnRightButtonReleased)
		{
			return true;
		}
		if ((ref method) == MethodName.OnLeftButtonReleased)
		{
			return true;
		}
		if ((ref method) == MethodName.OnEnchantTickClicked)
		{
			return true;
		}
		if ((ref method) == MethodName.SwitchToEnchant)
		{
			return true;
		}
		if ((ref method) == MethodName.SetCard)
		{
			return true;
		}
		if ((ref method) == MethodName.RefreshPanelState)
		{
			return true;
		}
		if ((ref method) == MethodName.WireModeButton)
		{
			return true;
		}
		if ((ref method) == MethodName.ModeLocKey)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateModeButtonStyles)
		{
			return true;
		}
		if ((ref method) == MethodName.SetModeButtonStyle)
		{
			return true;
		}
		if ((ref method) == MethodName.SetTickState)
		{
			return true;
		}
		return ((Control)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._card)
		{
			_card = VariantUtils.ConvertTo<NCard>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<ColorRect>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._hoverTipRect)
		{
			_hoverTipRect = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._leftButton)
		{
			_leftButton = VariantUtils.ConvertTo<NButton>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._rightButton)
		{
			_rightButton = VariantUtils.ConvertTo<NButton>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._cardArea)
		{
			_cardArea = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._panelArea)
		{
			_panelArea = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._slider)
		{
			_slider = VariantUtils.ConvertTo<NSlider>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._exportBtn)
		{
			_exportBtn = VariantUtils.ConvertTo<NButton>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._importBtn)
		{
			_importBtn = VariantUtils.ConvertTo<NButton>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._menuBtn)
		{
			_menuBtn = VariantUtils.ConvertTo<NButton>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._menuPopup)
		{
			_menuPopup = VariantUtils.ConvertTo<PopupMenu>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._enchantTickbox)
		{
			_enchantTickbox = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._enchantTickVisuals)
		{
			_enchantTickVisuals = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._suppressOriginReturn)
		{
			_suppressOriginReturn = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._index)
		{
			_index = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._viewAllUpgraded)
		{
			_viewAllUpgraded = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._cardPosition)
		{
			_cardPosition = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._leftButtonX)
		{
			_leftButtonX = VariantUtils.ConvertTo<float>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._rightButtonX)
		{
			_rightButtonX = VariantUtils.ConvertTo<float>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._openTween)
		{
			_openTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._cardTween)
		{
			_cardTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._originInspect)
		{
			_originInspect = VariantUtils.ConvertTo<NInspectCardScreen>(ref value);
			return true;
		}
		return ((GodotObject)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName.DefaultFocusedControl)
		{
			Control defaultFocusedControl = DefaultFocusedControl;
			value = VariantUtils.CreateFrom<Control>(ref defaultFocusedControl);
			return true;
		}
		if ((ref name) == PropertyName._card)
		{
			value = VariantUtils.CreateFrom<NCard>(ref _card);
			return true;
		}
		if ((ref name) == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom<ColorRect>(ref _backstop);
			return true;
		}
		if ((ref name) == PropertyName._hoverTipRect)
		{
			value = VariantUtils.CreateFrom<Control>(ref _hoverTipRect);
			return true;
		}
		if ((ref name) == PropertyName._leftButton)
		{
			value = VariantUtils.CreateFrom<NButton>(ref _leftButton);
			return true;
		}
		if ((ref name) == PropertyName._rightButton)
		{
			value = VariantUtils.CreateFrom<NButton>(ref _rightButton);
			return true;
		}
		if ((ref name) == PropertyName._cardArea)
		{
			value = VariantUtils.CreateFrom<Control>(ref _cardArea);
			return true;
		}
		if ((ref name) == PropertyName._panelArea)
		{
			value = VariantUtils.CreateFrom<Control>(ref _panelArea);
			return true;
		}
		if ((ref name) == PropertyName._slider)
		{
			value = VariantUtils.CreateFrom<NSlider>(ref _slider);
			return true;
		}
		if ((ref name) == PropertyName._exportBtn)
		{
			value = VariantUtils.CreateFrom<NButton>(ref _exportBtn);
			return true;
		}
		if ((ref name) == PropertyName._importBtn)
		{
			value = VariantUtils.CreateFrom<NButton>(ref _importBtn);
			return true;
		}
		if ((ref name) == PropertyName._menuBtn)
		{
			value = VariantUtils.CreateFrom<NButton>(ref _menuBtn);
			return true;
		}
		if ((ref name) == PropertyName._menuPopup)
		{
			value = VariantUtils.CreateFrom<PopupMenu>(ref _menuPopup);
			return true;
		}
		if ((ref name) == PropertyName._enchantTickbox)
		{
			value = VariantUtils.CreateFrom<Control>(ref _enchantTickbox);
			return true;
		}
		if ((ref name) == PropertyName._enchantTickVisuals)
		{
			value = VariantUtils.CreateFrom<Control>(ref _enchantTickVisuals);
			return true;
		}
		if ((ref name) == PropertyName._suppressOriginReturn)
		{
			value = VariantUtils.CreateFrom<bool>(ref _suppressOriginReturn);
			return true;
		}
		if ((ref name) == PropertyName._index)
		{
			value = VariantUtils.CreateFrom<int>(ref _index);
			return true;
		}
		if ((ref name) == PropertyName._viewAllUpgraded)
		{
			value = VariantUtils.CreateFrom<bool>(ref _viewAllUpgraded);
			return true;
		}
		if ((ref name) == PropertyName._cardPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _cardPosition);
			return true;
		}
		if ((ref name) == PropertyName._leftButtonX)
		{
			value = VariantUtils.CreateFrom<float>(ref _leftButtonX);
			return true;
		}
		if ((ref name) == PropertyName._rightButtonX)
		{
			value = VariantUtils.CreateFrom<float>(ref _rightButtonX);
			return true;
		}
		if ((ref name) == PropertyName._openTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _openTween);
			return true;
		}
		if ((ref name) == PropertyName._cardTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _cardTween);
			return true;
		}
		if ((ref name) == PropertyName._originInspect)
		{
			value = VariantUtils.CreateFrom<NInspectCardScreen>(ref _originInspect);
			return true;
		}
		return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._card, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._backstop, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._hoverTipRect, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._leftButton, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._rightButton, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._cardArea, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._panelArea, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._slider, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._exportBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._importBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._menuBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._menuPopup, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._enchantTickbox, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._enchantTickVisuals, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._suppressOriginReturn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._index, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._viewAllUpgraded, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._cardPosition, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)3, PropertyName._leftButtonX, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)3, PropertyName._rightButtonX, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._openTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._cardTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._originInspect, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName.DefaultFocusedControl, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._card, Variant.From<NCard>(ref _card));
		info.AddProperty(PropertyName._backstop, Variant.From<ColorRect>(ref _backstop));
		info.AddProperty(PropertyName._hoverTipRect, Variant.From<Control>(ref _hoverTipRect));
		info.AddProperty(PropertyName._leftButton, Variant.From<NButton>(ref _leftButton));
		info.AddProperty(PropertyName._rightButton, Variant.From<NButton>(ref _rightButton));
		info.AddProperty(PropertyName._cardArea, Variant.From<Control>(ref _cardArea));
		info.AddProperty(PropertyName._panelArea, Variant.From<Control>(ref _panelArea));
		info.AddProperty(PropertyName._slider, Variant.From<NSlider>(ref _slider));
		info.AddProperty(PropertyName._exportBtn, Variant.From<NButton>(ref _exportBtn));
		info.AddProperty(PropertyName._importBtn, Variant.From<NButton>(ref _importBtn));
		info.AddProperty(PropertyName._menuBtn, Variant.From<NButton>(ref _menuBtn));
		info.AddProperty(PropertyName._menuPopup, Variant.From<PopupMenu>(ref _menuPopup));
		info.AddProperty(PropertyName._enchantTickbox, Variant.From<Control>(ref _enchantTickbox));
		info.AddProperty(PropertyName._enchantTickVisuals, Variant.From<Control>(ref _enchantTickVisuals));
		info.AddProperty(PropertyName._suppressOriginReturn, Variant.From<bool>(ref _suppressOriginReturn));
		info.AddProperty(PropertyName._index, Variant.From<int>(ref _index));
		info.AddProperty(PropertyName._viewAllUpgraded, Variant.From<bool>(ref _viewAllUpgraded));
		info.AddProperty(PropertyName._cardPosition, Variant.From<Vector2>(ref _cardPosition));
		info.AddProperty(PropertyName._leftButtonX, Variant.From<float>(ref _leftButtonX));
		info.AddProperty(PropertyName._rightButtonX, Variant.From<float>(ref _rightButtonX));
		info.AddProperty(PropertyName._openTween, Variant.From<Tween>(ref _openTween));
		info.AddProperty(PropertyName._cardTween, Variant.From<Tween>(ref _cardTween));
		info.AddProperty(PropertyName._originInspect, Variant.From<NInspectCardScreen>(ref _originInspect));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._card, ref val))
		{
			_card = ((Variant)(ref val)).As<NCard>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._backstop, ref val2))
		{
			_backstop = ((Variant)(ref val2)).As<ColorRect>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._hoverTipRect, ref val3))
		{
			_hoverTipRect = ((Variant)(ref val3)).As<Control>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._leftButton, ref val4))
		{
			_leftButton = ((Variant)(ref val4)).As<NButton>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._rightButton, ref val5))
		{
			_rightButton = ((Variant)(ref val5)).As<NButton>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._cardArea, ref val6))
		{
			_cardArea = ((Variant)(ref val6)).As<Control>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._panelArea, ref val7))
		{
			_panelArea = ((Variant)(ref val7)).As<Control>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._slider, ref val8))
		{
			_slider = ((Variant)(ref val8)).As<NSlider>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._exportBtn, ref val9))
		{
			_exportBtn = ((Variant)(ref val9)).As<NButton>();
		}
		Variant val10 = default(Variant);
		if (info.TryGetProperty(PropertyName._importBtn, ref val10))
		{
			_importBtn = ((Variant)(ref val10)).As<NButton>();
		}
		Variant val11 = default(Variant);
		if (info.TryGetProperty(PropertyName._menuBtn, ref val11))
		{
			_menuBtn = ((Variant)(ref val11)).As<NButton>();
		}
		Variant val12 = default(Variant);
		if (info.TryGetProperty(PropertyName._menuPopup, ref val12))
		{
			_menuPopup = ((Variant)(ref val12)).As<PopupMenu>();
		}
		Variant val13 = default(Variant);
		if (info.TryGetProperty(PropertyName._enchantTickbox, ref val13))
		{
			_enchantTickbox = ((Variant)(ref val13)).As<Control>();
		}
		Variant val14 = default(Variant);
		if (info.TryGetProperty(PropertyName._enchantTickVisuals, ref val14))
		{
			_enchantTickVisuals = ((Variant)(ref val14)).As<Control>();
		}
		Variant val15 = default(Variant);
		if (info.TryGetProperty(PropertyName._suppressOriginReturn, ref val15))
		{
			_suppressOriginReturn = ((Variant)(ref val15)).As<bool>();
		}
		Variant val16 = default(Variant);
		if (info.TryGetProperty(PropertyName._index, ref val16))
		{
			_index = ((Variant)(ref val16)).As<int>();
		}
		Variant val17 = default(Variant);
		if (info.TryGetProperty(PropertyName._viewAllUpgraded, ref val17))
		{
			_viewAllUpgraded = ((Variant)(ref val17)).As<bool>();
		}
		Variant val18 = default(Variant);
		if (info.TryGetProperty(PropertyName._cardPosition, ref val18))
		{
			_cardPosition = ((Variant)(ref val18)).As<Vector2>();
		}
		Variant val19 = default(Variant);
		if (info.TryGetProperty(PropertyName._leftButtonX, ref val19))
		{
			_leftButtonX = ((Variant)(ref val19)).As<float>();
		}
		Variant val20 = default(Variant);
		if (info.TryGetProperty(PropertyName._rightButtonX, ref val20))
		{
			_rightButtonX = ((Variant)(ref val20)).As<float>();
		}
		Variant val21 = default(Variant);
		if (info.TryGetProperty(PropertyName._openTween, ref val21))
		{
			_openTween = ((Variant)(ref val21)).As<Tween>();
		}
		Variant val22 = default(Variant);
		if (info.TryGetProperty(PropertyName._cardTween, ref val22))
		{
			_cardTween = ((Variant)(ref val22)).As<Tween>();
		}
		Variant val23 = default(Variant);
		if (info.TryGetProperty(PropertyName._originInspect, ref val23))
		{
			_originInspect = ((Variant)(ref val23)).As<NInspectCardScreen>();
		}
	}
}
