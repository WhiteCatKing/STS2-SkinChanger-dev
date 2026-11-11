using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal sealed record WorkshopPackageAssessment(SkinModDescriptor[] Mods, WorkshopLoadReason Reason)
{
	public bool CanInspectResources
	{
		get
		{
			if (Reason == WorkshopLoadReason.None)
			{
				return Mods.Length != 0;
			}
			return false;
		}
	}
}
