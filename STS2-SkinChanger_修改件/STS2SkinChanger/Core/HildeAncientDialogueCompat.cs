using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(AncientEventModel), nameof(AncientEventModel.DialogueSet), MethodType.Getter)]
internal static class HildeAncientDialogueCompat
{
    private const string CharacterEntry = "FOOL";
    private const string SourceCharacterEntry = "REGENT";
    private const string AssemblyName = "HildeFoolSkin";
    private const string AuthorPatchType = "HildeFoolSkin.Entry+AncientConversations";

    internal static bool OwnsPatch(MethodBase target, MethodInfo callback) =>
        target.DeclaringType == typeof(AncientDialogueSet) &&
        target.Name == nameof(AncientDialogueSet.PopulateLocKeys) &&
        callback.DeclaringType?.FullName == AuthorPatchType &&
        callback.Name == "Prefix" &&
        callback.DeclaringType.Assembly.GetName().Name == AssemblyName;

    private static void Postfix(AncientEventModel __instance, ref AncientDialogueSet __result)
    {
        if (LocManager.Instance == null || __result.CharacterDialogues.ContainsKey(CharacterEntry) ||
            !__result.CharacterDialogues.ContainsKey(SourceCharacterEntry) ||
            SkinService.GetSelectedRuntimeProvider("fool") is not { } provider ||
            ManagedSkinModLoader.GetActiveProviderAssembly(provider) is not { } assembly ||
            assembly.GetName().Name != AssemblyName)
        {
            return;
        }

        try
        {
            var author = assembly.GetType(AuthorPatchType);
            var populate = author == null ? null : AccessTools.DeclaredMethod(
                author, "Prefix", [typeof(AncientDialogueSet), typeof(string)]);
            if (populate != null)
            {
                var addFoolDialogue = populate.CreateDelegate<Action<AncientDialogueSet, string>>();
                __result = Project(__result, __instance.Id.Entry, addFoolDialogue);
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"恢复希尔德的先古对话失败：{exception.GetBaseException().Message}");
        }
    }

    internal static AncientDialogueSet Project(
        AncientDialogueSet original,
        string ancientEntry,
        Action<AncientDialogueSet, string> addFoolDialogue)
    {
        var originalCharacters = original.CharacterDialogues;
        if (originalCharacters.ContainsKey(CharacterEntry) ||
            !originalCharacters.TryGetValue(SourceCharacterEntry, out var templates))
        {
            return original;
        }

        // The author adds a list of new FOOL dialogues based on REGENT. Populate just the new
        // entries first so the cached game set and all its loc keys survive a hot switch.
        var additions = new AncientDialogueSet
        {
            FirstVisitEverDialogue = null,
            AgnosticDialogues = [],
            CharacterDialogues = new(originalCharacters.Comparer) { [SourceCharacterEntry] = templates }
        };
        addFoolDialogue(additions, ancientEntry);
        if (!additions.CharacterDialogues.TryGetValue(CharacterEntry, out var dialogues))
        {
            return original;
        }

        additions.CharacterDialogues.Remove(SourceCharacterEntry);
        additions.PopulateLocKeys(ancientEntry);
        if (dialogues.SelectMany(dialogue => dialogue.Lines).Any(line =>
                line.LineText is not { } text || !LocString.Exists("ancients", text.LocEntryKey) ||
                (line.NextButtonText != null && !LocString.Exists("ancients", line.NextButtonText.LocEntryKey))))
        {
            return original;
        }

        return new AncientDialogueSet
        {
            FirstVisitEverDialogue = original.FirstVisitEverDialogue,
            AgnosticDialogues = original.AgnosticDialogues,
            CharacterDialogues = new(originalCharacters, originalCharacters.Comparer)
            {
                [CharacterEntry] = dialogues
            }
        };
    }
}
