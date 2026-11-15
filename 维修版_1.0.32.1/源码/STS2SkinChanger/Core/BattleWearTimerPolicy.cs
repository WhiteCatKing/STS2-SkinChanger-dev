namespace STS2SkinChanger.Core;

internal static class BattleWearTimerPolicy
{
    internal static bool IsVerifiedCallback(
        string? targetTypeName,
        string targetMethodName,
        string? callbackTypeName,
        string callbackMethodName,
        bool isStaticVoid,
        IReadOnlyList<string?> parameterTypeNames) =>
        targetTypeName == "MegaCrit.Sts2.Core.Nodes.NRun" &&
        targetMethodName == "_Ready" &&
        callbackTypeName == "BattleWear.BattleWearTimerPatch" &&
        callbackMethodName == "Postfix" &&
        isStaticVoid &&
        parameterTypeNames.Count == 1 &&
        parameterTypeNames[0] == "MegaCrit.Sts2.Core.Nodes.NRun";
}
