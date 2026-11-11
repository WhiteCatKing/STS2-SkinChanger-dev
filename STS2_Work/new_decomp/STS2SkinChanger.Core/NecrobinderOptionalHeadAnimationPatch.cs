using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NNecrobinderVfx), "UpdateFlameVisibility")]
internal static class NecrobinderOptionalHeadAnimationPatch
{
	private static bool Prefix(Node2D? ____headRef)
	{
		if (____headRef != null && GodotObject.IsInstanceValid((GodotObject)(object)____headRef))
		{
			return !((GodotObject)____headRef).IsQueuedForDeletion();
		}
		return false;
	}
}
