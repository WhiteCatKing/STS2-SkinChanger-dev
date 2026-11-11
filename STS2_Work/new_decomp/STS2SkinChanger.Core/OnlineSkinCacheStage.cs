namespace STS2SkinChanger.Core;

internal enum OnlineSkinCacheStage
{
	Idle,
	Preparing,
	WaitingForReady,
	CheckingWorkshop,
	Downloading,
	Verifying,
	Applying,
	Complete,
	Failed
}
