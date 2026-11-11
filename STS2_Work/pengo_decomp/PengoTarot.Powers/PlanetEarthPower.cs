using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.Powers;

public sealed class PlanetEarthPower : PowerModel
{
	internal static bool DisableEarthModifier;

	public List<Player> PairedPlayers { get; set; } = new List<Player>();

	public override PowerType Type => (PowerType)1;

	public override PowerStackType StackType => (PowerStackType)2;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new StringVar[1]
	{
		new StringVar("PairedName", "")
	};

	public void RefreshPairedName()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (PairedPlayers.Count != 0)
		{
			IEnumerable<string> values = PairedPlayers.Select((Player p) => PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, p.NetId));
			((StringVar)((PowerModel)this).DynamicVars["PairedName"]).StringValue = string.Join(", ", values);
		}
	}

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		RefreshPairedName();
		((PowerModel)this).Flash();
		return Task.CompletedTask;
	}

	public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (DisableEarthModifier)
		{
			return amount;
		}
		Creature owner = ((PowerModel)this).Owner;
		if (player != ((owner != null) ? owner.Player : null) || PairedPlayers.Count == 0)
		{
			return amount;
		}
		FieldInfo field = typeof(Player).GetField("<MaxEnergy>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			return amount;
		}
		HashSet<Player> hashSet = new HashSet<Player>();
		decimal num = default(decimal);
		foreach (Player pairedPlayer in PairedPlayers)
		{
			if (pairedPlayer != null && hashSet.Add(pairedPlayer) && pairedPlayer != player)
			{
				num += (decimal)(int)(field.GetValue(pairedPlayer) ?? ((object)0));
			}
		}
		return amount + num;
	}
}
