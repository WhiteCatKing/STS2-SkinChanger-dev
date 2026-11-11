namespace STS2SkinChanger.Core;

internal sealed record ProviderRuntimeAudit(bool CanRun, int PreparedMethods, string Failure)
{
	public string Describe()
	{
		if (!CanRun)
		{
			return Failure;
		}
		return $"已核验 {PreparedMethods} 个方法可在当前游戏运行";
	}
}
