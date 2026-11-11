using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class WorkshopSubmissionScanner
{
	internal static SkinModDescriptor[] ReadDescriptors(string directory)
	{
		List<SkinModDescriptor> list = new List<SkinModDescriptor>();
		string[] array = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Take(20001).ToArray();
		if (array.Length > 20000)
		{
			throw new InvalidDataException("Too many files in the selected Mod.");
		}
		foreach (string item in array.Where((string p) => Path.GetExtension(p).Equals(".json", StringComparison.OrdinalIgnoreCase)))
		{
			if (new FileInfo(item).Length > 1048576)
			{
				continue;
			}
			JsonDocument jsonDocument;
			try
			{
				jsonDocument = JsonDocument.Parse(File.ReadAllText(item).TrimStart('\ufeff'));
			}
			catch (JsonException)
			{
				continue;
			}
			using (jsonDocument)
			{
				JsonElement rootElement = jsonDocument.RootElement;
				if (rootElement.ValueKind != JsonValueKind.Object || !rootElement.TryGetProperty("id", out var value) || value.ValueKind != JsonValueKind.String || !rootElement.TryGetProperty("has_pck", out var value2) || !rootElement.TryGetProperty("affects_gameplay", out var value3) || value3.ValueKind != JsonValueKind.False)
				{
					continue;
				}
				string @string = value.GetString();
				if (string.IsNullOrWhiteSpace(@string) || Entry.IsSelfModId(@string))
				{
					continue;
				}
				string directoryName = Path.GetDirectoryName(item);
				JsonElement value4;
				bool flag = rootElement.TryGetProperty("has_dll", out value4) && value4.ValueKind == JsonValueKind.True;
				bool flag2 = value2.ValueKind == JsonValueKind.True;
				string text = (flag2 ? SkinPackagePaths.Resolve(directoryName, @string, ".pck") : null);
				if (flag2 && rootElement.TryGetProperty("pck_name", out var value5))
				{
					string string2 = value5.GetString();
					bool flag3 = string.IsNullOrWhiteSpace(string2);
					if (!flag3)
					{
						bool flag4 = ((string2 == "." || string2 == "..") ? true : false);
						flag3 = flag4;
					}
					if (flag3 || string2.IndexOfAny(new char[3] { '/', '\\', ':' }) >= 0 || string2.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
					{
						throw new InvalidDataException("Invalid declared PCK name.");
					}
					text = Path.Combine(directoryName, string2 + ".pck");
				}
				if ((flag2 && !File.Exists(text)) || (flag && !File.Exists(SkinPackagePaths.Resolve(directoryName, @string, ".dll"))))
				{
					throw new InvalidDataException("Declared skin files are missing.");
				}
				if (flag2 || flag)
				{
					list.Add(new SkinModDescriptor(@string, (rootElement.TryGetProperty("name", out var value6) && value6.ValueKind == JsonValueKind.String) ? value6.GetString() : @string, text, AffectsGameplay: false, directoryName, flag));
				}
			}
		}
		return list.DistinctBy((SkinModDescriptor m) => (Id: m.Id, RootPath: m.RootPath)).ToArray();
	}

	internal static string[] SubmittableNames(string directory)
	{
		List<string> list = (from mod in ReadDescriptors(directory)
			select mod.Name).ToList();
		list.AddRange(from pack in CardArtPackScanner.Describe(directory)
			select pack.Name);
		return list.Where((string name) => !string.IsNullOrWhiteSpace(name)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	internal static WorkshopScanResult Scan(WorkshopLocalSubmission local, string gamePack, CardCatalogEntry[] cards, string? gameVersion, IReadOnlyDictionary<string, string> dependencies, SkinModDescriptor[]? baselines = null)
	{
		try
		{
			Dictionary<string, (long, DateTime)> before = WorkshopPackagePolicy.Snapshot(local.Directory);
			SkinModDescriptor[] mods = ReadDescriptors(local.Directory);
			List<CardArtPack> list = CardArtPackScanner.Scan(new _003C_003Ez__ReadOnlySingleElementList<string>(local.Directory)).Packs.ToList();
			list.AddRange(LooseCardPortraitPackScanner.Scan(from mod in mods
				where !string.IsNullOrWhiteSpace(mod.RootPath)
				select new LooseCardPortraitProvider(mod.Id, mod.Name, mod.RootPath), cards));
			if (mods.Length == 0 && list.Count == 0)
			{
				return new WorkshopScanResult(null, local.Name, "No cosmetic manifest.");
			}
			SkinCatalog catalog = SkinCatalog.Build(gamePack, (baselines ?? Array.Empty<SkinModDescriptor>()).Where((SkinModDescriptor b) => b.AffectsGameplay && !mods.Any((SkinModDescriptor m) => string.Equals(m.RootPath, b.RootPath, StringComparison.OrdinalIgnoreCase))).Concat(mods));
			try
			{
				if (list.Count > 0)
				{
					catalog.AttachCardArtPacks(list, cards);
				}
				catalog.FinalizeCardGroups(cards);
				WorkshopCatalogItem workshopCatalogItem = catalog.ExportWorkshopCatalog().FirstOrDefault((WorkshopCatalogItem i) => i.Id == local.Id);
				if (workshopCatalogItem == null)
				{
					return new WorkshopScanResult(null, local.Name, "No recognized skin resources.");
				}
				WorkshopPackageAssessment workshopPackageAssessment = WorkshopPackagePolicy.Assess(local.Directory, gameVersion, dependencies);
				bool flag = workshopPackageAssessment.CanInspectResources && workshopPackageAssessment.Mods.All((SkinModDescriptor m) => catalog.HasCompleteWorkshopResourceCoverage(m.Id));
				if (!WorkshopPackagePolicy.Unchanged(local.Directory, before))
				{
					return new WorkshopScanResult(null, local.Name, "Files changed during scanning.");
				}
				string providerId = catalog.ProviderForSourceId(local.Id) ?? "";
				return new WorkshopScanResult(workshopCatalogItem with
				{
					RestartRequired = !flag
				}, local.Name, "", providerId, mods.FirstOrDefault((SkinModDescriptor mod) => mod.Id.Equals(providerId, StringComparison.OrdinalIgnoreCase))?.Name ?? local.Name);
			}
			finally
			{
				if (catalog != null)
				{
					((IDisposable)catalog).Dispose();
				}
			}
		}
		catch (Exception ex)
		{
			return new WorkshopScanResult(null, local.Name, ex.GetBaseException().Message);
		}
	}
}
