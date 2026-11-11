using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.RestSite;

namespace PengoTarot.Relics;

public sealed class StargazerKit : RelicModel
{
	private const int InitialCharges = 1;

	private const int AncientBonusCharges = 2;

	private int _timesUsed;

	private int _totalCharges = 1;

	public override RelicRarity Rarity => (RelicRarity)6;

	public override bool ShowCounter => !((RelicModel)this).IsUsedUp;

	public override bool IsUsedUp => TimesUsed >= TotalCharges;

	public override int DisplayAmount => TotalCharges - TimesUsed;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("Uses", (decimal)TotalCharges)
	};

	[SavedProperty]
	public int TimesUsed
	{
		get
		{
			return _timesUsed;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_timesUsed = value;
			UpdateDynamicVars();
			((RelicModel)this).InvokeDisplayAmountChanged();
			CheckIfUsedUp();
		}
	}

	[SavedProperty]
	public int TotalCharges
	{
		get
		{
			return _totalCharges;
		}
		set
		{
			((AbstractModel)this).AssertMutable();
			_totalCharges = value;
			UpdateDynamicVars();
			((RelicModel)this).InvokeDisplayAmountChanged();
			CheckIfUsedUp();
		}
	}

	private void UpdateDynamicVars()
	{
		((RelicModel)this).DynamicVars["Uses"].BaseValue = TotalCharges - TimesUsed;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		MapPoint currentMapPoint = ((RelicModel)this).Owner.RunState.CurrentMapPoint;
		if (currentMapPoint != null && (int)currentMapPoint.PointType == 8 && room.ModelId != ((AbstractModel)ModelDb.AncientEvent<Neow>()).Id)
		{
			TotalCharges += 2;
		}
		return Task.CompletedTask;
	}

	public void UseCharge()
	{
		if (!((RelicModel)this).IsUsedUp)
		{
			TimesUsed++;
		}
	}

	private void CheckIfUsedUp()
	{
		((RelicModel)this).Status = (RelicStatus)(((RelicModel)this).IsUsedUp ? 2 : 0);
	}

	public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
	{
		if (player != ((RelicModel)this).Owner)
		{
			return false;
		}
		if (((RelicModel)this).IsUsedUp)
		{
			return false;
		}
		options.Add((RestSiteOption)(object)new StargazeRestSiteOption(player));
		return true;
	}
}
