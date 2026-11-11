using System.ComponentModel;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace St2bs.St2bsCode.Backgrounds;

[ScriptPath("res://St2bsCode/Backgrounds/CustomBg.cs")]
public class CustomBg : NCombatBackground
{
	public class MethodName : MethodName
	{
	}

	public class PropertyName : PropertyName
	{
	}

	public class SignalName : SignalName
	{
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		((NCombatBackground)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NCombatBackground)this).RestoreGodotObjectData(info);
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.0.0.7889')
