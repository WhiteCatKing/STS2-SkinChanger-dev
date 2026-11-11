using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using PengoTarot.Cards;

namespace PengoTarot.Patch.VisualVanilla;

[HarmonyPatch(typeof(NCardLibrary))]
public static class CardLibraryVanillaPatch
{
	private static readonly HashSet<NCardLibrary> _handlerAdded = new HashSet<NCardLibrary>();

	internal static bool CurrentIsTarot;

	internal static bool CurrentIsPlanet;

	[HarmonyPatch(typeof(NCardLibraryGrid), "FilterCards", new Type[]
	{
		typeof(Func<CardModel, bool>),
		typeof(List<SortingOrders>)
	})]
	[HarmonyPostfix]
	private static void GridFilterCards_Postfix(NCardLibraryGrid __instance)
	{
		for (Node parent = ((Node)__instance).GetParent(); parent != null; parent = parent.GetParent())
		{
			NCardLibrary val = (NCardLibrary)(object)((parent is NCardLibrary) ? parent : null);
			if (val != null)
			{
				UpdateToggleState(val);
				break;
			}
		}
	}

	internal static void UpdateToggleState(NCardLibrary library)
	{
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		NTickbox nodeOrNull = ((Node)library).GetNodeOrNull<NTickbox>(NodePath.op_Implicit("%Upgrades"));
		NTickbox nodeOrNull2 = ((Node)library).GetNodeOrNull<NTickbox>(NodePath.op_Implicit("%Stats"));
		if (nodeOrNull == null)
		{
			return;
		}
		object value = Traverse.Create((object)library).Field("_grid").GetValue();
		List<CardModel> list = ((value != null) ? (Traverse.Create(value).Property("VisibleCards", (object[])null).GetValue() as IEnumerable<CardModel>) : null)?.ToList();
		bool flag = list != null && list.Count > 0 && list.All((CardModel c) => c is TarCard);
		bool flag2 = list != null && list.Count > 0 && list.All((CardModel c) => c is PlanetCard);
		bool num = flag || flag2;
		CurrentIsTarot = flag;
		CurrentIsPlanet = flag2;
		if (!_handlerAdded.Contains(library))
		{
			_handlerAdded.Add(library);
			((GodotObject)nodeOrNull).Connect(StringName.op_Implicit("Toggled"), Callable.From<NTickbox>((Action<NTickbox>)delegate(NTickbox nt)
			{
				if (CurrentIsTarot || CurrentIsPlanet)
				{
					bool isTicked = nt.IsTicked;
					if (CurrentIsTarot)
					{
						VanillaStyleConfig.TarotVanilla = isTicked;
					}
					else
					{
						VanillaStyleConfig.PlanetVanilla = isTicked;
					}
					ResetIsShowingUpgrades(library);
					AnimateRefresh(library);
				}
			}), 0u);
		}
		if (num)
		{
			((CanvasItem)nodeOrNull).Visible = true;
			((Control)nodeOrNull).MouseFilter = (MouseFilterEnum)0;
			bool isTicked2 = (flag ? VanillaStyleConfig.TarotVanilla : VanillaStyleConfig.PlanetVanilla);
			((GodotObject)nodeOrNull).SetBlockSignals(true);
			nodeOrNull.IsTicked = isTicked2;
			((GodotObject)nodeOrNull).SetBlockSignals(false);
			ResetIsShowingUpgrades(library);
			Node obj = ((Node)nodeOrNull).FindChild("Label", true, false);
			Label val = (Label)(object)((obj is Label) ? obj : null);
			if (val != null)
			{
				val.Text = (flag ? new LocString("gameplay_ui", "VANILLA_STYLE_TAROT").GetFormattedText() : new LocString("gameplay_ui", "VANILLA_STYLE_PLANET").GetFormattedText());
			}
			if (nodeOrNull2 != null)
			{
				((CanvasItem)nodeOrNull2).Visible = false;
				((Control)nodeOrNull2).MouseFilter = (MouseFilterEnum)2;
			}
		}
		else
		{
			Node obj2 = ((Node)nodeOrNull).FindChild("Label", true, false);
			Label val2 = (Label)(object)((obj2 is Label) ? obj2 : null);
			if (val2 != null)
			{
				val2.Text = new LocString("card_library", "VIEW_UPGRADES").GetRawText();
			}
			if (nodeOrNull2 != null)
			{
				((CanvasItem)nodeOrNull2).Visible = true;
				((Control)nodeOrNull2).MouseFilter = (MouseFilterEnum)0;
			}
		}
	}

	private static void ResetIsShowingUpgrades(NCardLibrary library)
	{
		object value = Traverse.Create((object)library).Field("_grid").GetValue();
		if (value != null)
		{
			Traverse.Create(value).Property("IsShowingUpgrades", (object[])null).SetValue((object)false);
		}
	}

	private static async void AnimateRefresh(NCardLibrary library)
	{
		object value = Traverse.Create((object)library).Field("_grid").GetValue();
		if (value != null)
		{
			MethodInfo method = value.GetType().GetMethod("AnimateOut", Type.EmptyTypes);
			if (method != null && method.Invoke(value, null) is Task task)
			{
				await task;
			}
			MethodInfo method2 = typeof(NCardLibrary).GetMethod("DisplayCards", BindingFlags.Instance | BindingFlags.NonPublic);
			if (method2 != null && method2.Invoke(library, null) is Task task2)
			{
				await task2;
			}
		}
	}

	internal static void SyncLibraryToggle()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			Node obj = ((Node)val.Root).FindChild("CardLibrary", true, false);
			NCardLibrary val2 = (NCardLibrary)(object)((obj is NCardLibrary) ? obj : null);
			if (val2 != null)
			{
				UpdateToggleState(val2);
			}
		}
	}
}
