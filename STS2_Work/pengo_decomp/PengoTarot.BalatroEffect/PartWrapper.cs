using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;

namespace PengoTarot.BalatroEffect;

public static class PartWrapper
{
	public class ShaderPartContainer : SubViewportContainer
	{
		public class MethodName : MethodName
		{
			public static readonly StringName _Process = StringName.op_Implicit("_Process");
		}

		public class PropertyName : PropertyName
		{
			public static readonly StringName PartName = StringName.op_Implicit("PartName");

			public static readonly StringName CardId = StringName.op_Implicit("CardId");

			public static readonly StringName OriginalPosition = StringName.op_Implicit("OriginalPosition");

			public static readonly StringName _tilt = StringName.op_Implicit("_tilt");
		}

		public class SignalName : SignalName
		{
		}

		public string? PartName;

		public string? CardId;

		public Vector2 OriginalPosition;

		private TiltWrapper.TiltContainer? _tilt;

		public override void _Process(double _)
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Expected O, but got Unknown
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_020c: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			Material material = ((CanvasItem)this).Material;
			ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val == null || string.IsNullOrEmpty(CardId) || string.IsNullOrEmpty(PartName))
			{
				return;
			}
			if (_tilt == null || !GodotObject.IsInstanceValid((GodotObject)(object)_tilt))
			{
				_tilt = null;
				for (Node parent = ((Node)this).GetParent(); parent != null; parent = parent.GetParent())
				{
					if (parent is TiltWrapper.TiltContainer tilt)
					{
						_tilt = tilt;
						break;
					}
				}
			}
			if (_tilt == null)
			{
				val.SetShaderParameter(TiltWrapper.XRotKey, Variant.op_Implicit(0f));
				val.SetShaderParameter(TiltWrapper.YRotKey, Variant.op_Implicit(0f));
			}
			int effect = Config.GetEffect(CardId, PartName);
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
				val2.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(effect));
				val2.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(Config.GetIntensity(CardId)));
				val2.SetShaderParameter(TiltWrapper.UvOffsetKey, val.GetShaderParameter(TiltWrapper.UvOffsetKey));
				val2.SetShaderParameter(TiltWrapper.UvScaleKey, val.GetShaderParameter(TiltWrapper.UvScaleKey));
				val2.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, val.GetShaderParameter(TiltWrapper.FoilAltUvOffsetKey));
				((CanvasItem)this).Material = (Material)(object)val2;
				val = val2;
			}
			else
			{
				val.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(effect));
				val.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(Config.GetIntensity(CardId)));
			}
			(float, float)? playRotation = CardPlayTracker.GetPlayRotation(CardId);
			if (playRotation.HasValue)
			{
				val.SetShaderParameter(TiltWrapper.XRotKey, Variant.op_Implicit(playRotation.Value.Item1));
				val.SetShaderParameter(TiltWrapper.YRotKey, Variant.op_Implicit(playRotation.Value.Item2));
				return;
			}
			TiltWrapper.TiltContainer? tilt2 = _tilt;
			Material obj = ((tilt2 != null) ? ((CanvasItem)tilt2).Material : null);
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
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			if ((ref name) == PropertyName.PartName)
			{
				PartName = VariantUtils.ConvertTo<string>(ref value);
				return true;
			}
			if ((ref name) == PropertyName.CardId)
			{
				CardId = VariantUtils.ConvertTo<string>(ref value);
				return true;
			}
			if ((ref name) == PropertyName.OriginalPosition)
			{
				OriginalPosition = VariantUtils.ConvertTo<Vector2>(ref value);
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
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			if ((ref name) == PropertyName.PartName)
			{
				value = VariantUtils.CreateFrom<string>(ref PartName);
				return true;
			}
			if ((ref name) == PropertyName.CardId)
			{
				value = VariantUtils.CreateFrom<string>(ref CardId);
				return true;
			}
			if ((ref name) == PropertyName.OriginalPosition)
			{
				value = VariantUtils.CreateFrom<Vector2>(ref OriginalPosition);
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
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			return new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, PropertyName.PartName, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)4, PropertyName.CardId, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)5, PropertyName.OriginalPosition, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)24, PropertyName._tilt, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)this).SaveGodotObjectData(info);
			info.AddProperty(PropertyName.PartName, Variant.From<string>(ref PartName));
			info.AddProperty(PropertyName.CardId, Variant.From<string>(ref CardId));
			info.AddProperty(PropertyName.OriginalPosition, Variant.From<Vector2>(ref OriginalPosition));
			info.AddProperty(PropertyName._tilt, Variant.From<TiltWrapper.TiltContainer>(ref _tilt));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)this).RestoreGodotObjectData(info);
			Variant val = default(Variant);
			if (info.TryGetProperty(PropertyName.PartName, ref val))
			{
				PartName = ((Variant)(ref val)).As<string>();
			}
			Variant val2 = default(Variant);
			if (info.TryGetProperty(PropertyName.CardId, ref val2))
			{
				CardId = ((Variant)(ref val2)).As<string>();
			}
			Variant val3 = default(Variant);
			if (info.TryGetProperty(PropertyName.OriginalPosition, ref val3))
			{
				OriginalPosition = ((Variant)(ref val3)).As<Vector2>();
			}
			Variant val4 = default(Variant);
			if (info.TryGetProperty(PropertyName._tilt, ref val4))
			{
				_tilt = ((Variant)(ref val4)).As<TiltWrapper.TiltContainer>();
			}
		}
	}

	public const string SpPrefix = "BalatroShaderPart_";

	public static readonly HashSet<string> TextPartNames = new HashSet<string> { "TitleLabel", "DescriptionLabel" };

	public static void WrapParts(string cid, Control root)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		List<Node> list = new List<Node>();
		foreach (Node child in ((Node)root).GetChildren(false))
		{
			list.Add(child);
		}
		List<Node> list2 = new List<Node>();
		foreach (Node item in list)
		{
			Control val = (Control)(object)((item is Control) ? item : null);
			if (val == null)
			{
				list2.Add(item);
				continue;
			}
			string text = StringName.op_Implicit(((Node)val).Name);
			if (TiltWrapper.SkipNames.Contains(text) || text == "PortraitCanvasGroup" || TextPartNames.Contains(text))
			{
				list2.Add((Node)(object)val);
			}
			else if (Config.AllPartNames.Contains(text) && Config.GetEffect(cid, text) > 0)
			{
				Vector2 gp = val.Position + root.Position;
				ShaderPartContainer shaderPartContainer = CreateShaderPart(cid, text, val, gp, val.Size);
				((Node)root).RemoveChild((Node)(object)val);
				val.Position = Vector2.Zero;
				((Node)((Node)shaderPartContainer).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"))).GetChild(0, false).AddChild((Node)(object)val, false, (InternalMode)0);
				list2.Add((Node)(object)shaderPartContainer);
			}
			else
			{
				list2.Add((Node)(object)val);
			}
		}
		foreach (Node child2 in ((Node)root).GetChildren(false))
		{
			((Node)root).RemoveChild(child2);
		}
		foreach (Node item2 in list2)
		{
			((Node)root).AddChild(item2, false, (InternalMode)0);
		}
	}

	public static void UnwrapParts(Control root)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		List<(ShaderPartContainer, int)> list = new List<(ShaderPartContainer, int)>();
		List<Node> list2 = new List<Node>();
		foreach (Node child2 in ((Node)root).GetChildren(false))
		{
			list2.Add(child2);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			if (((object)list2[i].Name).ToString().StartsWith("BalatroShaderPart_") && list2[i] is ShaderPartContainer item)
			{
				list.Add((item, i));
			}
		}
		list.Reverse();
		foreach (var item4 in list)
		{
			ShaderPartContainer item2 = item4.Item1;
			int item3 = item4.Item2;
			SubViewport node = ((Node)item2).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"));
			Node child = ((Node)node).GetChild(0, false).GetChild(0, false);
			((Node)node).GetChild(0, false).RemoveChild(child);
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null)
			{
				val.Position = item2.OriginalPosition;
			}
			((Node)root).RemoveChild((Node)(object)item2);
			((Node)root).AddChild(child, false, (InternalMode)0);
			((Node)root).MoveChild(child, item3);
			((Node)item2).QueueFree();
		}
	}

	public static ShaderPartContainer CreateShaderPart(string cid, string pn, Control pc, Vector2 gp, Vector2 sz)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Expected O, but got Unknown
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		int effect = Config.GetEffect(cid, pn);
		Shader shader = EffectRegistry.GetShader(effect) ?? EffectRegistry.GetShader(1);
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = shader
		};
		val.SetShaderParameter(TiltWrapper.SeedKey, Variant.op_Implicit((float)(((object)pc).GetHashCode() % 10000) / 10f));
		val.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(effect));
		val.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(Config.GetIntensity(cid)));
		val.SetShaderParameter(TiltWrapper.UvOffsetKey, Variant.op_Implicit(gp / TiltWrapper.UvRefSize));
		val.SetShaderParameter(TiltWrapper.UvScaleKey, Variant.op_Implicit(sz / TiltWrapper.UvRefSize));
		val.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, Variant.op_Implicit((gp - TiltWrapper.UvRefHalf) / TiltWrapper.UvRefSize));
		ShaderPartContainer shaderPartContainer = new ShaderPartContainer();
		((Node)shaderPartContainer).Name = StringName.op_Implicit("BalatroShaderPart_" + pn);
		((CanvasItem)shaderPartContainer).Material = (Material)(object)val;
		((Control)shaderPartContainer).Size = sz;
		((Control)shaderPartContainer).Position = pc.Position;
		((Control)shaderPartContainer).MouseFilter = (MouseFilterEnum)2;
		((SubViewportContainer)shaderPartContainer).Stretch = true;
		shaderPartContainer.PartName = pn;
		shaderPartContainer.CardId = cid;
		shaderPartContainer.OriginalPosition = pc.Position;
		SubViewport val2 = new SubViewport
		{
			Name = StringName.op_Implicit("SubViewport"),
			TransparentBg = true,
			Disable3D = true,
			Size = (Vector2I)sz
		};
		((Node)val2).AddChild((Node)new Control(), false, (InternalMode)0);
		((Node)shaderPartContainer).AddChild((Node)(object)val2, false, (InternalMode)0);
		return shaderPartContainer;
	}
}
