namespace STS2SkinChanger.Core;

internal static class MerchantFigureScopePolicy
{
	internal enum FigureAction
	{
		Leave,
		ShowGameFigure,
		HideGameFigure
	}

	internal static FigureAction ResolveFigure(bool partSelected, bool packageFigureVisible, bool gameFigureVisibleAtRest, bool gameFigureVisible)
	{
		if (partSelected)
		{
			if (!(packageFigureVisible && gameFigureVisible))
			{
				return FigureAction.Leave;
			}
			return FigureAction.HideGameFigure;
		}
		if (!gameFigureVisibleAtRest || gameFigureVisible)
		{
			return FigureAction.Leave;
		}
		return FigureAction.ShowGameFigure;
	}

	internal static bool ShouldRestoreLayout(bool partSelected, bool layoutDiffersFromRest)
	{
		return !partSelected && layoutDiffersFromRest;
	}

	internal static bool ShouldRevealGameFigure(bool selectedPartDrawsFigure)
	{
		return !selectedPartDrawsFigure;
	}
}
