namespace STS2SkinChanger.Core;

internal static partial class ModLocalization
{
    // 「把怪物（含商人）皮肤包设置复制到所有英雄」按钮与提示，15 种语言。
    private static readonly IReadOnlyDictionary<string, (string Apply, string Confirm, string Done, string Nothing)>
        MonsterCopyTexts = new Dictionary<string, (string, string, string, string)>
        {
            ["eng"] = ("Apply monster settings to all heroes", "Click again to confirm: this overwrites every other hero's monster settings.",
                "Monster settings copied to {0} bundle(s).", "No other hero has a skin bundle yet."),
            ["zhs"] = ("把怪物设置应用到所有英雄", "再次点击确认：这会覆盖其他英雄的怪物设置。",
                "已把怪物设置复制到 {0} 个皮肤包。", "其他英雄还没有皮肤包。"),
            ["zht"] = ("把怪物設定套用到所有英雄", "再按一次確認：這會覆寫其他英雄的怪物設定。",
                "已把怪物設定複製到 {0} 個外觀組合包。", "其他英雄還沒有外觀組合包。"),
            ["deu"] = ("Monster-Einstellungen auf alle Helden anwenden", "Erneut klicken zum Bestätigen: überschreibt die Monster-Einstellungen aller anderen Helden.",
                "Monster-Einstellungen in {0} Paket(e) kopiert.", "Kein anderer Held hat bereits ein Skin-Paket."),
            ["esp"] = ("Aplicar ajustes de monstruos a todos los héroes", "Haz clic otra vez para confirmar: sobrescribe los ajustes de monstruos de los demás héroes.",
                "Ajustes de monstruos copiados a {0} paquete(s).", "Ningún otro héroe tiene todavía un paquete de aspecto."),
            ["spa"] = ("Aplicar ajustes de monstruos a todos los héroes", "Haz clic otra vez para confirmar: sobrescribe los ajustes de monstruos de los demás héroes.",
                "Ajustes de monstruos copiados a {0} paquete(s).", "Ningún otro héroe tiene todavía un paquete de aspecto."),
            ["fra"] = ("Appliquer les réglages de monstres à tous les héros", "Cliquez à nouveau pour confirmer : cela écrase les réglages de monstres des autres héros.",
                "Réglages de monstres copiés dans {0} pack(s).", "Aucun autre héros n'a encore de pack d'apparence."),
            ["ita"] = ("Applica le impostazioni dei mostri a tutti gli eroi", "Clicca di nuovo per confermare: sovrascrive le impostazioni dei mostri degli altri eroi.",
                "Impostazioni dei mostri copiate in {0} pacchetto/i.", "Nessun altro eroe ha ancora un pacchetto di skin."),
            ["jpn"] = ("モンスター設定を全ヒーローに適用", "もう一度クリックで確定：他のヒーローのモンスター設定を上書きします。",
                "モンスター設定を {0} 個のスキンパックにコピーしました。", "他のヒーローにはまだスキンパックがありません。"),
            ["kor"] = ("몬스터 설정을 모든 영웅에게 적용", "다시 클릭하면 확인됩니다. 다른 영웅의 몬스터 설정을 덮어씁니다.",
                "몬스터 설정을 스킨 묶음 {0}개에 복사했습니다.", "다른 영웅에게는 아직 스킨 묶음이 없습니다."),
            ["pol"] = ("Zastosuj ustawienia potworów u wszystkich bohaterów", "Kliknij ponownie, aby potwierdzić: nadpisze ustawienia potworów innych bohaterów.",
                "Skopiowano ustawienia potworów do {0} pakietu/pakietów.", "Żaden inny bohater nie ma jeszcze pakietu skórek."),
            ["ptb"] = ("Aplicar ajustes de monstros a todos os heróis", "Clique novamente para confirmar: sobrescreve os ajustes de monstros dos outros heróis.",
                "Ajustes de monstros copiados para {0} pacote(s).", "Nenhum outro herói tem um pacote de skins ainda."),
            ["rus"] = ("Применить настройки монстров ко всем героям", "Нажмите ещё раз для подтверждения: это перезапишет настройки монстров других героев.",
                "Настройки монстров скопированы в {0} набор(ов).", "У других героев ещё нет набора обликов."),
            ["tha"] = ("ใช้การตั้งค่ามอนสเตอร์กับฮีโร่ทุกคน", "คลิกอีกครั้งเพื่อยืนยัน: จะเขียนทับการตั้งค่ามอนสเตอร์ของฮีโร่คนอื่น",
                "คัดลอกการตั้งค่ามอนสเตอร์ไปยังชุดสกิน {0} ชุดแล้ว", "ฮีโร่คนอื่นยังไม่มีชุดสกิน"),
            ["tur"] = ("Canavar ayarlarını tüm kahramanlara uygula", "Onaylamak için tekrar tıklayın: diğer kahramanların canavar ayarlarının üzerine yazar.",
                "Canavar ayarları {0} görünüm paketine kopyalandı.", "Diğer kahramanların henüz görünüm paketi yok.")
        };

    private static (string Apply, string Confirm, string Done, string Nothing) MonsterCopy =>
        MonsterCopyTexts.TryGetValue(CurrentLanguage, out var text) ? text : MonsterCopyTexts["eng"];

    internal static string CopyMonsterSettingsToAllHeroes => MonsterCopy.Apply;
    internal static string ConfirmCopyMonsterSettingsToAllHeroes => MonsterCopy.Confirm;
    internal static string MonsterSettingsCopied(int count) => string.Format(MonsterCopy.Done, count);
    internal static string MonsterSettingsNothingToCopy => MonsterCopy.Nothing;
}
