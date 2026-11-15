using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace STS2SkinChanger.Core.GameApi;

internal static class CardApi
{
    internal static readonly MethodInfo ReloadCardMethod =
        AccessTools.Method(typeof(NCard), "Reload");
    internal static readonly MethodInfo UpdateLibraryFilterMethod =
        AccessTools.Method(typeof(NCardLibrary), "UpdateFilter", [typeof(bool)]);
    internal static readonly FieldInfo? BuiltInOverlayField =
        AccessTools.Field(typeof(NCard), "_cardOverlay");
    internal static readonly FieldInfo? HighlightShaderMaterialField =
        AccessTools.Field(typeof(NCardHighlight), "_shaderMaterial");
    internal static readonly MethodInfo LibraryFilterCardsMethod =
        AccessTools.Method(
            typeof(NCardLibraryGrid),
            nameof(NCardLibraryGrid.FilterCards),
            [typeof(Func<CardModel, bool>), typeof(List<SortingOrders>)]);
    internal static readonly MethodInfo ColumnsGetter =
        AccessTools.PropertyGetter(typeof(NCardGrid), "Columns");
    internal static readonly MethodInfo CalculateRowsNeededMethod =
        AccessTools.Method(typeof(NCardGrid), "CalculateRowsNeeded");
    internal static readonly MethodInfo RewardRefreshOptionsMethod =
        AccessTools.Method(
            typeof(NCardRewardSelectionScreen),
            nameof(NCardRewardSelectionScreen.RefreshOptions));
    internal static readonly MethodInfo FreedToPoolMethod =
        AccessTools.Method(typeof(NCard), "OnFreedToPool");
    internal static readonly MethodInfo ReturnedFromPoolMethod =
        AccessTools.Method(typeof(NCard), "OnReturnedFromPool");
    internal static readonly MethodInfo UpdateVisualsMethod =
        AccessTools.Method(typeof(NCard), nameof(NCard.UpdateVisuals));
    internal static readonly FieldInfo? TinyCardBannerField =
        AccessTools.Field(typeof(NTinyCard), "_cardBanner");
}
