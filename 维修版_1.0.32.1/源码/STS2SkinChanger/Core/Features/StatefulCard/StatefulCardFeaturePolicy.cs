namespace STS2SkinChanger.Core.Features.StatefulCard;

/// <summary>带状态卡图模块要不要在位：装了任何带状态卡图的包（卡图选项带 StatefulArt 合同）才需要。</summary>
internal static class StatefulCardFeaturePolicy
{
    internal static bool? ShouldBeActive(IEnumerable<bool>? optionsHaveStatefulArt) =>
        optionsHaveStatefulArt?.Any(hasStatefulArt => hasStatefulArt);
}
