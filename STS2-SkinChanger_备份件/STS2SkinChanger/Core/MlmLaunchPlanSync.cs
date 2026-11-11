using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;

namespace STS2SkinChanger.Core;

/// <summary>
/// Mod Launch Manager (MLM) shows its own remembered launch plan before the game loads mods, and
/// that plan is a separate file from the game's <c>SettingsSave.ModSettings.ModList</c>. When Skin
/// Changer moves itself in the saved game order, MLM would keep showing the old order and write it
/// back on the next "continue", which makes the load-order prompt loop forever.
///
/// After Skin Changer changes the saved order, rewrite MLM's canonical plan file so its launcher
/// starts from the same order the game just saved. MLM is not modified; only its documented plan
/// file is synchronized.
/// </summary>
internal static class MlmLaunchPlanSync
{
    private const string ManagerId = "ModLaunchManager";
    private const string PlanFileName = "mod_launch_last_plan.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static void SyncFromSettings(string reason)
    {
        try
        {
            if (!ModManager.Mods.Any(mod =>
                    string.Equals(mod.manifest?.id, ManagerId, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            var modSettings = SaveManager.Instance?.SettingsSave?.ModSettings;
            if (modSettings == null) return;
            var live = ModManager.Mods.ToList();
            var ordered = OrderLikeSettings(live, modSettings.ModList);
            if (ordered.Count == 0) return;

            var userRoot = ProjectSettings.GlobalizePath("user://");
            var managerRoot = Path.Combine(userRoot, "ModLaunchManager");
            if (!Directory.Exists(managerRoot)) return;

            var files = Directory.GetFiles(managerRoot, PlanFileName, SearchOption.AllDirectories);
            var written = 0;
            foreach (var path in files)
            {
                try
                {
                    WritePlan(path, ordered, modSettings.ModList);
                    written++;
                }
                catch (Exception exception)
                {
                    ModLog.Warn($"同步 MLM 排序文件失败（{path}）：{exception.GetBaseException().Message}");
                }
            }

            if (written > 0)
            {
                ModLog.Info($"已按游戏保存的排序同步 MLM 启动计划（{written} 个文件，原因：{reason}）。");
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("同步 MLM 启动计划失败：" + exception.GetBaseException().Message);
        }
    }

    private static List<Mod> OrderLikeSettings(IReadOnlyList<Mod> live, IEnumerable<SettingsSaveMod> saved)
    {
        var byKey = new Dictionary<string, Mod>(StringComparer.OrdinalIgnoreCase);
        foreach (var mod in live)
        {
            byKey.TryAdd(Key(mod.manifest?.id, mod.modSource), mod);
        }

        var ordered = new List<Mod>();
        foreach (var entry in saved)
        {
            if (byKey.TryGetValue(Key(entry.Id, entry.Source), out var mod) && !ordered.Contains(mod))
            {
                ordered.Add(mod);
            }
        }

        foreach (var mod in live)
        {
            if (!ordered.Contains(mod)) ordered.Add(mod);
        }

        return ordered;
    }

    private static void WritePlan(string path, IReadOnlyList<Mod> ordered,
        IReadOnlyList<SettingsSaveMod> saved)
    {
        var enabledByKey = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in saved)
        {
            enabledByKey.TryAdd(Key(entry.Id, entry.Source), entry.IsEnabled);
        }

        var entries = new JsonArray();
        var liveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var mod in ordered)
        {
            var key = Key(mod.manifest?.id, mod.modSource);
            liveKeys.Add(key);
            var enabled = enabledByKey.TryGetValue(key, out var savedEnabled)
                ? savedEnabled
                : mod.state is not (ModLoadState.Disabled or ModLoadState.DisabledDuplicate);
            var source = mod.modSource.ToString();
            var workshopId = WorkshopItemId(mod);
            var entry = new JsonObject
            {
                ["source"] = source,
                ["id"] = mod.manifest?.id ?? string.Empty,
                ["path"] = NormalizePath(mod.path),
                ["workshop_item_id"] = workshopId,
                ["enabled"] = enabled
            };
            if (string.IsNullOrEmpty(workshopId)) entry.Remove("workshop_item_id");
            entries.Add(entry);
        }

        // Keep entries for mods that are currently not installed; MLM uses them to remember
        // disabled/unsubscribed rows instead of dropping the user's prior layout.
        try
        {
            if (File.Exists(path) &&
                JsonNode.Parse(File.ReadAllText(path, Encoding.UTF8)) is JsonObject old &&
                old["entries"] is JsonArray oldEntries)
            {
                foreach (var node in oldEntries)
                {
                    if (node is not JsonObject oldEntry) continue;
                    var key = Key(oldEntry["id"]?.GetValue<string>(),
                        Enum.TryParse<ModSource>(oldEntry["source"]?.GetValue<string>(), true, out var src)
                            ? src
                            : ModSource.None);
                    if (!liveKeys.Contains(key)) entries.Add(oldEntry.DeepClone());
                }
            }
        }
        catch
        {
            // A malformed previous file is replaced by the live order; MLM can rebuild the rest.
        }

        var root = new JsonObject
        {
            ["version"] = 1,
            ["game_version"] = (string?)null,
            ["written_utc"] = DateTimeOffset.UtcNow.ToString("O"),
            ["entries"] = entries
        };
        File.WriteAllText(path, root.ToJsonString(JsonOptions) + System.Environment.NewLine, new UTF8Encoding(false));
    }

    private static string? WorkshopItemId(Mod mod)
    {
        var normalized = NormalizePath(mod.path);
        const string marker = "/2868840/";
        var index = normalized.IndexOf(marker, StringComparison.Ordinal);
        if (index < 0) return null;
        var start = index + marker.Length;
        var end = normalized.IndexOf('/', start);
        var value = end < 0 ? normalized[start..] : normalized[start..end];
        return ulong.TryParse(value, out var parsed) && parsed != 0 ? parsed.ToString() : null;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        try
        {
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/');
        }
        catch
        {
            return path.Trim().TrimEnd('\\', '/').Replace('\\', '/');
        }
    }

    private static string Key(string? id, ModSource source) => $"{id}\n{source}";
}
