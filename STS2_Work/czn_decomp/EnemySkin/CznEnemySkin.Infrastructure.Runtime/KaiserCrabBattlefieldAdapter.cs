using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class KaiserCrabBattlefieldAdapter
{
	internal const string EncounterId = "KAISER_CRAB_BOSS";

	private const string AppliedProfileMeta = "czn_kaiser_crab_battlefield_profile";

	private static readonly string[] NativeLayerPaths = new string[6] { "Layer_00", "Layer_01", "Layer_02", "Layer_03", "Layer_04", "Foreground" };

	private readonly LayeredBackgroundRuntime _backgrounds;

	public KaiserCrabBattlefieldAdapter(LayeredBackgroundRuntime backgrounds)
	{
		_backgrounds = backgrounds;
	}

	public bool TryApply(NCombatBackground nativeBackground, BattlefieldPresentationProfile profile, Rng rng)
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		if (((GodotObject)nativeBackground).HasMeta(StringName.op_Implicit("czn_kaiser_crab_battlefield_profile")))
		{
			return true;
		}
		if (!_backgrounds.TryMount(profile, rng, (Node)(object)nativeBackground, out Control shell))
		{
			return false;
		}
		((Node)shell).Name = StringName.op_Implicit("CznKaiserCrabBattlefield");
		((Node)nativeBackground).MoveChild((Node)(object)shell, 0);
		string[] nativeLayerPaths = NativeLayerPaths;
		foreach (string text in nativeLayerPaths)
		{
			CanvasItem nodeOrNull = ((Node)nativeBackground).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit(text));
			if (nodeOrNull != null)
			{
				nodeOrNull.Visible = false;
			}
		}
		CanvasItem nodeOrNull2 = ((Node)nativeBackground).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("%KaiserCrab"));
		if (nodeOrNull2 != null)
		{
			Color modulate = nodeOrNull2.Modulate;
			nodeOrNull2.Modulate = new Color(modulate.R, modulate.G, modulate.B, 0f);
		}
		StringName obj = StringName.op_Implicit("czn_kaiser_crab_battlefield_profile");
		string id = profile.Id;
		((GodotObject)nativeBackground).SetMeta(obj, Variant.From<string>(ref id));
		Entry.Log.Info("Applied complete CZN Kaiser Crab battlefield presentation [profile=" + profile.Id + ", nativeVisuals=hidden, nativeController=retained].", 1);
		return true;
	}
}
