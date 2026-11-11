using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace PengoTarot.BalatroEffect;

public static class TiltWrapper
{
	public class TiltContainer : SubViewportContainer
	{
		public class MethodName : MethodName
		{
			public static readonly StringName _Process = StringName.op_Implicit("_Process");

			public static readonly StringName UpdHolder = StringName.op_Implicit("UpdHolder");
		}

		public class PropertyName : PropertyName
		{
			public static readonly StringName ProcessedCardId = StringName.op_Implicit("ProcessedCardId");

			public static readonly StringName _nCard = StringName.op_Implicit("_nCard");

			public static readonly StringName _holder = StringName.op_Implicit("_holder");

			public static readonly StringName OriginalBodyNames = StringName.op_Implicit("OriginalBodyNames");
		}

		public class SignalName : SignalName
		{
		}

		public string ProcessedCardId = "";

		internal NCard? _nCard;

		private const float MT = 16f;

		private const float LS = 0.2f;

		private NCardHolder? _holder;

		internal string[]? OriginalBodyNames;

		public override void _Process(double _)
		{
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_016d: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			if (_nCard == null || !Config.GlobalDynamicEffect)
			{
				return;
			}
			UpdHolder();
			Material material = ((CanvasItem)this).Material;
			ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val == null || _nCard == null || _holder == null)
			{
				return;
			}
			float num = 0f;
			float num2 = 0f;
			NCardHolder holder = _holder;
			if (!(holder is NHandCardHolder) || ((CanvasItem)holder).ZIndex <= 0)
			{
				NClickableControl hitbox = _holder.Hitbox;
				if (hitbox == null || !hitbox.IsEnabled || !Traverse.Create((object)hitbox).Field<bool>("_isHovered").Value)
				{
					goto IL_0103;
				}
			}
			Vector2 val2 = ((CanvasItem)_nCard).GetGlobalMousePosition() - ((Control)_nCard).GlobalPosition;
			Transform2D globalTransform = ((CanvasItem)_nCard).GetGlobalTransform();
			Vector2 scale = ((Transform2D)(ref globalTransform)).Scale;
			Vector2 val3 = ((Vector2)(ref scale)).Max(0.01f) * 256f;
			num = val2.Y / val3.X * -16f;
			num2 = val2.X / val3.Y * 16f;
			goto IL_0103;
			IL_0103:
			num = Mathf.Clamp(num, -16f, 16f);
			num2 = Mathf.Clamp(num2, -16f, 16f);
			float num3 = (float)val.GetShaderParameter(XRotKey);
			float num4 = (float)val.GetShaderParameter(YRotKey);
			float num5 = Mathf.Lerp(num3, num, 0.2f);
			float num6 = Mathf.Lerp(num4, num2, 0.2f);
			val.SetShaderParameter(XRotKey, Variant.op_Implicit(num5));
			val.SetShaderParameter(YRotKey, Variant.op_Implicit(num6));
		}

		private void UpdHolder()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			NCardHolder val = null;
			for (Node parent = ((Node)this).GetParent(); parent != null; parent = parent.GetParent())
			{
				NCardHolder val2 = (NCardHolder)(object)((parent is NCardHolder) ? parent : null);
				if (val2 != null)
				{
					val = val2;
					break;
				}
			}
			if (_holder != val)
			{
				_holder = val;
				Material material = ((CanvasItem)this).Material;
				ShaderMaterial val3 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
				if (val3 != null)
				{
					val3.SetShaderParameter(XRotKey, Variant.op_Implicit(0f));
					val3.SetShaderParameter(YRotKey, Variant.op_Implicit(0f));
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			return new List<MethodInfo>(2)
			{
				new MethodInfo(MethodName._Process, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
				{
					new PropertyInfo((Type)3, StringName.op_Implicit("_"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
				}, (List<Variant>)null),
				new MethodInfo(MethodName.UpdHolder, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName._Process && ((NativeVariantPtrArgs)(ref args)).Count == 1)
			{
				((Node)this)._Process(VariantUtils.ConvertTo<double>(ref ((NativeVariantPtrArgs)(ref args))[0]));
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName.UpdHolder && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				UpdHolder();
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
			if ((ref method) == MethodName.UpdHolder)
			{
				return true;
			}
			return ((SubViewportContainer)this).HasGodotClassMethod(ref method);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
		{
			if ((ref name) == PropertyName.ProcessedCardId)
			{
				ProcessedCardId = VariantUtils.ConvertTo<string>(ref value);
				return true;
			}
			if ((ref name) == PropertyName._nCard)
			{
				_nCard = VariantUtils.ConvertTo<NCard>(ref value);
				return true;
			}
			if ((ref name) == PropertyName._holder)
			{
				_holder = VariantUtils.ConvertTo<NCardHolder>(ref value);
				return true;
			}
			if ((ref name) == PropertyName.OriginalBodyNames)
			{
				OriginalBodyNames = VariantUtils.ConvertTo<string[]>(ref value);
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
			if ((ref name) == PropertyName.ProcessedCardId)
			{
				value = VariantUtils.CreateFrom<string>(ref ProcessedCardId);
				return true;
			}
			if ((ref name) == PropertyName._nCard)
			{
				value = VariantUtils.CreateFrom<NCard>(ref _nCard);
				return true;
			}
			if ((ref name) == PropertyName._holder)
			{
				value = VariantUtils.CreateFrom<NCardHolder>(ref _holder);
				return true;
			}
			if ((ref name) == PropertyName.OriginalBodyNames)
			{
				value = VariantUtils.CreateFrom<string[]>(ref OriginalBodyNames);
				return true;
			}
			return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<PropertyInfo> GetGodotPropertyList()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			return new List<PropertyInfo>
			{
				new PropertyInfo((Type)4, PropertyName.ProcessedCardId, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)24, PropertyName._nCard, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)24, PropertyName._holder, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
				new PropertyInfo((Type)34, PropertyName.OriginalBodyNames, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
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
			info.AddProperty(PropertyName.ProcessedCardId, Variant.From<string>(ref ProcessedCardId));
			info.AddProperty(PropertyName._nCard, Variant.From<NCard>(ref _nCard));
			info.AddProperty(PropertyName._holder, Variant.From<NCardHolder>(ref _holder));
			info.AddProperty(PropertyName.OriginalBodyNames, Variant.From<string[]>(ref OriginalBodyNames));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).RestoreGodotObjectData(info);
			Variant val = default(Variant);
			if (info.TryGetProperty(PropertyName.ProcessedCardId, ref val))
			{
				ProcessedCardId = ((Variant)(ref val)).As<string>();
			}
			Variant val2 = default(Variant);
			if (info.TryGetProperty(PropertyName._nCard, ref val2))
			{
				_nCard = ((Variant)(ref val2)).As<NCard>();
			}
			Variant val3 = default(Variant);
			if (info.TryGetProperty(PropertyName._holder, ref val3))
			{
				_holder = ((Variant)(ref val3)).As<NCardHolder>();
			}
			Variant val4 = default(Variant);
			if (info.TryGetProperty(PropertyName.OriginalBodyNames, ref val4))
			{
				OriginalBodyNames = ((Variant)(ref val4)).As<string[]>();
			}
		}
	}

	public static readonly StringName XRotKey = StringName.op_Implicit("x_rot");

	public static readonly StringName YRotKey = StringName.op_Implicit("y_rot");

	public static readonly StringName EffectModeKey = StringName.op_Implicit("effect_mode");

	public static readonly StringName IntensityKey = StringName.op_Implicit("intensity");

	public static readonly StringName SeedKey = StringName.op_Implicit("seed");

	public static readonly StringName UvOffsetKey = StringName.op_Implicit("uv_offset");

	public static readonly StringName UvScaleKey = StringName.op_Implicit("uv_scale");

	public static readonly StringName FoilAltUvOffsetKey = StringName.op_Implicit("foil_alt_uv_offset");

	public static readonly StringName NoiseTexKey = StringName.op_Implicit("noise_tex");

	public const string TcName = "BalatroTiltContainer";

	public const string TiltRootName = "BalatroTiltRoot";

	public static readonly Vector2 UvRefSize = new Vector2(480f, 480f);

	public static readonly Vector2 UvRefHalf = new Vector2(240f, 240f);

	public static readonly HashSet<string> SkipNames = new HashSet<string> { "CardVfxContainer", "RareGlow", "UncommonGlow" };

	public static readonly HashSet<string> CardTemplateNames = new HashSet<string>
	{
		"Shadow", "Highlight", "PortraitCanvasGroup", "AncientBorderGlassOverlay", "AncientBorder", "AncientTextBg", "Lock", "Frame", "DescriptionLabel", "PortraitBorder",
		"OverlayContainer", "TitleBanner", "AncientBanner", "TitleLabel", "TypePlaque", "EnergyIcon", "StarIcon", "Enchantment", "EnchantmentVfxOverride"
	};

	public static readonly Lazy<Shader?> TiltShader = new Lazy<Shader>((Func<Shader>)delegate
	{
		try
		{
			return GD.Load<Shader>("res://balatroeffect/Shaders/balatro_effects.gdshader");
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] Tilt shader: " + ex.Message);
			return (Shader?)null;
		}
	});

	private static readonly HashSet<string> VfxKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "vfx", "effect", "particle", "anim", "trail", "glow", "sparkle" };

	public static readonly HashSet<string> ThirdPartyContainerNames = new HashSet<string> { "EnhanceTabContainer" };

	private static bool ShouldIncludeExternalNode(Node node)
	{
		if (!(node is Control))
		{
			return false;
		}
		string text = ((object)node.Name).ToString();
		if (ThirdPartyContainerNames.Contains(text))
		{
			return false;
		}
		foreach (string vfxKeyword in VfxKeywords)
		{
			if (text.Contains(vfxKeyword, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}
		return true;
	}

	public static TiltContainer? CreateTilt(NCard card, string cid, bool fullCard = false)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Expected O, but got Unknown
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		if (card == null || !GodotObject.IsInstanceValid((GodotObject)(object)card))
		{
			return null;
		}
		if (card.Model == null || card.Body == null)
		{
			return null;
		}
		ShaderMaterial val = new ShaderMaterial
		{
			Shader = TiltShader.Value
		};
		if (val.Shader == null)
		{
			return null;
		}
		val.SetShaderParameter(SeedKey, Variant.op_Implicit((float)(((object)card).GetHashCode() % 10000) / 10f));
		val.SetShaderParameter(EffectModeKey, Variant.op_Implicit(0));
		TiltContainer tiltContainer = new TiltContainer();
		((Node)tiltContainer).Name = StringName.op_Implicit("BalatroTiltContainer");
		((CanvasItem)tiltContainer).Material = (Material)(object)val;
		((Control)tiltContainer).Size = Vector2I.op_Implicit(new Vector2I(1200, 1200));
		((SubViewportContainer)tiltContainer).Stretch = true;
		((Control)tiltContainer).MouseFilter = (MouseFilterEnum)2;
		((Control)tiltContainer).Position = new Vector2(-600f, -600f);
		((Control)tiltContainer).PivotOffset = new Vector2(600f, 600f);
		tiltContainer.ProcessedCardId = cid;
		tiltContainer._nCard = card;
		TiltContainer tiltContainer2 = tiltContainer;
		SubViewport val2 = new SubViewport
		{
			Name = StringName.op_Implicit("SubViewport"),
			TransparentBg = true,
			Disable3D = true,
			Size = new Vector2I(1200, 1200)
		};
		Control val3 = new Control
		{
			Name = StringName.op_Implicit("BalatroTiltRoot"),
			Position = new Vector2(600f, 600f)
		};
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)tiltContainer2).AddChild((Node)(object)val2, false, (InternalMode)0);
		List<string> list = new List<string>();
		for (int i = 0; i < ((Node)card.Body).GetChildCount(false); i++)
		{
			string text = StringName.op_Implicit(((Node)card.Body).GetChild(i, false).Name);
			if (text.StartsWith("BalatroShaderPart_"))
			{
				text = text.Substring("BalatroShaderPart_".Length);
			}
			list.Add(text);
		}
		tiltContainer2.OriginalBodyNames = list.ToArray();
		Node[] array = (Node[])(object)new Node[((Node)card.Body).GetChildCount(false)];
		for (int j = 0; j < ((Node)card.Body).GetChildCount(false); j++)
		{
			array[j] = ((Node)card.Body).GetChild(j, false);
		}
		Node[] array2 = array;
		foreach (Node val4 in array2)
		{
			if (!SkipNames.Contains(StringName.op_Implicit(val4.Name)))
			{
				bool num = CardTemplateNames.Contains(StringName.op_Implicit(val4.Name));
				bool flag = !num;
				if (num || (flag && ShouldIncludeExternalNode(val4)))
				{
					((Node)card.Body).RemoveChild(val4);
					((Node)val3).AddChild(val4, false, (InternalMode)0);
				}
			}
		}
		Control nodeOrNull = ((Node)card).GetNodeOrNull<Control>(NodePath.op_Implicit("_card_overlay_"));
		if (nodeOrNull != null && ((Node)nodeOrNull).GetParent() == card)
		{
			nodeOrNull.Position += card.Body.Position;
			((Node)card).RemoveChild((Node)(object)nodeOrNull);
			((Node)val3).AddChild((Node)(object)nodeOrNull, false, (InternalMode)0);
		}
		return tiltContainer2;
	}

	public static void DestroyTilt(TiltContainer tilt, Control body)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		SubViewport node = ((Node)tilt).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"));
		Node parent = ((Node)tilt).GetParent();
		if (parent != null)
		{
			parent.RemoveChild((Node)(object)tilt);
		}
		((Node)tilt).QueueFree();
		Control val = (Control)((Node)node).GetChild(0, false);
		PartWrapper.UnwrapParts(val);
		Control nodeOrNull = ((Node)val).GetNodeOrNull<Control>(NodePath.op_Implicit("_card_overlay_"));
		if (nodeOrNull != null)
		{
			nodeOrNull.Position -= body.Position;
			((Node)val).RemoveChild((Node)(object)nodeOrNull);
			NCard nCard = tilt._nCard;
			if (nCard != null && GodotObject.IsInstanceValid((GodotObject)(object)nCard))
			{
				((Node)nCard).AddChild((Node)(object)nodeOrNull, false, (InternalMode)0);
			}
			else
			{
				((Node)body).AddChild((Node)(object)nodeOrNull, false, (InternalMode)0);
			}
		}
		Node[] array = (Node[])(object)new Node[((Node)val).GetChildCount(false)];
		for (int i = 0; i < ((Node)val).GetChildCount(false); i++)
		{
			array[i] = ((Node)val).GetChild(i, false);
		}
		Node[] array2 = array;
		foreach (Node val2 in array2)
		{
			((Node)val).RemoveChild(val2);
			((Node)body).AddChild(val2, false, (InternalMode)0);
		}
		if (tilt.OriginalBodyNames == null || tilt.OriginalBodyNames.Length == 0)
		{
			return;
		}
		List<Node> list = new List<Node>();
		foreach (Node child in ((Node)body).GetChildren(false))
		{
			list.Add(child);
		}
		List<Node> list2 = new List<Node>();
		string[] originalBodyNames = tilt.OriginalBodyNames;
		foreach (string logicalName in originalBodyNames)
		{
			Node val3 = ((IEnumerable<Node>)list).FirstOrDefault((Func<Node, bool>)((Node n) => n.Name == StringName.op_Implicit(logicalName) || ((object)n.Name).ToString().StartsWith("BalatroShaderPart_" + logicalName)));
			if (val3 != null)
			{
				list2.Add(val3);
				list.Remove(val3);
			}
		}
		list2.AddRange(list);
		for (int k = 0; k < list2.Count; k++)
		{
			Node val4 = list2[k];
			if (val4.GetParent() == body)
			{
				((Node)body).RemoveChild(val4);
			}
			((Node)body).AddChild(val4, false, (InternalMode)0);
			((Node)body).MoveChild(val4, k);
		}
	}
}
