namespace STS2SkinChanger.Core;

internal static partial class ModLocalization
{
    internal static string BlitzMerchantTitle => CurrentLanguage switch
    {
        "zhs" => "商人极速版", "zht" => "商人極速版", "jpn" => "商人（Blitz）", "kor" => "상인 (Blitz)",
        "deu" => "Händler (Blitz)", "fra" => "Marchand (Blitz)", "ita" => "Mercante (Blitz)",
        "pol" => "Kupiec (Blitz)", "ptb" => "Mercador (Blitz)", "rus" => "Торговец (Blitz)",
        "esp" or "spa" => "Mercader (Blitz)", "tha" => "พ่อค้า (Blitz)", "tur" => "Tüccar (Blitz)",
        _ => "Merchant (Blitz)"
    };
}
