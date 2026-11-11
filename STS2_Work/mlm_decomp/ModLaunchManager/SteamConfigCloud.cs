using System.Globalization;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace ModLaunchManager;

internal sealed class SteamConfigCloud : IConfigCloud
{
	public string AccountKey { get; }

	public bool CanWrite { get; }

	private SteamConfigCloud(string accountKey, bool canWrite)
	{
		AccountKey = accountKey;
		CanWrite = canWrite;
	}

	internal static SteamConfigCloud? TryOpen(out ConfigCloudStatus unavailable)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		unavailable = ConfigCloudStatus.SteamUnavailable;
		if (!SteamInitializer.Initialized)
		{
			return null;
		}
		if (!SteamRemoteStorage.IsCloudEnabledForAccount() || !SteamRemoteStorage.IsCloudEnabledForApp())
		{
			unavailable = ConfigCloudStatus.CloudDisabled;
			return null;
		}
		ulong steamID = SteamUser.GetSteamID().m_SteamID;
		if (steamID == 0L)
		{
			return null;
		}
		bool canWrite = SteamRemoteStorage.FileExists("profile.save") || SteamRemoteStorage.FileExists("modded/profile.save");
		return new SteamConfigCloud(steamID.ToString(CultureInfo.InvariantCulture), canWrite);
	}

	public string? Read(string name)
	{
		if (!SteamRemoteStorage.FileExists(name))
		{
			return null;
		}
		int fileSize = SteamRemoteStorage.GetFileSize(name);
		if (fileSize <= 0)
		{
			return null;
		}
		byte[] array = new byte[fileSize];
		if (SteamRemoteStorage.FileRead(name, array, fileSize) != fileSize)
		{
			return null;
		}
		return Encoding.UTF8.GetString(array);
	}

	public bool Write(string name, string content)
	{
		if (!CanWrite)
		{
			return false;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(content);
		ulong num = default(ulong);
		ulong num2 = default(ulong);
		if (SteamRemoteStorage.GetQuota(ref num, ref num2) && num2 < (ulong)bytes.Length)
		{
			GD.PushWarning("[ModLaunchManager] Steam Cloud is full; " + name + " stays local.");
			return false;
		}
		return SteamRemoteStorage.FileWrite(name, bytes, bytes.Length);
	}
}
