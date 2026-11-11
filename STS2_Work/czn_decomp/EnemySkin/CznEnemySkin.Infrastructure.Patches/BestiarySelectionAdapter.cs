using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

internal static class BestiarySelectionAdapter
{
	private static readonly FieldInfo SelectedEntryField = AccessTools.Field(typeof(NBestiary), "_selectedEntry") ?? throw new MissingFieldException(typeof(NBestiary).FullName, "_selectedEntry");

	private static readonly MethodInfo SelectMonsterMethod = AccessTools.Method(typeof(NBestiary), "SelectMonster", new Type[1] { typeof(NBestiaryEntry) }, (Type[])null) ?? throw new MissingMethodException(typeof(NBestiary).FullName, "SelectMonster");

	public static void Refresh(NBestiary bestiary, NBestiaryEntry entry)
	{
		try
		{
			SelectedEntryField.SetValue(bestiary, null);
			SelectMonsterMethod.Invoke(bestiary, new object[1] { entry });
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not rebuild selected Bestiary enemy: " + ex.GetBaseException().Message, 1);
		}
	}
}
