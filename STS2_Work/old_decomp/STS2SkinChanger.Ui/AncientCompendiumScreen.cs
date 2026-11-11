using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class AncientCompendiumScreen : NSubmenu
{
	private enum OtherCategory
	{
		Ancients,
		Merchants,
		Creatures,
		Events
	}

	private sealed record OtherEntry(string Id, string Title, string ScenePath, string? GroupLookupId = null);

	private sealed record OtherPreviewLanguagePack(string OpenShop, string ShopPreview, string Hint, string Cards, string Relics, string Potions, string Item, string Standing, string Attack, string Block, string Sleep, string Wake);

	private static readonly Vector2I PreviewLogicalSize = new Vector2I(1920, 1080);

	private static readonly IReadOnlyDictionary<string, OtherPreviewLanguagePack> OtherPreviewPacks = new Dictionary<string, OtherPreviewLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new OtherPreviewLanguagePack("Open shop preview", "Shop preview", "A visual mockup only; nothing can be bought here.", "Cards", "Relics", "Potions", "Item {0}", "Standing", "Attack", "Block", "Sleep", "Wake"),
		["zhs"] = new OtherPreviewLanguagePack("打开商店预览", "商店预览", "这里只是查看商店布局的模拟界面，不会购买或改变游戏内容。", "卡牌", "遗物", "药水", "商品 {0}", "站姿", "攻击", "格挡", "休眠", "苏醒"),
		["zht"] = new OtherPreviewLanguagePack("開啟商店預覽", "商店預覽", "這只是查看商店配置的模擬介面，不會購買或改變遊戲內容。", "卡牌", "遺物", "藥水", "商品 {0}", "站姿", "攻擊", "格擋", "休眠", "甦醒"),
		["deu"] = new OtherPreviewLanguagePack("Shop-Vorschau öffnen", "Shop-Vorschau", "Nur eine visuelle Vorschau; hier kann nichts gekauft werden.", "Karten", "Relikte", "Tränke", "Artikel {0}", "Stehen", "Angriff", "Blocken", "Schlafen", "Aufwachen"),
		["esp"] = new OtherPreviewLanguagePack("Abrir vista de tienda", "Vista de tienda", "Solo es una maqueta visual; aquí no se puede comprar nada.", "Cartas", "Reliquias", "Pociones", "Objeto {0}", "De pie", "Ataque", "Bloqueo", "Dormir", "Despertar"),
		["fra"] = new OtherPreviewLanguagePack("Ouvrir l’aperçu de la boutique", "Aperçu de la boutique", "Ceci est une maquette visuelle ; aucun achat n’est possible ici.", "Cartes", "Reliques", "Potions", "Objet {0}", "Debout", "Attaque", "Blocage", "Sommeil", "Réveil"),
		["ita"] = new OtherPreviewLanguagePack("Apri anteprima negozio", "Anteprima negozio", "Solo un modello visivo: qui non è possibile acquistare nulla.", "Carte", "Reliquie", "Pozioni", "Oggetto {0}", "In piedi", "Attacco", "Blocco", "Sonno", "Risveglio"),
		["jpn"] = new OtherPreviewLanguagePack("ショッププレビューを開く", "ショッププレビュー", "これは見た目だけのプレビューです。ここでは購入できません。", "カード", "レリック", "ポーション", "商品 {0}", "立ち姿", "攻撃", "ブロック", "休眠", "目覚め"),
		["kor"] = new OtherPreviewLanguagePack("상점 미리보기 열기", "상점 미리보기", "시각적 모형일 뿐이며 여기서는 아무것도 구매할 수 없습니다.", "카드", "유물", "물약", "상품 {0}", "서 있기", "공격", "방어", "수면", "기상"),
		["pol"] = new OtherPreviewLanguagePack("Otwórz podgląd sklepu", "Podgląd sklepu", "To tylko wizualna makieta; tutaj niczego nie można kupić.", "Karty", "Relikty", "Mikstury", "Przedmiot {0}", "Postawa", "Atak", "Blok", "Sen", "Pobudka"),
		["ptb"] = new OtherPreviewLanguagePack("Abrir prévia da loja", "Prévia da loja", "Apenas uma simulação visual; nada pode ser comprado aqui.", "Cartas", "Relíquias", "Poções", "Item {0}", "Parado", "Ataque", "Bloquear", "Dormir", "Acordar"),
		["rus"] = new OtherPreviewLanguagePack("Открыть предпросмотр магазина", "Предпросмотр магазина", "Это только визуальная имитация; покупать здесь нельзя.", "Карты", "Реликвии", "Зелья", "Товар {0}", "Стойка", "Атака", "Блок", "Сон", "Пробуждение"),
		["spa"] = new OtherPreviewLanguagePack("Abrir vista de tienda", "Vista de tienda", "Solo es una maqueta visual; aquí no se puede comprar nada.", "Cartas", "Reliquias", "Pociones", "Objeto {0}", "De pie", "Ataque", "Bloqueo", "Dormir", "Despertar"),
		["tha"] = new OtherPreviewLanguagePack("เป\u0e34ดต\u0e31วอย\u0e48างร\u0e49านค\u0e49า", "ต\u0e31วอย\u0e48างร\u0e49านค\u0e49า", "เป\u0e47นเพ\u0e35ยงหน\u0e49าจอจำลองเพ\u0e37\u0e48อด\u0e39ร\u0e39ปแบบร\u0e49านค\u0e49า ไม\u0e48สามารถซ\u0e37\u0e49อของได\u0e49", "การ\u0e4cด", "ของท\u0e35\u0e48ระล\u0e36ก", "โพช\u0e31น", "ส\u0e34นค\u0e49า {0}", "ท\u0e48าย\u0e37น", "โจมต\u0e35", "ป\u0e49องก\u0e31น", "หล\u0e31บ", "ต\u0e37\u0e48น"),
		["tur"] = new OtherPreviewLanguagePack("Mağaza önizlemesini aç", "Mağaza önizlemesi", "Bu yalnızca görsel bir makettir; burada alışveriş yapılamaz.", "Kartlar", "Kalıntılar", "İksirler", "Eşya {0}", "Ayakta", "Saldırı", "Blok", "Uyku", "Uyan")
	};

	private readonly Dictionary<AncientEventModel, Button> _entryButtons = new Dictionary<AncientEventModel, Button>();

	private readonly Dictionary<string, Button> _otherEntryButtons = new Dictionary<string, Button>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<OtherCategory, Button> _categoryButtons = new Dictionary<OtherCategory, Button>();

	private CompendiumBackdrop _backdrop;

	private CompendiumSidebarDrawer? _sidebarDrawer;

	private VBoxContainer _entryList;

	private Label _nameLabel;

	private Label _epithetLabel;

	private Label _headingLabel;

	private HBoxContainer _skinSelector;

	private OptionButton _skinDropdown;

	private SubViewport _previewViewport;

	private SubViewportContainer _previewContainer;

	private NBackButton _compendiumBackButton;

	private NBackButton? _merchantInventoryBackButton;

	private Node? _merchantInventoryBackButtonParent;

	private int _merchantInventoryBackButtonIndex;

	private int _merchantInventoryBackButtonZIndex;

	private bool _merchantInventoryBackButtonZAsRelative;

	private VBoxContainer _otherActionSelector;

	private Node? _otherPreviewInstance;

	private string? _otherPreviewGroupId;

	private readonly List<(Button Button, OtherCreatureActionDefinition Action)> _otherActionButtons = new List<(Button, OtherCreatureActionDefinition)>();

	private AncientEventModel? _selectedAncient;

	private OtherEntry? _selectedOther;

	private OtherCategory _selectedCategory;

	private int _otherPreviewRequest;

	private bool _updatingDropdown;

	private bool _merchantInventoryOpen;

	private HBoxContainer _eventPriorityHeader;

	private Label _eventPriorityRegionName;

	private Button _eventPriorityButton;

	private Control _eventPriorityOverlay;

	private VBoxContainer _eventPriorityContent;

	private bool _eventPriorityPending;

	private OptionButton _eventRegionSelector;

	private string? _eventRegion;

	private HashSet<string> _eventRegionMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private bool _refreshingEventRegions;

	private static readonly IReadOnlyDictionary<string, (string Shared, string Other)> EventRegionNames = new Dictionary<string, (string, string)>
	{
		["eng"] = ("Across regions", "Other regions"),
		["zhs"] = ("跨地区", "其它地区"),
		["zht"] = ("跨地區", "其他地區"),
		["deu"] = ("Regionsübergreifend", "Andere Regionen"),
		["esp"] = ("Varias regiones", "Otras regiones"),
		["spa"] = ("Varias regiones", "Otras regiones"),
		["fra"] = ("Plusieurs régions", "Autres régions"),
		["ita"] = ("Più regioni", "Altre regioni"),
		["jpn"] = ("地域共通", "その他の地域"),
		["kor"] = ("공통 지역", "기타 지역"),
		["pol"] = ("Wiele regionów", "Pozostałe regiony"),
		["ptb"] = ("Várias regiões", "Outras regiões"),
		["rus"] = ("Разные регионы", "Прочие регионы"),
		["tha"] = ("หลายพ\u0e37\u0e49นท\u0e35\u0e48", "พ\u0e37\u0e49นท\u0e35\u0e48อ\u0e37\u0e48น"),
		["tur"] = ("Bölgeler arası", "Diğer bölgeler")
	};

	private static OtherPreviewLanguagePack OtherPreviewText
	{
		get
		{
			if (!OtherPreviewPacks.TryGetValue(ModLocalization.CurrentLanguage, out OtherPreviewLanguagePack value))
			{
				return OtherPreviewPacks["eng"];
			}
			return value;
		}
	}

	protected override Control? InitialFocusedControl => (Control?)(object)(_categoryButtons.Values.FirstOrDefault() ?? _entryButtons.Values.FirstOrDefault());

	public override void _Ready()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		BuildUi();
		((NSubmenu)this).ConnectSignals();
		((GodotObject)((Node)this).GetTree().Root).Connect(SignalName.SizeChanged, Callable.From((Action)ConfigurePreviewResolution), 0u);
		ConfigurePreviewResolution();
	}

	public override void OnSubmenuOpened()
	{
		((NSubmenu)this).OnSubmenuOpened();
		ConfigurePreviewResolution();
		_previewViewport.RenderTargetUpdateMode = (UpdateMode)4;
		RefreshAncients();
	}

	public override void OnSubmenuClosed()
	{
		ClearPreview();
		((CanvasItem)_otherActionSelector).Visible = false;
		_previewViewport.RenderTargetUpdateMode = (UpdateMode)0;
		FocusPreviewRuntimeProviderBehaviors(null, "关闭其它图鉴");
		((NSubmenu)this).OnSubmenuClosed();
	}

	private void BuildUi()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Expected O, but got Unknown
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Expected O, but got Unknown
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Expected O, but got Unknown
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Expected O, but got Unknown
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Expected O, but got Unknown
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Expected O, but got Unknown
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Expected O, but got Unknown
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Expected O, but got Unknown
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Expected O, but got Unknown
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Expected O, but got Unknown
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b7: Expected O, but got Unknown
		//IL_0615: Unknown result type (might be due to invalid IL or missing references)
		//IL_061a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Expected O, but got Unknown
		//IL_0638: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_064a: Expected O, but got Unknown
		_previewContainer = new SubViewportContainer
		{
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 0f,
			AnchorBottom = 0f,
			OffsetLeft = 0f,
			OffsetTop = 0f,
			OffsetRight = PreviewLogicalSize.X,
			OffsetBottom = PreviewLogicalSize.Y,
			Stretch = true,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)this).AddChild((Node)(object)_previewContainer, false, (InternalMode)0);
		_previewViewport = new SubViewport
		{
			Size = PreviewLogicalSize,
			Size2DOverride = PreviewLogicalSize,
			Size2DOverrideStretch = true,
			TransparentBg = false,
			GuiDisableInput = true,
			RenderTargetUpdateMode = (UpdateMode)0
		};
		((Node)_previewContainer).AddChild((Node)(object)_previewViewport, false, (InternalMode)0);
		_nameLabel = BuildLabel(48, new Color("efc850"));
		_nameLabel.HorizontalAlignment = (HorizontalAlignment)0;
		((Control)_nameLabel).AnchorLeft = 0f;
		((Control)_nameLabel).AnchorRight = 0f;
		((Control)_nameLabel).OffsetLeft = 82f;
		((Control)_nameLabel).OffsetTop = 900f;
		((Control)_nameLabel).OffsetRight = 750f;
		((Control)_nameLabel).OffsetBottom = 958f;
		((Node)this).AddChild((Node)(object)_nameLabel, false, (InternalMode)0);
		_epithetLabel = BuildLabel(24, new Color("87ceeB"));
		_epithetLabel.HorizontalAlignment = (HorizontalAlignment)0;
		((Control)_epithetLabel).AnchorLeft = 0f;
		((Control)_epithetLabel).AnchorRight = 0f;
		((Control)_epithetLabel).OffsetLeft = 86f;
		((Control)_epithetLabel).OffsetTop = 958f;
		((Control)_epithetLabel).OffsetRight = 750f;
		((Control)_epithetLabel).OffsetBottom = 998f;
		((Node)this).AddChild((Node)(object)_epithetLabel, false, (InternalMode)0);
		_skinSelector = new HBoxContainer
		{
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 0f,
			AnchorBottom = 0f,
			OffsetLeft = 818f,
			OffsetTop = 826f,
			OffsetRight = 1102f,
			OffsetBottom = 874f,
			Visible = false,
			ZIndex = 10
		};
		((Node)this).AddChild((Node)(object)_skinSelector, false, (InternalMode)0);
		_skinDropdown = new OptionButton
		{
			CustomMinimumSize = new Vector2(284f, 48f),
			SizeFlagsHorizontal = (SizeFlags)3,
			FitToLongestItem = false,
			ClipText = true,
			Alignment = (HorizontalAlignment)1
		};
		ContextualSkinControls.ApplyGameTheme(_skinDropdown);
		_skinDropdown.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			OnSkinSelected(checked((int)index));
		};
		((Node)_skinSelector).AddChild((Node)(object)_skinDropdown, false, (InternalMode)0);
		_otherActionSelector = new VBoxContainer
		{
			Name = StringName.op_Implicit("OtherPreviewActionSelector"),
			Visible = false,
			ZIndex = 11
		};
		((Control)_otherActionSelector).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Control)_otherActionSelector).SetAnchorsAndOffsetsPreset((LayoutPreset)0, (LayoutPresetMode)0, 0);
		((Control)_otherActionSelector).Position = new Vector2(650f, 430f);
		((Node)this).AddChild((Node)(object)_otherActionSelector, false, (InternalMode)0);
		Control val = new Control
		{
			Name = StringName.op_Implicit("OtherCompendiumSidebar"),
			AnchorLeft = 1f,
			AnchorTop = 0f,
			AnchorRight = 1f,
			AnchorBottom = 1f,
			OffsetLeft = -380f,
			OffsetTop = 0f,
			OffsetRight = 0f,
			OffsetBottom = 0f
		};
		((Node)this).AddChild((Node)(object)val, false, (InternalMode)0);
		_backdrop = new CompendiumBackdrop();
		_backdrop.AddPanel(val);
		MarginContainer val2 = new MarginContainer();
		((Control)val2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 34);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 58);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 34);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 90);
		VBoxContainer val3 = new VBoxContainer();
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 22);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		_headingLabel = BuildLabel(30, new Color("efc850"));
		_headingLabel.Text = ModLocalization.Get(ModText.OtherCompendium);
		((Control)_headingLabel).CustomMinimumSize = new Vector2(0f, 54f);
		((Control)_headingLabel).SizeFlagsHorizontal = (SizeFlags)3;
		_headingLabel.ClipText = true;
		HBoxContainer val4 = new HBoxContainer();
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_headingLabel, false, (InternalMode)0);
		Button themeButton = new Button
		{
			CustomMinimumSize = new Vector2(74f, 40f),
			SizeFlagsVertical = (SizeFlags)4
		};
		ContextualSkinControls.ApplyGameTheme(themeButton);
		((BaseButton)themeButton).Pressed += delegate
		{
			ModThemeEditor.Toggle((Node)(object)this);
		};
		ModLocalization.Bind((Node)(object)themeButton, delegate
		{
			themeButton.Text = ModThemeLocalization.Get(ThemeText.Theme);
		});
		((Node)val4).AddChild((Node)(object)themeButton, false, (InternalMode)0);
		HSeparator val5 = new HSeparator();
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
		GridContainer val6 = new GridContainer
		{
			Columns = 2
		};
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 8);
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 8);
		AddCategoryButton((Container)(object)val6, OtherCategory.Ancients, ModText.OtherCategoryAncients);
		AddCategoryButton((Container)(object)val6, OtherCategory.Merchants, ModText.OtherCategoryMerchants);
		AddCategoryButton((Container)(object)val6, OtherCategory.Creatures, ModText.OtherCategoryCreatures);
		AddCategoryButton((Container)(object)val6, OtherCategory.Events, ModText.OtherCategoryEvents);
		((Node)val3).AddChild((Node)(object)val6, false, (InternalMode)0);
		BuildEventRegionSelector(val3);
		ScrollContainer val7 = new ScrollContainer
		{
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0
		};
		((Node)val3).AddChild((Node)(object)val7, false, (InternalMode)0);
		_entryList = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_entryList).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		ModThemeListHover.AddScrollList(val7, _entryList);
		BuildEventPriorityControls();
		_sidebarDrawer = new CompendiumSidebarDrawer(val, () => !_merchantInventoryOpen && !((CanvasItem)_eventPriorityOverlay).Visible);
		_sidebarDrawer.KeepOpenFor(_eventRegionSelector);
		_compendiumBackButton = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("ui/back_button")).Instantiate<NBackButton>((GenEditState)0);
		((Node)_compendiumBackButton).Name = StringName.op_Implicit("BackButton");
		((Node)this).AddChild((Node)(object)_compendiumBackButton, false, (InternalMode)0);
		ModLocalization.Bind((Node)(object)this, RefreshLocalizedText);
	}

	private void ConfigurePreviewResolution()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		if (_previewContainer != null && _previewViewport != null)
		{
			Vector2I val = DisplayServer.WindowGetSize(0);
			float num = MathF.Min((float)val.X / (float)PreviewLogicalSize.X, (float)val.Y / (float)PreviewLogicalSize.Y);
			if (!float.IsFinite(num) || num <= 0f)
			{
				num = 1f;
			}
			Vector2I val2 = default(Vector2I);
			((Vector2I)(ref val2))._002Ector(Math.Max(2, Mathf.RoundToInt((float)PreviewLogicalSize.X * num)), Math.Max(2, Mathf.RoundToInt((float)PreviewLogicalSize.Y * num)));
			((Control)_previewContainer).Size = new Vector2((float)val2.X, (float)val2.Y);
			((Control)_previewContainer).Scale = new Vector2((float)PreviewLogicalSize.X / (float)val2.X, (float)PreviewLogicalSize.Y / (float)val2.Y);
			_previewViewport.Size2DOverride = PreviewLogicalSize;
			_previewViewport.Size2DOverrideStretch = true;
		}
	}

	private void RefreshLocalizedText()
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		_headingLabel.Text = ModLocalization.Get(ModText.OtherCompendium);
		RefreshEventRegions();
		foreach (var otherActionButton in _otherActionButtons)
		{
			otherActionButton.Button.Text = GetOtherActionText(otherActionButton.Action.Kind);
		}
		foreach (KeyValuePair<OtherCategory, Button> categoryButton in _categoryButtons)
		{
			categoryButton.Value.Text = GetCategoryText(categoryButton.Key);
		}
		if (_selectedAncient == null && _selectedOther == null && _entryButtons.Count == 0 && _otherEntryButtons.Count == 0)
		{
			_nameLabel.Text = ModLocalization.Get(ModText.NoAncientsAvailable);
		}
		if (_selectedAncient != null)
		{
			_nameLabel.Text = AncientCompendiumEntry.GetTitle(_selectedAncient);
		}
		else if (_selectedOther != null)
		{
			_nameLabel.Text = _selectedOther.Title;
		}
		Variant meta = ((GodotObject)_skinDropdown).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
		string groupId = ((Variant)(ref meta)).AsString();
		if (!string.IsNullOrWhiteSpace(groupId))
		{
			PopulateSkinDropdown(SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)));
		}
	}

	private void RebuildOtherActionButtons(OtherCreatureDefinition? creature)
	{
		foreach (Node child in ((Node)_otherActionSelector).GetChildren(false))
		{
			((Node)_otherActionSelector).RemoveChild(child);
			child.QueueFree();
		}
		_otherActionButtons.Clear();
		if (creature == null)
		{
			((CanvasItem)_otherActionSelector).Visible = false;
			return;
		}
		foreach (OtherCreatureActionDefinition action in creature.Actions)
		{
			AddOtherActionButton(action);
		}
	}

	private static string GetOtherActionText(OtherCreatureActionKind kind)
	{
		OtherPreviewLanguagePack otherPreviewText = OtherPreviewText;
		return kind switch
		{
			OtherCreatureActionKind.Attack => otherPreviewText.Attack, 
			OtherCreatureActionKind.Block => otherPreviewText.Block, 
			OtherCreatureActionKind.Sleep => otherPreviewText.Sleep, 
			OtherCreatureActionKind.Wake => otherPreviewText.Wake, 
			_ => kind.ToString(), 
		};
	}

	private void AddOtherActionButton(OtherCreatureActionDefinition action)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		Button val = new Button
		{
			Text = GetOtherActionText(action.Kind),
			CustomMinimumSize = new Vector2(104f, 44f),
			FocusMode = (FocusModeEnum)2,
			Alignment = (HorizontalAlignment)1,
			Visible = false
		};
		ContextualSkinControls.ApplyGameTheme(val);
		((BaseButton)val).Pressed += delegate
		{
			if (_otherPreviewInstance != null)
			{
				ManagedAncientSceneAnimation.TryPlay(_otherPreviewInstance, _otherPreviewGroupId, action.AnimationAliases, action.Loop, action.FollowUpAliases, action.FollowUpLoop, action.SfxPath);
			}
		};
		((Node)_otherActionSelector).AddChild((Node)(object)val, false, (InternalMode)0);
		_otherActionButtons.Add((val, action));
	}

	private void RefreshAuxiliaryPreviewControls()
	{
		((CanvasItem)_skinSelector).ZIndex = 10;
		ApplyMerchantPreviewLayerState();
	}

	private void SetMerchantInventoryOpen(bool isOpen)
	{
		_merchantInventoryOpen = isOpen;
		_sidebarDrawer?.Refresh();
		ApplyMerchantPreviewLayerState();
	}

	private void ApplyMerchantPreviewLayerState()
	{
		MerchantPreviewLayerState state = MerchantPreviewLayerPolicy.Resolve(_merchantInventoryOpen, _skinDropdown.ItemCount > 0, _otherActionButtons.Count > 0, ((CanvasItem)this).IsVisibleInTree());
		PlaceMerchantInventoryBackButton(state);
		((CanvasItem)_previewContainer).ZIndex = state.PreviewZIndex;
		((CanvasItem)_skinSelector).Visible = state.SkinSelectorVisible;
		((CanvasItem)_otherActionSelector).Visible = state.ActionSelectorVisible;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_compendiumBackButton))
		{
			if (state.CompendiumBackEnabled)
			{
				((NClickableControl)_compendiumBackButton).Enable();
				return;
			}
			((NClickableControl)_compendiumBackButton).Disable();
			_compendiumBackButton.MoveToHidePosition();
		}
	}

	private void CaptureMerchantInventoryBackButton(NBackButton? backButton)
	{
		_merchantInventoryBackButton = backButton;
		_merchantInventoryBackButtonParent = ((backButton != null) ? ((Node)backButton).GetParent() : null);
		_merchantInventoryBackButtonIndex = ((backButton != null) ? ((Node)backButton).GetIndex(false) : (-1));
		_merchantInventoryBackButtonZIndex = ((backButton != null) ? ((CanvasItem)backButton).ZIndex : 0);
		_merchantInventoryBackButtonZAsRelative = backButton == null || ((CanvasItem)backButton).ZAsRelative;
		if (backButton == null || !GodotObject.IsInstanceValid((GodotObject)(object)backButton))
		{
			ModLog.Warn("商人预览没有找到原生库存返回键。");
		}
	}

	private void PlaceMerchantInventoryBackButton(MerchantPreviewLayerState state)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		NBackButton backButton = _merchantInventoryBackButton;
		if (backButton == null || !GodotObject.IsInstanceValid((GodotObject)(object)backButton))
		{
			return;
		}
		if (state.NativeBackButtonHost == MerchantPreviewBackButtonHost.CompendiumOverlay)
		{
			if (((Node)backButton).GetParent() == this)
			{
				return;
			}
			((Node)backButton).Reparent((Node)(object)this, false);
			((CanvasItem)backButton).ZAsRelative = true;
			((CanvasItem)backButton).ZIndex = state.PreviewZIndex + 1;
			((NClickableControl)backButton).Disable();
			backButton.MoveToHidePosition();
			((NClickableControl)backButton).Enable();
			Callable val = Callable.From((Action)delegate
			{
				//IL_0051: Unknown result type (might be due to invalid IL or missing references)
				//IL_008c: Unknown result type (might be due to invalid IL or missing references)
				if (GodotObject.IsInstanceValid((GodotObject)(object)backButton))
				{
					ModLog.Info("商人预览原生返回键已迁入主覆盖层：" + $"visible={((CanvasItem)backButton).IsVisibleInTree()}, position={((Control)backButton).Position}, " + $"size={((Control)backButton).Size}, z={((CanvasItem)backButton).ZIndex}。");
				}
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		else
		{
			Node merchantInventoryBackButtonParent = _merchantInventoryBackButtonParent;
			if (merchantInventoryBackButtonParent != null && GodotObject.IsInstanceValid((GodotObject)(object)merchantInventoryBackButtonParent) && ((Node)backButton).GetParent() != merchantInventoryBackButtonParent)
			{
				((NClickableControl)backButton).Disable();
				((Node)backButton).Reparent(merchantInventoryBackButtonParent, false);
				merchantInventoryBackButtonParent.MoveChild((Node)(object)backButton, Math.Clamp(_merchantInventoryBackButtonIndex, 0, Math.Max(0, merchantInventoryBackButtonParent.GetChildCount(false) - 1)));
				((CanvasItem)backButton).ZIndex = _merchantInventoryBackButtonZIndex;
				((CanvasItem)backButton).ZAsRelative = _merchantInventoryBackButtonZAsRelative;
				backButton.MoveToHidePosition();
			}
		}
	}

	private void RefreshAncients()
	{
		RefreshEventRegions();
		foreach (Node child in ((Node)_entryList).GetChildren(false))
		{
			((Node)_entryList).RemoveChild(child);
			child.QueueFree();
		}
		_entryButtons.Clear();
		_otherEntryButtons.Clear();
		if (_selectedCategory == OtherCategory.Ancients)
		{
			AncientEventModel[] ancients = AncientCompendiumEntry.GetAncients();
			AncientEventModel[] array = ancients;
			foreach (AncientEventModel ancient in array)
			{
				Button val = CreateEntryButton(AncientCompendiumEntry.GetTitle(ancient));
				((BaseButton)val).Pressed += delegate
				{
					SelectAncient(ancient);
				};
				((Node)_entryList).AddChild((Node)(object)val, false, (InternalMode)0);
				_entryButtons[ancient] = val;
			}
			if (ancients.Length == 0)
			{
				ShowEmptyCategory();
				return;
			}
			AncientEventModel ancient2 = (AncientEventModel)((_selectedAncient == null) ? ((object)ancients[0]) : ((object)(((IEnumerable<AncientEventModel>)ancients).FirstOrDefault((Func<AncientEventModel, bool>)((AncientEventModel candidate) => ((AbstractModel)candidate).Id == ((AbstractModel)_selectedAncient).Id)) ?? ancients[0])));
			SelectAncient(ancient2);
			return;
		}
		OtherEntry[] array2 = GetOtherEntries(_selectedCategory);
		if (_selectedCategory == OtherCategory.Events)
		{
			array2 = array2.Where(delegate(OtherEntry entry)
			{
				HashSet<string> eventRegionMembers = _eventRegionMembers;
				string id = entry.Id;
				int length = "event:".Length;
				return eventRegionMembers.Contains(id.Substring(length, id.Length - length));
			}).ToArray();
		}
		OtherEntry[] array3 = array2;
		foreach (OtherEntry entry2 in array3)
		{
			Button val2 = CreateEntryButton(entry2.Title);
			((BaseButton)val2).Pressed += delegate
			{
				SelectOther(entry2);
			};
			((Node)_entryList).AddChild((Node)(object)val2, false, (InternalMode)0);
			_otherEntryButtons[entry2.Id] = val2;
		}
		if (array2.Length == 0)
		{
			ShowEmptyCategory();
			return;
		}
		OtherEntry entry3 = ((_selectedOther != null && array2.Any((OtherEntry entry) => entry.Id.Equals(_selectedOther.Id, StringComparison.OrdinalIgnoreCase))) ? array2.First((OtherEntry entry) => entry.Id.Equals(_selectedOther.Id, StringComparison.OrdinalIgnoreCase)) : array2[0]);
		SelectOther(entry3);
	}

	private Button CreateEntryButton(string title)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		Button val = new Button
		{
			Text = title,
			CustomMinimumSize = new Vector2(0f, 58f),
			ClipText = true,
			TextOverrunBehavior = (OverrunBehavior)3,
			FocusMode = (FocusModeEnum)2,
			Alignment = (HorizontalAlignment)1,
			Flat = true
		};
		ApplyEntryTheme(val, selected: false);
		ModThemeListHover.Attach(val);
		return val;
	}

	private void AddCategoryButton(Container row, OtherCategory category, ModText text)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		Button val = new Button
		{
			Text = ModLocalization.Get(text),
			CustomMinimumSize = new Vector2(96f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3,
			FocusMode = (FocusModeEnum)2,
			Alignment = (HorizontalAlignment)1,
			Flat = true
		};
		((BaseButton)val).Pressed += delegate
		{
			SelectCategory(category);
		};
		_categoryButtons[category] = val;
		((Node)row).AddChild((Node)(object)val, false, (InternalMode)0);
		ApplyCategoryTheme(val, category == _selectedCategory);
		ModThemeListHover.Attach(val);
	}

	private string GetCategoryText(OtherCategory category)
	{
		return category switch
		{
			OtherCategory.Ancients => ModLocalization.Get(ModText.OtherCategoryAncients), 
			OtherCategory.Merchants => ModLocalization.Get(ModText.OtherCategoryMerchants), 
			OtherCategory.Creatures => ModLocalization.Get(ModText.OtherCategoryCreatures), 
			OtherCategory.Events => ModLocalization.Get(ModText.OtherCategoryEvents), 
			_ => string.Empty, 
		};
	}

	private void SelectCategory(OtherCategory category)
	{
		if (_selectedCategory == category && ((Node)_entryList).GetChildCount(false) > 0)
		{
			return;
		}
		_selectedCategory = category;
		((CanvasItem)_nameLabel).Visible = category != OtherCategory.Events;
		_selectedAncient = null;
		_selectedOther = null;
		_otherPreviewRequest++;
		foreach (KeyValuePair<OtherCategory, Button> categoryButton in _categoryButtons)
		{
			ApplyCategoryTheme(categoryButton.Value, categoryButton.Key == category);
		}
		RefreshAncients();
	}

	private void ShowEmptyCategory()
	{
		_nameLabel.Text = ModLocalization.Get(ModText.NoAncientsAvailable);
		_epithetLabel.Text = string.Empty;
		((CanvasItem)_skinSelector).Visible = false;
		RebuildOtherActionButtons(null);
		ClearPreview();
	}

	private static OtherEntry[] GetOtherEntries(OtherCategory category)
	{
		return category switch
		{
			OtherCategory.Merchants => new OtherEntry[2]
			{
				new OtherEntry("merchant", GetLocalizedTitle("map", "LEGEND_MERCHANT.title", "商人"), "res://scenes/rooms/merchant_room.tscn"),
				new OtherEntry("fake_merchant_monster", GetLocalizedTitle("events", "FAKE_MERCHANT.title", "商人？？？"), "res://scenes/events/custom/fake_merchant.tscn")
			}, 
			OtherCategory.Events => (from model in EventCompendiumPreview.Events()
				select new OtherEntry(EventSkinPolicy.GroupId(((AbstractModel)model).Id.Entry), EventCompendiumPreview.Title(model), "res://scenes/events/default_event_layout.tscn")).ToArray(), 
			OtherCategory.Creatures => OtherCreatureCatalog.All.Select((OtherCreatureDefinition creature) => new OtherEntry(creature.Id, GetLocalizedTitle(creature.LocalizationTable, creature.LocalizationKey, creature.FallbackTitle), creature.ScenePath)).ToArray(), 
			_ => Array.Empty<OtherEntry>(), 
		};
	}

	private static string GetLocalizedTitle(string table, string key, string fallback)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			return new LocString(table, key).GetFormattedText();
		}
		catch
		{
			return fallback;
		}
	}

	private void SelectOther(OtherEntry entry)
	{
		_otherPreviewRequest++;
		_selectedAncient = null;
		_selectedOther = entry;
		_nameLabel.Text = entry.Title;
		_epithetLabel.Text = string.Empty;
		foreach (KeyValuePair<string, Button> otherEntryButton in _otherEntryButtons)
		{
			ApplyEntryTheme(otherEntryButton.Value, otherEntryButton.Key.Equals(entry.Id, StringComparison.OrdinalIgnoreCase));
		}
		RebuildOtherActionButtons(OtherCreatureCatalog.Find(entry.Id));
		SkinGroup group = FindOtherGroup(entry);
		FocusPreviewRuntimeProviderBehaviors(group, "其它图鉴预览");
		PopulateSkinDropdown(group);
		RebuildOtherPreview(entry);
		RefreshAuxiliaryPreviewControls();
	}

	private static SkinGroup? FindOtherGroup(OtherEntry entry)
	{
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog == null)
		{
			return null;
		}
		string lookup = entry.GroupLookupId ?? entry.Id;
		return catalog.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(lookup, StringComparison.OrdinalIgnoreCase));
	}

	private void ApplyCategoryTheme(Button button, bool selected)
	{
		_backdrop.SetSelected(button, selected);
		ModThemeRuntime.ListButton(button, selected, 19);
	}

	private void SelectAncient(AncientEventModel ancient)
	{
		_selectedCategory = OtherCategory.Ancients;
		_selectedAncient = ancient;
		_selectedOther = null;
		RebuildOtherActionButtons(null);
		_nameLabel.Text = AncientCompendiumEntry.GetTitle(ancient);
		try
		{
			_epithetLabel.Text = ancient.Epithet.GetFormattedText();
		}
		catch
		{
			_epithetLabel.Text = string.Empty;
		}
		foreach (KeyValuePair<AncientEventModel, Button> entryButton in _entryButtons)
		{
			ApplyEntryTheme(entryButton.Value, ((AbstractModel)entryButton.Key).Id == ((AbstractModel)ancient).Id);
		}
		SkinGroup group = AncientCompendiumEntry.FindGroup(((AbstractModel)ancient).Id.Entry);
		FocusPreviewRuntimeProviderBehaviors(group, "先古图鉴预览");
		PopulateSkinDropdown(group);
		RebuildPreview(ancient);
		RefreshAuxiliaryPreviewControls();
	}

	private static void FocusPreviewRuntimeProviderBehaviors(SkinGroup? group, string reason)
	{
		if (NRun.Instance != null)
		{
			IEnumerable<string> additionalGroupIds;
			if (group != null)
			{
				IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(group.Id);
				additionalGroupIds = enumerable;
			}
			else
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				additionalGroupIds = enumerable;
			}
			CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(additionalGroupIds, reason, null, null, refreshCurrentRoom: true);
		}
		else
		{
			IEnumerable<string> groupIds;
			if (group != null)
			{
				IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(group.Id);
				groupIds = enumerable;
			}
			else
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				groupIds = enumerable;
			}
			SkinService.FocusRuntimeProviderBehaviorsOnGroups(groupIds, (IReadOnlyCollection<string>)(object)Array.Empty<string>(), reason);
		}
	}

	private void PopulateSkinDropdown(SkinGroup? group)
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		string id = group?.Id ?? ((_selectedAncient != null) ? ((AbstractModel)_selectedAncient).Id.Entry.ToLowerInvariant() : _selectedOther?.Id);
		group = SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup g) => g.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) ?? group;
		if (group == null && id != null)
		{
			group = new SkinGroup(id, id);
		}
		_updatingDropdown = true;
		_skinDropdown.Clear();
		if (group == null)
		{
			((CanvasItem)_skinSelector).Visible = false;
			_updatingDropdown = false;
			return;
		}
		bool flag = EventSkinPolicy.IsEventGroup(group.Id) && SkinService.HasEventSkinCategory(group.Id);
		if (flag)
		{
			_skinDropdown.AddItem(ModLocalization.Get(ModText.FollowCategory), -1);
			_skinDropdown.SetItemMetadata(0, Variant.op_Implicit("__event_category__"));
		}
		int itemCount = _skinDropdown.ItemCount;
		_skinDropdown.AddItem(ModLocalization.Get(ModText.GameDefault), -1);
		_skinDropdown.SetItemMetadata(itemCount, Variant.op_Implicit("__base__"));
		var (readOnlyList, states) = SkinService.GetSkinListForDisplay(group.Id, group.Options);
		SkinChoiceDimming.Bind(_skinDropdown, states);
		foreach (SkinOption item in readOnlyList)
		{
			int itemCount2 = _skinDropdown.ItemCount;
			_skinDropdown.AddItem(ModLocalization.DisplayOptionName(item.Name), -1);
			_skinDropdown.SetItemMetadata(itemCount2, Variant.op_Implicit(item.Id));
		}
		SkinWorkshopEntry.Append(_skinDropdown);
		PresetChoiceColoring.Attach(_skinDropdown, SkinOptionStylePolicy.IsAccented, colorSelection: false);
		string current2 = (flag ? SkinService.GetEventOverrideSelection(group.Id) : SkinService.Config.GetSelection(group.Id));
		int num = Enumerable.Range(0, _skinDropdown.ItemCount).FirstOrDefault(delegate(int index)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = _skinDropdown.GetItemMetadata(index);
			return ((Variant)(ref itemMetadata)).AsString().Equals(current2, StringComparison.OrdinalIgnoreCase);
		});
		_skinDropdown.Select(num);
		SkinWorkshopEntry.RefreshSelectionColor(_skinDropdown);
		((GodotObject)_skinDropdown).SetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(group.Id));
		_updatingDropdown = false;
		((CanvasItem)_skinSelector).Visible = !_merchantInventoryOpen;
	}

	private void OnSkinSelected(int index)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		if (_updatingDropdown || (_selectedAncient == null && _selectedOther == null))
		{
			return;
		}
		Variant val = ((GodotObject)_skinDropdown).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
		string groupId = ((Variant)(ref val)).AsString();
		val = _skinDropdown.GetItemMetadata(index);
		string text = ((Variant)(ref val)).AsString();
		string optionId = text;
		string groupId2 = groupId;
		Action refresh = delegate
		{
			PopulateSkinDropdown(SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup g) => g.Id == groupId));
		};
		if (SkinWorkshopEntry.Open(optionId, (Node)(object)this, groupId2, refresh, cards: false, _selectedCategory switch
		{
			OtherCategory.Ancients => "ancient", 
			OtherCategory.Merchants => "merchant", 
			OtherCategory.Events => "event", 
			_ => "companion", 
		}))
		{
			return;
		}
		Callable val2;
		if (!SkinService.ApplySelection(groupId, text))
		{
			ModLog.Error("其它图鉴皮肤切换失败：" + SkinService.LastError);
			PopulateSkinDropdown((_selectedAncient != null) ? AncientCompendiumEntry.FindGroup(((AbstractModel)_selectedAncient).Id.Entry) : FindOtherGroup(_selectedOther));
		}
		else if (_selectedAncient != null)
		{
			AncientCompendiumEntry.RefreshCompendiumEntryIcon((Node)(object)this);
			AncientEventModel ancient = _selectedAncient;
			val2 = Callable.From((Action)delegate
			{
				RebuildPreview(ancient);
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		}
		else
		{
			if (!(_selectedOther != null))
			{
				return;
			}
			OtherEntry entry = _selectedOther;
			int request = ++_otherPreviewRequest;
			string expectedOption = SkinService.Config.GetSelection(groupId);
			val2 = Callable.From((Action)delegate
			{
				if (request == _otherPreviewRequest && !(_selectedOther == null) && _selectedOther.Id.Equals(entry.Id, StringComparison.OrdinalIgnoreCase) && SkinService.Config.GetSelection(groupId).Equals(expectedOption, StringComparison.OrdinalIgnoreCase))
				{
					RebuildOtherPreview(entry);
				}
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private void RebuildOtherPreview(OtherEntry entry)
	{
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Expected O, but got Unknown
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ClearPreview();
			_otherPreviewInstance = null;
			_otherPreviewGroupId = null;
			if (EventSkinPolicy.IsEventGroup(entry.Id))
			{
				EventModel model = EventCompendiumPreview.Events().First((EventModel candidate) => EventSkinPolicy.GroupId(((AbstractModel)candidate).Id.Entry) == entry.Id);
				((Control)_previewContainer).MouseFilter = (MouseFilterEnum)0;
				((Viewport)_previewViewport).GuiDisableInput = false;
				((Node)_previewViewport).AddChild((Node)(object)EventCompendiumPreview.Create(model), false, (InternalMode)0);
				if (_otherEntryButtons.TryGetValue(entry.Id, out Button value))
				{
					value.Text = EventCompendiumPreview.Title(model);
				}
				return;
			}
			SkinGroup skinGroup = FindOtherGroup(entry);
			OtherCreatureDefinition otherCreatureDefinition = OtherCreatureCatalog.Find(entry.Id);
			bool flag = entry.Id.Equals("merchant", StringComparison.OrdinalIgnoreCase);
			bool flag2 = entry.Id.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase);
			bool flag3 = flag || flag2;
			((Control)_previewContainer).MouseFilter = (MouseFilterEnum)(flag3 ? 0 : 2);
			((Viewport)_previewViewport).GuiDisableInput = !flag3;
			Node val;
			(Node, string) tuple2;
			if (flag || flag2)
			{
				(Node Scene, string ScenePath) tuple = MerchantRuntimeAppearance.InstantiateMerchantPreviewScene(flag2 ? "fake_merchant_monster" : "merchant");
				val = tuple.Scene;
				tuple2 = tuple;
			}
			else
			{
				tuple2 = InstantiateOtherPreviewScene(entry, skinGroup);
				(val, _) = tuple2;
			}
			if (otherCreatureDefinition != null)
			{
				val = ExtractPreviewNode(val, "Visuals", "SpineSprite");
			}
			if (!entry.Id.Equals("merchant", StringComparison.OrdinalIgnoreCase) && !entry.Id.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase))
			{
				val.Name = StringName.op_Implicit("OtherCompendiumPreview");
			}
			val.ProcessMode = (ProcessModeEnum)3;
			using ((skinGroup == null) ? null : SkinService.BeginRuntimeResourceScope(skinGroup.Id, tuple2.Item2))
			{
				if (flag || flag2)
				{
					Control val2 = new Control
					{
						Name = StringName.op_Implicit("MerchantPreviewHost"),
						MouseFilter = (MouseFilterEnum)2,
						ProcessMode = (ProcessModeEnum)3
					};
					val2.SetAnchorsAndOffsetsPreset((LayoutPreset)0, (LayoutPresetMode)0, 0);
					val2.Size = new Vector2((float)PreviewLogicalSize.X, (float)PreviewLogicalSize.Y);
					((Node)_previewViewport).AddChild((Node)(object)val2, false, (InternalMode)0);
					((Node)val2).AddChild(val, false, (InternalMode)0);
				}
				else
				{
					((Node)_previewViewport).AddChild(val, false, (InternalMode)0);
				}
				if (entry.Id.Equals("merchant", StringComparison.OrdinalIgnoreCase) || entry.Id.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase))
				{
					if (!flag)
					{
						goto IL_02cc;
					}
					NMerchantRoom val3 = (NMerchantRoom)(object)((val is NMerchantRoom) ? val : null);
					if (val3 == null)
					{
						goto IL_02cc;
					}
					CaptureMerchantInventoryBackButton(MerchantRuntimeAppearance.PrepareMerchantPreviewInteraction(val3, SetMerchantInventoryOpen));
				}
				goto end_IL_01e8;
				IL_02cc:
				if (flag2)
				{
					NFakeMerchant val4 = (NFakeMerchant)(object)((val is NFakeMerchant) ? val : null);
					if (val4 != null)
					{
						CaptureMerchantInventoryBackButton(MerchantRuntimeAppearance.PrepareFakeMerchantPreviewInteraction(val4, SetMerchantInventoryOpen));
					}
				}
				end_IL_01e8:;
			}
			if (flag || flag2)
			{
				MakePreviewCanvasVisible(val);
				Node obj = val.FindChild("MerchantVisual", true, false);
				CanvasItem val5 = (CanvasItem)(object)((obj is CanvasItem) ? obj : null);
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 4);
				defaultInterpolatedStringHandler.AppendLiteral("商人预览节点 ");
				defaultInterpolatedStringHandler.AppendFormatted(entry.Id);
				defaultInterpolatedStringHandler.AppendLiteral(": rootVisible=");
				CanvasItem val6 = (CanvasItem)(object)((val is CanvasItem) ? val : null);
				defaultInterpolatedStringHandler.AppendFormatted(val6 != null && val6.Visible);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				defaultInterpolatedStringHandler.AppendLiteral("visualVisible=");
				defaultInterpolatedStringHandler.AppendFormatted((val5 != null) ? new bool?(val5.Visible) : ((bool?)null));
				defaultInterpolatedStringHandler.AppendLiteral(", visualModulate=");
				defaultInterpolatedStringHandler.AppendFormatted((val5 != null) ? new Color?(val5.Modulate) : ((Color?)null));
				ModLog.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			_otherPreviewInstance = val;
			_otherPreviewGroupId = skinGroup?.Id;
			Control val7 = (Control)(object)((val is Control) ? val : null);
			if (val7 != null)
			{
				if (!flag && !flag2)
				{
					val7.SetAnchorsAndOffsetsPreset((LayoutPreset)8, (LayoutPresetMode)0, 0);
					val7.Position = new Vector2(960f, 540f);
					val7.Scale *= 1.35f;
				}
				val7.MouseFilter = (MouseFilterEnum)2;
			}
			else
			{
				Node2D val8 = (Node2D)(object)((val is Node2D) ? val : null);
				if (val8 != null)
				{
					val8.Position = new Vector2(960f, 600f);
					val8.Scale *= 1.35f;
				}
			}
			if (!flag && !flag2)
			{
				ManagedAncientSceneAnimation.TryStart(skinGroup?.Id, val);
				ManagedAncientSceneAnimation.ConfigureActions(val, skinGroup?.Id, _otherActionButtons);
				if (otherCreatureDefinition != null)
				{
					ManagedAncientSceneAnimation.TryPlay(val, skinGroup?.Id, otherCreatureDefinition.IdleAliases, loop: true);
				}
			}
			RefreshAuxiliaryPreviewControls();
			ModLog.Info("其它图鉴已展示 " + entry.Id + "。");
		}
		catch (Exception value2)
		{
			ModLog.Error($"其它图鉴预览 {entry.Id} 失败：{value2}");
		}
	}

	private static (Node Instance, string ScenePath) InstantiateOtherPreviewScene(OtherEntry entry, SkinGroup? group)
	{
		string[] source = ((!entry.Id.Equals("merchant", StringComparison.OrdinalIgnoreCase)) ? ((!entry.Id.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase)) ? new string[1] { entry.ScenePath } : new string[2] { "res://scenes/events/custom/fake_merchant_button.tscn", entry.ScenePath }) : new string[2] { "res://scenes/rooms/merchant_button.tscn", entry.ScenePath });
		Exception innerException = null;
		foreach (string item in source.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				if (group != null)
				{
					return (Instance: SkinService.InstantiateRuntimeScene<Node>(group.Id, item, (Action?)null), ScenePath: item);
				}
				PackedScene val = ResourceLoader.Load<PackedScene>(item, (string)null, (CacheMode)4);
				if (val != null)
				{
					return (Instance: val.Instantiate((GenEditState)0), ScenePath: item);
				}
				innerException = new InvalidOperationException("无法加载其它图鉴场景：" + item);
			}
			catch (Exception ex)
			{
				innerException = ex;
			}
		}
		throw new InvalidOperationException("无法加载其它图鉴场景：" + entry.Id + "。", innerException);
	}

	private static Node ExtractPreviewNode(Node root, params string[] names)
	{
		Node val = (Node)(names.Any((string name) => ((object)root.Name).ToString().Equals(name, StringComparison.OrdinalIgnoreCase)) ? ((object)root) : ((object)names.Select((string name) => root.FindChild(name, true, false)).FirstOrDefault((Func<Node, bool>)((Node node) => node != null))));
		if (val == null || val.GetParent() == null)
		{
			if (val == root)
			{
				ReownDetachedScene(val);
				MakePreviewCanvasVisible(val);
			}
			return root;
		}
		val.GetParent().RemoveChild(val);
		ReownDetachedScene(val);
		MakePreviewCanvasVisible(val);
		((GodotObject)root).Free();
		return val;
	}

	private static void MakePreviewCanvasVisible(Node root)
	{
		Reveal(root);
		bool flag = MerchantFigureScopePolicy.ShouldRevealGameFigure(PartTakeoverVisualScope.SelectedPartDrawsFigure(root));
		string[] array = new string[3] { "MerchantVisual", "Visuals", "SpineSprite" };
		foreach (string text in array)
		{
			if (!text.Equals("MerchantVisual", StringComparison.Ordinal) || flag)
			{
				Node val = root.FindChild(text, true, false);
				if (val != null)
				{
					Reveal(val);
				}
			}
		}
		static void Reveal(Node node)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			CanvasItem val2 = (CanvasItem)(object)((node is CanvasItem) ? node : null);
			if (val2 != null)
			{
				val2.Visible = true;
				val2.Modulate = Colors.White;
				val2.SelfModulate = Colors.White;
			}
		}
	}

	private static void ReownDetachedScene(Node node)
	{
		node.Owner = null;
		foreach (Node item in ((IEnumerable)node.GetChildren(false)).OfType<Node>())
		{
			item.Owner = node;
			ReownDescendantOwnership(item, node);
		}
	}

	private static void ReownDescendantOwnership(Node node, Node owner)
	{
		foreach (Node item in ((IEnumerable)node.GetChildren(false)).OfType<Node>())
		{
			item.Owner = owner;
			ReownDescendantOwnership(item, owner);
		}
	}

	private void RebuildPreview(AncientEventModel ancient)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		try
		{
			ClearPreview();
			string scenePath = AncientCompendiumEntry.GetScenePath(ancient);
			SkinGroup skinGroup = AncientCompendiumEntry.FindGroup(((AbstractModel)ancient).Id.Entry);
			bool flag = skinGroup != null && SkinService.IsInteractiveRuntimeProviderSelected(skinGroup.Id);
			((Control)_previewContainer).MouseFilter = (MouseFilterEnum)(flag ? 1 : 2);
			((Viewport)_previewViewport).GuiDisableInput = !flag;
			PackedScene val = (PackedScene)((skinGroup != null && SkinService.IsExternalRuntimeProviderSelected(skinGroup.Id)) ? BuildSelectedRuntimeImageScene(skinGroup.Id) : ((skinGroup == null) ? ((object)(ResourceLoader.Load<PackedScene>(scenePath, (string)null, (CacheMode)3) ?? throw new InvalidOperationException("无法加载先古场景：" + scenePath))) : ((object)SkinService.LoadRuntimeScene(skinGroup.Id, scenePath))));
			Control val2 = val.Instantiate<Control>((GenEditState)0);
			val2.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			Control val3 = new Control
			{
				Name = StringName.op_Implicit("PreviewHost"),
				MouseFilter = (MouseFilterEnum)1
			};
			val3.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			((Node)_previewViewport).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)val3).AddChild((Node)(object)val2, false, (InternalMode)0);
			ManagedAncientLayeredImage.TryApply(skinGroup?.Id, (Node)(object)val2);
			ManagedAncientSceneAnimation.TryStart(skinGroup?.Id, (Node)(object)val2);
			ModLog.Info("其它图鉴已展示先古 " + ((AbstractModel)ancient).Id.Entry + "。");
		}
		catch (Exception value)
		{
			ModLog.Error($"其它图鉴预览先古 {((AbstractModel)ancient).Id.Entry} 失败：{value}");
		}
	}

	internal static PackedScene BuildSelectedRuntimeImageScene(string groupId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Control val = new Control
		{
			Name = StringName.op_Implicit("RuntimeAncientBackground")
		};
		val.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		ManagedAncientStaticBackground.Mark(val);
		TextureRect val2 = new TextureRect
		{
			Name = StringName.op_Implicit("Image"),
			Texture = SkinService.GetSelectedRuntimeImageTexture(groupId),
			ExpandMode = (ExpandModeEnum)1,
			StretchMode = (StretchModeEnum)6,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val2).Owner = (Node)(object)val;
		PackedScene val3 = new PackedScene();
		Error val4 = val3.Pack((Node)(object)val);
		((GodotObject)val).Free();
		if ((int)val4 != 0)
		{
			throw new InvalidOperationException($"无法创建先古图片场景：{val4}");
		}
		return val3;
	}

	private void ClearPreview()
	{
		SetMerchantInventoryOpen(isOpen: false);
		_otherPreviewInstance = null;
		_otherPreviewGroupId = null;
		if (_otherActionSelector != null)
		{
			((CanvasItem)_otherActionSelector).Visible = false;
		}
		((Control)_previewContainer).MouseFilter = (MouseFilterEnum)2;
		((Viewport)_previewViewport).GuiDisableInput = true;
		foreach (Node child in ((Node)_previewViewport).GetChildren(false))
		{
			CanvasItem val = (CanvasItem)(object)((child is CanvasItem) ? child : null);
			if (val != null)
			{
				val.Visible = false;
			}
			child.ProcessMode = (ProcessModeEnum)4;
			((Node)_previewViewport).RemoveChild(child);
			child.QueueFree();
		}
		_merchantInventoryBackButton = null;
		_merchantInventoryBackButtonParent = null;
		_merchantInventoryBackButtonIndex = -1;
	}

	private static Label BuildLabel(int fontSize, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Label val = new Label
		{
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), color);
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), (fontSize >= 34) ? 10 : 5);
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), fontSize);
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		ModThemeRuntime.TextControl((Control)(object)val, fontSize, color == new Color("efc850"));
		return val;
	}

	private void ApplyEntryTheme(Button button, bool selected)
	{
		_backdrop.SetSelected(button, selected);
		ModThemeRuntime.ListButton(button, selected, 24);
	}

	private void BuildEventPriorityControls()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Expected O, but got Unknown
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Expected O, but got Unknown
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Expected O, but got Unknown
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Expected O, but got Unknown
		_eventPriorityHeader = new HBoxContainer
		{
			Name = StringName.op_Implicit("EventSkinPriorityHeader"),
			Position = new Vector2(70f, 884f),
			Visible = false,
			ZIndex = 10,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)_eventPriorityHeader).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		((Node)this).AddChild((Node)(object)_eventPriorityHeader, false, (InternalMode)0);
		_eventPriorityRegionName = new Label
		{
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		ModThemeRuntime.TextControl((Control)(object)_eventPriorityRegionName, 23, accent: true);
		((Node)_eventPriorityHeader).AddChild((Node)(object)_eventPriorityRegionName, false, (InternalMode)0);
		_eventPriorityButton = new Button
		{
			CustomMinimumSize = new Vector2(190f, 44f)
		};
		ContextualSkinControls.ApplyGameTheme(_eventPriorityButton);
		((Node)_eventPriorityHeader).AddChild((Node)(object)_eventPriorityButton, false, (InternalMode)0);
		_eventPriorityOverlay = new Control
		{
			Name = StringName.op_Implicit("EventSkinPriorityOverlay"),
			Visible = false,
			ZIndex = 2000,
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)this).AddChild((Node)(object)_eventPriorityOverlay, false, (InternalMode)0);
		_eventPriorityOverlay.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		ColorRect mask = new ColorRect
		{
			Color = new Color(0f, 0f, 0f, 0.68f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Node)_eventPriorityOverlay).AddChild((Node)(object)mask, false, (InternalMode)0);
		((Control)mask).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)mask).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && val.Pressed && (long)val.ButtonIndex == 1)
			{
				((CanvasItem)_eventPriorityOverlay).Hide();
				((Control)mask).AcceptEvent();
			}
		};
		PanelContainer val2 = new PanelContainer
		{
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			AnchorTop = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -360f,
			OffsetRight = 360f,
			OffsetTop = -260f,
			OffsetBottom = 260f,
			MouseFilter = (MouseFilterEnum)0
		};
		ModThemeRuntime.Panel(val2);
		((Node)_eventPriorityOverlay).AddChild((Node)(object)val2, false, (InternalMode)0);
		MarginContainer val3 = new MarginContainer();
		string[] array = new string[4] { "left", "right", "top", "bottom" };
		foreach (string text in array)
		{
			((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 20);
		}
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		_eventPriorityContent = new VBoxContainer();
		((Control)_eventPriorityContent).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val3).AddChild((Node)(object)_eventPriorityContent, false, (InternalMode)0);
		((BaseButton)_eventPriorityButton).Pressed += delegate
		{
			BuildEventPriorityList();
			((CanvasItem)_eventPriorityOverlay).Show();
			((CanvasItem)_eventPriorityOverlay).MoveToFront();
		};
		((CanvasItem)_eventPriorityOverlay).VisibilityChanged += delegate
		{
			_sidebarDrawer?.Refresh();
		};
		ModLocalization.Bind((Node)(object)_eventPriorityOverlay, delegate
		{
			RefreshEventPriorityHeader();
			if (((CanvasItem)_eventPriorityOverlay).Visible)
			{
				BuildEventPriorityList();
			}
		});
	}

	private void RefreshEventPriorityHeader()
	{
		if (_eventPriorityHeader != null)
		{
			((CanvasItem)_eventPriorityHeader).Visible = _selectedCategory == OtherCategory.Events && _eventRegion != null && SkinService.GetEventPriorityOptions(_eventRegion).Count > 0;
			_eventPriorityButton.Text = ModLocalization.Get(ModText.MonsterSkinPriority);
			_eventPriorityRegionName.Text = ((_eventRegionSelector.Selected < 0) ? "" : _eventRegionSelector.GetItemText(_eventRegionSelector.Selected));
			if (!((CanvasItem)_eventPriorityHeader).Visible)
			{
				((CanvasItem)_eventPriorityOverlay).Hide();
			}
		}
	}

	private void BuildEventPriorityList()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Expected O, but got Unknown
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Expected O, but got Unknown
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected O, but got Unknown
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Expected O, but got Unknown
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Expected O, but got Unknown
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Expected O, but got Unknown
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Expected O, but got Unknown
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Expected O, but got Unknown
		string region = _eventRegion;
		if (region == null)
		{
			return;
		}
		ScrollContainer obj = ScrollListRebuild.Begin(_eventPriorityContent, region);
		Label val = new Label
		{
			Text = _eventPriorityRegionName.Text + " · " + ModLocalization.Get(ModText.MonsterSkinPriority),
			HorizontalAlignment = (HorizontalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		ModThemeRuntime.TextControl((Control)(object)val, 25, accent: true);
		((Node)_eventPriorityContent).AddChild((Node)(object)val, false, (InternalMode)0);
		((Control)obj).CustomMinimumSize = new Vector2(670f, 350f);
		((Control)obj).SizeFlagsVertical = (SizeFlags)3;
		ScrollListRebuild.PlaceAfterHeader(obj);
		VBoxContainer val2 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Node)obj).AddChild((Node)(object)val2, false, (InternalMode)0);
		IReadOnlyList<SkinService.EventPriorityOptionState> eventPriorityOptions = SkinService.GetEventPriorityOptions(region);
		foreach (SkinService.EventPriorityOptionState option in eventPriorityOptions)
		{
			HBoxContainer val3 = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(650f, 42f)
			};
			((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
			CheckBox val4 = new CheckBox
			{
				Text = ModLocalization.Get(ModText.EnabledForCategory),
				ButtonPressed = option.Enabled,
				CustomMinimumSize = new Vector2(100f, 36f)
			};
			ContextualSkinControls.ApplyGameTheme((Button)(object)val4);
			((Control)val4).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 17);
			((BaseButton)val4).Toggled += (ToggledEventHandler)delegate(bool value)
			{
				QueueEventPriorityChange(region, () => SkinService.SetEventPriorityOptionEnabled(region, option.OptionId, value));
			};
			((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			Label val5 = new Label
			{
				Text = ModLocalization.DisplayOptionName(option.Name),
				ClipText = true,
				TextOverrunBehavior = (OverrunBehavior)3,
				SizeFlagsHorizontal = (SizeFlags)3,
				CustomMinimumSize = new Vector2(260f, 36f),
				VerticalAlignment = (VerticalAlignment)1
			};
			ModThemeRuntime.TextControl((Control)(object)val5, 18);
			((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
			Label val6 = new Label
			{
				Text = $"{option.Coverage}/{option.TotalEvents}",
				CustomMinimumSize = new Vector2(66f, 36f),
				HorizontalAlignment = (HorizontalAlignment)2,
				VerticalAlignment = (VerticalAlignment)1
			};
			ModThemeRuntime.TextControl((Control)(object)val6, 16);
			((Node)val3).AddChild((Node)(object)val6, false, (InternalMode)0);
			int[] array = new int[2] { -1, 1 };
			foreach (int direction in array)
			{
				Button val7 = new Button
				{
					Text = ((direction < 0) ? "↑" : "↓"),
					CustomMinimumSize = new Vector2(42f, 34f)
				};
				bool disabled;
				if (direction >= 0)
				{
					disabled = option == eventPriorityOptions[eventPriorityOptions.Count - 1];
				}
				else
				{
					disabled = option == eventPriorityOptions[0];
				}
				((BaseButton)val7).Disabled = disabled;
				Button val8 = val7;
				ContextualSkinControls.ApplyGameTheme(val8);
				((BaseButton)val8).Pressed += delegate
				{
					QueueEventPriorityChange(region, () => SkinService.MoveEventPriority(region, option.OptionId, direction));
				};
				((Node)val3).AddChild((Node)(object)val8, false, (InternalMode)0);
			}
		}
		Button val9 = new Button
		{
			Text = ModLocalization.Get(ModText.Close),
			CustomMinimumSize = new Vector2(180f, 42f),
			SizeFlagsHorizontal = (SizeFlags)4
		};
		ContextualSkinControls.ApplyGameTheme(val9);
		((BaseButton)val9).Pressed += delegate
		{
			((CanvasItem)_eventPriorityOverlay).Hide();
		};
		HBoxContainer val10 = new HBoxContainer();
		((Node)val10).AddChild((Node)(object)val9, false, (InternalMode)0);
		((Node)val10).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)val10).AddChild((Node)(object)SkinWorkshopEntry.CreatePriorityButton((Node)(object)this, "event", "", BuildEventPriorityList, region), false, (InternalMode)0);
		((Node)_eventPriorityContent).AddChild((Node)(object)val10, false, (InternalMode)0);
	}

	private void QueueEventPriorityChange(string region, Func<bool> apply)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (_eventPriorityPending)
		{
			return;
		}
		_eventPriorityPending = true;
		Callable val = Callable.From((Action)delegate
		{
			try
			{
				if (((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion() && !(_eventRegion != region))
				{
					if (!apply())
					{
						ModLog.Error("事件优先级修改失败：" + SkinService.LastError);
					}
					RefreshAncients();
					BuildEventPriorityList();
				}
			}
			finally
			{
				_eventPriorityPending = false;
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void BuildEventRegionSelector(VBoxContainer sidebar)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		_eventRegionSelector = new OptionButton
		{
			Name = StringName.op_Implicit("EventRegionSelector"),
			Visible = false,
			FitToLongestItem = false,
			ClipText = true,
			Alignment = (HorizontalAlignment)1,
			CustomMinimumSize = new Vector2(0f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ContextualSkinControls.ApplyGameTheme(_eventRegionSelector);
		((Node)sidebar).AddChild((Node)(object)_eventRegionSelector, false, (InternalMode)0);
		_eventRegionSelector.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if (!_refreshingEventRegions)
			{
				Variant itemMetadata = _eventRegionSelector.GetItemMetadata(checked((int)index));
				_eventRegion = ((Variant)(ref itemMetadata)).AsString();
				_selectedOther = null;
				RefreshAncients();
			}
		};
	}

	private void RefreshEventRegions()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		((CanvasItem)_eventRegionSelector).Visible = _selectedCategory == OtherCategory.Events;
		((Control)_skinSelector).Position = ((_selectedCategory == OtherCategory.Events) ? new Vector2(70f, 940f) : new Vector2(818f, 826f));
		if (_selectedCategory != OtherCategory.Events)
		{
			RefreshEventPriorityHeader();
			return;
		}
		ActModel[] source = ModelDb.Acts.ToArray();
		IReadOnlyDictionary<string, string[]> readOnlyDictionary = EventRegionPolicy.Group(from e in EventCompendiumPreview.Events()
			select ((AbstractModel)e).Id.Entry, source.ToDictionary((ActModel a) => ((AbstractModel)a).Id.Entry, (ActModel a) => a.AllEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry)), ModelDb.AllSharedEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry));
		(string, string) valueOrDefault = EventRegionNames.GetValueOrDefault(ModLocalization.CurrentLanguage, EventRegionNames["eng"]);
		Dictionary<string, string> dictionary = source.ToDictionary((ActModel a) => ((AbstractModel)a).Id.Entry, (ActModel a) => a.Title.GetFormattedText());
		dictionary["__shared__"] = valueOrDefault.Item1;
		dictionary["__other__"] = valueOrDefault.Item2;
		_refreshingEventRegions = true;
		try
		{
			_eventRegionSelector.Clear();
			if (_eventRegion == null || !readOnlyDictionary.ContainsKey(_eventRegion))
			{
				_eventRegion = readOnlyDictionary.Keys.FirstOrDefault();
			}
			foreach (KeyValuePair<string, string[]> item in readOnlyDictionary)
			{
				item.Deconstruct(out var key, out var _);
				string text = key;
				int itemCount = _eventRegionSelector.ItemCount;
				_eventRegionSelector.AddItem(dictionary[text], -1);
				_eventRegionSelector.SetItemMetadata(itemCount, Variant.op_Implicit(text));
				if (text == _eventRegion)
				{
					_eventRegionSelector.Select(itemCount);
				}
			}
			_eventRegionMembers = ((_eventRegion != null) ? ((IEnumerable<string>)readOnlyDictionary[_eventRegion]) : ((IEnumerable<string>)Array.Empty<string>())).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		finally
		{
			_refreshingEventRegions = false;
		}
		RefreshEventPriorityHeader();
	}
}
