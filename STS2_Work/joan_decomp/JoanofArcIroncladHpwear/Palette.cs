using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

namespace JoanofArcIroncladHpwear;

internal static class Palette
{
	private sealed class Page
	{
		internal required string Key;

		internal required ShaderMaterial Material;

		internal bool Emissive;
	}

	private sealed class Binding
	{
		internal Material? Previous;

		internal ShaderMaterial Applied;
	}

	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static Action _003C0_003E__Start;

		public static NodeAddedEventHandler _003C1_003E__Track;

		public static NodeRemovedEventHandler _003C2_003E__Untrack;
	}

	private const string Assets = "res://joanofarcironcladhpwear/palette/";

	private const double FadeSeconds = 0.25;

	private const float NeonEmission = 0.26f;

	internal static readonly string[] Names = new string[4] { "原色", "黑贞德", "Saber 王庭蓝", "霓虹审判官" };

	private static readonly string[] PageKeys = new string[4] { "s1_ironclad", "s2_ironclad", "s1_restsite", "s2_restsite" };

	private static readonly Dictionary<string, Page> Pages = new Dictionary<string, Page>(StringComparer.Ordinal);

	private static readonly Dictionary<ulong, Binding> Bound = new Dictionary<ulong, Binding>();

	private static readonly HashSet<Node> Tracked = new HashSet<Node>();

	private static readonly List<Node> Scratch = new List<Node>();

	private static readonly Dictionary<ulong, ulong> RetryAfter = new Dictionary<ulong, ulong>();

	private static Timer? _timer;

	private static Tween? _fade;

	private static int _preset = -1;

	private static Vector4 _weights = new Vector4(1f, 0f, 0f, 0f);

	private static bool _started;

	private static bool _failed;

	internal static bool Active
	{
		get
		{
			if (_started)
			{
				return !_failed;
			}
			return false;
		}
	}

	internal static void StartDeferred()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Callable val = Callable.From((Action)Start);
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("palette start deferred: " + ex.Message);
		}
	}

	private static void Start()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Expected O, but got Unknown
		if (_started || _failed)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (tree != null)
		{
			try
			{
				BuildPages();
			}
			catch (Exception ex)
			{
				_failed = true;
				ModEntry.TryError("palette assets unavailable, appearance switching disabled: " + ex.GetBaseException().Message);
				return;
			}
			_started = true;
			_preset = CurrentPreset();
			SetWeights(PresetWeights(_preset));
			SceneTree obj = tree;
			object obj2 = _003C_003EO._003C1_003E__Track;
			if (obj2 == null)
			{
				NodeAddedEventHandler val = Track;
				_003C_003EO._003C1_003E__Track = val;
				obj2 = (object)val;
			}
			obj.NodeAdded += (NodeAddedEventHandler)obj2;
			SceneTree obj3 = tree;
			object obj4 = _003C_003EO._003C2_003E__Untrack;
			if (obj4 == null)
			{
				NodeRemovedEventHandler val2 = Untrack;
				_003C_003EO._003C2_003E__Untrack = val2;
				obj4 = (object)val2;
			}
			obj3.NodeRemoved += (NodeRemovedEventHandler)obj4;
			Node currentScene = tree.CurrentScene;
			if (currentScene != null)
			{
				Discover(currentScene, 0);
			}
			_timer = new Timer
			{
				Name = StringName.op_Implicit("JoanPaletteTick"),
				WaitTime = 0.1,
				Autostart = true,
				ProcessMode = (ProcessModeEnum)3
			};
			_timer.Timeout += delegate
			{
				Tick(tree);
			};
			((Node)tree.Root).AddChild((Node)(object)_timer, false, (InternalMode)0);
			AppearanceHotkey.Start(tree);
			ModEntry.TryInfo($"palette ready: {Pages.Count} pages, {Names.Length} appearances, current={Names[_preset]}.");
		}
	}

	private static void BuildPages()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		Shader shader = new Shader
		{
			Code = ReadText("shader.txt")
		};
		string[] pageKeys = PageKeys;
		foreach (string text in pageKeys)
		{
			bool flag = ((text == "s1_ironclad" || text == "s2_ironclad") ? true : false);
			bool flag2 = flag;
			ShaderMaterial val = new ShaderMaterial
			{
				Shader = shader
			};
			val.SetShaderParameter(StringName.op_Implicit("pixel_indices"), Variant.op_Implicit((GodotObject)(object)LoadTexture(text + "/index.png")));
			if (flag2)
			{
				val.SetShaderParameter(StringName.op_Implicit("emission_map"), Variant.op_Implicit((GodotObject)(object)LoadTexture(text + "/emission.png")));
			}
			for (int j = 0; j < 3; j++)
			{
				val.SetShaderParameter(StringName.op_Implicit($"palette_{j + 1}"), Variant.op_Implicit((GodotObject)(object)LoadTexture($"{text}/{j + 1}.png")));
			}
			Pages[text] = new Page
			{
				Key = text,
				Material = val,
				Emissive = flag2
			};
		}
	}

	private static string ReadText(string relative)
	{
		string text = "res://joanofarcironcladhpwear/palette/" + relative;
		string fileAsString = FileAccess.GetFileAsString(text);
		if (string.IsNullOrEmpty(fileAsString))
		{
			throw new InvalidOperationException("cannot read " + text);
		}
		return fileAsString;
	}

	private static ImageTexture LoadTexture(string relative)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		string text = "res://joanofarcironcladhpwear/palette/" + relative;
		FileAccess val = FileAccess.Open(text, (ModeFlags)1);
		try
		{
			if (val == null)
			{
				throw new InvalidOperationException($"cannot open {text} ({FileAccess.GetOpenError()})");
			}
			Image val2 = new Image();
			Error val3 = val2.LoadPngFromBuffer(val.GetBuffer((long)val.GetLength()));
			if ((int)val3 != 0)
			{
				throw new InvalidOperationException($"{text}: {val3}");
			}
			return ImageTexture.CreateFromImage(val2);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static int CurrentPreset()
	{
		return Math.Clamp(WearSettingsRuntime.Current.Appearance, 1, Names.Length) - 1;
	}

	internal static void RefreshAppearance(SceneTree tree)
	{
		int num = CurrentPreset();
		if (num != _preset)
		{
			SwitchTo(tree, num);
		}
	}

	internal static bool HasVisibleTarget()
	{
		foreach (Node item in Tracked)
		{
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)item))
				{
					CanvasItem val = (CanvasItem)(object)((item is CanvasItem) ? item : null);
					if (val != null && val.IsVisibleInTree() && PageOf(item).Length != 0)
					{
						return true;
					}
				}
			}
			catch
			{
			}
		}
		return false;
	}

	private static void Tick(SceneTree tree)
	{
		try
		{
			RefreshAppearance(tree);
		}
		catch (Exception ex)
		{
			RecordFailure(0uL, ex);
		}
		Scratch.Clear();
		Scratch.AddRange(Tracked);
		foreach (Node item in Scratch)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)item))
			{
				Tracked.Remove(item);
				continue;
			}
			ulong instanceId = ((GodotObject)item).GetInstanceId();
			if (RetryAfter.TryGetValue(instanceId, out var value) && Time.GetTicksMsec() < value)
			{
				continue;
			}
			try
			{
				if (item.IsInsideTree())
				{
					Apply(item);
				}
				RetryAfter.Remove(instanceId);
			}
			catch (Exception ex2)
			{
				RecordFailure(instanceId, ex2);
			}
		}
	}

	private static void RecordFailure(ulong id, Exception ex)
	{
		ulong ticksMsec = Time.GetTicksMsec();
		if (!RetryAfter.TryGetValue(id, out var value) || ticksMsec >= value)
		{
			ModEntry.TryInfo("palette node " + id + " temporarily unavailable: " + ex.GetBaseException().Message);
		}
		RetryAfter[id] = ticksMsec + 2000;
	}

	private static Vector4 PresetWeights(int preset)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		return (Vector4)(preset switch
		{
			1 => new Vector4(0f, 1f, 0f, 0f), 
			2 => new Vector4(0f, 0f, 1f, 0f), 
			3 => new Vector4(0f, 0f, 0f, 1f), 
			_ => new Vector4(1f, 0f, 0f, 0f), 
		});
	}

	private static void SwitchTo(SceneTree tree, int next)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Vector4 from = _weights;
		Vector4 to = PresetWeights(next);
		if (_fade != null && GodotObject.IsInstanceValid((GodotObject)(object)_fade))
		{
			_fade.Kill();
		}
		_preset = next;
		_fade = tree.CreateTween();
		_fade.TweenMethod(Callable.From<float>((Action<float>)delegate(float p)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			SetWeights(((Vector4)(ref from)).Lerp(to, p));
		}), Variant.op_Implicit(0f), Variant.op_Implicit(1f), 0.25).SetTrans((TransitionType)1).SetEase((EaseType)2);
		ModEntry.TryInfo("[palette] " + Names[next]);
	}

	private static void SetWeights(Vector4 weights)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		_weights = weights;
		foreach (Page value in Pages.Values)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Material))
			{
				try
				{
					value.Material.SetShaderParameter(StringName.op_Implicit("appearance_weights"), Variant.op_Implicit(weights));
					value.Material.SetShaderParameter(StringName.op_Implicit("emission"), Variant.op_Implicit(value.Emissive ? (weights.W * 0.26f) : 0f));
				}
				catch (Exception ex)
				{
					RecordFailure(0uL, ex);
				}
			}
		}
	}

	internal static void ApplyNow(Node spine)
	{
		if (!_started || _failed || !GodotObject.IsInstanceValid((GodotObject)(object)spine))
		{
			return;
		}
		try
		{
			Apply(spine);
		}
		catch (Exception ex)
		{
			RecordFailure(((GodotObject)spine).GetInstanceId(), ex);
		}
	}

	private static void Apply(Node spine)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		Bound.TryGetValue(instanceId, out Binding value);
		string key = PageOf(spine);
		Pages.TryGetValue(key, out Page value2);
		if (value2 == null)
		{
			RestoreBinding(spine);
			return;
		}
		Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_normal_material"), Array.Empty<Variant>());
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		Material val2 = (Material)(object)((obj is Material) ? obj : null);
		if (val2 != value2.Material)
		{
			Material previous = (Material)((value != null && val2 == value.Applied) ? ((object)value.Previous) : ((object)val2));
			((GodotObject)spine).Call(StringName.op_Implicit("set_normal_material"), (Variant[])(object)new Variant[1] { Variant.From<ShaderMaterial>(ref value2.Material) });
			Bound[instanceId] = new Binding
			{
				Previous = previous,
				Applied = value2.Material
			};
			if (spine.IsInsideTree())
			{
				((GodotObject)spine).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0.0) });
			}
		}
	}

	private static string PageOf(Node spine)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
		if (val2 == null)
		{
			return "";
		}
		val = val2.Get(StringName.op_Implicit("atlas_res"));
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		string text = ((obj2 != null) ? ((Resource)obj2).ResourcePath : null) ?? "";
		if (!text.EndsWith(".spatlas", StringComparison.Ordinal))
		{
			val = val2.Get(StringName.op_Implicit("skeleton_file_res"));
			GodotObject obj3 = ((Variant)(ref val)).AsGodotObject();
			GodotObject obj4 = ((obj3 is Resource) ? obj3 : null);
			text = ((obj4 != null) ? ((Resource)obj4).ResourcePath : null) ?? "";
		}
		if (!SkinCatalog.IsOurPath(text))
		{
			return "";
		}
		string text2 = StringExtensions.GetFile(text);
		int num = text2.LastIndexOf('.');
		if (num > 0)
		{
			text2 = text2.Substring(0, num);
		}
		return MapPage(text2);
	}

	private static string MapPage(string stem)
	{
		switch (stem)
		{
		case "s1_ironclad":
		case "s1_ironclad_shop":
			return "s1_ironclad";
		case "s2_ironclad":
		case "s2_ironclad_shop":
			return "s2_ironclad";
		case "s1_restsite":
			return "s1_restsite";
		case "s2_restsite":
			return "s2_restsite";
		default:
			return "";
		}
	}

	private static void Track(Node node)
	{
		if (((GodotObject)node).GetClass() == "SpineSprite")
		{
			Tracked.Add(node);
		}
	}

	private static void RestoreBinding(Node spine)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		if (!Bound.TryGetValue(instanceId, out Binding value))
		{
			return;
		}
		Variant val = ((GodotObject)spine).Call(StringName.op_Implicit("get_normal_material"), Array.Empty<Variant>());
		GodotObject obj = ((Variant)(ref val)).AsGodotObject();
		if (((obj is Material) ? obj : null) == value.Applied)
		{
			StringName obj2 = StringName.op_Implicit("set_normal_material");
			Variant[] array = new Variant[1];
			Variant val2;
			if (value.Previous != null)
			{
				val2 = Variant.From<Material>(ref value.Previous);
			}
			else
			{
				val = default(Variant);
				val2 = val;
			}
			array[0] = val2;
			((GodotObject)spine).Call(obj2, (Variant[])(object)array);
		}
		Bound.Remove(instanceId);
	}

	private static void Untrack(Node node)
	{
		if (!Tracked.Remove(node))
		{
			return;
		}
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		try
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)node))
			{
				RestoreBinding(node);
			}
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("palette detach: " + ex.GetBaseException().Message);
		}
		finally
		{
			Bound.Remove(instanceId);
			RetryAfter.Remove(instanceId);
		}
	}

	private static void Discover(Node node, int depth)
	{
		if (depth <= 12 && GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			Track(node);
			int childCount = node.GetChildCount(false);
			for (int i = 0; i < childCount; i++)
			{
				Discover(node.GetChild(i, false), depth + 1);
			}
		}
	}
}
