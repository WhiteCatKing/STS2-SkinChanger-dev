using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ModLaunchManager;

internal static class ModTranslationFiles
{
	[CompilerGenerated]
	private sealed class _003CEnumerateFiles_003Ed__14 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private int depth;

		public int _003C_003E3__depth;

		private string directory;

		public string _003C_003E3__directory;

		private IEnumerator<string> _003C_003E7__wrap1;

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
		public _003CEnumerateFiles_003Ed__14(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 1)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				string current2;
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (depth > 16)
					{
						throw new TranslationFileException("invalidPath");
					}
					_003C_003E7__wrap1 = Directory.EnumerateFileSystemEntries(directory).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_011b;
				case 1:
					_003C_003E1__state = -4;
					goto IL_00d7;
				case 2:
					{
						_003C_003E1__state = -3;
						goto IL_011b;
					}
					IL_00d7:
					if (_003C_003E7__wrap2.MoveNext())
					{
						string current = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_011b;
					IL_011b:
					while (true)
					{
						if (_003C_003E7__wrap1.MoveNext())
						{
							current2 = _003C_003E7__wrap1.Current;
							FileAttributes attributes = File.GetAttributes(current2);
							if ((attributes & FileAttributes.ReparsePoint) != 0)
							{
								throw new TranslationFileException("invalidPath");
							}
							if ((attributes & FileAttributes.Directory) != 0)
							{
								break;
							}
							if (current2.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
							{
								_003C_003E2__current = current2;
								_003C_003E1__state = 2;
								return true;
							}
							continue;
						}
						_003C_003Em__Finally1();
						_003C_003E7__wrap1 = null;
						return false;
					}
					_003C_003E7__wrap2 = EnumerateFiles(current2, depth + 1).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00d7;
				}
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
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
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
			_003CEnumerateFiles_003Ed__14 _003CEnumerateFiles_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateFiles_003Ed__ = this;
			}
			else
			{
				_003CEnumerateFiles_003Ed__ = new _003CEnumerateFiles_003Ed__14(0);
			}
			_003CEnumerateFiles_003Ed__.directory = _003C_003E3__directory;
			_003CEnumerateFiles_003Ed__.depth = _003C_003E3__depth;
			return _003CEnumerateFiles_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	internal static readonly (string Code, string Name)[] Languages = new(string, string)[16]
	{
		("eng", "English"),
		("zhs", "简体中文"),
		("zht", "繁體中文"),
		("deu", "Deutsch"),
		("esp", "Español (Latinoamérica)"),
		("fra", "Français"),
		("ind", "Bahasa Indonesia"),
		("ita", "Italiano"),
		("jpn", "日本語"),
		("kor", "한국어"),
		("pol", "Polski"),
		("ptb", "Português (Brasil)"),
		("rus", "Русский"),
		("spa", "Español (España)"),
		("tha", "ไทย"),
		("tur", "Türkçe")
	};

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
	};

	internal static string ModFolder(string root, string modId)
	{
		ValidateModId(modId);
		string text = Convert.ToHexString(SHA256.HashData(PckLocalizationArchive.Utf8.GetBytes(modId))).Substring(0, 16);
		string text2 = new string(modId.Where(delegate(char c)
		{
			bool flag = char.IsAsciiLetterOrDigit(c);
			if (!flag)
			{
				bool flag2 = ((c == '-' || c == '_') ? true : false);
				flag = flag2;
			}
			return flag;
		}).Take(40).ToArray());
		return Path.Combine(root, text2 + "-" + text);
	}

	internal static void ValidateModId(string modId)
	{
		bool flag = string.IsNullOrWhiteSpace(modId) || modId.Length > 256 || modId.Any((char c) => char.IsControl(c) || "\\/:*?\"<>|".Contains(c));
		if (!flag)
		{
			bool flag2 = ((modId == "." || modId == "..") ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			throw new TranslationFileException("invalidPath");
		}
	}

	internal static string ValidateRelativePath(string path)
	{
		string[] array = path.Split('/');
		if (path.Length > 1024 || array.Length < 2 || array.Any(delegate(string part)
		{
			bool flag = string.IsNullOrWhiteSpace(part);
			if (!flag)
			{
				bool flag2 = ((part == "." || part == "..") ? true : false);
				flag = flag2;
			}
			return flag || part.EndsWith('.') || part.EndsWith(' ') || part.Any((char c) => char.IsControl(c) || "\\:*?\"<>|".Contains(c)) || IsDeviceName(part);
		}))
		{
			throw new TranslationFileException("invalidPath");
		}
		if (!array[0].All(delegate(char c)
		{
			bool flag3 = char.IsAsciiLetterOrDigit(c);
			if (!flag3)
			{
				bool flag4 = ((c == '-' || c == '_') ? true : false);
				flag3 = flag4;
			}
			return flag3;
		}) || !path.EndsWith(".json", StringComparison.Ordinal))
		{
			throw new TranslationFileException("invalidPath");
		}
		return path;
	}

	private static bool IsDeviceName(string part)
	{
		string text = part.Split('.')[0].ToUpperInvariant();
		bool flag;
		switch (text)
		{
		case "CON":
		case "PRN":
		case "AUX":
		case "NUL":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			if (text.Length == 4 && (text.StartsWith("COM") || text.StartsWith("LPT")))
			{
				return char.IsAsciiDigit(text[3]);
			}
			return false;
		}
		return true;
	}

	internal static string LanguageFolder(string modFolder, string language)
	{
		if (!Languages.Any(((string Code, string Name) l) => l.Code == language))
		{
			throw new TranslationFileException("invalidPath");
		}
		return Path.Combine(modFolder, "custom", language);
	}

	internal static Dictionary<string, string> Parse(byte[] bytes)
	{
		if (bytes.Length > 8388608)
		{
			throw new TranslationFileException("tooLarge");
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(PckLocalizationArchive.Utf8.GetString(bytes).TrimStart('\ufeff'), new JsonDocumentOptions
			{
				CommentHandling = JsonCommentHandling.Skip
			});
			if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object)
			{
				throw new TranslationFileException("invalidJson");
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (JsonProperty item in jsonDocument.RootElement.EnumerateObject())
			{
				if (item.Name.Length == 0 || item.Value.ValueKind != JsonValueKind.String || !dictionary.TryAdd(item.Name, item.Value.GetString()))
				{
					throw new TranslationFileException("invalidJson");
				}
			}
			return dictionary;
		}
		catch (Exception ex) when (((ex is JsonException || ex is DecoderFallbackException) ? 1 : 0) != 0)
		{
			throw new TranslationFileException("invalidJson", ex.Message);
		}
	}

	internal static byte[] Serialize(Dictionary<string, string> table)
	{
		return JsonSerializer.SerializeToUtf8Bytes(table, JsonOptions);
	}

	internal static byte[] ReadFile(string path)
	{
		using FileStream fileStream = File.OpenRead(path);
		if (fileStream.Length > 8388608)
		{
			throw new TranslationFileException("tooLarge");
		}
		byte[] array = new byte[checked((int)fileStream.Length)];
		fileStream.ReadExactly(array);
		return array;
	}

	internal static string Export(string modFolder, IReadOnlyDictionary<string, byte[]> original)
	{
		string text = Path.Combine(modFolder, "exports", DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6), "localization");
		foreach (KeyValuePair<string, byte[]> item in original)
		{
			item.Deconstruct(out var key, out var value);
			string path = key;
			byte[] bytes = value;
			string text2 = Path.Combine(text, ValidateRelativePath(path).Replace('/', Path.DirectorySeparatorChar));
			RejectLinks(modFolder, text2);
			Directory.CreateDirectory(Path.GetDirectoryName(text2));
			File.WriteAllText(text2, PckLocalizationArchive.Utf8.GetString(bytes), PckLocalizationArchive.Utf8);
		}
		return text;
	}

	internal static void Import(string modFolder, string language, string source, IReadOnlyDictionary<string, byte[]> original, Action<Dictionary<string, string>>? validate = null)
	{
		string name = Path.GetFileName(source);
		string[] array = (from p in original.Keys
			select p.Substring(p.IndexOf('/') + 1) into p
			where Path.GetFileName(p) == name
			select p).Distinct<string>(StringComparer.Ordinal).ToArray();
		if (array.Length != 1)
		{
			throw new TranslationFileException("unknownTable");
		}
		Dictionary<string, string> dictionary = Parse(ReadFile(source));
		validate?.Invoke(dictionary);
		string text = Path.Combine(LanguageFolder(modFolder, language), array[0].Replace('/', Path.DirectorySeparatorChar));
		RejectLinks(modFolder, text);
		WriteAtomic(text, Serialize(dictionary));
	}

	internal static Dictionary<string, byte[]> BuildOverlay(string modFolder, string modId, IReadOnlyDictionary<string, byte[]> original, Action<Dictionary<string, string>>? validate = null)
	{
		ValidateModId(modId);
		string text = Path.Combine(modFolder, "custom");
		Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>(StringComparer.Ordinal);
		if (!AiTranslationDocument.Enabled(modFolder))
		{
			return dictionary;
		}
		(string, string)[] languages = Languages;
		string key;
		string value2;
		for (int i = 0; i < languages.Length; i++)
		{
			string item = languages[i].Item1;
			AiTranslationDocument aiTranslationDocument = AiTranslationDocument.Read(modFolder, item);
			if (aiTranslationDocument == null)
			{
				continue;
			}
			foreach (KeyValuePair<string, Dictionary<string, string>> table in aiTranslationDocument.Tables)
			{
				table.Deconstruct(out key, out var value);
				string name = key;
				Dictionary<string, string> dictionary2 = value;
				string text2 = item + "/" + name;
				string text3 = (original.ContainsKey(text2) ? text2 : (original.ContainsKey("eng/" + name) ? ("eng/" + name) : original.Keys.Order<string>(StringComparer.Ordinal).FirstOrDefault((string p) => p.Substring(p.IndexOf('/') + 1) == name)));
				if (text3 == null)
				{
					continue;
				}
				Dictionary<string, string> dictionary3 = Parse(original[text3]);
				string key2 = (original.ContainsKey("eng/" + name) ? ("eng/" + name) : original.Keys.Order<string>(StringComparer.Ordinal).First((string p) => p.Substring(p.IndexOf('/') + 1) == name));
				Dictionary<string, string> dictionary4 = Parse(original[key2]);
				foreach (KeyValuePair<string, string> item2 in dictionary2)
				{
					item2.Deconstruct(out key, out value2);
					string key3 = key;
					string text4 = value2;
					if (dictionary4.TryGetValue(key3, out var value3) && aiTranslationDocument.Sources[name].TryGetValue(key3, out string value4) && value3 == value4 && (!original.ContainsKey(text2) || !dictionary3.TryGetValue(key3, out var value5) || value5 == value3))
					{
						new ProtectedTranslationText(value3).VerifyEquivalent(new ProtectedTranslationText(text4));
						dictionary3[key3] = ProtectedTranslationText.ForGamePlurals(item, text4);
					}
				}
				validate?.Invoke(dictionary3);
				dictionary[modId + "/localization/" + text2] = Serialize(dictionary3);
			}
		}
		if (dictionary.Count > 2048 || dictionary.Values.Any((byte[] b) => b.Length > 8388608) || ((IEnumerable<byte[]>)dictionary.Values).Sum((Func<byte[], long>)((byte[] b) => b.Length)) > 67108864)
		{
			throw new TranslationFileException("tooLarge");
		}
		if (!Directory.Exists(text))
		{
			return dictionary;
		}
		RejectLinks(modFolder, text);
		long num = ((IEnumerable<byte[]>)dictionary.Values).Sum((Func<byte[], long>)((byte[] b) => b.Length));
		foreach (string item3 in EnumerateFiles(text))
		{
			string text5 = ValidateRelativePath(Path.GetRelativePath(text, item3).Replace(Path.DirectorySeparatorChar, '/'));
			string language = text5.Substring(0, text5.IndexOf('/'));
			if (!Languages.Any(((string Code, string Name) l) => l.Code == language))
			{
				throw new TranslationFileException("unknownLanguage");
			}
			string tableName = text5.Substring(text5.IndexOf('/') + 1);
			string text6 = (original.ContainsKey(text5) ? text5 : (original.ContainsKey("eng/" + tableName) ? ("eng/" + tableName) : original.Keys.Order<string>(StringComparer.Ordinal).FirstOrDefault((string p) => p.Substring(p.IndexOf('/') + 1) == tableName)));
			if (text6 == null)
			{
				throw new TranslationFileException("unknownTable");
			}
			Dictionary<string, string> dictionary5;
			try
			{
				dictionary5 = Parse(ReadFile(item3));
				validate?.Invoke(dictionary5);
			}
			catch (TranslationFileException ex)
			{
				throw new TranslationFileException(ex.Reason, "Custom translation '" + text5 + "': " + ex.Message);
			}
			string key4 = modId + "/localization/" + text5;
			byte[] value6;
			Dictionary<string, string> dictionary6 = Parse(dictionary.TryGetValue(key4, out value6) ? value6 : original[text6]);
			foreach (KeyValuePair<string, string> item4 in dictionary5)
			{
				item4.Deconstruct(out value2, out key);
				string key5 = value2;
				string value7 = key;
				dictionary6[key5] = value7;
			}
			byte[] array = Serialize(dictionary6);
			num += array.Length;
			if (array.Length > 8388608 || num > 67108864 || dictionary.Count >= 2048)
			{
				throw new TranslationFileException("tooLarge");
			}
			dictionary[key4] = array;
		}
		return dictionary;
	}

	internal static bool HasCustomFiles(string modFolder)
	{
		if (!AiTranslationDocument.Enabled(modFolder))
		{
			return false;
		}
		if (Languages.Any(((string Code, string Name) l) => File.Exists(AiTranslationDocument.FilePath(modFolder, l.Code))))
		{
			return true;
		}
		string text = Path.Combine(modFolder, "custom");
		if (!Directory.Exists(text))
		{
			return false;
		}
		RejectLinks(modFolder, text);
		return EnumerateFiles(text).Any();
	}

	[IteratorStateMachine(typeof(_003CEnumerateFiles_003Ed__14))]
	private static IEnumerable<string> EnumerateFiles(string directory, int depth = 0)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateFiles_003Ed__14(-2)
		{
			_003C_003E3__directory = directory,
			_003C_003E3__depth = depth
		};
	}

	internal static void RejectLinks(string root, string destination)
	{
		string fullPath = Path.GetFullPath(root);
		for (string text = Path.GetFullPath(destination); text != null; text = Path.GetDirectoryName(text))
		{
			if ((File.Exists(text) || Directory.Exists(text)) && (File.GetAttributes(text) & FileAttributes.ReparsePoint) != 0)
			{
				throw new TranslationFileException("invalidPath");
			}
			if (text == fullPath)
			{
				return;
			}
		}
		throw new TranslationFileException("invalidPath");
	}

	internal static void WriteAtomic(string path, byte[] bytes)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		string text = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			File.WriteAllBytes(text, bytes);
			File.Move(text, path, overwrite: true);
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
	}

	internal static void WriteOverlay(string path, IReadOnlyDictionary<string, byte[]> files)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true, Encoding.UTF8))
		{
			foreach (var (entryName, array2) in files.OrderBy<KeyValuePair<string, byte[]>, string>((KeyValuePair<string, byte[]> p) => p.Key, StringComparer.Ordinal))
			{
				using Stream stream = zipArchive.CreateEntry(entryName, CompressionLevel.Fastest).Open();
				stream.Write(array2);
			}
		}
		WriteAtomic(path, memoryStream.ToArray());
	}
}
