using System;
using STS2SkinChanger.Core;

namespace STS2SkinChanger;

public static class ProviderSettingsApi
{
	public static int Refresh(Type providerType)
	{
		return ManagedMerchantSettingsBridge.RequestRefresh(providerType.Assembly);
	}
}
