using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed class ProviderSettingsTargets<T> where T : class
{
	private sealed record Binding(WeakReference<T> Target, string GroupId, string ProviderId);

	private readonly List<Binding> bindings = new List<Binding>();

	public void Bind(T target, string groupId, string providerId)
	{
		bindings.RemoveAll((Binding entry) => !entry.Target.TryGetTarget(out var target2) || target2 == target);
		bindings.Add(new Binding(new WeakReference<T>(target), groupId, providerId));
	}

	public int Refresh(Func<T, bool> isAlive, Func<T, string?> resolveGroup, Func<string, string?> selectedProvider, Action<T> apply)
	{
		bindings.RemoveAll((Binding entry) => !entry.Target.TryGetTarget(out var target) || !isAlive(target));
		int num = 0;
		Binding[] array = bindings.ToArray();
		foreach (Binding binding in array)
		{
			if (binding.Target.TryGetTarget(out var target2) && isAlive(target2) && string.Equals(resolveGroup(target2), binding.GroupId, StringComparison.OrdinalIgnoreCase) && string.Equals(selectedProvider(binding.GroupId), binding.ProviderId, StringComparison.OrdinalIgnoreCase))
			{
				apply(target2);
				num++;
			}
		}
		return num;
	}
}
