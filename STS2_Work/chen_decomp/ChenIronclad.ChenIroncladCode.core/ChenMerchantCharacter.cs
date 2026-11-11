using System.ComponentModel;
using Godot;
using Godot.Bridge;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace ChenIronclad.ChenIroncladCode.core;

[GlobalClass]
[ScriptPath("res://ChenIroncladCode/core/ChenMerchantCharacter.cs")]
public class ChenMerchantCharacter : NMerchantCharacter
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
		((NMerchantCharacter)this).SaveGodotObjectData(info);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NMerchantCharacter)this).RestoreGodotObjectData(info);
	}
}
