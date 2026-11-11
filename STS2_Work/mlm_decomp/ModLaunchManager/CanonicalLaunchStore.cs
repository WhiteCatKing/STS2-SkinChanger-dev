using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;

namespace ModLaunchManager;

internal static class CanonicalLaunchStore
{
	internal const int CurrentVersion = 1;

	private const string FileName = "mod_launch_last_plan.json";

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	private static string? _legacyPath;

	internal static string LegacyPath => _legacyPath ?? (_legacyPath = ProjectSettings.GlobalizePath(UserDataPathProvider.GetAccountScopedBasePath("mod_launch_last_plan.json", (PlatformType?)null, (ulong?)null)));

	internal static string Path => LauncherDataPaths.AccountPlan(LegacyPath);

	internal static CanonicalLaunchSession LoadAndReconcile(IReadOnlyList<Mod> mods)
	{
		(CanonicalLaunchDocument Document, CanonicalLoadState State, string? Error) tuple = LoadDocument();
		CanonicalLaunchDocument item = tuple.Document;
		CanonicalLoadState item2 = tuple.State;
		string item3 = tuple.Error;
		List<CanonicalLiveDescriptor> live = mods.Select((Mod mod, int index) => PhysicalModIdentity.Describe(mod, index, null)).ToList();
		CanonicalReconciliation canonicalReconciliation = CanonicalLaunchReconciler.Reconcile(item.Entries, live, LaunchPlan.KeepDisabledInPlace);
		List<LaunchPlanSeed> seeds = canonicalReconciliation.Seeds.Select((CanonicalSeedDescriptor seed) => new LaunchPlanSeed(mods[seed.LiveIndex], seed.Enabled)).ToList();
		return new CanonicalLaunchSession(item, seeds, canonicalReconciliation.Dormant.Select((CanonicalLaunchEntry entry) => entry.Clone()).ToList(), canonicalReconciliation, item2, item3);
	}

	internal static CanonicalLaunchDocument BuildCandidate(LaunchPlan plan, IReadOnlyList<CanonicalLaunchEntry> previousDormant)
	{
		List<CanonicalLiveDescriptor> live = plan.Entries.Select((LaunchEntry entry, int index) => PhysicalModIdentity.Describe(entry.Mod, index, entry.Enabled)).ToList();
		CanonicalReconciliation canonicalReconciliation = CanonicalLaunchReconciler.Reconcile(previousDormant, live);
		CanonicalLaunchDocument canonicalLaunchDocument = NewDocument();
		canonicalLaunchDocument.Entries.AddRange(plan.Entries.Select(FromEntry));
		canonicalLaunchDocument.Entries.AddRange(canonicalReconciliation.Dormant.Select((CanonicalLaunchEntry entry) => entry.Clone()));
		return canonicalLaunchDocument;
	}

	internal static CanonicalLaunchDocument BuildDisabledCandidate(CanonicalLaunchDocument current, Mod mod)
	{
		CanonicalLaunchDocument canonicalLaunchDocument = Clone(current);
		DisableEntry(canonicalLaunchDocument, mod);
		return canonicalLaunchDocument;
	}

	internal static void DisableEntry(CanonicalLaunchDocument candidate, Mod mod)
	{
		CanonicalLiveDescriptor physical = PhysicalModIdentity.Describe(mod, 0, null);
		List<CanonicalLaunchEntry> list = candidate.Entries.Where((CanonicalLaunchEntry entry) => CanonicalLaunchReconciler.ExactMatch(entry, physical)).ToList();
		(((list.Count == 1) ? list[0] : candidate.Entries.Where((CanonicalLaunchEntry entry) => string.Equals(entry.Source, physical.Source, StringComparison.OrdinalIgnoreCase) && string.Equals(entry.Id, physical.Id, StringComparison.Ordinal)).SingleOrDefault()) ?? throw new InvalidOperationException($"canonical entry for {physical.Id} ({physical.Source}) is missing or ambiguous")).Enabled = false;
		candidate.WrittenUtc = DateTimeOffset.UtcNow;
		candidate.GameVersion = CurrentGameVersion();
	}

	internal static List<CanonicalLaunchEntry> RememberRemoved(IReadOnlyList<CanonicalLaunchEntry> dormant, IEnumerable<LaunchEntry> removed)
	{
		return CanonicalLaunchReconciler.MergeDormant(dormant, removed.Select(FromEntry).ToList());
	}

	internal static IReadOnlyList<CanonicalLaunchEntry> DormantRows(CanonicalLaunchDocument document, int liveCount)
	{
		return (from entry in document.Entries.Skip(Math.Clamp(liveCount, 0, document.Entries.Count))
			select entry.Clone()).ToList();
	}

	internal static void Save(CanonicalLaunchDocument document)
	{
		string path = Path;
		string? directoryName = System.IO.Path.GetDirectoryName(path);
		if (string.IsNullOrWhiteSpace(directoryName))
		{
			throw new InvalidOperationException("canonical launch-plan directory is unavailable");
		}
		Directory.CreateDirectory(directoryName);
		string text = System.IO.Path.Combine(directoryName, $".{System.IO.Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
		try
		{
			byte[] bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(JsonSerializer.Serialize(document, Options) + Environment.NewLine);
			using (FileStream fileStream = new FileStream(text, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
			{
				fileStream.Write(bytes);
				fileStream.Flush(flushToDisk: true);
			}
			if (File.Exists(path))
			{
				File.Replace(text, path, null);
			}
			else
			{
				File.Move(text, path);
			}
		}
		finally
		{
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
			catch
			{
			}
		}
	}

	private static (CanonicalLaunchDocument Document, CanonicalLoadState State, string? Error) LoadDocument()
	{
		try
		{
			string path = LauncherDataPaths.ReadAccountPlan(LegacyPath);
			if (!File.Exists(path))
			{
				return (Document: NewDocument(), State: CanonicalLoadState.Missing, Error: null);
			}
			CanonicalLaunchDocument canonicalLaunchDocument = JsonSerializer.Deserialize<CanonicalLaunchDocument>(File.ReadAllText(path, Encoding.UTF8), Options);
			if (canonicalLaunchDocument == null)
			{
				throw new InvalidDataException("the JSON root is empty");
			}
			if (canonicalLaunchDocument.Version != 1)
			{
				throw new InvalidDataException($"unsupported canonical launch-plan version {canonicalLaunchDocument.Version}");
			}
			CanonicalLaunchDocument canonicalLaunchDocument2 = NewDocument();
			canonicalLaunchDocument2.GameVersion = canonicalLaunchDocument.GameVersion;
			canonicalLaunchDocument2.WrittenUtc = canonicalLaunchDocument.WrittenUtc;
			HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
			foreach (CanonicalLaunchEntry item2 in canonicalLaunchDocument.Entries ?? new List<CanonicalLaunchEntry>())
			{
				if (TryNormalize(item2, out CanonicalLaunchEntry normalized) && normalized != null)
				{
					string value = normalized.WorkshopItemId ?? normalized.Path.ToUpperInvariant();
					string item = $"{normalized.Source.ToUpperInvariant()}\n{normalized.Id}\n{value}";
					if (hashSet.Add(item))
					{
						canonicalLaunchDocument2.Entries.Add(normalized);
					}
				}
			}
			return (Document: canonicalLaunchDocument2, State: CanonicalLoadState.Loaded, Error: null);
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not read canonical launch plan: {ex}");
			return (Document: NewDocument(), State: CanonicalLoadState.Invalid, Error: ex.Message);
		}
	}

	private static bool TryNormalize(CanonicalLaunchEntry raw, out CanonicalLaunchEntry? normalized)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Invalid comparison between Unknown and I4
		normalized = null;
		string text = raw.Id?.Trim() ?? string.Empty;
		if (text.Length == 0 || !Enum.TryParse<ModSource>(raw.Source, ignoreCase: true, out ModSource result) || !Enum.IsDefined<ModSource>(result))
		{
			return false;
		}
		string text2 = null;
		if (!string.IsNullOrWhiteSpace(raw.WorkshopItemId) && ulong.TryParse(raw.WorkshopItemId, out var result2) && result2 != 0L)
		{
			text2 = result2.ToString(CultureInfo.InvariantCulture);
		}
		string text3 = PhysicalModIdentity.NormalizePath(raw.Path);
		if (text3.Length == 0 && text2 == null)
		{
			return false;
		}
		normalized = new CanonicalLaunchEntry
		{
			Source = ((object)(ModSource)(ref result)/*cast due to .constrained prefix*/).ToString(),
			Id = text,
			Path = text3,
			WorkshopItemId = (((int)result == 2) ? text2 : null),
			Enabled = raw.Enabled
		};
		return true;
	}

	private static CanonicalLaunchEntry FromEntry(LaunchEntry entry)
	{
		CanonicalLiveDescriptor canonicalLiveDescriptor = PhysicalModIdentity.Describe(entry.Mod, 0, entry.Enabled);
		return new CanonicalLaunchEntry
		{
			Source = canonicalLiveDescriptor.Source,
			Id = canonicalLiveDescriptor.Id,
			Path = canonicalLiveDescriptor.Path,
			WorkshopItemId = canonicalLiveDescriptor.WorkshopItemId,
			Enabled = canonicalLiveDescriptor.Enabled
		};
	}

	private static CanonicalLaunchDocument NewDocument()
	{
		return new CanonicalLaunchDocument
		{
			Version = 1,
			GameVersion = CurrentGameVersion(),
			WrittenUtc = DateTimeOffset.UtcNow
		};
	}

	internal static CanonicalLaunchDocument Clone(CanonicalLaunchDocument source)
	{
		return new CanonicalLaunchDocument
		{
			Version = source.Version,
			GameVersion = source.GameVersion,
			WrittenUtc = source.WrittenUtc,
			Entries = source.Entries.Select((CanonicalLaunchEntry entry) => entry.Clone()).ToList()
		};
	}

	private static string? CurrentGameVersion()
	{
		try
		{
			ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
			return (releaseInfo != null) ? releaseInfo.Version : null;
		}
		catch
		{
			return null;
		}
	}
}
