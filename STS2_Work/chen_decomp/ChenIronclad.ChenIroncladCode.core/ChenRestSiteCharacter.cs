using System.ComponentModel;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace ChenIronclad.ChenIroncladCode.core;

[GlobalClass]
[ScriptPath("res://ChenIroncladCode/core/ChenRestSiteCharacter.cs")]
public class ChenRestSiteCharacter : NRestSiteCharacter
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
		((NRestSiteCharacter)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NRestSiteCharacter)this).RestoreGodotObjectData(info);
	}
}
