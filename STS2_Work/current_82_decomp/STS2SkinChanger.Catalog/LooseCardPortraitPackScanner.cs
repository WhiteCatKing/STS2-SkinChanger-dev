using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace STS2SkinChanger.Catalog;

internal static class LooseCardPortraitPackScanner
{
	[CompilerGenerated]
	private sealed class _003CEnumerateImageFolders_003Ed__7 : IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>, IEnumerable, IEnumerator<KeyValuePair<string, IReadOnlyList<string>>>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private KeyValuePair<string, IReadOnlyList<string>> _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		private Stack<string> _003Cpending_003E5__2;

		private int _003Cvisited_003E5__3;

		KeyValuePair<string, IReadOnlyList<string>> IEnumerator<KeyValuePair<string, IReadOnlyList<string>>>.Current
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
		public _003CEnumerateImageFolders_003Ed__7(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cpending_003E5__2 = null;
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
				_003Cpending_003E5__2 = new Stack<string>();
				_003Cpending_003E5__2.Push(root);
				_003Cvisited_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			while (_003Cpending_003E5__2.Count > 0 && _003Cvisited_003E5__3 < 20000)
			{
				string text = _003Cpending_003E5__2.Pop();
				string[] files;
				try
				{
					foreach (string item in Directory.EnumerateDirectories(text))
					{
						if (!Path.GetFileName(item).StartsWith('.'))
						{
							_003Cpending_003E5__2.Push(item);
						}
					}
					files = Directory.GetFiles(text);
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
				{
					continue;
				}
				_003Cvisited_003E5__3 += files.Length;
				string[] array = files.Where(IsCardImageFile).OrderBy<string, string>((string file) => file, StringComparer.OrdinalIgnoreCase).ToArray();
				if (array.Length != 0)
				{
					_003C_003E2__current = new KeyValuePair<string, IReadOnlyList<string>>(text, array);
					_003C_003E1__state = 1;
					return true;
				}
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
		IEnumerator<KeyValuePair<string, IReadOnlyList<string>>> IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>.GetEnumerator()
		{
			_003CEnumerateImageFolders_003Ed__7 _003CEnumerateImageFolders_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateImageFolders_003Ed__ = this;
			}
			else
			{
				_003CEnumerateImageFolders_003Ed__ = new _003CEnumerateImageFolders_003Ed__7(0);
			}
			_003CEnumerateImageFolders_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateImageFolders_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, IReadOnlyList<string>>>)this).GetEnumerator();
		}
	}

	public const int MaxImageBytes = 33554432;

	private const int MaxScannedFiles = 20000;

	private const int MaxCardEntries = 4096;

	public static IReadOnlyList<CardArtPack> Scan(IEnumerable<LooseCardPortraitProvider> providers, IReadOnlyList<CardCatalogEntry> cards)
	{
		ArgumentNullException.ThrowIfNull(providers, "providers");
		ArgumentNullException.ThrowIfNull(cards, "cards");
		List<CardArtPack> list = new List<CardArtPack>();
		if (cards.Count == 0)
		{
			return list;
		}
		foreach (LooseCardPortraitProvider provider in providers)
		{
			CardArtPack cardArtPack = ScanProvider(provider, cards);
			if (cardArtPack != null)
			{
				list.Add(cardArtPack);
			}
		}
		return list;
	}

	public static bool IsPack(int imageCount, int matchedCards)
	{
		return LooseCardPortraitPackPolicy.IsPack(imageCount, matchedCards);
	}

	private static CardArtPack? ScanProvider(LooseCardPortraitProvider provider, IReadOnlyList<CardCatalogEntry> cards)
	{
		if (string.IsNullOrWhiteSpace(provider.Id) || string.IsNullOrWhiteSpace(provider.RootPath) || !Directory.Exists(provider.RootPath))
		{
			return null;
		}
		string fullPath;
		try
		{
			fullPath = Path.GetFullPath(provider.RootPath);
		}
		catch (Exception ex) when (((ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException) ? 1 : 0) != 0)
		{
			return null;
		}
		if (HasFileWithExtension(fullPath, ".pck", SearchOption.TopDirectoryOnly) || HasFileWithExtension(fullPath, ".cardartpack.json", SearchOption.AllDirectories))
		{
			return null;
		}
		string pathToken = LooseCardPortraitPackPolicy.SanitizePathToken(provider.Id);
		string text = null;
		List<CardArtPackCard> list = new List<CardArtPackCard>();
		foreach (var (text3, readOnlyList2) in EnumerateImageFolders(fullPath))
		{
			if (readOnlyList2.Count < 20)
			{
				continue;
			}
			List<CardArtPackCard> list2 = new List<CardArtPackCard>();
			foreach (string item in readOnlyList2)
			{
				if (!(SkinCatalog.ResolveCardForPortraitStem(Path.GetFileNameWithoutExtension(item), cards) == null) && TryReadCard(item, pathToken, fullPath, out CardArtPackCard entry))
				{
					list2.Add(entry);
				}
			}
			if (IsPack(readOnlyList2.Count, list2.Count) && list2.Count > list.Count)
			{
				text = text3;
				list = list2;
			}
		}
		if (text == null || list.Count == 0)
		{
			return null;
		}
		return new CardArtPack(provider.Id, string.IsNullOrWhiteSpace(provider.Name) ? provider.Id : provider.Name, text, fullPath, list.OrderBy<CardArtPackCard, string>((CardArtPackCard card) => card.SourcePath, StringComparer.OrdinalIgnoreCase).Take(4096).ToArray());
	}

	private static bool TryReadCard(string imagePath, string pathToken, string root, out CardArtPackCard entry)
	{
		entry = null;
		try
		{
			FileInfo fileInfo = new FileInfo(imagePath);
			if (!fileInfo.Exists || fileInfo.Length <= 0 || fileInfo.Length > 33554432)
			{
				return false;
			}
			byte[] array = File.ReadAllBytes(imagePath);
			string text = Path.GetRelativePath(root, imagePath).Replace(Path.DirectorySeparatorChar, '/');
			int width;
			int height;
			string displayMode = ((CardArtPackScanPolicy.TryReadPngSize(array, out width, out height) && height > width) ? "full_art" : "art_window");
			entry = new CardArtPackCard("res://" + pathToken + "/" + text, Path.GetFileNameWithoutExtension(imagePath), array, displayMode, "static", UsesEditSource: false);
			return true;
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException) ? 1 : 0) != 0)
		{
			return false;
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateImageFolders_003Ed__7))]
	private static IEnumerable<KeyValuePair<string, IReadOnlyList<string>>> EnumerateImageFolders(string root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateImageFolders_003Ed__7(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static bool IsCardImageFile(string path)
	{
		string fileName = Path.GetFileName(path);
		if (fileName.StartsWith('.') || fileName.EndsWith(".import", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool HasFileWithExtension(string root, string extension, SearchOption searchOption)
	{
		try
		{
			return Directory.EnumerateFiles(root, "*" + extension, searchOption).Any((string file) => !Path.GetFileName(file).StartsWith('.'));
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return false;
		}
	}
}
