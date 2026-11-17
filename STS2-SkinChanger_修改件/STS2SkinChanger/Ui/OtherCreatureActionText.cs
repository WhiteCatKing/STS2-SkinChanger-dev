using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class OtherCreatureActionText
{
    internal static readonly Dictionary<string, (string Cast, string Hurt, string Death, string Revive)> Names = new()
    {
        ["eng"] = ("Cast", "Hurt", "Death", "Revive"),
        ["zhs"] = ("施法", "受击", "倒下", "复活"), ["zht"] = ("施法", "受擊", "倒下", "復活"),
        ["jpn"] = ("詠唱", "被弾", "倒れる", "復活"), ["kor"] = ("시전", "피격", "쓰러짐", "부활"),
        ["deu"] = ("Zaubern", "Treffer", "Tod", "Wiederbeleben"),
        ["fra"] = ("Incantation", "Blessure", "Mort", "Résurrection"),
        ["ita"] = ("Magia", "Colpo subito", "Morte", "Rinascita"),
        ["pol"] = ("Czar", "Trafienie", "Śmierć", "Wskrzeszenie"),
        ["ptb"] = ("Conjurar", "Dano", "Morte", "Reviver"),
        ["rus"] = ("Заклинание", "Урон", "Смерть", "Возрождение"),
        ["esp"] = ("Conjurar", "Daño", "Muerte", "Revivir"),
        ["spa"] = ("Conjurar", "Daño", "Muerte", "Revivir"),
        ["tha"] = ("ร่ายเวท", "รับความเสียหาย", "ล้มลง", "คืนชีพ"),
        ["tur"] = ("Büyü", "Hasar", "Ölüm", "Dirilme")
    };
    internal static string Get(OtherCreatureActionKind kind)
    {
        var text = Names.GetValueOrDefault(ModLocalization.CurrentLanguage, Names["eng"]);
        return kind switch
        {
            OtherCreatureActionKind.Cast => text.Cast, OtherCreatureActionKind.Hurt => text.Hurt,
            OtherCreatureActionKind.Death => text.Death, OtherCreatureActionKind.Revive => text.Revive,
            _ => kind.ToString()
        };
    }
}
