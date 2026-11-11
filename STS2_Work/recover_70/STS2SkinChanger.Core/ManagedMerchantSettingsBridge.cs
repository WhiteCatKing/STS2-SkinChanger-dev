using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class ManagedMerchantSettingsBridge
{
	private sealed record Session(MerchantSettingsContract Contract)
	{
		public ProviderSettingsTargets<Node> Targets { get; } = new ProviderSettingsTargets<Node>();

		public bool Pending;

		public bool Applying;
	}

	private static readonly Dictionary<Assembly, Session> Sessions = new Dictionary<Assembly, Session>();

	private static readonly HashSet<Assembly> Inspected = new HashSet<Assembly>();

	private static readonly Harmony Observer = new Harmony("Gurio.SkinChanger.provider_settings");

	internal static void Install(Assembly assembly)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		if (!Inspected.Add(assembly))
		{
			return;
		}
		try
		{
			if (assembly.GetReferencedAssemblies().Any((AssemblyName reference) => reference.Name == typeof(Entry).Assembly.GetName().Name))
			{
				MerchantSettingsContract merchantSettingsContract = MerchantSettingsContract.TryCreate(assembly);
				if (!(merchantSettingsContract == null))
				{
					Session value = new Session(merchantSettingsContract);
					Sessions.Add(assembly, value);
					Observer.Patch((MethodBase)merchantSettingsContract.Save, (HarmonyMethod)null, new HarmonyMethod(typeof(ManagedMerchantSettingsBridge), "AfterSave", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
					ProviderSettingsControls.Add((AbstractConsoleCmd)Activator.CreateInstance(merchantSettingsContract.CommandType));
					ModLog.Info("已接入原商人设置：" + assembly.GetName().Name + "；原配置保留，手脚应用限定当前所选对象。");
				}
			}
		}
		catch (Exception ex)
		{
			if (Sessions.Remove(assembly, out Session value2))
			{
				Observer.Unpatch((MethodBase)value2.Contract.Save, AccessTools.Method(typeof(ManagedMerchantSettingsBridge), "AfterSave", (Type[])null, (Type[])null));
			}
			ModLog.Warn("接入原皮肤设置失败，未放开全局修改：" + ex.GetBaseException().Message);
		}
	}

	internal static bool OwnsSettingsPatch(MethodInfo callback)
	{
		return Sessions.Values.Any((Session session) => session.Contract.SettingsPostfix == callback);
	}

	internal static void Bind(Node node, string groupId, string providerId)
	{
		if ((!(node is NMerchantHand) && !(node is NMerchantInventory)) || 1 == 0)
		{
			return;
		}
		foreach (var (assembly2, session2) in Sessions)
		{
			if (ManagedSkinModLoader.IsProviderAssemblyFor(providerId, assembly2))
			{
				session2.Targets.Bind(node, groupId, providerId);
				if (IsLive(node) && SkinService.GetSelectedFullRuntimeProvider(groupId) == providerId)
				{
					ApplyNode(session2, node);
				}
			}
		}
	}

	internal static int Refresh(Assembly assembly)
	{
		if (!Sessions.TryGetValue(assembly, out Session session) || session.Applying)
		{
			return 0;
		}
		session.Applying = true;
		try
		{
			return session.Targets.Refresh(IsLive, MerchantRuntimeAppearance.ResolveMerchantGroupId, SkinService.GetSelectedFullRuntimeProvider, delegate(Node node)
			{
				ApplyNode(session, node);
			});
		}
		catch (Exception ex)
		{
			ModLog.Warn("原皮肤设置定向刷新失败：" + ex.GetBaseException().Message);
			return 0;
		}
		finally
		{
			session.Applying = false;
		}
	}

	private static bool IsLive(Node node)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)node) && !((GodotObject)node).IsQueuedForDeletion())
		{
			return node.IsInsideTree();
		}
		return false;
	}

	private static void ApplyNode(Session session, Node node)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (node is NMerchantHand)
			{
				Node parent = node.GetParent();
				if (parent != null && new MegaSprite(Variant.op_Implicit((GodotObject)(object)parent)).IsAnimationStateReady())
				{
					session.Contract.ApplyHand.Invoke(null, new object[1] { node });
					return;
				}
			}
			if (node is NMerchantInventory)
			{
				session.Contract.ApplyLegs.Invoke(null, new object[2]
				{
					node,
					!(bool)session.Contract.UseFoot.Invoke(null, null)
				});
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新当前商人内部设置失败：" + ex.GetBaseException().Message);
		}
	}

	private static void AfterSave(MethodBase __originalMethod)
	{
		RequestRefresh(__originalMethod.Module.Assembly);
	}

	internal static int RequestRefresh(Assembly assembly)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (!Sessions.TryGetValue(assembly, out Session session))
		{
			return 0;
		}
		try
		{
			int hands = 0;
			session.Targets.Refresh(IsLive, MerchantRuntimeAppearance.ResolveMerchantGroupId, SkinService.GetSelectedFullRuntimeProvider, delegate(Node node)
			{
				if (node is NMerchantHand)
				{
					hands++;
				}
			});
			if (session.Pending)
			{
				return hands;
			}
			session.Pending = true;
			Callable val = Callable.From((Action)delegate
			{
				session.Pending = false;
				Refresh(assembly);
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			return hands;
		}
		catch (Exception ex)
		{
			session.Pending = false;
			ModLog.Warn("延迟刷新皮肤内部设置失败：" + ex.GetBaseException().Message);
			return 0;
		}
	}

	internal static void FillSettings(NModInfoContainer container, Mod mod)
	{
		Session[] array = Sessions.Values.ToArray();
		foreach (Session session in array)
		{
			try
			{
				session.Contract.SettingsPostfix?.Invoke(null, new object[2] { container, mod });
			}
			catch (Exception ex)
			{
				ModLog.Warn("显示原皮肤设置失败：" + ex.GetBaseException().Message);
			}
		}
	}
}
