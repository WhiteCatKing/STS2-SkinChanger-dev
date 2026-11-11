using System;
using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using PengoTarot.BalatroEffect;
using PengoTarot.Powers;

namespace PengoTarot.ConfigFW;

[ScriptPath("res://configFW/Scripts/NConfigFloatingWindow.cs")]
public class NConfigFloatingWindow : Control, IScreenContext
{
	public class MethodName : MethodName
	{
		public static readonly StringName GetLocaleFont = StringName.op_Implicit("GetLocaleFont");

		public static readonly StringName ScheduleHintFontsWarmUp = StringName.op_Implicit("ScheduleHintFontsWarmUp");

		public static readonly StringName OnWarmUpFrame = StringName.op_Implicit("OnWarmUpFrame");

		public static readonly StringName SafeLoc = StringName.op_Implicit("SafeLoc");

		public static readonly StringName Create = StringName.op_Implicit("Create");

		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName BindSettingsToggle = StringName.op_Implicit("BindSettingsToggle");

		public static readonly StringName BindTipLabel = StringName.op_Implicit("BindTipLabel");

		public static readonly StringName ToggleSettingsOnly = StringName.op_Implicit("ToggleSettingsOnly");

		public static readonly StringName ApplySettingsToggleVisual = StringName.op_Implicit("ApplySettingsToggleVisual");

		public static readonly StringName ScaleTickIcon = StringName.op_Implicit("ScaleTickIcon");

		public static readonly StringName Tr = StringName.op_Implicit("Tr");

		public static readonly StringName BindLeftButtons = StringName.op_Implicit("BindLeftButtons");

		public static readonly StringName ApplyBigButtonIcon = StringName.op_Implicit("ApplyBigButtonIcon");

		public static readonly StringName BindRightButtons = StringName.op_Implicit("BindRightButtons");

		public static readonly StringName ToLabelBbcode = StringName.op_Implicit("ToLabelBbcode");

		public static readonly StringName FlagHintText = StringName.op_Implicit("FlagHintText");

		public static readonly StringName AddHover = StringName.op_Implicit("AddHover");

		public static readonly StringName ShowRightTopExtras = StringName.op_Implicit("ShowRightTopExtras");

		public static readonly StringName HideRightTopExtras = StringName.op_Implicit("HideRightTopExtras");

		public static readonly StringName ScaleTo = StringName.op_Implicit("ScaleTo");

		public static readonly StringName ApplyToggleVisual = StringName.op_Implicit("ApplyToggleVisual");

		public static readonly StringName GetBaseModulate = StringName.op_Implicit("GetBaseModulate");

		public static readonly StringName AnimateRightButtons = StringName.op_Implicit("AnimateRightButtons");

		public static readonly StringName PlayRightButtonsShow = StringName.op_Implicit("PlayRightButtonsShow");

		public static readonly StringName PlayRightButtonsHide = StringName.op_Implicit("PlayRightButtonsHide");

		public static readonly StringName RefreshFromRunData = StringName.op_Implicit("RefreshFromRunData");

		public static readonly StringName Open = StringName.op_Implicit("Open");

		public static readonly StringName Close = StringName.op_Implicit("Close");

		public static readonly StringName CloseLocal = StringName.op_Implicit("CloseLocal");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName DefaultFocusedControl = StringName.op_Implicit("DefaultFocusedControl");

		public static readonly StringName _backstop = StringName.op_Implicit("_backstop");

		public static readonly StringName _backstopHit = StringName.op_Implicit("_backstopHit");

		public static readonly StringName _openTween = StringName.op_Implicit("_openTween");

		public static readonly StringName _remoteMode = StringName.op_Implicit("_remoteMode");

		public static readonly StringName _editable = StringName.op_Implicit("_editable");

		public static readonly StringName _closing = StringName.op_Implicit("_closing");

		public static readonly StringName _hintLabel = StringName.op_Implicit("_hintLabel");

		public static readonly StringName _rightGrid = StringName.op_Implicit("_rightGrid");

		public static readonly StringName _panel = StringName.op_Implicit("_panel");

		public static readonly StringName _tarotBtn = StringName.op_Implicit("_tarotBtn");

		public static readonly StringName _planetBtn = StringName.op_Implicit("_planetBtn");

		public static readonly StringName _hoveredTipButton = StringName.op_Implicit("_hoveredTipButton");

		public static readonly StringName _settingsToggle = StringName.op_Implicit("_settingsToggle");

		public static readonly StringName _settingsToggleTick = StringName.op_Implicit("_settingsToggleTick");

		public static readonly StringName _tipLabel = StringName.op_Implicit("_tipLabel");
	}

	public class SignalName : SignalName
	{
	}

	private const string ScenePath = "res://configFW/Scenes/configfloatingwindow.tscn";

	private const string TarotIcon = "res://configFW/Scenes/images/tarot.webp";

	private const string TarotUpIcon = "res://configFW/Scenes/images/tarot_up.webp";

	private const string PlanetIcon = "res://configFW/Scenes/images/planet.webp";

	private const string PlanetUpIcon = "res://configFW/Scenes/images/planet_up.webp";

	private static Font? _boldFont;

	private static bool _hintFontsWarmedUp;

	private static RichTextLabel? _warmLabel;

	private static Queue<string>? _warmChunks;

	private ColorRect _backstop;

	private NButton _backstopHit;

	private Tween? _openTween;

	private bool _remoteMode;

	private bool _editable;

	private bool _closing;

	private readonly List<Control> _toggleButtons = new List<Control>();

	private readonly Dictionary<Control, bool> _toggleState = new Dictionary<Control, bool>();

	private RichTextLabel _hintLabel;

	private GridContainer _rightGrid;

	private readonly List<Control> _rightButtons = new List<Control>();

	private readonly List<Tween> _rightTweens = new List<Tween>();

	private Control? _panel;

	private Control? _tarotBtn;

	private Control? _planetBtn;

	private readonly Dictionary<Control, int> _rightButtonIndices = new Dictionary<Control, int>();

	private Control? _hoveredTipButton;

	private HBoxContainer _settingsToggle;

	private Control _settingsToggleTick;

	private Label _tipLabel;

	private static int _configOpensThisLaunch;

	private static readonly string[] FlagNames = new string[22]
	{
		"Fool", "Magician", "HighPriestess", "Empress", "Emperor", "Hierophant", "Lovers", "Chariot", "Strength", "Hermit",
		"WheelOfFortune", "Justice", "HangedMan", "Death", "Temperance", "Devil", "Tower", "Star", "Moon", "Sun",
		"Judgement", "World"
	};

	private static readonly List<string> _recentTips = new List<string>();

	private const int RecentTipCount = 10;

	private static Font BoldFont => _boldFont ?? (_boldFont = GD.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres"));

	public Control? DefaultFocusedControl => null;

	private static Font? GetLocaleFont(FontType type)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (LocManager.Instance != null && FontManager.NeedsFontSubstitution(LocManager.Instance.Language))
		{
			return FontManager.GetSubstituteFont(LocManager.Instance.Language, type);
		}
		return null;
	}

	public static void ScheduleHintFontsWarmUp()
	{
		if (_hintFontsWarmedUp)
		{
			return;
		}
		try
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				_hintFontsWarmedUp = true;
				val.ProcessFrame += OnWarmUpFrame;
			}
		}
		catch (Exception)
		{
			_hintFontsWarmedUp = true;
		}
	}

	private static void OnWarmUpFrame()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		try
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null)
			{
				return;
			}
			if (_warmChunks == null)
			{
				_warmChunks = BuildWarmUpChunks();
			}
			if (_warmChunks.Count == 0)
			{
				val.ProcessFrame -= OnWarmUpFrame;
				RichTextLabel? warmLabel = _warmLabel;
				if (warmLabel != null)
				{
					((Node)warmLabel).QueueFree();
				}
				_warmLabel = null;
				_warmChunks = null;
				return;
			}
			Window root = val.Root;
			if (root != null)
			{
				if (_warmLabel == null)
				{
					_warmLabel = new RichTextLabel
					{
						BbcodeEnabled = true,
						AutowrapMode = (AutowrapMode)2,
						MouseFilter = (MouseFilterEnum)2,
						Modulate = new Color(1f, 1f, 1f, 0.01f),
						Position = new Vector2(0f, 0f),
						Size = new Vector2(1100f, 520f)
					};
					((Control)_warmLabel).AddThemeFontOverride(StringName.op_Implicit("normal_font"), GetLocaleFont((FontType)0) ?? BoldFont);
					((Control)_warmLabel).AddThemeFontSizeOverride(StringName.op_Implicit("normal_font_size"), 24);
					((Node)root).AddChild((Node)(object)_warmLabel, false, (InternalMode)0);
				}
				_warmLabel.Text = _warmChunks.Dequeue();
			}
		}
		catch (Exception)
		{
			_warmChunks = new Queue<string>();
		}
	}

	private static Queue<string> BuildWarmUpChunks()
	{
		List<string> list = new List<string>
		{
			SafeLoc("CFW_DEFAULT_HINT"),
			SafeLoc("CFW_IN_DEVELOPMENT_TITLE"),
			SafeLoc("CFW_IN_DEVELOPMENT_DESC")
		};
		string[] flagNames = FlagNames;
		foreach (string text in flagNames)
		{
			list.Add(SafeLoc("CFW_FLAG_" + text + "_DESC"));
		}
		LocString ifExists = LocString.GetIfExists("gameplay_ui", "BAL_CFW_PROGRESS_LINE");
		if (ifExists != null)
		{
			ifExists.Add("Expired", false);
			ifExists.Add("Count", 3m);
			list.Add(ifExists.GetFormattedText() ?? string.Empty);
		}
		Queue<string> queue = new Queue<string>();
		List<string> list2 = new List<string>();
		int num = 0;
		foreach (string item in list)
		{
			if (num > 0 && num + item.Length > 350)
			{
				queue.Enqueue(string.Join("\n\n", list2));
				list2.Clear();
				num = 0;
			}
			list2.Add(item);
			num += item.Length;
		}
		if (list2.Count > 0)
		{
			queue.Enqueue(string.Join("\n\n", list2));
		}
		return queue;
	}

	private static string SafeLoc(string key)
	{
		LocString ifExists = LocString.GetIfExists("gameplay_ui", "BAL_" + key);
		return ((ifExists != null) ? ifExists.GetFormattedText() : null) ?? key;
	}

	public static NConfigFloatingWindow? Create()
	{
		return GD.Load<PackedScene>("res://configFW/Scenes/configfloatingwindow.tscn").Instantiate<NConfigFloatingWindow>((GenEditState)0);
	}

	public override void _Ready()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		_backstop = ((Node)this).GetNode<ColorRect>(NodePath.op_Implicit("Backstop"));
		_backstopHit = ((Node)this).GetNode<NButton>(NodePath.op_Implicit("BackstopHit"));
		((GodotObject)_backstopHit).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			ConfigFloatingWindow.ClosePanel();
		}), 0u);
		VBoxContainer node = ((Node)this).GetNode<VBoxContainer>(NodePath.op_Implicit("CenterPanel/PanelMargin/PanelVBox"));
		Label node2 = ((Node)node).GetNode<Label>(NodePath.op_Implicit("TitleLabel"));
		((Control)node2).AddThemeFontOverride(StringName.op_Implicit("font"), GetLocaleFont((FontType)1) ?? BoldFont);
		node2.Text = Tr("CFW_TITLE");
		_hintLabel = ((Node)node).GetNode<RichTextLabel>(NodePath.op_Implicit("HintLabel"));
		((Control)_hintLabel).AddThemeFontOverride(StringName.op_Implicit("normal_font"), GetLocaleFont((FontType)0) ?? BoldFont);
		_hintLabel.VerticalAlignment = (VerticalAlignment)1;
		_hintLabel.HorizontalAlignment = (HorizontalAlignment)1;
		_hintLabel.AutowrapMode = (AutowrapMode)2;
		_hintLabel.BbcodeEnabled = true;
		_hintLabel.Text = ToLabelBbcode(Tr("CFW_DEFAULT_HINT"));
		HBoxContainer node3 = ((Node)node).GetNode<HBoxContainer>(NodePath.op_Implicit("Body"));
		VBoxContainer node4 = ((Node)node3).GetNode<VBoxContainer>(NodePath.op_Implicit("LeftColumn"));
		_rightGrid = ((Node)node3).GetNode<GridContainer>(NodePath.op_Implicit("RightGrid"));
		_panel = ((Node)this).GetNodeOrNull<Control>(NodePath.op_Implicit("CenterPanel"));
		BindLeftButtons(node4);
		BindRightButtons(_rightGrid);
		((Control)node3).MouseFilter = (MouseFilterEnum)0;
		((Control)node3).MouseExited += delegate
		{
			_hintLabel.Text = ToLabelBbcode(Tr("CFW_DEFAULT_HINT"));
		};
		if (!ConfigFloatingWindowRunData.TarotEnabled)
		{
			foreach (Control rightButton in _rightButtons)
			{
				rightButton.MouseFilter = (MouseFilterEnum)2;
				NButton val = (NButton)(object)((rightButton is NButton) ? rightButton : null);
				if (val != null)
				{
					((NClickableControl)val).Disable();
				}
				((CanvasItem)rightButton).Modulate = new Color(((CanvasItem)rightButton).Modulate.R, ((CanvasItem)rightButton).Modulate.G, ((CanvasItem)rightButton).Modulate.B, 0f);
			}
		}
		ShaderController.ApplyStarcloudBgAspect((Control)(object)((Node)this).GetNodeOrNull<ColorRect>(NodePath.op_Implicit("CardAreaBg")));
		BindSettingsToggle();
		BindTipLabel();
		((CanvasItem)this).Visible = false;
	}

	private void BindSettingsToggle()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		_settingsToggle = ((Node)this).GetNode<HBoxContainer>(NodePath.op_Implicit("%SettingsToggle"));
		_settingsToggleTick = ((Node)_settingsToggle).GetNode<Control>(NodePath.op_Implicit("TickIcon"));
		Label label = ((Node)_settingsToggle).GetNode<Label>(NodePath.op_Implicit("Label"));
		((Control)label).AddThemeFontOverride(StringName.op_Implicit("font"), GetLocaleFont((FontType)0) ?? BoldFont);
		label.Text = Tr("CFW_SETTINGS_TOGGLE");
		_settingsToggleTick.MouseFilter = (MouseFilterEnum)1;
		((Control)label).MouseFilter = (MouseFilterEnum)1;
		_settingsToggleTick.MouseEntered += delegate
		{
			ScaleTickIcon(_settingsToggleTick, 1.1f);
		};
		_settingsToggleTick.MouseExited += delegate
		{
			ScaleTickIcon(_settingsToggleTick, 1f);
		};
		((Control)label).MouseEntered += delegate
		{
			ScaleTickIcon(_settingsToggleTick, 1.1f);
		};
		((Control)label).MouseExited += delegate
		{
			ScaleTickIcon(_settingsToggleTick, 1f);
		};
		((GodotObject)_settingsToggle).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent ev)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			InputEventMouseButton val = (InputEventMouseButton)(object)((ev is InputEventMouseButton) ? ev : null);
			if (val != null && (long)val.ButtonIndex == 1 && val.Pressed)
			{
				Vector2 localMousePosition = ((CanvasItem)_settingsToggle).GetLocalMousePosition();
				Rect2 rect = _settingsToggleTick.GetRect();
				if (!((Rect2)(ref rect)).HasPoint(localMousePosition))
				{
					rect = ((Control)label).GetRect();
					if (!((Rect2)(ref rect)).HasPoint(localMousePosition))
					{
						return;
					}
				}
				ToggleSettingsOnly();
			}
		}), 0u);
		ApplySettingsToggleVisual(ConfigFloatingWindowConfig.ShowInSettingsOnly);
	}

	private void BindTipLabel()
	{
		_tipLabel = ((Node)this).GetNodeOrNull<Label>(NodePath.op_Implicit("TipLabel"));
		if (_tipLabel != null)
		{
			_configOpensThisLaunch++;
			((Control)_tipLabel).AddThemeFontOverride(StringName.op_Implicit("font"), GetLocaleFont((FontType)0) ?? BoldFont);
			LocManager instance = LocManager.Instance;
			string text = ((instance != null) ? instance.Language : null) ?? "en";
			IReadOnlyList<string> mainTips = ConfigFloatingWindowLoc.GetMainTips(text);
			if (mainTips.Count == 0)
			{
				((CanvasItem)_tipLabel).Visible = false;
			}
			else
			{
				_tipLabel.Text = "TIPS：" + PickRandomTip(mainTips, text);
			}
		}
	}

	private static string PickRandomTip(IReadOnlyList<string> main, string lang)
	{
		IReadOnlyList<string> thanksTips = ConfigFloatingWindowLoc.GetThanksTips(lang);
		List<string> list = new List<string>(main);
		if (_configOpensThisLaunch >= 3)
		{
			list.AddRange(thanksTips);
		}
		List<string> list2 = new List<string>();
		foreach (string item in list)
		{
			if (!_recentTips.Contains(item))
			{
				list2.Add(item);
			}
		}
		if (list2.Count == 0)
		{
			list2 = list;
		}
		string text = list2[Random.Shared.Next(list2.Count)];
		_recentTips.Add(text);
		while (_recentTips.Count > 10)
		{
			_recentTips.RemoveAt(0);
		}
		return text;
	}

	private void ToggleSettingsOnly()
	{
		bool flag = !ConfigFloatingWindowConfig.ShowInSettingsOnly;
		ConfigFloatingWindowConfig.SetShowInSettingsOnly(flag);
		ConfigFloatingWindow.RefreshRunEntryDisplayMode();
		ApplySettingsToggleVisual(flag);
	}

	private void ApplySettingsToggleVisual(bool isOn)
	{
		if (_settingsToggleTick == null)
		{
			return;
		}
		Control nodeOrNull = ((Node)_settingsToggleTick).GetNodeOrNull<Control>(NodePath.op_Implicit("TickboxVisuals"));
		if (nodeOrNull != null)
		{
			Control nodeOrNull2 = ((Node)nodeOrNull).GetNodeOrNull<Control>(NodePath.op_Implicit("Ticked"));
			Control nodeOrNull3 = ((Node)nodeOrNull).GetNodeOrNull<Control>(NodePath.op_Implicit("NotTicked"));
			if (nodeOrNull2 != null)
			{
				((CanvasItem)nodeOrNull2).Visible = isOn;
			}
			if (nodeOrNull3 != null)
			{
				((CanvasItem)nodeOrNull3).Visible = !isOn;
			}
		}
	}

	private static void ScaleTickIcon(Control target, float scale)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		target.PivotOffset = target.Size / 2f;
		((Node)target).CreateTween().TweenProperty((GodotObject)(object)target, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * scale), 0.10000000149011612).SetTrans((TransitionType)4)
			.SetEase((EaseType)1);
	}

	private static string Tr(string key)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;
	}

	private void BindLeftButtons(VBoxContainer column)
	{
		_tarotBtn = ((Node)column).GetNodeOrNull<Control>(NodePath.op_Implicit("TarotBtn"));
		_planetBtn = ((Node)column).GetNodeOrNull<Control>(NodePath.op_Implicit("PlanetBtn"));
		BindLeftToggle(_tarotBtn, ConfigFloatingWindowRunData.TarotEnabled, delegate(bool v)
		{
			ConfigFloatingWindowConfig.SetTarotEnabled(v);
			ConfigFloatingWindowRunData.SetTarotEnabled(v);
			ConfigFloatingWindow.BroadcastConfig();
			if (v)
			{
				foreach (KeyValuePair<Control, int> rightButtonIndex in _rightButtonIndices)
				{
					_toggleState[rightButtonIndex.Key] = ConfigFloatingWindowRunData.GetTarFlag(rightButtonIndex.Value);
				}
			}
			AnimateRightButtons(v);
		}, "CFW_TAROT_DESC", "res://configFW/Scenes/images/tarot.webp", "res://configFW/Scenes/images/tarot_up.webp");
		BindLeftToggle(((Node)column).GetNodeOrNull<Control>(NodePath.op_Implicit("PlanetBtn")), ConfigFloatingWindowRunData.PlanetEnabled, delegate(bool v)
		{
			ConfigFloatingWindowConfig.SetPlanetEnabled(v);
			ConfigFloatingWindowRunData.SetPlanetEnabled(v);
			ConfigFloatingWindow.BroadcastConfig();
		}, "CFW_PLANET_DESC", "res://configFW/Scenes/images/planet.webp", "res://configFW/Scenes/images/planet_up.webp");
	}

	private static void ApplyBigButtonIcon(Control? btn, bool isUp, string? normalIcon, string? upIcon)
	{
		if (btn == null)
		{
			return;
		}
		TextureRect nodeOrNull = ((Node)btn).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Icon"));
		if (nodeOrNull == null)
		{
			return;
		}
		string text = (isUp ? upIcon : normalIcon);
		if (!string.IsNullOrEmpty(text))
		{
			Texture2D val = GD.Load<Texture2D>(text);
			if (val != null)
			{
				nodeOrNull.Texture = val;
			}
		}
	}

	private void BindLeftToggle(Control? btn, bool isOn, Action<bool> onToggled, string hintKey, string? normalIcon = null, string? upIcon = null)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if (btn == null)
		{
			return;
		}
		Control? obj = btn;
		NButton val = (NButton)(object)((obj is NButton) ? obj : null);
		if (val != null)
		{
			((GodotObject)val).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
			{
				bool flag = !_toggleState[btn];
				_toggleState[btn] = flag;
				onToggled(flag);
				ApplyToggleVisual(btn, flag);
				ApplyBigButtonIcon(btn, flag, normalIcon, upIcon);
			}), 0u);
		}
		AddHover(btn, hintKey);
		_toggleState[btn] = isOn;
		_toggleButtons.Add(btn);
		ApplyToggleVisual(btn, isOn);
		ApplyBigButtonIcon(btn, isOn, normalIcon, upIcon);
	}

	private void BindRightButtons(GridContainer grid)
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (Node child in ((Node)grid).GetChildren(false))
		{
			Control btn = (Control)(object)((child is Control) ? child : null);
			if (btn == null || !((object)((Node)btn).Name).ToString().StartsWith("Flag"))
			{
				continue;
			}
			int idx = num;
			_rightButtonIndices[btn] = idx;
			if (idx < FlagNames.Length)
			{
				AddHover(btn, "CFW_FLAG_" + FlagNames[idx].ToUpperInvariant() + "_DESC");
			}
			if (ExtraTipsForConfigFlag(idx).Length != 0)
			{
				btn.MouseEntered += delegate
				{
					_hoveredTipButton = btn;
					ShowRightTopExtras(idx, btn);
				};
				btn.MouseExited += delegate
				{
					if (_hoveredTipButton == btn)
					{
						_hoveredTipButton = null;
					}
					HideRightTopExtras(btn);
				};
			}
			Control obj = btn;
			NButton val = (NButton)(object)((obj is NButton) ? obj : null);
			if (val != null)
			{
				((GodotObject)val).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
				{
					bool flag = !_toggleState[btn];
					_toggleState[btn] = flag;
					ConfigFloatingWindowConfig.SetDifficultyFlag(idx, flag);
					ConfigFloatingWindowRunData.SetDifficultyFlag(idx, flag);
					ConfigFloatingWindow.BroadcastConfig();
					ApplyToggleVisual(btn, flag);
					if (_hoveredTipButton == btn)
					{
						ShowRightTopExtras(idx, btn);
					}
				}), 0u);
			}
			_toggleState[btn] = ConfigFloatingWindowRunData.GetTarFlag(idx);
			_toggleButtons.Add(btn);
			_rightButtons.Add(btn);
			ApplyToggleVisual(btn, _toggleState[btn]);
			num++;
		}
	}

	private static string ToLabelBbcode(string text)
	{
		if (text == null)
		{
			return string.Empty;
		}
		return "[center]" + text.Replace("[gold]", "[color=#EFC851]").Replace("[/gold]", "[/color]").Replace("[red]", "[color=#FF5555]")
			.Replace("[/red]", "[/color]")
			.Replace("[blue]", "[color=#87CEEB]")
			.Replace("[/blue]", "[/color]")
			.Replace("[purple]", "[color=#EE82EE]")
			.Replace("[/purple]", "[/color]")
			.Replace("[green]", "[color=#7FFF00]")
			.Replace("[/green]", "[/color]") + "[/center]";
	}

	private string FlagHintText(string hintKey)
	{
		if (hintKey.StartsWith("CFW_FLAG_") && hintKey.EndsWith("_DESC"))
		{
			string b = hintKey.Substring("CFW_FLAG_".Length, hintKey.Length - "CFW_FLAG_".Length - "_DESC".Length);
			for (int i = 0; i < FlagNames.Length; i++)
			{
				if (string.Equals(FlagNames[i], b, StringComparison.OrdinalIgnoreCase))
				{
					if (!ConfigFloatingWindowLoc.HasDynamicSettingsDescription(i))
					{
						break;
					}
					return ConfigFloatingWindowLoc.BuildSettingsDescription(i);
				}
			}
		}
		return Tr(hintKey);
	}

	private void AddHover(Control btn, string hintKey)
	{
		btn.MouseEntered += delegate
		{
			_hintLabel.Text = ToLabelBbcode(FlagHintText(hintKey));
			ScaleTo(btn, 1.05f);
		};
		btn.MouseExited += delegate
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I8
			ScaleTo(btn, 1f);
			if ((long)btn.MouseFilter != 2)
			{
				ApplyToggleVisual(btn, _toggleState[btn]);
			}
		};
	}

	private static IHoverTip[] ExtraTipsForConfigFlag(int flag)
	{
		IHoverTip[] array = flag switch
		{
			0 => (IHoverTip[])(object)new IHoverTip[1] { TarotPackTip() }, 
			1 => (IHoverTip[])(object)new IHoverTip[1] { TarotPackTip() }, 
			2 => (IHoverTip[])(object)new IHoverTip[1] { TarotPackTip() }, 
			3 => (IHoverTip[])(object)new IHoverTip[1] { TarotPackTip() }, 
			4 => (IHoverTip[])(object)new IHoverTip[1] { TarotPackTip() }, 
			5 => (IHoverTip[])(object)new IHoverTip[1] { TarotPackTip() }, 
			6 => (IHoverTip[])(object)new IHoverTip[1] { RoomTip("ROOM_ELITE") }, 
			7 => (IHoverTip[])(object)new IHoverTip[2]
			{
				RoomTip("ROOM_ELITE"),
				HoverTipFactory.FromPower<VulnerablePower>((int?)null)
			}, 
			8 => (IHoverTip[])(object)new IHoverTip[2]
			{
				RoomTip("ROOM_ELITE"),
				HoverTipFactory.FromPower<WeakPower>((int?)null)
			}, 
			9 => (IHoverTip[])(object)new IHoverTip[3]
			{
				RoomTip("ROOM_ELITE"),
				HoverTipFactory.FromPower<TarHermitReversedPower>((int?)null),
				HoverTipFactory.Static((StaticHoverTip)5, Array.Empty<DynamicVar>())
			}, 
			11 => (IHoverTip[])(object)new IHoverTip[2]
			{
				RoomTip("ROOM_ENEMY"),
				HoverTipFactory.FromKeyword((CardKeyword)1)
			}, 
			12 => (IHoverTip[])(object)new IHoverTip[2]
			{
				RoomTip("ROOM_ENEMY"),
				HoverTipFactory.FromKeyword((CardKeyword)1)
			}, 
			13 => (IHoverTip[])(object)new IHoverTip[1] { RoomTip("ROOM_ENEMY") }, 
			14 => (IHoverTip[])(object)new IHoverTip[1] { HoverTipFactory.FromPower<TarTemperanceReversedPower>((int?)null) }, 
			15 => (IHoverTip[])(object)new IHoverTip[1] { RoomTip("ROOM_REST") }, 
			16 => (IHoverTip[])(object)new IHoverTip[2]
			{
				HoverTipFactory.FromKeyword((CardKeyword)1),
				HoverTipFactory.FromCard<AscendersBane>(false)
			}, 
			20 => (IHoverTip[])(object)new IHoverTip[1] { PortableTarotTip() }, 
			_ => Array.Empty<IHoverTip>(), 
		};
		if (flag < 6)
		{
			return array;
		}
		IHoverTip[] array2 = (IHoverTip[])(object)new IHoverTip[array.Length + 1];
		array2[0] = InDevelopmentTip();
		Array.Copy(array, 0, array2, 1, array.Length);
		return array2;
	}

	private static IHoverTip RoomTip(string prefix)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0030: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		return (IHoverTip)(object)new HoverTip(new LocString("static_hover_tips", prefix + ".title"), new LocString("static_hover_tips", prefix + ".description"), (Texture2D)null);
	}

	private static IHoverTip InDevelopmentTip()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0026: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		HoverTip val = default(HoverTip);
		((HoverTip)(ref val))._002Ector(new LocString("gameplay_ui", "BAL_CFW_IN_DEVELOPMENT_TITLE"), new LocString("gameplay_ui", "BAL_CFW_IN_DEVELOPMENT_DESC"), (Texture2D)null);
		((HoverTip)(ref val)).IsDebuff = true;
		return (IHoverTip)(object)val;
	}

	private static IHoverTip TarotPackTip()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		LocString val = new LocString("gameplay_ui", "BAL_CFW_TAROT_PACK_DESC");
		val.Add("FoolOn", ConfigFloatingWindowRunData.GetTarFlag(0));
		val.Add("MagicianOn", ConfigFloatingWindowRunData.GetTarFlag(1));
		val.Add("HierophantOn", ConfigFloatingWindowRunData.GetTarFlag(5));
		val.Add("PriestessOn", ConfigFloatingWindowRunData.GetTarFlag(2));
		return (IHoverTip)(object)new HoverTip(new LocString("gameplay_ui", "BAL_CFW_TAROT_PACK_TITLE"), val, (Texture2D)null);
	}

	private static IHoverTip PortableTarotTip()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return (IHoverTip)(object)new HoverTip(new LocString("gameplay_ui", "BAL_CFW_PORTABLE_TAROT_TITLE"), new LocString("gameplay_ui", "BAL_CFW_PORTABLE_TAROT_DESC"), (Texture2D)null);
	}

	private void ShowRightTopExtras(int flag, Control owner)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		IHoverTip[] array = ExtraTipsForConfigFlag(flag);
		if (array.Length == 0 || _panel == null)
		{
			return;
		}
		NHoverTipSet.Remove(owner);
		NHoverTipSet tip = NHoverTipSet.CreateAndShow(owner, (IEnumerable<IHoverTip>)array, (HoverTipAlignment)0);
		if (tip == null)
		{
			return;
		}
		Control panel = _panel;
		Callable val = Callable.From((Action)delegate
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)tip) && GodotObject.IsInstanceValid((GodotObject)(object)panel))
			{
				tip.SetAlignment(panel, (HoverTipAlignment)2);
				NHoverTipSet obj = tip;
				((Control)obj).GlobalPosition = ((Control)obj).GlobalPosition + new Vector2(10f, 0f);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void HideRightTopExtras(Control owner)
	{
		NHoverTipSet.Remove(owner);
	}

	private void ScaleTo(Control target, float scale)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		target.PivotOffset = target.Size / 2f;
		((Node)target).CreateTween().TweenProperty((GodotObject)(object)target, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * scale), 0.10000000149011612).SetTrans((TransitionType)4)
			.SetEase((EaseType)1);
	}

	private void ApplyToggleVisual(Control btn, bool isOn)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		Panel nodeOrNull = ((Node)btn).GetNodeOrNull<Panel>(NodePath.op_Implicit("Bg"));
		if (nodeOrNull != null)
		{
			((Control)nodeOrNull).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)new StyleBoxFlat
			{
				BgColor = new Color(0f, 0f, 0f, 0f)
			});
		}
		((CanvasItem)btn).Modulate = GetBaseModulate(isOn);
	}

	private Color GetBaseModulate(bool isOn)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Color val = (Color)(isOn ? Colors.White : new Color(0.7f, 0.7f, 0.7f, 0.7f));
		if (!_editable)
		{
			((Color)(ref val))._002Ector(val.R * 0.65f, val.G * 0.65f, val.B * 0.65f, val.A * 0.9f);
		}
		return val;
	}

	private void AnimateRightButtons(bool show)
	{
		foreach (Tween rightTween in _rightTweens)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)rightTween))
			{
				rightTween.Kill();
			}
		}
		_rightTweens.Clear();
		((CanvasItem)_rightGrid).Visible = true;
		SceneTree tree;
		if (show)
		{
			tree = ((Node)this).GetTree();
			tree.ProcessFrame += Start;
		}
		else
		{
			PlayRightButtonsHide();
		}
		void Start()
		{
			tree.ProcessFrame -= Start;
			if (GodotObject.IsInstanceValid((GodotObject)(object)this))
			{
				PlayRightButtonsShow();
			}
		}
	}

	private void PlayRightButtonsShow()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		int count = _rightButtons.Count;
		for (int i = 0; i < count; i++)
		{
			Control val = _rightButtons[i];
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				continue;
			}
			Color baseModulate = GetBaseModulate(_toggleState[val]);
			((CanvasItem)val).Visible = true;
			val.MouseFilter = (MouseFilterEnum)0;
			if (_editable)
			{
				NButton val2 = (NButton)(object)((val is NButton) ? val : null);
				if (val2 != null)
				{
					((NClickableControl)val2).Enable();
				}
			}
			val.PivotOffset = val.Size / 2f;
			val.Scale = Vector2.One * 0.92f;
			((CanvasItem)val).Modulate = new Color(baseModulate.R, baseModulate.G, baseModulate.B, 0f);
			Tween val3 = ((Node)val).CreateTween();
			_rightTweens.Add(val3);
			val3.TweenInterval((double)((float)i * 0.045f));
			val3.Parallel().TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("modulate"), Variant.op_Implicit(baseModulate), 0.3199999928474426).SetTrans((TransitionType)4)
				.SetEase((EaseType)1);
			val3.Parallel().TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One), 0.3499999940395355).SetTrans((TransitionType)10)
				.SetEase((EaseType)1);
		}
	}

	private void PlayRightButtonsHide()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		int count = _rightButtons.Count;
		for (int i = 0; i < count; i++)
		{
			Control btn = _rightButtons[i];
			if (btn == null || !GodotObject.IsInstanceValid((GodotObject)(object)btn))
			{
				continue;
			}
			btn.MouseFilter = (MouseFilterEnum)2;
			btn.PivotOffset = btn.Size / 2f;
			Tween val = ((Node)btn).CreateTween();
			_rightTweens.Add(val);
			val.TweenInterval((double)((float)(count - 1 - i) * 0.04f));
			val.Parallel().TweenProperty((GodotObject)(object)btn, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.20000000298023224).SetTrans((TransitionType)4)
				.SetEase((EaseType)0);
			val.Parallel().TweenProperty((GodotObject)(object)btn, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One * 0.92f), 0.23999999463558197).SetTrans((TransitionType)4)
				.SetEase((EaseType)0);
			val.Chain().TweenCallback(Callable.From((Action)delegate
			{
				Control obj = btn;
				NButton val2 = (NButton)(object)((obj is NButton) ? obj : null);
				if (val2 != null)
				{
					((NClickableControl)val2).Disable();
				}
			}));
		}
	}

	public void RefreshFromRunData()
	{
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		foreach (Tween rightTween in _rightTweens)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)rightTween))
			{
				rightTween.Kill();
			}
		}
		_rightTweens.Clear();
		foreach (KeyValuePair<Control, int> rightButtonIndex in _rightButtonIndices)
		{
			bool tarFlag = ConfigFloatingWindowRunData.GetTarFlag(rightButtonIndex.Value);
			_toggleState[rightButtonIndex.Key] = tarFlag;
			ApplyToggleVisual(rightButtonIndex.Key, tarFlag);
		}
		if (_tarotBtn != null)
		{
			bool tarotEnabled = ConfigFloatingWindowRunData.TarotEnabled;
			_toggleState[_tarotBtn] = tarotEnabled;
			ApplyToggleVisual(_tarotBtn, tarotEnabled);
			ApplyBigButtonIcon(_tarotBtn, tarotEnabled, "res://configFW/Scenes/images/tarot.webp", "res://configFW/Scenes/images/tarot_up.webp");
			if (tarotEnabled)
			{
				AnimateRightButtons(show: true);
			}
			else
			{
				foreach (Control rightButton in _rightButtons)
				{
					rightButton.MouseFilter = (MouseFilterEnum)2;
					NButton val = (NButton)(object)((rightButton is NButton) ? rightButton : null);
					if (val != null)
					{
						((NClickableControl)val).Disable();
					}
					((CanvasItem)rightButton).Modulate = new Color(((CanvasItem)rightButton).Modulate.R, ((CanvasItem)rightButton).Modulate.G, ((CanvasItem)rightButton).Modulate.B, 0f);
				}
			}
		}
		if (_planetBtn != null)
		{
			bool planetEnabled = ConfigFloatingWindowRunData.PlanetEnabled;
			_toggleState[_planetBtn] = planetEnabled;
			ApplyToggleVisual(_planetBtn, planetEnabled);
			ApplyBigButtonIcon(_planetBtn, planetEnabled, "res://configFW/Scenes/images/planet.webp", "res://configFW/Scenes/images/planet_up.webp");
		}
		if (_editable)
		{
			return;
		}
		foreach (Control toggleButton in _toggleButtons)
		{
			NButton val2 = (NButton)(object)((toggleButton is NButton) ? toggleButton : null);
			if (val2 != null)
			{
				((NClickableControl)val2).Disable();
			}
		}
	}

	public void Open(bool remote, bool editable)
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		_remoteMode = remote;
		_editable = editable;
		((CanvasItem)this).Visible = true;
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		if (remote)
		{
			((Control)_backstopHit).MouseFilter = (MouseFilterEnum)2;
		}
		foreach (Control toggleButton in _toggleButtons)
		{
			NButton val = (NButton)(object)((toggleButton is NButton) ? toggleButton : null);
			if (val != null)
			{
				if (editable)
				{
					((NClickableControl)val).Enable();
				}
				else
				{
					((NClickableControl)val).Disable();
				}
			}
			ApplyToggleVisual(toggleButton, _toggleState[toggleButton]);
		}
		if (!ConfigFloatingWindowRunData.TarotEnabled)
		{
			foreach (Control rightButton in _rightButtons)
			{
				rightButton.MouseFilter = (MouseFilterEnum)2;
				NButton val2 = (NButton)(object)((rightButton is NButton) ? rightButton : null);
				if (val2 != null)
				{
					((NClickableControl)val2).Disable();
				}
				((CanvasItem)rightButton).Modulate = new Color(((CanvasItem)rightButton).Modulate.R, ((CanvasItem)rightButton).Modulate.G, ((CanvasItem)rightButton).Modulate.B, 0f);
			}
		}
		Tween? openTween = _openTween;
		if (openTween != null)
		{
			openTween.Kill();
		}
		_openTween = ((Node)this).CreateTween().SetParallel(true);
		_openTween.TweenProperty((GodotObject)(object)_backstop, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.9f), 0.25);
		_openTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(1f), 0.25).SetEase((EaseType)1).SetTrans((TransitionType)5)
			.From(Variant.op_Implicit(0f));
		ActiveScreenContext.Instance.Update();
		if (!remote)
		{
			NHotkeyManager.Instance.AddBlockingScreen((Node)(object)this);
			NHotkeyManager.Instance.PushHotkeyPressedBinding(StringName.op_Implicit(MegaInput.cancel), (Action)CloseLocal);
			NHotkeyManager.Instance.PushHotkeyPressedBinding(StringName.op_Implicit(MegaInput.pauseAndBack), (Action)CloseLocal);
		}
	}

	public void Close()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		if (!_closing && ((CanvasItem)this).Visible)
		{
			_closing = true;
			((Control)this).MouseFilter = (MouseFilterEnum)2;
			((Control)_backstopHit).MouseFilter = (MouseFilterEnum)2;
			if (!_remoteMode)
			{
				NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.cancel), (Action)CloseLocal);
				NHotkeyManager.Instance.RemoveHotkeyPressedBinding(StringName.op_Implicit(MegaInput.pauseAndBack), (Action)CloseLocal);
				NHotkeyManager.Instance.RemoveBlockingScreen((Node)(object)this);
			}
			Tween? openTween = _openTween;
			if (openTween != null)
			{
				openTween.Kill();
			}
			_openTween = ((Node)this).CreateTween().SetParallel(true);
			_openTween.TweenProperty((GodotObject)(object)_backstop, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.25);
			_openTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.1);
			_openTween.Chain().TweenCallback(Callable.From((Action)delegate
			{
				((CanvasItem)this).Visible = false;
				ActiveScreenContext.Instance.Update();
				((Node)this).QueueFree();
			}));
		}
	}

	private void CloseLocal()
	{
		ConfigFloatingWindow.ClosePanel();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected O, but got Unknown
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Expected O, but got Unknown
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Expected O, but got Unknown
		//IL_03e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_047b: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ae: Expected O, but got Unknown
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_052f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0552: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0583: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Expected O, but got Unknown
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0603: Unknown result type (might be due to invalid IL or missing references)
		//IL_0626: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0657: Expected O, but got Unknown
		//IL_0652: Unknown result type (might be due to invalid IL or missing references)
		//IL_065d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Expected O, but got Unknown
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_070a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0715: Expected O, but got Unknown
		//IL_0710: Unknown result type (might be due to invalid IL or missing references)
		//IL_0731: Unknown result type (might be due to invalid IL or missing references)
		//IL_073c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0762: Unknown result type (might be due to invalid IL or missing references)
		//IL_078a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0795: Expected O, but got Unknown
		//IL_0790: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0806: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Unknown result type (might be due to invalid IL or missing references)
		//IL_0837: Unknown result type (might be due to invalid IL or missing references)
		//IL_085a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0865: Unknown result type (might be due to invalid IL or missing references)
		//IL_088b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0894: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0918: Unknown result type (might be due to invalid IL or missing references)
		//IL_093b: Unknown result type (might be due to invalid IL or missing references)
		//IL_095c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0967: Unknown result type (might be due to invalid IL or missing references)
		//IL_098d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0996: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c5: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(30)
		{
			new MethodInfo(MethodName.GetLocaleFont, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Font"), false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("type"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ScheduleHintFontsWarmUp, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnWarmUpFrame, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.SafeLoc, new PropertyInfo((Type)4, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("key"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Create, new PropertyInfo((Type)24, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.BindSettingsToggle, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.BindTipLabel, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ToggleSettingsOnly, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ApplySettingsToggleVisual, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("isOn"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ScaleTickIcon, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("target"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)3, StringName.op_Implicit("scale"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Tr, new PropertyInfo((Type)4, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("key"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.BindLeftButtons, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("column"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("VBoxContainer"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ApplyBigButtonIcon, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("btn"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)1, StringName.op_Implicit("isUp"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)4, StringName.op_Implicit("normalIcon"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)4, StringName.op_Implicit("upIcon"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.BindRightButtons, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("grid"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("GridContainer"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ToLabelBbcode, new PropertyInfo((Type)4, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("text"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.FlagHintText, new PropertyInfo((Type)4, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, StringName.op_Implicit("hintKey"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.AddHover, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("btn"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)4, StringName.op_Implicit("hintKey"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ShowRightTopExtras, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)2, StringName.op_Implicit("flag"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)24, StringName.op_Implicit("owner"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.HideRightTopExtras, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("owner"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ScaleTo, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("target"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)3, StringName.op_Implicit("scale"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.ApplyToggleVisual, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("btn"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false),
				new PropertyInfo((Type)1, StringName.op_Implicit("isOn"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.GetBaseModulate, new PropertyInfo((Type)20, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("isOn"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.AnimateRightButtons, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("show"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.PlayRightButtonsShow, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.PlayRightButtonsHide, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.RefreshFromRunData, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.Open, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("remote"), (PropertyHint)0, "", (PropertyUsageFlags)6, false),
				new PropertyInfo((Type)1, StringName.op_Implicit("editable"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Close, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.CloseLocal, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.GetLocaleFont && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Font localeFont = GetLocaleFont(VariantUtils.ConvertTo<FontType>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Font>(ref localeFont);
			return true;
		}
		if ((ref method) == MethodName.ScheduleHintFontsWarmUp && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ScheduleHintFontsWarmUp();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnWarmUpFrame && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnWarmUpFrame();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SafeLoc && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text = SafeLoc(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text);
			return true;
		}
		if ((ref method) == MethodName.Create && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			NConfigFloatingWindow nConfigFloatingWindow = Create();
			ret = VariantUtils.CreateFrom<NConfigFloatingWindow>(ref nConfigFloatingWindow);
			return true;
		}
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.BindSettingsToggle && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			BindSettingsToggle();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.BindTipLabel && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			BindTipLabel();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ToggleSettingsOnly && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ToggleSettingsOnly();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplySettingsToggleVisual && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			ApplySettingsToggleVisual(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ScaleTickIcon && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			ScaleTickIcon(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Tr && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text2 = Tr(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text2);
			return true;
		}
		if ((ref method) == MethodName.BindLeftButtons && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			BindLeftButtons(VariantUtils.ConvertTo<VBoxContainer>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplyBigButtonIcon && ((NativeVariantPtrArgs)(ref args)).Count == 4)
		{
			ApplyBigButtonIcon(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[2]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[3]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.BindRightButtons && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			BindRightButtons(VariantUtils.ConvertTo<GridContainer>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ToLabelBbcode && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text3 = ToLabelBbcode(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text3);
			return true;
		}
		if ((ref method) == MethodName.FlagHintText && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text4 = FlagHintText(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text4);
			return true;
		}
		if ((ref method) == MethodName.AddHover && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			AddHover(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ShowRightTopExtras && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			ShowRightTopExtras(VariantUtils.ConvertTo<int>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.HideRightTopExtras && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			HideRightTopExtras(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ScaleTo && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			ScaleTo(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplyToggleVisual && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			ApplyToggleVisual(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.GetBaseModulate && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Color baseModulate = GetBaseModulate(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Color>(ref baseModulate);
			return true;
		}
		if ((ref method) == MethodName.AnimateRightButtons && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			AnimateRightButtons(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PlayRightButtonsShow && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayRightButtonsShow();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.PlayRightButtonsHide && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			PlayRightButtonsHide();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.RefreshFromRunData && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			RefreshFromRunData();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Open && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			Open(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Close && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Close();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.CloseLocal && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			CloseLocal();
			ret = default(godot_variant);
			return true;
		}
		return ((Control)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName.GetLocaleFont && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Font localeFont = GetLocaleFont(VariantUtils.ConvertTo<FontType>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<Font>(ref localeFont);
			return true;
		}
		if ((ref method) == MethodName.ScheduleHintFontsWarmUp && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ScheduleHintFontsWarmUp();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnWarmUpFrame && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnWarmUpFrame();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.SafeLoc && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text = SafeLoc(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text);
			return true;
		}
		if ((ref method) == MethodName.Create && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			NConfigFloatingWindow nConfigFloatingWindow = Create();
			ret = VariantUtils.CreateFrom<NConfigFloatingWindow>(ref nConfigFloatingWindow);
			return true;
		}
		if ((ref method) == MethodName.ScaleTickIcon && ((NativeVariantPtrArgs)(ref args)).Count == 2)
		{
			ScaleTickIcon(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<float>(ref ((NativeVariantPtrArgs)(ref args))[1]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Tr && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text2 = Tr(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text2);
			return true;
		}
		if ((ref method) == MethodName.ApplyBigButtonIcon && ((NativeVariantPtrArgs)(ref args)).Count == 4)
		{
			ApplyBigButtonIcon(VariantUtils.ConvertTo<Control>(ref ((NativeVariantPtrArgs)(ref args))[0]), VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[1]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[2]), VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[3]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ToLabelBbcode && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			string text3 = ToLabelBbcode(VariantUtils.ConvertTo<string>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = VariantUtils.CreateFrom<string>(ref text3);
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName.GetLocaleFont)
		{
			return true;
		}
		if ((ref method) == MethodName.ScheduleHintFontsWarmUp)
		{
			return true;
		}
		if ((ref method) == MethodName.OnWarmUpFrame)
		{
			return true;
		}
		if ((ref method) == MethodName.SafeLoc)
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
		if ((ref method) == MethodName.BindSettingsToggle)
		{
			return true;
		}
		if ((ref method) == MethodName.BindTipLabel)
		{
			return true;
		}
		if ((ref method) == MethodName.ToggleSettingsOnly)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplySettingsToggleVisual)
		{
			return true;
		}
		if ((ref method) == MethodName.ScaleTickIcon)
		{
			return true;
		}
		if ((ref method) == MethodName.Tr)
		{
			return true;
		}
		if ((ref method) == MethodName.BindLeftButtons)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplyBigButtonIcon)
		{
			return true;
		}
		if ((ref method) == MethodName.BindRightButtons)
		{
			return true;
		}
		if ((ref method) == MethodName.ToLabelBbcode)
		{
			return true;
		}
		if ((ref method) == MethodName.FlagHintText)
		{
			return true;
		}
		if ((ref method) == MethodName.AddHover)
		{
			return true;
		}
		if ((ref method) == MethodName.ShowRightTopExtras)
		{
			return true;
		}
		if ((ref method) == MethodName.HideRightTopExtras)
		{
			return true;
		}
		if ((ref method) == MethodName.ScaleTo)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplyToggleVisual)
		{
			return true;
		}
		if ((ref method) == MethodName.GetBaseModulate)
		{
			return true;
		}
		if ((ref method) == MethodName.AnimateRightButtons)
		{
			return true;
		}
		if ((ref method) == MethodName.PlayRightButtonsShow)
		{
			return true;
		}
		if ((ref method) == MethodName.PlayRightButtonsHide)
		{
			return true;
		}
		if ((ref method) == MethodName.RefreshFromRunData)
		{
			return true;
		}
		if ((ref method) == MethodName.Open)
		{
			return true;
		}
		if ((ref method) == MethodName.Close)
		{
			return true;
		}
		if ((ref method) == MethodName.CloseLocal)
		{
			return true;
		}
		return ((Control)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if ((ref name) == PropertyName._backstop)
		{
			_backstop = VariantUtils.ConvertTo<ColorRect>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._backstopHit)
		{
			_backstopHit = VariantUtils.ConvertTo<NButton>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._openTween)
		{
			_openTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._remoteMode)
		{
			_remoteMode = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._editable)
		{
			_editable = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._closing)
		{
			_closing = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._hintLabel)
		{
			_hintLabel = VariantUtils.ConvertTo<RichTextLabel>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._rightGrid)
		{
			_rightGrid = VariantUtils.ConvertTo<GridContainer>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._panel)
		{
			_panel = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._tarotBtn)
		{
			_tarotBtn = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._planetBtn)
		{
			_planetBtn = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._hoveredTipButton)
		{
			_hoveredTipButton = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._settingsToggle)
		{
			_settingsToggle = VariantUtils.ConvertTo<HBoxContainer>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._settingsToggleTick)
		{
			_settingsToggleTick = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._tipLabel)
		{
			_tipLabel = VariantUtils.ConvertTo<Label>(ref value);
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
		if ((ref name) == PropertyName.DefaultFocusedControl)
		{
			Control defaultFocusedControl = DefaultFocusedControl;
			value = VariantUtils.CreateFrom<Control>(ref defaultFocusedControl);
			return true;
		}
		if ((ref name) == PropertyName._backstop)
		{
			value = VariantUtils.CreateFrom<ColorRect>(ref _backstop);
			return true;
		}
		if ((ref name) == PropertyName._backstopHit)
		{
			value = VariantUtils.CreateFrom<NButton>(ref _backstopHit);
			return true;
		}
		if ((ref name) == PropertyName._openTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _openTween);
			return true;
		}
		if ((ref name) == PropertyName._remoteMode)
		{
			value = VariantUtils.CreateFrom<bool>(ref _remoteMode);
			return true;
		}
		if ((ref name) == PropertyName._editable)
		{
			value = VariantUtils.CreateFrom<bool>(ref _editable);
			return true;
		}
		if ((ref name) == PropertyName._closing)
		{
			value = VariantUtils.CreateFrom<bool>(ref _closing);
			return true;
		}
		if ((ref name) == PropertyName._hintLabel)
		{
			value = VariantUtils.CreateFrom<RichTextLabel>(ref _hintLabel);
			return true;
		}
		if ((ref name) == PropertyName._rightGrid)
		{
			value = VariantUtils.CreateFrom<GridContainer>(ref _rightGrid);
			return true;
		}
		if ((ref name) == PropertyName._panel)
		{
			value = VariantUtils.CreateFrom<Control>(ref _panel);
			return true;
		}
		if ((ref name) == PropertyName._tarotBtn)
		{
			value = VariantUtils.CreateFrom<Control>(ref _tarotBtn);
			return true;
		}
		if ((ref name) == PropertyName._planetBtn)
		{
			value = VariantUtils.CreateFrom<Control>(ref _planetBtn);
			return true;
		}
		if ((ref name) == PropertyName._hoveredTipButton)
		{
			value = VariantUtils.CreateFrom<Control>(ref _hoveredTipButton);
			return true;
		}
		if ((ref name) == PropertyName._settingsToggle)
		{
			value = VariantUtils.CreateFrom<HBoxContainer>(ref _settingsToggle);
			return true;
		}
		if ((ref name) == PropertyName._settingsToggleTick)
		{
			value = VariantUtils.CreateFrom<Control>(ref _settingsToggleTick);
			return true;
		}
		if ((ref name) == PropertyName._tipLabel)
		{
			value = VariantUtils.CreateFrom<Label>(ref _tipLabel);
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
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._backstop, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._backstopHit, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._openTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._remoteMode, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._editable, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._closing, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._hintLabel, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._rightGrid, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._panel, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._tarotBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._planetBtn, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._hoveredTipButton, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._settingsToggle, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._settingsToggleTick, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._tipLabel, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
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
		((GodotObject)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._backstop, Variant.From<ColorRect>(ref _backstop));
		info.AddProperty(PropertyName._backstopHit, Variant.From<NButton>(ref _backstopHit));
		info.AddProperty(PropertyName._openTween, Variant.From<Tween>(ref _openTween));
		info.AddProperty(PropertyName._remoteMode, Variant.From<bool>(ref _remoteMode));
		info.AddProperty(PropertyName._editable, Variant.From<bool>(ref _editable));
		info.AddProperty(PropertyName._closing, Variant.From<bool>(ref _closing));
		info.AddProperty(PropertyName._hintLabel, Variant.From<RichTextLabel>(ref _hintLabel));
		info.AddProperty(PropertyName._rightGrid, Variant.From<GridContainer>(ref _rightGrid));
		info.AddProperty(PropertyName._panel, Variant.From<Control>(ref _panel));
		info.AddProperty(PropertyName._tarotBtn, Variant.From<Control>(ref _tarotBtn));
		info.AddProperty(PropertyName._planetBtn, Variant.From<Control>(ref _planetBtn));
		info.AddProperty(PropertyName._hoveredTipButton, Variant.From<Control>(ref _hoveredTipButton));
		info.AddProperty(PropertyName._settingsToggle, Variant.From<HBoxContainer>(ref _settingsToggle));
		info.AddProperty(PropertyName._settingsToggleTick, Variant.From<Control>(ref _settingsToggleTick));
		info.AddProperty(PropertyName._tipLabel, Variant.From<Label>(ref _tipLabel));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((GodotObject)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._backstop, ref val))
		{
			_backstop = ((Variant)(ref val)).As<ColorRect>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._backstopHit, ref val2))
		{
			_backstopHit = ((Variant)(ref val2)).As<NButton>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._openTween, ref val3))
		{
			_openTween = ((Variant)(ref val3)).As<Tween>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._remoteMode, ref val4))
		{
			_remoteMode = ((Variant)(ref val4)).As<bool>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._editable, ref val5))
		{
			_editable = ((Variant)(ref val5)).As<bool>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._closing, ref val6))
		{
			_closing = ((Variant)(ref val6)).As<bool>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._hintLabel, ref val7))
		{
			_hintLabel = ((Variant)(ref val7)).As<RichTextLabel>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._rightGrid, ref val8))
		{
			_rightGrid = ((Variant)(ref val8)).As<GridContainer>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._panel, ref val9))
		{
			_panel = ((Variant)(ref val9)).As<Control>();
		}
		Variant val10 = default(Variant);
		if (info.TryGetProperty(PropertyName._tarotBtn, ref val10))
		{
			_tarotBtn = ((Variant)(ref val10)).As<Control>();
		}
		Variant val11 = default(Variant);
		if (info.TryGetProperty(PropertyName._planetBtn, ref val11))
		{
			_planetBtn = ((Variant)(ref val11)).As<Control>();
		}
		Variant val12 = default(Variant);
		if (info.TryGetProperty(PropertyName._hoveredTipButton, ref val12))
		{
			_hoveredTipButton = ((Variant)(ref val12)).As<Control>();
		}
		Variant val13 = default(Variant);
		if (info.TryGetProperty(PropertyName._settingsToggle, ref val13))
		{
			_settingsToggle = ((Variant)(ref val13)).As<HBoxContainer>();
		}
		Variant val14 = default(Variant);
		if (info.TryGetProperty(PropertyName._settingsToggleTick, ref val14))
		{
			_settingsToggleTick = ((Variant)(ref val14)).As<Control>();
		}
		Variant val15 = default(Variant);
		if (info.TryGetProperty(PropertyName._tipLabel, ref val15))
		{
			_tipLabel = ((Variant)(ref val15)).As<Label>();
		}
	}
}
