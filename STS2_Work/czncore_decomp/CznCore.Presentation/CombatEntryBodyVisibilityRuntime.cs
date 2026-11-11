using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class CombatEntryBodyVisibilityRuntime
{
	private readonly CharacterSkinProfile _profile;

	private readonly CombatCreatureSpineResolver _spines;

	private readonly Func<Player?, bool> _shouldHidePlayer;

	private readonly Func<bool>? _isEnabled;

	private readonly Action<string>? _warn;

	private readonly Action<string>? _debug;

	private readonly float _fallbackSeconds;

	private readonly string _hiddenMeta;

	public CombatEntryBodyVisibilityRuntime(CharacterSkinProfile profile, CombatCreatureSpineResolver spines, Func<Player?, bool> shouldHidePlayer, Func<bool>? isEnabled = null, Action<string>? warn = null, Action<string>? debug = null, float fallbackSeconds = 2f)
	{
		_profile = profile;
		_spines = spines;
		_shouldHidePlayer = shouldHidePlayer;
		_isEnabled = isEnabled;
		_warn = warn;
		_debug = debug;
		_fallbackSeconds = fallbackSeconds;
		_hiddenMeta = profile.RuntimeMetaKey("combat_entry_body_hidden_until_start");
	}

	public void HideUntilEntryStart(NCreature? creatureNode)
	{
		try
		{
			if (!ShouldHideUntilEntryStart(creatureNode) || ((GodotObject)creatureNode).HasMeta(StringName.op_Implicit(_hiddenMeta)))
			{
				return;
			}
			CanvasItem val = (CanvasItem)(object)_spines.Find(creatureNode);
			if (val != null)
			{
				CombatEntryVisibility.Hide((Node)(object)creatureNode, val, _hiddenMeta, delegate
				{
					Reveal(creatureNode, "fallback");
				}, _fallbackSeconds);
				_debug?.Invoke("combat entry body hidden until entry_start");
			}
		}
		catch (Exception ex)
		{
			_warn?.Invoke("combat entry pre-hide failed: " + SkinLog.FormatException(ex));
		}
	}

	public void Reveal(NCreature? creatureNode, string reason)
	{
		try
		{
			if (creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && ((GodotObject)creatureNode).HasMeta(StringName.op_Implicit(_hiddenMeta)))
			{
				Node2D val = _spines.Find(creatureNode);
				CanvasItem val2 = (CanvasItem)(object)val;
				if (val2 != null)
				{
					CombatEntryVisibility.Reveal((Node)(object)creatureNode, val2, val, _hiddenMeta);
				}
				_debug?.Invoke("combat entry body revealed reason=" + reason);
			}
		}
		catch (Exception ex)
		{
			_warn?.Invoke("combat entry reveal failed: " + SkinLog.FormatException(ex));
		}
	}

	private bool ShouldHideUntilEntryStart(NCreature? creatureNode)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		NCombatRoom instance = NCombatRoom.Instance;
		if ((_isEnabled != null && !_isEnabled()) || creatureNode == null || !GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) || instance == null || (int)instance.Mode != 0 || _profile.CombatBody.EntrySequence.Length == 0 || !CombatBodyEntryPlaybackRuntime.HasRunIn(_profile.CombatBody))
		{
			return false;
		}
		Creature entity = creatureNode.Entity;
		Player val = ((entity != null) ? entity.Player : null);
		if (_profile.AppliesTo(val))
		{
			return _shouldHidePlayer(val);
		}
		return false;
	}
}
