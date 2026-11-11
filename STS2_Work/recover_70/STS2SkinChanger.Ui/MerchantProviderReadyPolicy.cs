namespace STS2SkinChanger.Ui;

internal static class MerchantProviderReadyPolicy
{
	internal static MerchantProviderPostfixTiming ResolvePostfixTiming(MerchantProviderReadyTarget target)
	{
		if ((uint)target > 1u)
		{
			return MerchantProviderPostfixTiming.Immediate;
		}
		return MerchantProviderPostfixTiming.NextFrameThenSpineReady;
	}
}
