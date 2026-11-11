using System;
using Godot;

namespace ModLaunchManager;

internal static class NativeFileDialogs
{
	internal static bool Available
	{
		get
		{
			try
			{
				return DisplayServer.HasFeature((Feature)25);
			}
			catch
			{
				return false;
			}
		}
	}

	internal static bool TryShow(bool save, string title, string directory, string fileName, Action<string> onPicked, out string error)
	{
		return TryShow(title, directory, fileName, (FileDialogMode)(save ? 4 : 0), new string[1] { "*.json ; JSON" }, onPicked, out error);
	}

	internal static bool TryShow(string title, string directory, string fileName, FileDialogMode mode, string[] filters, Action<string> onPicked, out string error)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		error = string.Empty;
		if (!Available)
		{
			error = MlmLocManager.Get("transfer.file.dialogUnavailable");
			return false;
		}
		try
		{
			Error val = DisplayServer.FileDialogShow(title, directory, fileName, false, mode, filters, Callable.From<bool, string[], long>((Action<bool, string[], long>)delegate(bool status, string[] paths, long _)
			{
				if (status && paths != null && paths.Length > 0 && !string.IsNullOrWhiteSpace(paths[0]))
				{
					onPicked(paths[0]);
				}
			}));
			if ((int)val != 0)
			{
				error = ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString();
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			GD.PushWarning($"{"[ModLaunchManager]"} native file dialog failed: {ex}");
			return false;
		}
	}
}
