using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class ProviderCompatibilityReport
{
	public string Target { get; init; } = string.Empty;

	public List<string> Changes { get; } = new List<string>();

	public List<string> UnresolvedReferences { get; } = new List<string>();

	public int UncheckedGenericReferences { get; set; }

	public string? Failure { get; set; }

	public override string ToString()
	{
		return $"目标 {Target}；转换 {Changes.Count} 项；未解析 {UnresolvedReferences.Count} 项；泛型引用未验证 {UncheckedGenericReferences} 项；失败：{Failure ?? "无"}\n" + string.Join("\n", Changes.Concat(UnresolvedReferences.Select((string item) => "未处理：" + item)));
	}
}
