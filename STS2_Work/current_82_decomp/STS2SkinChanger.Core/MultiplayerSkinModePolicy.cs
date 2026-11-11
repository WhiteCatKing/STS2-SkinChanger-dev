using System;

namespace STS2SkinChanger.Core;

internal static class MultiplayerSkinModePolicy
{
	internal const MultiplayerSkinMode Default = MultiplayerSkinMode.Mine;

	internal static MultiplayerSkinMode Normalize(int raw)
	{
		if (!Enum.IsDefined(typeof(MultiplayerSkinMode), raw))
		{
			return MultiplayerSkinMode.Mine;
		}
		return (MultiplayerSkinMode)raw;
	}

	internal static MultiplayerSkinMode FromLegacyToggles(bool sendChanges, bool receiveChanges)
	{
		if (receiveChanges)
		{
			if (!sendChanges)
			{
				return MultiplayerSkinMode.Shared;
			}
			return MultiplayerSkinMode.Mine;
		}
		return MultiplayerSkinMode.Vanilla;
	}

	internal static bool SendsLocalAppearance(MultiplayerSkinMode mode, bool isMultiplayer)
	{
		return isMultiplayer;
	}

	internal static bool AppliesRemoteAppearance(MultiplayerSkinMode mode, bool isMultiplayer)
	{
		if (isMultiplayer)
		{
			return mode == MultiplayerSkinMode.Shared;
		}
		return false;
	}

	internal static bool UsesVanillaForRemotePlayers(MultiplayerSkinMode mode)
	{
		return mode == MultiplayerSkinMode.Vanilla;
	}
}
