using Godot;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

// One list and one saved selection for character select and the creature compendium.
internal static class OstySkinChoices
{
    internal static string GroupId => SkinService.CompanionGroupIds.FirstOrDefault() ?? "osty";
    internal static string Current(string groupId) => SkinService.GetCompanionRandomEnabled(groupId)
        ? RandomCharacterSkinPolicy.OptionId
        : string.IsNullOrEmpty(SkinService.Config.CompanionImpostorSkinId)
            ? SkinService.GetCompanionSelection(groupId)
            : OstyImpostorBridge.CreateOptionId(SkinService.Config.CompanionImpostorSkinId!);

    internal static void Populate(OptionButton dropdown, string groupId)
    {
        dropdown.GetPopup().Hide();
        SkinChoiceDimming.Bind(dropdown, null);
        dropdown.Clear();
        dropdown.AddItem(ModLocalization.Get(ModText.GameDefault));
        dropdown.SetItemMetadata(0, SkinCatalog.BaseOptionId);
        var group = SkinService.Catalog?.Groups.FirstOrDefault(g => g.Id == groupId);
        foreach (var option in group?.Options ?? [])
        {
            var i = dropdown.ItemCount;
            dropdown.AddItem(ModLocalization.DisplayOptionName(option.Name));
            dropdown.SetItemMetadata(i, option.Id);
        }
        foreach (var option in OstyImpostorBridge.Options)
        {
            var i = dropdown.ItemCount;
            dropdown.AddItem(option.Name);
            dropdown.SetItemMetadata(i, OstyImpostorBridge.CreateOptionId(option.Id));
        }
        if (dropdown.ItemCount > 1)
        {
            var i = dropdown.ItemCount;
            dropdown.AddItem(ModLocalization.Get(ModText.RandomCharacterSkin));
            dropdown.SetItemMetadata(i, RandomCharacterSkinPolicy.OptionId);
        }
        SkinWorkshopEntry.Append(dropdown, characterPopup: true);
        var current = Current(groupId);
        dropdown.Select(Enumerable.Range(0, dropdown.ItemCount)
            .FirstOrDefault(i => dropdown.GetItemMetadata(i).AsString().Equals(current, StringComparison.OrdinalIgnoreCase)));
    }

    internal static bool Apply(string groupId, string optionId)
    {
        if (RandomCharacterSkinPolicy.IsRandom(optionId)) return SkinService.SetCompanionRandomEnabled(groupId, true);
        SkinService.ClearCompanionRandom();
        return SkinService.ApplyCompanionSelection(groupId, optionId);
    }
}
