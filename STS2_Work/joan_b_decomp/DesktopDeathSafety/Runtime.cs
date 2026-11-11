using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Godot;

namespace DesktopDeathSafety;

public static class Runtime
{
	private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly Assembly Host = typeof(Runtime).Assembly;

	private static readonly Dictionary<ulong, WeakReference<Node>> Nodes = new Dictionary<ulong, WeakReference<Node>>();

	private static readonly List<ulong> Scratch = new List<ulong>();

	private const string FadeMarker = "desktop_death_fade_overlay";

	private static readonly Dictionary<ulong, Resource> LastLook = new Dictionary<ulong, Resource>();

	private static readonly Dictionary<ulong, (Resource, ulong)> RecentLook = new Dictionary<ulong, (Resource, ulong)>();

	private static readonly Dictionary<ulong, Node2D> Overlays = new Dictionary<ulong, Node2D>();

	private static bool Started;

	private static int Errors;

	public static void Bootstrap()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)Start);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	public static void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		if (!Started)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				Started = true;
				val.NodeAdded += new NodeAddedEventHandler(Track);
				val.NodeRemoved += new NodeRemovedEventHandler(Untrack);
				Discover((Node)(object)val.Root);
				Timer val2 = new Timer
				{
					Name = StringName.op_Implicit("DesktopDeathSafety_" + Host.GetName().Name),
					WaitTime = 0.03,
					Autostart = true,
					ProcessMode = (ProcessModeEnum)3
				};
				val2.Timeout += Tick;
				((Node)val.Root).AddChild((Node)(object)val2, false, (InternalMode)0);
			}
		}
	}

	private static void Discover(Node node)
	{
		Track(node);
		foreach (Node child in node.GetChildren(false))
		{
			Discover(child);
		}
	}

	public static void Track(Node node)
	{
		if (((GodotObject)node).GetClass() == "SpineSprite" && !((GodotObject)node).HasMeta(StringName.op_Implicit("desktop_death_fade_overlay")))
		{
			Nodes[((GodotObject)node).GetInstanceId()] = new WeakReference<Node>(node);
		}
	}

	private static void Untrack(Node node)
	{
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		Nodes.Remove(instanceId);
		LastLook.Remove(instanceId);
		RecentLook.Remove(instanceId);
		if (Overlays.Remove(instanceId, out var value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			((Node)value).QueueFree();
		}
	}

	private static object Call(string type, string method, [Optional] object[] arguments)
	{
		return Host.GetType(type)?.GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, arguments);
	}

	private static object Member(object obj, string name)
	{
		object obj2 = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(obj);
		if (obj2 == null)
		{
			FieldInfo? field = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if ((object)field == null)
			{
				return null;
			}
			obj2 = field.GetValue(obj);
		}
		return obj2;
	}

	private static bool Allowed()
	{
		object obj = Call("JoanofArcIroncladHpwear.SkinSelectionGate", "SceneAllowed");
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}

	public static void Tick()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!Allowed())
			{
				return;
			}
			Call("JoanofArcIroncladHpwear.WearSettingsRuntime", "TickHotkey");
			object obj = Member(Host.GetType("JoanofArcIroncladHpwear.WearSettingsRuntime").GetProperty("Current", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetValue(null), "WearEnabled");
			bool enabled = obj is bool && (bool)obj;
			Scratch.Clear();
			Scratch.AddRange(Nodes.Keys);
			foreach (ulong item in Scratch)
			{
				if (!Nodes.TryGetValue(item, out var value) || !value.TryGetTarget(out var target) || !GodotObject.IsInstanceValid((GodotObject)(object)target))
				{
					Nodes.Remove(item);
					LastLook.Remove(item);
					RecentLook.Remove(item);
					Overlays.Remove(item);
				}
				else
				{
					if (!target.IsInsideTree())
					{
						continue;
					}
					Variant val = ((GodotObject)target).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
					GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
					object obj2;
					if (val2 == null)
					{
						obj2 = null;
					}
					else
					{
						val = val2.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
						obj2 = ((Variant)(ref val)).AsGodotObject();
					}
					GodotObject val3 = (GodotObject)obj2;
					object obj3;
					if (val3 == null)
					{
						obj3 = null;
					}
					else
					{
						val = val3.Call(StringName.op_Implicit("get_animation"), Array.Empty<Variant>());
						GodotObject obj4 = ((Variant)(ref val)).AsGodotObject();
						if (obj4 == null)
						{
							obj3 = null;
						}
						else
						{
							val = obj4.Call(StringName.op_Implicit("get_name"), Array.Empty<Variant>());
							obj3 = ((Variant)(ref val)).AsString();
						}
					}
					if (obj3 == null)
					{
						obj3 = "";
					}
					string text = (string)obj3;
					bool dead = text.Equals("die", StringComparison.OrdinalIgnoreCase) || text.Equals("dead", StringComparison.OrdinalIgnoreCase) || text.Equals("death", StringComparison.OrdinalIgnoreCase);
					val = ((GodotObject)target).Get(StringName.op_Implicit("skeleton_data_res"));
					GodotObject obj5 = ((Variant)(ref val)).AsGodotObject();
					Resource val4 = (Resource)(object)((obj5 is Resource) ? obj5 : null);
					if (val4 != null)
					{
						ObserveLook(item, target, val4, dead, enabled, text, val3, val2);
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (Errors++ < 3)
			{
				GD.PushWarning("DesktopDeathSafety " + Host.GetName().Name + ": " + ex.GetBaseException().Message);
			}
		}
	}

	private static void ObserveLook(ulong id, Node spine, Resource current, bool dead, bool enabled, string animation, GodotObject track, GodotObject state)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		ulong ticksMsec = Time.GetTicksMsec();
		if (LastLook.TryGetValue(id, out var value) && value != current)
		{
			RecentLook[id] = (value, ticksMsec);
		}
		LastLook[id] = current;
		if (!RecentLook.TryGetValue(id, out var value2))
		{
			return;
		}
		if (ticksMsec - value2.Item2 > 150)
		{
			RecentLook.Remove(id);
		}
		else
		{
			if (!dead)
			{
				return;
			}
			RecentLook.Remove(id);
			if (enabled && !SameSkeleton(value2.Item1, current))
			{
				Resource val = FirstStageFor(value2.Item1);
				Resource val2 = FirstStageFor(current);
				if (val != null && val2 != null && SameSkeleton(val, val2) && !SameSkeleton(current, val2))
				{
					Resource item = value2.Item1;
					Variant val3 = track.Call(StringName.op_Implicit("get_track_time"), Array.Empty<Variant>());
					double time = ((Variant)(ref val3)).AsDouble();
					val3 = state.Call(StringName.op_Implicit("get_time_scale"), Array.Empty<Variant>());
					BeginFade(spine, item, animation, time, ((Variant)(ref val3)).AsDouble());
				}
			}
		}
	}

	private static bool SameSkeleton(Resource a, Resource b)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)a).Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
		val = ((GodotObject)b).Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject val3 = ((Variant)(ref val)).AsGodotObject();
		val = ((GodotObject)a).Get(StringName.op_Implicit("atlas_res"));
		GodotObject val4 = ((Variant)(ref val)).AsGodotObject();
		val = ((GodotObject)b).Get(StringName.op_Implicit("atlas_res"));
		GodotObject val5 = ((Variant)(ref val)).AsGodotObject();
		if (a != b)
		{
			if (val2 == val3)
			{
				return val4 == val5;
			}
			return false;
		}
		return true;
	}

	private static Resource FirstStageFor(Resource current)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)current).Get(StringName.op_Implicit("skeleton_file_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		string text = ((obj2 != null) ? ((Resource)obj2).ResourcePath : null) ?? "";
		Type type = Host.GetType("JoanofArcIroncladHpwear.SkinCatalog");
		string[] array = new string[3] { "ironclad", "ironclad_shop", "restsite" };
		foreach (string text2 in array)
		{
			if ((int)type.GetMethod("StageFromPath", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Invoke(null, new object[2] { text2, text }) > 0)
			{
				object obj3 = Call("JoanofArcIroncladHpwear.SpineSwap", "LoadStageData", new object[2] { text2, 1 });
				return (Resource)((obj3 is Resource) ? obj3 : null);
			}
		}
		return null;
	}

	public static Texture2D SafePortraitTexture(Texture2D original)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)original))
		{
			return null;
		}
		return original;
	}

	public static void FadeBeforeSafetySwap(Node spine)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		LastLook.Remove(instanceId);
		RecentLook.Remove(instanceId);
		Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		Resource val2 = (Resource)(object)((obj is Resource) ? obj : null);
		val = ((GodotObject)spine).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
		GodotObject val3 = ((Variant)(ref val)).AsGodotObject();
		object obj2;
		if (val3 == null)
		{
			obj2 = null;
		}
		else
		{
			val = val3.Call(StringName.op_Implicit("get_current"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0) });
			obj2 = ((Variant)(ref val)).AsGodotObject();
		}
		GodotObject val4 = (GodotObject)obj2;
		if (val2 != null && val4 != null && val3 != null)
		{
			val = val4.Call(StringName.op_Implicit("get_track_time"), Array.Empty<Variant>());
			double time = ((Variant)(ref val)).AsDouble();
			val = val3.Call(StringName.op_Implicit("get_time_scale"), Array.Empty<Variant>());
			BeginFade(spine, val2, "die", time, ((Variant)(ref val)).AsDouble());
		}
	}

	private static void BeginFade(Node spine, Resource old, string animation, double time, double speed)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = (Node2D)(object)((spine is Node2D) ? spine : null);
		if (val == null || !((Node)val).IsInsideTree())
		{
			return;
		}
		ulong instanceId = ((GodotObject)val).GetInstanceId();
		if (Overlays.Remove(instanceId, out var value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			((CanvasItem)value).Hide();
			((Node)value).QueueFree();
		}
		Node2D val2 = null;
		try
		{
			Variant val3 = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			GodotObject obj = ((Variant)(ref val3)).AsGodotObject();
			val2 = (Node2D)(object)((obj is Node2D) ? obj : null);
			if (val2 != null)
			{
				((GodotObject)val2).SetMeta(StringName.op_Implicit("desktop_death_fade_overlay"), Variant.op_Implicit(true));
				((Node)val2).Name = StringName.op_Implicit("PreviousDeathWear");
				((Node)val2).ProcessMode = (ProcessModeEnum)3;
				((CanvasItem)val2).TextureFilter = ((CanvasItem)val).TextureFilter;
				((CanvasItem)val2).TextureRepeat = ((CanvasItem)val).TextureRepeat;
				((CanvasItem)val2).SelfModulate = ((CanvasItem)val).SelfModulate;
				((CanvasItem)val2).Material = ((CanvasItem)val).Material;
				((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
				((GodotObject)val2).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)old));
				if (((GodotObject)val).HasMethod(StringName.op_Implicit("get_normal_material")) && ((GodotObject)val2).HasMethod(StringName.op_Implicit("set_normal_material")))
				{
					((GodotObject)val2).Call(StringName.op_Implicit("set_normal_material"), (Variant[])(object)new Variant[1] { ((GodotObject)val).Call(StringName.op_Implicit("get_normal_material"), Array.Empty<Variant>()) });
				}
				Call("JoanofArcIroncladHpwear.UnderwearVisibility", "Attach", new object[1] { val2 });
				Call("JoanofArcIroncladHpwear.Palette", "ApplyNow", new object[1] { val2 });
				val3 = ((GodotObject)val2).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
				GodotObject val4 = ((Variant)(ref val3)).AsGodotObject();
				object obj2;
				if (val4 == null)
				{
					obj2 = null;
				}
				else
				{
					val3 = val4.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
					{
						Variant.op_Implicit(animation),
						Variant.op_Implicit(false),
						Variant.op_Implicit(0)
					});
					obj2 = ((Variant)(ref val3)).AsGodotObject();
				}
				GodotObject val5 = (GodotObject)obj2;
				if (val5 == null)
				{
					((Node)val2).QueueFree();
					return;
				}
				val4.Call(StringName.op_Implicit("set_time_scale"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(speed) });
				val5.Call(StringName.op_Implicit("set_track_time"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(time) });
				((GodotObject)val2).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0.0) });
				Tween obj3 = ((Node)val2).CreateTween().SetPauseMode((TweenPauseMode)2);
				obj3.TweenProperty((GodotObject)(object)val2, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.0), 0.25).SetTrans((TransitionType)1).SetEase((EaseType)2);
				obj3.TweenCallback(Callable.From((Action)((Node)val2).QueueFree));
				Overlays[instanceId] = val2;
			}
		}
		catch (Exception ex)
		{
			if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				((Node)val2).QueueFree();
			}
			if (Errors++ < 3)
			{
				GD.PushWarning("DesktopDeathSafety fade: " + ex.GetBaseException().Message);
			}
		}
	}
}
