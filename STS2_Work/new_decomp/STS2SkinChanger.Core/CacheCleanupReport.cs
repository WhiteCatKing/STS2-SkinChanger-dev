using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed record CacheCleanupReport(string UserDirectory, string TempDirectory)
{
	public int RemovedFiles { get; set; }

	public List<string> Files { get; } = new List<string>();

	public List<string> Directories { get; } = new List<string>();

	public List<string> Errors { get; } = new List<string>();
}
