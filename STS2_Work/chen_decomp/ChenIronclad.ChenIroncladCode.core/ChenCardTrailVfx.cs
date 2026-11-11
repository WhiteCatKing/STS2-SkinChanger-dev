using System.ComponentModel;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace ChenIronclad.ChenIroncladCode.core;

[GlobalClass]
[ScriptPath("res://ChenIroncladCode/core/ChenCardTrailVfx.cs")]
public class ChenCardTrailVfx : NCardTrailVfx
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
		((NCardTrailVfx)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NCardTrailVfx)this).RestoreGodotObjectData(info);
	}
}
