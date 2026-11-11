using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Characters;

public static class CznCharacterRegistry
{
	[HarmonyPatch(typeof(ModelDb), "get_AllCharacters")]
	private static class CharacterListPatch
	{
		private static void Postfix(ref IEnumerable<CharacterModel> __result)
		{
			__result = Merge(__result, CharacterTypes);
		}
	}

	private static readonly HashSet<Type> CharacterTypes = new HashSet<Type>();

	private static bool _patched;

	public static void Register<T>() where T : CharacterModel
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (typeof(T).IsAbstract)
		{
			throw new ArgumentException("Only concrete character models can be registered.");
		}
		if (!_patched)
		{
			new Harmony("czncore.character-registry").CreateClassProcessor(typeof(CharacterListPatch)).Patch();
			_patched = true;
		}
		CharacterTypes.Add(typeof(T));
	}

	internal static IEnumerable<CharacterModel> Merge(IEnumerable<CharacterModel> existing, IEnumerable<Type> registeredTypes)
	{
		HashSet<Type> types = registeredTypes.ToHashSet();
		if (types.Count == 0)
		{
			return existing;
		}
		IEnumerable<CharacterModel> second = (from type in types.OrderBy<Type, string>((Type type) => ((object)ModelDb.GetId(type)).ToString(), StringComparer.Ordinal)
			select ModelDb.GetByIdOrNull<CharacterModel>(ModelDb.GetId(type))).OfType<CharacterModel>();
		return existing.Where((CharacterModel character) => !types.Contains(((object)character).GetType())).Concat(second).ToArray();
	}
}
