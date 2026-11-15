using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace STS2SkinChanger.Core.GameApi;

internal static class MerchantRoomApi
{
    internal const string StandardScenePath = "res://scenes/rooms/standard_merchant_room.tscn";
    internal const string BlitzScenePath = "res://scenes/rooms/blitz_merchant_room.tscn";
    private const string StandardTypeName = "MegaCrit.Sts2.Core.Nodes.Rooms.NStandardMerchantRoom";
    private const string BlitzTypeName = "MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantRoom";
    private static readonly Dictionary<Type, FieldInfo?> MerchantButtonFields = [];

    internal static string ResolveMerchantRoomScenePath(NMerchantRoom? room) =>
        room != null && string.Equals(room.GetType().Name, "NBlitzMerchantRoom", StringComparison.Ordinal)
            ? BlitzScenePath
            : StandardScenePath;

    internal static FieldInfo? ResolveMerchantButtonField(object? instance)
    {
        if (instance == null)
        {
            return null;
        }

        var type = instance.GetType();
        lock (MerchantButtonFields)
        {
            if (MerchantButtonFields.TryGetValue(type, out var cached))
            {
                return cached;
            }

            FieldInfo? field = null;
            for (var current = type; current != null && field == null; current = current.BaseType)
            {
                field = AccessTools.Field(current, "<MerchantButton>k__BackingField") ??
                        AccessTools.Field(current, "_merchantButton");
            }

            MerchantButtonFields[type] = field;
            return field;
        }
    }

    internal static IEnumerable<MethodInfo> ConcreteReadyMethods()
    {
        foreach (var typeName in new[] { StandardTypeName, BlitzTypeName })
        {
            var type = AccessTools.TypeByName(typeName);
            var ready = type == null ? null : AccessTools.Method(type, "_Ready");
            if (ready != null)
            {
                yield return ready;
            }
        }
    }
}
