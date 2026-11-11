using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;

namespace CznCore.Presentation;

internal static class SharedDotRouteScenes
{
	private const string Root = "res://CznCore/";

	private const string GeneratedRoot = "res://CznCore/generated/dot/";

	private const string RangesMeta = "czn_dot_ranges";

	private const string LifetimeMeta = "czn_effect_scene_lifetime_seconds";

	private static readonly Lazy<JsonElement> Routes = new Lazy<JsonElement>(() => JsonDocument.Parse(FileAccess.GetFileAsString("res://CznCore/presentation/dot.json")).RootElement.GetProperty("routes"));

	public static IEnumerable<string> ScenePaths => (from route in Routes.Value.EnumerateObject()
		select ScenePath(route.Name)).ToArray();

	public static string ScenePath(string route)
	{
		return "res://CznCore/generated/dot/" + route + ".tscn";
	}

	public static float Lifetime(Node2D effect)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		StringName obj = StringName.op_Implicit("czn_effect_scene_lifetime_seconds");
		Variant val = default(Variant);
		val = ((GodotObject)effect).GetMeta(obj, val);
		return ((Variant)(ref val)).AsSingle();
	}

	public static PackedScene? Build(string scenePath)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Expected O, but got Unknown
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		if (!scenePath.StartsWith("res://CznCore/generated/dot/", StringComparison.Ordinal) || !Routes.Value.TryGetProperty(Path.GetFileNameWithoutExtension(scenePath), out var value))
		{
			return null;
		}
		JsonElement property = value.GetProperty("event");
		JsonElement property2 = property.GetProperty("offset");
		Node2D val = new Node2D
		{
			Name = StringName.op_Implicit("CznDotEffect")
		};
		Node2D val2 = new Node2D
		{
			Name = StringName.op_Implicit("Surface"),
			Modulate = new Color(1f, 1f, 1f, property.GetProperty("opacity").GetSingle() / 255f)
		};
		SetRanges(val2, property2[0].GetRawText(), property2[1].GetRawText(), Text(property, "scale"), Text(property, "rotation"));
		Mount((Node)(object)val, val2, val);
		Dictionary<string, Node2D> dictionary = new Dictionary<string, Node2D>(StringComparer.Ordinal);
		float num = 0f;
		foreach (JsonElement item in value.GetProperty("primitives").EnumerateArray())
		{
			string text = Text(item, "source");
			bool flag = Text(item, "format") == "spine";
			JsonElement value2;
			Node2D val3 = (Node2D)((!item.TryGetProperty("identity_node", out value2) || !value2.GetBoolean()) ? (flag ? ((object)Spine(text, Text(item, "ani"))) : ((object)ResourceLoader.Load<PackedScene>("res://CznCore/scenes/dot/" + text + ".tscn", (string)null, (CacheMode)1).Instantiate<Node2D>((GenEditState)0))) : ((object)new Node2D()));
			((Node)val3).Name = StringName.op_Implicit(text);
			if (flag)
			{
				SetRanges(val3, Text(item, "x"), Text(item, "y"), Text(item, "scale"), Text(item, "rotate"));
				((CanvasItem)val3).ZIndex = (int)Number(Text(item, "z"), 0f);
			}
			string text2 = Text(item, "attach");
			Mount((Node)(object)((text2.Length == 0) ? val2 : dictionary[text2.Split('/')[0]]), val3, val);
			dictionary[text] = val3;
			num = Math.Max(num, item.GetProperty("duration_seconds").GetSingle() + Number(Text(item, "delay"), 0f) / 1000f + Number(Text(item, "particle_tail_seconds"), 0f));
		}
		((GodotObject)val).SetMeta(StringName.op_Implicit("czn_effect_scene_lifetime_seconds"), Variant.op_Implicit(num));
		Roll(val);
		PackedScene val4 = new PackedScene();
		Error val5 = val4.Pack((Node)(object)val);
		((GodotObject)val).Free();
		if ((int)val5 != 0)
		{
			return null;
		}
		return val4;
	}

	public static void Roll(Node2D effect)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node2D item in ((IEnumerable)((Node)effect).FindChildren("*", "Node2D", true, false)).OfType<Node2D>())
		{
			if (((GodotObject)item).HasMeta(StringName.op_Implicit("czn_dot_ranges")))
			{
				StringName obj = StringName.op_Implicit("czn_dot_ranges");
				Variant val = default(Variant);
				val = ((GodotObject)item).GetMeta(obj, val);
				string[] array = ((Variant)(ref val)).AsStringArray();
				item.Position = new Vector2(Number(array[0], 0f), 0f - Number(array[1], 0f));
				item.Scale = Vector2.One * Number(array[2], 1f);
				item.RotationDegrees = 0f - Number(array[3], 0f);
			}
		}
	}

	private static Node2D Spine(string name, string animation)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
		Node2D val2 = (Node2D)((Variant)(ref val)).AsGodotObject();
		((GodotObject)val2).Set(StringName.op_Implicit("skeleton_data_res"), Variant.op_Implicit((GodotObject)(object)ResourceLoader.Load("res://CznCore/animations/dot/" + name + "_skel_data.tres", "", (CacheMode)1)));
		((GodotObject)val2).SetMeta(StringName.op_Implicit("czn_spine_animation_name"), Variant.op_Implicit((animation.Length == 0) ? "animation" : animation));
		return val2;
	}

	private static void SetRanges(Node2D node, string x, string y, string scale, string rotate)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)node).SetMeta(StringName.op_Implicit("czn_dot_ranges"), Variant.op_Implicit(new string[4] { x, y, scale, rotate }));
	}

	private static void Mount(Node parent, Node2D node, Node2D owner)
	{
		parent.AddChild((Node)(object)node, false, (InternalMode)0);
		((Node)node).Owner = (Node)(object)owner;
	}

	private static string Text(JsonElement element, string property)
	{
		if (element.TryGetProperty(property, out var value))
		{
			if (value.ValueKind != JsonValueKind.String)
			{
				if (value.ValueKind != JsonValueKind.Number)
				{
					return "";
				}
				return value.GetRawText();
			}
			return value.GetString();
		}
		return "";
	}

	private static float Number(string text, float fallback)
	{
		if (text.Length == 0)
		{
			return fallback;
		}
		string[] array = text.Split('~');
		if (array.Length != 2)
		{
			return float.Parse(text, CultureInfo.InvariantCulture);
		}
		float num = float.Parse(array[0], CultureInfo.InvariantCulture);
		float num2 = float.Parse(array[1], CultureInfo.InvariantCulture);
		return num + Random.Shared.NextSingle() * (num2 - num);
	}
}
