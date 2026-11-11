using System.Runtime.CompilerServices;
using System.Text;

namespace STS2SkinChanger.Ui;

internal readonly record struct MerchantPreviewFocusState(bool IsMouseHovered, bool IsControllerFocused)
{
	internal static MerchantPreviewFocusState None => new MerchantPreviewFocusState(IsMouseHovered: false, IsControllerFocused: false);

	internal bool IsFocused
	{
		get
		{
			if (!IsMouseHovered)
			{
				return IsControllerFocused;
			}
			return true;
		}
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append("IsMouseHovered = ");
		builder.Append(IsMouseHovered.ToString());
		builder.Append(", IsControllerFocused = ");
		builder.Append(IsControllerFocused.ToString());
		return true;
	}
}
