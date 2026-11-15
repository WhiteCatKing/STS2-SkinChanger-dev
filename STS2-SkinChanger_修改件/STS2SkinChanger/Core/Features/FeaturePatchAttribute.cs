using System;

namespace STS2SkinChanger.Core.Features;

/// <summary>
/// 标记一个 Harmony 补丁类归属哪个功能模块。带这个标记的类不再由 <c>Entry.InstallPatchClasses</c>
/// 逐个安装，而是交给 <see cref="FeatureRegistry"/> 按模块成组装卸：模块内部失败整套回滚，
/// 模块之间互不牵连。
///
/// 只标真正属于某个模块的补丁；入口、宿主、联机、兼容桥这些跨模块的补丁不加标记，保持常驻。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class FeaturePatchAttribute : Attribute
{
    internal FeaturePatchAttribute(string featureId)
    {
        FeatureId = featureId;
    }

    /// <summary>见 <see cref="FeatureIds"/>；必须在它的集合里，否则按拼错处理。</summary>
    internal string FeatureId { get; }
}
