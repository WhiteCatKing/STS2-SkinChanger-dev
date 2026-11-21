using STS2SkinChanger.Core;

internal static class DirectCharacterRuntimeTargetPolicyTests
{
    public static void Run()
    {
        var known = new[] { "ironclad", "necrobinder" };

        Require(
            DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField("CharacterKey"),
            "CharacterKey 是整包自绘角色皮肤最常用的声明字段名，必须能识别。");
        Require(
            DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField("TargetCharacterId") &&
            DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField("SkinCharacter"),
            "已有的 TargetCharacterId / SkinCharacter 写法必须继续识别。");
        Require(
            !DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField("Id") &&
            !DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField("Prefix") &&
            !DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField("SettingsPath") &&
            !DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField(null),
            "与角色无关的字段不能被当成目标角色声明，否则任何整包都会被按皮肤接管。");

        var ironclad = DirectCharacterRuntimeTargetPolicy.ResolveTargets(
            new[] { ("CharacterKey", "ironclad") },
            known);
        Require(
            ironclad.Count == 1 && ironclad.Contains("ironclad"),
            "CharacterKey=ironclad 应解析出 ironclad 角色组。");

        var necrobinder = DirectCharacterRuntimeTargetPolicy.ResolveTargets(
            new[] { ("CharacterKey", "necrobinder") },
            known);
        Require(
            necrobinder.Count == 1 && necrobinder.Contains("necrobinder"),
            "CharacterKey=necrobinder 应解析出 necrobinder 角色组。");

        var unknownValue = DirectCharacterRuntimeTargetPolicy.ResolveTargets(
            new[] { ("CharacterKey", "redhood") },
            known);
        Require(
            unknownValue.Count == 0,
            "值不是已知角色组时不能当成声明，否则会把包挂到不存在的角色上。");

        var unrelatedField = DirectCharacterRuntimeTargetPolicy.ResolveTargets(
            new[] { ("Prefix", "ironclad") },
            known);
        Require(
            unrelatedField.Count == 0,
            "即使值等于角色 id，字段名与角色无关时也不能当成声明。");

        var compatFallback = DirectCharacterRuntimeTargetPolicy.ResolveTargets(
            new[] { ("CharacterKey", "ironclad"), ("DefaultCharacterId", "necrobinder") },
            known);
        Require(
            compatFallback.Count == 1 && compatFallback.Contains("ironclad"),
            "名字带 default 的兼容字段是「没选中时退回谁」，不能把完整皮肤串进另一个角色列表。");
    }

    private static void Require(bool condition, string reason)
    {
        if (!condition) throw new InvalidOperationException(reason);
    }
}
