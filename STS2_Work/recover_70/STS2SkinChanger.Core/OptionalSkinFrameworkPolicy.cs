using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class OptionalSkinFrameworkPolicy
{
	public static bool ShouldTreatAsGameplayBaseline(bool manifestAffectsGameplay, bool requiredByAnotherMod, bool exposesSelectableCosmetics)
	{
		if (!manifestAffectsGameplay)
		{
			if (requiredByAnotherMod)
			{
				return !exposesSelectableCosmetics;
			}
			return false;
		}
		return true;
	}

	public static bool CanSatisfyMissingDependency(OptionalSkinFrameworkEvidence evidence, IReadOnlyCollection<string> compatibilityAssemblyNames)
	{
		if (evidence.HasDeclarativeSkinContract && evidence.ResourceClosureComplete && !string.IsNullOrWhiteSpace(evidence.DependencyId) && evidence.DependencyId.Equals(evidence.ReferencedAssemblyName, StringComparison.OrdinalIgnoreCase))
		{
			return compatibilityAssemblyNames.Contains<string>(evidence.ReferencedAssemblyName, StringComparer.OrdinalIgnoreCase);
		}
		return false;
	}

	public static bool IsFrameworkHostRequired(string frameworkId, IEnumerable<OptionalSkinFrameworkEvidence> dependents, IReadOnlyCollection<string> compatibilityAssemblyNames)
	{
		OptionalSkinFrameworkEvidence[] array = dependents.Where((OptionalSkinFrameworkEvidence evidence) => evidence.DependencyId.Equals(frameworkId, StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length != 0)
		{
			return array.Any((OptionalSkinFrameworkEvidence evidence) => !CanSatisfyMissingDependency(evidence, compatibilityAssemblyNames));
		}
		return true;
	}

	public static bool CanInstallCompatibilityAssembly(string frameworkId, IEnumerable<OptionalSkinFrameworkEvidence> dependents, bool originalFrameworkHostAvailable = false)
	{
		if (!dependents.Where((OptionalSkinFrameworkEvidence candidate) => candidate.DependencyId.Equals(frameworkId, StringComparison.OrdinalIgnoreCase)).ToArray().Any((OptionalSkinFrameworkEvidence candidate) => CanSatisfyMissingDependency(candidate, new _003C_003Ez__ReadOnlySingleElementList<string>(frameworkId))))
		{
			return false;
		}
		return !originalFrameworkHostAvailable;
	}
}
