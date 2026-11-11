using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

internal static class StatefulCardArtRuntime
{
	internal sealed class Runtime
	{
		private readonly Type _settings;

		private readonly Type _art;

		private readonly Dictionary<Type, Type> _handlers = new Dictionary<Type, Type>();

		private readonly Dictionary<Type, object?> _instances = new Dictionary<Type, object>();

		private readonly Dictionary<string, PropertyInfo?> _settingsProperties = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);

		private readonly Dictionary<string, byte[]> _localizations = new Dictionary<string, byte[]>(StringComparer.Ordinal);

		internal StatefulCardArtContract Contract { get; }

		internal Assembly Assembly { get; }

		internal bool Enabled => Setting("UseCustomArt", fallback: true);

		internal bool Simple => Setting("UseSimpleMode");

		internal Runtime(StatefulCardArtContract contract, Assembly assembly)
		{
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a9: Expected O, but got Unknown
			Contract = contract;
			Assembly = assembly;
			_settings = assembly.GetType(contract.SettingsType, throwOnError: true);
			_art = assembly.GetType(contract.ArtType, throwOnError: true);
			byte[] array = _art.TypeInitializer?.GetMethodBody()?.GetILAsByteArray();
			if (array == null || array.Length != 1 || array[0] != 42)
			{
				throw new InvalidOperationException("Provider was already initialized outside the isolated loader; restart with corrected load order.");
			}
			using (PckArchive pckArchive = PckArchive.Open(Path.ChangeExtension(contract.AssemblyPath, ".pck")))
			{
				if (Contract.AvailablePortraits.Count == 0)
				{
					Contract = contract with
					{
						AvailablePortraits = pckArchive.Paths.Where((string path) => path.StartsWith(contract.AtlasRoot, StringComparison.Ordinal) && path.EndsWith(".tres", StringComparison.Ordinal)).ToHashSet<string>(StringComparer.Ordinal)
					};
				}
				foreach (string item in pckArchive.Paths.Where((string path) => path.StartsWith(contract.ResourceRoot + "/localization/", StringComparison.Ordinal) && new string[3] { "artists.json", "usernames.json", "event_chatter.json" }.Contains<string>(Path.GetFileName(path))))
				{
					_localizations[item] = pckArchive.ReadFile(item);
				}
			}
			InstallLocalization();
			AccessTools.Method(_settings, "Init", (Type[])null, (Type[])null).Invoke(null, null);
			RegisterHandlers(assembly);
			if (typeof(CardModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Cards.Dowsing") != null)
			{
				string text = Path.ChangeExtension(contract.AssemblyPath, ".Beta.betapack");
				if (File.Exists(text))
				{
					RegisterHandlers(AssemblyLoadContext.GetLoadContext(assembly).LoadFromAssemblyPath(text));
				}
			}
			Type type2 = assembly.GetTypes().FirstOrDefault((Type type) => type.Name == "NCardAwareCardsPatch");
			if (type2 != null)
			{
				MethodInfo methodInfo = AccessTools.Method(type2, "get_Node", new Type[1] { typeof(CardModel) }, (Type[])null);
				if ((object)methodInfo != null)
				{
					new Harmony("Gurio.SkinChanger.stateful-card-node").Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(StatefulCardArtRuntime), "NodeForCard", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
			}
		}

		private void RegisterHandlers(Assembly assembly)
		{
			foreach (Type item in from type in assembly.GetTypes()
				where !type.IsAbstract && (type.BaseType?.IsGenericType ?? false)
				select type)
			{
				if (!(item.BaseType.GetGenericTypeDefinition().FullName != Contract.ArtType + "`1"))
				{
					_handlers[item.BaseType.GetGenericArguments()[0]] = item;
				}
			}
		}

		internal bool Setting(string name, bool fallback = false)
		{
			try
			{
				return (Property(name)?.GetValue(null) is bool flag) ? flag : fallback;
			}
			catch (Exception exception)
			{
				Warn("setting " + name, exception);
				return fallback;
			}
		}

		private PropertyInfo? Property(string name)
		{
			if (!_settingsProperties.TryGetValue(name, out PropertyInfo value))
			{
				value = (_settingsProperties[name] = AccessTools.Property(_settings, name));
			}
			return value;
		}

		internal void InstallLocalization()
		{
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Expected O, but got Unknown
			if (LocManager.Instance == null || !(AccessTools.Field(typeof(LocManager), "_tables").GetValue(LocManager.Instance) is Dictionary<string, LocTable> dictionary))
			{
				return;
			}
			string[] array = new string[3] { "artists", "usernames", "event_chatter" };
			foreach (string text in array)
			{
				Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.Ordinal);
				foreach (string item in new string[2]
				{
					"eng",
					LocManager.Instance.Language
				}.Distinct())
				{
					string text2 = Contract.ResourceRoot + "/localization/" + item + "/" + text + ".json";
					if (!_localizations.TryGetValue(text2, out byte[] value))
					{
						continue;
					}
					try
					{
						Dictionary<string, string> dictionary3 = JsonSerializer.Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetString(value).TrimStart('\ufeff'));
						if (dictionary3 == null)
						{
							continue;
						}
						foreach (KeyValuePair<string, string> item2 in dictionary3)
						{
							dictionary2[item2.Key] = item2.Value;
						}
					}
					catch (JsonException ex)
					{
						if (Warnings.Add("localization:" + text2 + ex.Message))
						{
							ModLog.Warn($"Stateful card art localization {text2}: {ex.Message}; skipping this optional text table only, card visuals remain enabled.");
						}
					}
				}
				string text3 = Contract.LocalizationPrefix + text;
				dictionary[text3] = new LocTable(text3, dictionary2, (LocTable)null);
			}
		}

		internal IEnumerable<IHoverTip> Credits(CardModel card)
		{
			if (!(AccessTools.Method(Assembly.GetType(Contract.SettingsType.Substring(0, Contract.SettingsType.LastIndexOf('.')) + ".Credits"), "Tooltips", (Type[])null, (Type[])null)?.Invoke(null, new object[1] { card }) is IEnumerable<IHoverTip> source))
			{
				return Array.Empty<IHoverTip>();
			}
			return source.ToArray();
		}

		internal string? Portrait(CardModel card, string? baseline)
		{
			string path = null;
			try
			{
				Type type = ((object)card).GetType();
				if ((!Simple || type.Name == "MadScience") && _handlers.TryGetValue(type, out Type value))
				{
					if (!_instances.TryGetValue(value, out object value2))
					{
						value2 = (_instances[value] = Activator.CreateInstance(value, nonPublic: true));
					}
					if (value2 != null)
					{
						object obj2 = _art.GetMethod("Get", new Type[1] { typeof(CardModel) }).Invoke(value2, new object[1] { card });
						path = obj2?.GetType().GetProperty("PortraitPath")?.GetValue(obj2) as string;
					}
				}
			}
			catch (Exception exception)
			{
				Warn(((object)card).GetType().Name, exception);
			}
			return ValidatePortrait(Contract, path, card.IsUpgraded) ?? ValidatePortrait(Contract, baseline, card.IsUpgraded);
		}

		internal CardPresentationDefinition? Presentation(CardModel card)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (!Enabled)
			{
				return null;
			}
			int num;
			if (!Setting("MakeEverythingAncient"))
			{
				CardRarity rarity = card.Rarity;
				num = ((((object)(CardRarity)(ref rarity)/*cast due to .constrained prefix*/).ToString() == "Ancient") ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			bool flag = (byte)num != 0;
			bool useExpandedPortraitLayout = !flag;
			bool? bannerVisible = !Setting("HideTitleBanner", fallback: true);
			bool? textBackgroundVisible = !Setting("HideDescription", fallback: true);
			bool? descriptionVisible = !Setting("HideDescription", fallback: true);
			bool? energyIconVisible = !Setting("HideEnergy", fallback: true) || (((object)card).GetType().Name == "FranticEscape" && Setting("ExcludeFranticEscape"));
			return new CardPresentationDefinition(TypeLabelVisible: !Setting("HideType", fallback: true), TypePlaqueVisible: !Setting("HideType", fallback: true), UseAncientLayout: flag, Frame: null, FrameMaterial: null, BannerTexture: null, BannerMaterial: null, PortraitBorder: null, PortraitBorderMaterial: null, AncientTextBackground: null, TextBackgroundMaterial: null, EnergyIcon: null, Highlight: null, HighlightMaterial: null, FrameVisible: null, BannerVisible: bannerVisible, TextBackgroundVisible: textBackgroundVisible, PortraitBorderVisible: null, EnergyIconVisible: energyIconVisible, HighlightVisible: null, DescriptionVisible: descriptionVisible, InfectionOverlayVisible: null, UseFullFrameArt: false, UseExpandedPortraitLayout: useExpandedPortraitLayout, FrameOverlay: null, PortraitVisible: null, FrameOverlayOffsetTop: null, FrameOverlayOffsetBottom: null, FrameOverlayOffsetLeft: null, FrameOverlayOffsetRight: null, FrameOverlayScaleX: null, FrameOverlayScaleY: null, BuiltInOverlayVisible: null);
		}
	}

	private static readonly Dictionary<string, Runtime?> Runtimes = new Dictionary<string, Runtime>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, long> RetryAt = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> Warnings = new HashSet<string>(StringComparer.Ordinal);

	private static readonly ConditionalWeakTable<CardModel, WeakReference<NCard>> CardNodes = new ConditionalWeakTable<CardModel, WeakReference<NCard>>();

	internal static void Bind(NCard card)
	{
		if (card.Model != null)
		{
			CardNodes.Remove(card.Model);
			CardNodes.Add(card.Model, new WeakReference<NCard>(card));
		}
	}

	internal static bool NodeForCard(CardModel __0, ref NCard? __result)
	{
		__result = ((CardNodes.TryGetValue(__0, out WeakReference<NCard> value) && value.TryGetTarget(out var target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && target.Model == __0) ? target : null);
		return false;
	}

	internal static Runtime? For(CardSkinOption? option)
	{
		if (option?.StatefulArt == null || option.ProviderId == null)
		{
			return null;
		}
		if (Runtimes.TryGetValue(option.ProviderId, out Runtime value) && value != null)
		{
			return value;
		}
		if (RetryAt.GetValueOrDefault(option.ProviderId) > Environment.TickCount64)
		{
			return null;
		}
		try
		{
			Assembly statefulCardArtAssembly = ManagedSkinModLoader.GetStatefulCardArtAssembly(option.ProviderId);
			if (statefulCardArtAssembly == null)
			{
				return null;
			}
			if (Runtimes.TryGetValue(option.ProviderId, out value) && value != null)
			{
				return value;
			}
			if (RetryAt.GetValueOrDefault(option.ProviderId) > Environment.TickCount64)
			{
				return null;
			}
			value = new Runtime(option.StatefulArt, statefulCardArtAssembly);
			Runtimes[option.ProviderId] = value;
			ModLog.Info("Stateful card art ready: " + option.ProviderId + "; per-card ownership, no original PatchAll or preload.");
			return value;
		}
		catch (Exception exception)
		{
			Warn(option.ProviderId, exception);
			Runtimes[option.ProviderId] = null;
			RetryAt[option.ProviderId] = Environment.TickCount64 + 2000;
			return null;
		}
	}

	internal static Runtime? For(CardModel? card)
	{
		if (card != null)
		{
			return For(SkinService.GetSelectedCardOption(card));
		}
		return null;
	}

	internal static void Warn(string operation, Exception exception)
	{
		Exception baseException = exception.GetBaseException();
		if (Warnings.Add(operation + baseException.Message))
		{
			ModLog.Warn($"Stateful card art {operation}: {baseException.Message}; retaining safe card rendering.");
		}
	}

	internal static string? ValidatePortrait(StatefulCardArtContract contract, string? path, bool upgraded)
	{
		if (path == null || !path.StartsWith(contract.AtlasRoot, StringComparison.Ordinal) || !path.EndsWith(".tres", StringComparison.Ordinal) || path.Contains("..", StringComparison.Ordinal) || !contract.AvailablePortraits.Contains(path))
		{
			return null;
		}
		if (upgraded && !path.EndsWith("_plus.tres", StringComparison.Ordinal))
		{
			string text = path.Substring(0, path.Length - 5) + "_plus.tres";
			if (contract.AvailablePortraits.Contains(text))
			{
				return text;
			}
		}
		return path;
	}

	internal static void RefreshLocalization()
	{
		foreach (Runtime item in Runtimes.Values.OfType<Runtime>())
		{
			try
			{
				item.InstallLocalization();
			}
			catch (Exception exception)
			{
				Warn("localization", exception);
			}
		}
	}

	internal static bool Owns(Assembly assembly)
	{
		return Runtimes.Values.Any((Runtime runtime) => (object)runtime?.Assembly == assembly);
	}

	internal static Runtime? Ensure(string providerId, StatefulCardArtContract contract)
	{
		return For(new CardSkinOption(providerId, providerId, new Dictionary<string, string>(), new Dictionary<string, AncientCardPortrait>(), null, null, providerId)
		{
			StatefulArt = contract
		});
	}
}
