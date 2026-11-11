using System;
using System.Collections.Generic;
using Godot;

namespace CznCore.Presentation;

internal static class DefeatCgFraming
{
	internal const float FitRatio = 0.9f;

	internal static Rect2 ContentRect(Texture2D texture)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Rect2 val = default(Rect2);
		((Rect2)(ref val))._002Ector(Vector2.Zero, texture.GetSize());
		Image image = texture.GetImage();
		try
		{
			if (image == null || (image.IsCompressed() && (int)image.Decompress() != 0))
			{
				return val;
			}
			Rect2I usedRect = image.GetUsedRect();
			return (Rect2)(((Rect2I)(ref usedRect)).HasArea() ? new Rect2(Vector2I.op_Implicit(((Rect2I)(ref usedRect)).Position), Vector2I.op_Implicit(((Rect2I)(ref usedRect)).Size)) : val);
		}
		finally
		{
			((IDisposable)image)?.Dispose();
		}
	}

	internal static IReadOnlyList<DefeatCgMotionLayerProfile> Reframe(IReadOnlyList<DefeatCgMotionLayerProfile> layers, Vector2 viewport, Vector2 textureSize, Rect2 content, float baseScale)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		DefeatCgMotionKeyProfile defeatCgMotionKeyProfile = null;
		foreach (DefeatCgMotionLayerProfile layer in layers)
		{
			if (!layer.Additive && layer.Keyframes.Count != 0)
			{
				DefeatCgMotionKeyProfile defeatCgMotionKeyProfile2 = Sample(layer.Keyframes, 0.5f);
				if (defeatCgMotionKeyProfile == null || defeatCgMotionKeyProfile2.Alpha > defeatCgMotionKeyProfile.Alpha)
				{
					defeatCgMotionKeyProfile = defeatCgMotionKeyProfile2;
				}
			}
		}
		Vector2 val = ((Rect2)(ref content)).Size * baseScale * (defeatCgMotionKeyProfile?.ScaleMultiplier ?? 0f);
		if (defeatCgMotionKeyProfile == null || viewport.X <= 0f || viewport.Y <= 0f || val.X <= 0.001f || val.Y <= 0.001f)
		{
			return layers;
		}
		float num = MathF.Min(viewport.X / val.X, viewport.Y / val.Y) * 0.9f;
		Vector2 val2 = (((Rect2)(ref content)).GetCenter() - textureSize * 0.5f) * (baseScale * defeatCgMotionKeyProfile.ScaleMultiplier) / viewport;
		List<DefeatCgMotionLayerProfile> list = new List<DefeatCgMotionLayerProfile>(layers.Count);
		foreach (DefeatCgMotionLayerProfile layer2 in layers)
		{
			List<DefeatCgMotionKeyProfile> list2 = new List<DefeatCgMotionKeyProfile>(layer2.Keyframes.Count);
			foreach (DefeatCgMotionKeyProfile keyframe in layer2.Keyframes)
			{
				list2.Add(keyframe with
				{
					OffsetXRatio = (keyframe.OffsetXRatio - defeatCgMotionKeyProfile.OffsetXRatio - val2.X) * num,
					OffsetYRatio = (keyframe.OffsetYRatio - defeatCgMotionKeyProfile.OffsetYRatio - val2.Y) * num,
					ScaleMultiplier = keyframe.ScaleMultiplier * num
				});
			}
			list.Add(layer2 with
			{
				Keyframes = list2
			});
		}
		return list;
	}

	private static DefeatCgMotionKeyProfile Sample(IReadOnlyList<DefeatCgMotionKeyProfile> keys, float time)
	{
		if (time <= keys[0].Offset)
		{
			return keys[0];
		}
		for (int i = 1; i < keys.Count; i++)
		{
			DefeatCgMotionKeyProfile defeatCgMotionKeyProfile = keys[i];
			if (!(time > defeatCgMotionKeyProfile.Offset))
			{
				DefeatCgMotionKeyProfile defeatCgMotionKeyProfile2 = keys[i - 1];
				float amount = Math.Clamp((time - defeatCgMotionKeyProfile2.Offset) / MathF.Max(1E-05f, defeatCgMotionKeyProfile.Offset - defeatCgMotionKeyProfile2.Offset), 0f, 1f);
				return new DefeatCgMotionKeyProfile(time, float.Lerp(defeatCgMotionKeyProfile2.OffsetXRatio, defeatCgMotionKeyProfile.OffsetXRatio, amount), float.Lerp(defeatCgMotionKeyProfile2.OffsetYRatio, defeatCgMotionKeyProfile.OffsetYRatio, amount), float.Lerp(defeatCgMotionKeyProfile2.ScaleMultiplier, defeatCgMotionKeyProfile.ScaleMultiplier, amount), float.Lerp(defeatCgMotionKeyProfile2.Alpha, defeatCgMotionKeyProfile.Alpha, amount));
			}
		}
		return keys[keys.Count - 1];
	}
}
