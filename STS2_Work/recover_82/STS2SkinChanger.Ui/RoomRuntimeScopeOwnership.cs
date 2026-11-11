using System.Runtime.CompilerServices;
using Godot;

namespace STS2SkinChanger.Ui;

internal static class RoomRuntimeScopeOwnership
{
	private static readonly ConditionalWeakTable<object, StrongBox<long>> Leases = new ConditionalWeakTable<object, StrongBox<long>>();

	internal static void Record(object owner, long lease)
	{
		if (lease != 0L)
		{
			Leases.GetOrCreateValue(owner).Value = lease;
		}
	}

	internal static void Refresh(object owner, long lease)
	{
		if (lease != 0L && Leases.TryGetValue(owner, out StrongBox<long> value))
		{
			value.Value = lease;
		}
	}

	internal static void Release(object owner)
	{
		if (Leases.TryGetValue(owner, out StrongBox<long> value))
		{
			Leases.Remove(owner);
			CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(null, "对局角色", value.Value);
		}
	}

	internal static void RefreshTree(Node? root, long lease)
	{
		if (root == null || !GodotObject.IsInstanceValid((GodotObject)(object)root) || lease == 0L)
		{
			return;
		}
		Refresh(root, lease);
		foreach (Node child in root.GetChildren(false))
		{
			RefreshTree(child, lease);
		}
	}
}
