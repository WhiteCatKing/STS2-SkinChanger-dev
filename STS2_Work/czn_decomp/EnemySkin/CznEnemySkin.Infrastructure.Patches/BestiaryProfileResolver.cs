using System.Collections.Generic;
using CznEnemySkin.Domain;
using CznEnemySkin.Localization;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

internal static class BestiaryProfileResolver
{
	public static bool TryResolve(NBestiaryEntry entry, out EnemyReplacementProfile profile)
	{
		if (!entry.IsDiscovered || entry.IsUnderConstruction)
		{
			profile = null;
			return false;
		}
		MonsterModel monsterModel = entry.Entry.monsterModel;
		if (monsterModel != null && Entry.Replacements.TryGetMonsterProfile(((AbstractModel)monsterModel).Id.Entry, out profile))
		{
			return true;
		}
		if (entry.Entry.encounterModel is DecimillipedeElite && Entry.Replacements.TryGetMonsterProfile("DECIMILLIPEDE_SEGMENT_FRONT", out profile))
		{
			return true;
		}
		profile = null;
		return false;
	}

	public static IReadOnlyList<EnemyReplacementProfile> ResolveLinkedProfiles(NBestiaryEntry entry, EnemyReplacementProfile profile)
	{
		if (!(entry.Entry.encounterModel is DecimillipedeElite))
		{
			return new _003C_003Ez__ReadOnlySingleElementList<EnemyReplacementProfile>(profile);
		}
		List<EnemyReplacementProfile> list = new List<EnemyReplacementProfile>();
		string[] array = new string[3] { "DECIMILLIPEDE_SEGMENT_FRONT", "DECIMILLIPEDE_SEGMENT_MIDDLE", "DECIMILLIPEDE_SEGMENT_BACK" };
		foreach (string monsterId in array)
		{
			if (Entry.Replacements.TryGetMonsterProfile(monsterId, out EnemyReplacementProfile profile2))
			{
				list.Add(profile2);
			}
		}
		return list;
	}

	public static void RefreshLabels(NBestiary bestiary, NBestiaryEntry entry, EnemyReplacementProfile profile)
	{
		bool enabled = Entry.Replacements.IsEnabled(profile);
		RichTextLabel nodeOrNull = ((Node)bestiary).GetNodeOrNull<RichTextLabel>(NodePath.op_Implicit("%MonsterName"));
		if (nodeOrNull != null)
		{
			nodeOrNull.Text = FormatDisplayName(entry, profile, enabled);
		}
		RefreshEntryLabel(entry, profile);
	}

	public static void RefreshEntryLabel(NBestiaryEntry entry, EnemyReplacementProfile profile)
	{
		RichTextLabel nodeOrNull = ((Node)entry).GetNodeOrNull<RichTextLabel>(NodePath.op_Implicit("%Label"));
		if (nodeOrNull != null)
		{
			nodeOrNull.Text = FormatDisplayName(entry, profile, Entry.Replacements.IsEnabled(profile));
		}
	}

	private static string FormatDisplayName(NBestiaryEntry entry, EnemyReplacementProfile profile, bool enabled)
	{
		string entryTitle = entry.Entry.GetEntryTitle();
		if (!enabled)
		{
			return entryTitle;
		}
		return EnemyLocalization.FormatBestiaryDisplayName(profile, entryTitle);
	}
}
