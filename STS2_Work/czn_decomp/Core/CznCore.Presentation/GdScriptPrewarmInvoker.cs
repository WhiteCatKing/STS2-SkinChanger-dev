using System;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

namespace CznCore.Presentation;

internal static class GdScriptPrewarmInvoker
{
	public static Task InvokeAsync(string scriptPath, string methodName, string label)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!ResourceLoader.Exists(scriptPath, ""))
			{
				SkinLog.Debug(() => "GDScript prewarm skipped (script missing): " + scriptPath);
				return Task.CompletedTask;
			}
			Resource obj = ResourceLoader.Load(scriptPath, "", (CacheMode)1);
			GDScript val = (GDScript)(object)((obj is GDScript) ? obj : null);
			if (val == null)
			{
				SkinLog.Warn("GDScript prewarm skipped (not a GDScript): " + scriptPath);
				return Task.CompletedTask;
			}
			if (!HasMethod(val, methodName))
			{
				SkinLog.Debug(() => "GDScript prewarm entry not present yet: " + scriptPath + "#" + methodName);
				return Task.CompletedTask;
			}
			ulong ticksMsec = Time.GetTicksMsec();
			Variant val2 = ((GodotObject)val).Call(StringName.op_Implicit(methodName), Array.Empty<Variant>());
			SkinLog.Info(label + " prewarmed=" + ((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString() + " elapsedMs=" + (Time.GetTicksMsec() - ticksMsec));
		}
		catch (Exception ex)
		{
			SkinLog.Warn(label + " GDScript prewarm failed: " + SkinLog.FormatException(ex));
		}
		return Task.CompletedTask;
	}

	private static bool HasMethod(GDScript script, string methodName)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Variant val = default(Variant);
		foreach (Dictionary scriptMethod in ((Script)script).GetScriptMethodList())
		{
			if (scriptMethod.TryGetValue(Variant.op_Implicit("name"), ref val) && ((Variant)(ref val)).AsString() == methodName)
			{
				return true;
			}
		}
		return false;
	}
}
