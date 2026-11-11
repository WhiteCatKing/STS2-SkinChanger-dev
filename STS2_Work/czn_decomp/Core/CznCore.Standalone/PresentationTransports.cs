using System;
using System.Collections.Generic;
using CznCore.Presentation;
using HarmonyLib;

namespace CznCore.Standalone;

public static class PresentationTransports
{
	public const string HarmonyId = "local.czncore.presentation";

	private static readonly TransportRegistration Registration = new TransportRegistration();

	private static readonly Harmony Patches = new Harmony("local.czncore.presentation");

	public static void Ensure(PresentationTransport transport)
	{
		StandaloneEntry.RequireApi(1);
		EnsureTypes(transport switch
		{
			PresentationTransport.Cards => SkinCardTransportPatches.PatchTypes, 
			PresentationTransport.RoomsAndCombat => SkinTransportPatches.PatchTypes, 
			PresentationTransport.CharacterLifecycle => CharacterLifecycleTransport.PatchTypes, 
			PresentationTransport.BlockSound => CombatBlockGainSfxReplacementPatches.PatchTypes, 
			PresentationTransport.Defense => SharedDefenseTransport.PatchTypes, 
			_ => throw new ArgumentOutOfRangeException("transport"), 
		});
	}

	internal static void EnsureTypes(IEnumerable<Type> types)
	{
		StandaloneEntry.RequireApi(1);
		Registration.Ensure(types, Install);
	}

	internal static IReadOnlyList<Type> TryEnsureTypes(IEnumerable<Type> types, Action<Type, Exception> reportFailure)
	{
		StandaloneEntry.RequireApi(1);
		return Registration.TryEnsure(types, Install, reportFailure);
	}

	private static void Install(Type type)
	{
		Patches.CreateClassProcessor(type).Patch();
	}
}
