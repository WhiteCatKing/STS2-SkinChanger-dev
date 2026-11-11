using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal sealed class GitHubSkinCatalog
{
	public const int Format = 4;

	private const int MaxCacheBytes = 4194304;

	private const int MaxEntries = 1000;

	public GitHubSkinState State { get; private set; } = GitHubSkinState.Empty;

	public GitHubSkinEntry[] Entries => State.Entries;

	public static GitHubSkinEntry FromRepo(GitHubRepoInfo info)
	{
		return new GitHubSkinEntry(info.Repo.FullName, info.Repo.Name, info.Description, info.Stars, info.PushedAt, "", GitHubRecognition.Pending, "", Array.Empty<WorkshopTarget>(), RestartRequired: true, "", "", 0L);
	}

	public void Merge(IEnumerable<GitHubSkinEntry> entries)
	{
		Dictionary<string, GitHubSkinEntry> dictionary = State.Entries.ToDictionary<GitHubSkinEntry, string>((GitHubSkinEntry entry) => entry.FullName, StringComparer.OrdinalIgnoreCase);
		foreach (GitHubSkinEntry entry in entries)
		{
			if (dictionary.TryGetValue(entry.FullName, out var value) && value.PushedAt == entry.PushedAt)
			{
				dictionary[entry.FullName] = value with
				{
					Stars = entry.Stars,
					Description = entry.Description,
					PushedAt = entry.PushedAt
				};
				continue;
			}
			dictionary[entry.FullName] = entry with
			{
				Recognition = GitHubRecognition.Pending,
				Error = "",
				Targets = Array.Empty<WorkshopTarget>(),
				ReleaseTag = "",
				AssetUrl = "",
				AssetSize = 0L,
				AssetDigest = "",
				Readme = ""
			};
		}
		Publish(dictionary.Values.OrderByDescending((GitHubSkinEntry entry) => entry.PushedAt).ToArray());
	}

	public void Publish(GitHubSkinEntry entry)
	{
		List<GitHubSkinEntry> list = State.Entries.ToList();
		int num = list.FindIndex((GitHubSkinEntry known) => known.FullName.Equals(entry.FullName, StringComparison.OrdinalIgnoreCase));
		if (num >= 0)
		{
			list[num] = entry;
		}
		else
		{
			list.Add(entry);
		}
		Publish(list.ToArray());
	}

	private void Publish(GitHubSkinEntry[] entries)
	{
		State = new GitHubSkinState(4, entries);
	}

	public void Restore(string path)
	{
		if (File.Exists(path))
		{
			if (new FileInfo(path).Length > 4194304)
			{
				throw new InvalidDataException("GitHub catalog cache too large.");
			}
			GitHubSkinState gitHubSkinState;
			try
			{
				gitHubSkinState = JsonSerializer.Deserialize<GitHubSkinState>(File.ReadAllText(path), new JsonSerializerOptions
				{
					MaxDepth = 16
				});
			}
			catch (Exception ex) when (((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
			{
				throw new InvalidDataException("Unreadable GitHub catalog cache.", ex);
			}
			Validate(gitHubSkinState);
			State = gitHubSkinState with
			{
				Entries = gitHubSkinState.Entries.Select((GitHubSkinEntry entry) => entry with
				{
					AssetDigest = GitHubSkinPolicy.NormalizeDigest(entry.AssetDigest)
				}).ToArray()
			};
		}
	}

	public void Save(string path, Action<string, Action>? cacheWriter)
	{
		string json = JsonSerializer.Serialize(State);
		if (Encoding.UTF8.GetByteCount(json) > 4194304)
		{
			json = JsonSerializer.Serialize(State with
			{
				Entries = State.Entries.Take(999).ToArray()
			});
		}
		if (cacheWriter == null)
		{
			Write();
		}
		else
		{
			cacheWriter(path, Write);
		}
		void Write()
		{
			string directoryName = Path.GetDirectoryName(path);
			if (directoryName != null && directoryName.Length > 0)
			{
				Directory.CreateDirectory(directoryName);
			}
			string text = path + ".tmp";
			File.WriteAllText(text, json);
			File.Move(text, path, overwrite: true);
		}
	}

	private static void Validate(GitHubSkinState? state)
	{
		if ((object)state == null || state.Format != 4 || state.Entries == null || state.Entries.Length > 1000 || state.Entries.Any(delegate(GitHubSkinEntry entry)
		{
			int num;
			if (!(entry == null) && GitHubSkinPolicy.TryParseFullName(entry.FullName, out GitHubRepoRef _))
			{
				string name = entry.Name;
				if (name != null && name.Length <= 128)
				{
					name = entry.Description;
					if (name != null && name.Length <= 1024 && entry.Stars >= 0)
					{
						name = entry.ReleaseTag;
						if (name != null && name.Length <= 128)
						{
							name = entry.Error;
							num = ((name == null || name.Length > 512) ? 1 : 0);
							goto IL_0089;
						}
					}
				}
			}
			num = 1;
			goto IL_0089;
			IL_0089:
			bool flag = (byte)num != 0;
			if (!flag)
			{
				long assetSize = entry.AssetSize;
				bool flag2 = ((assetSize < 0 || assetSize > 134217728) ? true : false);
				flag = flag2;
			}
			if (!flag)
			{
				string name = entry.AssetDigest;
				if (name == null || name.Length <= 64)
				{
					name = entry.Readme;
					if (name != null && name.Length <= 800)
					{
						name = entry.AssetUrl;
						if (name != null && name.Length <= 4096 && (entry.AssetUrl.Length <= 0 || (Uri.TryCreate(entry.AssetUrl, UriKind.Absolute, out Uri result) && !(result.Scheme != Uri.UriSchemeHttps) && GitHubSkinPolicy.IsAllowedHost(result.Host))) && Enum.IsDefined(entry.Recognition) && entry.Targets != null && entry.Targets.Length <= 512 && !entry.Targets.Any((WorkshopTarget target) => target == null || string.IsNullOrWhiteSpace(target.Kind) || target.Kind.Length > 32 || string.IsNullOrWhiteSpace(target.Target) || target.Target.Length > 160))
						{
							if (entry.Recognition == GitHubRecognition.Recognized)
							{
								return entry.Targets.Length == 0;
							}
							return false;
						}
					}
				}
			}
			return true;
		}))
		{
			throw new InvalidDataException("Unverified or invalid GitHub catalog cache.");
		}
	}
}
