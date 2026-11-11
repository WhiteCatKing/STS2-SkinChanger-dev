using System.Collections.Generic;
using System.ComponentModel;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace PengoTarot.BalatroEffect;

public static class FullCardWrapper
{
	public class FullCardEffectContainer : SubViewportContainer
	{
		public class MethodName : MethodName
		{
			public static readonly StringName _Process = StringName.op_Implicit("_Process");
		}

		public class PropertyName : PropertyName
		{
			public static readonly StringName CardId = StringName.op_Implicit("CardId");

			public static readonly StringName _tilt = StringName.op_Implicit("_tilt");
		}

		public class SignalName : SignalName
		{
		}

		public string? CardId;

		private TiltWrapper.TiltContainer? _tilt;

		public override void _Process(double _)
		{
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Expected O, but got Unknown
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			Material material = ((CanvasItem)this).Material;
			ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val == null || string.IsNullOrEmpty(CardId))
			{
				return;
			}
			if (_tilt == null || !GodotObject.IsInstanceValid((GodotObject)(object)_tilt))
			{
				SubViewport nodeOrNull = ((Node)this).GetNodeOrNull<SubViewport>(NodePath.op_Implicit("SubViewport"));
				if (nodeOrNull != null && ((Node)nodeOrNull).GetChildCount(false) > 0)
				{
					_tilt = ((Node)nodeOrNull).GetChild(0, false) as TiltWrapper.TiltContainer;
				}
			}
			int effect = Config.GetEffect(CardId, "FullCard");
			if (effect == 0)
			{
				val.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(0));
				return;
			}
			Shader shader = EffectRegistry.GetShader(effect);
			if (shader != null && val.Shader != shader)
			{
				ShaderMaterial val2 = new ShaderMaterial
				{
					Shader = shader
				};
				val2.SetShaderParameter(TiltWrapper.SeedKey, val.GetShaderParameter(TiltWrapper.SeedKey));
				val2.SetShaderParameter(TiltWrapper.UvOffsetKey, val.GetShaderParameter(TiltWrapper.UvOffsetKey));
				val2.SetShaderParameter(TiltWrapper.UvScaleKey, val.GetShaderParameter(TiltWrapper.UvScaleKey));
				((CanvasItem)this).Material = (Material)(object)val2;
				val = val2;
			}
			val.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(effect));
			val.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(Config.GetIntensity(CardId)));
			(float, float)? playRotation = CardPlayTracker.GetPlayRotation(CardId);
			if (playRotation.HasValue)
			{
				val.SetShaderParameter(TiltWrapper.XRotKey, Variant.op_Implicit(playRotation.Value.Item1));
				val.SetShaderParameter(TiltWrapper.YRotKey, Variant.op_Implicit(playRotation.Value.Item2));
				return;
			}
			TiltWrapper.TiltContainer? tilt = _tilt;
			Material obj = ((tilt != null) ? ((CanvasItem)tilt).Material : null);
			ShaderMaterial val3 = (ShaderMaterial)(object)((obj is ShaderMaterial) ? obj : null);
			if (val3 != null && GodotObject.IsInstanceValid((GodotObject)(object)_tilt))
			{
				val.SetShaderParameter(TiltWrapper.XRotKey, val3.GetShaderParameter(TiltWrapper.XRotKey));
				val.SetShaderParameter(TiltWrapper.YRotKey, val3.GetShaderParameter(TiltWrapper.YRotKey));
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			return new List<MethodInfo>(1)
			{
				new MethodInfo(MethodName._Process, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
				{
					new PropertyInfo((Type)3, StringName.op_Implicit("_"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
				}, (List<Variant>)null)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName._Process && ((NativeVariantPtrArgs)(ref args)).Count == 1)
			{
				((Node)this)._Process(VariantUtils.ConvertTo<double>(ref ((NativeVariantPtrArgs)(ref args))[0]));
				ret = default(godot_variant);
				return true;
			}
			return ((SubViewportContainer)this).InvokeGodotClassMethod(ref method, args, ref ret);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool HasGodotClassMethod(in godot_string_name method)
		{
			if ((ref method) == MethodName._Process)
			{
				return true;
			}
			return ((SubViewportContainer)this).HasGodotClassMethod(ref method);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
		{
			if ((ref name) == PropertyName.CardId)
			{
				CardId = VariantUtils.ConvertTo<string>(ref value);
				return true;
			}
			if ((ref name) == PropertyName._tilt)
			{
				_tilt = VariantUtils.ConvertTo<TiltWrapper.TiltContainer>(ref value);
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
			if ((ref name) == PropertyName.CardId)
			{
				value = VariantUtils.CreateFrom<string>(ref CardId);
				return true;
			}
			if ((ref name) == PropertyName._tilt)
			{
				value = VariantUtils.CreateFrom<TiltWrapper.TiltContainer>(ref _tilt);
				return true;
			}
			return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<PropertyInfo> GetGodotPropertyList()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			return new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, PropertyName.CardId, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)24, PropertyName._tilt, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)this).SaveGodotObjectData(info);
			info.AddProperty(PropertyName.CardId, Variant.From<string>(ref CardId));
			info.AddProperty(PropertyName._tilt, Variant.From<TiltWrapper.TiltContainer>(ref _tilt));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).RestoreGodotObjectData(info);
			Variant val = default(Variant);
			if (info.TryGetProperty(PropertyName.CardId, ref val))
			{
				CardId = ((Variant)(ref val)).As<string>();
			}
			Variant val2 = default(Variant);
			if (info.TryGetProperty(PropertyName._tilt, ref val2))
			{
				_tilt = ((Variant)(ref val2)).As<TiltWrapper.TiltContainer>();
			}
		}
	}

	public const string FxName = "BalatroFullCardFx";

	public static FullCardEffectContainer CreateFullCard(NCard card, string cid, TiltWrapper.TiltContainer tilt, int mode)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		Shader shader = EffectRegistry.GetShader(mode) ?? EffectRegistry.GetShader(1);
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = shader
		};
		val.SetShaderParameter(TiltWrapper.SeedKey, Variant.op_Implicit((float)(((object)card).GetHashCode() % 10000) / 10f));
		FullCardEffectContainer fullCardEffectContainer = new FullCardEffectContainer();
		((Node)fullCardEffectContainer).Name = StringName.op_Implicit("BalatroFullCardFx");
		((CanvasItem)fullCardEffectContainer).Material = (Material)(object)val;
		((Control)fullCardEffectContainer).Size = Vector2I.op_Implicit(new Vector2I(1200, 1200));
		((SubViewportContainer)fullCardEffectContainer).Stretch = true;
		((Control)fullCardEffectContainer).MouseFilter = (MouseFilterEnum)2;
		((Control)fullCardEffectContainer).Position = new Vector2(-600f, -600f);
		((Control)fullCardEffectContainer).PivotOffset = new Vector2(600f, 600f);
		fullCardEffectContainer.CardId = cid;
		SubViewport val2 = new SubViewport
		{
			Name = StringName.op_Implicit("SubViewport"),
			TransparentBg = true,
			Disable3D = true,
			Size = new Vector2I(1200, 1200)
		};
		Node parent = ((Node)tilt).GetParent();
		if (parent != null)
		{
			parent.RemoveChild((Node)(object)tilt);
		}
		((Control)tilt).Position = Vector2.Zero;
		((Node)val2).AddChild((Node)(object)tilt, false, (InternalMode)0);
		((Node)fullCardEffectContainer).AddChild((Node)(object)val2, false, (InternalMode)0);
		Control root = (Control)((Node)val2).GetChild(0, false);
		PartWrapper.WrapParts(cid, root);
		val.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(mode));
		val.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(Config.GetIntensity(cid)));
		val.SetShaderParameter(TiltWrapper.UvOffsetKey, Variant.op_Implicit(new Vector2(-0.75f, -0.75f)));
		val.SetShaderParameter(TiltWrapper.UvScaleKey, Variant.op_Implicit(new Vector2(2.5f, 2.5f)));
		val.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, Variant.op_Implicit(new Vector2(-1.25f, -1.25f)));
		return fullCardEffectContainer;
	}
}
