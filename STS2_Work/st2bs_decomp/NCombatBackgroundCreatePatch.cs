using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using St2bs.St2bsCode.Debug;
using St2bs.St2bsCode.Monsters;

namespace St2bs.St2bsCode.Patches;

[HarmonyPatch(typeof(NCombatBackground), "Create")]
public static class NCombatBackgroundCreatePatch
{
	private const string GirlsNightmareScene = "res://St2bs/scenes/GirlsNightmare.tscn";

	private const string GirlsScene = "res://St2bs/scenes/girls.tscn";

	private static readonly string[] CustomPaths = new string[18]
	{
		"res://St2bs/scenes/WINTER_BELL.tscn", "res://St2bs/scenes/backgrounds/gulan/gulan_bg.tscn", "res://St2bs/scenes/KURANINDE.tscn", "res://St2bs/scenes/CRIMEA.tscn", "res://St2bs/scenes/Psychiatric.tscn", "res://St2bs/scenes/blood.tscn", "res://St2bs/scenes/xians.tscn", "res://St2bs/scenes/redcity.tscn", "res://St2bs/scenes/redcity2.tscn", "res://St2bs/scenes/SmokeForest.tscn",
		"res://St2bs/scenes/OxfordAcademy.tscn", "res://St2bs/scenes/OxfordAcademy2.tscn", "res://St2bs/scenes/ClockTower.tscn", "res://St2bs/scenes/ClockTower2.tscn", "res://St2bs/scenes/Carol.tscn", "res://St2bs/scenes/Carol2.tscn", "res://St2bs/scenes/GirlsNightmare.tscn", "res://St2bs/scenes/girls.tscn"
	};

	private static string? GetScenePathForCurrentAct()
	{
		RunManager instance = RunManager.Instance;
		if (instance == null)
		{
			return null;
		}
		RunState value = Traverse.Create((object)instance).Property("State", (object[])null).GetValue<RunState>();
		if (((value != null) ? value.Acts : null) == null)
		{
			return null;
		}
		if (value.CurrentActIndex < 0 || value.CurrentActIndex >= value.Acts.Count)
		{
			return null;
		}
		ActModel obj = value.Acts[value.CurrentActIndex];
		string text = ((obj != null) ? ((AbstractModel)obj).Id.Entry : null);
		if (1 == 0)
		{
		}
		string result = text switch
		{
			"WINTER_BELL" => "res://St2bs/scenes/WINTER_BELL.tscn", 
			"DARK_STAGE" => "res://St2bs/scenes/backgrounds/gulan/gulan_bg.tscn", 
			"CRIMEA_CARETAKER_CEMETERY" => "res://St2bs/scenes/CRIMEA.tscn", 
			"KURANINDE" => "res://St2bs/scenes/KURANINDE.tscn", 
			"RED_CITY_FRIEDELL" => "res://St2bs/scenes/redcity.tscn", 
			"SMOKE_FOREST" => "res://St2bs/scenes/SmokeForest.tscn", 
			"OXFORD_ACADEMY" => "res://St2bs/scenes/OxfordAcademy.tscn", 
			"CLOCK_TOWER" => "res://St2bs/scenes/ClockTower.tscn", 
			"PSYCHIATRIC_WARD" => "res://St2bs/scenes/Psychiatric.tscn", 
			"CARROLL_RIVER" => "res://St2bs/scenes/Carol.tscn", 
			"GIRLS_NIGHTMARE" => "res://St2bs/scenes/GirlsNightmare.tscn", 
			_ => null, 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	[HarmonyPrefix]
	public static bool BeforeCreate(BackgroundAssets bg, ref NCombatBackground __result)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		string text = bg.BackgroundScenePath;
		GulanPositionHelper.ClearSource();
		if (text == null || !CustomPaths.Contains(text))
		{
			string scenePathForCurrentAct = GetScenePathForCurrentAct();
			if (scenePathForCurrentAct == null)
			{
				return true;
			}
			St2bsLogger.Info("NCombatBackgroundCreatePatch: fallback to " + scenePathForCurrentAct, "BeforeCreate", 80);
			text = scenePathForCurrentAct;
		}
		PackedScene val = ResourceLoader.Load<PackedScene>(text, (string)null, (CacheMode)1);
		if (val == null)
		{
			return true;
		}
		Control val2;
		try
		{
			val2 = val.Instantiate<Control>((GenEditState)0);
		}
		catch
		{
			return true;
		}
		NCombatBackground val3 = new NCombatBackground();
		((Node)val3).Name = ((Node)val2).Name;
		while (((Node)val2).GetChildCount(false) > 0)
		{
			Node child = ((Node)val2).GetChild(0, false);
			((Node)val2).RemoveChild(child);
			((Node)val3).AddChild(child, false, (InternalMode)0);
		}
		((Node)val2).QueueFree();
		if (text == CustomPaths[1] || text == "res://St2bs/scenes/GirlsNightmare.tscn" || text == "res://St2bs/scenes/girls.tscn")
		{
			GulanPositionHelper.SetSource(text);
			GulanPositionHelper.CacheBackground((Control)(object)val3);
			for (int i = 1; i <= 3; i++)
			{
				Control nodeOrNull = ((Node)val3).GetNodeOrNull<Control>(NodePath.op_Implicit(i.ToString()));
				if (nodeOrNull != null)
				{
					GulanPositionHelper.CachePosition(i, nodeOrNull.Position);
				}
			}
		}
		if (text == "res://St2bs/scenes/girls.tscn")
		{
			Camera2D nodeOrNull2 = ((Node)val3).GetNodeOrNull<Camera2D>(NodePath.op_Implicit("girls1"));
			if (nodeOrNull2 != null)
			{
				nodeOrNull2.Enabled = false;
			}
		}
		__result = val3;
		return false;
	}

	[HarmonyPostfix]
	public static void AfterCreate(BackgroundAssets bg, NCombatBackground __result)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		RunManager instance = RunManager.Instance;
		if (instance == null)
		{
			return;
		}
		RunState value = Traverse.Create((object)instance).Property("State", (object[])null).GetValue<RunState>();
		if (((value != null) ? value.Acts : null) != null && value.CurrentActIndex >= 0 && value.CurrentActIndex < value.Acts.Count)
		{
			ActModel obj = value.Acts[value.CurrentActIndex];
			if (!(((obj != null) ? ((AbstractModel)obj).Id.Entry : null) != "DEEP_SEA"))
			{
				ColorRect val = new ColorRect();
				val.Color = new Color(0f, 0f, 0.25f, 0.35f);
				((Control)val).Size = new Vector2(5000f, 5000f);
				((Control)val).MouseFilter = (MouseFilterEnum)2;
				GodotTreeExtensions.AddChildSafely((Node)(object)NCombatRoom.Instance.CombatVfxContainer, (Node)(object)val);
			}
		}
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.0.0.7889')
