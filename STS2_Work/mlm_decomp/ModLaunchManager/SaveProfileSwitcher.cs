using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace ModLaunchManager;

internal static class SaveProfileSwitcher
{
	internal const int MaxProfileCount = 3;

	private static int? _requestedProfileId;

	internal static bool Available { get; private set; }

	internal static bool Install(Harmony harmony)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_0098: Expected O, but got Unknown
		Available = false;
		try
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(SaveManager), "InitProfileId", new Type[1] { typeof(int?) }, (Type[])null);
			MethodInfo methodInfo2 = AccessTools.Method(typeof(SaveProfileSwitcher), "BeforeInitProfileId", (Type[])null, (Type[])null);
			MethodInfo methodInfo3 = AccessTools.Method(typeof(SaveProfileSwitcher), "AfterInitProfileId", (Type[])null, (Type[])null);
			if (methodInfo == null || methodInfo2 == null || methodInfo3 == null)
			{
				throw new MissingMemberException("SaveManager.InitProfileId or profile switch hooks are missing");
			}
			harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(methodInfo2), new HarmonyMethod(methodInfo3), (HarmonyMethod)null, (HarmonyMethod)null);
			Available = true;
			return true;
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} save-profile switch hook failed: {value}");
			return false;
		}
	}

	internal static void Request(int profileId)
	{
		if (!Available)
		{
			_requestedProfileId = null;
			return;
		}
		if ((profileId < 1 || profileId > 3) ? true : false)
		{
			throw new ArgumentOutOfRangeException("profileId", profileId, $"Game save profile must be between 1 and {3}.");
		}
		_requestedProfileId = profileId;
		GD.Print($"{"[ModLaunchManager]"} requested game save profile: {profileId}.");
	}

	internal static void CancelRequest()
	{
		_requestedProfileId = null;
	}

	internal static int? ReadCurrentProfileId()
	{
		try
		{
			string profileSavePath = GetProfileSavePath();
			if (string.IsNullOrWhiteSpace(profileSavePath))
			{
				GD.PushWarning("[ModLaunchManager] profile save path API was not found.");
				return null;
			}
			string fullPath = Path.GetFullPath(ProjectSettings.GlobalizePath(UserDataPathProvider.GetAccountScopedBasePath(profileSavePath, (PlatformType?)null, (ulong?)null)));
			if (!File.Exists(fullPath))
			{
				return null;
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(fullPath, Encoding.UTF8));
			JsonElement value;
			int value2 = default(int);
			bool flag = !jsonDocument.RootElement.TryGetProperty("last_profile_id", out value) || !value.TryGetInt32(out value2);
			if (!flag)
			{
				bool flag2 = ((value2 < 1 || value2 > 3) ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				return null;
			}
			return value2;
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not read the current game save profile: " + ex.Message);
			return null;
		}
	}

	private static string? GetProfileSavePath()
	{
		if (AccessTools.Property(typeof(ProfileSaveManager), "ProfilePath")?.GetValue(null) is string text && !string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		MethodInfo methodInfo = AccessTools.Method(typeof(ProfileSaveManager), "GetProfileSavePath", (Type[])null, (Type[])null);
		if (methodInfo == null)
		{
			return null;
		}
		return methodInfo.Invoke(null, methodInfo.GetParameters().Length switch
		{
			0 => Array.Empty<object>(), 
			1 => new object[1] { true }, 
			_ => throw new MissingMethodException("ProfileSaveManager.GetProfileSavePath has an unsupported signature"), 
		}) as string;
	}

	private static void BeforeInitProfileId(ref int? profileId)
	{
		if (_requestedProfileId.HasValue)
		{
			profileId = null;
		}
	}

	private static void AfterInitProfileId()
	{
		int? requestedProfileId = _requestedProfileId;
		_requestedProfileId = null;
		if (!requestedProfileId.HasValue)
		{
			return;
		}
		try
		{
			SaveManager instance = SaveManager.Instance;
			if (instance.IsProfileInitialized && instance.CurrentProfileId != requestedProfileId.Value)
			{
				instance.SwitchProfileId(requestedProfileId.Value);
				GD.Print($"{"[ModLaunchManager]"} switched to game save profile {requestedProfileId.Value}.");
			}
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not switch to game save profile {requestedProfileId.Value}: {value}");
		}
	}
}
