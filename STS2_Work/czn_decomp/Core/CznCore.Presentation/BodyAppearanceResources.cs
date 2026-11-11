using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CznCore.Presentation;

internal static class BodyAppearanceResources
{
	internal static bool Apply(CharacterSkinProfile? profile, Player? owner, Node2D spine)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if (profile?.Appearance == null || owner == null || !profile.AppliesTo(owner) || ((GodotObject)spine).GetClass() != "SpineSprite")
		{
			return false;
		}
		CharacterSkinProfile characterSkinProfile = profile.For(owner);
		Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		Resource val2 = (Resource)(object)((obj is Resource) ? obj : null);
		if (val2 == null)
		{
			return false;
		}
		string text = profile.RuntimeMetaKey("noncombat_body_appearance");
		if (!((GodotObject)spine).HasMeta(StringName.op_Implicit(text)) && !profile.NonCombatBodySkeletonPaths.Contains(val2.ResourcePath))
		{
			return false;
		}
		string text2;
		if (!(characterSkinProfile.BodySkeletonDataPath == profile.BodySkeletonDataPath))
		{
			text2 = characterSkinProfile.BodySkeletonDataPath;
		}
		else
		{
			val = ((GodotObject)spine).GetMeta(StringName.op_Implicit(text), Variant.op_Implicit(val2.ResourcePath));
			text2 = ((Variant)(ref val)).AsString();
		}
		string text3 = text2;
		if (val2.ResourcePath == text3)
		{
			return false;
		}
		Resource val3 = ResourceLoader.Load<Resource>(text3, (string)null, (CacheMode)1);
		if (val3 == null)
		{
			return false;
		}
		if (!((GodotObject)spine).HasMeta(StringName.op_Implicit(text)))
		{
			((GodotObject)spine).SetMeta(StringName.op_Implicit(text), Variant.op_Implicit(val2.ResourcePath));
		}
		((GodotObject)spine).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)val3));
		return true;
	}
}
