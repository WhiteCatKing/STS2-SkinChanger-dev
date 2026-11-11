using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class NativePlayedCardVisibilityRuntime<TCard> where TCard : class
{
	private static readonly bool EnableNativePlayedCardHiding = true;

	private readonly string _playContainerModulateMeta;

	private readonly Func<TCard?, bool> _isCardEligible;

	private readonly Action<string, int> _warn;

	public NativePlayedCardVisibilityRuntime(string metaKeyPrefix, INativePlayedCardVisibilityHost<TCard> host)
	{
		ArgumentNullException.ThrowIfNull(host, "host");
		_playContainerModulateMeta = (string.IsNullOrWhiteSpace(metaKeyPrefix) ? "character_skin" : metaKeyPrefix.Trim()) + "_native_play_container_modulate";
		_isCardEligible = host.IsCardEligible;
		_warn = host.Warn;
	}

	public void Hide(TCard? card)
	{
		if (EnableNativePlayedCardHiding && card != null && _isCardEligible(card))
		{
			HidePlayContainer();
		}
	}

	public void RestorePlayContainer()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Control val = FindPlayContainer();
			if (val != null && ((GodotObject)val).HasMeta(StringName.op_Implicit(_playContainerModulateMeta)))
			{
				StringName obj = StringName.op_Implicit(_playContainerModulateMeta);
				Variant val2 = default(Variant);
				val2 = ((GodotObject)val).GetMeta(obj, val2);
				((CanvasItem)val).Modulate = ((Variant)(ref val2)).AsColor();
				((GodotObject)val).RemoveMeta(StringName.op_Implicit(_playContainerModulateMeta));
			}
		}
		catch (Exception ex)
		{
			_warn("Native played card container restore failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	private void HidePlayContainer()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Control val = FindPlayContainer();
			if (val != null)
			{
				if (!((GodotObject)val).HasMeta(StringName.op_Implicit(_playContainerModulateMeta)))
				{
					StringName obj = StringName.op_Implicit(_playContainerModulateMeta);
					Color modulate = ((CanvasItem)val).Modulate;
					((GodotObject)val).SetMeta(obj, Variant.From<Color>(ref modulate));
				}
				((CanvasItem)val).Modulate = Colors.Transparent;
			}
		}
		catch (Exception ex)
		{
			_warn("Native played card container hide failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	private static Control? FindPlayContainer()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NCombatUi ui = instance.Ui;
			obj = ((ui != null) ? ui.PlayContainer : null);
		}
		Control val = (Control)obj;
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return null;
		}
		return val;
	}
}
