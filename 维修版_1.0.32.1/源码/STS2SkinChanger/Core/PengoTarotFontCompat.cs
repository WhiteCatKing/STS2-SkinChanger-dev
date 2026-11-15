using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;

namespace STS2SkinChanger.Core;

/// <summary>
/// Font compatibility for "Pengo's Tarot Enchantments Pack" (<c>PengoTarot</c>).
///
/// That pack draws its Balatro-style card inspect screens itself and hard-codes the Balatro
/// pixel font it ships (<c>kreon_bold.ttf</c>, Latin only) onto most of its labels — for example
/// the "effect / intensity / parts" headers and the card effect rows. Those labels hold localized
/// text, so in Chinese/Japanese/Korean every one of them renders as empty boxes: players describe
/// it as "the text appears broken". Only one label in each screen asks
/// <c>LocaleFontUtil.GetLocaleFont</c> for the locale font; the rest never do.
///
/// This bridge runs after the pack's own screens are ready and re-applies the game's locale font
/// to any label whose text actually contains CJK characters, leaving pure ASCII labels (numbers,
/// "0-", ranks) on the pack's own font so its look is preserved. Nothing in the other mod is
/// modified, and the bridge is skipped when that pack is not installed.
/// </summary>
internal static class PengoTarotFontCompat
{
    private static readonly string[] ScreenTypeNames =
    [
        "PengoTarot.BalatroEffect.NBalatroInspectScreen",
        "PengoTarot.BalatroEffect.NBalatroInspectEnchantScreen",
        "PengoTarot.ConfigFW.NPortableTarotPopup",
        "PengoTarot.UI.NMerchantTarot"
    ];

    private static Harmony? _harmony;
    private static readonly HashSet<string> PatchedTypes = new(StringComparer.Ordinal);
    private static bool _reported;

    public static void Install(Harmony harmony)
    {
        _harmony = harmony;
        TryInstall();
        AppDomain.CurrentDomain.AssemblyLoad += (_, _) => TryInstall();
    }

    private static void TryInstall()
    {
        if (_harmony == null)
        {
            return;
        }

        foreach (var typeName in ScreenTypeNames)
        {
            if (PatchedTypes.Contains(typeName))
            {
                continue;
            }

            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(typeName, throwOnError: false))
                .FirstOrDefault(candidate => candidate != null);
            if (type == null)
            {
                continue;
            }

            var ready = type.GetMethod(
                "_Ready",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                Type.EmptyTypes,
                null);
            if (ready == null)
            {
                PatchedTypes.Add(typeName);
                continue;
            }

            try
            {
                _harmony.Patch(
                    ready,
                    postfix: new HarmonyMethod(
                        typeof(PengoTarotFontCompat),
                        nameof(ApplyLocaleFontsPostfix)));
                PatchedTypes.Add(typeName);
                if (!_reported)
                {
                    _reported = true;
                    ModLog.Info(
                        "Pengo's Tarot Enchantments 字体兼容已启用：它的卡牌检视界面会用中/日/韩字体" +
                        "重绘含 CJK 字符的文本，数字与英文仍用原像素字体。");
                }
            }
            catch (Exception exception)
            {
                PatchedTypes.Add(typeName);
                ModLog.Warn($"Pengo's Tarot 字体兼容安装失败（{type.Name}）：" +
                            exception.GetBaseException().Message);
            }
        }
    }

    private static void ApplyLocaleFontsPostfix(Node __instance)
    {
        try
        {
            // The pack fills its labels inside _Ready and again when the popup opens, so run both
            // immediately and one frame later instead of depending on a single call order.
            ApplyToTree(__instance);
            if (Engine.GetMainLoop() is SceneTree tree && GodotObject.IsInstanceValid(__instance))
            {
                tree.CreateTimer(0.15).Timeout += () =>
                {
                    if (GodotObject.IsInstanceValid(__instance))
                    {
                        ApplyToTree(__instance);
                    }
                };
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("Pengo's Tarot 字体兼容应用失败：" + exception.GetBaseException().Message);
        }
    }

    private static void ApplyToTree(Node node)
    {
        var font = LocaleSubstituteFont();
        if (font == null)
        {
            return;
        }

        ApplyToNode(node, font);
        foreach (var child in node.GetChildren())
        {
            if (child is Node childNode)
            {
                ApplyToTree(childNode, font);
            }
        }
    }

    private static void ApplyToTree(Node node, Font font)
    {
        ApplyToNode(node, font);
        foreach (var child in node.GetChildren())
        {
            if (child is Node childNode)
            {
                ApplyToTree(childNode, font);
            }
        }
    }

    private static void ApplyToNode(Node node, Font font)
    {
        switch (node)
        {
            case Label label when ContainsCjk(label.Text):
                label.AddThemeFontOverride("font", font);
                break;
            case RichTextLabel rich when ContainsCjk(rich.Text):
                rich.AddThemeFontOverride("font", font);
                break;
            case Button button when ContainsCjk(button.Text):
                button.AddThemeFontOverride("font", font);
                break;
        }
    }

    private static Font? LocaleSubstituteFont()
    {
        try
        {
            var manager = LocManager.Instance;
            if (manager == null)
            {
                return null;
            }

            var language = manager.Language;
            if (string.IsNullOrWhiteSpace(language) || !FontManager.NeedsFontSubstitution(language))
            {
                return null;
            }

            return FontManager.GetSubstituteFont(language, FontType.Bold);
        }
        catch
        {
            return null;
        }
    }

    private static bool ContainsCjk(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (var character in text)
        {
            if (character is >= '\u2e80' and <= '\u9fff' or
                >= '\uac00' and <= '\ud7af' or
                >= '\uf900' and <= '\ufaff' or
                >= '\uff00' and <= '\uffef')
            {
                return true;
            }
        }

        return false;
    }
}
