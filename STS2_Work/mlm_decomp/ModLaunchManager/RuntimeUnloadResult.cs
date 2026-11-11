using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal readonly record struct RuntimeUnloadResult(bool Success, int RemovedHarmonyPatches, int ClearedGameRegistrations, string? ErrorKey, string? ErrorDetail)
{
	internal static RuntimeUnloadResult NothingToDo => new RuntimeUnloadResult(Success: true, 0, 0, null, null);

	internal bool RequiresRestart
	{
		get
		{
			switch (ErrorKey)
			{
			case "loading.error.unload.notCollectible":
			case "loading.error.unload.foreignContext":
			case "loading.error.unload.strayAssembly":
				return true;
			default:
				return false;
			}
		}
	}

	internal static RuntimeUnloadResult Failure(string key, string? detail = null)
	{
		return new RuntimeUnloadResult(Success: false, 0, 0, key, detail);
	}

	internal string Describe()
	{
		if (ErrorKey != null)
		{
			return MlmLocManager.Get(ErrorKey, ("detail", ErrorDetail ?? string.Empty));
		}
		return string.Empty;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append("Success = ");
		builder.Append(Success.ToString());
		builder.Append(", RemovedHarmonyPatches = ");
		builder.Append(RemovedHarmonyPatches.ToString());
		builder.Append(", ClearedGameRegistrations = ");
		builder.Append(ClearedGameRegistrations.ToString());
		builder.Append(", ErrorKey = ");
		builder.Append((object?)ErrorKey);
		builder.Append(", ErrorDetail = ");
		builder.Append((object?)ErrorDetail);
		return true;
	}
}
