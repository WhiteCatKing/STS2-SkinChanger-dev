using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Ui;

internal sealed record CreatureAppearanceBinding(SkinGroup Group, string TransformKey, bool UsesMonsterScale, bool CanSelectSkin, bool SupportsIntent, bool SupportsCombatControls);
