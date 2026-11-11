using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal interface ICombatCutInToggleHudHost
{
	bool IsTargetPlayer(Player? player);

	bool IsAvailable();

	bool ShouldShow(NCombatUi ui)
	{
		if (IsAvailable() && GodotNodeUtil.IsAlive((GodotObject?)(object)ui) && ((CanvasItem)ui).Visible && GodotNodeUtil.IsAlive((GodotObject?)(object)ui.Hand))
		{
			return !ui.Hand.IsInCardSelection;
		}
		return false;
	}

	bool IsCutInEnabled();

	void SetCutInEnabled(bool enabled, bool persist);

	string RuntimeNodeName(string suffix);

	void Warn(string message, int priority);

	void Debug(string message, int priority)
	{
	}
}
