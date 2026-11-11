using System;
using System.Globalization;
using System.IO;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class PhysicalModIdentity
{
	internal static CanonicalLiveDescriptor Describe(Mod mod, int index, bool? enabled = null)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Invalid comparison between Unknown and I4
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		string? source = ((object)(ModSource)(ref mod.modSource)/*cast due to .constrained prefix*/).ToString();
		string id = mod.manifest?.id ?? string.Empty;
		string path = NormalizePath(SourcePath(mod));
		string? workshopItemId = WorkshopItemId(mod);
		bool? flag = enabled;
		int enabled2;
		if (!flag.HasValue)
		{
			ModLoadState state = mod.state;
			enabled2 = (((int)state != 3 && (int)state != 4) ? 1 : 0);
		}
		else
		{
			enabled2 = ((flag == true) ? 1 : 0);
		}
		return new CanonicalLiveDescriptor(index, source, id, path, workshopItemId, (byte)enabled2 != 0);
	}

	internal static bool Same(Mod left, Mod right)
	{
		CanonicalLiveDescriptor canonicalLiveDescriptor = Describe(left, 0, null);
		CanonicalLiveDescriptor live = Describe(right, 1, null);
		return CanonicalLaunchReconciler.ExactMatch(new CanonicalLaunchEntry
		{
			Source = canonicalLiveDescriptor.Source,
			Id = canonicalLiveDescriptor.Id,
			Path = canonicalLiveDescriptor.Path,
			WorkshopItemId = canonicalLiveDescriptor.WorkshopItemId
		}, live);
	}

	internal static string KeyFor(Mod mod)
	{
		CanonicalLiveDescriptor canonicalLiveDescriptor = Describe(mod, 0, null);
		string value = canonicalLiveDescriptor.WorkshopItemId ?? canonicalLiveDescriptor.Path;
		return $"{canonicalLiveDescriptor.Source}:{canonicalLiveDescriptor.Id}:{value}";
	}

	internal static string NormalizePath(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return string.Empty;
		}
		try
		{
			return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');
		}
		catch
		{
			return path.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '/');
		}
	}

	internal static string SourcePath(Mod mod)
	{
		return WorkshopBranches.SourcePathFor(mod);
	}

	internal static ulong WorkshopItemIdFor(Mod mod)
	{
		return WorkshopUpdates.ItemIdFor(mod);
	}

	internal static ulong ParseWorkshopItemId(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return 0uL;
		}
		string text = $"/{2868840}/";
		string text2 = path.Replace('\\', '/');
		int num = text2.IndexOf(text, StringComparison.Ordinal);
		if (num < 0)
		{
			return 0uL;
		}
		int num2 = num + text.Length;
		int num3 = text2.IndexOf('/', num2);
		string s;
		if (num3 >= 0)
		{
			int num4 = num2;
			s = text2.Substring(num4, num3 - num4);
		}
		else
		{
			s = text2.Substring(num2);
		}
		if (!ulong.TryParse(s, out var result))
		{
			return 0uL;
		}
		return result;
	}

	private static string? WorkshopItemId(Mod mod)
	{
		ulong num = WorkshopItemIdFor(mod);
		if (num != 0L)
		{
			return num.ToString(CultureInfo.InvariantCulture);
		}
		return null;
	}
}
