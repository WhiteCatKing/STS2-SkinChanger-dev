namespace STS2SkinChanger.Core;

internal sealed class WorkshopDownload
{
	public WorkshopTextKey State = WorkshopTextKey.Waiting;

	public string Error = "";

	public bool Busy;

	public WorkshopLoadReason Reason;

	public bool BrowserSubscription;

	public bool HotLoadVerified;

	public bool Removing;

	public bool Unsubscribed;
}
