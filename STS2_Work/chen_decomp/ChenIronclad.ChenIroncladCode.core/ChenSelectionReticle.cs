using System.ComponentModel;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ChenIronclad.ChenIroncladCode.core;

[GlobalClass]
[ScriptPath("res://ChenIroncladCode/core/ChenSelectionReticle.cs")]
public class ChenSelectionReticle : NSelectionReticle
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
		((NSelectionReticle)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NSelectionReticle)this).RestoreGodotObjectData(info);
	}
}
