using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace STS2SkinChanger.Catalog;

internal static class CardArtPackScanner
{
	[CompilerGenerated]
	private sealed class _003CEnumeratePackFiles_003Ed__4 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		private Stack<string> _003Cpending_003E5__2;

		private IEnumerator<string> _003C_003E7__wrap2;

		string IEnumerator<string>.Current
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
		public _003CEnumeratePackFiles_003Ed__4(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Cpending_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					goto IL_014c;
				}
				_003C_003E1__state = -1;
				_003Cpending_003E5__2 = new Stack<string>();
				_003Cpending_003E5__2.Push(root);
				goto IL_0166;
				IL_014c:
				if (_003C_003E7__wrap2.MoveNext())
				{
					string current = _003C_003E7__wrap2.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				goto IL_0166;
				IL_0166:
				IEnumerable<string> sequence2;
				while (true)
				{
					if (_003Cpending_003E5__2.Count > 0)
					{
						string path = _003Cpending_003E5__2.Pop();
						IEnumerable<string> sequence;
						try
						{
							sequence = Directory.EnumerateDirectories(path);
						}
						catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
						{
							continue;
						}
						foreach (string item in TakeAll(sequence))
						{
							_003Cpending_003E5__2.Push(item);
						}
						try
						{
							sequence2 = Directory.EnumerateFiles(path, "*.cardartpack.json");
						}
						catch (Exception ex2) when (((ex2 is IOException || ex2 is UnauthorizedAccessException) ? 1 : 0) != 0)
						{
							continue;
						}
						break;
					}
					return false;
				}
				_003C_003E7__wrap2 = TakeAll(sequence2).GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_014c;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CEnumeratePackFiles_003Ed__4 _003CEnumeratePackFiles_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumeratePackFiles_003Ed__ = this;
			}
			else
			{
				_003CEnumeratePackFiles_003Ed__ = new _003CEnumeratePackFiles_003Ed__4(0);
			}
			_003CEnumeratePackFiles_003Ed__.root = _003C_003E3__root;
			return _003CEnumeratePackFiles_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	public static CardArtPackScanResult Scan(IEnumerable<string> scanRoots)
	{
		List<CardArtPack> list = new List<CardArtPack>();
		List<string> list2 = new List<string>();
		foreach (string item in NormalizeRoots(scanRoots))
		{
			foreach (string item2 in EnumeratePackFiles(item))
			{
				try
				{
					CardArtPack cardArtPack = ReadPack(item2, list2);
					if (cardArtPack != null)
					{
						list.Add(cardArtPack);
					}
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is InvalidDataException || ex is FormatException || ex is ArgumentException) ? 1 : 0) != 0)
				{
					list2.Add("无法读取卡图包 " + item2 + "：" + ex.GetBaseException().Message);
				}
			}
		}
		return new CardArtPackScanResult(PreferHighResolution(list), list2);
	}

	public static IReadOnlyList<(string Id, string Name)> Describe(string root)
	{
		IReadOnlyList<string> readOnlyList = NormalizeRoots(new _003C_003Ez__ReadOnlySingleElementList<string>(root));
		if (readOnlyList.Count == 0)
		{
			return Array.Empty<(string, string)>();
		}
		List<(string, string)> list = new List<(string, string)>();
		foreach (string item3 in EnumeratePackFiles(readOnlyList[0]))
		{
			try
			{
				if (HasBundleHeader(item3))
				{
					(string? Id, string? Name) tuple = ReadMetadata(Path.GetDirectoryName(item3));
					string item = tuple.Id;
					string item2 = tuple.Name;
					string id = CardArtPackScanPolicy.NormalizePackId(item, Path.GetFileName(item3));
					if (!list.Any<(string, string)>(((string Id, string Name) pack) => pack.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
					{
						list.Add((id, item2 ?? id));
					}
				}
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? 1 : 0) != 0)
			{
			}
		}
		return list;
	}

	private static IReadOnlyList<CardArtPack> PreferHighResolution(IReadOnlyList<CardArtPack> packs)
	{
		Dictionary<string, CardArtPack> best = new Dictionary<string, CardArtPack>(StringComparer.OrdinalIgnoreCase);
		List<string> list = new List<string>();
		foreach (CardArtPack pack in packs)
		{
			string id = pack.Id;
			if (!best.TryGetValue(id, out CardArtPack value))
			{
				best.Add(id, pack);
				list.Add(id);
			}
			else if (TotalBytes(pack) > TotalBytes(value))
			{
				best[id] = pack;
			}
		}
		return list.Select((string key) => best[key]).ToArray();
		static long TotalBytes(CardArtPack pack)
		{
			long num = 0L;
			foreach (CardArtPackCard card in pack.Cards)
			{
				num += card.Bytes.Length;
			}
			return num;
		}
	}

	private static IReadOnlyList<string> NormalizeRoots(IEnumerable<string> scanRoots)
	{
		List<string> list = new List<string>();
		foreach (string scanRoot in scanRoots)
		{
			if (string.IsNullOrWhiteSpace(scanRoot) || !Directory.Exists(scanRoot))
			{
				continue;
			}
			string full;
			try
			{
				full = Path.GetFullPath(scanRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
			}
			catch (Exception ex) when (((ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException) ? 1 : 0) != 0)
			{
				continue;
			}
			if (!list.Any((string existing) => IsUnder(full, existing)))
			{
				list.RemoveAll((string existing) => IsUnder(existing, full));
				list.Add(full);
			}
		}
		return list;
		static bool IsUnder(string candidate, string parent)
		{
			return candidate.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
		}
	}

	[IteratorStateMachine(typeof(_003CEnumeratePackFiles_003Ed__4))]
	private static IEnumerable<string> EnumeratePackFiles(string root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumeratePackFiles_003Ed__4(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static IReadOnlyList<string> TakeAll(IEnumerable<string> sequence)
	{
		List<string> list = new List<string>();
		try
		{
			foreach (string item in sequence)
			{
				list.Add(item);
			}
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
		}
		return list;
	}

	private static CardArtPack? ReadPack(string packPath, ICollection<string> warnings)
	{
		if (!HasBundleHeader(packPath))
		{
			warnings.Add($"忽略卡图包 {packPath}：缺少 {"card_art_bundle"} 标识。");
			return null;
		}
		string directoryName = Path.GetDirectoryName(packPath);
		(string? Id, string? Name) tuple = ReadMetadata(directoryName);
		string item = tuple.Id;
		string item2 = tuple.Name;
		string fileName = Path.GetFileName(packPath);
		string text = CardArtPackScanPolicy.NormalizePackId(item, fileName);
		IReadOnlyList<CardArtPackCard> readOnlyList = ReadCards(packPath, text, warnings);
		if (readOnlyList.Count == 0)
		{
			warnings.Add("卡图包 " + packPath + " 不含可用的卡面覆盖，已跳过。");
			return null;
		}
		int num = readOnlyList.Count((CardArtPackCard card) => card.IsFullArt && !card.IsCardShaped);
		if (num > 0)
		{
			warnings.Add($"卡图包 {packPath} 有 {num} 张声明全画幅的卡面实际是卡框裁切图，" + "已按卡框尺寸呈现。");
		}
		return new CardArtPack(text, item2 ?? text, packPath, directoryName, readOnlyList);
	}

	private static bool HasBundleHeader(string packPath)
	{
		using FileStream stream = OpenRead(packPath);
		using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		char[] array = new char[4096];
		int num = streamReader.Read(array, 0, array.Length);
		return num > 0 && CardArtPackScanPolicy.IsBundleHeader(new ReadOnlySpan<char>(array, 0, num));
	}

	private static IReadOnlyList<CardArtPackCard> ReadCards(string packPath, string packId, ICollection<string> warnings)
	{
		Dictionary<string, CardArtPackCard> dictionary = new Dictionary<string, CardArtPackCard>(StringComparer.OrdinalIgnoreCase);
		using (FileStream stream = OpenRead(packPath))
		{
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			char[] array = new char[524288];
			string text = string.Empty;
			bool flag = false;
			int length;
			while ((length = streamReader.Read(array, 0, 524288)) > 0)
			{
				string text2 = text + new string(array, 0, length);
				if (text2.Length > 67108864)
				{
					if (!flag)
					{
						flag = true;
						warnings.Add($"卡图包 {packPath} 的单条覆盖超出 {64}MB 解析上限，已停止读取。");
					}
					break;
				}
				var (readOnlyList, text3) = CardArtPackScanPolicy.SplitEntries(text2);
				foreach (string item in readOnlyList)
				{
					Merge(dictionary, CardArtPackScanPolicy.ParseCards(item), packId);
				}
				text = text3;
			}
			if (text.Length > 0)
			{
				Merge(dictionary, CardArtPackScanPolicy.ParseCards(text), packId);
			}
			return dictionary.Values.OrderBy<CardArtPackCard, string>((CardArtPackCard card) => card.SourcePath, StringComparer.OrdinalIgnoreCase).ToArray();
		}
		static void Merge(IDictionary<string, CardArtPackCard> target, IReadOnlyList<CardArtPackCard> parsed, string owner)
		{
			foreach (CardArtPackCard item2 in parsed)
			{
				if (!target.TryGetValue(item2.SourcePath, out CardArtPackCard value) || CardArtPackScanPolicy.ReplacesExisting(value, item2))
				{
					target[item2.SourcePath] = item2 with
					{
						PackId = owner
					};
				}
			}
		}
	}

	private static FileStream OpenRead(string path)
	{
		return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
	}

	private static (string? Id, string? Name) ReadMetadata(string root)
	{
		foreach (string item2 in Directory.EnumerateFiles(root, "*.json", SearchOption.TopDirectoryOnly))
		{
			if (Path.GetFileName(item2).EndsWith(".cardartpack.json", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			try
			{
				long length = new FileInfo(item2).Length;
				if ((length > 65536 || length == 0L) ? true : false)
				{
					continue;
				}
				using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(item2).TrimStart('\ufeff'));
				if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object || !jsonDocument.RootElement.TryGetProperty("id", out var value) || value.ValueKind != JsonValueKind.String)
				{
					continue;
				}
				JsonElement value2;
				string item = ((jsonDocument.RootElement.TryGetProperty("name", out value2) && value2.ValueKind == JsonValueKind.String) ? value2.GetString() : null);
				return (Id: value.GetString(), Name: item);
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? 1 : 0) != 0)
			{
			}
		}
		return default((string, string));
	}
}
