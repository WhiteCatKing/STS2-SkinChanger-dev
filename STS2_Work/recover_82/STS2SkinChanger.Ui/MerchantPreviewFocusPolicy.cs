namespace STS2SkinChanger.Ui;

internal static class MerchantPreviewFocusPolicy
{
	internal static MerchantPreviewFocusState Resolve(MerchantPreviewFocusState current, MerchantPreviewFocusEvent focusEvent)
	{
		return focusEvent switch
		{
			MerchantPreviewFocusEvent.MouseEntered => current with
			{
				IsMouseHovered = true
			}, 
			MerchantPreviewFocusEvent.MouseExited => current with
			{
				IsMouseHovered = false
			}, 
			MerchantPreviewFocusEvent.ControllerFocused => current with
			{
				IsControllerFocused = true
			}, 
			MerchantPreviewFocusEvent.ControllerUnfocused => current with
			{
				IsControllerFocused = false
			}, 
			_ => current, 
		};
	}
}
