using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ModLaunchManager;

internal static class SteamWorkshopCache
{
	private sealed class Node
	{
		internal readonly Dictionary<string, Node> Children = new Dictionary<string, Node>(StringComparer.Ordinal);

		internal string Text = string.Empty;

		internal Node Child(string key)
		{
			return Children.GetValueOrDefault(key) ?? new Node();
		}

		internal string Value(string key)
		{
			return Child(key).Text;
		}

		internal ulong Number(string key)
		{
			if (!ulong.TryParse(Value(key), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
			{
				return 0uL;
			}
			return result;
		}
	}

	private sealed class Parser(string text)
	{
		private int _position;

		internal Node Read()
		{
			Node result = Block(0, nested: false);
			if (_position != text.Length)
			{
				throw new InvalidDataException("Trailing Steam cache data");
			}
			return result;
		}

		private Node Block(int depth, bool nested)
		{
			if (depth > 12)
			{
				throw new InvalidDataException("Steam cache nesting is too deep");
			}
			Node node = new Node();
			string key;
			Node value;
			do
			{
				Space();
				if (_position == text.Length)
				{
					if (nested)
					{
						throw new InvalidDataException("Truncated Steam cache");
					}
					return node;
				}
				if (text[_position] == '}')
				{
					if (!nested)
					{
						throw new InvalidDataException("Unexpected Steam cache brace");
					}
					_position++;
					return node;
				}
				key = Quoted();
				Space();
				if (_position < text.Length && text[_position] == '{')
				{
					_position++;
					value = Block(depth + 1, nested: true);
				}
				else
				{
					value = new Node
					{
						Text = Quoted()
					};
				}
			}
			while (node.Children.TryAdd(key, value));
			throw new InvalidDataException("Duplicate Steam cache key");
		}

		private void Space()
		{
			while (_position < text.Length)
			{
				if (char.IsWhiteSpace(text[_position]) || text[_position] == '\ufeff')
				{
					_position++;
					continue;
				}
				if (_position + 1 < text.Length)
				{
					string text = text;
					int position = _position;
					if (text.Substring(position, _position + 2 - position) == "//")
					{
						while (_position < text.Length && text[_position] != '\n')
						{
							_position++;
						}
						continue;
					}
					break;
				}
				break;
			}
		}

		private string Quoted()
		{
			Space();
			if (_position >= text.Length || text[_position++] != '"')
			{
				throw new InvalidDataException("Expected Steam cache string");
			}
			StringBuilder stringBuilder = new StringBuilder();
			while (_position < text.Length)
			{
				char c = text[_position++];
				int num;
				switch (c)
				{
				case '"':
					return stringBuilder.ToString();
				case '\\':
					num = ((_position < text.Length) ? 1 : 0);
					break;
				default:
					num = 0;
					break;
				}
				bool flag = (byte)num != 0;
				if (flag)
				{
					char c2 = text[_position];
					bool flag2 = ((c2 == '"' || c2 == '\\') ? true : false);
					flag = flag2;
				}
				if (flag)
				{
					c = text[_position++];
				}
				stringBuilder.Append(c);
			}
			throw new InvalidDataException("Truncated Steam cache string");
		}
	}

	internal static (string CacheFile, string ItemRoot)? Locate(string modPath, uint appId, ulong itemId)
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(modPath);
		while (directoryInfo?.Parent?.Parent?.Parent != null)
		{
			if (!(directoryInfo.Name != itemId.ToString(CultureInfo.InvariantCulture)) && !(directoryInfo.Parent.Name != appId.ToString(CultureInfo.InvariantCulture)) && directoryInfo.Parent.Parent.Name.Equals("content", StringComparison.OrdinalIgnoreCase) && directoryInfo.Parent.Parent.Parent.Name.Equals("workshop", StringComparison.OrdinalIgnoreCase))
			{
				return (Path.Combine(directoryInfo.Parent.Parent.Parent.FullName, $"appworkshop_{appId}.acf"), directoryInfo.FullName);
			}
			directoryInfo = directoryInfo.Parent;
		}
		return null;
	}

	internal static IReadOnlyDictionary<ulong, WorkshopItemVersions> Read(string path)
	{
		using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		if (fileStream.Length > 8388608)
		{
			throw new InvalidDataException("Steam Workshop cache is too large");
		}
		using StreamReader streamReader = new StreamReader(fileStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true), detectEncodingFromByteOrderMarks: true);
		return Parse(streamReader.ReadToEnd());
	}

	internal static IReadOnlyDictionary<ulong, WorkshopItemVersions> Parse(string text)
	{
		if (text.Length > 8388608)
		{
			throw new InvalidDataException("Steam Workshop cache is too large");
		}
		Node node = new Parser(text).Read().Child("AppWorkshop");
		Node node2 = node.Child("WorkshopItemsInstalled");
		Dictionary<ulong, WorkshopItemVersions> dictionary = new Dictionary<ulong, WorkshopItemVersions>();
		foreach (var (text3, node4) in node.Child("WorkshopItemDetails").Children)
		{
			if (!ulong.TryParse(text3, out var result) || result == 0L)
			{
				continue;
			}
			List<WorkshopItemVersion> list = new List<WorkshopItemVersion>();
			Node node5 = node4.Child("author_snapshots");
			bool isComplete = true;
			foreach (Node value in node5.Children.Values)
			{
				ulong num = value.Number("manifest");
				if (num != 0L)
				{
					list.Add(new WorkshopItemVersion(num, (uint)value.Number("timeupdated"), value.Value("game_branch_min"), value.Value("game_branch_max")));
				}
				else
				{
					isComplete = false;
				}
			}
			if (node5.Children.Count == 0 && node5.Text.Length == 0)
			{
				ulong num2 = node4.Number("latest_manifest");
				if (num2 != 0)
				{
					list.Add(new WorkshopItemVersion(num2, (uint)node4.Number("latest_timeupdated"), string.Empty, string.Empty));
				}
			}
			dictionary[result] = new WorkshopItemVersions(node2.Child(text3).Number("manifest"), list, isComplete);
		}
		return dictionary;
	}
}
