using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal sealed record BackgroundOption(string Id, string DisplayName, string Source, string Title, BackgroundPackShell? Pack);
