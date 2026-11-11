using System;
using System.IO;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class LocalModInstallTime
{
	internal static bool TryGet(Mod mod, out DateTimeOffset installTime)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		installTime = default(DateTimeOffset);
		if ((int)mod.modSource == 2 || string.IsNullOrWhiteSpace(mod.path))
		{
			return false;
		}
		try
		{
			FileSystemInfo fileSystemInfo = (Directory.Exists(mod.path) ? ((FileSystemInfo)new DirectoryInfo(mod.path)) : ((FileSystemInfo)(File.Exists(mod.path) ? new FileInfo(mod.path) : null)));
			if (fileSystemInfo == null || !fileSystemInfo.Exists)
			{
				return false;
			}
			DateTime creationTimeUtc = fileSystemInfo.CreationTimeUtc;
			if (creationTimeUtc <= DateTime.UnixEpoch || creationTimeUtc == DateTime.MaxValue)
			{
				return false;
			}
			installTime = new DateTimeOffset(DateTime.SpecifyKind(creationTimeUtc, DateTimeKind.Utc)).ToLocalTime();
			return true;
		}
		catch
		{
			return false;
		}
	}
}
