using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Powers;

public sealed class PlanetJupiterHitMarkerPower : PowerModel
{
	public override PowerType Type => (PowerType)2;

	public override PowerStackType StackType => (PowerStackType)1;

	protected override bool IsVisibleInternal => false;
}
