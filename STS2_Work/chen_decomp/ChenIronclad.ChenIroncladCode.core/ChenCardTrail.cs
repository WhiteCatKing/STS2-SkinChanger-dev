using System.ComponentModel;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ChenIronclad.ChenIroncladCode.core;

[GlobalClass]
[ScriptPath("res://ChenIroncladCode/core/ChenCardTrail.cs")]
public class ChenCardTrail : NCardTrail
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
		((NCardTrail)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NCardTrail)this).RestoreGodotObjectData(info);
	}
}
