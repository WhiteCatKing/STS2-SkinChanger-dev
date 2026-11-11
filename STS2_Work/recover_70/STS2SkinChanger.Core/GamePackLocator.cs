using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class GamePackLocator
{
	private const string CanonicalPackName = "SlayTheSpire2.pck";

	private const string MainPackArgumentName = "--main-pack";

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

	public static string Resolve(string executablePath, IReadOnlyList<string> commandLineArguments, Action<string>? warn = null)
	{
		IReadOnlyList<string> readOnlyList = BuildCandidates(executablePath, commandLineArguments, warn);
		foreach (string item in readOnlyList)
		{
			if (File.Exists(item))
			{
				return item;
			}
		}
		throw new FileNotFoundException("找不到游戏主资源包。已检查：" + string.Join("；", readOnlyList));
	}

	internal static IReadOnlyList<string> BuildCandidates(string executablePath, IReadOnlyList<string> commandLineArguments, Action<string>? warn = null)
	{
		if (string.IsNullOrWhiteSpace(executablePath))
		{
			throw new ArgumentException("游戏可执行文件路径为空。", "executablePath");
		}
		string fullPath = Path.GetFullPath(executablePath);
		string text = Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException("无法取得游戏可执行文件目录。");
		string fileName = Path.GetFileName(fullPath);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
		List<string> list = new List<string> { text };
		if (Path.GetFileName(text).Equals("MacOS", StringComparison.OrdinalIgnoreCase))
		{
			list.Insert(0, Path.GetFullPath(Path.Combine(text, "..", "Resources")));
		}
		string[] source = new string[3]
		{
			"SlayTheSpire2.pck",
			fileNameWithoutExtension + ".pck",
			fileName + ".pck"
		}.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		List<string> list2 = new List<string>();
		string text2 = MainPackPath(commandLineArguments);
		if (!string.IsNullOrWhiteSpace(text2))
		{
			try
			{
				list2.Add(Path.GetFullPath(text2));
			}
			catch (Exception ex) when (((ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException) ? 1 : 0) != 0)
			{
				warn?.Invoke("启动参数 --main-pack 的路径无效，已忽略：" + ex.Message);
			}
		}
		foreach (string directory in list.Distinct<string>(PathComparer))
		{
			list2.AddRange(source.Select((string name) => Path.Combine(directory, name)));
			if (IsFileSystemRoot(directory))
			{
				warn?.Invoke("跳过文件系统根目录 " + directory + "，不在根目录枚举游戏资源包。");
				continue;
			}
			try
			{
				if (!Directory.Exists(directory))
				{
					continue;
				}
				foreach (string item in Directory.EnumerateFiles(directory))
				{
					string fileName2 = Path.GetFileName(item);
					if (source.Contains<string>(fileName2, StringComparer.OrdinalIgnoreCase))
					{
						list2.Add(item);
					}
				}
			}
			catch (Exception ex2) when (((ex2 is UnauthorizedAccessException || ex2 is IOException) ? 1 : 0) != 0)
			{
				warn?.Invoke("无法枚举目录 " + directory + "，跳过该来源继续使用其它候选：" + ex2.Message);
			}
		}
		return list2.Select(Path.GetFullPath).Distinct<string>(PathComparer).ToArray();
	}

	internal static bool IsFileSystemRoot(string directory)
	{
		if (string.IsNullOrWhiteSpace(directory))
		{
			return false;
		}
		string fullPath = Path.GetFullPath(directory);
		string pathRoot = Path.GetPathRoot(fullPath);
		if (!string.IsNullOrEmpty(pathRoot))
		{
			return PathComparer.Equals(Path.TrimEndingDirectorySeparator(fullPath), Path.TrimEndingDirectorySeparator(Path.GetFullPath(pathRoot)));
		}
		return false;
	}

	private static string? MainPackPath(IReadOnlyList<string>? commandLineArguments)
	{
		if (commandLineArguments == null)
		{
			return null;
		}
		for (int i = 0; i < commandLineArguments.Count; i++)
		{
			string text = commandLineArguments[i];
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (text.Equals("--main-pack", StringComparison.Ordinal))
			{
				if (i + 1 >= commandLineArguments.Count)
				{
					return null;
				}
				return commandLineArguments[i + 1];
			}
			if (text.StartsWith("--main-pack=", StringComparison.Ordinal))
			{
				string text2 = text;
				int num = "--main-pack".Length + 1;
				return text2.Substring(num, text2.Length - num);
			}
		}
		return null;
	}
}
