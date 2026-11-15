using System.Reflection;
using System.Runtime.CompilerServices;

namespace STS2SkinChanger.Core;

/// <summary>
/// Third-party skin DLLs commonly cache the resources they swapped (a hurt/less-clothed skeleton
/// per monster, a palette per character…). Those caches are keyed by the *canonical* resource path
/// and survive a skin switch, so after the player picks another skin the pack keeps re-applying the
/// previous skin's atlas: the model then renders with mismatched regions ("纹理出问题") until the
/// game is restarted.
///
/// Example: `汐の雨い 半血爆衣 csxjt&ghxjt` (HalfHealthSkin) keeps
/// `_hurtSkelCache: Dictionary&lt;string, MegaSkeletonDataResource&gt;` and
/// `_appliedMap: ConditionalWeakTable&lt;Creature, object&gt;`. Once populated, every later battle
/// reuses the old atlas page even though the selection changed.
///
/// Clearing the *contents* of those static collections (rather than replacing the fields, which are
/// readonly) makes the pack reload from the currently mounted skin on its next callback.
/// </summary>
internal static class ProviderRuntimeCacheReset
{
    private static readonly List<CacheField> KnownFields = [];

    private sealed record CacheField(Type Owner, FieldInfo Field, MethodInfo Clear);

    internal static void ResetForSelections(IEnumerable<string?> optionIds)
    {
        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            return;
        }

        foreach (var optionId in optionIds
                     .Where(id => !string.IsNullOrWhiteSpace(id))
                     .Select(id => id!)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string providerId;
            try
            {
                providerId = catalog.ResolveVisualProviderId(optionId);
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(providerId))
            {
                continue;
            }

            Assembly? assembly;
            try
            {
                assembly = ManagedSkinModLoader.GetActiveProviderAssembly(providerId) ??
                           ManagedSkinModLoader.GetActiveProviderAssembly(
                               ManagedSkinModLoader.ResolveBehaviorProviderId(providerId));
            }
            catch
            {
                assembly = null;
            }

            if (assembly == null)
            {
                continue;
            }

            var cleared = ClearCaches(assembly);
            if (cleared > 0)
            {
                ModLog.Info(
                    $"[外观一致性] 已清空 {providerId} 的 {cleared} 个运行时资源缓存：" +
                    "换肤后它会按当前皮肤重新读取，不会继续用上一套纹理。");
            }
        }
    }

    private static int ClearCaches(Assembly assembly)
    {
        var cleared = 0;
        foreach (var cache in Discover(assembly))
        {
            try
            {
                var instance = cache.Field.GetValue(null);
                if (instance == null)
                {
                    continue;
                }

                cache.Clear.Invoke(instance, null);
                cleared++;
            }
            catch (Exception exception)
            {
                ModLog.Warn(
                    $"[外观一致性] 清空 {cache.Owner.Name}.{cache.Field.Name} 失败：" +
                    exception.GetBaseException().Message);
            }
        }

        return cleared;
    }

    private static IEnumerable<CacheField> Discover(Assembly assembly)
    {
        lock (KnownFields)
        {
            if (KnownFields.Count > 0 && KnownFields[0].Owner.Assembly == assembly)
            {
                return KnownFields;
            }

            KnownFields.Clear();
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type != null)!;
            }
            catch
            {
                return KnownFields;
            }

            foreach (var type in types)
            {
                FieldInfo[] fields;
                try
                {
                    fields = type.GetFields(
                        BindingFlags.Static |
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.DeclaredOnly);
                }
                catch
                {
                    continue;
                }

                foreach (var field in fields)
                {
                    if (!IsCacheField(field))
                    {
                        continue;
                    }

                    var clear = field.FieldType.GetMethod("Clear", Type.EmptyTypes);
                    if (clear == null)
                    {
                        continue;
                    }

                    KnownFields.Add(new CacheField(type, field, clear));
                }
            }

            return KnownFields;
        }
    }

    private static bool IsCacheField(FieldInfo field)
    {
        if (field.IsLiteral)
        {
            return false;
        }

        var type = field.FieldType;
        if (!type.IsGenericType)
        {
            return false;
        }

        var definition = type.GetGenericTypeDefinition();
        if (definition != typeof(Dictionary<,>) &&
            definition != typeof(ConditionalWeakTable<,>) &&
            definition != typeof(HashSet<>))
        {
            return false;
        }

        var name = field.Name;
        return name.Contains("cache", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("applied", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("hurt", StringComparison.OrdinalIgnoreCase);
    }
}
