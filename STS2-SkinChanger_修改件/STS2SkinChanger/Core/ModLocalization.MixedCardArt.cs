using System.Globalization;

namespace STS2SkinChanger.Core;

internal static partial class ModLocalization
{
    private sealed record MixedCardArtTexts(string Title, string Body);

    private static readonly IReadOnlyDictionary<string, MixedCardArtTexts> MixedCardArtDirectoryTranslations =
        new Dictionary<string, MixedCardArtTexts>(StringComparer.OrdinalIgnoreCase)
        {
            ["eng"] = new(
                "Mixed Mod folder detected",
                "\"{0}\" contains both enabled and disabled Mods. Card art in this folder cannot be separated by Mod, so disabling one will not disable its card art. Please move each Mod into its own folder."),
            ["zhs"] = new(
                "检测到混放的 Mod 文件夹",
                "「{0}」里同时放着已启用和已禁用的 Mod，其中的卡图无法按 Mod 区分，禁用不会让这些卡图失效。请把这些 Mod 分别放进各自的文件夹。"),
            ["zht"] = new(
                "偵測到混放的 Mod 資料夾",
                "「{0}」裡同時放著已啟用和已停用的 Mod，其中的卡圖無法按 Mod 區分，停用不會讓這些卡圖失效。請把這些 Mod 分別放進各自的資料夾。"),
            ["deu"] = new(
                "Gemischter Mod-Ordner erkannt",
                "„{0}“ enthält sowohl aktivierte als auch deaktivierte Mods. Kartenbilder in diesem Ordner lassen sich nicht nach Mod trennen, sodass das Deaktivieren diese Kartenbilder nicht abschaltet. Bitte lege jede Mod in einen eigenen Ordner."),
            ["esp"] = new(
                "Carpeta de mods mezclados detectada",
                "«{0}» contiene mods activados y desactivados a la vez. Las ilustraciones de cartas de esta carpeta no se pueden separar por mod, por lo que desactivarlos no deshabilitará esas cartas. Coloca cada mod en su propia carpeta."),
            ["spa"] = new(
                "Carpeta de mods mezclados detectada",
                "\"{0}\" contiene mods activados y desactivados a la vez. Las ilustraciones de cartas de esta carpeta no se pueden separar por mod, así que desactivarlos no hará que dejen de aplicarse. Coloca cada mod en su propia carpeta."),
            ["fra"] = new(
                "Dossier de mods mélangés détecté",
                "« {0} » contient à la fois des mods activés et désactivés. Les illustrations de cartes de ce dossier ne peuvent pas être séparées par mod, donc la désactivation ne les désactivera pas. Veuillez placer chaque mod dans son propre dossier."),
            ["ita"] = new(
                "Rilevata cartella con mod miste",
                "«{0}» contiene sia mod attivate che disattivate. Le illustrazioni delle carte in questa cartella non possono essere separate per mod, quindi disattivarle non disabiliterà queste carte. Sposta ogni mod nella propria cartella."),
            ["jpn"] = new(
                "Mod が混在するフォルダを検出しました",
                "「{0}」に有効な Mod と無効な Mod が混在しています。このフォルダ内のカードアートは Mod ごとに区別できないため、無効化しても反映が解除されません。各 Mod を別々のフォルダに分けてください。"),
            ["kor"] = new(
                "모드가 섞인 폴더 감지됨",
                "'{0}' 폴더에 활성화된 모드와 비활성화된 모드가 함께 들어 있습니다. 이 폴더의 카드 아트는 모드별로 구분할 수 없어 비활성화해도 카드 아트가 해제되지 않습니다. 각 모드를 별도의 폴더로 분리해 주세요."),
            ["pol"] = new(
                "Wykryto wspólny folder modów",
                "Folder „{0}” zawiera jednocześnie włączone i wyłączone mody. Grafik kart z tego folderu nie można rozdzielić według moda, więc wyłączenie go nie wyłączy tych grafik. Przenieś każdy mod do osobnego folderu."),
            ["ptb"] = new(
                "Pasta com mods misturados detectada",
                "\"{0}\" contém mods ativados e desativados juntos. As artes de cartas nesta pasta não podem ser separadas por mod, então desativá-los não desabilitará essas artes. Coloque cada mod em sua própria pasta."),
            ["rus"] = new(
                "Обнаружена общая папка с модами",
                "В папке «{0}» одновременно находятся включённые и отключённые моды. Арты карт в этой папке нельзя разделить по модам, поэтому отключение мода не отключит эти арты. Разместите каждый мод в отдельной папке."),
            ["tha"] = new(
                "ตรวจพบโฟลเดอร์ที่มี Mod ปะปนกัน",
                "\"{0}\" มีทั้ง Mod ที่เปิดใช้งานและปิดใช้งานอยู่รวมกัน ไม่สามารถแยกภาพการ์ดในโฟลเดอร์นี้ตาม Mod ได้ การปิดใช้งานจึงไม่ทำให้ภาพการ์ดเหล่านี้หายไป โปรดแยกแต่ละ Mod ไปไว้ในโฟลเดอร์ของตัวเอง"),
            ["tur"] = new(
                "Karışık Mod klasörü algılandı",
                "\"{0}\" klasöründe hem etkin hem de devre dışı modlar birlikte bulunuyor. Bu klasördeki kart görselleri moda göre ayrılamadığı için devre dışı bırakmak bu görselleri kaldırmaz. Lütfen her modu kendi klasörüne taşıyın.")
        };

    internal static string MixedCardArtDirectoryTitle(string language) =>
        MixedCardArtDirectoryTranslations.GetValueOrDefault(
            language,
            MixedCardArtDirectoryTranslations["eng"]).Title;

    internal static string MixedCardArtDirectoryTemplate(string language) =>
        MixedCardArtDirectoryTranslations.GetValueOrDefault(
            language,
            MixedCardArtDirectoryTranslations["eng"]).Body;

    internal static string MixedCardArtDirectoryMessage(string folderNames, string language) =>
        string.Format(CultureInfo.InvariantCulture, MixedCardArtDirectoryTemplate(language), folderNames);
}
