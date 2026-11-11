using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

internal sealed class FrameworkRegistrySession
{
	private static readonly Dictionary<Type, FrameworkRegistrySession> Sessions = new Dictionary<Type, FrameworkRegistrySession>();

	private readonly Type _registry;

	private readonly Type _data;

	private readonly FieldInfo _skins;

	private readonly PropertyInfo _skinId;

	private readonly MethodInfo _config;

	private readonly Func<ModelId, string?> _read;

	private readonly Action<ModelId, string> _request;

	private readonly IDictionary _nativeSelections;

	private readonly IDictionary _nativePointers;

	private int _publishing;

	private bool _loaded;

	private bool _cacheDirty;

	public Action PrepareCharacters { get; set; } = delegate
	{
	};

	public Assembly Assembly => _registry.Assembly;

	private IDictionary Skins => (IDictionary)_skins.GetValue(null);

	public FrameworkRegistrySession(Assembly assembly, Func<ModelId, string?> read, Action<ModelId, string> request)
	{
		_registry = assembly.GetType("thunninoiSkinManager.thunninoiSkinManagerCode.SkinRegistry", throwOnError: true);
		_data = assembly.GetType("thunninoiSkinManager.thunninoiSkinManagerCode.SkinData", throwOnError: true);
		_skins = _registry.GetField("_skins", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new MissingFieldException(_registry.FullName, "_skins");
		_skinId = _data.GetProperty("SkinId");
		_config = _data.GetMethod("IsConfigEnabled", new Type[2]
		{
			typeof(string),
			typeof(bool)
		});
		if (_skins.FieldType != typeof(Dictionary<, >).MakeGenericType(typeof(ModelId), typeof(List<>).MakeGenericType(_data)) || _skinId?.PropertyType != typeof(string) || _config?.ReturnType != typeof(bool) || _data.GetConstructor(new Type[3]
		{
			typeof(ModelId),
			typeof(string),
			typeof(string)
		}) == null)
		{
			throw new NotSupportedException("原管理器注册表结构已变化，不能安全启用协作。");
		}
		_read = read;
		_request = request;
		_nativeSelections = (IDictionary)(_registry.GetField("_activeSkins", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new MissingFieldException(_registry.FullName, "_activeSkins")).GetValue(null);
		_nativePointers = (IDictionary)(_registry.GetField("_activePointer", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new MissingFieldException(_registry.FullName, "_activePointer")).GetValue(null);
	}

	private IList? GetSkins(ModelId id)
	{
		return Skins[id] as IList;
	}

	private string Id(object skin)
	{
		return (string)_skinId.GetValue(skin);
	}

	private object? Find(ModelId id, string skinId)
	{
		return GetSkins(id)?.Cast<object>().FirstOrDefault((object skin) => Id(skin).Equals(skinId, StringComparison.Ordinal));
	}

	private object? Active(ModelId id)
	{
		return Find(id, ((_publishing > 0) ? (_nativeSelections[id] as string) : _read(id)) ?? "default") ?? Find(id, "default");
	}

	public void PublishSelection(ModelId id, string? skinId)
	{
		EnsureCharacter(id);
		if (skinId == null)
		{
			skinId = "default";
		}
		object obj = Find(id, skinId);
		if (obj == null)
		{
			return;
		}
		int num = GetSkins(id).IndexOf(obj);
		if (!_cacheDirty && object.Equals(_nativeSelections[id], skinId) && object.Equals(_nativePointers[id], num))
		{
			return;
		}
		object value = _nativeSelections[id];
		object value2 = _nativePointers[id];
		bool flag = _nativePointers.Contains(id);
		_cacheDirty = true;
		_publishing++;
		try
		{
			_registry.GetMethod("SetActiveSkin", new Type[2]
			{
				typeof(ModelId),
				typeof(string)
			}).Invoke(null, new object[2] { id, skinId });
			_cacheDirty = false;
		}
		catch
		{
			_nativeSelections[id] = value;
			if (flag)
			{
				_nativePointers[id] = value2;
			}
			else
			{
				_nativePointers.Remove(id);
			}
			throw;
		}
		finally
		{
			_publishing--;
		}
	}

	public void EnsureCharacter(ModelId id)
	{
		if (GetSkins(id) == null)
		{
			Skins[id] = Activator.CreateInstance(typeof(List<>).MakeGenericType(_data));
		}
		if (Find(id, "default") == null)
		{
			object obj = Activator.CreateInstance(_data, id, "default", "Default");
			_data.GetMethod("AsDefault").Invoke(obj, null);
			GetSkins(id).Insert(0, obj);
		}
		if (!_nativeSelections.Contains(id))
		{
			_nativeSelections[id] = "default";
		}
	}

	public bool IsConfigEnabled(string groupId, string skinId, string key)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		foreach (ModelId key2 in Skins.Keys)
		{
			ModelId val = key2;
			if (Normalize(val.Entry).Equals(Normalize(groupId), StringComparison.Ordinal))
			{
				object obj = Find(val, skinId);
				if (obj != null)
				{
					return (bool)_config.Invoke(obj, new object[2] { key, true });
				}
			}
		}
		return true;
	}

	public bool IsRegistered(string groupId, string skinId)
	{
		return Skins.Keys.Cast<ModelId>().Any((ModelId id) => Normalize(id.Entry) == Normalize(groupId) && Find(id, skinId) != null);
	}

	private MethodInfo[] RegistrationCallbacks(Assembly provider)
	{
		return (from patch in Harmony.GetPatchInfo((MethodBase)AccessTools.Method(_registry, "SkinDbSetup", (Type[])null, (Type[])null))?.Postfixes
			where patch.PatchMethod.Module.Assembly == provider
			orderby patch.priority descending, patch.index
			select patch.PatchMethod).Distinct().ToArray() ?? Array.Empty<MethodInfo>();
	}

	public bool HasRegistrationCallbacks(Assembly provider)
	{
		return RegistrationCallbacks(provider).Length != 0;
	}

	public void RegisterProvider(Assembly provider)
	{
		MethodInfo[] array = RegistrationCallbacks(provider);
		if (array.Length != 0)
		{
			if (array.Any((MethodInfo callback) => !callback.IsStatic || callback.GetParameters().Length != 0))
			{
				throw new NotSupportedException("原框架登记接口已变化，不能安全补做该提供者登记。");
			}
			Setup(AccessTools.Method(_registry, "SkinDbSetup", (Type[])null, (Type[])null));
			MethodInfo[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Invoke(null, null);
			}
			_registry.GetMethod("finializeSetup").Invoke(null, null);
		}
	}

	private void Request(ModelId id, string skinId)
	{
		if (Find(id, skinId) != null)
		{
			_request(id, skinId);
		}
	}

	public void Install()
	{
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Expected O, but got Unknown
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Expected O, but got Unknown
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Expected O, but got Unknown
		if (Sessions.ContainsKey(_registry))
		{
			return;
		}
		List<(MethodInfo Target, MethodInfo Prefix)> patches = new List<(MethodInfo, MethodInfo)>();
		Add("GetActiveSkin", new Type[1] { typeof(ModelId) }, "ReadActive");
		Add("IsUsingSkin", new Type[2]
		{
			typeof(ModelId),
			typeof(string)
		}, "ReadUsing");
		Add("GetAllActiveSkins", Array.Empty<Type>(), "ReadAll");
		Add("SetActiveSkin", new Type[2]
		{
			typeof(ModelId),
			typeof(string)
		}, "WriteString");
		Add("SetActiveSkin", new Type[2]
		{
			typeof(ModelId),
			typeof(int)
		}, "WriteIndex");
		Add("CycleNext", new Type[1] { typeof(ModelId) }, "Cycle");
		Add("CyclePrevious", new Type[1] { typeof(ModelId) }, "Cycle");
		Add("SkinDbSetup", Array.Empty<Type>(), "Setup");
		Add("finializeSetup", Array.Empty<Type>(), "BeginFinalize");
		string[] array = new string[4] { "ResolvePower", "ResolvePotion", "ResolveRelic", "ResolveOrb" };
		foreach (string name2 in array)
		{
			Add(name2, new Type[1] { typeof(ModelId) }, "ResolveShared");
		}
		Harmony val = new Harmony("Gurio.SkinChanger.native-framework-registry");
		Sessions.Add(_registry, this);
		try
		{
			foreach (var item in patches)
			{
				val.Patch((MethodBase)item.Target, new HarmonyMethod(item.Prefix)
				{
					priority = 800
				}, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			val.Patch((MethodBase)AccessTools.Method(_registry, "finializeSetup", (Type[])null, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(FrameworkRegistrySession), "EndFinalize", (Type[])null));
		}
		catch
		{
			foreach (var item2 in patches)
			{
				val.Unpatch((MethodBase)item2.Target, item2.Prefix);
			}
			Sessions.Remove(_registry);
			throw;
		}
		void Add(string name, Type[] args, string prefix)
		{
			patches.Add((AccessTools.Method(_registry, name, args, (Type[])null) ?? throw new MissingMethodException(_registry.FullName, name), AccessTools.Method(typeof(FrameworkRegistrySession), prefix, (Type[])null, (Type[])null)));
		}
	}

	private static FrameworkRegistrySession Session(MethodBase method)
	{
		return Sessions[method.DeclaringType];
	}

	private static bool ReadActive(MethodBase __originalMethod, ModelId __0, ref object? __result)
	{
		__result = Session(__originalMethod).Active(__0);
		return false;
	}

	private static bool ReadUsing(MethodBase __originalMethod, ModelId __0, string __1, ref bool __result)
	{
		FrameworkRegistrySession frameworkRegistrySession = Session(__originalMethod);
		object obj = frameworkRegistrySession.Active(__0);
		__result = obj != null && frameworkRegistrySession.Id(obj).Equals(__1, StringComparison.Ordinal);
		return false;
	}

	private static bool ReadAll(MethodBase __originalMethod, ref Dictionary<ModelId, string> __result)
	{
		FrameworkRegistrySession session = Session(__originalMethod);
		if (session._publishing > 0)
		{
			return true;
		}
		__result = session.Skins.Keys.Cast<ModelId>().ToDictionary((ModelId id) => id, delegate(ModelId id)
		{
			object obj = session.Active(id);
			return (obj == null) ? "default" : session.Id(obj);
		});
		return false;
	}

	private static bool WriteString(MethodBase __originalMethod, ModelId __0, string __1)
	{
		FrameworkRegistrySession frameworkRegistrySession = Session(__originalMethod);
		if (frameworkRegistrySession._publishing > 0)
		{
			return frameworkRegistrySession.Find(__0, __1) != null;
		}
		frameworkRegistrySession.Request(__0, __1);
		return false;
	}

	private static bool WriteIndex(MethodBase __originalMethod, ModelId __0, int __1)
	{
		FrameworkRegistrySession frameworkRegistrySession = Session(__originalMethod);
		IList skins = frameworkRegistrySession.GetSkins(__0);
		if (frameworkRegistrySession._publishing > 0)
		{
			if (skins != null && __1 >= 0)
			{
				return __1 < skins.Count;
			}
			return false;
		}
		if (skins != null && __1 >= 0 && __1 < skins.Count)
		{
			frameworkRegistrySession.Request(__0, frameworkRegistrySession.Id(skins[__1]));
		}
		return false;
	}

	private static bool Cycle(MethodBase __originalMethod, ModelId __0)
	{
		FrameworkRegistrySession frameworkRegistrySession = Session(__originalMethod);
		IList skins = frameworkRegistrySession.GetSkins(__0);
		if (skins == null || skins.Count == 0)
		{
			return false;
		}
		int val = skins.IndexOf(frameworkRegistrySession.Active(__0));
		int num = ((__originalMethod.Name == "CycleNext") ? 1 : (-1));
		frameworkRegistrySession.Request(__0, frameworkRegistrySession.Id(skins[(Math.Max(val, 0) + skins.Count + num) % skins.Count]));
		return false;
	}

	private static bool Setup(MethodBase __originalMethod)
	{
		FrameworkRegistrySession frameworkRegistrySession = Session(__originalMethod);
		if (!frameworkRegistrySession._loaded)
		{
			frameworkRegistrySession._registry.GetMethod("Load").Invoke(null, null);
			frameworkRegistrySession._loaded = true;
		}
		frameworkRegistrySession.PrepareCharacters();
		return false;
	}

	private static void BeginFinalize(MethodBase __originalMethod, out bool __state)
	{
		__state = false;
		FrameworkRegistrySession frameworkRegistrySession = Session(__originalMethod);
		frameworkRegistrySession._nativePointers.Clear();
		ModelId[] array = frameworkRegistrySession._nativeSelections.Keys.Cast<ModelId>().ToArray();
		foreach (ModelId val in array)
		{
			if (frameworkRegistrySession.Find(val, (frameworkRegistrySession._nativeSelections[val] as string) ?? "default") == null)
			{
				frameworkRegistrySession._nativeSelections[val] = "default";
			}
		}
		frameworkRegistrySession._publishing++;
		__state = true;
	}

	private static void EndFinalize(MethodBase __originalMethod, bool __state)
	{
		if (__state)
		{
			Session(__originalMethod)._publishing--;
		}
	}

	private static bool ResolveShared(MethodBase __originalMethod, ModelId __0, ref object? __result)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		FrameworkRegistrySession frameworkRegistrySession = Session(__originalMethod);
		string name = __originalMethod.Name;
		int length = "Resolve".Length;
		string text = name.Substring(length, name.Length - length) + "SkinDict";
		__result = null;
		foreach (ModelId key in frameworkRegistrySession.Skins.Keys)
		{
			ModelId id = key;
			object obj = frameworkRegistrySession.Active(id);
			if (obj != null && (!(text == "OrbSkinDict") || (bool)frameworkRegistrySession._config.Invoke(obj, new object[2] { "UseDefectOrbs", true })) && frameworkRegistrySession._data.GetProperty(text, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj) is IDictionary dictionary && dictionary.Contains(__0))
			{
				__result = dictionary[__0];
				break;
			}
		}
		return false;
	}

	private static string Normalize(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}
}
