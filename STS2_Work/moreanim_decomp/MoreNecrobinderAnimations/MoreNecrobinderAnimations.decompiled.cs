using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Saves;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: InternalsVisibleTo("MoreNecrobinderAnimations.NativeTests")]
[assembly: InternalsVisibleTo("MoreNecrobinderAnimations.Settings")]
[assembly: InternalsVisibleTo("MoreNecrobinderAnimations.BaseLib")]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("MoreNecrobinderAnimations")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("0.1.26.0")]
[assembly: AssemblyInformationalVersion("0.1.26")]
[assembly: AssemblyProduct("MoreNecrobinderAnimations")]
[assembly: AssemblyTitle("MoreNecrobinderAnimations")]
[assembly: AssemblyVersion("0.1.26.0")]
[module: RefSafetyRules(11)]
namespace MoreNecrobinderAnimations;

internal static class ActorAnchor
{
	internal static Transform2D Ground(Node2D body)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		for (Node parent = ((Node)body).GetParent(); parent != null; parent = parent.GetParent())
		{
			NCreatureVisuals val = (NCreatureVisuals)(object)((parent is NCreatureVisuals) ? parent : null);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val.Bounds))
			{
				Control bounds = val.Bounds;
				float num = Math.Clamp(bounds.Size.Y / 400f, 0.25f, 3f);
				return ((CanvasItem)bounds).GetGlobalTransform() * new Transform2D(0f, Vector2.One * num, 0f, new Vector2(bounds.Size.X * 0.5f, bounds.Size.Y));
			}
		}
		return new Transform2D(0f, body.GlobalPosition);
	}

	internal static Transform2D Head(Node2D body)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		return Ground(body) * new Transform2D(0f, Vector2.One * 0.27f, 0f, new Vector2(0f, -330f));
	}
}
internal sealed class ActorCapture : IDisposable
{
	private readonly Node2D _body;

	private readonly SpineValues _values;

	private readonly SubViewport _viewport;

	private readonly Rid _originalParent;

	private readonly int _size;

	private readonly float _padding;

	private readonly Func<Rect2>? _customBounds;

	internal readonly Sprite2D Composite;

	private bool _disposed;

	private bool _redirected;

	private Vector2I _resolution;

	internal Texture2D Texture => (Texture2D)(object)((Viewport)_viewport).GetTexture();

	internal Rect2 Bounds { get; private set; }

	internal ActorCapture(Node2D body, Node lifetime, int size, float padding, ShaderMaterial material, Func<Rect2>? bounds = null)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		_body = body;
		_size = size;
		_padding = padding;
		_customBounds = bounds;
		_values = new SpineValues(body);
		_originalParent = ((CanvasItem)((Node)body).GetParent()).GetCanvasItem();
		_viewport = new SubViewport
		{
			Name = StringName.op_Implicit("NecroActorCapture"),
			Size = new Vector2I(2, 2),
			TransparentBg = true,
			Disable3D = true,
			RenderTargetUpdateMode = (UpdateMode)4,
			RenderTargetClearMode = (ClearMode)0
		};
		lifetime.AddChild((Node)(object)_viewport, false, (InternalMode)0);
		material.SetShaderParameter(StringName.op_Implicit("capture_texture"), Variant.op_Implicit((GodotObject)(object)Texture));
		Composite = new Sprite2D
		{
			Name = StringName.op_Implicit("NecroActorComposite"),
			Texture = Texture,
			Centered = false,
			Material = (Material)(object)material,
			TextureFilter = (TextureFilterEnum)2
		};
		((Node)body).GetParent().AddChild((Node)(object)Composite, false, (InternalMode)0);
		((Node)body).GetParent().MoveChild((Node)(object)Composite, ((Node)body).GetIndex(false) + 1);
		try
		{
			RenderingServer.FramePreDraw += Prepare;
			Prepare();
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private void Prepare()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		if (!_values.Current)
		{
			Dispose();
			return;
		}
		Rect2 val = (Rect2)(((??)_customBounds?.Invoke()) ?? ((??)BodyDisplay.Bounds(_body)) ?? (_body.GlobalTransform * _values.Bounds));
		Rect2 val2 = BodyDisplay.ExtendBounds(_body, val);
		val = ((Rect2)(ref val2)).Grow(_padding);
		Vector2 size = ((Rect2)(ref val)).Size;
		if (((Vector2)(ref size)).IsFinite() && !(((Rect2)(ref val)).Size.X <= 0f) && !(((Rect2)(ref val)).Size.Y <= 0f))
		{
			Bounds = val;
			val2 = ((Node)_body).GetViewport().GetFinalTransform() * ((CanvasItem)_body).GetCanvasTransform() * val;
			Vector2 size2 = ((Rect2)(ref val2)).Size;
			Vector2I val3 = default(Vector2I);
			((Vector2I)(ref val3))..ctor(Math.Max(_resolution.X, Budget(size2.X)), Math.Max(_resolution.Y, Budget(size2.Y)));
			if (val3 != _resolution)
			{
				_resolution = val3;
				_viewport.Size = val3;
			}
			Vector2 val4 = default(Vector2);
			((Vector2)(ref val4))..ctor((float)_resolution.X / ((Rect2)(ref val)).Size.X, (float)_resolution.Y / ((Rect2)(ref val)).Size.Y);
			((Viewport)_viewport).CanvasTransform = new Transform2D(0f, val4, 0f, -((Rect2)(ref val)).Position * val4);
			RenderingServer.CanvasItemSetParent(((CanvasItem)_body).GetCanvasItem(), ((Viewport)_viewport).FindWorld2D().Canvas);
			RenderingServer.CanvasItemSetTransform(((CanvasItem)_body).GetCanvasItem(), _body.GlobalTransform);
			_redirected = true;
			BodyDisplay.Redirect(_body, ((Viewport)_viewport).FindWorld2D().Canvas);
			((Node2D)Composite).GlobalTransform = new Transform2D(0f, ((Rect2)(ref val)).Size / Vector2I.op_Implicit(_resolution), 0f, ((Rect2)(ref val)).Position);
		}
		int Budget(float length)
		{
			return Math.Clamp((int)MathF.Ceiling(length / 32f) * 32, 128, _size);
		}
	}

	public void Dispose()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed)
		{
			_disposed = true;
			RenderingServer.FramePreDraw -= Prepare;
			if (_redirected && GodotObject.IsInstanceValid((GodotObject)(object)_body))
			{
				RenderingServer.CanvasItemSetParent(((CanvasItem)_body).GetCanvasItem(), _originalParent);
				RenderingServer.CanvasItemSetTransform(((CanvasItem)_body).GetCanvasItem(), _body.Transform);
			}
			BodyDisplay.Restore(_body);
			if (GodotObject.IsInstanceValid((GodotObject)(object)Composite))
			{
				((CanvasItem)Composite).Hide();
				((Node)Composite).QueueFree();
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_viewport))
			{
				((Node)_viewport).QueueFree();
			}
			_values.Dispose();
		}
	}
}
internal static class ActorLayers
{
	internal sealed record Display(Node2D Parent, Func<Transform2D> Body, Func<Transform2D> Wrist);

	private static readonly Dictionary<Node2D, Display> Moving = new Dictionary<Node2D, Display>();

	internal static void Register(Node2D body, Display display)
	{
		Moving[body] = display;
	}

	internal static void Remove(Node2D body, Node2D owner)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (!Moving.TryGetValue(body, out Display value) || value.Parent != owner)
		{
			return;
		}
		Moving.Remove(body);
		foreach (Node child in ((Node)owner).GetChildren(false))
		{
			Node2D val = (Node2D)(object)((child is Node2D) ? child : null);
			if (val != null)
			{
				Variant meta = ((GodotObject)val).GetMeta(StringName.op_Implicit("necro_tag"), Variant.op_Implicit(""));
				if (((Variant)(ref meta)).AsString().StartsWith("osty-wrist", StringComparison.Ordinal))
				{
					Transform2D globalTransform = val.GlobalTransform;
					((Node)val).Reparent(((Node)body).GetParent(), true);
					val.GlobalTransform = globalTransform;
				}
			}
		}
	}

	internal static Display? Get(Node2D body)
	{
		return Moving.GetValueOrDefault(body);
	}

	internal static void Place(Node2D layer, Node parent, string tag)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)layer).GetParent() == parent && ((GodotObject)layer).HasMeta(StringName.op_Implicit("necro_layer")))
		{
			return;
		}
		if (((Node)layer).GetParent() != parent)
		{
			Node parent2 = ((Node)layer).GetParent();
			if (parent2 != null)
			{
				parent2.RemoveChild((Node)(object)layer);
			}
			parent.AddChild((Node)(object)layer, false, (InternalMode)0);
		}
		int num;
		switch (tag)
		{
		case "osty-back":
			num = 0;
			break;
		case "osty-body-back":
			num = 1;
			break;
		case "osty-wrist-back":
			num = 2;
			break;
		case "osty-body-front":
		case "osty-body":
			num = 3;
			break;
		case "osty-wrist-front":
			num = 4;
			break;
		default:
			num = 5;
			break;
		}
		int num2 = num;
		((GodotObject)layer).SetMeta(StringName.op_Implicit("necro_layer"), Variant.op_Implicit(num2));
		((GodotObject)layer).SetMeta(StringName.op_Implicit("necro_tag"), Variant.op_Implicit(tag));
		int num3 = 0;
		foreach (Node child in parent.GetChildren(false))
		{
			if (child != layer)
			{
				Variant meta = ((GodotObject)child).GetMeta(StringName.op_Implicit("necro_layer"), Variant.op_Implicit(0));
				if (((Variant)(ref meta)).AsInt32() <= num2)
				{
					num3++;
				}
			}
		}
		parent.MoveChild((Node)(object)layer, num3);
	}
}
public sealed record AnimationSettings
{
	public bool Enabled { get; init; } = true;

	public bool ScourgeEnabled { get; init; } = true;

	public bool GraveWardenEnabled { get; init; }

	public bool InvokeEnabled { get; init; } = true;

	public bool FriendshipEnabled { get; init; } = true;

	public bool NegativePulseEnabled { get; init; } = true;

	public bool NeurosurgeEnabled { get; init; } = true;

	public bool DeathbringerEnabled { get; init; } = true;

	public bool DirgeEnabled { get; init; } = true;

	public bool CaptureSpiritEnabled { get; init; } = true;

	public bool EndOfDaysEnabled { get; init; } = true;

	public bool BansheesCryEnabled { get; init; } = true;

	public bool UnleashEnabled { get; init; } = true;

	public bool ProtectorEnabled { get; init; } = true;

	public bool LethalityEnabled { get; init; } = true;

	public bool ReaperFormEnabled { get; init; } = true;

	public bool EradicateEnabled { get; init; } = true;

	public int OstySlamChance { get; init; } = 70;

	public static AnimationSettings Current { get; internal set; } = new AnimationSettings();

	internal static string FilePath => ProjectSettings.GlobalizePath("user://mod_settings/MoreNecrobinderAnimations.json");

	internal bool Allows(string card)
	{
		bool flag = Enabled;
		if (flag)
		{
			flag = card switch
			{
				"Scourge" => ScourgeEnabled, 
				"GraveWarden" => GraveWardenEnabled, 
				"Invoke" => InvokeEnabled, 
				"Friendship" => FriendshipEnabled, 
				"NegativePulse" => NegativePulseEnabled, 
				"Neurosurge" => NeurosurgeEnabled, 
				"Deathbringer" => DeathbringerEnabled, 
				"Dirge" => DirgeEnabled, 
				"CaptureSpirit" => CaptureSpiritEnabled, 
				"EndOfDays" => EndOfDaysEnabled, 
				"BansheesCry" => BansheesCryEnabled, 
				"Unleash" => UnleashEnabled, 
				"Protector" => ProtectorEnabled, 
				"Lethality" => LethalityEnabled, 
				"ReaperForm" => ReaperFormEnabled, 
				"Eradicate" => EradicateEnabled, 
				_ => false, 
			};
		}
		return flag;
	}

	internal static void Load()
	{
		try
		{
			if (File.Exists(FilePath))
			{
				Current = JsonSerializer.Deserialize<AnimationSettings>(File.ReadAllText(FilePath)) ?? new AnimationSettings();
			}
			Current = Current with
			{
				OstySlamChance = Math.Clamp(Current.OstySlamChance, 0, 100)
			};
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	internal static void Save(AnimationSettings value)
	{
		value = value with
		{
			OstySlamChance = Math.Clamp(value.OstySlamChance, 0, 100)
		};
		if (!(value == Current) || !File.Exists(FilePath))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
			string text = FilePath + ".tmp";
			File.WriteAllText(text, JsonSerializer.Serialize(value, new JsonSerializerOptions
			{
				WriteIndented = true
			}));
			File.Move(text, FilePath, overwrite: true);
			Current = value;
			VisualSession.ResetDisabled();
			EradicateVisual.ResetDisabled();
		}
	}

	[CompilerGenerated]
	private AnimationSettings(AnimationSettings original)
	{
		Enabled = original.Enabled;
		ScourgeEnabled = original.ScourgeEnabled;
		GraveWardenEnabled = original.GraveWardenEnabled;
		InvokeEnabled = original.InvokeEnabled;
		FriendshipEnabled = original.FriendshipEnabled;
		NegativePulseEnabled = original.NegativePulseEnabled;
		NeurosurgeEnabled = original.NeurosurgeEnabled;
		DeathbringerEnabled = original.DeathbringerEnabled;
		DirgeEnabled = original.DirgeEnabled;
		CaptureSpiritEnabled = original.CaptureSpiritEnabled;
		EndOfDaysEnabled = original.EndOfDaysEnabled;
		BansheesCryEnabled = original.BansheesCryEnabled;
		UnleashEnabled = original.UnleashEnabled;
		ProtectorEnabled = original.ProtectorEnabled;
		LethalityEnabled = original.LethalityEnabled;
		ReaperFormEnabled = original.ReaperFormEnabled;
		EradicateEnabled = original.EradicateEnabled;
		OstySlamChance = original.OstySlamChance;
	}

	public AnimationSettings()
	{
	}
}
internal sealed class BansheeEffect : IVisualEffect, IDisposable
{
	private readonly record struct Front(float Radius, float Height, float Width, float Opacity, float Breakup);

	private readonly ClipPlayer _body;

	private readonly NativeBansheeScream _scream;

	private readonly EffectQuad _refraction;

	private readonly EffectQuad _main;

	private readonly EffectQuad[] _ridges = new EffectQuad[2];

	private readonly EffectQuad[] _fragments = new EffectQuad[11];

	private readonly EffectQuad[] _spooky = new EffectQuad[3];

	private readonly BackBufferCopy _copy;

	private readonly Vector2 _origin;

	private readonly float _height;

	private readonly float _distance;

	private readonly float _acceleration;

	private readonly uint _seed;

	private readonly VisualShake _shake = new VisualShake();

	private readonly float[] _hits;

	private int _hitIndex;

	private bool _disposed;

	public float Duration => Math.Max(0.85f, _scream.Duration);

	public bool Hold => false;

	private static float Smooth(float a, float b, float x)
	{
		float num = Math.Clamp((x - a) / (b - a), 0f, 1f);
		return num * num * (3f - 2f * num);
	}

	private static uint Hash(uint seed, int index)
	{
		int num = (int)seed + (index + 1) * -1640531527;
		int num2 = (num ^ (num >>> 16)) * 569420461;
		int num3 = (num2 ^ (num2 >>> 15)) * 1935289751;
		return (uint)(num3 ^ (num3 >>> 15));
	}

	private float Random(int index)
	{
		return (float)Hash(_seed, index) / 4.2949673E+09f;
	}

	internal BansheeEffect(NCreature hero, NCreature osty, NCombatRoom room)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Span<byte> span = stackalloc byte[4];
			RandomNumberGenerator.Fill(span);
			_seed = BitConverter.ToUInt32(span);
			ClipAssets clipAssets = ClipAssets.Get("BansheesCry");
			_body = new ClipPlayer(clipAssets, hero, osty, (Node)(object)room.CombatVfxContainer);
			float[] mouth = clipAssets.Data.Mouth;
			_origin = (SpineCompatibility.Allows(hero.Body, "necrobinder") ? (hero.Body.GlobalTransform * (new Vector2(mouth[0], 0f - mouth[1]) / 0.27f)) : ActorAnchor.Head(hero.Body).Origin);
			Vector2[] array = (from c in room.CreatureNodes
				where (int)c.Entity.Side == 2 && c.Entity.IsAlive
				select c.VfxSpawnPosition).ToArray();
			_height = MathF.Max(148f, (array.Length == 0) ? 148f : array.Max((Vector2 p) => MathF.Abs(p.Y - _origin.Y) + 45f));
			_distance = ((array.Length == 0) ? 1040f : MathF.Max(0.001f, array.Max((Vector2 p) => MathF.Max(0f, p.X - _origin.X) / MathF.Sqrt(MathF.Max(1E-06f, 1f - MathF.Pow((p.Y - _origin.Y) / _height, 2f))))));
			_hits = array.Select(delegate(Vector2 p)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				Vector2 val = new Vector2(p.X - _origin.X, (p.Y - _origin.Y) * _distance / _height);
				float num = Math.Clamp(((Vector2)(ref val)).Length() / _distance, 0f, 1f);
				return 0.12f + 0.32f * num / (1.35f + MathF.Sqrt(1.8225001f - 1.4f * num));
			}).Order().ToArray();
			Rect2 val2 = ((CanvasItem)room.CombatVfxContainer).GetGlobalTransform() * BattleScreen.Bounds((CanvasItem)(object)room.CombatVfxContainer);
			float num2 = 0.65f * _distance / 0.16f;
			float num3 = MathF.Max(_distance, ((Rect2)(ref val2)).End.X + 180f - _origin.X);
			_acceleration = MathF.Max(90f * _distance, 2f * (num3 - _distance - num2 * 0.18f) / 0.0324f);
			ClipAssets clipAssets2 = ClipAssets.Get("BansheePrograms");
			Control combatVfxContainer = room.CombatVfxContainer;
			_copy = BattleScreen.Copy((Node)(object)combatVfxContainer, "NecroBansheeSceneCopy");
			_refraction = new EffectQuad((Node)(object)combatVfxContainer, clipAssets2.Shaders[2]);
			_refraction.Set("TEXTURE", Variant.op_Implicit((GodotObject)(object)clipAssets2.Textures[clipAssets2.Data.Extra.GetProperty("mask").GetInt32()]));
			_refraction.Set("u_tiling", Variant.op_Implicit(new Vector2(5f, 0.5f)));
			_refraction.Set("u_offset_speed", Variant.op_Implicit(new Vector2(0f, -2f)));
			_refraction.Set("u_band", Variant.op_Implicit(86f));
			_refraction.Set("u_amount", Variant.op_Implicit(1f));
			_refraction.Set("u_base_intensity", Variant.op_Implicit(0.04f));
			_refraction.Set("u_mask_radius", Variant.op_Implicit(0.25f));
			_refraction.Set("u_spooky_band", Variant.op_Implicit(new Vector2(0.48f, 0.82f)));
			_scream = new NativeBansheeScream((Node)(object)combatVfxContainer, _origin, _seed);
			for (int i = 0; i < 3; i++)
			{
				EffectQuad effectQuad = (_spooky[i] = new EffectQuad((Node)(object)combatVfxContainer, clipAssets2.Shaders[1]));
				effectQuad.Set("TEXTURE", Variant.op_Implicit((GodotObject)(object)clipAssets2.Textures[clipAssets2.Data.Extra.GetProperty("spooky").GetInt32()]));
				foreach (JsonProperty item in clipAssets2.Data.Extra.GetProperty("parameters").EnumerateObject())
				{
					JsonElement value = item.Value;
					if (value.ValueKind == JsonValueKind.Number)
					{
						effectQuad.Set(item.Name, Variant.op_Implicit(value.GetSingle()));
					}
					else if (value.ValueKind == JsonValueKind.Array)
					{
						switch (value.GetArrayLength())
						{
						case 2:
							effectQuad.Set(item.Name, Variant.op_Implicit(new Vector2(value[0].GetSingle(), value[1].GetSingle())));
							break;
						case 4:
							effectQuad.Set(item.Name, Variant.op_Implicit(new Vector4(value[0].GetSingle(), value[1].GetSingle(), value[2].GetSingle(), value[3].GetSingle())));
							break;
						}
					}
				}
			}
			for (int j = 0; j < 11; j++)
			{
				_fragments[j] = new EffectQuad((Node)(object)combatVfxContainer, clipAssets2.Shaders[0]);
			}
			for (int num4 = 1; num4 >= 0; num4--)
			{
				_ridges[num4] = new EffectQuad((Node)(object)combatVfxContainer, clipAssets2.Shaders[0]);
			}
			_main = new EffectQuad((Node)(object)combatVfxContainer, clipAssets2.Shaders[0]);
			Advance(0f);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private Front Sample(float age)
	{
		float num = Math.Clamp((age - 0.12f) / 0.16f, 0f, 1f);
		float num2 = MathF.Max(0f, age - 0.28f);
		float num3 = _distance * num * (1.35f - 0.35f * num) + 0.65f * _distance / 0.16f * num2 + 0.5f * _acceleration * num2 * num2;
		return new Front(num3, _height * num3 / _distance, 42f * (0.8f + 0.2f * num) * (1f + 0.3f * MathF.Exp(0f - MathF.Pow((age - 0.265f) / 0.049f, 2f))), Smooth(0.12f, 0.136f, age) * (1f - Smooth(0.43f, 0.5f, age)), Smooth(0.46f, 0.5f, age));
	}

	private void Band(EffectQuad q, Front front, float mode)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		if (front.Opacity <= 0.001f)
		{
			((CanvasItem)q.Node).Hide();
			return;
		}
		q.Rect(_origin + new Vector2(front.Radius, 0f), 0f - front.Radius - front.Width - 6f, 6f, front.Height * 2.12f);
		q.Set("u_span", Variant.op_Implicit(new Vector2(0f - front.Radius - front.Width - 6f, 6f)));
		q.Set("u_height", Variant.op_Implicit(front.Height));
		q.Set("u_depth", Variant.op_Implicit(front.Radius));
		q.Set("u_width", Variant.op_Implicit(front.Width));
		q.Set("u_opacity", Variant.op_Implicit(front.Opacity));
		q.Set("u_breakup", Variant.op_Implicit(front.Breakup));
		q.Set("u_seed", Variant.op_Implicit((float)(_seed % 4096)));
		q.Set("u_mode", Variant.op_Implicit(mode));
	}

	public void Advance(float age)
	{
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_0837: Unknown result type (might be due to invalid IL or missing references)
		//IL_0848: Unknown result type (might be due to invalid IL or missing references)
		//IL_084d: Unknown result type (might be due to invalid IL or missing references)
		//IL_089f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0910: Unknown result type (might be due to invalid IL or missing references)
		//IL_0925: Unknown result type (might be due to invalid IL or missing references)
		//IL_0926: Unknown result type (might be due to invalid IL or missing references)
		//IL_0979: Unknown result type (might be due to invalid IL or missing references)
		//IL_097e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0728: Unknown result type (might be due to invalid IL or missing references)
		//IL_0763: Unknown result type (might be due to invalid IL or missing references)
		//IL_0768: Unknown result type (might be due to invalid IL or missing references)
		//IL_077b: Unknown result type (might be due to invalid IL or missing references)
		//IL_078e: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0803: Unknown result type (might be due to invalid IL or missing references)
		if (age < 0.85f)
		{
			_body.Advance(age);
		}
		else
		{
			_body.Dispose();
		}
		_scream.Advance(age);
		while (_hitIndex < _hits.Length && age >= _hits[_hitIndex])
		{
			_shake.Play(40f, 0.5, Random(1000 + _hitIndex) * 360f);
			_hitIndex++;
		}
		if (age < 0.12f || age >= 0.65f)
		{
			((CanvasItem)_copy).Hide();
			((CanvasItem)_refraction.Node).Hide();
			((CanvasItem)_main.Node).Hide();
			EffectQuad[] ridges = _ridges;
			for (int i = 0; i < ridges.Length; i++)
			{
				((CanvasItem)ridges[i].Node).Hide();
			}
			ridges = _fragments;
			for (int i = 0; i < ridges.Length; i++)
			{
				((CanvasItem)ridges[i].Node).Hide();
			}
			ridges = _spooky;
			for (int i = 0; i < ridges.Length; i++)
			{
				((CanvasItem)ridges[i].Node).Hide();
			}
			return;
		}
		((CanvasItem)_copy).Show();
		Front front = Sample(age);
		Band(_main, front, 0f);
		float num = age - 0.12f;
		float num2 = 1f - 0.34392f * num * num - 0.65608f * num * num * num;
		for (int j = 0; j < 2; j++)
		{
			float num3 = ((j == 0) ? 0.64f : 0.34f);
			float num4 = ((j == 0) ? 0.32f : 0.14f) * Smooth(0.132f, 0.155f, age) * (1f - Smooth(0.225f, 0.31f, age));
			float width = 42f * ((j == 0) ? 0.31f : 0.19f);
			Front front2 = new Front(front.Radius * num3, front.Height * num3, width, num4, front.Breakup);
			Band(_ridges[j], front2, 1f);
			_refraction.Set((j == 0) ? "u_ridge0" : "u_ridge1", Variant.op_Implicit(new Vector4(front2.Radius, front2.Height, MathF.Max(14f, front2.Width * 2.7f), num4 * 1.25f * num2)));
		}
		int k;
		for (k = 0; k < 11; k++)
		{
			float num5 = 0.205f + R(0) * 0.105f;
			float num6 = 0.23f + R(1) * 0.11f;
			float num7 = age - num5;
			EffectQuad effectQuad = _fragments[k];
			if (num7 < 0f || num7 >= num6)
			{
				((CanvasItem)effectQuad.Node).Hide();
				continue;
			}
			Front front3 = Sample(num5);
			float num8 = (R(2) * 2f - 1f) * front3.Height * 0.83f;
			float num9 = front3.Radius * MathF.Sqrt(MathF.Max(0f, 1f - MathF.Pow(num8 / front3.Height, 2f)));
			Vector2 val = new Vector2(num9 / (front3.Radius * front3.Radius), num8 / (front3.Height * front3.Height));
			Vector2 val2 = ((Vector2)(ref val)).Normalized();
			float num10 = num7 / num6;
			float num11 = (140f + 330f * R(4)) * num7 - 42f * (0.2f + 0.6f * R(3));
			float num12 = (R(5) * 2f - 1f) * (35f * num7 + 35f * num7 * num7);
			float num13 = (26f + R(6) * 38f) * (1f - 0.36f * num10);
			float height = (3f + R(7) * 7f) * (1f - 0.65f * num10);
			float num14 = ((Vector2)(ref val2)).Angle() + (R(8) * 2f - 1f) * 0.3f;
			Vector2 center = _origin + new Vector2(num9 + val2.X * num11 - val2.Y * num12, 0f - num8 - val2.Y * num11 - val2.X * num12);
			effectQuad.Rect(center, (0f - num13) / 2f, num13 / 2f, height, 0f - num14);
			effectQuad.Set("u_mode", Variant.op_Implicit(2f));
			effectQuad.Set("u_opacity", Variant.op_Implicit((0.22f + 0.26f * R(9)) * Smooth(0f, 0.014f, num7) * MathF.Pow(1f - num10, 1.35f) * (1f - Smooth(0.57f, 0.65f, age))));
		}
		Vector4 zero = Vector4.Zero;
		for (int l = 0; l < 3; l++)
		{
			float num15 = 0.18f + (float)l * 0.06f;
			float num16 = age - num15;
			float num17 = Math.Clamp(num16 / 0.35f, 0f, 1f);
			float num18 = MathF.Min(_distance * 1.7f * num17, front.Radius * 0.82f);
			float num19 = MathF.Min(num18 * Math.Clamp(_height / _distance * 0.85f, 0.1f, 0.13f), front.Height * 0.82f);
			float num20 = (0.9f - 0.15f * (float)l) * Smooth(0f, 0.025f, num16) * (1f - Smooth(0.8f, 1f, num17));
			float num21 = num17 * 0.4f;
			float num22 = ((num21 <= 0.147287f) ? (0.50096f * Smooth(0f, 0.147287f, num21)) : (0.50096f * (1f - Smooth(0.147287f, 0.400517f, num21))));
			float num23 = (float)Hash(_seed, l) / 4.2949673E+09f * ((float)Math.PI * 2f);
			EffectQuad effectQuad2 = _spooky[l];
			bool flag = num16 >= 0f && num16 < 0.35f && num18 > 0.5f && num19 > 0.5f;
			if (flag)
			{
				effectQuad2.Rect(_origin, 0f, num18, num19 * 2f, 0f, 0.5f);
				effectQuad2.Set("lifetime", Variant.op_Implicit(num21));
				effectQuad2.Set("vertex_color", Variant.op_Implicit(new Vector4(1f, 1f, 1f, 0.7f * MathF.Min(0.85f, num20 * num22 * 1.6f))));
				effectQuad2.Set("rotation_rad", Variant.op_Implicit(num23));
				effectQuad2.Set("TIME", Variant.op_Implicit(num16));
			}
			else
			{
				((CanvasItem)effectQuad2.Node).Hide();
			}
			_refraction.Set(l switch
			{
				1 => "u_spooky1", 
				0 => "u_spooky0", 
				_ => "u_spooky2", 
			}, Variant.op_Implicit(flag ? new Vector4(num18, num19, num20 * num22 * 1.6f * 0.7f, num23) : new Vector4(1f, 1f, 0f, 0f)));
			((Vector4)(ref zero))[l] = MathF.Max(0f, num16);
		}
		_refraction.Rect(_origin + new Vector2(front.Radius, 0f), 0f - front.Radius - 2f, 3f, front.Height * 2.12f);
		_refraction.Set("u_span", Variant.op_Implicit(new Vector2(0f - front.Radius - 2f, 3f)));
		_refraction.Set("u_radius", Variant.op_Implicit(front.Radius));
		_refraction.Set("u_height", Variant.op_Implicit(front.Height));
		_refraction.Set("u_opacity", Variant.op_Implicit(front.Opacity * num2));
		_refraction.Set("TIME", Variant.op_Implicit(num));
		_refraction.Set("u_spooky_times", Variant.op_Implicit(zero));
		_refraction.Set("u_wake", Variant.op_Implicit(new Vector4(0.12f, 0.96f, 0.75f * num2 * Smooth(0.14f, 0.21f, age) * (1f - Smooth(0.42f, 0.65f, age)), 0f)));
		float R(int n)
		{
			return Random(k * 11 + n);
		}
	}

	public void CancelBody()
	{
		_body.CancelBody();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_shake.Dispose();
			_body?.Dispose();
			_scream?.Dispose();
			_refraction?.Dispose();
			_main?.Dispose();
			EffectQuad[] ridges = _ridges;
			for (int i = 0; i < ridges.Length; i++)
			{
				ridges[i]?.Dispose();
			}
			ridges = _spooky;
			for (int i = 0; i < ridges.Length; i++)
			{
				ridges[i]?.Dispose();
			}
			ridges = _fragments;
			for (int i = 0; i < ridges.Length; i++)
			{
				ridges[i]?.Dispose();
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_copy))
			{
				((CanvasItem)_copy).Hide();
				((Node)_copy).QueueFree();
			}
		}
	}
}
internal static class BattleScreen
{
	internal static BackBufferCopy Copy(Node parent, string name)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		BackBufferCopy val = new BackBufferCopy
		{
			Name = StringName.op_Implicit(name),
			CopyMode = (CopyModeEnum)2
		};
		parent.AddChild((Node)(object)val, false, (InternalMode)0);
		return val;
	}

	internal static Rect2 Bounds(CanvasItem item)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Transform2D globalTransformWithCanvas = item.GetGlobalTransformWithCanvas();
		return ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * item.GetViewportRect();
	}

	internal static void Cover(EffectQuad quad)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Rect2 val = Bounds((CanvasItem)(object)quad.Node);
		quad.Rect(((Rect2)(ref val)).GetCenter(), (0f - ((Rect2)(ref val)).Size.X) / 2f, ((Rect2)(ref val)).Size.X / 2f, ((Rect2)(ref val)).Size.Y);
	}
}
internal static class BodyDisplay
{
	private static readonly Dictionary<Node2D, List<Node2D>> Layers = new Dictionary<Node2D, List<Node2D>>(ReferenceEqualityComparer.Instance);

	private static readonly Dictionary<Node2D, ClipPlayer> BoundsReaders = new Dictionary<Node2D, ClipPlayer>();

	private static readonly Dictionary<Node2D, LethalityEffect> Residents = new Dictionary<Node2D, LethalityEffect>();

	internal static void AddResident(Node2D body, LethalityEffect effect)
	{
		Residents[body] = effect;
	}

	internal static void RemoveResident(Node2D body, LethalityEffect effect)
	{
		if (Residents.GetValueOrDefault(body) == effect)
		{
			Residents.Remove(body);
		}
	}

	internal static Rect2 ExtendBounds(Node2D body, Rect2 bounds)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!Residents.TryGetValue(body, out LethalityEffect value))
		{
			return bounds;
		}
		return ((Rect2)(ref bounds)).Merge(value.Bounds);
	}

	internal static Transform2D? Bone(Node2D body)
	{
		if (!BoundsReaders.TryGetValue(body, out ClipPlayer value))
		{
			return null;
		}
		return value.BodyUpper;
	}

	internal static float Opacity(Node2D body)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ClipPlayer value;
		return (BoundsReaders.TryGetValue(body, out value) ? value.BodyOpacity : ((CanvasItem)body).Modulate.A) * ((CanvasItem)body).SelfModulate.A;
	}

	internal static void Refresh(Node2D body)
	{
		if (Residents.TryGetValue(body, out LethalityEffect value))
		{
			value.Pose();
		}
	}

	internal static void SetBounds(Node2D body, ClipPlayer player)
	{
		BoundsReaders[body] = player;
	}

	internal static void ClearBounds(Node2D body, ClipPlayer player)
	{
		if (BoundsReaders.GetValueOrDefault(body) == player)
		{
			BoundsReaders.Remove(body);
		}
	}

	internal static Rect2? Bounds(Node2D body)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!BoundsReaders.TryGetValue(body, out ClipPlayer value))
		{
			return null;
		}
		return value.BodyBounds;
	}

	internal static bool OtherBody(Node2D body, ClipPlayer? owner)
	{
		if (BoundsReaders.TryGetValue(body, out ClipPlayer value))
		{
			return value != owner;
		}
		return false;
	}

	internal static ClipPlayer? Owner(Node2D body)
	{
		return BoundsReaders.GetValueOrDefault(body);
	}

	internal static Node2D FrontAnchor(Node2D body)
	{
		Node2D val = body;
		if (Layers.TryGetValue(body, out List<Node2D> value))
		{
			foreach (Node2D item in value)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)item) && !((GodotObject)item).IsQueuedForDeletion() && ((Node)item).GetParent() == ((Node)body).GetParent() && ((Node)item).GetIndex(false) > ((Node)val).GetIndex(false))
				{
					val = item;
				}
			}
		}
		return val;
	}

	internal static void Add(Node2D body, Node2D layer)
	{
		if (!Layers.TryGetValue(body, out List<Node2D> value))
		{
			Layers.Add(body, value = new List<Node2D>());
		}
		value.Add(layer);
	}

	internal static void Remove(Node2D body, Node2D layer)
	{
		if (Layers.TryGetValue(body, out List<Node2D> value))
		{
			value.Remove(layer);
			if (value.Count == 0)
			{
				Layers.Remove(body);
			}
		}
	}

	internal static void Redirect(Node2D body, Rid canvas)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!Layers.TryGetValue(body, out List<Node2D> value))
		{
			return;
		}
		foreach (Node2D item in value)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item) && !((GodotObject)item).IsQueuedForDeletion())
			{
				RenderingServer.CanvasItemSetParent(((CanvasItem)item).GetCanvasItem(), canvas);
				RenderingServer.CanvasItemSetTransform(((CanvasItem)item).GetCanvasItem(), item.GlobalTransform);
			}
		}
	}

	internal static void Restore(Node2D body)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (!Layers.TryGetValue(body, out List<Node2D> value))
		{
			return;
		}
		foreach (Node2D item in value)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item) && !((GodotObject)item).IsQueuedForDeletion())
			{
				Node parent = ((Node)item).GetParent();
				CanvasItem val = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
				if (val != null)
				{
					RenderingServer.CanvasItemSetParent(((CanvasItem)item).GetCanvasItem(), val.GetCanvasItem());
					RenderingServer.CanvasItemSetTransform(((CanvasItem)item).GetCanvasItem(), item.Transform);
				}
			}
		}
	}
}
internal sealed class CaptureSpiritEffect : IVisualEffect, IDisposable
{
	private static Shader[]? _shaders;

	private static readonly Vector2[] Lattice = Enumerable.Range(0, 65).SelectMany((int y) => Enumerable.Range(0, 11).Select((Func<int, Vector2>)((int x) => new Vector2((float)x / 10f, (float)y / 64f)))).ToArray();

	private static readonly int[] Indices = Enumerable.Range(0, 64).SelectMany((int y) => Enumerable.Range(0, 10).SelectMany(delegate(int x)
	{
		int num = y * 11 + x;
		return new int[6]
		{
			num,
			num + 1,
			num + 11,
			num + 1,
			num + 12,
			num + 11
		};
	})).ToArray();

	private static readonly int[][] LayerIndices = new int[2][]
	{
		Indices[..1200],
		Indices[1200..]
	};

	private static readonly string[] Parameters = new string[7] { "u_base", "u_blade", "u_size", "u_pull", "u_time", "u_transport", "u_opacity" };

	private static readonly StringName[] Keys = ((IEnumerable<string>)Parameters).Select((Func<string, StringName>)((string s) => new StringName(s))).ToArray();

	private readonly NCreature _hero;

	private readonly SpineValues? _values;

	private readonly SpiritSnapshot _snapshot;

	private readonly Node2D[] _nodes = (Node2D[])(object)new Node2D[4];

	private readonly ShaderMaterial[] _materials = (ShaderMaterial[])(object)new ShaderMaterial[2];

	private readonly ClipPlayer _body;

	private Vector2 _size;

	private Vector2 _original;

	private Vector2 _sourceCenter;

	private bool _layout;

	private bool _disposed;

	private Rect2 _viewportRect;

	private Transform2D _canvas;

	public float Duration => 1.55f;

	public bool Hold => false;

	internal static void Preload()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if (_shaders != null)
		{
			return;
		}
		string text = File.ReadAllText(Path.Combine(ClipAssets.Directory, "CaptureSpirit.gdshader"));
		Shader[] array = (Shader[])(object)new Shader[2]
		{
			new Shader
			{
				Code = text.Replace("blend_premul_alpha", "blend_add", StringComparison.Ordinal).Replace("COLOR=color;", "COLOR=vec4(color.rgb,1.);", StringComparison.Ordinal)
			},
			new Shader
			{
				Code = text
			}
		};
		Shader[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Array shaderUniformList = array2[i].GetShaderUniformList(false);
			try
			{
				if (shaderUniformList.Count == 0)
				{
					Shader[] array3 = array;
					for (int j = 0; j < array3.Length; j++)
					{
						((GodotObject)array3[j]).Dispose();
					}
					throw new InvalidDataException("Capture Spirit GPU mesh shader");
				}
			}
			finally
			{
				((IDisposable)shaderUniformList)?.Dispose();
			}
		}
		_shaders = array;
	}

	internal CaptureSpiritEffect(NCreature hero, NCreature osty, NCreature enemy, NCombatRoom room)
	{
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Expected O, but got Unknown
		//IL_014b: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_01af: Expected O, but got Unknown
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_040c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Preload();
			_hero = hero;
			if (SpineCompatibility.Allows(hero.Body, "necrobinder"))
			{
				_values = new SpineValues(hero.Body);
			}
			_snapshot = SpiritSnapshot.Rent(enemy.Body, room) ?? throw new InvalidOperationException("Capture Spirit pool is full");
			_body = new ClipPlayer(ClipAssets.Get("CaptureSpirit"), hero, osty, (Node)(object)room.CombatVfxContainer);
			Node node = ((Node)room).GetNode(NodePath.op_Implicit("CombatSceneContainer"));
			NCombatUi node2 = ((Node)room).GetNode<NCombatUi>(NodePath.op_Implicit("CombatUi"));
			CanvasItem[] source = new CanvasItem[3]
			{
				(CanvasItem)node2.PlayContainer,
				(CanvasItem)node2.CardPreviewContainer,
				(CanvasItem)node2.MessyCardPreviewContainer
			};
			CanvasItem val = ((IEnumerable<CanvasItem>)(object)source).OrderBy((CanvasItem n) => ((Node)n).GetIndex(false)).Last();
			int zIndex = ((IEnumerable<CanvasItem>)(object)source).Max((CanvasItem n) => n.ZIndex);
			for (int i = 0; i < 2; i++)
			{
				ShaderMaterial[] materials = _materials;
				int num = i;
				ShaderMaterial val2 = new ShaderMaterial
				{
					Shader = _shaders[i]
				};
				ShaderMaterial val3 = val2;
				materials[num] = val2;
				ShaderMaterial val4 = val3;
				for (int j = 0; j < 2; j++)
				{
					int[] indices = LayerIndices[j];
					Node2D[] nodes = _nodes;
					int num2 = j * 2 + i;
					Node2D val5 = new Node2D
					{
						Name = StringName.op_Implicit((j == 0) ? "NecroCapturedSpiritBack" : "NecroCapturedSpiritFlight"),
						Material = (Material)(object)val4,
						Visible = false
					};
					Node2D val6 = val5;
					nodes[num2] = val5;
					Node2D node3 = val6;
					if (j == 0)
					{
						node.AddChild((Node)(object)node3, false, (InternalMode)0);
						node.MoveChild((Node)(object)node3, Math.Min(node.GetNode(NodePath.op_Implicit("AllyContainer")).GetIndex(false), node.GetNode(NodePath.op_Implicit("EnemyContainer")).GetIndex(false)));
					}
					else
					{
						((Node)val).GetParent().AddChild((Node)(object)node3, false, (InternalMode)0);
						((Node)val).GetParent().MoveChild((Node)(object)node3, ((Node)val).GetIndex(false) + 1 + i);
						((CanvasItem)node3).ZIndex = zIndex;
					}
					node3.GlobalTransform = Transform2D.Identity;
					RenderingServer.CanvasItemSetCustomRect(((CanvasItem)node3).GetCanvasItem(), true, (Rect2?)BattleScreen.Bounds((CanvasItem)(object)node3));
					((CanvasItem)node3).Draw += delegate
					{
						//IL_0006: Unknown result type (might be due to invalid IL or missing references)
						//IL_0028: Unknown result type (might be due to invalid IL or missing references)
						//IL_002d: Unknown result type (might be due to invalid IL or missing references)
						//IL_004d: Unknown result type (might be due to invalid IL or missing references)
						//IL_0053: Unknown result type (might be due to invalid IL or missing references)
						RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)node3).GetCanvasItem(), (ReadOnlySpan<int>)indices, (ReadOnlySpan<Vector2>)Lattice, (ReadOnlySpan<Color>)(Color[]?)(object)new Color[1] { Colors.White }, (ReadOnlySpan<Vector2>)Lattice, ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
					};
				}
				val4.SetShaderParameter(StringName.op_Implicit("u_texture"), Variant.op_Implicit((GodotObject)(object)_snapshot.Texture));
				val4.SetShaderParameter(StringName.op_Implicit("u_mode"), Variant.op_Implicit((i == 0) ? 1f : 0f));
				val4.SetShaderParameter(StringName.op_Implicit("u_texel"), Variant.op_Implicit(Vector2.One / 512f));
				val4.SetShaderParameter(StringName.op_Implicit("u_arch"), Variant.op_Implicit(135f));
				val4.SetShaderParameter(StringName.op_Implicit("u_ceiling"), Variant.op_Implicit(-120f));
				val4.SetShaderParameter(StringName.op_Implicit("u_tipWidth"), Variant.op_Implicit(0.3f));
				val4.SetShaderParameter(StringName.op_Implicit("u_tipLook"), Variant.op_Implicit(new Vector2(0.95f, 0.12f)));
				val4.SetShaderParameter(StringName.op_Implicit("u_emission"), Variant.op_Implicit(0.34f));
				val4.SetShaderParameter(StringName.op_Implicit("u_distortion"), Variant.op_Implicit(0.09f));
				val4.SetShaderParameter(StringName.op_Implicit("u_flow"), Variant.op_Implicit(new Vector4(3f, 0.86f, 45f, 6.8f)));
				val4.SetShaderParameter(StringName.op_Implicit("u_dark"), Variant.op_Implicit(new Vector3(0.035f, 0.255f, 0.25f)));
				val4.SetShaderParameter(StringName.op_Implicit("u_light"), Variant.op_Implicit(new Vector3(0.5f, 0.93f, 0.69f)));
				val4.SetShaderParameter(StringName.op_Implicit("u_streamDark"), Variant.op_Implicit(new Vector3(0.075f, 0.425f, 0.35f)));
				val4.SetShaderParameter(StringName.op_Implicit("u_streamLight"), Variant.op_Implicit(new Vector3(0.63f, 0.97f, 0.78f)));
				val4.SetShaderParameter(StringName.op_Implicit("u_highlight"), Variant.op_Implicit(new Vector3(0.82f, 0.98f, 0.88f)));
				val4.SetShaderParameter(StringName.op_Implicit("u_halo"), Variant.op_Implicit(new Vector3(0.04f, 0.64f, 0.65f)));
			}
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private static float Smooth(float a, float b, float x)
	{
		float num = Math.Clamp((x - a) / (b - a), 0f, 1f);
		return num * num * (3f - 2f * num);
	}

	public void Advance(float age)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		_body.Advance(age);
		if (_snapshot.Ready && !(age < 0.06f))
		{
			if (!_layout)
			{
				Rect2 bounds = _snapshot.Bounds;
				float num = MathF.Min(1f, MathF.Min(360f / (((Rect2)(ref bounds)).Size.X * 0.92f), 420f / ((Rect2)(ref bounds)).Size.Y));
				_size = new Vector2(((Rect2)(ref bounds)).Size.X * 0.92f * num, ((Rect2)(ref bounds)).Size.Y * num);
				_sourceCenter = new Vector2(((Rect2)(ref bounds)).GetCenter().X, 0f - ((Rect2)(ref bounds)).End.Y + ((Rect2)(ref bounds)).Size.Y * (0.5f + 0.16f * (1f - num)));
				_layout = true;
			}
			RefreshScreenBounds();
			SpineValues? values = _values;
			if (values != null && !values.Current)
			{
				throw new InvalidOperationException("Capture Spirit source rig changed");
			}
			Vector2 val = ((_values != null) ? (_hero.Body.GlobalTransform * _values.Matrix("scythe_vfx_attach_2").Origin) : _hero.VfxSpawnPosition);
			val.Y = 0f - val.Y;
			float num2 = 0.78f * Math.Clamp((age - 0.06f) / 1.36f, 0f, 1f) + 0.22f * Smooth(0.06f, 1.42f, age);
			float num3 = 0.76f * Smooth(0.06f, 0.18f, age) * (1f - Smooth(1.4f, 1.55f, age));
			Vector2 val2 = _original + new Vector2(-26f, 24f) * num2;
			Vector2 val3 = ((Vector2)(ref val2)).Lerp(val, 0.09f * num2);
			for (int i = 0; i < 2; i++)
			{
				ShaderMaterial obj = _materials[i];
				obj.SetShaderParameter(Keys[0], Variant.op_Implicit(val3));
				obj.SetShaderParameter(Keys[1], Variant.op_Implicit(val));
				obj.SetShaderParameter(Keys[2], Variant.op_Implicit(_size));
				obj.SetShaderParameter(Keys[3], Variant.op_Implicit(Smooth(0.06f, 0.32f, age)));
				obj.SetShaderParameter(Keys[4], Variant.op_Implicit(age * 1.6f));
				obj.SetShaderParameter(Keys[5], Variant.op_Implicit(3.48f * num2));
				obj.SetShaderParameter(Keys[6], Variant.op_Implicit(num3 * ((i == 0) ? 0.36f : 1f)));
				((CanvasItem)_nodes[i]).Show();
				((CanvasItem)_nodes[i + 2]).Show();
			}
		}
	}

	private void RefreshScreenBounds()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		Node2D[] nodes = _nodes;
		foreach (Node2D val in nodes)
		{
			if (val.GlobalTransform != Transform2D.Identity)
			{
				val.GlobalTransform = Transform2D.Identity;
			}
		}
		Rect2 viewportRect = ((CanvasItem)_nodes[0]).GetViewportRect();
		Transform2D globalTransformWithCanvas = ((CanvasItem)_nodes[0]).GetGlobalTransformWithCanvas();
		if (!(viewportRect == _viewportRect) || !(globalTransformWithCanvas == _canvas))
		{
			_viewportRect = viewportRect;
			_canvas = globalTransformWithCanvas;
			Rect2 val2 = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * viewportRect;
			float num = ((Rect2)(ref val2)).Position.X + 70f + _size.X / 2f;
			float num2 = ((Rect2)(ref val2)).End.X - 70f - _size.X / 2f;
			float num3 = 0f - ((Rect2)(ref val2)).End.Y + 150f + _size.Y / 2f;
			float num4 = 0f - ((Rect2)(ref val2)).Position.Y - 120f - _size.Y / 2f;
			_original = new Vector2((num <= num2) ? Math.Clamp(_sourceCenter.X, num, num2) : ((Rect2)(ref val2)).GetCenter().X, (num3 <= num4) ? Math.Clamp(_sourceCenter.Y, num3, num4) : (0f - ((Rect2)(ref val2)).GetCenter().Y));
			nodes = _nodes;
			foreach (Node2D val3 in nodes)
			{
				RenderingServer.CanvasItemSetCustomRect(((CanvasItem)val3).GetCanvasItem(), true, (Rect2?)BattleScreen.Bounds((CanvasItem)(object)val3));
			}
		}
	}

	public void CancelBody()
	{
		_body.CancelBody();
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		_body?.Dispose();
		_snapshot?.Release();
		_values?.Dispose();
		Node2D[] nodes = _nodes;
		foreach (Node2D val in nodes)
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((CanvasItem)val).Hide();
				((CanvasItem)val).Material = null;
				((Node)val).QueueFree();
			}
		}
		ShaderMaterial[] materials = _materials;
		foreach (ShaderMaterial obj in materials)
		{
			if (obj != null)
			{
				((GodotObject)obj).Dispose();
			}
		}
	}
}
internal sealed class ClipAssets
{
	internal sealed class TextureSpec
	{
		public int Id { get; set; }

		public string? File { get; set; }

		public int Width { get; set; }

		public int Height { get; set; }

		public bool Screen { get; set; }
	}

	internal sealed class ShaderSpec
	{
		public int Id { get; set; }

		public string Source { get; set; } = "";

		public string? GodotSource { get; set; }

		public bool Reveal { get; set; }

		public string Blend { get; set; } = "mix";

		public Dictionary<string, string> Uniforms { get; set; } = new Dictionary<string, string>();
	}

	internal sealed class DrawSpec
	{
		internal SurfaceSpec? Binding;

		internal DrawSpec? SourceMesh;

		internal float[] Uvs = Array.Empty<float>();

		internal Uniform[] Compiled = Array.Empty<Uniform>();

		internal UniformPlan? Parameters;

		internal DrawSpec? Next;

		internal bool HiddenAfterForm;

		public string Tag { get; set; } = "";

		public int Target { get; set; } = -1;

		public int Shader { get; set; }

		public float[] Vertices { get; set; } = Array.Empty<float>();

		public int[] Indices { get; set; } = Array.Empty<int>();

		public Dictionary<string, JsonElement> Uniforms { get; set; } = new Dictionary<string, JsonElement>();

		public float[]? Colors { get; set; }

		public float[] Positions { get; set; } = Array.Empty<float>();

		public int Layout { get; set; } = -1;

		public int ColorSet { get; set; } = -1;

		public string? Track { get; set; }

		public int Surface { get; set; } = -1;

		public int SurfaceSource { get; set; } = -1;

		public int NativeMesh { get; set; } = -1;

		public int NativeCorrection { get; set; } = -1;

		public float NativeLoopWeight { get; set; }

		public float[] NativeParameters { get; set; } = Array.Empty<float>();

		public double[] RevealField { get; set; } = Array.Empty<double>();

		public int RevealFieldId { get; set; } = -1;

		internal int Count
		{
			get
			{
				if (NativeMesh < 0)
				{
					SurfaceSpec binding = Binding;
					if (binding == null)
					{
						return Positions.Length / 2;
					}
					return binding.Ids.Length / 3;
				}
				return Uvs.Length / 2;
			}
		}
	}

	internal sealed class LayoutSpec
	{
		public float[] Uvs { get; set; } = Array.Empty<float>();

		public int[] Indices { get; set; } = Array.Empty<int>();
	}

	internal sealed class SurfaceSpec
	{
		public int[] Ids { get; set; } = Array.Empty<int>();

		public double[] Weights { get; set; } = Array.Empty<double>();
	}

	internal sealed class FrameSequence
	{
		internal Vector4[] Compiled = Array.Empty<Vector4>();

		internal static readonly StringName First = StringName.op_Implicit("p_u_frameA");

		internal static readonly StringName Second = StringName.op_Implicit("p_u_frameB");

		internal static readonly StringName Mix = StringName.op_Implicit("p_u_mix");

		public int Shader { get; set; }

		public float[] Times { get; set; } = Array.Empty<float>();

		public float[][] Rects { get; set; } = Array.Empty<float[]>();

		internal (Vector4 First, Vector4 Second, float Mix) Sample(float time)
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			int num = Array.BinarySearch(Times, time);
			if (num < 0)
			{
				num = ~num - 1;
			}
			num = Math.Clamp(num, 0, Times.Length - 1);
			int num2 = Math.Min(num + 1, Times.Length - 1);
			float item = ((num2 == num) ? 0f : Math.Clamp((time - Times[num]) / (Times[num2] - Times[num]), 0f, 1f));
			return (First: Compiled[num], Second: Compiled[num2], Mix: item);
		}
	}

	internal readonly record struct Uniform(StringName Name, int Kind, Vector4 Value, int Texture, bool Boolean)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Name = ");
			builder.Append(Name);
			builder.Append(", Kind = ");
			builder.Append(Kind.ToString());
			builder.Append(", Value = ");
			Vector4 value = Value;
			builder.Append(((object)(Vector4)(ref value)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Texture = ");
			builder.Append(Texture.ToString());
			builder.Append(", Boolean = ");
			builder.Append(Boolean.ToString());
			return true;
		}
	}

	internal sealed class UniformPlan
	{
		internal Uniform[] Constants = Array.Empty<Uniform>();

		internal int[] Animated = Array.Empty<int>();
	}

	internal sealed class Recipe
	{
		public int Format { get; set; }

		public string Name { get; set; } = "";

		public int Hz { get; set; }

		public bool Body { get; set; }

		public float BodyEnd { get; set; }

		public bool Hold { get; set; }

		public string? Bone { get; set; }

		public TextureSpec[] Textures { get; set; } = Array.Empty<TextureSpec>();

		public ShaderSpec[] Shaders { get; set; } = Array.Empty<ShaderSpec>();

		public DrawSpec[][] Frames { get; set; } = Array.Empty<DrawSpec[]>();

		public string[] Pending { get; set; } = Array.Empty<string>();

		public JsonElement Extra { get; set; }

		public float[] Mouth { get; set; } = Array.Empty<float>();

		public string BodyActor { get; set; } = "hero";

		public float[][] BodyUpper { get; set; } = Array.Empty<float[]>();

		public bool Reaper { get; set; }

		public LayoutSpec[] Layouts { get; set; } = Array.Empty<LayoutSpec>();

		public float[][] ColorSets { get; set; } = Array.Empty<float[]>();

		public FrameSequence[] FrameSequences { get; set; } = Array.Empty<FrameSequence>();

		public SurfaceSpec[] Surfaces { get; set; } = Array.Empty<SurfaceSpec>();

		public NativeReaperGeometry.Recipe? Native { get; set; }
	}

	internal readonly Recipe Data;

	internal readonly Texture2D?[] Textures;

	internal readonly Shader[] Shaders;

	internal readonly FrameSequence?[] Sequences;

	internal readonly NativeReaperGeometry.Shared? NativeData;

	internal readonly int NativeBodyUpperBone = -1;

	private static readonly Dictionary<string, ClipAssets> Cache = new Dictionary<string, ClipAssets>();

	private static readonly Dictionary<string, ImageTexture> SharedTextures = new Dictionary<string, ImageTexture>();

	private static readonly Dictionary<string, Shader> SharedShaders = new Dictionary<string, Shader>();

	internal static string Directory => Path.Combine(Path.GetDirectoryName(typeof(ModEntry).Assembly.Location), "resources");

	internal static string Version { get; set; } = "stable";

	internal static ImageTexture Texture(string path)
	{
		if (SharedTextures.TryGetValue(path, out ImageTexture value))
		{
			return value;
		}
		Image val = Image.LoadFromFile(Path.Combine(Directory, "textures", path));
		try
		{
			if (val.IsEmpty())
			{
				throw new InvalidDataException(path);
			}
			value = ImageTexture.CreateFromImage(val);
			SharedTextures.Add(path, value);
			return value;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	internal static ImageTexture BlueTexture(string path)
	{
		return Texture(Path.GetFileNameWithoutExtension(path) + "-blue.png");
	}

	internal static bool Exists(string card)
	{
		return File.Exists(Path.Combine(Directory, card + "-" + Version + ".json.br"));
	}

	internal static ClipAssets Get(string card)
	{
		string text = card + "-" + Version;
		if (Cache.TryGetValue(text, out ClipAssets value))
		{
			return value;
		}
		value = new ClipAssets(text);
		Cache.Add(text, value);
		return value;
	}

	internal static void Preload()
	{
		ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
		string text = ((releaseInfo != null) ? releaseInfo.Version : null);
		bool flag = ((text == "v0.111.0" || text == "0.111.0") ? true : false);
		Version = (flag ? "beta" : "stable");
		string[] cards = Hooks.Cards;
		foreach (string card in cards)
		{
			if (AnimationSettings.Current.Allows(card) && Exists(card))
			{
				try
				{
					Get(card);
				}
				catch (Exception error)
				{
					ModEntry.Log(error);
				}
			}
		}
		try
		{
			DeathbringerEffect.Preload();
		}
		catch (Exception error2)
		{
			ModEntry.Log(error2);
		}
		if (Exists("BansheePrograms"))
		{
			try
			{
				Get("BansheePrograms");
			}
			catch (Exception error3)
			{
				ModEntry.Log(error3);
			}
		}
		if (AnimationSettings.Current.Allows("BansheesCry"))
		{
			try
			{
				NativeBansheeScream.Preload();
			}
			catch (Exception error4)
			{
				ModEntry.Log(error4);
			}
		}
		if (File.Exists(Path.Combine(Directory, "CaptureSpirit.gdshader")))
		{
			try
			{
				CaptureSpiritEffect.Preload();
			}
			catch (Exception error5)
			{
				ModEntry.Log(error5);
			}
		}
		cards = new string[8] { "OstyBody", "OstyBodyStrong", "OstyHits", "OstyPrograms", "LethalityPrograms", "EndPrograms", "EndImpact", "EndAnticipation" };
		foreach (string card2 in cards)
		{
			if (Exists(card2))
			{
				try
				{
					Get(card2);
				}
				catch (Exception error6)
				{
					ModEntry.Log(error6);
				}
			}
		}
		cards = new string[2] { "osty-flame-basic.png", "osty-flame-advanced.png" };
		foreach (string text2 in cards)
		{
			if (File.Exists(Path.Combine(Directory, "textures", text2)))
			{
				try
				{
					Texture(text2);
				}
				catch (Exception error7)
				{
					ModEntry.Log(error7);
				}
			}
		}
		if (AnimationSettings.Current.ReaperFormEnabled)
		{
			try
			{
				foreach (string assetName in ReaperFormEffect.AssetNames)
				{
					if (Exists(assetName))
					{
						Get(assetName);
					}
				}
				ReaperRavens.Preload();
				Get("ReaperLethalityPrograms");
			}
			catch (Exception error8)
			{
				ModEntry.Log(error8);
			}
		}
		if (!Exists("OstyHits"))
		{
			return;
		}
		try
		{
			ClipAssets clipAssets = Get("OstyHits");
			cards = new string[2] { "blunt", "spark" };
			foreach (string propertyName in cards)
			{
				foreach (JsonElement item in clipAssets.Data.Extra.GetProperty("modes").GetProperty(propertyName).GetProperty("textures")
					.EnumerateArray())
				{
					BlueTexture(clipAssets.Data.Textures[item.GetInt32()].File);
				}
			}
		}
		catch (Exception error9)
		{
			ModEntry.Log(error9);
		}
	}

	private ClipAssets(string name)
	{
		using FileStream stream = File.OpenRead(Path.Combine(Directory, name + ".json.br"));
		using BrotliStream utf8Json = new BrotliStream(stream, CompressionMode.Decompress);
		Data = JsonSerializer.Deserialize<Recipe>(utf8Json, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		}) ?? throw new InvalidDataException(name);
		int format = Data.Format;
		bool flag = (uint)(format - 1) <= 1u;
		bool flag2 = !flag;
		if (!flag2)
		{
			int hz = Data.Hz;
			bool flag3 = ((hz == 30 || hz == 60) ? true : false);
			flag2 = !flag3;
		}
		if (flag2 || Data.Frames.Length == 0 || (Data.Format == 2 && (!Data.Reaper || Data.Native == null)))
		{
			throw new InvalidDataException("Unsupported visual clip " + name);
		}
		if (Data.Native != null)
		{
			NativeData = NativeReaperGeometry.Bind(Data);
			NativeBodyUpperBone = Array.IndexOf(Data.Native.Bones, "body_upper");
		}
		SurfaceGeometry.Bind(Data);
		Textures = (Texture2D?[])(object)new Texture2D[Data.Textures.Length];
		TextureSpec[] textures = Data.Textures;
		foreach (TextureSpec textureSpec in textures)
		{
			string file = textureSpec.File;
			if (file != null)
			{
				Textures[textureSpec.Id] = (Texture2D?)(object)Texture(file);
			}
		}
		Shaders = ((IEnumerable<ShaderSpec>)Data.Shaders).Select((Func<ShaderSpec, Shader>)delegate(ShaderSpec s)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Expected O, but got Unknown
			string text = s.GodotSource ?? ConvertShader(s);
			if (SharedShaders.TryGetValue(text, out Shader value))
			{
				return value;
			}
			Shader val = new Shader
			{
				Code = text
			};
			Array shaderUniformList = val.GetShaderUniformList(false);
			try
			{
				if (shaderUniformList.Count == 0 && s.Uniforms.Count > 0)
				{
					((GodotObject)val).Dispose();
					throw new InvalidDataException("Visual shader rejected: " + name + "/" + s.Id);
				}
				SharedShaders.Add(text, val);
				return val;
			}
			finally
			{
				((IDisposable)shaderUniformList)?.Dispose();
			}
		}).ToArray();
		Sequences = new FrameSequence[Shaders.Length];
		FrameSequence[] frameSequences = Data.FrameSequences;
		foreach (FrameSequence frameSequence in frameSequences)
		{
			if (frameSequence.Shader < 0 || frameSequence.Shader >= Shaders.Length || frameSequence.Times.Length < 2 || frameSequence.Times.Length != frameSequence.Rects.Length || frameSequence.Rects.Any((float[] r) => r.Length != 4) || frameSequence.Times.Zip(frameSequence.Times.Skip(1)).Any(((float First, float Second) p) => p.First >= p.Second))
			{
				throw new InvalidDataException("Invalid atlas frame sequence " + name);
			}
			frameSequence.Compiled = ((IEnumerable<float[]>)frameSequence.Rects).Select((Func<float[], Vector4>)((float[] r) => new Vector4(r[0], r[1], r[2], r[3]))).ToArray();
			frameSequence.Rects = Array.Empty<float[]>();
			Sequences[frameSequence.Shader] = frameSequence;
		}
		Dictionary<string, StringName> names = new Dictionary<string, StringName>();
		DrawSpec[][] frames = Data.Frames;
		foreach (DrawSpec[] array in frames)
		{
			foreach (DrawSpec drawSpec in array)
			{
				if (drawSpec.Layout >= 0)
				{
					LayoutSpec layoutSpec = Data.Layouts[drawSpec.Layout];
					drawSpec.Uvs = layoutSpec.Uvs;
					drawSpec.Indices = layoutSpec.Indices;
				}
				else
				{
					int num = drawSpec.Vertices.Length / 4;
					drawSpec.Positions = new float[num * 2];
					drawSpec.Uvs = new float[num * 2];
					for (int i = 0; i < num; i++)
					{
						drawSpec.Positions[i * 2] = drawSpec.Vertices[i * 4];
						drawSpec.Positions[i * 2 + 1] = drawSpec.Vertices[i * 4 + 1];
						drawSpec.Uvs[i * 2] = drawSpec.Vertices[i * 4 + 2];
						drawSpec.Uvs[i * 2 + 1] = drawSpec.Vertices[i * 4 + 3];
					}
				}
				drawSpec.Vertices = Array.Empty<float>();
				if (drawSpec.ColorSet >= 0)
				{
					drawSpec.Colors = Data.ColorSets[drawSpec.ColorSet];
				}
				drawSpec.Compiled = drawSpec.Uniforms.Select<KeyValuePair<string, JsonElement>, Uniform>(delegate(KeyValuePair<string, JsonElement> pair)
				{
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_0028: Expected O, but got Unknown
					//IL_0051: Unknown result type (might be due to invalid IL or missing references)
					//IL_0057: Unknown result type (might be due to invalid IL or missing references)
					//IL_0094: Unknown result type (might be due to invalid IL or missing references)
					//IL_009a: Unknown result type (might be due to invalid IL or missing references)
					//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
					//IL_013c: Unknown result type (might be due to invalid IL or missing references)
					if (!names.TryGetValue(pair.Key, out StringName value2))
					{
						value2 = new StringName(Parameter(pair.Key));
						names.Add(pair.Key, value2);
					}
					JsonElement value3 = pair.Value;
					if (value3.ValueKind == JsonValueKind.Object)
					{
						return new Uniform(value2, -1, default(Vector4), value3.GetProperty("texture").GetInt32(), Boolean: false);
					}
					JsonValueKind valueKind = value3.ValueKind;
					if (valueKind - 5 <= JsonValueKind.Object)
					{
						return new Uniform(value2, 0, default(Vector4), -1, value3.GetBoolean());
					}
					if (value3.ValueKind == JsonValueKind.Number)
					{
						return new Uniform(value2, 1, new Vector4(value3.GetSingle(), 0f, 0f, 0f), -1, Boolean: false);
					}
					int arrayLength = value3.GetArrayLength();
					return new Uniform(value2, arrayLength, new Vector4(value3[0].GetSingle(), value3[1].GetSingle(), (arrayLength > 2) ? value3[2].GetSingle() : 0f, (arrayLength > 3) ? value3[3].GetSingle() : 0f), -1, Boolean: false);
				}).ToArray();
				drawSpec.HiddenAfterForm = Data.Reaper && (Data.Shaders[drawSpec.Shader].Uniforms.ContainsKey("u_revealOpacity") || (Data.Shaders[drawSpec.Shader].Reveal && drawSpec.Compiled.Any((Uniform u) => u.Name == StringName.op_Implicit("p_u_mode") && u.Value.X == 2f)));
				drawSpec.Uniforms = new Dictionary<string, JsonElement>();
			}
		}
		for (int j = 0; j < Data.Frames.Length - 1; j++)
		{
			Dictionary<string, Queue<DrawSpec>> dictionary = Data.Frames[j + 1].GroupBy(Track).ToDictionary((IGrouping<string, DrawSpec> g) => g.Key, (IGrouping<string, DrawSpec> g) => new Queue<DrawSpec>(g));
			DrawSpec[] array = Data.Frames[j];
			foreach (DrawSpec drawSpec2 in array)
			{
				if (dictionary.TryGetValue(Track(drawSpec2), out var value4) && value4.Count > 0)
				{
					drawSpec2.Next = value4.Dequeue();
				}
			}
			static string Track(DrawSpec d)
			{
				return $"{d.Track}/{d.Tag}/{d.Target}/{d.Shader}/{d.Layout}/{d.Count}/{d.Indices.Length}/" + string.Join(',', from u in d.Compiled
					where u.Kind == -1
					select u.Texture) + "/" + string.Join(',', from u in d.Compiled
					where u.Name == StringName.op_Implicit("p_u_mode")
					select u.Value.X);
			}
		}
		if (Data.Reaper)
		{
			CompileUniformPlans();
		}
	}

	private void CompileUniformPlans()
	{
		foreach (IGrouping<(string, int), DrawSpec> item in from d in Data.Frames.SelectMany((DrawSpec[] frame) => frame)
			where d.Track != null
			group d by (Track: d.Track, Shader: d.Shader))
		{
			Uniform[] compiled = item.First().Compiled;
			bool[] animated = new bool[compiled.Length];
			bool flag = true;
			foreach (DrawSpec item2 in item)
			{
				if (item2.Compiled.Length != compiled.Length)
				{
					flag = false;
					break;
				}
				for (int j = 0; j < compiled.Length; j++)
				{
					Uniform uniform = item2.Compiled[j];
					if (uniform.Name != compiled[j].Name || uniform.Kind != compiled[j].Kind)
					{
						flag = false;
						break;
					}
					if (uniform != compiled[j])
					{
						animated[j] = true;
					}
				}
				if (!flag)
				{
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			if (Sequences[item.Key.Item2] != null)
			{
				for (int k = 0; k < compiled.Length; k++)
				{
					if (compiled[k].Name == FrameSequence.First || compiled[k].Name == FrameSequence.Second || compiled[k].Name == FrameSequence.Mix)
					{
						animated[k] = true;
					}
				}
			}
			UniformPlan parameters = new UniformPlan
			{
				Constants = compiled.Where((Uniform _, int i) => !animated[i]).ToArray(),
				Animated = (from i in Enumerable.Range(0, compiled.Length)
					where animated[i]
					select i).ToArray()
			};
			foreach (DrawSpec item3 in item)
			{
				item3.Parameters = parameters;
			}
		}
	}

	internal static string Parameter(string name)
	{
		return name switch
		{
			"TIME" => "effect_time", 
			"TEXTURE" => "source_texture", 
			"UV" => "source_uv", 
			_ => "p_" + name, 
		};
	}

	internal static string ConvertShader(ShaderSpec shader)
	{
		string input = Regex.Replace(shader.Source, "precision\\s+\\w+\\s+\\w+\\s*;", "");
		string[] array = new string[3] { "PI", "TAU", "E" };
		foreach (string text in array)
		{
			if (Regex.IsMatch(input, "const\\s+float\\s+" + text + "\\b"))
			{
				input = Regex.Replace(input, "\\b" + text + "\\b", "source_" + text);
			}
		}
		input = Regex.Replace(input, "varying\\s+vec2\\s+UV\\s*;", "");
		input = Regex.Replace(input, "varying\\s+vec4\\s+COLOR\\s*;", "");
		input = Regex.Replace(input, "vec4\\s+COLOR\\s*=\\s*vec4\\(1\\.0\\)\\s*;", "");
		input = Regex.Replace(input, "\\bCOLOR\\b", "original_tint");
		input = Regex.Replace(input, "uniform\\s+(\\w+)\\s+([^;]+);", (Match m) => string.Join("\n", from v in m.Groups[2].Value.Split(',')
			select "uniform " + m.Groups[1].Value + " " + v.Trim() + ";"));
		foreach (string item in shader.Uniforms.Keys.OrderByDescending((string x) => x.Length))
		{
			input = Regex.Replace(input, "\\b" + Regex.Escape(item) + "\\b", Parameter(item));
		}
		input = Regex.Replace(input, "\\bUV\\b", "source_uv");
		input = Regex.Replace(input, "\\btexture2D\\b", "texture");
		input = input.Replace("gl_FragColor", "out_color", StringComparison.Ordinal);
		input = Regex.Replace(input, "void\\s+main\\s*\\(\\s*\\)", "void source_fragment(inout vec4 out_color)");
		input = Regex.Replace(input, "(void source_fragment\\([^)]*\\)\\s*\\{)", "$1\nvec4 original_tint=vec4(1.0);");
		if (input.Contains("gl_FragCoord", StringComparison.Ordinal))
		{
			input = input.Replace("void source_fragment(inout vec4 out_color)", "void fragment()", StringComparison.Ordinal).Replace("out_color", "COLOR", StringComparison.Ordinal);
			input = input.Replace("gl_FragCoord.xy/p_u_resolution", "SCREEN_UV", StringComparison.Ordinal).Replace("offset.y = -offset.y;", "", StringComparison.Ordinal);
			input = input.Replace("gl_FragCoord.xy/p_u_screen_size", "SCREEN_UV", StringComparison.Ordinal).Replace("vec2(direction.x,-direction.y)", "direction", StringComparison.Ordinal);
			input = input.Replace("uniform sampler2D p_u_screen;", "uniform sampler2D p_u_screen : hint_screen_texture, repeat_disable, filter_linear;", StringComparison.Ordinal);
			input = input.Replace("uniform sampler2D p_screen_texture;", "uniform sampler2D p_screen_texture : hint_screen_texture, repeat_disable, filter_linear;", StringComparison.Ordinal);
			input = input.Replace("uniform sampler2D source_texture;", "uniform sampler2D source_texture : repeat_enable, filter_linear;", StringComparison.Ordinal);
			return "shader_type canvas_item;\nrender_mode unshaded, blend_disabled;\nvarying vec2 source_uv;\n" + input + "\nvoid vertex(){source_uv=UV;}";
		}
		string text2;
		switch (shader.Blend)
		{
		case "premul_alpha":
			text2 = "blend_premul_alpha";
			break;
		case "add":
		case "add_premul":
			text2 = "blend_add";
			break;
		case "disabled":
			text2 = "blend_disabled";
			break;
		default:
			text2 = "blend_mix";
			break;
		}
		string text3 = text2;
		string text4 = ((shader.Blend == "add_premul") ? "color.a=1.0;" : "");
		return "shader_type canvas_item;\nrender_mode unshaded, " + text3 + ";\nvarying vec4 mesh_tint;\nvarying vec2 source_uv;\n" + input + "\nvoid vertex(){mesh_tint=COLOR;source_uv=UV;}\nvoid fragment(){vec4 color=vec4(0.0);source_fragment(color);" + text4 + "COLOR=color*mesh_tint;}";
	}
}
internal sealed class ClipPlayer : IVisualEffect, IDisposable
{
	private sealed class DrawNode
	{
		internal readonly MeshInstance2D Node = new MeshInstance2D
		{
			TextureFilter = (TextureFilterEnum)2
		};

		internal ShaderMaterial Material = new ShaderMaterial();

		internal Vector2[] Points = Array.Empty<Vector2>();

		internal Vector2[] Uvs = Array.Empty<Vector2>();

		internal int[] Indices = Array.Empty<int>();

		internal float[] SurfaceScratch = Array.Empty<float>();

		internal Color[] Colors = (Color[])(object)new Color[1] { Colors.White };

		internal int Shader = -1;

		internal string Tag = "";

		internal int Target = -1;

		internal readonly Dictionary<StringName, Vector4> Values = new Dictionary<StringName, Vector4>();

		internal readonly Dictionary<StringName, int> Textures = new Dictionary<StringName, int>();

		internal ClipAssets.UniformPlan? Parameters;

		internal Color? Tint;

		internal float[]? FirstUvs;

		internal float[]? SecondUvs;

		internal float[]? FirstColors;

		internal float[]? SecondColors;

		internal float UvFraction;

		internal float ColorFraction;

		internal bool StaticColors;

		private ArrayMesh? _mesh;

		private int _meshVertices;

		private int[]? _meshIndices;

		private ushort[] _indexBytes = Array.Empty<ushort>();

		internal DrawNode()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			((CanvasItem)Node).Material = (Material)(object)Material;
			((CanvasItem)Node).Draw += Draw;
		}

		private void Draw()
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			if (Node.Mesh == null && Indices.Length != 0)
			{
				RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)Node).GetCanvasItem(), (ReadOnlySpan<int>)Indices, (ReadOnlySpan<Vector2>)Points, (ReadOnlySpan<Color>)Colors, (ReadOnlySpan<Vector2>)Uvs, ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
			}
		}

		internal void Submit(bool persistent, bool uvChanged)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Expected O, but got Unknown
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Expected O, but got Unknown
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			if (!persistent || Indices.Length == 0)
			{
				if (Node.Mesh != null)
				{
					Node.Mesh = null;
				}
				((CanvasItem)Node).QueueRedraw();
				return;
			}
			if (_mesh == null)
			{
				_mesh = new ArrayMesh
				{
					CustomAabb = new Aabb(new Vector3(-4096f, -4096f, -1f), new Vector3(8192f, 8192f, 2f))
				};
			}
			bool flag = Node.Mesh != null;
			if (_meshVertices != Points.Length || _meshIndices == null || _meshIndices.Length != Indices.Length)
			{
				_mesh.ClearSurfaces();
				Array val = new Array();
				try
				{
					val.Resize(13);
					val[0] = Variant.op_Implicit(Points);
					val[4] = Variant.op_Implicit(Uvs);
					val[12] = Variant.op_Implicit(Indices);
					_mesh.AddSurfaceFromArrays((PrimitiveType)3, val, (Array<Array>)null, (Dictionary)null, (ArrayFormat)100663296);
					ArrayFormat val2 = (ArrayFormat)_mesh.SurfaceGetFormat(0);
					if (RenderingServer.MeshSurfaceGetFormatVertexStride(val2, Points.Length) != 8 || RenderingServer.MeshSurfaceGetFormatAttributeStride(val2, Points.Length) != 8)
					{
						throw new InvalidDataException("Unexpected Reaper mesh layout");
					}
					_meshVertices = Points.Length;
					_meshIndices = Indices;
					_indexBytes = new ushort[Indices.Length];
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			else
			{
				_mesh.SurfaceUpdateVertexRegion(0, 0, (ReadOnlySpan<byte>)MemoryMarshal.AsBytes<Vector2>(Points.AsSpan()));
				if (uvChanged || !flag)
				{
					_mesh.SurfaceUpdateAttributeRegion(0, 0, (ReadOnlySpan<byte>)MemoryMarshal.AsBytes<Vector2>(Uvs.AsSpan()));
				}
				if (_meshIndices != Indices)
				{
					for (int i = 0; i < Indices.Length; i++)
					{
						_indexBytes[i] = checked((ushort)Indices[i]);
					}
					RenderingServer.MeshSurfaceUpdateIndexRegion(((Resource)_mesh).GetRid(), 0, 0, (ReadOnlySpan<byte>)MemoryMarshal.AsBytes(_indexBytes.AsSpan()));
					_meshIndices = Indices;
				}
			}
			if (!flag)
			{
				Node.Mesh = (Mesh)(object)_mesh;
			}
		}

		internal void DisposeMesh()
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)Node))
			{
				Node.Mesh = null;
			}
			ArrayMesh? mesh = _mesh;
			if (mesh != null)
			{
				((GodotObject)mesh).Dispose();
			}
		}
	}

	internal ClipAssets Assets;

	private readonly Node2D _hero;

	private readonly Node2D _osty;

	private readonly Node2D _ownedBody;

	private readonly bool _ownsBody;

	internal Func<string, Transform2D?>? BasisOverride;

	internal Func<string, float>? LayerOpacity;

	internal Func<bool>? BodyPermit;

	internal bool PersistentReaperMesh = true;

	internal bool CacheStaticUniforms = true;

	private readonly SpineValues? _heroValues;

	private readonly SpineValues? _ostyValues;

	private readonly bool _heroCompatible;

	private readonly bool _ostyCompatible;

	private readonly Callable _pose;

	private readonly List<DrawNode> _draws = new List<DrawNode>();

	private readonly Dictionary<string, Node2D> _layers = new Dictionary<string, Node2D>();

	private Node2D? _wristSlot;

	private readonly Dictionary<int, SubViewport> _buffers = new Dictionary<int, SubViewport>();

	private Texture2D?[] _textures;

	private readonly Node _lifetime;

	private readonly Node? _layerParent;

	private bool _disposed;

	private bool _bodyOwned;

	private Color _heroColor;

	private float _age;

	private bool _bodyCanceled;

	private float _formPhase = -1f;

	private NativeReaperGeometry? _native;

	private NativeReaperGeometry.Sample? _nativeLeft;

	private NativeReaperGeometry.Sample? _nativeRight;

	private float _requestedPhase;

	internal int DrawCount => _draws.Count;

	internal bool BodyRendered
	{
		get
		{
			if (_bodyOwned && _layers.Any<KeyValuePair<string, Node2D>>((KeyValuePair<string, Node2D> p) => IsBody(p.Key) && ((CanvasItem)p.Value).IsVisibleInTree()))
			{
				return _draws.Any((DrawNode d) => IsBody(d.Tag) && ((CanvasItem)d.Node).IsVisibleInTree());
			}
			return false;
		}
	}

	internal float BodyOpacity => _heroColor.A;

	internal Color DisplayTint => (_bodyOwned ? _heroColor : ((CanvasItem)_ownedBody).Modulate) * ((CanvasItem)_ownedBody).SelfModulate;

	internal Transform2D? BodyUpper
	{
		get
		{
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			if (_bodyOwned && Assets.NativeBodyUpperBone >= 0 && _nativeLeft != null && _nativeRight != null)
			{
				float[] bones = _nativeLeft.Bones;
				float[] bones2 = _nativeRight.Bones;
				int num = Assets.NativeBodyUpperBone * 6;
				float num2 = Math.Clamp(_age * (float)Assets.Data.Hz, 0f, Assets.Data.Frames.Length - 1);
				float num3 = num2 - (float)(int)num2;
				return Basis("body") * new Transform2D(new Vector2(Mathf.Lerp(bones[num], bones2[num], num3), Mathf.Lerp(bones[num + 2], bones2[num + 2], num3)), new Vector2(Mathf.Lerp(bones[num + 1], bones2[num + 1], num3), Mathf.Lerp(bones[num + 3], bones2[num + 3], num3)), new Vector2(Mathf.Lerp(bones[num + 4], bones2[num + 4], num3), Mathf.Lerp(bones[num + 5], bones2[num + 5], num3)));
			}
			float[][] bodyUpper = Assets.Data.BodyUpper;
			if (!_bodyOwned || bodyUpper.Length == 0)
			{
				return null;
			}
			float num4 = Math.Clamp(_age * (float)Assets.Data.Hz, 0f, bodyUpper.Length - 1);
			int num5 = (int)num4;
			float[] array = bodyUpper[num5];
			float[] array2 = bodyUpper[Math.Min(num5 + 1, bodyUpper.Length - 1)];
			float num6 = num4 - (float)num5;
			return Basis("body") * new Transform2D(new Vector2(Mathf.Lerp(array[0], array2[0], num6), Mathf.Lerp(array[1], array2[1], num6)), new Vector2(Mathf.Lerp(array[2], array2[2], num6), Mathf.Lerp(array[3], array2[3], num6)), new Vector2(Mathf.Lerp(array[4], array2[4], num6), Mathf.Lerp(array[5], array2[5], num6)));
		}
	}

	internal Rect2 BodyBounds
	{
		get
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			bool flag = true;
			Rect2 result = default(Rect2);
			foreach (DrawNode draw in _draws)
			{
				if (!IsBody(draw.Tag) || !((CanvasItem)draw.Node).Visible)
				{
					continue;
				}
				Transform2D globalTransform = ((Node2D)draw.Node).GlobalTransform;
				Vector2[] points = draw.Points;
				foreach (Vector2 val in points)
				{
					Vector2 val2 = globalTransform * val;
					if (flag)
					{
						result = new Rect2(val2, Vector2.Zero);
						flag = false;
					}
					else
					{
						result = ((Rect2)(ref result)).Expand(val2);
					}
				}
			}
			if (!flag)
			{
				return result;
			}
			return _ownedBody.GlobalTransform * (Rect2)(((??)((Assets.Data.BodyActor == "osty") ? _ostyValues : _heroValues)?.Bounds) ?? new Rect2(-100f, -400f, 200f, 400f));
		}
	}

	public float Duration => (float)(Assets.Data.Frames.Length - 1) / (float)Assets.Data.Hz;

	public bool Hold => Assets.Data.Hold;

	internal ClipPlayer(ClipAssets assets, NCreature hero, NCreature osty, Node lifetime, bool ownBody = true, Node? layerParent = null)
		: this(assets, hero.Body, osty.Body, lifetime, ownBody, layerParent)
	{
	}

	internal ClipPlayer(ClipAssets assets, Node2D hero, Node2D osty, Node lifetime, bool ownBody = true, Node? layerParent = null)
	{
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		if (ownBody && assets.Data.Body && !assets.Data.Reaper && assets.Data.BodyActor == "hero" && ReaperFormEffect.For(hero) != null)
		{
			string text = assets.Data.Name.Substring(0, assets.Data.Name.LastIndexOf('-'));
			if (ClipAssets.Exists("Reaper_" + text))
			{
				assets = ClipAssets.Get("Reaper_" + text);
			}
		}
		Assets = assets;
		_hero = hero;
		_osty = osty;
		_lifetime = lifetime;
		_ownedBody = ((assets.Data.BodyActor == "osty") ? _osty : _hero);
		_ownsBody = ownBody;
		_layerParent = layerParent;
		_heroCompatible = SpineCompatibility.Allows(_hero, "necrobinder");
		_ostyCompatible = ((assets.Data.BodyActor == "osty") ? SpineCompatibility.AllowsOstyBody(_osty) : SpineCompatibility.Allows(_osty, "osty"));
		if (_heroCompatible)
		{
			_heroValues = new SpineValues(_hero);
		}
		if (_ostyCompatible)
		{
			_ostyValues = new SpineValues(_osty);
		}
		_textures = (Texture2D[])assets.Textures.Clone();
		if (ownBody && assets.Data.Body && assets.Data.BodyActor == "hero" && _heroCompatible)
		{
			ScytheFlames.Ensure(_hero);
		}
		_pose = Callable.From<Variant>((Action<Variant>)delegate
		{
			Pose();
		});
		foreach (Node2D item in ((IEnumerable<Node2D>)(object)new Node2D[2] { _hero, _osty }).Distinct())
		{
			if (((GodotObject)item).HasSignal(StringName.op_Implicit("world_transforms_changed")))
			{
				((GodotObject)item).Connect(StringName.op_Implicit("world_transforms_changed"), _pose, 0u);
			}
		}
		ClipAssets.TextureSpec[] textures = assets.Data.Textures;
		foreach (ClipAssets.TextureSpec textureSpec in textures)
		{
			if (textureSpec.File == null && !textureSpec.Screen)
			{
				SubViewport val = EmissionPool.Rent(_lifetime, textureSpec.Width, textureSpec.Height);
				_buffers.Add(textureSpec.Id, val);
				_textures[textureSpec.Id] = (Texture2D?)(object)((Viewport)val).GetTexture();
			}
		}
		if (ownBody && assets.Data.Body && assets.Data.BodyActor == "hero" && _heroCompatible)
		{
			ReaperFormEffect.For(_hero)?.YieldBody();
		}
		try
		{
			Advance(0f);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	internal void Visible(bool visible)
	{
		foreach (Node2D value in _layers.Values)
		{
			((CanvasItem)value).Visible = visible;
		}
	}

	private static bool IsBody(string tag)
	{
		if (!(tag == "body"))
		{
			return tag.StartsWith("osty-body", StringComparison.Ordinal);
		}
		return true;
	}

	public void CancelBody()
	{
		_bodyCanceled = true;
		RestoreBody();
	}

	internal void YieldBody()
	{
		RestoreBody();
		Visible(visible: false);
	}

	internal void SwitchFormClip(ClipAssets assets)
	{
		if (!Assets.Data.Reaper || !assets.Data.Reaper || _buffers.Count != 0)
		{
			throw new InvalidOperationException("Only prebaked form tracks can be switched in place.");
		}
		Assets = assets;
		_textures = assets.Textures;
		_age = 0f;
		_formPhase = -1f;
		foreach (DrawNode draw in _draws)
		{
			draw.Shader = -1;
			draw.Tint = null;
			draw.Parameters = null;
			draw.Values.Clear();
			draw.Textures.Clear();
		}
	}

	private Node2D Layer(string tag)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		if (_layers.TryGetValue(tag, out Node2D value))
		{
			return value;
		}
		Node2D val = (tag.StartsWith("osty", StringComparison.Ordinal) ? _osty : _hero);
		value = new Node2D
		{
			Name = StringName.op_Implicit("Necro_" + tag),
			TextureFilter = (TextureFilterEnum)2
		};
		Node val2 = _layerParent ?? ((Node)val).GetParent();
		val2.AddChild((Node)(object)value, false, (InternalMode)0);
		if (_layerParent == null && tag != "hero-head")
		{
			val2.MoveChild((Node)(object)value, ((Node)val).GetIndex(false) + ((!tag.EndsWith("back", StringComparison.Ordinal)) ? 1 : 0));
		}
		else
		{
			ActorLayers.Place(value, val2, tag);
		}
		_layers.Add(tag, value);
		if (IsBody(tag))
		{
			BodyDisplay.Add(val, value);
		}
		return value;
	}

	private Transform2D Basis(string tag)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		Transform2D? val = BasisOverride?.Invoke(tag);
		if (val.HasValue)
		{
			return val.GetValueOrDefault();
		}
		if (Assets.Data.Name.StartsWith("Friendship", StringComparison.Ordinal))
		{
			ActorLayers.Display display = ActorLayers.Get(_osty);
			if ((object)display != null)
			{
				return display.Wrist();
			}
		}
		if (Assets.Data.Name.StartsWith("Dirge", StringComparison.Ordinal) && tag == "osty-back")
		{
			ActorLayers.Display display2 = ActorLayers.Get(_osty);
			if ((object)display2 != null)
			{
				return display2.Body();
			}
		}
		if (tag == "hero-head")
		{
			if (_heroValues == null)
			{
				return ActorAnchor.Head(_hero);
			}
			return _hero.GlobalTransform * _heroValues.Matrix("head");
		}
		if (tag.StartsWith("osty-wrist", StringComparison.Ordinal))
		{
			if (_ostyValues == null)
			{
				return ActorAnchor.Ground(_osty) * new Transform2D(0f, new Vector2(0f, -90f));
			}
			return _osty.GlobalTransform * _ostyValues.Matrix(Assets.Data.Bone ?? "wrist");
		}
		if (tag.StartsWith("osty", StringComparison.Ordinal))
		{
			if (!_ostyCompatible)
			{
				return ActorAnchor.Ground(_osty);
			}
			return _osty.GlobalTransform * new Transform2D(0f, Vector2.One / 0.26f, 0f, Vector2.Zero);
		}
		if (!_heroCompatible)
		{
			return ActorAnchor.Ground(_hero);
		}
		return _hero.GlobalTransform * new Transform2D(0f, Vector2.One / 0.27f, 0f, Vector2.Zero);
	}

	private void Pose()
	{
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_hero) && GodotObject.IsInstanceValid((GodotObject)(object)_osty))
		{
			SpineValues? heroValues = _heroValues;
			if (heroValues == null || heroValues.Current)
			{
				SpineValues? ostyValues = _ostyValues;
				if (ostyValues == null || ostyValues.Current)
				{
					Color val5 = default(Color);
					foreach (var (text2, val2) in _layers)
					{
						if (_ostyCompatible && text2.StartsWith("osty-wrist", StringComparison.Ordinal))
						{
							ActorLayers.Display display = ActorLayers.Get(_osty);
							if ((object)display != null)
							{
								ActorLayers.Place(val2, (Node)(object)display.Parent, text2);
							}
							else if (text2 == "osty-wrist-back")
							{
								if (_wristSlot == null)
								{
									Variant val3 = ClassDB.Instantiate(StringName.op_Implicit("SpineSlotNode"));
									try
									{
										_wristSlot = (Node2D)((Variant)(ref val3)).AsGodotObject();
										((Node)_wristSlot).Name = StringName.op_Implicit("NecroFriendshipWrist");
										((Node)_osty).AddChild((Node)(object)_wristSlot, false, (InternalMode)0);
										((GodotObject)_wristSlot).Set(StringName.op_Implicit("slot_name"), Variant.op_Implicit("wrist"));
									}
									finally
									{
										((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
									}
								}
								ActorLayers.Place(val2, (Node)(object)_wristSlot, text2);
							}
							else if (((Node)val2).GetParent() != ((Node)_osty).GetParent())
							{
								ActorLayers.Place(val2, ((Node)_osty).GetParent(), text2);
								((Node)_osty).GetParent().MoveChild((Node)(object)val2, ((Node)_osty).GetIndex(false) + 1);
							}
						}
						val2.GlobalTransform = Basis(text2);
						float num = LayerOpacity?.Invoke(text2) ?? 1f;
						Color val4 = (IsBody(text2) ? ((_bodyOwned ? _heroColor : ((CanvasItem)_ownedBody).Modulate) * ((CanvasItem)_ownedBody).SelfModulate) : Colors.White);
						((CanvasItem)val2).Modulate = (Color)(Assets.Data.Reaper ? Colors.White : new Color(val4.R, val4.G, val4.B, val4.A * num));
						if (!Assets.Data.Reaper)
						{
							continue;
						}
						foreach (DrawNode draw in _draws)
						{
							if (!(draw.Tag == text2) || draw.Shader < 0)
							{
								continue;
							}
							((Color)(ref val5))..ctor(val4.R, val4.G, val4.B, val4.A * num);
							if (Assets.Data.Shaders[draw.Shader].Reveal)
							{
								((CanvasItem)draw.Node).Modulate = Colors.White;
								Color? tint = draw.Tint;
								Color val6 = val5;
								if (!tint.HasValue || tint.GetValueOrDefault() != val6)
								{
									draw.Material.SetShaderParameter(StringName.op_Implicit("reaper_tint"), Variant.op_Implicit(val5));
									draw.Tint = val5;
								}
							}
							else
							{
								((CanvasItem)draw.Node).Modulate = val5;
							}
						}
					}
					BodyDisplay.Refresh(_ownedBody);
					return;
				}
			}
		}
		CancelBody();
		Visible(visible: false);
	}

	public void Advance(float age)
	{
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_065b: Unknown result type (might be due to invalid IL or missing references)
		//IL_067d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0854: Unknown result type (might be due to invalid IL or missing references)
		//IL_0859: Unknown result type (might be due to invalid IL or missing references)
		//IL_0834: Unknown result type (might be due to invalid IL or missing references)
		//IL_0839: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a3e: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		int num2;
		ClipAssets.DrawSpec[] array;
		float num3;
		int num4;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_hero) && GodotObject.IsInstanceValid((GodotObject)(object)_osty))
		{
			SpineValues? heroValues = _heroValues;
			if (heroValues == null || heroValues.Current)
			{
				SpineValues? ostyValues = _ostyValues;
				if (ostyValues == null || ostyValues.Current)
				{
					if (Assets.Data.Hold && age >= Duration && _age >= Duration)
					{
						return;
					}
					_age = age;
					float num = Math.Clamp(age * (float)Assets.Data.Hz, 0f, Assets.Data.Frames.Length - 1);
					num2 = (int)num;
					array = Assets.Data.Frames[num2];
					_ = Assets.Data.Frames[Math.Min(num2 + 1, Assets.Data.Frames.Length - 1)];
					num3 = num - (float)num2;
					if (Assets.Data.Body && !_bodyCanceled && age < Assets.Data.BodyEnd)
					{
						Func<bool>? bodyPermit = BodyPermit;
						if (bodyPermit == null || bodyPermit())
						{
							num4 = (((Assets.Data.BodyActor == "osty") ? _ostyCompatible : _heroCompatible) ? 1 : 0);
							goto IL_0175;
						}
					}
					num4 = 0;
					goto IL_0175;
				}
			}
		}
		throw new InvalidOperationException("Displayed rig changed during visual playback.");
		IL_0175:
		bool flag = (byte)num4 != 0;
		if (_ownsBody && !_bodyOwned && (!((CanvasItem)_ownedBody).IsVisibleInTree() || ((CanvasItem)_ownedBody).Modulate.A <= 0.001f || ((CanvasItem)_ownedBody).SelfModulate.A <= 0.001f))
		{
			flag = false;
		}
		if (flag && _ownsBody && !_bodyOwned)
		{
			_heroColor = ((CanvasItem)_ownedBody).Modulate;
			((CanvasItem)_ownedBody).Modulate = new Color(_heroColor.R, _heroColor.G, _heroColor.B, 0f);
			_bodyOwned = true;
			BodyDisplay.SetBounds(_ownedBody, this);
		}
		if (!flag)
		{
			RestoreBody();
		}
		NativeReaperGeometry.Sample sample = null;
		NativeReaperGeometry.Sample sample2 = null;
		bool flag2 = (ReaperFormEffect.For(_hero)?.Phase ?? _requestedPhase) < 1f;
		if (flag && Assets.Data.Native != null)
		{
			if (_native == null)
			{
				_native = new NativeReaperGeometry(_hero);
			}
			(sample, sample2) = _native.Advance(Assets, num2, flag2);
		}
		_nativeLeft = sample;
		_nativeRight = sample2;
		for (int i = 0; i < array.Length; i++)
		{
			ClipAssets.DrawSpec drawSpec = array[i];
			ClipAssets.DrawSpec drawSpec2 = drawSpec.Next ?? drawSpec;
			if (drawSpec.Shader != drawSpec2.Shader || !(drawSpec.Tag == drawSpec2.Tag) || drawSpec.Target != drawSpec2.Target || drawSpec.Count != drawSpec2.Count || drawSpec.Indices.Length != drawSpec2.Indices.Length)
			{
				drawSpec2 = drawSpec;
			}
			if (i == _draws.Count)
			{
				_draws.Add(new DrawNode());
			}
			DrawNode drawNode = _draws[i];
			Node val = (Node)((drawSpec.Target >= 0) ? ((object)_buffers[drawSpec.Target]) : ((object)Layer(drawSpec.Tag)));
			if (((Node)drawNode.Node).GetParent() != val)
			{
				Node parent = ((Node)drawNode.Node).GetParent();
				if (parent != null)
				{
					parent.RemoveChild((Node)(object)drawNode.Node);
				}
				val.AddChild((Node)(object)drawNode.Node, false, (InternalMode)0);
			}
			drawNode.Tag = drawSpec.Tag;
			drawNode.Target = drawSpec.Target;
			((CanvasItem)drawNode.Node).Visible = (!IsBody(drawSpec.Tag) && !(drawSpec.Tag == "osty-shadow")) || flag;
			if (PersistentReaperMesh && !flag2 && drawSpec.HiddenAfterForm)
			{
				((CanvasItem)drawNode.Node).Visible = false;
			}
			if (!_ostyCompatible && drawSpec.Tag.StartsWith("osty-wrist", StringComparison.Ordinal))
			{
				((CanvasItem)drawNode.Node).Visible = false;
			}
			if (!((CanvasItem)drawNode.Node).Visible)
			{
				continue;
			}
			if (drawNode.Shader != drawSpec.Shader)
			{
				drawNode.Material.Shader = Assets.Shaders[drawSpec.Shader];
				drawNode.Shader = drawSpec.Shader;
				drawNode.Tint = null;
				drawNode.Parameters = null;
				_formPhase = -1f;
				drawNode.Values.Clear();
				drawNode.Textures.Clear();
			}
			int count = drawSpec.Count;
			if (drawNode.Points.Length != count)
			{
				drawNode.Points = (Vector2[])(object)new Vector2[count];
				drawNode.Uvs = (Vector2[])(object)new Vector2[count];
				drawNode.FirstUvs = (drawNode.SecondUvs = null);
			}
			NativeReaperGeometry.Sample sample3 = ((drawSpec == drawSpec2) ? sample : sample2);
			if (drawSpec.Binding != null || drawSpec2.Binding != null)
			{
				SurfaceGeometry.Interpolate(drawSpec, drawSpec2, num3, drawNode.Points, ref drawNode.SurfaceScratch, sample, sample3);
			}
			float[] left = SurfaceGeometry.Positions(drawSpec, sample);
			float[] right = SurfaceGeometry.Positions(drawSpec2, sample3);
			if (drawSpec.Binding == null && drawSpec2.Binding == null)
			{
				MeshMath.Interpolate(left, right, num3, drawNode.Points);
			}
			bool flag3 = drawSpec.Uvs == drawSpec2.Uvs;
			bool flag4 = drawNode.FirstUvs != drawSpec.Uvs || drawNode.SecondUvs != drawSpec2.Uvs || (!flag3 && drawNode.UvFraction != num3);
			if (flag4)
			{
				for (int j = 0; j < count; j++)
				{
					int num5 = j * 2;
					drawNode.Uvs[j] = (flag3 ? new Vector2(drawSpec.Uvs[num5], drawSpec.Uvs[num5 + 1]) : new Vector2(Mathf.Lerp(drawSpec.Uvs[num5], drawSpec2.Uvs[num5], num3), Mathf.Lerp(drawSpec.Uvs[num5 + 1], drawSpec2.Uvs[num5 + 1], num3)));
				}
				drawNode.FirstUvs = drawSpec.Uvs;
				drawNode.SecondUvs = drawSpec2.Uvs;
				drawNode.UvFraction = num3;
			}
			float[] array2 = sample?.Colors.GetValueOrDefault(drawSpec) ?? drawSpec.Colors;
			float[] array3 = sample3?.Colors.GetValueOrDefault(drawSpec2) ?? drawSpec2.Colors;
			if (Assets.Data.Reaper && !flag2 && Assets.Data.Shaders[drawSpec.Shader].Reveal)
			{
				array2 = (array3 = null);
			}
			int num6 = ((array2 == null) ? 1 : count);
			if (drawNode.Colors.Length != num6)
			{
				drawNode.Colors = (Color[])(object)new Color[num6];
				drawNode.StaticColors = false;
			}
			if (array2 != null && (array3 == null || array3.Length != array2.Length))
			{
				array3 = array2;
			}
			bool flag5 = array2 == null || (array2 == drawSpec.Colors && array3 == drawSpec2.Colors);
			if (!flag5 || !drawNode.StaticColors || drawNode.FirstColors != array2 || drawNode.SecondColors != array3 || (array2 != array3 && drawNode.ColorFraction != num3))
			{
				if (array2 != null)
				{
					for (int k = 0; k < count; k++)
					{
						int num7 = k * 4;
						drawNode.Colors[k] = new Color(Mathf.Lerp(array2[num7], array3[num7], num3), Mathf.Lerp(array2[num7 + 1], array3[num7 + 1], num3), Mathf.Lerp(array2[num7 + 2], array3[num7 + 2], num3), Mathf.Lerp(array2[num7 + 3], array3[num7 + 3], num3));
					}
				}
				else
				{
					drawNode.Colors[0] = Colors.White;
				}
				drawNode.FirstColors = array2;
				drawNode.SecondColors = array3;
				drawNode.ColorFraction = num3;
				drawNode.StaticColors = flag5;
			}
			drawNode.Indices = drawSpec.Indices;
			ClipAssets.UniformPlan uniformPlan = (CacheStaticUniforms ? drawSpec.Parameters : null);
			if (uniformPlan != null && drawNode.Parameters != uniformPlan)
			{
				ClipAssets.Uniform[] constants = uniformPlan.Constants;
				foreach (ClipAssets.Uniform uniform in constants)
				{
					Set(drawNode, uniform, uniform, 0f);
				}
			}
			drawNode.Parameters = uniformPlan;
			ClipAssets.FrameSequence frameSequence = Assets.Sequences[drawSpec.Shader];
			(Vector4, Vector4, float) tuple2 = frameSequence?.Sample(age) ?? default((Vector4, Vector4, float));
			int num8 = ((uniformPlan != null) ? uniformPlan.Animated.Length : drawSpec.Compiled.Length);
			for (int m = 0; m < num8; m++)
			{
				int num9 = ((uniformPlan == null) ? m : uniformPlan.Animated[m]);
				ClipAssets.Uniform uniform2 = drawSpec.Compiled[num9];
				ClipAssets.Uniform next = ((num9 < drawSpec2.Compiled.Length && drawSpec2.Compiled[num9].Name == uniform2.Name) ? drawSpec2.Compiled[num9] : uniform2);
				if (frameSequence != null)
				{
					if (uniform2.Name == ClipAssets.FrameSequence.First)
					{
						uniform2 = uniform2 with
						{
							Value = tuple2.Item1
						};
						next = uniform2;
					}
					else if (uniform2.Name == ClipAssets.FrameSequence.Second)
					{
						uniform2 = uniform2 with
						{
							Value = tuple2.Item2
						};
						next = uniform2;
					}
					else if (uniform2.Name == ClipAssets.FrameSequence.Mix)
					{
						uniform2 = uniform2 with
						{
							Value = new Vector4(tuple2.Item3, 0f, 0f, 0f)
						};
						next = uniform2;
					}
				}
				Set(drawNode, uniform2, next, num3);
			}
			drawNode.Submit(PersistentReaperMesh && Assets.Data.Reaper && !flag2 && drawNode.Colors.Length == 1 && drawNode.Colors[0] == Colors.White, flag4);
		}
		for (int n = array.Length; n < _draws.Count; n++)
		{
			((CanvasItem)_draws[n].Node).Visible = false;
		}
		Pose();
		if (Assets.Data.Reaper)
		{
			ReaperFormEffect reaperFormEffect = ReaperFormEffect.For(_hero);
			if (reaperFormEffect != null)
			{
				FormPhase(reaperFormEffect.Phase);
			}
		}
	}

	internal void Uniform(string name, StringName key, Variant value)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		foreach (DrawNode draw in _draws)
		{
			if (draw.Shader >= 0 && Assets.Data.Shaders[draw.Shader].Uniforms.ContainsKey(name))
			{
				draw.Material.SetShaderParameter(key, value);
			}
		}
	}

	internal void FormPhase(float phase)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		_requestedPhase = phase;
		if (_formPhase != phase)
		{
			_formPhase = phase;
			Uniform("u_phase", StringName.op_Implicit("p_u_phase"), Variant.op_Implicit(phase));
			float num = Math.Clamp((phase - 0.24f) / 0.22f, 0f, 1f);
			Uniform("u_revealOpacity", StringName.op_Implicit("p_u_revealOpacity"), Variant.op_Implicit(1f - num * num * (3f - 2f * num)));
		}
	}

	private void Set(DrawNode item, ClipAssets.Uniform value, ClipAssets.Uniform next, float fraction)
	{
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		ShaderMaterial material = item.Material;
		if (value.Kind == -1)
		{
			if (!Assets.Data.Textures[value.Texture].Screen && (!item.Textures.TryGetValue(value.Name, out var value2) || value2 != value.Texture))
			{
				material.SetShaderParameter(value.Name, Variant.op_Implicit((GodotObject)(object)(_textures[value.Texture] ?? throw new InvalidDataException("Missing effect texture"))));
				item.Textures[value.Name] = value.Texture;
			}
			return;
		}
		if (value.Kind == 0)
		{
			Vector4 val = default(Vector4);
			((Vector4)(ref val))..ctor((float)(value.Boolean ? 1 : 0), 0f, 0f, 0f);
			if (!item.Values.TryGetValue(value.Name, out var value3) || !(value3 == val))
			{
				material.SetShaderParameter(value.Name, Variant.op_Implicit(value.Boolean));
				item.Values[value.Name] = val;
			}
			return;
		}
		Vector4 value4 = value.Value;
		Vector4 val2 = ((Vector4)(ref value4)).Lerp(next.Value, fraction);
		if (!item.Values.TryGetValue(value.Name, out var value5) || !(value5 == val2))
		{
			item.Values[value.Name] = val2;
			switch (value.Kind)
			{
			case 1:
				material.SetShaderParameter(value.Name, Variant.op_Implicit(val2.X));
				break;
			case 2:
				material.SetShaderParameter(value.Name, Variant.op_Implicit(new Vector2(val2.X, val2.Y)));
				break;
			case 3:
				material.SetShaderParameter(value.Name, Variant.op_Implicit(new Vector3(val2.X, val2.Y, val2.Z)));
				break;
			case 4:
				material.SetShaderParameter(value.Name, Variant.op_Implicit(val2));
				break;
			}
		}
	}

	private void RestoreBody()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (!_bodyOwned)
		{
			return;
		}
		_bodyOwned = false;
		BodyDisplay.ClearBounds(_ownedBody, this);
		if (GodotObject.IsInstanceValid((GodotObject)(object)_ownedBody))
		{
			if (((CanvasItem)_ownedBody).Modulate == new Color(_heroColor.R, _heroColor.G, _heroColor.B, 0f))
			{
				((CanvasItem)_ownedBody).Modulate = _heroColor;
			}
			ReaperFormEffect.For(_ownedBody)?.ResumeBody(this);
		}
	}

	public void Dispose()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		RestoreBody();
		foreach (Node2D item in ((IEnumerable<Node2D>)(object)new Node2D[2] { _hero, _osty }).Distinct())
		{
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)item) && ((GodotObject)item).HasSignal(StringName.op_Implicit("world_transforms_changed")) && ((GodotObject)item).IsConnected(StringName.op_Implicit("world_transforms_changed"), _pose))
				{
					((GodotObject)item).Disconnect(StringName.op_Implicit("world_transforms_changed"), _pose);
				}
			}
			catch (Exception error)
			{
				ModEntry.Log(error);
			}
		}
		foreach (var (tag, val2) in _layers)
		{
			if (IsBody(tag))
			{
				BodyDisplay.Remove(_ownedBody, val2);
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				((CanvasItem)val2).Hide();
				((Node)val2).QueueFree();
			}
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_wristSlot))
		{
			((Node)_wristSlot).QueueFree();
		}
		foreach (DrawNode draw in _draws)
		{
			draw.DisposeMesh();
			if (GodotObject.IsInstanceValid((GodotObject)(object)draw.Node))
			{
				((CanvasItem)draw.Node).Material = null;
			}
			((GodotObject)draw.Material).Dispose();
		}
		foreach (DrawNode draw2 in _draws)
		{
			if (draw2.Target >= 0 && GodotObject.IsInstanceValid((GodotObject)(object)draw2.Node))
			{
				Node parent = ((Node)draw2.Node).GetParent();
				if (parent != null)
				{
					parent.RemoveChild((Node)(object)draw2.Node);
				}
				((Node)draw2.Node).QueueFree();
			}
		}
		foreach (SubViewport value in _buffers.Values)
		{
			EmissionPool.Return(value);
		}
		_heroValues?.Dispose();
		_ostyValues?.Dispose();
		_native?.Dispose();
	}
}
internal sealed class ColorBatch : IDisposable
{
	private readonly Node2D _node;

	private readonly Vector2[] _points;

	private readonly Color[] _colors;

	private static readonly int[] Indices = Enumerable.Range(0, 4096).ToArray();

	private int _count;

	internal Node2D Node => _node;

	internal ColorBatch(Node parent, string name, int capacity = 4096)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		_points = (Vector2[])(object)new Vector2[capacity];
		_colors = (Color[])(object)new Color[capacity];
		_node = new Node2D
		{
			Name = StringName.op_Implicit(name)
		};
		parent.AddChild((Node)(object)_node, false, (InternalMode)0);
		_node.GlobalTransform = Transform2D.Identity;
		((CanvasItem)_node).Draw += Draw;
	}

	internal void Clear()
	{
		_count = 0;
	}

	internal void Triangle(Vector2 a, Vector2 b, Vector2 c, Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Triangle(a, b, c, color, color, color);
	}

	internal void Triangle(Vector2 a, Vector2 b, Vector2 c, Color ca, Color cb, Color cc)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (_count + 3 > _points.Length)
		{
			throw new InvalidOperationException("Bounded visual batch overflow");
		}
		_points[_count] = new Vector2(a.X, 0f - a.Y);
		_colors[_count++] = ca;
		_points[_count] = new Vector2(b.X, 0f - b.Y);
		_colors[_count++] = cb;
		_points[_count] = new Vector2(c.X, 0f - c.Y);
		_colors[_count++] = cc;
	}

	internal void Commit()
	{
		((CanvasItem)_node).Visible = _count > 0;
		((CanvasItem)_node).QueueRedraw();
	}

	private void Draw()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (_count > 0)
		{
			RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)_node).GetCanvasItem(), (ReadOnlySpan<int>)Indices.AsSpan(0, _count), (ReadOnlySpan<Vector2>)_points.AsSpan(0, _count), (ReadOnlySpan<Color>)_colors.AsSpan(0, _count), ReadOnlySpan<Vector2>.Empty, ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
		}
	}

	public void Dispose()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_node))
		{
			((CanvasItem)_node).Hide();
			((Node)_node).QueueFree();
		}
	}
}
internal sealed class DeathbringerEffect : IVisualEffect, IDisposable
{
	private sealed record Enemy(Vector2 Position, float Width, float Height)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Position = ");
			Vector2 position = Position;
			builder.Append(((object)(Vector2)(ref position)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Width = ");
			builder.Append(Width.ToString());
			builder.Append(", Height = ");
			builder.Append(Height.ToString());
			return true;
		}
	}

	private readonly record struct Spec(int Index, int Band, int Group, bool Loose, float Spawn, float Transfer, float TransferDuration, float Vy, float Gravity, float Reach, float Flutter, float Phase, float Outer, float Size, int Palette, float Flip, float Brightness, float Angle, float Spin, float Opacity);

	private readonly record struct Cloud(float Left, float Right, float[] Heights, float Jitter);

	private static Shader? _shader;

	private static ImageTexture? _texture;

	private static Vector2[] _origins = Array.Empty<Vector2>();

	private static Vector4[] _shadows = Array.Empty<Vector4>();

	private static Vector4[] _lights = Array.Empty<Vector4>();

	private static float _duration;

	private readonly Node2D[] _nodes = (Node2D[])(object)new Node2D[4];

	private readonly ShaderMaterial[] _materials = (ShaderMaterial[])(object)new ShaderMaterial[4];

	private float _age;

	private bool _disposed;

	private static readonly StringName Age = StringName.op_Implicit("age");

	private static readonly StringName Cutoff = StringName.op_Implicit("cutoff_age");

	private const string ShaderSource = "shader_type canvas_item;\nrender_mode unshaded, blend_mix;\nuniform sampler2D petal_texture;\nuniform float age=0.0;\nuniform float cutoff_age=1000.0;\nuniform bool rain_mode=false;\nuniform vec4 p0[84]; uniform vec4 p1[84]; uniform vec4 p2[84]; uniform vec4 p3[84];\nuniform vec4 p4[84]; uniform vec4 p5[84]; uniform vec4 p6[84]; uniform vec4 p7[84];\nuniform vec4 shadows[4]; uniform vec4 lights[4];\nvarying float opacity;varying float brightness;varying vec3 petal_shadow;varying vec3 petal_light;varying vec2 local_uv;\nvoid vertex(){\n local_uv=VERTEX+vec2(.5);\n int i=int(round(COLOR.r*255.0));vec4 a=p0[i];vec4 b=p1[i];vec4 c=p2[i];vec4 d=p3[i];vec4 origins=p4[i];vec4 rain=p5[i];vec4 bounds=p6[i];\n float t=age-a.x;float rain_age=age-a.y;float width=c.y;float motion_age=t;vec2 center=origins.xy;\n opacity=0.0;\n if(rain_mode){\n  float h=rain.x-rain.y*rain_age-rain.z*rain_age*rain_age*.5;\n  float drift=(sin(rain_age*2.1+a.w)-sin(a.w))*12.0+rain_age*bounds.z*13.0;\n  center=origins.zw+vec2(clamp(rain.w+drift,bounds.x,bounds.y),-(bounds.w+h));\n  width=p7[i].y;\n  if(rain_age>=0.0&&h>0.0)opacity=smoothstep(0.0,a.z,rain_age)*smoothstep(0.0,26.0,h)*d.x;\n }else{\n  float spread=smoothstep(.06,.65,t);float flow=1.0-exp(-t*2.3);float flutter=sin(t*b.w+a.w)-sin(a.w);\n  float x=(flow*b.z+t*7.0+flutter*11.0*spread+c.x*smoothstep(.14,1.05,t))*1.3;\n  float y=(b.x*t-b.y*t*t*.5+sin(t*4.0+float(i))*7.0*spread)*1.3;\n  center+=vec2(x,-y)*p7[i].x;width*=p7[i].x;\n  if(t>=0.0)opacity=smoothstep(0.0,.04,t)*(1.0-smoothstep(a.y,a.y+a.z,age))*d.x;\n }\n if(a.x>=cutoff_age-.00000001)opacity=0.0;\n float angle=c.z+t*c.w;float co=cos(angle);float si=sin(angle);float face=cos(motion_age*5.0+d.z);float flip=.055+.945*abs(face);\n vec2 q=vec2(VERTEX.x*width*flip,-VERTEX.y*width*1.4);\n VERTEX=center+vec2(q.x*co-q.y*si,-q.x*si-q.y*co);\n brightness=d.y*(.68+.32*max(0.0,face));opacity*=.93;\n int palette=int(d.w);petal_shadow=shadows[palette].rgb;petal_light=lights[palette].rgb;\n}\nvoid fragment(){\n float fold=smoothstep(.28,.72,local_uv.x*.7+local_uv.y*.3);\n COLOR=vec4(mix(petal_shadow,petal_light,fold)*brightness,texture(petal_texture,UV).a*opacity);\n}";

	public float Duration => _duration;

	public bool Hold => false;

	internal static void Preload()
	{
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		if (_shader != null)
		{
			return;
		}
		using (JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(Path.Combine(ClipAssets.Directory, "Deathbringer-" + ClipAssets.Version + ".deathdata"))))
		{
			JsonElement rootElement = jsonDocument.RootElement;
			_duration = rootElement.GetProperty("duration").GetSingle();
			_origins = ((IEnumerable<JsonElement>)rootElement.GetProperty("origins").EnumerateArray()).Select((Func<JsonElement, Vector2>)((JsonElement a) => new Vector2(a[0].GetSingle(), a[1].GetSingle()))).ToArray();
			_shadows = (from a in rootElement.GetProperty("palettes").EnumerateArray()
				select Vector(a.GetProperty("shadow"))).ToArray();
			_lights = (from a in rootElement.GetProperty("palettes").EnumerateArray()
				select Vector(a.GetProperty("light"))).ToArray();
			Image val = Image.LoadFromFile(Path.Combine(ClipAssets.Directory, "textures", rootElement.GetProperty("texture").GetString()));
			try
			{
				_texture = ImageTexture.CreateFromImage(val);
				Shader val2 = new Shader
				{
					Code = "shader_type canvas_item;\nrender_mode unshaded, blend_mix;\nuniform sampler2D petal_texture;\nuniform float age=0.0;\nuniform float cutoff_age=1000.0;\nuniform bool rain_mode=false;\nuniform vec4 p0[84]; uniform vec4 p1[84]; uniform vec4 p2[84]; uniform vec4 p3[84];\nuniform vec4 p4[84]; uniform vec4 p5[84]; uniform vec4 p6[84]; uniform vec4 p7[84];\nuniform vec4 shadows[4]; uniform vec4 lights[4];\nvarying float opacity;varying float brightness;varying vec3 petal_shadow;varying vec3 petal_light;varying vec2 local_uv;\nvoid vertex(){\n local_uv=VERTEX+vec2(.5);\n int i=int(round(COLOR.r*255.0));vec4 a=p0[i];vec4 b=p1[i];vec4 c=p2[i];vec4 d=p3[i];vec4 origins=p4[i];vec4 rain=p5[i];vec4 bounds=p6[i];\n float t=age-a.x;float rain_age=age-a.y;float width=c.y;float motion_age=t;vec2 center=origins.xy;\n opacity=0.0;\n if(rain_mode){\n  float h=rain.x-rain.y*rain_age-rain.z*rain_age*rain_age*.5;\n  float drift=(sin(rain_age*2.1+a.w)-sin(a.w))*12.0+rain_age*bounds.z*13.0;\n  center=origins.zw+vec2(clamp(rain.w+drift,bounds.x,bounds.y),-(bounds.w+h));\n  width=p7[i].y;\n  if(rain_age>=0.0&&h>0.0)opacity=smoothstep(0.0,a.z,rain_age)*smoothstep(0.0,26.0,h)*d.x;\n }else{\n  float spread=smoothstep(.06,.65,t);float flow=1.0-exp(-t*2.3);float flutter=sin(t*b.w+a.w)-sin(a.w);\n  float x=(flow*b.z+t*7.0+flutter*11.0*spread+c.x*smoothstep(.14,1.05,t))*1.3;\n  float y=(b.x*t-b.y*t*t*.5+sin(t*4.0+float(i))*7.0*spread)*1.3;\n  center+=vec2(x,-y)*p7[i].x;width*=p7[i].x;\n  if(t>=0.0)opacity=smoothstep(0.0,.04,t)*(1.0-smoothstep(a.y,a.y+a.z,age))*d.x;\n }\n if(a.x>=cutoff_age-.00000001)opacity=0.0;\n float angle=c.z+t*c.w;float co=cos(angle);float si=sin(angle);float face=cos(motion_age*5.0+d.z);float flip=.055+.945*abs(face);\n vec2 q=vec2(VERTEX.x*width*flip,-VERTEX.y*width*1.4);\n VERTEX=center+vec2(q.x*co-q.y*si,-q.x*si-q.y*co);\n brightness=d.y*(.68+.32*max(0.0,face));opacity*=.93;\n int palette=int(d.w);petal_shadow=shadows[palette].rgb;petal_light=lights[palette].rgb;\n}\nvoid fragment(){\n float fold=smoothstep(.28,.72,local_uv.x*.7+local_uv.y*.3);\n COLOR=vec4(mix(petal_shadow,petal_light,fold)*brightness,texture(petal_texture,UV).a*opacity);\n}"
				};
				Array shaderUniformList = val2.GetShaderUniformList(false);
				try
				{
					if (shaderUniformList.Count == 0)
					{
						throw new InvalidDataException("Deathbringer GPU program");
					}
					_shader = val2;
				}
				finally
				{
					((IDisposable)shaderUniformList)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		static Vector4 Vector(JsonElement a)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			return new Vector4(a[0].GetSingle(), a[1].GetSingle(), a[2].GetSingle(), a[3].GetSingle());
		}
	}

	private static float R(double n)
	{
		double num = Math.Sin(n * 127.1 + 311.7) * 43758.5453;
		return (float)(num - Math.Floor(num));
	}

	private static float Spawn(int i)
	{
		float num = (float)i / 83f;
		float num2 = ((num < 0.27f) ? (num / 0.27f * 0.15f) : ((num < 0.8f) ? (0.28f + (num - 0.27f) / 0.53f * 0.22f) : (0.71f + (num - 0.8f) / 0.2f * 0.29f)));
		return 2f / 15f + 0.44f * num2 + ((i % 7 < 2) ? (0.075f * (0.65f + 0.35f * R(i + 91))) : 0f);
	}

	private static Spec Compose(int i, uint seed)
	{
		int num = i % 6;
		int num2 = ((!((double)i < 22.68)) ? (((double)i < 67.2) ? 1 : 2) : 0);
		bool flag = i % 7 < 2;
		bool flag2 = num < 2;
		ReadOnlySpan<float> readOnlySpan = RuntimeHelpers.CreateSpan<float>((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		ReadOnlySpan<float> readOnlySpan2 = RuntimeHelpers.CreateSpan<float>((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		ReadOnlySpan<float> readOnlySpan3 = RuntimeHelpers.CreateSpan<float>((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		ReadOnlySpan<float> readOnlySpan4 = RuntimeHelpers.CreateSpan<float>((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		float num3 = (flag ? Rand(20) : (readOnlySpan[num2] + (Rand(20) - 0.5f) * readOnlySpan2[num2]));
		float num4 = Rand(30);
		int num5 = ((!((double)num4 < 0.18)) ? (((double)num4 < 0.66) ? 1 : (((double)num4 < 0.86) ? 2 : 3)) : 0);
		float num6 = Spawn(i);
		return new Spec(i, num, num2, flag, num6, num6 + 0.55f + (float)num2 * 0.035f + (Rand(40) - 0.5f) * 0.12f + (flag ? (0.18f * (0.55f + 0.45f * Rand(41))) : 0f), 0.4f + Rand(42) * 0.14f, 285f + num3 * 105f, 230f + Rand(21) * 50f, 30f * readOnlySpan3[num2] + (Rand(10) - 0.5f) * 108f * readOnlySpan4[num2], 3f + Rand(9) * 2f, Rand(8) * 6f, flag ? ((float)((i % 7 != 0) ? 1 : (-1)) * (76f + Rand(11) * 76f)) : 0f, (float)(5 + num * 2) + Rand(3) * (3f + (float)num * 0.8f), num5, Rand(5) * 6f, (flag2 ? 0.83f : 1f) * (1f + ((num5 == 3) ? 0.12f : 0f)), Rand(4) * 6f, Rand(6) * 3f - 1.5f, flag2 ? 0.78f : 0.96f);
		float Rand(int n)
		{
			return R(i + n + seed % 104729);
		}
	}

	internal DeathbringerEffect(NCreature hero, NCombatRoom room)
	{
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_072c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0731: Unknown result type (might be due to invalid IL or missing references)
		//IL_073e: Expected O, but got Unknown
		//IL_074f: Unknown result type (might be due to invalid IL or missing references)
		//IL_076d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0798: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0651: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_0699: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0838: Unknown result type (might be due to invalid IL or missing references)
		//IL_0840: Unknown result type (might be due to invalid IL or missing references)
		//IL_084d: Expected O, but got Unknown
		//IL_08dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0906: Unknown result type (might be due to invalid IL or missing references)
		//IL_0920: Unknown result type (might be due to invalid IL or missing references)
		Preload();
		Span<byte> span = stackalloc byte[4];
		RandomNumberGenerator.Fill(span);
		uint seed = BitConverter.ToUInt32(span);
		Enemy[] enemies = room.CreatureNodes.Where((NCreature c) => (int)c.Entity.Side == 2 && c.Entity.IsAlive).Select(delegate(NCreature c)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			Vector2 globalPosition = ((Control)c).GlobalPosition;
			Rect2 globalRect = c.Visuals.Bounds.GetGlobalRect();
			float x = ((Rect2)(ref globalRect)).Size.X;
			globalRect = c.Visuals.Bounds.GetGlobalRect();
			return new Enemy(globalPosition, x, ((Rect2)(ref globalRect)).Size.Y);
		}).ToArray();
		if (enemies.Length == 0)
		{
			throw new InvalidOperationException("No visual targets");
		}
		Array<Vector4>[] array = (from _ in Enumerable.Range(0, 8)
			select new Array<Vector4>()).ToArray();
		Spec[] array2 = (from i in Enumerable.Range(0, 84)
			select Compose(i, seed)).ToArray();
		Cloud[] array3 = enemies.Select((Enemy e) => Cluster(e, enemies, seed)).ToArray();
		Transform2D val = (SpineCompatibility.Allows(hero.Body, "necrobinder") ? (hero.Body.GlobalTransform * new Transform2D(0f, Vector2.One / 0.27f, 0f, Vector2.Zero)) : ActorAnchor.Ground(hero.Body));
		float num = ((Vector2)(ref val.X)).Length();
		Vector2 origin = val.Origin;
		Spec[] array4 = array2;
		Spec p2;
		for (int j = 0; j < array4.Length; j++)
		{
			p2 = array4[j];
			int num2 = (p2.Index / 6 + p2.Index % 6 + (int)(seed % (uint)enemies.Length)) % enemies.Length;
			Enemy enemy = enemies[num2];
			Cloud cloud = array3[num2];
			float num3 = cloud.Right - cloud.Left;
			float num4 = MathF.Min(18f, num3 * 0.2f);
			float num5 = (p2.Loose ? ((p2.Index % 7 == 0) ? (0.05f + Rand(55) * 0.12f) : (0.83f + Rand(55) * 0.12f)) : (((p2.Group == 0) ? 0.25f : ((p2.Group == 1) ? 0.53f : 0.76f)) + (Rand(56) - 0.5f) * 0.25f));
			Vector2 val2 = origin + new Vector2(_origins[p2.Index].X, 0f - _origins[p2.Index].Y) * num;
			array[0].Add(new Vector4(p2.Spawn, p2.Transfer, p2.TransferDuration, p2.Phase));
			array[1].Add(new Vector4(p2.Vy, p2.Gravity, p2.Reach, p2.Flutter));
			array[2].Add(new Vector4(p2.Outer, p2.Size, p2.Angle, p2.Spin));
			array[3].Add(new Vector4(p2.Opacity, p2.Brightness, p2.Flip, (float)p2.Palette));
			array[4].Add(new Vector4(val2.X, val2.Y, enemy.Position.X, enemy.Position.Y));
			array[5].Add(new Vector4(cloud.Heights[p2.Group] + (Rand(51) - 0.5f) * cloud.Jitter, 46f + Rand(52) * 28f, 72f + Rand(53) * 36f, cloud.Left + num4 + num5 * (num3 - 2f * num4)));
			array[6].Add(new Vector4(cloud.Left + num4, cloud.Right - num4, Rand(57) - 0.5f, (float)(12 - p2.Band * 3)));
			array[7].Add(new Vector4(num, MathF.Min(p2.Size, MathF.Max(3f, num3 * 0.35f)), 0f, 0f));
		}
		try
		{
			for (int k = 0; k < 4; k++)
			{
				bool rear = k % 2 == 0;
				Spec[] array5 = (from p in array2
					where p.Band < 2 == rear
					orderby p.Band, p.Index
					select p).ToArray();
				Vector2[] vertices = (Vector2[])(object)new Vector2[array5.Length * 4];
				Vector2[] uv = (Vector2[])(object)new Vector2[vertices.Length];
				Color[] colors = (Color[])(object)new Color[vertices.Length];
				int[] indices = new int[array5.Length * 6];
				for (int l = 0; l < array5.Length; l++)
				{
					Spec spec = array5[l];
					for (int m = 0; m < 4; m++)
					{
						vertices[l * 4 + m] = new Vector2((float)(m % 2) - 0.5f, (m < 2) ? (-0.5f) : 0.5f);
						uv[l * 4 + m] = new Vector2((float)(spec.Index % 2 + m % 2) * 0.5f, (float)(spec.Index % 4 / 2 + ((m >= 2) ? 1 : 0)) * 0.5f);
						colors[l * 4 + m] = new Color((float)spec.Index / 255f, 1f, 1f, 1f);
					}
					ReadOnlySpan<int> readOnlySpan = RuntimeHelpers.CreateSpan<int>((RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
					for (int num6 = 0; num6 < 6; num6++)
					{
						indices[l * 6 + num6] = l * 4 + readOnlySpan[num6];
					}
				}
				ShaderMaterial val3 = new ShaderMaterial
				{
					Shader = _shader
				};
				val3.SetShaderParameter(StringName.op_Implicit("petal_texture"), Variant.op_Implicit((GodotObject)(object)_texture));
				val3.SetShaderParameter(StringName.op_Implicit("rain_mode"), Variant.op_Implicit(k >= 2));
				for (int num7 = 0; num7 < 8; num7++)
				{
					val3.SetShaderParameter(StringName.op_Implicit("p" + num7), Array<Vector4>.op_Implicit(array[num7]));
				}
				Array val4 = (Array)new Array<Vector4>(_shadows);
				try
				{
					Array val5 = (Array)new Array<Vector4>(_lights);
					try
					{
						val3.SetShaderParameter(StringName.op_Implicit("shadows"), Variant.op_Implicit(val4));
						val3.SetShaderParameter(StringName.op_Implicit("lights"), Variant.op_Implicit(val5));
						val3.SetShaderParameter(Cutoff, Variant.op_Implicit(1000f));
						Node2D node = new Node2D
						{
							Name = StringName.op_Implicit("NecroPetals" + k),
							Material = (Material)(object)val3,
							TextureFilter = (TextureFilterEnum)2
						};
						_nodes[k] = node;
						_materials[k] = val3;
						Node node2 = ((Node)room).GetNode(NodePath.op_Implicit("CombatSceneContainer"));
						node2.AddChild((Node)(object)node, false, (InternalMode)0);
						if (rear)
						{
							node2.MoveChild((Node)(object)node, Math.Min(node2.GetNode(NodePath.op_Implicit("AllyContainer")).GetIndex(false), node2.GetNode(NodePath.op_Implicit("EnemyContainer")).GetIndex(false)));
						}
						node.GlobalTransform = Transform2D.Identity;
						((CanvasItem)node).Draw += delegate
						{
							//IL_0006: Unknown result type (might be due to invalid IL or missing references)
							//IL_0043: Unknown result type (might be due to invalid IL or missing references)
							//IL_0049: Unknown result type (might be due to invalid IL or missing references)
							RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)node).GetCanvasItem(), (ReadOnlySpan<int>)indices, (ReadOnlySpan<Vector2>)vertices, (ReadOnlySpan<Color>)colors, (ReadOnlySpan<Vector2>)uv, ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
						};
						RenderingServer.CanvasItemSetCustomRect(((CanvasItem)node).GetCanvasItem(), true, (Rect2?)new Rect2(-500f, -500f, 2920f, 2080f));
						((CanvasItem)node).QueueRedraw();
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
				}
			}
		}
		catch
		{
			Dispose();
			throw;
		}
		finally
		{
			Array<Vector4>[] array6 = array;
			for (int j = 0; j < array6.Length; j++)
			{
				((Array)array6[j]).Dispose();
			}
		}
		float Rand(int n)
		{
			return R(p2.Index + n + seed % 104729);
		}
	}

	private static Cloud Cluster(Enemy enemy, Enemy[] neighbors, uint seed)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Clamp(enemy.Width * 1.28f, 150f, 1200f) * 1.3f;
		float num2 = MathF.Max((0f - num) / 2f, 30f - enemy.Position.X);
		float num3 = MathF.Min(num / 2f, 1890f - enemy.Position.X);
		foreach (Enemy enemy2 in neighbors)
		{
			if ((object)enemy2 != enemy && !(MathF.Abs(enemy2.Position.Y - enemy.Position.Y) > 150f))
			{
				float num4 = (enemy2.Position.X - enemy.Position.X) / 2f;
				if (enemy2.Position.X < enemy.Position.X)
				{
					num2 = MathF.Max(num2, num4 + 10f);
				}
				else if (enemy2.Position.X > enemy.Position.X)
				{
					num3 = MathF.Min(num3, num4 - 10f);
				}
			}
		}
		if (num3 - num2 < 12f)
		{
			float num5 = Math.Clamp((num2 + num3) / 2f, 36f - enemy.Position.X, 1884f - enemy.Position.X);
			num2 = num5 - 6f;
			num3 = num5 + 6f;
		}
		float num6 = MathF.Min(640f, MathF.Max(80f, enemy.Position.Y - 35f));
		float value = MathF.Min(num6, MathF.Max(180f, enemy.Height + 105f));
		float num7 = MathF.Min(28f, num6 * 0.16f);
		float num8 = num7 + 40f;
		float num9 = num6 - num7;
		float heightSpan = MathF.Min(80f, (num9 - num8) * 0.8f);
		float centerHeight = Math.Clamp(value, num8 + heightSpan / 2f, num9 - heightSpan / 2f);
		int[][] array = new int[6][]
		{
			new int[3] { 0, 2, 1 },
			new int[3] { 2, 0, 1 },
			new int[3] { 1, 2, 0 },
			new int[3] { 1, 0, 2 },
			new int[3] { 0, 1, 2 },
			new int[3] { 2, 1, 0 }
		};
		double roll = (double)seed + (double)enemy.Position.X * 0.371;
		int[] order = array[(int)(R(roll + 71.0) * (float)array.Length)];
		return new Cloud(num2, num3, (from g in Enumerable.Range(0, 3)
			select centerHeight + (float)(order[g] - 1) * heightSpan / 2f + (R(roll + 72.0 + (double)g) - 0.5f) * MathF.Min(12f, heightSpan * 0.16f)).ToArray(), MathF.Min(44f, num7 * 1.5f));
	}

	public void Advance(float age)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		_age = age;
		ShaderMaterial[] materials = _materials;
		for (int i = 0; i < materials.Length; i++)
		{
			materials[i].SetShaderParameter(Age, Variant.op_Implicit(age));
		}
	}

	public void CancelBody()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		ShaderMaterial[] materials = _materials;
		for (int i = 0; i < materials.Length; i++)
		{
			materials[i].SetShaderParameter(Cutoff, Variant.op_Implicit(_age));
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		Node2D[] nodes = _nodes;
		foreach (Node2D val in nodes)
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((CanvasItem)val).Hide();
				((CanvasItem)val).Material = null;
				((Node)val).QueueFree();
			}
		}
		ShaderMaterial[] materials = _materials;
		foreach (ShaderMaterial obj in materials)
		{
			if (obj != null)
			{
				((GodotObject)obj).Dispose();
			}
		}
	}
}
internal sealed class EffectQuad : IDisposable
{
	private static readonly Dictionary<string, StringName> Names = new Dictionary<string, StringName>();

	internal readonly Node2D Node = new Node2D
	{
		TextureFilter = (TextureFilterEnum)2
	};

	internal readonly ShaderMaterial Material;

	private readonly Vector2[] _points = (Vector2[])(object)new Vector2[4];

	private readonly Vector2[] _uvs = (Vector2[])(object)new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f)
	};

	private static readonly int[] Indices = new int[6] { 0, 1, 2, 2, 1, 3 };

	private static readonly Color[] White = (Color[])(object)new Color[1] { Colors.White };

	private bool _disposed;

	internal EffectQuad(Node parent, Shader shader)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		Material = new ShaderMaterial
		{
			Shader = shader
		};
		((CanvasItem)Node).Material = (Material)(object)Material;
		parent.AddChild((Node)(object)Node, false, (InternalMode)0);
		Node.GlobalTransform = Transform2D.Identity;
		((CanvasItem)Node).Hide();
		((CanvasItem)Node).Draw += Draw;
	}

	internal void Set(string key, Variant value)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		if (!Names.TryGetValue(key, out StringName value2))
		{
			value2 = new StringName(ClipAssets.Parameter(key));
			Names.Add(key, value2);
		}
		Material.SetShaderParameter(value2, value);
	}

	internal void Apply(ClipAssets assets, ClipAssets.DrawSpec draw)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		ClipAssets.Uniform[] compiled = draw.Compiled;
		for (int i = 0; i < compiled.Length; i++)
		{
			ClipAssets.Uniform uniform = compiled[i];
			if (uniform.Kind == -1)
			{
				if (!assets.Data.Textures[uniform.Texture].Screen)
				{
					Material.SetShaderParameter(uniform.Name, Variant.op_Implicit((GodotObject)(object)assets.Textures[uniform.Texture]));
				}
				continue;
			}
			switch (uniform.Kind)
			{
			case 0:
				Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(uniform.Boolean));
				break;
			case 1:
				Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(uniform.Value.X));
				break;
			case 2:
				Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(new Vector2(uniform.Value.X, uniform.Value.Y)));
				break;
			case 3:
				Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(new Vector3(uniform.Value.X, uniform.Value.Y, uniform.Value.Z)));
				break;
			case 4:
				Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(uniform.Value));
				break;
			}
		}
	}

	internal void Rect(Vector2 center, float left, float right, float height, float rotation = 0f, float uvLeft = 0f)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		float num = MathF.Cos(rotation);
		float num2 = MathF.Sin(rotation);
		for (int i = 0; i < 4; i++)
		{
			float num3 = ((i % 2 == 0) ? left : right);
			float num4 = ((i < 2) ? (-0.5f) : 0.5f) * height;
			_points[i] = center + new Vector2(num3 * num - num4 * num2, num3 * num2 + num4 * num);
			_uvs[i] = new Vector2((i % 2 == 0) ? uvLeft : 1f, (float)((i >= 2) ? 1 : 0));
		}
		((CanvasItem)Node).Show();
		((CanvasItem)Node).QueueRedraw();
	}

	private void Draw()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)Node).GetCanvasItem(), (ReadOnlySpan<int>)Indices, (ReadOnlySpan<Vector2>)_points, (ReadOnlySpan<Color>)White, (ReadOnlySpan<Vector2>)_uvs, ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)Node))
			{
				((CanvasItem)Node).Hide();
				((CanvasItem)Node).Material = null;
				((Node)Node).QueueFree();
			}
			((GodotObject)Material).Dispose();
		}
	}
}
internal static class EmissionPool
{
	private static readonly Dictionary<Vector2I, Stack<SubViewport>> Available = new Dictionary<Vector2I, Stack<SubViewport>>();

	private static Node? _root;

	private static Node Root
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected O, but got Unknown
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				return _root;
			}
			_root = new Node
			{
				Name = StringName.op_Implicit("NecroEmissionReserve")
			};
			((Node)((SceneTree)Engine.GetMainLoop()).Root).AddChild(_root, false, (InternalMode)0);
			_root.TreeExiting += delegate
			{
				Available.Clear();
				_root = null;
			};
			return _root;
		}
	}

	internal static SubViewport Rent(Node parent, int width, int height)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		Vector2I val = default(Vector2I);
		((Vector2I)(ref val))..ctor(width, height);
		SubViewport val2;
		if (Available.TryGetValue(val, out Stack<SubViewport> value) && value.Count > 0)
		{
			val2 = value.Pop();
			Node parent2 = ((Node)val2).GetParent();
			if (parent2 != null)
			{
				parent2.RemoveChild((Node)(object)val2);
			}
		}
		else
		{
			val2 = new SubViewport
			{
				Name = StringName.op_Implicit("NecroEmission"),
				Size = val,
				World2D = new World2D(),
				TransparentBg = true,
				Disable3D = true,
				RenderTargetClearMode = (ClearMode)0
			};
		}
		parent.AddChild((Node)(object)val2, false, (InternalMode)0);
		val2.RenderTargetUpdateMode = (UpdateMode)4;
		return val2;
	}

	internal static void Return(SubViewport view)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)view) || ((GodotObject)view).IsQueuedForDeletion())
		{
			return;
		}
		view.RenderTargetUpdateMode = (UpdateMode)0;
		if (!Available.TryGetValue(view.Size, out Stack<SubViewport> value))
		{
			Available.Add(view.Size, value = new Stack<SubViewport>());
		}
		if (value.Count >= 2)
		{
			((Node)view).QueueFree();
			return;
		}
		Node parent = ((Node)view).GetParent();
		if (parent != null)
		{
			parent.RemoveChild((Node)(object)view);
		}
		Root.AddChild((Node)(object)view, false, (InternalMode)0);
		value.Push(view);
	}
}
internal sealed class EndEmissionBatch : IDisposable
{
	private const int Capacity = 40;

	private static readonly QuadMesh Quad = new QuadMesh
	{
		Size = Vector2.One
	};

	private static readonly Dictionary<(Shader, bool), ShaderMaterial> Materials = new Dictionary<(Shader, bool), ShaderMaterial>();

	private readonly MultiMesh _mesh = new MultiMesh
	{
		TransformFormat = (TransformFormatEnum)0,
		UseColors = true,
		UseCustomData = true,
		Mesh = (Mesh)(object)Quad,
		InstanceCount = 40,
		VisibleInstanceCount = 0
	};

	private readonly float[] _buffer = new float[640];

	internal readonly MultiMeshInstance2D Node;

	private ShaderMaterial? _material;

	private int _count;

	private Vector2 _lo;

	private Vector2 _hi;

	private bool _disposed;

	internal int ShaderId { get; private set; } = -1;

	internal bool Additive { get; private set; }

	internal EndEmissionBatch(Node parent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		Node = new MultiMeshInstance2D
		{
			Name = StringName.op_Implicit("NecroMeteorEmissions"),
			Multimesh = _mesh,
			Visible = false
		};
		parent.AddChild((Node)(object)Node, false, (InternalMode)0);
		((Node2D)Node).GlobalTransform = Transform2D.Identity;
	}

	internal void Begin(ClipAssets assets, int shader, bool add)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Expected O, but got Unknown
		ShaderId = shader;
		Additive = add;
		_count = 0;
		_lo = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
		_hi = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
		if (!Materials.TryGetValue((assets.Shaders[shader], add), out ShaderMaterial value))
		{
			string text = assets.Shaders[shader].Code.Replace("uniform float p_mode;", "varying float p_mode;", StringComparison.Ordinal).Replace("uniform vec4 p_tint;", "varying vec4 p_tint;", StringComparison.Ordinal).Replace("uniform vec4 p_params;", "varying vec4 p_params;", StringComparison.Ordinal)
				.Replace("mesh_tint=COLOR;source_uv=UV;", "VERTEX=UV-vec2(.5);mesh_tint=vec4(1.0);source_uv=UV;p_mode=INSTANCE_CUSTOM.z;p_tint=vec4(COLOR.rgb,1.0);p_params=vec4(INSTANCE_CUSTOM.xy,COLOR.a,INSTANCE_CUSTOM.w);", StringComparison.Ordinal);
			if (add)
			{
				text = text.Replace("blend_mix", "blend_add", StringComparison.Ordinal);
			}
			Shader val = new Shader
			{
				Code = text
			};
			Array shaderUniformList = val.GetShaderUniformList(false);
			try
			{
				value = new ShaderMaterial
				{
					Shader = val
				};
				Materials.Add((assets.Shaders[shader], add), value);
			}
			finally
			{
				((IDisposable)shaderUniformList)?.Dispose();
			}
		}
		if (value != _material)
		{
			_material = value;
			((CanvasItem)Node).Material = (Material)(object)value;
		}
	}

	internal void Add(Vector2 position, float width, float height, float rotation, float mode, float depth, float alpha, int seed, float age, Vector3 color)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		if (_count == 40)
		{
			throw new InvalidOperationException("Meteor emission batch capacity");
		}
		int num = _count++ * 16;
		float num2 = MathF.Cos(rotation);
		float num3 = MathF.Sin(rotation);
		_buffer[num] = num2 * width;
		_buffer[num + 1] = (0f - num3) * height;
		_buffer[num + 2] = 0f;
		_buffer[num + 3] = position.X;
		_buffer[num + 4] = num3 * width;
		_buffer[num + 5] = num2 * height;
		_buffer[num + 6] = 0f;
		_buffer[num + 7] = position.Y;
		_buffer[num + 8] = color.X;
		_buffer[num + 9] = color.Y;
		_buffer[num + 10] = color.Z;
		_buffer[num + 11] = alpha;
		_buffer[num + 12] = age;
		_buffer[num + 13] = depth;
		_buffer[num + 14] = mode;
		_buffer[num + 15] = seed;
		Vector2 val = new Vector2(MathF.Abs(num2 * width) + MathF.Abs(num3 * height), MathF.Abs(num3 * width) + MathF.Abs(num2 * height)) * 0.5f;
		_lo = ((Vector2)(ref _lo)).Min(position - val);
		_hi = ((Vector2)(ref _hi)).Max(position + val);
	}

	internal void Commit()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (_count == 0)
		{
			((CanvasItem)Node).Hide();
			return;
		}
		_mesh.CustomAabb = new Aabb(new Vector3(_lo.X, _lo.Y, -1f), new Vector3(_hi.X - _lo.X, _hi.Y - _lo.Y, 2f));
		_mesh.Buffer = _buffer;
		_mesh.VisibleInstanceCount = _count;
		((CanvasItem)Node).Show();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)Node))
			{
				((CanvasItem)Node).Hide();
				Node.Multimesh = null;
				((CanvasItem)Node).Material = null;
				((Node)Node).QueueFree();
			}
			((GodotObject)_mesh).Dispose();
		}
	}
}
internal sealed class EndMeteor
{
	internal readonly record struct Composition(string Id, bool Front, float Depth, float Start, float Hit, Vector2 From, Vector2 To, float Radius, float Opacity, bool Primary = false, bool Follow = false, bool Offscreen = false)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Id = ");
			builder.Append((object?)Id);
			builder.Append(", Front = ");
			builder.Append(Front.ToString());
			builder.Append(", Depth = ");
			builder.Append(Depth.ToString());
			builder.Append(", Start = ");
			builder.Append(Start.ToString());
			builder.Append(", Hit = ");
			builder.Append(Hit.ToString());
			builder.Append(", From = ");
			Vector2 val = From;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", To = ");
			val = To;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Radius = ");
			builder.Append(Radius.ToString());
			builder.Append(", Opacity = ");
			builder.Append(Opacity.ToString());
			builder.Append(", Primary = ");
			builder.Append(Primary.ToString());
			builder.Append(", Follow = ");
			builder.Append(Follow.ToString());
			builder.Append(", Offscreen = ");
			builder.Append(Offscreen.ToString());
			return true;
		}
	}

	internal readonly record struct Pose(Vector2 Position, float Progress, bool Flight, float Impact, float Alpha, float Radius)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Position = ");
			Vector2 position = Position;
			builder.Append(((object)(Vector2)(ref position)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Progress = ");
			builder.Append(Progress.ToString());
			builder.Append(", Flight = ");
			builder.Append(Flight.ToString());
			builder.Append(", Impact = ");
			builder.Append(Impact.ToString());
			builder.Append(", Alpha = ");
			builder.Append(Alpha.ToString());
			builder.Append(", Radius = ");
			builder.Append(Radius.ToString());
			return true;
		}
	}

	internal readonly record struct Rock(int Tier, float Size, float Lag, float Side, float Drift, float Spin, float Angle, bool Front, Vector2[] Corners, float Squash);

	internal static readonly Composition[] CompositionData = new Composition[10]
	{
		new Composition("far-left", Front: false, 0.14f, 0.1f, 1.5f, new Vector2(310f, -90f), new Vector2(630f, 350f), 9f, 0.23f),
		new Composition("far-center", Front: false, 0.24f, 0.04f, 1.4f, new Vector2(1060f, -160f), new Vector2(840f, 455f), 12f, 0.31f),
		new Composition("far-right", Front: false, 0.19f, 0.28f, 1.7f, new Vector2(1810f, -120f), new Vector2(1630f, 390f), 10f, 0.25f),
		new Composition("far-late", Front: false, 0.12f, 0.7f, 2.08f, new Vector2(960f, -180f), new Vector2(1120f, 390f), 8f, 0.21f),
		new Composition("middle-left", Front: false, 0.56f, 0.08f, 1.09f, new Vector2(490f, -170f), new Vector2(1040f, 655f), 26f, 0.64f),
		new Composition("middle-right", Front: false, 0.67f, 0.17f, 1.29f, new Vector2(2080f, -190f), new Vector2(1610f, 670f), 34f, 0.72f),
		new Composition("middle-late", Front: false, 0.48f, 0.63f, 1.68f, new Vector2(1260f, -140f), new Vector2(1800f, 660f), 23f, 0.54f),
		new Composition("main", Front: true, 1f, 0.6f, 0.8f, new Vector2(943f, 80f), new Vector2(1298f, 694f), 100f, 1f, Primary: true),
		new Composition("near-edge", Front: true, 1.25f, 0.34f, 1.17f, new Vector2(1810f, -230f), new Vector2(2300f, 1260f), 135f, 0.78f, Primary: false, Follow: false, Offscreen: true),
		new Composition("follow-up", Front: true, 1.16f, 0.72f, 0.98f, new Vector2(1083f, 30f), new Vector2(1468f, 712f), 140f, 1f, Primary: false, Follow: true)
	};

	internal readonly Composition Recipe;

	internal readonly Vector2 From;

	internal readonly Vector2 To;

	internal readonly Vector2 Direction;

	internal readonly float Angle;

	internal readonly float Radius;

	internal readonly float Spin;

	internal readonly float CorePhase;

	internal readonly float CoreSpeed;

	internal readonly float CoreWidth;

	internal readonly float RibbonWidth;

	internal readonly float Split;

	internal readonly float Heat;

	internal readonly int Seed;

	internal readonly Rock[] Rocks;

	internal bool Hero
	{
		get
		{
			if (!Recipe.Primary)
			{
				return Recipe.Follow;
			}
			return true;
		}
	}

	internal static float Random(int n)
	{
		double num = Math.Sin((double)n * 127.1 + 311.7) * 43758.5453123;
		return (float)(num - Math.Floor(num));
	}

	internal static float Smooth(float x)
	{
		return OstyMotion.Smooth(x);
	}

	internal EndMeteor(int index, int seed, Vector2 floor)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		Recipe = CompositionData[index];
		Seed = seed + index * 47;
		Composition recipe = Recipe;
		Vector2 val = floor - new Vector2(1298f, 694f);
		float num = (Random(Seed) - 0.5f) * 42f * (float)((!recipe.Primary) ? 1 : 0);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))..ctor(floor.X - (floor.Y - 80f) / MathF.Sqrt(3f), 80f);
		From = new Vector2(recipe.From.X + val.X * 0.48f + num, recipe.From.Y);
		To = new Vector2(recipe.To.X + val.X * (recipe.Front ? 1f : 0.6f), recipe.To.Y + val.Y * ((recipe.Depth > 0.4f) ? 1f : 0.2f));
		if (recipe.Primary)
		{
			From = val2;
			To = floor;
		}
		if (recipe.Follow)
		{
			From = val2 + new Vector2(140f + num * 0.35f, -50f);
			To = floor + new Vector2(170f + num * 0.35f, 18f);
		}
		Vector2 val3 = To - From;
		Direction = ((Vector2)(ref val3)).Normalized();
		Angle = MathF.Atan2(0f - Direction.X, Direction.Y);
		Radius = recipe.Radius * (Hero ? 1f : (0.94f + 0.12f * Random(Seed + 3)));
		Spin = (Random(Seed + 4) - 0.5f) * 0.16f;
		CorePhase = (Random(seed + 1401) * 4f + (recipe.Follow ? (3.15f + Random(seed + 1402) * 0.2f) : 0f)) % 4f;
		CoreSpeed = 0.92f + Random(Seed + 1403) * 0.16f;
		CoreWidth = 0.96f + Random(Seed + 1404) * 0.08f;
		RibbonWidth = 0.88f + Random(Seed + 1405) * 0.24f;
		Split = (Random(Seed + 1406) - 0.5f) * 0.36f;
		Heat = 0.9f + Random(Seed + 1407) * 0.2f;
		int[] array = ((!recipe.Primary) ? ((!recipe.Follow) ? ((!(recipe.Depth < 0.4f)) ? ((!recipe.Offscreen) ? new int[3] { 3, 10, 35 } : new int[3] { 6, 18, 58 }) : new int[3] { 0, 2, 7 }) : new int[3] { 10, 34, 112 }) : new int[3] { 8, 28, 106 });
		List<Rock> list = new List<Rock>();
		for (int i = 0; i < 3; i++)
		{
			for (int k = 0; k < array[i]; k++)
			{
				int rockSeed = Seed + list.Count * 113 + 219;
				float num2 = i switch
				{
					1 => 7f + R(1) * 12f, 
					0 => 25f + R(1) * 24f, 
					_ => 1.4f + R(1) * 4.6f, 
				};
				float squash = 0.35f + R(9) * 0.92f;
				int count = 4 + (int)(R(10) * 4f);
				Vector2[] corners = Enumerable.Range(0, count).Select((Func<int, Vector2>)delegate(int j)
				{
					//IL_003f: Unknown result type (might be due to invalid IL or missing references)
					float x = (float)j * ((float)Math.PI * 2f) / (float)count;
					float num3 = 0.55f + 0.45f * R(j + 20);
					return new Vector2(MathF.Cos(x) * num3, MathF.Sin(x) * num3 * squash);
				}).ToArray();
				list.Add(new Rock(i, num2 * Radius / 100f, ((i == 0) ? (0.18f + R(2) * 2.8f) : (R(2) * 6.6f)) * Radius, (R(3) - 0.5f) * ((i == 0) ? 2.4f : 4.3f) * Radius, (R(4) - 0.5f) * Radius * 1.2f, (R(5) - 0.5f) * (float)((i == 0) ? 3 : 8), R(6) * ((float)Math.PI * 2f), (i == 0) ? (k % 3 != 0) : (R(7) > 0.34f), corners, 0.35f + R(9) * 0.92f));
				float R(int n)
				{
					return Random(rockSeed + n);
				}
			}
		}
		Rocks = list.ToArray();
	}

	internal Pose Sample(float age)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float num = (age - Recipe.Start) / (Recipe.Hit - Recipe.Start);
		float num2 = Math.Clamp(num, 0f, 1f);
		float num3 = (Hero ? num2 : (0.72f * num2 + 0.28f * num2 * num2));
		return new Pose(((Vector2)(ref From)).Lerp(To, num3), num2, num >= 0f && num < 1f, age - Recipe.Hit, Recipe.Opacity * (Hero ? 1f : Smooth(num / 0.13f)) * ((Recipe.Depth < 0.4f) ? (1f - Smooth((num - 0.82f) / 0.18f)) : 1f), Radius * ((Recipe.Depth > 1f && !Hero) ? (0.72f + 0.28f * num2) : 1f));
	}
}
internal sealed class EndMeteorView : IDisposable
{
	private readonly EndMeteor _meteor;

	private readonly Node2D _root;

	private readonly Node2D _quadsRoot;

	private readonly ClipAssets _assets;

	private readonly EffectQuad _core;

	private readonly EndRibbon _ribbon;

	private readonly ColorBatch _back;

	private readonly ColorBatch _front;

	private readonly EndEmissionBatch[] _batches;

	private int _used;

	private bool _impactOrder;

	private bool _inactive;

	private static readonly Dictionary<ClipAssets, Vector2[]> Curves = new Dictionary<ClipAssets, Vector2[]>();

	private readonly Vector2[] _curve;

	private static readonly Vector3[] RockColors = new float[3] { 0.04f, 0.75f, 0.26f }.Select(delegate(float light)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = new Vector3(0.012f, 0.01f, 0.017f);
		return ((Vector3)(ref val)).Lerp(new Vector3(0.095f, 0.042f, 0.028f), light);
	}).ToArray();

	internal EndMeteorView(Node parent, EndMeteor meteor, ClipAssets assets)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Expected O, but got Unknown
		_meteor = meteor;
		_assets = assets;
		if (!Curves.TryGetValue(assets, out _curve))
		{
			_curve = ((IEnumerable<JsonElement>)assets.Data.Extra.GetProperty("coreScale").EnumerateArray()).Select((Func<JsonElement, Vector2>)((JsonElement p) => new Vector2(p[0].GetSingle(), p[1].GetSingle()))).ToArray();
			Curves.Add(assets, _curve);
		}
		_root = new Node2D
		{
			Name = StringName.op_Implicit("NecroMeteor_" + meteor.Recipe.Id)
		};
		parent.AddChild((Node)(object)_root, false, (InternalMode)0);
		_root.GlobalTransform = Transform2D.Identity;
		_back = new ColorBatch((Node)(object)_root, "RockBack", meteor.Rocks.Where((EndMeteor.Rock r) => !r.Front).Sum((EndMeteor.Rock r) => r.Corners.Length * 3));
		_ribbon = new EndRibbon((Node)(object)_root, meteor);
		ClipAssets.DrawSpec drawSpec = assets.Data.Frames[0][(meteor.Recipe.Depth < 0.4f) ? 1u : 0u];
		_core = new EffectQuad((Node)(object)_root, assets.Shaders[drawSpec.Shader]);
		_core.Apply(assets, drawSpec);
		_front = new ColorBatch((Node)(object)_root, "RockFront", meteor.Rocks.Where((EndMeteor.Rock r) => r.Front).Sum((EndMeteor.Rock r) => r.Corners.Length * 3));
		_quadsRoot = new Node2D
		{
			Name = StringName.op_Implicit("Emission")
		};
		((Node)_root).AddChild((Node)(object)_quadsRoot, false, (InternalMode)0);
		_batches = (from _ in Enumerable.Range(0, (meteor.Recipe.Depth < 0.4f) ? 2 : 10)
			select new EndEmissionBatch((Node)(object)_quadsRoot)).ToArray();
	}

	private EndEmissionBatch Batch(int shader, bool add)
	{
		if (_used > 0 && _batches[_used - 1].ShaderId == shader && _batches[_used - 1].Additive == add)
		{
			return _batches[_used - 1];
		}
		if (_used == _batches.Length)
		{
			throw new InvalidOperationException("Meteor emission order capacity");
		}
		EndEmissionBatch obj = _batches[_used++];
		obj.Begin(_assets, shader, add);
		return obj;
	}

	private void Sprite(Vector2 p, float w, float h, float angle, float mode, float depth, float alpha, int seed, float age, Vector3 color, bool add = false)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!(alpha < 0.001f))
		{
			Batch(2, add).Add(p, w, h, angle, mode, depth, alpha, seed, age, color);
		}
	}

	private void Burst(Vector2 p, float w, float h, float mode, float age, float alpha, int seed, Vector3 color, bool add = false)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!(alpha < 0.001f))
		{
			Batch(3, add).Add(p, w, h, 0f, mode, 0f, alpha, seed, age, color);
		}
	}

	private Vector2 CoreScale(float progress)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Clamp(progress, 0f, 1f) * 120f;
		int num2 = Math.Min(119, (int)num);
		return ((Vector2)(ref _curve[num2])).Lerp(_curve[num2 + 1], num - (float)num2);
	}

	internal void Advance(float age)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0423: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		if (age < _meteor.Recipe.Start || age > _meteor.Recipe.Hit + ((_meteor.Recipe.Offscreen || _meteor.Recipe.Depth < 0.4f) ? 0.85f : 2.3f))
		{
			if (!_inactive)
			{
				((CanvasItem)_root).Hide();
				_inactive = true;
			}
			return;
		}
		if (_inactive)
		{
			((CanvasItem)_root).Show();
			_inactive = false;
		}
		_used = 0;
		EndMeteor meteor = _meteor;
		EndMeteor.Pose pose = meteor.Sample(age);
		_ribbon.Advance(age);
		((CanvasItem)_core.Node).Hide();
		if (pose.Flight)
		{
			float num = pose.Radius / 69f * (meteor.Hero ? 1f : 0.68f);
			Vector2 val = CoreScale(pose.Progress);
			float num2 = 1792f * val.X * num * (meteor.Hero ? meteor.CoreWidth : 1f);
			float height = 896f * val.Y * num;
			float num3 = ((!meteor.Hero) ? (meteor.Recipe.Offscreen ? 0.67f : 0.65f) : (meteor.Recipe.Follow ? 0.94f : 1f));
			_core.Rect(pose.Position - meteor.Direction * 345f * num, (0f - num2) / 2f, num2 / 2f, height, meteor.Angle);
			_core.Set("vertex_color", Variant.op_Implicit(new Vector4(num3, num3, num3, pose.Alpha * 0.92f)));
			_core.Set("modulate_color", Variant.op_Implicit(new Vector4(num3, num3, num3, pose.Alpha * 0.92f)));
			_core.Set("lifetime", Variant.op_Implicit(pose.Progress));
			_core.Set("TIME", Variant.op_Implicit(meteor.Hero ? ((age - meteor.Recipe.Start) * meteor.CoreSpeed + meteor.CorePhase / 15f) : age));
			if (_ribbon.Active)
			{
				for (int i = 0; i < ((meteor.Recipe.Depth < 0.4f) ? 3 : 15); i++)
				{
					int k = meteor.Seed + i * 31;
					int num4 = 5 + i * 3 % 40;
					int num5 = i % 3;
					float num6 = _ribbon.Widths[num4] * ((num5 == 0) ? ((R(0) - 0.5f) * 0.45f) : ((float)((i % 2 != 0) ? 1 : (-1)) * ((num5 == 1) ? 1f : (1.5f + R(0) * 0.45f))));
					Vector2 p = _ribbon.History[num4] + new Vector2(0f - meteor.Direction.Y, meteor.Direction.X) * num6;
					float num7 = ((num5 == 0) ? 11f : (3f + R(4) * 5f)) * MathF.Min(1.5f, meteor.Recipe.Depth + 0.2f);
					Sprite(p, num7 * ((num5 == 0) ? 1.8f : 1f), num7, (num5 == 0) ? (meteor.Angle + (float)Math.PI / 2f) : (meteor.Angle + R(2) * 3f), (num5 == 0) ? 2 : 4, meteor.Recipe.Depth, pose.Alpha * ((num5 == 0) ? 0.2f : 0.55f), k, age, (Vector3)(num5 switch
					{
						1 => new Vector3(1f, 0.27f, 0.05f), 
						0 => new Vector3(1f, 0.83f, 0.7f), 
						_ => new Vector3(0.93f, 0.1f, 0.16f), 
					}), num5 == 0);
					float R(int n)
					{
						return EndMeteor.Random(k + n);
					}
				}
			}
		}
		if (pose.Impact >= 0f)
		{
			if (!_impactOrder)
			{
				_impactOrder = true;
				((Node)_root).MoveChild((Node)(object)_back.Node, ((Node)_root).GetChildCount(false) - 1);
				((Node)_root).MoveChild((Node)(object)_front.Node, ((Node)_root).GetChildCount(false) - 1);
			}
			Impact(pose.Impact, age);
		}
		Rocks(age);
		for (int j = 0; j < _used; j++)
		{
			_batches[j].Commit();
		}
		for (int l = _used; l < _batches.Length; l++)
		{
			((CanvasItem)_batches[l].Node).Hide();
		}
	}

	private void Impact(float a, float age)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		EndMeteor meteor = _meteor;
		if (a > 2.3f || meteor.Recipe.Offscreen || meteor.Recipe.Depth < 0.4f)
		{
			return;
		}
		Vector2 to = meteor.To;
		float num = (meteor.Hero ? (meteor.Radius * 0.69f) : meteor.Radius);
		float num2 = (meteor.Recipe.Primary ? 1f : (meteor.Recipe.Follow ? 1.08f : 0.3f));
		Burst(to + new Vector2(0f, -3f), num * 7.3f, num * 1.65f, 2f, a, (1f - Math.Clamp((a - 0.25f) / 2.05f, 0f, 1f)) * num2, meteor.Seed, new Vector3(1f, 0.2f, 0.017f), add: true);
		if (a < 1.05f)
		{
			float num3 = 1f - MathF.Pow(1f - Math.Clamp(a / 1.05f, 0f, 1f), 3f);
			float num4 = num * (2f + num3 * 13f);
			Burst(to + new Vector2(0f, -8f), num4, num4 * 0.25f, 1f, a, (1f - a / 1.05f) * 0.9f * num2, meteor.Seed, new Vector3(0.91f, 0.25f, 0.095f));
			Burst(to + new Vector2(0f, -7f), num4 * 0.96f, num4 * 0.21f, 1f, a, (1f - a / 1.05f) * 0.3f * num2, meteor.Seed, new Vector3(1f, 0.47f, 0.11f), add: true);
		}
		if (a < 0.2f)
		{
			Burst(to + new Vector2(0f, (0f - num) * 0.4f), num * (7f + a * 17f), num * (5f + a * 9f), 0f, a, MathF.Pow(1f - a / 0.2f, 2f) * num2, meteor.Seed, new Vector3(1f, 0.31f, 0.065f), add: true);
		}
		for (int i = 0; i < 7; i++)
		{
			int num5 = meteor.Seed + i * 59;
			float num6 = a - (float)i * 0.025f;
			if (!(num6 < 0f) && !(num6 > 1.65f))
			{
				float num7 = (float)((i % 2 != 0) ? 1 : (-1)) * (35f + EndMeteor.Random(num5) * 190f) * num6;
				float num8 = -35f - (45f + EndMeteor.Random(num5 + 3) * 75f) * num6;
				float num9 = num * (0.8f + num6 * 1.4f + EndMeteor.Random(num5 + 4) * 0.8f);
				Burst(to + new Vector2(num7, num8), num9, num9 * 0.68f, 3f, num6, MathF.Sin((float)Math.PI * Math.Clamp(num6 / 1.65f, 0f, 1f)) * 0.4f * num2, num5, new Vector3(0.22f, 0.1f, 0.12f));
			}
		}
		for (int j = 0; j < (meteor.Hero ? 22 : 9); j++)
		{
			int k = meteor.Seed + j * 97;
			float num10 = 0.45f + R(2) * 0.95f;
			if (!(a > num10))
			{
				float num11 = (R(0) - 0.5f) * (float)(meteor.Hero ? 780 : 390);
				float num12 = -90f - R(1) * 350f;
				float num13 = 3f + R(4) * (float)(meteor.Hero ? 12 : 7);
				Sprite(to + new Vector2(num11 * a, num12 * a + 360f * a * a), num13, num13 * (0.6f + R(5)), a * (R(7) * 10f - 5f), 4f, 1f, MathF.Pow(1f - a / num10, 0.8f) * num2, k, age, (j % 4 == 0) ? new Vector3(0.07f, 0.04f, 0.08f) : new Vector3(1f, 0.27f, 0.019f));
			}
			float R(int n)
			{
				return EndMeteor.Random(k + n);
			}
		}
	}

	private unsafe void Rocks(float age)
	{
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		_back.Clear();
		_front.Clear();
		EndMeteor meteor = _meteor;
		if (age >= meteor.Recipe.Start && age <= meteor.Recipe.Hit + 0.85f)
		{
			EndMeteor.Pose pose = meteor.Sample(MathF.Min(age, meteor.Recipe.Hit));
			float num = MathF.Max(0f, age - meteor.Recipe.Hit);
			Vector2 val = default(Vector2);
			((Vector2)(ref val))..ctor(0f - meteor.Direction.Y, meteor.Direction.X);
			Span<Vector2> span = new Span<Vector2>(stackalloc Vector2[7], 7);
			EndMeteor.Rock[] rocks = meteor.Rocks;
			Vector2 a = default(Vector2);
			for (int i = 0; i < rocks.Length; i++)
			{
				EndMeteor.Rock rock = rocks[i];
				float num2 = ((rock.Tier == 0) ? 0.85f : 0.52f);
				float num3 = meteor.Recipe.Opacity * Math.Clamp((age - meteor.Recipe.Start) / 0.055f, 0f, 1f) * (1f - Math.Clamp(num / num2, 0f, 1f)) * ((meteor.Recipe.Depth < 0.4f) ? 0.38f : 1f);
				if (!(num3 <= 0.002f))
				{
					Vector2 val2 = pose.Position + meteor.Direction * (0f - rock.Lag + num * (float)((rock.Tier == 0) ? 130 : 70)) + val * (rock.Side + rock.Drift * (pose.Progress + num * 2f)) + new Vector2(0f, num * num * 220f);
					float x = rock.Angle + rock.Spin * (age - meteor.Recipe.Start);
					float num4 = MathF.Cos(x);
					float num5 = MathF.Sin(x);
					ColorBatch colorBatch = (rock.Front ? _front : _back);
					for (int j = 0; j < rock.Corners.Length; j++)
					{
						Vector2 val3 = rock.Corners[j] * rock.Size;
						Vector2 val4 = val2 + new Vector2(val3.X * num4 - val3.Y * num5, val3.X * num5 + val3.Y * num4);
						span[j] = new Vector2(val4.X, 0f - val4.Y);
					}
					((Vector2)(ref a))..ctor(val2.X, 0f - val2.Y);
					float num6 = num3 * ((meteor.Recipe.Depth < 0.4f) ? 0.65f : 1f);
					for (int k = 0; k < rock.Corners.Length; k++)
					{
						Vector3 val5 = RockColors[k switch
						{
							2 => 2, 
							1 => 1, 
							_ => 0, 
						}];
						colorBatch.Triangle(a, span[k], span[(k + 1) % rock.Corners.Length], new Color(val5.X, val5.Y, val5.Z, num6));
					}
				}
			}
		}
		_back.Commit();
		_front.Commit();
	}

	public void Dispose()
	{
		_ribbon.Dispose();
		_core.Dispose();
		_back.Dispose();
		_front.Dispose();
		EndEmissionBatch[] batches = _batches;
		for (int i = 0; i < batches.Length; i++)
		{
			batches[i].Dispose();
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			((CanvasItem)_root).Hide();
			((Node)_root).QueueFree();
		}
	}
}
internal sealed class EndOfDaysEffect : IVisualEffect, IDisposable
{
	private readonly ClipAssets _assets;

	private readonly EndMeteor[] _meteors;

	private readonly EndMeteorView[] _views;

	private readonly Node2D _rear;

	private readonly Node2D _front;

	private readonly Node2D _environment;

	private readonly ClipPlayer _anticipation;

	private readonly ClipPlayer[] _impacts = new ClipPlayer[2];

	private readonly EffectQuad _sky;

	private readonly EffectQuad _haze;

	private readonly VisualShake _shake = new VisualShake();

	private bool _disposed;

	private static Shader? SkyShader;

	private static Shader? HazeShader;

	private static readonly Dictionary<ClipAssets, bool[]> ImpactScreenFrames = new Dictionary<ClipAssets, bool[]>();

	private readonly BackBufferCopy[] _impactCopies = (BackBufferCopy[])(object)new BackBufferCopy[2];

	private readonly bool[] _impactScreenFrames;

	private readonly Node2D[] _impactRoots = (Node2D[])(object)new Node2D[2];

	public float Duration => 4.6f;

	public bool Hold => false;

	internal EndOfDaysEffect(NCreature hero, NCreature osty, NCombatRoom room)
	{
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Expected O, but got Unknown
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Expected O, but got Unknown
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Expected O, but got Unknown
		//IL_0410: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Expected O, but got Unknown
		//IL_0530: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_0547: Expected O, but got Unknown
		try
		{
			_assets = ClipAssets.Get("EndPrograms");
			NCreature[] array = room.CreatureNodes.Where((NCreature c) => (int)c.Entity.Side == 2 && c.Entity.IsAlive).ToArray();
			if (array.Length == 0)
			{
				throw new InvalidOperationException("No live meteor target");
			}
			Vector2 floor = new Vector2(array.Average((NCreature c) => ((Control)c).GlobalPosition.X), array.Max((NCreature c) => ((Control)c).GlobalPosition.Y));
			Vector2 top = new Vector2(floor.X - (floor.Y - 80f) / MathF.Sqrt(3f), 80f);
			int seed = RandomNumberGenerator.GetInt32(10000);
			_meteors = (from i in Enumerable.Range(0, 10)
				select new EndMeteor(i, seed, floor)).ToArray();
			_environment = new Node2D
			{
				Name = StringName.op_Implicit("NecroMeteorSky")
			};
			Node node = ((Node)room).GetNode(NodePath.op_Implicit("CombatSceneContainer"));
			node.AddChild((Node)(object)_environment, false, (InternalMode)0);
			node.MoveChild((Node)(object)_environment, node.GetNode(NodePath.op_Implicit("BgContainer")).GetIndex(false) + 1);
			_environment.GlobalTransform = Transform2D.Identity;
			BattleScreen.Copy((Node)(object)_environment, "NecroSkySceneCopy");
			if (SkyShader == null)
			{
				SkyShader = new Shader
				{
					Code = ClipAssets.ConvertShader(_assets.Data.Shaders[4]).Replace("uniform sampler2D source_texture;", "uniform sampler2D source_texture : hint_screen_texture, repeat_disable, filter_linear;", StringComparison.Ordinal).Replace("void source_fragment(inout vec4 out_color)", "void source_fragment(inout vec4 out_color,vec2 screen_uv)", StringComparison.Ordinal)
						.Replace("source_fragment(color);", "source_fragment(color,SCREEN_UV);", StringComparison.Ordinal)
						.Replace("texture(source_texture,source_uv)", "texture(source_texture,screen_uv)", StringComparison.Ordinal)
				};
				Array shaderUniformList = SkyShader.GetShaderUniformList(false);
				try
				{
					if (shaderUniformList.Count == 0)
					{
						throw new InvalidDataException("Meteor background shader");
					}
				}
				finally
				{
					((IDisposable)shaderUniformList)?.Dispose();
				}
			}
			_sky = new EffectQuad((Node)(object)_environment, SkyShader);
			_sky.Set("dark", Variant.op_Implicit(0f));
			BattleScreen.Cover(_sky);
			_rear = new Node2D
			{
				Name = StringName.op_Implicit("NecroMeteorRear")
			};
			((Node)room).GetNode(NodePath.op_Implicit("CombatSceneContainer/BackCombatVfxContainer")).AddChild((Node)(object)_rear, false, (InternalMode)0);
			_rear.GlobalTransform = Transform2D.Identity;
			if (HazeShader == null)
			{
				HazeShader = new Shader
				{
					Code = _assets.Shaders[3].Code.Replace("blend_mix", "blend_add", StringComparison.Ordinal)
				};
				Array shaderUniformList2 = HazeShader.GetShaderUniformList(false);
				try
				{
					if (shaderUniformList2.Count == 0)
					{
						throw new InvalidDataException("Meteor floor haze shader");
					}
				}
				finally
				{
					((IDisposable)shaderUniformList2)?.Dispose();
				}
			}
			_haze = new EffectQuad((Node)(object)_rear, HazeShader);
			_haze.Rect(floor, -200f, 200f, 65f);
			_haze.Set("mode", Variant.op_Implicit(3f));
			_haze.Set("tint", Variant.op_Implicit(new Vector4(0.9f, 0.2f, 0.035f, 1f)));
			_front = new Node2D
			{
				Name = StringName.op_Implicit("NecroMeteorFront")
			};
			((Node)room.CombatVfxContainer).AddChild((Node)(object)_front, false, (InternalMode)0);
			_front.GlobalTransform = Transform2D.Identity;
			_views = new EndMeteorView[_meteors.Length];
			for (int j = 0; j < _meteors.Length; j++)
			{
				EndMeteor endMeteor = _meteors[j];
				_views[j] = new EndMeteorView((Node)(object)(endMeteor.Recipe.Front ? _front : _rear), endMeteor, _assets);
			}
			_anticipation = new ClipPlayer(ClipAssets.Get("EndAnticipation"), hero, osty, (Node)(object)_front, ownBody: true, (Node?)(object)_front)
			{
				BasisOverride = (string _) => new Transform2D(0f, top)
			};
			ClipAssets impactAssets = ClipAssets.Get("EndImpact");
			if (!ImpactScreenFrames.TryGetValue(impactAssets, out _impactScreenFrames))
			{
				_impactScreenFrames = impactAssets.Data.Frames.Select((ClipAssets.DrawSpec[] frame) => frame.Any((ClipAssets.DrawSpec draw) => draw.Compiled.Any((ClipAssets.Uniform u) => u.Kind == -1 && impactAssets.Data.Textures[u.Texture].Screen))).ToArray();
				ImpactScreenFrames.Add(impactAssets, _impactScreenFrames);
			}
			for (int k = 0; k < 2; k++)
			{
				Node2D val = new Node2D
				{
					Name = StringName.op_Implicit("NecroMeteorNativeImpact")
				};
				((Node)_front).AddChild((Node)(object)val, false, (InternalMode)0);
				_impactCopies[k] = BattleScreen.Copy((Node)(object)val, "NecroImpactSceneCopy");
				EndMeteor meteor = _meteors[(k == 0) ? 7 : 9];
				float scale = ((k == 0) ? 1f : 1.16f);
				_impacts[k] = new ClipPlayer(impactAssets, hero, osty, (Node)(object)val, ownBody: true, (Node?)(object)val)
				{
					BasisOverride = (string _) => new Transform2D(0f, Vector2.One * scale, 0f, meteor.To)
				};
				_impacts[k].Visible(visible: false);
				_impactRoots[k] = val;
				((CanvasItem)val).Hide();
			}
			Advance(0f);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public void Advance(float age)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		JsonElement property = _assets.Data.Extra.GetProperty("environment");
		float num = Math.Clamp(age * 60f, 0f, property.GetArrayLength() - 1);
		int num2 = (int)num;
		int index = Math.Min(num2 + 1, property.GetArrayLength() - 1);
		float num3 = num - (float)num2;
		_sky.Set("strength", Variant.op_Implicit(Mathf.Lerp(property[num2][0].GetSingle(), property[index][0].GetSingle(), num3)));
		_sky.Set("skyStrength", Variant.op_Implicit(Mathf.Lerp(property[num2][1].GetSingle(), property[index][1].GetSingle(), num3)));
		_sky.Set("skyTime", Variant.op_Implicit(Mathf.Lerp(property[num2][2].GetSingle(), property[index][2].GetSingle(), num3)));
		((CanvasItem)_environment).Visible = age < 1.31f;
		if (((CanvasItem)_environment).Visible)
		{
			BattleScreen.Cover(_sky);
		}
		float num4 = Math.Clamp(age / 0.65f, 0f, 1f) * (1f - Math.Clamp((age - 0.8f) / 0.15f, 0f, 1f));
		((CanvasItem)_haze.Node).Visible = num4 > 0.001f;
		_haze.Set("params", Variant.op_Implicit(new Vector4(age, 0f, num4 * 0.14f, (float)(_meteors[7].Seed - 329))));
		EndMeteorView[] views = _views;
		for (int i = 0; i < views.Length; i++)
		{
			views[i].Advance(age);
		}
		_anticipation.Advance(age);
		Vector2 val = Vector2.Zero;
		for (int j = 0; j < 2; j++)
		{
			float num5 = age - ((j == 0) ? 0.8f : 0.98f);
			bool flag = num5 >= 0f && num5 < 2f;
			((CanvasItem)_impactRoots[j]).Visible = flag;
			_impacts[j].Visible(flag);
			if (flag)
			{
				_impacts[j].Advance(num5);
			}
			int num6 = (int)Math.Clamp(num5 * (float)_impacts[j].Assets.Data.Hz, 0f, _impactScreenFrames.Length - 1);
			((CanvasItem)_impactCopies[j]).Visible = flag && _impactScreenFrames[num6];
			if (num5 >= 0f && num5 < 0.55f)
			{
				float num7 = MathF.Pow(1f - num5 / 0.55f, 2f) * ((j == 0) ? 1f : 1.12f);
				val += new Vector2(MathF.Sin(num5 * 82f) * 15f, MathF.Sin(num5 * 103f) * 12f + MathF.Cos(num5 * 29f) * 5f) * num7;
			}
		}
		_shake.SetOffset(new Vector2(Math.Clamp(val.X, -20f, 20f), Math.Clamp(val.Y, -18f, 18f)));
	}

	public void CancelBody()
	{
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		_shake.Dispose();
		EndMeteorView[] array = _views ?? Array.Empty<EndMeteorView>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i]?.Dispose();
		}
		_anticipation?.Dispose();
		ClipPlayer[] impacts = _impacts;
		for (int i = 0; i < impacts.Length; i++)
		{
			impacts[i]?.Dispose();
		}
		_sky?.Dispose();
		_haze?.Dispose();
		Node2D[] array2 = (Node2D[])(object)new Node2D[3] { _rear, _front, _environment };
		foreach (Node2D val in array2)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((CanvasItem)val).Hide();
				((Node)val).QueueFree();
			}
		}
	}
}
internal sealed class EndRibbon : IDisposable
{
	private readonly record struct TongueSpec(float From, float To, float Offset, float Size, Vector3 Color, float Opacity, bool Jagged = false)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("From = ");
			builder.Append(From.ToString());
			builder.Append(", To = ");
			builder.Append(To.ToString());
			builder.Append(", Offset = ");
			builder.Append(Offset.ToString());
			builder.Append(", Size = ");
			builder.Append(Size.ToString());
			builder.Append(", Color = ");
			Vector3 color = Color;
			builder.Append(((object)(Vector3)(ref color)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Opacity = ");
			builder.Append(Opacity.ToString());
			builder.Append(", Jagged = ");
			builder.Append(Jagged.ToString());
			return true;
		}
	}

	private sealed record TongueData(int Start, int End, int Vertex, float Offset, float Size, Color Color, bool Jagged, float[] U, float[] Envelope)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Start = ");
			builder.Append(Start.ToString());
			builder.Append(", End = ");
			builder.Append(End.ToString());
			builder.Append(", Vertex = ");
			builder.Append(Vertex.ToString());
			builder.Append(", Offset = ");
			builder.Append(Offset.ToString());
			builder.Append(", Size = ");
			builder.Append(Size.ToString());
			builder.Append(", Color = ");
			Color color = Color;
			builder.Append(((object)(Color)(ref color)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Jagged = ");
			builder.Append(Jagged.ToString());
			builder.Append(", U = ");
			builder.Append(U);
			builder.Append(", Envelope = ");
			builder.Append(Envelope);
			return true;
		}
	}

	private static readonly Shader Shader = new Shader
	{
		Code = "shader_type canvas_item;render_mode unshaded;uniform float soft;void fragment(){COLOR.a*=smoothstep(0.,soft,UV.x)*(1.-smoothstep(1.-soft,1.,UV.x));}"
	};

	private readonly Vector2[] _points = (Vector2[])(object)new Vector2[882];

	private readonly Vector2[] _uvs = Geometry.Uvs;

	private readonly Color[] _colors = (Color[])(object)new Color[882];

	private readonly int[] _indices = Geometry.Indices;

	private readonly int _vertices = Geometry.Vertices;

	private readonly int _count = Geometry.Count;

	private readonly Node2D _node;

	private readonly EndMeteor _meteor;

	internal readonly Vector2[] History = (Vector2[])(object)new Vector2[49];

	internal readonly float[] Widths = new float[49];

	private static readonly TongueSpec[] Shapes = new TongueSpec[9]
	{
		new TongueSpec(0f, 1f, 0f, 1.42f, new Vector3(0.045f, 0.009f, 0.04f), 0.9f, Jagged: true),
		new TongueSpec(0.04f, 0.97f, 0f, 1.1f, new Vector3(0.3f, 0.018f, 0.095f), 1f),
		new TongueSpec(0.04f, 0.91f, -0.51f, 0.6f, new Vector3(0.87f, 0.06f, 0.1f), 0.96f, Jagged: true),
		new TongueSpec(0.17f, 1f, 0.55f, 0.48f, new Vector3(1f, 0.19f, 0.045f), 0.96f, Jagged: true),
		new TongueSpec(0.22f, 0.8f, 0.99f, 0.23f, new Vector3(1f, 0.4f, 0.13f), 0.9f, Jagged: true),
		new TongueSpec(0.1f, 0.9f, -0.12f, 0.7f, new Vector3(0.13f, 0.014f, 0.042f), 0.97f),
		new TongueSpec(0.26f, 1f, 0.07f, 0.48f, new Vector3(1f, 0.26f, 0.045f), 0.95f),
		new TongueSpec(0.36f, 0.88f, -0.47f, 0.14f, new Vector3(0.12f, 0.019f, 0.04f), 0.97f, Jagged: true),
		new TongueSpec(0.57f, 1f, 0.42f, 0.18f, new Vector3(1f, 0.67f, 0.31f), 0.91f)
	};

	private static readonly float[] WidthShape = Enumerable.Range(0, 49).Select(delegate(int i)
	{
		float num = (float)i / 48f;
		return MathF.Pow(num, 0.75f) * Math.Clamp((1f - num) * 9f, 0f, 1f) * (1f + 0.07f * MathF.Sin((float)i * 1.8f));
	}).ToArray();

	private static readonly (TongueData[] Tongues, Vector2[] Uvs, int[] Indices, int Vertices, int Count) Geometry = BuildGeometry();

	private readonly TongueData[] _tongues = Geometry.Tongues;

	private readonly Color[] _tints = (Color[])(object)new Color[Shapes.Length];

	private readonly float[] _tooth = new float[49];

	private readonly float[] _animatedTooth = new float[49];

	private bool _disposed;

	internal bool Active { get; private set; }

	internal EndRibbon(Node parent, EndMeteor meteor)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		_meteor = meteor;
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = Shader
		};
		val.SetShaderParameter(StringName.op_Implicit("soft"), Variant.op_Implicit((meteor.Recipe.Depth < 0.4f) ? 0.36f : 0.1f));
		_node = new Node2D
		{
			Name = StringName.op_Implicit("NecroMeteorFlame"),
			Material = (Material)(object)val,
			Visible = false
		};
		parent.AddChild((Node)(object)_node, false, (InternalMode)0);
		_node.GlobalTransform = Transform2D.Identity;
		((CanvasItem)_node).Draw += Draw;
		for (int i = 0; i < 49; i++)
		{
			_tooth[i] = 0.2f * MathF.Sin((float)i * 0.71f + (float)meteor.Seed);
		}
		for (int j = 0; j < Shapes.Length; j++)
		{
			TongueSpec tongueSpec = Shapes[j];
			Vector3 val2 = tongueSpec.Color;
			val2.Y *= (meteor.Hero ? meteor.Heat : 1f);
			val2 = ((Vector3)(ref val2)).Lerp(new Vector3(0.19f, 0.11f, 0.24f), Math.Clamp((0.7f - meteor.Recipe.Depth) * 0.65f, 0f, 1f));
			_tints[j] = new Color(val2.X, val2.Y, val2.Z, tongueSpec.Opacity);
		}
	}

	private static (TongueData[], Vector2[], int[], int, int) BuildGeometry()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		TongueData[] array = new TongueData[Shapes.Length];
		Vector2[] array2 = (Vector2[])(object)new Vector2[882];
		int[] array3 = new int[2592];
		int num = 0;
		int item = 0;
		for (int i = 0; i < Shapes.Length; i++)
		{
			TongueSpec tongueSpec = Shapes[i];
			int num2 = (int)MathF.Round(tongueSpec.From * 48f);
			int num3 = (int)MathF.Round(tongueSpec.To * 48f);
			int num4 = num;
			float[] array4 = new float[num3 - num2 + 1];
			float[] array5 = new float[array4.Length];
			for (int j = num2; j <= num3; j++)
			{
				array4[j - num2] = (float)(j - num2) / (float)(num3 - num2);
				array5[j - num2] = MathF.Pow(MathF.Max(0f, MathF.Sin((float)Math.PI * array4[j - num2])), 0.65f);
				array2[num++] = new Vector2(1f, array4[j - num2]);
				array2[num++] = new Vector2(0f, array4[j - num2]);
			}
			for (int k = num4 + 2; k < num; k++)
			{
				array3[item++] = k - 2;
				array3[item++] = k - 1;
				array3[item++] = k;
			}
			array[i] = new TongueData(num2, num3, num4, tongueSpec.Offset, tongueSpec.Size, new Color(tongueSpec.Color.X, tongueSpec.Color.Y, tongueSpec.Color.Z, tongueSpec.Opacity), tongueSpec.Jagged, array4, array5);
		}
		return (array, array2, array3, num, item);
	}

	internal void Advance(float age)
	{
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		EndMeteor meteor = _meteor;
		float num = MathF.Min(age, meteor.Recipe.Hit);
		float x = meteor.Recipe.Hit - meteor.Recipe.Start;
		float num2 = MathF.Max(meteor.Recipe.Start, num - MathF.Min(x, meteor.Hero ? 0.15f : 0.46f));
		float num3 = MathF.Max(0f, age - meteor.Recipe.Hit);
		if (num <= num2 || num3 >= 0.25f)
		{
			Active = false;
			((CanvasItem)_node).Hide();
			return;
		}
		Active = true;
		float num4 = meteor.Radius * 0.69f * MathF.Pow(1f - Math.Clamp(num3 / 0.25f, 0f, 1f), 1.3f);
		for (int i = 0; i < 49; i++)
		{
			float num5 = (float)i / 48f;
			History[i] = meteor.Sample(Mathf.Lerp(num2, num, num5)).Position;
			Widths[i] = num4 * WidthShape[i];
		}
		float num6 = ((age < meteor.Recipe.Hit) ? meteor.Sample(age).Alpha : (meteor.Recipe.Opacity * MathF.Pow(1f - num3 / 0.25f, 1.7f)));
		float num7 = age * 4.5f * (meteor.Hero ? meteor.CoreSpeed : 1f) + (float)meteor.Seed * 0.37f;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))..ctor(0f - meteor.Direction.Y, meteor.Direction.X);
		for (int j = 0; j < 49; j++)
		{
			_animatedTooth[j] = _tooth[j] + 0.12f * MathF.Sin((float)j * 1.43f + num7);
		}
		for (int k = 0; k < _tongues.Length; k++)
		{
			TongueData tongueData = _tongues[k];
			Color val2 = _tints[k];
			val2.A = Math.Clamp(num6 * val2.A, 0f, 1f);
			int vertex = tongueData.Vertex;
			for (int l = tongueData.Start; l <= tongueData.End; l++)
			{
				int num8 = l - tongueData.Start;
				float num9 = tongueData.Envelope[num8];
				float num10 = (tongueData.Jagged ? _animatedTooth[l] : 0f);
				float num11 = Widths[l] * (tongueData.Offset + ((meteor.Hero ? meteor.Split : 0f) + MathF.Sin(tongueData.U[num8] * 5.2f + num7) * 0.13f) * num9);
				float num12 = Widths[l] * tongueData.Size * (meteor.Hero ? meteor.RibbonWidth : 1f) * num9 * (1f + num10);
				_points[vertex] = History[l] + val * (num11 + num12);
				_colors[vertex++] = val2;
				_points[vertex] = History[l] + val * (num11 - num12);
				_colors[vertex++] = val2;
			}
		}
		((CanvasItem)_node).Show();
		((CanvasItem)_node).QueueRedraw();
	}

	private void Draw()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (_count > 0)
		{
			RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)_node).GetCanvasItem(), (ReadOnlySpan<int>)_indices.AsSpan(0, _count), (ReadOnlySpan<Vector2>)_points.AsSpan(0, _vertices), (ReadOnlySpan<Color>)_colors.AsSpan(0, _vertices), (ReadOnlySpan<Vector2>)_uvs.AsSpan(0, _vertices), ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_node))
		{
			((CanvasItem)_node).Hide();
			Material material = ((CanvasItem)_node).Material;
			((CanvasItem)_node).Material = null;
			if (material != null)
			{
				((GodotObject)material).Dispose();
			}
			((Node)_node).QueueFree();
		}
	}
}
internal static class EradicateAssets
{
	internal sealed class Needle
	{
		public int Surface { get; set; }

		public int[] Ids { get; set; } = Array.Empty<int>();

		public float[] Weights { get; set; } = Array.Empty<float>();

		public float[] Coeff { get; set; } = Array.Empty<float>();

		public float Length { get; set; }

		public bool Rear { get; set; }

		public string Tier { get; set; } = "";
	}

	internal sealed class Target
	{
		public string Id { get; set; } = "";

		public string Rig { get; set; } = "";

		public bool Burrowing { get; set; }

		public float SizeScale { get; set; } = 1f;

		public JsonElement Surfaces { get; set; }

		public JsonElement Clips { get; set; }

		public Dictionary<int, Needle[]> Banks { get; set; } = new Dictionary<int, Needle[]>();

		public Dictionary<int, Needle[]> StunnedBanks { get; set; } = new Dictionary<int, Needle[]>();

		internal Needle[] Bank(int hits, bool stunned = false)
		{
			Dictionary<int, Needle[]> dictionary = ((stunned && StunnedBanks.Count > 0) ? StunnedBanks : Banks);
			return dictionary[dictionary.Keys.Order().FirstOrDefault((int n) => n >= hits, dictionary.Keys.Max())];
		}
	}

	private sealed class Bank
	{
		public Dictionary<string, Dictionary<string, Target>> Versions { get; set; } = new Dictionary<string, Dictionary<string, Target>>();
	}

	private static Bank? _bank;

	private static Texture2D? _texture;

	internal static readonly Shader Shader = new Shader
	{
		Code = ShaderCode()
	};

	internal static Texture2D Texture => _texture ?? (_texture = ResourceLoader.Load<Texture2D>("res://images/packed/vfx/combat/slice_thin_vfx.png", (string)null, (CacheMode)1));

	internal static void Prepare()
	{
		if (_bank != null)
		{
			return;
		}
		using Stream stream = typeof(EradicateAssets).Assembly.GetManifestResourceStream("eradicate-bindings.json.br") ?? throw new InvalidDataException("Eradicate bindings missing");
		using BrotliStream utf8Json = new BrotliStream(stream, CompressionMode.Decompress);
		_bank = JsonSerializer.Deserialize<Bank>(utf8Json, new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		});
		_ = Texture;
	}

	internal static Target? Find(string id)
	{
		Prepare();
		return _bank.Versions[ClipAssets.Version].GetValueOrDefault(id.ToLowerInvariant());
	}

	internal static IEnumerable<Target> All()
	{
		Prepare();
		return _bank.Versions[ClipAssets.Version].Values;
	}

	private static string ShaderCode()
	{
		using Stream stream = typeof(EradicateAssets).Assembly.GetManifestResourceStream("eradicate-spike.gdshader") ?? throw new InvalidDataException("Eradicate material missing");
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}
}
[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class EradicateAttackPatch
{
	internal sealed class Invocation(AttackCommand command, Invocation? previous)
	{
		internal readonly AttackCommand Command = command;

		internal readonly Invocation? Previous = previous;

		internal int Count = 1;

		internal EradicateVisual? Visual;
	}

	internal static readonly AsyncLocal<Invocation?> Current = new AsyncLocal<Invocation>();

	private static readonly ConditionalWeakTable<AttackCommand, object> Bound = new ConditionalWeakTable<AttackCommand, object>();

	private static readonly FieldInfo? Target = AccessTools.Field(typeof(AttackCommand), "_singleTarget");

	private static Node2D? PrepareStrike(AttackCommand command)
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Invalid comparison between Unknown and I4
		try
		{
			Invocation value = Current.Value;
			if (value == null || value.command != command || !AnimationSettings.Current.Allows("Eradicate"))
			{
				return null;
			}
			if (value.Visual == null)
			{
				object? obj = Target?.GetValue(command);
				Creature val = (Creature)((obj is Creature) ? obj : null);
				if (val != null)
				{
					value.Visual = EradicateVisual.Create(val, value.Count);
				}
			}
			Creature attacker = command.Attacker;
			float? obj2;
			if (attacker == null)
			{
				obj2 = null;
			}
			else
			{
				Player player = attacker.Player;
				obj2 = ((player != null) ? new float?(player.Character.AttackAnimDelay) : ((float?)null));
			}
			float num = obj2 ?? 0.15f;
			if ((int)SaveManager.Instance.PrefsSave.FastMode != 1)
			{
				num *= 0.5f;
			}
			value.Visual?.Prepare(num);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
		return null;
	}

	private static Node2D? Hit(AttackCommand command, Creature target)
	{
		try
		{
			Invocation value = Current.Value;
			if (value == null || value.command != command || !AnimationSettings.Current.Allows("Eradicate"))
			{
				return null;
			}
			Invocation invocation = value;
			if (invocation.Visual == null)
			{
				invocation.Visual = EradicateVisual.Create(target, value.Count);
			}
			value.Visual?.Hit();
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
		return null;
	}

	[HarmonyPrefix]
	private static void Prefix(AttackCommand __instance, out Invocation? __state)
	{
		__state = null;
		if (!(__instance.ModelSource is Eradicate) || !AnimationSettings.Current.Allows("Eradicate"))
		{
			return;
		}
		try
		{
			if (!Bound.TryGetValue(__instance, out object _))
			{
				__instance.WithAttackerFx((Func<Node2D>)(() => PrepareStrike(__instance)));
				__instance.WithHitVfxNode((Func<Creature, Node2D>)((Creature target) => Hit(__instance, target)));
				Bound.Add(__instance, new object());
			}
			Current.Value = (__state = new Invocation(__instance, Current.Value));
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(Task<AttackCommand> __result, Invocation? __state)
	{
		if (__state != null)
		{
			Current.Value = __state.previous;
			Observe(__result, __state);
		}
	}

	private static async Task Observe(Task original, Invocation call)
	{
		bool cancelled = false;
		try
		{
			await original;
		}
		catch
		{
			cancelled = true;
		}
		try
		{
			call.Visual?.Finish(cancelled);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	[HarmonyFinalizer]
	private static Exception? Finalizer(Exception? __exception, Invocation? __state)
	{
		if (__state != null && Current.Value == __state)
		{
			Current.Value = __state.previous;
		}
		return __exception;
	}
}
[HarmonyPatch(typeof(Hook), "ModifyAttackHitCount")]
internal static class EradicateHitCountPatch
{
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	private static void Postfix(AttackCommand attackCommand, decimal __result)
	{
		try
		{
			EradicateAttackPatch.Invocation value = EradicateAttackPatch.Current.Value;
			if (value != null && value.command == attackCommand)
			{
				value.Count = (int)Math.Clamp(decimal.Ceiling(__result), 0m, 2147483647m);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(CombatHistory), "DamageReceived")]
internal static class EradicateDamagePatch
{
	[HarmonyPostfix]
	private static void Postfix(Creature receiver, DamageResult result, CardModel? cardSource)
	{
		try
		{
			if (cardSource is Eradicate && result.WasTargetKilled)
			{
				EradicateAttackPatch.Invocation value = EradicateAttackPatch.Current.Value;
				if (value != null && value.command.ModelSource == cardSource)
				{
					value.Visual?.Killed();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal sealed class EradicateSurface : IDisposable
{
	private static readonly GDScript Script = Compile();

	private readonly RefCounted _reader;

	internal readonly Node2D Body;

	internal bool Stunned
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("stunned"), Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsBool();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal static void Prepare()
	{
		_ = Script;
	}

	internal EradicateSurface(Node2D body, EradicateAssets.Target target)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		Body = body;
		Variant val = Script.New(Array.Empty<Variant>());
		try
		{
			_reader = (RefCounted)((Variant)(ref val)).AsGodotObject();
			Variant val2 = ((GodotObject)_reader).Call(StringName.op_Implicit("bind"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit((GodotObject)(object)body),
				Variant.op_Implicit(target.Surfaces.GetRawText()),
				Variant.op_Implicit(target.Clips.GetRawText())
			});
			try
			{
			}
			finally
			{
				((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal (Vector2[][] Points, Vector2[][] Clips, bool[] Active) Sample()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("sample"), Array.Empty<Variant>());
		try
		{
			Array val2 = ((Variant)(ref val)).AsGodotArray();
			try
			{
				Vector2[][] array = new Vector2[val2.Count][];
				Vector2[][] array2 = new Vector2[val2.Count][];
				bool[] array3 = new bool[val2.Count];
				for (int i = 0; i < val2.Count; i++)
				{
					Variant val3 = val2[i];
					try
					{
						Array val4 = ((Variant)(ref val3)).AsGodotArray();
						try
						{
							Variant val5 = val4[0];
							try
							{
								Variant val6 = val4[1];
								try
								{
									Variant val7 = val4[2];
									try
									{
										array3[i] = ((Variant)(ref val5)).AsBool();
										array[i] = ((Variant)(ref val6)).AsVector2Array();
										array2[i] = ((Variant)(ref val7)).AsVector2Array();
									}
									finally
									{
										((IDisposable)(Variant)(ref val7)/*cast due to .constrained prefix*/).Dispose();
									}
								}
								finally
								{
									((IDisposable)(Variant)(ref val6)/*cast due to .constrained prefix*/).Dispose();
								}
							}
							finally
							{
								((IDisposable)(Variant)(ref val5)/*cast due to .constrained prefix*/).Dispose();
							}
						}
						finally
						{
							((IDisposable)val4)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
					}
				}
				return (Points: array, Clips: array2, Active: array3);
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public void Dispose()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("release"), Array.Empty<Variant>());
		try
		{
			((GodotObject)_reader).Dispose();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static GDScript Compile()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		GDScript val = new GDScript
		{
			SourceCode = "extends RefCounted\nvar body\nvar skeleton\nvar data\nvar specs = []\nvar clips = []\nvar bones = {}\nvar slots = {}\nvar dirty = false\nfunc changed():\n    dirty = true\nfunc bind(value, text, clip_text):\n    body = value\n    skeleton = body.get_skeleton()\n    data = body.get_skeleton_data_res()\n    specs = JSON.parse_string(text)\n    clips = JSON.parse_string(clip_text)\n    data.changed.connect(changed)\n    if data.has_signal(\"_internal_spine_objects_invalidated\"):\n        data.connect(\"_internal_spine_objects_invalidated\",changed)\n    for spec in specs:\n        register(spec)\n        if spec.clip != null: register(spec.clip)\n    for clip in clips: register(clip)\nfunc register(spec):\n    var slot = skeleton.find_slot(spec.slot)\n    if slot == null: return\n    slots[spec.slot] = slot\n    for vertex in spec.vertices:\n        for weight in vertex:\n            if not bones.has(weight[0]): bones[weight[0]] = skeleton.find_bone(weight[0])\nfunc release():\n    if is_instance_valid(data):\n        if data.changed.is_connected(changed): data.changed.disconnect(changed)\n        if data.has_signal(\"_internal_spine_objects_invalidated\") and data.is_connected(\"_internal_spine_objects_invalidated\",changed):\n            data.disconnect(\"_internal_spine_objects_invalidated\",changed)\n    slots.clear()\n    bones.clear()\n    skeleton = null\n    data = null\n    body = null\nfunc active(spec, clip = false):\n    if not slots.has(spec.slot): return false\n    var slot = slots[spec.slot]\n    var attachment = slot.get_attachment()\n    return attachment != null and attachment.get_attachment_name() == spec.attachment and (clip or slot.get_color().a > .01) and slot.get_bone().is_active()\nfunc points(spec, poses):\n    var output = PackedVector2Array()\n    if not slots.has(spec.slot): return output\n    var deform = slots[spec.slot].get_deform()\n    for vertex in spec.vertices:\n        var p = Vector2.ZERO\n        for weight in vertex:\n            if not poses.has(weight[0]): return PackedVector2Array()\n            var offset = int(weight[4])\n            var local = Vector2(weight[1],weight[2])\n            if offset >= 0 and deform.size() > 0:\n                if offset+1 >= deform.size(): return PackedVector2Array()\n                var delta = Vector2(deform[offset],deform[offset+1])\n                local = local+delta if spec.weighted else delta\n            p += (poses[weight[0]]*local)*weight[3]\n        # The Godot Spine bone matrices already use canvas Y-down coordinates.\n        output.append(body.global_transform*p)\n    return output\nfunc sample():\n    var result = []\n    if dirty or not is_instance_valid(body) or body.get_skeleton_data_res() != data or body.get_skeleton() != skeleton:\n        return result\n    var poses = {}\n    for name in bones:\n        var b = bones[name]\n        if b != null:\n            poses[name] = Transform2D(Vector2(b.get_a(),b.get_c()),Vector2(b.get_b(),b.get_d()),Vector2(b.get_world_x(),b.get_world_y()))\n    var visible = body.is_visible_in_tree() and skeleton.get_color().a > .01\n    var live_clips = {}\n    if not clips.is_empty():\n        var current = null\n        for slot in skeleton.get_draw_order():\n            var name = slot.get_data().get_name()\n            for clip in clips:\n                if clip.slot == name and active(clip,true): current = clip\n            if current != null:\n                live_clips[name] = current\n                if current.end == name: current = null\n    for spec in specs:\n        var valid = visible and active(spec)\n        var p = points(spec,poses) if valid else PackedVector2Array()\n        var clip = PackedVector2Array()\n        if live_clips.has(spec.slot): clip = points(live_clips[spec.slot],poses)\n        elif spec.clip != null and active(spec.clip,true): clip = points(spec.clip,poses)\n        result.append([valid and p.size() == spec.vertices.size(),p,clip])\n    return result\nfunc stunned():\n    if dirty or not is_instance_valid(body): return false\n    var track = body.get_animation_state().get_current(0)\n    if track == null or track.get_animation() == null: return false\n    var name = track.get_animation().get_name()\n    return name.begins_with(\"stun\") or name == \"wake_up\""
		};
		if ((int)((Script)val).Reload(false) != 0)
		{
			throw new InvalidDataException("Eradicate native surface bridge");
		}
		return val;
	}
}
internal sealed class EradicateVisual : IDisposable
{
	internal sealed class Strike
	{
		internal float At;

		internal bool Hit;

		internal EradicateAssets.Needle[] Bank = Array.Empty<EradicateAssets.Needle>();

		internal readonly List<Pin> Pins = new List<Pin>();
	}

	internal sealed class Pin(EradicateAssets.Needle spec)
	{
		internal EradicateAssets.Needle Spec = spec;

		internal Vector2[] Last = Array.Empty<Vector2>();

		internal readonly Vector2[] Quad = (Vector2[])(object)new Vector2[4];
	}

	private static readonly List<EradicateVisual> Active = new List<EradicateVisual>();

	private readonly EradicateAssets.Target _data;

	private readonly EradicateAssets.Needle[] _bank;

	private readonly EradicateAssets.Needle[] _stunnedBank;

	private readonly NCombatRoom _room;

	private readonly Node2D _body;

	private readonly SceneTree _tree;

	private readonly Vector2[][] _lastPoints;

	private EradicateSurface? _surface;

	private readonly EradicateBatch _back;

	private readonly EradicateBatch _front;

	private readonly List<Strike> _strikes = new List<Strike>();

	private Strike? _pending;

	private Strike? _last;

	private float _age;

	private float _lastHit;

	private int _hitCount;

	private int _visible;

	private bool _ended;

	private bool _killed;

	private bool _disposed;

	private bool _bodyGone;

	internal static int Count => Active.Count;

	internal static long Starts { get; private set; }

	internal static long Hits { get; private set; }

	internal static long Bursts { get; private set; }

	internal static long CreatedPins { get; private set; }

	internal static int VisiblePins => Active.Sum((EradicateVisual v) => v._visible);

	internal static int RetainedPins => Active.Sum((EradicateVisual v) => v._strikes.Where((Strike s) => s.Hit).Sum((Strike s) => s.Pins.Count));

	internal int Planned { get; set; }

	internal static EradicateVisual? Create(Creature target, int hits)
	{
		if (AnimationSettings.Current.Allows("Eradicate"))
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				NCreature creatureNode = target.GetCreatureNode();
				if (creatureNode != null)
				{
					MonsterModel monster = target.Monster;
					EradicateAssets.Target target2 = EradicateAssets.Find(((monster != null) ? ((AbstractModel)monster).Id.Entry : null) ?? "");
					if (target2 == null)
					{
						return null;
					}
					Node2D val = FindBody((Node)(object)creatureNode.Body, "");
					bool flag = val == null;
					if (flag)
					{
						string id = target2.Id;
						bool flag2 = ((id == "crusher" || id == "rocket") ? true : false);
						flag = flag2;
					}
					if (flag)
					{
						val = FindBody((Node)(object)instance.Background, "kaiser_crab");
					}
					if (val == null)
					{
						return null;
					}
					EradicateVisual eradicateVisual = new EradicateVisual(creatureNode, val, instance, target2, hits);
					Active.Add(eradicateVisual);
					Starts++;
					return eradicateVisual;
				}
			}
		}
		return null;
	}

	internal static Node2D? FindBody(Node node, string rig)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = (Node2D)(object)((node is Node2D) ? node : null);
		if (val != null && ((GodotObject)node).HasMethod(StringName.op_Implicit("get_skeleton")))
		{
			Variant val2 = ((GodotObject)node).Call(StringName.op_Implicit("get_skeleton_data_res"), Array.Empty<Variant>());
			try
			{
				GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
				Resource val3 = (Resource)(object)((obj is Resource) ? obj : null);
				if (val3 != null && (rig.Length == 0 || val3.ResourcePath.Contains("/" + rig + "/", StringComparison.OrdinalIgnoreCase)))
				{
					return val;
				}
			}
			finally
			{
				((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		foreach (Node child in node.GetChildren(false))
		{
			Node2D val4 = FindBody(child, rig);
			if (val4 != null)
			{
				return val4;
			}
		}
		return null;
	}

	private EradicateVisual(NCreature target, Node2D body, NCombatRoom room, EradicateAssets.Target data, int hits)
	{
		_body = body;
		_room = room;
		_tree = ((Node)room).GetTree();
		_data = data;
		Planned = Math.Max(1, hits);
		_bank = data.Bank(Planned);
		_stunnedBank = data.Bank(Planned, stunned: true);
		_lastPoints = new Vector2[data.Surfaces.GetArrayLength()][];
		_surface = new EradicateSurface(body, data);
		_back = new EradicateBatch(body, rear: true);
		_front = new EradicateBatch(body, rear: false);
		_tree.ProcessFrame += Frame;
		((Node)_room).TreeExiting += Dispose;
		((Node)_body).TreeExiting += BodyExiting;
	}

	internal void Prepare(float delay)
	{
		if (_disposed || _pending != null)
		{
			return;
		}
		EradicateAssets.Needle[] array;
		if (_data.Burrowing)
		{
			EradicateSurface? surface = _surface;
			if (surface != null && surface.Stunned)
			{
				array = _stunnedBank;
				goto IL_0040;
			}
		}
		array = _bank;
		goto IL_0040;
		IL_0040:
		EradicateAssets.Needle[] array2 = array;
		Strike strike = new Strike
		{
			At = _age + Math.Max(0f, delay),
			Bank = array2
		};
		_strikes.Add(strike);
		_pending = strike;
		if (_hitCount + 1 >= Planned)
		{
			for (int i = array2.Length - 6; i < array2.Length; i++)
			{
				strike.Pins.Add(new Pin(array2[i]));
			}
		}
		else
		{
			strike.Pins.Add(new Pin(array2[_hitCount % Math.Max(1, array2.Length - 6)]));
		}
		CreatedPins += strike.Pins.Count;
		Sample();
	}

	internal void Hit()
	{
		if (!_disposed)
		{
			Prepare(0f);
			Strike pending = _pending;
			pending.At = _age;
			pending.Hit = true;
			_last = pending;
			_pending = null;
			_hitCount++;
			_lastHit = _age;
			Hits++;
			if (pending.Pins.Count == 6)
			{
				Bursts++;
			}
			Sample();
		}
	}

	internal void Killed()
	{
		if (!_disposed)
		{
			_killed = true;
			Promote();
		}
	}

	private void Promote()
	{
		if (_last != null && _last.Pins.Count != 6)
		{
			EradicateAssets.Needle[] bank = _last.Bank;
			_last.Pins[0].spec = bank[^6];
			for (int i = bank.Length - 5; i < bank.Length; i++)
			{
				_last.Pins.Add(new Pin(bank[i]));
			}
			CreatedPins += 5L;
			Bursts++;
			Sample();
		}
	}

	internal void Finish(bool cancelled)
	{
		if (!_disposed)
		{
			_ended = true;
			if (_pending != null)
			{
				_strikes.Remove(_pending);
				_pending = null;
			}
			if (_last == null || cancelled)
			{
				Dispose();
			}
			else
			{
				Promote();
			}
		}
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			if (!AnimationSettings.Current.Allows("Eradicate") || !GodotObject.IsInstanceValid((GodotObject)(object)_room) || NCombatRoom.Instance != _room)
			{
				Dispose();
			}
			else if (!_tree.Paused)
			{
				_age += (float)((Node)_room).GetProcessDeltaTime();
				if ((_ended && _age >= _lastHit + 0.77f) || (!_ended && _age - _lastHit > 30f))
				{
					Dispose();
				}
				else
				{
					Sample();
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			Dispose();
		}
	}

	private void Sample()
	{
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0420: Unknown result type (might be due to invalid IL or missing references)
		//IL_0425: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_043e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0444: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		(Vector2[][], Vector2[][], bool[])? tuple = _surface?.Sample();
		_back.Clear();
		_front.Clear();
		_visible = 0;
		if (tuple.HasValue)
		{
			(Vector2[][], Vector2[][], bool[]) valueOrDefault = tuple.GetValueOrDefault();
			for (int j = 0; j < valueOrDefault.Item3.Length; j++)
			{
				if (valueOrDefault.Item3[j])
				{
					_lastPoints[j] = valueOrDefault.Item1[j];
				}
			}
		}
		float num = (_ended ? Math.Clamp((_lastHit + 0.77f - _age) / 0.22f, 0f, 1f) : 1f);
		foreach (Strike strike in _strikes)
		{
			foreach (Pin pin in strike.Pins)
			{
				EradicateAssets.Needle spec = pin.spec;
				float num2 = _age - strike.At;
				if (num2 < -0.12f)
				{
					continue;
				}
				Vector2[] points = Array.Empty<Vector2>();
				Vector2[] array = Array.Empty<Vector2>();
				if (tuple.HasValue)
				{
					(Vector2[][], Vector2[][], bool[]) valueOrDefault2 = tuple.GetValueOrDefault();
					if (spec.Surface < valueOrDefault2.Item3.Length && valueOrDefault2.Item3[spec.Surface])
					{
						points = valueOrDefault2.Item1[spec.Surface];
						array = valueOrDefault2.Item2[spec.Surface];
						pin.Last = points;
						goto IL_01da;
					}
				}
				if (_killed && !_data.Burrowing)
				{
					points = _lastPoints[spec.Surface] ?? pin.Last;
				}
				goto IL_01da;
				IL_01da:
				if (points.Length == 0 || spec.Ids.Any((int i) => i >= points.Length))
				{
					continue;
				}
				Vector2 val = points[spec.Ids[0]];
				Vector2 val2 = points[spec.Ids[1]];
				Vector2 val3 = points[spec.Ids[2]];
				Vector2 val4 = val * spec.Weights[0] + val2 * spec.Weights[1] + val3 * spec.Weights[2];
				if (array.Length <= 2 || Geometry2D.IsPointInPolygon(val4, array) || _killed)
				{
					Vector2 val5 = (val2 - val) * spec.Coeff[0] + (val3 - val) * spec.Coeff[1];
					float num3 = ((Vector2)(ref val5)).Length();
					if (!(num3 < 1E-05f))
					{
						val5 /= num3;
						float num4 = spec.Length * num3;
						float num5 = ((num2 < 0f) ? (MathF.Pow((0f - num2) / 0.12f, 2f) * num4 * 2.5f) : 0f);
						float num6 = Math.Clamp(num2 / 0.1f, 0f, 1f);
						float num7 = 0.65f * (1f - (1f - num6) * (1f - num6));
						float num8 = num4 * (1f - num7);
						float num9 = num4 * 38f / 256f * 1.35f * ((spec.Tier == "core") ? 1.1f : 0.85f);
						Vector2 val6 = new Vector2(0f - val5.Y, val5.X) * num9 * 0.5f;
						Vector2 val7 = val4 + val5 * num5;
						pin.Quad[0] = val7 - val6;
						pin.Quad[1] = val7 + val6;
						pin.Quad[2] = val7 + val5 * num8 + val6;
						pin.Quad[3] = val7 + val5 * num8 - val6;
						float alpha = num * (spec.Rear ? 0.82f : ((spec.Tier == "middle") ? 0.94f : 1f));
						(spec.Rear ? _back : _front).Quad(pin.Quad, num7, alpha, _data.Burrowing ? array : Array.Empty<Vector2>());
						_visible++;
					}
				}
			}
		}
		_back.Commit();
		_front.Commit();
	}

	private void BodyExiting()
	{
		if (!_disposed && !_bodyGone)
		{
			if (!_killed)
			{
				Dispose();
				return;
			}
			_bodyGone = true;
			_surface?.Dispose();
			_surface = null;
			_back.KeepIn((Node)(object)_room.CombatVfxContainer);
			_front.KeepIn((Node)(object)_room.CombatVfxContainer);
		}
	}

	internal static void Reset()
	{
		EradicateVisual[] array = Active.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Dispose();
		}
	}

	internal static void ResetDisabled()
	{
		if (!AnimationSettings.Current.Allows("Eradicate"))
		{
			Reset();
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Active.Remove(this);
			_tree.ProcessFrame -= Frame;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_room))
			{
				((Node)_room).TreeExiting -= Dispose;
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_body))
			{
				((Node)_body).TreeExiting -= BodyExiting;
			}
			_surface?.Dispose();
			_surface = null;
			_back.Dispose();
			_front.Dispose();
			_strikes.Clear();
		}
	}
}
internal sealed class EradicateBatch : IDisposable
{
	private readonly Node2D _node;

	private readonly ShaderMaterial _material;

	private readonly List<Vector2> _points = new List<Vector2>(256);

	private readonly List<Vector2> _uvs = new List<Vector2>(256);

	private readonly List<Color> _colors = new List<Color>(256);

	private int[] _indices = Enumerable.Range(0, 256).ToArray();

	private static readonly Vector2[] Unit = (Vector2[])(object)new Vector2[4]
	{
		new Vector2(0f, 0f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f)
	};

	private static readonly int[] QuadIndices = new int[6] { 0, 1, 2, 2, 3, 0 };

	internal EradicateBatch(Node2D body, bool rear)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		_material = new ShaderMaterial
		{
			Shader = EradicateAssets.Shader
		};
		_node = new Node2D
		{
			Name = StringName.op_Implicit(rear ? "NecroEradicateBack" : "NecroEradicateFront"),
			Material = (Material)(object)_material,
			TextureFilter = (TextureFilterEnum)2,
			ZIndex = ((CanvasItem)body).ZIndex
		};
		Node parent = ((Node)body).GetParent();
		parent.AddChild((Node)(object)_node, false, (InternalMode)0);
		parent.MoveChild((Node)(object)_node, ((Node)body).GetIndex(false) + ((!rear) ? 1 : 0));
		((CanvasItem)_node).Draw += Draw;
		((CanvasItem)_node).Hide();
	}

	internal void Clear()
	{
		_points.Clear();
		_uvs.Clear();
		_colors.Clear();
	}

	private void Vertex(Vector2 p, Vector2 uv, Color data)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_points.Add(p);
		_uvs.Add(uv);
		_colors.Add(data);
	}

	internal void Quad(Vector2[] p, float cut, float alpha, Vector2[] clip)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = p[3] - p[0];
		float num = ((Vector2)(ref val)).Length() / Math.Max(1E-06f, 1f - cut);
		if (num <= 1E-06f)
		{
			return;
		}
		float num2 = cut;
		val = p[1] - p[0];
		Color data = default(Color);
		((Color)(ref data))..ctor(num2, ((Vector2)(ref val)).Length() / num, 1f / num, alpha);
		int[] array;
		if (clip.Length > 2)
		{
			foreach (Vector2[] item in Geometry2D.IntersectPolygons(p, clip))
			{
				array = Geometry2D.TriangulatePolygon(item);
				foreach (int num3 in array)
				{
					Vertex(item[num3], Uv(item[num3]), data);
				}
			}
			return;
		}
		array = QuadIndices;
		foreach (int num4 in array)
		{
			Vertex(p[num4], new Vector2(Unit[num4].X, cut + (1f - cut) * Unit[num4].Y), data);
		}
		Vector2 Uv(Vector2 v)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val2 = p[1] - p[0];
			Vector2 val3 = p[3] - p[0];
			Vector2 val4 = v - p[0];
			return new Vector2(((Vector2)(ref val4)).Dot(val2) / Math.Max(1E-06f, ((Vector2)(ref val2)).LengthSquared()), cut + (1f - cut) * ((Vector2)(ref val4)).Dot(val3) / Math.Max(1E-06f, ((Vector2)(ref val3)).LengthSquared()));
		}
	}

	internal void Commit()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_node.GlobalTransform = Transform2D.Identity;
		((CanvasItem)_node).Visible = _points.Count > 0;
		if (_indices.Length < _points.Count)
		{
			_indices = Enumerable.Range(0, _points.Count * 2).ToArray();
		}
		((CanvasItem)_node).QueueRedraw();
	}

	private void Draw()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (_points.Count > 0)
		{
			RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)_node).GetCanvasItem(), (ReadOnlySpan<int>)_indices.AsSpan(0, _points.Count), (ReadOnlySpan<Vector2>)CollectionsMarshal.AsSpan(_points), (ReadOnlySpan<Color>)CollectionsMarshal.AsSpan(_colors), (ReadOnlySpan<Vector2>)CollectionsMarshal.AsSpan(_uvs), ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, ((Resource)EradicateAssets.Texture).GetRid(), -1);
		}
	}

	internal void KeepIn(Node parent)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)_node))
		{
			((Node)_node).Reparent(parent, false);
			_node.GlobalTransform = Transform2D.Identity;
		}
	}

	public void Dispose()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_node))
		{
			((CanvasItem)_node).Hide();
			((CanvasItem)_node).Material = null;
			((Node)_node).QueueFree();
		}
		((GodotObject)_material).Dispose();
	}
}
[HarmonyPatch(typeof(OstyCmd), "Summon")]
internal static class FriendshipRevival
{
	[HarmonyPostfix]
	private static void Postfix(Player summoner, Task __result)
	{
		if (AnimationSettings.Current.Allows("Friendship"))
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null && summoner.Creature.Powers.Any((PowerModel p) => p is FriendshipPower && p.Amount > 0))
			{
				RestoreAfterSummon(summoner, instance, __result);
			}
		}
	}

	private static async Task RestoreAfterSummon(Player player, NCombatRoom room, Task original)
	{
		try
		{
			await original;
		}
		catch
		{
			return;
		}
		try
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)room) || NCombatRoom.Instance != room)
			{
				return;
			}
			Creature osty = player.Osty;
			NCreature obj2 = ((osty != null) ? osty.GetCreatureNode() : null);
			Task task = ((obj2 != null) ? obj2.DeathAnimationTask : null);
			if (task != null && !task.IsCompleted)
			{
				try
				{
					await task;
				}
				catch
				{
					return;
				}
			}
			VisualSession.RestoreFriendship(player, room);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal static class Hooks
{
	internal static readonly string[] Cards = new string[16]
	{
		"Scourge", "GraveWarden", "Invoke", "Friendship", "NegativePulse", "Neurosurge", "Deathbringer", "Dirge", "CaptureSpirit", "EndOfDays",
		"BansheesCry", "Unleash", "Protector", "Lethality", "ReaperForm", "Eradicate"
	};
}
[HarmonyPatch]
internal static class CardVisualPatch
{
	private sealed record State(CardVisualContext? Previous, CardVisualContext Call);

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return Hooks.Cards.Select((string name) => typeof(CardModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Cards." + name)?.GetMethod("OnPlay", BindingFlags.Instance | BindingFlags.NonPublic)).OfType<MethodBase>();
	}

	[HarmonyPrefix]
	private static void Prefix(CardModel __instance, out State __state)
	{
		CardVisualContext cardVisualContext = new CardVisualContext
		{
			Card = ((object)__instance).GetType().Name
		};
		__state = new State(CardVisualContext.Current.Value, cardVisualContext);
		CardVisualContext.Current.Value = cardVisualContext;
	}

	[HarmonyPostfix]
	private static void Postfix(CardModel __instance, CardPlay cardPlay, Task __result, State? __state, bool __runOriginal)
	{
		if ((object)__state == null)
		{
			return;
		}
		if (!__runOriginal)
		{
			CardVisualContext.Current.Value = __state.Previous;
			return;
		}
		try
		{
			long starts = VisualSession.Starts;
			bool flag = !__result.IsFaulted && !__result.IsCanceled;
			if (flag)
			{
				bool flag2 = ((__instance is BansheesCry || __instance is Unleash || __instance is Protector || __instance is Eradicate) ? true : false);
				flag = !flag2;
			}
			if (flag)
			{
				Creature target = cardPlay.Target;
				VisualSession.Start(__instance, initialAttack: false, (target != null) ? target.GetCreatureNode() : null);
			}
			__state.Call.CompleteStart(VisualSession.Starts > starts);
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
		finally
		{
			CardVisualContext.Current.Value = __state.Previous;
		}
	}

	[HarmonyFinalizer]
	private static void Finalizer(State? __state)
	{
		if ((object)__state != null)
		{
			CardVisualContext.Current.Value = __state.Previous;
		}
	}
}
internal sealed class CardVisualContext
{
	internal static readonly AsyncLocal<CardVisualContext?> Current = new AsyncLocal<CardVisualContext>();

	internal string Card = "";

	internal bool Started;

	private List<CanvasItem>? _pending;

	internal void Observe(CanvasItem node)
	{
		if (Started)
		{
			NativeVfxVisibility.HideTransient(node);
		}
		else
		{
			(_pending ?? (_pending = new List<CanvasItem>())).Add(node);
		}
	}

	internal void CompleteStart(bool started)
	{
		Started |= started;
		if (Started)
		{
			List<CanvasItem> pending = _pending;
			if (pending != null)
			{
				foreach (CanvasItem item in pending)
				{
					NativeVfxVisibility.HideTransient(item);
				}
			}
		}
		_pending = null;
	}
}
[HarmonyPatch(typeof(NLargeMagicMissileVfx), "Create")]
internal static class EndNativeMissilePatch
{
	[HarmonyPostfix]
	private static void Postfix(NLargeMagicMissileVfx? __result)
	{
		CardVisualContext value = CardVisualContext.Current.Value;
		if (value != null && value.Card == "EndOfDays" && value.Started && __result != null)
		{
			((CanvasItem)__result).Hide();
		}
	}
}
[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
internal static class BodyActionPatch
{
	[HarmonyPrefix]
	private static void Prefix(Creature creature, string triggerName)
	{
		try
		{
			if (!(triggerName == "Idle") && !(triggerName == "Relaxed"))
			{
				VisualSession.InterruptBody(creature.GetCreatureNode(), triggerName);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class AttackVisualPatch
{
	private sealed class State
	{
		internal string? Original;

		internal bool Suppressed;
	}

	private static readonly ConditionalWeakTable<AttackCommand, State> Bound = new ConditionalWeakTable<AttackCommand, State>();

	private static readonly FieldInfo? Target = AccessTools.Field(typeof(AttackCommand), "_singleTarget");

	[HarmonyPrefix]
	private static void Prefix(AttackCommand __instance)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		AbstractModel modelSource = __instance.ModelSource;
		bool flag = ((modelSource is BansheesCry || modelSource is Unleash || modelSource is Protector) ? true : false);
		if (!flag || Bound.TryGetValue(__instance, out State _))
		{
			return;
		}
		CardModel card = (CardModel)__instance.ModelSource;
		State state = new State();
		Bound.Add(__instance, state);
		__instance.WithAttackerFx((Func<Node2D>)delegate
		{
			try
			{
				long starts = VisualSession.Starts;
				CardModel card2 = card;
				object? obj = Target?.GetValue(__instance);
				object? obj2 = ((obj is Creature) ? obj : null);
				VisualSession.Start(card2, initialAttack: true, (obj2 != null) ? ((Creature)obj2).GetCreatureNode() : null);
				if (VisualSession.Starts > starts && !state.Suppressed)
				{
					CardVisualContext.Current.Value?.CompleteStart(started: true);
					state.Original = __instance.HitVfx;
					state.Suppressed = true;
					__instance.WithHitFx((string)null, __instance.HitSfx, __instance.TmpHitSfx);
				}
			}
			catch (Exception error)
			{
				ModEntry.Log(error);
			}
			return (Node2D?)null;
		});
	}

	[HarmonyPostfix]
	private static void Postfix(AttackCommand __instance, Task<AttackCommand> __result)
	{
		if (Bound.TryGetValue(__instance, out State value))
		{
			Restore(__instance, __result, value);
		}
	}

	private static async Task Restore(AttackCommand command, Task original, State state)
	{
		try
		{
			await original;
		}
		catch
		{
		}
		finally
		{
			if (state.Suppressed)
			{
				if (command.HitVfx == null)
				{
					command.WithHitFx(state.Original, command.HitSfx, command.TmpHitSfx);
				}
				state.Suppressed = false;
				state.Original = null;
			}
		}
	}
}
[HarmonyPatch(typeof(SummonNextTurnPower), "AfterPlayerTurnStart")]
internal static class InvokeTurnPatch
{
	[HarmonyPrefix]
	private static void Prefix(SummonNextTurnPower __instance, Player player)
	{
		try
		{
			if (player == ((PowerModel)__instance).Owner.Player && ((PowerModel)__instance).AmountOnTurnStart != 0)
			{
				VisualSession.ResolveInvoke(((PowerModel)__instance).Owner.GetCreatureNode());
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal sealed class InvokeEffect : IVisualEffect, IDisposable
{
	private readonly record struct State(float Opacity, float Energy, float Front, float Clock, float Burst, float Consume, float Fade, float Peak);

	private readonly ClipPlayer _visual;

	private float _age;

	private float _origin;

	private float _burst = 0.25f;

	private float _reveal = 0.25f;

	private float _resolve = -1f;

	private float _startFade = 1f;

	private float _startOpacity;

	private float _startEnergy;

	private float _startPeak;

	private State _state;

	private static readonly StringName Phase = StringName.op_Implicit("p_u_phase");

	private static readonly StringName Lifecycle = StringName.op_Implicit("p_u_lifecycle");

	private static readonly string[] Names = new string[7] { "outer", "rest", "inner", "active", "highlight", "soul", "soulCore" };

	private static readonly StringName[] Keys = ((IEnumerable<string>)Names).Select((Func<string, StringName>)((string n) => new StringName("p_u_" + n))).ToArray();

	private static readonly Vector4[] Base = (Vector4[])(object)new Vector4[7]
	{
		new Vector4(0.35f, 0.32f, 0.6f, 1f),
		new Vector4(0.5f, 0.4f, 0.73f, 1f),
		new Vector4(0.65f, 0.53f, 0.84f, 1f),
		new Vector4(0.7f, 0.58f, 0.89f, 1f),
		new Vector4(0.9f, 0.87f, 0.98f, 1f),
		new Vector4(0.61f, 0.61f, 0.82f, 1f),
		new Vector4(0.91f, 0.91f, 1f, 1f)
	};

	private static readonly Vector4[] Peak = (Vector4[])(object)new Vector4[7]
	{
		new Vector4(0.42f, 0.23f, 0.77f, 1f),
		new Vector4(0.52f, 0.27f, 0.86f, 1f),
		new Vector4(0.6317645f, 0.175f, 0.965f, 1f),
		new Vector4(0.6317645f, 0.155f, 0.97f, 1f),
		new Vector4(0.85f, 0.77f, 0.99f, 1f),
		new Vector4(0.5967645f, 0.2f, 0.935f, 1f),
		new Vector4(0.89f, 0.84f, 1f, 1f)
	};

	public float Duration
	{
		get
		{
			if (!(_resolve < 0f))
			{
				return _resolve + 1.55f;
			}
			return float.PositiveInfinity;
		}
	}

	public bool Hold => _resolve < 0f;

	internal InvokeEffect(NCreature hero, NCreature osty, NCombatRoom room)
	{
		_visual = new ClipPlayer(ClipAssets.Get("Invoke"), hero, osty, (Node)(object)room.CombatVfxContainer);
		_visual.Advance(1f);
		Advance(0f);
	}

	private static float Smooth(float a, float b, float x)
	{
		float num = Math.Clamp((x - a) / (b - a), 0f, 1f);
		return num * num * (3f - 2f * num);
	}

	private static float ColorPeak(float age)
	{
		if (!(age < 0f))
		{
			return 0.725f * Smooth(0f, 0.07f, age) * (1f - Smooth(0.22f, 0.58f, age));
		}
		return 0f;
	}

	internal void Recast()
	{
		bool flag = _state.Opacity > 0f || _resolve < 0f;
		if (!flag)
		{
			_origin = _age;
		}
		_burst = (_reveal = _age + (flag ? 0f : 0.25f));
		_resolve = -1f;
		_startFade = (flag ? _state.Fade : 1f);
		_startOpacity = (flag ? _state.Opacity : 0f);
		_startEnergy = (flag ? _state.Energy : 0f);
		_startPeak = (flag ? _state.Peak : 0f);
	}

	internal void Resolve()
	{
		if (_resolve < 0f)
		{
			_resolve = _age;
		}
	}

	public void CancelBody()
	{
	}

	public void Advance(float age)
	{
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		_age = age;
		float num = age - _burst;
		float clock = MathF.Max(0f, age - _origin - 0.25f);
		if (_resolve >= 0f)
		{
			float num2 = age - _resolve;
			float num3 = 1f - Smooth(0.85f, 1.55f, num2);
			float num4 = Smooth(0f, 0.09f, num2);
			_state = new State((0.58f + 0.38f * num4) * num3, num4 * num3, 1.15f * Smooth(0f, 0.85f, num2), clock, num2, 1f, 1f - num3, ColorPeak(num2));
		}
		else
		{
			float fade = _startFade * (1f - Smooth(0f, 0.35f, age - _reveal));
			if (num < 0f)
			{
				_state = new State(0f, 0f, -1f, clock, -1f, 0f, fade, 0f);
			}
			else if (num < 1.1f)
			{
				float num5 = MathF.Max(_startEnergy * (1f - Smooth(0f, 0.2f, num)), (1f - Smooth(0.2f, 1.1f, num)) * Smooth(0f, 0.07f, num));
				_state = new State(Mathf.Lerp(_startOpacity, 0.58f + 0.38f * num5, Smooth(0f, 0.14f, num)), num5, 1.15f * Smooth(0f, 1.1f, num), clock, num, 0f, fade, MathF.Max(_startPeak * (1f - Smooth(0f, 0.07f, num)), ColorPeak(num)));
			}
			else
			{
				_state = new State(0.58f * (0.96f + 0.04f * MathF.Sin((num - 1.1f) * 2f)), 0f, -1f, clock, -1f, 0f, fade, 0f);
			}
		}
		State state = _state;
		_visual.Uniform("u_phase", Phase, Variant.op_Implicit(new Vector4(state.Opacity, state.Energy, state.Front, state.Clock)));
		_visual.Uniform("u_lifecycle", Lifecycle, Variant.op_Implicit(new Vector4(state.Burst, state.Consume, state.Fade, 0f)));
		float num6 = 1f - 0.075f * state.Energy;
		for (int i = 0; i < Names.Length; i++)
		{
			Vector4 val = ((Vector4)(ref Base[i])).Lerp(Peak[i], state.Peak);
			val.X *= num6;
			val.Y *= num6;
			val.Z *= num6;
			_visual.Uniform("u_" + Names[i], Keys[i], Variant.op_Implicit(val));
		}
	}

	public void Dispose()
	{
		_visual.Dispose();
	}
}
internal interface IVisualEffect : IDisposable
{
	float Duration { get; }

	bool Hold { get; }

	void Advance(float age);

	void CancelBody();
}
internal sealed class LethalityEffect : IVisualEffect, IDisposable
{
	private sealed class Mesh : IDisposable
	{
		internal readonly Node2D Node = new Node2D
		{
			TextureFilter = (TextureFilterEnum)2
		};

		internal readonly ShaderMaterial Material = new ShaderMaterial();

		internal readonly Vector2[] Points;

		internal readonly Vector2[] Uvs;

		private readonly int[] _indices;

		internal Mesh(ClipAssets assets, ClipAssets.DrawSpec draw, Node parent)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_0160: Unknown result type (might be due to invalid IL or missing references)
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01db: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			Points = (Vector2[])(object)new Vector2[draw.Count];
			Uvs = (Vector2[])(object)new Vector2[Points.Length];
			_indices = draw.Indices;
			for (int i = 0; i < Points.Length; i++)
			{
				Uvs[i] = new Vector2(draw.Uvs[i * 2], draw.Uvs[i * 2 + 1]);
			}
			Material.Shader = assets.Shaders[draw.Shader];
			ClipAssets.Uniform[] compiled = draw.Compiled;
			for (int j = 0; j < compiled.Length; j++)
			{
				ClipAssets.Uniform uniform = compiled[j];
				switch (uniform.Kind)
				{
				case -1:
					Material.SetShaderParameter(uniform.Name, Variant.op_Implicit((GodotObject)(object)(assets.Textures[uniform.Texture] ?? throw new InvalidDataException("Missing ornament texture"))));
					break;
				case 0:
					Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(uniform.Boolean));
					break;
				case 1:
					Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(uniform.Value.X));
					break;
				case 2:
					Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(new Vector2(uniform.Value.X, uniform.Value.Y)));
					break;
				case 3:
					Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(new Vector3(uniform.Value.X, uniform.Value.Y, uniform.Value.Z)));
					break;
				case 4:
					Material.SetShaderParameter(uniform.Name, Variant.op_Implicit(uniform.Value));
					break;
				}
			}
			((CanvasItem)Node).Material = (Material)(object)Material;
			((CanvasItem)Node).Draw += Draw;
			parent.AddChild((Node)(object)Node, false, (InternalMode)0);
		}

		private void Draw()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			Rid canvasItem = ((CanvasItem)Node).GetCanvasItem();
			ReadOnlySpan<int> readOnlySpan = _indices;
			ReadOnlySpan<Vector2> readOnlySpan2 = Points;
			Color reference = Colors.White;
			RenderingServer.CanvasItemAddTriangleArray(canvasItem, readOnlySpan, readOnlySpan2, new ReadOnlySpan<Color>(in reference), (ReadOnlySpan<Vector2>)Uvs, ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
		}

		public void Dispose()
		{
			((CanvasItem)Node).Material = null;
			((GodotObject)Material).Dispose();
		}
	}

	private readonly record struct Timing(float Delay, float Duration, float Stagger, float Settle, float FadeIn)
	{
		internal float End => Delay + Duration + Stagger + Settle;

		internal static Timing Read(JsonElement e)
		{
			return new Timing(e.GetProperty("delay").GetSingle(), e.GetProperty("duration").GetSingle(), e.GetProperty("stagger").GetSingle(), e.GetProperty("settle").GetSingle(), e.GetProperty("fadeIn").GetSingle());
		}

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Delay = ");
			builder.Append(Delay.ToString());
			builder.Append(", Duration = ");
			builder.Append(Duration.ToString());
			builder.Append(", Stagger = ");
			builder.Append(Stagger.ToString());
			builder.Append(", Settle = ");
			builder.Append(Settle.ToString());
			builder.Append(", FadeIn = ");
			builder.Append(FadeIn.ToString());
			return true;
		}
	}

	private readonly Node2D _body;

	private readonly Node2D _root;

	private readonly SpineValues _values;

	private readonly Callable _pose;

	private readonly Mesh _fire;

	private readonly Mesh _bones;

	private readonly Mesh _soul;

	private readonly JsonElement _recipe;

	private readonly Vector4 _originalOuter;

	private readonly Vector4 _originalInner;

	private float _soulWeight;

	private readonly Vector2 _pivot;

	private readonly Vector2 _origin;

	private readonly Vector2 _pixelX;

	private readonly Vector2 _pixelY;

	private readonly Vector2 _size;

	private readonly Vector4 _fireBounds;

	private readonly float _fireScale;

	private readonly float _fireSpeed;

	private readonly float _fireOpacity;

	private readonly Timing _boneTime;

	private readonly Timing _fireTime;

	private readonly float _deathFade;

	private float _age;

	private float _recast = -10f;

	private float _death = -1f;

	private float _fireAlpha;

	private float _boneAlpha;

	private bool _formed;

	private bool _disposed;

	private Rect2 _bounds;

	private static readonly StringName Opacity = StringName.op_Implicit("p_u_opacity");

	private static readonly StringName Clock = StringName.op_Implicit("effect_time");

	public float Duration
	{
		get
		{
			if (!_disposed)
			{
				if (!(_death < 0f))
				{
					return _death + _deathFade;
				}
				return float.PositiveInfinity;
			}
			return 0f;
		}
	}

	public bool Hold
	{
		get
		{
			if (!_disposed)
			{
				return _death < 0f;
			}
			return false;
		}
	}

	internal static int Live { get; private set; }

	internal int GeometryUpdates { get; private set; }

	internal Vector2[] BonePoints => _bones.Points;

	internal Vector2[] FirePoints => _fire.Points;

	internal Vector2 Opacities => new Vector2(_fireAlpha, _boneAlpha);

	internal Transform2D Transform => _root.GlobalTransform;

	internal Rect2 Bounds => _root.GlobalTransform * _bounds;

	internal LethalityEffect(Node2D body)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Expected O, but got Unknown
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		_body = body;
		_values = new SpineValues(body);
		ClipAssets clipAssets = ClipAssets.Get("LethalityPrograms");
		_recipe = clipAssets.Data.Extra;
		JsonElement property = _recipe.GetProperty("bindings").GetProperty(ClipAssets.Version);
		_origin = V(property.GetProperty("origin"));
		_pixelX = V(property.GetProperty("pixelX"));
		_pixelY = V(property.GetProperty("pixelY"));
		_pivot = V(_recipe.GetProperty("pivot"));
		JsonElement property2 = _recipe.GetProperty("texture");
		_size = new Vector2(property2.GetProperty("width").GetSingle(), property2.GetProperty("height").GetSingle());
		_boneTime = Timing.Read(_recipe.GetProperty("timing"));
		_deathFade = _recipe.GetProperty("timing").GetProperty("deathFade").GetSingle();
		JsonElement property3 = _recipe.GetProperty("backfire");
		_fireTime = Timing.Read(property3.GetProperty("timing"));
		JsonElement property4 = property3.GetProperty("bounds");
		_fireBounds = new Vector4(property4[0].GetSingle(), property4[1].GetSingle(), property4[2].GetSingle(), property4[3].GetSingle());
		_fireScale = property3.GetProperty("scale").GetSingle();
		_fireSpeed = property3.GetProperty("speed").GetSingle();
		_fireOpacity = property3.GetProperty("opacity").GetSingle();
		_root = new Node2D
		{
			Name = StringName.op_Implicit("NecroLethality")
		};
		Node parent = ((Node)body).GetParent();
		parent.AddChild((Node)(object)_root, false, (InternalMode)0);
		parent.MoveChild((Node)(object)_root, ((Node)body).GetIndex(false));
		_fire = new Mesh(clipAssets, clipAssets.Data.Frames[0][0], (Node)(object)_root);
		_bones = new Mesh(clipAssets, clipAssets.Data.Frames[0][1], (Node)(object)_root);
		ClipAssets clipAssets2 = ClipAssets.Get("ReaperLethalityPrograms");
		_soul = new Mesh(clipAssets2, clipAssets2.Data.Frames[0][0], (Node)(object)_root);
		((CanvasItem)_soul.Node).Hide();
		Variant shaderParameter = _fire.Material.GetShaderParameter(StringName.op_Implicit("p_OuterColor"));
		_originalOuter = ((Variant)(ref shaderParameter)).AsVector4();
		shaderParameter = _fire.Material.GetShaderParameter(StringName.op_Implicit("p_InnerColor"));
		_originalInner = ((Variant)(ref shaderParameter)).AsVector4();
		_pose = Callable.From<Variant>((Action<Variant>)delegate
		{
			Pose();
		});
		if (((GodotObject)body).HasSignal(StringName.op_Implicit("world_transforms_changed")))
		{
			((GodotObject)body).Connect(StringName.op_Implicit("world_transforms_changed"), _pose, 0u);
		}
		BodyDisplay.Add(body, _root);
		BodyDisplay.AddResident(body, this);
		Live++;
		Advance(0f);
		static Vector2 V(JsonElement x)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			return new Vector2(x[0].GetSingle(), x[1].GetSingle());
		}
	}

	internal void Recast()
	{
		if (!(_death >= 0f))
		{
			_recast = _age;
			_formed = false;
		}
	}

	internal void Die()
	{
		if (_death < 0f)
		{
			_death = _age;
		}
	}

	public void CancelBody()
	{
	}

	private static float Ease(float x)
	{
		x = Math.Clamp(x, 0f, 1f);
		return x * x * (3f - 2f * x);
	}

	private void Geometry(Mesh mesh, Timing t, bool fire, float pulse)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		float num = Ease((_age - t.Delay) / t.Duration);
		float num2 = _age - t.Delay - t.Duration - t.Stagger;
		float num3 = ((num2 > 0f && num2 < t.Settle) ? (MathF.Sin((float)Math.PI * num2 / t.Settle) * 0.018f) : 0f);
		float num4 = 0.12f + 0.88f * num + num3 + pulse;
		for (int i = 0; i < mesh.Points.Length; i++)
		{
			Vector2 val = mesh.Uvs[i];
			Vector2 val2 = (fire ? (_pivot + (new Vector2(_fireBounds.X + val.X * _fireBounds.Z, _fireBounds.Y + val.Y * _fireBounds.W) - _pivot) * _fireScale) : ((mesh == _soul) ? (new Vector2(-14f, -14f) + val * (_size + new Vector2(28f, 28f))) : (val * _size)));
			float num5 = Ease((_age - t.Delay - t.Stagger * Ease(val2.X / _size.X)) / t.Duration);
			float num6 = 0.025f + 0.975f * num5 + num3 + pulse;
			Vector2 val3 = val2 - _pivot;
			((Vector2)(ref val2))..ctor(_pivot.X + val3.X * num4, _pivot.Y + val3.Y * num6 + (1f - num) * 0.05f * MathF.Abs(val3.X));
			val2 = _origin + _pixelX * val2.X + _pixelY * val2.Y;
			mesh.Points[i] = val2;
			_bounds = ((Rect2)(ref _bounds)).Expand(val2);
		}
		((CanvasItem)mesh.Node).QueueRedraw();
	}

	internal void Pose()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed)
		{
			if (!_values.Current)
			{
				Dispose();
				return;
			}
			_root.GlobalTransform = (Transform2D)(((??)BodyDisplay.Bone(_body)) ?? (_body.GlobalTransform * _values.Matrix("body_upper")));
			float num = BodyDisplay.Opacity(_body) * _values.TorsoOpacity;
			_fire.Material.SetShaderParameter(Opacity, Variant.op_Implicit(_fireAlpha * num));
			_bones.Material.SetShaderParameter(Opacity, Variant.op_Implicit(_boneAlpha * num));
			_soul.Material.SetShaderParameter(Opacity, Variant.op_Implicit(_boneAlpha * num * 0.45f * _soulWeight * (1f + 0.025f * MathF.Sin(_age * 1.7f))));
		}
	}

	public void Advance(float age)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed)
		{
			_age = age;
			float pulse = ((_death < 0f) ? (MathF.Sin((float)Math.PI * Math.Clamp((age - _recast) / 0.4f, 0f, 1f)) * 0.024f) : 0f);
			if (age - _recast >= 0.4f)
			{
				pulse = 0f;
			}
			if (!_formed)
			{
				_bounds = new Rect2(_origin, Vector2.Zero);
				Geometry(_fire, _fireTime, fire: true, pulse);
				Geometry(_bones, _boneTime, fire: false, pulse);
				Geometry(_soul, _boneTime, fire: false, pulse);
				GeometryUpdates++;
				_formed = age >= Math.Max(_boneTime.End, _fireTime.End) && (age - _recast >= 0.4f || _death >= 0f);
			}
			if (_death < 0f)
			{
				_soulWeight = ReaperFormEffect.For(_body)?.SoulWeight ?? 0f;
			}
			((CanvasItem)_soul.Node).Visible = _soulWeight > 0f;
			_fire.Material.SetShaderParameter(StringName.op_Implicit("p_OuterColor"), Variant.op_Implicit(((Vector4)(ref _originalOuter)).Lerp(new Vector4(0.16f, 0.78f, 0.7f, 1f), _soulWeight)));
			_fire.Material.SetShaderParameter(StringName.op_Implicit("p_InnerColor"), Variant.op_Implicit(((Vector4)(ref _originalInner)).Lerp(new Vector4(0.015f, 0.1f, 0.085f, 1f), _soulWeight)));
			float num = ((_death < 0f) ? 1f : (1f - Ease((age - _death) / _deathFade)));
			_fire.Material.SetShaderParameter(Clock, Variant.op_Implicit(age * _fireSpeed));
			_fireAlpha = Ease((age - _fireTime.Delay) / _fireTime.FadeIn) * num * _fireOpacity;
			_boneAlpha = Ease((age - _boneTime.Delay) / _boneTime.FadeIn) * num;
			Pose();
		}
	}

	public void Dispose()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed)
		{
			_disposed = true;
			Live--;
			BodyDisplay.RemoveResident(_body, this);
			BodyDisplay.Remove(_body, _root);
			if (GodotObject.IsInstanceValid((GodotObject)(object)_body) && ((GodotObject)_body).IsConnected(StringName.op_Implicit("world_transforms_changed"), _pose))
			{
				((GodotObject)_body).Disconnect(StringName.op_Implicit("world_transforms_changed"), _pose);
			}
			_fire.Dispose();
			_bones.Dispose();
			_soul.Dispose();
			((CanvasItem)_root).Hide();
			((Node)_root).QueueFree();
			_values.Dispose();
		}
	}
}
internal static class MeshMath
{
	internal static void Interpolate(float[] left, float[] right, float fraction, Span<float> output)
	{
		if (left == right)
		{
			left.AsSpan(0, output.Length).CopyTo(output);
			return;
		}
		int i = 0;
		if (Vector.IsHardwareAccelerated)
		{
			Vector<float> vector = new Vector<float>(fraction);
			for (int count = Vector<float>.Count; i <= output.Length - count; i += count)
			{
				Vector<float> vector2 = new Vector<float>(left, i);
				Vector<float> vector3 = new Vector<float>(right, i);
				(vector2 + (vector3 - vector2) * vector).CopyTo(output.Slice(i, count));
			}
		}
		for (; i < output.Length; i++)
		{
			output[i] = Mathf.Lerp(left[i], right[i], fraction);
		}
	}

	internal static void Interpolate(float[] left, float[] right, float fraction, Vector2[] output)
	{
		Interpolate(left, right, fraction, MemoryMarshal.Cast<Vector2, float>(output.AsSpan()));
	}

	internal static void Surface(ClipAssets.SurfaceSpec surface, float[] source, float[] output)
	{
		if (Vector128.IsHardwareAccelerated)
		{
			int num = 0;
			int num2 = 0;
			while (num < output.Length)
			{
				Vector128<double> zero = Vector128<double>.Zero;
				for (int i = 0; i < 3; i++)
				{
					int num3 = surface.Ids[num2 + i] * 2;
					zero += Vector128.Create(source[num3], source[num3 + 1]) * Vector128.Create(surface.Weights[num2 + i]);
				}
				output[num] = (float)zero.GetElement(0);
				output[num + 1] = (float)zero.GetElement(1);
				num += 2;
				num2 += 3;
			}
			return;
		}
		int num4 = 0;
		int num5 = 0;
		while (num4 < output.Length)
		{
			double num6 = 0.0;
			double num7 = 0.0;
			for (int j = 0; j < 3; j++)
			{
				int num8 = surface.Ids[num5 + j] * 2;
				double num9 = surface.Weights[num5 + j];
				num6 += (double)source[num8] * num9;
				num7 += (double)source[num8 + 1] * num9;
			}
			output[num4] = (float)num6;
			output[num4 + 1] = (float)num7;
			num4 += 2;
			num5 += 3;
		}
	}
}
[ModInitializer("Initialize")]
public static class ModEntry
{
	internal const string Id = "MoreNecrobinderAnimations";

	internal static string DirectoryPath => Path.GetDirectoryName(typeof(ModEntry).Assembly.Location);

	public static void Initialize()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Harmony val = new Harmony("local.more-necrobinder-animations");
		try
		{
			val.PatchAll();
			SettingsLocalization.Install();
			OptionalModSettings.Install();
			Callable val2 = Callable.From((Action)delegate
			{
				OstyAddonCompatibility.Install();
				ClipAssets.Preload();
				PresentationPreloader.Warm();
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception error)
		{
			val.UnpatchAll("local.more-necrobinder-animations");
			Log(error);
		}
	}

	internal static void Log(Exception error)
	{
		GD.PrintErr($"[{"MoreNecrobinderAnimations"}] Visual skipped safely: {error}");
	}
}
internal sealed class NativeBansheeScream : IDisposable
{
	internal const float Launch = 0.12f;

	internal const float EmissionDuration = 1f;

	private const string ScenePath = "res://scenes/vfx/vfx_scream.tscn";

	private static PackedScene? _scene;

	private Node2D? _root;

	private GpuParticles2D[] _particles = Array.Empty<GpuParticles2D>();

	private bool _started;

	private bool _stopped;

	private bool _disposed;

	internal float Duration { get; private set; } = 1.12f;

	internal static void Preload()
	{
		if (_scene == null)
		{
			_scene = ResourceLoader.Load<PackedScene>("res://scenes/vfx/vfx_scream.tscn", (string)null, (CacheMode)1) ?? throw new InvalidDataException("res://scenes/vfx/vfx_scream.tscn");
		}
	}

	internal NativeBansheeScream(Node parent, Vector2 origin, uint seed)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		Node2D val = null;
		try
		{
			Preload();
			val = _scene.Instantiate<Node2D>((GenEditState)0);
			_root = new Node2D
			{
				Name = StringName.op_Implicit("NecroNativeBansheeScream"),
				TextureFilter = (TextureFilterEnum)2
			};
			string[] array = new string[3] { "vfx_scream_streaks", "vfx_common_glow", "vfx_scream_ring_polar" };
			_particles = (GpuParticles2D[])(object)new GpuParticles2D[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				GpuParticles2D node = ((Node)val).GetNode<GpuParticles2D>(NodePath.op_Implicit(array[i]));
				_particles[i] = node;
				((Node)node).GetParent().RemoveChild((Node)(object)node);
				node.Emitting = false;
				node.UseFixedSeed = true;
				node.Seed = seed + (uint)(i * 337);
				if (array[i] == "vfx_common_glow")
				{
					node.OneShot = true;
					node.Lifetime = 1.0;
				}
				else
				{
					Duration = Math.Max(Duration, 1.12f + (float)node.Lifetime);
				}
				((CanvasItem)node).ZIndex = 0;
				((CanvasItem)node).ZAsRelative = true;
				((Node)_root).AddChild((Node)(object)node, false, (InternalMode)0);
			}
			((CanvasItem)_root).Hide();
			parent.AddChild((Node)(object)_root, false, (InternalMode)0);
			_root.GlobalTransform = new Transform2D(0f, origin);
		}
		catch
		{
			Dispose();
			throw;
		}
		finally
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((GodotObject)val).Free();
			}
		}
	}

	internal void Advance(float age)
	{
		if (_disposed)
		{
			return;
		}
		if (age >= Duration)
		{
			Dispose();
		}
		else
		{
			if (age < 0.12f)
			{
				return;
			}
			GpuParticles2D[] particles;
			if (!_started)
			{
				_started = true;
				((CanvasItem)_root).Show();
				particles = _particles;
				foreach (GpuParticles2D obj in particles)
				{
					obj.Restart(true);
					obj.Emitting = true;
				}
			}
			if (_stopped || !(age >= 1.12f))
			{
				return;
			}
			_stopped = true;
			particles = _particles;
			foreach (GpuParticles2D val in particles)
			{
				if (!val.OneShot)
				{
					val.Emitting = false;
				}
			}
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				((CanvasItem)_root).Hide();
				((Node)_root).QueueFree();
			}
			_particles = Array.Empty<GpuParticles2D>();
			_root = null;
		}
	}
}
internal sealed class NativeReaperGeometry : IDisposable
{
	internal sealed class Mesh
	{
		internal int Count;

		public string Kind { get; set; } = "";

		public int Slot { get; set; }

		public bool Region { get; set; }

		public bool Weighted { get; set; }

		public int[] Bones { get; set; } = Array.Empty<int>();

		public float[] Weights { get; set; } = Array.Empty<float>();

		public int[] Offsets { get; set; } = Array.Empty<int>();

		public int Source { get; set; }

		public int Surface { get; set; }

		public int[] Parts { get; set; } = Array.Empty<int>();

		public int Anchor { get; set; }

		public int Hips { get; set; }

		public double[] Bindings { get; set; } = Array.Empty<double>();

		public double[] ReferenceAnchor { get; set; } = Array.Empty<double>();

		public double[] ReferenceHips { get; set; } = Array.Empty<double>();
	}

	internal sealed class Shared
	{
		internal int SlotCapacity;

		public Mesh[] Meshes { get; set; } = Array.Empty<Mesh>();

		public ClipAssets.LayoutSpec[] Layouts { get; set; } = Array.Empty<ClipAssets.LayoutSpec>();

		public float[][] Colors { get; set; } = Array.Empty<float[]>();

		public ClipAssets.SurfaceSpec[] Surfaces { get; set; } = Array.Empty<ClipAssets.SurfaceSpec>();

		public float[][] Corrections { get; set; } = Array.Empty<float[]>();

		public float[][][] ExtraPoses { get; set; } = Array.Empty<float[][]>();

		public double[][] RevealFields { get; set; } = Array.Empty<double[]>();
	}

	internal sealed class Deform
	{
		public int Slot { get; set; }

		public bool Extra { get; set; }

		public float[] Values { get; set; } = Array.Empty<float>();
	}

	internal sealed class Pose
	{
		public string Animation { get; set; } = "";

		public double Time { get; set; }

		public float[] Overrides { get; set; } = Array.Empty<float>();

		public float[][] Extra { get; set; } = Array.Empty<float[]>();

		public Deform[] Deforms { get; set; } = Array.Empty<Deform>();

		public int ExtraPoseId { get; set; } = -1;
	}

	internal sealed class Recipe
	{
		internal int[] NeededBones = Array.Empty<int>();

		internal int[] NeededSlots = Array.Empty<int>();

		public string Shared { get; set; } = "";

		public Pose?[] Poses { get; set; } = Array.Empty<Pose>();

		public string[] Bones { get; set; } = Array.Empty<string>();
	}

	internal sealed class Sample
	{
		internal int Frame = -1;

		internal bool Reveal;

		internal float[] Bones = Array.Empty<float>();

		internal readonly Dictionary<ClipAssets.DrawSpec, float[]> Points = new Dictionary<ClipAssets.DrawSpec, float[]>();

		internal readonly Dictionary<ClipAssets.DrawSpec, float[]> Colors = new Dictionary<ClipAssets.DrawSpec, float[]>();

		internal readonly Dictionary<int, float[]> Meshes = new Dictionary<int, float[]>();

		internal readonly HashSet<int> ValidMeshes = new HashSet<int>();

		internal readonly Dictionary<int, float[]> ColorBuffers = new Dictionary<int, float[]>();

		internal readonly Dictionary<int, float[]> CorrectionBuffers = new Dictionary<int, float[]>();

		internal float[] ExtraMatrices = Array.Empty<float>();

		internal float[][] Deforms = Array.Empty<float[]>();

		internal readonly Dictionary<int, float[]> ExtraDeforms = new Dictionary<int, float[]>();
	}

	private static readonly Dictionary<string, Shared> SharedCache = new Dictionary<string, Shared>();

	private readonly RefCounted _reader;

	private ClipAssets? _assets;

	private Sample _first = new Sample();

	private Sample _second = new Sample();

	private bool _disposed;

	private static readonly GDScript Script = Compile();

	internal static Shared Bind(ClipAssets.Recipe recipe)
	{
		Recipe recipe2 = recipe.Native ?? throw new InvalidDataException("Missing native action recipe");
		if (recipe2.Shared != Path.GetFileName(recipe2.Shared) || recipe2.Poses.Length != recipe.Frames.Length)
		{
			throw new InvalidDataException("Invalid native action recipe");
		}
		if (!SharedCache.TryGetValue(recipe2.Shared, out Shared data))
		{
			using FileStream stream = File.OpenRead(Path.Combine(ClipAssets.Directory, recipe2.Shared));
			using BrotliStream utf8Json = new BrotliStream(stream, CompressionMode.Decompress);
			data = JsonSerializer.Deserialize<Shared>(utf8Json, new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? throw new InvalidDataException(recipe2.Shared);
			Mesh[] meshes = data.Meshes;
			foreach (Mesh mesh in meshes)
			{
				Mesh mesh2 = mesh;
				mesh2.Count = mesh.Kind switch
				{
					"surface" => data.Surfaces[mesh.Surface].Ids.Length / 3, 
					"cloth" => mesh.Bindings.Length / 7, 
					"group" => mesh.Parts.Sum((int id) => data.Meshes[id].Count), 
					_ => mesh.Offsets.Length - 1, 
				};
			}
			SharedCache.Add(recipe2.Shared, data);
		}
		recipe.Layouts = data.Layouts;
		recipe.ColorSets = data.Colors;
		recipe.Surfaces = data.Surfaces;
		Pose[] poses = recipe2.Poses;
		foreach (Pose pose in poses)
		{
			if (pose != null && pose.ExtraPoseId >= 0)
			{
				if (pose.ExtraPoseId >= data.ExtraPoses.Length)
				{
					throw new InvalidDataException("Invalid shared arm pose");
				}
				pose.Extra = data.ExtraPoses[pose.ExtraPoseId];
			}
		}
		if (data.SlotCapacity == 0)
		{
			data.SlotCapacity = (from m in data.Meshes
				where m.Kind == "native"
				select m.Slot + 1).DefaultIfEmpty().Max();
		}
		ClipAssets.DrawSpec[][] frames = recipe.Frames;
		foreach (ClipAssets.DrawSpec[] array in frames)
		{
			foreach (ClipAssets.DrawSpec drawSpec in array)
			{
				if (drawSpec.RevealFieldId >= 0)
				{
					if (drawSpec.RevealFieldId >= data.RevealFields.Length)
					{
						throw new InvalidDataException("Invalid shared reveal field");
					}
					drawSpec.RevealField = data.RevealFields[drawSpec.RevealFieldId];
				}
				if (drawSpec.NativeMesh >= 0 && (drawSpec.NativeMesh >= data.Meshes.Length || drawSpec.Layout < 0 || drawSpec.Layout >= data.Layouts.Length || data.Meshes[drawSpec.NativeMesh].Count * 2 != data.Layouts[drawSpec.Layout].Uvs.Length))
				{
					throw new InvalidDataException("Invalid native mesh topology");
				}
				int num = drawSpec.RevealField.Length;
				bool flag = ((num == 0 || num == 8) ? true : false);
				if (!flag || drawSpec.NativeCorrection >= data.Corrections.Length)
				{
					throw new InvalidDataException("Invalid native reveal or loop correction");
				}
			}
		}
		HashSet<int> used = new HashSet<int>();
		frames = recipe.Frames;
		foreach (ClipAssets.DrawSpec[] array in frames)
		{
			foreach (ClipAssets.DrawSpec drawSpec2 in array)
			{
				if (drawSpec2.NativeMesh >= 0)
				{
					Need(drawSpec2.NativeMesh);
				}
			}
		}
		Mesh[] source = used.Select((int i) => data.Meshes[i]).ToArray();
		recipe2.NeededBones = (from i in source.SelectMany((Mesh m) => (!(m.Kind == "native")) ? ((m.Kind == "cloth") ? ((IEnumerable<int>)new int[2] { m.Anchor, m.Hips }) : ((IEnumerable<int>)Array.Empty<int>())) : m.Bones).Append(Array.IndexOf(recipe2.Bones, "body_upper"))
			where i >= 0
			select i).Distinct().Order().ToArray();
		recipe2.NeededSlots = (from m in source
			where m.Kind == "native" && !m.Region
			select m.Slot).Distinct().Order().ToArray();
		return data;
		void Need(int id)
		{
			if (used.Add(id))
			{
				Mesh mesh3 = data.Meshes[id];
				if (mesh3.Kind == "surface")
				{
					Need(mesh3.Source);
				}
				else if (mesh3.Kind == "group")
				{
					int[] parts = mesh3.Parts;
					foreach (int id2 in parts)
					{
						Need(id2);
					}
				}
			}
		}
	}

	internal NativeReaperGeometry(Node2D body)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)Script).Call(StringName.op_Implicit("new"), Array.Empty<Variant>());
		try
		{
			_reader = (RefCounted)((Variant)(ref val)).AsGodotObject();
			try
			{
				Variant val2 = ((GodotObject)_reader).Call(StringName.op_Implicit("bind"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)body) });
				try
				{
					if (!((Variant)(ref val2)).AsBool())
					{
						throw new InvalidDataException("Native skeleton unavailable");
					}
				}
				finally
				{
					((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
				}
			}
			catch
			{
				Dispose();
				throw;
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal (Sample Left, Sample Right) Advance(ClipAssets assets, int frame, bool reveal = true)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (_assets != assets)
		{
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("matches"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(assets.Data.Native.Bones),
				Variant.op_Implicit(assets.Data.Native.NeededBones),
				Variant.op_Implicit(assets.Data.Native.NeededSlots)
			});
			try
			{
				if (!((Variant)(ref val)).AsBool())
				{
					throw new InvalidDataException("Native skeleton bone layout changed");
				}
				_first.Frame = (_second.Frame = -1);
				_assets = assets;
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		int num = Math.Min(frame + 1, assets.Data.Frames.Length - 1);
		if (_second.Frame == frame)
		{
			Sample second = _second;
			Sample first = _first;
			_first = second;
			_second = first;
		}
		if (_first.Frame != frame || (reveal && !_first.Reveal))
		{
			Fill(_first, assets, frame, reveal);
		}
		if (num == frame)
		{
			return (Left: _first, Right: _first);
		}
		if (_second.Frame != num || (reveal && !_second.Reveal))
		{
			Fill(_second, assets, num, reveal);
		}
		return (Left: _first, Right: _second);
	}

	private void Fill(Sample sample, ClipAssets assets, int frame, bool reveal)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		sample.Points.Clear();
		sample.Colors.Clear();
		sample.ValidMeshes.Clear();
		sample.Frame = frame;
		sample.Reveal = reveal;
		Shared data = assets.NativeData;
		Pose pose = assets.Data.Native.Poses[frame];
		if (pose == null)
		{
			return;
		}
		float num = (float)pose.Time;
		if ((double)num > pose.Time)
		{
			num = MathF.BitDecrement(num);
		}
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("sample"), (Variant[])(object)new Variant[2]
		{
			Variant.op_Implicit(pose.Animation),
			Variant.op_Implicit(num)
		});
		float[] bones;
		float[][] deforms;
		float[] extra2;
		Dictionary<int, float[]> extraDeform;
		try
		{
			Array val2 = ((Variant)(ref val)).AsGodotArray();
			try
			{
				Variant val3 = val2[0];
				try
				{
					float[] array = ((Variant)(ref val3)).AsFloat32Array();
					int[] neededBones = assets.Data.Native.NeededBones;
					int[] neededSlots = assets.Data.Native.NeededSlots;
					if (array.Length != neededBones.Length * 6)
					{
						throw new InvalidDataException("Native bone layout changed");
					}
					if (sample.Bones.Length != assets.Data.Native.Bones.Length * 6)
					{
						sample.Bones = new float[assets.Data.Native.Bones.Length * 6];
					}
					bones = sample.Bones;
					for (int i = 0; i < neededBones.Length; i++)
					{
						Array.Copy(array, i * 6, bones, neededBones[i] * 6, 6);
					}
					Variant val4 = val2[1];
					try
					{
						Array val5 = ((Variant)(ref val4)).AsGodotArray();
						try
						{
							if (val5.Count != neededSlots.Length)
							{
								throw new InvalidDataException("Native deformation layout changed");
							}
							if (sample.Deforms.Length != data.SlotCapacity)
							{
								sample.Deforms = new float[data.SlotCapacity][];
							}
							deforms = sample.Deforms;
							for (int j = 0; j < neededSlots.Length; j++)
							{
								Variant val6 = val5[j];
								try
								{
									deforms[neededSlots[j]] = ((Variant)(ref val6)).AsFloat32Array();
								}
								finally
								{
									((IDisposable)(Variant)(ref val6)/*cast due to .constrained prefix*/).Dispose();
								}
							}
							int num2 = 0;
							float[][] extra = pose.Extra;
							foreach (float[] array2 in extra)
							{
								num2 = Math.Max(num2, ((int)array2[0] + 1) * 6);
							}
							if (sample.ExtraMatrices.Length < num2)
							{
								sample.ExtraMatrices = new float[num2];
							}
							extra2 = sample.ExtraMatrices;
							extra = pose.Extra;
							foreach (float[] array3 in extra)
							{
								Array.Copy(array3, 1, extra2, (int)array3[0] * 6, 6);
							}
							for (int l = 0; l < pose.Overrides.Length; l += 7)
							{
								Array.Copy(pose.Overrides, l + 1, bones, (int)pose.Overrides[l] * 6, 6);
							}
							extraDeform = sample.ExtraDeforms;
							extraDeform.Clear();
							Deform[] deforms2 = pose.Deforms;
							foreach (Deform deform in deforms2)
							{
								if (deform.Extra)
								{
									extraDeform[deform.Slot] = deform.Values;
								}
								else if (deform.Slot < deforms.Length)
								{
									deforms[deform.Slot] = deform.Values;
								}
							}
							int num3 = -1;
							ClipAssets.DrawSpec[] array4 = assets.Data.Frames[frame];
							foreach (ClipAssets.DrawSpec drawSpec in array4)
							{
								num3++;
								float[] array5 = ((drawSpec.NativeMesh >= 0) ? Evaluate(drawSpec.NativeMesh, drawSpec.NativeParameters) : drawSpec.Positions);
								if (reveal && drawSpec.RevealField.Length != 0)
								{
									double[] revealField = drawSpec.RevealField;
									float[] array6 = Buffer(sample.ColorBuffers, num3, array5.Length * 2);
									for (int m = 0; m < array5.Length; m += 2)
									{
										double num4 = revealField[0] * (double)array5[m] + revealField[1] * (double)array5[m + 1] + revealField[2];
										double num5 = revealField[3] * (double)array5[m] + revealField[4] * (double)array5[m + 1] + revealField[5];
										int num6 = m * 2;
										array6[num6] = (float)(num4 / 52.0);
										array6[num6 + 1] = (float)(num5 / 48.0);
										array6[num6 + 2] = (float)(revealField[7] + 0.39 * Math.Clamp(num4 / revealField[6], 0.0, 1.0));
										array6[num6 + 3] = 1f;
									}
									sample.Colors.Add(drawSpec, array6);
								}
								if (drawSpec.NativeMesh < 0)
								{
									continue;
								}
								if (drawSpec.NativeCorrection >= 0)
								{
									float[] array7 = data.Corrections[drawSpec.NativeCorrection];
									float[] array8 = Buffer(sample.CorrectionBuffers, num3, array5.Length);
									for (int n = 0; n < array8.Length; n++)
									{
										array8[n] = array5[n] - array7[n] * drawSpec.NativeLoopWeight;
									}
									sample.Points.Add(drawSpec, array8);
								}
								else
								{
									sample.Points.Add(drawSpec, array5);
								}
							}
						}
						finally
						{
							((IDisposable)val5)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)(Variant)(ref val4)/*cast due to .constrained prefix*/).Dispose();
					}
				}
				finally
				{
					((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
		float[] Evaluate(int id, float[] parameters, int parameterOffset = 0)
		{
			Mesh mesh = data.Meshes[id];
			string kind = mesh.Kind;
			bool flag = ((kind == "cloth" || kind == "group") ? true : false);
			bool flag2 = !flag;
			if (flag2 && sample.ValidMeshes.Contains(id))
			{
				return sample.Meshes[id];
			}
			float[] array9 = Buffer(sample.Meshes, id, mesh.Count * 2);
			if (mesh.Kind == "surface")
			{
				float[] source = Evaluate(mesh.Source, parameters);
				MeshMath.Surface(data.Surfaces[mesh.Surface], source, array9);
			}
			else if (mesh.Kind == "group")
			{
				int num7 = parameterOffset;
				int num8 = 0;
				int[] parts = mesh.Parts;
				foreach (int num10 in parts)
				{
					float[] array10 = Evaluate(num10, parameters, num7);
					array10.CopyTo(array9, num8);
					num8 += array10.Length;
					num7 += ((data.Meshes[num10].Kind == "cloth") ? 6 : 0);
				}
			}
			else if (mesh.Kind == "cloth")
			{
				for (int num11 = 0; num11 < mesh.Bindings.Length; num11 += 7)
				{
					double[] bindings = mesh.Bindings;
					double x = bindings[num11] + (double)parameters[parameterOffset] * bindings[num11 + 5] + (double)parameters[parameterOffset + 2] * bindings[num11 + 6];
					double y = bindings[num11 + 1] + (double)parameters[parameterOffset + 1] * bindings[num11 + 5] + (double)parameters[parameterOffset + 3] * bindings[num11 + 6];
					(double, double) tuple = ClothPoint(mesh.ReferenceAnchor, bones, mesh.Anchor, x, y);
					(double, double) tuple2 = ClothPoint(mesh.ReferenceHips, bones, mesh.Hips, x, y);
					double num12 = bindings[num11 + 2];
					int num13 = num11 / 7 * 2;
					array9[num13] = (float)(tuple.Item1 * num12 + tuple2.Item1 * (1.0 - num12) + bindings[num11 + 3] * (double)parameters[parameterOffset + 4] + bindings[num11 + 4] * (double)parameters[parameterOffset + 5]);
					array9[num13 + 1] = (float)(tuple.Item2 * num12 + tuple2.Item2 * (1.0 - num12));
				}
			}
			else
			{
				float[] array11 = ((mesh.Kind == "native") ? bones : extra2);
				float[] array12 = (mesh.Region ? Array.Empty<float>() : ((mesh.Kind == "native") ? deforms[mesh.Slot] : extraDeform.GetValueOrDefault(mesh.Slot, Array.Empty<float>())));
				for (int num14 = 0; num14 < mesh.Offsets.Length - 1; num14++)
				{
					double num15 = 0.0;
					double num16 = 0.0;
					for (int num17 = mesh.Offsets[num14]; num17 < mesh.Offsets[num14 + 1]; num17++)
					{
						int num18 = num17 * 3;
						int num19 = mesh.Bones[num17] * 6;
						double num20 = mesh.Weights[num18];
						double num21 = mesh.Weights[num18 + 1];
						double num22 = mesh.Weights[num18 + 2];
						if (!mesh.Region && array12.Length != 0)
						{
							if (mesh.Weighted)
							{
								num20 += (double)array12[num17 * 2];
								num21 += (double)array12[num17 * 2 + 1];
							}
							else
							{
								num20 = array12[num14 * 2];
								num21 = array12[num14 * 2 + 1];
							}
						}
						num15 += ((double)array11[num19] * num20 + (double)array11[num19 + 1] * num21 + (double)array11[num19 + 4]) * num22;
						num16 += ((double)array11[num19 + 2] * num20 + (double)array11[num19 + 3] * num21 + (double)array11[num19 + 5]) * num22;
					}
					array9[num14 * 2] = (float)num15;
					array9[num14 * 2 + 1] = (float)num16;
				}
			}
			if (flag2)
			{
				sample.ValidMeshes.Add(id);
			}
			return array9;
		}
	}

	private static float[] Buffer(Dictionary<int, float[]> pool, int key, int length)
	{
		if (!pool.TryGetValue(key, out float[] value) || value.Length != length)
		{
			value = (pool[key] = new float[length]);
		}
		return value;
	}

	private static (double X, double Y) ClothPoint(double[] reference, float[] bones, int bone, double x, double y)
	{
		double num = reference[0] * x + reference[1] * y + reference[4];
		double num2 = reference[2] * x + reference[3] * y + reference[5];
		int num3 = bone * 6;
		return (X: (double)bones[num3] * num + (double)bones[num3 + 1] * num2 + (double)bones[num3 + 4], Y: (double)bones[num3 + 2] * num + (double)bones[num3 + 3] * num2 + (double)bones[num3 + 5]);
	}

	public void Dispose()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("release"), Array.Empty<Variant>());
		try
		{
			((GodotObject)_reader).Dispose();
			_first.Points.Clear();
			_second.Points.Clear();
			_first.Colors.Clear();
			_second.Colors.Clear();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static GDScript Compile()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		GDScript val = new GDScript
		{
			SourceCode = "extends RefCounted\nvar sprite\nvar data\nvar skeleton\nvar bones=[]\nvar slots=[]\nvar animations={}\nvar needed_bones=[]\nvar needed_slots=[]\nfunc bind(body):\n    data=body.get_skeleton_data_res()\n    sprite=ClassDB.instantiate(\"SpineSprite\")\n    sprite.set_skeleton_data_res(data)\n    sprite.set_update_mode(2)\n    sprite.visible=false\n    body.get_parent().add_child(sprite)\n    skeleton=sprite.get_skeleton()\n    bones=skeleton.get_bones()\n    slots=skeleton.get_slots()\n    skeleton.set_scale_x(0.27)\n    skeleton.set_scale_y(0.27)\n    return bones.size()>0\nfunc sample(name,time):\n    if not animations.has(name): animations[name]=data.find_animation(name)\n    skeleton.set_to_setup_pose()\n    animations[name].apply(skeleton,-1.0,time,false,[],1.0,2,0)\n    skeleton.update_world_transform(3)\n    var matrices=PackedFloat32Array()\n    matrices.resize(needed_bones.size()*6)\n    for i in needed_bones.size():\n        var b=bones[needed_bones[i]]\n        var j=i*6\n        matrices[j]=b.get_a()\n        matrices[j+1]=b.get_b()\n        matrices[j+2]=b.get_c()\n        matrices[j+3]=b.get_d()\n        matrices[j+4]=b.get_world_x()\n        matrices[j+5]=b.get_world_y()\n    var deforms=[]\n    deforms.resize(needed_slots.size())\n    for i in needed_slots.size(): deforms[i]=PackedFloat32Array(slots[needed_slots[i]].get_deform())\n    return [matrices,deforms]\nfunc matches(names,bone_indices,slot_indices):\n    if bones.size()!=names.size(): return false\n    for i in bones.size():\n        if bones[i].get_data().get_bone_name()!=names[i]: return false\n    needed_bones=bone_indices\n    needed_slots=slot_indices\n    return true\nfunc release():\n    animations.clear()\n    bones.clear()\n    slots.clear()\n    skeleton=null\n    data=null\n    if is_instance_valid(sprite): sprite.queue_free()\n    sprite=null"
		};
		if ((int)((Script)val).Reload(false) != 0)
		{
			throw new InvalidDataException("Native Reaper value bridge");
		}
		return val;
	}
}
internal static class NativeVfxVisibility
{
	[CompilerGenerated]
	private sealed class <Descendants>d__5 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int <>1__state;

		private Node <>2__current;

		private int <>l__initialThreadId;

		private Node node;

		public Node <>3__node;

		private IEnumerator<Node> <>7__wrap1;

		private Node <child>5__3;

		private IEnumerator<Node> <>7__wrap3;

		Node IEnumerator<Node>.Current
		{
			[DebuggerHidden]
			get
			{
				return <>2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return <>2__current;
			}
		}

		[DebuggerHidden]
		public <Descendants>d__5(int <>1__state)
		{
			this.<>1__state = <>1__state;
			<>l__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = <>1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							<>m__Finally2();
						}
					}
				}
				finally
				{
					<>m__Finally1();
				}
			}
			<>7__wrap1 = null;
			<child>5__3 = null;
			<>7__wrap3 = null;
			<>1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (<>1__state)
				{
				default:
					return false;
				case 0:
					<>1__state = -1;
					<>7__wrap1 = node.GetChildren(false).GetEnumerator();
					<>1__state = -3;
					break;
				case 1:
					<>1__state = -3;
					<>7__wrap3 = Descendants(<child>5__3).GetEnumerator();
					<>1__state = -4;
					goto IL_00c4;
				case 2:
					{
						<>1__state = -4;
						goto IL_00c4;
					}
					IL_00c4:
					if (<>7__wrap3.MoveNext())
					{
						Node current = <>7__wrap3.Current;
						<>2__current = current;
						<>1__state = 2;
						return true;
					}
					<>m__Finally2();
					<>7__wrap3 = null;
					<child>5__3 = null;
					break;
				}
				if (<>7__wrap1.MoveNext())
				{
					<child>5__3 = <>7__wrap1.Current;
					<>2__current = <child>5__3;
					<>1__state = 1;
					return true;
				}
				<>m__Finally1();
				<>7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void <>m__Finally1()
		{
			<>1__state = -1;
			if (<>7__wrap1 != null)
			{
				<>7__wrap1.Dispose();
			}
		}

		private void <>m__Finally2()
		{
			<>1__state = -3;
			if (<>7__wrap3 != null)
			{
				<>7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			<Descendants>d__5 <Descendants>d__;
			if (<>1__state == -2 && <>l__initialThreadId == Environment.CurrentManagedThreadId)
			{
				<>1__state = 0;
				<Descendants>d__ = this;
			}
			else
			{
				<Descendants>d__ = new <Descendants>d__5(0);
			}
			<Descendants>d__.node = <>3__node;
			return <Descendants>d__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	internal static long HiddenTransient { get; private set; }

	internal static void HideTransient(CanvasItem node)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return;
		}
		node.Hide();
		HiddenTransient++;
		if (((object)node).GetType().Name == "NPowerUpVfx")
		{
			object? obj = AccessTools.Field(((object)node).GetType(), "_backVfx")?.GetValue(node);
			CanvasItem val = (CanvasItem)((obj is CanvasItem) ? obj : null);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				val.Hide();
			}
		}
	}

	[IteratorStateMachine(typeof(<Descendants>d__5))]
	internal static IEnumerable<Node> Descendants(Node node)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new <Descendants>d__5(-2)
		{
			<>3__node = node
		};
	}
}
[HarmonyPatch]
internal static class NativePowerPresentationPatch
{
	private static readonly HashSet<string> Names = new HashSet<string> { "NPowerUpVfx", "NPowerAppliedVfx", "NPowerFlashVfx", "NPowerRemovedVfx", "NPowerAppliedBuffVfx", "NPowerAppliedDebuffVfx" };

	internal static IEnumerable<MethodBase> Methods()
	{
		return (from t in typeof(CardModel).Assembly.GetTypes()
			where Names.Contains(t.Name)
			select t).SelectMany((Type t) => from m in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where m.DeclaringType == t && typeof(CanvasItem).IsAssignableFrom(m.ReturnType) && (m.Name == "Create" || m.Name == "CreatePowerUpVfx")
			select m);
	}

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return Methods();
	}

	[HarmonyPostfix]
	private static void Postfix(object? __result, object[] __args)
	{
		try
		{
			CanvasItem val = (CanvasItem)((__result is CanvasItem) ? __result : null);
			if (val == null)
			{
				return;
			}
			CardVisualContext value = CardVisualContext.Current.Value;
			if (value != null)
			{
				value.Observe(val);
				return;
			}
			PowerModel val2 = __args.OfType<PowerModel>().FirstOrDefault();
			if (val2 == null)
			{
				return;
			}
			string text = ((object)val2).GetType().Name switch
			{
				"ReaperFormPower" => "ReaperForm", 
				"LethalityPower" => "Lethality", 
				"FriendshipPower" => "Friendship", 
				"SummonNextTurnPower" => "Invoke", 
				"NeurosurgePower" => "Neurosurge", 
				_ => null, 
			};
			if (text != null)
			{
				NCreature creatureNode = val2.Owner.GetCreatureNode();
				if (creatureNode != null && AnimationSettings.Current.Allows(text) && VisualSession.CountFor(creatureNode, text) > 0)
				{
					NativeVfxVisibility.HideTransient(val);
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch]
internal static class NativeReaperPresentationPatch
{
	[HarmonyPrepare]
	private static bool Prepare()
	{
		return TargetMethods().Any();
	}

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return (from t in typeof(CardModel).Assembly.GetTypes()
			where t.Name == "NReaperFormVfx"
			select t).SelectMany((Type t) => from m in t.GetMethods(BindingFlags.Static | BindingFlags.Public)
			where m.Name == "Create"
			select m);
	}

	[HarmonyPostfix]
	private static void Postfix(object? __result, object[] __args)
	{
		try
		{
			CanvasItem val = (CanvasItem)((__result is CanvasItem) ? __result : null);
			if (val != null)
			{
				Creature? obj = __args.OfType<Creature>().FirstOrDefault();
				NCreature val2 = ((obj != null) ? obj.GetCreatureNode() : null);
				if (val2 != null)
				{
					ReaperFormEffect.For(val2.Body)?.HideNative(val);
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
[HarmonyPatch]
internal static class NativeReaperTriggerPatch
{
	[HarmonyPrepare]
	private static bool Prepare()
	{
		return TargetMethods().Any();
	}

	private static IEnumerable<MethodBase> TargetMethods()
	{
		return (from t in typeof(CardModel).Assembly.GetTypes()
			where t.Name == "NReaperFormVfx"
			select t).SelectMany((Type t) => from m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
			where m.Name == "OnEffectTriggered" && m.DeclaringType == t
			select m);
	}

	[HarmonyPrefix]
	private static bool Prefix(CanvasItem __instance)
	{
		return !NativeFormVisibility.IsHidden(__instance);
	}
}
[HarmonyPatch(typeof(ReaperFormPower), "AfterDamageGiven")]
internal static class ReaperPowerPresentationPatch
{
	private sealed record State(CardVisualContext? Previous);

	[HarmonyPrefix]
	private static void Prefix(ReaperFormPower __instance, out State? __state)
	{
		__state = null;
		try
		{
			NCreature creatureNode = ((PowerModel)__instance).Owner.GetCreatureNode();
			if (creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && !((GodotObject)creatureNode).IsQueuedForDeletion())
			{
				Node2D body = creatureNode.Body;
				if (body != null && GodotObject.IsInstanceValid((GodotObject)(object)body) && !((GodotObject)body).IsQueuedForDeletion() && ReaperFormEffect.For(body) != null)
				{
					__state = new State(CardVisualContext.Current.Value);
					CardVisualContext.Current.Value = new CardVisualContext
					{
						Card = "ReaperForm",
						Started = true
					};
				}
			}
		}
		catch (Exception error)
		{
			if ((object)__state != null)
			{
				CardVisualContext.Current.Value = __state.Previous;
			}
			__state = null;
			ModEntry.Log(error);
		}
	}

	[HarmonyPostfix]
	private static void Postfix(State? __state)
	{
		if ((object)__state != null)
		{
			CardVisualContext.Current.Value = __state.Previous;
		}
	}

	[HarmonyFinalizer]
	private static void Finalizer(State? __state)
	{
		if ((object)__state != null)
		{
			CardVisualContext.Current.Value = __state.Previous;
		}
	}
}
[HarmonyPatch]
internal static class ReaperPowerGainedPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(ReaperFormPower), "AfterApplied", (Type[])null, (Type[])null);
	}

	[HarmonyPostfix]
	private static void Postfix(PowerModel __instance, Task __result, bool __runOriginal)
	{
		try
		{
			if (__runOriginal && __instance is ReaperFormPower && !__result.IsFaulted && !__result.IsCanceled)
			{
				VisualSession.ReaperPowerGained(__instance.Owner);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal sealed class NativeFormVisibility
{
	private static readonly ConditionalWeakTable<CanvasItem, object> Hidden = new ConditionalWeakTable<CanvasItem, object>();

	private readonly CanvasItem _node;

	private readonly bool _visible;

	private readonly List<(GpuParticles2D Node, bool Emitting)> _particles = new List<(GpuParticles2D, bool)>();

	internal static bool IsHidden(CanvasItem node)
	{
		object value;
		return Hidden.TryGetValue(node, out value);
	}

	internal NativeFormVisibility(CanvasItem node)
	{
		_node = node;
		_visible = node.Visible;
		node.Hide();
		Hidden.GetValue(node, (CanvasItem _) => new object());
		foreach (GpuParticles2D item in NativeVfxVisibility.Descendants((Node)(object)node).OfType<GpuParticles2D>())
		{
			_particles.Add((item, item.Emitting));
			item.Emitting = false;
		}
	}

	internal bool Owns(CanvasItem node)
	{
		return _node == node;
	}

	internal void Release(bool restore)
	{
		if (restore)
		{
			Hidden.Remove(_node);
			if (GodotObject.IsInstanceValid((GodotObject)(object)_node) && !((GodotObject)_node).IsQueuedForDeletion())
			{
				_node.Visible = _visible;
			}
			foreach (var (val, emitting) in _particles)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)val) && !((GodotObject)val).IsQueuedForDeletion())
				{
					val.Emitting = emitting;
				}
			}
		}
		_particles.Clear();
	}
}
internal sealed class NeurosurgeEffect : IVisualEffect, IDisposable
{
	private readonly ClipPlayer _brain;

	private readonly NCreature _hero;

	private readonly NCombatRoom _room;

	private ActorCapture? _capture;

	private ShaderMaterial? _material;

	private static Shader? _shader;

	private bool _disposed;

	private const string ShaderSource = "shader_type canvas_item;\nrender_mode unshaded, blend_premul_alpha;\nuniform float strength=0.0;\nuniform vec4 offsets=vec4(0.0);\nuniform sampler2D capture_texture;\nvec4 actor(vec2 p){float inside=step(0.0,p.x)*step(p.x,1.0)*step(0.0,p.y)*step(p.y,1.0);return texture(capture_texture,clamp(p,vec2(0.0),vec2(1.0)))*inside;}\nvec4 ghost(vec2 p,float opacity,vec3 tint){\n vec4 source=actor(p);vec2 d=vec2(2.0/512.0);\n float blur=(actor(p+vec2(d.x,0.0)).a+actor(p-vec2(d.x,0.0)).a+actor(p+vec2(0.0,d.y)).a+actor(p-vec2(0.0,d.y)).a)*.25;\n float detail=dot(source.rgb,vec3(.21,.72,.07))/max(source.a,.001);\n float body=source.a*opacity*strength;float halo=max(0.0,blur-source.a*.7)*opacity*strength*.9;\n return vec4(tint*(.65+.35*detail)*body+tint*halo,min(1.0,body+halo));\n}\nvoid fragment(){vec4 original=actor(UV);vec4 far_ghost=ghost(UV-offsets.zw,.42,vec3(.24,.28,1.0));vec4 near_ghost=ghost(UV-offsets.xy,.55,vec3(.03,.72,1.0));vec4 echoes=near_ghost+far_ghost*(1.0-near_ghost.a);COLOR=original+echoes*(1.0-original.a);}";

	public float Duration => _brain.Duration;

	public bool Hold => false;

	internal static Shader Shader
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			if (_shader == null)
			{
				Shader val = new Shader
				{
					Code = "shader_type canvas_item;\nrender_mode unshaded, blend_premul_alpha;\nuniform float strength=0.0;\nuniform vec4 offsets=vec4(0.0);\nuniform sampler2D capture_texture;\nvec4 actor(vec2 p){float inside=step(0.0,p.x)*step(p.x,1.0)*step(0.0,p.y)*step(p.y,1.0);return texture(capture_texture,clamp(p,vec2(0.0),vec2(1.0)))*inside;}\nvec4 ghost(vec2 p,float opacity,vec3 tint){\n vec4 source=actor(p);vec2 d=vec2(2.0/512.0);\n float blur=(actor(p+vec2(d.x,0.0)).a+actor(p-vec2(d.x,0.0)).a+actor(p+vec2(0.0,d.y)).a+actor(p-vec2(0.0,d.y)).a)*.25;\n float detail=dot(source.rgb,vec3(.21,.72,.07))/max(source.a,.001);\n float body=source.a*opacity*strength;float halo=max(0.0,blur-source.a*.7)*opacity*strength*.9;\n return vec4(tint*(.65+.35*detail)*body+tint*halo,min(1.0,body+halo));\n}\nvoid fragment(){vec4 original=actor(UV);vec4 far_ghost=ghost(UV-offsets.zw,.42,vec3(.24,.28,1.0));vec4 near_ghost=ghost(UV-offsets.xy,.55,vec3(.03,.72,1.0));vec4 echoes=near_ghost+far_ghost*(1.0-near_ghost.a);COLOR=original+echoes*(1.0-original.a);}"
				};
				Array shaderUniformList = val.GetShaderUniformList(false);
				try
				{
					if (shaderUniformList.Count == 0)
					{
						((GodotObject)val).Dispose();
						throw new InvalidDataException("Neurosurge composite shader");
					}
					_shader = val;
				}
				finally
				{
					((IDisposable)shaderUniformList)?.Dispose();
				}
			}
			return _shader;
		}
	}

	internal NeurosurgeEffect(NCreature hero, NCreature osty, NCombatRoom room)
	{
		_hero = hero;
		_room = room;
		_brain = new ClipPlayer(ClipAssets.Get("Neurosurge"), hero, osty, (Node)(object)room.CombatVfxContainer);
	}

	public void CancelBody()
	{
	}

	private static float Smooth(float a, float b, float x)
	{
		float num = Math.Clamp((x - a) / (b - a), 0f, 1f);
		return num * num * (3f - 2f * num);
	}

	public void Advance(float age)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		_brain.Advance(age);
		if (!SpineCompatibility.Allows(_hero.Body, "necrobinder"))
		{
			return;
		}
		float num = Smooth(0.4f, 0.58f, age) * (1f - Smooth(1.15f, 1.65f, age));
		if (num <= 0f)
		{
			_capture?.Dispose();
			_capture = null;
			return;
		}
		if (_capture == null)
		{
			_material = new ShaderMaterial
			{
				Shader = Shader
			};
			_capture = new ActorCapture(_hero.Body, (Node)(object)_room.CombatVfxContainer, 512, 60f, _material);
		}
		Rect2 bounds = _capture.Bounds;
		Vector2 size = ((Rect2)(ref bounds)).Size;
		_material.SetShaderParameter(StringName.op_Implicit("strength"), Variant.op_Implicit(num));
		_material.SetShaderParameter(StringName.op_Implicit("offsets"), Variant.op_Implicit(new Vector4(-12f * (0.85f + 0.15f * MathF.Sin(age * 7f)) / size.X, -2.16f * MathF.Sin(age * 5f) / size.Y, 12f * (0.9f + 0.1f * MathF.Sin(age * 5f + 0.8f)) / size.X, 2.6399999f * MathF.Sin(age * 4f + 0.5f) / size.Y)));
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		try
		{
			_capture?.Dispose();
			ShaderMaterial? material = _material;
			if (material != null)
			{
				((GodotObject)material).Dispose();
			}
		}
		finally
		{
			_brain.Dispose();
		}
	}
}
internal static class OptionalModSettings
{
	private static bool _watching;

	private static readonly HashSet<string> Attempted = new HashSet<string>();

	internal static bool Available { get; private set; }

	internal static void Install()
	{
		if (_watching)
		{
			return;
		}
		_watching = true;
		AnimationSettings.Current = new AnimationSettings();
		ModManager.OnModDetected += OnModDetected;
		foreach (Mod mod in ModManager.Mods)
		{
			OnModDetected(mod);
		}
	}

	private static void OnModDetected(Mod mod)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		string text = mod.manifest?.id;
		bool flag = ((text == "JmcModLib" || text == "BaseLib") ? true : false);
		if (!flag || (int)mod.state != 1 || !Attempted.Add(text))
		{
			return;
		}
		AnimationSettings current = AnimationSettings.Current;
		try
		{
			Version version = ((text == "BaseLib") ? new Version(3, 4, 7) : new Version(1, 9, 0));
			List<LocString> errors = mod.errors;
			if ((errors != null && errors.Count > 0) || !Version.TryParse(mod.manifest.version?.TrimStart('v', 'V'), out Version result) || result < version)
			{
				Console.WriteLine($"[MoreNecrobinderAnimations] Optional {text} menu needs version {version}+; retaining current animation settings.");
				return;
			}
			Assembly assembly = typeof(ModEntry).Assembly;
			if (!Available)
			{
				AnimationSettings.Load();
			}
			string assemblyPath = Path.Combine(Path.GetDirectoryName(assembly.Location), (text == "BaseLib") ? "MoreNecrobinderAnimations.BaseLib.dll" : "MoreNecrobinderAnimations.Settings.dll");
			AssemblyLoadContext.GetLoadContext(assembly).LoadFromAssemblyPath(assemblyPath).GetType((text == "BaseLib") ? "MoreNecrobinderAnimations.BaseLibSettingsMenu" : "MoreNecrobinderAnimations.ModSettingsMenu", throwOnError: true)
				.GetMethod("Install", BindingFlags.Static | BindingFlags.Public)
				.Invoke(null, null);
			Available = true;
			Console.WriteLine("[MoreNecrobinderAnimations] Optional " + text + " menu ready: 15 independent card animations and Osty attack probability.");
		}
		catch (Exception ex)
		{
			AnimationSettings.Current = current;
			Console.Error.WriteLine("[MoreNecrobinderAnimations] Optional " + text + " menu unavailable; retaining current animation settings: " + ex.GetBaseException().Message);
		}
	}
}
internal static class OstyAddonCompatibility
{
	private sealed record Home(Vector2 Position, WeakReference<Node> Parent)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Position = ");
			Vector2 position = Position;
			builder.Append(((object)(Vector2)(ref position)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Parent = ");
			builder.Append(Parent);
			return true;
		}
	}

	private const string Id = "local.more-necrobinder-animations.osty-addon";

	private const string AddonId = "kinetickinkajou.OstyAnime";

	private const string DllHash = "6423c7b1d68ad165c5e30a3c19f84f6a651578157c2121a277aeefa5eba626da";

	internal const string RigHash = "6a748faa59ea714572fdfc9e5c3f9f55316fbc8d5299d5946877dea2944495cc";

	private static Action<NCreature>? _restore;

	private static Action<NCreature>? _preempt;

	private static Action<NCreature>? _bounds;

	private static Func<bool>? _flying;

	private static WeakReference<NCreature>? _flightOwner;

	private static readonly ConditionalWeakTable<CardModel, object> Claimed = new ConditionalWeakTable<CardModel, object>();

	private static readonly ConditionalWeakTable<object, Home> Homes = new ConditionalWeakTable<object, Home>();

	private static IDictionary? _motionStates;

	internal static bool Active { get; private set; }

	internal static void Install()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly a) => a.GetName().Name == "OstyAnime");
		if ((object)assembly == null || Active)
		{
			return;
		}
		Harmony harmony = new Harmony("local.more-necrobinder-animations.osty-addon");
		try
		{
			if (!(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(assembly.Location))) != "6423c7b1d68ad165c5e30a3c19f84f6a651578157c2121a277aeefa5eba626da") && Harmony.GetAllPatchedMethods().Any(delegate(MethodBase m)
			{
				if (m.Name == "BeforeSideTurnEnd")
				{
					Patches patchInfo = Harmony.GetPatchInfo(m);
					if (patchInfo == null)
					{
						return false;
					}
					return patchInfo.Owners.Contains("kinetickinkajou.OstyAnime");
				}
				return false;
			}))
			{
				_restore = Method("OstyMotion", "HardRestore").CreateDelegate<Action<NCreature>>();
				_preempt = Method("FetchDragonPatch", "PreemptForNewAction").CreateDelegate<Action<NCreature>>();
				_flying = Method("FetchDragonPatch", "get_Flying").CreateDelegate<Func<bool>>();
				_bounds = Method("OstyMotion", "UpdateBoundsCompat").CreateDelegate<Action<NCreature>>();
				_motionStates = (IDictionary)AccessTools.Field(assembly.GetType("OstyAnime.OstyMotion", throwOnError: true), "_states").GetValue(null);
				Patch(Method("SlamTravel", "Before"), "BeforeTravel");
				Patch(Method("SlamTravel", "AfterChain"), "AfterTravel");
				Patch(Method("OstyCombo", "IsOurCard"), null, "StagingCard");
				Patch(Method("OstyMotion", "MoveInternal"), "Move");
				Patch(Method("FetchDragonPatch", "Flight"), "Flight");
				Patch(Method("OstyMotion", "GetOrCreate"), null, "RememberHome");
				Patch(Method("OstyMotion", "ClearState"), "LeavingExcursion", "RestoreHome");
				string[] array = new string[12]
				{
					"Poke", "PullAggro", "Snap", "HighFive", "Fetch", "Rattle", "RightHandHand", "Flatten", "Squeeze", "SweepingGaze",
					"SicEm", "BoneShards"
				};
				foreach (string text in array)
				{
					Patch(AccessTools.Method(typeof(CardModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Cards." + text, throwOnError: true), "OnPlay", (Type[])null, (Type[])null), "ForeignAction");
				}
				Active = true;
			}
		}
		catch (Exception error)
		{
			harmony.UnpatchAll("local.more-necrobinder-animations.osty-addon");
			Active = false;
			ModEntry.Log(error);
		}
		MethodInfo Method(string type, string method)
		{
			return AccessTools.Method(assembly.GetType("OstyAnime." + type, throwOnError: true), method, (Type[])null, (Type[])null) ?? throw new MissingMethodException(type, method);
		}
		void Patch(MethodInfo method, string? prefix = null, string? postfix = null)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			Harmony obj = harmony;
			object obj2;
			if (prefix != null)
			{
				HarmonyMethod val = new HarmonyMethod(typeof(OstyAddonCompatibility), prefix, (Type[])null);
				val.priority = 800;
				val.before = new string[1] { "kinetickinkajou.OstyAnime" };
				obj2 = val;
			}
			else
			{
				obj2 = null;
			}
			obj.Patch((MethodBase)method, (HarmonyMethod)obj2, (postfix == null) ? ((HarmonyMethod)null) : new HarmonyMethod(typeof(OstyAddonCompatibility), postfix, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null);
		}
	}

	private static bool Owns(CardModel card)
	{
		bool flag = !Active;
		if (!flag)
		{
			bool flag2 = ((card is Unleash || card is Protector) ? true : false);
			flag = !flag2;
		}
		if (flag || !AnimationSettings.Current.Allows(((object)card).GetType().Name))
		{
			return false;
		}
		if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding || !card.Owner.Creature.IsAlive)
		{
			return false;
		}
		Creature osty = card.Owner.Osty;
		NCreature val = ((osty != null) ? osty.GetCreatureNode() : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && val.Entity.IsAlive && !VisualSession.IsDying(val) && NCombatRoom.Instance != null)
		{
			return SpineCompatibility.AllowsOstyBody(val.Body);
		}
		return false;
	}

	private static bool BeforeTravel(CardModel card, CardPlay cardPlay)
	{
		Claimed.Remove(card);
		try
		{
			if (Owns(card))
			{
				Creature target = cardPlay.Target;
				if (((target != null) ? target.GetCreatureNode() : null) != null)
				{
					NCreature creatureNode = card.Owner.Osty.GetCreatureNode();
					if (_flying())
					{
						WeakReference<NCreature>? flightOwner = _flightOwner;
						if (flightOwner != null && flightOwner.TryGetTarget(out NCreature target2) && target2 == creatureNode)
						{
							_preempt(creatureNode);
						}
					}
					_restore(creatureNode);
					Claimed.Add(card, new object());
					return false;
				}
			}
			return true;
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			return true;
		}
	}

	private static bool AfterTravel(CardModel card)
	{
		return !Claimed.Remove(card);
	}

	private static void StagingCard(CardModel c, ref bool __result)
	{
		try
		{
			if (__result && Owns(c))
			{
				__result = false;
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	private static bool Move(NCreature osty, ref Task<bool> __result)
	{
		if (!VisualSession.OwnsOstyBody(osty))
		{
			return true;
		}
		__result = Task.FromResult(result: false);
		return false;
	}

	private static void Flight(Player owner)
	{
		try
		{
			Creature osty = owner.Osty;
			NCreature val = ((osty != null) ? osty.GetCreatureNode() : null);
			_flightOwner = ((val == null) ? null : new WeakReference<NCreature>(val));
		}
		catch (Exception error)
		{
			_flightOwner = null;
			ModEntry.Log(error);
		}
	}

	private static void ForeignAction(CardModel __instance)
	{
		try
		{
			Creature osty = __instance.Owner.Osty;
			VisualSession.InterruptBody((osty != null) ? osty.GetCreatureNode() : null, "OstyAddonAction");
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	private static void RememberHome(NCreature osty, object __result)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (!Homes.TryGetValue(__result, out Home _) && AnimationSettings.Current.Enabled && GodotObject.IsInstanceValid((GodotObject)(object)osty))
			{
				Node parent = ((Node)osty).GetParent();
				if (parent != null)
				{
					Homes.Add(__result, new Home(((Control)osty).Position, new WeakReference<Node>(parent)));
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	private static void LeavingExcursion(NCreature? osty, out Home? __state)
	{
		__state = null;
		try
		{
			if (osty != null && GodotObject.IsInstanceValid((GodotObject)(object)osty))
			{
				object obj = _motionStates[((GodotObject)osty).GetInstanceId()];
				if (obj != null && Homes.TryGetValue(obj, out __state))
				{
					Homes.Remove(obj);
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	private static void RestoreHome(NCreature? osty, Home? __state)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((object)__state != null && osty != null && GodotObject.IsInstanceValid((GodotObject)(object)osty) && osty.Entity.IsAlive && !_motionStates.Contains(((GodotObject)osty).GetInstanceId()) && __state.Parent.TryGetTarget(out Node target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && ((Node)osty).GetParent() == target)
			{
				((Control)osty).Position = __state.Position;
				_bounds(osty);
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal sealed class OstyEffect : IVisualEffect, IDisposable
{
	internal static Func<int>? SelectionForTests;

	private readonly ClipPlayer _body;

	private readonly OstyMotion _motion;

	private readonly OstyRibbon _ribbon;

	private readonly OstyHit _hit;

	private readonly VisualShake _shake = new VisualShake();

	private readonly Vector2 _ground;

	private readonly float _scale;

	private readonly bool _strong;

	private readonly bool _upper;

	private OstyMotion.Sample _sample;

	private bool _disposed;

	private bool _canceled;

	private bool _secondShake;

	private ActorCapture? _aura;

	private readonly ShaderMaterial _auraMaterial;

	private readonly ClipPlayer? _edge;

	private OstyMotion.Sample _edgeSample;

	private static readonly StringName Strength = StringName.op_Implicit("p_strength");

	private static readonly StringName Pivot = StringName.op_Implicit("p_pivot");

	private readonly OstyParticles? _particles;

	private readonly Node2D _front;

	private readonly Node2D _osty;

	private float _age;

	private readonly RadialFeedback? _blur;

	private readonly Node2D _rear;

	internal static string LastVariant { get; private set; } = "";

	public float Duration => 2.55f;

	public bool Hold => false;

	internal bool OwnsBody
	{
		get
		{
			if (!_disposed && !_canceled)
			{
				return _age < 0.9f;
			}
			return false;
		}
	}

	internal OstyEffect(NCreature hero, NCreature osty, NCreature enemy, NCombatRoom room, bool strong)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			_osty = osty.Body;
			_strong = strong;
			_upper = (SelectionForTests?.Invoke() ?? RandomNumberGenerator.GetInt32(100)) >= AnimationSettings.Current.OstySlamChance;
			LastVariant = (_upper ? "uppercut" : "slam");
			Vector2 globalPosition = osty.Body.GlobalPosition;
			globalPosition.Y = 0f - globalPosition.Y;
			Transform2D globalTransform = osty.Body.GlobalTransform;
			_scale = ((Vector2)(ref globalTransform.X)).Length() / 0.26f;
			Rect2 globalRect = enemy.Visuals.Bounds.GetGlobalRect();
			_ground = new Vector2(((Control)enemy).GlobalPosition.X, 0f - ((Rect2)(ref globalRect)).End.Y);
			Vector2 vfxSpawnPosition = enemy.VfxSpawnPosition;
			ClipAssets assets = ClipAssets.Get(strong ? "OstyBodyStrong" : "OstyBody");
			_motion = new OstyMotion(assets, globalPosition, _scale, _ground, new Vector2(vfxSpawnPosition.X, 0f - vfxSpawnPosition.Y), strong, _upper);
			_front = new Node2D
			{
				Name = StringName.op_Implicit("NecroOstyDisplay")
			};
			((Node)room.CombatVfxContainer).AddChild((Node)(object)_front, false, (InternalMode)0);
			((Node)room.CombatVfxContainer).MoveChild((Node)(object)_front, 0);
			_sample = _motion.Get(0f);
			Node2D val = new Node2D
			{
				Name = StringName.op_Implicit("NecroOstyTrailLayer")
			};
			((Node)room).GetNode(NodePath.op_Implicit("CombatSceneContainer/BackCombatVfxContainer")).AddChild((Node)(object)val, false, (InternalMode)0);
			_ribbon = new OstyRibbon((Node)(object)val, _motion, strong);
			_rear = val;
			_body = new ClipPlayer(assets, hero, osty, (Node)(object)room.CombatVfxContainer, ownBody: true, (Node?)(object)_front)
			{
				BasisOverride = (string tag) => (!tag.StartsWith("osty-body", StringComparison.Ordinal)) ? ((!(tag == "osty-impact")) ? ((Transform2D?)null) : new Transform2D?(new Transform2D(0f, Vector2.One * _scale, 0f, new Vector2(_ground.X, 0f - _ground.Y)))) : new Transform2D?(_sample.Canvas),
				LayerOpacity = delegate(string tag)
				{
					if (tag == "osty-shadow")
					{
						return 1f - _sample.Weight;
					}
					return (tag == "osty-impact") ? ((float)((!_upper) ? 1 : 0)) : 1f;
				}
			};
			if (_upper)
			{
				_edgeSample = _motion.Get(0f, edge: true);
				_edge = new ClipPlayer(assets, hero, osty, (Node)(object)room.CombatVfxContainer, ownBody: false, (Node?)(object)_front)
				{
					BasisOverride = (string tag) => (!tag.StartsWith("osty-body", StringComparison.Ordinal)) ? ((Transform2D?)null) : new Transform2D?(_edgeSample.Canvas),
					LayerOpacity = (string tag) => (!tag.StartsWith("osty-body", StringComparison.Ordinal)) ? 0f : _sample.EdgeOpacity
				};
				_edge.Visible(visible: false);
			}
			_auraMaterial = new ShaderMaterial
			{
				Shader = ClipAssets.Get("OstyPrograms").Shaders[0]
			};
			_auraMaterial.SetShaderParameter(StringName.op_Implicit("p_texel"), Variant.op_Implicit(Vector2.One / 512f));
			_auraMaterial.SetShaderParameter(StringName.op_Implicit("p_tier"), Variant.op_Implicit(strong ? 1f : 0f));
			_auraMaterial.SetShaderParameter(Strength, Variant.op_Implicit(0f));
			_aura = new ActorCapture(osty.Body, (Node)(object)room.CombatVfxContainer, 512, 39f * _scale / 0.28f, _auraMaterial, () => _body.BodyBounds);
			_auraMaterial.SetShaderParameter(StringName.op_Implicit("source_texture"), Variant.op_Implicit((GodotObject)(object)_aura.Texture));
			ActorLayers.Place((Node2D)(object)_aura.Composite, (Node)(object)_front, "osty-body-front");
			ActorLayers.Register(_osty, new ActorLayers.Display(_front, () => _sample.Canvas, () => _motion.Wrist(_age, _sample)));
			if (strong)
			{
				_particles = new OstyParticles((Node)(object)val, (Node)(object)room.CombatVfxContainer, _scale);
			}
			_hit = new OstyHit((Node)(object)room.CombatVfxContainer, vfxSpawnPosition, strong, Mathf.DegToRad((float)(_upper ? (-56) : 56)));
			if (strong)
			{
				_blur = new RadialFeedback((Node)(object)room.CombatVfxContainer);
			}
			Advance(0f);
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	public void Advance(float age)
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		_age = age;
		_sample = _motion.Get(age);
		_body.Advance(age);
		if (!_canceled)
		{
			_ribbon.Advance(age, _sample);
		}
		_hit.Advance(age);
		_blur?.Advance(age);
		if (age >= 0.9f)
		{
			ActorLayers.Remove(_osty, _front);
		}
		if (_edge != null)
		{
			bool flag = !_canceled && _sample.EdgeOpacity > 0.001f && age < 0.9f;
			_edge.Visible(flag);
			if (flag)
			{
				_edgeSample = _motion.Get(age, edge: true);
				_edge.Advance(age);
			}
		}
		float num = OstyMotion.Smooth((age - 0.045f) / 0.08f) * (1f - OstyMotion.Smooth((age - _motion.Land - 0.07f) / 0.17f)) * (_strong ? 1f : 0.65f);
		if (!_canceled)
		{
			_particles?.Advance(age, _ribbon, _sample, num);
		}
		if (_aura != null)
		{
			if (age > _motion.Land + 0.24f || _canceled)
			{
				_aura.Dispose();
				_aura = null;
			}
			else
			{
				_auraMaterial.SetShaderParameter(Strength, Variant.op_Implicit(num));
				Rect2 bounds = _aura.Bounds;
				_auraMaterial.SetShaderParameter(Pivot, Variant.op_Implicit((new Vector2(_sample.Front.X, 0f - _sample.Front.Y) - ((Rect2)(ref bounds)).Position) / ((Rect2)(ref bounds)).Size));
			}
		}
		if (_strong && !_secondShake && age >= 0.4f)
		{
			_secondShake = true;
			_shake.Play(40f, 0.3, _upper ? (-56) : 56);
		}
	}

	public void CancelBody()
	{
		_canceled = true;
		ActorLayers.Remove(_osty, _front);
		_aura?.Dispose();
		_aura = null;
		_edge?.CancelBody();
		_body.CancelBody();
		_ribbon.Dispose();
		_particles?.Dispose();
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (_osty != null && _front != null)
		{
			ActorLayers.Remove(_osty, _front);
		}
		_aura?.Dispose();
		_edge?.Dispose();
		_body?.Dispose();
		_ribbon?.Dispose();
		_particles?.Dispose();
		_hit?.Dispose();
		_blur?.Dispose();
		_shake.Dispose();
		ShaderMaterial auraMaterial = _auraMaterial;
		if (auraMaterial != null)
		{
			((GodotObject)auraMaterial).Dispose();
		}
		Node2D[] array = (Node2D[])(object)new Node2D[2] { _rear, _front };
		foreach (Node2D val in array)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((CanvasItem)val).Hide();
				((Node)val).QueueFree();
			}
		}
	}
}
internal sealed class OstyHit : IDisposable
{
	private readonly Sprite2D[] _sprites = (Sprite2D[])(object)new Sprite2D[2];

	private readonly JsonElement[] _recipes = new JsonElement[2];

	private readonly ClipAssets _assets;

	private readonly Vector2 _point;

	private readonly bool _strong;

	private readonly float _angle;

	private bool _disposed;

	internal OstyHit(Node parent, Vector2 point, bool strong, float angle)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Expected O, but got Unknown
		//IL_00d8: Expected O, but got Unknown
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		_point = point;
		_strong = strong;
		_angle = angle;
		_assets = ClipAssets.Get("OstyHits");
		for (int i = 0; i < 2; i++)
		{
			_recipes[i] = _assets.Data.Extra.GetProperty("modes").GetProperty((i == 0) ? (strong ? "mecha" : "blunt") : "spark");
			Sprite2D[] sprites = _sprites;
			int num = i;
			Sprite2D val = new Sprite2D
			{
				Name = StringName.op_Implicit("NecroOstyNativeHit"),
				Centered = false,
				RegionEnabled = true,
				RegionFilterClipEnabled = true,
				Visible = false,
				TextureFilter = (TextureFilterEnum)2
			};
			Sprite2D val2 = val;
			sprites[num] = val;
			Sprite2D val3 = val2;
			parent.AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node2D)val3).GlobalTransform = Transform2D.Identity;
		}
	}

	internal void Advance(float time)
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		float num = time - 0.3f;
		for (int i = 0; i < 2; i++)
		{
			JsonElement jsonElement = _recipes[i];
			Sprite2D val = _sprites[i];
			if (num < 0f || num >= jsonElement.GetProperty("duration").GetSingle())
			{
				((CanvasItem)val).Hide();
				continue;
			}
			JsonElement property = jsonElement.GetProperty("frames");
			JsonElement jsonElement2 = property[Math.Clamp((int)(num * jsonElement.GetProperty("fps").GetSingle()), 0, property.GetArrayLength() - 1)];
			float single = jsonElement2.GetProperty("w").GetSingle();
			float single2 = jsonElement2.GetProperty("h").GetSingle();
			if (single == 0f || single2 == 0f)
			{
				((CanvasItem)val).Hide();
				continue;
			}
			int @int = jsonElement2.GetProperty("page").GetInt32();
			int int2 = jsonElement.GetProperty("textures")[@int].GetInt32();
			val.Texture = (Texture2D)(_strong ? ((object)_assets.Textures[int2]) : ((object)ClipAssets.BlueTexture(_assets.Data.Textures[int2].File)));
			val.RegionRect = new Rect2(jsonElement2.GetProperty("x").GetSingle(), jsonElement2.GetProperty("y").GetSingle(), single, single2);
			float num2 = 1f / jsonElement.GetProperty("scale").GetSingle();
			float num3 = ((i == 0 && _strong) ? _angle : 0f);
			Vector2 val2 = new Vector2(jsonElement2.GetProperty("ox").GetSingle(), jsonElement2.GetProperty("oy").GetSingle()) * num2;
			((Node2D)val).GlobalTransform = new Transform2D(num3, Vector2.One * num2, 0f, _point + ((Vector2)(ref val2)).Rotated(num3));
			((CanvasItem)val).Show();
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		Sprite2D[] sprites = _sprites;
		foreach (Sprite2D val in sprites)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((CanvasItem)val).Hide();
				Material material = ((CanvasItem)val).Material;
				((CanvasItem)val).Material = null;
				if (material != null)
				{
					((GodotObject)material).Dispose();
				}
				((Node)val).QueueFree();
			}
		}
	}
}
internal sealed class OstyMotion
{
	private sealed class Pose
	{
		public float[] Palm { get; set; } = Array.Empty<float>();

		public float Heading { get; set; }

		public float[] Contact { get; set; } = Array.Empty<float>();

		public float[] Section { get; set; } = Array.Empty<float>();

		public float[] Wrist { get; set; } = Array.Empty<float>();
	}

	internal readonly record struct Sample(Transform2D Up, Vector2 Palm, Vector2 Front, Vector2 Section, float HalfWidth, float Heading, float Weight, float EdgeOpacity)
	{
		internal Transform2D Canvas => new Transform2D(new Vector2(Up.X.X, 0f - Up.X.Y), new Vector2(0f - Up.Y.X, Up.Y.Y), new Vector2(Up.Origin.X, 0f - Up.Origin.Y));

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Up = ");
			Transform2D up = Up;
			builder.Append(((object)(Transform2D)(ref up)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Palm = ");
			Vector2 val = Palm;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Front = ");
			val = Front;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Section = ");
			val = Section;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", HalfWidth = ");
			builder.Append(HalfWidth.ToString());
			builder.Append(", Heading = ");
			builder.Append(Heading.ToString());
			builder.Append(", Weight = ");
			builder.Append(Weight.ToString());
			builder.Append(", EdgeOpacity = ");
			builder.Append(EdgeOpacity.ToString());
			return true;
		}
	}

	private static readonly Dictionary<ClipAssets, Pose[]> Cached = new Dictionary<ClipAssets, Pose[]>();

	private readonly Pose[] _poses;

	private readonly Vector2 _origin;

	private readonly Vector2 _ground;

	private readonly Vector2 _hit;

	private readonly float _scale;

	private readonly bool _strong;

	private readonly bool _upper;

	internal readonly float Land;

	private readonly Vector2[] _lift;

	private readonly Vector2[] _path;

	private readonly Vector2 _end;

	private readonly Vector2 _offset;

	private readonly float _rotation;

	private readonly Vector2[] _approach = Array.Empty<Vector2>();

	private readonly Vector2[] _strike = Array.Empty<Vector2>();

	private readonly Vector2[] _follow = Array.Empty<Vector2>();

	private readonly Vector2 _upperEnd;

	internal OstyMotion(ClipAssets assets, Vector2 origin, float scale, Vector2 ground, Vector2 hit, bool strong, bool upper)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		if (!Cached.TryGetValue(assets, out _poses))
		{
			_poses = assets.Data.Extra.GetProperty("poses").Deserialize<Pose[]>(new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			});
			Cached.Add(assets, _poses);
		}
		Land = assets.Data.Extra.GetProperty("land").GetSingle();
		_origin = origin;
		_ground = ground;
		_hit = hit;
		_scale = scale;
		_strong = strong;
		_upper = upper;
		Vector2 val = Point(_poses[0].Palm);
		Vector2 val2 = Point(_poses[9].Palm);
		val2.Y = val.Y + (val2.Y - val.Y) * 0.72f;
		if (strong && !upper)
		{
			val2.Y += 120f;
		}
		Pose pose = _poses[20];
		_rotation = Mathf.DegToRad(-56f) - pose.Heading;
		Transform2D val3 = Matrix(_rotation, origin);
		Vector2 val4 = Front(pose, val3);
		_offset = ground - val4;
		_end = val3 * Local(pose.Palm) + _offset;
		float num = _end.X - val2.X;
		float num2 = val2.Y - _end.Y;
		float num3 = Mathf.DegToRad(-56f);
		float num4 = MathF.Min(num * 0.4f / MathF.Cos(num3), num2 * 0.65f / (0f - MathF.Sin(num3)));
		_path = (Vector2[])(object)new Vector2[4]
		{
			val2,
			new Vector2(val2.X + num * 0.42f, val2.Y),
			_end - Vector2.FromAngle(num3) * num4,
			_end
		};
		float num5 = val2.Y - val.Y;
		_lift = (Vector2[])(object)new Vector2[4]
		{
			val,
			new Vector2(val.X, val.Y + num5 * 0.55f),
			new Vector2(val2.X - (val2.X - val.X) * 0.35f, val2.Y),
			val2
		};
		if (upper)
		{
			Sample sample = Slam(0.3f);
			Vector2 val5 = Vector2.FromAngle(Mathf.DegToRad(56f));
			Vector2 val6 = Vector2.FromAngle(sample.Heading);
			Vector2 val7 = sample.Front - sample.Palm;
			float num6 = ((Vector2)(ref val7)).Dot(val6);
			float num7 = val7.X * val6.Y - val7.Y * val6.X;
			Vector2 val8 = hit - new Vector2(val5.X * num6 - val5.Y * num7, val5.Y * num6 + val5.X * num7);
			_upperEnd = val8 + val5 * (float)(strong ? 170 : 70);
			Vector2 val9 = val8 - new Vector2(MathF.Max(140f, 44.199997f * scale / 0.28f), 75f);
			num = val9.X - val2.X;
			_approach = (Vector2[])(object)new Vector2[4]
			{
				val2,
				new Vector2(val2.X + num * 0.5f, val2.Y),
				new Vector2(val9.X - num * 0.2f, val9.Y),
				val9
			};
			_strike = (Vector2[])(object)new Vector2[4]
			{
				val9,
				val9 + new Vector2(75f, 0f),
				val8 - val5 * 65f,
				val8
			};
			_follow = (Vector2[])(object)new Vector2[4]
			{
				val8,
				val8 + val5 * 43f,
				_upperEnd - val5 * (float)(strong ? 55 : 25),
				_upperEnd
			};
		}
	}

	internal static float Smooth(float x)
	{
		x = Math.Clamp(x, 0f, 1f);
		return x * x * (3f - 2f * x);
	}

	private Vector2 Local(float[] p)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(p[0], p[1]);
	}

	private Vector2 Point(float[] p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return _origin + Local(p) * _scale;
	}

	private Transform2D Matrix(float rotation, Vector2 translation)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return new Transform2D(rotation, Vector2.One * _scale, 0f, translation);
	}

	private static (Vector2 P, Vector2 T) Curve(Vector2[] p, float t)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		t = Math.Clamp(t, 0f, 1f);
		float num = 1f - t;
		return (P: num * num * num * p[0] + 3f * num * num * t * p[1] + 3f * num * t * t * p[2] + t * t * t * p[3], T: 3f * num * num * (p[1] - p[0]) + 6f * num * t * (p[2] - p[1]) + 3f * t * t * (p[3] - p[2]));
	}

	private Pose At(float time, out Pose next, out float f)
	{
		float num = Math.Clamp(time * 60f, 0f, _poses.Length - 1);
		int num2 = (int)num;
		next = _poses[Math.Min(num2 + 1, _poses.Length - 1)];
		f = num - (float)num2;
		return _poses[num2];
	}

	private static Vector2 Front(Pose pose, Transform2D transform)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		for (int i = 0; i < pose.Contact.Length; i += 2)
		{
			Vector2 val = transform * new Vector2(pose.Contact[i], pose.Contact[i + 1]);
			num = MathF.Min(num, val.X);
			num2 = MathF.Max(num2, val.X);
			num3 = MathF.Min(num3, val.Y);
		}
		return new Vector2((num + num2) / 2f, num3);
	}

	private Sample Slam(float time)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		Pose next;
		float f;
		Pose pose = At(time, out next, out f);
		Vector2 val = Local(pose.Palm);
		Vector2 val2 = ((Vector2)(ref val)).Lerp(Local(next.Palm), f);
		float num = Mathf.LerpAngle(pose.Heading, next.Heading, f);
		float num2 = 1f - Smooth((time - 0.6f) / 0.3f);
		float weight = Smooth(time / 0.15f) * num2;
		float num4;
		Vector2 translation;
		if (time < Land)
		{
			bool flag = time >= 0.15f;
			float num3 = Math.Clamp((time - 0.15f) / (Land - 0.15f), 0f, 1f);
			(Vector2, Vector2) tuple = (flag ? Curve(_path, num3 * num3) : Curve(_lift, Smooth(time / 0.15f)));
			num4 = ((flag ? ((Vector2)(ref tuple.Item2)).Angle() : 0f) - num) * (flag ? 1f : Smooth(time / 0.15f));
			Vector2 item = tuple.Item1;
			val = val2 * _scale;
			translation = item - ((Vector2)(ref val)).Rotated(num4);
		}
		else
		{
			num4 = _rotation * num2;
			translation = _origin + _offset * num2;
		}
		Transform2D val3 = Matrix(num4, translation);
		val = new Vector2(pose.Section[0], pose.Section[1]);
		Vector2 val4 = ((Vector2)(ref val)).Lerp(new Vector2(next.Section[0], next.Section[1]), f);
		return new Sample(val3, val3 * val2, Front(pose, val3), val3 * val4, Mathf.Lerp(pose.Section[2], next.Section[2], f) * _scale, num + num4, weight, 0f);
	}

	internal Sample Get(float time, bool edge = false)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		Sample result = Slam(time);
		if (!_upper)
		{
			return result;
		}
		float num = 1f - Smooth((time - 0.6f) / 0.3f);
		float num2 = result.Heading;
		Vector2 val = result.Palm;
		if (time >= 0.15f && time < Land)
		{
			(Vector2, Vector2) tuple;
			if (!(time < 0.26f))
			{
				tuple = ((!(time < 0.3f)) ? Curve(_follow, (time - 0.3f) / (Land - 0.3f)) : Curve(_strike, (time - 0.26f) / 0.04f));
			}
			else
			{
				float num3 = Math.Clamp((time - 0.15f) / 0.11f, 0f, 1f);
				tuple = Curve(_approach, num3 * num3);
			}
			val = tuple.Item1;
			num2 = ((Vector2)(ref tuple.Item2)).Angle();
		}
		else if (time >= Land)
		{
			((Vector2)(ref val))..ctor(result.Palm.X + (_upperEnd.X - _end.X) * num, result.Palm.Y + (_upperEnd.Y - _end.Y - 2f * (result.Palm.Y - _end.Y)) * num);
			num2 += Mathf.DegToRad(112f) * num;
		}
		float num4 = Smooth((time - 0.215f) / 0.04f) * (1f - Smooth((time - 0.7f) / 0.08f));
		float num5 = MathF.Cos((float)Math.PI * num4);
		float num6 = (edge ? 0.24f : num5);
		float num7 = MathF.Cos(result.Heading);
		float num8 = MathF.Sin(result.Heading);
		float num9 = MathF.Cos(num2);
		float num10 = MathF.Sin(num2);
		Transform2D val2 = default(Transform2D);
		((Transform2D)(ref val2))..ctor(new Vector2(num9 * num7 + num10 * num6 * num8, num10 * num7 - num9 * num6 * num8), new Vector2(num9 * num8 - num10 * num6 * num7, num10 * num8 + num9 * num6 * num7), Vector2.Zero);
		val2.Origin = val - val2 * result.Palm;
		return new Sample(val2 * result.Up, val, val2 * result.Front, val2 * result.Section, result.HalfWidth * MathF.Abs(num6), num2, result.Weight, 1f - Smooth(MathF.Abs(num5) / 0.4f));
	}

	internal Transform2D Wrist(float time, Sample sample)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		Pose b;
		float f;
		Pose a = At(time, out b, out f);
		Transform2D val = sample.Up * new Transform2D(new Vector2(V(0), V(2)), new Vector2(V(1), V(3)), new Vector2(V(4), V(5)));
		return new Transform2D(new Vector2(val.X.X, 0f - val.X.Y), new Vector2(val.Y.X, 0f - val.Y.Y), new Vector2(val.Origin.X, 0f - val.Origin.Y));
		float V(int i)
		{
			return Mathf.Lerp(a.Wrist[i], b.Wrist[i], f);
		}
	}
}
internal sealed class OstyParticles : IDisposable
{
	private readonly ColorBatch _back;

	private readonly ColorBatch _front;

	private readonly float _size;

	internal OstyParticles(Node back, Node front, float scale)
	{
		_back = new ColorBatch(back, "NecroOstyInnerLights");
		_front = new ColorBatch(front, "NecroOstyOuterSpecks");
		_size = MathF.Pow(scale / 1.0666667f, 0.7f);
	}

	private static float Noise(int n)
	{
		double num = Math.Sin((double)n * 127.1 + 38.71) * 43758.5453;
		return (float)(num - Math.Floor(num));
	}

	private static Color Color(string html, float alpha)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Color result = default(Color);
		((Color)(ref result))..ctor(html);
		result.A = alpha;
		return result;
	}

	private static void Glint(ColorBatch batch, Vector2 p, float r, string color, float alpha)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Color color2 = Color(color, alpha);
		batch.Triangle(p + new Vector2(0f - r, 0f), p + new Vector2(0f, r * 1.5f), p + new Vector2(r, 0f), color2);
		batch.Triangle(p + new Vector2(0f - r, 0f), p + new Vector2(r, 0f), p + new Vector2(0f, (0f - r) * 1.5f), color2);
	}

	private static void Shard(ColorBatch batch, Vector2 p, float r, float angle, string color, float alpha)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		Color color2 = Color("#0c1831", alpha);
		batch.Triangle(At(-0.65f, -1f), At(0.3f, 1.6f), At(0.8f, 0.1f), color2);
		batch.Triangle(At(-0.65f, -1f), At(0.8f, 0.1f), At(0f, -0.9f), color2);
		batch.Triangle(At(-0.35f, -0.45f), At(0.22f, 1.2f), At(0.56f, 0.05f), Color(color, alpha));
		Vector2 At(float x, float y)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = p;
			Vector2 val2 = new Vector2(x * r, y * r);
			return val + ((Vector2)(ref val2)).Rotated(angle);
		}
	}

	private static void Light(ColorBatch batch, Vector2 p, float r, float angle, float alpha)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		Color ca = default(Color);
		((Color)(ref ca))..ctor(46f / 51f, 50f / 51f, 1f, 0.65f * alpha);
		Color val = default(Color);
		((Color)(ref val))..ctor(64f / 85f, 0.9411765f, 1f, 0.2f * alpha);
		Color val2 = default(Color);
		((Color)(ref val2))..ctor(0.7058824f, 46f / 51f, 1f, 0f);
		for (int i = 0; i < 16; i++)
		{
			float theta2 = (float)i * ((float)Math.PI * 2f) / 16f;
			float theta3 = (float)(i + 1) * ((float)Math.PI * 2f) / 16f;
			Vector2 val3 = At(theta2, 0.4f);
			Vector2 c = At(theta3, 0.4f);
			Vector2 b = At(theta2, 1f);
			Vector2 val4 = At(theta3, 1f);
			batch.Triangle(p, val3, c, ca, val, val);
			batch.Triangle(val3, b, val4, val, val2, val2);
			batch.Triangle(val3, val4, c, val, val2, val);
		}
		Vector2 At(float theta, float radius)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val5 = p;
			Vector2 val6 = new Vector2(MathF.Cos(theta), MathF.Sin(theta) * 0.4f) * r * radius;
			return val5 + ((Vector2)(ref val6)).Rotated(angle);
		}
	}

	internal void Advance(float time, OstyRibbon ribbon, OstyMotion.Sample hand, float aura)
	{
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		_back.Clear();
		_front.Clear();
		float alpha = ribbon.Alpha;
		OstyRibbon.FlowPoint[] points = ribbon.Points;
		if (alpha > 0f)
		{
			for (int i = 0; i < 7; i++)
			{
				OstyRibbon.FlowPoint flowPoint = points[9 + i * 5];
				float num = flowPoint.Width * (Noise(i + 20) - 0.5f) * 0.55f;
				float r = (5f + Noise(i + 30) * 5f) * _size;
				Light(_back, flowPoint.Position + flowPoint.Normal * num, r, MathF.Atan2(0f - flowPoint.Normal.X, flowPoint.Normal.Y), alpha * 0.34f);
			}
			for (int j = 0; j < 12; j++)
			{
				OstyRibbon.FlowPoint flowPoint2 = points[8 + j * 7 % 36];
				float num2 = flowPoint2.Width * (float)((j % 2 != 0) ? 1 : (-1)) * (1.23f + Noise(j + 3) * 0.7f);
				float r2 = (4.5f + Noise(j + 50) * 5f) * _size;
				Shard(_back, flowPoint2.Position + flowPoint2.Normal * num2, r2, (float)j + time * 0.7f, (j % 3 != 0) ? "#428fca" : "#245384", alpha * (0.6f + Noise(j + 60) * 0.18f));
			}
			for (int k = 0; k < 23; k++)
			{
				OstyRibbon.FlowPoint flowPoint3 = points[5 + k * 11 % 41];
				float num3 = flowPoint3.Width * ((k % 4 == 0) ? ((Noise(k) - 0.5f) * 0.6f) : ((float)((k % 2 != 0) ? 1 : (-1)) * (1.65f + Noise(k + 25) * 1.1f)));
				Glint((k % 3 != 0) ? _front : _back, flowPoint3.Position + flowPoint3.Normal * num3, (1.5f + Noise(k + 80) * 2f) * _size, (k % 4 != 0) ? "#93d4fc" : "#d8efff", alpha * (0.52f + Noise(k + 90) * 0.25f));
			}
		}
		if (aura > 0f)
		{
			Vector2 val = Vector2.FromAngle(hand.Heading);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))..ctor(0f - val.Y, val.X);
			for (int l = 0; l < 13; l++)
			{
				float x = (float)l * 2.399f + time * 0.8f;
				float num4 = hand.HalfWidth * (1.25f + Noise(l + 210) * 0.4f);
				Vector2 p = hand.Palm + val * (MathF.Cos(x) * num4 * 0.95f) + val2 * (MathF.Sin(x) * num4);
				if (l % 3 == 0)
				{
					Shard(_back, p, (3.5f + Noise(l + 220) * 3f) * _size, (float)l + time * 0.7f, "#3a7ab0", aura * 0.65f);
				}
				else
				{
					Glint((l % 2 != 0) ? _front : _back, p, (1.4f + Noise(l + 230) * 1.7f) * _size, (l % 3 != 0) ? "#a3dafa" : "#d8edff", aura * 0.64f);
				}
			}
		}
		_back.Commit();
		_front.Commit();
	}

	public void Dispose()
	{
		_back.Dispose();
		_front.Dispose();
	}
}
internal sealed class OstyRibbon : IDisposable
{
	internal readonly record struct FlowPoint(Vector2 Position, Vector2 Normal, float Width)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Position = ");
			Vector2 val = Position;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Normal = ");
			val = Normal;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Width = ");
			builder.Append(Width.ToString());
			return true;
		}
	}

	private static readonly Shader Shader = new Shader
	{
		Code = "shader_type canvas_item;\nrender_mode unshaded;\nuniform sampler2D flame_texture : filter_linear, repeat_disable;\nuniform float frame_index;\nuniform float opacity;\nvoid fragment(){vec2 cell=vec2(mod(frame_index,7.),floor(frame_index/7.));vec2 uv=(cell+clamp(UV,vec2(.5/384.,.5/128.),vec2(1.-.5/384.,1.-.5/128.)))/vec2(7.,4.);COLOR=texture(flame_texture,uv);COLOR.a*=opacity;}"
	};

	private static readonly StringName Frame = StringName.op_Implicit("frame_index");

	private static readonly StringName Opacity = StringName.op_Implicit("opacity");

	private static readonly int[] Indices = Enumerable.Range(0, 48).SelectMany((int i) => new int[6]
	{
		i * 2,
		i * 2 + 1,
		i * 2 + 2,
		i * 2 + 1,
		i * 2 + 3,
		i * 2 + 2
	}).ToArray();

	private static readonly Color[] White = (Color[])(object)new Color[1] { Colors.White };

	private readonly Vector2[] _points = (Vector2[])(object)new Vector2[98];

	private readonly Vector2[] _uvs = Enumerable.Range(0, 49).SelectMany((int i) => (IEnumerable<Vector2>)(object)new Vector2[2]
	{
		new Vector2((float)i / 48f, 0f),
		new Vector2((float)i / 48f, 1f)
	}).ToArray();

	private readonly Node2D _node;

	private readonly ShaderMaterial _material;

	private readonly OstyMotion _motion;

	private readonly bool _strong;

	private readonly OstyMotion.Sample[] _track;

	internal readonly FlowPoint[] Points = new FlowPoint[49];

	private bool _disposed;

	internal float Alpha { get; private set; }

	internal OstyRibbon(Node parent, OstyMotion motion, bool strong)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		OstyRibbon ostyRibbon = this;
		_motion = motion;
		_strong = strong;
		_track = (from i in Enumerable.Range(0, 81)
			select motion.Get((float)i / 80f * motion.Land)).ToArray();
		_material = new ShaderMaterial
		{
			Shader = Shader
		};
		_material.SetShaderParameter(StringName.op_Implicit("flame_texture"), Variant.op_Implicit((GodotObject)(object)ClipAssets.Texture("osty-flame-" + (strong ? "advanced" : "basic") + ".png")));
		_node = new Node2D
		{
			Name = StringName.op_Implicit("NecroOstyFlameRibbon"),
			Material = (Material)(object)_material,
			Visible = false
		};
		parent.AddChild((Node)(object)_node, false, (InternalMode)0);
		_node.GlobalTransform = Transform2D.Identity;
		((CanvasItem)_node).Draw += delegate
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)ostyRibbon._node).GetCanvasItem(), (ReadOnlySpan<int>)Indices, (ReadOnlySpan<Vector2>)ostyRibbon._points, (ReadOnlySpan<Color>)White, (ReadOnlySpan<Vector2>)ostyRibbon._uvs, ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
		};
	}

	private (Vector2 P, float Width, Vector2 Normal) Sample(float time)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Clamp(time / _motion.Land, 0f, 1f) * 80f;
		int num2 = Math.Min(79, (int)num);
		float num3 = num - (float)num2;
		OstyMotion.Sample sample = _track[num2];
		OstyMotion.Sample sample2 = _track[num2 + 1];
		float x = Mathf.LerpAngle(sample.Heading, sample2.Heading, num3);
		Vector2 section = sample.Section;
		return (P: ((Vector2)(ref section)).Lerp(sample2.Section, num3), Width: Mathf.Lerp(sample.HalfWidth, sample2.HalfWidth, num3), Normal: new Vector2(0f - MathF.Sin(x), MathF.Cos(x)));
	}

	internal void Advance(float time, OstyMotion.Sample hand)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		float num = time - _motion.Land;
		float num2 = (_strong ? 0.23f : 0.16f);
		if (time <= 0.15f || num >= num2)
		{
			Alpha = 0f;
			((CanvasItem)_node).Hide();
			return;
		}
		float num3 = MathF.Pow(1f - Math.Clamp(num / num2, 0f, 1f), 1.7f);
		float num4 = MathF.Min(time, _motion.Land);
		float num5 = MathF.Max(0.12f, num4 - (_strong ? 0.18f : 0.115f));
		(Vector2, float, Vector2) tuple = Sample(num4);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))..ctor(0f - MathF.Sin(hand.Heading), MathF.Cos(hand.Heading));
		for (int i = 0; i <= 48; i++)
		{
			float num6 = (float)i / 48f;
			float num7 = Mathf.Lerp(num5, num4, num6);
			float num8 = OstyMotion.Smooth((num6 - 0.65f) / 0.35f);
			(Vector2 P, float Width, Vector2 Normal) tuple2 = Sample(num7);
			Vector2 val2 = Sample(num7 + 0.001f).P - Sample(num7 - 0.001f).P;
			Vector2 val3 = new Vector2(0f - val2.Y, val2.X);
			val3 = ((Vector2)(ref val3)).Normalized();
			val3 = ((Vector2)(ref val3)).Lerp(val, num8);
			Vector2 val4 = ((Vector2)(ref val3)).Normalized();
			Vector2 val5 = tuple2.P + (hand.Section - tuple.Item1) * num8;
			float num9 = hand.HalfWidth * MathF.Pow(num6, 1.05f) * (1f + (_strong ? 0.24f : 0f) * MathF.Sin((float)Math.PI * num6));
			Points[i] = new FlowPoint(val5, val4, num9 / 1.52f);
			for (int j = 0; j < 2; j++)
			{
				Vector2 val6 = val5 + val4 * num9 * (float)((j != 0) ? 1 : (-1));
				_points[i * 2 + j] = new Vector2(val6.X, 0f - val6.Y);
			}
		}
		Alpha = num3;
		_material.SetShaderParameter(Frame, Variant.op_Implicit((float)Math.Clamp((int)(time * 48f), 0, 27)));
		_material.SetShaderParameter(Opacity, Variant.op_Implicit(num3));
		((CanvasItem)_node).Show();
		((CanvasItem)_node).QueueRedraw();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_node))
			{
				((CanvasItem)_node).Hide();
				((CanvasItem)_node).Material = null;
				((Node)_node).QueueFree();
			}
			((GodotObject)_material).Dispose();
		}
	}
}
internal sealed class OstySkinEffect : IVisualEffect, IDisposable
{
	private readonly OstyHit _hit;

	private readonly RadialFeedback? _blur;

	private readonly VisualShake _shake = new VisualShake();

	private bool _shaken;

	private readonly float _angle;

	public float Duration => 2.55f;

	public bool Hold => false;

	internal OstySkinEffect(NCreature target, NCombatRoom room, bool strong)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_angle = (((OstyEffect.SelectionForTests?.Invoke() ?? RandomNumberGenerator.GetInt32(100)) >= AnimationSettings.Current.OstySlamChance) ? (-56) : 56);
		_hit = new OstyHit((Node)(object)room.CombatVfxContainer, target.VfxSpawnPosition, strong, Mathf.DegToRad(_angle));
		if (strong)
		{
			_blur = new RadialFeedback((Node)(object)room.CombatVfxContainer);
		}
	}

	public void Advance(float age)
	{
		_hit.Advance(age);
		_blur?.Advance(age);
		if (_blur != null && !_shaken && age >= 0.4f)
		{
			_shaken = true;
			_shake.Play(40f, 0.3, _angle);
		}
	}

	public void CancelBody()
	{
	}

	public void Dispose()
	{
		_hit.Dispose();
		_blur?.Dispose();
		_shake.Dispose();
	}
}
internal static class PresentationPreloader
{
	internal static bool Ready { get; private set; }

	internal static Exception? Failure { get; private set; }

	internal static async Task Warm()
	{
		SceneTree tree = (SceneTree)Engine.GetMainLoop();
		SubViewport view = null;
		try
		{
			while (true)
			{
				NGame instance = NGame.Instance;
				if (((instance != null) ? instance.MainMenu : null) != null)
				{
					break;
				}
				await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
			}
			foreach (Type item in from t in typeof(ModEntry).Assembly.GetTypes()
				where !t.ContainsGenericParameters
				select t)
			{
				foreach (MethodBase item2 in item.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Cast<MethodBase>().Concat(item.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)))
				{
					if (!item2.IsAbstract && !item2.ContainsGenericParameters && item2.GetMethodBody() != null)
					{
						RuntimeHelpers.PrepareMethod(item2.MethodHandle);
					}
				}
			}
			view = new SubViewport
			{
				Name = StringName.op_Implicit("NecroPresentationWarmup"),
				Size = new Vector2I(128, 128),
				World2D = new World2D(),
				TransparentBg = true,
				Disable3D = true,
				RenderTargetUpdateMode = (UpdateMode)4
			};
			((Node)tree.Root).AddChild((Node)(object)view, false, (InternalMode)0);
			((Viewport)view).CanvasTransform = new Transform2D(0f, Vector2.One * 0.05f, 0f, Vector2.Zero);
			Node2D hero = Actor("necrobinder");
			Node2D osty = Actor("osty");
			SpineCompatibility.Allows(hero, "necrobinder");
			SpineCompatibility.Allows(osty, "osty");
			if (AnimationSettings.Current.Allows("BansheesCry"))
			{
				using NativeBansheeScream scream = new NativeBansheeScream((Node)(object)view, Vector2.Zero, 481u);
				scream.Advance(0.12f);
				await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
				await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
			}
			foreach (string item3 in Hooks.Cards.Concat(new <>z__ReadOnlyArray<string>(new string[4] { "OstyBody", "OstyBodyStrong", "EndImpact", "EndAnticipation" })))
			{
				if (ClipAssets.Exists(item3) && (!Hooks.Cards.Contains(item3) || AnimationSettings.Current.Allows(item3)))
				{
					using ClipPlayer clip = new ClipPlayer(ClipAssets.Get(item3), hero, osty, (Node)(object)view, ownBody: false, (Node?)(object)view);
					clip.Advance(0.3f);
					await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
					clip.Advance(0.7f);
					await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
				}
			}
			if (SpineCompatibility.Allows(hero, "necrobinder"))
			{
				if (AnimationSettings.Current.ReaperFormEnabled)
				{
					using ReaperFormEffect form = new ReaperFormEffect(hero, osty, (Node)(object)view);
					float[] array = new float[4] { 0.2f, 0.6f, 1.3f, 2f };
					foreach (float age in array)
					{
						form.Advance(age);
						await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
					}
				}
				using (LethalityEffect ornament = new LethalityEffect(hero))
				{
					ornament.Advance(0.35f);
					await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
					ornament.Advance(0.9f);
					await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
				}
				ShaderMaterial material = new ShaderMaterial
				{
					Shader = NeurosurgeEffect.Shader
				};
				try
				{
					using (new ActorCapture(hero, (Node)(object)view, 512, 60f, material))
					{
						material.SetShaderParameter(StringName.op_Implicit("strength"), Variant.op_Implicit(1f));
						await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
					}
				}
				finally
				{
					((IDisposable)material)?.Dispose();
				}
			}
			ClipAssets programs = ClipAssets.Get("EndPrograms");
			for (int i = 0; i < 2; i++)
			{
				using EndMeteorView meteor = new EndMeteorView((Node)(object)view, new EndMeteor((i == 0) ? 7 : 9, 571, new Vector2(1298f, 694f)), programs);
				meteor.Advance(0.78f);
				await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
				meteor.Advance(1.1f);
				await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
			}
			using (RadialFeedback blur = new RadialFeedback((Node)(object)view))
			{
				blur.Advance(0.4f);
				await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
			}
			if (AnimationSettings.Current.Allows("Eradicate"))
			{
				EradicateAssets.Prepare();
				EradicateSurface.Prepare();
				using EradicateBatch spikes = new EradicateBatch(hero, rear: false);
				spikes.Quad((Vector2[])(object)new Vector2[4]
				{
					new Vector2(0f, 0f),
					new Vector2(10f, 0f),
					new Vector2(10f, 60f),
					new Vector2(0f, 60f)
				}, 0.65f, 1f, Array.Empty<Vector2>());
				spikes.Commit();
				await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
			}
			Ready = true;
		}
		catch (Exception ex)
		{
			Failure = ex;
			ModEntry.Log(ex);
		}
		finally
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)view))
			{
				((Node)view).QueueFree();
			}
		}
		Node2D Actor(string name)
		{
			NCreatureVisuals val = ResourceLoader.Load<PackedScene>("res://scenes/creature_visuals/" + name + ".tscn", (string)null, (CacheMode)1).Instantiate<NCreatureVisuals>((GenEditState)0);
			((Node)view).AddChild((Node)(object)val, false, (InternalMode)0);
			return val.GetCurrentBody();
		}
	}
}
internal sealed class RadialFeedback : IDisposable
{
	private static readonly Shader Shader = new Shader
	{
		Code = "shader_type canvas_item;\nrender_mode unshaded, blend_disabled;\nuniform sampler2D screen : hint_screen_texture, repeat_disable, filter_linear;\nuniform float power=0.0;\nvoid fragment(){vec2 center=vec2(.6,.55);vec4 result=vec4(0.0);for(int i=0;i<12;i++)result+=texture(screen,center+(SCREEN_UV-center)*(1.0-power*float(i)));COLOR=result/12.0;}"
	};

	private readonly Node2D _root = new Node2D
	{
		Name = StringName.op_Implicit("NecroMechaFeedback")
	};

	private readonly ShaderMaterial _material = new ShaderMaterial
	{
		Shader = Shader
	};

	private readonly ColorRect _quad;

	private bool _disposed;

	internal RadialFeedback(Node parent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		parent.AddChild((Node)(object)_root, false, (InternalMode)0);
		_root.GlobalTransform = Transform2D.Identity;
		BattleScreen.Copy((Node)(object)_root, "NecroFeedbackSceneCopy");
		_quad = new ColorRect
		{
			Material = (Material)(object)_material,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)_root).AddChild((Node)(object)_quad, false, (InternalMode)0);
		((CanvasItem)_root).Hide();
	}

	internal void Advance(float age)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		float num = age - 0.3f;
		((CanvasItem)_root).Visible = num >= 0f && num < 1f;
		if (((CanvasItem)_root).Visible)
		{
			Rect2 val = BattleScreen.Bounds((CanvasItem)(object)_root);
			((Control)_quad).Position = ((Rect2)(ref val)).Position;
			((Control)_quad).Size = ((Rect2)(ref val)).Size;
			_material.SetShaderParameter(StringName.op_Implicit("power"), Variant.op_Implicit(0.005f * ((num < 0.1f) ? (num / 0.1f) : (1f - MathF.Pow((num - 0.1f) / 0.9f, 3f)))));
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				((CanvasItem)_root).Hide();
				((Node)_root).QueueFree();
			}
			((GodotObject)_material).Dispose();
		}
	}
}
internal sealed class ReaperFormEffect : IVisualEffect, IDisposable
{
	private sealed class Track
	{
		public string Name { get; set; } = "";

		public string Key { get; set; } = "";

		public float Duration { get; set; }
	}

	private sealed class Recipe
	{
		public float Duration { get; set; }

		public Track[] Tracks { get; set; } = Array.Empty<Track>();
	}

	private static readonly Dictionary<Node2D, ReaperFormEffect> Active = new Dictionary<Node2D, ReaperFormEffect>(ReferenceEqualityComparer.Instance);

	private static Recipe? _recipe;

	private readonly Node2D _body;

	private readonly SpineValues _values;

	private readonly ClipPlayer _clip;

	private readonly ReaperRavens _ravens;

	private string _track = "idle_loop";

	private float _age;

	private float _entrance;

	private float _ravenEntrance;

	private bool _disposed;

	private readonly List<NativeFormVisibility> _native = new List<NativeFormVisibility>();

	private readonly NCreature? _owner;

	internal static IEnumerable<string> AssetNames
	{
		get
		{
			Preload();
			return _recipe.Tracks.Select((Track t) => t.Key).Concat(new <>z__ReadOnlyArray<string>(new string[5] { "Reaper_Dirge", "Reaper_Scourge", "Reaper_GraveWarden", "Reaper_CaptureSpirit", "Reaper_BansheesCry" }));
		}
	}

	internal float Phase => Math.Clamp((_age - _entrance) / _recipe.Duration, 0f, 1f);

	internal bool Rendered => _clip.BodyRendered;

	internal float SoulWeight
	{
		get
		{
			float num = Math.Clamp((Phase - 0.1f) / 0.6f, 0f, 1f);
			return num * num * (3f - 2f * num);
		}
	}

	public float Duration
	{
		get
		{
			if (!_disposed)
			{
				return float.PositiveInfinity;
			}
			return 0f;
		}
	}

	public bool Hold => !_disposed;

	internal static ReaperFormEffect? For(Node2D body)
	{
		return Active.GetValueOrDefault(body);
	}

	internal static void Preload()
	{
		if (_recipe == null)
		{
			_recipe = JsonSerializer.Deserialize<Recipe>(File.ReadAllText(Path.Combine(ClipAssets.Directory, "Reaper-" + ClipAssets.Version + ".formdata")), new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			}) ?? throw new InvalidDataException("Form recipe");
		}
	}

	internal ReaperFormEffect(Node2D body, Node2D osty, Node lifetime, bool restored = false)
	{
		ReaperFormEffect reaperFormEffect = this;
		Preload();
		_body = body;
		_values = new SpineValues(body);
		_entrance = (_ravenEntrance = (restored ? (-10) : 0));
		_clip = new ClipPlayer(ClipAssets.Get("Reaper_idle_loop"), body, body, lifetime);
		_clip.BodyPermit = () => !BodyDisplay.OtherBody(body, reaperFormEffect._clip);
		_ravens = new ReaperRavens(body, lifetime, _values, restored);
		Active.Add(body, this);
		Node parent = ((Node)body).GetParent();
		while (parent != null && !(parent is NCreature))
		{
			parent = parent.GetParent();
		}
		_owner = (NCreature?)(object)((parent is NCreature) ? parent : null);
		if (parent != null)
		{
			foreach (CanvasItem item in from n in NativeVfxVisibility.Descendants(parent).OfType<CanvasItem>()
				where ((object)n).GetType().Name == "NReaperFormVfx"
				select n)
			{
				HideNative(item);
			}
		}
		Advance(0f);
	}

	internal void HideNative(CanvasItem node)
	{
		if (!_disposed && !_native.Any((NativeFormVisibility v) => v.Owns(node)))
		{
			_native.Add(new NativeFormVisibility(node));
		}
	}

	internal void Recast()
	{
		_ravenEntrance = _age;
		_ravens.Restart();
	}

	public void CancelBody()
	{
	}

	internal void YieldBody()
	{
		_clip.YieldBody();
	}

	internal void ResumeBody(ClipPlayer previous)
	{
		if (!_disposed && previous != _clip)
		{
			RefreshBody();
		}
	}

	public void Advance(float age)
	{
		if (!_disposed)
		{
			_age = age;
			RefreshBody();
			if (!_disposed)
			{
				_ravens.Advance(age - _ravenEntrance);
			}
		}
	}

	private void RefreshBody()
	{
		if (!_values.Current)
		{
			Dispose();
			return;
		}
		(string Name, float Time) state = _values.Animation();
		Track track = _recipe.Tracks.FirstOrDefault((Track t) => t.Name == state.Name);
		if (track == null)
		{
			_clip.YieldBody();
			return;
		}
		if (_track != track.Name)
		{
			_clip.SwitchFormClip(ClipAssets.Get(track.Key));
			_track = track.Name;
		}
		float age = ((state.Name.EndsWith("_loop", StringComparison.Ordinal) && track.Duration > 0f) ? (state.Time % track.Duration) : Math.Min(state.Time, track.Duration));
		_clip.Visible(visible: true);
		_clip.Advance(age);
		_clip.FormPhase(Phase);
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (Active.GetValueOrDefault(_body) == this)
		{
			Active.Remove(_body);
		}
		try
		{
			_clip.Dispose();
		}
		finally
		{
			try
			{
				_ravens.Dispose();
			}
			finally
			{
				_values.Dispose();
				bool restore = _owner != null && GodotObject.IsInstanceValid((GodotObject)(object)_owner) && _owner.Entity.IsAlive && !VisualSession.IsDying(_owner) && _owner.Entity.Powers.Any((PowerModel p) => ((object)p).GetType().Name == "ReaperFormPower" && p.Amount > 0);
				foreach (NativeFormVisibility item in _native)
				{
					item.Release(restore);
				}
				_native.Clear();
			}
		}
	}
}
internal sealed class ReaperRavens : IDisposable
{
	private sealed class Frame
	{
		public int Page { get; set; }

		public float X { get; set; }

		public float Y { get; set; }

		public float Z { get; set; }

		public float Perspective { get; set; }

		public int W { get; set; }

		public int H { get; set; }

		public float Ox { get; set; }

		public float Oy { get; set; }

		public float Width { get; set; }

		public float Height { get; set; }

		public float U { get; set; }

		public float V { get; set; }
	}

	private sealed class Bird
	{
		public float Delay { get; set; }

		public float Life { get; set; }

		public float MinY { get; set; }

		public float Vertical { get; set; }

		public Frame[] Frames { get; set; } = Array.Empty<Frame>();
	}

	private sealed class Feather
	{
		public int Bird { get; set; }

		public float LiftY { get; set; }

		public float Alpha { get; set; }

		public float[] Back { get; set; } = Array.Empty<float>();

		public float[] Front { get; set; } = Array.Empty<float>();
	}

	private sealed class Recipe
	{
		public Bird[] Birds { get; set; } = Array.Empty<Bird>();

		public Feather[][] Feathers { get; set; } = Array.Empty<Feather[]>();

		public string[] Pages { get; set; } = Array.Empty<string>();

		public float[] Center { get; set; } = Array.Empty<float>();

		public float Floor { get; set; }
	}

	private sealed class Birth
	{
		public string Bone { get; set; } = "";

		public float X { get; set; }

		public float Y { get; set; }
	}

	private sealed class Batch : IDisposable
	{
		internal readonly Node2D Node = new Node2D
		{
			Name = StringName.op_Implicit("NecroBakedRavens"),
			TextureFilter = (TextureFilterEnum)2
		};

		private readonly ShaderMaterial _material;

		private readonly Vector2[] _points = (Vector2[])(object)new Vector2[784];

		private readonly Vector2[] _uvs = (Vector2[])(object)new Vector2[784];

		private readonly Color[] _colors = (Color[])(object)new Color[784];

		private readonly int[] _indices = Enumerable.Range(0, 196).SelectMany((int i) => new int[6]
		{
			i * 4,
			i * 4 + 1,
			i * 4 + 2,
			i * 4 + 2,
			i * 4 + 1,
			i * 4 + 3
		}).ToArray();

		private int _count;

		internal Batch(Node parent, Node2D body, Texture2D texture, bool front)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Expected O, but got Unknown
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			_material = new ShaderMaterial
			{
				Shader = Shader
			};
			_material.SetShaderParameter(StringName.op_Implicit("page"), Variant.op_Implicit((GodotObject)(object)texture));
			_material.SetShaderParameter(StringName.op_Implicit("front"), Variant.op_Implicit(front));
			((CanvasItem)Node).Material = (Material)(object)_material;
			parent.AddChild((Node)(object)Node, false, (InternalMode)0);
			parent.MoveChild((Node)(object)Node, front ? (((Node)BodyDisplay.FrontAnchor(body)).GetIndex(false) + 1) : ((Node)body).GetIndex(false));
			Node.GlobalTransform = Transform2D.Identity;
			((CanvasItem)Node).Draw += Draw;
			((CanvasItem)Node).Hide();
		}

		internal void Clear()
		{
			_count = 0;
		}

		internal void Add(Vector2 origin, Frame frame, float alpha, Vector2 scale)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			int num = _count * 4;
			for (int i = 0; i < 4; i++)
			{
				_points[num + i] = origin + new Vector2(frame.Ox + (float)(i % 2) * frame.Width, frame.Oy + (float)(i / 2) * frame.Height) * scale;
				_uvs[num + i] = new Vector2((frame.U + (float)(i % 2 * frame.W)) / 1024f, (frame.V + (float)(i / 2 * frame.H)) / 1024f);
				_colors[num + i] = new Color(1f, 1f, 1f, alpha);
			}
			_count++;
		}

		internal void Commit()
		{
			((CanvasItem)Node).Visible = _count > 0;
			((CanvasItem)Node).QueueRedraw();
		}

		private void Draw()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			if (_count > 0)
			{
				RenderingServer.CanvasItemAddTriangleArray(((CanvasItem)Node).GetCanvasItem(), (ReadOnlySpan<int>)_indices.AsSpan(0, _count * 6), (ReadOnlySpan<Vector2>)_points.AsSpan(0, _count * 4), (ReadOnlySpan<Color>)_colors.AsSpan(0, _count * 4), (ReadOnlySpan<Vector2>)_uvs.AsSpan(0, _count * 4), ReadOnlySpan<int>.Empty, ReadOnlySpan<float>.Empty, default(Rid), -1);
			}
		}

		public void Dispose()
		{
			((CanvasItem)Node).Material = null;
			((CanvasItem)Node).Hide();
			((Node)Node).QueueFree();
			((GodotObject)_material).Dispose();
		}
	}

	private static Recipe? _recipe;

	private static Birth[] _births = Array.Empty<Birth>();

	private static Shader? _shader;

	private readonly Node2D _body;

	private readonly SpineValues _values;

	private readonly Batch[] _back;

	private readonly Batch[] _front;

	private readonly ColorBatch _backFeathers;

	private readonly ColorBatch _frontFeathers;

	private readonly Vector2[] _origins = (Vector2[])(object)new Vector2[196];

	private readonly Vector2[] _centers = (Vector2[])(object)new Vector2[196];

	private readonly Vector2[] _scales = (Vector2[])(object)new Vector2[196];

	private readonly float[] _vertical = new float[196];

	private readonly bool[] _born = new bool[196];

	private bool _finished;

	private bool _disposed;

	private static Shader Shader
	{
		get
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			object obj = _shader;
			if (obj == null)
			{
				Shader val = new Shader
				{
					Code = "shader_type canvas_item; render_mode unshaded; uniform sampler2D page : filter_linear,repeat_disable; uniform bool front; void fragment(){vec4 sample_color=texture(page,UV);COLOR=vec4(0.0,0.0,0.0,(front?sample_color.g:sample_color.r)*COLOR.a);}"
				};
				_shader = val;
				obj = (object)val;
			}
			return (Shader)obj;
		}
	}

	internal static void Preload()
	{
		if (_recipe == null)
		{
			JsonSerializerOptions options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};
			_recipe = JsonSerializer.Deserialize<Recipe>(File.ReadAllText(Path.Combine(ClipAssets.Directory, "ReaperRavens-" + ClipAssets.Version + ".ravendata")), options);
			_births = JsonSerializer.Deserialize<Birth[]>(File.ReadAllText(Path.Combine(ClipAssets.Directory, "ReaperBirths-" + ClipAssets.Version + ".formdata")), options);
			string[] pages = _recipe.Pages;
			for (int i = 0; i < pages.Length; i++)
			{
				ClipAssets.Texture(pages[i]);
			}
		}
	}

	internal ReaperRavens(Node2D body, Node lifetime, SpineValues values, bool restored)
	{
		Preload();
		_body = body;
		_values = values;
		_finished = restored;
		Node parent = ((Node)body).GetParent();
		_back = _recipe.Pages.Select((string p) => new Batch(parent, body, (Texture2D)(object)ClipAssets.Texture(p), front: false)).ToArray();
		_front = _recipe.Pages.Select((string p) => new Batch(parent, body, (Texture2D)(object)ClipAssets.Texture(p), front: true)).ToArray();
		_backFeathers = new ColorBatch(parent, "NecroReaperFeathersBack", 3000);
		parent.MoveChild((Node)(object)_backFeathers.Node, ((Node)body).GetIndex(false));
		_frontFeathers = new ColorBatch(parent, "NecroReaperFeathersFront", 3000);
		parent.MoveChild((Node)(object)_frontFeathers.Node, ((Node)BodyDisplay.FrontAnchor(body)).GetIndex(false) + 1);
	}

	internal void Restart()
	{
		Array.Clear(_born);
		_finished = false;
	}

	private static float Smooth(float x)
	{
		x = Math.Clamp(x, 0f, 1f);
		return x * x * (3f - 2f * x);
	}

	internal void Advance(float age)
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed || _finished)
		{
			return;
		}
		Batch[] back = _back;
		for (int i = 0; i < back.Length; i++)
		{
			back[i].Clear();
		}
		back = _front;
		for (int i = 0; i < back.Length; i++)
		{
			back[i].Clear();
		}
		_backFeathers.Clear();
		_frontFeathers.Clear();
		float num = BodyDisplay.Opacity(_body);
		if (age < 1.82f)
		{
			for (int j = 0; j < _recipe.Birds.Length; j++)
			{
				Bird bird = _recipe.Birds[j];
				float num2 = age - bird.Delay;
				if (!(num2 < 0f))
				{
					if (!_born[j])
					{
						Birth birth = _births[j];
						_origins[j] = _body.GlobalTransform * _values.Matrix(birth.Bone) * new Vector2(birth.X, birth.Y);
						Vector2[] scales = _scales;
						int num3 = j;
						Transform2D globalTransform = _body.GlobalTransform;
						scales[num3] = ((Transform2D)(ref globalTransform)).Scale / 0.27f;
						_centers[j] = _body.GlobalTransform * (new Vector2(_recipe.Center[0], _recipe.Center[1]) / 0.27f);
						float val = ((_body.GlobalTransform * new Vector2(0f, _recipe.Floor / 0.27f)).Y - _origins[j].Y) / MathF.Max(0.001f, MathF.Abs(_scales[j].Y));
						_vertical[j] = ((bird.MinY < 0f) ? Math.Min(1f, Math.Max(9f, val) / (0f - bird.MinY)) : 1f) / bird.Vertical;
						_born[j] = true;
					}
					if (!(num2 >= bird.Life))
					{
						float num4 = num2 * 30f;
						int num5 = Math.Min((int)num4, bird.Frames.Length - 1);
						Frame frame = bird.Frames[num5];
						Frame frame2 = bird.Frames[Math.Min(num5 + 1, bird.Frames.Length - 1)];
						Vector2 origin = _centers[j] + (_origins[j] - _centers[j]) * Mathf.Lerp(frame.Perspective, frame2.Perspective, num4 - (float)num5) + new Vector2(Mathf.Lerp(frame.X, frame2.X, num4 - (float)num5), Mathf.Lerp(frame.Y, frame2.Y, num4 - (float)num5) * _vertical[j]) * _scales[j];
						float alpha = Smooth(num2 / 0.064f) * (1f - Smooth((num2 / bird.Life - 0.73f) / 0.27f)) * num;
						_back[frame.Page].Add(origin, frame, alpha, _scales[j]);
						_front[frame.Page].Add(origin, frame, alpha, _scales[j]);
					}
				}
			}
			Feather[] array = _recipe.Feathers[Math.Clamp((int)(age * 30f), 0, _recipe.Feathers.Length - 1)];
			foreach (Feather feather in array)
			{
				int i2;
				Color color;
				if (_born[feather.Bird])
				{
					i2 = feather.Bird;
					color = new Color(0f, 0f, 0f, feather.Alpha * num);
					Draw(feather.Back, _backFeathers);
					Draw(feather.Front, _frontFeathers);
				}
				void Draw(float[] points, ColorBatch batch)
				{
					//IL_0014: Unknown result type (might be due to invalid IL or missing references)
					//IL_0022: Unknown result type (might be due to invalid IL or missing references)
					//IL_0030: Unknown result type (might be due to invalid IL or missing references)
					//IL_0037: Unknown result type (might be due to invalid IL or missing references)
					for (int k = 0; k < points.Length; k += 9)
					{
						batch.Triangle(Point(k), Point(k + 3), Point(k + 6), color);
					}
					Vector2 Point(int at)
					{
						//IL_0018: Unknown result type (might be due to invalid IL or missing references)
						//IL_0029: Unknown result type (might be due to invalid IL or missing references)
						//IL_003a: Unknown result type (might be due to invalid IL or missing references)
						//IL_003f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0045: Unknown result type (might be due to invalid IL or missing references)
						//IL_004a: Unknown result type (might be due to invalid IL or missing references)
						//IL_0085: Unknown result type (might be due to invalid IL or missing references)
						//IL_0096: Unknown result type (might be due to invalid IL or missing references)
						//IL_009b: Unknown result type (might be due to invalid IL or missing references)
						//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
						//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
						//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
						//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
						float num6 = points[at + 2];
						Vector2 val2 = _centers[i2] + (_origins[i2] - _centers[i2]) * num6 + new Vector2(points[at], points[at + 1] + feather.LiftY * (_vertical[i2] - 1f) * num6) * _scales[i2];
						return new Vector2(val2.X, 0f - val2.Y);
					}
				}
			}
		}
		back = _back;
		for (int i = 0; i < back.Length; i++)
		{
			back[i].Commit();
		}
		back = _front;
		for (int i = 0; i < back.Length; i++)
		{
			back[i].Commit();
		}
		_backFeathers.Commit();
		_frontFeathers.Commit();
		if (age >= 1.82f)
		{
			_finished = true;
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Batch[] back = _back;
			for (int i = 0; i < back.Length; i++)
			{
				back[i].Dispose();
			}
			back = _front;
			for (int i = 0; i < back.Length; i++)
			{
				back[i].Dispose();
			}
			_backFeathers.Dispose();
			_frontFeathers.Dispose();
		}
	}
}
[HarmonyPatch(typeof(NCombatRoom), "_Ready")]
internal static class RoomPresentation
{
	[HarmonyPostfix]
	private static void Postfix(NCombatRoom __instance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			Restore(__instance);
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static async Task Restore(NCombatRoom room)
	{
		try
		{
			SceneTree tree = ((Node)room).GetTree();
			double elapsed = 0.0;
			while (GodotObject.IsInstanceValid((GodotObject)(object)room) && ((Node)room).IsInsideTree() && elapsed < 30.0)
			{
				await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				elapsed += ((Node)tree.Root).GetProcessDeltaTime();
				if (!GodotObject.IsInstanceValid((GodotObject)(object)room) || !((Node)room).IsInsideTree())
				{
					break;
				}
				if (CombatManager.Instance.IsInProgress && room.CreatureNodes.Any(delegate(NCreature c)
				{
					Player player = c.Entity.Player;
					return ((player != null) ? ((object)player.Character).GetType().Name : null) == "Necrobinder";
				}))
				{
					VisualSession.RestoreResidents(room);
					break;
				}
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal sealed class ScytheFlames : IDisposable
{
	private sealed record Emitter(GpuParticles2D Node, ParticleProcessMaterial Original, ParticleProcessMaterial Material, Rid Parent)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Node = ");
			builder.Append(Node);
			builder.Append(", Original = ");
			builder.Append(Original);
			builder.Append(", Material = ");
			builder.Append(Material);
			builder.Append(", Parent = ");
			Rid parent = Parent;
			builder.Append(((object)(Rid)(ref parent)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	private static readonly Dictionary<Node2D, ScytheFlames> Live = new Dictionary<Node2D, ScytheFlames>(ReferenceEqualityComparer.Instance);

	private static Dictionary<string, float[]>? Palette;

	private readonly Node2D _body;

	private readonly Node2D _carrier;

	private readonly List<Emitter> _emitters = new List<Emitter>();

	private bool _disposed;

	private string _style = "";

	internal static void Ensure(Node2D body)
	{
		if (!Live.ContainsKey(body) && SpineCompatibility.Allows(body, "necrobinder"))
		{
			ScytheFlames value = new ScytheFlames(body);
			Live.Add(body, value);
		}
	}

	private ScytheFlames(Node2D body)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if (Palette == null)
		{
			Palette = JsonSerializer.Deserialize<Dictionary<string, float[]>>(File.ReadAllText(Path.Combine(ClipAssets.Directory, "Scythe.flamedata")));
		}
		_body = body;
		_carrier = new Node2D
		{
			Name = StringName.op_Implicit("NecroScytheFlames")
		};
		((Node)body).GetParent().AddChild((Node)(object)_carrier, false, (InternalMode)0);
		((Node)body).GetParent().MoveChild((Node)(object)_carrier, ((Node)body).GetIndex(false));
		try
		{
			for (int i = 1; i <= 2; i++)
			{
				GpuParticles2D nodeOrNull = ((Node)body).GetNodeOrNull<GpuParticles2D>(NodePath.op_Implicit($"ScytheVfxSlot{i}/ScytheParticles"));
				if (nodeOrNull != null)
				{
					Material processMaterial = nodeOrNull.ProcessMaterial;
					ParticleProcessMaterial val = (ParticleProcessMaterial)(object)((processMaterial is ParticleProcessMaterial) ? processMaterial : null);
					if (val != null)
					{
						ParticleProcessMaterial material = (ParticleProcessMaterial)(object)(nodeOrNull.ProcessMaterial = (Material)(ParticleProcessMaterial)((Resource)val).Duplicate(false));
						_emitters.Add(new Emitter(nodeOrNull, val, material, ((CanvasItem)((Node)nodeOrNull).GetParent()).GetCanvasItem()));
					}
				}
			}
			BodyDisplay.Add(body, _carrier);
			((Node)body).TreeExiting += Dispose;
			RenderingServer.FramePreDraw += Pose;
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	private void Pose()
	{
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_body) || ((GodotObject)_body).IsQueuedForDeletion())
		{
			Dispose();
			return;
		}
		ClipPlayer clipPlayer = BodyDisplay.Owner(_body);
		string text = clipPlayer?.Assets.Data.Name ?? "";
		if (clipPlayer == null && ReaperFormEffect.For(_body) == null)
		{
			Dispose();
			return;
		}
		string text2 = (text.Contains("CaptureSpirit", StringComparison.Ordinal) ? "CaptureSpirit" : (text.Contains("GraveWarden", StringComparison.Ordinal) ? "GraveWarden" : ((ReaperFormEffect.For(_body) != null) ? "ReaperForm" : "native")));
		((CanvasItem)_carrier).Visible = ((CanvasItem)_body).IsVisibleInTree();
		((CanvasItem)_carrier).Modulate = clipPlayer?.DisplayTint ?? (((CanvasItem)_body).Modulate * ((CanvasItem)_body).SelfModulate);
		Transform2D globalTransform = _carrier.GlobalTransform;
		Transform2D val = ((Transform2D)(ref globalTransform)).AffineInverse();
		foreach (Emitter emitter in _emitters)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)emitter.Node) && !((GodotObject)emitter.Node).IsQueuedForDeletion() && emitter.Node.ProcessMaterial == emitter.Material)
			{
				if (_style != text2)
				{
					float[] array = Palette[text2];
					emitter.Material.Color = new Color(array[0], array[1], array[2], emitter.Original.Color.A);
				}
				RenderingServer.CanvasItemSetParent(((CanvasItem)emitter.Node).GetCanvasItem(), ((CanvasItem)_carrier).GetCanvasItem());
				RenderingServer.CanvasItemSetTransform(((CanvasItem)emitter.Node).GetCanvasItem(), val * ((Node2D)emitter.Node).GlobalTransform);
			}
		}
		_style = text2;
	}

	public void Dispose()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		Live.Remove(_body);
		RenderingServer.FramePreDraw -= Pose;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_body))
		{
			((Node)_body).TreeExiting -= Dispose;
		}
		foreach (Emitter emitter in _emitters)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)emitter.Node) && !((GodotObject)emitter.Node).IsQueuedForDeletion())
			{
				RenderingServer.CanvasItemSetParent(((CanvasItem)emitter.Node).GetCanvasItem(), emitter.Parent);
				RenderingServer.CanvasItemSetTransform(((CanvasItem)emitter.Node).GetCanvasItem(), ((Node2D)emitter.Node).Transform);
				if (emitter.Node.ProcessMaterial == emitter.Material)
				{
					emitter.Node.ProcessMaterial = (Material)(object)emitter.Original;
				}
			}
			((GodotObject)emitter.Material).Dispose();
		}
		BodyDisplay.Remove(_body, _carrier);
		if (GodotObject.IsInstanceValid((GodotObject)(object)_carrier))
		{
			((CanvasItem)_carrier).Hide();
			((Node)_carrier).QueueFree();
		}
	}
}
[HarmonyPatch(typeof(LocManager), "Initialize")]
internal static class SettingsLocalization
{
	[CompilerGenerated]
	private static class <>O
	{
		public static LocaleChangeCallback <0>__Refresh;
	}

	internal const string Table = "settings_ui";

	private static LocManager? _subscribed;

	internal static string TitleKey(string key)
	{
		return "MORENECROBINDERANIMATIONS." + key + ".title";
	}

	internal static string DescriptionKey(string key)
	{
		return "MORENECROBINDERANIMATIONS." + key + ".description";
	}

	[HarmonyPostfix]
	internal static void Install()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		try
		{
			LocManager instance = LocManager.Instance;
			if (instance == null)
			{
				return;
			}
			if (instance != _subscribed)
			{
				LocManager? subscribed = _subscribed;
				if (subscribed != null)
				{
					object obj = <>O.<0>__Refresh;
					if (obj == null)
					{
						LocaleChangeCallback val = Refresh;
						<>O.<0>__Refresh = val;
						obj = (object)val;
					}
					subscribed.UnsubscribeToLocaleChange((LocaleChangeCallback)obj);
				}
				object obj2 = <>O.<0>__Refresh;
				if (obj2 == null)
				{
					LocaleChangeCallback val2 = Refresh;
					<>O.<0>__Refresh = val2;
					obj2 = (object)val2;
				}
				instance.SubscribeToLocaleChange((LocaleChangeCallback)obj2);
				_subscribed = instance;
			}
			Refresh();
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}

	private static void Refresh()
	{
		try
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string> { 
			{
				"MORENECROBINDERANIMATIONS.mod_title",
				SettingsState.Text("Title")
			} };
			SettingsState.Option[] options = SettingsState.Options;
			foreach (SettingsState.Option option in options)
			{
				dictionary[TitleKey(option.Key)] = option.Title;
				dictionary[DescriptionKey(option.Key)] = SettingsState.Text(option.Key + "Description");
			}
			LocManager.Instance.GetTable("settings_ui").MergeWith(dictionary);
			ModManifest val = ((IEnumerable<Mod>)ModManager.Mods).FirstOrDefault((Func<Mod, bool>)((Mod mod) => mod.manifest?.id == "MoreNecrobinderAnimations"))?.manifest;
			if (val != null)
			{
				val.name = SettingsState.Text("Title");
				val.description = SettingsState.Text("Description");
			}
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
		}
	}
}
internal static class SettingsState
{
	internal sealed record Option(string Key, object Default)
	{
		internal string Title => Text(Key);

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Key = ");
			builder.Append((object?)Key);
			builder.Append(", Default = ");
			builder.Append(Default);
			return true;
		}
	}

	internal static readonly Option[] Options = new Option[18]
	{
		new Option("Enabled", true),
		new Option("ScourgeEnabled", true),
		new Option("GraveWardenEnabled", false),
		new Option("InvokeEnabled", true),
		new Option("FriendshipEnabled", true),
		new Option("NegativePulseEnabled", true),
		new Option("NeurosurgeEnabled", true),
		new Option("DeathbringerEnabled", true),
		new Option("DirgeEnabled", true),
		new Option("CaptureSpiritEnabled", true),
		new Option("EndOfDaysEnabled", true),
		new Option("BansheesCryEnabled", true),
		new Option("UnleashEnabled", true),
		new Option("ProtectorEnabled", true),
		new Option("LethalityEnabled", true),
		new Option("ReaperFormEnabled", true),
		new Option("EradicateEnabled", true),
		new Option("OstySlamChance", 70)
	};

	private static readonly Dictionary<string, Dictionary<string, string>> Texts = new Dictionary<string, Dictionary<string, string>>();

	internal static event Action? Changed;

	internal static string Text(string key, string? language = null)
	{
		if (language == null)
		{
			LocManager instance = LocManager.Instance;
			language = ((instance != null) ? instance.Language : null);
		}
		if ((!(language == "zhs") && !(language == "zht")) || 1 == 0)
		{
			language = "eng";
		}
		if (!Texts.TryGetValue(language, out Dictionary<string, string> value))
		{
			value = (Texts[language] = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine(ModEntry.DirectoryPath, "localization", language + ".locale"))));
		}
		return value.GetValueOrDefault(key, key);
	}

	internal static object Get(string key)
	{
		return typeof(AnimationSettings).GetProperty(key).GetValue(AnimationSettings.Current);
	}

	internal static void Save(string key, object? value)
	{
		Option option = Options.FirstOrDefault((Option o) => o.Key == key) ?? throw new ArgumentException("Unknown animation setting");
		if (value?.GetType() != option.Default.GetType())
		{
			throw new ArgumentException("Invalid animation setting type");
		}
		AnimationSettings animationSettings = AnimationSettings.Current.<Clone>$();
		typeof(AnimationSettings).GetProperty(key).SetValue(animationSettings, value);
		AnimationSettings.Save(animationSettings);
		SettingsState.Changed?.Invoke();
	}
}
internal static class SpineCompatibility
{
	private sealed class Stamp
	{
		internal SpineValues? Reader;

		internal bool Supported;

		internal bool OstyBody;

		internal Action? Release;

		internal string Actor = "";
	}

	private sealed record Signature(string Value);

	private static readonly ConditionalWeakTable<Node2D, Stamp> Bodies = new ConditionalWeakTable<Node2D, Stamp>();

	private static readonly ConditionalWeakTable<Resource, Signature> Signatures = new ConditionalWeakTable<Resource, Signature>();

	private static readonly ConditionalWeakTable<Resource, object> Watched = new ConditionalWeakTable<Resource, object>();

	private static JsonDocument? _approved;

	internal static bool AllowsOstyBody(Node2D body)
	{
		if (!Allows(body, "osty"))
		{
			if (OstyAddonCompatibility.Active && Bodies.TryGetValue(body, out Stamp value) && value.OstyBody)
			{
				SpineValues? reader = value.Reader;
				if (reader != null && reader.Current)
				{
					return value.Reader.DefaultSkin;
				}
			}
			return false;
		}
		return true;
	}

	internal static bool Allows(Node2D body, string actor)
	{
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)body) || !((GodotObject)body).HasMethod(StringName.op_Implicit("get_skeleton_data_res")) || !((GodotObject)body).HasMethod(StringName.op_Implicit("get_skeleton")))
		{
			return false;
		}
		if (Bodies.TryGetValue(body, out Stamp value))
		{
			SpineValues? reader = value.Reader;
			if (reader != null && reader.Current)
			{
				if (value.Actor == actor)
				{
					if (value.Supported)
					{
						return value.Reader.DefaultSkin;
					}
					return false;
				}
				if (value.Supported)
				{
					return false;
				}
			}
			if (value.Release != null)
			{
				((Node)body).TreeExiting -= value.Release;
			}
			value.Reader?.Dispose();
			value.Reader = null;
		}
		Bodies.Remove(body);
		Stamp stamp = new Stamp
		{
			Actor = actor
		};
		Bodies.Add(body, stamp);
		try
		{
			if (_approved == null)
			{
				_approved = JsonDocument.Parse(File.ReadAllText(Path.Combine(ClipAssets.Directory, "rig-guards.rigdata")));
			}
			JsonElement property = _approved.RootElement.GetProperty(ClipAssets.Version).GetProperty(actor);
			SpineValues spineValues = (stamp.Reader = new SpineValues(body));
			stamp.Release = Release;
			((Node)body).TreeExiting += Release;
			Resource[] array = spineValues.Resources();
			Resource resource = array[1];
			Resource resource2 = array[2];
			Resource[] array2 = array;
			foreach (Resource resource3 in array2)
			{
				if (!Watched.TryGetValue(resource3, out object _))
				{
					Watched.Add(resource3, new object());
					resource3.Changed += Changed;
					if (((GodotObject)resource3).HasSignal(StringName.op_Implicit("_internal_spine_objects_invalidated")))
					{
						((GodotObject)resource3).Connect(StringName.op_Implicit("_internal_spine_objects_invalidated"), Callable.From((Action)Changed), 0u);
					}
				}
				void Changed()
				{
					Signatures.Remove(resource3);
				}
			}
			string text = Hash(resource, "file");
			bool flag = text == property.GetProperty("file").GetString();
			bool flag2 = actor == "osty" && OstyAddonCompatibility.Active && text == "6a748faa59ea714572fdfc9e5c3f9f55316fbc8d5299d5946877dea2944495cc";
			bool flag3 = (flag || flag2) && Hash(resource2, "atlas") == property.GetProperty("atlas").GetString();
			JsonElement property2 = property.GetProperty("textures");
			flag3 &= array.Length == 3 + property2.GetArrayLength();
			int num = 3;
			while (flag3 && num < array.Length)
			{
				flag3 = Hash(array[num], "texture") == property2[num - 3].GetString();
				num++;
			}
			flag3 &= spineValues.DefaultSkin && spineValues.OriginalLayout;
			stamp.Supported = flag && flag3;
			stamp.OstyBody = flag2 && flag3;
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			stamp.Supported = false;
		}
		return stamp.Supported;
		void Release()
		{
			((Node)body).TreeExiting -= Release;
			stamp.Reader?.Dispose();
			stamp.Reader = null;
			Bodies.Remove(body);
		}
	}

	private static string Hash(Resource resource, string kind)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (Signatures.TryGetValue(resource, out Signature value))
		{
			return value.Value;
		}
		string text;
		if (kind == "texture")
		{
			Image image = ((Texture2D)resource).GetImage();
			try
			{
				if (image.IsCompressed() && (int)image.Decompress() != 0)
				{
					throw new InvalidDataException("Displayed atlas image");
				}
				image.ClearMipmaps();
				image.Convert((Format)5);
				text = $"{image.GetWidth()}x{image.GetHeight()}:" + Convert.ToHexStringLower(SHA256.HashData(image.GetData()));
			}
			finally
			{
				((IDisposable)image)?.Dispose();
			}
		}
		else
		{
			DirAccess.MakeDirRecursiveAbsolute("user://MoreNecrobinderAnimations/compatibility");
			string text2 = "user://MoreNecrobinderAnimations/compatibility" + ((kind == "file") ? "/inspect.spskel" : "/inspect.spatlas");
			if ((int)ResourceSaver.Save(resource, text2, (SaverFlags)0) != 0)
			{
				throw new IOException("Displayed rig inspection");
			}
			string path = ProjectSettings.GlobalizePath(text2);
			if (kind == "file")
			{
				text = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path)));
			}
			else
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(path));
				string[] array = jsonDocument.RootElement.GetProperty("atlas_data").GetString().Replace("\r", "")
					.Trim()
					.Split('\n');
				bool flag = true;
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = array[i].Trim();
					if (array[i].Length == 0)
					{
						flag = true;
					}
					else if (flag)
					{
						array[i] = "<page>";
						flag = false;
					}
				}
				text = string.Join('\n', array);
			}
		}
		Signatures.Add(resource, new Signature(text));
		return text;
	}
}
internal sealed class SpineValues : IDisposable
{
	private static readonly GDScript Script = Compile();

	private readonly RefCounted _reader;

	internal Rect2 Bounds
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("bounds"), Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsRect2();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal bool Current
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("current"), Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsBool();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal float TorsoOpacity
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("torso_opacity"), Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsSingle();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal bool DefaultSkin
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("default_skin"), Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsBool();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal bool OriginalLayout
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("original_layout"), Array.Empty<Variant>());
			try
			{
				return ((Variant)(ref val)).AsBool();
			}
			finally
			{
				((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
			}
		}
	}

	internal SpineValues(Node2D body)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)Script).Call(StringName.op_Implicit("new"), Array.Empty<Variant>());
		try
		{
			_reader = (RefCounted)((Variant)(ref val)).AsGodotObject();
			Variant val2 = ((GodotObject)_reader).Call(StringName.op_Implicit("bind"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)body) });
			try
			{
			}
			finally
			{
				((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal Transform2D Matrix(string bone)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("matrix"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(bone) });
		try
		{
			return ((Variant)(ref val)).AsTransform2D();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal Resource[] Resources()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("resources"), Array.Empty<Variant>());
		try
		{
			Array val2 = ((Variant)(ref val)).AsGodotArray();
			try
			{
				Resource[] array = (Resource[])(object)new Resource[val2.Count];
				for (int i = 0; i < array.Length; i++)
				{
					Variant val3 = val2[i];
					try
					{
						array[i] = (Resource)((Variant)(ref val3)).AsGodotObject();
					}
					finally
					{
						((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
					}
				}
				return array;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	internal (string Name, float Time) Animation()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("animation"), Array.Empty<Variant>());
		try
		{
			Array val2 = ((Variant)(ref val)).AsGodotArray();
			try
			{
				Variant val3 = val2[0];
				string item = ((Variant)(ref val3)).AsString();
				val3 = val2[1];
				return (Name: item, Time: ((Variant)(ref val3)).AsSingle());
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public void Dispose()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)_reader).Call(StringName.op_Implicit("release"), Array.Empty<Variant>());
		try
		{
			((GodotObject)_reader).Dispose();
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static GDScript Compile()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		GDScript val = new GDScript
		{
			SourceCode = "extends RefCounted\nvar body\nvar data\nvar skeleton\nvar bones = {}\nvar file\nvar atlas\nvar pages\nvar skin_name\nvar dirty = false\nvar watched = []\nfunc changed():\n    dirty = true\nfunc release():\n    for resource in watched:\n        if is_instance_valid(resource):\n            if resource.changed.is_connected(changed): resource.changed.disconnect(changed)\n            if resource.has_signal(\"_internal_spine_objects_invalidated\") and resource.is_connected(\"_internal_spine_objects_invalidated\", changed): resource.disconnect(\"_internal_spine_objects_invalidated\", changed)\n    watched.clear()\nfunc watch(resource):\n    if resource in watched: return\n    watched.append(resource)\n    resource.changed.connect(changed)\n    if resource.has_signal(\"_internal_spine_objects_invalidated\"): resource.connect(\"_internal_spine_objects_invalidated\", changed)\nfunc bind(value):\n    body = value\n    data = body.get_skeleton_data_res()\n    skeleton = body.get_skeleton()\n    file = data.get_skeleton_file_res()\n    atlas = data.get_atlas_res()\n    pages = atlas.get_textures()\n    skin_name = current_skin_name()\n    for resource in [data, file, atlas] + pages: watch(resource)\nfunc current():\n    if dirty or not is_instance_valid(body) or body.get_skeleton_data_res() != data or body.get_skeleton() != skeleton: return false\n    return data.get_skeleton_file_res() == file and data.get_atlas_res() == atlas and atlas.get_textures() == pages and current_skin_name() == skin_name\nfunc current_skin_name():\n    # Spine returns a new borrowed wrapper on every get_skin() call. Its object\n    # identity is never a skin-change signal (notably for Toadpole variants).\n    var value = skeleton.get_skin()\n    return value.get_name() if value else null\nfunc resources():\n    return [data, file, atlas] + pages\nfunc default_skin():\n    var current_skin = skeleton.get_skin()\n    return current_skin == null or current_skin.get_name().to_lower() in [\"default\"]\nfunc layout(value):\n    var result = []\n    for b in value.get_bones():\n        result.append([b.get_bone_name(), b.get_parent().get_index() if b.get_parent() else -1, b.get_x(), b.get_y(), b.get_rotation(), b.get_scale_x(), b.get_scale_y(), b.get_shear_x(), b.get_shear_y(), b.get_length()])\n    for s in value.get_slots(): result.append([s.get_name(), s.get_bone_data().get_index(), s.get_attachment_name(), s.get_blend_mode()])\n    for sk in value.get_skins():\n        var names = [sk.get_name()]\n        for slot in value.get_slots(): names.append(Array(sk.find_names_for_slot(slot.get_index())))\n        result.append(names)\n    return result\nfunc original_layout():\n    var reference = ClassDB.instantiate(\"SpineSkeletonDataResource\")\n    reference.atlas_res = atlas\n    reference.skeleton_file_res = file\n    reference.update_skeleton_data()\n    return layout(skeleton.get_data()) == layout(reference)\nfunc matrix(name):\n    if not bones.has(name): bones[name] = skeleton.find_bone(name)\n    var b = bones[name]\n    if not b: return Transform2D.IDENTITY\n    return Transform2D(Vector2(b.get_a(),b.get_c()),Vector2(b.get_b(),b.get_d()),Vector2(b.get_world_x(),b.get_world_y()))\nfunc animation():\n    var entry = body.get_animation_state().get_current(0)\n    if not entry or not entry.get_animation(): return [\"\", 0.0]\n    return [entry.get_animation().get_name(), entry.get_track_time()]\nfunc bounds():\n    return skeleton.get_bounds()\nfunc torso_opacity():\n    var chest = skeleton.find_slot(\"chest\")\n    var upper = skeleton.find_bone(\"body_upper\")\n    if not chest or not chest.get_attachment() or not upper or not upper.is_active(): return 0.0\n    return skeleton.get_color().a"
		};
		if ((int)((Script)val).Reload(false) != 0)
		{
			throw new InvalidDataException("Spine value bridge");
		}
		return val;
	}
}
internal sealed class SpiritSnapshot : IDisposable
{
	private static readonly List<SpiritSnapshot> Pool = new List<SpiritSnapshot>();

	private static readonly Shader CopyShader = new Shader
	{
		Code = "shader_type canvas_item;render_mode unshaded;void fragment(){COLOR=texture(TEXTURE,UV);}"
	};

	private readonly SubViewport _viewport;

	private readonly Sprite2D _composite;

	private readonly ShaderMaterial _copyMaterial;

	private readonly NCombatRoom _room;

	private Node2D? _body;

	private SpineValues? _reader;

	private Rid _parent;

	private bool _drawing;

	private bool _disposed;

	internal bool Ready { get; private set; }

	internal bool Leased { get; private set; }

	internal Rect2 Bounds { get; private set; }

	internal Texture2D Texture => (Texture2D)(object)((Viewport)_viewport).GetTexture();

	private SpiritSnapshot(NCombatRoom room)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		_room = room;
		_viewport = new SubViewport
		{
			Name = StringName.op_Implicit("NecroSpiritSnapshot"),
			Size = new Vector2I(512, 512),
			TransparentBg = true,
			Disable3D = true,
			RenderTargetUpdateMode = (UpdateMode)0
		};
		((Node)room.CombatVfxContainer).AddChild((Node)(object)_viewport, false, (InternalMode)0);
		_copyMaterial = new ShaderMaterial
		{
			Shader = CopyShader
		};
		_composite = new Sprite2D
		{
			Name = StringName.op_Implicit("NecroSpiritCaptureReplacement"),
			Texture = Texture,
			Centered = false,
			Visible = false,
			Material = (Material)(object)_copyMaterial
		};
		((Node)room.CombatVfxContainer).AddChild((Node)(object)_composite, false, (InternalMode)0);
		((Node)room).TreeExiting += Dispose;
		RenderingServer.FramePreDraw += Before;
		RenderingServer.FramePostDraw += After;
	}

	internal static SpiritSnapshot? Rent(Node2D body, NCombatRoom room)
	{
		SpiritSnapshot spiritSnapshot = Pool.FirstOrDefault((SpiritSnapshot s) => !s.Leased && s._room == room);
		int num = 3 * Math.Max(1, room.CreatureNodes.Count((NCreature c) => c.Entity.Player != null));
		if (spiritSnapshot == null)
		{
			if (Pool.Count((SpiritSnapshot s) => s._room == room) >= num)
			{
				return null;
			}
			spiritSnapshot = new SpiritSnapshot(room);
			Pool.Add(spiritSnapshot);
		}
		spiritSnapshot.Leased = true;
		spiritSnapshot.Ready = false;
		spiritSnapshot._body = body;
		spiritSnapshot._reader = new SpineValues(body);
		return spiritSnapshot;
	}

	private void Before()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed || !Leased || Ready || _drawing || _body == null || Pool.Any((SpiritSnapshot s) => s._drawing))
		{
			return;
		}
		if (!_reader.Current)
		{
			Release();
			return;
		}
		Rect2 val = _body.GlobalTransform * _reader.Bounds;
		Bounds = ((Rect2)(ref val)).Grow(12f);
		val = Bounds;
		Vector2 size = ((Rect2)(ref val)).Size;
		if (((Vector2)(ref size)).IsFinite())
		{
			val = Bounds;
			if (!(((Rect2)(ref val)).Size.X <= 0f))
			{
				val = Bounds;
				if (!(((Rect2)(ref val)).Size.Y <= 0f))
				{
					val = Bounds;
					float num = 512f / ((Rect2)(ref val)).Size.X;
					val = Bounds;
					Vector2 val2 = default(Vector2);
					((Vector2)(ref val2))..ctor(num, 512f / ((Rect2)(ref val)).Size.Y);
					SubViewport viewport = _viewport;
					Vector2 val3 = val2;
					val = Bounds;
					((Viewport)viewport).CanvasTransform = new Transform2D(0f, val3, 0f, -((Rect2)(ref val)).Position * val2);
					Node parent = ((Node)_body).GetParent();
					_parent = ((CanvasItem)parent).GetCanvasItem();
					if (((Node)_composite).GetParent() != parent)
					{
						Node parent2 = ((Node)_composite).GetParent();
						if (parent2 != null)
						{
							parent2.RemoveChild((Node)(object)_composite);
						}
						parent.AddChild((Node)(object)_composite, false, (InternalMode)0);
					}
					parent.MoveChild((Node)(object)_composite, ((Node)_body).GetIndex(false) + 1);
					Sprite2D composite = _composite;
					val = Bounds;
					Vector2 val4 = ((Rect2)(ref val)).Size / 512f;
					val = Bounds;
					((Node2D)composite).GlobalTransform = new Transform2D(0f, val4, 0f, ((Rect2)(ref val)).Position);
					((CanvasItem)_composite).Show();
					RenderingServer.CanvasItemSetParent(((CanvasItem)_body).GetCanvasItem(), ((Viewport)_viewport).FindWorld2D().Canvas);
					RenderingServer.CanvasItemSetTransform(((CanvasItem)_body).GetCanvasItem(), _body.GlobalTransform);
					_viewport.RenderTargetUpdateMode = (UpdateMode)1;
					_drawing = true;
					return;
				}
			}
		}
		Release();
	}

	private void Restore()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (_drawing && _body != null && GodotObject.IsInstanceValid((GodotObject)(object)_body))
		{
			RenderingServer.CanvasItemSetParent(((CanvasItem)_body).GetCanvasItem(), _parent);
			RenderingServer.CanvasItemSetTransform(((CanvasItem)_body).GetCanvasItem(), _body.Transform);
		}
		_drawing = false;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_composite))
		{
			((CanvasItem)_composite).Hide();
			if (!_disposed && GodotObject.IsInstanceValid((GodotObject)(object)_room) && !((GodotObject)_room).IsQueuedForDeletion() && ((Node)_composite).GetParent() != _room.CombatVfxContainer)
			{
				((Node)_composite).Reparent((Node)(object)_room.CombatVfxContainer, true);
			}
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_viewport))
		{
			_viewport.RenderTargetUpdateMode = (UpdateMode)0;
		}
		_reader?.Dispose();
		_reader = null;
		_body = null;
	}

	private void After()
	{
		if (_drawing)
		{
			Restore();
			Ready = true;
		}
	}

	internal void Release()
	{
		Restore();
		Leased = false;
		Ready = false;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			Pool.Remove(this);
			RenderingServer.FramePreDraw -= Before;
			RenderingServer.FramePostDraw -= After;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_room))
			{
				((Node)_room).TreeExiting -= Dispose;
			}
			Release();
			if (GodotObject.IsInstanceValid((GodotObject)(object)_composite))
			{
				((CanvasItem)_composite).Material = null;
				((Node)_composite).QueueFree();
			}
			((GodotObject)_copyMaterial).Dispose();
			if (GodotObject.IsInstanceValid((GodotObject)(object)_viewport))
			{
				((Node)_viewport).QueueFree();
			}
		}
	}
}
internal static class SurfaceGeometry
{
	internal static void Bind(ClipAssets.Recipe recipe)
	{
		ClipAssets.DrawSpec[][] frames = recipe.Frames;
		foreach (ClipAssets.DrawSpec[] array in frames)
		{
			ClipAssets.DrawSpec[] array2 = array;
			foreach (ClipAssets.DrawSpec drawSpec in array2)
			{
				if (drawSpec.Surface >= 0)
				{
					if (drawSpec.Surface >= recipe.Surfaces.Length || drawSpec.SurfaceSource < 0 || drawSpec.SurfaceSource >= array.Length || drawSpec.Layout < 0 || drawSpec.Layout >= recipe.Layouts.Length)
					{
						throw new InvalidDataException("Invalid soul surface reference");
					}
					ClipAssets.SurfaceSpec surfaceSpec = recipe.Surfaces[drawSpec.Surface];
					ClipAssets.DrawSpec drawSpec2 = array[drawSpec.SurfaceSource];
					int sourceCount = ((drawSpec2.NativeMesh >= 0) ? (recipe.Layouts[drawSpec2.Layout].Uvs.Length / 2) : (drawSpec2.Positions.Length / 2));
					if (drawSpec2 == drawSpec || drawSpec2.Surface >= 0 || drawSpec2.Tag != drawSpec.Tag || drawSpec2.Target != drawSpec.Target || drawSpec.Positions.Length != 0 || surfaceSpec.Ids.Length == 0 || surfaceSpec.Ids.Length % 3 != 0 || surfaceSpec.Weights.Length != surfaceSpec.Ids.Length || recipe.Layouts[drawSpec.Layout].Uvs.Length != surfaceSpec.Ids.Length / 3 * 2 || surfaceSpec.Ids.Any((int i) => i < 0 || i >= sourceCount) || surfaceSpec.Weights.Any((double w) => !double.IsFinite(w)))
					{
						throw new InvalidDataException("Invalid soul surface binding");
					}
					drawSpec.Binding = surfaceSpec;
					drawSpec.SourceMesh = drawSpec2;
				}
			}
		}
	}

	internal static float[] Positions(ClipAssets.DrawSpec draw, NativeReaperGeometry.Sample? sample)
	{
		return sample?.Points.GetValueOrDefault(draw) ?? draw.Positions;
	}

	internal static Vector2 Point(ClipAssets.DrawSpec draw, int point, NativeReaperGeometry.Sample? sample = null)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		ClipAssets.SurfaceSpec binding = draw.Binding;
		if (binding == null)
		{
			float[] array = Positions(draw, sample);
			return new Vector2(array[point * 2], array[point * 2 + 1]);
		}
		return Weighted(binding, Positions(draw.SourceMesh, sample), point);
	}

	private static Vector2 Weighted(ClipAssets.SurfaceSpec surface, float[] source, int point)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		double num = 0.0;
		double num2 = 0.0;
		int num3 = point * 3;
		for (int i = 0; i < 3; i++)
		{
			int num4 = surface.Ids[num3 + i] * 2;
			double num5 = surface.Weights[num3 + i];
			num += (double)source[num4] * num5;
			num2 += (double)source[num4 + 1] * num5;
		}
		return new Vector2((float)num, (float)num2);
	}

	internal static void Interpolate(ClipAssets.DrawSpec a, ClipAssets.DrawSpec b, float fraction, Vector2[] points, ref float[] scratch, NativeReaperGeometry.Sample? first = null, NativeReaperGeometry.Sample? second = null)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		ClipAssets.SurfaceSpec binding = a.Binding;
		if (binding != null && binding == b.Binding && Positions(a.SourceMesh, first).Length == Positions(b.SourceMesh, second).Length)
		{
			float[] array = Positions(a.SourceMesh, first);
			float[] right = Positions(b.SourceMesh, second);
			if (scratch.Length != array.Length)
			{
				scratch = new float[array.Length];
			}
			MeshMath.Interpolate(array, right, fraction, scratch);
			for (int i = 0; i < points.Length; i++)
			{
				points[i] = Weighted(binding, scratch, i);
			}
		}
		else
		{
			for (int j = 0; j < points.Length; j++)
			{
				int num = j;
				Vector2 val = Point(a, j, first);
				points[num] = ((Vector2)(ref val)).Lerp(Point(b, j, second), fraction);
			}
		}
	}
}
internal sealed class VisualSession : IDisposable
{
	private static readonly List<VisualSession> Active = new List<VisualSession>();

	private readonly NCreature _hero;

	private readonly NCreature _actor;

	private readonly NCreature _osty;

	private readonly NCombatRoom _room;

	private readonly SceneTree _tree;

	private readonly IVisualEffect _visual;

	private readonly string _card;

	private float _age;

	private bool _disposed;

	private bool _awaitNativeAttack;

	private bool _formPowerSeen;

	internal static int Count => Active.Count;

	internal static long Starts { get; private set; }

	internal static double LastStartMs { get; private set; }

	internal static double LastUpdateMs { get; private set; }

	internal static int CountFor(NCreature hero, string name)
	{
		return Active.Count((VisualSession s) => s._hero == hero && s._card == name);
	}

	internal static float Age(string name)
	{
		return Active.FirstOrDefault((VisualSession s) => s._card == name)?._age ?? (-1f);
	}

	internal static bool Contains(string name)
	{
		return Active.Any((VisualSession s) => s._card == name);
	}

	internal static bool IsDying(NCreature actor)
	{
		Task deathAnimationTask = actor.DeathAnimationTask;
		if (deathAnimationTask != null)
		{
			return !deathAnimationTask.IsCompleted;
		}
		return false;
	}

	internal static bool HasBanshee(CardModel card)
	{
		return Active.Any((VisualSession s) => s._card == "BansheesCry" && s._hero.Entity == card.Owner.Creature);
	}

	internal static bool HasOsty(CardModel card)
	{
		return Active.Any(delegate(VisualSession s)
		{
			bool flag = s._card == ((object)card).GetType().Name;
			if (flag)
			{
				IVisualEffect visual = s._visual;
				bool flag2 = ((visual is OstyEffect || visual is OstySkinEffect) ? true : false);
				flag = flag2;
			}
			return flag && s._hero.Entity == card.Owner.Creature;
		});
	}

	internal static bool OwnsOstyBody(NCreature actor)
	{
		return Active.Any((VisualSession s) => s._actor == actor && s._visual is OstyEffect ostyEffect && ostyEffect.OwnsBody);
	}

	internal static void Start(CardModel card, bool initialAttack = false, NCreature? target = null)
	{
		string name = ((object)card).GetType().Name;
		if (!AnimationSettings.Current.Allows(name))
		{
			return;
		}
		bool flag = !ClipAssets.Exists(name);
		if (flag)
		{
			bool flag2;
			switch (name)
			{
			case "Deathbringer":
			case "Unleash":
			case "Protector":
			case "EndOfDays":
			case "Lethality":
			case "ReaperForm":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = !flag2;
		}
		if (flag || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding || !card.Owner.Creature.IsAlive)
		{
			return;
		}
		NCreature hero = card.Owner.Creature.GetCreatureNode();
		Creature osty = card.Owner.Osty;
		NCreature val = ((osty != null) ? osty.GetCreatureNode() : null);
		string text;
		if (val == null)
		{
			text = name;
			flag = ((text == "ReaperForm" || text == "Lethality") ? true : false);
			val = (flag ? hero : null);
		}
		NCreature val2 = val;
		NCombatRoom instance = NCombatRoom.Instance;
		if (hero == null || val2 == null || instance == null)
		{
			return;
		}
		switch (name)
		{
		case "Scourge":
		case "Lethality":
		case "ReaperForm":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if ((flag && !SpineCompatibility.Allows(hero.Body, "necrobinder")) || (name == "Friendship" && !SpineCompatibility.Allows(val2.Body, "osty")) || (name == "CaptureSpirit" && target == null))
		{
			return;
		}
		text = name;
		if ((text == "CaptureSpirit" || text == "EndOfDays") ? true : false)
		{
			VisualSession[] array = Active.Where((VisualSession s) => s._card == name && s._hero == hero).ToArray();
			if (array.Length >= 3)
			{
				array[0].Dispose();
			}
		}
		text = name;
		flag = ((text == "Unleash" || text == "Protector") ? true : false);
		if (flag && target == null)
		{
			return;
		}
		text = name;
		flag = ((text == "Unleash" || text == "Protector") ? true : false);
		NCreature val3 = (flag ? val2 : hero);
		for (int num = Active.Count - 1; num >= 0; num--)
		{
			if (Active[num]._hero == hero)
			{
				if (name == "Lethality" && Active[num]._visual is LethalityEffect lethalityEffect)
				{
					lethalityEffect.Recast();
					Starts++;
					return;
				}
				if (name == "ReaperForm" && Active[num]._visual is ReaperFormEffect reaperFormEffect)
				{
					reaperFormEffect.Recast();
					Starts++;
					return;
				}
				if (name == "Invoke" && Active[num]._visual is InvokeEffect invokeEffect)
				{
					invokeEffect.Recast();
					Starts++;
					return;
				}
				flag = Active[num]._card == name;
				if (flag)
				{
					bool flag2;
					switch (name)
					{
					case "Friendship":
					case "Invoke":
					case "Neurosurge":
						flag2 = true;
						break;
					default:
						flag2 = false;
						break;
					}
					flag = flag2;
				}
				if (flag)
				{
					Active[num].Dispose();
				}
				else if (Active[num]._actor == val3)
				{
					Active[num]._visual.CancelBody();
				}
			}
		}
		long timestamp = Stopwatch.GetTimestamp();
		VisualSession item = new VisualSession(name, hero, val2, instance, target)
		{
			_awaitNativeAttack = initialAttack
		};
		if (name == "Scourge")
		{
			hero.SetAnimationTrigger("Attack");
		}
		Active.Add(item);
		Starts++;
		LastStartMs = Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds;
	}

	private VisualSession(string card, NCreature hero, NCreature osty, NCombatRoom room, NCreature? target)
	{
		_card = card;
		_hero = hero;
		_osty = osty;
		_actor = (((card == "Unleash" || card == "Protector") ? true : false) ? osty : hero);
		_room = room;
		_tree = ((Node)room).GetTree();
		IVisualEffect visual;
		switch (card)
		{
		case "ReaperForm":
			visual = new ReaperFormEffect(hero.Body, osty.Body, (Node)(object)room.CombatVfxContainer);
			break;
		case "Lethality":
			visual = new LethalityEffect(hero.Body);
			break;
		case "Deathbringer":
			visual = new DeathbringerEffect(hero, room);
			break;
		case "Neurosurge":
			visual = new NeurosurgeEffect(hero, osty, room);
			break;
		case "Invoke":
			visual = new InvokeEffect(hero, osty, room);
			break;
		case "BansheesCry":
			visual = new BansheeEffect(hero, osty, room);
			break;
		case "CaptureSpirit":
			visual = new CaptureSpiritEffect(hero, osty, target, room);
			break;
		case "Protector":
		case "Unleash":
		{
			IVisualEffect visualEffect2;
			if (!SpineCompatibility.AllowsOstyBody(osty.Body))
			{
				IVisualEffect visualEffect = new OstySkinEffect(target, room, card == "Protector");
				visualEffect2 = visualEffect;
			}
			else
			{
				IVisualEffect visualEffect = new OstyEffect(hero, osty, target, room, card == "Protector");
				visualEffect2 = visualEffect;
			}
			visual = visualEffect2;
			break;
		}
		case "EndOfDays":
			visual = new EndOfDaysEffect(hero, osty, room);
			break;
		default:
			visual = new ClipPlayer(ClipAssets.Get(card), hero, osty, (Node)(object)room.CombatVfxContainer);
			break;
		}
		_visual = visual;
		_tree.ProcessFrame += Frame;
		((Node)_hero).TreeExiting += Dispose;
		((Node)_room).TreeExiting += Dispose;
	}

	internal static void InterruptBody(NCreature? actor, string trigger)
	{
		if (actor == null)
		{
			return;
		}
		foreach (VisualSession item in Active)
		{
			if (item._actor == actor)
			{
				if (item._awaitNativeAttack && trigger == "Attack")
				{
					item._awaitNativeAttack = false;
				}
				else
				{
					item._visual.CancelBody();
				}
			}
		}
	}

	internal static void ResolveInvoke(NCreature? actor)
	{
		if (actor == null)
		{
			return;
		}
		foreach (VisualSession item in Active)
		{
			if (item._hero == actor && item._visual is InvokeEffect invokeEffect)
			{
				invokeEffect.Resolve();
			}
		}
	}

	private void Frame()
	{
		if (_disposed)
		{
			return;
		}
		try
		{
			bool flag = NCombatRoom.Instance != _room || (_card != "Lethality" && (!_hero.Entity.IsAlive || IsDying(_hero)));
			if (!flag)
			{
				bool flag2;
				switch (_card)
				{
				case "Friendship":
				case "Invoke":
				case "Unleash":
				case "Protector":
					flag2 = true;
					break;
				default:
					flag2 = false;
					break;
				}
				flag = flag2 && (!_osty.Entity.IsAlive || IsDying(_osty));
			}
			if (flag)
			{
				Dispose();
				return;
			}
			if ((CombatManager.Instance.IsEnding || !CombatManager.Instance.IsInProgress) && (_visual.Hold || !float.IsFinite(_visual.Duration)))
			{
				Dispose();
				return;
			}
			if (_visual is LethalityEffect lethalityEffect && (!_hero.Entity.IsAlive || IsDying(_hero)))
			{
				lethalityEffect.Die();
				if (!CombatManager.Instance.IsInProgress || !((Node)_hero).CanProcess())
				{
					Dispose();
					return;
				}
			}
			if (_tree.Paused || !((Node)_hero).CanProcess())
			{
				return;
			}
			if (_card == "ReaperForm")
			{
				if (_hero.Entity.Powers.Any((PowerModel p) => ((object)p).GetType().Name == "ReaperFormPower" && p.Amount > 0))
				{
					_formPowerSeen = true;
				}
				else if (_formPowerSeen || _age > 5f)
				{
					Dispose();
					return;
				}
			}
			long timestamp = Stopwatch.GetTimestamp();
			_age += (float)((Node)_hero).GetProcessDeltaTime();
			if (!_visual.Hold && _age >= _visual.Duration)
			{
				Dispose();
				return;
			}
			_visual.Advance(_age);
			LastUpdateMs = Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds;
		}
		catch (Exception error)
		{
			ModEntry.Log(error);
			Dispose();
		}
	}

	internal static void Reset()
	{
		while (Active.Count > 0)
		{
			try
			{
				List<VisualSession> active = Active;
				active[active.Count - 1].Dispose();
			}
			catch (Exception error)
			{
				ModEntry.Log(error);
			}
		}
	}

	internal static void ReaperPowerGained(Creature owner)
	{
		if (CardVisualContext.Current.Value?.Card == "ReaperForm" || !AnimationSettings.Current.Allows("ReaperForm") || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding || !owner.IsAlive)
		{
			return;
		}
		NCreature creatureNode = owner.GetCreatureNode();
		NCombatRoom instance = NCombatRoom.Instance;
		if (creatureNode != null && instance != null)
		{
			Player player = owner.Player;
			if (!(((player != null) ? ((object)player.Character).GetType().Name : null) != "Necrobinder") && SpineCompatibility.Allows(creatureNode.Body, "necrobinder") && CountFor(creatureNode, "ReaperForm") <= 0)
			{
				Creature osty = owner.Player.Osty;
				VisualSession item = new VisualSession("ReaperForm", creatureNode, ((osty != null) ? osty.GetCreatureNode() : null) ?? creatureNode, instance, null)
				{
					_formPowerSeen = true
				};
				Active.Add(item);
				Starts++;
			}
		}
	}

	internal static void RestoreResidents(NCombatRoom room)
	{
		foreach (NCreature hero in room.CreatureNodes)
		{
			Player player = hero.Entity.Player;
			if (((player != null) ? ((object)player.Character).GetType().Name : null) != "Necrobinder" || !hero.Entity.IsAlive || IsDying(hero))
			{
				continue;
			}
			Creature osty = player.Osty;
			NCreature val = ((osty != null) ? osty.GetCreatureNode() : null);
			NCreature val2 = val ?? hero;
			string[] array = new string[4] { "Invoke", "Friendship", "Lethality", "ReaperForm" };
			foreach (string name in array)
			{
				string power = ((name == "Invoke") ? "SummonNextTurnPower" : (name + "Power"));
				bool flag = val == null;
				if (flag)
				{
					string text = name;
					bool flag2 = ((text == "Invoke" || text == "Friendship") ? true : false);
					flag = flag2;
				}
				if (!flag)
				{
					string text = name;
					flag = ((text == "Lethality" || text == "ReaperForm") ? true : false);
					if ((!flag || SpineCompatibility.Allows(hero.Body, "necrobinder")) && (!(name == "Friendship") || SpineCompatibility.Allows(val2.Body, "osty")) && AnimationSettings.Current.Allows(name) && !Active.Any((VisualSession s) => s._hero == hero && s._card == name) && hero.Entity.Powers.Any((PowerModel p) => ((object)p).GetType().Name == power && p.Amount > 0))
					{
						VisualSession visualSession = new VisualSession(name, hero, val2, room, null)
						{
							_age = ((name == "ReaperForm") ? 3f : 1.4f),
							_formPowerSeen = (name == "ReaperForm")
						};
						Active.Add(visualSession);
						visualSession._visual.Advance(visualSession._age);
					}
				}
			}
		}
	}

	internal static void RestoreFriendship(Player player, NCombatRoom room)
	{
		if (NCombatRoom.Instance == room && GodotObject.IsInstanceValid((GodotObject)(object)room) && ((Node)room).IsInsideTree() && CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding && AnimationSettings.Current.Allows("Friendship"))
		{
			NCreature creatureNode = player.Creature.GetCreatureNode();
			Creature osty = player.Osty;
			NCreature val = ((osty != null) ? osty.GetCreatureNode() : null);
			if (creatureNode != null && val != null && creatureNode.Entity.IsAlive && !IsDying(creatureNode) && val.Entity.IsAlive && !IsDying(val) && CountFor(creatureNode, "Friendship") <= 0 && creatureNode.Entity.Powers.Any((PowerModel p) => p is FriendshipPower && p.Amount > 0) && SpineCompatibility.Allows(val.Body, "osty"))
			{
				VisualSession visualSession = new VisualSession("Friendship", creatureNode, val, room, null);
				Active.Add(visualSession);
				visualSession._visual.Advance(0f);
			}
		}
	}

	internal static void ResetDisabled()
	{
		for (int num = Active.Count - 1; num >= 0; num--)
		{
			if (!AnimationSettings.Current.Allows(Active[num]._card))
			{
				Active[num].Dispose();
			}
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		Active.Remove(this);
		try
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)_tree))
			{
				_tree.ProcessFrame -= Frame;
			}
		}
		finally
		{
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)_hero))
				{
					((Node)_hero).TreeExiting -= Dispose;
				}
			}
			finally
			{
				try
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)_room))
					{
						((Node)_room).TreeExiting -= Dispose;
					}
				}
				finally
				{
					_visual.Dispose();
				}
			}
		}
	}
}
internal sealed class VisualShake : IDisposable
{
	private static readonly FieldInfo? Multiplier = AccessTools.Field(typeof(NScreenShake), "_multiplier");

	private static readonly FieldInfo? Instance = AccessTools.Field(typeof(NScreenShake), "_shakeInstance");

	private NScreenShake? _node;

	private ScreenPunchInstance? _owned;

	private static readonly List<VisualShake> Offsets = new List<VisualShake>();

	private Vector2 _offset;

	internal void SetOffset(Vector2 value)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_node == null)
		{
			NGame instance = NGame.Instance;
			_node = ((instance != null) ? ((Node)instance).GetNodeOrNull<NScreenShake>(NodePath.op_Implicit("ScreenShake")) : null);
		}
		_offset = value;
		if (!Offsets.Contains(this))
		{
			Offsets.Add(this);
		}
	}

	internal static void ApplyOffsets(NScreenShake node)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (Offsets.Count == 0)
		{
			return;
		}
		Control shakeTarget = node.ShakeTarget;
		if (shakeTarget == null || !GodotObject.IsInstanceValid((GodotObject)(object)shakeTarget) || ((GodotObject)shakeTarget).IsQueuedForDeletion())
		{
			return;
		}
		float num = (((object)Multiplier == null) ? 1f : ((float)Multiplier.GetValue(node)));
		Vector2 val = Vector2.Zero;
		foreach (VisualShake offset in Offsets)
		{
			if (offset._node == node)
			{
				val += offset._offset;
			}
		}
		shakeTarget.Position += new Vector2(Math.Clamp(val.X, -20f, 20f), Math.Clamp(val.Y, -18f, 18f)) * num;
	}

	internal void Play(float strength, double duration, float angle)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		NGame instance = NGame.Instance;
		_node = ((instance != null) ? ((Node)instance).GetNodeOrNull<NScreenShake>(NodePath.op_Implicit("ScreenShake")) : null);
		NScreenShake? node = _node;
		if (((node != null) ? node.ShakeTarget : null) != null && (object)Multiplier != null && (object)Instance != null)
		{
			_owned = new ScreenPunchInstance(strength * (float)Multiplier.GetValue(_node), duration, angle);
			Instance.SetValue(_node, _owned);
		}
	}

	public void Dispose()
	{
		Offsets.Remove(this);
		if (_node != null && GodotObject.IsInstanceValid((GodotObject)(object)_node) && Instance?.GetValue(_node) == _owned)
		{
			ScreenPunchInstance? owned = _owned;
			if (owned != null)
			{
				owned.Cancel();
			}
		}
		_node = null;
		_owned = null;
	}
}
[HarmonyPatch]
internal static class BansheeDamageContext
{
	private sealed record State(bool Shake, bool Spark);

	internal static readonly AsyncLocal<bool> Suppress = new AsyncLocal<bool>();

	internal static readonly AsyncLocal<bool> HideSpark = new AsyncLocal<bool>();

	private static MethodBase TargetMethod()
	{
		return typeof(CreatureCmd).GetMethods().Single((MethodInfo m) => m.Name == "Damage" && m.GetParameters().Length >= 6 && m.GetParameters()[1].Name == "targets" && m.GetParameters()[2].ParameterType == typeof(decimal) && m.GetParameters().Any((ParameterInfo p) => p.Name == "cardSource"));
	}

	[HarmonyPrefix]
	private static void Prefix(CardModel? cardSource, out State __state)
	{
		__state = new State(Suppress.Value, HideSpark.Value);
		Suppress.Value = cardSource is BansheesCry && VisualSession.HasBanshee(cardSource);
		bool flag = ((cardSource is Unleash || cardSource is Protector) ? true : false);
		HideSpark.Value = flag && VisualSession.HasOsty(cardSource);
	}

	[HarmonyPostfix]
	private static void Postfix(State? __state)
	{
		if ((object)__state != null)
		{
			Suppress.Value = __state.Shake;
			HideSpark.Value = __state.Spark;
		}
	}

	[HarmonyFinalizer]
	private static void Finalizer(State? __state)
	{
		if ((object)__state != null)
		{
			Suppress.Value = __state.Shake;
			HideSpark.Value = __state.Spark;
		}
	}
}
[HarmonyPatch(typeof(NHitSparkVfx), "Create")]
internal static class NativeOstySparkPatch
{
	[HarmonyPostfix]
	private static void Postfix(NHitSparkVfx? __result)
	{
		if (BansheeDamageContext.HideSpark.Value && __result != null)
		{
			((CanvasItem)__result).Hide();
		}
	}
}
[HarmonyPatch(typeof(NScreenShake), "Shake")]
internal static class NativeBansheeShakePatch
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		if (!BansheeDamageContext.Suppress.Value)
		{
			CardVisualContext value = CardVisualContext.Current.Value;
			return value == null || !(value.Card == "EndOfDays") || !value.Started;
		}
		return false;
	}
}
[HarmonyPatch(typeof(NScreenShake), "_Process")]
internal static class ExtraVisualOffsetPatch
{
	[HarmonyPostfix]
	private static void Postfix(NScreenShake __instance)
	{
		VisualShake.ApplyOffsets(__instance);
	}
}
[CompilerGenerated]
internal sealed class <>z__ReadOnlyArray<T> : IEnumerable, ICollection, IList, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList<T>
{
	int ICollection.Count => _items.Length;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	object? IList.this[int index]
	{
		get
		{
			return _items[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	int IReadOnlyCollection<T>.Count => _items.Length;

	T IReadOnlyList<T>.this[int index] => _items[index];

	int ICollection<T>.Count => _items.Length;

	bool ICollection<T>.IsReadOnly => true;

	T IList<T>.this[int index]
	{
		get
		{
			return _items[index];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public <>z__ReadOnlyArray(T[] items)
	{
		_items = items;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_items).GetEnumerator();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)_items).CopyTo(array, index);
	}

	int IList.Add(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object? value)
	{
		return ((IList)_items).Contains(value);
	}

	int IList.IndexOf(object? value)
	{
		return ((IList)_items).IndexOf(value);
	}

	void IList.Insert(int index, object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)_items).GetEnumerator();
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Contains(T item)
	{
		return ((ICollection<T>)_items).Contains(item);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		((ICollection<T>)_items).CopyTo(array, arrayIndex);
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	int IList<T>.IndexOf(T item)
	{
		return ((IList<T>)_items).IndexOf(item);
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}
}
