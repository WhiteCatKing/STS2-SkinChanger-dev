using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal static class WorkshopCodeErrorText
{
	private static readonly Dictionary<string, string[]> Packs = new Dictionary<string, string[]>
	{
		["zhs"] = "码错误|无法识别模组|码内名称：{0}|Steam 名称：{0}|组号：{0}|缺少：{0}|第 {0} 页 · 第 {1} 条|定位原帖|复制错误|没有码错误|收到 {0}/{1} 片|信息码：{0}|主楼|格式不正确|缺少模组名|模组名称不一致|分片未收齐|同组内容冲突|校验失败|不支持的码版本|信息码过大|内容无效|不属于本游戏|工坊物品不可用|旧码缺少分片完整性信息".Split('|'),
		["zht"] = "碼錯誤|無法辨識模組|碼內名稱：{0}|Steam 名稱：{0}|組號：{0}|缺少：{0}|第 {0} 頁 · 第 {1} 則|定位原文|複製錯誤|沒有碼錯誤|收到 {0}/{1} 片|資訊碼：{0}|主文|格式不正確|缺少模組名稱|模組名稱不一致|分片未收齊|同組內容衝突|校驗失敗|不支援的碼版本|資訊碼過大|內容無效|不屬於本遊戲|工坊物品無法使用|舊碼缺少分片完整性資訊".Split('|'),
		["eng"] = "Code errors|Unidentified Mod|Code name: {0}|Steam name: {0}|Group: {0}|Missing: {0}|Page {0} · Post {1}|Locate post|Copy error|No code errors|Received {0}/{1} parts|Code: {0}|Original post|Invalid format|Missing Mod name|Mod name mismatch|Missing parts|Conflicting group contents|Checksum failed|Unsupported code version|Code too large|Invalid contents|Different game|Workshop item unavailable|Legacy code lacks complete-group information".Split('|'),
		["deu"] = "Codefehler|Unbekannte Mod|Name im Code: {0}|Steam-Name: {0}|Gruppe: {0}|Fehlend: {0}|Seite {0} · Beitrag {1}|Zum Beitrag|Fehler kopieren|Keine Codefehler|{0}/{1} Teile erhalten|Code: {0}|Originalbeitrag|Ungültiges Format|Mod-Name fehlt|Mod-Namen stimmen nicht überein|Teile fehlen|Widersprüchliche Gruppeninhalte|Prüfsumme fehlerhaft|Codeversion nicht unterstützt|Code zu groß|Ungültiger Inhalt|Anderes Spiel|Workshop-Objekt nicht verfügbar|Alter Code ohne Angaben zur Gruppenvollständigkeit".Split('|'),
		["esp"] = "Errores de código|Mod sin identificar|Nombre del código: {0}|Nombre en Steam: {0}|Grupo: {0}|Faltan: {0}|Página {0} · Mensaje {1}|Ir al mensaje|Copiar error|Sin errores de código|Recibidas {0}/{1} partes|Código: {0}|Mensaje original|Formato incorrecto|Falta el nombre del mod|El nombre no coincide|Faltan partes|Contenido de grupo contradictorio|Fallo de verificación|Versión de código no admitida|Código demasiado grande|Contenido no válido|Pertenece a otro juego|Artículo no disponible|Código antiguo sin datos de integridad del grupo".Split('|'),
		["spa"] = "Errores de código|Mod sin identificar|Nombre del código: {0}|Nombre en Steam: {0}|Grupo: {0}|Faltan: {0}|Página {0} · Mensaje {1}|Ir al mensaje|Copiar error|Sin errores de código|Recibidas {0}/{1} partes|Código: {0}|Mensaje original|Formato incorrecto|Falta el nombre del mod|El nombre no coincide|Faltan partes|Contenido de grupo contradictorio|Fallo de verificación|Versión de código no admitida|Código demasiado grande|Contenido no válido|Pertenece a otro juego|Artículo no disponible|Código antiguo sin datos de integridad del grupo".Split('|'),
		["fra"] = "Erreurs de code|Mod non identifié|Nom du code : {0}|Nom Steam : {0}|Groupe : {0}|Manquants : {0}|Page {0} · Message {1}|Voir le message|Copier l’erreur|Aucune erreur de code|{0}/{1} parties reçues|Code : {0}|Message initial|Format incorrect|Nom du mod absent|Le nom du mod ne correspond pas|Parties manquantes|Contenus contradictoires|Échec de la somme de contrôle|Version de code non prise en charge|Code trop volumineux|Contenu invalide|Autre jeu|Objet indisponible|Ancien code sans données d’intégrité du groupe".Split('|'),
		["ita"] = "Errori del codice|Mod non identificata|Nome nel codice: {0}|Nome su Steam: {0}|Gruppo: {0}|Mancanti: {0}|Pagina {0} · Messaggio {1}|Apri messaggio|Copia errore|Nessun errore del codice|Ricevute {0}/{1} parti|Codice: {0}|Messaggio originale|Formato non valido|Nome mod mancante|Nome mod diverso|Parti mancanti|Contenuti del gruppo in conflitto|Verifica non riuscita|Versione codice non supportata|Codice troppo grande|Contenuto non valido|Altro gioco|Elemento non disponibile|Codice vecchio senza dati di completezza del gruppo".Split('|'),
		["jpn"] = "コードエラー|不明なMod|コード内の名前：{0}|Steam上の名前：{0}|グループ：{0}|不足：{0}|{0}ページ · 投稿{1}|元の投稿を開く|エラーをコピー|コードエラーなし|{0}/{1}個受信|コード：{0}|元の投稿|形式が不正|Mod名がありません|Mod名が一致しません|分割コードが不足|グループの内容が矛盾|チェックサム不一致|非対応のコードバージョン|コードが大きすぎます|内容が不正|別のゲームのアイテム|アイテムを利用できません|旧コードに分割グループの完全性情報がありません".Split('|'),
		["kor"] = "코드 오류|확인되지 않은 모드|코드 이름: {0}|Steam 이름: {0}|그룹: {0}|누락: {0}|{0}페이지 · 게시글 {1}|원문으로 이동|오류 복사|코드 오류 없음|{0}/{1}개 수신|코드: {0}|원글|잘못된 형식|모드 이름 누락|모드 이름 불일치|일부 조각 누락|그룹 내용 충돌|체크섬 오류|지원하지 않는 코드 버전|코드가 너무 큼|잘못된 내용|다른 게임의 항목|항목을 사용할 수 없음|이전 코드에 그룹 완전성 정보 없음".Split('|'),
		["pol"] = "Błędy kodu|Niezidentyfikowany mod|Nazwa w kodzie: {0}|Nazwa Steam: {0}|Grupa: {0}|Brakuje: {0}|Strona {0} · Wpis {1}|Otwórz wpis|Kopiuj błąd|Brak błędów kodu|Odebrano {0}/{1} części|Kod: {0}|Pierwszy wpis|Nieprawidłowy format|Brak nazwy moda|Niezgodna nazwa moda|Brak części|Sprzeczna zawartość grupy|Błąd sumy kontrolnej|Nieobsługiwana wersja kodu|Kod jest za duży|Nieprawidłowa zawartość|Inna gra|Przedmiot niedostępny|Stary kod bez danych kompletności grupy".Split('|'),
		["ptb"] = "Erros de código|Mod não identificado|Nome no código: {0}|Nome no Steam: {0}|Grupo: {0}|Faltando: {0}|Página {0} · Post {1}|Localizar post|Copiar erro|Nenhum erro de código|Recebidas {0}/{1} partes|Código: {0}|Post original|Formato inválido|Nome do mod ausente|Nome do mod diferente|Partes ausentes|Conteúdo do grupo conflitante|Falha de verificação|Versão de código incompatível|Código grande demais|Conteúdo inválido|Outro jogo|Item indisponível|Código antigo sem dados de integridade do grupo".Split('|'),
		["rus"] = "Ошибки кода|Неизвестный мод|Имя в коде: {0}|Имя в Steam: {0}|Группа: {0}|Не хватает: {0}|Страница {0} · Сообщение {1}|Открыть сообщение|Копировать ошибку|Нет ошибок кода|Получено {0}/{1} частей|Код: {0}|Первое сообщение|Неверный формат|Нет имени мода|Имя мода не совпадает|Не все части получены|Конфликт содержимого группы|Ошибка контрольной суммы|Версия кода не поддерживается|Код слишком большой|Неверное содержимое|Другая игра|Предмет недоступен|В старом коде нет данных о полноте группы".Split('|'),
		["tha"] = "รห\u0e31สผ\u0e34ดพลาด|ไม\u0e48ทราบม\u0e47อด|ช\u0e37\u0e48อในรห\u0e31ส: {0}|ช\u0e37\u0e48อ Steam: {0}|กล\u0e38\u0e48ม: {0}|ขาด: {0}|หน\u0e49า {0} · โพสต\u0e4c {1}|เป\u0e34ดโพสต\u0e4cต\u0e49นทาง|ค\u0e31ดลอกข\u0e49อผ\u0e34ดพลาด|ไม\u0e48ม\u0e35รห\u0e31สผ\u0e34ดพลาด|ได\u0e49ร\u0e31บ {0}/{1} ส\u0e48วน|รห\u0e31ส: {0}|โพสต\u0e4cต\u0e49นฉบ\u0e31บ|ร\u0e39ปแบบไม\u0e48ถ\u0e39กต\u0e49อง|ไม\u0e48ม\u0e35ช\u0e37\u0e48อม\u0e47อด|ช\u0e37\u0e48อม\u0e47อดไม\u0e48ตรงก\u0e31น|ส\u0e48วนของรห\u0e31สไม\u0e48ครบ|ข\u0e49อม\u0e39ลกล\u0e38\u0e48มข\u0e31ดแย\u0e49งก\u0e31น|การตรวจสอบล\u0e49มเหลว|ไม\u0e48รองร\u0e31บร\u0e38\u0e48นรห\u0e31ส|รห\u0e31สใหญ\u0e48เก\u0e34นไป|เน\u0e37\u0e49อหาไม\u0e48ถ\u0e39กต\u0e49อง|เป\u0e47นของเกมอ\u0e37\u0e48น|ไม\u0e48พบไอเท\u0e47มเว\u0e34ร\u0e4cกชอป|รห\u0e31สเก\u0e48าไม\u0e48ม\u0e35ข\u0e49อม\u0e39ลความครบถ\u0e49วนของกล\u0e38\u0e48ม".Split('|'),
		["tur"] = "Kod hataları|Bilinmeyen mod|Koddaki ad: {0}|Steam adı: {0}|Grup: {0}|Eksik: {0}|Sayfa {0} · Gönderi {1}|Gönderiye git|Hatayı kopyala|Kod hatası yok|{0}/{1} parça alındı|Kod: {0}|İlk gönderi|Geçersiz biçim|Mod adı eksik|Mod adı eşleşmiyor|Eksik parçalar|Grup içeriği çakışıyor|Sağlama toplamı hatası|Desteklenmeyen kod sürümü|Kod çok büyük|Geçersiz içerik|Başka oyun|Atölye öğesi kullanılamıyor|Eski kodda grup bütünlüğü bilgisi yok".Split('|')
	};

	internal static string Get(WorkshopCodeUi key, params object[] args)
	{
		return string.Format(ForLanguage(ModLocalization.CurrentLanguage, (int)key), args);
	}

	internal static string Error(WorkshopCodeError error)
	{
		return ForLanguage(ModLocalization.CurrentLanguage, (int)(13 + error));
	}

	internal static string ForLanguage(string language, int index)
	{
		return Packs.GetValueOrDefault(language, Packs["eng"])[index];
	}
}
