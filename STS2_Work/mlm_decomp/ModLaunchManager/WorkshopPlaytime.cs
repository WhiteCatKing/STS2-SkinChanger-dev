using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using Steamworks;

namespace ModLaunchManager;

internal static class WorkshopPlaytime
{
	internal const ulong OwnItemId = 3788556097uL;

	internal const int MaxIdsPerCall = 100;

	private static readonly List<IDisposable> PendingResults = new List<IDisposable>();

	private static bool _started;

	private static (ulong ItemId, string Name, string Folder)[] LoadedMods(IEnumerable<Mod> mods)
	{
		return (from mod in mods
			where (int)mod.state == 1 && (int)mod.modSource == 2
			select (ItemId: WorkshopUpdates.ItemIdFor(mod), Name: mod.manifest?.id ?? mod.path, Folder: mod.path) into mod
			where mod.ItemId != 0L && mod.ItemId != 3788556097u
			select mod).ToArray();
	}

	internal static bool ReportsItself(string directory, ulong itemId)
	{
		try
		{
			return Directory.Exists(directory) && Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories).Any((string path) => CallsPlaytimeTracking(path) && HoldsItemId(File.ReadAllBytes(path), itemId));
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return false;
		}
	}

	private static bool CallsPlaytimeTracking(string path)
	{
		try
		{
			using FileStream peStream = File.OpenRead(path);
			using PEReader pEReader = new PEReader(peStream);
			if (!pEReader.HasMetadata)
			{
				return false;
			}
			MetadataReader metadata = pEReader.GetMetadataReader();
			return metadata.MemberReferences.Any(delegate(MemberReferenceHandle handle)
			{
				StringHandle name = metadata.GetMemberReference(handle).Name;
				return metadata.StringComparer.StartsWith(name, "Start") && metadata.GetString(name).EndsWith("PlaytimeTracking", StringComparison.Ordinal);
			});
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is BadImageFormatException || ex is InvalidOperationException) ? 1 : 0) != 0)
		{
			return false;
		}
	}

	private static bool HoldsItemId(ReadOnlySpan<byte> image, ulong itemId)
	{
		Span<byte> span = stackalloc byte[8];
		BinaryPrimitives.WriteUInt64LittleEndian(span, itemId);
		if (image.IndexOf(span) >= 0)
		{
			return true;
		}
		if (itemId <= uint.MaxValue)
		{
			Span<byte> span2 = stackalloc byte[5];
			span2[0] = 32;
			BinaryPrimitives.WriteUInt32LittleEndian(span2.Slice(1), (uint)itemId);
			if (image.IndexOf(span2) >= 0)
			{
				return true;
			}
		}
		return image.IndexOf(MemoryMarshal.AsBytes(itemId.ToString(CultureInfo.InvariantCulture).AsSpan())) >= 0;
	}

	internal static async Task Start(IEnumerable<Mod> mods)
	{
		if (_started || !WorkshopUpdates.IsAvailable)
		{
			return;
		}
		_started = true;
		try
		{
			StartCore(new ulong[1] { 3788556097uL });
			(ulong ItemId, string Name, string Folder)[] loaded = LoadedMods(mods);
			(ulong, string, string)[] array = await Task.Run(() => loaded.Where(((ulong ItemId, string Name, string Folder) mod) => ReportsItself(mod.Folder, mod.ItemId)).ToArray());
			(ulong, string, string)[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				var (value, value2, _) = array2[i];
				GD.Print($"{"[ModLaunchManager]"} {value2} reports its own Workshop playtime; {value} is left to it.");
			}
			ulong[] array3 = loaded.Select(((ulong ItemId, string Name, string Folder) mod) => mod.ItemId).Except(array.Select(((ulong ItemId, string Name, string Folder) mod) => mod.ItemId)).ToArray();
			for (int j = 0; j < array3.Length; j += 100)
			{
				StartCore(array3.Skip(j).Take(100).ToArray());
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] Workshop playtime could not be reported: " + ex.Message);
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void StartCore(ulong[] itemIds)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		PublishedFileId_t[] array = ((IEnumerable<ulong>)itemIds).Select((Func<ulong, PublishedFileId_t>)((ulong id) => new PublishedFileId_t(id))).ToArray();
		SteamAPICall_t val = SteamUGC.StartPlaytimeTracking(array, (uint)array.Length);
		if (val == SteamAPICall_t.Invalid)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} Steam did not accept playtime tracking for {itemIds.Length} Workshop item(s).");
			return;
		}
		CallResult<StartPlaytimeTrackingResult_t> callResult = null;
		callResult = CallResult<StartPlaytimeTrackingResult_t>.Create((APIDispatchDelegate<StartPlaytimeTrackingResult_t>)delegate(StartPlaytimeTrackingResult_t result, bool ioFailure)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			PendingResults.Remove((IDisposable)callResult);
			if (ioFailure || (int)result.m_eResult != 1)
			{
				GD.PushWarning($"{"[ModLaunchManager]"} Steam playtime tracking failed for {itemIds.Length} Workshop item(s): {(ioFailure ? "IO failure" : ((object)(EResult)(ref result.m_eResult)/*cast due to .constrained prefix*/).ToString())}");
			}
			else
			{
				GD.Print($"{"[ModLaunchManager]"} Steam is counting playtime for {itemIds.Length} Workshop item(s): {string.Join(", ", itemIds)}");
			}
		});
		PendingResults.Add((IDisposable)callResult);
		callResult.Set(val, (APIDispatchDelegate<StartPlaytimeTrackingResult_t>)null);
	}
}
