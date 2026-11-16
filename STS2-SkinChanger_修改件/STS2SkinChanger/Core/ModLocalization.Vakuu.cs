namespace STS2SkinChanger.Core;

internal static partial class ModLocalization
{
    internal static string GameDefaultEasterEgg() => Get(ModText.GameDefault) + (CurrentLanguage switch
    {
        "zhs" => "彩蛋", "zht" => "彩蛋", "jpn" => "（隠し絵）", "kor" => " (이스터 에그)",
        "deu" => " (Easter Egg)", "fra" => " (clin d’œil)", "ita" => " (easter egg)",
        "esp" or "spa" => " (huevo de Pascua)", "pol" => " (easter egg)", "ptb" => " (easter egg)",
        "rus" => " (пасхалка)", "tha" => " (อีสเตอร์เอ็ก)", "tur" => " (sürpriz)", _ => " (Easter egg)"
    });
}
