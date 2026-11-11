using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Data.Judgement;

public static class JudgementDivination
{
	public const int FlagIndex = 20;

	public static bool IsEnabled => ConfigFloatingWindowRunData.GetTarFlag(20);

	public static bool IsEnabledForCardDisplay
	{
		get
		{
			if (RunManager.Instance.IsInProgress)
			{
				return IsEnabled;
			}
			return false;
		}
	}
}
