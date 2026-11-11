using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class FrameworkSkinRuntime
{
	public static bool TryGetCharacterContract(CharacterModel model, out string groupId, out FrameworkCharacterSkinContract contract)
	{
		groupId = NormalizeToken(((AbstractModel)model).Id.Entry);
		if (SkinService.TryGetSelectedFrameworkContract(groupId, out contract))
		{
			return UsesDeclarativePresentation(contract);
		}
		return false;
	}

	internal static bool UsesDeclarativePresentation(FrameworkCharacterSkinContract contract)
	{
		return !FrameworkRegistryCooperation.UsesNativePresentation(contract);
	}

	public static bool TryGetCharacterResource(CharacterModel model, string propertyName, out string path)
	{
		path = string.Empty;
		if (TryGetCharacterContract(model, out FrameworkCharacterSkinContract contract))
		{
			return contract.CharacterResources.TryGetValue(propertyName, out path);
		}
		return false;
	}

	public static bool TryGetPoolResource(CardPoolModel pool, string propertyName, out string path)
	{
		path = string.Empty;
		if (SkinService.TryGetSelectedFrameworkContract(NormalizeToken(pool.Title), out FrameworkCharacterSkinContract contract) && UsesDeclarativePresentation(contract))
		{
			return contract.CharacterResources.TryGetValue(propertyName, out path);
		}
		return false;
	}

	public static bool TryGetCharacterColor(CharacterModel model, string propertyName, out Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		color = default(Color);
		if (TryGetCharacterContract(model, out FrameworkCharacterSkinContract contract) && contract.CharacterValues.TryGetValue(propertyName, out string value))
		{
			return TryParseColor(value, out color);
		}
		return false;
	}

	public static bool TryGetPoolColor(CardPoolModel pool, string propertyName, out Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		color = default(Color);
		if (SkinService.TryGetSelectedFrameworkContract(NormalizeToken(pool.Title), out FrameworkCharacterSkinContract contract) && UsesDeclarativePresentation(contract) && contract.CharacterValues.TryGetValue(propertyName, out string value))
		{
			return TryParseColor(value, out color);
		}
		return false;
	}

	public static bool TryGetEnergyLayers(CharacterModel model, out IReadOnlyList<string> paths)
	{
		paths = Array.Empty<string>();
		if (!TryGetCharacterContract(model, out FrameworkCharacterSkinContract contract) || !contract.CharacterResourceLists.TryGetValue("EnergyLayers", out IReadOnlyList<string> value))
		{
			return false;
		}
		paths = value;
		return true;
	}

	public static bool TryGetOrbResource(OrbModel model, string propertyName, out string path)
	{
		path = string.Empty;
		string targetName = ((object)model).GetType().Name;
		FrameworkModelSkinContract frameworkModelSkinContract = SkinService.GetSelectedFrameworkContracts().Where(UsesDeclarativePresentation).SelectMany((FrameworkCharacterSkinContract contract) => contract.Orbs)
			.LastOrDefault((FrameworkModelSkinContract candidate) => candidate.TargetModelName.Equals(targetName, StringComparison.Ordinal));
		if (frameworkModelSkinContract != null)
		{
			return frameworkModelSkinContract.Resources.TryGetValue(propertyName, out path);
		}
		return false;
	}

	public static bool TryGetOrbColor(OrbModel model, string propertyName, out Color color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		color = default(Color);
		string targetName = ((object)model).GetType().Name;
		FrameworkModelSkinContract frameworkModelSkinContract = SkinService.GetSelectedFrameworkContracts().Where(UsesDeclarativePresentation).SelectMany((FrameworkCharacterSkinContract contract) => contract.Orbs)
			.LastOrDefault((FrameworkModelSkinContract candidate) => candidate.TargetModelName.Equals(targetName, StringComparison.Ordinal));
		if (frameworkModelSkinContract != null && frameworkModelSkinContract.Values.TryGetValue(propertyName, out string value))
		{
			return TryParseColor(value, out color);
		}
		return false;
	}

	public static bool TryGetRelicResource(RelicModel model, string propertyName, out string path)
	{
		path = string.Empty;
		string targetName = ((object)model).GetType().Name;
		FrameworkModelSkinContract frameworkModelSkinContract = SkinService.GetSelectedFrameworkContracts().Where(UsesDeclarativePresentation).SelectMany((FrameworkCharacterSkinContract contract) => contract.Relics)
			.LastOrDefault((FrameworkModelSkinContract candidate) => candidate.TargetModelName.Equals(targetName, StringComparison.Ordinal));
		if (frameworkModelSkinContract != null)
		{
			return frameworkModelSkinContract.Resources.TryGetValue(propertyName, out path);
		}
		return false;
	}

	public static FrameworkRelicVisualPlan? ResolveRelicVisual(RelicModel model, bool largeIcon)
	{
		return FrameworkRelicVisualPolicy.Resolve(SkinService.GetSelectedFrameworkContracts().Where(UsesDeclarativePresentation).SelectMany((FrameworkCharacterSkinContract contract) => contract.Relics), ((object)model).GetType().Name, largeIcon);
	}

	public static bool HasSelectedCharacterContract(CharacterModel model)
	{
		FrameworkCharacterSkinContract contract;
		return TryGetCharacterContract(model, out contract);
	}

	public static string NormalizeToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static bool TryGetCharacterContract(CharacterModel model, out FrameworkCharacterSkinContract contract)
	{
		string groupId;
		return TryGetCharacterContract(model, out groupId, out contract);
	}

	private static bool TryParseColor(string value, out Color color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			color = new Color(value);
			return true;
		}
		catch
		{
			color = default(Color);
			return false;
		}
	}
}
