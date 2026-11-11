using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class ProviderManifestCompatibility
{
	private const string MigrationKey = "MOD_ERROR.MIGRATION_REQUIRED";

	private const string BranchKey = "MOD_ERROR.STEAM_BRANCH_UNSUPPORTED";

	public static MemoryStream? PrepareLegacyStream(Stream source)
	{
		if (!source.CanSeek || source.Length - source.Position > 1048576)
		{
			return null;
		}
		long position = source.Position;
		bool flag = false;
		try
		{
			JsonNode jsonNode = JsonNode.Parse(source, null);
			if (!(jsonNode is JsonObject) || !(jsonNode["id"] is JsonValue jsonValue) || !jsonValue.TryGetValue<string>(out string _) || !(jsonNode["dependencies"] is JsonArray { Count: >0 } jsonArray) || jsonArray.Any((JsonNode value) => !(value is JsonValue jsonValue2) || !jsonValue2.TryGetValue<string>(out string value3) || string.IsNullOrWhiteSpace(value3)))
			{
				return null;
			}
			jsonNode["dependencies"] = new JsonArray(((IEnumerable<JsonNode>)jsonArray).Select((Func<JsonNode, JsonNode>)((JsonNode value) => new JsonObject((JsonNodeOptions?)null)
			{
				["id"] = value.GetValue<string>(),
				["min_version"] = null
			})).ToArray());
			MemoryStream result = new MemoryStream(Encoding.UTF8.GetBytes(jsonNode.ToJsonString()), writable: false);
			flag = true;
			return result;
		}
		catch (Exception ex) when (((ex is JsonException || ex is IOException || ex is InvalidOperationException) ? 1 : 0) != 0)
		{
			return null;
		}
		finally
		{
			if (!flag)
			{
				source.Position = position;
			}
		}
	}

	public static void AcknowledgeParsedManifests(IEnumerable<Mod> mods)
	{
		foreach (Mod mod in mods)
		{
			if (mod.manifest?.id == null)
			{
				continue;
			}
			List<LocString> errors = mod.errors;
			if (errors == null || !errors.Any((LocString error) => IsError(error, "MOD_ERROR.MIGRATION_REQUIRED")))
			{
				continue;
			}
			try
			{
				IReadOnlyList<string> readOnlyList = SkinPackagePaths.FindManifests(mod.path, mod.manifest.id);
				if (readOnlyList.Count != 1)
				{
					continue;
				}
				using FileStream source = File.OpenRead(readOnlyList[0]);
				using MemoryStream memoryStream = PrepareLegacyStream(source);
				if (memoryStream == null)
				{
					continue;
				}
				string[] array = (from item in JsonNode.Parse(memoryStream, null)["dependencies"].AsArray()
					select item["id"].GetValue<string>()).ToArray();
				if (mod.manifest.dependencies != null && !mod.manifest.dependencies.Any((ModDependency dep) => dep.minVersion != null) && mod.manifest.dependencies.Select((ModDependency dep) => dep.id).SequenceEqual<string>(array, StringComparer.Ordinal))
				{
					mod.errors.RemoveAll((LocString error) => IsError(error, "MOD_ERROR.MIGRATION_REQUIRED"));
					ModLog.Info($"已核验 {mod.manifest.id} 的旧依赖清单等价迁移：{string.Join("、", array)}；" + "依赖检查保留，原清单未修改；迁移提醒已处理。");
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn("核验 " + mod.manifest.id + " 的清单迁移失败，保留原提示：" + ex.GetBaseException().Message);
			}
		}
	}

	public static void AcknowledgeManagedBranch(Mod mod, bool hasResourceBackedCosmetics)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		bool flag = !hasResourceBackedCosmetics;
		if (!flag)
		{
			ModLoadState state = mod.state;
			bool flag2 = (((int)state == 1 || (int)state == 4) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			return;
		}
		ModManifest manifest = mod.manifest;
		if (manifest == null || manifest.id == null || manifest.affectsGameplay)
		{
			return;
		}
		List<LocString> errors = mod.errors;
		if (errors == null || !errors.Any((LocString error) => IsError(error, "MOD_ERROR.STEAM_BRANCH_UNSUPPORTED")))
		{
			return;
		}
		try
		{
			if (manifest.hasPck && !File.Exists(SkinPackagePaths.Resolve(mod.path, manifest.id, ".pck")))
			{
				return;
			}
			int value = 0;
			if (manifest.hasDll)
			{
				string text = SkinPackagePaths.Resolve(mod.path, manifest.id, ".dll");
				if (!File.Exists(text))
				{
					return;
				}
				ProviderCompatibilityResult providerCompatibilityResult = ProviderAssemblyCompatibility.PrepareForCurrentGame(text);
				using (providerCompatibilityResult.Assembly)
				{
					if (providerCompatibilityResult.Report.Failure != null || providerCompatibilityResult.Report.UnresolvedReferences.Count != 0 || providerCompatibilityResult.Report.UncheckedGenericReferences != 0)
					{
						return;
					}
					value = providerCompatibilityResult.Report.Changes.Count;
				}
			}
			mod.errors.RemoveAll((LocString error) => IsError(error, "MOD_ERROR.STEAM_BRANCH_UNSUPPORTED"));
			ModLog.Info($"{manifest.id} 的工坊版本声明不含当前分支；本次已接管其皮肤资源并核验静态游戏接口（转换 {value} 项），不再把原声明作为加载错误显示。" + "工坊声明与原文件未修改；反射及实际游戏行为仍需运行验证。");
		}
		catch (Exception ex)
		{
			ModLog.Warn("核验 " + manifest.id + " 的版本兼容状态失败，保留原提示：" + ex.GetBaseException().Message);
		}
	}

	private static bool IsError(LocString error, string key)
	{
		if (error.LocTable == "main_menu_ui")
		{
			return error.LocEntryKey == key;
		}
		return false;
	}
}
