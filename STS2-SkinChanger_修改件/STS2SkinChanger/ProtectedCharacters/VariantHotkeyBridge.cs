// 受保护兼容模块：十字军/贞德及同包变体。修改前阅读本目录 AGENTS.md 和 README.md。
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

/// <summary>
/// Keeps the Skin Changer's dropdown in sync with a skin pack's <b>own</b> variant switching.
///
/// Packs like JoanofArcIroncladHpwear expose their palette as a setting plus a hotkey
/// (<c>AppearanceHotkey</c> → <c>PersistAppearance</c> → <c>Palette.RefreshAppearance</c>). When the
/// player presses that hotkey the pack changes its value directly, so the dropdown kept showing the
/// variant that was selected before. This bridge reads the pack's value back and adopts the matching
/// dropdown entry, which is the only direction the Skin Changer cannot observe on its own.
///
/// Resolution rules (the author's requirement):
/// <list type="bullet">
/// <item>with a target selected in the Appearance screen, only that target's variant changes;</item>
/// <item>without a target, only the local player's variant changes;</item>
/// <item>the pack's value always wins, so the dropdown can never disagree with what is drawn.</item>
/// </list>
///
/// The read-back is <b>per package</b>: two packs may ship the same assembly identity and the same
/// settings type (Joan 0.3.5-S next to 0.3.6), and a value read from one of them must only ever be
/// adopted by that pack's own variant family. Grouping by assembly simple name used to merge the two
/// families, so pressing the hotkey on one pack re-pointed the other pack's dropdown entry.
///
/// Everything here is contract based (the same metadata scan that creates the variant options), so a
/// new pack with settings-driven variants is handled without touching this file.
/// </summary>
internal static class VariantHotkeyBridge
{
    private const double PollInterval = 0.25;

    /// <summary>
    /// 战士（Joan 系）变体热键桥。作者要求先停用所有战士专属逻辑，代码保留待恢复。
    /// 刻意用 static readonly 而不是 const：运行期同样恒为 false，但编译器不会把保留下来的
    /// 分支判成不可达代码（CS0162），恢复时只改这一行的值即可。
    /// </summary>
    private static readonly bool IroncladCompatEnabled = false;

    private static double _elapsed;
    private static readonly Dictionary<string, int> AdoptedLogKeys = new(StringComparer.OrdinalIgnoreCase);

    internal static void Tick(double delta)
    {
        if (!IroncladCompatEnabled)
        {
            return;
        }

        _elapsed += delta;
        if (_elapsed < PollInterval)
        {
            return;
        }

        _elapsed = 0;

        var slots = ProviderSettingsVariantOptions.Slots();
        if (slots.Count == 0)
        {
            return;
        }

        foreach (var contractSlots in slots.GroupBy(
                     slot => ProviderSettingsVariantOptions.ContractKey(slot.Contract),
                     StringComparer.OrdinalIgnoreCase))
        {
            var contract = contractSlots.First().Contract;
            if (!ProviderSettingsVariantOptions.IsPackageLoaded(contract))
            {
                // The pack that lost the assembly identity cannot change its own value, so there is
                // nothing to read back. Its variants are cycled by the Skin Changer instead.
                continue;
            }

            var observed = ProviderSettingsVariantOptions.ReadCurrentIndex(contract);
            if (observed is not { } index)
            {
                continue;
            }

            var current = FindGroupUsingAny(contractSlots);
            if (current is not { } pair)
            {
                continue;
            }

            var selectedSlot = contractSlots.FirstOrDefault(slot =>
                slot.OptionId.Equals(pair.Value, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(selectedSlot.OptionId) || selectedSlot.Index == index)
            {
                continue;
            }

            var target = contractSlots.FirstOrDefault(slot => slot.Index == index);
            if (string.IsNullOrEmpty(target.OptionId))
            {
                continue;
            }

            Adopt(pair.Key, target.OptionId);
        }
    }

    private static KeyValuePair<string, string>? FindGroupUsingAny(IEnumerable<ProviderSettingsVariantOptions.VariantSlot> slots)
    {
        var optionIds = slots
            .Select(slot => slot.OptionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in SkinService.Config.Selections)
        {
            if (optionIds.Contains(pair.Value))
            {
                return pair;
            }
        }

        return null;
    }

    private static void Adopt(string groupId, string optionId)
    {
        try
        {
            var targetCreature = CharacterAppearanceScreen.HotkeyTargetCreature;
            var targetGroup = CharacterAppearanceScreen.HotkeyTargetGroupId;
            var targetIsTeammate = targetCreature != null &&
                                   groupId.Equals(targetGroup, StringComparison.OrdinalIgnoreCase) &&
                                   MultiplayerSkinSync.UsesLocalFallbackControls(targetCreature);
            if (targetIsTeammate)
            {
                if (!MultiplayerSkinSync.TrySetLocalFallbackSkin(
                        targetCreature!,
                        groupId,
                        optionId,
                        out var error))
                {
                    ModLog.Warn($"快捷键切换变体后同步到队友失败：{error}");
                    return;
                }
            }
            else
            {
                CharacterAppearanceRuntime.RequestSelection(groupId, optionId);
            }

            // The value the pack now holds and the option we just adopted must also be what both
            // dropdowns show; otherwise the panel keeps displaying the previous entry even though the
            // model already changed.
            ContextualSkinControls.RefreshCharacterSelectDropdown(groupId);
            CharacterAppearanceScreen.RefreshLiveSkinDropdowns(groupId);

            var key = groupId + "\n" + optionId;
            if (AdoptedLogKeys.TryAdd(key, 1))
            {
                ModLog.Info(
                    $"检测到皮肤包自己的变体切换（{groupId}），皮肤切换器选项已同步为：{optionId}" +
                    (targetIsTeammate ? "（仅该队友）。" : "（本机）。"));
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"同步皮肤包变体到皮肤切换器选项失败：{exception.GetBaseException().Message}");
        }
    }

    private static string ContractKey(ProviderSettingsVariantOptions.SettingsVariantContract contract) =>
        ProviderSettingsVariantOptions.ContractKey(contract);
}
