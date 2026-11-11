using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ModLaunchManager;

internal sealed class SteamWorkshopNativeDownload : IDisposable
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate nint Factory([MarshalAs(UnmanagedType.LPStr)] string name, out int error);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int CreatePipe(nint self);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate int ConnectUser(nint self, int pipe);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void ReleaseUser(nint self, int pipe, int user);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool ReleasePipe(nint self, int pipe);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate nint GetBuilder(nint self, int user, int pipe);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate ulong DownloadDepot(nint self, uint app, uint depot, ulong flags, ulong manifest, ulong deltaManifest, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool GetCallback(int pipe, out CallbackMessage message);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void FreeCallback(int pipe);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private delegate bool GetResult(nint self, int pipe, ulong call, nint data, int size, int expected, [MarshalAs(UnmanagedType.I1)] out bool failed);

	private struct CallbackMessage
	{
		internal int User;

		internal int Id;

		internal nint Data;

		internal int Size;
	}

	private const int DownloadCallback = 1030003;

	private readonly nint _library;

	private readonly nint _engine;

	private readonly nint _builder;

	private readonly int _pipe;

	private readonly int _user;

	private readonly DownloadDepot _download;

	private readonly GetCallback _getCallback;

	private readonly FreeCallback _freeCallback;

	private readonly GetResult _getResult;

	private readonly Dictionary<ulong, (string Path, Action<bool, string?> Complete)> _jobs = new Dictionary<ulong, (string, Action<bool, string>)>();

	private bool _disposed;

	internal bool HasPendingDownloads => _jobs.Count != 0;

	internal static string? LoadedClientPath()
	{
		return SteamClientLibrary.FindLoaded();
	}

	internal static bool IsVerifiedClient(string path)
	{
		return SteamClientLibrary.IsVerified(path);
	}

	private static nint LoadVerifiedClient(string clientPath)
	{
		if (!IsVerifiedClient(clientPath))
		{
			throw new NotSupportedException("Unverified Steam client ABI");
		}
		return NativeLibrary.Load(clientPath);
	}

	internal SteamWorkshopNativeDownload(string clientPath)
		: this(LoadVerifiedClient(clientPath))
	{
	}

	private SteamWorkshopNativeDownload(nint library)
	{
		_library = library;
		try
		{
			_engine = Marshal.GetDelegateForFunctionPointer<Factory>(NativeLibrary.GetExport(_library, "CreateInterface"))("CLIENTENGINE_INTERFACE_VERSION005", out var error);
			if (_engine == 0 || error != 0)
			{
				throw new InvalidOperationException("Steam client engine unavailable");
			}
			_pipe = Method<CreatePipe>(_engine, 0)(_engine);
			if (_pipe == 0)
			{
				throw new InvalidOperationException("Steam pipe unavailable");
			}
			_user = Method<ConnectUser>(_engine, 3)(_engine, _pipe);
			if (_user == 0)
			{
				throw new InvalidOperationException("Steam user unavailable");
			}
			_builder = Method<GetBuilder>(_engine, 35)(_engine, _user, _pipe);
			if (_builder == 0)
			{
				throw new InvalidOperationException("Steam depot interface unavailable");
			}
			_download = Method<DownloadDepot>(_builder, 2);
			_getResult = Method<GetResult>(_engine, 33);
			_getCallback = Marshal.GetDelegateForFunctionPointer<GetCallback>(NativeLibrary.GetExport(_library, "Steam_BGetCallback"));
			_freeCallback = Marshal.GetDelegateForFunctionPointer<FreeCallback>(NativeLibrary.GetExport(_library, "Steam_FreeLastCallback"));
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	internal void Start(uint appId, ulong manifestId, string outputPath, Action<bool, string?> complete)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (HasPendingDownloads)
		{
			throw new InvalidOperationException("Steam is still processing the previous depot request");
		}
		string fullPath = Path.GetFullPath(outputPath);
		if (appId == 0 || manifestId == 0L || Encoding.UTF8.GetByteCount(fullPath) >= 480 || !Directory.Exists(fullPath) || Directory.EnumerateFileSystemEntries(fullPath).Any())
		{
			throw new ArgumentException("Download requires an empty, short, absolute staging directory");
		}
		ulong num = _download(_builder, appId, appId, 0uL, manifestId, 0uL, fullPath);
		if (num == 0L)
		{
			throw new InvalidOperationException("Steam rejected the depot request");
		}
		_jobs.Add(num, (fullPath, complete));
	}

	internal void Poll()
	{
		if (_disposed)
		{
			return;
		}
		CallbackMessage message;
		for (int i = 0; i < 128 && _getCallback(_pipe, out message); i++)
		{
			Action action = null;
			try
			{
				if (message.Id != 703 || message.Size != 16 || message.Data == 0)
				{
					continue;
				}
				ulong num = (ulong)Marshal.ReadInt64(message.Data);
				if (!_jobs.Remove(num, out var job))
				{
					continue;
				}
				int num2 = Marshal.ReadInt32(message.Data, 8);
				int num3 = Marshal.ReadInt32(message.Data, 12);
				bool success = false;
				string error = "Unexpected Steam download response";
				if (num2 == 1030003 && num3 == 516)
				{
					nint num4 = Marshal.AllocHGlobal(num3);
					try
					{
						if (_getResult(_engine, _pipe, num, num4, num3, num2, out var failed) && !failed)
						{
							int num5 = Marshal.ReadInt32(num4);
							error = $"Steam download result: {num5}";
							if (num5 == 1)
							{
								byte[] array = new byte[512];
								Marshal.Copy(num4 + 4, array, 0, array.Length);
								int num6 = Array.IndexOf(array, (byte)0);
								string text = ((num6 >= 0) ? Encoding.UTF8.GetString(array, 0, num6) : string.Empty);
								success = text.Length > 0 && string.Equals(Path.TrimEndingDirectorySeparator(Path.GetFullPath(text)), Path.TrimEndingDirectorySeparator(job.Path), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
								error = (success ? null : "Steam returned a different download directory");
							}
						}
					}
					finally
					{
						Marshal.FreeHGlobal(num4);
					}
				}
				action = delegate
				{
					job.Complete(success, error);
				};
				goto IL_01ff;
			}
			finally
			{
				_freeCallback(_pipe);
			}
			IL_01ff:
			action?.Invoke();
		}
	}

	private static T Method<T>(nint instance, int slot) where T : Delegate
	{
		return Marshal.GetDelegateForFunctionPointer<T>(Marshal.ReadIntPtr(Marshal.ReadIntPtr(instance), slot * IntPtr.Size));
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_jobs.Clear();
			if (_user != 0)
			{
				Method<ReleaseUser>(_engine, 6)(_engine, _pipe, _user);
			}
			if (_pipe != 0)
			{
				Method<ReleasePipe>(_engine, 1)(_engine, _pipe);
			}
			if (_library != 0)
			{
				NativeLibrary.Free(_library);
			}
		}
	}
}
