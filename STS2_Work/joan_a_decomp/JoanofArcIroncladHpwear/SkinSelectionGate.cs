using System;
using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Runs;

namespace JoanofArcIroncladHpwear;

internal static class SkinSelectionGate
{
	private const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static Type? service;

	private static MethodInfo? selected;

	private static MethodInfo? direct;

	private static MethodInfo? playerScope;

	private static bool managerPresent;

	private static long searchAfter;

	static SkinSelectionGate()
	{
		AppDomain.CurrentDomain.AssemblyLoad += delegate
		{
			searchAfter = 0L;
		};
	}

	private static bool HasManager()
	{
		if (service != null)
		{
			return true;
		}
		if (Environment.TickCount64 < searchAfter)
		{
			return managerPresent;
		}
		searchAfter = Environment.TickCount64 + 1000;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			Type type = assembly.GetType("STS2SkinChanger.Core.SkinService", throwOnError: false);
			if (type == null)
			{
				string name = assembly.GetName().Name;
				if ((name == "Gurio.SkinChanger" || name == "STS2SkinChanger") ? true : false)
				{
					managerPresent = true;
				}
				continue;
			}
			managerPresent = true;
			service = type;
			selected = service.GetMethod("GetSelectedRuntimeProvider", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null);
			direct = service.GetMethod("IsDirectCharacterRuntimeProviderSelected", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(string),
				typeof(string)
			}, null);
			playerScope = assembly.GetType("STS2SkinChanger.Core.MultiplayerSkinSync", throwOnError: false)?.GetMethod("BeginPlayerSelectionScope", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ulong) }, null);
			break;
		}
		return managerPresent;
	}

	private static bool Query(bool perPlayer, ulong id)
	{
		if (!HasManager())
		{
			return true;
		}
		if (selected == null || direct == null || (perPlayer && playerScope == null))
		{
			return false;
		}
		try
		{
			using (perPlayer ? (playerScope.Invoke(null, new object[1] { id }) as IDisposable) : null)
			{
				string text = selected.Invoke(null, new object[1] { "ironclad" }) as string;
				if (string.IsNullOrEmpty(text) || (!text.Equals("JoanofArcIroncladHpwear", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("JoanofArcIroncladHpwear::source:", StringComparison.OrdinalIgnoreCase)))
				{
					return false;
				}
				int result;
				if (!perPlayer)
				{
					result = 1;
				}
				else
				{
					object obj = direct.Invoke(null, new object[2] { text, "ironclad" });
					result = ((obj is bool && (bool)obj) ? 1 : 0);
				}
				return (byte)result != 0;
			}
		}
		catch
		{
			return false;
		}
	}

	internal static bool MenuAllowed()
	{
		return Query(perPlayer: false, 0uL);
	}

	internal static bool AllowPlayer(ulong id)
	{
		return Query(perPlayer: true, id);
	}

	internal static bool PlayerAllowed(Player? player)
	{
		if (HasManager())
		{
			if (player != null)
			{
				return AllowPlayer(player.NetId);
			}
			return false;
		}
		return true;
	}

	internal static bool SceneAllowed()
	{
		if (!HasManager() || MenuAllowed())
		{
			return true;
		}
		try
		{
			RunManager instance = RunManager.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				RunState obj2 = instance.DebugOnlyGetState();
				obj = ((obj2 != null) ? obj2.Players : null);
			}
			IReadOnlyList<Player> readOnlyList = (IReadOnlyList<Player>)obj;
			if (readOnlyList != null)
			{
				foreach (Player item in readOnlyList)
				{
					if (((item != null) ? item.Character : null) is Ironclad && AllowPlayer(item.NetId))
					{
						return true;
					}
				}
			}
		}
		catch
		{
		}
		return false;
	}
}
