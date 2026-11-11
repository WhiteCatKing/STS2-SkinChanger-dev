using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DesktopDeathSafety;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace JoanofArcIroncladHpwear;

internal static class Palette
{
	private sealed class Page
	{
		internal required string Key;

		internal required ShaderMaterial Material;

		internal bool Emissive;

		internal SubViewport? PortraitView;
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

	private static readonly string[] PageKeys = new string[2] { "s1_ironclad", "s1_restsite" };

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
		Shader shader = new Shader
		{
			Code = ReadText("shader.txt")
		};
		string[] pageKeys = PageKeys;
		foreach (string key in pageKeys)
		{
			BuildPage(shader, key);
		}
		pageKeys = new string[5] { "s1_charselect", "hand_paper", "hand_point", "hand_rock", "hand_scissors" };
		foreach (string text in pageKeys)
		{
			if (FileAccess.FileExists("res://joanofarcironcladhpwear/palette/" + text + "/index.png"))
			{
				BuildPage(shader, text);
			}
		}
		int[] array = new int[1] { 1 };
		foreach (int value in array)
		{
			pageKeys = new string[2] { "run_icon", "select_icon" };
			foreach (string value2 in pageKeys)
			{
				string text2 = $"s{value}_{value2}";
				if (FileAccess.FileExists("res://joanofarcironcladhpwear/palette/" + text2 + "/index.png"))
				{
					BuildPage(shader, text2);
				}
			}
		}
	}

	internal static Texture2D? PortraitTexture(int stage, bool selection)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Expected O, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Expected O, but got Unknown
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		if (_started && !_failed)
		{
			Texture2D obj = TopBarPalette.Texture(stage, selection);
			if (obj != null)
			{
				return obj;
			}
		}
		string text = $"s{stage}_" + (selection ? "select_icon" : "run_icon");
		if (!_started || _failed || !Pages.TryGetValue(text, out Page value))
		{
			return null;
		}
		if (value.PortraitView != null && GodotObject.IsInstanceValid((GodotObject)(object)value.PortraitView))
		{
			return (Texture2D?)(object)((Viewport)value.PortraitView).GetTexture();
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return null;
		}
		Texture2D val2 = ResourceLoader.Load<Texture2D>("res://joanofarcironcladhpwear/" + text + ".ctex", (string)null, (CacheMode)1);
		if (val2 == null)
		{
			return null;
		}
		SubViewport val3 = new SubViewport
		{
			Name = StringName.op_Implicit("JoanPortrait_" + text),
			Size = new Vector2I(val2.GetWidth(), val2.GetHeight()),
			TransparentBg = true,
			RenderTargetUpdateMode = (UpdateMode)1,
			RenderTargetClearMode = (ClearMode)0,
			GuiDisableInput = true
		};
		TextureRect val4 = new TextureRect
		{
			Texture = val2,
			Material = (Material)(object)value.Material,
			Size = new Vector2((float)val3.Size.X, (float)val3.Size.Y),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val4).SetMeta(StringName.op_Implicit("JoanPortraitSource"), Variant.op_Implicit(true));
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		value.PortraitView = val3;
		((Node)val.Root).AddChild((Node)(object)val3, false, (InternalMode)0);
		return (Texture2D?)(object)((Viewport)val3).GetTexture();
	}

	private static void BuildPage(Shader shader, string key)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((key == "s1_ironclad" || key == "s2_ironclad") ? true : false);
		bool flag2 = flag;
		if (key.EndsWith("_run_icon", StringComparison.Ordinal) || key.EndsWith("_select_icon", StringComparison.Ordinal))
		{
			shader = new Shader
			{
				Code = shader.Code.Replace("shader_type canvas_item;", "shader_type canvas_item;\nrender_mode blend_disabled;")
			};
		}
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = shader
		};
		val.SetShaderParameter(StringName.op_Implicit("pixel_indices"), Variant.op_Implicit((GodotObject)(object)LoadTexture(key + "/index.png")));
		if (flag2)
		{
			val.SetShaderParameter(StringName.op_Implicit("emission_map"), Variant.op_Implicit((GodotObject)(object)LoadTexture(key + "/emission.png")));
		}
		for (int i = 0; i < 3; i++)
		{
			val.SetShaderParameter(StringName.op_Implicit($"palette_{i + 1}"), Variant.op_Implicit((GodotObject)(object)LoadTexture($"{key}/{i + 1}.png")));
		}
		Pages[key] = new Page
		{
			Key = key,
			Material = val,
			Emissive = flag2
		};
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
		return PortraitSync.HasVisibleTarget();
	}

	private static void Tick(SceneTree tree)
	{
		try
		{
			RefreshAppearance(tree);
			PortraitSync.RefreshTracked();
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
					if (((GodotObject)item).GetClass() == "SpineSprite")
					{
						HpWear.RefreshDeathSafety(item);
					}
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
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		_weights = weights;
		foreach (Page value in Pages.Values)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)value.Material))
			{
				continue;
			}
			try
			{
				value.Material.SetShaderParameter(StringName.op_Implicit("appearance_weights"), Variant.op_Implicit(weights));
				value.Material.SetShaderParameter(StringName.op_Implicit("emission"), Variant.op_Implicit(value.Emissive ? (weights.W * 0.26f) : 0f));
				if (value.PortraitView != null && GodotObject.IsInstanceValid((GodotObject)(object)value.PortraitView))
				{
					value.PortraitView.RenderTargetUpdateMode = (UpdateMode)1;
				}
			}
			catch (Exception ex)
			{
				RecordFailure(0uL, ex);
			}
		}
		TopBarPalette.Update(weights);
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
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		Bound.TryGetValue(instanceId, out Binding value);
		string key = PageOf(spine);
		Pages.TryGetValue(key, out Page value2);
		if (value2 == null)
		{
			RestoreBinding(spine);
			return;
		}
		Material material = GetMaterial(spine);
		if (material != value2.Material)
		{
			Material previous = (Material)((value != null && material == value.Applied) ? ((object)value.Previous) : ((object)material));
			SetMaterial(spine, (Material?)(object)value2.Material);
			Bound[instanceId] = new Binding
			{
				Previous = previous,
				Applied = value2.Material
			};
			if (spine.IsInsideTree() && !(spine is TextureRect))
			{
				((GodotObject)spine).Call(StringName.op_Implicit("update_skeleton"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(0.0) });
			}
		}
	}

	private static string PageOf(Node spine)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		TextureRect val = (TextureRect)(object)((spine is TextureRect) ? spine : null);
		if (val != null)
		{
			Node parent = ((Node)val).GetParent();
			NHandImage val2 = (NHandImage)(object)((parent is NHandImage) ? parent : null);
			if (val2 != null)
			{
				Player player = val2.Player;
				if (((player != null) ? player.Character : null) is Ironclad && SkinSelectionGate.PlayerAllowed(val2.Player))
				{
					Texture2D texture = val.Texture;
					string file = StringExtensions.GetFile(((texture != null) ? ((Resource)texture).ResourcePath : null) ?? "");
					if (!file.StartsWith("multiplayer_hand_ironclad_", StringComparison.Ordinal) || !file.EndsWith(".png", StringComparison.Ordinal))
					{
						return "";
					}
					string text = file;
					int length = "multiplayer_hand_ironclad_".Length;
					return "hand_" + text.Substring(length, text.Length - 4 - length);
				}
			}
			return "";
		}
		Variant val3 = ((GodotObject)spine).Get(StringName.op_Implicit("skeleton_data_res"));
		GodotObject val4 = ((Variant)(ref val3)).AsGodotObject();
		if (val4 == null)
		{
			return "";
		}
		val3 = val4.Get(StringName.op_Implicit("atlas_res"));
		GodotObject obj = ((Variant)(ref val3)).AsGodotObject();
		GodotObject obj2 = ((obj is Resource) ? obj : null);
		string text2 = ((obj2 != null) ? ((Resource)obj2).ResourcePath : null) ?? "";
		if (!text2.EndsWith(".spatlas", StringComparison.Ordinal))
		{
			val3 = val4.Get(StringName.op_Implicit("skeleton_file_res"));
			GodotObject obj3 = ((Variant)(ref val3)).AsGodotObject();
			GodotObject obj4 = ((obj3 is Resource) ? obj3 : null);
			text2 = ((obj4 != null) ? ((Resource)obj4).ResourcePath : null) ?? "";
		}
		if (!SkinCatalog.IsOurPath(text2))
		{
			return "";
		}
		string text3 = StringExtensions.GetFile(text2);
		int num = text3.LastIndexOf('.');
		if (num > 0)
		{
			text3 = text3.Substring(0, num);
		}
		return MapPage(text3);
	}

	private static string MapPage(string stem)
	{
		if (stem != null)
		{
			int length = stem.Length;
			if (length != 11)
			{
				if (length != 13)
				{
					if (length == 16)
					{
						char c = stem[1];
						if (c != '1')
						{
							if (c == '2' && stem == "s2_ironclad_shop")
							{
								goto IL_0102;
							}
						}
						else if (stem == "s1_ironclad_shop")
						{
							goto IL_00fa;
						}
					}
				}
				else
				{
					switch (stem[1])
					{
					case '1':
						if (!(stem == "s1_charselect"))
						{
							break;
						}
						return "s1_charselect";
					case '2':
						if (!(stem == "s2_charselect"))
						{
							break;
						}
						return "s2_charselect";
					case '3':
						if (!(stem == "s3_charselect"))
						{
							break;
						}
						return "s3_charselect";
					}
				}
			}
			else
			{
				char c = stem[1];
				if (c != '1')
				{
					if (c == '2')
					{
						if (stem == "s2_ironclad")
						{
							goto IL_0102;
						}
						if (stem == "s2_restsite")
						{
							return "s2_restsite";
						}
					}
				}
				else
				{
					if (stem == "s1_ironclad")
					{
						goto IL_00fa;
					}
					if (stem == "s1_restsite")
					{
						return "s1_restsite";
					}
				}
			}
		}
		return "";
		IL_0102:
		return "s2_ironclad";
		IL_00fa:
		return "s1_ironclad";
	}

	private static void Track(Node node)
	{
		PortraitSync.Track(node);
		if (((GodotObject)node).GetClass() == "SpineSprite" || (node is TextureRect && node.GetParent() is NHandImage))
		{
			Tracked.Add(node);
		}
	}

	private static Material? GetMaterial(Node node)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		TextureRect val = (TextureRect)(object)((node is TextureRect) ? node : null);
		if (val == null)
		{
			Variant val2 = ((GodotObject)node).Call(StringName.op_Implicit("get_normal_material"), Array.Empty<Variant>());
			GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
			return (Material?)(object)((obj is Material) ? obj : null);
		}
		return ((CanvasItem)val).Material;
	}

	private static void SetMaterial(Node node, Material? material)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		TextureRect val = (TextureRect)(object)((node is TextureRect) ? node : null);
		if (val != null)
		{
			((CanvasItem)val).Material = material;
			return;
		}
		((GodotObject)node).Call(StringName.op_Implicit("set_normal_material"), (Variant[])(object)new Variant[1] { (Variant)((material == null) ? default(Variant) : Variant.From<Material>(ref material)) });
	}

	private static void RestoreBinding(Node spine)
	{
		ulong instanceId = ((GodotObject)spine).GetInstanceId();
		if (Bound.TryGetValue(instanceId, out Binding value))
		{
			if (GetMaterial(spine) == value.Applied)
			{
				SetMaterial(spine, value.Previous);
			}
			Bound.Remove(instanceId);
		}
	}

	private static void Untrack(Node node)
	{
		PortraitSync.Untrack(node);
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
