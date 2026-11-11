using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HarmonyLib;

namespace STS2SkinChanger.Catalog;

internal sealed record StatefulCardArtContract(string AssemblyPath, string ArtType, string ImageType, string SettingsType, string AtlasRoot, IReadOnlyDictionary<string, string[]> SpecialPortraits, IReadOnlySet<string> DisabledCharacters)
{
	[CompilerGenerated]
	private sealed class _003CCharacterAssets_003Ed__45 : IEnumerable<(string GroupId, IReadOnlyDictionary<string, ResourceAsset> Assets)>, IEnumerable, IEnumerator<(string GroupId, IReadOnlyDictionary<string, ResourceAsset> Assets)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (string GroupId, IReadOnlyDictionary<string, ResourceAsset> Assets) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private PckResourceIndex index;

		public PckResourceIndex _003C_003E3__index;

		public StatefulCardArtContract _003C_003E4__this;

		private IEnumerator<string> _003C_003E7__wrap1;

		(string GroupId, IReadOnlyDictionary<string, ResourceAsset> Assets) IEnumerator<(string, IReadOnlyDictionary<string, ResourceAsset>)>.Current
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
		public _003CCharacterAssets_003Ed__45(int _003C_003E1__state)
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
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				StatefulCardArtContract CS_0024_003C_003E8__locals3 = _003C_003E4__this;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = index.Archive.Paths.Where((string path) => path.StartsWith(CS_0024_003C_003E8__locals3.ResourceRoot + "/scenes/character_select/", StringComparison.Ordinal) && path.EndsWith("_bg.tscn", StringComparison.Ordinal)).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					string current = _003C_003E7__wrap1.Current;
					string fileName = Path.GetFileName(current);
					string text = fileName.Substring(0, fileName.Length - 8);
					if (!CS_0024_003C_003E8__locals3.DisabledCharacters.Contains(text))
					{
						ResourceAsset resourceAsset = index.TryBuildAsset(CS_0024_003C_003E8__locals3.ResourceRoot + "/images/character_select/" + text + "_icon.png");
						ResourceAsset resourceAsset2 = index.TryBuildAsset(current);
						if (resourceAsset != null && resourceAsset2 != null)
						{
							_003C_003E2__current = (GroupId: text, Assets: new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase)
							{
								["res://scenes/screens/char_select/char_select_bg_" + text + ".tscn"] = resourceAsset2,
								["res://images/packed/character_select/char_select_" + text + ".png"] = resourceAsset
							});
							_003C_003E1__state = 1;
							return true;
						}
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<(string GroupId, IReadOnlyDictionary<string, ResourceAsset> Assets)> IEnumerable<(string, IReadOnlyDictionary<string, ResourceAsset>)>.GetEnumerator()
		{
			_003CCharacterAssets_003Ed__45 _003CCharacterAssets_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CCharacterAssets_003Ed__ = this;
			}
			else
			{
				_003CCharacterAssets_003Ed__ = new _003CCharacterAssets_003Ed__45(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CCharacterAssets_003Ed__.index = _003C_003E3__index;
			return _003CCharacterAssets_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(string, IReadOnlyDictionary<string, ResourceAsset>)>)this).GetEnumerator();
		}
	}

	public IReadOnlySet<string> AvailablePortraits { get; init; } = new HashSet<string>(StringComparer.Ordinal);

	public string LocalizationPrefix => "sc_stateful_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ResourceRoot))).Substring(0, 12).ToLowerInvariant() + "_";

	public string ResourceRoot => AtlasRoot.Substring(0, AtlasRoot.IndexOf("/images/", StringComparison.Ordinal));

	private static readonly Dictionary<string, (long Size, DateTime Stamp, DateTime ManifestStamp, StatefulCardArtContract? Contract)> Cache = new Dictionary<string, (long, DateTime, DateTime, StatefulCardArtContract)>();

	public static StatefulCardArtContract? Scan(SkinModDescriptor mod)
	{
		if (!mod.HasDll || mod.RootPath == null)
		{
			return null;
		}
		return Read(SkinPackagePaths.Resolve(mod.RootPath, mod.ResourceNamespaceId, ".dll"));
	}

	internal static StatefulCardArtContract? Read(string path)
	{
		try
		{
			FileInfo fileInfo = new FileInfo(path);
			if (!fileInfo.Exists || fileInfo.Length > 33554432)
			{
				return null;
			}
			DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(Path.ChangeExtension(path, ".json"));
			lock (Cache)
			{
				if (Cache.TryGetValue(path, out (long, DateTime, DateTime, StatefulCardArtContract) value) && value.Item1 == fileInfo.Length && value.Item2 == fileInfo.LastWriteTimeUtc && value.Item3 == lastWriteTimeUtc)
				{
					return value.Item4;
				}
				StatefulCardArtContract statefulCardArtContract = Inspect(path);
				Cache[path] = (fileInfo.Length, fileInfo.LastWriteTimeUtc, lastWriteTimeUtc, statefulCardArtContract);
				return statefulCardArtContract;
			}
		}
		catch (Exception ex) when (!(ex is OutOfMemoryException))
		{
			return null;
		}
	}

	private static StatefulCardArtContract? Inspect(string path)
	{
		using FileStream fileStream = File.OpenRead(path);
		object obj = typeof(Harmony).Assembly.GetType("Mono.Cecil.AssemblyDefinition", throwOnError: true).GetMethod("ReadAssembly", new Type[1] { typeof(Stream) }).Invoke(null, new object[1] { fileStream });
		try
		{
			object[] array = Items(Get(Get(obj, "MainModule"), "Types")).ToArray();
			object art = array.SingleOrDefault((object type) => Text(type, "Name") == "AlternateCardArt" && HasMethods(type, "Get", "GetAll", "GetSplit", "AfterNCardUpdateVisuals", "OnNCardSubscribed", "OnNCardUnsubscribed"));
			if (art == null)
			{
				return null;
			}
			object obj2 = array.SingleOrDefault((object type) => Text(type, "Name") == "CardImg" && HasMethods(type, "get_PortraitPath", "get_Path", "Upgraded", "UpgradedIfExists"));
			if (obj2 == null)
			{
				return null;
			}
			object obj3 = array.SingleOrDefault((object type) => HasMethods(type, "get_UseCustomArt", "get_UseSimpleMode", "get_HideTitle", "get_HideDescription", "get_HideEnergy", "Init"));
			if (obj3 == null || !array.Any((object type) => Text(type, "Name") == "AlternateCardArt`1" && Text(type, "BaseType") == Text(art, "FullName")))
			{
				return null;
			}
			object method2 = Items(Get(obj2, "Methods")).Single((object method) => Text(method, "Name") == "get_PortraitPath");
			string root = Strings(method2).SingleOrDefault((string value) => value.StartsWith("res://", StringComparison.Ordinal) && value.EndsWith("/images/atlases/card_atlas.sprites/", StringComparison.Ordinal));
			if (root == null || !Strings(method2).Contains(".tres"))
			{
				return null;
			}
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
			object[] array2 = array;
			foreach (object value2 in array2)
			{
				object obj4 = Get(value2, "BaseType");
				if (obj4 == null || !Text(obj4, "FullName").StartsWith(Text(art, "FullName") + "`1<", StringComparison.Ordinal))
				{
					continue;
				}
				object obj5 = Items(Get(obj4, "GenericArguments")).SingleOrDefault();
				if (obj5 != null && Text(obj5, "FullName").StartsWith("MegaCrit.Sts2.Core.Models.Cards.", StringComparison.Ordinal))
				{
					dictionary[Text(obj5, "Name")] = (from value in Items(Get(value2, "Methods")).SelectMany(Strings)
						where value.Contains('/') && !value.Contains(':') && !value.Contains("..", StringComparison.Ordinal)
						select root + value + ".tres").Distinct<string>(StringComparer.Ordinal).ToArray();
				}
			}
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			int? num = ReadMinor(Path.ChangeExtension(path, ".json"));
			foreach (object item in array.SelectMany((object type) => Items(Get(type, "Methods"))))
			{
				string text = Text(item, "Name");
				if (!text.StartsWith("get_", StringComparison.Ordinal) || !text.EndsWith("SetActive", StringComparison.Ordinal))
				{
					continue;
				}
				object[] source = Instructions(item).ToArray();
				if (source.Any((object instruction) => Text(Get(instruction, "Operand"), "Name") == "get_Minor"))
				{
					int result;
					int num2 = (from instruction in source
						select Text(instruction, "OpCode") into opcode
						where opcode.StartsWith("ldc.i4.", StringComparison.Ordinal)
						select int.TryParse(opcode.Substring(7, opcode.Length - 7), out result) ? result : 0).DefaultIfEmpty().Max();
					if (!num.HasValue || num < num2)
					{
						string text2 = text;
						hashSet.Add(text2.Substring(4, text2.Length - 9 - 4).ToLowerInvariant());
					}
				}
			}
			return new StatefulCardArtContract(path, Text(art, "FullName"), Text(obj2, "FullName"), Text(obj3, "FullName"), root, dictionary, hashSet);
		}
		finally
		{
			(obj as IDisposable)?.Dispose();
		}
	}

	public IReadOnlyDictionary<string, ResourceAsset> ReadAtlas(PckResourceIndex index)
	{
		return (from path in index.Archive.Paths
			where path.StartsWith(AtlasRoot, StringComparison.Ordinal) && path.EndsWith(".tres", StringComparison.Ordinal)
			where Encoding.UTF8.GetString(index.Archive.ReadFile(path)).Contains("type=\"AtlasTexture\"", StringComparison.Ordinal)
			select path).Select(index.TryBuildAsset).OfType<ResourceAsset>().ToDictionary<ResourceAsset, string>((ResourceAsset asset) => asset.SourcePath, StringComparer.OrdinalIgnoreCase);
	}

	public string? DefaultPortrait(CardCatalogEntry card, IReadOnlyDictionary<string, ResourceAsset> assets)
	{
		if (DisabledCharacters.Contains(card.PoolGroupId))
		{
			return null;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(card.PortraitPath);
		string text;
		int num = (text = card.PortraitPath.Substring(0, card.PortraitPath.LastIndexOf('/'))).LastIndexOf('/') + 1;
		string text2 = text.Substring(num, text.Length - num);
		foreach (string pool in new string[2] { text2, card.PoolGroupId }.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			string text3 = AtlasRoot + pool + "/" + fileNameWithoutExtension + ".tres";
			if (assets.ContainsKey(text3))
			{
				text = text3;
			}
			else
			{
				string text4 = assets.Keys.Where((string path) => path.StartsWith(AtlasRoot + pool + "/", StringComparison.OrdinalIgnoreCase)).FirstOrDefault((string path) => Token(Path.GetFileNameWithoutExtension(path)) == Token(card.TypeName));
				if (text4 == null)
				{
					continue;
				}
				text = text4;
			}
			goto IL_01b6;
		}
		return SpecialPortraits.GetValueOrDefault(card.TypeName)?.FirstOrDefault(assets.ContainsKey);
		IL_01b6:
		return text;
	}

	[IteratorStateMachine(typeof(_003CCharacterAssets_003Ed__45))]
	public IEnumerable<(string GroupId, IReadOnlyDictionary<string, ResourceAsset> Assets)> CharacterAssets(PckResourceIndex index)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCharacterAssets_003Ed__45(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__index = index
		};
	}

	private static int? ReadMinor(string manifest)
	{
		if (!File.Exists(manifest))
		{
			return null;
		}
		using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(manifest));
		JsonElement value;
		Version result;
		return (jsonDocument.RootElement.TryGetProperty("version", out value) && Version.TryParse(value.GetString()?.TrimStart('v'), out result)) ? new int?(result.Minor) : ((int?)null);
	}

	private static string Token(string text)
	{
		return new string(text.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static bool HasMethods(object type, params string[] names)
	{
		return names.All((string name) => Items(Get(type, "Methods")).Any((object method) => Text(method, "Name") == name));
	}

	private static IEnumerable<object> Instructions(object method)
	{
		object obj = Get(method, "HasBody");
		if (!(obj is bool) || !(bool)obj)
		{
			return Array.Empty<object>();
		}
		return Items(Get(Get(method, "Body"), "Instructions"));
	}

	private static IEnumerable<string> Strings(object method)
	{
		return (from instruction in Instructions(method)
			where Text(instruction, "OpCode") == "ldstr"
			select Get(instruction, "Operand")).OfType<string>();
	}

	private static object? Get(object? value, string name)
	{
		return value?.GetType().GetProperty(name)?.GetValue(value);
	}

	private static string Text(object? value, string name)
	{
		return Get(value, name)?.ToString() ?? string.Empty;
	}

	private static IEnumerable<object> Items(object? value)
	{
		if (!(value is IEnumerable source))
		{
			return Array.Empty<object>();
		}
		return source.Cast<object>();
	}

	[CompilerGenerated]
	private StatefulCardArtContract(StatefulCardArtContract original)
	{
		AssemblyPath = original.AssemblyPath;
		ArtType = original.ArtType;
		ImageType = original.ImageType;
		SettingsType = original.SettingsType;
		AtlasRoot = original.AtlasRoot;
		SpecialPortraits = original.SpecialPortraits;
		DisabledCharacters = original.DisabledCharacters;
		AvailablePortraits = original.AvailablePortraits;
	}
}
