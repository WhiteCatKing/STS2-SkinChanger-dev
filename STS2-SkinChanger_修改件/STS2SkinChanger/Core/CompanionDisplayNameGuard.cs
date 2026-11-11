using System;
using System.Text;
using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace STS2SkinChanger.Core;

/// <summary>
/// 「奥斯提」的名字安全阀。
/// <para>
/// 工坊 Mod 「Midori Black - Character Name Localization」（3747561432 目录下的独立
/// 子 Mod `midori_black_localization`）会把奥斯提无差别改名成「坚果」。实机验证发现它改的不只是
/// 格式化结果：连 <c>GetRawText</c> 都已经是「坚果」，说明它在加载本地化表时就替换了原文，
/// 所以不能靠「读原表」还原，只能回到游戏包里的原始 JSON 取值。
/// </para>
/// <para>
/// 这里只做一件事：当**奥斯提分组当前选中的提供者不是 midori** 时，把名字还原成游戏原包里的值；
/// 选中的是 midori（猫）时保留第三方 Mod 的改名，让「猫 = 坚果」成立。
/// </para>
/// </summary>
internal static class CompanionDisplayNameGuard
{
    /// <summary>语言表 `monsters.json` 里奥斯提的名字键。</summary>
    private const string OstyNameKey = "OSTY.name";

    /// <summary>米德莉（猫）皮肤提供者的判定片段。</summary>
    private const string MidoriProviderFragment = "midori";

    /// <summary>分组 ID，与 <c>SkinCatalog</c> 里的随从分组一致。</summary>
    private const string OstyGroupId = "osty";

    private static string? _vanillaOstyName;
    private static bool _vanillaOstyNameResolved;

    public static void Restore(ref string result, LocString? locString)
    {
        if (string.IsNullOrEmpty(result) || locString == null)
        {
            return;
        }

        if (!string.Equals(locString.LocEntryKey, OstyNameKey, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (OstyUsesMidoriProvider())
        {
            return;
        }

        var original = GetVanillaOstyName();
        if (!string.IsNullOrEmpty(original) &&
            !string.Equals(original, result, StringComparison.Ordinal))
        {
            result = original;
        }
    }

    /// <summary>
    /// 从游戏原包的 <c>res://localization/&lt;lang&gt;/monsters.json</c> 里读出奥斯提的原始名字。
    /// 结果按语言缓存一次；读不到就返回 null，宁可显示第三方的名字也不写死一个中文串。
    /// </summary>
    private static string? GetVanillaOstyName()
    {
        if (_vanillaOstyNameResolved)
        {
            return _vanillaOstyName;
        }

        _vanillaOstyNameResolved = true;
        try
        {
            var language = LocManager.Instance?.Language;
            foreach (var candidate in new[] { language, "eng" })
            {
                if (string.IsNullOrEmpty(candidate))
                {
                    continue;
                }

                var path = $"res://localization/{candidate}/monsters.json";
                if (!SkinService.TryReadBaseGameResource(path, out var bytes) || bytes.Length == 0)
                {
                    continue;
                }

                var json = Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF');
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind == JsonValueKind.Object &&
                    document.RootElement.TryGetProperty(OstyNameKey, out var value) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    _vanillaOstyName = value.GetString();
                    if (!string.IsNullOrEmpty(_vanillaOstyName))
                    {
                        ModLog.Info($"[名字] 已从游戏原包读取奥斯提原名：{_vanillaOstyName}（{path}）");
                        return _vanillaOstyName;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[名字] 读取奥斯提原名失败：{exception.GetBaseException().Message}");
        }

        return null;
    }

    private static bool OstyUsesMidoriProvider()
    {
        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            // 目录还没建好（启动早期）：没有选择信息时按「不是猫」处理，
            // 这样宁可显示原版名字，也不会让猫的名字泄漏到别的皮肤上。
            return false;
        }

        var selection = SkinService.Config.GetSelection(OstyGroupId);
        var providerId = catalog.ResolveVisualProviderId(selection);
        return providerId.Contains(MidoriProviderFragment, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// 在第三方改名补丁之后执行（<see cref="Priority.Last"/> + 显式排在 midori 的本地化补丁之后），
/// 把不该生效的改名还原。
/// </summary>
[HarmonyPatch(typeof(LocString), nameof(LocString.GetFormattedText))]
[HarmonyAfter("midori_black_localization")]
internal static class OstyDisplayNameGuardPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(LocString __instance, ref string __result)
    {
        CompanionDisplayNameGuard.Restore(ref __result, __instance);
    }
}
