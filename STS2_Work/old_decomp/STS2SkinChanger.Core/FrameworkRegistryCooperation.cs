using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class FrameworkRegistryCooperation
{
	private static FrameworkRegistrySession? _session;

	private static Type? _selectorType;

	private static readonly List<WeakReference<Node>> Controls = new List<WeakReference<Node>>();

	private static readonly Dictionary<string, long> Requests = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> PendingOptions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static long _requestGeneration;

	private static bool _refreshQueued;

	private static readonly ConditionalWeakTable<FrameworkCharacterSkinContract, Dictionary<int, FrameworkCharacterSkinContract>> Filtered = new ConditionalWeakTable<FrameworkCharacterSkinContract, Dictionary<int, FrameworkCharacterSkinContract>>();

	public static bool IsActive => _session != null;

	public static bool HasRegistrationCallbacks(Assembly provider)
	{
		return _session?.HasRegistrationCallbacks(provider) ?? false;
	}

	public static void RegisterProvider(Assembly provider)
	{
		_session?.RegisterProvider(provider);
	}

	public static bool IsNativeProvider(string providerId)
	{
		if (_session != null)
		{
			return SkinService.Catalog?.Groups.SelectMany((SkinGroup group) => group.Options).Any(delegate(SkinOption option)
			{
				FrameworkCharacterSkinContract frameworkContract = option.FrameworkContract;
				return (object)frameworkContract != null && frameworkContract.ProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase) && frameworkContract.FrameworkAssemblyName == _session.Assembly.GetName().Name;
			}) ?? false;
		}
		return false;
	}

	public static void Bind(Assembly assembly)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_014a: Expected O, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Expected O, but got Unknown
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Expected O, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Expected O, but got Unknown
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Expected O, but got Unknown
		if (_session != null)
		{
			return;
		}
		FrameworkCharacterSkinContract contract;
		FrameworkRegistrySession session = new FrameworkRegistrySession(assembly, (ModelId id) => (!SkinService.TryGetSelectedFrameworkContract(Normalize(id.Entry), out contract)) ? null : contract.SkinId, delegate(ModelId id, string skinId)
		{
			RequestSelection(Normalize(id.Entry), skinId);
		});
		session.PrepareCharacters = delegate
		{
			if (!ModelDb.Contains(typeof(Ironclad)))
			{
				return;
			}
			foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
			{
				session.EnsureCharacter(((AbstractModel)allCharacter).Id);
			}
		};
		Type selector = assembly.GetType("thunninoiSkinManager.thunninoiSkinManagerCode.SkinSelector", throwOnError: true);
		MethodInfo[] array = new string[2] { "Refresh", "LoadPreview" }.Select((string name) => AccessTools.Method(selector, name, (Type[])null, (Type[])null) ?? throw new MissingMethodException(selector.FullName, name)).ToArray();
		MethodInfo[] array2 = new string[2] { "OnPrevPressed", "OnNextPressed" }.Select((string name) => AccessTools.Method(selector, name, (Type[])null, (Type[])null) ?? throw new MissingMethodException(selector.FullName, name)).ToArray();
		session.Install();
		_session = session;
		_selectorType = selector;
		Harmony val = new Harmony("Gurio.SkinChanger.native-framework-controls");
		try
		{
			MethodInfo[] array3 = array;
			foreach (MethodInfo methodInfo in array3)
			{
				val.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(FrameworkRegistryCooperation), "CanRefreshControl", (Type[])null), new HarmonyMethod(typeof(FrameworkRegistryCooperation), "UpdateLabel", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
			}
			array3 = array2;
			foreach (MethodInfo methodInfo2 in array3)
			{
				val.Patch((MethodBase)methodInfo2, new HarmonyMethod(typeof(FrameworkRegistryCooperation), "CycleControl", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			string[] array4 = new string[3] { "RefreshBg", "RefreshButton", "MultiplayerIconRefresh" };
			foreach (string text in array4)
			{
				val.Patch((MethodBase)AccessTools.Method(selector, text, (Type[])null, (Type[])null), new HarmonyMethod(typeof(FrameworkRegistryCooperation), "CanRefreshMainPresentation", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			val.Patch((MethodBase)AccessTools.Method(selector, "PlayPreviewSprite", (Type[])null, (Type[])null), new HarmonyMethod(typeof(FrameworkRegistryCooperation), "PlayModelPreview", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			val.Patch((MethodBase)AccessTools.Method(selector, "_Ready", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(typeof(FrameworkRegistryCooperation), "TrackControl", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
			FrameworkNativeUiPatch.Install(assembly, val);
		}
		catch (Exception ex)
		{
			val.UnpatchAll(val.Id);
			ModLog.Warn("原管理器附加入口无法接入；注册/设置协作及 SC 入口继续可用：" + ex.GetBaseException().Message);
		}
		ModLog.Info("已启用原皮肤管理器协作：原 UI、预览、设置、存档和呈现补丁保留；双方显式选择经热切换事务同步，未加载内置同名后备接口。");
	}

	internal static bool UsesNativePresentation(FrameworkCharacterSkinContract contract)
	{
		if (_session != null && contract.FrameworkAssemblyName == _session.Assembly.GetName().Name)
		{
			return _session.IsRegistered(contract.TargetGroupId, contract.SkinId);
		}
		return false;
	}

	public static void SynchronizeLocalSelections(SkinCatalog catalog)
	{
		if (_session == null || !ModelDb.Contains(typeof(Ironclad)))
		{
			return;
		}
		foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
		{
			string groupId = Normalize(((AbstractModel)allCharacter).Id.Entry);
			FrameworkCharacterSkinContract contract;
			bool flag = catalog.TryGetSelectedFrameworkContract(groupId, SkinService.Config.GetSelection(groupId), out contract) && contract.FrameworkAssemblyName == _session.Assembly.GetName().Name;
			_session.PublishSelection(((AbstractModel)allCharacter).Id, flag ? contract.SkinId : null);
		}
	}

	private static void RequestSelection(string groupId, string skinId)
	{
		string text = ((skinId == "default") ? "__base__" : SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id == groupId)?.Options.FirstOrDefault(delegate(SkinOption option)
		{
			FrameworkCharacterSkinContract frameworkContract = option.FrameworkContract;
			return (object)frameworkContract != null && frameworkContract.SkinId == skinId && frameworkContract.FrameworkAssemblyName == _session?.Assembly.GetName().Name;
		})?.Id);
		if (text != null)
		{
			RequestOptionSelection(groupId, text);
		}
	}

	private static void RequestOptionSelection(string groupId, string optionId)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		long generation = ++_requestGeneration;
		Requests[groupId] = generation;
		PendingOptions[groupId] = optionId;
		Callable val4 = Callable.From((Action)delegate
		{
			if (Requests.TryGetValue(groupId, out var value) && value == generation)
			{
				foreach (Node item in LiveControls())
				{
					ModelId val = CharacterId(item);
					if (val != null && !(Normalize(val.Entry) != groupId))
					{
						for (Node val2 = item; val2 != null; val2 = val2.GetParent())
						{
							NCharacterSelectScreen val3 = (NCharacterSelectScreen)(object)((val2 is NCharacterSelectScreen) ? val2 : null);
							if (val3 != null && ContextualSkinControls.RequestFrameworkSelection(val3, groupId, optionId))
							{
								return;
							}
						}
					}
				}
				Requests.Remove(groupId);
				PendingOptions.Remove(groupId);
				ModLog.Info("已忽略不在当前选角界面中的原管理器切肤请求：" + groupId + "/" + optionId);
			}
		});
		((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
	}

	private static ModelId? CharacterId(Node control)
	{
		object? obj = _selectorType?.GetProperty("characterId")?.GetValue(control);
		return (ModelId?)((obj is ModelId) ? obj : null);
	}

	private static IEnumerable<Node> LiveControls()
	{
		Controls.RemoveAll((WeakReference<Node> reference) => !reference.TryGetTarget(out Node target) || !GodotObject.IsInstanceValid((GodotObject)(object)target) || ((GodotObject)target).IsQueuedForDeletion());
		Node target2;
		return (from node in Controls.Select((WeakReference<Node> reference) => (!reference.TryGetTarget(out target2)) ? null : target2).OfType<Node>()
			where node.IsInsideTree()
			select node).ToArray();
	}

	public static void RefreshControls()
	{
		foreach (Node item in LiveControls())
		{
			try
			{
				AccessTools.Method(_selectorType, "Refresh", (Type[])null, (Type[])null).Invoke(item, null);
			}
			catch (Exception ex)
			{
				ModLog.Warn("原管理器预览刷新失败，已保留 SC 当前选择：" + ex.GetBaseException().Message);
			}
			UpdateLabel(item);
		}
	}

	public static void SelectionStarting(string groupId, string optionId)
	{
		if (_session != null)
		{
			Requests.Remove(groupId);
			PendingOptions[groupId] = optionId;
		}
	}

	public static void QueueRefreshControls()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (_session != null && !_refreshQueued)
		{
			_refreshQueued = true;
			Callable val = Callable.From((Action)delegate
			{
				_refreshQueued = false;
				RefreshControls();
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private static NCharacterSelectScreen? CharacterScreen(Node control)
	{
		for (Node val = control; val != null; val = val.GetParent())
		{
			NCharacterSelectScreen val2 = (NCharacterSelectScreen)(object)((val is NCharacterSelectScreen) ? val : null);
			if (val2 != null)
			{
				return val2;
			}
		}
		return null;
	}

	private static void UpdateLabel(Node __instance)
	{
		ModelId val = CharacterId(__instance);
		if (val == null)
		{
			return;
		}
		NCharacterSelectScreen val2 = CharacterScreen(__instance);
		if (val2 == null)
		{
			return;
		}
		string text = Normalize(val.Entry);
		string frameworkSelectionName = ContextualSkinControls.GetFrameworkSelectionName(val2, text, PendingOptions.GetValueOrDefault(text));
		if (frameworkSelectionName != null)
		{
			Label nodeOrNull = __instance.GetNodeOrNull<Label>(NodePath.op_Implicit("HBoxContainer/ScrollTextContainer/SkinName"));
			if (nodeOrNull != null)
			{
				nodeOrNull.Text = frameworkSelectionName;
			}
		}
	}

	private static void TrackControl(Node __instance)
	{
		if (!LiveControls().Any((Node node) => node == __instance))
		{
			Controls.Add(new WeakReference<Node>(__instance));
		}
	}

	private static bool CanRefreshControl(Node __instance, MethodBase __originalMethod)
	{
		TrackControl(__instance);
		ModelId val = CharacterId(__instance);
		if (val == null)
		{
			return false;
		}
		string key = Normalize(val.Entry);
		for (Node val2 = __instance; val2 != null; val2 = val2.GetParent())
		{
			NCharacterSelectScreen val3 = (NCharacterSelectScreen)(object)((val2 is NCharacterSelectScreen) ? val2 : null);
			if (val3 != null && ContextualSkinControls.IsCharacterSelectionLoading(val3))
			{
				return false;
			}
		}
		if (Requests.ContainsKey(key))
		{
			return false;
		}
		PendingOptions.Remove(key);
		if (__originalMethod.Name != "LoadPreview")
		{
			return true;
		}
		object? obj = _selectorType?.GetProperty("characterModel")?.GetValue(__instance);
		CharacterModel val4 = (CharacterModel)((obj is CharacterModel) ? obj : null);
		if (val4 != null)
		{
			FrameworkModelPreview.Refresh(__instance, val4);
		}
		return false;
	}

	private static bool CanRefreshMainPresentation(Node __instance)
	{
		ModelId val = CharacterId(__instance);
		if (val != null && SkinService.TryGetSelectedFrameworkContract(Normalize(val.Entry), out FrameworkCharacterSkinContract contract))
		{
			return UsesNativePresentation(contract);
		}
		return false;
	}

	private static bool PlayModelPreview(Node __instance)
	{
		if (__instance.GetNodeOrNull<Node>(NodePath.op_Implicit("VisualContainer/PreviewSprite")) is FrameworkPreviewSurface frameworkPreviewSurface)
		{
			frameworkPreviewSurface.PlayEntry();
		}
		else
		{
			Node nodeOrNull = __instance.GetNodeOrNull<Node>(NodePath.op_Implicit("VisualContainer/PreviewSprite"));
			if (nodeOrNull != null)
			{
				ModelId? obj = CharacterId(__instance);
				FrameworkModelPreview.StartAnimations(nodeOrNull, ((obj != null) ? obj.Entry : null) ?? "unknown");
			}
		}
		return false;
	}

	private static bool CycleControl(Node __instance, MethodBase __originalMethod)
	{
		ModelId val = CharacterId(__instance);
		if (val != null)
		{
			NCharacterSelectScreen val2 = CharacterScreen(__instance);
			if (val2 != null)
			{
				string text = Normalize(val.Entry);
				int direction = ((__originalMethod.Name == "OnNextPressed") ? 1 : (-1));
				string frameworkCycleOption = ContextualSkinControls.GetFrameworkCycleOption(val2, text, PendingOptions.GetValueOrDefault(text), direction);
				if (frameworkCycleOption != null)
				{
					RequestOptionSelection(text, frameworkCycleOption);
				}
				return false;
			}
		}
		return false;
	}

	internal static FrameworkCharacterSkinContract Filter(FrameworkCharacterSkinContract contract)
	{
		if (_session == null || contract.FrameworkAssemblyName != _session.Assembly.GetName().Name)
		{
			return contract;
		}
		return Filter(contract, (string key) => _session.IsConfigEnabled(contract.TargetGroupId, contract.SkinId, key));
	}

	public static string CacheSuffix(string groupId, string selection)
	{
		if (_session == null || SkinService.Catalog == null || !SkinService.Catalog.TryGetSelectedFrameworkContract(groupId, selection, out FrameworkCharacterSkinContract contract) || contract.FrameworkAssemblyName != _session.Assembly.GetName().Name)
		{
			return string.Empty;
		}
		return "\nframework-settings:" + SettingsMask((string key) => _session.IsConfigEnabled(contract.TargetGroupId, contract.SkinId, key));
	}

	public static SkinOption? FilterAssets(SkinOption? option)
	{
		FrameworkCharacterSkinContract contract = option?.FrameworkContract;
		if ((object)contract == null)
		{
			return option;
		}
		FrameworkCharacterSkinContract frameworkCharacterSkinContract = Filter(contract);
		if ((object)contract == frameworkCharacterSkinContract)
		{
			return option;
		}
		ResourceAsset value;
		HashSet<string> excluded = (from key in contract.CharacterResources.Keys.Except(frameworkCharacterSkinContract.CharacterResources.Keys)
			select (Canonical: SkinCatalog.GetFrameworkCharacterCanonicalPath(key, contract.TargetGroupId), Source: contract.CharacterResources[key]) into pair
			where pair.Canonical != null && option.Assets.TryGetValue(pair.Canonical, out value) && value.SourcePath.Equals(pair.Source, StringComparison.OrdinalIgnoreCase)
			select pair.Canonical).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return option with
		{
			Assets = option.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => !excluded.Contains(pair.Key)).ToDictionary()
		};
	}

	private static int SettingsMask(Func<string, bool> enabled)
	{
		return (int)((enabled("UseCardFrame") ? 1u : 0u) | (uint)(enabled("UseEnergy") ? 2 : 0) | (uint)(enabled("UseHands") ? 4 : 0) | (uint)(enabled("UseDefectOrbs") ? 8 : 0)) | (enabled("SilentRecolorShiv") ? 16 : 0);
	}

	public static FrameworkCharacterSkinContract Filter(FrameworkCharacterSkinContract contract, Func<string, bool> enabled)
	{
		int mask = SettingsMask(enabled);
		if (mask == 31)
		{
			return contract;
		}
		Dictionary<int, FrameworkCharacterSkinContract> orCreateValue = Filtered.GetOrCreateValue(contract);
		if (orCreateValue.TryGetValue(mask, out var value))
		{
			return value;
		}
		FrameworkCharacterSkinContract obj = contract with
		{
			CharacterResources = contract.CharacterResources.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => Include(pair.Key)).ToDictionary(),
			CharacterResourceLists = contract.CharacterResourceLists.Where<KeyValuePair<string, IReadOnlyList<string>>>((KeyValuePair<string, IReadOnlyList<string>> pair) => Include(pair.Key)).ToDictionary(),
			CharacterValues = contract.CharacterValues.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => Include(pair.Key)).ToDictionary()
		};
		IReadOnlyList<FrameworkModelSkinContract> orbs;
		if ((mask & 8) != 0)
		{
			orbs = contract.Orbs;
		}
		else
		{
			IReadOnlyList<FrameworkModelSkinContract> readOnlyList = Array.Empty<FrameworkModelSkinContract>();
			orbs = readOnlyList;
		}
		obj.Orbs = orbs;
		FrameworkCharacterSkinContract frameworkCharacterSkinContract = obj;
		orCreateValue[mask] = frameworkCharacterSkinContract;
		return frameworkCharacterSkinContract;
		bool Include(string property)
		{
			switch (property)
			{
			case "CardTrail":
			case "CardFrameMaterial":
				return (mask & 1) != 0;
			case "EnergyLayers":
			case "EnergyIcon":
			case "EnergyLabelColor":
			case "EnergyLabelOutlineColor":
				return (mask & 2) != 0;
			case "HandPaper":
			case "HandPoint":
			case "HandScissors":
			case "HandRock":
				return (mask & 4) != 0;
			case "ShivTintColor":
				return (mask & 0x10) != 0;
			default:
				return true;
			}
		}
	}

	private static string Normalize(string value)
	{
		return FrameworkSkinRuntime.NormalizeToken(value);
	}
}
