using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Forms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace CznCore.Presentation;

internal static class CombatFormVfxSurfaceRuntime
{
	private const string CombatBodySurfaceMetaSuffix = "combat_body_surface_scaled";

	public static bool OwnsSurface(CharacterSkinProfile profile, Player? owner)
	{
		if (owner != null && profile.AppliesTo(owner) && profile.Features.CombatBodyScalePatch)
		{
			return !string.IsNullOrWhiteSpace(profile.BodySkeletonDataPath);
		}
		return false;
	}

	public static void Adapt(CharacterSkinProfile profile, NFormVfx formVfx, Player owner)
	{
		if (!OwnsSurface(profile, owner) || !TryResolveBoneName(profile.FormVfx, formVfx, out string boneName))
		{
			return;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(owner.Creature) : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return;
		}
		Node2D currentBody = val.Visuals.GetCurrentBody();
		if (currentBody == null || !GodotObject.IsInstanceValid((GodotObject)(object)currentBody) || !((GodotObject)currentBody).HasMeta(StringName.op_Implicit(profile.RuntimeMetaKey("combat_body_surface_scaled"))))
		{
			return;
		}
		MegaSprite spineBody = val.Visuals.SpineBody;
		if (spineBody == null)
		{
			return;
		}
		if (!spineBody.GetGlobalBoneTransform(boneName).HasValue)
		{
			int num = RebindFollowers((Node)(object)formVfx, spineBody, string.Empty);
			SkinLog.Warn("Form VFX adapter could not resolve replacement bone '" + boneName + "' for " + ((object)formVfx).GetType().Name + "; disabled " + num + " bone follower(s) and kept the holder-relative fallback.");
			return;
		}
		int rebound = RebindFollowers((Node)(object)formVfx, spineBody, boneName);
		if (rebound == 0)
		{
			SkinLog.Warn("Form VFX adapter found no bone follower for " + ((object)formVfx).GetType().Name + ".");
			return;
		}
		SkinLog.Debug(() => "Adapted " + ((object)formVfx).GetType().Name + " to replacement bone '" + boneName + "' followers=" + rebound);
	}

	public static void DisableFailedVisual(NFormVfx? formVfx)
	{
		if (formVfx != null && GodotObject.IsInstanceValid((GodotObject)(object)formVfx))
		{
			((CanvasItem)formVfx).Visible = false;
			((Node)formVfx).SetProcess(false);
			((Node)formVfx).SetPhysicsProcess(false);
		}
	}

	private static bool TryResolveBoneName(CharacterSkinFormVfxProfile profile, NFormVfx formVfx, out string boneName)
	{
		string text = ((formVfx is NReaperFormVfx) ? profile.ReaperBodyBoneName : ((!(formVfx is NEchoFormVfx) && !(formVfx is NSerpentFormVfx)) ? ((!(formVfx is NVoidFormVfx)) ? string.Empty : profile.HeadBoneName) : profile.UpperBodyBoneName));
		boneName = text;
		return !string.IsNullOrWhiteSpace(boneName);
	}

	private static int RebindFollowers(Node node, MegaSprite spineSprite, string boneName)
	{
		int num = 0;
		NSpineSpriteBoneFollower val = (NSpineSpriteBoneFollower)(object)((node is NSpineSpriteBoneFollower) ? node : null);
		if (val != null)
		{
			val.SetSpineSprite(spineSprite, boneName);
			num++;
		}
		foreach (Node child in node.GetChildren(false))
		{
			num += RebindFollowers(child, spineSprite, boneName);
		}
		return num;
	}
}
