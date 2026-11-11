using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Modding;

namespace CznCore.Standalone;

internal static class GameplayRelevance
{
	internal static string[] Consumers(IEnumerable<Mod> mods)
	{
		Dictionary<string, Mod> loaded = mods.Where((Mod m) => (int)m.state == 1 && m.manifest?.id != null).ToDictionary<Mod, string>((Mod m) => m.manifest.id, StringComparer.Ordinal);
		return (from m in loaded.Values
			where m.manifest.id != "CznCore" && m.manifest.affectsGameplay && DependsOnCore(m.manifest.id, new HashSet<string>(StringComparer.Ordinal))
			select m.manifest.id).OrderBy<string, string>((string id) => id, StringComparer.Ordinal).ToArray();
		bool DependsOnCore(string id, HashSet<string> visited)
		{
			if (id == "CznCore")
			{
				return true;
			}
			if (!visited.Add(id) || !loaded.TryGetValue(id, out var value))
			{
				return false;
			}
			return value.manifest.dependencies?.Any((ModDependency d) => DependsOnCore(d.id, visited)) ?? false;
		}
	}

	internal static void Refresh()
	{
		Mod[] array = ModManager.Mods.ToArray();
		Mod val = ((IEnumerable<Mod>)array).FirstOrDefault((Func<Mod, bool>)((Mod m) => m.manifest?.id == "CznCore" && (int)m.state == 1));
		if (!(val?.manifest == (ModManifest)null))
		{
			string[] array2 = Consumers(array);
			bool flag = array2.Length != 0;
			if (val.manifest.affectsGameplay != flag)
			{
				val.manifest.affectsGameplay = flag;
				Console.WriteLine("[CznCore] Multiplayer gameplay relevance=" + flag + " loaded gameplay consumers=" + string.Join(",", array2));
			}
		}
	}
}
