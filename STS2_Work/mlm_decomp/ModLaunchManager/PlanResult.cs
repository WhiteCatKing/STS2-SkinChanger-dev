namespace ModLaunchManager;

internal readonly record struct PlanResult(bool Success, string Message, bool Caution = false)
{
	internal static PlanResult Ok(string message)
	{
		return new PlanResult(Success: true, message);
	}

	internal static PlanResult Warning(string message)
	{
		return new PlanResult(Success: true, message, Caution: true);
	}

	internal static PlanResult Error(string message)
	{
		return new PlanResult(Success: false, message);
	}
}
