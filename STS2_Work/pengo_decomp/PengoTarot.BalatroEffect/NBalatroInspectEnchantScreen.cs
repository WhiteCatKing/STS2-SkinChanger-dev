using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
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

[ScriptPath("res://balatroeffect/Scenes/NBalatroInspectEnchantScreen.cs")]
public class NBalatroInspectEnchantScreen : Control, IScreenContext
{
	private sealed class EnchantSlot
	{
		public Control Root;

		public TextureRect Icon;

		public int SlotIndex;
	}

	public class MethodName : MethodName
	{
		public static readonly StringName Tr = StringName.op_Implicit("Tr");

		public static readonly StringName Create = StringName.op_Implicit("Create");

		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName ApplyLocaleFontsToUi = StringName.op_Implicit("ApplyLocaleFontsToUi");

		public static readonly StringName InitializeFromScene = StringName.op_Implicit("InitializeFromScene");

		public static readonly StringName AddHoverScale = StringName.op_Implicit("AddHoverScale");

		public static readonly StringName HoverScaleTo = StringName.op_Implicit("HoverScaleTo");

		public static readonly StringName Close = StringName.op_Implicit("Close");

		public static readonly StringName ReturnToCardScreen = StringName.op_Implicit("ReturnToCardScreen");

		public static readonly StringName BuildEnchantList = StringName.op_Implicit("BuildEnchantList");

		public static readonly StringName SetEnchantment = StringName.op_Implicit("SetEnchantment");

		public static readonly StringName RemoveEnchantmentAt = StringName.op_Implicit("RemoveEnchantmentAt");

		public static readonly StringName UpdateArrowVisibility = StringName.op_Implicit("UpdateArrowVisibility");

		public static readonly StringName WrapIndex = StringName.op_Implicit("WrapIndex");

		public static readonly StringName InitializeEnchantList = StringName.op_Implicit("InitializeEnchantList");

		public static readonly StringName OnListSlotClicked = StringName.op_Implicit("OnListSlotClicked");

		public static readonly StringName RefreshEnchantList = StringName.op_Implicit("RefreshEnchantList");

		public static readonly StringName TryShowEnchantment = StringName.op_Implicit("TryShowEnchantment");

		public static readonly StringName OnRightButtonReleased = StringName.op_Implicit("OnRightButtonReleased");

		public static readonly StringName OnLeftButtonReleased = StringName.op_Implicit("OnLeftButtonReleased");

		public static readonly StringName PlayUiClickSfx = StringName.op_Implicit("PlayUiClickSfx");

		public static readonly StringName OnEnchantTickClicked = StringName.op_Implicit("OnEnchantTickClicked");

		public static readonly StringName OnEnchantMenuAction = StringName.op_Implicit("OnEnchantMenuAction");

		public static readonly StringName UpdateEnchantMenuAvailability = StringName.op_Implicit("UpdateEnchantMenuAvailability");

		public static readonly StringName RefreshPanelState = StringName.op_Implicit("RefreshPanelState");

		public static readonly StringName UpdateButtonAvailability = StringName.op_Implicit("UpdateButtonAvailability");

		public static readonly StringName OnPartClicked = StringName.op_Implicit("OnPartClicked");

		public static readonly StringName OnEditModeSelected = StringName.op_Implicit("OnEditModeSelected");

		public static readonly StringName OnCopyCard = StringName.op_Implicit("OnCopyCard");

		public static readonly StringName OnPasteCard = StringName.op_Implicit("OnPasteCard");

		public static readonly StringName InitPaginator = StringName.op_Implicit("InitPaginator");

		public static readonly StringName UpdatePaginatorTarget = StringName.op_Implicit("UpdatePaginatorTarget");

		public static readonly StringName SetPaginatorToMode = StringName.op_Implicit("SetPaginatorToMode");

		public static readonly StringName OnPaginatorNavigate = StringName.op_Implicit("OnPaginatorNavigate");

		public static readonly StringName UpdatePaginatorLabel = StringName.op_Implicit("UpdatePaginatorLabel");

		public static readonly StringName ApplyCurrentEffectToCheckedParts = StringName.op_Implicit("ApplyCurrentEffectToCheckedParts");

		public static readonly StringName WireModeButton = StringName.op_Implicit("WireModeButton");

		public static readonly StringName ModeLocKey = StringName.op_Implicit("ModeLocKey");

		public static readonly StringName UpdateModeButtonStyles = StringName.op_Implicit("UpdateModeButtonStyles");

		public static readonly StringName SetModeButtonStyle = StringName.op_Implicit("SetModeButtonStyle");

		public static readonly StringName SetTickState = StringName.op_Implicit("SetTickState");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName CurrentEnchantmentId = StringName.op_Implicit("CurrentEnchantmentId");

		public static readonly StringName DefaultFocusedControl = StringName.op_Implicit("DefaultFocusedControl");

		public static readonly StringName _card = StringName.op_Implicit("_card");

		public static readonly StringName _backstop = StringName.op_Implicit("_backstop");

		public static readonly StringName _hoverTipRect = StringName.op_Implicit("_hoverTipRect");

		public static readonly StringName _leftButton = StringName.op_Implicit("_leftButton");

		public static readonly StringName _rightButton = StringName.op_Implicit("_rightButton");

		public static readonly StringName _slider = StringName.op_Implicit("_slider");

		public static readonly StringName _exportBtn = StringName.op_Implicit("_exportBtn");

		public static readonly StringName _importBtn = StringName.op_Implicit("_importBtn");

		public static readonly StringName _menuBtn = StringName.op_Implicit("_menuBtn");

		public static readonly StringName _menuPopup = StringName.op_Implicit("_menuPopup");

		public static readonly StringName _enchantTickbox = StringName.op_Implicit("_enchantTickbox");

		public static readonly StringName _enchantTickVisuals = StringName.op_Implicit("_enchantTickVisuals");

		public static readonly StringName _enchantNameLabel = StringName.op_Implicit("_enchantNameLabel");

		public static readonly StringName _index = StringName.op_Implicit("_index");

		public static readonly StringName _separatelyHighlight = StringName.op_Implicit("_separatelyHighlight");

		public static readonly StringName _returnIndex = StringName.op_Implicit("_returnIndex");

		public static readonly StringName _returnViewAllUpgraded = StringName.op_Implicit("_returnViewAllUpgraded");

		public static readonly StringName _returnOriginInspect = StringName.op_Implicit("_returnOriginInspect");

		public static readonly StringName _cardPosition = StringName.op_Implicit("_cardPosition");

		public static readonly StringName _leftButtonY = StringName.op_Implicit("_leftButtonY");

		public static readonly StringName _rightButtonY = StringName.op_Implicit("_rightButtonY");

		public static readonly StringName _enchantList = StringName.op_Implicit("_enchantList");

		public static readonly StringName _pgCurrentIndex = StringName.op_Implicit("_pgCurrentIndex");

		public static readonly StringName _pgLabel = StringName.op_Implicit("_pgLabel");
	}

	public class SignalName : SignalName
	{
	}

	private const string SliderName = "BalatroEffectsSlider";

	private static readonly string CardScenePath = "res://scenes/cards/card.tscn";

	private static Font? _boldFont;

	private static Font? _regularFont;

	private NCard _card;

	private ColorRect _backstop;

	private Control _hoverTipRect;

	private NButton _leftButton;

	private NButton _rightButton;

	private NSlider _slider;

	private NButton _exportBtn;

	private NButton _importBtn;

	private NButton _menuBtn;

	private PopupMenu _menuPopup;

	private Control _enchantTickbox;

	private Control _enchantTickVisuals;

	private Label _enchantNameLabel;

	private readonly Dictionary<Control, Tween> _hoverTweens = new Dictionary<Control, Tween>();

	private List<EnchantmentModel> _enchantments = new List<EnchantmentModel>();

	private int _index = -1;

	private string _separatelyHighlight = "";

	private readonly Dictionary<string, CardModel> _displayCardCache = new Dictionary<string, CardModel>();

	private List<CardModel>? _returnCards;

	private int _returnIndex;

	private bool _returnViewAllUpgraded;

	private NInspectCardScreen? _returnOriginInspect;

	private Vector2 _cardPosition;

	private float _leftButtonY;

	private float _rightButtonY;

	private const int ListSlotCount = 9;

	private const int ListCenterSlot = 4;

	private static readonly Vector2 ListSlotSize = new Vector2(55f, 55f);

	private VBoxContainer _enchantList;

	private readonly List<EnchantSlot> _enchantSlots = new List<EnchantSlot>();

	private static List<EffectRegistry.EffectDef> _pgEffects = new List<EffectRegistry.EffectDef>();

	private static bool _pgInitialized;

	private int _pgCurrentIndex;

	private Label? _pgLabel;

	private static readonly HashSet<string> _loggedSkippedEnchantments = new HashSet<string>();

	private static readonly (string, int)[] TarotMajorOrder = new(string, int)[20]
	{
		("Fool", 0),
		("Magician", 1),
		("HighPriestess", 2),
		("Empress", 3),
		("Emperor", 4),
		("Hierophant", 5),
		("Lovers", 6),
		("Chariot", 7),
		("Strength", 8),
		("Hermit", 9),
		("Justice", 11),
		("HangedMan", 12),
		("Death", 13),
		("Temperance", 14),
		("Devil", 15),
		("Star", 17),
		("Moon", 18),
		("Sun", 19),
		("Judgement", 20),
		("World", 21)
	};

	private static readonly (string, int)[] SubOrder = new(string, int)[5]
	{
		("Devil", 0),
		("Star", 1),
		("Moon", 2),
		("Sun", 3),
		("World", 4)
	};

	private static readonly (string, int)[] PlanetOrder = new(string, int)[12]
	{
		("Mercury", 0),
		("Venus", 1),
		("Earth", 2),
		("Mars", 3),
		("Jupiter", 4),
		("Saturn", 5),
		("Uranus", 6),
		("Neptune", 7),
		("Pluto", 8),
		("X", 9),
		("Ceres", 10),
		("Eris", 11)
	};

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

	private string CurrentEnchantmentId
	{
		get
		{
			if (_index < 0 || _index >= _enchantments.Count)
			{
				return "";
			}
			return ((object)((AbstractModel)_enchantments[_index]).Id).ToString();
		}
	}

	public Control? DefaultFocusedControl => null;

	private static string Tr(string key)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;
	}

	public static NBalatroInspectEnchantScreen? Create()
	{
		return GD.Load<PackedScene>("res://balatroeffect/Scenes/balatro_inspect_enchant_screen.tscn").Instantiate<NBalatroInspectEnchantScreen>((GenEditState)0);
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
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Expected O, but got Unknown
		//IL_05bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_0639: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Expected O, but got Unknown
		//IL_06fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0736: Unknown result type (might be due to invalid IL or missing references)
		//IL_073c: Unknown result type (might be due to invalid IL or missing references)
		//IL_078d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0793: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		_backstop = ((Node)this).GetNode<ColorRect>(NodePath.op_Implicit("Backstop"));
		((GodotObject)((Node)this).GetNode<NButton>(NodePath.op_Implicit("BackstopHit"))).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			Close();
		}), 0u);
		Control node = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%CardAnchor"));
		PackedScene val = GD.Load<PackedScene>(CardScenePath);
		_card = val.Instantiate<NCard>((GenEditState)0);
		((Node)_card).Name = StringName.op_Implicit("Card");
		((Control)_card).SetAnchorsPreset((LayoutPreset)8, false);
		((Control)_card).Scale = Vector2.One * 2f;
		_cardPosition = ((Control)_card).Position;
		((Node)node).AddChild((Node)(object)_card, false, (InternalMode)0);
		_hoverTipRect = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%HoverTipRect"));
		_leftButton = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%LeftArrow"));
		_leftButtonY = ((Control)_leftButton).Position.Y;
		((GodotObject)_leftButton).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnLeftButtonReleased();
		}), 0u);
		AddHoverScale((Control)(object)_leftButton, (Control)(object)_leftButton);
		_rightButton = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%RightArrow"));
		_rightButtonY = ((Control)_rightButton).Position.Y;
		((GodotObject)_rightButton).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnRightButtonReleased();
		}), 0u);
		AddHoverScale((Control)(object)_rightButton, (Control)(object)_rightButton);
		InitializeEnchantList();
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
		SetTickState(_enchantTickVisuals, ticked: true);
		_enchantNameLabel = ((Node)this).GetNode<Label>(NodePath.op_Implicit("%EnchantNameLabel"));
		_enchantNameLabel.Text = "";
		((CanvasItem)_enchantNameLabel).Visible = false;
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
			((Control)val3).MouseFilter = (MouseFilterEnum)0;
			((GodotObject)val3).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent evt)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I8
				InputEventMouseButton val6 = (InputEventMouseButton)(object)((evt is InputEventMouseButton) ? evt : null);
				if (val6 != null && (long)val6.ButtonIndex == 1 && val6.Pressed)
				{
					OnPartClicked(partName);
				}
			}), 0u);
			val4.MouseFilter = (MouseFilterEnum)2;
			if (nodeOrNull != null)
			{
				((Control)nodeOrNull).MouseFilter = (MouseFilterEnum)2;
			}
			Control nodeOrNull2 = ((Node)val4).GetNodeOrNull<Control>(NodePath.op_Implicit("TickboxVisuals"));
			if (nodeOrNull2 != null)
			{
				AddHoverScale((Control)(object)val3, nodeOrNull2, 1.15f);
			}
		}
		_slider = ((Node)this).GetNode<NSlider>(NodePath.op_Implicit("%SliderCenter/Slider"));
		((Node)_slider).Name = StringName.op_Implicit("BalatroEffectsSlider");
		((Range)_slider).ValueChanged += (ValueChangedEventHandler)delegate(double v)
		{
			string currentEnchantmentId = CurrentEnchantmentId;
			if (currentEnchantmentId != "")
			{
				EnchantmentConfig.SetIntensity(currentEnchantmentId, (double)(float)v / 100.0);
			}
		};
		Control nodeOrNull3 = ((Node)_slider).GetNodeOrNull<Control>(NodePath.op_Implicit("%Handle"));
		if (nodeOrNull3 != null)
		{
			AddHoverScale((Control)(object)_slider, nodeOrNull3, 1.15f);
		}
		_exportBtn = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%ExportBtn"));
		((Node)_exportBtn).GetNode<Label>(NodePath.op_Implicit("Label")).Text = Tr("BTN_COPY_CARD");
		((GodotObject)_exportBtn).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnCopyCard();
		}), 0u);
		AddHoverScale((Control)(object)_exportBtn, (Control)(object)_exportBtn);
		_importBtn = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%ImportBtn"));
		((Node)_importBtn).GetNode<Label>(NodePath.op_Implicit("Label")).Text = Tr("BTN_PASTE_CARD");
		((GodotObject)_importBtn).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnPasteCard();
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
		_menuPopup.AddItem(Tr("MENU_CLEAR_ENCHANT"), 0, (Key)0);
		((GodotObject)_menuPopup).Connect(SignalName.IdPressed, Callable.From<int>((Action<int>)OnEnchantMenuAction), 0u);
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
			UpdateEnchantMenuAvailability();
			Vector2 globalPosition = ((Control)_menuBtn).GlobalPosition;
			Vector2 size = ((Control)_menuBtn).Size;
			((Window)_menuPopup).Popup((Rect2I?)new Rect2I((int)globalPosition.X, (int)(globalPosition.Y + size.Y), 0, 0));
		}), 0u);
		NButton node8 = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%PgLeftBtn"));
		NButton node9 = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("%PgRightBtn"));
		Label node10 = ((Node)this).GetNode<Label>(NodePath.op_Implicit("%PgLabel"));
		((GodotObject)node8).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnPaginatorNavigate(-1);
		}), 0u);
		((GodotObject)node9).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			OnPaginatorNavigate(1);
		}), 0u);
		AddHoverScale((Control)(object)node8, (Control)(object)node8);
		AddHoverScale((Control)(object)node9, (Control)(object)node9);
		InitPaginator(node10);
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

	public void Open(List<CardModel>? returnCards, int returnIndex, bool returnViewAllUpgraded, NInspectCardScreen? returnOriginInspect)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		_returnCards = returnCards;
		_returnIndex = returnIndex;
		_returnViewAllUpgraded = returnViewAllUpgraded;
		_returnOriginInspect = returnOriginInspect;
		BuildEnchantList();
		if (_separatelyHighlight == "")
		{
			_separatelyHighlight = ((Config.AllPartNames.Length != 0) ? Config.AllPartNames[0] : "");
		}
		((CanvasItem)this).Visible = true;
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		SetEnchantment(0);
		((Control)_card).Scale = Vector2.One * 1.75f;
		((CanvasItem)_card).Modulate = StsColors.transparentBlack;
		((CanvasItem)_leftButton).Modulate = StsColors.transparentBlack;
		((CanvasItem)_rightButton).Modulate = StsColors.transparentBlack;
		Tween obj = ((Node)this).CreateTween().SetParallel(true);
		obj.TweenProperty((GodotObject)(object)_backstop, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.9f), 0.25);
		obj.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(1f), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)5)
			.From(Variant.op_Implicit(0f));
		obj.TweenProperty((GodotObject)(object)_leftButton, NodePath.op_Implicit("position:y"), Variant.op_Implicit(_leftButtonY), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)10)
			.From(Variant.op_Implicit(_leftButtonY - 100f))
			.SetDelay(0.1);
		obj.TweenProperty((GodotObject)(object)_leftButton, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.25).SetDelay(0.1);
		obj.TweenProperty((GodotObject)(object)_rightButton, NodePath.op_Implicit("position:y"), Variant.op_Implicit(_rightButtonY), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)10)
			.From(Variant.op_Implicit(_rightButtonY + 100f))
			.SetDelay(0.1);
		obj.TweenProperty((GodotObject)(object)_rightButton, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.25).SetDelay(0.1);
		Tween obj2 = ((Node)this).CreateTween().SetParallel(true);
		obj2.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.25);
		obj2.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * 2f), 0.15).SetEase((EaseType)1).SetTrans((TransitionType)11)
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
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		if (((CanvasItem)this).Visible)
		{
			((Control)this).MouseFilter = (MouseFilterEnum)2;
			((Control)_leftButton).MouseFilter = (MouseFilterEnum)2;
			((Control)_rightButton).MouseFilter = (MouseFilterEnum)2;
			((NClickableControl)_rightButton).Disable();
			((NClickableControl)_leftButton).Disable();
			NHoverTipSet.Clear();
			ReturnToCardScreen();
			Tween obj = ((Node)this).CreateTween().SetParallel(true);
			obj.TweenProperty((GodotObject)(object)_backstop, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.25);
			obj.TweenProperty((GodotObject)(object)_leftButton, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.1);
			obj.TweenProperty((GodotObject)(object)_rightButton, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.1);
			obj.TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("modulate"), Variant.op_Implicit(StsColors.transparentWhite), 0.1);
			obj.Chain().TweenCallback(Callable.From((Action)delegate
			{
				((CanvasItem)this).Visible = false;
				ActiveScreenContext.Instance.Update();
				((Node)this).QueueFree();
			}));
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.cancel), (Action)Close);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.pauseAndBack), (Action)Close);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.left), (Action)OnLeftButtonReleased);
			NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.right), (Action)OnRightButtonReleased);
			NHotkeyManager.Instance.RemoveBlockingScreen((Node)(object)this);
		}
	}

	private void ReturnToCardScreen()
	{
		Config.ClearAllEnchantOverlays();
		List<CardModel> returnCards = _returnCards;
		if (returnCards == null || returnCards.Count <= 0)
		{
			return;
		}
		NBalatroInspectScreen nBalatroInspectScreen = NBalatroInspectScreen.Create();
		if (nBalatroInspectScreen != null)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
			if (obj != null)
			{
				((Node)((SceneTree)obj).Root).AddChild((Node)(object)nBalatroInspectScreen, false, (InternalMode)0);
			}
			nBalatroInspectScreen.Open(_returnCards, _returnIndex, _returnViewAllUpgraded, _returnOriginInspect);
		}
	}

	private void BuildEnchantList()
	{
		_enchantments.Clear();
		_displayCardCache.Clear();
		List<EnchantmentModel> list = new List<EnchantmentModel>();
		foreach (EnchantmentModel debugEnchantment in ModelDb.DebugEnchantments)
		{
			if ((((object)debugEnchantment).GetType().Namespace ?? "").Contains(".Mocks", StringComparison.Ordinal))
			{
				continue;
			}
			CardModel val = FindDisplayCard(debugEnchantment);
			if (val == null)
			{
				string text = ((object)((AbstractModel)debugEnchantment).Id).ToString();
				if (_loggedSkippedEnchantments.Add(text))
				{
					GD.Print("[PengoTarot] 附魔 " + text + " 被跳过：找不到任何可附魔的卡牌");
				}
			}
			else
			{
				list.Add(debugEnchantment);
				_displayCardCache[((object)((AbstractModel)debugEnchantment).Id).ToString()] = val;
			}
		}
		_enchantments = (from t in list.Select((EnchantmentModel e, int idx) => (e: e, idx: idx))
			orderby GetPengoTarotOrderKey(t.e), t.idx
			select t.e).ToList();
	}

	private static int GetPengoTarotOrderKey(EnchantmentModel ench)
	{
		if (!(((object)ench).GetType().Namespace ?? "").StartsWith("PengoTarot.", StringComparison.Ordinal))
		{
			return 0;
		}
		string name = ((object)ench).GetType().Name;
		(string, int)[] subOrder;
		if (name.Contains("Sub", StringComparison.Ordinal))
		{
			subOrder = SubOrder;
			for (int i = 0; i < subOrder.Length; i++)
			{
				var (text, num) = subOrder[i];
				if (name.StartsWith("Tar" + text, StringComparison.Ordinal))
				{
					return 2000000 + num * 2 + (name.Contains("Reversed", StringComparison.Ordinal) ? 1 : 0);
				}
			}
			return 4000000;
		}
		subOrder = TarotMajorOrder;
		for (int i = 0; i < subOrder.Length; i++)
		{
			var (text2, num2) = subOrder[i];
			if (name.StartsWith("Tar" + text2, StringComparison.Ordinal))
			{
				return 1000000 + num2 * 2 + (name.Contains("Reversed", StringComparison.Ordinal) ? 1 : 0);
			}
		}
		subOrder = PlanetOrder;
		for (int i = 0; i < subOrder.Length; i++)
		{
			var (text3, num3) = subOrder[i];
			if (name == "Planet" + text3 + "Enchantment")
			{
				return 3000000 + num3;
			}
		}
		return 4000000;
	}

	private static bool SafeCanEnchant(EnchantmentModel ench, CardModel card)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		if (!((AbstractModel)card).IsCanonical)
		{
			return ench.CanEnchant(card);
		}
		CardModel val = (CardModel)((AbstractModel)card).MutableClone();
		return ench.CanEnchant(val);
	}

	private CardModel? FindDisplayCard(EnchantmentModel ench)
	{
		CardModel val = FindNearestDisplayCard(ench);
		if (val != null)
		{
			return val;
		}
		foreach (CardModel allCard in ModelDb.AllCards)
		{
			if (SafeCanEnchant(ench, allCard))
			{
				return allCard;
			}
		}
		return null;
	}

	private CardModel? FindNearestDisplayCard(EnchantmentModel ench)
	{
		List<CardModel> returnCards = _returnCards;
		if (returnCards == null || returnCards.Count <= 0)
		{
			return null;
		}
		int count = _returnCards.Count;
		int num = Math.Clamp(_returnIndex, 0, count - 1);
		for (int i = 0; i < count; i++)
		{
			if (i == 0)
			{
				if (SafeCanEnchant(ench, _returnCards[num]))
				{
					return _returnCards[num];
				}
				continue;
			}
			int num2 = num + i;
			if (num2 < count && SafeCanEnchant(ench, _returnCards[num2]))
			{
				return _returnCards[num2];
			}
			int num3 = num - i;
			if (num3 >= 0 && SafeCanEnchant(ench, _returnCards[num3]))
			{
				return _returnCards[num3];
			}
		}
		return null;
	}

	private void SetEnchantment(int index)
	{
		if (_enchantments.Count == 0)
		{
			_index = -1;
			((CanvasItem)_leftButton).Visible = false;
			((Control)_leftButton).MouseFilter = (MouseFilterEnum)2;
			((CanvasItem)_rightButton).Visible = false;
			((Control)_rightButton).MouseFilter = (MouseFilterEnum)2;
			if (_enchantNameLabel != null)
			{
				((CanvasItem)_enchantNameLabel).Visible = false;
			}
			return;
		}
		_index = WrapIndex(index);
		for (int i = 0; i < _enchantments.Count; i++)
		{
			if (TryShowEnchantment(_index))
			{
				((CanvasItem)_card).Visible = true;
				UpdateArrowVisibility();
				RefreshEnchantList();
				return;
			}
			RemoveEnchantmentAt(_index);
			if (_enchantments.Count == 0)
			{
				break;
			}
			if (_index >= _enchantments.Count)
			{
				_index = 0;
			}
		}
		_index = -1;
		((CanvasItem)_card).Visible = false;
		if (_enchantNameLabel != null)
		{
			((CanvasItem)_enchantNameLabel).Visible = false;
		}
		((CanvasItem)_leftButton).Visible = false;
		((Control)_leftButton).MouseFilter = (MouseFilterEnum)2;
		((CanvasItem)_rightButton).Visible = false;
		((Control)_rightButton).MouseFilter = (MouseFilterEnum)2;
	}

	private void RemoveEnchantmentAt(int idx)
	{
		if (idx >= 0 && idx < _enchantments.Count)
		{
			string key = ((object)((AbstractModel)_enchantments[idx]).Id).ToString();
			_enchantments.RemoveAt(idx);
			_displayCardCache.Remove(key);
		}
	}

	private void UpdateArrowVisibility()
	{
		bool flag = _enchantments.Count > 1;
		((CanvasItem)_leftButton).Visible = flag;
		((Control)_leftButton).MouseFilter = (MouseFilterEnum)(flag ? 0 : 2);
		((CanvasItem)_rightButton).Visible = flag;
		((Control)_rightButton).MouseFilter = (MouseFilterEnum)(flag ? 0 : 2);
	}

	private int WrapIndex(int i)
	{
		if (_enchantments.Count == 0)
		{
			return 0;
		}
		return (i % _enchantments.Count + _enchantments.Count) % _enchantments.Count;
	}

	private void InitializeEnchantList()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		_enchantList = ((Node)this).GetNode<VBoxContainer>(NodePath.op_Implicit("%EnchantList"));
		for (int i = 0; i < 9; i++)
		{
			Control val = new Control
			{
				Name = StringName.op_Implicit("Slot" + i)
			};
			val.CustomMinimumSize = ListSlotSize;
			val.MouseFilter = (MouseFilterEnum)0;
			val.PivotOffset = ListSlotSize / 2f;
			TextureRect val2 = new TextureRect
			{
				Name = StringName.op_Implicit("Icon")
			};
			((Control)val2).SetAnchorsPreset((LayoutPreset)15, false);
			((Control)val2).MouseFilter = (MouseFilterEnum)2;
			val2.ExpandMode = (ExpandModeEnum)1;
			val2.StretchMode = (StretchModeEnum)5;
			((CanvasItem)val2).TextureFilter = (TextureFilterEnum)2;
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			EnchantSlot slot = new EnchantSlot
			{
				Root = val,
				Icon = val2,
				SlotIndex = i
			};
			int slotIdx = i;
			val.GuiInput += (GuiInputEventHandler)delegate(InputEvent evt)
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I8
				InputEventMouseButton val3 = (InputEventMouseButton)(object)((evt is InputEventMouseButton) ? evt : null);
				if (val3 != null && (long)val3.ButtonIndex == 1 && val3.Pressed)
				{
					OnListSlotClicked(slotIdx);
				}
			};
			val.MouseEntered += delegate
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				((CanvasItem)slot.Root).Modulate = Colors.White;
				HoverScaleTo(slot.Root, 1.12f);
			};
			val.MouseExited += delegate
			{
				if (_hoverTweens.TryGetValue(slot.Root, out Tween value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
				{
					value.Kill();
					_hoverTweens.Remove(slot.Root);
				}
				RefreshEnchantList();
			};
			((Node)_enchantList).AddChild((Node)(object)val, false, (InternalMode)0);
			_enchantSlots.Add(slot);
		}
	}

	private void OnListSlotClicked(int slotIndex)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (_enchantments.Count != 0)
		{
			int num = WrapIndex(_index - 4 + slotIndex);
			if (num != _index)
			{
				SetEnchantment(num);
				((CanvasItem)_card).Modulate = Colors.White;
				((Control)_card).Position = _cardPosition;
				float num2 = ((slotIndex < 4) ? (-120f) : 120f);
				((Node)this).CreateTween().SetParallel(true).TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("position"), Variant.op_Implicit(_cardPosition), 0.25)
					.SetEase((EaseType)1)
					.SetTrans((TransitionType)5)
					.From(Variant.op_Implicit(_cardPosition + new Vector2(0f, num2)));
				PlayUiClickSfx();
			}
		}
	}

	private void RefreshEnchantList()
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (_enchantSlots.Count == 0)
		{
			return;
		}
		bool flag = _enchantments.Count > 0;
		if (_enchantList != null)
		{
			((CanvasItem)_enchantList).Visible = flag;
		}
		if (flag)
		{
			for (int i = 0; i < _enchantSlots.Count; i++)
			{
				EnchantSlot enchantSlot = _enchantSlots[i];
				int index = WrapIndex(_index - 4 + i);
				enchantSlot.Icon.Texture = (Texture2D)(object)_enchantments[index].Icon;
				int num = Math.Abs(i - 4);
				bool flag2 = num == 0;
				float num2 = (flag2 ? 1f : Mathf.Pow(0.8f, (float)num));
				((CanvasItem)enchantSlot.Root).Modulate = new Color(num2, num2, num2, num2);
				enchantSlot.Root.Scale = (flag2 ? (Vector2.One * 1.05f) : Vector2.One);
			}
		}
	}

	private bool TryShowEnchantment(int idx)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		if (idx < 0 || idx >= _enchantments.Count)
		{
			return false;
		}
		_index = idx;
		EnchantmentModel val = _enchantments[idx];
		if (!_displayCardCache.TryGetValue(((object)((AbstractModel)val).Id).ToString(), out CardModel value) || value == null)
		{
			return false;
		}
		try
		{
			CardModel val2 = (CardModel)((AbstractModel)value).MutableClone();
			EnchantmentModel val3 = val.ToMutable();
			val2.EnchantInternal(val3, 1m);
			val2.IsEnchantmentPreview = true;
			try
			{
				val3.ModifyCard();
			}
			catch (Exception value2)
			{
				GD.PrintErr($"[PengoTarot] 附魔 {((AbstractModel)val).Id} 预览 ModifyCard 失败（仅影响数值显示）：\n{value2}");
			}
			_card.Model = val2;
			_card.UpdateVisuals((PileType)0, (CardPreviewMode)1);
			UpdateEnchantName(val3);
			UpdatePaginatorTarget(CurrentEnchantmentId);
			RefreshPanelState();
			ShaderController.ApplyShader(_card);
			return true;
		}
		catch (Exception value3)
		{
			GD.PrintErr($"[PengoTarot] 附魔预览失败 {((AbstractModel)val).Id}：\n{value3}");
			return false;
		}
	}

	private void UpdateEnchantName(EnchantmentModel ench)
	{
		if (_enchantNameLabel != null)
		{
			_enchantNameLabel.Text = ench.Title.GetFormattedText() ?? "";
			((CanvasItem)_enchantNameLabel).Visible = true;
		}
	}

	private void OnRightButtonReleased()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (((CanvasItem)_rightButton).Visible)
		{
			SetEnchantment(_index + 1);
			((CanvasItem)_card).Modulate = Colors.White;
			((Control)_card).Position = _cardPosition;
			((Node)this).CreateTween().SetParallel(true).TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("position"), Variant.op_Implicit(_cardPosition), 0.25)
				.SetEase((EaseType)1)
				.SetTrans((TransitionType)5)
				.From(Variant.op_Implicit(_cardPosition + new Vector2(0f, 100f)));
		}
	}

	private void OnLeftButtonReleased()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		if (((CanvasItem)_leftButton).Visible)
		{
			SetEnchantment(_index - 1);
			((CanvasItem)_card).Modulate = Colors.White;
			((Control)_card).Position = _cardPosition;
			((Node)this).CreateTween().SetParallel(true).TweenProperty((GodotObject)(object)_card, NodePath.op_Implicit("position"), Variant.op_Implicit(_cardPosition), 0.25)
				.SetEase((EaseType)1)
				.SetTrans((TransitionType)5)
				.From(Variant.op_Implicit(_cardPosition + new Vector2(0f, -100f)));
		}
	}

	private static void PlayUiClickSfx()
	{
		SfxCmd.Play("event:/sfx/ui/clicks/ui_click", 1f);
	}

	private void OnEnchantTickClicked()
	{
		SetTickState(_enchantTickVisuals, ticked: false);
		Close();
	}

	private void OnEnchantMenuAction(int id)
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		if (!(currentEnchantmentId == "") && id == 0)
		{
			EnchantmentConfig.ClearEffect(currentEnchantmentId);
			RefreshPanelState();
			NCard card = _card;
			if (((card != null) ? card.Model : null) != null)
			{
				ShaderController.ApplyShader(_card);
			}
		}
	}

	private void UpdateEnchantMenuAvailability()
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		_menuPopup.SetItemDisabled(0, currentEnchantmentId == "" || !EnchantmentConfig.HasEffect(currentEnchantmentId));
	}

	public void RefreshPanelState()
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		string currentEnchantmentId = CurrentEnchantmentId;
		if (currentEnchantmentId == "")
		{
			return;
		}
		NSlider slider = _slider;
		if (slider != null)
		{
			((GodotObject)slider).SetBlockSignals(true);
		}
		if (_slider != null)
		{
			((Range)_slider).Value = EnchantmentConfig.GetIntensity(currentEnchantmentId) * 100.0;
		}
		NSlider slider2 = _slider;
		if (slider2 != null)
		{
			((GodotObject)slider2).SetBlockSignals(false);
		}
		string editMode = EnchantmentConfig.GetEditMode(currentEnchantmentId);
		UpdateModeButtonStyles(editMode);
		bool flag = editMode == "separately";
		bool flag2 = editMode == "fully";
		string[] allPartNames = Config.AllPartNames;
		foreach (string text in allPartNames)
		{
			int effect = EnchantmentConfig.GetEffect(currentEnchantmentId, text);
			Control nodeOrNull = ((Node)this).GetNodeOrNull<Control>(NodePath.op_Implicit("%Tick" + text));
			SetTickState((nodeOrNull != null) ? ((Node)nodeOrNull).GetNodeOrNull<Control>(NodePath.op_Implicit("TickboxVisuals")) : null, effect > 0);
			Control nodeOrNull2 = ((Node)this).GetNodeOrNull<Control>(NodePath.op_Implicit("%Tick" + text));
			Control val = ((nodeOrNull2 != null) ? ((Node)nodeOrNull2).GetNodeOrNull<Control>(NodePath.op_Implicit("TickboxVisuals")) : null);
			if (val != null)
			{
				bool flag3 = (flag ? (text != _separatelyHighlight) : flag2);
				((CanvasItem)val).Modulate = (Color)(flag3 ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White);
				Control nodeOrNull3 = ((Node)this).GetNodeOrNull<Control>(NodePath.op_Implicit("%Tick" + text));
				Node obj = ((nodeOrNull3 != null) ? ((Node)nodeOrNull3).GetParent() : null);
				Node obj2 = ((obj is HBoxContainer) ? obj : null);
				Label val2 = ((obj2 != null) ? obj2.GetNodeOrNull<Label>(NodePath.op_Implicit("Label")) : null);
				if (val2 != null)
				{
					((CanvasItem)val2).Modulate = (Color)(flag3 ? new Color(0.7f, 0.7f, 0.7f, 0.7f) : Colors.White);
				}
			}
		}
		UpdateButtonAvailability(_exportBtn, EnchantmentConfig.HasEffect(currentEnchantmentId));
		UpdateButtonAvailability(_importBtn, Config.IsValidCardPresetJson(DisplayServer.ClipboardGet() ?? ""));
	}

	private static void UpdateButtonAvailability(NButton btn, bool valid)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((CanvasItem)btn).Modulate = (valid ? Colors.White : Color.FromHsv(0f, 0.1f, 0.72f, 1f));
	}

	private void OnPartClicked(string partName)
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		if (currentEnchantmentId == "")
		{
			return;
		}
		CardModel model = _card.Model;
		string cid = ((model != null) ? ((object)((AbstractModel)model).Id).ToString() : null) ?? "";
		string editMode = EnchantmentConfig.GetEditMode(currentEnchantmentId);
		if (editMode == "fully")
		{
			return;
		}
		if (editMode == "separately")
		{
			if (EnchantmentConfig.GetEffect(currentEnchantmentId, partName) > 0 && _separatelyHighlight == partName)
			{
				EnchantmentConfig.ClearEffect(currentEnchantmentId, partName);
			}
			else
			{
				_separatelyHighlight = partName;
			}
			SetPaginatorToMode(EnchantmentConfig.GetEffect(currentEnchantmentId, _separatelyHighlight));
			RefreshPanelState();
			ShaderController.RefreshAllCardsWithId(cid);
		}
		else
		{
			bool flag = EnchantmentConfig.GetEffect(currentEnchantmentId, partName) > 0;
			EnchantmentConfig.SetEffect(currentEnchantmentId, partName, (!flag) ? _pgEffects[_pgCurrentIndex].Mode : 0);
			RefreshPanelState();
			ShaderController.RefreshAllCardsWithId(cid);
		}
	}

	private void OnEditModeSelected(string mode)
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		if (!(currentEnchantmentId == ""))
		{
			EnchantmentConfig.SetEditMode(currentEnchantmentId, mode);
			if (mode == "separately")
			{
				_separatelyHighlight = ((Config.AllPartNames.Length != 0) ? Config.AllPartNames[0] : "");
				SetPaginatorToMode(EnchantmentConfig.GetEffect(currentEnchantmentId, _separatelyHighlight));
			}
			RefreshPanelState();
			CardModel model = _card.Model;
			ShaderController.RefreshAllCardsWithId(((model != null) ? ((object)((AbstractModel)model).Id).ToString() : null) ?? "");
		}
	}

	private void OnCopyCard()
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		if (!(currentEnchantmentId == ""))
		{
			DisplayServer.ClipboardSet(EnchantmentConfig.ExportCardPreset(currentEnchantmentId));
		}
	}

	private void OnPasteCard()
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		if (!(currentEnchantmentId == ""))
		{
			string text = DisplayServer.ClipboardGet();
			if (!string.IsNullOrEmpty(text) && EnchantmentConfig.ImportCardPreset(currentEnchantmentId, text))
			{
				UpdatePaginatorTarget(currentEnchantmentId);
				RefreshPanelState();
				CardModel model = _card.Model;
				ShaderController.RefreshAllCardsWithId(((model != null) ? ((object)((AbstractModel)model).Id).ToString() : null) ?? "");
			}
		}
	}

	private void InitPaginator(Label label)
	{
		_pgLabel = label;
		if (_pgInitialized)
		{
			return;
		}
		EffectRegistry.Initialize();
		_pgEffects.Clear();
		foreach (EffectRegistry.EffectDef allEffect in EffectRegistry.AllEffects)
		{
			_pgEffects.Add(allEffect);
		}
		_pgInitialized = true;
	}

	private void UpdatePaginatorTarget(string enchId)
	{
		int currentMode = EnchantmentConfig.GetCardEffectMode(enchId);
		int num = _pgEffects.FindIndex((EffectRegistry.EffectDef d) => d.Mode == currentMode);
		_pgCurrentIndex = ((num >= 0) ? num : 0);
		UpdatePaginatorLabel();
	}

	private void SetPaginatorToMode(int mode)
	{
		int num = _pgEffects.FindIndex((EffectRegistry.EffectDef d) => d.Mode == mode);
		if (num >= 0)
		{
			_pgCurrentIndex = num;
			UpdatePaginatorLabel();
		}
	}

	private void OnPaginatorNavigate(int delta)
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		if (!(currentEnchantmentId == ""))
		{
			int num = _pgCurrentIndex + delta;
			if (num < 0)
			{
				num = _pgEffects.Count - 1;
			}
			else if (num >= _pgEffects.Count)
			{
				num = 0;
			}
			_pgCurrentIndex = num;
			UpdatePaginatorLabel();
			int mode = _pgEffects[_pgCurrentIndex].Mode;
			EnchantmentConfig.SetCardEffectMode(currentEnchantmentId, mode);
			ApplyCurrentEffectToCheckedParts();
			RefreshPanelState();
			CardModel model = _card.Model;
			ShaderController.RefreshAllCardsWithId(((model != null) ? ((object)((AbstractModel)model).Id).ToString() : null) ?? "");
		}
	}

	private void UpdatePaginatorLabel()
	{
		if (_pgLabel != null && _pgInitialized)
		{
			string text = "BAL_" + _pgEffects[_pgCurrentIndex].LocKey;
			LocManager instance = LocManager.Instance;
			string text2 = ((instance != null) ? instance.GetTable("gameplay_ui").GetRawText(text) : null);
			_pgLabel.Text = ((!string.IsNullOrEmpty(text2)) ? text2 : _pgEffects[_pgCurrentIndex].LocKey);
		}
	}

	private void ApplyCurrentEffectToCheckedParts()
	{
		string currentEnchantmentId = CurrentEnchantmentId;
		if (currentEnchantmentId == "")
		{
			return;
		}
		int mode = _pgEffects[_pgCurrentIndex].Mode;
		string editMode = EnchantmentConfig.GetEditMode(currentEnchantmentId);
		if (!(editMode == "fully"))
		{
			if (editMode == "separately")
			{
				string text = _separatelyHighlight;
				if (string.IsNullOrEmpty(text))
				{
					text = ((Config.AllPartNames.Length != 0) ? Config.AllPartNames[0] : "");
				}
				if (!string.IsNullOrEmpty(text))
				{
					EnchantmentConfig.SetEffect(currentEnchantmentId, text, mode);
				}
				return;
			}
			string[] allPartNames = Config.AllPartNames;
			foreach (string partName in allPartNames)
			{
				if (EnchantmentConfig.GetEffect(currentEnchantmentId, partName) > 0)
				{
					EnchantmentConfig.SetEffect(currentEnchantmentId, partName, mode);
				}
			}
		}
		else
		{
			EnchantmentConfig.SetEffect(currentEnchantmentId, "FullCard", mode);
		}
	}

	private void WireModeButton(string nodePath, string mode)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
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
				OnEditModeSelected(mode);
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
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		//IL_0481: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_052a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_060a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_066a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0673: Unknown result type (might be due to invalid IL or missing references)
		//IL_0699: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cd: Expected O, but got Unknown
		//IL_06c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_073d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Unknown result type (might be due to invalid IL or missing references)
		//IL_076e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_079c: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0820: Unknown result type (might be due to invalid IL or missing references)
		//IL_0848: Unknown result type (might be due to invalid IL or missing references)
		//IL_0853: Expected O, but got Unknown
		//IL_084e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0859: Unknown result type (might be due to invalid IL or missing references)
		//IL_087f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0901: Unknown result type (might be due to invalid IL or missing references)
		//IL_0927: Unknown result type (might be due to invalid IL or missing references)
		//IL_094a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0955: Unknown result type (might be due to invalid IL or missing references)
		//IL_097b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0984: Unknown result type (might be due to invalid IL or missing references)
		//IL_09aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a28: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ad1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b20: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2b: Expected O, but got Unknown
		//IL_0b26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b47: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b52: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bac: Expected O, but got Unknown
		//IL_0ba7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bd3: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(41)
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
			new MethodInfo(MethodName.Close, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ReturnToCardScreen, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.BuildEnchantList, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.SetEnchantment, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("index"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.RemoveEnchantmentAt, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("idx"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateArrowVisibility, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.WrapIndex, new PropertyInfo((Type)2, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("i"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.InitializeEnchantList, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnListSlotClicked, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("slotIndex"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.RefreshEnchantList, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.TryShowEnchantment, new PropertyInfo((Type)1, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("idx"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnRightButtonReleased, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnLeftButtonReleased, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.PlayUiClickSfx, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnEnchantTickClicked, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnEnchantMenuAction, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("id"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateEnchantMenuAvailability, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.RefreshPanelState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateButtonAvailability, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("btn"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)1, StringName.op_Implicit("valid"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnPartClicked, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("partName"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnEditModeSelected, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("mode"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnCopyCard, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnPasteCard, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.InitPaginator, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("label"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Label"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdatePaginatorTarget, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("enchId"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.SetPaginatorToMode, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("mode"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnPaginatorNavigate, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("delta"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.UpdatePaginatorLabel, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ApplyCurrentEffectToCheckedParts, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
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
			new MethodInfo(MethodName.SetTickState, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("visuals"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
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
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05db: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0705: Unknown result type (might be due to invalid IL or missing references)
		//IL_078d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0744: Unknown result type (might be due to invalid IL or missing references)
		//IL_0783: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.Tr && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text = Tr(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text);
			return true;
		}
		if ((ref method) == MethodName.Create && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			NBalatroInspectEnchantScreen nBalatroInspectEnchantScreen = Create();
			ret = VariantUtils.CreateFrom<NBalatroInspectEnchantScreen>(ref nBalatroInspectEnchantScreen);
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
		if ((ref method) == MethodName.Close && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ReturnToCardScreen && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ReturnToCardScreen();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.BuildEnchantList && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			BuildEnchantList();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetEnchantment && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			SetEnchantment(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RemoveEnchantmentAt && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			RemoveEnchantmentAt(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdateArrowVisibility && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			UpdateArrowVisibility();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.WrapIndex && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			int num = WrapIndex(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<int>(ref num);
			return true;
		}
		if ((ref method) == MethodName.InitializeEnchantList && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			InitializeEnchantList();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnListSlotClicked && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnListSlotClicked(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RefreshEnchantList && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RefreshEnchantList();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.TryShowEnchantment && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			bool flag = TryShowEnchantment(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<bool>(ref flag);
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
		if ((ref method) == MethodName.PlayUiClickSfx && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayUiClickSfx();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnEnchantTickClicked && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnEnchantTickClicked();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnEnchantMenuAction && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnEnchantMenuAction(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdateEnchantMenuAvailability && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			UpdateEnchantMenuAvailability();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RefreshPanelState && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RefreshPanelState();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdateButtonAvailability && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			UpdateButtonAvailability(VariantUtils.ConvertTo<NButton>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnPartClicked && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnPartClicked(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnEditModeSelected && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnEditModeSelected(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnCopyCard && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnCopyCard();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnPasteCard && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnPasteCard();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.InitPaginator && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			InitPaginator(VariantUtils.ConvertTo<Label>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdatePaginatorTarget && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			UpdatePaginatorTarget(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SetPaginatorToMode && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			SetPaginatorToMode(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnPaginatorNavigate && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnPaginatorNavigate(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdatePaginatorLabel && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			UpdatePaginatorLabel();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplyCurrentEffectToCheckedParts && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ApplyCurrentEffectToCheckedParts();
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
			SetTickState(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
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
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.Tr && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text = Tr(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text);
			return true;
		}
		if ((ref method) == MethodName.Create && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			NBalatroInspectEnchantScreen nBalatroInspectEnchantScreen = Create();
			ret = VariantUtils.CreateFrom<NBalatroInspectEnchantScreen>(ref nBalatroInspectEnchantScreen);
			return true;
		}
		if ((ref method) == MethodName.PlayUiClickSfx && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayUiClickSfx();
			ret = default(godot_variant);
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
		if ((ref method) == MethodName.SetTickState && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			SetTickState(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
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
		if ((ref method) == MethodName.Close)
		{
			return true;
		}
		if ((ref method) == MethodName.ReturnToCardScreen)
		{
			return true;
		}
		if ((ref method) == MethodName.BuildEnchantList)
		{
			return true;
		}
		if ((ref method) == MethodName.SetEnchantment)
		{
			return true;
		}
		if ((ref method) == MethodName.RemoveEnchantmentAt)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateArrowVisibility)
		{
			return true;
		}
		if ((ref method) == MethodName.WrapIndex)
		{
			return true;
		}
		if ((ref method) == MethodName.InitializeEnchantList)
		{
			return true;
		}
		if ((ref method) == MethodName.OnListSlotClicked)
		{
			return true;
		}
		if ((ref method) == MethodName.RefreshEnchantList)
		{
			return true;
		}
		if ((ref method) == MethodName.TryShowEnchantment)
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
		if ((ref method) == MethodName.PlayUiClickSfx)
		{
			return true;
		}
		if ((ref method) == MethodName.OnEnchantTickClicked)
		{
			return true;
		}
		if ((ref method) == MethodName.OnEnchantMenuAction)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateEnchantMenuAvailability)
		{
			return true;
		}
		if ((ref method) == MethodName.RefreshPanelState)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateButtonAvailability)
		{
			return true;
		}
		if ((ref method) == MethodName.OnPartClicked)
		{
			return true;
		}
		if ((ref method) == MethodName.OnEditModeSelected)
		{
			return true;
		}
		if ((ref method) == MethodName.OnCopyCard)
		{
			return true;
		}
		if ((ref method) == MethodName.OnPasteCard)
		{
			return true;
		}
		if ((ref method) == MethodName.InitPaginator)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdatePaginatorTarget)
		{
			return true;
		}
		if ((ref method) == MethodName.SetPaginatorToMode)
		{
			return true;
		}
		if ((ref method) == MethodName.OnPaginatorNavigate)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdatePaginatorLabel)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplyCurrentEffectToCheckedParts)
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
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
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
		if ((ref name) == PropertyName._enchantNameLabel)
		{
			_enchantNameLabel = VariantUtils.ConvertTo<Label>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._index)
		{
			_index = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._separatelyHighlight)
		{
			_separatelyHighlight = VariantUtils.ConvertTo<string>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._returnIndex)
		{
			_returnIndex = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._returnViewAllUpgraded)
		{
			_returnViewAllUpgraded = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._returnOriginInspect)
		{
			_returnOriginInspect = VariantUtils.ConvertTo<NInspectCardScreen>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._cardPosition)
		{
			_cardPosition = VariantUtils.ConvertTo<Vector2>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._leftButtonY)
		{
			_leftButtonY = VariantUtils.ConvertTo<float>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._rightButtonY)
		{
			_rightButtonY = VariantUtils.ConvertTo<float>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._enchantList)
		{
			_enchantList = VariantUtils.ConvertTo<VBoxContainer>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._pgCurrentIndex)
		{
			_pgCurrentIndex = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._pgLabel)
		{
			_pgLabel = VariantUtils.ConvertTo<Label>(ref value);
			return true;
		}
		return ((GodotObject)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName.CurrentEnchantmentId)
		{
			string currentEnchantmentId = CurrentEnchantmentId;
			value = VariantUtils.CreateFrom<string>(ref currentEnchantmentId);
			return true;
		}
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
		if ((ref name) == PropertyName._enchantNameLabel)
		{
			value = VariantUtils.CreateFrom<Label>(ref _enchantNameLabel);
			return true;
		}
		if ((ref name) == PropertyName._index)
		{
			value = VariantUtils.CreateFrom<int>(ref _index);
			return true;
		}
		if ((ref name) == PropertyName._separatelyHighlight)
		{
			value = VariantUtils.CreateFrom<string>(ref _separatelyHighlight);
			return true;
		}
		if ((ref name) == PropertyName._returnIndex)
		{
			value = VariantUtils.CreateFrom<int>(ref _returnIndex);
			return true;
		}
		if ((ref name) == PropertyName._returnViewAllUpgraded)
		{
			value = VariantUtils.CreateFrom<bool>(ref _returnViewAllUpgraded);
			return true;
		}
		if ((ref name) == PropertyName._returnOriginInspect)
		{
			value = VariantUtils.CreateFrom<NInspectCardScreen>(ref _returnOriginInspect);
			return true;
		}
		if ((ref name) == PropertyName._cardPosition)
		{
			value = VariantUtils.CreateFrom<Vector2>(ref _cardPosition);
			return true;
		}
		if ((ref name) == PropertyName._leftButtonY)
		{
			value = VariantUtils.CreateFrom<float>(ref _leftButtonY);
			return true;
		}
		if ((ref name) == PropertyName._rightButtonY)
		{
			value = VariantUtils.CreateFrom<float>(ref _rightButtonY);
			return true;
		}
		if ((ref name) == PropertyName._enchantList)
		{
			value = VariantUtils.CreateFrom<VBoxContainer>(ref _enchantList);
			return true;
		}
		if ((ref name) == PropertyName._pgCurrentIndex)
		{
			value = VariantUtils.CreateFrom<int>(ref _pgCurrentIndex);
			return true;
		}
		if ((ref name) == PropertyName._pgLabel)
		{
			value = VariantUtils.CreateFrom<Label>(ref _pgLabel);
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
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._card, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._backstop, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._hoverTipRect, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._leftButton, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._rightButton, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._slider, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._exportBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._importBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._menuBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._menuPopup, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._enchantTickbox, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._enchantTickVisuals, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._enchantNameLabel, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._index, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)4, PropertyName._separatelyHighlight, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)4, PropertyName.CurrentEnchantmentId, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._returnIndex, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._returnViewAllUpgraded, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._returnOriginInspect, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)5, PropertyName._cardPosition, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)3, PropertyName._leftButtonY, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)3, PropertyName._rightButtonY, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._enchantList, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._pgCurrentIndex, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._pgLabel, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
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
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._card, Variant.From<NCard>(ref _card));
		info.AddProperty(PropertyName._backstop, Variant.From<ColorRect>(ref _backstop));
		info.AddProperty(PropertyName._hoverTipRect, Variant.From<Control>(ref _hoverTipRect));
		info.AddProperty(PropertyName._leftButton, Variant.From<NButton>(ref _leftButton));
		info.AddProperty(PropertyName._rightButton, Variant.From<NButton>(ref _rightButton));
		info.AddProperty(PropertyName._slider, Variant.From<NSlider>(ref _slider));
		info.AddProperty(PropertyName._exportBtn, Variant.From<NButton>(ref _exportBtn));
		info.AddProperty(PropertyName._importBtn, Variant.From<NButton>(ref _importBtn));
		info.AddProperty(PropertyName._menuBtn, Variant.From<NButton>(ref _menuBtn));
		info.AddProperty(PropertyName._menuPopup, Variant.From<PopupMenu>(ref _menuPopup));
		info.AddProperty(PropertyName._enchantTickbox, Variant.From<Control>(ref _enchantTickbox));
		info.AddProperty(PropertyName._enchantTickVisuals, Variant.From<Control>(ref _enchantTickVisuals));
		info.AddProperty(PropertyName._enchantNameLabel, Variant.From<Label>(ref _enchantNameLabel));
		info.AddProperty(PropertyName._index, Variant.From<int>(ref _index));
		info.AddProperty(PropertyName._separatelyHighlight, Variant.From<string>(ref _separatelyHighlight));
		info.AddProperty(PropertyName._returnIndex, Variant.From<int>(ref _returnIndex));
		info.AddProperty(PropertyName._returnViewAllUpgraded, Variant.From<bool>(ref _returnViewAllUpgraded));
		info.AddProperty(PropertyName._returnOriginInspect, Variant.From<NInspectCardScreen>(ref _returnOriginInspect));
		info.AddProperty(PropertyName._cardPosition, Variant.From<Vector2>(ref _cardPosition));
		info.AddProperty(PropertyName._leftButtonY, Variant.From<float>(ref _leftButtonY));
		info.AddProperty(PropertyName._rightButtonY, Variant.From<float>(ref _rightButtonY));
		info.AddProperty(PropertyName._enchantList, Variant.From<VBoxContainer>(ref _enchantList));
		info.AddProperty(PropertyName._pgCurrentIndex, Variant.From<int>(ref _pgCurrentIndex));
		info.AddProperty(PropertyName._pgLabel, Variant.From<Label>(ref _pgLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
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
		if (info.TryGetProperty(PropertyName._slider, ref val6))
		{
			_slider = ((Variant)(ref val6)).As<NSlider>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._exportBtn, ref val7))
		{
			_exportBtn = ((Variant)(ref val7)).As<NButton>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._importBtn, ref val8))
		{
			_importBtn = ((Variant)(ref val8)).As<NButton>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._menuBtn, ref val9))
		{
			_menuBtn = ((Variant)(ref val9)).As<NButton>();
		}
		Variant val10 = default(Variant);
		if (info.TryGetProperty(PropertyName._menuPopup, ref val10))
		{
			_menuPopup = ((Variant)(ref val10)).As<PopupMenu>();
		}
		Variant val11 = default(Variant);
		if (info.TryGetProperty(PropertyName._enchantTickbox, ref val11))
		{
			_enchantTickbox = ((Variant)(ref val11)).As<Control>();
		}
		Variant val12 = default(Variant);
		if (info.TryGetProperty(PropertyName._enchantTickVisuals, ref val12))
		{
			_enchantTickVisuals = ((Variant)(ref val12)).As<Control>();
		}
		Variant val13 = default(Variant);
		if (info.TryGetProperty(PropertyName._enchantNameLabel, ref val13))
		{
			_enchantNameLabel = ((Variant)(ref val13)).As<Label>();
		}
		Variant val14 = default(Variant);
		if (info.TryGetProperty(PropertyName._index, ref val14))
		{
			_index = ((Variant)(ref val14)).As<int>();
		}
		Variant val15 = default(Variant);
		if (info.TryGetProperty(PropertyName._separatelyHighlight, ref val15))
		{
			_separatelyHighlight = ((Variant)(ref val15)).As<string>();
		}
		Variant val16 = default(Variant);
		if (info.TryGetProperty(PropertyName._returnIndex, ref val16))
		{
			_returnIndex = ((Variant)(ref val16)).As<int>();
		}
		Variant val17 = default(Variant);
		if (info.TryGetProperty(PropertyName._returnViewAllUpgraded, ref val17))
		{
			_returnViewAllUpgraded = ((Variant)(ref val17)).As<bool>();
		}
		Variant val18 = default(Variant);
		if (info.TryGetProperty(PropertyName._returnOriginInspect, ref val18))
		{
			_returnOriginInspect = ((Variant)(ref val18)).As<NInspectCardScreen>();
		}
		Variant val19 = default(Variant);
		if (info.TryGetProperty(PropertyName._cardPosition, ref val19))
		{
			_cardPosition = ((Variant)(ref val19)).As<Vector2>();
		}
		Variant val20 = default(Variant);
		if (info.TryGetProperty(PropertyName._leftButtonY, ref val20))
		{
			_leftButtonY = ((Variant)(ref val20)).As<float>();
		}
		Variant val21 = default(Variant);
		if (info.TryGetProperty(PropertyName._rightButtonY, ref val21))
		{
			_rightButtonY = ((Variant)(ref val21)).As<float>();
		}
		Variant val22 = default(Variant);
		if (info.TryGetProperty(PropertyName._enchantList, ref val22))
		{
			_enchantList = ((Variant)(ref val22)).As<VBoxContainer>();
		}
		Variant val23 = default(Variant);
		if (info.TryGetProperty(PropertyName._pgCurrentIndex, ref val23))
		{
			_pgCurrentIndex = ((Variant)(ref val23)).As<int>();
		}
		Variant val24 = default(Variant);
		if (info.TryGetProperty(PropertyName._pgLabel, ref val24))
		{
			_pgLabel = ((Variant)(ref val24)).As<Label>();
		}
	}
}
