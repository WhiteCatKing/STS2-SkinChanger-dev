using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Core;

internal static class ExternalCardVisualBridge
{
	private sealed record ProviderTextureView(string Identity, AtlasTexture Texture);

	private sealed class ProviderCaptureScope
	{
		private readonly TextureRect? _portrait;

		private readonly TextureRect? _ancientPortrait;

		private readonly Texture2D? _previousPortrait;

		private readonly Texture2D? _previousAncientPortrait;

		private readonly Texture2D _provider;

		public ProviderCaptureScope(TextureRect? portrait, TextureRect? ancientPortrait, Texture2D provider)
		{
			_portrait = portrait;
			_ancientPortrait = ancientPortrait;
			_previousPortrait = ((portrait != null) ? portrait.Texture : null);
			_previousAncientPortrait = ((ancientPortrait != null) ? ancientPortrait.Texture : null);
			_provider = provider;
			if (portrait != null)
			{
				portrait.Texture = provider;
			}
			if (ancientPortrait != null)
			{
				ancientPortrait.Texture = provider;
			}
		}

		public void Restore()
		{
			RestoreNode(_portrait, _previousPortrait);
			RestoreNode(_ancientPortrait, _previousAncientPortrait);
		}

		private void RestoreNode(TextureRect? node, Texture2D? previous)
		{
			if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node) && node.Texture == _provider)
			{
				node.Texture = previous;
			}
		}
	}

	private interface ICardVisualOwnershipAdapter
	{
		ExternalCardVisualOwnership GetOwnership(CardModel card);
	}

	private sealed class CardEditorAdapter : ICardVisualOwnershipAdapter
	{
		public Assembly Assembly { get; }

		public CardEditorAdapter(Assembly assembly, MethodInfo resolveIdentity, MethodInfo hasPortrait, RegistryProbe? frameProbe, RegistryProbe? textProbe)
		{
			Assembly = assembly;
			base._002Ector();
		}

		public ExternalCardVisualOwnership GetOwnership(CardModel card)
		{
			string text;
			try
			{
				text = resolveIdentity.Invoke(null, new object[1] { card }) as string;
			}
			catch (Exception exception)
			{
				LogFailure(Assembly, "卡牌标识读取", exception);
				return default(ExternalCardVisualOwnership);
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				return default(ExternalCardVisualOwnership);
			}
			bool portrait = false;
			try
			{
				object obj = hasPortrait.Invoke(null, new object[2] { card, text });
				portrait = obj is bool && (bool)obj;
			}
			catch (Exception exception2)
			{
				LogFailure(Assembly, "卡图状态读取", exception2);
			}
			return new ExternalCardVisualOwnership(portrait, HasRegistryOverride(text, frameProbe, "卡框状态读取"), HasRegistryOverride(text, textProbe, "文字状态读取"));
		}

		private bool HasRegistryOverride(string cardId, RegistryProbe? probe, string operation)
		{
			if (probe == null)
			{
				return false;
			}
			try
			{
				return probe.HasOverride(cardId);
			}
			catch (Exception exception)
			{
				LogFailure(Assembly, operation, exception);
				return false;
			}
		}
	}

	private sealed class FullCardPresentationAdapter(Assembly assembly, MethodInfo hasPresentation, MethodInfo isEnabled, MethodInfo getCurrent) : ICardVisualOwnershipAdapter
	{
		public ExternalCardVisualOwnership GetOwnership(CardModel card)
		{
			try
			{
				object obj = hasPresentation.Invoke(null, new object[1] { card });
				if (obj is bool && (bool)obj)
				{
					obj = isEnabled.Invoke(null, new object[1] { card });
					if (obj is bool && (bool)obj && getCurrent.Invoke(null, new object[1] { card }) != null)
					{
						return new ExternalCardVisualOwnership(Portrait: true, Frame: true, Text: true);
					}
				}
			}
			catch (Exception exception)
			{
				LogFailure(assembly, "完整卡牌呈现状态读取", exception);
			}
			return default(ExternalCardVisualOwnership);
		}
	}

	private sealed class RegistryProbe(MethodInfo tryGetOverride, MethodInfo hasAnyValue)
	{
		public bool HasOverride(string cardId)
		{
			object[] array = new object[2] { cardId, null };
			object obj = tryGetOverride.Invoke(null, array);
			if (!(obj is bool) || !(bool)obj || array[1] == null)
			{
				return false;
			}
			obj = hasAnyValue.Invoke(array[1], null);
			if (obj is bool)
			{
				return (bool)obj;
			}
			return false;
		}
	}

	private static readonly object Sync;

	private static readonly List<ICardVisualOwnershipAdapter> Adapters;

	private static readonly HashSet<Assembly> ScannedAssemblies;

	private static readonly HashSet<string> LoggedFailures;

	private static ICardVisualOwnershipAdapter[] _adapterSnapshot;

	private static volatile bool _discoveryDirty;

	private static readonly HashSet<string> ArtPackCoveredCardTypes;

	private static WeakReference<Node>? _scriptedManagerReference;

	private static ulong _nextScriptedManagerProbeTicks;

	private static bool _scriptedManagerLogged;

	private static readonly ConditionalWeakTable<Texture2D, ProviderTextureView> ProviderTextureViews;

	private const BindingFlags AllStatic = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	static ExternalCardVisualBridge()
	{
		Sync = new object();
		Adapters = new List<ICardVisualOwnershipAdapter>();
		ScannedAssemblies = new HashSet<Assembly>();
		LoggedFailures = new HashSet<string>(StringComparer.Ordinal);
		_adapterSnapshot = Array.Empty<ICardVisualOwnershipAdapter>();
		_discoveryDirty = true;
		ArtPackCoveredCardTypes = new HashSet<string>(StringComparer.Ordinal);
		ProviderTextureViews = new ConditionalWeakTable<Texture2D, ProviderTextureView>();
		AppDomain.CurrentDomain.AssemblyLoad += delegate
		{
			_discoveryDirty = true;
		};
	}

	public static ExternalCardVisualOwnership GetOwnership(CardModel card)
	{
		ExternalCardVisualOwnership result = default(ExternalCardVisualOwnership);
		ICardVisualOwnershipAdapter[] array = EnumerateAdapters();
		foreach (ICardVisualOwnershipAdapter cardVisualOwnershipAdapter in array)
		{
			result = result.Merge(cardVisualOwnershipAdapter.GetOwnership(card));
		}
		return result;
	}

	public static ExternalCardVisualOwnership GetOwnership(NCard card)
	{
		return ((card.Model == null) ? default(ExternalCardVisualOwnership) : GetOwnership(card.Model)).Merge(GetScriptedManagerOwnership(card));
	}

	public static void WarmUp()
	{
		EnumerateAdapters();
	}

	public static void RegisterArtPackCoverage(IEnumerable<string> cardTypes)
	{
		lock (Sync)
		{
			bool flag = false;
			foreach (string cardType in cardTypes)
			{
				if (!string.IsNullOrWhiteSpace(cardType))
				{
					flag |= ArtPackCoveredCardTypes.Add(NormalizeCardType(cardType));
				}
			}
			if (flag)
			{
				ModLog.Info("卡图包覆盖的卡牌由皮肤切换器负责全画幅呈现；外部编辑器不再接管这些卡牌的卡框与文字层。");
			}
		}
	}

	private static string NormalizeCardType(string cardType)
	{
		return cardType.ToLowerInvariant();
	}

	public static void SynchronizeProvider(NCard card)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Node scriptedManager = GetScriptedManager();
		if (scriptedManager == null || card.Model == null)
		{
			return;
		}
		try
		{
			ProviderCaptureScope providerCaptureScope = PrepareManagedProviderIdentity(card);
			try
			{
				((GodotObject)scriptedManager).Call(StringName.op_Implicit("capture_card_provider_after_visual_update"), (Variant[])(object)new Variant[2]
				{
					Variant.op_Implicit((GodotObject)(object)card),
					Variant.op_Implicit(true)
				});
			}
			finally
			{
				providerCaptureScope?.Restore();
			}
			((GodotObject)scriptedManager).Call(StringName.op_Implicit("apply_card_override_after_visual_update"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)card) });
		}
		catch (Exception exception)
		{
			LogScriptedFailure(scriptedManager, "最终卡图同步", exception);
		}
	}

	private static ICardVisualOwnershipAdapter[] EnumerateAdapters()
	{
		lock (Sync)
		{
			if (_discoveryDirty)
			{
				DiscoverAdapters();
			}
			return _adapterSnapshot;
		}
	}

	private static void DiscoverAdapters()
	{
		_discoveryDirty = false;
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (!ScannedAssemblies.Add(assembly) || IsFrameworkAssembly(assembly))
			{
				continue;
			}
			try
			{
				Type[] loadableTypes = GetLoadableTypes(assembly);
				Type type2 = loadableTypes.FirstOrDefault((Type type) => type.Name.Equals("CardVisualIdentity", StringComparison.Ordinal));
				Type type3 = loadableTypes.FirstOrDefault((Type type) => type.Name.Equals("CardPortraitReplacementResolver", StringComparison.Ordinal));
				if (type2 != null && type3 != null)
				{
					MethodInfo methodInfo = type2.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo method)
					{
						if (method.Name.Equals("Resolve", StringComparison.Ordinal) && method.ReturnType == typeof(string))
						{
							ParameterInfo[] parameters = method.GetParameters();
							if (parameters != null && parameters.Length == 1)
							{
								ParameterInfo parameterInfo = parameters[0];
								if (parameterInfo != null)
								{
									Type parameterType = parameterInfo.ParameterType;
									return AcceptsCardModel(parameterType);
								}
							}
						}
						return false;
					});
					MethodInfo methodInfo2 = type3.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo method)
					{
						if (method.Name.Equals("HasAnyPortraitReplacement", StringComparison.Ordinal) && method.ReturnType == typeof(bool))
						{
							ParameterInfo[] parameters2 = method.GetParameters();
							if (parameters2 != null && parameters2.Length == 2)
							{
								ParameterInfo parameterInfo2 = parameters2[0];
								if (parameterInfo2 != null)
								{
									Type parameterType2 = parameterInfo2.ParameterType;
									ParameterInfo parameterInfo3 = parameters2[1];
									if (parameterInfo3 != null)
									{
										Type parameterType3 = parameterInfo3.ParameterType;
										if (AcceptsCardModel(parameterType2))
										{
											return parameterType3 == typeof(string);
										}
									}
								}
							}
						}
						return false;
					});
					if (methodInfo != null && methodInfo2 != null)
					{
						RegistryProbe frameProbe = CreateRegistryProbe((IReadOnlyCollection<Type>)(object)loadableTypes, "CardFrameOverrideRegistry", "TryGetOverride");
						RegistryProbe textProbe = CreateRegistryProbe((IReadOnlyCollection<Type>)(object)loadableTypes, "CardTextOverrideRegistry", "TryGetOverride");
						Adapters.Add(new CardEditorAdapter(assembly, methodInfo, methodInfo2, frameProbe, textProbe));
						ModLog.Info("检测到外部卡牌视觉管理器 " + assembly.GetName().Name + "；它明确编辑的卡图、卡框和文字层将优先于皮肤切换器。");
					}
				}
				FullCardPresentationAdapter fullCardPresentationAdapter = CreateFullCardPresentationAdapter(assembly, (IReadOnlyCollection<Type>)(object)loadableTypes);
				if (fullCardPresentationAdapter != null)
				{
					Adapters.Add(fullCardPresentationAdapter);
					ModLog.Info("检测到外部完整卡牌呈现系统 " + assembly.GetName().Name + "；启用中的完整卡图、卡框和文字布局将保留其最终显示权。");
				}
			}
			catch (Exception exception)
			{
				LogFailure(assembly, "能力探测", exception);
			}
		}
		_adapterSnapshot = Adapters.ToArray();
	}

	private static ExternalCardVisualOwnership GetScriptedManagerOwnership(NCard card)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		Node scriptedManager = GetScriptedManager();
		if (scriptedManager == null)
		{
			return default(ExternalCardVisualOwnership);
		}
		try
		{
			Variant val = ((GodotObject)scriptedManager).Call(StringName.op_Implicit("get_source_path_for_card_node"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)card) });
			string text = ((Variant)(ref val)).AsString();
			if (string.IsNullOrWhiteSpace(text))
			{
				return default(ExternalCardVisualOwnership);
			}
			val = ((GodotObject)scriptedManager).Call(StringName.op_Implicit("has_override"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(text) });
			bool hasOverride = ((Variant)(ref val)).AsBool();
			val = ((GodotObject)scriptedManager).Call(StringName.op_Implicit("is_full_art_mode"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(text) });
			bool fullArt = ((Variant)(ref val)).AsBool();
			val = ((GodotObject)scriptedManager).Call(StringName.op_Implicit("is_ancient_text_outside_enabled"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(text) });
			ExternalCardVisualOwnershipState externalCardVisualOwnershipState = ExternalCardProviderIdentityPolicy.ResolveEditorOwnership(hasOverride, fullArt, ((Variant)(ref val)).AsBool());
			if (ArtPackCoveredCardTypes.Contains(NormalizeCardType(((object)card.Model).GetType().Name)))
			{
				return new ExternalCardVisualOwnership(externalCardVisualOwnershipState.Portrait, Frame: false, Text: false);
			}
			return new ExternalCardVisualOwnership(externalCardVisualOwnershipState.Portrait, externalCardVisualOwnershipState.Frame, externalCardVisualOwnershipState.Text);
		}
		catch (Exception exception)
		{
			LogScriptedFailure(scriptedManager, "视觉所有权读取", exception);
			return default(ExternalCardVisualOwnership);
		}
	}

	private static Node? GetScriptedManager()
	{
		WeakReference<Node>? scriptedManagerReference = _scriptedManagerReference;
		if (scriptedManagerReference != null && scriptedManagerReference.TryGetTarget(out Node target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && HasScriptedManagerCapabilities(target))
		{
			return target;
		}
		ulong ticksMsec = Time.GetTicksMsec();
		if (ticksMsec < _nextScriptedManagerProbeTicks)
		{
			return null;
		}
		_nextScriptedManagerProbeTicks = ticksMsec + 1000;
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null || val.Root == null || !GodotObject.IsInstanceValid((GodotObject)(object)val.Root))
		{
			return null;
		}
		foreach (Node item in ((IEnumerable)((Node)val.Root).GetChildren(false)).OfType<Node>())
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item) && HasScriptedManagerCapabilities(item))
			{
				_scriptedManagerReference = new WeakReference<Node>(item);
				if (!_scriptedManagerLogged)
				{
					_scriptedManagerLogged = true;
					ModLog.Info($"检测到脚本型卡牌视觉管理器 {item.Name}；" + "将按其公开能力同步最终卡图与编辑器覆盖层。");
				}
				return item;
			}
		}
		return null;
	}

	private static bool HasScriptedManagerCapabilities(Node candidate)
	{
		if (((GodotObject)candidate).HasMethod(StringName.op_Implicit("get_source_path_for_card_node")) && ((GodotObject)candidate).HasMethod(StringName.op_Implicit("has_override")) && ((GodotObject)candidate).HasMethod(StringName.op_Implicit("is_full_art_mode")) && ((GodotObject)candidate).HasMethod(StringName.op_Implicit("is_ancient_text_outside_enabled")) && ((GodotObject)candidate).HasMethod(StringName.op_Implicit("capture_card_provider_after_visual_update")))
		{
			return ((GodotObject)candidate).HasMethod(StringName.op_Implicit("apply_card_override_after_visual_update"));
		}
		return false;
	}

	private static ProviderCaptureScope? PrepareManagedProviderIdentity(NCard card)
	{
		TextureRect nodeOrNull = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Portrait"));
		TextureRect nodeOrNull2 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientPortrait"));
		TextureRect val = ((nodeOrNull2 != null && ((CanvasItem)nodeOrNull2).Visible) ? nodeOrNull2 : nodeOrNull);
		if (val != null && ((CanvasItem)val).Visible)
		{
			Texture2D texture = val.Texture;
			if (texture != null)
			{
				string externalCardProviderIdentity = SkinService.GetExternalCardProviderIdentity(card.Model, texture);
				if (externalCardProviderIdentity != null)
				{
					if (!ProviderTextureViews.TryGetValue(texture, out ProviderTextureView value) || value.Identity != externalCardProviderIdentity || !GodotObject.IsInstanceValid((GodotObject)(object)value.Texture))
					{
						AtlasTexture val2 = CreateProviderView(texture);
						string text = externalCardProviderIdentity;
						((Resource)val2).ResourcePath = text.Substring(0, text.Length - 4) + "_" + ((GodotObject)val2).GetInstanceId() + ".png";
						value = new ProviderTextureView(externalCardProviderIdentity, val2);
						ProviderTextureViews.Remove(texture);
						ProviderTextureViews.Add(texture, value);
					}
					return new ProviderCaptureScope(nodeOrNull, nodeOrNull2, (Texture2D)(object)value.Texture);
				}
			}
		}
		return null;
	}

	private static AtlasTexture CreateProviderView(Texture2D source)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		AtlasTexture val = (AtlasTexture)(object)((source is AtlasTexture) ? source : null);
		if (val != null)
		{
			return new AtlasTexture
			{
				Atlas = val.Atlas,
				Region = val.Region,
				Margin = val.Margin,
				FilterClip = val.FilterClip
			};
		}
		return new AtlasTexture
		{
			Atlas = source,
			FilterClip = false
		};
	}

	private static FullCardPresentationAdapter? CreateFullCardPresentationAdapter(Assembly assembly, IReadOnlyCollection<Type> types)
	{
		MethodInfo methodInfo = null;
		MethodInfo methodInfo2 = null;
		MethodInfo methodInfo3 = null;
		foreach (Type type in types)
		{
			if ((object)methodInfo == null)
			{
				methodInfo = FindCardStateMethod(type, "HasSignature", typeof(bool));
			}
			if ((object)methodInfo2 == null)
			{
				methodInfo2 = FindCardStateMethod(type, "IsEnableSignature", typeof(bool));
			}
			if ((object)methodInfo3 == null)
			{
				methodInfo3 = FindCardStateMethod(type, "GetCurrentSignature", null);
			}
		}
		bool flag = types.Any((Type type) => FindNodeMethod(type, "AfterReload", typeof(bool)) != null && FindNodeMethod(type, "ShowSignature", typeof(void)) != null && FindNodeMethod(type, "HideSignature", typeof(void)) != null);
		if (!(methodInfo != null && methodInfo2 != null && methodInfo3 != null && flag))
		{
			return null;
		}
		return new FullCardPresentationAdapter(assembly, methodInfo, methodInfo2, methodInfo3);
	}

	private static MethodInfo? FindCardStateMethod(Type type, string name, Type? returnType)
	{
		return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo method)
		{
			if (method.Name.Equals(name, StringComparison.Ordinal) && ((returnType == null) ? (method.ReturnType != typeof(void)) : (method.ReturnType == returnType)))
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == 1)
				{
					ParameterInfo parameterInfo = parameters[0];
					if (parameterInfo != null)
					{
						Type parameterType = parameterInfo.ParameterType;
						return AcceptsCardModel(parameterType);
					}
				}
			}
			return false;
		});
	}

	private static MethodInfo? FindNodeMethod(Type type, string name, Type returnType)
	{
		return type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo method)
		{
			if (method.Name.Equals(name, StringComparison.Ordinal) && method.ReturnType == returnType)
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == 1)
				{
					ParameterInfo parameterInfo = parameters[0];
					if (parameterInfo != null)
					{
						Type parameterType = parameterInfo.ParameterType;
						if (!(parameterType == typeof(NCard)))
						{
							return parameterType.IsAssignableFrom(typeof(NCard));
						}
						return true;
					}
				}
			}
			return false;
		});
	}

	private static bool AcceptsCardModel(Type parameterType)
	{
		if (!(parameterType == typeof(CardModel)))
		{
			return parameterType.IsAssignableFrom(typeof(CardModel));
		}
		return true;
	}

	private static bool IsFrameworkAssembly(Assembly assembly)
	{
		if (assembly == typeof(ExternalCardVisualBridge).Assembly || assembly == typeof(CardModel).Assembly)
		{
			return true;
		}
		string text = assembly.GetName().Name ?? string.Empty;
		if (!text.Equals("mscorlib", StringComparison.OrdinalIgnoreCase) && !text.Equals("netstandard", StringComparison.OrdinalIgnoreCase) && !text.Equals("GodotSharp", StringComparison.OrdinalIgnoreCase) && !text.Equals("0Harmony", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("System", StringComparison.OrdinalIgnoreCase))
		{
			return text.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static RegistryProbe? CreateRegistryProbe(IReadOnlyCollection<Type> types, string registryTypeName, string methodName)
	{
		Type type2 = types.FirstOrDefault((Type type) => type.Name.Equals(registryTypeName, StringComparison.Ordinal));
		if (type2 == null)
		{
			return null;
		}
		foreach (MethodInfo item in from method in type2.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where method.Name.Equals(methodName, StringComparison.Ordinal) && method.ReturnType == typeof(bool)
			select method)
		{
			ParameterInfo[] parameters = item.GetParameters();
			if (parameters.Length == 2 && !(parameters[0].ParameterType != typeof(string)) && parameters[1].ParameterType.IsByRef)
			{
				Type? elementType = parameters[1].ParameterType.GetElementType();
				MethodInfo methodInfo = elementType?.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((MethodInfo candidate) => candidate.Name.Equals("HasAnyValue", StringComparison.Ordinal) && candidate.ReturnType == typeof(bool) && candidate.GetParameters().Length == 0);
				if (elementType != null && methodInfo != null)
				{
					return new RegistryProbe(item, methodInfo);
				}
			}
		}
		return null;
	}

	private static Type[] GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type type) => type != null).Cast<Type>().ToArray();
		}
	}

	private static void LogFailure(Assembly assembly, string operation, Exception exception)
	{
		string item = assembly.FullName + "\n" + operation + "\n" + exception.GetType().FullName;
		lock (Sync)
		{
			if (!LoggedFailures.Add(item))
			{
				return;
			}
		}
		ModLog.Warn($"外部卡牌视觉管理器 {assembly.GetName().Name} 的{operation}失败，将由皮肤切换器继续呈现：" + exception.GetBaseException().Message);
	}

	private static void LogScriptedFailure(Node manager, string operation, Exception exception)
	{
		string item = $"scripted:{((GodotObject)manager).GetInstanceId()}\n{operation}\n{exception.GetType().FullName}";
		lock (Sync)
		{
			if (!LoggedFailures.Add(item))
			{
				return;
			}
		}
		ModLog.Warn($"脚本型卡牌视觉管理器 {manager.Name} 的{operation}失败，" + "将保留 Skin Changer 的当前显示：" + exception.GetBaseException().Message);
	}
}
