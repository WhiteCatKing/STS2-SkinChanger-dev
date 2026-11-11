using System.Runtime.CompilerServices;
using System.Text;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace CznCore.Presentation;

internal readonly record struct DamageReceivedEvent(PlayerChoiceContext ChoiceContext, IRunState RunState, ICombatState? CombatState, Creature Target, DamageResult Result, ValueProp Props, Creature? Dealer, CardModel? CardSource)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		builder.Append("ChoiceContext = ");
		builder.Append(ChoiceContext);
		builder.Append(", RunState = ");
		builder.Append(RunState);
		builder.Append(", CombatState = ");
		builder.Append(CombatState);
		builder.Append(", Target = ");
		builder.Append(Target);
		builder.Append(", Result = ");
		builder.Append(Result);
		builder.Append(", Props = ");
		ValueProp props = Props;
		builder.Append(((object)(ValueProp)(ref props)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", Dealer = ");
		builder.Append(Dealer);
		builder.Append(", CardSource = ");
		builder.Append(CardSource);
		return true;
	}

	[CompilerGenerated]
	public void Deconstruct(out PlayerChoiceContext ChoiceContext, out IRunState RunState, out ICombatState? CombatState, out Creature Target, out DamageResult Result, out ValueProp Props, out Creature? Dealer, out CardModel? CardSource)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected I4, but got Unknown
		ChoiceContext = this.ChoiceContext;
		RunState = this.RunState;
		CombatState = this.CombatState;
		Target = this.Target;
		Result = this.Result;
		Props = (ValueProp)(int)this.Props;
		Dealer = this.Dealer;
		CardSource = this.CardSource;
	}
}
