namespace STS2SkinChanger.Core;

internal static partial class ModLocalization
{
    private sealed record CacheWriteTexts(string Budget, string Disk, string Blocked);
    private static readonly IReadOnlyDictionary<string, CacheWriteTexts> CacheWriteTranslations =
        new Dictionary<string, CacheWriteTexts>(StringComparer.OrdinalIgnoreCase)
        {
            ["eng"] = new("Skin cache limit reached (16 GiB). Your current appearance is kept. Restart the game to free the cache.",
                "Not enough disk space for this skin. Your current appearance is kept. Free disk space and try again.",
                "The skin cache cannot be updated safely. Your current appearance is kept. Restart the game and try again."),
            ["zhs"] = new("皮肤缓存已达上限（16 GiB）。已保留当前外观，请重启游戏释放缓存后再试。",
                "磁盘空间不足，无法写入皮肤。已保留当前外观，请清理磁盘后重试。",
                "皮肤缓存暂时无法安全更新。已保留当前外观，请重启游戏后重试。"),
            ["zht"] = new("皮膚快取已達上限（16 GiB）。已保留目前外觀，請重新啟動遊戲釋放快取後再試。",
                "磁碟空間不足，無法寫入皮膚。已保留目前外觀，請清理磁碟後重試。",
                "皮膚快取暫時無法安全更新。已保留目前外觀，請重新啟動遊戲後重試。"),
            ["deu"] = new("Skin-Cache-Limit erreicht (16 GiB). Dein aktuelles Aussehen bleibt erhalten. Starte das Spiel neu, um den Cache freizugeben.",
                "Nicht genug Speicherplatz für diesen Skin. Dein aktuelles Aussehen bleibt erhalten. Gib Speicherplatz frei und versuche es erneut.",
                "Der Skin-Cache kann nicht sicher aktualisiert werden. Dein aktuelles Aussehen bleibt erhalten. Starte das Spiel neu."),
            ["esp"] = new("La caché de aspectos ha alcanzado el límite (16 GiB). Se conserva tu apariencia actual. Reinicia el juego para liberar la caché.",
                "No hay espacio en disco para este aspecto. Se conserva tu apariencia actual. Libera espacio y vuelve a intentarlo.",
                "No se puede actualizar la caché de aspectos con seguridad. Se conserva tu apariencia actual. Reinicia el juego."),
            ["spa"] = new("La caché de aspectos llegó al límite (16 GiB). Se conserva tu apariencia actual. Reinicia el juego para liberar la caché.",
                "No hay espacio en disco para este aspecto. Se conserva tu apariencia actual. Libera espacio e inténtalo de nuevo.",
                "No se puede actualizar la caché de aspectos de forma segura. Se conserva tu apariencia actual. Reinicia el juego."),
            ["fra"] = new("La limite du cache d'apparences est atteinte (16 Gio). Votre apparence actuelle est conservée. Redémarrez le jeu pour libérer le cache.",
                "Espace disque insuffisant pour cette apparence. Votre apparence actuelle est conservée. Libérez de l'espace puis réessayez.",
                "Le cache d'apparences ne peut pas être mis à jour sans risque. Votre apparence actuelle est conservée. Redémarrez le jeu."),
            ["ita"] = new("Limite della cache degli aspetti raggiunto (16 GiB). L'aspetto attuale viene mantenuto. Riavvia il gioco per liberare la cache.",
                "Spazio su disco insufficiente per questo aspetto. L'aspetto attuale viene mantenuto. Libera spazio e riprova.",
                "Impossibile aggiornare la cache degli aspetti in sicurezza. L'aspetto attuale viene mantenuto. Riavvia il gioco."),
            ["jpn"] = new("スキンキャッシュが上限（16 GiB）に達しました。現在の外見を維持します。ゲームを再起動してキャッシュを解放してください。",
                "スキンを書き込むディスク容量が不足しています。現在の外見を維持します。空き容量を増やして再試行してください。",
                "スキンキャッシュを安全に更新できません。現在の外見を維持します。ゲームを再起動して再試行してください。"),
            ["kor"] = new("스킨 캐시가 한도(16 GiB)에 도달했습니다. 현재 외형을 유지합니다. 게임을 재시작하여 캐시를 비워 주세요.",
                "스킨을 저장할 디스크 공간이 부족합니다. 현재 외형을 유지합니다. 공간을 확보한 뒤 다시 시도해 주세요.",
                "스킨 캐시를 안전하게 갱신할 수 없습니다. 현재 외형을 유지합니다. 게임을 재시작해 주세요."),
            ["pol"] = new("Osiągnięto limit pamięci podręcznej skórek (16 GiB). Obecny wygląd zostaje zachowany. Uruchom grę ponownie, aby zwolnić pamięć podręczną.",
                "Za mało miejsca na dysku dla tej skórki. Obecny wygląd zostaje zachowany. Zwolnij miejsce i spróbuj ponownie.",
                "Nie można bezpiecznie zaktualizować pamięci podręcznej skórek. Obecny wygląd zostaje zachowany. Uruchom grę ponownie."),
            ["ptb"] = new("O cache de visuais atingiu o limite (16 GiB). A aparência atual foi mantida. Reinicie o jogo para liberar o cache.",
                "Espaço em disco insuficiente para este visual. A aparência atual foi mantida. Libere espaço e tente novamente.",
                "Não foi possível atualizar o cache de visuais com segurança. A aparência atual foi mantida. Reinicie o jogo."),
            ["rus"] = new("Достигнут лимит кэша обликов (16 ГиБ). Текущий облик сохранён. Перезапустите игру, чтобы освободить кэш.",
                "Недостаточно места на диске для этого облика. Текущий облик сохранён. Освободите место и повторите попытку.",
                "Нельзя безопасно обновить кэш обликов. Текущий облик сохранён. Перезапустите игру."),
            ["tha"] = new("แคชสกินถึงขีดจำกัดแล้ว (16 GiB) รูปลักษณ์ปัจจุบันยังคงอยู่ โปรดเริ่มเกมใหม่เพื่อคืนพื้นที่แคช",
                "พื้นที่ดิสก์ไม่พอสำหรับสกินนี้ รูปลักษณ์ปัจจุบันยังคงอยู่ โปรดเพิ่มพื้นที่ว่างแล้วลองอีกครั้ง",
                "ไม่สามารถอัปเดตแคชสกินได้อย่างปลอดภัย รูปลักษณ์ปัจจุบันยังคงอยู่ โปรดเริ่มเกมใหม่"),
            ["tur"] = new("Görünüm önbelleği sınırına ulaşıldı (16 GiB). Mevcut görünümünüz korundu. Önbelleği boşaltmak için oyunu yeniden başlatın.",
                "Bu görünüm için diskte yeterli alan yok. Mevcut görünümünüz korundu. Yer açıp yeniden deneyin.",
                "Görünüm önbelleği güvenle güncellenemiyor. Mevcut görünümünüz korundu. Oyunu yeniden başlatın.")
        };

    internal static string CacheWriteMessage(ModText text, string language)
    {
        var pack = CacheWriteTranslations.GetValueOrDefault(language, CacheWriteTranslations["eng"]);
        return text switch
        {
            ModText.CacheDiskSpaceRejected => pack.Disk,
            ModText.CacheWriteBlocked => pack.Blocked,
            _ => pack.Budget
        };
    }
}
