using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class CharacterCombatSceneInstantiationPolicy
{
	public static bool ShouldUseManagedFactory(bool isBaseSelection, bool hasManagedCombatScene, bool hasManagedCombatDependencies)
	{
		return isBaseSelection || hasManagedCombatScene || hasManagedCombatDependencies;
	}

	public static bool ShouldOwnCanonicalSceneInstantiation(bool isBaseSelection, bool servesCanonicalCombatScene)
	{
		return !isBaseSelection && servesCanonicalCombatScene;
	}

	public static bool HasManagedCombatDependencies(string scenePath, IEnumerable<string> assetPaths)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(scenePath);
		string combatDependencyPrefix = "res://animations/characters/" + fileNameWithoutExtension + "/";
		return assetPaths.Any((string path) => path.StartsWith(combatDependencyPrefix, StringComparison.OrdinalIgnoreCase));
	}

	public static bool ShouldRestoreCanonicalOwnership(string? scopedSelection, string configuredSelection)
	{
		if (scopedSelection != null)
		{
			return !scopedSelection.Equals(configuredSelection, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}
