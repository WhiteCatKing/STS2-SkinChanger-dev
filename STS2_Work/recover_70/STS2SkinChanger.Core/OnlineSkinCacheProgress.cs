using System.Runtime.CompilerServices;
using System.Text;

namespace STS2SkinChanger.Core;

internal readonly record struct OnlineSkinCacheProgress(OnlineSkinCacheStage Stage, string ProviderId, ulong WorkshopItemId, ulong DownloadedBytes, ulong TotalBytes, string Detail)
{
	internal bool IsVisible => Stage != OnlineSkinCacheStage.Idle;

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append("Stage = ");
		builder.Append(Stage.ToString());
		builder.Append(", ProviderId = ");
		builder.Append((object?)ProviderId);
		builder.Append(", WorkshopItemId = ");
		builder.Append(WorkshopItemId.ToString());
		builder.Append(", DownloadedBytes = ");
		builder.Append(DownloadedBytes.ToString());
		builder.Append(", TotalBytes = ");
		builder.Append(TotalBytes.ToString());
		builder.Append(", Detail = ");
		builder.Append((object?)Detail);
		return true;
	}
}
