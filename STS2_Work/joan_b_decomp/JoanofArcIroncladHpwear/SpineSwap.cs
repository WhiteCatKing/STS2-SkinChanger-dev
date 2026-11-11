using System;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace JoanofArcIroncladHpwear;

internal static class SpineSwap
{
	internal readonly struct MegaScope : IDisposable
	{
		internal readonly MegaSprite Sprite;

		internal MegaScope(Node spine)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			Sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spine));
		}

		public void Dispose()
		{
		}
	}

	internal static Node? FindSpine(Node? root, int depth = 0)
	{
		if (root == null || depth > 6 || !GodotObject.IsInstanceValid((GodotObject)(object)root))
		{
			return null;
		}
		if (root is Node2D && ((GodotObject)root).GetClass() == "SpineSprite")
		{
			return root;
		}
		foreach (Node child in root.GetChildren(false))
		{
			Node val = FindSpine(child, depth + 1);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	internal static Resource? GetSkeletonData(Node spine)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		return (Resource?)(object)((obj is Resource) ? obj : null);
	}

	internal static string? GetSkeletonPath(Node spine)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Resource skeletonData = GetSkeletonData(spine);
		if (skeletonData == null)
		{
			return null;
		}
		Variant val = ((GodotObject)skeletonData).Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		if (obj2 == null)
		{
			return null;
		}
		return ((Resource)obj2).ResourcePath;
	}

	internal static bool IsOurs(Node spine)
	{
		return SkinCatalog.IsOurPath(GetSkeletonPath(spine));
	}

	internal static Resource? LoadStageData(string group, int stage)
	{
		string text = SkinCatalog.DataPath(group, stage);
		try
		{
			Resource obj = GD.Load<Resource>(text);
			if (obj == null)
			{
				ModEntry.Log.Error("missing skeleton data resource: " + text, 1);
			}
			return obj;
		}
		catch (Exception ex)
		{
			ModEntry.Log.Error("cannot load " + text + ": " + ex.Message, 1);
			return null;
		}
	}

	internal static bool SetSkeletonData(Node spine, Resource data)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		string skeletonPath = GetSkeletonPath(spine);
		Variant val = ((GodotObject)data).Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		string text = ((obj2 != null) ? ((Resource)obj2).ResourcePath : null);
		if (text == null)
		{
			return false;
		}
		if (string.Equals(skeletonPath, text, StringComparison.Ordinal))
		{
			return true;
		}
		((GodotObject)spine).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)data));
		if (SkinCatalog.IsOurPath(text))
		{
			UnderwearVisibility.Attach(spine);
			Palette.ApplyNow(spine);
		}
		return string.Equals(GetSkeletonPath(spine), text, StringComparison.Ordinal);
	}

	internal static string? CurrentAnimation(Node spine)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				val = obj.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
				obj2 = ((Variant)(ref val)).AsGodotObject();
			}
			object obj3;
			if (obj2 == null)
			{
				obj3 = null;
			}
			else
			{
				val = ((GodotObject)obj2).Call(StringName.op_Implicit("get_animation"), Array.Empty<Variant>());
				obj3 = ((Variant)(ref val)).AsGodotObject();
			}
			object result;
			if (obj3 == null)
			{
				result = null;
			}
			else
			{
				val = ((GodotObject)obj3).Call(StringName.op_Implicit("get_name"), Array.Empty<Variant>());
				result = ((Variant)(ref val)).AsString();
			}
			return (string?)result;
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("read animation failed: " + ex.Message, 1);
			return null;
		}
	}

	internal static bool IsMixing(Node spine)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			object obj2;
			if (obj == null)
			{
				obj2 = null;
			}
			else
			{
				val = obj.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
				obj2 = ((Variant)(ref val)).AsGodotObject();
			}
			GodotObject val2 = (GodotObject)obj2;
			if (val2 == null || !GodotObject.IsInstanceValid(val2))
			{
				return false;
			}
			val = val2.Call(StringName.op_Implicit("get_mixing_from"), Array.Empty<Variant>());
			GodotObject val3 = ((Variant)(ref val)).AsGodotObject();
			if (val3 == null || !GodotObject.IsInstanceValid(val3))
			{
				return false;
			}
			val = val2.Call(StringName.op_Implicit("get_mix_duration"), Array.Empty<Variant>());
			double num = ((Variant)(ref val)).AsDouble();
			val = val2.Call(StringName.op_Implicit("get_mix_time"), Array.Empty<Variant>());
			double num2 = ((Variant)(ref val)).AsDouble();
			return num > 0.0 && num2 + 0.001 < num;
		}
		catch
		{
			return false;
		}
	}

	internal static bool Play(Node spine, string animation, bool loop)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(animation))
		{
			return false;
		}
		try
		{
			Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
			GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
			if (val2 == null)
			{
				return false;
			}
			val2.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(animation),
				Variant.op_Implicit(loop),
				Variant.op_Implicit(0)
			});
			return true;
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("play '" + animation + "' failed: " + ex.Message, 1);
			return false;
		}
	}
}
