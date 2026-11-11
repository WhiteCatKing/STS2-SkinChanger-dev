using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using PengoTarot.Data;

namespace PengoTarot.Patches;

[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
public static class NCardLibrary_TarotFilter_Patch
{
	private static readonly FieldInfo _lastHoveredControlField = typeof(NCardLibrary).GetField("_lastHoveredControl", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly MethodInfo _updateCardPoolFilterMethod = typeof(NCardLibrary).GetMethod("UpdateCardPoolFilter", BindingFlags.Instance | BindingFlags.NonPublic);

	private static void Postfix(NCardLibrary __instance)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Expected O, but got Unknown
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Expected O, but got Unknown
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		FieldInfo field = typeof(NCardLibrary).GetField("_poolFilters", BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null || !(field.GetValue(__instance) is Dictionary<NCardPoolFilter, Func<CardModel, bool>> dictionary))
		{
			return;
		}
		NCardPoolFilter node = ((Node)__instance).GetNode<NCardPoolFilter>(NodePath.op_Implicit("%AncientsPool"));
		if (node != null && dictionary.ContainsKey(node))
		{
			dictionary[node] = (CardModel c) => (int)c.Rarity == 5 && !(c.Pool is TarotPool) && !(c.Pool is PlanetPool);
		}
		NCardPoolFilter val = node;
		if (val == null)
		{
			return;
		}
		Node parent = ((Node)val).GetParent();
		NCardPoolFilter tarotFilter = (NCardPoolFilter)((Node)val).Duplicate(15);
		((Node)tarotFilter).Name = StringName.op_Implicit("TarotPool");
		tarotFilter.Loc = new LocString("card_library", "POOL_TAROT_TIP");
		TextureRect node2 = ((Node)tarotFilter).GetNode<TextureRect>(NodePath.op_Implicit("Image"));
		if (node2 != null)
		{
			((CanvasItem)node2).Material = (Material)((Resource)((CanvasItem)node2).Material).Duplicate(false);
			Texture2D val2 = ResourceLoader.Load<Texture2D>("res://images/ui/filter_icons/tarot_filter_icon.png", (string)null, (CacheMode)1);
			if (val2 != null)
			{
				node2.Texture = val2;
			}
		}
		parent.AddChild((Node)(object)tarotFilter, false, (InternalMode)0);
		dictionary[tarotFilter] = (CardModel c) => c.Pool is TarotPool && !((object)c).GetType().Name.EndsWith("Sub");
		Callable val3 = Callable.From<NCardPoolFilter>((Action<NCardPoolFilter>)Delegate.CreateDelegate(typeof(Action<NCardPoolFilter>), __instance, _updateCardPoolFilterMethod));
		((GodotObject)tarotFilter).Connect(SignalName.Toggled, val3, 0u);
		((GodotObject)tarotFilter).Connect(SignalName.FocusEntered, Callable.From((Action)delegate
		{
			_lastHoveredControlField.SetValue(__instance, tarotFilter);
		}), 0u);
		NCardPoolFilter planetFilter = (NCardPoolFilter)((Node)val).Duplicate(15);
		((Node)planetFilter).Name = StringName.op_Implicit("PlanetPool");
		planetFilter.Loc = new LocString("card_library", "POOL_PLANET_TIP");
		TextureRect node3 = ((Node)planetFilter).GetNode<TextureRect>(NodePath.op_Implicit("Image"));
		if (node3 != null)
		{
			((CanvasItem)node3).Material = (Material)((Resource)((CanvasItem)node3).Material).Duplicate(false);
			Texture2D val4 = ResourceLoader.Load<Texture2D>("res://images/enchantments/planet_earth_enchantment.png", (string)null, (CacheMode)1);
			if (val4 != null)
			{
				node3.Texture = val4;
			}
		}
		parent.AddChild((Node)(object)planetFilter, false, (InternalMode)0);
		dictionary[planetFilter] = (CardModel c) => c.Pool is PlanetPool && !((object)c).GetType().Name.EndsWith("Sub");
		Callable val5 = Callable.From<NCardPoolFilter>((Action<NCardPoolFilter>)Delegate.CreateDelegate(typeof(Action<NCardPoolFilter>), __instance, _updateCardPoolFilterMethod));
		((GodotObject)planetFilter).Connect(SignalName.Toggled, val5, 0u);
		((GodotObject)planetFilter).Connect(SignalName.FocusEntered, Callable.From((Action)delegate
		{
			_lastHoveredControlField.SetValue(__instance, planetFilter);
		}), 0u);
	}
}
