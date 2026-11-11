using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ModLaunchManager;

internal static class ModContentFingerprint
{
	private sealed record CacheEntry(string Signature, string Token);

	internal const string NoRuntimeFilesToken = "-";

	private static readonly Dictionary<string, CacheEntry> Cache = new Dictionary<string, CacheEntry>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

	private static readonly HashSet<string> RuntimeExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".dll", ".pck" };

	internal static bool TryCompute(string? modPath, out string token, out string? error)
	{
		return TryCompute(modPath, out token, out error, CancellationToken.None);
	}

	internal static bool TryCompute(string? modPath, out string token, out string? error, CancellationToken cancellation)
	{
		token = string.Empty;
		error = null;
		try
		{
			if (!TryList(modPath, out string root, out List<FileInfo> files, out string signature, out error))
			{
				return false;
			}
			if (files.Count == 0)
			{
				token = "-";
				lock (Cache)
				{
					Cache[root] = new CacheEntry(string.Empty, token);
				}
				return true;
			}
			if (TryGetCached(root, signature, out token))
			{
				return true;
			}
			using IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
			Span<byte> span = stackalloc byte[8];
			foreach (FileInfo item in files)
			{
				cancellation.ThrowIfCancellationRequested();
				byte[] bytes = Encoding.UTF8.GetBytes(RelativePath(root, item.FullName));
				BinaryPrimitives.WriteInt32LittleEndian(span, bytes.Length);
				incrementalHash.AppendData(span.Slice(0, 4));
				incrementalHash.AppendData(bytes);
				BinaryPrimitives.WriteInt64LittleEndian(span, item.Length);
				incrementalHash.AppendData(span);
				using FileStream fileStream = item.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
				byte[] array = new byte[131072];
				int count;
				while ((count = fileStream.Read(array, 0, array.Length)) > 0)
				{
					cancellation.ThrowIfCancellationRequested();
					incrementalHash.AppendData(array, 0, count);
				}
			}
			byte[] hashAndReset = incrementalHash.GetHashAndReset();
			token = Convert.ToBase64String(hashAndReset, 0, 12).TrimEnd('=').Replace('+', '-')
				.Replace('/', '_');
			lock (Cache)
			{
				Cache[root] = new CacheEntry(signature, token);
			}
			return true;
		}
		catch (Exception ex) when (!(ex is OperationCanceledException))
		{
			error = ex.GetType().Name + ": " + ex.Message;
			return false;
		}
	}

	internal static bool Matches(string? modPath, string? expected, out string? error)
	{
		error = null;
		if (string.IsNullOrWhiteSpace(expected))
		{
			error = "the expected content fingerprint is unknown";
			return false;
		}
		if (TryCompute(modPath, out string token, out error))
		{
			return string.Equals(token, expected, StringComparison.Ordinal);
		}
		return false;
	}

	internal static bool TryGetKnown(string? modPath, out string token)
	{
		token = string.Empty;
		try
		{
			if (!TryList(modPath, out string root, out List<FileInfo> files, out string signature, out string _))
			{
				return false;
			}
			if (files.Count == 0)
			{
				token = "-";
				return true;
			}
			return TryGetCached(root, signature, out token);
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool TryList(string? modPath, out string root, out List<FileInfo> files, out string signature, out string? error)
	{
		root = string.Empty;
		files = new List<FileInfo>();
		signature = string.Empty;
		error = null;
		if (string.IsNullOrWhiteSpace(modPath))
		{
			error = "the mod path is empty";
			return false;
		}
		string folder = Path.GetFullPath(modPath);
		if (File.Exists(folder))
		{
			FileInfo fileInfo = new FileInfo(folder);
			List<FileInfo> list;
			if (!RuntimeExtensions.Contains(fileInfo.Extension))
			{
				list = new List<FileInfo>();
			}
			else
			{
				int num = 1;
				list = new List<FileInfo>(num);
				CollectionsMarshal.SetCount(list, num);
				CollectionsMarshal.AsSpan(list)[0] = fileInfo;
			}
			files = list;
			folder = fileInfo.DirectoryName ?? folder;
		}
		else
		{
			if (!Directory.Exists(folder))
			{
				error = "the mod path does not exist";
				return false;
			}
			EnumerationOptions enumerationOptions = new EnumerationOptions
			{
				RecurseSubdirectories = true,
				IgnoreInaccessible = true,
				AttributesToSkip = FileAttributes.ReparsePoint
			};
			files = (from path in Directory.EnumerateFiles(folder, "*", enumerationOptions)
				select new FileInfo(path) into file
				where RuntimeExtensions.Contains(file.Extension)
				select file).OrderBy<FileInfo, string>((FileInfo file) => RelativePath(folder, file.FullName), StringComparer.Ordinal).ToList();
		}
		root = folder;
		signature = string.Join('|', files.Select((FileInfo file) => $"{RelativePath(folder, file.FullName)}:{file.Length}:{file.LastWriteTimeUtc.Ticks}"));
		return true;
	}

	private static bool TryGetCached(string root, string signature, out string token)
	{
		CacheEntry value;
		lock (Cache)
		{
			Cache.TryGetValue(root, out value);
		}
		bool flag = value != null && string.Equals(value.Signature, signature, StringComparison.Ordinal);
		token = (flag ? value.Token : string.Empty);
		return flag;
	}

	private static string RelativePath(string root, string file)
	{
		return Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/');
	}
}
