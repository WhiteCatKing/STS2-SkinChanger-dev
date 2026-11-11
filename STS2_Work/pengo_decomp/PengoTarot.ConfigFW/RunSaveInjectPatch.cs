using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace PengoTarot.ConfigFW;

public static class RunSaveInjectPatch
{
	private sealed class InMemoryRunSnapshot(JsonObject data)
	{
		public JsonObject Data { get; } = data;
	}

	[HarmonyPatch(typeof(RunManager), "SetUpNewSingleplayer")]
	public static class SetUpNewSingleplayerPatch
	{
		public static void Postfix()
		{
			ConfigFloatingWindowRunData.SnapshotFromDefaults();
			ConfigFloatingWindowRunData.SetSaveConfigFromRunData();
		}
	}

	[HarmonyPatch(typeof(RunManager), "SetUpNewMultiplayer")]
	public static class SetUpNewMultiplayerPatch
	{
		public static void Postfix(StartRunLobby lobby)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			ConfigFloatingWindowRunData.SetSaveConfigFromRunData();
			if ((int)lobby.NetService.Type == 2)
			{
				lobby.NetService.SendMessage<ConfigFloatingWindowDataMessage>(ConfigFloatingWindowDataMessage.FromRunData());
			}
		}
	}

	[HarmonyPatch(typeof(NCharacterSelectScreen), "OnEmbarkPressed")]
	public static class CharacterSelectEmbarkPatch
	{
		public static void Postfix(NCharacterSelectScreen __instance)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			StartRunLobby lobby = __instance.Lobby;
			if (((lobby != null) ? lobby.NetService : null) != null && (int)lobby.NetService.Type == 2)
			{
				lobby.NetService.SendMessage<ConfigFloatingWindowDataMessage>(ConfigFloatingWindowDataMessage.FromRunData());
			}
		}
	}

	[HarmonyPatch(typeof(RunManager), "SetUpSavedSingleplayer")]
	public static class SetUpSavedSingleplayerPatch
	{
		public static void Prefix(SerializableRun save)
		{
			if (!TryRestoreInMemorySnapshot(save))
			{
				RunSaveManager runSaveManager = GetRunSaveManager(SaveManager.Instance);
				if (runSaveManager != null)
				{
					ExtractFromSave(runSaveManager, success: true, "current_run.save");
				}
			}
		}
	}

	[HarmonyPatch(typeof(RunManager), "SetUpSavedMultiplayer")]
	public static class SetUpSavedMultiplayerPatch
	{
		public static void Prefix(LoadRunLobby lobby)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			if ((int)lobby.NetService.Type == 2 && !TryRestoreInMemorySnapshot(lobby.Run))
			{
				RunSaveManager runSaveManager = GetRunSaveManager(SaveManager.Instance);
				if (runSaveManager != null)
				{
					ExtractFromSave(runSaveManager, success: true, "current_run_mp.save");
				}
			}
		}

		public static void Postfix(LoadRunLobby lobby)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			if ((int)lobby.NetService.Type == 2)
			{
				lobby.NetService.SendMessage<ConfigFloatingWindowDataMessage>(ConfigFloatingWindowDataMessage.FromRunData());
			}
		}
	}

	[HarmonyPatch(typeof(RunManager), "ToSave")]
	public static class ToSavePatch
	{
		public static void Postfix(SerializableRun __result)
		{
			if (__result != null && ConfigFloatingWindowRunData.SaveConfig != null)
			{
				InMemorySnapshots.Remove(__result);
				InMemorySnapshots.Add(__result, new InMemoryRunSnapshot((JsonObject)ConfigFloatingWindowRunData.ToJson().DeepClone()));
			}
		}
	}

	[HarmonyPatch(typeof(GodotFileIo), "WriteFile", new Type[]
	{
		typeof(string),
		typeof(byte[])
	})]
	public static class WriteFilePatch
	{
		public static void Prefix(string path, ref byte[] bytes)
		{
			bytes = TryInjectRunData(path, bytes);
		}
	}

	[HarmonyPatch(typeof(GodotFileIo), "WriteFileAsync", new Type[]
	{
		typeof(string),
		typeof(byte[])
	})]
	public static class WriteFileAsyncPatch
	{
		public static void Prefix(string path, ref byte[] bytes)
		{
			bytes = TryInjectRunData(path, bytes);
		}
	}

	[HarmonyPatch(typeof(CloudSaveStore), "WriteFile", new Type[]
	{
		typeof(string),
		typeof(byte[])
	})]
	public static class CloudSaveStoreWriteFilePatch
	{
		public static void Prefix(string path, ref byte[] bytes)
		{
			bytes = TryInjectRunData(path, bytes);
		}
	}

	[HarmonyPatch(typeof(CloudSaveStore), "WriteFileAsync", new Type[]
	{
		typeof(string),
		typeof(byte[])
	})]
	public static class CloudSaveStoreWriteFileAsyncPatch
	{
		public static void Prefix(string path, ref byte[] bytes)
		{
			bytes = TryInjectRunData(path, bytes);
		}
	}

	[HarmonyPatch(typeof(RunSaveManager), "LoadRunSave")]
	public static class LoadRunSavePatch
	{
		public static void Postfix(RunSaveManager __instance, ReadSaveResult<SerializableRun> __result)
		{
			if (!RunManager.Instance.IsInProgress)
			{
				ExtractFromSave(__instance, __result.Success && __result.SaveData != null, "current_run.save");
			}
		}
	}

	[HarmonyPatch(typeof(RunSaveManager), "LoadMultiplayerRunSave")]
	public static class LoadMultiplayerRunSavePatch
	{
		public static void Postfix(RunSaveManager __instance, ReadSaveResult<SerializableRun> __result)
		{
			if (!RunManager.Instance.IsInProgress)
			{
				ExtractFromSave(__instance, __result.Success && __result.SaveData != null, "current_run_mp.save");
			}
		}
	}

	private static readonly ConditionalWeakTable<SerializableRun, InMemoryRunSnapshot> InMemorySnapshots = new ConditionalWeakTable<SerializableRun, InMemoryRunSnapshot>();

	private static bool TryRestoreInMemorySnapshot(SerializableRun save)
	{
		if (!InMemorySnapshots.TryGetValue(save, out InMemoryRunSnapshot value))
		{
			return false;
		}
		ConfigFloatingWindowRunData.FromJson((JsonObject)value.Data.DeepClone());
		ConfigFloatingWindowRunData.SetSaveConfigFromRunData();
		return true;
	}

	private static byte[] TryInjectRunData(string path, byte[] bytes)
	{
		string fileName = Path.GetFileName(path);
		if (fileName != "current_run.save" && fileName != "current_run_mp.save")
		{
			return bytes;
		}
		try
		{
			if (!(JsonNode.Parse(Encoding.UTF8.GetString(bytes), null) is JsonObject jsonObject))
			{
				return bytes;
			}
			if (ConfigFloatingWindowRunData.SaveConfig == null)
			{
				return bytes;
			}
			JsonObject jsonObject2 = ConfigFloatingWindowRunData.ToJson();
			JsonNode jsonNode = jsonObject["_pengotarot_cfw"];
			if (jsonNode != null && JsonNode.DeepEquals(jsonNode, jsonObject2))
			{
				return bytes;
			}
			jsonObject["_pengotarot_cfw"] = jsonObject2;
			return Encoding.UTF8.GetBytes(jsonObject.ToJsonString());
		}
		catch
		{
			return bytes;
		}
	}

	private static void ExtractFromSave(RunSaveManager manager, bool success, string fileName)
	{
		if (!success)
		{
			return;
		}
		try
		{
			string runSavePath = RunSaveManager.GetRunSavePath(SaveManager.Instance.CurrentProfileId, fileName, (bool?)null);
			ISaveStore saveStore = GetSaveStore(manager);
			if (saveStore == null)
			{
				return;
			}
			string text = saveStore.ReadFile(runSavePath);
			if (!string.IsNullOrEmpty(text) && JsonNode.Parse(text, null) is JsonObject jsonObject)
			{
				if (jsonObject["_pengotarot_cfw"] is JsonObject obj)
				{
					ConfigFloatingWindowRunData.FromJson(obj);
				}
				else if (ConfigFloatingWindowRunData.SaveConfig == null)
				{
					ConfigFloatingWindowRunData.Reset();
				}
				ConfigFloatingWindowRunData.SetSaveConfigFromRunData();
			}
		}
		catch
		{
		}
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_saveStore")]
	private static extern ref ISaveStore GetSaveStore(RunSaveManager manager);

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_runSaveManager")]
	private static extern ref RunSaveManager GetRunSaveManager(SaveManager manager);
}
