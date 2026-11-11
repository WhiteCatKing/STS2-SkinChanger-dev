using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznCore.Presentation;

internal readonly record struct DefenseBlockDamaged(Creature Target, int Before, int After);
