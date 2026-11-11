using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal sealed class GitHubInstallLedger
{
	public const int Format = 2;

	private const int MaxEntries = 512;

	private const int MaxPending = 64;

	private const int MaxDeletes = 64;

	private const int MaxBytes = 1048576;

	private readonly Dictionary<string, GitHubInstall> _installs = new Dictionary<string, GitHubInstall>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, GitHubPendingOp> _pending = new Dictionary<string, GitHubPendingOp>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string[]> _deletes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyDictionary<string, GitHubInstall> Installs => _installs;

	public IReadOnlyDictionary<string, GitHubPendingOp> Pending => _pending;

	public int Count => _installs.Count;

	public IReadOnlyDictionary<string, string[]> Deletes => _deletes;

	private int TotalDeletes => _deletes.Values.Sum((string[] folders) => folders.Length);

	public GitHubInstall? Find(string? fullName)
	{
		if (fullName != null)
		{
			return _installs.GetValueOrDefault(fullName);
		}
		return null;
	}

	public bool Record(string fullName, GitHubInstall install)
	{
		install = Normalized(install);
		if (!GitHubSkinPolicy.TryParseFullName(fullName, out GitHubRepoRef _) || !Valid(install) || (_installs.Count >= 512 && !_installs.ContainsKey(fullName)))
		{
			return false;
		}
		_installs[fullName] = install;
		return true;
	}

	public bool Remove(string fullName)
	{
		return _installs.Remove(fullName);
	}

	public GitHubPendingOp? PendingFor(string? fullName)
	{
		if (fullName != null)
		{
			return _pending.GetValueOrDefault(fullName);
		}
		return null;
	}

	public bool HasDelete(string? fullName)
	{
		if (fullName != null)
		{
			return _deletes.ContainsKey(fullName);
		}
		return false;
	}

	public bool SetDelete(string fullName, string folder)
	{
		if (!GitHubSkinPolicy.TryParseFullName(fullName, out GitHubRepoRef _) || !ValidDelete(folder))
		{
			return false;
		}
		string[] value;
		string[] array = (_deletes.TryGetValue(fullName, out value) ? value : Array.Empty<string>());
		if (array.Contains<string>(folder, StringComparer.OrdinalIgnoreCase))
		{
			return true;
		}
		if (TotalDeletes + 1 > 64)
		{
			return false;
		}
		Dictionary<string, string[]> deletes = _deletes;
		string[] array2 = array;
		int num = 0;
		string[] array3 = new string[1 + array2.Length];
		ReadOnlySpan<string> readOnlySpan = new ReadOnlySpan<string>(array2);
		readOnlySpan.CopyTo(new Span<string>(array3).Slice(num, readOnlySpan.Length));
		num += readOnlySpan.Length;
		array3[num] = folder;
		deletes[fullName] = array3;
		return true;
	}

	public bool ClearDelete(string fullName, string folder)
	{
		if (!_deletes.TryGetValue(fullName, out string[] value))
		{
			return false;
		}
		string[] array = value.Where((string name) => !name.Equals(folder, StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length == value.Length)
		{
			return false;
		}
		if (array.Length == 0)
		{
			_deletes.Remove(fullName);
		}
		else
		{
			_deletes[fullName] = array;
		}
		return true;
	}

	public bool ClearDeletes(string fullName)
	{
		return _deletes.Remove(fullName);
	}

	public GitHubPendingOp? ClearPending(string fullName)
	{
		if (!_pending.Remove(fullName, out GitHubPendingOp value))
		{
			return null;
		}
		return value;
	}

	public void Restore(string path)
	{
		if (!File.Exists(path))
		{
			return;
		}
		if (new FileInfo(path).Length > 1048576)
		{
			throw new InvalidDataException("GitHub install ledger too large.");
		}
		GitHubInstallLedgerState gitHubInstallLedgerState;
		try
		{
			gitHubInstallLedgerState = JsonSerializer.Deserialize<GitHubInstallLedgerState>(File.ReadAllText(path), new JsonSerializerOptions
			{
				MaxDepth = 8
			});
		}
		catch (Exception ex) when (((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			throw new InvalidDataException("Unreadable GitHub install ledger.", ex);
		}
		if ((object)gitHubInstallLedgerState != null && gitHubInstallLedgerState.Format == 2 && gitHubInstallLedgerState.Installs != null && gitHubInstallLedgerState.Installs.Count <= 512)
		{
			Dictionary<string, GitHubPendingOp> pending = gitHubInstallLedgerState.Pending;
			if (pending == null || pending.Count <= 64)
			{
				Dictionary<string, string[]> deletes = gitHubInstallLedgerState.Deletes;
				if (deletes == null || deletes.Count <= 64)
				{
					_installs.Clear();
					string key;
					GitHubRepoRef repo;
					foreach (KeyValuePair<string, GitHubInstall> install2 in gitHubInstallLedgerState.Installs)
					{
						install2.Deconstruct(out key, out var value);
						string text = key;
						GitHubInstall install = value;
						if (GitHubSkinPolicy.TryParseFullName(text, out repo) && Valid(install))
						{
							_installs[text] = Normalized(install);
						}
					}
					_pending.Clear();
					foreach (KeyValuePair<string, GitHubPendingOp> item in gitHubInstallLedgerState.Pending ?? new Dictionary<string, GitHubPendingOp>())
					{
						item.Deconstruct(out key, out var value2);
						string text2 = key;
						GitHubPendingOp gitHubPendingOp = value2;
						if (GitHubSkinPolicy.TryParseFullName(text2, out repo) && ValidPending(gitHubPendingOp) && (gitHubPendingOp.Kind != GitHubModsOperationKind.Replace || _installs.GetValueOrDefault(text2)?.Folder == gitHubPendingOp.Folder))
						{
							_pending[text2] = gitHubPendingOp;
						}
					}
					_deletes.Clear();
					{
						foreach (KeyValuePair<string, string[]> item2 in gitHubInstallLedgerState.Deletes ?? new Dictionary<string, string[]>())
						{
							item2.Deconstruct(out key, out var value3);
							string text3 = key;
							string[] array = value3;
							if (GitHubSkinPolicy.TryParseFullName(text3, out repo) && array != null)
							{
								string[] array2 = array.Where(ValidDelete).ToArray();
								if (array2.Length != 0 && TotalDeletes + array2.Length <= 64)
								{
									_deletes[text3] = array2;
								}
							}
						}
						return;
					}
				}
			}
		}
		throw new InvalidDataException("Unverified or invalid GitHub install ledger.");
	}

	public void Save(string path, Action<string, Action>? writer)
	{
		string json = JsonSerializer.Serialize(new GitHubInstallLedgerState(2, new Dictionary<string, GitHubInstall>(_installs), new Dictionary<string, GitHubPendingOp>(_pending), new Dictionary<string, string[]>(_deletes)));
		if (writer == null)
		{
			Write();
		}
		else
		{
			writer(path, Write);
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

	private static bool Valid(GitHubInstall? install)
	{
		bool flag = (object)install != null && install.Folder != null && install.PackageId != null && install.Version != null && GitHubInstallPolicy.IsSafeToken(install.Folder) && (install.PackageId.Length == 0 || GitHubInstallPolicy.IsSafeToken(install.PackageId)) && install.Version.Length <= 64 && !install.Version.Any(char.IsControl);
		if (flag)
		{
			string digest = install.Digest;
			bool flag2 = ((digest == null || digest.Length <= 64) ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private static GitHubInstall Normalized(GitHubInstall install)
	{
		return install with
		{
			Digest = GitHubSkinPolicy.NormalizeDigest(install.Digest)
		};
	}

	private static bool ValidDelete(string? folder)
	{
		return GitHubInstallPolicy.IsSafeToken(folder);
	}

	private static bool ValidPending(GitHubPendingOp? operation)
	{
		bool flag = (object)operation != null && operation.Folder != null && operation.Staging != null;
		if (flag)
		{
			GitHubModsOperationKind kind = operation.Kind;
			bool flag2 = (uint)kind <= 1u;
			flag = flag2;
		}
		if (flag && GitHubModsPolicy.IsModsFolderName(operation.Folder) && operation.Folder[0] != '.')
		{
			if (operation.Kind != 0)
			{
				string staging = operation.Staging;
				if (staging != null)
				{
					int length = staging.Length;
					if (length > 0 && length <= 512 && Path.IsPathRooted(operation.Staging))
					{
						return !operation.Staging.Any(char.IsControl);
					}
				}
				return false;
			}
			return operation.Staging.Length == 0;
		}
		return false;
	}
}
