using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ModLaunchManager;

internal static class LocalModImport
{
	[CompilerGenerated]
	private sealed class _003CValidate_003Ed__17 : IEnumerable<LocalModImportIssue>, IEnumerable, IEnumerator<LocalModImportIssue>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private LocalModImportIssue _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private LocalModManifest mod;

		public LocalModManifest _003C_003E3__mod;

		private HashSet<string> seenIds;

		public HashSet<string> _003C_003E3__seenIds;

		private string _003Cfolder_003E5__2;

		LocalModImportIssue IEnumerator<LocalModImportIssue>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CValidate_003Ed__17(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cfolder_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cfolder_003E5__2 = Path.GetDirectoryName(mod.ManifestFile);
				if (!mod.HasDll && !mod.HasPck)
				{
					_003C_003E2__current = new LocalModImportIssue(mod.DisplayName, LocalModImportProblem.NothingToLoad);
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0086;
			case 1:
				_003C_003E1__state = -1;
				goto IL_0086;
			case 2:
				_003C_003E1__state = -1;
				goto IL_00f6;
			case 3:
				_003C_003E1__state = -1;
				goto IL_0166;
			case 4:
				{
					_003C_003E1__state = -1;
					break;
				}
				IL_00f6:
				if (mod.HasPck && !File.Exists(Path.Combine(_003Cfolder_003E5__2, mod.Id + ".pck")))
				{
					_003C_003E2__current = new LocalModImportIssue(mod.DisplayName, LocalModImportProblem.MissingFile, mod.Id + ".pck");
					_003C_003E1__state = 3;
					return true;
				}
				goto IL_0166;
				IL_0086:
				if (mod.HasDll && !File.Exists(Path.Combine(_003Cfolder_003E5__2, mod.Id + ".dll")))
				{
					_003C_003E2__current = new LocalModImportIssue(mod.DisplayName, LocalModImportProblem.MissingFile, mod.Id + ".dll");
					_003C_003E1__state = 2;
					return true;
				}
				goto IL_00f6;
				IL_0166:
				if (!seenIds.Add(mod.Id))
				{
					_003C_003E2__current = new LocalModImportIssue(mod.DisplayName, LocalModImportProblem.DuplicateInSource);
					_003C_003E1__state = 4;
					return true;
				}
				break;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<LocalModImportIssue> IEnumerable<LocalModImportIssue>.GetEnumerator()
		{
			_003CValidate_003Ed__17 _003CValidate_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CValidate_003Ed__ = this;
			}
			else
			{
				_003CValidate_003Ed__ = new _003CValidate_003Ed__17(0);
			}
			_003CValidate_003Ed__.mod = _003C_003E3__mod;
			_003CValidate_003Ed__.seenIds = _003C_003E3__seenIds;
			return _003CValidate_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<LocalModImportIssue>)this).GetEnumerator();
		}
	}

	internal const long MaxTotalBytes = 4294967296L;

	internal const int MaxFiles = 50000;

	private const int MaxScanDepth = 6;

	private static readonly StringComparison PathComparison = (OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

	private static StringComparer PathComparer
	{
		get
		{
			if (!OperatingSystem.IsWindows())
			{
				return StringComparer.Ordinal;
			}
			return StringComparer.OrdinalIgnoreCase;
		}
	}

	internal static bool IsSupportedArchive(string path)
	{
		return string.Equals(Path.GetExtension(path), ".zip", StringComparison.OrdinalIgnoreCase);
	}

	internal static string CreateStagingFolder()
	{
		string text = Path.Combine(Path.GetTempPath(), "ModLaunchManager-import", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(text);
		return text;
	}

	internal static void DeleteStagingFolder(string? folder)
	{
		try
		{
			if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
			{
				Directory.Delete(folder, recursive: true);
			}
		}
		catch
		{
		}
	}

	internal static void ExtractArchive(string archivePath, string stagingFolder)
	{
		if (!IsSupportedArchive(archivePath))
		{
			throw new LocalModImportException(LocalModImportProblem.UnsupportedArchive, Path.GetExtension(archivePath));
		}
		string fullPath = Path.GetFullPath(stagingFolder);
		string value = (fullPath.EndsWith(Path.DirectorySeparatorChar) ? fullPath : (fullPath + Path.DirectorySeparatorChar));
		try
		{
			using ZipArchive zipArchive = ZipFile.OpenRead(archivePath);
			if (zipArchive.Entries.Count > 50000)
			{
				throw new LocalModImportException(LocalModImportProblem.TooLarge);
			}
			long num = 0L;
			foreach (ZipArchiveEntry entry in zipArchive.Entries)
			{
				num += Math.Max(0L, entry.Length);
				if (num > 4294967296L)
				{
					throw new LocalModImportException(LocalModImportProblem.TooLarge);
				}
			}
			long num2 = 0L;
			byte[] array = new byte[81920];
			foreach (ZipArchiveEntry entry2 in zipArchive.Entries)
			{
				string text = entry2.FullName.Replace('\\', '/');
				if (text.Length == 0 || IsMacMetadata(text))
				{
					continue;
				}
				string fullPath2 = Path.GetFullPath(Path.Combine(fullPath, text));
				if (!fullPath2.StartsWith(value, PathComparison))
				{
					throw new LocalModImportException(LocalModImportProblem.ArchiveBroken, "unsafe entry " + entry2.FullName);
				}
				if (text.EndsWith('/'))
				{
					Directory.CreateDirectory(fullPath2);
					continue;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(fullPath2));
				using Stream stream = entry2.Open();
				using FileStream fileStream = new FileStream(fullPath2, FileMode.CreateNew, FileAccess.Write);
				int num3;
				while ((num3 = stream.Read(array, 0, array.Length)) > 0)
				{
					num2 += num3;
					if (num2 > 4294967296L)
					{
						throw new LocalModImportException(LocalModImportProblem.TooLarge);
					}
					fileStream.Write(array, 0, num3);
				}
			}
		}
		catch (LocalModImportException)
		{
			throw;
		}
		catch (Exception ex2) when (((ex2 is InvalidDataException || ex2 is IOException || ex2 is UnauthorizedAccessException || ex2 is NotSupportedException) ? 1 : 0) != 0)
		{
			throw new LocalModImportException(LocalModImportProblem.ArchiveBroken, ex2.Message);
		}
	}

	internal static LocalModImportScan Scan(string root, string rootName, string modsFolder, IReadOnlyDictionary<string, LocalModInstalled> installed)
	{
		List<LocalModImportIssue> list = new List<LocalModImportIssue>();
		List<string> list2 = FindJsonFiles(root);
		Dictionary<string, List<LocalModManifest>> byFolder = new Dictionary<string, List<LocalModManifest>>(PathComparer);
		Dictionary<string, List<LocalModImportIssue>> folderIssues = new Dictionary<string, List<LocalModImportIssue>>(PathComparer);
		foreach (string item in list2)
		{
			string directoryName = Path.GetDirectoryName(item);
			LocalModManifest localModManifest = ReadManifest(item);
			if ((object)localModManifest != null)
			{
				Add(byFolder, directoryName, localModManifest);
			}
			else if (LooksLikeManifest(item))
			{
				Add(folderIssues, directoryName, new LocalModImportIssue(Path.GetFileName(directoryName), LocalModImportProblem.ManifestBroken, Path.GetFileName(item)));
			}
		}
		if (byFolder.Count == 0 && folderIssues.Count == 0)
		{
			return new LocalModImportScan(Array.Empty<LocalModImportUnit>(), new _003C_003Ez__ReadOnlySingleElementList<LocalModImportIssue>(new LocalModImportIssue(rootName, LocalModImportProblem.NoMods)));
		}
		List<string> list3 = (from folder in byFolder.Keys.Concat(folderIssues.Keys).Distinct<string>(PathComparer)
			orderby folder.Length
			select folder).ToList();
		List<string> list4 = new List<string>();
		foreach (string folder2 in list3)
		{
			if (!list4.Any((string parent) => IsInside(folder2, parent)))
			{
				list4.Add(folder2);
			}
		}
		HashSet<string> seenIds = new HashSet<string>(StringComparer.Ordinal);
		HashSet<string> taken = new HashSet<string>(PathComparer);
		List<LocalModImportUnit> list5 = new List<LocalModImportUnit>();
		foreach (string top in list4.OrderBy<string, string>((string folder) => folder, PathComparer))
		{
			List<string> source = list3.Where((string folder) => IsInside(folder, top) || PathComparer.Equals(folder, top)).ToList();
			List<LocalModManifest> list6 = source.SelectMany((string folder) => byFolder.GetValueOrDefault(folder) ?? new List<LocalModManifest>()).ToList();
			List<LocalModImportIssue> list7 = ((list6.Count > 0) ? new List<LocalModImportIssue>() : source.SelectMany((string folder) => folderIssues.GetValueOrDefault(folder) ?? new List<LocalModImportIssue>()).ToList());
			foreach (LocalModManifest item2 in list6)
			{
				list7.AddRange(Validate(item2, seenIds));
			}
			if (list7.Count > 0 || list6.Count == 0)
			{
				list.AddRange(list7);
				continue;
			}
			LocalModManifest localModManifest2 = list6.FirstOrDefault((LocalModManifest mod) => installed.ContainsKey(mod.Id));
			if ((object)localModManifest2 != null)
			{
				LocalModInstalled localModInstalled = installed[localModManifest2.Id];
				LocalModImportProblem? localModImportProblem = ((localModInstalled.LocalFolder == null) ? new LocalModImportProblem?(LocalModImportProblem.InstalledFromWorkshop) : (localModInstalled.InUse ? new LocalModImportProblem?(LocalModImportProblem.InstalledInUse) : (CanReplace(top, list6, localModInstalled, modsFolder) ? ((LocalModImportProblem?)null) : new LocalModImportProblem?(LocalModImportProblem.AlreadyInstalled))));
				if (localModImportProblem.HasValue)
				{
					LocalModImportProblem valueOrDefault = localModImportProblem.GetValueOrDefault();
					list.Add(new LocalModImportIssue(localModManifest2.DisplayName, valueOrDefault));
				}
				else
				{
					list5.Add(new LocalModImportUnit(top, Path.GetFileName(localModInstalled.LocalFolder), list6, localModInstalled));
				}
			}
			else
			{
				string name = ((!PathComparer.Equals(top, Path.GetFullPath(root))) ? Path.GetFileName(top) : ((list6.Count == 1) ? list6[0].Id : rootName));
				list5.Add(new LocalModImportUnit(top, UniqueTargetName(modsFolder, SafeFolderName(name, list6[0].Id), taken), list6));
			}
		}
		return new LocalModImportScan(list5, list);
	}

	internal static string Install(LocalModImportUnit unit, string modsFolder, string backupFolder)
	{
		List<string> list = SourceFiles(unit.Folder);
		if (list.Count > 50000 || list.Sum((string file) => new FileInfo(file).Length) > 4294967296L)
		{
			throw new LocalModImportException(LocalModImportProblem.TooLarge);
		}
		string text = unit.TargetFolder(modsFolder);
		if (unit.Replaces == null)
		{
			if (Directory.Exists(text) || File.Exists(text))
			{
				throw new LocalModImportException(LocalModImportProblem.CopyFailed, "target exists: " + text);
			}
			try
			{
				CopyFiles(unit.Folder, list, text);
				return text;
			}
			catch (Exception ex)
			{
				TryDelete(text);
				throw new LocalModImportException(LocalModImportProblem.CopyFailed, ex.Message);
			}
		}
		string text2 = Path.Combine(backupFolder, "previous-" + Guid.NewGuid().ToString("N"));
		try
		{
			CopyFiles(text, Directory.EnumerateFiles(text, "*", SearchOption.AllDirectories).ToList(), text2);
		}
		catch (Exception ex2)
		{
			throw new LocalModImportException(LocalModImportProblem.CopyFailed, ex2.Message);
		}
		try
		{
			ClearReadOnly(text);
			Directory.Delete(text, recursive: true);
			CopyFiles(unit.Folder, list, text);
			return text;
		}
		catch (Exception ex3)
		{
			try
			{
				TryDelete(text);
				CopyFiles(text2, Directory.EnumerateFiles(text2, "*", SearchOption.AllDirectories).ToList(), text, overwrite: true);
			}
			catch (Exception ex4)
			{
				throw new LocalModImportException(LocalModImportProblem.CopyFailed, $"{ex3.Message}; restoring the previous copy also failed ({ex4.Message}), it is kept at {text2}");
			}
			throw new LocalModImportException(LocalModImportProblem.CopyFailed, ex3.Message);
		}
	}

	internal static IReadOnlyList<string> TargetManifests(LocalModImportUnit unit, string modsFolder)
	{
		string target = unit.TargetFolder(modsFolder);
		return (from file in SourceFiles(unit.Folder).Where(IsJson)
			select Path.GetFullPath(Path.Combine(target, Path.GetRelativePath(unit.Folder, file)))).ToList();
	}

	private static bool CanReplace(string top, IReadOnlyList<LocalModManifest> mods, LocalModInstalled current, string modsFolder)
	{
		if (mods.Count != 1 || current.LocalFolder == null || !PathComparer.Equals(Path.GetDirectoryName(mods[0].ManifestFile), top) || !IsInside(Path.GetFullPath(current.LocalFolder), Path.GetFullPath(modsFolder)) || !Directory.Exists(current.LocalFolder))
		{
			return false;
		}
		return FindJsonFiles(current.LocalFolder).Select(ReadManifest).All((LocalModManifest manifest) => manifest == null || manifest.Id == current.Id);
	}

	private static List<string> SourceFiles(string folder)
	{
		return (from file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
			where !IsMacMetadata(Path.GetRelativePath(folder, file).Replace('\\', '/'))
			select file).ToList();
	}

	private static void CopyFiles(string fromFolder, IReadOnlyList<string> files, string target, bool overwrite = false)
	{
		Directory.CreateDirectory(target);
		foreach (string item in files.OrderBy((string file) => IsJson(file) ? 1 : 0))
		{
			string text = Path.Combine(target, Path.GetRelativePath(fromFolder, item));
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			if (overwrite && File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				FileInfo fileInfo2 = new FileInfo(item);
				if (fileInfo.Length == fileInfo2.Length && fileInfo.LastWriteTimeUtc == fileInfo2.LastWriteTimeUtc)
				{
					continue;
				}
				File.SetAttributes(text, FileAttributes.Normal);
			}
			File.Copy(item, text, overwrite);
		}
	}

	private static void ClearReadOnly(string folder)
	{
		foreach (string item in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
		{
			FileAttributes attributes = File.GetAttributes(item);
			if (attributes.HasFlag(FileAttributes.ReadOnly))
			{
				File.SetAttributes(item, attributes & ~FileAttributes.ReadOnly);
			}
		}
	}

	private static void TryDelete(string folder)
	{
		try
		{
			if (Directory.Exists(folder))
			{
				ClearReadOnly(folder);
				Directory.Delete(folder, recursive: true);
			}
		}
		catch
		{
		}
	}

	internal static bool OverlapsModsFolder(string folder, string modsFolder)
	{
		string fullPath = Path.GetFullPath(folder);
		string fullPath2 = Path.GetFullPath(modsFolder);
		if (!PathComparer.Equals(fullPath.TrimEnd(Path.DirectorySeparatorChar), fullPath2.TrimEnd(Path.DirectorySeparatorChar)) && !IsInside(fullPath, fullPath2))
		{
			return IsInside(fullPath2, fullPath);
		}
		return true;
	}

	[IteratorStateMachine(typeof(_003CValidate_003Ed__17))]
	private static IEnumerable<LocalModImportIssue> Validate(LocalModManifest mod, HashSet<string> seenIds)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CValidate_003Ed__17(-2)
		{
			_003C_003E3__mod = mod,
			_003C_003E3__seenIds = seenIds
		};
	}

	internal static LocalModManifest? ReadManifest(string file)
	{
		try
		{
			if (!(JsonNode.Parse(File.ReadAllText(file, Encoding.UTF8), null) is JsonObject jsonObject) || !(jsonObject["id"] is JsonValue jsonValue) || jsonValue.GetValueKind() != JsonValueKind.String || string.IsNullOrWhiteSpace(jsonValue.GetValue<string>()))
			{
				return null;
			}
			return new LocalModManifest(jsonValue.GetValue<string>(), Text(jsonObject, "name"), Text(jsonObject, "version"), Path.GetFullPath(file), Flag(jsonObject, "has_dll"), Flag(jsonObject, "has_pck"));
		}
		catch
		{
			return null;
		}
	}

	private static bool LooksLikeManifest(string file)
	{
		try
		{
			return JsonNode.Parse(File.ReadAllText(file, Encoding.UTF8), null) is JsonObject jsonObject && (jsonObject.ContainsKey("id") || jsonObject.ContainsKey("has_dll") || jsonObject.ContainsKey("has_pck"));
		}
		catch
		{
			return false;
		}
	}

	private static string? Text(JsonObject json, string key)
	{
		if (!(json[key] is JsonValue jsonValue) || jsonValue.GetValueKind() != JsonValueKind.String)
		{
			return null;
		}
		return jsonValue.GetValue<string>();
	}

	private static bool Flag(JsonObject json, string key)
	{
		if (json[key] is JsonValue jsonValue)
		{
			return jsonValue.GetValueKind() == JsonValueKind.True;
		}
		return false;
	}

	private static List<string> FindJsonFiles(string root)
	{
		EnumerationOptions enumerationOptions = new EnumerationOptions
		{
			RecurseSubdirectories = true,
			MaxRecursionDepth = 6,
			IgnoreInaccessible = true,
			AttributesToSkip = (FileAttributes.Hidden | FileAttributes.System | FileAttributes.ReparsePoint)
		};
		string fullRoot = Path.GetFullPath(root);
		return (from file in Directory.EnumerateFiles(fullRoot, "*.json", enumerationOptions)
			where !IsMacMetadata(Path.GetRelativePath(fullRoot, file).Replace('\\', '/'))
			select file).Select(Path.GetFullPath).Take(50000).ToList();
	}

	private static string SafeFolderName(string name, string fallback)
	{
		char[] invalid = Path.GetInvalidFileNameChars();
		string text = new string(name.Select((char ch) => (!((ReadOnlySpan<char>)invalid).Contains(ch)) ? ch : '_').ToArray()).Trim().TrimEnd('.');
		bool flag = text.Length == 0;
		if (!flag)
		{
			bool flag2 = ((text == "." || text == "..") ? true : false);
			flag = flag2;
		}
		if (!flag)
		{
			return text;
		}
		return fallback;
	}

	private static string UniqueTargetName(string modsFolder, string baseName, HashSet<string> taken)
	{
		string text = baseName;
		int num = 2;
		while (taken.Contains(text) || Directory.Exists(Path.Combine(modsFolder, text)) || File.Exists(Path.Combine(modsFolder, text)))
		{
			text = $"{baseName} ({num})";
			num++;
		}
		taken.Add(text);
		return text;
	}

	private static bool IsInside(string path, string parent)
	{
		string value = parent.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		return path.StartsWith(value, PathComparison);
	}

	private static bool IsJson(string file)
	{
		return string.Equals(Path.GetExtension(file), ".json", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsMacMetadata(string relative)
	{
		if (!relative.StartsWith("__MACOSX/", StringComparison.Ordinal) && !relative.Contains("/__MACOSX/", StringComparison.Ordinal))
		{
			return Path.GetFileName(relative).StartsWith("._", StringComparison.Ordinal);
		}
		return true;
	}

	private static void Add<T>(Dictionary<string, List<T>> map, string key, T value)
	{
		if (!map.TryGetValue(key, out List<T> value2))
		{
			value2 = (map[key] = new List<T>());
		}
		value2.Add(value);
	}
}
