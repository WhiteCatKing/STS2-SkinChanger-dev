using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace CznCore.Presentation;

internal static class CharacterSelectIconBackground
{
	private const string NodeName = "CznIconBackground";

	internal static void Apply(NCharacterSelectButton button, CharacterSelectPresentationProfile? profile)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		TextureRect nodeOrNull = ((Node)button).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon"));
		if (nodeOrNull == null)
		{
			return;
		}
		TextureRect val = ((Node)nodeOrNull).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("CznIconBackground"));
		string text = (button.IsLocked ? "" : (profile?.CharacterSelectIconBackgroundPath ?? ""));
		if (string.IsNullOrEmpty(text))
		{
			if (val != null)
			{
				((Node)nodeOrNull).RemoveChild((Node)(object)val);
				((Node)val).QueueFree();
			}
			return;
		}
		Texture2D val2 = ResourceLoader.Load<Texture2D>(text, (string)null, (CacheMode)1);
		if (val2 != null)
		{
			if (val == null)
			{
				val = new TextureRect
				{
					Name = StringName.op_Implicit("CznIconBackground"),
					MouseFilter = (MouseFilterEnum)2,
					ShowBehindParent = true,
					UseParentMaterial = true,
					ExpandMode = (ExpandModeEnum)1,
					StretchMode = (StretchModeEnum)0
				};
				((Node)nodeOrNull).AddChild((Node)(object)val, false, (InternalMode)0);
				((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			}
			val.Texture = val2;
		}
	}
}
