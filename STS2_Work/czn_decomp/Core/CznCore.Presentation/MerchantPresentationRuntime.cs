using System;
using System.Collections.Generic;
using CznCore.Godot;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace CznCore.Presentation;

internal sealed class MerchantPresentationRuntime
{
	private readonly CharacterSkinProfile? _appearanceProfile;

	private readonly Func<Player?, bool> _appliesTo;

	private readonly float _bodySpineScaleMultiplier;

	private readonly MerchantSurfaceProfile _surface;

	private readonly Func<Node, IReadOnlyList<Node>> _findSpineSprites;

	private readonly string _surfaceMeta;

	private readonly string _fakeMerchantSurfaceMeta;

	public MerchantPresentationRuntime(CharacterSkinProfile profile, MerchantSurfaceProfile surface, Func<Node, IReadOnlyList<Node>> findSpineSprites)
		: this(profile.RuntimeMetaKeyPrefix, (Player? player) => profile.AppliesTo(player), profile.BodySpineScaleMultiplier, surface, findSpineSprites)
	{
		_appearanceProfile = profile;
	}

	public MerchantPresentationRuntime(string runtimeMetaKeyPrefix, Func<Player?, bool> appliesTo, float bodySpineScaleMultiplier, MerchantSurfaceProfile surface, Func<Node, IReadOnlyList<Node>> findSpineSprites)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(runtimeMetaKeyPrefix, "runtimeMetaKeyPrefix");
		_appliesTo = appliesTo ?? throw new ArgumentNullException("appliesTo");
		_bodySpineScaleMultiplier = bodySpineScaleMultiplier;
		_surface = surface ?? throw new ArgumentNullException("surface");
		_findSpineSprites = findSpineSprites ?? throw new ArgumentNullException("findSpineSprites");
		_surfaceMeta = runtimeMetaKeyPrefix + "_merchant_surface_applied";
		_fakeMerchantSurfaceMeta = runtimeMetaKeyPrefix + "_fake_merchant_surface_applied";
	}

	public MerchantPresentationResult Apply(NMerchantCharacter merchantCharacter)
	{
		Player val = ResolveMerchantCharacterPlayer(merchantCharacter);
		if (val != null && !_appliesTo(val))
		{
			return new MerchantPresentationResult(Applied: false, StartedLoop: false, string.Empty, HasPlayerMapping: true);
		}
		foreach (Node item in _findSpineSprites((Node)(object)merchantCharacter))
		{
			Node2D val2 = (Node2D)(object)((item is Node2D) ? item : null);
			if (val2 != null && !(((GodotObject)item).GetClass() != "SpineSprite"))
			{
				BodyAppearanceResources.Apply(_appearanceProfile, val, val2);
				ApplySurface(val2);
				PlayLoopWhenReady((Node)(object)merchantCharacter, (Node)(object)val2);
				return new MerchantPresentationResult(Applied: true, StartedLoop: true, _surface.LoopAnimationName, val != null);
			}
		}
		return new MerchantPresentationResult(Applied: false, StartedLoop: false, string.Empty, val != null);
	}

	public FakeMerchantPresentationResult Apply(NFakeMerchant fakeMerchant)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		List<Player> value = Traverse.Create((object)fakeMerchant).Field("_players").GetValue<List<Player>>();
		if (value == null || value.Count == 0)
		{
			return new FakeMerchantPresentationResult(0, _surface.LoopAnimationName, Vector2.Zero);
		}
		Control value2 = Traverse.Create((object)fakeMerchant).Field("_characterContainer").GetValue<Control>();
		if (value2 == null)
		{
			return new FakeMerchantPresentationResult(0, _surface.LoopAnimationName, Vector2.Zero);
		}
		List<NCreatureVisuals> list = new List<NCreatureVisuals>();
		foreach (Node child in ((Node)value2).GetChildren(false))
		{
			NCreatureVisuals val = (NCreatureVisuals)(object)((child is NCreatureVisuals) ? child : null);
			if (val != null)
			{
				list.Add(val);
			}
		}
		int num = 0;
		int num2 = Math.Min(value.Count, list.Count);
		for (int i = 0; i < num2; i++)
		{
			Player val2 = value[num2 - i - 1];
			if (_appliesTo(val2) && ApplyFakeMerchantVisualSurface(list[i], val2))
			{
				num++;
			}
		}
		return new FakeMerchantPresentationResult(num, _surface.LoopAnimationName, CharacterSkinSurfaceDefaults.ResolveCznCombatBodySpineScale(Vector2.One, _bodySpineScaleMultiplier));
	}

	private void ApplySurface(Node2D spineNode)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)spineNode).HasMeta(StringName.op_Implicit(_surfaceMeta)))
		{
			spineNode.Scale = _surface.SpineScale;
			StringName obj = StringName.op_Implicit(_surfaceMeta);
			bool flag = true;
			((GodotObject)spineNode).SetMeta(obj, Variant.From<bool>(ref flag));
		}
	}

	private bool ApplyFakeMerchantVisualSurface(NCreatureVisuals visuals, Player? player)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		Node2D currentBody = visuals.GetCurrentBody();
		if (((GodotObject)currentBody).GetClass() != "SpineSprite")
		{
			return false;
		}
		GodotObject val = (GodotObject)(object)currentBody;
		BodyAppearanceResources.Apply(_appearanceProfile, player, currentBody);
		if (val.HasMeta(StringName.op_Implicit(_fakeMerchantSurfaceMeta)))
		{
			return false;
		}
		Vector2 scale = currentBody.Scale;
		currentBody.Scale = CharacterSkinSurfaceDefaults.ResolveCznCombatBodySpineScale(scale, _bodySpineScaleMultiplier);
		PlayLoopWhenReady((Node)(object)visuals, (Node)(object)currentBody);
		StringName obj = StringName.op_Implicit(_fakeMerchantSurfaceMeta);
		bool flag = true;
		val.SetMeta(obj, Variant.From<bool>(ref flag));
		return true;
	}

	private void PlayLoopWhenReady(Node host, Node spineNode)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)spineNode));
		SpineNodeExtensions.RunWhenSpineReady(host, sprite, (Action<MegaAnimationState>)delegate
		{
			TryPlayLoop(sprite, "Merchant loop animation failed: ");
			GodotScheduler.Deferred(delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)host) && GodotObject.IsInstanceValid(((MegaSpineBinding)sprite).BoundObject))
				{
					TryPlayLoop(sprite, "Merchant deferred loop animation failed: ");
				}
			});
		});
	}

	private void TryPlayLoop(MegaSprite sprite, string warningPrefix)
	{
		try
		{
			PlayLoop(sprite);
		}
		catch (Exception ex)
		{
			SkinLog.Warn(warningPrefix + _surface.LoopAnimationName + " " + SkinLog.FormatException(ex));
		}
	}

	private void PlayLoop(MegaSprite sprite)
	{
		if (!sprite.HasAnimation(_surface.LoopAnimationName))
		{
			throw new InvalidOperationException("Merchant SpineSprite missing animation: " + _surface.LoopAnimationName);
		}
		SpineAnimationCompat.SetAnimation(sprite.GetAnimationState(), _surface.LoopAnimationName);
	}

	private Player? ResolveMerchantCharacterPlayer(NMerchantCharacter merchantCharacter)
	{
		NMerchantRoom instance = NMerchantRoom.Instance;
		if (instance == null)
		{
			return null;
		}
		List<Player> value = Traverse.Create((object)instance).Field("_players").GetValue<List<Player>>();
		if (value == null)
		{
			return null;
		}
		int index = ((Node)merchantCharacter).GetIndex(false);
		if (index < 0 || index >= value.Count)
		{
			return null;
		}
		return value[index];
	}
}
