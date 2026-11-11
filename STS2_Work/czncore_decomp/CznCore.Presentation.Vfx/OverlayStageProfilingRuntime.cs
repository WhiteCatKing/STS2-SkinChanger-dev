using System;
using System.Text;
using CznCore.Godot;
using Godot;

namespace CznCore.Presentation.Vfx;

internal static class OverlayStageProfilingRuntime
{
	public static OverlayStageProfileSnapshot Capture(Node? activeOverlay, Node? parkedOverlay)
	{
		bool hasActiveOverlay = GodotNodeUtil.IsAlive((GodotObject?)(object)activeOverlay);
		bool hasParkedOverlay = GodotNodeUtil.IsAlive((GodotObject?)(object)parkedOverlay);
		OverlayStageNodeCounts overlayStageNodeCounts = CountSpineAndViewport(activeOverlay);
		OverlayStageNodeCounts overlayStageNodeCounts2 = CountSpineAndViewport(parkedOverlay);
		return new OverlayStageProfileSnapshot(hasActiveOverlay, hasParkedOverlay, overlayStageNodeCounts.Spines, overlayStageNodeCounts2.Spines, overlayStageNodeCounts.Viewports, overlayStageNodeCounts2.Viewports, GC.GetTotalMemory(forceFullCollection: false) / 1048576, OS.GetStaticMemoryUsage() / 1048576);
	}

	public static string FormatSnapshot(string label, string phase, string? extraFields, OverlayStageProfileSnapshot snapshot)
	{
		StringBuilder stringBuilder = new StringBuilder(string.IsNullOrWhiteSpace(label) ? "overlay profile" : label);
		stringBuilder.Append(" phase=").Append(phase);
		if (!string.IsNullOrWhiteSpace(extraFields))
		{
			if (extraFields[0] != ' ')
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(extraFields);
		}
		stringBuilder.Append(" activeOverlay=").Append(snapshot.HasActiveOverlay ? 1 : 0).Append(" parkedOverlay=")
			.Append(snapshot.HasParkedOverlay ? 1 : 0)
			.Append(" activeSpines=")
			.Append(snapshot.ActiveSpines)
			.Append(" parkedSpines=")
			.Append(snapshot.ParkedSpines)
			.Append(" activeViewports=")
			.Append(snapshot.ActiveViewports)
			.Append(" parkedViewports=")
			.Append(snapshot.ParkedViewports)
			.Append(" managedMiB=")
			.Append(snapshot.ManagedMiB)
			.Append(" nativeMiB=")
			.Append(snapshot.NativeMiB);
		return stringBuilder.ToString();
	}

	public static OverlayStageNodeCounts CountSpineAndViewport(Node? root)
	{
		if (!GodotNodeUtil.IsAlive((GodotObject?)(object)root))
		{
			return new OverlayStageNodeCounts(0, 0);
		}
		int num = 0;
		int num2 = 0;
		foreach (Node item in GodotNodeUtil.TraverseDepthFirst(root))
		{
			if (GodotNodeUtil.IsClass((GodotObject?)(object)item, "SpineSprite"))
			{
				num++;
			}
			else if (item is SubViewport)
			{
				num2++;
			}
		}
		return new OverlayStageNodeCounts(num, num2);
	}
}
