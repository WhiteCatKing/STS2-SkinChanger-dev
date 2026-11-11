using System;
using System.IO;

namespace STS2SkinChanger.Core;

internal static class ModPrivateDependencyPolicy
{
	public static bool IsModOwned(string? assemblyPath, string gameDirectory, string runtimeDirectory)
	{
		if (!TryFullPath(assemblyPath, out string fullPath))
		{
			return false;
		}
		if (!IsUnder(fullPath, gameDirectory))
		{
			return !IsUnder(fullPath, runtimeDirectory);
		}
		return false;
	}

	public static string? CandidatePath(string? requestingAssemblyPath, string? requestedAssemblyName, string gameDirectory, string runtimeDirectory, Func<string, bool> fileExists)
	{
		if (string.IsNullOrWhiteSpace(requestedAssemblyName) || !IsModOwned(requestingAssemblyPath, gameDirectory, runtimeDirectory) || !TryFullPath(requestingAssemblyPath, out string fullPath))
		{
			return null;
		}
		if (fileExists(Path.Combine(gameDirectory, requestedAssemblyName + ".dll")))
		{
			return null;
		}
		string directoryName = Path.GetDirectoryName(fullPath);
		if (string.IsNullOrEmpty(directoryName))
		{
			return null;
		}
		string text = Path.Combine(directoryName, requestedAssemblyName + ".dll");
		if (!fileExists(text))
		{
			return null;
		}
		return text;
	}

	private static bool IsUnder(string path, string? root)
	{
		if (string.IsNullOrWhiteSpace(root) || !TryFullPath(root, out string fullPath))
		{
			return false;
		}
		return path.StartsWith(fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
	}

	private static bool TryFullPath(string? path, out string fullPath)
	{
		fullPath = string.Empty;
		if (string.IsNullOrWhiteSpace(path))
		{
			return false;
		}
		try
		{
			fullPath = Path.GetFullPath(path);
			return true;
		}
		catch (Exception ex) when (((ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException) ? 1 : 0) != 0)
		{
			return false;
		}
	}
}
