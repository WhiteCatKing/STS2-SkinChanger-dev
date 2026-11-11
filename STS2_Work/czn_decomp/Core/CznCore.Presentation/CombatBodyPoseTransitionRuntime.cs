using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznCore.Presentation;

internal static class CombatBodyPoseTransitionRuntime
{
	private const string SourcePathMeta = "czn_atomic_pose_source_path";

	public static Resource CreateResource(Resource source)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Resource obj = source.Duplicate(false);
		((GodotObject)obj).Set(StringName.op_Implicit("default_mix"), Variant.op_Implicit(0f));
		StringName obj2 = StringName.op_Implicit("czn_atomic_pose_source_path");
		Variant val;
		if (!((GodotObject)source).HasMeta(StringName.op_Implicit("czn_atomic_pose_source_path")))
		{
			string resourcePath = source.ResourcePath;
			val = Variant.From<string>(ref resourcePath);
		}
		else
		{
			val = ((GodotObject)source).GetMeta(StringName.op_Implicit("czn_atomic_pose_source_path"), default(Variant));
		}
		((GodotObject)obj).SetMeta(obj2, val);
		return obj;
	}

	public static bool IsSource(Resource resource, string path)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (!(resource.ResourcePath == path))
		{
			if (((GodotObject)resource).HasMeta(StringName.op_Implicit("czn_atomic_pose_source_path")))
			{
				StringName obj = StringName.op_Implicit("czn_atomic_pose_source_path");
				Variant val = default(Variant);
				val = ((GodotObject)resource).GetMeta(obj, val);
				return ((Variant)(ref val)).AsString() == path;
			}
			return false;
		}
		return true;
	}

	public static void Enable(MegaSprite sprite)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		GodotObject boundObject = ((MegaSpineBinding)sprite).BoundObject;
		Node val = (Node)(object)((boundObject is Node) ? boundObject : null);
		if (val == null || ((GodotObject)val).HasMeta(StringName.op_Implicit("czn_atomic_pose_bound")))
		{
			return;
		}
		Variant val2 = ((GodotObject)val).Call(StringName.op_Implicit("get_skeleton_data_res"), Array.Empty<Variant>());
		try
		{
			GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
			Resource val3 = (Resource)(object)((obj is Resource) ? obj : null);
			if (val3 == null)
			{
				SkinLog.Warn("Cannot configure atomic body pose: missing skeleton resource");
				return;
			}
			GodotObject val4 = (GodotObject)(object)CreateResource(val3);
			sprite.SetSkeletonDataRes(new MegaSkeletonDataResource(Variant.From<GodotObject>(ref val4)));
			Bind(sprite);
		}
		finally
		{
			((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public static void Bind(MegaSprite sprite)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		GodotObject boundObject = ((MegaSpineBinding)sprite).BoundObject;
		Node node = (Node)(object)((boundObject is Node) ? boundObject : null);
		if (node == null || ((GodotObject)node).HasMeta(StringName.op_Implicit("czn_atomic_pose_bound")))
		{
			return;
		}
		bool pending = false;
		Callable settle = Callable.From((Action)delegate
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			pending = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)node) && node.IsInsideTree())
			{
				((GodotObject)node).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
				((GodotObject)node).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0f) });
			}
		});
		Callable val = Callable.From<GodotObject, GodotObject, GodotObject>((Action<GodotObject, GodotObject, GodotObject>)delegate
		{
			if (!pending)
			{
				pending = true;
				((Callable)(ref settle)).CallDeferred(Array.Empty<Variant>());
			}
		});
		if ((int)sprite.ConnectAnimationStarted(val) == 0)
		{
			((GodotObject)node).SetMeta(StringName.op_Implicit("czn_atomic_pose_bound"), Variant.op_Implicit(true));
		}
		else
		{
			SkinLog.Warn("Cannot bind atomic body pose transitions");
		}
	}
}
