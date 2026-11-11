using System;
using System.Runtime.CompilerServices;
using Godot;

namespace JoanofArcIroncladHpwear;

internal static class UnderwearVisibility
{
	private sealed class State
	{
		internal Color? HiddenColor;

		internal ulong DataId;
	}

	private static readonly ConditionalWeakTable<Node, State> States = new ConditionalWeakTable<Node, State>();

	internal static void Attach(Node spine)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		if (!States.TryGetValue(spine, out State value))
		{
			value = new State();
			States.Add(spine, value);
			State captured = value;
			((GodotObject)spine).Connect(StringName.op_Implicit("before_animation_state_apply"), Callable.From<GodotObject>((Action<GodotObject>)delegate
			{
				Restore(spine, captured);
			}), 0u);
			((GodotObject)spine).Connect(StringName.op_Implicit("before_world_transforms_change"), Callable.From<GodotObject>((Action<GodotObject>)delegate
			{
				Apply(spine, captured);
			}), 0u);
			((GodotObject)spine).Connect(StringName.op_Implicit("_internal_spine_objects_invalidated"), Callable.From((Action)delegate
			{
				captured.HiddenColor = null;
			}), 0u);
			spine.TreeExiting += delegate
			{
				Restore(spine, captured);
			};
		}
		Restore(spine, value);
		Apply(spine, value);
	}

	private static GodotObject? Slot(Node spine)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_skeleton"), Array.Empty<Variant>());
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		if (obj == null)
		{
			return null;
		}
		val = obj.Call(StringName.op_Implicit("find_slot"), (Variant[])(object)new Variant[1] { Variant.op_Implicit("neiku") });
		return ((Variant)(ref val)).AsGodotObject();
	}

	private static void Restore(Node spine, State state)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (!state.HiddenColor.HasValue)
		{
			return;
		}
		Color value = state.HiddenColor.Value;
		state.HiddenColor = null;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		Resource? skeletonData = SpineSwap.GetSkeletonData(spine);
		if (((skeletonData != null) ? new ulong?(((GodotObject)skeletonData).GetInstanceId()) : ((ulong?)null)) == state.DataId)
		{
			GodotObject? obj = Slot(spine);
			if (obj != null)
			{
				obj.Call(StringName.op_Implicit("set_color"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(value) });
			}
		}
	}

	private static void Apply(Node spine, State state)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (WearSettingsRuntime.Current.ShowUnderwear || !GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		string skeletonPath = SpineSwap.GetSkeletonPath(spine);
		if (SkinCatalog.IsOurPath(skeletonPath) && skeletonPath.EndsWith("/s2_ironclad.spskel", StringComparison.Ordinal))
		{
			GodotObject val = Slot(spine);
			if (val != null)
			{
				Variant val2 = val.Call(StringName.op_Implicit("get_color"), Array.Empty<Variant>());
				Color val3 = ((Variant)(ref val2)).AsColor();
				state.HiddenColor = val3;
				state.DataId = ((GodotObject)SpineSwap.GetSkeletonData(spine)).GetInstanceId();
				val3.A = 0f;
				val.Call(StringName.op_Implicit("set_color"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(val3) });
			}
		}
	}
}
