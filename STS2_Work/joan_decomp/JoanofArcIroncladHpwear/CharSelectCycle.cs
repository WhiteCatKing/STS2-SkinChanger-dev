using System;
using System.Reflection;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace JoanofArcIroncladHpwear;

internal static class CharSelectCycle
{
	private const string CatcherName = "JoanofArcIroncladHpwearClickCatcher";

	private const string PollerName = "JoanofArcIroncladHpwearInputPoller";

	private const string IdleAnimation = "animation";

	private static FieldInfo? _bgContainerField;

	private static int _stage = 1;

	private static readonly Resource?[] StageResources = (Resource?[])(object)new Resource[4];

	private static readonly bool[] Loading = new bool[4];

	internal static int CurrentStage
	{
		get
		{
			if (!WearSettingsRuntime.Current.WearEnabled)
			{
				return 1;
			}
			return _stage;
		}
	}

	internal static void Attach(Control bg)
	{
		try
		{
			InstallCatcher(bg);
			if (!SkinSelectionGate.MenuAllowed())
			{
				return;
			}
			Node val = SpineSwap.FindSpine((Node?)(object)bg);
			if (val == null)
			{
				ModEntry.Log.Info("char-select: no SpineSprite found — cycling off.", 1);
				return;
			}
			if (!SpineSwap.IsOurs(val))
			{
				ModEntry.Log.Info("char-select: live skeleton is not this mod's (" + (SpineSwap.GetSkeletonPath(val) ?? "none") + ") — cycling off.", 1);
				return;
			}
			BeginPreload();
			if (!WearSettingsRuntime.Current.WearEnabled)
			{
				_stage = 1;
			}
			Apply(bg, val, _stage, replayIdle: true);
			ModEntry.Log.Info($"char-select: cycling armed (stage {_stage}/3).", 1);
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select attach failed: {value}", 1);
		}
	}

	private static bool Apply(Control bg, Node spine, int stage, bool replayIdle)
	{
		Resource stageData = GetStageData(stage);
		if (stageData == null)
		{
			return false;
		}
		Resource skeletonData = SpineSwap.GetSkeletonData(spine);
		if (!SpineSwap.SetSkeletonData(spine, stageData))
		{
			ModEntry.Log.Error($"char-select: stage {stage} did not apply.", 1);
			if (skeletonData != null)
			{
				SpineSwap.SetSkeletonData(spine, skeletonData);
			}
			return false;
		}
		if (replayIdle)
		{
			SpineSwap.Play(spine, "animation", loop: true);
		}
		return true;
	}

	private static void InstallCatcher(Control bg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (!((Node)bg).HasNode(new NodePath("JoanofArcIroncladHpwearClickCatcher")))
		{
			Control val = new Control
			{
				Name = StringName.op_Implicit("JoanofArcIroncladHpwearClickCatcher"),
				MouseFilter = (MouseFilterEnum)2
			};
			val.SetAnchorsPreset((LayoutPreset)15, false);
			val.GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
			{
				OnInput(bg, input);
			};
			((Node)bg).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)bg).MoveChild((Node)(object)val, ((Node)bg).GetChildCount(false) - 1);
			InstallPoller(bg);
		}
	}

	private static void InstallPoller(Control bg)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		if (((Node)bg).HasNode(new NodePath("JoanofArcIroncladHpwearInputPoller")))
		{
			return;
		}
		Timer poller = new Timer
		{
			Name = StringName.op_Implicit("JoanofArcIroncladHpwearInputPoller"),
			WaitTime = 0.03,
			OneShot = false,
			Autostart = true,
			ProcessCallback = (TimerProcessCallback)1,
			ProcessMode = (ProcessModeEnum)0
		};
		((Node)bg).AddChild((Node)(object)poller, false, (InternalMode)0);
		((Node)bg).TreeExiting += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)poller))
			{
				poller.Stop();
				((Node)poller).QueueFree();
			}
			Array.Clear(StageResources, 0, StageResources.Length);
		};
		poller.Timeout += delegate
		{
			Poll(bg);
		};
		poller.Start(-1.0);
		ModEntry.Log.Info("char-select: click catcher + poller installed.", 1);
	}

	private static void BeginPreload()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I8
		for (int i = 1; i <= 3; i++)
		{
			if (StageResources[i] == null && !Loading[i])
			{
				Error val = ResourceLoader.LoadThreadedRequest(SkinCatalog.DataPath("charselect", i), "", false, (CacheMode)1);
				Loading[i] = (long)val == 0;
			}
		}
	}

	private static Resource? GetStageData(int stage)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I8
		if (stage < 1 || stage > 3)
		{
			return null;
		}
		if (StageResources[stage] != null && GodotObject.IsInstanceValid((GodotObject)(object)StageResources[stage]))
		{
			return StageResources[stage];
		}
		string text = SkinCatalog.DataPath("charselect", stage);
		if (Loading[stage] && (long)ResourceLoader.LoadThreadedGetStatus(text, (Array)null) == 3)
		{
			Loading[stage] = false;
			StageResources[stage] = ResourceLoader.LoadThreadedGet(text);
		}
		if (StageResources[stage] == null)
		{
			StageResources[stage] = SpineSwap.LoadStageData("charselect", stage);
		}
		return StageResources[stage];
	}

	private static void Poll(Control bg)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I8
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)bg))
			{
				return;
			}
			for (int i = 1; i <= 3; i++)
			{
				if (Loading[i] && (long)ResourceLoader.LoadThreadedGetStatus(SkinCatalog.DataPath("charselect", i), (Array)null) == 3)
				{
					StageResources[i] = ResourceLoader.LoadThreadedGet(SkinCatalog.DataPath("charselect", i));
					Loading[i] = false;
				}
			}
			if (!((CanvasItem)bg).IsVisibleInTree())
			{
				return;
			}
			Control nodeOrNull = ((Node)bg).GetNodeOrNull<Control>(new NodePath("JoanofArcIroncladHpwearClickCatcher"));
			Node val = SpineSwap.FindSpine((Node?)(object)bg);
			bool flag = SkinSelectionGate.MenuAllowed() && val != null && SpineSwap.IsOurs(val);
			if (nodeOrNull != null)
			{
				nodeOrNull.MouseFilter = (MouseFilterEnum)(flag ? 0 : 2);
			}
			if (flag)
			{
				BeginPreload();
				WearSettingsRuntime.TickHotkey();
				if (!WearSettingsRuntime.Current.WearEnabled && val != null && (_stage != 1 || SkinCatalog.StageFromPath("charselect", SpineSwap.GetSkeletonPath(val)) != 1) && Apply(bg, val, 1, replayIdle: true))
				{
					_stage = 1;
				}
			}
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select poll failed: {value}", 1);
		}
	}

	private static void OnInput(Control bg, InputEvent input)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I8
		InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
		if (val == null || (long)val.ButtonIndex != 1 || !val.Pressed)
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)bg) || !((CanvasItem)bg).IsVisibleInTree() || !SkinSelectionGate.MenuAllowed() || !WearSettingsRuntime.Current.WearEnabled)
			{
				return;
			}
			Node val2 = SpineSwap.FindSpine((Node?)(object)bg);
			if (val2 != null && SpineSwap.IsOurs(val2))
			{
				int num = HpWearRules.NextStage(_stage, 3);
				if (Apply(bg, val2, num, replayIdle: true))
				{
					int stage = _stage;
					_stage = num;
					PortraitSync.RefreshMenuNow((Node)(object)bg);
					ModEntry.Log.Info($"char-select: stage {stage}->{num}/3.", 1);
				}
			}
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select click failed: {value}", 1);
		}
	}

	internal static void OnCharacterSelected(NCharacterSelectScreen screen, CharacterModel? characterModel)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		try
		{
			string text = ((characterModel != null) ? ((AbstractModel)characterModel).Id.Entry : null);
			if (string.Equals(text, "ironclad", StringComparison.OrdinalIgnoreCase))
			{
				if ((object)_bgContainerField == null)
				{
					_bgContainerField = AccessTools.Field(typeof(NCharacterSelectScreen), "_bgContainer");
				}
				object? obj = _bgContainerField?.GetValue(screen);
				object? obj2 = ((obj is Control) ? obj : null);
				Control val = ((obj2 != null) ? ((Node)obj2).GetNodeOrNull<Control>(new NodePath(text + "_bg")) : null);
				if (val == null)
				{
					ModEntry.Log.Info("char-select: ironclad_bg not found — cycling off.", 1);
				}
				else
				{
					Attach(val);
				}
			}
		}
		catch (Exception value)
		{
			ModEntry.Log.Error($"char-select hook failed: {value}", 1);
		}
	}
}
