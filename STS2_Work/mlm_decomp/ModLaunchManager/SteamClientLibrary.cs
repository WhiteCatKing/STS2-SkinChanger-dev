using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ModLaunchManager;

internal static class SteamClientLibrary
{
	internal static string? VerifiedHash
	{
		get
		{
			Architecture processArchitecture = RuntimeInformation.ProcessArchitecture;
			bool flag = OperatingSystem.IsWindows();
			bool flag2 = OperatingSystem.IsLinux();
			bool flag3 = OperatingSystem.IsMacOS();
			if (processArchitecture != Architecture.X64)
			{
				if (processArchitecture != Architecture.Arm64)
				{
					goto IL_0047;
				}
			}
			else
			{
				if (flag)
				{
					return "CABA4826AA3501039D095AEE1843A6BFB270FB43A3AB4455B2D6733223579FEE";
				}
				if (flag2)
				{
					return "1A99F39637A505CE2DBFEC697DAFA118C2D1B8DF7DEA1412557E2D5AD0A2C7D4";
				}
			}
			if (!flag3)
			{
				goto IL_0047;
			}
			return "035722A42C74989FA38C7EFE0D735FDB38444E1102C92246652B5D913D019840";
			IL_0047:
			return null;
		}
	}

	internal static bool IsVerified(string path)
	{
		string verifiedHash = VerifiedHash;
		if (verifiedHash == null)
		{
			return false;
		}
		using FileStream source = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
		return Convert.ToHexString(SHA256.HashData(source)) == verifiedHash;
	}

	internal static string? FindLoaded()
	{
		if (VerifiedHash == null)
		{
			return null;
		}
		if (OperatingSystem.IsMacOS())
		{
			uint num = DyldImageCount();
			for (uint num2 = 0u; num2 < num; num2++)
			{
				string text = Marshal.PtrToStringUTF8(DyldGetImageName(num2));
				if (text != null && Path.GetFileName(text) == "steamclient.dylib")
				{
					return text;
				}
			}
			return null;
		}
		string name = (OperatingSystem.IsWindows() ? "steamclient64.dll" : "steamclient.so");
		StringComparison comparison = (OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		using Process process = Process.GetCurrentProcess();
		return ((IEnumerable)process.Modules).Cast<ProcessModule>().FirstOrDefault((ProcessModule module) => string.Equals(Path.GetFileName(module.FileName), name, comparison))?.FileName;
	}

	[DllImport("/usr/lib/libSystem.B.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "_dyld_image_count")]
	private static extern uint DyldImageCount();

	[DllImport("/usr/lib/libSystem.B.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "_dyld_get_image_name")]
	private static extern nint DyldGetImageName(uint index);
}
