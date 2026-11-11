using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;
using St2bs.St2bsCode.Ancients;
using St2bs.St2bsCode.Encounters;
using St2bs.St2bsCode.Events;

namespace St2bs.St2bsCode.Acts;

public sealed class CarrollRiver : ActModel
{
	public override int Index => 4;

	public override bool IsDefault => false;

	public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";

	public override IEnumerable<EncounterModel> BossDiscoveryOrder => new <>z__ReadOnlySingleElementList<EncounterModel>((EncounterModel)(object)ModelDb.Encounter<JabberwockEncounter>());

	public override IEnumerable<AncientEventModel> AllAncients => new <>z__ReadOnlySingleElementList<AncientEventModel>((AncientEventModel)(object)ModelDb.AncientEvent<JabberwockAncient>());

	public override IEnumerable<EventModel> AllEvents => Array.Empty<EventModel>();

	public IReadOnlyList<EventModel> ExploreEvents => new List<EventModel>
	{
		(EventModel)(object)ModelDb.Event<CarrollChickEvent>(),
		(EventModel)(object)ModelDb.Event<BanaeiEvent>(),
		(EventModel)(object)ModelDb.Event<CarrollRiverCorpseEvent>(),
		(EventModel)(object)ModelDb.Event<CarrollRiverDungeonEvent>(),
		(EventModel)(object)ModelDb.Event<CarrollRiverFisherEvent>(),
		(EventModel)(object)ModelDb.Event<CarrollRiverChickenEvent>(),
		(EventModel)(object)ModelDb.Event<CarrollRiverStrongEncounterEvent>(),
		(EventModel)(object)ModelDb.Event<CarrollRiverWeakEncounterEvent>(),
		(EventModel)(object)ModelDb.Event<OldSheepShopEvent>()
	};

	protected override int NumberOfWeakEncounters => 3;

	protected override int BaseNumberOfRooms => 10;

	public override string[] BgMusicOptions => new string[1] { "event:/music/act3_a1_v1" };

	public override string[] MusicBankPaths => new string[1] { "res://banks/desktop/act3_a1.bank" };

	public override string AmbientSfx => "event:/sfx/ambience/act3_ambience";

	public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_3_skel_data.tres";

	public override string ChestSpineSkinNameNormal => "act3";

	public override string ChestSpineSkinNameStroke => "act3_stroke";

	public override Color MapTraveledColor => new Color("F2F4F8");

	public override Color MapUntraveledColor => new Color("FFFFFF");

	public override Color MapBgColor => new Color("9FB6CF");

	public override bool IsUnlocked(UnlockState unlockState)
	{
		return true;
	}

	public override IEnumerable<EncounterModel> GenerateAllEncounters()
	{
		return new <>z__ReadOnlyArray<EncounterModel>((EncounterModel[])(object)new EncounterModel[11]
		{
			(EncounterModel)ModelDb.Encounter<JabberwockEncounter>(),
			(EncounterModel)ModelDb.Encounter<FromYuggothEncounter>(),
			(EncounterModel)ModelDb.Encounter<SnowManEncounter>(),
			(EncounterModel)ModelDb.Encounter<CarrollWinterKnightMentorEncounter>(),
			(EncounterModel)ModelDb.Encounter<IceStormBeastEncounter>(),
			(EncounterModel)ModelDb.Encounter<CarrollSnowManFromYuggothEncounter>(),
			(EncounterModel)ModelDb.Encounter<SantaClausEncounter>(),
			(EncounterModel)ModelDb.Encounter<FrostHeadSharkEncounter>(),
			(EncounterModel)ModelDb.Encounter<SteelHeadSharkEncounter>(),
			(EncounterModel)ModelDb.Encounter<NarcissisticBeastEncounter>(),
			(EncounterModel)ModelDb.Encounter<GreedyDogEncounter>()
		});
	}

	public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
	{
		return new <>z__ReadOnlySingleElementList<AncientEventModel>((AncientEventModel)(object)ModelDb.AncientEvent<JabberwockAncient>());
	}

	protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
	{
	}

	public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		int num = MapPointTypeCounts.StandardRandomUnknownCount(mapRng);
		MapPointTypeCounts val = new MapPointTypeCounts(num, 0);
		val.set_NumOfElites(2);
		return val;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.0.0.7889')
