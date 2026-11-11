using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Data;

public sealed class TarotConsoleCmd : AbstractConsoleCmd
{
	public override string CmdName => "tarot";

	public override string Args => "<tarot-id:string>";

	public override string Description => "Executes a Tarot acquisition, or adds it to the portable Tarot slots while Judgement is enabled.";

	public override bool IsNetworked => true;

	public override CmdResult Process(Player? issuingPlayer, string[] args)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (!RunManager.Instance.IsInProgress || issuingPlayer == null)
		{
			return new CmdResult(false, "A run is currently not in progress!");
		}
		if (args.Length == 0)
		{
			return new CmdResult(false, "No Tarot name specified. Use a name such as FOOL_UPRIGHT.");
		}
		string text = string.Join(" ", args);
		TarotDef tarotDef = FindTarot(text);
		if (tarotDef == null)
		{
			return new CmdResult(false, "Tarot '" + text + "' not found.");
		}
		if (JudgementDivination.IsEnabled)
		{
			if (!JudgementTarotInventoryState.TryProcure(issuingPlayer.NetId, tarotDef.Id))
			{
				return new CmdResult(false, "Could not add Tarot '" + tarotDef.Id + "': the portable Tarot slots are full.");
			}
			return new CmdResult(true, "Added Tarot '" + tarotDef.Id + "' to the portable Tarot slots.");
		}
		return new CmdResult((Task)TarotEffectExecutor.ExecuteEffectAndEnchant(tarotDef, issuingPlayer, LocalContext.IsMe(issuingPlayer)), true, "Executing Tarot '" + tarotDef.Id + "'.");
	}

	public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		if (args.Length <= 1)
		{
			return ((AbstractConsoleCmd)this).CompleteArgument(TarotDeck.All.Select((TarotDef def) => def.Id), Array.Empty<string>(), args.FirstOrDefault() ?? string.Empty, (CompletionType)2, (Func<string, string, bool>)null);
		}
		return new CompletionResult
		{
			Type = (CompletionType)2,
			ArgumentContext = ((AbstractConsoleCmd)this).CmdName
		};
	}

	private static TarotDef? FindTarot(string input)
	{
		string normalized = Normalize(input);
		return TarotDeck.All.FirstOrDefault((TarotDef def) => Normalize(def.Id) == normalized || Normalize(def.CardType.Name) == normalized || TryMatchCardModel(def, normalized));
	}

	private static bool TryMatchCardModel(TarotDef def, string normalized)
	{
		CardModel val = ModelDb.AllCards.FirstOrDefault((Func<CardModel, bool>)((CardModel candidate) => ((object)candidate).GetType() == def.CardType));
		if (val != null)
		{
			if (!(Normalize(((AbstractModel)val).Id.Entry) == normalized))
			{
				return Normalize(val.Title) == normalized;
			}
			return true;
		}
		return false;
	}

	private static string Normalize(string value)
	{
		string text = new string(value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());
		if (!text.StartsWith("TAR", StringComparison.Ordinal))
		{
			return text;
		}
		string text2 = text;
		return text2.Substring(3, text2.Length - 3);
	}
}
