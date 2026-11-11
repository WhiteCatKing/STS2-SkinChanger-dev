using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace CznCore.Presentation;

internal static class CombatLayerLiftRuntime
{
	private static readonly Dictionary<string, HashSet<Node2D>> OwnerSpines = new Dictionary<string, HashSet<Node2D>>(StringComparer.Ordinal);

	private const string SharedStateMeta = "czn_core_combat_layer_lift_shared_state";

	private const string OriginalZIndexKey = "original_z_index";

	private const string OriginalZAsRelativeKey = "original_z_as_relative";

	private const string LeaseKeyPrefix = "lease:";

	public static void Acquire(Node2D? spine, string ownerKey, int zIndex)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		ArgumentException.ThrowIfNullOrEmpty(ownerKey, "ownerKey");
		if (spine != null && GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			if (zIndex < 0)
			{
				Release(spine, ownerKey);
				return;
			}
			Dictionary orCreateState = GetOrCreateState(spine);
			orCreateState[Variant.op_Implicit(LeaseKey(ownerKey))] = Variant.op_Implicit(CombatPresentationGodotLayeringRuntime.ClampCanvasItemZIndex(zIndex));
			((GodotObject)spine).SetMeta(StringName.op_Implicit("czn_core_combat_layer_lift_shared_state"), Variant.op_Implicit(orCreateState));
			Track(ownerKey, spine);
			Apply(spine, orCreateState);
		}
	}

	public static void AcquireDrawOrder(Node2D? spine, string ownerKey)
	{
		ArgumentException.ThrowIfNullOrEmpty(ownerKey, "ownerKey");
		if (spine != null && GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			CombatActorDrawOrder.AcquireTracked(spine, ownerKey);
			Track(ownerKey, spine);
		}
	}

	public static bool Release(Node2D? spine, string ownerKey)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		ArgumentException.ThrowIfNullOrEmpty(ownerKey, "ownerKey");
		if (spine == null)
		{
			return false;
		}
		Untrack(ownerKey, spine);
		bool result = CombatActorDrawOrder.ReleaseTracked(spine, ownerKey);
		if (!GodotObject.IsInstanceValid((GodotObject)(object)spine) || !TryGetState(spine, out Dictionary state))
		{
			return result;
		}
		string text = LeaseKey(ownerKey);
		if (!state.ContainsKey(Variant.op_Implicit(text)))
		{
			return false;
		}
		state.Remove(Variant.op_Implicit(text));
		if (TryResolveHighestLeaseZIndex(state, out var highestZIndex))
		{
			((GodotObject)spine).SetMeta(StringName.op_Implicit("czn_core_combat_layer_lift_shared_state"), Variant.op_Implicit(state));
			Apply(spine, highestZIndex);
			return true;
		}
		try
		{
			Variant val = state[Variant.op_Implicit("original_z_index")];
			((CanvasItem)spine).ZIndex = ((Variant)(ref val)).AsInt32();
			val = state[Variant.op_Implicit("original_z_as_relative")];
			((CanvasItem)spine).ZAsRelative = ((Variant)(ref val)).AsBool();
		}
		finally
		{
			((GodotObject)spine).RemoveMeta(StringName.op_Implicit("czn_core_combat_layer_lift_shared_state"));
		}
		return true;
	}

	public static int ReleaseAll(string ownerKey)
	{
		ArgumentException.ThrowIfNullOrEmpty(ownerKey, "ownerKey");
		if (!OwnerSpines.TryGetValue(ownerKey, out HashSet<Node2D> value))
		{
			return 0;
		}
		Node2D[] array = (Node2D[])(object)new Node2D[value.Count];
		value.CopyTo(array);
		int num = 0;
		Node2D[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (Release(array2[i], ownerKey))
			{
				num++;
			}
		}
		OwnerSpines.Remove(ownerKey);
		return num;
	}

	public static int ActiveCount(string ownerKey)
	{
		ArgumentException.ThrowIfNullOrEmpty(ownerKey, "ownerKey");
		if (!OwnerSpines.TryGetValue(ownerKey, out HashSet<Node2D> value))
		{
			return 0;
		}
		return value.Count;
	}

	private static Dictionary GetOrCreateState(Node2D surface)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		if (TryGetState(surface, out Dictionary state))
		{
			return state;
		}
		return new Dictionary
		{
			[Variant.op_Implicit("original_z_index")] = Variant.op_Implicit(((CanvasItem)surface).ZIndex),
			[Variant.op_Implicit("original_z_as_relative")] = Variant.op_Implicit(((CanvasItem)surface).ZAsRelative)
		};
	}

	private static bool TryGetState(Node2D surface, out Dictionary state)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		state = new Dictionary();
		try
		{
			if (!((GodotObject)surface).HasMeta(StringName.op_Implicit("czn_core_combat_layer_lift_shared_state")))
			{
				return false;
			}
			StringName obj = StringName.op_Implicit("czn_core_combat_layer_lift_shared_state");
			Variant val = default(Variant);
			val = ((GodotObject)surface).GetMeta(obj, val);
			state = ((Variant)(ref val)).AsGodotDictionary();
			return state.ContainsKey(Variant.op_Implicit("original_z_index")) && state.ContainsKey(Variant.op_Implicit("original_z_as_relative"));
		}
		catch
		{
			return false;
		}
	}

	private static void Apply(Node2D surface, Dictionary state)
	{
		if (TryResolveHighestLeaseZIndex(state, out var highestZIndex))
		{
			Apply(surface, highestZIndex);
		}
	}

	private static void Apply(Node2D surface, int zIndex)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)surface))
		{
			((CanvasItem)surface).ZAsRelative = false;
			((CanvasItem)surface).ZIndex = zIndex;
		}
	}

	private static string LeaseKey(string ownerKey)
	{
		return "lease:" + ownerKey;
	}

	private static bool TryResolveHighestLeaseZIndex(Dictionary state, out int highestZIndex)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I8
		highestZIndex = int.MinValue;
		foreach (Variant key in state.Keys)
		{
			Variant current = key;
			if (((Variant)(ref current)).AsString().StartsWith("lease:", StringComparison.Ordinal))
			{
				Variant val = state[current];
				int val2 = (((long)((Variant)(ref val)).VariantType == 2) ? ((Variant)(ref val)).AsInt32() : (-6));
				highestZIndex = Math.Max(highestZIndex, val2);
			}
		}
		return highestZIndex != int.MinValue;
	}

	private static void Track(string ownerKey, Node2D spine)
	{
		if (!OwnerSpines.TryGetValue(ownerKey, out HashSet<Node2D> value))
		{
			value = new HashSet<Node2D>();
			OwnerSpines[ownerKey] = value;
		}
		value.Add(spine);
	}

	private static void Untrack(string ownerKey, Node2D spine)
	{
		if (OwnerSpines.TryGetValue(ownerKey, out HashSet<Node2D> value) && value.Remove(spine) && value.Count == 0)
		{
			OwnerSpines.Remove(ownerKey);
		}
	}
}
