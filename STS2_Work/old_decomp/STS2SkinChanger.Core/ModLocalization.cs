using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Localization;

namespace STS2SkinChanger.Core;

internal static class ModLocalization
{
	private sealed record AppearanceTargetHintPack(string Template, string Empty, string Separator, string[] Names);

	private sealed record LanguagePack(string AncientCompendium, string NoAncientsAvailable, string GameDefault, string GameOriginal, string SkinnedCardsOnly, string CardCategorySkinTooltip, string IndividualCardTooltip, string FollowCategory, string MonsterSize, string Reset, string LoadOrderTitle, string LoadOrderMessage, string Acknowledge, string DoNotShowAgain, string PrioritizeAndRestart, string LoadOrderFailure, string DefaultVariant)
	{
		public string Get(ModText text)
		{
			return text switch
			{
				ModText.AncientCompendium => AncientCompendium, 
				ModText.NoAncientsAvailable => NoAncientsAvailable, 
				ModText.GameDefault => GameDefault, 
				ModText.GameOriginal => GameOriginal, 
				ModText.SkinnedCardsOnly => SkinnedCardsOnly, 
				ModText.CardCategorySkinTooltip => CardCategorySkinTooltip, 
				ModText.IndividualCardTooltip => IndividualCardTooltip, 
				ModText.FollowCategory => "↷" + FollowCategory, 
				ModText.MonsterSize => MonsterSize, 
				ModText.Reset => Reset, 
				ModText.LoadOrderTitle => LoadOrderTitle, 
				ModText.LoadOrderMessage => LoadOrderMessage, 
				ModText.Acknowledge => Acknowledge, 
				ModText.DoNotShowAgain => DoNotShowAgain, 
				ModText.PrioritizeAndRestart => PrioritizeAndRestart, 
				ModText.LoadOrderFailure => LoadOrderFailure, 
				ModText.DefaultVariant => DefaultVariant, 
				_ => throw new ArgumentOutOfRangeException("text", text, null), 
			};
		}
	}

	private sealed record AppearanceLanguagePack(string CharacterAppearance, string SelectAppearanceTarget, string Skin, string CharacterScale, string HorizontalOffset, string VerticalOffset, string HoldToCompare, string AlignmentGuide, string AlignmentHint, string AppearanceApplied, string AppearanceQueued, string NoCombatPreview, string AppearanceFailed)
	{
		public string Get(ModText text)
		{
			return text switch
			{
				ModText.CharacterAppearance => CharacterAppearance, 
				ModText.SelectAppearanceTarget => SelectAppearanceTarget, 
				ModText.Skin => Skin, 
				ModText.CharacterScale => CharacterScale, 
				ModText.HorizontalOffset => HorizontalOffset, 
				ModText.VerticalOffset => VerticalOffset, 
				ModText.HoldToCompare => HoldToCompare, 
				ModText.AlignmentGuide => AlignmentGuide, 
				ModText.AlignmentHint => AlignmentHint, 
				ModText.AppearanceApplied => AppearanceApplied, 
				ModText.AppearanceQueued => AppearanceQueued, 
				ModText.NoCombatPreview => NoCombatPreview, 
				ModText.AppearanceFailed => AppearanceFailed, 
				_ => throw new ArgumentOutOfRangeException("text", text, null), 
			};
		}
	}

	private sealed record CardPriorityLanguagePack(string EnabledForCategory, string CardSkinPriority, string CardPriorityTooltip, string CardArtCoverage, string CurrentCardSource, string AvailableCardSources, string Close, string MonsterSkinPriority, string MonsterPriorityTooltip)
	{
		public string Get(ModText text)
		{
			return text switch
			{
				ModText.CardSkinPriority => CardSkinPriority, 
				ModText.CardPriorityTooltip => CardPriorityTooltip, 
				ModText.CardArtCoverage => CardArtCoverage, 
				ModText.CurrentCardSource => CurrentCardSource, 
				ModText.AvailableCardSources => AvailableCardSources, 
				ModText.EnabledForCategory => EnabledForCategory, 
				ModText.Close => Close, 
				ModText.MonsterSkinPriority => MonsterSkinPriority, 
				ModText.MonsterPriorityTooltip => MonsterPriorityTooltip, 
				_ => throw new ArgumentOutOfRangeException("text", text, null), 
			};
		}
	}

	private sealed record CardPresetLanguagePack(string CardPresets, string CardPresetTooltip, string CardPresetName, string SaveCurrentPreset, string ApplyCardPreset, string OverwriteCardPreset, string RenameCardPreset, string DeleteCardPreset, string ConfirmDeleteCardPreset, string ActiveCardPreset, string NoCardPresets)
	{
		public string Get(ModText text)
		{
			return text switch
			{
				ModText.CardPresets => CardPresets, 
				ModText.CardPresetTooltip => CardPresetTooltip, 
				ModText.CardPresetName => CardPresetName, 
				ModText.SaveCurrentPreset => SaveCurrentPreset, 
				ModText.ApplyCardPreset => ApplyCardPreset, 
				ModText.OverwriteCardPreset => OverwriteCardPreset, 
				ModText.RenameCardPreset => RenameCardPreset, 
				ModText.DeleteCardPreset => DeleteCardPreset, 
				ModText.ConfirmDeleteCardPreset => ConfirmDeleteCardPreset, 
				ModText.ActiveCardPreset => ActiveCardPreset, 
				ModText.NoCardPresets => NoCardPresets, 
				_ => throw new ArgumentOutOfRangeException("text", text, null), 
			};
		}
	}

	private sealed record AdjustmentLanguagePack(string ModelTransform, string HealthBarTransform, string IntentTransform, string SelectionReticleTransform, string HealthBarScale, string FollowModelScale, string FollowModelMovement, string DirectDragHint, string DirectDragIntentHint)
	{
		public string Get(ModText text)
		{
			return text switch
			{
				ModText.ModelTransform => ModelTransform, 
				ModText.HealthBarTransform => HealthBarTransform, 
				ModText.IntentTransform => IntentTransform, 
				ModText.SelectionReticleTransform => SelectionReticleTransform, 
				ModText.HealthBarScale => HealthBarScale, 
				ModText.FollowModelScale => FollowModelScale, 
				ModText.FollowModelMovement => FollowModelMovement, 
				ModText.DirectDragHint => DirectDragHint, 
				ModText.DirectDragIntentHint => DirectDragIntentHint, 
				_ => throw new ArgumentOutOfRangeException("text", text, null), 
			};
		}
	}

	private sealed record OtherCompendiumLanguagePack(string Title, string Ancients, string Merchants, string Creatures);

	private sealed record CardPortraitModeLanguagePack(string Differential, string AncientStyle, string AncientDifferential);

	private sealed record CharacterSkinCompositionLanguagePack(string CharacterSkinMerge, string NewCharacterSkinMerge, string CharacterSkinMergeName, string HideMergedSkinSources, string SaveCharacterSkinMerge, string DeleteCharacterSkinMerge, string ConfirmDeleteCharacterSkinMerge, string CharacterSkinSourceUnavailable, string CharacterSkinMergeNeedsSource, string CombinedSkinDefaultName)
	{
		public string Get(ModText text)
		{
			return text switch
			{
				ModText.CharacterSkinMerge => CharacterSkinMerge, 
				ModText.NewCharacterSkinMerge => NewCharacterSkinMerge, 
				ModText.CharacterSkinMergeName => CharacterSkinMergeName, 
				ModText.HideMergedSkinSources => HideMergedSkinSources, 
				ModText.SaveCharacterSkinMerge => SaveCharacterSkinMerge, 
				ModText.DeleteCharacterSkinMerge => DeleteCharacterSkinMerge, 
				ModText.ConfirmDeleteCharacterSkinMerge => ConfirmDeleteCharacterSkinMerge, 
				ModText.CharacterSkinSourceUnavailable => CharacterSkinSourceUnavailable, 
				ModText.CharacterSkinMergeNeedsSource => CharacterSkinMergeNeedsSource, 
				ModText.CombinedSkinDefaultName => CombinedSkinDefaultName, 
				_ => throw new ArgumentOutOfRangeException("text", text, null), 
			};
		}
	}

	private sealed record MultiplayerProgressLanguagePack(string Preparing, string WaitingForReady, string CheckingWorkshop, string Downloading, string Verifying, string Applying, string Complete, string Failed);

	private sealed record MultiplayerFailureLanguagePack(string Title, string Message);

	private static readonly IReadOnlyDictionary<string, AppearanceTargetHintPack> AppearanceTargetHintPacks = new Dictionary<string, AppearanceTargetHintPack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new AppearanceTargetHintPack("Choose what to customize: {0}.", "No adjustable targets here.", ", ", new string[6] { "characters", "monsters", "boss icons on the map", "Ancients", "companions", "merchants" }),
		["zhs"] = new AppearanceTargetHintPack("请选择要调整外观的目标：{0}。", "当前没有可调整的目标。", "、", new string[6] { "角色", "怪物", "地图上的 Boss 图标", "先古之民", "同伴", "商人" }),
		["zht"] = new AppearanceTargetHintPack("請選擇要調整外觀的目標：{0}。", "目前沒有可調整的目標。", "、", new string[6] { "角色", "怪物", "地圖上的 Boss 圖示", "先古之民", "同伴", "商人" }),
		["deu"] = new AppearanceTargetHintPack("Wähle, was du anpassen möchtest: {0}.", "Hier gibt es keine anpassbaren Ziele.", ", ", new string[6] { "Charaktere", "Monster", "Boss-Symbole auf der Karte", "Uralte", "Begleiter", "Händler" }),
		["esp"] = new AppearanceTargetHintPack("Elige qué personalizar: {0}.", "No hay objetivos que ajustar aquí.", ", ", new string[6] { "personajes", "monstruos", "iconos de jefe del mapa", "Antiguos", "compañeros", "mercaderes" }),
		["spa"] = new AppearanceTargetHintPack("Elige qué personalizar: {0}.", "No hay objetivos que ajustar aquí.", ", ", new string[6] { "personajes", "monstruos", "iconos de jefe del mapa", "Antiguos", "compañeros", "mercaderes" }),
		["fra"] = new AppearanceTargetHintPack("Choisissez quoi personnaliser : {0}.", "Aucune cible à personnaliser ici.", ", ", new string[6] { "personnages", "monstres", "icônes de boss sur la carte", "Anciens", "compagnons", "marchands" }),
		["ita"] = new AppearanceTargetHintPack("Scegli cosa personalizzare: {0}.", "Qui non ci sono bersagli da personalizzare.", ", ", new string[6] { "personaggi", "mostri", "icone dei boss sulla mappa", "Antichi", "compagni", "mercanti" }),
		["jpn"] = new AppearanceTargetHintPack("外見を変更する対象を選択：{0}", "現在、外見を変更できる対象はありません。", "、", new string[6] { "キャラクター", "モンスター", "マップのボスアイコン", "エンシェント", "仲間", "商人" }),
		["kor"] = new AppearanceTargetHintPack("외형을 조정할 대상을 선택하세요: {0}", "현재 조정할 수 있는 대상이 없습니다.", ", ", new string[6] { "캐릭터", "몬스터", "지도의 보스 아이콘", "고대인", "동료", "상인" }),
		["pol"] = new AppearanceTargetHintPack("Wybierz, co dostosować: {0}.", "Brak celów do dostosowania.", ", ", new string[6] { "postacie", "potwory", "ikony bossów na mapie", "Pradawni", "towarzysze", "kupcy" }),
		["ptb"] = new AppearanceTargetHintPack("Escolha o que personalizar: {0}.", "Não há alvos para ajustar aqui.", ", ", new string[6] { "personagens", "monstros", "ícones de chefes no mapa", "Anciões", "companheiros", "mercadores" }),
		["rus"] = new AppearanceTargetHintPack("Выберите, что настроить: {0}.", "Здесь нет доступных целей для настройки.", ", ", new string[6] { "персонажи", "монстры", "значки боссов на карте", "Древние", "спутники", "торговцы" }),
		["tha"] = new AppearanceTargetHintPack("เล\u0e37อกเป\u0e49าหมายท\u0e35\u0e48ต\u0e49องการปร\u0e31บร\u0e39ปล\u0e31กษณ\u0e4c: {0}", "ขณะน\u0e35\u0e49ไม\u0e48ม\u0e35เป\u0e49าหมายท\u0e35\u0e48ปร\u0e31บได\u0e49", ", ", new string[6] { "ต\u0e31วละคร", "มอนสเตอร\u0e4c", "ไอคอนบอสบนแผนท\u0e35\u0e48", "Ancient", "สหาย", "พ\u0e48อค\u0e49า" }),
		["tur"] = new AppearanceTargetHintPack("Özelleştirmek istediğini seç: {0}.", "Burada ayarlanabilecek hedef yok.", ", ", new string[6] { "karakterler", "canavarlar", "haritadaki Boss simgeleri", "Kadimler", "yoldaşlar", "tüccarlar" })
	};

	private static readonly IReadOnlyDictionary<string, (string Cards, string Monsters, string Presets, string Priority)> BundleContentTexts = new Dictionary<string, (string, string, string, string)>
	{
		["eng"] = ("Cards", "Monsters", "Multiple presets", "Mod priority"),
		["zhs"] = ("卡牌", "怪物", "多预设", "Mod 优先级"),
		["zht"] = ("卡牌", "怪物", "多預設", "Mod 優先順序"),
		["deu"] = ("Karten", "Monster", "Mehrere Vorlagen", "Mod-Priorität"),
		["esp"] = ("Cartas", "Monstruos", "Varios preajustes", "Prioridad de mods"),
		["spa"] = ("Cartas", "Monstruos", "Varios preajustes", "Prioridad de mods"),
		["fra"] = ("Cartes", "Monstres", "Plusieurs préréglages", "Priorité des mods"),
		["ita"] = ("Carte", "Mostri", "Più preset", "Priorità delle mod"),
		["jpn"] = ("カード", "モンスター", "複数プリセット", "Mod優先順位"),
		["kor"] = ("카드", "몬스터", "여러 프리셋", "모드 우선순위"),
		["pol"] = ("Karty", "Potwory", "Wiele presetów", "Priorytet modów"),
		["ptb"] = ("Cartas", "Monstros", "Várias predefinições", "Prioridade de mods"),
		["rus"] = ("Карты", "Монстры", "Несколько пресетов", "Приоритет модов"),
		["tha"] = ("การ\u0e4cด", "มอนสเตอร\u0e4c", "หลายพร\u0e35เซ\u0e47ต", "ลำด\u0e31บความสำค\u0e31ญม\u0e47อด"),
		["tur"] = ("Kartlar", "Canavarlar", "Birden çok ön ayar", "Mod önceliği")
	};

	private static readonly IReadOnlyDictionary<string, (string Hide, string Locked)> BundlePresetTexts = new Dictionary<string, (string, string)>
	{
		["eng"] = ("Hide the selected skin from the skin list", "Manage this preset's name and deletion through its skin bundle."),
		["zhs"] = ("在皮肤列表中隐藏选中的皮肤", "请通过皮肤包管理名称和删除；此预设只能应用或覆盖。"),
		["zht"] = ("在外觀清單中隱藏選取的外觀", "請透過組合包管理名稱與刪除；此預設只能套用或覆寫。"),
		["deu"] = ("Gewählten Skin in der Skin-Liste ausblenden", "Name und Löschung werden über das Skin-Paket verwaltet."),
		["esp"] = ("Ocultar el aspecto seleccionado en la lista", "Gestiona el nombre y la eliminación desde el paquete."),
		["spa"] = ("Ocultar el aspecto seleccionado en la lista", "Gestiona el nombre y la eliminación desde el paquete."),
		["fra"] = ("Masquer l’apparence sélectionnée dans la liste", "Le nom et la suppression se gèrent depuis le pack."),
		["ita"] = ("Nascondi la skin scelta dall’elenco", "Gestisci nome ed eliminazione dal pacchetto."),
		["jpn"] = ("選択したスキンを一覧から隠す", "名前の変更と削除はスキンパックから行ってください。"),
		["kor"] = ("목록에서 선택한 스킨 숨기기", "이름 변경과 삭제는 스킨 묶음에서 관리하세요."),
		["pol"] = ("Ukryj wybraną skórkę na liście", "Zarządzaj nazwą i usuwaniem poprzez pakiet skórek."),
		["ptb"] = ("Ocultar a skin selecionada da lista", "Gerencie o nome e a exclusão pelo pacote de skins."),
		["rus"] = ("Скрыть выбранный облик из списка", "Название и удаление управляются через набор обликов."),
		["tha"] = ("ซ\u0e48อนสก\u0e34นท\u0e35\u0e48เล\u0e37อกจากรายการ", "จ\u0e31ดการช\u0e37\u0e48อและการลบผ\u0e48านช\u0e38ดสก\u0e34น"),
		["tur"] = ("Seçilen görünümü listeden gizle", "Adlandırma ve silme işlemlerini görünüm paketinden yönetin.")
	};

	private static readonly IReadOnlyDictionary<string, string[]> BundlePacks = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new string[19]
		{
			"Manage skin bundles", "New bundle", "Bundle name", "Character skin", "Card presets", "Monster presets", "Unchanged", "Apply", "Saved", "Applied",
			"Character skin unavailable; using the original skin.", "Preset unavailable; skipped: {0}", "Bundle unavailable", "Enter a name of 1–40 characters", "A bundle with this name already exists", "Uses existing presets. Choosing a [P] bundle applies its character skin immediately; its card and monster presets apply only during the run and are restored after leaving. In-run changes do not edit the bundle.", "Save your changes before applying", "Applying…", "These skins require linked changes to other characters or regions. Your settings were left unchanged."
		},
		["zhs"] = new string[19]
		{
			"管理皮肤包", "新建皮肤包", "皮肤包名称", "角色皮肤", "卡牌预设", "怪物预设", "不修改", "应用", "已保存", "已应用",
			"角色皮肤不可用，已使用原皮。", "预设不可用，已跳过：{0}", "皮肤包不可用", "请输入 1–40 字的名称", "同名皮肤包已存在", "引用已有预设。在角色皮肤列表选择 [P] 皮肤包后会立即切换角色皮肤；卡牌和怪物预设仅在本局生效，离开本局后恢复。局内修改不会改写皮肤包。", "请先保存修改再应用", "正在应用…", "这些皮肤需要联动修改其他角色或地区，无法按本皮肤包的选择应用；原设置未改变。"
		},
		["zht"] = new string[19]
		{
			"管理外觀組合包", "新增外觀組合包", "組合包名稱", "角色外觀", "卡牌預設", "怪物預設", "不變更", "套用", "已儲存", "已套用",
			"角色外觀無法使用，已使用原始外觀。", "預設無法使用，已略過：{0}", "組合包無法使用", "請輸入 1–40 字的名稱", "同名組合包已存在", "引用現有預設。在角色外觀清單選擇 [P] 組合包後會立即切換角色外觀；卡牌與怪物預設只在本局生效，離開本局後恢復。遊戲內變更不會改寫組合包。", "請先儲存變更再套用", "正在套用…", "這些外觀需要連動變更其他角色或地區，無法依此組合包套用；原設定未變更。"
		},
		["deu"] = new string[19]
		{
			"Skin-Pakete verwalten", "Neues Paket", "Paketname", "Charakter-Skin", "Kartenvorlagen", "Monstervorlagen", "Unverändert", "Anwenden", "Gespeichert", "Angewendet",
			"Charakter-Skin nicht verfügbar; Original wird verwendet.", "Vorlage nicht verfügbar, übersprungen: {0}", "Paket nicht verfügbar", "Namen mit 1–40 Zeichen eingeben", "Ein Paket mit diesem Namen existiert bereits", "Verwendet vorhandene Vorlagen. Ein [P]-Paket wechselt den Charakter-Skin sofort; Karten- und Monstervorlagen gelten nur für diesen Lauf und werden danach zurückgesetzt. Änderungen im Lauf bearbeiten das Paket nicht.", "Änderungen vor dem Anwenden speichern", "Wird angewendet…", "Diese Skins erfordern verknüpfte Änderungen an anderen Charakteren oder Regionen. Die Einstellungen bleiben unverändert."
		},
		["fra"] = new string[19]
		{
			"Gérer les packs d’apparences", "Nouveau pack", "Nom du pack", "Apparence du personnage", "Préréglages de cartes", "Préréglages de monstres", "Inchangé", "Appliquer", "Enregistré", "Appliqué",
			"Apparence indisponible ; apparence d’origine utilisée.", "Préréglage indisponible, ignoré : {0}", "Pack indisponible", "Saisissez un nom de 1 à 40 caractères", "Un pack porte déjà ce nom", "Utilise les préréglages existants. Un pack [P] applique immédiatement l’apparence du personnage ; ses préréglages de cartes et de monstres ne valent que pour la partie et sont restaurés en la quittant. Les changements en partie ne modifient pas le pack.", "Enregistrez les changements avant de les appliquer", "Application…", "Ces apparences imposent des changements liés à d’autres personnages ou régions. Vos réglages restent inchangés."
		},
		["ita"] = new string[19]
		{
			"Gestisci pacchetti skin", "Nuovo pacchetto", "Nome pacchetto", "Skin personaggio", "Preset carte", "Preset mostri", "Invariato", "Applica", "Salvato", "Applicato",
			"Skin personaggio non disponibile; viene usata l’originale.", "Preset non disponibile, ignorato: {0}", "Pacchetto non disponibile", "Inserisci un nome di 1–40 caratteri", "Esiste già un pacchetto con questo nome", "Usa i preset esistenti. Un pacchetto [P] applica subito la skin del personaggio; i preset di carte e mostri valgono solo per la partita e vengono ripristinati all’uscita. Le modifiche durante la partita non cambiano il pacchetto.", "Salva le modifiche prima di applicarle", "Applicazione…", "Queste skin richiedono modifiche collegate ad altri personaggi o regioni. Le impostazioni non sono cambiate."
		},
		["jpn"] = new string[19]
		{
			"スキンパック管理", "新規パック", "パック名", "キャラクタースキン", "カードプリセット", "モンスタープリセット", "変更しない", "適用", "保存しました", "適用しました",
			"キャラクタースキンが利用できないため、元のスキンを使用します。", "プリセットが利用できないためスキップ：{0}", "パックを利用できません", "1～40文字で名前を入力してください", "同名のパックが既にあります", "既存のプリセットを参照します。[P] パックを選ぶとキャラクタースキンはすぐ切り替わり、カードとモンスターのプリセットはラン中だけ適用され、終了後に元へ戻ります。ラン中の変更はパックを書き換えません。", "適用する前に変更を保存してください", "適用中…", "これらのスキンは他のキャラクターや地域の連動変更が必要なため適用できません。元の設定を維持しました。"
		},
		["kor"] = new string[19]
		{
			"스킨 묶음 관리", "새 묶음", "묶음 이름", "캐릭터 스킨", "카드 프리셋", "몬스터 프리셋", "변경 안 함", "적용", "저장됨", "적용됨",
			"캐릭터 스킨을 사용할 수 없어 기본 스킨을 사용합니다.", "프리셋을 사용할 수 없어 건너뜀: {0}", "묶음을 사용할 수 없음", "1~40자의 이름을 입력하세요", "같은 이름의 묶음이 있습니다", "기존 프리셋을 참조합니다. [P] 묶음을 선택하면 캐릭터 스킨은 즉시 바뀌고 카드와 몬스터 프리셋은 이번 런에만 적용되며 나가면 복원됩니다. 런 중 변경은 묶음을 수정하지 않습니다.", "적용하기 전에 변경 사항을 저장하세요", "적용 중…", "다른 캐릭터나 지역의 연동 변경이 필요한 스킨입니다. 기존 설정을 유지했습니다."
		},
		["pol"] = new string[19]
		{
			"Zarządzaj pakietami skórek", "Nowy pakiet", "Nazwa pakietu", "Skórka postaci", "Presety kart", "Presety potworów", "Bez zmian", "Zastosuj", "Zapisano", "Zastosowano",
			"Skórka postaci niedostępna; użyto oryginalnej.", "Preset niedostępny, pominięto: {0}", "Pakiet niedostępny", "Wpisz nazwę od 1 do 40 znaków", "Pakiet o tej nazwie już istnieje", "Korzysta z istniejących presetów. Pakiet [P] od razu zmienia skórkę postaci; presety kart i potworów działają tylko w tej grze i są przywracane po wyjściu. Zmiany podczas gry nie edytują pakietu.", "Zapisz zmiany przed zastosowaniem", "Stosowanie…", "Te skórki wymagają powiązanych zmian innych postaci lub regionów. Ustawienia pozostały bez zmian."
		},
		["ptb"] = new string[19]
		{
			"Gerenciar pacotes de skins", "Novo pacote", "Nome do pacote", "Skin do personagem", "Predefinições de cartas", "Predefinições de monstros", "Sem alteração", "Aplicar", "Salvo", "Aplicado",
			"Skin do personagem indisponível; usando a original.", "Predefinição indisponível, ignorada: {0}", "Pacote indisponível", "Digite um nome de 1 a 40 caracteres", "Já existe um pacote com esse nome", "Usa predefinições existentes. Um pacote [P] troca a skin do personagem imediatamente; as predefinições de cartas e monstros valem só na partida e são restauradas ao sair. Alterações durante a partida não editam o pacote.", "Salve as alterações antes de aplicar", "Aplicando…", "Estas skins exigem alterações vinculadas a outros personagens ou regiões. As configurações foram mantidas."
		},
		["rus"] = new string[19]
		{
			"Управление наборами обликов", "Новый набор", "Название набора", "Облик персонажа", "Пресеты карт", "Пресеты монстров", "Не менять", "Применить", "Сохранено", "Применено",
			"Облик персонажа недоступен; используется оригинальный.", "Пресет недоступен, пропущен: {0}", "Набор недоступен", "Введите название длиной 1–40 символов", "Набор с таким названием уже существует", "Использует существующие пресеты. Набор [P] сразу меняет облик персонажа; пресеты карт и монстров действуют только в этом забеге и восстанавливаются после выхода. Изменения в забеге не меняют набор.", "Сохраните изменения перед применением", "Применение…", "Эти облики требуют связанных изменений других персонажей или регионов. Настройки не изменены."
		},
		["spa"] = new string[19]
		{
			"Gestionar paquetes de aspectos", "Nuevo paquete", "Nombre del paquete", "Aspecto del personaje", "Preajustes de cartas", "Preajustes de monstruos", "Sin cambios", "Aplicar", "Guardado", "Aplicado",
			"Aspecto del personaje no disponible; se usa el original.", "Preajuste no disponible, omitido: {0}", "Paquete no disponible", "Introduce un nombre de 1 a 40 caracteres", "Ya existe un paquete con ese nombre", "Usa preajustes existentes. Un paquete [P] cambia el aspecto del personaje de inmediato; los preajustes de cartas y monstruos solo se aplican durante la partida y se restauran al salir. Los cambios durante la partida no modifican el paquete.", "Guarda los cambios antes de aplicarlos", "Aplicando…", "Estos aspectos requieren cambios vinculados en otros personajes o regiones. La configuración no se ha modificado."
		},
		["tha"] = new string[19]
		{
			"จ\u0e31ดการช\u0e38ดสก\u0e34น", "ช\u0e38ดใหม\u0e48", "ช\u0e37\u0e48อช\u0e38ด", "สก\u0e34นต\u0e31วละคร", "พร\u0e35เซ\u0e47ตการ\u0e4cด", "พร\u0e35เซ\u0e47ตมอนสเตอร\u0e4c", "ไม\u0e48เปล\u0e35\u0e48ยน", "ใช\u0e49", "บ\u0e31นท\u0e36กแล\u0e49ว", "ใช\u0e49แล\u0e49ว",
			"สก\u0e34นต\u0e31วละครไม\u0e48พร\u0e49อมใช\u0e49งาน ใช\u0e49สก\u0e34นเด\u0e34มแทน", "พร\u0e35เซ\u0e47ตไม\u0e48พร\u0e49อมใช\u0e49งาน ข\u0e49ามแล\u0e49ว: {0}", "ช\u0e38ดไม\u0e48พร\u0e49อมใช\u0e49งาน", "กรอกช\u0e37\u0e48อ 1–40 ต\u0e31วอ\u0e31กษร", "ม\u0e35ช\u0e38ดช\u0e37\u0e48อน\u0e35\u0e49อย\u0e39\u0e48แล\u0e49ว", "ใช\u0e49พร\u0e35เซ\u0e47ตท\u0e35\u0e48ม\u0e35อย\u0e39\u0e48 เม\u0e37\u0e48อเล\u0e37อกช\u0e38ด [P] สก\u0e34นต\u0e31วละครจะเปล\u0e35\u0e48ยนท\u0e31นท\u0e35 ส\u0e48วนพร\u0e35เซ\u0e47ตการ\u0e4cดและมอนสเตอร\u0e4cใช\u0e49เฉพาะร\u0e31นน\u0e35\u0e49และจะค\u0e37นค\u0e48าเม\u0e37\u0e48อออก การเปล\u0e35\u0e48ยนระหว\u0e48างร\u0e31นจะไม\u0e48แก\u0e49ไขช\u0e38ด", "บ\u0e31นท\u0e36กการเปล\u0e35\u0e48ยนแปลงก\u0e48อนใช\u0e49", "กำล\u0e31งใช\u0e49…", "สก\u0e34นเหล\u0e48าน\u0e35\u0e49ต\u0e49องเปล\u0e35\u0e48ยนต\u0e31วละครหร\u0e37อภ\u0e39ม\u0e34ภาคอ\u0e37\u0e48นร\u0e48วมด\u0e49วย การต\u0e31\u0e49งค\u0e48าเด\u0e34มย\u0e31งคงอย\u0e39\u0e48"
		},
		["tur"] = new string[19]
		{
			"Görünüm paketlerini yönet", "Yeni paket", "Paket adı", "Karakter görünümü", "Kart ön ayarları", "Canavar ön ayarları", "Değiştirme", "Uygula", "Kaydedildi", "Uygulandı",
			"Karakter görünümü kullanılamıyor; özgün görünüm kullanılıyor.", "Ön ayar kullanılamıyor, atlandı: {0}", "Paket kullanılamıyor", "1–40 karakterlik bir ad girin", "Bu adda bir paket zaten var", "Mevcut ön ayarları kullanır. Bir [P] paketi karakter görünümünü hemen değiştirir; kart ve canavar ön ayarları yalnızca bu koşuda uygulanır ve çıkınca geri yüklenir. Koşu içindeki değişiklikler paketi düzenlemez.", "Uygulamadan önce değişiklikleri kaydedin", "Uygulanıyor…", "Bu görünümler diğer karakter veya bölgelerde bağlantılı değişiklikler gerektiriyor. Ayarlarınız değiştirilmedi."
		},
		["esp"] = new string[19]
		{
			"Gestionar paquetes de aspectos", "Nuevo paquete", "Nombre del paquete", "Aspecto del personaje", "Preajustes de cartas", "Preajustes de monstruos", "Sin cambios", "Aplicar", "Guardado", "Aplicado",
			"Aspecto del personaje no disponible; se usa el original.", "Preajuste no disponible, omitido: {0}", "Paquete no disponible", "Introduce un nombre de 1 a 40 caracteres", "Ya existe un paquete con ese nombre", "Usa preajustes existentes. Un paquete [P] cambia el aspecto del personaje de inmediato; los preajustes de cartas y monstruos solo se aplican durante la partida y se restauran al salir. Los cambios durante la partida no modifican el paquete.", "Guarda los cambios antes de aplicarlos", "Aplicando…", "Estos aspectos requieren cambios vinculados en otros personajes o regiones. La configuración no se ha modificado."
		}
	};

	public const string DefaultVariantMarker = "{skin-changer-default}";

	public const string DifferentialVariantMarker = "{skin-changer-differential}";

	public const string AncientStyleVariantMarker = "{skin-changer-ancient-style}";

	public const string AncientDifferentialVariantMarker = "{skin-changer-ancient-differential}";

	private static readonly IReadOnlyDictionary<string, LanguagePack> Packs = new Dictionary<string, LanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new LanguagePack("Ancient Compendium", "No Ancients available to preview", "Game default", "Original game", "Cards with skins", "Card skins for this category", "Choose art for this card; Follow category uses the Card Compendium setting", "Follow category", "Size", "Reset", "Skin Changer load order", "The skin mods below were found before Skin Changer, so Skin Changer has been moved before them automatically:\n{0}\n\n[color=#F0C951][b]Restart the game for skin management to take effect.[/b][/color]", "Got it", "Don't show again", "Restart", "Adjusting the load order or restarting failed. The setting was not ignored. Move Skin Changer before every skin mod manually in Mod Manager, then restart the game.", "Default"),
		["zhs"] = new LanguagePack("先古图鉴", "没有可预览的先古之民", "游戏默认", "游戏原版", "有皮肤卡牌", "此分类的卡牌皮肤", "为这张卡单独选择卡图；“跟随分类”会使用卡牌总览中的设置", "跟随分类", "大小", "重置", "皮肤切换器-Skin Changer 加载顺序", "检测到以下皮肤 Mod 排在本 Mod 前面，本 Mod 已自动移到它们之前：\n{0}\n\n[color=#F0C951][b]请重启游戏，使皮肤接管生效。[/b][/color]", "知道了", "不再提示", "重启", "自动调整顺序或重启失败，设置没有被静默忽略。请在 Mod 管理界面手动把皮肤切换器-Skin Changer 移到所有皮肤 Mod 之前，然后重启游戏。", "默认"),
		["zht"] = new LanguagePack("先古圖鑑", "沒有可預覽的先古之民", "遊戲預設", "遊戲原版", "有外觀卡牌", "此分類的卡牌外觀", "為這張卡單獨選擇卡圖；「跟隨分類」會使用卡牌圖鑑中的設定", "跟隨分類", "大小", "重設", "外觀切換器-Skin Changer 載入順序", "以下外觀 Mod 排在本 Mod 前面，本 Mod 已自動移到它們之前：\n{0}\n\n[color=#F0C951][b]請重新啟動遊戲，讓外觀管理生效。[/b][/color]", "知道了", "不再提示", "重新啟動", "自動調整順序或重新啟動失敗，設定並未被忽略。請在 Mod 管理畫面手動將 Skin Changer 移到所有外觀 Mod 之前，然後重新啟動遊戲。", "預設"),
		["deu"] = new LanguagePack("Ahnenkompendium", "Keine Ahnen für die Vorschau verfügbar", "Spielstandard", "Originalspiel", "Karten mit Skins", "Karten-Skins für diese Kategorie", "Wähle das Bild dieser Karte; Kategorie folgen nutzt die Einstellung im Kartenkompendium", "Kategorie folgen", "Größe", "Zurücksetzen", "Skin-Changer-Ladereihenfolge", "Die folgenden Skin-Mods wurden vor Skin Changer gefunden. Skin Changer wurde automatisch davor verschoben:\n{0}\n\n[color=#F0C951][b]Starte das Spiel neu, damit die Skin-Verwaltung wirksam wird.[/b][/color]", "Verstanden", "Nicht mehr anzeigen", "Neu starten", "Das Anpassen der Ladereihenfolge oder der Neustart ist fehlgeschlagen. Verschiebe Skin Changer im Mod-Manager manuell vor alle Skin-Mods und starte das Spiel neu.", "Standard"),
		["esp"] = new LanguagePack("Compendio de Antiguos", "No hay Antiguos disponibles para previsualizar", "Predeterminado del juego", "Juego original", "Cartas con aspectos", "Aspectos de cartas para esta categoría", "Elige el arte de esta carta; Seguir categoría usa el ajuste del compendio de cartas", "Seguir categoría", "Tamaño", "Restablecer", "Orden de carga de Skin Changer", "Estos mods de aspectos estaban antes que Skin Changer. Skin Changer se ha movido automáticamente delante de ellos:\n{0}\n\n[color=#F0C951][b]Reinicia el juego para que la gestión de aspectos surta efecto.[/b][/color]", "Entendido", "No volver a mostrar", "Reiniciar", "No se pudo ajustar el orden o reiniciar. Mueve Skin Changer manualmente antes de todos los mods de aspectos y reinicia el juego.", "Predeterminado"),
		["fra"] = new LanguagePack("Compendium des Anciens", "Aucun Ancien disponible pour l’aperçu", "Réglage du jeu", "Jeu d’origine", "Cartes avec skins", "Skins de cartes pour cette catégorie", "Choisissez l’image de cette carte ; Suivre la catégorie utilise le réglage du compendium des cartes", "Suivre la catégorie", "Taille", "Réinitialiser", "Ordre de chargement de Skin Changer", "Les mods de skins suivants étaient placés avant Skin Changer. Skin Changer a été déplacé automatiquement devant eux :\n{0}\n\n[color=#F0C951][b]Redémarrez le jeu pour activer la gestion des skins.[/b][/color]", "Compris", "Ne plus afficher", "Redémarrer", "L’ajustement de l’ordre ou le redémarrage a échoué. Placez manuellement Skin Changer avant tous les mods de skins, puis redémarrez le jeu.", "Par défaut"),
		["ita"] = new LanguagePack("Compendio degli Antichi", "Nessun Antico disponibile per l’anteprima", "Predefinito del gioco", "Gioco originale", "Carte con skin", "Skin delle carte per questa categoria", "Scegli l’immagine di questa carta; Segui categoria usa l’impostazione del compendio carte", "Segui categoria", "Dimensione", "Ripristina", "Ordine di caricamento di Skin Changer", "I seguenti mod delle skin erano prima di Skin Changer. Skin Changer è stato spostato automaticamente prima di loro:\n{0}\n\n[color=#F0C951][b]Riavvia il gioco per rendere attiva la gestione delle skin.[/b][/color]", "Ho capito", "Non mostrare più", "Riavvia", "La regolazione dell’ordine o il riavvio non è riuscito. Sposta manualmente Skin Changer prima di tutti i mod delle skin e riavvia il gioco.", "Predefinito"),
		["jpn"] = new LanguagePack("エンシェント図鑑", "プレビューできるエンシェントがありません", "ゲーム標準", "ゲーム原版", "スキン付きカード", "この分類のカードスキン", "このカードの画像を個別に選択。「分類に従う」はカード図鑑の設定を使用します", "分類に従う", "サイズ", "リセット", "Skin Changerのロード順", "次のスキンModがSkin Changerより前にあったため、Skin Changerを自動的にその前へ移動しました：\n{0}\n\n[color=#F0C951][b]スキン管理を有効にするにはゲームを再起動してください。[/b][/color]", "了解", "今後表示しない", "再起動", "ロード順の調整または再起動に失敗しました。Skin ChangerをすべてのスキンModより前に手動で移動し、ゲームを再起動してください。", "デフォルト"),
		["kor"] = new LanguagePack("고대인 도감", "미리 볼 수 있는 고대인이 없습니다", "게임 기본", "게임 원본", "스킨 카드", "이 분류의 카드 스킨", "이 카드의 그림을 따로 선택합니다. 분류 따르기는 카드 도감 설정을 사용합니다", "분류 따르기", "크기", "초기화", "Skin Changer 로드 순서", "다음 스킨 Mod가 Skin Changer보다 먼저 있어 Skin Changer를 자동으로 그 앞에 옮겼습니다:\n{0}\n\n[color=#F0C951][b]스킨 관리를 적용하려면 게임을 다시 시작하세요.[/b][/color]", "확인", "다시 표시하지 않기", "재시작", "로드 순서 조정 또는 재시작에 실패했습니다. Skin Changer를 모든 스킨 Mod 앞에 직접 옮긴 뒤 게임을 재시작하세요.", "기본"),
		["pol"] = new LanguagePack("Kompendium Pradawnych", "Brak Pradawnych do podglądu", "Domyślne gry", "Oryginalna gra", "Karty ze skórkami", "Skórki kart dla tej kategorii", "Wybierz grafikę tej karty; Podążaj za kategorią używa ustawienia z kompendium kart", "Podążaj za kategorią", "Rozmiar", "Resetuj", "Kolejność ładowania Skin Changer", "Następujące mody skórek znajdowały się przed Skin Changer, więc Skin Changer został automatycznie przeniesiony przed nie:\n{0}\n\n[color=#F0C951][b]Uruchom grę ponownie, aby zarządzanie skórkami zaczęło działać.[/b][/color]", "Rozumiem", "Nie pokazuj ponownie", "Uruchom ponownie", "Zmiana kolejności lub ponowne uruchomienie nie powiodło się. Ręcznie przenieś Skin Changer przed wszystkie mody skórek i uruchom grę ponownie.", "Domyślne"),
		["ptb"] = new LanguagePack("Compêndio dos Anciões", "Nenhum Ancião disponível para prévia", "Padrão do jogo", "Jogo original", "Cartas com visuais", "Visuais de cartas desta categoria", "Escolha a arte desta carta; Seguir categoria usa a configuração do compêndio de cartas", "Seguir categoria", "Tamanho", "Redefinir", "Ordem de carregamento do Skin Changer", "Os seguintes mods de visuais estavam antes do Skin Changer, que foi movido automaticamente para antes deles:\n{0}\n\n[color=#F0C951][b]Reinicie o jogo para ativar o gerenciamento de visuais.[/b][/color]", "Entendi", "Não mostrar novamente", "Reiniciar", "Não foi possível ajustar a ordem ou reiniciar. Mova o Skin Changer manualmente para antes de todos os mods de visuais e reinicie o jogo.", "Padrão"),
		["rus"] = new LanguagePack("Энциклопедия Древних", "Нет Древних для предпросмотра", "По умолчанию игры", "Оригинальная игра", "Карты с обликами", "Облики карт для этой категории", "Выберите изображение этой карты; Следовать категории использует настройку энциклопедии карт", "Следовать категории", "Размер", "Сбросить", "Порядок загрузки Skin Changer", "Следующие моды обликов находились перед Skin Changer, поэтому Skin Changer автоматически перемещён перед ними:\n{0}\n\n[color=#F0C951][b]Перезапустите игру, чтобы управление обликами вступило в силу.[/b][/color]", "Понятно", "Больше не показывать", "Перезапустить", "Не удалось изменить порядок или перезапустить игру. Вручную поставьте Skin Changer перед всеми модами обликов и перезапустите игру.", "По умолчанию"),
		["spa"] = new LanguagePack("Compendio de los Antiguos", "No hay Antiguos disponibles para previsualizar", "Predeterminado del juego", "Juego original", "Cartas con aspectos", "Aspectos de cartas para esta categoría", "Elige la ilustración de esta carta; Seguir categoría usa el ajuste del compendio de cartas", "Seguir categoría", "Tamaño", "Restablecer", "Orden de carga de Skin Changer", "Estos mods de aspectos estaban antes que Skin Changer, que se ha movido automáticamente delante de ellos:\n{0}\n\n[color=#F0C951][b]Reinicia el juego para activar la gestión de aspectos.[/b][/color]", "Entendido", "No volver a mostrar", "Reiniciar", "No se pudo ajustar el orden o reiniciar. Mueve manualmente Skin Changer antes de todos los mods de aspectos y reinicia el juego.", "Predeterminado"),
		["tha"] = new LanguagePack("สาราน\u0e38กรม Ancient", "ไม\u0e48ม\u0e35 Ancient ท\u0e35\u0e48พร\u0e49อมให\u0e49ด\u0e39ต\u0e31วอย\u0e48าง", "ค\u0e48าเร\u0e34\u0e48มต\u0e49นของเกม", "เกมต\u0e49นฉบ\u0e31บ", "การ\u0e4cดท\u0e35\u0e48ม\u0e35สก\u0e34น", "สก\u0e34นการ\u0e4cดสำหร\u0e31บหมวดน\u0e35\u0e49", "เล\u0e37อกภาพของการ\u0e4cดใบน\u0e35\u0e49โดยเฉพาะ; ตามหมวดจะใช\u0e49ค\u0e48าจากสาราน\u0e38กรมการ\u0e4cด", "ตามหมวด", "ขนาด", "ร\u0e35เซ\u0e47ต", "ลำด\u0e31บการโหลด Skin Changer", "Mod สก\u0e34นต\u0e48อไปน\u0e35\u0e49อย\u0e39\u0e48ก\u0e48อน Skin Changer จ\u0e36งย\u0e49าย Skin Changer ไปไว\u0e49ด\u0e49านหน\u0e49าของ Mod เหล\u0e48าน\u0e31\u0e49นโดยอ\u0e31ตโนม\u0e31ต\u0e34:\n{0}\n\n[color=#F0C951][b]เร\u0e34\u0e48มเกมใหม\u0e48เพ\u0e37\u0e48อให\u0e49การจ\u0e31ดการสก\u0e34นม\u0e35ผล[/b][/color]", "เข\u0e49าใจแล\u0e49ว", "ไม\u0e48ต\u0e49องแสดงอ\u0e35ก", "เร\u0e34\u0e48มเกมใหม\u0e48", "ไม\u0e48สามารถปร\u0e31บลำด\u0e31บหร\u0e37อเร\u0e34\u0e48มเกมใหม\u0e48ได\u0e49 โปรดย\u0e49าย Skin Changer ไปไว\u0e49ก\u0e48อน Mod สก\u0e34นท\u0e31\u0e49งหมดด\u0e49วยตนเอง แล\u0e49วเร\u0e34\u0e48มเกมใหม\u0e48", "ค\u0e48าเร\u0e34\u0e48มต\u0e49น"),
		["tur"] = new LanguagePack("Kadimler Külliyatı", "Önizlenecek Kadim yok", "Oyun varsayılanı", "Orijinal oyun", "Görünümlü kartlar", "Bu kategori için kart görünümleri", "Bu kartın görselini seç; Kategoriyi izle, kart külliyatındaki ayarı kullanır", "Kategoriyi izle", "Boyut", "Sıfırla", "Skin Changer yükleme sırası", "Aşağıdaki görünüm Modları Skin Changer'ın önündeydi; Skin Changer otomatik olarak bunların önüne taşındı:\n{0}\n\n[color=#F0C951][b]Görünüm yönetiminin çalışması için oyunu yeniden başlatın.[/b][/color]", "Anladım", "Bir daha gösterme", "Yeniden başlat", "Sıra ayarı veya yeniden başlatma başarısız oldu. Skin Changer'ı tüm görünüm Modlarının önüne elle taşıyıp oyunu yeniden başlatın.", "Varsayılan")
	};

	private static readonly IReadOnlyDictionary<string, OtherCompendiumLanguagePack> OtherCompendiumPacks = new Dictionary<string, OtherCompendiumLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new OtherCompendiumLanguagePack("Other Compendium", "Ancients", "Merchants", "Creatures"),
		["zhs"] = new OtherCompendiumLanguagePack("其它图鉴", "先古", "商人", "生物"),
		["zht"] = new OtherCompendiumLanguagePack("其他圖鑑", "先古", "商人", "生物"),
		["deu"] = new OtherCompendiumLanguagePack("Weitere Enzyklopädie", "Uralte", "Händler", "Kreaturen"),
		["esp"] = new OtherCompendiumLanguagePack("Compendio adicional", "Antiguos", "Mercaderes", "Criaturas"),
		["fra"] = new OtherCompendiumLanguagePack("Autre compendium", "Anciens", "Marchands", "Créatures"),
		["ita"] = new OtherCompendiumLanguagePack("Altro compendio", "Antichi", "Mercanti", "Creature"),
		["jpn"] = new OtherCompendiumLanguagePack("その他の図鑑", "エンシェント", "商人", "生物"),
		["kor"] = new OtherCompendiumLanguagePack("기타 도감", "고대인", "상인", "생물"),
		["pol"] = new OtherCompendiumLanguagePack("Inny bestiariusz", "Pradawni", "Kupcy", "Stworzenia"),
		["ptb"] = new OtherCompendiumLanguagePack("Outro compêndio", "Anciões", "Mercadores", "Criaturas"),
		["rus"] = new OtherCompendiumLanguagePack("Другой справочник", "Древние", "Торговцы", "Существа"),
		["spa"] = new OtherCompendiumLanguagePack("Otro compendio", "Antiguos", "Mercaderes", "Criaturas"),
		["tha"] = new OtherCompendiumLanguagePack("สาราน\u0e38กรมอ\u0e37\u0e48น", "Ancient", "พ\u0e48อค\u0e49า", "ส\u0e34\u0e48งม\u0e35ช\u0e35ว\u0e34ต"),
		["tur"] = new OtherCompendiumLanguagePack("Diğer külliyat", "Kadimler", "Tüccarlar", "Yaratıklar")
	};

	private static readonly IReadOnlyDictionary<string, AppearanceLanguagePack> AppearancePacks = new Dictionary<string, AppearanceLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new AppearanceLanguagePack("Appearance", "Click a character, monster, map Boss icon, Ancient, companion, or merchant to adjust its appearance.", "Skin", "Scale", "Horizontal offset", "Vertical offset", "Hold to compare", "Alignment guide", "Gold cross: original anchor · Cyan frame: current bounds", "Applied immediately", "Queued until the current action finishes", "Saved. Live positioning preview is available during combat.", "Could not apply appearance"),
		["zhs"] = new AppearanceLanguagePack("外观", "请选择一个角色、怪物、地图上的 Boss 图标、先古之民、同伴或商人来调整外观。", "皮肤", "缩放", "水平位移", "垂直位移", "按住对比原位", "定位参考线", "金色十字：原始落点 · 青色边框：当前范围", "已立即应用", "当前动作结束后自动应用", "已保存；进入战斗后可实时预览位置。", "应用角色外观失败"),
		["zht"] = new AppearanceLanguagePack("外觀", "請選擇一個角色、怪物、地圖上的 Boss 圖示、先古之民、同伴或商人來調整外觀。", "外觀", "縮放", "水平位移", "垂直位移", "按住比較原位", "定位參考線", "金色十字：原始落點 · 青色邊框：目前範圍", "已立即套用", "目前動作結束後自動套用", "已儲存；進入戰鬥後可即時預覽位置。", "無法套用角色外觀"),
		["deu"] = new AppearanceLanguagePack("Aussehen", "Klicke auf einen Charakter, ein Monster, ein Boss-Symbol auf der Karte, einen Ahnen, einen Begleiter oder den Händler, um das Aussehen anzupassen.", "Skin", "Skalierung", "Horizontaler Versatz", "Vertikaler Versatz", "Zum Vergleichen halten", "Ausrichtungshilfe", "Goldenes Kreuz: Ursprung · Türkiser Rahmen: aktuelle Grenzen", "Sofort angewendet", "Wird nach der aktuellen Aktion angewendet", "Gespeichert. Die Live-Vorschau ist im Kampf verfügbar.", "Aussehen konnte nicht angewendet werden"),
		["esp"] = new AppearanceLanguagePack("Aspecto", "Haz clic en un personaje, monstruo, icono de jefe del mapa, Antiguo, compañero o comerciante para ajustar su aspecto.", "Aspecto", "Escala", "Desplazamiento horizontal", "Desplazamiento vertical", "Mantén para comparar", "Guía de alineación", "Cruz dorada: origen · Marco cian: límites actuales", "Aplicado al instante", "Se aplicará al terminar la acción actual", "Guardado. La vista previa en vivo está disponible en combate.", "No se pudo aplicar el aspecto"),
		["fra"] = new AppearanceLanguagePack("Apparence", "Cliquez sur un personnage, un monstre, une icône de Boss sur la carte, un Ancien, un compagnon ou le marchand pour modifier son apparence.", "Skin", "Échelle", "Décalage horizontal", "Décalage vertical", "Maintenir pour comparer", "Guide d’alignement", "Croix dorée : origine · Cadre cyan : limites actuelles", "Appliqué immédiatement", "Sera appliqué après l’action en cours", "Enregistré. L’aperçu en direct est disponible en combat.", "Impossible d’appliquer l’apparence"),
		["ita"] = new AppearanceLanguagePack("Aspetto", "Fai clic su un personaggio, un mostro, un’icona Boss sulla mappa, un Antico, un compagno o il mercante per modificarne l’aspetto.", "Skin", "Scala", "Spostamento orizzontale", "Spostamento verticale", "Tieni premuto per confrontare", "Guida allineamento", "Croce dorata: origine · Cornice ciano: limiti attuali", "Applicato subito", "Verrà applicato al termine dell’azione corrente", "Salvato. L’anteprima dal vivo è disponibile in combattimento.", "Impossibile applicare l’aspetto"),
		["jpn"] = new AppearanceLanguagePack("外見", "外見を調整するキャラクター、モンスター、マップ上のボスアイコン、エンシェント、仲間、または商人をクリックしてください。", "スキン", "拡大率", "横位置", "縦位置", "長押しで元と比較", "位置合わせガイド", "金の十字：元の基準点 · 水色の枠：現在の範囲", "すぐに適用しました", "現在のアクション終了後に適用します", "保存しました。戦闘中に位置をリアルタイム確認できます。", "外見を適用できませんでした"),
		["kor"] = new AppearanceLanguagePack("외형", "외형을 조정할 캐릭터, 몬스터, 지도 보스 아이콘, 고대인, 동료 또는 상인을 클릭하세요.", "스킨", "크기", "가로 위치", "세로 위치", "길게 눌러 원본 비교", "정렬 안내선", "금색 십자: 원래 기준점 · 청록색 테두리: 현재 범위", "즉시 적용됨", "현재 행동이 끝나면 적용됨", "저장됨. 전투 중 위치를 실시간으로 확인할 수 있습니다.", "외형을 적용하지 못했습니다"),
		["pol"] = new AppearanceLanguagePack("Wygląd", "Kliknij postać, potwora, ikonę Bossa na mapie, Pradawnego, towarzysza lub kupca, aby dostosować wygląd.", "Skórka", "Skala", "Przesunięcie poziome", "Przesunięcie pionowe", "Przytrzymaj, aby porównać", "Linie wyrównania", "Złoty krzyż: punkt bazowy · Turkusowa ramka: aktualny obszar", "Zastosowano natychmiast", "Zostanie zastosowane po bieżącej akcji", "Zapisano. Podgląd na żywo jest dostępny w walce.", "Nie udało się zastosować wyglądu"),
		["ptb"] = new AppearanceLanguagePack("Visual", "Clique em um personagem, monstro, ícone de chefe no mapa, Ancião, companheiro ou mercador para ajustar seu visual.", "Visual", "Escala", "Deslocamento horizontal", "Deslocamento vertical", "Segure para comparar", "Guia de alinhamento", "Cruz dourada: origem · Moldura ciano: limites atuais", "Aplicado imediatamente", "Será aplicado após a ação atual", "Salvo. A prévia ao vivo está disponível em combate.", "Não foi possível aplicar o visual"),
		["rus"] = new AppearanceLanguagePack("Облик", "Нажмите на персонажа, монстра, значок босса на карте, Древнего, спутника или торговца, чтобы настроить облик.", "Облик", "Масштаб", "Смещение по горизонтали", "Смещение по вертикали", "Удерживать для сравнения", "Направляющие", "Золотой крест: исходная точка · Голубая рамка: текущие границы", "Применено сразу", "Будет применено после текущего действия", "Сохранено. Предпросмотр положения доступен в бою.", "Не удалось применить облик"),
		["spa"] = new AppearanceLanguagePack("Aspecto", "Haz clic en un personaje, monstruo, icono de jefe del mapa, Antiguo, compañero o comerciante para ajustar su aspecto.", "Aspecto", "Escala", "Desplazamiento horizontal", "Desplazamiento vertical", "Mantén para comparar", "Guía de alineación", "Cruz dorada: origen · Marco cian: límites actuales", "Aplicado al instante", "Se aplicará al terminar la acción actual", "Guardado. La vista previa en vivo está disponible en combate.", "No se pudo aplicar el aspecto"),
		["tha"] = new AppearanceLanguagePack("ร\u0e39ปล\u0e31กษณ\u0e4c", "คล\u0e34กต\u0e31วละคร มอนสเตอร\u0e4c ไอคอนบอสบนแผนท\u0e35\u0e48 Ancient เพ\u0e37\u0e48อนร\u0e48วมทาง หร\u0e37อพ\u0e48อค\u0e49าเพ\u0e37\u0e48อปร\u0e31บร\u0e39ปล\u0e31กษณ\u0e4c", "สก\u0e34น", "ขนาด", "ตำแหน\u0e48งแนวนอน", "ตำแหน\u0e48งแนวต\u0e31\u0e49ง", "กดค\u0e49างเพ\u0e37\u0e48อเท\u0e35ยบ", "เส\u0e49นช\u0e48วยจ\u0e31ดตำแหน\u0e48ง", "กากบาทส\u0e35ทอง: จ\u0e38ดเด\u0e34ม · กรอบส\u0e35ฟ\u0e49า: ขอบเขตป\u0e31จจ\u0e38บ\u0e31น", "ใช\u0e49ท\u0e31นท\u0e35แล\u0e49ว", "จะใช\u0e49หล\u0e31งแอ\u0e47กช\u0e31นป\u0e31จจ\u0e38บ\u0e31นจบ", "บ\u0e31นท\u0e36กแล\u0e49ว ด\u0e39ตำแหน\u0e48งแบบสดได\u0e49ระหว\u0e48างต\u0e48อส\u0e39\u0e49", "ไม\u0e48สามารถใช\u0e49ร\u0e39ปล\u0e31กษณ\u0e4cได\u0e49"),
		["tur"] = new AppearanceLanguagePack("Görünüm", "Görünümü ayarlamak için bir karaktere, canavara, haritadaki Boss simgesine, Kadime, yoldaşa veya tüccara tıkla.", "Görünüm", "Ölçek", "Yatay konum", "Dikey konum", "Karşılaştırmak için basılı tut", "Hizalama kılavuzu", "Altın artı: özgün konum · Camgöbeği çerçeve: geçerli sınırlar", "Hemen uygulandı", "Geçerli eylem bitince uygulanacak", "Kaydedildi. Canlı konum önizlemesi savaşta kullanılabilir.", "Görünüm uygulanamadı")
	};

	private static readonly IReadOnlyDictionary<string, CardPriorityLanguagePack> CardPriorityPacks = new Dictionary<string, CardPriorityLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new CardPriorityLanguagePack("Enabled", "Skin priority ({0})", "Enable card-skin mods and adjust priority. Each card uses every visual effect from the highest enabled mod that supports it; effects are never mixed between mods.", "Art {0}/{1}", "Current: {0}", "Available: {0}", "Close", "Skin priority", "Set the enabled skin mods and their order for this bestiary region. Each monster uses the highest enabled mod that supports it; individual choices take priority."),
		["zhs"] = new CardPriorityLanguagePack("启用", "皮肤优先级（{0}）", "勾选“启用”决定 Mod 是否作用于当前分类；每张牌只使用最靠上且支持它的 Mod 的全部外观效果，不会混用多个 Mod。", "卡图 {0}/{1}", "当前：{0}", "可用：{0}", "关闭", "皮肤优先级", "设置当前图鉴地区启用的皮肤 Mod 和顺序；每只怪物使用最靠上且支持它的 Mod，单只怪物的手动选择优先。"),
		["zht"] = new CardPriorityLanguagePack("啟用", "外觀優先順序（{0}）", "勾選「啟用」決定 Mod 是否作用於目前分類；每張牌只使用最上方且支援它的 Mod 的全部外觀效果，不會混用多個 Mod。", "卡圖 {0}/{1}", "目前：{0}", "可用：{0}", "關閉", "外觀優先順序", "設定目前圖鑑地區啟用的外觀 Mod 與順序；每隻怪物使用最上方且支援它的 Mod，單隻怪物的手動選擇優先。"),
		["deu"] = new CardPriorityLanguagePack("Aktiv", "Skin-Priorität ({0})", "Aktiviere und ordne Karten-Skin-Mods. Jede Karte nutzt alle Effekte des höchsten aktiven Mods, der sie unterstützt; Mods werden nie gemischt.", "Bilder {0}/{1}", "Aktuell: {0}", "Verfügbar: {0}", "Schließen", "Skin-Priorität", "Aktiviere und ordne Skin-Mods für diese Bestiarium-Region. Jedes Monster nutzt den höchsten unterstützten Mod; einzelne Auswahlen haben Vorrang."),
		["esp"] = new CardPriorityLanguagePack("Activo", "Prioridad de aspectos ({0})", "Activa y ordena los mods. Cada carta usa todos los efectos del mod activo más alto que la admita; nunca se mezclan mods.", "Arte {0}/{1}", "Actual: {0}", "Disponibles: {0}", "Cerrar", "Prioridad de aspectos", "Activa y ordena aspectos para esta región del bestiario. Cada monstruo usa el primer mod compatible; las elecciones individuales tienen prioridad."),
		["fra"] = new CardPriorityLanguagePack("Activé", "Priorité des skins ({0})", "Activez et classez les mods. Chaque carte utilise tous les effets du premier mod actif qui la prend en charge ; les mods ne sont jamais mélangés.", "Images {0}/{1}", "Actuel : {0}", "Disponibles : {0}", "Fermer", "Priorité des skins", "Activez et classez les skins de cette région du bestiaire. Chaque monstre utilise le premier mod compatible ; les choix individuels sont prioritaires."),
		["ita"] = new CardPriorityLanguagePack("Attivo", "Priorità skin ({0})", "Attiva e ordina i mod. Ogni carta usa tutti gli effetti del primo mod attivo che la supporta; i mod non vengono mai combinati.", "Immagini {0}/{1}", "Attuale: {0}", "Disponibili: {0}", "Chiudi", "Priorità skin", "Attiva e ordina le skin per questa regione del bestiario. Ogni mostro usa il primo mod compatibile; le scelte individuali hanno priorità."),
		["jpn"] = new CardPriorityLanguagePack("有効", "スキン優先順位（{0}）", "カードスキンModを有効化して並べ替えます。各カードは対応する最上位の有効Modの全外観効果だけを使い、複数Modを混在させません。", "画像 {0}/{1}", "現在：{0}", "利用可能：{0}", "閉じる", "スキン優先順位", "この図鑑地域で有効なスキンModと順序を設定します。各モンスターは対応する最上位Modを使い、個別指定が優先されます。"),
		["kor"] = new CardPriorityLanguagePack("사용", "스킨 우선순위 ({0})", "카드 스킨 Mod를 켜고 순서를 조정합니다. 각 카드는 지원하는 가장 위의 활성 Mod 효과만 모두 사용하며 여러 Mod를 섞지 않습니다.", "그림 {0}/{1}", "현재: {0}", "사용 가능: {0}", "닫기", "스킨 우선순위", "이 도감 지역에서 사용할 스킨 Mod와 순서를 정합니다. 각 몬스터는 지원하는 최상위 Mod를 사용하며 개별 선택이 우선합니다."),
		["pol"] = new CardPriorityLanguagePack("Włączona", "Priorytet skórek ({0})", "Włącz i uporządkuj mody. Każda karta używa wszystkich efektów najwyższego aktywnego moda, który ją obsługuje; mody nie są mieszane.", "Grafiki {0}/{1}", "Bieżąca: {0}", "Dostępne: {0}", "Zamknij", "Priorytet skórek", "Włącz i uporządkuj skórki dla tego regionu bestiariusza. Każdy potwór używa najwyższego zgodnego moda; wybór indywidualny ma pierwszeństwo."),
		["ptb"] = new CardPriorityLanguagePack("Ativo", "Prioridade dos visuais ({0})", "Ative e ordene os mods. Cada carta usa todos os efeitos do mod ativo mais alto que a suporta; efeitos de mods diferentes não são misturados.", "Artes {0}/{1}", "Atual: {0}", "Disponíveis: {0}", "Fechar", "Prioridade dos visuais", "Ative e ordene visuais para esta região do bestiário. Cada monstro usa o primeiro mod compatível; escolhas individuais têm prioridade."),
		["rus"] = new CardPriorityLanguagePack("Включено", "Приоритет обликов ({0})", "Включайте и упорядочивайте моды. Каждая карта использует все эффекты самого верхнего активного мода, который её поддерживает; моды не смешиваются.", "Изображения {0}/{1}", "Текущий: {0}", "Доступны: {0}", "Закрыть", "Приоритет обликов", "Включайте и упорядочивайте облики для этого региона бестиария. Монстр использует верхний совместимый мод; отдельный выбор имеет приоритет."),
		["spa"] = new CardPriorityLanguagePack("Activo", "Prioridad de aspectos ({0})", "Activa y ordena los mods. Cada carta usa todos los efectos del mod activo más alto que la admita; nunca se mezclan mods.", "Arte {0}/{1}", "Actual: {0}", "Disponibles: {0}", "Cerrar", "Prioridad de aspectos", "Activa y ordena aspectos para esta región del bestiario. Cada monstruo usa el primer mod compatible; las elecciones individuales tienen prioridad."),
		["tha"] = new CardPriorityLanguagePack("เป\u0e34ดใช\u0e49", "ลำด\u0e31บสก\u0e34น ({0})", "เป\u0e34ดใช\u0e49และจ\u0e31ดลำด\u0e31บ Mod สก\u0e34นการ\u0e4cด การ\u0e4cดแต\u0e48ละใบจะใช\u0e49เอฟเฟกต\u0e4cท\u0e31\u0e49งหมดจาก Mod ท\u0e35\u0e48เป\u0e34ดใช\u0e49และอย\u0e39\u0e48ส\u0e39งส\u0e38ดซ\u0e36\u0e48งรองร\u0e31บการ\u0e4cดน\u0e31\u0e49น โดยไม\u0e48ผสมหลาย Mod", "ภาพ {0}/{1}", "ป\u0e31จจ\u0e38บ\u0e31น: {0}", "ม\u0e35ให\u0e49ใช\u0e49: {0}", "ป\u0e34ด", "ลำด\u0e31บสก\u0e34น", "เป\u0e34ดใช\u0e49และจ\u0e31ดลำด\u0e31บสก\u0e34นสำหร\u0e31บพ\u0e37\u0e49นท\u0e35\u0e48น\u0e35\u0e49ในสาราน\u0e38กรม มอนสเตอร\u0e4cแต\u0e48ละต\u0e31วใช\u0e49 Mod ท\u0e35\u0e48รองร\u0e31บและอย\u0e39\u0e48ส\u0e39งส\u0e38ด โดยการเล\u0e37อกเฉพาะต\u0e31วม\u0e35ส\u0e34ทธ\u0e34\u0e4cก\u0e48อน"),
		["tur"] = new CardPriorityLanguagePack("Etkin", "Görünüm önceliği ({0})", "Kart görünümü Modlarını etkinleştirip sırala. Her kart, onu destekleyen en üstteki etkin Modun tüm efektlerini kullanır; Modlar karıştırılmaz.", "Görsel {0}/{1}", "Geçerli: {0}", "Mevcut: {0}", "Kapat", "Görünüm önceliği", "Bu bestiyer bölgesi için görünümleri etkinleştirip sırala. Her canavar en üstteki uyumlu Modu kullanır; tekil seçimler önceliklidir.")
	};

	private static readonly IReadOnlyDictionary<string, CardPresetLanguagePack> CardPresetPacks = new Dictionary<string, CardPresetLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new CardPresetLanguagePack("Presets", "Save and switch card-art settings for this category.", "Preset name", "Save current", "Apply", "Overwrite", "Rename", "Delete", "Confirm", "Active", "No presets saved for this category"),
		["zhs"] = new CardPresetLanguagePack("预设", "保存并切换当前分类的卡图设置。", "预设名称", "保存当前", "应用", "覆盖", "重命名", "删除", "确认删除", "当前", "当前分类尚未保存预设"),
		["zht"] = new CardPresetLanguagePack("預設", "儲存並切換目前分類的卡圖設定。", "預設名稱", "儲存目前", "套用", "覆蓋", "重新命名", "刪除", "確認刪除", "目前", "目前分類尚未儲存預設"),
		["deu"] = new CardPresetLanguagePack("Profile", "Kartenbild-Einstellungen dieser Kategorie speichern und wechseln.", "Profilname", "Aktuelles speichern", "Anwenden", "Überschreiben", "Umbenennen", "Löschen", "Bestätigen", "Aktiv", "Für diese Kategorie sind keine Profile gespeichert"),
		["esp"] = new CardPresetLanguagePack("Perfiles", "Guarda y cambia el arte de esta categoría.", "Nombre del perfil", "Guardar actual", "Aplicar", "Sobrescribir", "Renombrar", "Borrar", "Confirmar", "Activo", "No hay perfiles guardados para esta categoría"),
		["fra"] = new CardPresetLanguagePack("Profils", "Enregistrez et changez les cartes de cette catégorie.", "Nom du profil", "Enregistrer", "Appliquer", "Écraser", "Renommer", "Supprimer", "Confirmer", "Actif", "Aucun profil enregistré pour cette catégorie"),
		["ita"] = new CardPresetLanguagePack("Profili", "Salva e cambia le carte di questa categoria.", "Nome profilo", "Salva attuale", "Applica", "Sovrascrivi", "Rinomina", "Elimina", "Conferma", "Attivo", "Nessun profilo salvato per questa categoria"),
		["jpn"] = new CardPresetLanguagePack("プリセット", "この分類のカード画像設定を保存して切り替えます。", "プリセット名", "現在を保存", "適用", "上書き", "名前変更", "削除", "削除確認", "使用中", "この分類のカード画像プリセットはありません"),
		["kor"] = new CardPresetLanguagePack("프리셋", "이 분류의 카드 그림 설정을 저장하고 전환합니다.", "프리셋 이름", "현재 저장", "적용", "덮어쓰기", "이름 변경", "삭제", "삭제 확인", "사용 중", "이 분류에 저장된 카드 그림 프리셋이 없습니다"),
		["pol"] = new CardPresetLanguagePack("Profile", "Zapisuj i przełączaj grafiki kart tej kategorii.", "Nazwa profilu", "Zapisz bieżące", "Zastosuj", "Nadpisz", "Zmień nazwę", "Usuń", "Potwierdź", "Aktywny", "Brak profili kart zapisanych dla tej kategorii"),
		["ptb"] = new CardPresetLanguagePack("Perfis", "Salve e alterne as artes desta categoria.", "Nome do perfil", "Salvar atual", "Aplicar", "Substituir", "Renomear", "Excluir", "Confirmar", "Ativo", "Nenhum perfil salvo para esta categoria"),
		["rus"] = new CardPresetLanguagePack("Профили", "Сохраняйте и переключайте изображения карт этой категории.", "Имя профиля", "Сохранить", "Применить", "Перезаписать", "Переименовать", "Удалить", "Подтвердить", "Активен", "Нет сохранённых профилей карт для этой категории"),
		["spa"] = new CardPresetLanguagePack("Perfiles", "Guarda y cambia las ilustraciones de esta categoría.", "Nombre del perfil", "Guardar actual", "Aplicar", "Sobrescribir", "Renombrar", "Eliminar", "Confirmar", "Activo", "No hay perfiles de cartas guardados para esta categoría"),
		["tha"] = new CardPresetLanguagePack("พร\u0e35เซ\u0e47ต", "บ\u0e31นท\u0e36กและสล\u0e31บการต\u0e31\u0e49งค\u0e48าภาพการ\u0e4cดของหมวดน\u0e35\u0e49", "ช\u0e37\u0e48อพร\u0e35เซ\u0e47ต", "บ\u0e31นท\u0e36กป\u0e31จจ\u0e38บ\u0e31น", "ใช\u0e49", "เข\u0e35ยนท\u0e31บ", "เปล\u0e35\u0e48ยนช\u0e37\u0e48อ", "ลบ", "ย\u0e37นย\u0e31น", "ใช\u0e49อย\u0e39\u0e48", "ย\u0e31งไม\u0e48ม\u0e35พร\u0e35เซ\u0e47ตภาพการ\u0e4cดสำหร\u0e31บหมวดน\u0e35\u0e49"),
		["tur"] = new CardPresetLanguagePack("Profiller", "Bu kategorinin kart görsellerini kaydet ve değiştir.", "Profil adı", "Geçerliyi kaydet", "Uygula", "Üzerine yaz", "Yeniden adlandır", "Sil", "Onayla", "Etkin", "Bu kategori için kayıtlı kart profili yok")
	};

	private static readonly IReadOnlyDictionary<string, string> NoMonsterPresetTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = "No monster presets saved for this region",
		["zhs"] = "当前地区尚未保存怪物预设",
		["zht"] = "目前地區尚未儲存怪物預設",
		["deu"] = "Für dieses Gebiet sind keine Monsterprofile gespeichert",
		["esp"] = "No hay perfiles de monstruos guardados para esta región",
		["fra"] = "Aucun profil de monstres enregistré pour cette région",
		["ita"] = "Nessun profilo mostri salvato per questa regione",
		["jpn"] = "この地域のモンスタープリセットはありません",
		["kor"] = "이 지역에 저장된 몬스터 프리셋이 없습니다",
		["pol"] = "Brak profili potworów dla tego regionu",
		["ptb"] = "Nenhum perfil de monstro salvo para esta região",
		["rus"] = "Для этого региона нет профилей монстров",
		["spa"] = "No hay perfiles de monstruos guardados para esta región",
		["tha"] = "ย\u0e31งไม\u0e48ม\u0e35พร\u0e35เซ\u0e47ตมอนสเตอร\u0e4cสำหร\u0e31บพ\u0e37\u0e49นท\u0e35\u0e48น\u0e35\u0e49",
		["tur"] = "Bu bölge için kayıtlı canavar profili yok"
	};

	private static readonly IReadOnlyDictionary<string, AdjustmentLanguagePack> AdjustmentPacks = new Dictionary<string, AdjustmentLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new AdjustmentLanguagePack("Model", "Health bar", "Intent", "Selection box", "Health-bar scale", "Follow model scale", "Follow model movement", "Drag the selected model or health bar directly to adjust its position.", "Drag the selected model, health bar, intent, or selection box directly to adjust its position."),
		["zhs"] = new AdjustmentLanguagePack("模型", "血条", "意图", "选择框", "血条缩放", "跟随模型缩放", "跟随模型移动", "可直接拖动所选模型或血条调整位置。", "可直接拖动所选模型、血条、意图或选择框调整位置。"),
		["zht"] = new AdjustmentLanguagePack("模型", "血條", "意圖", "選擇框", "血條縮放", "跟隨模型縮放", "跟隨模型移動", "可直接拖曳所選模型或血條調整位置。", "可直接拖曳所選模型、血條、意圖或選擇框調整位置。"),
		["deu"] = new AdjustmentLanguagePack("Modell", "Lebensleiste", "Absicht", "Auswahlrahmen", "Skalierung der Lebensleiste", "Modellskalierung folgen", "Modellbewegung folgen", "Ziehe das ausgewählte Modell oder die Lebensleiste direkt, um die Position anzupassen.", "Ziehe das Modell, die Lebensleiste, die Absicht oder den Auswahlrahmen direkt, um die Position anzupassen."),
		["esp"] = new AdjustmentLanguagePack("Modelo", "Barra de vida", "Intención", "Marco de selección", "Escala de la barra", "Seguir escala del modelo", "Seguir movimiento del modelo", "Arrastra directamente el modelo o la barra de vida para ajustar su posición.", "Arrastra directamente el modelo, la barra de vida, la intención o el marco de selección para ajustar su posición."),
		["fra"] = new AdjustmentLanguagePack("Modèle", "Barre de vie", "Intention", "Cadre de sélection", "Échelle de la barre de vie", "Suivre l’échelle du modèle", "Suivre le déplacement du modèle", "Faites glisser directement le modèle ou la barre de vie pour régler sa position.", "Faites glisser le modèle, la barre de vie, l’intention ou le cadre de sélection pour régler sa position."),
		["ita"] = new AdjustmentLanguagePack("Modello", "Barra salute", "Intento", "Riquadro selezione", "Scala barra salute", "Segui scala modello", "Segui movimento modello", "Trascina direttamente il modello o la barra salute per regolarne la posizione.", "Trascina il modello, la barra salute, l’intento o il riquadro di selezione per regolarne la posizione."),
		["jpn"] = new AdjustmentLanguagePack("モデル", "HPバー", "行動予告", "選択枠", "HPバーの拡大率", "モデルの拡大率に追従", "モデルの移動に追従", "選択したモデルまたはHPバーを直接ドラッグして位置を調整できます。", "モデル、HPバー、行動予告、選択枠を直接ドラッグして位置を調整できます。"),
		["kor"] = new AdjustmentLanguagePack("모델", "체력 바", "의도", "선택 테두리", "체력 바 크기", "모델 크기 따라가기", "모델 이동 따라가기", "선택한 모델이나 체력 바를 직접 끌어 위치를 조정하세요.", "모델, 체력 바, 의도 또는 선택 테두리를 직접 끌어 위치를 조정하세요."),
		["pol"] = new AdjustmentLanguagePack("Model", "Pasek zdrowia", "Zamiar", "Ramka wyboru", "Skala paska zdrowia", "Skaluj razem z modelem", "Przesuwaj razem z modelem", "Przeciągnij bezpośrednio model lub pasek zdrowia, aby zmienić położenie.", "Przeciągnij model, pasek zdrowia, zamiar lub ramkę wyboru, aby zmienić położenie."),
		["ptb"] = new AdjustmentLanguagePack("Modelo", "Barra de vida", "Intenção", "Moldura de seleção", "Escala da barra de vida", "Seguir escala do modelo", "Seguir movimento do modelo", "Arraste diretamente o modelo ou a barra de vida para ajustar a posição.", "Arraste o modelo, a barra de vida, a intenção ou a moldura de seleção para ajustar a posição."),
		["rus"] = new AdjustmentLanguagePack("Модель", "Полоса здоровья", "Намерение", "Рамка выбора", "Масштаб полосы здоровья", "Следовать масштабу модели", "Следовать перемещению модели", "Перетаскивайте модель или полосу здоровья, чтобы настроить положение.", "Перетаскивайте модель, полосу здоровья, намерение или рамку выбора, чтобы настроить положение."),
		["spa"] = new AdjustmentLanguagePack("Modelo", "Barra de vida", "Intención", "Marco de selección", "Escala de la barra", "Seguir escala del modelo", "Seguir movimiento del modelo", "Arrastra directamente el modelo o la barra de vida para ajustar su posición.", "Arrastra directamente el modelo, la barra de vida, la intención o el marco de selección para ajustar su posición."),
		["tha"] = new AdjustmentLanguagePack("โมเดล", "แถบพล\u0e31งช\u0e35ว\u0e34ต", "เจตนา", "กรอบเล\u0e37อก", "ขนาดแถบพล\u0e31งช\u0e35ว\u0e34ต", "ปร\u0e31บขนาดตามโมเดล", "เคล\u0e37\u0e48อนตามโมเดล", "ลากโมเดลหร\u0e37อแถบพล\u0e31งช\u0e35ว\u0e34ตโดยตรงเพ\u0e37\u0e48อปร\u0e31บตำแหน\u0e48ง", "ลากโมเดล แถบพล\u0e31งช\u0e35ว\u0e34ต เจตนา หร\u0e37อกรอบเล\u0e37อกโดยตรงเพ\u0e37\u0e48อปร\u0e31บตำแหน\u0e48ง"),
		["tur"] = new AdjustmentLanguagePack("Model", "Can çubuğu", "Niyet", "Seçim çerçevesi", "Can çubuğu ölçeği", "Model ölçeğini izle", "Model hareketini izle", "Konumu ayarlamak için modeli veya can çubuğunu doğrudan sürükleyin.", "Konumu ayarlamak için modeli, can çubuğunu, niyeti veya seçim çerçevesini sürükleyin.")
	};

	private static readonly LocaleChangeCallback LocaleChangeCallback = new LocaleChangeCallback(NotifyLanguageChanged);

	private static LocManager? _subscribedManager;

	private static readonly IReadOnlyDictionary<string, CardPortraitModeLanguagePack> CardPortraitModePacks = new Dictionary<string, CardPortraitModeLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new CardPortraitModeLanguagePack("Differential", "Ancient style", "Ancient differential"),
		["zhs"] = new CardPortraitModeLanguagePack("差分", "先古样式", "先古差分"),
		["zht"] = new CardPortraitModeLanguagePack("差分", "先古樣式", "先古差分"),
		["deu"] = new CardPortraitModeLanguagePack("Differenzbild", "Ahnenstil", "Ahnen-Differenzbild"),
		["esp"] = new CardPortraitModeLanguagePack("Diferencial", "Estilo ancestral", "Diferencial ancestral"),
		["fra"] = new CardPortraitModeLanguagePack("Variante", "Style ancestral", "Variante ancestrale"),
		["ita"] = new CardPortraitModeLanguagePack("Differenziale", "Stile ancestrale", "Differenziale ancestrale"),
		["jpn"] = new CardPortraitModeLanguagePack("差分", "古代様式", "古代差分"),
		["kor"] = new CardPortraitModeLanguagePack("차분", "고대 양식", "고대 차분"),
		["pol"] = new CardPortraitModeLanguagePack("Wariant", "Styl starożytny", "Starożytny wariant"),
		["ptb"] = new CardPortraitModeLanguagePack("Variante", "Estilo ancestral", "Variante ancestral"),
		["rus"] = new CardPortraitModeLanguagePack("Вариант", "Древний стиль", "Древний вариант"),
		["spa"] = new CardPortraitModeLanguagePack("Diferencial", "Estilo ancestral", "Diferencial ancestral"),
		["tha"] = new CardPortraitModeLanguagePack("ภาพต\u0e48าง", "ร\u0e39ปแบบโบราณ", "ภาพต\u0e48างแบบโบราณ"),
		["tur"] = new CardPortraitModeLanguagePack("Varyant", "Kadim stil", "Kadim varyant")
	};

	private static readonly IReadOnlyDictionary<string, string> MultiplayerSkinLoadingTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = "Receive skin changes",
		["zhs"] = "接收皮肤改变",
		["zht"] = "接收外觀變更",
		["deu"] = "Skin-Änderungen empfangen",
		["esp"] = "Recibir cambios de aspecto",
		["fra"] = "Recevoir les changements d’apparence",
		["ita"] = "Ricevi modifiche alle skin",
		["jpn"] = "スキン変更を受信",
		["kor"] = "스킨 변경 받기",
		["pol"] = "Odbieraj zmiany skórek",
		["ptb"] = "Receber alterações de visual",
		["rus"] = "Получать изменения обликов",
		["spa"] = "Recibir cambios de aspecto",
		["tha"] = "ร\u0e31บการเปล\u0e35\u0e48ยนแปลงสก\u0e34น",
		["tur"] = "Görünüm değişikliklerini al"
	};

	private static readonly IReadOnlyDictionary<string, string> MultiplayerSkinSyncTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = "Send skin changes",
		["zhs"] = "发送皮肤改变",
		["zht"] = "傳送外觀變更",
		["deu"] = "Skin-Änderungen senden",
		["esp"] = "Enviar cambios de aspecto",
		["fra"] = "Envoyer les changements d’apparence",
		["ita"] = "Invia modifiche alle skin",
		["jpn"] = "スキン変更を送信",
		["kor"] = "스킨 변경 보내기",
		["pol"] = "Wysyłaj zmiany skórek",
		["ptb"] = "Enviar alterações de visual",
		["rus"] = "Отправлять изменения обликов",
		["spa"] = "Enviar cambios de aspecto",
		["tha"] = "ส\u0e48งการเปล\u0e35\u0e48ยนแปลงสก\u0e34น",
		["tur"] = "Görünüm değişikliklerini gönder"
	};

	private static readonly IReadOnlyDictionary<string, string> HideAppearanceHoldHintTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = "Right-click to hide; right-click Resume to show it again",
		["zhs"] = "右键隐藏；右键“继续游戏”可恢复",
		["zht"] = "右鍵隱藏；右鍵「繼續遊戲」可恢復",
		["deu"] = "Rechtsklick zum Ausblenden; Rechtsklick auf „Fortsetzen“ zum Einblenden",
		["esp"] = "Clic derecho para ocultar; clic derecho en «Continuar» para mostrar",
		["fra"] = "Clic droit pour masquer ; clic droit sur « Reprendre » pour réafficher",
		["ita"] = "Clic destro per nascondere; clic destro su «Riprendi» per mostrare",
		["jpn"] = "右クリックで非表示。「ゲームに戻る」を右クリックで再表示",
		["kor"] = "오른쪽 클릭으로 숨기기. ‘계속하기’를 오른쪽 클릭하면 다시 표시",
		["pol"] = "Prawy klik ukrywa; prawy klik „Wznów” pokazuje ponownie",
		["ptb"] = "Clique direito para ocultar; clique direito em “Continuar” para mostrar",
		["rus"] = "ПКМ — скрыть; ПКМ по «Продолжить» — вернуть",
		["spa"] = "Clic derecho para ocultar; clic derecho en «Continuar» para mostrar",
		["tha"] = "คล\u0e34กขวาเพ\u0e37\u0e48อซ\u0e48อน คล\u0e34กขวา “เล\u0e48นต\u0e48อ” เพ\u0e37\u0e48อแสดงอ\u0e35กคร\u0e31\u0e49ง",
		["tur"] = "Gizlemek için sağ tıkla; göstermek için “Devam et”e sağ tıkla"
	};

	private static readonly IReadOnlyDictionary<string, string> ShowAppearanceHoldHintTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = "Right-click to show the Appearance button",
		["zhs"] = "右键显示“外观”按钮",
		["zht"] = "右鍵顯示「外觀」按鈕",
		["deu"] = "Rechtsklick zeigt „Aussehen“ wieder an",
		["esp"] = "Clic derecho para mostrar el botón «Aspecto»",
		["fra"] = "Clic droit pour afficher le bouton « Apparence »",
		["ita"] = "Clic destro per mostrare il pulsante «Aspetto»",
		["jpn"] = "右クリックで「外見」ボタンを表示",
		["kor"] = "오른쪽 클릭으로 ‘외형’ 버튼 표시",
		["pol"] = "Prawy klik pokazuje przycisk „Wygląd”",
		["ptb"] = "Clique direito para mostrar o botão “Visual”",
		["rus"] = "ПКМ — показать кнопку «Облик»",
		["spa"] = "Clic derecho para mostrar el botón «Aspecto»",
		["tha"] = "คล\u0e34กขวาเพ\u0e37\u0e48อแสดงป\u0e38\u0e48ม “ร\u0e39ปล\u0e31กษณ\u0e4c”",
		["tur"] = "“Görünüm” düğmesini göstermek için sağ tıkla"
	};

	private static readonly IReadOnlyDictionary<string, string> CardSkinSelectorDragHintTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = "Drag to move; right-click to reset",
		["zhs"] = "拖动移动；右键重置",
		["zht"] = "拖曳移動；右鍵重設",
		["deu"] = "Ziehen zum Verschieben; Rechtsklick zum Zurücksetzen",
		["esp"] = "Arrastra para mover; clic derecho para restablecer",
		["fra"] = "Faire glisser pour déplacer ; clic droit pour réinitialiser",
		["ita"] = "Trascina per spostare; clic destro per ripristinare",
		["jpn"] = "ドラッグで移動、右クリックでリセット",
		["kor"] = "드래그하여 이동, 오른쪽 클릭으로 초기화",
		["pol"] = "Przeciągnij, aby przesunąć; prawy klik resetuje",
		["ptb"] = "Arraste para mover; clique direito para redefinir",
		["rus"] = "Перетащите для перемещения; ПКМ — сброс",
		["spa"] = "Arrastra para mover; clic derecho para restablecer",
		["tha"] = "ลากเพ\u0e37\u0e48อย\u0e49าย; คล\u0e34กขวาเพ\u0e37\u0e48อร\u0e35เซ\u0e47ต",
		["tur"] = "Taşımak için sürükle; sıfırlamak için sağ tıkla"
	};

	private static readonly IReadOnlyDictionary<string, CharacterSkinCompositionLanguagePack> CharacterSkinCompositionPacks = new Dictionary<string, CharacterSkinCompositionLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new CharacterSkinCompositionLanguagePack("Merge skins", "New merge", "Merged skin name", "Hide source skins from the skin list", "Save", "Delete", "Confirm Delete", "Unavailable", "Select at least one skin", "Combined Skin"),
		["zhs"] = new CharacterSkinCompositionLanguagePack("皮肤合并", "新建合并", "合并皮肤名称", "在皮肤列表中隐藏参与合并的皮肤", "保存", "删除", "确认删除", "不可用", "请至少选择一个皮肤", "合并皮肤"),
		["zht"] = new CharacterSkinCompositionLanguagePack("外觀合併", "新增合併", "合併外觀名稱", "在外觀列表中隱藏參與合併的外觀", "儲存", "刪除", "確認刪除", "無法使用", "請至少選擇一個外觀", "合併外觀"),
		["deu"] = new CharacterSkinCompositionLanguagePack("Skins kombinieren", "Neue Kombination", "Name der Kombination", "Quell-Skins in der Skin-Liste ausblenden", "Speichern", "Löschen", "Bestätigen", "Nicht verfügbar", "Wähle mindestens einen Skin", "Kombinierter Skin"),
		["esp"] = new CharacterSkinCompositionLanguagePack("Combinar aspectos", "Nueva combinación", "Nombre del aspecto combinado", "Ocultar los aspectos de origen en la lista", "Guardar", "Eliminar", "Confirmar", "No disponible", "Selecciona al menos un aspecto", "Aspecto combinado"),
		["fra"] = new CharacterSkinCompositionLanguagePack("Fusionner les skins", "Nouvelle fusion", "Nom du skin fusionné", "Masquer les skins sources dans la liste", "Enregistrer", "Supprimer", "Confirmer", "Indisponible", "Sélectionnez au moins un skin", "Skin fusionné"),
		["ita"] = new CharacterSkinCompositionLanguagePack("Combina skin", "Nuova combinazione", "Nome della skin combinata", "Nascondi le skin sorgente dall’elenco", "Salva", "Elimina", "Conferma", "Non disponibile", "Seleziona almeno una skin", "Skin combinata"),
		["jpn"] = new CharacterSkinCompositionLanguagePack("スキン合成", "新しい合成", "合成スキン名", "素材スキンを一覧から隠す", "保存", "削除", "削除を確認", "利用不可", "スキンを1つ以上選択してください", "合成スキン"),
		["kor"] = new CharacterSkinCompositionLanguagePack("스킨 합치기", "새 합치기", "합친 스킨 이름", "스킨 목록에서 원본 스킨 숨기기", "저장", "삭제", "삭제 확인", "사용 불가", "스킨을 하나 이상 선택하세요", "합친 스킨"),
		["pol"] = new CharacterSkinCompositionLanguagePack("Połącz skórki", "Nowe połączenie", "Nazwa połączonej skórki", "Ukryj skórki źródłowe na liście", "Zapisz", "Usuń", "Potwierdź", "Niedostępne", "Wybierz co najmniej jedną skórkę", "Połączona skórka"),
		["ptb"] = new CharacterSkinCompositionLanguagePack("Combinar visuais", "Nova combinação", "Nome do visual combinado", "Ocultar visuais de origem da lista", "Salvar", "Excluir", "Confirmar", "Indisponível", "Selecione pelo menos um visual", "Visual combinado"),
		["rus"] = new CharacterSkinCompositionLanguagePack("Объединить облики", "Новое объединение", "Название объединённого облика", "Скрыть исходные облики из списка", "Сохранить", "Удалить", "Подтвердить", "Недоступно", "Выберите хотя бы один облик", "Объединённый облик"),
		["spa"] = new CharacterSkinCompositionLanguagePack("Combinar aspectos", "Nueva combinación", "Nombre del aspecto combinado", "Ocultar los aspectos de origen en la lista", "Guardar", "Eliminar", "Confirmar", "No disponible", "Selecciona al menos un aspecto", "Aspecto combinado"),
		["tha"] = new CharacterSkinCompositionLanguagePack("รวมสก\u0e34น", "สร\u0e49างการรวมใหม\u0e48", "ช\u0e37\u0e48อสก\u0e34นท\u0e35\u0e48รวมแล\u0e49ว", "ซ\u0e48อนสก\u0e34นต\u0e49นทางจากรายการ", "บ\u0e31นท\u0e36ก", "ลบ", "ย\u0e37นย\u0e31นการลบ", "ใช\u0e49ไม\u0e48ได\u0e49", "เล\u0e37อกสก\u0e34นอย\u0e48างน\u0e49อยหน\u0e36\u0e48งรายการ", "สก\u0e34นรวม"),
		["tur"] = new CharacterSkinCompositionLanguagePack("Görünümleri birleştir", "Yeni birleştirme", "Birleşik görünüm adı", "Kaynak görünümleri listede gizle", "Kaydet", "Sil", "Silmeyi onayla", "Kullanılamıyor", "En az bir görünüm seçin", "Birleşik görünüm")
	};

	private static readonly IReadOnlyDictionary<string, string> RestorePlayerPositionTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = "Restore player position",
		["zhs"] = "恢复角色位置",
		["zht"] = "恢復角色位置",
		["deu"] = "Figur zurücksetzen",
		["esp"] = "Restaurar personaje",
		["fra"] = "Replacer le personnage",
		["ita"] = "Ripristina personaggio",
		["jpn"] = "キャラクター位置を戻す",
		["kor"] = "캐릭터 위치 복원",
		["pol"] = "Przywróć postać",
		["ptb"] = "Restaurar personagem",
		["rus"] = "Вернуть персонажа",
		["spa"] = "Restaurar personaje",
		["tha"] = "ค\u0e37นตำแหน\u0e48งต\u0e31วละคร",
		["tur"] = "Karakteri geri getir"
	};

	private static readonly IReadOnlyDictionary<string, MultiplayerProgressLanguagePack> MultiplayerProgressPacks = new Dictionary<string, MultiplayerProgressLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new MultiplayerProgressLanguagePack("Preparing {0}…", "Waiting until everyone is ready: {0}", "Checking {0} in Workshop…", "Downloading {0}: {1}", "Verifying {0}…", "Applying {0}…", "Loaded {0}", "Could not load {0}; using the original skin"),
		["zhs"] = new MultiplayerProgressLanguagePack("正在准备 {0}…", "等待所有玩家准备后加载 {0}", "正在检查工坊资源 {0}…", "正在下载 {0}：{1}", "正在校验 {0}…", "正在应用 {0}…", "已加载 {0}", "无法加载 {0}，已使用原皮"),
		["zht"] = new MultiplayerProgressLanguagePack("正在準備 {0}…", "等待所有玩家準備後載入 {0}", "正在檢查工作坊資源 {0}…", "正在下載 {0}：{1}", "正在驗證 {0}…", "正在套用 {0}…", "已載入 {0}", "無法載入 {0}，已使用原始外觀"),
		["deu"] = new MultiplayerProgressLanguagePack("{0} wird vorbereitet…", "Warte auf alle Spieler: {0}", "Workshop-Dateien für {0} werden geprüft…", "{0} wird heruntergeladen: {1}", "{0} wird überprüft…", "{0} wird angewendet…", "{0} geladen", "{0} konnte nicht geladen werden; Original-Skin wird verwendet"),
		["esp"] = new MultiplayerProgressLanguagePack("Preparando {0}…", "Esperando a que todos estén listos: {0}", "Comprobando {0} en Workshop…", "Descargando {0}: {1}", "Verificando {0}…", "Aplicando {0}…", "{0} cargado", "No se pudo cargar {0}; se usa el aspecto original"),
		["fra"] = new MultiplayerProgressLanguagePack("Préparation de {0}…", "En attente de tous les joueurs : {0}", "Vérification de {0} dans le Workshop…", "Téléchargement de {0} : {1}", "Vérification de {0}…", "Application de {0}…", "{0} chargé", "Impossible de charger {0} ; skin d’origine utilisé"),
		["ita"] = new MultiplayerProgressLanguagePack("Preparazione di {0}…", "In attesa che tutti siano pronti: {0}", "Controllo di {0} nel Workshop…", "Download di {0}: {1}", "Verifica di {0}…", "Applicazione di {0}…", "{0} caricato", "Impossibile caricare {0}; uso della skin originale"),
		["jpn"] = new MultiplayerProgressLanguagePack("{0}を準備中…", "全員の準備完了を待っています：{0}", "Workshopの{0}を確認中…", "{0}をダウンロード中：{1}", "{0}を検証中…", "{0}を適用中…", "{0}を読み込みました", "{0}を読み込めないため、原版スキンを使用します"),
		["kor"] = new MultiplayerProgressLanguagePack("{0} 준비 중…", "모든 플레이어의 준비를 기다리는 중: {0}", "Workshop에서 {0} 확인 중…", "{0} 다운로드 중: {1}", "{0} 검증 중…", "{0} 적용 중…", "{0} 불러옴", "{0}을 불러올 수 없어 원본 스킨을 사용합니다"),
		["pol"] = new MultiplayerProgressLanguagePack("Przygotowywanie {0}…", "Oczekiwanie na gotowość wszystkich: {0}", "Sprawdzanie {0} w Warsztacie…", "Pobieranie {0}: {1}", "Weryfikowanie {0}…", "Stosowanie {0}…", "Wczytano {0}", "Nie udało się wczytać {0}; użyto oryginalnej skórki"),
		["ptb"] = new MultiplayerProgressLanguagePack("Preparando {0}…", "Aguardando todos ficarem prontos: {0}", "Verificando {0} na Oficina…", "Baixando {0}: {1}", "Verificando {0}…", "Aplicando {0}…", "{0} carregado", "Não foi possível carregar {0}; usando o visual original"),
		["rus"] = new MultiplayerProgressLanguagePack("Подготовка {0}…", "Ожидание готовности всех игроков: {0}", "Проверка {0} в Мастерской…", "Загрузка {0}: {1}", "Проверка {0}…", "Применение {0}…", "{0} загружен", "Не удалось загрузить {0}; используется оригинальный облик"),
		["spa"] = new MultiplayerProgressLanguagePack("Preparando {0}…", "Esperando a que todos estén listos: {0}", "Comprobando {0} en Workshop…", "Descargando {0}: {1}", "Verificando {0}…", "Aplicando {0}…", "{0} cargado", "No se pudo cargar {0}; se usa el aspecto original"),
		["tha"] = new MultiplayerProgressLanguagePack("กำล\u0e31งเตร\u0e35ยม {0}…", "กำล\u0e31งรอให\u0e49ผ\u0e39\u0e49เล\u0e48นท\u0e38กคนพร\u0e49อม: {0}", "กำล\u0e31งตรวจสอบ {0} ในเว\u0e34ร\u0e4cกชอป…", "กำล\u0e31งดาวน\u0e4cโหลด {0}: {1}", "กำล\u0e31งตรวจสอบ {0}…", "กำล\u0e31งใช\u0e49 {0}…", "โหลด {0} แล\u0e49ว", "โหลด {0} ไม\u0e48สำเร\u0e47จ จ\u0e36งใช\u0e49สก\u0e34นด\u0e31\u0e49งเด\u0e34ม"),
		["tur"] = new MultiplayerProgressLanguagePack("{0} hazırlanıyor…", "Herkesin hazır olması bekleniyor: {0}", "Atölyede {0} denetleniyor…", "{0} indiriliyor: {1}", "{0} doğrulanıyor…", "{0} uygulanıyor…", "{0} yüklendi", "{0} yüklenemedi; özgün görünüm kullanılıyor")
	};

	private static readonly IReadOnlyDictionary<string, MultiplayerFailureLanguagePack> MultiplayerFailurePacks = new Dictionary<string, MultiplayerFailureLanguagePack>(StringComparer.OrdinalIgnoreCase)
	{
		["eng"] = new MultiplayerFailureLanguagePack("Custom skin could not be loaded", "[b]{0}[/b] could not be loaded for this multiplayer run.\n\nReason: {1}\n\nAfter confirmation, this player will use the original skin on this computer."),
		["zhs"] = new MultiplayerFailureLanguagePack("无法加载联机皮肤", "本次联机无法加载 [b]{0}[/b]。\n\n原因：{1}\n\n确认后，该玩家将在本机使用原皮。"),
		["zht"] = new MultiplayerFailureLanguagePack("無法載入連線外觀", "本次連線無法載入 [b]{0}[/b]。\n\n原因：{1}\n\n確認後，該玩家將在此電腦使用原始外觀。"),
		["deu"] = new MultiplayerFailureLanguagePack("Online-Skin konnte nicht geladen werden", "[b]{0}[/b] konnte für diese Mehrspielerpartie nicht geladen werden.\n\nGrund: {1}\n\nNach der Bestätigung wird für diesen Spieler auf diesem PC der Original-Skin verwendet."),
		["esp"] = new MultiplayerFailureLanguagePack("No se pudo cargar el aspecto en línea", "No se pudo cargar [b]{0}[/b] en esta partida multijugador.\n\nMotivo: {1}\n\nTras confirmar, este jugador usará el aspecto original en este equipo."),
		["fra"] = new MultiplayerFailureLanguagePack("Impossible de charger le skin en ligne", "[b]{0}[/b] n’a pas pu être chargé pour cette partie multijoueur.\n\nRaison : {1}\n\nAprès confirmation, ce joueur utilisera le skin d’origine sur cet ordinateur."),
		["ita"] = new MultiplayerFailureLanguagePack("Impossibile caricare la skin online", "Impossibile caricare [b]{0}[/b] per questa partita multigiocatore.\n\nMotivo: {1}\n\nDopo la conferma, questo giocatore userà la skin originale su questo computer."),
		["jpn"] = new MultiplayerFailureLanguagePack("オンラインスキンを読み込めません", "このマルチプレイでは [b]{0}[/b] を読み込めませんでした。\n\n理由：{1}\n\n確認後、この端末では対象プレイヤーに原版スキンを使用します。"),
		["kor"] = new MultiplayerFailureLanguagePack("온라인 스킨을 불러올 수 없음", "이번 멀티플레이에서 [b]{0}[/b]을 불러올 수 없습니다.\n\n원인: {1}\n\n확인하면 이 컴퓨터에서는 해당 플레이어에게 원본 스킨을 사용합니다."),
		["pol"] = new MultiplayerFailureLanguagePack("Nie udało się wczytać skórki online", "Nie udało się wczytać [b]{0}[/b] w tej grze wieloosobowej.\n\nPowód: {1}\n\nPo potwierdzeniu ten gracz użyje na tym komputerze oryginalnej skórki."),
		["ptb"] = new MultiplayerFailureLanguagePack("Não foi possível carregar o visual online", "Não foi possível carregar [b]{0}[/b] nesta partida multijogador.\n\nMotivo: {1}\n\nApós confirmar, este jogador usará o visual original neste computador."),
		["rus"] = new MultiplayerFailureLanguagePack("Не удалось загрузить сетевой облик", "Не удалось загрузить [b]{0}[/b] для этой сетевой игры.\n\nПричина: {1}\n\nПосле подтверждения на этом компьютере для игрока будет использован оригинальный облик."),
		["spa"] = new MultiplayerFailureLanguagePack("No se pudo cargar el aspecto en línea", "No se pudo cargar [b]{0}[/b] en esta partida multijugador.\n\nMotivo: {1}\n\nTras confirmar, este jugador usará el aspecto original en este equipo."),
		["tha"] = new MultiplayerFailureLanguagePack("โหลดสก\u0e34นออนไลน\u0e4cไม\u0e48สำเร\u0e47จ", "ไม\u0e48สามารถโหลด [b]{0}[/b] สำหร\u0e31บเกมหลายผ\u0e39\u0e49เล\u0e48นคร\u0e31\u0e49งน\u0e35\u0e49ได\u0e49\n\nสาเหต\u0e38: {1}\n\nหล\u0e31งย\u0e37นย\u0e31น ผ\u0e39\u0e49เล\u0e48นน\u0e35\u0e49จะใช\u0e49สก\u0e34นด\u0e31\u0e49งเด\u0e34มบนเคร\u0e37\u0e48องน\u0e35\u0e49"),
		["tur"] = new MultiplayerFailureLanguagePack("Çevrimiçi görünüm yüklenemedi", "Bu çok oyunculu oyun için [b]{0}[/b] yüklenemedi.\n\nNeden: {1}\n\nOnaydan sonra bu oyuncu bu bilgisayarda özgün görünümü kullanacak.")
	};

	private static readonly IReadOnlyDictionary<string, string> EventCategoryTexts = new Dictionary<string, string>
	{
		["eng"] = "Events",
		["zhs"] = "事件",
		["zht"] = "事件",
		["deu"] = "Ereignisse",
		["esp"] = "Eventos",
		["spa"] = "Eventos",
		["fra"] = "Événements",
		["ita"] = "Eventi",
		["jpn"] = "イベント",
		["kor"] = "이벤트",
		["pol"] = "Wydarzenia",
		["ptb"] = "Eventos",
		["rus"] = "События",
		["tha"] = "เหต\u0e38การณ\u0e4c",
		["tur"] = "Olaylar"
	};

	private static readonly IReadOnlyDictionary<string, string> RandomCharacterSkinTexts = new Dictionary<string, string>
	{
		["eng"] = "Random skin",
		["zhs"] = "随机皮肤",
		["zht"] = "隨機外觀",
		["deu"] = "Zufälliger Skin",
		["esp"] = "Aspecto aleatorio",
		["fra"] = "Apparence aléatoire",
		["ita"] = "Aspetto casuale",
		["jpn"] = "ランダムスキン",
		["kor"] = "무작위 스킨",
		["pol"] = "Losowa skórka",
		["ptb"] = "Visual aleatório",
		["rus"] = "Случайный облик",
		["spa"] = "Aspecto aleatorio",
		["tha"] = "สก\u0e34นส\u0e38\u0e48ม",
		["tur"] = "Rastgele görünüm"
	};

	internal static string BundleCards => BundleContentTexts[CurrentLanguage].Cards;

	internal static string BundleMonsters => BundleContentTexts[CurrentLanguage].Monsters;

	internal static string BundleMultiplePresets => BundleContentTexts[CurrentLanguage].Presets;

	internal static string BundleModPriority => BundleContentTexts[CurrentLanguage].Priority;

	internal static string BundleHideSource => BundlePresetTexts[CurrentLanguage].Hide;

	internal static string BundlePresetLocked => BundlePresetTexts[CurrentLanguage].Locked;

	public static IReadOnlyCollection<string> SupportedLanguages => (IReadOnlyCollection<string>)(object)Packs.Keys.ToArray();

	public static string CurrentLanguage
	{
		get
		{
			LocManager instance = LocManager.Instance;
			string text = ((instance != null) ? instance.Language : null);
			if (string.IsNullOrWhiteSpace(text) || !Packs.ContainsKey(text))
			{
				return "eng";
			}
			return text;
		}
	}

	private static event Action? LanguageChanged;

	internal static string FormatAppearanceTargetHint(IEnumerable<AppearanceTargetKind> kinds, string? language = null)
	{
		AppearanceTargetHintPack pack = AppearanceTargetHintPacks.GetValueOrDefault(language ?? CurrentLanguage) ?? AppearanceTargetHintPacks["eng"];
		string[] array = (from kind in kinds.Distinct()
			orderby kind
			where kind >= AppearanceTargetKind.Character && (int)kind < pack.Names.Length
			select pack.Names[(int)kind]).ToArray();
		if (array.Length != 0)
		{
			return string.Format(pack.Template, string.Join(pack.Separator, array));
		}
		return pack.Empty;
	}

	internal static string GetOnlineSkinFailureTitle()
	{
		return MultiplayerFailurePacks[CurrentLanguage].Title;
	}

	internal static string FormatOnlineSkinFailure(string providerId, string detail)
	{
		return string.Format(MultiplayerFailurePacks[CurrentLanguage].Message, providerId, detail);
	}

	internal static string FormatOnlineSkinCacheProgress(OnlineSkinCacheProgress progress)
	{
		string arg = (string.IsNullOrWhiteSpace(progress.ProviderId) ? "Skin" : progress.ProviderId);
		MultiplayerProgressLanguagePack multiplayerProgressLanguagePack = MultiplayerProgressPacks[CurrentLanguage];
		return progress.Stage switch
		{
			OnlineSkinCacheStage.Preparing => string.Format(multiplayerProgressLanguagePack.Preparing, arg), 
			OnlineSkinCacheStage.WaitingForReady => string.Format(multiplayerProgressLanguagePack.WaitingForReady, arg), 
			OnlineSkinCacheStage.CheckingWorkshop => string.Format(multiplayerProgressLanguagePack.CheckingWorkshop, arg), 
			OnlineSkinCacheStage.Downloading => string.Format(multiplayerProgressLanguagePack.Downloading, arg, (progress.TotalBytes != 0) ? $"{Math.Clamp((double)progress.DownloadedBytes * 100.0 / (double)progress.TotalBytes, 0.0, 100.0):F0}%" : "…"), 
			OnlineSkinCacheStage.Verifying => string.Format(multiplayerProgressLanguagePack.Verifying, arg), 
			OnlineSkinCacheStage.Applying => string.Format(multiplayerProgressLanguagePack.Applying, arg), 
			OnlineSkinCacheStage.Complete => string.Format(multiplayerProgressLanguagePack.Complete, arg), 
			OnlineSkinCacheStage.Failed => string.Format(multiplayerProgressLanguagePack.Failed, arg), 
			_ => string.Empty, 
		};
	}

	public static string Get(ModText text)
	{
		if (text != ModText.RandomCharacterSkin)
		{
			if (text != ModText.GameDefault)
			{
				if (text != ModText.OtherCategoryEvents)
				{
					if (text != ModText.NoMonsterPresets)
					{
						if (text != ModText.MultiplayerSkinSync)
						{
							if (text != ModText.LoadOtherPlayersCustomSkins)
							{
								if (text != ModText.HideAppearanceHoldHint)
								{
									if (text != ModText.ShowAppearanceHoldHint)
									{
										if (text != ModText.CardSkinSelectorDragHint)
										{
											if (text != ModText.RestorePlayerPosition)
											{
												if (text != ModText.OtherCompendium)
												{
													if (text != ModText.OtherCategoryAncients)
													{
														if (text != ModText.OtherCategoryMerchants)
														{
															if (text != ModText.OtherCategoryCreatures)
															{
																if (text < ModText.CharacterSkinBundle)
																{
																	if (text < ModText.CharacterSkinMerge)
																	{
																		if (text < ModText.ModelTransform)
																		{
																			if (text < ModText.CharacterAppearance)
																			{
																				if (text < ModText.CardPresets)
																				{
																					if (text < ModText.CardSkinPriority)
																					{
																						return Packs[CurrentLanguage].Get(text);
																					}
																					return CardPriorityPacks[CurrentLanguage].Get(text);
																				}
																				return CardPresetPacks[CurrentLanguage].Get(text);
																			}
																			return AppearancePacks[CurrentLanguage].Get(text);
																		}
																		return AdjustmentPacks[CurrentLanguage].Get(text);
																	}
																	return CharacterSkinCompositionPacks[CurrentLanguage].Get(text);
																}
																return BundlePacks[CurrentLanguage][(int)(text - 81)];
															}
															return OtherCompendiumPacks[CurrentLanguage].Creatures;
														}
														return OtherCompendiumPacks[CurrentLanguage].Merchants;
													}
													return OtherCompendiumPacks[CurrentLanguage].Ancients;
												}
												return OtherCompendiumPacks[CurrentLanguage].Title;
											}
											return RestorePlayerPositionTexts[CurrentLanguage];
										}
										return CardSkinSelectorDragHintTexts[CurrentLanguage];
									}
									return ShowAppearanceHoldHintTexts[CurrentLanguage];
								}
								return HideAppearanceHoldHintTexts[CurrentLanguage];
							}
							return MultiplayerSkinLoadingTexts[CurrentLanguage];
						}
						return MultiplayerSkinSyncTexts[CurrentLanguage];
					}
					return NoMonsterPresetTexts[CurrentLanguage];
				}
				return EventCategoryTexts[CurrentLanguage];
			}
			return "◈" + Packs[CurrentLanguage].Get(ModText.GameDefault);
		}
		return "⁇" + RandomCharacterSkinTexts[CurrentLanguage];
	}

	public static string DisplayOptionName(string name)
	{
		CardPortraitModeLanguagePack cardPortraitModeLanguagePack = CardPortraitModePacks[CurrentLanguage];
		return ReplaceSuffix(name, "{skin-changer-default}", Get(ModText.DefaultVariant)) ?? ReplaceSuffix(name, "{skin-changer-differential}", cardPortraitModeLanguagePack.Differential) ?? ReplaceSuffix(name, "{skin-changer-ancient-style}", cardPortraitModeLanguagePack.AncientStyle) ?? ReplaceSuffix(name, "{skin-changer-ancient-differential}", cardPortraitModeLanguagePack.AncientDifferential) ?? name;
		static string? ReplaceSuffix(string name, string marker, string replacement)
		{
			string text = " · " + marker;
			if (!name.EndsWith(text, StringComparison.Ordinal))
			{
				return null;
			}
			int length = text.Length;
			return name.Substring(0, name.Length - length) + " · " + replacement;
		}
	}

	public static void Bind(Node owner, Action refresh)
	{
		EnsureSubscribed();
		Action handler = null;
		handler = delegate
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)owner))
			{
				LanguageChanged -= handler;
				return;
			}
			try
			{
				refresh();
			}
			catch (Exception ex)
			{
				ModLog.Warn("刷新 Mod 本地化界面失败：" + ex.GetBaseException().Message);
			}
		};
		LanguageChanged += handler;
		owner.TreeExited += delegate
		{
			LanguageChanged -= handler;
		};
		handler();
	}

	internal static void NotifyLanguageChanged()
	{
		foreach (Action item in ModLocalization.LanguageChanged?.GetInvocationList().Cast<Action>() ?? Array.Empty<Action>())
		{
			item();
		}
	}

	private static void EnsureSubscribed()
	{
		LocManager instance = LocManager.Instance;
		if (instance != null && instance != _subscribedManager)
		{
			LocManager? subscribedManager = _subscribedManager;
			if (subscribedManager != null)
			{
				subscribedManager.UnsubscribeToLocaleChange(LocaleChangeCallback);
			}
			instance.SubscribeToLocaleChange(LocaleChangeCallback);
			_subscribedManager = instance;
		}
	}
}
