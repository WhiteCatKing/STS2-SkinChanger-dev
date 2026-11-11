using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Modding;

namespace CznCore.Telemetry;

internal sealed class OptionalCharacterTelemetry
{
	private bool _attempted;

	public void Initialize(Assembly owner, string modId, string resourceName, Func<string> characterId, Action<string> info, Action<string> warn)
	{
		if (_attempted)
		{
			return;
		}
		_attempted = true;
		try
		{
			Mod[] source = ModManager.GetLoadedMods().ToArray();
			Mod val = ((IEnumerable<Mod>)source).FirstOrDefault((Func<Mod, bool>)((Mod mod) => mod.manifest?.id == "STS2-RitsuLib"));
			if (val == null)
			{
				return;
			}
			if (!Version.TryParse((val.manifest?.version ?? string.Empty).TrimStart('v').Split('-')[0], out Version result) || result < new Version(0, 6, 4))
			{
				info(modId + " telemetry unavailable: requires RitsuLib 0.6.4 or newer.");
				return;
			}
			using Stream assembly = owner.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Embedded telemetry bridge is missing.");
			Assembly assembly2 = (AssemblyLoadContext.GetLoadContext(owner) ?? AssemblyLoadContext.Default).LoadFromStream(assembly);
			string text = ((IEnumerable<Mod>)source).FirstOrDefault((Func<Mod, bool>)((Mod mod) => mod.manifest?.id == modId))?.manifest?.version ?? "unknown";
			assembly2.GetType("CznCore.CharacterTelemetry.Bootstrap", throwOnError: true).GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[3]
			{
				characterId(),
				text,
				info
			});
		}
		catch (Exception ex)
		{
			warn(modId + " telemetry disabled: " + ex.GetBaseException().GetType().Name + ".");
		}
	}
}
