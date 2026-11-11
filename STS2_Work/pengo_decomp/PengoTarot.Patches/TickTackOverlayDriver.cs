using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

internal class TickTackOverlayDriver : Node
{
	public class MethodName : MethodName
	{
		public static readonly StringName _EnterTree = StringName.op_Implicit("_EnterTree");

		public static readonly StringName _Process = StringName.op_Implicit("_Process");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName _creature = StringName.op_Implicit("_creature");

		public static readonly StringName _overlay = StringName.op_Implicit("_overlay");

		public static readonly StringName _flashTimer = StringName.op_Implicit("_flashTimer");

		public static readonly StringName _overlayVisible = StringName.op_Implicit("_overlayVisible");
	}

	public class SignalName : SignalName
	{
	}

	private readonly NCreature _creature;

	private ColorRect? _overlay;

	private float _flashTimer;

	private bool _overlayVisible;

	public TickTackOverlayDriver(NCreature creature)
	{
		_creature = creature;
	}

	public override void _EnterTree()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		_overlay = new ColorRect
		{
			Name = StringName.op_Implicit("TickTackOverlay"),
			MouseFilter = (MouseFilterEnum)2,
			Color = new Color(1f, 0f, 0f, 0f)
		};
		GodotTreeExtensions.AddChildSafely((Node)(object)_creature, (Node)(object)_overlay);
		((Control)_overlay).SetAnchorsPreset((LayoutPreset)15, false);
		((CanvasItem)_overlay).ZIndex = 100;
	}

	public override void _Process(double delta)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (_overlay == null)
		{
			return;
		}
		Creature entity = _creature.Entity;
		if (entity == null || !entity.IsPlayer)
		{
			_overlay.Color = new Color(1f, 0f, 0f, 0f);
			_flashTimer = 0f;
			return;
		}
		TickTackPower power = entity.GetPower<TickTackPower>();
		if (power == null || ((PowerModel)power).Amount <= 0)
		{
			_overlay.Color = new Color(1f, 0f, 0f, 0f);
			_flashTimer = 0f;
			return;
		}
		float flashTimer = Mathf.Clamp((float)((PowerModel)power).Amount * 0.08f, 0.1f, 0.5f);
		_flashTimer -= (float)delta;
		if (_flashTimer <= 0f)
		{
			_flashTimer = flashTimer;
			_overlayVisible = !_overlayVisible;
			if (_overlayVisible)
			{
				_overlay.Color = new Color(1f, 0f, 0f, 0.25f);
			}
			else
			{
				_overlay.Color = new Color(1f, 0f, 0f, 0f);
			}
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(2)
		{
			new MethodInfo(MethodName._EnterTree, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName._Process, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)3, StringName.op_Implicit("delta"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName._EnterTree && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._EnterTree();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._Process && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			((Node)this)._Process(VariantUtils.ConvertTo<double>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		return ((Node)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName._EnterTree)
		{
			return true;
		}
		if ((ref method) == MethodName._Process)
		{
			return true;
		}
		return ((Node)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if ((ref name) == PropertyName._overlay)
		{
			_overlay = VariantUtils.ConvertTo<ColorRect>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._flashTimer)
		{
			_flashTimer = VariantUtils.ConvertTo<float>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._overlayVisible)
		{
			_overlayVisible = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		return ((GodotObject)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName._creature)
		{
			value = VariantUtils.CreateFrom<NCreature>(ref _creature);
			return true;
		}
		if ((ref name) == PropertyName._overlay)
		{
			value = VariantUtils.CreateFrom<ColorRect>(ref _overlay);
			return true;
		}
		if ((ref name) == PropertyName._flashTimer)
		{
			value = VariantUtils.CreateFrom<float>(ref _flashTimer);
			return true;
		}
		if ((ref name) == PropertyName._overlayVisible)
		{
			value = VariantUtils.CreateFrom<bool>(ref _overlayVisible);
			return true;
		}
		return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._creature, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._overlay, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)3, PropertyName._flashTimer, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._overlayVisible, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._overlay, Variant.From<ColorRect>(ref _overlay));
		info.AddProperty(PropertyName._flashTimer, Variant.From<float>(ref _flashTimer));
		info.AddProperty(PropertyName._overlayVisible, Variant.From<bool>(ref _overlayVisible));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((GodotObject)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._overlay, ref val))
		{
			_overlay = ((Variant)(ref val)).As<ColorRect>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._flashTimer, ref val2))
		{
			_flashTimer = ((Variant)(ref val2)).As<float>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._overlayVisible, ref val3))
		{
			_overlayVisible = ((Variant)(ref val3)).As<bool>();
		}
	}
}
