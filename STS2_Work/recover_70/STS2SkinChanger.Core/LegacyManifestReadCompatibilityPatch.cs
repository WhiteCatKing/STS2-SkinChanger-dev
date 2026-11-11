using System.IO;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(ModManifest), "ReadFromStream")]
internal static class LegacyManifestReadCompatibilityPatch
{
	private static void Prefix(ref Stream stream, out MemoryStream? __state)
	{
		__state = ProviderManifestCompatibility.PrepareLegacyStream(stream);
		if (__state != null)
		{
			stream = __state;
		}
	}

	private static void Finalizer(MemoryStream? __state)
	{
		__state?.Dispose();
	}
}
