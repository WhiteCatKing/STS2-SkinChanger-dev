using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class RuntimeProviderScopePolicy
{
	public static IReadOnlyCollection<string> ResolveCharacterSelectionTargets(bool isFullRuntimeProvider, IEnumerable<string> providerGroupIds, string requestedGroupId)
	{
		if (!isFullRuntimeProvider)
		{
			return new _003C_003Ez__ReadOnlySingleElementList<string>(requestedGroupId);
		}
		return (IReadOnlyCollection<string>)(object)providerGroupIds.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public static IReadOnlySet<string> SelectActiveProviders(IEnumerable<RuntimeProviderCandidate> candidates, RuntimeProviderScope? scope)
	{
		RuntimeProviderCandidate[] source = candidates.ToArray();
		if (scope == null)
		{
			return source.Select((RuntimeProviderCandidate candidate) => candidate.ProviderId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		HashSet<string> visibleGroups = scope.VisibleGroupIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> runEnvironmentProviders = scope.RunEnvironmentProviderIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return (from candidate in source
			where candidate.GroupIds.Any(visibleGroups.Contains) || (candidate.IsRunWideMonsterProvider && runEnvironmentProviders.Contains(candidate.ProviderId))
			select candidate.ProviderId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public static IReadOnlySet<string> SelectRunEnvironmentProviders(IEnumerable<RuntimeProviderPriorityCandidate> priorities)
	{
		RuntimeProviderPriorityCandidate runtimeProviderPriorityCandidate = priorities.FirstOrDefault((RuntimeProviderPriorityCandidate candidate) => candidate.Enabled && candidate.IsRunWideMonsterProvider && candidate.AppliesToCurrentCombat);
		if (!(runtimeProviderPriorityCandidate == null))
		{
			return new HashSet<string>(new _003C_003Ez__ReadOnlySingleElementList<string>(runtimeProviderPriorityCandidate.ProviderId), StringComparer.OrdinalIgnoreCase);
		}
		return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public static IReadOnlySet<string> MergeVisibleGroups(IEnumerable<string>? requestedGroupIds, IEnumerable<string>? currentCombatGroupIds)
	{
		HashSet<string> hashSet = requestedGroupIds?.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (currentCombatGroupIds != null)
		{
			hashSet.UnionWith(currentCombatGroupIds.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)));
		}
		return hashSet;
	}

	public static bool IsRunEnvironmentPatchTarget(string typeName, string methodName)
	{
		if (!IsRunEnvironmentIdentifier(typeName))
		{
			return IsRunEnvironmentIdentifier(methodName);
		}
		return true;
	}

	public static bool IsRunEnvironmentCallback(string typeName, string methodName)
	{
		if (methodName.StartsWith("On", StringComparison.Ordinal) || methodName.StartsWith("Set", StringComparison.Ordinal) || methodName.StartsWith("Update", StringComparison.Ordinal) || methodName.StartsWith("Play", StringComparison.Ordinal))
		{
			if (!IsRunEnvironmentIdentifier(typeName))
			{
				return IsRunEnvironmentIdentifier(methodName);
			}
			return true;
		}
		return false;
	}

	public static bool IsRunEnvironmentControllerType(string typeName)
	{
		return IsRunEnvironmentIdentifier(typeName);
	}

	private static bool IsRunEnvironmentIdentifier(string value)
	{
		if (!value.Contains("music", StringComparison.OrdinalIgnoreCase) && !value.Contains("bgm", StringComparison.OrdinalIgnoreCase) && !value.Contains("background", StringComparison.OrdinalIgnoreCase))
		{
			return value.Contains("battlefield", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
