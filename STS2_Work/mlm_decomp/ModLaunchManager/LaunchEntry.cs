using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal sealed class LaunchEntry
{
	private ModLoadPriority? _declaredPriority;

	private bool _priorityResolved;

	private IReadOnlyList<ModIncompatibility>? _declaredIncompatibilities;

	private ModManifest? _incompatibilitiesSource;

	internal Mod Mod { get; }

	internal int OriginalIndex { get; }

	internal bool OriginalEnabled { get; }

	internal bool Enabled { get; set; }

	internal string Id => Mod.manifest?.id ?? "<unknown>";

	internal string Name
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Mod.manifest?.name))
			{
				return TextEncodingRepair.Repair(Mod.manifest.name);
			}
			return Id;
		}
	}

	internal string Alias { get; set; }

	internal string Group { get; set; }

	internal string TranslatedName { get; set; }

	internal string TranslatedDescription { get; set; }

	internal string DisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Alias))
			{
				return Alias;
			}
			if (!string.IsNullOrWhiteSpace(TranslatedName))
			{
				return TranslatedName;
			}
			return Name;
		}
	}

	internal string DescriptionOverride { get; set; }

	internal string ManifestDescription => TextEncodingRepair.Repair(Mod.manifest?.description);

	internal string DisplayDescription
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(DescriptionOverride))
			{
				return DescriptionOverride;
			}
			if (!string.IsNullOrWhiteSpace(TranslatedDescription))
			{
				return TranslatedDescription;
			}
			return ManifestDescription;
		}
	}

	internal bool IgnorePriority { get; set; }

	internal ModLoadPriority? DeclaredPriority
	{
		get
		{
			if (!_priorityResolved)
			{
				_priorityResolved = true;
				_declaredPriority = ModLoadPriorities.Declared(Mod);
			}
			return _declaredPriority;
		}
	}

	internal ModLoadPriority EffectivePriority
	{
		get
		{
			ModLoadPriority? declaredPriority = DeclaredPriority;
			if (declaredPriority.HasValue)
			{
				ModLoadPriority valueOrDefault = declaredPriority.GetValueOrDefault();
				if (!IgnorePriority)
				{
					return valueOrDefault;
				}
			}
			return ModLoadPriority.Default;
		}
	}

	internal bool PriorityIsBinding
	{
		get
		{
			if (DeclaredPriority.HasValue)
			{
				return !IgnorePriority;
			}
			return false;
		}
	}

	internal IReadOnlyList<ModIncompatibility> DeclaredIncompatibilities
	{
		get
		{
			if (_declaredIncompatibilities == null || _incompatibilitiesSource != Mod.manifest)
			{
				_incompatibilitiesSource = Mod.manifest;
				_declaredIncompatibilities = ModIncompatibilities.Declared(Mod);
			}
			return _declaredIncompatibilities;
		}
	}

	internal string Key => PhysicalModIdentity.KeyFor(Mod);

	internal bool IsManager => string.Equals(Id, "ModLaunchManager", StringComparison.Ordinal);

	internal bool IsCompat => string.Equals(Id, "CrossVersionCompat", StringComparison.Ordinal);

	internal bool IsDuplicate => (int)Mod.state == 4;

	internal bool CanToggle => !IsManager;

	internal bool CanMove => !IsManager;

	internal LaunchEntry(Mod mod, int originalIndex)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		ModLoadState state = mod.state;
		this._002Ector(mod, originalIndex, (int)state != 3 && (int)state != 4);
	}

	internal LaunchEntry(Mod mod, int originalIndex, bool enabled)
	{
		Alias = string.Empty;
		Group = string.Empty;
		TranslatedName = string.Empty;
		TranslatedDescription = string.Empty;
		DescriptionOverride = string.Empty;
		base._002Ector();
		Mod = mod;
		OriginalIndex = originalIndex;
		OriginalEnabled = enabled;
		Enabled = enabled;
	}

	internal void InvalidateManifestMetadata()
	{
		_priorityResolved = false;
		_declaredIncompatibilities = null;
	}
}
