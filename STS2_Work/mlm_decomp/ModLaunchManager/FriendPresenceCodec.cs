using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal static class FriendPresenceCodec
{
	internal const int ProtocolVersion = 1;

	internal const int ChunkSize = 240;

	internal const int MaxChunks = 15;

	internal const int MaxMods = 512;

	internal const int MaxDecodedBytes = 1048576;

	internal const string ManagerScreenKey = "mlm_screen";

	internal const string ManagerScreenValue = "manager";

	internal const string HeaderKey = "mlm_mods";

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
	};

	internal static string ChunkKey(int index)
	{
		return $"mlm_m{index:x}";
	}

	internal static bool IsManagerScreen(string? value)
	{
		return string.Equals(value, "manager", StringComparison.Ordinal);
	}

	internal static bool TryEncode(FriendModList list, out FriendPresencePacket? packet, out FriendSyncError? error)
	{
		packet = null;
		error = null;
		try
		{
			if (!Validate(list, out error))
			{
				return false;
			}
			byte[] array = JsonSerializer.SerializeToUtf8Bytes(list, Options);
			using MemoryStream memoryStream = new MemoryStream();
			using (BrotliStream brotliStream = new BrotliStream(memoryStream, CompressionLevel.Optimal, leaveOpen: true))
			{
				brotliStream.Write(array);
			}
			byte[] array2 = memoryStream.ToArray();
			string text = Convert.ToBase64String(array2);
			int num = Math.Max(1, (text.Length + 240 - 1) / 240);
			if (num > 15)
			{
				error = FriendSyncError.Create("friendSync.error.listTooLarge");
				return false;
			}
			List<string> list2 = new List<string>(num);
			for (int i = 0; i < text.Length; i += 240)
			{
				list2.Add(text.Substring(i, Math.Min(240, text.Length - i)));
			}
			string value = Convert.ToHexString(SHA256.HashData(array2)).Substring(0, 16).ToLowerInvariant();
			packet = new FriendPresencePacket($"{1}|{list2.Count}|{value}|{list.Mods.Count}", list2);
			return true;
		}
		catch
		{
			error = FriendSyncError.Create("friendSync.error.encodeFailed");
			return false;
		}
	}

	internal static bool TryGetChunkCount(string header, out int chunkCount, out FriendSyncError? error)
	{
		chunkCount = 0;
		error = null;
		string[] array = header.Split('|');
		if (array.Length != 4 || !int.TryParse(array[0], out var result))
		{
			error = FriendSyncError.Create("friendSync.error.headerMalformed");
			return false;
		}
		if (result != 1)
		{
			error = FriendSyncError.Create("friendSync.error.protocolUnsupported", ("version", result));
			return false;
		}
		bool flag = !int.TryParse(array[1], out chunkCount);
		if (!flag)
		{
			int num = chunkCount;
			bool flag2 = ((num < 1 || num > 15) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			string text = array[2];
			FriendSyncError friendSyncError = ((text == "overflow") ? FriendSyncError.Create("friendSync.error.listTooLarge") : ((!(text == "invalid")) ? FriendSyncError.Create("friendSync.error.chunkCountInvalid") : FriendSyncError.Create("friendSync.error.invalidPublishedList")));
			error = friendSyncError;
			return false;
		}
		return true;
	}

	internal static bool TryDecode(string header, IReadOnlyList<string> chunks, out FriendModList? list, out FriendSyncError? error)
	{
		list = null;
		error = null;
		try
		{
			if (!TryGetChunkCount(header, out var chunkCount, out error))
			{
				return false;
			}
			if (chunks.Count != chunkCount || chunks.Any(string.IsNullOrEmpty))
			{
				error = FriendSyncError.Create("friendSync.error.listIncomplete");
				return false;
			}
			string[] array = header.Split('|');
			byte[] array2 = Convert.FromBase64String(string.Concat(chunks));
			string s = Convert.ToHexString(SHA256.HashData(array2)).Substring(0, 16).ToLowerInvariant();
			if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(s), Encoding.ASCII.GetBytes(array[2].ToLowerInvariant())))
			{
				error = FriendSyncError.Create("friendSync.error.listChanged");
				return false;
			}
			using MemoryStream stream = new MemoryStream(array2);
			using BrotliStream brotliStream = new BrotliStream(stream, CompressionMode.Decompress);
			using MemoryStream memoryStream = new MemoryStream();
			byte[] array3 = new byte[8192];
			while (true)
			{
				int num = brotliStream.Read(array3);
				if (num == 0)
				{
					break;
				}
				if (memoryStream.Length + num > 1048576)
				{
					error = FriendSyncError.Create("friendSync.error.safetyLimit");
					return false;
				}
				memoryStream.Write(array3, 0, num);
			}
			list = JsonSerializer.Deserialize<FriendModList>(memoryStream.ToArray(), Options);
			if (list == null || !Validate(list, out error))
			{
				list = null;
				return false;
			}
			if (!int.TryParse(array[3], out var result) || result != list.Mods.Count)
			{
				error = FriendSyncError.Create("friendSync.error.countMismatch");
				list = null;
				return false;
			}
			return true;
		}
		catch
		{
			error = FriendSyncError.Create("friendSync.error.decodeFailed");
			list = null;
			return false;
		}
	}

	internal static bool TryValidate(FriendModList list, out FriendSyncError? error)
	{
		return Validate(list, out error);
	}

	private static bool Validate(FriendModList list, out FriendSyncError? error)
	{
		error = null;
		if (list.ProtocolVersion != 1)
		{
			error = FriendSyncError.Create("friendSync.error.protocolUnsupported", ("version", list.ProtocolVersion));
			return false;
		}
		if (list.Mods == null)
		{
			error = FriendSyncError.Create("friendSync.error.collectionMissing");
			return false;
		}
		if (list.Mods.Count > 512)
		{
			error = FriendSyncError.Create("friendSync.error.tooManyMods", ("limit", 512));
			return false;
		}
		list.GameVersion = list.GameVersion?.Trim() ?? string.Empty;
		list.ManagerVersion = list.ManagerVersion?.Trim() ?? string.Empty;
		if (list.GameVersion.Length > 64 || list.ManagerVersion.Length > 64)
		{
			error = FriendSyncError.Create("friendSync.error.versionMetadataTooLong");
			return false;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (FriendModEntry mod in list.Mods)
		{
			if (mod == null)
			{
				error = FriendSyncError.Create("friendSync.error.emptyEntry");
				return false;
			}
			mod.Id = mod.Id?.Trim() ?? string.Empty;
			mod.Version = mod.Version?.Trim() ?? string.Empty;
			mod.Source = mod.Source?.Trim() ?? string.Empty;
			mod.Name = SanitizeDisplayName(mod.Name);
			mod.ContentHash = mod.ContentHash?.Trim() ?? string.Empty;
			if (mod.Id.Length > 256 || (mod.Name?.Length ?? 0) > 256 || mod.Version.Length > 64 || mod.Source.Length > 64 || (mod.Dependencies != null && (mod.Dependencies.Length > 512 || mod.Dependencies.Any((string id) => string.IsNullOrWhiteSpace(id) || id.Length > 256))))
			{
				error = FriendSyncError.Create("friendSync.error.metadataTooLong", ("id", mod.Id));
				return false;
			}
			if (mod.ContentHash.Length > 64)
			{
				error = FriendSyncError.Create("friendSync.error.fingerprintTooLong", ("id", mod.Id));
				return false;
			}
			if (mod.Id.Length == 0 || !hashSet.Add(mod.Id))
			{
				error = ((mod.Id.Length == 0) ? FriendSyncError.Create("friendSync.error.emptyModId") : FriendSyncError.Create("friendSync.error.duplicateModId", ("id", mod.Id)));
				return false;
			}
		}
		return true;
	}

	private static string? SanitizeDisplayName(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}
		string text = new string(value.Where((char character) => !char.IsControl(character)).ToArray());
		text = text.Trim();
		if (text.Length != 0)
		{
			return text;
		}
		return null;
	}
}
