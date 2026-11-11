namespace STS2SkinChanger.Ui.LoadOrderWarning;

// F35：自动排序不能反复撤销玩家的手动排序。第一次自动排序把“本次被移动的提供者”记入配置，
// 作为下一轮判断的种子；之后玩家再把它移到本 Mod 之上，就会被识别为刻意排序并尊重。只有
// 从未记录过的提供者（新安装）才需要再次自动排序并提醒重启。
internal static class LoadOrderRespectPolicy
{
    internal static (IReadOnlyList<string> Deliberate, IReadOnlyList<string> Actionable) Resolve(
        IEnumerable<string> providersAboveSelf,
        IEnumerable<string> rememberedProvidersAboveSelf)
    {
        var remembered = new HashSet<string>(rememberedProvidersAboveSelf, StringComparer.OrdinalIgnoreCase);
        var deliberate = new List<string>();
        var actionable = new List<string>();
        foreach (var provider in providersAboveSelf)
        {
            if (remembered.Remove(provider))
            {
                deliberate.Add(provider);
            }
            else
            {
                actionable.Add(provider);
            }
        }

        return (deliberate, actionable);
    }

    // The previous bug was storing only `deliberate`: on the first run it is always empty, so the
    // list could never learn the providers that were just moved and every later manual move was
    // treated as a new provider again. Keep everything already remembered plus the providers
    // moved this run; a later appearance above SkinChanger then counts as deliberate.
    internal static IReadOnlyList<string> Remember(
        IEnumerable<string> rememberedProvidersAboveSelf,
        IEnumerable<string> movedProviders)
    {
        return rememberedProvidersAboveSelf
            .Concat(movedProviders)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
