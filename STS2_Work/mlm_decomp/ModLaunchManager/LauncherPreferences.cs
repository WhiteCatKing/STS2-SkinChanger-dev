using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class LauncherPreferences
{
	[JsonPropertyName("language")]
	public string Language { get; set; } = "system";

	[JsonPropertyName("theme")]
	[JsonConverter(typeof(LauncherThemeConverter))]
	public LauncherTheme Theme { get; set; }

	[JsonPropertyName("saveProfileId")]
	public int? SaveProfileId { get; set; }

	[JsonPropertyName("autoUnlockAll")]
	public bool AutoUnlockAll { get; set; }

	[JsonPropertyName("modIsolation")]
	public bool ModIsolation { get; set; } = true;

	[JsonPropertyName("background")]
	public string Background { get; set; } = "auto";

	[JsonPropertyName("fontScale")]
	public float FontScale { get; set; } = 1f;

	[JsonPropertyName("showAffectsGameplayColumn")]
	public bool ShowAffectsGameplayColumn { get; set; }

	[JsonPropertyName("showRandomModButton")]
	public bool ShowRandomModButton { get; set; }

	[JsonPropertyName("showVanillaLaunchButton")]
	public bool ShowVanillaLaunchButton { get; set; }

	[JsonPropertyName("acknowledgedUpdateRevision")]
	public int AcknowledgedUpdateRevision { get; set; }

	[JsonPropertyName("groupListView")]
	public bool GroupListView { get; set; }

	[JsonPropertyName("filtersCollapsed")]
	public bool FiltersCollapsed { get; set; }

	[JsonPropertyName("keepDisabledInPlace")]
	public bool KeepDisabledInPlace { get; set; }

	[JsonPropertyName("collapsedGroups")]
	public List<string> CollapsedGroups { get; set; } = new List<string>();

	[JsonPropertyName("cloudSync")]
	public bool CloudSync { get; set; } = true;
}
