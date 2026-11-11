using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using MegaCrit.Sts2.Core.Entities.Ancients;

namespace CznCore.Characters;

internal readonly record struct ArchitectDialogueDefinition(int LineCount, int? VisitIndex, bool IsRepeating = true, ArchitectAttackers StartAttackers = 0, ArchitectAttackers EndAttackers = 3)
{
	internal AncientDialogue Create()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		AncientDialogue val = new AncientDialogue(Enumerable.Repeat(string.Empty, LineCount).ToArray());
		val.set_VisitIndex(VisitIndex);
		val.IsRepeating = IsRepeating;
		val.set_StartAttackers(StartAttackers);
		val.set_EndAttackers(EndAttackers);
		return val;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		builder.Append("LineCount = ");
		builder.Append(LineCount.ToString());
		builder.Append(", VisitIndex = ");
		builder.Append(VisitIndex.ToString());
		builder.Append(", IsRepeating = ");
		builder.Append(IsRepeating.ToString());
		builder.Append(", StartAttackers = ");
		ArchitectAttackers val = StartAttackers;
		builder.Append(((object)(ArchitectAttackers)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", EndAttackers = ");
		val = EndAttackers;
		builder.Append(((object)(ArchitectAttackers)(ref val)/*cast due to .constrained prefix*/).ToString());
		return true;
	}

	[CompilerGenerated]
	public void Deconstruct(out int LineCount, out int? VisitIndex, out bool IsRepeating, out ArchitectAttackers StartAttackers, out ArchitectAttackers EndAttackers)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected I4, but got Unknown
		LineCount = this.LineCount;
		VisitIndex = this.VisitIndex;
		IsRepeating = this.IsRepeating;
		StartAttackers = (ArchitectAttackers)(int)this.StartAttackers;
		EndAttackers = (ArchitectAttackers)(int)this.EndAttackers;
	}
}
