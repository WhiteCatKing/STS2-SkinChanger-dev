using System.Collections.Generic;

namespace STS2SkinChanger.Core.Features;

/// <summary>
/// 功能模块的固定 id。补丁类用 <see cref="FeaturePatchAttribute"/> 声明归属，注册表按这个 id
/// 分组安装，一个模块装失败只回滚它自己。
///
/// 命名与目录一一对应：<c>Core\Features\&lt;目录&gt;\</c>。改 id 等于改目录名，两边必须同时改。
/// </summary>
internal static class FeatureIds
{
    internal const string Character = "character";
    internal const string Monster = "monster";
    internal const string Merchant = "merchant";
    internal const string Event = "event";
    internal const string Ancient = "ancient";
    internal const string Card = "card";
    internal const string StatefulCard = "stateful-card";
    internal const string Background = "background";

    /// <summary>全部已分区的模块。不在这里的 id 视为拼错，补丁仍按旧的全量方式安装。</summary>
    internal static readonly IReadOnlySet<string> All = new HashSet<string>(System.StringComparer.Ordinal)
    {
        Character,
        Monster,
        Merchant,
        Event,
        Ancient,
        Card,
        StatefulCard,
        Background,
    };
}
