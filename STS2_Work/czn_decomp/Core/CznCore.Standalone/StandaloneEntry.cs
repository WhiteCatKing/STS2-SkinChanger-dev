using System;
using CznCore.Presentation;
using MegaCrit.Sts2.Core.Modding;

namespace CznCore.Standalone;

[ModInitializer("Init")]
public static class StandaloneEntry
{
	public const int ApiVersion = 1;

	public static bool IsInitialized { get; private set; }

	public static void Init()
	{
		if (!IsInitialized)
		{
			ModManager.OnModDetected += delegate
			{
				GameplayRelevance.Refresh();
			};
			GameplayRelevance.Refresh();
			UiSharpnessAudit.InstallHotkeyIfRequested();
			IsInitialized = true;
		}
	}

	public static void RequireApi(int expectedApi)
	{
		if (!IsInitialized)
		{
			throw new InvalidOperationException("CznCore must be enabled and initialized before dependent Mods. Restart after enabling both Mods.");
		}
		if (expectedApi != 1)
		{
			throw new InvalidOperationException($"CznCore API mismatch: Consumer requires {expectedApi}, installed Core provides {1}.");
		}
	}
}
