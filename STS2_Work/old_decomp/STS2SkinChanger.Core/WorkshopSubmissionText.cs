using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal static class WorkshopSubmissionText
{
	private static readonly Dictionary<string, string[]> Packs = new Dictionary<string, string[]>
	{
		["zhs"] = "刷新|刷新中…|刷新失败，已保留缓存|已刷新|选择已订阅的皮肤模组|扫描并生成|读取已下载模组…|扫描 {0}/{1}：{2}|已识别 {0} · 未识别 {1} · 投稿码 {2}|没有已下载的皮肤候选|全选|清空选择|复制|已复制✓|打开投稿区|返回选择|搜索模组".Split('|'),
		["zht"] = "重新整理|更新中…|更新失敗，已保留快取|已更新|選擇已訂閱的皮膚模組|掃描並產生|讀取已下載模組…|掃描 {0}/{1}：{2}|已辨識 {0} · 未辨識 {1} · 投稿碼 {2}|沒有已下載的皮膚候選|全選|清空選擇|複製|已複製✓|開啟投稿區|返回選擇|搜尋模組".Split('|'),
		["eng"] = "Refresh|Refreshing…|Refresh failed; cache kept|Refreshed|Select subscribed skin Mods|Scan and generate|Reading installed Mods…|Scanning {0}/{1}: {2}|Recognized {0} · Skipped {1} · Codes {2}|No downloaded skin candidates|Select all|Clear selection|Copy|Copied✓|Open submissions|Back to selection|Search Mods".Split('|'),
		["deu"] = "Aktualisieren|Aktualisierung…|Fehlgeschlagen; Cache behalten|Aktualisiert|Abonnierte Skin-Mods auswählen|Scannen und erzeugen|Installierte Mods lesen…|Scan {0}/{1}: {2}|Erkannt {0} · Übersprungen {1} · Codes {2}|Keine heruntergeladenen Skin-Kandidaten|Alle auswählen|Auswahl leeren|Kopieren|Kopiert✓|Beiträge öffnen|Zur Auswahl|Mods suchen".Split('|'),
		["esp"] = "Actualizar|Actualizando…|Falló; caché conservada|Actualizado|Seleccionar mods de aspectos suscritos|Analizar y generar|Leyendo mods instalados…|Analizando {0}/{1}: {2}|Reconocidos {0} · Omitidos {1} · Códigos {2}|No hay aspectos descargados|Seleccionar todos|Borrar selección|Copiar|Copiado✓|Abrir propuestas|Volver a selección|Buscar mods".Split('|'),
		["spa"] = "Actualizar|Actualizando…|Falló; caché conservada|Actualizado|Seleccionar mods de aspectos suscritos|Analizar y generar|Leyendo mods instalados…|Analizando {0}/{1}: {2}|Reconocidos {0} · Omitidos {1} · Códigos {2}|No hay aspectos descargados|Seleccionar todos|Borrar selección|Copiar|Copiado✓|Abrir propuestas|Volver a selección|Buscar mods".Split('|'),
		["fra"] = "Actualiser|Actualisation…|Échec ; cache conservé|Actualisé|Choisir les mods d’apparence abonnés|Analyser et générer|Lecture des mods installés…|Analyse {0}/{1} : {2}|Reconnus {0} · Ignorés {1} · Codes {2}|Aucune apparence téléchargée|Tout sélectionner|Effacer la sélection|Copier|Copié✓|Ouvrir les propositions|Retour à la sélection|Rechercher des mods".Split('|'),
		["ita"] = "Aggiorna|Aggiornamento…|Non riuscito; cache mantenuta|Aggiornato|Scegli le mod estetiche sottoscritte|Analizza e genera|Lettura mod installate…|Analisi {0}/{1}: {2}|Riconosciute {0} · Ignorate {1} · Codici {2}|Nessuna skin scaricata|Seleziona tutto|Svuota selezione|Copia|Copiato✓|Apri proposte|Torna alla selezione|Cerca mod".Split('|'),
		["jpn"] = "更新|更新中…|更新失敗・キャッシュ保持|更新済み|登録済みスキンModを選択|解析して生成|導入済みModを読み込み中…|解析 {0}/{1}：{2}|認識 {0} · スキップ {1} · コード {2}|ダウンロード済みの候補がありません|すべて選択|選択を解除|コピー|コピー済み✓|投稿先を開く|選択に戻る|Modを検索".Split('|'),
		["kor"] = "새로고침|새로고침 중…|실패; 캐시 유지|새로고침 완료|구독한 스킨 모드 선택|분석 및 생성|설치된 모드 읽는 중…|분석 {0}/{1}: {2}|인식 {0} · 건너뜀 {1} · 코드 {2}|다운로드한 스킨 후보 없음|모두 선택|선택 해제|복사|복사됨✓|제안 게시판 열기|선택으로 돌아가기|모드 검색".Split('|'),
		["pol"] = "Odśwież|Odświeżanie…|Błąd; zachowano pamięć podręczną|Odświeżono|Wybierz subskrybowane skórki|Skanuj i generuj|Odczytywanie modów…|Skan {0}/{1}: {2}|Rozpoznane {0} · Pominięte {1} · Kody {2}|Brak pobranych skórek|Wybierz wszystkie|Wyczyść wybór|Kopiuj|Skopiowano✓|Otwórz zgłoszenia|Wróć do wyboru|Szukaj modów".Split('|'),
		["ptb"] = "Atualizar|Atualizando…|Falha; cache mantido|Atualizado|Selecionar mods de skins inscritos|Analisar e gerar|Lendo mods instalados…|Analisando {0}/{1}: {2}|Reconhecidos {0} · Ignorados {1} · Códigos {2}|Nenhuma skin baixada|Selecionar todos|Limpar seleção|Copiar|Copiado✓|Abrir sugestões|Voltar à seleção|Buscar mods".Split('|'),
		["rus"] = "Обновить|Обновление…|Ошибка; кэш сохранён|Обновлено|Выбрать подписанные моды обликов|Сканировать и создать|Чтение установленных модов…|Сканирование {0}/{1}: {2}|Найдено {0} · Пропущено {1} · Кодов {2}|Нет скачанных обликов|Выбрать все|Снять выбор|Копировать|Скопировано✓|Открыть предложения|Назад к выбору|Поиск модов".Split('|'),
		["tha"] = "ร\u0e35เฟรช|กำล\u0e31งร\u0e35เฟรช…|ร\u0e35เฟรชไม\u0e48สำเร\u0e47จ เก\u0e47บแคชไว\u0e49|ร\u0e35เฟรชแล\u0e49ว|เล\u0e37อกม\u0e47อดสก\u0e34นท\u0e35\u0e48สม\u0e31ครไว\u0e49|สแกนและสร\u0e49าง|กำล\u0e31งอ\u0e48านม\u0e47อดท\u0e35\u0e48ต\u0e34ดต\u0e31\u0e49ง…|สแกน {0}/{1}: {2}|พบ {0} · ข\u0e49าม {1} · รห\u0e31ส {2}|ไม\u0e48ม\u0e35สก\u0e34นท\u0e35\u0e48ดาวน\u0e4cโหลดไว\u0e49|เล\u0e37อกท\u0e31\u0e49งหมด|ล\u0e49างการเล\u0e37อก|ค\u0e31ดลอก|ค\u0e31ดลอกแล\u0e49ว✓|เป\u0e34ดกระท\u0e39\u0e49เสนอ|กล\u0e31บไปเล\u0e37อก|ค\u0e49นหาม\u0e47อด".Split('|'),
		["tur"] = "Yenile|Yenileniyor…|Başarısız; önbellek korundu|Yenilendi|Abone olunan görünüm modlarını seç|Tara ve oluştur|Kurulu modlar okunuyor…|Taranıyor {0}/{1}: {2}|Tanınan {0} · Atlanan {1} · Kod {2}|İndirilmiş görünüm yok|Tümünü seç|Seçimi temizle|Kopyala|Kopyalandı✓|Önerileri aç|Seçime dön|Mod ara".Split('|')
	};

	public static string Get(SubmissionText key, params object[] args)
	{
		return string.Format(ForLanguage(ModLocalization.CurrentLanguage, key), args);
	}

	public static string ForLanguage(string language, SubmissionText key)
	{
		return Packs.GetValueOrDefault(language, Packs["eng"])[(int)key];
	}
}
