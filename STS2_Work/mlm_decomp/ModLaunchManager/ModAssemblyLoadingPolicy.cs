using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModAssemblyLoadingPolicy
{
	internal static Dictionary<Mod, ModAssemblyLoadDecision> Build(IEnumerable<Mod> mods)
	{
		Mod[] source = mods.Where(delegate(Mod mod)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			bool flag = mod.manifest?.hasDll ?? false;
			if (flag)
			{
				ModLoadState state = mod.state;
				bool flag2 = (int)state <= 1;
				flag = flag2;
			}
			return flag;
		}).Distinct().ToArray();
		Dictionary<Mod, ModAssemblyLoadDecision> decisions = source.ToDictionary((Mod mod) => mod, ReadRequirement);
		Dictionary<string, Mod[]> dictionary = source.Where((Mod mod) => mod.manifest?.id != null).GroupBy<Mod, string>((Mod mod) => mod.manifest.id, StringComparer.Ordinal).ToDictionary<IGrouping<string, Mod>, string, Mod[]>((IGrouping<string, Mod> group) => group.Key, (IGrouping<string, Mod> group) => group.ToArray(), StringComparer.Ordinal);
		Queue<Mod> queue = new Queue<Mod>(source.Where((Mod mod) => decisions[mod].UseGameContext));
		Mod result;
		while (queue.TryDequeue(out result))
		{
			if (result.manifest?.dependencies == null)
			{
				continue;
			}
			foreach (ModDependency dependency in result.manifest.dependencies)
			{
				if (dependency.id == null || !dictionary.TryGetValue(dependency.id, out var value))
				{
					continue;
				}
				Mod[] array = value;
				foreach (Mod val in array)
				{
					if (!decisions[val].UseGameContext)
					{
						decisions[val] = new ModAssemblyLoadDecision(UseGameContext: true, "dependency of game-context mod " + result.manifest.id);
						queue.Enqueue(val);
					}
				}
			}
		}
		return decisions;
	}

	internal static ModAssemblyLoadDecision ReadRequirement(Mod mod)
	{
		string text = ModManifestReader.FindManifestFile(mod);
		if (text == null)
		{
			return Automatic();
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(text, Encoding.UTF8));
			JsonElement rootElement = jsonDocument.RootElement;
			if (rootElement.ValueKind != JsonValueKind.Object)
			{
				return new ModAssemblyLoadDecision(UseGameContext: true, "manifest changed while preparing loading policy; preserving native loading");
			}
			if (!rootElement.TryGetProperty("mod_launch_manager", out var value))
			{
				return Automatic();
			}
			if (value.ValueKind != JsonValueKind.Object)
			{
				return new ModAssemblyLoadDecision(UseGameContext: true, "unrecognized mod_launch_manager declaration; preserving native loading");
			}
			if (!value.TryGetProperty("load_context", out var value2))
			{
				return Automatic();
			}
			if (value2.ValueKind == JsonValueKind.String)
			{
				if (value2.GetString() == "game")
				{
					return new ModAssemblyLoadDecision(UseGameContext: true, "manifest requests the game's load context");
				}
				if (value2.GetString() == "auto")
				{
					return Automatic();
				}
			}
			return new ModAssemblyLoadDecision(UseGameContext: true, "unrecognized load_context declaration; preserving native loading");
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? 1 : 0) != 0)
		{
			return new ModAssemblyLoadDecision(UseGameContext: true, "could not read loading declaration (" + ex.GetType().Name + "); preserving native loading");
		}
		ModAssemblyLoadDecision Automatic()
		{
			if (!string.IsNullOrWhiteSpace(mod.path) && !string.IsNullOrWhiteSpace(mod.manifest?.id))
			{
				return ModAssemblyPreflight.Inspect(Path.Combine(mod.path, mod.manifest.id + ".dll"));
			}
			return new ModAssemblyLoadDecision(UseGameContext: false, "no runtime path to inspect");
		}
	}
}
