using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CznEnemySkin.Domain;

internal sealed class BattlefieldBackgroundPresentation
{
	public PresentationMode Mode { get; }

	public string ShellScenePath { get; }

	public IReadOnlyList<IReadOnlyList<string>> BgLayerGroups { get; }

	public IReadOnlyList<string> FgLayers { get; }

	public IEnumerable<string> ResourcePaths => new string[1] { ShellScenePath }.Concat(BgLayerGroups.SelectMany((IReadOnlyList<string> group) => group)).Concat(FgLayers).Distinct<string>(StringComparer.Ordinal);

	public BattlefieldBackgroundPresentation(PresentationMode mode, string shellScenePath, IEnumerable<IEnumerable<string>> bgLayerGroups, IEnumerable<string>? fgLayers = null)
	{
		Mode = mode;
		ShellScenePath = RequireNamespacedResourcePath(shellScenePath, "shellScenePath");
		IReadOnlyList<string>[] array = bgLayerGroups?.Select(delegate(IEnumerable<string> group, int index)
		{
			string[] obj = group?.Select((string path) => RequireNamespacedResourcePath(path, $"{"bgLayerGroups"}[{index}]")).Distinct<string>(StringComparer.Ordinal).ToArray() ?? throw new ArgumentException("Background layer group cannot be null.", "bgLayerGroups");
			if (obj.Length == 0)
			{
				throw new ArgumentException("Background layer group cannot be empty.", "bgLayerGroups");
			}
			return new ReadOnlyCollection<string>(obj);
		}).ToArray() ?? throw new ArgumentNullException("bgLayerGroups");
		IReadOnlyList<string>[] array2 = array;
		if (mode == PresentationMode.ReplaceReady && array2.Length == 0)
		{
			throw new ArgumentException("A replace-ready background requires at least one layer group.", "bgLayerGroups");
		}
		BgLayerGroups = new ReadOnlyCollection<IReadOnlyList<string>>(array2);
		FgLayers = new ReadOnlyCollection<string>((fgLayers ?? Array.Empty<string>()).Select((string path) => RequireNamespacedResourcePath(path, "fgLayers")).Distinct<string>(StringComparer.Ordinal).ToArray());
	}

	private static string RequireNamespacedResourcePath(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Resource path cannot be empty.", parameterName);
		}
		string text = value.Trim();
		if (!text.StartsWith("res://CznEnemySkin/", StringComparison.Ordinal))
		{
			throw new ArgumentException("Battlefield replacement resources must stay under res://CznEnemySkin/ (" + text + ").", parameterName);
		}
		return text;
	}
}
