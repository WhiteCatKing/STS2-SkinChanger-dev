using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;

namespace STS2SkinChanger.Core;

internal static class MlmLaunchPlanSync
{
	private const string ManagerId = "ModLaunchManager";

	private const string PlanFileName = "mod_launch_last_plan.json";

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	public static void SyncFromSettings(string reason)
	{
		try
		{
			if (!ModManager.Mods.Any((Mod mod) => string.Equals(mod.manifest?.id, "ModLaunchManager", StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}
			SaveManager instance = SaveManager.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				SettingsSave settingsSave = instance.SettingsSave;
				obj = ((settingsSave != null) ? settingsSave.ModSettings : null);
			}
			ModSettings val = (ModSettings)obj;
			if (val == null)
			{
				return;
			}
			List<Mod> list = OrderLikeSettings(ModManager.Mods.ToList(), val.ModList);
			if (list.Count == 0)
			{
				return;
			}
			string path = Path.Combine(ProjectSettings.GlobalizePath("user://"), "ModLaunchManager");
			if (!Directory.Exists(path))
			{
				return;
			}
			string[] files = Directory.GetFiles(path, "mod_launch_last_plan.json", SearchOption.AllDirectories);
			int num = 0;
			string[] array = files;
			foreach (string text in array)
			{
				try
				{
					WritePlan(text, list, val.ModList);
					num++;
				}
				catch (Exception ex)
				{
					ModLog.Warn("同步 MLM 排序文件失败（" + text + "）：" + ex.GetBaseException().Message);
				}
			}
			if (num > 0)
			{
				ModLog.Info($"已按游戏保存的排序同步 MLM 启动计划（{num} 个文件，原因：{reason}）。");
			}
		}
		catch (Exception ex2)
		{
			ModLog.Warn("同步 MLM 启动计划失败：" + ex2.GetBaseException().Message);
		}
	}

	private static List<Mod> OrderLikeSettings(IReadOnlyList<Mod> live, IEnumerable<SettingsSaveMod> saved)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, Mod> dictionary = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
		foreach (Mod item in live)
		{
			dictionary.TryAdd(Key(item.manifest?.id, item.modSource), item);
		}
		List<Mod> list = new List<Mod>();
		foreach (SettingsSaveMod item2 in saved)
		{
			if (dictionary.TryGetValue(Key(item2.Id, item2.Source), out var value) && !list.Contains(value))
			{
				list.Add(value);
			}
		}
		foreach (Mod item3 in live)
		{
			if (!list.Contains(item3))
			{
				list.Add(item3);
			}
		}
		return list;
	}

	private static void WritePlan(string path, IReadOnlyList<Mod> ordered, IReadOnlyList<SettingsSaveMod> saved)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Invalid comparison between Unknown and I4
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		foreach (SettingsSaveMod item2 in saved)
		{
			dictionary.TryAdd(Key(item2.Id, item2.Source), item2.IsEnabled);
		}
		JsonArray jsonArray = new JsonArray((JsonNodeOptions?)null);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (Mod item3 in ordered)
		{
			string text = Key(item3.manifest?.id, item3.modSource);
			hashSet.Add(text);
			bool flag;
			if (dictionary.TryGetValue(text, out var value))
			{
				flag = value;
			}
			else
			{
				ModLoadState state = item3.state;
				bool flag2 = state - 3 <= 1;
				flag = !flag2;
			}
			bool flag3 = flag;
			string text2 = ((object)(ModSource)(ref item3.modSource)/*cast due to .constrained prefix*/).ToString();
			string text3 = WorkshopItemId(item3);
			JsonObject jsonObject = new JsonObject((JsonNodeOptions?)null)
			{
				["source"] = text2,
				["id"] = item3.manifest?.id ?? string.Empty,
				["path"] = NormalizePath(item3.path),
				["workshop_item_id"] = text3,
				["enabled"] = flag3
			};
			if (string.IsNullOrEmpty(text3))
			{
				jsonObject.Remove("workshop_item_id");
			}
			jsonArray.Add(jsonObject);
		}
		try
		{
			if (File.Exists(path) && JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8), null) is JsonObject jsonObject2 && jsonObject2["entries"] is JsonArray jsonArray2)
			{
				foreach (JsonNode item4 in jsonArray2)
				{
					if (item4 is JsonObject jsonObject3)
					{
						ModSource result;
						string item = Key(jsonObject3["id"]?.GetValue<string>(), (ModSource)(Enum.TryParse<ModSource>(jsonObject3["source"]?.GetValue<string>(), ignoreCase: true, out result) ? ((int)result) : 0));
						if (!hashSet.Contains(item))
						{
							jsonArray.Add(jsonObject3.DeepClone());
						}
					}
				}
			}
		}
		catch
		{
		}
		JsonObject jsonObject4 = new JsonObject((JsonNodeOptions?)null)
		{
			["version"] = 1,
			["game_version"] = (string?)null,
			["written_utc"] = DateTimeOffset.UtcNow.ToString("O"),
			["entries"] = jsonArray
		};
		File.WriteAllText(path, jsonObject4.ToJsonString(JsonOptions) + Environment.NewLine, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
	}

	private static string? WorkshopItemId(Mod mod)
	{
		string text = NormalizePath(mod.path);
		int num = text.IndexOf("/2868840/", StringComparison.Ordinal);
		if (num < 0)
		{
			return null;
		}
		int num2 = num + "/2868840/".Length;
		int num3 = text.IndexOf('/', num2);
		string s;
		if (num3 >= 0)
		{
			int num4 = num2;
			s = text.Substring(num4, num3 - num4);
		}
		else
		{
			string text2 = text;
			int num4 = num2;
			s = text2.Substring(num4, text2.Length - num4);
		}
		if (!ulong.TryParse(s, out var result) || result == 0L)
		{
			return null;
		}
		return result.ToString();
	}

	private static string NormalizePath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return string.Empty;
		}
		try
		{
			return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');
		}
		catch
		{
			return path.Trim().TrimEnd('\\', '/').Replace('\\', '/');
		}
	}

	private static string Key(string? id, ModSource source)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return $"{id}\n{source}";
	}
}
