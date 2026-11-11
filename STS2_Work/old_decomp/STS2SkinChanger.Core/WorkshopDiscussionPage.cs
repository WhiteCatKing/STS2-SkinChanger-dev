using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed record WorkshopDiscussionPage(int Total, int Start, int PageSize, WorkshopCodePost[] Sources)
{
	public string[] Posts => Sources.Select((WorkshopCodePost p) => p.Text).ToArray();
}
