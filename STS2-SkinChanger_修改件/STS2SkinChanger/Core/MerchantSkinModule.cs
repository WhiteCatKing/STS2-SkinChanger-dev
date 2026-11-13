using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace STS2SkinChanger.Core;

/// <summary>
/// 商人皮肤的独立注册表。商人（含假商人）不参与角色/怪物的分组继承与场景重建：
/// 这里只做三件事——发现包、判定它是否面向商人、给下拉提供候选项。
/// 激活/停用由 <see cref="MerchantSkinModule.Activate"/> / <see cref="MerchantSkinModule.Deactivate"/> 负责，
/// 原则是「让包自己的 DLL 补丁生效并保持」，我们绝不重放 _Ready 前缀、不写位姿、不重建节点。
/// </summary>
internal static class MerchantSkinModule
{
    internal const string MerchantGroupId = "merchant";
    internal const string FakeMerchantGroupId = "fake_merchant";

    internal sealed record MerchantProvider(
        string Id,
        string DisplayName,
        string Directory,
        string? PckPath,
        string? DllPath,
        string Evidence)
    {
        internal string GroupId =>
            Id.Contains("fake", StringComparison.OrdinalIgnoreCase) ||
            DisplayName.Contains("假", StringComparison.OrdinalIgnoreCase)
                ? FakeMerchantGroupId
                : MerchantGroupId;
    }

    private static readonly List<MerchantProvider> Providers = [];
    private static bool _discovered;

    internal static IReadOnlyList<MerchantProvider> All
    {
        get { EnsureDiscovered(); return Providers; }
    }

    internal static IEnumerable<MerchantProvider> ForGroup(string groupId) =>
        All.Where(provider => string.Equals(provider.GroupId, groupId, StringComparison.OrdinalIgnoreCase));

    /// <summary>选中项 → 注册表条目；选择值可能是裸 id，也可能是 id::source:…</summary>
    internal static MerchantProvider? Resolve(string? selection)
    {
        if (string.IsNullOrWhiteSpace(selection))
        {
            return null;
        }

        var id = selection;
        var separator = id.IndexOf("::source:", StringComparison.OrdinalIgnoreCase);
        if (separator > 0)
        {
            id = id[..separator];
        }

        return All.FirstOrDefault(provider =>
            string.Equals(provider.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    internal static void EnsureDiscovered()
    {
        if (_discovered)
        {
            return;
        }

        _discovered = true;
        foreach (var root in EnumerateRoots())
        {
            IEnumerable<string> manifests;
            try
            {
                manifests = Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories)
                    .Where(path => Path.GetFileName(path).Contains("manifest", StringComparison.OrdinalIgnoreCase) ||
                                   Path.GetFileName(path).StartsWith("SkinChanger", StringComparison.OrdinalIgnoreCase))
                    .Take(4000)
                    .ToArray();
            }
            catch (Exception exception)
            {
                ModLog.Warn($"[商人注册] 扫描 {root} 失败：{exception.GetBaseException().Message}");
                continue;
            }

            foreach (var manifest in manifests)
            {
                TryRegister(manifest);
            }
        }

        ModLog.Info($"[商人注册] 共发现 {Providers.Count} 个商人皮肤包：{string.Join(", ", Providers.Select(p => p.Id))}");
    }

    private static IEnumerable<string> EnumerateRoots()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "mods");
        yield return @"D:\steam\steamapps\common\Slay the Spire 2\mods";
        yield return @"D:\steam\steamapps\workshop\content\2868840";
    }

    private static void TryRegister(string manifestPath)
    {
        string text;
        try
        {
            text = File.ReadAllText(manifestPath);
        }
        catch
        {
            return;
        }

        if (!text.Contains("\"has_pck\"", StringComparison.OrdinalIgnoreCase) &&
            !text.Contains("\"id\"", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var id = ReadJsonString(text, "id");
        if (string.IsNullOrWhiteSpace(id) || Resolve(id) != null)
        {
            return;
        }

        var directory = Path.GetDirectoryName(manifestPath);
        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        var pckName = ReadJsonString(text, "pck_name") ?? id;
        var pck = Path.Combine(directory, pckName + ".pck");
        if (!File.Exists(pck))
        {
            pck = Directory.EnumerateFiles(directory, "*.pck", SearchOption.TopDirectoryOnly).FirstOrDefault();
        }

        var dll = Path.Combine(directory, id + ".dll");
        if (!File.Exists(dll))
        {
            dll = Directory.EnumerateFiles(directory, "*.dll", SearchOption.TopDirectoryOnly).FirstOrDefault();
        }

        var evidence = CollectMerchantEvidence(pck, dll);
        if (evidence == null)
        {
            return;
        }

        var displayName = ReadJsonString(text, "name") ?? id;
        Providers.Add(new MerchantProvider(id, displayName, directory, pck, dll, evidence));
        ModLog.Info($"[商人注册] id={id} 名称={displayName} 证据={evidence} 目录={directory}");
    }

    /// <summary>
    /// 判定一个包是否面向商人：「人」的骨架/补丁证据。背景、事件美术不算证据。
    /// </summary>
    private static string? CollectMerchantEvidence(string? pck, string? dll)
    {
        if (pck != null)
        {
            var hits = ScanStrings(pck, Encoding.Latin1,
                "spine/merchant/", "merchant/npc", "merchant/hand", "merchant_skel_data");
            if (hits.Count > 0)
            {
                return "pck:" + string.Join("+", hits);
            }
        }

        if (dll != null)
        {
            var hits = ScanStrings(dll, Encoding.Unicode,
                "%MerchantVisual", "MerchantButton", "NMerchantRoom", "NMerchantCharacter");
            if (hits.Count > 0)
            {
                return "dll:" + string.Join("+", hits);
            }
        }

        return null;
    }

    private static List<string> ScanStrings(string path, Encoding encoding, params string[] needles)
    {
        var found = new List<string>();
        try
        {
            using var stream = File.OpenRead(path);
            var buffer = new byte[4 * 1024 * 1024];
            int read;
            var carry = string.Empty;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = carry + encoding.GetString(buffer, 0, read);
                foreach (var needle in needles)
                {
                    if (found.Contains(needle) || !chunk.Contains(needle, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    found.Add(needle);
                }

                carry = chunk.Length > 64 ? chunk[^64..] : chunk;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[商人注册] 读取 {Path.GetFileName(path)} 失败：{exception.GetBaseException().Message}");
        }

        return found;
    }

    private static string? ReadJsonString(string text, string key)
    {
        var marker = "\"" + key + "\"";
        var index = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var colon = text.IndexOf(':', index + marker.Length);
        if (colon < 0)
        {
            return null;
        }

        var start = text.IndexOf('"', colon + 1);
        if (start < 0)
        {
            return null;
        }

        var end = text.IndexOf('"', start + 1);
        return end < 0 ? null : text[(start + 1)..end];
    }
}
