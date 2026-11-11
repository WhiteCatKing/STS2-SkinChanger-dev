using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;

namespace ModLaunchManager;

internal static class ModOptionApplier
{
	private const string TempSuffix = ".mlmtmp";

	private const string BackupSuffix = ".mlmbak";

	private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	internal static ModOptionApplyReport ApplyAll(LaunchPlan plan, ModOptionDocument document)
	{
		ModOptionApplyReport modOptionApplyReport = new ModOptionApplyReport();
		KeyValuePair<string, ModOptionEntry>[] array = document.Mods.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, ModOptionEntry> keyValuePair = array[i];
			var (id, modOptionEntry2) = (KeyValuePair<string, ModOptionEntry>)(ref keyValuePair);
			if (modOptionEntry2.IsEmpty)
			{
				continue;
			}
			List<LaunchEntry> list = plan.Entries.Where((LaunchEntry entry) => string.Equals(entry.Id, id, StringComparison.OrdinalIgnoreCase)).ToList();
			if (list.Count != 0)
			{
				LaunchEntry launchEntry = list.FirstOrDefault((LaunchEntry entry) => entry.Enabled) ?? list[0];
				try
				{
					ApplyToMod(launchEntry, modOptionEntry2, modOptionApplyReport);
				}
				catch (Exception ex)
				{
					GD.PushWarning($"{"[ModLaunchManager]"} could not write launch options for {launchEntry.Id}: {ex}");
					modOptionApplyReport.Failures.Add(MlmLocManager.Get("options.apply.writeFailed", ("name", launchEntry.DisplayName), ("error", ex.Message)));
				}
				if (modOptionEntry2.IsEmpty)
				{
					document.Mods.Remove(id);
					modOptionApplyReport.StoreChanged = true;
				}
			}
		}
		return modOptionApplyReport;
	}

	private static void ApplyToMod(LaunchEntry entry, ModOptionEntry stored, ModOptionApplyReport report)
	{
		string text = ModManifestReader.FindManifestFile(entry.Mod);
		if (text == null)
		{
			report.Failures.Add(MlmLocManager.Get("options.apply.manifestMissing", ("name", entry.DisplayName)));
			return;
		}
		if (!(JsonNode.Parse(File.ReadAllText(text, Encoding.UTF8), null) is JsonObject jsonObject))
		{
			report.Failures.Add(MlmLocManager.Get("options.apply.manifestInvalid", ("name", entry.DisplayName)));
			return;
		}
		ModOptionCatalog modOptionCatalog = ModOptionCatalog.For(entry.Mod);
		bool flag = false;
		int num = 0;
		KeyValuePair<string, JsonNode>[] array = stored.Values.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, JsonNode> keyValuePair = array[i];
			var (text3, jsonNode2) = (KeyValuePair<string, JsonNode>)(ref keyValuePair);
			if (modOptionCatalog.Find(text3) != null)
			{
				if (!stored.Baseline.ContainsKey(text3))
				{
					stored.Baseline[text3] = ((!jsonObject.ContainsKey(text3)) ? null : jsonObject[text3]?.DeepClone());
					report.StoreChanged = true;
				}
				if (!JsonNode.DeepEquals(jsonObject[text3], jsonNode2))
				{
					jsonObject[text3] = jsonNode2?.DeepClone();
					flag = true;
					num++;
				}
			}
		}
		string[] array2 = stored.Baseline.Keys.Where((string candidate) => !stored.Values.ContainsKey(candidate)).ToArray();
		foreach (string text4 in array2)
		{
			JsonNode jsonNode3 = stored.Baseline[text4];
			if (jsonNode3 == null)
			{
				if (jsonObject.Remove(text4))
				{
					flag = true;
					num++;
				}
			}
			else if (jsonObject.ContainsKey(text4) && !JsonNode.DeepEquals(jsonObject[text4], jsonNode3))
			{
				jsonObject[text4] = jsonNode3.DeepClone();
				flag = true;
				num++;
			}
			stored.Baseline.Remove(text4);
			report.StoreChanged = true;
		}
		if (flag)
		{
			WriteManifest(text, jsonObject);
			ModOptionCatalog.Invalidate(text);
			report.ModsChanged++;
			report.KeysWritten += num;
			GD.Print($"{"[ModLaunchManager]"} wrote {num} launch option(s) into {Path.GetFileName(text)} for {entry.Id}.");
		}
	}

	private static void WriteManifest(string file, JsonObject root)
	{
		string text = file + ".mlmbak";
		if (!File.Exists(text))
		{
			File.Copy(file, text);
		}
		string text2 = file + ".mlmtmp";
		File.WriteAllText(text2, root.ToJsonString(WriteOptions) + Environment.NewLine, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		File.Move(text2, file, overwrite: true);
	}
}
