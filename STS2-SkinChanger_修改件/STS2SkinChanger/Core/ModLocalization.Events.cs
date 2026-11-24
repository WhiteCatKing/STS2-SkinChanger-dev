namespace STS2SkinChanger.Core;

internal static partial class ModLocalization
{
    private static readonly IReadOnlyDictionary<string, string> EventCategoryTexts = new Dictionary<string, string>
    {
        ["eng"] = "Events", ["zhs"] = "事件", ["zht"] = "事件", ["deu"] = "Ereignisse",
        ["esp"] = "Eventos", ["spa"] = "Eventos", ["fra"] = "Événements", ["ita"] = "Eventi",
        ["jpn"] = "イベント", ["kor"] = "이벤트", ["pol"] = "Wydarzenia", ["ptb"] = "Eventos",
        ["rus"] = "События", ["tha"] = "เหตุการณ์", ["tur"] = "Olaylar"
    };

    private static readonly IReadOnlyDictionary<string, string> EventTextOnlySuffixTexts =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["eng"] = " (text only)",
            ["zhs"] = "（仅文本）",
            ["zht"] = "（僅文字）",
            ["deu"] = " (nur Text)",
            ["esp"] = " (solo texto)",
            ["spa"] = " (solo texto)",
            ["fra"] = " (texte uniquement)",
            ["ita"] = " (solo testo)",
            ["jpn"] = "（テキストのみ）",
            ["kor"] = " (텍스트만)",
            ["pol"] = " (tylko tekst)",
            ["ptb"] = " (apenas texto)",
            ["rus"] = " (только текст)",
            ["tha"] = " (ข้อความเท่านั้น)",
            ["tur"] = " (yalnızca metin)"
        };

    public static string FormatEventOptionLabel(string name, bool isTextOnly)
    {
        var display = DisplayOptionName(name);
        return isTextOnly ? display + Get(ModText.EventTextOnlySuffix) : display;
    }
}
