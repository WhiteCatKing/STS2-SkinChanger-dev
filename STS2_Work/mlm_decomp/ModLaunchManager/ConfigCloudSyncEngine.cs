using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ModLaunchManager;

internal static class ConfigCloudSyncEngine
{
	private sealed class SyncState
	{
		public int Version { get; set; } = 1;

		public Dictionary<string, Dictionary<string, string>> Accounts { get; set; } = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

		internal Dictionary<string, string> For(string account)
		{
			if (!Accounts.TryGetValue(account, out Dictionary<string, string> value))
			{
				value = new Dictionary<string, string>(StringComparer.Ordinal);
				Accounts[account] = value;
			}
			return value;
		}

		internal static SyncState Load(string root)
		{
			string path = Path.Combine(root, "mod_launch_cloud_sync.json");
			try
			{
				return File.Exists(path) ? (JsonSerializer.Deserialize<SyncState>(File.ReadAllText(path, Encoding.UTF8)) ?? new SyncState()) : new SyncState();
			}
			catch (Exception ex)
			{
				Log("cloud sync state unreadable, starting fresh: " + ex.Message);
				return new SyncState();
			}
		}

		internal void Save(string root)
		{
			File.WriteAllText(Path.Combine(root, "mod_launch_cloud_sync.json"), JsonSerializer.Serialize(this, WriteOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		}
	}

	internal const string StateFileName = "mod_launch_cloud_sync.json";

	internal const string BackupSuffix = ".before-cloud.json";

	private static readonly JsonNodeOptions NodeOptions = new JsonNodeOptions
	{
		PropertyNameCaseInsensitive = true
	};

	private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	internal static Action<string> Log { get; set; } = delegate
	{
	};

	internal static CloudSyncReport Run(string root, IConfigCloud cloud, IReadOnlyList<CloudSyncedFile> files, bool pushOnly = false)
	{
		List<CloudSyncFileResult> list = new List<CloudSyncFileResult>();
		SyncState syncState = SyncState.Load(root);
		Dictionary<string, string> baselines = syncState.For(cloud.AccountKey);
		bool flag = false;
		foreach (CloudSyncedFile file in files)
		{
			CloudSyncFileResult cloudSyncFileResult;
			try
			{
				cloudSyncFileResult = SyncOne(root, cloud, file, baselines, pushOnly, out var pending);
				flag = flag || pending;
			}
			catch (Exception ex)
			{
				cloudSyncFileResult = new CloudSyncFileResult(file.LocalName, CloudSyncDirection.None, ex.Message);
			}
			if (cloudSyncFileResult.Problem != null)
			{
				Log("cloud sync skipped " + file.LocalName + ": " + cloudSyncFileResult.Problem);
			}
			else if (cloudSyncFileResult.Direction != 0)
			{
				Log("cloud sync " + cloudSyncFileResult.Direction.ToString().ToLowerInvariant() + " " + file.LocalName);
			}
			list.Add(cloudSyncFileResult);
		}
		try
		{
			syncState.Save(root);
		}
		catch (Exception ex2)
		{
			Log("cloud sync state could not be saved: " + ex2.Message);
		}
		return new CloudSyncReport(list)
		{
			HasPending = flag
		};
	}

	private static CloudSyncFileResult SyncOne(string root, IConfigCloud cloud, CloudSyncedFile file, Dictionary<string, string> baselines, bool pushOnly, out bool pending)
	{
		pending = false;
		string text = Path.Combine(root, file.LocalName);
		JsonObject jsonObject = null;
		if (File.Exists(text))
		{
			jsonObject = ParseObject(File.ReadAllText(text, Encoding.UTF8));
			if (jsonObject == null)
			{
				return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.None, "the local file is not valid JSON");
			}
		}
		JsonObject jsonObject2 = ParseObject(cloud.Read(file.CloudName));
		JsonObject jsonObject3 = ((jsonObject == null) ? null : Project(jsonObject, file));
		string text2 = Hash(jsonObject3);
		string text3 = Hash(jsonObject2);
		baselines.TryGetValue(file.LocalName, out string value);
		if (text2 == null && text3 == null)
		{
			baselines.Remove(file.LocalName);
			return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.None, null);
		}
		if (text2 == text3)
		{
			baselines[file.LocalName] = text2;
			return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.None, null);
		}
		if (text2 != null && (text3 == null || text3 == value))
		{
			return Upload(cloud, file, jsonObject3, text2, baselines, out pending);
		}
		if (pushOnly)
		{
			pending = text2 != null && text2 != value;
			return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.None, null);
		}
		if (jsonObject == null || text2 == value)
		{
			JsonObject document = Adopt(jsonObject, jsonObject2, file);
			WriteLocal(text, document);
			baselines[file.LocalName] = text3;
			return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.Downloaded, null);
		}
		File.Copy(text, text + ".before-cloud.json", overwrite: true);
		JsonObject document2 = Merge(jsonObject, jsonObject2, file);
		WriteLocal(text, document2);
		JsonObject jsonObject4 = Project(document2, file);
		string text4 = Hash(jsonObject4);
		if (text4 == text3)
		{
			baselines[file.LocalName] = text3;
			return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.Merged, null);
		}
		baselines[file.LocalName] = text3;
		Upload(cloud, file, jsonObject4, text4, baselines, out pending);
		return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.Merged, null);
	}

	private static CloudSyncFileResult Upload(IConfigCloud cloud, CloudSyncedFile file, JsonObject projection, string hash, Dictionary<string, string> baselines, out bool pending)
	{
		if (!cloud.CanWrite)
		{
			pending = true;
			return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.None, null);
		}
		if (!cloud.Write(file.CloudName, projection.ToJsonString(WriteOptions)))
		{
			pending = true;
			return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.None, "the cloud refused the write");
		}
		pending = false;
		baselines[file.LocalName] = hash;
		return new CloudSyncFileResult(file.LocalName, CloudSyncDirection.Uploaded, null);
	}

	internal static JsonObject Project(JsonObject document, CloudSyncedFile file)
	{
		JsonObject jsonObject = (JsonObject)document.DeepClone();
		foreach (string localOnlyKey in file.LocalOnlyKeys)
		{
			RemoveKey(jsonObject, localOnlyKey);
		}
		return jsonObject;
	}

	internal static JsonObject Adopt(JsonObject? local, JsonObject cloudCopy, CloudSyncedFile file)
	{
		JsonObject jsonObject = Project(cloudCopy, file);
		foreach (string localOnlyKey in file.LocalOnlyKeys)
		{
			if (local != null && TryGet(local, localOnlyKey, out string actualKey, out JsonNode value))
			{
				jsonObject[actualKey] = value?.DeepClone();
			}
		}
		return jsonObject;
	}

	internal static JsonObject Merge(JsonObject local, JsonObject cloudCopy, CloudSyncedFile file)
	{
		JsonObject jsonObject = (JsonObject)local.DeepClone();
		foreach (KeyValuePair<string, JsonNode> item in cloudCopy)
		{
			item.Deconstruct(out var key, out var value);
			string text = key;
			JsonNode jsonNode = value;
			if (file.LocalOnlyKeys.Contains<string>(text, StringComparer.OrdinalIgnoreCase))
			{
				continue;
			}
			if (!TryGet(jsonObject, text, out key, out JsonNode value2))
			{
				jsonObject[text] = jsonNode?.DeepClone();
			}
			else if (value2 is JsonObject jsonObject2 && jsonNode is JsonObject jsonObject3)
			{
				foreach (KeyValuePair<string, JsonNode> item2 in jsonObject3)
				{
					item2.Deconstruct(out key, out value);
					string text2 = key;
					JsonNode jsonNode2 = value;
					if (!TryGet(jsonObject2, text2, out key, out value))
					{
						jsonObject2[text2] = jsonNode2?.DeepClone();
					}
				}
			}
			else
			{
				if (!(value2 is JsonArray jsonArray) || !(jsonNode is JsonArray jsonArray2))
				{
					continue;
				}
				HashSet<string> hashSet = new HashSet<string>(jsonArray.Select(IdentityOf), StringComparer.Ordinal);
				foreach (JsonNode item3 in jsonArray2)
				{
					if (hashSet.Add(IdentityOf(item3)))
					{
						jsonArray.Add(item3?.DeepClone());
					}
				}
			}
		}
		return jsonObject;
	}

	private static string IdentityOf(JsonNode? node)
	{
		string value3;
		if (!(node is JsonValue jsonValue))
		{
			if (node is JsonObject map && TryGet(map, "Name", out string _, out JsonNode value) && value is JsonValue jsonValue2 && jsonValue2.TryGetValue<string>(out string value2))
			{
				return "n:" + value2;
			}
		}
		else if (jsonValue.TryGetValue<string>(out value3))
		{
			return "s:" + value3.Trim().ToUpperInvariant();
		}
		return "j:" + Canonical(node);
	}

	private static JsonObject? ParseObject(string? text)
	{
		if (string.IsNullOrWhiteSpace(text) || text[0] == '\0')
		{
			return null;
		}
		try
		{
			return JsonNode.Parse(text, NodeOptions) as JsonObject;
		}
		catch (JsonException)
		{
			return null;
		}
	}

	private static void WriteLocal(string path, JsonObject document)
	{
		string text = path + ".tmp";
		File.WriteAllText(text, document.ToJsonString(WriteOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		File.Move(text, path, overwrite: true);
	}

	internal static string? Hash(JsonNode? node)
	{
		if (node == null)
		{
			return null;
		}
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Canonical(node))));
	}

	private static string Canonical(JsonNode? node)
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendCanonical(stringBuilder, node);
		return stringBuilder.ToString();
	}

	private static void AppendCanonical(StringBuilder builder, JsonNode? node)
	{
		if (!(node is JsonObject source))
		{
			if (!(node is JsonArray jsonArray))
			{
				if (node == null)
				{
					builder.Append("null");
				}
				else
				{
					builder.Append(node.ToJsonString());
				}
				return;
			}
			builder.Append('[');
			foreach (JsonNode item in jsonArray)
			{
				AppendCanonical(builder, item);
				builder.Append(',');
			}
			builder.Append(']');
			return;
		}
		builder.Append('{');
		foreach (var (text2, node2) in source.OrderBy<KeyValuePair<string, JsonNode>, string>((KeyValuePair<string, JsonNode> pair) => pair.Key, StringComparer.OrdinalIgnoreCase))
		{
			builder.Append(JsonSerializer.Serialize(text2.ToLowerInvariant())).Append(':');
			AppendCanonical(builder, node2);
			builder.Append(',');
		}
		builder.Append('}');
	}

	private static bool TryGet(JsonObject map, string key, out string? actualKey, out JsonNode? value)
	{
		foreach (var (text2, jsonNode2) in map)
		{
			if (string.Equals(text2, key, StringComparison.OrdinalIgnoreCase))
			{
				actualKey = text2;
				value = jsonNode2;
				return true;
			}
		}
		actualKey = null;
		value = null;
		return false;
	}

	private static void RemoveKey(JsonObject map, string key)
	{
		if (TryGet(map, key, out string actualKey, out JsonNode _))
		{
			map.Remove(actualKey);
		}
	}
}
