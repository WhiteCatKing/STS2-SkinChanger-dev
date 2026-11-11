using System;
using System.Collections.Generic;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace CznCore.Presentation;

internal sealed class RestSitePresentationRuntime(RestSitePresentationProfile profile, IRestSitePresentationRuntimeContext context)
{
	private readonly CharacterSkinProfile? _appearanceProfile;

	public RestSitePresentationRuntime(RestSitePresentationProfile profile, IRestSitePresentationRuntimeContext context, CharacterSkinProfile appearanceProfile)
		: this(profile, context)
	{
		_appearanceProfile = appearanceProfile;
	}

	public RestSitePresentationResult Apply(NRestSiteCharacter restSiteCharacter)
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		if (_appearanceProfile != null)
		{
			CharacterSkinProfile characterSkinProfile = _appearanceProfile.For(restSiteCharacter.Player);
			if ((object)characterSkinProfile.RestSite != profile)
			{
				foreach (Node item in context.FindSpineSprites((Node)(object)restSiteCharacter))
				{
					Node2D val = (Node2D)(object)((item is Node2D) ? item : null);
					if (val != null)
					{
						BodyAppearanceResources.Apply(_appearanceProfile, restSiteCharacter.Player, val);
					}
				}
				return new RestSitePresentationRuntime(characterSkinProfile.RestSite, context).Apply(restSiteCharacter);
			}
		}
		ApplySurface((Node)(object)restSiteCharacter);
		bool applied = false;
		Node2D val2 = null;
		string text = string.Empty;
		foreach (Node item2 in context.FindSpineSprites((Node)(object)restSiteCharacter))
		{
			Node2D val3 = (Node2D)(object)((item2 is Node2D) ? item2 : null);
			if (val3 == null || ((GodotObject)item2).GetClass() != "SpineSprite")
			{
				continue;
			}
			BodyAppearanceResources.Apply(_appearanceProfile, restSiteCharacter.Player, val3);
			MegaSprite val4 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val3));
			string text2 = ResolveAnimation(val4);
			if (string.IsNullOrEmpty(text2))
			{
				continue;
			}
			MegaAnimationState animationState = val4.GetAnimationState();
			if (animationState == null)
			{
				continue;
			}
			MegaTrackEntry val5 = SpineAnimationCompat.SetAnimationTracked(animationState, text2);
			try
			{
				if (val5 != null)
				{
					val5.SetTrackTime(val5.GetAnimationEnd() * GD.Randf());
				}
			}
			finally
			{
				SpineAnimationCompat.ReleaseTracked(val5);
			}
			if (string.Equals(text2, profile.PrimaryAnimationName, StringComparison.Ordinal))
			{
				ApplyBodyOffset(val3);
				ApplyHiddenSlots(val4);
				ApplyHiddenSlotsDeferred((Node)(object)restSiteCharacter, val4);
				if (val2 == null)
				{
					val2 = val3;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = text2;
			}
			applied = true;
		}
		bool hasCampingEffects = val2 != null && AttachCampingEffects(val2);
		return new RestSitePresentationResult(applied, text, hasCampingEffects);
	}

	private void ApplySurface(Node root)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		RestSiteSurfaceProfile restSiteSurfaceProfile = profile.Surface ?? CharacterSkinSurfaceDefaults.CznRestSiteSurface;
		if (!restSiteSurfaceProfile.ApplySceneLayout)
		{
			return;
		}
		foreach (Node item in context.FindSpineSprites(root))
		{
			Node2D val = (Node2D)(object)((item is Node2D) ? item : null);
			if (val != null && !(((GodotObject)item).GetClass() != "SpineSprite"))
			{
				val.Position = restSiteSurfaceProfile.SpinePosition;
				val.Scale = restSiteSurfaceProfile.SpineScale;
			}
		}
		ApplyControlRect(root, "ControlRoot", restSiteSurfaceProfile.ControlRoot);
		ApplyControlRect(root, "ControlRoot/SelectionReticle", restSiteSurfaceProfile.SelectionReticle);
		ApplyControlRect(root, "ControlRoot/Hitbox", restSiteSurfaceProfile.Hitbox);
		ApplyControlRect(root, "ControlRoot/ThoughtBubbleRight", restSiteSurfaceProfile.ThoughtBubbleRight);
		ApplyControlRect(root, "ControlRoot/ThoughtBubbleLeft", restSiteSurfaceProfile.ThoughtBubbleLeft);
	}

	private static void ApplyControlRect(Node root, string path, RestSiteControlRectProfile? rect)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (!(rect == null))
		{
			Control nodeOrNull = root.GetNodeOrNull<Control>(NodePath.op_Implicit(path));
			if (nodeOrNull != null)
			{
				StringName obj = StringName.op_Implicit("offset_left");
				float left = rect.Left;
				((GodotObject)nodeOrNull).Set(obj, Variant.From<float>(ref left));
				StringName obj2 = StringName.op_Implicit("offset_top");
				left = rect.Top;
				((GodotObject)nodeOrNull).Set(obj2, Variant.From<float>(ref left));
				StringName obj3 = StringName.op_Implicit("offset_right");
				left = rect.Right;
				((GodotObject)nodeOrNull).Set(obj3, Variant.From<float>(ref left));
				StringName obj4 = StringName.op_Implicit("offset_bottom");
				left = rect.Bottom;
				((GodotObject)nodeOrNull).Set(obj4, Variant.From<float>(ref left));
			}
		}
	}

	public void Normalize(Node root)
	{
		foreach (Node item in FindCampingEffectNodes(root))
		{
			Node2D val = (Node2D)(object)((item is Node2D) ? item : null);
			if (val != null)
			{
				string nodeName = ((object)((Node)val).Name).ToString();
				if (TryGetCampingLayerProfile(nodeName, out RestSiteCampingLayerProfile layerProfile))
				{
					bool behindParent = layerProfile.BehindParent;
					Node parent = ((Node)val).GetParent();
					NormalizeCampingLayer(val, behindParent, (CanvasItem?)(object)((parent is CanvasItem) ? parent : null));
				}
			}
		}
	}

	private string ResolveAnimation(MegaSprite sprite)
	{
		if (sprite.HasAnimation(profile.PrimaryAnimationName))
		{
			return profile.PrimaryAnimationName;
		}
		foreach (string fallbackAnimationName in profile.FallbackAnimationNames)
		{
			if (sprite.HasAnimation(fallbackAnimationName))
			{
				return fallbackAnimationName;
			}
		}
		return string.Empty;
	}

	private void ApplyBodyOffset(Node2D bodyNode)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Vector2 bodyOffset = profile.BodyOffset;
		if (!(bodyOffset == Vector2.Zero))
		{
			Vector2 val2;
			if (((GodotObject)bodyNode).HasMeta(StringName.op_Implicit(profile.BodyBasePositionMeta)))
			{
				StringName obj = StringName.op_Implicit(profile.BodyBasePositionMeta);
				Variant val = default(Variant);
				val = ((GodotObject)bodyNode).GetMeta(obj, val);
				val2 = ((Variant)(ref val)).AsVector2();
			}
			else
			{
				val2 = bodyNode.Position;
				((GodotObject)bodyNode).SetMeta(StringName.op_Implicit(profile.BodyBasePositionMeta), Variant.From<Vector2>(ref val2));
			}
			bodyNode.Position = val2 + bodyOffset;
		}
	}

	private void ApplyHiddenSlots(MegaSprite sprite)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if (profile.HiddenSlotNames == null || profile.HiddenSlotNames.Count == 0)
		{
			return;
		}
		MegaSkeleton skeleton = sprite.GetSkeleton();
		if (skeleton == null)
		{
			return;
		}
		GodotObject boundObject = ((MegaSpineBinding)skeleton).BoundObject;
		foreach (string hiddenSlotName in profile.HiddenSlotNames)
		{
			string current = hiddenSlotName;
			if (string.IsNullOrWhiteSpace(current))
			{
				continue;
			}
			try
			{
				Variant val = boundObject.Call(StringName.op_Implicit("find_slot"), (Variant[])(object)new Variant[1] { Variant.From<string>(ref current) });
				GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
				if (val2 == null)
				{
					context.LogWarn("Rest-site hidden slot not found: " + current);
					continue;
				}
				Color val3;
				if (!val2.HasMethod(StringName.op_Implicit("get_color")))
				{
					val3 = Colors.White;
				}
				else
				{
					Variant val4 = val2.Call(StringName.op_Implicit("get_color"), Array.Empty<Variant>());
					val3 = ((Variant)(ref val4)).AsColor();
				}
				Color val5 = val3;
				val5.A = 0f;
				val2.Call(StringName.op_Implicit("set_color"), (Variant[])(object)new Variant[1] { Variant.From<Color>(ref val5) });
			}
			catch (Exception ex)
			{
				context.LogWarn("Rest-site hidden slot apply failed: " + current + " " + SkinLog.FormatException(ex));
			}
		}
	}

	private void ApplyHiddenSlotsDeferred(Node host, MegaSprite sprite)
	{
		if (profile.HiddenSlotNames == null || profile.HiddenSlotNames.Count == 0)
		{
			return;
		}
		GodotScheduler.Deferred(delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)host) && GodotObject.IsInstanceValid(((MegaSpineBinding)sprite).BoundObject))
			{
				ApplyHiddenSlots(sprite);
			}
		});
	}

	private bool AttachCampingEffects(Node2D bodyNode)
	{
		if (profile.CampingLayers.Count == 0)
		{
			return false;
		}
		bool flag = false;
		foreach (RestSiteCampingLayerProfile campingLayer in profile.CampingLayers)
		{
			if (((Node)bodyNode).GetNodeOrNull<Node2D>(NodePath.op_Implicit(campingLayer.NodeName)) != null)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Normalize((Node)(object)bodyNode);
			return true;
		}
		bool result = false;
		foreach (RestSiteCampingLayerProfile campingLayer2 in profile.CampingLayers)
		{
			Node? obj = CreateCampingLayer(campingLayer2);
			Node2D val = (Node2D)(object)((obj is Node2D) ? obj : null);
			if (val != null)
			{
				((Node)bodyNode).AddChild((Node)(object)val, false, (InternalMode)0);
				NormalizeCampingLayer(val, campingLayer2.BehindParent, (CanvasItem?)(object)bodyNode);
				context.ApplyLayerMaterial((Node)(object)val);
				context.ApplyLayerMaterialDeferred((Node)(object)val, 4);
				context.StartLayerEffects((Node)(object)val, visible: true);
				result = true;
			}
		}
		return result;
	}

	private void NormalizeCampingLayer(Node2D node, bool behindParent, CanvasItem? bodyItem)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		node.Position = Vector2.Zero;
		node.Scale = Vector2.One;
		node.Rotation = 0f;
		((CanvasItem)node).TopLevel = false;
		((CanvasItem)node).ZAsRelative = true;
		((CanvasItem)node).ZIndex = 0;
		((CanvasItem)node).ShowBehindParent = behindParent;
		if (bodyItem != null)
		{
			((CanvasItem)node).Visible = bodyItem.Visible;
			((CanvasItem)node).VisibilityLayer = bodyItem.VisibilityLayer;
			((CanvasItem)node).LightMask = bodyItem.LightMask;
		}
	}

	private List<Node> FindCampingEffectNodes(Node root)
	{
		List<Node> list = new List<Node>();
		Stack<Node> stack = new Stack<Node>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node val = stack.Pop();
			string nodeName = ((object)val.Name).ToString();
			if (TryGetCampingLayerProfile(nodeName, out RestSiteCampingLayerProfile _))
			{
				list.Add(val);
				continue;
			}
			foreach (Node child in val.GetChildren(false))
			{
				stack.Push(child);
			}
		}
		return list;
	}

	private bool TryGetCampingLayerProfile(string nodeName, out RestSiteCampingLayerProfile? layerProfile)
	{
		foreach (RestSiteCampingLayerProfile campingLayer in profile.CampingLayers)
		{
			if (string.Equals(nodeName, campingLayer.NodeName, StringComparison.Ordinal))
			{
				layerProfile = campingLayer;
				return true;
			}
		}
		layerProfile = null;
		return false;
	}

	private Node? CreateCampingLayer(RestSiteCampingLayerProfile layer)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Variant val = ClassDB.Instantiate(StringName.op_Implicit("SpineAtlasResource"));
			GodotObject val2 = ((Variant)(ref val)).AsGodotObject();
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonFileResource"));
			GodotObject val3 = ((Variant)(ref val)).AsGodotObject();
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineSkeletonDataResource"));
			GodotObject val4 = ((Variant)(ref val)).AsGodotObject();
			val = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
			GodotObject obj = ((Variant)(ref val)).AsGodotObject();
			Node val5 = (Node)(object)((obj is Node) ? obj : null);
			if (val2 == null || val3 == null || val4 == null || val5 == null)
			{
				return null;
			}
			Variant val6 = val2.Call(StringName.op_Implicit("load_from_atlas_file"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(layer.AtlasPath) });
			Variant val7 = val3.Call(StringName.op_Implicit("load_from_file"), (Variant[])(object)new Variant[1] { Variant.op_Implicit(layer.SkeletonPath) });
			if (((Variant)(ref val6)).AsInt32() != 0 || ((Variant)(ref val7)).AsInt32() != 0)
			{
				context.LogWarn("Rest-site camping layer load failed: " + layer.NodeName + " atlasErr=" + ((Variant)(ref val6)).AsInt32() + " skelErr=" + ((Variant)(ref val7)).AsInt32());
				return null;
			}
			val4.Set(StringName.op_Implicit("atlas_res"), Variant.From<GodotObject>(ref val2));
			val4.Set(StringName.op_Implicit("skeleton_file_res"), Variant.From<GodotObject>(ref val3));
			val5.Name = StringName.op_Implicit(layer.NodeName);
			((GodotObject)val5).Set(StringName.op_Implicit("skeleton_data_res"), Variant.From<GodotObject>(ref val4));
			return val5;
		}
		catch (Exception ex)
		{
			context.LogWarn("Rest-site camping layer instantiate failed: " + layer.NodeName + " " + SkinLog.FormatException(ex));
			return null;
		}
	}
}
