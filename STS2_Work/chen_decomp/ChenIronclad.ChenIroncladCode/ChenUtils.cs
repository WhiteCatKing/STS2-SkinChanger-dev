using System;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;

namespace ChenIronclad.ChenIroncladCode;

public class ChenUtils
{
	public static bool IsIronclad(CharacterModel instance)
	{
		ModelId id = ((AbstractModel)instance).Id;
		string entry = id.Entry;
		if (ChenConfig.DebugMode)
		{
			Log.Info("[CHEN] Utils has been called to check " + id.Entry + " - " + entry, 2);
		}
		if (string.IsNullOrWhiteSpace(entry))
		{
			return false;
		}
		if (!string.Equals(entry, "ironclad", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return true;
	}
}
